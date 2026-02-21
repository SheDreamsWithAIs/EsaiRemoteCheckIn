# Esai Check-In Mode: Conversation Flow Design (v3)

**Document purpose:** Full node-graph architecture for Check-In Mode.
This is the spec Holiday writes lines against and the dev implements in `CreateCheckInModule.cs`.

**v2 changes from v1:** Internal state model added; `support_select` split into event and state variants; node naming convention established; two-beat pacing formalized as a named design rule; state persistence rules documented.

**v3 changes from v2:** `Numb` and `Empty` split into distinct `EmotionState` values with separate flows; `state_numb` reconnection node added; `state_empty_widen` soft safety-widening beat added (post-stabilization only); `support_select_loss` node introduced (Loss context replaces Boundary with OneStep); Loss context ambiguity resolved; State Reset & Loop Behavior section added; Node Integrity Confirmation added; Context Routing Rule documented.

---

## Design Principles

These are not guidelines — they are structural rules that every node must respect.

### 1. Two-Beat Pacing

Every entry into distress content uses exactly two beats before routing:

- **Beat 1 — Empathy:** Esai responds to *what happened* with pure emotional validation. No analysis, no questions, no reframing. He just meets them where they are. This is implemented as `responseTextKey` on the OptionDef — it fires immediately when the player picks an option.
- **Beat 2 — Clarification:** Esai asks one focused question to understand what they need. This is the destination node that follows Beat 1.

> **Rule:** Beat 1 must never be skipped. Beat 2 must not come before Beat 1.

This pacing prevents Esai from feeling clinical. The player is heard before they're helped.

### 2. Validate the Feeling, Not the Conclusion

Esai validates the player's *emotional reality*, not their *interpretation of events*. He does not know what actually happened or whether the player's conclusions are accurate.

- ✓ "That sounds awful." (validates the pain)
- ✗ "They were wrong." (validates an unverified claim)
- ✓ "That kind of hurt is real." (validates the experience)
- ✗ "You deserved better from them." (takes sides on incomplete information)

Clarifying questions are the tool for this. "What's bothering you most — the situation, or how it made you feel about yourself?" gently separates event from identity conclusion without challenging anything.

### 3. Agency is Preserved at Every Branch

The player always chooses what kind of support they receive. Esai never decides for them. Every action mode is player-initiated. This is the structural expression of the therapeutic principle that client autonomy is load-bearing.

### 4. Every Branch Reaches the Hub

No branch dead-ends. Every path through the tree reaches `hub_checkin` where the player can loop, try something different, or exit. There are no stuck states.

---

## Internal State Model

### Enums

Three state variables are tracked internally. These are conceptually separate — they answer different questions about where the player is in the flow.

```
EventType
  None          — no event context (direct support request, okay branch)
  Conflict      — someone hurt me / interpersonal situation
  Loss          — I lost something
  Disappointment — something didn't go my way
  Overwhelm     — just too much at once

EmotionState
  Unknown       — not yet identified (default for event branch entry)
  Anxious       — player named "anxious"
  Sad           — player named "sad"
  Angry         — player named "angry"
  Numb          — protective shutdown; player named "numb" (routes to state_numb — reconnection work)
  Empty         — persistent flatness, absence of signal; player selected "Just empty" from dontknow (routes to state_empty — stabilization + optional widening)
  Neutral       — player reported okay

SupportType
  None          — not yet chosen
  Vent          — sympathetic ear / listening mode
  Grounding     — somatic grounding exercise
  Reassurance   — validation and steadying
  Boundary      — clarifying what's theirs to carry
  OneStep       — identifying one small next action
```

### Implementation: entryContext vs. Persistent Fields

The existing system has one context mechanism: `entryContext` on `OptionDef`, which sets the context string used by the destination node's `textKeyByContext` lookup. This is **per-navigation** — it only affects the immediately downstream node.

For the current flow, entryContext chaining is sufficient: each option in the chain re-propagates the relevant context value to the next node. The dev sets `entryContext` on each option so the value carries forward through clarification → selection → action.

For future expansion (action mode variants keyed to EventType, hub text keyed to SupportType, etc.), add three persistent fields to `WheelMenuController`:

```csharp
public EventType CurrentEvent { get; private set; } = EventType.None;
public EmotionState CurrentEmotion { get; private set; } = EmotionState.Unknown;
public SupportType CurrentSupport { get; private set; } = SupportType.None;
```

Set these when specific nodes are entered, independent of entryContext. This decouples state tracking from the text-selection mechanism.

### When Variables Are Set

| Variable | Set By | Trigger |
|---|---|---|
| `EventType` | Option in `something` | Player picks Conflict / Loss / Disappointment / Overwhelm |
| `EmotionState` | Option in `state_emotion` | Player picks Anxious / Sad / Angry / Numb |
| `EmotionState` | Option "Just empty." in `dontknow` | Set to Empty (primary Empty path — widening beat eligible) |
| `EmotionState` | Entering `okay_body_good` "genuinely okay" path | Set to Neutral |
| `SupportType` | Option in `support`, `support_select_event`, `support_select_state` | Player picks support type |

### When Variables Are Cleared

| Trigger | Effect |
|---|---|
| `hub_checkin` → "Check in again." → `root` | All three reset to None / Unknown / None |
| `session_close` (end overlay) | All three reset |
| Session launch (fresh start) | All three initialize to defaults |

Variables persist **within** a session loop (through action mode and back to hub) but reset on re-entry to `root`. This means if the player vents about a conflict, reaches hub, chooses "Try something different," and goes into grounding — the EventType = Conflict context is still available for the grounding node's lead-in if the dev adds that variant later.

### How Variables Are Used (Current vs. Future)

| Variable | Current Use | Future Use |
|---|---|---|
| `EventType` | `entryContext` chains to `support_select_event` text variants | Action mode lead-in variants ("after conflict" vs "after loss" framing for grounding, reassurance, etc.) |
| `EmotionState` | `entryContext` chains to `support_select_state` text variants | Same |
| `SupportType` | Not yet used downstream | Hub "how are you feeling after [venting / grounding / etc.]" variants |

---

## Node Naming Convention

### Prefix Rules

All new nodes use a prefix that identifies their branch. This makes the node list scannable and prevents naming drift as content expands.

