# 11 — Système de Configuration

> **Priorité**: P1 (Core Features)
> **Statut**: Draft

---

## 1. Vue d'Ensemble

### 1.1 Catégories de Settings

| Catégorie | Description | Persistance |
|-----------|-------------|-------------|
| **Gameplay** | Sensibilité, durée session | Oui |
| **Crosshair** | Configuration du viseur | Oui |
| **Audio** | Volumes par canal | Oui |
| **Video** | Qualité, résolution | Oui |

### 1.2 Stockage

| Méthode | Usage |
|---------|-------|
| ScriptableObjects | Runtime data, defaults |
| JSON File | Persistance disk |
| PlayerPrefs | Non utilisé (limité) |

### 1.3 Emplacement Fichier

```
Windows: %APPDATA%/ValorantAimTrainer/settings.json
```

---

## 2. Settings Gameplay

### 2.1 Liste des Settings

| Setting | Type | Range | Default | Description |
|---------|------|-------|---------|-------------|
| MouseSensitivity | float | 0.1 - 10.0 | 1.0 | Multiplicateur sensibilité |
| InvertY | bool | - | false | Inverser axe Y |
| SessionDuration | int | 30, 60, 90, 120 | 60 | Durée en secondes |
| ShowTutorial | bool | - | true | Afficher tutoriel |
| FOV | float | 90 - 120 | 103 | Field of View (Valorant default) |

### 2.2 Sensibilité

**Calcul:**
```
Final Sensitivity = BaseSensitivity × MouseSensitivity × DPI_Factor

Où:
- BaseSensitivity = 0.1 (valeur de base)
- MouseSensitivity = User setting (0.1 - 10.0)
- DPI_Factor = 1.0 (assume 800 DPI)
```

**Conversion Valorant:**
Pour matcher la sensibilité Valorant:
```
Unity Sens ≈ Valorant Sens × 0.1
```

### 2.3 Session Duration

| Option | Durée | Label UI |
|--------|-------|----------|
| Short | 30s | 30 seconds |
| Standard | 60s | 60 seconds (Recommended) |
| Extended | 90s | 90 seconds |
| Long | 120s | 120 seconds |

---

## 3. Settings Crosshair

### 3.1 Liste des Settings

| Setting | Type | Range | Default |
|---------|------|-------|---------|
| **Color** | | | |
| UsePreset | bool | - | true |
| PresetColor | enum | 8 presets | Cyan |
| CustomColor | Color | RGB | White |
| **Center Dot** | | | |
| ShowCenterDot | bool | - | true |
| CenterDotSize | float | 1-10 | 2 |
| CenterDotOpacity | float | 0-1 | 1.0 |
| **Inner Lines** | | | |
| ShowInnerLines | bool | - | true |
| InnerLineLength | float | 1-20 | 4 |
| InnerLineThickness | float | 1-10 | 2 |
| InnerLineOffset | float | 0-20 | 2 |
| InnerLineOpacity | float | 0-1 | 1.0 |
| **Outer Lines** | | | |
| ShowOuterLines | bool | - | false |
| OuterLineLength | float | 1-20 | 2 |
| OuterLineThickness | float | 1-10 | 2 |
| OuterLineOffset | float | 5-30 | 10 |
| OuterLineOpacity | float | 0-1 | 1.0 |
| **Outline** | | | |
| ShowOutline | bool | - | true |
| OutlineThickness | float | 0.5-3 | 1 |
| OutlineOpacity | float | 0-1 | 0.5 |

### 3.2 Presets Couleur

| Index | Nom | Hex |
|-------|-----|-----|
| 0 | White | #FFFFFF |
| 1 | Green | #00FF00 |
| 2 | Green Yellow | #ADFF2F |
| 3 | Cyan | #00FFFF |
| 4 | Pink | #FF69B4 |
| 5 | Yellow | #FFFF00 |
| 6 | Red | #FF0000 |
| 7 | Custom | User defined |

---

## 4. Settings Audio

### 4.1 Liste des Settings

| Setting | Type | Range | Default |
|---------|------|-------|---------|
| MasterVolume | float | 0-1 | 1.0 |
| SFXVolume | float | 0-1 | 1.0 |
| UIVolume | float | 0-1 | 0.8 |
| MusicVolume | float | 0-1 | 0.5 |

### 4.2 Application des Volumes

```
Final Volume = Master × Category × Individual

Exemples:
- Gun shot: Master(0.8) × SFX(1.0) × Individual(1.0) = 0.8
- Button click: Master(0.8) × UI(0.8) × Individual(1.0) = 0.64
- Menu music: Master(0.8) × Music(0.5) × Individual(1.0) = 0.4
```

---

## 5. Settings Video (Future)

### 5.1 Liste des Settings

