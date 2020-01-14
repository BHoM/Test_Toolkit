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
using BH.oM.Structure.Elements;

using BH.oM.Test.Results;
using BH.Adapter;
using BH.oM.Structure.Requests;
using BH.oM.Geometry;

namespace BH.Engine.Test.Interoperability
{
    public static partial class Compute
    {
        
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

            IEnumerable<ILoad> loads = objects.Where(x => x is ILoad).Cast<ILoad>();
            IEnumerable<IBHoMObject> nonLoads = objects.Where(x => !(x is ILoad));

            //2. Push non+load objects        

            List<IBHoMObject> pushedObjects = new List<IBHoMObject>();

            pushedObjects = adapter.Push(nonLoads).Cast<IBHoMObject>().ToList();

            //3. Re+assign pushed elements to loads

            foreach (ILoad load in loads)
            {
                ReassignObjectsToLoad(load as dynamic, pushedObjects);
            }

            // Push loads

            adapter.Push(loads);

            // Run model
                                  
            adapter.Execute("Analyze");

            //Invoke checking method
            if(checkingMethod != null)
                checkingMethod.Invoke(null, new object[] { adapter, objects });          
            

        }

        /***************************************************/
        /**** Private Methods                           ****/
        /***************************************************/


        private static Load<T> ReassignObjectsToLoad<T>(Load<T> load, IEnumerable<IBHoMObject> objects) where T : IBHoMObject
        {
            IEnumerable<T> filteredObjects = objects.OfType<T>().Cast<T>();
            for (int i = 0; i < load.Objects.Elements.Count; i++)
            {
                load.Objects.Elements[i] = filteredObjects.Single(x => x.BHoM_Guid == load.Objects.Elements[i].BHoM_Guid);
            }
            return load;
        }

        /****************************************************/


        private static ILoad ReassignObjectsToLoad<T>(ILoad load, IEnumerable<IBHoMObject> objects) where T : IBHoMObject
        {
            return load;
        }

        /***************************************************/

        public static bool CheckShearForceAndMoments(BHoMAdapter adapter, IEnumerable<IBHoMObject> objects)
        {
            //Pull Results from robot
            BarResultRequest request = new BarResultRequest();
            List<IBHoMObject> pulledResults = new List<IBHoMObject>();
            
            pulledResults = adapter.Pull(request).Cast<IBHoMObject>().ToList();

            //Make "hand calc" on the assumed results based on the "model" i.e., the list of BHoMObjects and some maths

            IEnumerable<BarStress> barStress = pulledResults.Where(x => (x is BarStress)).Cast<BarStress>();

            double test = (barStress as BarStress).ShearZ;
            //List<double> test2 = new List<double>((barStress as BarStress).Position);
            foreach (double pos in (barStress as BarStress).Position)
            {

            }


            foreach (BarStress result in barStress)
            {
                double AdapterResultShearZ = result.ShearZ;
                    
            }


            ///Handcalc

            IEnumerable<Bar> bar = objects.Where(x => (x is Bar)).Cast<Bar>();
            double barLength = (bar as Bar).EndNode.Position.CompareTo((bar as Bar).StartNode.Position);

            //foreach (bar bara in bars)
            //{
            //    double barlength = bara.startnode.position.compareto(bara.endnode.position);
            //}


            //Compare the results


            return false;

        }

    }
}
