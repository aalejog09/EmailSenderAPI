using MailSenderAPI.Context;
using MailSenderAPI.Models;
using MailSenderAPI.Models.DTO;
using MailSenderAPI.Models.JsonAPIResposes.MailSenderAPI.Utils.Responses;
using MailSenderAPI.Models.NewFolder;
using MailSenderAPI.Utils.Exceptions;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace MailSenderAPI.Services
{
    public class SmtpSettingsService
    {
        private readonly ApplicationDbContext _context;
        private readonly EncryptionService _encryptionService;
        private readonly ErrorService _errorService;

        public SmtpSettingsService(ApplicationDbContext context, EncryptionService encryptionService, ErrorService errorService)
        {
            _context = context;
            _encryptionService = encryptionService;
            _errorService = errorService;
        }

        public async Task<ApiResponse<List<SmtpSettignsRsDTO>>> GetAllSmtpSettingsAsync()
        {
            var smtpSettingsList =  await _context.SmtpSettings.ToListAsync();
            if (smtpSettingsList.Count == 0)
                throw _errorService.GetApiException(ErrorCodes.NotFound, "No existe algun SMTP configurado");

            return new ApiResponse<List<SmtpSettignsRsDTO>>(mapToDTOlist(smtpSettingsList));
        }

       
        public async Task<ApiResponse<SmtpSettignsRsDTO>> CreateAsync(SmtpSettignsRqDTO smtpSettingsDTO)
        {

            string[] validationErrors = await ValidateErrorsSmtpSettignsRqDTO(smtpSettingsDTO);
            if (validationErrors.Length > 0)
                throw _errorService.GetApiException(ErrorCodes.BadRequest, validationErrors);

            //encryptar password antes de mapear a entidad
            smtpSettingsDTO.Password = _encryptionService.Encrypt(smtpSettingsDTO.Password);
            
            var smtpSettings = mapToEntity(smtpSettingsDTO);
           
            _context.SmtpSettings.Add(smtpSettings); 
            await _context.SaveChangesAsync();

            return new ApiResponse<SmtpSettignsRsDTO>(mapToDTO(smtpSettings));
        }

        public async Task<ApiResponse<SmtpSettignsRsDTO>> GetByIdAsync(int id)
        {
            var smtpSettings = await _context.SmtpSettings.FirstOrDefaultAsync(s => s.Id == id);

            return smtpSettings == null
                ? throw _errorService.GetApiException(ErrorCodes.NotFound, "Smtp no encontrado")
                : new ApiResponse<SmtpSettignsRsDTO>(mapToDTO(smtpSettings));
        }

        public async Task<SmtpSettignsRsDTO> GetByEmailAsync(string FromEmail)
        {
            var smtpSettings = await _context.SmtpSettings.FirstOrDefaultAsync(s => s.FromEmail == FromEmail) ??
                null;

            if (smtpSettings != null)
            {
                SmtpSettignsRsDTO smtpSettignsRsDTO = mapToDTO(smtpSettings);

                return smtpSettignsRsDTO;
            }
            return null;

        }

        public async Task<ApiResponse<SmtpSettignsRsDTO>> GetSettingsDTOByEmailAsync(string FromEmail)
        {
            var smtpSettings = await _context.SmtpSettings.FirstOrDefaultAsync(s => s.FromEmail == FromEmail) ??
               throw _errorService.GetApiException(ErrorCodes.NotFound, "Smtp no encontrado.");

            return new ApiResponse<SmtpSettignsRsDTO>(mapToDTO(smtpSettings));

        }


        public async Task<ApiResponse<string>> DeleteAsync(string fromEmail)
        {
            var smtpSettings = await _context.SmtpSettings.FirstOrDefaultAsync(s => s.FromEmail == fromEmail) ?? 
                throw _errorService.GetApiException(ErrorCodes.NotFound, "Smtp no encontrado.");
           
            _context.SmtpSettings.Remove(smtpSettings);
            
            await _context.SaveChangesAsync();
            return new ApiResponse<string>("Registro eliminado correctamente."); ;
        }

        private SmtpSettignsRsDTO mapToDTO(SmtpSettings smtpSettings)
        {
            SmtpSettignsRsDTO SmtpSettignsRsDTO = new SmtpSettignsRsDTO
            {
                CreatedAt = smtpSettings.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                FromEmail = smtpSettings.FromEmail,
                Username = smtpSettings.Username,
                Host = smtpSettings.Host,
                Port = smtpSettings.Port,
                UseSSL = smtpSettings.UseSSL,
                Password = smtpSettings.Password
            };

            return SmtpSettignsRsDTO;
        }

        private List<SmtpSettignsRsDTO> mapToDTOlist(List<SmtpSettings> SmtpSettingsList)
        {
            List<SmtpSettignsRsDTO> smtpSettingsList = new  List<SmtpSettignsRsDTO>();
            foreach (var item in SmtpSettingsList)
            {
                SmtpSettignsRsDTO smtpSettignsRsDTO = mapToDTO(item);
                smtpSettingsList.Add(smtpSettignsRsDTO);
            }
            return smtpSettingsList;

        }

        private SmtpSettings mapToEntity(SmtpSettignsRqDTO smtpSettingsDTO)
        {
            SmtpSettings smtpSettings = new SmtpSettings
            {
                CreatedAt = DateTime.Now,
                FromEmail = smtpSettingsDTO.FromEmail,
                Username = smtpSettingsDTO.Username,
                Host = smtpSettingsDTO.Host,
                Port = smtpSettingsDTO.Port,
                UseSSL = smtpSettingsDTO.UseSSL,
                Password = smtpSettingsDTO.Password
            };

            return smtpSettings;
        }

        private async Task<string[]> ValidateErrorsSmtpSettignsRqDTO(SmtpSettignsRqDTO smtpSettingsDTO)
        {

            if (smtpSettingsDTO == null)
                throw _errorService.GetApiException(ErrorCodes.BadRequest, "Verifique el contenido del body");

            var validationErrors = new List<string>();

            // Validar host (IP o dominio)
            if (!Regex.IsMatch(smtpSettingsDTO.Host, @"^(([a-zA-Z0-9\-\.]+)\.([a-zA-Z]{2,})|(\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}))$"))
                validationErrors.Add("El host debe ser un dominio o IP válida.");

            // Validar puerto
            if (smtpSettingsDTO.Port < 1 || smtpSettingsDTO.Port > 999999)
                validationErrors.Add("El puerto debe ser un número entre 1 y 999999.");

            // Validar email
            if (!new EmailAddressAttribute().IsValid(smtpSettingsDTO.FromEmail))
                validationErrors.Add("El formato del correo electrónico no es válido.");

            SmtpSettignsRsDTO smtpSettingsExist = await GetByEmailAsync(smtpSettingsDTO.FromEmail);

            if(smtpSettingsExist != null)
                throw _errorService.GetApiException(ErrorCodes.AlreadyRegistered, "Smtp ya registrado.");

            return validationErrors.ToArray();

        }


    }
}