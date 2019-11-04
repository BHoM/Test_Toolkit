using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test_Profiling
{
    class Program
    {
        static void Main(string[] args)
        {
            /// ************************************/
            /// Diffing test and profiling methods
            /// ************************************/

            Diffing_Engine.Test01();

            Diffing_Engine.Profiling01();

            /// ************************************/

            Console.Read();
        }
    }
}
