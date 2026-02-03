# Plan: Mode Elimination avec Déplacement VALORANT

## Résumé
Transformer l'aim trainer d'un mode statique (tir sur cibles en strafe) vers un mode **Elimination** avec déplacement FPS complet style VALORANT.

## Fonctionnalités à implémenter

### 1. Système de déplacement joueur
- **ZQSD** (AZERTY par défaut) : Z=avancer, S=reculer, Q=gauche, D=droite
- **Course par défaut** (~5.4 m/s comme VALORANT)
- **SHIFT** = marcher (plus lent ~2.8 m/s)
- **CTRL** = s'accroupir (~1.9 m/s + hauteur réduite)
- **ESPACE** = sauter
- Utilisation de `CharacterController` (standard FPS)

### 2. Configuration des touches clavier
- Interface dans les Settings pour rebind les touches
- Sauvegarde via PlayerPrefs
- API Unity InputSystem pour le rebinding interactif

### 3. Mode Elimination
- Targets placés à des positions fixes (via Transform dans l'éditeur Unity)
- **Pas de strafe** (cibles statiques)
- **Pas de limite de temps**
- Chronomètre qui compte le temps écoulé (affiché coin supérieur gauche)
- Fin quand tous les targets sont éliminés
- Résultats : temps de complétion + stats habituelles

---

## Fichiers à créer

| Fichier | Description |
|---------|-------------|
| `Scripts/Data/KeybindSettings.cs` | Structure de données pour les keybinds |
| `Scripts/Core/KeybindManager.cs` | Gestionnaire de rebinding avec InputSystem |
| `Scripts/Data/ModeConfiguration.cs` | Configuration par mode de jeu |
| `Scripts/UI/KeybindSettingsPanel.cs` | UI pour configurer les touches |

---

## Fichiers à modifier

### Phase 1 : Fondation (données)

**`Scripts/Data/GameSettings.cs`**
- Ajouter constantes de vitesse VALORANT (RUN=5.4, WALK=2.8, CROUCH=1.9, JUMP=7.0)
- Ajouter propriétés pour les vitesses de mouvement

**`Scripts/Data/SessionStats.cs`**
- Ajouter `CompletionTime` (temps pour finir)
- Ajouter `TotalTargetsToKill` et `IsEliminationMode`
- Ajouter méthode `AreAllTargetsEliminated()`

**`Scripts/Core/GameManager.cs`**
- Ajouter `TrainingMode.Elimination` à l'enum

### Phase 2 : Systèmes core

**`Scripts/Core/SessionManager.cs`**
- Support dual-mode : timer countdown VS chrono montant
- `_elapsedTime` pour tracker le temps en Elimination
- Fin de session quand tous targets éliminés

**`Scripts/Gameplay/Target.cs`**
- Ajouter `SetStrafingEnabled(bool)` public
- Ajouter `SetInfiniteLifetime()` pour mode Elimination

**`Scripts/Gameplay/TargetSpawner.cs`**
- Ajouter `Transform[] fixedSpawnPoints` (assignés dans l'éditeur)
- Ajouter `_useFixedSpawns` et logique de spawn fixe
- Spawn tous les targets au début en Elimination

### Phase 3 : Mouvement joueur

**`Scripts/Gameplay/PlayerController.cs`**
- Ajouter `CharacterController` (RequireComponent)
- Ajouter champs : `_moveInput`, `_velocity`, `_isGrounded`, `_isCrouching`, `_isWalking`
- Implémenter `HandleMovement()` : direction relative au facing, gravité, saut
- Implémenter `HandleCrouch()` : transition hauteur smoothe
- Lire les InputActions : Move, Jump, Crouch, Walk

**`InputSystem_Actions.inputactions`**
- Modifier bindings Move pour ZQSD (au lieu de WASD)
- Ajouter Left Control pour Crouch
- Renommer "Sprint" en "Walk" (logique VALORANT inversée)

### Phase 4 : UI

**`Scripts/UI/HUDController.cs`**
- Afficher temps écoulé (chrono montant) dans le panneau stats (coin supérieur gauche)
- Afficher "Targets restants: X" en mode Elimination

**`Scripts/UI/ResultsScreen.cs`**
- Afficher "TEMPS DE COMPLÉTION" en mode Elimination
- Code couleur selon performance (vert < 10s, jaune < 20s)

**`Scripts/UI/SettingsScreen.cs`**
- Ajouter section "CONTRÔLES" avec bouton "CONFIGURER TOUCHES"
- Ouvrir KeybindSettingsPanel au clic

### Phase 5 : Intégration

**`Scripts/Core/EventBus.cs`**
- Ajouter `OnAllTargetsEliminated` event
- Ajouter `OnRebindStarted` / `OnRebindComplete` events

---

## Configuration scène Unity (manuel)

1. **Player GameObject** :
   - Ajouter composant `CharacterController` (height=1.8, radius=0.3)

2. **Spawn Points Elimination** :
   - Créer des GameObjects vides comme marqueurs de position
   - Les positionner dans la map aux endroits souhaités
   - Les assigner dans `TargetSpawner.fixedSpawnPoints[]`

3. **KeybindManager** :
   - Créer GameObject avec composant `KeybindManager`
   - Assigner l'asset `InputSystem_Actions`

---

## Ordre d'implémentation

1. `GameSettings.cs` - constantes mouvement
2. `ModeConfiguration.cs` - nouveau fichier
3. `SessionStats.cs` - champs completion
4. `KeybindSettings.cs` - nouveau fichier
5. `KeybindManager.cs` - nouveau fichier
6. `TrainingMode enum` - ajouter Elimination
7. `SessionManager.cs` - support dual-mode
8. `Target.cs` - toggle strafe + infinite lifetime
9. `TargetSpawner.cs` - fixed spawn points
10. `PlayerController.cs` - mouvement complet
11. `InputSystem_Actions.inputactions` - bindings ZQSD
12. `HUDController.cs` - chrono + targets restants
13. `ResultsScreen.cs` - temps complétion
14. `SettingsScreen.cs` - section contrôles
15. `KeybindSettingsPanel.cs` - nouveau fichier
16. `EventBus.cs` - nouveaux events

---

## Vérification

1. **Test mouvement** : Lancer le jeu, vérifier ZQSD/Shift/Ctrl/Space
2. **Test rebind** : Settings → Contrôles → Changer une touche → Vérifier en jeu
3. **Test Elimination** :
   - Placer 5 spawn points dans la scène
   - Lancer mode Elimination
   - Tuer tous les targets
   - Vérifier que le chrono s'arrête et affiche le temps final
4. **Test stats** : Vérifier accuracy, headshots, temps de complétion dans Results

---

## Architecture détaillée

### Constantes de mouvement VALORANT
```csharp
public const float VALORANT_RUN_SPEED = 5.4f;    // Course (défaut)
public const float VALORANT_WALK_SPEED = 2.8f;   // Marche (Shift)
public const float VALORANT_CROUCH_SPEED = 1.9f; // Accroupi
public const float VALORANT_JUMP_FORCE = 7.0f;   // Force de saut
public const float VALORANT_GRAVITY = 20f;       // Gravité
```

### Structure ModeConfiguration
```csharp
public class ModeConfiguration
{
    public TrainingMode mode;
    public bool useTimerLimit;           // false pour Elimination
    public bool useRandomSpawns;         // false pour Elimination
    public bool enableTargetStrafing;    // false pour Elimination
    public bool trackCompletionTime;     // true pour Elimination
    public int fixedTargetCount;         // nombre de targets
}
```

### Flux du mode Elimination
```
StartSession()
  ↓
Spawn tous les targets aux positions fixes
  ↓
Joueur se déplace et élimine les targets
  ↓
Chrono monte (affiché en haut à gauche)
  ↓
Dernier target éliminé → OnAllTargetsEliminated
  ↓
EndSession() avec CompletionTime
  ↓
ResultsScreen affiche le temps + stats
```
