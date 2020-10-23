using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using TestGeneratorLib;
using TestGeneratorConsoleApp.IO;

namespace TestGeneratorConsoleApp
{
    public class Pipeline
    {
        private string FileFolder { get; }
        private int FilesInputCount { get; set; } = 1;
        private int FilesOutputCount { get; set; } = 1;
        private int TasksCount { get; set; } = 1;

        public Pipeline(string fileFolder)
        {
            FileFolder = fileFolder;
        }

        public void SetParameters(int filesInputCount, int filesOutputCount, int tasksCount)
        {
            FilesInputCount = filesInputCount;
            FilesOutputCount = filesOutputCount;
            TasksCount = tasksCount;
        }

        public async Task Processing(List<string> filesPath)
        {
            var readingBlock = new TransformBlock<string, File>(
                async filePath => await ReadFile(filePath),
                new ExecutionDataflowBlockOptions
                {
                    MaxDegreeOfParallelism = FilesInputCount
                });
            var generatingBlock = new TransformBlock<File, File>(
                async file => await GenerateTest(file),
                new ExecutionDataflowBlockOptions
                {
                    MaxDegreeOfParallelism = TasksCount
                });
            var writingBlock = new ActionBlock<File>(
                async file => await WriteFile(file),
                new ExecutionDataflowBlockOptions
                {
                    MaxDegreeOfParallelism = FilesOutputCount
                });

            readingBlock.LinkTo(generatingBlock, new DataflowLinkOptions { PropagateCompletion = true });
            generatingBlock.LinkTo(writingBlock, new DataflowLinkOptions { PropagateCompletion = true });

            for (int i = 0; i < filesPath.Count; i++)
            {
                readingBlock.Post(filesPath[i]);
            }
            readingBlock.Complete();

            await writingBlock.Completion;
        }

        private async Task<File> ReadFile(string filePath)
        {
            File file = new File(FileFolder, filePath);
            await file.ReadFromFile(filePath);
            return file;
        }

        private async Task<File> GenerateTest(File file)
        {
            file.FileContent = await Task.Run(() => TestGenerator.Generate(file.FileContent));
            return file;
        }

        private async Task WriteFile(File file)
        {
            await file.Write();
        }
    }
}

