---
stepsCompleted: [1, 2, 3]
session_continued: true
continuation_date: '2026-02-22'
session_completed: true
inputDocuments: []
session_topic: 'Système automatisé de recherche et analyse de propriétés immobilières en Ohio'
session_goals: 'Trouver ~5 propriétés/jour correspondant à des critères définis, contourner les protections anti-bot, analyser automatiquement (cap rate, cash-on-cash), alerter sur les propriétés éligibles, historique anti-doublons'
selected_approach: 'ai-recommended'
techniques_used: ['Question Storming', 'First Principles Thinking', 'Solution Matrix']
ideas_generated: []
context_file: '_bmad/bmm/data/project-context-template.md'
---

# Brainstorming Session Results

**Facilitator:** Philippe
**Date:** 2026-02-20

## Session Overview

**Topic:** Système automatisé de recherche et analyse de propriétés immobilières en Ohio
**Goals:** Trouver ~5 propriétés/jour correspondant à des critères définis (zone, prix max, caractéristiques), contourner les protections anti-bot des sites comme Zillow, analyser automatiquement chaque propriété trouvée (taux de capitalisation, cash-on-cash), alerter quand une propriété répond aux critères d'investissement, garder un historique pour éviter de réévaluer les mêmes propriétés

### Context Guidance

Contexte projet chargé: focus sur les problèmes utilisateurs, idées de fonctionnalités, approches techniques, expérience utilisateur, modèle d'affaires, différenciation marché, risques techniques et métriques de succès. Les résultats alimenteront potentiellement un Product Brief, un PRD et une architecture technique.

### Session Setup

**Approche choisie:** Techniques recommandées par l'IA
**Langue:** Français

## Sélection des Techniques

**Approche :** Recommandations IA
**Contexte d'analyse :** Système automatisé de recherche immobilière — contraintes anti-bot, analyse financière, alertes

**Techniques sélectionnées :**

- **Question Storming** *(Analyse Profonde)* : Cartographier toutes les bonnes questions avant de chercher des solutions — révèle les angles morts et les hypothèses cachées dans un système multi-composantes
- **First Principles Thinking** *(Innovation Créative)* : Déconstruire les hypothèses de base (ex: "on doit obligatoirement scraper Zillow") pour reconstruire à partir des vérités fondamentales — ouvre des alternatives non évidentes
- **Solution Matrix** *(Pensée Structurée)* : Grille systématique des approches par composante (acquisition, analyse, alertes, stockage) — identifie les combinaisons optimales

**Rationale IA :** Séquence conçue pour aller de la définition précise du problème → à la génération d'idées disruptives → à la cartographie structurée des solutions, en tenant compte de la nature technique et business du projet.

---

## Technique 1 : Question Storming — Résultats

### Profil du projet — Ce qu'on a découvert

**Propriétés cibles :**
- Type : duplex / triplex (single family écarté — rendement insuffisant vs. prix)
- Prix : $100k minimum (contrainte DSCR) — Columbus souvent trop cher
- Villes Ohio : Springfield, Dayton, Akron, Cleveland, Toledo
- Critères d'exclusion à définir : année de construction, état apparent, quartiers

**Modèle financier — Variables et sources :**

| Variable | Source | Automatisable |
|---|---|---|
| Prix demandé | Listing | ✅ Oui |
| Taxes foncières | Listing (souvent fourni) | ✅ Oui |
| Loyer estimé | Taux Section 8 HUD par code postal | ✅ Oui (API HUD) |
| Maintenance | 10% du loyer | ✅ Calcul |
| Property management | 10% du loyer | ✅ Calcul |
| Vacance | 8% du loyer | ✅ Calcul |
| Assurance | Référence 1ère acquisition | ⚠️ Estimation fixe |
| Utilities | Listing si disponible, sinon défaut | ⚠️ Semi-auto |
| Rehab cost | PM après visite terrain | ❌ Non automatisable en amont |

**Résultat : ~85% du modèle financier automatisable**

**Critères de décision :**
- CAP RATE ≥ 8%
- Cash-on-Cash ≥ 12%
- Rehab cost ≤ 20% du prix d'achat
- Cash flow, Cash required, Mortgage amount limit → seuils à préciser
- 5/6 critères → alerte avec ⚠️ WARNING (pas rejet automatique)

**Alertes & historique :**
- Canal : WhatsApp ou Microsoft Teams
- Fréquence : digest quotidien
- Historique : perpétuel, déduplication par adresse
- Propriété rejetée + changement de prix → réévaluation à envisager

**Flux opérationnel :**
- Phase 1 : alertes → Philippe → contact PM pour visite
- Phase 2 : alertes → PM directement
- Excel remplacé par le système

