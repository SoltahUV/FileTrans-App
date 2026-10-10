using FileTrans.Core.Connection;
using FileTrans.Core.Contracts;
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
            var existing = pairingService.GetCurrentPairingSession();
            if (existing is { Status: PairingStatus.Pending })
                return Results.Conflict(new { error = "Pairing already in progress" });

            var session = pairingService.CreatePairingSession(port);
            var qrPng = QrCodeGenerator.GeneratePng(session.HostIp, session.HttpPort, session.Pin);
            return Results.Created("/pairing/session", new PairingStartResponse(
                session.Pin,
                Convert.ToBase64String(qrPng),
                session.HostIp,
                session.HttpPort
            ));
        });
        app.MapPost("/pairing/confirm", async (HttpRequest request) =>
        {
            var body = await request.ReadFromJsonAsync<ConfirmRequest>();
            if (body is null) return Results.BadRequest();

            if (!pairingService.TryToConfirm(body.Pin, out var token))
                return Results.Json(new { error = "Invalid or expired pin" }, 
                    statusCode: StatusCodes.Status401Unauthorized);

            return Results.Ok(new { token });
        });
    }
    
}