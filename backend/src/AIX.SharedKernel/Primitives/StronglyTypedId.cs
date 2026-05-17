namespace AIX.SharedKernel.Primitives;

public abstract record StronglyTypedId<TValue>(TValue Value) where TValue : notnull
{
    public override string ToString() => Value.ToString() ?? string.Empty;
}
