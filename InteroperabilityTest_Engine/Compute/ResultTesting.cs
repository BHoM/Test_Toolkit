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
using BH.oM.Adapter.Commands;

namespace BH.Engine.Test.Interoperability
{
    public static partial class Compute
    {

        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Compares the results extraced from an adapter to a handcalculation of checkingMethod")]
        [Input("adapter", "The instance of the adapter to test for")]
        [Input("objects", "The type of object to test. This will use test sets in the Dataset library")]
        [Input("active", "Toggles whether to run the test")]
        [Output("analysisResults", "Diffing results outlining any differences found between the pushed and pulled objects. Also contains any error or warning messages returned by the adapter in the process")]
        public static CustomObject CheckAnalysisResults(BHoMAdapter adapter, IEnumerable<IBHoMObject> objects, MethodInfo checkingMethod = null, bool active = false)
        {
            if (active == false)
            {
                return new CustomObject();
            }

            CustomObject results = new CustomObject();

            //1. Filter out non+load objects

            IEnumerable<ILoad> loads = objects.Where(x => x is ILoad).Cast<ILoad>();
            IEnumerable<IBHoMObject> nonLoads = objects.Where(x => !(x is ILoad));

            //Clear everything in the analysisprogram before pushing?

            //2. Push non+load objects        

            List<IBHoMObject> pushedObjects = new List<IBHoMObject>();

            pushedObjects = adapter.Push(nonLoads).Cast<IBHoMObject>().ToList();

            results.CustomData["PushObjectsSuccess"] = pushedObjects.Count == nonLoads.Count();

            //3. Re+assign pushed elements to loads

            foreach (ILoad load in loads)
            {
                ReassignObjectsToLoad(load as dynamic, pushedObjects);
            }

            // Push loads

            results.CustomData["PushObjectsSuccess"] = adapter.Push(loads).Count() == loads.Count();

            // Run model
            
            AnalyseLoadCases commandAnalyse = new AnalyseLoadCases() {LoadCases = nonLoads.OfType<ICase>() };
            
            results.CustomData["AnalysisRunSuccess"] = adapter.Execute(commandAnalyse).Item2;

            //Invoke checking method

            //      if(checkingMethod != null)
            //      checkingMethod.Invoke(null, new object[] { adapter, objects, loads });                   
            
            results.CustomData["TestResults"] = CheckShearForceAndMoments(adapter, nonLoads, loads); // rename based on what method ran?

            
            //Close commandClose = new Close();
            //adapter.Execute(commandClose);
            return results;
            //Close adapter after sending results back?

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

        public static CustomObject CheckShearForceAndMoments(BHoMAdapter adapter, IEnumerable<IBHoMObject> objects, IEnumerable<ILoad> loads)
        {
            //Pull Results from robot

           // BarResultRequest request = new BarResultRequest() { ResultType = BarResultType.BarForce};
            BarResultRequest request2 = new BarResultRequest() { Cases = new List<object>() {objects.Single((x => x is Loadcase)) } };
            //BarResultRequest request2 = new BarResultRequest() {  };
            List<BarForce> pulledResults = new List<BarForce>();

            pulledResults = adapter.Pull(request2).Cast<BarForce>().ToList();
            bool pullSuccess = pulledResults.Count > 0;

            //
            for (int i = 0; i < pulledResults.Count; i++)
            {
                //if (pulledResults.ElementAt(i).ResultCase() == loads.Cast<BarUniformlyDistributedLoad>.)
                //{

                //}
                
            }
            //loads.Cast<BarUniformlyDistributedLoad>.Loadcase


            //IEnumerable<BarStress> barStress = pulledResults.Where(x => (x is BarStress)).Cast<BarStress>();

            List<double> positions = new List<double>();
            List<double> FY = new List<double>();
            List<double> FZ = new List<double>();
            List<double> MY = new List<double>();
            List<double> MZ = new List<double>();

            foreach (BarForce result in pulledResults)
            {
                
                positions.Add(result.Position);
                FY.Add(result.FY);
                FZ.Add(result.FZ);
                MY.Add(result.MY);
                MZ.Add(result.MZ);
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
                double x = positions[i] * barLength;
                HandCshearY.Add(yForce * (x - 3 * barLength / 8));
                HandCshearZ.Add(zForce * (x - 3 * barLength / 8));
                HandCmomentY.Add(zForce * ((Math.Pow(x, 2) / 2) - 3 * barLength * x / 8));
                HandCmomentZ.Add(yForce * ((Math.Pow(x, 2) / 2) - 3 * barLength * x / 8));
            }

            //Compare the results
            List<double> FYDiff = new List<double>();
            List<double> FZDiff = new List<double>();
            List<double> MYDiff = new List<double>();
            List<double> MZDiff = new List<double>();

            //double shearYDiff; double shearZDiff; double momentYDiff; double momentZDiff;
            //int fails = 0; double diffAllowence = 0.1;

            for (int i = 0; i < positions.Count; i++)
            {

                if (FY[i] == 0 && HandCshearY[i] == 0)
                {
                    FYDiff.Add(0);
                }
                else
                {
                    FYDiff.Add(FY[i] / HandCshearY[i] - 1);
                }
                if (FZ[i] == 0 && HandCshearZ[i] == 0)
                {
                    FZDiff.Add(0);
                }
                else
                {
                    FZDiff.Add(FZ[i] / HandCshearZ[i] - 1);
                }
                if (MY[i] == 0 && HandCmomentY[i] == 0)
                {
                    MYDiff.Add(0);
                }
                else
                {
                    MYDiff.Add(MY[i] / HandCmomentY[i] - 1);
                }
                if (MZ[i] == 0 && HandCmomentZ[i] == 0)
                {
                    MZDiff.Add(0);
                }
                else
                {
                    MZDiff.Add(MZ[i] / HandCmomentZ[i] - 1);
                }
                //FYDiff.Add(FY[i] / HandCshearY[i]);
                //FZDiff.Add(FZ[i] / HandCshearZ[i]);
                //MYDiff.Add(MY[i] / HandCmomentY[i]);
                //MZDiff.Add(MZ[i] / HandCmomentZ[i]);                

                //if (Math.Abs(FYDiff[i]) > diffAllowence)         // shearYDiff < 1-diffAllowence || shearYDiff > 1+diffAllowence
                //{
                //    fails += 1;
                //}
                //if (Math.Abs(FZDiff[i]) > diffAllowence)
                //{
                //    fails += 1;
                //}
                //if (Math.Abs(MYDiff[i]) > diffAllowence)
                //{
                //    fails += 1;
                //}
                //if (Math.Abs(MZDiff[i]) > diffAllowence)
                //{
                //    fails += 1;
                //}
            }

            //string numberOfFails = string.Format("{0} of the checks failed.", fails);
            //if (fails > 0)
            //{
            //    return false;
            //}
            //else
            //{
            //    return true;
            //}
            CustomObject results = new CustomObject();

            results.CustomData["PullSuccess"] = pullSuccess;
            results.CustomData["FYDifference"] = FYDiff;
            results.CustomData["FZDifference"] = FZDiff;
            results.CustomData["MYDifference"] = MYDiff;
            results.CustomData["MZDifference"] = MZDiff;

            return results;

        }

    }
}
