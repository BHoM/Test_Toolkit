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

namespace BH.Engine.Test.Interoperability
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Creates a key for grouping based on the propertyID and options.")]
        [Input("propertyID", "The property ID to generate a grouping key from.")]
        [Input("onlyLastProperty", "Only group by the last property key. This is, only the name of the final property failing, excluding any initial property.\n" +
               "As an example this would be StartNode.Position vs Position for the point of the start Node of a Bar.")]
        [Input("ignoreListIndex", "Ignores the list index position of a Property. if true the will return Nodes rather than for example Nodes[4] for list properties.")]
        [Output("key", "Grouping key based on the property ID.")]
        public static string PropertyGroupingIDKey(this string propertyID, bool onlyLastProperty, bool ignoreListIndex)
        {
            if (propertyID == null)
                return "";

            if (onlyLastProperty)
                propertyID = propertyID.Split('.').Last();

            if (ignoreListIndex)
            {
                int stIndex = propertyID.LastIndexOf('[');
                int endIndex = propertyID.LastIndexOf(']');

                while (stIndex != -1 && endIndex != -1)
                {
                    propertyID = propertyID.Remove(stIndex, endIndex - stIndex + 1);
                    stIndex = propertyID.LastIndexOf('[');
                    endIndex = propertyID.LastIndexOf(']');
                }

            }
            return propertyID;
        }

        /***************************************************/
    }
}




