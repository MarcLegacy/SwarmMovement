using System.Collections.Generic;
using Core;
using UnityEngine;
using Core.MyMonoBehaviour;
using Systems.Globals;
using Random = UnityEngine.Random;

// Order of operations:
// 1. Calculate the combined movement direction based on all movement forces for the desired velocity. (Goal seeking, avoidance, flocking/formation, etc.)
// 2. Normalize desired velocity and scale to max speed.
// 3. Compute steering force based on desired velocity and current velocity. Clamp steering force to max acceleration/steering force.
// 4. Update current velocity based on steering force and delta time. (clamp to max speed)
// 5. Update position based on current velocity and delta time.
// 6. Update rotation based on current velocity and apply rotation speed to smoothly rotate towards the desired direction.
// 7. (Maybe add obstacle avoidance here, as this should be absolute)

namespace MyProject.SwarmMovement
{
    /// <summary>
    /// The SwarmMovementSystem class is responsible for managing the movement and rotation of swarm agents.
    /// It calculates movement forces, updates velocities, positions, and rotations for all registered swarm agents.
    /// </summary>
    public class SwarmMovementSystem : MyMonoBehaviour, IMyFixedUpdate
    {
        #region Fields
        #region Constants
        public const float MinSqr = 0.0001f;
        #endregion
        
        #region Serialized Fields
        [Header("Force Gizmos")]
        [SerializeField] private EDrawGizmosMode drawGizmosMode = EDrawGizmosMode.None;
        [SerializeField] private Color seekForceColor = Color.green;
        [SerializeField] private Color avoidanceForceColor = Color.red;
        [SerializeField] private Color separationForceColor = Color.yellow;
        [SerializeField] private Color alignmentForceColor = Color.blue;
        [SerializeField] private Color cohesionForceColor = Color.cyan;
        
        [Header("Target Positions")]
        [SerializeField] private List<Vector3> targetPositions = new();
        [SerializeField] private Color targetPositionColor = Color.red;
        #endregion
        
        #region Private Fields
        private readonly FastRemoveUniqueList<ISwarmMovable> _swarmMovables = new();
        private readonly UniqueList<IMovementForce> _movementForces = new();

        private readonly Collider[] _neighborHits = new Collider[128];
        #endregion
        #endregion
        
        #region Methods
        #region Unity Methods
        private void OnDrawGizmos()
        {
            DrawGizmosMovementForces(_swarmMovables, _movementForces);
        }

        private void OnDrawGizmosSelected()
        {
            DrawGizmosTargetPositions(targetPositions, targetPositionColor);
        }

        public override void MyAwake()
        {
            _movementForces.Add(new SeekForce());
            _movementForces.Add(new AvoidanceForce());
            _movementForces.Add(new SeparationForce());
            _movementForces.Add(new AlignmentForce());
            _movementForces.Add(new CohesionForce());
        }
        
        public void MyFixedUpdate()
        {
            var deltaTime = Time.fixedDeltaTime;
            
            for (var i = _swarmMovables.Count - 1; i >= 0; i--)
            {
                var swarmMovable = _swarmMovables[i];

                UpdateTarget(swarmMovable);
                UpdateNeighbors(swarmMovable);
                UpdateMovement(swarmMovable, deltaTime);
                UpdateRotation(swarmMovable, deltaTime);
            }
        }
        #endregion
        
        #region Public Methods
        public void Register(ISwarmMovable swarmMovable)
        {
            _swarmMovables.Add(swarmMovable);   // TODO: Get the interfaces from a Registry.
        }

        public void Unregister(ISwarmMovable swarmMovable)
        {
            _swarmMovables.Remove(swarmMovable);
        }
        #endregion

        #region Private Methods
        // TODO: Move this logic outside of this system.
        private void UpdateTarget(ISwarmMovable swarmMovable)
        {
            var currentPosition = swarmMovable.Position;
            var targetPosition = swarmMovable.TargetPosition;
            var toTarget = targetPosition - currentPosition;
            var distanceSqr = toTarget.sqrMagnitude;
            var currentTargetPositionIndex = swarmMovable.CurrentTargetPositionIndex;

            if (distanceSqr > swarmMovable.RetargetDistanceSqr) return;
            
            if (targetPositions.Count == 0) return;
            currentTargetPositionIndex = GetRandomOtherIndex(currentTargetPositionIndex, targetPositions.Count);
                    
            if (currentTargetPositionIndex >= 0 && currentTargetPositionIndex < targetPositions.Count)
            {
                swarmMovable.TargetPosition = targetPositions[currentTargetPositionIndex] + transform.position;
                swarmMovable.CurrentTargetPositionIndex = currentTargetPositionIndex;
            }
        }
        private void UpdateNeighbors(ISwarmMovable swarmMovable)
        {
            // TODO: Replace with a more efficient system to find neighbors, either Quadtree or grid-based. 
            swarmMovable.Neighbors = Utilities.Finder.FindNeighborsNonAlloc(swarmMovable, swarmMovable.Position, swarmMovable.InfluenceRadius, swarmMovable.InfluenceMask, QueryTriggerInteraction.Ignore, _neighborHits, swarmMovable.Neighbors);
        }
        
