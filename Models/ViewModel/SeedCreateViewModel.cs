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
        [Required]
        public int DiagramId { get; set; }

        [Required]
        public string SeedQuery { get; set; }
    }
}