---
stepsCompleted: ['step-01-init', 'step-02-context', 'step-03-starter', 'step-04-decisions', 'step-05-patterns', 'step-06-structure', 'step-07-validation', 'step-08-complete']
inputDocuments: ['_bmad-output/planning-artifacts/prd.md', '_bmad-output/planning-artifacts/product-brief-funimmo_bmad-2026-02-22.md']
workflowType: 'architecture'
workflowStatus: 'complete'
completedAt: '2026-02-22'
project_name: 'ImmoPilot'
user_name: 'Philippe'
date: '2026-02-22'
---

# Architecture Decision Document

_Ce document se construit collaborativement étape par étape. Les sections sont ajoutées au fil des décisions architecturales._

## Project Context Analysis

### Requirements Overview

**Functional Requirements :** 30 FRs organisées en 6 domaines de capacité — Property Discovery, Financial Qualification, Data Persistence & Deduplication, Notification & Alerting, Configuration Management, Pipeline Operations. Aucun epic/UX spec — CLI pipeline non-interactif.

**Non-Functional Requirements critiques pour l'architecture :**

- Performance : ≤ 30 min d'exécution totale, 10s timeout par appel API, digest avant 8h EST
- Reliability : ≥ 99% uptime, retry 3x backoff exponentiel (1s/2s/4s) sur tous les clients HTTP, panne partielle non-bloquante
- Idempotence : relancer le même jour ne re-notifie pas — déduplication Azure SQL par MLS ID + date
- Testabilité : ≥ 80% couverture tests unitaires sur `PropertyAnalyzer`, BDD SpecFlow/Reqnroll
- Security : `appsettings.json` gitignored, secrets via GitHub Secrets

**Scale & Complexity :**

- Complexité : Moyenne — pipeline backend séquentiel, 4 intégrations externes, pas de concurrence ni multitenancy
- Domaine technique : CLI Backend Pipeline (Generic Host, `dotnet run`, GitHub Actions)
- Composants architecturaux estimés : 7–8
- Contrainte runtime : GitHub Actions exit codes 0/1/2, `dotnet run --dry-run`

### Architectural Pattern : Clean Architecture

**Décision :** Clean Architecture à 4 couches — choisie pour la testabilité, l'évolutivité (v1.1 Facebook, fallback SQLite), et l'isolation du moteur DSCR.

```
┌─────────────────────────────────────────────────────────┐
│  Entry Point  │  Program.cs · Generic Host · DI wiring  │
├─────────────────────────────────────────────────────────┤
│ Infrastructure│  RedfinClient · HudFmrClient             │
│               │  PropertyRepository (Azure SQL)          │
│               │  WhatsAppNotifier · NullWhatsAppNotifier  │
│               │  SerilogLogger                           │
├─────────────────────────────────────────────────────────┤
│  Application  │  PipelineOrchestrator (use case)         │
│               │  IPropertySource · IHudFmrClient         │
│               │  IPropertyRepository · IWhatsAppNotifier │
│               │  ILogger (abstraction)                   │
├─────────────────────────────────────────────────────────┤
│    Domain     │  PropertyAnalyzer (logique DSCR pure)    │
│  (0 dépend.)  │  Property · DscrResult · FmrData         │
│               │  DscrThresholds · MarketConfig           │
└─────────────────────────────────────────────────────────┘
```

**Règle de dépendance :** Domain ← Application ← Infrastructure / Entry Point. Jamais l'inverse.

### Technical Constraints & Dependencies

| Contrainte | Implication architecturale |
|------------|---------------------------|
| Generic Host (pas ASP.NET) | `IHostBuilder` + `IHostedService` ou console app simple avec DI manuel |
| GitHub Actions exit codes | `PipelineOrchestrator` retourne un enum `PipelineResult` → `Program.cs` fait `Environment.Exit()` |
| Azure SQL Basic ~$5/mo | Schéma minimal, EF Core + LINQ + EF Migrations |
| Redfin API non-officielle | `IPropertySource` en Application → swap facile vers Zillow/Facebook en Infrastructure |
| `appsettings.json` non commité | Options pattern C# (`IOptions<T>`) avec `appsettings.Example.json` commité |
| `--dry-run` flag | `NullWhatsAppNotifier : IWhatsAppNotifier` injecté en mode dry-run — même orchestrateur |

### Cross-Cutting Concerns Identified

- **Retry/Resilience** : Polly (`Microsoft.Extensions.Http.Resilience` .NET 10) — appliqué au niveau `IHttpClientFactory` dans Infrastructure, transparent pour Application
- **Structured Logging** : Serilog avec Run ID injected via `LogContext.PushProperty` — toutes les couches logguent via `ILogger<T>`
- **Configuration** : Options pattern — `PipelineOptions`, `MarketOptions`, `DscrThresholdOptions`, `NotificationOptions`, `ApiOptions` mappés depuis `appsettings.json`
- **Idempotence** : `PropertyRepository.IsAlreadyNotified(mlsId, date)` — vérification avant envoi, pas après
- **Partial failure handling** : `PipelineOrchestrator` accumule les erreurs par propriété, continue, détermine exit code 0/1/2 à la fin selon le taux d'échec

