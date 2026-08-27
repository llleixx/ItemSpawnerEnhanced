using System;
using System.Collections.Generic;
using System.Linq;
using ItemSpawnerEnhanced.Core;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ItemSpawnerEnhanced.UI;

internal sealed class ControllerFocusCoordinator
{
    private const float NavigationDeadZone = 0.55f;
    private const float InitialRepeatDelay = 0.35f;
    private const float RepeatInterval = 0.10f;

    private readonly ItemSpawnerView _view;
    private readonly Action _close;
    private ControllerFocusLocation _focus;
    private ControllerFocusLocation _lastFocus;
    private ControllerFocusLocation _lastTagFocus;
    private ControllerFocusLocation _lastItemFocus;
    private ControllerNavigationDirection? _heldDirection;
    private float _nextNavigationAt;
    private TagToggle? _pressedTag;
    private ItemTile? _pressedTile;
    private bool _tagClearPressed;
    private bool _controllerMode;
    private bool _windowOpen;

    public ControllerFocusCoordinator(ItemSpawnerView view, Action close)
    {
        _view = view;
        _close = close;
    }

    public void OpenWithController()
    {
        _windowOpen = true;
        _controllerMode = true;
        _heldDirection = null;
        ClearEventSystemSelection();
        RestoreFocus(preferFirstItem: true);
    }

    public void OpenWithoutController()
    {
        _windowOpen = true;
        _controllerMode = false;
        _heldDirection = null;
        ClearFocusVisual(_focus);
    }

