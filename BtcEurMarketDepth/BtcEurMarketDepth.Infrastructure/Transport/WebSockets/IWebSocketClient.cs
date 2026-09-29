using System.Net.WebSockets;

namespace BtcEurMarketDepth.Infrastructure.Transport.WebSockets;

public interface IWebSocketClient : IDisposable
{
    WebSocketState State { get; }

    Task ConnectAsync(Uri uri, CancellationToken cancellationToken);

    ValueTask SendAsync(ReadOnlyMemory<byte> buffer, WebSocketMessageType messageType, bool endOfMessage, CancellationToken cancellationToken);

    ValueTask<ValueWebSocketReceiveResult> ReceiveAsync(Memory<byte> buffer, CancellationToken cancellationToken);
}
