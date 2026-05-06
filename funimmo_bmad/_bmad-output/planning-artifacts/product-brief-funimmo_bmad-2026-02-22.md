---
stepsCompleted: [1, 2, 3, 4, 5, 6]
inputDocuments: ['_bmad-output/brainstorming/brainstorming-session-2026-02-20.md', '_bmad-output/brainstorming/953 OAK ST.xlsx']
date: '2026-02-22'
author: 'Philippe'
---

# Product Brief: funimmo_bmad

<!-- Content will be appended sequentially through collaborative workflow steps -->

## Executive Summary

ImmoPilot est un système d'intelligence immobilière automatisé, conçu pour transformer la recherche manuelle de propriétés en un pipeline industriel de deals qualifiés. En scannant chaque nuit les marchés secondaires de l'Ohio (duplex/triplex, $100k–$300k), ImmoPilot calcule automatiquement les métriques DSCR, CAP rate et Cash-on-Cash selon les loyers Section 8 HUD, et livre uniquement les opportunités qui passent les seuils d'investissement définis.

Son objectif immédiat : générer le volume de deals qualifiés nécessaire pour justifier l'engagement d'un Property Manager dédié, et constituer un flux d'opportunités structurables en offres pour investisseurs externes à rendement modéré et risque défini.

---

## Core Vision

### Problem Statement

Un investisseur immobilier en phase de démarrage en Ohio fait face à un cercle vicieux : sans volume de deals analysés, il ne peut pas attirer l'attention de son Property Manager ni démontrer la viabilité de son modèle à des investisseurs externes. La recherche manuelle (web + Excel + calcul) prend des heures par jour pour produire quelques pistes non qualifiées. Ce goulot d'étranglement bloque la croissance avant même que le premier réel pipeline soit établi.

### Problem Impact

- **Court terme** : 0-2 deals/an faute de flux suffisant
- **Moyen terme** : Incapacité à démontrer le track record aux investisseurs
- **Long terme** : Impossibilité de constituer un portefeuille multi-propriétés auto-finançable
- **Coût d'opportunité** : Les marchés Ohio secondaires (Springfield, Dayton) offrent des CAP rates 2-3x supérieurs aux marchés primaires — fenêtre qui se referme avec la hausse des prix

### Why Existing Solutions Fall Short

- **Property Manager humain** : Trop occupé, prioritise ses clients à fort volume
- **PropStream / DealMachine** ($99-199/mois) : Générique US, non calibré Section 8 / DSCR Ohio, ne couvre pas Facebook Marketplace off-market, analyse financière superficielle
- **Excel manuel** : Long, non-scalable, dépendant de l'associé pour la collecte
- **Zillow / Redfin navigation manuelle** : Pas de filtrage financier intégré, pas d'alerte automatique sur critères personnalisés

### Proposed Solution

ImmoPilot : pipeline automatisé nightly en C# .NET exécuté sur GitHub Actions. Sources : Redfin API (listings structurés) + Facebook Marketplace Playwright (FSBO off-market). Enrichissement HUD FMR API (loyer Section 8 conservateur par ZIP). Moteur `PropertyAnalyzer` calculant CAP rate, CoC, Cash Flow, Cash Required. Déduplication Azure SQL. Alert WhatsApp digest quotidien.

**Mode 1 — Screening automatique** : découverte nightly des opportunités qualifiées
**Mode 2 — Simulation interactive** : ajustement post-identification (prix, taux, réhabilitation) pour structurer l'offre d'investissement

### Key Differentiators

1. **Calibration Section 8 / HUD FMR** : Estimation de loyer conservatrice par code postal — aucun outil générique ne l'intègre nativement
2. **Facebook Marketplace off-market** : Propriétés FSBO invisibles aux investisseurs utilisant les portails classiques = avantage compétitif réel
3. **Coût $5/mois** vs $99-199/mois pour PropStream — même niveau d'analyse avec critères précisément calibrés sur le modèle DSCR Ohio
4. **Brise le cercle vicieux** : Génère le volume qui attire le PM → qui valide les deals → qui attire les investisseurs externes
5. **Base analytique pour syndication** : Chaque deal qualifié peut être packagé en offre d'investissement structurée (rendement modéré, risque défini)