**Profil technique :**
- Développeur : C# / Java (construction en propre)
- Fraîcheur des données : hebdomadaire acceptable pour commencer
- Données minimales par listing : adresse, prix, taxes, nb chambres/unités

### Questions ouvertes (décisions à prendre)

1. Seuils exacts pour Cash flow, Cash required, Mortgage amount limit
2. Critères d'exclusion : année de construction minimum? Mots-clés ("as-is", "needs work" → rehab élevé?)
3. Déduplication cross-sources : même propriété sur Redfin ET Zillow → une seule entrée
4. Réévaluation automatique si prix change sur propriété historique rejetée?
5. Format exact du rapport envoyé au PM

### Insight clé

**Le vrai problème de données n'est pas "contourner Zillow" mais "obtenir des listings fiables pour l'Ohio (duplex/triplex) par code postal, gratuitement, avec une fraîcheur hebdomadaire".** Zillow n'est peut-être ni la seule ni la meilleure route. Piste identifiée : Redfin API non officielle, county auditor records Ohio, MLS public.

---

## Technique 2 : First Principles Thinking — En cours

### Hypothèse #1 déconstruite

> *"Pour trouver des propriétés à vendre en Ohio, il faut scraper des portails immobiliers comme Zillow."*

**Vérité fondamentale :** En Ohio (disclosure state), toute transaction et tout listing est une donnée publique. Les county auditors publient ces données en ligne. Le problème est donc : *obtenir des listings fiables pour le type duplex/triplex dans les villes cibles, gratuitement, fraîcheur hebdomadaire.*

**5 alternatives identifiées :**

- **[A] County Auditor sites** — Cuyahoga/Cleveland, Montgomery/Dayton, Summit/Akron, Lucas/Toledo, Clark/Springfield publient des données propriétés en ligne. Service public = pas d'anti-bot.
- **[B] Redfin API non officielle** — Endpoints non documentés utilisés par l'app mobile Redfin, accessibles via `HttpClient` C#. Bien moins protégé que Zillow.
- **[C] Alertes email Realtor.com → parsing IMAP** — Créer des alertes de recherche sur Realtor.com, lire les emails entrants automatiquement via IMAP ou Microsoft Graph API. Zéro scraping, zéro anti-bot.
- **[D] Facebook Marketplace** — Nombreux FSBO Ohio, politique anti-bot plus souple, opportunités off-market.
- **[E] MLS via agent partenaire** — Si le PM a une licence d'agent, accès direct MLS → export hebdomadaire ou portail IDX partagé.

### Hypothèses déconstruites — Résultats complets

