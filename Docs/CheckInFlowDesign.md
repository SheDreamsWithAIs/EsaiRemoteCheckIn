# Esai Check-In Mode: Conversation Flow Design

**Document purpose:** Full node-graph architecture for Check-In Mode. This is the spec Holiday writes lines against and the dev implements in `CreateCheckInModule.cs`.

---

## Architecture Overview

Aethon named it correctly: this is **cognitive-behavioral flow architecture** organized around how humans actually narrate distress, not how clinicians categorize it.

```
Layer 1 │ State Gate          — "How are you, really?"
Layer 2 │ Branch Selection    — Okay / Need Support / Something Happened / Don't Know
Layer 3 │ Problem Framing     — Clarifying questions (validate feeling, not conclusion)
Layer 4 │ Support Selection   — Player chooses what kind of help they need
Layer 5 │ Action Mode         — Esai delivers the chosen support
Layer 6 │ Check-In Loop       — How are you now? Loop until natural exit.
```

Key therapeutic principles embedded in the structure:
- Esai validates the **emotional reality** before asking clarifying questions
- He does **not** validate conclusions he can't verify ("they were wrong") — he validates the *pain* ("that sounds awful")
- The player chooses their support type — **agency is always preserved**
- Every branch can reach the hub and exit cleanly

---

## Full Node Map

### Layer 1 — State Gate

---

#### `root`
> **Esai says:** Warm greeting. "How are you, really?" — inviting, not clinical.
> **Portrait:** Warm, intensity 1, Direct Eye Contact

| Option Label | Goes To |
|---|---|
| "I'm okay today." | `okay` |
| "I need support." | `support` |
| "Something happened." | `something` |
| "I don't know." | `dontknow` |

---

### Layer 2A — Okay Branch

---

#### `okay`
> **Esai says:** Glad you're steady. Keep it light, or do a real check-in?
> **Portrait:** Friendly, intensity 1

| Option Label | Goes To |
|---|---|
| "Keep it light." | `okay_light` |
| "Quick check-in." | `okay_body_check` |
| "Actually... I need support." | `support` |

---

#### `okay_light`
> **Esai says:** "Let's keep it gentle. One small good thing from today?" — no pressure, celebrating the ordinary.
> **Portrait:** Warm, intensity 1, slight smile

| Option Label | Goes To | Context |
|---|---|---|
| "Something I got done." | `okay_celebrate` | `accomplishment` |
| "A moment that made me smile." | `okay_celebrate` | `smile` |
| "Just still standing." | `okay_celebrate` | `survival` |
| "Something I'm looking forward to." | `okay_celebrate` | `anticipation` |

---

#### `okay_celebrate`
> **Esai says:** [Context-dependent celebration — see text keys below]
> - `accomplishment` → Acknowledges the effort, not just the result. Something like "That counts. Getting things done while carrying what you carry — that's real."
> - `smile` → Celebrates the noticing. "You found it. That's not nothing."
> - `survival` → No irony, full sincerity. "That IS something. You're here."
> - `anticipation` → Warm curiosity. "Hold on to that. Having something to look forward to is a form of strength."
>
> **Portrait:** Warm → Excited (low intensity), context-appropriate
> **Advance:** TapToContinue → `hub_checkin`

---

#### `okay_body_check`
> **Esai says:** "Body first — water, food, somewhere comfortable. Are those covered?"
> **Portrait:** Friendly, intensity 1, attentive

| Option Label | Goes To |
|---|---|
| "Yes, I'm good." | `okay_body_good` |
| "Partly." | `okay_body_partial` |
| "Not really." | `okay_body_missing` |

---

#### `okay_body_good`
> **Esai says:** "Good. That's the foundation. Emotionally — genuinely okay, or more like functional?"
> **Portrait:** Friendly, intensity 1

| Option Label | Goes To |
|---|---|
| "Genuinely okay." | `hub_checkin` |
| "Functional. Holding it together." | `support` |

---

