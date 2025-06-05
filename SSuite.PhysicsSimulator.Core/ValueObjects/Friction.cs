using System;
using Ardalis.GuardClauses;

namespace SSuite.PhysicsSimulator.Core.ValueObjects;

public readonly record struct Friction(float Value)
{
    public static implicit operator float(Friction friction) => friction.Value;
    public static implicit operator Friction(float value) => new(value);
}
