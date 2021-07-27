using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using System;

[Serializable, VolumeComponentMenu("Post-processing/Custom/PixelateImageFilter")]
public sealed class PixelateImageFilter : CustomPostProcessVolumeComponent, IPostProcessComponent
{
    [Tooltip("Controls the intensity of the effect.")]
    public ClampedIntParameter blockSize = new ClampedIntParameter(5, 2, 20);

    public ComputeShader m_FilterComputeShader;

    const string _kernelName = "Pixelate";
    const string _resultRenderTargetName = "_ImageFilterResult";
    int _resultRenderTargetId = Shader.PropertyToID(_resultRenderTargetName);
    RenderTargetIdentifier _resultRenderTextureIdentifier;

    public bool IsActive() => m_FilterComputeShader != null;
    public override bool visibleInSceneView => false;

    // Do not forget to add this post process in the Custom Post Process Orders list (Project Settings > HDRP Default Settings).
    public override CustomPostProcessInjectionPoint injectionPoint => CustomPostProcessInjectionPoint.AfterPostProcess;

    public override void Setup()
    {
        m_FilterComputeShader = Resources.Load<ComputeShader>("Pixelate");
        //    AssetDatabase.LoadAssetAtPath<ComputeShader>("Assets/PixelatePostProcessing/Pixelate.compute");
    }

    public override void Render(CommandBuffer cmd, HDCamera camera, RTHandle source, RTHandle destination)
    {
        var rtDescriptor = source.rt.descriptor;
        rtDescriptor.enableRandomWrite = true;
        cmd.GetTemporaryRT(_resultRenderTargetId, rtDescriptor);
        var mainKernel = m_FilterComputeShader.FindKernel(_kernelName);
        m_FilterComputeShader.GetKernelThreadGroupSizes(mainKernel, out uint xGroupSize, out uint yGroupSize, out _);
        cmd.Blit(source, _resultRenderTargetId);
        cmd.SetComputeTextureParam(m_FilterComputeShader, mainKernel, _resultRenderTargetId,
            _resultRenderTextureIdentifier);
        cmd.SetComputeIntParam(m_FilterComputeShader, "_BlockSize", blockSize.value);
        cmd.SetComputeIntParam(m_FilterComputeShader, "_ResultWidth", source.rt.width);
        cmd.SetComputeIntParam(m_FilterComputeShader, "_ResultHeight", source.rt.height);
        cmd.DispatchCompute(m_FilterComputeShader, mainKernel,
            Mathf.CeilToInt(source.rt.width / (float) blockSize.value / xGroupSize),
            Mathf.CeilToInt(source.rt.height / (float) blockSize.value / yGroupSize),
            1);
        cmd.Blit(_resultRenderTextureIdentifier, destination);
        cmd.ReleaseTemporaryRT(_resultRenderTargetId);
    }

    public override void Cleanup()
    {
    }
}