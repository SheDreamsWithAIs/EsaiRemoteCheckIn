# Esai Dialogue Flows — Lines in Order

This document lists all Esai's spoken lines in the order they appear as the player steps through each branch. Based on `CheckInModule.asset` and `Lines.json`. Duplicates appear when multiple branches share the same line.

---

## Root (Entry)

| Order | Text Key | Esai Says |
|-------|----------|-----------|
| 1 | `root.greeting` | Hey... come here a second. How are you, really? |

**Player options:** I'm okay today. | I need support. | Something happened. | I don't know.

---

## Okay Branch

*Path: Root → "I'm okay today."*

| Order | Text Key | Esai Says |
|-------|----------|-----------|
| 1 | `root.greeting` | Hey... come here a second. How are you, really? |
| 2 | `okay.prompt` | I'm glad you're steady today. Want to keep things light, or do a quick check-in anyway? |

**Player options:** Keep it light. | Quick check-in. | Actually... I need support.

### Okay → Keep it light (response only)

| Order | Text Key | Esai Says |
|-------|----------|-----------|
| 1 | `root.greeting` | Hey... come here a second. How are you, really? |
| 2 | `okay.prompt` | I'm glad you're steady today. Want to keep things light, or do a quick check-in anyway? |
| 3 | `okay.keep_light` | Okay. Then let's keep it gentle. One small good thing from today? |

### Okay → Quick check-in (response only)

| Order | Text Key | Esai Says |
|-------|----------|-----------|
| 1 | `root.greeting` | Hey... come here a second. How are you, really? |
| 2 | `okay.prompt` | I'm glad you're steady today. Want to keep things light, or do a quick check-in anyway? |
| 3 | `okay.quick_checkin` | Alright. Body first—water, food, somewhere comfortable. Are those covered? |

### Okay → Actually... I need support

*Continues to Support branch below.*

---

## Support Branch

*Path: Root → "I need support." OR Okay → "Actually... I need support."*

| Order | Text Key | Esai Says |
|-------|----------|-----------|
| 1 | `root.greeting` | Hey... come here a second. How are you, really? |
| 2 | *(if from Okay)* `okay.prompt` | I'm glad you're steady today. Want to keep things light, or do a quick check-in anyway? |
| 3 | `support.prompt` | Then that's what we'll do. What kind of support fits right now? |

**Player options:** Reassurance. | Grounding. | Help me choose one step.

### Support → Help me choose one step (response only)

| Order | Text Key | Esai Says |
|-------|----------|-----------|
| 1 | `root.greeting` | Hey... come here a second. How are you, really? |
| 2 | `support.prompt` | Then that's what we'll do. What kind of support fits right now? |
| 3 | `support.one_step` | Okay. What's the smallest next thing that would make today 1% easier? |

### Support → Reassurance

*Continues to Reassurance branch below.*

### Support → Grounding

*Continues to Grounding branch below.*

---

## Reassurance Branch

*Path: Support → "Reassurance."*

| Order | Text Key | Esai Says |
|-------|----------|-----------|
| 1 | `root.greeting` | Hey... come here a second. How are you, really? |
| 2 | `support.prompt` | Then that's what we'll do. What kind of support fits right now? |
| 3 | `reassure.general` | *(variants)* Hey. You're okay. You don't have to solve everything today—just keep yourself safe. That's enough. / You don't need to earn rest. You're allowed to be still. |

**Player options:** Thanks.

*Next: Hub (Check in again).*

---

## Grounding Branch

*Path: Support → "Grounding."*

| Order | Text Key | Esai Says |
|-------|----------|-----------|
| 1 | `root.greeting` | Hey... come here a second. How are you, really? |
| 2 | `support.prompt` | Then that's what we'll do. What kind of support fits right now? |
| 3 | `grounding.exercise` | Can you feel your feet on the floor? One slow breath in... and out. Good. Stay with me. |

**Player options:** Continue.

| Order | Text Key | Esai Says |
|-------|----------|-----------|
| 4 | `grounding.followup.question` | How's that? Do you feel a bit more grounded? |

**Player options:** Yes. | No. | I don't know.

