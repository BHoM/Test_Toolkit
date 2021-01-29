using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BH.oM.Test.CodeCompliance;
using BH.oM.Test;

namespace BH.Engine.Test.CodeCompliance
{
    public static partial class Modify
    {
        public static List<Error> GroupErrors(this List<Error> errors, string filePath)
        {
            List<Error> groupedErrors = new List<Error>();

            Dictionary<int, List<Error>> errorsByLine = new Dictionary<int, List<Error>>();

            foreach (Error e in errors)
            {
                if (!errorsByLine.ContainsKey(e.Location.Line.Start.Line))
                    errorsByLine.Add(e.Location.Line.Start.Line, new List<Error>());

                errorsByLine[e.Location.Line.Start.Line].Add(e);
            }

            foreach (KeyValuePair<int, List<Error>> kvp in errorsByLine)
            {
                TestStatus level = TestStatus.Warning;
                string message = "";
                Location loc = null;

                foreach (Error e in kvp.Value)
                {
                    level = e.Status == TestStatus.Error ? e.Status : level; //Max out at error
                    loc = e.Location; //Any location will do
                    message += e.FullMessage();
                }

                loc.FilePath = filePath;

                Error newError = new Error()
                {
                    Status = level,
                    Message = message,
                    Location = loc,
                };

                groupedErrors.Add(newError);
            }

            return groupedErrors;
        }
    }
}
