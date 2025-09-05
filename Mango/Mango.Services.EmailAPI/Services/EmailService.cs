using Mango.Services.EmailAPI.Data;
using Mango.Services.EmailAPI.Message;
using Mango.Services.EmailAPI.Models;
using Mango.Services.EmailAPI.Models.Dto;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;

namespace Mango.Services.EmailAPI.Services
{
    public class EmailService : IEmailService
    {
        private DbContextOptions<AppDbContext> _dbOptions;

        public EmailService(DbContextOptions<AppDbContext> options)
        {
            this._dbOptions = options;
        }

        public async Task EmailCartAndLog(CartDto cartDto)
        {
            StringBuilder message = new StringBuilder();

            message.AppendLine("<br/>Cart Email Requested ");
            message.AppendLine("<br/>Total " + cartDto.CartHeader.CartTotal);
            message.Append("<br/>");
            message.Append("<ul>");
            foreach (var item in cartDto.CartDetails)
            {
                message.Append("<li>");
                message.Append(item.Product.Name + " x " + item.Count);
                message.Append("</li>");
            }
            message.Append("</ul>");

            await LogAndEmail(message.ToString(), cartDto.CartHeader.Email);
            //SendEmail(await Task.FromResult(cartDto.CartHeader.Email), "Cart Email", message.ToString());

        }

        private async Task<bool> LogAndEmail(string message, string email)
        {
            try
            {
                EmailLogger emailLog = new()
                {
                    Email = email,
                    EmailSent = DateTime.Now,
                    Message = message
                };
                await using var _db = new AppDbContext(_dbOptions);
                await _db.EmailLoggers.AddAsync(emailLog);
                await _db.SaveChangesAsync();

                await SendEmail(email, "Email log", message.ToString());

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task LogOrderPlaced(RewardsMessage rewardsDto)
        {
            string message = "New Order Placed. <br/> Order ID : " + rewardsDto.OrderId;
            await LogAndEmail(message, "deborajroy123@gmail.com");
        }

        public async Task RegisterUserEmailAndLog(string email)
        {
            string message = "User Registeration Successful. <br/> Email : " + email;
            await LogAndEmail(message, "deborajroy123@gmail.com");
        }

        public async Task<bool> SendEmail(string ToEmail, string subject, string body)
        {
            var fromEmail = "Deb@dotnet.com";
            var userName = "2032fbaa2dd6f6";
            var fromEmailPassword = "135756f590ea4d";
            var smtpHost = "sandbox.smtp.mailtrap.io";
            var port = 587;

            var message = new MailMessage()
            {
                From = new MailAddress(fromEmail),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            if (string.IsNullOrEmpty(ToEmail))
            {
                throw new ArgumentNullException(nameof(ToEmail));
            }

            //define a regex pattern for email validation
            string pattern = @"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$";
            Regex regex = new Regex(pattern);

            // return true if email is valid pattern
            var result = regex.IsMatch(ToEmail);

            if (result)
            {
                message.To.Add(ToEmail);

                var smtpClient = new SmtpClient(smtpHost)
                {
                    Port = Convert.ToInt32(port),
                    Credentials = new System.Net.NetworkCredential(userName, fromEmailPassword),
                    EnableSsl = true
                };

                await smtpClient.SendMailAsync(message);
                smtpClient.Dispose();
                return true;
            }
            else
            {
                throw new ArgumentException("Invalid email address.");
            }
        }
    }
}
