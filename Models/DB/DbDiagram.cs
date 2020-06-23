using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SQLExerciser.Models.DB
{
    public class DbDiagram
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int DbDiagramId { get; set; }

        [Required]
        public string CreationQuery { get; set; }

        [Required]
        public string Name { get; set; }

        public byte[] Diagram { get; set; }
    }
}