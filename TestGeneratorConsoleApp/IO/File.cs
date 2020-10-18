using System.IO;
using System.Threading.Tasks;

namespace TestGeneratorConsoleApp.IO
{
    internal class File
    {
        internal string FileName { get; }
        internal string FolderPath { get; }
        internal string FileContent { get; set; }

        internal File(string folderPath, string filePath)
        {
            FolderPath = folderPath;
            FileName = "TestTemplate" + Path.GetFileName(filePath);
        }

        internal async Task ReadFromFile(string filePath)
        {
            using (StreamReader reader = new StreamReader(filePath))
            {
                FileContent = await reader.ReadToEndAsync();
            }
        }

        internal async Task Write()
        {
            using (StreamWriter writer = new StreamWriter(FolderPath + "/" + FileName))
            {
                await writer.WriteAsync(FileContent);
            }
        }
    }
}
