# 02 — Gameplay & Modes

> **Priorité**: P0 (MVP) / P1 (Modes additionnels)
> **Statut**: Draft

---

## 1. Vue d'Ensemble

### 1.1 Concept Core

Le gameplay consiste à éliminer des cibles (agents Valorant) le plus rapidement et précisément possible dans un temps limité, avec différents modes d'entraînement ciblant des compétences spécifiques.

### 1.2 Game Loop Principal

```
Session Start
     │
     ▼
┌─────────────┐
│  Countdown  │ ←── 3-2-1-GO (4 secondes)
└──────┬──────┘
       │
       ▼
┌─────────────────────────────────────────┐
│              GAME LOOP                   │
│  ┌─────────────────────────────────┐    │
│  │  1. Target Spawn                │    │
│  │  2. Player Aims                 │    │
│  │  3. Player Shoots               │    │
│  │  4. Hit Detection               │    │
│  │  5. Feedback (visual + audio)   │    │
│  │  6. Stats Update                │    │
│  │  7. Target Despawn              │    │
│  │  8. Repeat until Timer = 0      │    │
│  └─────────────────────────────────┘    │
└────────────────────┬────────────────────┘
                     │
                     ▼
              ┌─────────────┐
              │   Results   │
              └─────────────┘
```

### 1.3 Inputs Joueur

| Input | Action | Contexte |
|-------|--------|----------|
| Mouse Move | Rotation caméra (look) | In-Game |
| Left Click | Tir | In-Game |
| ESC | Pause / Resume | In-Game |
| R | Restart session | In-Game (Pause) |

---

## 2. Modes d'Entraînement

### 2.1 Vue Comparative

| Mode | Cible | Mouvement | Timing | Focus | Difficulté |
|------|-------|-----------|--------|-------|------------|
| Flick | Statique | Non | 3s timeout | Précision rapide | ★★☆☆☆ |
| Tracking | Mobile | Continu | Infini | Suivi fluide | ★★★☆☆ |
| Speed | Statique | Non | 0.5-1.5s | Réactivité | ★★★★☆ |
| Headshot | Statique/Lent | Léger | 3s timeout | Précision tête | ★★★★☆ |

### 2.2 Mode Flick (MVP)

#### Description
Cibles statiques apparaissant à des positions aléatoires. Le joueur doit effectuer des "flick shots" rapides et précis.

#### Paramètres

| Paramètre | Valeur | Configurable |
|-----------|--------|--------------|
| Target Lifetime | 3 secondes | Non |
| Spawn Delay | 0ms (immédiat) | Non |
| Movement | Aucun | - |
| Spawn Zone | Zone définie par map | Non |
| Targets Simultanés | 1 | Non |

#### Comportement Cible

```
SPAWN
  │
  ├── Position: Aléatoire dans spawn zone
  ├── Rotation: Face au joueur
  └── État: Active
       │
       ▼
WAIT FOR INTERACTION
  │
  ├── [Hit] ─────► DESPAWN (Success)
  │                 ├── Play hit feedback
  │                 ├── Record reaction time
  │                 └── Spawn next target
  │
  └── [Timeout 3s] ► DESPAWN (Miss)
                     ├── Record as miss
                     └── Spawn next target
```

#### Scoring

| Action | Points | Stat Impact |
|--------|--------|-------------|
| Headshot | +1 | Hits++, Headshots++, RT recorded |
| Bodyshot | +1 | Hits++, Bodyshots++, RT recorded |
| Timeout | 0 | Misses++ |
| Miss (click) | 0 | Shots++, Misses++ |

### 2.3 Mode Tracking

#### Description
Une cible mobile se déplace de façon fluide. Le joueur doit maintenir son viseur sur la cible.

#### Paramètres

| Paramètre | Valeur | Configurable |
|-----------|--------|--------------|
| Target Lifetime | Session entière | - |
| Movement Pattern | Sinusoïdal + Random | Non |
| Movement Speed | Variable (1-3 m/s) | Non |
| Hit Detection | Continuous (tick) | - |
| Tick Rate | 10Hz | Non |

#### Comportement Cible

