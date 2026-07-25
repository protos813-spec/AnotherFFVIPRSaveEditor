using System.Text.Json.Nodes;
using Ffvi.SaveTool;

// Verifies ForgetEverywhere clears an ability from all three places the game records
// ownership: abilityList skillLevel, abilityDictionary categories, and the ownership
// order lists. Uses Cure (ability 31 / content 361), which Terra legitimately owns.
// Operates on a temp copy; the real save is untouched.

var savesDir = SaveFile.DefaultSaveDirectory();
string? path = null;
foreach (var f in Directory.GetFiles(savesDir).Where(f => new FileInfo(f).Length > 30000 && !f.EndsWith(".backup")))
    if (SaveFile.Load(f).SlotId == 1) { path = f; break; }
if (path is null) { Console.WriteLine("slot 1 not found"); return 1; }

var tmp = Path.Combine(Path.GetTempPath(), "ffvi_forget_test");
File.Copy(path, tmp, overwrite: true);

const int abilityId = 31, contentId = 361;   // Cure

void Report(string label, SaveFile s)
{
    var c = s.UserData.Characters.First(x => x.Id == 1);
    var inList = c.Abilities.AllAbilities().FirstOrDefault(a => a.AbilityId == abilityId);
    var dictHits = 0;
    var dict = JsonNode.Parse(Raw(c, "abilityDictionary"))!.AsObject();
    var values = dict["values"]!.AsArray();
    foreach (var v in values)
    {
        var cat = JsonNode.Parse(v!.GetValue<string>())!.AsObject();
        foreach (var e in cat["target"]!.AsArray())
            if (JsonNode.Parse(e!.GetValue<string>())!.AsObject()["abilityId"]?.GetValue<int>() == abilityId) dictHits++;
    }
    var orderHits = 0;
    foreach (var key in new[] { "additionOrderOwnedAbilityIds", "sortOrderOwnedAbilityIds" })
    {
        var arr = JsonNode.Parse(Raw(c, key))!.AsObject()["target"]!.AsArray();
        orderHits += arr.Count(n => n?.GetValue<int>() == contentId);
    }
    Console.WriteLine($"{label,-22} abilityList skillLevel={inList?.SkillLevel.ToString() ?? "absent"}   dictionary entries={dictHits}   ownership-order entries={orderHits}");
}

static string Raw(Character c, string key)
{
    var n = c.Node[key];
    return n is JsonValue jv && jv.TryGetValue<string>(out var s) ? s : n?.ToJsonString() ?? "{\"target\":[]}";
}

Report("before forget:", SaveFile.Load(tmp));

var save = SaveFile.Load(tmp);
save.UserData.Characters.First(c => c.Id == 1).Abilities.ForgetSpell(abilityId);
save.Save(tmp + ".out");

Report("after forget (saved):", SaveFile.Load(tmp + ".out"));

File.Delete(tmp);
File.Delete(tmp + ".out");
return 0;
