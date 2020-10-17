using System;
using System.Collections.Generic;

namespace TestGeneratorLib
{
    public class TestGenerator
    {
        private Pipeline _conveyor;
        
        public TestGenerator(int filesInputCount, int filesOutputCount, int tasksCount)
        {
            _conveyor = new Pipeline(filesInputCount, filesOutputCount, tasksCount);
        }

        public void Generate(List<string> inputFiles, string outputFolder)
        {

        }
    }
}
