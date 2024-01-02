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

using BH.oM.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace BH.oM.Test.NUnit
{
    [XmlRoot(ElementName = "environment")]
    public class Environment : IObject
    {
        [XmlAttribute(AttributeName = "framework-version")]
        public virtual string FrameworkVersion { get; set; }

        [XmlAttribute(AttributeName = "clr-version")]
        public virtual string ClrVersion { get; set; }

        [XmlAttribute(AttributeName = "os-version")]
        public virtual string OsVersion { get; set; }

        [XmlAttribute(AttributeName = "platform")]
        public virtual string Platform { get; set; }

        [XmlAttribute(AttributeName = "cwd")]
        public virtual string Cwd { get; set; }

        [XmlAttribute(AttributeName = "machine-name")]
        public virtual string MachineName { get; set; }

        [XmlAttribute(AttributeName = "user")]
        public virtual string User { get; set; }

        [XmlAttribute(AttributeName = "user-domain")]
        public virtual string UserDomain { get; set; }

        [XmlAttribute(AttributeName = "culture")]
        public virtual string Culture { get; set; }

        [XmlAttribute(AttributeName = "uiculture")]
        public virtual string Uiculture { get; set; }

        [XmlAttribute(AttributeName = "os-architecture")]
        public virtual string OsArchitecture { get; set; }
    }
}

