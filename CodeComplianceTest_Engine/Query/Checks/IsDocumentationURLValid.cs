/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2023, the respective contributors. All rights reserved.
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

using BH.oM.Base.Attributes;
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
using Microsoft.CodeAnalysis;

namespace BH.Engine.Test.CodeCompliance.Checks
{
    public static partial class Query
    {
        [Message("Documentation link does not link to a valid web resource", "IsDocumentationURLValid")]
        [ErrorLevel(TestStatus.Error)]
        [Path(@"([a-zA-Z0-9]+)_(oM|Engine|Adapter)\\.*\.cs$")]
        [Path(@"([a-zA-Z0-9]+)_Engine\\Objects\\.*\.cs$", false)]
        [Path(@"([a-zA-Z0-9]+)_Tests\\.*\.cs$", false)]
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
                HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
                request.Method = "HEAD";
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                response.Close();
                if (response.StatusCode != HttpStatusCode.OK)
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