# 03 — Système de Cibles

> **Priorité**: P0 (MVP)
> **Statut**: Draft

---

## 1. Vue d'Ensemble

### 1.1 Concept

Les cibles sont des représentations 3D des agents Valorant avec des hitboxes fidèles au jeu original, permettant un entraînement réaliste.

### 1.2 Composants d'une Cible

```
Target (GameObject)
│
├── Model (3D mesh de l'agent)
│   ├── Skeleton/Rig
│   ├── Materials
│   └── Animations (optionnel)
│
├── Hitboxes
│   ├── Head Collider (Sphere)
│   └── Body Collider (Capsule)
│
├── Components
│   ├── Target (script principal)
│   ├── HitZone (x2 - head & body)
│   └── TargetAnimator (optionnel)
│
└── VFX Attachment Points
    ├── HeadHitPoint
    └── BodyHitPoint
```

---

## 2. Agents Disponibles

### 2.1 Liste des Agents

| Agent | Priorité | Hitbox Type | Status | Notes |
|-------|----------|-------------|--------|-------|
| ISO | P0 | Standard | MVP | Premier agent, template |
| Jett | P1 | Slim | V1.0 | Hitbox fine, bon pour précision |
| Phoenix | P1 | Standard | V1.0 | Hitbox référence |
| Breach | P2 | Large | Future | Hitbox large |
| Cypher | P2 | Standard | Future | - |
| Reyna | P2 | Standard | Future | - |
| Neon | P3 | Slim | Future | Hitbox très fine |
| Brimstone | P3 | Large | Future | - |

### 2.2 Catégories de Hitbox

| Catégorie | Agents | Caractéristiques |
|-----------|--------|------------------|
| **Slim** | Jett, Neon | Corps fin, tête standard |
| **Standard** | ISO, Phoenix, Reyna, Cypher | Proportions moyennes |
| **Large** | Breach, Brimstone | Corps plus large |

### 2.3 Fiche Agent: ISO (Template)

| Propriété | Valeur |
|-----------|--------|
| **Nom** | ISO |
| **ID** | agent_iso |
| **Catégorie** | Standard |
| **Hauteur totale** | ~1.85m |
| **Largeur épaules** | ~0.45m |

#### Hitbox Specifications

| Zone | Type | Position (local) | Dimensions |
|------|------|------------------|------------|
| Head | Sphere | (0, 1.65, 0) | Radius: 0.11m |
| Body | Capsule | (0, 0.95, 0) | Height: 1.1m, Radius: 0.22m |

### 2.4 Fiche Agent: Jett

| Propriété | Valeur |
|-----------|--------|
| **Nom** | Jett |
| **ID** | agent_jett |
| **Catégorie** | Slim |
| **Hauteur totale** | ~1.75m |
| **Largeur épaules** | ~0.40m |

#### Hitbox Specifications

| Zone | Type | Position (local) | Dimensions |
|------|------|------------------|------------|
| Head | Sphere | (0, 1.60, 0) | Radius: 0.10m |
| Body | Capsule | (0, 0.90, 0) | Height: 1.0m, Radius: 0.18m |

### 2.5 Fiche Agent: Phoenix

| Propriété | Valeur |
|-----------|--------|
| **Nom** | Phoenix |
| **ID** | agent_phoenix |
| **Catégorie** | Standard |
| **Hauteur totale** | ~1.85m |
| **Largeur épaules** | ~0.48m |

#### Hitbox Specifications

| Zone | Type | Position (local) | Dimensions |
|------|------|------------------|------------|
| Head | Sphere | (0, 1.68, 0) | Radius: 0.12m |
| Body | Capsule | (0, 0.95, 0) | Height: 1.15m, Radius: 0.24m |

---

## 3. Système de Hitbox

### 3.1 Zones de Hit

| Zone | Layer | Damage Multiplier | Color Feedback |
|------|-------|-------------------|----------------|
| Head | Target_Head | x2.0 (bonus) | Yellow |
| Body | Target_Body | x1.0 | Orange |

### 3.2 Configuration des Colliders

#### Head Collider

