using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using System;
using System.Reflection;
using System.IO;

namespace AltSystems.AltTrees.Editor
{
    [CustomEditor(typeof(AltTrees))]
    public class AltTrees_Editor : UnityEditor.Editor
    {
        static public string version = "0.9.8.1";
        static public int altTreesVersionUnity = 560;

        int currentUnityVer = 0;

        AltTreesDataLinks dataLinks = null;
        int sizePatchTemp = 0;
        int maxLODTemp = 0;
        AltTrees obj = null;

        int selectedPatch = -1;

        int sizePatch;
        int maxLOD;
        float distancePatchFactor;
        float distanceTreesLODFactor;
        float distanceObjectsLODFactor;
        float checkTreesPercentPerFrame;
        float crossFadeTimeBillboard;
        float crossFadeTimeMesh;

        int initCollidersCountPool;
        int collidersPerOneMaxPool;
        int initColliderBillboardsCountPool;
        int colliderBillboardsPerOneMaxPool;

        bool shadowsMeshes;
        bool shadowsBillboards;
        bool shadowsGroupBillboards;


        bool crossFadeDependenceOnSpeed;
        float crossFadeDependenceOnSpeedMaxSpeed;
        float crossFadeDependenceOnSpeedMaxCoefficient;
        bool checkTreesDependenceOnSpeed;
        float checkTreesDependenceOnSpeedMaxSpeed;
        float checkTreesDependenceOnSpeedMaxCoefficient;

        bool draw;
        bool autoConfig;
        bool autoConfigStar;
        //bool generateAllBillboardsOnStart;
        bool enableColliders;
        bool colliderEvents;
        bool drawDebugPatches;
        bool drawDebugBillboards;
        bool drawDebugBillboardsStar;
        bool debugWindow;
        bool debugLog;
        bool debugLogInBilds;
        bool drawDebugWindow;
        bool drawDebugWindowInBuilds;
        bool hideMeshes;
        bool hideMeshesStar;
        bool hideBillboards;
        bool hideBillboardsStar;
        bool hideGroupBillboards;
        bool hideGroupBillboardsStar;

        bool multiThreading;
        bool multiThreadingStar;

        bool frustumCullingMultiThreading;
        bool frustumCullingMultiThreadingStar;

        bool stableEditorMode;


        GUIContent content;
        GUIStyle st;
        Vector2 size;

        List<int> prototypesIds = new List<int>();
        List<int> prototypesCount = new List<int>();
        List<int> prototypesEmpty = new List<int>();

#if (UNITY_5_2 || UNITY_5_3 || UNITY_5_4)
            int renderType;
            int renderTypeStar;
#endif

        int cameraModeFrustum = 0;
        //Camera activeCameraFrustum = null;
        int cameraModeDistance = 0;
        //Camera activeCameraDistance = null;

        float densityObjects;

        bool isRefresh = false;


        SerializedProperty menuId;
        int menuIdStar = 0;



        SerializedProperty idTreeSelected;
        int idTreeSelectedStar = -1;
        SerializedProperty brushSize;
        SerializedProperty treeCount;
        SerializedProperty speedPlace;
        SerializedProperty randomRotation;

        SerializedProperty height;
        SerializedProperty heightRandom;
        SerializedProperty isRandomHeight;

        SerializedProperty lockWidthToHeight;
        SerializedProperty width;
        SerializedProperty widthRandom;
        SerializedProperty isRandomWidth;

        SerializedProperty isRandomHueLeaves;
        SerializedProperty isRandomHueBark;
        Color hueColorLeaves;
        Color hueColorBark;

        SerializedProperty angleLimit;


        Transform projectorTransform = null;
        Projector projector = null;

        bool isPlacingShift = false;
        bool isPlacingCtrl = false;
        bool isPlacingAlt = false;

        bool isImport = false;
        bool isExport = false;
        //bool isExportAll = false;
        //bool isExportAllStar = false;
        //List<AltTree> atpExportList = new List<AltTree>();
        //Terrain[] terrainsTempExportAll = null;
        public Terrain terrainTempImport = null;
        Terrain terrainTempExport = null;
        Terrain terrainTempExportStar = null;
        Terrain terrainTempMassPlace = null;
        int countMassPlace = 10000;
        public bool isDeleteTreesFromTerrain = true;
        public bool isDeletePrototypesFromTerrain = false;
        public bool isDeleteTreesFromAltTrees = true;
        public bool useDefaultSettingsConvertTrees = false;
        GUIStyle sty;
        GUIStyle sty2;
        GUIStyle sty3;
        GUIStyle sty4;
        GUIStyle sty5;
        GUIStyle sty6;
        GUIStyle sty7;

        AltTree[] altTreesArrayExport;
        GameObject[] terrainTreesArrayExport;
        List<AltTreesTrees> attTemp;
        List<int> prototypesListTemp;
        AltTreesPatch[] listPatchesExport;

        Vector2 posScroll = Vector2.zero;

        AltTree treeTemp;

        int[] treeIdsTemp;

        Texture2D textureBackground;

        bool checkTreeVersionsStatus = false;

        static Dictionary<int, Texture2D> icons = new Dictionary<int, Texture2D>();

        public void OnEnable()
        {
            obj = (AltTrees)target;
            
            if (obj == null)
                return;

            if (obj.altTreesManagerData == null)
                CreateAltTreesManagerData();

            getDataLinks();

            if (obj.dataLinksCorrupted)
                return;

            checkTreeVersionsStatus = dataLinks.checkTreeVersionsStatus();
            if (checkTreeVersionsStatus)
            {
                sty6 = new GUIStyle();
                sty6.fontStyle = FontStyle.Bold;
                sty6.normal.textColor = Color.red;
                sty6.wordWrap = true;
                altTT = dataLinks.checkTreeVersions();
                return;
            }

            selectedPatch = -1;

            sty = new GUIStyle();
            sty.wordWrap = true;
            sty2 = new GUIStyle();
            sty3 = new GUIStyle();
            sty3.alignment = TextAnchor.LowerCenter;
            sty4 = new GUIStyle();
            sty4.richText = true;
            sty5 = new GUIStyle();
            sty5.fontStyle = FontStyle.Bold;
            sty6 = new GUIStyle();
            sty6.fontStyle = FontStyle.Bold;
            sty6.normal.textColor = Color.red;
            sty6.wordWrap = true;

            selectedPatch = -1;

            sty7 = new GUIStyle();
            sty7.wordWrap = true;
            sty7.normal.textColor = Color.grey;

            Color32 cvet = new Color32(20, 97, 225, 255);
            textureBackground = new Texture2D(2, 2);
            textureBackground.SetPixels32(new Color32[] { cvet, cvet, cvet, cvet });
            textureBackground.hideFlags = HideFlags.HideAndDontSave;
            textureBackground.Apply();

            isRefresh = false;

            sizePatch = obj.altTreesManagerData.sizePatch;
            maxLOD = obj.altTreesManagerData.maxLOD;
            distancePatchFactor = obj.altTreesManagerData.distancePatchFactor;
            distanceTreesLODFactor = obj.altTreesManagerData.distanceTreesLODFactor;
            distanceObjectsLODFactor = obj.altTreesManagerData.distanceObjectsLODFactor;
            checkTreesPercentPerFrame = obj.altTreesManagerData.checkTreesPerFramePercent;
            crossFadeTimeBillboard = obj.altTreesManagerData.crossFadeTimeBillboard;
            crossFadeTimeMesh = obj.altTreesManagerData.crossFadeTimeMesh;

            initCollidersCountPool = obj.altTreesManagerData.initCollidersCountPool;
            collidersPerOneMaxPool = obj.altTreesManagerData.collidersPerOneMaxPool;
            initColliderBillboardsCountPool = obj.altTreesManagerData.initColliderBillboardsCountPool;
            colliderBillboardsPerOneMaxPool = obj.altTreesManagerData.colliderBillboardsPerOneMaxPool;

            shadowsGroupBillboards = obj.altTreesManagerData.shadowsGroupBillboards;
            shadowsBillboards = obj.altTreesManagerData.shadowsBillboards;
            shadowsMeshes = obj.altTreesManagerData.shadowsMeshes;

            crossFadeDependenceOnSpeed = obj.altTreesManagerData.crossFadeDependenceOnSpeed;
            crossFadeDependenceOnSpeedMaxSpeed = obj.altTreesManagerData.crossFadeDependenceOnSpeedMaxSpeed;
            crossFadeDependenceOnSpeedMaxCoefficient = obj.altTreesManagerData.crossFadeDependenceOnSpeedMaxCoefficient;
            checkTreesDependenceOnSpeed = obj.altTreesManagerData.checkTreesDependenceOnSpeed;
            checkTreesDependenceOnSpeedMaxSpeed = obj.altTreesManagerData.checkTreesDependenceOnSpeedMaxSpeed;
            checkTreesDependenceOnSpeedMaxCoefficient = obj.altTreesManagerData.checkTreesDependenceOnSpeedMaxCoefficient;


            draw = obj.altTreesManagerData.draw;
            autoConfig = obj.altTreesManagerData.autoConfig;
            autoConfigStar = autoConfig;
            //generateAllBillboardsOnStart = obj.altTreesManagerData.generateAllBillboardsOnStart;
            enableColliders = obj.altTreesManagerData.enableColliders;
            colliderEvents = obj.altTreesManagerData.colliderEvents;
            drawDebugPatches = obj.altTreesManagerData.drawDebugPatches;
            drawDebugBillboards = obj.altTreesManagerData.drawDebugBillboards;
            drawDebugBillboardsStar = obj.altTreesManagerData.drawDebugBillboardsStar;
            debugLog = obj.altTreesManagerData.debugLog;
            debugLogInBilds = obj.altTreesManagerData.debugLogInBilds;
            drawDebugWindow = obj.altTreesManagerData.drawDebugWindow;
            drawDebugWindowInBuilds = obj.altTreesManagerData.drawDebugWindowInBuilds;
            hideMeshes = obj.altTreesManagerData.hideMeshes;
            hideMeshesStar = hideMeshes;
            hideBillboards = obj.altTreesManagerData.hideBillboards;
            hideBillboardsStar = hideBillboards;
            hideGroupBillboards = obj.altTreesManagerData.hideGroupBillboards;
            hideGroupBillboardsStar = hideGroupBillboards;

            multiThreading = obj.altTreesManagerData.multiThreading;
            multiThreadingStar = multiThreading;

            frustumCullingMultiThreading = obj.enableFrustum;
            frustumCullingMultiThreadingStar = frustumCullingMultiThreading;

            stableEditorMode = obj.altTreesManagerData.stableEditorMode;

            #if (UNITY_5_2 || UNITY_5_3 || UNITY_5_4)
                renderType = obj.altTreesManagerData.renderType;
                renderTypeStar = renderType;
            #endif

            cameraModeFrustum = obj.cameraModeFrustum;
            //activeCameraFrustum = obj.activeCameraFrustum;
            cameraModeDistance = obj.cameraModeDistance;
            //activeCameraDistance = obj.activeCameraDistance;

            densityObjects = obj.altTreesManagerData.densityObjects;

            menuId = serializedObject.FindProperty("menuId");
            menuIdStar = menuId.intValue;

            idTreeSelected = serializedObject.FindProperty("idTreeSelected");
            brushSize = serializedObject.FindProperty("brushSize");
            treeCount = serializedObject.FindProperty("treeCount");
            speedPlace = serializedObject.FindProperty("speedPlace");
            randomRotation = serializedObject.FindProperty("randomRotation");

            height = serializedObject.FindProperty("height");
            heightRandom = serializedObject.FindProperty("heightRandom");
            isRandomHeight = serializedObject.FindProperty("isRandomHeight");
            lockWidthToHeight = serializedObject.FindProperty("lockWidthToHeight");
            width = serializedObject.FindProperty("width");
            widthRandom = serializedObject.FindProperty("widthRandom");
            isRandomWidth = serializedObject.FindProperty("isRandomWidth");

            isRandomHueLeaves = serializedObject.FindProperty("isRandomHueLeaves");
            isRandomHueBark = serializedObject.FindProperty("isRandomHueBark");

            angleLimit = serializedObject.FindProperty("angleLimit");

            

            sizePatchTemp = sizePatch;
            maxLODTemp = maxLOD;


            projectorTransform = (Instantiate(AssetDatabase.LoadAssetAtPath("Assets/Plugins/Editor/AltSystems/AltTrees/EditorResources/Projector/Projector.prefab", typeof(GameObject))) as GameObject).transform;
            projector = projectorTransform.GetComponent<Projector>();
            projectorTransform.gameObject.SetActive(false);
            projectorTransform.gameObject.hideFlags = HideFlags.HideAndDontSave;


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
            schs = new int[countTrees];
            if (idTreeSelected.intValue >= treeIdsTemp.Length)
                idTreeSelected.intValue = -1;
            if (dataLinks.altTrees != null)
            {
                for (int i = 0; i < dataLinks.altTrees.Length && countTreesTemp < countTrees; i++)
                {
                    if (dataLinks.altTrees[i] != null)
                    {
                        trees[countTreesTemp] = dataLinks.altTrees[i];
                        treeIdsTemp[countTreesTemp] = i;
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
                    if (icons.ContainsKey(trees[i].id))
                    {
                        textures[i].image = icons[trees[i].id];
                    }
                    else
                    {
                        textures[i].image = AssetPreview.GetAssetPreview(trees[i].gameObject);
                        if (textures[i].image != null)
                        {
                            textures[i].image = Instantiate(textures[i].image) as Texture;
                            textures[i].image.hideFlags = HideFlags.HideAndDontSave;
                            icons.Add(trees[i].id, textures[i].image as Texture2D);
                        }
                    }
                    textures[i].text = trees[i].name;
                }
            }


            if (menuId.intValue == 1 && idTreeSelected.intValue != -1)
            {
                projectorTransform.gameObject.SetActive(true);
                OffMouseSelect.enable();
                idTreeSelectedStar = idTreeSelected.intValue;


                if (idTreeSelected.intValue >= treeIdsTemp.Length)
                    idTreeSelected.intValue = -1;


                treeTemp = dataLinks.altTrees[treeIdsTemp[idTreeSelected.intValue]];
                if (treeTemp != null)
                {
                    hueColorLeaves = treeTemp.hueVariationLeaves;
                    hueColorBark = treeTemp.hueVariationBark;
                }
            }


            if (autoConfig && obj.isActiveAndEnabled && obj.altTreesManagerData.draw && obj.isInitialized)
            {
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

                if (size != sizePatch)
                {
                    serializedObject.ApplyModifiedProperties();

                    EditorUtility.DisplayProgressBar("Working ... Please wait ... ", "Working ... Please wait ... ", 0.1f);

                    resizePatches((int)size);

                    EditorUtility.ClearProgressBar();

                    sizePatch = (int)size;


                    obj.ReInit(true);

                    Selection.activeGameObject = null;

                    return;
                }
                if (degree != obj.altTreesManagerData.maxLOD)
                {
                    obj.altTreesManagerData.maxLOD = degree;
                    maxLOD = degree;
                    maxLODTemp = maxLOD;

                    EditorUtility.SetDirty(obj.altTreesManagerData);

                    serializedObject.ApplyModifiedProperties();


                    EditorUtility.DisplayProgressBar("Working ... Please wait ... ", "Working ... Please wait ... ", 0.1f);

                    resizeMaxLOD();

                    EditorUtility.ClearProgressBar();

                    
                    obj.ReInit(true);

                    return;
                }
            }
            
            int.TryParse( (Application.unityVersion.Substring(0, 5)).Replace(".", "") , out currentUnityVer);



            content = new GUIContent("");
        }

        void OnDisable()
        {
            selectedPatch = -1;
            checkTreeVersionsStatus = false;
            textures = null;
            treeTemp = null;
            if (projectorTransform != null)
            {
                projector = null;
                DestroyImmediate(projectorTransform.gameObject);
            }
            OffMouseSelect.disable();

            if (isRefresh)
            {
                AssetDatabase.SaveAssets();
            }
            AssetDatabase.Refresh();
        }

        GUIContent[] textures;
        int[] schs;
        int countTrees = 0;
        int countTreesTemp = 0;
        AltTree[] trees;
        Type _type = null;
        FieldInfo _propInfo = null;
        static bool isUpdate = false;

        AltTree[] altTT;

        public override void OnInspectorGUI()
        {
            if (obj == null)
                return;

            if (Application.isPlaying)
            {
                GUILayout.Space(20);
                GUILayout.Label("<b>Not available in play mode.</b>", sty4);
                GUILayout.Space(20);

                return;
            }

            if(content == null)
                content = new GUIContent("");

            if (AltSystemsNewsCheck.newsCheckStatic == null)
            {
                GameObject goTemp = new GameObject("newsCheckStatic");
                goTemp.hideFlags = HideFlags.HideInHierarchy;
                AltSystemsNewsCheck.newsCheckStatic = goTemp.AddComponent<AltSystemsNewsCheck>();
            }

            if(checkTreeVersionsStatus)
            {
                GUILayout.Space(20);
                GUILayout.Label("Some trees require an upgrade:", sty6);
                GUILayout.Space(20);

                if (altTT != null)
                {
                    if (GUILayout.Button("Upgrade All"))
                    {
                        for (int i = 0; i < altTT.Length; i++)
                        {
                            EditorUtility.DisplayProgressBar("Upgrading (" + (i + 1) + " / " + altTT.Length + ")... ", "Upgrading... ", 0.0f);
                            altTT[i].checkVersionTree(false, "Upgrading (" + (i + 1) + " / " + altTT.Length + ")... ");
                        }

                        Selection.activeGameObject = null;

                        AltTrees[] ats = FindObjectsOfType(typeof(AltTrees)) as AltTrees[];
                        foreach (AltTrees at in ats)
                        {
                            at.reInitTimer = 10;
                        }
                        return;
                    }
                    GUILayout.Space(20);
                    for (int i = 0; i < altTT.Length; i++)
                    {
                        GUILayout.BeginHorizontal();
                        {
                            EditorGUILayout.ObjectField(altTT[i], typeof(AltTree), false);
                            if(GUILayout.Button("Upgrade"))
                            {
                                Selection.activeGameObject = altTT[i].gameObject;
                            }
                        }
                        GUILayout.EndHorizontal();
                    }
                }

                return;
            }

            serializedObject.Update();

            if (obj.altTreesManagerData == null)
                CreateAltTreesManagerData();

            getDataLinks();

            if (obj.dataLinksCorrupted)
                return;

            _type = System.Type.GetType("AltSystems.AltTrees.Editor.InstallerAltTrees");
            if (!isUpdate && _type != null)
            {
                if (_type != null)
                    _propInfo = _type.GetField("isInstallOk", BindingFlags.Static | BindingFlags.Public);

                if (_propInfo != null)
                {
                    isUpdate = true;
                    MethodInfo _method = _type.GetMethod("sendQuestion");
                    _method.Invoke(null, null);
                    Selection.activeGameObject = null;
                }
            }

            for (int i = 0; i < countTrees; i++)
            {
                if (textures[i].image == null && trees[i] != null && schs[i] < 10)
                {
                    schs[i]++;
                    if (icons.ContainsKey(trees[i].id))
                    {
                        textures[i].image = icons[trees[i].id];
                    }
                    else
                    {
                        textures[i].image = AssetPreview.GetAssetPreview(trees[i].gameObject);
                        if (textures[i].image != null)
                        {
                            textures[i].image = Instantiate(textures[i].image) as Texture;
                            textures[i].image.hideFlags = HideFlags.HideAndDontSave;
                            icons.Add(trees[i].id, textures[i].image as Texture2D);
                        }
                    }

                    if (schs[i] == 5)
                    {
                        //schs[i] = 0;
                        AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(trees[i].gameObject), ImportAssetOptions.ForceUpdate);
                    }
                    Repaint();
                }
            }

            if (idTreeSelected.intValue >= treeIdsTemp.Length)
                idTreeSelected.intValue = -1;

            if (idTreeSelectedStar != idTreeSelected.intValue)
            {
                if (idTreeSelected.intValue != -1)
                {
                    treeTemp = dataLinks.altTrees[treeIdsTemp[idTreeSelected.intValue]];
                    if (treeTemp != null)
                    {
                        hueColorLeaves = treeTemp.hueVariationLeaves;
                        hueColorBark = treeTemp.hueVariationBark;
                    }
                }

                idTreeSelectedStar = idTreeSelected.intValue;
            }

            GUIStyle style = new GUIStyle();
            style.padding = new RectOffset(2, 2, 2, 2);
            style.imagePosition = ImagePosition.ImageAbove;

            style.clipping = TextClipping.Clip;
            style.alignment = TextAnchor.UpperCenter;
            style.fontSize = 9;

            style.onNormal.background = textureBackground;

            EditorGUILayout.BeginHorizontal();
            {
                if (menuId.intValue == 1)
                    GUI.enabled = false;
                if (GUILayout.Button("Place"))
                {
                    if (idTreeSelected.intValue != -1)
                    {
                        projectorTransform.gameObject.SetActive(true);
                        OffMouseSelect.enable();
                    }
                    menuId.intValue = 1;
                }
                GUI.enabled = true;
                if (menuId.intValue == 2)
                    GUI.enabled = false;
                if (GUILayout.Button("Patches"))
                {
                    selectedPatch = -1;
                    projectorTransform.gameObject.SetActive(false);
                    OffMouseSelect.disable();

                    menuId.intValue = 2;
                }
                GUI.enabled = true;
                if (menuId.intValue == 3)
                    GUI.enabled = false;
                if (GUILayout.Button("Settings"))
                {
                    projectorTransform.gameObject.SetActive(false);
                    OffMouseSelect.disable();

                    menuId.intValue = 3;
                }
                GUI.enabled = true;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            if(altTreesVersionUnity == 520)
			{
				if(currentUnityVer >= 560)
				{
					GUILayout.Label("Attention! You use the AltTrees for Unity 5.2.0+. Please, update AltTrees for Unity 5.6.0+", sty6);
				}
				else if(currentUnityVer >= 550)
				{
					GUILayout.Label("Attention! You use the AltTrees for Unity 5.2.0+. Please, update AltTrees for Unity 5.5.0+", sty6);
				}
				else if(currentUnityVer >= 540)
				{
					GUILayout.Label("Attention! You use the AltTrees for Unity 5.2.0+. Please, update AltTrees for Unity 5.4.0+", sty6);
				}
			}
			else if(altTreesVersionUnity == 540)
			{
				if(currentUnityVer >= 560)
				{
					GUILayout.Label("Attention! You use the AltTrees for Unity 5.4.0+. Please, update AltTrees for Unity 5.6.0+", sty6);
				}
				else if(currentUnityVer >= 550)
				{
					GUILayout.Label("Attention! You use the AltTrees for Unity 5.4.0+. Please, update AltTrees for Unity 5.5.0+", sty6);
				}
			}
			else if(altTreesVersionUnity == 550)
			{
				if(currentUnityVer >= 560)
				{
					GUILayout.Label("Attention! You use the AltTrees for Unity 5.5.0+. Please, update AltTrees for Unity 5.6.0+", sty6);
				}
			}

            EditorGUILayout.Space();

            if (menuId.intValue == 1)
            {
                if (!(obj.isActiveAndEnabled && obj.altTreesManagerData.draw))
                    return;
                
                if (idTreeSelected.intValue != -1 && !projectorTransform.gameObject.activeSelf)
                {
                    projectorTransform.gameObject.SetActive(true);
                    OffMouseSelect.enable();
                }

                GUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    GUILayout.Label("Place Trees", sty5);
                    GUILayout.Label("Hold down shift to erase trees.\nHold down ctrl to erase the selected tree type.", EditorStyles.wordWrappedMiniLabel);
                }
                GUILayout.EndVertical();

                GUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    idTreeSelected.intValue = GUI.SelectionGrid(GetBrushAspectRect(countTrees, 64, 12), idTreeSelected.intValue, textures, (int)Mathf.Ceil((float)((Screen.width - 20) / 64)), style);
                }
                GUILayout.EndVertical();

                if (idTreeSelected.intValue >= countTrees)
                    idTreeSelected.intValue = -1;

                if (idTreeSelected.intValue != -1)
                {
                    GUILayout.Label("Settings:", EditorStyles.boldLabel);

                    brushSize.intValue = EditorGUILayout.IntSlider("Brush Size: ", brushSize.intValue, 1, 300);
                    treeCount.intValue = EditorGUILayout.IntSlider("Tree Count: ", treeCount.intValue, 1, 1000);
                    speedPlace.intValue = EditorGUILayout.IntSlider("Speed Place: ", speedPlace.intValue, 1, 10);
                    randomRotation.boolValue = EditorGUILayout.Toggle("Random Y Rotation: ", randomRotation.boolValue);

                    GUILayout.Space(7);

                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.Label("Tree Height:", GUILayout.Width(EditorGUIUtility.labelWidth - 6f));


                        GUILayout.Label("Random?");
                        isRandomHeight.boolValue = GUILayout.Toggle(isRandomHeight.boolValue, "");

                        if (isRandomHeight.boolValue)
                        {
                            float heightTemp = height.floatValue;
                            float heightRandomTemp = height.floatValue + heightRandom.floatValue;
                            EditorGUILayout.MinMaxSlider(ref heightTemp, ref heightRandomTemp, 0.1f, 2f);

                            if (heightTemp != height.floatValue || heightRandom.floatValue != heightRandomTemp)
                            {
                                height.floatValue = heightTemp;
                                heightRandom.floatValue = heightRandomTemp - heightTemp;
                            }

                            GUILayout.Label(heightTemp.ToString("0.0") + " - " + heightRandomTemp.ToString("0.0"));
                        }
                        else
                        {
                            height.floatValue = (float)System.Math.Round(EditorGUILayout.Slider(height.floatValue, 0.1f, 2f), 2);
                        }
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.Space(7);

                    lockWidthToHeight.boolValue = EditorGUILayout.Toggle("Lock Width to Height: ", lockWidthToHeight.boolValue);

                    GUILayout.Space(7);

                    if (lockWidthToHeight.boolValue)
                        GUI.enabled = false;

                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.Label("Tree Width:", GUILayout.Width(EditorGUIUtility.labelWidth - 6f));


                        GUILayout.Label("Random?");
                        isRandomWidth.boolValue = GUILayout.Toggle(isRandomWidth.boolValue, "");

                        if (isRandomWidth.boolValue)
                        {
                            float widthTemp = width.floatValue;
                            float widthRandomTemp = width.floatValue + widthRandom.floatValue;
                            EditorGUILayout.MinMaxSlider(ref widthTemp, ref widthRandomTemp, 0.1f, 2f);

                            if (widthTemp != width.floatValue || widthRandom.floatValue != widthRandomTemp)
                            {
                                width.floatValue = widthTemp;
                                widthRandom.floatValue = widthRandomTemp - widthTemp;
                            }

                            GUILayout.Label(widthTemp.ToString("0.0") + " - " + widthRandomTemp.ToString("0.0"));
                        }
                        else
                        {
                            width.floatValue = (float)System.Math.Round(EditorGUILayout.Slider(width.floatValue, 0.1f, 2f), 2);
                        }
                    }
                    GUILayout.EndHorizontal();

                    GUI.enabled = true;


                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.Label("Hue Leaves:", GUILayout.Width(EditorGUIUtility.labelWidth - 6f));


                        GUILayout.Label("Random(from alpha color)?");
                        isRandomHueLeaves.boolValue = GUILayout.Toggle(isRandomHueLeaves.boolValue, "");

                        hueColorLeaves = EditorGUILayout.ColorField(hueColorLeaves);
                    }
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.Label("Hue Bark:", GUILayout.Width(EditorGUIUtility.labelWidth - 6f));


