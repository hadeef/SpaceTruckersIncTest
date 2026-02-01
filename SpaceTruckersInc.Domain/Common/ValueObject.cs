namespace SpaceTruckersInc.Domain.Common;

public abstract class ValueObject
{
    public static bool operator !=(ValueObject? left, ValueObject? right)
    {
        return !Equals(left, right);
    }

    public static bool operator ==(ValueObject? left, ValueObject? right)
    {
        return Equals(left, right);
    }

    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) ||
        (obj is ValueObject other &&
        GetType() == other.GetType() &&
        GetEqualityComponents().SequenceEqual(other.GetEqualityComponents(), EqualityComparer<object?>.Default));
    }

    public override int GetHashCode()
    {
        return GetEqualityComponents()
            .Select(c => c?.GetHashCode() ?? 0)
            .Aggregate(0, (hash, componentHash) => HashCode.Combine(hash, componentHash));
    }

    protected abstract IEnumerable<object?> GetEqualityComponents();
}