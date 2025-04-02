namespace MailSenderAPI.Models.JsonAPIResposes
{
    namespace MailSenderAPI.Utils.Responses
    {
        public class ApiResponse<T>
        {
            public int Code { get; set; } = 200; // Código de éxito por defecto
            public T Data { get; set; } // Contenido de la respuesta

            public ApiResponse(T data)
            {
                Data = data;
            }
        }
    }
}
