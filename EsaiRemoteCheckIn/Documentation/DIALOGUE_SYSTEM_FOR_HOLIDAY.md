# Esai Dialogue System — Writer's Guide for Holiday

Hi Holiday. This document explains how Esai's dialogue system works so you can write lines that fit the structure, and make notes about mood and intensity for Seraphine as you go.

---

## What Needs Writing (v3 Update)

The flow expanded significantly. All lines below marked **DRAFT** were written by Claude as structural placeholders — they're in the right place and do the right thing, but they need your voice on them. Lines marked **UNCHANGED** were already in the game and are fine unless you want to revisit them.

Lines marked **ORPHANED** are still in Lines.json but are no longer reachable in the flow. They can stay as archival content or be removed.

### Keys to prioritise

These are the ones players will hit most often:

| Priority | Key | Current draft | Note |
|----------|-----|---------------|------|
| High | `ok.light` | "Let's keep it gentle. What's one small good thing from today?" | First response in the Okay happy path |
| High | `action.vent.listen` | 6 variants: "I hear you." / "Keep going." etc. | Repeating loop — needs the most variants |
| High | `action.vent.open` | "I'm here. Take all the space you need. Say it as messy as it is." | Entry to vent mode |
| High | `state.anxious.validate` | "Anxiety makes everything feel urgent and impossible at once..." | High-traffic state |
| High | `event.conflict.react` | "Ugh. That lands hard. I'm sorry." | Beat 1 empathy, fires immediately |
| High | `grounding.success.response` | 2 variants: "Good. I'm glad. Hold onto that." etc. | The moment grounding works |
| High | `session_close` | "Take care of yourself. I'm here whenever you need." | Last thing player sees |

---

## All Keys by Branch

### Root

| Key | Status | Current text | Portrait |
|-----|--------|--------------|---------|
| `root.greeting` | UNCHANGED | "Hey... come here a second. How are you, really?" | Warm 7, intensity 1 |

---

### Okay Branch

Player said "I'm okay today." New in v3: this branch now has real depth instead of two inline responses.

**Player picks "Keep it light."**

| Key | Status | Current text | Portrait |
|-----|--------|--------------|---------|
| `ok.light` | DRAFT | "Let's keep it gentle. What's one small good thing from today?" | Warm 7, intensity 1 |
| `ok.celebrate.accomplishment` | DRAFT | "Getting things done while carrying what you carry — that's real. I mean that." | Warm 7, intensity 2 |
| `ok.celebrate.smile` | DRAFT | "You found it. Hold onto that — noticing is its own kind of skill." | Friendly 1, intensity 1 |
| `ok.celebrate.survival` | DRAFT | "That IS something. Genuinely. You're here." | Warm 7, intensity 2 |
| `ok.celebrate.anticipation` | DRAFT | "Hold onto that. Having something to look forward to is a quiet kind of fuel." | Warm 7, intensity 1 |

*`ok.celebrate.*` keys use context — the game picks the right one based on which option the player chose at `ok.light`.*

**Player picks "Quick check-in."**

| Key | Status | Current text | Portrait |
|-----|--------|--------------|---------|
| `ok.body.check` | DRAFT | "Body first — water, food, somewhere comfortable. Are those covered right now?" | Friendly 1, intensity 1 |
| `ok.body.good` | DRAFT | "Good. That's the foundation. Emotionally — genuinely okay, or more like functional?" | Friendly 1, intensity 1 |
| `ok.body.partial` | DRAFT | "Okay. Let's close the gap a little. What's most off — sleep, food, water, or somewhere to land?" | Concerned 2, intensity 1 |
| `ok.body.missing` | DRAFT | "Okay. The body speaks first. What's most off right now?" | Concerned 2, intensity 1 |

**Orphaned from old flow (no longer reachable):**

| Key | Status | Note |
|-----|--------|------|
| `okay.prompt` | UNCHANGED but slightly orphaned | Still used as the Okay entry node text. The options have changed but the line itself still works. May want a light touch. |
| `okay.keep_light` | ORPHANED | Was an inline response, now replaced by the ok_light → ok_celebrate path |
| `okay.quick_checkin` | ORPHANED | Was an inline response, now replaced by the ok_body path |

---

