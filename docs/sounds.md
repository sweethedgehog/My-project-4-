# Sound Documentation

Complete reference of all audio files, their usage in code/scenes, and the playback architecture.

---

## Audio Architecture Overview

All audio playback is centralized through **`AudioManager`** (singleton, `DontDestroyOnLoad`), which holds exactly **2 AudioSources**:

| AudioSource | Mixer Group | Purpose |
|-------------|-------------|---------|
| `musicSource` | Music | Looping background music |
| `sfxSource` | SFX | One-shot sound effects via `PlayOneShot()` |

Volume is controlled through Unity's **AudioMixer** (`Assets/Audio/MainMixer`) with two exposed parameters: `MusicVolume` and `SFXVolume`. No manual volume multiplication in code.

**Key rule:** Sound components (`CardSound`, `UISound`, `RulesPanelSound`) own their `AudioClip` references but do NOT own AudioSources. They all route through `AudioManager.Instance.PlaySFX(clip)`.

---

## Sound Components

### AudioManager (`Assets/Scripts/Managers/AudioManager.cs`)

Central singleton. Holds 3 music clips assigned via Inspector:

| Field | Clip (assigned in Inspector) | Called by |
|-------|------------------------------|----------|
| `menuMusic` | `ambience/575284__trp__various-wind-chimes-inside-store-09.mp3` (placeholder) | `TutorialManager.Start()` via `PlayMenuMusic()` |
| `gameplayMusic` | Same file as `menuMusic` (placeholder — needs a distinct track) | `RoundManager.Start()` via `PlayGameplayMusic()` |
| `victoryMusic` | **Not assigned** (null) | **Not called anywhere** (defined but unused in code) |

### CardSound (`Assets/Scripts/UI/CardSound.cs`)

Attached to each card prefab (`Card.prefab`, `CardNew.prefab`). Uses `OnMouse*` callbacks (requires `BoxCollider2D`).

| Field | Trigger | Description |
|-------|---------|-------------|
| `cardHoverEnterSound` | `OnMouseEnter()` | Mouse hovers over a card |
| `cardClickSound` | `OnMouseDown()` | Mouse clicks on a card |
| `cardPickupSound` | `PlayPickup()` (called from `SimpleDraggableWithBoard`) | **Not assigned** (null) — needs a clip |
| `cardDropSound` | `PlayDrop()` (called from `SimpleDraggableWithBoard`) | Card drag ends |

### UISound (`Assets/Scripts/UI/UISound.cs`)

Attached to UI buttons (Canvas-based). Uses EventSystem interfaces (`IPointerEnterHandler`, `IPointerClickHandler`). Requires a `Selectable` component on the same GameObject.

| Field | Trigger | Description |
|-------|---------|-------------|
| `hoverEnterSound` | `OnPointerEnter()` | **Not assigned** (null) — needs a clip |
| `clickSound` | `OnPointerClick()` | **Not assigned** (null) — needs a clip |

### RulesPanelSound (`Assets/Scripts/UI/RulesPanelSound.cs`)

Attached alongside `RulesPanel`. Tracks open/closed state to avoid duplicate sounds.

| Field | Trigger | Description |
|-------|---------|-------------|
| `panelOpenSound` | `PlaySoundForState(RulesCoords.Open)` | Rules scroll opens |
| `panelCloseSound` | `PlaySoundForState(RulesCoords.Closed)` | Rules scroll closes |

### CardScorer (`Assets/Scripts/Score/CardScorer.cs`)

Plays goal-completion sounds when the player's card arrangement meets value/suit targets.

| Field | Trigger | Description |
|-------|---------|-------------|
| `valueGoalCompleteSound` | Value goal reached (exact match) | Celebratory chime for hitting the target number |
| `roseSuitCompleteSound` | Suit goal = Roses matched | Suit-specific activation sound |
| `crownSuitCompleteSound` | Suit goal = Crowns matched | Suit-specific activation sound |
| `skullSuitCompleteSound` | Suit goal = Skulls matched | Suit-specific activation sound |
| `coinSuitCompleteSound` | Suit goal = Coins matched | Suit-specific activation sound |

