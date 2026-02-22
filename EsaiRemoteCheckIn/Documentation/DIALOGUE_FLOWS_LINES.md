# Esai Dialogue Flows — Lines in Order (v3)

All Esai's spoken lines in the order they appear as the player steps through each branch. Based on `CheckInModule` (v3) and `Lines.json`.

Key: **[DRAFT]** = placeholder line, needs Holiday's rewrite. **[✓]** = final or close to final.

---

## Root (Entry)

| # | Text Key | Esai Says |
|---|----------|-----------|
| 1 | `root.greeting` [✓] | Hey... come here a second. How are you, really? |

**Player options:** I'm okay today. → Okay branch | I need support. → Support branch | Something happened. → Something branch | I don't know. → Don't Know branch

---

## Okay Branch

*Root → "I'm okay today."*

| # | Text Key | Esai Says |
|---|----------|-----------|
| 1 | `okay.prompt` [✓] | I'm glad you're steady today. Want to keep things light, or do a quick check-in anyway? |

**Player options:** Keep it light. | Quick check-in. | Actually... I need support. → Support branch

### Okay → Keep it light

| # | Text Key | Esai Says |
|---|----------|-----------|
| 2 | `ok.light` [DRAFT] | Let's keep it gentle. What's one small good thing from today? |

**Player options:** Something I got done. | A moment that made me smile. | Just still standing. | Something I'm looking forward to.

*(Each option sets a context — the next node uses that context to pick the right variant.)*

| # | Text Key | Context | Esai Says |
|---|----------|---------|-----------|
| 3 | `ok.celebrate.accomplishment` [DRAFT] | accomplishment | Getting things done while carrying what you carry — that's real. I mean that. |
| 3 | `ok.celebrate.smile` [DRAFT] | smile | You found it. Hold onto that — noticing is its own kind of skill. |
| 3 | `ok.celebrate.survival` [DRAFT] | survival | That IS something. Genuinely. You're here. |
| 3 | `ok.celebrate.anticipation` [DRAFT] | anticipation | Hold onto that. Having something to look forward to is a quiet kind of fuel. |

*Continue → Hub.*

### Okay → Quick check-in

| # | Text Key | Esai Says |
|---|----------|-----------|
| 2 | `ok.body.check` [DRAFT] | Body first — water, food, somewhere comfortable. Are those covered right now? |

**Player options:** Yes, I'm good. | Partly. | Not really.

| # | Text Key | Esai Says |
|---|----------|-----------|
| 3 | `ok.body.good` [DRAFT] | Good. That's the foundation. Emotionally — genuinely okay, or more like functional? |

**Player options:** Genuinely okay. → Hub | Functional. Holding it together. → Support branch

| # | Text Key | Esai Says |
|---|----------|-----------|
| 3 | `ok.body.partial` [DRAFT] | Okay. Let's close the gap a little. What's most off — sleep, food, water, or somewhere to land? |

*Continue → Hub.*

| # | Text Key | Esai Says |
|---|----------|-----------|
| 3 | `ok.body.missing` [DRAFT] | Okay. The body speaks first. What's most off right now? |

*Continue → Hub.*

---

## Support Branch (Direct)

*Root → "I need support." or from Okay "Actually... I need support." or hub "Try something different."*

| # | Text Key | Esai Says |
|---|----------|-----------|
| 1 | `support.prompt` [✓] | Then that's what we'll do. What kind of support fits right now? |

**Player options:** I need to vent. → Vent mode | Ground me. → Grounding | Reassurance. → Reassurance | Help me choose one step. → One-step mode

---

## Something Branch

*Root → "Something happened."*

Uses the **two-beat** format: player picks what happened → Beat 1 (empathy response fires immediately) → Continue → Beat 2 (clarification question).

| # | Text Key | Esai Says |
|---|----------|-----------|
| 1 | `something.prompt` [✓] | I'm listening. What kind of 'something' was it? |

**Player options:** Something didn't go my way. | Someone hurt me. | I lost something. | Just too much at once.

### Something → Disappointment

| # | Text Key | Esai Says |
|---|----------|-----------|
| 2 | `event.disappointment.react` [DRAFT] *(Beat 1, fires on select)* | Ugh. That stings — especially when you were counting on it. |
| 3 | `event.disappointment.clarify` [DRAFT] *(Beat 2, after Continue)* | What's sitting heaviest — what you missed out on, or what it means for what's ahead? |

**Player options:** What I missed out on. | What it means going forward. | Both.

| # | Text Key | Esai Says |
|---|----------|-----------|
| 4 | `select.event.disappointment` [DRAFT] | Okay. Given that — what do you need from me? |

