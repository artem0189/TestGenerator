using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TestGeneratorLib.Analys
{
    internal class Class
    {
        internal string Name { get; }
        internal bool IsStatic { get; }
        internal Parameter[] Parameters { get; }
        internal Method[] Methods { get; }

        internal Class(ClassDeclarationSyntax classDeclaration)
        {
            Name = classDeclaration.Identifier.ValueText;
            IsStatic = classDeclaration.Modifiers.Where(md => md.Kind().Equals(SyntaxKind.StaticKeyword)).Any();
            Parameters = GetConstructor(classDeclaration).ParameterList.Parameters.Select(pr => new Parameter(pr)).ToArray();
            Methods = classDeclaration.DescendantNodes().OfType<MethodDeclarationSyntax>().Where(mt => mt.Modifiers.Where(md => md.Kind().Equals(SyntaxKind.PublicKeyword)).Any()).Select(mt => new Method(this, mt)).ToArray();
        }

        private ConstructorDeclarationSyntax GetConstructor(ClassDeclarationSyntax classDeclaration)
        {
            var constructors = classDeclaration.DescendantNodes().OfType<ConstructorDeclarationSyntax>();
            return constructors.OrderBy(cn => cn.ParameterList.Parameters.Count).FirstOrDefault();
        }

        internal ObjectCreationExpressionSyntax ClassCreation()
        {
            return SyntaxFactory.ObjectCreationExpression(
                SyntaxFactory.IdentifierName(Name))
                .WithArgumentList(
                    SyntaxFactory.ArgumentList(
                        SyntaxFactory.SeparatedList<ArgumentSyntax>(
                            Parameters.Select(
                                pr => pr.GetParameter(pr.Name + "Global", true))
                            .ToList())));
        }
    }
}
