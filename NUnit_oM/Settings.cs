using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace BH.oM.Test.NUnit
{
    [XmlRoot(ElementName = "settings")]
    public class Settings
    {

        [XmlElement(ElementName = "setting")]
        public virtual Setting Setting { get; set; }
    }
}
