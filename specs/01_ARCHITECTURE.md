# 01 — Architecture Technique

> **Priorité**: P0 (MVP)
> **Statut**: Draft

---

## 1. Vue d'Ensemble

### 1.1 Principes Architecturaux

| Principe | Application |
|----------|-------------|
| **Separation of Concerns** | Chaque système a une responsabilité unique |
| **Event-Driven** | Communication découplée via events |
| **Data-Driven** | Configuration via ScriptableObjects |
| **Object Pooling** | Réutilisation des objets fréquents |
| **Single Source of Truth** | GameManager centralise l'état global |

### 1.2 Layers Applicatifs

```
┌─────────────────────────────────────────────────────────────┐
│                    PRESENTATION LAYER                        │
│         UI Views, HUD, Input Controller, Crosshair          │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                       GAME LAYER                             │
│    GameManager, ModeManager, SessionManager, ShootSystem    │
│         TargetSpawner, CrosshairSystem, HitFeedback         │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                       CORE LAYER                             │
│     EventBus, SaveSystem, ObjectPool, AudioManager          │
│            AssetLoader, SettingsManager                      │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                       DATA LAYER                             │
│   ScriptableObjects: AgentData, MapData, CrosshairSettings  │
│              GameSettings, AudioSettings                     │
└─────────────────────────────────────────────────────────────┘
```

---

## 2. Design Patterns

### 2.1 Patterns Utilisés

| Pattern | Système | Justification |
|---------|---------|---------------|
| **Singleton** | GameManager, AudioManager, UIManager | Point d'accès global unique, cycle de vie application |
| **Strategy** | Training Modes | Comportements interchangeables selon le mode |
| **Observer** | EventBus | Découplage total entre systèmes |
| **Object Pool** | Targets, Particles, Decals | Éviter allocations/GC pendant gameplay |
| **State Machine** | Game Flow | Gestion claire des états (Menu, Playing, Paused) |
| **Factory** | TargetFactory | Création flexible selon type d'agent |
| **ScriptableObject** | All Data | Data-driven, éditable dans l'éditeur |

### 2.2 Singleton Pattern

**Utilisé pour:**
- GameManager (état global du jeu)
- AudioManager (lecture audio centralisée)
- UIManager (gestion des écrans)
- SaveSystem (persistance)

**Règles:**
- Créé dans la scene _Preload
- Marqué DontDestroyOnLoad
- Accessible via propriété statique Instance
- Initialisation dans Awake()

### 2.3 Strategy Pattern - Modes

**Interface commune**: ITrainingMode

**Implémentations:**
- FlickMode
- TrackingMode
- SpeedMode
- HeadshotMode

**Responsabilités de chaque mode:**
- Définir le comportement de spawn
- Définir le comportement des cibles
- Définir les règles de scoring
- Gérer les événements hit/miss

### 2.4 Observer Pattern - EventBus

**Catégories d'events:**

| Catégorie | Events |
|-----------|--------|
| Session | OnSessionStart, OnSessionEnd, OnSessionPause, OnSessionResume |
| Target | OnTargetSpawned, OnTargetHit, OnTargetMissed, OnTargetDespawned |
| Shooting | OnShoot, OnHit, OnMiss |
| Game State | OnGameStateChanged |
| Stats | OnStatsUpdated |

**Règles:**
- Subscribers s'enregistrent dans OnEnable
- Subscribers se désenregistrent dans OnDisable
- Events sont statiques pour accès global
- Pas de références circulaires

### 2.5 Object Pool Pattern

**Objets poolés:**

| Objet | Pool Size | Raison |
|-------|-----------|--------|
| Targets (par agent) | 5 | Spawn fréquent |
| Hit Particles Body | 10 | Effet à chaque hit |
| Hit Particles Head | 10 | Effet à chaque headshot |
| Bullet Hole Decals | 20 | Persiste quelques secondes |
| Audio Sources | 5 | Sons simultanés |

**Lifecycle:**
1. Pool.Get() → Active l'objet, appelle OnSpawn()
2. Utilisation normale
3. Pool.Return() → Appelle OnDespawn(), désactive l'objet

### 2.6 State Machine - Game States

```
                    ┌─────────────┐
                    │  MainMenu   │
                    └──────┬──────┘
                           │ Play
                           ▼
                    ┌─────────────┐
                    │ ModeSelect  │
                    └──────┬──────┘
                           │ Select
                           ▼
                    ┌─────────────┐
                    │ AgentSelect │
                    └──────┬──────┘
                           │ Select
                           ▼
                    ┌─────────────┐
                    │  MapSelect  │
                    └──────┬──────┘
                           │ Select
                           ▼
                    ┌─────────────┐
                    │   Loading   │
                    └──────┬──────┘
                           │ Loaded
                           ▼
                    ┌─────────────┐
                    │  Countdown  │
                    └──────┬──────┘
                           │ GO!
                           ▼
┌──────────┐        ┌─────────────┐        ┌──────────┐
│  Paused  │◄──────►│   Playing   │───────►│ Results  │
└──────────┘  ESC   └─────────────┘ Timer  └──────────┘
                                    =0
```

