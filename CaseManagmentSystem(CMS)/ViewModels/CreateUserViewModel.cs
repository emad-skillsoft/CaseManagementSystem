using System.ComponentModel.DataAnnotations;

namespace CaseManagmentSystem_CMS_.ViewModels
{
    public class CreateUserViewModel
    {
        [Required]
        public string FullName { get; set; } = string.Empty;

        [Required]
        public string EmployeeNumber { get; set; } = string.Empty;

        [Required]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = string.Empty;
    }
}