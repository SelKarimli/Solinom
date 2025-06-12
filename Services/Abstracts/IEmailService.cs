namespace FinalProject.MVC.Services.Abstracts
{
    public interface IEmailService
    {
        void SendEmailConfirmation(string reciever, string name, string token);
        Task SendEmailAsync(string name, string email, string phone, string subject, string message);
    }
}
