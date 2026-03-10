namespace Library.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void ThisTestShouldPass()
        {
            string name = "John";
            Assert.Equal("John", name);

        }
    }
}
