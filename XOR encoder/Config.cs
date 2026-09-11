namespace XorEncoder;

/// <summary>
/// Paramètres globaux du programme.
/// Modifier ici uniquement.
/// </summary>
public static class Config
{
    /// <summary>Nombre de codes à générer (doit être une puissance de 2).</summary>
    public const int NItems = 1024;

    /// <summary>Emplacements disponibles par conteneur (double coffre Minecraft = 54).</summary>
    public const int Capacity = 54;

    /// <summary>Seeds testées en phase glouton pur (phase 1, rapide).</summary>
    public const int MaxSeedsGreedy = 64;

    /// <summary>
    /// On démarre la recherche à K_theorique + StartOffset,
    /// puis on descend dès qu'une solution est trouvée.
    /// </summary>
    public const int StartOffset = 7;

    /// <summary>
    /// Temps maximum (secondes) alloué par valeur de K.
    /// Si écoulé sans solution → K déclaré infaisable, on arrête.
    /// </summary>
    public const int TimeoutPerKSeconds = 30000;

    /// <summary>
    /// Température initiale du recuit simulé.
    /// Plus haute = explore plus largement au début.
    /// Décroît exponentiellement vers 0 au fil des itérations.
    /// </summary>
    public const double SAInitialTemp = 8.0;

    /// <summary>Fichier de sortie : un conteneur par ligne avec sa valeur binaire.</summary>
    public const string OutputEncoder = "encoder-11bit.csv";

    /// <summary>Fichier de sortie : mapping item_id → indices de conteneurs à XORer.</summary>
    public const string OutputMappings = "mappings-11bit.csv";
}
