using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Concert.Services;

public class EmailService
{
    public async Task SendEmailAsync(string toEmail, string subject, string message)
    {
        // ЗАМІНИ НА СВОЮ ПОШТУ ТА 16-ЗНАЧНИЙ ПАРОЛЬ ДОДАТКА З GOOGLE
        var fromAddress = new MailAddress("nastia.domra@gmail.com", "ConcertApp Support");
        var toAddress = new MailAddress(toEmail);
        const string fromPassword = "kgzb kctu cizy lcvc"; 

        var smtp = new SmtpClient
        {
            Host = "smtp.gmail.com",
            Port = 587,
            EnableSsl = true,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
        };

        using var emailMessage = new MailMessage(fromAddress, toAddress)
        {
            Subject = subject,
            Body = message,
            IsBodyHtml = true
        };

        await smtp.SendMailAsync(emailMessage);
    }
}