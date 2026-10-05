using FileTrans.Core.Connection;
using FileTrans.Server;
using FileTrans.ServerTest;

var handler = new TestHandler();
var saveDir = Path.Combine(AppContext.BaseDirectory, "received");

await using var server = new ServerHost(handler, saveDir);
await server.StartAsync(port: 5000);

Console.WriteLine($"Сервер запущен на порту {server.Port}");
Console.WriteLine("UDP дискавери слушает порт 45678");
Console.WriteLine();

// Сразу создаём сессию — в реальном приложении это будет по кнопке
var session = server.PairingService!.CreatePairingSession(server.Port);
server.PairingService.OnPairingConfirmed += s =>
{
    Console.WriteLine($"[PAIRING] Устройство подключилось! IP: {s.HostIp}");
};
// после CreateSession:

var qrBytes = QrCodeGenerator.GeneratePng(session);
var qrPath = Path.Combine(AppContext.BaseDirectory, "qr.png");
await File.WriteAllBytesAsync(qrPath, qrBytes);
Console.WriteLine($"QR сохранён: {qrPath}");

Console.WriteLine($"=== PIN: {session.Pin} ===");
Console.WriteLine($"QR payload: filetrans://{session.HostIp}:{session.HttpPort}?pin={session.Pin}");
Console.WriteLine();
Console.WriteLine("Жми Enter чтобы остановить.");
Console.ReadLine();

await server.StopAsync();