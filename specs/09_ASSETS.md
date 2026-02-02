# 09 — Gestion des Assets

> **Priorité**: P0 (MVP)
> **Statut**: Draft

---

## 1. Vue d'Ensemble

### 1.1 Types d'Assets

| Type | Source | Format | Priorité |
|------|--------|--------|----------|
| **Modèles 3D Agents** | Valorant rips | GLB/FBX | P0 |
| **Modèles 3D Maps** | Valorant rips | GLB/FBX | P0 |
| **Textures** | Avec modèles | PNG/JPG | P0 |
| **Sons** | Créés/Royalty-free | OGG/WAV | P1 |
| **UI Sprites** | Créés | PNG | P1 |
| **Fonts** | Google Fonts | TTF/OTF | P1 |
| **Particles** | Créés Unity | Prefab | P1 |

### 1.2 Contraintes Légales

| Contrainte | Description |
|------------|-------------|
| Propriété | Modèles Valorant = propriété Riot Games |
| Usage | Personnel uniquement |
| Distribution | Interdite |
| Source | Utilisateur fournit les modèles |

---

## 2. Import de Modèles 3D

### 2.1 Package glTFast

| Propriété | Valeur |
|-----------|--------|
| Package | com.atteneder.gltfast |
| Version | 6.0.x |
| Formats supportés | GLB, GLTF |

### 2.2 Workflow Import GLB

```
1. PRÉPARATION
   ├── Obtenir le fichier GLB
   ├── Vérifier la taille (< 100 MB recommandé)
   └── Renommer si nécessaire (agent_iso.glb)

2. PLACEMENT
   └── Copier dans Assets/Models/Agents/{AgentName}/
       ou Assets/Models/Maps/{MapName}/

3. IMPORT AUTOMATIQUE
   └── Unity + glTFast importent automatiquement
       ├── Mesh(es)
       ├── Materials (générés)
       └── Textures (extraites)

4. VÉRIFICATION
   ├── Prévisualiser dans Scene
   ├── Vérifier l'échelle
   ├── Vérifier les materials
   └── Vérifier l'orientation
```

### 2.3 Import Settings GLB

| Setting | Valeur Recommandée |
|---------|-------------------|
| Animation | Import si présente |
| Materials | Generate |
| Textures | Extract |

### 2.4 Workflow Import FBX

Si GLB non disponible, FBX est supporté nativement:

```
1. PLACEMENT
   └── Copier dans Assets/Models/...

2. IMPORT SETTINGS
   ├── Model Tab:
   │   ├── Scale Factor: 1 (ajuster si besoin)
   │   ├── Convert Units: On
   │   └── Import Cameras/Lights: Off
   │
   ├── Rig Tab:
   │   └── Animation Type: None (ou Humanoid si animé)
   │
   └── Materials Tab:
       ├── Material Creation: Import via MaterialDescription
       └── Location: Use External Materials

3. APPLY
   └── Cliquer Apply
```

---

## 3. Configuration des Modèles Agents

### 3.1 Structure Attendue

```
Assets/Models/Agents/ISO/
├── iso.glb (ou iso.fbx)
├── Textures/
│   ├── iso_diffuse.png
│   ├── iso_normal.png (optionnel)
│   └── iso_other.png
└── Materials/ (générés)
    └── iso_material.mat
```

### 3.2 Vérification du Modèle

| Check | Critère | Action si KO |
|-------|---------|--------------|
| Échelle | Hauteur ~1.8m | Ajuster Scale Factor |
| Orientation | Face +Z | Rotation Y dans prefab |
| Origin | Au niveau des pieds | Ajuster offset |
| Materials | Appliqués correctement | Réassigner manuellement |
| Polycount | < 50k tris | LOD ou simplification |

### 3.3 Création du Prefab Agent

