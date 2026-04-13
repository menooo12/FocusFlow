namespace FocusFlow.Models
{
    public class SubjectProgressViewModel
    {
        public Subject Subject { get; set; } = null!;
        public double LoggedHours { get; set; }  // from StudySessions only
        public double ProgressPct { get; set; }  // 0–100, clamped
    }
}