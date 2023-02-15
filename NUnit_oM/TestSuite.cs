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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace BH.oM.Test.NUnit
{
    [XmlRoot(ElementName = "test-suite")]
    public class TestSuite
    {

        [XmlElement(ElementName = "failure")]
        public virtual Failure Failure { get; set; }

        [XmlElement(ElementName = "test-case")]
        public virtual List<TestCase> TestCases { get; set; }

        [XmlAttribute(AttributeName = "type")]
        public virtual string Type { get; set; }

        [XmlAttribute(AttributeName = "id")]
        public virtual string ID { get; set; }

        [XmlAttribute(AttributeName = "name")]
        public virtual string Name { get; set; }

        [XmlAttribute(AttributeName = "fullname")]
        public virtual string FullName { get; set; }

        [XmlAttribute(AttributeName = "classname")]
        public virtual string Classname { get; set; }

        [XmlAttribute(AttributeName = "runstate")]
        public virtual string RunState { get; set; }

        [XmlAttribute(AttributeName = "testcasecount")]
        public virtual int TestCaseCount { get; set; }

        [XmlAttribute(AttributeName = "result")]
        public virtual string Result { get; set; }

        [XmlAttribute(AttributeName = "site")]
        public virtual string Site { get; set; }

        [XmlAttribute(AttributeName = "start-time")]
        public virtual DateTime StartTime { get; set; }

        [XmlAttribute(AttributeName = "end-time")]
        public virtual DateTime EndTime { get; set; }

        [XmlAttribute(AttributeName = "duration")]
        public virtual double Duration { get; set; }

        [XmlAttribute(AttributeName = "total")]
        public virtual int Total { get; set; }

        [XmlAttribute(AttributeName = "passed")]
        public virtual int Passed { get; set; }

        [XmlAttribute(AttributeName = "failed")]
        public virtual int Failed { get; set; }

        [XmlAttribute(AttributeName = "warnings")]
        public virtual int Warnings { get; set; }

        [XmlAttribute(AttributeName = "inconclusive")]
        public virtual int Inconclusive { get; set; }

        [XmlAttribute(AttributeName = "skipped")]
        public virtual int Skipped { get; set; }

        [XmlAttribute(AttributeName = "asserts")]
        public virtual int Asserts { get; set; }

        [XmlText]
        public virtual string Text { get; set; }

        [XmlElement(ElementName = "test-suite")]
        public virtual TestSuite Child { get; set; }

        [XmlElement(ElementName = "environment")]
        public virtual Environment Environment { get; set; }

        [XmlElement(ElementName = "settings")]
        public virtual Settings Settings { get; set; }

        [XmlElement(ElementName = "properties")]
        public virtual Properties Properties { get; set; }
    }
}

