namespace MailSenderAPI.Services;

using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using global::MailSenderAPI.Context;
using MailSenderAPI.Models;
using MailSenderAPI.Models.JsonAPIResposes.MailSenderAPI.Utils.Responses;
using MailSenderAPI.Utils.Exceptions;
using Org.BouncyCastle.Asn1.X509;
using System.Text.RegularExpressions;
using Azure;
using MailSenderAPI.Models.NewFolder;
using System;

public class EmailService
{
    private readonly IConfiguration _configuration;
    private readonly ApplicationDbContext _context;
    private readonly EncryptionService _encryptionService;
    private readonly EmailExtensionService _extensionService;
    private readonly ErrorService _errorService;

    public EmailService(IConfiguration configuration, ApplicationDbContext context, EncryptionService encryptionService, EmailExtensionService extensionService, ErrorService errorService)
    {
        _context = context;
        _configuration = configuration;
        _encryptionService = encryptionService;
        _extensionService = extensionService;
        _errorService = errorService;
    }

    public async Task<JSendResponse<string>> SendEmailAsync(string to, string subject, string body)
    {
        try
        {

            List<string> errors = await ValidateEmailTo(to);
            if(errors.Count > 0) 
                throw _errorService.GetApiException(ErrorCodes.BadRequest, [.. errors]);

            // Eliminar ';' al final si está presente
            to = to.TrimEnd(';');

            // Obtener la configuración SMTP desde la base de datos
            var smtpSettings = await _context.SmtpSettings.OrderBy(s => s.Id).LastOrDefaultAsync(); //trae siempre el registro mas bajo.

            if (smtpSettings == null)
            {
                throw _errorService.GetApiException(ErrorCodes.NotFound, "SMTP no configurado.");
            }

            // Crear el mensaje de correo
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress("EmailSender API", smtpSettings.FromEmail));

            // Dividir la lista de destinatarios por ';' y agregarlos al mensaje
            var recipients = to.Split(';');
            foreach (var recipient in recipients)
            {
                if (!string.IsNullOrEmpty(recipient))  // Validar que la dirección no esté vacía
                {
                    emailMessage.To.Add(new MailboxAddress("", recipient.Trim()));
                }
            }

            // Validar extensiones de los destinatarios
            foreach (var recipient in recipients)
            {
                if (!await IsEmailExtensionAllowedAsync(recipient))
                    throw _errorService.GetApiException(ErrorCodes.NotFound, $"Extensión '{recipient}' no disponible para enviar correos.");
            }

            // Configurar el asunto y el cuerpo del mensaje
            emailMessage.Subject = subject;
            string decryptedPassword = _encryptionService.Decrypt(smtpSettings.Password);
            var bodyBuilder = new BodyBuilder { HtmlBody = body };
            emailMessage.Body = bodyBuilder.ToMessageBody();

            // Conectar y enviar el correo
            using var client = new SmtpClient();
            await client.ConnectAsync(smtpSettings.Host, smtpSettings.Port, smtpSettings.UseSSL);
            await client.AuthenticateAsync(smtpSettings.Username, decryptedPassword);
            await client.SendAsync(emailMessage);
            await client.DisconnectAsync(true);
            return new JSendResponse<string> { Status = ResponseStatus.SUCCESS, Code = 200, Message = "Correo Enviado.", Data = "Correo enviado con éxito" };

        }
        catch (MailKit.Security.AuthenticationException authEx)
        {
            throw _errorService.GetApiException(ErrorCodes.AuthenticationFailed, "Verificar configuración del SMTP");
        }

    }

    private async Task<List<string>> ValidateEmailTo(string to)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(to))
        {
            errors.Add("El campo 'to' no puede estar vacío.");
        }

        to = to.Trim().TrimEnd(';');
        var recipients = to.Split(';', StringSplitOptions.RemoveEmptyEntries)
                           .Select(email => email.Trim())
                           .ToList();

        if (recipients.Count == 0)
        {
            errors.Add("No se encontraron direcciones de correo válidas.");
        }

        var emailSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var email in recipients)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                errors.Add("Se encontró una dirección vacía.");
            }

            if (!StrictEmailRegex.IsMatch(email))
            {
                errors.Add($"La dirección '{email}' tiene un formato inválido. Verifique los caracteres y la estructura.");
            }

            if (!emailSet.Add(email))
            {
                errors.Add($"La dirección '{email}' está duplicada.");
            }

            if (!await IsEmailExtensionAllowedAsync(email))
            {
                errors.Add($"La extensión de correo de '{email}' no está permitida.");
            }
        }

        return errors;
    }



    private static readonly Regex StrictEmailRegex = new Regex(
    @"^(?!.*\.\.)(?!\.)([a-zA-Z0-9]+[\w\.-]*[a-zA-Z0-9]+)@([a-zA-Z0-9-]+\.)+[a-zA-Z]{2,}$",
    RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private async Task<bool> IsEmailExtensionAllowedAsync(string email)
    {
        var emailParts = email.Split('@');
        if (emailParts.Length != 2)
            return false;

        var extension = emailParts[1]; // Obtener la parte después del '@'

        var extensionEntity = await _extensionService.GetAllowedByExtensionAsync(extension);
        return true;
    }

  


}