using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace ItemSpawnerEnhanced.UI;

internal sealed class ItemFavoriteTrigger : MonoBehaviour, IPointerClickHandler
{
    private UnityAction? _onFavorite;

    public bool InteractionEnabled { get; set; }

    public void Configure(UnityAction onFavorite) => _onFavorite = onFavorite;

    public bool TryInvoke()
    {
        if (!InteractionEnabled)
            return false;
        _onFavorite?.Invoke();
        return _onFavorite != null;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
            TryInvoke();
    }
}
