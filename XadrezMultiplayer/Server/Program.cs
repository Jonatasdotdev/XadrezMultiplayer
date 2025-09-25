using Server;

var server = new GameServer("127.0.0.1", 8080);
await server.StartAsync();