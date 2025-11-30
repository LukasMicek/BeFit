using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BeFit.Models
{
    public class ExerciseType
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        [Display(Name = "Nazwa Ćwiczenia", Description = "Nazwa typu ćwiczenia, np. Wyciskanie na ławkce")]
        public string Name { get; set; } = string.Empty;
    }
}
