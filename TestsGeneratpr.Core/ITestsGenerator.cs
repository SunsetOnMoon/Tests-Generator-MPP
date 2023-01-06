using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestsGenerator.Core
{
    public interface ITestsGenerator
    {
        public List<GenerationResult> Generate(string file, int generationTemplate);
    }
}
