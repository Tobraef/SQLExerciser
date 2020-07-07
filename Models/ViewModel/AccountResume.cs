using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using SQLExerciser.Models.DB;

namespace SQLExerciser.Models.ViewModel
{
    public class AccountResume
    {
        public string AccountName { get; set; }

        public IEnumerable<ExerciseStatus> Submissions { get; set; }

        public string Role { get; set; }

        public DateTime Created { get; set; }

        public double AverageOn10Last =>
            Submissions.OrderByDescending(s => s.SubmittedOn).Take(10).Average(s => s.Exercise.Difficulty);

        public IEnumerable<KeyValuePair<int, string>> NotSuccessfulSubmissions =>
            Submissions.GroupBy(s => s.Exercise).Where(kvp => kvp.All(s => !s.Accepted))
            .Select(kvp => new KeyValuePair<int, string>(kvp.Key.ExerciseId, kvp.Key.Title));
    }
}