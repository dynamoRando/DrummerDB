using Drummersoft.DrummerDB.Core.Structures;
using Drummersoft.DrummerDB.Core.Structures.Abstract;
using System.Collections.Generic;
using System.Linq;

namespace Drummersoft.DrummerDB.Core.Tests.Mocks
{
    internal class MockDbFile
    {
        public TreeAddress Address { get; set; }
        public List<UserDataPage> Pages { get; set; }

        public MockDbFile()
        {
            Pages = new List<UserDataPage>();
        }

        public uint[] PageIds()
        {
            return Pages.Select(p => p.PageId()).ToArray();
        }
    }
}