| Setting | Type | Options | Default |
|---------|------|---------|---------|
| Resolution | enum | Auto, 1080p, 1440p, 4K | Auto |
| WindowMode | enum | Fullscreen, Windowed, Borderless | Fullscreen |
| QualityPreset | enum | Low, Medium, High | High |
| VSync | bool | - | false |
| FrameRateCap | int | 0, 60, 144, 240 | 0 (unlimited) |
| AntiAliasing | enum | Off, MSAA 2x, MSAA 4x | MSAA 4x |

### 5.2 Application Quality Preset

| Setting | Low | Medium | High |
|---------|-----|--------|------|
| Texture Quality | Half | Full | Full |
| Shadow Quality | Off | Medium | High |
| Shadow Distance | 0 | 35 | 50 |
| Anti-Aliasing | Off | 2x | 4x |

---

## 6. Architecture de Persistance

### 6.1 Structure JSON

```json
{
  "version": "1.0",
  "lastModified": "2026-02-02T10:30:00Z",
  "gameplay": {
    "mouseSensitivity": 1.0,
    "invertY": false,
    "sessionDuration": 60,
    "showTutorial": true,
    "fov": 103
  },
  "crosshair": {
    "usePreset": true,
    "presetColor": 3,
    "customColor": { "r": 255, "g": 255, "b": 255 },
    "showCenterDot": true,
    "centerDotSize": 2,
    "centerDotOpacity": 1.0,
    "showInnerLines": true,
    "innerLineLength": 4,
    "innerLineThickness": 2,
    "innerLineOffset": 2,
    "innerLineOpacity": 1.0,
    "showOuterLines": false,
    "outerLineLength": 2,
    "outerLineThickness": 2,
    "outerLineOffset": 10,
    "outerLineOpacity": 1.0,
    "showOutline": true,
    "outlineThickness": 1,
    "outlineOpacity": 0.5
  },
  "audio": {
    "masterVolume": 1.0,
    "sfxVolume": 1.0,
    "uiVolume": 0.8,
    "musicVolume": 0.5
  },
  "video": {
    "resolution": "auto",
    "windowMode": "fullscreen",
    "qualityPreset": "high",
    "vsync": false,
    "frameRateCap": 0,
    "antiAliasing": "msaa4x"
  }
}
```

### 6.2 Save System Flow

```
SAVE FLOW
─────────
User clicks "Apply"
       │
       ▼
Validate all settings
       │
       ├── Invalid? → Show error, abort
       │
       └── Valid? → Continue
              │
              ▼
       Update ScriptableObjects
              │
              ▼
       Apply to runtime systems
       (Audio, Crosshair, etc.)
              │
              ▼
       Serialize to JSON
              │
              ▼
       Write to disk
              │
              ▼
       Show confirmation


LOAD FLOW
─────────
Game Start
    │
    ▼
Check settings file exists
    │
    ├── No → Use defaults
    │
    └── Yes → Continue
           │
           ▼
    Read JSON from disk
           │
           ▼
    Validate JSON schema
           │
           ├── Invalid → Use defaults, warn
           │
           └── Valid → Continue
                  │
                  ▼
           Deserialize to objects
                  │
                  ▼
           Update ScriptableObjects
                  │
                  ▼
           Apply to runtime systems
```

### 6.3 Versioning

| Version | Changes |
|---------|---------|
| 1.0 | Initial schema |

**Migration:** Si version fichier < version app, migrer les settings.

---

## 7. ScriptableObjects

### 7.1 GameSettings

```
GameSettings (ScriptableObject)
├── Gameplay Settings
│   ├── mouseSensitivity: float
│   ├── invertY: bool
│   ├── sessionDuration: int
│   ├── showTutorial: bool
│   └── fov: float
│
└── Default values defined in asset
```

### 7.2 CrosshairSettings

```
CrosshairSettings (ScriptableObject)
├── Color Settings
├── Center Dot Settings
├── Inner Lines Settings
├── Outer Lines Settings
└── Outline Settings
```

### 7.3 AudioSettings

```
AudioSettings (ScriptableObject)
├── masterVolume: float
├── sfxVolume: float
├── uiVolume: float
└── musicVolume: float
```

### 7.4 VideoSettings

```
VideoSettings (ScriptableObject)
├── resolution: ResolutionOption
├── windowMode: WindowMode
├── qualityPreset: QualityPreset
├── vsync: bool
├── frameRateCap: int
└── antiAliasing: AAOption
```

---

## 8. Settings Manager

### 8.1 Responsabilités

| Responsabilité | Description |
|----------------|-------------|
| Load | Charger settings au démarrage |
| Save | Sauvegarder sur disque |
| Apply | Appliquer aux systèmes |
| Reset | Restaurer défauts |
| Validate | Vérifier valeurs valides |

