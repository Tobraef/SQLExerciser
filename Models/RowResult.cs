using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SQLExerciser.Models
{
    public class RowResult
    {
        readonly List<int> ints = new List<int>();
        readonly List<DateTime> dates = new List<DateTime>();
        readonly List<string> strings = new List<string>();
        readonly List<double> doubles = new List<double>();

        public ICollection<int> Ints => ints;
        public ICollection<string> Strings => strings;
        public ICollection<DateTime> Dates => dates;
        public ICollection<double> Doubles => doubles;

        public override bool Equals(object obj)
        {
            var other = (RowResult)obj;
            return
                other.ints.SequenceEqual(ints) &&
                other.dates.SequenceEqual(dates) &&
                other.strings.SequenceEqual(strings) &&
                other.doubles.SequenceEqual(doubles);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public string Data =>
            string.Join(";",
                string.Join(", ", dates.Select(i => i.ToString())),
                string.Join(", ", ints.Select(i => i.ToString())),
                string.Join(", ", strings.Select(i => i.ToString())),
                string.Join(", ", doubles.Select(i => i.ToString())));
    }

}