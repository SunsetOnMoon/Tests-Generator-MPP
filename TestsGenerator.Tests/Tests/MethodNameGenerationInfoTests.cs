using NUnit.Framework;
using Moq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using TestsGenerator.Core;

namespace Tests
{
    [TestFixture]
    public class MethodNameGenerationInfoTests
    {
        private MethodNameGenerationInfo _testMethodNameGenerationInfoTests;
        [SetUp]
        public void SetUp()
        {
            int methodsWithTheSameNameCountFake = default;
            _testMethodNameGenerationInfoTests = new MethodNameGenerationInfo(methodsWithTheSameNameCountFake);
        }
    }
}