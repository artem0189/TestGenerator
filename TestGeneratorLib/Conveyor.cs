using System;
using System.Threading.Tasks.Dataflow;

namespace TestGeneratorLib
{
    internal class Conveyor
    {
        private int FilesInputCount { get; }
        private int FilesOutputCount { get; }
        private int TasksCount { get; }

        internal Conveyor(int filesInputCount, int filesOutputCount, int tasksCount)
        {
            FilesInputCount = filesInputCount;
            FilesOutputCount = filesOutputCount;
            TasksCount = tasksCount;
        }
    }
}
