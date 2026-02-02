# 06 — Système de Statistiques

> **Priorité**: P0 (MVP basique) / P1 (Stats avancées)
> **Statut**: Draft

---

## 1. Vue d'Ensemble

### 1.1 Objectif

Tracker et afficher les performances du joueur pendant et après chaque session d'entraînement.

### 1.2 Types de Stats

| Type | Description | Affichage |
|------|-------------|-----------|
| **Live Stats** | Mises à jour en temps réel | HUD in-game |
| **Session Stats** | Calculées à la fin | Results screen |
| **Historical Stats** | Agrégées sur le temps | Future (V2) |

---

## 2. Métriques Core

### 2.1 Métriques de Base

| Métrique | Description | Formule |
|----------|-------------|---------|
| **Score** | Nombre de cibles touchées | Count(Hits) |
| **Total Shots** | Nombre de tirs effectués | Count(Shoots) |
| **Total Hits** | Nombre de tirs réussis | Count(Hits) |
| **Total Misses** | Nombre de tirs ratés | TotalShots - TotalHits |
| **Headshots** | Nombre de headshots | Count(Headshots) |
| **Bodyshots** | Nombre de bodyshots | TotalHits - Headshots |

### 2.2 Métriques Calculées

| Métrique | Description | Formule | Unité |
|----------|-------------|---------|-------|
| **Accuracy** | Précision globale | (TotalHits / TotalShots) × 100 | % |
| **Headshot %** | Ratio de headshots | (Headshots / TotalHits) × 100 | % |
| **KPM** | Kills par minute | (TotalHits / Duration) × 60 | hits/min |

### 2.3 Métriques de Temps de Réaction

| Métrique | Description | Formule | Unité |
|----------|-------------|---------|-------|
| **Average RT** | Temps de réaction moyen | Sum(RT) / Count(RT) | ms |
| **Best RT** | Meilleur temps de réaction | Min(RT) | ms |
| **Worst RT** | Pire temps de réaction | Max(RT) | ms |
| **Consistency** | Écart-type des RT | σ(RT) | ms |

---

## 3. Session Stats Structure

### 3.1 Données Brutes

| Champ | Type | Description |
|-------|------|-------------|
| SessionId | string | Identifiant unique (GUID) |
| StartTime | DateTime | Heure de début |
| EndTime | DateTime | Heure de fin |
| Duration | float | Durée en secondes |
| Mode | string | Mode d'entraînement |
| Agent | string | Agent cible utilisé |
| Map | string | Map jouée |
| TotalShots | int | Nombre de tirs |
| TotalHits | int | Nombre de hits |
| Headshots | int | Nombre de headshots |
| Bodyshots | int | Nombre de bodyshots |
| ReactionTimes | List<float> | Liste des RT (en secondes) |

### 3.2 Données Calculées

| Champ | Type | Calcul |
|-------|------|--------|
| TotalMisses | int | TotalShots - TotalHits |
| Accuracy | float | (TotalHits / TotalShots) × 100 |
| HeadshotPercentage | float | (Headshots / TotalHits) × 100 |
| KillsPerMinute | float | (TotalHits / Duration) × 60 |
| AverageReactionTime | float | Average(ReactionTimes) |
| BestReactionTime | float | Min(ReactionTimes) |
| WorstReactionTime | float | Max(ReactionTimes) |
| Consistency | float | StdDev(ReactionTimes) |

---

## 4. Calcul des Métriques

### 4.1 Accuracy

```
Accuracy = (TotalHits / TotalShots) × 100

Exemples:
- 45 hits sur 50 tirs = 90.0%
- 38 hits sur 42 tirs = 90.5%
- 0 tirs = 0% (division par zéro évitée)

Edge cases:
- TotalShots = 0 → Accuracy = 0%
- TotalHits > TotalShots → Error (impossible)
```

### 4.2 Headshot Percentage

```
HeadshotPercentage = (Headshots / TotalHits) × 100

Exemples:
- 30 headshots sur 45 hits = 66.7%
- 0 hits = 0% (division par zéro évitée)

Edge cases:
- TotalHits = 0 → HeadshotPercentage = 0%
- Mode Headshot: devrait être ~100% ou basé sur attempts
```

### 4.3 Kills Per Minute (KPM)

```
KPM = (TotalHits / Duration) × 60

Exemples:
- 45 hits en 60s = 45.0 KPM
- 90 hits en 120s = 45.0 KPM
- 30 hits en 30s = 60.0 KPM

Edge cases:
- Duration = 0 → KPM = 0
```

### 4.4 Average Reaction Time

