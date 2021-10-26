using Drummersoft.DrummerDB.Core.Structures.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drummersoft.DrummerDB.Core.Structures
{
    internal class ResultsetLayout
    {
        public List<IResultsetSource> Columns { get; set; }

        public ResultsetLayout()
        {
            Columns = new List<IResultsetSource>();
        }
    }
}
