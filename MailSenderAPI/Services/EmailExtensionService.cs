using Azure;
using MailSenderAPI.Context;
using MailSenderAPI.Models;
using MailSenderAPI.Models.DTO;
using MailSenderAPI.Models.JsonAPIResposes.MailSenderAPI.Utils.Responses;
using MailSenderAPI.Models.NewFolder;
using MailSenderAPI.Utils.Exceptions;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MailSenderAPI.Services
{
    public class EmailExtensionService
    {

        private readonly ApplicationDbContext _context;
        private readonly ErrorService _errorService;

        public EmailExtensionService(ApplicationDbContext context, ErrorService errorService)
        {
            _errorService = errorService;
            _context = context;
        }

        public async Task<JSendResponse<ExtensionRsDTO>> AddExtensionAsync(string extension)
        {

            string[] validationErrors = await ValidateErrorsSmtpSettings(extension);


            if (validationErrors.Length > 0)
                throw _errorService.GetApiException(ErrorCodes.BadRequest, validationErrors);

            var emailExtension = new EmailExtension { Extension = extension, Status = true };
            _context.EmailExtensions.Add(emailExtension);
            await _context.SaveChangesAsync();

            var response = new ExtensionRsDTO { ExtensionName = emailExtension.Extension, Status = emailExtension.Status };
            return new JSendResponse<ExtensionRsDTO> { Status = ResponseStatus.SUCCESS, Code = 201, Message = "Extensión registrada con éxito.", Data = response };

        }

        public async Task<JSendResponse<List<ExtensionRsDTO>>> GetAllExtensionsAsync()
        {
           
            var ExtensionList = await _context.EmailExtensions.ToListAsync();
            if (ExtensionList.Count == 0)
                throw _errorService.GetApiException(ErrorCodes.NotFound,"No hay extensiones registradas");
            List < ExtensionRsDTO > extensionRsDTOs = new List<ExtensionRsDTO>();
            foreach (var item in ExtensionList)
            {
                ExtensionRsDTO extensionRsDTO = new ExtensionRsDTO { ExtensionName = item.Extension, Status = item.Status };
                extensionRsDTOs.Add(extensionRsDTO);
            }


            return new JSendResponse<List<ExtensionRsDTO>> { Status = ResponseStatus.SUCCESS, Code = 200, Message = "Extensión encontrada.", Data = extensionRsDTOs };

        }

        public async Task<JSendResponse<ExtensionRsDTO>> GetByExtensionAsync(string extension)
        {

            var emailExtension  = await _context.EmailExtensions.FirstOrDefaultAsync(e => e.Extension == extension);
            if (emailExtension == null)
                throw _errorService.GetApiException(ErrorCodes.NotFound, $"La extensión '{extension}' no está registrada.");
            var response = new ExtensionRsDTO { ExtensionName = emailExtension.Extension, Status = emailExtension.Status };
            return new JSendResponse<ExtensionRsDTO> { Status = ResponseStatus.SUCCESS, Code = 200, Message = "Extensión encontrada.", Data = response };

        }

        public async Task<JSendResponse<ExtensionRsDTO>> GetAllowedByExtensionAsync(string extension)
        {

            var emailExtension = await _context.EmailExtensions.FirstOrDefaultAsync(e => e.Extension == extension);
            if (emailExtension == null)
                throw _errorService.GetApiException(ErrorCodes.NotFound, $"La extensión '{extension}' no está registrada.");
            if (emailExtension.Status == false)
                throw _errorService.GetApiException(ErrorCodes.NotFound, $"La extensión '{extension}' no está disponible.");

            var response = new ExtensionRsDTO { ExtensionName = emailExtension.Extension, Status = emailExtension.Status };
            return new JSendResponse<ExtensionRsDTO> { Status = ResponseStatus.SUCCESS, Code = 200, Message = "Extensión encontrada.", Data = response };

        }

        public async Task<JSendResponse<ExtensionRsDTO>> ChangeExtensionStatus(string extension, bool status)
        {

            var emailExtension = await _context.EmailExtensions.FirstOrDefaultAsync(e => e.Extension == extension);
            if (emailExtension == null)
            {
                throw _errorService.GetApiException(ErrorCodes.NotFound, $"La extensión '{extension}' no está registrada.");
            }
            emailExtension.Status = status;
            await _context.SaveChangesAsync();

            var response = new ExtensionRsDTO { ExtensionName = emailExtension.Extension, Status = emailExtension.Status };
            return new JSendResponse<ExtensionRsDTO> { Status = ResponseStatus.SUCCESS, Code = 200, Message = "Extensión actualizada con éxito", Data = response };
        }


        public async Task<JSendResponse<string>> DeleteExtensionAsync(string extension)
        {
             var emailExtension = await _context.EmailExtensions.FirstOrDefaultAsync(e => e.Extension == extension);
            if (emailExtension == null)
                throw _errorService.GetApiException(ErrorCodes.NotFound, $"La extensión '{extension}' no está registrada.");

            _context.EmailExtensions.Remove(emailExtension);
            await _context.SaveChangesAsync();
            return new JSendResponse<string> { Status = ResponseStatus.SUCCESS, Code = 200, Message = "Extensión eliminada con éxito", Data = null };
        }

        private async Task<string[]> ValidateErrorsSmtpSettings(string extension)
        {
            var validationErrors = new List<string>();
            if (extension == null || extension.Equals(" "))
                validationErrors.Add($"La extensión no puede ser nula.");


            if (!Regex.IsMatch(extension, @"^(?!-)[A-Za-z0-9-]+(?<!-)\.[A-Za-z]{2,}(\.[A-Za-z]{2,})?$"))
                validationErrors.Add($"La extensión [{extension}] no es válida.");

            var existExtension = await _context.EmailExtensions.FirstOrDefaultAsync(e => e.Extension == extension);
            if (existExtension != null)
                throw _errorService.GetApiException(ErrorCodes.AlreadyRegistered, "Extensión ya registrado.");

            return validationErrors.ToArray();

        }

    }
}