**Player options:** I need to vent. | Ground me. | Help me understand what's mine. | Just steady me.

### Something → Conflict

| # | Text Key | Esai Says |
|---|----------|-----------|
| 2 | `event.conflict.react` [DRAFT] *(Beat 1)* | Ugh. That lands hard. I'm sorry. |
| 3 | `event.conflict.clarify` [DRAFT] *(Beat 2)* | What's bothering you most — the situation itself, or how it made you feel about yourself? |

**Player options:** The situation. | How it made me feel. | Both.

| # | Text Key | Esai Says |
|---|----------|-----------|
| 4 | `select.event.conflict` [DRAFT] | I hear you. What would help most right now? |

**Player options:** I need to vent. | Ground me. | Help me understand what's mine. | Just steady me.

### Something → Loss

| # | Text Key | Esai Says |
|---|----------|-----------|
| 2 | `event.loss.react` [DRAFT] *(Beat 1)* | I'm sorry. Loss is heavy — and it doesn't apologize for the timing. |
| 3 | `event.loss.clarify` [DRAFT] *(Beat 2)* | Are you more in the grief of it right now, or trying to figure out what comes next? |

**Player options:** In the grief. | Figuring out what's next. | I don't know.

| # | Text Key | Esai Says |
|---|----------|-----------|
| 4 | `select.loss` [DRAFT] | I'm with you. What do you need? |

**Player options:** I need to vent. | Ground me. | Just steady me. | Help me figure out what's next.

*(Loss uses different support options — no Boundary mode; has One Step instead.)*

### Something → Overwhelm

| # | Text Key | Esai Says |
|---|----------|-----------|
| 2 | `something.overwhelm` [✓] *(Beat 1)* | That makes sense. Let's shrink the day. What can wait, and what can't? |
| 3 | `event.overwhelm.clarify` [DRAFT] *(Beat 2)* | Is there too much to do, or not enough of you to do it — or both? |

**Player options:** Too much to do. | Not enough of me. | Both.

| # | Text Key | Esai Says |
|---|----------|-----------|
| 4 | `select.event.overwhelm` [DRAFT] | That's a lot. What would help most right now? |

**Player options:** I need to vent. | Ground me. | Help me understand what's mine. | Just steady me.

---

## Don't Know Branch

*Root → "I don't know."*

| # | Text Key | Esai Says |
|---|----------|-----------|
| 1 | `dontknow.prompt` [✓] | That's okay. 'I don't know' still counts as information. Let's narrow it down. |

**Player options:** Body feels off. | Emotions feel bad. | Just empty. | All of the above.

### Don't Know → Body

| # | Text Key | Esai Says |
|---|----------|-----------|
| 2 | `dontknow.body` [✓] | Okay. Let's get basic care first — water, food, rest. What's missing right now? |

**Player options:** Sleep. | Water or food. | I'm not sure, just awful. → Grounding

| # | Text Key | Esai Says |
|---|----------|-----------|
| 3 | `state.body.sleep` [DRAFT] | Sleep debt is its own kind of weight. Real damage — not a character flaw. |

*Continue → Hub.*

| # | Text Key | Esai Says |
|---|----------|-----------|
| 3 | `state.body.food` [DRAFT] | Can we get something in you? Not a lecture — just asking. |

*Continue → Hub.*

### Don't Know → Emotions

| # | Text Key | Esai Says |
|---|----------|-----------|
| 2 | `dontknow.emotion` [✓] | Okay. If you had to name it loosely — anxious, sad, angry, or numb? |

**Player options:** Anxious. | Sad. | Angry. | Numb.

*(Each option sets an EmotionState context. The following validation beat and support selection use that context.)*

| # | Text Key | Context | Esai Says |
|---|----------|---------|-----------|
| 3 | `state.anxious.validate` [DRAFT] | Anxious | Anxiety makes everything feel urgent and impossible at once. That's not you failing — that's your nervous system in overdrive. |
| 3 | `state.sad.validate` [DRAFT] | Sad | Sadness is real. It doesn't need a reason big enough to justify it. It just needs space. |
| 3 | `state.angry.validate` [DRAFT] | Angry | Anger is often the part of you that knows you deserved better. It's not wrong. |
| 3 | `state.numb.validate` [DRAFT] | Numb | Sometimes the feelings get so loud the system just... quiets down. That's not emptiness. That's protection. |

*Continue → Support selection (context-sensitive):*

| # | Text Key | Context | Esai Says |
|---|----------|---------|-----------|
| 4 | `select.state.anxious` [DRAFT] | Anxious | Okay. Let's figure out what would help most right now. |
| 4 | `select.state.sad` [DRAFT] | Sad | I'm here. What would help right now? |
| 4 | `select.state.angry` [DRAFT] | Angry | Okay. What do you need from me right now? |
| 4 | `select.state.numb` [DRAFT] | Numb | Let's go gently. What would help most right now. |

