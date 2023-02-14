using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace BH.oM.Test.NUnit
{
    [XmlRoot(ElementName = "test-run")]
    public class TestRun
    {
        [XmlElement(ElementName = "command-line")]
        public virtual string Commandline { get; set; }

        [XmlElement(ElementName = "test-suite")]
        public virtual TestSuite TestSuite { get; set; }

        [XmlAttribute(AttributeName = "id")]
        public virtual int ID { get; set; }

        [XmlAttribute(AttributeName = "name")]
        public virtual string Name { get; set; }

        [XmlAttribute(AttributeName = "fullname")]
        public virtual string FullName { get; set; }

        [XmlAttribute(AttributeName = "runstate")]
        public virtual string Runstate { get; set; }

        [XmlAttribute(AttributeName = "testcasecount")]
        public virtual int Testcasecount { get; set; }

        [XmlAttribute(AttributeName = "result")]
        public virtual string Result { get; set; }

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

        [XmlAttribute(AttributeName = "engine-version")]
        public virtual string EngineVersion { get; set; }

        [XmlAttribute(AttributeName = "clr-version")]
        public virtual string ClrVersion { get; set; }

        [XmlAttribute(AttributeName = "start-time")]
        public virtual string StartTime { get; set; }

        [XmlAttribute(AttributeName = "end-time")]
        public virtual string EndTime { get; set; }

        [XmlAttribute(AttributeName = "duration")]
        public virtual double Duration { get; set; }

        [XmlText]
        public virtual string Text { get; set; }
    }
}
