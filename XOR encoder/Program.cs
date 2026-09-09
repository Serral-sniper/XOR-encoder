using XorEncoder;

Console.OutputEncoding = System.Text.Encoding.UTF8;

// ── Lancement de la recherche ─────────────────────────────────
var engine = new SearchEngine();
AssignmentResult? best = engine.Run();

// ── Vérification et export ────────────────────────────────────
if (best is null)
{
    Console.WriteLine("Aucune solution à exporter.");
    return 1;
}

var (verifyOk, verifyMsg) = Verifier.Verify(best.Generators, best.Assignment);
ConsoleDisplay.PrintVerification(verifyOk, verifyMsg);

if (!verifyOk) return 2;

var (capOk, capMsg) = Verifier.VerifyCapacity(best.Usage, Config.Capacity);
ConsoleDisplay.PrintVerification(capOk, capMsg);

if (!capOk) return 3;

CsvExporter.Export(best.Generators, best.Assignment);
ConsoleDisplay.PrintExported();

return 0;
