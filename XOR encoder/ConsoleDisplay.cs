namespace XorEncoder;

/// <summary>Centralise tout l'affichage console.</summary>
public static class ConsoleDisplay
{
    private const int Width = 62;
    private static readonly string Line  = new('─', Width);
    private static readonly string Heavy = new('═', Width);

    public static void PrintHeader(int kTheory, int kStart)
    {
        Console.WriteLine(Heavy);
        Console.WriteLine("  XOR Encoder Search — C# Edition");
        Console.WriteLine($"  NItems           = {Config.NItems}");
        Console.WriteLine($"  Capacity         = {Config.Capacity}");
        Console.WriteLine($"  Borne théorique  = K = {kTheory}");
        Console.WriteLine($"  Recherche depuis   K = {kStart}  (descend si solution trouvée)");
        Console.WriteLine($"  Timeout par K    = {(Config.TimeoutPerKSeconds > 0 ? $"{Config.TimeoutPerKSeconds}s" : "désactivé")}");
        Console.WriteLine($"  Threads          = {Environment.ProcessorCount} (zéro lock pendant le calcul)");
        Console.WriteLine($"  Phases           = Glouton → LocalSearch → RecuitSimulé");
        Console.WriteLine($"  Optimisation     = séparation poids 1/2/3 + early break (~8x)");
        Console.WriteLine(Heavy);
    }

    public static void PrintAttempt(int k) =>
        Console.WriteLine($"\n{Line}\n  Tentative K = {k}");

    public static void PrintSolverStart(int k, int timeoutSec)
    {
        string t = timeoutSec > 0 ? $"  (timeout = {timeoutSec}s)" : "";
        Console.WriteLine($"  Lancement{t}...");
    }

    public static void PrintSolverEnd(AssignmentResult r, double seconds)
    {
        if (r.Success)
            Console.WriteLine($"  Durée : {seconds:F1}s");
        else
            Console.WriteLine($"  Timeout après {seconds:F1}s — meilleur = {r.Failures} échecs restants");
    }

    public static void PrintSolverSuccess(AssignmentResult r) =>
        Console.WriteLine($"  ✓ SOLUTION trouvée via [{r.Method}] (seed {r.Seed})");

    public static void PrintDescending(int nextK) =>
        Console.WriteLine($"  → K faisable, on tente K = {nextK}...");

    public static void PrintInfeasible(int k, int timeoutSec) =>
        Console.WriteLine($"  ✗ K = {k} non résolu en {timeoutSec}s — arrêt.");

    public static void PrintFinalResult(AssignmentResult r, double totalSeconds)
    {
        Console.WriteLine($"\n{Heavy}");
        Console.WriteLine($"  RÉSULTAT FINAL : K minimum = {r.K}");
        int[] wd = r.WeightDistribution();
        Console.WriteLine($"  Méthode  : {r.Method} (seed {r.Seed})");
        Console.WriteLine($"  Usage    : min={r.MinUsage}, max={r.MaxUsage}");
        Console.WriteLine($"  Poids    : 1×{wd[1]}  2×{wd[2]}  3×{wd[3]}");
        Console.WriteLine($"  Temps    : {totalSeconds:F1}s");
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
        Console.WriteLine("  Fichiers exportés :");
        Console.WriteLine($"    {Config.OutputEncoder}");
        Console.WriteLine($"    {Config.OutputMappings}");
    }
}
