using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ItemSpawnerEnhanced.UI;

internal sealed class ItemSpawnTrigger : MonoBehaviour,
    IPointerDownHandler,
    IPointerUpHandler,
    IPointerExitHandler,
    ISubmitHandler
{
    private const float RepeatDelaySeconds = 0.4f;
    private const float RepeatIntervalSeconds = 0.15f;

    private Button _button = null!;
    private UnityAction? _spawn;
    private float _nextSpawnAt;
    private bool _holding;

    public void Configure(Button button, UnityAction spawn)
    {
        _button = button;
        _spawn = spawn;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left || !CanSpawn())
            return;

        _holding = true;
        _nextSpawnAt = Time.unscaledTime + RepeatDelaySeconds;
        _spawn?.Invoke();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
            _holding = false;
    }

    public void OnPointerExit(PointerEventData eventData) => _holding = false;

    public void OnSubmit(BaseEventData eventData)
    {
        if (CanSpawn())
            _spawn?.Invoke();
    }

    private void Update()
    {
        if (!_holding)
            return;

        if (!CanSpawn())
        {
            _holding = false;
            return;
        }

        float currentTime = Time.unscaledTime;
        if (currentTime < _nextSpawnAt)
            return;

        _nextSpawnAt = currentTime + RepeatIntervalSeconds;
        _spawn?.Invoke();
    }

    private bool CanSpawn() =>
        _button != null && _button.IsActive() && _button.IsInteractable();

    private void OnDisable() => _holding = false;
}
