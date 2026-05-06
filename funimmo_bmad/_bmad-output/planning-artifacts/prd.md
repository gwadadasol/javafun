---
stepsCompleted: ['step-01-init', 'step-02-discovery', 'step-02b-vision', 'step-02c-executive-summary', 'step-03-success', 'step-04-journeys', 'step-05-domain', 'step-06-innovation', 'step-07-project-type', 'step-08-scoping', 'step-09-functional', 'step-10-nonfunctional', 'step-11-polish', 'step-12-complete']
workflowStatus: complete
completedAt: '2026-02-22'
inputDocuments: ['_bmad-output/planning-artifacts/product-brief-funimmo_bmad-2026-02-22.md', '_bmad-output/brainstorming/brainstorming-session-2026-02-20.md']
workflowType: 'prd'
classification:
  projectType: cli_tool
  domain: proptech_real_estate_investment
  complexity: medium
  projectContext: greenfield
---

# Product Requirements Document - funimmo_bmad

**Author:** Philippe
**Date:** 2026-02-22

## Executive Summary

ImmoPilot est un pipeline d'automatisation C# .NET à exécution nightly qui détecte, analyse, filtre et notifie des opportunités d'investissement immobilier (duplex/triplex) sur 5 marchés secondaires de l'Ohio (Springfield, Dayton, Akron, Cleveland, Toledo). Le système consomme l'API Redfin, enrichit chaque listing avec les loyers Section 8 via l'API publique HUD FMR, calcule les métriques DSCR (CAP rate, Cash-on-Cash, Cash Flow, Cash Required), et délivre un digest WhatsApp quotidien à 3 décisionnaires (Philippe, Giancarlo, Linh) contenant uniquement les propriétés passant les seuils définis : CAP ≥ 8%, CoC ≥ 12%, Rehab ≤ 20% du prix d'achat.

**Problème résolu :** Sans flux suffisant de deals pré-qualifiés, l'équipe ne peut attirer l'attention d'un Property Manager dédié ni démontrer la viabilité du modèle à des investisseurs externes — bloquant la croissance avant même que le premier pipeline réel soit établi. ImmoPilot brise ce cercle vicieux en industrialisant la phase de détection et de qualification initiale.

**Utilisateurs primaires :** Philippe (investisseur-développeur, opérateur système), Giancarlo (agent immobilier, gate-keeper terrain), Linh (co-investisseur, filtre intuitif). Aucun accès investisseur externe au système — ils reçoivent des offres packagées en aval.

**Stack technique :** C# .NET · Azure SQL · GitHub Actions · WhatsApp Business API · Redfin API (non-officielle) · HUD FMR REST API. Coût opérationnel : ~$5/mois.

### What Makes This Special

1. **HUD FMR API intégrée nativement** — Seul outil utilisant les loyers Section 8 HUD (huduser.gov/hudapi/public/fmr) comme estimateur conservateur par code postal. Élimine le biais haussier des comparables de marché standard. Aucun concurrent générique (PropStream, DealMachine) ne l'intègre.

2. **Calibration DSCR Ohio granulaire** — Seuils configurés précisément pour les marchés secondaires Ohio (prix min $100k, types duplex/triplex, 5 villes cibles). PropStream ($99-199/mois) est générique US sans adaptation aux contraintes DSCR locales. ImmoPilot coûte 20x moins cher pour une analyse plus précise.

3. **Tri humain complémentaire** — L'outil est le filtre quantitatif ; l'équipe est le filtre qualitatif (terrain Giancarlo + instinct Linh). Conception intentionnelle : ImmoPilot ne décide pas, il pré-qualifie pour que les humains décident vite et bien.

4. **Facebook Marketplace off-market (v1.1)** — Propriétés FSBO invisibles sur les portails classiques. Avantage compétitif réel sur les autres investisseurs qui ne consultent que Zillow/Redfin.

5. **Infrastructure $5/mois** — GitHub Actions (cron gratuit) + Azure SQL Basic. Viable dès le premier deal, sans investissement infrastructure.

## Project Classification

- **Type :** Automation CLI Pipeline — background service C# .NET, exécution cron GitHub Actions, interface utilisateur = digest WhatsApp + fichier appsettings.json
- **Domaine :** Proptech / Real Estate Investment Analysis — analyse financière multi-variable, agrégation multi-sources, alerting décisionnel
- **Complexité :** Moyenne — APIs externes fragiles (Redfin non-officielle), calcul financier DSCR multi-variable, déduplication cross-sources, pas de compliance réglementaire
- **Contexte :** Greenfield — nouveau système, usage interne (3 utilisateurs actifs), pas de codebase existante ni d'intégrations héritées

## Success Criteria

### User Success

**Philippe (investisseur-développeur, opérateur système)**
- Reçoit le digest WhatsApp avant 8h chaque matin, 7j/7
- Au moins 1 propriété/semaine obtient consensus positif de l'équipe (Philippe + Giancarlo + Linh)
- Délai alerte → visite physique ≤ 7 jours pour les propriétés qualifiées
- Peut modifier les seuils DSCR via `appsettings.json` sans modifier le code

