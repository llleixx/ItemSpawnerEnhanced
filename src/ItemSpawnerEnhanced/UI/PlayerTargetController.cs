using System;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using UnityEngine;

namespace ItemSpawnerEnhanced.UI;

internal sealed class PlayerTargetController
{
    private readonly PlayerTargetService _service;
    private readonly ItemSpawnerView _view;
    private readonly Func<string, string> _localize;
    private readonly List<PlayerTarget> _dropdownTargets = new();
    private PlayerTarget[] _lastTargets = Array.Empty<PlayerTarget>();
    private int? _manualActorId;
    private float _lastRefresh;
    private bool _labelsDirty = true;
    private bool _updatingDropdown;

    public PlayerTargetController(
        PlayerTargetService service,
        ItemSpawnerView view,
        Func<string, string> localize)
    {
        _service = service;
        _view = view;
        _localize = localize;
    }

    public bool CanSpawn { get; private set; }
    public string ErrorKey { get; private set; } = "noTarget";

    public bool IsRefreshDue(float currentTime) => currentTime - _lastRefresh >= 1f;

    public bool Refresh(bool force)
    {
        _lastRefresh = Time.unscaledTime;
        PlayerTargetSnapshot snapshot = _service.Capture();
        IReadOnlyList<PlayerTarget> current = snapshot.Targets;

        if (force || _labelsDirty || !TargetsEqual(current, _lastTargets))
        {
            _lastTargets = current.ToArray();
            _labelsDirty = false;
            UpdateDropdown(current);
            force = true;
        }

        return UpdateSpawnState(snapshot, force);
    }

    public bool Select(int index)
    {
        if (_updatingDropdown)
            return false;

        _manualActorId = index > 0 && index <= _dropdownTargets.Count
            ? _dropdownTargets[index - 1].ActorId
            : null;
        return UpdateSpawnState(_service.Capture(), force: false);
    }

    public bool TrySpawn(Item item, out string errorKey) =>
        _service.TrySpawn(item, _manualActorId, out errorKey);

    public void InvalidateLabels() => _labelsDirty = true;

    private void UpdateDropdown(IReadOnlyList<PlayerTarget> current)
    {
        _dropdownTargets.Clear();
        _dropdownTargets.AddRange(current.Where(target => target.IsSelectable));
        if (_manualActorId.HasValue && _dropdownTargets.All(target => target.ActorId != _manualActorId.Value))
            _manualActorId = null;

        Dictionary<string, int> duplicateNames = _dropdownTargets
            .GroupBy(target => target.Name, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.Count(), StringComparer.OrdinalIgnoreCase);

        var labels = new List<string> { _localize("smartTarget") };
        labels.AddRange(_dropdownTargets.Select(target =>
        {
            string name = duplicateNames[target.Name] > 1 ? $"{target.Name} #{target.ActorId}" : target.Name;
            if (target.IsLocal)
                name += $" {_localize("youSuffix")}";
            return name;
        }));

        _updatingDropdown = true;
        try
        {
            int selectedIndex = _manualActorId.HasValue
                ? _dropdownTargets.FindIndex(target => target.ActorId == _manualActorId.Value) + 1
                : 0;
            _view.SetTargets(labels, selectedIndex);
        }
        finally
        {
            _updatingDropdown = false;
        }
    }

    private bool UpdateSpawnState(PlayerTargetSnapshot snapshot, bool force)
    {
        bool connected = PhotonNetwork.IsConnected;
        bool canSpawn = connected && snapshot.Resolve(_manualActorId) != null;
        bool changed = canSpawn != CanSpawn;
        CanSpawn = canSpawn;
        ErrorKey = connected ? "noTarget" : "notConnected";
        _view.SetSpawnEnabled(canSpawn, force);
        return changed || force;
    }

    private static bool TargetsEqual(IReadOnlyList<PlayerTarget> left, IReadOnlyList<PlayerTarget> right)
    {
        if (left.Count != right.Count)
            return false;

        for (int index = 0; index < left.Count; index++)
        {
            PlayerTarget first = left[index];
            PlayerTarget second = right[index];
            if (first.ActorId != second.ActorId ||
                !string.Equals(first.Name, second.Name, StringComparison.Ordinal) ||
                first.IsLocal != second.IsLocal ||
                first.IsSpectated != second.IsSpectated ||
                first.IsSelectable != second.IsSelectable)
            {
                return false;
            }
        }

        return true;
    }
}
