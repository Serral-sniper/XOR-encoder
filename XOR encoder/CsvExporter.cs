using System.Text;

namespace XorEncoder;

/// <summary>
/// Exporte la solution en deux fichiers CSV :
/// - encoder-11bit.csv : liste des générateurs (conteneurs) avec leur valeur binaire
/// - mappings-11bit.csv : pour chaque item_id, les indices de conteneurs à XORer
/// </summary>
public static class CsvExporter
{
    private static readonly int BitWidth =
        (int)Math.Ceiling(Math.Log2(Config.NItems));

    /// <summary>Exporte l'encodeur et les mappings dans les fichiers configurés.</summary>
    public static void Export(int[] gens, Combo[] assignment)
    {
        ExportEncoder(gens);
        ExportMappings(assignment);
    }

    // ── Encodeur (liste des générateurs) ─────────────────────

    private static void ExportEncoder(int[] gens)
    {
        var sb = new StringBuilder();
        sb.AppendLine("chest_index,decimal_value,binary_11bit");

        for (int i = 0; i < gens.Length; i++)
            sb.AppendLine($"{i},{gens[i]},{ToBinary(gens[i])}");

        File.WriteAllText(Config.OutputEncoder, sb.ToString());
    }

    // ── Mappings (item → conteneurs) ─────────────────────────

    private static void ExportMappings(Combo[] assignment)
    {
        var sb = new StringBuilder();
        sb.AppendLine("item_id,chest_1,chest_2,chest_3");

        for (int id = 0; id < Config.NItems; id++)
        {
            if (id == 0)
            {
                sb.AppendLine($"{id},,,");
                continue;
            }

            Combo c = assignment[id];
            string row = c.Weight switch
            {
                1 => $"{id},{c.G0},,",
                2 => $"{id},{c.G0},{c.G1},",
                _ => $"{id},{c.G0},{c.G1},{c.G2}"
            };
            sb.AppendLine(row);
        }

        File.WriteAllText(Config.OutputMappings, sb.ToString());
    }

    // ── Helpers ───────────────────────────────────────────────

    private static string ToBinary(int value) =>
        Convert.ToString(value, 2).PadLeft(BitWidth, '0');
}