**Giancarlo (agent immobilier, gate-keeper terrain)**
- Reçoit ≥ 3 propriétés qualifiées/semaine avec 5 données clés : adresse, prix, CAP, CoC, Cash Flow
- Peut ajuster les paramètres (villes, seuils) de manière autonome via fichier config
- Investit ≤ 30 min/semaine sur ImmoPilot pour filtrer et agir

**Linh (co-investisseur, filtre intuitif)**
- Reçoit le même digest que Philippe et Giancarlo pour préparer le meeting hebdomadaire
- Ses jugements intuitifs sont informés par les données avant la réunion d'équipe

**Signal externe de succès**
- Un Property Manager demande spontanément : *"Comment vous identifiez ces deals ?"* — validation que le volume et la qualité des leads démontre un avantage compétitif réel

### Business Success

**3 mois — Opérationnel**
- Pipeline GitHub Actions stable 7j/7, zéro arrêt non planifié
- Au moins 1 visite physique d'une propriété générée directement par une alerte ImmoPilot
- Les 3 décisionnaires reçoivent et lisent le digest WhatsApp quotidiennement

**6 mois — Traction**
- ≥ 5 visites physiques générées par des alertes ImmoPilot
- 2e deal signé (conditionnel au financement disponible) — brise le cercle vicieux PM
- Au moins 1 propriété off-market Facebook identifiée (si v1.1 déployée)

**12 mois — Viabilité du modèle**
- ≥ 500 propriétés évaluées au total (base de données Azure SQL)
- Tendances saisonnières visibles : données suffisantes pour ajuster les périodes de prospection
- Au moins 1 conversation sérieuse avec un investisseur externe basée sur la démonstration du pipeline

### Technical Success

**Performance opérationnelle**
- Durée d'exécution nightly ≤ 30 minutes (GitHub Actions gratuit : 2000 min/mois)
- Digest WhatsApp livré avant 8h00 heure locale (GMT-5 Ohio)
- Zéro doublon dans la base Azure SQL — déduplication par adresse + MLS ID

**Résilience API**
- Retry automatique : 3 tentatives avec backoff exponentiel (1s, 2s, 4s) sur toute erreur API
- Panne partielle non-bloquante : si HUD FMR échoue pour un ZIP, la propriété est marquée `FMR_UNAVAILABLE` mais reste traitée avec les données disponibles
- Auto-monitoring : WhatsApp d'alerte envoyé à Philippe si le pipeline GitHub Actions échoue

**Qualité du code**
- ≥ 80% de couverture de tests unitaires sur `PropertyAnalyzer` (moteur DSCR)
- Scénarios BDD avec SpecFlow (Gherkin Given/When/Then) couvrant les cas limites DSCR (propriétés à la frontière des seuils, valeurs manquantes, FMR indisponible)

### Measurable Outcomes

| KPI | Cible | Mesure |
|-----|-------|--------|
| Disponibilité pipeline | 99% (≤ 3 pannes/an) | GitHub Actions logs |
| Durée d'exécution | ≤ 30 min/run | GitHub Actions duration |
| Propriétés scannées | ≥ 50/semaine (5 marchés) | Azure SQL COUNT |
| Propriétés qualifiées | ≥ 3/semaine (CAP≥8%, CoC≥12%) | Azure SQL filtered COUNT |
| Taux de qualification | 5–15% des propriétés scannées | Ratio qualifiées/scannées |
| Délai alerte → visite | ≤ 7 jours | Suivi manuel équipe |
| Couverture tests | ≥ 80% sur PropertyAnalyzer | dotnet test coverage |
| Doublons | 0 doublon/semaine | Azure SQL duplicate check |

## Product Scope

### MVP — Minimum Viable Product (v1.0)

Fonctionnalités essentielles pour prouver la viabilité du modèle :

1. **Scraping Redfin** — API non-officielle, filtrage duplex/triplex, 5 villes Ohio, prix ≥ $100k
2. **Enrichissement HUD FMR** — API publique `huduser.gov/hudapi/public/fmr`, loyer Section 8 par ZIP code, estimateur conservateur
3. **Moteur DSCR** — Calcul : CAP rate, Cash-on-Cash, Cash Flow mensuel, Cash Required ; seuils : CAP ≥ 8%, CoC ≥ 12%, Rehab ≤ 20% prix achat ; hypothèse : 20% down payment
4. **Déduplication Azure SQL** — Évite re-notification de propriétés déjà vues ; track les propriétés rejetées
5. **Digest WhatsApp** — Résumé quotidien aux 3 décisionnaires, ≤ 5 propriétés/jour, avant 8h ; alerte WARNING si 5/6 critères passés
6. **Configuration `appsettings.json`** — Seuils DSCR, liste des villes, numéros WhatsApp, fréquence
7. **GitHub Actions cron** — Exécution nightly automatique, logs structurés, retry 3x backoff exponentiel, auto-monitoring

**Hors scope MVP :**

| Feature | Raison du report |
|---------|-----------------|
| Facebook Marketplace FSBO | Playwright .NET = complexité supplémentaire, valeur prouvée en v1.0 d'abord |
| Simulation "what-if" refinancing | Nice-to-have, pas bloquant pour la qualification initiale |
| Interface CLI interactive | `appsettings.json` suffit pour 3 utilisateurs techniques |
| Dashboard statistiques | Base de données insuffisante au lancement |