| Propriété | Valeur | Notes |
|-----------|--------|-------|
| Type | Sphere Collider | Forme simple, performant |
| Is Trigger | Yes | Pour raycast detection |
| Layer | Target_Head (7) | Séparé du body |
| Center | Ajusté par agent | Au niveau des yeux |
| Radius | 0.10 - 0.12m | Selon agent |

#### Body Collider

| Propriété | Valeur | Notes |
|-----------|--------|-------|
| Type | Capsule Collider | Suit la forme du corps |
| Is Trigger | Yes | Pour raycast detection |
| Layer | Target_Body (6) | Distinct de head |
| Center | Ajusté par agent | Centre du torse |
| Height | 1.0 - 1.2m | Selon agent |
| Radius | 0.18 - 0.25m | Selon agent |
| Direction | Y-Axis | Vertical |

### 3.3 Hiérarchie des Colliders

```
Target (Root)
│
├── Colliders
│   ├── HeadCollider (Sphere)
│   │   ├── Layer: Target_Head
│   │   └── Component: HitZone (IsHead = true)
│   │
│   └── BodyCollider (Capsule)
│       ├── Layer: Target_Body
│       └── Component: HitZone (IsHead = false)
│
└── Model (Visual only, no collider)
```

### 3.4 Priorité de Détection

Le raycast vérifie les deux layers, mais la **tête a priorité** si les deux sont touchés au même point:

```
Raycast Hit
    │
    ├── Check Target_Head first
    │   └── If hit → HEADSHOT
    │
    └── Else check Target_Body
        └── If hit → BODYSHOT
```

---

## 4. Agent Data (ScriptableObject)

### 4.1 Structure

| Champ | Type | Description |
|-------|------|-------------|
| AgentName | string | Nom affiché (ex: "ISO") |
| AgentId | string | ID unique (ex: "agent_iso") |
| AgentIcon | Sprite | Icône pour UI |
| ModelPrefab | GameObject | Prefab du modèle 3D |
| HitboxCategory | Enum | Slim, Standard, Large |
| HeadColliderCenter | Vector3 | Position locale head |
| HeadColliderRadius | float | Rayon head |
| BodyColliderCenter | Vector3 | Position locale body |
| BodyColliderHeight | float | Hauteur capsule body |
| BodyColliderRadius | float | Rayon capsule body |
| Description | string | Description pour UI |

### 4.2 Valeurs par Défaut

| Catégorie | Head Radius | Body Height | Body Radius |
|-----------|-------------|-------------|-------------|
| Slim | 0.10m | 1.0m | 0.18m |
| Standard | 0.11m | 1.1m | 0.22m |
| Large | 0.12m | 1.2m | 0.26m |

---

## 5. Target Lifecycle

### 5.1 États d'une Cible

```
┌──────────┐
│  POOLED  │ ←── Initial state (inactive in pool)
└────┬─────┘
     │ Pool.Get()
     ▼
┌──────────┐
│ SPAWNING │ ←── Position set, activation en cours
└────┬─────┘
     │ Activate()
     ▼
┌──────────┐
│  ACTIVE  │ ←── Visible, hittable
└────┬─────┘
     │ Hit or Timeout
     ▼
┌──────────┐
│DESPAWNING│ ←── Feedback en cours
└────┬─────┘
     │ Pool.Return()
     ▼
┌──────────┐
│  POOLED  │ ←── Retour au pool
└──────────┘
```

### 5.2 Activation

**Séquence d'activation:**

1. Récupérer du pool
2. Définir position et rotation
3. Activer GameObject
4. Enregistrer spawn time
5. Marquer comme Active
6. Déclencher event OnTargetSpawned
7. Jouer son de spawn (optionnel)

### 5.3 Désactivation

**Séquence de désactivation:**

1. Marquer comme Inactive
2. Jouer feedback (si hit)
3. Déclencher event approprié (OnTargetHit ou OnTargetMissed)
4. Attendre fin feedback (optionnel)
5. Désactiver GameObject
6. Retourner au pool

---

## 6. Target Spawner

### 6.1 Responsabilités

