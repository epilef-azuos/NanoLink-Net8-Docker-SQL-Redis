# 0002. Identificadores Base62 criptograficamente seguros

- **Status:** Aceita
- **Data:** 2026-10-21
- **Decisores:** Time NanoLink

## Contexto

Cada link encurtado precisa de um código curto, único e seguro para aparecer na URL. A escolha do formato afeta três coisas: o tamanho da URL, a capacidade de enumeração e a dependência de coordenação entre instâncias.

Se o código fosse um inteiro sequencial auto-incrementável, `https://nano.link/1`, `/2`, `/3`... seria trivial enumerar todos os links do sistema, expondo destinos que os usuários podem não querer tornar públicos. Além disso, um sequencial exige que o banco seja a fonte da sequência, o que acopla a geração ao PostgreSQL.

## Opções consideradas

1. **Inteiro sequencial** (ex.: `SEQUENCE` do PostgreSQL) codificado em Base62.
2. **UUID v4** completo, sem encurtamento.
3. **Código aleatório Base62** de 7 caracteres, gerado com `RandomNumberGenerator`.
4. **Hash** (ex.: SHA-256 da URL) truncado.

## Decisão

Geramos códigos de **7 caracteres** no alfabeto `[0-9a-zA-Z]` (Base62), usando `System.Security.Cryptography.RandomNumberGenerator.GetInt32` para cada caractere. O gerador fica em `Common/Base62Generator.cs`.

Com 62⁷ ≈ 3,5 trilhões de combinações, o espaço é grande o suficiente para tornar a enumeração impraticável, e a geração não depende de estado compartilhado.

Escolhemos aleatoriedade criptográfica em vez de `Random` porque `Random` é previsível: uma sequência de códigos observada permitiria inferir os próximos.

A unicidade é garantida pelo banco, com um índice único em `Links.Code`. Não há retry no código de aplicação (ver consequências).

## Consequências

### Positivas

- **Não enumerável:** códigos aleatórios não revelam ordem nem volume de links criados.
- **Sem coordenação:** qualquer instância gera códigos sem consultar um contador central, o que facilita escalar horizontalmente.
- **URLs curtas:** 7 caracteres mantêm o link legível e cabem em mensagens e QR codes.
- **Sem dependência do banco para gerar:** a geração é uma função pura de entropia.

### Negativas

- **Colisões possíveis:** embora improvável, uma colisão viola o índice único e a inserção falha com erro 500. O tratamento com nova tentativa ainda não foi implementado e está listado como próximo passo.
- **Códigos não são ordenáveis:** não é possível inferir a ordem de criação a partir do código, o que é desejável aqui, mas limita algumas consultas de paginação por código.
- **Tamanho fixo:** mudar o comprimento depois exige coordenar links antigos e novos, já que o redirecionamento valida o tamanho de entrada.
