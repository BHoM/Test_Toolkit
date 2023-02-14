using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace BH.oM.Test.NUnit
{
    [XmlRoot(ElementName = "test-case")]
    public class TestCase
    {

        [XmlAttribute(AttributeName = "id")]
        public virtual string ID { get; set; }

        [XmlAttribute(AttributeName = "name")]
        public virtual string Name { get; set; }

        [XmlAttribute(AttributeName = "fullname")]
        public virtual string Fullname { get; set; }

        [XmlAttribute(AttributeName = "methodname")]
        public virtual string Methodname { get; set; }

        [XmlAttribute(AttributeName = "classname")]
        public virtual string Classname { get; set; }

        [XmlAttribute(AttributeName = "runstate")]
        public virtual string Runstate { get; set; }

        [XmlAttribute(AttributeName = "seed")]
        public virtual int Seed { get; set; }

        [XmlAttribute(AttributeName = "result")]
        public virtual string Result { get; set; }

        [XmlAttribute(AttributeName = "start-time")]
        public virtual string StartTime { get; set; }

        [XmlAttribute(AttributeName = "end-time")]
        public virtual string EndTime { get; set; }

        [XmlAttribute(AttributeName = "duration")]
        public virtual double Duration { get; set; }

        [XmlAttribute(AttributeName = "asserts")]
        public virtual int Asserts { get; set; }

        [XmlElement(ElementName = "failure")]
        public virtual Failure Failure { get; set; }

        [XmlElement(ElementName = "assertions")]
        public virtual Assertions Assertions { get; set; }

        [XmlText]
        public virtual string Text { get; set; }
    }
}