### Grounding → Yes (first time)

*Next: Hub.*

### Grounding → No (first time)

| Order | Text Key | Esai Says |
|-------|----------|-----------|
| 5 | `grounding.exercise2.from_no` | That's okay. Let's try something different—name five things you can see right now. |

**Player options:** Continue.

| Order | Text Key | Esai Says |
|-------|----------|-----------|
| 6 | `grounding.followup.question` | How's that? Do you feel a bit more grounded? |

**Player options:** Yes. | No. | I don't know.

#### No → Yes (second time)

*Next: Hub.*

#### No → No (second time)

| Order | Text Key | Esai Says |
|-------|----------|-----------|
| 7 | `grounding.failed.no` | That's okay. Sometimes it takes longer. You're still here, you're still trying. That matters. |

**Player options:** Okay.

*Next: Follow-up → Hub.*

#### No → I don't know (second time)

| Order | Text Key | Esai Says |
|-------|----------|-----------|
| 7 | `grounding.failed.dontknow` | Maybe grounding isn't what you need right now. That's okay—let's try something different. |

**Player options:** Okay.

*Next: Redirect prelude → Hub redirect.*

### Grounding → I don't know (first time)

| Order | Text Key | Esai Says |
|-------|----------|-----------|
| 5 | `grounding.exercise2.from_dontknow` | That's okay. Let's narrow it down—are you more in your head or in your body right now? |

**Player options:** Continue.

| Order | Text Key | Esai Says |
|-------|----------|-----------|
| 6 | `grounding.followup.question` | How's that? Do you feel a bit more grounded? |

*Same Yes / No / I don't know options as above.*

---

## Something Branch

*Path: Root → "Something happened."*

| Order | Text Key | Esai Says |
|-------|----------|-----------|
| 1 | `root.greeting` | Hey... come here a second. How are you, really? |
| 2 | `something.prompt` | I'm listening. What kind of 'something' was it? |

**Player options:** Conflict / people. | Bad news. | Overwhelm / too much.

### Something → Conflict / people (response only)

| Order | Text Key | Esai Says |
|-------|----------|-----------|
| 1 | `root.greeting` | Hey... come here a second. How are you, really? |
| 2 | `something.prompt` | I'm listening. What kind of 'something' was it? |
| 3 | `something.conflict` | Ugh. I'm sorry. Do you need to vent first, or would grounding help more right now? |

### Something → Bad news (response only)

| Order | Text Key | Esai Says |
|-------|----------|-----------|
| 1 | `root.greeting` | Hey... come here a second. How are you, really? |
| 2 | `something.prompt` | I'm listening. What kind of 'something' was it? |
| 3 | `something.bad_news` | Okay. That's heavy. You don't have to carry it alone—tell me what happened. |

### Something → Overwhelm / too much (response only)

| Order | Text Key | Esai Says |
|-------|----------|-----------|
| 1 | `root.greeting` | Hey... come here a second. How are you, really? |
| 2 | `something.prompt` | I'm listening. What kind of 'something' was it? |
| 3 | `something.overwhelm` | That makes sense. Let's shrink the day. What can wait, and what can't? |

---

## Don't Know Branch

*Path: Root → "I don't know."*

| Order | Text Key | Esai Says |
|-------|----------|-----------|
| 1 | `root.greeting` | Hey... come here a second. How are you, really? |
| 2 | `dontknow.prompt` | That's okay. 'I don't know' still counts as information. Let's narrow it down. |

**Player options:** Body feels bad. | Emotion feels bad. | Just empty.

### Don't Know → Body feels bad (response only)

| Order | Text Key | Esai Says |
|-------|----------|-----------|
| 1 | `root.greeting` | Hey... come here a second. How are you, really? |
| 2 | `dontknow.prompt` | That's okay. 'I don't know' still counts as information. Let's narrow it down. |
| 3 | `dontknow.body` | Okay. Let's get basic care first—water, food, rest. What's missing right now? |

### Don't Know → Emotion feels bad (response only)