When both goals complete simultaneously, value sound plays first, then suit sound after a `dualGoalDelay` (0.5s).

### TilesManager (`Assets/Scripts/Tiles/TilesManager.cs`)

Plays round-result sounds in `SetVisibility()`.

| Field | Trigger | Description |
|-------|---------|-------------|
| `audioClipFail` | `SuccessCodes.Failer` | Round failed (no goals met) |
| `audioClipSuccess` | `SuccessCodes.Partial` | Partial success (value OR suit matched) |
| `audioClipFullSuccess` | `SuccessCodes.Success` | Full success (both goals matched) |

### RoundManager (`Assets/Scripts/Managers/RoundManager.cs`)

| Field | Trigger | Description |
|-------|---------|-------------|
| `cardsShuffle` | `DealCardsWithAnimation()` coroutine | Shuffle sound when dealing cards |

### TutorialManager (`Assets/Scripts/Managers/TutorialManager.cs`)

| Field | Trigger | Description |
|-------|---------|-------------|
| `cardDrawSound` | `DealTutorialCards()` coroutine | Sound when tutorial cards are dealt (assigned `card_shuffle.wav`) |
| `tutorialCompleteSound` | `Step17_FinalAdvice()` | **Not assigned** (null) — needs a clip |

### EndMenuManager (`Assets/Scripts/Managers/EndMenuManager.cs`)

| Field | Trigger | Description |
|-------|---------|-------------|
| `audioClip` | `Start()` | Plays automatically when ending scene loads (win or lose jingle) |

---

## Audio Files Inventory

All project audio files are in `Assets/Sounds/`. Music clips are assigned directly on the `AudioManager` GameObject in scenes (not stored under `Assets/Sounds/`).

### Card Sounds (`Assets/Sounds/card/`)

| File | Format | Used | Assigned to |
|------|--------|------|-------------|
| `card_click/card_click_ver1.wav` | WAV | Yes | `CardSound.cardClickSound` (Card.prefab, CardNew.prefab) |
| `card_hover/card_hover.wav` | WAV | Yes | `CardSound.cardHoverEnterSound` (Card.prefab, CardNew.prefab) |
| `card_placed/card_placed.wav` | WAV | Yes | `CardSound.cardDropSound` (Card.prefab, CardNew.prefab) |
| `card_shuffle/card_shuffle.wav` | WAV | Yes | `RoundManager.cardsShuffle`, `TutorialManager.cardDrawSound` (MainScene, TutorialScene) |

### Activation Sounds (`Assets/Sounds/activations/`)

These play when goal conditions are met (value match or suit match).

| File | Format | Used | Assigned to |
|------|--------|------|-------------|
| `value_match_ver2.wav` | WAV | Yes | `CardScorer.valueGoalCompleteSound` (MainScene, TutorialScene) |
| `suit_match_ver2.wav` | WAV | Yes | `CardScorer` suit complete sounds (MainScene, TutorialScene) |
| `coins_match/628548__gmlh__icemagic.mp3` | MP3 | **UNUSED** | Not referenced in any scene or prefab |
| `crowns_match/48223__slothrop__trumpetc2.wav` | WAV | **UNUSED** | Not referenced in any scene or prefab |
| `sculls_match/49190__angel_perez_grandi__ice-breaking.wav` | WAV | **UNUSED** | Not referenced in any scene or prefab |
| `rose_match/398448__brachern__match-ignite-no-strike.wav` | WAV | **UNUSED** | Not referenced in any scene or prefab |
| `suit_match_ver1.wav` | WAV | **UNUSED** | Replaced by `suit_match_ver2.wav` |
| `value_match_ver1.wav` | WAV | **UNUSED** | Replaced by `value_match_ver2.wav` |

