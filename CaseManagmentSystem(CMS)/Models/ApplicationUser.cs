using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace CaseManagementSystem.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        public string EmployeeNumber { get; set; } = string.Empty;

        [Required]
        public string FullName { get; set; } = string.Empty;
    }
}