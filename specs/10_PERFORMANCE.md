# 10 — Performance & Optimisation

> **Priorité**: P0 (Targets) / P2 (Fine-tuning)
> **Statut**: Draft

---

## 1. Objectifs de Performance

### 1.1 Targets Principaux

| Métrique | Target | Priorité | Justification |
|----------|--------|----------|---------------|
| **Frame Rate** | ≥144 FPS stable | P0 | Standard compétitif |
| **Input Latency** | <10ms | P0 | Réactivité aim trainer |
| **Frame Time** | <7ms | P0 | Pour maintenir 144 FPS |
| **Load Time** | <3s | P1 | UX acceptable |
| **Memory** | <2 GB RAM | P1 | PC gaming standard |

### 1.2 Hardware Cible

| Composant | Minimum | Recommandé |
|-----------|---------|------------|
| CPU | i5-6600 / Ryzen 3 1200 | i7-8700 / Ryzen 5 3600 |
| GPU | GTX 1050 Ti / RX 570 | RTX 3060 / RX 6600 |
| RAM | 8 GB | 16 GB |
| Storage | HDD | SSD |

### 1.3 Budgets par Frame

| Catégorie | Budget | Notes |
|-----------|--------|-------|
| **Total Frame** | 6.9ms | Pour 144 FPS |
| Rendering | 4ms | GPU |
| Game Logic | 1.5ms | CPU |
| Physics | 0.5ms | Raycast, collisions |
| Audio | 0.2ms | Playback |
| UI | 0.5ms | HUD updates |
| Headroom | 0.2ms | Margin |

---

## 2. Optimisation CPU

### 2.1 Principes Généraux

| Principe | Application |
|----------|-------------|
| Éviter allocations | Pas de new dans Update |
| Cache références | GetComponent une fois dans Awake |
| Éviter Find | Pas de Find/FindObjectsOfType runtime |
| Event-driven | Réagir aux events, pas poll |

### 2.2 Patterns à Éviter

| Pattern | Problème | Alternative |
|---------|----------|-------------|
| `FindObjectsOfType` in Update | O(n) chaque frame | Cache dans liste |
| `GetComponent` in Update | Allocation, lookup | Cache dans Awake |
| String concatenation | Allocation | StringBuilder ou interpolation |
| LINQ in hot paths | Allocations cachées | Boucles for |
| `foreach` sur collections | Enumerator allocation | Boucle `for` indexée |
| Boxing value types | Allocation heap | Éviter object cast |

### 2.3 Patterns Recommandés

| Pattern | Usage |
|---------|-------|
| Object Pooling | Targets, particles, audio sources |
| Event Bus | Communication entre systèmes |
| Cached References | Tous les components fréquemment accédés |
| Preallocated Collections | Listes avec capacité initiale |

### 2.4 Object Pooling

**Objets à pooler:**

| Objet | Pool Size | Raison |
|-------|-----------|--------|
| Targets (par type) | 5 | Spawn fréquent |
| Hit Particles Body | 10 | Effet chaque hit |
| Hit Particles Head | 10 | Effet chaque headshot |
| Bullet Hole Decals | 20 | Persiste plusieurs secondes |
| Audio Sources | 5 | Sons simultanés |

**Lifecycle:**

```
INIT (Awake)
├── Instantiate pool objects
├── Deactivate all
└── Store in queue

GET
├── Dequeue from pool
├── Activate
├── Call OnSpawn()
└── Return reference

RETURN
├── Call OnDespawn()
├── Deactivate
└── Enqueue to pool
```

---

## 3. Optimisation GPU

### 3.1 Rendering Pipeline

| Setting | Valeur | Notes |
|---------|--------|-------|
| Render Pipeline | URP | Plus léger que HDRP |
| VSync | Off | Uncapped framerate |
| Anti-Aliasing | MSAA 4x | Configurable |
| Shadow Resolution | 1024 | Configurable |

### 3.2 Budgets Rendering

| Métrique | Budget |
|----------|--------|
| Draw Calls | < 200 |
| SetPass Calls | < 100 |
| Triangles | < 500K |
| Batches | < 150 |

### 3.3 Techniques d'Optimisation

