using UnityEngine;

namespace MyProject.SwarmMovement
{
    public class AlignmentForce : IMovementForce
    {
        public MovementForceType MovementForceType => MovementForceType.Alignment;

        public Vector3 Calculate(ISwarmMovable swarmMovable)
        {
            var averageVelocity = Vector3.zero;
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
                    distanceSqr > swarmMovable.AlignmentDistanceSqr) continue;
                
                averageVelocity += neighbor.CurrentVelocity;
                count++;
            }

            averageVelocity.y = 0;
            
            return count > 0 ? averageVelocity / count : Vector3.zero;
        }
    }
}
