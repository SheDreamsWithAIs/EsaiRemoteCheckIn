using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class WheelMenuController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text esaiResponseText;
    [SerializeField] private Transform wheelOptionsContainer;
    [SerializeField] private Button wheelOptionButtonPrefab;
    [SerializeField] private GameObject endOverlayPanel;

    [Header("End Overlay")]
    [SerializeField] private float endOverlayDelaySeconds = 1.5f;

    private readonly Dictionary<string, Node> nodes = new();
    private Coroutine endOverlayCoroutine;
    private readonly Stack<HistoryEntry> nodeHistory = new();
    private string currentNodeId = "root";
    private string currentEntryContext;
    private string lastLine = "";
    private bool isShowingResponseLine;

    /// <summary>True when the player can go back one step.</summary>
    public bool CanGoBack => nodeHistory.Count > 0;

    private struct HistoryEntry
    {
        public string NodeId;
        public bool IsShowingResponseLine;
        public string EntryContext;

        public HistoryEntry(string nodeId, bool isShowingResponseLine, string entryContext)
        {
            NodeId = nodeId;
            IsShowingResponseLine = isShowingResponseLine;
            EntryContext = entryContext;
        }
    }

    private void Awake()
    {
        BuildNodes();
        ShowNode("root", pushHistory: false);
    }

    private void BuildNodes()
    {
        // --- Root and initial branches ---
        nodes["root"] = new Node(
            "Hey... come here a second. How are you, really?",
            new List<Option>
            {
                new Option("I'm okay today.", nextNodeId: "okay"),
                new Option("I need support.", nextNodeId: "support"),
                new Option("Something happened.", nextNodeId: "something"),
                new Option("I don't know.", nextNodeId: "dontknow"),
            }
        );

        nodes["okay"] = new Node(
            "I'm glad you're steady today. Want to keep it light, or check in anyway?",
            new List<Option>
            {
                new Option("Keep it light.", responseLine: "Okay. Then let's keep it gentle. One small good thing from today?"),
                new Option("Quick check-in.", responseLine: "Alright. Body first: water, food, and a place to sit?are those covered?"),
                new Option("Actually... I need support.", nextNodeId: "support")
            }
        );

        nodes["support"] = new Node(
            "Then that's what we'll do. What kind of support fits right now?",
            new List<Option>
            {
                new Option("Reassurance.", nextNodeId: "reassurance_response"),
                new Option("Grounding.", nextNodeId: "grounding_exercise"),
                new Option("Help me choose one step.", responseLine: "Okay. What's the smallest next thing that would make today 1% easier?")
            }
        );

        nodes["something"] = new Node(
            "I'm listening. What kind of 'something' was it?",
            new List<Option>
            {
                new Option("Conflict / people.", responseLine: "Ugh. I'm sorry. Do you want to vent, or do you want grounding first?"),
                new Option("Bad news.", responseLine: "Okay. That's heavy. You don't have to carry it alone?tell me what happened."),
                new Option("Overwhelm / too much.", responseLine: "That makes sense. Let's shrink the day. What can wait, and what can't?")
            }
        );

        nodes["dontknow"] = new Node(
            "That's okay. 'I don't know' still counts as information. Let's narrow it down.",
            new List<Option>
            {
                new Option("Body feels bad.", responseLine: "Okay. Let's get basic care first?water, food, rest. What's missing right now?"),
                new Option("Emotion feels bad.", responseLine: "Got it. If you had to name it loosely: anxious, sad, angry, or numb?"),
                new Option("Just empty.", responseLine: "You're allowed to be empty. Let's do minimum-viable-human for a bit.")
            }
        );

        // --- Reassurance path (wired to hub for MVP) ---
        nodes["reassurance_response"] = new Node(
            "Hey. You're okay. You don't have to solve everything today?just keep yourself safe. That's enough.",
            new List<Option>
            {
                new Option("Thanks.", nextNodeId: "hub_checkin")
            }
        );

        // --- Grounding branch ---
        nodes["grounding_exercise"] = new Node(
            "Can you feel your feet on the floor? One slow breath in... and out. Good. Stay with me.",
            new List<Option>
            {
                new Option("Continue.", nextNodeId: "grounding_followup_1")
            }
        );

        nodes["grounding_followup_1"] = new Node(
            "Do you feel a bit more grounded?",
            new List<Option>
            {
                new Option("Yes.", nextNodeId: "hub_checkin"),
                new Option("No.", nextNodeId: "grounding_exercise_2", entryContext: "from_no"),
                new Option("I don't know.", nextNodeId: "grounding_exercise_2", entryContext: "from_dontknow")
            }
        );

        nodes["grounding_exercise_2"] = new Node(
            "Let's try something else.",
            new List<Option>
            {
                new Option("Continue.", nextNodeId: "grounding_followup_2")
            },
            esaiLineByContext: new Dictionary<string, string>
            {
                ["from_no"] = "Okay. Let's try a different grounding. Name five things you can see right now.",
                ["from_dontknow"] = "That's okay. Let's narrow it down?are you more in your head or in your body right now?"
            }
        );

        nodes["grounding_followup_2"] = new Node(
            "Do you feel a bit more grounded?",
            new List<Option>
            {
                new Option("Yes.", nextNodeId: "hub_checkin"),
                new Option("No.", nextNodeId: "grounding_failed_response_no"),
                new Option("I don't know.", nextNodeId: "grounding_failed_response_dontknow")
            }
        );

        nodes["grounding_failed_response_no"] = new Node(
            "That's okay. Sometimes it takes longer. You're still here, you're still trying. That matters.",
            new List<Option>
            {
                new Option("Okay.", nextNodeId: "follow_up")
            }
        );

        nodes["grounding_failed_response_dontknow"] = new Node(
            "Maybe this isn't the right solution for what you're going through. Let's try something completely different.",
            new List<Option>
            {
                new Option("Okay.", nextNodeId: "redirect_prelude")
            }
        );

        nodes["follow_up"] = new Node(
            "Okay. Where would you like to go from here?",
            new List<Option>
            {
                new Option("Okay.", nextNodeId: "hub_checkin")
            }
        );

        nodes["redirect_prelude"] = new Node(
            "Let's try something completely different.",
            new List<Option>
            {
                new Option("Okay.", nextNodeId: "hub_redirect")
            }
        );

        // --- Hubs ---
        nodes["hub_checkin"] = new Node(
            "Where would you like to go from here?",
            new List<Option>
            {
                new Option("Check in again.", nextNodeId: "root"),
                new Option("I'll get back to the day.", nextNodeId: "session_close"),
                new Option("Something else.", nextNodeId: "coming_soon")
            }
        );

        nodes["hub_redirect"] = new Node(
            "Where would you like to go from here?",
            new List<Option>
            {
                new Option("Check in again.", nextNodeId: "root"),
                new Option("I'll get back to the day.", nextNodeId: "session_close"),
                new Option("Something else.", nextNodeId: "coming_soon")
            }
        );

        nodes["coming_soon"] = new Node(
            "Other modes aren't ready yet. Check back soon.",
            new List<Option>
            {
                new Option("Back.", nextNodeId: "hub_checkin")
            }
        );

        nodes["session_close"] = new Node(
            "Take care of yourself. I'm here whenever you need.",
            options: null,
            triggersEndOverlay: true
        );
    }

    private void ShowNode(string nodeId, bool pushHistory, string entryContext = null)
    {
        if (endOverlayCoroutine != null)
        {
            StopCoroutine(endOverlayCoroutine);
            endOverlayCoroutine = null;
        }

        if (pushHistory && (currentNodeId != nodeId || isShowingResponseLine))
        {
            nodeHistory.Push(new HistoryEntry(currentNodeId, isShowingResponseLine, currentEntryContext));
        }

        currentNodeId = nodeId;
        currentEntryContext = entryContext;
        isShowingResponseLine = false;

        if (!nodes.TryGetValue(nodeId, out var node))
        {
            Debug.LogError($"Node not found: {nodeId}");
            return;
        }

        string displayLine = node.GetEsaiLine(entryContext);
        esaiResponseText.text = displayLine;
        lastLine = displayLine;

        ClearOptions();

        if (node.Options == null || node.Options.Count == 0)
        {
            if (wheelOptionsContainer != null)
                wheelOptionsContainer.gameObject.SetActive(false);

            if (node.TriggersEndOverlay)
            {
                if (endOverlayCoroutine != null) StopCoroutine(endOverlayCoroutine);
                endOverlayCoroutine = StartCoroutine(ShowEndOverlayAfterDelay());
            }
            return;
        }

        if (wheelOptionsContainer != null)
            wheelOptionsContainer.gameObject.SetActive(true);

        foreach (var opt in node.Options)
        {
            var btn = Instantiate(wheelOptionButtonPrefab, wheelOptionsContainer);
            var label = btn.GetComponentInChildren<TMP_Text>();
            if (label != null) label.text = opt.Label;

            btn.onClick.RemoveAllListeners();
            var captureOpt = opt;
            btn.onClick.AddListener(() =>
            {
                if (!string.IsNullOrEmpty(captureOpt.NextNodeId))
                {
                    ShowNode(captureOpt.NextNodeId, pushHistory: true, captureOpt.EntryContext);
                }
                else if (!string.IsNullOrEmpty(captureOpt.ResponseLine))
                {
                    nodeHistory.Push(new HistoryEntry(currentNodeId, isShowingResponseLine, currentEntryContext));
                    isShowingResponseLine = true;
                    esaiResponseText.text = captureOpt.ResponseLine;
                    lastLine = captureOpt.ResponseLine;
                }
            });
        }
    }

    private IEnumerator ShowEndOverlayAfterDelay()
    {
        yield return new WaitForSeconds(endOverlayDelaySeconds);
        EndSession();
    }

    private void ClearOptions()
    {
        if (wheelOptionsContainer == null) return;
        for (int i = wheelOptionsContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(wheelOptionsContainer.GetChild(i).gameObject);
        }
    }

    private class Node
    {
        public string EsaiLine;
        public List<Option> Options;
        public bool TriggersEndOverlay;
        public Dictionary<string, string> EsaiLineByContext;

        public Node(string esaiLine, List<Option> options, bool triggersEndOverlay = false,
            Dictionary<string, string> esaiLineByContext = null)
        {
            EsaiLine = esaiLine;
            Options = options;
            TriggersEndOverlay = triggersEndOverlay;
            EsaiLineByContext = esaiLineByContext;
        }

        public string GetEsaiLine(string entryContext)
        {
            if (!string.IsNullOrEmpty(entryContext) && EsaiLineByContext != null &&
                EsaiLineByContext.TryGetValue(entryContext, out var line))
                return line;
            return EsaiLine;
        }
    }

    private class Option
    {
        public string Label;
        public string NextNodeId;
        public string ResponseLine;
        public string EntryContext;

        public Option(string label, string nextNodeId = null, string responseLine = null, string entryContext = null)
        {
            Label = label;
            NextNodeId = nextNodeId;
            ResponseLine = responseLine;
            EntryContext = entryContext;
        }
    }

    public void BackOne()
    {
        if (nodeHistory.Count == 0) return;

        if (endOverlayCoroutine != null)
        {
            StopCoroutine(endOverlayCoroutine);
            endOverlayCoroutine = null;
        }

        var entry = nodeHistory.Pop();
        ShowNode(entry.NodeId, pushHistory: false, entry.EntryContext);
    }

    public void SayAgain()
    {
        if (!string.IsNullOrEmpty(lastLine))
            esaiResponseText.text = lastLine;
    }

    public void EndSession()
    {
        if (endOverlayPanel != null)
            endOverlayPanel.SetActive(true);
    }

    public void CancelEndSession()
    {
        if (endOverlayPanel != null)
            endOverlayPanel.SetActive(false);
    }
}
