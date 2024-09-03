using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine.UI;
using System;
using static UnityEditor.BaseShaderGUI;
using System.Runtime.InteropServices;

namespace OpenNGS.RenderPipline.Editor
{
    public class MaterialTools
    {
        public enum RenderPiplineType
        {
            Builtin,
            URP,
            HDRP,
            NGS
        }


        public class MaterialInspector
        {
            public Material material;

            public MaterialInspector(Material material)
            {
                this.material = material;
                Process();
            }

            public string GetNGSShader()
            {
                StringBuilder shader = new StringBuilder();
                shader.Append("NGS/");
                if (this.IsParticle) shader.Append("Particles/");
                if(this.IsUnlit)
                    shader.Append("Unlit");
                else
                    shader.Append("Lit");
                return shader.ToString();
            }

            void Process()
            {
                var shader = material.shader;

                if (shader.name.Contains("Universal Render Pipeline")) PiplineType = RenderPiplineType.URP;
                else if (shader.name.Contains("HDRP")) PiplineType= RenderPiplineType.HDRP;
                else if (shader.name.Contains("NGS")) PiplineType= RenderPiplineType.NGS;
                else PiplineType = RenderPiplineType.Builtin;


                IsLit = shader.name.Contains("Lit", StringComparison.InvariantCulture);
                IsUnlit = shader.name.Contains("Unlit", StringComparison.InvariantCulture);

                IsParticle = shader.name.Contains("particles", StringComparison.InvariantCultureIgnoreCase);

                 if(shader.name.Contains("additive", StringComparison.InvariantCultureIgnoreCase))
                    BlendMode = BlendMode.Additive;
                 else if(shader.name.Contains("alpha", StringComparison.InvariantCultureIgnoreCase))
                    BlendMode = BlendMode.Alpha;
                else if (shader.name.Contains("multiply", StringComparison.InvariantCultureIgnoreCase))
                    BlendMode = BlendMode.Multiply;
            }


            public RenderPiplineType PiplineType { get;private set; }

            public bool IsParticle { get; private set; }

            public bool IsLit { get; private set; }
            public bool IsUnlit { get; private set; }

            public BlendMode BlendMode { get; private set; }
        }

        [MenuItem("Assets/Open NGS/Render Pipline/Convert To NGS Shader", isValidateFunction: true, priority: 0)]
        private static bool ConvertToNGSShadersValidation()
        {
            foreach (var guid in Selection.assetGUIDs)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                if (assetPath.EndsWith(".mat"))
                {
                    return true;
                }
                if (Directory.Exists(assetPath))
                {
                    return true;
                }
            }
            return false;
        }



