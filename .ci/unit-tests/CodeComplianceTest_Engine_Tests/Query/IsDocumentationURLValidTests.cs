/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2026, the respective contributors. All rights reserved.
 *
 * Each contributor holds copyright over their respective contributions.
 * The project versioning (Git) records all such contribution source information.
 *                                           
 *                                                                              
 * The BHoM is free software: you can redistribute it and/or modify         
 * it under the terms of the GNU Lesser General Public License as published by  
 * the Free Software Foundation, either version 3.0 of the License, or          
 * (at your option) any later version.                                          
 *                                                                              
 * The BHoM is distributed in the hope that it will be useful,              
 * but WITHOUT ANY WARRANTY; without even the implied warranty of               
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the                 
 * GNU Lesser General Public License for more details.                          
 *                                                                            
 * You should have received a copy of the GNU Lesser General Public License     
 * along with this code. If not, see <https://www.gnu.org/licenses/lgpl-3.0.html>.      
 */

using BH.oM.Test.CodeCompliance;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace BH.Test.Engine.CodeCompliance
{
    public class IsDocumentationURLValidTests
    {
        /***************************************************/
        /**** Setup                                     ****/
        /***************************************************/

        [OneTimeSetUp]
        [Description("Starts a local HTTP listener on a free port, so that the tests do not depend on any external website.")]
        public void StartServer()
        {
            TcpListener portFinder = new TcpListener(IPAddress.Loopback, 0);
            portFinder.Start();
            int port = ((IPEndPoint)portFinder.LocalEndpoint).Port;
            portFinder.Stop();

            m_BaseUrl = $"http://localhost:{port}/";
            m_Server = new HttpListener();
            m_Server.Prefixes.Add(m_BaseUrl);
            m_Server.Start();
            m_ServerLoop = Task.Run(Serve);
        }

        /***************************************************/

        [OneTimeTearDown]
        [Description("Stops the local HTTP listener and waits up to five seconds for its request loop to finish.")]
        public void StopServer()
        {
            //Guarded so that a failure in StartServer is reported rather than hidden behind a NullReferenceException here
            if (m_Server?.IsListening == true)
                m_Server.Stop();

            m_Server?.Close();
            m_ServerLoop?.Wait(TimeSpan.FromSeconds(5));
        }

        /***************************************************/
        /**** Test Methods                              ****/
        /***************************************************/

        [Test]
        [Description("Checks that a URL returning 200 OK passes.")]
        public void OkResponsePasses()
        {
            Assert.That(Check(m_BaseUrl + "ok"), Is.Null);
        }

        /***************************************************/

        [Test]
        [Description("Checks that any successful response passes, using 204 No Content, rather than only 200 OK.")]
        public void NoContentResponsePasses()
        {
            Assert.That(Check(m_BaseUrl + "nocontent"), Is.Null);
        }

        /***************************************************/

        [Test]
        [Description("Checks that a URL returning 404 Not Found is reported as a failure.")]
        public void NotFoundResponseFails()
        {
            Assert.That(Check(m_BaseUrl + "missing"), Is.Not.Null);
        }

        /***************************************************/

        [Test]
        [Description("Checks that a site which refuses requests carrying no User-Agent passes, because the check sends one.")]
        public void SiteRejectingRequestsWithoutUserAgentPasses()
        {
            Assert.That(Check(m_BaseUrl + "needs-user-agent"), Is.Null);
        }

        /***************************************************/

        [Test]
        [Description("Checks that the User-Agent sent by the check identifies it as the BHoM compliance check.")]
        public void UserAgentIdentifiesTheCheck()
        {
            m_LastUserAgent = null;
            Check(m_BaseUrl + "needs-user-agent");
            Assert.That(m_LastUserAgent, Does.StartWith("BHoM-ComplianceCheck/"));
        }

        /***************************************************/

        [Test]
        [Description("Checks that a URL with a scheme other than HTTP or HTTPS is reported as a failure rather than throwing an exception.")]
        public void NonHttpSchemeFailsWithoutThrowing()
        {
            Span result = null;
            Assert.That(() => result = Check("ftp://example.com/"), Throws.Nothing);
            Assert.That(result, Is.Not.Null);
        }

        /***************************************************/

        //Relies on the .invalid top-level domain never resolving (RFC 6761). Behind an HTTP proxy the proxy's own error, such as a 502, still fails the check, so this passes for a different reason. A DNS resolver that answers for .invalid can make it fail.
        [Test]
        [Description("Checks that a URL whose host cannot be resolved is reported as a failure rather than throwing an exception.")]
        public void UnresolvableHostFailsWithoutThrowing()
        {
            Span result = null;
            Assert.That(() => result = Check("http://example.invalid/"), Throws.Nothing);
            Assert.That(result, Is.Not.Null);
        }

        /***************************************************/
        /**** Private Methods                           ****/
        /***************************************************/

        private static Span Check(string url)
        {
            string source = $"[DocumentationURL(\"{url}\", DocumentationType.Documentation)]\npublic enum Example {{ A }}";
            AttributeSyntax attribute = CSharpSyntaxTree.ParseText(source).GetRoot().DescendantNodes().OfType<AttributeSyntax>().Single();
            return BH.Engine.Test.CodeCompliance.Checks.Query.IsDocumentationURLValid(attribute);
        }

        /***************************************************/

        private void Serve()
        {
            while (m_Server.IsListening)
            {
                HttpListenerContext context;
                try
                {
                    context = m_Server.GetContext();
                }
                catch (Exception e) when (e is HttpListenerException || e is ObjectDisposedException)
                {
                    return; //Listener stopped by StopServer
                }

                string userAgent = context.Request.UserAgent;
                switch (context.Request.Url.AbsolutePath)
                {
                    case "/ok":
                        context.Response.StatusCode = 200;
                        break;
                    case "/nocontent":
                        context.Response.StatusCode = 204;
                        break;
                    case "/needs-user-agent":
                        m_LastUserAgent = userAgent;
                        context.Response.StatusCode = string.IsNullOrEmpty(userAgent) ? 403 : 200;
                        break;
                    default:
                        context.Response.StatusCode = 404;
                        break;
                }
                context.Response.Close();
            }
        }

        /***************************************************/
        /**** Private Fields                            ****/
        /***************************************************/

        private HttpListener m_Server;
        private Task m_ServerLoop;
        private string m_BaseUrl;
        private string m_LastUserAgent;

        /***************************************************/
    }
}
