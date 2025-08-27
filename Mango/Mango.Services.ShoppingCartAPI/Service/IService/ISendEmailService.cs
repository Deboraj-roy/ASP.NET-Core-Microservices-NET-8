namespace Mango.Services.ShoppingCartAPI.Service.IService
{
    public interface ISendEmailService
    {
        Task<string> SendAsync(string toEmail, string subject, string body);
    }
}
