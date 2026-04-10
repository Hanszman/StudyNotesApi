# StudyNotesApi

> Blueprint completo para uma API de estudos/anotações em **.NET + C# + Entity Framework Core + MySQL + Swagger + JWT + XUnit**, com arquitetura em camadas e roadmap de implementação guiado para o Codex.

---

## 1. Nome do projeto

**StudyNotesApi**

Motivos:
- é claro e direto
- comunica exatamente o domínio
- soa profissional no GitHub e no currículo
- funciona bem como solution name, namespace e nome de repositório

Sugestão de repositório:

```bash
git init StudyNotesApi
```

Sugestão de solution:

```bash
dotnet new sln -n StudyNotesApi
```

---

## 2. Objetivo do projeto

Construir uma API REST para gerenciamento de **notas de estudo**, com:

- cadastro e login com JWT
- CRUD de notas
- CRUD de categorias
- CRUD de tags
- associação N:N entre notas e tags
- filtros, paginação e ordenação
- validações de negócio
- testes unitários
- Swagger configurado
- cobertura de testes com meta de **100% nos arquivos aplicáveis**

O projeto foi pensado para ser simples no domínio, mas forte tecnicamente, para aprender na prática:

- organização real de API em .NET
- separação por camadas
- DTOs
- serviços
- repositórios
- Entity Framework Core
- autenticação e autorização
- testes com XUnit

---

## 3. Stack do projeto

### Backend
- .NET 8
- ASP.NET Core Web API
- C#

### Persistência
- Entity Framework Core
- MySQL
- Pomelo.EntityFrameworkCore.MySql

### Segurança
- JWT Bearer Authentication
- Password hashing

### Documentação
- Swagger / OpenAPI

### Testes
- XUnit
- FluentAssertions
- Moq
- coverlet

---

## 4. Arquitetura do projeto

Sim: essa estrutura em camadas é bem comum em projetos .NET profissionais, principalmente quando você quer separar responsabilidades e manter a base escalável.

Estrutura proposta:

```text
StudyNotesApi/
├── StudyNotesApi.sln
├── src/
│   ├── StudyNotesApi.Api/
│   │   ├── Controllers/
│   │   ├── DTOs/
│   │   │   ├── Auth/
│   │   │   ├── Categories/
│   │   │   ├── Notes/
│   │   │   ├── Tags/
│   │   │   └── Common/
│   │   ├── Mappings/
│   │   ├── Configurations/
│   │   ├── Middlewares/
│   │   ├── Extensions/
│   │   ├── Program.cs
│   │   ├── appsettings.json
│   │   ├── appsettings.Development.json
│   │   └── Properties/
│   │
│   ├── StudyNotesApi.Application/
│   │   ├── Interfaces/
│   │   │   ├── Services/
│   │   │   ├── Repositories/
│   │   │   └── Security/
│   │   ├── Services/
│   │   ├── Validators/
│   │   ├── Models/
│   │   └── Common/
│   │
│   ├── StudyNotesApi.Domain/
│   │   ├── Entities/
│   │   ├── Enums/
│   │   ├── ValueObjects/
│   │   └── Common/
│   │
│   └── StudyNotesApi.Infrastructure/
│       ├── Data/
│       │   ├── Context/
│       │   ├── Configurations/
│       │   └── Migrations/
│       ├── Repositories/
│       ├── Security/
│       ├── Extensions/
│       └── DependencyInjection/
│
├── tests/
│   ├── StudyNotesApi.UnitTests/
│   │   ├── Services/
│   │   ├── Validators/
│   │   ├── Security/
│   │   └── Helpers/
│   └── StudyNotesApi.ArchitectureTests/
│
├── .env.example
├── .gitignore
├── README.md
└── StudyNotesApi-Blueprint.md
```

---

## 5. Responsabilidade de cada camada

### 5.1 StudyNotesApi.Api
Responsável pela borda HTTP da aplicação.