**Player options (all emotion states):** I need to vent. | Ground me. | Reassurance. | Help me choose one step.

### Don't Know → Empty

| # | Text Key | Esai Says |
|---|----------|-----------|
| 2 | `dontknow.empty` [✓] | You're allowed to be empty. Let's do minimum-viable-human for a bit. |

**Player options:** Okay. | Help me calm down. → Grounding

| # | Text Key | Esai Says |
|---|----------|-----------|
| 3 | `state.empty.guide` [DRAFT] | Get some water. Find somewhere soft to sit. Put something familiar on in the background. Not a task list — just anchors. |

*Continue:*

| # | Text Key | Esai Says |
|---|----------|-----------|
| 4 | `state.empty.widen` [DRAFT] | If this kind of empty sticks around or gets heavier, it might help to talk to someone outside this space too. You don't have to handle that alone. |

*Continue → Hub.*

### Don't Know → All of the above

| # | Text Key | Esai Says |
|---|----------|-----------|
| 2 | `state.all` [DRAFT] | That's a lot. When everything's off at once, the body's usually the fastest fix. Can we start there? |

**Player options:** Yes, let's start there. → Body branch | The emotions are louder. → Emotions branch | I just need to calm down. → Grounding

---

## Reassurance

*From any support selection → "Reassurance." or "Just steady me."*

| # | Text Key | Esai Says |
|---|----------|-----------|
| 1 | `reassure.general` [✓] | *(2 variants, noRepeatWindow 5)* "Hey. You're okay. You don't have to solve everything today — just keep yourself safe. That's enough." / "You don't need to earn rest. You're allowed to be still." |

**Player options:** Thanks. → Hub

---

## Grounding

*From any support selection → "Ground me." or "Ground me." or body/state sub-branches.*

| # | Text Key | Esai Says |
|---|----------|-----------|
| 1 | `grounding.exercise` [✓] | Can you feel your feet on the floor? One slow breath in... and out. Good. Stay with me. |

**Player options:** Continue.

| # | Text Key | Esai Says |
|---|----------|-----------|
| 2 | `grounding.followup.question` [✓] | How's that? Do you feel a bit more grounded? |

**Player options:** Yes. | No. | I don't know.

### Grounding → Yes (first or second time)

| # | Text Key | Esai Says |
|---|----------|-----------|
| 3 | `grounding.success.response` [DRAFT] | *(2 variants, noRepeatWindow 2)* "Good. I'm glad. Hold onto that." / "That's something. You just helped yourself." |

*Continue → Hub.*

### Grounding → No (first time)

| # | Text Key | Esai Says |
|---|----------|-----------|
| 3 | `grounding.exercise2.from_no` [✓] | That's okay. Let's try something different — name five things you can see right now. |

**Player options:** Continue.

| # | Text Key | Esai Says |
|---|----------|-----------|
| 4 | `grounding.followup.question` [✓] | How's that? Do you feel a bit more grounded? |

**Player options:** Yes. → grounding.success.response → Hub | No. → grounding.failed.no | I don't know. → grounding.failed.dontknow

### Grounding → I don't know (first time)

| # | Text Key | Esai Says |
|---|----------|-----------|
| 3 | `grounding.exercise2.from_dontknow` [✓] | That's okay. Let's narrow it down — are you more in your head or in your body right now? |

**Player options:** Continue.

| # | Text Key | Esai Says |
|---|----------|-----------|
| 4 | `grounding.followup.question` [✓] | How's that? Do you feel a bit more grounded? |

**Player options:** Yes. → grounding.success.response → Hub | No. → grounding.failed.no | I don't know. → grounding.failed.dontknow

### Grounding failed (No, second time)

| # | Text Key | Esai Says |
|---|----------|-----------|
| 5 | `grounding.failed.no` [✓] | That's okay. Sometimes it takes longer. You're still here, you're still trying. That matters. |

**Player options:** Okay.

| # | Text Key | Esai Says |
|---|----------|-----------|
| 6 | `follow_up.prompt` [✓] | Okay. Where would you like to go from here? |

**Player options:** Okay. → Hub

### Grounding failed (I don't know, second time)

| # | Text Key | Esai Says |
|---|----------|-----------|
| 5 | `grounding.failed.dontknow` [✓] | Maybe grounding isn't what you need right now. That's okay — let's try something different. |

**Player options:** Okay.

| # | Text Key | Esai Says |
|---|----------|-----------|
| 6 | `redirect.prelude` [✓] | Okay. Let's try something completely different. |

