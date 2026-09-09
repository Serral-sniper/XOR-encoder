namespace XorEncoder;

/// <summary>
/// Table de toutes les combinaisons XOR (poids 1, 2, 3) pour un ensemble
/// de K générateurs.
///
/// OPTIMISATION CLEF — Mise à jour incrémentale :
///   Remplacer UN générateur coûte O(K²) au lieu de O(K³) pour une
///   reconstruction complète, soit un gain de ~K fois (≈80x pour K=81).
///   La recherche locale l'exploite massivement.
///
/// Stockage : List&lt;Combo&gt;[] mutable (permet ajout/suppression O(1) amorti)
/// pendant la recherche, puis snapshot en Combo[][] si besoin de perf max.
/// </summary>
public sealed class ComboTable
{
    // Stockage mutable pour permettre les mises à jour incrémentales
    private readonly List<Combo>[] _data;

    public int   K          { get; }
    public int[] Generators { get; }   // mutable, mis à jour par ReplaceGenerator

    // ── Construction complète O(K³) ───────────────────────────

    public ComboTable(int[] generators)
    {
        K          = generators.Length;
        Generators = (int[])generators.Clone();

        int n = Config.NItems;
        _data = new List<Combo>[n];
        for (int i = 0; i < n; i++)
            _data[i] = new List<Combo>();

        BuildAll();
    }

    /// <summary>Clone cette table (deep copy) — utile pour tester sans modifier l'original.</summary>
    public ComboTable Clone()
    {
        var copy = new ComboTable(Generators);   // reconstruit proprement
        return copy;
    }

    // ── Mise à jour incrémentale O(K²) ───────────────────────

    /// <summary>
    /// Remplace le générateur d'indice <paramref name="idx"/> par <paramref name="newVal"/>.
    /// Met à jour la table en O(K²) : retire toutes les combos impliquant
    /// l'ancienne valeur, puis ajoute les nouvelles combos avec la nouvelle valeur.
    /// </summary>
    public void ReplaceGenerator(int idx, int newVal)
    {
        RemoveCombosFor(idx);
        Generators[idx] = newVal;
        AddCombosFor(idx);
    }

    // ── Accesseurs ────────────────────────────────────────────

    public List<Combo> GetCombos(int target)  => _data[target];
    public bool        HasCoverage(int target) => _data[target].Count > 0;
    public int         ComboCount(int target)  => _data[target].Count;

    public bool IsFullyCovered()
    {
        for (int t = 1; t < Config.NItems; t++)
            if (_data[t].Count == 0) return false;
        return true;
    }

    // ── Internals ─────────────────────────────────────────────

    private void BuildAll()
    {
        int k = K, n = Config.NItems;

        // Poids 1
        for (int i = 0; i < k; i++)
        {
            int v = Generators[i];
            if (v > 0 && v < n) _data[v].Add(new Combo(i));
        }

        // Poids 2
        for (int i = 0; i < k; i++)
        for (int j = i + 1; j < k; j++)
        {
            int v = Generators[i] ^ Generators[j];
            if (v > 0 && v < n) _data[v].Add(new Combo(i, j));
        }

        // Poids 3
        for (int i = 0; i < k; i++)
        for (int j = i + 1; j < k; j++)
        {
            int baseXor = Generators[i] ^ Generators[j];
            for (int l = j + 1; l < k; l++)
            {
                int v = baseXor ^ Generators[l];
                if (v > 0 && v < n) _data[v].Add(new Combo(i, j, l));
            }
        }
    }

    /// <summary>Retire toutes les combos impliquant l'indice <paramref name="idx"/>.</summary>
    private void RemoveCombosFor(int idx)
    {
        int k = K, n = Config.NItems;
        int g0 = Generators[idx];

        // Poids 1
        if (g0 > 0 && g0 < n)
            RemoveFirst(_data[g0], c => c.Weight == 1 && c.G0 == idx);

        // Poids 2 : idx apparaît avec chaque autre générateur
        for (int j = 0; j < k; j++)
        {
            if (j == idx) continue;
            int v = g0 ^ Generators[j];
            if (v > 0 && v < n)
            {
                int lo = Math.Min(idx, j), hi = Math.Max(idx, j);
                RemoveFirst(_data[v], c => c.Weight == 2 && c.G0 == lo && c.G1 == hi);
            }
        }

        // Poids 3 : idx apparaît avec chaque paire (j, l), j < l, j≠idx, l≠idx
        for (int j = 0; j < k; j++)
        {
            if (j == idx) continue;
            for (int l = j + 1; l < k; l++)
            {
                if (l == idx) continue;
                int v = g0 ^ Generators[j] ^ Generators[l];
                if (v > 0 && v < n)
                {
                    // Les trois indices triés
                    Span<int> trio = stackalloc int[3] { idx, j, l };
                    trio.Sort();
                    int g0Index = trio[0];
                    int g1Index = trio[1];
                    int g2Index = trio[2];
                    RemoveFirst(_data[v],
                        c => c.Weight == 3
                          && c.G0 == g0Index
                          && c.G1 == g1Index
                          && c.G2 == g2Index);
                }
            }
        }
    }

    /// <summary>Ajoute toutes les combos impliquant l'indice <paramref name="idx"/>
    /// avec sa nouvelle valeur (déjà mise à jour dans Generators[idx]).</summary>
    private void AddCombosFor(int idx)
    {
        int k = K, n = Config.NItems;
        int g0 = Generators[idx];

        // Poids 1
        if (g0 > 0 && g0 < n)
            _data[g0].Add(new Combo(idx));

        // Poids 2
        for (int j = 0; j < k; j++)
        {
            if (j == idx) continue;
            int v = g0 ^ Generators[j];
            if (v > 0 && v < n)
            {
                int lo = Math.Min(idx, j), hi = Math.Max(idx, j);
                _data[v].Add(new Combo(lo, hi));
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
                    _data[v].Add(new Combo(trio[0], trio[1], trio[2]));
                }
            }
        }
    }

    /// <summary>Retire le premier élément satisfaisant le prédicat (swap-and-pop, O(1)).</summary>
    private static void RemoveFirst(List<Combo> list, Func<Combo, bool> pred)
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (!pred(list[i])) continue;
            // Swap with last and remove last → O(1), préserve pas l'ordre (pas grave ici)
            list[i] = list[^1];
            list.RemoveAt(list.Count - 1);
            return;
        }
    }
}
