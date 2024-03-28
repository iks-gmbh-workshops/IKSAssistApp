using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace IKSAssistApp;

internal partial class MainPageViewModel : ObservableObject
{
    [ObservableProperty] private MainPageModel model;

    #region Model-independent fields

    // Selectable items with default values
    [ObservableProperty] private LanguageVoiceItem selectedLanguageVoiceItem = new("English", "en", "en-US", "en-US-JennyNeural");
    [ObservableProperty] private string selectedMood = "Neutral";

    // Free-text entry
    [ObservableProperty] private string inputText = string.Empty;

    // Flag for readiness state
    [ObservableProperty] private bool isReady = true;
    [ObservableProperty] private bool isBusy = false;

    #endregion

    #region Model-dependent fields

    // Text output
    [ObservableProperty] private ObservableCollection<OutputMessage> outputMessages = [];

    #endregion

    public MainPageViewModel()
    {
        if (App.ErrorMessage is not null)
        {
            IsReady = false;
            OutputMessages.Add(App.ErrorMessage);
        }
        else
        {
            Model = new MainPageModel();
            OutputMessages = Model.OutputMessages;

            Load();
        }
    }

    private void Load()
    {
        var languageLocale = Util.LoadTextPropertySync("assist_language_locale", false);
        var voiceName = Util.LoadTextPropertySync("assist_voice_name", false);
        if (languageLocale == string.Empty || voiceName == string.Empty)
        {
            languageLocale = SelectedLanguageVoiceItem.LanguageLocale;
            voiceName = SelectedLanguageVoiceItem.VoiceName;
        }
        SelectedLanguageVoiceItem = Model.LanguageVoiceItems.ToList().Find(x => x.LanguageLocale == languageLocale && x.VoiceName == voiceName);

        var mood = Util.LoadTextPropertySync("assist_mood", false);
        if (mood == string.Empty)
        {
            mood = SelectedMood;
        }
        SelectedMood = Model.Moods.ToList().Find(x => x == mood);
    }

    [RelayCommand]
    public void Save(string property)
    {
        switch (property)
        {
            case "LanguageVoiceItem":
                Util.SaveTextProperty("assist_language_locale", SelectedLanguageVoiceItem.LanguageLocale, false);
                Util.SaveTextProperty("assist_voice_name", SelectedLanguageVoiceItem.VoiceName, false);
                break;
            case "Mood":
                Util.SaveTextProperty("assist_mood", SelectedMood, false);
                break;
            default:
                throw new ArgumentException($"Property '{property}' unknown.");
        }
    }

    [RelayCommand]
    public async Task Speak()
    {
        IsReady = false;
        IsBusy = true;

        await Model.Speak(SelectedLanguageVoiceItem, SelectedMood);
        if (Model.Error) { Model.Error = false; } else { InputText = string.Empty; }

        IsReady = true;
        IsBusy = false;
    }

    [RelayCommand]
    public async Task Send()
    {
        IsReady = false;
        IsBusy = true;

        await Model.Send(InputText, SelectedLanguageVoiceItem, SelectedMood);
        if (Model.Error) { Model.Error = false; } else { InputText = string.Empty; }

        IsReady = true;
        IsBusy = false;
    }
}