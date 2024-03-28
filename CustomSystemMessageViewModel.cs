using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace IKSAssistApp;

internal partial class CustomSystemMessageViewModel : ObservableObject
{
    [ObservableProperty] private CustomSystemMessageModel model;

    public CustomSystemMessageViewModel()
    {
        Model = App.CustomSystemMessageModel;
    }

    [RelayCommand]
    public void Save(string property)
    {
        Model.Save(property);
    }
}