---

## Target Users

### Primary Users

#### Persona 1 — Philippe : L'Investisseur Architecte

**Profil :** Développeur C#/Java basé au Canada, co-fondateur de la structure d'investissement Ohio. Phase de démarrage actif : 1 deal closé, en construction de portefeuille. Combine compétences techniques et vision business. Investit plus d'efforts opérationnels que le deal initial ne le prévoyait, mais voit l'aventure comme un laboratoire d'apprentissage.

**Rôle dans ImmoPilot :** Récepteur principal des alertes, décisionnaire final, développeur et opérateur du système.

**Objectifs :**
- Recevoir chaque matin un digest des opportunités qualifiées sans lever le petit doigt
- Avoir les données suffisantes pour préparer le meeting hebdo avec Giancarlo
- Limiter la complexité technique (CLI d'abord, dashboard plus tard)

**Frustrations actuelles :**
- Recherche manuelle chronophage pour peu de résultats qualifiés
- Dépendance sur l'associé pour le sourcing terrain
- Pas assez de volume pour démontrer la viabilité aux investisseurs externes

**Moment de succès :** Recevoir un WhatsApp le matin avec 3 propriétés dont une que Giancarlo n'a pas encore vue et qui passe tous les critères financiers.

---

#### Persona 2 — Giancarlo : L'Expert Terrain Gate-Keeper

**Profil :** Agent immobilier canadien, associé fondateur. Apporte expertise métier immobilier + connaissance fine des marchés (Canada + USA). Présent sur 2 deals canadiens précédents. Rôle initial : main d'œuvre terrain. Réalité : gate-keeper humain essentiel pour valider ou rejeter les opportunités identifiées par l'outil avant d'engager des ressources de visite.

**Rôle dans ImmoPilot :** Validation humaine post-alerte ("est-ce que je connais déjà cette propriété ? Y a-t-il des red flags terrain que l'algo ne voit pas ?")

**Objectifs :**
- Avoir les infos essentielles rapidement (adresse, photos, métriques clés)
- Pouvoir signaler en 2 minutes si une propriété mérite qu'on appelle le PM ou non
- Participer au meeting hebdo sur la base d'une sélection pré-filtrée

**Frustrations actuelles :**
- Doit évaluer des opportunités sans outil structuré
- Le volume de deals à examiner est insuffisant pour justifier une implication sérieuse

**Moment de succès :** Recevoir le même digest que Philippe et pouvoir dire "celui-là, je le connais pas encore — c'est une bonne piste" en moins de 5 min.

---

### Secondary Users

#### Linh : Co-Investisseur + Filtre Intuitif

**Profil :** Co-fondatrice de la structure. Investisseur actif dans les décisions clés. Possède un sens aigu de l'évaluation des deals — jugements intuitifs systématiquement justes sur la qualité d'une opportunité, indépendamment des métriques financières brutes.

**Rôle dans ImmoPilot :** Validation qualitative complémentaire. Son instinct capte ce que les ratios ne voient pas : potentiel de quartier, timing de marché, "feeling" sur le vendeur ou la situation. Intervient entre l'alerte et le meeting hebdo comme second filtre humain.

**Interaction avec ImmoPilot :** Reçoit le digest (même que Philippe et Giancarlo) → donne son ressenti rapide sur les propriétés avant le meeting. Sa contribution : pointer celles qui lui semblent prometteuses ou à éviter, sans justification analytique requise — le track record parle pour lui.

**Note produit :** À terme, son historique de jugements (deal sélectionné / rejeté vs réalité) pourrait alimenter un scoring qualitatif complémentaire aux métriques DSCR.

#### PM (Property Manager) : Partenaire Terrain

**Profil :** Gestionnaire immobilière en Ohio, en phase de rodage sur le premier deal. Process pas encore streamliné avec l'équipe. Reçoit les opportunités **après** la décision de visiter — pas d'accès direct au système.

**Interaction avec ImmoPilot :** En aval uniquement. Planifie et effectue les visites sur les propriétés que l'équipe a décidé d'approfondir.

#### Investisseurs Externes (futur)

**Profil :** Apporteurs de fonds. N'interagissent pas avec ImmoPilot. Reçoivent des offres d'investissement structurées produites à partir des analyses du système.

**Interaction avec ImmoPilot :** Aucune — bénéficiaires indirects de la qualité analytique du pipeline.

---

### User Journey

**1. Nuit — Scan automatique**
GitHub Actions déclenche ImmoPilot → scrape Redfin + Facebook Marketplace → calcule métriques → filtre selon critères → déduplication Azure SQL

**2. Matin — Alerte**
Philippe, Giancarlo et Linh reçoivent digest WhatsApp :
`"3 nouvelles propriétés qualifiées · Springfield x2 · Dayton x1"`

**3. Validation parallèle (async)**
- Giancarlo : connait-il la propriété ? Red flags terrain ?
- Linh : ressenti intuitif sur les opportunités

**4. Meeting hebdomadaire**
Philippe + Giancarlo + Linh passent en revue les sélections de la semaine → vote : quelles propriétés méritent contact vendeur + visite PM ?

**5. Outreach**
Contact vendeur / agent listing → visite acceptée → planification avec PM

**6. Visite + Simulation**
PM visite et rapporte (état, rehab estimé) → Philippe utilise le Mode 2 (simulation interactive) pour ajuster les hypothèses avec les vraies données terrain

**7. Décision finale**
- Go → structurer l'offre d'achat (financement DSCR, seller financing si applicable)
- No-go → enregistré dans Azure SQL avec raison de rejet, déduplication activée

---

## Success Metrics

### User Success Metrics

**Philippe & Équipe — Validation qualité :**
- Au moins 1 propriété/semaine sur laquelle Philippe, Giancarlo et Linh sont unanimement d'accord : "c'est un bon deal à approfondir"
- Taux de consensus équipe ≥ 50% des alertes envoyées au meeting hebdo (évite les faux positifs qui gaspillent le temps de Giancarlo)

**PM — Signal de validation externe :**
- Objectif qualitatif : Le Property Manager demande spontanément "comment vous identifiez ces propriétés ?" — indicateur que la qualité des deals soumis dépasse la moyenne du marché
- Délai moyen alerte → décision visite ≤ 7 jours

**Giancarlo — Pipeline utilisable :**
- Reçoit ≥ 3 propriétés qualifiées/semaine avec les 5 données clés (adresse, prix, CAP rate, CoC, lien listing)
- Peut ajuster les paramètres de filtrage (villes, prix min/max, seuils de critères) sans intervention de Philippe

---

### Business Objectives

**3 mois — Opérationnel :**
- Système fonctionnel 7j/7, scan nightly, alerte WhatsApp quotidienne
- ≥ 1 visite de propriété planifiée avec le PM sur la base d'une alerte ImmoPilot

**6 mois — Premier résultat tangible :**
- 2ème deal closé (objectif conditionnel à la disponibilité du financement)
- ≥ 5 propriétés soumises à visite depuis le lancement du système
- Au moins 1 opportunité identifiée sur Facebook Marketplace off-market (validation de la source non-conventionnelle)

**12 mois — Portefeuille en construction :**
- Base de données historique ≥ 500 propriétés scannées et évaluées
- Premières tendances saisonnières identifiables (prix par saison, volume listings par ville, variation CAP rates)
- Discussion avec au moins 1 investisseur externe sur la base d'un deal packagé

---

### Key Performance Indicators

| KPI | Cible | Fréquence de mesure |
|-----|-------|---------------------|
| Disponibilité système | 7j/7, ≤ 30 min/run | Quotidien (log GitHub Actions) |
| Propriétés scannées | ≥ 50/semaine toutes sources | Hebdomadaire |
| Propriétés qualifiées (alertes) | 3-10/semaine | Hebdomadaire |
| Taux de qualification | 5-20% des scannées | Mensuel |
| Deals soumis à visite | ≥ 1/mois | Mensuel |
| Taux de conversion visite→offre | > 0% à 6 mois | Mensuel |
| Latence alerte | Delivery WhatsApp avant 8h00 heure Canada | Quotidien |
| Taux de doublons | 0% (déduplication Azure SQL) | Hebdomadaire |

**Données historiques pour analyse de tendances :**
- Prix médian par ville par trimestre
- Volume de listings duplex/triplex par saison
- CAP rate moyen alerté vs CAP rate marché
- Raisons de rejet les plus fréquentes (prix, CAP, CoC, rehab)
- Propriétés re-qualifiées après baisse de prix

---

## MVP Scope

### Core Features (v1.0 — MVP)

**1. Scraper Redfin (source principale)**
- Appels API non-officielle Redfin via `HttpClient` C#
- Filtres : duplex/triplex, Ohio (Springfield, Dayton, Akron, Cleveland, Toledo)
- Prix min $100k, fraîcheur ≤ 7 jours
- Données extraites : adresse, prix, nb unités, taxes foncières, lien listing

**2. Enrichissement HUD FMR**
- Appel API publique `huduser.gov/hudapi/public/fmr` par code postal
- Loyer Section 8 estimé par nombre de chambres → loyer conservateur

**3. Moteur d'analyse — `PropertyAnalyzer` C#**
- Calcul : CAP rate, Cash-on-Cash, Cash Flow mensuel, Cash Required
- Variables fixes : Maintenance 10%, PropMgmt 10%, Vacancy 8%
- Rehab : forfait 10% prix par défaut
- Mortgage : DSCR, 20% down, taux marché configurable

**4. Filtre décision**
- CAP rate ≥ 8%
- Cash-on-Cash ≥ 12%
- Rehab ≤ 20% prix achat
- Alerte ⚠️ WARNING si 5/6 critères (near-miss)

**5. Déduplication — Azure SQL**
- Clé : adresse normalisée
- Historique : date, métriques calculées, raison de rejet
- Réévaluation automatique si prix baisse

**6. Notification WhatsApp**
- Digest quotidien avant 8h00 (heure Canada Est)
- Destinataires : Philippe, Giancarlo, Linh
- Format : adresse · prix · CAP · CoC · lien

**7. Infrastructure**
- GitHub Actions cron nightly
- `appsettings.json` pour paramètres configurables (villes, seuils, taux)
- Logs GitHub Actions pour monitoring

---

### Out of Scope for MVP

| Fonctionnalité | Raison du report | Phase cible |
|---|---|---|
| Facebook Marketplace (Playwright) | Complexité technique, fragilité | v1.1 |
| Mode Simulation interactif (Mode 2) | Post-identification, non-bloquant | v1.1 |
| Interface de configuration (UI) | `appsettings.json` suffit | v2.0 |
| Dashboard web | Vibe coding après validation MVP | v2.0 |
| Statistiques & tendances historiques | Requiert volume de données d'abord | v2.0 |
| Scoring qualitatif Linh | Requiert historique de jugements | v3.0 |
| Seller financing detection | Complexité d'analyse | v2.0 |

---

### MVP Success Criteria

Le MVP est considéré réussi quand :
- Tourne 7j/7 sans intervention manuelle pendant 4 semaines consécutives
- Génère ≥ 3 alertes qualifiées/semaine
- Au moins 1 alerte débouche sur une décision de visite PM
- Zéro doublon détecté en base (déduplication fonctionnelle)

---

### Future Vision

**v1.1 — Source off-market**
- Ajout Facebook Marketplace FSBO (Playwright .NET)
- Mode Simulation CLI interactif pour négociation post-visite

**v2.0 — Intelligence & Accessibilité**
- Dashboard web (vibe coding) pour revue visuelle des opportunités
- Statistiques historiques : tendances prix/saison par ville
- Interface de configuration sans modifier le code
- Seller financing signals (equity élevé, vendeur pressé)

**v3.0 — Portefeuille & Investisseurs**
- Suivi portefeuille existant (performance réelle vs projection)
- Génération automatique de mémo investisseur (deal packagé)
- Scoring qualitatif basé sur l'historique de jugements de Linh
- Extension géographique (autres états à fort rendement)
