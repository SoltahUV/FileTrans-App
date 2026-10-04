using System.Runtime.InteropServices.JavaScript;

namespace FileTrans.Core.Connection;


public enum PairingStatus
{
    Pending,
    Confirmed,
    Expired,
    Rejected
}
public class PairingSession
{
    public string Pin { get; init; } = string.Empty;
    public string HostIp { get; init;} = string.Empty;
    public int HttpPort { get; init; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public PairingStatus Status { get; set; } = PairingStatus.Pending;
    
    public bool IsExpired => (DateTime.UtcNow - CreatedAt).TotalMinutes > 5;

}