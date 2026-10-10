
using System.Collections.Generic;
using UnityEngine;

namespace MyProject.SwarmMovement
{
    public interface ISwarmMovable
    {
        public float MaxMoveSpeed { get; }
        public float MaxAcceleration { get; }
        public float MaxRotationSpeed { get; }
        
        public float CurrentMoveSpeed { get; set; }
        public Vector3 CurrentVelocity { get; set; }
        public float CurrentRotationSpeed { get; set; }
        public int CurrentTargetPositionIndex { get; set; }
        
        public Vector3 TargetPosition { get; set; }
        public float RetargetDistanceSqr { get; }
        
        public Vector3 Position { get; set; }
        public Vector3 Forward { get; set; }
        public Quaternion Rotation { get; set; }
        
        public LayerMask InfluenceMask { get; }
        public float InfluenceRadius { get; }
        
        public MovementForceConfig MovementForceConfig { get; }
        public List<ISwarmMovable> Neighbors { get; set; }

        public float SeparationDistanceSqr { get; }
        public float AlignmentDistanceSqr { get; }
        public float CohesionDistanceSqr { get; }
    }
}
