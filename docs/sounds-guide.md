# Sound Guide

All sounds in the game, when they play, and which files are unused.

---

## Music

| Track | When it plays | Status |
|-------|---------------|--------|
| **Menu music** | Main menu + tutorial | Placeholder — uses wind chimes ambience |
| **Gameplay music** | During all 6 rounds | Placeholder — same wind chimes as menu |
| **Victory music** | — | Not assigned, not called in code |

Both menu and gameplay music currently point to the same file: `ambience/575284__trp__various-wind-chimes-inside-store-09.mp3`. The system supports separate tracks — they just need to be provided and assigned.

---

## Sound Effects

### Card Interaction

| Sound | File | When it plays |
|-------|------|---------------|
| Card hover | `card/card_hover/card_hover.wav` | Mouse over a card |
| Card click | `card/card_click/card_click_ver1.wav` | Mouse down on a card |
| Card pickup | **Not assigned** — needs a clip | Card begins being dragged |
| Card drop | `card/card_placed/card_placed.wav` | Card placed on the board |
| Card shuffle | `card/card_shuffle/card_shuffle.wav` | Cards are dealt at the start of a round |

### Goal Completion

| Sound | File | When it plays |
|-------|------|---------------|
| Value goal reached | `activations/value_match_ver2.wav` | Card score matches target number |
| Suit goal reached | `activations/suit_match_ver2.wav` | Dominant suit matches target suit |

If both goals are met at the same time, value sound plays first, suit sound follows after 0.5s.

### Round Results

| Sound | File | When it plays |
|-------|------|---------------|
| Full success | `round_result/round_win_ver2.wav` | Both goals matched |
| Partial success | `round_result/round_semi_win_ver2.wav` | One goal matched |
| Failure | `round_result/round_lose_ver2.wav` | No goals matched |

### Game Ending

| Sound | File | When it plays |
|-------|------|---------------|
| Game win | `game_result/game_win_ver2.wav` | Correct accusation (BadgerEndingScene) |
| Game lose | `game_result/game_lose_ver2.wav` | Wrong accusation (Cat/Rabbit/Squirrel endings) |

### UI

| Sound | File | When it plays |
|-------|------|---------------|
| Button hover | **Not assigned** — needs a clip | Mouse over a button |
| Button click | **Not assigned** — needs a clip | Button pressed |
| Rules scroll open | `scroll/scroll_open.mp3` | Rules panel opens |
| Rules scroll close | `scroll/scroll_close.mp3` | Rules panel closes |

### Other

| Sound | File | When it plays |
|-------|------|---------------|
| Wind chimes | `ambience/575284__trp__various-wind-chimes-inside-store-09.mp3` | Main menu |
| Tutorial complete | **Not assigned** — needs a clip | End of tutorial |

---

## Unused Files

### Replaced by newer versions

| File | Replaced by |
|------|-------------|
| `activations/suit_match_ver1.wav` | `suit_match_ver2.wav` |
| `activations/value_match_ver1.wav` | `value_match_ver2.wav` |
| `round_result/round_win_ver1.mp3` | `round_win_ver2.wav` |
| `round_result/round_semi_win_ver1.mp3` | `round_semi_win_ver2.wav` |
| `round_result/round_lose_ver1.wav` | `round_lose_ver2.wav` |

### Never connected

| File | Was meant for |
|------|---------------|
| `activations/coins_match/628548__gmlh__icemagic.mp3` | Per-suit activation for Coins |
| `activations/crowns_match/48223__slothrop__trumpetc2.wav` | Per-suit activation for Crowns |
| `activations/sculls_match/49190__angel_perez_grandi__ice-breaking.wav` | Per-suit activation for Skulls |
| `activations/rose_match/398448__brachern__match-ignite-no-strike.wav` | Per-suit activation for Roses |
| `clicks/254286__jagadamba__mechanical-switch.wav` | Alternative click sound |
| `clicks/endround_click.wav` | End-round button click |
| `clicks/endround_hover.wav` | End-round button hover |