### Support Branch (Direct)

Player said "I need support." or pivoted from elsewhere. Options changed in v3.

| Key | Status | Current text | Portrait |
|-----|--------|--------------|---------|
| `support.prompt` | UNCHANGED | "Then that's what we'll do. What kind of support fits right now?" | Warm 7, intensity 2 |

*The options on this node are now: "I need to vent." / "Ground me." / "Reassurance." / "Help me choose one step." — no longer "Help me choose one step." as an inline response.*

| Key | Status | Note |
|-----|--------|------|
| `support.one_step` | ORPHANED | Was an inline response on the support node. The one-step path now goes through `action.step.prompt` and `action.step.confirm`. |

---

### Something Branch

Player said "Something happened." Completely restructured in v3. Now uses a **two-beat** format: Beat 1 is an empathy response that fires immediately when the player picks an option, then a Continue button leads to Beat 2 (a clarification question).

**Beat 1 — Empathy (fires on option select):**

| Key | Status | Current text | Portrait |
|-----|--------|--------------|---------|
| `something.prompt` | UNCHANGED | "I'm listening. What kind of 'something' was it?" | Concerned 2, intensity 1 |
| `event.disappointment.react` | DRAFT | "Ugh. That stings — especially when you were counting on it." | Concerned 2, intensity 2 |
| `event.conflict.react` | DRAFT | "Ugh. That lands hard. I'm sorry." | Concerned 2, intensity 2 |
| `event.loss.react` | DRAFT | "I'm sorry. Loss is heavy — and it doesn't apologize for the timing." | Sad 4, intensity 1 |
| `something.overwhelm` | UNCHANGED | "That makes sense. Let's shrink the day. What can wait, and what can't?" | Warm 7, intensity 1 |

**Beat 2 — Clarification (player taps Continue after Beat 1):**

| Key | Status | Current text | Portrait |
|-----|--------|--------------|---------|
| `event.disappointment.clarify` | DRAFT | "What's sitting heaviest — what you missed out on, or what it means for what's ahead?" | Concerned 2, intensity 1 |
| `event.conflict.clarify` | DRAFT | "What's bothering you most — the situation itself, or how it made you feel about yourself?" | Concerned 2, intensity 1 |
| `event.loss.clarify` | DRAFT | "Are you more in the grief of it right now, or trying to figure out what comes next?" | Sad 4, intensity 1 |
| `event.overwhelm.clarify` | DRAFT | "Is there too much to do, or not enough of you to do it — or both?" | Concerned 2, intensity 1 |

**Support selection (after clarification):**

| Key | Status | Current text | Portrait |
|-----|--------|--------------|---------|
| `select.event.conflict` | DRAFT | "I hear you. What would help most right now?" | Warm 7, intensity 2 |
| `select.event.disappointment` | DRAFT | "Okay. Given that — what do you need from me?" | Warm 7, intensity 1 |
| `select.event.overwhelm` | DRAFT | "That's a lot. What would help most right now?" | Warm 7, intensity 1 |
| `select.loss` | DRAFT | "I'm with you. What do you need?" | Warm 7, intensity 1 |

*`select.event.*` keys use context — Loss uses `select.loss` separately because its support options differ (no Boundary mode; has OneStep instead).*

**Orphaned from old flow:**

| Key | Status | Note |
|-----|--------|------|
| `something.conflict` | ORPHANED | Replaced by event.conflict.react + event.conflict.clarify |
| `something.bad_news` | ORPHANED | "Bad news" is no longer an option; replaced by conflict/disappointment/loss |

---

### Don't Know Branch

Player said "I don't know." Now has 4 options and routes into full state sub-branches instead of inline responses.

| Key | Status | Current text | Portrait |
|-----|--------|--------------|---------|
| `dontknow.prompt` | UNCHANGED | "That's okay. 'I don't know' still counts as information. Let's narrow it down." | Warm 7, intensity 1 |
| `dontknow.body` | UNCHANGED (reused) | "Okay. Let's get basic care first — water, food, rest. What's missing right now?" | Friendly 1, intensity 1 |
| `dontknow.emotion` | UNCHANGED (reused) | "Okay. If you had to name it loosely — anxious, sad, angry, or numb?" | Warm 7, intensity 1 |
| `dontknow.empty` | UNCHANGED (reused) | "You're allowed to be empty. Let's do minimum-viable-human for a bit." | Warm 7, intensity 2 |

