# Feature Specification: Backend do Widget de Comentários

**Feature Branch**: `001-comments-widget-backend`
**Created**: 2026-04-05
**Status**: Draft
**Input**: User description: "Crie o backend de um widget de comentários que pode ser colocado em qualquer website"

## Clarifications

### Session 2026-04-05

- Q: Como o sistema identifica a qual página um comentário pertence? → A: URL completa da página (ex: `https://site.com/blog/post-1`)
- Q: Quem pode editar/excluir comentários? → A: Autor pode editar e excluir (soft delete) + moderador do tenant pode excluir qualquer comentário
- Q: Como o tenant é identificado nas requisições? → A: Header customizado `X-Tenant-Id`
- Q: Qual abordagem de rate limiting? → A: Rate limiting por IP + por usuário autenticado (ex: max 10 comentários/minuto)
- Q: Tamanho padrão da página na paginação? → A: 15 comentários por página

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Leitura de Comentários (Priority: P1)

Um visitante acessa qualquer website que integra o widget de comentários e visualiza os comentários existentes sem precisar estar autenticado. Ele pode rolar a lista infinitamente, carregando mais comentários conforme desce a página, em lotes de 15 comentários. Pode também alternar a ordenação entre "mais recentes" e "mais populares".

**Why this priority**: A leitura é a funcionalidade mais básica e essencial — a maioria dos visitantes apenas lê comentários. Sem isso, o widget não tem razão de existir.

**Independent Test**: Pode ser testado enviando uma requisição GET ao endpoint de comentários sem token de autenticação e verificando que a lista paginada é retornada com ordenação correta.

**Acceptance Scenarios**:

1. **Given** um website com comentários existentes, **When** um visitante não autenticado acessa a lista de comentários informando a URL da página, **Then** o sistema retorna uma lista paginada (cursor-based, 15 por página) com os comentários e suas respostas aninhadas
2. **Given** uma lista de comentários, **When** o visitante solicita ordenação por "mais populares", **Then** os comentários são retornados ordenados pela quantidade de likes (decrescente)
3. **Given** uma lista de comentários, **When** o visitante solicita ordenação por "mais recentes", **Then** os comentários são retornados ordenados pela data de criação (decrescente)
4. **Given** uma página de comentários exibida, **When** o visitante rola até o final da lista, **Then** o sistema retorna o próximo lote de 15 comentários usando o cursor da página anterior

---

### User Story 2 - Criação de Comentários (Priority: P2)

Um usuário autenticado escreve um novo comentário em uma página (identificada pela URL). O comentário pode conter texto, emojis e GIFs do Giphy. O sistema persiste o comentário e o torna visível para todos os visitantes.

**Why this priority**: Sem a criação de comentários, o widget seria apenas leitura. Esta é a segunda funcionalidade mais importante para gerar engajamento.

**Independent Test**: Pode ser testado autenticando via NAuth, enviando um POST com conteúdo de comentário (texto + emoji + GIF URL) e verificando que o comentário aparece na listagem.

**Acceptance Scenarios**:

1. **Given** um usuário autenticado, **When** ele envia um comentário com texto simples para uma URL de página, **Then** o sistema persiste o comentário vinculado àquela URL e retorna o comentário criado com status 201
2. **Given** um usuário autenticado, **When** ele envia um comentário com emojis no texto, **Then** o sistema persiste corretamente os caracteres Unicode dos emojis
3. **Given** um usuário autenticado, **When** ele busca GIFs no Giphy e seleciona um, **Then** a URL do GIF é incluída no comentário e armazenada
4. **Given** um usuário não autenticado, **When** ele tenta criar um comentário, **Then** o sistema retorna erro 401 Unauthorized
5. **Given** um usuário autenticado que excedeu o rate limit (10 comentários/minuto), **When** ele tenta criar outro comentário, **Then** o sistema retorna erro 429 Too Many Requests

---

