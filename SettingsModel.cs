using CommunityToolkit.Mvvm.ComponentModel;

namespace IKSAssistApp;

internal partial class SettingsModel : ObservableObject
{
    [ObservableProperty] internal string speechSubscriptionKey;
    [ObservableProperty] internal string speechServiceRegion;
    [ObservableProperty] internal int? initialSilenceTimeoutMs;
    [ObservableProperty] internal int? endSilenceTimeoutMs;
    [ObservableProperty] internal string openAIServiceKey;
    [ObservableProperty] internal string openAIServiceEndpoint;
    [ObservableProperty] internal string llmDeploymentName;
    [ObservableProperty] internal int? llmMaxTokens;

    internal SettingsModel()
    {
        Load();
    }

    public void Load()
    {
        SpeechSubscriptionKey = Util.LoadTextPropertySync("speech_subscription_key", true);
        SpeechServiceRegion = Util.LoadTextPropertySync("speech_service_region", true);
        InitialSilenceTimeoutMs = Util.GetInt(Util.LoadTextPropertySync("initial_silence_timeout_ms", true));
        EndSilenceTimeoutMs = Util.GetInt(Util.LoadTextPropertySync("end_silence_timeout_ms", true));
        OpenAIServiceKey = Util.LoadTextPropertySync("openai_service_key", true);
        OpenAIServiceEndpoint = Util.LoadTextPropertySync("openai_service_endpoint", true);
        LlmDeploymentName = Util.LoadTextPropertySync("llm_deployment_name", true);
        LlmMaxTokens = Util.GetInt(Util.LoadTextPropertySync("llm_max_tokens", true));

        if (SpeechSubscriptionKey is null
            || SpeechServiceRegion is null
            || InitialSilenceTimeoutMs is null
            || EndSilenceTimeoutMs is null
            || OpenAIServiceKey is null
            || OpenAIServiceEndpoint is null
            || LlmDeploymentName is null
            || LlmMaxTokens is null)
        {
            App.ErrorMessage = Util.GetMissingSettingsText();
        }
    }

    public void Save(string property)
    {
        switch (property)
        {
            case "SpeechSubscriptionKey":
                Util.SaveTextProperty("speech_subscription_key", SpeechSubscriptionKey, true);
                break;
            case "SpeechServiceRegion":
                Util.SaveTextProperty("speech_service_region", SpeechServiceRegion, true);
                break;
            case "InitialSilenceTimeoutMs":
                Util.SaveTextProperty("initial_silence_timeout_ms", InitialSilenceTimeoutMs.ToString(), true);
                break;
            case "EndSilenceTimeoutMs":
                Util.SaveTextProperty("end_silence_timeout_ms", EndSilenceTimeoutMs.ToString(), true);
                break;
            case "OpenAIServiceKey":
                Util.SaveTextProperty("openai_service_key", OpenAIServiceKey, true);
                break;
            case "OpenAIServiceEndpoint":
                Util.SaveTextProperty("openai_service_endpoint", OpenAIServiceEndpoint, true);
                break;
            case "LlmDeploymentName":
                Util.SaveTextProperty("llm_deployment_name", LlmDeploymentName, true);
                break;
            case "LlmMaxTokens":
                Util.SaveTextProperty("llm_max_tokens", LlmMaxTokens.ToString(), true);
                break;
            default:
                throw new ArgumentException($"Property '{property}' unknown.");
        }
    }
}
