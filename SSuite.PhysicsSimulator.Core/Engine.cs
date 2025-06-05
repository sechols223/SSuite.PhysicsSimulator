using System.Numerics;
using SSuite.PhysicsSimulator.Core.PhysicalObjects;
using SSuite.PhysicsSimulator.Core.ValueObjects;

namespace SSuite.PhysicsSimulator.Core;

public sealed class Engine
{
    private readonly Vector2 gravity;
    private readonly AirResistance airResistance;

    private readonly List<PhysicalObject> physicalObjects = [];

    public Engine(Vector2 gravity, AirResistance? airResistance = null)
    {
        this.gravity = gravity;
        this.airResistance = airResistance ?? new AirResistance(0.01f);
    }

    public void AddPhysicalObject(PhysicalObject physicalObject)
        => this.physicalObjects.Add(physicalObject);

    public void RemovePhysicalObject(PhysicalObject physicalObject)
        => this.physicalObjects.Remove(physicalObject);

    public void UpdatePhysicalObjects(DeltaTime deltaTime)
    {
        this.ApplyGlobalForces();
        this.CheckCollisions();

        foreach (var physicalObject in this.physicalObjects)
        {
            physicalObject.UpdateKinematicVectors(deltaTime);
        }
    }

    private void ApplyGlobalForces()
    {
        foreach (var physicalObject in this.physicalObjects)
        {
            physicalObject.ApplyGlobalForces(this.airResistance, this.gravity);
        }
    }

    private void CheckCollisions()
    {
        var count = this.physicalObjects.Count;

        for (var i = 0; i < count; i++)
        {
            for (var j = i + 1; j < count; j++)
            {
                var collidingObject = this.physicalObjects[j];
                this.physicalObjects[i].ResolveCollision(collidingObject);
            }
        }
    }
}
