using System.Collections.Generic;
using Network.FileManagement;
using Network.Utilities;
using Newtonsoft.Json;

namespace Network.ChatSafety;

public class ChatGuardian
{
    private FileHandler _fileHandler;
    private string path = "ForbiddenWords.txt";
    private List<string> forbiddenWords = new List<string>();

    public ChatGuardian()
    {
        _fileHandler = new FileHandler(path);
        forbiddenWords = TryGetWords();
        Logger.Log($"ForbiddenWordsCount: {forbiddenWords.Count}");
    }

    public bool IsSafeMessage(string message)
    {
        foreach (var word in forbiddenWords)
        {
            if (message.Contains(word))
                return false;
        }

        return true;
    }


    private List<string> TryGetWords()
    {
        List<string> forbidden = JsonConvert.DeserializeObject<List<string>>(_fileHandler.ReadFile());
        return forbidden;
    }
}