using FileTrans.Core.Abstractions;
using FileTrans.Core.Connection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace FileTrans.Server.Endpoints;

public static class PairingEndpoints
{
    public static void MapPairingEndpoints(
        this WebApplication app,
        PairingService pairingService,
        int port)
    {
        app.MapPost("/pairing/start", () =>
        {
            var session = pairingService.CreatePairingSession(port);
            var qrPng = QrCodeGenerator.GeneratePng(session);

            return Results.Ok(new
            {
                pin = session.Pin,
                qr = Convert.ToBase64String(qrPng)
            });

        });
        app.MapPost("/pairing/confirm", async (HttpRequest request) =>
        {
            var body = await request.ReadFromJsonAsync<ConfirmRequest>();
            if (body is null) return Results.BadRequest();

            var ok = pairingService.TryToConfirm(body.Pin);
            return ok
                ? Results.Ok(new {success = true}) 
                : Results.Json(new { error = "Invalid or expired pin"},
                    statusCode: StatusCodes.Status401Unauthorized);
        });
    }

    private record ConfirmRequest(string Pin);
}