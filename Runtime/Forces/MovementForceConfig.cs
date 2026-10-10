using System.Collections.Generic;
using UnityEngine;

namespace MyProject.SwarmMovement
{
    [CreateAssetMenu(fileName = "MovementForceConfig", menuName = "SO/SwarmMovement/MovementForceConfig")]
    public class MovementForceConfig : ScriptableObject
    {
        [SerializeField] public Dictionary<MovementForceType, float> forceWeights = new();
    }
}
