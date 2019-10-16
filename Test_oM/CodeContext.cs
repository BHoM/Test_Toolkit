using BH.oM.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.oM.Test
{
    public class CodeContext : IObject
    {
        public string Namespace { get; set; }
        public string Class { get; set; }
        public string Method { get; set; }
    }
}