## Starter Template Evaluation

### Primary Technology Domain

CLI Backend Pipeline — C# .NET défini par le PRD. Pas de starter tiers requis : le SDK .NET fournit tous les templates nécessaires.

### Selected Starter : Solution multi-projets .NET 10 LTS

**Rationale :** Clean Architecture impose plusieurs projets avec des règles de dépendance strictes — une solution multi-projets est l'approche standard en C#. .NET 10 LTS est choisi pour la stabilité en production (support jusqu'à novembre 2028).

**Commandes d'initialisation :**

```bash
dotnet new sln -n ImmoPilot
dotnet new classlib -n ImmoPilot.Domain       -o src/ImmoPilot.Domain
dotnet new classlib -n ImmoPilot.Application   -o src/ImmoPilot.Application
dotnet new classlib -n ImmoPilot.Infrastructure -o src/ImmoPilot.Infrastructure
dotnet new console  -n ImmoPilot.Console        -o src/ImmoPilot.Console
dotnet new xunit    -n ImmoPilot.Domain.Tests        -o tests/ImmoPilot.Domain.Tests
dotnet new xunit    -n ImmoPilot.Application.Tests   -o tests/ImmoPilot.Application.Tests
dotnet new xunit    -n ImmoPilot.BDD.Tests           -o tests/ImmoPilot.BDD.Tests
dotnet sln add src/**/*.csproj tests/**/*.csproj
```

**Structure de solution :**

```
ImmoPilot.sln
├── src/
│   ├── ImmoPilot.Domain/           # 0 dépendances NuGet — logique pure
│   ├── ImmoPilot.Application/      # dépend de Domain uniquement
│   ├── ImmoPilot.Infrastructure/   # dépend de Application (via interfaces)
│   └── ImmoPilot.Console/          # entry point — dépend de tous
└── tests/
    ├── ImmoPilot.Domain.Tests/     # xUnit — PropertyAnalyzer ≥80% coverage
    ├── ImmoPilot.Application.Tests/ # xUnit + mocks
    └── ImmoPilot.BDD.Tests/        # Reqnroll (SpecFlow .NET 8) — scénarios Gherkin
```

**Packages NuGet par couche :**

| Projet | Packages |
|--------|---------|
| `Domain` | aucun |
| `Application` | `Microsoft.Extensions.Logging.Abstractions` |
| `Infrastructure` | `Microsoft.EntityFrameworkCore.SqlServer` · `Microsoft.EntityFrameworkCore.Tools` · `Microsoft.Extensions.Http.Resilience` · `Serilog.Extensions.Hosting` · `Serilog.Sinks.Console` · `Twilio` |
| `Console` | `Microsoft.Extensions.Hosting` · `Microsoft.Extensions.Configuration.Json` |
| `*.Tests` | `xunit` · `NSubstitute` · `FluentAssertions` |
| `BDD.Tests` | `Reqnroll.xUnit` · `NSubstitute` |

**Note :** La création de la solution multi-projets avec références croisées sera la première story d'implémentation.

## Core Architectural Decisions

### Decision Priority Analysis

