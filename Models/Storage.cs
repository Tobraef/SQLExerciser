using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SQLExerciser.Models
{
    public interface IStorage
    {
        HttpSessionStateBase Session { set; }

        bool HoldsValue<T>();

        T GetValue<T>();

        void StoreValue<T>(T value);

        void ClearValue<T>();

        void ClearStorage();
    }

    public class Storage : IStorage
    {
        HttpSessionStateBase _session;
        public static string ConnectionString => string.Format("Data Source=(localdb)\\MSSQLLocalDB;Integrated Security=SSPI");
        public const string WebsiteName = "www.SqlExerciser.com";

        public HttpSessionStateBase Session {
            set
            {
                _session = value;
            }
        }

        private string SaveString<T>() => typeof(T).FullName;

        public bool HoldsValue<T>()
        {
            return _session[SaveString<T>()] != null;
        }

        public T GetValue<T>()
        {
            return (T)_session[SaveString<T>()];
        }

        public void StoreValue<T>(T value)
        {
            _session[SaveString<T>()] = value;
        }

        public void ClearStorage()
        {
            _session.Clear();
        }

        public void ClearValue<T>()
        {
            _session.Remove(SaveString<T>());
        }
    }
}