# 04 — Système de Tir

> **Priorité**: P0 (MVP)
> **Statut**: Draft

---

## 1. Vue d'Ensemble

### 1.1 Concept

Le système de tir utilise un raycast depuis le centre de l'écran pour détecter les impacts sur les cibles et l'environnement.

### 1.2 Caractéristiques

| Caractéristique | Valeur |
|-----------------|--------|
| Type | Hitscan (raycast instantané) |
| Origin | Centre de la caméra |
| Range | Infini (limité par map) |
| Rate of Fire | Semi-auto (1 tir par clic) |
| Ammunition | Infinie |
| Spread | Aucun (100% précis) |

---

## 2. Raycast System

### 2.1 Configuration du Raycast

| Paramètre | Valeur | Notes |
|-----------|--------|-------|
| Origin | Camera.ViewportToWorldPoint(0.5, 0.5) | Centre exact de l'écran |
| Direction | Camera.forward | Direction du regard |
| Max Distance | Mathf.Infinity | Pas de limite |
| Layer Mask | Target_Head, Target_Body, Environment | Dans cet ordre |
| Query Trigger | Yes | Pour détecter les colliders trigger |

### 2.2 Ordre de Détection

```
Raycast
   │
   ├─► Priority 1: Target_Head (Layer 7)
   │   └── If hit → HEADSHOT
   │
   ├─► Priority 2: Target_Body (Layer 6)
   │   └── If hit → BODYSHOT
   │
   ├─► Priority 3: Environment (Layer 8)
   │   └── If hit → MISS (avec point d'impact)
   │
   └─► No hit → MISS (sans impact)
```

### 2.3 Gestion des Overlaps

Si le raycast traverse plusieurs colliders (ex: head devant body), Unity retourne le **premier hit** dans la direction du ray. Configuration:

- Head collider positionné **devant** le body (vers le joueur)
- Ou utiliser RaycastAll et trier par distance

**Approche recommandée**: Raycast séquentiel par layer

```
1. Raycast Target_Head only
   - If hit: return HEADSHOT

2. Raycast Target_Body only
   - If hit: return BODYSHOT

3. Raycast Environment only
   - If hit: return MISS with impact

4. No hit: return MISS without impact
```

---

## 3. Input Handling

### 3.1 Input Configuration

| Input | Action | System |
|-------|--------|--------|
| Left Mouse Button | Shoot | New Input System |
| Mouse Delta | Look | New Input System |

### 3.2 Input Action Map

| Action | Type | Binding |
|--------|------|---------|
| Shoot | Button | Mouse/leftButton |
| Look | Vector2 | Mouse/delta |
| Pause | Button | Keyboard/escape |

### 3.3 Input Processing

```
OnShootInput (InputAction callback)
│
├── Check: Game State == Playing?
│   └── No → Ignore input
│
├── Check: Not paused?
│   └── Paused → Ignore input
│
└── Execute Shoot()
    ├── Perform raycast
    ├── Process result
    ├── Trigger feedback
    └── Update stats
```

### 3.4 Timing Considerations

| Aspect | Handling |
|--------|----------|
| Input Lag | Minimisé via Raw Mouse Input |
| Frame Independence | Shoot immédiat, pas dans FixedUpdate |
| Double Click Prevention | N/A (semi-auto, un tir par press) |

---

## 4. Hit Processing

### 4.1 Hit Result Types

| Type | IsHit | IsHeadshot | Target | Impact Point |
|------|-------|------------|--------|--------------|
| HEADSHOT | true | true | ref | yes |
| BODYSHOT | true | false | ref | yes |
| MISS_ENV | false | false | null | yes |
| MISS_AIR | false | false | null | no |

### 4.2 Hit Info Structure

| Field | Type | Description |
|-------|------|-------------|
| ResultType | Enum | HEADSHOT, BODYSHOT, MISS_ENV, MISS_AIR |
| IsHit | bool | Cible touchée |
| IsHeadshot | bool | Tête touchée |
| HitPoint | Vector3 | Point d'impact (world space) |
| HitNormal | Vector3 | Normale de surface |
| Target | Target | Référence cible (null si miss) |
| HitCollider | Collider | Collider touché |
| Distance | float | Distance du tir |
| Timestamp | float | Time.time au moment du hit |