#### `okay_body_partial` / `okay_body_missing`
> **Esai says:** [Gentle, not scolding. Acknowledges the body's needs. Suggests addressing the gap.]
> - `partial` → "Okay, let's close the gap. What's most off — sleep, water/food, or somewhere to land?"
> - `missing` → "Okay. The body speaks first. What's most off right now?"
>
> **Portrait:** Concerned, intensity 1
> **Advance:** TapToContinue → `hub_checkin` (with encouragement to address basics before anything else)

---

### Layer 2B — Need Support Branch

---

#### `support`
> **Esai says:** "Then that's what we'll do. What kind of support fits right now?"
> **Portrait:** Warm, intensity 2

| Option Label | Goes To |
|---|---|
| "I need to vent." | `vent_open` |
| "Ground me." | `grounding_exercise` |
| "Reassurance." | `reassurance_response` |
| "Help me choose one step." | `one_step_prompt` |

*Note: This node is also reached from `okay_body_good`, `something` branches after clarification, and `dontknow` branches after emotion identification.*

---

### Layer 2C — Something Happened Branch

---

#### `something`
> **Esai says:** "I'm listening. What kind of 'something' was it?"
> **Portrait:** Concerned, intensity 1

| Option Label | Immediate Response (responseTextKey) | Then Goes To |
|---|---|---|
| "Something didn't go my way." | `something.disappointment.react` | `disappointment_clarify` |
| "Someone hurt me." | `something.conflict.react` | `conflict_clarify` |
| "I lost something." | `something.loss.react` | `loss_clarify` |
| "Just too much at once." | `something.overwhelm.react` | `overwhelm_clarify` |

**Important design note:** The immediate `responseTextKey` is Esai's first beat — pure empathy, no analysis. The clarify node is the second beat — he digs in. Two-beat structure: *feel first, think second.*

---

#### `disappointment_clarify`
> **Esai says:** "What's sitting heaviest — what you missed out on, or what it means for what's ahead?"
> **Portrait:** Concerned, intensity 1, attentive

| Option Label | Goes To | Context |
|---|---|---|
| "What I missed out on." | `support_select` | `disappointment` |
| "What it means going forward." | `support_select` | `disappointment` |
| "Both." | `support_select` | `disappointment` |

*Holiday note: These options can carry the same context — the distinction mostly informs her line variants for the next reassurance or boundary content, not the routing.*

---

#### `conflict_clarify`
> **Esai says:** "What's bothering you most — the situation itself, or how it made you feel about yourself?"
> **Portrait:** Concerned, intensity 1–2

| Option Label | Goes To | Context |
|---|---|---|
| "The situation." | `support_select` | `conflict` |
| "How it made me feel." | `support_select` | `conflict` |
| "Both." | `support_select` | `conflict` |

*Therapeutic note: This question gently separates event from identity conclusion. Esai doesn't need to challenge anything here — the question plants the seed.*

---

#### `loss_clarify`
> **Esai says:** "Are you more in the grief of it right now, or trying to figure out what comes next?"
> **Portrait:** Sad, intensity 1 — present with them, not trying to fix

| Option Label | Goes To | Context |
|---|---|---|
| "In the grief." | `support_select` | `loss` |
| "Figuring out what's next." | `support_select` | `loss` |
| "I don't know." | `support_select` | `loss` |

---

#### `overwhelm_clarify`
> **Esai says:** "Is there too much to do, or not enough of you to do it — or both?"
> **Portrait:** Concerned, intensity 1, warm

| Option Label | Goes To | Context |
|---|---|---|
| "Too much to do." | `support_select` | `overwhelm` |
| "Not enough of me." | `support_select` | `overwhelm` |
| "Both." | `support_select` | `overwhelm` |

*Note: "Not enough of me" is the depletion/burnout signal. It routes to the same support_select, but the context should inform whether Esai's first action mode beat leans toward grounding (body) vs. reassurance (worth).*

---

### Layer 3 — Support Selection (shared node, context-aware)

---

#### `support_select`
> **Esai says:** [Varies by context — see text keys]
> - `conflict` → "I hear you. What would help most right now?"
> - `disappointment` → "Okay. Given that... what do you need from me?"
> - `loss` → "I'm with you. What do you need?"
> - `overwhelm` → "That's a lot. What would help most right now?"
>
> **Portrait:** Warm, intensity 1–2, calm

| Option Label | Goes To |
|---|---|
| "I need to vent." | `vent_open` |
| "Ground me." | `grounding_exercise` |
| "Help me understand what's mine." | `boundary_mode` |
| "Just steady me." | `reassurance_response` |

*Note for loss context: Consider swapping "Help me understand what's mine" for "Help me figure out what's next" — grief often isn't about boundaries, it's about navigation. Can be handled with context-based label text if the system supports it, or by adding `support_select_loss` as a variant node.*

---

### Layer 2D — Don't Know Branch

---

#### `dontknow`
> **Esai says:** "That's okay. 'I don't know' still counts as information. Let's narrow it down."
> **Portrait:** Warm, intensity 1

| Option Label | Goes To |
|---|---|
| "Body feels off." | `dontknow_body` |
| "Emotions feel bad." | `dontknow_emotion` |
| "Just empty." | `dontknow_empty` |
| "All of the above." | `dontknow_all` |

---

#### `dontknow_body`
> **Esai says:** "Okay. Let's start there. Water, food, sleep — what's most off?"
> **Portrait:** Concerned/Friendly, intensity 1

| Option Label | Goes To |
|---|---|
| "Sleep." | `dontknow_body_sleep` |
| "Water or food." | `dontknow_body_food` |
| "I'm not sure, just awful." | `grounding_exercise` |

---

#### `dontknow_body_sleep`
> **Esai says:** Sleep deprivation is real damage, not weakness. Gentle acknowledgment, no lecture. Maybe: "Sleep debt is its own kind of weight. Is there any way to get more rest today?"
> **Portrait:** Concerned, intensity 1
> **Advance:** TapToContinue → `hub_checkin`

---

#### `dontknow_body_food`
> **Esai says:** Pure care, zero guilt. "Can we get you something? Not a lecture — just asking."
> **Portrait:** Warm, intensity 1
> **Advance:** TapToContinue → `hub_checkin`

---

#### `dontknow_emotion`
> **Esai says:** "If you had to name it loosely — anxious, sad, angry, or numb?"
> **Portrait:** Warm/Concerned, intensity 1

| Option Label | Goes To |
|---|---|
| "Anxious." | `emotion_anxious` |
| "Sad." | `emotion_sad` |
| "Angry." | `emotion_angry` |
| "Numb." | `dontknow_empty` |

---

#### `emotion_anxious`
> **Esai says:** Validates the experience without feeding the spiral. Something like: "Anxiety makes everything feel urgent and impossible at once. That's not you failing — that's your nervous system in overdrive."
> **Portrait:** Calm, intensity 1 (grounding presence)
> **Advance:** TapToContinue → `support_select` (context: `anxious`)

---

#### `emotion_sad`
> **Esai says:** Pure, unhurried validation. "Sadness is real. It doesn't need a reason big enough to justify it. It just needs space."
> **Portrait:** Sad/Warm blend — present with them
> **Advance:** TapToContinue → `support_select` (context: `sad`)

---

#### `emotion_angry`
> **Esai says:** Validates anger as signal, not flaw. "Anger is often the part of you that knows you deserved better. It's not wrong."
> **Portrait:** Firm/Warm — steady, not alarmed
> **Advance:** TapToContinue → `support_select` (context: `angry`)

---

#### `dontknow_empty`
> **Esai says:** "You're allowed to be empty. Let's do minimum-viable-human — just the basics, no guilt."
> **Portrait:** Warm, intensity 2

| Option Label | Goes To |
|---|---|
| "Okay." | `dontknow_empty_guide` |
| "Help me calm down." | `grounding_exercise` |

---

#### `dontknow_empty_guide`
> **Esai says:** Something practical and kind. A small, specific task: get water, sit somewhere soft, put on something familiar. Not a lecture, just a quiet nudge.
> **Portrait:** Warm/Calm
> **Advance:** TapToContinue → `hub_checkin`

---

#### `dontknow_all`
> **Esai says:** "That's a lot. When everything is off, let's start with the body — it's usually the fastest fix. Is that okay?"
> **Portrait:** Warm, intensity 1, gentle

| Option Label | Goes To |
|---|---|
| "Yes, let's start there." | `dontknow_body` |
| "The emotions are louder." | `dontknow_emotion` |
| "I just need to calm down." | `grounding_exercise` |

---

### Layer 5 — Action Modes

---

#### `grounding_exercise` *(existing — keep)*
> Two-round grounding sequence with check-ins. Already well-built. Routes to `hub_checkin` on success, gentle acknowledgment on failure.

---

#### `reassurance_response` *(existing — expand content)*
> One-beat reassurance, then `hub_checkin`. The structural loop is correct.
> Holiday should expand to 8–12 variants with `noRepeatWindow: 8` so it stays fresh across sessions.

---

#### `one_step_prompt` *(replace dead end)*
> **Esai says:** "What's the smallest thing that would make today 1% easier? Not the whole to-do list. Just one thing."
> **Portrait:** Friendly/Warm, intensity 1
> **Advance:** TapToContinue → `one_step_response`

---

#### `one_step_response`
> **Esai says:** "Okay. Just that. Not the rest — just that one thing. You've got it."
> **Portrait:** Warm, intensity 1, gentle affirm
> **Advance:** TapToContinue → `hub_checkin`

---

#### `vent_open`
> **Esai says:** "I'm here. Take all the space you need. Say it as messy as it is."
> **Portrait:** Warm, intensity 2, open — no judgment
> **Advance:** TapToContinue → `vent_listening`

---

#### `vent_listening`
> **Esai says:** A brief, non-directive reflection. "I hear you." — Not summarizing, not analyzing. Just present.
> **Portrait:** Concerned/Warm, intensity 1–2

| Option Label | Goes To |
|---|---|
| "There's more." | `vent_more` |
| "I think I've said it." | `vent_reflection` |
| "I'm still really upset." | `support_redirect` |

---

#### `vent_more`
> **Esai says:** "Keep going. I'm with you."
> **Portrait:** Warm/Concerned, intensity 1 — steady, holding space
> **Advance:** TapToContinue → `vent_listening`

*This creates the loop: player can cycle through vent_more → vent_listening as many times as needed.*

---

#### `vent_reflection`
> **Esai says:** Thanks them for trusting him. Gentle reflection of what he heard — not analysis, just acknowledgment. "Thank you for trusting me with that. How are you feeling right now?"
> **Portrait:** Warm, intensity 2

| Option Label | Goes To |
|---|---|
| "A bit better." | `hub_checkin` |
| "Still the same." | `support_redirect` |
| "Worse." | `support_redirect` |

---

#### `support_redirect`
> **Esai says:** Not venting first. Redirects with care. "Okay. What would help more right now?"
> **Portrait:** Concerned/Warm, intensity 1

| Option Label | Goes To |
|---|---|
| "Ground me." | `grounding_exercise` |
| "Reassurance." | `reassurance_response` |
| "Help me choose one step." | `one_step_prompt` |
| "Just stay with me." | `vent_open` |

*"Just stay with me" loops back to vent — player may need to vent more before any other mode helps.*

---

#### `boundary_mode`
> **Esai says:** "Let's sort out what's actually yours to carry here."
> **Portrait:** Calm/Firm, intensity 1 — steady and clear
> **Advance:** TapToContinue → `boundary_question`

---

#### `boundary_question`
> **Esai says:** "What feels hardest — figuring out what's yours to own, or letting go of what isn't?"
> **Portrait:** Concerned, intensity 1

| Option Label | Goes To |
|---|---|
| "What's mine to own." | `boundary_own` |
| "Letting go of what isn't mine." | `boundary_release` |
| "Both." | `boundary_both` |

---

#### `boundary_own`
> **Esai says:** Helps name what genuinely belongs to them — without shame, with clarity. Validates that owning your part doesn't mean taking all the blame.
> **Portrait:** Calm/Warm, intensity 1
> **Advance:** TapToContinue → `hub_checkin`

---

#### `boundary_release`
> **Esai says:** Helps name what isn't theirs. Gently externalizes what's been internalized. "What someone else chose to do — that belongs to them, not to you."
> **Portrait:** Warm/Firm, intensity 1
> **Advance:** TapToContinue → `hub_checkin`

---

#### `boundary_both`
> **Esai says:** Acknowledges the complexity. Suggests starting with one end — whichever feels clearest. "Let's not try to do both at once. Which end do you see more clearly right now?"
> **Portrait:** Calm, intensity 1
> **Advance:** TapToContinue → `hub_checkin`

---

### Layer 6 — Check-In Loop

---

#### `hub_checkin` *(existing — keep, minor expansion)*
> **Esai says:** "Where would you like to go from here?"
> **Portrait:** Neutral/Warm, intensity 1

| Option Label | Goes To |
|---|---|
| "Check in again." | `root` |
| "Try something different." | `support` |
| "I'll get back to the day." | `session_close` |

*Remove or replace "Something else." → `coming_soon` once content exists. For now keep as placeholder.*

---

#### `session_close` *(existing — expand content)*
> **Esai says:** A real goodbye — warm, specific, not perfunctory. "Take care of yourself. I'm here whenever you need."
> **Portrait:** Warm, intensity 2
> `triggersEndOverlay: true`

---

## Complete Node Reference

| nodeId | Status | Leads To |
|---|---|---|
| `root` | existing | okay / support / something / dontknow |
| `okay` | existing | okay_light / okay_body_check / support |
| `okay_light` | **new** | okay_celebrate (×4 contexts) |
| `okay_celebrate` | **new** | hub_checkin |
| `okay_body_check` | **new** | okay_body_good / okay_body_partial / okay_body_missing |
| `okay_body_good` | **new** | hub_checkin / support |
| `okay_body_partial` | **new** | hub_checkin |
| `okay_body_missing` | **new** | hub_checkin |
| `support` | existing | vent_open / grounding_exercise / reassurance_response / one_step_prompt |
| `something` | existing (fix dead ends) | disappointment_clarify / conflict_clarify / loss_clarify / overwhelm_clarify |
| `disappointment_clarify` | **new** | support_select (context: disappointment) |
| `conflict_clarify` | **new** | support_select (context: conflict) |
| `loss_clarify` | **new** | support_select (context: loss) |
| `overwhelm_clarify` | **new** | support_select (context: overwhelm) |
| `support_select` | **new** | vent_open / grounding_exercise / boundary_mode / reassurance_response |
| `dontknow` | existing (expand) | dontknow_body / dontknow_emotion / dontknow_empty / dontknow_all |
| `dontknow_body` | **new** | dontknow_body_sleep / dontknow_body_food / grounding_exercise |
| `dontknow_body_sleep` | **new** | hub_checkin |
| `dontknow_body_food` | **new** | hub_checkin |
| `dontknow_emotion` | existing (fix options) | emotion_anxious / emotion_sad / emotion_angry / dontknow_empty |
| `emotion_anxious` | **new** | support_select (context: anxious) |
| `emotion_sad` | **new** | support_select (context: sad) |
| `emotion_angry` | **new** | support_select (context: angry) |
| `dontknow_empty` | existing | dontknow_empty_guide / grounding_exercise |
| `dontknow_empty_guide` | **new** | hub_checkin |
| `dontknow_all` | **new** | dontknow_body / dontknow_emotion / grounding_exercise |
| `vent_open` | **new** | vent_listening |
| `vent_listening` | **new** | vent_more / vent_reflection / support_redirect |
| `vent_more` | **new** | vent_listening |
| `vent_reflection` | **new** | hub_checkin / support_redirect |
| `support_redirect` | **new** | grounding_exercise / reassurance_response / one_step_prompt / vent_open |
| `boundary_mode` | **new** | boundary_question |
| `boundary_question` | **new** | boundary_own / boundary_release / boundary_both |
| `boundary_own` | **new** | hub_checkin |
| `boundary_release` | **new** | hub_checkin |
| `boundary_both` | **new** | hub_checkin |
| `one_step_prompt` | **new** | one_step_response |
| `one_step_response` | **new** | hub_checkin |
| `grounding_exercise` | existing ✓ | grounding_followup_1 |
| `grounding_followup_1` | existing ✓ | hub_checkin / grounding_exercise_2 |
| `grounding_exercise_2` | existing ✓ | grounding_followup_2 |
| `grounding_followup_2` | existing ✓ | hub_checkin / grounding_failed_response_no / grounding_failed_response_dontknow |
| `grounding_failed_response_no` | existing ✓ | follow_up |
| `grounding_failed_response_dontknow` | existing ✓ | redirect_prelude |
| `follow_up` | existing ✓ | hub_checkin |
| `redirect_prelude` | existing ✓ | hub_redirect |
| `reassurance_response` | existing ✓ | hub_checkin |
| `hub_checkin` | existing (expand) | root / support / session_close |
| `hub_redirect` | existing | root / support / session_close |
| `coming_soon` | existing (placeholder) | hub_checkin |
| `session_close` | existing ✓ | — (end) |

**Total nodes: 17 existing + 27 new = 44 nodes**

---

## Text Keys Holiday Needs to Write

All new keys for `Lines.json`. Existing keys not listed here — they're already written.

### Okay Branch

| Key | Content Direction | Portrait |
|---|---|---|
| `okay.light` | "Let's keep it gentle. One small good thing from today?" | Warm 1 |
| `okay.celebrate.accomplishment` | Celebrates effort + result. "Getting things done while carrying what you carry — that's real." | Warm→Excited low |
| `okay.celebrate.smile` | Celebrates the *noticing*. "You found it. That's not nothing." | Friendly/Warm |
| `okay.celebrate.survival` | Full sincerity. "That IS something. You're here." | Warm 2 |
| `okay.celebrate.anticipation` | "Hold on to that. Having something to look forward to is a form of strength." | Warm/Excited |
| `okay.body.check` | Body first. Water, food, comfortable? | Friendly 1 |
| `okay.body.good` | "Good. That's the foundation. Genuinely okay, or more like functional?" | Friendly 1 |
| `okay.body.partial` | Gentle, not scolding. "Let's close the gap. What's most off?" | Concerned 1 |
| `okay.body.missing` | "The body speaks first. What's most off right now?" | Concerned 1 |

### Something Happened Branch

| Key | Content Direction | Portrait |
|---|---|---|
| `something.disappointment.react` | Pure empathy beat. Validates the sting of things not going your way. NOT analysis. | Concerned 1–2 |
| `something.conflict.react` | Immediate warmth. "Ugh." energy. The kind that stays under the skin. | Concerned 2 |
| `something.loss.react` | Quiet, unhurried. "I'm sorry. Loss is heavy." Don't rush to fix anything. | Sad/Warm |
| `something.overwhelm.react` | Already exists as `something.overwhelm` — keep, just add `next` target |  |
| `disappointment.clarify` | "What's sitting heaviest — what you missed, or what it means going forward?" | Concerned 1 |
| `conflict.clarify` | "What's bothering you most — the situation, or how it made you feel about yourself?" | Concerned 1–2 |
| `loss.clarify` | "Are you more in the grief, or trying to figure out what comes next?" | Sad 1 — present, not fixing |
| `overwhelm.clarify` | "Too much to do, or not enough of you to do it — or both?" | Concerned/Warm 1 |

### Support Selection

| Key | Content Direction | Portrait |
|---|---|---|
| `support_select.conflict` | "I hear you. What would help most right now?" | Warm 1–2 |
| `support_select.disappointment` | "Okay. Given that — what do you need from me?" | Warm 1 |
| `support_select.loss` | "I'm with you. What do you need?" | Warm/Sad — present |
| `support_select.overwhelm` | "That's a lot. What would help most right now?" | Concerned/Warm 1 |
| `support_select.anxious` | Transition from emotion validation to "what do you need" | Calm/Warm |
| `support_select.sad` | Same, gentler tone | Sad/Warm |
| `support_select.angry` | Same, steady not alarmed | Firm/Warm |

*Note: These can all be a single key `support_select.prompt` with textKeyByContext variants if that's cleaner. The distinction matters mostly for Holiday's voice calibration.*

### Don't Know Branch

| Key | Content Direction | Portrait |
|---|---|---|
| `dontknow.body.check` | "Let's start there. Water, food, sleep — what's most off?" | Concerned/Friendly 1 |
| `dontknow.body.sleep` | Sleep debt is real damage, not weakness. No lecture. | Concerned 1 |
| `dontknow.body.food` | Pure care. "Can we get you something?" No guilt. | Warm 1 |
| `dontknow.emotion.narrow` | "If you had to name it loosely — anxious, sad, angry, or numb?" | Warm/Concerned 1 |
| `dontknow.emotion.anxious` | Anxiety as nervous system in overdrive, not personal failure. | Calm 1 |
| `dontknow.emotion.sad` | Sadness doesn't need justification. Just space. | Sad/Warm — present |
| `dontknow.emotion.angry` | Anger as signal. "The part of you that knows you deserved better." | Firm/Warm — steady |
| `dontknow.empty.guide` | Minimum viable human. Small specific task. Water, something soft, something familiar. | Warm/Calm |
| `dontknow.all` | "When everything's off, let's start with the body." Gentle logic, no pressure. | Warm 1 |

### Vent Mode

| Key | Content Direction | Portrait |
|---|---|---|
| `vent.open` | "I'm here. Take all the space you need. Say it as messy as it is." | Warm 2, open |
| `vent.listening` | Brief, non-directive reflection. Just "I hear you." — present, not analyzing. | Concerned/Warm 1–2 |
| `vent.more` | "Keep going. I'm with you." | Warm 1, steady |
| `vent.reflection` | Thanks them for trusting him. Gentle acknowledgment of what was shared. "How are you feeling now?" | Warm 2 |

### Boundary Mode

| Key | Content Direction | Portrait |
|---|---|---|
| `boundary.open` | "Let's sort out what's actually yours to carry here." | Calm/Firm 1 |
| `boundary.question` | "What feels hardest — figuring out what's yours, or letting go of what isn't?" | Concerned 1 |
| `boundary.own` | Naming what genuinely belongs to them. No shame. Owning your part ≠ taking all the blame. | Calm/Warm 1 |
| `boundary.release` | Naming what isn't theirs. Gentle externalization. "What someone else chose — that's theirs." | Warm/Firm 1 |
| `boundary.both` | Complexity acknowledged. Start with whichever end is clearest. | Calm 1 |

### One Step Mode

| Key | Content Direction | Portrait |
|---|---|---|
| `one_step.prompt` | "Smallest thing that would make today 1% easier? Not the list. Just one thing." | Friendly/Warm 1 |
| `one_step.response` | "Okay. Just that. Not the rest — just that one thing. You've got it." | Warm 1, gentle affirm |

### Support Redirect

| Key | Content Direction | Portrait |
|---|---|---|
| `support.redirect` | "Okay. What would help more right now?" — not a failure, just a pivot | Concerned/Warm 1 |

### Session Close (expand variants)

| Key | Content Direction | Portrait |
|---|---|---|
| `session_close` | Already exists — Holiday should expand to 4–5 variants. Real goodbyes, not perfunctory. | Warm 2 |

---

## Portrait Mood Quick Reference

For Holiday's calibration when writing lines:

| Situation | Mood | Intensity | Notes |
|---|---|---|---|
| Opening / warmth | Warm (7) | 1–2 | Default caring presence |
| Celebrating with player | Friendly (1) or Excited (10) | 1 | Keep low — not performative |
| Listening / concerned | Concerned (2) | 1–2 | Increase with severity |
| Grief / loss | Sad (4) | 1–2 | Present, not fixing |
| Anger validation | Firm (3) | 1 | Steady, not alarmed |
| Grounding | Calm (12) | 1 | The regulated nervous system in the room |
| Gentle accountability | Firm (3) or Neutral (0) | 1 | Not cold, just clear |
| Boundaries | Firm (3) → Warm (7) | 1 | Clear then warm — not harsh |
| Goodbye | Warm (7) | 2 | Full close |

---

## Implementation Notes (for dev)

### Existing nodes that need fixes

1. **`something` options** — `responseTextKey` fields are populated but `next` is not set. Add `next` fields pointing to the four clarify nodes.
2. **`dontknow` options** — same issue. Add `next` fields.
3. **`okay` options** — `keep_light` and `quick_checkin` options use `responseTextKey` but have no `next`. Wire to `okay_celebrate` / `okay_body_check`.
4. **`support` node** — expand from 3 options to 4 (add `vent_open`).
5. **`hub_checkin`** — swap "Something else." → "Try something different." pointing to `support`.

### `textKeyByContext` usage

Several new nodes use context-based text variation. The existing `TextKeyByContextEntry` struct handles this. New nodes using it:
- `okay_celebrate` — 4 contexts (accomplishment / smile / survival / anticipation)
- `support_select` — 4+ contexts (conflict / disappointment / loss / overwhelm / anxious / sad / angry)

### vent loop
`vent_open → vent_listening → vent_more → vent_listening` is an intentional loop. The player can cycle as many times as needed. This is by design — venting isn't always one pass.

### No TapToContinue + Options in same node
The current `NodeDef` has either `TapToContinue` (tapContinueNodeId) or `WaitForChoice` (options). Nodes like `emotion_anxious` use TapToContinue to deliver a validation beat before routing to support_select. Nodes like `support_select` use WaitForChoice. These are separate nodes, not combined.

---

*Design by SheDreamsWithAIs. Flow architecture consultation by Aethon. Document authored Claude/Sonnet.*
*Lines to be written by Holiday (Claude/Opus).*
