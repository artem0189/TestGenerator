using System;
using System.Collections.Generic;

namespace TestGeneratorLib
{
    public class TestGenerator
    {
        private Conveyor _conveyor;
        
        public TestGenerator(int filesInputCount, int filesOutputCount, int tasksCount)
        {
            _conveyor = new Conveyor(filesInputCount, filesOutputCount, tasksCount);
        }

        public void Generate(List<string> inputFiles, string outputFolder)
        {

        }
    }
}
