# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

A mystery detective card game built in **Unity 6.0 LTS** (Unity 6000.0.32f1) with Russian-language narrative. Players solve a murder mystery through card arrangement mechanics, matching value+suit combinations to goals across 6 rounds before a final "postdiction" scene where they identify the culprit.

## Build & Development

This is a standard Unity project. Open in Unity Editor 6000.0.32f1 or compatible version.

**Opening**: File → Open Project → Select this folder

**Running**: Open `Assets/Scenes/MainMenu.unity` and press Play, or use `Assets/Scenes/MainScene.unity` for direct gameplay testing.

**Build**: File → Build Settings → Select target platform → Build

## Architecture

### Namespace Organization
```
CardGame.Core          - Card data, deck, scoring calculations, SceneNames
CardGame.Cards         - SimpleCard (SpriteRenderer-based card)
CardGame.GameObjects   - CardBoard, dragging, deck (world-space)
CardGame.Managers      - RoundManager, TutorialManager, AudioManager, MenuManager,
                         EndMenuManager, PostdictionManager, GameMenuManager,
                         CreatorsManager, RulesManager
CardGame.Scoring       - CardScorer (live score display)
CardGame.UI            - FinalImage, CatAnimationController, CrystalDisplay
DefaultNamespace.Tiles - Story tiles, hints, TileScript, TilesManager
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
- `SimpleCardPrefab.prefab` - Card template (SpriteRenderer + BoxCollider2D, Sorting Layer: Cards)
- `SimpleDeck.prefab` - Deck visual (SpriteRenderer + BoxCollider2D, Sorting Layer: Gameplay)

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
- `CardSound` uses `OnMouseEnter`/`OnMouseDown` for hover/click + explicit `PlayPickup()`/`PlayDrop()` called from `SimpleDraggableWithBoard`

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

**Phase 4 (DONE - already resolved):** `GetSuitColor()` duplication no longer exists. Only RoundManager has it now; SimpleCard/CrystalDisplay use sprites not colors; CardScorer's copy was removed in prior work.

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

## Canvas-to-World-Space Refactoring (DONE)

Moved gameplay objects (cards, boards, deck) from ScreenSpace-Overlay Canvas to world-space with SpriteRenderers. Pure UI stays on Canvas overlay. All scenes (MainScene, TutorialScene) migrated. DevMainScene deprecated (to be deleted).

### Rendering Architecture
```
Scene Root
  Main Camera (Orthographic, size=5.4, pos 0,0,-10)
  GameplayRoot (empty, pos 0,0,0)
    Background (SpriteRenderer, Sorting Layer: Background)
    SimpleDeck (SpriteRenderer + BoxCollider2D, Sorting Layer: Gameplay)
      AdviceGlow (child, toggled by RoundManager between rounds)
    CardBoard (SpriteRenderer, Sorting Layer: Gameplay)
    CardBoardHand (SpriteRenderer, Sorting Layer: Gameplay)
    GoalPanel (empty container, world-space)
      Dialog_bubble (Animator)
        bubble (SpriteRenderer, Sorting Layer: Gameplay, order 1)
        Suit (TextMeshPro 3D, Sorting Layer: Gameplay, order 2)
      Ball (SpriteRenderer + BallSpriteByGoalSuit, Sorting Layer: Gameplay, order 1)
      Number (TextMeshPro 3D, Sorting Layer: Gameplay, order 2)
  Canvas (ScreenSpace-Overlay)
    EndRoundButton, ScoreDisplay, Tiles Panel,
    RulesPanel, Cat, Frog_hands, RoundManager/TutorialManager refs
  EventSystem
  Managers/AudioManager
