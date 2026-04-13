using System.ComponentModel.DataAnnotations;

namespace FocusFlow.Models
{
    public class FocusTask
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        public bool IsComplete { get; set; } = false;

        public DateTime DueDate { get; set; } = DateTime.Today;

        public string? Notes { get; set; }
    }
}