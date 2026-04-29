namespace BackEndAPI.Models.Other
{
    public class Endpoints
    {
        public string Host { get; set; }
    }

    public class MailSettings
    {
        public string EmailId { get; set; }
        public string Name { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public bool UseSSL { get; set; }
        public string Link { get; set; }
    }

    public class MailData
    {
        public string EmailToId { get; set; }
        public string EmailToName { get; set; }
        public string EmailSubject { get; set; }
        public string EmailBody { get; set; }
        public string Link { get; set; }
    }

    public class InfoLoginMail
    {
        public string ToEmail { get; set; }
        public string Name { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        public string CustomerName { get; set; }
        public string ForgotLink { get; set; }
    }
    public class OTPMail
    {
        public string ToEmail { get; set; }
        public string OTP { get; set; }
    }
}