using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using TestsGenerator.Core;

namespace Tests
{
    [TestClass]
    public class MethodNameGenerationInfoTests
    {
        private MethodNameGenerationInfo _testMethodNameGenerationInfoTests;
        public MethodNameGenerationInfoTests()
        {
            int methodsWithTheSameNameCountFake = default;
            _testMethodNameGenerationInfoTests = new MethodNameGenerationInfo(methodsWithTheSameNameCountFake);
        }
    }
}