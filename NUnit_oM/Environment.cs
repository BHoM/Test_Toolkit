using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace BH.oM.Test.NUnit
{
    [XmlRoot(ElementName = "environment")]
    public class Environment
    {

        [XmlAttribute(AttributeName = "framework-version")]
        public string FrameworkVersion { get; set; }

        [XmlAttribute(AttributeName = "clr-version")]
        public string ClrVersion { get; set; }

        [XmlAttribute(AttributeName = "os-version")]
        public string OsVersion { get; set; }

        [XmlAttribute(AttributeName = "platform")]
        public string Platform { get; set; }

        [XmlAttribute(AttributeName = "cwd")]
        public string Cwd { get; set; }

        [XmlAttribute(AttributeName = "machine-name")]
        public string MachineName { get; set; }

        [XmlAttribute(AttributeName = "user")]
        public string User { get; set; }

        [XmlAttribute(AttributeName = "user-domain")]
        public string UserDomain { get; set; }

        [XmlAttribute(AttributeName = "culture")]
        public string Culture { get; set; }

        [XmlAttribute(AttributeName = "uiculture")]
        public string Uiculture { get; set; }

        [XmlAttribute(AttributeName = "os-architecture")]
        public string OsArchitecture { get; set; }
    }
}
