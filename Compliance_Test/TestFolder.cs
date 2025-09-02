using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.Tests.Compliace
{

    public class TestFolder
    {
        [Test]
        public void FolderTest()
        {
            Console.WriteLine(BH.Tests.Setup.Query.CurrentRepoFolder());
        }
    }
}
