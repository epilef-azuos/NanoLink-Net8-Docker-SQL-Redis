using FluentValidation;
using FluentValidation.Results;
using NanoLink.Api.Common;
using NanoLink.Api.Data;
using NanoLink.Api.Data.Entities;

namespace NanoLink.Api.Features.ShortenUrl;

public class ShortenUrlHandler(AppDbContext db, IValidator<ShortenUrlRequest> validator)
{
    private const int CodeLength = 7;

    public async Task<(ValidationResult Validation, ShortenUrlResponse? Response)> HandleAsync(
        ShortenUrlRequest request,
        string baseUrl,
        CancellationToken cancellationToken)
    {
        // Validação antes de qualquer acesso ao banco (fail-fast)
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return (validation, null);
        }

        var link = new Link(Base62Generator.Generate(CodeLength), request.OriginalUrl);

        db.Links.Add(link);
        await db.SaveChangesAsync(cancellationToken);

        var response = new ShortenUrlResponse(
            link.Code,
            $"{baseUrl}/{link.Code}",
            link.OriginalUrl,
            link.CreatedAt);

        return (validation, response);
    }
}
