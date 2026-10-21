# Regras Invioláveis de Engenharia - Fase 02

1. **Adesão Estrita a VSA**: É estritamente proibido criar pastas genéricas de camada (ex.: `Services/`, `Controllers/`, `Repositories/`). Todo o fluxo de negócio do endpoint reside em `Features/ShortenUrl/`.
2. **Validação Fail-Fast**: Nenhum acesso ao banco de dados deve ocorrer antes da validação completa do payload com o FluentValidation.
3. **Geração Criptograficamente Segura**: O gerador Base62 deve utilizar `System.Security.Cryptography.RandomNumberGenerator` para evitar previsibilidade sequencial e colisões triviais.
4. **Isolamento Transacional**: A entidade `Link` deve encapsular as suas propriedades com mutação restrita (`init` ou métodos de domínio).