# 08 — Système Audio

> **Priorité**: P1 (Core Features)
> **Statut**: Draft

---

## 1. Vue d'Ensemble

### 1.1 Objectif

Créer un feedback audio immersif et fonctionnel qui améliore l'expérience d'entraînement sans être distrayant.

### 1.2 Catégories Audio

| Catégorie | Description | Priorité |
|-----------|-------------|----------|
| **SFX** | Sons de gameplay (tir, hits) | Highest |
| **UI** | Sons d'interface | Medium |
| **Music** | Musique d'ambiance | Low |

### 1.3 Format Audio

| Format | Usage | Raison |
|--------|-------|--------|
| OGG Vorbis | Tous les sons | Bonne compression, qualité |
| WAV | Sources (développement) | Qualité maximale |

---

## 2. Canaux Audio

### 2.1 Configuration des Canaux

| Canal | Parent | Volume Défaut | Description |
|-------|--------|---------------|-------------|
| Master | - | 100% | Volume global |
| SFX | Master | 100% | Effets sonores gameplay |
| UI | Master | 80% | Sons interface |
| Music | Master | 50% | Musique d'ambiance |

### 2.2 Hiérarchie Audio Mixer

```
Master
├── SFX
│   ├── Weapons
│   ├── Hits
│   └── Environment
├── UI
│   ├── Buttons
│   └── Notifications
└── Music
    └── Ambient
```

### 2.3 Calcul du Volume Final

```
Final Volume = Master × Category × Individual

Exemple:
- Master: 80%
- SFX: 100%
- Individual (gunshot): 100%
- Final: 0.8 × 1.0 × 1.0 = 80%
```

---

## 3. Catalogue des Sons

### 3.1 Sons de Gameplay (SFX)

| ID | Nom | Description | Durée | Priorité |
|----|-----|-------------|-------|----------|
| SFX_001 | gun_shot | Tir d'arme (style Vandal) | ~0.3s | P0 |
| SFX_002 | hit_body | Impact sur corps | ~0.2s | P0 |
| SFX_003 | hit_head_dink | Headshot "dink" métallique | ~0.4s | P0 |
| SFX_004 | target_spawn | Apparition de cible | ~0.2s | P1 |
| SFX_005 | target_despawn | Disparition de cible | ~0.15s | P2 |

### 3.2 Sons d'Interface (UI)

| ID | Nom | Description | Durée | Priorité |
|----|-----|-------------|-------|----------|
| UI_001 | button_hover | Survol bouton | ~0.05s | P1 |
| UI_002 | button_click | Clic bouton | ~0.1s | P1 |
| UI_003 | button_back | Navigation retour | ~0.1s | P1 |
| UI_004 | tab_switch | Changement d'onglet | ~0.08s | P2 |
| UI_005 | slider_tick | Déplacement slider | ~0.02s | P2 |
| UI_006 | toggle_on | Activation toggle | ~0.1s | P2 |
| UI_007 | toggle_off | Désactivation toggle | ~0.1s | P2 |

### 3.3 Sons de Session

| ID | Nom | Description | Durée | Priorité |
|----|-----|-------------|-------|----------|
| SES_001 | countdown_tick | Tick du countdown (3, 2, 1) | ~0.15s | P0 |
| SES_002 | countdown_go | "GO!" de démarrage | ~0.4s | P0 |
| SES_003 | session_end | Fin de session | ~1.0s | P0 |
| SES_004 | timer_warning | Warning à 10s restantes | ~0.3s | P1 |

### 3.4 Musique (Music)

| ID | Nom | Description | Durée | Priorité |
|----|-----|-------------|-------|----------|
| MUS_001 | menu_ambient | Ambiance menu principal | Loop | P2 |
| MUS_002 | training_ambient | Ambiance pendant session | Loop | P3 |

---

## 4. Spécifications des Sons Clés

### 4.1 Gun Shot (SFX_001)

| Propriété | Valeur |
|-----------|--------|
| Style | Vandal/Phantom-like |
| Caractère | Punchy, satisfaisant |
| Fréquences | Médium-grave (100-2000 Hz) |
| Attack | Rapide (<10ms) |
| Decay | Moyen (~200ms) |
| Stéréo | Centré |

