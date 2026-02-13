using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DialogueModule", menuName = "Esai/Dialogue Module")]
public class DialogueModuleSO : ScriptableObject
{
    public string moduleId;
    public string entryNodeId;
    public NodeDef[] nodes;

    private Dictionary<string, NodeDef> _nodeLookup;

    public Dictionary<string, NodeDef> GetNodeLookup()
    {
        if (_nodeLookup == null) BuildLookup();
        return _nodeLookup;
    }

    public void BuildLookup()
    {
        _nodeLookup = new Dictionary<string, NodeDef>();
        if (nodes == null) return;
        foreach (var n in nodes)
        {
            if (string.IsNullOrEmpty(n.nodeId)) continue;
            _nodeLookup[n.nodeId] = n;
        }
    }

    public NodeDef GetNode(string nodeId)
    {
        if (_nodeLookup == null) BuildLookup();
        return _nodeLookup != null && _nodeLookup.TryGetValue(nodeId, out var n) ? n : null;
    }
}