```
1. CRÉER PREFAB VIDE
   └── Create > Prefab
       └── Nommer: ISO.prefab

2. STRUCTURE DU PREFAB
   ISO (Root)
   ├── Model (imported mesh)
   │   └── Position: (0, 0, 0)
   │
   ├── Colliders (Empty)
   │   ├── HeadCollider
   │   │   ├── Add: Sphere Collider
   │   │   ├── Is Trigger: true
   │   │   ├── Center: (0, 1.65, 0)
   │   │   ├── Radius: 0.11
   │   │   ├── Layer: Target_Head
   │   │   └── Add: HitZone (IsHead = true)
   │   │
   │   └── BodyCollider
   │       ├── Add: Capsule Collider
   │       ├── Is Trigger: true
   │       ├── Center: (0, 0.95, 0)
   │       ├── Radius: 0.22
   │       ├── Height: 1.1
   │       ├── Direction: Y-Axis
   │       ├── Layer: Target_Body
   │       └── Add: HitZone (IsHead = false)
   │
   └── Add to Root: Target component

3. SAUVEGARDER
   └── Assets/Prefabs/Agents/ISO.prefab
```

### 3.4 Création du ScriptableObject Agent

```
1. CRÉER ASSET
   └── Create > ValorantAimTrainer > Agent Data
       └── Nommer: ISO_Data.asset

2. CONFIGURER
   ├── Agent Name: ISO
   ├── Agent Id: agent_iso
   ├── Agent Icon: (sprite ISO)
   ├── Model Prefab: ISO.prefab
   ├── Hitbox Category: Standard
   ├── Head Collider Center: (0, 1.65, 0)
   ├── Head Collider Radius: 0.11
   ├── Body Collider Center: (0, 0.95, 0)
   ├── Body Collider Height: 1.1
   ├── Body Collider Radius: 0.22
   └── Description: "Standard hitbox agent..."

3. SAUVEGARDER
   └── Assets/ScriptableObjects/Agents/ISO_Data.asset
```

---

## 4. Configuration des Maps

### 4.1 Structure Attendue

```
Assets/Models/Maps/TheRange/
├── therange.glb
├── Textures/
│   └── (textures extraites)
└── Materials/
    └── (materials générés)
```

### 4.2 Vérification de la Map

| Check | Critère | Action si KO |
|-------|---------|--------------|
| Échelle | 1 unit = 1 meter | Ajuster Scale Factor |
| Orientation | Convention Unity | Rotation dans scene |
| Collisions | Sol/murs walkable | Ajouter MeshColliders |
| Performance | < 500k tris total | Optimisation/LOD |

### 4.3 Setup de la Scene Map

```
1. CRÉER SCENE
   └── File > New Scene
       └── Sauvegarder: Assets/Scenes/Maps/TheRange.unity

2. STRUCTURE DE LA SCENE

   TheRange (Scene)
   │
   ├── --- ENVIRONMENT ---
   │   └── Map (imported model)
   │       ├── Add: MeshCollider (pour chaque mesh sol/mur)
   │       └── Layer: Environment (tous les colliders)
   │
   ├── --- LIGHTING ---
   │   ├── Directional Light
   │   │   ├── Rotation: (50, -30, 0)
   │   │   └── Intensity: 1
   │   └── Ambient (Skybox ou Color)
   │
   ├── --- SPAWN POINTS ---
   │   ├── PlayerSpawnPoints
   │   │   └── SpawnPoint_1 (Empty)
   │   │       └── Tag: PlayerSpawn
   │   │
   │   └── TargetSpawnZones
   │       ├── SpawnZone_1
   │       │   ├── Add: BoxCollider (Is Trigger: true)
   │       │   ├── Add: SpawnZone component
   │       │   └── Tag: SpawnZone
   │       ├── SpawnZone_2
   │       └── SpawnZone_3
   │
   └── --- POST-PROCESSING --- (optionnel)
       └── Post Process Volume

3. LIGHTING SETUP
   ├── Mode: Baked ou Realtime
   └── Generate Lighting
```

