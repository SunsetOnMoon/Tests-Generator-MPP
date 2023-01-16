using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.ObjectModel;

namespace TestsGenerator.Core
{
    public class TestGenerator : ITestsGenerator
    {
        private readonly Dictionary<int, string> _generationTemplateDict = new Dictionary<int, string>()
        {
            {1, "NUnit.Framework" },
            {2, "Xunit" },
            {3, "Microsoft.VisualStudio.TestTools.UnitTesting" }
        };

        public List<GenerationResult> Generate(string file, int generationTemplate)
        {
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(file);
            var root = syntaxTree.GetCompilationUnitRoot();
            var classes = root.GetClasses();
            List<GenerationResult> generResults = new List<GenerationResult>(classes.Count);

            foreach (var c in classes)
            {
                var tests = GerenerateSignleClassTests(c, generationTemplate);
                generResults.Add(tests);
            }
            return generResults;
        }

        private static GenerationResult GerenerateSignleClassTests(ClassDeclarationSyntax syntax, int generationTemplate)
        {
            Dictionary<int, string> generationTemplateDict = new Dictionary<int, string>()
            {
                {1, "NUnit.Framework" },
                {2, "Xunit" },
                {3, "Microsoft.VisualStudio.TestTools.UnitTesting" }
            };
            //{
            //    {1, "TestFixture" },
            //    {2, "" },
            //    {3, "TestClass" }
            //};
            string className = syntax.Identifier.Text;
            string modifs = syntax.Modifiers.ToString();
            List<MethodDeclarationSyntax> methods = syntax.GetPublicMethods();
            ConstructorDeclarationSyntax constructor = syntax.GetConstructorWithMaxArgumentsCount();

            List<MemberDeclarationSyntax> members;
            if (!modifs.Contains("static"))
            {
                var setupSection = GenerateSetup(className, constructor, generationTemplate);
                var methodsSection = GenerateMethods(setupSection.ClassVariableName, methods, generationTemplate);


                members = new List<MemberDeclarationSyntax>(setupSection.SetupSection);
                members.AddRange(methodsSection);
            }
            else
            {
                var methodsSection = GenerateMethods(className, methods, generationTemplate);
                members = new List<MemberDeclarationSyntax>();
                members.AddRange(methodsSection);
            }
            Dictionary<int, Func<ClassDeclarationSyntax>> testClassDeclDict = new Dictionary<int, Func<ClassDeclarationSyntax>>()
            {
                {1,  () => { return SyntaxFactory.ClassDeclaration($"{className}Tests")
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
                                        SyntaxFactory.List(members)); } },
                {2,  () =>  { return SyntaxFactory.ClassDeclaration($"{className}Tests")
                                    .WithModifiers(
                                        SyntaxFactory.TokenList(
                                            SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                                    .WithMembers(
                                        SyntaxFactory.List(members)); } },
                {3,  () => { return SyntaxFactory.ClassDeclaration($"{className}Tests")
                                    .WithAttributeLists(
                                        SyntaxFactory.SingletonList(
                                            SyntaxFactory.AttributeList(
                                                SyntaxFactory.SingletonSeparatedList(
                                                    SyntaxFactory.Attribute(
                                                        SyntaxFactory.IdentifierName("TestClass"))))))
                                    .WithModifiers(
                                        SyntaxFactory.TokenList(
                                            SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                                    .WithMembers(
                                        SyntaxFactory.List(members));
        }
    }

            };

            SyntaxTree syntaxTree = CSharpSyntaxTree.Create(
                SyntaxFactory.CompilationUnit()
                    .WithUsings(
                        SyntaxFactory.List(
                            new UsingDirectiveSyntax[] {
                        SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName($"{generationTemplateDict[generationTemplate]}")),
                            SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName("Moq")),
                            SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName("Microsoft.CodeAnalysis.CSharp.Syntax")),
                            SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName("Microsoft.CodeAnalysis.CSharp")),
                            SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName("Microsoft.CodeAnalysis")),
                            SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName("TestsGenerator.Core"))}))
                        
