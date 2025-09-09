using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
// this was used on https://gamedevbill.com, but originally taken from https://cyangamedev.wordpress.com/2020/06/22/urp-post-processing/
// Saved in Blit.cs
public class PostBlit : ScriptableRendererFeature
{

    public class PostPass : ScriptableRenderPass
    {
        public enum RenderTarget
        {
            Color,
            RenderTexture,
        }
        public ScriptableRenderer renderer = null;
        public Material blitMaterial = null;
        public int blitShaderPassIndex = 0;
        public FilterMode filterMode { get; set; }

        private RenderTargetIdentifier source { get; set; }
        private RenderTargetHandle destination { get; set; }
        
        RenderTargetHandle m_TemporaryColorTexture;
        string m_ProfilerTag;

        public PostPass(RenderPassEvent renderPassEvent, Material blitMaterial, int blitShaderPassIndex, string tag)
        {
            this.renderPassEvent = renderPassEvent;
            this.blitMaterial = blitMaterial;
            this.blitShaderPassIndex = blitShaderPassIndex;
            m_ProfilerTag = tag;
            m_TemporaryColorTexture.Init("_TemporaryColorTexture");
        }
        // 获取active的CPPs下标，并返回是否存在有效组件
        public bool SetupCustomPostProcessing()
        {
            return true;
        }


        public void Setup(ScriptableRenderer renderer, Target destination, RenderTargetHandle renderTextureHandle)
        {
            this.renderer = renderer;
            this.destination = (destination == Target.Color) ? RenderTargetHandle.CameraTarget : renderTextureHandle;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            this.source = renderer.cameraColorTargetHandle;
            if (this.blitMaterial == null)
            {
                Debug.LogWarningFormat("Missing Blit Material. {0} blit pass will not execute. Check for missing reference in the assigned renderer.", GetType().Name);
                return;
            }
            

            CommandBuffer cmd = CommandBufferPool.Get(m_ProfilerTag);

            RenderTextureDescriptor opaqueDesc = renderingData.cameraData.cameraTargetDescriptor;
            opaqueDesc.depthBufferBits = 0;

            // Can't read and write to same color target, use a TemporaryRT
            if (destination == RenderTargetHandle.CameraTarget)
            {
                cmd.GetTemporaryRT(m_TemporaryColorTexture.id, opaqueDesc, filterMode);
                Blit(cmd, source, m_TemporaryColorTexture.Identifier(), blitMaterial, blitShaderPassIndex);
                Blit(cmd, m_TemporaryColorTexture.Identifier(), source);
            }
            else
            {
                Blit(cmd, source, destination.Identifier(), blitMaterial, blitShaderPassIndex);
            }

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void FrameCleanup(CommandBuffer cmd)
        {
            if (destination == RenderTargetHandle.CameraTarget)
                cmd.ReleaseTemporaryRT(m_TemporaryColorTexture.id);
        }
    }

    [System.Serializable]
    public class BlitSettings
    {
        public RenderPassEvent Event = RenderPassEvent.AfterRenderingOpaques;

        public Material blitMaterial = null;
        public int blitMaterialPassIndex = 0;
        public Target destination = Target.Color;
        public string textureId = "_BlitPassTexture";
    }

    public enum Target
    {
        Color,
        Texture
    }

    public BlitSettings settings = new BlitSettings();
    RenderTargetHandle m_RenderTextureHandle;

    PostPass blitPass;

    public override void Create()
    {
        var passIndex = settings.blitMaterial != null ? settings.blitMaterial.passCount - 1 : 1;
        settings.blitMaterialPassIndex = Mathf.Clamp(settings.blitMaterialPassIndex, -1, passIndex);
        blitPass = new PostPass(settings.Event, settings.blitMaterial, settings.blitMaterialPassIndex, name);
        m_RenderTextureHandle.Init(settings.textureId);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        blitPass.Setup(renderer, settings.destination, m_RenderTextureHandle);
        renderer.EnqueuePass(blitPass);
    }
}