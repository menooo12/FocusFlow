using Microsoft.AspNetCore.Identity;

namespace FocusFlow.Models
{
    public class ApplicationUser : IdentityUser
    {
        public int WeeklyTargetHours { get; set; } = 20;
    }
}