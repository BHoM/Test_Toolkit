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

using BH.oM.Test.NUnit;
using BH.Test.Engine.NUnit.Objects;
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
    public class BadlyReferencedAssembliesTests
    {
        [Test]
        [Description("This test project contains some assemblies that are badly referenced " +
            "(i.e. they do not have Copy Local set to true, that is required by Unit Test projects)." +
            "This test verifies that the base NUnitTest class correctly throws an appropriate explanatory exception.")]
        public void LoadAssembliesShouldThrowException()
        {
            NUnitTest testClass = new SampleTestClass();

            try
            {
                testClass.LoadReferencedAssemblies();
            }
            catch (FileLoadException e)
            {
                // These assemblies are badly referenced (have copy local to false)
                e.Message.ShouldContain("Analytical_Engine");
                e.Message.ShouldContain("Dimensional_oM");

                // These assemblies are well referenced (have copy local to true)
                e.Message.ShouldNotContain("Structure_oM");
                e.Message.ShouldNotContain("Geometry_oM");
                e.Message.ShouldNotContain("BHoM");
                e.Message.ShouldNotContain("BHoM_Engine");

                // The exception was correctly thrown. Write to console to show it.
                Console.WriteLine($"The exception was correctly thrown. Message:\n\t{e.Message.Replace("\n", "\n\t")}");
                return;
            }

            Assert.Fail("This method should have thrown an exception.");
        }
    }


}


