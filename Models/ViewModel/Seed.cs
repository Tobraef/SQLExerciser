using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SQLExerciser.Models.ViewModel
{
    public class Seed
    {
        public class TableSeed
        {
            public string TableName { get; set; }
            public List<string> TableHeaders { get; set; }
            public List<List<string>> Rows { get; set; }
            
            private List<string> ReadSection(string subSection)
            {
                subSection = subSection.Replace("\'", "");
                var strings = subSection.Split(',');
                strings[0] = strings[0].Replace("(", "");
                strings[strings.Length - 1] = strings[strings.Length - 1].Replace(")", "");
                for (int i = 0; i < strings.Length; ++i)
                {
                    strings[i] = strings[i].Trim();
                }
                return strings.ToList();
            }

            private List<List<string>> ReadAllSections(string sections)
            {
                List<List<string>> toRet = new List<List<string>>();
                int startI = sections.IndexOf('(');
                int endI = sections.IndexOf(')');
                while (startI != -1 && endI != -1)
                {
                    toRet.Add(ReadSection(sections.Substring(startI, endI - startI)));
                    startI = sections.IndexOf('(', endI);
                    if (startI == -1) break;
                    endI = sections.IndexOf(')', startI);
                }
                return toRet;
            }

            public TableSeed(string fromQuery)
            {
                int startI = "INSERT INTO ".Length;
                int endI = fromQuery.IndexOf(' ', startI + 1);
                TableName = fromQuery.Substring(startI, endI - startI);
                startI = fromQuery.IndexOf('(');
                endI = fromQuery.IndexOf(')');
                TableHeaders = ReadSection(fromQuery.Substring(startI, endI - startI));
                Rows = ReadAllSections(fromQuery.Substring(endI + 1));
            }
        }

        public List<TableSeed> TableSeeds { get; set; }

        public int Id { get; set; }

        public int DiagramId { get; set; }

        public Seed(string seedQuery, int id, int diagramId)
        {
            DiagramId = diagramId;
            Id = id;
            TableSeeds = seedQuery.Split(';').TakeWhile(s => !string.IsNullOrEmpty(s)).Select(s => new TableSeed(s)).ToList();
        }
    }
}