**[H1] "Il faut scraper Zillow"**
→ Faux. Le vrai besoin est : listings duplex/triplex Ohio, gratuit, fraîcheur hebdomadaire.
→ Alternatives : Redfin API non officielle (C# HttpClient), County Auditor sites publics, Alertes email Realtor.com → IMAP parsing, Facebook Marketplace (off-market), MLS via PM agent

**[H2] "L'analyse financière nécessite Excel"**
→ Faux. C'est un calcul pur sur variables connues. 85% automatisable immédiatement.
→ Seul Rehab cost non automatisable → valeur forfaitaire conservatrice par défaut (ex: 10% prix)

**[H3] "Il faut une infrastructure coûteuse"**
→ Faux. Besoin réel : ~30 min d'exécution/jour. Coût = quasi zéro.
→ Phase 1 : machine locale + Windows Task Scheduler + SQLite ($0/mois)
→ Phase 2 : GitHub Actions ou Azure Functions consumption plan (< $1/mois)

**[H4] "Azure coûte cher"**
→ Contexte corrigé : AKS/Azure SQL = infrastructure travail, non disponible en perso.
→ Plan B validé : local machine suffit largement pour commencer

**[H5] "Les données HUD nécessitent saisie manuelle"**
→ Faux. API REST publique gratuite HUD : `huduser.gov/hudapi/public/fmr`
→ Code postal → loyers par nb chambres en JSON. Un seul HttpClient call C#.

**[H6] "Facebook Marketplace est trop difficile à scraper"**
→ Faux. Anti-bot moins agressif que Zillow. Microsoft.Playwright (.NET) simule navigateur réel.
→ Bonus stratégique : propriétés off-market = zéro compétition avec autres investisseurs.

### Architecture candidate émergente

```
CronJob (Task Scheduler / GitHub Actions)
  ↓
Scraper C# [Redfin API + Facebook Marketplace Playwright]
  ↓
Enrichissement [HUD API → loyer Section 8 par ZIP]
  ↓
Moteur d'analyse [calcul CAP rate, CoC, Cash flow, etc.]
  ↓
SQLite [historique + déduplication par adresse]
  ↓
Filtre critères [8% CAP / 12% CoC / Rehab ≤20%]
  ↓
Notification [WhatsApp API / Teams Webhook] → digest quotidien
```

---

## Technique 3 : Solution Matrix — Résultats

| # | Composante | Choix retenu | Justification |
|---|---|---|---|
| 1 | Sources de données | Redfin API non officielle + Facebook Marketplace (Playwright) | Combinaison MLS structuré + off-market, gratuit, C# natif |
| 2 | Loyer estimé | API HUD FMR (huduser.gov/hudapi/public/fmr) | Gratuite, publique, par code postal, mise à jour annuelle automatique |
| 3 | Analyse financière | Moteur C# pur — classe `PropertyAnalyzer` | Calcul pur, testable, maintenable, remplace Excel |
| 4 | Stockage & historique | Azure SQL (Basic ~$5/mois) | Durabilité cloud, accessible GitHub Actions, historique perpétuel |
| 5 | Déduplication | Adresse normalisée comme clé primaire | Cross-sources, détecte changements de prix sur propriétés rejetées |
| 6 | Notifications | WhatsApp Business API (gratuit ≤1000 msg/mois) | Mobile-first, digest quotidien, partage facile avec PM |
| 7 | Infrastructure | GitHub Actions (cron gratuit) | Zéro machine locale, logs intégrés, secrets gérés, gratuit |

**Stack final : C# .NET · Azure SQL · GitHub Actions · WhatsApp Business API**
**Coût mensuel estimé : ~$5/mois**

---

## Rapport de Synthèse Final

### Vision du système

Un agent automatisé de recherche et d'analyse immobilière qui tourne chaque nuit, trouve des duplex/triplex à fort rendement en Ohio, calcule leur performance financière, et alerte l'équipe via WhatsApp uniquement sur les opportunités qualifiées.

### Problème résolu

Élimination du goulot d'étranglement manuel : recherche quotidienne sur le web, saisie dans Excel, calcul des indicateurs, tri des résultats. Le système fait tout ça automatiquement, à coût quasi nul.

### Architecture finale

```
GitHub Actions CronJob (nuit, heure Canada)
  ↓
[Scraper 1] Redfin API HttpClient → duplex/triplex Ohio par ZIP
[Scraper 2] Playwright Facebook Marketplace → FSBO off-market Ohio
  ↓
Déduplication → Azure SQL (adresse normalisée, déjà vu?)
  ↓
[Enrichissement] API HUD → loyer Section 8 par code postal
  ↓
[PropertyAnalyzer C#]
  - Purchase Price, Taxes (listing)
  - Rent (HUD FMR), Insurance (fixe), Utilities (listing/défaut)
  - Maintenance 10%, PropMgmt 10%, Vacancy 8%
  - Mortgage (DSCR 20% down, taux marché)
  - Rehab (forfait 10% prix par défaut)
  → CAP Rate / Cash-on-Cash / Cash Flow / Cash Required
  ↓
[Filtre décision]
  - CAP RATE ≥ 8% ✅
  - Cash-on-Cash ≥ 12% ✅
  - Rehab ≤ 20% prix achat ✅
  - Cash flow, Mortgage limit (à définir) ✅
  - 5/6 critères → alerte avec ⚠️ WARNING
  ↓
Azure SQL → historique perpétuel (rejet + raison + date)
  ↓
WhatsApp Business API → digest quotidien à Philippe
```

### Villes et propriétés ciblées

- **Type :** Duplex / Triplex
- **Prix :** $100k minimum (contrainte DSCR)
- **Villes :** Springfield, Dayton, Akron, Cleveland, Toledo
- **Exclus :** Columbus (prix trop élevé)

### Décisions restantes à prendre (Product Brief)

1. Seuils Cash flow minimum et Mortgage amount limit
2. Critères d'exclusion automatique (mots-clés "as-is", "needs work", année construction)
3. Seuil de prix déclenchant l'envoi au PM (quand une opportunité mérite une visite)
4. Format exact de la notification WhatsApp (résumé court vs. rapport détaillé)
5. Stratégie de réévaluation des propriétés rejetées si le prix baisse

### Avantage compétitif identifié

**Facebook Marketplace off-market** : propriétés FSBO Ohio invisibles sur Zillow/Redfin = zéro compétition avec les autres investisseurs qui utilisent les portails classiques.

### Prochaines étapes recommandées

1. **[CB] Créer un Brief** — Formaliser la vision en Product Brief exécutif
2. **[CP] Créer un PRD** — Détailler les exigences fonctionnelles et techniques
3. **[CA] Créer l'Architecture** — Schéma technique détaillé, modèle de données Azure SQL, structure C# du projet

---

*Session de brainstorming complétée le 2026-02-22 | Facilitation : Mary (Analyste Business BMAD)*
