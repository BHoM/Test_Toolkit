using BH.Engine.Base;
using BH.Engine.Test.CodeCompliance;
using BH.oM.Test.CodeCompliance;
using BH.oM.Test.CodeCompliance.Attributes;
using BH.oM.Test.Results;
using BH.oM.Test;
using BH.Tests.Setup.TestBases;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Engine.Internal.Backports;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace BH.Tests.Setup
{
    [TestFixtureSource("TestMethods")]
    //[TestFixtureSource("TestFiles")]
    public abstract class ComplianceTestBase2 : BaseTestBase
    {
        //private SyntaxNode m_Node;

        //public ComplianceTestBase2(string filePath) : base("TestFiles", typeof(string)) 
        //{
        //    string file;
        //    using (StreamReader sr = new StreamReader(filePath))
        //    {
        //        file = sr.ReadToEnd();
        //    }

        //    m_Node = BH.Engine.Test.CodeCompliance.Convert.ToSyntaxTree(file, filePath).GetRoot();
        //}

        private MethodInfo m_Method;

        public ComplianceTestBase2(string methodName) : base("TestFiles", typeof(string))
        {
            m_Method = m_checkMethods[methodName];
        }

        private static Dictionary<string, SyntaxNode> m_Nodes = new Dictionary<string, SyntaxNode>();
        private static object m_nodeLock = new object();

        private static SyntaxNode GetNode(string fileName)
        {
            if( m_Nodes.TryGetValue(fileName, out SyntaxNode node))
                return node;

            lock (m_nodeLock)
            {
                if (m_Nodes.TryGetValue(fileName, out node))
                    return node;
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

        //[TestCaseSource("TestMethods")]
        [TestCaseSource("TestFiles")]
        public void TestCompliance(string fileName)
        {
            SyntaxNode node = GetNode(fileName);
            MethodInfo method = m_Method;
            Assume.That(method != null);
            Assume.That(node != null);
            Assume.That(System.IO.Path.GetFileName(node.SyntaxTree.FilePath), Is.Not.EqualTo("AssemblyInfo.cs"), "Skipping AssemblyInfo.cs files.");
            
            //Assume.That(method.GetCustomAttributes<ConditionAttribute>().Select(condition => condition.IPasses(node)), Is.All.True, $"Method {method.Name} is not applicable to be tested with the {node.SyntaxTree.FilePath}.");

            Assert.Multiple(() =>
            {
                CheckMethod(method, node, node.SyntaxTree.FilePath);
            });
        }

        private static void CheckMethod(MethodInfo method, SyntaxNode node, string filePath)
        {
            Type type = node.GetType();


            if (method.GetParameters()[0].ParameterType.IsAssignableFrom(type) &&
                    !(typeof(MemberDeclarationSyntax).IsAssignableFrom(node.GetType())
                    && ((MemberDeclarationSyntax)node).IsDeprecated()) &&
                    method.GetCustomAttributes<ConditionAttribute>().All(condition => condition.IPasses(node)))
            {
                Func<object[], object> fn = GetFunction(method);

                Span result = fn(new object[] { node }) as Span;
                if (result != null)
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
                CheckMethod(method, child, filePath);
            }

        }

       private static IEnumerable<string> TestMethods()
        {
            if (m_checkMethods == null)
            {
                lock (m_compileLock)
                {
                    if (m_checkMethods == null)
                        m_checkMethods = new Dictionary<string, MethodInfo>();

                    foreach (var method in BH.Engine.Test.CodeCompliance.Query.AllChecks().Distinct())
                    {
                        string key = method.Name + ": " + method.GetParameters().First().ParameterType.Name;
                        m_checkMethods[key] = method;
                    }
                }
            }

            return m_checkMethods.Keys;
        }

        public static IEnumerable<string> GetCsFiles(string folder)
        {
            if(m_testFiles.TryGetValue(folder, out List<string> files))
                return files;
            lock (m_fileLock)
            {
                if (m_testFiles.TryGetValue(folder, out files))
                    return files;

                files = Query.GetFiles(System.IO.Path.Combine(Query.CurrentRepoFolder(), folder), "*.cs", true).ToList();
                m_testFiles[folder] = files;
                return files;
            }

        }

        private static Dictionary<string, List<string>> m_testFiles = new Dictionary<string, List<string>>();
        private static object m_fileLock = new object();

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

        private static Dictionary<string, MethodInfo> m_checkMethods = null;

        private static Dictionary<MethodInfo, Func<object[], object>> m_checkMethodFunctions = new Dictionary<MethodInfo, Func<object[], object>>();
        private static object m_compileLock = new object();
    }
}
