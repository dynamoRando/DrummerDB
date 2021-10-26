using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drummersoft.DrummerDB.Core.QueryTransaction.Interface
{
    /// <summary>
    /// Represents a source for a column in an UPDATE statement.
    /// This could either be a constant value i.e NAME = 'FOO',
    /// or another column elsewhere in the UPDATE statement,
    /// i.e. in a JOIN, such as NAME = joinedTable.NAME
    /// </summary>
    interface IUpdateColumnSource
    {
    }
}
