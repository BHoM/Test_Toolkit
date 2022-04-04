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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BH.oM.Test.CodeCompliance;
using BH.oM.Base.Attributes;

using System.Reflection;

using BH.Engine.Test;

using BH.oM.Test;
using BH.oM.Test.Results;

namespace BH.Engine.Test.CodeCompliance.DynamicChecks
{
    public static partial class Query
    {
        public static TestResult ImplementsBaseOption(this MethodInfo method)
        {
            if(method == null || !method.Name.StartsWith("I"))
                return Create.TestResult(TestStatus.Pass); //Method is not an Interface method

            ParameterInfo firstParam = method.GetParameters()[0];
            string methodName = method.Name.Substring(1); //Trim the I

            string ns = method.DeclaringType.FullName;

            List<MethodBase> mi = BH.Engine.Base.Query.AllMethodList().Where(x => x.DeclaringType.FullName == ns && x.Name == methodName).ToList();
            if(mi.Count == 0 || (mi.Where(x => x.GetParameters()[0].ParameterType == firstParam.ParameterType).FirstOrDefault() == null && mi.Where(x => x.GetParameters()[0].ParameterType == typeof(object)).FirstOrDefault() == null))
            {
                //No extension method exists
                Error e = new Error();
                e.Status = TestStatus.Error;
                e.Location = Create.Location(method.Signature(), Create.LineSpan(1, 1));
                e.DocumentationLink = ""; //ToDo - provide this
                e.Message = "Interface Methods dispatching as dynamic need to provide base methods which can take the original object as a fall back method to prevent StackOverflow issues.";
                return Create.TestResult(TestStatus.Error, new List<Error> { e });
            }

            return Create.TestResult(TestStatus.Pass); //All good
        }
    }
}


