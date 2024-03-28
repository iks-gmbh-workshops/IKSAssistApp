using Azure;
using Azure.AI.OpenAI;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
namespace IKSAssistApp;

public partial class App : Application
{
    internal static ProfanityOption? Profanity = ProfanityOption.Raw; // Disable profanity filter

    internal static SettingsModel SettingsModel;
    internal static CustomSystemMessageModel CustomSystemMessageModel;

    internal static OutputMessage ErrorMessage;

    internal static SpeechConfig SpeechConfig;
    internal static SpeechRecognizer SpeechRecognizer;
    internal static SpeechSynthesizer SpeechSynthesizer;

    internal static AudioConfig AudioConfig;

    internal static OpenAIClient OpenAIClient;
    internal static ChatCompletionsOptions ChatCompletionsOptions;

    public App()
    {
        InitializeComponent();
        MainPage = new AppShell();

        SettingsModel = new SettingsModel();
        CustomSystemMessageModel = new CustomSystemMessageModel();

        if (ErrorMessage is null)
        {
            try
            {
                SpeechConfig = SpeechConfig.FromSubscription(SettingsModel.SpeechSubscriptionKey, SettingsModel.SpeechServiceRegion);
                SpeechConfig.SetProperty(PropertyId.SpeechServiceConnection_InitialSilenceTimeoutMs, SettingsModel.InitialSilenceTimeoutMs.ToString());
                SpeechConfig.SetProperty(PropertyId.SpeechServiceConnection_EndSilenceTimeoutMs, SettingsModel.EndSilenceTimeoutMs.ToString());
                SpeechConfig.SetProfanity(ProfanityOption.Raw);

                AudioConfig = AudioConfig.FromDefaultMicrophoneInput();

                SpeechRecognizer = new SpeechRecognizer(SpeechConfig, AudioConfig);
                SpeechSynthesizer = new SpeechSynthesizer(SpeechConfig);

                // OpenAIClient
                OpenAIClient = new OpenAIClient(new Uri(SettingsModel.OpenAIServiceEndpoint), new AzureKeyCredential(SettingsModel.OpenAIServiceKey));

                // ChatCompletionsOptions
                ChatCompletionsOptions = new ChatCompletionsOptions
                {
                    DeploymentName = SettingsModel.LlmDeploymentName
                };

                if (SettingsModel.LlmMaxTokens is not null)
                {
                    ChatCompletionsOptions.MaxTokens = SettingsModel.LlmMaxTokens;
                }

                ChatCompletionsOptions.Messages.Add(null);
            }
            catch (Exception ex)
            {
                ErrorMessage = Util.GetErrorText(ex);
            }
        }
    }

    protected override Window CreateWindow(IActivationState activationState)
    {
        Window window = base.CreateWindow(activationState);

        window.Destroying += (s, e) =>
        {
            try
            {
                AudioConfig?.Dispose();

                if (SpeechRecognizer is not null)
                {
                    SpeechRecognizer.StopContinuousRecognitionAsync();
                    Task.Delay(1000);
                    Connection.FromRecognizer(SpeechRecognizer).Close();
                    SpeechRecognizer.Dispose();
                }
            }
            catch (Exception) { }
        };

        return window;
    }
}