| Prefix | Branch | Example |
|---|---|---|
| *(none)* | Core / global | `root`, `support`, `hub_checkin` |
| `ok_` | Okay branch | `ok_light`, `ok_body` |
| `event_` | Something happened branch | `event_conflict`, `event_loss` |
| `state_` | Don't know / internal state branch | `state_body`, `state_anxious` |
| `action_vent_` | Vent mode | `action_vent_open`, `action_vent_listen` |
| `action_bound_` | Boundary mode | `action_bound_open`, `action_bound_question` |
| `action_step_` | One-step mode | `action_step_prompt` |
| `support_select_` | Support selection (post-branch) | `support_select_event`, `support_select_state` |

### Stability Rule

**Existing node IDs are never renamed.** Renaming breaks asset references in `CheckInModule.asset`. All nodes listed as "existing ✓" in the reference table keep their exact current IDs regardless of prefix convention. The convention applies to new nodes only.

### Granularity Rule

Node IDs describe the role of the node, not the specific line. `action_bound_own` (not `action_bound_own_clarify_1`) — the node ID names the purpose, not the content. Holiday maps her line variants onto the text key, not the node ID.

---

## Architecture Overview

```
Layer 1 │ State Gate          — "How are you, really?"
Layer 2 │ Branch Selection    — Okay / Need Support / Something Happened / Don't Know
Layer 3 │ Problem Framing     — Two-beat clarification (empathy → one question)
Layer 4 │ Support Selection   — Player chooses support type
         │                      split: support_select_event (EventType context)
         │                             support_select_state (EmotionState context)
         │                             support (no prior context — direct request)
Layer 5 │ Action Mode         — Esai delivers the chosen support
Layer 6 │ Check-In Loop       — Where to next? Loop until natural exit.
```

Key flow invariants:
- State Gate and Branch Selection contain no therapeutic content — they're routing only.
- Problem Framing always uses Beat 1 + Beat 2 structure.
- Support Selection uses `textKeyByContext` for lead-in text; options are identical across variants.
- Action Mode nodes are standalone and reachable from any branch via any selection node.

---

## Full Node Map

---

### Layer 1 — State Gate

---

#### `root` *(existing)*
> **Esai says:** Warm greeting. "How are you, really?" — inviting, not clinical.
> **Portrait:** Warm (7), intensity 1, DirectEyeContact

| Option Label | Goes To | Sets |
|---|---|---|
| "I'm okay today." | `okay` | — |
| "I need support." | `support` | — |
| "Something happened." | `something` | — |
| "I don't know." | `dontknow` | — |

---

### Layer 2A — Okay Branch

---

#### `okay` *(existing)*
> **Esai says:** Glad you're steady today. Keep it light, or a real check-in?
> **Portrait:** Friendly (1), intensity 1

| Option Label | Goes To |
|---|---|
| "Keep it light." | `ok_light` |
| "Quick check-in." | `ok_body` |
| "Actually... I need support." | `support` |

---

#### `ok_light` *(new)*
> **Esai says:** "Let's keep it gentle. One small good thing from today?" — no pressure, celebrating the ordinary.
> **Portrait:** Warm (7), intensity 1

| Option Label | Goes To | entryContext |
|---|---|---|
| "Something I got done." | `ok_celebrate` | `accomplishment` |
| "A moment that made me smile." | `ok_celebrate` | `smile` |
| "Just still standing." | `ok_celebrate` | `survival` |
| "Something I'm looking forward to." | `ok_celebrate` | `anticipation` |

---

#### `ok_celebrate` *(new — textKeyByContext)*
> **Esai says:** [Context-dependent celebration]
> - `accomplishment` → Effort + result. "Getting things done while carrying what you carry — that's real."
> - `smile` → Celebrates the noticing. "You found it. That's not nothing."
> - `survival` → Full sincerity, no irony. "That IS something. You're here."
> - `anticipation` → Warm curiosity. "Hold on to that. Having something to look forward to is a form of strength."
>
> **Portrait:** Warm (7) → Excited (10) low intensity, context-appropriate
> **Advance:** TapToContinue → `hub_checkin`

---

#### `ok_body` *(new)*
> **Esai says:** "Body first — water, food, somewhere comfortable. Are those covered?"
> **Portrait:** Friendly (1), intensity 1, attentive

| Option Label | Goes To |
|---|---|
| "Yes, I'm good." | `ok_body_good` |
| "Partly." | `ok_body_partial` |
| "Not really." | `ok_body_missing` |

---

#### `ok_body_good` *(new)*
> **Esai says:** "Good. That's the foundation. Emotionally — genuinely okay, or more like functional?"
> **Portrait:** Friendly (1), intensity 1

| Option Label | Goes To |
|---|---|
| "Genuinely okay." | `hub_checkin` |
| "Functional. Holding it together." | `support` |

---

#### `ok_body_partial` *(new)*
> **Esai says:** Gentle, no lecture. "Okay, let's close the gap. What's most off — sleep, water/food, or somewhere to land?"
> **Portrait:** Concerned (2), intensity 1
> **Advance:** TapToContinue → `hub_checkin`

---

#### `ok_body_missing` *(new)*
> **Esai says:** "Okay. The body speaks first. What's most off right now?"
> **Portrait:** Concerned (2), intensity 1
> **Advance:** TapToContinue → `hub_checkin`

---

### Layer 2B — Direct Support Branch

---

#### `support` *(existing — modify: add vent option)*
> **Esai says:** "Then that's what we'll do. What kind of support fits right now?"
> **Portrait:** Warm (7), intensity 2
>
> *This node has no prior EventType or EmotionState context. It's a direct support request from root or ok_body_good.*

| Option Label | Goes To |
|---|---|
| "I need to vent." | `action_vent_open` |
| "Ground me." | `grounding_exercise` |
| "Reassurance." | `reassurance_response` |
| "Help me choose one step." | `action_step_prompt` |

---

### Layer 2C — Something Happened Branch

**Two-beat rule in effect.** Each option fires a `responseTextKey` (Beat 1 — empathy) then routes to its clarify node (Beat 2 — one question).

---

#### `something` *(existing — fix dead ends)*
> **Esai says:** "I'm listening. What kind of 'something' was it?"
> **Portrait:** Concerned (2), intensity 1

