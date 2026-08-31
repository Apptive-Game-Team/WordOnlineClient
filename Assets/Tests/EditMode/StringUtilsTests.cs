using Data.Util;
using NUnit.Framework;

namespace WordOnline.Tests
{
    public class StringUtilsTests
    {
        [TestCase("storm_stag", "StormStag")]
        [TestCase("stormStag", "StormStag")]
        [TestCase("StormStag", "StormStag")]
        [TestCase("sea_serpent", "SeaSerpent")]
        [TestCase("seaSerpent", "SeaSerpent")]
        [TestCase("SeaSerpent", "SeaSerpent")]
        public void ToPascalCasePreservesWordBoundaries(string input, string expected)
        {
            Assert.That(StringUtils.ToPascalCase(input), Is.EqualTo(expected));
        }
    }
}
