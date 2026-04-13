using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FocusFlow.Models
{
    public class StudySession
    {
        public int Id { get; set; }

        [Required]
        public int SubjectId { get; set; }

        [ForeignKey("SubjectId")]
        public Subject? Subject { get; set; }

        [MaxLength(100)]
        public string SubjectName { get; set; } = string.Empty;

        [Range(0.5, 24.0)]
        public string UserId { get; set; } = string.Empty;
        public double Hours { get; set; }

        [DataType(DataType.Date)]
        public DateTime Date { get; set; } = DateTime.Today;
    }
}