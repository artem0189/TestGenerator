using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TestGeneratorLib.Analys
{
    internal class Method
    {
        internal Class Parent { get; }
        internal string Name { get; }
        internal Type ReturnType { get; }
        internal Parameter[] Parameters { get; }

        internal Method(Class parent, MethodDeclarationSyntax methodDeclaration)
        {
            Parent = parent;
            Name = methodDeclaration.Identifier.ValueText;
            ReturnType = new Type(methodDeclaration.ReturnType);
            Parameters = methodDeclaration.ParameterList.Parameters.Select(pr => new Parameter(pr)).ToArray();
        }

        internal InvocationExpressionSyntax GetCallSignature(string callerObject)
        {
            return SyntaxFactory.InvocationExpression(
                SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    SyntaxFactory.IdentifierName(callerObject),
                    SyntaxFactory.IdentifierName(Name)),
                SyntaxFactory.ArgumentList(
                    SyntaxFactory.SeparatedList<ArgumentSyntax>(
                        Parameters.Select(pr => pr.GetParameter(pr.Name, true)))));
        }
    }
}
