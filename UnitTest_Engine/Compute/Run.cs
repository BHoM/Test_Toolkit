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

using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using BH.oM.Base.Attributes;
using BH.Engine.Base;
using BH.oM.Base;
using BH.Engine.Reflection;
using Microsoft.CSharp.RuntimeBinder;
using UT = BH.oM.Test.UnitTests;

namespace BH.Engine.UnitTest
{
    public static partial class Compute
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Runs through the method in the unit test with all corresponding testdata and returns the result of the method and any errors encountered during the execution.")]
        [Input("test", "The test to run, containing the method to be executed as well as the data to test on.")]
        [MultiOutput(0, "result", "The result from the execution of the test data. Each outer list corresponds to one execution of the method and each inner list to the outputs from the method call.")]
        [MultiOutput(1, "errors", "Any errors encountered during the execution of the method.")]
        public static Output<List<List<object>>, List<string>> Run(this UT.UnitTest test)
        {                   
            if(test == null)
            {
                BH.Engine.Base.Compute.RecordError("Could not run unit test as provided test was null.");
                return null;
            }

            List<List<object>> results = new List<List<object>>();
            List<string> errors = new List<string>();
            MethodBase method = test.Method;
            foreach (UT.TestData data in test.Data)
            {
                var result = Run(method, data);
                results.Add(result.Item1);
                errors.AddRange(result.Item2);
            }
            return new Output<List<List<object>>, List<string>> { Item1 = results, Item2 = errors };
        }

        /***************************************************/

        [Description("Executes the method with the corresponding inputdata in the provided TestData and returns the result of the method and any errors encountered during the execution.")]
        [Input("method", "The method to run.")]
        [Input("data", "The data to invoke the method with. Only the Input part of the test data will be used by this method.")]
        [MultiOutput(0, "result", "The result from the execution of the test data.")]
        [MultiOutput(1, "errors", "Any errors encountered during the execution of the method.")]
        public static Output<List<object>, List<string>> Run(MethodBase method, UT.TestData data)
        {
            if(method == null)
            {
                BH.Engine.Base.Compute.RecordError("Method to run a test on cannot be null.");
                return null;
            }

            if(data == null)
            {
                BH.Engine.Base.Compute.RecordError("Data to use to test method with cannot be null.");
                return null;
            }

            List<object> result = new List<object>();
            List<string> errors = new List<string>();

            if (method.IsGenericMethod && method is MethodInfo)
                method = Engine.Base.Compute.MakeGenericFromInputs(method as MethodInfo, data.Inputs.Select(x => FixType(x)?.GetType()).ToList());

            try
            {
                ParameterInfo[] parameters = method.GetParameters();
                if (data.Inputs.Count == parameters.Length)
                {
                    List<object> inputs = new List<object>();
                    for (int i = 0; i < parameters.Length; i++)
                        inputs.Add(CastToType(data.Inputs[i], parameters[i].ParameterType));

                    object resultObject = method.Invoke(null, inputs.ToArray());

                    IOutput output = resultObject as IOutput;

                    if (output == null)
                        result.Add(resultObject);
                    else
                    {
                        for (int i = 0; i < output.OutputCount(); i++)
                        {
                            result.Add(output.IItem(i));
                        }
                    }
                }
                else
                {
                    errors.Add("The number of inputs is not matching the number of parameters");
                }
            }
            catch (Exception e)
            {
                string message = "Failed to run with the given inputs";
                if (e is NotImplementedException || e.InnerException is NotImplementedException)
                    message = "The method is not implemented.";
                else if (e is RuntimeBinderException || e.InnerException is RuntimeBinderException)
                    message = "The method with the correct input types cannot be found.";
                errors.Add(message);
            }

            return new Output<List<object>, List<string>> { Item1 = result, Item2 = errors };
        }

        /***************************************************/
        /**** private Methods                           ****/
        /***************************************************/


        private static object CastToType(object item, Type type)
        {
            try
            {
                if (type.IsGenericType && item is IEnumerable)
                {
                    dynamic list;
                    if (type.IsInterface)
                        list = Activator.CreateInstance(typeof(List<>).MakeGenericType(type.GenericTypeArguments[0]));
                    else
                        list = Activator.CreateInstance(type);
                    foreach (dynamic val in (IEnumerable)item)
                    {
                        if (val is IEnumerable)
                            list.Add(CastToType(val, type.GenericTypeArguments[0]));
                        else
                            list.Add(val);
                    }
                    return list;
                }
                else
                {
                    return item;
                }
            }
            catch
            {
                return item;
            }
        }

        /***************************************************/

        private static object FixType(object argument)
        {
            if (argument is List<object>)
            {
                List<object> list = argument as List<object>;
                List<Type> types = list.Select(x => x.GetType()).Distinct().ToList();
                if (types.Count == 1)
                    return CastList(list, list.First() as dynamic);
                else
                    return argument;
            }
            else
                return argument;
        }

        /***************************************************/

        private static List<T> CastList<T>(List<object> list, T first)
        {
            return list.Cast<T>().ToList();
        }

        /***************************************************/

    }
}