| Option Label | Beat 1 (responseTextKey) | Beat 2 (next) | Sets EventType |
|---|---|---|---|
| "Something didn't go my way." | `event.disappointment.react` | `event_disappointment` | `Disappointment` |
| "Someone hurt me." | `event.conflict.react` | `event_conflict` | `Conflict` |
| "I lost something." | `event.loss.react` | `event_loss` | `Loss` |
| "Just too much at once." | `something.overwhelm` *(existing key)* | `event_overwhelm` | `Overwhelm` |

> **Implementation note:** The existing `something.overwhelm` key is already in Lines.json and its content is correct — reuse it. Set `entryContext = "Overwhelm"` on this option to propagate EventType through the chain.

---

#### `event_disappointment` *(new)*
> **Esai says:** "What's sitting heaviest — what you missed out on, or what it means for what's ahead?"
> **Portrait:** Concerned (2), intensity 1

| Option Label | Goes To | entryContext |
|---|---|---|
| "What I missed out on." | `support_select_event` | `Disappointment` |
| "What it means going forward." | `support_select_event` | `Disappointment` |
| "Both." | `support_select_event` | `Disappointment` |

*All three options carry the same EventType context — the option sub-choice informs Holiday's line variants for action modes, not the routing.*

---

#### `event_conflict` *(new)*
> **Esai says:** "What's bothering you most — the situation itself, or how it made you feel about yourself?"
> **Portrait:** Concerned (2), intensity 1–2
>
> *Therapeutic note: This question gently separates event from identity conclusion. Esai plants the seed without challenging anything.*

| Option Label | Goes To | entryContext |
|---|---|---|
| "The situation." | `support_select_event` | `Conflict` |
| "How it made me feel." | `support_select_event` | `Conflict` |
| "Both." | `support_select_event` | `Conflict` |

---

#### `event_loss` *(new)*
> **Esai says:** "Are you more in the grief of it right now, or trying to figure out what comes next?"
> **Portrait:** Sad (4), intensity 1 — present with them, not fixing

| Option Label | Goes To | entryContext |
|---|---|---|
| "In the grief." | `support_select_loss` | `Loss` |
| "Figuring out what's next." | `support_select_loss` | `Loss` |
| "I don't know." | `support_select_loss` | `Loss` |

*Loss note: All options route to `support_select_loss` — a dedicated support selection node for Loss context. The Boundary option ("Help me understand what's mine") is replaced with OneStep ("Help me figure out what's next"). See `support_select_loss` below.*

---

#### `event_overwhelm` *(new)*
> **Esai says:** "Is there too much to do, or not enough of you to do it — or both?"
> **Portrait:** Concerned (2), intensity 1, warm

| Option Label | Goes To | entryContext |
|---|---|---|
| "Too much to do." | `support_select_event` | `Overwhelm` |
| "Not enough of me." | `support_select_event` | `Overwhelm` |
| "Both." | `support_select_event` | `Overwhelm` |

*"Not enough of me" is the depletion signal. Same routing, but Holiday should write a gentler lead-in for the Overwhelm context in `support_select_event`.*

---

### Layer 4A — Support Selection (Event Branch)

---

#### `support_select_event` *(new — textKeyByContext)*
> **Esai says:** [Context-dependent transition to support type selection]
> - `Conflict` → "I hear you. What would help most right now?"
> - `Disappointment` → "Okay. Given that — what do you need from me?"
> - `Loss` → "I'm with you. What do you need?"
> - `Overwhelm` → "That's a lot. What would help most right now?"
>
> **Portrait:** Warm (7), intensity 1–2, calm

| Option Label | Goes To | Sets SupportType |
|---|---|---|
| "I need to vent." | `action_vent_open` | `Vent` |
| "Ground me." | `grounding_exercise` | `Grounding` |
| "Help me understand what's mine." | `action_bound_open` | `Boundary` |
| "Just steady me." | `reassurance_response` | `Reassurance` |

---

---

#### `support_select_loss` *(new)*
> **Esai says:** "I'm with you. What do you need?"
> **Portrait:** Warm (7) / Sad (4), intensity 1 — present, not fixing

| Option Label | Goes To | Sets SupportType |
|---|---|---|
| "I need to vent." | `action_vent_open` | `Vent` |
| "Ground me." | `grounding_exercise` | `Grounding` |
| "Just steady me." | `reassurance_response` | `Reassurance` |
| "Help me figure out what's next." | `action_step_prompt` | `OneStep` |

*Loss note: "Help me understand what's mine" (Boundary) is intentionally absent in the loss context. Grief rarely maps to ownership sorting — asking what's "yours to carry" during loss can feel like blame. "Help me figure out what's next" (OneStep) offers forward traction without implying the player should be further along in processing. This is the locked-in approach — `support_select_loss` is the dedicated variant, not a text key change to `support_select_event`.*

---

### Layer 2D — Don't Know Branch

---

#### `dontknow` *(existing — expand options)*
> **Esai says:** "That's okay. 'I don't know' still counts as information. Let's narrow it down."
> **Portrait:** Warm (7), intensity 1

| Option Label | Goes To |
|---|---|
| "Body feels off." | `state_body` |
| "Emotions feel bad." | `state_emotion` |
| "Just empty." | `state_empty` |
| "All of the above." | `state_all` |

> **Implementation note:** Replace the existing `responseTextKey`-only options with these routed options. The existing responseTextKey content (`dontknow.body`, `dontknow.emotion`, `dontknow.empty`) is moved to be the `textKey` of the destination nodes — the content is correct, it just needed a next target.

---

#### `state_body` *(new — reuses existing `dontknow.body` text key)*
> **Esai says:** "Okay. Let's get basic care first — water, food, rest. What's missing right now?"
> **Portrait:** Concerned (2) / Friendly (1), intensity 1

| Option Label | Goes To |
|---|---|
| "Sleep." | `state_body_sleep` |
| "Water or food." | `state_body_food` |
| "I'm not sure, just awful." | `grounding_exercise` |

---

#### `state_body_sleep` *(new)*
> **Esai says:** Sleep debt is real damage, not a character flaw. No lecture. "Sleep debt is its own kind of weight."
> **Portrait:** Concerned (2), intensity 1
> **Advance:** TapToContinue → `hub_checkin`

---

#### `state_body_food` *(new)*
> **Esai says:** Pure care, zero guilt. "Can we get you something? Not a lecture — just asking."
> **Portrait:** Warm (7), intensity 1
> **Advance:** TapToContinue → `hub_checkin`

---

