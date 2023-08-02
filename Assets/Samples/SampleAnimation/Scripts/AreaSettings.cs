using Prototypes.Core.ECS.Configs;
using System;
using UnityEngine;

namespace Prototypes.SamplesBRG.AnimationSample
{
    [Serializable]
    public sealed class AreaSettings
    {
        [field: SerializeField] public Material AreaMaterial { get; private set; }
        [field: SerializeField, Range(10, 5000)] public int AreaResolution { get; private set; } = 2000;
        [field: SerializeField] public EntityConfigAsset[] Configs { get; private set; }
    }
}
