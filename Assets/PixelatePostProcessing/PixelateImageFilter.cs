using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using System;

[Serializable, VolumeComponentMenu("Post-processing/Custom/PixelateImageFilter")]
public sealed class PixelateImageFilter : CustomPostProcessVolumeComponent, IPostProcessComponent
{
    [Tooltip("Controls the intensity of the effect.")]
    public ClampedIntParameter BlockSize = new ClampedIntParameter(5, 2, 20);
    public BoolParameter ShowInSceneView = new BoolParameter(false, false);
    public ComputeShader FilterComputeShader;

    public bool IsActive() => FilterComputeShader != null;
    
    public override bool visibleInSceneView => ShowInSceneView.value;

    public override CustomPostProcessInjectionPoint injectionPoint => CustomPostProcessInjectionPoint.AfterPostProcess;

    public override void Setup()
    {
        if (FilterComputeShader == null) 
            FilterComputeShader = Resources.Load<ComputeShader>("Pixelate");
    }

    public override void Render(CommandBuffer cmd, HDCamera camera, RTHandle source, RTHandle destination)
    {
        var mainKernel = FilterComputeShader.FindKernel("Pixelate");
        FilterComputeShader.GetKernelThreadGroupSizes(mainKernel, out uint xGroupSize, out uint yGroupSize, out _);
        cmd.SetComputeTextureParam(FilterComputeShader, mainKernel, "_ImageFilterSource", source.nameID);
        cmd.SetComputeTextureParam(FilterComputeShader, mainKernel, "_ImageFilterResult", destination.nameID);
        cmd.SetComputeIntParam(FilterComputeShader, "_BlockSize", BlockSize.value);
        cmd.SetComputeIntParam(FilterComputeShader, "_ResultWidth", destination.rt.width);
        cmd.SetComputeIntParam(FilterComputeShader, "_ResultHeight", destination.rt.height);
        cmd.DispatchCompute(FilterComputeShader, mainKernel,
            Mathf.CeilToInt(destination.rt.width / (float) BlockSize.value / xGroupSize),
            Mathf.CeilToInt(destination.rt.height / (float) BlockSize.value / yGroupSize),
            1);
    }

    public override void Cleanup()
    {
    }
}