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
