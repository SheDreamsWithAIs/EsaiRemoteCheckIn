using UnityEditor;
using UnityEngine;

/// <summary>Creates CheckInModule.asset with the grounding branch as proof of Phase 3 migration.</summary>
public static class CreateCheckInModule
{
    private const string AssetPath = "Assets/ScriptableObjects/Dialogue/Modules/CheckInModule.asset";

    [MenuItem("Esai/Create Check-In Module (Grounding Branch)")]
    public static void Create()
    {
        var module = AssetDatabase.LoadAssetAtPath<DialogueModuleSO>(AssetPath);
        if (module != null)
        {
            Debug.Log($"CheckInModule already exists at {AssetPath}");
            Selection.activeObject = module;
            return;
        }

        module = ScriptableObject.CreateInstance<DialogueModuleSO>();
        module.moduleId = "checkin";
        module.entryNodeId = "root";

        module.nodes = new[]
        {
            new NodeDef
            {
                nodeId = "root",
                textKey = "root.greeting",
                options = new[]
                {
                    new OptionDef { labelText = "I'm okay today.", next = "okay" },
                    new OptionDef { labelText = "I need support.", next = "support" },
                    new OptionDef { labelText = "Something happened.", next = "something" },
                    new OptionDef { labelText = "I don't know.", next = "dontknow" }
                }
            },
            new NodeDef
            {
                nodeId = "okay",
                textKey = "okay.prompt",
                options = new[]
                {
                    new OptionDef { labelText = "Keep it light.", responseTextKey = "okay.keep_light" },
                    new OptionDef { labelText = "Quick check-in.", responseTextKey = "okay.quick_checkin" },
                    new OptionDef { labelText = "Actually... I need support.", next = "support" }
                }
            },
            new NodeDef
            {
                nodeId = "support",
                textKey = "support.prompt",
                options = new[]
                {
                    new OptionDef { labelText = "Reassurance.", next = "reassurance_response" },
                    new OptionDef { labelText = "Grounding.", next = "grounding_exercise" },
                    new OptionDef { labelText = "Help me choose one step.", responseTextKey = "support.one_step" }
                }
            },
            new NodeDef
            {
                nodeId = "grounding_exercise",
                textKey = "grounding.exercise",
                options = new[] { new OptionDef { labelText = "Continue.", next = "grounding_followup_1", labelKey = "labels.continue" } }
            },
            new NodeDef
            {
                nodeId = "grounding_followup_1",
                textKey = "grounding.followup.question",
                options = new[]
                {
                    new OptionDef { labelText = "Yes.", next = "hub_checkin", labelKey = "labels.yes" },
                    new OptionDef { labelText = "No.", next = "grounding_exercise_2", entryContext = "from_no", labelKey = "labels.no" },
                    new OptionDef { labelText = "I don't know.", next = "grounding_exercise_2", entryContext = "from_dontknow" }
                }
            },
            new NodeDef
            {
                nodeId = "grounding_exercise_2",
                textKeyByContext = new[]
                {
                    new TextKeyByContextEntry { context = "from_no", textKey = "grounding.exercise2.from_no" },
                    new TextKeyByContextEntry { context = "from_dontknow", textKey = "grounding.exercise2.from_dontknow" }
                },
                options = new[] { new OptionDef { labelText = "Continue.", next = "grounding_followup_2", labelKey = "labels.continue" } }
            },
            new NodeDef
            {
                nodeId = "grounding_followup_2",
                textKey = "grounding.followup.question",
                options = new[]
                {
                    new OptionDef { labelText = "Yes.", next = "hub_checkin", labelKey = "labels.yes" },
                    new OptionDef { labelText = "No.", next = "grounding_failed_response_no", labelKey = "labels.no" },
                    new OptionDef { labelText = "I don't know.", next = "grounding_failed_response_dontknow" }
                }
            },
            new NodeDef
            {
                nodeId = "grounding_failed_response_no",
                textKey = "grounding.failed.no",
                options = new[] { new OptionDef { labelText = "Okay.", next = "follow_up", labelKey = "labels.okay" } }
            },
            new NodeDef
            {
                nodeId = "grounding_failed_response_dontknow",
                textKey = "grounding.failed.dontknow",
                options = new[] { new OptionDef { labelText = "Okay.", next = "redirect_prelude", labelKey = "labels.okay" } }
            },
            new NodeDef
            {
                nodeId = "follow_up",
                textKey = "follow_up.prompt",
                options = new[] { new OptionDef { labelText = "Okay.", next = "hub_checkin", labelKey = "labels.okay" } }
            },
            new NodeDef
            {
                nodeId = "redirect_prelude",
                textKey = "redirect.prelude",
                options = new[] { new OptionDef { labelText = "Okay.", next = "hub_redirect", labelKey = "labels.okay" } }
            },
            new NodeDef
            {
                nodeId = "hub_checkin",
                textKey = "hub.checkin",
                options = new[]
                {
                    new OptionDef { labelText = "Check in again.", next = "root" },
                    new OptionDef { labelText = "I'll get back to the day.", next = "session_close" },
                    new OptionDef { labelText = "Something else.", next = "coming_soon" }
                }
            },
            new NodeDef
            {
                nodeId = "hub_redirect",
                textKey = "hub.checkin",
                options = new[]
                {
                    new OptionDef { labelText = "Check in again.", next = "root" },
                    new OptionDef { labelText = "I'll get back to the day.", next = "session_close" },
                    new OptionDef { labelText = "Something else.", next = "coming_soon" }
                }
            },
            new NodeDef
            {
                nodeId = "reassurance_response",
                textKey = "reassure.general",
                options = new[] { new OptionDef { labelText = "Thanks.", next = "hub_checkin", labelKey = "labels.thanks" } }
            },
            new NodeDef
            {
                nodeId = "something",
                textKey = "something.prompt",
                options = new[]
                {
                    new OptionDef { labelText = "Conflict / people.", responseTextKey = "something.conflict" },
                    new OptionDef { labelText = "Bad news.", responseTextKey = "something.bad_news" },
                    new OptionDef { labelText = "Overwhelm / too much.", responseTextKey = "something.overwhelm" }
                }
            },
            new NodeDef
            {
                nodeId = "dontknow",
                textKey = "dontknow.prompt",
                options = new[]
                {
                    new OptionDef { labelText = "Body feels bad.", responseTextKey = "dontknow.body" },
                    new OptionDef { labelText = "Emotion feels bad.", responseTextKey = "dontknow.emotion" },
                    new OptionDef { labelText = "Just empty.", responseTextKey = "dontknow.empty" }
                }
            },
            new NodeDef
            {
                nodeId = "coming_soon",
                textKey = "coming_soon",
                options = new[] { new OptionDef { labelText = "Back.", next = "hub_checkin", labelKey = "labels.back" } }
            },
            new NodeDef
            {
                nodeId = "session_close",
                textKey = "session_close",
                triggersEndOverlay = true,
                options = null
            }
        };

        var dir = System.IO.Path.GetDirectoryName(AssetPath);
        if (!System.IO.Directory.Exists(dir))
            System.IO.Directory.CreateDirectory(dir);

        AssetDatabase.CreateAsset(module, AssetPath);
        AssetDatabase.SaveAssets();
        Debug.Log($"Created CheckInModule at {AssetPath}. Assign to WheelMenuController and enable Use Module.");
        Selection.activeObject = module;
    }
}
