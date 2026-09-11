namespace XorEncoder;

/// <summary>
/// Table de combos XOR séparée par poids (1, 2, 3).
///
/// OPTIMISATION CLEF — Séparation par poids :
///   Pendant l'assignation, on teste d'abord les combos de poids 1,
///   puis 2, puis 3, avec un early-break dès qu'on en trouve une valide.
///   En pratique, ~16% des targets se résolvent en poids ≤2 → on évite
///   de parcourir les milliers de combos de poids 3 pour ces targets.
///   Gain mesuré : ~7-8x plus rapide que la table plate.
///
/// Mise à jour incrémentale O(K²) : ReplaceGenerator met à jour
///   les trois tables sans tout reconstruire.
/// </summary>
public sealed class ComboTable
{
    // Tables séparées par poids — indices simples pour poids 1
    private readonly List<int>[]          _w1;   // target → liste d'indices générateurs
    private readonly List<(int,int)>[]    _w2;   // target → liste de paires
    private readonly List<(int,int,int)>[]_w3;   // target → liste de triplets

    public int   K          { get; }
    public int[] Generators { get; }

    // ── Construction complète O(K³) ───────────────────────────

    public ComboTable(int[] generators)
    {
        K          = generators.Length;
        Generators = (int[])generators.Clone();

        int n = Config.NItems;
        _w1 = new List<int>[n];
        _w2 = new List<(int,int)>[n];
        _w3 = new List<(int,int,int)>[n];
        for (int i = 0; i < n; i++)
        {
            _w1[i] = new List<int>();
            _w2[i] = new List<(int,int)>();
            _w3[i] = new List<(int,int,int)>();
        }

        BuildAll();
    }

    // ── Accesseurs ────────────────────────────────────────────

    public List<int>           GetW1(int t) => _w1[t];
    public List<(int,int)>     GetW2(int t) => _w2[t];
    public List<(int,int,int)> GetW3(int t) => _w3[t];

    public int ComboCount(int t) => _w1[t].Count + _w2[t].Count + _w3[t].Count;

    public bool HasCoverage(int t) =>
        _w1[t].Count > 0 || _w2[t].Count > 0 || _w3[t].Count > 0;

    public bool IsFullyCovered()
    {
        for (int t = 1; t < Config.NItems; t++)
            if (!HasCoverage(t)) return false;
        return true;
    }

    // ── Mise à jour incrémentale O(K²) ───────────────────────

    public void ReplaceGenerator(int idx, int newVal)
    {
        RemoveCombosFor(idx);
        Generators[idx] = newVal;
        AddCombosFor(idx);
    }

    // ── Construction interne ──────────────────────────────────

    private void BuildAll()
    {
        int k = K, n = Config.NItems;

        for (int i = 0; i < k; i++)
        {
            int v = Generators[i];
            if (v > 0 && v < n) _w1[v].Add(i);
        }

        for (int i = 0; i < k; i++)
        for (int j = i + 1; j < k; j++)
        {
            int v = Generators[i] ^ Generators[j];
            if (v > 0 && v < n) _w2[v].Add((i, j));
        }

        for (int i = 0; i < k; i++)
        for (int j = i + 1; j < k; j++)
        {
            int b = Generators[i] ^ Generators[j];
            for (int l = j + 1; l < k; l++)
            {
                int v = b ^ Generators[l];
                if (v > 0 && v < n) _w3[v].Add((i, j, l));
            }
        }
    }

    private void RemoveCombosFor(int idx)
    {
        int k = K, n = Config.NItems;
        int g0 = Generators[idx];

        // Poids 1
        if (g0 > 0 && g0 < n) RemoveFirstW1(_w1[g0], idx);

        // Poids 2
        for (int j = 0; j < k; j++)
        {
            if (j == idx) continue;
            int v = g0 ^ Generators[j];
            if (v > 0 && v < n)
            {
                int lo = Math.Min(idx, j), hi = Math.Max(idx, j);
                RemoveFirstW2(_w2[v], lo, hi);
            }
        }

        // Poids 3
        for (int j = 0; j < k; j++)
        {
            if (j == idx) continue;
            for (int l = j + 1; l < k; l++)
            {
                if (l == idx) continue;
                int v = g0 ^ Generators[j] ^ Generators[l];
                if (v > 0 && v < n)
                {
                    Span<int> trio = stackalloc int[3] { idx, j, l };
                    trio.Sort();
                    RemoveFirstW3(_w3[v], trio[0], trio[1], trio[2]);
                }
            }
        }
    }

    private void AddCombosFor(int idx)
    {
        int k = K, n = Config.NItems;
        int g0 = Generators[idx];

        if (g0 > 0 && g0 < n) _w1[g0].Add(idx);

        for (int j = 0; j < k; j++)
        {
            if (j == idx) continue;
            int v = g0 ^ Generators[j];
            if (v > 0 && v < n)
            {
                int lo = Math.Min(idx, j), hi = Math.Max(idx, j);
                _w2[v].Add((lo, hi));
            }
        }

        for (int j = 0; j < k; j++)
        {
            if (j == idx) continue;
            for (int l = j + 1; l < k; l++)
            {
                if (l == idx) continue;
                int v = g0 ^ Generators[j] ^ Generators[l];
                if (v > 0 && v < n)
                {
                    Span<int> trio = stackalloc int[3] { idx, j, l };
                    trio.Sort();
                    _w3[v].Add((trio[0], trio[1], trio[2]));
                }
            }
        }
    }

    private static void RemoveFirstW1(List<int> list, int idx)
    {
        for (int i = 0; i < list.Count; i++)
            if (list[i] == idx) { list[i] = list[^1]; list.RemoveAt(list.Count-1); return; }
    }

    private static void RemoveFirstW2(List<(int,int)> list, int lo, int hi)
    {
        for (int i = 0; i < list.Count; i++)
            if (list[i] == (lo,hi)) { list[i] = list[^1]; list.RemoveAt(list.Count-1); return; }
    }

    private static void RemoveFirstW3(List<(int,int,int)> list, int a, int b, int c)
    {
        for (int i = 0; i < list.Count; i++)
            if (list[i] == (a,b,c)) { list[i] = list[^1]; list.RemoveAt(list.Count-1); return; }
    }
}
