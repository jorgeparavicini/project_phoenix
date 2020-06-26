using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class FCVShaderChanger : EditorWindow
{

    [MenuItem("Window/FCV Pipeline changer")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(FCVShaderChanger));
    }

    public void OnGUI()
    {
        GUILayout.Label("Change pipeline to:");

        if (GUILayout.Button("Standard RP"))
        {
            FindShaders();
            ChangeToSRP();
        }
        if (GUILayout.Button("(Universal)LWRP"))
        {
            FindShaders();
            ChangeToLWRP();
        }
        if (GUILayout.Button("HDRP"))
        {
            FindShaders();
            ChangeToHDRP();
        }
    }

    Shader Add_CG;
    Shader Blend_CG;
    Shader Blood;
    Shader Blend_Tornado;

    Shader Add_CG_LWRP;
    Shader Blend_CG_LWRP;
    Shader Blood_LWRP;
    Shader Tornado_LWRP;

    Shader Add_CG_HDRP;
    Shader Blend_CG_HDRP;
    Shader Blood_HDRP;
    Shader Tornado_HDRP;

    Material[] shaderMaterials;

    private void FindShaders()
    {
        if (Shader.Find("Hovl/Particles/Add_CenterGlow") != null) Add_CG = Shader.Find("Hovl/Particles/Add_CenterGlow");
        if (Shader.Find("Hovl/Particles/Blend_CenterGlow") != null) Blend_CG = Shader.Find("Hovl/Particles/Blend_CenterGlow");
        if (Shader.Find("Hovl/Particles/Blood") != null) Blood = Shader.Find("Hovl/Particles/Blood");
        if (Shader.Find("Hovl/Particles/Blend_Tornado") != null) Blend_Tornado = Shader.Find("Hovl/Particles/Blend_Tornado");

        if (Shader.Find("Shader Graphs/LWRP_Add_CG") != null) Add_CG_LWRP = Shader.Find("Shader Graphs/LWRP_Add_CG");
        if (Shader.Find("Shader Graphs/LWRP_Blend_CG") != null) Blend_CG_LWRP = Shader.Find("Shader Graphs/LWRP_Blend_CG");
        if (Shader.Find("Shader Graphs/LWRP_Blood") != null) Blood_LWRP = Shader.Find("Shader Graphs/LWRP_Blood");
        if (Shader.Find("Shader Graphs/LWRP_Tornado") != null) Tornado_LWRP = Shader.Find("Shader Graphs/LWRP_Tornado");

        if (Shader.Find("Shader Graphs/HDRP_Add_CG") != null) Add_CG_HDRP = Shader.Find("Shader Graphs/HDRP_Add_CG");
        if (Shader.Find("Shader Graphs/HDRP_Blend_CG") != null) Blend_CG_HDRP = Shader.Find("Shader Graphs/HDRP_Blend_CG");
        if (Shader.Find("Shader Graphs/HDRP_Blood") != null) Blood_HDRP = Shader.Find("Shader Graphs/HDRP_Blood");
        if (Shader.Find("Shader Graphs/HDRP_Tornado") != null) Tornado_HDRP = Shader.Find("Shader Graphs/HDRP_Tornado");

        string[] folderMat = AssetDatabase.FindAssets("t:Material", new[] { "Assets/Hovl Studio" });
        shaderMaterials = new Material[folderMat.Length];

        for (int i = 0; i < folderMat.Length; i++)
        {
            var patch = AssetDatabase.GUIDToAssetPath(folderMat[i]);
            shaderMaterials[i] = (Material)AssetDatabase.LoadAssetAtPath(patch, typeof(Material));
        }
    }

    private void ChangeToLWRP()
    {

        foreach (var material in shaderMaterials)
        {
            if (Shader.Find("Shader Graphs/LWRP_Blood") != null)
            {
                if (material.shader == Blood || material.shader == Blood_HDRP)
                {
                    if (material.GetTextureScale("_MainTex") != null)
                    {
                        Vector2 MainScale = material.GetTextureScale("_MainTex");
                        Vector2 MainOffset = material.GetTextureOffset("_MainTex");
                        material.shader = Blood_LWRP;
                        if (material.GetVector("_MainTexTiling") != null)
                            material.SetVector("_MainTexTiling", new Vector4(MainScale[0], MainScale[1], MainOffset[0], MainOffset[1]));
                    }
                    else
                        material.shader = Blood_LWRP;
                }
            }
            /*----------------------------------------------------------------------------------------------------*/
            if (Shader.Find("Shader Graphs/LWRP_Tornado") != null)
            {
                if (material.shader == Blend_Tornado || material.shader == Tornado_HDRP)
                {
                    if (material.GetTextureScale("_MainTex") != null)
                    {
                        Vector2 MainScale = material.GetTextureScale("_MainTex");
                        Vector2 MainOffset = material.GetTextureOffset("_MainTex");
                        material.shader = Tornado_LWRP;
                        if (material.GetVector("_MainTexTiling") != null)
                            material.SetVector("_MainTexTiling", new Vector4(MainScale[0], MainScale[1], MainOffset[0], MainOffset[1])); 
                    }
                    else
                        material.shader = Tornado_LWRP;
                }
            }
            /*----------------------------------------------------------------------------------------------------*/
            if (Shader.Find("Shader Graphs/LWRP_Add_CG") != null)
            {
                if (material.shader == Add_CG || material.shader == Add_CG_HDRP)
                {
                    if (material.GetTextureScale("_MainTex") != null || material.GetTextureScale("_Noise") != null
                        || material.GetTextureScale("_Flow") != null || material.GetTextureScale("_Mask") != null)
                    {
                        Vector2 MainScale = material.GetTextureScale("_MainTex");
                        Vector2 MainOffset = material.GetTextureOffset("_MainTex");
                        Vector2 NoiseScale = material.GetTextureScale("_Noise");
                        Vector2 NoiseOffset = material.GetTextureOffset("_Noise");
                        Vector2 FlowScale = material.GetTextureScale("_Flow");
                        Vector2 FlowOffset = material.GetTextureOffset("_Flow");
                        Vector2 MaskScale = material.GetTextureScale("_Mask");
                        Vector2 MaskOffset = material.GetTextureOffset("_Mask");
                        material.shader = Add_CG_LWRP;
                        if (material.HasProperty("_ZWrite")) material.SetFloat("_ZWrite", 0);
                        if (material.HasProperty("_MainTexTiling"))
                            material.SetVector("_MainTexTiling", new Vector4(MainScale[0], MainScale[1], MainOffset[0], MainOffset[1]));
                        if (material.HasProperty("_NoiseTiling"))
                            material.SetVector("_NoiseTiling", new Vector4(NoiseScale[0], NoiseScale[1], NoiseOffset[0], NoiseOffset[1]));
                        if (material.HasProperty("_FlowTiling"))
                            material.SetVector("_FlowTiling", new Vector4(FlowScale[0], FlowScale[1], FlowOffset[0], FlowOffset[1]));
                        if (material.HasProperty("_MaskTiling"))
                            material.SetVector("_MaskTiling", new Vector4(MaskScale[0], MaskScale[1], MaskOffset[0], MaskOffset[1]));
                    }
                    else
                        material.shader = Add_CG_LWRP;
                }
            }
            /*----------------------------------------------------------------------------------------------------*/
            if (Shader.Find("Shader Graphs/LWRP_Blend_CG") != null)
            {
                if (material.shader == Blend_CG || material.shader == Blend_CG_HDRP)
                {
                    if (material.GetTextureScale("_MainTex") != null || material.GetTextureScale("_Noise") != null
                        || material.GetTextureScale("_Flow") != null || material.GetTextureScale("_Mask") != null)
                    {
                        Vector2 MainScale = material.GetTextureScale("_MainTex");
                        Vector2 MainOffset = material.GetTextureOffset("_MainTex");
                        Vector2 NoiseScale = material.GetTextureScale("_Noise");
                        Vector2 NoiseOffset = material.GetTextureOffset("_Noise");
                        Vector2 FlowScale = material.GetTextureScale("_Flow");
                        Vector2 FlowOffset = material.GetTextureOffset("_Flow");
                        Vector2 MaskScale = material.GetTextureScale("_Mask");
                        Vector2 MaskOffset = material.GetTextureOffset("_Mask");
                        material.shader = Blend_CG_LWRP;
                        if (material.HasProperty("_ZWrite")) material.SetFloat("_ZWrite", 0);
                        if (material.HasProperty("_MainTexTiling"))
                            material.SetVector("_MainTexTiling", new Vector4(MainScale[0], MainScale[1], MainOffset[0], MainOffset[1]));
                        if (material.HasProperty("_NoiseTiling"))
                            material.SetVector("_NoiseTiling", new Vector4(NoiseScale[0], NoiseScale[1], NoiseOffset[0], NoiseOffset[1]));
                        if (material.HasProperty("_FlowTiling"))
                            material.SetVector("_FlowTiling", new Vector4(FlowScale[0], FlowScale[1], FlowOffset[0], FlowOffset[1]));
                        if (material.HasProperty("_MaskTiling"))
                            material.SetVector("_MaskTiling", new Vector4(MaskScale[0], MaskScale[1], MaskOffset[0], MaskOffset[1]));
                    }
                    else
                        material.shader = Blend_CG_LWRP;
                }
            }
        }
    }


    private void ChangeToSRP()
    {

        foreach (var material in shaderMaterials)
        {
            if (Shader.Find("Hovl/Particles/Blood") != null)
            {
                if (material.shader == Blood_LWRP || material.shader == Blood_HDRP)
                {
                    if (material.GetVector("_MainTexTiling") != null)
                    {
                        Vector4 MainTiling = material.GetVector("_MainTexTiling");
                        material.shader = Blood;
                        if (material.GetTextureScale("_MainTex") != null)
                        {
                            material.SetTextureScale("_MainTex", new Vector2(MainTiling[0], MainTiling[1]));
                            material.SetTextureOffset("_MainTex", new Vector2(MainTiling[2], MainTiling[3]));
                        }
                    }
                    else
                        material.shader = Blood;
                }
            }
            /*----------------------------------------------------------------------------------------------------*/
            if (Shader.Find("Hovl/Particles/Blend_Tornado") != null)
            {
                if (material.shader == Tornado_LWRP || material.shader == Tornado_HDRP)
                {
                    if (material.GetVector("_MainTexTiling") != null)
                    {
                        Vector4 MainTiling = material.GetVector("_MainTexTiling");
                        material.shader = Blend_Tornado;
                        if (material.GetTextureScale("_MainTex") != null)
                        {
                            material.SetTextureScale("_MainTex", new Vector2(MainTiling[0], MainTiling[1]));
                            material.SetTextureOffset("_MainTex", new Vector2(MainTiling[2], MainTiling[3]));
                        }
                    }
                    else
                        material.shader = Blend_Tornado;
                }
            }
            /*----------------------------------------------------------------------------------------------------*/
            if (Shader.Find("Hovl/Particles/Add_CenterGlow") != null)
            {
                if (material.shader == Add_CG_LWRP || material.shader == Add_CG_HDRP)
                {
                    if (material.HasProperty("_MainTexTiling") && material.HasProperty("_NoiseTiling")
                        && material.HasProperty("_FlowTiling") && material.HasProperty("_MaskTiling"))
                    {
                        Vector4 MainTiling = material.GetVector("_MainTexTiling");
                        Vector4 NoiseTiling = material.GetVector("_NoiseTiling");
                        Vector4 FlowTiling = material.GetVector("_FlowTiling");
                        Vector4 MaskTiling = material.GetVector("_MaskTiling");
                        material.shader = Add_CG;
                        if (material.GetTextureScale("_MainTex") != null && material.GetTextureScale("_Noise") != null)
                        {
                            material.SetTextureScale("_MainTex", new Vector2(MainTiling[0], MainTiling[1]));
                            material.SetTextureOffset("_MainTex", new Vector2(MainTiling[2], MainTiling[3]));
                            material.SetTextureScale("_Noise", new Vector2(NoiseTiling[0], NoiseTiling[1]));
                            material.SetTextureOffset("_Noise", new Vector2(NoiseTiling[2], NoiseTiling[3]));
                            material.SetTextureScale("_Flow", new Vector2(FlowTiling[0], FlowTiling[1]));
                            material.SetTextureOffset("_Flow", new Vector2(FlowTiling[2], FlowTiling[3]));
                            material.SetTextureScale("_Mask", new Vector2(MaskTiling[0], MaskTiling[1]));
                            material.SetTextureOffset("_Mask", new Vector2(MaskTiling[2], MaskTiling[3]));
                        }
                    }
                    else
                        material.shader = Add_CG;
                }
            }
            /*----------------------------------------------------------------------------------------------------*/
            if (Shader.Find("Hovl/Particles/Blend_CenterGlow") != null)
            {
                if (material.shader == Blend_CG_LWRP || material.shader == Blend_CG_HDRP)
                {
                    if (material.HasProperty("_MainTexTiling") && material.HasProperty("_NoiseTiling")
                        && material.HasProperty("_FlowTiling") && material.HasProperty("_MaskTiling"))
                    {
                        Vector4 MainTiling = material.GetVector("_MainTexTiling");
                        Vector4 NoiseTiling = material.GetVector("_NoiseTiling");
                        Vector4 FlowTiling = material.GetVector("_FlowTiling");
                        Vector4 MaskTiling = material.GetVector("_MaskTiling");
                        material.shader = Blend_CG;
                        if (material.GetTextureScale("_MainTex") != null && material.GetTextureScale("_Noise") != null)
                        {
                            material.SetTextureScale("_MainTex", new Vector2(MainTiling[0], MainTiling[1]));
                            material.SetTextureOffset("_MainTex", new Vector2(MainTiling[2], MainTiling[3]));
                            material.SetTextureScale("_Noise", new Vector2(NoiseTiling[0], NoiseTiling[1]));
                            material.SetTextureOffset("_Noise", new Vector2(NoiseTiling[2], NoiseTiling[3]));
                            material.SetTextureScale("_Flow", new Vector2(FlowTiling[0], FlowTiling[1]));
                            material.SetTextureOffset("_Flow", new Vector2(FlowTiling[2], FlowTiling[3]));
                            material.SetTextureScale("_Mask", new Vector2(MaskTiling[0], MaskTiling[1]));
                            material.SetTextureOffset("_Mask", new Vector2(MaskTiling[2], MaskTiling[3]));
                        }
                    }
                    else
                        material.shader = Blend_CG;
                }
            }
        }
    }

    private void ChangeToHDRP()
    {
        foreach (var material in shaderMaterials)
        {
            if (Shader.Find("Shader Graphs/HDRP_Blood") != null)
            {
                if (material.shader == Blood || material.shader == Blood_LWRP)
                {
                    if (material.GetTextureScale("_MainTex") != null)
                    {
                        Vector2 MainScale = material.GetTextureScale("_MainTex");
                        Vector2 MainOffset = material.GetTextureOffset("_MainTex");
                        material.shader = Blood_HDRP;
                        if (material.GetVector("_MainTexTiling") != null)
                            material.SetVector("_MainTexTiling", new Vector4(MainScale[0], MainScale[1], MainOffset[0], MainOffset[1]));
                    }
                    else
                        material.shader = Blood_HDRP;
                }
            }
            /*----------------------------------------------------------------------------------------------------*/
            if (Shader.Find("Shader Graphs/HDRP_Tornado") != null)
            {
                if (material.shader == Blend_Tornado || material.shader == Tornado_LWRP)
                {
                    if (material.GetTextureScale("_MainTex") != null)
                    {
                        Vector2 MainScale = material.GetTextureScale("_MainTex");
                        Vector2 MainOffset = material.GetTextureOffset("_MainTex");
                        material.shader = Tornado_HDRP;
                        if (material.GetVector("_MainTexTiling") != null)
                            material.SetVector("_MainTexTiling", new Vector4(MainScale[0], MainScale[1], MainOffset[0], MainOffset[1]));
                    }
                    else
                        material.shader = Tornado_HDRP;
                }
            }
            /*----------------------------------------------------------------------------------------------------*/
            if (Shader.Find("Shader Graphs/HDRP_Add_CG") != null)
            {
                if (material.shader == Add_CG || material.shader == Add_CG_LWRP)
                {
                    if (material.GetTextureScale("_MainTex") != null || material.GetTextureScale("_Noise") != null
                        || material.GetTextureScale("_Flow") != null || material.GetTextureScale("_Mask") != null)
                    {
                        Vector2 MainScale = material.GetTextureScale("_MainTex");
                        Vector2 MainOffset = material.GetTextureOffset("_MainTex");
                        Vector2 NoiseScale = material.GetTextureScale("_Noise");
                        Vector2 NoiseOffset = material.GetTextureOffset("_Noise");
                        Vector2 FlowScale = material.GetTextureScale("_Flow");
                        Vector2 FlowOffset = material.GetTextureOffset("_Flow");
                        Vector2 MaskScale = material.GetTextureScale("_Mask");
                        Vector2 MaskOffset = material.GetTextureOffset("_Mask");
                        material.SetFloat("_StencilRef", 0);
                        material.SetFloat("_AlphaDstBlend", 1);
                        material.SetFloat("_DstBlend", 1);
                        material.SetFloat("_ZWrite", 0);
                        material.SetFloat("_SrcBlend", 1);
                        material.EnableKeyword("_BLENDMODE_ADD _DOUBLESIDED_ON _SURFACE_TYPE_TRANSPARENT");
                        material.SetShaderPassEnabled("TransparentBackface", false);
                        material.SetOverrideTag("RenderType", "Transparent");
                        material.SetFloat("_CullModeForward", 0);
                        material.shader = Add_CG_HDRP;
                        if (material.HasProperty("_MainTexTiling"))
                            material.SetVector("_MainTexTiling", new Vector4(MainScale[0], MainScale[1], MainOffset[0], MainOffset[1]));
                        if (material.HasProperty("_NoiseTiling"))
                            material.SetVector("_NoiseTiling", new Vector4(NoiseScale[0], NoiseScale[1], NoiseOffset[0], NoiseOffset[1]));
                        if (material.HasProperty("_FlowTiling"))
                            material.SetVector("_FlowTiling", new Vector4(FlowScale[0], FlowScale[1], FlowOffset[0], FlowOffset[1]));
                        if (material.HasProperty("_MaskTiling"))
                            material.SetVector("_MaskTiling", new Vector4(MaskScale[0], MaskScale[1], MaskOffset[0], MaskOffset[1]));
                    }
                    else
                        material.shader = Add_CG_HDRP;
                }
            }
            /*----------------------------------------------------------------------------------------------------*/
            if (Shader.Find("Shader Graphs/HDRP_Blend_CG") != null)
            {
                if (material.shader == Blend_CG || material.shader == Blend_CG_LWRP)
                {
                    if (material.GetTextureScale("_MainTex") != null || material.GetTextureScale("_Noise") != null
                        || material.GetTextureScale("_Flow") != null || material.GetTextureScale("_Mask") != null)
                    {
                        Vector2 MainScale = material.GetTextureScale("_MainTex");
                        Vector2 MainOffset = material.GetTextureOffset("_MainTex");
                        Vector2 NoiseScale = material.GetTextureScale("_Noise");
                        Vector2 NoiseOffset = material.GetTextureOffset("_Noise");
                        Vector2 FlowScale = material.GetTextureScale("_Flow");
                        Vector2 FlowOffset = material.GetTextureOffset("_Flow");
                        Vector2 MaskScale = material.GetTextureScale("_Mask");
                        Vector2 MaskOffset = material.GetTextureOffset("_Mask");
                        material.SetFloat("_ZWrite", 0);
                        material.SetFloat("_StencilRef", 0);
                        material.SetShaderPassEnabled("TransparentBackface", false);
                        material.SetOverrideTag("RenderType", "Transparent");
                        material.SetFloat("_AlphaDstBlend", 10);
                        material.SetFloat("_DstBlend", 10);
                        material.SetFloat("_SrcBlend", 1);
                        material.EnableKeyword("_BLENDMODE_ALPHA _DOUBLESIDED_ON _SURFACE_TYPE_TRANSPARENT");
                        if (material.HasProperty("_CullModeForward")) material.SetFloat("_CullModeForward", 0);
                        material.shader = Blend_CG_HDRP;
                        if (material.HasProperty("_MainTexTiling"))
                            material.SetVector("_MainTexTiling", new Vector4(MainScale[0], MainScale[1], MainOffset[0], MainOffset[1]));
                        if (material.HasProperty("_NoiseTiling"))
                            material.SetVector("_NoiseTiling", new Vector4(NoiseScale[0], NoiseScale[1], NoiseOffset[0], NoiseOffset[1]));
                        if (material.HasProperty("_FlowTiling"))
                            material.SetVector("_FlowTiling", new Vector4(FlowScale[0], FlowScale[1], FlowOffset[0], FlowOffset[1]));
                        if (material.HasProperty("_MaskTiling"))
                            material.SetVector("_MaskTiling", new Vector4(MaskScale[0], MaskScale[1], MaskOffset[0], MaskOffset[1]));
                    }
                    else
                        material.shader = Blend_CG_HDRP;
                }
            }
        }
    }
}