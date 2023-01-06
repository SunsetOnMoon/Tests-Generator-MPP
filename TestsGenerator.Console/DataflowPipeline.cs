using System.Threading.Tasks.Dataflow;
using TestsGenerator.Core;

namespace TestsGenerator.Console
{
    internal class DataflowPipeline
    {
        private readonly string _outputDir;
        private readonly int _maxFilesRead;
        private readonly int _maxFilesWrite;
        private readonly int _maxFilesParse;
        private readonly int _generationTemplate;

        public DataflowPipeline(string outputDir, int maxFilesRead, int maxFilesWrite, int maxFilesParse, int generationTemplate)
        {
            if (maxFilesRead <= 0)
                throw new ArgumentException($"Should be >= 0, {nameof(maxFilesRead)}");
            if (maxFilesWrite <= 0)
                throw new ArgumentException($"Should be >= 0, {nameof(maxFilesWrite)}");
            if (maxFilesParse <= 0)
                throw new ArgumentException($"Should be >= 0, {nameof(maxFilesParse)}");

            _outputDir = outputDir;
            _maxFilesRead = maxFilesRead;
            _maxFilesWrite = maxFilesWrite;
            _maxFilesParse = maxFilesParse;
            _generationTemplate = generationTemplate;
        }

        public TransformBlock<string, string> GenerateDataflowPipeline(ITestsGenerator generator)
        {
            TransformBlock<string, string> fileRead = new TransformBlock<string, string>(
                async path => await IO.AsyncRead(path),
                new ExecutionDataflowBlockOptions
                {
                    MaxDegreeOfParallelism = _maxFilesRead
                });

            TransformManyBlock<string, GenerationResult> testsGenerate = new TransformManyBlock<string, GenerationResult>(
                data => generator.Generate(data, _generationTemplate),
                new ExecutionDataflowBlockOptions
                {
                    MaxDegreeOfParallelism = _maxFilesParse
                });

            ActionBlock<GenerationResult> fileWrite = new ActionBlock<GenerationResult>(
                async data =>
                {
                    string ouputFileName = data.ClassName + "Tests";
                    string fullPath = Path.Combine(_outputDir, ouputFileName);
                    fullPath = Path.ChangeExtension(fullPath, ".cs");
                    await IO.AsyncWrite(fullPath, data.Content);
                },
                new ExecutionDataflowBlockOptions
                {
                    MaxDegreeOfParallelism = _maxFilesWrite
                });

            fileRead.LinkTo(testsGenerate, new DataflowLinkOptions { PropagateCompletion = true });
            testsGenerate.LinkTo(fileWrite, new DataflowLinkOptions { PropagateCompletion = true });

            return fileRead;


        }
    }
}
