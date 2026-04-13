using System.ComponentModel.DataAnnotations;

namespace FocusFlow.Models
{
    public class Subject
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Range(1, 10000)]
        public string UserId { get; set; } = string.Empty;
        public int TargetHours { get; set; }

        // Navigation property — one Subject has many StudySessions
        public ICollection<StudySession> StudySessions { get; set; }
            = new List<StudySession>();
    }
}