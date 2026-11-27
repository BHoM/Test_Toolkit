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

using BH.Engine.Diffing;
using BH.oM.Base;
using BH.oM.Base.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;

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
        [MultiOutput(1, "validJsonObjects", "Json objects that pass all checks.")]
        [MultiOutput(2, "failingToJson", "Json strings show some failure of running the ToJson convert.")]
        [MultiOutput(3, "failingToJsonObject", "Obejcts that failed the ToJson convert.")]
        [MultiOutput(4, "failingFromJson", "Json for cases where the FromJson is failing.")]
        [MultiOutput(5, "failingFromJsonObjects", "Objects that had some failures going FromJson.")]
        [MultiOutput(6, "notEqualJson", "Jsons strings for objects not equal to incoming object after serialisation ToJson followed by deserialisation FromJson.")]
        [MultiOutput(7, "notEqualObjects", "Objects not equal to incoming object after serialisation ToJson followed by deserialisation FromJson.")]
        public static Output<List<string>, List<object>, List<string>, List<object>, List<string>, List<object>, List<string>, List<object>> TryToJsonAndFromJsonAndCheckIfEqual(List<object> objects)
        {
            List<string> successfulJson = new List<string>();
            List<object> successfulJsonObjects = new List<object>();
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
                    // Added here, after the checks, so they run on the json as serialised.
                    successfulJson.Add(AddDeclaringAssembly(json, obj));
                    successfulJsonObjects.Add(obj);
                }
                else
                {
                    notEqualJson.Add(json);
                    notEqualObjects.Add(obj);
                    continue;
                }
            }

            return new Output<List<string>, List<object>, List<string>, List<object>, List<string>, List<object>, List<string>, List<object>>
            {
                Item1 = successfulJson,
                Item2 = successfulJsonObjects,
                Item3 = failingToJson,
                Item4 = failingToJsonObjects,
                Item5 = failingFromJson,
                Item6 = failingFromJsonObjects,
                Item7 = notEqualJson,
                Item8 = notEqualObjects
            };
        }

        /***************************************************/
        /**** Private Methods                           ****/
        /***************************************************/

        [Description("Records the assembly declaring the object's type on the record, as an `_asm` field.")]
        [Input("json", "Serialised record to add the field to.")]
        [Input("obj", "Object the record was serialised from.")]
        [Output("json", "The record, with the declaring assembly recorded on it.")]
        private static string AddDeclaringAssembly(string json, object obj)
        {
            if (string.IsNullOrEmpty(json) || obj == null)
                return json;

            // A method record already carries its declaring assembly, because its declaring type is
            // serialised assembly-qualified. Its top-level type is MethodBase, so recording the
            // assembly of that would be untrue.
            if (obj is System.Reflection.MethodBase)
                return json;

            string assembly;
            try
            {
                assembly = obj.GetType().Assembly.GetName().Name;
            }
            catch
            {
                // Left as it was. A guessed value would be indistinguishable from a real one.
                return json;
            }

            if (string.IsNullOrWhiteSpace(assembly))
                return json;

            // Before `_bhomVersion`, which Versioning_Engine appends last. Helpers.DescriptionFromJson
            // reads fixed quote-delimited indices, so the field has to sit past them.
            int at = json.LastIndexOf("\"_bhomVersion\"", StringComparison.Ordinal);
            if (at < 0)
                return json;

            return json.Substring(0, at) + $"\"_asm\" : \"{assembly}\", " + json.Substring(at);
        }

        /***************************************************/
    }
}


