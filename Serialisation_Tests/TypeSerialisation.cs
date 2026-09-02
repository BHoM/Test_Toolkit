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
    public class TypeSerialisation
    {

        [TestCaseSource(typeof(DataSource), nameof(DataSource.OmTypes))]
        public void ToFromJson(Type oMType)
        {
            TestResult result = TypeToFromJson(oMType);

            Assert.That(result.Status, Is.EqualTo(TestStatus.Pass), result.FullMessage(3, TestStatus.Warning));
        }

        /*************************************/


        //Below is copy pasted from Verification solution in BHoM_Engine


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

    }
}
