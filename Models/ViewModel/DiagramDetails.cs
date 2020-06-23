using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SQLExerciser.Models
{
    public class DiagramDetails
    {
        public int DbDiagramId { get; set; }
        public string Name { get; set; }
        public byte[] Diagram { get; set; }
        
        public List<DB.Exercise> Exercises { get; set; }
    }
}