        private void UpdateMovement(ISwarmMovable swarmMovable, float deltaTime)
        {
            var currentVelocity = swarmMovable.CurrentVelocity;
            var maxAcceleration = swarmMovable.MaxAcceleration;
            
            var desiredVelocity = CalculateDesiredVelocity(swarmMovable);
            var steeringForce = CalculateSteeringForce(currentVelocity, desiredVelocity, maxAcceleration);
            
            swarmMovable.CurrentVelocity = Vector3.ClampMagnitude(currentVelocity + steeringForce * deltaTime, swarmMovable.MaxMoveSpeed);
            
            swarmMovable.CurrentMoveSpeed = swarmMovable.CurrentVelocity.magnitude; // Currently only used for debug info, but can be removed for optimization if needed.
            
            swarmMovable.Position += swarmMovable.CurrentVelocity * deltaTime;
        }
        
        private Vector3 CalculateDesiredVelocity(ISwarmMovable swarmMovable)
        {
            var desiredVelocity = Vector3.zero;
            
            foreach (var behavior in _movementForces)
            {
                if (swarmMovable.MovementForceConfig.forceWeights.TryGetValue(behavior.MovementForceType, out var forceWeight))
                {
                    if (forceWeight != 0.0f) // Don't calculate forces if not even applied.
                    {
                        desiredVelocity += behavior.Calculate(swarmMovable) * forceWeight;
                    }
                }
            }

            return desiredVelocity.normalized * swarmMovable.MaxMoveSpeed;
        }
        
        private Vector3 CalculateSteeringForce(Vector3 currentVelocity, Vector3 desiredVelocity, float maxAcceleration)
        {
            var steeringForce = desiredVelocity - currentVelocity;
            return Vector3.ClampMagnitude(steeringForce, maxAcceleration);
        }
        
        private void UpdateRotation(ISwarmMovable swarmMovable, float deltaTime)
        {
            var currentVelocity = swarmMovable.CurrentVelocity;
    
            if (currentVelocity.sqrMagnitude <= MinSqr) return;

            var desiredAngle = Mathf.Atan2(currentVelocity.x, currentVelocity.z) * Mathf.Rad2Deg;
            var currentAngle = swarmMovable.Rotation.eulerAngles.y;
            var newAngle =
                Mathf.MoveTowardsAngle(currentAngle, desiredAngle, swarmMovable.MaxRotationSpeed * deltaTime);
            
            var appliedRotation = Mathf.DeltaAngle(currentAngle, newAngle);
            swarmMovable.CurrentRotationSpeed = appliedRotation / deltaTime;  // Currently only used for debug info, but can be removed for optimization if needed.
    
            swarmMovable.Rotation = Quaternion.Euler(0, newAngle, 0);
        }

        private void DrawGizmosMovementForces(FastRemoveUniqueList<ISwarmMovable> swarmMovables,
            UniqueList<IMovementForce> movementForces)
        {
            if (drawGizmosMode == EDrawGizmosMode.None) return;

            GameObject selected = null;

            if (drawGizmosMode == EDrawGizmosMode.Selected)
            {
#if (UNITY_EDITOR)
                selected = UnityEditor.Selection.activeGameObject;
#endif
                if (selected == null) return;
            }

            foreach (var swarmMovable in swarmMovables)
            {
                if (drawGizmosMode == EDrawGizmosMode.Selected)
                {
                    if (swarmMovable is Component component)
                    {
                        if (component.gameObject != selected) continue;
                    }
                }

                foreach (var force in movementForces)
                {
                    if (!swarmMovable.MovementForceConfig.forceWeights.TryGetValue(force.MovementForceType,
                            out var weight) || weight == 0f) continue;

                    var forceVector = force.Calculate(swarmMovable) * weight;
                    Gizmos.color = GetMovementForceColor(force);
                    Gizmos.DrawLine(swarmMovable.Position, swarmMovable.Position + forceVector);
                }
            }
        }

        private Color GetMovementForceColor(IMovementForce movementForce)
        {
            return movementForce.MovementForceType switch
            {
                MovementForceType.Seek => seekForceColor,
                MovementForceType.Avoidance => avoidanceForceColor,
                MovementForceType.Separation => separationForceColor,
                MovementForceType.Alignment => alignmentForceColor,
                MovementForceType.Cohesion => cohesionForceColor,
                _ => Color.white
            };
        }
        
        private void DrawGizmosTargetPositions(List<Vector3> positions, Color color)
        {
            Gizmos.color = color;
            foreach (var position in positions)
            {
                Gizmos.DrawSphere(transform.position + position, 0.5f);
            }
        }

        private int GetRandomOtherIndex(int currentIndex, int count)
        {
            var newTargetPositionIndex = Random.Range(0, count);
            if (newTargetPositionIndex != currentIndex) return newTargetPositionIndex;
            
            if (newTargetPositionIndex == count - 1)
            {
                newTargetPositionIndex = 0;
            }
            else
            {
                newTargetPositionIndex++;
            }

            return newTargetPositionIndex;
        }
        #endregion
        #endregion
    }
}
