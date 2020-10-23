using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TestGeneratorLib.Analys;

namespace TestGeneratorLib
{
    internal class TestCreator
    {
        private Unit _unit;
        private CompilationUnitSyntax _test;

        internal TestCreator(SyntaxNode root)
        {
            _unit = new Unit(root);
            _test = SyntaxFactory.CompilationUnit();
        }

        internal string GetTest()
        {
            _test = _test.AddUsings(_unit.Usings.Select(us => SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName(us))).ToArray());
            _test = _test.AddUsings(
                SyntaxFactory.UsingDirective(
                    SyntaxFactory.IdentifierName("NUnit.Framework")), 
                SyntaxFactory.UsingDirective(
                    SyntaxFactory.IdentifierName("Moq")));
            _test = _test.AddMembers(_unit.Namespaces.Select(nm => CreateNamespace(nm)).ToArray());
            return _test.NormalizeWhitespace().ToFullString();
        }

        internal NamespaceDeclarationSyntax CreateNamespace(Namespace namespaceDeclaration)
        {
            var @namespace = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.IdentifierName(namespaceDeclaration.Name + ".Tests"));
            @namespace = @namespace.AddMembers(namespaceDeclaration.Classes.Select(cl => CreateClass(cl)).ToArray());
            return @namespace;
        }

        internal ClassDeclarationSyntax CreateClass(Class classDeclaration)
        {
            var @class = SyntaxFactory.ClassDeclaration(classDeclaration.Name + "Tests")
                .WithAttributeLists(
                    SyntaxFactory.SingletonList<AttributeListSyntax>(
                        SyntaxFactory.AttributeList(
                            SyntaxFactory.SingletonSeparatedList<AttributeSyntax>(
                                SyntaxFactory.Attribute(
                                    SyntaxFactory.IdentifierName("TestFixture"))))))
                 .WithModifiers(
                      SyntaxFactory.TokenList(
                          SyntaxFactory.Token(SyntaxKind.PublicKeyword)));
            if (!classDeclaration.IsStatic) 
            {
                @class = @class.AddMembers(classDeclaration.Parameters.Select(
                    pr => SyntaxFactory.FieldDeclaration(
                        SyntaxFactory.VariableDeclaration(pr.Type.GetMockOrDefaultType())
                        .WithVariables(
                            SyntaxFactory.SingletonSeparatedList<VariableDeclaratorSyntax>(
                                SyntaxFactory.VariableDeclarator(
                                    SyntaxFactory.Identifier(pr.Name + "Global")))))
                    .WithModifiers(
                        SyntaxFactory.TokenList(
                            SyntaxFactory.Token(
                                SyntaxKind.PrivateKeyword))))
                    .ToArray());
                @class = @class.AddMembers(SyntaxFactory.FieldDeclaration(
                    SyntaxFactory.VariableDeclaration(
                        SyntaxFactory.IdentifierName(classDeclaration.Name))
                    .WithVariables(
                        SyntaxFactory.SingletonSeparatedList<VariableDeclaratorSyntax>(
                            SyntaxFactory.VariableDeclarator(
                                SyntaxFactory.Identifier(classDeclaration.Name + "Global")))))
                    .WithModifiers(
                        SyntaxFactory.TokenList(
                            SyntaxFactory.Token(
                                SyntaxKind.PrivateKeyword))));
                @class = @class.AddMembers(CreateSetUpMethod(classDeclaration));
            }
            @class = @class.AddMembers(classDeclaration.Methods.Select(mt => CreateTestMethod(mt)).ToArray());
            return @class;
        }

        internal MethodDeclarationSyntax CreateTestMethod(Method methodDeclaration)
        {
            var method = SyntaxFactory.MethodDeclaration(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)), methodDeclaration.Name + "Test")
                .WithAttributeLists(
                    SyntaxFactory.SingletonList<AttributeListSyntax>(
                        SyntaxFactory.AttributeList(
                            SyntaxFactory.SingletonSeparatedList<AttributeSyntax>(
                                SyntaxFactory.Attribute(
                                    SyntaxFactory.IdentifierName("Test"))))))
                .WithModifiers(
                     SyntaxFactory.TokenList(
                         SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                .WithBody(CreateTestMethodBody(methodDeclaration));
            return method;
        }

        internal MethodDeclarationSyntax CreateSetUpMethod(Class classDeclaration)
        {
            var method = SyntaxFactory.MethodDeclaration(
                SyntaxFactory.PredefinedType(
                    SyntaxFactory.Token(
                        SyntaxKind.VoidKeyword)),
                "SetUp")
                 .WithAttributeLists(
                      SyntaxFactory.SingletonList<AttributeListSyntax>(
                          SyntaxFactory.AttributeList(
                              SyntaxFactory.SingletonSeparatedList<AttributeSyntax>(
                                  SyntaxFactory.Attribute(
                                      SyntaxFactory.IdentifierName("SetUp"))))))
                 .WithModifiers(
                      SyntaxFactory.TokenList(
                          SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                 .WithBody(
                     SyntaxFactory.Block(
                         classDeclaration.Parameters.Select(
                             pr => SyntaxFactory.ExpressionStatement(
                                 SyntaxFactory.AssignmentExpression(
                                     SyntaxKind.SimpleAssignmentExpression,
                                     SyntaxFactory.IdentifierName(pr.Name + "Global"),
                                     pr.Type.GetMockOrDefaultValue())))));
            method = method.WithBody(method.Body.AddStatements(SyntaxFactory.ExpressionStatement(
                SyntaxFactory.AssignmentExpression(
                    SyntaxKind.SimpleAssignmentExpression,
                    SyntaxFactory.IdentifierName(classDeclaration.Name + "Global"),
                    classDeclaration.ClassCreation()))));
            return method;
        }

        internal BlockSyntax CreateTestMethodBody(Method methodDeclaration)
        {
            var body = SyntaxFactory.Block();
            body = body.AddStatements(CreateArrange(methodDeclaration));
            body = body.AddStatements(CreateAct(methodDeclaration).ToArray());
            body = body.AddStatements(CreateAssert(methodDeclaration).ToArray());
            return body;
        }

        internal StatementSyntax[] CreateArrange(Method methodDeclaration)
        {
            return methodDeclaration.Parameters.Select(
                pr => SyntaxFactory.LocalDeclarationStatement(
                    CreateVariable(
                        pr.Type.GetMockOrDefaultType(),
                        pr.Name,
                        pr.Type.GetMockOrDefaultValue())))
                .ToArray();
        }

        internal List<StatementSyntax> CreateAct(Method methodDeclaration)
        {
            List<StatementSyntax> result = new List<StatementSyntax>();
            var callSignature = methodDeclaration.GetCallSignature(methodDeclaration.Parent.IsStatic ? methodDeclaration.Parent.Name : methodDeclaration.Parent.Name + "Global");
            if (methodDeclaration.ReturnType.Name.Equals("void"))
            { 
                if (methodDeclaration.Parameters.Where(pr => pr.Modifiers.Length != 0).Any())
                {
                    result.Add(SyntaxFactory.ExpressionStatement(callSignature));
                }
                else
                {
                    result.Add(SyntaxFactory.LocalDeclarationStatement(
                        CreateVariable(
                            SyntaxFactory.PredefinedType(
                                SyntaxFactory.Token(
                                    SyntaxKind.IntKeyword)),
                            "actual",
                            SyntaxFactory.LiteralExpression(
                                SyntaxKind.NumericLiteralExpression,
                                SyntaxFactory.Literal(0)))));
                    result.Add(SyntaxFactory.TryStatement()
                        .WithCatches(
                            SyntaxFactory.SingletonList<CatchClauseSyntax>(
                                SyntaxFactory.CatchClause()
                                .WithBlock(
                                    SyntaxFactory.Block(
                                        SyntaxFactory.SingletonList<StatementSyntax>(
                                            SyntaxFactory.ExpressionStatement(
                                                SyntaxFactory.AssignmentExpression(
                                                    SyntaxKind.SimpleAssignmentExpression,
                                                    SyntaxFactory.IdentifierName("actual"),
                                                    SyntaxFactory.LiteralExpression(
                                                        SyntaxKind.NumericLiteralExpression,
                                                        SyntaxFactory.Literal(1)))))))))
                        .WithBlock(
                            SyntaxFactory.Block(
                                SyntaxFactory.SingletonList<StatementSyntax>(
                                    SyntaxFactory.ExpressionStatement(callSignature)))));
                }
            }
            else
            {
                result.Add(SyntaxFactory.LocalDeclarationStatement(
                    CreateVariable(
                        methodDeclaration.ReturnType.GetMockOrDefaultType(),
                        "actual",
                        callSignature)));
            }
            return result;
        }

        internal List<StatementSyntax> CreateAssert(Method methodDeclaration)
        {
            List<StatementSyntax> result = new List<StatementSyntax>();
            if (!methodDeclaration.ReturnType.Name.Equals("void"))
            {
                if (!methodDeclaration.ReturnType.IsDefault)
                {
                    result.Add(SyntaxFactory.ExpressionStatement(
                        CreateAssertFunc(
                            "IsNotNull",
                            SyntaxFactory.ArgumentList(
                                SyntaxFactory.SingletonSeparatedList<ArgumentSyntax>(
                                    SyntaxFactory.Argument(
                                        SyntaxFactory.IdentifierName("actual")))))));
                }
                else
                {
                    result.Add(SyntaxFactory.LocalDeclarationStatement(
                        CreateVariable(
                            methodDeclaration.ReturnType.GetMockOrDefaultType(),
                            "expected",
                            methodDeclaration.ReturnType.GetMockOrDefaultValue())));
                    result.Add(SyntaxFactory.ExpressionStatement(
                        CreateAssertFunc(
                            "AreEqual",
                            SyntaxFactory.ArgumentList(
                                SyntaxFactory.SeparatedList<ArgumentSyntax>(
                                    new[]
                                    { 
                                        SyntaxFactory.Argument(
                                            SyntaxFactory.IdentifierName("expected")),
                                        SyntaxFactory.Argument(
                                            SyntaxFactory.IdentifierName("actual"))
                                    })))));
                }
            }
            else
            {
                Parameter[] parameters = methodDeclaration.Parameters.Where(pr => pr.Modifiers.Contains("out") || pr.Modifiers.Contains("ref")).ToArray();
                if (parameters.Length != 0)
                {
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        if (parameters[i].Type.IsDefault)
                        {
                            result.Add(SyntaxFactory.LocalDeclarationStatement(
                                CreateVariable(
                                    parameters[i].Type.GetMockOrDefaultType(),
                                    parameters[i].Name + "Expected",
                                    parameters[i].Type.GetMockOrDefaultValue())));
                            result.Add(SyntaxFactory.ExpressionStatement(
                                CreateAssertFunc(
                                    "AreEqual",
                                    SyntaxFactory.ArgumentList(
                                        SyntaxFactory.SeparatedList<ArgumentSyntax>(
                                            new[]
                                            {
                                            SyntaxFactory.Argument(
                                                SyntaxFactory.IdentifierName(parameters[i].Name + "Expected")),
                                            parameters[i].GetParameter(parameters[i].Name, false)
                                            })))));
                        }
                        else
                        {
                            result.Add(SyntaxFactory.ExpressionStatement(
                                CreateAssertFunc(
                                    "IsNotNull",
                                    SyntaxFactory.ArgumentList(
                                        SyntaxFactory.SingletonSeparatedList<ArgumentSyntax>(
                                            parameters[i].GetParameter(parameters[i].Name, false))))));
                        }
                    }
                }
                else
                {
                    result.Add(SyntaxFactory.ExpressionStatement(
                        CreateAssertFunc(
                            "Zero",
                            SyntaxFactory.ArgumentList(
                                SyntaxFactory.SingletonSeparatedList<ArgumentSyntax>(
                                    SyntaxFactory.Argument(
                                        SyntaxFactory.IdentifierName("actual")))))));
                }
            }
            result.Add(SyntaxFactory.ExpressionStatement(
                CreateAssertFunc(
                    "Fail",
                    SyntaxFactory.ArgumentList(
                        SyntaxFactory.SingletonSeparatedList<ArgumentSyntax>(
                            SyntaxFactory.Argument(
                                SyntaxFactory.LiteralExpression(
                                    SyntaxKind.StringLiteralExpression,
                                    SyntaxFactory.Literal("autogenerated"))))))));
            return result;
        }

        internal VariableDeclarationSyntax CreateVariable(TypeSyntax type, string name, ExpressionSyntax initializer)
        {
            return SyntaxFactory.VariableDeclaration(type)
                .WithVariables(
                    SyntaxFactory.SingletonSeparatedList<VariableDeclaratorSyntax>(
                        SyntaxFactory.VariableDeclarator(
                            SyntaxFactory.Identifier(name))
                        .WithInitializer(
                            SyntaxFactory.EqualsValueClause(initializer))));
        }

        internal InvocationExpressionSyntax CreateAssertFunc(string funcName, ArgumentListSyntax arguments)
        {
            return SyntaxFactory.InvocationExpression(
                SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    SyntaxFactory.IdentifierName("Assert"),
                    SyntaxFactory.IdentifierName(funcName)))
                .WithArgumentList(arguments);
        }
    }
}
