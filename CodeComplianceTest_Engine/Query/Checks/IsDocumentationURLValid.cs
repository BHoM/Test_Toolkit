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
using BH.oM.Test.CodeCompliance.Attributes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BH.oM.Test;

using System.Net.Http;

namespace BH.Engine.Test.CodeCompliance.Checks
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Checks that the URL in a DocumentationURL attribute can be reached.\n" +
            "Fails if the request cannot be made, or if the response is not a success status.")]
        [Message("Documentation link does not link to a valid web resource", "IsDocumentationURLValid")]
        [ErrorLevel(TestStatus.Error)]
        [Path(@"([a-zA-Z0-9]+)_(oM|Engine|Adapter)\\.*\.cs$")]
        [Path(@"([a-zA-Z0-9]+)_Engine\\Objects\\.*\.cs$", false)]
        [Path(@"([a-zA-Z0-9]+)_Tests\\.*\.cs$", false)]     //NUnit style projects
        [Path(@"([a-zA-Z0-9]+)_Test\\.*\.cs$", false)]      //Verification projects
        [ComplianceType("documentation")]
        public static Span IsDocumentationURLValid(this AttributeSyntax node)
        {
            if (node == null)
                return null;

            if (node.Name.ToString() != "DocumentationURL")
                return null;

            var url = node.ArgumentList.Arguments[0].ToString().Replace("\"", "");

            try
            {
                using (HttpResponseMessage response = m_UrlClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead).GetAwaiter().GetResult())
                {
                    if (!response.IsSuccessStatusCode)
                        return node.Span.ToSpan();
                }
            }
            catch
            {
                return node.Span.ToSpan();
            }

            return null;
        }

        /***************************************************/
        /**** Private Methods                           ****/
        /***************************************************/

        private static HttpClient CreateUrlClient()
        {
            //Some sites refuse requests that carry no User-Agent, so identify the check and where to read about it
            HttpClient client = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
            client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "BHoM-ComplianceCheck/1.0 (+https://bhom.xyz/documentation/DevOps/Code%20Compliance%20and%20CI/Compliance%20Checks/IsDocumentationURLValid/)");
            //A single client lives as long as the host process, so do not keep connections, and their DNS results, open between checks
            client.DefaultRequestHeaders.ConnectionClose = true;
            return client;
        }

        /***************************************************/
        /**** Private Fields                            ****/
        /***************************************************/

        private static readonly HttpClient m_UrlClient = CreateUrlClient();

        /***************************************************/
    }
}


