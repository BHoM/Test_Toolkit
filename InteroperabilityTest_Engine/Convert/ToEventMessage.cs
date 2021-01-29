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
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using BH.oM.Reflection.Attributes;
using BH.oM.Reflection.Debugging;
using BH.oM.Test.Results;
using BH.oM.Test;
using BH.oM.Base;

namespace BH.Engine.Test.Interoperability
{
    public static partial class Convert
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("")]
        [Input("", "")]
        [Output("", "")]
        public static EventMessage ToEventMessage(this Event debugEvent, bool enforceStatus = false, TestStatus enforcedStatus = TestStatus.Error)
        {
            TestStatus status;

            if (enforceStatus)
                status = enforcedStatus;

            switch (debugEvent.Type)
            {

                case EventType.Error:
                    status = TestStatus.Error;
                    break;
                case EventType.Warning:
                    status = TestStatus.Warning;
                    break;
                case EventType.Note:
                    status = TestStatus.Pass;
                    break;
                case EventType.Unknown:
                default:
                    status = TestStatus.Error;
                    break;
            }

            return new EventMessage
            {
                Message = debugEvent.Message,
                UTCTime = debugEvent.Time,
                Status = status
            };
        }

        /***************************************************/
    }
}
