using BH.Engine.Base;
using BH.Engine.Diffing;
using BH.Engine.Reflection;
using BH.Engine.Serialiser;
using BH.Engine.Test;
using BH.oM.Base;
using BH.oM.Base.Attributes;
using BH.oM.Test;
using BH.oM.Test.Results;
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
    public class ObjectSerialisation
    {
        [TestCaseSource(typeof(DataSource), nameof(DataSource.OmTypes))]
        public void ToFromJson(Type oMType)
        {
            TestResult result = ObjectToFromJson(oMType);

            Assert.That(result.Status, Is.EqualTo(TestStatus.Pass), result.FullMessage(3, TestStatus.Warning));
            Assert.Pass($"Passing object serialisation test for {oMType.FullName} from Assembly {oMType.Assembly.FullName}");
        }

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

    }
}
