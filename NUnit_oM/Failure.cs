﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace BH.oM.Test.NUnit
{
    [XmlRoot(ElementName = "failure")]
    public class Failure
    {

        [XmlElement(ElementName = "message")]
        public string Message { get; set; }

        [XmlElement(ElementName = "stack-trace")]
        public string Stacktrace { get; set; }
    }
}
