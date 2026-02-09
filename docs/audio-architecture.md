# Audio Architecture: Current State vs Best Practice

## What we have now (the chaos)

Audio is split across **4 scripts + every caller**, and they fight over responsibilities:

```
AudioManager (singleton)
├── Stores SOME clips (cardDrawSound, menuMusic, etc.)
├── Has 2 AudioSources (music + sfx)
├── Has global volume (musicVolume, sfxVolume)
├── Has 10 per-category volume multipliers (cardShuffleVolume, uiClickVolume, etc.)
└── Provides 20+ Play methods

CardSound (on every card - 3 AudioSources each!)
├── Stores ITS OWN clips (hover, click, drag, drop)
├── Has ITS OWN volume settings
├── Creates 3 AudioSources in Awake
└── Sometimes uses AudioManager, sometimes plays locally

UISound (on every button - 2 AudioSources each)
├── Stores ITS OWN clips
├── Has ITS OWN volume settings
├── Creates 2 AudioSources in Awake
└── Same mixed pattern

RulesPanelSound (2 AudioSources)
├── Stores ITS OWN clips
├── Has ITS OWN volume settings
├── Polls RulesPanel.IsMoving every frame in Update()
└── Same mixed pattern

CardScorer (1 AudioSource - never used!)
├── Stores 5 AudioClips for suit completion sounds
├── Creates an AudioSource it never uses
└── Routes everything through AudioManager anyway

TilesManager
├── Stores 3 AudioClips
└── Routes through AudioManager

RoundManager
├── Stores 1 AudioClip (shuffle)
└── Routes through AudioManager
```

### The main problems

1. **AudioClips are scattered across 7+ components.** To find "where is the card drop sound?", you have to hunt through inspectors on different GameObjects.

2. **Volume is tripled.** Final volume = `sfxSource.volume` x `categoryVolume` (AudioManager) x `localVolume` (CardSound/UISound). Impossible to tune consistently.

3. **AudioSources multiply wildly.** Deal 5 cards = 15 new AudioSources (3 per CardSound). But most sounds use `AudioManager.PlaySFX()` anyway, so those local sources sit idle except for loops.

4. **CardScorer creates an AudioSource it never uses** (line 54) - it routes everything through AudioManager.

5. **No AudioMixer** - all mixing is done manually in code, which Unity already solves natively.

---

## How it should look

For a 2D card game like ours, the standard Unity approach has 3 layers:

### Layer 1: AudioMixer (Unity built-in, no code needed)

Create in Unity Editor: `Assets/Audio/MainMixer`

```
Master
├── Music          (exposed parameter: "MusicVolume")
└── SFX            (exposed parameter: "SFXVolume")
    ├── UI         (buttons, panels)
    ├── Cards      (hover, drag, drop, place)
    └── Gameplay   (round results, goal completion, shuffle)
```

Volume control becomes one line: `mixer.SetFloat("MusicVolume", dB)`. No manual multiplication. Muting a whole category = one call. Players can adjust Music vs SFX independently, and sub-categories (UI, Cards, Gameplay) inherit from SFX automatically.

### Layer 2: AudioManager (thin singleton)

```csharp
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [SerializeField] private AudioMixer mainMixer;
    [SerializeField] private AudioMixerGroup musicGroup;
    [SerializeField] private AudioMixerGroup sfxGroup;

    private AudioSource musicSource;
    private AudioSource sfxSource;  // PlayOneShot handles overlapping sounds

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SetupAudioSources();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void SetupAudioSources()
    {
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.outputAudioMixerGroup = musicGroup;
        musicSource.loop = true;
        musicSource.playOnAwake = false;

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.outputAudioMixerGroup = sfxGroup;
        sfxSource.playOnAwake = false;
    }

    public void PlayMusic(AudioClip clip)
    {
        if (clip == null || musicSource.clip == clip) return;
        musicSource.clip = clip;
        musicSource.Play();
    }

    public void StopMusic() => musicSource.Stop();

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;
        sfxSource.PlayOneShot(clip);
    }

    public void SetMusicVolume(float normalized)
    {
        // Convert 0-1 to decibels (-80 to 0)
        float dB = normalized > 0.001f ? Mathf.Log10(normalized) * 20f : -80f;
        mainMixer.SetFloat("MusicVolume", dB);
    }

    public void SetSFXVolume(float normalized)
    {
        float dB = normalized > 0.001f ? Mathf.Log10(normalized) * 20f : -80f;
        mainMixer.SetFloat("SFXVolume", dB);
    }
}
```

