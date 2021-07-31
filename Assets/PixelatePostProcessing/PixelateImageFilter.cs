using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using System;

[Serializable, VolumeComponentMenu("Post-processing/Custom/PixelateImageFilter")]
public sealed class PixelateImageFilter : CustomPostProcessVolumeComponent, IPostProcessComponent
{
    public ClampedIntParameter BlockSize = new ClampedIntParameter(5, 2, 20);
    public BoolParameter ShowInSceneView = new BoolParameter(false);
    public ComputeShaderParameter FilterComputeShaderParameter = new ComputeShaderParameter(null);
    
    public bool IsActive() => FilterComputeShaderParameter.value != null;

    public override bool visibleInSceneView => ShowInSceneView.value;

    public override CustomPostProcessInjectionPoint injectionPoint => CustomPostProcessInjectionPoint.AfterPostProcess;

    public override void Render(CommandBuffer cmd, HDCamera camera, RTHandle source, RTHandle destination)
    {
        var filterComputeShader = FilterComputeShaderParameter.value;
        var mainKernel = filterComputeShader.FindKernel("Pixelate");
        filterComputeShader.GetKernelThreadGroupSizes(mainKernel, out uint xGroupSize, out uint yGroupSize, out _);
        cmd.SetComputeTextureParam(filterComputeShader, mainKernel, "_ImageFilterSource", source.nameID);
        cmd.SetComputeTextureParam(filterComputeShader, mainKernel, "_ImageFilterResult", destination.nameID);
        cmd.SetComputeIntParam(filterComputeShader, "_BlockSize", BlockSize.value);
        cmd.SetComputeIntParam(filterComputeShader, "_ResultWidth", destination.rt.width);
        cmd.SetComputeIntParam(filterComputeShader, "_ResultHeight", destination.rt.height);
        cmd.DispatchCompute(filterComputeShader, mainKernel,
            Mathf.CeilToInt(destination.rt.width / (float) BlockSize.value / xGroupSize),
            Mathf.CeilToInt(destination.rt.height / (float) BlockSize.value / yGroupSize),
            1);
    }
}