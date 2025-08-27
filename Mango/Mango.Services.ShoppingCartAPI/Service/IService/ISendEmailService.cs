namespace Mango.Services.ShoppingCartAPI.Service.IService
{
    public interface ISendEmailService
    {
        Task<bool> SendAsync(string toEmail, string subject, string body);
    }
}
