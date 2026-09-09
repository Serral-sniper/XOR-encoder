namespace XorEncoder;

/// <summary>
/// Vérifie qu'une solution produit bien une bijection parfaite :
/// chaque item_id → exactement un code XOR unique et correct.
/// </summary>
public static class Verifier
{
    /// <summary>
    /// Vérifie l'assignation complète.
    /// Retourne (true, "OK") si parfaite, (false, message d'erreur) sinon.
    /// </summary>
    public static (bool Ok, string Message) Verify(int[] gens, Combo[] assignment)
    {
        int n = Config.NItems;
        var seen = new bool[n];

        for (int id = 0; id < n; id++)
        {
            int xorVal = id == 0 ? 0 : assignment[id].ComputeXor(gens);

            if (xorVal != id)
                return (false, $"Mismatch item {id} : XOR produit {xorVal}, attendu {id}");

            if (seen[xorVal])
                return (false, $"Collision : code {xorVal} attribué à deux items différents");

            seen[xorVal] = true;
        }

        // Vérifier qu'aucun code n'a été oublié
        for (int code = 0; code < n; code++)
            if (!seen[code])
                return (false, $"Code {code} jamais produit (couverture incomplète)");

        return (true, "OK");
    }

    /// <summary>
    /// Vérifie qu'aucun générateur ne dépasse la capacité.
    /// </summary>
    public static (bool Ok, string Message) VerifyCapacity(int[] usage, int capacity)
    {
        for (int i = 0; i < usage.Length; i++)
            if (usage[i] > capacity)
                return (false, $"Générateur {i} : {usage[i]} items > capacité {capacity}");
        return (true, "OK");
    }
}
