# StudyNotesApi

API REST para gerenciamento de notas de estudo, construída em camadas com ASP.NET Core, C#, EF Core, MySQL, Swagger, JWT e XUnit.

## Status atual

Estamos concluindo a `Etapa 0` do blueprint:

- solution reorganizada em `src/` e `tests/`
- template `WeatherForecast` removido
- Swagger base configurado
- `.env` local carregado automaticamente no startup
- MySQL local apontando para `study-notes-db` no XAMPP
- projeto de testes preparado para receber os primeiros testes de regra de negocio a partir da proxima etapa

## Estrutura

```text
src/
  StudyNotesApi.Api/
  StudyNotesApi.Application/
  StudyNotesApi.Domain/
  StudyNotesApi.Infrastructure/
tests/
  StudyNotesApi.UnitTests/
```

## Configuracao local

O projeto agora procura um arquivo `.env` na raiz da solution antes de subir a API. Isso permite manter a configuracao local simples sem depender de variaveis globais no Windows.

Configuracao atual esperada:

- Host: `localhost`
- Porta MySQL: `3306`
- Banco: `study-notes-db`
- Usuario: `root`
- Senha: vazia (padrao comum do XAMPP)

Se voce alterar a senha do MySQL no XAMPP, atualize a linha `ConnectionStrings__DefaultConnection` no arquivo `.env`.

## Comandos principais

Restaurar dependencias:

```powershell
dotnet restore StudyNotesApi.sln
```

Compilar a solution:

```powershell
dotnet build StudyNotesApi.sln
```

Rodar a API:

```powershell
dotnet run --project src/StudyNotesApi.Api/StudyNotesApi.Api.csproj
```

Rodar em modo watch:

```powershell
dotnet watch --project src/StudyNotesApi.Api/StudyNotesApi.Api.csproj run
```

Rodar testes unitarios:

```powershell
dotnet test tests/StudyNotesApi.UnitTests/StudyNotesApi.UnitTests.csproj
```

Abrir Swagger:

```text
http://localhost:5080/swagger
```

## Como vamos evoluir

Vamos seguir o blueprint por incrementos pequenos. A cada etapa funcional, eu vou:

1. implementar a parte da stack
2. adicionar ou ajustar os testes daquela etapa
3. atualizar este `README.md` com os comandos e observacoes necessarios
4. atualizar o blueprint se a execucao real pedir algum ajuste

## Proximo passo sugerido

Entrar na `Etapa 1 - Domain` para modelar `User`, `Category`, `Tag`, `Note` e `NoteTag`, ja preparando a base para os testes das regras de negocio nas proximas etapas.
