using System.ComponentModel.DataAnnotations;

namespace BeFit.Models
{
    public class TrainingSessionCreateDto
    {
        [Required]
        [DataType(DataType.DateTime)]
        [Display(Name = "Czas rozpoczęcia", Description = "Data i godzina rozpoczęcia treningu")]
        public DateTime StartTime { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        [Display(Name = "Czas zakończenia", Description = "Data i godzina zakończenia treningu")]
        public DateTime EndTime { get; set; }

    }
}

