using System.Diagnostics;

namespace XorEncoder;

/// <summary>
/// Orchestre la descente de K depuis kStart jusqu'à la borne théorique.
///
/// Pour chaque K :
///   1. Lance le solveur glouton (rapide, parallèle).
///   2. Si succès → sauvegarde, tente K-1.
///   3. Si échec  → arrêt (K semble infaisable).
/// </summary>
public sealed class SearchEngine
{
    // ── Borne théorique ───────────────────────────────────────

    /// <summary>
    /// Borne inférieure mathématique :
    ///   usage_min = K + (N-1-K)*2 = 2N - K - 2
    ///   Condition : 2N - K - 2 ≤ K × capacity
    ///   => K ≥ (2N - 2) / (capacity + 1)
    /// </summary>
    public static int TheoreticalLowerBound() =>
        (int)Math.Ceiling((2.0 * Config.NItems - 2) / (Config.Capacity + 1));

    // ── Recherche principale ──────────────────────────────────

    /// <summary>
    /// Lance la recherche descendante et retourne la meilleure solution trouvée,
    /// ou null si aucune solution n'a pu être construite.
    /// </summary>
    public AssignmentResult? Run()
    {
        int kTheory = TheoreticalLowerBound();
        int kStart  = kTheory + Config.StartOffset;
        var clock   = Stopwatch.StartNew();

        ConsoleDisplay.PrintHeader(kTheory, kStart);

        AssignmentResult? best = null;
        int k = kStart;

        while (k >= kTheory)
        {
            ConsoleDisplay.PrintAttempt(k);

            var result = TryK(k);

            if (result is { Success: true })
            {
                ConsoleDisplay.PrintGreedySuccess(result.Seed);
                best = result;
                ConsoleDisplay.PrintDescending(k - 1);
                k--;
            }
            else
            {
                int fail = result?.Failures ?? -1;
                ConsoleDisplay.PrintGreedyFail(fail);
                ConsoleDisplay.PrintInfeasible(k);
                break;
            }
        }

        clock.Stop();

        if (best != null)
            ConsoleDisplay.PrintFinalResult(best, clock.Elapsed.TotalSeconds);
        else
            ConsoleDisplay.PrintNoSolution(kStart);

        return best;
    }

    // ── Tentative pour un K donné ─────────────────────────────

    private static AssignmentResult TryK(int k)
    {
        ConsoleDisplay.PrintGreedyStart(Config.MaxSeedsGreedy);
        var sw = Stopwatch.StartNew();

        // Mode hybride : glouton pur (phase 1) + local search incrémental (phase 2)
        // Le local search est ~K fois plus rapide par itération grâce à la
        // mise à jour incrémentale de la ComboTable (O(K²) vs O(K³)).
        AssignmentResult result = Config.ParallelGreedy
            ? GreedySolver.SolveHybrid(k, Config.MaxSeedsGreedy)
            : GreedySolver.SolveSequential(k, Config.MaxSeedsGreedy);

        ConsoleDisplay.PrintGreedyEnd(sw.Elapsed.TotalSeconds);
        return result;
    }
}
