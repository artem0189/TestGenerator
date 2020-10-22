using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TestGeneratorLib.Analys
{
    internal class Type
    {
        internal string Name { get; }
        internal bool IsDefault { get; }

        internal Type(TypeSyntax typeDeclaration)
        {
            Name = typeDeclaration.ToString();
            IsDefault = typeDeclaration.Kind().Equals(SyntaxKind.PredefinedType);
        }

        internal TypeSyntax GetMockOrDefaultType()
        {
            TypeSyntax type = null;
            if (!IsDefault)
            {
                type = SyntaxFactory.GenericName(
                    SyntaxFactory.Identifier("Mock"))
                .WithTypeArgumentList(
                    SyntaxFactory.TypeArgumentList(
                        SyntaxFactory.SingletonSeparatedList<TypeSyntax>(
                            SyntaxFactory.IdentifierName(Name))));
            }
            else
            {
                type = SyntaxFactory.IdentifierName(Name);
            }
            return type;
        }

        internal ExpressionSyntax GetMockOrDefaultValue()
        {
            ExpressionSyntax value = null;
            if (!IsDefault)
            {
                value = SyntaxFactory.ObjectCreationExpression(
                    SyntaxFactory.GenericName(
                        SyntaxFactory.Identifier("Mock"))
                    .WithTypeArgumentList(
                        SyntaxFactory.TypeArgumentList(
                            SyntaxFactory.SingletonSeparatedList<TypeSyntax>(
                                SyntaxFactory.IdentifierName(Name)))))
                    .WithArgumentList(SyntaxFactory.ArgumentList());
            }
            else
            {
                value = SyntaxFactory.DefaultExpression(
                    SyntaxFactory.IdentifierName(Name));
            }
            return value;
        }
    }
}