```
AverageRT = Sum(ReactionTimes) / Count(ReactionTimes)

Conversion: Stocké en secondes, affiché en millisecondes

Exemples:
- [0.250, 0.300, 0.275] → 0.275s → 275ms
- Liste vide → 0ms

Edge cases:
- Aucun hit → AverageRT = 0
- Mode Tracking → RT non applicable (temps sur cible)
```

### 4.5 Consistency (Standard Deviation)

```
σ = √(Σ(RT - μ)² / n)

Où:
- RT = chaque temps de réaction
- μ = moyenne des RT
- n = nombre de RT

Interprétation:
- σ bas (< 50ms) = Très consistant
- σ moyen (50-100ms) = Consistant
- σ élevé (> 100ms) = Inconsistant

Edge cases:
- Moins de 2 RT → σ = 0
- Tous RT identiques → σ = 0
```

---

## 5. Stats par Mode

### 5.1 Mode Flick

| Stat | Tracked | Notes |
|------|---------|-------|
| Score | Oui | = TotalHits |
| Accuracy | Oui | Standard |
| Headshot % | Oui | Standard |
| KPM | Oui | Standard |
| Average RT | Oui | Temps spawn→hit |
| Best RT | Oui | - |
| Consistency | Oui | σ des RT |

### 5.2 Mode Tracking

| Stat | Tracked | Notes |
|------|---------|-------|
| Score | Oui | = TimeOnTarget (seconds) |
| Accuracy | Oui | TimeOnTarget / TotalTime × 100 |
| Head Accuracy | Oui | HeadTime / TimeOnTarget × 100 |
| KPM | Non | N/A |
| Average RT | Non | N/A (continu) |

**Métriques spécifiques Tracking:**

| Stat | Description |
|------|-------------|
| TimeOnTarget | Temps total sur la cible |
| HeadTime | Temps sur la tête |
| BodyTime | Temps sur le corps |
| LongestStreak | Plus longue période continue sur cible |

### 5.3 Mode Speed

| Stat | Tracked | Notes |
|------|---------|-------|
| Score | Oui | Avec bonus vitesse |
| Accuracy | Oui | Standard |
| Headshot % | Oui | Standard |
| KPM | Oui | Plus élevé attendu |
| Average RT | Oui | Plus court attendu |
| Best RT | Oui | Focus principal |

### 5.4 Mode Headshot

| Stat | Tracked | Notes |
|------|---------|-------|
| Score | Oui | Headshots only |
| Headshot % | Oui | Devrait être ~100% |
| Bodyshot Attempts | Oui | Compte les bodyshots comme erreurs |
| Average RT | Oui | Standard |

---

## 6. Live Stats (HUD)

### 6.1 Stats Affichées en Temps Réel

| Stat | Position | Update Rate |
|------|----------|-------------|
| Timer | Top Left | Chaque seconde |
| Score | Bottom Left | Chaque hit |
| Accuracy | Top Right | Chaque tir |
| Last RT | Bottom Center | Chaque hit |
| Avg RT | Bottom Center | Chaque hit |
| HS % | Bottom Right | Chaque hit |

### 6.2 Format d'Affichage HUD

| Stat | Format | Exemple |
|------|--------|---------|
| Timer | MM:SS | 00:45 |
| Score | Integer | 47 |
| Accuracy | XX.X% | 89.5% |
| Last RT | XXXms | 245ms |
| Avg RT | XXXms | 312ms |
| HS % | XX% | 67% |

### 6.3 Règles de Mise à Jour

```
On Shoot:
  └── Update Accuracy

On Hit:
  ├── Update Score (+1)
  ├── Update Last RT
  ├── Update Avg RT
  └── Update HS % (if headshot)

Every Second:
  └── Update Timer (-1)

On Timer = 0:
  └── Freeze all stats
```

---

## 7. Results Screen Stats

### 7.1 Layout des Stats

