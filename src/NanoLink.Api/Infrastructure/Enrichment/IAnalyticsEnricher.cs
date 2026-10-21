using NanoLink.Api.Data.Entities;

namespace NanoLink.Api.Infrastructure.Enrichment;

public interface IAnalyticsEnricher
{
    void EnrichEvent(ClickEvent evt);
}