Contém:
- controllers
- DTOs de request/response
- configurações de Swagger
- autenticação JWT
- tratamento global de erros
- mapeamentos entre DTOs e modelos internos

### 5.2 StudyNotesApi.Application
Responsável pelos casos de uso.

Contém:
- services
- interfaces
- validações de negócio
- paginação, filtros, ordenação
- regras de autorização por usuário

### 5.3 StudyNotesApi.Domain
Responsável pelo coração do domínio.

Contém:
- entidades
- enums
- regras mais centrais de negócio
- contratos básicos do domínio

### 5.4 StudyNotesApi.Infrastructure
Responsável por detalhes técnicos de persistência e integrações.

Contém:
- DbContext
- mapeamentos do EF
- repositórios
- migrations
- geração de JWT
- hashing de senha

### 5.5 Tests
Responsável por garantir comportamento.

Contém:
- testes unitários de services
- testes de validadores
- testes de security helpers
- testes de arquitetura, se quiser elevar o nível

---

## 6. Domínio da aplicação

A API será um mini sistema de notas de estudo, parecido com um backend simplificado de um app estilo Notion/Evernote/Obsidian.

### O usuário poderá:
- se cadastrar e logar
- criar, editar, listar e remover notas
- criar categorias
- criar tags
- vincular tags às notas
- buscar notas por texto
- filtrar notas por categoria, tag, favoritos, arquivadas
- paginar resultados
- ordenar resultados por múltiplos campos

---

## 7. Entidades do domínio

### 7.1 User
Representa o usuário autenticável do sistema.

Campos sugeridos:
- `Id` : Guid
- `Name` : string
- `Email` : string
- `PasswordHash` : string
- `CreatedAt` : DateTime
- `UpdatedAt` : DateTime?

Relacionamentos:
- 1:N com `Note`
- 1:N com `Category`
- 1:N com `Tag`

---

### 7.2 Category
Representa a categoria de uma nota.

Campos sugeridos:
- `Id` : Guid
- `Name` : string
- `Color` : string?
- `UserId` : Guid
- `CreatedAt` : DateTime
- `UpdatedAt` : DateTime?

Relacionamentos:
- N:1 com `User`
- 1:N com `Note`

Regras:
- o nome da categoria deve ser único por usuário

---

### 7.3 Tag
Representa uma etiqueta vinculada à nota.

Campos sugeridos:
- `Id` : Guid
- `Name` : string
- `UserId` : Guid
- `CreatedAt` : DateTime
- `UpdatedAt` : DateTime?

Relacionamentos:
- N:1 com `User`
- N:N com `Note` via `NoteTag`

Regras:
- o nome da tag deve ser único por usuário

---

### 7.4 Note
Representa a anotação em si.

Campos sugeridos:
- `Id` : Guid
- `Title` : string
- `Content` : string
- `IsFavorite` : bool
- `IsArchived` : bool
- `IsPinned` : bool
- `UserId` : Guid
- `CategoryId` : Guid?
- `CreatedAt` : DateTime
- `UpdatedAt` : DateTime?

Relacionamentos:
- N:1 com `User`
- N:1 com `Category`
- N:N com `Tag` via `NoteTag`

Regras:
- título é obrigatório
- nota pertence a um único usuário
- categoria, se informada, precisa pertencer ao mesmo usuário da nota
- tags vinculadas precisam pertencer ao mesmo usuário da nota

---

### 7.5 NoteTag
Tabela de junção para relacionamento N:N.

Campos sugeridos:
- `NoteId` : Guid
- `TagId` : Guid

Relacionamentos:
- N:1 com `Note`
- N:1 com `Tag`

Chave composta:
- `(NoteId, TagId)`

---

## 8. Relacionamentos resumidos

```text
User
 ├── Notes (1:N)
 ├── Categories (1:N)
 └── Tags (1:N)

Category
 └── Notes (1:N)

Note
 └── Tags (N:N via NoteTag)
```

