namespace XorEncoder;

/// <summary>
/// Paramètres globaux du programme.
/// Modifier ici uniquement — aucune autre classe ne contient de constantes métier.
/// </summary>
public static class Config
{
    /// <summary>Nombre de codes à générer (doit être une puissance de 2).</summary>
    public const int NItems = 2048;

    /// <summary>Emplacements disponibles par conteneur (double coffre Minecraft = 54).</summary>
    public const int Capacity = 54;

    /// <summary>Nombre de graines testées par le solveur glouton.</summary>
    public const int MaxSeedsGreedy = 300;

    /// <summary>
    /// On démarre la recherche à K_theorique + StartOffset,
    /// puis on descend dès qu'une solution est trouvée.
    /// </summary>
    public const int StartOffset = 3;

    /// <summary>Paralléliser les seeds glouton sur tous les cœurs disponibles.</summary>
    public const bool ParallelGreedy = true;

    /// <summary>Fichier de sortie : un conteneur par ligne avec sa valeur binaire.</summary>
    public const string OutputEncoder = "encoder-11bit.csv";

    /// <summary>Fichier de sortie : mapping item_id → indices de conteneurs à XORer.</summary>
    public const string OutputMappings = "mappings-11bit.csv";
}
