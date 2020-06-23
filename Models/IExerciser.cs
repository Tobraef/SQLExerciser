using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;

using SQLExerciser.Models.DB;

using System.Data.SqlClient;

namespace SQLExerciser.Models
{
    public interface IExerciser
    {
        Task<KeyValuePair<bool, string>> ExecuteExercise(Exercise exercise, string query, IEnumerable<string> setup);
    }

    public class Exerciser : IExerciser
    {
        const string acceptString = "ACCEPT";
        readonly IQueryExecutor _executor;

        async Task<string> PerformAndRollback(IEnumerable<string> setupQueries, string selectQuery)
        {
            try
            {
                var result = await Task.Run(() => _executor.ExecuteWithRollback(
                    setupQueries, new List<string> { selectQuery }));
                return string.Concat(result.Single().Select(r => r.Data));
            }
            catch (Exception ex)
            {
                return "Error in query: " + ex.Message;
            }
        }

        List<string> GetBaseSetup(IEnumerable<string> setup)
        {
            return setup.Select(s => s.EndsWith(";") ? s : s + ";").ToList();
        }

        async Task<string> SolveDDLExercise(Exercise exercise, string userInput, IEnumerable<string> setup)
        {
            var setupQ = GetBaseSetup(setup);
            setupQ.Add(exercise.Judge.AnswerQuery);
            var correctResult = await PerformAndRollback(setupQ, exercise.Judge.VerifyQuery);
            setupQ.RemoveAt(setupQ.Count - 1);
            setupQ.Add(userInput);
            var userResult = await PerformAndRollback(setupQ, exercise.Judge.VerifyQuery);
            if (correctResult != userResult)
            {
                return "Verification after solution query returned: " + correctResult + " and yours: " + userResult + " and they don't match";
            }
            return acceptString;
        }

        async Task<string> SolveSelectExercise(Exercise exercise, string userInput, IEnumerable<string> setup)
        {
            try
            {
                var result = await Task.Run(() => _executor.ExecuteWithRollback(GetBaseSetup(setup),
                    new List<string> { exercise.Judge.AnswerQuery, userInput }));
                var judgeOutput = result.First();
                var userOutput = result.Last();
                if (!userOutput.SequenceEqual(judgeOutput))
                {
                    return "Retrieved result doesn't match judge's result:\n" +
                        "Your output: " + userOutput.Select(r => r.Data) + "\n" +
                        "Judge's output: " + judgeOutput.Select(r => r.Data);
                }
            }
            catch (Exception ex)
            {
                return "Error in query: " + ex.Message;
            }
            return acceptString;
        }

        public async Task<KeyValuePair<bool, string>> ExecuteExercise(Exercise exercise, string query, IEnumerable<string> setup)
        {
            string output;
            if (string.IsNullOrEmpty(exercise.Judge.VerifyQuery))
            {
                output = await SolveSelectExercise(exercise, query, setup);
            }
            else
            {
                output = await SolveDDLExercise(exercise, query, setup);
            }
            _executor.Dispose();
            return new KeyValuePair<bool, string>(output.Equals(acceptString), output);
        }

        public Exerciser(IQueryExecutor executor)
        {
            _executor = executor;
        }
    }
}