### Growth Features — Post-MVP (v1.1)

Fonctionnalités ajoutées une fois le MVP validé et opérationnel :

- **Facebook Marketplace off-market** — Microsoft.Playwright .NET, scraping FSBO Ohio, propriétés invisibles sur Zillow/Redfin ; avantage compétitif majeur
- **Simulation "what-if"** — CLI `dotnet run --simulate --price=120000 --rehab=15000` pour évaluer des deals off-line ou négocier en temps réel
- **Auto-monitoring avancé** — Alertes dégradation (HUD FMR down >24h, Redfin throttling, Azure SQL storage)

### Vision — Future (v2.0+)

Évolution vers un vrai outil d'équipe et de démonstration investisseur :

- **Dashboard web** (Blazor ou React) — Visualisation historique, tendances saisonnières, carte deal flow Ohio
- **Scoring prédictif Linh** — Modèle ML entraîné sur les décisions historiques de Linh pour automatiser son filtre intuitif
- **Mémo investisseur automatique** — PDF/email generé automatiquement pour les propriétés passant tous les filtres (Philippe + Giancarlo + Linh)
- **Seller financing calculator** — Simulation de financement vendeur pour transactions sans banque
- **Interface config UI** — Web simple pour modifier les seuils sans éditer JSON
- **Multi-marché** — Extension au-delà de l'Ohio : Midwest secondaire (Indianapolis, Columbus, Pittsburgh)

## User Journeys

### Journey 1 — Philippe : Le Premier Vrai Deal (à distance)

*Parcours principal — chemin de succès*

**Scène d'ouverture**
Lundi 7h32, Montréal. Philippe est dans sa cuisine avant de démarrer sa journée. ImmoPilot tourne depuis 3 semaines. Ce matin : 3 propriétés dans le digest — dont une avec tous les critères au vert.

**Action montante**
Duplex à Springfield, $127,500, CAP 9.1%, CoC 14.3%, Cash Flow $380/mois, Rehab $18k (14%). Section 8 FMR $820/unité. Philippe transfère dans le groupe WhatsApp équipe.

**Climax**
Giancarlo valide la structure financière — il connaît les marchés secondaires Ohio via ses contacts. Linh : *"Ça me parle."* L'équipe contacte le courtier inscripteur Ohio en fin de journée. Le courtier confirme : propriété toujours disponible, vendeur motivé. Ils contactent un Property Manager de Springfield pour organiser une visite.

**Résolution**
Le PM visite, envoie photos et rapport. L'équipe décide de faire une offre à distance. Le PM pose la question fatale : *"Comment vous avez identifié cette propriété ?"* — le cercle vicieux est brisé. ImmoPilot a généré le deal flow qui attire le PM dédié.

**Capacités révélées :** Scraping Redfin, moteur DSCR, WhatsApp digest formatté avec données clés, déduplication.

---

### Journey 2 — Giancarlo : Validation Financière à Distance

*Utilisateur secondaire — validation stratégique*

**Scène d'ouverture**
Mercredi 9h00, Giancarlo est entre deux rendez-vous au Québec. Il reçoit le digest ImmoPilot avec 2 propriétés qualifiées.

**Action montante**
Triplex à Akron, CAP 8.3% — il passe. Duplex à Dayton, $115k — les ratios CAP/CoC sont solides pour ce type de marché secondaire. Il appelle Philippe : *"Le Dayton a l'air sérieux. Les chiffres tiennent. Il faut trouver quelqu'un pour le visiter."*

**Climax**
Giancarlo utilise son réseau pour trouver un contact Ohio qui peut organiser une visite terrain via un PM local. Il prépare un dossier financier succinct : données ImmoPilot + contexte Section 8 HUD. Ce dossier pourra être montré à des investisseurs potentiels — *"voilà notre processus de qualification."*

**Résolution**
La visite est organisée en 48h via un PM Ohio. Giancarlo a passé 20 minutes sur ce deal — ImmoPilot lui a évité 3 heures de recherche manuelle sur PropStream et Zillow.

**Capacités révélées :** Digest ≤ 5 props avec données clés, adresse précise, format exploitable en conversation téléphonique.

---

### Journey 3 — Linh : Le Filtre Avant le Meeting

*Utilisateur secondaire — filtre intuitif*

**Scène d'ouverture**
Vendredi soir, Montréal. Linh consulte les alertes de la semaine sur son téléphone avant le meeting hebdo du samedi. 9 propriétés qualifiées cette semaine.

**Action montante**
Philippe et Giancarlo ont écarté la #4 : triplex à Toledo, CAP 8.1% — seuil minimal, quartier *"à surveiller"*. Linh la regarde. Elle n'a pas d'explication rationnelle mais cette adresse retient son attention.

**Climax**
Elle ajoute dans le groupe : *"Toledo m'intéresse. On peut en parler samedi ?"* Philippe n'avait pas d'avis tranché. L'équipe vote samedi : 2 pour 1 contre — Philippe contacte un PM Toledo pour exploration.

**Résolution**
Le PM Ohio visite. Le budget rehab est sous-estimé (+$8k). L'équipe passe. Mais Linh avait raison d'insister sur le processus : le PM Toledo est maintenant un contact actif dans leur réseau Ohio.

