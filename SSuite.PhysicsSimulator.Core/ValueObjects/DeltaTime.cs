namespace SSuite.PhysicsSimulator.Core.ValueObjects;

public readonly record struct DeltaTime(float Value)
{
    public static implicit operator float(DeltaTime deltaTime) => deltaTime.Value;
    public static implicit operator DeltaTime(float value) => new(value);
}
