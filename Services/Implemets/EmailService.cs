using FinalProject.MVC.Helpers;
using FinalProject.MVC.Services.Abstracts;
using Azure.Core;
using Humanizer;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace FinalProject.MVC.Services.Implemets
{
    public class EmailService : IEmailService
    {
        readonly SmtpClient _client;
        readonly MailAddress _from;
        readonly HttpContext Context;
        public EmailService(IOptions<SmtpOptions> option, IHttpContextAccessor acc)
        {
            var opt = option.Value;
            _client = new(opt.Host, opt.Port);
            _client.Credentials = new NetworkCredential(opt.Sender, opt.Password);
            _client.EnableSsl = true;
            _from = new MailAddress(opt.Sender, "Fatima");
            Context = acc.HttpContext;
        }

        public void SendEmailConfirmation(string reciever, string name, string token)
        {
            MailAddress to = new(reciever);
            MailMessage msg = new MailMessage(_from,to);
            msg.IsBodyHtml = true;
            msg.Subject = "Confirm your email adress";
            string url = Context.Request.Scheme + "://" + Context.Request.Host + "/Account/VerifyEmail?code=" + token+"&user="+name;
            msg.Body = EmailTemplates.VerifyEmail.Replace("__$name", name).Replace("__$link", url);
            _client.Send(msg);
        }
        public async Task SendEmailAsync(string name, string email, string phone, string subject, string message)
        {
            var mailMessage = new MailMessage
            {
                From = _from,
                Subject = $"New Contact Form Submission: {subject}",
                Body = $"Name: {name}\nEmail: {email}\nPhone: {phone}\n\nMessage:\n{message}",
                IsBodyHtml = false
            };

            mailMessage.To.Add("fatimaor@code.edu.az");

            try
            {
                await _client.SendMailAsync(mailMessage);
            }
            catch (Exception ex)
            {
                // Handle or log the exception as needed
                throw;
            }
        }

    }
}