**Capacités révélées :** Même digest pour les 3, format uniforme, historique de la semaine accessible via scroll WhatsApp.

---

### Journey 4 — Philippe : La Nuit Où Rien N'Est Arrivé

*Cas limite — Troubleshooting & résilience*

**Scène d'ouverture**
Mardi 8h20, Montréal. Pas de WhatsApp ImmoPilot. À 8h30, un message d'un numéro dédié : *"[ImmoPilot ALERT] Pipeline failed at 03:47 UTC. Redfin API 429 after 3 retries (1s, 2s, 4s). Run #247."*

**Action montante**
Philippe ouvre GitHub Actions depuis son bureau. Log clair : Redfin throttlé, 3 retries épuisés, arrêt propre. Azure SQL intact. Aucune propriété corrompue — la panne partielle non-bloquante a fonctionné.

**Climax**
Il relance manuellement le scan à 10h. 2 propriétés qualifiées arrivent à 10h23 sur WhatsApp. L'équipe est informée du délai. Zéro panique : l'auto-monitoring a fait son travail avant que quiconque cherche.

**Résolution**
Il décale le cron de 07:00 UTC à 04:30 UTC pour éviter les pics Redfin — une ligne dans `appsettings.json`. Le lendemain, le digest arrive à 6h58. Incident clos, aucune donnée perdue.

**Capacités révélées :** Auto-monitoring WhatsApp sur échec pipeline, logs structurés (erreur, retry count, run ID), arrêt propre sans corruption Azure SQL, relance manuelle idempotente (pas de doublons).

---

### Journey 5 — Philippe Opérateur : Ajuster les Seuils Entre les Saisons

*Admin/Configuration — paramétrage sans développement*

**Scène d'ouverture**
Début mars, Montréal. 2e deal finalisé. L'équipe réalise que les triplex Cleveland offrent de meilleures marges. Giancarlo suggère de monter le seuil CAP à 9% pour filtrer plus agressivement.

**Action montante**
`appsettings.json` : `MinCapRate: 9.0`, `PropertyTypes: [3]` (triplex uniquement), ajout d'Akron comme 6e marché test. Commit, push. Le prochain cron prend les nouveaux paramètres automatiquement.

**Climax**
Lendemain matin : 1 seule propriété dans le digest. Triplex Cleveland, CAP 9.4%. Giancarlo : *"C'est exactement ce qu'on cherche. Le bruit a disparu."*

**Résolution**
Deux semaines plus tard, le marché se resserre. Philippe remet les duplex : une ligne JSON, aucun redéploiement. La flexibilité config-first justifiée par le comportement réel de l'équipe.

**Capacités révélées :** `appsettings.json` exhaustif, paramètres `PropertyTypes`, `MinCapRate`, `MinCocRate`, `MaxRehabPercent`, `TargetCities`, modifications sans redéploiement, effet au prochain cron.

---

### Journey Requirements Summary

| Journey | Capacités requises |
|---------|-------------------|
| Philippe — deal à distance | Scraping Redfin, moteur DSCR, digest WhatsApp formatté, déduplication |
| Giancarlo — validation stratégique | Données clés lisibles en 30s, adresse précise, ≤ 5 props/jour |
| Linh — filtre intuitif | Même digest que Philippe/Giancarlo, historique hebdo, format identique |
| Philippe — panne nightly | Auto-monitoring WhatsApp, logs structurés, arrêt propre, relance idempotente |
| Philippe — config saisonnière | `appsettings.json` exhaustif, effet au prochain cron, sans redéploiement |

## Domain-Specific Requirements

**Domaine :** Proptech / Real Estate Investment Analysis — aucune certification réglementaire requise (pas de traitement de fonds, pas de données patients, pas de transactions financières directes). La complexité provient de la fragilité des sources de données externes et de la correctness du modèle financier.

### Légalité & Usage API

- **Redfin API non-officielle** — Endpoints non documentés, utilisés par l'app mobile Redfin. Usage techniquement contraire aux CGU Redfin. Risque : throttling agressif, blocage IP, changement de structure de réponse sans préavis. Mitigation : retry avec backoff exponentiel, aucune donnée revendue ou publiée.
- **HUD FMR API publique** — Données gouvernementales US librement accessibles (`huduser.gov/hudapi/public/fmr`). Aucun risque légal. Données mises à jour annuellement (octobre) — la valeur FMR peut avoir 0 à 12 mois de retard. Mitigation : inclure l'année FMR dans le digest WhatsApp.
- **Données Redfin** — Listings publics uniquement, aucune donnée personnelle collectée. PIPEDA/GDPR non applicables.

### Correctness Financière

- **Modèle DSCR** — Calcul basé sur hypothèses documentées (20% down, loyer = FMR Section 8, rehab = estimation configurable). Les résultats sont des **indicateurs de pré-qualification**, pas des analyses d'investissement certifiées. Le digest WhatsApp doit inclure le disclaimer : *"Chiffres estimatifs — validation terrain requise avant toute offre."*
- **FMR comme proxy loyer** — Section 8 FMR est conservateur par design (percentile 40 du marché local). Le loyer de marché réel peut être 10–20% supérieur. Biais conservateur intentionnel : réduire les faux positifs, pas les faux négatifs.
- **Rehab estimate** — Redfin ne fournit pas d'estimation rehab officielle. ImmoPilot utilise une règle configurable via `appsettings.json` (ex: 10% du prix comme valeur par défaut). Risque de sous-estimation significative. Mitigation : flag WARNING quand rehab > 15% du prix d'achat.