| Responsabilité | Description |
|----------------|-------------|
| Pool Management | Gère les pools de targets par agent |
| Position Selection | Choisit position valide dans spawn zone |
| Spawn Execution | Instancie et active les targets |
| Active Tracking | Maintient liste des targets actives |
| Despawn Management | Gère le retour au pool |

### 6.2 Configuration

| Paramètre | Valeur | Description |
|-----------|--------|-------------|
| Pool Size Per Agent | 5 | Targets pré-instanciées par type |
| Min Spawn Distance | 5m | Distance minimum du joueur |
| Max Spawn Distance | 30m | Distance maximum du joueur |
| Min Target Separation | 2m | Distance min entre targets |
| Max Spawn Attempts | 10 | Tentatives avant fallback |

### 6.3 Spawn Zones

Définies par map, chaque zone a:

| Propriété | Type | Description |
|-----------|------|-------------|
| Bounds | BoxCollider | Volume de spawn |
| Weight | float (0-1) | Probabilité relative |
| Tags | string[] | Filtres optionnels |

### 6.4 Algorithme de Positionnement

```
INPUT: AgentData agent, SpawnZone preferredZone

1. SELECT zone
   - If preferredZone specified: use it
   - Else: weighted random from available zones

2. GENERATE candidate position
   - Random point within zone bounds
   - Clamp to valid height range

3. VALIDATE position
   a. Distance from player: 5m < d < 30m
   b. Distance from other active targets: > 2m
   c. Line of sight to player: clear (raycast)
   d. Ground check: valid floor below

4. IF invalid AND attempts < 10
   - Retry from step 2

5. IF all attempts failed
   - Use fallback position (zone center)

6. CALCULATE rotation
   - Face toward player position
   - Y-axis only (no tilt)

OUTPUT: (Vector3 position, Quaternion rotation)
```

### 6.5 Fallback Behavior

Si aucune position valide trouvée après 10 tentatives:

1. Utiliser le centre de la zone
2. Ajuster la hauteur au sol
3. Log warning (debug mode)
4. Continuer normalement

---

## 7. Hit Registration

### 7.1 Processus de Détection

```
Player Click
     │
     ▼
ShootingSystem.Shoot()
     │
     ├── Cast ray from camera center
     │
     ├── Check Layer: Target_Head
     │   └── Hit? → Return HEADSHOT
     │
     ├── Check Layer: Target_Body
     │   └── Hit? → Return BODYSHOT
     │
     └── Check Layer: Environment
         └── Hit? → Return MISS (with impact point)

     No hit → Return MISS (no impact)
```

### 7.2 Hit Info Structure

| Champ | Type | Description |
|-------|------|-------------|
| IsHit | bool | True si cible touchée |
| IsHeadshot | bool | True si headshot |
| HitPoint | Vector3 | Point d'impact world |
| HitNormal | Vector3 | Normale au point d'impact |
| Target | Target | Référence à la cible |
| ReactionTime | float | Temps depuis spawn |

### 7.3 Post-Hit Processing

**Après un hit confirmé:**

1. Target.RegisterHit(hitInfo)
2. Calculate reaction time
3. Trigger OnTargetHit event
4. Play hit feedback (visual + audio)
5. Update session stats
6. Schedule target despawn
7. Request next target spawn

---

## 8. Target Behaviors

### 8.1 Static Behavior (Flick, Speed, Headshot)

| Propriété | Valeur |
|-----------|--------|
| Movement | None |
| Rotation | Fixed (face player) |
| Animation | Idle (T-pose ou idle anim) |

### 8.2 Moving Behavior (Tracking)

| Propriété | Valeur |
|-----------|--------|
| Movement | Continuous |
| Pattern | Configurable |
| Speed | 1-3 m/s |
| Bounds | Spawn zone limits |
| Rotation | Always face player |

#### Movement Patterns

| Pattern | Description | Parameters |
|---------|-------------|------------|
| Horizontal | Gauche-droite | Amplitude, Frequency |
| Vertical | Haut-bas | Amplitude, Frequency |
| Circular | Cercle horizontal | Radius, Speed |
| Figure8 | Huit horizontal | Size, Speed |
| Random | Direction aléatoire smooth | Speed, Change Rate |

