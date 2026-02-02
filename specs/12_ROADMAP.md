# 12 — Roadmap & Planning

> **Priorité**: P0
> **Statut**: Draft

---

## 1. Vue d'Ensemble

### 1.1 Timeline Globale

| Phase | Semaines | Focus | Objectif |
|-------|----------|-------|----------|
| **Phase 1: MVP** | 1-3 | Foundation | Session jouable |
| **Phase 2: Core** | 4-5 | Features | Expérience complète |
| **Phase 3: Content** | 6-7 | Assets | Multi-agents/maps |
| **Phase 4: Polish** | 8 | Quality | Release ready |

### 1.2 Milestones

| Milestone | Date Target | Critères |
|-----------|-------------|----------|
| **MVP** | Fin Semaine 3 | Mode Flick jouable, 1 agent, stats |
| **Alpha** | Fin Semaine 5 | 4 modes, crosshair complet, settings |
| **Beta** | Fin Semaine 7 | Multi-agents, multi-maps, audio |
| **Release** | Fin Semaine 8 | Stable, optimisé, testé |

---

## 2. Phase 1: MVP (Semaines 1-3)

### 2.1 Semaine 1: Foundation

**Objectif:** Setup projet et architecture de base

| Task | Priorité | Dépendances | Livrable |
|------|----------|-------------|----------|
| Setup projet Unity 2022.3 LTS | P0 | - | Project créé |
| Configuration URP | P0 | Project | Render pipeline OK |
| Installation packages (glTFast, Input System) | P0 | Project | Packages installés |
| Structure dossiers projet | P0 | Project | Folders créés |
| GameManager (singleton) | P0 | Structure | GameManager.cs |
| EventBus (observer pattern) | P0 | Structure | EventBus.cs |
| Scene _Preload + Bootstrap | P0 | GameManager | Scene fonctionnelle |
| Player Controller (FPS camera) | P0 | Scene | Mouvement caméra |
| Input configuration (Mouse look, Shoot) | P0 | Player | Inputs mappés |

**Définition of Done Semaine 1:**
- [ ] Projet Unity configuré avec URP
- [ ] Peut bouger la caméra avec la souris
- [ ] Peut détecter le clic gauche
- [ ] Architecture de base en place

### 2.2 Semaine 2: Core Gameplay

**Objectif:** Système de tir et cibles fonctionnels

| Task | Priorité | Dépendances | Livrable |
|------|----------|-------------|----------|
| Import modèle ISO (GLB) | P0 | glTFast | Model importé |
| Configuration prefab ISO | P0 | Model | ISO.prefab |
| Setup hitboxes (head + body) | P0 | Prefab | Colliders configurés |
| HitZone component | P0 | Hitboxes | HitZone.cs |
| Target component | P0 | HitZone | Target.cs |
| ShootingSystem (raycast) | P0 | Target | ShootingSystem.cs |
| Object Pool | P0 | - | ObjectPool.cs |
| TargetSpawner | P0 | Pool, Target | TargetSpawner.cs |
| FlickMode (basic) | P0 | Spawner | FlickMode.cs |
| Hit detection (body vs head) | P0 | Shooting | Détection fonctionnelle |

**Définition of Done Semaine 2:**
- [ ] Agent ISO visible dans la scene
- [ ] Peut tirer et toucher la cible
- [ ] Distinction headshot vs bodyshot
- [ ] Cibles respawn après hit

### 2.3 Semaine 3: MVP Complete

**Objectif:** Session jouable de bout en bout

| Task | Priorité | Dépendances | Livrable |
|------|----------|-------------|----------|
| Map placeholder ou The Range | P0 | - | Scene training |
| Spawn zones configuration | P0 | Map | Zones définies |
| SessionManager | P0 | - | SessionManager.cs |
| SessionStats tracking | P0 | SessionManager | Stats collectées |
| Timer countdown | P0 | SessionManager | 3-2-1-GO |
| Session timer | P0 | SessionManager | Countdown 60s |
| HUD basique (timer, score, accuracy) | P0 | Stats | HUD fonctionnel |
| Crosshair basique (static) | P0 | - | Crosshair visible |
| Results screen (basic) | P0 | Stats | Affichage stats fin |
| Main Menu (Play, Quit) | P0 | - | Navigation basique |
| Game state machine | P0 | GameManager | États fonctionnels |

**Définition of Done Semaine 3 (MVP):**
- [ ] Peut lancer une session depuis le menu
- [ ] Countdown 3-2-1-GO
- [ ] 60 secondes de gameplay mode Flick
- [ ] Stats affichées pendant et après session
- [ ] Peut retourner au menu

---

## 3. Phase 2: Core Features (Semaines 4-5)

### 3.1 Semaine 4: Modes & Crosshair

**Objectif:** Tous les modes + crosshair complet

