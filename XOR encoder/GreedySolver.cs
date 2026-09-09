namespace XorEncoder;

/// <summary>
/// Solveur combiné : Glouton pur + Local Search incrémental.
///
/// GLOUTON PUR (TrySolve) :
///   Tire K générateurs aléatoires, reconstruit la table en O(K³),
///   assigne greedily. Bon pour découvrir vite une première solution.
///
/// LOCAL SEARCH (LocalSearchSolve) :
///   Part d'une solution existante, remplace UN générateur à la fois
///   en O(K²) grâce à la mise à jour incrémentale de ComboTable.
///   ~K fois plus rapide par itération → exploite massivement les seeds.
///
/// Parallélisme : les deux modes distribuent les seeds sur tous les cœurs.
/// </summary>
public static class GreedySolver
{
    // ═══════════════════════════════════════════════════════
    //  API PUBLIQUE
    // ═══════════════════════════════════════════════════════

    /// <summary>
    /// Mode hybride recommandé :
    ///   - Phase 1 : quelques seeds glouton pur (découverte rapide)
    ///   - Phase 2 : local search intensif depuis les meilleures solutions
    /// </summary>
    public static AssignmentResult SolveHybrid(int k, int maxSeeds)
    {
        int phaseOneSeeds = Math.Min(32, maxSeeds / 10);
        int phaseTwoSeeds = maxSeeds - phaseOneSeeds;

        // Phase 1 — glouton pur (rapide, large éventail)
        var phase1 = SolveParallel(k, phaseOneSeeds, pure: true);
        if (phase1.Success) return phase1;

        // Phase 2 — local search depuis la meilleure solution phase 1
        var phase2 = SolveParallel(k, phaseTwoSeeds, pure: false, startFrom: phase1);
        return phase2.Failures < phase1.Failures ? phase2 : phase1;
    }

    /// <summary>Glouton pur parallèle (reconstruction complète par seed).</summary>
    public static AssignmentResult SolveParallel(
        int k, int maxSeeds, bool pure = true, AssignmentResult? startFrom = null)
    {
        AssignmentResult? best = null;
        int bestFail = int.MaxValue;
        bool found = false;
        object lck = new();

        Parallel.For(0, maxSeeds, new ParallelOptions
        {
            MaxDegreeOfParallelism = Environment.ProcessorCount
        },
        (seed, state) =>
        {
            if (Volatile.Read(ref found)) { state.Break(); return; }

            AssignmentResult result = pure
                ? TrySolve(k, seed)
                : LocalSearchSolve(k, seed, startFrom);

            lock (lck)
            {
                if (result.Failures < bestFail)
                {
                    bestFail = result.Failures;
                    best = result;
                }
                if (result.Success)
                {
                    Volatile.Write(ref found, true);
                    state.Break();
                }
            }
        });

        return best!;
    }

    /// <summary>Séquentiel — utile pour le debug ou machines mono-cœur.</summary>
    public static AssignmentResult SolveSequential(int k, int maxSeeds)
    {
        AssignmentResult? best = null;
        for (int seed = 0; seed < maxSeeds; seed++)
        {
            var r = TrySolve(k, seed);
            if (best == null || r.Failures < best.Failures) best = r;
            if (r.Success) break;
        }
        return best!;
    }

    // ═══════════════════════════════════════════════════════
    //  GLOUTON PUR — O(K³) par seed
    // ═══════════════════════════════════════════════════════

    public static AssignmentResult TrySolve(int k, int seed)
    {
        int[] gens = RandomGenerators(k, seed);
        var table  = new ComboTable(gens);

        if (!table.IsFullyCovered())
            return Fail(k, seed, gens, "Greedy(gap)");

        var (assignment, usage, failures) = Assign(table, k);

        return new AssignmentResult(
            failures == 0, k, seed, gens, assignment, usage, failures, "Greedy");
    }

    // ═══════════════════════════════════════════════════════
    //  LOCAL SEARCH — O(K²) par itération
    // ═══════════════════════════════════════════════════════

