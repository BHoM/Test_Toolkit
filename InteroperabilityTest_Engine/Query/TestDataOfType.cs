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

using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using BH.oM.Base.Attributes;
using BH.oM.Base;

namespace BH.Engine.Test.Interoperability
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Extracts test data of a specific type from the DataSets/TestSets")]
        [Input("type", "Type of test data to extract.")]
        [MultiOutput(0, "Set Names", "The name of the extracted test sets.")]
        [MultiOutput(1, "Set Data", "The data of the extracted test sets. Each inner list corresponds to the set names.")]
        public static Output<List<string>, List<List<IBHoMObject>>> TestDataOfType(Type type)
        {
            if (type == null)
                return null;

            string nameSpace = type.Namespace;
            string discipline;

            string[] splitNameSpace = nameSpace.Split('.');

            if (splitNameSpace.Length < 3 || splitNameSpace[0] != "BH")
            {
                Base.Compute.RecordWarning("Only BHoMtypes are currently supported");
                return new Output<List<string>, List<List<IBHoMObject>>>() { Item1 = new List<string>(), Item2 = new List<List<IBHoMObject>>() };
            }
            else
            {
                discipline = splitNameSpace[2];
            }

            //Generate match string for libraries
            string match = "TestSets\\" + discipline + "\\" + type.Name + "\\";

            //Get and filter out libraries of interest
            List<string> libraryNames = Engine.Library.Query.LibraryNames().Where(x => x.Contains(match)).ToList();

            if (libraryNames == null || libraryNames.Count == 0)
            {
                Base.Compute.RecordWarning("No test set libraries found of the type  " + type.Name);
                return new Output<List<string>, List<List<IBHoMObject>>> { Item1 = new List<string>(), Item2 = new List<List<IBHoMObject>>() };
            }

            //Extract data from library
            List<List<IBHoMObject>> testData = libraryNames.Select(x => Library.Query.Library(x)).ToList();

            if (testData == null || testData.Count == 0)
            {
                Base.Compute.RecordWarning("Failed to extract test data of type  " + type.Name);
                return new Output<List<string>, List<List<IBHoMObject>>> { Item1 = new List<string>(), Item2 = new List<List<IBHoMObject>>() };

            }

            return new Output<List<string>, List<List<IBHoMObject>>>() { Item1 = libraryNames.Select(x => x.Split('\\').Last()).ToList(), Item2 = testData };
        }

        /***************************************************/
    }
}