| Task | Priorité | Dépendances | Livrable |
|------|----------|-------------|----------|
| ITrainingMode interface | P1 | FlickMode | Interface propre |
| TrackingMode | P1 | Interface | Mode fonctionnel |
| SpeedMode | P1 | Interface | Mode fonctionnel |
| HeadshotMode | P1 | Interface | Mode fonctionnel |
| Mode selection UI | P1 | Modes | Écran sélection |
| CrosshairSettings (ScriptableObject) | P1 | - | Data structure |
| CrosshairRenderer (4 layers) | P1 | Settings | Rendu complet |
| Crosshair customization UI | P1 | Renderer | Sliders, toggles |
| Crosshair preview | P1 | UI | Prévisualisation |
| Crosshair code import/export | P2 | Settings | Parse Valorant codes |

**Définition of Done Semaine 4:**
- [ ] 4 modes jouables
- [ ] Peut sélectionner le mode avant session
- [ ] Crosshair entièrement personnalisable
- [ ] Preview en temps réel

### 3.2 Semaine 5: Feedback & Settings

**Objectif:** Feedback complet + persistance

| Task | Priorité | Dépendances | Livrable |
|------|----------|-------------|----------|
| Hit particles (body - orange) | P1 | - | Particle system |
| Hit particles (head - yellow) | P1 | - | Particle system |
| Particle pooling | P1 | Particles | Pool fonctionnel |
| AudioManager | P1 | - | AudioManager.cs |
| Gun shot sound | P1 | AudioManager | Son intégré |
| Body hit sound | P1 | AudioManager | Son intégré |
| Headshot dink sound | P1 | AudioManager | Son signature |
| Countdown sounds | P1 | AudioManager | Tick + GO |
| UI sounds (hover, click) | P2 | AudioManager | Sons UI |
| GameSettings (ScriptableObject) | P1 | - | Data structure |
| AudioSettings | P1 | - | Data structure |
| Settings UI (Gameplay tab) | P1 | Settings | Sliders fonctionnels |
| Settings UI (Audio tab) | P1 | Settings | Volume controls |
| SaveSystem (JSON) | P1 | Settings | Persistance disk |
| Load settings on start | P1 | SaveSystem | Auto-load |

**Définition of Done Semaine 5 (Alpha):**
- [ ] Feedback visuel sur chaque hit
- [ ] Sons pour tir, hit, headshot
- [ ] Settings modifiables et sauvegardés
- [ ] Settings persistent entre sessions

---

## 4. Phase 3: Content (Semaines 6-7)

### 4.1 Semaine 6: Agents

**Objectif:** Multi-agents fonctionnel

| Task | Priorité | Dépendances | Livrable |
|------|----------|-------------|----------|
| Import modèle Jett | P1 | glTFast | Model importé |
| Configuration prefab Jett | P1 | Model | Jett.prefab |
| Hitbox calibration Jett | P1 | Prefab | Colliders ajustés |
| AgentData Jett | P1 | Prefab | ScriptableObject |
| Import modèle Phoenix | P1 | glTFast | Model importé |
| Configuration prefab Phoenix | P1 | Model | Phoenix.prefab |
| Hitbox calibration Phoenix | P1 | Prefab | Colliders ajustés |
| AgentData Phoenix | P1 | Prefab | ScriptableObject |
| Agent selection UI | P1 | AgentData | Écran sélection |
| Agent info panel | P1 | UI | Description agent |
| TargetSpawner multi-agent | P1 | Agents | Spawn agent sélectionné |

**Définition of Done Semaine 6:**
- [ ] 3 agents disponibles (ISO, Jett, Phoenix)
- [ ] Peut sélectionner l'agent avant session
- [ ] Hitboxes calibrées par agent

### 4.2 Semaine 7: Maps & Polish

**Objectif:** Multi-maps + polish général

| Task | Priorité | Dépendances | Livrable |
|------|----------|-------------|----------|
| Import map Ascent (ou zone) | P1 | glTFast | Map importée |
| Scene setup Ascent | P1 | Map | Scene jouable |
| Spawn zones Ascent | P1 | Scene | Zones configurées |
| MapData Ascent | P1 | Scene | ScriptableObject |
| Map selection UI | P1 | MapData | Écran sélection |
| Additive scene loading | P1 | Scenes | Load/unload maps |
| Target spawn sound | P2 | AudioManager | Son subtil |
| Session end sound | P2 | AudioManager | Fanfare |
| Menu ambient music | P2 | AudioManager | Loop ambient |
| Performance profiling | P1 | All | Profiler clean |
| Memory optimization | P1 | Profiling | No leaks |
| Draw call optimization | P2 | Profiling | Batching |

**Définition of Done Semaine 7 (Beta):**
- [ ] 2 maps jouables
- [ ] Audio complet
- [ ] Performance stable 144 FPS
- [ ] Pas de memory leaks

---

## 5. Phase 4: Polish (Semaine 8)

### 5.1 Tasks Polish

| Task | Priorité | Dépendances | Livrable |
|------|----------|-------------|----------|
| UI transitions/animations | P2 | UI | Animations subtiles |
| Screen fade transitions | P2 | UI | Smooth transitions |
| Button hover animations | P2 | UI | Feedback visuel |
| Stats reveal animation | P2 | Results | Stagger reveal |
| Bug fixing | P0 | Testing | Bugs fixés |
| Edge case handling | P1 | Testing | Robustesse |
| Playtesting | P0 | All | Feedback collecté |
| Balance adjustment | P1 | Playtesting | Tweaks valeurs |
| Final performance pass | P1 | All | 144 FPS confirmé |
| Build configuration | P0 | All | IL2CPP, Release |
| Windows build | P0 | Config | .exe fonctionnel |
| Final testing | P0 | Build | Smoke test OK |

