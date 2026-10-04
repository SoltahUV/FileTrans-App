using FileTrans.Server;
using FileTrans.ServerTest;

var handler = new TestHandler();
var saveDir = Path.Combine(AppContext.BaseDirectory, "received");

await using var server = new ServerHost(handler, saveDir);
await server.StartAsync(port: 5000);

Console.WriteLine($"Сервер запущен на порту {server.Port}. Жми Enter чтобы остановить.");
Console.ReadLine();

await server.StopAsync();