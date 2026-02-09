# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

A mystery detective card game built in **Unity 6.0 LTS** (Unity 6000.0.32f1) with Russian-language narrative. Players solve a murder mystery through card arrangement mechanics, matching value+suit combinations to goals across 6 rounds before a final "postdiction" scene where they identify the culprit.

## Build & Development

This is a standard Unity project. Open in Unity Editor 6000.0.32f1 or compatible version.

**Opening**: File → Open Project → Select this folder

**Running**: Open `Assets/Scenes/MainMenu.unity` and press Play, or use `Assets/Scenes/MainScene.unity` for direct gameplay testing. `DevMainScene.unity` exists for development/testing.

**Build**: File → Build Settings → Select target platform → Build

## Architecture

### Namespace Organization
```
CardGame.Core          - Card data, deck, scoring calculations
CardGame.Cards         - SimpleCard UI component
CardGame.GameObjects   - CardBoard, dragging, deck visuals
CardGame.Managers      - RoundManager, TutorialManager, AudioManager
CardGame.Scoring       - CardScorer (live score display)
DefaultNamespace.Tiles - Story tiles & hints for postdiction
```

### Core Systems

**Card System** (`Assets/Scripts/Core/`):
- `CardData` - Serializable card info (suit, value 1-3, sprite)
- `CardDeck` - 24-card deck with Fisher-Yates shuffle
- `CardLayout` - Score calculation with position-based multipliers
- `CardCombinations` - Brute-force permutation checking for goal matching

**Multiplier Rules** (critical for scoring):
- **Roses**: 2x in center positions, 1x on edges
- **Coins**: 2x if adjacent card is also Coins
- **Skulls**: 2x on edge positions, 1x in center
- **Crowns**: 2x if NO adjacent Crowns

**Game Flow** (`Assets/Scripts/Managers/RoundManager.cs`):
- Generates random goals (value 8-14, random suit limited to 2 occurrences per game)
- Deals 5 cards per round from `SimpleDeckObject`
- Calculates scores via `CardLayout` when player submits
- After 6 rounds, loads PostdictionScene for culprit selection

**Card Board** (`Assets/Scripts/GameObjects/CardBoard.cs`):
- Magnetic layout system for card arrangement
- Position-based insertion during drag/drop
- Freeze states for tutorial control
- Integrates with `CardScorer` for real-time score updates

**Tutorial System** (`Assets/Scripts/Managers/TutorialManager.cs`):
- 17-step progressive tutorial with individual card freezing
- Step validation via coroutines
- Spawns predefined cards for guided learning

### Scene Flow
```
MainMenu → MainScene (6 rounds) → PostdictionScene → Win/Lose → MainMenu
              ↓
         TutorialScene (standalone)
```

### Key Prefabs
- `SimpleCardPrefab.prefab` - Card UI template
- `SimpleDeck.prefab` - Deck visual

### Audio
`AudioManager` is a singleton (100 lines) with only 2 AudioSources (music + sfx), routed through a Unity `AudioMixer` asset (`Assets/Audio/MainMixer`). Volume is controlled via exposed mixer parameters (`MusicVolume`, `SFXVolume`) — no manual volume multiplication in code.

**How to add a new sound:**
1. Add a `[SerializeField] private AudioClip mySound;` field on the component that triggers it
2. Assign the clip in the Inspector on that GameObject
3. Play it with `AudioManager.Instance.PlaySFX(mySound);`
4. For new music tracks: add the clip to `AudioManager` and create a `PlayXxxMusic()` convenience method

**Rules:**
- Never create local `AudioSource` components — all playback goes through `AudioManager`
- Never add per-category volume multipliers — use AudioMixer groups instead
- Sound components (`CardSound`, `UISound`, `RulesPanelSound`) own their clips but don't own AudioSources

## Code Conventions

- Russian comments/strings throughout (narrative game in Russian)
- Coroutines for async operations (dealing, animations, tutorial steps)
- Component-based Unity patterns with `[SerializeField]` for inspector bindings
- Legacy code was removed in Phase 1 cleanup (was in `Assets/Scripts/Legacy/`)

