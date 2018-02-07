using UnityEngine;
using UnityEditor;

namespace AltSystems.AltTrees.Editor
{
    public class ImportTreesFromSceneWindow : EditorWindow
    {
        static AltTrees at;
        static AltTreesDataLinks dataLinks = null;

        public static void CreateWindow(AltTrees _at)
        {
            at = _at;

            if (_at.altTreesManagerData == null)
                CreateAltTreesManagerData();

            getDataLinks();

            if (_at.dataLinksCorrupted)
                return;

            ImportTreesFromSceneWindow w = (ImportTreesFromSceneWindow)EditorWindow.GetWindow(typeof(ImportTreesFromSceneWindow), true, "Import trees from scene");
            w.minSize = new Vector2(500, 600);
            w.maxSize = new Vector2(500, 600);
            AltSystems.AltBackup.Editor.CenterOnMainEditorWindow.CenterOnMainWin(w);

            idTreeSelected = 0;
            countTrees = 0;
            countTreesTemp = 0;

            if (dataLinks.altTrees != null)
            {
                for (int i = 0; i < dataLinks.altTrees.Length; i++)
                {
                    if (dataLinks.altTrees[i] != null)
                        countTrees++;
                }
            }


            trees = new AltTree[countTrees];
            treeIdsTemp = new int[countTrees];

            if (dataLinks.altTrees != null)
            {
                for (int i = 0; i < dataLinks.altTrees.Length && countTreesTemp < countTrees; i++)
                {
                    if (dataLinks.altTrees[i] != null)
                    {
                        trees[countTreesTemp] = dataLinks.altTrees[i];
                        treeIdsTemp[countTreesTemp] = dataLinks.altTrees[i].id;
                        countTreesTemp++;
                    }
                }
            }
            
            textures = new GUIContent[countTrees];
            for (int i = 0; i < countTrees; i++)
            {
                textures[i] = new GUIContent();
                if (trees[i] != null)
                {
                    textures[i].image = AssetPreview.GetAssetPreview(trees[i].gameObject);
                    textures[i].text = trees[i].name;
                }
            }

            style = new GUIStyle();
            style.padding = new RectOffset(2, 2, 2, 2);
            style.imagePosition = ImagePosition.ImageAbove;

            style.clipping = TextClipping.Clip;
            style.alignment = TextAnchor.UpperCenter;
            style.fontSize = 9;

            style2 = new GUIStyle();
            style2.fontStyle = FontStyle.Bold;

            Texture2D textureBackground;
            Color32 cvet = new Color32(20, 97, 225, 255);
            textureBackground = new Texture2D(2, 2);
            textureBackground.SetPixels32(new Color32[] { cvet, cvet, cvet, cvet });
            textureBackground.hideFlags = HideFlags.HideAndDontSave;
            textureBackground.Apply();

            style.onNormal.background = textureBackground;
        }

        static int idTreeSelected = 0;
        static int countTrees = 0;
        static int countTreesTemp = 0;
        static GUIContent[] textures;
        static AltTree[] trees;
        static GUIStyle style;
        static GUIStyle style2;
        static int[] treeIdsTemp;
        static Vector2 scroll = new Vector2();

        bool getYrotationFromTransform = true;
        bool randomRotation = true;

        bool getScaleFromTransform = true;
        bool isRandomHeight = true;
        float height = 0.3f;
        float heightRandom = 1.0f;
        bool lockWidthToHeight = true;
        bool isRandomWidth = true;
        float width = 0.3f;
        float widthRandom = 1.0f;
        bool isRandomHueLeaves = true;
        bool isRandomHueBark = true;
        bool hue = true;
        Color hueColorLeaves = new Color32(255, 0, 0, 80);
        Color hueColorBark = new Color32(0, 0, 0, 100);
        Color noHue = new Color32(0, 0, 0, 0);