#### `state_emotion` *(new — reuses existing `dontknow.emotion` text key)*
> **Esai says:** "If you had to name it loosely — anxious, sad, angry, or numb?"
> **Portrait:** Warm (7) / Concerned (2), intensity 1

| Option Label | Goes To | Sets EmotionState |
|---|---|---|
| "Anxious." | `state_anxious` | `Anxious` |
| "Sad." | `state_sad` | `Sad` |
| "Angry." | `state_angry` | `Angry` |
| "Numb." | `state_numb` | `Numb` |

---

#### `state_anxious` *(new)*
> **Esai says:** Validates without feeding the spiral. "Anxiety makes everything feel urgent and impossible at once. That's not you failing — that's your nervous system in overdrive."
> **Portrait:** Calm (12), intensity 1
> **Advance:** TapToContinue → `support_select_state` (entryContext: `Anxious`)

---

#### `state_sad` *(new)*
> **Esai says:** Unhurried, no rush to fix. "Sadness is real. It doesn't need a reason big enough to justify it. It just needs space."
> **Portrait:** Sad (4) / Warm (7), intensity 1–2 — present with them
> **Advance:** TapToContinue → `support_select_state` (entryContext: `Sad`)

---

#### `state_angry` *(new)*
> **Esai says:** Anger as signal, not flaw. Steady, not alarmed. "Anger is often the part of you that knows you deserved better. It's not wrong."
> **Portrait:** Firm (3) / Warm (7), intensity 1
> **Advance:** TapToContinue → `support_select_state` (entryContext: `Angry`)

---

#### `state_numb` *(new)*
> **Esai says:** Gentle naming — protective shutdown as the nervous system doing its job. "Sometimes the feelings get so loud the system just... quiets down. That's not emptiness. That's protection."
> **Portrait:** Calm (12) / Warm (7), intensity 1 — regulated, unhurried
>
> **Design intention:** Reconnection work. Sensory anchoring, gentle emotional naming. No escalation language. Numb is something Esai can usually help with directly.
>
> **Advance:** TapToContinue → `support_select_state` (entryContext: `Numb`)

---

#### `state_empty` *(new — reuses existing `dontknow.empty` text key)*
> **Esai says:** "You're allowed to be empty. Let's do minimum-viable-human — just the basics, no guilt."
> **Portrait:** Warm (7), intensity 2
>
> **EmotionState:** Sets to `Empty` when reached from `dontknow` "Just empty." option. This is the **primary Empty path** — the widening beat (`state_empty_widen`) is eligible.
>
> **Design intention:** Minimum viable human stabilization first. No alarm, no diagnostic tone. Then (not immediately) gentle optional widening.

| Option Label | Goes To |
|---|---|
| "Okay." | `state_empty_guide` |
| "Help me calm down." | `grounding_exercise` |

---

#### `state_empty_guide` *(new)*
> **Esai says:** Small, specific, practical. Get water, sit somewhere soft, put on something familiar. A quiet nudge, not a task list.
> **Portrait:** Warm (7) / Calm (12)
> **Advance:** TapToContinue → `state_empty_widen`

---

#### `state_empty_widen` *(new)*
> **Esai says:** Soft, optional-feeling, never alarming. Not diagnostic. No implication of brokenness or defect.
>
> *"If this kind of empty sticks around or gets heavier, it might help to talk to someone outside this space too. You don't have to handle that alone."*
>
> **Portrait:** Warm (7), intensity 1 — gentle, no urgency
>
> **Design intention:** Minimum viable safety widening. This is a seed of permission, not medical advice. It preserves agency by framing external support as an option, not a directive.
>
> **Conditions (must all hold):**
> - This node appears ONLY after `state_empty_guide` (stabilization first — widening is never the first beat)
> - EmotionState = `Empty` must be the **primary state for this session**, set from `dontknow` "Just empty." — not set incidentally during venting or other content
> - If the player reached `state_empty` via `grounding_exercise` redirect (i.e., they chose "Help me calm down." at `state_empty`), the widening beat is bypassed — the grounding path does not run through `state_empty_guide`
>
> **Advance:** TapToContinue → `hub_checkin`

---

#### `state_all` *(new)*
> **Esai says:** "That's a lot. When everything is off, let's start with the body — it's usually the fastest fix. Is that okay?"
> **Portrait:** Warm (7), intensity 1, gentle

| Option Label | Goes To |
|---|---|
| "Yes, let's start there." | `state_body` |
| "The emotions are louder." | `state_emotion` |
| "I just need to calm down." | `grounding_exercise` |

---

### Layer 4B — Support Selection (State Branch)

---

#### `support_select_state` *(new — textKeyByContext)*
> **Esai says:** [Context-dependent transition to support type selection]
> - `Anxious` → Steady, grounding tone. "Okay. Let's figure out what would help most."
> - `Sad` → Gentle, unhurried. "I'm here. What would help right now?"
> - `Angry` → Calm, not alarmed. "Okay. What do you need from me right now?"
> - `Numb` → Gentle, unhurried reconnection tone. "Let's go gently. What would help most right now?"
>
> **Portrait:** Warm (7) / Calm (12), intensity 1, calm

| Option Label | Goes To | Sets SupportType |
|---|---|---|
| "I need to vent." | `action_vent_open` | `Vent` |
| "Ground me." | `grounding_exercise` | `Grounding` |
| "Reassurance." | `reassurance_response` | `Reassurance` |
| "Help me choose one step." | `action_step_prompt` | `OneStep` |

*Note: State branch favors OneStep over Boundary — internal emotional states benefit more from forward traction than boundary sorting. The Boundary option is available via `support_redirect` if needed.*

---

### Layer 5 — Action Modes

---

#### `grounding_exercise` *(existing ✓ — keep entirely)*
> Two-round grounding sequence. Already complete. Routes to `hub_checkin` on success, gentle fallback on failure.

---

#### `reassurance_response` *(existing ✓ — expand content only)*
> One-beat reassurance, then `hub_checkin`. Structure correct.
> Holiday: expand to 10–12 variants with `noRepeatWindow: 8`.

---

#### `action_vent_open` *(new)*
> **Esai says:** "I'm here. Take all the space you need. Say it as messy as it is."
> **Portrait:** Warm (7), intensity 2, open — no judgment
> **Advance:** TapToContinue → `action_vent_listen`

---

