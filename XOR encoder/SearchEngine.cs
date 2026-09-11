using System.Diagnostics;

namespace XorEncoder;

/// <summary>
/// Orchestre la descente de K depuis kStart jusqu'à la borne théorique.
/// Chaque K dispose d'un budget temps fixe (Config.TimeoutPerKSeconds).
/// </summary>
public sealed class SearchEngine
{
    public static int TheoreticalLowerBound() =>
        (int)Math.Ceiling((2.0 * Config.NItems - 2) / (Config.Capacity + 1));

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
                ConsoleDisplay.PrintSolverSuccess(result);
                best = result;
                ConsoleDisplay.PrintDescending(k - 1);
                k--;
            }
            else
            {
                ConsoleDisplay.PrintInfeasible(k, Config.TimeoutPerKSeconds);
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

    private static AssignmentResult TryK(int k)
    {
        ConsoleDisplay.PrintSolverStart(k, Config.TimeoutPerKSeconds);

        using var cts = Config.TimeoutPerKSeconds > 0
            ? new CancellationTokenSource(TimeSpan.FromSeconds(Config.TimeoutPerKSeconds))
            : new CancellationTokenSource();

        var sw     = Stopwatch.StartNew();
        var result = GreedySolver.SolveWithTimeout(k, cts.Token);
        sw.Stop();

        ConsoleDisplay.PrintSolverEnd(result, sw.Elapsed.TotalSeconds);
        return result;
    }
}
