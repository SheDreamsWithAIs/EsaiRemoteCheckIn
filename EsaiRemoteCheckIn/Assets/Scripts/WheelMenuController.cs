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


    private readonly Dictionary<string, Node> nodes = new();
    private readonly Stack<string> nodeHistory = new();
    private string currentNodeId = "root";
    private string lastLine = "";


    private void Awake()
    {
        BuildNodes();
        ShowNode("root", pushHistory: false);
    }

    private void BuildNodes()
    {
        nodes["root"] = new Node(
            "Hey… come here a second. How are you, really?",
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
                new Option("Quick check-in.", responseLine: "Alright. Body first: water, food, and a place to sit—are those covered?"),
                new Option("Actually… I need support.", nextNodeId: "support")
            }
        );

        nodes["support"] = new Node(
            "Then that's what we'll do. What kind of support fits right now?",
            new List<Option>
            {
                new Option("Reassurance.", responseLine: "Hey. You're okay. You don't have to solve everything today—just keep yourself safe. That's enough."),
                new Option("Grounding.", responseLine: "Can you feel your feet on the floor? One slow breath in… and out. Good. Stay with me."),
                new Option("Help me choose one step.", responseLine: "Okay. What's the smallest next thing that would make today 1% easier?")            }
        );

        nodes["something"] = new Node(
            "I'm listening. What kind of 'something' was it?",
            new List<Option>
            {
                new Option("Conflict / people.", responseLine: "Ugh. I'm sorry. Do you want to vent, or do you want grounding first?"),
                new Option("Bad news.", responseLine: "Okay. That’s heavy. You don’t have to carry it alone—tell me what happened."),
                new Option("Overwhelm / too much.", responseLine: "That makes sense. Let's shrink the day. What can wait, and what can't?")
            }
        );

        nodes["dontknow"] = new Node(
            "That's okay. 'I don't know' still counts as information. Let's narrow it down.",
            new List<Option>
            {
                new Option("Body feels bad.", responseLine: "Okay. Let's get basic care first—water, food, rest. What's missing right now?"),
                new Option("Emotion feels bad.", responseLine: "Got it. If you had to name it loosely: anxious, sad, angry, or numb?"),
                new Option("Just empty.", responseLine: "You're allowed to be empty. Let's do minimum-viable-human for a bit.")
            }
        );
    }

    private void ShowNode(string nodeId, bool pushHistory)
    {
        if (pushHistory && currentNodeId != nodeId)
            nodeHistory.Push(currentNodeId);

        currentNodeId = nodeId;

        if (!nodes.TryGetValue(nodeId, out var node))
        {
            Debug.LogError($"Node not found: {nodeId}");
            return;
        }

        esaiResponseText.text = node.EsaiLine;
        lastLine = node.EsaiLine;

        ClearOptions();

        foreach (var opt in node.Options)
        {
            var btn = Instantiate(wheelOptionButtonPrefab, wheelOptionsContainer);
            var label = btn.GetComponentInChildren<TMP_Text>();
            if (label != null) label.text = opt.Label;

            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() =>
            {
                if (!string.IsNullOrEmpty(opt.NextNodeId))
                {
                    ShowNode(opt.NextNodeId, pushHistory: true);
                }
                else if (!string.IsNullOrEmpty(opt.ResponseLine))
                {
                    esaiResponseText.text = opt.ResponseLine;
                    lastLine = opt.ResponseLine;
                    // For MVP, keep the options up.
                }
            });
        }
    }


    private void ClearOptions()
    {
        for (int i = wheelOptionsContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(wheelOptionsContainer.GetChild(i).gameObject);
        }
    }

    private class Node
    {
        public string EsaiLine;
        public List<Option> Options;

        public Node(string esaiLine, List<Option> options)
        {
            EsaiLine = esaiLine;
            Options = options;
        }
    }

    private class Option
    {
        public string Label;
        public string NextNodeId;
        public string ResponseLine;

        public Option(string label, string nextNodeId = null, string responseLine = null)
        {
            Label = label;
            NextNodeId = nextNodeId;
            ResponseLine = responseLine;
        }
    }

    public void BackOne()
    {
        if (nodeHistory.Count > 0)
        {
            var prev = nodeHistory.Pop();
            ShowNode(prev, pushHistory: false); // IMPORTANT: don’t re-push when going back
        }
        else
        {
            // Already at earliest point; do nothing.
            // (Or optionally keep showing current node)
        }
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
