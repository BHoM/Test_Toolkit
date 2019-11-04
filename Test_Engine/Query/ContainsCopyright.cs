/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2019, the respective contributors. All rights reserved.
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

using BH.oM.Test;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace BH.Engine.Test
{
    public static partial class Query
    {
        public static ComplianceResult ContainsCopyright(SyntaxTriviaList leadingTrivia, int year = -1)
        {
            bool checkAllYears = false;
            if (year == -1)
            {
                checkAllYears = true;
                year = 2018; //Start
            }

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
                Error e = Create.Error("Copyright message is not accurate at line " + 1, Create.Location("", Create.LineSpan(1, 2).ToSpan(l)), ErrorLevel.Error);
                return Create.ComplianceResult(ResultStatus.CriticalFail, new List<Error> { e });
            }

            if (!checkAllYears)
            {
                for (int x = 0; x < copyrightSplit.Length; x++)
                {
                    if (split[x].TrimEnd() != copyrightSplit[x].TrimEnd())
                    {
                        Error e = Create.Error("Copyright message is not accurate at line " + (x + 1), Create.Location("", Create.LineSpan(x + 1, x + 2).ToSpan(l)), ErrorLevel.Error);
                        return Create.ComplianceResult(ResultStatus.CriticalFail, new List<Error> { e });
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
                        Error e = Create.Error("Copyright message is not accurate at line " + (x + 1), Create.Location("", Create.LineSpan(x + 1, x + 2).ToSpan(l)), ErrorLevel.Error);
                        return Create.ComplianceResult(ResultStatus.CriticalFail, new List<Error> { e });
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
                    Error e = Create.Error("Copyright message is not accurate at line 3", Create.Location("", Create.LineSpan(3, 4).ToSpan(l)), ErrorLevel.Error);
                    return Create.ComplianceResult(ResultStatus.CriticalFail, new List<Error> { e });
                }
            }

            return Create.ComplianceResult( ResultStatus.Pass);
        }
    }
}
