using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drummersoft.DrummerDB.Core.Structures.Interface
{
    internal interface ITable
    {
        string Name { get; }
        TreeAddress Address { get; }
        void BringTreeOnline();
    }
}