        void OnGUI()
        {
            if (at == null)
                return;

            scroll = GUILayout.BeginScrollView(scroll);
            {
                GUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    GUILayout.Label("Select Prototype:", style2);
                    GUILayout.Space(5);
                    idTreeSelected = GUI.SelectionGrid(GetBrushAspectRect(countTrees, 64, 12), idTreeSelected, textures, (int)Mathf.Ceil((float)((Screen.width - 20) / 64)), style);
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndScrollView();

            GUILayout.Space(10);

            int countSelection = 0;
            
            for(int i = 0; i < Selection.gameObjects.Length; i++)
            {
                if (Selection.gameObjects[i].activeSelf && Selection.gameObjects[i].GetComponent<AltTrees>() == null)
                    countSelection++;
            }

            GUILayout.BeginVertical(EditorStyles.helpBox);
            {
                GUILayout.Label("Select Objects from the Scene.", style2);
                GUILayout.Label("Selected objects: " + countSelection);
                GUILayout.Space(5);

                GUILayout.Label("Settings:", style2);

                GUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    getYrotationFromTransform = EditorGUILayout.Toggle("Rotation from Transform: ", getYrotationFromTransform, GUILayout.Width(300));
                    if (getYrotationFromTransform)
                        GUI.enabled = false;
                    randomRotation = EditorGUILayout.Toggle("Random Y Rotation: ", randomRotation, GUILayout.Width(300));
                    GUI.enabled = true;
                }
                GUILayout.EndVertical();


                GUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    getScaleFromTransform = EditorGUILayout.Toggle("Scale from Transform: ", getScaleFromTransform, GUILayout.Width(300));
                    if (getScaleFromTransform)
                        GUI.enabled = false;

                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.Label("Tree Height:", GUILayout.Width(EditorGUIUtility.labelWidth - 6f));


                        GUILayout.Label("Random?");
                        isRandomHeight = GUILayout.Toggle(isRandomHeight, "");

                        if (isRandomHeight)
                        {
                            float heightTemp = height;
                            float heightRandomTemp = height + heightRandom;
                            EditorGUILayout.MinMaxSlider(ref heightTemp, ref heightRandomTemp, 0.1f, 2f);

                            if (heightTemp != height || heightRandom != heightRandomTemp)
                            {
                                height = heightTemp;
                                heightRandom = heightRandomTemp - heightTemp;
                            }

                            GUILayout.Label(heightTemp.ToString("0.0") + " - " + heightRandomTemp.ToString("0.0"));
                        }
                        else
                        {
                            height = (float)System.Math.Round(EditorGUILayout.Slider(height, 0.1f, 2f), 2);
                        }
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.Space(7);

                    lockWidthToHeight = EditorGUILayout.Toggle("Lock Width to Height: ", lockWidthToHeight);

                    GUILayout.Space(7);

                    if (lockWidthToHeight)
                        GUI.enabled = false;

                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.Label("Tree Width:", GUILayout.Width(EditorGUIUtility.labelWidth - 6f));


                        GUILayout.Label("Random?");
                        isRandomWidth = GUILayout.Toggle(isRandomWidth, "");

                        if (isRandomWidth)
                        {
                            float widthTemp = width;
                            float widthRandomTemp = width + widthRandom;
                            EditorGUILayout.MinMaxSlider(ref widthTemp, ref widthRandomTemp, 0.1f, 2f);

                            if (widthTemp != width || widthRandom != widthRandomTemp)
                            {
                                width = widthTemp;
                                widthRandom = widthRandomTemp - widthTemp;
                            }

                            GUILayout.Label(widthTemp.ToString("0.0") + " - " + widthRandomTemp.ToString("0.0"));
                        }
                        else
                        {
                            width = (float)System.Math.Round(EditorGUILayout.Slider(width, 0.1f, 2f), 2);
                        }
                    }
                    GUILayout.EndHorizontal();

                    GUI.enabled = true;
                }
                GUILayout.EndVertical();


                GUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    hue = EditorGUILayout.Toggle("Hue: ", hue, GUILayout.Width(300));
                    if (!hue)
                        GUI.enabled = false;
                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.Label("Hue Leaves:", GUILayout.Width(EditorGUIUtility.labelWidth - 6f));


                        GUILayout.Label("Random(from alpha color)?");
                        isRandomHueLeaves = GUILayout.Toggle(isRandomHueLeaves, "");

                        hueColorLeaves = EditorGUILayout.ColorField(hueColorLeaves);
                    }
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.Label("Hue Bark:", GUILayout.Width(EditorGUIUtility.labelWidth - 6f));


                        GUILayout.Label("Random(from alpha color)?");
                        isRandomHueBark = GUILayout.Toggle(isRandomHueBark, "");