---

## 9. Regras de negócio do V1

### Auth
- e-mail é obrigatório
- e-mail deve ser único
- senha é obrigatória
- senha deve ser armazenada com hash
- login deve retornar token JWT

### Notes
- título é obrigatório
- usuário só pode acessar as próprias notas
- usuário não pode associar categoria de outro usuário
- usuário não pode associar tags de outro usuário
- usuário pode marcar nota como favorita, arquivada e fixada

### Categories
- categoria deve ter nome
- nome da categoria não pode repetir para o mesmo usuário
- usuário só pode manipular as próprias categorias

### Tags
- tag deve ter nome
- nome da tag não pode repetir para o mesmo usuário
- usuário só pode manipular as próprias tags

---

## 10. Paginação e ordenação

Essas duas entram no V1 estendido.

### 10.1 Paginação
Todos os endpoints de listagem devem aceitar:
- `pageNumber`
- `pageSize`

Regras:
- `pageNumber` mínimo = 1
- `pageSize` mínimo = 1
- `pageSize` máximo = 100

Modelo de resposta paginada:

```json
{
  "items": [],
  "pageNumber": 1,
  "pageSize": 10,
  "totalCount": 42,
  "totalPages": 5,
  "hasPreviousPage": false,
  "hasNextPage": true
}
```

### 10.2 Ordenação
Todos os endpoints de listagem devem aceitar:
- `sortBy`
- `sortDirection`

Valores:
- `sortDirection=asc`
- `sortDirection=desc`

Campos ordenáveis por entidade:

#### Notes
- `title`
- `content`
- `isFavorite`
- `isArchived`
- `isPinned`
- `createdAt`
- `updatedAt`

#### Categories
- `name`
- `color`
- `createdAt`
- `updatedAt`

#### Tags
- `name`
- `createdAt`
- `updatedAt`

Regras:
- se `sortBy` for inválido, retornar 400
- se `sortDirection` for inválido, retornar 400
- definir ordenação padrão por `createdAt desc`

---

## 11. Filtros suportados

### Notes
- `search` → busca por texto em título e conteúdo
- `categoryId`
- `tagId`
- `isFavorite`
- `isArchived`
- `isPinned`

### Categories
- `search` → busca por nome

### Tags
- `search` → busca por nome

---

## 12. Endpoints do projeto

## 12.1 Auth

### POST `/api/auth/register`
Cria usuário.

Request:
```json
{
  "name": "Victor",
  "email": "victor@email.com",
  "password": "123456"
}
```

Response:
```json
{
  "id": "guid",
  "name": "Victor",
  "email": "victor@email.com"
}
```

### POST `/api/auth/login`
Autentica e retorna token.

Request:
```json
{
  "email": "victor@email.com",
  "password": "123456"
}
```

Response:
```json
{
  "accessToken": "jwt-token",
  "expiresIn": 3600,
  "tokenType": "Bearer"
}
```

---

## 12.2 Notes

### GET `/api/notes`
Lista notas com filtros, paginação e ordenação.

Query params:
- `search`
- `categoryId`
- `tagId`
- `isFavorite`
- `isArchived`
- `isPinned`
- `pageNumber`
- `pageSize`
- `sortBy`
- `sortDirection`

### GET `/api/notes/{id}`
Busca nota por id.

### POST `/api/notes`
Cria nota.

Request:
```json
{
  "title": "Estudar JWT",
  "content": "Aprender claims, expiration e authorization",
  "categoryId": "guid-opcional",
  "tagIds": ["guid-1", "guid-2"],
  "isFavorite": true,
  "isArchived": false,
  "isPinned": false
}
```

### PUT `/api/notes/{id}`
Atualiza nota.

### DELETE `/api/notes/{id}`
Remove nota.

---

## 12.3 Categories

### GET `/api/categories`
Lista categorias com paginação, filtro e ordenação.

