# 0004. Negative Caching com sentinela e Jitter anti-stampede

- **Status:** Aceita
- **Data:** 2026-10-21
- **Decisores:** Time NanoLink

## Contexto

O redirecionamento é a operação mais frequente do sistema. Sem cache, cada `GET /{code}` consultaria o PostgreSQL. Dois problemas aparecem nesse cenário:

1. **Cache penetration:** requisições para códigos que **não existem** nunca encontram cache, então cada uma vai ao banco. Um atacante que enumera códigos aleatórios pode esgotar conexões do PostgreSQL sem nunca acertar um link real.
2. **Cache stampede:** se muitos links expiram ao mesmo tempo, todas as requisições de um mesmo período viram cache miss simultâneo e disparam uma avalanche de consultas.

## Opções consideradas

Para o problema de penetração:

1. **Nada:** consultar o banco em todo miss.
2. **Bloom filter** de códigos existentes.
3. **Negative caching:** gravar a ausência de um código com um TTL curto.

Para o problema de stampede:

1. **TTL fixo** para todos os links.
2. **TTL fixo mais jitter** aleatório, espalhando as expirações.
3. **Lock distribuído** (single-flight) na reconstrução do cache.

## Decisão

**Negative caching:** quando um código não existe no banco, gravamos a chave `link:{code}` com o valor sentinela `__NOT_FOUND__` e TTL de **60 segundos**. Requisições repetidas para o mesmo código inexistente são respondidas `404` pelo Redis, sem nova consulta ao banco.

O TTL é curto de propósito. Um link criado logo após uma consulta negativa fica invisível por no máximo 60 s, um custo aceitável em troca de proteção contra penetração.

**Jitter:** links válidos ficam em cache por **24 horas mais um jitter aleatório de 0 a 1800 segundos** (até 30 minutos). O valor é gerado por `Random.Shared.Next(0, 1801)` a cada gravação. Assim, links populados no mesmo instante não expiram juntos.

**Read-through:** o `RedirectUrlHandler` consulta o Redis primeiro. Em miss, consulta o PostgreSQL e grava o resultado no Redis antes de responder. Não há single-flight: se muitas requisições chegarem ao mesmo tempo para um código sem cache, todas consultam o banco. O jitter reduz a chance disso ao espalhar as expirações, mas não a elimina.

## Consequências

### Positivas

- **Proteção contra penetração:** códigos inexistentes repetidos não atingem o PostgreSQL. Validado nos testes de integração: a chave `link:{code}` contém `__NOT_FOUND__` após o primeiro 404.
- **Dispersão de expirações:** o jitter evita que todos os links populados juntos expirem na mesma janela.
- **Sem estrutura extra:** a sentinela usa o mesmo Redis e o mesmo formato de chave do cache positivo.
- **Sem serialização:** a URL e a sentinela são strings brutas, o que mantém o caminho crítico livre de JSON.

### Negativas

- **Atraso de visibilidade:** um link criado até 60 s após uma consulta negativa para o mesmo código fica indisponível nesse intervalo.
- **Sem single-flight:** o jitter não elimina o stampede totalmente. Um pico concentrado em um único código sem cache ainda pode gerar várias consultas simultâneas.
- **Memória de sentinelas:** um atacante que gera muitos códigos distintos ocupa Redis com sentinelas. Cada uma é pequena e expira em 60 s, mas o volume é limitado pelo tráfego, e não pelo TTL.
- **Dependência de disponibilidade do Redis:** com o Redis fora do ar, todo o tráfego cai no PostgreSQL. O sistema continua funcionando, mas sem a proteção do cache.
