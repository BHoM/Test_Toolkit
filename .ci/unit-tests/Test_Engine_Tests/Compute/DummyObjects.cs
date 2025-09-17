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

using BH.Engine.Diffing;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.Test.Engine.Test
{ 
    public class DummyObjectsTests
    {
        /***************************************************/
        /**** Test Methods                              ****/
        /***************************************************/

        [TestCaseSource(nameof(TestTypes))]
        public void TestDummyObjectCreation(Type type)
        {
            Assume.That(type != null, "Type should not be null.");

            var dummyObject = BH.Engine.Test.Compute.DummyObject(type);

            // Assert
            Assert.That(dummyObject, Is.Not.Null, $"Dummy object of type {type.Name} should not be null.");
            if(type.IsGenericType)
                Assert.That(dummyObject.GetType().GetGenericTypeDefinition(), Is.EqualTo(type.GetGenericTypeDefinition()), $"Dummy object should be of type {type.GetGenericTypeDefinition().Name}.");
            else
                Assert.That(dummyObject, Is.InstanceOf(type), $"Dummy object should be an instance of {type.Name}.");

            Assert.Multiple(() =>
            {
                foreach (var prop in dummyObject.GetType().GetProperties())
                {
                    if (prop.CanRead)
                    {
                        if (IsValidPropType(prop.PropertyType))
                        {
                            var value = prop.GetValue(dummyObject);
                            Assert.That(value, Is.Not.Null, $"Property {prop.Name} of type {type.FullName} should not be null.");
                        }
                        else
                            Warn.If(true, $"Property {prop.Name} of type {type.FullName} is skipped due to its type being {prop.PropertyType.FullName} which currently is not handled by the DummyObject creation.");
                    }
                }
            });
        }

        private bool IsValidPropType(Type propertyType)
        {
            if(typeof(Stream).IsAssignableFrom(propertyType) || 
               typeof(Delegate).IsAssignableFrom(propertyType) ||
               typeof(System.Drawing.Bitmap).IsAssignableFrom(propertyType) ||  //Temporarily excluded unitl https://github.com/BHoM/Test_Toolkit/issues/526 has been resolved
               propertyType.Namespace.StartsWith("Microsoft.CodeAnalysis.CSharp"))
            {
                return false; // Skip properties of these types
            }
            return true;
        }

        /***************************************************/
        /**** Test Data Methods                         ****/
        /***************************************************/

        public static IEnumerable<Type> TestTypes()
        {
            HashSet<Type> excludedTypes = new HashSet<Type>
            {
                typeof(BH.oM.Base.FragmentSet), // Not common IObject
            };

            HashSet<string> excludedTypeNames = new HashSet<string>
            {
                "BH.oM.Forms.WindowLayoutSettings"  //Requires access to WinForms to instantiate, which is not available in this context

            };

            BH.Engine.Base.Compute.LoadAllAssemblies("","oM$"); //Load all oM assemblies to ensure types are available.
            return BH.Engine.Base.Query.BHoMTypeList().Where(x => !x.IsAbstract)    // Exclude abstract types
                                                      .Where(x => !typeof(Attribute).IsAssignableFrom(x))   // Exclude attributes
                                                      .Where(x => !typeof(Exception).IsAssignableFrom(x))   // Exclude exceptions
                                                      .Except(excludedTypes)   // Exclude specific types
                                                      .Where(x => !excludedTypeNames.Contains(x.FullName)); // Exclude specific type names
        }

        /***************************************************/
    }
}
