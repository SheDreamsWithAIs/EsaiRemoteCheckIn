using System;
using UnityEngine;

[Serializable]
public class OptionDef
{
    public string labelKey;
    public string labelText;
    public string next;
    public SpecialNext specialNext;
    public string entryContext;
    public string responseTextKey;
}

public enum SpecialNext
{
    None,
    NextModule,
    Hub,
    End
}
