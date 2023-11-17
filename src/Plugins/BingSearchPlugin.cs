using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using System.Linq;
using Microsoft.BotBuilderSamples;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

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
    public async Task<string> BingNewsSearch([Description("検索に使用するクエリ")] string query)
    {
        string url = $"https://api.bing.microsoft.com/v7.0/news/search?q={Uri.EscapeDataString(query)}";

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
}

public class BingSearchResult
{
    public WebPages WebPages { get; set; }
}

public class WebPages
{
    public List<Value> Value { get; set; }
}

public class Value
{
    public string Name { get; set; }
}