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
    public class MethodSerialisation
    {

        [TestCaseSource(typeof(DataSource), nameof(DataSource.EngineMethods))]
        public void ToFromJson(MethodBase method)
        {
            TestResult result = MethodToFromJson(method);

            Assert.That(result.Status, Is.EqualTo(TestStatus.Pass), result.FullMessage(3, TestStatus.Warning));
            Assert.Pass($"Passing method serialisation test for {method.IToText(true)} from Assembly {method.DeclaringType.Assembly.FullName}");
        }

        /*************************************/


        //Below is copy pasted from Verification solution in BHoM_Engine

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