### 8.2 Méthodes Publiques

| Méthode | Description |
|---------|-------------|
| LoadSettings() | Charge depuis fichier |
| SaveSettings() | Sauvegarde sur disque |
| ApplySettings() | Applique tous les settings |
| ResetToDefaults() | Restaure les défauts |
| GetGameplaySettings() | Retourne ref gameplay |
| GetCrosshairSettings() | Retourne ref crosshair |
| GetAudioSettings() | Retourne ref audio |
| GetVideoSettings() | Retourne ref video |

### 8.3 Events

| Event | Trigger |
|-------|---------|
| OnSettingsLoaded | Après chargement |
| OnSettingsSaved | Après sauvegarde |
| OnSettingsApplied | Après application |
| OnSettingsReset | Après reset |

---

## 9. UI Settings

### 9.1 Structure des Tabs

```
┌──────────┬──────────┬──────────┬──────────┐
│ GAMEPLAY │ CROSSHAIR│  AUDIO   │  VIDEO   │
└──────────┴──────────┴──────────┴──────────┘
```

### 9.2 Tab Gameplay

```
┌────────────────────────────────────────────────────────────┐
│  GAMEPLAY                                                   │
│  ──────────────────────────────────────────────────────    │
│                                                             │
│  Mouse Sensitivity                                          │
│  [================●=============] 1.0                       │
│  Adjust to match your in-game Valorant sensitivity         │
│                                                             │
│  Invert Y Axis                                              │
│  [     ] Off                                               │
│                                                             │
│  ──────────────────────────────────────────────────────    │
│                                                             │
│  Session Duration                                           │
│  ┌─────────────────────────────────────────────┐           │
│  │ 60 seconds (Recommended)                [▼] │           │
│  └─────────────────────────────────────────────┘           │
│                                                             │
│  ──────────────────────────────────────────────────────    │
│                                                             │
│  Field of View                                              │
│  [================●=============] 103°                      │
│  Valorant default: 103°                                    │
│                                                             │
│  ──────────────────────────────────────────────────────    │
│                                                             │
│  Show Tutorial                                              │
│  [●    ] On                                                │
│                                                             │
└────────────────────────────────────────────────────────────┘
```

### 9.3 Tab Audio

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
│  Gunshots, hits, target sounds                             │
│                                                             │
│  UI Volume                                                  │
│  [=========================●============]  80%              │
│  Button clicks, navigation                                 │
│                                                             │
│  Music Volume                                               │
│  [=============●========================]  50%              │
│  Background music                                          │
│                                                             │
└────────────────────────────────────────────────────────────┘
```

### 9.4 Actions Settings Screen

| Bouton | Action |
|--------|--------|
| Apply & Close | Save + Apply + Close |
| Reset to Defaults | Reset all to defaults |
| Back (sans apply) | Discard changes, close |

---

## 10. Validation

### 10.1 Règles de Validation

| Setting | Validation |
|---------|------------|
| MouseSensitivity | Clamp(0.1, 10.0) |
| SessionDuration | Must be in [30, 60, 90, 120] |
| Volumes | Clamp(0, 1) |
| FOV | Clamp(90, 120) |
| Colors | RGB values 0-255 |
| Sizes | Must be in defined range |

### 10.2 Handling Invalid Data

```
On Load:
├── Parse JSON
├── For each field:
│   ├── Missing? → Use default
│   ├── Invalid type? → Use default, warn
│   └── Out of range? → Clamp to range, warn
└── Continue with validated data
```

---

## 11. Reset & Defaults

### 11.1 Default Values Source

Les valeurs par défaut sont définies dans les ScriptableObjects:
- `DefaultGameSettings.asset`
- `DefaultCrosshairSettings.asset`
- `DefaultAudioSettings.asset`
- `DefaultVideoSettings.asset`

### 11.2 Reset Flow

```
User clicks "Reset to Defaults"
          │
          ▼
    Show confirmation dialog
          │
          ├── Cancel → Abort
          │
          └── Confirm → Continue
                 │
                 ▼
          Load default assets
                 │
                 ▼
          Copy to runtime settings
                 │
                 ▼
          Update UI to show defaults
                 │
                 ▼
          (NOT saved until Apply)
```

---

## 12. Checklist Implementation

### 12.1 MVP Settings

```
□ Mouse sensitivity slider
□ Session duration dropdown
□ Basic volume control (master)
□ Settings persist between sessions
□ Apply button functional
```

### 12.2 V1.0 Settings

```
□ Full gameplay settings
□ Complete crosshair customization
□ All audio volume controls
□ Video quality presets
□ Reset to defaults
□ Settings validation
□ JSON persistence
□ Version migration support
```

---

*Voir [12_ROADMAP.md](./12_ROADMAP.md) pour le planning du projet.*
