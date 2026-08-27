using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ItemSpawnerEnhanced.UI;

internal sealed class ItemTile
{
    public ItemTile(
        GameItemRecord record,
        GameObject gameObject,
        Button button,
        Image background,
        TMP_Text label,
        RawImage icon,
        GameObject favoriteMarker,
        ItemFavoriteTrigger favoriteTrigger,
        ItemSpawnTrigger spawnTrigger)
    {
        Record = record;
        GameObject = gameObject;
        Button = button;
        _background = background;
        _label = label;
        _icon = icon;
        FavoriteMarker = favoriteMarker;
        FavoriteTrigger = favoriteTrigger;
        SpawnTrigger = spawnTrigger;
        _focusFrame = RuntimeUiFactory.CreateFocusFrame(button.transform);
        _interaction = button.gameObject.AddComponent<ControlInteractionState>();
        _interaction.Changed += Render;
        button.transition = Selectable.Transition.None;
        Render();
    }

    private readonly Image _background;
    private readonly TMP_Text _label;
    private readonly RawImage _icon;
    private readonly Image _focusFrame;
    private readonly ControlInteractionState _interaction;
    private bool _canSpawn;

    public GameItemRecord Record { get; }
    public GameObject GameObject { get; }
    public Button Button { get; }
    public GameObject FavoriteMarker { get; }
    public ItemFavoriteTrigger FavoriteTrigger { get; }
    public ItemSpawnTrigger SpawnTrigger { get; }
    public RectTransform RectTransform => (RectTransform)GameObject.transform;
    public bool CanSpawn => _canSpawn;

    public void SetFavorite(bool favorite) => FavoriteMarker.SetActive(favorite);

    public void SetSpawnEnabled(bool enabled)
    {
        _canSpawn = enabled;
        SpawnTrigger.SpawnEnabled = enabled;
        Render();
    }

    public void SetFocused(bool focused) => _interaction.SetFocused(focused);

    public void BeginControllerSubmit()
    {
        _interaction.SetControllerPressed(true);
        SpawnTrigger.BeginControllerHold();
    }

    public void EndControllerSubmit()
    {
        SpawnTrigger.EndControllerHold();
        _interaction.SetControllerPressed(false);
    }

    public bool TryToggleFavorite() => FavoriteTrigger.TryInvoke();

    public bool HasTruncatedName()
    {
        _label.ForceMeshUpdate();
        return _label.isTextOverflowing;
    }

    private void Render()
    {
        Core.ControlVisualState state = Core.ControllerVisualState.ForItem(
            _canSpawn,
            _interaction.Focused,
            _interaction.Hovered,
            _interaction.Pressed);
        _background.color = RuntimeUiFactory.ResolvePalette(state.Background);
        _label.color = RuntimeUiFactory.ResolvePalette(state.Foreground);
        _icon.color = new Color(1, 1, 1, _canSpawn ? 1f : 0.55f);
        _focusFrame.gameObject.SetActive(state.ShowFocus);
    }
}
