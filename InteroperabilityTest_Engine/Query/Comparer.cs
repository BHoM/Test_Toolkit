/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2026, the respective contributors. All rights reserved.
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
using BH.oM.Test.Interoperability;

namespace BH.Engine.Test.Interoperability
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Gets the EqualityComparer to be used for comparing Pushed and Pulled obejcts based on the provided PushPullCompareConfig.")]
        [Input("config", "PushPullCompareConfig used to determine which type of EqualityComparer that is to be used.")]
        [Input("adapterIDFragmentType", "The type of IdFragment to be used if the comparer returned is of type AdapterIDComparer.")]
        [Output("comparer", "The comparer to be used based on the provided PushPullCompareConfig.")]
        public static IEqualityComparer<IBHoMObject> Comparer(this PushPullCompareConfig config, Type adapterIDFragmentType)
        {
            if (config == null)
                return null;

            switch (config.ComparerType)
            {
                case ComparerType.AdapterID:
                    if (adapterIDFragmentType == null)
                        return null;

                    return new AdapterIDComparer(adapterIDFragmentType);
                case ComparerType.Name:
                    return new Base.Objects.BHoMObjectNameComparer();
                default:
                    Engine.Base.Compute.RecordWarning("Comarer of type " + config.ComparerType + " is not yet supported");
                    return null;
            }

        }

        /***************************************************/
 
    }
}






