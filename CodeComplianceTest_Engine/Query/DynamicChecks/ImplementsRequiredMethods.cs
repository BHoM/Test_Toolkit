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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BH.oM.Test;
using BH.oM.Test.Results;
using BH.oM.Dimensional;
using BH.oM.Test.CodeCompliance;
using BH.oM.Base.Debugging;
using BH.oM.Geometry;

namespace BH.Engine.Test.CodeCompliance.DynamicChecks
{
    public static partial class Query
    {
        public static TestResult ImplementsRequiredMethods(Type type)
        {
            if (type == null)
                return Create.TestResult(oM.Test.TestStatus.Pass); //Null type cannot need required methods and so cannot fail

            BH.Engine.Base.Compute.LoadAllAssemblies(); //Just to be sure

            var obj = BH.Engine.Test.Compute.DummyObject(type);
            if (obj == null)
                return Create.TestResult(TestStatus.Pass);

            var objectTypeFullName = obj.GetType().FullName;

            TestResult result = null;

            //All attempts to run are wrapped in a try catch - if the catch element works because of a stack overflow or other exception because of bad dummy data, that's fine cause it means the extension method existed which is all this check cares about
            if (typeof(IElement0D).IsAssignableFrom(type))
                result = result.Merge(ImplementsIElement0DMethods(obj, objectTypeFullName));

            if (typeof(IElement1D).IsAssignableFrom(type))
                result = result.Merge(ImplementsIElement1DMethods(obj, objectTypeFullName));

            if (typeof(IElement2D).IsAssignableFrom(type))
                result = result.Merge(ImplementsIElement2DMethods(obj, objectTypeFullName));

            if (typeof(IElementM).IsAssignableFrom(type))
                result = result.Merge(ImplementsIElementMMethods(obj, objectTypeFullName));

            if(result == null)
                result = new TestResult() { Status = TestStatus.Pass };

            return result;            
        }

        /***************************************************/

        private static TestResult ImplementsIElement0DMethods(object obj, string objectFullName)
        {
            BH.Engine.Base.Compute.ClearCurrentEvents();

            TestResult result = null;

            object methodResult = null;
            try
            {
                methodResult = BH.Engine.Base.Compute.RunExtensionMethod(obj, "Geometry");
            }
            catch { }
            if (methodResult == null || BH.Engine.Base.Query.CurrentEvents().Where(x => x.Type == EventType.Error && x.Message.Contains("Failed to run Geometry extension method")).Any())
            {
                Error error = new Error()
                {
                    Message = $"Geometry extension method not implemented for object of type {objectFullName}.",
                    DocumentationLink = "ImplementsRequiredMethods",
                    Status = TestStatus.Error,
                };

                result = result.Merge(new TestResult()
                {
                    Description = $"Check if object of type {objectFullName} implements all required extension methods as an IElement0D object.",
                    ID = "",
                    Status = TestStatus.Error,
                    Information = new List<ITestInformation>() { error },
                });
            }

            BH.Engine.Base.Compute.ClearCurrentEvents();

            Point pt = new Point();
            try
            {
                methodResult = BH.Engine.Base.Compute.RunExtensionMethod(obj, "SetGeometry", new object[] { pt });
            }
            catch { }
            if (methodResult == null || BH.Engine.Base.Query.CurrentEvents().Where(x => x.Type == EventType.Error && x.Message.Contains("Failed to run SetGeometry extension method")).Any())
            {
                Error error = new Error()
                {
                    Message = $"SetGeometry extension method not implemented for object of type {objectFullName}.",
                    DocumentationLink = "ImplementsRequiredMethods",
                    Status = TestStatus.Error,
                };

                result = result.Merge(new TestResult()
                {
                    Description = $"Check if object of type {objectFullName} implements all required extension methods as an IElement0D object.",
                    ID = "",
                    Status = TestStatus.Error,
                    Information = new List<ITestInformation>() { error },
                });
            }

            BH.Engine.Base.Compute.ClearCurrentEvents();

            try
            {
                methodResult = BH.Engine.Base.Compute.RunExtensionMethod(obj, "HasMergeablePropertiesWith", new object[] { obj });
            }
            catch { }
            if (methodResult == null || BH.Engine.Base.Query.CurrentEvents().Where(x => x.Type == EventType.Error && x.Message.Contains("Failed to run HasMergeablePropertiesWith extension method")).Any())
            {
                Error error = new Error()
                {
                    Message = $"HasMergeablePropertiesWith extension method not implemented for object of type {objectFullName}.",
                    DocumentationLink = "ImplementsRequiredMethods",
                    Status = TestStatus.Error,
                };

                result = result.Merge(new TestResult()
                {
                    Description = $"Check if object of type {objectFullName} implements all required extension methods as an IElement0D object.",
                    ID = "",
                    Status = TestStatus.Error,
                    Information = new List<ITestInformation>() { error },
                });
            }

            return result;
        }

