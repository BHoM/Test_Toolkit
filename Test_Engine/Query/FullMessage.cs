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
using System.Collections.Generic;
using System.ComponentModel;
using BH.oM.Base.Attributes;
using BH.oM.Base;
using BH.oM.Test;
using BH.oM.Test.Results;

namespace BH.Engine.Test
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Returns a full concatenated message for a test result and its inner result to the target max depth, only giving messages worse or equal to the provided severity level.")]
        [Input("result", "The TestResult to give the full message for.")]
        [Input("maxDepth", "Maximum level of recursiveness for inner TestInformation.")]
        [Input("minSeverity", "The minimum level of severity of the information to report back.")]
        [Output("message", "Full message for the TestResult.")]
        public static string FullMessage(this TestResult result, int maxDepth = 3, TestStatus minSeverity = TestStatus.Pass)
        {
            //Return an empty string if the status is not at least as severe as the provided minimum.
            if (result == null || !result.Status.IsEqualOrMoreSevere(minSeverity))
                return "";

            string message = result.Description + Environment.NewLine;
            message += result.Message + Environment.NewLine;

            //Filter out only info that passes the severity level
            List<ITestInformation> innerInfo = result.Information.Where(x => x.Status.IsEqualOrMoreSevere(minSeverity)).ToList();

            if (maxDepth > 0 && innerInfo.Count > 0)
            {
                message += "Inner results:" + Environment.NewLine;

                foreach (ITestInformation info in innerInfo)
                {
                    string innerMessage = IFullMessage(info, maxDepth - 1, minSeverity);
                    string[] arr = innerMessage.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string s in arr)
                    {
                        message += "\t" + s + Environment.NewLine;
                    }

                }
            }

            return message;
        }

        /***************************************************/
        /**** Public Methods - Interface                ****/
        /***************************************************/

        public static string IFullMessage(this ITestInformation result, int maxDepth = 3, TestStatus minSeverity = TestStatus.Pass)
        {
            if (result == null)
                return "";

            return FullMessage(result as dynamic, maxDepth, minSeverity);
        }

        /***************************************************/
        /**** Private Methods - Fallback                ****/
        /***************************************************/

        private static string FullMessage(this ITestInformation result, int maxDepth = 3, TestStatus minSeverity = TestStatus.Pass)
        {
            object ret = Base.Compute.RunExtensionMethod(result, "FullMessage", new object[] { maxDepth, minSeverity });

            if (ret != null)
                return ret.ToString();
            else
                return result.Message;
        }

        /***************************************************/
    }
}




