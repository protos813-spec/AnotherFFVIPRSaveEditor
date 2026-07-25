using Ffvi.SaveTool;
using Ffvi.SaveTool.Data;

// End-to-end check of the level/exp fix: apply SetLevel to a copy of File 1, reload the
// written file, and confirm both fields persisted consistently. Writes to a temp path so
// the real save is untouched.

var savesDir = SaveFile.DefaultSaveDirectory();
string? path = null;
foreach (var f in Directory.GetFiles(savesDir).Where(f => new FileInfo(f).Length > 30000 && !f.EndsWith(".backup")))
    if (SaveFile.Load(f).SlotId == 1) { path = f; break; }
if (path is null) { Console.WriteLine("slot 1 not found"); return 1; }

var tmp = Path.Combine(Path.GetTempPath(), "ffvi_level_test");
File.Copy(path, tmp, overwrite: true);

var before = SaveFile.Load(tmp);
var t0 = before.UserData.Characters.First(c => c.Id == 1);
Console.WriteLine($"before: level {t0.Stats.AdditionalLevel}, exp {t0.CurrentExp:N0}");

foreach (var target in new[] { 25, 50, 99, 1 })
{
    var s = SaveFile.Load(tmp);
    var terra = s.UserData.Characters.First(c => c.Id == 1);
    terra.SetLevel(target);
    s.Save(tmp + ".out");

    var reloaded = SaveFile.Load(tmp + ".out");
    var r = reloaded.UserData.Characters.First(c => c.Id == 1);
    var implied = LevelGrowth.LevelForExp(r.CurrentExp);
    var ok = r.Stats.AdditionalLevel == target && implied == target;
    Console.WriteLine($"SetLevel({target,2}) -> stored L{r.Stats.AdditionalLevel,-2} exp {r.CurrentExp,9:N0}  exp implies L{implied,-2}  {(ok ? "consistent" : "MISMATCH")}");
}

File.Delete(tmp);
File.Delete(tmp + ".out");
return 0;
