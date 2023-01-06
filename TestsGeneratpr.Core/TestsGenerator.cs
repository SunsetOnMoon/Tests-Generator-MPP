﻿using Microsoft.CodeAnalysis;
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
                var tests = GerenerateSignleClassTests(c);
                generResults.Add(tests);
            }
            return generResults;
        }

        private static GenerationResult GerenerateSignleClassTests(ClassDeclarationSyntax syntax)
        {
            string className = syntax.Identifier.Text;
            List<MethodDeclarationSyntax> methods = syntax.GetPublicMethods();
            ConstructorDeclarationSyntax constructor = syntax.GetConstructorWithMaxArgumentsCount();

            var setupSection = GenerateSetup(className, constructor);
            var methodsSection = GenerateMethods(setupSection.ClassVariableName, methods);


            var members = new List<MemberDeclarationSyntax>(setupSection.SetupSection);
            members.AddRange(methodsSection);

            SyntaxTree syntaxTree = CSharpSyntaxTree.Create(
                SyntaxFactory.CompilationUnit()
                    .WithUsings(
                        SyntaxFactory.List(
                            new UsingDirectiveSyntax[] {
                        SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName("NUnit.Framework")) }))
                    .WithMembers(
                        SyntaxFactory.SingletonList<MemberDeclarationSyntax>(
                            SyntaxFactory.NamespaceDeclaration(
                                SyntaxFactory.IdentifierName("Tests"))
                            .WithMembers(
                                SyntaxFactory.SingletonList<MemberDeclarationSyntax>(
                                    SyntaxFactory.ClassDeclaration($"{className}Tests")
                                    .WithAttributeLists(
                                        SyntaxFactory.SingletonList(
                                            SyntaxFactory.AttributeList(
                                                SyntaxFactory.SingletonSeparatedList(
                                                    SyntaxFactory.Attribute(
                                                        SyntaxFactory.IdentifierName("TestFixture"))))))
                                    .WithModifiers(
                                        SyntaxFactory.TokenList(
                                            SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                                    .WithMembers(
                                        SyntaxFactory.List(members))))))
                    .NormalizeWhitespace());
            return new GenerationResult(className, syntaxTree.ToString());
        }

        private static (string ClassVariableName, List<MemberDeclarationSyntax> SetupSection) GenerateSetup(string className, ConstructorDeclarationSyntax constructor)
        {
            string testClassObjectName = $"_test{className}";
            List<StatementSyntax> initializations = new List<StatementSyntax>();
            List<SyntaxNodeOrToken> constructorArgs = new List<SyntaxNodeOrToken>();

            if (constructor != null && constructor.ParameterList.Parameters.Count > 0)
            {
                foreach (var parameter in constructor.ParameterList.Parameters)
                {
                    var paramSection = GenerateParameterCreationSection(parameter);
                    initializations.Add(paramSection.InitializationsExpression);

                    constructorArgs.Add(paramSection.Argument);
                    constructorArgs.Add(SyntaxFactory.Token(SyntaxKind.CommaToken));
                }

                constructorArgs.RemoveAt(constructorArgs.Count - 1);
            }

            initializations.Add(
                SyntaxFactory.ExpressionStatement(
                    SyntaxFactory.AssignmentExpression(
                        SyntaxKind.SimpleAssignmentExpression,
                        SyntaxFactory.IdentifierName(testClassObjectName),
                        SyntaxFactory.ObjectCreationExpression(
                        SyntaxFactory.IdentifierName(className))
                        .WithArgumentList(
                            SyntaxFactory.ArgumentList(
                                SyntaxFactory.SeparatedList<ArgumentSyntax>(
                                    new SyntaxNodeOrTokenList(constructorArgs)))))));

            List<MemberDeclarationSyntax> setup = new List<MemberDeclarationSyntax>()
            {
                SyntaxFactory.FieldDeclaration(
                    SyntaxFactory.VariableDeclaration(
                        SyntaxFactory.IdentifierName(className))
                    .WithVariables(
                        SyntaxFactory.SingletonSeparatedList(
                            SyntaxFactory.VariableDeclarator(
                                SyntaxFactory.Identifier(testClassObjectName)))))
                .WithModifiers(
                    SyntaxFactory.TokenList(
                        SyntaxFactory.Token(SyntaxKind.PrivateKeyword))),

                SyntaxFactory.MethodDeclaration(
                    SyntaxFactory.PredefinedType(
                        SyntaxFactory.Token(SyntaxKind.VoidKeyword)),
                    SyntaxFactory.Identifier("SetUp"))
                .WithAttributeLists(
                    SyntaxFactory.SingletonList(
                        SyntaxFactory.AttributeList(
                            SyntaxFactory.SingletonSeparatedList(
                                SyntaxFactory.Attribute(
                                    SyntaxFactory.IdentifierName("SetUp"))))))
                    .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                    .WithBody(SyntaxFactory.Block(initializations))
            };

            return (testClassObjectName, setup);
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
                return $"{methodName}{++method.LastGenerationNum}Test";
        }
    }
}
