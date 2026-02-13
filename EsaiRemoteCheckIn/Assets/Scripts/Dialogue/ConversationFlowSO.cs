using UnityEngine;

[CreateAssetMenu(fileName = "ConversationFlow", menuName = "Esai/Conversation Flow")]
public class ConversationFlowSO : ScriptableObject
{
    public string flowId;
    public DialogueModuleSO[] modules;
}
