/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2025, the respective contributors. All rights reserved.
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

using BH.oM.Test.CodeCompliance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using BH.oM.Test;
using BH.oM.Test.Results;

namespace BH.Engine.Test.CodeCompliance
{
    public static partial class Query
    {
        public static TestResult HasValidCopyright(this SyntaxTriviaList leadingTrivia, int year = -1, string filePath = "")
        {
            if (leadingTrivia == null)
                return Create.TestResult(TestStatus.Pass);

            bool checkAllYears = false;
            if (year == -1)
            {
                checkAllYears = true;
                year = 2018; //Start
            }

            string documentationLink = "HasValidCopyright";

            int maxYear = DateTime.Now.Year; //Max

            string copyrightStatement = $@"/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - {year}, the respective contributors. All rights reserved.
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
 */";

            string l = leadingTrivia.ToString();
            l = l.Replace('\r', ' ');
            copyrightStatement = copyrightStatement.Replace('\r', ' ');

            string[] split = l.Split('\n');
            string[] copyrightSplit = copyrightStatement.Split('\n');

            if(split.Length < copyrightSplit.Length)
            {
                Error e = Create.Error("Copyright message is not accurate at line " + 1, Create.Location(filePath, Create.LineSpan(1,2)), documentationLink, TestStatus.Error);
                return Create.TestResult(TestStatus.Error, new List<Error> { e });
            }

            if (!checkAllYears)
            {
                for (int x = 0; x < copyrightSplit.Length; x++)
                {
                    if (split[x].TrimEnd() != copyrightSplit[x].TrimEnd())
                    {
                        Error e = Create.Error("Copyright message is not accurate at line " + (x + 1), Create.Location(filePath, Create.LineSpan(x + 1, x + 2)), documentationLink, TestStatus.Error);
                        return Create.TestResult(TestStatus.Error, new List<Error> { e });
                    }
                }
            }
            else
            {
                List<int> availableYears = new List<int>();
                for (int x = 2018; x <= maxYear; x++)
                    availableYears.Add(x);

                for (int x = 0; x < copyrightSplit.Length; x++)
                {
                    if (x == 2) continue; //Skip the year line

                    if (split[x].TrimEnd() != copyrightSplit[x].TrimEnd())
                    {
                        Error e = Create.Error("Copyright message is not accurate at line " + (x + 1), Create.Location(filePath, Create.LineSpan(x + 1, x + 2)), documentationLink, TestStatus.Error);
                        return Create.TestResult(TestStatus.Error, new List<Error> { e });
                    }
                }

                bool validOnOneYear = false;
                foreach (int a in availableYears)
                {
                    copyrightSplit[2] = $" * Copyright (c) 2015 - {a}, the respective contributors. All rights reserved.";
                    if (split[2].TrimEnd() == copyrightSplit[2].TrimEnd())
                        validOnOneYear = true;
                }

                if (!validOnOneYear)
                {
                    Error e = Create.Error("Copyright message is not accurate at line 3", Create.Location(filePath, Create.LineSpan(3, 4)), documentationLink, TestStatus.Error);
                    return Create.TestResult(TestStatus.Error, new List<Error> { e });
                }
            }

            return Create.TestResult(TestStatus.Pass);
        }
    }
}






