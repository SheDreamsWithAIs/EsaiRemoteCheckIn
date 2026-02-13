using System.Collections.Generic;
using UnityEngine;

/// <summary>Runs a multi-module conversation flow. Handles NEXT_MODULE transitions.</summary>
public class ConversationRunner
{
    private ConversationFlowSO _flow;
    private int _currentModuleIndex;
    private DialogueModuleSO _currentModule;

    public string EntryNodeId => _currentModule != null ? _currentModule.entryNodeId : "root";
    public DialogueModuleSO CurrentModule => _currentModule;
    public bool IsInitialized => _flow != null;

    public void Init(ConversationFlowSO flow)
    {
        _flow = flow;
        _currentModuleIndex = 0;
        _currentModule = flow != null && flow.modules != null && flow.modules.Length > 0 ? flow.modules[0] : null;
        if (_currentModule != null) _currentModule.BuildLookup();
    }

    public NodeDef GetNode(string nodeId)
    {
        if (_currentModule != null)
        {
            var n = _currentModule.GetNode(nodeId);
            if (n != null) return n;
        }
        return null;
    }

    /// <summary>When option has specialNext NextModule, advance to next module and return its entry node id.</summary>
    public string AdvanceToNextModule()
    {
        if (_flow == null || _flow.modules == null || _currentModuleIndex + 1 >= _flow.modules.Length)
            return null;

        _currentModuleIndex++;
        _currentModule = _flow.modules[_currentModuleIndex];
        if (_currentModule != null) _currentModule.BuildLookup();
        return _currentModule != null ? _currentModule.entryNodeId : null;
    }
}
