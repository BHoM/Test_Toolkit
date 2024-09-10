/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2024, the respective contributors. All rights reserved.
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

namespace BH.Engine.Test.CodeCompliance.Checks
{
    public static partial class Query
    {
        [Message("Constructors on BHoM objects implementing the IImmutable interface must take every property which only has a get accessor as a parameter.", "HasValidConstructor")]
        [Path(@"([a-zA-Z0-9]+)_?oM\\.*\.cs$")]
        [Path(@"([a-zA-Z0-9]+)_Engine\\.*\.cs$", false)]
        [Path(@"([a-zA-Z0-9]+)_Adapter\\.*\.cs$", false)]
        [Path(@"([a-zA-Z0-9]+)_UI\\.*\.cs$", false)]
        [Path(@"([a-zA-Z0-9]+)_Tests\\.*\.cs$", false)]     //NUnit style projects
        [Path(@"([a-zA-Z0-9]+)_Test\\.*\.cs$", false)]      //Verification projects
        [ComplianceType("code")]
        [ErrorLevel(TestStatus.Error)]
        [Output("A span that represents where this error resides or null if there is no error")]
        public static Span HasValidConstructor(this ClassDeclarationSyntax node)
        {
            if (node == null || !node.HasAConstructor() || node.HasConstructor() != null)
                return null; //Either the class has no constructor, or the constructor it has is not valid based on Immutable interface

            List<ConstructorDeclarationSyntax> constructors = node.Members.Where(x => x.IsConstructor()).Select(x => x as ConstructorDeclarationSyntax).ToList();
            List<PropertyDeclarationSyntax> properties = node.Members.Where(x => x is PropertyDeclarationSyntax).Select(x => x as PropertyDeclarationSyntax).ToList();
            properties = properties.Where(x => x.AccessorList.Accessors.Where(y => y.ToString() == "set;").Count() == 0).ToList(); //Obtain the 'get' only properties
            List<string> propertyNames = properties.Select(x => x.Identifier.ToString().ToLower()).ToList();

            List<bool> results = new List<bool>();
            foreach(ConstructorDeclarationSyntax con in constructors)
            {
                List<string> parameters = con.ParameterList.Parameters.Select(x => x.Identifier.ToString().ToLower()).ToList();

                if (parameters.Count == 0)
                    continue; //Empty constructor which is allowed in some cases

                bool allPropertiesExistInParams = true;

                foreach(string s in propertyNames)
                    allPropertiesExistInParams &= parameters.Contains(s);

                results.Add(allPropertiesExistInParams);
            }

            if (results.Where(x => x).Count() == 0)
                return node.Span.ToSpan(); //None of the constructors contain all of the properties in its parameters, so the class is invalid

            return null;
        }
    }
}





