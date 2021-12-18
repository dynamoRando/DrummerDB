using Drummersoft.DrummerDB.Core.QueryTransaction.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drummersoft.DrummerDB.Core.QueryTransaction
{
    internal record struct CoopActionOptionParticipant : ICoopActionPlanOption
    {
        public string ParticipantAlias { get; set; }
        public string Text { get; set; }
    }
}
