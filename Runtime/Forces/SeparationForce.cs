using UnityEngine;

namespace MyProject.SwarmMovement
{
    public class SeparationForce : IMovementForce
    {
        public MovementForceType MovementForceType => MovementForceType.Separation;

        public Vector3 Calculate(ISwarmMovable swarmMovable)
        {
            var force = Vector3.zero;
            var count = 0;
            var position = swarmMovable.Position;
            position.y = 0;
            
            for (var i = 0; i < swarmMovable.Neighbors.Count; i++)
            {
                var neighbor = swarmMovable.Neighbors[i];
                var toNeighbor = position - neighbor.Position;
                toNeighbor.y = 0;

                var distanceSqr = toNeighbor.sqrMagnitude;
                if (distanceSqr < SwarmMovementSystem.MinSqr ||
                    distanceSqr > swarmMovable.SeparationDistanceSqr) continue;
                
                force += toNeighbor / distanceSqr;
                count++;
            }

            force.y = 0;
            
            return count > 0 ? force / count : Vector3.zero;
        }
    }
}
