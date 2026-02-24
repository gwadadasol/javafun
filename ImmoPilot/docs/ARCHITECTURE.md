# Architecture Technique — ImmoPilot

## Vue d'ensemble

ImmoPilot suit une **Clean Architecture** en 4 couches concentriques. Les dépendances ne pointent que vers l'intérieur : le domaine ne connaît rien des couches externes.

```mermaid
graph TD
    subgraph Console["🖥️ Console (Entry Point)"]
        P[Program.cs<br/>Generic Host · DI · MigrateAsync]
    end

    subgraph Application["📋 Application"]
        O[PipelineOrchestrator]
        IF[Interfaces<br/>IPropertySource · IHudFmrClient<br/>IPropertyRepository · IWhatsAppNotifier]
        OPT[Options<br/>PipelineOptions · MarketOptions<br/>DscrThresholdOptions]
        PM[PipelineResult enum]
    end

    subgraph Infrastructure["🔧 Infrastructure"]
        RF[RedfinHttpClient]
        HUD[HudFmrHttpClient]
        EF[EfPropertyRepository<br/>ImmoPilotDbContext]
        TW[TwilioWhatsAppNotifier]
        NW[NullWhatsAppNotifier<br/>dry-run]
        IOPT[Options<br/>NotificationOptions · ApiOptions]
    end

    subgraph Domain["🏛️ Domain (no dependencies)"]
        PR[Property]
        DR[DscrResult · FmrData]
        EN[DomainEnums<br/>DscrStatus · FmrStatus]
        PA[PropertyAnalyzer<br/>domain service]
        VO[DscrThresholds · MarketConfig<br/>value objects]
        RS[Result&lt;T&gt;]
    end

    Console --> Application
    Console --> Infrastructure
    Application --> Domain
    Infrastructure --> Application
    Infrastructure --> Domain

    style Domain fill:#e8f5e9,stroke:#388e3c
    style Application fill:#e3f2fd,stroke:#1976d2
    style Infrastructure fill:#fff3e0,stroke:#f57c00
    style Console fill:#fce4ec,stroke:#c2185b
```

## Flux de données

```mermaid
sequenceDiagram
    participant GH as GitHub Actions
    participant CLI as Console
    participant RF as RedfinHttpClient
    participant HUD as HudFmrHttpClient
    participant PA as PropertyAnalyzer
    participant DB as EfPropertyRepository
    participant TW as TwilioWhatsAppNotifier

    GH->>CLI: dotnet run (04:00 UTC)
    CLI->>CLI: MigrateAsync()
    CLI->>RF: FetchListingsAsync(marketConfig)
    RF-->>CLI: Result<IEnumerable<Property>>

    loop For each property
        CLI->>HUD: GetFmrByZipAsync(zip, bedrooms)
        HUD-->>CLI: Result<FmrData>
        CLI->>PA: Analyze(property, fmrData, thresholds)
        PA-->>CLI: DscrResult (ratio, status)
        CLI->>DB: SaveAsync(property)
    end

    CLI->>DB: GetUnnotifiedQualifiedAsync(today)
    DB-->>CLI: qualified properties

    alt qualified.Count > 0
        CLI->>TW: SendDigestAsync(qualified, runId)
        TW-->>CLI: Result<bool>
        CLI->>DB: LogNotificationAsync(mlsIds, today)
    end

    CLI->>GH: Environment.Exit(0|1|2)
```

## Couches

### Domain (`ImmoPilot.Domain`)

Aucune dépendance externe. Contient la logique métier pure.

| Élément | Rôle |
|---|---|
| `Property` | Entité principale — données Redfin + enrichissement FMR/DSCR |
| `DscrResult` | Record immuable renvoyé par l'analyse |
| `FmrData` | Données HUD Fair Market Rent |
| `DscrThresholds` | Value object — seuils DSCR configurables |
| `MarketConfig` | Value object — configuration du marché |
| `PropertyAnalyzer` | Service domaine statique — calcule le ratio DSCR |
| `Result<T>` | Monade résultat — pas d'exceptions pour les cas métier |
| `DscrStatus` | `Qualified` / `Warning` / `Rejected` |

**Formule DSCR :**
```
DSCR = (FMR × 12 × (1 − VacancyRate − OpexRate))
       ─────────────────────────────────────────────
       LoanAmount × (r×(1+r)³⁶⁰)/((1+r)³⁶⁰−1) × 12
```

### Application (`ImmoPilot.Application`)

Orchestre les cas d'usage. Dépend uniquement du domaine.

