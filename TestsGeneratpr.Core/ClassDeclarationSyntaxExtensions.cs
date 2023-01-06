using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TestsGenerator.Core
{
    internal static class ClassDeclarationSyntaxExtensions
    {
        public static ConstructorDeclarationSyntax GetConstructorWithMaxArgumentsCount(this ClassDeclarationSyntax syntax) =>
            syntax.Members
                .OfType<ConstructorDeclarationSyntax>()
                .OrderByDescending(c => c.ParameterList.Parameters.Count)
                .FirstOrDefault();

        public static List<MethodDeclarationSyntax> GetPublicMethods(this ClassDeclarationSyntax syntax) =>
            syntax.Members
                .OfType<MethodDeclarationSyntax>()
                .Where(m => m.Modifiers.Any(mod => mod.ValueText == "public"))
                .ToList();
    }
}
