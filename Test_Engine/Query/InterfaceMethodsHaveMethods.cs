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
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BH.oM.Base;

using System.Reflection;
using BH.Engine.Reflection;

namespace BH.Engine.Test
{
    public static partial class Query
    {
        public static List<string> InterfaceMethodsHaveMethods()
        {
            BH.Engine.Reflection.Compute.LoadAllAssemblies();
            Dictionary<string, List<string>> errors = new Dictionary<string, List<string>>();

            List<string> errorOut = new List<string>();

            // Get the types implementing each interface
            Dictionary<Type, List<Type>> interfaceDic = new Dictionary<Type, List<Type>>();
            foreach (Type type in BH.Engine.Reflection.Query.BHoMTypeList())
            {
                foreach (Type inter in type.GetInterfaces())
                {
                    if (!interfaceDic.ContainsKey(inter))
                        interfaceDic[inter] = new List<Type> { type };
                    else
                        interfaceDic[inter].Add(type);
                }
            }

            // Get all the methods organised between I methods and others
            Dictionary<string, List<MethodInfo>> methods = new Dictionary<string, List<MethodInfo>>();
            List<MethodInfo> iMethods = new List<MethodInfo>();
            foreach (MethodInfo method in BH.Engine.Reflection.Query.BHoMMethodList())
            {
                if (method.IsInterfaceMethod())
                    iMethods.Add(method);
                else if (methods.ContainsKey(method.Name))
                    methods[method.Name].Add(method);
                else
                    methods[method.Name] = new List<MethodInfo> { method };
            }

            // Make sure each I method has a corresponding method for all the types implementing its first argument
            foreach (MethodInfo inter in iMethods)
            {
                var name = inter.Name.Substring(1);
                if (!methods.ContainsKey(name))
                {
                    errorOut.Add("Interface method does not have associated methods - " + inter.ToText(true, "(", ",", ")", false));
                    continue;
                }

                ParameterInfo[] param = inter.GetParameters();
                if (param.Length == 0)
                {
                    errorOut.Add("Interface method does not have parameters - " + inter.ToText(true, "(", ",", ")", false));
                    continue;
                }

                Type paramType = param[0].ParameterType;
                if (!interfaceDic.ContainsKey(paramType))
                {
                    errorOut.Add("Interface method does not have recognised interface as first parameter - " + inter.ToText(true, "(", ",", ")", false));
                    continue;
                }

                List<Type> coveredTypes = methods[name].Where(x => x.GetParameters().Count() > 0).Select(x => x.GetParameters().First().ParameterType).Distinct().ToList();
                List<Type> missingTypes = interfaceDic[paramType].Except(coveredTypes).ToList();
                if (missingTypes.Count() > 0)
                {
                    if (missingTypes.Count() > 10)
                        errorOut.Add(inter.ToText(true, "(", ",", ")", false) + " is missing: Many types");
                    else
                        errorOut.Add(inter.ToText(true, "(", ",", ")", false) + " is missing: " + missingTypes.Select(x => x.ToText(true)).Aggregate((a, b) => a + ", " + b));
                }
            }

            return errorOut;
        }

        private static void AddError(Dictionary<string, List<string>> errors, string message, string item)
        {
            if (!errors.ContainsKey(message))
                errors.Add(message, new List<string>());
            errors[message].Add(item);
        }
    }
}
