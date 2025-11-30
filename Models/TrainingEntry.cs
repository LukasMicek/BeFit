using System.ComponentModel.DataAnnotations;

namespace BeFit.Models
{
    public class TrainingEntry
    {
        public int Id { get; set; }

        // klucz do TrainingSession
        [Required]
        [Display(Name = "Sesja Treningowa")]
        public int TrainingSessionId { get; set; }
        public TrainingSession? TrainingSession { get; set; }

        // kliusz do ExerciseType
        [Required]
        [Display(Name = "Typ ćwiczenia")]
        public int ExerciseTypeId { get; set; }
        public ExerciseType? ExerciseType { get; set; }

        // Parametry
        [Range(0, double.MaxValue)]
        [Display(Name = "Waga (kg)", Description = "Waga użyta do wykonania ćwiczenia")]
        public double Weight { get; set; }

        [Range(1, int.MaxValue)]
        [Display(Name = "Serie", Description = "Ilość serii")]
        public int Sets { get; set; }

        [Range(1, int.MaxValue)]
        [Display(Name = "Powtórzenia", Description = "Ilość powtórzeń")]
        public int Repetitions { get; set; }

        [Required]
        [Display(Name = "Użytkownik")]
        public string UserId { get; set; } = string.Empty;
        public AppUser? User { get; set; }
    }
}

