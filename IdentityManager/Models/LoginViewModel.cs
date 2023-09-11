using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace IdentityManager.Models
{
    public class LoginViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display (Name ="Remember Me ?")]
        [Required]
        public bool RememberMe { get; set; }
    }
}
