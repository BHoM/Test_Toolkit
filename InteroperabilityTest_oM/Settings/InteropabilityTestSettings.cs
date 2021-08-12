using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using BH.oM.Base;

namespace BH.oM.Test.Interoperability.Settings
{
    [Description("All settings needed to run a PushPullCompare for a specific adapter.")]
    public class InteropabilityTestSettings : BHoMObject
    {
        [Description("The type of adapter to test.")]
        public virtual Type AdapterType { get; set; }

        [Description("The arguments required by the constructor of the adapter with the largest set of arguments.")]
        public virtual List<object> AdapterConstructorArguments { get; set; } = new List<object>();

        [Description("The types of objects to be tested")]
        public virtual List<Type> TestTypes { get; set; } = new List<Type>();

        [Description("Config to be used for the PushPullCompare method.")]
        public virtual PushPullCompareConfig PushPullConfig { get; set; }
    }
}
