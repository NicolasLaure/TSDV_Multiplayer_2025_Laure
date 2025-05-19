using System.IO;

namespace Network.FileManagement;

public class FileHandler
{
    private string path;

    public FileHandler(string path)
    {
        this.path = path;
    }

    public string ReadFile()
    {
        if (!File.Exists(path))
            return "";

        StreamReader reader = new StreamReader(path);
        string text = reader.ReadToEnd();
        reader.Close();
        return text;
    }

    public void WriteFile(string text)
    {
        StreamWriter writer = new StreamWriter(path);
        writer.Write(text);
        writer.Close();
    }
}