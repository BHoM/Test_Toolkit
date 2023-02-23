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

using BH.oM.Test.NUnit;
using NUnit.Framework;
using Shouldly;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace BH.Test.Engine.NUnit
{
    public class NUnitSampleTestClass
    {
        [Test]
        public void LoadAssembliesShouldThrowException()
        {
            NUnitTest testClass = new NUnitTest();

            Should.Throw(() => testClass.LoadReferencedAssemblies(), typeof(FileLoadException));
        }

        [Test]
        public void ErrorReportingPassingTest()
        {
            BH.Engine.Base.Compute.RecordError($"Some error logged via BH.Engine.Base.Compute.{nameof(Compute.RecordError)}");
            Assert.Pass(); // Make it pass intentionally here; the recorded error should still make the test report failure on TearDown
        }

        [Test]
        public void WarningReportingPassingTest()
        {
            BH.Engine.Base.Compute.RecordWarning($"Some warning logged via BH.Engine.Base.Compute.{nameof(Compute.RecordWarning)}");
            Assert.Pass();
        }


        [Test]
        public void NoteReportingPassingTest()
        {
            BH.Engine.Base.Compute.RecordNote($"Some note logged via BH.Engine.Base.Compute.{nameof(Compute.RecordNote)}");
            Assert.Pass();
        }
    }
}
