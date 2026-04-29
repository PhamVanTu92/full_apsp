using System.Net;
using System.Net.Mail;
using System.Text;
using BackEndAPI.Models;
using BackEndAPI.Models.Other;
using BackEndAPI.TemplateHtml;
using MailKit.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing.Template;
using Microsoft.Extensions.Options;
using MimeKit;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;

namespace BackEndAPI.Service.Mail
{
    public static class TemplateHelper
    {
        public static string GetEmailTemplate(string templatePath, Dictionary<string, string> placeholders)
        {
            var template = File.ReadAllText(templatePath);
            foreach (var placeholder in placeholders)
            {
                template = template.Replace(placeholder.Key, placeholder.Value);
            }

            return template;
        }
    }

    public class MailService : IMailService
    {
        MailSettings Mail_Settings;
        SmtpSettings _smtpSettings;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;

        public MailService(IOptions<MailSettings> options, IConfiguration configuration, IWebHostEnvironment env, SmtpSettings smtpSettings)
        {
            Mail_Settings = options.Value;
            _configuration = configuration;
            _env = env;
            _smtpSettings = smtpSettings;
        }

        public async Task<(bool, Mess)> SendMail(MailData Mail_Data)
        {
            Mess mess = new Mess();
            try
            {
                //MimeMessage - a class from Mimekit
                MimeMessage email_Message = new MimeMessage();
                MailboxAddress email_From = new MailboxAddress(Mail_Settings.Name, Mail_Settings.EmailId);
                email_Message.From.Add(email_From);
                MailboxAddress email_To = new MailboxAddress(Mail_Data.EmailToName, Mail_Data.EmailToId);
                email_Message.To.Add(email_To);
                email_Message.Subject = Mail_Data.EmailSubject;
                var placeholders = new Dictionary<string, string>
                {
                    { "{{UserName}}", Mail_Data.EmailToName },
                    { "{{ConfirmationLink}}", Mail_Settings.Link }
                };
                var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "TemplateHtml", "RegisterEmail.html");
                var emailBody = TemplateHelper.GetEmailTemplate(templatePath, placeholders);
                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = emailBody
                };
                email_Message.Body = bodyBuilder.ToMessageBody();
                //this is the SmtpClient class from the Mailkit.Net.Smtp namespace, not the System.Net.Mail one
                SmtpClient MailClient = new SmtpClient();
                MailClient.Connect(Mail_Settings.Host, Mail_Settings.Port, Mail_Settings.UseSSL);
                MailClient.Authenticate(Mail_Settings.EmailId, Mail_Settings.Password);
                MailClient.Send(email_Message);
                MailClient.Disconnect(true);
                MailClient.Dispose();
                return (true, null);
            }
            catch (Exception ex)
            {
                mess.Status = 900;
                mess.Errors = ex.Message;
                return (false, mess);
            }
        }

        public async Task<bool> SendMailBy(InfoLoginMail mailData, int type = 0)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("AP Saigon Petro JSC", _smtpSettings.Username)); // Email gửi đi
                message.To.Add(new MailboxAddress("Recipient Name", mailData.ToEmail)); // Email người nhận
                switch (type)
                {
                    case 0:
                        message.Subject = "[APSP Portal] Thông tin đăng nhập hệ thống quản lý";
                        break;
                    case 1:
                        message.Subject = "[APSP Portal] Thông tin tài khoản truy cập hệ thống đặt hàng";
                        break;
                    case 2:
                    case 3:
                        message.Subject = "[APSP Portal] Yêu cầu đặt lại mật khẩu tài khoản";
                        break;
                }

                var placeholders = new Dictionary<string, string>
                {
                    { "[UserName]", mailData.ToEmail },
                    { "[Password]", mailData.Password },
                    { "[CustomerName]", mailData.Name },
                    { "[ForgotLink]", mailData.ForgotLink },
                };
                var templatePath = Path.Combine(_env.WebRootPath, "TemplateHtml", "LoginInfo.html");
                if (type == 1)
                {
                    templatePath = Path.Combine(_env.WebRootPath, "TemplateHtml", "LoginInfoApsp.html");
                }

                switch (type)
                {
                    case 0:
                        templatePath = Path.Combine(_env.WebRootPath, "TemplateHtml", "LoginInfo.html");
                        break;
                    case 1:
                        templatePath = Path.Combine(_env.WebRootPath, "TemplateHtml", "LoginInfoApsp.html");
                        break;
                    case 2:
                        templatePath = Path.Combine(_env.WebRootPath, "TemplateHtml", "ForgotPassword.html");
                        break;
                    case 3:
                        templatePath = Path.Combine(_env.WebRootPath, "TemplateHtml", "ForgotPasswordApsp.html");
                        break;
                }

                var emailBody = TemplateHelper.GetEmailTemplate(templatePath, placeholders);
                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = emailBody
                };
                message.Body = bodyBuilder.ToMessageBody();
                message.Body = new TextPart("html") { Text = emailBody };
                using (var client = new SmtpClient())
                {
                    // Kết nối tới SMTP server của Microsoft 365
                    await client.ConnectAsync(_smtpSettings.Host, _smtpSettings.Port, SecureSocketOptions.StartTls);
                    await client.AuthenticateAsync(_smtpSettings.Username, _smtpSettings.Password);
                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);
                }

                Console.WriteLine("Email sent successfully!");

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send email: {ex.Message}");
                return false;
            }
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("AP Saigon Petro JSC", _smtpSettings.Username));
            message.To.Add(new MailboxAddress(to, to));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder { HtmlBody = body };
            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(_smtpSettings.Host, _smtpSettings.Port, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_smtpSettings.Username, _smtpSettings.Password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }

        public async Task SendMailOTP(OTPMail OTPMail)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("AP Saigon Petro JSC", _smtpSettings.Username));
            message.To.Add(new MailboxAddress(OTPMail.ToEmail, OTPMail.ToEmail));
            message.Subject = "Mã xác thực đăng nhập tài khoản";

            var bodyBuilder = new BodyBuilder { HtmlBody = OTPMail.OTP };
            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(_smtpSettings.Host, _smtpSettings.Port, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_smtpSettings.Username, _smtpSettings.Password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}