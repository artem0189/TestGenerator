using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TestGeneratorLib.Analys
{
    internal class Namespace
    {
        internal string Name { get; }
        internal Class[] Classes { get; }

        internal Namespace(NamespaceDeclarationSyntax namespaceDeclaration)
        {
            Name = namespaceDeclaration.Name.ToString();
            Classes = namespaceDeclaration.DescendantNodes().OfType<ClassDeclarationSyntax>().Where(cl => cl.TypeParameterList == null).Select(cl => new Class(cl)).ToArray();
        }
    }
}
