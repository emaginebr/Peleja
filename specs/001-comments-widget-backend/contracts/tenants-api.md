# API Contract: Tenants

**Base Path**: `/api/v1/tenants`
**Note**: Endpoints de gestão de tenants são administrativos.

---

## Middleware: TenantMiddleware

Todas as requisições (exceto endpoints administrativos) DEVEM incluir o header `X-Tenant-Id`.

**Header**: `X-Tenant-Id: {tenant_slug}`

**Comportamento**:
- Valida que o header está presente
- Busca o tenant pelo slug
- Verifica que o tenant está ativo (`is_active = true`)
- Propaga `TenantContext` (scoped) com o `tenant_id`
- Se ausente ou inválido: retorna 400

**Response 400** (header ausente):

```json
{
  "sucesso": false,
  "mensagem": "Header X-Tenant-Id é obrigatório"
}
```

**Response 400** (tenant inválido/inativo):

```json
{
  "sucesso": false,
  "mensagem": "Tenant não encontrado ou inativo"
}
```

---

## Autenticação NAuth (por tenant)

Cada tenant possui sua própria URL e API key do NAuth. O `NAuthHandler` usa as configurações do tenant corrente (via `TenantContext`) para validar tokens.

**Flow**:
1. TenantMiddleware identifica o tenant via `X-Tenant-Id`
2. NAuthHandler usa `nauth_api_url` e `nauth_api_key` do tenant para validar o `Authorization: Basic {token}`
3. Se válido, o usuário é identificado e associado ao tenant
4. Se o usuário não existe localmente, é criado automaticamente (auto-provisioning)

**Auto-provisioning de usuários**:
- Na primeira autenticação, o sistema cria o registro `User` com dados do NAuth
- Role padrão: `User` (1)
- Promoção a `Moderator` (2) é feita via configuração administrativa
