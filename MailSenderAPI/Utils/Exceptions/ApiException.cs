namespace MailSenderAPI.Utils.Exceptions
{
    public class ApiException : Exception
    {
        public int Code { get; set; }
        public string Message { get; set; }
        public List<string> Detail { get; set; }

        public ApiException(int code, string message, List<string> detail = null)
        {
            Code = code;
            Message = message;
            Detail = detail ?? new List<string>(); // Si no hay detalles adicionales, inicializa con una lista vacía
        }
    }
}
