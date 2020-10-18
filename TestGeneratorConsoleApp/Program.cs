using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TestGeneratorConsoleApp
{
    class Program
    { 
        static List<string> InputFiles()
        {
            List<string> result = new List<string>();

            string inputValue = Console.ReadLine();
            while (inputValue != "")
            {
                result.Add(inputValue);
                inputValue = Console.ReadLine();
            }

            return result;
        }

        static async Task Main(string[] args)
        {
            Console.WriteLine("Введите файлы для генерации тестов:");
            List<string> files = InputFiles();
            Console.WriteLine("Введите папку для вывода:");
            string folder = Console.ReadLine();
            Console.WriteLine("Введите параметры конвейера:");
            int[] pipelineParams = new int[3];
            for (int i = 0; i < 3; i++)
            {
                Int32.TryParse(Console.ReadLine(), out pipelineParams[i]);
            }

            Pipeline pipeline = new Pipeline(folder);
            pipeline.SetParameters(pipelineParams[0], pipelineParams[1], pipelineParams[3]);
        }
    }
}
