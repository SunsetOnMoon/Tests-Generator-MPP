using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TestsGenerator.Core
{
    public class TestsGenerator : ITestsGenerator
    {
        public List<GenerationResult> Generate(string file)
        {
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(file);
            var root = syntaxTree.GetCompilationUnitRoot();
            var classes = root.GetClasses();
            List<GenerationResult> generResults = new List<GenerationResult>(classes.Count);

            foreach (var c in classes)
            {

            }
        }
    }

    internal class MethodNameGenerator
    {
        private readonly Dictionary<string, MethodNameGenerationInfo> _methods;

        public MethodNameGenerator(List<MethodDeclarationSyntax> methods)
        {
            _methods = new();
            Dictionary<string, int> methodCountDict = new Dictionary<string, int>();
            foreach(var method in methods)
            {
                string methodName = method.Identifier.ValueText;
                if (methodCountDict.ContainsKey(methodName))
                    methodCountDict[methodName]++;
                else
                    methodCountDict.Add(methodName, 1); //Check
            }

            foreach (var methodCount in methodCountDict)
                _methods.Add(methodCount.Key, new(methodCount.Value));
        }

        public string GenerateMethodName(string methodName)
        {
            var method = _methods[methodName];
            if (method.MethodsWithTheSameNameCount <= 1)
                return $"{methodName}Test";
            else
                return $"{methodName}{++method.LastGenerationNum}Test"
        }
    }
}