#### `action_vent_listen` *(new)*
> **Esai says:** Brief, non-directive. "I hear you." — present, not analyzing, not summarizing.
> **Portrait:** Concerned (2) / Warm (7), intensity 1–2

| Option Label | Goes To |
|---|---|
| "There's more." | `action_vent_more` |
| "I think I've said it." | `action_vent_reflect` |
| "I'm still really upset." | `support_redirect` |

---

#### `action_vent_more` *(new)*
> **Esai says:** "Keep going. I'm with you."
> **Portrait:** Warm (7) / Concerned (2), intensity 1 — steady, holding space
> **Advance:** TapToContinue → `action_vent_listen`

*Intentional loop: `action_vent_more` → `action_vent_listen` cycles as many times as the player needs. Venting is rarely one pass.*

---

#### `action_vent_reflect` *(new)*
> **Esai says:** Thanks them for trusting him. Gentle acknowledgment — not analysis, just witnessing. "Thank you for trusting me with that. How are you feeling right now?"
> **Portrait:** Warm (7), intensity 2

| Option Label | Goes To |
|---|---|
| "A bit better." | `hub_checkin` |
| "Still the same." | `support_redirect` |
| "Worse." | `support_redirect` |

---

#### `support_redirect` *(new)*
> **Esai says:** Gentle pivot. "Okay. What would help more right now?" — not a failure, just a course correction.
> **Portrait:** Concerned (2) / Warm (7), intensity 1

| Option Label | Goes To |
|---|---|
| "Ground me." | `grounding_exercise` |
| "Reassurance." | `reassurance_response` |
| "Help me choose one step." | `action_step_prompt` |
| "Just stay with me." | `action_vent_open` |

*"Just stay with me" re-enters vent mode — the player may need more space before any other mode can land.*

---

#### `action_bound_open` *(new)*
> **Esai says:** "Let's sort out what's actually yours to carry here."
> **Portrait:** Calm (12) / Firm (3), intensity 1 — steady and clear
> **Advance:** TapToContinue → `action_bound_question`

---

#### `action_bound_question` *(new)*
> **Esai says:** "What feels hardest — figuring out what's yours to own, or letting go of what isn't?"
> **Portrait:** Concerned (2), intensity 1

| Option Label | Goes To |
|---|---|
| "What's mine to own." | `action_bound_own` |
| "Letting go of what isn't mine." | `action_bound_release` |
| "Both." | `action_bound_both` |

---

#### `action_bound_own` *(new)*
> **Esai says:** Names what genuinely belongs to them — no shame, with clarity. "Owning your part isn't the same as taking all the blame."
> **Portrait:** Calm (12) / Warm (7), intensity 1
> **Advance:** TapToContinue → `hub_checkin`

---

#### `action_bound_release` *(new)*
> **Esai says:** Names what isn't theirs. Gentle externalization. "What someone else chose to do — that belongs to them, not to you."
> **Portrait:** Warm (7) / Firm (3), intensity 1
> **Advance:** TapToContinue → `hub_checkin`

---

#### `action_bound_both` *(new)*
> **Esai says:** Acknowledges the complexity without drowning in it. "Let's not try to do both at once. Which end do you see more clearly right now?"
> **Portrait:** Calm (12), intensity 1
> **Advance:** TapToContinue → `hub_checkin`

---

#### `action_step_prompt` *(new)*
> **Esai says:** "What's the smallest thing that would make today 1% easier? Not the whole list. Just one thing."
> **Portrait:** Friendly (1) / Warm (7), intensity 1
> **Advance:** TapToContinue → `action_step_confirm`

---

#### `action_step_confirm` *(new)*
> **Esai says:** "Okay. Just that. Not the rest — just that one thing. You've got it."
> **Portrait:** Warm (7), intensity 1, gentle affirm
> **Advance:** TapToContinue → `hub_checkin`

---

### Layer 6 — Check-In Loop

---

#### `hub_checkin` *(existing — modify options)*
> **Esai says:** "Where would you like to go from here?"
> **Portrait:** Neutral (0) / Warm (7), intensity 1

| Option Label | Goes To |
|---|---|
| "Check in again." | `root` *(clears all state)* |
| "Try something different." | `support` |
| "I'll get back to the day." | `session_close` |

---

#### `session_close` *(existing ✓ — expand content)*
> **Esai says:** A real goodbye — warm, unhurried, specific. Not perfunctory.
> **Portrait:** Warm (7), intensity 2
> `triggersEndOverlay: true`
> Holiday: expand to 4–5 variants. These are the last thing a player hears. They should land.

---

## Complete Node Reference

