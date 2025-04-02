using MailSenderAPI.Utils.Exceptions;

namespace MailSenderAPI.Middleware
{
    public class ExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionHandlerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (ApiException ex)
            {
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = ex.Code;
                var response = new { code = ex.Code, message = ex.Message, detail = ex.Detail };
                await context.Response.WriteAsJsonAsync(response);
            }
            catch (Exception ex)
            {
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = 500;
                var response = new { code = 500, message = "Server Error", detail = ex.Message };
                await context.Response.WriteAsJsonAsync(response);
            }
        }
    }


}
