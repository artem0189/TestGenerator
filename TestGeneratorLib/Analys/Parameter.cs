using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TestGeneratorLib.Analys
{
    internal class Parameter
    {
        internal string Name { get; }
        internal Type Type { get; }
        internal string[] Modifiers { get; }

        internal Parameter(ParameterSyntax parameterDeclaration)
        {
            Name = parameterDeclaration.Identifier.ValueText;
            Type = new Type(parameterDeclaration.Type);
            Modifiers = parameterDeclaration.Modifiers.Select(md => md.ValueText).ToArray();
        }

        internal ArgumentSyntax GetParameter(string paramName, bool withModifiers)
        {
            ArgumentSyntax param = null;
            if (Type.IsDefault)
            {
                param = SyntaxFactory.Argument(
                    SyntaxFactory.IdentifierName(paramName));
            }
            else
            {
                param = SyntaxFactory.Argument(
                    SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        SyntaxFactory.IdentifierName(paramName),
                        SyntaxFactory.IdentifierName("Object")));
            }

            if (withModifiers && Modifiers.Length != 0)
            {
                param = param.WithRefKindKeyword(
                    SyntaxFactory.ParseToken(Modifiers[0]));
            }
            return param;
        }
    }
}
