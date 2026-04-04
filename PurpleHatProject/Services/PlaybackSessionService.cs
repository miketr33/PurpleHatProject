using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

namespace PurpleHatProject.Services;

public interface IPlaybackSessionService
{
    Task SaveAsync(int userId, string audioUrl, double position, int volume);
    Task<PlaybackSession?> LoadAsync(int userId);
}

public record PlaybackSession(string AudioUrl, double Position, int Volume);

public class PlaybackSessionService(IAmazonDynamoDB dynamoDb) : IPlaybackSessionService
{
    private const string TableName = "PlaybackSessions";

    public async Task SaveAsync(int userId, string audioUrl, double position, int volume)
    {
        await EnsureTableExists();

        await dynamoDb.PutItemAsync(new PutItemRequest
        {
            TableName = TableName,
            Item = new Dictionary<string, AttributeValue>
            {
                ["UserId"] = new(userId.ToString()),
                ["AudioUrl"] = new(audioUrl),
                ["Position"] = new AttributeValue { N = position.ToString("F2") },
                ["Volume"] = new AttributeValue { N = volume.ToString() }
            }
        });
    }

    public async Task<PlaybackSession?> LoadAsync(int userId)
    {
        await EnsureTableExists();

        var response = await dynamoDb.GetItemAsync(new GetItemRequest
        {
            TableName = TableName,
            Key = new Dictionary<string, AttributeValue>
            {
                ["UserId"] = new(userId.ToString())
            }
        });

        if (!response.IsItemSet)
            return null;

        var item = response.Item;
        return new PlaybackSession(
            item["AudioUrl"].S,
            double.Parse(item["Position"].N),
            int.Parse(item["Volume"].N)
        );
    }

    private async Task EnsureTableExists()
    {
        var tables = await dynamoDb.ListTablesAsync();
        if (tables.TableNames.Contains(TableName))
            return;

        await dynamoDb.CreateTableAsync(new CreateTableRequest
        {
            TableName = TableName,
            KeySchema = [new KeySchemaElement("UserId", KeyType.HASH)],
            AttributeDefinitions = [new AttributeDefinition("UserId", ScalarAttributeType.S)],
            BillingMode = BillingMode.PAY_PER_REQUEST
        });
    }
}
