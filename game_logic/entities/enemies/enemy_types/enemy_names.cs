using System.Collections.Generic;
using System.Linq;
using GameLogic.Systems;

namespace GameLogic.Entities.Enemies.EnemyTypes
{
    /// <summary>
    /// The name lists for regular enemies, one per family (docs/enemy_roster.md, final
    /// 2026-11-24). An enemy is shown as "Name the Type" ("Grik the Goblin"), picked fresh each
    /// fight with no repeats in one fight. Bosses keep their own names.
    /// </summary>
    public static class EnemyNames
    {
        private static readonly Dictionary<string, string[]> NamesByFamily = new Dictionary<string, string[]>
        {
            [EnemyRoster.Goblins] = new[] { "Grik", "Snag", "Nib", "Wort", "Skab", "Gub", "Rizzle", "Muck", "Fizz", "Krag" },
            [EnemyRoster.Bandits] = new[] { "Knuckles", "Scar", "Two-Toes", "Lefty", "Rook", "Patch", "Mad Mags", "Sly Sal", "Crow", "Dirk" },
            [EnemyRoster.OwnArmy] = new[] { "Tom", "Bob", "Will", "Ned", "Hugh", "Martha", "Greta", "Ivy", "Bram", "Cole" },
            [EnemyRoster.KingdomB] = new[] { "Anselm", "Dorek", "Valka", "Ilsa", "Torvin", "Mira", "Brannoc", "Zelde", "Kasimir", "Oda" },
            [EnemyRoster.CastleGuards] = new[] { "Gerald", "Percy", "Edmund", "Agnes", "Mabel", "Oswin", "Cedric", "Winifred", "Rupert", "Hilda" },
            [EnemyRoster.Wolves] = new[] { "Fang", "Ash", "Grey", "Howl", "Shade", "Thorn", "Rill", "Snarl", "Ember", "Frost" },
            [EnemyRoster.Slimes] = new[] { "Gloop", "Blorp", "Wobble", "Squish", "Jiggle", "Ooze", "Plop", "Goober", "Burble", "Splat" },
            [EnemyRoster.Bugs] = new[] { "Skitter", "Click", "Nibbles", "Chitter", "Itsy", "Crunch", "Buzz", "Dot", "Pincer", "Lacy" },
            [EnemyRoster.Fungusoars] = new[] { "Rex", "Morel", "Truffle", "Porcini", "Enoki", "Shiitake", "Puffball", "Chanterelle", "Button", "Toadstool" },
            [EnemyRoster.Imps] = new[] { "Snik", "Pox", "Grub", "Cinder", "Hex", "Nettle", "Jinx", "Smudge", "Wick", "Gnash" },
            [EnemyRoster.Dragons] = new[] { "Emberclaw", "Ashwing", "Cindermaw", "Scorchtail", "Smoulder", "Blazefang", "Pyra", "Kindle" },
        };

        /// <summary>A family's name list (empty for an unknown family or null).</summary>
        public static IReadOnlyList<string> GetNames(string family)
        {
            return family != null && NamesByFamily.TryGetValue(family, out var names)
                ? names
                : System.Array.Empty<string>();
        }

        /// <summary>
        /// Pick a name from the family's list that isn't in usedNames. If every name has been
        /// used this fight (only possible when enemies keep joining), one not worn by a standing
        /// enemy is reused (inUseNow). Null if the family has no list or nothing is free.
        /// </summary>
        public static string PickName(string family, ICollection<string> usedNames, ICollection<string> inUseNow, RNGManager rng)
        {
            var names = GetNames(family);
            if (names.Count == 0) return null;

            var free = names.Where(n => usedNames == null || !usedNames.Contains(n)).ToList();
            if (free.Count == 0)
            {
                free = names.Where(n => inUseNow == null || !inUseNow.Contains(n)).ToList();
            }
            if (free.Count == 0) return null;

            return free[rng.Next(0, free.Count)];
        }
    }
}