                        hueColorBark = EditorGUILayout.ColorField(hueColorBark);
                    }
                    GUILayout.EndHorizontal();
                    GUI.enabled = true;
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndVertical();

            if (Selection.gameObjects.Length == 0)
                GUI.enabled = false;

            
            if (GUILayout.Button("Import", GUILayout.Height(30)))
            {
                if (countSelection != 0)
                {
                    GameObject goParent = new GameObject("ImportedObjects");
                    goParent.transform.position = Vector3.zero;
                    goParent.transform.rotation = Quaternion.identity;
                    goParent.SetActive(false);

                    AddTreesStruct[] ats = new AddTreesStruct[countSelection];
                    int h = 0;
                    float _height;
                    float _width;

                    for (int i = 0; i < Selection.gameObjects.Length; i++)
                    {
                        if (Selection.gameObjects[i].activeSelf && Selection.gameObjects[i].GetComponent<AltTrees>() == null)
                        {
                            _height = (getScaleFromTransform ? Selection.gameObjects[i].transform.localScale.y : (isRandomHeight ? height + Random.value * (heightRandom - height) : height));
                            _width = (getScaleFromTransform ? Selection.gameObjects[i].transform.localScale.x : (lockWidthToHeight ? _height : (isRandomWidth ? width + Random.value * (widthRandom - width) : width)));

                            ats[h] = new AddTreesStruct(Selection.gameObjects[i].transform.position, treeIdsTemp[idTreeSelected], dataLinks.getAltTree(treeIdsTemp[idTreeSelected]).isObject, (getYrotationFromTransform ? Selection.gameObjects[i].transform.localRotation.eulerAngles.y : (randomRotation ? Random.value * 360f : 0f)), _height, _width, (hue ? (isRandomHueLeaves ? new Color(hueColorLeaves.r, hueColorLeaves.g, hueColorLeaves.b, Random.value * hueColorLeaves.a) : hueColorLeaves) : noHue), (hue ? (isRandomHueBark ? new Color(hueColorBark.r, hueColorBark.g, hueColorBark.b, Random.value * hueColorBark.a) : hueColorBark) : noHue));
                            Selection.gameObjects[i].transform.parent = goParent.transform;
                            h++;
                        }
                    }
                    at.addTrees(ats, true);

                    Selection.activeObject = null;
                    Close();
                }
            }

            
            GUI.enabled = true;
            GUILayout.Space(10);
        }


        public void OnInspectorUpdate()
        {
            Repaint();
        }




        Rect GetBrushAspectRect(int elementCount, int approxSize, int extraLineHeight)
        {
            int num1 = (int)Mathf.Ceil((float)((Screen.width - 20) / approxSize));
            int num2 = elementCount / num1;
            if (elementCount % num1 != 0)
                ++num2;
            Rect aspectRect = GUILayoutUtility.GetAspectRect((float)num1 / (float)num2);
            Rect rect = GUILayoutUtility.GetRect(10f, (float)(extraLineHeight * num2));
            aspectRect.height += rect.height;
            return aspectRect;
        }

        static void CreateAltTreesManagerData()
        {
            if (!System.IO.Directory.Exists("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + at.getIdManager()))
            {
                System.IO.Directory.CreateDirectory("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + at.getIdManager());
            }

            if (!System.IO.File.Exists("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + at.getIdManager() + "/altTreesManagerData.asset"))
            {
                AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<AltTreesManagerData>(), "Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + at.getIdManager() + "/altTreesManagerData.asset");
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            at.altTreesManagerData = (AltTreesManagerData)AssetDatabase.LoadAssetAtPath("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + at.getIdManager() + "/altTreesManagerData.asset", typeof(AltTreesManagerData));
            EditorUtility.SetDirty(at);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        
        static void getDataLinks()
        {
            if (dataLinks == null)
            {
                if (!System.IO.Directory.Exists("Assets/Plugins/AltSystems/AltTrees/DataBase"))
                {
                    System.IO.Directory.CreateDirectory("Assets/Plugins/AltSystems/AltTrees/DataBase");
                }

                if (!System.IO.File.Exists("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesDataLinks.asset"))
                {
                    AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<AltTreesDataLinks>(), "Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesDataLinks.asset");
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }

                dataLinks = (AltTreesDataLinks)AssetDatabase.LoadAssetAtPath("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesDataLinks.asset", typeof(AltTreesDataLinks));

                if (dataLinks == null)
                {
                    at.dataLinksIsCorrupted();
                }
            }
        }

    }
}