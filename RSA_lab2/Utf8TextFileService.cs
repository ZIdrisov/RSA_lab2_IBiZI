using System.Text;

namespace RSA_lab2;

public sealed class Utf8TextFileService : ITextFileService
{
    public string Read(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        return File.ReadAllText(filePath, Encoding.UTF8);
    }

    public void Write(string filePath, string content)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        ArgumentNullException.ThrowIfNull(content);
        File.WriteAllText(filePath, content, Encoding.UTF8);
    }
}
