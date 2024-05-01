using System.ComponentModel.DataAnnotations;

namespace DeaneryManagementSystem.Models.Authentication
{
    public class RegisterModel
    {
        [Required]
        public int? PersonId { get; set; }

        [Required, MinLength(8)]
        public string Password { get; set; }

        [MaxLength(15)]
        public string? AccessKey { get; set; }
    }
}