*These three keys are now node text (player sees them and then picks from options below) rather than inline responses. The content is the same but the context is slightly different — worth checking they still feel right as standalone screens rather than one-line reactions.*

**Body sub-branch:**

| Key | Status | Current text | Portrait |
|-----|--------|--------------|---------|
| `state.body.sleep` | DRAFT | "Sleep debt is its own kind of weight. Real damage — not a character flaw." | Concerned 2, intensity 1 |
| `state.body.food` | DRAFT | "Can we get something in you? Not a lecture — just asking." | Warm 7, intensity 1 |

**Emotion sub-branch — validation beats:**

These are TapToContinue nodes. The player names their emotion, Esai validates it, then Continue leads to support selection.

| Key | Status | Current text | Portrait |
|-----|--------|--------------|---------|
| `state.anxious.validate` | DRAFT | "Anxiety makes everything feel urgent and impossible at once. That's not you failing — that's your nervous system in overdrive." | Calm 12, intensity 1 |
| `state.sad.validate` | DRAFT | "Sadness is real. It doesn't need a reason big enough to justify it. It just needs space." | Sad 4, intensity 1 |
| `state.angry.validate` | DRAFT | "Anger is often the part of you that knows you deserved better. It's not wrong." | Firm 3, intensity 1 |
| `state.numb.validate` | DRAFT | "Sometimes the feelings get so loud the system just... quiets down. That's not emptiness. That's protection." | Calm 12, intensity 1 |

**Empty sub-branch:**

| Key | Status | Current text | Portrait |
|-----|--------|--------------|---------|
| `state.empty.guide` | DRAFT | "Get some water. Find somewhere soft to sit. Put something familiar on in the background. Not a task list — just anchors." | Warm 7, intensity 1 |
| `state.empty.widen` | DRAFT | "If this kind of empty sticks around or gets heavier, it might help to talk to someone outside this space too. You don't have to handle that alone." | Warm 7, intensity 1 |

*`state.empty.widen` is a soft safety widening beat — shown only after the stabilisation guide. Tone: a seed of permission, not a directive. No alarm language, no clinical framing. "You don't have to handle that alone" is about as far as it should go.*

**All-of-the-above entry:**

| Key | Status | Current text | Portrait |
|-----|--------|--------------|---------|
| `state.all` | DRAFT | "That's a lot. When everything's off at once, the body's usually the fastest fix. Can we start there?" | Warm 7, intensity 1 |

**State support selection (after validation beat):**

| Key | Status | Current text | Portrait |
|-----|--------|--------------|---------|
| `select.state.anxious` | DRAFT | "Okay. Let's figure out what would help most right now." | Calm 12, intensity 1 |
| `select.state.sad` | DRAFT | "I'm here. What would help right now?" | Sad 4, intensity 1 |
| `select.state.angry` | DRAFT | "Okay. What do you need from me right now?" | Firm 3, intensity 1 |
| `select.state.numb` | DRAFT | "Let's go gently. What would help most right now?" | Calm 12, intensity 1 |

---

### Action Modes

These are the "what would help" destinations — Vent, Grounding, Boundary, OneStep, Reassurance.

**Reassurance** — unchanged:

| Key | Status | Current text | Portrait |
|-----|--------|--------------|---------|
| `reassure.general` | UNCHANGED | *(2 variants)* "Hey. You're okay..." / "You don't need to earn rest..." | Warm 7, intensity 2 |

**Grounding** — unchanged except the success beat is new:

