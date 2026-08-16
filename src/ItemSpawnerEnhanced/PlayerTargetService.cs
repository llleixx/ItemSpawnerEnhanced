using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Logging;
using HarmonyLib;
using ItemSpawnerEnhanced.Core;
using Photon.Pun;

namespace ItemSpawnerEnhanced;

internal sealed class PlayerTargetService
{
    private static readonly System.Reflection.MethodInfo? SpawnItemMethod =
        AccessTools.Method(typeof(CharacterItems), "SpawnItemInHand", new[] { typeof(string) });
    private readonly ManualLogSource _logger;

    public PlayerTargetService(ManualLogSource logger) => _logger = logger;

    public PlayerTargetSnapshot Capture()
    {
        Character[] characters = GetPlayerCharacters();
        Character? spectated = MainCameraMovement.specCharacter;
        var targets = new List<PlayerTarget>(characters.Length);
        var candidates = new TargetCandidate[characters.Length];

        for (int index = 0; index < characters.Length; index++)
        {
            Character character = characters[index];
            int actorId = GetActorId(character);
            bool canReceiveItem = CanReceiveItem(character);
            bool isLocal = character == Character.localCharacter;
            bool isSpectated = character == spectated;
            bool isSelectable = IsSelectable(actorId, canReceiveItem);
            candidates[index] = new TargetCandidate(
                actorId,
                isLocal,
                isSpectated,
                isSelectable,
                canReceiveItem);

            if (actorId > 0)
                targets.Add(new PlayerTarget(actorId, character.characterName, isLocal, isSpectated, isSelectable));
        }

        PlayerTarget[] orderedTargets = targets
            .OrderByDescending(target => target.IsLocal)
            .ThenBy(target => target.Name, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        return new PlayerTargetSnapshot(characters, candidates, orderedTargets);
    }

    public bool TrySpawn(Item item, int? manualActorId, out string errorKey)
    {
        if (!PhotonNetwork.IsConnected)
        {
            errorKey = "notConnected";
            return false;
        }

        Character? target = Capture().Resolve(manualActorId);
        if (target == null)
        {
            errorKey = "noTarget";
            return false;
        }

        try
        {
            if (SpawnItemMethod == null)
                throw new MissingMethodException(typeof(CharacterItems).FullName, "SpawnItemInHand");
            SpawnItemMethod.Invoke(target.refs.items, new object[] { item.name });
            _logger.LogInfo($"Spawned '{item.name}' for '{target.characterName}'.");
            errorKey = string.Empty;
            return true;
        }
        catch (Exception exception)
        {
            _logger.LogError($"Failed to spawn '{item.name}': {exception}");
            errorKey = "spawnFailed";
            return false;
        }
    }

    private static bool IsPlayerCharacter(Character? character) => character != null && !character.isBot;

    private static Character[] GetPlayerCharacters() =>
        PlayerHandler.GetAllPlayerCharacters().Where(IsPlayerCharacter).ToArray();

    private static bool CanReceiveItem(Character? character) =>
        IsPlayerCharacter(character) && character!.refs != null && character.refs.items != null;

    private static bool IsSelectable(int actorId, bool canReceiveItem) =>
        actorId > 0 && canReceiveItem;

    private static int GetActorId(Character character)
    {
        try
        {
            return character.photonView?.OwnerActorNr ?? -1;
        }
        catch
        {
            return -1;
        }
    }
}

internal sealed class PlayerTargetSnapshot
{
    private readonly Character[] _characters;
    private readonly TargetCandidate[] _candidates;

    public PlayerTargetSnapshot(
        Character[] characters,
        TargetCandidate[] candidates,
        IReadOnlyList<PlayerTarget> targets)
    {
        _characters = characters;
        _candidates = candidates;
        Targets = targets;
    }

    public IReadOnlyList<PlayerTarget> Targets { get; }

    public Character? Resolve(int? manualActorId)
    {
        // Airport lobby characters can receive items before initialization completes
        // and become manually selectable once they have a stable Photon actor ID.
        int? targetIndex = TargetResolver.ResolveIndex(_candidates, manualActorId);
        return targetIndex.HasValue ? _characters[targetIndex.Value] : null;
    }
}

internal readonly struct PlayerTarget
{
    public PlayerTarget(int actorId, string name, bool isLocal, bool isSpectated, bool isSelectable)
    {
        ActorId = actorId;
        Name = name;
        IsLocal = isLocal;
        IsSpectated = isSpectated;
        IsSelectable = isSelectable;
    }

    public int ActorId { get; }
    public string Name { get; }
    public bool IsLocal { get; }
    public bool IsSpectated { get; }
    public bool IsSelectable { get; }
}
