using BH.oM.Test.NUnit;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace BH.Tests.Setup.TestBases
{
    public abstract class BaseTestBase : NUnitTest
    {
        private List<Tuple<string, Type>> m_AssumedStaticMethods;
        private bool m_AllRequired;

        public BaseTestBase(List<Tuple<string, Type>> assumedStaticMethods, bool allRequired)
        {
            m_AssumedStaticMethods = assumedStaticMethods;
            m_AllRequired = allRequired;
        }

        public BaseTestBase(string assumedMethodName, Type assumedReturnType)
        {
            m_AssumedStaticMethods = new List<Tuple<string, Type>>()
            {
                new Tuple<string, Type>(assumedMethodName, assumedReturnType)
            };
            m_AllRequired = true;
        }

        [OneTimeSetUp]
        public void EnsureStaticMembers()
        {
            if (m_AllRequired)
            {
                foreach (Tuple<string, Type> assumedType in m_AssumedStaticMethods)
                {
                    string methodName = assumedType.Item1;
                    Type type = assumedType.Item2;
                    var testDataMethod = GetType().GetMethod(methodName, BindingFlags.Static | BindingFlags.Public);
                    Assume.That(testDataMethod != null, $"Expected static member {methodName} is not implemented on derrived test class");

                    Type enumerableType = typeof(IEnumerable<>).MakeGenericType(type);

                    Assume.That(testDataMethod.ReturnType == enumerableType, $"Expected return type of {methodName} does not match expected {enumerableType.Name}");

                }
            }
            else
            { 
                bool oneExist = false;

                foreach (Tuple<string, Type> assumedType in m_AssumedStaticMethods)
                {
                    string methodName = assumedType.Item1;
                    Type type = assumedType.Item2;
                    var testDataMethod = GetType().GetMethod(methodName, BindingFlags.Static | BindingFlags.Public);
                    if( testDataMethod != null )
                    {
                        Type enumerableType = typeof(IEnumerable<>).MakeGenericType(type);
                        Assume.That(testDataMethod.ReturnType == enumerableType, $"Expected return type of {methodName} does not match expected {enumerableType.Name}");
                        oneExist = true;
                    }

                }

                Assume.That(oneExist, $"Expected at least one of the static members {string.Join(", ", m_AssumedStaticMethods.Select(x => x.Item1))} to be implemented on derrived test class");

            }
        }
    }
}
