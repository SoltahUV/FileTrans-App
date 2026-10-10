namespace FileTrans.Core.Contracts;

public record ConfirmRequest(string Pin);
public record PairingStartResponse(string Pin, string Qr, string Ip, int Port);