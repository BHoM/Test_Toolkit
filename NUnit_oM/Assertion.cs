using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace BH.oM.Test.NUnit
{
    [XmlRoot(ElementName = "assertion")]
    public class Assertion
    {

        [XmlElement(ElementName = "message")]
        public virtual string Message { get; set; }

        [XmlElement(ElementName = "stack-trace")]
        public virtual string Stacktrace { get; set; }

        [XmlAttribute(AttributeName = "result")]
        public virtual string Result { get; set; }

        [XmlText]
        public virtual string Text { get; set; }
    }
}
