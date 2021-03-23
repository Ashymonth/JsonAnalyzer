using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace JsonAnalyzer.Test
{
    [TestClass]
    public class CodeFixTests
    {
        [TestMethod]
        public async Task CodeFIstText_Should_Be_One_Change()
        {
            //Arrange
            var expected = await File.ReadAllTextAsync("CodeForTests\\AfterFix_Should_Be_One_Change.txt", Encoding.UTF8);
            
            //Act
            var actual = await ExecuteTest("CodeForTests\\BeforeFix_Should_Be_One_Change.txt");

            //Assert
            Assert.AreEqual(expected.Length, actual.Length - 1);
        }

        [TestMethod]
        public async Task CodeFIstText_Should_Be_Zero_Change()
        {
            //Arrange
            var expected = await File.ReadAllTextAsync("CodeForTests\\AfterFix_Should_Be_Zero_Change.txt", Encoding.UTF8);
            
            //Act
            var actual = await ExecuteTest("CodeForTests\\BeforeFix_Should_Be_Zero_Change.txt");

            //Assert
            Assert.AreEqual(expected.Length, actual.Length);
        }

        private async Task<string> ExecuteTest(string sourceCode)
        {
            var (diagnostic, document, workspace) = await TestSetup.SetupAsync(sourceCode);
            var codeFixProvider = new JsonAnalyzerCodeFixProvider();
            CodeAction codeAction = null;
            if (!diagnostic.Any())
            {
                return (await document.GetTextAsync(CancellationToken.None)).ToString();
            }
            var context = new CodeFixContext(document, diagnostic[0], (action, _) =>
            {
                codeAction = action;
            }, CancellationToken.None);

            await codeFixProvider.RegisterCodeFixesAsync(context);

            if (codeAction == null)
            {
                throw new Exception("Code action was not registered");
            }

            var operations = await codeAction.GetOperationsAsync(CancellationToken.None);
            foreach (var operation in operations)
            {
                operation.Apply(workspace, CancellationToken.None);
            }

            var updateDocument = workspace.CurrentSolution.GetDocument(document.Id);
            var actual = (await updateDocument!.GetTextAsync()).ToString();

            return actual;
        }
    }
}