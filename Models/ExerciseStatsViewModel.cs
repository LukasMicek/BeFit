using System.ComponentModel.DataAnnotations;

namespace BeFit.Models
{
    public class ExerciseStatsViewModel
    {
        [Display(Name = "Ćwiczenie")]
        public string ExerciseTypeName { get; set; } = string.Empty;

        [Display(Name = "Ilość wykonań")]
        public int TimesPerformed { get; set; }

        [Display(Name = "Powtórzenia")]
        public int TotalRepetitions { get; set; }

        [Display(Name = "Średnia waga(kg)")]
        public double AverageWeight { get; set; }

        [Display(Name = "Waga maksymalna(kg)")]
        public double MaxWeight { get; set; }
    }
}

