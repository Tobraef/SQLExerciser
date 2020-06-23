using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web;

namespace SQLExerciser.Tests.Framework
{
    class SessionFake : HttpSessionStateBase
    {
        readonly Dictionary<string, object> _impl = new Dictionary<string, object>();

        public override void Remove(string name)
        {
            _impl.Remove(name);
        }

        public override object this[string name] { get => _impl[name]; set => _impl[name] = value; }
    }
}
