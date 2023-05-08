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
using BH.oM.Test.NUnit;
using System.ComponentModel;
using System.Xml;
using System.Xml.Serialization;

namespace BH.Engine.Test.NUnit
{
    public static partial class Convert
    {
        [Description("Convert an XML node returned by NUnit test run to a BHoM TestRun object that reflects the structure of the XML.")]
        [Input("nunitRun", "The XML node returned by NUnit test run to convert.")]
        [Output("testRun", "The converted BHoM TestRun.")]
        public static TestRun ToTestRun(this XmlNode nunitRun)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(nunitRun.OuterXml);

            XmlSerializer serialiser = new XmlSerializer(typeof(TestRun));
            TestRun result = null;
            using (XmlNodeReader reader = new XmlNodeReader(xmlDoc))
            {
                result = serialiser.Deserialize(reader) as TestRun;
            }
            return result;
        }
    }
}
