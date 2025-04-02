namespace MailSenderAPI.Utils.Exceptions
{
    public enum ErrorCodes
    {
        BadRequest = 400,
        AuthenticationFailed=401,
        NotFound = 404,
        AlreadyRegistered = 409,
        ServerError = 500,
        
    }
}
