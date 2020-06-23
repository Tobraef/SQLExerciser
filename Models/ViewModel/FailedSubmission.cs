using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using SQLExerciser.Models;
using SQLExerciser.Models.DB;

namespace SQLExerciser.Models.ViewModel
{
    public class FailedSubmission
    {
        public Exercise Exercise { get; set; }
        
        public string Query { get; set; }

        public string Result { get; set; }
    }
}