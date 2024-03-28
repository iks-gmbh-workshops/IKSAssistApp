using CommunityToolkit.Mvvm.ComponentModel;

namespace IKSAssistApp;

internal partial class CustomSystemMessageModel : ObservableObject
{

    [ObservableProperty] internal string customSystemMessageText;

    internal CustomSystemMessageModel()
    {
        Load();
    }

    public void Load()
    {
        CustomSystemMessageText = Util.LoadTextPropertySync("custom_system_message_text", false);
    }

    public void Save(string property)
    {
        switch (property)
        {
            case "CustomSystemMessageText":
                Util.SaveTextProperty("custom_system_message_text", CustomSystemMessageText, false);
                break;
            default:
                throw new ArgumentException($"Property '{property}' unknown.");
        }
    }
}
