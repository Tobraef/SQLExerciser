using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Data.Sql;
using System.Data.SqlClient;

namespace SQLExerciser.Models
{
    public class SqlCleanupException : Exception { public SqlCleanupException(string t) : base(t) { } }

    public interface IQueryExecutor : IDisposable
    {
        void ExecuteSetup(string query);

        int ExecuteCRUD(string query);

        IEnumerable<RowResult> ExecuteQuery(string query);

        IEnumerable<IEnumerable<RowResult>> ExecuteWithRollback(IEnumerable<string> setupQueries, IEnumerable<string> selectQueries);
    }

    public class QueryExecutor : IQueryExecutor
    {
        const string rollbackTransactionName = "rollback_title";

        public IEnumerable<IEnumerable<RowResult>> ExecuteWithRollback(IEnumerable<string> setupQueries, IEnumerable<string> selectQueries)
        {
            SqlConnection connection;
            List<List<RowResult>> rowsToSelect = new List<List<RowResult>>();

            using (connection = new SqlConnection(Storage.ConnectionString))
            {
                connection.Open();
                using (SqlCommand cmd = new SqlCommand($"BEGIN TRANSACTION {rollbackTransactionName};" , connection))
                {
                    cmd.ExecuteNonQuery();
                }
                foreach (var setup in setupQueries)
                {
                    NonQuery(setup);
                }
                if (selectQueries != null)
                foreach (var query in selectQueries)
                {
                    Query(query);
                }
                using (SqlCommand cmd = new SqlCommand($"ROLLBACK TRANSACTION {rollbackTransactionName};", connection))
                {
                    cmd.ExecuteNonQuery();
                }
            }
            return rowsToSelect;

            void NonQuery(string query)
            {
                using (SqlCommand cmd = new SqlCommand(query, connection))
                    cmd.ExecuteNonQuery();
            }

            void Query(string query)
            {
                var rows = new List<RowResult>();
                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        RowResult row = new RowResult();
                        for (int i = 0; i < reader.FieldCount; ++i)
                        {
                            AddOne(row, reader, i);
                        }
                        rows.Add(row);
                    }
                    reader.Close();
                }
                rowsToSelect.Add(rows);
            }
        }

        void DropTables(IEnumerable<string> tableNames)
        {
            bool notCleared = true;
            while (notCleared)
            {
                notCleared = false;
                foreach (var t in tableNames)
                {
                    try
                    {
                        Execute("DROP TABLE " + t);
                    }
                    catch (SqlException e)
                    {
                        if (e.Message.Contains("FOREIGN KEY constraint"))
                        {
                            notCleared = true;
                        }
                        else if (e.Message.Contains("Cannot drop the table"))
                        {
                            continue;
                        }
                        else
                            throw;
                    }
                }
            }
        }

        void CleanupLeftovers()
        {
            var leftOvers = new List<string>();
            Execute(@"SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE'", r =>
            {
                while (r.Read())
                {
                    leftOvers.Add(r.GetString(0));
                }
            });
            DropTables(leftOvers);
        }

        public void Dispose()
        {
            CleanupLeftovers();
        }

        ~QueryExecutor() => Dispose();

        void Execute(string query, Action<SqlDataReader> readerExecutor)
        {
            using (SqlConnection connection = new SqlConnection(Storage.ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    connection.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        readerExecutor(reader);
                    }
                }
            }
        }

        void Execute(string query)
        {
            using (SqlConnection connection = new SqlConnection(Storage.ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    connection.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void ExecuteSetup(string query)
        {
            Execute(query);
        }

        public int ExecuteCRUD(string query)
        {
            int result = 1;
            void Read(SqlDataReader reader)
            {
                if (reader.Read())
                {
                    result = reader.GetInt32(0);
                }
            }
            Execute(query, Read);
            return result;
        }

        void AddOne(RowResult result, SqlDataReader reader, int i)
        {
            var t = reader.GetFieldType(i);
            if (t == typeof(int))
            {
                result.Ints.Add(reader.GetInt32(i));
            } else
            if (t == typeof(string))
            {
                result.Strings.Add(reader.GetString(i));
            } else
            if (t == typeof(double))
            {
                result.Doubles.Add(reader.GetDouble(i));
            } else
            if (t == typeof(DateTime))
            {
                result.Dates.Add(reader.GetDateTime(i));
            } else
            {
                result.Strings.Add(t.Name);
            }
        }

        public IEnumerable<RowResult> ExecuteQuery(string query)
        {
            List<RowResult> results = new List<RowResult>();
            void Read(SqlDataReader reader)
            {
                while (reader.Read())
                {
                    RowResult result = new RowResult();
                    for (int i = 0; i < reader.FieldCount; ++i)
                    {
                        AddOne(result, reader, i);
                    }
                    results.Add(result);
                }
            }
            Execute(query, Read);
            return results;
        }

        public QueryExecutor()
        {
            CleanupLeftovers();
        }
    }
}