| nodeId | Status | Branch | Leads To |
|---|---|---|---|
| `root` | existing ✓ | Gate | okay / support / something / dontknow |
| `okay` | existing ✓ | Okay | ok_light / ok_body / support |
| `ok_light` | **new** | Okay | ok_celebrate (×4 contexts) |
| `ok_celebrate` | **new** | Okay | hub_checkin |
| `ok_body` | **new** | Okay | ok_body_good / ok_body_partial / ok_body_missing |
| `ok_body_good` | **new** | Okay | hub_checkin / support |
| `ok_body_partial` | **new** | Okay | hub_checkin |
| `ok_body_missing` | **new** | Okay | hub_checkin |
| `support` | existing (add vent) | Support | action_vent_open / grounding_exercise / reassurance_response / action_step_prompt |
| `something` | existing (fix next) | Event | event_disappointment / event_conflict / event_loss / event_overwhelm |
| `event_disappointment` | **new** | Event | support_select_event (context: Disappointment) |
| `event_conflict` | **new** | Event | support_select_event (context: Conflict) |
| `event_loss` | **new** | Event | **support_select_loss** |
| `event_overwhelm` | **new** | Event | support_select_event (context: Overwhelm) |
| `support_select_event` | **new** | Event | action_vent_open / grounding_exercise / action_bound_open / reassurance_response |
| `support_select_loss` | **new** | Event | action_vent_open / grounding_exercise / reassurance_response / action_step_prompt |
| `dontknow` | existing (fix next) | State | state_body / state_emotion / state_empty / state_all |
| `state_body` | **new** | State | state_body_sleep / state_body_food / grounding_exercise |
| `state_body_sleep` | **new** | State | hub_checkin |
| `state_body_food` | **new** | State | hub_checkin |
| `state_emotion` | **new** | State | state_anxious / state_sad / state_angry / **state_numb** |
| `state_anxious` | **new** | State | support_select_state (context: Anxious) |
| `state_sad` | **new** | State | support_select_state (context: Sad) |
| `state_angry` | **new** | State | support_select_state (context: Angry) |
| `state_numb` | **new** | State | support_select_state (context: Numb) |
| `state_empty` | **new** | State | state_empty_guide / grounding_exercise |
| `state_empty_guide` | **new** | State | **state_empty_widen** |
| `state_empty_widen` | **new** | State | hub_checkin |
| `state_all` | **new** | State | state_body / state_emotion / grounding_exercise |
| `support_select_state` | **new** | State | action_vent_open / grounding_exercise / reassurance_response / action_step_prompt |
| `action_vent_open` | **new** | Action | action_vent_listen |
| `action_vent_listen` | **new** | Action | action_vent_more / action_vent_reflect / support_redirect |
| `action_vent_more` | **new** | Action | action_vent_listen *(loop)* |
| `action_vent_reflect` | **new** | Action | hub_checkin / support_redirect |
| `support_redirect` | **new** | Action | grounding_exercise / reassurance_response / action_step_prompt / action_vent_open |
| `action_bound_open` | **new** | Action | action_bound_question |
| `action_bound_question` | **new** | Action | action_bound_own / action_bound_release / action_bound_both |
| `action_bound_own` | **new** | Action | hub_checkin |
| `action_bound_release` | **new** | Action | hub_checkin |
| `action_bound_both` | **new** | Action | hub_checkin |
| `action_step_prompt` | **new** | Action | action_step_confirm |
| `action_step_confirm` | **new** | Action | hub_checkin |
| `grounding_exercise` | existing ✓ | Action | grounding_followup_1 |
| `grounding_followup_1` | existing ✓ | Action | hub_checkin / grounding_exercise_2 |
| `grounding_exercise_2` | existing ✓ | Action | grounding_followup_2 |
| `grounding_followup_2` | existing ✓ | Action | hub_checkin / grounding_failed_response_no / grounding_failed_response_dontknow |
| `grounding_failed_response_no` | existing ✓ | Action | follow_up |
| `grounding_failed_response_dontknow` | existing ✓ | Action | redirect_prelude |
| `follow_up` | existing ✓ | Action | hub_checkin |
| `redirect_prelude` | existing ✓ | Action | hub_redirect |
| `reassurance_response` | existing ✓ | Action | hub_checkin |
| `hub_checkin` | existing (modify) | Hub | root / support / session_close |
| `hub_redirect` | existing ✓ | Hub | root / support / session_close |
| `coming_soon` | existing (placeholder) | — | hub_checkin |
| `session_close` | existing ✓ | — | — (end) |