        /***************************************************/

        private static TestResult ImplementsIElement1DMethods(object obj, string objectFullName)
        {
            BH.Engine.Base.Compute.ClearCurrentEvents();

            TestResult result = null;

            object methodResult = null;
            try
            {
                methodResult = BH.Engine.Base.Compute.RunExtensionMethod(obj, "Geometry");
            }
            catch { }
            if (methodResult == null || BH.Engine.Base.Query.CurrentEvents().Where(x => x.Type == EventType.Error && x.Message.Contains("Failed to run Geometry extension method")).Any())
            {
                Error error = new Error()
                {
                    Message = $"Geometry extension method not implemented for object of type {objectFullName}.",
                    DocumentationLink = "ImplementsRequiredMethods",
                    Status = TestStatus.Error,
                };

                result = result.Merge(new TestResult()
                {
                    Description = $"Check if object of type {objectFullName} implements all required extension methods as an IElement1D object.",
                    ID = "",
                    Status = TestStatus.Error,
                    Information = new List<ITestInformation>() { error },
                });
            }

            BH.Engine.Base.Compute.ClearCurrentEvents();

            Polyline pline = new Polyline() { ControlPoints = new List<Point>() { new Point(), new Point() } }; //0 length polyline for testing purposes only
            try
            {
                methodResult = BH.Engine.Base.Compute.RunExtensionMethod(obj, "SetGeometry", new object[] { pline });
            }
            catch { }
            if (methodResult == null || BH.Engine.Base.Query.CurrentEvents().Where(x => x.Type == EventType.Error && x.Message.Contains("Failed to run SetGeometry extension method")).Any())
            {
                Error error = new Error()
                {
                    Message = $"SetGeometry extension method not implemented for object of type {objectFullName}.",
                    DocumentationLink = "ImplementsRequiredMethods",
                    Status = TestStatus.Error,
                };

                result = result.Merge(new TestResult()
                {
                    Description = $"Check if object of type {objectFullName} implements all required extension methods as an IElement1D object.",
                    ID = "",
                    Status = TestStatus.Error,
                    Information = new List<ITestInformation>() { error },
                });
            }

            BH.Engine.Base.Compute.ClearCurrentEvents();

            try
            {
                methodResult = BH.Engine.Base.Compute.RunExtensionMethod(obj, "HasMergeablePropertiesWith", new object[] { obj });
            }
            catch { }
            if (methodResult == null || BH.Engine.Base.Query.CurrentEvents().Where(x => x.Type == EventType.Error && x.Message.Contains("Failed to run HasMergeablePropertiesWith extension method")).Any())
            {
                Error error = new Error()
                {
                    Message = $"HasMergeablePropertiesWith extension method not implemented for object of type {objectFullName}.",
                    DocumentationLink = "ImplementsRequiredMethods",
                    Status = TestStatus.Error,
                };

                result = result.Merge(new TestResult()
                {
                    Description = $"Check if object of type {objectFullName} implements all required extension methods as an IElement1D object.",
                    ID = "",
                    Status = TestStatus.Error,
                    Information = new List<ITestInformation>() { error },
                });
            }

            return result;
        }

        /***************************************************/

        private static TestResult ImplementsIElement2DMethods(object obj, string objectFullName)
        {
            BH.Engine.Base.Compute.ClearCurrentEvents();

            TestResult result = null;

            object methodResult = null;
            try
            {
                methodResult = BH.Engine.Base.Compute.RunExtensionMethod(obj, "OutlineElements1D");
            }
            catch { }
            if (methodResult == null || BH.Engine.Base.Query.CurrentEvents().Where(x => x.Type == EventType.Error && x.Message.Contains("Failed to run OutlineElements1D extension method")).Any())
            {
                Error error = new Error()
                {
                    Message = $"OutlineElements1D extension method not implemented for object of type {objectFullName}.",
                    DocumentationLink = "ImplementsRequiredMethods",
                    Status = TestStatus.Error,
                };

                result = result.Merge(new TestResult()
                {
                    Description = $"Check if object of type {objectFullName} implements all required extension methods as an IElement2D object.",
                    ID = "",
                    Status = TestStatus.Error,
                    Information = new List<ITestInformation>() { error },
                });
            }

            BH.Engine.Base.Compute.ClearCurrentEvents();

            Polyline pline = new Polyline() { ControlPoints = new List<Point>() { new Point(), new Point() } }; //0 length polyline for testing purposes only
            List<IElement1D> outlines = new List<IElement1D>() { pline };

            try
            {
                methodResult = BH.Engine.Base.Compute.RunExtensionMethod(obj, "SetOutlineElements1D", new object[] { outlines });
            }
            catch { }
            if (methodResult == null || BH.Engine.Base.Query.CurrentEvents().Where(x => x.Type == EventType.Error && x.Message.Contains("Failed to run SetOutlineElements1D extension method")).Any())
            {
                Error error = new Error()
                {
                    Message = $"SetOutlineElements1D extension method not implemented for object of type {objectFullName}.",
                    DocumentationLink = "ImplementsRequiredMethods",
                    Status = TestStatus.Error,
                };

                result = result.Merge(new TestResult()
                {
                    Description = $"Check if object of type {objectFullName} implements all required extension methods as an IElement2D object.",
                    ID = "",
                    Status = TestStatus.Error,
                    Information = new List<ITestInformation>() { error },
                });
            }

            BH.Engine.Base.Compute.ClearCurrentEvents();

            try
            {
                methodResult = BH.Engine.Base.Compute.RunExtensionMethod(obj, "HasMergeablePropertiesWith", new object[] { obj });
            }
            catch { }
            if (methodResult == null || BH.Engine.Base.Query.CurrentEvents().Where(x => x.Type == EventType.Error && x.Message.Contains("Failed to run HasMergeablePropertiesWith extension method")).Any())
            {
                Error error = new Error()
                {
                    Message = $"HasMergeablePropertiesWith extension method not implemented for object of type {objectFullName}.",
                    DocumentationLink = "ImplementsRequiredMethods",
                    Status = TestStatus.Error,
                };

                result = result.Merge(new TestResult()
                {
                    Description = $"Check if object of type {objectFullName} implements all required extension methods as an IElement2D object.",
                    ID = "",
                    Status = TestStatus.Error,
                    Information = new List<ITestInformation>() { error },
                });
            }

            return result;
        }

