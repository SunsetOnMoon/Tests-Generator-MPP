using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using TestsGenerator.Core;

namespace Tests
{
    [TestClass]
    public class CompilationUnitSyntaxExtensionsTests
    {
        [TestMethod]
        public void GetClassesTest()
        {
            CompilationUnitSyntax rootFake = default;
            object actual = CompilationUnitSyntaxExtensions.GetClasses(rootFake);
            object expected = default;
            Assert.AreEqual(actual, expected);
            Assert.Fail("autogenerated");
        }
    }
}