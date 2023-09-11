using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IdentityManager.Models
{
    public class RegisterViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name="Email")]
        public string Email { get; set; }
        [Required]
        [StringLength(100,ErrorMessage ="The {0} Must be at least {2} characters long .")]
        [DataType(DataType.Password)]
        [Display(Name="Password")]
        public string Password { get; set; }
        [DataType(DataType.Password)]
        [Display(Name="Confirm Password")]
        [Compare("Password",ErrorMessage ="The passwrod and confirmation password do not match")]
        public string ConfirmPassword { get; set; }
        [Required]
        public string Name { get; set; }

        public IEnumerable<SelectListItem>RoleList { get; set; }
        public string RoleSelected { get; set; }

    }
}