**États:**

| État | Entrée | Sortie | Actions |
|------|--------|--------|---------|
| MainMenu | App start, Quit to Menu | Play, Settings, Quit | Afficher menu |
| ModeSelect | Play | Select mode, Back | Afficher modes |
| AgentSelect | Mode selected | Select agent, Back | Afficher agents |
| MapSelect | Agent selected | Select map, Back | Afficher maps |
| Loading | Map selected | Load complete | Charger scene, init |
| Countdown | Loading complete | Timer = 0 | 3-2-1-GO |
| Playing | Countdown done | Pause, Timer = 0 | Gameplay actif |
| Paused | ESC in Playing | Resume, Quit | Freeze gameplay |
| Results | Timer = 0 | Retry, Menu | Afficher stats |

---

## 3. Structure des Managers

### 3.1 GameManager

**Responsabilités:**
- État global du jeu (GameState)
- Références aux autres managers
- Transitions entre états
- Configuration de session (mode, agent, map)

**Dépendances:**
- UIManager
- AudioManager
- SessionManager
- ModeManager

### 3.2 SessionManager

**Responsabilités:**
- Tracking du temps de session
- Calcul et stockage des statistiques
- Gestion du countdown
- Déclenchement fin de session

**Données gérées:**
- Timer restant
- SessionStats courantes
- État de la session (active, paused, ended)

### 3.3 ModeManager

**Responsabilités:**
- Instanciation du mode sélectionné
- Délégation des events au mode
- Interface entre GameManager et ITrainingMode

**Modes disponibles:**
- FlickMode
- TrackingMode
- SpeedMode
- HeadshotMode

### 3.4 UIManager

**Responsabilités:**
- Gestion des écrans (show/hide)
- Transitions entre écrans
- Mise à jour du HUD
- Gestion des popups

**Écrans gérés:**
- MainMenuScreen
- ModeSelectScreen
- AgentSelectScreen
- MapSelectScreen
- SettingsScreen
- HUDScreen
- PauseScreen
- ResultsScreen
- LoadingScreen

### 3.5 AudioManager

**Responsabilités:**
- Lecture des sons (SFX, UI, Music)
- Gestion des volumes par catégorie
- Pooling des AudioSources
- Application des settings audio

**Canaux:**
- Master
- SFX
- UI
- Music

---

## 4. Structure du Projet Unity

### 4.1 Hiérarchie des Dossiers

```
Assets/
│
├── _Project/
│   ├── Scripts/
│   │   ├── Core/
│   │   ├── Gameplay/
│   │   ├── Modes/
│   │   ├── UI/
│   │   ├── Audio/
│   │   ├── Data/
│   │   └── Utilities/
│   │
│   ├── ScriptableObjects/
│   │   ├── Agents/
│   │   ├── Maps/
│   │   └── Settings/
│   │
│   ├── Prefabs/
│   │   ├── Agents/
│   │   ├── Effects/
│   │   └── UI/
│   │
│   ├── Scenes/
│   │   ├── _Preload.unity
│   │   ├── MainMenu.unity
│   │   └── Maps/
│   │
│   ├── UI/
│   │   ├── Sprites/
│   │   ├── Fonts/
│   │   └── Stylesheets/
│   │
│   └── Audio/
│       ├── SFX/
│       ├── UI/
│       └── Music/
│
├── Models/
│   ├── Agents/
│   └── Maps/
│
└── Settings/
    └── URP/
```

### 4.2 Organisation des Scripts

| Dossier | Contenu |
|---------|---------|
| Core/ | GameManager, EventBus, ObjectPool, SaveSystem |
| Gameplay/ | ShootingSystem, Target, TargetSpawner, HitZone, HitFeedback |
| Modes/ | ITrainingMode, FlickMode, TrackingMode, SpeedMode, HeadshotMode |
| UI/ | UIManager, HUDController, CrosshairRenderer, Screens/ |
| Audio/ | AudioManager |
| Data/ | SessionStats, GameSettings, CrosshairSettings, AgentData, MapData |
| Utilities/ | Extensions, Constants, Helpers |

### 4.3 Scenes

| Scene | Type | Contenu |
|-------|------|---------|
| _Preload | Persistent | Managers (GameManager, AudioManager, UIManager) |
| MainMenu | Additive | Menu principal, UI navigation |
| TheRange | Additive | Map training, spawn zones |
| Ascent | Additive | Map alternative |

