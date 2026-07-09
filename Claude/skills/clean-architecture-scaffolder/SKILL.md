---
name: clean-architecture-scaffolder
description: Scaffold a .NET solution using Clean Architecture or Vertical Slice Architecture. Use this skill whenever the user wants to start a new .NET project/solution, structure an existing one, add a new feature/use-case/slice, set up layers (Domain, Application, Infrastructure, API), wire up CQRS/MediatR, or asks "how should I structure my .NET project" / "scaffold a clean architecture solution". Always use this skill for .NET project-structure and scaffolding requests; it generates a consistent folder layout, project references, and a working sample feature rather than prose advice.
---

# Clean / Vertical Slice Architecture Scaffolder

Generate a consistent, opinionated .NET solution structure — either Clean Architecture (layered) or Vertical Slice Architecture (feature-first) — with project references wired correctly and one working sample feature so the pattern is obvious.

## Input — point it at your project

For a new solution, just take the name. For an **existing** project, point this at the repo so it matches the house style instead of imposing one:

- **A project or repo root** — detect the current architecture and conventions, then scaffold a new slice/feature that fits.
- **A GitHub URL** — clone first, then inspect: `git clone --depth 1 <url> /tmp/repo && cd /tmp/repo`

Detect the existing style yourself before generating, e.g.:
```
find <target> -name "*.sln" -o -name "*.csproj" | grep -vi "/obj/\|/bin/"
ls <target>/**/Features 2>/dev/null   # Vertical Slice?  vs  Domain/Application/Infrastructure folders = Clean
grep -rln "ICarterModule\|IRequestHandler\|MediatR\|FluentValidation\|OneOf" --include=*.cs <target> | head
```
When adding to an existing codebase, **read one existing feature first** and mirror its conventions (endpoint style, validation, result type) so the new code looks like it belongs.

## When this runs

The user is starting a new .NET solution, restructuring one, or adding a feature to an existing one. For new work, decide the style first (ask if unclear). For existing projects, detect the style from the code and match it:

- **Clean Architecture** — layered (Domain / Application / Infrastructure / API). Good for larger teams, strong boundaries, swappable infrastructure.
- **Vertical Slice Architecture** — organized by feature, each slice self-contained. Good for fast-moving teams, less ceremony, fewer cross-layer jumps.

Also confirm: project name, .NET version (default 8/9), and whether they want CQRS/MediatR (default yes for both styles).

## Output: shell commands + key files

Produce (a) the `dotnet` CLI commands to create projects and references, and (b) the contents of the key files. The user should be able to run the commands and paste the files and have a building solution.

## Clean Architecture layout

```
src/
├── Acme.Domain/            # entities, value objects, domain events — NO dependencies
├── Acme.Application/       # use cases (CQRS handlers), interfaces, DTOs — depends on Domain
├── Acme.Infrastructure/    # EF Core, external services — depends on Application
└── Acme.Api/               # endpoints, DI, middleware — depends on Application + Infrastructure
tests/
├── Acme.Domain.Tests/
├── Acme.Application.Tests/
└── Acme.Api.Tests/
```

**The dependency rule:** dependencies point inward. Domain depends on nothing. Application depends on Domain. Infrastructure and Api depend inward. Application defines interfaces (e.g. `IOrderRepository`); Infrastructure implements them.

### Project creation

```bash
dotnet new sln -n Acme

dotnet new classlib -n Acme.Domain        -o src/Acme.Domain
dotnet new classlib -n Acme.Application    -o src/Acme.Application
dotnet new classlib -n Acme.Infrastructure -o src/Acme.Infrastructure
dotnet new webapi    -n Acme.Api           -o src/Acme.Api --use-minimal-apis

dotnet sln add src/**/*.csproj

dotnet add src/Acme.Application/Acme.Application.csproj reference src/Acme.Domain/Acme.Domain.csproj
dotnet add src/Acme.Infrastructure/Acme.Infrastructure.csproj reference src/Acme.Application/Acme.Application.csproj
dotnet add src/Acme.Api/Acme.Api.csproj reference src/Acme.Application/Acme.Application.csproj
dotnet add src/Acme.Api/Acme.Api.csproj reference src/Acme.Infrastructure/Acme.Infrastructure.csproj
```

## Vertical Slice layout

```
src/Acme.Api/
├── Features/
│   ├── Orders/
│   │   ├── CreateOrder.cs        # request, handler, validator, endpoint — all in one file
│   │   ├── GetOrder.cs
│   │   └── Order.cs              # the entity (or shared in Domain)
│   └── Customers/
│       └── ...
├── Common/                       # cross-cutting: behaviors, results, persistence
└── Program.cs
```

Each slice owns its request, handler, validation, and endpoint. Shared concerns (DbContext, pipeline behaviors, Result type) live in `Common/`. A slice rarely touches another slice.

## Sample feature (CQRS, both styles)

A "Create Order" command handler the user can copy as the template for every feature:

```csharp
// Command + Result
public record CreateOrderCommand(Guid CustomerId, List<OrderLine> Lines) : IRequest<Result<Guid>>;

// Handler
public class CreateOrderHandler(IOrderRepository repo, IUnitOfWork uow)
    : IRequestHandler<CreateOrderCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateOrderCommand cmd, CancellationToken ct)
    {
        var order = Order.Create(cmd.CustomerId, cmd.Lines);
        if (order.IsFailure)
            return Result.Failure<Guid>(order.Error);

        await repo.AddAsync(order.Value, ct);
        await uow.SaveChangesAsync(ct);
        return Result.Success(order.Value.Id);
    }
}

// Validator (FluentValidation)
public class CreateOrderValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.Lines).NotEmpty();
    }
}

// Minimal API endpoint
app.MapPost("/orders", async (CreateOrderCommand cmd, ISender sender) =>
{
    var result = await sender.Send(cmd);
    return result.IsSuccess
        ? Results.Created($"/orders/{result.Value}", result.Value)
        : Results.BadRequest(result.Error);
});
```

## Recommended building blocks (offer, don't force)

- **MediatR** — request/handler dispatch + pipeline behaviors (validation, logging, transactions).
- **FluentValidation** — wired as a MediatR pipeline behavior so validation runs before handlers.
- **Result pattern** — avoid exceptions for expected failures; return `Result<T>`.
- **NetArchTest** — architecture tests that fail the build if the dependency rule is violated (great for Clean Architecture).

## Architecture guardrail test (Clean Architecture)

Include this so the structure can't silently rot:

```csharp
[Fact]
public void Domain_should_not_depend_on_other_projects()
{
    var result = Types.InAssembly(typeof(Order).Assembly)
        .Should()
        .NotHaveDependencyOnAny("Acme.Application", "Acme.Infrastructure", "Acme.Api")
        .GetResult();

    Assert.True(result.IsSuccessful, string.Join(", ", result.FailingTypeNames ?? new List<string>()));
}
```

## Principles

- Match the style to the team, not the trend. Vertical Slice for speed and small teams; Clean for strong boundaries and scale. Don't over-engineer a CRUD app into 4 layers.
- Generate one *working* sample feature — a pattern people can copy beats a diagram.
- Wire the dependency rule into a test so it's enforced, not just documented.
- Keep Domain free of framework dependencies (no EF attributes leaking into entities — use Fluent configuration).
