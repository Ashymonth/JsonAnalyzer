using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace JsonAnalyzer.Test
{
    [TestClass]
    public class AnalyzerTests
    {
        [TestMethod]
        public async Task TestAnalyzer_ShouldBe_Zero_Analyze()
        {
            //Arrange
            const int expected = 0;

            //Act
            var actual = await TestSetup.SetupAsync("CodeForTests\\Should_Be_Zero_Analyze_hit.txt");

            //Assert
            Assert.AreEqual(expected, actual.diagnostics.Length);
        }

        [TestMethod]
        public async Task TestAnalyzer_ShouldBe_One_Analyze()
        {
            //Arrange
            const int expected = 1;
                
            //Act
            var actual = await TestSetup.SetupAsync("CodeForTests\\Should_Be_One_Analyze_Hit.txt");

            //Assert
            Assert.AreEqual(expected, actual.diagnostics.Length);
        }
    }
}