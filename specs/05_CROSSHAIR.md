# 05 — Système de Crosshair

> **Priorité**: P0 (MVP basique) / P1 (Customisation complète)
> **Statut**: Draft

---

## 1. Vue d'Ensemble

### 1.1 Concept

Réplique fidèle du système de crosshair Valorant avec toutes les options de personnalisation, permettant aux joueurs d'utiliser leur configuration habituelle.

### 1.2 Structure du Crosshair Valorant

```
                    │
          ─────     │     ─────     ← Outer Lines
                    │

              ───   │   ───         ← Inner Lines
                    │
         ─────────  ●  ─────────    ← Center Dot
                    │
              ───   │   ───
                    │

          ─────     │     ─────
                    │
```

### 1.3 Layers du Crosshair

| Layer | Ordre Z | Description |
|-------|---------|-------------|
| Outline | 0 (back) | Contour noir pour visibilité |
| Outer Lines | 1 | Lignes extérieures |
| Inner Lines | 2 | Lignes intérieures |
| Center Dot | 3 (front) | Point central |

---

## 2. Composants du Crosshair

### 2.1 Center Dot

| Propriété | Type | Range | Default |
|-----------|------|-------|---------|
| Enabled | bool | - | true |
| Size | float | 1-10 px | 2 |
| Opacity | float | 0-1 | 1.0 |

### 2.2 Inner Lines

| Propriété | Type | Range | Default |
|-----------|------|-------|---------|
| Enabled | bool | - | true |
| Length | float | 1-20 px | 6 |
| Thickness | float | 1-10 px | 2 |
| Offset | float | 0-20 px | 3 |
| Opacity | float | 0-1 | 1.0 |

**Note**: Les inner lines sont 4 éléments symétriques (haut, bas, gauche, droite).

### 2.3 Outer Lines

| Propriété | Type | Range | Default |
|-----------|------|-------|---------|
| Enabled | bool | - | false |
| Length | float | 1-20 px | 2 |
| Thickness | float | 1-10 px | 2 |
| Offset | float | 5-30 px | 10 |
| Opacity | float | 0-1 | 1.0 |

### 2.4 Outline

| Propriété | Type | Range | Default |
|-----------|------|-------|---------|
| Enabled | bool | - | true |
| Thickness | float | 0.5-3 px | 1 |
| Opacity | float | 0-1 | 0.5 |
| Color | Color | - | Black |

---

## 3. Couleurs

### 3.1 Presets de Couleur

| Preset | Hex | RGB |
|--------|-----|-----|
| White | #FFFFFF | (255, 255, 255) |
| Green | #00FF00 | (0, 255, 0) |
| Green Yellow | #ADFF2F | (173, 255, 47) |
| Cyan | #00FFFF | (0, 255, 255) |
| Pink | #FF69B4 | (255, 105, 180) |
| Yellow | #FFFF00 | (255, 255, 0) |
| Red | #FF0000 | (255, 0, 0) |
| Custom | User defined | User defined |

### 3.2 Color Settings

| Propriété | Type | Description |
|-----------|------|-------------|
| UsePreset | bool | Utiliser un preset ou custom |
| PresetColor | Enum | Sélection parmi les presets |
| CustomColor | Color | Couleur RGB personnalisée |

---

## 4. Rendering

### 4.1 Approche UI

Le crosshair est rendu via le système UI (Canvas) avec des éléments Image.

### 4.2 Hiérarchie UI

```
Canvas (Screen Space - Overlay)
└── Crosshair (Center of screen)
    ├── OutlineGroup
    │   ├── Outline_Top
    │   ├── Outline_Bottom
    │   ├── Outline_Left
    │   ├── Outline_Right
    │   └── Outline_Dot
    │
    ├── OuterLinesGroup
    │   ├── OuterLine_Top
    │   ├── OuterLine_Bottom
    │   ├── OuterLine_Left
    │   └── OuterLine_Right
    │
    ├── InnerLinesGroup
    │   ├── InnerLine_Top
    │   ├── InnerLine_Bottom
    │   ├── InnerLine_Left
    │   └── InnerLine_Right
    │
    └── CenterDot
```

### 4.3 Canvas Settings

| Setting | Value |
|---------|-------|
| Render Mode | Screen Space - Overlay |
| Sort Order | 100 (always on top) |
| Pixel Perfect | true |
| Reference Resolution | Match device |

### 4.4 Positioning

| Element | Anchor | Position |
|---------|--------|----------|
| Crosshair Root | Center | (0, 0) |
| Center Dot | Center | (0, 0) |
| Line_Top | Center | (0, +offset) |
| Line_Bottom | Center | (0, -offset) |
| Line_Left | Center | (-offset, 0) |
| Line_Right | Center | (+offset, 0) |

