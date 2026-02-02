# VALORANT AIM TRAINER — Vision Produit

## 1. Vue d'Ensemble

### Pitch
Un aim trainer personnel sous Unity utilisant des modèles 3D authentiques de Valorant (agents, maps) pour un entraînement immersif et réaliste.

### Objectif
Permettre aux joueurs de Valorant de s'entraîner sur des cibles fidèles au jeu (hitboxes, proportions, environnements) pour une progression directement transférable en ranked.

### Public Cible
- Joueur Valorant souhaitant améliorer son aim
- Usage personnel uniquement
- Tous niveaux (Iron à Radiant)

### Stack Technique
- **Moteur**: Unity 2022.3 LTS
- **Modèles**: GLB/FBX (Valorant rips)
- **Package requis**: glTFast 6.0

---

## 2. Modèles 3D

### 2.1 Agents (Cibles)

Les cibles sont des modèles d'agents Valorant pour un entraînement réaliste sur les vraies hitboxes.

| Agent | Priorité | Status | Notes |
|-------|----------|--------|-------|
| ISO | P0 | À importer | Premier agent, servira de template |
| Jett | P1 | Futur | Hitbox slim, bon pour precision |
| Phoenix | P1 | Futur | Hitbox standard |
| Breach | P2 | Futur | Hitbox large |
| Cypher | P2 | Futur | - |
| Autres | P3 | Futur | Selon disponibilité |

#### Workflow Import Agent
1. Obtenir le modèle GLB/FBX de l'agent
2. Importer dans Unity via glTFast
3. Configurer les colliders (body + head)
4. Créer le prefab avec script Target
5. Ajouter à la liste du TargetSpawner

#### Hitbox par Agent
Chaque agent a des proportions légèrement différentes. Les colliders doivent être ajustés individuellement pour matcher les vraies hitboxes Valorant.

### 2.2 Maps (Environnements)

L'environnement de training utilise des maps Valorant pour une immersion totale.

| Map | Priorité | Status | Notes |
|-----|----------|--------|-------|
| The Range | P0 | À importer | Map d'entraînement officielle |
| Ascent | P1 | Futur | Map compétitive populaire |
| Bind | P2 | Futur | - |
| Haven | P2 | Futur | - |
| Autres | P3 | Futur | Selon demande |

#### Workflow Import Map
1. Obtenir le modèle de map
2. Importer dans Unity
3. Configurer les colliders sol/murs (Layer: Environment)
4. Définir les spawn points joueur
5. Définir les zones de spawn cibles
6. Optimiser (LOD, occlusion culling si nécessaire)

---

## 3. Fonctionnalités Core

### 3.1 Modes d'Entraînement

| Mode | Description | Difficulté |
|------|-------------|------------|
| **Flick** | Cibles statiques, flick shots | ★★☆☆☆ |
| **Tracking** | Cibles mobiles, suivi continu | ★★★☆☆ |
| **Speed** | Cibles rapides, réactivité | ★★★★☆ |
| **Headshot** | Focus précision tête | ★★★★☆ |

### 3.2 Système de Tir

- Raycast depuis centre écran
- Détection hit/miss
- Différenciation body shot / headshot
- Temps de réaction calculé par cible

### 3.3 Crosshair Valorant

Réplique exacte du système Valorant:
- 4 couches: Outline, Outer Lines, Inner Lines, Center Dot
- Tous paramètres configurables
- Import/export de codes crosshair Valorant
- Preview temps réel

### 3.4 Hit Feedback

Style Valorant authentique:
- Particules au point d'impact (pas de hitmarker sur crosshair)
- Couleur différente body (orange) vs head (jaune)
- Son "dink" distinctif pour headshots
- Effet attaché au corps de la cible

### 3.5 Statistiques

| Stat | Description |
|------|-------------|
| Accuracy | Hits / Total shots (%) |
| Avg Reaction Time | Temps moyen spawn → hit |
| Best Reaction Time | Record de la session |
| KPM | Kills par minute |
| Headshot % | Proportion de headshots |
| Consistency | Écart-type des RT |

---

## 4. Interface Utilisateur

### 4.1 Style

