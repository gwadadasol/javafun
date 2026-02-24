# ImmoPilot

Nightly CLI pipeline that scans Redfin listings, enriches them with HUD Fair Market Rent (FMR) data, computes DSCR ratios, persists results to Azure SQL, and sends a WhatsApp digest via Twilio — all driven by GitHub Actions.

## How it works

```
Redfin CSV  →  HUD FMR API  →  DSCR Analysis  →  Azure SQL  →  WhatsApp (Twilio)
```

Each night at 04:00 UTC the pipeline:
1. Downloads active listings from Redfin for your configured ZIP codes
2. Fetches HUD Fair Market Rent for each ZIP
3. Calculates the DSCR ratio using a 30-year fixed mortgage model
4. Saves / updates each property in Azure SQL (idempotent)
5. Sends a WhatsApp digest for properties not yet notified today

## Prerequisites

| Requirement | Version |
|---|---|
| .NET SDK | 10.0+ |
| Azure SQL Database | Any tier |
| Twilio account | WhatsApp-enabled sandbox or number |
| HUD USPS API key | Free at [huduser.gov](https://www.huduser.gov/portal/dataset/fmr-api.html) |

## Quick start

### 1. Clone and configure

```bash
git clone <repo-url>
cd ImmoPilot
cp appsettings.Example.json src/ImmoPilot.Console/appsettings.json
```

Edit `src/ImmoPilot.Console/appsettings.json` and fill in every blank field (see [Configuration](#configuration)).

### 2. Apply database migrations

```bash
cd src/ImmoPilot.Console
dotnet run
# Migrations are applied automatically at startup via MigrateAsync()
```

### 3. Run in dry-run mode (no messages sent, no notification logs written)

```bash
Pipeline__DryRun=true dotnet run --project src/ImmoPilot.Console
```

### 4. Run normally

```bash
dotnet run --project src/ImmoPilot.Console --configuration Release
```

## Configuration

All settings can be overridden by environment variables using the `__` separator (e.g. `Market__City=Tempe`).

### Connection string

```json
"ConnectionStrings": {
  "ImmoPilot": "Server=tcp:<server>.database.windows.net,1433;Database=ImmoPilot;Authentication=Active Directory Default;"
}
```

### Pipeline

| Key | Type | Default | Description |
|---|---|---|---|
| `Pipeline:DryRun` | bool | `false` | Skip Twilio send and notification logging |
| `Pipeline:RunIdPrefix` | string | `"Run"` | Prefix for run ID in WhatsApp message |

### Market

| Key | Type | Example | Description |
|---|---|---|---|
| `Market:ZipCodes` | string[] | `["85281","85282"]` | ZIP codes to scan |
| `Market:City` | string | `"Tempe"` | City label |
| `Market:State` | string | `"AZ"` | State abbreviation |
| `Market:BedroomsFilter` | int | `2` | Bedroom count for FMR lookup |
| `Market:MaxPrice` | decimal | `500000` | Maximum listing price filter |
| `Market:MinCapRate` | decimal | `0.05` | Minimum cap rate filter (currently informational) |

### DSCR Thresholds

| Key | Default | Description |
|---|---|---|
| `DscrThresholds:MinQualifyingRatio` | `1.25` | DSCR ≥ this → **Qualified** |
| `DscrThresholds:WarningRatio` | `1.0` | DSCR ≥ this → **Warning**, otherwise **Rejected** |
| `DscrThresholds:VacancyRate` | `0.05` | 5% vacancy deducted from gross rent |
| `DscrThresholds:OpexRate` | `0.40` | 40% operating expense ratio |
| `DscrThresholds:AnnualMortgageRate` | `0.07` | 7% annual mortgage rate |
| `DscrThresholds:DownPaymentPercent` | `0.20` | 20% down payment |

### Notification (Twilio)

| Key | Description |
|---|---|
| `Notification:TwilioAccountSid` | Twilio Account SID (`ACxxx…`) |
| `Notification:TwilioAuthToken` | Twilio Auth Token |
| `Notification:FromWhatsApp` | Twilio WhatsApp number (e.g. `+14155238886`) |
| `Notification:ToWhatsApp` | Recipient WhatsApp number (e.g. `+1XXXXXXXXXX`) |

### API

| Key | Default | Description |
|---|---|---|
| `Api:HudApiKey` | — | HUD USPS API key |
| `Api:RedfinBaseUrl` | `https://www.redfin.com` | Redfin base URL |
| `Api:HudBaseUrl` | `https://www.huduser.gov` | HUD API base URL |

## Exit codes

| Code | Meaning | GitHub Actions behaviour |
|---|---|---|
| `0` | Success — all properties saved and digest sent | Workflow passes |
| `1` | Partial success — some save errors, digest still sent | Workflow passes (warning) |
| `2` | Failure — fetch failed or critical error | Workflow fails |

## DSCR formula

```
Annual NOI        = FMR × 12 × (1 − VacancyRate − OpexRate)
Annual Debt Ser.  = LoanAmount × (r × (1+r)^360) / ((1+r)^360 − 1) × 12
DSCR              = Annual NOI / Annual Debt Service

where LoanAmount  = Price × (1 − DownPaymentPercent)
      r           = AnnualMortgageRate / 12
```

## GitHub Actions (nightly)

The workflow at `.github/workflows/nightly.yml` runs daily at 04:00 UTC.

### Required repository secrets

| Secret | Description |
|---|---|
| `AZURE_SQL_CONNECTION_STRING` | Full ADO.NET connection string |
| `TWILIO_ACCOUNT_SID` | Twilio Account SID |
| `TWILIO_AUTH_TOKEN` | Twilio Auth Token |
| `TWILIO_FROM_WHATSAPP` | Sender WhatsApp number |
| `WHATSAPP_RECIPIENT` | Recipient WhatsApp number |
| `HUD_API_KEY` | HUD Fair Market Rent API key |
| `MARKET_ZIP_0` … `MARKET_ZIP_2` | ZIP codes to scan (one secret per ZIP) |
| `MARKET_CITY` | City name |
| `MARKET_STATE` | State abbreviation |

## Running tests

```bash
# All tests
dotnet test ImmoPilot.sln

# With coverage report
dotnet test ImmoPilot.sln --collect:"XPlat Code Coverage" --results-directory ./coverage
reportgenerator -reports:"coverage/**/coverage.cobertura.xml" \
                -targetdir:"coverage/report" \
                -reporttypes:"Html;TextSummary" \
                -assemblyfilters:"+ImmoPilot.*"
xdg-open coverage/report/index.html
```

Current coverage: **87% line coverage** across Domain + Application layers.

## Project structure

```
ImmoPilot/
├── src/
│   ├── ImmoPilot.Domain/          # Entities, value objects, domain service
│   ├── ImmoPilot.Application/     # Interfaces, orchestrator, options
│   ├── ImmoPilot.Infrastructure/  # EF Core, HTTP clients, Twilio
│   └── ImmoPilot.Console/         # Entry point, DI wiring
├── tests/
│   ├── ImmoPilot.Domain.Tests/    # Unit tests (xUnit + Shouldly)
│   ├── ImmoPilot.Application.Tests/ # Unit tests (NSubstitute mocks)
│   └── ImmoPilot.BDD.Tests/       # BDD scenarios (Reqnroll / Gherkin)
├── appsettings.Example.json       # Configuration reference
└── .github/workflows/nightly.yml  # CI/CD
```
