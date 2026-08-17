using UnityEngine;
using UnityEngine.UI;

namespace ItemSpawnerEnhanced.UI;

internal sealed class ItemTile
{
    public ItemTile(
        GameItemRecord record,
        GameObject gameObject,
        Button button,
        GameObject favoriteMarker,
        ItemFavoriteTrigger favoriteTrigger)
    {
        Record = record;
        GameObject = gameObject;
        Button = button;
        FavoriteMarker = favoriteMarker;
        FavoriteTrigger = favoriteTrigger;
    }

    public GameItemRecord Record { get; }
    public GameObject GameObject { get; }
    public Button Button { get; }
    public GameObject FavoriteMarker { get; }
    public ItemFavoriteTrigger FavoriteTrigger { get; }

    public void SetFavorite(bool favorite) => FavoriteMarker.SetActive(favorite);
}
