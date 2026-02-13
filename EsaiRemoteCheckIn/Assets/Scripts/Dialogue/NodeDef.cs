using System;
using UnityEngine;

[Serializable]
public class NodeDef
{
    public string nodeId;
    public Speaker speaker = Speaker.Esai;
    public string textKey;
    public string esaiLine;
    public PortraitRequest portraitRequest;
    public AdvanceModeDef advanceMode = AdvanceModeDef.WaitForChoice;
    public string tapContinueNodeId;
    public OptionDef[] options;
    public bool triggersEndOverlay;
    public TextKeyByContextEntry[] textKeyByContext;
}

public enum Speaker { Esai, System }

public enum AdvanceModeDef { WaitForChoice, TapToContinue }

[Serializable]
public class TextKeyByContextEntry
{
    public string context;
    public string textKey;
}