### 4.5 Line Dimensions

```
Pour une Inner Line horizontale (Left/Right):
- Width = InnerLineLength
- Height = InnerLineThickness
- Position.x = ±(InnerLineOffset + InnerLineLength/2)
- Position.y = 0

Pour une Inner Line verticale (Top/Bottom):
- Width = InnerLineThickness
- Height = InnerLineLength
- Position.x = 0
- Position.y = ±(InnerLineOffset + InnerLineLength/2)
```

---

## 5. Crosshair Settings Data

### 5.1 Structure ScriptableObject

| Section | Fields |
|---------|--------|
| **Color** | UsePreset, PresetColor, CustomColor |
| **Center Dot** | Enabled, Size, Opacity |
| **Inner Lines** | Enabled, Length, Thickness, Offset, Opacity |
| **Outer Lines** | Enabled, Length, Thickness, Offset, Opacity |
| **Outline** | Enabled, Thickness, Opacity |

### 5.2 Default Values (Valorant Standard)

| Setting | Default |
|---------|---------|
| Color | Cyan |
| Center Dot | On, Size 2, Opacity 1 |
| Inner Lines | On, Length 4, Thickness 2, Offset 2, Opacity 1 |
| Outer Lines | Off |
| Outline | On, Thickness 1, Opacity 0.5 |

---

## 6. Import/Export de Codes

### 6.1 Format Code Valorant

Valorant utilise un format de code pour partager les configurations crosshair. Structure:

```
0;P;c;5;h;0;m;1;0l;4;0o;2;0a;1;0f;0;1b;0
```

### 6.2 Parsing du Code

| Segment | Signification |
|---------|---------------|
| P | Profile marker |
| c;X | Color (0-8) |
| h;X | Center dot (0=off, 1=on) |
| m;X | Movement error (0=off, 1=on) |
| 0l;X | Inner line length |
| 0o;X | Inner line offset |
| 0a;X | Inner line opacity (0-1) |
| 0f;X | Firing error (0=off, 1=on) |
| 1b;X | Outer lines (0=off, 1=on) |
| ... | etc. |

### 6.3 Paramètres de Code Complet

| Key | Parameter | Values |
|-----|-----------|--------|
| c | Color | 0-8 (presets) |
| u | Custom color hex | 6 chars |
| h | Show center dot | 0, 1 |
| d | Center dot size | 1-6 |
| z | Center dot opacity | 0.0-1.0 |
| a | Crosshair opacity | 0.0-1.0 |
| m | Movement error | 0, 1 |
| s | Movement error multiplier | 0.0-3.0 |
| 0t | Inner line thickness | 1-10 |
| 0l | Inner line length | 1-20 |
| 0o | Inner line offset | 0-20 |
| 0a | Inner line opacity | 0.0-1.0 |
| 0m | Inner line movement | 0, 1 |
| 0f | Inner line firing error | 0, 1 |
| 1t | Outer line thickness | 1-10 |
| 1l | Outer line length | 1-20 |
| 1o | Outer line offset | 0-40 |
| 1a | Outer line opacity | 0.0-1.0 |
| 1m | Outer line movement | 0, 1 |
| 1f | Outer line firing error | 0, 1 |
| 1b | Show outer lines | 0, 1 |
| o | Outline | 0, 1 |
| t | Outline opacity | 0.0-1.0 |

### 6.4 Import Flow

```
User pastes code
      │
      ▼
Parse code string
      │
      ├── Split by ';'
      ├── Extract key-value pairs
      └── Validate values
      │
      ▼
Map to CrosshairSettings
      │
      ├── Color preset or custom
      ├── Center dot settings
      ├── Inner lines settings
      ├── Outer lines settings
      └── Outline settings
      │
      ▼
Apply settings
      │
      └── Update CrosshairRenderer
```

### 6.5 Export Flow

```
CrosshairSettings
      │
      ▼
Build code string
      │
      ├── Start with "0;P;"
      ├── Add color: "c;X;"
      ├── Add center dot: "h;X;d;X;"
      ├── Add inner lines: "0t;X;0l;X;0o;X;"
      ├── Add outer lines: "1b;X;1t;X;1l;X;1o;X;"
      └── Add outline: "o;X;t;X"
      │
      ▼
Return code string
```

---

## 7. Crosshair Editor UI

### 7.1 Layout