| Key | Status | Current text | Portrait |
|-----|--------|--------------|---------|
| `grounding.exercise` | UNCHANGED | "Can you feel your feet on the floor? One slow breath in... and out. Good. Stay with me." | Calm 12, intensity 1 |
| `grounding.followup.question` | UNCHANGED | "How's that? Do you feel a bit more grounded?" | Friendly 1, intensity 1 |
| `grounding.exercise2.from_no` | UNCHANGED | "That's okay. Let's try something different — name five things you can see right now." | Warm 7, intensity 1 |
| `grounding.exercise2.from_dontknow` | UNCHANGED | "That's okay. Let's narrow it down — are you more in your head or in your body right now?" | Warm 7, intensity 1 |
| `grounding.failed.no` | UNCHANGED | "That's okay. Sometimes it takes longer. You're still here, you're still trying. That matters." | Warm 7, intensity 2 |
| `grounding.failed.dontknow` | UNCHANGED | "Maybe grounding isn't what you need right now. That's okay — let's try something different." | Warm 7, intensity 1 |
| `grounding.success.response` | DRAFT | *(2 variants)* "Good. I'm glad. Hold onto that." / "That's something. You just helped yourself." | Warm 7, intensity 2 |

**Vent mode** — all new:

| Key | Status | Current text | Portrait |
|-----|--------|--------------|---------|
| `action.vent.open` | DRAFT | "I'm here. Take all the space you need. Say it as messy as it is." | Warm 7, intensity 2 |
| `action.vent.listen` | DRAFT | *(6 variants, noRepeatWindow 4)* "I hear you." / "Keep going." / "I'm with you." / "Yeah." / "That's a lot." / "I'm listening." | Concerned 2, intensity 1 |
| `action.vent.more` | DRAFT | *(4 variants, noRepeatWindow 3)* "Keep going. I'm with you." / "Say the rest of it." / "There's more — let it out." / "I'm still here." | Warm 7, intensity 1 |
| `action.vent.reflect` | DRAFT | "Thank you for trusting me with that. How are you feeling right now?" | Warm 7, intensity 2 |
| `support.redirect` | DRAFT | "Okay. What would help more right now?" | Concerned 2, intensity 1 |

*`action.vent.listen` is the main loop — the player can keep saying "There's more" and Esai cycles through variants. This key will benefit most from more variants (aim for 8–10 eventually).*

**Boundary mode** — all new:

| Key | Status | Current text | Portrait |
|-----|--------|--------------|---------|
| `action.bound.open` | DRAFT | "Okay. Let's sort out what's actually yours to carry here." | Calm 12, intensity 1 |
| `action.bound.question` | DRAFT | "What feels hardest — figuring out what's yours to own, or letting go of what isn't?" | Concerned 2, intensity 1 |
| `action.bound.own` | DRAFT | "Owning your part isn't the same as taking all the blame. What's genuinely yours is worth naming — without the extra weight that isn't." | Calm 12, intensity 1 |
| `action.bound.release` | DRAFT | "What someone else chose to do — that belongs to them, not to you. You don't have to carry decisions you didn't make." | Warm 7, intensity 1 |
| `action.bound.both` | DRAFT | "Both at once is a lot. Which end do you see more clearly right now — what's yours, or what isn't?" | Calm 12, intensity 1 |

**One-step mode** — all new:

| Key | Status | Current text | Portrait |
|-----|--------|--------------|---------|
| `action.step.prompt` | DRAFT | "What's the smallest thing that would make today 1% easier? Not the whole list. Just one thing." | Friendly 1, intensity 1 |
| `action.step.confirm` | DRAFT | "Okay. Just that. Not the rest — just that one thing. You've got it." | Warm 7, intensity 1 |

---

### Hub and Close

