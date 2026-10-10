using UnityEngine;

namespace MyProject.SwarmMovement
{
    /// <summary>
    /// This will be used for agents who specifically need to avoid other type of agents.
    /// </summary>
    public class AvoidanceForce : IMovementForce
    {
        #region Private Fields
        private const float RayLength = 5f;
        private const float RayAngle = 30f;
        #endregion
        
        #region Properties
        public MovementForceType MovementForceType => MovementForceType.Avoidance;
        #endregion

        #region Methods
        #region Public Methods
        public Vector3 Calculate(ISwarmMovable swarmMovable)
        {
            var avoidance = Vector3.zero;
            var forward = swarmMovable.Forward;
            var position = swarmMovable.Position;

            // Center ray
            avoidance += CheckRay(position, forward, swarmMovable);

            // Side rays
            var rightDirection = Quaternion.Euler(0, RayAngle, 0) * forward;
            var leftDirection  = Quaternion.Euler(0, -RayAngle, 0) * forward;
            
            avoidance += CheckRay(position, rightDirection, swarmMovable) * 0.5f;
            avoidance += CheckRay(position, leftDirection, swarmMovable) * 0.5f;

            return avoidance.normalized;
        }
        #endregion
        
        #region Private Methods
        private Vector3 CheckRay(Vector3 origin, Vector3 direction, ISwarmMovable swarmMovable)
        {
            var hitSomething = Physics.Raycast(origin, direction, out var hit, RayLength, swarmMovable.InfluenceMask,
                QueryTriggerInteraction.Ignore);

#if UNITY_EDITOR
            // Debug.DrawLine(origin, hitSomething ? hit.point : origin + direction * RayLength,
            //     hitSomething ? Color.red : Color.green);
#endif

            if (!hitSomething) return Vector3.zero;
            
            // Steer away from the hit normal, stronger when closer
            var closeness = 1f - (hit.distance / RayLength);
            var steer = hit.normal * closeness;
            return steer;
        }
        #endregion
        #endregion
    }
}
