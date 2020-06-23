using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SQLExerciser.Models.DB
{
    public class DataSeed
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int DataSeedId { get; set; }

        [Required]
        public string SeedQuery { get; set; }

        public virtual DbDiagram Diagram { get; set; }

        public virtual ICollection<Judge> Judges { get; set; }
    }
}