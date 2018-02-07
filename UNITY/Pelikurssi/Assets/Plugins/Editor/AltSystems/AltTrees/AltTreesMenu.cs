using UnityEngine;
using UnityEditor;
using AltSystems.AltBackup.Editor;
using AltSystems.AltBackup;

namespace AltSystems.AltTrees.Editor
{
    public class AltTreesMenu : EditorWindow
    {
        [MenuItem("Window/AltSystems/AltTrees/Create AltTrees", false, 1)] static void Create ()
	    {
            GameObject go = new GameObject("AltTrees", typeof(AltTrees));
		    go.transform.position = Vector3.zero;
		    go.transform.rotation = Quaternion.identity;


            Terrain terr = (Terrain)Transform.FindObjectOfType(typeof(Terrain));
            float size = 0f;
            int degree = 0;

            if (terr != null)
                size = Mathf.Clamp(Mathf.Max(Mathf.Floor(terr.terrainData.size.x), Mathf.Floor(terr.terrainData.size.z)), 100, 10000);
            else
                size = 1000f;

            for (int d = 1; d < 16; d++)
            {
                if (size / Mathf.Pow(2f, (float)d) <= 160)
                {
                    degree = d + 1;
                    break;
                }
            }

            if (go.GetComponent<AltTrees>().altTreesManagerData == null)
            {
                go.GetComponent<AltTrees>().CreateAltTreesManagerData();
            }

            go.GetComponent<AltTrees>().altTreesManagerData.sizePatch = (int)size;
            go.GetComponent<AltTrees>().altTreesManagerData.maxLOD = degree;

            go.GetComponent<AltTrees>().altTreesManagerData.crossFadeDependenceOnSpeed = true;
            go.GetComponent<AltTrees>().altTreesManagerData.crossFadeDependenceOnSpeedMaxCoefficient = 1;
            go.GetComponent<AltTrees>().altTreesManagerData.crossFadeDependenceOnSpeedMaxSpeed = 50;
            go.GetComponent<AltTrees>().altTreesManagerData.checkTreesDependenceOnSpeed = true;
            go.GetComponent<AltTrees>().altTreesManagerData.checkTreesDependenceOnSpeedMaxCoefficient = 25;
            go.GetComponent<AltTrees>().altTreesManagerData.checkTreesDependenceOnSpeedMaxSpeed = 50;
            go.GetComponent<AltTrees>().altTreesManagerData.crossFadeTimeBillboard = 0.2f;
            go.GetComponent<AltTrees>().altTreesManagerData.crossFadeTimeMesh = 0.2f;
            go.GetComponent<AltTrees>().altTreesManagerData.checkTreesPerFramePercent = 5;

            int idTemp = go.GetComponent<AltTrees>().getIdManager();

            if (idTemp == -1)
            {
                if (!System.IO.Directory.Exists("Assets/Plugins/AltSystems/AltTrees/DataBase"))
                {
                    System.IO.Directory.CreateDirectory("Assets/Plugins/AltSystems/AltTrees/DataBase");
                }

                if (!System.IO.Directory.Exists("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData"))
                {
                    System.IO.Directory.CreateDirectory("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData");
                }

                while (System.IO.Directory.Exists("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + idTemp))
                {
                    idTemp = Random.Range(100000000, 999999999);
                }

                System.IO.Directory.CreateDirectory("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + idTemp);

                go.GetComponent<AltTrees>().setIdManager(idTemp);
            }

            if (!System.IO.File.Exists("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesDataLinks.asset"))
		    {
                AssetDatabase.CreateAsset(CreateInstance<AltTreesDataLinks>(), "Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesDataLinks.asset");
			
			    AssetDatabase.SaveAssets ();
			    AssetDatabase.Refresh();
		    }

		    Undo.RegisterCreatedObjectUndo (go, "Create AltTrees");

		    Selection.activeGameObject = go;
        }

        [MenuItem("GameObject/AltSystems/AltTrees", false)] static void Create2()
        {
            Create();
        }

        [MenuItem("Window/AltSystems/AltTrees/Create Wind", false, 2)] static void CreateWind()
        {
            GameObject go = new GameObject("AltWind", typeof(AltWind));
            go.transform.position = Vector3.zero;
            go.transform.rotation = Quaternion.identity;

            Undo.RegisterCreatedObjectUndo(go, "Create AltWind");

            Selection.activeGameObject = go;
        }



        [MenuItem("Window/AltSystems/AltTrees/Shaders Configurator", false, 3)]
        static void ShadersConfigurator()
        {
            ShadersConfiguratorWindow.CreateWindow();
        }


        [MenuItem("GameObject/AltSystems/Wind", false)] static void CreateWind2()
        {
            CreateWind();
        }

        [MenuItem("Window/AltSystems/AltTrees/Documentation", false, 51)]
        static void Documentation()
        {
            Application.OpenURL("http://altsystems-unity.net/AltTrees/Documentation/");
        }

        [MenuItem("Window/AltSystems/AltTrees/API", false, 52)]
        static void API()
        {
            Application.OpenURL("http://altsystems-unity.net/AltTrees/API/");
        }


        [MenuItem("Window/AltSystems/AltTrees/About...", false, 53)]
        static void About()
        {

            AboutAltTrees w = (AboutAltTrees)EditorWindow.GetWindow(typeof(AboutAltTrees), true, "About...");
            w.minSize = new Vector2(300, 200);
            w.maxSize = new Vector2(300, 200);
            CenterOnMainEditorWindow.CenterOnMainWin(w);
        }

    }
}