### 8.3 Pattern: Horizontal Sine

```
Position.x = StartPos.x + sin(Time * Frequency) * Amplitude
Position.y = StartPos.y (constant)
Position.z = StartPos.z (constant)
```

| Paramètre | Valeur Défaut |
|-----------|---------------|
| Amplitude | 3m |
| Frequency | 1.5 |

### 8.4 Pattern: Random Smooth

```
Every ChangeInterval seconds:
  1. Pick new random direction
  2. Interpolate toward new direction (smooth)
  3. Move in current direction at Speed
  4. Clamp to bounds
```

| Paramètre | Valeur Défaut |
|-----------|---------------|
| Speed | 2 m/s |
| Change Interval | 1-2s (random) |
| Smoothing | 0.1 (lerp factor) |

---

## 9. Visual Appearance

### 9.1 Model Requirements

| Aspect | Requirement |
|--------|-------------|
| Format | GLB ou FBX |
| Polycount | < 50k tris |
| Textures | Diffuse, Normal (optionnel) |
| Scale | Real-world (1 unit = 1 meter) |
| Orientation | Face +Z, Up +Y |
| Origin | Feet level |

### 9.2 Materials

| Material | Shader | Notes |
|----------|--------|-------|
| Body | URP/Lit | Standard PBR |
| Head | URP/Lit | Peut être distinct |
| Accessories | URP/Lit | Armes, équipement |

### 9.3 LOD (Future)

| LOD Level | Distance | Polycount |
|-----------|----------|-----------|
| LOD0 | 0-15m | 100% |
| LOD1 | 15-30m | 50% |
| LOD2 | 30m+ | 25% |

---

## 10. Animation (Optionnel)

### 10.1 Animations Supportées

| Animation | Usage | Priorité |
|-----------|-------|----------|
| Idle | État par défaut | P2 |
| Hit React | Feedback visuel au hit | P3 |

### 10.2 Animation States

```
┌────────────┐
│    Idle    │ ←── Default state
└─────┬──────┘
      │ OnHit
      ▼
┌────────────┐
│ Hit React  │ ←── Brief reaction
└─────┬──────┘
      │ Animation complete
      ▼
   [Despawn]
```

### 10.3 V1.0 Approach

Pour V1.0, les cibles sont en **T-pose statique** ou **idle simple**. Les animations de réaction sont prévues pour une version future.

---

## 11. Import Workflow

### 11.1 Étapes d'Import

| Étape | Action | Vérification |
|-------|--------|--------------|
| 1 | Obtenir fichier GLB/FBX | Format valide |
| 2 | Placer dans Assets/Models/Agents/{Name}/ | Path correct |
| 3 | Vérifier import settings | Scale, materials |
| 4 | Ajuster scale si nécessaire | Hauteur ~1.8m |
| 5 | Créer prefab vide | Structure correcte |
| 6 | Ajouter model comme child | Positionné au sol |
| 7 | Ajouter colliders | Head + Body |
| 8 | Configurer layers | Correct layers |
| 9 | Ajouter scripts | Target, HitZone |
| 10 | Créer AgentData asset | Toutes valeurs |
| 11 | Tester en jeu | Hit detection OK |

### 11.2 Import Settings Recommandés

| Setting | Valeur |
|---------|--------|
| Scale Factor | 1 (ajuster si nécessaire) |
| Convert Units | On |
| Import Materials | On |
| Material Location | Use External Materials |
| Animation Type | None (ou Humanoid si animé) |

### 11.3 Checklist Validation

- [ ] Model visible et correct
- [ ] Échelle réaliste (~1.8m de haut)
- [ ] Orienté face +Z
- [ ] Materials appliqués
- [ ] Head collider sur la tête
- [ ] Body collider sur le corps
- [ ] Layers corrects
- [ ] HitZone components ajoutés
- [ ] AgentData créé et configuré
- [ ] Test hit detection fonctionnel
- [ ] Test headshot vs bodyshot

---

*Voir [04_SHOOTING.md](./04_SHOOTING.md) pour le système de tir.*
