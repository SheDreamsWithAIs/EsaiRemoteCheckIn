using System;
using System.Collections.Generic;
using UnityEngine;

    /// <summary>
    /// Loads and serves dialogue lines from Lines.json. Supports Select mode (choose variant)
    /// and Replay mode (return exact same variant for Back).
    /// </summary>
    public class LinesService
    {
        private const string LinesResourcePath = "Dialogue/Lines";
        private const string MissingKeyPrefix = "MISSING_TEXTKEY:";
        private const string FallbackText = "...";

        private Dictionary<string, LineEntry> _lines;
        private readonly Dictionary<string, Queue<int>> _recentVariants = new();

        public bool IsLoaded => _lines != null;

        public void Load()
        {
            var asset = Resources.Load<TextAsset>(LinesResourcePath);
            if (asset == null)
            {
                Debug.LogError($"LinesService: Could not load {LinesResourcePath} from Resources.");
                _lines = new Dictionary<string, LineEntry>();
                return;
            }

            try
            {
                var wrapper = JsonUtility.FromJson<LinesWrapper>(asset.text);
                _lines = new Dictionary<string, LineEntry>();
                if (wrapper?.entries != null)
                {
                    foreach (var e in wrapper.entries)
                    {
                        if (!string.IsNullOrEmpty(e.key))
                            _lines[e.key] = e;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"LinesService: Failed to parse Lines.json: {ex.Message}");
                _lines = new Dictionary<string, LineEntry>();
            }
        }

        /// <summary>Select mode: choose a variant (weighted/no-repeat), return text + selectedVariantId.</summary>
        public LineResult GetLine(string textKey)
        {
            EnsureLoaded();

            if (!_lines.TryGetValue(textKey, out var entry) || entry.variants == null || entry.variants.Length == 0)
            {
                string fallback = Application.isEditor
                    ? $"{MissingKeyPrefix} {textKey}"
                    : FallbackText;
                return new LineResult { text = fallback, selectedVariantId = -1 };
            }

            int variantIndex = SelectVariant(entry, textKey);
            return new LineResult { text = entry.variants[variantIndex].text, selectedVariantId = variantIndex };
        }

        public struct LineResult
        {
            public string text;
            public int selectedVariantId;
        }

        /// <summary>Replay mode: return exact same text for selectedVariantId. No reroll.</summary>
        public string GetLineReplay(string textKey, int selectedVariantId)
        {
            EnsureLoaded();

            if (!_lines.TryGetValue(textKey, out var entry) || entry.variants == null || entry.variants.Length == 0)
            {
                return Application.isEditor ? $"{MissingKeyPrefix} {textKey}" : FallbackText;
            }

            if (selectedVariantId < 0 || selectedVariantId >= entry.variants.Length)
                return entry.variants[0].text;

            return entry.variants[selectedVariantId].text;
        }

        private int SelectVariant(LineEntry entry, string textKey)
        {
            if (entry.variants.Length == 1) return 0;

            int noRepeat = entry.rules?.noRepeatWindow ?? 0;
            var exclude = noRepeat > 0 ? GetRecentIndices(textKey, noRepeat) : null;

            int totalWeight = 0;
            var candidates = new List<int>();
            for (int i = 0; i < entry.variants.Length; i++)
            {
                if (exclude != null && exclude.Contains(i)) continue;
                int w = Mathf.Max(1, entry.variants[i].weight);
                totalWeight += w;
                candidates.Add(i);
            }

            if (candidates.Count == 0)
            {
                candidates.Clear();
                for (int i = 0; i < entry.variants.Length; i++) candidates.Add(i);
                totalWeight = 0;
                foreach (int i in candidates)
                    totalWeight += Mathf.Max(1, entry.variants[i].weight);
            }

            if (totalWeight <= 0) return 0;

            int roll = UnityEngine.Random.Range(0, totalWeight);
            foreach (int i in candidates)
            {
                int w = Mathf.Max(1, entry.variants[i].weight);
                if (roll < w)
                {
                    if (noRepeat > 0) PushRecent(textKey, i, noRepeat);
                    return i;
                }
                roll -= w;
            }

            int chosen = candidates[0];
            if (noRepeat > 0) PushRecent(textKey, chosen, noRepeat);
            return chosen;
        }

        private HashSet<int> GetRecentIndices(string textKey, int window)
        {
            if (!_recentVariants.TryGetValue(textKey, out var q)) return null;
            var set = new HashSet<int>();
            foreach (int i in q) set.Add(i);
            return set;
        }

        private void PushRecent(string textKey, int index, int window)
        {
            if (!_recentVariants.TryGetValue(textKey, out var q))
            {
                q = new Queue<int>();
                _recentVariants[textKey] = q;
            }
            q.Enqueue(index);
            while (q.Count > window) q.Dequeue();
        }

        private void EnsureLoaded()
        {
            if (_lines == null) Load();
        }

        [Serializable]
        private class LinesWrapper
        {
            public LineEntry[] entries;
        }

        [Serializable]
        public class LineEntry
        {
            public string key;
            public LineVariant[] variants;
            public LineRules rules;
        }

        [Serializable]
        public class LineVariant
        {
            public string text;
            public int weight = 1;
        }

        [Serializable]
        public class LineRules
        {
            public int noRepeatWindow;
        }
    }