### 4.3 Processing Pipeline

```
Shoot()
   │
   ▼
Perform Raycast
   │
   ▼
Build HitInfo
   │
   ├─► [HIT] ──────────────────────────────┐
   │   │                                   │
   │   ▼                                   │
   │   Calculate Reaction Time             │
   │   │                                   │
   │   ▼                                   │
   │   Notify Target (RegisterHit)         │
   │   │                                   │
   │   ▼                                   │
   │   Trigger OnTargetHit Event           │
   │                                       │
   └─► [MISS] ─────────────────────────────┤
       │                                   │
       ▼                                   │
       Trigger OnMiss Event                │
                                           │
                                           ▼
                                   Play Feedback
                                   (Visual + Audio)
                                           │
                                           ▼
                                   Update Stats
                                           │
                                           ▼
                                   [HIT] Spawn Next Target
```

---

## 5. Reaction Time Calculation

### 5.1 Définition

**Reaction Time (RT)** = Temps entre l'apparition de la cible et le moment où le joueur la touche.

### 5.2 Calcul

```
ReactionTime = HitTimestamp - Target.SpawnTime

Où:
- HitTimestamp = Time.time au moment du Shoot()
- Target.SpawnTime = Time.time au moment de l'activation
```

### 5.3 Précision

| Aspect | Valeur |
|--------|--------|
| Unité | Millisecondes (ms) |
| Précision | ~1ms (dépend du framerate) |
| Affichage | Arrondi à l'entier |

### 5.4 Edge Cases

| Cas | Handling |
|-----|----------|
| Target hit frame 0 | RT = delta time (~7ms à 144fps) |
| Very slow RT | Pas de limite max |
| Negative RT | Impossible (log error si détecté) |

---

## 6. Feedback System

### 6.1 Vue d'Ensemble

| Event | Visual Feedback | Audio Feedback |
|-------|-----------------|----------------|
| Shoot | Aucun (pas de muzzle flash) | Gun shot sound |
| Headshot | Yellow particles on target | Dink sound |
| Bodyshot | Orange particles on target | Hit sound |
| Miss (env) | Bullet hole decal | Impact sound (optionnel) |
| Miss (air) | Aucun | Aucun |

### 6.2 Visual Feedback

#### Headshot Particles

