using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using SQLExerciser.Models.DB;

namespace SQLExerciser.Models.ViewModel
{
    public class ExerciseWSubmissions
    {
        public Exercise Exercise { get; set; }

        public IEnumerable<ExerciseStatus> Statuses { get; set; }
    }
}