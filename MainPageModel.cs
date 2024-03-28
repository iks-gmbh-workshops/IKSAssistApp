using Azure;
using Azure.AI.OpenAI;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.CognitiveServices.Speech;
using System.Collections.ObjectModel;

namespace IKSAssistApp;

internal partial class MainPageModel : ObservableObject
{
    [ObservableProperty] internal ObservableCollection<LanguageVoiceItem> languageVoiceItems;
    [ObservableProperty] internal ObservableCollection<string> moods;

    [ObservableProperty] internal ObservableCollection<OutputMessage> outputMessages = [];

    [ObservableProperty] bool error = false;

    internal MainPageModel()
    {
        LanguageVoiceItems =
        [
            new("Chinese", "zh-Hant", "zh-TW", "zh-TW-HsiaoChenNeural"),
            new("English", "en", "en-US", "en-US-JennyNeural"),
            new("English", "en", "en-US", "en-US-TonyNeural"),
            new("French", "fr", "fr-FR", "fr-FR-BrigitteNeural"),
            new("German", "de", "de-DE", "de-DE-KatjaNeural"),
            new("Italian", "it", "it-IT", "it-IT-IsabellaNeural"),
            new("Spanish", "es", "es-ES", "es-ES-ElviraNeural")
        ];

        Moods =
        [
            "Neutral",
            "Praising",
            "Silly",
            "Bender",
            "Adventure Game",
            "Emergency",
            "Custom"
        ];
    }

    public async Task Speak(LanguageVoiceItem languageVoiceItem, string mood)
    {      
        // Ensure microphone access
        if (!await Util.EnsureMicrophonePermission())
        {
            Error = true;
            return;
        }

        OutputMessages.Add(Util.GetInfoText("Listening..."));

        try
        {
            var statusMessage = Util.EnsureSpeechRecognizer(languageVoiceItem.LanguageLocale, languageVoiceItem.LanguageLocale, languageVoiceItem.VoiceName);

            if (statusMessage is not null)
            {
                OutputMessages.RemoveAt(OutputMessages.Count - 1);
                OutputMessages.Add(statusMessage);
                Error = true;
            }
            else
            {
                var result = await App.SpeechRecognizer.RecognizeOnceAsync();

                // Output results
                switch (result.Reason)
                {
                    case ResultReason.RecognizedSpeech:
                        var recognizedText = result.Text;
                        await Send(recognizedText, languageVoiceItem, mood);
                        break;

                    case ResultReason.NoMatch:
                        OutputMessages.Add(Util.GetNoMatchText());
                        Error = true;
                        break;

                    case ResultReason.Canceled:
                        OutputMessages.Add(Util.GetCanceledText(result));
                        Error = true;
                        break;

                    default:
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            OutputMessages.Add(Util.GetErrorText(ex));
            Error = true;
        }
        finally
        {
        }
    }

    public async Task Send(string text, LanguageVoiceItem languageVoiceItem, string mood)
    {
        OutputMessages.Add(new OutputMessage { Type = OutputMessageType.ChatMessageSelf, Text = text });

        var chatMessage = new ChatMessage { Text = text, Prompt = text };
        var responseText = await Chat(chatMessage, languageVoiceItem, mood);

        OutputMessages.Add(new OutputMessage { Type = OutputMessageType.ChatMessageBot, Text = responseText });

        // Assign a voice style according to selected mood
        var style = mood switch
        {
            "Praising" => "Hopeful",
            "Silly" => "Excited",
            "Bender" => "Unfriendly",
            _ => "Default",
        };

        var statusMessage = Util.EnsureSpeechSynthesizer(languageVoiceItem.LanguageLocale, languageVoiceItem.LanguageLocale, languageVoiceItem.VoiceName);
        if (statusMessage is not null)
        {
            OutputMessages.Add(statusMessage);
            Error = true;
        }
        else
        {
            var outputMessage = await Util.Synthesize(responseText, languageVoiceItem.LanguageLocale, languageVoiceItem.VoiceName, style);
            if (outputMessage is not null)
            {
                OutputMessages.Add(outputMessage);
                Error = true;
            }
        }
    }

    private async Task<string> Chat(ChatMessage chatMessage, LanguageVoiceItem languageVoiceItem, string mood)
    {
        // More roles: https://github.com/f/awesome-chatgpt-prompts/blob/main/prompts.csv
        string variantDefinitionText = mood switch
        {
            "Praising" => "You are a helpful and admiring assistant. Answer all questions very reverently, as if the person asking were a renowned academic celebrity.",
            "Silly" => "You are a helpful assistant who gives answers in a funny way. You are a bit crazy and sometimes overdo it with the answers.",
            "Bender" => "You act like Bender from Futurama, using the tone, manner and vocabulary Bender would use. Do not write any explanations. Only answer like Bender. You must know all of the knowledge of Bender.",
            "Adventure Game" => "You act as a text based adventure game. You explain to me what is happening around me. I will tell you what I want to do and you describe the consequences of my actions.",
            "Emergency" => "You act as a first aid professional that reacts to an emergency situation. I will describe the emergency and will provide advice on how to handle it. You should only reply with your advice, and nothing else. Do not write explanations.",
            "Custom" => App.CustomSystemMessageModel.CustomSystemMessageText,
            _ => "You are a helpful assistant."
        };

        chatMessage.Prompt = chatMessage.Text + $" Write your reply in {languageVoiceItem.LanguageLocale} language.";

        var systemChatMessage = new ChatRequestSystemMessage(variantDefinitionText);
        var newChatMessage = new ChatRequestUserMessage(chatMessage.Prompt);

        App.ChatCompletionsOptions.Messages[0] = systemChatMessage;
        App.ChatCompletionsOptions.Messages.Add(newChatMessage);

        // Submit prompt
        string responseText = await Chat(App.ChatCompletionsOptions);

        var newResponseChatMessage = new ChatRequestAssistantMessage(responseText);
        App.ChatCompletionsOptions.Messages.Add(newResponseChatMessage);

        return responseText;
    }

    private async Task<string> Chat(ChatCompletionsOptions chatCompletionsOptions)
    {
        string responseText = string.Empty;

        var statusMessage = Util.EnsureOpenAIClient(App.SettingsModel.OpenAIServiceEndpoint, App.SettingsModel.OpenAIServiceKey, App.SettingsModel.LlmMaxTokens);

        if (statusMessage is not null)
        {
            OutputMessages.Add(statusMessage);
            Error = true;
        }
        else
        {
            try
            {
                // Get chatbot response
                var completionsResponse = await App.OpenAIClient.GetChatCompletionsAsync(chatCompletionsOptions);
                responseText = completionsResponse.Value.Choices[0].Message.Content;
            }
            catch (RequestFailedException ex)
            {
                OutputMessages.Add(Util.GetErrorText(ex));
                Error = true;
            }
        }

        return responseText;
    }
}