| Élément | Rôle |
|---|---|
| `IPropertySource` | Contrat d'accès aux annonces Redfin |
| `IHudFmrClient` | Contrat d'accès aux données HUD FMR |
| `IPropertyRepository` | CRUD + déduplication des notifications |
| `IWhatsAppNotifier` | Contrat d'envoi WhatsApp |
| `PipelineOrchestrator` | Coordination complète du pipeline |
| `PipelineResult` | Enum → code de sortie Unix (0/1/2) |
| `*Options` | Options typées lues depuis la configuration |

### Infrastructure (`ImmoPilot.Infrastructure`)

Implémente les interfaces de l'Application.

| Élément | Technologie | Rôle |
|---|---|---|
| `RedfinHttpClient` | `HttpClient` + Polly | Télécharge le CSV Redfin |
| `HudFmrHttpClient` | `HttpClient` + Polly | Interroge l'API HUD |
| `ImmoPilotDbContext` | EF Core 10 / Azure SQL | Contexte base de données |
| `EfPropertyRepository` | EF Core | Persistance + logs notifications |
| `TwilioWhatsAppNotifier` | Twilio SDK | Envoi réel WhatsApp |
| `NullWhatsAppNotifier` | — | No-op pour dry-run |

Toutes les requêtes HTTP utilisent `AddStandardResilienceHandler()` (Polly).

### Console (`ImmoPilot.Console`)

Point d'entrée unique. Câble le tout via **Generic Host**.

```
Program.cs
  ├── UseSerilog()           → logs structurés console
  ├── ConfigureAppConfiguration() → appsettings.json + variables d'env
  ├── ConfigureServices()    → DI : EF Core, HTTP clients, notifier, orchestrator
  ├── MigrateAsync()         → migrations EF appliquées au démarrage
  ├── PipelineOrchestrator.RunAsync()
  └── Environment.Exit(0|1|2)
```

## Schéma de la base de données

```mermaid
erDiagram
    Properties {
        uniqueidentifier Id PK
        nvarchar50 MlsId UK
        nvarchar200 Address
        nvarchar100 City
        nvarchar10 Zip
        decimal18_2 Price
        decimal8_6 CapRate
        decimal8_6 CocRate
        decimal12_2 CashFlow
        decimal12_2 CashRequired
        decimal10_2 FmrRent
        int FmrYear
        decimal5_4 RehabPercent
        nvarchar DscrStatus
        nvarchar FmrStatus
        nvarchar ListingStatus
        datetime2 ScannedAt
        decimal8_4 DscrRatio
    }

    NotificationLogs {
        uniqueidentifier Id PK
        uniqueidentifier PropertyId FK
        datetime2 SentAt
        date DigestDate
        nvarchar500 Recipients
    }

    Properties ||--o{ NotificationLogs : "has"
```

## Dépendances NuGet

| Package | Version | Usage |
|---|---|---|
| `Microsoft.EntityFrameworkCore.SqlServer` | 10.0.3 | ORM Azure SQL |
| `Microsoft.Extensions.Http.Resilience` | 10.3.0 | Polly retry/circuit-breaker |
| `Serilog.Extensions.Hosting` | 10.0.0 | Logs structurés |
| `Serilog.Sinks.Console` | 6.1.1 | Sortie console formatée |
| `Twilio` | 7.14.3 | SDK WhatsApp |
| `Microsoft.Extensions.Hosting` | 10.0.3 | Generic Host |
| `xunit` | 2.9.3 | Tests unitaires |
| `Reqnroll.xUnit` | 3.3.3 | Tests BDD (successeur SpecFlow) |
| `NSubstitute` | 5.3.0 | Mocking |
| `Shouldly` | 4.3.0 | Assertions (MIT) |

## Décisions d'architecture

### `Result<T>` au lieu des exceptions
Les erreurs prévisibles (FMR indisponible, échec HTTP) retournent `Result<T>.Fail(message)`. Seules les exceptions système inattendues remontent. Cela rend le flux de contrôle explicite dans `PipelineOrchestrator`.

### `PropertyAnalyzer` comme service domaine statique
Le calcul DSCR est de la logique métier pure, sans état et sans I/O. Une classe statique dans `Domain/Services/` est l'expression la plus directe de ce fait — testable unitairement sans mock.

### Deux implémentations de `IWhatsAppNotifier`
`TwilioWhatsAppNotifier` pour la production, `NullWhatsAppNotifier` pour le dry-run. Le choix est fait au démarrage dans `Program.cs` selon `Pipeline:DryRun`, sans aucun `if` dans l'orchestrateur.

### Entités EF séparées des entités Domain
`PropertyEntity` (Infrastructure) ≠ `Property` (Domain). Le mapping explicite dans `EfPropertyRepository` isole complètement le schéma SQL du modèle domaine.
