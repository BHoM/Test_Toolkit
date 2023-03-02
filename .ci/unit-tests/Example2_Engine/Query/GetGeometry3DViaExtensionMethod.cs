using System;
using System.Runtime.CompilerServices;
using BH.oM.Geometry;
using BH.Engine.Base;
using BH.Engine.Geometry;
using BH.oM.Structure.Elements;
using BH.oM.Structure.SectionProperties;

namespace BH.Engine.Example
{
    public static partial class Query
    {
        public static IGeometry GetGeometry3DViaExtensionMethod(this ConcreteSection section)
        {
            // If structure_engine is not referenced, this should fail to find a method:
            System.Reflection.MethodInfo mi = BH.Engine.Base.Query.ExtensionMethodToCall(section, "Geometry"); 
            if (mi != null)
                return BH.Engine.Base.Compute.RunExtensionMethod(section, "Geometry") as IGeometry;
            else
                return null;
        }
    }
}