That's it. ~60 lines. No per-category volume multipliers (AudioMixer handles that). No 20 specialized Play methods. No clip storage - callers own their clips.

### Layer 3: Sound components (on GameObjects, own their clips)

```csharp
// Simple - no AudioSources created, no volume management
public class CardSound : MonoBehaviour, IPointerEnterHandler, IEndDragHandler
{
    [SerializeField] private AudioClip hoverSound;
    [SerializeField] private AudioClip dropSound;

    public void OnPointerEnter(PointerEventData e)
    {
        AudioManager.Instance.PlaySFX(hoverSound);
    }

    public void OnEndDrag(PointerEventData e)
    {
        AudioManager.Instance.PlaySFX(dropSound);
    }
}
```

No local AudioSources. No local volume fields. No loop management. Just "play this clip".

### What about looping sounds?

For looping sounds (the one case where you might need a local AudioSource), the AudioManager can provide a helper:

```csharp
// In AudioManager:
public AudioSource CreateLoopSource(AudioMixerGroup group)
{
    var source = gameObject.AddComponent<AudioSource>();
    source.outputAudioMixerGroup = group;
    source.loop = true;
    source.playOnAwake = false;
    return source;
}
```

But in our game, do we actually need looping hover/drag sounds on cards? That's unusual for a card game. A simple one-shot on hover-enter and on drop is typically enough.

---

## Summary: current vs ideal

| Aspect | Current | Should be |
|---|---|---|
| Where clips live | Scattered across 7+ components | On the component that triggers them |
| Volume control | 3 layers of manual multiplication | AudioMixer handles it all |
| AudioSources | Created on every card/button (20+) | 2 total (music + sfx) on AudioManager |
| AudioManager size | 270 lines, 20+ methods | ~60 lines, 4-5 methods |
| To change volume | Find the right multiplier in 3 places | One slider on the mixer group |
| Sub-category mixing | Manual code per category | Mixer groups with automatic inheritance |

---

## Refactoring Plan (ALL STEPS DONE)

### Step 1: Create AudioMixer (DONE)
- Right-click in Project > Create > Audio Mixer, save as `Assets/Audio/MainMixer`
- Add child groups under Master:
  - `Music` - expose parameter as "MusicVolume"
  - `SFX` - expose parameter as "SFXVolume"
  - Under SFX, optionally add: `UI`, `Cards`, `Gameplay`
- To expose a parameter: click a group, right-click the Volume slider > "Expose to script", rename it

### Step 2: Rewrite AudioManager.cs (DONE)
- Delete all 10 per-category volume multipliers (`cardShuffleVolume`, `uiClickVolume`, etc.)
- Delete all 15+ category-specific Play methods (`PlayCardShuffle`, `PlayUIClick`, etc.)
- Delete stored clips (`cardDrawSound`, `buttonClickSound`, etc.) - callers own their clips
- Add `[SerializeField] AudioMixer mainMixer` and mixer group references
- Route `musicSource` to Music group, `sfxSource` to SFX group
- Replace `SetMusicVolume`/`SetSFXVolume` with mixer parameter calls
- Result: ~60 lines, 4-5 public methods

### Step 3: Simplify CardSound.cs (DONE)
- Delete 3 local AudioSources (`clickAudioSource`, `hoverLoopSource`, `dragLoopSource`)
- Delete all local volume fields (`clickVolume`, `hoverEnterVolume`, etc.)
- Delete `hoverLoopSound` and `dragLoopSound` (looping sounds on cards are unnecessary)
- Keep clip fields: `hoverSound`, `clickSound`, `pickupSound`, `dropSound`
- Each handler becomes one line: `AudioManager.Instance.PlaySFX(clip)`
- Result: ~30 lines

### Step 4: Simplify UISound.cs (DONE)
- Delete 2 local AudioSources
- Delete local volume fields
- Delete `hoverLoopSound` (one-shot hover enter is enough)
- Keep: `clickSound`, `hoverEnterSound`
- Result: ~25 lines

### Step 5: Simplify RulesPanelSound.cs (DONE)
- Delete 2 local AudioSources
- Delete `Update()` polling loop (was checking `IsMoving` every frame)
- Delete `slideLoopSound` and volume fields
- Keep: `openSound`, `closeSound`, `PlaySoundForState()` method
- Result: ~30 lines

### Step 6: Clean up callers (DONE)
- `CardScorer.cs`: delete unused AudioSource creation in Awake (line 54-57)
  - Keep clip fields and `AudioManager.Instance.PlaySFX()` calls (already correct pattern)
