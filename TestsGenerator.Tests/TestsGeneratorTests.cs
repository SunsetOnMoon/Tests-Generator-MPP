using TestsGenerator.Core;
namespace TestsGenerator.Tests
{
    public class TestsGeneratorTests
    {
        private readonly ITestsGenerator _generator = new TestsGenerator.Core.TestsGenerator();

        [Fact]
        public void Generate_Returns3TestClasses()
        {
            var source = _generator.Generate(SourceCode.ClassWithConstructor + "\n"
                + SourceCode.ClassWithOverridedMethod + "\n"
                + SourceCode.ClassWithInterfacePassedInConstructor, 1);

            Assert.Equal(3, source.Count);
        }

        [Fact]
        public void Generate_ReturnsTestClassWithSetUpMethod()
        {
            var source = _generator.Generate(SourceCode.ClassWithConstructor, 1).First().Content;
            var actual = TestHelper.GetClass(source);

            var methods = TestHelper.GetMethods(actual);
            Assert.Contains(methods, m => m.Identifier.ValueText == "SetUp");
            var method = methods.First(m => m.Identifier.ValueText == "SetUp");
            Assert.Single(method.AttributeLists);
            Assert.Single(method.AttributeLists[0].Attributes);
            Assert.Equal("SetUp", method.AttributeLists[0].Attributes[0].Name.ToString());

            var fields = TestHelper.GetFields(actual);
            Assert.Single(fields);
            Assert.Equal("private MyClassTests _testMyClassTests;", fields.First().ToString());
            Assert.NotNull(method.Body);
            Assert.Equal(3, method.Body!.Statements.Count);
            Assert.Equal("string nameFake = default;", method.Body!.Statements[0].ToString());
            Assert.Equal("int ageFake = default;", method.Body!.Statements[1].ToString());
            Assert.Equal("_testMyClassTests = new MyClassTests(nameFake, ageFake);", method.Body!.Statements[2].ToString());
        }

        [Theory]
        [InlineData("Method1Test")]
        [InlineData("Method2Test")]
        public void Generate_IfClassContainsOverridedMethods(string methodName)
        {
            var source = _generator.Generate(SourceCode.ClassWithOverridedMethod, 1).First().Content;
            var actual = TestHelper.GetClass(source);

            var methods = TestHelper.GetMethods(actual);
            Assert.Contains(methods, m => m.Identifier.ValueText == methodName);
        }

        [Fact]
        public void Generate_IfMethodReturnsValue()
        {
            var source = _generator.Generate(SourceCode.ClassWithFunction, 1).First().Content;

            var actual = TestHelper.GetClass(source);
            var method = TestHelper.GetMethods(actual).First(m => m.Identifier.ValueText == "LolToStringTest");

            Assert.Equal("ILol lolFake = new Mock<ILol>();", method.Body!.Statements[0].ToString());
            Assert.Equal("string actual = _testMyClassTests.LolToString(lolFake.Object);", method.Body!.Statements[1].ToString());
            Assert.Equal("string expected = default;", method.Body!.Statements[2].ToString());
            Assert.Equal("Assert.That(actual, Is.EqualTo(expected));", method.Body!.Statements[3].ToString());
            Assert.Equal("Assert.Fail(\"autogenerated\");", method.Body!.Statements[4].ToString());
        }

        [Fact]
        public void Generate_IfMethodDoesntReturnsValue()
        {
            var source = _generator.Generate(SourceCode.ClassWithProcedure, 1).First().Content;

            var actual = TestHelper.GetClass(source);
            var method = TestHelper.GetMethods(actual).First(m => m.Identifier.ValueText == "PrintLolTest");

            Assert.Equal("ILol lolFake = new Mock<ILol>();", method.Body!.Statements[0].ToString());
            Assert.Equal("_testMyClassTests.PrintLol(lolFake.Object);", method.Body!.Statements[1].ToString());
            Assert.Equal("Assert.Fail(\"autogenerated\");", method.Body!.Statements[2].ToString());
        }
    }
}