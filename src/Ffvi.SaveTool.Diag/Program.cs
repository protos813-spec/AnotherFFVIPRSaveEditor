using Ffvi.SaveTool;
using Ffvi.SaveTool.Data;

// Validates CharacterRoster (id -> canonical English name) against every character in
// every local save, and reports which skill owners resolve by id.

var savesDir = SaveFile.DefaultSaveDirectory();
var seen = new SortedDictionary<int, (string SaveName, string Roster, int JobId, bool BaseStats)>();

foreach (var f in Directory.GetFiles(savesDir).Where(f => new FileInfo(f).Length > 30000 && !f.EndsWith(".backup")))
{
    SaveFile save;
    try { save = SaveFile.Load(f); } catch { continue; }
    foreach (var c in save.UserData.Characters)
    {
        var entry = CharacterRoster.ForId(c.Id);
        seen[c.Id] = (c.Name, entry?.EnglishName ?? "(not in roster)", c.JobId,
                      CharacterBaseStats.ForId(c.Id) is not null);
    }
}

Console.WriteLine($"{"id",3}  {"save name",-12} {"roster name",-14} {"jobId",5}  base stats  match");
Console.WriteLine(new string('-', 66));
var mismatches = 0;
foreach (var (id, v) in seen)
{
    var rosterJob = CharacterRoster.ForId(id)?.JobId;
    var nameOk = v.Roster == v.SaveName;
    var jobOk = rosterJob is null || rosterJob == v.JobId;
    if (!nameOk || !jobOk) mismatches++;
    var flag = (nameOk ? "" : " NAME-DIFF") + (jobOk ? "" : $" JOB-DIFF(roster={rosterJob})");
    if (flag.Length == 0) flag = " ok";
    Console.WriteLine($"{id,3}  {v.SaveName,-12} {v.Roster,-14} {v.JobId,5}  {(v.BaseStats ? "yes" : "no ")}        {flag}");
}

Console.WriteLine($"\nmismatches: {mismatches}");
Console.WriteLine("\nSkill owner resolution in these saves:");
foreach (var (label, id) in new[]
{
    ("Rages/Gau", CharacterRoster.GauId), ("Bushido/Cyan", CharacterRoster.CyanId),
    ("Lore/Strago", CharacterRoster.StragoId), ("Blitz/Sabin", CharacterRoster.SabinId),
})
{
    var present = seen.ContainsKey(id);
    Console.WriteLine($"  {label,-14} id={id,2}  {(present ? $"present as '{seen[id].SaveName}'" : "not in these saves")}");
}
return 0;