- `TilesManager.cs`: already clean, no changes needed
- `RoundManager.cs`: change `AudioManager.Instance.PlayCardShuffle(clip)` to `AudioManager.Instance.PlaySFX(clip)`
  - Same for any other category-specific calls that get removed

### Step 7: Wire up in Unity Editor (DONE)
- Select the AudioManager GameObject in scene
- Assign the MainMixer asset and mixer group references in inspector
- Verify all AudioClip fields on prefabs/scene objects are still assigned
  (simplified scripts keep the same clip field names where possible, so Unity preserves assignments)

### Step 8: Playtest all scenes (DONE)
Test sounds in each scene - no compile errors if something breaks, just silence:
- MainMenu: menu music, button clicks
- MainScene: shuffle, card hover/drag/drop, goal completion, round results
- TutorialScene: same as MainScene + tutorial sounds
- PostdictionScene: selection sounds
- Ending scenes: victory/lose music
- GameMenu: pause, button clicks
- Rules panel: open/close sounds

### Effort estimate

| Part | Effort | Who |
|---|---|---|
| Rewrite .cs files (steps 2-6) | ~1 hour | Claude |
| Create AudioMixer (step 1) | ~10 min | You in Unity Editor |
| Wire mixer groups (step 7) | ~5 min | You in Unity Editor |
| Verify sounds work (step 8) | ~20 min | You playtesting |

### Risk notes
- Audio breaks are silent (no errors, just missing sounds) - playtesting is essential
- If a clip field gets renamed, Unity loses the inspector assignment. Steps 3-5 should keep field names where possible to avoid this
- DontDestroyOnLoad on AudioManager means it persists across scenes - only one instance ever exists

---

## How to Add New Sounds

### Adding a sound effect to an existing component

1. Add a clip field to the component that triggers the sound:
   ```csharp
   [SerializeField] private AudioClip myNewSound;
   ```
2. Play it via AudioManager:
   ```csharp
   if (AudioManager.Instance != null)
       AudioManager.Instance.PlaySFX(myNewSound);
   ```
3. In Unity Inspector, select the GameObject and drag your `.wav`/`.ogg` clip onto the field.

### Adding a sound effect to a new component

1. Create your MonoBehaviour script. Do NOT add any `AudioSource` to it.
2. Add `[SerializeField] private AudioClip` fields for each sound.
3. Call `AudioManager.Instance.PlaySFX(clip)` to play them.
4. Example:
   ```csharp
   using UnityEngine;
   using CardGame.Managers;

   public class MyNewComponent : MonoBehaviour
   {
       [SerializeField] private AudioClip activateSound;
       [SerializeField] private AudioClip deactivateSound;

       public void Activate()
       {
           if (AudioManager.Instance != null)
               AudioManager.Instance.PlaySFX(activateSound);
       }

       public void Deactivate()
       {
           if (AudioManager.Instance != null)
               AudioManager.Instance.PlaySFX(deactivateSound);
       }
   }
   ```

### Adding new music tracks

1. Add the clip field to `AudioManager.cs`:
   ```csharp
   [SerializeField] private AudioClip myNewMusic;
   ```
2. Add a convenience method:
   ```csharp
   public void PlayMyNewMusic() => PlayMusic(myNewMusic);
   ```
3. Assign the clip on the AudioManager GameObject in the inspector.
4. Call from anywhere: `AudioManager.Instance.PlayMyNewMusic();`

### Adding a new mixer sub-group (e.g. for UI sounds)

1. In the Audio Mixer window, right-click `SFX` > Add child group > name it (e.g. `UI`).
2. In `AudioManager.cs`, add a field:
   ```csharp
   [SerializeField] private AudioMixerGroup uiGroup;
   ```
3. Create a dedicated AudioSource for it in `SetupAudioSources()`:
   ```csharp
   uiSource = gameObject.AddComponent<AudioSource>();
   uiSource.outputAudioMixerGroup = uiGroup;
   uiSource.playOnAwake = false;
   ```
4. Add a play method:
   ```csharp
   public void PlayUI(AudioClip clip)
   {
       if (clip == null) return;
       uiSource.PlayOneShot(clip);
   }
   ```
5. Wire the mixer group in the Inspector on the AudioManager GameObject.

### What NOT to do

- Do NOT create `AudioSource` components on cards, buttons, or other GameObjects
- Do NOT add per-sound volume multiplier fields — use AudioMixer groups for category volume
- Do NOT store SFX clips on AudioManager — clips belong on the component that plays them
- Do NOT use `AudioSource.Play()` directly — always go through `AudioManager.Instance.PlaySFX()`