```
SPAWN
  │
  └── Position: Centre de zone
       │
       ▼
MOVEMENT LOOP
  │
  ├── Calculate next position (smooth)
  ├── Apply movement
  └── Repeat until session end
       │
       ▼
EVERY TICK (100ms)
  │
  ├── Check if crosshair on target
  │    ├── [On Target] ► TimeOnTarget++
  │    │                  ├── [On Head] ► HeadTime++
  │    │                  └── [On Body] ► BodyTime++
  │    └── [Off Target] ► TimeOffTarget++
  │
  └── Update accuracy display
```

#### Scoring

| Métrique | Calcul |
|----------|--------|
| Accuracy | TimeOnTarget / TotalTime × 100% |
| Head Accuracy | HeadTime / TimeOnTarget × 100% |
| Score | TimeOnTarget in seconds |

#### Patterns de Mouvement

| Pattern | Description |
|---------|-------------|
| Horizontal Sine | Mouvement gauche-droite sinusoïdal |
| Vertical Sine | Mouvement haut-bas sinusoïdal |
| Figure 8 | Combinaison des deux |
| Random Smooth | Direction aléatoire avec interpolation |

### 2.4 Mode Speed

#### Description
Cibles apparaissant brièvement. Focus sur la réactivité et les décisions rapides.

#### Paramètres

| Paramètre | Valeur | Configurable |
|-----------|--------|--------------|
| Target Lifetime | 0.5 - 1.5s (random) | Non |
| Spawn Delay | 0-200ms | Non |
| Movement | Aucun | - |
| Targets Simultanés | 1-2 | Non |

#### Comportement Cible

```
SPAWN
  │
  ├── Position: Aléatoire
  ├── Lifetime: Random(0.5s, 1.5s)
  └── État: Active
       │
       ▼
SHORT WINDOW
  │
  ├── [Hit before timeout] ► SUCCESS
  │                          ├── Bonus points (faster = more)
  │                          └── Spawn next
  │
  └── [Timeout] ► MISS
                  ├── Record miss
                  └── Spawn next immediately
```

#### Scoring

| Action | Points | Notes |
|--------|--------|-------|
| Hit < 300ms | +3 | Excellent |
| Hit 300-600ms | +2 | Good |
| Hit 600-1000ms | +1 | OK |
| Hit > 1000ms | +1 | Slow |
| Miss | 0 | - |

### 2.5 Mode Headshot

#### Description
Seuls les headshots comptent. Les bodyshots sont considérés comme des miss.

#### Paramètres

| Paramètre | Valeur | Configurable |
|-----------|--------|--------------|
| Target Lifetime | 3 secondes | Non |
| Valid Hit Zone | Head only | - |
| Movement | Léger sway (optionnel) | Non |
| Feedback on Bodyshot | Négatif (X rouge) | - |

#### Comportement

```
SPAWN
  │
  └── Target active
       │
       ▼
WAIT FOR SHOT
  │
  ├── [Headshot] ► SUCCESS
  │                ├── +1 Score
  │                ├── Positive feedback
  │                └── Spawn next
  │
  ├── [Bodyshot] ► PENALTY
  │                ├── 0 Points
  │                ├── Negative feedback (X)
  │                └── Target reste (ou despawn selon config)
  │
  └── [Timeout] ► MISS
                  └── Spawn next
```

#### Scoring

| Action | Points | Feedback |
|--------|--------|----------|
| Headshot | +1 | Dink + Yellow particles |
| Bodyshot | 0 | X rouge + son négatif |
| Timeout | 0 | Cible disparaît |

---

## 3. Session Configuration

### 3.1 Durées de Session

| Option | Durée | Usage |
|--------|-------|-------|
| Quick | 30 secondes | Warm-up rapide |
| Standard | 60 secondes | Session normale (défaut) |
| Extended | 90 secondes | Entraînement prolongé |
| Long | 120 secondes | Session intensive |

### 3.2 Sélection de Session

```
┌─────────────────────────────────────────┐
│           SESSION SETUP                  │
├─────────────────────────────────────────┤
│                                         │
│  Mode:     [Flick ▼]                    │
│                                         │
│  Agent:    [ISO ▼]                      │
│                                         │
│  Map:      [The Range ▼]                │
│                                         │
│  Duration: [60 seconds ▼]               │
│                                         │
│           ┌───────────────┐             │
│           │     START     │             │
│           └───────────────┘             │
└─────────────────────────────────────────┘
```

---

## 4. Spawning System

### 4.1 Spawn Zones

Chaque map définit des zones de spawn pour les cibles.

**Propriétés d'une Spawn Zone:**

