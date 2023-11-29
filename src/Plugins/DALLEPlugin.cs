using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.BotBuilderSamples;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Azure.AI.OpenAI;
using System.Collections.Generic;

namespace Plugins;
public class DALLEPlugin
{
    private readonly OpenAIClient client;
    private ITurnContext<IMessageActivity> _turnContext;

    public DALLEPlugin(IConfiguration config, ConversationData conversationData, ITurnContext<IMessageActivity> turnContext)
    {
        var _aoaiApiKey = config.GetValue<string>("DALLE_AOAI_API_KEY");
        var _aoaiApiEndpoint = config.GetValue<string>("DALLE_AOAI_API_ENDPOINT");
        client = new(new Uri(_aoaiApiEndpoint), new AzureKeyCredential(_aoaiApiKey));
        _turnContext = turnContext;
    }



    [SKFunction, Description("説明から画像を生成します。")]
    public async Task<string> GenerateImages(
        [Description("生成する画像の説明。")] string prompt,
        [Description("生成するイメージの数。指定しない場合は、1を使用する必要があります")] int n
    )
    {
        await _turnContext.SendActivityAsync($"次の説明による {n} 個の画像を生成しています \"{prompt}\"...");
        Response<ImageGenerations> imageGenerations = await client.GetImageGenerationsAsync(
            new ImageGenerationOptions()
            {
                Prompt = prompt,
                Size = ImageSize.Size512x512,
                ImageCount = n
            });

        List<object> images = new();
        images.Add(
            new {
                type="TextBlock",
                text="こちらが生成された画像です。",
                size="large"
            }
        );
        foreach (ImageLocation img in imageGenerations.Value.Data)
            images.Add(new { type = "Image", url = img.Url.AbsoluteUri });
        object adaptiveCardJson = new
        {
            type = "AdaptiveCard",
            version = "1.0",
            body = images
        };

        var adaptiveCardAttachment = new Microsoft.Bot.Schema.Attachment()
        {
            ContentType = "application/vnd.microsoft.card.adaptive",
            Content = adaptiveCardJson,
        };
        await _turnContext.SendActivityAsync(MessageFactory.Attachment(adaptiveCardAttachment));
        return "画像は正常に生成され、既にユーザーに送信されています。";
    }

}