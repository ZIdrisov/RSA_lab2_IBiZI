namespace RSA_lab2;

public sealed class TestFileGenerator
{
    private readonly ITextFileService textFileService;

    public TestFileGenerator(ITextFileService textFileService)
    {
        this.textFileService = textFileService ?? throw new ArgumentNullException(nameof(textFileService));
    }

    public void Generate(string directoryPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directoryPath);
        Directory.CreateDirectory(directoryPath);

        string[] testTexts =
        {
            "Hello, RSA!",
            "Short test: 12345.",
            "Привет, RSA! Это короткая проверка.",
            "English text with punctuation: A, B, C; test #4!",
            "Русский текст: числа 2025 и знаки !?,.;:",
            "Line one.\r\nLine two.\r\nLine three.",
            "RSA test 7: 0 1 2 3 4 5 6 7 8 9.",
            "Длинная фраза для проверки работы шифрования и расшифрования текста.",
            "Mixed text: Пример ABC xyz 42% + - = / * ( ) [ ] { }.",
            "A longer final sample: RSA protects messages, including русский текст."
        };

        for (int index = 0; index < testTexts.Length; index++)
        {
            string filePath = Path.Combine(directoryPath, $"test{index + 1}.txt");
            textFileService.Write(filePath, testTexts[index]);
        }
    }
}
