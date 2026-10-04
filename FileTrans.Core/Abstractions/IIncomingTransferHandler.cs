namespace FileTrans.Core.Abstractions;

public interface IIncomingTransferHandler
{
    Task OnTextReceivedAsync(string text, CancellationToken ct);
    Task OnFileReceivedAsync(string savedPath, string originalName, CancellationToken ct);
}