> **Note:** The 4 individual suit-match sounds (`coins_match`, `crowns_match`, `sculls_match`, `rose_match`) appear to be early prototypes for per-suit activation sounds. The project currently uses a single `suit_match_ver2.wav` for all suit completions. `CardScorer` has separate fields per suit (`roseSuitCompleteSound`, `coinSuitCompleteSound`, etc.) but they are all assigned the same clip in scenes.

### Click / UI Sounds (`Assets/Sounds/clicks/`)

| File | Format | Used | Assigned to |
|------|--------|------|-------------|
| `endround_click.wav` | WAV | **UNUSED** | Not referenced in any active scene or prefab |
| `endround_hover.wav` | WAV | **UNUSED** (legacy only) | Only in `_Recovery/0 (2).unity` |
| `254286__jagadamba__mechanical-switch.wav` | WAV | **UNUSED** | Not referenced in any scene or prefab |

### Round Result Sounds (`Assets/Sounds/round_result/`)

| File | Format | Used | Assigned to |
|------|--------|------|-------------|
| `round_win_ver2.wav` | WAV | Yes | `TilesManager.audioClipFullSuccess` (MainScene, TutorialScene) |
| `round_semi_win_ver2.wav` | WAV | Yes | `TilesManager.audioClipSuccess` (MainScene, TutorialScene) |
| `round_lose_ver2.wav` | WAV | Yes | `TilesManager.audioClipFail` (MainScene, TutorialScene) |
| `round_win_ver1.mp3` | MP3 | **UNUSED** (legacy only) | Only in `Legacy/Win.unity`, `_Recovery/` scenes |
| `round_semi_win_ver1.mp3` | MP3 | **UNUSED** (legacy only) | Only in `_Recovery/` scenes |
| `round_lose_ver1.wav` | WAV | **UNUSED** (legacy only) | Only in `Legacy/Lose.unity`, `_Recovery/` scenes |

### Game Result Sounds (`Assets/Sounds/game_result/`)

| File | Format | Used | Assigned to |
|------|--------|------|-------------|
| `game_win_ver2.wav` | WAV | Yes | `EndMenuManager.audioClip` (BadgerEndingScene) |
| `game_lose_ver2.wav` | WAV | Yes | `EndMenuManager.audioClip` (CatEndingScene, RabbitEndingScene, SquirrelEndingScene) |

### Scroll / Rules Panel Sounds (`Assets/Sounds/scroll/`)

| File | Format | Used | Assigned to |
|------|--------|------|-------------|
| `scroll_open.mp3` | MP3 | Yes | `RulesPanelSound.panelOpenSound` (MainScene, TutorialScene) |
| `scroll_close.mp3` | MP3 | Yes | `RulesPanelSound.panelCloseSound` (MainScene, TutorialScene) |

### Ambience (`Assets/Sounds/ambience/`)

| File | Format | Used | Assigned to |
|------|--------|------|-------------|
| `575284__trp__various-wind-chimes-inside-store-09.mp3` | MP3 | Yes | Assigned as both `menuMusic` and `gameplayMusic` on AudioManager (placeholder — same track for both) |

---

## Unused Files Summary

The following **11 files** are not referenced in any active scene or prefab and can be safely removed:

### Completely unreferenced (safe to delete)

| # | File | Reason |
|---|------|--------|
| 1 | `activations/coins_match/628548__gmlh__icemagic.mp3` | Per-suit prototype, never wired |
| 2 | `activations/crowns_match/48223__slothrop__trumpetc2.wav` | Per-suit prototype, never wired |
| 3 | `activations/sculls_match/49190__angel_perez_grandi__ice-breaking.wav` | Per-suit prototype, never wired |
| 4 | `activations/rose_match/398448__brachern__match-ignite-no-strike.wav` | Per-suit prototype, never wired |
| 5 | `activations/suit_match_ver1.wav` | Replaced by ver2 |
| 6 | `activations/value_match_ver1.wav` | Replaced by ver2 |
| 7 | `clicks/254286__jagadamba__mechanical-switch.wav` | Never used |
| 8 | `clicks/endround_click.wav` | Never used |

