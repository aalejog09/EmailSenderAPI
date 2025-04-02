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

        public async Task<ApiResponse<ExtensionRsDTO>> AddExtensionAsync(string extension)
        {

            string[] validationErrors = await ValidateErrorsSmtpSettings(extension);


            if (validationErrors.Length > 0)
                throw _errorService.GetApiException(ErrorCodes.BadRequest, validationErrors);

            var emailExtension = new EmailExtension { Extension = extension, Status = true };
            _context.EmailExtensions.Add(emailExtension);
            await _context.SaveChangesAsync();

            var response = new ExtensionRsDTO { ExtensionName = emailExtension.Extension, Status = emailExtension.Status };
            return new ApiResponse<ExtensionRsDTO>(response);
        }

        public async Task<ApiResponse<List<ExtensionRsDTO>>> GetAllExtensionsAsync()
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

            var response = new ApiResponse<List<ExtensionRsDTO>>(extensionRsDTOs);

            return response;
        }

        public async Task<ApiResponse<ExtensionRsDTO>> GetByExtensionAsync(string extension)
        {

            var emailExtension  = await _context.EmailExtensions.FirstOrDefaultAsync(e => e.Extension == extension);
            if (emailExtension == null)
                throw _errorService.GetApiException(ErrorCodes.NotFound, $"La extensión '{extension}' no esta registrada.");
            var response = new ExtensionRsDTO { ExtensionName = emailExtension.Extension, Status = emailExtension.Status };
            return new ApiResponse<ExtensionRsDTO>(response);
        }

        public async Task<ApiResponse<ExtensionRsDTO>> GetAllowedByExtensionAsync(string extension)
        {

            var emailExtension = await _context.EmailExtensions.FirstOrDefaultAsync(e => e.Extension == extension);
            if (emailExtension == null)
                throw _errorService.GetApiException(ErrorCodes.NotFound, $"La extensión '{extension}' no esta registrada.");
            if (emailExtension.Status == false)
                throw _errorService.GetApiException(ErrorCodes.NotFound, $"La extensión '{extension}' no esta disponible.");

            var response = new ExtensionRsDTO { ExtensionName = emailExtension.Extension, Status = emailExtension.Status };
            return new ApiResponse<ExtensionRsDTO>(response);
        }

        public async Task<ApiResponse<ExtensionRsDTO>> ChangeExtensionStatus(string extension, bool status)
        {

            var emailExtension = await _context.EmailExtensions.FirstOrDefaultAsync(e => e.Extension == extension);
            if (emailExtension == null)
            {
                throw _errorService.GetApiException(ErrorCodes.NotFound, $"La extensión '{extension}' no esta registrada.");
            }
            emailExtension.Status = status;
            await _context.SaveChangesAsync();

            var response = new ExtensionRsDTO { ExtensionName = emailExtension.Extension, Status = emailExtension.Status };
            return new ApiResponse<ExtensionRsDTO>(response);
        }


        public async Task<ApiResponse<string>> DeleteExtensionAsync(string extension)
        {
             var emailExtension = await _context.EmailExtensions.FirstOrDefaultAsync(e => e.Extension == extension);
            if (emailExtension == null)
                throw _errorService.GetApiException(ErrorCodes.NotFound, $"La extension '{extension}' no esta registrada.");

            _context.EmailExtensions.Remove(emailExtension);
            await _context.SaveChangesAsync();
            return new ApiResponse<string>("Extension eliminada con exito");
        }

        private async Task<string[]> ValidateErrorsSmtpSettings(string extension)
        {
            var validationErrors = new List<string>();
            if (extension == null || extension.Equals(" "))
                validationErrors.Add($"La extencion no puede ser nula.");


            if (!Regex.IsMatch(extension, @"^(?!-)[A-Za-z0-9-]+(?<!-)\.[A-Za-z]{2,}(\.[A-Za-z]{2,})?$"))
                validationErrors.Add($"La extensión [{extension}] no es válida.");

            var existExtension = await _context.EmailExtensions.FirstOrDefaultAsync(e => e.Extension == extension);
            if (existExtension != null)
                throw _errorService.GetApiException(ErrorCodes.AlreadyRegistered, "Extension ya registrado.");

            return validationErrors.ToArray();

        }

    }
}
