using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SQLExerciser.Models.DB
{
    public class ExerciseStatus
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public virtual User User { get; set; }

        public virtual Exercise Exercise { get; set; }

        public string Solution { get; set; }

        public string Resolution { get; set; }

        public DateTime SubmittedOn { get; set; }

        public bool Accepted { get; set; }
    }
}