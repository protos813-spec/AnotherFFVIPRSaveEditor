using System.Text.Json.Nodes;

namespace Ffvi.SaveTool;

public class CharacterAbilities
{
    public const int MagicCategoryKey = 7;
    public const int ContentIdOffset = 330;

    private readonly JsonObject _abilityListNode;
    private readonly JsonObject _abilityDictNode;
    private readonly JsonObject _characterNode;

    internal CharacterAbilities(JsonObject abilityListNode, JsonObject abilityDictNode, JsonObject characterNode)
    {
        _abilityListNode = abilityListNode;
        _abilityDictNode = abilityDictNode;
        _characterNode = characterNode;
    }

    public List<AbilityEntry> AllAbilities() => ParseTarget(_abilityListNode);

    // Rages, Lores, Bushidos, Blitzes, Dances — these "skill" abilities are only stored in
    // abilityList (not also in abilityDictionary like spells), and use different offsets.
    // For a rage, contentId = abilityId + 168 (per the Go reference).
    public List<AbilityEntry> LearnedSkillsInRange(int fromId, int toId) => AllAbilities()
        .Where(a => a.AbilityId >= fromId && a.AbilityId <= toId && a.SkillLevel >= 100)
        .ToList();

    public void LearnSkill(int abilityId, int contentIdOffset)
    {
        var target = _abilityListNode["target"]!.AsArray();
        for (var i = 0; i < target.Count; i++)
        {
            var ab = JsonNode.Parse(target[i]!.GetValue<string>())!.AsObject();
            if (ab["abilityId"]?.GetValue<int>() == abilityId)
            {
                ab["skillLevel"] = 100;
                target[i] = JsonValue.Create(ab.ToJsonString(SaveFile.JsonOpts));
                return;
            }
        }
        target.Add(JsonValue.Create(new JsonObject
        {
            ["abilityId"] = abilityId,
            ["contentId"] = abilityId + contentIdOffset,
            ["skillLevel"] = 100,
        }.ToJsonString(SaveFile.JsonOpts)));
    }

    public void ForgetSkill(int abilityId) => ForgetEverywhere(abilityId);

    // Clears every record of an ability the character owns.
    //
    // Setting skillLevel to 0 in abilityList is NOT enough on its own: the game also keeps
    // learned abilities in abilityDictionary (and their contentIds in the ownership order
    // lists), and it appears to merge those in rather than rebuild them, so a skill cleared
    // only in abilityList stays visible in the in-game menu. Rages were reported this way:
    // ticking a box added the rage, unticking it left the rage in Gau's list.
    //
    // The dictionary is scrubbed across ALL categories rather than one known key, because
    // the category ids are not documented and differ per ability type.
    public void ForgetEverywhere(int abilityId)
    {
        var contentId = -1;

        var target = _abilityListNode["target"]!.AsArray();
        for (var i = 0; i < target.Count; i++)
        {
            var ab = JsonNode.Parse(target[i]!.GetValue<string>())!.AsObject();
            if (ab["abilityId"]?.GetValue<int>() != abilityId) continue;
            contentId = ab["contentId"]?.GetValue<int>() ?? -1;
            ab["skillLevel"] = 0;
            target[i] = JsonValue.Create(ab.ToJsonString(SaveFile.JsonOpts));
            break;
        }

        RemoveFromAllDictionaryCategories(abilityId);
        if (contentId < 0) contentId = abilityId + ContentIdOffset;
        RemoveFromOwnershipOrder(contentId);
    }

    private void RemoveFromAllDictionaryCategories(int abilityId)
    {
        var keys = _abilityDictNode["keys"]!.AsArray();
        var values = _abilityDictNode["values"]!.AsArray();
        for (var i = 0; i < keys.Count; i++)
        {
            var catObj = JsonNode.Parse(values[i]!.GetValue<string>())!.AsObject();
            var catTarget = catObj["target"]?.AsArray();
            if (catTarget is null) continue;

            var removed = false;
            for (var j = catTarget.Count - 1; j >= 0; j--)
            {
                var ab = JsonNode.Parse(catTarget[j]!.GetValue<string>())!.AsObject();
                if (ab["abilityId"]?.GetValue<int>() != abilityId) continue;
                catTarget.RemoveAt(j);
                removed = true;
            }
            if (removed) values[i] = JsonValue.Create(catObj.ToJsonString(SaveFile.JsonOpts));
        }
    }

    // additionOrderOwnedAbilityIds and sortOrderOwnedAbilityIds hold contentIds (ability id
    // plus offset) of the abilities the character owns, in acquisition and sort order.
    private void RemoveFromOwnershipOrder(int contentId)
    {
        foreach (var key in new[] { "additionOrderOwnedAbilityIds", "sortOrderOwnedAbilityIds" })
        {
            if (!_characterNode.ContainsKey(key)) continue;
            var node = NestedJson.Unwrap(_characterNode, key).AsObject();
            var list = node["target"]?.AsArray();
            if (list is null) continue;

            var removed = false;
            for (var i = list.Count - 1; i >= 0; i--)
            {
                if (list[i]?.GetValue<int>() != contentId) continue;
                list.RemoveAt(i);
                removed = true;
            }
            if (removed) NestedJson.Rewrap(_characterNode, key, node);
        }
    }

