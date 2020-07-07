using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Threading.Tasks;
using System.Data.SqlClient;

namespace SQLExerciser.Models
{
    public interface IQueryTester
    {
        Task<string> TestTableSetup(string setup);

        Task<string> TestCRUD(string setup, string query);
        Task<string> TestCRUD(string setup, IEnumerable<string> query);

        Task<string> TestSelect(string setup, IEnumerable<string> seeds, string query);
    }

    public class QueryTester : IQueryTester
    {
        readonly IQueryExecutor _executor;

        public async Task<string> TestTableSetup(string setup)
        {
            try
            {
                if (string.IsNullOrEmpty(setup))
                {
                    throw new Exception("Query cannot be empty");
                }
                await Task.Run(() => _executor.ExecuteWithRollback(new List<string>
                {
                    setup
                }, null));
            }
            catch (Exception ex)
            {
                return "ERROR: " + ex.Message;
            }
            return string.Empty;
        }

        public async Task<string> TestCRUD(string setup, string query)
        {
            try
            {
                if (string.IsNullOrEmpty(query))
                {
                    throw new Exception("Query cannot be empty");
                }
                await Task.Run(() => _executor.ExecuteWithRollback(new List<string>
                {
                    setup, query
                }, null));
                return string.Empty;
            }
            catch (Exception ex)
            {
                return "ERROR: " + ex.Message;
            }
        }

        public async Task<string> TestCRUD(string setup, IEnumerable<string> queries)
        {
            try
            {
                if (string.IsNullOrEmpty(queries.Last()))
                {
                    throw new Exception("Query cannot be empty");
                }
                var setups = new List<string> { setup };
                setups.AddRange(queries);
                await Task.Run(() => _executor.ExecuteWithRollback(setups, null));
                return string.Empty;
            }
            catch (Exception ex)
            {
                return "ERROR: " + ex.Message;
            }
        }

        public async Task<string> TestSelect(string setup, IEnumerable<string> seeds, string query)
        {
            try
            {
                if (seeds.Any(s => string.IsNullOrEmpty(s)) || string.IsNullOrEmpty(setup) || string.IsNullOrEmpty(query))
                {
                    throw new Exception("Query cannot be empty");
                }
                var setups = new List<string> { setup };
                setups.AddRange(seeds);
                var result = await Task.Run(() => _executor.ExecuteWithRollback(setups, new List<string> { query }));
                return string.Concat(result.Single().Select(s => s.Data));
            }
            catch (Exception ex)
            {
                return "ERROR: " + ex.Message;
            }
        }

        public QueryTester(IQueryExecutor executor)
        {
            _executor = executor;
        }
    }
}