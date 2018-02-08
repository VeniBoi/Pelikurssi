using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace AltSystems.AltTrees.Editor
{
    public class ShadersConfiguratorWindow : EditorWindow
    {
        static int hueNum = 0;
        static Dictionary<string, string> dict = null;
        static bool isInit = false;

        public static void CreateWindow(bool createWindow = true)
        {
            if (createWindow)
            {
                ShadersConfiguratorWindow w = (ShadersConfiguratorWindow)EditorWindow.GetWindow(typeof(ShadersConfiguratorWindow), true, "Shaders Configurator");
                w.minSize = new Vector2(300, 200);
                w.maxSize = new Vector2(300, 200);
                AltSystems.AltBackup.Editor.CenterOnMainEditorWindow.CenterOnMainWin(w);
            }

            dict = AltTrees.getConfigShaders();

            foreach (var item in dict)
            {
                if(item.Key.Equals("hue"))
                {
                    if (item.Value.Equals("high"))
                        hueNum = 0;
                    else if (item.Value.Equals("simple"))
                        hueNum = 1;
                    else if(item.Value.Equals("off"))
                        hueNum = 2;
                }
            }

            isInit = true;
        }



        void OnGUI()
        {
            if(!isInit)
            {
                CreateWindow(false);
                return;
            }

            GUIStyle sty = new GUIStyle();
            sty.alignment = TextAnchor.MiddleLeft;
            sty.fontSize = 12;

            GUI.Label(new Rect(20, 20, 500, 25), "Hue:");
            hueNum = EditorGUI.Popup(new Rect(60, 20, 150, 25), hueNum, new string[3] { "High", "Simple", "Off" });

            if(GUI.Button(new Rect(50, 165, 200, 25), "Save"))
            {
                string pathHueTemp = "";

                if (hueNum == 0)
                    pathHueTemp = "Assets/Plugins/AltSystems/AltTrees/Shaders/Library/Configurator/HueHigh.cgincTemp";
                else if (hueNum == 1)
                    pathHueTemp = "Assets/Plugins/AltSystems/AltTrees/Shaders/Library/Configurator/HueSimple.cgincTemp";
                else if (hueNum == 2)
                    pathHueTemp = "Assets/Plugins/AltSystems/AltTrees/Shaders/Library/Configurator/HueOff.cgincTemp";

                string str = "";
                using (System.IO.StreamReader sr = new System.IO.StreamReader(pathHueTemp))
                {
                    str = sr.ReadToEnd();
                }
                using (System.IO.StreamWriter sw = new System.IO.StreamWriter("Assets/Plugins/AltSystems/AltTrees/Shaders/Library/Configurator/Hue.cginc", false, System.Text.Encoding.UTF8))
                {
                    sw.Write(str);
                }


                str = "";
                bool isFirst = true;
                foreach (var item in dict)
                {
                    if (!isFirst)
                        str += ";";
                    isFirst = false;

                    if (!item.Key.Equals("hue"))
                        str += item.Key + ":" + item.Value;
                    else
                    {
                        str += item.Key + ":";
                        if (hueNum == 0)
                            str += "high";
                        else if (hueNum == 1)
                            str += "simple";
                        else if(hueNum == 2)
                            str += "off";
                    }
                }

                using (System.IO.StreamWriter sw = new System.IO.StreamWriter("Assets/Plugins/AltSystems/AltTrees/Shaders/Library/Configurator/config.AltConf", false, System.Text.Encoding.UTF8))
                {
                    sw.Write(str);
                }
                
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                
                if (AltTrees.altTreesVersionUnity == 550 || AltTrees.altTreesVersionUnity == 560)
                {
                    EditorUtility.DisplayProgressBar("Compiling Shaders (1 / 16)...", "Compiling Shaders... ", 1f / 16f);
                    AssetDatabase.ImportAsset("Assets/Plugins/AltSystems/AltTrees/Shaders/BillboardGroupAltTree.shader");
                    EditorUtility.DisplayProgressBar("Compiling Shaders (2 / 16)...", "Compiling Shaders... ", 2f / 16f);
                    AssetDatabase.ImportAsset("Assets/Plugins/AltSystems/AltTrees/Shaders/BarkAltTree.shader");
                    EditorUtility.DisplayProgressBar("Compiling Shaders (3 / 16)...", "Compiling Shaders... ", 3f / 16f);
                    AssetDatabase.ImportAsset("Assets/Plugins/AltSystems/AltTrees/Shaders/BarkBumpAltTree.shader");
                    EditorUtility.DisplayProgressBar("Compiling Shaders (4 / 16)...", "Compiling Shaders... ", 4f / 16f);
                    AssetDatabase.ImportAsset("Assets/Plugins/AltSystems/AltTrees/Shaders/BillboardAltTree.shader");
                    EditorUtility.DisplayProgressBar("Compiling Shaders (5 / 16)...", "Compiling Shaders... ", 5f / 16f);
                    AssetDatabase.ImportAsset("Assets/Plugins/AltSystems/AltTrees/Shaders/SpeedTreeAltTree.shader");
                    EditorUtility.DisplayProgressBar("Compiling Shaders (6 / 16)...", "Compiling Shaders... ", 6f / 16f);
                    AssetDatabase.ImportAsset("Assets/Plugins/AltSystems/AltTrees/Shaders/LeavesAltTree.shader");
                    EditorUtility.DisplayProgressBar("Compiling Shaders (7 / 16)...", "Compiling Shaders... ", 7f / 16f);
                    AssetDatabase.ImportAsset("Assets/Plugins/AltSystems/AltTrees/Shaders/LeavesBumpAltTree.shader");
                    EditorUtility.DisplayProgressBar("Compiling Shaders (8 / 16)...", "Compiling Shaders... ", 8f / 16f);
                    AssetDatabase.ImportAsset("Assets/Plugins/AltSystems/AltTrees/Shaders/TreeCreatorBarkAltTree.shader");
                    EditorUtility.DisplayProgressBar("Compiling Shaders (9 / 16)...", "Compiling Shaders... ", 9f / 16f);
                    AssetDatabase.ImportAsset("Assets/Plugins/AltSystems/AltTrees/Shaders/TreeCreatorLeavesAltTree.shader");

                    EditorUtility.DisplayProgressBar("Compiling Shaders (10 / 16)...", "Compiling Shaders... ", 10f / 16f);
                    AssetDatabase.ImportAsset("Assets/Plugins/AltSystems/AltTrees/Shaders/SpeedTreeAltTreeInstanced.shader");
                    EditorUtility.DisplayProgressBar("Compiling Shaders (11 / 16)...", "Compiling Shaders... ", 11f / 16f);
                    AssetDatabase.ImportAsset("Assets/Plugins/AltSystems/AltTrees/Shaders/BarkAltTreeInstanced.shader");
                    EditorUtility.DisplayProgressBar("Compiling Shaders (12 / 16)...", "Compiling Shaders... ", 12f / 16f);
                    AssetDatabase.ImportAsset("Assets/Plugins/AltSystems/AltTrees/Shaders/BarkBumpAltTreeInstanced.shader");
                    EditorUtility.DisplayProgressBar("Compiling Shaders (13 / 16)...", "Compiling Shaders... ", 13f / 16f);
                    AssetDatabase.ImportAsset("Assets/Plugins/AltSystems/AltTrees/Shaders/LeavesAltTreeInstanced.shader");
                    EditorUtility.DisplayProgressBar("Compiling Shaders (14 / 16)...", "Compiling Shaders... ", 14f / 16f);
                    AssetDatabase.ImportAsset("Assets/Plugins/AltSystems/AltTrees/Shaders/LeavesBumpAltTreeInstanced.shader");
                    EditorUtility.DisplayProgressBar("Compiling Shaders (15 / 16)...", "Compiling Shaders... ", 15f / 16f);
                    AssetDatabase.ImportAsset("Assets/Plugins/AltSystems/AltTrees/Shaders/TreeCreatorBarkAltTreeInstanced.shader");
                    EditorUtility.DisplayProgressBar("Compiling Shaders (16 / 16)...", "Compiling Shaders... ", 16f / 16f);
                    AssetDatabase.ImportAsset("Assets/Plugins/AltSystems/AltTrees/Shaders/TreeCreatorLeavesAltTreeInstanced.shader");
                    EditorUtility.ClearProgressBar();
                }
                else
                {
                    EditorUtility.DisplayProgressBar("Compiling Shaders (1 / 9)...", "Compiling Shaders... ", 1f / 9f);
                    AssetDatabase.ImportAsset("Assets/Plugins/AltSystems/AltTrees/Shaders/BillboardGroupAltTree.shader");
                    EditorUtility.DisplayProgressBar("Compiling Shaders (2 / 9)...", "Compiling Shaders... ", 2f / 9f);
                    AssetDatabase.ImportAsset("Assets/Plugins/AltSystems/AltTrees/Shaders/BarkAltTree.shader");
                    EditorUtility.DisplayProgressBar("Compiling Shaders (3 / 9)...", "Compiling Shaders... ", 3f / 9f);
                    AssetDatabase.ImportAsset("Assets/Plugins/AltSystems/AltTrees/Shaders/BarkBumpAltTree.shader");
                    EditorUtility.DisplayProgressBar("Compiling Shaders (4 / 9)...", "Compiling Shaders... ", 4f / 9f);
                    AssetDatabase.ImportAsset("Assets/Plugins/AltSystems/AltTrees/Shaders/BillboardAltTree.shader");
                    EditorUtility.DisplayProgressBar("Compiling Shaders (5 / 9)...", "Compiling Shaders... ", 5f / 9f);
                    AssetDatabase.ImportAsset("Assets/Plugins/AltSystems/AltTrees/Shaders/SpeedTreeAltTree.shader");
                    EditorUtility.DisplayProgressBar("Compiling Shaders (6 / 9)...", "Compiling Shaders... ", 6f / 9f);
                    AssetDatabase.ImportAsset("Assets/Plugins/AltSystems/AltTrees/Shaders/LeavesAltTree.shader");
                    EditorUtility.DisplayProgressBar("Compiling Shaders (7 / 9)...", "Compiling Shaders... ", 7f / 9f);
                    AssetDatabase.ImportAsset("Assets/Plugins/AltSystems/AltTrees/Shaders/LeavesBumpAltTree.shader");
                    EditorUtility.DisplayProgressBar("Compiling Shaders (8 / 9)...", "Compiling Shaders... ", 8f / 9f);
                    AssetDatabase.ImportAsset("Assets/Plugins/AltSystems/AltTrees/Shaders/TreeCreatorBarkAltTree.shader");
                    EditorUtility.DisplayProgressBar("Compiling Shaders (9 / 9)...", "Compiling Shaders... ", 9f / 9f);
                    AssetDatabase.ImportAsset("Assets/Plugins/AltSystems/AltTrees/Shaders/TreeCreatorLeavesAltTree.shader");
                    EditorUtility.ClearProgressBar();
                }

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                AltTrees[] ats = FindObjectsOfType(typeof(AltTrees)) as AltTrees[];
                foreach (AltTrees at in ats)
                {
                    at.reInitTimer = 10;
                }
            }
        }
    }
}