        [MenuItem("Assets/Open NGS/Render Pipline/Convert To NGS Shader", priority = 0)]
        public static void ConvertToNGSShaders()
        {
            Debug.Log("Convert To NGS Shader==============" + Selection.count);

            List<string> targets = new List<string>();
            foreach (var guid in Selection.assetGUIDs)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                if (assetPath.EndsWith(".mat"))
                {
                    targets.Add(assetPath);
                }
                if (Directory.Exists(assetPath))
                {
                    targets.AddRange(Directory.GetFiles(assetPath, "*.mat", SearchOption.AllDirectories));
                }
            }
            int total = targets.Count;
            string name = "";
            for (int i = 0; i < total; i++)
            {
                string key = targets[i];
                Material mat = AssetDatabase.LoadAssetAtPath<Material>(key);
                EditorUtility.DisplayProgressBar("material Upgrade", "process " + key, i / total);
                if (mat != null)
                {
                    MaterialInspector inspector = new MaterialInspector(mat);
                    UpgradeToNGSShader(inspector);

                    switch (mat.shader.name)
                    {
                        case "Universal Render Pipeline/Lit":

                            UpgradeToNGS(mat, RenderPiplineType.URP, "NGS/Lit");
                            break;
                        case "Universal Render Pipeline/Unlit":
                            UpgradeToNGS(mat, RenderPiplineType.URP, "NGS/Unlit");
                            break;
                        case "Universal Render Pipeline/Particles/Lit":
                            UpgradeToNGS(mat, RenderPiplineType.URP, "NGS/Particles/Lit");
                            break;

                        case "Mobile/Particles/Additive":
                            UpgradeToNGS(mat, RenderPiplineType.URP, "NGS/Particles/Unlit", SurfaceType.Transparent, BlendMode.Additive);
                            break;
                        case "Mobile/Particles/Alpha Blended":
                            UpgradeToNGS(mat, RenderPiplineType.URP, "NGS/Particles/Unlit", SurfaceType.Transparent, BlendMode.Alpha);
                            break;
                        case "Universal Render Pipeline/Particles/Unlit":
                            UpgradeToNGS(mat, RenderPiplineType.URP, "NGS/Particles/Unlit");
                            break;
                        //case "HDRP/Lit":
                        //    UpgradeToNGS(mat, RenderPiplineType.HDRP, "NGS-Lit");
                        //    break;
                        //case "HDRP/Unlit":
                        //    UpgradeToNGS(mat, RenderPiplineType.HDRP, "NGS-Unlit");
                        //    break;
                        case "Shader Graphs/NGS-Lit":
                            UpgradeToNGS(mat, RenderPiplineType.NGS, "NGS-Lit");
                            break;
                        case "Shader Graphs/NGS-Unlit":
                            UpgradeToNGS(mat, RenderPiplineType.NGS, "NGS-Unlit");
                            break;
                        default:
                            UpgradeToNGS(mat, RenderPiplineType.NGS, "NGS-Lit");
                            break;
                    }
                }
            }
            EditorUtility.ClearProgressBar();
        }

        private static void UpgradeToNGSShader(MaterialInspector mat)
        {
            if (mat.IsParticle)
            {
                UpgradeToNGS(mat.material, mat.PiplineType, mat.GetNGSShader(), SurfaceType.Transparent, mat.BlendMode);
            }
            else
                UpgradeToNGS(mat.material, mat.PiplineType, mat.GetNGSShader());



        }

        private static void UpgradeToNGS(Material material, RenderPiplineType version, string name)
        {
            SurfaceType surfaceType = (SurfaceType)material.GetFloat("_Surface");
            BlendMode blendMode = (BlendMode)material.GetFloat("_Blend");
            UpgradeToNGS(material, version, name, surfaceType, blendMode);
        }

        private static void UpgradeToNGS(Material material, RenderPiplineType version, string name, SurfaceType surfaceType, BlendMode blendMode)
        {
            if (material.HasProperty("_EmissionColor"))
            {
                Color emissionColor = material.GetColor("_EmissionColor");
                material.SetColor("_Emission_Color", emissionColor);

            }
            bool alphaTest = material.IsKeywordEnabled("_ALPHATEST_ON");
            if(material.HasProperty("_AlphaClip"))
            {
                float clip = material.GetFloat("_AlphaClip");
                Debug.LogFormat("alphaTest:{0}, _AlphaClip:{1}", alphaTest, clip);
            }
            if (material.HasProperty("_AlphaToMask"))
            {
                float mask = material.GetFloat("_AlphaToMask");
                Debug.LogFormat("alphaTest:{0}, _AlphaToMask:{1}", alphaTest, mask);
            }
            material.shader = Shader.Find("Shader Graphs/" + name);

            material.SetFloat("_SurfaceType", (float)surfaceType);
            material.SetFloat("_Blend", (float)blendMode);
            

            if (material.HasProperty("_MainTex"))
            {
                material.SetTexture("_BaseMap", material.GetTexture("_MainTex"));
            }
            if (material.HasProperty("_BaseMap"))
            {
                material.SetTexture("_BaseMap", material.GetTexture("_BaseMap"));
            }


            if (alphaTest)
            {
                material.EnableKeyword("_ALPHATEST_ON");
                material.SetFloat("_AlphaCutoffEnable", 1f);
            }
            else
            {
                material.SetFloat("_AlphaCutoffEnable", 0f);
                material.DisableKeyword("_ALPHATEST_ON");
            }
            Debug.Log(string.Concat(material.shaderKeywords));

            if (surfaceType == SurfaceType.Opaque)
            {
                material.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");
            }
            else
            {
                material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            }

            AssetDatabase.SaveAssetIfDirty(material);
        }
    }
}