using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>Editor-time validator for dialogue content. Tools > Esai > Validate Content</summary>
public static class DialogueContentValidator
{
    private const string LinesResourcePath = "Dialogue/Lines";

    /// <summary>Keys referenced by nodes (textKey, responseTextKey). Update when migrating to Phase 3 modules.</summary>
    private static readonly string[] RequiredTextKeys =
    {
        "root.greeting", "okay.prompt", "support.prompt", "something.prompt", "dontknow.prompt",
        "reassure.general", "grounding.exercise", "grounding.followup.question",
        "grounding.exercise2.from_no", "grounding.exercise2.from_dontknow",
        "grounding.failed.no", "grounding.failed.dontknow",
        "follow_up.prompt", "redirect.prelude", "hub.checkin",
        "coming_soon", "session_close",
        "okay.keep_light", "okay.quick_checkin", "support.one_step",
        "something.conflict", "something.bad_news", "something.overwhelm",
        "dontknow.body", "dontknow.emotion", "dontknow.empty"
    };

    /// <summary>Keys used for option labels (labelKey). Update when migrating to Phase 3.</summary>
    private static readonly string[] RequiredLabelKeys = { "labels.thanks", "labels.continue", "labels.yes", "labels.no", "labels.okay", "labels.back" };

    [MenuItem("Tools/Esai/Validate Content")]
    public static void Validate()
    {
        var errors = new List<string>();
        var warnings = new List<string>();

        ValidateLinesDb(errors, warnings);
        ValidatePortraitCoverage(warnings);

        if (errors.Count > 0)
        {
            foreach (var e in errors) Debug.LogError($"[Esai Validator] {e}");
        }
        if (warnings.Count > 0)
        {
            foreach (var w in warnings) Debug.LogWarning($"[Esai Validator] {w}");
        }

        if (errors.Count == 0 && warnings.Count == 0)
        {
            Debug.Log("[Esai Validator] All checks passed.");
        }
        else
        {
            Debug.Log($"[Esai Validator] Done. {errors.Count} error(s), {warnings.Count} warning(s).");
        }
    }

    private static void ValidateLinesDb(List<string> errors, List<string> warnings)
    {
        var asset = Resources.Load<TextAsset>(LinesResourcePath);
        if (asset == null)
        {
            errors.Add($"Could not load {LinesResourcePath} from Resources.");
            return;
        }

        Dictionary<string, LinesService.LineEntry> linesByKey;
        try
        {
            var wrapper = JsonUtility.FromJson<LinesWrapper>(asset.text);
            linesByKey = new Dictionary<string, LinesService.LineEntry>();
            if (wrapper?.entries != null)
            {
                foreach (var e in wrapper.entries)
                {
                    if (string.IsNullOrEmpty(e.key))
                    {
                        warnings.Add("Lines.json: entry with empty key skipped.");
                        continue;
                    }
                    if (linesByKey.ContainsKey(e.key))
                    {
                        errors.Add($"Lines.json: duplicate key '{e.key}'.");
                    }
                    linesByKey[e.key] = e;
                }
            }
        }
        catch (Exception ex)
        {
            errors.Add($"Lines.json parse failed: {ex.Message}");
            return;
        }

        foreach (var key in RequiredTextKeys)
        {
            if (!linesByKey.TryGetValue(key, out var entry))
            {
                errors.Add($"Missing textKey in Lines DB: '{key}'");
                continue;
            }
            if (entry.variants == null || entry.variants.Length == 0)
            {
                errors.Add($"textKey '{key}' has no variants.");
            }
        }

        foreach (var key in RequiredLabelKeys)
        {
            if (!linesByKey.TryGetValue(key, out var entry))
            {
                warnings.Add($"Missing labelKey in Lines DB: '{key}' (used when labelKey is set on options)");
                continue;
            }
            if (entry.variants == null || entry.variants.Length == 0)
            {
                warnings.Add($"labelKey '{key}' has no variants.");
            }
        }
    }

    private static void ValidatePortraitCoverage(List<string> warnings)
    {
        var db = AssetDatabase.LoadAssetAtPath<PortraitDatabaseSO>("Assets/ScriptableObjects/Dialogue/PortraitDatabase.asset");
        if (db == null)
        {
            warnings.Add("PortraitDatabase not found. Run Esai > Create Portrait Database.");
            return;
        }

        db.BuildLookup();

        var missing = new List<string>();
        foreach (PortraitMood mood in Enum.GetValues(typeof(PortraitMood)))
        {
            for (int i = 0; i <= 4; i++)
            {
                foreach (PortraitModifier mod in Enum.GetValues(typeof(PortraitModifier)))
                {
                    var key = new PortraitKey(mood, i, mod);
                    if (!db.HasKey(key))
                    {
                        missing.Add($"{mood}/{i}/{mod}");
                    }
                }
            }
        }

        if (missing.Count > 0)
        {
            warnings.Add($"Portrait coverage: {missing.Count} Mood/Intensity/Modifier combos missing (fallback exists). Sample: {string.Join(", ", missing.Take(5))}...");
        }
    }

    [Serializable]
    private class LinesWrapper
    {
        public LinesService.LineEntry[] entries;
    }
}