### GET `/api/categories/{id}`
Busca categoria por id.

### POST `/api/categories`
Cria categoria.

Request:
```json
{
  "name": "Backend",
  "color": "#2563EB"
}
```

### PUT `/api/categories/{id}`
Atualiza categoria.

### DELETE `/api/categories/{id}`
Remove categoria.

---

## 12.4 Tags

### GET `/api/tags`
Lista tags com paginação, filtro e ordenação.

### GET `/api/tags/{id}`
Busca tag por id.

### POST `/api/tags`
Cria tag.

Request:
```json
{
  "name": "csharp"
}
```

### PUT `/api/tags/{id}`
Atualiza tag.

### DELETE `/api/tags/{id}`
Remove tag.

---

## 13. DTOs sugeridos

### Auth
- `RegisterRequestDto`
- `RegisterResponseDto`
- `LoginRequestDto`
- `LoginResponseDto`

### Notes
- `CreateNoteRequestDto`
- `UpdateNoteRequestDto`
- `NoteResponseDto`
- `NoteListItemResponseDto`
- `GetNotesQueryDto`

### Categories
- `CreateCategoryRequestDto`
- `UpdateCategoryRequestDto`
- `CategoryResponseDto`
- `GetCategoriesQueryDto`

### Tags
- `CreateTagRequestDto`
- `UpdateTagRequestDto`
- `TagResponseDto`
- `GetTagsQueryDto`

### Common
- `PagedResponseDto<T>`
- `ErrorResponseDto`

---

## 14. Interfaces sugeridas

### Services
- `IAuthService`
- `INoteService`
- `ICategoryService`
- `ITagService`

### Repositories
- `IUserRepository`
- `INoteRepository`
- `ICategoryRepository`
- `ITagRepository`

### Security
- `IJwtTokenGenerator`
- `IPasswordHasher`
- `ICurrentUserService`

---

## 15. Services sugeridos

- `AuthService`
- `NoteService`
- `CategoryService`
- `TagService`

Responsabilidades:
- validar regras de negócio
- consultar repositórios
- montar resultado paginado
- aplicar filtros
- aplicar ordenação
- garantir isolamento por usuário

---

## 16. Repositórios sugeridos

- `UserRepository`
- `NoteRepository`
- `CategoryRepository`
- `TagRepository`

Responsabilidades:
- persistência
- consultas com EF Core
- filtros baseados em usuário
- paginação e ordenação em queries

---

## 17. Segurança

### JWT
O token deve conter pelo menos:
- `sub` → UserId
- `email`
- `unique_name` ou `name`

### Hash de senha
Não armazenar senha em texto puro nunca.

Opções:
- começar com `BCrypt.Net-Next`
- ou encapsular hash em uma interface própria

Recomendação:
- criar `IPasswordHasher` e `PasswordHasher`
- criar `IJwtTokenGenerator` e `JwtTokenGenerator`

---

## 18. Banco de dados

Banco: **MySQL local**

Sugestão de nome do banco:
- `study_notes_api_db`

Sugestão de conexão local:

```env
DB_HOST=localhost
DB_PORT=3306
DB_NAME=study_notes_api_db
DB_USER=root
DB_PASSWORD=root
```

---

## 19. .env.example

Crie um arquivo `.env.example` na raiz com o seguinte conteúdo:

```env
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://localhost:5080

DB_HOST=localhost
DB_PORT=3306
DB_NAME=study_notes_api_db
DB_USER=root
DB_PASSWORD=root

JWT_SECRET=your-super-secret-key-with-at-least-32-characters
JWT_ISSUER=StudyNotesApi
JWT_AUDIENCE=StudyNotesApiUsers
JWT_EXPIRATION_MINUTES=60
```

### Observações
- em .NET, o `.env` não é carregado automaticamente por padrão como em Node
- você pode:
  - usar variáveis de ambiente do sistema
  - usar `appsettings.Development.json`
  - ou instalar lib para leitura de `.env`

