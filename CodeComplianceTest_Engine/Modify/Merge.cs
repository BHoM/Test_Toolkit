/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2022, the respective contributors. All rights reserved.
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

using BH.Engine.Base;
using BH.oM.Test.CodeCompliance;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BH.oM.Test;
using BH.oM.Test.Results;

namespace BH.Engine.Test.CodeCompliance
{
    public static partial class Modify
    {
        public static TestResult Merge(this TestResult a, TestResult b)
        {
            if(a == null)
            {
                if (b != null)
                    return b;

                return null;
            }

            if(b == null)
            {
                if (a != null)
                    return a;

                return null;
            }

            TestResult merged = a.ShallowClone();
            if (merged.Information == null)
                merged.Information = new List<ITestInformation>();

            merged.Information.AddRange(b.Information);

            if (b.Status == TestStatus.Error || a.Status == TestStatus.Error)
                merged.Status = TestStatus.Error;
            else if (b.Status == TestStatus.Warning || a.Status == TestStatus.Warning)
                merged.Status = TestStatus.Warning;
            else
                merged.Status = TestStatus.Pass;

            return merged;
        }
    }
}



