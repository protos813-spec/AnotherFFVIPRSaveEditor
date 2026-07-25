namespace Ffvi.SaveTool.Data;

// Per-level growth for party members (Pixel Remaster).
// Exp is the CUMULATIVE experience required to reach that level; Hp/Mp are the
// level-derived portion of max HP/MP, on top of the character base in
// CharacterBaseStats. Source: FF wiki "Final Fantasy VI stats" level evolution table.
//
// The game recomputes level from experience after each battle, so any edit to a
// character level must set currentExp to match, or the level reverts on the next
// battle (with a spurious "Level Up" message).
public record LevelRow(int Level, int Hp, int Mp, int Exp);

public static class LevelGrowth
{
    public const int MinLevel = 1;
    public const int MaxLevel = 99;

    public static readonly IReadOnlyList<LevelRow> Rows =
    [
        new(1, 0, 0, 0),
        new(2, 11, 4, 32),
        new(3, 23, 8, 96),
        new(4, 37, 13, 208),
        new(5, 54, 18, 400),
        new(6, 74, 24, 672),
        new(7, 96, 30, 1056),
        new(8, 120, 37, 1552),
        new(9, 146, 45, 2184),
        new(10, 173, 53, 2976),
        new(11, 201, 62, 3936),
        new(12, 231, 71, 5080),
        new(13, 266, 81, 6432),
        new(14, 305, 91, 7992),
        new(15, 349, 101, 9784),
        new(16, 399, 111, 11840),
        new(17, 453, 121, 14152),
        new(18, 510, 132, 16736),
        new(19, 571, 143, 19616),
        new(20, 636, 154, 22832),
        new(21, 703, 165, 26360),
        new(22, 772, 176, 30232),
        new(23, 844, 188, 34456),
        new(24, 920, 200, 39056),
        new(25, 999, 212, 44072),
        new(26, 1081, 224, 49464),
        new(27, 1167, 236, 55288),
        new(28, 1257, 249, 61568),
        new(29, 1352, 262, 68304),
        new(30, 1451, 275, 75496),
        new(31, 1551, 288, 83184),
        new(32, 1652, 301, 91384),
        new(33, 1754, 315, 100088),
        new(34, 1856, 329, 109344),
        new(35, 1959, 343, 119136),
        new(36, 2063, 357, 129504),
        new(37, 2169, 371, 140464),
        new(38, 2276, 386, 152008),
        new(39, 2384, 401, 164184),
        new(40, 2494, 416, 176976),
        new(41, 2605, 431, 190416),
        new(42, 2718, 446, 204520),
        new(43, 2832, 462, 219320),
        new(44, 2948, 478, 234808),
        new(45, 3065, 494, 251000),
        new(46, 3184, 510, 267936),
        new(47, 3304, 526, 285600),
        new(48, 3426, 543, 304040),
        new(49, 3551, 560, 323248),
        new(50, 3679, 577, 343248),
        new(51, 3809, 593, 364064),
        new(52, 3940, 608, 385696),
        new(53, 4073, 622, 408160),
        new(54, 4207, 635, 431488),
        new(55, 4343, 647, 455680),
        new(56, 4480, 658, 480776),
        new(57, 4619, 668, 506760),
        new(58, 4761, 677, 533680),
        new(59, 4905, 685, 561528),
        new(60, 5050, 692, 590320),
        new(61, 5197, 698, 620096),
        new(62, 5345, 703, 650840),
        new(63, 5495, 708, 682600),
        new(64, 5647, 714, 715368),
        new(65, 5800, 720, 749160),
        new(66, 5955, 727, 784016),
        new(67, 6111, 734, 819920),
        new(68, 6269, 741, 856920),
        new(69, 6429, 749, 895016),
        new(70, 6591, 757, 934208),
        new(71, 6751, 765, 974536),
        new(72, 6906, 773, 1016000),
        new(73, 7057, 781, 1058640),
        new(74, 7202, 788, 1102456),
        new(75, 7342, 795, 1147456),
        new(76, 7478, 802, 1193648),
        new(77, 7610, 808, 1241080),
        new(78, 7736, 814, 1289744),
        new(79, 7856, 820, 1339672),
        new(80, 7973, 826, 1390872),
        new(81, 8086, 831, 1443368),
        new(82, 8196, 836, 1497160),
        new(83, 8304, 841, 1552264),
        new(84, 8409, 846, 1608712),
        new(85, 8511, 851, 1666512),
        new(86, 8611, 856, 1725688),
        new(87, 8709, 861, 1786240),
        new(88, 8804, 867, 1848184),
        new(89, 8896, 873, 1911552),
        new(90, 8986, 879, 1976352),
        new(91, 9074, 885, 2042608),
        new(92, 9161, 891, 2110320),
        new(93, 9246, 898, 2179504),
        new(94, 9329, 906, 2250192),
        new(95, 9411, 915, 2322392),
        new(96, 9491, 925, 2396128),
        new(97, 9574, 936, 2471400),
        new(98, 9660, 948, 2548224),
        new(99, 9748, 961, 2637112),
    ];

    // Cumulative experience needed to be at the given level.
    public static int ExpForLevel(int level)
    {
        level = Math.Clamp(level, MinLevel, MaxLevel);
        return Rows[level - 1].Exp;
    }

    // Level the game will derive from a given experience total.
    public static int LevelForExp(int exp)
    {
        var level = MinLevel;
        foreach (var r in Rows)
            if (exp >= r.Exp) level = r.Level;
        return level;
    }
}
