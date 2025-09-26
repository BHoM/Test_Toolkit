using BH.Engine.Base;
using BH.Engine.Test.CodeCompliance;
using BH.oM.Test;
using BH.oM.Test.CodeCompliance;
using BH.oM.Test.CodeCompliance.Attributes;
using BH.oM.Test.Results;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace BH.Tests.Compliance
{
    [Description("Class that executes compliance checks for .cs files. The methods provided to the test fixture are the various check methods available in CodeComplianceTest_Engine." +
                 "This will isntanciate a test class for each test method, and then the files are all checked against all method")]
    [TestFixtureSource(nameof(TestMethods))]
    public class CsFile
    {
        /***************************************************/
        /**** Private fields                            ****/
        /***************************************************/

        private MethodInfo m_Method;    //Check method

        /***************************************************/
        /**** Constructor                               ****/
        /***************************************************/

        public CsFile(MethodInfo method)
        {
            m_Method = method;
        }

        /***************************************************/
        /**** Test methods                              ****/
        /***************************************************/

        [Description("Test method being executed for the current test method, and current .cs file.")]
        [TestCaseSource(typeof(BH.Tests.Setup.Query), nameof(BH.Tests.Setup.Query.TestFilesCs))]
        public void TestCompliance(string fileName)
        {
            SyntaxNode node = GetNode(fileName);    
            MethodInfo method = m_Method;
            Assume.That(method != null);
            Assume.That(node != null);
            Assume.That(System.IO.Path.GetFileName(node.SyntaxTree.FilePath), Is.Not.EqualTo("AssemblyInfo.cs"), "Skipping AssemblyInfo.cs files.");
            
            if (method.GetCustomAttributes<PathAttribute>().All(condition => condition.IPasses(node)))  //Prefilter out files that don't match the path condition. Not really required (handled internally as well) but speeds up the execution
            {
                Assert.Multiple(() =>   //Allow multiple assertions to be raised for the same file
                {
                    CheckMethod(method, node, node.SyntaxTree.FilePath);
                });
            }
        }

        /***************************************************/

        [Description("Main validator method handling checking and assertion raising for the provided method and node.")]
        private static void CheckMethod(MethodInfo method, SyntaxNode node, string filePath)
        {
            Type type = node.GetType();

            if (method.GetParameters()[0].ParameterType.IsAssignableFrom(type) &&   //Check that the method can handle this type of node
                    !(typeof(MemberDeclarationSyntax).IsAssignableFrom(type) // Ignore deprecated members
                    && ((MemberDeclarationSyntax)node).IsDeprecated()) &&
                    method.GetCustomAttributes<ConditionAttribute>().All(condition => condition.IPasses(node))) //Check all conditions are met
            {
                Func<object[], object> fn = GetFunction(method);    //Get compiled function of the compliance test method

                Span result = fn(new object[] { node }) as Span;    //Execute the method
                if (result != null)     //If a result is returned, then raise the appropriate assertion. Null return means no issue found
                {
                    string message = method.GetCustomAttribute<MessageAttribute>()?.Message ?? "";
                    string documentation = method.GetCustomAttribute<MessageAttribute>()?.DocumentationLink ?? "";
                    TestStatus errLevel = method.GetCustomAttribute<ErrorLevelAttribute>()?.Level ?? TestStatus.Error;
                    var error = BH.Engine.Test.CodeCompliance.Create.Error(message, BH.Engine.Test.CodeCompliance.Create.Location(filePath, result.ToLineSpan(node.SyntaxTree.GetRoot().ToFullString())), documentation, errLevel, method.Name);

                    string finalMessage = error.ToText();

                    switch (errLevel)
                    {
                        case TestStatus.Pass:
                            Console.WriteLine(finalMessage);
                            break;
                        case TestStatus.Warning:
                            Assert.Warn(finalMessage);
                            break;
                        default:
                        case TestStatus.Error:
                            Assert.Fail(finalMessage);
                            break;
                    }
                }
            }

            foreach (var child in node.ChildNodes())
            {
                CheckMethod(method, child, filePath);   //Recurse through all child nodes
            }

        }

        /***************************************************/
        /**** Test data methods                         ****/
        /***************************************************/

        [Description("Returns the test methods available in CodeComplianceTest_Engine to be executed as test fixtures.")]
        private static IEnumerable<TestFixtureData> TestMethods()
        {
            foreach (var methodGroup in BH.Engine.Test.CodeCompliance.Query.AllChecks().Distinct().GroupBy(x => x.Name))
            {
                if (methodGroup.Count() == 1)
                {
                    yield return new TestFixtureData(methodGroup.First()).SetArgDisplayNames(methodGroup.First().Name);
                }
                else
                {
                    foreach (var method in methodGroup)
                    {
                        string key = method.Name + ": " + method.GetParameters().First().ParameterType.Name.Replace("DeclarationSyntax", "");
                        yield return new TestFixtureData(method).SetArgDisplayNames(key);
                    }
                }

            }
        }

        /***************************************************/
        /**** Extraction, compilation and cashing       ****/
        /***************************************************/

        [Description("Returns the syntax node for the provided file name, using a cache to avoid reloading and reparsing files.")]
        private static SyntaxNode GetNode(string fileName)
        {
            if (m_Nodes.TryGetValue(fileName, out SyntaxNode node))
                return node;

            lock (m_nodeLock)
            {
                if (m_Nodes.TryGetValue(fileName, out node))
                    return node;
                fileName = System.IO.Path.GetFullPath(fileName);
                string file;
                using (StreamReader sr = new StreamReader(fileName))
                {
                    file = sr.ReadToEnd();
                }
                node = BH.Engine.Test.CodeCompliance.Convert.ToSyntaxTree(file, fileName).GetRoot();
                m_Nodes[fileName] = node;
                return node;
            }
        }

        /***************************************************/

        [Description("Returns a compiled function for the provided method, using a cache to avoid recompiling methods.")]
        private static Func<object[], object> GetFunction(MethodInfo method)
        {
            Func<object[], object> fn;
            if (m_checkMethodFunctions.TryGetValue(method, out fn))
                return fn;

            fn = method.ToFunc();
            lock (m_compileLock)
            {
                m_checkMethodFunctions[method] = fn;
            }
            return fn;
        }

        /***************************************************/
        /**** Caches and lock fields                    ****/
        /***************************************************/

        private static Dictionary<string, SyntaxNode> m_Nodes = new Dictionary<string, SyntaxNode>();
        private static object m_nodeLock = new object();

        private static Dictionary<MethodInfo, Func<object[], object>> m_checkMethodFunctions = new Dictionary<MethodInfo, Func<object[], object>>();
        private static object m_compileLock = new object();

        /***************************************************/
    }
}
