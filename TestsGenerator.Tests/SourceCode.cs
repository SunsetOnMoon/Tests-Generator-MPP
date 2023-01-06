using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestsGenerator.Tests
{
    public static class SourceCode
    {
        public const string ClassWithOverridedMethod =
            @"using System;
            namespace MyCode
            {
                public class MyClass
                {
                    public void Method(int a)
                    {
                        Console.WriteLine(""Method (int)"");
                    }
                    public void Method(double a)
                    {
                        Console.WriteLine(""Method (double)"");
                    }
                }
            }";

        public const string ClassWithConstructor =
            @"using System;
            namespace MyCode
            {
                public class MyClass
                {
                    private readonly string _name;
                    private readonly int _age;
                    public MyClass(string name, int age)
                    {
                        _name = name;
                        _age = age;
                    }
                    public string GetName() => _name;
                    public int GetAge() => _age;
                }
            }";

        public const string ClassWithInterfacePassedInConstructor =
            @"using System;
            namespace MyCode
            {
                public class MyClass
                {
                    private readonly ILol _lol;
                    public MyClass(ILol lol)
                    {
                        _lol = lol;
                    }
                    public ILol GetLol() => _lol;
                }
            }";

        public const string ClassWithFunction =
            @"using System;
            namespace MyCode
            {
                public class MyClass
                {
                    public string LolToString(ILol lol) => lol.ToString();
                }
            }";

        public const string ClassWithProcedure =
            @"using System;
            namespace MyCode
            {
                public class MyClass
                {
                    public void PrintLol(ILol lol) => Console.WriteLine(lol.ToString());
                }
            }";
    }
}
