namespace XorEncoder;

/// <summary>
/// Centralise tout l'affichage console pour garder
/// le reste du code lisible et testable sans IO.
/// </summary>
public static class ConsoleDisplay
{
    private const int Width = 60;
    private static readonly string Line  = new('─', Width);
    private static readonly string Heavy = new('═', Width);

    // ── En-tête ───────────────────────────────────────────────

    public static void PrintHeader(int kTheory, int kStart)
    {
        Console.WriteLine(Heavy);
        Console.WriteLine("  XOR Encoder Search — C# Edition");
        Console.WriteLine($"  NItems          = {Config.NItems}");
        Console.WriteLine($"  Capacity        = {Config.Capacity}");
        Console.WriteLine($"  Borne théorique = K = {kTheory}");
        Console.WriteLine($"  Recherche depuis  K = {kStart}  (descend si solution trouvée)");
        Console.WriteLine($"  Parallélisme    = {(Config.ParallelGreedy
            ? $"oui ({Environment.ProcessorCount} cœurs)"
            : "non")}");
        Console.WriteLine(Heavy);
    }

    // ── Tentative de K ────────────────────────────────────────

    public static void PrintAttempt(int k) =>
        Console.WriteLine($"\n{Line}\n  Tentative K = {k}");

    public static void PrintGreedyStart(int maxSeeds) =>
        Console.Write($"  [Hybride] {maxSeeds} seeds (glouton + local search incrémental)" +
                      (Config.ParallelGreedy
                          ? $" × {Environment.ProcessorCount} cœurs..."
                          : "..."));

    public static void PrintGreedyEnd(double seconds) =>
        Console.WriteLine($" ({seconds:F1}s)");

    public static void PrintGreedySuccess(int seed) =>
        Console.WriteLine($"  [Glouton] SOLUTION trouvée (graine {seed}) !");

    public static void PrintGreedyFail(int bestFail) =>
        Console.WriteLine($"  [Glouton] échec — meilleur = {bestFail} items non placés");

    public static void PrintDescending(int nextK) =>
        Console.WriteLine($"  --> K faisable, on tente K = {nextK}...");

    public static void PrintInfeasible(int k) =>
        Console.WriteLine($"  --> K = {k} semble INFAISABLE, arrêt.");

    // ── Résultat final ────────────────────────────────────────

    public static void PrintFinalResult(AssignmentResult result, double totalSeconds)
    {
        Console.WriteLine($"\n{Heavy}");
        Console.WriteLine($"  RÉSULTAT FINAL : K minimum = {result.K}");
        PrintStats(result);
        Console.WriteLine($"  Temps total : {totalSeconds:F1}s");
        Console.WriteLine(Heavy);
    }

    public static void PrintNoSolution(int kStart)
    {
        Console.WriteLine($"\n{Heavy}");
        Console.WriteLine($"  Aucune solution trouvée jusqu'à K = {kStart}");
        Console.WriteLine(Heavy);
    }

    public static void PrintVerification(bool ok, string msg)
    {
        Console.Write("  Vérification... ");
        Console.WriteLine(ok ? "OK ✓" : $"ERREUR : {msg}");
    }

    public static void PrintExported()
    {
        Console.WriteLine($"  Fichiers exportés :");
        Console.WriteLine($"    {Config.OutputEncoder}");
        Console.WriteLine($"    {Config.OutputMappings}");
    }

    // ── Stats ─────────────────────────────────────────────────

    private static void PrintStats(AssignmentResult r)
    {
        int[] wd = r.WeightDistribution();
        Console.WriteLine($"  Méthode         : {r.Method} (graine {r.Seed})");
        Console.WriteLine($"  Usage conteneurs: min={r.MinUsage}, max={r.MaxUsage}");
        Console.WriteLine($"  Distribution    : " +
                          $"poids1={wd[1]}, poids2={wd[2]}, poids3={wd[3]}");
    }
}
