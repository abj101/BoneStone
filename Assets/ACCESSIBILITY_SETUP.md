# Settings & Accessibility – Simplified Plan

The game flow is intentionally simple:

- **In‑game**: players can **pause**, which only shows a basic pause overlay and lets them **return to the main menu**.
- **In the main menu**: players can **start the game**, open **Settings**, or **Exit**.
- **Settings** only contains:
  - **Audio** (master / music / SFX sliders)
  - **Controls** (keyboard/mouse + controller remapping)
  - **Accessibility** (**high contrast** toggle only).

---

## Visual style reference

| Element | Color |
|---|---|
| Panel background | `#0D0D12` |
| Tab bar background | `#131318` |
| Tab text (inactive) | `#888899` |
| Tab text (active) | `#FFFFFF` |
| Tab active indicator (bottom border) | `#C8B49A` |
| Buttons (normal) | transparent, white text |
| Buttons (highlighted) | `#2A2A38` background |
| Slider track | `#2A2A38` |
| Slider fill / handle | `#C8B49A` / `#FFFFFF` |
| Toggle checkmark | `#C8B49A` |
| Dropdown background | `#1A1A22` |

---

## 1. Build the SettingsPanel hierarchy (Main Menu)

Under the main menu `Canvas`, create the following hierarchy (set **SettingsPanel** inactive by default):

```
SettingsPanel               ← Image (#0D0D12), CanvasGroup
├── TabBar                  ← HorizontalLayoutGroup, Image (#131318)
│   ├── Tab_Audio           ← Button  →  child TextMeshProUGUI "AUDIO"
│   │   └── Indicator       ← Image (#C8B49A), height ~3px, set inactive
│   ├── Tab_Controls        ← Button  →  child TextMeshProUGUI "CONTROLS"
│   │   └── Indicator       ← Image (#C8B49A), height ~3px, set inactive
│   └── Tab_Accessibility   ← Button  →  child TextMeshProUGUI "ACCESSIBILITY"
│       └── Indicator       ← Image (#C8B49A), height ~3px, set inactive
├── CloseButton             ← Button  →  child TextMeshProUGUI "✕"  (anchor: top-right)
├── Content_Audio           ← VerticalLayoutGroup (active by default)
│   ├── Row: TextMeshProUGUI "MASTER"  +  Slider (wired to AudioSettings.SetMasterVolume)
│   ├── Row: TextMeshProUGUI "MUSIC"   +  Slider (wired to AudioSettings.SetMusicVolume)
│   └── Row: TextMeshProUGUI "SFX"    +  Slider (wired to AudioSettings.SetSfxVolume)
├── Content_Controls        ← ScrollRect → Viewport → Content (VerticalLayoutGroup)
│   ├── RebindRow × N       ← (one per action: Move, Look, Right Attack, Left Attack, Slow, Dash)
│   └── ResetButton         ← Button "Reset to defaults"
└── Content_Accessibility   ← VerticalLayoutGroup
    └── Row: Toggle (wired to AccessibilitySettingsUI) + TextMeshProUGUI "HIGH CONTRAST"
```

---

## 2. Wire SettingsTabController

Add **SettingsTabController** to the **SettingsPanel** root object and assign:

| Field | Assign |
|---|---|
| `_panel` | SettingsPanel (the root GameObject itself) |
| `_tabButtons [0]` | Tab_Audio Button |
| `_tabButtons [1]` | Tab_Controls Button |
| `_tabButtons [2]` | Tab_Accessibility Button |
| `_tabPanels [0]` | Content_Audio |
| `_tabPanels [1]` | Content_Controls |
| `_tabPanels [2]` | Content_Accessibility |
| `_tabIndicators [0]` | Tab_Audio / Indicator Image |
| `_tabIndicators [1]` | Tab_Controls / Indicator Image |
| `_tabIndicators [2]` | Tab_Accessibility / Indicator Image |
| `_closeButton` | CloseButton |

---

## 3. Wire AudioSettings

Add **AudioSettings** anywhere in the scene (or on the SettingsPanel root). Assign:
- `mainMixer` → your AudioMixer asset
- `masterSlider`, `musicSlider`, `SfxSlider` → the three sliders in Content_Audio
- Wire each slider's **OnValueChanged** → `AudioSettings.SetMasterVolume` / `SetMusicVolume` / `SetSfxVolume`

---

## 4. Wire AccessibilitySettingsUI

Add **AccessibilitySettingsUI** to the Content_Accessibility panel. Assign:
- `_highContrastToggle` → the Toggle in Content_Accessibility

Add **HighContrastUITheme** to both the SettingsPanel root and any other panels that should respond to the high-contrast toggle.

---

## 5. Wire ControlsRebindUI

Add **ControlsRebindUI** to the Content_Controls panel. Assign:
- `_actions` → InputSystem_Actions asset
- `_actionMapName` → `"Player"`
- `_rebindRows` → fill the array (one entry per action row)
  - `actionName` → `"Move"`, `"Look"`, `"RightTrigger"`, `"LeftTrigger"`, `"TimeSlow"`, `"Dash"`
  - per row, assign:
    - `actionLabel` → the row's action-name TextMeshProUGUI (e.g. "MOVE", "AIM", "RIGHT ATTACK", …)
    - keyboard/mouse binding label + rebind button
    - controller binding label + rebind button
- `_resetToDefaultsButton` → ResetButton

---

## 6. Main Menu scene

- Add **MainMenu** component to a root GameObject.
  - `_mainMenuPanel` → your main menu panel (Play / Settings / Exit buttons)
  - `_settingsTabController` → SettingsTabController on SettingsPanel
- Wire the **Settings** button's OnClick → `MainMenu.OpenSettings()`
- Wire the **Exit** button to quit the application.

---

## 7. Tutorial / Game scene – Pause → Main Menu

- Create a simple **PausePanel** (Canvas panel, inactive by default) containing:
  - **Resume** button
  - **Return to Main Menu** button
- Add a **PauseMenuController** (or equivalent script) that:
  - Listens to the Pause input action.
  - Toggles `PausePanel` on/off and pauses game time.
  - On **Return to Main Menu**, loads the `"Menu"` scene.

Players **do not** open Settings from in‑game; they always return to the main menu first, then open Settings there.

---

## 8. High contrast – optional in‑world volume

If you want high contrast to also affect the world:

- Add a **Global Volume** to the Tutorial / Game scene with a **Color Adjustments** override (Contrast ~+30).
- Add an empty GameObject with **HighContrastVolumeBridge** and assign this Volume so it responds to the same high‑contrast toggle.

---

## Verification checklist

- [ ] From the game, pressing Pause opens a simple pause overlay; **Return to Main Menu** loads the Menu scene.
- [ ] From the main menu, **Settings** opens the tabbed panel; tabs switch correctly; ✕ returns to the main menu.
- [ ] Sliders adjust audio in real time and settings persist across restart.
- [ ] High contrast toggle switches UI theme (and world volume if configured).
- [ ] Rebind rows display current bindings for Move / Look / Right Attack / Left Attack / Slow / Dash; clicking Rebind captures new input; Reset restores defaults.
- [ ] Localization / language options are **not** exposed in the UI.
