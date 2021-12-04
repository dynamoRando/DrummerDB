using Drummersoft.DrummerDB.Core.Structures.Interface;
using System.Collections.Generic;

namespace Drummersoft.DrummerDB.Core.Structures
{
    internal class ResultsetLayout
    {
        public List<IResultsetSource> Columns { get; set; }

        public ResultsetLayout()
        {
            Columns = new List<IResultsetSource>();
        }

        public void AddSource(IResultsetSource source)
        {
            Columns.Add(source);
        }
    }
}