    public List<AbilityEntry> LearnedMagic()
    {
        var (_, target) = GetCategory(MagicCategoryKey);
        return target is null ? new List<AbilityEntry>() : ParseTargetArray(target);
    }

    public bool KnowsAbility(int abilityId) =>
        AllAbilities().Any(a => a.AbilityId == abilityId && a.SkillLevel >= 100);

    public void LearnAbility(int abilityId, int categoryKey, int contentId = -1)
    {
        if (contentId < 0) contentId = abilityId + ContentIdOffset;

        var target = _abilityListNode["target"]!.AsArray();
        var existing = false;
        for (var i = 0; i < target.Count; i++)
        {
            var ab = JsonNode.Parse(target[i]!.GetValue<string>())!.AsObject();
            if (ab["abilityId"]?.GetValue<int>() == abilityId)
            {
                ab["skillLevel"] = 100;
                target[i] = JsonValue.Create(ab.ToJsonString(SaveFile.JsonOpts));
                existing = true;
                break;
            }
        }
        if (!existing)
        {
            target.Add(JsonValue.Create(new JsonObject
            {
                ["abilityId"] = abilityId,
                ["contentId"] = contentId,
                ["skillLevel"] = 100,
            }.ToJsonString(SaveFile.JsonOpts)));
        }

        // .NET 10's JsonNode enforces single-parent ownership, so we keep a reference to the
        // parsed category object (catObj) and mutate its target in place, then re-serialize the
        // whole catObj. Wrapping catTarget in a new JsonObject throws ("node already has a parent").
        var (catIdx, catObj) = GetCategoryObj(categoryKey);
        if (catIdx == -1)
        {
            var keys = _abilityDictNode["keys"]!.AsArray();
            var values = _abilityDictNode["values"]!.AsArray();
            keys.Add(categoryKey);
            catObj = new JsonObject { ["target"] = new JsonArray() };
            values.Add(JsonValue.Create(catObj.ToJsonString(SaveFile.JsonOpts)));
            catIdx = keys.Count - 1;
        }
        var catTarget = catObj!["target"]!.AsArray();
        var inDict = false;
        for (var i = 0; i < catTarget.Count; i++)
        {
            var ab = JsonNode.Parse(catTarget[i]!.GetValue<string>())!.AsObject();
            if (ab["abilityId"]?.GetValue<int>() == abilityId) { inDict = true; break; }
        }
        if (!inDict)
        {
            catTarget.Add(JsonValue.Create(new JsonObject
            {
                ["abilityId"] = abilityId,
                ["contentId"] = contentId,
                ["skillLevel"] = 100,
            }.ToJsonString(SaveFile.JsonOpts)));
            var values = _abilityDictNode["values"]!.AsArray();
            values[catIdx] = JsonValue.Create(catObj.ToJsonString(SaveFile.JsonOpts));
        }
    }

    // categoryKey is no longer needed: ForgetEverywhere scrubs all categories, which also
    // covers abilities recorded under an unexpected one. Kept for call-site compatibility.
    public void ForgetAbility(int abilityId, int categoryKey) => ForgetEverywhere(abilityId);

    public void LearnSpell(int spellId) => LearnAbility(spellId, MagicCategoryKey);
    public void ForgetSpell(int spellId) => ForgetAbility(spellId, MagicCategoryKey);

    private (int idx, JsonObject? catObj) GetCategoryObj(int key)
    {
        var keys = _abilityDictNode["keys"]!.AsArray();
        var values = _abilityDictNode["values"]!.AsArray();
        for (var i = 0; i < keys.Count; i++)
        {
            if (keys[i]?.GetValue<int>() == key)
            {
                var catObj = JsonNode.Parse(values[i]!.GetValue<string>())!.AsObject();
                return (i, catObj);
            }
        }
        return (-1, null);
    }

    private (int idx, JsonArray? target) GetCategory(int key)
    {
        var keys = _abilityDictNode["keys"]!.AsArray();
        var values = _abilityDictNode["values"]!.AsArray();
        for (var i = 0; i < keys.Count; i++)
        {
            if (keys[i]?.GetValue<int>() == key)
            {
                var catObj = JsonNode.Parse(values[i]!.GetValue<string>())!.AsObject();
                return (i, catObj["target"]!.AsArray());
            }
        }
        return (-1, null);
    }

    private static List<AbilityEntry> ParseTarget(JsonObject root) =>
        ParseTargetArray(root["target"]!.AsArray());

    private static List<AbilityEntry> ParseTargetArray(JsonArray target)
    {
        var list = new List<AbilityEntry>(target.Count);
        foreach (var item in target)
        {
            var ab = JsonNode.Parse(item!.GetValue<string>())!.AsObject();
            list.Add(new AbilityEntry(
                ab["abilityId"]?.GetValue<int>() ?? 0,
                ab["contentId"]?.GetValue<int>() ?? 0,
                ab["skillLevel"]?.GetValue<int>() ?? 0));
        }
        return list;
    }

    internal void Commit()
    {
        NestedJson.Rewrap(_characterNode, "abilityList", _abilityListNode);
        NestedJson.Rewrap(_characterNode, "abilityDictionary", _abilityDictNode);
    }
}

public record AbilityEntry(int AbilityId, int ContentId, int SkillLevel);
