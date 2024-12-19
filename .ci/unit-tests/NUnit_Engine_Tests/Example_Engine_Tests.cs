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

using BH.Engine.Base;
using BH.oM.Test.NUnit;
using NUnit.Framework;
using Shouldly;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using BH.Engine.Example;
using BH.oM.Structure.Elements;
using BH.oM.Structure.SectionProperties;
using BH.oM.Geometry;

namespace BH.Test.Engine.NUnit
{
    public partial class Example_Engine_Tests : NUnitTest
    {
        [Test]
        [Description("Tests invoking a function (located in the Example_Engine) that requires a call to an extension method. " +
            "The extension method requires that a specific assembly which is not referenced by the test project (Geometry_Engine) is loaded in memory." +
            "The loading in memory has to be taken care by the base NUnitTest class.")]
        public void DynamicallyLoadedDependency()
        {
            ConcreteSection concreteSection = (ConcreteSection)BH.Engine.Base.Create.RandomObject(typeof(ConcreteSection));
            IGeometry result = BH.Engine.Example.Query.GetGeometryViaExtensionMethod(concreteSection);
            result.ShouldNotBeNull("The dynamically loaded Extension method could not be called.");
        }
    }
}