| Technique | Application | Impact |
|-----------|-------------|--------|
| **Static Batching** | Geometry statique des maps | High |
| **GPU Instancing** | Multiple targets identiques | Medium |
| **LOD** | Objets distants | Medium |
| **Occlusion Culling** | Maps avec murs | Medium |
| **Texture Compression** | Toutes textures | Medium |
| **Baked Lighting** | Maps statiques | High |

### 3.4 LOD Configuration

| LOD Level | Distance | Polycount |
|-----------|----------|-----------|
| LOD0 | 0-15m | 100% |
| LOD1 | 15-30m | 50% |
| LOD2 | 30m+ | 25% |
| Culled | >50m | 0% |

### 3.5 URP Settings

```
URP Asset Configuration:
├── Quality
│   ├── Anti Aliasing: MSAA 4x
│   ├── Render Scale: 1.0
│   └── Upscaling Filter: Auto
│
├── Lighting
│   ├── Main Light: Per Pixel
│   ├── Additional Lights: Per Pixel
│   ├── Max Additional Lights: 4
│   └── Cast Shadows: On
│
├── Shadows
│   ├── Max Distance: 50
│   ├── Cascade Count: 2
│   └── Resolution: 1024
│
└── Post Processing
    └── Enabled: Off (ou minimal)
```

---

## 4. Optimisation Mémoire

### 4.1 Budget Mémoire

| Catégorie | Budget |
|-----------|--------|
| **Total** | < 2 GB |
| Textures | < 800 MB |
| Meshes | < 400 MB |
| Audio | < 100 MB |
| Scripts/Runtime | < 200 MB |
| Unity Overhead | ~500 MB |

### 4.2 Texture Optimization

| Type | Max Size | Format | Compression |
|------|----------|--------|-------------|
| Agent Diffuse | 2048² | RGB | DXT1 |
| Agent Normal | 1024² | RGB | BC5 |
| Map Diffuse | 2048² | RGB | DXT1 |
| UI | 512² | RGBA | None/DXT5 |

### 4.3 Mesh Optimization

| Optimization | Impact |
|--------------|--------|
| Mesh Compression | Reduces storage, slight CPU cost |
| Remove unused data | Tangents, colors si non utilisés |
| Combine meshes | Moins de draw calls |

### 4.4 Garbage Collection

**Minimiser GC:**
- Pas d'allocation dans Update/FixedUpdate
- Préallouer collections avec capacité
- Réutiliser objets (pooling)
- Éviter boxing/unboxing

**Monitoring:**
- Profiler > Memory > GC Alloc
- Target: 0 bytes/frame pendant gameplay

---

## 5. Optimisation Input

### 5.1 Input Latency Sources

| Source | Latency | Mitigation |
|--------|---------|------------|
| USB Polling | 1-8ms | High polling rate mouse |
| OS Processing | 1-2ms | - |
| Unity Input | 1-2ms | Raw Mouse Input |
| Game Processing | 1-2ms | Immediate processing |
| **Total** | 4-14ms | Target: <10ms |

### 5.2 Configuration Input

| Setting | Valeur | Raison |
|---------|--------|--------|
| Raw Mouse Input | Enabled | Bypass OS processing |
| Input System | New Input System | Better performance |
| Update Mode | Process Events In Dynamic Update | Immediate |

### 5.3 Best Practices Input

| Practice | Description |
|----------|-------------|
| Process immédiat | Shoot dès l'input, pas dans Update |
| Pas de smoothing | Aucun lissage souris |
| Delta time correct | Utiliser frame-independent |

---

## 6. Optimisation Physics

### 6.1 Configuration Physics

| Setting | Valeur | Notes |
|---------|--------|-------|
| Fixed Timestep | 0.02 (50Hz) | Suffisant pour raycast |
| Gravity | (0, -9.81, 0) | Standard |
| Default Contact Offset | 0.01 | Default |

### 6.2 Layer Collision Matrix

Désactiver collisions inutiles:

```
Target_Body ↔ Target_Body: OFF
Target_Head ↔ Target_Head: OFF
Target_Body ↔ Target_Head: OFF
```

### 6.3 Raycast Optimization

| Technique | Impact |
|-----------|--------|
| Layer Mask | Limiter aux layers nécessaires |
| Single raycast | Pas RaycastAll sauf nécessaire |
| Cached masks | Pas recalculer LayerMask |

**Raycast Budget:**
- Target: < 0.1ms par raycast
- Fréquence: Une fois par tir (pas par frame)

