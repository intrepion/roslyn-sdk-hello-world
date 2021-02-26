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
        private const string VariableAssigned = @"
using System;

namespace MakeConstTest
{
    class Program
    {
        static void Main(string[] args)
        {
            int i = 0;
            Console.WriteLine(i++);
        }
    }
}";

        private const string AlreadyConst = @"
using System;

namespace MakeConstTest
{
    class Program
    {
        static void Main(string[] args)
        {
            const int i = 0;
            Console.WriteLine(i);
        }
    }
}";

        private const string NoInitializer = @"
using System;

namespace MakeConstTest
{
    class Program
    {
        static void Main(string[] args)
        {
            int i;
            i = 0;
            Console.WriteLine(i);
        }
    }
}";

        private const string InitializerNotConstant = @"
using System;

namespace MakeConstTest
{
    class Program
    {
        static void Main(string[] args)
        {
            int i = DateTime.Now.DayOfYear;
            Console.WriteLine(i);
        }
    }
}";

        private const string MultipleInitializers = @"
using System;

namespace MakeConstTest
{
    class Program
    {
        static void Main(string[] args)
        {
            int i = 0, j = DateTime.Now.DayOfYear;
            Console.WriteLine(i, j);
        }
    }
}";

        //No diagnostics expected to show up
        [DataTestMethod]
        [DataRow(""),
            DataRow(VariableAssigned),
            DataRow(AlreadyConst),
            DataRow(NoInitializer),
            DataRow(InitializerNotConstant),
            DataRow(MultipleInitializers)]
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
