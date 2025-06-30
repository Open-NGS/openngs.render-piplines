using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class TexturePacker : EditorWindow
{
    [SerializeField]
    private VisualTreeAsset m_VisualTreeAsset = default;

    [MenuItem("Window/UI Toolkit/TexturePacker")]
    public static void ShowExample()
    {
        TexturePacker wnd = GetWindow<TexturePacker>();
        wnd.titleContent = new GUIContent("TexturePacker");
    }

    public void CreateGUI()
    {
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;
        // Instantiate UXML
        VisualElement labelFromUXML = m_VisualTreeAsset.Instantiate();
        root.Add(labelFromUXML);

        ObjectField fieldR = root.Q<ObjectField>("TextureR");
        MaskField maskR = root.Q<MaskField>("MaskR");
        ObjectField fieldG = root.Q<ObjectField>("TextureG");
        MaskField maskG = root.Q<MaskField>("MaskG");
        ObjectField fieldB = root.Q<ObjectField>("TextureB");
        MaskField maskB = root.Q<MaskField>("MaskB");
        ObjectField fieldA = root.Q<ObjectField>("TextureA");
        MaskField maskA = root.Q<MaskField>("MaskA");

        ObjectField fieldTarget = root.Q<ObjectField>("TextureTarget"); 

        Button btnR = root.Q<Button>("Pack");
        btnR.clicked += () =>
        {
            Texture2D textureR = null;
            Texture2D textureG = null;
            Texture2D textureB = null;
            Texture2D textureA = null;

            if (fieldR.value != null)
            {
                textureR = fieldR.value as Texture2D;
            }
            if (fieldG.value != null)
            {
                textureG = fieldG.value as Texture2D;
            }
            if (fieldB.value != null)
            {
                textureB = fieldB.value as Texture2D;
            }
            if (fieldA.value != null)
            {
                textureA = fieldA.value as Texture2D;
            }
            Debug.LogFormat("MASK({0},{1},{2},{3})", maskR.value, maskG.value, maskB.value, maskA.value);

            if (textureR != null)
            {

                Texture2D texture = fieldTarget!=null ? fieldTarget.value as Texture2D : new Texture2D(textureR.width, textureR.height, TextureFormat.RGBA32, true);

                for (int x = 0; x < texture.width; x++)
                {
                    for (int y = 0; y < texture.height; y++)
                    {

                        Color r = textureR != null ? textureR.GetPixel(x, y) : new Color(0, 0, 0, 0);
                        Color g = textureG != null ? textureG.GetPixel(x, y) : new Color(0, 0, 0, 0);
                        Color b = textureB != null ? textureB.GetPixel(x, y) : new Color(0, 0, 0, 0);
                        Color a = textureA != null ? textureA.GetPixel(x, y) : new Color(0, 0, 0, 0);
                        Color color = new Color(
                            maskR.value == 1 ? r.r : r.a,
                            maskG.value == 1 ? g.r : g.a,
                            maskB.value == 1 ? b.r : b.a,
                            maskA.value == 1 ? a.r : a.a
                            );
                        texture.SetPixel(x, y, color);
                    }
                }
                texture.Apply();

                //string path = AssetDatabase.GetAssetPath(texture);
                //string directory = System.IO.Path.GetDirectoryName(path);
                //string file = System.IO.Path.GetFileName(path);
                //string target = EditorUtility.SaveFilePanel("Save Texture", directory, "file", "png");
                //if (target.Length != 0)
                //{
                //    byte[] bytes = texture.EncodeToPNG();
                //    System.IO.File.WriteAllBytes(target, bytes);
                //    AssetDatabase.Refresh();
                //}


            }
            
        };


    }
}
