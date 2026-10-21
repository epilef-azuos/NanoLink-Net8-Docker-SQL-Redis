using System.Threading.Channels;
using NanoLink.Api.Data.Entities;

namespace NanoLink.Api.Infrastructure.Analytics;

public sealed class ClickEventChannel
{
    private const int Capacity = 50_000;

    // Bounded: uma fila sem limite sob ataque de tráfego causaria OOM
    private readonly Channel<ClickEvent> _channel = Channel.CreateBounded<ClickEvent>(new BoundedChannelOptions(Capacity)
    {
        FullMode = BoundedChannelFullMode.DropOldest,
        SingleWriter = false,
        SingleReader = true
    });

    // Não bloqueante: o redirecionamento nunca espera pela gravação analítica
    public bool TryWrite(ClickEvent evt) => _channel.Writer.TryWrite(evt);

    public IAsyncEnumerable<ClickEvent> ReadAllAsync(CancellationToken ct) => _channel.Reader.ReadAllAsync(ct);

    // Encerra a escrita para que o worker drene o que restou no shutdown
    public void Complete() => _channel.Writer.TryComplete();
}
