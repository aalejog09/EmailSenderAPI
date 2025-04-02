namespace MailSenderAPI.Models.DTO
{
    public class SmtpSettignsRqDTO
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool UseSSL { get; set; }
        public string FromEmail { get; set; }
    }
}