**Total: 18 existing + 38 new = 56 nodes**
*(v3 additions: `state_numb`, `state_empty_widen`, `support_select_loss` +3 from v2's 52)*

---

## Text Keys Holiday Needs to Write

Existing keys in Lines.json are not listed unless they need changes. Keys prefixed with `event.`, `state.`, `action.` follow the same branch convention as node IDs.

### Okay Branch

| Key | Content Direction | Portrait |
|---|---|---|
| `ok.light` | "Let's keep it gentle. One small good thing from today?" | Warm 1 |
| `ok.celebrate.accomplishment` | Effort + result. "Getting things done while carrying what you carry — that's real." | Warm→Excited low |
| `ok.celebrate.smile` | Celebrates the *noticing*. "You found it. That's not nothing." | Friendly/Warm |
| `ok.celebrate.survival` | Full sincerity. "That IS something. You're here." | Warm 2 |
| `ok.celebrate.anticipation` | "Hold on to that. Having something to look forward to is a form of strength." | Warm/Excited |
| `ok.body.check` | Body first. Water, food, comfortable? | Friendly 1 |
| `ok.body.good` | "Good foundation. Genuinely okay, or more like functional?" | Friendly 1 |
| `ok.body.partial` | Gentle, no scolding. "Let's close the gap." | Concerned 1 |
| `ok.body.missing` | "The body speaks first." | Concerned 1 |

### Event Branch (Something Happened)

| Key | Content Direction | Portrait |
|---|---|---|
| `event.disappointment.react` | **Beat 1.** Pure empathy — the sting of things not going your way. No analysis. | Concerned 1–2 |
| `event.conflict.react` | **Beat 1.** Immediate warmth. "Ugh." energy. | Concerned 2 |
| `event.loss.react` | **Beat 1.** Quiet, unhurried. "I'm sorry. Loss is heavy." Don't rush to fix. | Sad/Warm |
| *(reuse `something.overwhelm`)* | **Beat 1 for overwhelm.** Already in Lines.json. | — |
| `event.disappointment.clarify` | **Beat 2.** "What's sitting heaviest — what you missed, or what it means going forward?" | Concerned 1 |
| `event.conflict.clarify` | **Beat 2.** "What's bothering you most — the situation, or how it made you feel about yourself?" | Concerned 1–2 |
| `event.loss.clarify` | **Beat 2.** "Are you more in the grief, or trying to figure out what comes next?" | Sad 1 — present |
| `event.overwhelm.clarify` | **Beat 2.** "Too much to do, or not enough of you to do it — or both?" | Concerned/Warm 1 |

### Event Support Selection

| Key | Content Direction | Portrait |
|---|---|---|
| `select.event.conflict` | "I hear you. What would help most right now?" | Warm 1–2 |
| `select.event.disappointment` | "Okay. Given that — what do you need from me?" | Warm 1 |
| `select.event.loss` | *(Not used — Loss routes to `support_select_loss`, which has a fixed textKey rather than textKeyByContext)* | — |
| `select.loss` | "I'm with you. What do you need?" — fixed textKey for `support_select_loss` | Warm/Sad — present |
| `select.event.overwhelm` | "That's a lot. What would help most right now?" | Concerned/Warm 1 |

### State Branch (Don't Know)

| Key | Reuse or New | Content Direction | Portrait |
|---|---|---|---|
| *(reuse `dontknow.body`)* | reuse | State body node text — already written | — |
| *(reuse `dontknow.emotion`)* | reuse | State emotion node text — already written | — |
| *(reuse `dontknow.empty`)* | reuse | State empty node text — already written | — |
| `state.body.sleep` | **new** | Sleep debt is real damage, not weakness. No lecture. | Concerned 1 |
| `state.body.food` | **new** | Pure care. "Can we get you something?" Zero guilt. | Warm 1 |
| `state.anxious.validate` | **new** | Anxiety as nervous system in overdrive, not personal failure. | Calm 1 |
| `state.sad.validate` | **new** | Sadness doesn't need justification. Just space. | Sad/Warm — present |
| `state.angry.validate` | **new** | Anger as signal. "The part of you that knows you deserved better." | Firm/Warm — steady |
| `state.numb.validate` | **new** | Protective shutdown as the nervous system doing its job. "Sometimes the feelings get so loud the system just quiets down. That's not emptiness. That's protection." | Calm/Warm 1 |
| `state.empty.guide` | **new** | Minimum viable human. Small specific task. Water, something soft, something familiar. | Warm/Calm |
| `state.empty.widen` | **new** | Soft optional external support mention. "If this kind of empty sticks around or gets heavier, it might help to talk to someone outside this space too. You don't have to handle that alone." No alarm, no diagnostic tone. | Warm 1, gentle |
| `state.all` | **new** | "When everything's off, let's start with the body." Gentle logic, no pressure. | Warm 1 |

### State Support Selection

| Key | Content Direction | Portrait |
|---|---|---|
| `select.state.anxious` | Steady, grounding tone. "Okay. Let's figure out what would help most." | Calm/Warm |
| `select.state.sad` | Gentle, unhurried. "I'm here. What would help right now?" | Sad/Warm |
| `select.state.angry` | Calm, not alarmed. "Okay. What do you need from me right now?" | Firm/Warm |
| `select.state.numb` | Gentle, unhurried reconnection tone. "Let's go gently. What would help most right now?" | Calm/Warm 1 |

### Action Modes

| Key | Content Direction | Portrait |
|---|---|---|
| `action.vent.open` | "I'm here. Take all the space you need. Say it as messy as it is." | Warm 2, open |
| `action.vent.listen` | Brief, non-directive. "I hear you." — present, not analyzing. 6–8 variants, noRepeatWindow: 4 | Concerned/Warm 1–2 |
| `action.vent.more` | "Keep going. I'm with you." 4–6 variants, noRepeatWindow: 3 | Warm 1, steady |
| `action.vent.reflect` | Thanks for trusting. Gentle witnessing. "How are you feeling right now?" | Warm 2 |
| `action.bound.open` | "Let's sort out what's actually yours to carry here." | Calm/Firm 1 |
| `action.bound.question` | "What feels hardest — what's yours to own, or letting go of what isn't?" | Concerned 1 |
| `action.bound.own` | Naming what genuinely belongs to them. No shame. Owning your part ≠ taking all the blame. | Calm/Warm 1 |
| `action.bound.release` | Naming what isn't theirs. Gentle externalization. "What someone else chose — that's theirs." | Warm/Firm 1 |
| `action.bound.both` | Complexity acknowledged. Pick the clearer end first. | Calm 1 |
| `action.step.prompt` | "Smallest thing that would make today 1% easier? Not the list. Just one." | Friendly/Warm 1 |
| `action.step.confirm` | "Okay. Just that. Not the rest — just that one thing. You've got it." | Warm 1, gentle affirm |
| `support.redirect` | "Okay. What would help more right now?" — pivot, not failure | Concerned/Warm 1 |

### Hub and Close (expand variants)

| Key | Content Direction | Portrait |
|---|---|---|
| `session_close` | Expand to 4–5 variants. Real goodbyes. Last thing the player hears. They should land. | Warm 2 |

---

## Portrait Mood Quick Reference

| Situation | Mood | Intensity | Notes |
|---|---|---|---|
| Opening / warmth | Warm (7) | 1–2 | Default caring presence |
| Celebrating with player | Friendly (1) or Excited (10) | 1 | Keep low — not performative |
| Listening / concerned | Concerned (2) | 1–2 | Increase with severity |
| Grief / loss | Sad (4) | 1–2 | Present, not fixing |
| Anger validation | Firm (3) | 1 | Steady, not alarmed |
| Grounding | Calm (12) | 1 | The regulated nervous system in the room |
| Gentle accountability | Firm (3) or Neutral (0) | 1 | Not cold, just clear |
| Boundary clarity | Firm (3) → Warm (7) | 1 | Clear then warm — not harsh |
| Goodbye | Warm (7) | 2 | Full close |

---

## Implementation Notes

### Existing Nodes That Need Fixes

1. **`something` options** — add `next` to each option pointing to `event_disappointment`, `event_conflict`, `event_loss`, `event_overwhelm`. Add `entryContext` on each option matching the EventType enum value.
2. **`dontknow` options** — remove `responseTextKey`, add `next` pointing to `state_body`, `state_emotion`, `state_empty`. Add fourth option for `state_all`.
3. **`okay` options** — `keep_light` and `quick_checkin` options: wire `next` to `ok_light` and `ok_body` respectively.
4. **`support` node** — add "I need to vent." option routing to `action_vent_open`.
5. **`hub_checkin`** — replace "Something else." / `coming_soon` with "Try something different." / `support`.
6. **`hub_redirect`** — same change as hub_checkin.

### entryContext Chaining (short-term implementation)

To carry EventType or EmotionState context from branch entry through clarification to support_select, set `entryContext` on every option in the chain with the same value:

```
// Example: conflict path
something option: entryContext = "Conflict", next = event_conflict
event_conflict option: entryContext = "Conflict", next = support_select_event
// support_select_event uses textKeyByContext to show the "Conflict" lead-in line
```

Each option in the chain re-sets the context. This is explicit and slightly redundant but requires no code changes to the existing system.

### Persistent State Fields (medium-term implementation)

Add to `WheelMenuController`:

```csharp
public EventType CurrentEvent { get; private set; } = EventType.None;
public EmotionState CurrentEmotion { get; private set; } = EmotionState.Unknown;
public SupportType CurrentSupport { get; private set; } = SupportType.None;

private void ClearSessionState()
{
    CurrentEvent = EventType.None;
    CurrentEmotion = EmotionState.Unknown;
    CurrentSupport = SupportType.None;
}
```

Call `ClearSessionState()` when navigating to `root` via "Check in again" and on session start. Set individual variables when entering specific nodes. This decouples persistent state from per-navigation entryContext and enables future action mode variants.

### Text Key Reuse

The existing keys `dontknow.body`, `dontknow.emotion`, `dontknow.empty` are reused as the `textKey` fields of `state_body`, `state_emotion`, `state_empty` respectively. Their content is correct — they just lacked routing.

### Vent Loop Is Intentional

`action_vent_open → action_vent_listen → action_vent_more → action_vent_listen` is a deliberate cycle. Venting is rarely one pass. The loop terminates when the player picks "I think I've said it" or "I'm still upset." There is no forced exit.

### TapToContinue vs. WaitForChoice

Validation beat nodes (`state_anxious`, `state_sad`, `state_angry`, `state_numb`, `action_bound_open`, `action_step_prompt`, etc.) use `TapToContinue` to deliver their content before presenting the next choice. This enforces the two-beat rhythm — Esai speaks, player acknowledges, then chooses. Never the reverse.

---

## State Reset & Loop Behavior

### What Persists Through `hub_checkin`

`hub_checkin` itself does **not** reset state. All three state variables (`CurrentEvent`, `CurrentEmotion`, `CurrentSupport`) persist while the player is at the hub.

| Player Action at Hub | Effect on State |
|---|---|
| "Try something different." → `support` | State **persists.** EventType and EmotionState remain set. |
| "Check in again." → `root` | State **clears.** All three reset to defaults (None / Unknown / None). |
| "I'll get back to the day." → `session_close` | State **clears.** All three reset on session end. |

### What Resets on `session_close`

All three state variables are cleared when `session_close` triggers the end overlay. No state persists across sessions.

### Context in a Second Check-In

If a player selects "Check in again." from `hub_checkin`, they re-enter `root` and all state resets. The second check-in begins from a clean slate — EventType, EmotionState, and SupportType from the first loop are gone.

This is intentional: a second check-in is a fresh emotional start, not a continuation. A player who vented about a conflict, reached the hub, and chose to check in again is starting over — Esai does not carry implicit memory of what was shared.

### Vent Loop State Behavior

The vent loop (`action_vent_open → action_vent_listen → action_vent_more → action_vent_listen`) does **not** mutate state — it holds it. EventType and EmotionState remain what they were when the player entered vent mode. The loop terminates when the player selects "I think I've said it" or "I'm still really upset." There is no forced exit.

Incidental emotional language within the vent loop (e.g., a player mentioning feeling empty while venting about a conflict) does **not** set or change EmotionState. State is only set by explicit node routing, not by content. This is why the `state_empty_widen` widening beat is scoped to the primary Empty path (from `dontknow` → `state_empty`) and never triggers based on what the player says.

---

## Node Integrity Confirmation

### No Dual-Mode Nodes

Confirmed: no node combines `TapToContinue` with choice options. Every node is exclusively one or the other:

- **TapToContinue nodes:** validation beats, stabilization beats, action mode content, widening beat (`state_numb`, `state_anxious`, `state_sad`, `state_angry`, `state_empty_guide`, `state_empty_widen`, `state_body_sleep`, `state_body_food`, `ok_body_partial`, `ok_body_missing`, `ok_celebrate`, `action_vent_open`, `action_vent_more`, `action_bound_open`, `action_step_prompt`, `action_step_confirm`, `action_bound_own`, `action_bound_release`, `action_bound_both`, `grounding_exercise` series)
- **WaitForChoice nodes:** all routing and selection nodes

This is a structural invariant. A node that presents a TapToContinue beat and also offers choice options is a spec violation.

### All Paths Route to Hub or Valid Node

Confirmed: every branch terminates at `hub_checkin`, `session_close`, or a valid intermediate node. Dead-end branches verified:

- `dontknow` → all four options have valid `next` targets (`state_body` / `state_emotion` / `state_empty` / `state_all`) ✓
- `something` → all four options have valid `next` targets with `entryContext` ✓
- `state_emotion` → all four options route cleanly (`state_anxious` / `state_sad` / `state_angry` / **`state_numb`**) ✓
- `state_empty` → both options route cleanly (`state_empty_guide` → `state_empty_widen` → `hub_checkin`; `grounding_exercise` path bypasses widening) ✓
- `event_loss` → routes to **`support_select_loss`** (not `support_select_event`) ✓
- `support_select_loss` → all four options have valid action mode targets ✓
- No node routes to `coming_soon` in the primary flow — `hub_checkin` and `hub_redirect` route to `support` ✓

### No Orphaned Text Keys

Text keys from prior versions reused in current routing:

| Key | Reused As | Status |
|---|---|---|
| `dontknow.body` | `textKey` of `state_body` | ✓ |
| `dontknow.emotion` | `textKey` of `state_emotion` | ✓ |
| `dontknow.empty` | `textKey` of `state_empty` | ✓ |
| `something.overwhelm` | `responseTextKey` on `something` overwhelm option | ✓ |

No placeholder routing (`coming_soon`) remains in primary flow paths.

---

## Context Routing Rule for `support_select` Nodes

### The Rule

Support selection nodes use `textKeyByContext` to vary lead-in text at render time. Choice options are **identical** across context variants. Routing does **not** split by context into different node IDs — one node handles all contexts via `textKeyByContext`.

```
// Correct pattern
node: support_select_event
textKeyByContext:
  Conflict     → select.event.conflict
  Disappointment → select.event.disappointment
  Loss         → [not used — Loss has its own node]
  Overwhelm    → select.event.overwhelm
options: identical across all contexts
```

### The Exception: `support_select_loss`

`support_select_loss` is a dedicated node (not just a context variant of `support_select_event`) because the **choice options themselves differ** — OneStep replaces Boundary in the loss context. This is the only justified reason to create a separate `support_select_` node: when the option set changes, not just the lead-in text.

### Future Contributor Rule

- **Add a text key variant** if you need new context-specific lead-in text for a new EventType or EmotionState.
- **Create a new `support_select_` node** only if the choice options themselves change for that context.
- Do not create a new node just to vary wording. `textKeyByContext` handles that.

### Action Mode Nodes Are Context-Agnostic

Action mode nodes (`action_vent_open`, `grounding_exercise`, `action_step_prompt`, etc.) do not fork by context at the node level. They receive EventType and EmotionState from persistent state fields but use them for Holiday's line variants within those modes, not for architectural routing. Context-aware action mode lead-ins are a future variant — not implemented in v3.

---

*Design by SheDreamsWithAIs. Architecture consultation by Aethon. Document authored by Claude/Sonnet.*
*Lines to be written by Holiday (Claude/Opus).*