## Configuration Constants (in RoundManager)
- Goal value range: 8-14
- Cards per round: 5
- Max rounds: 6
- Max same suit per game: 2
- Deal delay: 0.3s

## Code Cleanup - Refactoring Plan (complete)

**Phase 1 (DONE):** Removed dead code - Legacy folder (Card.cs, CardSequence.cs, DeckManager.cs, GameplayManager.cs), empty DeckGameObject stub, unused `System.Collections` import in CardBoard.cs.

**Phase 2 (DONE - already fixed):** Bugs from review were already resolved in prior work. CardScorer's `GetSuitColor()` was removed, RulesPanel's float comparison uses threshold now.

**Phase 3 (DONE):** Fixed typos - `rightChoise`/`lastChoise` -> `rightChoice`/`lastChoice` (PostdictionManager), `Patrial` -> `Partial` (SuccessCodes + all refs), `setVisability`/`setHistoryVisability` -> `setVisibility`/`setHistoryVisibility` (TileScript, TilesManager). `rulsePanel` was already fixed.

**Phase 4 (DONE - already resolved):** `GetSuitColor()` duplication no longer exists. Only RoundManager has it now; SimpleCard/CryLogic use sprites not colors; CardScorer's copy was removed in prior work.

**Phase 5 (DONE):** Extracted magic numbers in SimpleDeckObject's `UpdateVisual()` to named constants. Removed unused `UnityEngine.Tilemaps` import from TilesManager. Cleaned ~25 stray `Debug.Log()` calls across CardBoard, CardDeck, CardScorer, RoundManager, SimpleDeckObject, TutorialManager. RoundManager/RulesPanel access modifiers were already clean from prior work.

**Phase 6 (DONE):** Reduced RoundManager complexity. Removed duplicate `ClearBoard` method (reuses `ClearPreviousRoundCards`). Simplified `CalculateRoundScore` (removed redundant condition). Extracted `PrepareForPrediction()` from `ShowRoundResult`. Added section comments for navigation (Lifecycle, Round Flow, Goal Generation, Card Dealing, Score Calculation, UI Updates, Helpers).

## Audio Architecture Refactoring (DONE)

Replaced scattered audio system (7+ components with local AudioSources, 3-layer manual volume) with centralized AudioMixer-based architecture. See `docs/audio-architecture.md` for original plan.

- **AudioManager.cs** (270 → 100 lines): Removed 7 stored SFX clips, 10 per-category volume multipliers, 20+ category methods. Added AudioMixer with exposed `MusicVolume`/`SFXVolume` parameters.
- **CardSound.cs** (167 → 43 lines): Removed 3 local AudioSources, volume fields, loop sounds. One-line `PlaySFX()` calls.
- **UISound.cs** (100 → 30 lines): Removed 2 local AudioSources, volume fields, loop sounds.
- **RulesPanelSound.cs** (126 → 41 lines): Removed 2 local AudioSources, `Update()` polling, slide loop.
- **CardScorer.cs**: Removed unused AudioSource creation.
- **TutorialManager.cs**: Removed unused AudioSource field. Category calls → `PlaySFX()`.
- **RoundManager.cs**: Removed unused AudioSource field. `PlayCardShuffle()` → `PlaySFX()`.
- **TilesManager.cs**: `PlayRoundResult()` → `PlaySFX()`.

Total AudioSources in project: 2 (both on AudioManager). Previously: 20+ (3 per card, 2 per button, 2 per panel, etc.)

## Future Technical Roadmap

### Phase A: Quick Wins (low risk, high impact)

**A1. Cache `FindObjectsOfType` in SimpleDraggableWithBoard**
- `SimpleDraggableWithBoard.cs` calls `FindObjectsOfType<CardBoard>()` during every drag operation (lines ~142, 157, 184)
- This runs O(n) scan each frame while dragging. Cache board references on Awake instead.

**A2. Create `SceneNames` constants class**
- Scene names are hardcoded strings in 6+ files: `MenuManager`, `RoundManager`, `EndMenuManager`, `PostdictionManager`, `GameMenuManager`, `CreatorsManager`
- Create `Assets/Scripts/Core/SceneNames.cs` with `public const string` fields
- One place to update if scene names change