**Notes:**
- Doit être satisfaisant mais pas fatigant en répétition
- Pas de variation aléatoire (consistance)

### 4.2 Headshot Dink (SFX_003)

| Propriété | Valeur |
|-----------|--------|
| Style | Métallique, résonant |
| Caractère | Distinctif, rewarding |
| Fréquences | Aigu (2000-8000 Hz) |
| Attack | Très rapide (<5ms) |
| Decay | Long avec résonance (~400ms) |
| Stéréo | Centré |

**Notes:**
- Son signature de Valorant
- Doit être immédiatement reconnaissable
- Récompense auditive pour la précision

### 4.3 Body Hit (SFX_002)

| Propriété | Valeur |
|-----------|--------|
| Style | Impact sourd |
| Caractère | Satisfaisant mais moins que dink |
| Fréquences | Grave-médium (80-1500 Hz) |
| Attack | Rapide (<15ms) |
| Decay | Court (~150ms) |

**Notes:**
- Clairement différent du dink
- Confirme le hit sans être aussi rewarding

### 4.4 Countdown (SES_001, SES_002)

**Tick (3, 2, 1):**
| Propriété | Valeur |
|-----------|--------|
| Style | Beep électronique |
| Pitch | Montant (3 → 2 → 1) |

**GO!:**
| Propriété | Valeur |
|-----------|--------|
| Style | Accent musical/electronic |
| Caractère | Énergique, motivant |

---

## 5. Audio Manager

### 5.1 Responsabilités

| Responsabilité | Description |
|----------------|-------------|
| Playback | Lecture des sons |
| Volume Control | Application des volumes par canal |
| Pooling | Réutilisation des AudioSources |
| Settings | Application des préférences |

### 5.2 Méthodes Publiques

| Méthode | Description |
|---------|-------------|
| PlayGunShot() | Joue le son de tir |
| PlayBodyHit() | Joue le son de hit body |
| PlayHeadshotDink() | Joue le son de headshot |
| PlayTargetSpawn() | Joue le son de spawn |
| PlayUIClick() | Joue le son de clic UI |
| PlayUIHover() | Joue le son de hover UI |
| PlayCountdownTick() | Joue le tick countdown |
| PlayCountdownGo() | Joue le GO |
| PlaySessionEnd() | Joue le son de fin |
| SetMasterVolume(float) | Définit le volume master |
| SetSFXVolume(float) | Définit le volume SFX |
| SetUIVolume(float) | Définit le volume UI |
| SetMusicVolume(float) | Définit le volume musique |

### 5.3 Audio Source Pool

| Configuration | Valeur |
|---------------|--------|
| Pool Size | 5 sources |
| Reuse Strategy | Oldest playing |
| Preload | On Awake |

---

## 6. Règles de Lecture

### 6.1 Priorités de Lecture

| Priorité | Sons | Comportement |
|----------|------|--------------|
| 1 (Highest) | Dink, Gun Shot | Jamais coupé |
| 2 | Body Hit, Countdown | Peut couper P3 |
| 3 | UI, Spawn | Peut être coupé |
| 4 (Lowest) | Ambient | Toujours en arrière-plan |

### 6.2 Règles de Superposition

| Règle | Description |
|-------|-------------|
| Gun Shot | Un seul à la fois (pas de semi-auto rapide) |
| Hit Sounds | Peuvent se superposer (rare) |
| UI Sounds | Un seul à la fois |
| Music | Toujours en arrière-plan |

### 6.3 Timing

| Event | Timing |
|-------|--------|
| Shoot | Immédiat sur input |
| Hit | Immédiat sur hit detection |
| UI | Immédiat sur interaction |
| Countdown | Précis à la seconde |

---

## 7. Spatial Audio

### 7.1 Configuration V1.0

En V1.0, tous les sons sont en **2D** (non-spatialisés):

| Son | Spatial Blend |
|-----|---------------|
| Tous | 0 (2D) |

### 7.2 Future: 3D Audio

Pour une version future:
- Hit sounds pourraient être 3D (position de la cible)
- Target spawn en 3D léger

---

## 8. Settings Audio

### 8.1 Options Disponibles

