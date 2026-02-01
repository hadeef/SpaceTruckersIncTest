using SpaceTruckersInc.Domain.Common;

namespace SpaceTruckersInc.Domain.ValueObjects;

public sealed class Cargo : ValueObject
{
    public Cargo(decimal weightKg, decimal volumeCubicMeters, int quantity = 1, string? description = null)
    {
        if (weightKg < 0m)
        {
            throw new ArgumentException("weightKg must be non-negative.", nameof(weightKg));
        }
        if (volumeCubicMeters < 0m)
        {
            throw new ArgumentException("volumeCubicMeters must be non-negative.", nameof(volumeCubicMeters));
        }
        if (quantity <= 0)
        {
            throw new ArgumentException("quantity must be positive.", nameof(quantity));
        }

        WeightKg = weightKg;
        VolumeCubicMeters = volumeCubicMeters;
        Quantity = quantity;
        Description = string.IsNullOrWhiteSpace(description) ? null : description!.Trim();
    }

    public string? Description { get; }

    /// <summary>
    /// Number of identical units.
    /// </summary>
    public int Quantity { get; }

    public decimal TotalVolumeCubicMeters => VolumeCubicMeters * Quantity;

    public decimal TotalWeightKg => WeightKg * Quantity;

    /// <summary>
    /// Volume of a single unit (m³).
    /// </summary>
    public decimal VolumeCubicMeters { get; }

    /// <summary>
    /// Weight of a single unit (kg).
    /// </summary>
    public decimal WeightKg { get; }

    public Cargo WithQuantity(int newQuantity)
    {
        return new Cargo(WeightKg, VolumeCubicMeters, newQuantity, Description);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return WeightKg;
        yield return VolumeCubicMeters;
        yield return Quantity;
        yield return Description;
    }
}