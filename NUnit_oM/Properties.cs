using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace BH.oM.Test.NUnit
{
    [XmlRoot(ElementName = "properties")]
    public class Properties
    {

        [XmlElement(ElementName = "property")]
        public List<Property> Property { get; set; }
    }
}
