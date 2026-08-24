using CaseManagementSystem.Enums;

namespace CaseManagementSystem.Models
{
    public class SLAConfiguration
    {
        public int Id { get; set; }
        public CasePriority Priority { get; set; }
        public int AllowedHours { get; set; }
    }
}
