using UnityEditor;
using UnityEngine;

/// <summary>Creates the default PortraitDatabase asset with Neutral/0/Default fallback.</summary>
public static class CreatePortraitDatabase
{
    private const string AssetPath = "Assets/ScriptableObjects/Dialogue/PortraitDatabase.asset";

    [MenuItem("Esai/Create Portrait Database")]
    public static void Create()
    {
        var db = AssetDatabase.LoadAssetAtPath<PortraitDatabaseSO>(AssetPath);
        if (db != null)
        {
            Debug.Log($"PortraitDatabase already exists at {AssetPath}");
            Selection.activeObject = db;
            return;
        }

        var defaultSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Portraits/EsaiDefaultSmilePortrait.png");
        if (defaultSprite == null)
        {
            Debug.LogWarning("EsaiDefaultSmilePortrait.png not found. Create the asset manually and assign sprites.");
        }

        db = ScriptableObject.CreateInstance<PortraitDatabaseSO>();
        db.entries.Add(new PortraitDatabaseSO.PortraitEntry
        {
            mood = PortraitMood.Neutral,
            intensity = 0,
            modifier = PortraitModifier.Default,
            sprite = defaultSprite
        });

        var dir = System.IO.Path.GetDirectoryName(AssetPath);
        if (!System.IO.Directory.Exists(dir))
            System.IO.Directory.CreateDirectory(dir);

        AssetDatabase.CreateAsset(db, AssetPath);
        AssetDatabase.SaveAssets();
        Debug.Log($"Created PortraitDatabase at {AssetPath}");
        Selection.activeObject = db;
    }
}
