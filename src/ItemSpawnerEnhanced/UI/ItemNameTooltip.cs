using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ItemSpawnerEnhanced.UI;

internal sealed class ItemNameTooltip : MonoBehaviour
{
    private const float ShowDelaySeconds = 0.2f;
    private const float MaxTextWidth = 380f;
    private const float HorizontalPadding = 24f;
    private const float VerticalPadding = 16f;
    private const float PointerOffset = 18f;
    private const float EdgeMargin = 8f;

    private RectTransform _bounds = null!;
    private RectTransform _visual = null!;
    private TextMeshProUGUI _label = null!;
    private ItemNameTooltipTrigger? _owner;
    private string _text = string.Empty;
    private Vector2 _screenPosition;
    private float _showAt;
    private bool _visible;

    public void Configure(RectTransform bounds, RectTransform visual, TextMeshProUGUI label)
    {
        _bounds = bounds;
        _visual = visual;
        _label = label;
        _visual.gameObject.SetActive(false);
    }

    public void Begin(ItemNameTooltipTrigger owner, string text, Vector2 screenPosition)
    {
        _owner = owner;
        _text = text;
        _screenPosition = screenPosition;
        _showAt = Time.unscaledTime + ShowDelaySeconds;
        _visible = false;
        _visual.gameObject.SetActive(false);
    }

    public void Move(ItemNameTooltipTrigger owner, Vector2 screenPosition)
    {
        if (_owner != owner)
            return;

        _screenPosition = screenPosition;
        if (_visible)
            PositionVisual();
    }

    public void End(ItemNameTooltipTrigger owner)
    {
        if (_owner == owner)
            Hide();
    }

    public void Hide()
    {
        _owner = null;
        _text = string.Empty;
        _visible = false;
        if (_visual != null)
            _visual.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (_owner == null || _visible || Time.unscaledTime < _showAt)
            return;

        Show();
    }

    private void Show()
    {
        if (string.IsNullOrWhiteSpace(_text))
            return;

        _label.text = _text;
        Vector2 preferred = _label.GetPreferredValues(_text, MaxTextWidth, 0);
        float textWidth = Mathf.Min(MaxTextWidth, Mathf.Ceil(preferred.x));
        preferred = _label.GetPreferredValues(_text, textWidth, 0);
        _visual.sizeDelta = new Vector2(
            textWidth + HorizontalPadding,
            Mathf.Ceil(preferred.y) + VerticalPadding);
        _visual.SetAsLastSibling();
        _visual.gameObject.SetActive(true);
        _visible = true;
        PositionVisual();
    }

    private void PositionVisual()
    {
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(_bounds, _screenPosition, null, out Vector2 pointer))
            return;

        Rect bounds = _bounds.rect;
        Vector2 size = _visual.sizeDelta;
        float x = pointer.x + PointerOffset;
        float y = pointer.y - PointerOffset;

        if (x + size.x > bounds.xMax - EdgeMargin)
            x = pointer.x - size.x - PointerOffset;
        if (y - size.y < bounds.yMin + EdgeMargin)
            y = pointer.y + size.y + PointerOffset;

        x = Mathf.Clamp(x, bounds.xMin + EdgeMargin, bounds.xMax - size.x - EdgeMargin);
        y = Mathf.Clamp(y, bounds.yMin + size.y + EdgeMargin, bounds.yMax - EdgeMargin);
        _visual.anchoredPosition = new Vector2(x, y);
    }

    private void OnDisable() => Hide();
}

internal sealed class ItemNameTooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerMoveHandler, IPointerExitHandler
{
    private ItemNameTooltip _tooltip = null!;
    private string _text = string.Empty;

    public void Configure(ItemNameTooltip tooltip, string text)
    {
        _tooltip = tooltip;
        _text = text;
    }

    public void SetText(string text) => _text = text;

    public void OnPointerEnter(PointerEventData eventData) => _tooltip.Begin(this, _text, eventData.position);

    public void OnPointerMove(PointerEventData eventData) => _tooltip.Move(this, eventData.position);

    public void OnPointerExit(PointerEventData eventData) => _tooltip.End(this);

    private void OnDisable()
    {
        if (_tooltip != null)
            _tooltip.End(this);
    }
}