**Décisions critiques (bloquent l'implémentation) :**
- ORM + migrations : EF Core + LINQ + EF Migrations
- Error handling pattern : `Result<T>` custom
- WhatsApp provider : Twilio
- HttpClient management : `IHttpClientFactory` typed clients + Polly

**Décisions importantes (structurent l'architecture) :**
- Clean Architecture 4 couches avec règle de dépendance stricte
- Generic Host (sans ASP.NET Core)
- Options pattern pour toute la configuration

**Décisions différées (post-MVP) :**
- Fallback SQLite (migration en 1h si Azure SQL change de pricing — swap Infrastructure uniquement)
- Facebook Marketplace client (nouveau `IPropertySource` en Infrastructure, aucun changement Domain/Application)

### Data Architecture

**ORM :** EF Core 10 + LINQ + EF Migrations

- `DbContext` (`ImmoPilotDbContext`) dans `Infrastructure` — jamais exposé en dehors
- Entités EF (`PropertyEntity`, `NotificationLogEntity`) distinctes des entités Domain (`Property`, `DscrResult`) — mapping dans Repository
- Migration auto au démarrage : `await dbContext.Database.MigrateAsync()` dans `Program.cs`
- Schéma minimal — 2 tables :

```sql
Properties     : Id, MlsId, Address, City, Price, CapRate, CocRate,
                 CashFlow, CashRequired, FmrRent, FmrYear, RehabPercent,
                 DscrStatus, FmrStatus, ScannedAt
NotificationLog: Id, PropertyId, SentAt, DigestDate, Recipients
```

**Rationale EF Core vs Dapper :** LINQ type-safe + migrations intégrées + productivité supérieure pour un schéma simple sur un pipeline nightly. Performance non-critique (50 propriétés/nuit).

### Security

- Aucune authentification requise — CLI pipeline interne, pas d'endpoint web exposé
- `appsettings.json` et `appsettings.Development.json` dans `.gitignore`
- `appsettings.Example.json` commité (valeurs placeholder) comme référence de structure
- Secrets en CI/CD : GitHub Secrets → variables d'environnement injectées dans GitHub Actions runner
- Connection string Azure SQL et Twilio Auth Token jamais en clair dans le code ou le repo

### API & Communication Patterns

**Error handling : `Result<T>` custom**

```csharp
// Domain/Common/Result.cs — record simple, 0 dépendance externe
public record Result<T>(bool IsSuccess, T? Value, string? Error)
{
    public static Result<T> Ok(T value) => new(true, value, null);
    public static Result<T> Fail(string error) => new(false, default, error);
}
```

Chaque méthode de service retourne `Result<T>` — `PipelineOrchestrator` inspecte les résultats pour déterminer l'exit code final (0/1/2) sans relancer d'exceptions.

**Clients HTTP : Typed clients avec Polly**

- `RedfinHttpClient` : `User-Agent` configuré, timeout 10s, politique retry 3x backoff (1s/2s/4s)
- `HudFmrHttpClient` : timeout 10s, politique retry 3x backoff
- Les deux enregistrés via `IHttpClientFactory` dans `Console/Program.cs` — invisibles en Application

**WhatsApp : Twilio SDK .NET**

- `TwilioWhatsAppNotifier : IWhatsAppNotifier` dans Infrastructure
- `NullWhatsAppNotifier : IWhatsAppNotifier` injecté automatiquement quand `DryRun: true`
- `AccountSid`, `AuthToken`, `FromNumber` via `IOptions<NotificationOptions>` (GitHub Secrets)

### Infrastructure & Deployment

**CI/CD : GitHub Actions**

- `schedule: cron: "0 4 * * *"` — exécution nightly 04:00 UTC
- `workflow_dispatch` — déclenchement manuel depuis l'interface GitHub
- Steps : `dotnet restore` → `dotnet build` → `dotnet test` → `dotnet run`
- Exit codes propagés : `if: failure()` sur le step `dotnet run` → alerte WhatsApp via `PipelineOrchestrator`
- Pas de Docker — runner GitHub Actions Ubuntu hosted suffit pour `dotnet run`

**Environment configuration :**
- `appsettings.json` injecté via GitHub Secret `APPSETTINGS_JSON` → fichier créé dynamiquement dans le workflow
- Ou variables individuelles mappées via `IConfiguration` environment variables override

### Decision Impact Analysis

**Séquence d'implémentation dictée par les décisions :**

1. Créer la solution multi-projets + références croisées
2. Définir les entités Domain (`Property`, `DscrResult`, `FmrData`, `Result<T>`)
3. Créer les interfaces Application (`IPropertySource`, `IHudFmrClient`, `IPropertyRepository`, `IWhatsAppNotifier`)
4. Implémenter `PropertyAnalyzer` (Domain pur) + tests unitaires ≥80%
5. Créer `ImmoPilotDbContext` + EF Migrations (`Properties`, `NotificationLog`)
6. Implémenter `RedfinHttpClient`, `HudFmrHttpClient` (Infrastructure)
7. Implémenter `PropertyRepository` EF Core (Infrastructure)
8. Implémenter `TwilioWhatsAppNotifier` + `NullWhatsAppNotifier` (Infrastructure)
9. Implémenter `PipelineOrchestrator` (Application) + exit codes
10. Câbler `Program.cs` + GitHub Actions workflow YAML

**Dépendances croisées clés :**
- `PropertyAnalyzer` (Domain) est le seul composant sans dépendance — peut être développé et testé en premier
- `PipelineOrchestrator` dépend de toutes les interfaces — implémenté en dernier côté Application
- EF Migrations nécessite `ImmoPilotDbContext` finalisé avant la première migration

## Implementation Patterns & Consistency Rules

### Naming Patterns

**Namespaces — règle stricte par couche :**

| Projet | Namespace racine | Exemples |
|--------|----------------|---------|
| Domain | `ImmoPilot.Domain` | `ImmoPilot.Domain.Entities`, `ImmoPilot.Domain.Common` |
| Application | `ImmoPilot.Application` | `ImmoPilot.Application.Interfaces`, `ImmoPilot.Application.Services` |
| Infrastructure | `ImmoPilot.Infrastructure` | `ImmoPilot.Infrastructure.Http`, `ImmoPilot.Infrastructure.Persistence` |
| Console | `ImmoPilot.Console` | |

**Nommage des classes :**

- Entités Domain : nom nu — `Property`, `DscrResult`, `FmrData`
- Entités EF (Infrastructure) : suffixe `Entity` — `PropertyEntity`, `NotificationLogEntity`
- Interfaces Application : préfixe `I` — `IPropertyRepository`, `IWhatsAppNotifier`
- Implémentations Infrastructure : nom descriptif — `EfPropertyRepository`, `TwilioWhatsAppNotifier`, `NullWhatsAppNotifier`
- Options (configuration) : suffixe `Options` — `PipelineOptions`, `DscrThresholdOptions`, `NotificationOptions`
- Clients HTTP : suffixe `Client` — `RedfinHttpClient`, `HudFmrHttpClient`
- Target framework : `net10.0` dans tous les `.csproj`

**Nommage des méthodes :**

- Repository : `GetByMlsIdAsync`, `SaveAsync`, `IsAlreadyNotifiedAsync`, `GetUnnotifiedQualifiedAsync`
- Clients HTTP : `FetchListingsAsync`, `GetFmrByZipAsync`
- Suffixe `Async` obligatoire sur toutes les méthodes async
- Préfixes standards : `Get` (lecture), `Save`/`Add` (écriture), `Is`/`Has` (booléen), `Fetch` (appel HTTP)

**Base de données (EF Core 10) :**

- Tables : PascalCase pluriel — `Properties`, `NotificationLogs`
- Colonnes : PascalCase — `MlsId`, `CapRate`, `ScannedAt`
- Clé primaire : toujours `Id` (Guid)
- Clés étrangères : `{NomEntité}Id` — `PropertyId`

### Structure Patterns

**Organisation intra-projet :**

```
ImmoPilot.Domain/
├── Common/         # Result<T>, exceptions métier
├── Entities/       # Property, DscrResult, FmrData
└── ValueObjects/   # DscrThresholds, MarketConfig

ImmoPilot.Application/
├── Interfaces/     # IPropertySource, IHudFmrClient, IPropertyRepository, IWhatsAppNotifier
├── Services/       # PipelineOrchestrator
└── DTOs/           # PropertyDto (mapping Domain ↔ Infrastructure si besoin)

ImmoPilot.Infrastructure/
├── Http/           # RedfinHttpClient, HudFmrHttpClient
├── Persistence/    # ImmoPilotDbContext, PropertyEntity, EfPropertyRepository, Migrations/
└── Notifications/  # TwilioWhatsAppNotifier, NullWhatsAppNotifier

ImmoPilot.Console/
├── Program.cs      # DI wiring, Generic Host, exit codes
└── appsettings.Example.json
```

**Tests :**

```
tests/ImmoPilot.Domain.Tests/
├── PropertyAnalyzerTests.cs      # un fichier par classe testée
└── Fixtures/

tests/ImmoPilot.BDD.Tests/
├── Features/                     # fichiers .feature Gherkin
│   ├── DscrQualification.feature
│   └── PipelineResilience.feature
└── StepDefinitions/
```

### Format Patterns

**`Result<T>` — usage obligatoire :**

```csharp
// Domain/Common/Result.cs — 0 dépendance externe
public record Result<T>(bool IsSuccess, T? Value, string? Error)
{
    public static Result<T> Ok(T value) => new(true, value, null);
    public static Result<T> Fail(string error) => new(false, default, error);
}

// CORRECT — toute méthode de service/repository retourne Result<T>
public async Task<Result<IEnumerable<Property>>> FetchListingsAsync(MarketConfig config);
public async Task<Result<FmrData>> GetFmrByZipAsync(string zip);

// INTERDIT — throw pour les cas métier attendus
throw new FmrNotFoundException(zip); // ❌ → utiliser Result.Fail("FMR_UNAVAILABLE: {zip}")
```

**Statuts de propriété — enums Domain :**

```csharp
public enum DscrStatus  { Qualified, Warning, Rejected }
public enum FmrStatus   { Available, Unavailable }
public enum ListingStatus { Active, Pending }
```

**Format de log structuré — obligatoire :**

```
[{YYYY-MM-DD HH:mm:ss} UTC] {LEVEL}  {Message}
[2026-02-22 04:02:11 UTC] INFO  Pipeline started — Run #248
[2026-02-22 04:03:41 UTC] WARN  HUD FMR: ZIP 45502 not found — property marked FMR_UNAVAILABLE
[2026-02-22 04:05:19 UTC] INFO  Pipeline completed — Duration: 3m08s — Exit: 0
```

Run ID injecté systématiquement via `LogContext.PushProperty("RunId", runId)` au démarrage.

### Process Patterns

**Gestion des pannes partielles dans `PipelineOrchestrator` :**

```
Pour chaque propriété :
  1. FetchFmr → si FMR_UNAVAILABLE → marquer FmrStatus.Unavailable, CONTINUER
  2. CalculateDscr → si données incomplètes → DscrStatus.Rejected, CONTINUER
  Aucune propriété ne bloque le pipeline

Détermination exit code final :
  0 : Redfin OK + WhatsApp envoyé (même si certains FMR manquants)
  1 : Succès partiel (≥1 FMR_UNAVAILABLE, WhatsApp envoyé)
  2 : Redfin inaccessible après 3 retries, OU WhatsApp non envoyé
```

**Retry Polly (.NET 10) — pattern standardisé :**

```csharp
// Même politique sur tous les clients HTTP — définie une seule fois en Program.cs
services.AddHttpClient<RedfinHttpClient>()
        .AddStandardResilienceHandler(); // .NET 10 — retry configurable via Options
```

**Dry-run — injection conditionnelle dans Program.cs uniquement :**

```csharp
if (pipelineOptions.DryRun)
    services.AddSingleton<IWhatsAppNotifier, NullWhatsAppNotifier>();
else
    services.AddSingleton<IWhatsAppNotifier, TwilioWhatsAppNotifier>();
```

### Enforcement Guidelines

**Tout agent IA DOIT :**

- Respecter la règle de dépendance Clean Architecture — Domain zéro dépendance NuGet
- Retourner `Result<T>` sur toutes les méthodes de service/repository/client HTTP
- Utiliser le suffixe `Async` sur toutes les méthodes async
- Nommer les entités EF avec suffixe `Entity` pour éviter les collisions avec les entités Domain
- Utiliser les statuts comme enums Domain, jamais des strings libres
- Logger via `ILogger<T>`, jamais `Console.WriteLine`
- Cibler `net10.0` dans tous les `.csproj`

**Anti-patterns à éviter :**

```csharp
// ❌ Entité Domain confondue avec entité EF
public class Property { } // dans Infrastructure/Persistence → collision avec Domain

// ❌ String magique pour les statuts
property.Status = "FMR_UNAVAILABLE"; // ❌ → utiliser FmrStatus.Unavailable

// ❌ throw pour cas métier attendus
throw new NoPropertiesFoundException(); // ❌ → utiliser Result.Fail(...)

// ❌ Logging direct
Console.WriteLine("Pipeline started"); // ❌ → utiliser _logger.LogInformation(...)
```

## Project Structure & Boundaries

### Complete Project Directory Structure

```
ImmoPilot/
├── ImmoPilot.sln
├── README.md
├── .gitignore                           # appsettings.json, appsettings.Development.json, bin/, obj/
├── .github/
│   └── workflows/
│       └── nightly.yml                  # cron 04:00 UTC + workflow_dispatch
│
├── src/
│   ├── ImmoPilot.Domain/                # 0 dépendances NuGet
│   │   ├── ImmoPilot.Domain.csproj
│   │   ├── Common/
│   │   │   └── Result.cs                # Result<T> record
│   │   ├── Entities/
│   │   │   ├── Property.cs              # entité centrale (MlsId, Price, DscrStatus, FmrStatus…)
│   │   │   ├── DscrResult.cs            # résultat de calcul DSCR
│   │   │   └── FmrData.cs               # données HUD FMR par ZIP
│   │   └── ValueObjects/
│   │       ├── DscrThresholds.cs        # seuils DSCR (min ratio, warning ratio)
│   │       └── MarketConfig.cs          # ZIP cible, ville, paramètres marché
│   │
│   ├── ImmoPilot.Application/           # dépend de Domain uniquement
│   │   ├── ImmoPilot.Application.csproj
│   │   ├── Interfaces/
│   │   │   ├── IPropertySource.cs       # FetchListingsAsync(MarketConfig) → Result<IEnumerable<Property>>
│   │   │   ├── IHudFmrClient.cs         # GetFmrByZipAsync(zip) → Result<FmrData>
│   │   │   ├── IPropertyRepository.cs   # SaveAsync, IsAlreadyNotifiedAsync, GetUnnotifiedQualifiedAsync
│   │   │   └── IWhatsAppNotifier.cs     # SendDigestAsync(properties, runId) → Result<bool>
│   │   ├── Services/
│   │   │   └── PipelineOrchestrator.cs  # use case principal → retourne PipelineResult
│   │   └── DTOs/
│   │       └── PropertyDto.cs           # mapping Domain ↔ Infrastructure si nécessaire
│   │
│   ├── ImmoPilot.Infrastructure/        # dépend de Application (via interfaces)
│   │   ├── ImmoPilot.Infrastructure.csproj
│   │   ├── Http/
│   │   │   ├── RedfinHttpClient.cs      # IPropertySource — scraping non-officiel, User-Agent configuré
│   │   │   └── HudFmrHttpClient.cs      # IHudFmrClient — API officielle HUD
│   │   ├── Persistence/
│   │   │   ├── ImmoPilotDbContext.cs    # EF Core DbContext
│   │   │   ├── Entities/
│   │   │   │   ├── PropertyEntity.cs    # entité EF (≠ Domain.Property)
│   │   │   │   └── NotificationLogEntity.cs
│   │   │   ├── EfPropertyRepository.cs  # IPropertyRepository — EF Core + LINQ
│   │   │   └── Migrations/              # générées par `dotnet ef migrations add`
│   │   └── Notifications/
│   │       ├── TwilioWhatsAppNotifier.cs  # IWhatsAppNotifier — Twilio SDK
│   │       └── NullWhatsAppNotifier.cs    # IWhatsAppNotifier — no-op pour --dry-run
│   │
│   └── ImmoPilot.Console/               # entry point — dépend de tous
│       ├── ImmoPilot.Console.csproj
│       ├── Program.cs                   # Generic Host, DI wiring, MigrateAsync, Environment.Exit
│       └── appsettings.Example.json     # structure de référence (valeurs placeholder, commité)
│
└── tests/
    ├── ImmoPilot.Domain.Tests/
    │   ├── ImmoPilot.Domain.Tests.csproj
    │   ├── PropertyAnalyzerTests.cs      # ≥80% coverage sur PropertyAnalyzer
    │   └── Fixtures/
    │       └── PropertyFixtures.cs       # builders de données de test
    │
    ├── ImmoPilot.Application.Tests/
    │   ├── ImmoPilot.Application.Tests.csproj
    │   └── Services/
    │       └── PipelineOrchestratorTests.cs  # mocks NSubstitute de toutes les interfaces
    │
    └── ImmoPilot.BDD.Tests/
        ├── ImmoPilot.BDD.Tests.csproj
        ├── Features/
        │   ├── DscrQualification.feature     # scénarios FR4-9
        │   └── PipelineResilience.feature    # scénarios FR25-30 (pannes partielles, exit codes)
        └── StepDefinitions/
            ├── DscrQualificationSteps.cs
            └── PipelineResilienceSteps.cs
```

### Architectural Boundaries

**Règle de dépendance (stricte) :**
- `Domain` ← `Application` ← `Infrastructure` ← `Console`
- Jamais l'inverse — toute violation est une erreur de compilation ou de conception
- `Application` ne référence **jamais** un namespace `ImmoPilot.Infrastructure.*`

**Boundaries de couche :**

| Couche | Reçoit | Retourne | N'accède jamais à |
|--------|--------|----------|-------------------|
| Domain | valeurs primitives / value objects | entités Domain, `Result<T>` | tout package NuGet |
| Application | entités Domain via interfaces | `PipelineResult` | `ImmoPilot.Infrastructure.*` |
| Infrastructure | entités Domain + options | entités EF internes, HTTP responses | `ImmoPilot.Application.Services.*` |
| Console | configuration, args | `Environment.Exit(code)` | logique métier directe |

### Requirements to Structure Mapping

**FR → Fichiers clés :**

| Domaine FR | Fichiers clés |
|------------|--------------|
| Property Discovery (FR1-3) | `Infrastructure/Http/RedfinHttpClient.cs` · `Application/Interfaces/IPropertySource.cs` |
| Financial Qualification (FR4-9) | `Domain/Entities/PropertyAnalyzer.cs` · `Domain/ValueObjects/DscrThresholds.cs` · `Infrastructure/Http/HudFmrHttpClient.cs` |
| Data Persistence & Deduplication (FR10-13) | `Infrastructure/Persistence/EfPropertyRepository.cs` · `ImmoPilotDbContext.cs` · `Migrations/` |
| Notification & Alerting (FR14-19) | `Infrastructure/Notifications/TwilioWhatsAppNotifier.cs` · `NullWhatsAppNotifier.cs` |
| Configuration Management (FR20-24) | `Console/Program.cs` · `appsettings.Example.json` · classes Options dans chaque couche |
| Pipeline Operations (FR25-30) | `Application/Services/PipelineOrchestrator.cs` · `.github/workflows/nightly.yml` |

**Cross-cutting concerns → Fichiers :**

| Concern | Fichier / Pattern |
|---------|------------------|
| Retry / Resilience | `Console/Program.cs` — `AddStandardResilienceHandler()` sur chaque `IHttpClientFactory` |
| Structured Logging | Toutes couches via `ILogger<T>` — `Serilog` configuré dans `Console/Program.cs` |
| Idempotence | `EfPropertyRepository.IsAlreadyNotifiedAsync()` appelé avant tout envoi |
| Dry-run | Injection conditionnelle `NullWhatsAppNotifier` dans `Console/Program.cs` |
| Exit codes | `PipelineResult` enum → `Environment.Exit()` dans `Console/Program.cs` |

### Integration Points

**Flux de données pipeline nightly :**

```
Program.cs (Console)
  └─► PipelineOrchestrator.RunAsync()
        ├─► RedfinHttpClient.FetchListingsAsync(MarketConfig)
        │     └─► Result<IEnumerable<Property>>     [FR1-3]
        │
        ├─► Pour chaque Property :
        │     ├─► HudFmrHttpClient.GetFmrByZipAsync(zip)
        │     │     └─► Result<FmrData>               [FR4]  ← FMR_UNAVAILABLE → continue
        │     ├─► PropertyAnalyzer.Analyze(property, fmrData, thresholds)
        │     │     └─► DscrResult (Qualified/Warning/Rejected)  [FR5-9]
        │     └─► EfPropertyRepository.IsAlreadyNotifiedAsync(mlsId, today)
        │           └─► bool → déduplication         [FR10-13]
        │
        ├─► EfPropertyRepository.SaveAsync(qualifiedProperties)  [FR10-13]
        │
        ├─► TwilioWhatsAppNotifier.SendDigestAsync(qualified, runId)
        │     └─► Result<bool>                        [FR14-19]
        │
        └─► PipelineResult → Environment.Exit(0/1/2) [FR25-30]
```

**Intégrations externes :**

| Service | Client | Interface | Mode dégradé |
|---------|--------|-----------|-------------|
| Redfin (non-officiel) | `RedfinHttpClient` | `IPropertySource` | Exit code 2 après 3 retries |
| HUD FMR API (officiel) | `HudFmrHttpClient` | `IHudFmrClient` | `FmrStatus.Unavailable` → pipeline continue |
| Azure SQL | `EfPropertyRepository` | `IPropertyRepository` | Exception non gérée → exit code 2 |
| Twilio WhatsApp | `TwilioWhatsAppNotifier` | `IWhatsAppNotifier` | Exit code 2 si échec envoi |

### Development Workflow Integration

**GitHub Actions (`nightly.yml`) :**

```yaml
# Structure des steps CI/CD
- dotnet restore
- dotnet build --no-restore
- dotnet test --no-build
- dotnet run --project src/ImmoPilot.Console  # exit code propagé
```

**Configuration au runtime (GitHub Actions) :**
- Secret `APPSETTINGS_JSON` → fichier `src/ImmoPilot.Console/appsettings.json` créé dynamiquement
- `appsettings.Example.json` commité sert de documentation de structure

## Architecture Validation Results

### Coherence Validation ✅

**Decision Compatibility :**
- .NET 10 + EF Core 10 + Serilog + Polly (`Microsoft.Extensions.Http.Resilience`) + Twilio SDK + xUnit + Reqnroll → compatibles sans conflit
- Generic Host + IHttpClientFactory + Options pattern → trio natif .NET, aucune friction
- Clean Architecture → règle de dépendance enforced par les project references `.csproj` (erreur de compilation si violation)

**Pattern Consistency :**
- `Result<T>` sans exception → cohérent avec `PipelineOrchestrator` qui accumule les erreurs sans propager
- `ILogger<T>` via Serilog → wrapper standard, toutes couches loggent sans connaître Serilog
- `NullWhatsAppNotifier` dry-run → Null Object Pattern, aucune duplication de logique dans l'orchestrateur

**Structure Alignment :**
- 4 projets src = 4 couches Clean Architecture → bijection stricte
- 3 projets tests = Domain + Application + BDD → couverture ciblée
- `.github/workflows/nightly.yml` = entry point CI/CD séparé du code source

### Requirements Coverage Validation ✅

**FR Coverage (30/30) :**

| Domaine | Couverture |
|---------|-----------|
| FR1-3 Property Discovery | `RedfinHttpClient` + `IPropertySource` ✅ |
| FR4-9 Financial Qualification | `PropertyAnalyzer` + `DscrThresholds` + `HudFmrHttpClient` ✅ |
| FR10-13 Persistence & Dedup | `EfPropertyRepository` + `IsAlreadyNotifiedAsync` ✅ |
| FR14-19 Notification & Alerting | `TwilioWhatsAppNotifier` + `NullWhatsAppNotifier` ✅ |
| FR20-24 Configuration Management | Options pattern + `appsettings.Example.json` ✅ |
| FR25-30 Pipeline Operations | `PipelineOrchestrator` + exit codes 0/1/2 + `nightly.yml` ✅ |

**NFR Coverage :**
- ≤30 min / 10s timeout → Polly `AddStandardResilienceHandler` ✅
- Digest avant 8h EST → `cron: "0 4 * * *"` UTC ✅
- Idempotence → `IsAlreadyNotifiedAsync` avant tout envoi ✅
- ≥80% coverage → `Domain.Tests/PropertyAnalyzerTests.cs` ciblé ✅
- BDD Reqnroll → `ImmoPilot.BDD.Tests` avec `.feature` Gherkin ✅
- Security → `.gitignore` + GitHub Secrets + jamais de secret en clair ✅

### Gap Analysis & Corrections Appliquées

**3 gaps importants identifiés et corrigés :**

1. **`PropertyAnalyzer` est un domain service** (pas une entité) → placé dans `Domain/Services/`
2. **`PipelineResult` enum non localisé** → ajouté dans `Application/Models/`
3. **Classes Options non localisées** → réparties par couche consommatrice

**Structure intra-projet corrigée (finale) :**

```
ImmoPilot.Domain/
├── Common/         # Result<T>
├── Entities/       # Property, DscrResult, FmrData       ← données uniquement
├── Services/       # PropertyAnalyzer                    ← domain service
└── ValueObjects/   # DscrThresholds, MarketConfig

ImmoPilot.Application/
├── Interfaces/     # IPropertySource, IHudFmrClient, IPropertyRepository, IWhatsAppNotifier
├── Models/         # PipelineResult (enum)
├── Options/        # PipelineOptions, MarketOptions, DscrThresholdOptions
└── Services/       # PipelineOrchestrator

ImmoPilot.Infrastructure/
├── Http/           # RedfinHttpClient, HudFmrHttpClient
├── Options/        # NotificationOptions, ApiOptions
├── Persistence/    # ImmoPilotDbContext, Entities/, EfPropertyRepository, Migrations/
└── Notifications/  # TwilioWhatsAppNotifier, NullWhatsAppNotifier

ImmoPilot.Console/
├── Program.cs      # DI wiring, Generic Host, MigrateAsync, Environment.Exit
└── appsettings.Example.json
```

### Architecture Completeness Checklist

**Requirements Analysis ✅**
- [x] Contexte projet analysé — 30 FRs, 6 domaines, NFRs critiques
- [x] Complexité évaluée — pipeline séquentiel, 4 intégrations, pas de concurrence
- [x] Contraintes techniques identifiées — exit codes, gitignore, dry-run
- [x] Cross-cutting concerns mappés — retry, logging, idempotence

**Architectural Decisions ✅**
- [x] Stack .NET 10 LTS complet avec packages par couche
- [x] ORM EF Core 10 + Migrations + schéma 2 tables
- [x] Error handling Result<T> + Polly retry 3x backoff exponentiel
- [x] Twilio + NullNotifier dry-run
- [x] Exit codes 0/1/2 + partial failure non-bloquant

**Implementation Patterns ✅**
- [x] Conventions de nommage — namespaces, classes, méthodes, DB
- [x] Format patterns — Result<T>, enums, log structuré UTC
- [x] Process patterns — pannes partielles, retry, dry-run injection
- [x] Anti-patterns documentés avec exemples de code

**Project Structure ✅**
- [x] Arborescence fichier-par-fichier complète (src + tests + CI/CD)
- [x] Boundaries de couche explicites avec table de restrictions
- [x] Mapping 30 FRs → fichiers
- [x] Corrections de validation intégrées (PropertyAnalyzer, PipelineResult, Options)

### Architecture Readiness Assessment

**Status global : READY FOR IMPLEMENTATION**

**Confiance : Haute** — toutes les décisions sont prises, la structure est fichier-par-fichier, les patterns sont illustrés par des exemples de code, les anti-patterns sont documentés.

**Forces clés :**
- Domain sans dépendance NuGet → `PropertyAnalyzer` testable en isolation totale
- Interfaces Application → swap Redfin→Zillow ou SQLite fallback sans toucher Domain/Application
- `NullWhatsAppNotifier` → dry-run transparent sans if/else dans le code métier
- `Result<T>` → exit codes 0/1/2 déterministes sans propagation d'exceptions

**Évolutions post-MVP (décisions différées) :**
- Facebook Marketplace → nouveau `IPropertySource` en Infrastructure uniquement
- SQLite fallback → swap `ImmoPilotDbContext` + connection string en Infrastructure uniquement

### Implementation Handoff

**Ordre d'implémentation recommandé :**

1. `dotnet new sln` + 7 projets + références croisées
2. `Domain` : `Result<T>`, `Property`, `DscrResult`, `FmrData`, `DscrThresholds`, `MarketConfig`
3. `Domain/Services` : `PropertyAnalyzer` — tests unitaires en parallèle (≥80%)
4. `Application` : 4 interfaces + `PipelineResult` enum + classes `Options/`
5. `Infrastructure/Persistence` : `ImmoPilotDbContext`, entités EF, `EfPropertyRepository`, migration initiale
6. `Infrastructure/Http` : `RedfinHttpClient`, `HudFmrHttpClient`
7. `Infrastructure/Notifications` : `TwilioWhatsAppNotifier`, `NullWhatsAppNotifier`
8. `Application/Services` : `PipelineOrchestrator` (toutes interfaces mockées via NSubstitute)
9. `Console/Program.cs` : Generic Host, DI wiring, `MigrateAsync`, `Environment.Exit`
10. `.github/workflows/nightly.yml` : cron, secrets injection, exit code propagation

