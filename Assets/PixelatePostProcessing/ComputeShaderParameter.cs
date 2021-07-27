using UnityEngine;
using UnityEngine.Rendering;

public class ComputeShaderParameter : VolumeParameter<ComputeShader>
{
    public ComputeShaderParameter(ComputeShader value, bool overrideState = false)
        : base(value, overrideState)
    {
    }
}