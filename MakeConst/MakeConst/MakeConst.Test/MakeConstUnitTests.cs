using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using VerifyCS = MakeConst.Test.CSharpCodeFixVerifier<
    MakeConst.MakeConstAnalyzer,
    MakeConst.MakeConstCodeFixProvider>;

namespace MakeConst.Test
{
    [TestClass]
    public class MakeConstUnitTest
    {
        //No diagnostics expected to show up
        [DataTestMethod]
        [DataRow("")]
        public async Task WhenTestCodeIsValidNoDiagnosticIsTriggered(string testCode)
        {
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        private const string LocalIntCouldBeConstant = @"
using System;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            {|#0:int x = 0;|}
            Console.WriteLine(x);
        }
    }
}";

        private const string LocalIntCouldBeConstantFixed = @"
using System;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            const int x = 0;
            Console.WriteLine(x);
        }
    }
}";

        //Diagnostic and CodeFix both triggered and checked for
        [DataTestMethod]
        [DataRow(LocalIntCouldBeConstant, LocalIntCouldBeConstantFixed)]
        public async Task WhenDiagnosticIsRaisedFixUpdatesCode(string testCode, string testCodeFixed)
        {
            var expected = VerifyCS.Diagnostic("MakeConst").WithLocation(0);
            await VerifyCS.VerifyCodeFixAsync(testCode, expected, testCodeFixed);
        }
    }
}