```
┌─────────────────────────────────────────────────────────────┐
│                    CROSSHAIR SETTINGS                       │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  ┌─────────────────────────────────────────────────────┐   │
│  │                                                     │   │
│  │                    PREVIEW                          │   │
│  │                       +                             │   │
│  │                                                     │   │
│  └─────────────────────────────────────────────────────┘   │
│                                                             │
│  COLOR                                                      │
│  ┌─────────────────────────────────────────────────────┐   │
│  │ [●] White  [ ] Green  [ ] Cyan  [ ] Pink  [ ] Custom│   │
│  └─────────────────────────────────────────────────────┘   │
│                                                             │
│  CENTER DOT                                                 │
│  ┌─────────────────────────────────────────────────────┐   │
│  │ [✓] Show    Size: [====●====]  Opacity: [======●==] │   │
│  └─────────────────────────────────────────────────────┘   │
│                                                             │
│  INNER LINES                                                │
│  ┌─────────────────────────────────────────────────────┐   │
│  │ [✓] Show                                            │   │
│  │ Length:    [====●====]    Thickness: [==●======]   │   │
│  │ Offset:    [==●======]    Opacity:   [======●==]   │   │
│  └─────────────────────────────────────────────────────┘   │
│                                                             │
│  OUTER LINES                                                │
│  ┌─────────────────────────────────────────────────────┐   │
│  │ [ ] Show                                            │   │
│  │ (Disabled when not shown)                          │   │
│  └─────────────────────────────────────────────────────┘   │
│                                                             │
│  OUTLINE                                                    │
│  ┌─────────────────────────────────────────────────────┐   │
│  │ [✓] Show    Thickness: [●========]  Opacity: [==●==]│   │
│  └─────────────────────────────────────────────────────┘   │
│                                                             │
│  IMPORT/EXPORT                                              │
│  ┌─────────────────────────────────────────────────────┐   │
│  │ Code: [_________________________________] [Import]  │   │
│  │                                          [Export]  │   │
│  └─────────────────────────────────────────────────────┘   │
│                                                             │
│  ┌────────────────┐              ┌────────────────────┐    │
│  │     RESET      │              │   APPLY & CLOSE    │    │
│  └────────────────┘              └────────────────────┘    │
└─────────────────────────────────────────────────────────────┘
```

### 7.2 Preview

- Zone avec fond représentant un environnement (screenshot ou couleur neutre)
- Crosshair affiché en temps réel
- Mise à jour instantanée à chaque changement

### 7.3 Sliders

| Slider | Min | Max | Step |
|--------|-----|-----|------|
| Size | 1 | 10 | 1 |
| Length | 1 | 20 | 1 |
| Thickness | 1 | 10 | 1 |
| Offset | 0 | 20 | 1 |
| Opacity | 0 | 100 | 5 (displayed as %) |

---

## 8. Dynamic Crosshair (Future)

### 8.1 Features Non-Implémentées V1.0

Les features suivantes de Valorant ne sont **pas implémentées** en V1.0:

| Feature | Description | Status |
|---------|-------------|--------|
| Movement Error | Crosshair expand quand on bouge | Future |
| Firing Error | Crosshair expand quand on tire | Future |
| Spread Indicator | Visualisation du spread | N/A (pas de spread) |

### 8.2 Crosshair Statique

En V1.0, le crosshair est **toujours statique**:
- Pas d'expansion au mouvement
- Pas d'expansion au tir
- Taille constante

---

## 9. Persistance

### 9.1 Sauvegarde

Les settings crosshair sont sauvegardés avec les autres settings dans le fichier de configuration global.

### 9.2 Structure de Sauvegarde

| Field | Type | Saved |
|-------|------|-------|
| CrosshairSettings | ScriptableObject reference | Via JSON |

### 9.3 Load/Save Flow

```
On Game Start:
1. Load settings JSON
2. Deserialize CrosshairSettings
3. Apply to CrosshairRenderer

On Settings Change:
1. Update CrosshairSettings
2. Update CrosshairRenderer (live)
3. Mark settings dirty

On Settings Apply:
1. Serialize CrosshairSettings
2. Save to JSON file
```

---

## 10. Performance

### 10.1 Optimizations

| Technique | Benefit |
|-----------|---------|
| Canvas caching | UI elements not rebuilt each frame |
| Single material | All crosshair elements share material |
| Minimal hierarchy | Flat structure, few transforms |
| Disable raycast | No raycast target on crosshair |

### 10.2 Update Frequency

| Action | Update Rate |
|--------|-------------|
| Position | Once (center of screen) |
| Settings change | On demand only |
| Color change | On demand only |
| Visibility | On demand only |

---

## 11. Accessibility

### 11.1 Color Blindness Support

Les presets de couleur incluent des options visibles pour les daltoniens:
- Cyan (visible pour la plupart)
- Yellow (haute visibilité)
- White (neutre)
- Custom (choix libre)

### 11.2 Size Options

- Range de taille suffisant (1-10px)
- Option pour outline épaisse
- Opacity réglable pour le contraste

---

*Voir [06_STATISTICS.md](./06_STATISTICS.md) pour le système de statistiques.*
