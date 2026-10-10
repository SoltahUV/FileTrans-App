using Net.Codecrete.QrCodeGenerator;

namespace FileTrans.Core.Connection;

public static class QrCodeGenerator
{
    
    /// <summary>
    /// pattern inside: "filetrans://192.168.1.1:5000?pin=123456"
    /// </summary>

    public static byte[] GeneratePng(string ip, int port, string pin, int scale = 10, int border = 4)
    {
        var payload = $"filetrans://{ip}:{port}?pin={pin}";
        var qr = QrCode.EncodeText(payload, QrCode.Ecc.Medium);
        return qr.ToPngBitmap(border, scale);
    }
    public static (string ip, int port, string pin)? ParseQrPayload(string payload)
    {
        if (!Uri.TryCreate(payload, UriKind.Absolute, out var uri))
            return null;
        if (uri.Scheme != "filetrans")
            return null;
        
        var pin = System.Web.HttpUtility.ParseQueryString(uri.Query)["pin"];
        if (pin is null) return null;

        return (uri.Host, uri.Port, pin);
    }
    
    
}