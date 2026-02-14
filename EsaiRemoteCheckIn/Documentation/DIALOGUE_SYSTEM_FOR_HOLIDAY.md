# Esai Dialogue System — Writer's Guide for Holiday

Hi Holiday. This document explains how Esai's dialogue system works so you can write lines that fit the structure, and make notes about mood and intensity for Seraphine as you go.

---

## How the System Works

### Text Keys and Lines.json

All of Esai's spoken lines live in **`Assets/Resources/Dialogue/Lines.json`**. Each line has a **text key** (e.g. `reassure.general`, `grounding.exercise`). Nodes reference these keys; the game looks up the key and displays the text. You add or edit lines by adding/editing entries in that JSON file.

**Format for a line entry:**
```json
{
  "key": "branch.context",
  "variants": [
    { "text": "Your line here.", "weight": 1 },
    { "text": "Alternative variant.", "weight": 1 }
  ],
  "rules": { "noRepeatWindow": 5 }
}
```

- **key** — Unique identifier. Use dot notation: `branch.context` (e.g. `reassure.general`, `grounding.failed.no`).
- **variants** — One or more possible lines. The game picks one (weighted randomly). Add variants for variety; the same key can have many.
- **rules.noRepeatWindow** — Optional. If set (e.g. 5), the game avoids repeating the same variant for the last N picks. Good for reassurance and other repeatable lines.

### Node Structure

Each dialogue "screen" is a **node**. A node has:
- A **node ID** (e.g. `reassurance_response`, `grounding_exercise_2`)
- A **text key** that pulls the line from Lines.json
- **Options** (buttons the player can tap)

Some nodes use **context** — the same node can show different text depending on how the player got there. For example, `grounding_exercise_2` shows different text when the player came from "No" vs "I don't know."

### Module

The main check-in flow lives in a **module** called **CheckInModule** (module ID: `checkin`). It contains all nodes for the root, support, reassurance, grounding, and hub flows.

---

## Branch Outlines

### Reassurance Branch

**Intent:** Offer gentle validation and permission to rest. Esai affirms that the player doesn't have to fix everything, doesn't need to earn rest, and is allowed to be still. Short, warm, low-pressure.

| Node ID | Text Key | What Esai Says | Next Options |
|---------|----------|----------------|--------------|
| `reassurance_response` | `reassure.general` | Reassurance line (variants) | Thanks → hub_checkin |

**Flow:** support → reassurance_response → hub_checkin

**Text keys to write for:**
- `reassure.general` — The main reassurance line. Supports multiple variants; use `noRepeatWindow: 5` so the same line doesn't repeat too often.

**Possible next steps (for Seraphine):**
- Add more reassurance sub-branches (e.g. "I don't believe it" / "I feel guilty")
- Add a "more" option for another reassurance line before returning to hub
- Expand variants significantly (50+ lines as per the plan)

---

### Grounding Branch

**Intent:** Help the player feel more present in their body. Starts with breath/feet, then 5-4-3-2-1 or similar if the first exercise doesn't land. Handles "no" and "I don't know" gently. If grounding isn't working, validate the effort and offer to try something different.

| Node ID | Text Key | Context | What Esai Says | Next Options |
|---------|----------|---------|----------------|--------------|
| `grounding_exercise` | `grounding.exercise` | — | Breath + feet on floor | Continue → grounding_followup_1 |
| `grounding_followup_1` | `grounding.followup.question` | — | "Do you feel a bit more grounded?" | Yes → hub_checkin; No → grounding_exercise_2 (context: from_no); I don't know → grounding_exercise_2 (context: from_dontknow) |
| `grounding_exercise_2` | `grounding.exercise2.from_no` | from_no | "Name five things you can see" (5-4-3-2-1 style) | Continue → grounding_followup_2 |
| `grounding_exercise_2` | `grounding.exercise2.from_dontknow` | from_dontknow | "Are you more in your head or in your body?" | Continue → grounding_followup_2 |
| `grounding_followup_2` | `grounding.followup.question` | — | "Do you feel a bit more grounded?" (again) | Yes → hub_checkin; No → grounding_failed_response_no; I don't know → grounding_failed_response_dontknow |
| `grounding_failed_response_no` | `grounding.failed.no` | — | Validate that it takes time; they're still trying | Okay → follow_up |
| `grounding_failed_response_dontknow` | `grounding.failed.dontknow` | — | Maybe grounding isn't right; offer something different | Okay → redirect_prelude |
| `follow_up` | `follow_up.prompt` | — | "Where would you like to go from here?" | Okay → hub_checkin |
| `redirect_prelude` | `redirect.prelude` | — | "Let's try something completely different." | Okay → hub_redirect |

