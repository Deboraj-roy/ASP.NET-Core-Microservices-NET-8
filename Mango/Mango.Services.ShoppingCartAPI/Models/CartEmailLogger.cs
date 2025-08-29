namespace Mango.Services.ShoppingCartAPI.Models
{
    public class CartEmailLogger
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public DateTime? EmailSent { get; set; }
    }
}
