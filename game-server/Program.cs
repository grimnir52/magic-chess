using System.Net;
using game_server.Hubs;
using game_server.Models.Configrations;
using game_server.Repos;
using game_server.Services;
using game_server.Tcp;

var builder = WebApplication.CreateBuilder(args);

var config = builder.Configuration;
var dbConfig = config.GetSection("MongoDatabase");

builder.Services.AddSingleton<Client>();
builder.Services.AddSingleton<PacketHandler>();
builder.Services.AddSingleton<PacketRouter>();
builder.Services.Configure<MongoDatabase>(dbConfig);
builder.Services.AddSingleton<GameTableRepo>();
builder.Services.AddSingleton<GameService>();
builder.Services.AddSignalR();
builder.Services.AddCors(options =>
{
    options.AddPolicy("CORSPolicy", builder => builder
    .AllowAnyMethod()
    .AllowAnyHeader()
    .AllowCredentials()
    .SetIsOriginAllowed((hosts) => true));
});

var app = builder.Build();

app.UseCors("CORSPolicy");
app.UseHttpsRedirection();
app.UseCors(builder => builder.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());
app.MapHub<GameHub>("/chess");

var client = app.Services.GetRequiredService<Client>();
_ = Task.Run(async () => await client.HandleConnect(IPAddress.Loopback, 13));

app.Run();
