using System.Net.WebSockets;

namespace BtcEurMarketDepth.Infrastructure.Transport.WebSockets;

public sealed class ClientWebSocketAdapter : IWebSocketClient
{
    private readonly ClientWebSocket socket = new();

    public WebSocketState State => socket.State;

    public Task ConnectAsync(Uri uri, CancellationToken cancellationToken)
    {
        return socket.ConnectAsync(uri, cancellationToken);
    }

    public ValueTask SendAsync(ReadOnlyMemory<byte> buffer, WebSocketMessageType messageType, bool endOfMessage, CancellationToken cancellationToken)
    {
        return socket.SendAsync(buffer, messageType, endOfMessage, cancellationToken);
    }

    public ValueTask<ValueWebSocketReceiveResult> ReceiveAsync(Memory<byte> buffer, CancellationToken cancellationToken)
    {
        return socket.ReceiveAsync(buffer, cancellationToken);
    }

    public void Dispose()
    {
        socket.Dispose();
    }
}
