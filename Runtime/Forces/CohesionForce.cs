using UnityEngine;

namespace MyProject.SwarmMovement
{
    public class CohesionForce : IMovementForce
    {
        public MovementForceType MovementForceType => MovementForceType.Cohesion;

        public Vector3 Calculate(ISwarmMovable swarmMovable)
        {
            var center = Vector3.zero;
            var count = 0;
            var position = swarmMovable.Position;
            position.y = 0;
            
            for (var i = 0; i < swarmMovable.Neighbors.Count; i++)
            {
                var neighbor = swarmMovable.Neighbors[i];
                var toNeighbor = neighbor.Position - position;
                toNeighbor.y = 0;

                var distanceSqr = toNeighbor.sqrMagnitude;
                if (distanceSqr < SwarmMovementSystem.MinSqr ||
                    distanceSqr > swarmMovable.CohesionDistanceSqr) continue;
                
                center += neighbor.Position;
                count++;
            }

            center.y = 0;
            
            return count > 0 ? center / count - position : Vector3.zero;
        }
    }
}
