using BH.oM.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.oM.Test
{
    public class Error : BHoMObject
    {
        public ErrorLevel Level { get; set; }
        public string Message { get; set; }
        public Span Location { get; set; }
    }
}
