namespace TestsGenerator.Core
{
    public record GenerationResult(string ClassName, string Content);
    internal class MethodNameGenerationInfo
    {
        public int LastGenerationNum { get; set; } = 0;
        public int MethodsWithTheSameNameCount { get; set; }

        public MethodNameGenerationInfo(int methodsWithTheSameNameCount)
        {
            MethodsWithTheSameNameCount = methodsWithTheSameNameCount;
        }
    }
}
