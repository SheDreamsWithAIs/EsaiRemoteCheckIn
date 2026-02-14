# Esai Dialogue System: Setup & Content Guide

## Part 1: Flipping Over to the New System

### Step 1: Create the Portrait Database

1. In Unity, go to **Esai > Create Portrait Database**.
2. This creates `Assets/ScriptableObjects/Dialogue/PortraitDatabase.asset` with a Neutral/0/Default fallback.
3. Assign your portrait sprites in the Inspector (e.g. `EsaiDefaultSmilePortrait` from `Assets/Art/Portraits/`).

### Step 2: Assign References on WheelMenuController

1. Select the GameObject with `WheelMenuController`.
2. In the Inspector, under **Dialogue Data**:
   - **Portrait Database** → drag the `PortraitDatabase` asset.
   - **Portrait Image** → drag the UI Image used for Esai's portrait.
3. If you want to use the module-based flow:
   - **Check-In Module** → drag the Check-In Module asset (created in Step 3).
   - Enable **Use Module**.
4. For multi-module flows:
   - **Conversation Flow** → drag a `ConversationFlowSO` asset.
   - Enable **Use Flow**.

### Step 3: Create the Check-In Module (Optional)

1. Go to **Esai > Create Check-In Module (Grounding Branch)**.
2. This creates `Assets/ScriptableObjects/Dialogue/Modules/CheckInModule.asset` with the full check-in flow.
3. Assign it to **Check-In Module** and enable **Use Module** on `WheelMenuController`.

### Step 4: Validate Content

1. Go to **Tools > Esai > Validate Content**.
2. Fix any reported missing keys or portrait gaps.

---

## Part 2: Adding Real Content

### Adding Dialogue Lines (Lines.json)

Lines live in `Assets/Resources/Dialogue/Lines.json`.

**Format:**

```json
{
  "entries": [
    {
      "key": "your.key.here",
      "variants": [
        { "text": "Your dialogue text here.", "weight": 1 },
        { "text": "Alternative variant.", "weight": 1 }
      ],
      "rules": { "noRepeatWindow": 5 }
    }
  ]
}
```

- **key** – Used by nodes via `textKey` or `responseTextKey`.
- **variants** – One or more lines; higher `weight` = more likely to be chosen.
- **rules.noRepeatWindow** – Optional; avoids repeating the same variant for the last N picks.

**Example – new reassurance line:**

```json
{
  "key": "reassure.general",
  "variants": [
    { "text": "Hey. You're okay. You don't have to solve everything today.", "weight": 1 },
    { "text": "You don't need to earn rest.", "weight": 1 },
    { "text": "Your new line here.", "weight": 1 }
  ],
  "rules": { "noRepeatWindow": 5 }
}
```

### Adding Option Labels (Lines.json)

For options that use `labelKey`, add entries like:

```json
{
  "key": "labels.your_label",
  "variants": [{ "text": "Button Text", "weight": 1 }]
}
```

### Adding Nodes (Hardcoded Path)

If **Use Module** is off, edit `WheelMenuController.BuildNodes()`:

```csharp
nodes["your_node_id"] = new Node(
    "Fallback line if textKey missing",
    new List<Option>
    {
        new Option("Button text", nextNodeId: "next_node"),
        new Option("Response only", responseTextKey: "your.response.key"),
        new Option("With label key", nextNodeId: "other", labelKey: "labels.okay")
    },
    textKey: "your.node.textkey"
);
```

### Adding Nodes (Module Path)

If **Use Module** is on, edit the Check-In Module asset:

1. Select `CheckInModule.asset`.
2. In the Inspector, expand **Nodes**.
3. Add a new element and set:
   - **Node Id** – Unique ID (e.g. `my_new_node`).
   - **Text Key** – Key in `Lines.json`.
   - **Options** – Array of options with:
     - **Label Text** or **Label Key**
     - **Next** – Target node ID.
     - **Response Text Key** – For response-only options.
     - **Entry Context** – For context-specific text.
     - **Special Next** – For `NextModule`, `Hub`, `End`.

### Adding Portraits

1. Open `PortraitDatabase.asset`.
2. Add entries with **Mood**, **Intensity** (0–4), **Modifier**, and **Sprite**.
3. In nodes, set **Portrait Request** (mood, intensity, modifier) to choose which portrait is used.

---

## Quick Reference: Where Things Live

| What | Where |
|------|-------|
| Dialogue text | `Assets/Resources/Dialogue/Lines.json` |
| Portrait sprites | `PortraitDatabase.asset` |
| Node structure (module mode) | `CheckInModule.asset` (or your module) |
| Node structure (hardcoded) | `WheelMenuController.cs` → `BuildNodes()` |
| Validation | **Tools > Esai > Validate Content** |

---

## Workflow for Adding 50 New Lines

1. Add entries to `Lines.json` with new keys.
2. Reference those keys in nodes (via `textKey` or `responseTextKey`).
3. Run **Tools > Esai > Validate Content**.
4. Test in Play mode.
