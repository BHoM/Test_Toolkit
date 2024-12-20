/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2025, the respective contributors. All rights reserved.
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BH.oM.Test;

using System.Net;
using System.Net.Http;

namespace BH.Engine.Test.CodeCompliance.Checks
{
    public static partial class Query
    {
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
                HttpClient client = new HttpClient();
                var checkingResponse = client.GetAsync(url).Result;
                if (checkingResponse.StatusCode != HttpStatusCode.OK)
                    return node.Span.ToSpan();
            }
            catch
            {
                return node.Span.ToSpan();
            }

            return null;
        }
    }
}

