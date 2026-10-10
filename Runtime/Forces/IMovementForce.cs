using UnityEngine;

namespace MyProject.SwarmMovement
{
    public interface IMovementForce
    {
        public MovementForceType MovementForceType { get; }
        
        /// <summary>
        /// Returns a vector representing the force to be applied to the swarmMovable.
        /// The force returned is a direction and length that should be applied to the swarmMovable's current velocity.
        /// </summary>
        public Vector3 Calculate(ISwarmMovable swarmMovable);
    }
    
    public enum MovementForceType
    {
        Seek,
        Avoidance,
        Separation,
        Alignment,
        Cohesion,
    }
}