**A3. Add missing namespaces to 8+ classes**
- No namespace: `EndMenuManager`, `FinalImage`, `PostdictionManager`, `CreatorsManager`, `GameMenuManager`, `MenuManager`, `RulesManager`, `CatAnimationController`, `CryLogic`, `TileScript`, `TilesManager`
- Wrap in `CardGame.Managers`, `CardGame.UI`, etc. to match existing conventions

**A4. Replace public fields with `[SerializeField] private`**
- `PostdictionManager.cs`: all public references (lines 15-20)
- `TilesManager.cs`: public arrays and references (lines 9-21)
- `TileScript.cs`: public sprites and manager reference
- `CardBoard.cs`: `public bool neverGlow`, `public CardScorer scorer`

### Phase B: Medium Impact (moderate effort)

**B1. Extract `ScoreCalculator` as non-MonoBehaviour class**
- Pull score calculation logic out of `RoundManager.CalculateRoundScore()` and `CardBoard`
- Makes scoring logic unit-testable without Unity scene

**B2. Create `GameConfig` ScriptableObject**
- Move all game settings (goal range, cards per round, deal delay, max rounds, max same suit) into a single ScriptableObject
- Currently scattered as `[SerializeField]` in `RoundManager.cs` (lines 59-67)
- Allows designers to tweak values without touching code

**B3. Pre-compute card combinations**
- `CardSystem.cs` `AllOrderedSubsets()` generates all permutations every time availability is checked
- For 5 cards this is 120+ layouts per combination
- Cache results or compute only on card release, not during drag

**B4. Create `IAudioService` interface**
- `AudioManager.Instance` singleton used in 10+ places
- Create interface for mockability and cleaner dependency

**B5. Return `IReadOnlyList<T>` instead of copying lists**
- `CardBoard.GetCards()` creates `new List<SimpleCard>(cards)` every call
- `CardBoard.GetCardsData()` creates new list and loops
- Return read-only view instead of allocating

### Phase C: Architectural Refactor (high effort, high reward)

**C1. Implement event system**
- Create `GameEventBus` with events: `OnRoundEnded`, `OnGoalSet`, `OnCardPlaced`, `OnScoreChanged`
- Decouple `TilesManager` from `RoundManager` (currently direct method call)
- Let `CardScorer` subscribe to board changes instead of being polled
- Replace Update-loop polling in `RoundManager` (line 160-163) and `BallSpriteByGoalSuit`

**C2. Split large classes**
- `TutorialManager.cs` (909 lines) - extract tutorial step definitions, bubble management, card spawning into separate classes
- `RoundManager.cs` (730 lines) - extract `RoundGoalGenerator`, `CardDealer`
- `CardSystem.cs` (329 lines) - split `Card`, `Score`, `CardLayout`, `CardCombinations` into separate files

**C3. Explicit state machine for round flow**
- Replace boolean flags (`isDealing`, `isRoundActive`, `isWaitingToDeal`, `isReadyForPrediction`) with:
  ```
  enum RoundState { Waiting, Dealing, Active, ShowingResult, ReadyForPrediction }
  ```
- Prevents invalid state combinations

**C4. Fix remaining naming inconsistencies**
- Method casing: `setTexture()`, `setVisibility()`, `clickOn()` should be PascalCase
- `RulesCords` enum should be `RulesCoords`
- `CryLogic` class should be `CrystalDisplay` or similar
- `Cristal` typo in CryLogic field names (`grayCristal`, `roseCristal`, etc.)

### Phase D: Long-term (requires planning)

**D1. Add unit test framework**
- Extract business logic from MonoBehaviours into testable POCO classes
- Create interfaces for key components (`ICardBoard`, `IDeck`, `IAudioService`)
- Set up Unity Test Framework with EditMode tests for logic, PlayMode tests for integration

**D2. Dependency injection**
- Replace singleton access and `FindObjectsOfType` with proper DI
- Consider Zenject/VContainer or a lightweight custom solution

**D3. Performance profiling pass**
- Profile actual frame times to prioritize optimization
- Address `CardCombinations` algorithm complexity
- Review all Update() methods for unnecessary work
