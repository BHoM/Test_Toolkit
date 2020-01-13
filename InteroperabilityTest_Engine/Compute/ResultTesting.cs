/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2019, the respective contributors. All rights reserved.
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
using System.Reflection;
using BH.oM.Reflection.Attributes;
using BH.oM.Base;
using BH.oM.Data.Requests;
using BH.oM.Data.Collections;
using BH.oM.Structure.Loads;
using BH.oM.Structure.Results;

using BH.oM.Test.Results;
using BH.Adapter;
using BH.oM.Structure.Requests;

namespace BH.Engine.Test.Interoperability
{
    public static partial class Compute
    {
        private static IRequest request;

        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Tests Pushing objects of a specific type, then pulling them back and comparing the objects. Returns Results outlining if the objects pulled are identical to the pushed ones, and if not, what properties are different between the two")]
        [Input("adapter", "The instance of the adapter to test for")]
        [Input("objects", "The type of object to test. This will use test sets in the Dataset library")]
        [Input("config", "Config for the test. Not yet in use")]
        [Input("active", "Toggles whether to run the test")]
        [Output("diffingResults", "Diffing results outlining any differences found between the pushed and pulled objects. Also contains any error or warning messages returned by the adapter in the process")]
        public static void CheckAnalysisResults(BHoMAdapter adapter, IEnumerable<IBHoMObject> objects, MethodInfo checkingMethod = null, bool active = false)
        {

            //1. Filter out non+load objects

            IEnumerable<IBHoMObject> loads = objects.Where(x => x is ILoad);
            IEnumerable<IBHoMObject> noneLoads = objects.Where(x => !(x is ILoad));

            //2. Push non+load objects        

            List<IBHoMObject> pushedObjects = new List<IBHoMObject>();

            pushedObjects = adapter.Push(noneLoads).Cast<IBHoMObject>().ToList();
                       
            //3. Re+assign pushed elements to loads
            
            for (int i = 0; i < loads.Count(); i++)
            {
                if (loads.ElementAt(i) is Load<IBHoMObject>)
                {                    
                    (loads.ElementAt(i) as Load<IBHoMObject>).Objects.Elements = (loads.ElementAt(i) as Load<IBHoMObject>).Objects.Elements.Select(x => pushedObjects.Where(y => y.Equals(x)).Single()).ToList();
                }
            }

            // Push loads

            adapter.Push(loads);

            // Run model
                                  
            adapter.Execute("Analyse");

            //Invoke checking method

            double diffingPrecentage;
            
            BarResultRequest request = new BarResultRequest();
            
                        
            IEnumerable<IBHoMObject> results = adapter.Pull(request).Cast<IBHoMObject>();
            
            if (checkingMethod.Name == "ShearMoment")
            {
                diffingPrecentage = CheckingMethodShear(results as BarStress);
            }


        }

        /***************************************************/
        /**** Private Methods                           ****/
        /***************************************************/

        //new equals method maybe?


        public static double CheckingMethodShear(BarStress results)
        {
            
            
            return 0;
        }


    }
}
