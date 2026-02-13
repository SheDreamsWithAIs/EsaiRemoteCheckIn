using UnityEngine;

public class PortraitResolver
{
    private readonly PortraitDatabaseSO _db;

    public PortraitResolver(PortraitDatabaseSO db)
    {
        _db = db;
    }

    /// <summary>Resolve request to sprite. Returns (sprite, resolvedKey) for Back-safe restore.</summary>
    public (Sprite sprite, PortraitKey resolvedKey) Resolve(PortraitRequest request)
    {
        if (_db == null)
            return (null, new PortraitKey(PortraitMood.Neutral, 0, PortraitModifier.Default));

        var key = ResolveWithFallback(request);
        var sprite = _db.GetSprite(key);
        return (sprite, key);
    }

    /// <summary>Resolve by exact key (for Back restore).</summary>
    public Sprite ResolveByKey(PortraitKey key)
    {
        return _db != null ? _db.GetSprite(key) : null;
    }

    private PortraitKey ResolveWithFallback(PortraitRequest request)
    {
        int intensity = Mathf.Clamp(request.intensity, 0, 4);

        if (Try(request.mood, intensity, request.modifier, out var k)) return k;
        if (Try(request.mood, intensity, PortraitModifier.Default, out k)) return k;
        for (int i = intensity - 1; i >= 0; i--)
        {
            if (Try(request.mood, i, PortraitModifier.Default, out k)) return k;
        }
        if (Try(request.mood, 0, PortraitModifier.Default, out k)) return k;
        if (Try(PortraitMood.Neutral, 0, PortraitModifier.Default, out k)) return k;

        return new PortraitKey(request.mood, intensity, request.modifier);
    }

    private bool Try(PortraitMood mood, int intensity, PortraitModifier modifier, out PortraitKey key)
    {
        key = new PortraitKey(mood, intensity, modifier);
        return _db.HasKey(key);
    }
}