                        GUILayout.Label("Random(from alpha color)?");
                        isRandomHueBark.boolValue = GUILayout.Toggle(isRandomHueBark.boolValue, "");

                        hueColorBark = EditorGUILayout.ColorField(hueColorBark);
                    }
                    GUILayout.EndHorizontal();

                    angleLimit.floatValue = (float)System.Math.Round(EditorGUILayout.Slider("Angle Limit:", angleLimit.floatValue, 0.0f, 90f), 1);

                    GUILayout.Space(10);



                    GUILayout.BeginVertical(EditorStyles.helpBox);
                    {
                        GUILayout.Label("Mass Place:", EditorStyles.boldLabel);
                        terrainTempMassPlace = (Terrain)EditorGUILayout.ObjectField(terrainTempMassPlace, typeof(Terrain), true);
                        countMassPlace = EditorGUILayout.IntField("Count:", countMassPlace);
                        if (terrainTempMassPlace == null || countMassPlace <= 0)
                            GUI.enabled = false;
                        if (GUILayout.Button("Place"))
                        {
                            massPlace();
                        }
                        GUI.enabled = true;
                    }
                    GUILayout.EndVertical();
                }

            }
            else if (menuId.intValue == 2)
            {
                if (!(obj.isActiveAndEnabled && obj.altTreesManagerData.draw))
                {
                    selectedPatch = -1;
                    GUI.enabled = false;
                }

                EditorGUILayout.LabelField("Patches:", EditorStyles.boldLabel);

                EditorGUILayout.Space();

                GUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    posScroll = GUILayout.BeginScrollView(posScroll);
                    {
                        sty2.fontSize = 9;
                        sty2.alignment = TextAnchor.MiddleLeft;
                        sty2.fontStyle = FontStyle.Bold;

                        GUILayout.BeginHorizontal();
                        {
                            GUILayout.Label("   Patch:", sty2);
                            GUILayout.Label("Prototypes:", sty2);
                            GUILayout.Label("Trees:", sty2);
                            GUILayout.Label("Objects:", sty2);
                        }
                        GUILayout.EndHorizontal();

                        for (int i = 0; i < obj.altTreesManagerData.patches.Length; i++)
                        {
                            
                            if (selectedPatch == i)
                                GUILayout.BeginVertical(EditorStyles.helpBox);
                            
                            GUILayout.BeginHorizontal(EditorStyles.helpBox);
                            {
                                GUILayout.Label("");
                                GUILayout.Label("");
                                GUILayout.Label("");
                                GUILayout.Label("");
                            }
                            GUILayout.EndHorizontal();

                            Rect scaleRect = GUILayoutUtility.GetLastRect();

                            if (selectedPatch != i)
                            {
                                if (GUI.Button(scaleRect, ""))
                                {
                                    byte[] bytesTemp;
                                    byte[] bytesTemp3;

                                    selectedPatch = i;

                                    prototypesIds.Clear();
                                    prototypesCount.Clear();
                                    prototypesEmpty.Clear();

                                    string pathStr = "";
                                    int _idPrototype = -1;
                                    int _idTemp = -1;

                                    if (obj.altTreesManagerData.patches[i].treesData != null)
                                    {
                                        pathStr = AssetDatabase.GetAssetPath(obj.altTreesManagerData.patches[i].treesData);
                                        if (pathStr != "")
                                            bytesTemp = File.ReadAllBytes(pathStr);
                                        else
                                            bytesTemp = obj.altTreesManagerData.patches[i].treesData.bytes;




                                        int version = AltUtilities.ReadBytesInt(bytesTemp, 0);

                                        if (version == 1)
                                        {
                                            int treesCount = AltUtilities.ReadBytesInt(bytesTemp, 4);
                                            int treesEmptyCount = AltUtilities.ReadBytesInt(bytesTemp, 8);

                                            for (int f = 0; f < treesCount; f++)
                                            {
                                                _idPrototype = AltUtilities.ReadBytesInt(bytesTemp, 12 + f * 60 + 12);

                                                if (prototypesIds.Contains(_idPrototype))
                                                {
                                                    _idTemp = prototypesIds.IndexOf(_idPrototype);
                                                    prototypesCount[_idTemp]++;
                                                }
                                                else
                                                {
                                                    prototypesIds.Add(_idPrototype);
                                                    _idTemp = prototypesIds.IndexOf(_idPrototype);
                                                    prototypesCount.Add(1);
                                                    prototypesEmpty.Add(0);
                                                }

                                                for (int w = 0; w < treesEmptyCount; w++)
                                                {
                                                    if(AltUtilities.ReadBytesInt(bytesTemp, 12 + treesCount * 60 + w * 4) == f)
                                                    {
                                                        prototypesEmpty[_idTemp]++;

                                                        break;
                                                    }
                                                }



                                                
                                            }

                                            
                                        }




                                    }
                                    if (obj.altTreesManagerData.patches[i].treesNoGroupData != null)
                                    {
                                        pathStr = AssetDatabase.GetAssetPath(obj.altTreesManagerData.patches[i].treesNoGroupData);
                                        if (pathStr != "")
                                            bytesTemp3 = File.ReadAllBytes(pathStr);
                                        else
                                            bytesTemp3 = obj.altTreesManagerData.patches[i].treesNoGroupData.bytes;




                                        int version = AltUtilities.ReadBytesInt(bytesTemp3, 0);
                                        int countQuads = AltUtilities.ReadBytesInt(bytesTemp3, 8);

                                        if (version == 2)
                                        {
                                            //int treesCount = AltUtilities.ReadBytesInt(bytesTemp3, 12);
                                            //int treesEmptyCount = AltUtilities.ReadBytesInt(bytesTemp3, 16);




                                            for (int t = 0; t < countQuads; t++)
                                            {
                                                int addrObjs = AltUtilities.ReadBytesInt(bytesTemp3, 20 + t * 4);
                                                int countObjs = 0;

                                                while (addrObjs != -1)
                                                {
                                                    countObjs = AltUtilities.ReadBytesInt(bytesTemp3, addrObjs + 4);

                                                    if (countObjs > 0)
                                                    {
                                                        for (int w = 0; w < countObjs; w++)
                                                        {
                                                            _idPrototype = AltUtilities.ReadBytesInt(bytesTemp3, addrObjs + 8 + w * 61 + 13);

                                                            if (prototypesIds.Contains(_idPrototype))
                                                            {
                                                                _idTemp = prototypesIds.IndexOf(_idPrototype);
                                                                prototypesCount[_idTemp]++;
                                                            }
                                                            else
                                                            {
                                                                prototypesIds.Add(_idPrototype);
                                                                _idTemp = prototypesIds.IndexOf(_idPrototype);
                                                                prototypesCount.Add(1);
                                                                prototypesEmpty.Add(0);
                                                            }

                                                            if (!AltUtilities.ReadBytesBool(bytesTemp3, addrObjs + 8 + w * 61 + 0))
                                                                prototypesEmpty[_idTemp]++;

                                                        }
                                                    }
                                                    addrObjs = AltUtilities.ReadBytesInt(bytesTemp3, addrObjs);
                                                }
                                            }
                                        }
                                    }
                                    
                                    for (int t = 0; t < obj.altTreesManagerData.patches[i].prototypes.Length; t++)
                                    {
                                        if (obj.altTreesManagerData.patches[i].prototypes[t] != null && obj.altTreesManagerData.patches[i].prototypes[t].tree != null)
                                        {
                                            _idPrototype = obj.altTreesManagerData.patches[i].prototypes[t].tree.id;
                                            if (!prototypesIds.Contains(_idPrototype))
                                            {
                                                prototypesIds.Add(_idPrototype);
                                                _idTemp = prototypesIds.IndexOf(_idPrototype);
                                                prototypesCount.Add(0);
                                                prototypesEmpty.Add(0);
                                            }
                                        }
                                        else
                                        {

                                        }
                                    }

                                }
                            }

                            st = GUI.skin.label;

                            GUI.Label(new Rect(scaleRect.x + 5, scaleRect.y + 3, 120, 25), "Patch [" + obj.altTreesManagerData.patches[i].stepX + " x " + obj.altTreesManagerData.patches[i].stepY + "]");

                            int countPrefabs = 0;

                            if(obj.altTreesManagerData.patches[i].prototypes != null)
                            {
                                for(int t = 0; t < obj.altTreesManagerData.patches[i].prototypes.Length; t++)
                                {
                                    if (obj.altTreesManagerData.patches[i].prototypes[t] != null && obj.altTreesManagerData.patches[i].prototypes[t].tree != null)
                                        countPrefabs++;
                                }
                            }

                            content.text = countPrefabs + "";
                            size = st.CalcSize(content);

                            GUI.Label(new Rect(scaleRect.x + 5 + (scaleRect.width - 5f) / 4f + 20f - size.x / 2f, scaleRect.y + 3, 120, 25), content);

                            content.text = (obj.altTreesManagerData.patches[i].treesCount - obj.altTreesManagerData.patches[i].treesEmptyCount) + "" + " ["+ obj.altTreesManagerData.patches[i].treesEmptyCount + "]";
                            size = st.CalcSize(content);

                            GUI.Label(new Rect(scaleRect.x + 5 + ((scaleRect.width - 5f) / 4f) * 2f + 25f - size.x / 2f, scaleRect.y + 3, 120, 25), content);

                            content.text = (obj.altTreesManagerData.patches[i].treesNoGroupCount - obj.altTreesManagerData.patches[i].treesNoGroupEmptyCount) + "" + " [" + obj.altTreesManagerData.patches[i].treesNoGroupEmptyCount + "]";
                            size = st.CalcSize(content);

                            GUI.Label(new Rect(scaleRect.x + 5 + ((scaleRect.width - 5f) / 4f) * 3f + 25f - size.x / 2f, scaleRect.y + 3, 120, 25), content);
                            
                            if(selectedPatch == i)
                            {
                                Vector3 pos = obj.altTreesManagerData.patches[i].step * obj.altTreesManagerData.sizePatch;

                                GUILayout.Label("Area: X1Y1: " + pos.x + ", " + pos.z + ";  X2Y2: " + (pos.x + obj.altTreesManagerData.sizePatch) + ", " + (pos.z + obj.altTreesManagerData.sizePatch));

                                GUILayout.Space(20);

                                //GUILayout.Label("Prototypes:");
                                EditorGUILayout.LabelField("Prototypes:", EditorStyles.boldLabel);

                                EditorGUILayout.Space();

                                sty2.fontSize = 9;
                                sty2.alignment = TextAnchor.MiddleLeft;
                                sty2.fontStyle = FontStyle.Bold;



                                GUILayout.BeginVertical(EditorStyles.helpBox);
                                {
                                    GUILayout.Space(20 + 90 * prototypesIds.Count);
                                }
                                GUILayout.EndVertical();

                                scaleRect = GUILayoutUtility.GetLastRect();

                                /*GUILayout.BeginHorizontal();
                                {
                                    GUILayout.Label("     ID:", sty2);
                                    GUILayout.Label("Count:", sty2);
                                    GUILayout.Label("Empty:", sty2);
                                    GUILayout.Label("Prefab:", sty2);
                                    GUILayout.Label("-", sty2);
                                }
                                GUILayout.EndHorizontal();*/


                                GUI.Label(new Rect(scaleRect.x + ((scaleRect.width - 5f) / 5f) * 0f + 17f, scaleRect.y - 3, 120, 25), "ID:", sty2);
                                GUI.Label(new Rect(scaleRect.x + ((scaleRect.width - 5f) / 5f) * 1f + 5f, scaleRect.y - 3, 120, 25), "Count:", sty2);
                                GUI.Label(new Rect(scaleRect.x + ((scaleRect.width - 5f) / 5f) * 2f + 10f, scaleRect.y - 3, 120, 25), "Empty:", sty2);
                                GUI.Label(new Rect(scaleRect.x + ((scaleRect.width - 5f) / 5f) * 3f + 15f, scaleRect.y - 3, 120, 25), "Prefab:", sty2);
                                //GUI.Label(new Rect(scaleRect.x + ((scaleRect.width - 5f) / 5f) * 4f + 11f, scaleRect.y - 3, 120, 25), "-", sty2);

                                AltTree atTemp = null;

                                for (int h = 0; h < prototypesIds.Count; h++)
                                {

                                    bool isOk = false;
                                    bool repl = false;

                                    for (int t = 0; t < obj.altTreesManagerData.patches[i].prototypes.Length; t++)
                                    {
                                        if (obj.altTreesManagerData.patches[i].prototypes[t].tree != null)
                                        {
                                            if (obj.altTreesManagerData.patches[i].prototypes[t].tree.id == prototypesIds[h])
                                            {
                                                atTemp = obj.altTreesManagerData.patches[i].prototypes[t].tree;

                                                Texture tex = null;

                                                if (icons.ContainsKey(obj.altTreesManagerData.patches[i].prototypes[t].tree.id))
                                                {
                                                    tex = icons[obj.altTreesManagerData.patches[i].prototypes[t].tree.id];
                                                }
                                                else
                                                {
                                                    tex = AssetPreview.GetAssetPreview(obj.altTreesManagerData.patches[i].prototypes[t].tree.gameObject);
                                                    if (tex != null)
                                                    {
                                                        tex = Instantiate(tex) as Texture;
                                                        tex.hideFlags = HideFlags.HideAndDontSave;
                                                        icons.Add(obj.altTreesManagerData.patches[i].prototypes[t].tree.id, tex as Texture2D);
                                                    }
                                                }

                                                if (tex != null)
                                                    GUI.Label(new Rect(scaleRect.x + ((scaleRect.width - 5f) / 5f) * 3f + 13f - 50f + 40, scaleRect.y + 19 + 90f * h, 60, 60), tex);


                                                atTemp = EditorGUI.ObjectField(new Rect(scaleRect.x + ((scaleRect.width - 5f) / 5f) * 3f + 3f - 50f + 40, scaleRect.y + 77 + 90f * h, 100, 15), obj.altTreesManagerData.patches[i].prototypes[t].tree, typeof(AltTree), false) as AltTree;

                                                isOk = true;

                                                
                                                if(atTemp == null || !atTemp.Equals(obj.altTreesManagerData.patches[i].prototypes[t].tree))
                                                {
                                                    if(atTemp != null && !atTemp.Equals(obj.altTreesManagerData.patches[i].prototypes[t].tree))
                                                    {
                                                        repl = true;
                                                    }
                                                    else
                                                    {
                                                        obj.altTreesManagerData.patches[i].prototypes[t].tree = atTemp;

                                                        EditorUtility.SetDirty(obj.altTreesManagerData);
                                                        AssetDatabase.SaveAssets();
                                                        AssetDatabase.Refresh();
                                                        obj.ReInit(true);
                                                    }
                                                }

                                                break;
                                            }
                                        }
                                    }

                                    if (!isOk)
                                    {
                                        Color gc = GUI.color;
                                        GUI.color = new Color(1,0,0,0.3f);
                                        GUI.Box(new Rect(scaleRect.x, scaleRect.y + 18 + 90f * h, scaleRect.width, 75), "");
                                        GUI.color = gc;

                                        //GUI.Label(new Rect(scaleRect.x + ((scaleRect.width - 5f) / 5f) * 3f + 13f - size.x / 2f, scaleRect.y + 19 + 90f * h, 60, 60), "[null]");

                                        atTemp = EditorGUI.ObjectField(new Rect(scaleRect.x + ((scaleRect.width - 5f) / 5f) * 3f + 3f - 50 + 45, scaleRect.y + 77 + 90f * h, 100, 15), null, typeof(AltTree), false) as AltTree;

                                        if (atTemp != null)
                                        {
                                            if (atTemp.id != prototypesIds[h])
                                            {
                                                repl = true;
                                            }
                                            else
                                            {
                                                obj.altTreesManagerData.patches[i].checkTreePrototype(prototypesIds[h], atTemp, false, false);

                                                EditorUtility.SetDirty(obj.altTreesManagerData);
                                                AssetDatabase.SaveAssets();
                                                AssetDatabase.Refresh();
                                                obj.ReInit(true);
                                            }
                                        }
                                    }

                                    if(repl)
                                    {










                                        List<AltTreesTrees> attListObjs = new List<AltTreesTrees>();
                                        List<AltTreesTrees> attListTrees = new List<AltTreesTrees>();


                                        
                                        #if UNITY_EDITOR
                                            EditorUtility.DisplayProgressBar("Working ... Please wait ... ", "Working ... Please wait ... ", 0.1f);
                                        #endif

                                        AltTreesTrees[] treesTemp = obj.altTreesManagerData.patches[i].trees;
                                        //AltTreesTrees[] objectsTemp = new AltTreesTrees[treesNoGroupCount - treesNoGroupEmptyCount];
                                        AltTreesTrees[] objectsTemp = new AltTreesTrees[obj.altTreesManagerData.patches[i].treesNoGroupCount];

                                        int objectsSch = 0;
                                        int countQuads = 0;

                                        if (obj.altTreesManagerData.patches[i].bytesTemp3 != null && obj.altTreesManagerData.patches[i].bytesTemp3.Length > 0)
                                        {
                                            int version = AltUtilities.ReadBytesInt(obj.altTreesManagerData.patches[i].bytesTemp3, 0);

                                            Vector3 _pos = new Vector3();
                                            int _idPrototype;
                                            Color _color = new Color();
                                            Color _colorBark = new Color();
                                            float _rotation;
                                            float _heightScale;
                                            float _widthScale;
                                            int objectsSch2 = 0;

                                            countQuads = AltUtilities.ReadBytesInt(obj.altTreesManagerData.patches[i].bytesTemp3, 8);

                                            if (version == 2)
                                            {
                                                if (countQuads == obj.altTreesManagerData.patches[i].objectsQuadIdTemp - 1)
                                                {
                                                    for (int t = 0; t < countQuads; t++)
                                                    {
                                                        int addrObjs = AltUtilities.ReadBytesInt(obj.altTreesManagerData.patches[i].bytesTemp3, 20 + t * 4);
                                                        int countObjects = 0;
                                                        objectsSch2 = 0;

                                                        while (addrObjs != -1)
                                                        {
                                                            countObjects = AltUtilities.ReadBytesInt(obj.altTreesManagerData.patches[i].bytesTemp3, addrObjs + 4);

                                                            if (countObjects > 0)
                                                            {
                                                                for (int d = 0; d < countObjects; d++)
                                                                {
                                                                    if (AltUtilities.ReadBytesBool(obj.altTreesManagerData.patches[i].bytesTemp3, addrObjs + 8 + d * 61 + 0))
                                                                    {
                                                                        _pos = AltUtilities.ReadBytesVector3(obj.altTreesManagerData.patches[i].bytesTemp3, addrObjs + 8 + d * 61 + 1);
                                                                        _idPrototype = AltUtilities.ReadBytesInt(obj.altTreesManagerData.patches[i].bytesTemp3, addrObjs + 8 + d * 61 + 13);
                                                                        _color = AltUtilities.ReadBytesColor(obj.altTreesManagerData.patches[i].bytesTemp3, addrObjs + 8 + d * 61 + 17);
                                                                        _colorBark = AltUtilities.ReadBytesColor(obj.altTreesManagerData.patches[i].bytesTemp3, addrObjs + 8 + d * 61 + 33);
                                                                        _rotation = AltUtilities.ReadBytesFloat(obj.altTreesManagerData.patches[i].bytesTemp3, addrObjs + 8 + d * 61 + 49);
                                                                        _heightScale = AltUtilities.ReadBytesFloat(obj.altTreesManagerData.patches[i].bytesTemp3, addrObjs + 8 + d * 61 + 53);
                                                                        _widthScale = AltUtilities.ReadBytesFloat(obj.altTreesManagerData.patches[i].bytesTemp3, addrObjs + 8 + d * 61 + 57);

                                                                        AltTreesTrees att = new AltTreesTrees(_pos, objectsSch2, _idPrototype, true, _color, _colorBark, _rotation, _heightScale, _widthScale, obj.altTreesManagerData.patches[i]);

                                                                        att.idQuadObject = t;
                                                                        objectsTemp[objectsSch] = att;
                                                                        objectsSch++;
                                                                        objectsSch2++;
                                                                    }
                                                                }
                                                            }
                                                            addrObjs = AltUtilities.ReadBytesInt(obj.altTreesManagerData.patches[i].bytesTemp3, addrObjs);
                                                        }
                                                    }
                                                }
                                                else
                                                    obj.LogError("Count Quads in file(" + AltUtilities.ReadBytesInt(obj.altTreesManagerData.patches[i].bytesTemp3, 8) + ") != Count Quads in Manager(" + (obj.altTreesManagerData.patches[i].objectsQuadIdTemp - 1) + ")");
                                            }
                                            else
                                                obj.LogError("Version == "+version);
                                        }
                                        //altTrees.LogError("treesNoGroupCount(" + treesNoGroupCount + ") != treesNoGroupEmptyCount(" + treesNoGroupEmptyCount + ")");

                                        obj.altTreesManagerData.patches[i].treesNoGroupCount = objectsSch;
                                        obj.altTreesManagerData.patches[i].treesNoGroupEmptyCount = 0;

                                        //if (objectsTemp.Length != treesNoGroupCount)
                                        //    altTrees.LogError("objectsTemp.Length("+ objectsTemp.Length + ") != treesNoGroupCount("+ treesNoGroupCount + ")");

                                        AltTreesTrees[] treesNoGroup = null;

                                        #if UNITY_EDITOR
                                            EditorUtility.DisplayProgressBar("Working ... Please wait ... ", "Working ... Please wait ... ", 0.2f);
                                        #endif

                                        int countObjs = 0;
                                        int countTrees = 0;
                                        bool boolTemp = false;
                                        

                                        for (int d = 0; d < obj.altTreesManagerData.patches[i].treesNoGroupCount; d++)
                                        {
                                            if (objectsTemp[d].idPrototype == prototypesIds[h])
                                            {
                                                countObjs++;
                                            }
                                        }
                                        for (int d = 0; d < treesTemp.Length; d++)
                                        {
                                            if (treesTemp[d] != null && treesTemp[d].idPrototype == prototypesIds[h])
                                            {
                                                boolTemp = false;
                                                for (int j = 0; j < obj.altTreesManagerData.patches[i].treesEmptyCount; j++)
                                                {
                                                    if (obj.altTreesManagerData.patches[i].treesEmpty[j] == d)
                                                    {
                                                        boolTemp = true;
                                                        break;
                                                    }
                                                }
                                                if (!boolTemp)
                                                    countTrees++;
                                            }
                                        }

                                        treesNoGroup = new AltTreesTrees[obj.altTreesManagerData.patches[i].treesNoGroupCount - countObjs + (atTemp.isObject ? countTrees + countObjs : 0)];
                                        obj.altTreesManagerData.patches[i].trees = new AltTreesTrees[obj.altTreesManagerData.patches[i].treesCount - obj.altTreesManagerData.patches[i].treesEmptyCount - countTrees + (!atTemp.isObject ? countTrees + countObjs : 0)];


                                        int treesNoGroupIndx = 0;

                                        for (int d = 0; d < obj.altTreesManagerData.patches[i].treesNoGroupCount; d++)
                                        {
                                            if (objectsTemp[d].idPrototype != prototypesIds[h])
                                            {
                                                treesNoGroup[treesNoGroupIndx] = objectsTemp[d];
                                                treesNoGroupIndx++;
                                            }
                                            else
                                                attListObjs.Add(objectsTemp[d]);
                                        }

                                        

                                        int treesIndx = 0;

                                        for (int d = 0; d < treesTemp.Length; d++)
                                        {
                                            boolTemp = false;
                                            for (int j = 0; j < obj.altTreesManagerData.patches[i].treesEmptyCount; j++)
                                            {
                                                if (obj.altTreesManagerData.patches[i].treesEmpty[j] == d)
                                                {
                                                    boolTemp = true;
                                                    break;
                                                }
                                            }
                                            if (!boolTemp)
                                            {
                                                if (treesTemp[d] != null && treesTemp[d].idPrototype != prototypesIds[h])
                                                {
                                                    obj.altTreesManagerData.patches[i].trees[treesIndx] = treesTemp[d];
                                                    treesIndx++;
                                                }
                                                else if (treesTemp[d] != null && treesTemp[d].idPrototype == prototypesIds[h])
                                                    attListTrees.Add(treesTemp[d]);
                                            }
                                        }


                                        if (atTemp.isObject)
                                        {
                                            for (int d = 0; d < attListTrees.Count; d++)
                                            {
                                                treesNoGroup[treesNoGroupIndx] = attListTrees[d];
                                                treesNoGroup[treesNoGroupIndx].idPrototype = atTemp.id;
                                                treesNoGroupIndx++;
                                            }
                                            for (int d = 0; d < attListObjs.Count; d++)
                                            {
                                                treesNoGroup[treesNoGroupIndx] = attListObjs[d];
                                                treesNoGroup[treesNoGroupIndx].idPrototype = atTemp.id;
                                                treesNoGroupIndx++;
                                            }
                                        }
                                        else
                                        {
                                            for (int d = 0; d < attListTrees.Count; d++)
                                            {
                                                obj.altTreesManagerData.patches[i].trees[treesIndx] = attListTrees[d];
                                                obj.altTreesManagerData.patches[i].trees[treesIndx].idPrototype = atTemp.id;
                                                treesIndx++;
                                            }
                                            for (int d = 0; d < attListObjs.Count; d++)
                                            {
                                                obj.altTreesManagerData.patches[i].trees[treesIndx] = attListObjs[d];
                                                obj.altTreesManagerData.patches[i].trees[treesIndx].idPrototype = atTemp.id;
                                                treesIndx++;
                                            }
                                        }


                                        obj.altTreesManagerData.patches[i].treesCount = obj.altTreesManagerData.patches[i].trees.Length;
                                        obj.altTreesManagerData.patches[i].treesEmptyCount = 0;
                                        obj.altTreesManagerData.patches[i].treesEmpty = new int[0];

                                        obj.altTreesManagerData.patches[i].treesNoGroupCount = treesNoGroup.Length;
                                        obj.altTreesManagerData.patches[i].treesNoGroupEmptyCount = 0;

                                        byte[] bytes4Temp = new byte[4];
                                        byte[] bytes60Temp = new byte[60];
                                        byte[] bytes61Temp = new byte[61];

                                        #if UNITY_EDITOR
                                            EditorUtility.DisplayProgressBar("Working ... Please wait ... ", "Working ... Please wait ... ", 0.3f);
                                        #endif

                                        if(obj.altTreesManagerData.patches[i].treesCount == 0)
                                        { 
                                            if (obj.altTreesManagerData.patches[i].treesData != null)
                                            {
                                                obj.altTreesManagerData.patches[i].trees = new AltTreesTrees[0];
                                                obj.altTreesManagerData.patches[i].treesCount = 0;
                                                obj.altTreesManagerData.patches[i].treesEmptyCount = 0;
                                                obj.altTreesManagerData.patches[i].treesEmpty = new int[0];

                                                #if UNITY_EDITOR
                                                    AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(obj.altTreesManagerData.patches[i].treesData));

                                                    EditorUtility.SetDirty(obj.altTreesManagerData);
                                                    AssetDatabase.SaveAssets();
                                                    AssetDatabase.Refresh();
                                                #endif
                                            }
                                        }
                                        else
                                        {
                                            if (File.Exists("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + obj.getIdManager() + "/treesData_" + obj.altTreesManagerData.patches[i].stepX + "_" + obj.altTreesManagerData.patches[i].stepY + ".bytesTemp"))
                                                File.Delete("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + obj.getIdManager() + "/treesData_" + obj.altTreesManagerData.patches[i].stepX + "_" + obj.altTreesManagerData.patches[i].stepY + ".bytesTemp");

                                            if (File.Exists("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + obj.getIdManager() + "/treesData_" + obj.altTreesManagerData.patches[i].stepX + "_" + obj.altTreesManagerData.patches[i].stepY + ".bytesTemp.meta"))
                                                File.Delete("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + obj.getIdManager() + "/treesData_" + obj.altTreesManagerData.patches[i].stepX + "_" + obj.altTreesManagerData.patches[i].stepY + ".bytesTemp.meta");

                                            using (BinaryWriter writer = new BinaryWriter(File.Open("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + obj.getIdManager() + "/treesData_" + obj.altTreesManagerData.patches[i].stepX + "_" + obj.altTreesManagerData.patches[i].stepY + ".bytesTemp", FileMode.Create)))
                                            {
                                                AltUtilities.WriteBytes(1, bytes4Temp, 0);
                                                writer.Write(bytes4Temp);
                                                AltUtilities.WriteBytes(obj.altTreesManagerData.patches[i].treesCount, bytes4Temp, 0);
                                                writer.Write(bytes4Temp);
                                                AltUtilities.WriteBytes(0, bytes4Temp, 0);
                                                writer.Write(bytes4Temp);
                    
                                                for (int d = 0; d < obj.altTreesManagerData.patches[i].treesCount; d++)
                                                {
                                                    AltUtilities.WriteBytes(obj.altTreesManagerData.patches[i].trees[d].pos, bytes60Temp, 0);
                                                    AltUtilities.WriteBytes(obj.altTreesManagerData.patches[i].trees[d].idPrototype, bytes60Temp, 12);
                                                    AltUtilities.WriteBytes(obj.altTreesManagerData.patches[i].trees[d].color, bytes60Temp, 16);
                                                    AltUtilities.WriteBytes(obj.altTreesManagerData.patches[i].trees[d].colorBark, bytes60Temp, 32);
                                                    AltUtilities.WriteBytes(obj.altTreesManagerData.patches[i].trees[d].rotation, bytes60Temp, 48);
                                                    AltUtilities.WriteBytes(obj.altTreesManagerData.patches[i].trees[d].heightScale, bytes60Temp, 52);
                                                    AltUtilities.WriteBytes(obj.altTreesManagerData.patches[i].trees[d].widthScale, bytes60Temp, 56);
                                                    writer.Write(bytes60Temp);
                                                }
                                            }
                                            if(File.Exists("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + obj.getIdManager() + "/treesData_" + obj.altTreesManagerData.patches[i].stepX + "_" + obj.altTreesManagerData.patches[i].stepY + ".bytes"))
                                                File.Replace("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + obj.getIdManager() + "/treesData_" + obj.altTreesManagerData.patches[i].stepX + "_" + obj.altTreesManagerData.patches[i].stepY + ".bytesTemp", "Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + obj.getIdManager() + "/treesData_" + obj.altTreesManagerData.patches[i].stepX + "_" + obj.altTreesManagerData.patches[i].stepY + ".bytes", null);
                                            else
                                                File.Move("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + obj.getIdManager() + "/treesData_" + obj.altTreesManagerData.patches[i].stepX + "_" + obj.altTreesManagerData.patches[i].stepY + ".bytesTemp", "Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + obj.getIdManager() + "/treesData_" + obj.altTreesManagerData.patches[i].stepX + "_" + obj.altTreesManagerData.patches[i].stepY + ".bytes");

                                        }
                                        #if UNITY_EDITOR
                                            EditorUtility.DisplayProgressBar("Working ... Please wait ... ", "Working ... Please wait ... ", 0.4f);
                                        #endif
                                        
                                        if(obj.altTreesManagerData.patches[i].treesNoGroupCount == 0)
                                        {
                                            if (obj.altTreesManagerData.patches[i].treesNoGroupData != null)
                                            {
                                                obj.altTreesManagerData.patches[i].treesNoGroupCount = 0;
                                                obj.altTreesManagerData.patches[i].treesNoGroupEmptyCount = 0;

                                                #if UNITY_EDITOR
                                                    AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(obj.altTreesManagerData.patches[i].treesNoGroupData));

                                                    EditorUtility.SetDirty(obj.altTreesManagerData);
                                                    AssetDatabase.SaveAssets();
                                                    AssetDatabase.Refresh();
                                                #endif
                
                                                obj.altTreesManagerData.patches[i].bytesTemp3 = null;
                                            }
                                        }
                                        else
                                        {
                                            if (File.Exists("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + obj.getIdManager() + "/treesData_" + obj.altTreesManagerData.patches[i].stepX + "_" + obj.altTreesManagerData.patches[i].stepY + "_objs.bytesTemp"))
                                                File.Delete("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + obj.getIdManager() + "/treesData_" + obj.altTreesManagerData.patches[i].stepX + "_" + obj.altTreesManagerData.patches[i].stepY + "_objs.bytesTemp");

                                            if (File.Exists("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + obj.getIdManager() + "/treesData_" + obj.altTreesManagerData.patches[i].stepX + "_" + obj.altTreesManagerData.patches[i].stepY + "_objs.bytesTemp.meta"))
                                                File.Delete("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + obj.getIdManager() + "/treesData_" + obj.altTreesManagerData.patches[i].stepX + "_" + obj.altTreesManagerData.patches[i].stepY + "_objs.bytesTemp.meta");

                                            using (BinaryWriter writer = new BinaryWriter(File.Open("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + obj.getIdManager() + "/treesData_" + obj.altTreesManagerData.patches[i].stepX + "_" + obj.altTreesManagerData.patches[i].stepY + "_objs.bytesTemp", FileMode.Create)))
                                            {
                                                int summBytes = 20 + (obj.altTreesManagerData.patches[i].objectsQuadIdTemp - 1) * 4;
                                                byte[] bytesTemp2 = new byte[summBytes];
                                                AltUtilities.WriteBytes(2, bytesTemp2, 0);
                                                AltUtilities.WriteBytes(summBytes, bytesTemp2, 4);
                                                AltUtilities.WriteBytes((obj.altTreesManagerData.patches[i].objectsQuadIdTemp - 1), bytesTemp2, 8);
                                                AltUtilities.WriteBytes(obj.altTreesManagerData.patches[i].treesNoGroupCount, bytesTemp2, 12);
                                                AltUtilities.WriteBytes(0, bytesTemp2, 16);

                                                for (int d = 0; d < (obj.altTreesManagerData.patches[i].objectsQuadIdTemp - 1); d++)
                                                {
                                                    AltUtilities.WriteBytes(-1, bytesTemp2, 20 + d * 4);
                                                }
                                                writer.Write(bytesTemp2);
                    
                                                AltTreesTrees attTemp = null;
                                                int startBytes = 0;

                                                Vector3 vector3Temp;
                                                bool stopTemp = false;
                                                for (int d = 0; d < treesNoGroup.Length; d++)
                                                {
                                                    stopTemp = false;
                                                    vector3Temp = treesNoGroup[d].getPosWorld();
                                                    obj.altTreesManagerData.patches[i].altTreesManager.quads[obj.altTreesManagerData.patches[i].altTreesId].getObjectQuadId(vector3Temp.x, vector3Temp.z, ref treesNoGroup[d].idQuadObject, ref stopTemp);
                                                }

                                                for (int d = 0; d < (obj.altTreesManagerData.patches[i].objectsQuadIdTemp - 1); d++)
                                                {
                                                    int countObjects = 0;
                                                    for (int f = 0; f < treesNoGroup.Length; f++)
                                                    {
                                                        attTemp = treesNoGroup[f];

                                                        if (attTemp.idQuadObject - 1 == d)
                                                        {
                                                            if (countObjects == 0)
                                                            {
                                                                AltUtilities.WriteBytes(summBytes, bytes4Temp, 0);
                                                                writer.Seek(20 + d * 4, SeekOrigin.Begin);
                                                                writer.Write(bytes4Temp);

                                                                writer.Seek(summBytes, SeekOrigin.Begin);
                                                                startBytes = summBytes + 4;

                                                                AltUtilities.WriteBytes(-1, bytes4Temp, 0);
                                                                writer.Write(bytes4Temp);
                                                                AltUtilities.WriteBytes(-1, bytes4Temp, 0);
                                                                writer.Write(bytes4Temp);

                                                                summBytes += 8;
                                                            }

                                                            AltUtilities.WriteBytes(true, bytes61Temp, 0);
                                                            AltUtilities.WriteBytes(attTemp.pos, bytes61Temp, 1);
                                                            AltUtilities.WriteBytes(attTemp.idPrototype, bytes61Temp, 13);
                                                            AltUtilities.WriteBytes(attTemp.color, bytes61Temp, 17);
                                                            AltUtilities.WriteBytes(attTemp.colorBark, bytes61Temp, 33);
                                                            AltUtilities.WriteBytes(attTemp.rotation, bytes61Temp, 49);
                                                            AltUtilities.WriteBytes(attTemp.heightScale, bytes61Temp, 53);
                                                            AltUtilities.WriteBytes(attTemp.widthScale, bytes61Temp, 57);
                                                            writer.Write(bytes61Temp);

                                                            summBytes += 61;
                                                            countObjects++;
                                                        }
                                                    }
                                                    if (countObjects != 0)
                                                    {
                                                        writer.Seek(startBytes, SeekOrigin.Begin);
                                                        AltUtilities.WriteBytes(countObjects, bytes4Temp, 0);
                                                        writer.Write(bytes4Temp);
                                                    }
                                                }

                                                writer.Seek(4, SeekOrigin.Begin);
                                                AltUtilities.WriteBytes(summBytes, bytes4Temp, 0);
                                                writer.Write(bytes4Temp);
                                            }

                                            if (File.Exists("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + obj.getIdManager() + "/treesData_" + obj.altTreesManagerData.patches[i].stepX + "_" + obj.altTreesManagerData.patches[i].stepY + "_objs.bytes"))
                                                File.Replace("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + obj.getIdManager() + "/treesData_" + obj.altTreesManagerData.patches[i].stepX + "_" + obj.altTreesManagerData.patches[i].stepY + "_objs.bytesTemp", "Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + obj.getIdManager() + "/treesData_" + obj.altTreesManagerData.patches[i].stepX + "_" + obj.altTreesManagerData.patches[i].stepY + "_objs.bytes", null);
                                            else
                                                File.Move("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + obj.getIdManager() + "/treesData_" + obj.altTreesManagerData.patches[i].stepX + "_" + obj.altTreesManagerData.patches[i].stepY + "_objs.bytesTemp", "Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + obj.getIdManager() + "/treesData_" + obj.altTreesManagerData.patches[i].stepX + "_" + obj.altTreesManagerData.patches[i].stepY + "_objs.bytes");
                                        }

                                        #if UNITY_EDITOR
                                            AssetDatabase.SaveAssets();
                                            AssetDatabase.Refresh();
                                            EditorUtility.SetDirty(obj.altTreesManagerData);
                    
                                            EditorUtility.ClearProgressBar();
                                        #endif
                                        
                                        if (obj.altTreesManagerData.patches[i].treesNoGroupData != null)
                                        {
                                            string pathStr2 = "";
                                            pathStr2 = AssetDatabase.GetAssetPath(obj.altTreesManagerData.patches[i].treesNoGroupData);
                                            if (pathStr2 != "")
                                                obj.altTreesManagerData.patches[i].bytesTemp3 = File.ReadAllBytes(pathStr2);
                                            else
                                            {
                                                obj.altTreesManagerData.patches[i].bytesTemp3 = obj.altTreesManagerData.patches[i].treesNoGroupData.bytes;
                                            }
                                        }
                                        else
                                            obj.altTreesManagerData.patches[i].bytesTemp3 = null;
                
                                        AltTreePrototypes[] atpTemp = obj.altTreesManagerData.patches[i].prototypes;
                                        int prId = 0;

                                        for (int d = 0; d < obj.altTreesManagerData.patches[i].prototypes.Length; d++)
                                        {
                                            if(obj.altTreesManagerData.patches[i].prototypes[d] == null || obj.altTreesManagerData.patches[i].prototypes[d].tree == null || obj.altTreesManagerData.patches[i].prototypes[d].tree.id == prototypesIds[h])
                                            {
                                                prId++;
                                            }
                                        }

                                        obj.altTreesManagerData.patches[i].prototypes = new AltTreePrototypes[atpTemp.Length - prId];
                                        prId = 0;
                                        for (int d = 0; d < atpTemp.Length; d++)
                                        {
                                            if (atpTemp[d] != null && atpTemp[d].tree != null && atpTemp[d].tree.id != prototypesIds[h])
                                            {
                                                obj.altTreesManagerData.patches[i].prototypes[prId] = atpTemp[d];
                                                prId++;
                                            }
                                        }
                                        selectedPatch = -1;

                                        if (obj.altTreesManagerData.patches[i].treesCount == 0 && obj.altTreesManagerData.patches[i].treesNoGroupCount == 0)
                                            obj.removePatch(obj.altTreesManagerData.patches[i]);


                                        bool stop = false;
                                        int idProt = -1;
                                        for(int d = 0; d < obj.altTreesManagerData.patches[i].prototypes.Length; d++)
                                        {
                                            if (obj.altTreesManagerData.patches[i].prototypes[d].tree.id == prototypesIds[h])
                                                idProt = d;
                                            if (obj.altTreesManagerData.patches[i].prototypes[d].tree.id == atTemp.id)
                                                stop = true;
                                        }

                                        if(idProt != -1 && !stop)
                                        {
                                            obj.altTreesManagerData.patches[i].prototypes[idProt].tree = atTemp;
                                            obj.altTreesManagerData.patches[i].prototypes[idProt].isObject = atTemp.isObject;
                                        }
                                        else if (idProt != -1)
                                        {
                                            obj.altTreesManagerData.patches[i].prototypes[idProt].tree = null;
                                        }
                                        else if (!stop)
                                        {
                                            AltTreePrototypes[] atprotTemp = obj.altTreesManagerData.patches[i].prototypes;
                                            obj.altTreesManagerData.patches[i].prototypes = new AltTreePrototypes[atprotTemp.Length + 1];

                                            for (int d = 0; d < atprotTemp.Length; d++)
                                            {
                                                obj.altTreesManagerData.patches[i].prototypes[d] = atprotTemp[d];
                                            }
                                            obj.altTreesManagerData.patches[i].prototypes[atprotTemp.Length] = new AltTreePrototypes();
                                            obj.altTreesManagerData.patches[i].prototypes[atprotTemp.Length].tree = atTemp;
                                            obj.altTreesManagerData.patches[i].prototypes[atprotTemp.Length].isObject = atTemp.isObject;
                                        }
                                        
                                        EditorUtility.SetDirty(obj.altTreesManagerData);
                                        AssetDatabase.SaveAssets();
                                        AssetDatabase.Refresh();
                                        obj.ReInit(true);








                                    }


                                    //GUILayout.Label(prototypesIds[h] + "");
                                    GUI.Label(new Rect(scaleRect.x + 2, scaleRect.y + 44 + 90f * h, 120, 25), prototypesIds[h] + "");

                                    content.text = (prototypesCount[h] - prototypesEmpty[h]) + "";
                                    size = st.CalcSize(content);

                                    GUI.Label(new Rect(scaleRect.x + ((scaleRect.width - 5f) / 5f) * 1f + 22f - size.x / 2f, scaleRect.y + 44 + 90f * h, 120, 25), content);
                                    //GUILayout.Label((prototypesCount[h] - prototypesEmpty[h]) + "");

                                    content.text = prototypesEmpty[h] + "";
                                    size = st.CalcSize(content);


                                    GUI.Label(new Rect(scaleRect.x + ((scaleRect.width - 5f) / 5f) * 2f + 27f - size.x / 2f, scaleRect.y + 44 + 90f * h, 120, 25), content);
                                    //GUILayout.Label(prototypesEmpty[h] + "");
                                    
                                    if(GUI.Button(new Rect(scaleRect.x + scaleRect.width - 60, scaleRect.y + 44 + 90f * h, 50, 16), "Delete") && EditorUtility.DisplayDialog("Delete This Prototype?", "Are you sure you want to Delete All Trees with this Prototype? ", "Delete", "Cancel"))
                                    {













                                        
                                        #if UNITY_EDITOR
                                            EditorUtility.DisplayProgressBar("Working ... Please wait ... 0%", "Working ... Please wait ... ", 0.0f);
                                        #endif

                                        float schProgress = 0;
                                        float treesCountProgress = 0;

                                        AltTreesTrees[] treesTemp = obj.altTreesManagerData.patches[i].trees;
                                        //AltTreesTrees[] objectsTemp = new AltTreesTrees[treesNoGroupCount - treesNoGroupEmptyCount];
                                        AltTreesTrees[] objectsTemp = new AltTreesTrees[obj.altTreesManagerData.patches[i].treesNoGroupCount];

                                        int objectsSch = 0;
                                        int countQuads = 0;

                                        int count = 0;

                                        if (obj.altTreesManagerData.patches[i].bytesTemp3 != null && obj.altTreesManagerData.patches[i].bytesTemp3.Length > 0)
                                        {
                                            int version = AltUtilities.ReadBytesInt(obj.altTreesManagerData.patches[i].bytesTemp3, 0);

                                            Vector3 _pos = new Vector3();
                                            int _idPrototype;
                                            Color _color = new Color();
                                            Color _colorBark = new Color();
                                            float _rotation;
                                            float _heightScale;
                                            float _widthScale;
                                            int objectsSch2 = 0;

                                            countQuads = AltUtilities.ReadBytesInt(obj.altTreesManagerData.patches[i].bytesTemp3, 8);

                                            treesCountProgress = objectsTemp.Length;

                                            if (version == 2)
                                            {
                                                if (countQuads == obj.altTreesManagerData.patches[i].objectsQuadIdTemp - 1)
                                                {
                                                    for (int t = 0; t < countQuads; t++)
                                                    {
                                                        int addrObjs = AltUtilities.ReadBytesInt(obj.altTreesManagerData.patches[i].bytesTemp3, 20 + t * 4);
                                                        int countObjs = 0;
                                                        objectsSch2 = 0;

                                                        while (addrObjs != -1)
                                                        {
                                                            countObjs = AltUtilities.ReadBytesInt(obj.altTreesManagerData.patches[i].bytesTemp3, addrObjs + 4);

                                                            if (countObjs > 0)
                                                            {
                                                                for (int d = 0; d < countObjs; d++)
                                                                {
                                                                    if (AltUtilities.ReadBytesBool(obj.altTreesManagerData.patches[i].bytesTemp3, addrObjs + 8 + d * 61 + 0))
                                                                    {
                                                                        _pos = AltUtilities.ReadBytesVector3(obj.altTreesManagerData.patches[i].bytesTemp3, addrObjs + 8 + d * 61 + 1);
                                                                        _idPrototype = AltUtilities.ReadBytesInt(obj.altTreesManagerData.patches[i].bytesTemp3, addrObjs + 8 + d * 61 + 13);
                                                                        _color = AltUtilities.ReadBytesColor(obj.altTreesManagerData.patches[i].bytesTemp3, addrObjs + 8 + d * 61 + 17);
                                                                        _colorBark = AltUtilities.ReadBytesColor(obj.altTreesManagerData.patches[i].bytesTemp3, addrObjs + 8 + d * 61 + 33);
                                                                        _rotation = AltUtilities.ReadBytesFloat(obj.altTreesManagerData.patches[i].bytesTemp3, addrObjs + 8 + d * 61 + 49);
                                                                        _heightScale = AltUtilities.ReadBytesFloat(obj.altTreesManagerData.patches[i].bytesTemp3, addrObjs + 8 + d * 61 + 53);
                                                                        _widthScale = AltUtilities.ReadBytesFloat(obj.altTreesManagerData.patches[i].bytesTemp3, addrObjs + 8 + d * 61 + 57);

                                                                        AltTreesTrees att = new AltTreesTrees(_pos, objectsSch2, _idPrototype, true, _color, _colorBark, _rotation, _heightScale, _widthScale, obj.altTreesManagerData.patches[i]);

                                                                        att.idQuadObject = t;
                                                                        objectsTemp[objectsSch] = att;
                                                                        objectsSch++;
                                                                        objectsSch2++;

                                                                        if (att.idPrototype == prototypesIds[h])
                                                                        {
                                                                            count++;
                                                                        }

                                                                        schProgress++;
                                                                        if (schProgress > treesCountProgress / 20f)
                                                                        {
                                                                            EditorUtility.DisplayProgressBar("Working ... Please wait ...  " + Mathf.FloorToInt((objectsSch / treesCountProgress) * 40f) + "%", "Working ... Please wait ... ", (objectsSch / treesCountProgress) /2.5f);

                                                                            schProgress = 0;
                                                                        }

                                                                    }
                                                                }
                                                            }
                                                            addrObjs = AltUtilities.ReadBytesInt(obj.altTreesManagerData.patches[i].bytesTemp3, addrObjs);
                                                        }
                                                    }
                                                }
                                                else
                                                    obj.LogError("Count Quads in file(" + AltUtilities.ReadBytesInt(obj.altTreesManagerData.patches[i].bytesTemp3, 8) + ") != Count Quads in Manager(" + (obj.altTreesManagerData.patches[i].objectsQuadIdTemp - 1) + ")");
                                            }
                                            else
                                                obj.LogError("Version == "+version);
                                        }
                                        //altTrees.LogError("treesNoGroupCount(" + treesNoGroupCount + ") != treesNoGroupEmptyCount(" + treesNoGroupEmptyCount + ")");

                                        obj.altTreesManagerData.patches[i].treesNoGroupCount = objectsSch;
                                        obj.altTreesManagerData.patches[i].treesNoGroupEmptyCount = 0;

                                        //if (objectsTemp.Length != treesNoGroupCount)
                                        //    altTrees.LogError("objectsTemp.Length("+ objectsTemp.Length + ") != treesNoGroupCount("+ treesNoGroupCount + ")");

                                        AltTreesTrees[] treesNoGroup = null;

                                        #if UNITY_EDITOR
                                            EditorUtility.DisplayProgressBar("Working ... Please wait ... 42%", "Working ... Please wait ... ", 0.42f);
                                        #endif

                                        bool boolTemp = false;

                                        
                                        treesNoGroup = new AltTreesTrees[obj.altTreesManagerData.patches[i].treesNoGroupCount - count];

                                        
                                        int treesNoGroupIndx = 0;

                                        for (int d = 0; d < obj.altTreesManagerData.patches[i].treesNoGroupCount; d++)
                                        {
                                            if (objectsTemp[d].idPrototype != prototypesIds[h])
                                            {
                                                treesNoGroup[treesNoGroupIndx] = objectsTemp[d];
                                                treesNoGroupIndx++;
                                            }
                                        }

                                        count = 0;

                                        for (int d = 0; d < treesTemp.Length; d++)
                                        {
                                            if (treesTemp[d] != null && treesTemp[d].idPrototype == prototypesIds[h])
                                            {
                                                boolTemp = false;
                                                for (int j = 0; j < obj.altTreesManagerData.patches[i].treesEmptyCount; j++)
                                                {
                                                    if (obj.altTreesManagerData.patches[i].treesEmpty[j] == d)
                                                    {
                                                        boolTemp = true;
                                                        break;
                                                    }
                                                }
                                                if (!boolTemp)
                                                    count++;
                                            }
                                        }
                                        obj.altTreesManagerData.patches[i].trees = new AltTreesTrees[obj.altTreesManagerData.patches[i].treesCount - obj.altTreesManagerData.patches[i].treesEmptyCount - count];


                                        int treesIndx = 0;

                                        for (int d = 0; d < treesTemp.Length; d++)
                                        {
                                            boolTemp = false;
                                            for (int j = 0; j < obj.altTreesManagerData.patches[i].treesEmptyCount; j++)
                                            {
                                                if (obj.altTreesManagerData.patches[i].treesEmpty[j] == d)
                                                {
                                                    boolTemp = true;
                                                    break;
                                                }
                                            }
                                            if (!boolTemp)
                                            {
                                                if (treesTemp[d] != null && treesTemp[d].idPrototype != prototypesIds[h])
                                                {
                                                    obj.altTreesManagerData.patches[i].trees[treesIndx] = treesTemp[d];
                                                    treesIndx++;
                                                }
                                            }
                                        }



                                        obj.altTreesManagerData.patches[i].treesCount = obj.altTreesManagerData.patches[i].trees.Length;
                                        obj.altTreesManagerData.patches[i].treesEmptyCount = 0;
                                        obj.altTreesManagerData.patches[i].treesEmpty = new int[0];

                                        obj.altTreesManagerData.patches[i].treesNoGroupCount = treesNoGroup.Length;
                                        obj.altTreesManagerData.patches[i].treesNoGroupEmptyCount = 0;

                                        byte[] bytes4Temp = new byte[4];
                                        byte[] bytes60Temp = new byte[60];
                                        byte[] bytes61Temp = new byte[61];

                                        #if UNITY_EDITOR
                                            EditorUtility.DisplayProgressBar("Working ... Please wait ... 44%", "Working ... Please wait ... ", 0.44f);
                                        #endif

                                        if(obj.altTreesManagerData.patches[i].treesCount == 0)
                                        { 
                                            if (obj.altTreesManagerData.patches[i].treesData != null)
                                            {
                                                obj.altTreesManagerData.patches[i].trees = new AltTreesTrees[0];
                                                obj.altTreesManagerData.patches[i].treesCount = 0;
                                                obj.altTreesManagerData.patches[i].treesEmptyCount = 0;
                                                obj.altTreesManagerData.patches[i].treesEmpty = new int[0];

                                                #if UNITY_EDITOR
                                                    AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(obj.altTreesManagerData.patches[i].treesData));

                                                    EditorUtility.SetDirty(obj.altTreesManagerData);
                                                    AssetDatabase.SaveAssets();
                                                    AssetDatabase.Refresh();
                                                #endif
                                            }
                                        }
                                        else
                                        { 
                                            if (File.Exists("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + obj.getIdManager() + "/treesData_" + obj.altTreesManagerData.patches[i].stepX + "_" + obj.altTreesManagerData.patches[i].stepY + ".bytesTemp"))
                                                File.Delete("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + obj.getIdManager() + "/treesData_" + obj.altTreesManagerData.patches[i].stepX + "_" + obj.altTreesManagerData.patches[i].stepY + ".bytesTemp");

                                            if (File.Exists("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + obj.getIdManager() + "/treesData_" + obj.altTreesManagerData.patches[i].stepX + "_" + obj.altTreesManagerData.patches[i].stepY + ".bytesTemp.meta"))
                                                File.Delete("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + obj.getIdManager() + "/treesData_" + obj.altTreesManagerData.patches[i].stepX + "_" + obj.altTreesManagerData.patches[i].stepY + ".bytesTemp.meta");

                                            using (BinaryWriter writer = new BinaryWriter(File.Open("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + obj.getIdManager() + "/treesData_" + obj.altTreesManagerData.patches[i].stepX + "_" + obj.altTreesManagerData.patches[i].stepY + ".bytesTemp", FileMode.Create)))
                                            {
                                                AltUtilities.WriteBytes(1, bytes4Temp, 0);
                                                writer.Write(bytes4Temp);
                                                AltUtilities.WriteBytes(obj.altTreesManagerData.patches[i].treesCount, bytes4Temp, 0);
                                                writer.Write(bytes4Temp);
                                                AltUtilities.WriteBytes(0, bytes4Temp, 0);
                                                writer.Write(bytes4Temp);
                    
                                                for (int d = 0; d < obj.altTreesManagerData.patches[i].treesCount; d++)
                                                {
                                                    AltUtilities.WriteBytes(obj.altTreesManagerData.patches[i].trees[d].pos, bytes60Temp, 0);
                                                    AltUtilities.WriteBytes(obj.altTreesManagerData.patches[i].trees[d].idPrototype, bytes60Temp, 12);
                                                    AltUtilities.WriteBytes(obj.altTreesManagerData.patches[i].trees[d].color, bytes60Temp, 16);
                                                    AltUtilities.WriteBytes(obj.altTreesManagerData.patches[i].trees[d].colorBark, bytes60Temp, 32);
                                                    AltUtilities.WriteBytes(obj.altTreesManagerData.patches[i].trees[d].rotation, bytes60Temp, 48);
                                                    AltUtilities.WriteBytes(obj.altTreesManagerData.patches[i].trees[d].heightScale, bytes60Temp, 52);
                                                    AltUtilities.WriteBytes(obj.altTreesManagerData.patches[i].trees[d].widthScale, bytes60Temp, 56);
                                                    writer.Write(bytes60Temp);
                                                }
                                            }

                                            if (File.Exists("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + obj.getIdManager() + "/treesData_" + obj.altTreesManagerData.patches[i].stepX + "_" + obj.altTreesManagerData.patches[i].stepY + ".bytes"))
                                                File.Replace("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + obj.getIdManager() + "/treesData_" + obj.altTreesManagerData.patches[i].stepX + "_" + obj.altTreesManagerData.patches[i].stepY + ".bytesTemp", "Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + obj.getIdManager() + "/treesData_" + obj.altTreesManagerData.patches[i].stepX + "_" + obj.altTreesManagerData.patches[i].stepY + ".bytes", null);
                                            else
                                                File.Move("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + obj.getIdManager() + "/treesData_" + obj.altTreesManagerData.patches[i].stepX + "_" + obj.altTreesManagerData.patches[i].stepY + ".bytesTemp", "Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + obj.getIdManager() + "/treesData_" + obj.altTreesManagerData.patches[i].stepX + "_" + obj.altTreesManagerData.patches[i].stepY + ".bytes");
                                        }
                                        #if UNITY_EDITOR
                                            EditorUtility.DisplayProgressBar("Working ... Please wait ... 45%", "Working ... Please wait ... ", 0.45f);
                                        #endif
                                        
                                        if(obj.altTreesManagerData.patches[i].treesNoGroupCount == 0)
                                        {
                                            if (obj.altTreesManagerData.patches[i].treesNoGroupData != null)
                                            {
                                                obj.altTreesManagerData.patches[i].treesNoGroupCount = 0;
                                                obj.altTreesManagerData.patches[i].treesNoGroupEmptyCount = 0;

                                                #if UNITY_EDITOR
                                                    AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(obj.altTreesManagerData.patches[i].treesNoGroupData));

                                                    EditorUtility.SetDirty(obj.altTreesManagerData);
                                                    AssetDatabase.SaveAssets();
                                                    AssetDatabase.Refresh();
                                                #endif
                
                                                obj.altTreesManagerData.patches[i].bytesTemp3 = null;
                                            }
                                        }
                                        else
                                        { 
                                            if (File.Exists("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + obj.getIdManager() + "/treesData_" + obj.altTreesManagerData.patches[i].stepX + "_" + obj.altTreesManagerData.patches[i].stepY + "_objs.bytesTemp"))
                                                File.Delete("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + obj.getIdManager() + "/treesData_" + obj.altTreesManagerData.patches[i].stepX + "_" + obj.altTreesManagerData.patches[i].stepY + "_objs.bytesTemp");

                                            if (File.Exists("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + obj.getIdManager() + "/treesData_" + obj.altTreesManagerData.patches[i].stepX + "_" + obj.altTreesManagerData.patches[i].stepY + "_objs.bytesTemp.meta"))
                                                File.Delete("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + obj.getIdManager() + "/treesData_" + obj.altTreesManagerData.patches[i].stepX + "_" + obj.altTreesManagerData.patches[i].stepY + "_objs.bytesTemp.meta");


                                            using (BinaryWriter writer = new BinaryWriter(File.Open("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + obj.getIdManager() + "/treesData_" + obj.altTreesManagerData.patches[i].stepX + "_" + obj.altTreesManagerData.patches[i].stepY + "_objs.bytesTemp", FileMode.Create)))
                                            {
                                                int summBytes = 20 + (obj.altTreesManagerData.patches[i].objectsQuadIdTemp - 1) * 4;
                                                byte[] bytesTemp2 = new byte[summBytes];
                                                AltUtilities.WriteBytes(2, bytesTemp2, 0);
                                                AltUtilities.WriteBytes(summBytes, bytesTemp2, 4);
                                                AltUtilities.WriteBytes((obj.altTreesManagerData.patches[i].objectsQuadIdTemp - 1), bytesTemp2, 8);
                                                AltUtilities.WriteBytes(obj.altTreesManagerData.patches[i].treesNoGroupCount, bytesTemp2, 12);
                                                AltUtilities.WriteBytes(0, bytesTemp2, 16);

                                                for (int d = 0; d < (obj.altTreesManagerData.patches[i].objectsQuadIdTemp - 1); d++)
                                                {
                                                    AltUtilities.WriteBytes(-1, bytesTemp2, 20 + d * 4);
                                                }
                                                writer.Write(bytesTemp2);
                    
                                                AltTreesTrees attTemp = null;
                                                int startBytes = 0;

                                                Dictionary<int, List<AltTreesTrees>> idQuadsTemp = new Dictionary<int, List<AltTreesTrees>>();

                                                Vector3 vector3Temp;
                                                bool stopTemp = false;
                                                for (int d = 0; d < treesNoGroup.Length; d++)
                                                {
                                                    stopTemp = false;
                                                    vector3Temp = treesNoGroup[d].getPosWorld();
                                                    obj.altTreesManagerData.patches[i].altTreesManager.quads[obj.altTreesManagerData.patches[i].altTreesId].getObjectQuadId(vector3Temp.x, vector3Temp.z, ref treesNoGroup[d].idQuadObject, ref stopTemp);


                                                    if (treesNoGroup[d].idQuadObject == -1)
                                                    {
                                                        vector3Temp.x += 0.01f;
                                                        vector3Temp.z += 0.01f;
                                                        stopTemp = false;
                                                        obj.altTreesManagerData.patches[i].altTreesManager.quads[obj.altTreesManagerData.patches[i].altTreesId].getObjectQuadId(vector3Temp.x, vector3Temp.z, ref treesNoGroup[d].idQuadObject, ref stopTemp);
                                                        if (treesNoGroup[d].idQuadObject == -1)
                                                        {
                                                            Debug.LogWarning("treesNoGroup[d].idQuadObject == -1");
                                                            continue;
                                                        }
                                                    }

                                                    if (!idQuadsTemp.ContainsKey(treesNoGroup[d].idQuadObject - 1))
                                                    {
                                                        List<AltTreesTrees> attListT = new List<AltTreesTrees>();
                                                        attListT.Add(treesNoGroup[d]);
                                                        idQuadsTemp.Add(treesNoGroup[d].idQuadObject - 1, attListT);
                                                    }
                                                    else
                                                    {
                                                        idQuadsTemp[treesNoGroup[d].idQuadObject - 1].Add(treesNoGroup[d]);
                                                    }
                                                }

                                                EditorUtility.DisplayProgressBar("Working ... Please wait ... 50%", "Working ... Please wait ... ", 0.5f);

                                                //Debug.Log("= "+ idQuadsTemp.Keys.Count);

                                                schProgress = 0;
                                                treesCountProgress = treesNoGroup.Length;
                                                objectsSch = 0;

                                                foreach (int d in idQuadsTemp.Keys)
                                                {
                                                    List<AltTreesTrees> attArr = idQuadsTemp[d];
                                                    //Debug.Log(" " + attArr.Count);
                                                    int countObjs = 0;
                                                    int valueCount = attArr.Count;
                                                    for (int f = 0; f < valueCount; f++)
                                                    {
                                                        attTemp = attArr[f];

                                                        if (attTemp.idQuadObject - 1 == d)
                                                        {
                                                            if (countObjs == 0)
                                                            {
                                                                AltUtilities.WriteBytes(summBytes, bytes4Temp, 0);
                                                                writer.Seek(20 + d * 4, SeekOrigin.Begin);
                                                                writer.Write(bytes4Temp);

                                                                writer.Seek(summBytes, SeekOrigin.Begin);
                                                                startBytes = summBytes + 4;

                                                                AltUtilities.WriteBytes(-1, bytes4Temp, 0);
                                                                writer.Write(bytes4Temp);
                                                                AltUtilities.WriteBytes(-1, bytes4Temp, 0);
                                                                writer.Write(bytes4Temp);

                                                                summBytes += 8;
                                                            }

                                                            AltUtilities.WriteBytes(true, bytes61Temp, 0);
                                                            AltUtilities.WriteBytes(attTemp.pos, bytes61Temp, 1);
                                                            AltUtilities.WriteBytes(attTemp.idPrototype, bytes61Temp, 13);
                                                            AltUtilities.WriteBytes(attTemp.color, bytes61Temp, 17);
                                                            AltUtilities.WriteBytes(attTemp.colorBark, bytes61Temp, 33);
                                                            AltUtilities.WriteBytes(attTemp.rotation, bytes61Temp, 49);
                                                            AltUtilities.WriteBytes(attTemp.heightScale, bytes61Temp, 53);
                                                            AltUtilities.WriteBytes(attTemp.widthScale, bytes61Temp, 57);
                                                            writer.Write(bytes61Temp);

                                                            summBytes += 61;
                                                            countObjs++;

                                                            objectsSch++;

                                                            schProgress++;
                                                            if (schProgress > treesCountProgress / 20f)
                                                            {
                                                                EditorUtility.DisplayProgressBar("Working ... Please wait ...  " + Mathf.FloorToInt((objectsSch / treesCountProgress) * 40f + 50f) + "%", "Working ... Please wait ... ", (objectsSch / treesCountProgress) / 2.5f + 0.5f);

                                                                schProgress = 0;
                                                            }
                                                        }
                                                    }
                                                    if (countObjs != 0)
                                                    {
                                                        writer.Seek(startBytes, SeekOrigin.Begin);
                                                        AltUtilities.WriteBytes(countObjs, bytes4Temp, 0);
                                                        writer.Write(bytes4Temp);
                                                    }
                                                }

                                                writer.Seek(4, SeekOrigin.Begin);
                                                AltUtilities.WriteBytes(summBytes, bytes4Temp, 0);
                                                writer.Write(bytes4Temp);

                                                idQuadsTemp.Clear();
                                            }

                                            if (File.Exists("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + obj.getIdManager() + "/treesData_" + obj.altTreesManagerData.patches[i].stepX + "_" + obj.altTreesManagerData.patches[i].stepY + "_objs.bytes"))
                                                File.Replace("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + obj.getIdManager() + "/treesData_" + obj.altTreesManagerData.patches[i].stepX + "_" + obj.altTreesManagerData.patches[i].stepY + "_objs.bytesTemp", "Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + obj.getIdManager() + "/treesData_" + obj.altTreesManagerData.patches[i].stepX + "_" + obj.altTreesManagerData.patches[i].stepY + "_objs.bytes", null);
                                            else
                                                File.Move("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + obj.getIdManager() + "/treesData_" + obj.altTreesManagerData.patches[i].stepX + "_" + obj.altTreesManagerData.patches[i].stepY + "_objs.bytesTemp", "Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + obj.getIdManager() + "/treesData_" + obj.altTreesManagerData.patches[i].stepX + "_" + obj.altTreesManagerData.patches[i].stepY + "_objs.bytes");
                                        }

                                        EditorUtility.DisplayProgressBar("Working ... Please wait ... 90%", "Working ... Please wait ... ", 0.9f);
                                        
                                        AssetDatabase.SaveAssets();
                                        AssetDatabase.Refresh();
                                        EditorUtility.SetDirty(obj.altTreesManagerData);
                    
                                        EditorUtility.ClearProgressBar();


                                        if (obj.altTreesManagerData.patches[i].treesNoGroupData != null)
                                        {
                                            string pathStr2 = "";
                                            pathStr2 = AssetDatabase.GetAssetPath(obj.altTreesManagerData.patches[i].treesNoGroupData);
                                            if (pathStr2 != "")
                                                obj.altTreesManagerData.patches[i].bytesTemp3 = File.ReadAllBytes(pathStr2);
                                            else
                                            {
                                                obj.altTreesManagerData.patches[i].bytesTemp3 = obj.altTreesManagerData.patches[i].treesNoGroupData.bytes;
                                            }
                                        }
                                        else
                                            obj.altTreesManagerData.patches[i].bytesTemp3 = null;
                
                                        AltTreePrototypes[] atpTemp = obj.altTreesManagerData.patches[i].prototypes;
                                        int prId = 0;

                                        for (int d = 0; d < obj.altTreesManagerData.patches[i].prototypes.Length; d++)
                                        {
                                            if(obj.altTreesManagerData.patches[i].prototypes[d] == null || obj.altTreesManagerData.patches[i].prototypes[d].tree == null || obj.altTreesManagerData.patches[i].prototypes[d].tree.id == prototypesIds[h])
                                            {
                                                prId++;
                                            }
                                        }

                                        obj.altTreesManagerData.patches[i].prototypes = new AltTreePrototypes[atpTemp.Length - prId];
                                        prId = 0;
                                        for (int d = 0; d < atpTemp.Length; d++)
                                        {
                                            if (atpTemp[d] != null && atpTemp[d].tree != null && atpTemp[d].tree.id != prototypesIds[h])
                                            {
                                                obj.altTreesManagerData.patches[i].prototypes[prId] = atpTemp[d];
                                                prId++;
                                            }
                                        }
                                        selectedPatch = -1;

                                        if (obj.altTreesManagerData.patches[i].treesCount == 0 && obj.altTreesManagerData.patches[i].treesNoGroupCount == 0)
                                            obj.removePatch(obj.altTreesManagerData.patches[i]);

                                        obj.ReInit();








                                    }


                                }


                                GUILayout.EndVertical();
                            }

                        }
                    }
                    GUILayout.EndScrollView();

                    GUILayout.BeginHorizontal();
                    {
                        if (!(obj.isActiveAndEnabled && obj.altTreesManagerData.draw))
                            GUI.enabled = false;
                        if (GUILayout.Button("Delete All Trees") )
                        {
                            if(EditorUtility.DisplayDialog("Delete All Trees?", "Are you sure you want to Delete All Trees? ", "Delete", "Cancel"))
                            {
                                for (int i = 0; i < obj.altTreesManagerData.patches.Length; i++)
                                {
                                    AltTreesPatch atp = obj.altTreesManagerData.patches[i];

                                    if (atp.treesNoGroupCount == atp.treesNoGroupEmptyCount)
                                    {
                                        int count = 0;
                                        AltTreesPatch[] patchesTemp = obj.altTreesManagerData.patches;
                                        obj.altTreesManagerData.patches = new AltTreesPatch[patchesTemp.Length - 1];
                                        for (int j = 0; j < patchesTemp.Length; j++)
                                        {
                                            if (!patchesTemp[j].Equals(atp))
                                            {
                                                obj.altTreesManagerData.patches[count] = patchesTemp[j];
                                                count++;
                                            }
                                        }

                                        if (atp.treesData != null)
                                            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(atp.treesData));
                                        if (atp.treesNoGroupData != null)
                                            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(atp.treesNoGroupData));
                                        obj.altTreesManager.removeAltTrees(atp, false);
                                    }
                                    else
                                    {
                                        if (atp.treesData != null)
                                            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(atp.treesData));

                                        atp.treesEmptyCount = 0;
                                        atp.treesCount = 0;
                                        atp.trees = new AltTreesTrees[0];
                                        atp.treesEmpty = new int[0];

                                        int tempInt = 0;

                                        for(int p = 0; p < atp.prototypes.Length; p++)
                                        {
                                            if (!atp.prototypes[p].isObject)
                                                tempInt++;
                                        }

                                        if(tempInt != 0)
                                        {
                                            AltTreePrototypes[] atpTemp = atp.prototypes;
                                            atp.prototypes = new AltTreePrototypes[atpTemp.Length - tempInt];

                                            tempInt = 0;
                                            for (int p = 0; p < atpTemp.Length; p++)
                                            {
                                                if (atpTemp[p].isObject)
                                                {
                                                    atp.prototypes[tempInt] = atpTemp[p];
                                                    tempInt++;
                                                }
                                            }
                                        }
                                    }
                                }

                                EditorUtility.SetDirty(obj.altTreesManagerData);
                                AssetDatabase.SaveAssets();
                                AssetDatabase.Refresh();

                                obj.ReInit(true);
                                return;
                            }
                        }
                        GUILayout.Space(15);
                        if (GUILayout.Button("Delete All Objects"))
                        {
                            if (EditorUtility.DisplayDialog("Delete All Objects?", "Are you sure you want to Delete All Objects? ", "Delete", "Cancel"))
                            {
                                for (int i = 0; i < obj.altTreesManagerData.patches.Length; i++)
                                {
                                    AltTreesPatch atp = obj.altTreesManagerData.patches[i];

                                    if (atp.treesCount == atp.treesEmptyCount)
                                    {
                                        int count = 0;
                                        AltTreesPatch[] patchesTemp = obj.altTreesManagerData.patches;
                                        obj.altTreesManagerData.patches = new AltTreesPatch[patchesTemp.Length - 1];
                                        for (int j = 0; j < patchesTemp.Length; j++)
                                        {
                                            if (!patchesTemp[j].Equals(atp))
                                            {
                                                obj.altTreesManagerData.patches[count] = patchesTemp[j];
                                                count++;
                                            }
                                        }

                                        if (atp.treesData != null)
                                            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(atp.treesData));
                                        if (atp.treesNoGroupData != null)
                                            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(atp.treesNoGroupData));
                                        obj.altTreesManager.removeAltTrees(atp, false);
                                    }
                                    else
                                    {
                                        if (atp.treesNoGroupData != null)
                                            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(atp.treesNoGroupData));

                                        atp.treesNoGroupEmptyCount = 0;
                                        atp.treesNoGroupCount = 0;

                                        int tempInt = 0;

                                        for (int p = 0; p < atp.prototypes.Length; p++)
                                        {
                                            if (atp.prototypes[p].isObject)
                                                tempInt++;
                                        }

                                        if (tempInt != 0)
                                        {
                                            AltTreePrototypes[] atpTemp = atp.prototypes;
                                            atp.prototypes = new AltTreePrototypes[atpTemp.Length - tempInt];

                                            tempInt = 0;
                                            for (int p = 0; p < atpTemp.Length; p++)
                                            {
                                                if (!atpTemp[p].isObject)
                                                {
                                                    atp.prototypes[tempInt] = atpTemp[p];
                                                    tempInt++;
                                                }
                                            }
                                        }
                                    }
                                }

                                EditorUtility.SetDirty(obj.altTreesManagerData);
                                AssetDatabase.SaveAssets();
                                AssetDatabase.Refresh();

                                obj.ReInit(true);
                                return;
                            }
                        }
                        GUI.enabled = true;
                    }
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndVertical();

                EditorGUILayout.Space();
            }
            else if (menuId.intValue == 3)
            {
                EditorGUILayout.LabelField("ID Manager:    <b>" + obj.getIdManager() + "</b>", sty4);

                GUILayout.Space(5);

                GUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    GUILayout.Label("Settings", sty5);


                    bool starDraw = draw;

                    draw = GUILayout.Toggle(draw, "Draw");
                    enableColliders = GUILayout.Toggle(enableColliders, "Enable Tree Colliders");
                    if(!enableColliders)
                    {
                        colliderEvents = false;
                        GUI.enabled = false;
                    }
                    colliderEvents = GUILayout.Toggle(colliderEvents, "Enable Collider Events");
                    GUI.enabled = true;

                    //generateAllBillboardsOnStart = GUILayout.Toggle(generateAllBillboardsOnStart, "Generate All Billboards On Start");

                    multiThreading = GUILayout.Toggle(multiThreading, "MultiThreading");

                    if (multiThreadingStar != multiThreading)
                    {
                        obj.altTreesManagerData.multiThreading = multiThreading;
                        multiThreadingStar = multiThreading;

                        if(obj.enabled && obj.gameObject.activeInHierarchy)
                            obj.ReInit(true);
                        return;
                    }

                    frustumCullingMultiThreading = GUILayout.Toggle(frustumCullingMultiThreading, "Frustum Culling MultiThreading");

                    if (frustumCullingMultiThreadingStar != frustumCullingMultiThreading)
                    {
                        obj.enableFrustum = frustumCullingMultiThreading;
                        frustumCullingMultiThreadingStar = frustumCullingMultiThreading;

                        if(obj.enabled && obj.gameObject.activeInHierarchy)
                            obj.ReInit(true);
                        return;
                    }

                    serializedObject.ApplyModifiedProperties();

                    if (starDraw != draw)
                    {
                        if (draw)
                        {
                            obj.ReInit(true);
                        }
                        else
                            obj.altTreesManager.destroy(true);
                    }

                    #if (UNITY_5_2 || UNITY_5_3 || UNITY_5_4)

                        GUILayout.Space(10);
                        renderType = EditorGUILayout.IntPopup("Render Type:", renderType, new string[] { "Legacy", "PBR" }, new int[] { 0, 1 });

                        if (renderTypeStar != renderType)
                        {
                            if (obj.altTreesManagerData.shaders != null)
                            {
                                for (int i = 0; i < obj.altTreesManagerData.shaders.Length; i++)
                                {
                                    obj.altTreesManagerData.shaders[i].maximumLOD = 1000 + renderType;
                                }
                            }

                            renderTypeStar = renderType;
                        }
                    #endif


                    GUILayout.Space(10);


                    cameraModeFrustum = EditorGUILayout.IntPopup("Frustum Culling Camera:", cameraModeFrustum, new string[] { "Automatic (first active camera)", "Custom", "Via Script" }, new int[] { 0, 1, 2 });
                    if (cameraModeFrustum == 1)
                    {
                        SerializedProperty tps = serializedObject.FindProperty("activeCameraFrustum");
                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(tps, new GUIContent("Custom Frustum Culling Cameras:"), true);
                        if (EditorGUI.EndChangeCheck())
                            serializedObject.ApplyModifiedProperties();

                        //activeCameraFrustum = EditorGUILayout.ObjectField("Custom Frustum Culling Camera:", activeCameraFrustum, typeof(Camera), true) as Camera;
                    }
                    cameraModeDistance = EditorGUILayout.IntPopup("Distance Camera:", cameraModeDistance, new string[] { "Automatic (first active camera)", "Custom", "Via Script" }, new int[] { 0, 1, 2 });
                    if (cameraModeDistance == 1)
                    {
                        SerializedProperty tps2 = serializedObject.FindProperty("activeCameraDistance");
                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(tps2, new GUIContent("Custom Distance Cameras:"), true);
                        if (EditorGUI.EndChangeCheck())
                            serializedObject.ApplyModifiedProperties();

                        //activeCameraDistance = EditorGUILayout.ObjectField("Custom Distance Camera:", activeCameraDistance, typeof(Camera), true) as Camera;
                    }
                }
                GUILayout.EndVertical();

                GUILayout.Space(20);

                GUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    GUILayout.Label("Patch Settings", sty5);

                    if (!(obj.isActiveAndEnabled && obj.altTreesManagerData.draw))
                        GUI.enabled = false;

                    autoConfig = GUILayout.Toggle(autoConfig, "Auto Configuration");

                    if (autoConfigStar != autoConfig && obj.isActiveAndEnabled && obj.altTreesManagerData.draw && obj.isInitialized)
                    {
                        autoConfigStar = autoConfig;

                        obj.altTreesManagerData.autoConfig = autoConfig;
                        EditorUtility.SetDirty(obj.altTreesManagerData);

                        if (autoConfig)
                        {
                            sizePatchTemp = sizePatch;
                            maxLODTemp = maxLOD;

                            Terrain terr = (Terrain)Transform.FindObjectOfType(typeof(Terrain));
                            float size = 0f;
                            int degree = 0;

                            if (terr != null)
                                size = Mathf.Clamp(Mathf.Max(terr.terrainData.size.x, terr.terrainData.size.z), 100, 10000);
                            else
                                size = 1000f;
                            
                            if (size != sizePatch)
                            {
                                serializedObject.ApplyModifiedProperties();

                                EditorUtility.DisplayProgressBar("Working ... Please wait ... ", "Working ... Please wait ... ", 0.1f);

                                obj.altTreesManagerData.autoConfig = autoConfig;

                                resizePatches((int)size);

                                EditorUtility.ClearProgressBar();

                                sizePatch = (int)size;

                                Selection.activeGameObject = null;

                                return;
                            }


                            for (int d = 1; d < 16; d++)
                            {
                                if (size / Mathf.Pow(2f, (float)d) <= 160)
                                {
                                    degree = d + 1;
                                    break;
                                }
                            }

                            if (degree != obj.altTreesManagerData.maxLOD)
                            {
                                obj.altTreesManagerData.maxLOD = degree;
                                maxLOD = degree;
                                maxLODTemp = maxLOD;

                                EditorUtility.SetDirty(obj.altTreesManagerData);
                                AssetDatabase.SaveAssets();
                                AssetDatabase.Refresh();

                                serializedObject.ApplyModifiedProperties();


                                EditorUtility.DisplayProgressBar("Working ... Please wait ... ", "Working ... Please wait ... ", 0.1f);

                                resizeMaxLOD();

                                EditorUtility.ClearProgressBar();


                                obj.ReInit(true);

                                return;
                            }
                        }
                    }

                    if (autoConfig)
                        GUI.enabled = false;
                    EditorGUILayout.BeginHorizontal();
                    {
                        sizePatchTemp = EditorGUILayout.IntField("Size Patch:", sizePatchTemp);
                        if (GUILayout.Button("Set", GUILayout.Width(60f)))
                        {
                            if (sizePatch != sizePatchTemp)
                            {
                                serializedObject.ApplyModifiedProperties();

                                EditorUtility.DisplayProgressBar("Working ... Please wait ... ", "Working ... Please wait ... ", 0.1f);

                                resizePatches(sizePatchTemp);

                                EditorUtility.ClearProgressBar();

                                sizePatch = sizePatchTemp;

                                return;
                            }
                        }
                        GUILayout.Space(100);
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    {
                        maxLODTemp = EditorGUILayout.IntField("Max LOD Patch:", maxLODTemp);
                        if (GUILayout.Button("Set", GUILayout.Width(60f)))
                        {
                            if (maxLOD != maxLODTemp)
                            {
                                maxLOD = maxLODTemp;
                                obj.altTreesManagerData.maxLOD = maxLOD;
                                EditorUtility.SetDirty(obj.altTreesManagerData);
                                AssetDatabase.SaveAssets();
                                AssetDatabase.Refresh();

                                serializedObject.ApplyModifiedProperties();


                                EditorUtility.DisplayProgressBar("Working ... Please wait ... ", "Working ... Please wait ... ", 0.1f);

                                resizeMaxLOD();

                                EditorUtility.ClearProgressBar();

                                obj.ReInit(true);
                            }
                        }
                        GUILayout.Space(100);
                    }
                    EditorGUILayout.EndHorizontal();

                    GUI.enabled = true;

                    distancePatchFactor = Mathf.Clamp(EditorGUILayout.FloatField("Distance Patch Factor:", distancePatchFactor), 0.1f, 20f);
                }
                EditorGUILayout.EndVertical();

                GUILayout.Space(10);


                GUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    GUILayout.Label("LOD Settings", sty5);

                    distanceTreesLODFactor = EditorGUILayout.FloatField("Distance Trees LOD Factor:", distanceTreesLODFactor);
                    distanceObjectsLODFactor = EditorGUILayout.FloatField("Distance Objects LOD Factor:", distanceObjectsLODFactor);
                    checkTreesPercentPerFrame = Mathf.Floor(Mathf.Clamp(EditorGUILayout.FloatField("Check Trees Per Frame, Percent:", checkTreesPercentPerFrame), 0, 100));

                    crossFadeTimeBillboard = EditorGUILayout.FloatField("Cross-fade Billboard Time:", crossFadeTimeBillboard);
                    crossFadeTimeMesh = EditorGUILayout.FloatField("Cross-fade Mesh Time:", crossFadeTimeMesh);

                }
                GUILayout.EndVertical();

                GUILayout.Space(10);

                GUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    densityObjects = EditorGUILayout.Slider("Density Objects:", densityObjects, 0f, 100f);
                    GUILayout.Label("Note: in editor mode affects only the billboards", sty7);
                }
                GUILayout.EndVertical();

                GUILayout.Space(10);

                GUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    stableEditorMode = EditorGUILayout.Toggle("Stable Editor Mode:", stableEditorMode);
                    GUILayout.Label("Try to disable this option if you have lags in the editor.", sty7);
                }
                GUILayout.EndVertical();

                GUILayout.Space(10);

                GUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    GUILayout.Label("Dependencies on Camera Speed", sty5);

                    crossFadeDependenceOnSpeed = EditorGUILayout.Toggle("Dependence of Cross-fade Time:", crossFadeDependenceOnSpeed);
                    if (!crossFadeDependenceOnSpeed)
                        GUI.enabled = false;
                    crossFadeDependenceOnSpeedMaxSpeed = EditorGUILayout.FloatField("Maximum Camera Speed, m/s:", crossFadeDependenceOnSpeedMaxSpeed);
                    crossFadeDependenceOnSpeedMaxCoefficient = EditorGUILayout.FloatField("Maximum Coefficient:", crossFadeDependenceOnSpeedMaxCoefficient);
                    GUI.enabled = true;
                    GUILayout.Space(5);
                    checkTreesDependenceOnSpeed = EditorGUILayout.Toggle("Dependence of Check Trees:", checkTreesDependenceOnSpeed);
                    if (!checkTreesDependenceOnSpeed)
                        GUI.enabled = false;
                    checkTreesDependenceOnSpeedMaxSpeed = EditorGUILayout.FloatField("Maximum Camera Speed, m/s:", checkTreesDependenceOnSpeedMaxSpeed);
                    checkTreesDependenceOnSpeedMaxCoefficient = EditorGUILayout.FloatField("Maximum Coefficient:", checkTreesDependenceOnSpeedMaxCoefficient);
                    GUI.enabled = true;
                }
                GUILayout.EndVertical();

                GUILayout.Space(10);

                GUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    GUILayout.Label("Shadow Settings", sty5);

                    shadowsMeshes = EditorGUILayout.Toggle("Draw Mesh Shadows(Play Mode):", shadowsMeshes);
                    shadowsBillboards = EditorGUILayout.Toggle("Draw Billboard Shadows:", shadowsBillboards);
                    shadowsGroupBillboards = EditorGUILayout.Toggle("Draw GroupBillboards Shadows:", shadowsGroupBillboards);
                }
                GUILayout.EndVertical();

                GUILayout.Space(10);


                GUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    GUILayout.Label("Pool Settings", sty5);
                    
                    initCollidersCountPool = EditorGUILayout.IntField("Initial Collider Count:", initCollidersCountPool);
                    collidersPerOneMaxPool = EditorGUILayout.IntField("Max Colliders Count:", collidersPerOneMaxPool);
                    EditorGUILayout.Space();
                    initColliderBillboardsCountPool = EditorGUILayout.IntField("Initial Billboard Collider Count:", initColliderBillboardsCountPool);
                    colliderBillboardsPerOneMaxPool = EditorGUILayout.IntField("Max Billboard Colliders Count:", colliderBillboardsPerOneMaxPool);

                }
                GUILayout.EndVertical();

                EditorGUILayout.Space();


                if (!draw)
                    GUI.enabled = false;



                GUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    GUILayout.Label("Debug Settings", sty5);

                    drawDebugWindow = GUILayout.Toggle(drawDebugWindow, "Draw Debug Window");
                    drawDebugWindowInBuilds = GUILayout.Toggle(drawDebugWindowInBuilds, "Draw Debug Window in Builds");

                    drawDebugPatches = GUILayout.Toggle(drawDebugPatches, "Draw debug patches");
                    drawDebugBillboards = GUILayout.Toggle(drawDebugBillboards, "Debug billboards DrawCalls");


                    if (drawDebugBillboardsStar != drawDebugBillboards)
                    {
                        obj.altTreesManagerData.drawDebugBillboards = drawDebugBillboards;
                        EditorUtility.SetDirty(obj.altTreesManagerData);
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();

                        drawDebugBillboardsStar = drawDebugBillboards;
                        serializedObject.ApplyModifiedProperties();

                        obj.ReInit(true);
                    }

                    debugLog = GUILayout.Toggle(debugLog, "Debug Logs");
                    debugLogInBilds = GUILayout.Toggle(debugLogInBilds, "Debug Logs in Builds");
                    hideMeshes = GUILayout.Toggle(hideMeshes, "Hide Meshes(only Play Mode)");
                    hideBillboards = GUILayout.Toggle(hideBillboards, "Hide Billboards");
                    hideGroupBillboards = GUILayout.Toggle(hideGroupBillboards, "Hide GroupBillboards");

                }
                GUILayout.EndVertical();

                EditorGUILayout.Space();
                EditorGUILayout.Space();
                EditorGUILayout.Space();

                GUI.enabled = true;

                if (!(obj.isActiveAndEnabled && obj.altTreesManagerData.draw))
                    GUI.enabled = false;

                GUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    GUILayout.Label("Import/Export Trees", sty5);


                    isImport = EditorGUILayout.Foldout(isImport, "Import trees from Terrain");
                    if (isImport)
                    {
                        GUILayout.BeginVertical(EditorStyles.helpBox);
                        {
                            GUILayout.Label("Select the terrain for import:");
                            EditorGUILayout.Space();

                            terrainTempImport = (Terrain)EditorGUILayout.ObjectField(terrainTempImport, typeof(Terrain), true);


                            if (terrainTempImport != null)
                            {
                                EditorGUILayout.Space();
                                EditorGUILayout.Space();

                                GUILayout.BeginHorizontal();
                                {
                                    GUILayout.Label("Count of instance trees for import: ", sty, GUILayout.Width(190));

                                    sty.fontStyle = FontStyle.Bold;
                                    GUILayout.Label(terrainTempImport.terrainData.treeInstances.Length.ToString(), sty);
                                    sty.fontStyle = FontStyle.Normal;
                                }
                                GUILayout.EndHorizontal();

                                GUILayout.BeginHorizontal();
                                {
                                    GUILayout.Label("Count of prototype trees: ", sty, GUILayout.Width(150));

                                    sty.fontStyle = FontStyle.Bold;
                                    GUILayout.Label(terrainTempImport.terrainData.treePrototypes.Length.ToString(), sty);
                                    sty.fontStyle = FontStyle.Normal;
                                }
                                GUILayout.EndHorizontal();

                                EditorGUILayout.Space();

                                isDeleteTreesFromTerrain = GUILayout.Toggle(isDeleteTreesFromTerrain, "Delete Trees from Terrain");
                                if(!isDeleteTreesFromTerrain)
                                {
                                    isDeletePrototypesFromTerrain = false;
                                    GUI.enabled = false;
                                }
                                isDeletePrototypesFromTerrain = GUILayout.Toggle(isDeletePrototypesFromTerrain, "Delete Prototypes from Terrain");
                                GUI.enabled = true;

                                if (!(obj.isActiveAndEnabled && obj.altTreesManagerData.draw))
                                    GUI.enabled = false;
                                isDeleteTreesFromAltTrees = GUILayout.Toggle(isDeleteTreesFromAltTrees, "Delete Trees from AltTrees");

                                EditorGUILayout.Space();

                                if (terrainTempImport.terrainData.treeInstances.Length == 0)
                                    GUI.enabled = false;

                                if (GUILayout.Button("Import"))
                                {
                                    serializedObject.ApplyModifiedProperties();
                                    
                                    useDefaultSettingsConvertTrees = false;

                                    Import();

                                    obj.altTreesManagerData.draw = true;
                                    EditorUtility.SetDirty(obj.altTreesManagerData);
                                    obj.ReInit(true);

                                    terrainTempImport = null;
                                    isImport = false;
                                }

                                GUI.enabled = true;

                                if (!(obj.isActiveAndEnabled && obj.altTreesManagerData.draw))
                                    GUI.enabled = false;
                                EditorGUILayout.Space();
                            }
                        }
                        GUILayout.EndVertical();
                    }

                    isExport = EditorGUILayout.Foldout(isExport, "Export trees to Terrain");
                    if (isExport)
                    {
                        GUILayout.BeginVertical(EditorStyles.helpBox);
                        {
                            GUILayout.Label("Select the terrain for export:");
                            EditorGUILayout.Space();

                            terrainTempExport = (Terrain)EditorGUILayout.ObjectField(terrainTempExport, typeof(Terrain), true);


                            if (terrainTempExport != null)
                            {
                                TerrainData terrainData = terrainTempExport.terrainData;


                                if (terrainTempExportStar == null || !terrainTempExportStar.Equals(terrainTempExport))
                                {
                                    attTemp = new List<AltTreesTrees>();
                                    prototypesListTemp = new List<int>();

                                    listPatchesExport = obj.getPatches(terrainTempExport.transform.position + obj.altTreesManager.jump * obj.altTreesManagerData.sizePatch, terrainData.size.x, terrainData.size.z);
                                    for (int i = 0; i < listPatchesExport.Length; i++)
                                    {
                                        AltTreesTrees[] attArrayTemp = listPatchesExport[i].getTreesForExport(new Vector2((terrainTempExport.transform.position + obj.altTreesManager.jump * obj.altTreesManagerData.sizePatch).x, (terrainTempExport.transform.position + obj.altTreesManager.jump * obj.altTreesManagerData.sizePatch).z), terrainData.size.x, terrainData.size.z);

                                        if (attArrayTemp != null && attArrayTemp.Length != 0)
                                        {
                                            for (int k = 0; k < attArrayTemp.Length; k++)
                                            {
                                                attTemp.Add(attArrayTemp[k]);

                                                if (!prototypesListTemp.Contains(attArrayTemp[k].idPrototype))
                                                {
                                                    prototypesListTemp.Add(attArrayTemp[k].idPrototype);
                                                }
                                            }
                                        }
                                    }

                                    

                                    altTreesArrayExport = new AltTree[prototypesListTemp.Count];
                                    terrainTreesArrayExport = new GameObject[prototypesListTemp.Count];

                                    for (int i = 0; i < prototypesListTemp.Count; i++)
                                    {
                                        altTreesArrayExport[i] = dataLinks.getAltTree(prototypesListTemp[i]);
                                        terrainTreesArrayExport[i] = dataLinks.getTree(prototypesListTemp[i]);
                                    }

                                    terrainTempExportStar = terrainTempExport;
                                }


                                EditorGUILayout.Space();
                                EditorGUILayout.Space();

                                GUILayout.BeginHorizontal();
                                {
                                    GUILayout.Label("Count of instance trees for export: ", sty, GUILayout.Width(190));

                                    sty.fontStyle = FontStyle.Bold;
                                    if (attTemp != null && terrainData != null && terrainData.treeInstances != null)
                                        GUILayout.Label(attTemp.Count.ToString() + " - " + terrainData.treeInstances.Length, sty);
                                    else
                                    {
                                        if (attTemp != null)
                                            GUILayout.Label(attTemp.Count.ToString() + " - 0", sty);
                                        else if (terrainData != null && terrainData.treeInstances != null)
                                            GUILayout.Label("0 - " + terrainData.treeInstances.Length, sty);
                                        else
                                            GUILayout.Label("0 - 0", sty);
                                    }
                                    sty.fontStyle = FontStyle.Normal;
                                }
                                GUILayout.EndHorizontal();

                                GUILayout.BeginHorizontal();
                                {
                                    GUILayout.Label("Count of prototype trees: ", sty, GUILayout.Width(150));

                                    sty.fontStyle = FontStyle.Bold;
                                    GUILayout.Label(prototypesListTemp.Count.ToString() + " - " + terrainData.treePrototypes.Length, sty);
                                    sty.fontStyle = FontStyle.Normal;
                                }
                                GUILayout.EndHorizontal();

                                EditorGUILayout.Space();

                                isDeleteTreesFromTerrain = GUILayout.Toggle(isDeleteTreesFromTerrain, "Delete Trees from Terrain");
                                if (!isDeleteTreesFromTerrain)
                                {
                                    isDeletePrototypesFromTerrain = false;
                                    GUI.enabled = false;
                                }
                                isDeletePrototypesFromTerrain = GUILayout.Toggle(isDeletePrototypesFromTerrain, "Delete Prototypes from Terrain");
                                GUI.enabled = true;

                                if (!(obj.isActiveAndEnabled && obj.altTreesManagerData.draw))
                                    GUI.enabled = false;
                                isDeleteTreesFromAltTrees = GUILayout.Toggle(isDeleteTreesFromAltTrees, "Delete Trees from AltTrees");

                                EditorGUILayout.Space();



                                if (attTemp != null && prototypesListTemp != null && !(attTemp.Count == 0 || prototypesListTemp.Count == 0))
                                {
                                    sty.fontStyle = FontStyle.Bold;
                                    GUILayout.Label("Prototypes Dependency:", sty);


                                    GUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.MaxWidth(430));
                                    {
                                        GUILayout.BeginHorizontal();
                                        {
                                            GUILayout.BeginVertical(GUILayout.Width(20));
                                            {
                                                GUILayout.Label("", sty);
                                            }
                                            GUILayout.EndVertical();
                                            GUILayout.BeginVertical(sty3, GUILayout.MaxWidth(200));
                                            {
                                                GUILayout.Label("AltTrees:", sty);
                                            }
                                            GUILayout.EndVertical();
                                            GUILayout.BeginVertical(sty3, GUILayout.MaxWidth(200));
                                            {
                                                GUILayout.Label("Terrain Trees:", sty);
                                            }
                                            GUILayout.EndVertical();
                                        }
                                        GUILayout.EndHorizontal();


                                        for (int i = 0; i < prototypesListTemp.Count; i++)
                                        {
                                            GUILayout.BeginHorizontal();
                                            {
                                                GUILayout.BeginVertical(GUILayout.Width(20));
                                                {
                                                    GUILayout.Label((i + 1) + ".", sty);
                                                }
                                                GUILayout.EndVertical();
                                                GUILayout.BeginVertical();
                                                {
                                                    if (altTreesArrayExport[i] != null)
                                                    {
                                                        Texture tex = null;

                                                        if (icons.ContainsKey(altTreesArrayExport[i].id))
                                                        {
                                                            tex = icons[altTreesArrayExport[i].id];
                                                        }
                                                        else
                                                        {
                                                            tex = AssetPreview.GetAssetPreview(altTreesArrayExport[i].gameObject);
                                                            if (tex != null)
                                                            {
                                                                tex = Instantiate(tex) as Texture;
                                                                tex.hideFlags = HideFlags.HideAndDontSave;
                                                                icons.Add(altTreesArrayExport[i].id, tex as Texture2D);
                                                            }
                                                        }

                                                        if (tex != null)
                                                            GUILayout.Label(tex, GUILayout.Width(90), GUILayout.Height(90));
                                                        EditorGUILayout.ObjectField(altTreesArrayExport[i], typeof(AltTree), false, GUILayout.Width(150));
                                                    }

                                                }
                                                GUILayout.EndVertical();
                                                GUILayout.BeginVertical();
                                                {
                                                    if (altTreesArrayExport[i] != null)
                                                    {
                                                        Texture2D tex = null;
                                                        tex = AssetPreview.GetAssetPreview(terrainTreesArrayExport[i]);
                                                        if (tex != null)
                                                            GUILayout.Label(tex, GUILayout.Width(90), GUILayout.Height(90));
                                                        else
                                                            GUILayout.Label("", GUILayout.Width(90), GUILayout.Height(90));
                                                        terrainTreesArrayExport[i] = EditorGUILayout.ObjectField(terrainTreesArrayExport[i], typeof(GameObject), false, GUILayout.Width(150)) as GameObject;
                                                    }

                                                }
                                                GUILayout.EndVertical();

                                                this.Repaint();
                                            }
                                            GUILayout.EndHorizontal();
                                        }
                                    }
                                    GUILayout.EndVertical();






                                    sty.fontStyle = FontStyle.Normal;

                                    EditorGUILayout.Space();


                                    bool isStop = false;
                                    for (int i = 0; i < prototypesListTemp.Count; i++)
                                    {
                                        if (terrainTreesArrayExport[i] != null)
                                        {
                                            LODGroup lodGroup = terrainTreesArrayExport[i].GetComponent<LODGroup>();
                                            SerializedObject objj;
                                            SerializedProperty prop = null;
                                            GameObject goo = null;
                                            if (lodGroup != null)
                                            {
                                                objj = new SerializedObject(lodGroup);
                                                if (lodGroup.lodCount > 0)
                                                {
                                                    prop = objj.FindProperty("m_LODs.Array.data[0].renderers");
                                                    if (prop.arraySize > 0)
                                                    {
                                                        goo = (prop.GetArrayElementAtIndex(0).FindPropertyRelative("renderer").objectReferenceValue as Renderer).gameObject;
                                                    }
                                                }
                                            }

                                            if (terrainTreesArrayExport[i] == null || !(terrainTreesArrayExport[i].GetComponent<Tree>() != null || (goo != null && goo.GetComponent<Tree>() != null)))
                                                isStop = true;
                                        }
                                        else
                                            isStop = true;
                                    }


                                    if (isStop)
                                        GUI.enabled = false;

                                }
                                else
                                    GUI.enabled = false;

                                if (GUILayout.Button("Export"))
                                {
                                    Export();

                                    serializedObject.ApplyModifiedProperties();
                                    obj.ReInit(true);

                                    terrainTempExport = null;
                                    isExport = false;
                                    return;
                                }

                                GUI.enabled = true;

                                if (!(obj.isActiveAndEnabled && obj.altTreesManagerData.draw))
                                    GUI.enabled = false;
                                EditorGUILayout.Space();
                            }
                        }
                        GUILayout.EndVertical();
                    }
                    /*
                    isExportAll = EditorGUILayout.Foldout(isExportAll, "Export trees to All Terrains");
                    if (isExportAll)
                    {
                        if(isExportAllStar != isExportAll)
                        {
                            terrainsTempExportAll = FindObjectsOfType<Terrain>();

                            atpExportList.Clear();
                            for (int i = 0; i < obj.altTreesManagerData.patches.Length; i++)
                            {
                                for (int j = 0; j < obj.altTreesManagerData.patches[i].prototypes.Length; j++)
                                {
                                    if (!atpExportList.Contains(obj.altTreesManagerData.patches[i].prototypes[j].tree))
                                        atpExportList.Add(obj.altTreesManagerData.patches[i].prototypes[j].tree);
                                }
                            }
                            



                            isExportAllStar = isExportAll;
                        }

                        GUILayout.BeginVertical(EditorStyles.helpBox);
                        {
                            if (terrainsTempExportAll != null && terrainsTempExportAll.Length > 0)
                            {
                                GUILayout.Label("Terrains for Export: " + terrainsTempExportAll.Length);
                                EditorGUILayout.Space();
                                
                                TerrainData terrainData = terrainTempExport.terrainData;


                                if (terrainTempExportStar == null || !terrainTempExportStar.Equals(terrainTempExport))
                                {
                                    attTemp = new List<AltTreesTrees>();
                                    prototypesListTemp = new List<int>();

                                    listPatchesExport = obj.getPatches(terrainTempExport.transform.position + obj.altTreesManager.jump * obj.altTreesManagerData.sizePatch, terrainData.size.x, terrainData.size.z);
                                    for (int i = 0; i < listPatchesExport.Length; i++)
                                    {
                                        AltTreesTrees[] attArrayTemp = listPatchesExport[i].getTreesForExport(new Vector2((terrainTempExport.transform.position + obj.altTreesManager.jump * obj.altTreesManagerData.sizePatch).x, (terrainTempExport.transform.position + obj.altTreesManager.jump * obj.altTreesManagerData.sizePatch).z), terrainData.size.x, terrainData.size.z);

                                        if (attArrayTemp != null && attArrayTemp.Length != 0)
                                        {
                                            for (int k = 0; k < attArrayTemp.Length; k++)
                                            {
                                                attTemp.Add(attArrayTemp[k]);
                                            }
                                        }
                                    }


                                    for (int i = 0; i < attTemp.Count; i++)
                                    {
                                        if (!prototypesListTemp.Contains(attTemp[i].idPrototype))
                                        {
                                            prototypesListTemp.Add(attTemp[i].idPrototype);
                                        }
                                    }



                                    altTreesArrayExport = new AltTree[prototypesListTemp.Count];
                                    terrainTreesArrayExport = new GameObject[prototypesListTemp.Count];

                                    for (int i = 0; i < prototypesListTemp.Count; i++)
                                    {
                                        altTreesArrayExport[i] = dataLinks.getAltTree(prototypesListTemp[i]);
                                        terrainTreesArrayExport[i] = dataLinks.getTree(prototypesListTemp[i]);
                                    }

                                    terrainTempExportStar = terrainTempExport;
                                }


                                EditorGUILayout.Space();
                                EditorGUILayout.Space();

                                GUILayout.BeginHorizontal();
                                {
                                    GUILayout.Label("Count of instance trees for export: ", sty, GUILayout.Width(190));

                                    sty.fontStyle = FontStyle.Bold;
                                    GUILayout.Label(attTemp.Count.ToString() + " - " + terrainData.treeInstances.Length, sty);
                                    sty.fontStyle = FontStyle.Normal;
                                }
                                GUILayout.EndHorizontal();

                                GUILayout.BeginHorizontal();
                                {
                                    GUILayout.Label("Count of prototype trees: ", sty, GUILayout.Width(150));

                                    sty.fontStyle = FontStyle.Bold;
                                    GUILayout.Label(prototypesListTemp.Count.ToString() + " - " + terrainData.treePrototypes.Length, sty);
                                    sty.fontStyle = FontStyle.Normal;
                                }
                                GUILayout.EndHorizontal();

                                EditorGUILayout.Space();

                                isDeleteTreesFromTerrain = GUILayout.Toggle(isDeleteTreesFromTerrain, "Delete Trees from Terrain");
                                if (!isDeleteTreesFromTerrain)
                                {
                                    isDeletePrototypesFromTerrain = false;
                                    GUI.enabled = false;
                                }
                                isDeletePrototypesFromTerrain = GUILayout.Toggle(isDeletePrototypesFromTerrain, "Delete Prototypes from Terrain");
                                GUI.enabled = true;

                                if (!(obj.isActiveAndEnabled && obj.altTreesManagerData.draw))
                                    GUI.enabled = false;
                                isDeleteTreesFromAltTrees = GUILayout.Toggle(isDeleteTreesFromAltTrees, "Delete Trees from AltTrees");

                                EditorGUILayout.Space();



                                if (!(attTemp.Count == 0 || prototypesListTemp.Count == 0))
                                {
                                    sty.fontStyle = FontStyle.Bold;
                                    GUILayout.Label("Prototypes Dependency:", sty);


                                    GUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.MaxWidth(430));
                                    {
                                        GUILayout.BeginHorizontal();
                                        {
                                            GUILayout.BeginVertical(GUILayout.Width(20));
                                            {
                                                GUILayout.Label("", sty);
                                            }
                                            GUILayout.EndVertical();
                                            GUILayout.BeginVertical(sty3, GUILayout.MaxWidth(200));
                                            {
                                                GUILayout.Label("AltTrees:", sty);
                                            }
                                            GUILayout.EndVertical();
                                            GUILayout.BeginVertical(sty3, GUILayout.MaxWidth(200));
                                            {
                                                GUILayout.Label("Terrain Trees:", sty);
                                            }
                                            GUILayout.EndVertical();
                                        }
                                        GUILayout.EndHorizontal();


                                        for (int i = 0; i < prototypesListTemp.Count; i++)
                                        {
                                            GUILayout.BeginHorizontal();
                                            {
                                                GUILayout.BeginVertical(GUILayout.Width(20));
                                                {
                                                    GUILayout.Label((i + 1) + ".", sty);
                                                }
                                                GUILayout.EndVertical();
                                                GUILayout.BeginVertical();
                                                {
                                                    if (altTreesArrayExport[i] != null)
                                                    {
                                                        Texture2D tex = null;
                                                        tex = AssetPreview.GetAssetPreview(altTreesArrayExport[i].lods[0]);
                                                        if (tex != null)
                                                            GUILayout.Label(tex, GUILayout.Width(90), GUILayout.Height(90));
                                                        EditorGUILayout.ObjectField(altTreesArrayExport[i], typeof(AltTree), false, GUILayout.Width(150));
                                                    }

                                                }
                                                GUILayout.EndVertical();
                                                GUILayout.BeginVertical();
                                                {
                                                    if (altTreesArrayExport[i] != null)
                                                    {
                                                        Texture2D tex = null;
                                                        tex = AssetPreview.GetAssetPreview(terrainTreesArrayExport[i]);
                                                        if (tex != null)
                                                            GUILayout.Label(tex, GUILayout.Width(90), GUILayout.Height(90));
                                                        else
                                                            GUILayout.Label("", GUILayout.Width(90), GUILayout.Height(90));
                                                        terrainTreesArrayExport[i] = EditorGUILayout.ObjectField(terrainTreesArrayExport[i], typeof(GameObject), false, GUILayout.Width(150)) as GameObject;
                                                    }

                                                }
                                                GUILayout.EndVertical();

                                                this.Repaint();
                                            }
                                            GUILayout.EndHorizontal();
                                        }
                                    }
                                    GUILayout.EndVertical();






                                    sty.fontStyle = FontStyle.Normal;

                                    EditorGUILayout.Space();


                                    bool isStop = false;
                                    for (int i = 0; i < prototypesListTemp.Count; i++)
                                    {
                                        if (terrainTreesArrayExport[i] != null)
                                        {
                                            LODGroup lodGroup = terrainTreesArrayExport[i].GetComponent<LODGroup>();
                                            SerializedObject objj;
                                            SerializedProperty prop = null;
                                            GameObject goo = null;
                                            if (lodGroup != null)
                                            {
                                                objj = new SerializedObject(lodGroup);
                                                if (lodGroup.lodCount > 0)
                                                {
                                                    prop = objj.FindProperty("m_LODs.Array.data[0].renderers");
                                                    if (prop.arraySize > 0)
                                                    {
                                                        goo = (prop.GetArrayElementAtIndex(0).FindPropertyRelative("renderer").objectReferenceValue as Renderer).gameObject;
                                                    }
                                                }
                                            }

                                            if (terrainTreesArrayExport[i] == null || !(terrainTreesArrayExport[i].GetComponent<Tree>() != null || (goo != null && goo.GetComponent<Tree>() != null)))
                                                isStop = true;
                                        }
                                        else
                                            isStop = true;
                                    }


                                    if (isStop)
                                        GUI.enabled = false;

                                }
                                else
                                    GUI.enabled = false;

                                if (GUILayout.Button("Export"))
                                {
                                    Export();

                                    serializedObject.ApplyModifiedProperties();
                                    obj.ReInit(true);

                                    terrainTempExport = null;
                                    isExportAll = false;
                                    return;
                                }

                                GUI.enabled = true;

                                if (!(obj.isActiveAndEnabled && obj.altTreesManagerData.draw))
                                    GUI.enabled = false;
                                EditorGUILayout.Space();
                            }
                            else
                            {
                                GUILayout.Label("No terrains found on scene");
                                EditorGUILayout.Space();
                            }
                        }
                        GUILayout.EndVertical();
                    }
                    else
                    {
                        if (isExportAllStar != isExportAll)
                        {
                            terrainsTempExportAll = null;
                            isExportAllStar = isExportAll;
                        }
                    }*/

                    if (GUILayout.Button("Import trees from scene", GUILayout.Width(160), GUILayout.Height(18)))
                    {
                        ImportTreesFromSceneWindow.CreateWindow(obj);
                        Selection.activeGameObject = null;
                    }
                    GUI.enabled = true;
                }
                GUILayout.EndVertical();

                EditorGUILayout.Space();


                bool save = false;
                if (hideMeshes != hideMeshesStar)
                {
                    hideMeshesStar = hideMeshes;
                    save = true;
                }
                if (hideBillboards != hideBillboardsStar)
                {
                    hideBillboardsStar = hideBillboards;
                    save = true;
                }
                if (hideGroupBillboards != hideGroupBillboardsStar)
                {
                    hideGroupBillboardsStar = hideGroupBillboards;
                    save = true;
                }

                if ((GUI.changed && menuIdStar == menuId.intValue && menuId.intValue == 3) || save)
                {
                    //obj.altTreesManagerData.sizePatch = sizePatch;
                    //obj.altTreesManagerData.maxLOD = maxLOD;
                    obj.altTreesManagerData.distancePatchFactor = distancePatchFactor;
                    obj.altTreesManagerData.distanceTreesLODFactor = distanceTreesLODFactor;
                    obj.altTreesManagerData.distanceObjectsLODFactor = distanceObjectsLODFactor;
                    obj.altTreesManagerData.checkTreesPerFramePercent = checkTreesPercentPerFrame;
                    obj.altTreesManagerData.crossFadeTimeBillboard = crossFadeTimeBillboard;
                    obj.altTreesManagerData.crossFadeTimeMesh = crossFadeTimeMesh;

                    obj.altTreesManagerData.initCollidersCountPool = initCollidersCountPool;
                    obj.altTreesManagerData.collidersPerOneMaxPool = collidersPerOneMaxPool;
                    obj.altTreesManagerData.initColliderBillboardsCountPool = initColliderBillboardsCountPool;
                    obj.altTreesManagerData.colliderBillboardsPerOneMaxPool = colliderBillboardsPerOneMaxPool;

                    #if (UNITY_5_2 || UNITY_5_3 || UNITY_5_4)
                        obj.altTreesManagerData.renderType = renderType;
                    #endif

                    obj.altTreesManagerData.densityObjects = densityObjects;

                    obj.cameraModeFrustum = cameraModeFrustum;
                    //obj.activeCameraFrustum = activeCameraFrustum;
                    obj.cameraModeDistance = cameraModeDistance;
                    //obj.activeCameraDistance = activeCameraDistance;

                    obj.altTreesManagerData.shadowsBillboards = shadowsBillboards;
                    obj.altTreesManagerData.shadowsGroupBillboards = shadowsGroupBillboards;
                    obj.altTreesManagerData.shadowsMeshes = shadowsMeshes;

                    obj.altTreesManagerData.crossFadeDependenceOnSpeed = crossFadeDependenceOnSpeed;
                    obj.altTreesManagerData.crossFadeDependenceOnSpeedMaxSpeed = crossFadeDependenceOnSpeedMaxSpeed;
                    obj.altTreesManagerData.crossFadeDependenceOnSpeedMaxCoefficient = crossFadeDependenceOnSpeedMaxCoefficient;
                    obj.altTreesManagerData.checkTreesDependenceOnSpeed = checkTreesDependenceOnSpeed;
                    obj.altTreesManagerData.checkTreesDependenceOnSpeedMaxSpeed = checkTreesDependenceOnSpeedMaxSpeed;
                    obj.altTreesManagerData.checkTreesDependenceOnSpeedMaxCoefficient = checkTreesDependenceOnSpeedMaxCoefficient;

                    obj.altTreesManagerData.draw = draw;
                    obj.altTreesManagerData.autoConfig = autoConfig;
                    //obj.altTreesManagerData.generateAllBillboardsOnStart = generateAllBillboardsOnStart;
                    obj.altTreesManagerData.enableColliders = enableColliders;
                    obj.altTreesManagerData.colliderEvents = colliderEvents;
                    obj.altTreesManagerData.drawDebugPatches = drawDebugPatches;
                    obj.altTreesManagerData.drawDebugBillboards = drawDebugBillboards;
                    obj.altTreesManagerData.drawDebugBillboardsStar = drawDebugBillboardsStar;
                    obj.altTreesManagerData.debugLog = debugLog;
                    obj.altTreesManagerData.debugLogInBilds = debugLogInBilds;
                    obj.altTreesManagerData.drawDebugWindow = drawDebugWindow;
                    obj.altTreesManagerData.drawDebugWindowInBuilds = drawDebugWindowInBuilds;

                    obj.altTreesManagerData.hideMeshes = hideMeshes;
                    obj.altTreesManagerData.hideBillboards = hideBillboards;
                    obj.altTreesManagerData.hideGroupBillboards = hideGroupBillboards;

                    obj.altTreesManagerData.stableEditorMode = stableEditorMode;
                    

                    EditorUtility.SetDirty(obj.altTreesManagerData);

                    isRefresh = true;
                }
                menuIdStar = menuId.intValue;
            }

            serializedObject.ApplyModifiedProperties();

            if (treeTemp != null)
            {
                if (!hueColorLeaves.Equals(treeTemp.hueVariationLeaves))
                {
                    treeTemp.hueVariationLeaves = hueColorLeaves;
                    EditorUtility.SetDirty(treeTemp);
                }
                if (!hueColorBark.Equals(treeTemp.hueVariationBark))
                {
                    treeTemp.hueVariationBark = hueColorBark;
                    EditorUtility.SetDirty(treeTemp);
                }
            }
        }

        public void OnSceneGUI()
        {
            if (obj == null)
                return;

            Event current = Event.current;

            getDataLinks();

            if (obj.dataLinksCorrupted)
                return;
            
            if(checkTreeVersionsStatus)
                return;

            if (current.shift)
                isPlacingShift = true;
            else
                isPlacingShift = false;
            if (current.control)
                isPlacingCtrl = true;
            else
                isPlacingCtrl = false;
            if (current.alt)
                isPlacingAlt = true;
            else
                isPlacingAlt = false;
            


            if ((menuId.intValue == 1 && idTreeSelected.intValue != -1) && (current.type == EventType.MouseMove || current.type == EventType.MouseDown || current.type == EventType.MouseDrag))
            {
                projector.orthographicSize = brushSize.intValue;

                Ray worldRay = HandleUtility.GUIPointToWorldRay(current.mousePosition);
                RaycastHit hitInfo;


                if (Physics.Raycast(worldRay, out hitInfo))
                {
                    projectorTransform.position = hitInfo.point + Vector3.up * 100;

                    if ((current.type == EventType.MouseDown || current.type == EventType.MouseDrag) && current.button == 0 && !isPlacingAlt)
                    {
                        Dictionary<AltTreesPatch, List<AddTreesPositionsStruct>> tempListPatches = new Dictionary<AltTreesPatch, List<AddTreesPositionsStruct>>();

                        if (isPlacingShift)
                        {
                            obj.removeTrees(hitInfo.point, 0.9f * brushSize.intValue, true, -1);
                        }
                        else if (isPlacingCtrl)
                        {
                            obj.removeTrees(hitInfo.point, 0.9f * brushSize.intValue, true, dataLinks.altTrees[treeIdsTemp[idTreeSelected.intValue]].id);
                        }
                        else
                        {
                            Vector2 randVector = new Vector2();
                            RaycastHit hitInfo2;
                            int countPlace = 0;

                            if (current.type == EventType.MouseDown)
                                countPlace = treeCount.intValue;
                            else
                                countPlace = Mathf.CeilToInt((((float)treeCount.intValue) / 10f) * ((float)speedPlace.intValue));

                            int sch = 0;
                            for (int i = 0; i < countPlace; i++)
                            {
                                randVector = UnityEngine.Random.insideUnitCircle * 0.9f * brushSize.intValue;

                                if (Physics.Raycast(hitInfo.point + new Vector3(randVector.x, 100f, randVector.y), Vector3.up * -1, out hitInfo2, 200f) && Vector3.Angle(hitInfo2.normal.normalized, Vector3.up) <= angleLimit.floatValue)
                                {
                                    sch = 0;
                                    AltTreesPatch tempPatch = getPatch(hitInfo2.point);

                                    if (tempListPatches.ContainsKey(tempPatch))
                                    {
                                        tempListPatches[tempPatch].Add(new AddTreesPositionsStruct(hitInfo2.point, dataLinks.altTrees[treeIdsTemp[idTreeSelected.intValue]]));
                                    }
                                    else
                                    {
                                        tempPatch.checkTreePrototype(dataLinks.altTrees[treeIdsTemp[idTreeSelected.intValue]].id, dataLinks.altTrees[treeIdsTemp[idTreeSelected.intValue]], true, true);
                                        tempListPatches.Add(tempPatch, new List<AddTreesPositionsStruct>());
                                        tempListPatches[tempPatch].Add(new AddTreesPositionsStruct(hitInfo2.point, dataLinks.altTrees[treeIdsTemp[idTreeSelected.intValue]]));
                                    }
                                }
                                else
                                {
                                    sch++;

                                    if (sch < 100)
                                        i--;
                                    else
                                        sch = 0;
                                }
                            }

                            foreach (AltTreesPatch key in tempListPatches.Keys)
                            {
                                key.addTrees(tempListPatches[key].ToArray(), randomRotation.boolValue, isRandomHeight.boolValue, height.floatValue, heightRandom.floatValue,
                                             lockWidthToHeight.boolValue, isRandomWidth.boolValue, width.floatValue, widthRandom.floatValue, hueColorLeaves, hueColorBark, isRandomHueLeaves.boolValue, isRandomHueBark.boolValue);
                            }
                        }
                    }
                }
                else
                    projectorTransform.position = Vector3.up * -1000000;
            }
        }

        AltTreesPatch getPatch(Vector3 pos, int sizePatch = 0, bool reInit = true)
        {
            if (sizePatch == 0)
                sizePatch = obj.altTreesManagerData.sizePatch;
            AltTreesPatch altTreesPatchTemp = obj.getPatch(pos + obj.altTreesManager.jump * sizePatch, sizePatch);
            if (altTreesPatchTemp != null)
                return altTreesPatchTemp;
            else
                return addPatch(Mathf.FloorToInt((pos.x - obj.altTreesManager.jumpPos.x) / ((float)sizePatch)) + (int)obj.altTreesManager.jump.x, Mathf.FloorToInt((pos.z - obj.altTreesManager.jumpPos.z) / ((float)sizePatch)) + (int)obj.altTreesManager.jump.z);
        }
        
        AltTreesPatch addPatch(int _stepX, int _stepY)
        {
            //Log("addPatch");
            #if UNITY_EDITOR
                AltTreesPatch atpTemp = new AltTreesPatch(_stepX, _stepY);

                atpTemp.prototypes = new AltTreePrototypes[0];
                atpTemp.trees = new AltTreesTrees[0];

                AltTreesPatch[] patchesTemp = obj.altTreesManagerData.patches;
                obj.altTreesManagerData.patches = new AltTreesPatch[patchesTemp.Length + 1];
                for (int i = 0; i < patchesTemp.Length; i++)
                {
                    obj.altTreesManagerData.patches[i] = patchesTemp[i];
                }
                obj.altTreesManagerData.patches[patchesTemp.Length] = atpTemp;

                EditorUtility.SetDirty(obj.altTreesManagerData);
                atpTemp.Init(obj.altTreesManager, obj, obj.altTreesManagerData, true);
                obj.altTreesManager.addPatch(atpTemp);

                return atpTemp;
            #else
                    return null;
            #endif
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

        public void Import()
        {
            getDataLinks();

            if (obj.dataLinksCorrupted)
                return;
            
            TerrainData terrainData = terrainTempImport.terrainData;

            int countTemp = 0;
            int treePrototypesLength = terrainData.treePrototypes.Length;

            AltTree[] prototypesTemp = new AltTree[treePrototypesLength];

            for (int i = 0; i < treePrototypesLength; i++)
            {
                AltTree treeTemp2 = null;
                treeTemp2 = dataLinks.getAltTree(terrainData.treePrototypes[i].prefab);

                if (treeTemp2 == null)
                {
                    ImportTreeWindow.ConvertTree(terrainData.treePrototypes[i].prefab, this, terrainTempImport, isDeleteTreesFromTerrain, isDeleteTreesFromAltTrees, isDeletePrototypesFromTerrain, useDefaultSettingsConvertTrees);
                    return;
                }

                prototypesTemp[i] = dataLinks.getAltTree(terrainData.treePrototypes[i].prefab);
            }

            EditorUtility.DisplayProgressBar("Import trees from Terrain... ", "Please wait... ", 0.0f);

            for (int i = 0; i < prototypesTemp.Length; i++)
            {
                if (prototypesTemp[i] == null)
                    Debug.Log("null pr");
            }


            if(isDeleteTreesFromAltTrees)
            {
                List<AltTreesPatch> patchesTemp = new List<AltTreesPatch>(obj.altTreesManagerData.patches);
                foreach (AltTreesPatch _patch in patchesTemp)
                {
                    //obj.removePatch(_patch);


                    if (_patch.treesNoGroupCount == _patch.treesNoGroupEmptyCount)
                        obj.removePatch(_patch);
                    else
                    {
                            _patch.trees = new AltTreesTrees[0];
                            _patch.treesCount = 0;
                            _patch.treesEmptyCount = 0;
                            _patch.treesEmpty = new int[0];
                            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(_patch.treesData));

                            EditorUtility.SetDirty(_patch.altTreesManagerData);
                            AssetDatabase.SaveAssets();
                            AssetDatabase.Refresh();
                    }
                }
            }
            



            Dictionary<AltTreesPatch, List<AddTreesStruct>> tempListPatches = new Dictionary<AltTreesPatch, List<AddTreesStruct>>();

            Color colorLeavesTemp = Color.black;
            Color colorBarkTemp = Color.black;

            TreeInstance[] treeInsts = terrainData.treeInstances;
            TreeInstance treeInst;
            AltTree aTree;
            int treeCounts = treeInsts.Length;

            Vector3 terrainTempImportPos = terrainTempImport.GetPosition();
            Vector3 terrainTempImportSize = terrainData.size;
            
            for (int j = 0; j < treeCounts; j++)
            {
                treeInst = treeInsts[j];
                aTree = prototypesTemp[treeInst.prototypeIndex];
                
                Vector3 posTemp = Vector3.Scale(treeInst.position, terrainTempImportSize) + terrainTempImportPos;

                AltTreesPatch tempPatch = getPatch(posTemp);
                /*colorLeavesTemp = aTree.hueVariationLeaves;
                colorLeavesTemp.a = UnityEngine.Random.value * colorLeavesTemp.a;
                colorBarkTemp = aTree.hueVariationBark;
                colorBarkTemp.a = UnityEngine.Random.value * colorBarkTemp.a;*/

                colorLeavesTemp.a = UnityEngine.Random.value * 0.15f;
                colorBarkTemp.a = UnityEngine.Random.value * 0.24f;
                //colorLeavesTemp.a = 0;
                //colorBarkTemp.a = 0;

                if (tempListPatches.ContainsKey(tempPatch))
                {
                    tempPatch.checkTreePrototype(aTree.id, aTree, false, true);
                    tempListPatches[tempPatch].Add(new AddTreesStruct(posTemp, aTree.id, aTree.isObject, treeInst.rotation * 57.2958f, treeInst.heightScale, treeInst.widthScale, colorLeavesTemp, colorBarkTemp));
                }
                else
                {
                    tempPatch.checkTreePrototype(aTree.id, aTree, false, true);
                    tempListPatches.Add(tempPatch, new List<AddTreesStruct>());
                    tempListPatches[tempPatch].Add(new AddTreesStruct(posTemp, aTree.id, aTree.isObject, treeInst.rotation * 57.2958f, treeInst.heightScale, treeInst.widthScale, colorLeavesTemp, colorBarkTemp));
                }


                countTemp++;
                if (countTemp > ((float)treeCounts) / 100f)
                {
                    EditorUtility.DisplayProgressBar("Import trees from Terrain... ", "Please wait... " + (int)(((float)j / (float)treeCounts) * 100f) + "%", (float)j / (float)treeCounts);
                    countTemp = 0;
                }
            }
            

            foreach (AltTreesPatch key in tempListPatches.Keys)
            {
                key.addTreesImport(tempListPatches[key].ToArray(), true);
            }

            
            if (isDeleteTreesFromTerrain)
            {
                terrainData.treeInstances = new TreeInstance[0];
                if(isDeletePrototypesFromTerrain)
                    terrainData.treePrototypes = new TreePrototype[0];

                terrainData.RefreshPrototypes();
                terrainTempImport.Flush();
            }
            else
                terrainTempImport.drawTreesAndFoliage = false;

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.ClearProgressBar();
            
            OnDisable();
            OnEnable();
            obj.gameObject.SetActive(true);
            obj.ReInit();
        }


        void Export()
        {
            EditorUtility.DisplayProgressBar("Export trees to Terrain... ", "Please wait... ", 0.0f);

            TerrainData terrainData = terrainTempExport.terrainData;

            int treePrototypesLength = terrainData.treePrototypes.Length;


            Dictionary<int, int> prototypesIndexArray = new Dictionary<int, int>();
            int treeInstancesStart = 0;
            TreeInstance[] tisTemp;

            if (!isDeleteTreesFromTerrain)
            {
                for (int i = 0; i < prototypesListTemp.Count; i++)
                {
                    bool isOk = false;
                    for (int k = 0; k < treePrototypesLength; k++)
                    {
                        if (terrainData.treePrototypes[k].prefab.Equals(terrainTreesArrayExport[i]))
                        {
                            prototypesIndexArray.Add(prototypesListTemp[i], k);
                            k = treePrototypesLength;
                            isOk = true;
                        }
                    }
                    if (!isOk)
                    {
                        TreePrototype[] tpsTemp = new TreePrototype[treePrototypesLength + 1];
                        TreePrototype tpTemp = new TreePrototype();
                        for (int k = 0; k < treePrototypesLength; k++)
                        {
                            tpsTemp[k] = terrainData.treePrototypes[k];
                        }
                        tpTemp.prefab = terrainTreesArrayExport[i];

                        tpsTemp[treePrototypesLength] = tpTemp;
                        terrainData.treePrototypes = tpsTemp;

                        prototypesIndexArray.Add(prototypesListTemp[i], treePrototypesLength);
                        treePrototypesLength++;
                    }
                }

                treeInstancesStart = terrainData.treeInstances.Length;
                tisTemp = new TreeInstance[terrainData.treeInstances.Length + attTemp.Count];

                for (int k = 0; k < terrainData.treeInstances.Length; k++)
                {
                    tisTemp[k] = terrainData.treeInstances[k];
                }

            }
            else
            {
                if (!isDeletePrototypesFromTerrain)
                {
                    for (int i = 0; i < prototypesListTemp.Count; i++)
                    {
                        bool isOk = false;
                        for (int k = 0; k < treePrototypesLength; k++)
                        {
                            if (terrainData.treePrototypes[k].prefab.Equals(terrainTreesArrayExport[i]))
                            {
                                prototypesIndexArray.Add(prototypesListTemp[i], k);
                                k = treePrototypesLength;
                                isOk = true;
                            }
                        }
                        if (!isOk)
                        {
                            TreePrototype[] tpsTemp = new TreePrototype[treePrototypesLength + 1];
                            TreePrototype tpTemp = new TreePrototype();
                            for (int k = 0; k < treePrototypesLength; k++)
                            {
                                tpsTemp[k] = terrainData.treePrototypes[k];
                            }
                            tpTemp.prefab = terrainTreesArrayExport[i];

                            tpsTemp[treePrototypesLength] = tpTemp;
                            terrainData.treePrototypes = tpsTemp;

                            prototypesIndexArray.Add(prototypesListTemp[i], treePrototypesLength);
                            treePrototypesLength++;
                        }
                    }
                }
                else
                {
                    TreePrototype[] tpsTemp = new TreePrototype[prototypesListTemp.Count];
                    for (int i = 0; i < prototypesListTemp.Count; i++)
                    {
                        TreePrototype tpTemp = new TreePrototype();
                        tpTemp.prefab = terrainTreesArrayExport[i];
                        tpsTemp[i] = tpTemp;
                        prototypesIndexArray.Add(prototypesListTemp[i], i);
                    }
                    terrainData.treePrototypes = tpsTemp;
                    treePrototypesLength = prototypesListTemp.Count;
                }

                treeInstancesStart = 0;
                tisTemp = new TreeInstance[attTemp.Count];
            }

            Color colT = Color.white;
            //colT.a = 0;

            for (int i = 0; i < attTemp.Count; i++)
            {
                Vector3 posTemp = (attTemp[i].getPosWorld() - terrainTempExport.GetPosition());
                posTemp.x = posTemp.x / terrainData.size.x;
                posTemp.y = posTemp.y / terrainData.size.y;
                posTemp.z = posTemp.z / terrainData.size.z;

                TreeInstance tiTemp = new TreeInstance();
                tiTemp.color = colT;
                tiTemp.lightmapColor = colT;
                tiTemp.heightScale = attTemp[i].heightScale;
                tiTemp.position = posTemp;
                tiTemp.prototypeIndex = prototypesIndexArray[attTemp[i].idPrototype];
                tiTemp.rotation = attTemp[i].rotation / 57.2958f;
                tiTemp.widthScale = attTemp[i].widthScale;
                tisTemp[treeInstancesStart + i] = tiTemp;
            }

            terrainData.treeInstances = tisTemp;


            terrainData.RefreshPrototypes();
            terrainTempExport.Flush();


            if (isDeleteTreesFromAltTrees)
            {
                //List<int> removedTrees = new List<int>();
                //List<AltTreesTrees> removedTreesNoGroup = new List<AltTreesTrees>();
                for (int i = 0; i < listPatchesExport.Length; i++)
                {
                    //AltTreesTrees[] attArrayTemp = listPatchesExport[i].getTreesForExport(new Vector2((terrainTempExport.transform.position + obj.altTreesManager.jump * obj.altTreesManagerData.sizePatch).x, (terrainTempExport.transform.position + obj.altTreesManager.jump * obj.altTreesManagerData.sizePatch).z), terrainData.size.x, terrainData.size.z);

                    AltTreesTrees[] removedTreesTemp = listPatchesExport[i].getTreesForExport(new Vector2((terrainTempExport.transform.position + obj.altTreesManager.jump * obj.altTreesManagerData.sizePatch).x, (terrainTempExport.transform.position + obj.altTreesManager.jump * obj.altTreesManagerData.sizePatch).z), terrainData.size.x, terrainData.size.z);

                    List<int> removedTreesTempIds = new List<int>();
                    for (int p = 0; p < removedTreesTemp.Length; p++)
                    {
                        removedTreesTempIds.Add(removedTreesTemp[p].idTree);
                    }

                    //removedTrees.Clear();
                    //removedTreesNoGroup.Clear();
                    //if (listPatchesExport[i].removeTrees(new Vector2((terrainTempExport.transform.position + obj.altTreesManager.jump * obj.altTreesManagerData.sizePatch).x, (terrainTempExport.transform.position + obj.altTreesManager.jump * obj.altTreesManagerData.sizePatch).z), terrainData.size.x, terrainData.size.z, removedTrees, false))
                    if (removedTreesTemp.Length > 0)
                    {
                        if (listPatchesExport[i].treesCount == listPatchesExport[i].treesEmptyCount + removedTreesTemp.Length && listPatchesExport[i].treesNoGroupCount == listPatchesExport[i].treesNoGroupEmptyCount/* + removedTreesNoGroup.Count*/)
                            obj.removePatch(listPatchesExport[i]);
                        else
                        {
                            if (removedTreesTemp.Length > 0)
                                listPatchesExport[i].EditDataFileTrees(null, 0, removedTreesTempIds);

                            if (listPatchesExport[i].treesCount == listPatchesExport[i].treesEmptyCount)
                            {
                                if (listPatchesExport[i].treesData != null)
                                {
                                    listPatchesExport[i].trees = new AltTreesTrees[0];
                                    listPatchesExport[i].treesCount = 0;
                                    listPatchesExport[i].treesEmptyCount = 0;
                                    listPatchesExport[i].treesEmpty = new int[0];
                                    AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(listPatchesExport[i].treesData));

                                    EditorUtility.SetDirty(listPatchesExport[i].altTreesManagerData);
                                    AssetDatabase.SaveAssets();
                                    AssetDatabase.Refresh();
                                }
                            }

                            /*if (removedTreesNoGroup.Count > 0)
                                listPatchesExport[i].EditDataFileObjects(-1, removedTreesNoGroup);

                            if (listPatchesExport[i].treesNoGroupCount == listPatchesExport[i].treesNoGroupEmptyCount)
                            {
                                if (listPatchesExport[i].treesNoGroupData != null)
                                {
                                    listPatchesExport[i].treesNoGroupCount = 0;
                                    listPatchesExport[i].treesNoGroupEmptyCount = 0;
                                    AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(listPatchesExport[i].treesNoGroupData));

                                    EditorUtility.SetDirty(listPatchesExport[i].altTreesManagerData);
                                    AssetDatabase.SaveAssets();
                                    AssetDatabase.Refresh();
                                }
                            }*/
                        }
                    }
                }



                EditorUtility.SetDirty(obj.altTreesManagerData);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();


                OnDisable();
                OnEnable();
                obj.gameObject.SetActive(true);
                obj.ReInit();
            }
            else
            {
                EditorUtility.SetDirty(obj.altTreesManagerData);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                obj.altTreesManager.destroy(true);
                obj.gameObject.SetActive(false);
            }

            attTemp = null;
            terrainTempExportStar = null;
            terrainTempExport.drawTreesAndFoliage = true;
            EditorUtility.ClearProgressBar();
        }

        
        void massPlace()
        {
            #if UNITY_EDITOR
            {
                EditorUtility.DisplayProgressBar("Mass Placing... ", "Placing... ", 0.0f);
            }
            #endif

            Vector3 vect3Temp = new Vector3();
            float randX = 0f;
            float randZ = 0f;
            Dictionary<AltTreesPatch, List<AddTreesPositionsStruct>> tempListPatches = new Dictionary<AltTreesPatch, List<AddTreesPositionsStruct>>();
            
            int countPlaced = 0;
            int percentAdded = 0;


            for (int i = 0; i < countMassPlace; i++)
            {
                randX = UnityEngine.Random.value;
                randZ = UnityEngine.Random.value;
                vect3Temp.x = terrainTempMassPlace.GetPosition().x + (terrainTempMassPlace.terrainData.size.x - 0.1f) * randX;
                vect3Temp.z = terrainTempMassPlace.GetPosition().z + (terrainTempMassPlace.terrainData.size.z - 0.1f) * randZ;
                vect3Temp.y = terrainTempMassPlace.SampleHeight(vect3Temp);

                if (Vector3.Angle(terrainTempMassPlace.terrainData.GetInterpolatedNormal(randX, randZ).normalized, Vector3.up) <= angleLimit.floatValue)
                {
                    AltTreesPatch tempPatch = getPatch(vect3Temp);

                    if (tempListPatches.ContainsKey(tempPatch))
                    {
                        tempListPatches[tempPatch].Add(new AddTreesPositionsStruct(vect3Temp, dataLinks.altTrees[treeIdsTemp[idTreeSelected.intValue]]));
                    }
                    else
                    {
                        tempPatch.checkTreePrototype(dataLinks.altTrees[treeIdsTemp[idTreeSelected.intValue]].id, dataLinks.altTrees[treeIdsTemp[idTreeSelected.intValue]], true, true);
                        tempListPatches.Add(tempPatch, new List<AddTreesPositionsStruct>());
                        tempListPatches[tempPatch].Add(new AddTreesPositionsStruct(vect3Temp, dataLinks.altTrees[treeIdsTemp[idTreeSelected.intValue]]));
                    }
                    
                    #if UNITY_EDITOR
                    {
                        countPlaced++;
                        if(countPlaced >= (countMassPlace / 20f))
                        {
                            countPlaced = 0;
                            percentAdded++;

                            EditorUtility.DisplayProgressBar("Mass Placing... ", "Placing... ", percentAdded * 0.1f);
                        }
                    }
                    #endif
                }
                else
                    i--;
            }

            #if UNITY_EDITOR
            {
                EditorUtility.ClearProgressBar();
            }
            #endif

            int num = 1;
            foreach (AltTreesPatch key in tempListPatches.Keys)
            {
                key.addTrees(tempListPatches[key].ToArray(), randomRotation.boolValue, isRandomHeight.boolValue, height.floatValue, heightRandom.floatValue,
                             lockWidthToHeight.boolValue, isRandomWidth.boolValue, width.floatValue, widthRandom.floatValue, hueColorLeaves, hueColorBark, isRandomHueLeaves.boolValue, isRandomHueBark.boolValue, true, "Mass Placing... (" + num + " / " + tempListPatches.Keys.Count + ")");
                EditorUtility.SetDirty(key.altTreesManagerData);
                num++;
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }


        void getDataLinks()
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

                if(dataLinks == null)
                {
                    obj.dataLinksIsCorrupted();
                }
            }
        }


        public int updateDataQuadsTempId = 0;
        public List<UpdateDataQuadsTemp> updateDataQuadsTempList;
        public bool checkTreesAddStop = false;

        void resizePatches(int sizePatchTemp)
        {
            AltTreesPatch[] patchesTemp = obj.altTreesManagerData.patches;

            string nameDirBackup = "";

            if (!System.IO.Directory.Exists("AltSystems/AltTrees/AltTreesDataBackups/" + obj.getIdManager()))
            {
                nameDirBackup = "AltSystems/AltTrees/AltTreesDataBackups/" + obj.getIdManager();
                System.IO.Directory.CreateDirectory(nameDirBackup);
            }
            else
            {
                bool stop = false;
                int t = 2;
                while (!stop)
                {
                    if (!System.IO.Directory.Exists("AltSystems/AltTrees/AltTreesDataBackups/" + obj.getIdManager() + "_" + t))
                    {
                        nameDirBackup = "AltSystems/AltTrees/AltTreesDataBackups/" + obj.getIdManager() + "_" + t;
                        System.IO.Directory.CreateDirectory(nameDirBackup);
                        stop = true;
                    }
                    else
                        t++;
                }
            }

            List<AltTreesTrees> tempObjects = new List<AltTreesTrees>();

            for (int i = 0; i < patchesTemp.Length; i++)
            {
                string strTemp = "";
                if (patchesTemp[i].treesData != null)
                {
                    strTemp = AssetDatabase.GetAssetPath(patchesTemp[i].treesData);
                    System.IO.File.Move(strTemp, nameDirBackup + "/treesData_" + patchesTemp[i].stepX + "_" + patchesTemp[i].stepY + ".bytes");

                    if (System.IO.File.Exists(strTemp + ".meta"))
                        System.IO.File.Delete(strTemp + ".meta");
                }
                if (patchesTemp[i].treesNoGroupData != null)
                {
                    byte[] bytesTemp = System.IO.File.ReadAllBytes(AssetDatabase.GetAssetPath(patchesTemp[i].treesNoGroupData));

                    if (bytesTemp != null && bytesTemp.Length > 0)
                    {
                        int version = AltUtilities.ReadBytesInt(bytesTemp, 0);

                        Vector3 _pos = new Vector3();
                        int _idPrototype;
                        Color _color = new Color();
                        Color _colorBark = new Color();
                        float _rotation;
                        float _heightScale;
                        float _widthScale;
                        
                        if (version == 2)
                        {
                            int countQuads = AltUtilities.ReadBytesInt(bytesTemp, 8);

                            for (int g = 0; g < countQuads; g++)
                            {
                                int addrObjs = AltUtilities.ReadBytesInt(bytesTemp, 20 + g * 4);
                                int countObjs = 0;
                                int countAll = 0;

                                while (addrObjs != -1)
                                {
                                    countObjs = AltUtilities.ReadBytesInt(bytesTemp, addrObjs + 4);

                                    if (countObjs > 0)
                                    {
                                        for (int h = 0; h < countObjs; h++)
                                        {
                                            if (AltUtilities.ReadBytesBool(bytesTemp, addrObjs + 8 + h * 61 + 0))
                                            {
                                                _pos = AltUtilities.ReadBytesVector3(bytesTemp, addrObjs + 8 + h * 61 + 1);
                                                _idPrototype = AltUtilities.ReadBytesInt(bytesTemp, addrObjs + 8 + h * 61 + 13);
                                                _color = AltUtilities.ReadBytesColor(bytesTemp, addrObjs + 8 + h * 61 + 17);
                                                _colorBark = AltUtilities.ReadBytesColor(bytesTemp, addrObjs + 8 + h * 61 + 33);
                                                _rotation = AltUtilities.ReadBytesFloat(bytesTemp, addrObjs + 8 + h * 61 + 49);
                                                _heightScale = AltUtilities.ReadBytesFloat(bytesTemp, addrObjs + 8 + h * 61 + 53);
                                                _widthScale = AltUtilities.ReadBytesFloat(bytesTemp, addrObjs + 8 + h * 61 + 57);

                                                AltTreesTrees att = new AltTreesTrees(_pos, countAll + h, _idPrototype, true, _color, _colorBark, _rotation, _heightScale, _widthScale, patchesTemp[i]);

                                                tempObjects.Add(att);
                                            }
                                        }
                                        countAll += countObjs;
                                    }
                                    addrObjs = AltUtilities.ReadBytesInt(bytesTemp, addrObjs);
                                }
                            }
                        }
                    }

                    strTemp = AssetDatabase.GetAssetPath(patchesTemp[i].treesNoGroupData);
                    System.IO.File.Move(strTemp, nameDirBackup + "/treesData_" + patchesTemp[i].stepX + "_" + patchesTemp[i].stepY + "_objs.bytes");

                    if (System.IO.File.Exists(strTemp + ".meta"))
                        System.IO.File.Delete(strTemp + ".meta");
                }
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            obj.altTreesManagerData.patches = new AltTreesPatch[0];

            Dictionary<AltTreesPatch, List<AddTreesStruct>> tempListPatches = new Dictionary<AltTreesPatch, List<AddTreesStruct>>();


            AltTreesPatch tempPatch = null;

            if (tempObjects.Count > 0)
            {
                updateDataQuadsTempList = new List<UpdateDataQuadsTemp>();

                for (int h = 0; h < tempObjects.Count; h++)
                {
                    tempPatch = getPatch(tempObjects[h].getPosWorld(), sizePatchTemp, false);

                    tempPatch.checkTreePrototype(tempObjects[h].idPrototype, dataLinks.getAltTree(tempObjects[h].idPrototype), false, false);
                    tempPatch.tempTrees.Add(tempObjects[h]);
                }
                


                for (int i = 0; i < obj.altTreesManagerData.patches.Length; i++)
                {
                    tempPatch = obj.altTreesManagerData.patches[i];

                    
                    updateDataQuadsTempId = 1;
                    updateDataQuadsTempList.Clear();

                    UpdateDataQuadsTemp qt = new UpdateDataQuadsTemp(tempPatch.stepX * sizePatchTemp + (float)sizePatchTemp / 2f, tempPatch.stepY * sizePatchTemp + (float)sizePatchTemp / 2f, sizePatchTemp, 0, obj.altTreesManagerData.maxLOD, updateDataQuadsTempList, ref updateDataQuadsTempId, 0);
                    UpdateDataQuadsTemp[] qtArray = updateDataQuadsTempList.ToArray();


                    for (int h = 0; h < updateDataQuadsTempList.Count; h++)
                        updateDataQuadsTempList[h].objs.Clear();

                    int tempObjectsCount = tempPatch.tempTrees.Count;
                    Vector2 pos2D;

                    for (int h = 0; h < tempObjectsCount; h++)
                    {
                        pos2D = tempPatch.tempTrees[h].getPosWorld2DWithJump();
                        checkTreesAddStop = false;
                        qt.checkTreesAdd(pos2D.x, pos2D.y, tempPatch.tempTrees[h], ref checkTreesAddStop);
                    }

                    

                    int summBytes = 20 + qtArray.Length * 4;
                    
                    using (BinaryWriter writer = new BinaryWriter(File.Open("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + obj.getIdManager() + "/treesData_" + tempPatch.stepX + "_" + tempPatch.stepY + "_objs.bytes", FileMode.Create)))
                    {
                        byte[] bytesTemp2 = new byte[summBytes];
                        AltUtilities.WriteBytes(2, bytesTemp2, 0);
                        AltUtilities.WriteBytes(summBytes, bytesTemp2, 4);
                        AltUtilities.WriteBytes(qtArray.Length, bytesTemp2, 8);
                        AltUtilities.WriteBytes(tempObjectsCount, bytesTemp2, 12);
                        AltUtilities.WriteBytes(0, bytesTemp2, 16);

                        for (int h = 0; h < qtArray.Length; h++)
                        {
                            AltUtilities.WriteBytes(-1, bytesTemp2, 20 + h*4);
                        }
                            
                        writer.Write(bytesTemp2);
                    }
                    
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    tempPatch.treesNoGroupData = (TextAsset)AssetDatabase.LoadAssetAtPath("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + obj.getIdManager() + "/treesData_" + tempPatch.stepX + "_" + tempPatch.stepY + "_objs.bytes", typeof(TextAsset));
                    EditorUtility.SetDirty(obj.altTreesManagerData);


                        
                    int startBytes = 0;
                    byte[] bytes4Temp = new byte[4];
                    byte[] bytes61Temp = new byte[61];
                    AltTreesTrees attTemp;


                    if (File.Exists("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + obj.getIdManager() + "/treesData_" + tempPatch.stepX + "_" + tempPatch.stepY + "_objs.bytesTemp"))
                        File.Delete("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + obj.getIdManager() + "/treesData_" + tempPatch.stepX + "_" + tempPatch.stepY + "_objs.bytesTemp");

                    if (File.Exists("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + obj.getIdManager() + "/treesData_" + tempPatch.stepX + "_" + tempPatch.stepY + "_objs.bytesTemp.meta"))
                        File.Delete("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + obj.getIdManager() + "/treesData_" + tempPatch.stepX + "_" + tempPatch.stepY + "_objs.bytesTemp.meta");

                    File.Copy("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + obj.getIdManager() + "/treesData_" + tempPatch.stepX + "_" + tempPatch.stepY + "_objs.bytes", "Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + obj.getIdManager() + "/treesData_" + tempPatch.stepX + "_" + tempPatch.stepY + "_objs.bytesTemp");

                    using (BinaryWriter writer = new BinaryWriter(File.Open("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + obj.getIdManager() + "/treesData_" + tempPatch.stepX + "_" + tempPatch.stepY + "_objs.bytesTemp", FileMode.Open)))
                    {
                        for (int g = 0; g < qtArray.Length; g++)
                        {
                            for (int h = 0; h < qtArray[g].objs.Count; h++)
                            {
                                attTemp = qtArray[g].objs[h];

                                if (h == 0)
                                {
                                    AltUtilities.WriteBytes(summBytes, bytes4Temp, 0);
                                    writer.Seek(20 + g * 4, SeekOrigin.Begin);
                                    writer.Write(bytes4Temp);
                                        
                                    writer.Seek(summBytes, SeekOrigin.Begin);
                                    startBytes = summBytes + 4;

                                    AltUtilities.WriteBytes(-1, bytes4Temp, 0);
                                    writer.Write(bytes4Temp);
                                    AltUtilities.WriteBytes(-1, bytes4Temp, 0);
                                    writer.Write(bytes4Temp);

                                    summBytes += 8;
                                }

                                AltUtilities.WriteBytes(true, bytes61Temp, 0);
                                AltUtilities.WriteBytes(/*attTemp.pos*/ tempPatch.getTreePosLocal(attTemp.getPosWorld(), Vector3.zero, Vector3.zero, sizePatchTemp), bytes61Temp, 1);
                                AltUtilities.WriteBytes(attTemp.idPrototype, bytes61Temp, 13);
                                AltUtilities.WriteBytes(attTemp.color, bytes61Temp, 17);
                                AltUtilities.WriteBytes(attTemp.colorBark, bytes61Temp, 33);
                                AltUtilities.WriteBytes(attTemp.rotation, bytes61Temp, 49);
                                AltUtilities.WriteBytes(attTemp.heightScale, bytes61Temp, 53);
                                AltUtilities.WriteBytes(attTemp.widthScale, bytes61Temp, 57);
                                writer.Write(bytes61Temp);

                                summBytes += 61;
                            }
                            if (qtArray[g].objs.Count != 0)
                            {
                                writer.Seek(startBytes, SeekOrigin.Begin);
                                AltUtilities.WriteBytes(qtArray[g].objs.Count, bytes4Temp, 0);
                                writer.Write(bytes4Temp);
                            }
                        }

                        writer.Seek(4, SeekOrigin.Begin);
                        AltUtilities.WriteBytes(summBytes, bytes4Temp, 0);
                        writer.Write(bytes4Temp);
                    }

                    if (File.Exists("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + obj.getIdManager() + "/treesData_" + tempPatch.stepX + "_" + tempPatch.stepY + "_objs.bytes"))
                        File.Replace("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + obj.getIdManager() + "/treesData_" + tempPatch.stepX + "_" + tempPatch.stepY + "_objs.bytesTemp", "Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + obj.getIdManager() + "/treesData_" + tempPatch.stepX + "_" + tempPatch.stepY + "_objs.bytes", null);
                    else
                        File.Move("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + obj.getIdManager() + "/treesData_" + tempPatch.stepX + "_" + tempPatch.stepY + "_objs.bytesTemp", "Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + obj.getIdManager() + "/treesData_" + tempPatch.stepX + "_" + tempPatch.stepY + "_objs.bytes");

                    tempPatch.treesNoGroupCount = 0;
                    tempPatch.treesNoGroupEmptyCount = 0;


                    tempPatch.tempTrees.Clear();
                }
            }





            for (int i = 0; i < patchesTemp.Length; i++)
            {
                if (patchesTemp[i].trees != null)
                {
                    for (int h = 0; h < patchesTemp[i].trees.Length; h++)
                    {
                        if (patchesTemp[i].trees[h] != null && patchesTemp[i].trees[h].noNull)
                        {
                            tempPatch = getPatch(patchesTemp[i].trees[h].getPosWorld(), sizePatchTemp, false);

                            if (tempListPatches.ContainsKey(tempPatch))
                            {
                                tempPatch.checkTreePrototype(patchesTemp[i].trees[h].idPrototype, dataLinks.getAltTree(patchesTemp[i].trees[h].idPrototype), false, false);
                                tempListPatches[tempPatch].Add(new AddTreesStruct(patchesTemp[i].trees[h].getPosWorld(), patchesTemp[i].trees[h].idPrototype, false, patchesTemp[i].trees[h].rotation, patchesTemp[i].trees[h].heightScale, patchesTemp[i].trees[h].widthScale, patchesTemp[i].trees[h].color, patchesTemp[i].trees[h].colorBark));
                            }
                            else
                            {
                                if (tempPatch == null)
                                    Debug.Log("tempPatch null");
                                if (tempPatch == null)
                                    Debug.Log("tempPatch null");
                                if (patchesTemp[i] == null)
                                    Debug.Log("patchesTemp[i] null");
                                if (patchesTemp[i].trees[h] == null)
                                    Debug.Log("patchesTemp[i].trees[h] null");
                                tempPatch.checkTreePrototype(patchesTemp[i].trees[h].idPrototype, dataLinks.getAltTree(patchesTemp[i].trees[h].idPrototype), false, false);
                                tempListPatches.Add(tempPatch, new List<AddTreesStruct>());
                                tempListPatches[tempPatch].Add(new AddTreesStruct(patchesTemp[i].trees[h].getPosWorld(), patchesTemp[i].trees[h].idPrototype, false, patchesTemp[i].trees[h].rotation, patchesTemp[i].trees[h].heightScale, patchesTemp[i].trees[h].widthScale, patchesTemp[i].trees[h].color, patchesTemp[i].trees[h].colorBark));
                            }
                        }
                    }
                }
                /*if (patchesTemp[i].treesNoGroup != null)
                {
                    for (int h = 0; h < patchesTemp[i].treesNoGroup.Length; h++)
                    {
                        AltTreesPatch tempPatch = getPatch(patchesTemp[i].treesNoGroup[h].getPosWorld(), sizePatchTemp);

                        if (tempListPatches.ContainsKey(tempPatch))
                        {
                            tempPatch.checkTreePrototype(patchesTemp[i].treesNoGroup[h].idPrototype, dataLinks.getAltTree(patchesTemp[i].treesNoGroup[h].idPrototype));
                            tempListPatches[tempPatch].Add(new AddTreesStruct(patchesTemp[i].treesNoGroup[h].getPosWorld(), patchesTemp[i].treesNoGroup[h].idPrototype, true, patchesTemp[i].treesNoGroup[h].rotation, patchesTemp[i].treesNoGroup[h].heightScale, patchesTemp[i].treesNoGroup[h].widthScale, patchesTemp[i].treesNoGroup[h].color, patchesTemp[i].treesNoGroup[h].colorBark));
                        }
                        else
                        {
                            tempPatch.checkTreePrototype(patchesTemp[i].treesNoGroup[h].idPrototype, dataLinks.getAltTree(patchesTemp[i].treesNoGroup[h].idPrototype));
                            tempListPatches.Add(tempPatch, new List<AddTreesStruct>());
                            tempListPatches[tempPatch].Add(new AddTreesStruct(patchesTemp[i].treesNoGroup[h].getPosWorld(), patchesTemp[i].treesNoGroup[h].idPrototype, true, patchesTemp[i].treesNoGroup[h].rotation, patchesTemp[i].treesNoGroup[h].heightScale, patchesTemp[i].treesNoGroup[h].widthScale, patchesTemp[i].treesNoGroup[h].color, patchesTemp[i].treesNoGroup[h].colorBark));
                        }
                    }
                }*/
                patchesTemp[i].disable();
            }


            obj.altTreesManagerData.sizePatch = sizePatchTemp;
            EditorUtility.SetDirty(obj.altTreesManagerData);
            obj.ReInit(true);

            foreach (AltTreesPatch key in tempListPatches.Keys)
            {
                key.addTreesImport(tempListPatches[key].ToArray(), true);
            }
            EditorUtility.SetDirty(obj.altTreesManagerData);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        void resizeMaxLOD()
        {
            string nameDirBackup = "";

            if (!System.IO.Directory.Exists("AltSystems/AltTrees/AltTreesDataBackups/" + obj.getIdManager()))
            {
                nameDirBackup = "AltSystems/AltTrees/AltTreesDataBackups/" + obj.getIdManager();
                System.IO.Directory.CreateDirectory(nameDirBackup);
            }
            else
            {
                bool stop = false;
                int t = 2;
                while (!stop)
                {
                    if (!System.IO.Directory.Exists("AltSystems/AltTrees/AltTreesDataBackups/" + obj.getIdManager() + "_" + t))
                    {
                        nameDirBackup = "AltSystems/AltTrees/AltTreesDataBackups/" + obj.getIdManager() + "_" + t;
                        System.IO.Directory.CreateDirectory(nameDirBackup);
                        stop = true;
                    }
                    else
                        t++;
                }
            }

            updateDataQuadsTempList = new List<UpdateDataQuadsTemp>();

            AltTreesPatch tempPatch = null;

            for (int i = 0; i < obj.altTreesManagerData.patches.Length; i++)
            {
                tempPatch = obj.altTreesManagerData.patches[i];
                tempPatch.tempTrees = new List<AltTreesTrees>();

                if (tempPatch.treesNoGroupData != null)
                {
                    byte[] bytesTemp = System.IO.File.ReadAllBytes(AssetDatabase.GetAssetPath(tempPatch.treesNoGroupData));

                    if (bytesTemp != null && bytesTemp.Length > 0)
                    {
                        int version = AltUtilities.ReadBytesInt(bytesTemp, 0);

                        Vector3 _pos = new Vector3();
                        int _idPrototype;
                        Color _color = new Color();
                        Color _colorBark = new Color();
                        float _rotation;
                        float _heightScale;
                        float _widthScale;

                        if (version == 2)
                        {
                            int countQuads = AltUtilities.ReadBytesInt(bytesTemp, 8);

                            for (int g = 0; g < countQuads; g++)
                            {
                                int addrObjs = AltUtilities.ReadBytesInt(bytesTemp, 20 + g * 4);
                                int countObjs = 0;
                                int countAll = 0;

                                while (addrObjs != -1)
                                {
                                    countObjs = AltUtilities.ReadBytesInt(bytesTemp, addrObjs + 4);

                                    if (countObjs > 0)
                                    {
                                        for (int h = 0; h < countObjs; h++)
                                        {
                                            if (AltUtilities.ReadBytesBool(bytesTemp, addrObjs + 8 + h * 61 + 0))
                                            {
                                                _pos = AltUtilities.ReadBytesVector3(bytesTemp, addrObjs + 8 + h * 61 + 1);
                                                _idPrototype = AltUtilities.ReadBytesInt(bytesTemp, addrObjs + 8 + h * 61 + 13);
                                                _color = AltUtilities.ReadBytesColor(bytesTemp, addrObjs + 8 + h * 61 + 17);
                                                _colorBark = AltUtilities.ReadBytesColor(bytesTemp, addrObjs + 8 + h * 61 + 33);
                                                _rotation = AltUtilities.ReadBytesFloat(bytesTemp, addrObjs + 8 + h * 61 + 49);
                                                _heightScale = AltUtilities.ReadBytesFloat(bytesTemp, addrObjs + 8 + h * 61 + 53);
                                                _widthScale = AltUtilities.ReadBytesFloat(bytesTemp, addrObjs + 8 + h * 61 + 57);

                                                AltTreesTrees att = new AltTreesTrees(_pos, countAll + h, _idPrototype, true, _color, _colorBark, _rotation, _heightScale, _widthScale, tempPatch);
                                                
                                                tempPatch.tempTrees.Add(att);
                                            }
                                        }
                                        countAll += countObjs;
                                    }
                                    addrObjs = AltUtilities.ReadBytesInt(bytesTemp, addrObjs);
                                }
                            }
                        }
                    }

                    string strTemp = AssetDatabase.GetAssetPath(tempPatch.treesNoGroupData);

                    System.IO.File.Move(strTemp, nameDirBackup + "/treesData_" + tempPatch.stepX + "_" + tempPatch.stepY + "_objs.bytes");

                    if (System.IO.File.Exists(strTemp + ".meta"))
                        System.IO.File.Delete(strTemp + ".meta");



                    updateDataQuadsTempId = 1;
                    updateDataQuadsTempList.Clear();

                    UpdateDataQuadsTemp qt = new UpdateDataQuadsTemp(tempPatch.stepX * sizePatchTemp + (float)sizePatchTemp / 2f, tempPatch.stepY * sizePatchTemp + (float)sizePatchTemp / 2f, sizePatchTemp, 0, obj.altTreesManagerData.maxLOD, updateDataQuadsTempList, ref updateDataQuadsTempId, 0);
                    UpdateDataQuadsTemp[] qtArray = updateDataQuadsTempList.ToArray();


                    for (int h = 0; h < updateDataQuadsTempList.Count; h++)
                        updateDataQuadsTempList[h].objs.Clear();

                    int tempObjectsCount = tempPatch.tempTrees.Count;
                    Vector2 pos2D;

                    for (int h = 0; h < tempObjectsCount; h++)
                    {
                        pos2D = tempPatch.tempTrees[h].getPosWorld2DWithJump();
                        checkTreesAddStop = false;
                        qt.checkTreesAdd(pos2D.x, pos2D.y, tempPatch.tempTrees[h], ref checkTreesAddStop);
                    }



                    int summBytes = 20 + qtArray.Length * 4;
                    
                    using (BinaryWriter writer = new BinaryWriter(File.Open("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + obj.getIdManager() + "/treesData_" + tempPatch.stepX + "_" + tempPatch.stepY + "_objs.bytes", FileMode.Create)))
                    {
                        byte[] bytesTemp2 = new byte[summBytes];
                        AltUtilities.WriteBytes(2, bytesTemp2, 0);
                        AltUtilities.WriteBytes(summBytes, bytesTemp2, 4);
                        AltUtilities.WriteBytes(qtArray.Length, bytesTemp2, 8);
                        AltUtilities.WriteBytes(tempObjectsCount, bytesTemp2, 12);
                        AltUtilities.WriteBytes(0, bytesTemp2, 16);

                        for (int h = 0; h < qtArray.Length; h++)
                        {
                            AltUtilities.WriteBytes(-1, bytesTemp2, 20 + h * 4);
                        }

                        writer.Write(bytesTemp2);
                    }
                    
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    tempPatch.treesNoGroupData = (TextAsset)AssetDatabase.LoadAssetAtPath("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + obj.getIdManager() + "/treesData_" + tempPatch.stepX + "_" + tempPatch.stepY + "_objs.bytes", typeof(TextAsset));
                    EditorUtility.SetDirty(obj.altTreesManagerData);



                    int startBytes = 0;
                    byte[] bytes4Temp = new byte[4];
                    byte[] bytes61Temp = new byte[61];
                    AltTreesTrees attTemp;


                    if (File.Exists("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + obj.getIdManager() + "/treesData_" + tempPatch.stepX + "_" + tempPatch.stepY + "_objs.bytesTemp"))
                        File.Delete("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + obj.getIdManager() + "/treesData_" + tempPatch.stepX + "_" + tempPatch.stepY + "_objs.bytesTemp");

                    if (File.Exists("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + obj.getIdManager() + "/treesData_" + tempPatch.stepX + "_" + tempPatch.stepY + "_objs.bytesTemp.meta"))
                        File.Delete("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + obj.getIdManager() + "/treesData_" + tempPatch.stepX + "_" + tempPatch.stepY + "_objs.bytesTemp.meta");

                    File.Copy("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + obj.getIdManager() + "/treesData_" + tempPatch.stepX + "_" + tempPatch.stepY + "_objs.bytes", "Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + obj.getIdManager() + "/treesData_" + tempPatch.stepX + "_" + tempPatch.stepY + "_objs.bytesTemp");

                    using (BinaryWriter writer = new BinaryWriter(File.Open("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + obj.getIdManager() + "/treesData_" + tempPatch.stepX + "_" + tempPatch.stepY + "_objs.bytesTemp", FileMode.Open)))
                    {
                        for (int g = 0; g < qtArray.Length; g++)
                        {
                            for (int h = 0; h < qtArray[g].objs.Count; h++)
                            {
                                attTemp = qtArray[g].objs[h];

                                if (h == 0)
                                {
                                    AltUtilities.WriteBytes(summBytes, bytes4Temp, 0);
                                    writer.Seek(20 + g * 4, SeekOrigin.Begin);
                                    writer.Write(bytes4Temp);

                                    writer.Seek(summBytes, SeekOrigin.Begin);
                                    startBytes = summBytes + 4;

                                    AltUtilities.WriteBytes(-1, bytes4Temp, 0);
                                    writer.Write(bytes4Temp);
                                    AltUtilities.WriteBytes(-1, bytes4Temp, 0);
                                    writer.Write(bytes4Temp);

                                    summBytes += 8;
                                }

                                AltUtilities.WriteBytes(true, bytes61Temp, 0);
                                AltUtilities.WriteBytes(/*attTemp.pos*/ tempPatch.getTreePosLocal(attTemp.getPosWorld(), Vector3.zero, Vector3.zero, sizePatchTemp), bytes61Temp, 1);
                                AltUtilities.WriteBytes(attTemp.idPrototype, bytes61Temp, 13);
                                AltUtilities.WriteBytes(attTemp.color, bytes61Temp, 17);
                                AltUtilities.WriteBytes(attTemp.colorBark, bytes61Temp, 33);
                                AltUtilities.WriteBytes(attTemp.rotation, bytes61Temp, 49);
                                AltUtilities.WriteBytes(attTemp.heightScale, bytes61Temp, 53);
                                AltUtilities.WriteBytes(attTemp.widthScale, bytes61Temp, 57);
                                writer.Write(bytes61Temp);

                                summBytes += 61;
                            }
                            if (qtArray[g].objs.Count != 0)
                            {
                                writer.Seek(startBytes, SeekOrigin.Begin);
                                AltUtilities.WriteBytes(qtArray[g].objs.Count, bytes4Temp, 0);
                                writer.Write(bytes4Temp);
                            }
                        }

                        writer.Seek(4, SeekOrigin.Begin);
                        AltUtilities.WriteBytes(summBytes, bytes4Temp, 0);
                        writer.Write(bytes4Temp);
                    }

                    if (File.Exists("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + obj.getIdManager() + "/treesData_" + tempPatch.stepX + "_" + tempPatch.stepY + "_objs.bytes"))
                        File.Replace("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + obj.getIdManager() + "/treesData_" + tempPatch.stepX + "_" + tempPatch.stepY + "_objs.bytesTemp", "Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + obj.getIdManager() + "/treesData_" + tempPatch.stepX + "_" + tempPatch.stepY + "_objs.bytes", null);
                    else
                        File.Move("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + obj.getIdManager() + "/treesData_" + tempPatch.stepX + "_" + tempPatch.stepY + "_objs.bytesTemp", "Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + obj.getIdManager() + "/treesData_" + tempPatch.stepX + "_" + tempPatch.stepY + "_objs.bytes");

                    tempPatch.treesNoGroupCount = 0;
                    tempPatch.treesNoGroupEmptyCount = 0;


                    tempPatch.tempTrees.Clear();
                }
            }

            
            EditorUtility.SetDirty(obj.altTreesManagerData);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        void CreateAltTreesManagerData()
        {
            if (!System.IO.Directory.Exists("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + obj.getIdManager()))
            {
                System.IO.Directory.CreateDirectory("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + obj.getIdManager());
            }

            if (!System.IO.File.Exists("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + obj.getIdManager() + "/altTreesManagerData.asset"))
            {
                AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<AltTreesManagerData>(), "Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + obj.getIdManager() + "/altTreesManagerData.asset");
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            obj.altTreesManagerData = (AltTreesManagerData)AssetDatabase.LoadAssetAtPath("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + obj.getIdManager() + "/altTreesManagerData.asset", typeof(AltTreesManagerData));
            EditorUtility.SetDirty(obj);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

    }
}