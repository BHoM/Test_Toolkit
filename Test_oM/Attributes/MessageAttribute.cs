using BH.oM.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BH.oM.Test.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class MessageAttribute : Attribute, IImmutable
    {
        public string Message { get; }

        public MessageAttribute(string message)
        {
            Message = message;
        }
    }
}
