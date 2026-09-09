namespace XorEncoder;

/// <summary>
/// Résultat d'une tentative d'assignation (gloutonne ou exacte).
/// Immuable par conception.
/// </summary>
public sealed class AssignmentResult
{
    /// <summary>True si tous les items ont été placés sans dépasser la capacité.</summary>
    public bool Success { get; }

    /// <summary>Nombre de générateurs utilisés.</summary>
    public int K { get; }

    /// <summary>Graine aléatoire ayant produit ce résultat.</summary>
    public int Seed { get; }

    /// <summary>Valeurs des générateurs (un par conteneur logique).</summary>
    public int[] Generators { get; }

    /// <summary>
    /// Pour chaque item_id (index), la combo choisie.
    /// assignment[0] est toujours vide (XOR de rien = 0).
    /// </summary>
    public Combo[] Assignment { get; }

    /// <summary>Nombre d'items utilisés dans chaque générateur.</summary>
    public int[] Usage { get; }

    /// <summary>Nombre d'items non placés (0 si Success).</summary>
    public int Failures { get; }

    /// <summary>Nom de l'algorithme ayant produit ce résultat.</summary>
    public string Method { get; }

    public AssignmentResult(
        bool    success,
        int     k,
        int     seed,
        int[]   generators,
        Combo[] assignment,
        int[]   usage,
        int     failures,
        string  method)
    {
        Success    = success;
        K          = k;
        Seed       = seed;
        Generators = generators;
        Assignment = assignment;
        Usage      = usage;
        Failures   = failures;
        Method     = method;
    }

    // ── Statistiques ──────────────────────────────────────────

    public int MinUsage => Usage.Min();
    public int MaxUsage => Usage.Max();

    /// <summary>Nombre d'items assignés par poids (index = poids : 0, 1, 2, 3).</summary>
    public int[] WeightDistribution()
    {
        var dist = new int[4];
        if (Assignment == null) return dist;
        foreach (var c in Assignment)
            if (c.Weight is >= 1 and <= 3)
                dist[c.Weight]++;
        return dist;
    }
}
