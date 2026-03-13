using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Experimental.Rendering;

[System.Serializable]
public class OutlineFromMaskCustomPass : CustomPass
{
    [Header("Mask (white = outlined)")]
    public LayerMask outlineLayer = 0;
    public Material maskMaterial;

    [Header("Outline (fullscreen)")]
    public Material outlineMaterial;
    [ColorUsage(true, true)] public Color outlineColor = Color.yellow;
    [Range(1, 6)] public int thickness = 2;
    [Range(0.0001f, 0.02f)] public float depthThreshold = 0.0025f;

    RTHandle maskRT;
    RTHandle sourceRT;
    RTHandle tempRT;

    protected override void Setup(ScriptableRenderContext renderContext, CommandBuffer cmd)
    {
        // Allocate once (guard against multiple Setup calls)
        if (maskRT == null)
        {
            maskRT = RTHandles.Alloc(
                Vector2.one,
                TextureXR.slices,
                dimension: TextureXR.dimension,
                colorFormat: GraphicsFormat.R8_UNorm,
                useDynamicScale: true,
                name: "_OutlineMaskRT"
            );
        }

        if (sourceRT == null)
        {
            sourceRT = RTHandles.Alloc(
                Vector2.one,
                TextureXR.slices,
                dimension: TextureXR.dimension,
                colorFormat: GraphicsFormat.R16G16B16A16_SFloat,
                useDynamicScale: true,
                name: "_OutlineSourceRT"
            );
        }

        if (tempRT == null)
        {
            tempRT = RTHandles.Alloc(
                Vector2.one,
                TextureXR.slices,
                dimension: TextureXR.dimension,
                colorFormat: GraphicsFormat.R16G16B16A16_SFloat,
                useDynamicScale: true,
                name: "_OutlineTempRT"
            );
        }
    }

    protected override void Execute(CustomPassContext ctx)
    {
        if (maskMaterial == null || outlineMaterial == null)
            return;

        // ---- 0) Copy current camera color to sourceRT (RTHandle-safe)
        HDUtils.BlitCameraTexture(ctx.cmd, ctx.cameraColorBuffer, sourceRT);

        // ---- 1) Build maskRT: clear black, draw outlineLayer objects white using maskMaterial
        CoreUtils.SetRenderTarget(ctx.cmd, maskRT, ClearFlag.Color, Color.black);

        int layerMask = outlineLayer.value;

        // Uses new API (no obsolete warning). If your Unity complains, switch back to FindObjectsOfType.
        var renderers = Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None);
        for (int i = 0; i < renderers.Length; i++)
        {
            var r = renderers[i];
            if (r == null || !r.enabled) continue;
            if (((1 << r.gameObject.layer) & layerMask) == 0) continue;

            // Draw with override mask material (white)
            ctx.cmd.DrawRenderer(r, maskMaterial);
        }

        // ---- 2) Bind explicit textures/params (we don't rely on HDRP internal names)
        outlineMaterial.SetTexture("_SourceTex", sourceRT);
        outlineMaterial.SetTexture("_OutlineMask", maskRT);
        outlineMaterial.SetTexture("_DepthTex", ctx.cameraDepthBuffer);

        outlineMaterial.SetColor("_OutlineColor", outlineColor);
        outlineMaterial.SetFloat("_Thickness", thickness);
        outlineMaterial.SetFloat("_DepthThreshold", depthThreshold);

        // ---- 3) Apply outline material into tempRT, then copy back to camera (double blit to avoid overwrite issues)
        HDUtils.BlitCameraTexture(ctx.cmd, sourceRT, tempRT, outlineMaterial, 0);
        HDUtils.BlitCameraTexture(ctx.cmd, tempRT, ctx.cameraColorBuffer);
    }

    protected override void Cleanup()
    {
        if (maskRT != null) { maskRT.Release(); maskRT = null; }
        if (sourceRT != null) { sourceRT.Release(); sourceRT = null; }
        if (tempRT != null) { tempRT.Release(); tempRT = null; }
    }
}