**Définition of Done Semaine 8 (Release):**
- [ ] Aucun bug critique
- [ ] UI polished avec animations
- [ ] Build Windows stable
- [ ] 144 FPS sur hardware recommandé

---

## 6. Critères d'Acceptation par Phase

### 6.1 MVP Acceptance

| ID | Critère | Test |
|----|---------|------|
| MVP-01 | Peut compléter session 60s mode Flick | Play full session |
| MVP-02 | Cible ISO affichée correctement | Visual check |
| MVP-03 | Hits détectés (body + head) | Shoot and verify |
| MVP-04 | Stats en fin de session | Complete session |
| MVP-05 | Crosshair visible et centré | Visual check |
| MVP-06 | Son de tir | Click and listen |
| MVP-07 | Navigation menu fonctionnelle | Navigate all screens |

### 6.2 Alpha Acceptance

| ID | Critère | Test |
|----|---------|------|
| ALP-01 | 4 modes fonctionnels | Play each mode |
| ALP-02 | Crosshair fully customizable | Adjust all settings |
| ALP-03 | Settings persistent | Change, restart, verify |
| ALP-04 | Hit feedback (particles + sound) | Get hits |
| ALP-05 | Headshot dink distinctif | Get headshots |

### 6.3 Beta Acceptance

| ID | Critère | Test |
|----|---------|------|
| BET-01 | 3 agents sélectionnables | Select each, play |
| BET-02 | 2 maps sélectionnables | Select each, play |
| BET-03 | Audio complet | All sounds present |
| BET-04 | 144 FPS stable | Profiler check |
| BET-05 | No memory leaks (10min) | Extended play |

### 6.4 Release Acceptance

| ID | Critère | Test |
|----|---------|------|
| REL-01 | Aucun crash en 1h | Extended playtest |
| REL-02 | UI animations smooth | Visual check |
| REL-03 | Build Windows fonctionne | Run exe |
| REL-04 | Load time < 3s | Measure |
| REL-05 | All features documented | Review |

---

## 7. Risques & Mitigations

### 7.1 Risques Techniques

| Risque | Probabilité | Impact | Mitigation |
|--------|-------------|--------|------------|
| GLB import échoue | Medium | High | Test early, fallback FBX |
| Performance < 144 FPS | Low | High | Profile régulièrement |
| Hitbox imprécises | Medium | Medium | Calibration avec références |

### 7.2 Risques Planning

| Risque | Probabilité | Impact | Mitigation |
|--------|-------------|--------|------------|
| Scope creep | High | Medium | Strict MVP, backlog |
| Assets non disponibles | Medium | High | Placeholders prêts |
| Bugs bloquants | Medium | High | Testing continu |

---

## 8. Backlog Post-V1.0

### 8.1 Features Futures

| Feature | Priority | Notes |
|---------|----------|-------|
| Agents additionnels | P2 | Breach, Cypher, etc. |
| Maps additionnelles | P2 | Bind, Haven, etc. |
| Historical stats | P2 | Progression tracking |
| Custom mode | P3 | User-defined parameters |
| Leaderboards (local) | P3 | Personal bests |
| Achievements | P3 | Goals et rewards |
| Tutorial interactif | P2 | Guide nouveaux joueurs |
| Dynamic crosshair | P3 | Movement/firing error |
| Player movement | P3 | WASD |
| Multiple weapons | P3 | Vandal, Phantom, etc. |

---

## 9. Definition of Done (Global)

### 9.1 Pour chaque Feature

```
□ Code écrit et fonctionnel
□ Pas d'erreurs dans Console
□ Pas de warnings significatifs
□ Testé manuellement
□ Performance acceptable
□ Documenté si complexe
```

### 9.2 Pour chaque Phase

```
□ Tous les critères d'acceptation passent
□ Playtest effectué
□ Profiling clean
□ Pas de bugs critiques
□ Build testée
```

---

## 10. Ressources Requises

### 10.1 Assets à Obtenir

| Asset | Phase | Status |
|-------|-------|--------|
| Model ISO | Phase 1 | Required |
| Model Jett | Phase 3 | Required |
| Model Phoenix | Phase 3 | Required |
| Map The Range | Phase 1 | Required (ou placeholder) |
| Map Ascent | Phase 3 | Required |
| Sons SFX | Phase 2 | To create/source |
| UI Sprites | Phase 1-2 | To create |

### 10.2 Outils

| Outil | Usage |
|-------|-------|
| Unity 2022.3 LTS | Développement |
| VS Code / Rider | IDE |
| Git | Version control |
| Blender (optionnel) | Ajustement models |

---

*Ce document définit le planning complet du projet. Référencer les specs individuelles pour les détails d'implémentation.*
