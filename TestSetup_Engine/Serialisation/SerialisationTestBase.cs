using BH.Engine.Base;
using BH.Engine.Diffing;
using BH.Engine.Reflection;
using BH.Engine.Serialiser;
using BH.Engine.Test;
using BH.Engine.Test.CodeCompliance;
using BH.oM.Base;
using BH.oM.Base.Attributes;
using BH.oM.Test;
using BH.oM.Test.CodeCompliance;
using BH.oM.Test.NUnit;
using BH.oM.Test.Results;
using BH.oM.Test.UnitTests;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BH.Tests.Setup
{
    public abstract class SerialisationTestBase : NUnitTest
    {

        /*************************************/

        [TestCaseSource("OmTypes")]
        public void ObjectSerialisation(Type oMType)
        {
            TestResult result = ObjectToFromJson(oMType);

            Assert.That(result.Status, Is.EqualTo(TestStatus.Pass), result.FullMessage(3, TestStatus.Warning));

            Console.WriteLine(result.FullMessage());
        }

        /*************************************/

        [TestCaseSource("OmTypes")]
        public void TypeSerialisation(Type oMType)
        {
            TestResult result = TypeToFromJson(oMType);

            Assert.That(result.Status, Is.EqualTo(TestStatus.Pass), result.FullMessage(3, TestStatus.Warning));

            Console.WriteLine(result.FullMessage());
        }

        /*************************************/


        [TestCaseSource("EngineMethods")]
        public void MethodSerialisation(MethodBase method)
        {
            TestResult result = MethodToFromJson(method);

            Assert.That(result.Status, Is.EqualTo(TestStatus.Pass), result.FullMessage(3, TestStatus.Warning));

            Console.WriteLine(result.FullMessage());
        }

        /*************************************/

        public static List<Type> oMTypesToTest(Assembly assemblyToTest)
        {
            // It feels like the BHoMTypeList method should already return a clean list of Type but it doesn't at the moment
            return assemblyToTest.GetTypes().Where(x => {
                return typeof(IObject).IsAssignableFrom(x)
                  && !x.IsAbstract
                  && !x.IsDeprecated()
                  && !x.GetProperties().Select(p => p.PropertyType.Namespace).Any(n => !n.StartsWith("BH.") && !n.StartsWith("System"));
            }).ToList();
        }

        /*************************************/

        public static List<MethodInfo> EngineMethodsToTest(Assembly assemblyToTest)
        {
            BH.Engine.Base.Compute.ExtractAssembly(assemblyToTest);
            return BH.Engine.Base.Query.BHoMMethodList().Where(x => x.DeclaringType.Assembly == assemblyToTest).ToList();
        }

        /*************************************/

        //Below is copy pasted from Verification solution in BHoM_Engine

        public static TestResult ObjectToFromJson(Type type)
        {
            string typeDescription = type.IToText(true);

            // Create the test objects of the given type
            List<object> testObjects = new List<object>();
            if (testObjects.Count == 0)
            {
                object dummy = null;
                try
                {
                    Engine.Base.Compute.ClearCurrentEvents();
                    dummy = Engine.Test.Compute.DummyObject(type);
                }
                catch (Exception e)
                {
                    Engine.Base.Compute.RecordWarning(e.Message);
                }

                if (dummy == null)
                    return new TestResult
                    {
                        Description = typeDescription,
                        Status = TestStatus.Warning,
                        Message = $"Warning: Failed to create a dummy object of type {typeDescription}.",
                        Information = Engine.Base.Query.CurrentEvents().Select(x => x.ToEventMessage()).ToList<ITestInformation>()
                    };
                else
                    testObjects.Add(dummy);
            }

            // Test each object in the list
            foreach (object testObject in testObjects)
            {
                // To Json
                string json = "";
                try
                {
                    Engine.Base.Compute.ClearCurrentEvents();
                    json = testObject.ToJson();
                }
                catch (Exception e)
                {
                    Engine.Base.Compute.RecordError(e.Message);
                }

                if (string.IsNullOrWhiteSpace(json))
                    return new TestResult
                    {
                        Description = typeDescription,
                        Status = TestStatus.Error,
                        Message = $"Error: Failed to convert object of type {typeDescription} to json.",
                        Information = Engine.Base.Query.CurrentEvents().Select(x => x.ToEventMessage()).ToList<ITestInformation>()
                    };

                // From Json
                object copy = null;
                try
                {
                    Engine.Base.Compute.ClearCurrentEvents();
                    copy = Engine.Serialiser.Convert.FromJson(json);
                }
                catch (Exception e)
                {
                    Engine.Base.Compute.RecordError(e.Message);
                }

                bool isEqual;

                try
                {
                    isEqual = testObject.IsEqual(copy);
                }
                catch (Exception e)
                {
                    BH.Engine.Base.Compute.RecordWarning(e, $"Crashed when trying to compare objects.");

                    return new TestResult
                    {
                        Description = typeDescription,
                        Status = TestStatus.Warning,
                        Message = $"Warning: Failed to compare objects of type {typeDescription}.",
                        Information = Engine.Base.Query.CurrentEvents().Select(x => x.ToEventMessage()).ToList<ITestInformation>()
                    };
                }

                if (!isEqual)
                    return new TestResult
                    {
                        Description = typeDescription,
                        Status = TestStatus.Error,
                        Message = $"Error: Object of type {typeDescription} is not equal to the original after serialisation.",
                        Information = Engine.Base.Query.CurrentEvents().Select(x => x.ToEventMessage()).ToList<ITestInformation>()
                    };
            }

            // All test objects passed the test
            return Engine.Test.Create.PassResult(typeDescription);
        }

        /*************************************/


        public static TestResult TypeToFromJson(Type type)
        {
            string typeDescription = type.IToText(true);

            // To Json
            string json = "";
            try
            {
                Engine.Base.Compute.ClearCurrentEvents();
                json = type.ToJson();
            }
            catch (Exception e)
            {
                Engine.Base.Compute.RecordError(e.Message);
            }

            if (string.IsNullOrWhiteSpace(json))
                return new TestResult
                {
                    Description = typeDescription,
                    Status = TestStatus.Error,
                    Message = $"Error: Failed to convert type {typeDescription} to json.",
                    Information = Engine.Base.Query.CurrentEvents().Select(x => x.ToEventMessage()).ToList<ITestInformation>()
                };

            // From Json
            Type copy = null;
            try
            {
                Engine.Base.Compute.ClearCurrentEvents();
                copy = Engine.Serialiser.Convert.FromJson(json) as Type;
            }
            catch (Exception e)
            {
                Engine.Base.Compute.RecordError(e.Message);
            }

            if (!type.IsEqual(copy))
                return new TestResult
                {
                    Description = typeDescription,
                    Status = TestStatus.Error,
                    Message = $"Error: Type {typeDescription} is not equal to the original after serialisation.",
                    Information = Engine.Base.Query.CurrentEvents().Select(x => x.ToEventMessage()).ToList<ITestInformation>()
                };

            // All test objects passed the test
            return Engine.Test.Create.PassResult(typeDescription);
        }

        /*************************************/


        public static TestResult MethodToFromJson(MethodBase method)
        {
            string methodDescription = method.IToText(true);

            // To Json
            string json = "";
            try
            {
                Engine.Base.Compute.ClearCurrentEvents();
                json = method.ToJson();
            }
            catch (Exception e)
            {
                Engine.Base.Compute.RecordError(e.Message);
            }

            if (string.IsNullOrWhiteSpace(json))
                return new TestResult
                {
                    Description = methodDescription,
                    Status = TestStatus.Error,
                    Message = $"Error: Failed to convert method {methodDescription} to json.",
                    Information = Engine.Base.Query.CurrentEvents().Select(x => x.ToEventMessage()).ToList<ITestInformation>()
                };

            // From Json
            MethodInfo copy = null;
            try
            {
                Engine.Base.Compute.ClearCurrentEvents();
                copy = Engine.Serialiser.Convert.FromJson(json) as MethodInfo;
            }
            catch (Exception e)
            {
                Engine.Base.Compute.RecordError(e.Message);
            }

            if (!method.IsEqual(copy))
                return new TestResult
                {
                    Description = methodDescription,
                    Status = TestStatus.Error,
                    Message = $"Error: Method {methodDescription} is not equal to the original after serialisation.",
                    Information = Engine.Base.Query.CurrentEvents().Select(x => x.ToEventMessage()).ToList<ITestInformation>()
                };

            // All test objects passed the test
            return Engine.Test.Create.PassResult(methodDescription);
        }

        /*************************************/
    }
}