Para aprendizado, uma abordagem boa é:
- manter `appsettings.json` e `appsettings.Development.json`
- usar `.env.example` apenas como referência de valores esperados

---

## 20. appsettings.json sugerido

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=study_notes_api_db;User=root;Password=root;"
  },
  "Jwt": {
    "Secret": "your-super-secret-key-with-at-least-32-characters",
    "Issuer": "StudyNotesApi",
    "Audience": "StudyNotesApiUsers",
    "ExpirationMinutes": 60
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

---

## 21. Guia de implementação por etapas

Essa parte é a mais importante para o Codex seguir item por item sem virar um bingo de arquivos aleatórios.

# Etapa 0 — Bootstrap da solution

## Objetivo
Criar a solution e os projetos base.

## Tarefas
1. Criar a solution `StudyNotesApi.sln`
2. Criar os projetos:
   - `StudyNotesApi.Api`
   - `StudyNotesApi.Application`
   - `StudyNotesApi.Domain`
   - `StudyNotesApi.Infrastructure`
   - `StudyNotesApi.UnitTests`
3. Adicionar referências entre projetos
4. Configurar pastas iniciais
5. Adicionar `README.md`, `.gitignore` e `.env.example`

## Dependências esperadas
### Api
- Swashbuckle.AspNetCore
- Microsoft.AspNetCore.Authentication.JwtBearer

### Infrastructure
- Microsoft.EntityFrameworkCore
- Microsoft.EntityFrameworkCore.Design
- Pomelo.EntityFrameworkCore.MySql
- BCrypt.Net-Next

### UnitTests
- xunit
- xunit.runner.visualstudio
- FluentAssertions
- Moq
- coverlet.collector

## Critério de pronto
- solution compila
- projetos referenciados corretamente
- estrutura base criada

---

# Etapa 1 — Domain

## Objetivo
Criar as entidades e o coração do domínio.

## Tarefas
1. Criar entidade `User`
2. Criar entidade `Category`
3. Criar entidade `Tag`
4. Criar entidade `Note`
5. Criar entidade `NoteTag`
6. Definir propriedades, navegações e relacionamentos
7. Criar classes base comuns se necessário, como `BaseEntity`

## Critério de pronto
- entidades modeladas
- sem dependência de EF ou detalhes de infraestrutura na camada de domínio

---

# Etapa 2 — Infrastructure/Data

## Objetivo
Configurar persistência com EF Core + MySQL.

## Tarefas
1. Criar `ApplicationDbContext`
2. Criar configurações por entidade usando Fluent API
3. Configurar chaves primárias
4. Configurar chave composta em `NoteTag`
5. Configurar índices únicos:
   - `User.Email`
   - `Category(Name, UserId)`
   - `Tag(Name, UserId)`
6. Configurar tamanhos máximos de campos
7. Configurar relacionamento opcional entre `Note` e `Category`
8. Registrar DbContext em DI
9. Criar migration inicial
10. Aplicar migration localmente

## Critério de pronto
- banco sobe
- tabelas criadas corretamente
- relacionamentos funcionando

---

# Etapa 3 — Contratos da Application

## Objetivo
Criar interfaces e modelos auxiliares para a camada de aplicação.

## Tarefas
1. Criar interfaces de services
2. Criar interfaces de repositories
3. Criar interfaces de security (`IJwtTokenGenerator`, `IPasswordHasher`, `ICurrentUserService`)
4. Criar modelos de paginação
5. Criar modelos de filtros e ordenação

## Critério de pronto
- contratos definidos
- camada de aplicação independente de controller e EF

---

# Etapa 4 — Repositórios

## Objetivo
Implementar acesso a dados.

## Tarefas
1. Implementar `UserRepository`
2. Implementar `CategoryRepository`
3. Implementar `TagRepository`
4. Implementar `NoteRepository`
5. Garantir queries sempre filtradas por usuário quando aplicável
6. Implementar paginação
7. Implementar ordenação dinâmica validada
8. Implementar busca textual para notas

## Critério de pronto
- repositórios funcionais
- consultas cobertas por testes indiretos via service

---

# Etapa 5 — Segurança

## Objetivo
Implementar hashing e geração de JWT.

## Tarefas
1. Implementar `PasswordHasher`
2. Implementar `JwtTokenGenerator`
3. Configurar JWT Bearer no projeto Api
4. Criar `CurrentUserService` para extrair UserId dos claims
5. Configurar authorization nos endpoints protegidos

## Critério de pronto
- token gerado corretamente
- senha hasheada e validada
- usuário autenticado identificado via token

---

# Etapa 6 — Auth

## Objetivo
Implementar cadastro e login.

## Tarefas
1. Criar DTOs de auth
2. Criar `AuthService`
3. Implementar cadastro
4. Implementar login
5. Validar e-mail duplicado
6. Validar senha incorreta
7. Criar `AuthController`
8. Expor endpoints de register e login no Swagger

## Critério de pronto
- usuário consegue registrar
- usuário consegue logar
- token aparece no Swagger e pode ser usado nos endpoints protegidos

---

# Etapa 7 — Categories CRUD

## Objetivo
Implementar CRUD completo de categorias.

## Tarefas
1. Criar DTOs de categoria
2. Criar `CategoryService`
3. Implementar `GET /api/categories`
4. Implementar `GET /api/categories/{id}`
5. Implementar `POST /api/categories`
6. Implementar `PUT /api/categories/{id}`
7. Implementar `DELETE /api/categories/{id}`
8. Adicionar paginação
9. Adicionar filtro por nome
10. Adicionar ordenação
11. Garantir que o usuário só veja/manipule as próprias categorias

## Critério de pronto
- CRUD completo funcionando via Swagger
- paginação e ordenação funcionando

---

# Etapa 8 — Tags CRUD

## Objetivo
Implementar CRUD completo de tags.

## Tarefas
1. Criar DTOs de tag
2. Criar `TagService`
3. Implementar `GET /api/tags`
4. Implementar `GET /api/tags/{id}`
5. Implementar `POST /api/tags`
6. Implementar `PUT /api/tags/{id}`
7. Implementar `DELETE /api/tags/{id}`
8. Adicionar paginação
9. Adicionar filtro por nome
10. Adicionar ordenação
11. Garantir isolamento por usuário

## Critério de pronto
- CRUD completo funcionando
- validação de nome duplicado por usuário

---

# Etapa 9 — Notes CRUD

## Objetivo
Implementar CRUD completo de notas com categoria e tags.

## Tarefas
1. Criar DTOs de nota
2. Criar `NoteService`
3. Implementar `GET /api/notes`
4. Implementar `GET /api/notes/{id}`
5. Implementar `POST /api/notes`
6. Implementar `PUT /api/notes/{id}`
7. Implementar `DELETE /api/notes/{id}`
8. Validar título obrigatório
9. Validar categoria pertencente ao usuário
10. Validar tags pertencentes ao usuário
11. Persistir relação com `NoteTag`
12. Implementar filtros
13. Implementar paginação
14. Implementar ordenação

## Critério de pronto
- CRUD completo funcionando
- filtros, paginação e ordenação funcionando

---

# Etapa 10 — Swagger redondo

## Objetivo
Deixar a documentação útil de verdade.

## Tarefas
1. Configurar título e versão da API
2. Configurar esquema Bearer no Swagger
3. Adicionar descrição dos endpoints
4. Garantir que endpoints protegidos possam ser testados pelo Swagger UI

## Critério de pronto
- Swagger funcional com autenticação

---

# Etapa 11 — Tratamento global de erros

## Objetivo
Evitar controller entulhado de try/catch.

## Tarefas
1. Criar middleware global de exceptions
2. Padronizar respostas de erro
3. Retornar códigos corretos:
   - 400
   - 401
   - 403
   - 404
   - 409
   - 500

## Critério de pronto
- erros previsíveis padronizados

---

# Etapa 12 — Testes unitários

## Objetivo
Cobrir services e classes utilitárias.

## Escopo mínimo
### AuthService
- registra usuário com sucesso
- falha com e-mail duplicado
- falha com credenciais inválidas
- retorna token no login válido

### CategoryService
- cria categoria
- falha ao duplicar nome
- lista somente categorias do usuário
- atualiza categoria existente
- remove categoria do usuário

### TagService
- cria tag
- falha ao duplicar nome
- lista somente tags do usuário
- atualiza tag existente
- remove tag do usuário

### NoteService
- cria nota com sucesso
- falha sem título
- falha ao usar categoria de outro usuário
- falha ao usar tag de outro usuário
- lista só notas do usuário
- atualiza nota do usuário
- remove nota do usuário
- aplica filtros corretamente
- aplica paginação corretamente
- aplica ordenação corretamente

### Security
- gera hash
- valida hash
- gera token com claims esperados

## Critério de pronto
- cobertura alta e confiável
- comportamento protegido por testes

---

# Etapa 13 — Coverage

## Objetivo
Garantir cobertura total do que faz sentido cobrir.

## Meta
- **100% coverage** em services, validators e security helpers
- excluir de cobertura arquivos que não agregam valor testar diretamente, como:
  - `Program.cs`
  - classes puramente de configuração
  - migrations
  - DTOs sem lógica

## Estratégia
1. concentrar regra de negócio em services
2. manter controllers finos
3. manter lógica condicional fora de classes de configuração
4. usar `[ExcludeFromCodeCoverage]` onde realmente fizer sentido

---

## 22. Convenções recomendadas

### Padrões gerais
- controllers magros
- services com regra de negócio
- repositories com persistência
- DTOs separados de entidades
- não expor entidade diretamente no controller
- async em tudo que tocar banco
- cancellation token onde fizer sentido

### Convenções de naming
- classes no singular: `Note`, `Tag`, `Category`
- controllers no plural: `NotesController`, `TagsController`
- interfaces prefixadas com `I`
- namespaces alinhados ao projeto

---

## 23. Scripts e comandos do projeto

Como .NET normalmente usa `dotnet` CLI em vez de scripts estilo npm, o mais correto aqui é documentar **comandos operacionais** no README. Se você quiser muito, pode criar um `Makefile` depois, mas não é obrigatório.

### 23.1 Restaurar pacotes
```bash
dotnet restore
```

### 23.2 Build da solution
```bash
dotnet build StudyNotesApi.sln
```

### 23.3 Rodar API
```bash
dotnet run --project src/StudyNotesApi.Api
```

### 23.4 Rodar em watch mode
```bash
dotnet watch --project src/StudyNotesApi.Api run
```

### 23.5 Criar migration
```bash
dotnet ef migrations add InitialCreate \
  --project src/StudyNotesApi.Infrastructure \
  --startup-project src/StudyNotesApi.Api \
  --output-dir Data/Migrations
```

### 23.6 Aplicar migration
```bash
dotnet ef database update \
  --project src/StudyNotesApi.Infrastructure \
  --startup-project src/StudyNotesApi.Api
```

### 23.7 Remover última migration
```bash
dotnet ef migrations remove \
  --project src/StudyNotesApi.Infrastructure \
  --startup-project src/StudyNotesApi.Api
```

### 23.8 Rodar testes
```bash
dotnet test StudyNotesApi.sln
```

### 23.9 Rodar testes com coverage
```bash
dotnet test tests/StudyNotesApi.UnitTests/StudyNotesApi.UnitTests.csproj \
  /p:CollectCoverage=true \
  /p:CoverletOutput=./TestResults/Coverage/ \
  /p:CoverletOutputFormat=json,cobertura \
  /p:ExcludeByFile="**/Program.cs;**/*Configuration.cs;**/Migrations/*;**/*Dto.cs"
```

### 23.10 Gerar relatório HTML de coverage
Instale a ferramenta:
```bash
dotnet tool install -g dotnet-reportgenerator-globaltool
```

Depois gere o relatório:
```bash
reportgenerator \
  -reports:"tests/StudyNotesApi.UnitTests/TestResults/Coverage/coverage.cobertura.xml" \
  -targetdir:"tests/StudyNotesApi.UnitTests/TestResults/CoverageReport" \
  -reporttypes:Html
```

### 23.11 Abrir Swagger
Após rodar a API:

```text
http://localhost:5080/swagger
```

> Ajuste a porta conforme a configuração real do projeto.

---

## 24. README.md sugerido

Seu `README.md` deve conter pelo menos:

1. nome do projeto
2. objetivo
3. stack usada
4. arquitetura em camadas
5. pré-requisitos
6. como configurar o `.env`
7. como configurar o banco MySQL
8. como restaurar pacotes
9. como criar/applicar migrations
10. como rodar a API
11. URL do Swagger
12. como rodar testes
13. como gerar coverage
14. roadmap do projeto
15. próximos passos

---

## 25. Pré-requisitos

- .NET SDK 8
- MySQL local instalado e rodando
- EF Core CLI instalado

Instalar EF CLI se necessário:

```bash
dotnet tool install --global dotnet-ef
```

Verificar instalação:

```bash
dotnet ef
```

---

## 26. Checklist final de entrega do V1

- [ ] Solution criada
- [ ] Arquitetura em camadas criada
- [ ] Entidades modeladas
- [ ] DbContext configurado
- [ ] MySQL configurado
- [ ] Migration inicial criada
- [ ] Auth com JWT funcionando
- [ ] CRUD de Categories funcionando
- [ ] CRUD de Tags funcionando
- [ ] CRUD de Notes funcionando
- [ ] Filtros funcionando
- [ ] Paginação funcionando
- [ ] Ordenação funcionando
- [ ] Swagger com Bearer funcionando
- [ ] Middleware global de erro funcionando
- [ ] Testes unitários cobrindo regras principais
- [ ] Coverage 100% no escopo aplicável

---

## 27. Ordem ideal de execução no Codex

Se você quiser guiar o Codex sem deixar ele sair criando coisa torta, manda em lotes assim:

1. **Crie a solution e a estrutura dos projetos**
2. **Modele as entidades do domínio**
3. **Configure o DbContext e os mapeamentos do EF Core para MySQL**
4. **Implemente as interfaces da camada Application**
5. **Implemente hashing de senha e JWT**
6. **Implemente AuthController + AuthService + testes**
7. **Implemente Categories CRUD completo + testes**
8. **Implemente Tags CRUD completo + testes**
9. **Implemente Notes CRUD completo com NoteTag + testes**
10. **Implemente paginação, filtros e ordenação em todos os endpoints de listagem + testes**
11. **Implemente middleware global de erros**
12. **Configure Swagger com Bearer auth**
13. **Ajuste coverage para 100% no escopo aplicável**
14. **Atualize o README com todos os comandos operacionais**

---

## 28. Próximas evoluções possíveis

Depois do V1, você pode evoluir para:
- soft delete
- refresh token
- roles/perfis
- compartilhamento de notas
- anexos
- histórico de edição
- Docker Compose com MySQL
- testes de integração
- arquitetura CQRS

---

## 29. Conclusão

**StudyNotesApi** é um projeto excelente pra aprender backend .NET do jeito certo:
- domínio simples
- arquitetura profissional
- CRUD suficiente pra aprender de verdade
- auth com JWT
- EF Core com MySQL
- testes com cobertura alta

É pequeno o bastante pra terminar, mas robusto o suficiente pra te ensinar padrão de projeto real.

