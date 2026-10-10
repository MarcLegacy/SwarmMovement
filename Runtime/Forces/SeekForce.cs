using UnityEngine;

namespace MyProject.SwarmMovement
{
    public class SeekForce : IMovementForce
    {
        #region Properties
        public MovementForceType MovementForceType => MovementForceType.Seek;
        #endregion
        
        #region Methods
        public Vector3 Calculate(ISwarmMovable swarmMovable)
        {
            var toTarget = swarmMovable.TargetPosition - swarmMovable.Position;
            toTarget.y = 0;

            return toTarget.sqrMagnitude > 0.01f ? toTarget.normalized : Vector3.zero;
        }
        #endregion
    }
}
