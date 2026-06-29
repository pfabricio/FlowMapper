---
name: flow-core
description: Use when working with FlowCore mediator library — CQRS setup, commands, queries, events, pipeline behaviors, validation, caching, transactions, or event dispatching. Trigger on mentions of FlowCore, IFlowMediator, ICommand, IQuery, IEvent, SendAsync, QueryAsync, PublishAsync, or MediatR alternatives.
---

# FlowCore Skill

Mediator CQRS leve e extensível para .NET 8+ com pipeline de behaviors. Alternativa ao MediatR.

## Quando Usar

- Implementar CQRS com separação clara de Command/Query/Event
- Pipeline de behaviors (validação, logging, cache, transações, eventos)
- Auto-registro de handlers via Scrutor
- Eventos domain-driven (IEventSource → IEventHandler)

## Setup

### 1. Referenciar o projeto

```xml
<ItemGroup>
  <ProjectReference Include="..\FlowCore\FlowCore\FlowCore.csproj" />
</ItemGroup>
```

### 2. Configurar no DI (`Program.cs`)

```csharp
using FlowCore;

// Registro básico — scan de todos os assemblies do AppDomain
builder.Services.AddFlowCore();

// Ou limitar escopo de scan a assemblies específicos
builder.Services.AddFlowCore(typeof(Program).Assembly, typeof(DomainAssembly).Assembly);
```

### 3. Configurar dependências necessárias

```csharp
// EF Core (obrigatório para TransactionScopeBehavior)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

// FluentValidation (obrigatório para ValidationBehavior)
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// Logging
builder.Services.AddLogging();

// Cache (obrigatório implementar para CachingBehavior funcionar)
builder.Services.AddSingleton<ICacheProvider, MemoryCacheProvider>();
```

## API Principal (`IFlowMediator`)

### Commands

```csharp
// Interface do command
public record CreateUserCommand(string Name, string Email) : ICommand<Guid>;

// Handler
public class CreateUserCommandHandler : ICommandHandler<CreateUserCommand, Guid>
{
    private readonly AppDbContext _context;

    public CreateUserCommandHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> HandleAsync(CreateUserCommand command, CancellationToken ct)
    {
        var user = new User { Id = Guid.NewGuid(), Name = command.Name, Email = command.Email };
        _context.Users.Add(user);
        await _context.SaveChangesAsync(ct);
        return user.Id;
    }
}

// Validação com FluentValidation
public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
    }
}

// Uso
var userId = await _mediator.SendAsync(new CreateUserCommand("João", "joao@email.com"));
```

### Commands sem retorno

Use `Unit` para commands que não retornam valor:

```csharp
using FlowCore.Core;

public record DeactivateUserCommand(Guid UserId) : ICommand<Unit>;

public class DeactivateUserCommandHandler : ICommandHandler<DeactivateUserCommand, Unit>
{
    public async Task<Unit> HandleAsync(DeactivateUserCommand command, CancellationToken ct)
    {
        // ... lógica ...
        return Unit.Value;
    }
}
```

### Queries

```csharp
public record GetUserByIdQuery(Guid Id) : IQuery<UserDto>;

public class GetUserByIdQueryHandler : IQueryHandler<GetUserByIdQuery, UserDto>
{
    private readonly AppDbContext _context;

    public GetUserByIdQueryHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<UserDto> HandleAsync(GetUserByIdQuery query, CancellationToken ct)
    {
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == query.Id, ct);

        return new UserDto { Id = user.Id, Name = user.Name, Email = user.Email };
    }
}

// Uso
var user = await _mediator.QueryAsync(new GetUserByIdQuery(userId));
```

### Queries com Cache

Implemente `ICachableQuery<TResult>`:

```csharp
public record GetProductByIdQuery(Guid Id) : IQuery<ProductDto>, ICachableQuery<ProductDto>
{
    public string CacheKey => $"product:{Id}";
    public TimeSpan? Expiration => TimeSpan.FromMinutes(10);
}
```

O `CachingBehavior` verifica automaticamente se a query implementa `ICachableQuery` e usa o cache antes de executar o handler.

### Eventos

#### Publicação manual

