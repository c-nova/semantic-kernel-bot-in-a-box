using System;
using System.ComponentModel;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.BotBuilderSamples;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using System.Threading.Tasks;

namespace Plugins;

public class SQLPlugin
{
    private readonly string connectionString;
    private ITurnContext<IMessageActivity> _turnContext;
    public SQLPlugin(IConfiguration config, ConversationData conversationData, ITurnContext<IMessageActivity> turnContext) 
    {
        connectionString = config.GetValue<string>("SQL_CONNECTION_STRING");
        _turnContext = turnContext;
    }




    [SKFunction, Description("顧客データと売上データを含む AdventureWorksLT のテーブル名を取得します。ユーザーが正しい名前を述べたと想定するのではなく、他のクエリを実行する前に必ずこれを実行してください。販売員情報は Customer テーブルに含まれていることを忘れないでください。")]
    public async Task<string> GetTables() {
        await _turnContext.SendActivityAsync($"テーブルを取得中...");
        return QueryAsCSV($"SELECT TABLE_SCHEMA, TABLE_NAME FROM INFORMATION_SCHEMA.TABLES;");
    }



    [SKFunction, Description("AdventureWorksLT のテーブルのデータベース スキーマを取得します。")]
    public async Task<string> GetSchema(
        [Description("スキーマを取得するテーブル。スキーマ名は含めないでください。")] string tableName
    ) 
    {
        await _turnContext.SendActivityAsync($"テーブルのスキーマの取得 \"{tableName}\"...");
        return QueryAsCSV($"SELECT TABLE_SCHEMA, TABLE_NAME, COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{tableName}';");
    }



    [SKFunction, Description("AdventureWorksLT データベースに対して SQL を実行する")]
    public async Task<string> RunQuery(
        [Description("SQL Server で実行するクエリ。テーブルを参照するときは、必ずスキーマ名を追加してください。")] string query
    )
    {
        await _turnContext.SendActivityAsync($"実行中のクエリ...");
        return QueryAsCSV(query);
    }




    private string QueryAsCSV(string query) 
    {
        var output = "[DATABASE RESULTS] \n";
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            SqlCommand command = new SqlCommand(query, connection);
            connection.Open();
            SqlDataReader reader = command.ExecuteReader();
            try
            {
                for (int i = 0; i < reader.FieldCount; i++) {
                    output += reader.GetName(i);
                    if (i < reader.FieldCount - 1) 
                        output += ",";
                }
                output += "\n";
                while (reader.Read())
                {
                    for (int i = 0; i < reader.FieldCount; i++) {
                        var columnName = reader.GetName(i);
                        output += reader[columnName].ToString();
                        if (i < reader.FieldCount - 1) 
                            output += ",";
                    }
                    output += "\n";
                }
            } catch (Exception e) {
                Console.WriteLine(e);
            }
            finally
            {
                reader.Close();
            }
        }
        return output;
    }

}