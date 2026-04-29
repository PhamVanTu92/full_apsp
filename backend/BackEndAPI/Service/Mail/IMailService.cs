using BackEndAPI.Models.Banks;
using BackEndAPI.Models.Other;

namespace BackEndAPI.Service.Mail
{
    public interface IMailService
    {
        Task<bool> SendMailBy(InfoLoginMail Mail_Data, int type = 0);
        Task<(bool, Mess)> SendMail(MailData Mail_Data);
        Task SendMailOTP(OTPMail OTPMail);
        Task SendEmailAsync(string to, string subject, string body);
    }
}
