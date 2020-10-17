using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using TestGeneratorLib.IO;

namespace TestGeneratorLib
{
    internal class Pipeline
    {
        private string FileFolder { get; }
        private int FilesInputCount { get; }
        private int FilesOutputCount { get; }
        private int TasksCount { get; }

        internal Pipeline(int filesInputCount, int filesOutputCount, int tasksCount)
        {
            FilesInputCount = filesInputCount;
            FilesOutputCount = filesOutputCount;
            TasksCount = tasksCount;
        }

        internal async Task Processing(List<string> filesPath)
        {
            var readingBlock = new TransformBlock<string, File>(
                async filePath => await ReadFile(filePath),
                new ExecutionDataflowBlockOptions
                {
                    MaxDegreeOfParallelism = FilesInputCount
                });
            var generatingBlock = new TransformBlock<File, File>(
                file => GenerateTest(file),
                new ExecutionDataflowBlockOptions
                {
                    MaxDegreeOfParallelism = TasksCount
                });
            var writingBlock = new ActionBlock<File>(
                file => WriteFile(file),
                new ExecutionDataflowBlockOptions
                {
                    MaxDegreeOfParallelism = FilesOutputCount
                });

            readingBlock.LinkTo(generatingBlock);
            generatingBlock.LinkTo(writingBlock);

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
            await file.Read(filePath);
            return file;
        }

        private File GenerateTest(File file)
        {
            return file;
        }

        private async Task WriteFile(File file)
        {
            await file.Write();
        }
    }
}
