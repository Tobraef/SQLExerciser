using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SQLExerciser.Models.DB
{
    public class Exercise
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ExerciseId { get; set; }

        [Range(1, 100)]
        [Display(Description = "Difficulty of the exercise ranging from 1 to 100")]
        public int Difficulty { get; set; }

        [Required]
        [MaxLength(200, ErrorMessage = "The title cannot be longer than 200 characters")]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        public virtual Judge Judge { get; set; }

        public virtual ICollection<ExerciseStatus> ExerciseStatuses { get; set; }
    }
}