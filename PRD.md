# VALORANT AIM TRAINER — Product Requirements Document (PRD)

> **Version**: 1.0.0
> **Date**: 2026-02-01
> **Statut**: Draft
> **Auteur**: Auto-généré depuis 00_VISION.md

---

## Table des Matières

1. [Résumé Exécutif](#1-résumé-exécutif)
2. [Contexte & Objectifs](#2-contexte--objectifs)
3. [Périmètre du Projet](#3-périmètre-du-projet)
4. [Architecture Technique](#4-architecture-technique)
5. [Spécifications Fonctionnelles](#5-spécifications-fonctionnelles)
6. [Spécifications Techniques](#6-spécifications-techniques)
7. [Interface Utilisateur](#7-interface-utilisateur)
8. [Système Audio](#8-système-audio)
9. [Gestion des Assets](#9-gestion-des-assets)
10. [Performance & Optimisation](#10-performance--optimisation)
11. [Structure du Projet Unity](#11-structure-du-projet-unity)
12. [Roadmap & Livrables](#12-roadmap--livrables)
13. [Critères d'Acceptation](#13-critères-dacceptation)
14. [Risques & Mitigations](#14-risques--mitigations)
15. [Annexes](#15-annexes)

---

## 1. Résumé Exécutif

### 1.1 Vision Produit

Un aim trainer personnel développé sous Unity, utilisant des modèles 3D authentiques de Valorant (agents, maps) pour offrir un entraînement immersif et réaliste aux joueurs souhaitant améliorer leur précision.

### 1.2 Proposition de Valeur

| Aspect | Description |
|--------|-------------|
| **Réalisme** | Hitboxes et proportions fidèles aux agents Valorant |
| **Immersion** | Environnements tirés des maps officielles |
| **Transfert** | Compétences directement applicables en ranked |
| **Personnalisation** | Crosshair et settings identiques au jeu |

### 1.3 Public Cible

- Joueurs Valorant de tous niveaux (Iron à Radiant)
- Usage personnel uniquement
- Joueurs PC avec configuration gaming standard

### 1.4 Stack Technique

```
Moteur       : Unity 2022.3 LTS (Unity 6 compatible)
Langage      : C# 9.0+
Render       : Universal Render Pipeline (URP)
Import 3D    : glTFast 6.0
Audio        : Unity Audio System
UI           : UI Toolkit + uGUI
```

---

## 2. Contexte & Objectifs

### 2.1 Problématique

Les aim trainers génériques (Aim Lab, Kovaak's) ne reproduisent pas fidèlement:
- Les hitboxes spécifiques des agents Valorant
- L'environnement visuel du jeu
- Le feeling du crosshair Valorant

### 2.2 Solution

Créer un aim trainer sur mesure utilisant les assets réels de Valorant pour un entraînement avec transfert optimal vers le jeu.

### 2.3 Objectifs Mesurables

| Objectif | Métrique | Target |
|----------|----------|--------|
| Performance | FPS stable | ≥144 FPS |
| Latence | Input lag | <10ms |
| Chargement | Load time | <3s |
| Précision | Hitbox fidelity | 100% fidèle |

### 2.4 Contraintes

#### Légales
- Usage personnel uniquement
- Pas de distribution commerciale
- Modèles Valorant = propriété Riot Games
- Modèles fournis par l'utilisateur

#### Techniques
- Pas de multijoueur
- Pas de leaderboards online
- Single platform (Windows PC)

---

## 3. Périmètre du Projet

### 3.1 In Scope (MVP)

| Feature | Priorité | Phase |
|---------|----------|-------|
| Mode Flick | P0 | MVP |
| Agent ISO (cible) | P0 | MVP |
| Map The Range | P0 | MVP |
| Système de tir raycast | P0 | MVP |
| Crosshair configurable | P0 | MVP |
| HUD temps réel | P0 | MVP |
| Stats de session | P0 | MVP |
| Hit feedback visuel/audio | P0 | MVP |

### 3.2 In Scope (V1.0)

| Feature | Priorité | Phase |
|---------|----------|-------|
| Mode Tracking | P1 | Core |
| Mode Speed | P1 | Core |
| Mode Headshot | P1 | Core |
| Agents Jett, Phoenix | P1 | Content |
| Map Ascent | P1 | Content |
| Persistance settings | P1 | Core |
| Menu complet | P1 | Core |

### 3.3 Out of Scope

- Multijoueur / Leaderboards
- Replay system
- Mode custom / éditeur
- Support manette
- Support VR
- Tutoriel interactif avancé
- Système de progression/XP

---

## 4. Architecture Technique

### 4.1 Architecture Globale

```
┌─────────────────────────────────────────────────────────────────┐
│                        PRESENTATION LAYER                        │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────────────────┐  │
│  │   UI Views  │  │  HUD System │  │    Input Controller     │  │
│  │  (Screens)  │  │   (In-Game) │  │   (Mouse + Keyboard)    │  │
│  └─────────────┘  └─────────────┘  └─────────────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                         GAME LAYER                               │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────────────┐   │
│  │ GameManager  │  │ ModeManager  │  │   SessionManager     │   │
│  │  (Singleton) │  │  (Strategy)  │  │  (Stats Tracking)    │   │
│  └──────────────┘  └──────────────┘  └──────────────────────┘   │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────────────┐   │
│  │ ShootSystem  │  │ TargetSystem │  │   CrosshairSystem    │   │
│  │  (Raycast)   │  │  (Spawner)   │  │    (Renderer)        │   │
│  └──────────────┘  └──────────────┘  └──────────────────────┘   │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                        CORE LAYER                                │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────────────┐   │
│  │ EventBus     │  │ SettingsData │  │   AudioManager       │   │
│  │ (Observer)   │  │ (ScriptObj)  │  │    (Singleton)       │   │
│  └──────────────┘  └──────────────┘  └──────────────────────┘   │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────────────┐   │
│  │ ObjectPool   │  │ SaveSystem   │  │   AssetLoader        │   │
│  │  (Pattern)   │  │  (JSON)      │  │    (glTFast)         │   │
│  └──────────────┘  └──────────────┘  └──────────────────────┘   │
└─────────────────────────────────────────────────────────────────┘
```

### 4.2 Design Patterns Utilisés

| Pattern | Usage | Justification |
|---------|-------|---------------|
| **Singleton** | GameManager, AudioManager | Point d'accès global, cycle de vie unique |
| **Strategy** | Training Modes | Comportements interchangeables par mode |
| **Observer** | EventBus | Découplage entre systèmes |
| **Object Pool** | Targets, Particles | Performance (éviter GC) |
| **State Machine** | Game Flow | Gestion états (Menu, Playing, Paused, Results) |
| **Factory** | Target Creation | Instanciation flexible par agent type |
| **ScriptableObject** | Settings, Agent Data | Data-driven design |

### 4.3 Diagramme de Classes Principal

```
                    ┌─────────────────┐
                    │   GameManager   │
                    │   <<Singleton>> │
                    └────────┬────────┘
                             │
        ┌────────────────────┼────────────────────┐
        │                    │                    │
        ▼                    ▼                    ▼
┌───────────────┐    ┌───────────────┐    ┌───────────────┐
│  ModeManager  │    │SessionManager │    │  UIManager    │
│   <<State>>   │    │  <<Stats>>    │    │  <<Views>>    │
└───────┬───────┘    └───────────────┘    └───────────────┘
        │
        ▼
┌───────────────────────────────────────┐
│           ITrainingMode               │
│              <<Interface>>            │
├───────────────────────────────────────┤
│ + Initialize()                        │
│ + OnTargetHit(HitInfo)               │
│ + OnTargetMissed()                   │
│ + SpawnNextTarget()                  │
│ + GetTargetBehavior(): TargetBehavior│
└───────────────────────────────────────┘
        △
        │
        ├──────────────┬──────────────┬──────────────┐
        │              │              │              │
┌───────┴──────┐┌─────┴─────┐┌───────┴──────┐┌─────┴─────┐
│  FlickMode   ││TrackingMode││  SpeedMode   ││HeadshotMode│
└──────────────┘└───────────┘└──────────────┘└───────────┘
```

### 4.4 Event System

```csharp
// Events principaux du jeu
public static class GameEvents
{
    // Session Events
    public static event Action OnSessionStart;
    public static event Action OnSessionEnd;
    public static event Action OnSessionPause;
    public static event Action OnSessionResume;

    // Target Events
    public static event Action<Target> OnTargetSpawned;
    public static event Action<HitInfo> OnTargetHit;
    public static event Action<Target> OnTargetMissed;
    public static event Action<Target> OnTargetDespawned;

    // Shooting Events
    public static event Action OnShoot;
    public static event Action<HitInfo> OnHit;
    public static event Action OnMiss;

    // UI Events
    public static event Action<GameState> OnGameStateChanged;
    public static event Action<SessionStats> OnStatsUpdated;
}
```

---

## 5. Spécifications Fonctionnelles

### 5.1 Modes d'Entraînement

#### 5.1.1 Mode Flick (MVP)

| Paramètre | Valeur |
|-----------|--------|
| **Description** | Cibles statiques, exercice de flick shots |
| **Comportement cible** | Apparaît → Reste immobile → Disparaît si touchée ou timeout |
| **Spawn pattern** | Aléatoire dans zone définie |
| **Timeout par cible** | 3 secondes |
| **Difficulté** | ★★☆☆☆ |

**Logique de spawn:**
```
1. Spawn cible à position aléatoire dans zone
2. Attendre hit OU timeout (3s)
3. Si hit: +score, enregistrer RT
4. Si timeout: cible disparaît (miss)
5. Spawn nouvelle cible immédiatement
6. Répéter jusqu'à fin de session
```

#### 5.1.2 Mode Tracking

| Paramètre | Valeur |
|-----------|--------|
| **Description** | Cible mobile, exercice de suivi continu |
| **Comportement cible** | Se déplace de façon fluide |
| **Pattern mouvement** | Sinusoïdal / Aléatoire smooth |
| **Vitesse** | Variable selon difficulté |
| **Difficulté** | ★★★☆☆ |

**Logique de scoring:**
```
1. Spawn cible mobile
2. Cible se déplace continuellement
3. Joueur doit maintenir le viseur sur la cible
4. Score basé sur % temps sur cible
5. Headshot bonus si sur la tête
```

#### 5.1.3 Mode Speed

| Paramètre | Valeur |
|-----------|--------|
| **Description** | Cibles rapides, exercice de réactivité |
| **Comportement cible** | Apparaît brièvement puis disparaît |
| **Durée affichage** | 0.5 - 1.5 secondes |
| **Spawn rate** | Élevé |
| **Difficulté** | ★★★★☆ |

#### 5.1.4 Mode Headshot

| Paramètre | Valeur |
|-----------|--------|
| **Description** | Focus précision, seuls les headshots comptent |
| **Comportement cible** | Statique ou légèrement mobile |
| **Scoring** | Headshot only (body shots = miss) |
| **Feedback** | Feedback négatif sur body shot |
| **Difficulté** | ★★★★☆ |

### 5.2 Système de Tir

#### 5.2.1 Raycast Shooting

```csharp
public class ShootingSystem : MonoBehaviour
{
    [SerializeField] private LayerMask targetLayer;
    [SerializeField] private LayerMask environmentLayer;

    private Camera mainCamera;

    public HitResult Shoot()
    {
        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, targetLayer))
        {
            // Déterminer si headshot ou bodyshot
            var hitZone = hit.collider.GetComponent<HitZone>();
            return new HitResult
            {
                IsHit = true,
                HitPoint = hit.point,
                HitNormal = hit.normal,
                IsHeadshot = hitZone?.IsHead ?? false,
                Target = hit.collider.GetComponentInParent<Target>()
            };
        }

        // Check environnement pour impact visuel
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, environmentLayer))
        {
            return new HitResult
            {
                IsHit = false,
                HitPoint = hit.point,
                HitNormal = hit.normal
            };
        }

        return HitResult.Miss;
    }
}
```

#### 5.2.2 Hit Detection

| Zone | Collider Type | Layer | Multiplicateur |
|------|--------------|-------|----------------|
| Head | Capsule/Sphere | Target_Head | x2.0 |
| Body | Capsule | Target_Body | x1.0 |

### 5.3 Système de Cibles

#### 5.3.1 Target Component

```csharp
public class Target : MonoBehaviour, IPoolable
{
    [Header("Configuration")]
    [SerializeField] private AgentData agentData;
    [SerializeField] private HitZone headHitZone;
    [SerializeField] private HitZone bodyHitZone;

    [Header("State")]
    public float SpawnTime { get; private set; }
    public bool IsActive { get; private set; }

    // Events
    public event Action<Target, HitInfo> OnHit;
    public event Action<Target> OnTimeout;

    public void Activate(Vector3 position, Quaternion rotation)
    {
        transform.SetPositionAndRotation(position, rotation);
        SpawnTime = Time.time;
        IsActive = true;
        gameObject.SetActive(true);
    }

    public void Deactivate()
    {
        IsActive = false;
        gameObject.SetActive(false);
    }

    public void RegisterHit(HitInfo hitInfo)
    {
        hitInfo.ReactionTime = Time.time - SpawnTime;
        OnHit?.Invoke(this, hitInfo);
    }
}
```

#### 5.3.2 Agent Data (ScriptableObject)

```csharp
[CreateAssetMenu(fileName = "AgentData", menuName = "ValorantAimTrainer/Agent Data")]
public class AgentData : ScriptableObject
{
    [Header("Identity")]
    public string AgentName;
    public string AgentId;
    public Sprite AgentIcon;

    [Header("Model")]
    public GameObject ModelPrefab;

    [Header("Hitbox Configuration")]
    public Vector3 HeadColliderCenter;
    public float HeadColliderRadius = 0.12f;
    public Vector3 BodyColliderCenter;
    public float BodyColliderHeight = 1.2f;
    public float BodyColliderRadius = 0.25f;

    [Header("Metadata")]
    public HitboxType HitboxCategory; // Slim, Standard, Large
    public string Description;
}

public enum HitboxType
{
    Slim,      // Jett, Neon
    Standard,  // Phoenix, Reyna
    Large      // Breach, Brimstone
}
```

#### 5.3.3 Target Spawner

```csharp
public class TargetSpawner : MonoBehaviour
{
    [Header("Spawn Configuration")]
    [SerializeField] private SpawnZone[] spawnZones;
    [SerializeField] private float minDistanceBetweenTargets = 2f;

    [Header("Pool")]
    [SerializeField] private int poolSizePerAgent = 5;

    private Dictionary<string, ObjectPool<Target>> targetPools;
    private List<Target> activeTargets = new();

    public Target SpawnTarget(AgentData agent, SpawnZone zone = null)
    {
        zone ??= GetRandomSpawnZone();
        Vector3 spawnPosition = zone.GetRandomPoint();

        // Valider position (pas trop proche d'autres cibles)
        int attempts = 0;
        while (!IsValidSpawnPosition(spawnPosition) && attempts < 10)
        {
            spawnPosition = zone.GetRandomPoint();
            attempts++;
        }

        var target = targetPools[agent.AgentId].Get();
        target.Activate(spawnPosition, GetLookAtPlayerRotation(spawnPosition));
        activeTargets.Add(target);

        GameEvents.RaiseTargetSpawned(target);
        return target;
    }

    private Quaternion GetLookAtPlayerRotation(Vector3 targetPos)
    {
        Vector3 dirToPlayer = (Camera.main.transform.position - targetPos).normalized;
        dirToPlayer.y = 0;
        return Quaternion.LookRotation(dirToPlayer);
    }
}
```

### 5.4 Système de Crosshair

#### 5.4.1 Structure du Crosshair Valorant

```
┌─────────────────────────────────────────────┐
│              CROSSHAIR LAYERS               │
├─────────────────────────────────────────────┤
│                                             │
│    Layer 4: OUTLINE (optionnel)             │
│    ┌───────────────────────┐                │
│    │  Layer 3: OUTER LINES │                │
│    │  ┌─────────────────┐  │                │
│    │  │ Layer 2: INNER  │  │                │
│    │  │ ┌─────────────┐ │  │                │
│    │  │ │ L1: CENTER  │ │  │                │
│    │  │ │    DOT      │ │  │                │
│    │  │ └─────────────┘ │  │                │
│    │  └─────────────────┘  │                │
│    └───────────────────────┘                │
│                                             │
└─────────────────────────────────────────────┘
```

#### 5.4.2 Crosshair Settings Data

```csharp
[CreateAssetMenu(fileName = "CrosshairSettings", menuName = "ValorantAimTrainer/Crosshair Settings")]
public class CrosshairSettings : ScriptableObject
{
    [Header("Color")]
    public Color CrosshairColor = Color.white;
    public bool UseCustomColor = false;
    public Color CustomColor = Color.white;

    [Header("Center Dot")]
    public bool ShowCenterDot = true;
    public float CenterDotSize = 2f;
    public float CenterDotOpacity = 1f;

    [Header("Inner Lines")]
    public bool ShowInnerLines = true;
    public float InnerLineLength = 6f;
    public float InnerLineThickness = 2f;
    public float InnerLineOffset = 3f;
    public float InnerLineOpacity = 1f;

    [Header("Outer Lines")]
    public bool ShowOuterLines = false;
    public float OuterLineLength = 2f;
    public float OuterLineThickness = 2f;
    public float OuterLineOffset = 10f;
    public float OuterLineOpacity = 1f;

    [Header("Outline")]
    public bool ShowOutline = true;
    public float OutlineThickness = 1f;
    public float OutlineOpacity = 0.5f;

    // Import/Export Valorant crosshair code
    public string ExportToCode() { /* ... */ }
    public void ImportFromCode(string code) { /* ... */ }
}
```

#### 5.4.3 Crosshair Renderer

```csharp
public class CrosshairRenderer : MonoBehaviour
{
    [SerializeField] private CrosshairSettings settings;

    [Header("UI References")]
    [SerializeField] private RectTransform centerDot;
    [SerializeField] private RectTransform[] innerLines; // 4 lines: up, down, left, right
    [SerializeField] private RectTransform[] outerLines;
    [SerializeField] private Image[] outlineImages;

    public void UpdateCrosshair()
    {
        UpdateCenterDot();
        UpdateLines(innerLines, settings.ShowInnerLines,
            settings.InnerLineLength, settings.InnerLineThickness,
            settings.InnerLineOffset, settings.InnerLineOpacity);
        UpdateLines(outerLines, settings.ShowOuterLines,
            settings.OuterLineLength, settings.OuterLineThickness,
            settings.OuterLineOffset, settings.OuterLineOpacity);
        UpdateOutline();
    }

    private void UpdateCenterDot()
    {
        centerDot.gameObject.SetActive(settings.ShowCenterDot);
        if (settings.ShowCenterDot)
        {
            centerDot.sizeDelta = Vector2.one * settings.CenterDotSize;
            var img = centerDot.GetComponent<Image>();
            img.color = new Color(
                settings.CrosshairColor.r,
                settings.CrosshairColor.g,
                settings.CrosshairColor.b,
                settings.CenterDotOpacity
            );
        }
    }
}
```

### 5.5 Système de Statistiques

#### 5.5.1 Session Stats

```csharp
[Serializable]
public class SessionStats
{
    // Identifiers
    public string SessionId;
    public DateTime StartTime;
    public DateTime EndTime;
    public float Duration;
    public string Mode;
    public string Agent;
    public string Map;

    // Core Stats
    public int TotalShots;
    public int TotalHits;
    public int TotalMisses;
    public int Headshots;
    public int Bodyshots;

    // Calculated
    public float Accuracy => TotalShots > 0 ? (float)TotalHits / TotalShots * 100f : 0f;
    public float HeadshotPercentage => TotalHits > 0 ? (float)Headshots / TotalHits * 100f : 0f;
    public float KillsPerMinute => Duration > 0 ? TotalHits / (Duration / 60f) : 0f;

    // Reaction Times
    public List<float> ReactionTimes = new();
    public float AverageReactionTime => ReactionTimes.Count > 0 ? ReactionTimes.Average() : 0f;
    public float BestReactionTime => ReactionTimes.Count > 0 ? ReactionTimes.Min() : 0f;
    public float WorstReactionTime => ReactionTimes.Count > 0 ? ReactionTimes.Max() : 0f;
    public float Consistency => CalculateStandardDeviation();

    private float CalculateStandardDeviation()
    {
        if (ReactionTimes.Count < 2) return 0f;
        float avg = AverageReactionTime;
        float sumSquares = ReactionTimes.Sum(rt => Mathf.Pow(rt - avg, 2));
        return Mathf.Sqrt(sumSquares / ReactionTimes.Count);
    }
}
```

#### 5.5.2 Stats Display

| Stat | Format | Update Rate |
|------|--------|-------------|
| Score | "0" | Real-time |
| Accuracy | "00.0%" | Real-time |
| Timer | "00:00" | Real-time |
| Last RT | "000ms" | On hit |
| Avg RT | "000ms" | On hit |
| Best RT | "000ms" | On hit |
| KPM | "0.0" | Every 5s |
| HS% | "00.0%" | On hit |

### 5.6 Hit Feedback System

#### 5.6.1 Visual Feedback

| Event | Visual Effect | Duration |
|-------|---------------|----------|
| Body Hit | Orange particles + flash | 0.3s |
| Headshot | Yellow particles + larger flash | 0.5s |
| Miss | Bullet hole decal (environment) | 5s |

```csharp
public class HitFeedbackSystem : MonoBehaviour
{
    [Header("Particle Systems")]
    [SerializeField] private ParticleSystem bodyHitParticles;
    [SerializeField] private ParticleSystem headshotParticles;
    [SerializeField] private GameObject bulletHoleDecal;

    [Header("Colors")]
    [SerializeField] private Color bodyHitColor = new Color(1f, 0.5f, 0f); // Orange
    [SerializeField] private Color headshotColor = new Color(1f, 1f, 0f);  // Yellow

    public void PlayHitEffect(HitInfo hitInfo)
    {
        var particles = hitInfo.IsHeadshot ? headshotParticles : bodyHitParticles;
        particles.transform.position = hitInfo.HitPoint;
        particles.transform.rotation = Quaternion.LookRotation(hitInfo.HitNormal);
        particles.Play();
    }

    public void PlayMissEffect(Vector3 position, Vector3 normal)
    {
        var decal = ObjectPool.Get(bulletHoleDecal);
        decal.transform.position = position + normal * 0.01f;
        decal.transform.rotation = Quaternion.LookRotation(-normal);
        StartCoroutine(ReturnToPoolAfter(decal, 5f));
    }
}
```

#### 5.6.2 Audio Feedback

| Event | Sound | Volume |
|-------|-------|--------|
| Shoot | Vandal-style shot | 100% SFX |
| Body Hit | Dull impact | 100% SFX |
| Headshot | Metallic "dink" | 100% SFX |
| Target Spawn | Subtle whoosh | 50% SFX |

---

## 6. Spécifications Techniques

### 6.1 Configuration Unity

#### 6.1.1 Project Settings

```yaml
Unity Version: 2022.3 LTS (ou Unity 6 LTS)
Render Pipeline: Universal Render Pipeline (URP)
Color Space: Linear
Scripting Backend: IL2CPP
API Compatibility: .NET Standard 2.1
```

#### 6.1.2 Quality Settings

```yaml
# Performance Preset (défaut)
VSync: Off (uncapped framerate)
Anti-Aliasing: MSAA 4x (configurable)
Shadow Quality: Medium
Shadow Distance: 50m
Texture Quality: Full Res
Anisotropic Filtering: Per Texture
```

#### 6.1.3 Input Settings

```yaml
# Mouse
Raw Mouse Input: Enabled
Mouse Sensitivity: 1.0 (default, 0.1-10.0 range)

# Controls
Left Click: Shoot
Escape: Pause Menu
R: Restart (in-game)
```

### 6.2 Layer Configuration

| Layer # | Name | Usage |
|---------|------|-------|
| 0 | Default | General |
| 6 | Target_Body | Raycast body detection |
| 7 | Target_Head | Raycast head detection |
| 8 | Environment | Map colliders |
| 9 | UI | UI elements |

### 6.3 Tag Configuration

| Tag | Usage |
|-----|-------|
| Target | All target GameObjects |
| SpawnZone | Spawn area markers |
| PlayerSpawn | Player spawn points |

### 6.4 Physics Configuration

```yaml
# Physics Settings
Fixed Timestep: 0.01 (100Hz)
Maximum Allowed Timestep: 0.1

# Layer Collision Matrix
# Désactiver collisions inutiles pour performance
Target_Body ↔ Target_Body: OFF
Target_Head ↔ Target_Head: OFF
Target_Body ↔ Target_Head: OFF
```

### 6.5 Required Packages

```json
{
  "dependencies": {
    "com.unity.render-pipelines.universal": "14.0.x",
    "com.unity.inputsystem": "1.7.x",
    "com.unity.textmeshpro": "3.0.x",
    "com.unity.ui": "1.0.x",
    "com.atteneder.gltfast": "6.0.x",
    "com.unity.nuget.newtonsoft-json": "3.2.x"
  }
}
```

---

## 7. Interface Utilisateur

### 7.1 Design System

#### 7.1.1 Palette de Couleurs

| Nom | Hex | Usage |
|-----|-----|-------|
| Primary Red | `#FF4655` | Accents, CTA buttons |
| Dark Background | `#0F1923` | Main background |
| Card Background | `#1F2937` | Cards, panels |
| Text Primary | `#FFFFFF` | Main text |
| Text Secondary | `#9CA3AF` | Secondary text |
| Success | `#10B981` | Positive stats |
| Warning | `#F59E0B` | Warnings |
| Error | `#EF4444` | Errors |

#### 7.1.2 Typographie

```css
/* Font Stack */
Primary: "DIN Next", "Roboto", sans-serif
Monospace: "JetBrains Mono", monospace

/* Sizes */
H1: 48px, Bold
H2: 32px, Bold
H3: 24px, Semibold
Body: 16px, Regular
Caption: 14px, Regular
Stats: 24px, Monospace, Bold
```

### 7.2 Écrans & Composants

#### 7.2.1 Main Menu

```
┌─────────────────────────────────────────────────────────────┐
│                                                             │
│                    VALORANT AIM TRAINER                     │
│                         [Logo]                              │
│                                                             │
│              ┌─────────────────────────────┐                │
│              │          PLAY              │                │
│              └─────────────────────────────┘                │
│                                                             │
│              ┌─────────────────────────────┐                │
│              │        SETTINGS            │                │
│              └─────────────────────────────┘                │
│                                                             │
│              ┌─────────────────────────────┐                │
│              │          QUIT              │                │
│              └─────────────────────────────┘                │
│                                                             │
│                                              v1.0.0         │
└─────────────────────────────────────────────────────────────┘
```

#### 7.2.2 Mode Selection

```
┌─────────────────────────────────────────────────────────────┐
│  ← Back                    SELECT MODE                      │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐         │
│  │             │  │             │  │             │         │
│  │    FLICK    │  │  TRACKING   │  │    SPEED    │         │
│  │     ★★☆     │  │    ★★★☆     │  │    ★★★★     │         │
│  │             │  │             │  │             │         │
│  │ Static      │  │ Moving      │  │ Fast        │         │
│  │ targets     │  │ targets     │  │ reactions   │         │
│  └─────────────┘  └─────────────┘  └─────────────┘         │
│                                                             │
│  ┌─────────────┐                                           │
│  │             │                                           │
│  │  HEADSHOT   │         [Mode Description]                │
│  │    ★★★★     │         Selected: FLICK                   │
│  │             │         Focus on precision                │
│  │ Headshots   │         flick shots to static             │
│  │ only        │         targets.                          │
│  └─────────────┘                                           │
│                                                             │
│                              ┌─────────────────────────┐    │
│                              │       CONTINUE →        │    │
│                              └─────────────────────────┘    │
└─────────────────────────────────────────────────────────────┘
```

#### 7.2.3 Agent Selection

```
┌─────────────────────────────────────────────────────────────┐
│  ← Back                   SELECT AGENT                      │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  ┌─────────┐  ┌─────────┐  ┌─────────┐  ┌─────────┐        │
│  │  [ISO]  │  │ [JETT]  │  │[PHOENIX]│  │ [LOCK]  │        │
│  │         │  │         │  │         │  │         │        │
│  │   ISO   │  │  JETT   │  │ PHOENIX │  │ COMING  │        │
│  │Standard │  │  Slim   │  │Standard │  │  SOON   │        │
│  └─────────┘  └─────────┘  └─────────┘  └─────────┘        │
│      ✓                                                      │
│                                                             │
│  ┌──────────────────────────────────────────────────────┐   │
│  │                     AGENT INFO                        │   │
│  │  ISO - Standard Hitbox                               │   │
│  │  The default training target with balanced           │   │
│  │  proportions matching standard agent hitboxes.       │   │
│  └──────────────────────────────────────────────────────┘   │
│                                                             │
│                              ┌─────────────────────────┐    │
│                              │       CONTINUE →        │    │
│                              └─────────────────────────┘    │
└─────────────────────────────────────────────────────────────┘
```

#### 7.2.4 In-Game HUD

```
┌─────────────────────────────────────────────────────────────┐
│  FLICK | ISO | THE RANGE                                    │
│                                                             │
│  ┌──────────┐                            ┌──────────┐       │
│  │  00:45   │                            │   89.5%  │       │
│  │  TIMER   │                            │ ACCURACY │       │
│  └──────────┘                            └──────────┘       │
│                                                             │
│                                                             │
│                                                             │
│                          [+]  ← Crosshair                   │
│                                                             │
│                                                             │
│                                                             │
│                                                             │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐    │
│  │    47    │  │  245ms   │  │  312ms   │  │   67%    │    │
│  │  SCORE   │  │ LAST RT  │  │  AVG RT  │  │   HS%    │    │
│  └──────────┘  └──────────┘  └──────────┘  └──────────┘    │
└─────────────────────────────────────────────────────────────┘
```

#### 7.2.5 Pause Menu

```
┌─────────────────────────────────────────────────────────────┐
│                                                             │
│                         PAUSED                              │
│                                                             │
│              ┌─────────────────────────────┐                │
│              │         RESUME             │                │
│              └─────────────────────────────┘                │
│                                                             │
│              ┌─────────────────────────────┐                │
│              │        RESTART             │                │
│              └─────────────────────────────┘                │
│                                                             │
│              ┌─────────────────────────────┐                │
│              │        SETTINGS            │                │
│              └─────────────────────────────┘                │
│                                                             │
│              ┌─────────────────────────────┐                │
│              │       QUIT TO MENU         │                │
│              └─────────────────────────────┘                │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

#### 7.2.6 Results Screen

```
┌─────────────────────────────────────────────────────────────┐
│                      SESSION COMPLETE                       │
│                   FLICK | ISO | THE RANGE                   │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│    ┌─────────────────────┐  ┌─────────────────────┐        │
│    │        SCORE        │  │      ACCURACY       │        │
│    │         47          │  │       89.5%         │        │
│    └─────────────────────┘  └─────────────────────┘        │
│                                                             │
│  ┌──────────────────────────────────────────────────────┐   │
│  │  REACTION TIMES                                       │   │
│  │  ├─ Average:    312ms                                │   │
│  │  ├─ Best:       187ms                                │   │
│  │  ├─ Worst:      534ms                                │   │
│  │  └─ Consistency: 78ms (σ)                            │   │
│  └──────────────────────────────────────────────────────┘   │
│                                                             │
│  ┌──────────────────────────────────────────────────────┐   │
│  │  BREAKDOWN                                            │   │
│  │  ├─ Headshots:   32 (68%)                            │   │
│  │  ├─ Bodyshots:   15 (32%)                            │   │
│  │  ├─ Misses:       5                                  │   │
│  │  └─ KPM:        47.0                                 │   │
│  └──────────────────────────────────────────────────────┘   │
│                                                             │
│    ┌─────────────────┐          ┌─────────────────┐        │
│    │     RETRY       │          │   MAIN MENU     │        │
│    └─────────────────┘          └─────────────────┘        │
└─────────────────────────────────────────────────────────────┘
```

#### 7.2.7 Settings Screen

```
┌─────────────────────────────────────────────────────────────┐
│  ← Back                     SETTINGS                        │
├─────────────────────────────────────────────────────────────┤
│  ┌──────────┬──────────┬──────────┐                        │
│  │ GAMEPLAY │ CROSSHAIR│  AUDIO   │  ← Tabs                │
│  └──────────┴──────────┴──────────┘                        │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  GAMEPLAY                                                   │
│  ─────────────────────────────────────────────             │
│                                                             │
│  Mouse Sensitivity                                          │
│  [========●=========] 1.0                                   │
│                                                             │
│  Session Duration                                           │
│  [▼ 60 seconds                              ]              │
│     ├─ 30 seconds                                          │
│     ├─ 60 seconds                                          │
│     ├─ 90 seconds                                          │
│     └─ 120 seconds                                         │
│                                                             │
│  Show Tutorial                                              │
│  [●] On   [ ] Off                                          │
│                                                             │
│                              ┌─────────────────────────┐    │
│                              │    APPLY & CLOSE        │    │
│                              └─────────────────────────┘    │
└─────────────────────────────────────────────────────────────┘
```

### 7.3 User Flow

```
                              ┌─────────────┐
                              │  LAUNCHER   │
                              └──────┬──────┘
                                     │
                                     ▼
                              ┌─────────────┐
                              │  MAIN MENU  │◄───────────────┐
                              └──────┬──────┘                │
                                     │                       │
                    ┌────────────────┼────────────────┐      │
                    │                │                │      │
                    ▼                ▼                ▼      │
             ┌──────────┐     ┌──────────┐    ┌──────────┐   │
             │   PLAY   │     │ SETTINGS │    │   QUIT   │   │
             └────┬─────┘     └──────────┘    └──────────┘   │
                  │                                          │
                  ▼                                          │
           ┌─────────────┐                                   │
           │MODE SELECT  │                                   │
           └──────┬──────┘                                   │
                  │                                          │
                  ▼                                          │
           ┌─────────────┐                                   │
           │AGENT SELECT │                                   │
           └──────┬──────┘                                   │
                  │                                          │
                  ▼                                          │
           ┌─────────────┐                                   │
           │ MAP SELECT  │                                   │
           └──────┬──────┘                                   │
                  │                                          │
                  ▼                                          │
           ┌─────────────┐                                   │
           │  COUNTDOWN  │                                   │
           │   3-2-1-GO  │                                   │
           └──────┬──────┘                                   │
                  │                                          │
                  ▼                                          │
           ┌─────────────┐                                   │
           │   PLAYING   │◄─────────────┐                    │
           └──────┬──────┘              │                    │
                  │                     │                    │
          ┌───────┴───────┐             │                    │
          │               │             │                    │
          ▼               ▼             │                    │
    ┌──────────┐   ┌──────────┐         │                    │
    │  PAUSE   │   │TIMER = 0 │         │                    │
    └────┬─────┘   └────┬─────┘         │                    │
         │              │               │                    │
         │              ▼               │                    │
         │       ┌──────────┐           │                    │
         │       │ RESULTS  │───────────┤                    │
         │       └────┬─────┘           │                    │
         │            │          [Retry]│                    │
         │            └─────────────────┼────────────────────┘
         │                    [Menu]    │
         │                              │
         └──────────────────────────────┘
              [Resume]
```

---

## 8. Système Audio

### 8.1 Sound Design

#### 8.1.1 Catégories Audio

| Catégorie | Channel | Volume Par Défaut |
|-----------|---------|-------------------|
| Master | Master | 100% |
| SFX | Gameplay | 100% |
| UI | Interface | 80% |
| Music | Ambient | 50% |

#### 8.1.2 Liste des Sons

| ID | Nom | Catégorie | Format | Durée | Trigger |
|----|-----|-----------|--------|-------|---------|
| SFX_001 | gun_shot | SFX | OGG | 0.3s | Left Click |
| SFX_002 | hit_body | SFX | OGG | 0.2s | Body hit |
| SFX_003 | hit_head_dink | SFX | OGG | 0.4s | Headshot |
| SFX_004 | target_spawn | SFX | OGG | 0.2s | Target appears |
| SFX_005 | countdown_tick | SFX | OGG | 0.1s | 3, 2, 1 |
| SFX_006 | countdown_go | SFX | OGG | 0.3s | GO! |
| SFX_007 | session_end | SFX | OGG | 1.0s | Timer = 0 |
| UI_001 | button_hover | UI | OGG | 0.1s | Hover button |
| UI_002 | button_click | UI | OGG | 0.1s | Click button |
| UI_003 | menu_back | UI | OGG | 0.1s | Back navigation |
| MUS_001 | menu_ambient | Music | OGG | Loop | Main menu |
| MUS_002 | training_ambient | Music | OGG | Loop | In-game (optionnel) |

#### 8.1.3 Audio Manager

```csharp
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource uiSource;
    [SerializeField] private AudioSource musicSource;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip gunShot;
    [SerializeField] private AudioClip hitBody;
    [SerializeField] private AudioClip hitHeadDink;
    [SerializeField] private AudioClip targetSpawn;
    // ... autres clips

    [Header("Settings")]
    [SerializeField] private AudioSettings settings;

    public void PlayGunShot() => PlaySFX(gunShot);
    public void PlayBodyHit() => PlaySFX(hitBody);
    public void PlayHeadshotDink() => PlaySFX(hitHeadDink);

    private void PlaySFX(AudioClip clip)
    {
        sfxSource.volume = settings.MasterVolume * settings.SFXVolume;
        sfxSource.PlayOneShot(clip);
    }
}
```

### 8.2 Audio Settings

```csharp
[CreateAssetMenu(fileName = "AudioSettings", menuName = "ValorantAimTrainer/Audio Settings")]
public class AudioSettings : ScriptableObject
{
    [Range(0f, 1f)] public float MasterVolume = 1f;
    [Range(0f, 1f)] public float SFXVolume = 1f;
    [Range(0f, 1f)] public float UIVolume = 0.8f;
    [Range(0f, 1f)] public float MusicVolume = 0.5f;
}
```

---

## 9. Gestion des Assets

### 9.1 Import 3D Models

#### 9.1.1 Workflow Import Agent

```
1. OBTENTION
   └─► Obtenir le fichier .glb/.fbx de l'agent (Valorant rip)

2. IMPORT UNITY
   └─► Placer dans Assets/Models/Agents/{AgentName}/
   └─► glTFast importe automatiquement

3. CONFIGURATION MODEL
   ├─► Scale: Ajuster à taille réelle (1.8m environ)
   ├─► Rotation: Face +Z
   └─► Materials: Assigner URP materials

4. HITBOX SETUP
   ├─► Ajouter Capsule Collider (Body) - Layer: Target_Body
   │   ├─► Center: (0, 0.9, 0)
   │   ├─► Radius: 0.25
   │   └─► Height: 1.2
   └─► Ajouter Sphere Collider (Head) - Layer: Target_Head
       ├─► Center: (0, 1.65, 0)
       └─► Radius: 0.12

5. PREFAB CREATION
   ├─► Ajouter Target component
   ├─► Ajouter HitZone components (Head + Body)
   ├─► Configurer références
   └─► Sauvegarder Prefab dans Assets/Prefabs/Agents/

6. SCRIPTABLE OBJECT
   └─► Créer AgentData asset
       ├─► Assigner model prefab
       ├─► Configurer hitbox parameters
       └─► Ajouter à liste des agents disponibles
```

#### 9.1.2 Workflow Import Map

```
1. OBTENTION
   └─► Obtenir le fichier .glb/.fbx de la map

2. IMPORT UNITY
   └─► Placer dans Assets/Models/Maps/{MapName}/

3. CONFIGURATION MODEL
   ├─► Scale: Vérifier échelle (1 unit = 1 meter)
   └─► Materials: Assigner URP materials

4. COLLIDERS SETUP
   ├─► Ajouter MeshColliders pour sol/murs
   │   └─► Layer: Environment
   └─► Optimiser avec collision meshes simplifiés

5. SPAWN POINTS
   ├─► Placer PlayerSpawnPoint empty objects
   │   └─► Tag: PlayerSpawn
   └─► Placer SpawnZone objects pour targets
       └─► Tag: SpawnZone, component: SpawnZone

6. SCENE SETUP
   ├─► Créer Scene Assets/Scenes/Maps/{MapName}
   ├─► Configurer Lighting (Baked ou Realtime)
   ├─► Ajouter NavMesh (si targets mobiles)
   └─► Optimiser (LOD, Occlusion Culling)

7. SCRIPTABLE OBJECT
   └─► Créer MapData asset
       └─► Assigner scene reference
```

### 9.2 Asset Organization

```
Assets/
├── _Project/
│   ├── Scripts/
│   │   ├── Core/
│   │   │   ├── GameManager.cs
│   │   │   ├── EventBus.cs
│   │   │   ├── ObjectPool.cs
│   │   │   └── SaveSystem.cs
│   │   ├── Gameplay/
│   │   │   ├── ShootingSystem.cs
│   │   │   ├── Target.cs
│   │   │   ├── TargetSpawner.cs
│   │   │   └── HitZone.cs
│   │   ├── Modes/
│   │   │   ├── ITrainingMode.cs
│   │   │   ├── FlickMode.cs
│   │   │   ├── TrackingMode.cs
│   │   │   ├── SpeedMode.cs
│   │   │   └── HeadshotMode.cs
│   │   ├── UI/
│   │   │   ├── UIManager.cs
│   │   │   ├── HUDController.cs
│   │   │   ├── CrosshairRenderer.cs
│   │   │   └── Screens/
│   │   ├── Audio/
│   │   │   └── AudioManager.cs
│   │   ├── Data/
│   │   │   ├── SessionStats.cs
│   │   │   ├── GameSettings.cs
│   │   │   └── CrosshairSettings.cs
│   │   └── Utilities/
│   │       ├── Extensions.cs
│   │       └── Constants.cs
│   │
│   ├── ScriptableObjects/
│   │   ├── Agents/
│   │   │   ├── ISO_Data.asset
│   │   │   ├── Jett_Data.asset
│   │   │   └── Phoenix_Data.asset
│   │   ├── Maps/
│   │   │   ├── TheRange_Data.asset
│   │   │   └── Ascent_Data.asset
│   │   └── Settings/
│   │       ├── DefaultCrosshair.asset
│   │       ├── DefaultAudio.asset
│   │       └── DefaultGameplay.asset
│   │
│   ├── Prefabs/
│   │   ├── Agents/
│   │   │   ├── ISO.prefab
│   │   │   ├── Jett.prefab
│   │   │   └── Phoenix.prefab
│   │   ├── Effects/
│   │   │   ├── HitParticles_Body.prefab
│   │   │   ├── HitParticles_Head.prefab
│   │   │   └── BulletHole.prefab
│   │   └── UI/
│   │       └── Crosshair.prefab
│   │
│   ├── Scenes/
│   │   ├── _Preload.unity
│   │   ├── MainMenu.unity
│   │   └── Maps/
│   │       ├── TheRange.unity
│   │       └── Ascent.unity
│   │
│   ├── UI/
│   │   ├── Sprites/
│   │   ├── Fonts/
│   │   └── USS/ (UI Toolkit stylesheets)
│   │
│   └── Audio/
│       ├── SFX/
│       ├── UI/
│       └── Music/
│
├── Models/
│   ├── Agents/
│   │   ├── ISO/
│   │   ├── Jett/
│   │   └── Phoenix/
│   └── Maps/
│       ├── TheRange/
│       └── Ascent/
│
└── Settings/
    ├── URP_Settings.asset
    ├── URP_Renderer.asset
    └── Quality/
```

---

## 10. Performance & Optimisation

### 10.1 Objectifs Performance

| Métrique | Target | Priorité |
|----------|--------|----------|
| Frame Rate | ≥144 FPS stable | P0 |
| Input Latency | <10ms | P0 |
| Memory Usage | <2GB RAM | P1 |
| Load Time | <3s | P1 |
| GPU Usage | <50% (RTX 3060) | P2 |

### 10.2 Stratégies d'Optimisation

#### 10.2.1 CPU Optimization

```csharp
// ❌ ÉVITER - Allocation chaque frame
void Update()
{
    var targets = FindObjectsOfType<Target>();
    foreach (var t in targets) { /* ... */ }
}

// ✅ PRÉFÉRER - Cache et events
private List<Target> activeTargets = new();

void OnTargetSpawned(Target t) => activeTargets.Add(t);
void OnTargetDespawned(Target t) => activeTargets.Remove(t);
```

#### 10.2.2 Object Pooling

```csharp
public class ObjectPool<T> where T : Component, IPoolable
{
    private readonly Queue<T> pool = new();
    private readonly T prefab;
    private readonly Transform parent;

    public T Get()
    {
        if (pool.TryDequeue(out T obj))
        {
            obj.gameObject.SetActive(true);
            obj.OnSpawn();
            return obj;
        }

        var newObj = Object.Instantiate(prefab, parent);
        newObj.OnSpawn();
        return newObj;
    }

    public void Return(T obj)
    {
        obj.OnDespawn();
        obj.gameObject.SetActive(false);
        pool.Enqueue(obj);
    }
}
```

#### 10.2.3 Rendering Optimization

| Technique | Usage | Impact |
|-----------|-------|--------|
| LOD Groups | Maps complexes | High |
| Occlusion Culling | Maps indoor | Medium |
| GPU Instancing | Multiple targets | Medium |
| Baked Lighting | Static environments | High |
| Texture Compression | All textures | Medium |

#### 10.2.4 Memory Management

```csharp
// Précharger les assets nécessaires
public class AssetPreloader : MonoBehaviour
{
    [SerializeField] private AgentData[] agentsToPreload;
    [SerializeField] private AudioClip[] audioToPreload;

    private async void Start()
    {
        foreach (var agent in agentsToPreload)
        {
            // Instantiate et pool
            var pool = new ObjectPool<Target>(agent.ModelPrefab);
            pool.Prewarm(5);
        }
    }
}
```

### 10.3 Profiling Checklist

```
□ CPU Usage < 30% on target hardware
□ No GC allocations during gameplay
□ Draw calls < 100 per frame
□ SetPass calls < 50 per frame
□ Physics.Raycast < 1ms per frame
□ No frame drops below 144 FPS
□ Memory stable (no leaks over 10min session)
```

---

## 11. Structure du Projet Unity

### 11.1 Scene Management

```csharp
public enum GameScene
{
    Preload,     // Persistent managers, loading
    MainMenu,    // Menu principal
    Training     // Maps de training (chargée additivement)
}

public class SceneLoader : MonoBehaviour
{
    public async Task LoadTrainingSession(MapData map, AgentData agent, ITrainingMode mode)
    {
        // 1. Show loading screen
        UIManager.Instance.ShowLoadingScreen();

        // 2. Load map scene additively
        await SceneManager.LoadSceneAsync(map.SceneName, LoadSceneMode.Additive);

        // 3. Initialize systems
        GameManager.Instance.InitializeSession(map, agent, mode);

        // 4. Hide loading, show HUD
        UIManager.Instance.HideLoadingScreen();
        UIManager.Instance.ShowHUD();
    }
}
```

### 11.2 Bootstrap / Preload

```csharp
// _Preload scene - Always loaded first
public class Bootstrap : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Init()
    {
        // Ensure _Preload is first scene
        if (SceneManager.GetActiveScene().name != "_Preload")
        {
            SceneManager.LoadScene("_Preload");
        }
    }

    private void Awake()
    {
        // Initialize managers
        DontDestroyOnLoad(gameObject);

        // Load settings
        SaveSystem.LoadSettings();

        // Apply settings
        ApplySettings();

        // Load main menu
        SceneManager.LoadScene("MainMenu", LoadSceneMode.Additive);
    }
}
```

### 11.3 Game State Machine

```csharp
public enum GameState
{
    MainMenu,
    ModeSelect,
    AgentSelect,
    MapSelect,
    Loading,
    Countdown,
    Playing,
    Paused,
    Results
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public GameState CurrentState { get; private set; }

    private readonly Dictionary<GameState, IGameState> states = new();

    public void ChangeState(GameState newState)
    {
        states[CurrentState]?.Exit();
        CurrentState = newState;
        states[newState]?.Enter();

        GameEvents.RaiseGameStateChanged(newState);
    }
}
```

---

## 12. Roadmap & Livrables

### 12.1 Phase 1: MVP (Semaines 1-3)

#### Semaine 1: Foundation
| Task | Priorité | Livrable |
|------|----------|----------|
| Setup projet Unity + URP | P0 | Project configured |
| Architecture de base (GameManager, Events) | P0 | Core scripts |
| Import glTFast + test | P0 | Working import |
| Player controller (FPS camera) | P0 | Player prefab |

#### Semaine 2: Core Gameplay
| Task | Priorité | Livrable |
|------|----------|----------|
| Import modèle ISO | P0 | ISO prefab with hitbox |
| Système de tir (Raycast) | P0 | ShootingSystem.cs |
| Target system (spawn, hit, despawn) | P0 | Target components |
| Mode Flick basique | P0 | FlickMode.cs |

#### Semaine 3: MVP Polish
| Task | Priorité | Livrable |
|------|----------|----------|
| Map placeholder ou The Range | P0 | Training scene |
| HUD basique (timer, score) | P0 | HUD prefab |
| Session stats | P0 | SessionStats.cs |
| Menu minimal | P0 | Main menu scene |
| Basic crosshair | P0 | Crosshair UI |

**MVP Deliverable**: Session jouable de 60s en mode Flick

### 12.2 Phase 2: Core Features (Semaines 4-5)

#### Semaine 4: Modes & Crosshair
| Task | Priorité | Livrable |
|------|----------|----------|
| Mode Tracking | P1 | TrackingMode.cs |
| Mode Speed | P1 | SpeedMode.cs |
| Mode Headshot | P1 | HeadshotMode.cs |
| Crosshair system complet | P1 | CrosshairRenderer.cs |

#### Semaine 5: Feedback & Settings
| Task | Priorité | Livrable |
|------|----------|----------|
| Hit particles (body/head) | P1 | Particle prefabs |
| Audio system + sons | P1 | AudioManager + clips |
| Settings UI complet | P1 | Settings screens |
| Save/Load settings | P1 | SaveSystem.cs |

### 12.3 Phase 3: Content (Semaines 6-7)

#### Semaine 6: Agents
| Task | Priorité | Livrable |
|------|----------|----------|
| Import Jett | P1 | Jett prefab |
| Import Phoenix | P1 | Phoenix prefab |
| Agent selection UI | P1 | Agent select screen |
| Ajustement hitboxes | P1 | Calibrated colliders |

#### Semaine 7: Maps & Polish
| Task | Priorité | Livrable |
|------|----------|----------|
| Import Ascent (ou zone) | P1 | Ascent scene |
| Map selection UI | P1 | Map select screen |
| Audio polish | P2 | Additional SFX |
| Performance pass | P2 | Optimized build |

### 12.4 Phase 4: Polish (Semaine 8)

| Task | Priorité | Livrable |
|------|----------|----------|
| UI animations | P2 | Polished transitions |
| Bug fixes | P0 | Stable build |
| Playtesting | P0 | Bug reports addressed |
| Build final Windows | P0 | Release build |
| Documentation | P2 | User guide |

### 12.5 Milestones Summary

| Milestone | Date | Critères |
|-----------|------|----------|
| **MVP** | Fin Semaine 3 | Mode Flick jouable, 1 agent, stats basiques |
| **Alpha** | Fin Semaine 5 | 4 modes, crosshair complet, settings |
| **Beta** | Fin Semaine 7 | 3 agents, 2 maps, audio complet |
| **Release** | Fin Semaine 8 | Stable, optimisé, testé |

---

## 13. Critères d'Acceptation

### 13.1 MVP Acceptance Criteria

| # | Critère | Test |
|---|---------|------|
| MVP-01 | Peut compléter une session de 60s en mode Flick | Start → Play 60s → Results screen |
| MVP-02 | Cible ISO s'affiche correctement | Visual inspection |
| MVP-03 | Hits détectés (body et head) | Shoot target → feedback |
| MVP-04 | Stats affichées en fin de session | Complete session → check results |
| MVP-05 | Crosshair visible et centré | Visual inspection |
| MVP-06 | Son de tir joué | Click → audio plays |
| MVP-07 | Son "dink" sur headshot | Headshot → distinctive sound |

### 13.2 V1.0 Acceptance Criteria

| # | Critère | Test |
|---|---------|------|
| V1-01 | 4 modes jouables et fonctionnels | Test each mode |
| V1-02 | Au moins 2 agents sélectionnables | Agent select → play with each |
| V1-03 | Au moins 1 map Valorant | Visual inspection |
| V1-04 | Settings persistent après restart | Change settings → restart → verify |
| V1-05 | Crosshair fully customizable | All options functional |
| V1-06 | 144 FPS stable | Profiler verification |
| V1-07 | Input latency < 10ms | Input lag measurement |
| V1-08 | No crashes in 1h playtest | Extended playtest |

### 13.3 Definition of Done (DoD)

Pour chaque feature:
- [ ] Code écrit et fonctionnel
- [ ] Pas d'erreurs/warnings dans Console
- [ ] Testé manuellement
- [ ] Performance acceptable (pas de frame drops)
- [ ] Code review (self-review minimum)
- [ ] Documenté si complexe

---

## 14. Risques & Mitigations

### 14.1 Risques Techniques

| Risque | Probabilité | Impact | Mitigation |
|--------|-------------|--------|------------|
| Modèles GLB incompatibles | Medium | High | Tester glTFast early, avoir fallback FBX |
| Performance insuffisante | Low | High | Profiler dès MVP, LOD, pooling |
| Hitboxes imprécises | Medium | Medium | Calibration avec références Valorant |
| Audio désync | Low | Low | Audio pooling, preload |

### 14.2 Risques Projet

| Risque | Probabilité | Impact | Mitigation |
|--------|-------------|--------|------------|
| Assets non disponibles | Medium | High | Commencer par placeholder, plan B |
| Scope creep | High | Medium | Strict MVP, features en backlog |
| Bugs bloquants | Medium | High | Testing régulier, builds fréquents |

### 14.3 Risques Légaux

| Risque | Probabilité | Impact | Mitigation |
|--------|-------------|--------|------------|
| DMCA Riot Games | Low | Critical | Usage personnel UNIQUEMENT, pas de distribution |

---

## 15. Annexes

### 15.1 Glossaire

| Terme | Définition |
|-------|------------|
| **Flick** | Mouvement rapide de la souris pour viser une cible |
| **Tracking** | Suivi continu d'une cible mobile |
| **RT / Reaction Time** | Temps entre apparition de la cible et le hit |
| **Dink** | Son métallique distinctif lors d'un headshot dans Valorant |
| **KPM** | Kills Per Minute |
| **Hitbox** | Zone de collision pour la détection des hits |

### 15.2 Références

- [Unity 2022.3 LTS Documentation](https://docs.unity3d.com/2022.3/Documentation/Manual/)
- [glTFast Documentation](https://docs.unity3d.com/Packages/com.unity.cloud.gltfast@6.0/)
- [URP Documentation](https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@14.0/)
- [Valorant Crosshair Guide](https://playvalorant.com/en-us/news/game-updates/valorant-crosshair-guide/)

### 15.3 Contacts & Ressources

| Ressource | Lien/Note |
|-----------|-----------|
| Modèles Valorant | À sourcer par l'utilisateur |
| Sons SFX | À créer ou sourcer (royalty-free) |
| Fonts | Google Fonts (Roboto, DIN alternatives) |

### 15.4 Changelog PRD

| Version | Date | Changements |
|---------|------|-------------|
| 1.0.0 | 2026-02-01 | Version initiale générée depuis 00_VISION.md |

---

*Ce document est la source de vérité pour le développement du Valorant Aim Trainer. Toute modification doit être versionnée et documentée.*
