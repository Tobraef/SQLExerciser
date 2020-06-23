using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace SQLExerciser.Models
{
    public class CreateExercise
    {
        [Range(1, 100)]
        [Display(Description = "Difficulty of the exercise ranging from 1 to 100")]
        public int Difficulty { get; set; }

        [Required]
        [MaxLength(100)]
        [Display(Description = "Title of the exercise")]
        public string Title { get; set; }
        
        [Required]
        [Display(Description = "Full description of the exercise")]
        public string Description { get; set; }

        [Required]
        [Display(Description = "Solution query")]
        public string SolutionQuery { get; set; }

        public const string VerificationQueryDescription = "It is useful only for CRUD exercises. If not supplied the result from solution query " +
            "will be compared against user's input. Otherwise user's input will be applied parallel with solution query. Then verification query " +
            "will be issued against both versions of database and should yield equal results.\n" +
            "Example:\n" +
            "Solution query = INSERT INTO [Students] (id, name) VALUES (5, \'Joe\'); , then you can verify it with:\n" +
            "Verification query = SELECT id, name FROM [Students] WHERE id = 5 AND name LIKE \'Joe\';\n" +
            "Now user's input will be verified with the verify query, rather than solution query, as INSERT would return 1, which doesn't verify the input.";

        [Display(Description = "Verification query")]
        public string VerificationQuery { get; set; }

        public int Diagram { get; set; }
    }
}