using System.ComponentModel.DataAnnotations;

namespace FocusFlow.Models
{
    public class Subject
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Subject name is required.")]
        [MaxLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
        public string Name { get; set; } = string.Empty;
        [Range(1, 10000, ErrorMessage = "Target hours must be between 1 and 10000.")]
        public int TargetHours { get; set; }
        public string UserId { get; set; } = string.Empty;

        // Navigation property
        public ICollection<StudySession> StudySessions { get; set; }
            = new List<StudySession>();
    }
}