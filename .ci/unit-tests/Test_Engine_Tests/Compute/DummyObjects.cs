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
            // Arrange
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
                        var value = prop.GetValue(dummyObject);
                        Assert.That(value, Is.Not.Null, $"Property {prop.Name} of type {type.Name} should not be null.");
                    }
                }
            });
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
            BH.Engine.Base.Compute.LoadAllAssemblies("","oM$"); //Load all oM assemblies to ensure types are available.
            return BH.Engine.Base.Query.BHoMTypeList().Where(x => !x.IsAbstract).Except(excludedTypes);
        }

        /***************************************************/
    }
}
