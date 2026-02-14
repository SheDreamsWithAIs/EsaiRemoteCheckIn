using System;
using UnityEngine;

[Serializable]
public struct PortraitRequest
{
    public PortraitMood mood;
    public int intensity;
    public PortraitModifier modifier;

    public PortraitRequest(PortraitMood mood, int intensity = 0, PortraitModifier modifier = PortraitModifier.Default)
    {
        this.mood = mood;
        this.intensity = Mathf.Clamp(intensity, 0, 4);
        this.modifier = modifier;
    }
}

public enum PortraitMood
{
    Neutral,
    Friendly,
    Concerned,
    Firm,
    Sad,
    Shocked,
    Devastated,
    Warm,
    Amused,
    Embarrassed,
    Excited,
    Surprised
}

public enum PortraitModifier
{
    Default,
    SideLookLeft,
    SideLookRight,
    LookingDown,
    DirectEyeContact,
    OpenHands,
    HugOffer,
    SweatDrop,
    NoFace,
    MouthOpen,
    WideEyes
}

[Serializable]
public struct PortraitKey : IEquatable<PortraitKey>
{
    public PortraitMood mood;
    public int intensity;
    public PortraitModifier modifier;

    public PortraitKey(PortraitMood mood, int intensity, PortraitModifier modifier)
    {
        this.mood = mood;
        this.intensity = intensity;
        this.modifier = modifier;
    }

    public bool Equals(PortraitKey other) =>
        mood == other.mood && intensity == other.intensity && modifier == other.modifier;

    public override bool Equals(object obj) => obj is PortraitKey other && Equals(other);

    public override int GetHashCode() =>
        ((int)mood * 31 + intensity) * 31 + (int)modifier;
}