```csharp
public record OrderCreatedEvent(Guid OrderId) : IEvent;

public class OrderCreatedEventHandler : IEventHandler<OrderCreatedEvent>
{
    public Task HandleAsync(OrderCreatedEvent @event, CancellationToken ct)
    {
        Console.WriteLine($"Pedido criado: {@event.OrderId}");
        return Task.CompletedTask;
    }
}

// Uso
await _mediator.PublishAsync(new OrderCreatedEvent(orderId));
```

#### Eventos automáticos via IEventSource

O handler pode gerar eventos que são despachados automaticamente pelo `EventDispatcherBehavior`:

```csharp
public record CreateUserCommand(string Name, string Email) : ICommand<Guid>, IEventSource
{
    public IEnumerable<IEvent> Events { get; private set; } = Array.Empty<IEvent>();

    public class Handler : ICommandHandler<CreateUserCommand, Guid>
    {
        public async Task<Guid> HandleAsync(CreateUserCommand command, CancellationToken ct)
        {
            var userId = Guid.NewGuid();
            // ... salvar no banco ...

            // Define eventos que serão disparados após o handler
            command.Events = new IEvent[] { new UserCreatedEvent(userId) };
            return userId;
        }
    }
}
```

## Pipeline Behaviors (registrados por padrão)

| Ordem | Behavior | Função |
|-------|----------|--------|
| 1 | `LoggingBehavior` | Log entrada/saída com tempo de execução |
| 2 | `ValidationBehavior` | Valida com FluentValidation — lança `ValidationException` |
| 3 | `CachingBehavior` | Cache para queries que implementam `ICachableQuery` |
| 4 | `TransactionScopeBehavior` | Transação EF Core com commit/rollback automático |
| 5 | `EventDispatcherBehavior` | Despacha eventos de `IEventSource` após handler |

### Fluxo de execução

```
SendAsync(command)
  → LoggingBehavior (log entrada)
  → ValidationBehavior (valida)
  → CachingBehavior (skip para commands)
  → TransactionScopeBehavior (begin transaction)
  → EventDispatcherBehavior (executa handler → extrai eventos → dispatch)
  → ICommandHandler.HandleAsync() ← execução real
  → LoggingBehavior (log saída + tempo)
```

## Implementar ICacheProvider

O FlowCore não fornece implementação padrão. Exemplo com `MemoryCache`:

```csharp
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using FlowCore.Core.Interfaces;

public class MemoryCacheProvider : ICacheProvider
{
    private readonly IMemoryCache _cache;

    public MemoryCacheProvider(IMemoryCache cache)
    {
        _cache = cache;
    }

    public Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
    {
        _cache.TryGetValue(key, out T? value);
        return Task.FromResult(value);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken ct = default)
    {
        var options = new MemoryCacheEntryOptions();
        if (expiration.HasValue)
            options.AbsoluteExpirationRelativeToNow = expiration.Value;

        _cache.Set(key, value, options);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key, CancellationToken ct = default)
    {
        _cache.Remove(key);
        return Task.CompletedTask;
    }
}
```

## Custom Behavior

```csharp
using FlowCore.Core.Interfaces;

public class AuthorizationBehavior<TRequest, TResult> : IPipelineBehavior<TRequest, TResult>
{
    public async Task<TResult> Handle(
        TRequest request,
        RequestHandlerDelegate<TResult> next,
        CancellationToken cancellationToken)
    {
        // Lógica de autorização antes do handler
        // if (!IsAuthorized(request)) throw new UnauthorizedAccessException();

        return await next();
    }
}

// Registrar no DI (adiciona ao pipeline)
services.AddScoped(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehavior<,>));
```

## Pontos de Atenção

- **Autorização removida**: `IAuthorizationRule` foi removido do projeto mas ainda aparece no README. Não use.
- **ICacheProvider**: obrigatório implementar — sem isso, `CachingBehavior` silently skip.
- **Transações**: `TransactionScopeBehavior` usa transações separadas por DbContext. Não suporta two-phase commit para bancos diferentes.
- **EventDispatcherBehavior**: usa `dynamic` — se o handler não estiver registrado, lança exceção em runtime.
- **Scrutor**: auto-registro depende do namespace `Scrutor` estar disponível (via global usings ou import explícito).
- **Nullable**: `CachingBehavior` verifica `cached != null` — pode falhar para tipos valor não-nullable.
- **Behaviors em ordem inversa**: o pipeline encadeia behaviors em ordem reversa (`Reverse()`), então o último registrado é o primeiro a executar.