### User Story 3 - Respostas a Comentários (Priority: P3)

Um usuário autenticado responde a um comentário existente, criando uma thread de conversação. As respostas ficam vinculadas ao comentário pai e são exibidas de forma hierárquica.

**Why this priority**: Respostas transformam o widget de uma lista simples em uma ferramenta de conversação, aumentando significativamente o engajamento.

**Independent Test**: Pode ser testado criando um comentário e depois enviando uma resposta vinculada a ele, verificando que a resposta aparece como filho do comentário pai.

**Acceptance Scenarios**:

1. **Given** um comentário existente e um usuário autenticado, **When** ele envia uma resposta ao comentário, **Then** o sistema cria o comentário filho vinculado ao pai
2. **Given** um comentário com respostas, **When** qualquer visitante consulta o comentário, **Then** as respostas são retornadas aninhadas sob o comentário pai
3. **Given** um usuário autenticado, **When** ele tenta responder a um comentário inexistente, **Then** o sistema retorna erro 404

---

### User Story 4 - Likes em Comentários (Priority: P4)

Um usuário autenticado pode marcar um "like" (coração) em qualquer comentário. Cada usuário pode dar apenas um like por comentário. A contagem de likes influencia a ordenação por popularidade.

**Why this priority**: Likes fornecem feedback social e alimentam o algoritmo de ordenação por popularidade, complementando as funcionalidades de leitura e escrita.

**Independent Test**: Pode ser testado autenticando, enviando um like para um comentário e verificando que a contagem incrementou e que tentar dar like novamente remove o like anterior (toggle).

**Acceptance Scenarios**:

1. **Given** um comentário existente e um usuário autenticado, **When** ele dá like no comentário, **Then** a contagem de likes incrementa em 1
2. **Given** um comentário que o usuário já curtiu, **When** ele clica em like novamente, **Then** o like é removido e a contagem decrementa em 1 (toggle)
3. **Given** um usuário não autenticado, **When** ele tenta dar like, **Then** o sistema retorna erro 401 Unauthorized
4. **Given** comentários com diferentes quantidades de likes, **When** a ordenação por "mais populares" é solicitada, **Then** os comentários com mais likes aparecem primeiro

---

### User Story 5 - Integração Giphy (Priority: P5)

O sistema disponibiliza um endpoint para buscar GIFs no Giphy, permitindo que o frontend apresente uma interface de seleção de GIFs. O usuário busca por termo e seleciona um GIF para incluir no comentário.

**Why this priority**: A integração com Giphy é uma funcionalidade de enriquecimento que melhora a experiência mas não é essencial para o funcionamento básico.

**Independent Test**: Pode ser testado enviando uma busca por termo ao endpoint de pesquisa do Giphy e verificando que os resultados contêm URLs válidas de GIFs.

**Acceptance Scenarios**:

1. **Given** um usuário autenticado, **When** ele busca GIFs por um termo (ex: "feliz"), **Then** o sistema retorna uma lista paginada de GIFs do Giphy com URLs e previews
2. **Given** um termo de busca sem resultados, **When** a busca é realizada, **Then** o sistema retorna uma lista vazia
3. **Given** um usuário autenticado, **When** ele seleciona um GIF da lista, **Then** a URL do GIF é incluída no payload do comentário

---

### User Story 6 - Multi-Tenant e Autenticação (Priority: P6)

O sistema suporta múltiplos tenants (websites) isolados, identificados via header `X-Tenant-Id`. Cada tenant possui sua própria configuração de autenticação NAuth. Comentários de um tenant não são visíveis em outro.

**Why this priority**: Multi-tenancy é infraestrutural e essencial para o modelo de negócio, mas pode ser adicionado após as funcionalidades core estarem funcionando.

**Independent Test**: Pode ser testado criando comentários com tokens de tenants diferentes e verificando que cada tenant só vê seus próprios comentários.

**Acceptance Scenarios**:

