using System.Runtime.CompilerServices;

namespace XorEncoder;

/// <summary>
/// Représente une combinaison de 1, 2 ou 3 indices de générateurs
/// dont le XOR produit une valeur cible donnée.
///
/// Stockée en struct (4 bytes sur la stack) pour éviter toute
/// allocation heap dans les boucles critiques.
/// </summary>
public readonly struct Combo
{
    public readonly byte G0;
    public readonly byte G1;
    public readonly byte G2;

    /// <summary>Nombre de générateurs impliqués (1, 2 ou 3).</summary>
    public readonly byte Weight;

    public Combo(int g0)
    {
        G0 = (byte)g0; G1 = G2 = 0; Weight = 1;
    }

    public Combo(int g0, int g1)
    {
        G0 = (byte)g0; G1 = (byte)g1; G2 = 0; Weight = 2;
    }

    public Combo(int g0, int g1, int g2)
    {
        G0 = (byte)g0; G1 = (byte)g1; G2 = (byte)g2; Weight = 3;
    }

    /// <summary>
    /// Charge maximale parmi les générateurs de cette combo.
    /// Utilisé pour le tri par équilibrage de charge.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int MaxLoad(int[] usage) => Weight switch
    {
        1 => usage[G0],
        2 => Math.Max(usage[G0], usage[G1]),
        _ => Math.Max(usage[G0], Math.Max(usage[G1], usage[G2]))
    };

    /// <summary>
    /// True si tous les générateurs de cette combo sont encore
    /// sous la capacité maximale autorisée.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool HasCapacity(int[] usage, int cap) => Weight switch
    {
        1 => usage[G0] < cap,
        2 => usage[G0] < cap && usage[G1] < cap,
        _ => usage[G0] < cap && usage[G1] < cap && usage[G2] < cap
    };

    /// <summary>
    /// Incrémente le compteur d'usage de chaque générateur impliqué.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void IncrementUsage(int[] usage)
    {
        usage[G0]++;
        if (Weight >= 2) usage[G1]++;
        if (Weight == 3) usage[G2]++;
    }

    /// <summary>
    /// Calcule la valeur XOR produite par cette combo avec les générateurs fournis.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int ComputeXor(int[] gens) => Weight switch
    {
        1 => gens[G0],
        2 => gens[G0] ^ gens[G1],
        _ => gens[G0] ^ gens[G1] ^ gens[G2]
    };

    public override string ToString() => Weight switch
    {
        1 => $"({G0})",
        2 => $"({G0},{G1})",
        _ => $"({G0},{G1},{G2})"
    };
}
