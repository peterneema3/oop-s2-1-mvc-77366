namespace Library.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void ThisTestShouldFail()
        {
            string name = "John";
            Assert.Equal("Jane", name);

        }
    }
}