1. **Given** dois tenants distintos, **When** um comentário é criado no Tenant A, **Then** ele não aparece na listagem do Tenant B
2. **Given** um token de autenticação do Tenant A, **When** ele é usado para acessar recursos do Tenant B, **Then** o sistema retorna erro 403 Forbidden
3. **Given** um novo tenant configurado, **When** um usuário se autentica com credenciais válidas desse tenant, **Then** o sistema emite um token válido associado ao tenant
4. **Given** uma requisição sem o header `X-Tenant-Id`, **When** qualquer endpoint é acessado, **Then** o sistema retorna erro 400 Bad Request

---

### User Story 7 - Edição e Exclusão de Comentários (Priority: P7)

O autor de um comentário pode editar o conteúdo ou excluir (soft delete) seu próprio comentário. Um moderador do tenant pode excluir qualquer comentário dentro do seu tenant.

**Why this priority**: Funcionalidade de moderação que complementa o fluxo principal, importante para manutenção da qualidade do conteúdo.

**Independent Test**: Pode ser testado criando um comentário, editando-o e verificando que o conteúdo foi atualizado; depois excluindo e verificando que aparece como removido.

**Acceptance Scenarios**:

1. **Given** um comentário criado pelo usuário autenticado, **When** ele edita o conteúdo, **Then** o sistema atualiza o comentário e registra que foi editado
2. **Given** um comentário criado pelo usuário autenticado, **When** ele exclui o comentário, **Then** o sistema marca como soft deleted e o conteúdo não é mais exibido
3. **Given** um comentário de qualquer usuário, **When** um moderador do tenant exclui o comentário, **Then** o sistema marca como soft deleted
4. **Given** um comentário de outro usuário, **When** um usuário não-moderador tenta excluí-lo, **Then** o sistema retorna erro 403 Forbidden
5. **Given** um comentário excluído (soft delete) que possui respostas, **When** a listagem é consultada, **Then** o comentário aparece com indicação de removido e as respostas são mantidas

---

### Edge Cases

- O que acontece quando um comentário pai é excluído mas possui respostas? As respostas DEVEM ser mantidas com indicação de que o comentário pai foi removido (soft delete)
- Como o sistema lida com conteúdo excessivamente longo? Comentários DEVEM ter limite máximo de 2000 caracteres
- O que acontece com paginação quando novos comentários são adicionados durante a navegação? A paginação por cursor garante consistência
- Como o sistema lida com URLs de GIF inválidas ou expiradas? O sistema armazena a URL original sem validação de disponibilidade — a renderização é responsabilidade do frontend
- O que acontece quando o serviço do Giphy está indisponível? O endpoint de busca retorna erro 503 com mensagem amigável
- O que acontece quando um usuário excede o rate limit? O sistema retorna 429 Too Many Requests com header `Retry-After`
- O que acontece quando o header `X-Tenant-Id` está ausente? O sistema retorna erro 400 Bad Request

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: O sistema DEVE permitir leitura de comentários sem autenticação
- **FR-002**: O sistema DEVE exigir autenticação via NAuth para criação de comentários, respostas, likes e edição/exclusão
- **FR-003**: O sistema DEVE suportar paginação por cursor (keyset pagination) com tamanho padrão de 15 comentários por página
- **FR-004**: O sistema DEVE suportar ordenação por "mais recentes" (data de criação) e "mais populares" (contagem de likes)
- **FR-005**: O sistema DEVE permitir respostas aninhadas a comentários (1 nível de profundidade)
- **FR-006**: O sistema DEVE suportar like/unlike (toggle) em comentários, limitado a 1 like por usuário por comentário
- **FR-007**: O sistema DEVE fornecer endpoint de busca de GIFs via API do Giphy
- **FR-008**: O sistema DEVE suportar emojis (caracteres Unicode) no conteúdo dos comentários
- **FR-009**: O sistema DEVE isolar dados por tenant — comentários de um tenant não são visíveis em outro
- **FR-010**: O sistema DEVE identificar o tenant via header `X-Tenant-Id` em todas as requisições
- **FR-011**: O sistema DEVE limitar o conteúdo dos comentários a 2000 caracteres
- **FR-012**: O sistema DEVE retornar a contagem de likes junto com cada comentário
- **FR-013**: O sistema DEVE indicar se o usuário autenticado já curtiu cada comentário na listagem
- **FR-014**: O sistema DEVE possuir testes unitários cobrindo serviços e regras de negócio
- **FR-015**: O sistema DEVE possuir testes de API (integração) cobrindo todos os endpoints
- **FR-016**: O sistema DEVE possuir documentação da API para consumo pelo frontend
- **FR-017**: O sistema DEVE vincular cada comentário à URL da página onde foi criado
- **FR-018**: O autor DEVE poder editar e excluir (soft delete) seus próprios comentários
- **FR-019**: Moderadores do tenant DEVEM poder excluir qualquer comentário dentro do seu tenant
- **FR-020**: O sistema DEVE aplicar rate limiting por IP e por usuário autenticado na criação de comentários

