using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;
using BH.oM.Reflection.Attributes;
using BH.oM.Test;
using BH.oM.Quantities.Attributes;

namespace BH.Engine.Test.CodeCompliance.DynamicChecks
{
    public static partial class Query
    {
        public static bool IsValidQuantityInputAttribute(MemberInfo node)
        {
            List<InputAttribute> inputAttributes = node.GetCustomAttributes<InputAttribute>().ToList();

            foreach(InputAttribute ia in inputAttributes)
            {
                if(ia.Quantity != null)
                {
                    if (!typeof(QuantityAttribute).IsAssignableFrom(ia.Quantity.GetType()))
                        return false;
                }
            }

            return true;
        }
    }
}
