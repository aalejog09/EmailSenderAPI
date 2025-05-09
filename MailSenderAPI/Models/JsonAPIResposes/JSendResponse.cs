using Microsoft.AspNetCore.Mvc;

namespace MailSenderAPI.Models.JsonAPIResposes
{
    namespace MailSenderAPI.Utils.Responses
    {
        public class JSendResponse : JSendResponse<object> { }
        public class JSendResponse<T>
        {
            public string Status { get; set; } = ResponseStatus.SUCCESS;

            public int Code { get; set; } = 200;
            public string? Message { get; set; }

            public T? Data { get; set; }

            public OkObjectResult OkResponse() => new OkObjectResult(this);

            public override string ToString()
            {
                return $"Status: {Status},  Code: {Code}, Message: {Message},Data: {Data},";
            }
        }

        public static class ResponseStatus
        {
            public static string SUCCESS = "Success";
            public static string FAIL = "Error";
        }
    }
}