**Flow:** support → grounding_exercise → grounding_followup_1 → (if No/I don't know) grounding_exercise_2 → grounding_followup_2 → (if Yes) hub_checkin, or (if No/I don't know again) grounding_failed_response_no / grounding_failed_response_dontknow → follow_up / redirect_prelude → hub

**Text keys to write for:**
- `grounding.exercise` — First exercise (breath, feet)
- `grounding.followup.question` — "Do you feel a bit more grounded?"
- `grounding.exercise2.from_no` — Second exercise when they said "No" (e.g. 5 things you can see)
- `grounding.exercise2.from_dontknow` — When they said "I don't know" — help narrow down (head vs body)
- `grounding.failed.no` — Validation when grounding didn't work (they said No twice)
- `grounding.failed.dontknow` — When grounding doesn't fit — offer to try something different

**Possible next steps (for Seraphine):**
- Add more grounding exercise variants (different 5-4-3-2-1 prompts)
- Branch from `grounding.exercise2.from_dontknow` for head vs body paths
- Add a "still not grounded" path with a third exercise or different modality

---

### Other Branches (Brief)

| Branch | Entry Node | Text Keys | Intent |
|--------|------------|-----------|--------|
| Root | `root` | `root.greeting` | Opening; "How are you, really?" |
| Okay | `okay` | `okay.prompt`, `okay.keep_light`, `okay.quick_checkin` | Player is okay; offer light check-in or support |
| Support | `support` | `support.prompt`, `support.one_step` | Choose support type |
| Something | `something` | `something.prompt`, `something.conflict`, `something.bad_news`, `something.overwhelm` | Something happened; narrow down |
| Don't Know | `dontknow` | `dontknow.prompt`, `dontknow.body`, `dontknow.emotion`, `dontknow.empty` | Player unsure; narrow down (body/emotion/empty) |
| Hub | `hub_checkin`, `hub_redirect` | `hub.checkin` | "Where next?" — check in again, end session, or something else |
| Session Close | `session_close` | `session_close` | Goodbye; triggers end overlay |

---

## Mood and Intensity System

When you write a line, you can suggest a **mood** and **intensity** for the portrait. Seraphine (or the system) maps these to Esai's portrait so the art matches the tone.

### Mood (PortraitMood)

| Mood | Use when |
|------|----------|
| Neutral | Default, calm, even |
| Friendly | Warm, welcoming, gentle |
| Concerned | Worried about the player, caring |
| Firm | Steady, clear, "you can do this" |
| Sad | Sharing sadness, empathy |
| Shocked | Surprised, taken aback |
| Devastated | Deep distress (use sparingly) |
| Warm | Soft, nurturing |
| Amused | Light, gentle humor |
| Embarrassed | Shy, self-conscious |
| Excited | Eager, upbeat |
| Surprised | Mild surprise |

### Intensity (0–4)

- **0** — Subtle, understated
- **1** — Light
- **2** — Moderate
- **3** — Strong
- **4** — Maximum

### Modifier (PortraitModifier)

| Modifier | Use when |
|----------|----------|
| Default | Standard pose |
| SideLookLeft / SideLookRight | Looking away, thoughtful |
| LookingDown | Reflective, gentle |
| DirectEyeContact | Direct, present |
| OpenHands | Open, inviting |
| HugOffer | Offering comfort |
| SweatDrop | Nervous, awkward |
| NoFace | No face visible (e.g. back of head) |
| MouthOpen | Mid-speech, surprised |
| WideEyes | Surprised, alert |

### How to Note Mood/Intensity/Modifier

As you write, you can add a note like:

```
reassure.general — [Mood: Warm, Intensity: 1, Modifier: Default]
"You don't need to earn rest. You're allowed to be still."
```

Seraphine can use these notes when assigning portraits in the PortraitDatabase or when setting PortraitRequest on nodes. The system will pick the closest matching portrait if an exact match isn't available.

---

## Summary for Holiday

1. **Lines go in Lines.json** under `entries`, each with a `key` and `variants`.
2. **Keys use dot notation** like `branch.context` (e.g. `reassure.general`, `grounding.failed.no`).
3. **Reassurance** = one node, one key (`reassure.general`), many variants.
4. **Grounding** = multi-step flow with context-based lines for "from_no" and "from_dontknow."
5. **Mood/Intensity/Modifier** = optional notes for Seraphine so portraits match the tone.
6. **Possible next steps** = ideas for Seraphine to expand branches or add new ones.

When in doubt, match the tone of existing lines: gentle, validating, low-pressure, and warm.