    public void Close()
    {
        ReleasePressedControl();
        if (_view.TargetDropdown.Dropdown.IsExpanded)
            _view.TargetDropdown.Dropdown.Hide();
        _view.TargetDropdown.SetExpanded(false);
        ClearFocusVisual(_focus);
        ClearDropdownOptionFocus();
        _view.HideTooltip();
        _windowOpen = false;
        _controllerMode = false;
        _heldDirection = null;
        if (EventSystem.current != null &&
            EventSystem.current.currentSelectedGameObject != null &&
            EventSystem.current.currentSelectedGameObject.transform.IsChildOf(_view.Root.transform))
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    public void Tick(ItemSpawnerInputSnapshot input)
    {
        if (!_windowOpen)
            return;

        SyncDropdownState();
        if (input.MouseActivity)
            SetControllerMode(false);
        if (input.ControllerActivity)
            SetControllerMode(true);

        ValidateFocus();
        if (input.CancelPressed)
        {
            if (_view.TargetDropdown.Expanded)
                CollapseDropdown();
            else
                _close();
            return;
        }

        if (!_controllerMode)
            return;

        if (input.ConfirmPressed)
            ClearEventSystemSelection();

        ControllerNavigationDirection? direction = ReadDirection(input.Navigation);
        if (ShouldNavigate(direction))
            Move(direction!.Value);

        if (!_view.TargetDropdown.Expanded)
        {
            if (input.PreviousTargetPressed)
                SwitchRegion(-1);
            if (input.NextTargetPressed)
                SwitchRegion(1);
            if (input.FavoritePressed)
                ToggleFocusedFavorite();
        }

        if (input.ConfirmPressed)
            ActivateFocusedControl();
        if (input.ConfirmReleased || !input.ConfirmHeld)
            ReleasePressedControl();
    }

    private void SetControllerMode(bool controllerMode)
    {
        if (_controllerMode == controllerMode)
            return;

        _controllerMode = controllerMode;
        if (controllerMode)
        {
            ClearEventSystemSelection();
            RestoreFocus(preferFirstItem: false);
        }
        else
        {
            ReleasePressedControl();
            ClearFocusVisual(_focus);
            ClearDropdownOptionFocus();
            _view.HideTooltip();
        }
    }

    private void RestoreFocus(bool preferFirstItem)
    {
        if (_focus.Kind != ControllerFocusKind.None)
        {
            ApplyFocus(_focus, remember: false);
            return;
        }

        if (!preferFirstItem && _lastFocus.Kind != ControllerFocusKind.None)
        {
            ApplyFocus(_lastFocus, remember: false);
            return;
        }

        IReadOnlyList<ItemTile> tiles = _view.VisibleTiles;
        ApplyFocus(
            tiles.Count > 0
                ? ControllerFocusLocation.ForItem(0, tiles[0].Record.Item.name)
                : ControllerFocusLocation.PendingFirstItem(),
            remember: false);
    }

    private void ValidateFocus()
    {
        if (!_controllerMode)
            return;

        switch (_focus.Kind)
        {
            case ControllerFocusKind.Item:
                {
                    IReadOnlyList<ItemTile> tiles = _view.VisibleTiles;
                    if (tiles.Count == 0)
                    {
                        ClearFocusVisual(_focus);
                        _view.HideTooltip();
                        return;
                    }

                    int itemIndex = FindFocusedItemIndex(tiles);
                    if (itemIndex >= 0)
                    {
                        if (itemIndex != _focus.Index)
                            _focus = ControllerFocusLocation.ForItem(itemIndex, _focus.ItemName);
                        return;
                    }
                    int fallback = ControllerNavigation.ResolveFallbackIndex(_focus.Index, tiles.Count);
                    ApplyFocus(
                        ControllerFocusLocation.ForItem(fallback, tiles[fallback].Record.Item.name),
                        remember: true);
                    break;
                }
            case ControllerFocusKind.ClearTags when !_view.TagClear.Enabled:
                ApplyFocus(
                    ControllerFocusLocation.ForTag(_view.TagToggles.Count - 1),
                    remember: true);
                break;
            case ControllerFocusKind.DropdownOption when !_view.TargetDropdown.Expanded:
                ApplyFocus(ControllerFocusLocation.Target(), remember: true);
                break;
            case ControllerFocusKind.None:
                RestoreFocus(preferFirstItem: true);
                break;
        }
    }

    private bool ShouldNavigate(ControllerNavigationDirection? direction)
    {
        if (!direction.HasValue)
        {
            _heldDirection = null;
            return false;
        }

        float currentTime = Time.unscaledTime;
        if (_heldDirection != direction)
        {
            _heldDirection = direction;
            _nextNavigationAt = currentTime + InitialRepeatDelay;
            return true;
        }

        if (currentTime < _nextNavigationAt)
            return false;
        _nextNavigationAt = currentTime + RepeatInterval;
        return true;
    }

    private void Move(ControllerNavigationDirection direction)
    {
        if (_view.TargetDropdown.Expanded)
        {
            if (direction == ControllerNavigationDirection.Up)
                MoveDropdownOption(-1);
            else if (direction == ControllerNavigationDirection.Down)
                MoveDropdownOption(1);
            return;
        }

        switch (_focus.Kind)
        {
            case ControllerFocusKind.Target:
                if (direction == ControllerNavigationDirection.Down)
                    FocusNearestTag(GetHorizontalCenter(_view.TargetDropdown.Dropdown.transform));
                break;
            case ControllerFocusKind.Tag:
                MoveFromTag(direction);
                break;
            case ControllerFocusKind.ClearTags:
                MoveFromClearTags(direction);
                break;
            case ControllerFocusKind.Item:
                MoveFromItem(direction);
                break;
            default:
                RestoreFocus(preferFirstItem: true);
                break;
        }
    }

    private void MoveFromTag(ControllerNavigationDirection direction)
    {
        int tagIndex = Math.Clamp(_focus.Index, 0, _view.TagToggles.Count - 1);
        switch (direction)
        {
            case ControllerNavigationDirection.Left when tagIndex > 0:
                ApplyFocus(ControllerFocusLocation.ForTag(tagIndex - 1), remember: true);
                break;
            case ControllerNavigationDirection.Right when tagIndex + 1 < _view.TagToggles.Count:
                ApplyFocus(ControllerFocusLocation.ForTag(tagIndex + 1), remember: true);
                break;
            case ControllerNavigationDirection.Right when _view.TagClear.Enabled:
                ApplyFocus(ControllerFocusLocation.ClearTags(), remember: true);
                break;
            case ControllerNavigationDirection.Up:
                ApplyFocus(ControllerFocusLocation.Target(), remember: true);
                break;
            case ControllerNavigationDirection.Down:
                FocusNearestItem(GetHorizontalCenter(_view.TagToggles[tagIndex].Toggle.transform));
                break;
        }
    }

    private void MoveFromClearTags(ControllerNavigationDirection direction)
    {
        switch (direction)
        {
            case ControllerNavigationDirection.Left:
                ApplyFocus(
                    ControllerFocusLocation.ForTag(_view.TagToggles.Count - 1),
                    remember: true);
                break;
            case ControllerNavigationDirection.Up:
                ApplyFocus(ControllerFocusLocation.Target(), remember: true);
                break;
            case ControllerNavigationDirection.Down:
                FocusNearestItem(GetHorizontalCenter(_view.TagClear.Button.transform));
                break;
        }
    }

    private void MoveFromItem(ControllerNavigationDirection direction)
    {
        IReadOnlyList<ItemTile> tiles = _view.VisibleTiles;
        int currentIndex = FindFocusedItemIndex(tiles);
        if (currentIndex < 0)
            return;

        int columns = CalculateColumnCount();
        if (direction == ControllerNavigationDirection.Up && currentIndex < columns)
        {
            FocusNearestTag(GetHorizontalCenter(tiles[currentIndex].RectTransform));
            return;
        }

        int next = ControllerNavigation.MoveInGrid(currentIndex, tiles.Count, columns, direction);
        if (next != currentIndex)
        {
            ApplyFocus(
                ControllerFocusLocation.ForItem(next, tiles[next].Record.Item.name),
                remember: true);
        }
    }

    private void FocusNearestTag(float horizontalPosition)
    {
        var positions = _view.TagToggles
            .Select(tag => GetHorizontalCenter(tag.Toggle.transform))
            .ToList();
        if (_view.TagClear.Enabled)
            positions.Add(GetHorizontalCenter(_view.TagClear.Button.transform));
        int nearest = ControllerNavigation.FindNearest(horizontalPosition, positions);
        if (nearest < 0)
            return;
        ApplyFocus(
            nearest < _view.TagToggles.Count
                ? ControllerFocusLocation.ForTag(nearest)
                : ControllerFocusLocation.ClearTags(),
            remember: true);
    }

    private void FocusNearestItem(float horizontalPosition)
    {
        IReadOnlyList<ItemTile> tiles = _view.VisibleTiles;
        if (tiles.Count == 0)
            return;
        int columns = Math.Min(CalculateColumnCount(), tiles.Count);
        float[] positions = tiles.Take(columns)
            .Select(tile => GetHorizontalCenter(tile.RectTransform))
            .ToArray();
        int nearest = ControllerNavigation.FindNearest(horizontalPosition, positions);
        ApplyFocus(
            ControllerFocusLocation.ForItem(nearest, tiles[nearest].Record.Item.name),
            remember: true);
    }

    private void ActivateFocusedControl()
    {
        switch (_focus.Kind)
        {
            case ControllerFocusKind.Target:
                ExpandDropdown();
                break;
            case ControllerFocusKind.Tag when _focus.Index >= 0 && _focus.Index < _view.TagToggles.Count:
                {
                    TagToggle tag = _view.TagToggles[_focus.Index];
                    tag.SetControllerPressed(true);
                    _pressedTag = tag;
                    tag.Toggle.isOn = !tag.Toggle.isOn;
                    break;
                }
            case ControllerFocusKind.ClearTags when _view.TagClear.Enabled:
                _view.TagClear.SetControllerPressed(true);
                _tagClearPressed = true;
                _view.TagClear.Button.onClick.Invoke();
                ValidateFocus();
                break;
            case ControllerFocusKind.Item:
                {
                    ItemTile? tile = ResolveFocusedItem();
                    if (tile?.CanSpawn == true)
                    {
                        tile.BeginControllerSubmit();
                        _pressedTile = tile;
                    }
                    break;
                }
            case ControllerFocusKind.DropdownOption:
                SelectDropdownOption();
                break;
        }
    }

    private void ReleasePressedControl()
    {
        if (_pressedTag != null)
            _pressedTag.SetControllerPressed(false);
        if (_tagClearPressed)
            _view.TagClear.SetControllerPressed(false);
        if (_pressedTile != null && _pressedTile.GameObject != null)
            _pressedTile.EndControllerSubmit();
        _pressedTag = null;
        _pressedTile = null;
        _tagClearPressed = false;
    }

    private void ToggleFocusedFavorite()
    {
        ItemTile? tile = ResolveFocusedItem();
        if (tile?.TryToggleFavorite() != true)
            return;
        ValidateFocus();
    }

    private void SwitchRegion(int direction)
    {
        bool[] availableRegions =
        {
            _view.TargetDropdown.Enabled,
            _view.TagToggles.Count > 0,
            _view.VisibleTiles.Count > 0
        };
        int currentRegion = _focus.Kind switch
        {
            ControllerFocusKind.Target or ControllerFocusKind.DropdownOption => 0,
            ControllerFocusKind.Tag or ControllerFocusKind.ClearTags => 1,
            ControllerFocusKind.Item => 2,
            _ => 2
        };
        int nextRegion = ControllerNavigation.MoveRegion(
            currentRegion,
            direction,
            availableRegions);
        if (nextRegion < 0 || nextRegion == currentRegion)
            return;

        switch (nextRegion)
        {
            case 0:
                ApplyFocus(ControllerFocusLocation.Target(), remember: true);
                break;
            case 1:
                RestoreTagRegionFocus();
                break;
            case 2:
                RestoreItemRegionFocus();
                break;
        }
    }

    private void RestoreTagRegionFocus()
    {
        if (_lastTagFocus.Kind == ControllerFocusKind.ClearTags && _view.TagClear.Enabled)
        {
            ApplyFocus(_lastTagFocus, remember: true);
            return;
        }

        int index = _lastTagFocus.Kind == ControllerFocusKind.Tag
            ? Math.Clamp(_lastTagFocus.Index, 0, _view.TagToggles.Count - 1)
            : 0;
        ApplyFocus(ControllerFocusLocation.ForTag(index), remember: true);
    }

    private void RestoreItemRegionFocus()
    {
        IReadOnlyList<ItemTile> tiles = _view.VisibleTiles;
        if (tiles.Count == 0)
            return;

        int index = FindItemIndex(tiles, _lastItemFocus);
        if (index < 0)
            index = ControllerNavigation.ResolveFallbackIndex(_lastItemFocus.Index, tiles.Count);
        ApplyFocus(
            ControllerFocusLocation.ForItem(index, tiles[index].Record.Item.name),
            remember: true);
    }

    private void ExpandDropdown()
    {
        TMP_Dropdown dropdown = _view.TargetDropdown.Dropdown;
        if (!dropdown.IsActive() || !dropdown.IsInteractable())
            return;
        dropdown.Show();
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
        Canvas.ForceUpdateCanvases();
        _view.TargetDropdown.SetExpanded(dropdown.IsExpanded);
        if (!dropdown.IsExpanded)
            return;
        int index = Math.Clamp(dropdown.value, 0, Math.Max(0, dropdown.options.Count - 1));
        ApplyFocus(ControllerFocusLocation.DropdownOption(index), remember: false);
    }

    private void CollapseDropdown()
    {
        _view.TargetDropdown.Dropdown.Hide();
        _view.TargetDropdown.SetExpanded(false);
        ClearDropdownOptionFocus();
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
        ApplyFocus(ControllerFocusLocation.Target(), remember: true);
    }

    private void MoveDropdownOption(int delta)
    {
        IReadOnlyList<DropdownOptionVisual> options = _view.TargetDropdown.GetVisibleOptions();
        if (options.Count == 0)
            return;
        int next = Math.Clamp(_focus.Index + delta, 0, options.Count - 1);
        if (next != _focus.Index)
            ApplyFocus(ControllerFocusLocation.DropdownOption(next), remember: false);
    }

    private void SelectDropdownOption()
    {
        TMP_Dropdown dropdown = _view.TargetDropdown.Dropdown;
        if (_focus.Index >= 0 && _focus.Index < dropdown.options.Count)
            dropdown.value = _focus.Index;
        CollapseDropdown();
    }

    private void SyncDropdownState()
    {
        bool expanded = _view.TargetDropdown.Dropdown.IsExpanded;
        if (expanded == _view.TargetDropdown.Expanded)
            return;
        _view.TargetDropdown.SetExpanded(expanded);
        if (!expanded && _focus.Kind == ControllerFocusKind.DropdownOption)
            ApplyFocus(ControllerFocusLocation.Target(), remember: true);
    }

    private void ApplyFocus(ControllerFocusLocation next, bool remember)
    {
        ReleasePressedControl();
        ClearFocusVisual(_focus);
        ClearDropdownOptionFocus();
        _view.HideTooltip();
        _focus = next;
        if (next.Kind is ControllerFocusKind.Tag or ControllerFocusKind.ClearTags)
            _lastTagFocus = next;
        else if (next.Kind == ControllerFocusKind.Item)
            _lastItemFocus = next;
        if (remember && next.Kind != ControllerFocusKind.DropdownOption)
            _lastFocus = next;
        if (!_controllerMode)
            return;
        SetFocusVisual(next);
    }

    private void SetFocusVisual(ControllerFocusLocation location)
    {
        switch (location.Kind)
        {
            case ControllerFocusKind.Target:
                _view.TargetDropdown.SetFocused(true);
                break;
            case ControllerFocusKind.Tag when location.Index >= 0 && location.Index < _view.TagToggles.Count:
                _view.TagToggles[location.Index].SetFocused(true);
                break;
            case ControllerFocusKind.ClearTags when _view.TagClear.Enabled:
                _view.TagClear.SetFocused(true);
                break;
            case ControllerFocusKind.Item:
                {
                    ItemTile? tile = ResolveFocusedItem();
                    if (tile == null)
                        break;
                    tile.SetFocused(true);
                    EnsureItemVisible(tile);
                    if (tile.HasTruncatedName())
                        _view.ShowControllerTooltip(tile);
                    break;
                }
            case ControllerFocusKind.DropdownOption:
                {
                    IReadOnlyList<DropdownOptionVisual> options = _view.TargetDropdown.GetVisibleOptions();
                    if (location.Index >= 0 && location.Index < options.Count)
                    {
                        options[location.Index].SetFocused(true);
                        ScrollRect? optionScroll = options[location.Index].GetComponentInParent<ScrollRect>();
                        if (optionScroll != null)
                            EnsureVisible(optionScroll, (RectTransform)options[location.Index].transform);
                    }
                    break;
                }
        }
    }

    private void ClearFocusVisual(ControllerFocusLocation location)
    {
        switch (location.Kind)
        {
            case ControllerFocusKind.Target:
                _view.TargetDropdown.SetFocused(false);
                break;
            case ControllerFocusKind.Tag when location.Index >= 0 && location.Index < _view.TagToggles.Count:
                _view.TagToggles[location.Index].SetFocused(false);
                break;
            case ControllerFocusKind.ClearTags:
                _view.TagClear.SetFocused(false);
                break;
            case ControllerFocusKind.Item:
                ResolveFocusedItem()?.SetFocused(false);
                break;
        }
    }

    private void ClearDropdownOptionFocus()
    {
        foreach (DropdownOptionVisual option in _view.TargetDropdown.GetVisibleOptions())
            option.SetFocused(false);
    }

    private ItemTile? ResolveFocusedItem()
    {
        IReadOnlyList<ItemTile> tiles = _view.VisibleTiles;
        int index = FindFocusedItemIndex(tiles);
        return index >= 0 ? tiles[index] : null;
    }

    private int FindFocusedItemIndex(IReadOnlyList<ItemTile> tiles)
        => FindItemIndex(tiles, _focus);

    private static int FindItemIndex(
        IReadOnlyList<ItemTile> tiles,
        ControllerFocusLocation location)
    {
        if (!string.IsNullOrEmpty(location.ItemName))
        {
            if (location.Index >= 0 && location.Index < tiles.Count &&
                string.Equals(
                    tiles[location.Index].Record.Item.name,
                    location.ItemName,
                    StringComparison.Ordinal))
            {
                return location.Index;
            }

            for (int index = 0; index < tiles.Count; index++)
            {
                if (string.Equals(
                        tiles[index].Record.Item.name,
                        location.ItemName,
                        StringComparison.Ordinal))
                {
                    return index;
                }
            }
        }
        return -1;
    }

    private int CalculateColumnCount()
    {
        Canvas.ForceUpdateCanvases();
        GridLayoutGroup grid = _view.ItemContent.GetComponent<GridLayoutGroup>();
        float availableWidth = _view.ItemContent.rect.width - grid.padding.horizontal;
        return Math.Max(
            1,
            Mathf.FloorToInt((availableWidth + grid.spacing.x) / (grid.cellSize.x + grid.spacing.x)));
    }

    private void EnsureItemVisible(ItemTile tile)
    {
        EnsureVisible(_view.ItemScroll, tile.RectTransform);
    }

    private static void EnsureVisible(ScrollRect scroll, RectTransform target)
    {
        Canvas.ForceUpdateCanvases();
        RectTransform viewport = scroll.viewport;
        RectTransform content = scroll.content;
        Bounds bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(viewport, target);
        float offset = content.anchoredPosition.y;
        if (bounds.min.y < viewport.rect.yMin)
            offset += viewport.rect.yMin - bounds.min.y;
        else if (bounds.max.y > viewport.rect.yMax)
            offset -= bounds.max.y - viewport.rect.yMax;

        float maximum = Math.Max(0, content.rect.height - viewport.rect.height);
        content.anchoredPosition = new Vector2(content.anchoredPosition.x, Mathf.Clamp(offset, 0, maximum));
    }

    private static float GetHorizontalCenter(Transform transform)
    {
        if (transform is not RectTransform rect)
            return transform.position.x;
        var corners = new Vector3[4];
        rect.GetWorldCorners(corners);
        return (corners[0].x + corners[2].x) * 0.5f;
    }

    private static ControllerNavigationDirection? ReadDirection(Vector2 navigation)
    {
        if (navigation.sqrMagnitude < NavigationDeadZone * NavigationDeadZone)
            return null;
        if (Mathf.Abs(navigation.x) > Mathf.Abs(navigation.y))
            return navigation.x < 0
                ? ControllerNavigationDirection.Left
                : ControllerNavigationDirection.Right;
        return navigation.y < 0
            ? ControllerNavigationDirection.Down
            : ControllerNavigationDirection.Up;
    }

    private void ClearEventSystemSelection()
    {
        if (EventSystem.current == null || EventSystem.current.currentSelectedGameObject == null)
            return;
        if (EventSystem.current.currentSelectedGameObject.transform.IsChildOf(_view.Root.transform))
            EventSystem.current.SetSelectedGameObject(null);
    }
}

internal enum ControllerFocusKind
{
    None,
    Target,
    Tag,
    ClearTags,
    Item,
    DropdownOption
}

internal readonly struct ControllerFocusLocation
{
    private ControllerFocusLocation(ControllerFocusKind kind, int index, string? itemName)
    {
        Kind = kind;
        Index = index;
        ItemName = itemName;
    }

    public ControllerFocusKind Kind { get; }
    public int Index { get; }
    public string? ItemName { get; }

    public static ControllerFocusLocation Target() => new(ControllerFocusKind.Target, 0, null);
    public static ControllerFocusLocation ForTag(int index) => new(ControllerFocusKind.Tag, index, null);
    public static ControllerFocusLocation ClearTags() => new(ControllerFocusKind.ClearTags, 0, null);
    public static ControllerFocusLocation ForItem(int index, string? itemName) =>
        new(ControllerFocusKind.Item, index, itemName);
    public static ControllerFocusLocation PendingFirstItem() => ForItem(0, null);
    public static ControllerFocusLocation DropdownOption(int index) =>
        new(ControllerFocusKind.DropdownOption, index, null);
}