```

**Sorting Layers** (Project Settings → Tags & Layers):
1. `Background` — scene background
2. `Gameplay` — boards, deck
3. `Cards` — card sprites (sortingOrder 0 at rest, 100 during drag)

**Coordinate mapping:** `old_canvas_pixels / 100 = world_units` (100 PPU)

### Code Changes (DONE)
| File | Change |
|------|--------|
| `SimpleCard.cs` | `Image` → `SpriteRenderer`, `CanvasGroup` → `BoxCollider2D` + color |
| `CardBoard.cs` | `RectTransform` → serialized `boardWidth`/`boardHeight`, `Image` → `SpriteRenderer`, world-space bounds via `Camera.ScreenToWorldPoint` |
| `SmoothCardMover` | `RectTransform.anchoredPosition` → `Transform.localPosition` |
| `SimpleDraggableWithBoard.cs` | Full rewrite: EventSystem drag → `OnMouseDown`/`OnMouseDrag`/`OnMouseUp`, sorting order 100 during drag |
| `CardSound.cs` | EventSystem interfaces → `OnMouseEnter`/`OnMouseDown` + explicit `PlayPickup()`/`PlayDrop()` |
| `SimpleDeckObject.cs` | `Image` → `SpriteRenderer`, removed `Canvas` ref, removed dead code, added `UnityEvent onClick` for deck click, added `adviceGlow` field + `SetAdviceGlow()` |
| `TutorialManager.cs` | `WaitForDeckClick()`: `EventSystem.RaycastAll` → `Physics2D.Raycast`; goal fields: `TextMeshProUGUI` → `TextMeshPro`, `Image ballImage` → `SpriteRenderer ballImage` |
| `RoundManager.cs` | `OnStartButtonClicked()` made public; goal fields: `TextMeshProUGUI` → `TextMeshPro`, `Image goalCardImage` → `SpriteRenderer goalCardImage`; null-safe `SetIsWaitingToDeal()`; controls deck `adviceGlow` visibility; deck click no longer triggers postdiction (only end button does) |
| `BallSpriteByGoalSuit.cs` | Removed `Image` dependency — only uses `SpriteRenderer` now |

### Scene Setup (DONE — MainScene and TutorialScene migrated)

Both scenes have:
- Main Camera: Orthographic, Size=5.4, Position=(0,0,-10), Tag=MainCamera
- Gameplay Root at (0,0,0) containing CardBoard, CardBoardHand, SimpleDeck, GoalPanel, Background
- World positions: CardBoard (3.308,-1.108,0), CardBoardHand (3.296,-3.094,0), SimpleDeck (-1.085,-2.010,0), GoalPanel (-1.0846, 0.3108, 0)
- CardBoard Inspector: `boardWidth=5.92`, `boardHeight=1.58`, `edgeExtension=1.0`
- CardBoardHand Inspector: `boardWidth=5.95`, `boardHeight=1.70`, `edgeExtension=1.0`
- SimpleDeck: `onClick` event wired to `RoundManager.OnStartButtonClicked` (MainScene); TutorialScene uses `Physics2D.Raycast` for deck detection
- SimpleDeck: BoxCollider2D (required for click detection and TutorialManager's `Physics2D.Raycast`)
- SimpleDeck: AdviceGlow child assigned to `adviceGlow` field (glows between rounds)
- GoalPanel: all children use plain Transform (not RectTransform), layer Default (not UI)
- GoalPanel: bubble + Ball use SpriteRenderer; Suit + Number use TextMeshPro 3D with MeshRenderer

**Migration tool:** `Assets/Editor/SceneMigrationTool.cs` — Editor script used for TutorialScene migration (menu: Tools > Migrate Scene to World-Space). Can be deleted now that migration is complete.

### Key Rules
- Gameplay objects (cards, boards, deck, goal panel) use `SpriteRenderer`/`TextMeshPro` + `BoxCollider2D` in world-space
- UI elements (buttons, score text, tiles panel, rules panel) stay on `ScreenSpace-Overlay` Canvas
- Card interaction uses `OnMouse*` callbacks (requires `BoxCollider2D`), NOT EventSystem drag interfaces
- `SimpleCard.cardRenderer` must point to the card's own `SpriteRenderer` (not parent/sibling)
- Cards set their sorting layer to "Cards" programmatically in `Awake()`
- During drag, card sorting order raises to 100 and collider is disabled
- All objects under GameplayRoot must use plain `Transform` (not `RectTransform`) and layer Default (not UI)

## Future Technical Roadmap

### Phase A: Quick Wins (DONE)

**A1. (DONE)** Cached `FindObjectsOfType<CardBoard>()` in `SimpleDraggableWithBoard.Awake()` — no more per-frame scans during drag.

**A2. (DONE)** Created `Assets/Scripts/Core/SceneNames.cs` with all scene name constants. Replaced hardcoded strings in 8 files. Fixed bug: `"PostDictionScene"` → `SceneNames.PostdictionScene` (wrong casing caused scene load failure).

**A3. (DONE)** Added namespaces to 11 classes: `MenuManager`, `EndMenuManager`, `PostdictionManager`, `GameMenuManager`, `CreatorsManager`, `RulesManager` → `CardGame.Managers`; `FinalImage`, `CatAnimationController`, `CrystalDisplay` → `CardGame.UI`; `TileScript`, `TilesManager` → `DefaultNamespace.Tiles`.

**A4. (DONE)** Replaced public fields with `[SerializeField] private` in `PostdictionManager` (6 fields), `TilesManager` (10 fields, kept `isActive` public), `TileScript` (3 fields), `CardBoard` (`scorer`, `neverGlow`; kept `freeze` public).

### Phase B: Medium Impact (DONE)

**B1. (DONE)** Extracted `ScoreCalculator` static class (`Assets/Scripts/Core/ScoreCalculator.cs`) with `CalculateScore()` and `EvaluateGoal()` methods. Updated `CardBoard.UpdateScore()`, `RoundManager.CalculateRoundScore()`, and `TutorialManager.IsSpreadCorrect()` to use it. Removed `CalculateScoreFromBoard()` from RoundManager.

**B2. (DONE)** Created `GameConfig` ScriptableObject (`Assets/Scripts/Core/GameConfig.cs`) with all game settings (goal range, cards per round, deal delay, result display time, max rounds, max same suit, cat talk duration). RoundManager now uses a single `[SerializeField] private GameConfig config` field instead of 8 individual config fields. Asset must be created in Unity Editor via Create → CardGame → Game Config and assigned to RoundManager.

**B3. (DONE)** Optimized `CardCombinations.AllOrderedSubsets()`: added early exit when max possible score (all values × 2) < goal; skips size-0 subsets; reuses a single static `CardLayout` instance instead of allocating per permutation; replaced LINQ `.Sum()` with for-loop in inner loop.

**B4. (DONE)** Created `IAudioService` interface (`Assets/Scripts/Managers/IAudioService.cs`) with 13 public methods. `AudioManager` implements the interface; `Instance` typed as `IAudioService`. All 43 call sites across 9 consumer files unchanged.

**B5. (DONE)** `CardBoard.GetCards()` returns `IReadOnlyList<SimpleCard>` via cached `ReadOnlyCollection` (zero-allocation). `GetCardsData()` returns `IReadOnlyList<CardData>`. Updated all callers in `RoundManager` and `TutorialManager`.

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

**C4. (DONE)** Fixed naming inconsistencies across codebase + scene files:
- Renamed `CryLogic` → `CrystalDisplay` (class + file, preserved GUID)
- Fixed `Cristal` → `Crystal` in 5 sprite fields + 4 scene files (MainScene, TutorialScene, 2 recovery)
- Renamed `RulesCords` → `RulesCoords` enum (RulesPanel, RulesPanelSound, TutorialManager)
- PascalCased 14 methods: `setTexture`→`SetTexture`, `setVisibility`→`SetVisibility`, `clickOn`→`ClickOn`, `setHistoryVisibility`→`SetHistoryVisibility`, `changeSuccessSprites`→`ChangeSuccessSprites`, `setIndex`→`SetIndex`, `setFailerColor`→`SetFailColor`, `clearSelection`→`ClearSelection`, `select`→`Select`, `backToGame`→`BackToGame`, `makePostdiction`→`MakePostdiction`, `returnToMainMenu`→`ReturnToMainMenu`, `returnToGame`→`ReturnToGame`
- PascalCased 8 button callbacks + updated 7 scene `m_MethodName` refs: `onPlayButtonClick`→`OnPlayButtonClick`, `onTutorialButtonClick`→`OnTutorialButtonClick`, `onRulesButtonClick`→`OnRulesButtonClick`, `onCreatorsButtonClick`→`OnCreatorsButtonClick`, `onMainMenuClick`→`OnMainMenuClick`, `onContinueClick`→`OnContinueClick`, `exit`→`Exit` (CreatorsManager + RulesManager)

## Localization Implementation

### Architecture (ESTABLISHED — do not change)

All localization uses Unity's built-in **`LocalizeStringEvent`** component placed on the **same GameObject as the TMP component**. It fires `set_text` on locale change. Do NOT use `LocalizationSettings.StringDatabase.GetLocalizedString()` in manager scripts — the component approach is already wired in all scenes.

**LocalizeStringEvent script GUID:** `56eb0353ae6e5124bb35b17aff880f16`

**Table reference format in scene YAML:**
```yaml
m_TableCollectionName: GUID:<shared_data_guid>
m_KeyId: 100000000001   # long ID from shared data asset
```

**Tables and their shared data GUIDs:**
| Table | Shared Data GUID |
|-------|-----------------|
| PostdictionScene | `c0d1e2f3a4b5c6d7e8f9a0b1c2d3e4f5` |
| GameMenu | `a4b5c6d7e8f9a0b1c2d3e4f5a6b7c8d9` |
| EndingScenes | `e8f9a0b1c2d3e4f5a6b7c8d9e0f1a2b3` |
| TutorialScene | `e6f7a8b9c0d1e2f3a4b5c6d7e8f9a0b1` |
| MainScene | (check `MainScene Shared Data.asset`) |
| MainMenu | (check `MainMenu Shared Data.asset`) |

### What Is Done

- ✅ All `_en.asset` files filled: PostdictionScene (3), GameMenu (5), EndingScenes (5), TutorialScene (22)
- ✅ `TutorialScene.asset` collection + `TutorialScene_en.asset` + `.meta` files created
- ✅ PostdictionScene.unity — 3 TMP objects wired (IDs 2100000001–2100000003)
- ✅ GameMenu.unity — 5 TMP objects wired (IDs 2100000001–2100000005)
- ✅ All 4 EndingScenes — narrative + button text wired (IDs 2100000001–2100000002)
- ✅ TutorialScene.unity — 21 bubble TMP objects wired (IDs 2200000001–2200000022, skip 2200000016)
- ✅ TutorialScene.unity — 11 rules panel TMP objects wired (IDs 2200000016, 2200000030–2200000039)
- ✅ TutorialScene table extended with 10 new keys (rules_01–rules_10, IDs 100000000023–100000000032)
- ✅ tutor_16 ("скорая встреча") now wired — LSE ID 2200000016, GO 1259542633, key 100000000016
- ✅ `Assets/Scripts/UI/LocalizedText.cs` created (available for one-off use if needed)

### Remaining Work — TutorialManager goalSuitText

`TutorialManager.cs` line ~678: `goalSuitText.text = tutorialGoalSuit.ToString()` — outputs the C# enum name (e.g., "Coins"). This is not a localized string but may need translating if suit names should appear in the target language. Low priority; assess during testing.

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
