using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mango.Services.ShoppingCartAPI.Models
{
    [Table("MailConfigurations")]
    public class MailConfiguration
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Host { get; set; } = string.Empty;

        public int Port { get; set; } = 587;

        [MaxLength(200)]
        public string Username { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Password { get; set; } = string.Empty;

        [MaxLength(200)]
        public string FromEmail { get; set; } = string.Empty;

        [MaxLength(200)]
        public string FromName { get; set; } = string.Empty;

        public bool UseSsl { get; set; } = false;

        public bool UseStartTls { get; set; } = true;

        /// <summary>
        /// Mark only one config as active (true) - the repository will fetch the active one.
        /// </summary>
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
