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
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using BH.oM.Base.Attributes;
using BH.oM.Base;
using BH.oM.Test;
using BH.Engine.Test;

using BH.oM.Test.CodeCompliance;

namespace BH.Engine.Test.CodeCompliance
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Returns a full concatenated message for the Error, only giving messages worse or equal to the provided severity level.")]
        [Input("error", "The Error to give the full message for.")]
        [Input("maxDepth", "Maximum level of recursiveness for inner TestInformation. Not in use for this object type.")]
        [Input("minSeverity", "The minimum level of severity of the information to report back. Returns an empty string if the Error does not pass this check.")]
        [Output("message", "Full message for the Error.")]
        public static string FullMessage(this Error error, int maxDepth = 3, TestStatus minSeverity = TestStatus.Pass)
        {
            if (error == null || !error.Status.IsEqualOrMoreSevere(minSeverity))
                return "";

            return error.Message + " - For more information see https://github.com/BHoM/documentation/wiki/" + error.DocumentationLink + Environment.NewLine + Environment.NewLine;
        }

        /***************************************************/
    }
}

