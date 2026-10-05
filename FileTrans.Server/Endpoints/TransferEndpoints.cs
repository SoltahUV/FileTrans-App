using FileTrans.Core.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
namespace FileTrans.Server.Endpoints;

public static class TransferEndpoints
{
    public static void MapTransferEndpoints(
        this WebApplication app,
        IIncomingTransferHandler handler,
        string saveDirectory)
    {
        app.MapGet("/ping", () => Results.Ok("pong"));
        app.MapPost("/text", async (HttpRequest request, CancellationToken ct) =>
        {
            using var reader = new StreamReader(request.Body);
            var text = await reader.ReadToEndAsync(ct);
            
            if (string.IsNullOrEmpty(text))
                return Results.BadRequest("Empty text");
            
            await handler.OnTextReceivedAsync(text, ct);
            return Results.Ok();
        });
        app.MapPost("/file", async (HttpRequest request, CancellationToken ct) =>
        {
            var fileName = Path.GetFileName(request.Query["name"].ToString());
            
            if (string.IsNullOrWhiteSpace(fileName))
                return Results.BadRequest("Missing or invalid 'name'");  
            Directory.CreateDirectory(saveDirectory);
            var path = GetUniquePath(saveDirectory, fileName);
            
            // сразу пишем на диск что бы не убить телефон
            await using (var fs = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                await request.Body.CopyToAsync(fs, ct);
            }
            
            await handler.OnFileReceivedAsync(path, fileName, ct);
            return Results.Ok(new {saved = Path.GetFileName(path) });
        });
    }
    // Если файл есть то делаем "name (1).ext"
    private static string GetUniquePath(string directory, string fileName)
    {
        var path = Path.Combine(directory, fileName);
        if (!File.Exists(path)) return path;

        var name = Path.GetFileNameWithoutExtension(fileName);
        var ext = Path.GetExtension(fileName);

        for (var i = 1; ; i++)
        {
            path = Path.Combine(directory, $"{name}({i}){ext}");
            if(!File.Exists(path)) return path;
        }
    }
}