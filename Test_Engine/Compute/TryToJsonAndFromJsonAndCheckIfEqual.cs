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

using BH.Engine.Diffing;
using BH.oM.Base;
using BH.oM.Base.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BH.Engine.Test
{
    public static partial class Compute
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Runs through the provided objects and tries to convert to Json. Then performs checks if the object can be deserialised, and if the de-serailised object is equal to the serialised version.")]
        [Input("objects", "Objects to try to convert ToJson.")]
        [MultiOutput(0, "validJson", "Json strings that pass all checks.")]
        [MultiOutput(1, "failingToJson", "Json strings show some failure of running the ToJson convert.")]
        [MultiOutput(2, "failingToJsonObject", "Obejcts that failed the ToJson convert.")]
        [MultiOutput(3, "failingFromJson", "Json for cases where the FromJson is failing.")]
        [MultiOutput(4, "failingFromJsonObjects", "Objects that had some failures going FromJson.")]
        [MultiOutput(5, "notEqualJson", "Jsons strings for objects not equal to incoming object after serialisation ToJson followed by deserialisation FromJson.")]
        [MultiOutput(6, "notEqualObjects", "Objects not equal to incoming object after serialisation ToJson followed by deserialisation FromJson.")]
        public static Output<List<string>, List<string>, List<object>, List<string>, List<object>, List<string>, List<object>> TryToJsonAndFromJsonAndCheckIfEqual(List<object> objects)
        {
            List<string> successfullJson = new List<string>();
            List<string> failingToJson = new List<string>();
            List<object> failingToJsonObjects = new List<object>();
            List<string> failingFromJson = new List<string>();
            List<object> failingFromJsonObjects = new List<object>();
            List<string> notEqualJson = new List<string>();
            List<object> notEqualObjects = new List<object>();


            foreach (object obj in objects)
            {
                string json;
                try
                {
                    json = BH.Engine.Serialiser.Convert.ToJson(obj);
                }
                catch (Exception e)
                {
                    failingToJson.Add("");
                    failingToJsonObjects.Add(obj);
                    BH.Engine.Base.Compute.RecordError(e, $"Failed ToJson for {obj.GetType()}");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(json))
                {
                    failingToJson.Add(json);
                    failingToJsonObjects.Add(obj);
                    continue;
                }



                object retObj;
                try
                {
                    retObj = BH.Engine.Serialiser.Convert.FromJson(json);
                }
                catch (Exception e)
                {
                    failingFromJson.Add(json);
                    failingFromJsonObjects.Add(obj);
                    BH.Engine.Base.Compute.RecordError(e, $"Failed FromJson for {obj.GetType()}");
                    continue;
                }


                if (retObj == null || retObj.GetType() == typeof(CustomObject))
                {
                    failingFromJson.Add(json);
                    failingFromJsonObjects.Add(obj);
                    continue;
                }

                bool isEqual;

                try
                {
                    isEqual = obj.IsEqual(retObj);
                }
                catch (Exception e)
                {
                    notEqualJson.Add(json);
                    notEqualObjects.Add(obj);
                    BH.Engine.Base.Compute.RecordError(e, $"Failed IsEqual for {obj.GetType()}");
                    continue;
                }

                if (isEqual)
                {
                    successfullJson.Add(json);
                }
                else
                {
                    notEqualJson.Add(json);
                    notEqualObjects.Add(obj);
                    continue;
                }

            }

            return new Output<List<string>, List<string>, List<object>, List<string>, List<object>, List<string>, List<object>>
            {
                Item1 = successfullJson,
                Item2 = failingToJson,
                Item3 = failingToJsonObjects,
                Item4 = failingFromJson,
                Item5 = failingFromJsonObjects,
                Item6 = notEqualJson,
                Item7 = notEqualObjects
            };
        }
    }
}
