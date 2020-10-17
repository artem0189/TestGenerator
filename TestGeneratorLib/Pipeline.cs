using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace TestGeneratorLib
{
    internal class Pipeline
    {
        private int FilesInputCount { get; }
        private int FilesOutputCount { get; }
        private int TasksCount { get; }

        internal Pipeline(int filesInputCount, int filesOutputCount, int tasksCount)
        {
            FilesInputCount = filesInputCount;
            FilesOutputCount = filesOutputCount;
            TasksCount = tasksCount;
        }

        internal async Task Processing(List<string> filesName)
        {

        }

        private async Task<string> ReadFile(string fileName)
        {

        }

        private async Task<>
    }
}
