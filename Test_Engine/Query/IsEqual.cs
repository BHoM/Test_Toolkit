/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2021, the respective contributors. All rights reserved.
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
using BH.oM.Reflection;
using BH.oM.Base;
using BH.oM.Diffing;
using System.ComponentModel;
using BH.oM.Reflection.Attributes;
using KellermanSoftware.CompareNetObjects;
using BH.Engine.Reflection;

namespace BH.Engine.Test
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static bool IsEqual(this object a, object b)
        {
            if (a == null || b == null)
                return a == b;

            if (a.GetType() != b.GetType())
                return false;

            return m_EqualityComparer.Compare(a, b).AreEqual;
        }

        /***************************************************/

        [Description("Checks two objects for equality property by property and returns the differences")]
        [Input("config", "Config to be used for the comparison. Can set numeric tolerance, wheter to check the guid, if custom data should be ignored and if any additional properties should be ignored")]
        [MultiOutput(0, "IsEqual", "Returns true if the two items are deemed to be equal")]
        [MultiOutput(1, "DiffProperty", "List of the names of the properties found to be different")]
        [MultiOutput(2, "Obj1DiffValue", "List of the values deemed different for object 1")]
        [MultiOutput(3, "Obj2DiffValue", "List of the values deemed different for object 2")]
        public static Output<bool, List<string>, List<string>, List<string>> IsEqual(this object obj1, object obj2, BH.oM.Base.ComparisonConfig config = null)
        {
            //Use default config if null
            config = config ?? new oM.Base.ComparisonConfig();

            CompareLogic comparer = new CompareLogic();
            comparer.Config.MaxDifferences = 1000;
            comparer.Config.MembersToIgnore = config.PropertyExceptions;
            comparer.Config.DoublePrecision = config.NumericTolerance;

            Output<bool, List<string>, List<string>, List<string>> output = new Output<bool, List<string>, List<string>, List<string>>
            {
                Item1 = false,
                Item2 = new List<string>(),
                Item3 = new List<string>(),
                Item4 = new List<string>()
            };

            if (obj1 == null || obj2 == null)
                return output;

            try
            {
                ComparisonResult result = comparer.Compare(obj1, obj2);
                output.Item1 = result.AreEqual;
                output.Item2 = result.Differences.Select(x => x.PropertyName).ToList();
                output.Item3 = result.Differences.Select(x => x.Object1Value).ToList();
                output.Item4 = result.Differences.Select(x => x.Object2Value).ToList();
            }
            catch (Exception e)
            {
                Engine.Reflection.Compute.RecordError($"Comparison between {obj1.IToText()} and {obj2.IToText()} failed:\n{e.Message}");
            }

            return output;
        }

        [Description("Checks two BHoMObjects property by property and returns the differences")]
        [Input("config", "Config to be used for the comparison. Can set numeric tolerance, wheter to check the guid, if custom data should be ignored and if any additional properties should be ignored")]
        [Output("Dictionary whose key is the name of the property, and value is a tuple with its value in obj1 and obj2.")]
        public static Dictionary<string, Tuple<object, object>> DifferentProperties(this IBHoMObject obj1, IBHoMObject obj2, BH.oM.Base.ComparisonConfig config = null)
        {
            var dict = new Dictionary<string, Tuple<object, object>>();

            //Use default config if null
            config = config ?? new BH.oM.Base.ComparisonConfig();

            CompareLogic comparer = new CompareLogic();

            comparer.Config.MaxDifferences = 1000;
            comparer.Config.MembersToIgnore = config.PropertyExceptions;
            comparer.Config.DoublePrecision = config.NumericTolerance;

            ComparisonResult result = comparer.Compare(obj1, obj2);
            dict = result.Differences.ToDictionary(diff => diff.PropertyName, diff => new Tuple<object, object>(diff.Object1, diff.Object2));

            if (dict.Count == 0)
                return null;

            return dict;
        }


        /***************************************************/
        /**** Private Static Fields                     ****/
        /***************************************************/

        private static CompareLogic m_EqualityComparer = new CompareLogic();

        /***************************************************/
    }
}


