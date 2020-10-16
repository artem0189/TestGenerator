using System;
using TestGeneratorLib;

namespace TestGeneratorConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            TestGenerator t = new TestGenerator(0, 2, 3);
        }
    }
}