### Fragilité des Données

- **Listings obsolètes** — Redfin peut retourner des propriétés sous contrat ou vendues. Mitigation : déduplication par MLS ID, flag `PENDING` si le statut Redfin l'indique.
- **ZIP code manquant ou invalide** — HUD FMR lookup par ZIP peut échouer pour certains codes postaux ruraux. Mitigation : flag `FMR_UNAVAILABLE`, propriété conservée dans le pipeline avec les données Redfin disponibles uniquement.
- **Pas de vérification titre/hypothèque** — ImmoPilot ne vérifie pas les charges existantes (liens, arriérés de taxes). Hors scope MVP — responsabilité du due diligence post-qualification.

### Données & Vie Privée

- **3 utilisateurs internes** — Aucun accès tiers au système. Aucune donnée personnelle stockée. Azure SQL contient uniquement des données immobilières publiques et des logs de pipeline.
- **Numéros WhatsApp** — Stockés dans `appsettings.json`. Ce fichier ne doit **jamais** être commis dans le repo GitHub (`.gitignore` obligatoire).

## Innovation & Novel Patterns

### Detected Innovation Areas

**1. HUD FMR comme signal d'investissement (combinaison inédite)**

L'intégration de l'API HUD FMR (`huduser.gov/hudapi/public/fmr`) dans un pipeline d'analyse immobilière investisseur est sans précédent commercial. Les données Section 8 existent depuis des décennies pour la politique de logement — ImmoPilot est le premier outil à les retourner en signal d'investissement DSCR. La logique : si une propriété yield au taux conservateur du gouvernement, elle yield dans tous les scénarios de marché.

**2. Biais conservateur délibéré (contre-paradigme industrie)**

L'industrie proptech standard (Zillow Zestimate, PropStream, DealMachine) maximise les estimations de loyer pour montrer le meilleur rendement possible. ImmoPilot fait l'inverse : FMR au percentile 40, hypothèse 20% down, rehab configuré par défaut à 10%. Un deal qui passe ces filtres est un deal qui passe dans la réalité. Zéro faux positifs. L'innovation n'est pas technique — c'est épistémique.