### Referenced only in Recovery/Legacy scenes (safe to delete if those scenes are deprecated)

| # | File | Reason |
|---|------|--------|
| 9 | `clicks/endround_hover.wav` | Only in `_Recovery/0 (2).unity` |
| 10 | `round_result/round_win_ver1.mp3` | Only in Legacy/Win + Recovery scenes |
| 11 | `round_result/round_semi_win_ver1.mp3` | Only in Recovery scenes |
| 12 | `round_result/round_lose_ver1.wav` | Only in Legacy/Lose + Recovery scenes |

---

## Code Notes

### Unassigned sound slots (silent in-game)

5 `[SerializeField] AudioClip` fields exist in code but have no clip assigned in the Inspector — they fire silently due to null guards:

| Component | Field | Trigger |
|-----------|-------|---------|
| `CardSound` | `cardPickupSound` | Card drag start |
| `UISound` | `clickSound` | Button click |
| `UISound` | `hoverEnterSound` | Button hover |
| `TutorialManager` | `tutorialCompleteSound` | Tutorial end (step 17) |
| `AudioManager` | `victoryMusic` | Not assigned AND not called in code |

### `PlayVictoryMusic()` is never called

`AudioManager` defines `PlayVictoryMusic()` and holds a `victoryMusic` clip field, but no script in the project calls it. The ending scenes use `EndMenuManager.PlaySFX(audioClip)` for their win/lose jingles instead of switching the music track. Either:
- Wire `PlayVictoryMusic()` into the win ending scene for background music, or
- Remove the `victoryMusic` field and `PlayVictoryMusic()` method if it's not needed.

### Per-suit sounds are architecturally supported but not differentiated

`CardScorer` has 4 separate suit-sound fields (`roseSuitCompleteSound`, `crownSuitCompleteSound`, etc.) and a `GetSuitSound()` switch. In practice, all 4 fields are assigned the same `suit_match_ver2.wav`. The per-suit sounds in `Assets/Sounds/activations/` subfolders exist but were never wired. This could be a future enhancement.

---

## Sound Flow Diagram

```
Scene loads
  |
  +--> AudioManager.PlayGameplayMusic() / PlayMenuMusic()
  |       musicSource.Play() --> [Music MixerGroup] --> Master
  |
  +--> Player interacts with card
  |       CardSound --> AudioManager.PlaySFX(clip)
  |           sfxSource.PlayOneShot() --> [SFX MixerGroup] --> Master
  |
  +--> Player clicks UI button
  |       UISound --> AudioManager.PlaySFX(clip)
  |
  +--> Player opens/closes rules
  |       RulesPanelSound --> AudioManager.PlaySFX(clip)
  |
  +--> Deck deals cards
  |       RoundManager / TutorialManager --> AudioManager.PlaySFX(cardsShuffle)
  |
  +--> Score changes, goal met
  |       CardScorer --> AudioManager.PlaySFX(valueGoalCompleteSound / suitSound)
  |
  +--> Round ends
  |       TilesManager --> AudioManager.PlaySFX(fail/success/fullSuccess)
  |
  +--> Ending scene loads
          EndMenuManager --> AudioManager.PlaySFX(audioClip)
```

---

## How to Add New Sounds

See `docs/audio-architecture.md` section "How to Add New Sounds" for step-by-step instructions.

**Quick version:**
1. Add `[SerializeField] private AudioClip mySound;` to the triggering component
2. Assign the clip in Unity Inspector
3. Play with `AudioManager.Instance.PlaySFX(mySound);`
4. **Never** create local `AudioSource` components
