using Mango.Services.ShoppingCartAPI.Data;
using Mango.Services.ShoppingCartAPI.Models;
using Mango.Services.ShoppingCartAPI.Models.Dto;
using Mango.Services.ShoppingCartAPI.Service.IService;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;

namespace Mango.Services.ShoppingCartAPI.Service
{
    public class SendEmailService : ISendEmailService
    {
        private readonly string Host;
        private readonly int Port;
        private readonly string Username;
        private readonly string Password;
        private readonly string FromEmail;
        private readonly string FromName;
        private readonly AppDbContext _db;


        public SendEmailService(IConfiguration config, AppDbContext db)
        {
            this._db = db ?? throw new ArgumentNullException(nameof(db));
            var mailConfig = _db.MailConfigurations
                                    .AsNoTracking()
                                    .Where(s => s.IsActive == true)
                                    .FirstOrDefault();
            if (mailConfig != null)
            {
                this.Host = mailConfig.Host;
                this.Port = mailConfig.Port;
                this.Username = mailConfig.Username;
                this.Password = mailConfig.Password;
                this.FromEmail = mailConfig.FromEmail;
                this.FromName = mailConfig.FromName;
            }
            else
            {
                this.Host = config["Mailtrap:Host"] ?? throw new ArgumentNullException("Mailtrap:Host");
                this.Port = int.TryParse(config["Mailtrap:Port"], out var p) ? p : 2525;
                this.Username = config["Mailtrap:Username"] ?? throw new ArgumentNullException("Mailtrap:Username");
                this.Password = config["Mailtrap:Password"] ?? throw new ArgumentNullException("Mailtrap:Password");
                this.FromEmail = config["Mailtrap:FromEmail"] ?? throw new ArgumentNullException("Mailtrap:FromEmail");
                this.FromName = config["Mailtrap:FromName"] ?? "No Name";
            }

        }

        public async Task<bool> SendAsync(string toEmail, string subject, string body)
        {
            try
            {

                await LogCardEmailAsync(toEmail, subject, body);

                ///Send Raw Html Body...
                //using var client = new SmtpClient(Host, Port)
                //{
                //    Credentials = new NetworkCredential(Username, Password),
                //    EnableSsl = true
                //};

                //var message = new MailMessage
                //{
                //    From = new MailAddress(FromEmail, FromName),
                //    Subject = subject,
                //    Body = body,
                //    IsBodyHtml = false
                //};
                //message.To.Add(new MailAddress(toEmail));

                //await client.SendMailAsync(message);

                ///A decent Body
                ///
                var message = new MailMessage()
                {
                    From = new MailAddress(FromEmail),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                if (string.IsNullOrEmpty(toEmail))
                {
                    //throw new ArgumentNullException(nameof(toEmail));
                }

                //define a regex pattern for email validation
                string pattern = @"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$";
                Regex regex = new Regex(pattern);

                // return true if email is valid pattern
                var result = regex.IsMatch(toEmail);

                if (result)
                {
                    message.To.Add(toEmail);

                    var smtpClient = new SmtpClient(Host)
                    {
                        Port = Convert.ToInt32(Port),
                        Credentials = new NetworkCredential(Username, Password),
                        EnableSsl = true
                    };

                    await smtpClient.SendMailAsync(message);
                    smtpClient.Dispose();
                }


                //Console.WriteLine("Email sent successfully.");

                return true;
            }
            catch (Exception ex)
            {
                //Console.WriteLine($"Mailtrap send error: {ex.Message}");
                return false;
            }
        }


        private async Task<bool> LogCardEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                CartEmailLogger emailLog = new()
                {
                    Email = toEmail,
                    Subject = subject,
                    Message = body,
                    EmailSent = DateTime.Now,
                };
                await _db.CartEmailLoggers.AddAsync(emailLog);
                await _db.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }


    }

}

