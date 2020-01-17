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
using BH.Engine.Geometry;

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
            
            //OBS. this only work if the elements are not already pushed!

            //1. Filter out non+load objects

            IEnumerable<ILoad> loads = objects.Where(x => x is ILoad).Cast<ILoad>();
            IEnumerable<IBHoMObject> nonLoads = objects.Where(x => !(x is ILoad));

            //2. Push non+load objects        

            List<IBHoMObject> pushedObjects = new List<IBHoMObject>();

            pushedObjects = adapter.Push(nonLoads).Cast<IBHoMObject>().ToList();

            //3. Re+assign pushed elements to loads

            foreach (ILoad load in loads)
            {
                ReassignObjectsToLoad(load as dynamic, pushedObjects); // won't work
            }

            // Push loads

            adapter.Push(loads);

            // Run model
                                  
            adapter.Execute("Analyze");
            adapter.Execute("Run Analysis");

            //Invoke checking method
            //if(checkingMethod != null)
            //    checkingMethod.Invoke(null, new object[] { adapter, objects, loads });                   

            bool test = CheckShearForceAndMoments(adapter, nonLoads, loads);
            //(loads as BarUniformlyDistributedLoad).Force.
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

        public static bool CheckShearForceAndMoments(BHoMAdapter adapter, IEnumerable<IBHoMObject> objects, IEnumerable<ILoad> loads)
        {
            //Pull Results from robot
            BarResultRequest request = new BarResultRequest();
            GlobalResultRequest request2 = new GlobalResultRequest();            
            request.Divisions = 4;
            List<IBHoMObject> pulledResults = new List<IBHoMObject>();
                        
            pulledResults = adapter.Pull(request).Cast<IBHoMObject>().ToList();
            pulledResults = adapter.Pull(request2).Cast<IBHoMObject>().ToList();
            
            //Make "hand calc" on the assumed results based on the "model" i.e., the list of BHoMObjects and some maths

            //adapter results
            IEnumerable<BarStress> barStress = pulledResults.Where(x => (x is BarStress)).Cast<BarStress>();

            List<double> positions = new List<double>();
            List<double> shearY = new List<double>();
            List<double> shearZ = new List<double>();            
            List<double> momentY = new List<double>();
            List<double> momentZ = new List<double>();

            foreach (BarStress result in barStress)
            {
                positions.Add(result.Position);
                shearY.Add(result.ShearY);
                shearZ.Add(result.ShearZ);
                momentY.Add(result.BendingY_Top);
                momentZ.Add(result.BendingZ_Top);
            }

            //Handcalc
            //W
            IEnumerable<BarUniformlyDistributedLoad> uniformlyDistributedLoads = loads.Where(x => (x is BarUniformlyDistributedLoad)).Cast<BarUniformlyDistributedLoad>(); 
            double yForce = 0; double zForce = 0;

            for (int i = 0; i < uniformlyDistributedLoads.Count(); i++)
            {
                yForce += uniformlyDistributedLoads.ElementAt(i).Force.Y;
                zForce += uniformlyDistributedLoads.ElementAt(i).Force.Z;
            }

            //L
            IEnumerable<Bar> bar = objects.Where(x => (x is Bar)).Cast<Bar>(); //could maybe pull this instead, then wouldn't need "objects" input
            double barLength = bar.ElementAt(0).EndNode.Position.Distance(bar.ElementAt(0).StartNode.Position);

            //Result
            List<double> HandCshearY = new List<double>();
            List<double> HandCshearZ = new List<double>();
            List<double> HandCmomentY = new List<double>();
            List<double> HandCmomentZ = new List<double>();

            for (int i = 0; i < positions.Count; i++)
            {
                HandCshearY.Add(yForce * (positions[i] -3) * barLength / 8);
                HandCshearZ.Add(zForce * (positions[i] -3) * barLength / 8);
                HandCmomentY.Add(zForce * ((Math.Pow(positions[i], 2) / 2) - 3 * barLength * positions[i] / 8));
                HandCmomentZ.Add(yForce * ((Math.Pow(positions[i], 2) / 2) - 3 * barLength * positions[i] / 8));
            }

            //Compare the results
            //List<double> shearYDiff = new List<double>();
            //List<double> shearZDiff = new List<double>();
            //List<double> momentYDiff = new List<double>();
            //List<double> momentZDiff = new List<double>();

            double shearYDiff; double shearZDiff; double momentYDiff; double momentZDiff; int fails = 0;

            for (int i = 0; i < positions.Count; i++)
            {
                //shearYDiff.Add(shearY[i] / HandCshearY[i]);
                //shearZDiff.Add(shearZ[i] / HandCshearZ[i]);
                //momentYDiff.Add(momentY[i] / HandCmomentY[i]);
                //momentZDiff.Add(momentZ[i] / HandCmomentZ[i]);
                shearYDiff = shearY[i] / HandCshearY[i];
                shearZDiff = shearZ[i] / HandCshearZ[i];
                momentYDiff = momentY[i] / HandCmomentY[i];
                momentZDiff = momentZ[i] / HandCmomentZ[i];
                double diffAllowence = 0.1;

                if (Math.Abs(shearYDiff - 1) < diffAllowence)         // shearYDiff < 1-diffAllowence || shearYDiff > 1+diffAllowence
                {
                    fails += 1;
                }
                if (Math.Abs(shearZDiff - 1) < diffAllowence)
                {
                    fails += 1;
                }
                if (Math.Abs(momentYDiff - 1) < diffAllowence)
                {
                    fails += 1;
                }
                if (Math.Abs(momentZDiff - 1) < diffAllowence)
                {
                    fails += 1;
                }
            }

            //string numberOfFails = string.Format("{0} of the checks failed.", fails);
            if (fails > 0)
            {
                return false;
            }
            else
            {
                return true;
            }            

        }

    }
}
