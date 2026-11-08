using System.Numerics;

namespace RSA_lab2;

public partial class Form1 : Form
{
    private readonly TextBox sourceTextBox = new();
    private readonly TextBox encryptedTextBox = new();
    private readonly TextBox decryptedTextBox = new();
    private readonly TextBox publicSBox = new();
    private readonly TextBox publicNBox = new();
    private readonly TextBox privateEBox = new();
    private readonly Button generateButton = new();
    private readonly Button encryptButton = new();
    private readonly Button decryptButton = new();
    private readonly Button loadButton = new();
    private readonly Button saveButton = new();

    private readonly IRsaService rsaService;
    private readonly ITextFileService textFileService;
    private RsaKeyPair? currentKeyPair;
    private List<BigInteger> encryptedBlocks = new();

    public Form1() : this(new RsaService(), new Utf8TextFileService())
    {
    }

    public Form1(IRsaService rsaService, ITextFileService textFileService)
    {
        this.rsaService = rsaService ?? throw new ArgumentNullException(nameof(rsaService));
        this.textFileService = textFileService ?? throw new ArgumentNullException(nameof(textFileService));
        InitializeComponent();
        InitializeUserInterface();
    }

    private void InitializeUserInterface()
    {
        Text = "RSA — алгоритм шифрования";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(850, 650);
        Size = new Size(1050, 780);

        sourceTextBox.Multiline = true;
        encryptedTextBox.Multiline = true;
        decryptedTextBox.Multiline = true;
        foreach (TextBox textBox in new[] { sourceTextBox, encryptedTextBox, decryptedTextBox })
        {
            textBox.Dock = DockStyle.Fill;
            textBox.ScrollBars = ScrollBars.Both;
            textBox.Font = new Font("Consolas", 10F);
        }

        foreach (TextBox textBox in new[] { publicSBox, publicNBox, privateEBox })
        {
            textBox.Dock = DockStyle.Fill;
            textBox.ReadOnly = true;
            textBox.Font = new Font("Consolas", 10F);
        }

        ConfigureButton(generateButton, "Сгенерировать ключи", GenerateButton_Click);
        ConfigureButton(encryptButton, "Зашифровать", EncryptButton_Click);
        ConfigureButton(decryptButton, "Расшифровать", DecryptButton_Click);
        ConfigureButton(loadButton, "Загрузить из файла", LoadButton_Click);
        ConfigureButton(saveButton, "Сохранить в файл", SaveButton_Click);

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(12),
            ColumnCount = 2,
            RowCount = 12,
            AutoScroll = true
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 210));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        int[] heights = { 38, 135, 42, 34, 34, 34, 42, 135, 42, 135, 42, 42 };
        foreach (int height in heights) layout.RowStyles.Add(new RowStyle(SizeType.Absolute, height));

        AddRow(layout, 0, "Исходный текст:", sourceTextBox);
        layout.Controls.Add(generateButton, 1, 2);
        layout.Controls.Add(new Label { Text = "Открытый ключ (s):", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 3);
        layout.Controls.Add(publicSBox, 1, 3);
        layout.Controls.Add(new Label { Text = "N:", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 4);
        layout.Controls.Add(publicNBox, 1, 4);
        layout.Controls.Add(new Label { Text = "Закрытый ключ (e):", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 5);
        layout.Controls.Add(privateEBox, 1, 5);
        layout.Controls.Add(encryptButton, 1, 6);
        AddRow(layout, 7, "Зашифрованные блоки:", encryptedTextBox);
        layout.Controls.Add(decryptButton, 1, 8);
        AddRow(layout, 9, "Восстановленный текст:", decryptedTextBox);
        layout.Controls.Add(loadButton, 1, 10);
        layout.Controls.Add(saveButton, 1, 11);
        Controls.Add(layout);
    }

    private static void AddRow(TableLayoutPanel layout, int row, string labelText, Control control)
    {
        layout.Controls.Add(new Label { Text = labelText, Dock = DockStyle.Fill, TextAlign = ContentAlignment.TopLeft }, 0, row);
        layout.Controls.Add(control, 1, row);
    }

    private static void ConfigureButton(Button button, string text, EventHandler handler)
    {
        button.Text = text;
        button.AutoSize = true;
        button.Padding = new Padding(10, 4, 10, 4);
        button.Anchor = AnchorStyles.Left;
        button.Click += handler;
    }

    private void GenerateButton_Click(object? sender, EventArgs e)
    {
        try
        {
            Cursor = Cursors.WaitCursor;
            currentKeyPair = rsaService.GenerateKeys();

            if (currentKeyPair.ModulusN.ToString().Length != 49)
            {
                throw new InvalidOperationException("Модуль N должен содержать ровно 49 десятичных знаков.");
            }

            publicSBox.Text = currentKeyPair.PublicKeyS.ToString();
            publicNBox.Text = currentKeyPair.ModulusN.ToString();
            privateEBox.Text = currentKeyPair.PrivateKeyE.ToString();
            encryptedBlocks.Clear();
            encryptedTextBox.Clear();
            decryptedTextBox.Clear();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Ошибка генерации ключей", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            Cursor = Cursors.Default;
        }
    }

    private void EncryptButton_Click(object? sender, EventArgs e)
    {
        if (!KeysAreInitialized()) return;

        try
        {
            encryptedBlocks = rsaService.Encrypt(sourceTextBox.Text, currentKeyPair!).ToList();
            encryptedTextBox.Text = string.Join(" ", encryptedBlocks);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Ошибка шифрования", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void DecryptButton_Click(object? sender, EventArgs e)
    {
        if (!KeysAreInitialized()) return;

        try
        {
            encryptedBlocks = ParseEncryptedBlocks(encryptedTextBox.Text);
            decryptedTextBox.Text = rsaService.Decrypt(encryptedBlocks, currentKeyPair!);
        }
        catch (Exception ex) when (ex is FormatException or OverflowException or ArgumentException)
        {
            MessageBox.Show(this, ex.Message, "Ошибка расшифровки", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void LoadButton_Click(object? sender, EventArgs e)
    {
        using var dialog = new OpenFileDialog { Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*" };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;

        try
        {
            sourceTextBox.Text = textFileService.Read(dialog.FileName);
        }
        catch (IOException ex)
        {
            MessageBox.Show(this, ex.Message, "Ошибка загрузки файла", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void SaveButton_Click(object? sender, EventArgs e)
    {
        using var dialog = new SaveFileDialog { Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*", DefaultExt = "txt" };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;

        try
        {
            textFileService.Write(dialog.FileName, decryptedTextBox.Text);
        }
        catch (IOException ex)
        {
            MessageBox.Show(this, ex.Message, "Ошибка сохранения файла", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private bool KeysAreInitialized()
    {
        if (currentKeyPair is not null) return true;
        MessageBox.Show(this, "Сначала сгенерируйте ключи.", "RSA", MessageBoxButtons.OK, MessageBoxIcon.Information);
        return false;
    }

    private static List<BigInteger> ParseEncryptedBlocks(string text)
    {
        string[] tokens = text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        var blocks = new List<BigInteger>(tokens.Length);

        foreach (string token in tokens)
        {
            if (!BigInteger.TryParse(token, out BigInteger block))
            {
                throw new FormatException($"Некорректный зашифрованный блок: {token}");
            }

            blocks.Add(block);
        }

        return blocks;
    }
}
