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
