using Mango.Services.ShoppingCartAPI.Service.IService;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;

namespace Mango.Services.ShoppingCartAPI.Service
{
    public class SendEmailService : ISendEmailService
    {
        //private readonly IMailtrapClient _mailtrapClient;
        private readonly IConfiguration _configuration;
        private readonly string Host;
        private readonly string Port;
        private readonly string Username;
        private readonly string Password;
        private string FromEmail;
        private string FromName;

        public SendEmailService(IConfiguration config)
        {
            //_mailtrapClient = mailtrapClient;
            this._configuration = config;
            this.Host = _configuration.GetValue<string>("Mailtrap:Host") ?? "sandbox.smtp.mailtrap.io";
            this.Port = _configuration.GetValue<string>("Mailtrap:Port") ?? "2525";
            this.Username = _configuration.GetValue<string>("Mailtrap:Username") ?? "2032fbaa2dd6f6";
            this.Password = _configuration.GetValue<string>("Mailtrap:Password") ?? "135756f590ea4d";
            this.FromEmail = _configuration.GetValue<string>("Mailtrap:FromEmail") ?? "deborajroy123@gmail.com"; 
            this.FromName = _configuration.GetValue<string>("Mailtrap:FromName") ?? "DEBORAJ ROY";
        }

        public async Task<string> SendAsync(string toEmail, string subject, string body)
        {
            try
            {
                var port = int.Parse(this.Port);
                var client = new SmtpClient(Host, port)
                {
                    Credentials = new NetworkCredential(Username, Password),
                    EnableSsl = true
                };
                await client.SendMailAsync(FromEmail, toEmail, subject, body);
                System.Console.WriteLine("Sent");

                return "Sent";
            }
            catch (Exception ex)
            {
                // Mailtrap specific errors
                Console.WriteLine("Mailtrap error: " + ex.Message, ex);
                return ex.Message;
            }
        }
    }
}
 
