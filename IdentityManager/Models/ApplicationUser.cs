using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IdentityManager.Models
{
    public class ApplicationUser :IdentityUser 
    {
        [Required]
        public string Name { get; set; }
        [NotMapped] // we do not want to add that in database for this reason we used this anotation
        [Key]
        public int RoleId { get; set; }
        [NotMapped]
        public string Role { get; set; }
        [NotMapped]
        public IEnumerable<SelectListItem>RoleList { get; set; }
    }
}
