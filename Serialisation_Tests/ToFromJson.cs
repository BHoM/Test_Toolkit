using BH.Engine.Base;
using BH.Engine.Diffing;
using BH.Engine.Reflection;
using BH.Engine.Serialiser;
using BH.Engine.Test;
using BH.oM.Base;
using BH.oM.Base.Attributes;
using BH.oM.Test;
using BH.oM.Test.Results;
using BH.oM.Test.UnitTests;
using BH.Tests.Setup.TestBases;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BH.Tests.Serialisation
{
    public class ToFromJson : BaseTestBase
    {
        [Test]
        public void DisplayTestAssemblies()
        { 
            foreach( var asm in TestAssemblies())
            {
                Console.WriteLine(asm.FullName);
            }
        }

        public static IEnumerable<Type> OmTypes()
        {
            return oMTypesToTest(TestAssemblies());
        }

        public static IEnumerable<MethodBase> EngineMethods()
        {
            return EngineMethodsToTest(TestAssemblies());
        }

        public static List<Assembly> TestAssemblies()
        {
            return Setup.Query.CurrentAssemblies();
        }

        public ToFromJson() : base(new List<Tuple<string, Type>> { new Tuple<string, Type>("OmTypes", typeof(Type)), new Tuple<string, Type>("EngineMethods", typeof(MethodBase)) }, false) { }

        /*************************************/

        [Test]
        [TestCaseSource("OmTypes")]
        public void ObjectSerialisation(Type oMType)
        {
            TestResult result = ObjectToFromJson(oMType);

            Assert.That(result.Status, Is.EqualTo(TestStatus.Pass), result.FullMessage(3, TestStatus.Warning));
            Assert.Pass($"Passing object serialisation test for {oMType.FullName} from Assembly {oMType.Assembly.FullName}");
        }

        /*************************************/

        [Test]
        [TestCaseSource("OmTypes")]
        public void TypeSerialisation(Type oMType)
        {
            TestResult result = TypeToFromJson(oMType);

            Assert.That(result.Status, Is.EqualTo(TestStatus.Pass), result.FullMessage(3, TestStatus.Warning));
        }

        /*************************************/

        [Test]
        [TestCaseSource("EngineMethods")]
        public void MethodSerialisation(MethodBase method)
        {
            TestResult result = MethodToFromJson(method);

            Assert.That(result.Status, Is.EqualTo(TestStatus.Pass), result.FullMessage(3, TestStatus.Warning));
            Assert.Pass($"Passing method serialisation test for {method.IToText(true)} from Assembly {method.DeclaringType.Assembly.FullName}");
        }

        /*************************************/


        public static List<Type> oMTypesToTest(List<Assembly> assembliesToTest)
        {
            assembliesToTest = assembliesToTest.Where(x => x.IsOmAssembly()).ToList();

            // It feels like the BHoMTypeList method should already return a clean list of Type but it doesn't at the moment
            return assembliesToTest.SelectMany(a => a.GetTypes().Where(x => {
                return typeof(IObject).IsAssignableFrom(x)
                  && !x.IsAbstract
                  && !x.IsDeprecated()
                  && !x.GetProperties().Select(p => p.PropertyType.Namespace).Any(n => !n.StartsWith("BH.") && !n.StartsWith("System"));
            })).ToList();
        }

        /*************************************/

        public static List<MethodInfo> EngineMethodsToTest(List<Assembly> assembliesToTest)
        {
            assembliesToTest = assembliesToTest.Where(x => x.IsEngineAssembly()).ToList();
            return BH.Engine.Base.Query.BHoMMethodList().Where(x => assembliesToTest.Any(a => x.DeclaringType.Assembly == a)).ToList();
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
