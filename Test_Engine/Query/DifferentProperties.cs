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
using BH.oM.Base;
using BH.oM.Diffing;
using System.ComponentModel;
using BH.oM.Base.Attributes;
using KellermanSoftware.CompareNetObjects;

namespace BH.Engine.Test
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Checks two BHoMObjects property by property and returns the differences.")]
        [Input("config", "Config to be used for the comparison. Can set numeric tolerance, whether to check the guid, if custom data should be ignored and if any additional properties should be ignored")]
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
            comparer.Config.TypesToIgnore = config.TypeExceptions;

            ComparisonResult result = comparer.Compare(obj1, obj2);
            dict = result.Differences.ToDictionary(diff => diff.PropertyName, diff => new Tuple<object, object>(diff.Object1, diff.Object2));

            if (dict.Count == 0)
                return null;

            return dict;
        }


        /***************************************************/
    }
}



