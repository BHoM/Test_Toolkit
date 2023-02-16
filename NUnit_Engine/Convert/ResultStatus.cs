/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2023, the respective contributors. All rights reserved.
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
using System;
using System.Collections.Generic;
using System.Text;

using System.ComponentModel;
using BH.oM.Base.Attributes;

namespace BH.Engine.Test.NUnit
{
    public static partial class Convert
    {
        [Description("Convert an NUnit result string to a BHoM Test Status. 'Passed' converts to TestStatus.Pass and 'Failed' converts to TestStatus.Error. Any other string input returns TestStatus.Error.")]
        [Input("resultStatus", "The NUnit result string, typically either 'Passed' or 'Failed'.")]
        [Output("testStatus", "The BHoM TestStatus enum value converted from the NUnit result string.")]
        public static TestStatus ToTestStatus(string resultStatus)
        {
            if (resultStatus == "Failed")
                return TestStatus.Error;
            else if (resultStatus == "Passed")
                return TestStatus.Pass;

            return TestStatus.Error; //Default - something probably went wrong so we'd want to investigate
        }
    }
}
