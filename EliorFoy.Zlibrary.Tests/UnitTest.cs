using EliorFoy.Zlibrary.CLI;


namespace EliorFoy.Zlibrary.Tests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {

        }

        [Test]
        public void Test1()
        {
            AccountPool.Refresh();
            Assert.Pass();
        }
    }
}