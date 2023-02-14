using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace BH.oM.Test.NUnit
{
    [XmlRoot(ElementName = "assertions")]
    public class Assertions
    {

        [XmlElement(ElementName = "assertion")]
        public Assertion Assertion { get; set; }
    }
}
