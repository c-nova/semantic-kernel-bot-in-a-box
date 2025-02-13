using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Azure.Search.Documents;
using Azure;
using Azure.Search.Documents.Models;
using Microsoft.Extensions.Configuration;
using Model;
using Microsoft.SemanticKernel;
using System.Linq;
using Microsoft.BotBuilderSamples;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace Plugins;

public class SearchPlugin
{
    private readonly SearchClient searchClient;
    private ITurnContext<IMessageActivity> _turnContext;

    public SearchPlugin(IConfiguration config, ConversationData conversationData, ITurnContext<IMessageActivity> turnContext) {
        var _searchApiKey = config.GetValue<string>("SEARCH_API_KEY");
        var _searchApiEndpoint = config.GetValue<string>("SEARCH_API_ENDPOINT");
        var _searchIndex = config.GetValue<string>("SEARCH_INDEX");
        searchClient = new SearchClient (new Uri(_searchApiEndpoint), _searchIndex, new AzureKeyCredential(_searchApiKey));
        _turnContext = turnContext;
    }

    

    [SKFunction, Description("ホテルの説明から検索できます。")]
    public async Task<string> FindHotels(
        [Description("検索に使用する説明")] string query
    )
    {
        await _turnContext.SendActivityAsync($"この説明からホテルを検索 \"{query}\"...");
        var options = new SearchOptions();
        options.Select.Add("HotelName");
        options.Select.Add("Description");
        options.Size = 3;
        var response = await searchClient.SearchAsync<Hotel>(searchText: query, options);
        var textResults = "[HOTEL RESULTS]\n\n";
        var searchResults = response.Value.GetResults();
        if (searchResults.Count() == 0)
            return "ホテルが見つかりませんでした";
        foreach (SearchResult<Hotel> result in searchResults)
        {
            textResults += $"名前: {result.Document.HotelName}\n\n";
            textResults += $"説明: {result.Document.Description}\n*****\n\n";
        }
        return textResults;
    }

}