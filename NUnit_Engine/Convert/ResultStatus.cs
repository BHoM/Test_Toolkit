using BH.oM.Test;
using System;
using System.Collections.Generic;
using System.Text;

using System.ComponentModel;
using BH.oM.Base.Attributes;

namespace BH.Engine.Test.NUnit
{
    public static partial class Convert
    {
        [Description("Convert an NUnit result string to a BHoM Test Status. 'Passed' converts to TestStatus.Pass and 'Failed' converts to TestStatus.Error. Any other string input returns TestStatus.Error.")]
        [Input("resultStatus", "The NUnit result string, typically either 'Passed' or 'Failed'.")]
        [Output("testStatus", "The BHoM TestStatus enum value converted from the NUnit result string.")]
        public static TestStatus ToTestStatus(string resultStatus)
        {
            if (resultStatus == "Failed")
                return TestStatus.Error;
            else if (resultStatus == "Passed")
                return TestStatus.Pass;

            return TestStatus.Error; //Default - something probably went wrong so we'd want to investigate
        }
    }
}
