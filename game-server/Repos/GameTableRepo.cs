using game_server.Models;
using game_server.Models.Configrations;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace game_server.Repos;

public class GameTableRepo
{
    private readonly IMongoCollection<GameTable> gameTables;

    public GameTableRepo(IOptions<MongoDatabase> options)
    {
        var client = new MongoClient(options.Value.ConnectionString);
        var database = client.GetDatabase(options.Value.DatabaseName);
        gameTables = database.GetCollection<GameTable>(options.Value.Collection);
    }

    public async Task<GameTable> Get(string id) => await gameTables.Find(gt => gt.Id == id).FirstOrDefaultAsync();

    public async Task Add(GameTable gameTable) => await gameTables.InsertOneAsync(gameTable);

    public async Task Update(GameTable gameTable) =>
        await gameTables.ReplaceOneAsync(gt => gt.Id == gameTable.Id, gameTable);
}
