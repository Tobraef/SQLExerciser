using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Collections;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace SQLExerciser.Tests.Framework
{
    internal class FakeDbSet<T> : IDbSet<T> where T : class
    {
        readonly HashSet<T> _data;
        readonly IQueryable _query;

        public FakeDbSet()
        {
            _data = new HashSet<T>();
            _query = _data.AsQueryable();
        }

        public virtual T Find(params object[] keyValues)
        {
            var idProperty = typeof(T).GetProperties().First(p => p.PropertyType == typeof(int));
            return _data.First(i => (int)idProperty.GetValue(i) == (int)keyValues.First());
        }

        public T Add(T item)
        {
            _data.Add(item);
            return item;
        }

        public T Remove(T item)
        {
            _data.Remove(item);
            return item;
        }

        public T Attach(T item)
        {
            _data.Add(item);
            return item;
        }

        public void Detach(T item)
        {
            _data.Remove(item);
        }

        Type IQueryable.ElementType
        {
            get { return _query.ElementType; }
        }

        Expression IQueryable.Expression
        {
            get { return _query.Expression; }
        }

        IQueryProvider IQueryable.Provider
        {
            get { return _query.Provider; }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _data.GetEnumerator();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return _data.GetEnumerator();
        }


        public TDerivedEntity Create<TDerivedEntity>() where TDerivedEntity : class, T
        {
            return Activator.CreateInstance<TDerivedEntity>();
        }

        public T Create()
        {
            return Activator.CreateInstance<T>();
        }

        public IEnumerator<T> GetEnumerator() => _data.GetEnumerator();

        public ObservableCollection<T> Local
        {
            get
            {

                return new ObservableCollection<T>(_data);
            }
        }

        ObservableCollection<T> IDbSet<T>.Local => throw new NotImplementedException();

        public Expression Expression => throw new NotImplementedException();
    }
}