---

## 7. Optimisation Audio

### 7.1 Configuration Audio

| Setting | Valeur |
|---------|--------|
| DSP Buffer Size | Best performance |
| Sample Rate | 44100 Hz |
| Max Voices | 32 |

### 7.2 Audio Optimization

| Technique | Description |
|-----------|-------------|
| Pooling AudioSources | Réutiliser les sources |
| Compressed Audio | OGG Vorbis pour tous |
| Streaming | Pour fichiers > 200KB |
| Mono | Pour sons 2D |

---

## 8. Profiling

### 8.1 Outils Unity

| Outil | Usage |
|-------|-------|
| **Profiler** | CPU, GPU, Memory, Audio |
| **Frame Debugger** | Analyse draw calls |
| **Memory Profiler** | Analyse mémoire détaillée |
| **Physics Debugger** | Visualisation colliders |

### 8.2 Métriques à Surveiller

| Métrique | Tool | Target |
|----------|------|--------|
| Frame Time | Profiler | < 7ms |
| GC Alloc | Profiler > Memory | 0 B/frame |
| Draw Calls | Frame Debugger | < 200 |
| SetPass Calls | Frame Debugger | < 100 |
| Physics Time | Profiler > Physics | < 0.5ms |

### 8.3 Profiling Checklist

```
□ Activer Deep Profile pour analyse détaillée
□ Profiler sur target hardware (pas éditeur)
□ Tester avec contenu réaliste (pas scene vide)
□ Mesurer sur plusieurs minutes
□ Identifier les spikes
□ Vérifier GC allocations
□ Analyser draw calls
□ Mesurer input latency
```

---

## 9. Quality Settings

### 9.1 Presets Quality

| Setting | Low | Medium | High |
|---------|-----|--------|------|
| Texture Quality | Half | Full | Full |
| Shadow Quality | Hard Only | Medium | High |
| Shadow Distance | 25 | 50 | 75 |
| Anti-Aliasing | Off | MSAA 2x | MSAA 4x |
| Anisotropic | Off | Per Texture | Forced On |

### 9.2 Settings Configurables (UI)

| Setting | Options | Default |
|---------|---------|---------|
| Quality Preset | Low, Medium, High | High |
| VSync | Off, On | Off |
| Frame Rate Cap | Unlimited, 60, 144, 240 | Unlimited |
| Resolution | Auto-detect | Native |

---

## 10. Build Optimization

### 10.1 Build Settings

| Setting | Valeur |
|---------|--------|
| Scripting Backend | IL2CPP |
| API Compatibility | .NET Standard 2.1 |
| Managed Stripping Level | Medium |
| C++ Compiler | Release |

### 10.2 Asset Bundles (Future)

Pour réduire build size et permettre updates:
- Maps comme AssetBundles séparés
- Agents comme AssetBundles séparés
- Core game dans build principal

### 10.3 Build Checklist

```
□ Remove development build flag
□ Enable IL2CPP
□ Strip unused code
□ Compress textures
□ Remove debug symbols
□ Test on target hardware
□ Verify 144 FPS stable
□ Measure load times
```

---

## 11. Monitoring Runtime

### 11.1 Performance Overlay (Debug)

Afficher en debug mode:
- FPS actuel
- Frame time (ms)
- Memory usage
- Draw calls

### 11.2 Performance Logging

Logger si:
- FPS < 120 sustained
- Frame spike > 20ms
- GC collection > 1ms
- Memory > threshold

### 11.3 Automatic Quality Adjustment (Future)

Si FPS trop bas:
1. Réduire shadow distance
2. Réduire anti-aliasing
3. Réduire texture quality
4. Notifier utilisateur

---

## 12. Checklist Performance

### 12.1 MVP Performance Checklist

```
□ 144 FPS sur hardware recommandé
□ Input latency < 10ms (subjectif)
□ Pas de freeze/stutter
□ Load time < 5s
□ No memory leaks (10min test)
```

### 12.2 V1.0 Performance Checklist

```
□ 144 FPS stable (< 1% drops)
□ Load time < 3s
□ Memory < 2GB
□ Draw calls < 200
□ Zero GC during gameplay
□ Quality settings fonctionnels
□ Profiling clean sur toutes maps
```

---

*Voir [11_SETTINGS.md](./11_SETTINGS.md) pour le système de configuration.*
