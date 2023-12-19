namespace game_server.Models.Configrations;

public class MongoDatabase
{
    public required string ConnectionString { get; set; }
    public required string DatabaseName { get; set; }
    public required string Collection { get; set; }
}
