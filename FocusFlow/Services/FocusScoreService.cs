using FocusFlow.Models;

namespace FocusFlow.Services
{
    public class FocusScoreResult
    {
        public double TotalHours { get; set; }
        public double TargetHours { get; set; }
        public double ScorePercentage { get; set; }
        public string FeedbackMessage { get; set; } = string.Empty;
        public string FeedbackLevel { get; set; } = string.Empty; // "excellent" | "good" | "poor"
        public string BadgeColor { get; set; } = string.Empty; // Bootstrap color name
    }

    public class FocusScoreService
    {
        // ── 1. TOTAL HOURS ───────────────────────────────────────────────────
        // Sums the Hours property across every StudySession in the list.
        // Returns 0.0 safely when the list is null or empty.
        public double CalculateTotalHours(List<StudySession> sessions)
        {
            if (sessions == null || sessions.Count == 0)
                return 0.0;

            return sessions.Sum(s => s.Hours);
        }

        // ── 2. SUBJECT HOURS ────────────────────────────────────────────────
        // Breaks down total hours per subject name.
        // Returns a Dictionary<subjectName, totalHours> for use in charts.
        public Dictionary<string, double> CalculateHoursPerSubject(
            List<StudySession> sessions)
        {
            if (sessions == null || sessions.Count == 0)
                return new Dictionary<string, double>();

            return sessions
                .GroupBy(s => s.SubjectName)
                .ToDictionary(
                    group => group.Key,
                    group => group.Sum(s => s.Hours)
                );
        }

        // ── 3. WEEKLY HOURS ──────────────────────────────────────────────────
        // Filters sessions to only those within the current calendar week
        // (Monday → today), then sums their hours.
        public double CalculateWeeklyHours(List<StudySession> sessions)
        {
            if (sessions == null || sessions.Count == 0)
                return 0.0;

            // Find the most recent Monday at midnight
            var today = DateTime.Today;
            var dayOfWeek = (int)today.DayOfWeek;              // Sun=0 … Sat=6
            var daysBack = dayOfWeek == 0 ? 6 : dayOfWeek - 1; // shift to Mon=0
            var weekStart = today.AddDays(-daysBack);

            return sessions
                .Where(s => s.Date >= weekStart && s.Date <= today)
                .Sum(s => s.Hours);
        }

        // ── 4. FOCUS SCORE ───────────────────────────────────────────────────
        // Focus Score = (actual weekly hours / target weekly hours) × 100
        // Clamped to [0, 100] so going over target gives exactly 100, not 120.
        public double CalculateFocusScore(double actualHours, double targetHours)
        {
            if (targetHours <= 0)
                return 0.0;

            var raw = (actualHours / targetHours) * 100.0;
            return Math.Min(Math.Round(raw, 1), 100.0);
        }

        // ── 5. FEEDBACK MESSAGE ──────────────────────────────────────────────
        // Rule-based text returned as a plain string.
        // Controller stores this in ViewBag; service stays UI-unaware.
        public string GetFeedbackMessage(double scorePercentage)
        {
            if (scorePercentage >= 80)
                return "Excellent! You're hitting your study goals. Keep it up!";

            if (scorePercentage >= 50)
                return "Good progress! You're on track — push a little harder " +
                       "to reach your weekly target.";

            return "Needs improvement. Try to schedule more focused sessions " +
                   "this week to reach your goal.";
        }

        // ── 6. FEEDBACK LEVEL ────────────────────────────────────────────────
        // Returns a short token used for Bootstrap color selection in the view.
        public string GetFeedbackLevel(double scorePercentage)
        {
            if (scorePercentage >= 80) return "excellent";
            if (scorePercentage >= 50) return "good";
            return "poor";
        }

        // ── 7. BADGE COLOR ───────────────────────────────────────────────────
        // Maps level → Bootstrap contextual color name.
        public string GetBadgeColor(string level) => level switch
        {
            "excellent" => "success",
            "good" => "warning",
            _ => "danger"
        };

        // ── 8. FULL RESULT (convenience method) ─────────────────────────────
        // Runs all calculations in one call and returns a single result object.
        // Controllers call this instead of calling each method individually.
        public FocusScoreResult GetFullResult(
            List<StudySession> sessions,
            double targetWeeklyHours)
        {
            var weekly = CalculateWeeklyHours(sessions);
            var total = CalculateTotalHours(sessions);
            var score = CalculateFocusScore(weekly, targetWeeklyHours);
            var message = GetFeedbackMessage(score);
            var level = GetFeedbackLevel(score);
            var color = GetBadgeColor(level);

            return new FocusScoreResult
            {
                TotalHours = total,
                TargetHours = targetWeeklyHours,
                ScorePercentage = score,
                FeedbackMessage = message,
                FeedbackLevel = level,
                BadgeColor = color
            };
        }
    }
}