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
using BH.oM.Test.Results;
using BH.oM.Base.Debugging;

namespace BH.Engine.Test
{
    public static partial class Convert
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Convert a debugging event into a test event message.")]
        [Input("debugEvent", "Debugging event to convert.")]
        [Output("message", "Resulting test event message.")]
        public static EventMessage ToEventMessage(this Event debugEvent)
        {
            if (debugEvent == null)
                return null;

            return new EventMessage
            {
                Message = debugEvent.Message,
                Status = debugEvent.Type.ToTestStatus(),
                StackTrace = debugEvent.StackTrace,
                UTCTime = debugEvent.UtcTime
            };
        }

        /***************************************************/
    }
}