| Key | Status | Current text | Note |
|-----|--------|--------------|------|
| `hub.checkin` | UNCHANGED | "Where would you like to go from here?" | Options are now "Check in again." / "Try something different." / "I'll get back to the day." |
| `follow_up.prompt` | UNCHANGED | "Okay. Where would you like to go from here?" | After grounding fails (No path) |
| `redirect.prelude` | UNCHANGED | "Okay. Let's try something completely different." | After grounding fails (I don't know path) |
| `coming_soon` | UNCHANGED | "That part isn't ready yet — but it's coming. For now, let's work with what we have." | Not reachable from main flow in v3 |
| `session_close` | UNCHANGED | "Take care of yourself. I'm here whenever you need." | Now has an "Okay." button rather than auto-dismissing |

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
  "rules": { "noRepeatWindow": 5 },
  "portraitMood": 7,
  "portraitIntensity": 1,
  "portraitModifier": 0
}
```

- **key** — Unique identifier. Use dot notation: `branch.context` (e.g. `reassure.general`, `grounding.failed.no`).
- **variants** — One or more possible lines. The game picks one (weighted randomly). Add variants for variety; the same key can have many.
- **rules.noRepeatWindow** — Optional. If set (e.g. 5), the game avoids repeating the same variant for the last N picks. Good for reassurance, vent listen, and other repeatable lines.
- **portraitMood**, **portraitIntensity**, **portraitModifier** — Optional. Portrait is controlled **per text key**, not per node. If a line has these, Esai's portrait uses them; otherwise the node's default portrait is used.

### Node Structure

Each dialogue "screen" is a **node**. A node has:
- A **text key** that pulls the line from Lines.json
- **Options** (buttons the player can tap), each with a label and a destination

Some nodes use **context** — the same node can show different text depending on how the player got there. For example, `grounding_exercise_2` shows different text when the player came from "No" vs "I don't know."

**TapToContinue nodes** show a single Continue button instead of a choice. These are used for validation beats and acknowledgment moments — lines where Esai says something and the player just... receives it before moving on.

**Two-beat nodes** (new in v3): some options trigger a Beat 1 empathy response immediately (no navigation), then replace the options with a single Continue button that leads to Beat 2. Used in the Something branch — the player picks what happened, Esai reacts, then they continue to the clarification question.

---

## Mood and Intensity System

When you write a line, you can suggest a **mood** and **intensity** for the portrait.

### Mood (portraitMood)

| Value | Name | Use when |
|-------|------|----------|
| 0 | Neutral | Default, calm, even |
| 1 | Friendly | Warm, welcoming, gentle |
| 2 | Concerned | Worried about the player, caring |
| 3 | Firm | Steady, clear, "you can do this" |
| 4 | Sad | Sharing sadness, deep empathy |
| 5 | Shocked | Surprised, taken aback |
| 6 | Devastated | Deep distress (use sparingly) |
| 7 | Warm | Soft, nurturing |
| 8 | Amused | Light, gentle humor |
| 9 | Embarrassed | Shy, self-conscious |
| 10 | Excited | Eager, upbeat |
| 11 | Surprised | Mild surprise |
| 12 | Calm | Grounded, steady, safe |

### Intensity (0–4)

- **0** — Subtle, understated
- **1** — Light
- **2** — Moderate
- **3** — Strong
- **4** — Maximum

### Modifier (portraitModifier)

| Value | Name | Use when |
|-------|------|----------|
| 0 | Default | Standard pose |
| 1 | SideLookLeft | Looking away, thoughtful |
| 2 | SideLookRight | Looking away, thoughtful |
| 3 | LookingDown | Reflective, gentle |
| 4 | DirectEyeContact | Direct, present, serious |
| 5 | OpenHands | Open, inviting |
| 6 | HugOffer | Offering comfort |
| 7 | SweatDrop | Nervous, awkward |
| 8 | NoFace | No face visible |
| 9 | MouthOpen | Mid-speech, surprised |
| 10 | WideEyes | Surprised, alert |
| 11 | LeftHandOut | Reaching out |
| 12 | Waving | Greeting / goodbye |

### How to note mood in your draft

As you write, note it like this:

```
action.vent.open — [Mood: Warm (7), Intensity: 2, Modifier: Default]
"I'm here. Take all the space you need. Say it as messy as it is."
```

Seraphine adds `portraitMood`, `portraitIntensity`, `portraitModifier` to the Lines.json entry.

---

## Quick Reference — Key Counts

| Branch | DRAFT keys | UNCHANGED keys | ORPHANED keys |
|--------|-----------|----------------|---------------|
| Root | 0 | 1 | 0 |
| Okay | 9 | 1 | 2 |
| Support | 0 | 2 | 1 |
| Something / Event | 11 | 2 | 2 |
| Don't Know / State | 13 | 4 | 0 |
| Grounding | 1 | 6 | 0 |
| Vent | 5 | 0 | 0 |
| Boundary | 5 | 0 | 0 |
| One Step | 2 | 0 | 0 |
| Reassurance | 0 | 1 | 0 |
| Hub / Close | 0 | 5 | 0 |
| **Total** | **46** | **22** | **5** |

When in doubt, match the tone of existing lines: present, direct, warm without being cloying, non-clinical. Esai notices. He doesn't fix.
