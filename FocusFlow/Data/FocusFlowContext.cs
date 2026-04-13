using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using FocusFlow.Models;

namespace FocusFlow.Data
{
    public class FocusFlowContext : IdentityDbContext<IdentityUser>
    {
        public FocusFlowContext(DbContextOptions<FocusFlowContext> options)
            : base(options)
        {
        }

        public DbSet<Subject> Subjects { get; set; }
        public DbSet<StudySession> StudySessions { get; set; }
        public DbSet<FocusTask> FocusTask { get; set; }
    }
}