    /// <summary>
    /// Parte d'un jeu de générateurs (issu d'un seed ou d'une solution connue),
    /// puis remplace aléatoirement un générateur à la fois et garde la
    /// modification si elle réduit (ou ne dégrade pas) le nombre d'échecs.
    ///
    /// La mise à jour incrémentale de ComboTable évite de tout reconstruire.
    /// </summary>
    public static AssignmentResult LocalSearchSolve(
        int k, int seed, AssignmentResult? startFrom = null)
    {
        var rng = new Random(seed);

        // Générateurs de départ : solution connue ou tirage aléatoire
        int[] gens = startFrom != null
            ? (int[])startFrom.Generators.Clone()
            : RandomGenerators(k, rng.Next());

        var table = new ComboTable(gens);

        // Évaluation initiale
        var (bestAssign, bestUsage, bestFail) = Assign(table, k);
        if (bestFail == 0)
            return new AssignmentResult(true, k, seed, gens, bestAssign, bestUsage, 0, "LocalSearch");

        // Pool des valeurs non encore utilisées comme générateurs
        var usedSet = new HashSet<int>(gens);
        var pool    = Enumerable.Range(1, Config.NItems - 1)
                                .Where(v => !usedSet.Contains(v))
                                .ToList();

        int maxIter = k * k * 20;   // budget d'itérations proportionnel à K²

        for (int iter = 0; iter < maxIter && bestFail > 0; iter++)
        {
            // Choisir un générateur à remplacer (préférer les moins chargés)
            int replaceIdx = rng.Next(k);

            // Choisir une nouvelle valeur depuis le pool
            int poolIdx  = rng.Next(pool.Count);
            int newVal   = pool[poolIdx];
            int oldVal   = table.Generators[replaceIdx];

            // Mise à jour incrémentale O(K²)
            table.ReplaceGenerator(replaceIdx, newVal);

            if (table.IsFullyCovered())
            {
                var (assign, usage, fail) = Assign(table, k);
                if (fail <= bestFail)
                {
                    // Accepter le mouvement
                    bestFail   = fail;
                    bestAssign = assign;
                    bestUsage  = usage;
                    pool[poolIdx] = oldVal;   // l'ancienne valeur rejoint le pool
                    usedSet.Remove(oldVal);
                    usedSet.Add(newVal);
                    if (fail == 0) break;
                    continue;
                }
            }

            // Rejeter : revenir en arrière
            table.ReplaceGenerator(replaceIdx, oldVal);
        }

        return new AssignmentResult(
            bestFail == 0, k, seed,
            (int[])table.Generators.Clone(),
            bestAssign, bestUsage, bestFail, "LocalSearch");
    }

    // ═══════════════════════════════════════════════════════
    //  CŒUR : ASSIGNATION GLOUTONNE
    // ═══════════════════════════════════════════════════════

    /// <summary>
    /// Assignation gloutonne sur une ComboTable déjà construite.
    /// Retourne (assignment, usage, failures).
    /// </summary>
    public static (Combo[] assignment, int[] usage, int failures)
        Assign(ComboTable table, int k)
    {
        int n   = Config.NItems;
        int cap = Config.Capacity;

        // Trier les cibles du plus contraint au moins contraint
        int[] targets = Enumerable.Range(1, n - 1).ToArray();
        Array.Sort(targets, (a, b) =>
            table.ComboCount(a).CompareTo(table.ComboCount(b)));

        var assignment = new Combo[n];
        var usage      = new int[k];
        int failures   = 0;

        foreach (int t in targets)
        {
            var combos    = table.GetCombos(t);
            Combo best    = default;
            bool  placed  = false;
            int   bestScore = int.MaxValue;

            foreach (Combo c in combos)
            {
                if (!c.HasCapacity(usage, cap)) continue;
                int score = c.Weight * 1000 + c.MaxLoad(usage);
                if (score < bestScore) { bestScore = score; best = c; placed = true; }
            }

            if (!placed) { failures++; continue; }
            best.IncrementUsage(usage);
            assignment[t] = best;
        }

        return (assignment, usage, failures);
    }

    // ═══════════════════════════════════════════════════════
    //  HELPERS
    // ═══════════════════════════════════════════════════════

    /// <summary>Fisher-Yates partiel — O(K) au lieu de O(NItems).</summary>
    public static int[] RandomGenerators(int k, int seed)
    {
        var rng  = new Random(seed);
        var pool = Enumerable.Range(1, Config.NItems - 1).ToArray();
        for (int i = 0; i < k; i++)
        {
            int j = rng.Next(i, pool.Length);
            (pool[i], pool[j]) = (pool[j], pool[i]);
        }
        return pool[..k];
    }

    private static AssignmentResult Fail(int k, int seed, int[] gens, string method) =>
        new(false, k, seed, gens, Array.Empty<Combo>(), new int[k], Config.NItems, method);
}