| Order | Text Key | Esai Says |
|-------|----------|-----------|
| 1 | `root.greeting` | Hey... come here a second. How are you, really? |
| 2 | `dontknow.prompt` | That's okay. 'I don't know' still counts as information. Let's narrow it down. |
| 3 | `dontknow.emotion` | Okay. If you had to name it loosely—anxious, sad, angry, or numb? |

### Don't Know → Just empty (response only)

| Order | Text Key | Esai Says |
|-------|----------|-----------|
| 1 | `root.greeting` | Hey... come here a second. How are you, really? |
| 2 | `dontknow.prompt` | That's okay. 'I don't know' still counts as information. Let's narrow it down. |
| 3 | `dontknow.empty` | You're allowed to be empty. Let's do minimum-viable-human for a bit. |

---

## Follow-up (from Grounding → No → No → Okay)

*Path: Grounding failed (No) → "Okay."*

| Order | Text Key | Esai Says |
|-------|----------|-----------|
| 1 | `root.greeting` | Hey... come here a second. How are you, really? |
| 2 | `support.prompt` | Then that's what we'll do. What kind of support fits right now? |
| 3 | `grounding.exercise` | Can you feel your feet on the floor? One slow breath in... and out. Good. Stay with me. |
| 4 | `grounding.followup.question` | How's that? Do you feel a bit more grounded? |
| 5 | `grounding.exercise2.from_no` | That's okay. Let's try something different—name five things you can see right now. |
| 6 | `grounding.followup.question` | How's that? Do you feel a bit more grounded? |
| 7 | `grounding.failed.no` | That's okay. Sometimes it takes longer. You're still here, you're still trying. That matters. |
| 8 | `follow_up.prompt` | Okay. Where would you like to go from here? |

**Player options:** Okay.

*Next: Hub.*

---

## Redirect Prelude (from Grounding → I don't know → I don't know → Okay)

*Path: Grounding failed (I don't know) → "Okay."*

| Order | Text Key | Esai Says |
|-------|----------|-----------|
| 1 | `root.greeting` | Hey... come here a second. How are you, really? |
| 2 | `support.prompt` | Then that's what we'll do. What kind of support fits right now? |
| 3 | `grounding.exercise` | Can you feel your feet on the floor? One slow breath in... and out. Good. Stay with me. |
| 4 | `grounding.followup.question` | How's that? Do you feel a bit more grounded? |
| 5 | `grounding.exercise2.from_dontknow` | That's okay. Let's narrow it down—are you more in your head or in your body right now? |
| 6 | `grounding.followup.question` | How's that? Do you feel a bit more grounded? |
| 7 | `grounding.failed.dontknow` | Maybe grounding isn't what you need right now. That's okay—let's try something different. |
| 8 | `redirect.prelude` | Okay. Let's try something completely different. |

**Player options:** Okay.

*Next: Hub redirect.*

---

## Hub (Check in again)

*Path: From Reassurance, Grounding (Yes), Follow-up, or direct from Hub.*

| Order | Text Key | Esai Says |
|-------|----------|-----------|
| 1 | `hub.checkin` | Where would you like to go from here? |

**Player options:** Check in again. | I'll get back to the day. | Something else.

*Note: `hub.checkin` is also used by `hub_redirect` (same line).*

---

## Hub Redirect

*Path: From Redirect prelude.*

| Order | Text Key | Esai Says |
|-------|----------|-----------|
| 1 | `hub.checkin` | Where would you like to go from here? |

**Player options:** Check in again. | I'll get back to the day. | Something else.

---

## Coming Soon

*Path: Hub or Hub redirect → "Something else."*

| Order | Text Key | Esai Says |
|-------|----------|-----------|
| 1 | `hub.checkin` | Where would you like to go from here? |
| 2 | `coming_soon` | That part isn't ready yet—but it's coming. For now, let's work with what we have. |

**Player options:** Back.

*Next: Hub.*

---

## Session Close

*Path: Hub or Hub redirect → "I'll get back to the day."*

| Order | Text Key | Esai Says |
|-------|----------|-----------|
| 1 | `hub.checkin` | Where would you like to go from here? |
| 2 | `session_close` | Take care of yourself. I'm here whenever you need. |

*Triggers end overlay. No further options.*