### Key Entities

- **Tenant**: Representa um website/aplicação que utiliza o widget. Possui identificador único, nome e configuração de autenticação NAuth
- **User**: Usuário autenticado via NAuth, vinculado a um tenant. Possui nome de exibição, identificador externo do NAuth e role (user/moderator)
- **Comment**: Comentário principal ou resposta. Possui conteúdo (texto com emojis), URL de GIF opcional, URL da página onde foi criado, referência ao comentário pai (quando é resposta), autor, tenant, timestamps, flag de editado, flag de soft delete e contagem de likes
- **CommentLike**: Registro de like de um usuário em um comentário. Garante unicidade por par usuário-comentário

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Visitantes não autenticados conseguem visualizar comentários em menos de 2 segundos após o carregamento da página
- **SC-002**: O sistema suporta pelo menos 500 usuários concorrentes lendo comentários sem degradação perceptível
- **SC-003**: A criação de um comentário (incluindo com GIF e emojis) é concluída em menos de 3 segundos do ponto de vista do usuário
- **SC-004**: A paginação por cursor carrega cada página adicional de 15 comentários em menos de 1 segundo
- **SC-005**: Comentários de um tenant nunca aparecem para usuários de outro tenant (isolamento 100%)
- **SC-006**: Cobertura de testes unitários nos serviços de domínio acima de 80%
- **SC-007**: Todos os endpoints da API possuem pelo menos um teste de integração
- **SC-008**: Documentação da API cobre 100% dos endpoints com exemplos de request/response

## Assumptions

- O frontend será desenvolvido separadamente e consumirá esta API — este projeto é apenas o backend
- A autenticação é gerida inteiramente pelo NAuth — o backend não implementa registro/login próprio
- O multi-tenant aplica-se apenas à camada de autenticação NAuth — o isolamento de dados por tenant é feito via filtro no banco de dados
- Respostas a comentários são limitadas a 1 nível de profundidade (não há respostas de respostas)
- A API do Giphy é acessada via chave de API configurada no backend — o frontend não acessa o Giphy diretamente
- O encoding do banco de dados suporta UTF-8 completo para emojis Unicode
- O comportamento de like é toggle: clicar uma vez adiciona, clicar novamente remove
- A paginação por cursor usa o ID ou timestamp do último item como cursor, com tamanho padrão de 15 itens
- Os testes de API utilizam Flurl.Http + xUnit + FluentAssertions em projeto separado
- Os testes de API fazem login uma vez por sessão de teste e reutilizam o token
- Comentários são identificados pela URL completa da página onde foram criados
- Soft delete mantém o registro no banco com flag, preservando respostas vinculadas
- Roles de usuário: "user" (padrão) e "moderator" (pode excluir qualquer comentário do tenant)