**Minimaliste** inspiré de l'UI Valorant:
- Couleurs: Noir, blanc, accents rouges (#FF4655)
- Typographie: Clean, sans-serif
- Animations: Subtiles, pas distrayantes

### 4.2 Écrans

| Écran | Contenu |
|-------|---------|
| Main Menu | Play, Settings, Quit |
| Mode Select | 4 cartes de mode + description |
| Agent Select | Grille d'agents disponibles |
| Map Select | Grille de maps disponibles |
| Settings | Gameplay, Crosshair, Audio tabs |
| HUD (in-game) | Timer, Score, Accuracy, Last RT |
| Pause | Resume, Restart, Settings, Quit |
| Results | Stats complètes de la session |

### 4.3 Flow Utilisateur

```
Main Menu
    │
    ├─► Mode Select ─► Agent Select ─► Map Select ─► Countdown ─► Playing
    │                                                                │
    │                                                    ┌───────────┤
    │                                                    │           │
    │                                                  Pause      Timer=0
    │                                                    │           │
    │                                                    └─────┬─────┘
    │                                                          │
    └──────────────────────────────────────────────────────── Results
```

---

## 5. Configuration & Settings

### 5.1 Gameplay

| Setting | Type | Range | Défaut |
|---------|------|-------|--------|
| Sensitivity | Slider | 0.1-10 | 1.0 |
| Session Duration | Dropdown | 30s-120s | 60s |
| Show Tutorial | Toggle | - | On |

### 5.2 Audio

| Setting | Type | Range | Défaut |
|---------|------|-------|--------|
| Master Volume | Slider | 0-100% | 100% |
| SFX Volume | Slider | 0-100% | 100% |
| Music Volume | Slider | 0-100% | 50% |

### 5.3 Crosshair

Tous les paramètres Valorant:
- Color (presets + custom)
- Inner Lines (on/off, length, thickness, offset, opacity)
- Outer Lines (idem)
- Center Dot (on/off, size, opacity)
- Outline (on/off, thickness, opacity)
- Code import/export

---

## 6. Audio

### 6.1 Sons Requis

| Son | Source | Usage |
|-----|--------|-------|
| Tir | Style Vandal/Phantom | Chaque clic |
| Hit Body | Impact sourd | Body shot |
| Hit Head (Dink) | Métallique résonant | Headshot |
| Target Spawn | Subtil | Apparition cible |
| UI Click | Standard | Navigation |
| Countdown | Tick + Go | 3-2-1-GO |
| Session End | Fanfare courte | Fin de session |

### 6.2 Musique

| Track | Usage |
|-------|-------|
| Menu Ambient | Main menu, mode select |
| Training Ambient | Pendant les sessions (optionnel) |

---

## 7. Roadmap

### Phase 1: MVP (Semaines 1-3)

- [ ] Setup projet Unity
- [ ] Import modèle ISO
- [ ] Import map The Range (ou placeholder)
- [ ] Système de tir fonctionnel
- [ ] Mode Flick jouable
- [ ] UI minimale (menu, HUD, results)
- [ ] Stats de base

### Phase 2: Core Features (Semaines 4-5)

- [ ] 4 modes d'entraînement
- [ ] Système crosshair complet
- [ ] Hit feedback (particules + sons)
- [ ] Settings complets
- [ ] Persistance des settings

### Phase 3: Content (Semaines 6-7)

- [ ] Import agents additionnels (Jett, Phoenix)
- [ ] Import map Ascent
- [ ] Sélection agent/map dans UI
- [ ] Polish audio
- [ ] Optimisations performance

### Phase 4: Polish (Semaine 8)

- [ ] UI animations
- [ ] Bug fixes
- [ ] Playtesting
- [ ] Build final

---

## 8. Spécifications Techniques

### 8.1 Performance Cibles

| Métrique | Target |
|----------|--------|
| FPS | 144+ stable |
| Input Latency | <10ms |
| Load Time | <3s |

### 8.2 Résolutions Supportées

| Résolution | Aspect |
|------------|--------|
| 1920×1080 | 16:9 |
| 2560×1440 | 16:9 |
| 1920×1200 | 16:10 |

### 8.3 Inputs

| Input | Action |
|-------|--------|
| Mouse Move | Rotation caméra |
| Left Click | Tir |
| ESC | Pause |

---

## 9. Contraintes & Limitations

### 9.1 Légales

- **Usage personnel uniquement**
- Pas de distribution commerciale
- Modèles Valorant = propriété Riot Games

### 9.2 Techniques

- Pas de multijoueur
- Pas de leaderboards online
- Modèles doivent être fournis par l'utilisateur

---

## 10. Critères de Succès

### MVP Validé Si:

- [ ] Peut compléter une session de 60s en mode Flick
- [ ] Cible ISO s'affiche et réagit aux hits
- [ ] Stats affichées à la fin
- [ ] Crosshair personnalisable
- [ ] Son dink sur headshots

### V1.0 Validé Si:

- [ ] 4 modes jouables
- [ ] Au moins 2 agents disponibles
- [ ] Au moins 1 map Valorant
- [ ] Settings persistants
- [ ] 144 FPS stable

---

## 11. Assets Requis

### À Obtenir

| Asset | Format | Source |
|-------|--------|--------|
| ISO model | GLB/FBX | Valorant rip |
| The Range map | GLB/FBX | Valorant rip |
| Autres agents | GLB/FBX | À venir |
| Autres maps | GLB/FBX | À venir |

### À Créer

| Asset | Type |
|-------|------|
| Sons SFX | WAV/OGG |
| Particules hit | Unity Particles |
| UI sprites | PNG |

---

## 12. Questions Ouvertes

1. **Format des modèles**: GLB ou FBX préféré?
2. **Maps**: Importer la map entière ou juste une zone?
3. **Agents animés?**: T-pose statique ou idle animation?
4. **Arme visible?**: Afficher un modèle d'arme ou juste crosshair?

---

*Document à utiliser comme base pour générer le PRD complet.*
