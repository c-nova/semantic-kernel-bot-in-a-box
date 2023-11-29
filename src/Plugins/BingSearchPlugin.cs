using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using System.Linq;
using Microsoft.BotBuilderSamples;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Model;

namespace Plugins;

public class BingSearchPlugin
{
    private readonly HttpClient _client;
    private readonly ITurnContext<IMessageActivity> _turnContext;

    public BingSearchPlugin(IConfiguration config, ConversationData conversationData, ITurnContext<IMessageActivity> turnContext)
    {
        var bingSearchApiKey = config.GetValue<string>("BING_SEARCH_API_KEY");
        _client = new HttpClient();
        _client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", bingSearchApiKey);
        _turnContext = turnContext;
    }

    [SKFunction, Description("Bingでインターネット上の情報を検索します。")]
    public async Task<string> BingSearch([Description("検索に使用するクエリ")] string query)
    {
        string url = $"https://api.bing.microsoft.com/v7.0/search?q={Uri.EscapeDataString(query)}";

        var response = await _client.GetAsync(url);
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            // JSONを解析して必要な情報を取り出す
            var searchResult = JsonConvert.DeserializeObject<BingSearchResult>(content);
            // 必要な情報を文字列に変換して返す
            return string.Join(", ", searchResult.WebPages.Value.Select(page => page.Name));
        }
        else
        {
            return $"Error: {response.StatusCode}";
        }
    }

    [SKFunction, Description("Bingでインターネット上のニュースを検索します。")]
    public async Task<string> BingNewsSearch(
        [Description("検索に使用するクエリ")] string query,
        [Description("表示する記事の数。指定しない場合は、3 を使用する必要があります")] int n,
        [Description("記事の言語。指定しない場合は、ja-JP を使用する必要があります")] string mkt,
        [Description("記事のカテゴリー。指定しない場合は、Japan を使用する必要があります。カテゴリーは、Business, Entertainment, Japan, LifeStyle, Politics, ScienceAndTechnology, Sports, World のいずれかである必要がある。")] string category
        )
    {
        string url = $"https://api.bing.microsoft.com/v7.0/news/search?count={n}&mkt={mkt}&category={category}&q={Uri.EscapeDataString(query)}";

        var response = await _client.GetAsync(url);
        if (response.IsSuccessStatusCode)
        {
            
            var content = await response.Content.ReadAsStringAsync();
            // JSONを解析して必要な情報を取り出す
            var responseDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(content);
            var articles = responseDict["value"] as Newtonsoft.Json.Linq.JToken;
            var result = new StringBuilder();

            foreach (Newtonsoft.Json.Linq.JToken article in articles)
            {
                result.AppendLine("タイトル: " + article["name"]);
                result.AppendLine("記事への URL: " + article["url"]);
                result.AppendLine("詳細: " + article["description"]);
                result.AppendLine("発行元: " + GetPublisherString(article["provider"]));

                var image = article["image"];
                if (image != null)
                {
                    result.AppendLine("Thumbnail: " + image["thumbnail"]["contentUrl"]);
                    result.AppendLine($"Thumbnail size: {image["thumbnail"]["width"]} (w) x {image["thumbnail"]["height"]} (h)");
                }

                var video = article["video"];
                if (video != null && video["motionThumbnailUrl"] != null)
                {
                    result.AppendLine("Title: " + video["name"]);
                    result.AppendLine("Motion thumbnail: " + video["motionThumbnailUrl"]);
                    result.AppendLine($"Motion thumbnail size: {video["thumbnail"]["width"]} (w) x {video["thumbnail"]["height"]} (h)");
                }

                result.AppendLine();
            }

            return result.ToString();
        }
        else
        {
            return $"Error: {response.StatusCode}";
        }
    }

    private string GetPublisherString(Newtonsoft.Json.Linq.JToken publishers)
    {
        var publisherString = new StringBuilder();
        var isFirst = true;

        foreach (Newtonsoft.Json.Linq.JToken publisher in publishers)
        {
            if (!isFirst)
            {
                publisherString.Append(" | ");
            }

            publisherString.Append(publisher["name"]);
            isFirst = false;
        }

        return publisherString.ToString();
    }
}
