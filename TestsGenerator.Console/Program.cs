
using System.Threading.Tasks.Dataflow;
using TestsGenerator.Console;
using TestsGenerator.Core;

string[] arguments = new string[]
{
    "C:\\Work\\5 semester\\MPP\\MPP\\DirectoryScanner",
    "C:\\Work\\5 semester\\MPP\\MPP\\GenerationResult",
    "1", "1", "1"
};

args = arguments;
if (args.Length != 5)
{
    Console.WriteLine("Incorrect count of arguments");
    return; 
}

string inputDir = args[0];
string outputDir = args[1];
if (!int.TryParse(args[2], out var maxFilesRead))
{
    Console.WriteLine("Max files reading parallel argument should be int");
    return;
}

if (!int.TryParse(args[3], out var maxFilesWrite))
{
    Console.WriteLine("Max files writing parallel argument should be int");
    return;
}

if (!int.TryParse(args[4], out var maxFilesParse))
{
    Console.WriteLine("Max files parsing parallel argument should be int");
    return;
}

try
{
    if (Directory.Exists(outputDir))
        Directory.Delete(outputDir, true);
    Directory.CreateDirectory(outputDir);

    var generator = new TestsGenerator.Core.TestsGenerator();
    DataflowPipeline dataflowPipeline = new DataflowPipeline(outputDir, maxFilesRead, maxFilesWrite, maxFilesParse);
    TransformBlock<string, string> startPoint = dataflowPipeline.GenerateDataflowPipeline(generator);

    AddSubdirToQuery(inputDir, startPoint);
    startPoint.Complete();
    await startPoint.Completion;
    Console.WriteLine("Done!");
    Console.ReadLine();
}
catch (Exception exc)
{
    Console.WriteLine(exc.ToString());
}

static void AddSubdirToQuery(string path, TransformBlock<string, string> startPoint)
{
    try
    {
        DirectoryInfo dirInfo = new DirectoryInfo(path);
        DirectoryInfo[] dirInfos = dirInfo.GetDirectories();
        foreach (DirectoryInfo info in dirInfos)
            AddSubdirToQuery(info.FullName, startPoint);

        FileInfo[] fileInfos = dirInfo.GetFiles();
        foreach (FileInfo fileInfo in fileInfos)
            if (fileInfo.Extension == ".cs")
                startPoint.Post(fileInfo.FullName);
    }
    catch (Exception exc)
    {
        Console.WriteLine($"Can't read directory with this path: {path}\nException: {exc}");
    }
}