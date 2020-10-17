using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TestGeneratorLib.IO
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

        internal async Task Read(string filePath)
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
