using CommunityToolkit.Mvvm.ComponentModel;

namespace IKSAssistApp;

internal partial class OutputMessage : ObservableObject
{
    [ObservableProperty] private OutputMessageType type;
    [ObservableProperty] private string text;
}
