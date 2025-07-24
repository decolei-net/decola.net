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

        // metodo para enviar email assíncrono
        public async Task EnviarEmailAsync(string destinatario, string assunto, string corpoHtml)
        {
            // pegando as configurações do SMTP do appsettings.json
            var smtp = _config.GetSection("Smtp");

            // verificando se as configurações do SMTP estão preenchidas
            var client = new SmtpClient(smtp["Host"], int.Parse(smtp["Port"]))
            {
                // Configurando as credenciais do SMTP
                Credentials = new NetworkCredential(smtp["User"], smtp["Pass"]),
                // Habilitando SSL para segurança; ssl é um protocolo de segurança para comunicação via email, ele faza comunicação entre o cliente e o servidor de email mais segura.
                EnableSsl = true
            };

            // Criando a mensagem de email
            var mensagem = new MailMessage
            {
                From = new MailAddress(smtp["User"], smtp["Sender"]),
                Subject = assunto,
                Body = corpoHtml,
                IsBodyHtml = true
            };

            // Adicionando o destinatário - que é passo quando eu insiro o email no controller
            mensagem.To.Add(destinatario);
            // Enviando o email
            await client.SendMailAsync(mensagem);
        }
    }
}
