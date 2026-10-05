using FileTrans.Core.Abstractions;
using FileTrans.Core.Connection;
using FileTrans.Server.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace FileTrans.Server;

public class ServerHost : IAsyncDisposable
{
    private readonly IIncomingTransferHandler _handler;
    private readonly string _saveDirectory;
    private WebApplication? _app;
    private UdpDiscovery? _udpDiscovery;

    public int Port { get; private set; }
    public bool IsRunning => _app is not null;
    
    public PairingService? PairingService { get; private set; }

    public ServerHost(IIncomingTransferHandler handler, string saveDirectory)
    {
        _handler = handler;
        _saveDirectory = saveDirectory;
    }

    public async Task StartAsync(int port = 5000, CancellationToken ct = default)
    {
        if(_app is not null)
            throw new InvalidOperationException("Server is already running");

        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            ContentRootPath = AppContext.BaseDirectory 
        });

        builder.WebHost.ConfigureKestrel(options =>
        {
            options.ListenAnyIP(port);

            options.Limits.MaxRequestBodySize = null;
        });

        builder.Services.AddSingleton<PairingService>();
        
        var app = builder.Build();
        
        PairingService = app.Services.GetRequiredService<PairingService>();

        _udpDiscovery = new UdpDiscovery(PairingService);
        _udpDiscovery.StartListening();
        
        app.MapTransferEndpoints(_handler, _saveDirectory);
        app.MapPairingEndpoints(PairingService, Port);
        try
        {
            await app.StartAsync(ct);
        }
        catch
        {
            await app.DisposeAsync();
            throw;
        }

        _app = app;
        Port = port;
    }

    public async Task StopAsync()
    {
        if(_app is null) return;

        var app = _app;
        _app = null;

        await app.StopAsync();
        await app.DisposeAsync();
    }

    public async ValueTask DisposeAsync() => await StopAsync();
} 