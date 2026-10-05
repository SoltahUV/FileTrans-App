using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace FileTrans.Core.Connection;

public class UdpDiscovery : IDisposable
{
    public const int DiscoveryPort = 45678;

    private readonly PairingService _pairingService;
    private UdpClient? _udpClient;
    private CancellationTokenSource? _cts;

    public UdpDiscovery(PairingService pairingService)
    {
        _pairingService = pairingService;
    }

    public void StartListening()
    {
        _cts = new CancellationTokenSource();
        _udpClient = new UdpClient(DiscoveryPort);
        _udpClient.EnableBroadcast = true;
        
        Task.Run(() => ListenLoopAsync(_cts.Token));
    }

    public void StopListening()
    {
        _cts?.Cancel();
        _udpClient?.Close();
    }

    private async Task ListenLoopAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                var result = await _udpClient!.ReceiveAsync(ct);
                var message = Encoding.UTF8.GetString(result.Buffer);

                var doc = JsonDocument.Parse(message);
                if (!doc.RootElement.TryGetProperty("pin", out var pinElement))
                    continue;

                var pin = pinElement.GetString();
                var session = _pairingService.GetCurrentPairingSession();

                if (session is not null && session.Pin == pin && !session.IsExpired)
                {
                    var response = JsonSerializer.Serialize(new
                    {
                        ip = session.HostIp,
                        port = session.HttpPort
                    });

                    var responseBytes = Encoding.UTF8.GetBytes(response);
                    await _udpClient.SendAsync(responseBytes, result.RemoteEndPoint, ct);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[UdpDiscovery] Error: {ex.Message}");
            }
        }
    }
    
        
    public void Dispose()
    {
        StopListening();
        _udpClient?.Dispose();
        _cts?.Dispose();
    }
}