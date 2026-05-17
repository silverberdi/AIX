namespace AIX.SharedKernel.Primitives;

public readonly record struct CorrelationId(Guid Value)
{
    public static CorrelationId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}

public readonly record struct CausationId(Guid Value)
{
    public override string ToString() => Value.ToString();
}