                    .WithMembers(
                        SyntaxFactory.SingletonList<MemberDeclarationSyntax>(
                            SyntaxFactory.NamespaceDeclaration(
                                SyntaxFactory.IdentifierName("Tests"))
                            .WithMembers(
                                SyntaxFactory.SingletonList<MemberDeclarationSyntax>(
                                    testClassDeclDict[generationTemplate]()
                                    ))))
                    .NormalizeWhitespace());
            return new GenerationResult(className, syntaxTree.ToString());
        }
     
        private static (string ClassVariableName, List<MemberDeclarationSyntax> SetupSection) GenerateSetup(string className, ConstructorDeclarationSyntax constructor, int generationTemplate)
        {            
            string testClassObjectName = $"_test{className}Tests";
            List<StatementSyntax> initializations = new List<StatementSyntax>();
            List<SyntaxNodeOrToken> constructorArgs = new List<SyntaxNodeOrToken>();

            if (constructor != null && constructor.ParameterList.Parameters.Count > 0)
            {
                foreach (var parameter in constructor.ParameterList.Parameters)
                {
                    var paramSection = GenerateParameterCreationSection(parameter);
                    initializations.Add(paramSection.Initialization);

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

            Dictionary<int, Func<MemberDeclarationSyntax>> setupDeclDict = new Dictionary<int, Func<MemberDeclarationSyntax>>
            {
                {1, () => {return                 SyntaxFactory.MethodDeclaration(
                    SyntaxFactory.PredefinedType(
                        SyntaxFactory.Token(SyntaxKind.VoidKeyword)),
                    SyntaxFactory.Identifier($"SetUp"))
                .WithAttributeLists(
                    SyntaxFactory.SingletonList(
                        SyntaxFactory.AttributeList(
                            SyntaxFactory.SingletonSeparatedList(
                                SyntaxFactory.Attribute(
                                    SyntaxFactory.IdentifierName("SetUp"))))
                        ))
                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                    .WithBody(SyntaxFactory.Block(initializations)); } },
                {2, () => { return SyntaxFactory.ConstructorDeclaration(
                                SyntaxFactory.Identifier(className + "Tests"))
                                    //SyntaxFactory.IdentifierName($"{setupDeclDict.TryGetValue}"))))))
                    .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                    .WithBody(SyntaxFactory.Block(initializations)); } },
                {3, () => { return SyntaxFactory.ConstructorDeclaration(
                                SyntaxFactory.Identifier(className + "Tests"))
                                    //SyntaxFactory.IdentifierName($"{setupDeclDict.TryGetValue}"))))))
                    .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                    .WithBody(SyntaxFactory.Block(initializations)); } }
            };

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
                        SyntaxFactory.Token((SyntaxKind.PrivateKeyword)))),
                setupDeclDict[generationTemplate]()
                    
            };

            return (testClassObjectName, setup);
        }

        private static (ArgumentSyntax Argument, StatementSyntax Initialization) GenerateParameterCreationSection(ParameterSyntax parameter)
        {
            StatementSyntax inializationExpr;
            ArgumentSyntax constrArgument;
            string objectName = $"{parameter.Identifier.Text}Fake";
            string type = parameter.Type.ToString();

            if (type.StartsWith('I'))
            {
                inializationExpr = SyntaxFactory.LocalDeclarationStatement(
                    SyntaxFactory.VariableDeclaration(
                        SyntaxFactory.IdentifierName(type))
                    .WithVariables(
                        SyntaxFactory.SingletonSeparatedList(
                            SyntaxFactory.VariableDeclarator(
                                SyntaxFactory.Identifier(objectName))
                            .WithInitializer(
                                SyntaxFactory.EqualsValueClause(
                                    SyntaxFactory.ObjectCreationExpression(
                                        SyntaxFactory.GenericName(
                                            SyntaxFactory.Identifier("Mock"))
                                        .WithTypeArgumentList(
                                            SyntaxFactory.TypeArgumentList(
                                                SyntaxFactory.SingletonSeparatedList<TypeSyntax>(
                                                    SyntaxFactory.IdentifierName(type)))))
                                    .WithArgumentList(
                                        SyntaxFactory.ArgumentList()))))));
                constrArgument = SyntaxFactory.Argument(
                    SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        SyntaxFactory.IdentifierName(objectName),
                        SyntaxFactory.IdentifierName("Object")));
            }
            else
            {
                inializationExpr = GenerateLocalDeclarationStatement(type, objectName);
                constrArgument = SyntaxFactory.Argument(SyntaxFactory.IdentifierName(objectName));
            }
            return (constrArgument, inializationExpr);
        }

        private static LocalDeclarationStatementSyntax GenerateLocalDeclarationStatement(string type, string variableName)
        {
            var initialExpression = SyntaxFactory.LocalDeclarationStatement(
                SyntaxFactory.VariableDeclaration(
                    SyntaxFactory.IdentifierName(type))
                .WithVariables(
                    SyntaxFactory.SingletonSeparatedList(
                        SyntaxFactory.VariableDeclarator(
                            SyntaxFactory.Identifier(variableName))
                        .WithInitializer(
                            SyntaxFactory.EqualsValueClause(
                                SyntaxFactory.LiteralExpression(
                                    SyntaxKind.DefaultLiteralExpression,
                                    SyntaxFactory.Token(SyntaxKind.DefaultKeyword)))))));
            return initialExpression;
        }

        private static MemberDeclarationSyntax GenerateMethod(string methodName, string classVariableName, MethodDeclarationSyntax method, int generationTemplate)
        {
            Dictionary<int, string> testMetodDecl = new Dictionary<int, string>
            {
                {1, "Test" },
                {2, "Fact" },
                {3, "TestMethod" }
            };
            List<StatementSyntax> statements = new List<StatementSyntax>();
            List<SyntaxNodeOrToken> methodArgs = new List<SyntaxNodeOrToken>();
            string actualVarName = "actual";
            string expectedVarName = "expected";

            if (method.ParameterList.Parameters.Count > 0)
            {
                foreach (ParameterSyntax parameter in method.ParameterList.Parameters)
                {
                    var section = GenerateParameterCreationSection(parameter);
                    statements.Add(section.Initialization);

                    methodArgs.Add(section.Argument);
                    methodArgs.Add(SyntaxFactory.Token(SyntaxKind.CommaToken));
                }

                methodArgs.RemoveAt(methodArgs.Count - 1);
            }

            if (method.ReturnType is PredefinedTypeSyntax predifRetType && predifRetType.Keyword.ValueText == "void")
            {
                var actualStatement = SyntaxFactory.ExpressionStatement(
                    SyntaxFactory.InvocationExpression(
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            SyntaxFactory.IdentifierName(classVariableName),
                            SyntaxFactory.IdentifierName(method.Identifier.ValueText)))
                    .WithArgumentList(
                        SyntaxFactory.ArgumentList(
                            SyntaxFactory.SeparatedList<ArgumentSyntax>(
                                new SyntaxNodeOrTokenList(methodArgs)))));
                statements.Add(actualStatement);
            }
            else
            {
                string retTypeName;
                if (method.ReturnType is PredefinedTypeSyntax predTypeSyntax)
                    retTypeName = predTypeSyntax.Keyword.ValueText;
                else if (method.ReturnType is IdentifierNameSyntax nameIdentif)
                    retTypeName = nameIdentif.Identifier.ValueText;
                else
                    retTypeName = "object";

                var actualStatement = SyntaxFactory.LocalDeclarationStatement(
                    SyntaxFactory.VariableDeclaration(
                        SyntaxFactory.IdentifierName(retTypeName))
                    .WithVariables(
                        SyntaxFactory.SingletonSeparatedList(
                            SyntaxFactory.VariableDeclarator(
                                SyntaxFactory.Identifier(actualVarName))
                            .WithInitializer(
                                SyntaxFactory.EqualsValueClause(
                                    SyntaxFactory.InvocationExpression(
                                        SyntaxFactory.MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            SyntaxFactory.IdentifierName(classVariableName),
                                            SyntaxFactory.IdentifierName(method.Identifier.ValueText)))
                                    .WithArgumentList(
                                        SyntaxFactory.ArgumentList(
                                            SyntaxFactory.SeparatedList<ArgumentSyntax>(
                                                new SyntaxNodeOrTokenList(methodArgs)))))))));
                statements.Add(actualStatement);

                var expectedVarDeclStatement = GenerateLocalDeclarationStatement(retTypeName, expectedVarName);
                statements.Add(expectedVarDeclStatement);

                var resultAssertionStatement = SyntaxFactory.ExpressionStatement(
                    SyntaxFactory.InvocationExpression(
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            SyntaxFactory.IdentifierName("Assert"),
                            SyntaxFactory.IdentifierName("That")))
                    .WithArgumentList(
                        SyntaxFactory.ArgumentList(
                            SyntaxFactory.SeparatedList<ArgumentSyntax>(
                                new SyntaxNodeOrToken[]{
                                    SyntaxFactory.Argument(
                                        SyntaxFactory.IdentifierName(actualVarName)),
                                    SyntaxFactory.Token(SyntaxKind.CommaToken),
                                    SyntaxFactory.Argument(
                                        SyntaxFactory.InvocationExpression(
                                            SyntaxFactory.MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                SyntaxFactory.IdentifierName("Is"),
                                                SyntaxFactory.IdentifierName("EqualTo")))
                                        .WithArgumentList(
                                            SyntaxFactory.ArgumentList(
                                                SyntaxFactory.SingletonSeparatedList<ArgumentSyntax>(
                                                    SyntaxFactory.Argument(
                                                        SyntaxFactory.IdentifierName(expectedVarName))))))}))));

                statements.Add(resultAssertionStatement);
            }

            var assertFailStatement = SyntaxFactory.ExpressionStatement(
                SyntaxFactory.InvocationExpression(
                    SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        SyntaxFactory.IdentifierName("Assert"),
                        SyntaxFactory.IdentifierName("Fail")))
                .WithArgumentList(
                    SyntaxFactory.ArgumentList(
                        SyntaxFactory.SingletonSeparatedList(
                            SyntaxFactory.Argument(
                                SyntaxFactory.LiteralExpression(
                                    SyntaxKind.StringLiteralExpression,
                                    SyntaxFactory.Literal("autogenerated")))))));

            statements.Add(assertFailStatement);

            var methodDeclaration = SyntaxFactory.MethodDeclaration(
                SyntaxFactory.PredefinedType(
                    SyntaxFactory.Token(SyntaxKind.VoidKeyword)),
                SyntaxFactory.Identifier(methodName))
            .WithAttributeLists(
                SyntaxFactory.SingletonList(
                    SyntaxFactory.AttributeList(
                        SyntaxFactory.SingletonSeparatedList(
                            SyntaxFactory.Attribute(
                                SyntaxFactory.IdentifierName($"{testMetodDecl[generationTemplate]}"))))))
            .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
            .WithBody(SyntaxFactory.Block(statements));

            return methodDeclaration;
        }

        private static List<MemberDeclarationSyntax> GenerateMethods(string classVariableName, List<MethodDeclarationSyntax> methods, int generationTemplate)
        {
            List<MemberDeclarationSyntax> members = new List<MemberDeclarationSyntax>(methods.Count);
            var generator = new MethodNameGenerator(methods);

            foreach (var method in methods)
            {
                string methodName = generator.GenerateMethodName(method.Identifier.ValueText);
                var methodSection = GenerateMethod(methodName, classVariableName, method, generationTemplate);
                members.Add(methodSection);
            }
            return members;
        }
    }

    



    public class MethodNameGenerator
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
