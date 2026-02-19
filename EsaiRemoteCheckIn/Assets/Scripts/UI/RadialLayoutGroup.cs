using UnityEngine;

/// <summary>
/// Arranges child RectTransforms in a circle. Supports 1–6 (or more) buttons.
/// Replace GridLayoutGroup on WheelOptionsContainer for radial option layout.
/// </summary>
public class RadialLayoutGroup : UnityEngine.UI.LayoutGroup
{
    [SerializeField] private float radius = 200f;
    [SerializeField] private float startAngle = 90f;
    [SerializeField] private Vector2 childSize = new Vector2(430f, 100f);
    [SerializeField] private bool rotateChildrenToRadius;
    [Tooltip("Corrects the circular appearance when arranging rectangular UI elements in a radial layout within a rectangular space. Uses an ellipse instead of a circle so middle buttons spread apart and top/bottom buttons pull in.")]
    [SerializeField] private bool useRectangularCorrection = true;
    [SerializeField] [Range(0.8f, 1.5f)] private float horizontalRadiusMultiplier = 1.2f;
    [SerializeField] [Range(0.5f, 1.2f)] private float verticalRadiusMultiplier = 0.85f;

    public float Radius { get => radius; set { radius = value; SetDirty(); } }
    public float StartAngle { get => startAngle; set { startAngle = value; SetDirty(); } }
    public Vector2 ChildSize { get => childSize; set { childSize = value; SetDirty(); } }
    public bool UseRectangularCorrection { get => useRectangularCorrection; set { useRectangularCorrection = value; SetDirty(); } }

    public override void SetLayoutHorizontal()
    {
        SetRadial();
    }

    public override void SetLayoutVertical()
    {
        SetRadial();
    }

    public override void CalculateLayoutInputHorizontal()
    {
        float radiusX = useRectangularCorrection ? radius * horizontalRadiusMultiplier : radius;
        float totalSize = radiusX * 2f + childSize.x;
        SetLayoutInputForAxis(totalSize, totalSize, -1, 0);
    }

    public override void CalculateLayoutInputVertical()
    {
        float radiusY = useRectangularCorrection ? radius * verticalRadiusMultiplier : radius;
        float totalSize = radiusY * 2f + childSize.y;
        SetLayoutInputForAxis(totalSize, totalSize, -1, 1);
    }

    protected override void OnDisable()
    {
        m_Tracker.Clear();
        base.OnDisable();
    }

    private void SetRadial()
    {
        int count = 0;
        for (int i = 0; i < rectTransform.childCount; i++)
        {
            var rt = rectTransform.GetChild(i) as RectTransform;
            if (rt != null && rt.gameObject.activeSelf) count++;
        }
        if (count == 0) return;

        m_Tracker.Clear();
        float effectiveRadius = count == 1 ? 0f : radius;
        float radiusX = useRectangularCorrection ? effectiveRadius * horizontalRadiusMultiplier : effectiveRadius;
        float radiusY = useRectangularCorrection ? effectiveRadius * verticalRadiusMultiplier : effectiveRadius;
        float angleStep = count == 1 ? 0f : 360f / count;

        int index = 0;
        for (int i = 0; i < rectTransform.childCount; i++)
        {
            var child = rectTransform.GetChild(i) as RectTransform;
            if (child == null || !child.gameObject.activeSelf) continue;

            float angleDeg = startAngle - angleStep * index;
            float angleRad = angleDeg * Mathf.Deg2Rad;

            float x = radiusX * Mathf.Cos(angleRad);
            float y = radiusY * Mathf.Sin(angleRad);

            child.anchorMin = new Vector2(0.5f, 0.5f);
            child.anchorMax = new Vector2(0.5f, 0.5f);
            child.pivot = new Vector2(0.5f, 0.5f);
            child.anchoredPosition = new Vector2(x, y);
            child.sizeDelta = childSize;

            var drivenProps = DrivenTransformProperties.Anchors | DrivenTransformProperties.AnchoredPosition | DrivenTransformProperties.SizeDelta | DrivenTransformProperties.Pivot;
            if (rotateChildrenToRadius && count > 1)
            {
                child.localEulerAngles = new Vector3(0, 0, -angleDeg);
                drivenProps |= DrivenTransformProperties.Rotation;
            }
            m_Tracker.Add(this, child, drivenProps);
            index++;
        }
    }
}
