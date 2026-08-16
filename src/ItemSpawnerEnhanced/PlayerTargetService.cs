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

    public IReadOnlyList<PlayerTarget> GetTargets()
    {
        Character? spectated = MainCameraMovement.specCharacter;
        return PlayerHandler.GetAllPlayerCharacters()
            .Where(IsPlayerCharacter)
            .Select(character => new PlayerTarget(
                GetActorId(character),
                character.characterName,
                character == Character.localCharacter,
                character == spectated,
                IsValid(character)))
            .Where(target => target.ActorId > 0)
            .OrderByDescending(target => target.IsLocal)
            .ThenBy(target => target.Name, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    public Character? Resolve(int? manualActorId)
    {
        if (manualActorId.HasValue)
        {
            IReadOnlyList<PlayerTarget> targets = GetTargets();
            int? actorId = TargetResolver.Resolve(
                targets.Select(target => target.ToCandidate()).ToArray(),
                manualActorId);
            if (actorId.HasValue)
            {
                Character? selected = PlayerHandler.GetAllPlayerCharacters()
                    .FirstOrDefault(character =>
                        IsPlayerCharacter(character) && GetActorId(character) == actorId.Value && IsValid(character));
                if (selected != null)
                    return selected;
            }
        }

        Character? spectated = MainCameraMovement.specCharacter;
        if (CanReceiveItem(spectated))
            return spectated;

        Character? local = Character.localCharacter;
        return CanReceiveItem(local) ? local : null;
    }

    public bool TrySpawn(Item item, int? manualActorId, out string errorKey)
    {
        if (!PhotonNetwork.IsConnected)
        {
            errorKey = "notConnected";
            return false;
        }

        Character? target = Resolve(manualActorId);
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

    private static bool CanReceiveItem(Character? character) =>
        IsPlayerCharacter(character) && character!.refs != null && character.refs.items != null;

    private static bool IsValid(Character character) =>
        character.IsInitialized && character.refs != null && character.refs.items != null && GetActorId(character) > 0;

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

internal readonly struct PlayerTarget
{
    public PlayerTarget(int actorId, string name, bool isLocal, bool isSpectated, bool isValid)
    {
        ActorId = actorId;
        Name = name;
        IsLocal = isLocal;
        IsSpectated = isSpectated;
        IsValid = isValid;
    }

    public int ActorId { get; }
    public string Name { get; }
    public bool IsLocal { get; }
    public bool IsSpectated { get; }
    public bool IsValid { get; }

    public TargetCandidate ToCandidate() => new(ActorId, IsLocal, IsSpectated, IsValid);
}
