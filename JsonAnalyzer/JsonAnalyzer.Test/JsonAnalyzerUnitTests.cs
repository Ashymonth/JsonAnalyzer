using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using VerifyCS = JsonAnalyzer.Test.CSharpCodeFixVerifier<
    JsonAnalyzer.JsonAnalyzerAnalyzer,
    JsonAnalyzer.JsonAnalyzerCodeFixProvider>;

namespace JsonAnalyzer.Test
{
    [TestClass]
    public class JsonAnalyzerUnitTest
    {
        //No diagnostics expected to show up
        [TestMethod]
        public async Task TestMethod1()
        {
            var test = @"";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        //Diagnostic and CodeFix both triggered and checked for
        [TestMethod]
        public async Task TestMethod2()
        {
            var test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        [JsonObject(MemberSerialization.OptIn)]
        class {|#0:TypeName|}
        {

            [JsonProperty(""testPropr"")]
            public int TestPropr { get; set; }

            [JsonProperty(""testPropr1"")]
            public int TestPropr1 { get; set; }

            [JsonProperty(""testPropr2"")]
            public int TestPropr2 { get; set; }
        }
    }";

            var fixtest = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class TYPENAME
        {   
        }
    }";

            var expected = VerifyCS.Diagnostic("JsonAnalyzer").WithLocation(0).WithArguments("TypeName");
            await VerifyCS.VerifyCodeFixAsync(test, expected, fixtest);
        }
    }
}
