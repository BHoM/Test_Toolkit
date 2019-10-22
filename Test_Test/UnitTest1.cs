using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test_Test
{
    [TestClass]
    public class UnitTest1
    {
        private TestContext testContextInstance;

        public TestContext TestContext
        {
            get { return testContextInstance; }
            set { testContextInstance = value; }
        }


        [TestMethod]
        public void TestMethod1()
        {
            TestContext.WriteLine("true");
            Assert.IsTrue(true);
        }
    }
}
