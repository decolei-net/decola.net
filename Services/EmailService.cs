using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Decolei.net.Services
{
    public class EmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task EnviarEmailAsync(string destinatario, string assunto, string corpoHtml)
        {
            var smtp = _config.GetSection("Smtp");
            var client = new SmtpClient(smtp["Host"], int.Parse(smtp["Port"]))
            {
                Credentials = new NetworkCredential(smtp["User"], smtp["Pass"]),
                EnableSsl = true
            };

            var mensagem = new MailMessage
            {
                From = new MailAddress(smtp["User"], smtp["Sender"]),
                Subject = assunto,
                Body = corpoHtml,
                IsBodyHtml = true
            };

            mensagem.To.Add(destinatario);
            await client.SendMailAsync(mensagem);
        }
    }
}