| Propriété | Type | Description |
|-----------|------|-------------|
| Bounds | Box | Volume 3D de spawn |
| Weight | Float | Probabilité de sélection |
| MinDistance | Float | Distance min du joueur |
| MaxDistance | Float | Distance max du joueur |
| HeightRange | Vector2 | Hauteur min/max |

### 4.2 Règles de Spawn

| Règle | Description |
|-------|-------------|
| **Distance minimum** | Cible à au moins 5m du joueur |
| **Distance maximum** | Cible à maximum 30m du joueur |
| **Non-overlap** | Nouvelle cible pas sur l'ancienne position |
| **Line of Sight** | Cible visible depuis position joueur |
| **Face Player** | Cible orientée face au joueur |

### 4.3 Algorithme de Spawn

```
1. SELECT spawn zone (weighted random)
2. GENERATE random point in zone bounds
3. VALIDATE point:
   - Distance from player OK?
   - Not too close to last target?
   - Line of sight clear?
4. IF invalid: retry (max 10 attempts)
5. SPAWN target at validated position
6. ROTATE target to face player
7. TRIGGER OnTargetSpawned event
```

---

## 5. Difficulty Scaling (Future)

### 5.1 Paramètres de Difficulté

| Paramètre | Easy | Normal | Hard |
|-----------|------|--------|------|
| Target Size | 100% | 100% | 80% |
| Target Lifetime | 4s | 3s | 2s |
| Spawn Distance | 5-15m | 5-25m | 10-30m |
| Movement Speed | 1 m/s | 2 m/s | 3 m/s |

### 5.2 Progression (Future Feature)

Non implémenté en V1.0. Prévu pour future version:
- Difficulté adaptative basée sur performance
- Challenges progressifs
- Unlockables

---

## 6. Player Controller

### 6.1 First Person Camera

| Paramètre | Valeur |
|-----------|--------|
| FOV | 103° (Valorant default) |
| Near Clip | 0.1 |
| Far Clip | 1000 |
| Height | 1.6m (eye level) |

### 6.2 Mouse Look

| Paramètre | Valeur |
|-----------|--------|
| Sensitivity | 1.0 (configurable 0.1-10) |
| Raw Input | Enabled |
| Invert Y | Configurable |
| Smoothing | None (0) |

### 6.3 Movement

**V1.0**: Joueur statique (pas de déplacement)

**Future**: WASD movement optionnel

---

## 7. Pause System

### 7.1 États de Pause

| État | Timer | Input | Targets | Actions Disponibles |
|------|-------|-------|---------|---------------------|
| Playing | Running | Active | Active | Shoot, Pause |
| Paused | Frozen | Menu only | Frozen | Resume, Restart, Settings, Quit |

### 7.2 Comportement

**On Pause (ESC):**
1. Freeze Time.timeScale = 0
2. Unlock cursor
3. Show pause menu
4. Disable shooting

**On Resume:**
1. Hide pause menu
2. Lock cursor
3. Countdown 3-2-1 (optionnel)
4. Time.timeScale = 1
5. Enable shooting

---

## 8. End Session

### 8.1 Déclencheurs de Fin

| Trigger | Action |
|---------|--------|
| Timer = 0 | Normal end → Results |
| Quit in Pause | Early end → Results |

### 8.2 Séquence de Fin

```
TIMER REACHES ZERO
       │
       ▼
  Stop spawning
       │
       ▼
  Despawn active targets
       │
       ▼
  Calculate final stats
       │
       ▼
  Play end sound
       │
       ▼
  Unlock cursor
       │
       ▼
  Show Results screen
```

---

## 9. Règles de Gameplay

### 9.1 Règles Générales

| Règle | Description |
|-------|-------------|
| Un tir = un clic | Pas de full-auto |
| Pas de pénalité miss | Les miss n'enlèvent pas de points |
| Targets infinis | Nouveau spawn immédiat après hit/timeout |
| Pas de reload | Munitions infinies |

### 9.2 Règles par Mode

**Flick:**
- 1 cible active maximum
- Hit ou timeout pour passer à la suivante

**Tracking:**
- 1 cible continue
- Scoring basé sur temps sur cible

**Speed:**
- 1-2 cibles actives
- Timeout très court

**Headshot:**
- 1 cible active
- Bodyshots ne comptent pas

---

*Voir [03_TARGETS.md](./03_TARGETS.md) pour les spécifications des cibles.*
