using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SQLExerciser.Models.DB
{
    public class Judge
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int JudgeId { get; set; }

        [Required]
        public string AnswerQuery { get; set; }

        public string VerifyQuery { get; set; }

        public virtual DbDiagram Diagram { get; set; }
    }
}