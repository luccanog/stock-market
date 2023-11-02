namespace Stock.Market.Common.Tests
{
    public class UtilsTest
    {
        [Fact]
        public void Utils_ParseCost_WithValidStringFormat_ShouldSucceed()
        {
            //Arrange
            var strValue = "$176.6602";
            var expectedValue = 176.6602m;

            //Act
            var parsedCost = Utils.ParseCost(strValue);

            //Arrange
            Assert.True(parsedCost == expectedValue);
        }

        [Fact]
        public void Utils_ParseCost_WithCommaSeparator_ShouldFail()
        {
            //Arrange
            var strValue = "$176,6602";
            var expectedValue = 176.6602m;

            //Act
            var parsedCost = Utils.ParseCost(strValue);

            //Arrange
            Assert.False(parsedCost == expectedValue);
        }
    }
}
