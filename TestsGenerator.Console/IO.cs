namespace TestsGenerator.Console
{
    internal static class IO
    {
        public static async Task<string> AsyncRead(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                throw new ArgumentException($"{nameof(filePath)} is invalid!");

            using var sr = new StreamReader(filePath);
            return await sr.ReadToEndAsync();
        }

        public static async Task AsyncWrite(string filePath, string content)
        {
            using var sw = new StreamWriter(filePath);
            await sw.WriteAsync(content);
        }
    }
}
