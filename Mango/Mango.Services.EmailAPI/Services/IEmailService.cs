using Mango.Services.EmailAPI.Message;
using Mango.Services.EmailAPI.Models.Dto;

namespace Mango.Services.EmailAPI.Services
{
    public interface IEmailService
    {
        Task EmailCartAndLog(CartDto cartDto);
        Task RegisterUserEmailAndLog(string email);
        Task<bool> SendEmail(string ToEmail, string subject, string body);
        Task LogOrderPlaced(RewardsMessage rewardsDto);
    }
}
