using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SQLExerciser.Models
{
    public class SeedCreateViewModel
    {
        public int? SeedId { get; set; }

        [Required]
        public int DiagramId { get; set; }

        [Required]
        [RegularExpression("INSERT INTO .*? \\(.*?\\) VALUES \\(.*?;", 
            ErrorMessage = "The query must begin with INSERT INTO, then contain column names, followed by VALUES ...")]
        public string SeedQuery { get; set; }
    }
}