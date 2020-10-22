using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TestGeneratorLib.Analys
{
    internal class Unit
    {
        internal string[] Usings { get; }
        internal Namespace[] Namespaces { get; }

        internal Unit(SyntaxNode root)
        {
            Usings = root.DescendantNodes().OfType<UsingDirectiveSyntax>().Select(us => us.Name.ToString()).ToArray();
            Namespaces = root.DescendantNodes().OfType<NamespaceDeclarationSyntax>().Select(nm => new Namespace(nm)).ToArray();
        }
    }
}
