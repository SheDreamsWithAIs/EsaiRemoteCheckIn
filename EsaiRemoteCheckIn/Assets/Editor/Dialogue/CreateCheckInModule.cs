using UnityEditor;
using UnityEngine;

/// <summary>
/// Creates (or recreates) CheckInModule.asset with the full v3 Check-In flow.
/// See Docs/CheckInFlowDesign.md for the complete node-graph spec.
/// Lines are drafts — Holiday (Claude/Opus) writes the final content against text keys in Lines.json.
/// </summary>
public static class CreateCheckInModule
{
    private const string AssetPath = "Assets/ScriptableObjects/Dialogue/Modules/CheckInModule.asset";

    [MenuItem("Esai/Create Check-In Module (Full v3 Flow)")]
    public static void Create()
    {
        // Delete existing asset so this menu item is always idempotent.
        var existing = AssetDatabase.LoadAssetAtPath<DialogueModuleSO>(AssetPath);
        if (existing != null)
        {
            AssetDatabase.DeleteAsset(AssetPath);
            Debug.Log($"Deleted existing CheckInModule at {AssetPath} — recreating with v3 flow.");
        }

        var module = ScriptableObject.CreateInstance<DialogueModuleSO>();
        module.moduleId = "checkin";
        module.entryNodeId = "root";

        module.nodes = new[]
        {
            // ─────────────────────────────────────────────────────────────────
            // LAYER 1 — STATE GATE
            // ─────────────────────────────────────────────────────────────────

            new NodeDef
            {
                nodeId  = "root",
                textKey = "root.greeting",
                options = new[]
                {
                    new OptionDef { labelText = "I'm okay today.",      next = "okay"      },
                    new OptionDef { labelText = "I need support.",       next = "support"   },
                    new OptionDef { labelText = "Something happened.",   next = "something" },
                    new OptionDef { labelText = "I don't know.",         next = "dontknow"  }
                }
            },

            // ─────────────────────────────────────────────────────────────────
            // LAYER 2A — OKAY BRANCH
            // ─────────────────────────────────────────────────────────────────

            new NodeDef
            {
                nodeId  = "okay",
                textKey = "okay.prompt",
                options = new[]
                {
                    new OptionDef { labelText = "Keep it light.",                next = "ok_light" },
                    new OptionDef { labelText = "Quick check-in.",               next = "ok_body"  },
                    new OptionDef { labelText = "Actually... I need support.",   next = "support"  }
                }
            },
            new NodeDef
            {
                nodeId  = "ok_light",
                textKey = "ok.light",
                options = new[]
                {
                    new OptionDef { labelText = "Something I got done.",              next = "ok_celebrate", entryContext = "accomplishment" },
                    new OptionDef { labelText = "A moment that made me smile.",       next = "ok_celebrate", entryContext = "smile"          },
                    new OptionDef { labelText = "Just still standing.",               next = "ok_celebrate", entryContext = "survival"       },
                    new OptionDef { labelText = "Something I'm looking forward to.",  next = "ok_celebrate", entryContext = "anticipation"   }
                }
            },
            new NodeDef
            {
                // textKeyByContext resolves based on entryContext from ok_light options.
                nodeId       = "ok_celebrate",
                advanceMode  = AdvanceModeDef.TapToContinue,
                tapContinueNodeId = "hub_checkin",
                textKeyByContext = new[]
                {
                    new TextKeyByContextEntry { context = "accomplishment", textKey = "ok.celebrate.accomplishment" },
                    new TextKeyByContextEntry { context = "smile",          textKey = "ok.celebrate.smile"          },
                    new TextKeyByContextEntry { context = "survival",       textKey = "ok.celebrate.survival"       },
                    new TextKeyByContextEntry { context = "anticipation",   textKey = "ok.celebrate.anticipation"   }
                }
            },
            new NodeDef
            {
                nodeId  = "ok_body",
                textKey = "ok.body.check",
                options = new[]
                {
                    new OptionDef { labelText = "Yes, I'm good.",        next = "ok_body_good"    },
                    new OptionDef { labelText = "Partly.",               next = "ok_body_partial" },
                    new OptionDef { labelText = "Not really.",           next = "ok_body_missing" }
                }
            },
            new NodeDef
            {
                nodeId  = "ok_body_good",
                textKey = "ok.body.good",
                options = new[]
                {
                    new OptionDef { labelText = "Genuinely okay.",              next = "hub_checkin" },
                    new OptionDef { labelText = "Functional. Holding it together.", next = "support" }
                }
            },
            new NodeDef
            {
                nodeId            = "ok_body_partial",
                advanceMode       = AdvanceModeDef.TapToContinue,
                tapContinueNodeId = "hub_checkin",
                textKey           = "ok.body.partial"
            },
            new NodeDef
            {
                nodeId            = "ok_body_missing",
                advanceMode       = AdvanceModeDef.TapToContinue,
                tapContinueNodeId = "hub_checkin",
                textKey           = "ok.body.missing"
            },

            // ─────────────────────────────────────────────────────────────────
            // LAYER 2B — DIRECT SUPPORT BRANCH
            // ─────────────────────────────────────────────────────────────────

            new NodeDef
            {
                nodeId  = "support",
                textKey = "support.prompt",
                options = new[]
                {
                    new OptionDef { labelText = "I need to vent.",          next = "action_vent_open"     },
                    new OptionDef { labelText = "Ground me.",               next = "grounding_exercise"   },
                    new OptionDef { labelText = "Reassurance.",             next = "reassurance_response" },
                    new OptionDef { labelText = "Help me choose one step.", next = "action_step_prompt"   }
                }
            },

            // ─────────────────────────────────────────────────────────────────
            // LAYER 2C — SOMETHING HAPPENED BRANCH
            // Two-beat rule: responseTextKey = Beat 1 (empathy), next = Beat 2 (clarify node).
            // ─────────────────────────────────────────────────────────────────

            new NodeDef
            {
                nodeId  = "something",
                textKey = "something.prompt",
                options = new[]
                {
                    new OptionDef { labelText = "Something didn't go my way.", responseTextKey = "event.disappointment.react", next = "event_disappointment", entryContext = "Disappointment" },
                    new OptionDef { labelText = "Someone hurt me.",            responseTextKey = "event.conflict.react",       next = "event_conflict",       entryContext = "Conflict"       },
                    new OptionDef { labelText = "I lost something.",           responseTextKey = "event.loss.react",           next = "event_loss",           entryContext = "Loss"           },
                    new OptionDef { labelText = "Just too much at once.",      responseTextKey = "something.overwhelm",        next = "event_overwhelm",      entryContext = "Overwhelm"      }
                }
            },
            new NodeDef
            {
                // Beat 2 — Disappointment clarification.
                nodeId  = "event_disappointment",
                textKey = "event.disappointment.clarify",
                options = new[]
                {
                    new OptionDef { labelText = "What I missed out on.",     next = "support_select_event", entryContext = "Disappointment" },
                    new OptionDef { labelText = "What it means going forward.", next = "support_select_event", entryContext = "Disappointment" },
                    new OptionDef { labelText = "Both.",                     next = "support_select_event", entryContext = "Disappointment" }
                }
            },
            new NodeDef
            {
                // Beat 2 — Conflict clarification. Separates event from identity conclusion.
                nodeId  = "event_conflict",
                textKey = "event.conflict.clarify",
                options = new[]
                {
                    new OptionDef { labelText = "The situation.",        next = "support_select_event", entryContext = "Conflict" },
                    new OptionDef { labelText = "How it made me feel.",  next = "support_select_event", entryContext = "Conflict" },
                    new OptionDef { labelText = "Both.",                 next = "support_select_event", entryContext = "Conflict" }
                }
            },
            new NodeDef
            {
                // Beat 2 — Loss clarification. Routes to support_select_loss (not support_select_event).
                nodeId  = "event_loss",
                textKey = "event.loss.clarify",
                options = new[]
                {
                    new OptionDef { labelText = "In the grief.",             next = "support_select_loss", entryContext = "Loss" },
                    new OptionDef { labelText = "Figuring out what's next.", next = "support_select_loss", entryContext = "Loss" },
                    new OptionDef { labelText = "I don't know.",             next = "support_select_loss", entryContext = "Loss" }
                }
            },
            new NodeDef
            {
                // Beat 2 — Overwhelm clarification.
                nodeId  = "event_overwhelm",
                textKey = "event.overwhelm.clarify",
                options = new[]
                {
                    new OptionDef { labelText = "Too much to do.",       next = "support_select_event", entryContext = "Overwhelm" },
                    new OptionDef { labelText = "Not enough of me.",     next = "support_select_event", entryContext = "Overwhelm" },
                    new OptionDef { labelText = "Both.",                 next = "support_select_event", entryContext = "Overwhelm" }
                }
            },

            // ─────────────────────────────────────────────────────────────────
            // LAYER 4A — SUPPORT SELECTION (EVENT BRANCH)
            // textKeyByContext varies lead-in text; options are identical across contexts.
            // ─────────────────────────────────────────────────────────────────

            new NodeDef
            {
                nodeId = "support_select_event",
                textKeyByContext = new[]
                {
                    new TextKeyByContextEntry { context = "Conflict",       textKey = "select.event.conflict"       },
                    new TextKeyByContextEntry { context = "Disappointment", textKey = "select.event.disappointment" },
                    new TextKeyByContextEntry { context = "Overwhelm",      textKey = "select.event.overwhelm"      }
                },
                options = new[]
                {
                    new OptionDef { labelText = "I need to vent.",              next = "action_vent_open"     },
                    new OptionDef { labelText = "Ground me.",                   next = "grounding_exercise"   },
                    new OptionDef { labelText = "Help me understand what's mine.", next = "action_bound_open" },
                    new OptionDef { labelText = "Just steady me.",              next = "reassurance_response" }
                }
            },

            // Loss uses a dedicated node — options differ (OneStep replaces Boundary).
            // See CheckInFlowDesign.md: Context Routing Rule — The Exception.
            new NodeDef
            {
                nodeId  = "support_select_loss",
                textKey = "select.loss",
                options = new[]
                {
                    new OptionDef { labelText = "I need to vent.",                  next = "action_vent_open"     },
                    new OptionDef { labelText = "Ground me.",                       next = "grounding_exercise"   },
                    new OptionDef { labelText = "Just steady me.",                  next = "reassurance_response" },
                    new OptionDef { labelText = "Help me figure out what's next.",  next = "action_step_prompt"   }
                }
            },

            // ─────────────────────────────────────────────────────────────────
            // LAYER 2D — DON'T KNOW BRANCH
            // ─────────────────────────────────────────────────────────────────

            new NodeDef
            {
                nodeId  = "dontknow",
                textKey = "dontknow.prompt",
                options = new[]
                {
                    new OptionDef { labelText = "Body feels off.",    next = "state_body"    },
                    new OptionDef { labelText = "Emotions feel bad.", next = "state_emotion" },
                    new OptionDef { labelText = "Just empty.",        next = "state_empty"   },
                    new OptionDef { labelText = "All of the above.",  next = "state_all"     }
                }
            },
            new NodeDef
            {
                // Reuses existing dontknow.body text key — content is correct, just needed routing.
                nodeId  = "state_body",
                textKey = "dontknow.body",
                options = new[]
                {
                    new OptionDef { labelText = "Sleep.",                       next = "state_body_sleep"   },
                    new OptionDef { labelText = "Water or food.",               next = "state_body_food"    },
                    new OptionDef { labelText = "I'm not sure, just awful.",    next = "grounding_exercise" }
                }
            },
            new NodeDef
            {
                nodeId            = "state_body_sleep",
                advanceMode       = AdvanceModeDef.TapToContinue,
                tapContinueNodeId = "hub_checkin",
                textKey           = "state.body.sleep"
            },
            new NodeDef
            {
                nodeId            = "state_body_food",
                advanceMode       = AdvanceModeDef.TapToContinue,
                tapContinueNodeId = "hub_checkin",
                textKey           = "state.body.food"
            },
            new NodeDef
            {
                // Reuses existing dontknow.emotion text key.
                nodeId  = "state_emotion",
                textKey = "dontknow.emotion",
                options = new[]
                {
                    new OptionDef { labelText = "Anxious.", next = "state_anxious", entryContext = "Anxious" },
                    new OptionDef { labelText = "Sad.",     next = "state_sad",     entryContext = "Sad"     },
                    new OptionDef { labelText = "Angry.",   next = "state_angry",   entryContext = "Angry"   },
                    new OptionDef { labelText = "Numb.",    next = "state_numb",    entryContext = "Numb"    }
                }
            },
            new NodeDef
            {
                // Validation: anxiety as nervous system in overdrive, not personal failure.
                // TapToContinue preserves entryContext "Anxious" for support_select_state.
                nodeId            = "state_anxious",
                advanceMode       = AdvanceModeDef.TapToContinue,
                tapContinueNodeId = "support_select_state",
                textKey           = "state.anxious.validate"
            },
            new NodeDef
            {
                // Validation: sadness doesn't need justification, just space.
                nodeId            = "state_sad",
                advanceMode       = AdvanceModeDef.TapToContinue,
                tapContinueNodeId = "support_select_state",
                textKey           = "state.sad.validate"
            },
            new NodeDef
            {
                // Validation: anger as signal — the part that knows you deserved better.
                nodeId            = "state_angry",
                advanceMode       = AdvanceModeDef.TapToContinue,
                tapContinueNodeId = "support_select_state",
                textKey           = "state.angry.validate"
            },
            new NodeDef
            {
                // Validation: numb as protective shutdown, not emptiness.
                // Design intention: reconnection work — sensory anchoring, gentle naming.
                // No escalation language. Numb is something Esai can usually help with directly.
                nodeId            = "state_numb",
                advanceMode       = AdvanceModeDef.TapToContinue,
                tapContinueNodeId = "support_select_state",
                textKey           = "state.numb.validate"
            },
            new NodeDef
            {
                // Reuses existing dontknow.empty text key.
                // EmotionState = Empty is set here (primary Empty path — widening beat eligible).
                // Meaning protection: Empty is a signal state, not a defect state.
                nodeId  = "state_empty",
                textKey = "dontknow.empty",
                options = new[]
                {
                    new OptionDef { labelText = "Okay.",              next = "state_empty_guide"  },
                    new OptionDef { labelText = "Help me calm down.", next = "grounding_exercise" }
                }
            },
            new NodeDef
            {
                // Stabilization: minimum viable human — water, somewhere soft, something familiar.
                nodeId            = "state_empty_guide",
                advanceMode       = AdvanceModeDef.TapToContinue,
                tapContinueNodeId = "state_empty_widen",
                textKey           = "state.empty.guide"
            },
            new NodeDef
            {
                // Soft safety widening — post-stabilization only, never first beat.
                // Conditions: primary Empty path only (not incidental emptiness during venting).
                // Tone: seed of permission, not directive. No alarm, no diagnostic language.
                nodeId            = "state_empty_widen",
                advanceMode       = AdvanceModeDef.TapToContinue,
                tapContinueNodeId = "hub_checkin",
                textKey           = "state.empty.widen"
            },
            new NodeDef
            {
                // When everything is off at once — route to body first (fastest fix).
                nodeId  = "state_all",
                textKey = "state.all",
                options = new[]
                {
                    new OptionDef { labelText = "Yes, let's start there.",   next = "state_body"        },
                    new OptionDef { labelText = "The emotions are louder.",   next = "state_emotion"     },
                    new OptionDef { labelText = "I just need to calm down.",  next = "grounding_exercise" }
                }
            },

            // ─────────────────────────────────────────────────────────────────
            // LAYER 4B — SUPPORT SELECTION (STATE BRANCH)
            // textKeyByContext varies lead-in; options identical across contexts.
            // ─────────────────────────────────────────────────────────────────

            new NodeDef
            {
                nodeId = "support_select_state",
                textKeyByContext = new[]
                {
                    new TextKeyByContextEntry { context = "Anxious", textKey = "select.state.anxious" },
                    new TextKeyByContextEntry { context = "Sad",     textKey = "select.state.sad"     },
                    new TextKeyByContextEntry { context = "Angry",   textKey = "select.state.angry"   },
                    new TextKeyByContextEntry { context = "Numb",    textKey = "select.state.numb"    }
                },
                // State branch: OneStep replaces Boundary (internal states benefit from forward traction).
                options = new[]
                {
                    new OptionDef { labelText = "I need to vent.",          next = "action_vent_open"     },
                    new OptionDef { labelText = "Ground me.",               next = "grounding_exercise"   },
                    new OptionDef { labelText = "Reassurance.",             next = "reassurance_response" },
                    new OptionDef { labelText = "Help me choose one step.", next = "action_step_prompt"   }
                }
            },

            // ─────────────────────────────────────────────────────────────────
            // LAYER 5 — ACTION MODES
            // ─────────────────────────────────────────────────────────────────

            // — Vent mode —
            new NodeDef
            {
                nodeId            = "action_vent_open",
                advanceMode       = AdvanceModeDef.TapToContinue,
                tapContinueNodeId = "action_vent_listen",
                textKey           = "action.vent.open"
            },
            new NodeDef
            {
                // Intentional loop with action_vent_more. Player exits via reflect or redirect.
                nodeId  = "action_vent_listen",
                textKey = "action.vent.listen",
                options = new[]
                {
                    new OptionDef { labelText = "There's more.",          next = "action_vent_more"    },
                    new OptionDef { labelText = "I think I've said it.",  next = "action_vent_reflect" },
                    new OptionDef { labelText = "I'm still really upset.", next = "support_redirect"   }
                }
            },
            new NodeDef
            {
                // Loop back to action_vent_listen — venting is rarely one pass.
                nodeId            = "action_vent_more",
                advanceMode       = AdvanceModeDef.TapToContinue,
                tapContinueNodeId = "action_vent_listen",
                textKey           = "action.vent.more"
            },
            new NodeDef
            {
                nodeId  = "action_vent_reflect",
                textKey = "action.vent.reflect",
                options = new[]
                {
                    new OptionDef { labelText = "A bit better.",   next = "hub_checkin"     },
                    new OptionDef { labelText = "Still the same.", next = "support_redirect" },
                    new OptionDef { labelText = "Worse.",          next = "support_redirect" }
                }
            },
            new NodeDef
            {
                // Gentle pivot — not a failure, just a course correction.
                nodeId  = "support_redirect",
                textKey = "support.redirect",
                options = new[]
                {
                    new OptionDef { labelText = "Ground me.",                   next = "grounding_exercise"   },
                    new OptionDef { labelText = "Reassurance.",                 next = "reassurance_response" },
                    new OptionDef { labelText = "Help me choose one step.",     next = "action_step_prompt"   },
                    new OptionDef { labelText = "Just stay with me.",           next = "action_vent_open"     }
                }
            },

            // — Boundary mode —
            new NodeDef
            {
                nodeId            = "action_bound_open",
                advanceMode       = AdvanceModeDef.TapToContinue,
                tapContinueNodeId = "action_bound_question",
                textKey           = "action.bound.open"
            },
            new NodeDef
            {
                nodeId  = "action_bound_question",
                textKey = "action.bound.question",
                options = new[]
                {
                    new OptionDef { labelText = "What's mine to own.",         next = "action_bound_own"     },
                    new OptionDef { labelText = "Letting go of what isn't mine.", next = "action_bound_release" },
                    new OptionDef { labelText = "Both.",                       next = "action_bound_both"    }
                }
            },
            new NodeDef
            {
                nodeId            = "action_bound_own",
                advanceMode       = AdvanceModeDef.TapToContinue,
                tapContinueNodeId = "hub_checkin",
                textKey           = "action.bound.own"
            },
            new NodeDef
            {
                nodeId            = "action_bound_release",
                advanceMode       = AdvanceModeDef.TapToContinue,
                tapContinueNodeId = "hub_checkin",
                textKey           = "action.bound.release"
            },
            new NodeDef
            {
                nodeId            = "action_bound_both",
                advanceMode       = AdvanceModeDef.TapToContinue,
                tapContinueNodeId = "hub_checkin",
                textKey           = "action.bound.both"
            },

            // — One-step mode —
            new NodeDef
            {
                nodeId            = "action_step_prompt",
                advanceMode       = AdvanceModeDef.TapToContinue,
                tapContinueNodeId = "action_step_confirm",
                textKey           = "action.step.prompt"
            },
            new NodeDef
            {
                nodeId            = "action_step_confirm",
                advanceMode       = AdvanceModeDef.TapToContinue,
                tapContinueNodeId = "hub_checkin",
                textKey           = "action.step.confirm"
            },

            // ─────────────────────────────────────────────────────────────────
            // EXISTING ACTION MODES (unchanged)
            // ─────────────────────────────────────────────────────────────────

            new NodeDef
            {
                nodeId  = "grounding_exercise",
                textKey = "grounding.exercise",
                options = new[] { new OptionDef { labelText = "Continue.", next = "grounding_followup_1", labelKey = "labels.continue" } }
            },
            new NodeDef
            {
                nodeId  = "grounding_followup_1",
                textKey = "grounding.followup.question",
                options = new[]
                {
                    new OptionDef { labelText = "Yes.", next = "grounding_success_response", labelKey = "labels.yes" },
                    new OptionDef { labelText = "No.",  next = "grounding_exercise_2", entryContext = "from_no", labelKey = "labels.no" },
                    new OptionDef { labelText = "I don't know.", next = "grounding_exercise_2", entryContext = "from_dontknow" }
                }
            },
            new NodeDef
            {
                nodeId = "grounding_exercise_2",
                textKeyByContext = new[]
                {
                    new TextKeyByContextEntry { context = "from_no",       textKey = "grounding.exercise2.from_no"       },
                    new TextKeyByContextEntry { context = "from_dontknow", textKey = "grounding.exercise2.from_dontknow" }
                },
                options = new[] { new OptionDef { labelText = "Continue.", next = "grounding_followup_2", labelKey = "labels.continue" } }
            },
            new NodeDef
            {
                nodeId  = "grounding_followup_2",
                textKey = "grounding.followup.question",
                options = new[]
                {
                    new OptionDef { labelText = "Yes.", next = "grounding_success_response",    labelKey = "labels.yes" },
                    new OptionDef { labelText = "No.",  next = "grounding_failed_response_no",  labelKey = "labels.no" },
                    new OptionDef { labelText = "I don't know.", next = "grounding_failed_response_dontknow" }
                }
            },
            new NodeDef
            {
                nodeId  = "grounding_failed_response_no",
                textKey = "grounding.failed.no",
                options = new[] { new OptionDef { labelText = "Okay.", next = "follow_up", labelKey = "labels.okay" } }
            },
            new NodeDef
            {
                nodeId  = "grounding_failed_response_dontknow",
                textKey = "grounding.failed.dontknow",
                options = new[] { new OptionDef { labelText = "Okay.", next = "redirect_prelude", labelKey = "labels.okay" } }
            },
            new NodeDef
            {
                nodeId  = "follow_up",
                textKey = "follow_up.prompt",
                options = new[] { new OptionDef { labelText = "Okay.", next = "hub_checkin", labelKey = "labels.okay" } }
            },
            new NodeDef
            {
                nodeId  = "redirect_prelude",
                textKey = "redirect.prelude",
                options = new[] { new OptionDef { labelText = "Okay.", next = "hub_redirect", labelKey = "labels.okay" } }
            },
            // Grounding success — acknowledgment beat before the hub.
            // TapToContinue so the player has a moment to absorb it.
            new NodeDef
            {
                nodeId            = "grounding_success_response",
                advanceMode       = AdvanceModeDef.TapToContinue,
                tapContinueNodeId = "hub_checkin",
                textKey           = "grounding.success.response"
            },
            new NodeDef
            {
                nodeId  = "reassurance_response",
                textKey = "reassure.general",
                options = new[] { new OptionDef { labelText = "Thanks.", next = "hub_checkin", labelKey = "labels.thanks" } }
            },

            // ─────────────────────────────────────────────────────────────────
            // LAYER 6 — CHECK-IN LOOP
            // "Something else." replaced with "Try something different." → support.
            // ─────────────────────────────────────────────────────────────────

            new NodeDef
            {
                nodeId  = "hub_checkin",
                textKey = "hub.checkin",
                options = new[]
                {
                    new OptionDef { labelText = "Check in again.",           next = "root"          },
                    new OptionDef { labelText = "Try something different.",   next = "support"       },
                    new OptionDef { labelText = "I'll get back to the day.", next = "session_close" }
                }
            },
            new NodeDef
            {
                nodeId  = "hub_redirect",
                textKey = "hub.checkin",
                options = new[]
                {
                    new OptionDef { labelText = "Check in again.",           next = "root"          },
                    new OptionDef { labelText = "Try something different.",   next = "support"       },
                    new OptionDef { labelText = "I'll get back to the day.", next = "session_close" }
                }
            },
            new NodeDef
            {
                // Retained as a fallback — not reachable from primary flow.
                nodeId  = "coming_soon",
                textKey = "coming_soon",
                options = new[] { new OptionDef { labelText = "Back.", next = "hub_checkin", labelKey = "labels.back" } }
            },
            // session_close — uses SpecialNext.End so EndSession() fires explicitly on click.
            // Avoids relying on triggersEndOverlay serialized in the module asset, which can
            // silently default to false in stale .asset files.
            new NodeDef
            {
                nodeId  = "session_close",
                textKey = "session_close",
                options = new[] { new OptionDef { labelText = "Okay.", specialNext = SpecialNext.End, labelKey = "labels.okay" } }
            }
        };

        var dir = System.IO.Path.GetDirectoryName(AssetPath);
        if (!System.IO.Directory.Exists(dir))
            System.IO.Directory.CreateDirectory(dir);

        AssetDatabase.CreateAsset(module, AssetPath);
        AssetDatabase.SaveAssets();
        Debug.Log($"Created CheckInModule (v3, 55 nodes) at {AssetPath}. Assign to WheelMenuController and enable Use Module.");
        Selection.activeObject = module;
    }
}
