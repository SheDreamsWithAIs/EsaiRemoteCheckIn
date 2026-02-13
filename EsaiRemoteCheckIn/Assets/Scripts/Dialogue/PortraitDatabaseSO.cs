using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PortraitDatabase", menuName = "Esai/Portrait Database")]
public class PortraitDatabaseSO : ScriptableObject
{
    [Serializable]
    public class PortraitEntry
    {
        public PortraitMood mood;
        public int intensity;
        public PortraitModifier modifier;
        public Sprite sprite;
    }

    public List<PortraitEntry> entries = new();

    private Dictionary<PortraitKey, Sprite> _runtimeLookup;

    public void BuildLookup()
    {
        _runtimeLookup = new Dictionary<PortraitKey, Sprite>();
        if (entries == null) return;
        foreach (var e in entries)
        {
            if (e.sprite == null) continue;
            var key = new PortraitKey(e.mood, e.intensity, e.modifier);
            if (!_runtimeLookup.ContainsKey(key))
                _runtimeLookup[key] = e.sprite;
        }
    }

    public Sprite GetSprite(PortraitKey key)
    {
        if (_runtimeLookup == null) BuildLookup();
        return _runtimeLookup != null && _runtimeLookup.TryGetValue(key, out var s) ? s : null;
    }

    public bool HasKey(PortraitKey key)
    {
        if (_runtimeLookup == null) BuildLookup();
        return _runtimeLookup != null && _runtimeLookup.ContainsKey(key);
    }
}
