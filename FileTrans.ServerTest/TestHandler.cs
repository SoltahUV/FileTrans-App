using FileTrans.Core.Abstractions;

namespace FileTrans.ServerTest;

public class TestHandler : IIncomingTransferHandler
{
    public Task OnTextReceivedAsync(string text, CancellationToken ct)
    {
        Console.WriteLine($"[TEXT] {text}");
        return Task.CompletedTask;
    }

    public Task OnFileReceivedAsync(string savedPath, string originalName, CancellationToken ct)
    {
        Console.WriteLine($"[FILE] {originalName} -> {savedPath}");
        return Task.CompletedTask;
    }
}