```
┌─────────────────────────────────────────────────────────────┐
│                     SESSION COMPLETE                         │
│                   FLICK | ISO | THE RANGE                   │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│    ┌───────────────────┐    ┌───────────────────┐          │
│    │       SCORE       │    │     ACCURACY      │          │
│    │        47         │    │      89.5%        │          │
│    └───────────────────┘    └───────────────────┘          │
│                                                             │
│  ┌────────────────────────────────────────────────────┐    │
│  │  REACTION TIMES                                     │    │
│  │  ├─ Average:     312ms                             │    │
│  │  ├─ Best:        187ms                             │    │
│  │  ├─ Worst:       534ms                             │    │
│  │  └─ Consistency:  78ms (σ)                         │    │
│  └────────────────────────────────────────────────────┘    │
│                                                             │
│  ┌────────────────────────────────────────────────────┐    │
│  │  BREAKDOWN                                          │    │
│  │  ├─ Headshots:    32 (68%)                         │    │
│  │  ├─ Bodyshots:    15 (32%)                         │    │
│  │  ├─ Misses:        5                               │    │
│  │  └─ KPM:         47.0                              │    │
│  └────────────────────────────────────────────────────┘    │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

### 7.2 Sections

| Section | Stats Incluses |
|---------|----------------|
| **Header** | Mode, Agent, Map |
| **Primary** | Score, Accuracy (grands chiffres) |
| **Reaction Times** | Avg, Best, Worst, Consistency |
| **Breakdown** | Headshots, Bodyshots, Misses, KPM |

### 7.3 Formatting Rules

| Stat | Precision | Unit |
|------|-----------|------|
| Score | Integer | - |
| Accuracy | 1 decimal | % |
| HS % | Integer | % |
| RT values | Integer | ms |
| Consistency | Integer | ms |
| KPM | 1 decimal | - |

---

## 8. Tracking Implementation

### 8.1 Event-Based Tracking

```
OnSessionStart:
  ├── Create new SessionStats
  ├── Set SessionId = new GUID
  ├── Set StartTime = Now
  ├── Set Mode, Agent, Map
  └── Initialize counters to 0

OnShoot:
  └── TotalShots++

OnHit(HitInfo):
  ├── TotalHits++
  ├── ReactionTimes.Add(HitInfo.RT)
  ├── If IsHeadshot:
  │   └── Headshots++
  └── Else:
      └── Bodyshots++

OnMiss:
  └── (TotalMisses calculated from TotalShots - TotalHits)

OnSessionEnd:
  ├── Set EndTime = Now
  ├── Calculate Duration
  ├── Calculate all derived stats
  └── Return SessionStats
```

### 8.2 Session Manager Responsibilities

| Responsibility | Description |
|----------------|-------------|
| Track Time | Countdown timer |
| Aggregate Events | Receive shoot/hit/miss events |
| Calculate Stats | Compute derived metrics |
| Provide Data | Expose stats for HUD and Results |

---

## 9. Persistance (Future)

### 9.1 V1.0: No Persistence

En V1.0, les stats ne sont **pas sauvegardées** entre les sessions.

### 9.2 V2.0: Historical Stats (Future)

| Feature | Description |
|---------|-------------|
| Session History | Liste des 50 dernières sessions |
| Best Scores | Records par mode/agent/map |
| Progress Graphs | Évolution dans le temps |
| Averages | Moyennes sur X sessions |

---

## 10. Performance

### 10.1 Considérations

| Aspect | Approach |
|--------|----------|
| Update Rate | Stats updated on events, not every frame |
| Calculations | Derived stats calculated on demand |
| Memory | List<float> for RT (bounded by session length) |

### 10.2 Limites

| Limite | Valeur | Raison |
|--------|--------|--------|
| Max RT entries | ~500 | Session 120s, ~4 hits/s max |
| Precision float | 32-bit | Suffisant pour ms precision |

---

## 11. Edge Cases

### 11.1 Division par Zéro

| Cas | Handling |
|-----|----------|
| Accuracy avec 0 shots | Return 0% |
| HS% avec 0 hits | Return 0% |
| KPM avec 0 duration | Return 0 |
| Avg RT avec 0 hits | Return 0ms |

### 11.2 Valeurs Extrêmes

| Cas | Handling |
|-----|----------|
| RT < 50ms | Valide (très rapide) |
| RT > 3000ms | Valide (timeout) |
| Accuracy > 100% | Error (impossible) |
| Negative values | Error (impossible) |

### 11.3 Mode Tracking Special

| Cas | Handling |
|-----|----------|
| 0% time on target | Score = 0, Accuracy = 0% |
| 100% time on target | Perfect score |
| RT not applicable | Don't show RT section |

---

## 12. Glossaire des Stats

| Terme | Définition |
|-------|------------|
| **RT** | Reaction Time - temps entre spawn et hit |
| **KPM** | Kills Per Minute - hits par minute |
| **HS%** | Headshot Percentage - ratio de headshots |
| **σ (Sigma)** | Standard Deviation - mesure de consistance |
| **Accuracy** | Précision - ratio hits/shots |
| **Score** | Nombre de cibles éliminées |

---

*Voir [07_UI_UX.md](./07_UI_UX.md) pour l'interface utilisateur.*
