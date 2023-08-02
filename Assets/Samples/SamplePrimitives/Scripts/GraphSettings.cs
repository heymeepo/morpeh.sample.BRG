using Prototypes.Core.ECS.Configs;
using System;
using UnityEngine;

namespace Prototypes.SamplesBRG.PrimitivesSample
{
    [Serializable]
    public sealed class GraphSettings
    {
        public int ResolutionMin { get; private set; } = 10;
        [field: SerializeField] public int ResolutionMax { get; private set; } = 2048;
        [field: SerializeField] public EntityConfigAsset Config { get; private set; }
    }
}
