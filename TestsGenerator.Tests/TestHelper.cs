using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;
namespace TestsGenerator.Tests
{
    public static class TestHelper
    {
        public static List<ClassDeclarationSyntax> GetClasses(string source)
        {
            var root = CSharpSyntaxTree.ParseText(source).GetCompilationUnitRoot();
            return root.DescendantNodes()
                .OfType<ClassDeclarationSyntax>()
                .ToList();
        }

        public static ClassDeclarationSyntax GetClass(string source) => GetClasses(source).First();

        public static List<MethodDeclarationSyntax> GetMethods(ClassDeclarationSyntax classSyntax)
        {
            return classSyntax.Members.OfType<MethodDeclarationSyntax>().ToList();
        }

        public static List<FieldDeclarationSyntax> GetFields(ClassDeclarationSyntax classSyntax)
        {
            return classSyntax.Members.OfType<FieldDeclarationSyntax>().ToList();
        }
    }
}
