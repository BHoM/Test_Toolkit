using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.Engine.Test.CodeCompliance
{
    public static partial class Query
    {
        public static string CurrentAssemblyVersion()
        {
            return "5.0"; //Update each year - don't forget the one below!
        }

        public static string FullCurrentAssemblyVersion()
        {
            return $"{CurrentAssemblyVersion()}.0.0";
        }

        public static string CurrentAssemblyFileVersion()
        {
            return "5.3"; //Update each milestone - don't forget the one above!
        }

        public static string FullCurrentAssemblyFileVersion()
        {
            return $"{CurrentAssemblyFileVersion()}.0.0";
        }
    }
}