**3. Human-in-the-loop par design (déni de l'IA décisionnelle)**

En 2025, la tendance est aux pipelines IA qui décident à la place des humains. ImmoPilot fait le choix inverse : l'outil ne score pas, ne recommande pas, ne priorise pas au-delà du filtre binaire pass/fail. Il remet les humains en position de décision rapide avec le contexte nécessaire. L'instinct de Linh — inexplicable algorithmiquement — est une feature, pas un bug. C'est le positionnement différenciant face à toute solution ML-heavy.

**4. Infrastructure $5/mois comme modèle de viabilité**

GitHub Actions (cron gratuit) + Azure SQL Basic ($5/mois) : un pipeline institutionnel-grade pour le coût d'un café. La viabilité économique dès le premier deal est une innovation de modèle. PropStream ($99-199/mois) exige un volume de deals pour justifier son coût. ImmoPilot est rentable à zéro deal.

### Market Context & Competitive Landscape

| Outil | Loyer estimé | Marchés | Coût/mois | DSCR Ohio calibré |
|-------|-------------|---------|-----------|-------------------|
| PropStream | Comparables marché (haussiers) | US générique | $99–199 | Non |
| DealMachine | Zillow Zestimate | US générique | $49–99 | Non |
| Zillow | Zestimate propriétaire | US | Gratuit/payant | Non |
| **ImmoPilot** | **HUD FMR Section 8 (conservateur)** | **5 marchés Ohio ciblés** | **$5** | **Oui** |

Aucun concurrent n'adresse simultanément : (1) loyer conservateur HUD, (2) calibration marchés secondaires Ohio, (3) trio humain complémentaire, (4) coût < $10/mois.

### Validation Approach

- **HUD FMR comme proxy** : Valider en comparant FMR 2024 aux loyers Zillow/Rentometer sur les 5 marchés Ohio → FMR est systématiquement 10–20% sous le marché. Biais conservateur confirmé.
- **Modèle DSCR** : Tester sur les deals historiques de Philippe (1er deal existant) — est-ce qu'ImmoPilot l'aurait identifié ? Calibrer les seuils si écart significatif.
- **Volume de deals** : KPI clé à 3 mois = ≥ 3 propriétés qualifiées/semaine. Si 0 deals qualifiés, les seuils CAP/CoC sont trop restrictifs → réduire `MinCapRate` à 7.5% en test.

### Risk Mitigation

| Risque innovation | Probabilité | Mitigation |
|------------------|------------|-----------|
| Redfin bloque l'API | Moyen | Rotation user-agent, backoff exponentiel ; plan B : scraping Zillow |
| HUD FMR trop conservateur = 0 deals qualifiés | Faible | `MinCapRate` configurable, calibrage sur données historiques |
| Linh quitte l'équipe | Très faible | Outil conçu pour 3 décisionnaires — fonctionne à 2 |
| Azure SQL change de pricing | Très faible | Données < 1 GB, migration vers SQLite locale possible en 1 heure |

## CLI Tool Specific Requirements

### Project-Type Overview

ImmoPilot est un pipeline d'automatisation scriptable (non-interactif) exécuté via GitHub Actions cron et/ou manuellement via `dotnet run`. L'interface utilisateur principale est le digest WhatsApp — le CLI lui-même n'a pas d'interface interactive. La configuration est entièrement déclarative via `appsettings.json`.

### Command Structure

| Commande | Description |
|----------|-------------|
| `dotnet run` | Exécution pipeline complète : scan Redfin → enrichissement HUD FMR → calcul DSCR → déduplication → envoi WhatsApp |
| `dotnet run --dry-run` | Scan + calcul complets, **sans envoi WhatsApp** — pour tests et validation en développement |
| `dotnet run -- --dry-run` | Variante selon le runner .NET utilisé |

Pas de shell completion nécessaire (outil interne, 3 utilisateurs techniques).

**GitHub Actions triggers :**
- `schedule` : cron `"0 4 * * *"` (04:00 UTC = ~23:00 EST, résultats disponibles avant 8h00 EST)
- `workflow_dispatch` : déclenchement manuel depuis l'interface GitHub — relance sans modifier le code

### Output Formats

**Console / stdout (structured logs) :**
```
[2026-02-22 04:02:11 UTC] INFO  Pipeline started — Run #248
[2026-02-22 04:02:15 UTC] INFO  Redfin: 52 properties fetched (Springfield:11, Dayton:14, Akron:9, Cleveland:12, Toledo:6)
[2026-02-22 04:03:41 UTC] WARN  HUD FMR: ZIP 45502 not found — property marked FMR_UNAVAILABLE
[2026-02-22 04:05:02 UTC] INFO  DSCR Analysis: 4 qualified / 52 scanned (7.7%)
[2026-02-22 04:05:03 UTC] INFO  Deduplication: 1 already seen — 3 new properties to notify
[2026-02-22 04:05:18 UTC] INFO  WhatsApp: digest sent to 3 recipients
[2026-02-22 04:05:19 UTC] INFO  Pipeline completed — Duration: 3m08s — Exit: 0
```

**WhatsApp digest (format utilisateur) :**
```
🏠 ImmoPilot — 22 fév 2026
3 propriétés qualifiées sur 52 scannées

1. 📍 123 Main St, Springfield OH
   Prix: $127,500 | CAP: 9.1% | CoC: 14.3%
   Cash Flow: $380/mois | Cash req: $29,500
   FMR 2024: $820/unité (2BR)

2. ⚠️ WARNING (5/6) — 456 Oak Ave, Dayton OH
   Prix: $98,000 | CAP: 8.7% | CoC: 11.8%*
   *CoC sous seuil 12% — à valider

3. [...]

⚠️ Chiffres estimatifs — validation terrain requise.
```

**Azure SQL :** stockage persistant pour déduplication et historique (pas d'export MVP).

### Config Schema

`appsettings.json` (structure complète, jamais commité dans Git) :

```json
{
  "Pipeline": {
    "MaxPropertiesPerDigest": 5,
    "DryRun": false,
    "CronDescription": "Géré par GitHub Actions — ne pas modifier ici"
  },
  "Markets": {
    "TargetCities": ["Springfield", "Dayton", "Akron", "Cleveland", "Toledo"],
    "PropertyTypes": [2, 3],
    "MinPriceUsd": 100000
  },
  "DscrThresholds": {
    "MinCapRate": 8.0,
    "MinCocRate": 12.0,
    "MaxRehabPercent": 20.0,
    "DownPaymentPercent": 20.0,
    "DefaultRehabPercent": 10.0,
    "WarningRehabPercent": 15.0
  },
  "Notifications": {
    "WhatsAppRecipients": ["+1514XXXXXXX", "+1514XXXXXXX", "+1514XXXXXXX"],
    "AlertRecipient": "+1514XXXXXXX",
    "AlertOnFailure": true
  },
  "Apis": {
    "RedfinBaseUrl": "https://www.redfin.com/stingray/api",
    "HudFmrBaseUrl": "https://www.huduser.gov/hudapi/public/fmr",
    "RetryCount": 3,
    "RetryDelaysSeconds": [1, 2, 4]
  },
  "Database": {
    "ConnectionString": "Server=...;Database=ImmoPilot;..."
  }
}
```

`appsettings.Development.json` (overrides locaux, gitignored) :
```json
{
  "Pipeline": { "DryRun": true },
  "Markets": { "TargetCities": ["Springfield"] }
}
```

### Scripting Support

**Exit codes (utilisés par GitHub Actions pour marquer le job) :**

| Code | Signification | Comportement GitHub Actions |
|------|--------------|----------------------------|
| `0` | Succès complet | Job vert ✅ |
| `1` | Succès partiel (ex: HUD FMR indisponible pour certains ZIPs, WhatsApp envoyé) | Job orange / warning |
| `2` | Échec total (Redfin inaccessible, aucune propriété traitée, WhatsApp non envoyé) | Job rouge ❌ + alerte WhatsApp à Philippe |

**Idempotence :** relancer le pipeline le même jour ne re-notifie pas les propriétés déjà envoyées (déduplication Azure SQL par MLS ID + date d'envoi).

**`workflow_dispatch` inputs (optionnel v1.1) :** paramètre `dry_run: boolean` pour déclencher en mode dry-run depuis l'interface GitHub sans modifier le JSON.

## Project Scoping & Phased Development

### MVP Strategy & Philosophy

**Approche MVP :** Problem-solving MVP — prouver que le pipeline génère un deal flow suffisant pour justifier une relation PM dédiée. Le MVP ne cherche pas à impressionner les investisseurs externes : il brise le cercle vicieux "pas de deals → pas de PM → pas de deals" pour l'équipe interne de 3 personnes.

**Ressources MVP :** 1 développeur (Philippe), développement en dehors des heures de travail. Infrastructure : ~$5/mois. Aucune dépendance externe pour le lancement.

**Critère de succès MVP :** ImmoPilot est viable dès qu'un premier meeting "PM potentiel" est déclenché par une alerte — toutes les fonctionnalités listées dans *Product Scope — MVP v1.0* sont nécessaires et suffisantes pour ce moment.

### Risk Mitigation Strategy

| Risque | Impact | Probabilité | Mitigation |
|--------|--------|------------|-----------|
| Redfin bloque l'IP | Pipeline mort | Moyen | Rotation user-agent, backoff ; plan B Zillow |
| HUD FMR retourne 0 résultats Ohio | Analyse incomplète | Faible | Fallback FMR_UNAVAILABLE, propriété conservée |
| 0 deals qualifiés semaine 1 | Démoralisation équipe | Moyen | Seuils initiaux 8%/12% — réduire à 7.5%/10% si besoin |
| Financement 2e deal indisponible | Blocage business | Moyen | Hors contrôle ImmoPilot — risque business externe |
| GitHub Actions gratuit limite atteinte | Pipeline ralenti | Très faible | 2000 min/mois, scan ≤ 30 min = 40 runs/mois gratuits |

## Functional Requirements

### Property Discovery

- FR1: Le pipeline peut récupérer les listings duplex/triplex actifs sur Redfin pour chaque ville cible configurée dans `appsettings.json`
- FR2: Le pipeline peut filtrer les listings selon les critères : type de propriété (duplex=2, triplex=3), prix minimum, et statut actif (exclut PENDING/sous-contrat)
- FR3: Le pipeline peut récupérer le loyer HUD FMR Section 8 par code postal (ZIP) via l'API `huduser.gov/hudapi/public/fmr` pour chaque listing Redfin éligible

### Financial Qualification

- FR4: Le pipeline peut calculer le CAP rate pour chaque propriété (revenu net annuel / prix d'achat, avec loyer = FMR × nombre d'unités)
- FR5: Le pipeline peut calculer le Cash-on-Cash (CoC) return (cash flow annuel net / cash investi, avec 20% down payment configurable)
- FR6: Le pipeline peut calculer le Cash Flow mensuel net (loyers FMR – charges estimées – remboursement hypothèque)
- FR7: Le pipeline peut calculer le Cash Required (mise de fonds + rehab estimé)
- FR8: Le pipeline peut appliquer les seuils de qualification DSCR configurables (CAP ≥ MinCapRate, CoC ≥ MinCocRate, Rehab ≤ MaxRehabPercent) pour produire un résultat pass/fail binaire
- FR9: Le pipeline peut générer un flag WARNING pour les propriétés passant 5 critères sur 6 (à la frontière des seuils)

### Data Persistence & Deduplication

- FR10: Le pipeline peut stocker chaque propriété analysée dans Azure SQL avec toutes les métriques calculées (prix, CAP, CoC, Cash Flow, Cash Required, FMR, statut qualification)
- FR11: Le pipeline peut détecter les propriétés déjà notifiées via déduplication par MLS ID + date d'envoi pour éviter les re-notifications
- FR12: Le pipeline peut marquer une propriété `FMR_UNAVAILABLE` quand le lookup HUD FMR échoue pour un ZIP code, tout en conservant la propriété dans le pipeline avec les données Redfin disponibles
- FR13: Le pipeline peut marquer une propriété `PENDING` quand Redfin indique un statut sous-contrat ou vendu, et l'exclure du digest WhatsApp

### Notification & Alerting

- FR14: Le pipeline peut composer un digest WhatsApp quotidien contenant les propriétés nouvellement qualifiées (maximum `MaxPropertiesPerDigest` propriétés, configurable)
- FR15: Le digest peut présenter chaque propriété avec les 5 données clés : adresse, prix, CAP rate, CoC rate, Cash Flow mensuel, Cash Required, et année FMR
- FR16: Le digest peut inclure un flag et note explicative pour les propriétés WARNING (5/6 critères)
- FR17: Le digest peut inclure le disclaimer légal (`Chiffres estimatifs — validation terrain requise avant toute offre`)
- FR18: Le pipeline peut envoyer le digest WhatsApp simultanément à tous les destinataires configurés dans `WhatsAppRecipients`
- FR19: Le pipeline peut envoyer une alerte WhatsApp dédiée à `AlertRecipient` (Philippe) en cas d'échec du pipeline (exit code 2), incluant le type d'erreur, le nombre de retries épuisés, et le Run ID

### Configuration Management

- FR20: L'opérateur peut modifier les villes cibles, types de propriétés, et prix minimum via `TargetCities`, `PropertyTypes`, `MinPriceUsd` dans `appsettings.json` sans modifier ni redéployer le code
- FR21: L'opérateur peut modifier les seuils DSCR (MinCapRate, MinCocRate, MaxRehabPercent, DownPaymentPercent, DefaultRehabPercent) via `appsettings.json` avec effet au prochain run cron
- FR22: L'opérateur peut modifier les destinataires WhatsApp et les paramètres d'alerte via `Notifications` dans `appsettings.json`
- FR23: L'opérateur peut activer le mode dry-run (`DryRun: true`) pour exécuter le scan et calcul complets sans envoi WhatsApp, via `appsettings.json` ou argument CLI `--dry-run`
- FR24: L'opérateur peut configurer les paramètres de retry API (nombre de tentatives, délais backoff) via `RetryCount` et `RetryDelaysSeconds` dans `appsettings.json`

### Pipeline Operations

- FR25: Le pipeline peut s'exécuter automatiquement chaque nuit via GitHub Actions `schedule` cron (04:00 UTC), sans intervention humaine
- FR26: Le pipeline peut être déclenché manuellement via GitHub Actions `workflow_dispatch` depuis l'interface GitHub web (relance idempotente)
- FR27: Le pipeline peut produire des logs structurés horodatés (UTC) sur stdout avec niveaux INFO/WARN/ERROR, incluant Run ID, stats par ville, et durée d'exécution
- FR28: Le pipeline peut réessayer automatiquement chaque appel API échoué jusqu'à `RetryCount` fois avec backoff exponentiel configurable
- FR29: Le pipeline peut s'exécuter de manière idempotente : relancer le même jour ne re-notifie pas les propriétés déjà envoyées ce jour
- FR30: Le pipeline peut retourner les exit codes standardisés : 0 (succès complet), 1 (succès partiel, ex: FMR indisponible sur certains ZIPs), 2 (échec total, ex: Redfin inaccessible)

## Non-Functional Requirements

### Performance

- Le pipeline complet (Redfin scraping → HUD FMR enrichissement → DSCR calcul → déduplication → WhatsApp envoi) s'exécute en ≤ 30 minutes sur GitHub Actions pour respecter la limite gratuite (2000 min/mois, 40 runs/mois maximum)
- Le digest WhatsApp est livré avant 8h00 heure locale Ohio (GMT-5/GMT-4) — garanti par le cron 04:00 UTC avec ≤ 30 min d'exécution
- Chaque appel API individuel a un timeout de 10 secondes pour éviter les blocages silencieux

### Reliability

- Disponibilité pipeline : ≥ 99% (≤ 3 pannes non planifiées par an)
- Tout échec API est retryé automatiquement 3 fois avec backoff exponentiel (1s, 2s, 4s) avant d'être déclaré échoué
- Une panne partielle (ex: HUD FMR indisponible pour certains ZIPs) n'interrompt pas le pipeline — les propriétés affectées sont marquées `FMR_UNAVAILABLE` et le pipeline continue
- En cas d'échec total (exit code 2), une alerte WhatsApp automatique est envoyée à Philippe avant qu'il découvre l'absence de digest
- Les relances manuelles sont idempotentes : aucune duplication de données ni de re-notifications

### Integration Resilience

- API Redfin (non-officielle) : tolérance aux changements de structure de réponse via désérialisation défensive, rotation du User-Agent pour réduire le throttling
- API HUD FMR (publique gouvernementale) : données mises à jour annuellement — l'année FMR est incluse dans chaque digest pour signaler le potentiel retard de 0 à 12 mois
- Azure SQL : connexion poolée avec retry automatique sur `SqlException` transientes ; aucune propriété n'est perdue en cas d'échec de connexion isolé

### Maintainability & Testability

- Couverture de tests unitaires ≥ 80% sur `PropertyAnalyzer` (moteur de calcul DSCR)
- Tests BDD avec SpecFlow/Reqnroll couvrant les cas limites critiques : propriétés à la frontière des seuils, valeurs FMR manquantes, rehab 0% et rehab > 100%, exit codes corrects
- Tous les paramètres métier (seuils, villes, délais) sont configurables via `appsettings.json` — aucun magic number dans le code
- Les logs structurés (Run ID, timestamps UTC, stats par ville) permettent le diagnostic post-incident sans accès à la machine de production

### Security

- `appsettings.json` (numéros WhatsApp, connection string Azure SQL, secrets API) est explicitement listé dans `.gitignore` et ne doit jamais être commité dans le repo GitHub
- `appsettings.Development.json` (overrides locaux) est également gitignored
- Les secrets GitHub Actions (WhatsApp API token, Azure SQL connection string) sont stockés dans GitHub Secrets, pas dans le YAML de workflow