### 4.4 Configuration des Spawn Zones

| Propriété | Description |
|-----------|-------------|
| Position | Centre de la zone |
| Size | Dimensions du BoxCollider |
| Weight | Probabilité de sélection (0-1) |
| Min Distance | Distance min du joueur |
| Max Distance | Distance max du joueur |

### 4.5 Création du ScriptableObject Map

```
1. CRÉER ASSET
   └── Create > ValorantAimTrainer > Map Data
       └── Nommer: TheRange_Data.asset

2. CONFIGURER
   ├── Map Name: The Range
   ├── Map Id: map_therange
   ├── Map Icon: (sprite preview)
   ├── Scene Name: TheRange
   ├── Description: "Official Valorant training map..."
   └── Player Spawn Points: (références depuis scene)

3. SAUVEGARDER
   └── Assets/ScriptableObjects/Maps/TheRange_Data.asset
```

---

## 5. Assets UI

### 5.1 Sprites

| Asset | Taille | Format | Usage |
|-------|--------|--------|-------|
| Logo | 512×512 | PNG | Main menu |
| Agent Icons | 128×128 | PNG | Selection |
| Map Previews | 400×225 | PNG | Selection |
| Mode Icons | 64×64 | PNG | Mode select |
| UI Icons | 24×24 | PNG | Buttons, misc |

### 5.2 Fonts

| Font | Usage | Source |
|------|-------|--------|
| Roboto | Body text | Google Fonts |
| Roboto Bold | Headers | Google Fonts |
| JetBrains Mono | Stats, numbers | JetBrains |

### 5.3 Organisation

```
Assets/_Project/UI/
├── Sprites/
│   ├── Logo/
│   ├── Icons/
│   │   ├── Agents/
│   │   ├── Maps/
│   │   └── Modes/
│   └── Misc/
│
├── Fonts/
│   ├── Roboto/
│   └── JetBrainsMono/
│
└── Stylesheets/ (UI Toolkit)
    ├── Common.uss
    └── Theme.uss
```

---

## 6. Assets Audio

### 6.1 Organisation

```
Assets/_Project/Audio/
├── SFX/
│   ├── gun_shot.ogg
│   ├── hit_body.ogg
│   ├── hit_head_dink.ogg
│   └── target_spawn.ogg
│
├── UI/
│   ├── button_hover.ogg
│   ├── button_click.ogg
│   └── button_back.ogg
│
└── Music/
    ├── menu_ambient.ogg
    └── training_ambient.ogg
```

### 6.2 Import Settings Audio

| Setting | Valeur |
|---------|--------|
| Load Type | Decompress On Load (SFX) |
| Load Type | Streaming (Music) |
| Compression | Vorbis |
| Quality | 70-100% |
| Sample Rate | 44100 Hz |

---

## 7. Assets Effects

### 7.1 Particles

```
Assets/_Project/Prefabs/Effects/
├── HitParticles_Body.prefab
├── HitParticles_Head.prefab
└── BulletHole.prefab
```

### 7.2 Configuration Particle System

**HitParticles_Body:**

| Module | Setting | Value |
|--------|---------|-------|
| Main | Duration | 0.3 |
| Main | Start Lifetime | 0.2-0.3 |
| Main | Start Speed | 2-4 |
| Main | Start Size | 0.03-0.05 |
| Main | Start Color | Orange |
| Emission | Bursts | 1 burst, 10-15 count |
| Shape | Shape | Hemisphere |
| Renderer | Material | Particle/Additive |

**HitParticles_Head:**

| Module | Setting | Value |
|--------|---------|-------|
| Main | Duration | 0.5 |
| Main | Start Lifetime | 0.3-0.4 |
| Main | Start Speed | 3-5 |
| Main | Start Size | 0.04-0.06 |
| Main | Start Color | Yellow |
| Emission | Bursts | 1 burst, 15-20 count |
| Shape | Shape | Hemisphere |