| Propriété | Valeur |
|-----------|--------|
| Color | Yellow (#FFD700) |
| Shape | Burst, outward |
| Count | 15-20 particles |
| Lifetime | 0.3s |
| Size | Small (0.05m) |
| Position | Hit point |
| Rotation | Aligned with hit normal |

#### Bodyshot Particles

| Propriété | Valeur |
|-----------|--------|
| Color | Orange (#FF8C00) |
| Shape | Burst, outward |
| Count | 10-15 particles |
| Lifetime | 0.25s |
| Size | Small (0.04m) |
| Position | Hit point |
| Rotation | Aligned with hit normal |

#### Bullet Hole Decal

| Propriété | Valeur |
|-----------|--------|
| Size | 0.05m |
| Lifetime | 5 seconds |
| Max Count | 20 (pool) |
| Position | Impact point + offset |
| Rotation | Aligned with surface normal |

### 6.3 Audio Feedback

| Sound | Trigger | Priority | Volume |
|-------|---------|----------|--------|
| Gun Shot | Every shoot | High | 100% SFX |
| Dink (Headshot) | On headshot | Highest | 100% SFX |
| Body Hit | On bodyshot | High | 100% SFX |
| Impact (Env) | On miss env | Low | 50% SFX |

### 6.4 Feedback Timing

```
Shoot Input
    │
    ├── Immediately: Play gun shot sound
    │
    ├── Same frame: Perform raycast
    │
    └── Same frame: Based on result
        │
        ├── [Headshot]
        │   ├── Spawn head particles at hit point
        │   └── Play dink sound
        │
        ├── [Bodyshot]
        │   ├── Spawn body particles at hit point
        │   └── Play body hit sound
        │
        └── [Miss Env]
            └── Spawn bullet hole at hit point
```

---

## 7. Stats Integration

### 7.1 Stats Updated on Shoot

| Stat | Update Logic |
|------|--------------|
| TotalShots | +1 on every shoot |
| TotalHits | +1 if IsHit |
| TotalMisses | +1 if !IsHit |
| Headshots | +1 if IsHeadshot |
| Bodyshots | +1 if IsHit && !IsHeadshot |
| ReactionTimes | Add RT if IsHit |
| Accuracy | Recalculate (Hits/Shots × 100) |
| HeadshotPercent | Recalculate (Headshots/Hits × 100) |

### 7.2 Event Flow

```
OnShoot Event
    │
    ├── SessionManager.OnShoot()
    │   └── TotalShots++
    │
    └── [If Hit] OnHit Event
        │
        └── SessionManager.OnHit(hitInfo)
            ├── TotalHits++
            ├── ReactionTimes.Add(hitInfo.RT)
            ├── If headshot: Headshots++
            └── Else: Bodyshots++
```

---

## 8. Performance Considerations

### 8.1 Raycast Optimization

| Technique | Implementation |
|-----------|----------------|
| Layer Mask | Limiter aux layers nécessaires |
| Single Raycast | Un seul raycast par tir (pas RaycastAll) |
| No Physics Queries in Update | Seulement sur input |

### 8.2 Object Pooling

| Object | Pool Size | Reuse Strategy |
|--------|-----------|----------------|
| Hit Particles | 20 | Return after lifetime |
| Bullet Holes | 20 | Return after 5s or when pool full |
| Audio Sources | 5 | Return after clip ends |

### 8.3 Frame Budget

| Operation | Target Time |
|-----------|-------------|
| Raycast | < 0.1ms |
| Feedback Spawn | < 0.5ms |
| Stats Update | < 0.1ms |
| **Total Shoot** | < 1ms |

---

## 9. Edge Cases & Error Handling

### 9.1 Edge Cases

| Situation | Handling |
|-----------|----------|
| Shoot while paused | Input ignored |
| Shoot during countdown | Input ignored |
| Target despawns mid-raycast | Check target validity |
| Multiple targets same position | First hit by raycast wins |
| Shoot at exact head/body boundary | Head takes priority |

### 9.2 Error Handling

| Error | Response |
|-------|----------|
| Raycast returns null | Treat as MISS_AIR |
| Target reference lost | Log warning, continue |
| Particle pool empty | Skip visual feedback |
| Audio source unavailable | Skip audio feedback |

---

## 10. Debug Features

### 10.1 Debug Visualization

| Feature | Key | Description |
|---------|-----|-------------|
| Show Raycast | F1 | Draw ray in scene view |
| Show Hit Point | F2 | Sphere at impact point |
| Show Hitboxes | F3 | Wireframe colliders |

### 10.2 Debug Logging

| Log Level | Content |
|-----------|---------|
| Info | Hit/Miss results |
| Warning | Pool empty, fallbacks |
| Error | Invalid states, null refs |

### 10.3 Debug UI (Dev Only)

| Display | Value |
|---------|-------|
| Last Hit Type | HEADSHOT/BODYSHOT/MISS |
| Last RT | 000 ms |
| Raycast Time | 0.00 ms |
| Hit Point | (x, y, z) |

---

## 11. Configuration Summary

### 11.1 Shooting System Settings

| Setting | Value | Configurable |
|---------|-------|--------------|
| Fire Mode | Semi-Auto | No |
| Fire Rate | Unlimited | No |
| Accuracy | 100% | No |
| Range | Infinite | No |
| Ammo | Infinite | No |

### 11.2 Layer Masks

| Mask Name | Layers Included |
|-----------|-----------------|
| TargetMask | Target_Head, Target_Body |
| EnvironmentMask | Environment |
| AllShootable | Target_Head, Target_Body, Environment |

---

*Voir [05_CROSSHAIR.md](./05_CROSSHAIR.md) pour le système de viseur.*
