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
`AudioManager` is a singleton for global audio access. Music and SFX have separate volume controls.

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

## Code Cleanup - Refactoring Plan (in progress)

**Phase 1 (DONE):** Removed dead code - Legacy folder (Card.cs, CardSequence.cs, DeckManager.cs, GameplayManager.cs), empty DeckGameObject stub, unused `System.Collections` import in CardBoard.cs.

**Phase 2 (DONE - already fixed):** Bugs from review were already resolved in prior work. CardScorer's `GetSuitColor()` was removed, RulesPanel's float comparison uses threshold now.

**Phase 3 (DONE):** Fixed typos - `rightChoise`/`lastChoise` -> `rightChoice`/`lastChoice` (PostdictionManager), `Patrial` -> `Partial` (SuccessCodes + all refs), `setVisability`/`setHistoryVisability` -> `setVisibility`/`setHistoryVisibility` (TileScript, TilesManager). `rulsePanel` was already fixed.

**Phase 4 (DONE - already resolved):** `GetSuitColor()` duplication no longer exists. Only RoundManager has it now; SimpleCard/CryLogic use sprites not colors; CardScorer's copy was removed in prior work.

**Phase 5: Naming & Readability**
- Extract magic numbers to named constants (RoundManager, SimpleDeckObject)
- Add missing explicit access modifiers (RulesPanel, TilesManager)
- Remove stray `Debug.Log()` calls

**Phase 6: Reduce RoundManager Complexity**
- RoundManager.cs is ~604 lines with too many responsibilities
- Extract goal generation, score UI updates into helper methods
- Add summary comments to major methods
