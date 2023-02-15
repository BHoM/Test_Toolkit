using BH.oM.Test;
using System;
using System.Collections.Generic;
using System.Text;

namespace BH.Engine.Test.NUnit
{
    public static partial class Convert
    {
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
