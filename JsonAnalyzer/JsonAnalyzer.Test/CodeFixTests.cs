using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace JsonAnalyzer.Test
{
    [TestClass]
    public class CodeFixTests
    {
        [TestMethod]
        public async Task TestAsync()
        {
            //Arrange
            var expected = await File.ReadAllTextAsync("CodeForTests\\AfterFix.txt", Encoding.UTF8);
            var (diagnostic, document, workspace) = await TestSetup.SetupAsync("CodeForTests\\BeforeFix.txt");
            var codeFixProvider = new JsonAnalyzerCodeFixProvider();
            CodeAction codeAction = null;
            var context = new CodeFixContext(document, diagnostic[0], (action, _) =>
            {
                codeAction = action;
            }, CancellationToken.None);

            //Act
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

            Assert.AreEqual(expected.Length, actual.Length - 1);
        }
    }
}