using System.Net;
using System.Net.Sockets;

namespace FileTrans.Core.Connection;

public class PairingService
{

    private PairingSession? _pairingSession;
    public event Action<PairingSession>? OnPairingConfirmed;

    
    /// <summary>
    /// Create new session: generate pin, get local Ip
    /// </summary>
    public PairingSession CreatePairingSession(int httpPort)
    {
        _pairingSession = new PairingSession()
        {
            Pin = GeneratePin(),
            HostIp = GetLocalIp(),
            HttpPort = httpPort
        };
        
        return _pairingSession;
    }

    
    /// <summary>
    /// Check pin from client 
    /// </summary>
    public bool TryToConfirm(string pin)
    {
        if (_pairingSession is null) return false;

        if (_pairingSession.IsExpired)
        {
            _pairingSession.Status = PairingStatus.Expired;
            return false;
        }

        if (_pairingSession.Pin != pin) {
            _pairingSession.Status = PairingStatus.Rejected;
            return false; 
        }

        _pairingSession.Status = PairingStatus.Confirmed;
        OnPairingConfirmed?.Invoke(_pairingSession);
        return true;

    }
    
    public PairingSession? GetCurrentPairingSession() => _pairingSession;

    public void InvalidatePairingSession() 
    {
        _pairingSession = null;
    }
    
    private static string GetLocalIp()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        var ip = host.AddressList
            .FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork 
                                 && !IPAddress.IsLoopback(a));
        
        return ip?.ToString() ?? "127.0.0.1";
    }
    
    private static string GeneratePin()
    {
        var bytes = new byte[4];
        System.Security.Cryptography.RandomNumberGenerator.Fill(bytes);
        var number = BitConverter.ToUInt32(bytes, 0) % 1_000_000;
        return number.ToString("D6");
    }
}