using FluentValidation;

namespace NanoLink.Api.Features.ShortenUrl;

public class ShortenUrlValidator : AbstractValidator<ShortenUrlRequest>
{
    public ShortenUrlValidator()
    {
        RuleFor(request => request.OriginalUrl)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("O campo 'originalUrl' é obrigatório.")
            .MaximumLength(2048).WithMessage("O campo 'originalUrl' deve ter no máximo 2048 caracteres.")
            .Must(BeAbsoluteHttpUrl).WithMessage("O campo 'originalUrl' deve ser uma URL absoluta com esquema http ou https.");
    }

    private static bool BeAbsoluteHttpUrl(string? url) =>
        !string.IsNullOrWhiteSpace(url)
        && Uri.TryCreate(url, UriKind.Absolute, out var uri)
        && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
}