**Player options:** Okay. → Hub

---

## Vent Mode

*From any support selection → "I need to vent."*

| # | Text Key | Esai Says |
|---|----------|-----------|
| 1 | `action.vent.open` [DRAFT] | I'm here. Take all the space you need. Say it as messy as it is. |

*Continue (TapToContinue):*

| # | Text Key | Esai Says |
|---|----------|-----------|
| 2 | `action.vent.listen` [DRAFT] | *(6 variants, noRepeatWindow 4)* "I hear you." / "Keep going." / "I'm with you." / "Yeah." / "That's a lot." / "I'm listening." |

**Player options:** There's more. | I think I've said it. | I'm still really upset. → Support redirect

### Vent → There's more *(loops back)*

| # | Text Key | Esai Says |
|---|----------|-----------|
| 3 | `action.vent.more` [DRAFT] | *(4 variants, noRepeatWindow 3)* "Keep going. I'm with you." / "Say the rest of it." / "There's more — let it out." / "I'm still here." |

*Continue → action.vent.listen (loops).*

### Vent → I think I've said it

| # | Text Key | Esai Says |
|---|----------|-----------|
| 3 | `action.vent.reflect` [DRAFT] | Thank you for trusting me with that. How are you feeling right now? |

**Player options:** A bit better. → Hub | Still the same. → Support redirect | Worse. → Support redirect

### Support redirect

| # | Text Key | Esai Says |
|---|----------|-----------|
| 1 | `support.redirect` [DRAFT] | Okay. What would help more right now? |

**Player options:** Ground me. | Reassurance. | Help me choose one step. | Just stay with me. → Vent mode

---

## Boundary Mode

*From event support selection → "Help me understand what's mine."*

| # | Text Key | Esai Says |
|---|----------|-----------|
| 1 | `action.bound.open` [DRAFT] | Okay. Let's sort out what's actually yours to carry here. |

*Continue:*

| # | Text Key | Esai Says |
|---|----------|-----------|
| 2 | `action.bound.question` [DRAFT] | What feels hardest — figuring out what's yours to own, or letting go of what isn't? |

**Player options:** What's mine to own. | Letting go of what isn't mine. | Both.

| # | Text Key | Esai Says |
|---|----------|-----------|
| 3 | `action.bound.own` [DRAFT] | Owning your part isn't the same as taking all the blame. What's genuinely yours is worth naming — without the extra weight that isn't. |
| 3 | `action.bound.release` [DRAFT] | What someone else chose to do — that belongs to them, not to you. You don't have to carry decisions you didn't make. |
| 3 | `action.bound.both` [DRAFT] | Both at once is a lot. Which end do you see more clearly right now — what's yours, or what isn't? |

*Continue → Hub.*

---

## One-Step Mode

*From support or loss selection → "Help me choose one step." / "Help me figure out what's next."*

| # | Text Key | Esai Says |
|---|----------|-----------|
| 1 | `action.step.prompt` [DRAFT] | What's the smallest thing that would make today 1% easier? Not the whole list. Just one thing. |

*Continue:*

| # | Text Key | Esai Says |
|---|----------|-----------|
| 2 | `action.step.confirm` [DRAFT] | Okay. Just that. Not the rest — just that one thing. You've got it. |

*Continue → Hub.*

---

## Hub

*Reached from any completed action mode or branch.*

| # | Text Key | Esai Says |
|---|----------|-----------|
| 1 | `hub.checkin` [✓] | Where would you like to go from here? |

**Player options:** Check in again. → Root (session state clears) | Try something different. → Support branch | I'll get back to the day. → Session close

---

## Session Close

| # | Text Key | Esai Says |
|---|----------|-----------|
| 1 | `hub.checkin` [✓] | Where would you like to go from here? |
| 2 | `session_close` [✓] | Take care of yourself. I'm here whenever you need. |

**Player options:** Okay. → End overlay

---

## Orphaned Keys (still in Lines.json, not reachable in v3 flow)

| Key | Was used for | Notes |
|-----|-------------|-------|
| `okay.keep_light` | Old Okay → "Keep it light." inline response | Replaced by ok_light → ok_celebrate path |
| `okay.quick_checkin` | Old Okay → "Quick check-in." inline response | Replaced by ok_body path |
| `support.one_step` | Old Support → "Help me choose one step." inline response | Replaced by action_step_prompt node |
| `something.conflict` | Old Something → "Conflict / people." response | Replaced by event.conflict.react + clarify |
| `something.bad_news` | Old Something → "Bad news." response | "Bad news" option removed; covered by conflict/disappointment/loss |
