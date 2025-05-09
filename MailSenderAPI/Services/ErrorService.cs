namespace MailSenderAPI.Services
{
    using MailSenderAPI.Utils.Exceptions;
    using System.Text.Json;

    public class ErrorService
    {
        private readonly Dictionary<int, (string message, string detail)> _errorDictionary;

        public ErrorService(IWebHostEnvironment env)
        {
            var errorFilePath = Path.Combine(env.ContentRootPath, "Utils","Exceptions", "ErrorMessagesConfig.json");
            var json = File.ReadAllText(errorFilePath);
            var errors = JsonSerializer.Deserialize<ErrorCollection>(json);
            if(errors != null)
                _errorDictionary = errors.Errors.ToDictionary(e => e.Code, e => (e.Message, e.Detail));
        }

        public ApiException GetApiException(ErrorCodes errorCode, params string[] additionalDetails)
        {
            if (_errorDictionary.TryGetValue((int)errorCode, out var error))
            {
                var detailList = new List<string> { error.detail };

                if (additionalDetails != null && additionalDetails.Length > 0)
                {
                    detailList.AddRange(additionalDetails);
                }

                return new ApiException((int)errorCode, error.message, detailList);
            }

            return new ApiException(500, "Error Desconocido", new List<string> { "Verificar en la aplicacion." });
        }
    }

    public class ErrorCollection
    {
        public required List<ErrorItem> Errors { get; set; }
    }

    public class ErrorItem
    {
        public int Code { get; set; }
        public required string Message { get; set; }
        public required string Detail { get; set; }
    }

}