---

## 8. Optimisation des Assets

### 8.1 Textures

| Type | Max Size | Compression |
|------|----------|-------------|
| Character Diffuse | 2048×2048 | DXT1/BC1 |
| Character Normal | 1024×1024 | DXT5nm/BC5 |
| Environment | 2048×2048 | DXT1/BC1 |
| UI | 512×512 | None ou DXT5 |

### 8.2 Meshes

| Optimization | Description |
|--------------|-------------|
| LOD | 3 niveaux pour objets complexes |
| Mesh Compression | Medium |
| Remove Unused | Supprimer bones/blendshapes inutilisés |

### 8.3 Audio

| Optimization | Description |
|--------------|-------------|
| Mono | Pour sons non-spatialisés |
| Compression | Vorbis quality 70-100% |
| Streaming | Pour fichiers > 200KB |

---

## 9. Checklist Import Agent

### 9.1 Checklist Complète

```
□ Obtenir fichier GLB/FBX
□ Placer dans Assets/Models/Agents/{Name}/
□ Vérifier import (preview)
□ Ajuster scale si nécessaire (hauteur ~1.8m)
□ Vérifier materials appliqués
□ Créer prefab vide
□ Ajouter model comme enfant
□ Positionner model (feet at origin)
□ Ajouter GameObject Colliders
□ Ajouter HeadCollider (Sphere, Layer: Target_Head)
□ Ajouter BodyCollider (Capsule, Layer: Target_Body)
□ Configurer Is Trigger sur colliders
□ Ajouter HitZone component sur chaque collider
□ Ajouter Target component sur root
□ Sauvegarder prefab
□ Créer AgentData ScriptableObject
□ Configurer toutes les valeurs
□ Tester en jeu
□ Vérifier hit detection (body et head)
```

### 9.2 Validation Finale

| Test | Expected |
|------|----------|
| Spawn | Agent apparaît à la bonne taille |
| Body Hit | Détecté, feedback orange |
| Head Hit | Détecté, feedback jaune + dink |
| Despawn | Agent disparaît proprement |

---

## 10. Checklist Import Map

### 10.1 Checklist Complète

```
□ Obtenir fichier GLB/FBX
□ Placer dans Assets/Models/Maps/{Name}/
□ Vérifier import
□ Ajuster scale (1 unit = 1 meter)
□ Créer nouvelle scene
□ Ajouter model à la scene
□ Ajouter MeshColliders aux surfaces walkable
□ Configurer Layer: Environment
□ Ajouter lighting (Directional + Ambient)
□ Créer spawn points joueur (tag: PlayerSpawn)
□ Créer spawn zones cibles
□ Ajouter BoxCollider aux zones
□ Ajouter SpawnZone component
□ Configurer zones (poids, distances)
□ Bake lighting (optionnel)
□ Créer MapData ScriptableObject
□ Configurer toutes les valeurs
□ Tester navigation
□ Tester spawning cibles
```

---

## 11. Troubleshooting

### 11.1 Problèmes Courants

| Problème | Cause Probable | Solution |
|----------|----------------|----------|
| Model invisible | Scale trop petit | Augmenter Scale Factor |
| Model géant | Scale trop grand | Réduire Scale Factor |
| Materials roses | Textures manquantes | Vérifier extraction textures |
| Collisions traversées | Layer incorrect | Vérifier Layer settings |
| Hits non détectés | Is Trigger off | Activer Is Trigger |
| Performance basse | Trop de polys | LOD ou simplification |

### 11.2 Logs de Debug

| Log | Signification |
|-----|---------------|
| "glTFast import failed" | Fichier GLB corrompu ou incompatible |
| "Material not found" | Texture référencée manquante |
| "Collider layer mismatch" | Layer mal configuré |

---

*Voir [10_PERFORMANCE.md](./10_PERFORMANCE.md) pour l'optimisation des performances.*
