/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2024, the respective contributors. All rights reserved.
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

using BH.oM.Test;
using BH.oM.Test.Results;

namespace BH.Engine.Test.CodeCompliance.DynamicChecks
{
    public static partial class Query
    {
        public static TestResult InputFromPropertyMatchesProperty(this MethodInfo method)
        {
            if (method == null)
                return null;

            IEnumerable<InputFromProperty> inputFromPropDesc = method.GetCustomAttributes<InputFromProperty>();
            if(inputFromPropDesc.Count() == 0)
                return Create.TestResult(TestStatus.Pass); //All is good with no properties

            List<Error> errors = new List<Error>();
            IEnumerable<PropertyInfo> properties = method.ReturnType.GetProperties();
            foreach(InputFromProperty p in inputFromPropDesc)
            {
                PropertyInfo pi = properties.Where(x => x.Name == p.PropertyName).FirstOrDefault();
                if(pi == null)
                {
                    //This input from property does not match a property of the returned object
                    Error e = new Error();
                    e.Status = TestStatus.Error;
                    e.Location = Create.Location(method.Signature(), Create.LineSpan(1, 1));
                    e.DocumentationLink = ""; //ToDo - provide this
                    e.Message = "InputFromProperty attribute with input name " + p.InputName + " and property name " + p.PropertyName + " does not match any of the properties of the return type " + method.ReturnType.Name + ".";
                    errors.Add(e);
                }
            }

            if (errors.Count == 0)
                return Create.TestResult(TestStatus.Pass); //Everything is good
            else
                return Create.TestResult(TestStatus.Error, errors);
        }
    }
}




