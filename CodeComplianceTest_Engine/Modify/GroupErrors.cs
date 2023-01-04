/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2023, the respective contributors. All rights reserved.
 *
 * Each contributor holds copyright over their respective contributions.
 * The project versioning (Git) records all such contribution source information.
 *                                           
 *                                                                              
 * The BHoM is free software: you can redistribute it and/or modify         
 * it under the terms of the GNU Lesser General Public License as published by  
 * the Free Software Foundation, either version 3.0 of the License, or          
 * (at your option) any later version.                                          
 *                                                                              
 * The BHoM is distributed in the hope that it will be useful,              
 * but WITHOUT ANY WARRANTY; without even the implied warranty of               
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the                 
 * GNU Lesser General Public License for more details.                          
 *                                                                            
 * You should have received a copy of the GNU Lesser General Public License     
 * along with this code. If not, see <https://www.gnu.org/licenses/lgpl-3.0.html>.      
 */

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
            if (errors == null)
                return new List<Error>();

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