| Setting | Type | Range | Default |
|---------|------|-------|---------|
| Master Volume | Slider | 0-100% | 100% |
| SFX Volume | Slider | 0-100% | 100% |
| UI Volume | Slider | 0-100% | 80% |
| Music Volume | Slider | 0-100% | 50% |

### 8.2 UI Settings Audio

```
┌────────────────────────────────────────────────────────────┐
│  AUDIO                                                      │
│  ──────────────────────────────────────────────────────    │
│                                                             │
│  Master Volume                                              │
│  [================================●=====] 100%              │
│                                                             │
│  SFX Volume                                                 │
│  [================================●=====] 100%              │
│                                                             │
│  UI Volume                                                  │
│  [=========================●============]  80%              │
│                                                             │
│  Music Volume                                               │
│  [=============●========================]  50%              │
│                                                             │
└────────────────────────────────────────────────────────────┘
```

### 8.3 Persistance

Les settings audio sont sauvegardés avec les autres préférences et chargés au démarrage.

---

## 9. Événements Audio

### 9.1 Mapping Events → Sons

| Game Event | Son(s) Joué(s) |
|------------|----------------|
| OnShoot | SFX_001 (gun_shot) |
| OnHit (body) | SFX_002 (hit_body) |
| OnHit (head) | SFX_003 (hit_head_dink) |
| OnTargetSpawned | SFX_004 (target_spawn) |
| OnCountdownTick | SES_001 (countdown_tick) |
| OnCountdownGo | SES_002 (countdown_go) |
| OnSessionEnd | SES_003 (session_end) |
| OnButtonHover | UI_001 (button_hover) |
| OnButtonClick | UI_002 (button_click) |
| OnBackNavigation | UI_003 (button_back) |

### 9.2 Séquence Countdown

```
T-3: Play countdown_tick (pitch: low)
T-2: Play countdown_tick (pitch: medium)
T-1: Play countdown_tick (pitch: high)
T-0: Play countdown_go
     Start session
```

---

## 10. Performance

### 10.1 Optimisations

| Technique | Benefit |
|-----------|---------|
| Audio Pooling | Évite instantiation runtime |
| Preload Clips | Pas de chargement pendant jeu |
| Compressed Format | Moins de mémoire |

### 10.2 Memory Budget

| Type | Budget |
|------|--------|
| SFX Total | < 5 MB |
| UI Total | < 1 MB |
| Music Total | < 10 MB |
| **Total** | < 16 MB |

### 10.3 Latency

| Requirement | Target |
|-------------|--------|
| Input to Sound | < 10ms |
| Consistent Timing | ±1ms |

---

## 11. Sources des Sons

### 11.1 Options de Sourcing

| Option | Pros | Cons |
|--------|------|------|
| Créer (Synthesis) | Contrôle total, unique | Temps, expertise |
| Royalty-Free | Rapide, légal | Moins unique |
| Record | Authentique | Équipement, temps |

### 11.2 Resources Royalty-Free

| Site | Type | Notes |
|------|------|-------|
| Freesound.org | Community | Attribution souvent requise |
| Sonniss GDC | Pro packs | Haute qualité |
| ZapSplat | Mixed | Gratuit avec crédit |

### 11.3 À Créer/Obtenir

| Son | Méthode Suggérée |
|-----|------------------|
| Gun Shot | Synthesis ou royalty-free |
| Dink | Synthesis (critique pour authenticité) |
| Body Hit | Royalty-free, post-process |
| UI Sounds | Synthesis simple |
| Countdown | Synthesis |
| Music | Royalty-free ambient |

---

## 12. Checklist Implémentation

### 12.1 MVP

- [ ] AudioManager singleton
- [ ] Gun shot sound
- [ ] Body hit sound
- [ ] Headshot dink sound
- [ ] Basic volume control (master)

### 12.2 Core

- [ ] Full volume controls (Master, SFX, UI, Music)
- [ ] All UI sounds
- [ ] Countdown sounds
- [ ] Session end sound
- [ ] Audio pooling
- [ ] Settings persistence

### 12.3 Polish

- [ ] Target spawn sound
- [ ] Menu ambient music
- [ ] Training ambient music (optional)
- [ ] Timer warning sound
- [ ] Fine-tuning volumes

---

*Voir [09_ASSETS.md](./09_ASSETS.md) pour la gestion des assets.*
