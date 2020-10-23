using System.Linq;
using System.Collections.Generic;
using NUnit.Framework;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TestGeneratorLib;

namespace NUnitTestGenerator
{
    public class Tests
    {
        private SyntaxNode _root;
        private const string example = @"using System;
                                         using System.Collections.Generic;
                                         using System.Linq;
                                         using System.Text;
                                         using System.Threading.Tasks;
                                         using TestLibrary.Tree.TreeNode;
                                         
                                         namespace TestLibrary.Tree
                                         {
                                             public class TreeClass
                                             {
                                                 public TreeClass(int count, Tree tree)
                                                 {
                                         
                                                 }
                                         
                                                 public void AddElem(int a, out IList elem)
                                                 {
                                                     
                                                 }
                                         
                                                 public int CheckElem()
                                                 {
                                         
                                                 }
                                         
                                                 public void DeleteElem<T>()
                                                 {
                                         
                                                 }
                                             }

                                             public static class Second
                                             {
                                                 public static int Check()
                                                 {
                                             
                                                 }
                                             }
                                         }";

        [SetUp]
        public void Init()
        {
            _root = CSharpSyntaxTree.ParseText(TestGenerator.Generate(example)).GetRoot();
        }

        [Test]
        public void CheckUsings()
        {
            List<string> usingsName = new List<string>() { "System", "System.Collections.Generic", "System.Linq", "System.Text", "System.Threading.Tasks", "TestLibrary.Tree.TreeNode", "NUnit.Framework", "Moq" };
            var usings = _root.DescendantNodes().OfType<UsingDirectiveSyntax>().Select(us => us.Name.ToString()).ToList();
            Assert.AreEqual(usingsName.Count, usings.Count);
            foreach (var name in usingsName)
            {
                Assert.IsTrue(usings.Contains(name));
            }
        }

        [Test]
        public void CheckClass()
        {
            List<string> classesName = new List<string>() { "TreeClassTests", "SecondTests" };
            var classes = _root.DescendantNodes().OfType<ClassDeclarationSyntax>().Select(cl => cl.Identifier.ValueText).ToList();
            Assert.AreEqual(classesName.Count, classes.Count);
            foreach (var name in classesName)
            {
                Assert.IsTrue(classes.Contains(name));
            }
        }

        [Test]
        public void CheckMethod()
        {
            List<string> methodsName = new List<string>() { "AddElemTest", "CheckElemTest", "CheckTest", "SetUp" };
            var methods = _root.DescendantNodes().OfType<MethodDeclarationSyntax>().Select(mt => mt.Identifier.ValueText).ToList();
            Assert.AreEqual(methodsName.Count, methods.Count);
            foreach (var name in methodsName)
            {
                Assert.IsTrue(methods.Contains(name));
            }
        }

        [Test]
        public void CheckSetUpMethod()
        {
            var statements = _root.DescendantNodes().OfType<MethodDeclarationSyntax>().Where(mt => mt.Identifier.ValueText.Equals("SetUp")).First()
                .DescendantNodes().OfType<ExpressionStatementSyntax>().ToList();
            Assert.AreEqual(3, statements.Count);
        }

        [Test]
        public void CheckInvokeMethod()
        {
            var invokeMethod = _root.DescendantNodes().OfType<MethodDeclarationSyntax>().Where(mt => mt.Identifier.ValueText.Equals("CheckElemTest")).First();
            var expected = SyntaxFactory.ExpressionStatement(SyntaxFactory.ParseExpression("int actual = TreeClassGlobal.CheckElem()"));
            Assert.IsTrue(invokeMethod.ToString().Contains(expected.ToString()));
        }
    }
}