**Règles de chargement:**
1. _Preload toujours chargée en premier (Bootstrap)
2. MainMenu chargée additivement après init
3. Maps chargées additivement, déchargées au retour menu

---

## 5. Configuration Unity

### 5.1 Project Settings

| Setting | Valeur |
|---------|--------|
| Unity Version | 2022.3 LTS |
| Render Pipeline | URP |
| Color Space | Linear |
| Scripting Backend | IL2CPP |
| API Compatibility | .NET Standard 2.1 |

### 5.2 Layers

| Layer # | Nom | Usage |
|---------|-----|-------|
| 0 | Default | Objets généraux |
| 6 | Target_Body | Colliders body des cibles |
| 7 | Target_Head | Colliders head des cibles |
| 8 | Environment | Colliders map (sol, murs) |
| 9 | UI | Éléments UI (raycast UI) |

### 5.3 Tags

| Tag | Usage |
|-----|-------|
| Target | Tous les GameObjects cibles |
| SpawnZone | Zones de spawn des cibles |
| PlayerSpawn | Points de spawn joueur |
| Manager | Managers persistants |

### 5.4 Physics

| Setting | Valeur | Raison |
|---------|--------|--------|
| Fixed Timestep | 0.01 (100Hz) | Précision physique |
| Gravity | (0, -9.81, 0) | Standard |

**Layer Collision Matrix - Désactivé:**
- Target_Body ↔ Target_Body
- Target_Head ↔ Target_Head
- Target_Body ↔ Target_Head

### 5.5 Quality Settings

| Setting | Valeur |
|---------|--------|
| VSync | Off |
| Target Frame Rate | -1 (uncapped) |
| Anti-Aliasing | MSAA 4x |
| Shadow Quality | Medium |
| Shadow Distance | 50 |
| Texture Quality | Full |

---

## 6. Packages Requis

### 6.1 Packages Unity

| Package | Version | Usage |
|---------|---------|-------|
| Universal RP | 14.0.x | Render pipeline |
| Input System | 1.7.x | Gestion inputs moderne |
| TextMeshPro | 3.0.x | Texte UI |
| UI Toolkit | 1.0.x | Interface utilisateur |

### 6.2 Packages Externes

| Package | Version | Usage |
|---------|---------|-------|
| glTFast | 6.0.x | Import modèles GLB |
| Newtonsoft JSON | 3.2.x | Sérialisation settings |

---

## 7. Flux de Données

### 7.1 Flux de Configuration

```
ScriptableObjects (Data)
         │
         ▼
    Managers (Runtime)
         │
         ▼
    Systems (Gameplay)
         │
         ▼
    Components (Instances)
```

### 7.2 Flux d'Events

```
Input (Mouse Click)
         │
         ▼
  ShootingSystem
         │
    ┌────┴────┐
    ▼         ▼
  OnHit    OnMiss
    │         │
    ▼         ▼
┌───────┐ ┌────────┐
│Target │ │Feedback│
│System │ │ System │
└───┬───┘ └────────┘
    │
    ▼
SessionManager
(Stats Update)
    │
    ▼
   HUD
(Display)
```

### 7.3 Flux de Sauvegarde

```
User modifie Setting
         │
         ▼
  SettingsManager
         │
         ▼
  ScriptableObject (runtime)
         │
         ▼
    SaveSystem
         │
         ▼
   JSON File (disk)
```

---

## 8. Conventions de Code

### 8.1 Naming Conventions

| Type | Convention | Exemple |
|------|------------|---------|
| Classes | PascalCase | GameManager |
| Interfaces | I + PascalCase | ITrainingMode |
| Methods | PascalCase | SpawnTarget() |
| Properties | PascalCase | CurrentState |
| Private fields | _camelCase | _activeTargets |
| Serialized fields | camelCase | targetPrefab |
| Constants | UPPER_SNAKE | MAX_TARGETS |
| Events | On + Action | OnTargetHit |

### 8.2 Structure de Fichier

```
1. Using statements
2. Namespace
3. Class declaration
4. Constants
5. Serialized fields
6. Private fields
7. Properties
8. Unity callbacks (Awake, Start, Update)
9. Public methods
10. Private methods
11. Event handlers
```

### 8.3 Règles Générales

- Pas de `magic numbers` → utiliser constantes
- Pas de `Find` dans Update → cache dans Awake/Start
- Pas d'allocations dans Update → preallocate
- Events → subscribe dans OnEnable, unsubscribe dans OnDisable
- Null checks → utiliser `?` et `??`
- Collections → initialiser avec capacité si connue

---

*Voir [02_GAMEPLAY.md](./02_GAMEPLAY.md) pour les spécifications de gameplay.*