        /***************************************************/

        private static TestResult ImplementsIElementMMethods(object obj, string objectFullName)
        {
            BH.Engine.Base.Compute.ClearCurrentEvents();

            TestResult result = null;

            object methodResult = null;
            try
            {
                methodResult = BH.Engine.Base.Compute.RunExtensionMethod(obj, "MaterialComposition");
            }
            catch { }
            if (methodResult == null || BH.Engine.Base.Query.CurrentEvents().Where(x => x.Type == EventType.Error && x.Message.Contains("Failed to run MaterialComposition extension method")).Any())
            {
                Error error = new Error()
                {
                    Message = $"MaterialComposition extension method not implemented for object of type {objectFullName}.",
                    DocumentationLink = "ImplementsRequiredMethods",
                    Status = TestStatus.Error,
                };

                result = result.Merge(new TestResult()
                {
                    Description = $"Check if object of type {objectFullName} implements all required extension methods as an IElementM object.",
                    ID = "",
                    Status = TestStatus.Error,
                    Information = new List<ITestInformation>() { error },
                });
            }

            BH.Engine.Base.Compute.ClearCurrentEvents();

            try
            {
                methodResult = BH.Engine.Base.Compute.RunExtensionMethod(obj, "SolidVolume");
            }
            catch { }
            if (methodResult == null || BH.Engine.Base.Query.CurrentEvents().Where(x => x.Type == EventType.Error && x.Message.Contains("Failed to run SolidVolume extension method")).Any())
            {
                Error error = new Error()
                {
                    Message = $"SolidVolume extension method not implemented for object of type {objectFullName}.",
                    DocumentationLink = "ImplementsRequiredMethods",
                    Status = TestStatus.Error,
                };

                result = result.Merge(new TestResult()
                {
                    Description = $"Check if object of type {objectFullName} implements all required extension methods as an IElementM object.",
                    ID = "",
                    Status = TestStatus.Error,
                    Information = new List<ITestInformation>() { error },
                });
            }

            BH.Engine.Base.Compute.ClearCurrentEvents();

            if(result != null)
            {
                //For IElementM, it is either the above two methods OR VolumetricMaterialTakeoff - so if the result is not null, it means we haven't found one or both of the above methods.
                //If we have VolumetricMaterialTakeoff then we can reset the result

                try
                {
                    methodResult = BH.Engine.Base.Compute.RunExtensionMethod(obj, "VolumetricMaterialTakeoff");
                }
                catch { }
                if (methodResult == null || BH.Engine.Base.Query.CurrentEvents().Where(x => x.Type == EventType.Error && x.Message.Contains("Failed to run VolumetricMaterialTakeoff extension method")).Any())
                {
                    Error error = new Error()
                    {
                        Message = $"VolumetricMaterialTakeoff extension method not implemented for object of type {objectFullName}.",
                        DocumentationLink = "ImplementsRequiredMethods",
                        Status = TestStatus.Error,
                    };

                    result = result.Merge(new TestResult()
                    {
                        Description = $"Check if object of type {objectFullName} implements all required extension methods as an IElementM object.",
                        ID = "",
                        Status = TestStatus.Error,
                        Information = new List<ITestInformation>() { error },
                    });
                }
                else
                    result = null; //We do have a VolumetricMaterialTakeoff method, so the other 2 not existing is not a problem
            }

            return result;
        }
    }
}

