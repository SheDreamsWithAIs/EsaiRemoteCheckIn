using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Session state enums — see CheckInFlowDesign.md: Internal State Model
public enum EventType   { None, Conflict, Loss, Disappointment, Overwhelm }
public enum EmotionState { Unknown, Anxious, Sad, Angry, Numb, Empty, Neutral }
public enum SupportType  { None, Vent, Grounding, Reassurance, Boundary, OneStep }

public class WheelMenuController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text esaiResponseText;
    [SerializeField] private Transform wheelOptionsContainer;
    [SerializeField] private Transform wheelOptionsLayoutParent;
    [SerializeField] private Button wheelOptionButtonPrefab;
    [SerializeField] private GameObject endOverlayPanel;
    [SerializeField] private UnityEngine.UI.Image portraitImage;

    [Header("Dialogue Data")]
    [SerializeField] private PortraitDatabaseSO portraitDatabase;
    [SerializeField] private DialogueModuleSO checkInModule;
    [SerializeField] private bool useModule;
    [SerializeField] private ConversationFlowSO conversationFlow;
    [SerializeField] private bool useFlow;

    [Header("End Overlay")]
    [SerializeField] private float endOverlayDelaySeconds = 1.5f;

    [Header("Tooltip")]
    [SerializeField] private RectTransform tooltipPanel;
    [SerializeField] private TMP_Text tooltipText;

    private readonly LinesService _linesService = new();
    private PortraitResolver _portraitResolver;
    private ConversationRunner _conversationRunner;
    private readonly Dictionary<string, Node> nodes = new();
    private Coroutine endOverlayCoroutine;
    private readonly Stack<HistoryEntry> nodeHistory = new();
    private string currentNodeId = "root";
    private string currentEntryContext;
    private int currentSelectedVariantId = -1;
    private string lastLine = "";
    private bool isShowingResponseLine;
    private PortraitKey _currentResolvedPortraitKey;

    /// <summary>True when the player can go back one step.</summary>
    public bool CanGoBack => nodeHistory.Count > 0;

    // Persistent session state — see CheckInFlowDesign.md: State Reset & Loop Behavior.
    // Set by node routing; cleared on root re-entry and session close.
    // Current use: entryContext chaining. Future use: action mode lead-in variants.
    public EventType    CurrentEvent   { get; private set; } = EventType.None;
    public EmotionState CurrentEmotion { get; private set; } = EmotionState.Unknown;
    public SupportType  CurrentSupport { get; private set; } = SupportType.None;

    private struct HistoryEntry
    {
        public string NodeId;
        public bool IsShowingResponseLine;
        public string EntryContext;
        public int SelectedVariantId;
        public PortraitKey ResolvedPortraitKey;

        public HistoryEntry(string nodeId, bool isShowingResponseLine, string entryContext, int selectedVariantId, PortraitKey resolvedPortraitKey = default)
        {
            NodeId = nodeId;
            IsShowingResponseLine = isShowingResponseLine;
            EntryContext = entryContext;
            SelectedVariantId = selectedVariantId;
            ResolvedPortraitKey = resolvedPortraitKey;
        }
    }

    private void Awake()
    {
        _portraitResolver = portraitDatabase != null ? new PortraitResolver(portraitDatabase) : null;
        if (portraitDatabase != null) portraitDatabase.BuildLookup();
        if (useFlow && conversationFlow != null)
        {
            _conversationRunner = new ConversationRunner();
            _conversationRunner.Init(conversationFlow);
            var mod = _conversationRunner.CurrentModule;
            if (mod != null) LoadFromModule(mod);
            else BuildNodes();
        }
        else if (useModule && checkInModule != null)
        {
            LoadFromModule(checkInModule);
        }
        else
        {
            BuildNodes();
        }
        var entryNode = GetEntryNodeId();
        ShowNode(entryNode, pushHistory: false, entryContext: null, replayVariantId: -1);
    }

    private string GetEntryNodeId()
    {
        if (useFlow && _conversationRunner != null) return _conversationRunner.EntryNodeId ?? "root";
        if (useModule && checkInModule != null) return checkInModule.entryNodeId ?? "root";
        return "root";
    }

    private void LoadFromModule(DialogueModuleSO module)
    {
        nodes.Clear();
        module.BuildLookup();
        var lookup = module.GetNodeLookup();
        foreach (var kv in lookup)
        {
            var def = kv.Value;
            var opts = new List<Option>();
            if (def.options != null)
            {
                foreach (var o in def.options)
                {
                    opts.Add(new Option(
                        o.labelText,
                        o.next,
                        null,
                        o.entryContext,
                        o.labelKey,
                        o.responseTextKey,
                        o.specialNext
                    ));
                }
            }
            var textKeyByContext = (Dictionary<string, string>)null;
            if (def.textKeyByContext != null && def.textKeyByContext.Length > 0)
            {
                textKeyByContext = new Dictionary<string, string>();
                foreach (var e in def.textKeyByContext)
                {
                    if (!string.IsNullOrEmpty(e.context)) textKeyByContext[e.context] = e.textKey;
                }
            }
            var adv = def.advanceMode == AdvanceModeDef.TapToContinue ? AdvanceMode.TapToContinue : AdvanceMode.WaitForChoice;
            nodes[def.nodeId] = new Node(
                def.esaiLine ?? "",
                opts.Count > 0 ? opts : null,
                def.triggersEndOverlay,
                null,
                def.textKey,
                textKeyByContext,
                adv,
                def.tapContinueNodeId,
                def.portraitRequest
            );
        }
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
            },
            textKey: "root.greeting"
        );

        nodes["okay"] = new Node(
            "I'm glad you're steady today. Want to keep it light, or check in anyway?",
            new List<Option>
            {
                new Option("Keep it light.", responseTextKey: "okay.keep_light"),
                new Option("Quick check-in.", responseTextKey: "okay.quick_checkin"),
                new Option("Actually... I need support.", nextNodeId: "support")
            },
            textKey: "okay.prompt"
        );

        nodes["support"] = new Node(
            "Then that's what we'll do. What kind of support fits right now?",
            new List<Option>
            {
                new Option("Reassurance.", nextNodeId: "reassurance_response"),
                new Option("Grounding.", nextNodeId: "grounding_exercise"),
                new Option("Help me choose one step.", responseTextKey: "support.one_step")
            },
            textKey: "support.prompt"
        );

        nodes["something"] = new Node(
            "I'm listening. What kind of 'something' was it?",
            new List<Option>
            {
                new Option("Conflict / people.", responseTextKey: "something.conflict"),
                new Option("Bad news.", responseTextKey: "something.bad_news"),
                new Option("Overwhelm / too much.", responseTextKey: "something.overwhelm")
            },
            textKey: "something.prompt"
        );

        nodes["dontknow"] = new Node(
            "That's okay. 'I don't know' still counts as information. Let's narrow it down.",
            new List<Option>
            {
                new Option("Body feels bad.", responseTextKey: "dontknow.body"),
                new Option("Emotion feels bad.", responseTextKey: "dontknow.emotion"),
                new Option("Just empty.", responseTextKey: "dontknow.empty")
            },
            textKey: "dontknow.prompt"
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
                new Option("Yes.", nextNodeId: "grounding_success_response"),
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
                new Option("Yes.", nextNodeId: "grounding_success_response"),
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
                new Option("Okay.", nextNodeId: "redirect_prelude", labelKey: "labels.okay")
            },
            textKey: "grounding.failed.dontknow"
        );

        nodes["follow_up"] = new Node(
            "Okay. Where would you like to go from here?",
            new List<Option>
            {
                new Option("Okay.", nextNodeId: "hub_checkin", labelKey: "labels.okay")
            },
            textKey: "follow_up.prompt"
        );

        nodes["redirect_prelude"] = new Node(
            "Let's try something completely different.",
            new List<Option>
            {
                new Option("Okay.", nextNodeId: "hub_redirect", labelKey: "labels.okay")
            },
            textKey: "redirect.prelude"
        );

        // --- Hubs ---
        nodes["hub_checkin"] = new Node(
            "Where would you like to go from here?",
            new List<Option>
            {
                new Option("Check in again.", nextNodeId: "root"),
                new Option("I'll get back to the day.", nextNodeId: "session_close"),
                new Option("Something else.", nextNodeId: "coming_soon")
            },
            textKey: "hub.checkin"
        );

        nodes["hub_redirect"] = new Node(
            "Where would you like to go from here?",
            new List<Option>
            {
                new Option("Check in again.", nextNodeId: "root"),
                new Option("I'll get back to the day.", nextNodeId: "session_close"),
                new Option("Something else.", nextNodeId: "coming_soon")
            },
            textKey: "hub.checkin"
        );

        nodes["coming_soon"] = new Node(
            "Other modes aren't ready yet. Check back soon.",
            new List<Option>
            {
                new Option("Back.", nextNodeId: "hub_checkin", labelKey: "labels.back")
            },
            textKey: "coming_soon"
        );

        // grounding_success_response — acknowledgment beat after grounding succeeds.
        // TapToContinue so the player can absorb the moment before the hub.
        nodes["grounding_success_response"] = new Node(
            "",
            options: null,
            textKey: "grounding.success.response",
            advanceMode: AdvanceMode.TapToContinue,
            tapContinueNodeId: "hub_checkin"
        );

        // session_close — uses SpecialNext.End so EndSession() fires explicitly on button click.
        // More reliable than triggersEndOverlay+null-options, which depends on a serialized bool
        // in the module asset and a coroutine delay.
        nodes["session_close"] = new Node(
            "Take care of yourself. I'm here whenever you need.",
            options: new List<Option>
            {
                new Option("Okay.", specialNext: SpecialNext.End, labelKey: "labels.okay")
            },
            textKey: "session_close"
        );
    }

    private void ShowNode(string nodeId, bool pushHistory, string entryContext = null, int replayVariantId = -1, PortraitKey? replayPortraitKey = null)
    {
        if (endOverlayCoroutine != null)
        {
            StopCoroutine(endOverlayCoroutine);
            endOverlayCoroutine = null;
        }

        // Clear session state whenever we return to root (Check in again flow).
        // Does not fire on initial Awake load (pushHistory is false there).
        if (nodeId == "root" && pushHistory)
            ClearSessionState();

        if (pushHistory && (currentNodeId != nodeId || isShowingResponseLine))
        {
            nodeHistory.Push(new HistoryEntry(currentNodeId, isShowingResponseLine, currentEntryContext, currentSelectedVariantId, _currentResolvedPortraitKey));
        }

        currentNodeId = nodeId;
        currentEntryContext = entryContext;
        isShowingResponseLine = false;

        if (!nodes.TryGetValue(nodeId, out var node))
        {
            Debug.LogError($"Node not found: {nodeId}");
            return;
        }

        string displayLine = node.GetDisplayLine(_linesService, entryContext, replayVariantId >= 0 ? replayVariantId : (int?)null, out int variantId, out string effectiveTextKey);
        currentSelectedVariantId = variantId;
        esaiResponseText.text = displayLine;
        lastLine = displayLine;

        if (_portraitResolver != null && portraitImage != null)
        {
            Sprite portraitSprite;
            if (replayPortraitKey.HasValue)
            {
                portraitSprite = _portraitResolver.ResolveByKey(replayPortraitKey.Value);
                _currentResolvedPortraitKey = replayPortraitKey.Value;
            }
            else
            {
                var portraitReq = !string.IsNullOrEmpty(effectiveTextKey)
                    ? _linesService.GetPortraitRequest(effectiveTextKey)
                    : (PortraitRequest?)null;
                var req = portraitReq ?? node.PortraitRequest;
                var result = _portraitResolver.Resolve(req);
                portraitSprite = result.sprite;
                _currentResolvedPortraitKey = result.resolvedKey;
            }
            portraitImage.sprite = portraitSprite;
            portraitImage.enabled = portraitSprite != null;
        }

        ClearOptions();

        // TapToContinue: show a single Continue button that advances to the next node.
        // The current entryContext is forwarded so context-aware nodes downstream render correctly.
        if (node.AdvanceMode == AdvanceMode.TapToContinue && !string.IsNullOrEmpty(node.TapContinueNodeId))
        {
            if (wheelOptionsContainer != null) wheelOptionsContainer.gameObject.SetActive(true);
            var tapLayoutParent = wheelOptionsLayoutParent != null ? wheelOptionsLayoutParent : wheelOptionsContainer;
            var tapBtn = Instantiate(wheelOptionButtonPrefab, tapLayoutParent);
            var tapLabel = tapBtn.GetComponentInChildren<TMP_Text>();
            if (tapLabel != null)
            {
                var labelResult = _linesService.GetLine("labels.continue");
                tapLabel.text = labelResult.text;
            }
            var tapNextId = node.TapContinueNodeId;
            var tapContext = currentEntryContext;
            tapBtn.onClick.AddListener(() => ShowNode(tapNextId, pushHistory: true, tapContext, replayVariantId: -1));
            if (tapLayoutParent is RectTransform tapRt && tapLayoutParent.GetComponent<RadialLayoutGroup>() != null)
                LayoutRebuilder.ForceRebuildLayoutImmediate(tapRt);
            return;
        }

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

        var layoutParent = wheelOptionsLayoutParent != null ? wheelOptionsLayoutParent : wheelOptionsContainer;
        foreach (var opt in node.Options)
        {
            var btn = Instantiate(wheelOptionButtonPrefab, layoutParent);
            var label = btn.GetComponentInChildren<TMP_Text>();
            if (label != null)
            {
                if (!string.IsNullOrEmpty(opt.LabelKey))
                {
                    var labelResult = _linesService.GetLine(opt.LabelKey);
                    label.text = labelResult.text;
                }
                else
                {
                    label.text = opt.Label;
                }
            }

            // Tooltip — shows full label text on hover for buttons where label may be truncated.
            // Requires tooltipPanel and tooltipText to be assigned in the Inspector.
            var captureFullLabel = label != null ? label.text : opt.Label;
            var tooltipEvt = btn.gameObject.AddComponent<EventTrigger>();
            var enterEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
            enterEntry.callback.AddListener(data => ShowTooltip(captureFullLabel, ((PointerEventData)data).position));
            tooltipEvt.triggers.Add(enterEntry);
            var exitEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
            exitEntry.callback.AddListener(_ => HideTooltip());
            tooltipEvt.triggers.Add(exitEntry);

            btn.onClick.RemoveAllListeners();
            var captureOpt = opt;
            btn.onClick.AddListener(() =>
            {
                HideTooltip();

                // SpecialNext.End — fires EndSession directly. Used on session_close "Okay." button.
                // More reliable than the triggersEndOverlay+null-options path since it's explicit
                // and not subject to stale serialized bool values in the module asset.
                if (captureOpt.SpecialNext == SpecialNext.End)
                {
                    EndSession();
                    return;
                }

                if (captureOpt.SpecialNext == SpecialNext.NextModule && _conversationRunner != null)
                {
                    var entryNodeId = _conversationRunner.AdvanceToNextModule();
                    if (!string.IsNullOrEmpty(entryNodeId))
                    {
                        var mod = _conversationRunner.CurrentModule;
                        if (mod != null) LoadFromModule(mod);
                        ShowNode(entryNodeId, pushHistory: true, captureOpt.EntryContext, replayVariantId: -1);
                    }
                    return;
                }

                // Two-beat: responseTextKey (Beat 1 empathy) + next (Beat 2 destination).
                // Shows Beat 1 inline, then replaces options with a single Continue button.
                if (!string.IsNullOrEmpty(captureOpt.ResponseTextKey) && !string.IsNullOrEmpty(captureOpt.NextNodeId))
                {
                    var lineResult = _linesService.GetLine(captureOpt.ResponseTextKey);
                    nodeHistory.Push(new HistoryEntry(currentNodeId, isShowingResponseLine, currentEntryContext, currentSelectedVariantId, _currentResolvedPortraitKey));
                    isShowingResponseLine = true;
                    esaiResponseText.text = lineResult.text;
                    lastLine = lineResult.text;
                    if (_portraitResolver != null && portraitImage != null && nodes.TryGetValue(currentNodeId, out var beatOneNode))
                    {
                        var portraitReq = lineResult.portraitRequest ?? beatOneNode.PortraitRequest;
                        var result = _portraitResolver.Resolve(portraitReq);
                        portraitImage.sprite = result.sprite;
                        portraitImage.enabled = result.sprite != null;
                        _currentResolvedPortraitKey = result.resolvedKey;
                    }
                    ClearOptions();
                    var beatLp = wheelOptionsLayoutParent != null ? wheelOptionsLayoutParent : wheelOptionsContainer;
                    var continueBtn = Instantiate(wheelOptionButtonPrefab, beatLp);
                    var continueLabel = continueBtn.GetComponentInChildren<TMP_Text>();
                    if (continueLabel != null)
                    {
                        var lr = _linesService.GetLine("labels.continue");
                        continueLabel.text = lr.text;
                    }
                    var beatTwoNodeId = captureOpt.NextNodeId;
                    var beatTwoContext = captureOpt.EntryContext;
                    continueBtn.onClick.AddListener(() => ShowNode(beatTwoNodeId, pushHistory: true, beatTwoContext, replayVariantId: -1));
                    if (beatLp is RectTransform beatRt && beatLp.GetComponent<RadialLayoutGroup>() != null)
                        LayoutRebuilder.ForceRebuildLayoutImmediate(beatRt);
                    return;
                }

                if (!string.IsNullOrEmpty(captureOpt.NextNodeId))
                {
                    ShowNode(captureOpt.NextNodeId, pushHistory: true, captureOpt.EntryContext, replayVariantId: -1);
                }
                else if (!string.IsNullOrEmpty(captureOpt.ResponseTextKey))
                {
                    var lineResult = _linesService.GetLine(captureOpt.ResponseTextKey);
                    nodeHistory.Push(new HistoryEntry(currentNodeId, isShowingResponseLine, currentEntryContext, currentSelectedVariantId, _currentResolvedPortraitKey));
                    isShowingResponseLine = true;
                    esaiResponseText.text = lineResult.text;
                    lastLine = lineResult.text;
                    if (_portraitResolver != null && portraitImage != null && nodes.TryGetValue(currentNodeId, out var respNode))
                    {
                        var portraitReq = lineResult.portraitRequest ?? respNode.PortraitRequest;
                        var result = _portraitResolver.Resolve(portraitReq);
                        portraitImage.sprite = result.sprite;
                        portraitImage.enabled = result.sprite != null;
                        _currentResolvedPortraitKey = result.resolvedKey;
                    }
                }
                else if (!string.IsNullOrEmpty(captureOpt.ResponseLine))
                {
                    nodeHistory.Push(new HistoryEntry(currentNodeId, isShowingResponseLine, currentEntryContext, currentSelectedVariantId, _currentResolvedPortraitKey));
                    isShowingResponseLine = true;
                    esaiResponseText.text = captureOpt.ResponseLine;
                    lastLine = captureOpt.ResponseLine;
                    if (_portraitResolver != null && portraitImage != null && nodes.TryGetValue(currentNodeId, out var respNode))
                    {
                        var result = _portraitResolver.Resolve(respNode.PortraitRequest);
                        portraitImage.sprite = result.sprite;
                        portraitImage.enabled = result.sprite != null;
                        _currentResolvedPortraitKey = result.resolvedKey;
                    }
                }
            });
        }
        if (layoutParent is RectTransform rt && layoutParent.GetComponent<RadialLayoutGroup>() != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
    }

    private IEnumerator ShowEndOverlayAfterDelay()
    {
        yield return new WaitForSeconds(endOverlayDelaySeconds);
        EndSession();
    }

    private void ClearOptions()
    {
        HideTooltip();
        var layoutParent = wheelOptionsLayoutParent != null ? wheelOptionsLayoutParent : wheelOptionsContainer;
        if (layoutParent == null) return;
        for (int i = layoutParent.childCount - 1; i >= 0; i--)
        {
            Destroy(layoutParent.GetChild(i).gameObject);
        }
    }

    private void ShowTooltip(string text, Vector2 screenPos)
    {
        if (tooltipPanel == null || tooltipText == null) return;
        tooltipText.text = text;
        tooltipPanel.gameObject.SetActive(true);
        var parentRect = tooltipPanel.parent as RectTransform;
        if (parentRect != null &&
            RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, screenPos, null, out var localPt))
        {
            // Offset upward so the tooltip doesn't sit under the cursor/finger.
            tooltipPanel.anchoredPosition = localPt + Vector2.up * 60f;
        }
    }

    private void HideTooltip()
    {
        if (tooltipPanel != null)
            tooltipPanel.gameObject.SetActive(false);
    }

    private class Node
    {
        public string EsaiLine;
        public string TextKey;
        public Dictionary<string, string> TextKeyByContext;
        public List<Option> Options;
        public bool TriggersEndOverlay;
        public Dictionary<string, string> EsaiLineByContext;
        public AdvanceMode AdvanceMode;
        public string TapContinueNodeId;
        public PortraitRequest PortraitRequest;

        public Node(string esaiLine, List<Option> options, bool triggersEndOverlay = false,
            Dictionary<string, string> esaiLineByContext = null, string textKey = null,
            Dictionary<string, string> textKeyByContext = null, AdvanceMode advanceMode = AdvanceMode.WaitForChoice,
            string tapContinueNodeId = null, PortraitRequest? portraitRequest = null)
        {
            EsaiLine = esaiLine;
            Options = options;
            TriggersEndOverlay = triggersEndOverlay;
            EsaiLineByContext = esaiLineByContext;
            TextKey = textKey;
            TextKeyByContext = textKeyByContext;
            AdvanceMode = advanceMode;
            TapContinueNodeId = tapContinueNodeId;
            PortraitRequest = portraitRequest ?? new PortraitRequest(PortraitMood.Friendly, 0, PortraitModifier.Default);
        }

        public string GetDisplayLine(LinesService lines, string entryContext, int? replayVariantId, out int variantId, out string effectiveTextKey)
        {
            effectiveTextKey = null;
            string key = null;
            if (!string.IsNullOrEmpty(entryContext) && TextKeyByContext != null &&
                TextKeyByContext.TryGetValue(entryContext, out var ctxKey))
                key = ctxKey;
            else if (!string.IsNullOrEmpty(TextKey))
                key = TextKey;

            if (!string.IsNullOrEmpty(key))
            {
                effectiveTextKey = key;
                if (replayVariantId.HasValue && replayVariantId.Value >= 0)
                {
                    variantId = replayVariantId.Value;
                    return lines.GetLineReplay(key, replayVariantId.Value);
                }
                var result = lines.GetLine(key);
                variantId = result.selectedVariantId;
                return result.text;
            }

            variantId = -1;
            if (!string.IsNullOrEmpty(entryContext) && EsaiLineByContext != null &&
                EsaiLineByContext.TryGetValue(entryContext, out var line))
                return line;
            return EsaiLine;
        }
    }

    private enum AdvanceMode { WaitForChoice, TapToContinue }

    private class Option
    {
        public string Label;
        public string LabelKey;
        public string NextNodeId;
        public string ResponseLine;
        public string ResponseTextKey;
        public string EntryContext;
        public SpecialNext SpecialNext;

        public Option(string label, string nextNodeId = null, string responseLine = null, string entryContext = null, string labelKey = null, string responseTextKey = null, SpecialNext specialNext = SpecialNext.None)
        {
            Label = label;
            NextNodeId = nextNodeId;
            ResponseLine = responseLine;
            EntryContext = entryContext;
            LabelKey = labelKey;
            ResponseTextKey = responseTextKey;
            SpecialNext = specialNext;
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
        ShowNode(entry.NodeId, pushHistory: false, entry.EntryContext, entry.SelectedVariantId, entry.ResolvedPortraitKey);
    }

    public void SayAgain()
    {
        if (!string.IsNullOrEmpty(lastLine))
            esaiResponseText.text = lastLine;
    }

    public void EndSession()
    {
        ClearSessionState();
        if (endOverlayPanel != null)
            endOverlayPanel.SetActive(true);
    }

    /// <summary>
    /// Resets all session state variables to defaults.
    /// Called on root re-entry ("Check in again") and on session close.
    /// See CheckInFlowDesign.md: State Reset &amp; Loop Behavior.
    /// </summary>
    private void ClearSessionState()
    {
        CurrentEvent   = EventType.None;
        CurrentEmotion = EmotionState.Unknown;
        CurrentSupport = SupportType.None;
    }

    public void CancelEndSession()
    {
        if (endOverlayPanel != null)
            endOverlayPanel.SetActive(false);
    }
}
