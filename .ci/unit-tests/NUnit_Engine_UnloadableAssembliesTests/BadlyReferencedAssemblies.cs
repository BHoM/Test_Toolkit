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
    public class SampleTestClass : NUnitTest { }

    public class BadlyReferencedAssembliesTests
    {
        [Test]
        public void LoadAssembliesShouldThrowException()
        {
            // This test project contains some assemblies that are badly referenced 
            // (i.e. they do not have Copy Local set to true, that is required by Unit Test projects).
            // The following checks that the necessary explanatory exception is thrown.
            NUnitTest testClass = new SampleTestClass();

            try
            {
                testClass.LoadReferencedAssemblies();
            }
            catch(FileLoadException e)
            {
                // The exception was correctly thrown. Write to console to show it.
                Console.WriteLine($"The exception was correctly thrown. Message:\n\t{e.Message.Replace("\n", "\n\t")}");
                return;
            }

            Assert.Fail("This method should have thrown an exception.");
        }
    }
}