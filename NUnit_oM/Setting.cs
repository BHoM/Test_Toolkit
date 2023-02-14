using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace BH.oM.Test.NUnit
{
    [XmlRoot(ElementName = "setting")]
    public class Setting
    {

        [XmlAttribute(AttributeName = "name")]
        public virtual string Name { get; set; }

        [XmlAttribute(AttributeName = "value")]
        public virtual int Value { get; set; }
    }
}
