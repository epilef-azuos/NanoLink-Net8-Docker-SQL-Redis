using NanoLink.Api.Data;
using NanoLink.Api.Data.Entities;
using NanoLink.Api.Infrastructure.Enrichment;

namespace NanoLink.Api.Infrastructure.Analytics;

public sealed class AnalyticsBatchWorker(
    ClickEventChannel channel,
    IServiceScopeFactory scopeFactory,
    ILogger<AnalyticsBatchWorker> logger) : BackgroundService
{
    private const int BatchSize = 100;
    private static readonly TimeSpan FlushInterval = TimeSpan.FromSeconds(5);

    private readonly List<ClickEvent> _buffer = new(BatchSize);
    private DateTimeOffset _firstItemAt;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // A leitura não usa stoppingToken: o encerramento vem do Complete() em StopAsync,
        // que permite drenar o canal antes de o processo terminar
        while (true)
        {
            var channelOpen = await FillBufferAsync();

            if (_buffer.Count > 0)
            {
                await FlushAsync(channelOpen ? stoppingToken : CancellationToken.None);
            }

            if (!channelOpen)
            {
                break;
            }
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        channel.Complete();
        await base.StopAsync(cancellationToken);
    }

    // Retorna false quando o canal foi concluído e não há mais eventos a ler
    private async Task<bool> FillBufferAsync()
    {
        using var timeoutCts = new CancellationTokenSource();

        if (_buffer.Count > 0)
        {
            // O prazo de 5 segundos conta a partir do primeiro item do lote
            var remaining = _firstItemAt + FlushInterval - DateTimeOffset.UtcNow;
            timeoutCts.CancelAfter(remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero);
        }

        try
        {
            await foreach (var evt in channel.ReadAllAsync(timeoutCts.Token))
            {
                if (_buffer.Count == 0)
                {
                    // Arma o prazo de 5 segundos no primeiro item, mesmo quando o buffer estava vazio
                    _firstItemAt = DateTimeOffset.UtcNow;
                    timeoutCts.CancelAfter(FlushInterval);
                }

                _buffer.Add(evt);

                if (_buffer.Count >= BatchSize)
                {
                    return true;
                }
            }

            return false;
        }
        catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested)
        {
            // Janela de 5 segundos expirou: dispara o flush por tempo
            return true;
        }
    }

    private async Task FlushAsync(CancellationToken ct)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var enricher = scope.ServiceProvider.GetRequiredService<IAnalyticsEnricher>();

            // Enriquecimento fica fora do redirect: roda apenas aqui, em segundo plano
            foreach (var evt in _buffer)
            {
                enricher.EnrichEvent(evt);
            }

            await db.ClickEvents.AddRangeAsync(_buffer, ct);
            await db.SaveChangesAsync(ct);

            logger.LogInformation("Gravados {Count} cliques em lote na tabela ClickEvents", _buffer.Count);
        }
        catch (Exception ex)
        {
            // Falha transitória de banco não pode derrubar o worker nem o processo da API
            logger.LogError(ex, "Falha ao gravar lote de {Count} cliques; o lote foi descartado", _buffer.Count);
        }
        finally
        {
            _buffer.Clear();
        }
    }
}
