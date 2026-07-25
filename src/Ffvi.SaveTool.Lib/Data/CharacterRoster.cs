namespace Ffvi.SaveTool.Data;

public record RosterEntry(int Id, int JobId, string EnglishName, bool IsNpc);

// Canonical character roster keyed by the save's `id` field.
//
// Identify characters by Id, never by the save's `name` string: `name` holds the
// LOCALISED name (a save made in Chinese, Japanese, etc. stores it in that language)
// and the player can also rename characters in-game. Id is stable across both.
//
// Id is unique; JobId is not (Mog shares job 11 with the nine NPC moogles, and
// Wedge/Biggs share job 18), so Id is the correct key for lookups.
// Source: KiameV/final-fantasy-vi-save-editor, models/pr/baseOffsets.go.
public static class CharacterRoster
{
    public static readonly IReadOnlyList<RosterEntry> All =
    [
        new( 1,  1, "Terra",  false),
        new( 2, 18, "Wedge",  false),
        new( 3, 18, "Biggs",  false),
        // Kefka is absent from the Go reference's table but appears in real saves as
        // id 4 / job 21 (observed in Pixel Remaster saves from the opening sequence).
        new( 4, 21, "Kefka",  true),
        new( 5,  2, "Locke",  false),
        new( 6, 11, "Moglin", true),
        new( 7, 11, "Mogret", true),
        new( 8, 11, "Moggie", true),
        new( 9, 11, "Molulu", true),
        new(10, 11, "Moghan", true),
        new(11, 11, "Moguel", true),
        new(12, 11, "Mogsy",  true),
        new(13, 11, "Mogwin", true),
        new(14, 11, "Mugmug", true),
        new(15, 11, "Cosmog", true),
        new(16, 11, "Mog",    false),
        new(17,  5, "Edgar",  false),
        new(18,  6, "Sabin",  false),
        new(19,  4, "Shadow", false),
        new(20, 15, "Banon",  false),
        new(22,  3, "Cyan",   false),
        new(23, 17, "??????", true),
        new(24, 12, "Gau",    false),
        new(25,  7, "Celes",  false),
        new(26, 10, "Setzer", false),
        new(27, 20, "Maduin", true),
        new(28,  8, "Strago", false),
        new(29,  9, "Relm",   false),
        new(30, 16, "Leo",    false),
        new(32, 14, "Umaro",  false),
        new(33, 13, "Gogo",   false),
    ];

    private static readonly Dictionary<int, RosterEntry> ById = All.ToDictionary(e => e.Id);

    public static RosterEntry? ForId(int id) => ById.GetValueOrDefault(id);

    // Canonical English name for an id, for UI labels that must stay readable regardless
    // of the save's language. Falls back to the id when unknown (e.g. Kefka, id 4).
    public static string EnglishNameFor(int id) => ForId(id)?.EnglishName ?? $"#{id}";

    // Character ids that own each learnable skill set.
    public const int GauId    = 24; // Rages
    public const int CyanId   = 22; // Bushido
    public const int StragoId = 28; // Lore
    public const int SabinId  = 18; // Blitz
    public const int MogId    = 16; // Dance
    public const int SetzerId = 26; // Slot
}
