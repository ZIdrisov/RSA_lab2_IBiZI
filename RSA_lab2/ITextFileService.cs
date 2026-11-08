namespace RSA_lab2;

public interface ITextFileService
{
    string Read(string filePath);

    void Write(string filePath, string content);
}
