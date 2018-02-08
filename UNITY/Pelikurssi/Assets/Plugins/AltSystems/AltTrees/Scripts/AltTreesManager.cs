using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace AltSystems.AltTrees
{
    [ExecuteInEditMode]
    public class AltTreesManager : MonoBehaviour
    {
        public AltTrees altTreesMain;
        public bool isInit = false;
        public Object distanceCamerasLock = new Object();
        //public Transform[] cameras = new Transform[0];
        //public Vector3[] camerasPos = new Vector3[0];
        public AtiTemp[] treesCameras = new AtiTemp[0];
        public AtiTemp[] treesCamerasTemp = new AtiTemp[0];
        DistanceCamera[] camerasTemp = new DistanceCamera[0];
        public FrustumCamera[] frustumCameras;
        Transform frustumCameraTransformEditor;
        Vector3 frustumCameraTransformEditorPosStar;
        Quaternion frustumCameraTransformEditorRotStar;
        public Object frustumCamerasLock = new Object();
        public bool frustumCamerasInit = false;
        public bool distanceCamerasInit = false;
        public DistanceCamera[] distanceCameras;
        public float distanceCamerasSpeedTime;
        public Vector3 distanceCamerasSpeedVector;
        public float distanceCamerasSpeedTimeStar;
        public float maxSpeedCameras;
        public float maxSpeedCamerasTemp;
        public float[] maxSpeedCamerasArray = new float[5];
        static public Camera camEditor;

        public AltTreesQuad[] quads = new AltTreesQuad[0];
        public AltTreesPatch[] patches = new AltTreesPatch[0];

        int genPerFrame;

        GameObject goCubeDebug;

        MaterialPropertyBlock propBlock;
        Color colorTemp = new Color();
        MeshRenderer meshRendererTemp;
        MeshFilter meshFilterTemp;

        public Vector3 jump = new Vector3(0, 0, 0);
        public Vector3 jumpPos = new Vector3(0, 0, 0);

        bool GCCollect = false;

        Timer timerCheck;
        Object timerLock = new Object();
        bool timerLockStop = false;

        [System.NonSerialized]
        public AltTreesPool[] treesPoolArray = new AltTreesPool[0];
        public int[] treePrototypeIds = new int[0];
        GameObject goTemp;
        objBillboardPool objBillboardTemp;
	    Vector3 vectTemp;
        Bounds boundsTemp;

        bool isDestroyed = false;
        public Dictionary<GameObject, AltTreesTrees> treesList = new Dictionary<GameObject, AltTreesTrees>();
        int genPerFrameTemp = 0;

        [System.NonSerialized]
        List<AltTreesTrees> collidersUsedList = new List<AltTreesTrees>();

        //[System.NonSerialized]
        //List<objBillboardPool> objBillboardsPool = new List<objBillboardPool>();


        public List<AltTreesTrees> treesCrossFade = new List<AltTreesTrees>();
        //List<GameObject> rendersToRemove = new List<GameObject>();
        //int rendersToRemoveCounter = 0;

        System.Action<Plane[], Matrix4x4> ExtractPlanes;
        bool enabledExtractPlanes = false;

        public List<AltTreesQuad> quadsToRender = new List<AltTreesQuad>();
        
        public int drawGroupBillboardsCount = 0;
        public int drawBillboardsCount = 0;
        public int drawMeshesCount = 0;
        public int drawBillboardsCrossFadeCount = 0;
        public int drawMeshesCrossFadeCount = 0;
        public int drawCollidersCount = 0;
        public int drawColliderBillboardsCount = 0;

        static public int HueVariationLeave_PropertyID;
        static public int HueVariationBark_PropertyID;
        static public int Alpha_PropertyID;
        static public int Ind_PropertyID;
        static public int smoothValue_PropertyID;
        static public int Color_PropertyID;

        static public int Width_PropertyID;
        static public int Height_PropertyID;
        static public int Rotation_PropertyID;
        static public int HueVariation_PropertyID;

        public bool gpuInstancingSupport = false;
        int gpuInstancingCount;

        bool needInitColliders = false;
        public bool needUpdateScene = false;

        public int countPatchesforInit = 0;

        public int getPrototypeIndex(int idPrototype)
        {
            for(int i = 0; i < treePrototypeIds.Length; i++)
            {
                if (treePrototypeIds[i] == idPrototype)
                    return i;
            }

            return -1;
        }

        [System.NonSerialized]
        public bool initTimeStarted = false;

        [System.NonSerialized]
        int initTimeStartedCount = -1;
        System.Diagnostics.Stopwatch swTime = null;

        public void Init(AltTrees _altTreesMain)
        {
            if (isInit)
                return;

            initTimeStartedCount = -1;

            swTime = System.Diagnostics.Stopwatch.StartNew();
            //checkLoadingTimeTimerCount = 0;

            #if !(UNITY_5_2 || UNITY_5_3 || UNITY_5_4)
                gpuInstancingSupport = SystemInfo.supportsInstancing;
            #endif

            if(gpuInstancingSupport)
            {
                gpuInstancingCount = 500;

                if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.OpenGLES3 || SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.OpenGLCore || SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Metal)
                    gpuInstancingCount = Mathf.FloorToInt(gpuInstancingCount / 4);

                attDrawBillboards = new AltTreesTrees[gpuInstancingCount];
                attDrawBillboardsCrossFade = new AltTreesTrees[gpuInstancingCount];

                matrixDrawBillboards = new Matrix4x4[gpuInstancingCount];
                alphaDrawBillboards = new float[gpuInstancingCount];
                widthDrawBillboards = new float[gpuInstancingCount];
                heightDrawBillboards = new float[gpuInstancingCount];
                rotationDrawBillboards = new float[gpuInstancingCount];
                hueDrawBillboards = new Vector4[gpuInstancingCount];

                hueLeaveDraw = new Vector4[gpuInstancingCount];
                hueBarkDraw = new Vector4[gpuInstancingCount];
                indDraw = new float[gpuInstancingCount];
                smoothDraw = new float[gpuInstancingCount];
            }

            AltUtilities.initRandom();

            MethodInfo info = typeof(GeometryUtility).GetMethod("Internal_ExtractPlanes", BindingFlags.Static | BindingFlags.NonPublic, null, new System.Type[] { typeof(Plane[]), typeof(Matrix4x4) }, null);

            if (info == null)
            {
                enabledExtractPlanes = false;
                altTreesMain.LogError("ExtractPlanes is null");
            }
            else
            {
                ExtractPlanes = System.Delegate.CreateDelegate(typeof(System.Action<Plane[], Matrix4x4>), info) as System.Action<Plane[], Matrix4x4>;
                enabledExtractPlanes = true;
            }

            propBlock = new MaterialPropertyBlock();
            #if !(UNITY_5_2 || UNITY_5_3 || UNITY_5_4)
                mpbTemp = new MaterialPropertyBlock();
            #endif

            HueVariationLeave_PropertyID = Shader.PropertyToID("_HueVariationLeave");
            HueVariationBark_PropertyID = Shader.PropertyToID("_HueVariationBark");
            Alpha_PropertyID = Shader.PropertyToID("_Alpha");
            Ind_PropertyID = Shader.PropertyToID("_Ind");
            smoothValue_PropertyID = Shader.PropertyToID("_smoothValue");
            Color_PropertyID = Shader.PropertyToID("_Color");

            Width_PropertyID = Shader.PropertyToID("_Width");
            Height_PropertyID = Shader.PropertyToID("_Height");
            Rotation_PropertyID = Shader.PropertyToID("_Rotation");
            HueVariation_PropertyID = Shader.PropertyToID("_HueVariation");

            altTreesMain = _altTreesMain;

            genPerFrame = 1;
            if (!altTreesMain.isPlaying)
                genPerFrame = 10;

            vectTemp = new Vector3(1000000, 1000000, 1000000);
            
            if (altTreesMain.isPlaying)
            {
                if (altTreesMain.cameraModeFrustum == 0 || (altTreesMain.cameraModeFrustum == 1 && (altTreesMain.activeCameraFrustum == null || altTreesMain.activeCameraFrustum.Length == 0)))
                {
                    if (Camera.main != null && Camera.main.gameObject.activeInHierarchy && Camera.main.enabled)
                    {
                        lock (frustumCamerasLock)
                        {
                            frustumCameras = new FrustumCamera[1];
                            frustumCameras[0].cam = Camera.main;
                            frustumCameras[0].planes = new Plane[6];
                            frustumCameras[0].myPlanes = new MyPlane[6];
                            frustumCameras[0].myPlanes[0] = new MyPlane();
                            frustumCameras[0].myPlanes[1] = new MyPlane();
                            frustumCameras[0].myPlanes[2] = new MyPlane();
                            frustumCameras[0].myPlanes[3] = new MyPlane();
                            frustumCameras[0].myPlanes[4] = new MyPlane();
                            frustumCameras[0].myPlanes[5] = new MyPlane();
                        }

                        frustumCamerasInit = true;
                    }
                    else
                    {
                        Camera[] cams = Camera.allCameras;
                        if (cams != null && cams.Length > 0)
                        {
                            for (int i = 0; i < cams.Length; i++)
                            {
                                if (cams[i] != null && cams[i].gameObject.activeInHierarchy && cams[i].enabled)
                                {
                                    lock (frustumCamerasLock)
                                    {
                                        frustumCameras = new FrustumCamera[1];
                                        frustumCameras[0].cam = cams[i];
                                        frustumCameras[0].planes = new Plane[6];
                                        frustumCameras[0].myPlanes = new MyPlane[6];
                                        frustumCameras[0].myPlanes[0] = new MyPlane();
                                        frustumCameras[0].myPlanes[1] = new MyPlane();
                                        frustumCameras[0].myPlanes[2] = new MyPlane();
                                        frustumCameras[0].myPlanes[3] = new MyPlane();
                                        frustumCameras[0].myPlanes[4] = new MyPlane();
                                        frustumCameras[0].myPlanes[5] = new MyPlane();
                                    }

                                    frustumCamerasInit = true;
                                    break;
                                }
                            }
                        }
                    }
                }
                else if (altTreesMain.cameraModeFrustum != 2)
                {
                    lock (frustumCamerasLock)
                    {
                        frustumCameras = new FrustumCamera[altTreesMain.activeCameraFrustum.Length];
                        for (int i = 0; i < frustumCameras.Length; i++)
                        {
                            frustumCameras[i].cam = altTreesMain.activeCameraFrustum[i];
                            frustumCameras[i].planes = new Plane[6];
                            frustumCameras[i].myPlanes = new MyPlane[6];
                            frustumCameras[i].myPlanes[0] = new MyPlane();
                            frustumCameras[i].myPlanes[1] = new MyPlane();
                            frustumCameras[i].myPlanes[2] = new MyPlane();
                            frustumCameras[i].myPlanes[3] = new MyPlane();
                            frustumCameras[i].myPlanes[4] = new MyPlane();
                            frustumCameras[i].myPlanes[5] = new MyPlane();
                        }
                    }

                    frustumCamerasInit = true;
                }

                if (altTreesMain.cameraModeDistance == 0 || (altTreesMain.cameraModeDistance == 1 && (altTreesMain.activeCameraDistance == null || altTreesMain.activeCameraDistance.Length == 0)))
                {
                    if (Camera.main != null && Camera.main.gameObject.activeInHierarchy && Camera.main.enabled)
                    {
                        lock (distanceCamerasLock)
                        {
                            distanceCameras = new DistanceCamera[1];
                            distanceCameras[0].trans = Camera.main.transform;
                            distanceCameras[0].isSelected = false;
                            treesCameras = new AtiTemp[1];

                            distanceCamerasInit = true;
                        }
                    }
                    else
                    {
                        Camera[] cams = Camera.allCameras;
                        if (cams != null && cams.Length > 0)
                        {
                            for (int i = 0; i < cams.Length; i++)
                            {
                                if (cams[i] != null && cams[i].gameObject.activeInHierarchy && cams[i].enabled)
                                {
                                    lock (distanceCamerasLock)
                                    {
                                        distanceCameras = new DistanceCamera[1];
                                        distanceCameras[0].trans = cams[i].transform;
                                        distanceCameras[0].isSelected = false;
                                        treesCameras = new AtiTemp[1];

                                        distanceCamerasInit = true;
                                    }
                                    break;
                                }
                            }
                        }
                    }
                }
                else if(altTreesMain.cameraModeDistance != 2)
                {
                    lock (distanceCamerasLock)
                    {
                        distanceCameras = new DistanceCamera[altTreesMain.activeCameraDistance.Length];
                        treesCameras = new AtiTemp[altTreesMain.activeCameraDistance.Length];
                        for (int i = 0; i < distanceCameras.Length; i++)
                        {
                            distanceCameras[i].trans = altTreesMain.activeCameraDistance[i].transform;
                            distanceCameras[i].isSelected = false;
                        }

                        distanceCamerasInit = true;
                    }
                }
            }
            else
            {
                if (AltTreesManager.camEditor != null)
                {
                    lock (frustumCamerasLock)
                    {
                        frustumCameras = new FrustumCamera[1];
                        frustumCameras[0].cam = camEditor;
                        frustumCameraTransformEditor = camEditor.transform;
                        frustumCameras[0].planes = new Plane[6];
                        frustumCameras[0].myPlanes = new MyPlane[6];
                        frustumCameras[0].myPlanes[0] = new MyPlane();
                        frustumCameras[0].myPlanes[1] = new MyPlane();
                        frustumCameras[0].myPlanes[2] = new MyPlane();
                        frustumCameras[0].myPlanes[3] = new MyPlane();
                        frustumCameras[0].myPlanes[4] = new MyPlane();
                        frustumCameras[0].myPlanes[5] = new MyPlane();

                        frustumCamerasInit = true;
                    }

                    lock (distanceCamerasLock)
                    {
                        distanceCameras = new DistanceCamera[1];
                        distanceCameras[0].trans = camEditor.transform;
                        distanceCameras[0].isSelected = false;
                        treesCameras = new AtiTemp[1];

                        distanceCamerasInit = true;
                    }
                }
            }

            if (Application.isEditor)
            {
                goCubeDebug = GameObject.CreatePrimitive(PrimitiveType.Cube);
                DestroyImmediate(goCubeDebug.GetComponent<BoxCollider>());
                goCubeDebug.hideFlags = HideFlags.HideAndDontSave;
                goCubeDebug.transform.position = vectTemp;

                matTemp = new Material(goCubeDebug.GetComponent<MeshRenderer>().sharedMaterial);
                matTemp.shaderKeywords = goCubeDebug.GetComponent<MeshRenderer>().sharedMaterial.shaderKeywords;
                matTemp.hideFlags = HideFlags.DontSave;
                Shader sh = Shader.Find("Hidden/AltTrees/DebugCube");
                if (sh == null)
                {
                    matTemp.SetFloat("_Mode", 2);
                    matTemp.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    matTemp.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    matTemp.SetInt("_ZWrite", 0);
                    matTemp.DisableKeyword("_ALPHATEST_ON");
                    matTemp.EnableKeyword("_ALPHABLEND_ON");
                    matTemp.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                }
                else
                {
                    matTemp.shader = sh;
                }
                goCubeDebug.GetComponent<MeshRenderer>().sharedMaterial = matTemp;
                matTemp = null;
            }


            /*
            if (altTreesMain.isPlaying)
            {
                for (int j = 0; j < altTreesMain.altTreesManagerData.initBillboardCountPool; j++)
                {
                    objBillboardsPool.Add(getPlaneBillboard());
                }
            }*/

            isPlayingTimer = altTreesMain.isPlaying;

            lock (timerLock)
            {
                if (altTreesMain.altTreesManagerData.multiThreading)
                {
                    TimerCallback timeCB = new TimerCallback(checkerTimer);
                    timerCheck = new Timer(timeCB, null, 0, 100);
                }
                timerLockStop = false;
            }

            isInit = true;
        }

	    void Update()
        {
            if (isInit)
            {
                if (altTreesMain.isPlaying)
                    UpdateFunk();
            }
        }

        void LateUpdate()
        {
            if (isInit)
            {
                if (altTreesMain.isPlaying)
                    DrawMeshes();
            }
        }

        int idCheck = 0;

        int p1;
        int p2;
        float pp;
        bool starStatWind = false;
        int curLOD = 0;
        Quaternion quaternionIdentity = Quaternion.identity;

        AltTreesTrees[] attDrawBillboards;
        AltTreesTrees[] attDrawBillboardsCrossFade;
        int attDrawBillboardsCount = 0;
        int attDrawBillboardsCrossFadeCount = 0;
        #if !(UNITY_5_2 || UNITY_5_3 || UNITY_5_4)
            MaterialPropertyBlock mpbTemp;
        #endif

        Matrix4x4[] matrixDrawBillboards;
        float[] alphaDrawBillboards;
        float[] widthDrawBillboards;
        float[] heightDrawBillboards;
        float[] rotationDrawBillboards;
        Vector4[] hueDrawBillboards;

        Vector4[] hueLeaveDraw;
        Vector4[] hueBarkDraw;
        float[] indDraw;
        float[] smoothDraw;

        //float floatTemp = 0f;
        TreesToRender ttrTemp = null;

        bool gpuInstancingBoolTemp = false;
        bool enableFrustum;

        AltTreesQuad atqTemp = null;
        int atqIdTemp = -1;

        public bool TestPlanesAABB(ref Bounds bound)
        {
            lock (frustumCamerasLock)
            {
                for (int i = 0; i < frustumCameras.Length; i++)
                {
                    if (frustumCameras[i].cam.isActiveAndEnabled || !altTreesMain.isPlaying)
                        if (GeometryUtility.TestPlanesAABB(frustumCameras[i].planes, bound))
                            return true;
                }
            }

            return false;
        }

        public void DrawMeshes()
        {
            drawGroupBillboardsCount = 0;
            drawBillboardsCount = 0;
            drawMeshesCount = 0;
            drawBillboardsCrossFadeCount = 0;
            drawMeshesCrossFadeCount = 0;

            if (!altTreesMain.altTreesManagerData.hideGroupBillboards)
            {
                for (int i = 0; i < quadsToRender.Count; i++)
                {
                    for (int t = 0; t < quadsToRender[i].meshes.Count; t++)
                    {
                        Graphics.DrawMesh(quadsToRender[i].meshes[t].mesh, ((patches[quadsToRender[i].patchID].step - jump) * altTreesMain.altTreesManagerData.sizePatch) + jumpPos, Quaternion.identity, treesPoolArray[quadsToRender[i].meshes[t].materialId].materialBillboardGroup, 0, null, 0, null, altTreesMain.altTreesManagerData.shadowsGroupBillboards);
                        drawGroupBillboardsCount++;
                    }
                }
            }


            if (!frustumCamerasInit)
                return;

            lock (frustumCamerasLock)
            {
                for (int i = 0; i < frustumCameras.Length; i++)
                {
                    if (frustumCameras[i].cam.isActiveAndEnabled || !altTreesMain.isPlaying)
                    {
                        if (enabledExtractPlanes)
                            ExtractPlanes(frustumCameras[i].planes, frustumCameras[i].cam.projectionMatrix * frustumCameras[i].cam.worldToCameraMatrix);
                        else
                            frustumCameras[i].planes = GeometryUtility.CalculateFrustumPlanes(frustumCameras[i].cam);
                        frustumCameras[i].myPlanes[0].setPlane(ref frustumCameras[i].planes[0]);
                        frustumCameras[i].myPlanes[1].setPlane(ref frustumCameras[i].planes[1]);
                        frustumCameras[i].myPlanes[2].setPlane(ref frustumCameras[i].planes[2]);
                        frustumCameras[i].myPlanes[3].setPlane(ref frustumCameras[i].planes[3]);
                        frustumCameras[i].myPlanes[4].setPlane(ref frustumCameras[i].planes[4]);
                        frustumCameras[i].myPlanes[5].setPlane(ref frustumCameras[i].planes[5]);
                        frustumCameras[i].isActiveAndEnabled = true;
                    }
                    else
                        frustumCameras[i].isActiveAndEnabled = false;
                }
            }
            

            enableFrustum = altTreesMain.enableFrustum;

            if(enableFrustum)
                ThreadPool.QueueUserWorkItem(checkerFrustumCulling);

            
            if (gpuInstancingSupport)
            {
                for (int i = 0; i < treesPoolArray.Length; i++)
                {
                    attDrawBillboardsCrossFadeCount = 0;
                    attDrawBillboardsCrossFadeCount = 0;

                    gpuInstancingBoolTemp = treesPoolArray[i].tree.gpuInstancing;

                    /*if (treesPoolArray[i].tree.densityObjects != 0f && treesPoolArray[i].tree.isObject)
                        floatTemp = altTreesMain.altTreesManagerData.densityObjects * (3f - treesPoolArray[i].tree.densityObjects * 2f);
                    else
                        floatTemp = 101f;*/

                    for (int t = 0; t < treesPoolArray[i].treesToRenderCount; t++)
                    {
                        ttrTemp = treesPoolArray[i].treesToRender[t];

                        if (ttrTemp.noNull/* && ttrTemp.att.densityObjects <= floatTemp*/)
                        {
                            if (!altTreesMain.altTreesManagerData.hideBillboards)
                            {
                                if (ttrTemp.att.isBillboard)
                                {
                                    if ((!enableFrustum && TestPlanesAABB(ref ttrTemp.att.bound.bound)) || (enableFrustum && ttrTemp.att.inFrustum))
                                    {
                                        //if(gpuInstancingBoolTemp)
                                        {
                                            if (ttrTemp.att.isCrossFadeBillboard)
                                            {
                                                attDrawBillboardsCrossFade[attDrawBillboardsCrossFadeCount] = ttrTemp.att;
                                                attDrawBillboardsCrossFadeCount++;

                                                if (attDrawBillboardsCrossFadeCount == gpuInstancingCount)
                                                {
                                                    for (int m = 0; m < attDrawBillboardsCrossFadeCount; m++)
                                                    {
                                                        matrixDrawBillboards[m] = attDrawBillboardsCrossFade[m].matrixBillboard;
                                                        alphaDrawBillboards[m] = attDrawBillboardsCrossFade[m].alphaPropBlockBillboard;
                                                        widthDrawBillboards[m] = attDrawBillboardsCrossFade[m].widthPropBlock;
                                                        heightDrawBillboards[m] = attDrawBillboardsCrossFade[m].heightPropBlock;
                                                        rotationDrawBillboards[m] = attDrawBillboardsCrossFade[m].rotation;
                                                        hueDrawBillboards[m] = attDrawBillboardsCrossFade[m].huePropBlock;
                                                    }
                                                    #if !(UNITY_5_2 || UNITY_5_3 || UNITY_5_4)
                                                        mpbTemp.Clear();
                                                        mpbTemp.SetFloatArray(Alpha_PropertyID, alphaDrawBillboards);
                                                        mpbTemp.SetFloatArray(Width_PropertyID, widthDrawBillboards);
                                                        mpbTemp.SetFloatArray(Height_PropertyID, heightDrawBillboards);
                                                        mpbTemp.SetFloatArray(Rotation_PropertyID, rotationDrawBillboards);
                                                        mpbTemp.SetVectorArray(HueVariation_PropertyID, hueDrawBillboards);

                                                        Graphics.DrawMeshInstanced(treesPoolArray[i].mesh, 0, treesPoolArray[i].materialBillboardCrossFade, matrixDrawBillboards, attDrawBillboardsCrossFadeCount, mpbTemp, (altTreesMain.altTreesManagerData.shadowsBillboards ? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.Off));
                                                    #endif
                                                    attDrawBillboardsCrossFadeCount = 0;
                                                }

                                                drawBillboardsCrossFadeCount++;
                                            }
                                            else
                                            {

                                                attDrawBillboards[attDrawBillboardsCount] = ttrTemp.att;
                                                attDrawBillboardsCount++;

                                                if (attDrawBillboardsCount == gpuInstancingCount)
                                                {
                                                    for (int m = 0; m < attDrawBillboardsCount; m++)
                                                    {
                                                        matrixDrawBillboards[m] = attDrawBillboards[m].matrixBillboard;
                                                        alphaDrawBillboards[m] = attDrawBillboards[m].alphaPropBlockBillboard;
                                                        widthDrawBillboards[m] = attDrawBillboards[m].widthPropBlock;
                                                        heightDrawBillboards[m] = attDrawBillboards[m].heightPropBlock;
                                                        rotationDrawBillboards[m] = attDrawBillboards[m].rotation;
                                                        hueDrawBillboards[m] = attDrawBillboards[m].huePropBlock;
                                                    }
                                                    #if !(UNITY_5_2 || UNITY_5_3 || UNITY_5_4)
                                                        mpbTemp.Clear();
                                                        mpbTemp.SetFloatArray(Alpha_PropertyID, alphaDrawBillboards);
                                                        mpbTemp.SetFloatArray(Width_PropertyID, widthDrawBillboards);
                                                        mpbTemp.SetFloatArray(Height_PropertyID, heightDrawBillboards);
                                                        mpbTemp.SetFloatArray(Rotation_PropertyID, rotationDrawBillboards);
                                                        mpbTemp.SetVectorArray(HueVariation_PropertyID, hueDrawBillboards);

                                                        Graphics.DrawMeshInstanced(treesPoolArray[i].mesh, 0, treesPoolArray[i].materialBillboard, matrixDrawBillboards, attDrawBillboardsCount, mpbTemp, (altTreesMain.altTreesManagerData.shadowsBillboards ? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.Off));
                                                    #endif
                                                    attDrawBillboardsCount = 0;
                                                }
                                            }
                                        }
                                        /*else
                                        {
                                            if (ttrTemp.att.isCrossFadeBillboard)
                                            {
                                                Graphics.DrawMesh(treesPoolArray[i].mesh, ttrTemp.att.posWorldBillboard, quaternionIdentity, treesPoolArray[i].materialBillboardCrossFade, 0, null, 0, ttrTemp.att.propBlockBillboards, altTreesMain.altTreesManagerData.shadowsBillboards);
                                                drawBillboardsCrossFadeCount++;
                                            }
                                            else
                                                Graphics.DrawMesh(treesPoolArray[i].mesh, ttrTemp.att.posWorldBillboard, quaternionIdentity, treesPoolArray[i].materialBillboard, 0, null, 0, ttrTemp.att.propBlockBillboards, altTreesMain.altTreesManagerData.shadowsBillboards);

                                        }*/
                                        drawBillboardsCount++;
                                    }
                                }
                            }

                            if (!altTreesMain.altTreesManagerData.hideMeshes)
                            {
                                if (ttrTemp.att.isMesh)
                                {
                                    if ((!enableFrustum && TestPlanesAABB(ref ttrTemp.att.bound.bound)) || (enableFrustum && ttrTemp.att.inFrustum))
                                    {
                                        if (ttrTemp.att.currentCrossFadeId == 1)
                                            curLOD = ttrTemp.att.currentLOD;
                                        else if (ttrTemp.att.currentCrossFadeId == 2)
                                            curLOD = ttrTemp.att.currentCrossFadeLOD;
                                        else if (ttrTemp.att.currentCrossFadeId == 3)
                                            curLOD = ttrTemp.att.currentCrossFadeLOD;
                                        else if (ttrTemp.att.currentCrossFadeId == 4)
                                            curLOD = ttrTemp.att.currentLOD;
                                        else if (ttrTemp.att.currentCrossFadeId == 5)
                                            curLOD = ttrTemp.att.currentCrossFadeLOD;
                                        else if (ttrTemp.att.currentCrossFadeId == 6)
                                            curLOD = ttrTemp.att.currentLOD;
                                        else if (ttrTemp.att.currentCrossFadeId == -1)
                                            curLOD = ttrTemp.att.currentLOD;
                                        else
                                            curLOD = ttrTemp.att.currentLOD;

                                        if(gpuInstancingBoolTemp)
                                        {
                                            if (ttrTemp.att.isCrossFadeMesh)
                                            {
                                                treesPoolArray[i].objsArray[curLOD].attDrawCrossFade[treesPoolArray[i].objsArray[curLOD].attDrawCrossFadeCount] = ttrTemp.att;
                                                treesPoolArray[i].objsArray[curLOD].attDrawCrossFadeCount++;

                                                drawMeshesCrossFadeCount++;

                                                if (treesPoolArray[i].objsArray[curLOD].attDrawCrossFadeCount == gpuInstancingCount)
                                                {
                                                    for (int m = 0; m < treesPoolArray[i].objsArray[curLOD].attDrawCrossFadeCount; m++)
                                                    {
                                                        matrixDrawBillboards[m] = treesPoolArray[i].objsArray[curLOD].attDrawCrossFade[m].matrixMesh;
                                                        alphaDrawBillboards[m] = treesPoolArray[i].objsArray[curLOD].attDrawCrossFade[m].alphaPropBlockMesh;
                                                        indDraw[m] = treesPoolArray[i].objsArray[curLOD].attDrawCrossFade[m].indPropBlockMesh;
                                                        smoothDraw[m] = treesPoolArray[i].objsArray[curLOD].attDrawCrossFade[m].smoothPropBlock;
                                                        hueLeaveDraw[m] = treesPoolArray[i].objsArray[curLOD].attDrawCrossFade[m].color;
                                                        hueBarkDraw[m] = treesPoolArray[i].objsArray[curLOD].attDrawCrossFade[m].colorBark;
                                                    }
                                                    #if !(UNITY_5_2 || UNITY_5_3 || UNITY_5_4)
                                                        mpbTemp.Clear();
                                                        mpbTemp.SetFloatArray(Alpha_PropertyID, alphaDrawBillboards);
                                                        mpbTemp.SetFloatArray(Ind_PropertyID, indDraw);
                                                        mpbTemp.SetFloatArray(smoothValue_PropertyID, smoothDraw);
                                                        mpbTemp.SetVectorArray(HueVariationLeave_PropertyID, hueLeaveDraw);
                                                        mpbTemp.SetVectorArray(HueVariationBark_PropertyID, hueBarkDraw);
                                                    
                                                        for (int p = 0; p < treesPoolArray[i].objsArray[curLOD].materialsMesh.Length; p++)
                                                        {
                                                            Graphics.DrawMeshInstanced(treesPoolArray[i].objsArray[curLOD].mesh, p, treesPoolArray[i].objsArray[curLOD].materialsMeshCrossFade[p], matrixDrawBillboards, treesPoolArray[i].objsArray[curLOD].attDrawCrossFadeCount, mpbTemp, (altTreesMain.altTreesManagerData.shadowsMeshes ? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.Off));
                                                        }
                                                    #endif
                                                    treesPoolArray[i].objsArray[curLOD].attDrawCrossFadeCount = 0;
                                                }
                                            }
                                            else
                                            {
                                                treesPoolArray[i].objsArray[curLOD].attDraw[treesPoolArray[i].objsArray[curLOD].attDrawCount] = ttrTemp.att;
                                                treesPoolArray[i].objsArray[curLOD].attDrawCount++;

                                                if (treesPoolArray[i].objsArray[curLOD].attDrawCount == gpuInstancingCount)
                                                {
                                                    for (int m = 0; m < treesPoolArray[i].objsArray[curLOD].attDrawCount; m++)
                                                    {
                                                        matrixDrawBillboards[m] = treesPoolArray[i].objsArray[curLOD].attDraw[m].matrixMesh;
                                                        alphaDrawBillboards[m] = treesPoolArray[i].objsArray[curLOD].attDraw[m].alphaPropBlockMesh;
                                                        indDraw[m] = treesPoolArray[i].objsArray[curLOD].attDraw[m].indPropBlockMesh;
                                                        smoothDraw[m] = treesPoolArray[i].objsArray[curLOD].attDraw[m].smoothPropBlock;
                                                        hueLeaveDraw[m] = treesPoolArray[i].objsArray[curLOD].attDraw[m].color;
                                                        hueBarkDraw[m] = treesPoolArray[i].objsArray[curLOD].attDraw[m].colorBark;
                                                    }
                                                    #if !(UNITY_5_2 || UNITY_5_3 || UNITY_5_4)
                                                        mpbTemp.Clear();
                                                        mpbTemp.SetFloatArray(Alpha_PropertyID, alphaDrawBillboards);
                                                        mpbTemp.SetFloatArray(Ind_PropertyID, indDraw);
                                                        mpbTemp.SetFloatArray(smoothValue_PropertyID, smoothDraw);
                                                        mpbTemp.SetVectorArray(HueVariationLeave_PropertyID, hueLeaveDraw);
                                                        mpbTemp.SetVectorArray(HueVariationBark_PropertyID, hueBarkDraw);
                                                    
                                                        for (int p = 0; p < treesPoolArray[i].objsArray[curLOD].materialsMesh.Length; p++)
                                                        {
                                                            Graphics.DrawMeshInstanced(treesPoolArray[i].objsArray[curLOD].mesh, p, treesPoolArray[i].objsArray[curLOD].materialsMesh[p], matrixDrawBillboards, treesPoolArray[i].objsArray[curLOD].attDrawCount, mpbTemp, (altTreesMain.altTreesManagerData.shadowsMeshes ? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.Off));
                                                        }
                                                    #endif
                                                    treesPoolArray[i].objsArray[curLOD].attDrawCount = 0;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (ttrTemp.att.isCrossFadeMesh)
                                            {
                                                for (int p = 0; p < treesPoolArray[i].objsArray[curLOD].materialsMesh.Length; p++)
                                                {
                                                    Graphics.DrawMesh(treesPoolArray[i].objsArray[curLOD].mesh, ttrTemp.att.matrixMesh, treesPoolArray[i].objsArray[curLOD].materialsMeshCrossFade[p], 0, null, p, ttrTemp.att.propBlockMesh, altTreesMain.altTreesManagerData.shadowsMeshes);
                                                }
                                                drawMeshesCrossFadeCount++;
                                            }
                                            else
                                            {
                                                for (int p = 0; p < treesPoolArray[i].objsArray[curLOD].materialsMesh.Length; p++)
                                                {
                                                    Graphics.DrawMesh(treesPoolArray[i].objsArray[curLOD].mesh, ttrTemp.att.matrixMesh, treesPoolArray[i].objsArray[curLOD].materialsMesh[p], 0, null, p, ttrTemp.att.propBlockMesh, altTreesMain.altTreesManagerData.shadowsMeshes);
                                                }
                                            }
                                        }
                                        drawMeshesCount++;
                                    }
                                }
                            }

                        }
                    }

                    if (!altTreesMain.altTreesManagerData.hideBillboards)
                    {
                        if (attDrawBillboardsCrossFadeCount != 0)
                        {
                            for (int m = 0; m < attDrawBillboardsCrossFadeCount; m++)
                            {
                                matrixDrawBillboards[m] = attDrawBillboardsCrossFade[m].matrixBillboard;
                                alphaDrawBillboards[m] = attDrawBillboardsCrossFade[m].alphaPropBlockBillboard;
                                widthDrawBillboards[m] = attDrawBillboardsCrossFade[m].widthPropBlock;
                                heightDrawBillboards[m] = attDrawBillboardsCrossFade[m].heightPropBlock;
                                rotationDrawBillboards[m] = attDrawBillboardsCrossFade[m].rotation;
                                hueDrawBillboards[m] = attDrawBillboardsCrossFade[m].huePropBlock;
                            }
                            #if !(UNITY_5_2 || UNITY_5_3 || UNITY_5_4)
                                mpbTemp.Clear();
                                mpbTemp.SetFloatArray(Alpha_PropertyID, alphaDrawBillboards);
                                mpbTemp.SetFloatArray(Width_PropertyID, widthDrawBillboards);
                                mpbTemp.SetFloatArray(Height_PropertyID, heightDrawBillboards);
                                mpbTemp.SetFloatArray(Rotation_PropertyID, rotationDrawBillboards);
                                mpbTemp.SetVectorArray(HueVariation_PropertyID, hueDrawBillboards);

                                Graphics.DrawMeshInstanced(treesPoolArray[i].mesh, 0, treesPoolArray[i].materialBillboardCrossFade, matrixDrawBillboards, attDrawBillboardsCrossFadeCount, mpbTemp, (altTreesMain.altTreesManagerData.shadowsBillboards ? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.Off));
                            #endif
                            attDrawBillboardsCrossFadeCount = 0;
                        }
                        if (attDrawBillboardsCount != 0)
                        {
                            for (int m = 0; m < attDrawBillboardsCount; m++)
                            {
                                matrixDrawBillboards[m] = attDrawBillboards[m].matrixBillboard;
                                alphaDrawBillboards[m] = attDrawBillboards[m].alphaPropBlockBillboard;
                                widthDrawBillboards[m] = attDrawBillboards[m].widthPropBlock;
                                heightDrawBillboards[m] = attDrawBillboards[m].heightPropBlock;
                                rotationDrawBillboards[m] = attDrawBillboards[m].rotation;
                                hueDrawBillboards[m] = attDrawBillboards[m].huePropBlock;
                            }
                            #if !(UNITY_5_2 || UNITY_5_3 || UNITY_5_4)
                                mpbTemp.Clear();
                                mpbTemp.SetFloatArray(Alpha_PropertyID, alphaDrawBillboards);
                                mpbTemp.SetFloatArray(Width_PropertyID, widthDrawBillboards);
                                mpbTemp.SetFloatArray(Height_PropertyID, heightDrawBillboards);
                                mpbTemp.SetFloatArray(Rotation_PropertyID, rotationDrawBillboards);
                                mpbTemp.SetVectorArray(HueVariation_PropertyID, hueDrawBillboards);

                                Graphics.DrawMeshInstanced(treesPoolArray[i].mesh, 0, treesPoolArray[i].materialBillboard, matrixDrawBillboards, attDrawBillboardsCount, mpbTemp, (altTreesMain.altTreesManagerData.shadowsBillboards ? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.Off));
                            #endif
                            attDrawBillboardsCount = 0;
                        }
                    }

                    if (!altTreesMain.altTreesManagerData.hideMeshes)
                    {
                        for (int t = 0; t < treesPoolArray[i].objsArray.Length; t++)
                        {
                            if (treesPoolArray[i].objsArray[t].attDrawCrossFadeCount != 0)
                            {
								for (int m = 0; m < treesPoolArray[i].objsArray[t].attDrawCrossFadeCount; m++)
                                {
                                    matrixDrawBillboards[m] = treesPoolArray[i].objsArray[t].attDrawCrossFade[m].matrixMesh;
                                    alphaDrawBillboards[m] = treesPoolArray[i].objsArray[t].attDrawCrossFade[m].alphaPropBlockMesh;
                                    indDraw[m] = treesPoolArray[i].objsArray[t].attDrawCrossFade[m].indPropBlockMesh;
                                    smoothDraw[m] = treesPoolArray[i].objsArray[t].attDrawCrossFade[m].smoothPropBlock;
                                    hueLeaveDraw[m] = treesPoolArray[i].objsArray[t].attDrawCrossFade[m].color;
                                    hueBarkDraw[m] = treesPoolArray[i].objsArray[t].attDrawCrossFade[m].colorBark;
                                }
                                #if !(UNITY_5_2 || UNITY_5_3 || UNITY_5_4)
                                    mpbTemp.Clear();
                                    mpbTemp.SetFloatArray(Alpha_PropertyID, alphaDrawBillboards);
                                    mpbTemp.SetFloatArray(Ind_PropertyID, indDraw);
                                    mpbTemp.SetFloatArray(smoothValue_PropertyID, smoothDraw);
                                    mpbTemp.SetVectorArray(HueVariationLeave_PropertyID, hueLeaveDraw);
                                    mpbTemp.SetVectorArray(HueVariationBark_PropertyID, hueBarkDraw);
                                
                                    for (int p = 0; p < treesPoolArray[i].objsArray[t].materialsMesh.Length; p++)
                                    {
                                        Graphics.DrawMeshInstanced(treesPoolArray[i].objsArray[t].mesh, p, treesPoolArray[i].objsArray[t].materialsMeshCrossFade[p], matrixDrawBillboards, treesPoolArray[i].objsArray[t].attDrawCrossFadeCount, mpbTemp, (altTreesMain.altTreesManagerData.shadowsMeshes ? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.Off));
                                    }
                                #endif
                                treesPoolArray[i].objsArray[t].attDrawCrossFadeCount = 0;
                            }
                            if (treesPoolArray[i].objsArray[t].attDrawCount != 0)
                            {
                                for (int m = 0; m < treesPoolArray[i].objsArray[t].attDrawCount; m++)
                                {
                                    matrixDrawBillboards[m] = treesPoolArray[i].objsArray[t].attDraw[m].matrixMesh;
                                    alphaDrawBillboards[m] = treesPoolArray[i].objsArray[t].attDraw[m].alphaPropBlockMesh;
                                    indDraw[m] = treesPoolArray[i].objsArray[t].attDraw[m].indPropBlockMesh;
                                    smoothDraw[m] = treesPoolArray[i].objsArray[t].attDraw[m].smoothPropBlock;
                                    hueLeaveDraw[m] = treesPoolArray[i].objsArray[t].attDraw[m].color;
                                    hueBarkDraw[m] = treesPoolArray[i].objsArray[t].attDraw[m].colorBark;
                                }
                                #if !(UNITY_5_2 || UNITY_5_3 || UNITY_5_4)
                                    mpbTemp.Clear();
                                    mpbTemp.SetFloatArray(Alpha_PropertyID, alphaDrawBillboards);
                                    mpbTemp.SetFloatArray(Ind_PropertyID, indDraw);
                                    mpbTemp.SetFloatArray(smoothValue_PropertyID, smoothDraw);
                                    mpbTemp.SetVectorArray(HueVariationLeave_PropertyID, hueLeaveDraw);
                                    mpbTemp.SetVectorArray(HueVariationBark_PropertyID, hueBarkDraw);
                                
                                    for (int p = 0; p < treesPoolArray[i].objsArray[t].materialsMesh.Length; p++)
                                    {
                                        Graphics.DrawMeshInstanced(treesPoolArray[i].objsArray[t].mesh, p, treesPoolArray[i].objsArray[t].materialsMesh[p], matrixDrawBillboards, treesPoolArray[i].objsArray[t].attDrawCount, mpbTemp, (altTreesMain.altTreesManagerData.shadowsMeshes ? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.Off));
                                    }
                                #endif
                                treesPoolArray[i].objsArray[t].attDrawCount = 0;
                            }
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < treesPoolArray.Length; i++)
                {
                    /*if (treesPoolArray[i].tree.densityObjects != 0f && treesPoolArray[i].tree.isObject)
                        floatTemp = altTreesMain.altTreesManagerData.densityObjects * (3f - treesPoolArray[i].tree.densityObjects * 2f);
                    else
                        floatTemp = 101f;*/

                    for (int t = 0; t < treesPoolArray[i].treesToRenderCount; t++)
                    {
                        ttrTemp = treesPoolArray[i].treesToRender[t];

                        if (ttrTemp.noNull/* && ttrTemp.att.densityObjects <= floatTemp*/)
                        {
                            if (!altTreesMain.altTreesManagerData.hideBillboards)
                            {
                                if (ttrTemp.att.isBillboard)
                                {
                                    if ((!enableFrustum && TestPlanesAABB(ref ttrTemp.att.bound.bound)) || (enableFrustum && ttrTemp.att.inFrustum))
                                    {
                                        if (ttrTemp.att.isCrossFadeBillboard)
                                        {
                                            Graphics.DrawMesh(treesPoolArray[i].mesh, ttrTemp.att.posWorldBillboard, quaternionIdentity, treesPoolArray[i].materialBillboardCrossFade, 0, null, 0, ttrTemp.att.propBlockBillboards, altTreesMain.altTreesManagerData.shadowsBillboards);
                                            drawBillboardsCrossFadeCount++;
                                        }
                                        else
                                            Graphics.DrawMesh(treesPoolArray[i].mesh, ttrTemp.att.posWorldBillboard, quaternionIdentity, treesPoolArray[i].materialBillboard, 0, null, 0, ttrTemp.att.propBlockBillboards, altTreesMain.altTreesManagerData.shadowsBillboards);

                                        drawBillboardsCount++;
                                    }
                                }
                            }


                            if (!altTreesMain.altTreesManagerData.hideMeshes)
                            {
                                if (ttrTemp.att.isMesh)
                                {
                                    if ((!enableFrustum && TestPlanesAABB(ref ttrTemp.att.bound.bound)) || (enableFrustum && ttrTemp.att.inFrustum))
                                    {
                                        if (ttrTemp.att.currentCrossFadeId == 1)
                                            curLOD = ttrTemp.att.currentLOD;
                                        else if (ttrTemp.att.currentCrossFadeId == 2)
                                            curLOD = ttrTemp.att.currentCrossFadeLOD;
                                        else if (ttrTemp.att.currentCrossFadeId == 3)
                                            curLOD = ttrTemp.att.currentCrossFadeLOD;
                                        else if (ttrTemp.att.currentCrossFadeId == 4)
                                            curLOD = ttrTemp.att.currentLOD;
                                        else if (ttrTemp.att.currentCrossFadeId == 5)
                                            curLOD = ttrTemp.att.currentCrossFadeLOD;
                                        else if (ttrTemp.att.currentCrossFadeId == 6)
                                            curLOD = ttrTemp.att.currentLOD;
                                        else if (ttrTemp.att.currentCrossFadeId == -1)
                                            curLOD = ttrTemp.att.currentLOD;
                                        else
                                            curLOD = ttrTemp.att.currentLOD;


                                        if (ttrTemp.att.isCrossFadeMesh)
                                        {
                                            for (int p = 0; p < treesPoolArray[i].objsArray[curLOD].materialsMesh.Length; p++)
                                            {
                                                Graphics.DrawMesh(treesPoolArray[i].objsArray[curLOD].mesh, ttrTemp.att.matrixMesh, treesPoolArray[i].objsArray[curLOD].materialsMeshCrossFade[p], 0, null, p, ttrTemp.att.propBlockMesh, altTreesMain.altTreesManagerData.shadowsMeshes);
                                            }
                                            drawMeshesCrossFadeCount++;
                                        }
                                        else
                                        {
                                            for (int p = 0; p < treesPoolArray[i].objsArray[curLOD].materialsMesh.Length; p++)
                                            {
                                                Graphics.DrawMesh(treesPoolArray[i].objsArray[curLOD].mesh, ttrTemp.att.matrixMesh, treesPoolArray[i].objsArray[curLOD].materialsMesh[p], 0, null, p, ttrTemp.att.propBlockMesh, altTreesMain.altTreesManagerData.shadowsMeshes);
                                            }
                                        }
                                        drawMeshesCount++;
                                    }
                                }
                            }

                        }
                    }
                }
            }
            ttrTemp = null;
        }



        public void UpdateFunk()
        {
            //Debug.Log("UpdateFunk");
            if (isDestroyed)
            {
                if (this.gameObject!=null)
                    DestroyImmediate(this.gameObject);
                return;
            }

            lock (distanceCamerasLock)
            {
                if (!distanceCamerasInit || distanceCameras.Length == 0)
                {
                    if (altTreesMain.isPlaying)
                    {
                        if (altTreesMain.cameraModeDistance == 0 || (altTreesMain.cameraModeDistance == 1 && altTreesMain.activeCameraDistance == null))
                        {
                            if (Camera.main != null && Camera.main.gameObject.activeInHierarchy && Camera.main.enabled)
                            {
                                distanceCameras = new DistanceCamera[1];
                                distanceCameras[0].trans = Camera.main.transform;
                                distanceCameras[0].isSelected = false;
                                treesCameras = new AtiTemp[1];
                            }
                            else
                            {
                                Camera[] cams = Camera.allCameras;
                                if (cams != null && cams.Length > 0)
                                {
                                    for (int i = 0; i < cams.Length; i++)
                                    {
                                        if (cams[i] != null && cams[i].gameObject.activeInHierarchy && cams[i].enabled)
                                        {
                                            distanceCameras = new DistanceCamera[1];
                                            distanceCameras[0].trans = cams[i].transform;
                                            distanceCameras[0].isSelected = false;
                                            treesCameras = new AtiTemp[1];
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                        else if(altTreesMain.cameraModeDistance != 2)
                        {
                            distanceCameras = new DistanceCamera[altTreesMain.activeCameraDistance.Length];
                            treesCameras = new AtiTemp[altTreesMain.activeCameraDistance.Length];
                            for (int i = 0; i < distanceCameras.Length; i++)
                            {
                                distanceCameras[i].trans = altTreesMain.activeCameraDistance[i].transform;
                                distanceCameras[i].isSelected = false;

                            }
                        }
                    }
                    else
                    {
                        if (AltTreesManager.camEditor != null)
                        {
                            distanceCameras = new DistanceCamera[1];
                            distanceCameras[0].trans = camEditor.transform;
                            distanceCameras[0].isSelected = false;
                            treesCameras = new AtiTemp[1];
                        }
                    }
                }
                else
                {
                    for (int c = distanceCameras.Length - 1; c >= 0; c--)
                    {
                        if (distanceCameras[c].trans == null)
                        {
                            altTreesMain.LogError("distanceCameras[c].trans == null");
                            removeDistanceCamera(c);
                        }
                    }
                }
            }


            //if (!distanceCamerasInit)
            //    return;

            lock (frustumCamerasLock)
            {
                if (!frustumCamerasInit || frustumCameras.Length == 0)
                {
                    if (altTreesMain.isPlaying)
                    {
                        if (altTreesMain.cameraModeFrustum == 0 || (altTreesMain.cameraModeFrustum == 1 && altTreesMain.activeCameraFrustum == null))
                        {
                            if (Camera.main != null && Camera.main.gameObject.activeInHierarchy && Camera.main.enabled)
                            {
                                frustumCameras = new FrustumCamera[1];
                                frustumCameras[0].cam = Camera.main;
                                frustumCameras[0].planes = new Plane[6];
                                frustumCameras[0].myPlanes = new MyPlane[6];
                                frustumCameras[0].myPlanes[0] = new MyPlane();
                                frustumCameras[0].myPlanes[1] = new MyPlane();
                                frustumCameras[0].myPlanes[2] = new MyPlane();
                                frustumCameras[0].myPlanes[3] = new MyPlane();
                                frustumCameras[0].myPlanes[4] = new MyPlane();
                                frustumCameras[0].myPlanes[5] = new MyPlane();

                                frustumCamerasInit = true;
                            }
                            else
                            {
                                Camera[] cams = Camera.allCameras;
                                if (cams != null && cams.Length > 0)
                                {
                                    for (int i = 0; i < cams.Length; i++)
                                    {
                                        if (cams[i] != null && cams[i].gameObject.activeInHierarchy && cams[i].enabled)
                                        {
                                            frustumCameras = new FrustumCamera[1];
                                            frustumCameras[0].cam = cams[i];
                                            frustumCameras[0].planes = new Plane[6];
                                            frustumCameras[0].myPlanes = new MyPlane[6];
                                            frustumCameras[0].myPlanes[0] = new MyPlane();
                                            frustumCameras[0].myPlanes[1] = new MyPlane();
                                            frustumCameras[0].myPlanes[2] = new MyPlane();
                                            frustumCameras[0].myPlanes[3] = new MyPlane();
                                            frustumCameras[0].myPlanes[4] = new MyPlane();
                                            frustumCameras[0].myPlanes[5] = new MyPlane();

                                            frustumCamerasInit = true;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                        else if (altTreesMain.cameraModeFrustum != 2)
                        {
                            frustumCameras = new FrustumCamera[altTreesMain.activeCameraFrustum.Length];
                            for (int i = 0; i < frustumCameras.Length; i++)
                            {
                                frustumCameras[i].cam = altTreesMain.activeCameraFrustum[i];
                                frustumCameras[i].planes = new Plane[6];
                                frustumCameras[i].myPlanes = new MyPlane[6];
                                frustumCameras[i].myPlanes[0] = new MyPlane();
                                frustumCameras[i].myPlanes[1] = new MyPlane();
                                frustumCameras[i].myPlanes[2] = new MyPlane();
                                frustumCameras[i].myPlanes[3] = new MyPlane();
                                frustumCameras[i].myPlanes[4] = new MyPlane();
                                frustumCameras[i].myPlanes[5] = new MyPlane();
                            }

                            frustumCamerasInit = true;
                        }
                    }
                    else
                    {
                        if (AltTreesManager.camEditor != null)
                        {
                            frustumCameras = new FrustumCamera[1];
                            frustumCameras[0].cam = camEditor;
                            frustumCameraTransformEditor = camEditor.transform;
                            frustumCameras[0].planes = new Plane[6];
                            frustumCameras[0].myPlanes = new MyPlane[6];
                            frustumCameras[0].myPlanes[0] = new MyPlane();
                            frustumCameras[0].myPlanes[1] = new MyPlane();
                            frustumCameras[0].myPlanes[2] = new MyPlane();
                            frustumCameras[0].myPlanes[3] = new MyPlane();
                            frustumCameras[0].myPlanes[4] = new MyPlane();
                            frustumCameras[0].myPlanes[5] = new MyPlane();

                            frustumCamerasInit = true;
                        }
                    }
                }
                else
                {
                    for (int c = frustumCameras.Length - 1; c >= 0; c--)
                    {
                        if (frustumCameras[c].cam == null)
                        {
                            altTreesMain.LogError("frustumCameras[c].cam == null");
                            removeFrustumCullingCamera(c);
                        }
                    }
                }
            }

            if (isSelectionTree)
            {
                lock (distanceCamerasLock)
                {
                    if (distanceCamerasInit)
                    {
                        for (int i = 0; i < distanceCameras.Length; i++)
                        {
                            if (distanceCameras[i].isSelected)
                            {
                                altTreeInstanceTemp = distanceCameras[i].ati;
                                if (altTreeInstanceTemp != null)
                                {
                                    distanceCameras[i].trans.localRotation = Quaternion.Euler(0f, distanceCameras[i].trans.localRotation.eulerAngles.y, 0f);

                                    if (!distanceCameras[i].trans.localScale.Equals(altTreeInstanceTemp.scaleTempStar))
                                    {
                                        altTreeInstanceTemp.scaleTempStar.y = distanceCameras[i].trans.localScale.y;

                                        if (distanceCameras[i].trans.localScale.x != altTreeInstanceTemp.scaleTempStar.x)
                                        {
                                            altTreeInstanceTemp.scaleTempStar.x = distanceCameras[i].trans.localScale.x;
                                            altTreeInstanceTemp.scaleTempStar.z = distanceCameras[i].trans.localScale.x;
                                        }
                                        else
                                        {
                                            altTreeInstanceTemp.scaleTempStar.x = distanceCameras[i].trans.localScale.z;
                                            altTreeInstanceTemp.scaleTempStar.z = distanceCameras[i].trans.localScale.z;
                                        }

                                        distanceCameras[i].trans.localScale = altTreeInstanceTemp.scaleTempStar;
                                    }

                                    if (!altTreeInstanceTemp.hueLeave.Equals(altTreeInstanceTemp.hueLeaveStar) || !altTreeInstanceTemp.hueBark.Equals(altTreeInstanceTemp.hueBarkStar))
                                    {
                                        if (!altTreeInstanceTemp.isObject)
                                        {
                                            propBlock.Clear();
                                            propBlock.SetColor(HueVariationLeave_PropertyID, altTreeInstanceTemp.hueLeave);
                                            propBlock.SetColor(HueVariationBark_PropertyID, altTreeInstanceTemp.hueBark);
                                            patches[altTreeInstanceTemp.patchID].trees[altTreeInstanceTemp.idTree].alphaPropBlockMesh = Mathf.Clamp(tempFloatNext / altTreesMain.altTreesManagerData.getCrossFadeTimeBillboard(), 0f, 1.0f);
                                            patches[altTreeInstanceTemp.patchID].trees[altTreeInstanceTemp.idTree].indPropBlockMesh = 0;
                                            patches[altTreeInstanceTemp.patchID].trees[altTreeInstanceTemp.idTree].smoothPropBlock = 0;
                                            propBlock.SetFloat(Alpha_PropertyID, patches[altTreeInstanceTemp.patchID].trees[altTreeInstanceTemp.idTree].alphaPropBlockMesh);
                                            propBlock.SetFloat(Ind_PropertyID, 0.0f);
                                            propBlock.SetFloat(smoothValue_PropertyID, 0.0f);
                                            distanceCameras[i].trans.GetComponent<MeshRenderer>().SetPropertyBlock(propBlock);

                                            altTreeInstanceTemp.hueLeaveStar = altTreeInstanceTemp.hueLeave;
                                            altTreeInstanceTemp.hueBarkStar = altTreeInstanceTemp.hueBark;
                                        }
                                        else
                                        {
                                            propBlock.Clear();
                                            propBlock.SetColor(HueVariationLeave_PropertyID, altTreeInstanceTemp.hueLeave);
                                            propBlock.SetColor(HueVariationBark_PropertyID, altTreeInstanceTemp.hueBark);
                                            patches[altTreeInstanceTemp.patchID].quadObjects[altTreeInstanceTemp.quadID].findObjectById(altTreeInstanceTemp.idTree).alphaPropBlockMesh = Mathf.Clamp(tempFloatNext / altTreesMain.altTreesManagerData.getCrossFadeTimeBillboard(), 0f, 1.0f);
                                            patches[altTreeInstanceTemp.patchID].quadObjects[altTreeInstanceTemp.quadID].findObjectById(altTreeInstanceTemp.idTree).indPropBlockMesh = 0;
                                            patches[altTreeInstanceTemp.patchID].quadObjects[altTreeInstanceTemp.quadID].findObjectById(altTreeInstanceTemp.idTree).smoothPropBlock = 0;
                                            propBlock.SetFloat(Alpha_PropertyID, patches[altTreeInstanceTemp.patchID].quadObjects[altTreeInstanceTemp.quadID].findObjectById(altTreeInstanceTemp.idTree).alphaPropBlockMesh);
                                            propBlock.SetFloat(Ind_PropertyID, 0.0f);
                                            propBlock.SetFloat(smoothValue_PropertyID, 0.0f);
                                            distanceCameras[i].trans.GetComponent<MeshRenderer>().SetPropertyBlock(propBlock);

                                            altTreeInstanceTemp.hueLeaveStar = altTreeInstanceTemp.hueLeave;
                                            altTreeInstanceTemp.hueBarkStar = altTreeInstanceTemp.hueBark;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                altTreeInstanceTemp = null;
            }

            /*if (rendersToRemove.Count != 0)
            {
                rendersToRemoveCounter++;
                if (rendersToRemoveCounter == 2 || altTreesMain.isPlaying)
                {
                    for (int i = 0; i < rendersToRemove.Count; i++)
                    {
                        if (altTreesMain.isPlaying)
                            Destroy(rendersToRemove[i]);
                        else
                            DestroyImmediate(rendersToRemove[i]);
                    }
                    rendersToRemove.Clear();
                    rendersToRemoveCounter = 0;
                }
            }*/
            
            _speedWind = AltWind.getSpeed();
            _directionWind = AltWind.getDirection();
            

            if (_speedWind >= 0.01f)
            {
                if (!starStatWind)
                {
                    for (int i = 0; i < treePrototypeIds.Length; i++)
                    {
                        if (treesPoolArray[i].tree.windMode != 0 && ((treesPoolArray[i].tree.windMode == 2 && treesPoolArray[i].tree.windIntensity_TC != 0f) || (treesPoolArray[i].tree.windMode == 1 && treesPoolArray[i].tree.loadedConfig && treesPoolArray[i].tree.windIntensity_ST != 0f)))
                        {
                            for (int j = 0; j < treesPoolArray[i].objsArray.Length; j++)
                            {
                                for (int k = 0; k < treesPoolArray[i].objsArray[j].materialsMesh.Length; k++)
                                {
                                    treesPoolArray[i].objsArray[j].materialsMesh[k].EnableKeyword("ENABLE_ALTWIND");
                                }
                                for (int k = 0; k < treesPoolArray[i].objsArray[j].materialsMeshCrossFade.Length; k++)
                                {
                                    treesPoolArray[i].objsArray[j].materialsMeshCrossFade[k].EnableKeyword("ENABLE_ALTWIND");
                                }
                            }
                        }
                    }

                    starStatWind = true;
                }
                
                for (int i = 0; i < treePrototypeIds.Length; i++)
                {
                    if (!treesPoolArray[i].tree.windStar && treesPoolArray[i].tree.windMode != 0 && ((treesPoolArray[i].tree.windMode == 2 && treesPoolArray[i].tree.windIntensity_TC != 0f) || (treesPoolArray[i].tree.windMode == 1 && treesPoolArray[i].tree.loadedConfig && treesPoolArray[i].tree.windIntensity_ST != 0f)))
                    {
                        for (int j = 0; j < treesPoolArray[i].objsArray.Length; j++)
                        {
                            for (int k = 0; k < treesPoolArray[i].objsArray[j].materialsMesh.Length; k++)
                            {
                                treesPoolArray[i].objsArray[j].materialsMesh[k].EnableKeyword("ENABLE_ALTWIND");
                            }
                            for (int k = 0; k < treesPoolArray[i].objsArray[j].materialsMeshCrossFade.Length; k++)
                            {
                                treesPoolArray[i].objsArray[j].materialsMeshCrossFade[k].EnableKeyword("ENABLE_ALTWIND");
                            }
                        }
                        treesPoolArray[i].tree.windStar = true;
                    }

                    if (treesPoolArray[i].tree.windMode == 1 && treesPoolArray[i].tree.loadedConfig)
                    {
                        if (treesPoolArray[i].tree.windIntensity_ST != 0f)
                        {
                            treesPoolArray[i].tree.calcWindParams_ST(ref _speedWind, ref _directionWind);

                            for (int j = 0; j < treesPoolArray[i].objsArray.Length; j++)
                            {
                                for (int k = 0; k < treesPoolArray[i].objsArray[j].materialsMesh.Length; k++)
                                {
                                    treesPoolArray[i].objsArray[j].materialsMesh[k].SetVector("_ST_WindVector", treesPoolArray[i].tree._ST_WindVector);
                                    treesPoolArray[i].objsArray[j].materialsMesh[k].SetVector("_ST_WindGlobal", treesPoolArray[i].tree._ST_WindGlobal);
                                    treesPoolArray[i].objsArray[j].materialsMesh[k].SetVector("_ST_WindBranch", treesPoolArray[i].tree._ST_WindBranch);
                                    treesPoolArray[i].objsArray[j].materialsMesh[k].SetVector("_ST_WindBranchTwitch", treesPoolArray[i].tree._ST_WindBranchTwitch);
                                    treesPoolArray[i].objsArray[j].materialsMesh[k].SetVector("_ST_WindBranchWhip", treesPoolArray[i].tree._ST_WindBranchWhip);
                                    treesPoolArray[i].objsArray[j].materialsMesh[k].SetVector("_ST_WindBranchAnchor", treesPoolArray[i].tree._ST_WindBranchAnchor);
                                    treesPoolArray[i].objsArray[j].materialsMesh[k].SetVector("_ST_WindBranchAdherences", treesPoolArray[i].tree._ST_WindBranchAdherences);
                                    treesPoolArray[i].objsArray[j].materialsMesh[k].SetVector("_ST_WindTurbulences", treesPoolArray[i].tree._ST_WindTurbulences);
                                    treesPoolArray[i].objsArray[j].materialsMesh[k].SetVector("_ST_WindLeaf1Ripple", treesPoolArray[i].tree._ST_WindLeaf1Ripple);
                                    treesPoolArray[i].objsArray[j].materialsMesh[k].SetVector("_ST_WindLeaf1Tumble", treesPoolArray[i].tree._ST_WindLeaf1Tumble);
                                    treesPoolArray[i].objsArray[j].materialsMesh[k].SetVector("_ST_WindLeaf1Twitch", treesPoolArray[i].tree._ST_WindLeaf1Twitch);
                                    treesPoolArray[i].objsArray[j].materialsMesh[k].SetVector("_ST_WindLeaf2Ripple", treesPoolArray[i].tree._ST_WindLeaf2Ripple);
                                    treesPoolArray[i].objsArray[j].materialsMesh[k].SetVector("_ST_WindLeaf2Tumble", treesPoolArray[i].tree._ST_WindLeaf2Tumble);
                                    treesPoolArray[i].objsArray[j].materialsMesh[k].SetVector("_ST_WindLeaf2Twitch", treesPoolArray[i].tree._ST_WindLeaf2Twitch);
                                    treesPoolArray[i].objsArray[j].materialsMesh[k].SetVector("_ST_WindFrondRipple", treesPoolArray[i].tree._ST_WindFrondRipple);
                                    treesPoolArray[i].objsArray[j].materialsMesh[k].SetVector("_ST_WindAnimation", treesPoolArray[i].tree._ST_WindAnimation);
                                }
                                for (int k = 0; k < treesPoolArray[i].objsArray[j].materialsMeshCrossFade.Length; k++)
                                {
                                    treesPoolArray[i].objsArray[j].materialsMeshCrossFade[k].SetVector("_ST_WindVector", treesPoolArray[i].tree._ST_WindVector);
                                    treesPoolArray[i].objsArray[j].materialsMeshCrossFade[k].SetVector("_ST_WindGlobal", treesPoolArray[i].tree._ST_WindGlobal);
                                    treesPoolArray[i].objsArray[j].materialsMeshCrossFade[k].SetVector("_ST_WindBranch", treesPoolArray[i].tree._ST_WindBranch);
                                    treesPoolArray[i].objsArray[j].materialsMeshCrossFade[k].SetVector("_ST_WindBranchTwitch", treesPoolArray[i].tree._ST_WindBranchTwitch);
                                    treesPoolArray[i].objsArray[j].materialsMeshCrossFade[k].SetVector("_ST_WindBranchWhip", treesPoolArray[i].tree._ST_WindBranchWhip);
                                    treesPoolArray[i].objsArray[j].materialsMeshCrossFade[k].SetVector("_ST_WindBranchAnchor", treesPoolArray[i].tree._ST_WindBranchAnchor);
                                    treesPoolArray[i].objsArray[j].materialsMeshCrossFade[k].SetVector("_ST_WindBranchAdherences", treesPoolArray[i].tree._ST_WindBranchAdherences);
                                    treesPoolArray[i].objsArray[j].materialsMeshCrossFade[k].SetVector("_ST_WindTurbulences", treesPoolArray[i].tree._ST_WindTurbulences);
                                    treesPoolArray[i].objsArray[j].materialsMeshCrossFade[k].SetVector("_ST_WindLeaf1Ripple", treesPoolArray[i].tree._ST_WindLeaf1Ripple);
                                    treesPoolArray[i].objsArray[j].materialsMeshCrossFade[k].SetVector("_ST_WindLeaf1Tumble", treesPoolArray[i].tree._ST_WindLeaf1Tumble);
                                    treesPoolArray[i].objsArray[j].materialsMeshCrossFade[k].SetVector("_ST_WindLeaf1Twitch", treesPoolArray[i].tree._ST_WindLeaf1Twitch);
                                    treesPoolArray[i].objsArray[j].materialsMeshCrossFade[k].SetVector("_ST_WindLeaf2Ripple", treesPoolArray[i].tree._ST_WindLeaf2Ripple);
                                    treesPoolArray[i].objsArray[j].materialsMeshCrossFade[k].SetVector("_ST_WindLeaf2Tumble", treesPoolArray[i].tree._ST_WindLeaf2Tumble);
                                    treesPoolArray[i].objsArray[j].materialsMeshCrossFade[k].SetVector("_ST_WindLeaf2Twitch", treesPoolArray[i].tree._ST_WindLeaf2Twitch);
                                    treesPoolArray[i].objsArray[j].materialsMeshCrossFade[k].SetVector("_ST_WindFrondRipple", treesPoolArray[i].tree._ST_WindFrondRipple);
                                    treesPoolArray[i].objsArray[j].materialsMeshCrossFade[k].SetVector("_ST_WindAnimation", treesPoolArray[i].tree._ST_WindAnimation);
                                }
                            }
                        }
                    }
                    else if (treesPoolArray[i].tree.windMode == 2)
                    {
                        _directionWind2 = _directionWind * _speedWind * treesPoolArray[i].tree.windBendCoefficient_TC * treesPoolArray[i].tree.windIntensity_TC;
                        for (int j = 0; j < treesPoolArray[i].objsArray.Length; j++)
                        {
                            for (int k = 0; k < treesPoolArray[i].objsArray[j].materialsMesh.Length; k++)
                            {
                                treesPoolArray[i].objsArray[j].materialsMesh[k].SetVector("_WindAltTree", new Vector4(_directionWind2.x, _directionWind2.y, _directionWind2.z , _speedWind * treesPoolArray[i].tree.windIntensity_TC * treesPoolArray[i].tree.windTurbulenceCoefficient_TC));
                            }
                            for (int k = 0; k < treesPoolArray[i].objsArray[j].materialsMeshCrossFade.Length; k++)
                            {
                                treesPoolArray[i].objsArray[j].materialsMeshCrossFade[k].SetVector("_WindAltTree", new Vector4(_directionWind2.x, _directionWind2.y, _directionWind2.z, _speedWind * treesPoolArray[i].tree.windIntensity_TC * treesPoolArray[i].tree.windTurbulenceCoefficient_TC));
                            }
                        }
                    }
                }
            }
            else
            {
                if (starStatWind)
                {
                    for (int i = 0; i < treePrototypeIds.Length; i++)
                    {
                        for (int j = 0; j < treesPoolArray[i].objsArray.Length; j++)
                        {
                            for (int k = 0; k < treesPoolArray[i].objsArray[j].materialsMesh.Length; k++)
                            {
                                treesPoolArray[i].objsArray[j].materialsMesh[k].DisableKeyword("ENABLE_ALTWIND");
                            }
                            for (int k = 0; k < treesPoolArray[i].objsArray[j].materialsMeshCrossFade.Length; k++)
                            {
                                treesPoolArray[i].objsArray[j].materialsMeshCrossFade[k].DisableKeyword("ENABLE_ALTWIND");
                            }
                        }
                        treesPoolArray[i].tree.windStar = false;
                    }

                    starStatWind = false;
                }
            }


            checkCrossFade();

            lock (distanceCamerasLock)
            {
                if (distanceCamerasInit)
                {
                    distanceCamerasSpeedTimeStar = distanceCamerasSpeedTime;
                    distanceCamerasSpeedTime = Time.time;
                    maxSpeedCamerasTemp = 0;
                    for (int i = 0; i < distanceCameras.Length; i++)
                    {
                        distanceCamerasSpeedVector = distanceCameras[i].trans.position;
                        distanceCameras[i].speed = AltUtilities.fastDistance(ref distanceCameras[i].pos, ref distanceCamerasSpeedVector) / (distanceCamerasSpeedTime - distanceCamerasSpeedTimeStar);
                        maxSpeedCamerasTemp = Mathf.Max(maxSpeedCamerasTemp, distanceCameras[i].speed);
                        distanceCameras[i].pos = distanceCamerasSpeedVector;
                    }
                    maxSpeedCamerasArray[0] = maxSpeedCamerasArray[1];
                    maxSpeedCamerasArray[1] = maxSpeedCamerasArray[2];
                    maxSpeedCamerasArray[2] = maxSpeedCamerasArray[3];
                    maxSpeedCamerasArray[3] = maxSpeedCamerasArray[4];
                    maxSpeedCamerasArray[4] = maxSpeedCamerasTemp;
                    maxSpeedCameras = (maxSpeedCamerasArray[0] + maxSpeedCamerasArray[1] + maxSpeedCamerasArray[2] + maxSpeedCamerasArray[3] + maxSpeedCamerasArray[4]) / 5f;
                    altTreesMain.altTreesManagerData.maxSpeedCameras = maxSpeedCameras;
                }
            }

            if (distanceCamerasInit)
            {
                if (!altTreesMain.altTreesManagerData.multiThreading)
                {
                    if (idCheck >= quads.Length)
                        idCheck = 0;
                    if (quads.Length != 0 && quads[idCheck] != null)
                    {
                        quads[idCheck].check(distanceCameras, true, true);
                        quads[idCheck].checkObjs(null, distanceCameras, false, altTreesMain.isPlaying);
                    }
                    idCheck++;
                }
                else
                {
                    for (int y = 0; y < quads.Length; y++)
                    {
                        if (quads.Length != 0 && quads[y] != null)
                        {
                            lock (checkLock)
                            {
                                quads[y].checkObjs(null, distanceCameras, true, altTreesMain.isPlaying);
                            }
                        }
                    }
                }
            }

            if (altTreesMain.altTreesManagerData.drawDebugPatches != altTreesMain.altTreesManagerData.drawDebugPatchesStar)
            {
                for (int i = 0; i < patches.Length; i++)
                {
                    if (patches[i] != null)
                    {
                        if (quads[i] != null)
                            quads[i].checkDebugPutches(altTreesMain.altTreesManagerData.drawDebugPatches);
                    }
                }
                altTreesMain.altTreesManagerData.drawDebugPatchesStar = altTreesMain.altTreesManagerData.drawDebugPatches;
            }

            
            if (AltTreesQuad.objsToInit.Count > 0)
		    {
                if (altTreesMain.altTreesManagerData.multiThreading)
                {
                    for (int i = 0; i < AltTreesQuad.objsToInit.Count; i++)
                    {
                        if (!AltTreesQuad.objsToInit[i].createMeshBillboardsStarted && !AltTreesQuad.objsToInit[i].createMeshBillboardsOk && AltTreesQuad.objsToInit[i].treesCount != 0 && (AltTreesQuad.objsToInit[i].LOD <= AltTreesQuad.objsToInit[i].startBillboardsLOD /*|| AltTreesQuad.objsToInit[i].isGenerateAllBillboardsOnStart*/))
                        {
                            AltTreesQuad.objsToInit[i].createMeshBillboardsStarted = true;
                            AltTreesQuad.objsToInit[i].createMeshBillboardsOk = false;
                            ThreadPool.QueueUserWorkItem(createMeshBillboardsStartThread, AltTreesQuad.objsToInit[i]);
                        }
                    }
                }


                initTimeStartedCount = 0;
                genPerFrameTemp = 0;
                bool noDel = false;
                atqIdTemp = -1;
                
                countPatchesforInit = AltTreesQuad.objsToInit.Count;

                while (countPatchesforInit > 0 && genPerFrame > genPerFrameTemp)
                {
                    atqIdTemp = -1;
                    atqTemp = null;

                    if (altTreesMain.altTreesManagerData.multiThreading)
                    {
                        for(int i = 0; i < AltTreesQuad.objsToInit.Count; i++)
                        {
                            atqTemp = AltTreesQuad.objsToInit[i];
                            if (patches != null && patches.Length > atqTemp.patchID && patches[atqTemp.patchID] != null && patches[atqTemp.patchID].draw)
                            {
                                if (atqTemp.treesCount != 0 && (atqTemp.LOD <= atqTemp.startBillboardsLOD /*|| atqTemp.isGenerateAllBillboardsOnStart*/))
                                {
                                    if (altTreesMain.altTreesManagerData.multiThreading)
                                    {
                                        if (atqTemp.createMeshBillboardsOk)
                                        {
                                            atqIdTemp = i;
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    atqIdTemp = i;
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        atqIdTemp = 0;
                        atqTemp = AltTreesQuad.objsToInit[0];
                    }

                    noDel = false;
                    genPerFrameTemp++;
                    if (atqIdTemp != -1)
                    {
                        if (patches != null && patches.Length > atqTemp.patchID && patches[atqTemp.patchID] != null && patches[atqTemp.patchID].draw)
                        {
                            if (Application.isEditor && atqTemp.rendersDebug == null)
                            {
                                GameObject go = Instantiate(goCubeDebug);
                                go.transform.position = new Vector3(atqTemp.pos.x, 1, atqTemp.pos.y);
                                go.transform.localScale = new Vector3(atqTemp.size, 4, atqTemp.size);
                                propBlock.Clear();
                                colorTemp.r = Random.value;
                                colorTemp.g = Random.value;
                                colorTemp.b = Random.value;
                                colorTemp.a = 0.3f;
                                propBlock.SetColor(AltTreesManager.Color_PropertyID, colorTemp);
                                go.transform.parent = this.transform;
                                go.name = "debug_" + atqTemp.LOD;
                                go.hideFlags = HideFlags.DontSave;
                                go.GetComponent<MeshRenderer>().SetPropertyBlock(propBlock);
                                go.SetActive(false);
                                atqTemp.rendersDebug = go;
                                if (patches[atqTemp.patchID].rendersDebug == null)
                                    patches[atqTemp.patchID].rendersDebug = new List<GameObject>();
                                patches[atqTemp.patchID].rendersDebug.Add(go);
                            }

                            if (atqTemp.treesCount != 0 && (atqTemp.LOD <= atqTemp.startBillboardsLOD /*|| atqTemp.isGenerateAllBillboardsOnStart*/))
                            {
                                if (altTreesMain.altTreesManagerData.multiThreading)
                                {
                                    if (atqTemp.createMeshBillboardsOk)
                                    {
                                        if (createMeshBillboardsThread(atqTemp))
                                        {
                                            atqTemp.createMeshBillboardsOk = false;
                                            atqTemp.createMeshBillboardsStarted = false;
                                        }
                                        else
                                            noDel = true;
                                    }
                                    else
                                        noDel = true;
                                }
                                else
                                    createMeshBillboards(atqTemp);
                            }
                            else
                            {
                                if (atqTemp.meshes.Count != 0)
                                {
                                    for (int i = 0; i < atqTemp.meshes.Count; i++)
                                    {
                                        if (altTreesMain.isPlaying)
                                            Destroy(atqTemp.meshes[i].mesh);
                                        else
                                            DestroyImmediate(atqTemp.meshes[i].mesh);
                                    }
                                    atqTemp.meshes.Clear();
                                }
                                quadsToRender.Remove(atqTemp);
                                atqTemp.isInitBillboardsInQueue = false;
                                atqTemp.isInitBillboards = true;

                                if (GCCollect)
                                {
                                    //System.GC.Collect();
                                    GCCollect = false;
                                }
                            }
                        }
                        else
                            altTreesMain.LogError("(patches != null && patches.Length > atqTemp.patchID && patches[atqTemp.patchID] != null && patches[atqTemp.patchID].draw)");

                        if (!noDel)
                        {
                            AltTreesQuad.objsToInit[atqIdTemp].isInitBillboardsInQueue = false;
                            AltTreesQuad.objsToInit.RemoveAt(atqIdTemp);
                            needUpdateScene = true;
                        }

                        countPatchesforInit = AltTreesQuad.objsToInit.Count;
                    }
                    atqIdTemp = -1;
                    atqTemp = null;
                }
		    }
            else
            {
                if (initTimeStarted)
                {
                    if (initTimeStartedCount != -1)
                    {
                        initTimeStartedCount++;
                        if (initTimeStartedCount > 10)
                        {
                            initTimeStarted = false;
                            initTimeStartedCount = -1;

                            swTime.Stop();
                            System.GC.Collect();
                            altTreesMain.isInitialized = true;

                            if (altTreesMain.altTreesLoaded != null)
                                altTreesMain.altTreesLoaded(swTime.ElapsedMilliseconds);

                            altTreesMain.Log("Loading time: " + swTime.ElapsedMilliseconds);
                        }
                    }
                }

                if (GCCollect)
                {
                    //System.GC.Collect();
                    GCCollect = false;
                }
            }


            if (!altTreesMain.isPlaying)
            {
                if (needUpdateScene)
                {
                    altTreesMain.needUpdateScene = true;
                    needUpdateScene = false;
                }

                if(!frustumCameraTransformEditor.position.Equals(frustumCameraTransformEditorPosStar) || !frustumCameraTransformEditor.rotation.Equals(frustumCameraTransformEditorRotStar))
                {
                    altTreesMain.needUpdateScene = true;
                    frustumCameraTransformEditorPosStar = frustumCameraTransformEditor.position;
                    frustumCameraTransformEditorRotStar = frustumCameraTransformEditor.rotation;
                }
            }

            if (needInitColliders)
            {
                bool isNeedInitColliders = false;
                int countInitSch = 100;
                int countInit = 0;
                for (int i = 0; i < treePrototypeIds.Length; i++)
                {
                    if(treesPoolArray[i].needInitCollidersCount > 0)
                    {
                        if(countInitSch == 0)
                        {
                            isNeedInitColliders = true;
                            break;
                        }

                        countInit = Mathf.Min(treesPoolArray[i].needInitCollidersCount, countInitSch);
                        for (int j = 0; j < countInit; j++)
                        {
                            treesPoolArray[i].collidersArray.Add(new ColliderPool());
                            treesPoolArray[i].collidersArray[treesPoolArray[i].collidersArray.Count - 1].go = Instantiate(treesPoolArray[i].tree.colliders, vectTemp, Quaternion.identity) as GameObject;
                            treesPoolArray[i].collidersArray[treesPoolArray[i].collidersArray.Count - 1].go.SetActive(false);
                            treesPoolArray[i].collidersArray[treesPoolArray[i].collidersArray.Count - 1].go.transform.parent = transform;
                            treesPoolArray[i].collidersArray[treesPoolArray[i].collidersArray.Count - 1].go.hideFlags = HideFlags.DontSave;

                            if (altTreesMain.altTreesManagerData.colliderEvents)
                            {
                                Transform[] trTemp = treesPoolArray[i].collidersArray[treesPoolArray[i].collidersArray.Count - 1].go.GetComponentsInChildren<Transform>(true);
                            
                                treesPoolArray[i].collidersArray[treesPoolArray[i].collidersArray.Count - 1].colliders = new AltCollider[trTemp.Length];
                                for (int y = 0; y < trTemp.Length; y++)
                                {
                                    treesPoolArray[i].collidersArray[treesPoolArray[i].collidersArray.Count - 1].colliders[y] = trTemp[y].gameObject.AddComponent<AltCollider>();
                                }
                            }
                        }
                        treesPoolArray[i].needInitCollidersCount -= countInit;
                        countInitSch -= countInit;
                        if (treesPoolArray[i].needInitCollidersCount > 0)
                        {
                            isNeedInitColliders = true;
                            break;
                        }
                    }
                    if (treesPoolArray[i].needInitBillboardCollidersCount > 0)
                    {
                        if (countInitSch == 0)
                        {
                            isNeedInitColliders = true;
                            break;
                        }

                        countInit = Mathf.Min(treesPoolArray[i].needInitBillboardCollidersCount, countInitSch);
                        for (int j = 0; j < altTreesMain.altTreesManagerData.initColliderBillboardsCountPool; j++)
                        {
                            treesPoolArray[i].colliderBillboardsArray.Add(new ColliderPool());
                            treesPoolArray[i].colliderBillboardsArray[treesPoolArray[i].colliderBillboardsArray.Count - 1].go = Instantiate(treesPoolArray[i].tree.billboardColliders, vectTemp, Quaternion.identity) as GameObject;
                            treesPoolArray[i].colliderBillboardsArray[treesPoolArray[i].colliderBillboardsArray.Count - 1].go.SetActive(false);
                            treesPoolArray[i].colliderBillboardsArray[treesPoolArray[i].colliderBillboardsArray.Count - 1].go.transform.parent = transform;
                            treesPoolArray[i].colliderBillboardsArray[treesPoolArray[i].colliderBillboardsArray.Count - 1].go.hideFlags = HideFlags.DontSave;

                            if (altTreesMain.altTreesManagerData.colliderEvents)
                            {
                                Transform[] trTemp = treesPoolArray[i].colliderBillboardsArray[treesPoolArray[i].colliderBillboardsArray.Count - 1].go.GetComponentsInChildren<Transform>(true);
                            
                                treesPoolArray[i].colliderBillboardsArray[treesPoolArray[i].colliderBillboardsArray.Count - 1].colliders = new AltCollider[trTemp.Length];
                                for (int y = 0; y < trTemp.Length; y++)
                                {
                                    treesPoolArray[i].colliderBillboardsArray[treesPoolArray[i].colliderBillboardsArray.Count - 1].colliders[y] = trTemp[y].gameObject.AddComponent<AltCollider>();
                                }
                            }
                        }
                        treesPoolArray[i].needInitBillboardCollidersCount -= countInit;
                        countInitSch -= countInit;
                        if (treesPoolArray[i].needInitBillboardCollidersCount > 0)
                        {
                            isNeedInitColliders = true;
                            break;
                        }
                    }
                }

                if (!isNeedInitColliders)
                    needInitColliders = false;
            }
	    }
        bool checkerTimerIsWorked = false;
        object checkerTimerLock = new object();
        object checkLock = new object();
        bool isPlayingTimer = false;

        void checkerTimer(object state)
        {
            lock (timerLock)
            {
                if (timerLockStop || !distanceCamerasInit)
                    return;
                #if UNITY_EDITOR
                try
                #endif
                {
                    if (checkerTimerIsWorked || !isInit)
                        return;

                    lock (checkerTimerLock)
                    {
                        checkerTimerIsWorked = true;


                        /*if (boxInFrustum(planes, myPlanes, new Bounds3D()))
                        {

                        }*/
                        lock (checkLock)
                        {
                            for (int y = 0; y < quads.Length; y++)
                            {
                                if (quads.Length != 0 && quads[y] != null)
                                {
                                    quads[y].check(distanceCameras, true, true);
                                }
                            }
                        }
                        for (int y = 0; y < quads.Length; y++)
                        {
                            if (quads.Length != 0 && quads[y] != null)
                            {
                                quads[y].checkObjsTimer(distanceCameras, isPlayingTimer, altTreesMain.altTreesManagerData.multiThreading);
                            }
                        }


                        checkerTimerIsWorked = false;
                    }
                }
                #if UNITY_EDITOR
                catch (System.Exception e)
                {
                    Debug.LogError(e);
                }
                #endif
            }
        }

        bool checkerFrustumCullingIsWorked = false;
        object checkerFrustumCullingLock = new object();
        TreesToRender ttrTempFrustum = null;
        //float floatTempCheckerFrustumCulling;
        bool isGoTo = false;

        void checkerFrustumCulling(object state)
        {
            #if UNITY_EDITOR
            try
            #endif
            {
                if (checkerFrustumCullingIsWorked || !isInit)
                {
                    isGoTo = true;
                    return;
                }

                lock (checkerFrustumCullingLock)
                {
                    checkerFrustumCullingIsWorked = true;

                    GotoStart:

                    int treesPoolArrayLength = 0;
                    int treesPoolArrayLengthItems = 0;
                    bool isOkB = false;
                    bool inFrustum = false;

                    treesPoolArrayLength = treesPoolArray.Length;

                    for (int i = 0; i < treesPoolArrayLength; i++)
                    {
                        /*if (treesPoolArray[i].tree.densityObjects != 0f && treesPoolArray[i].tree.isObject)
                            floatTempCheckerFrustumCulling = altTreesMain.altTreesManagerData.densityObjects * (3f - treesPoolArray[i].tree.densityObjects * 2f);
                        else
                            floatTempCheckerFrustumCulling = 101f;*/

                        treesPoolArrayLengthItems = treesPoolArray[i].treesToRenderCount;

                        for (int t = 0; t < treesPoolArrayLengthItems; t++)
                        {
                            ttrTempFrustum = treesPoolArray[i].treesToRender[t];

                            if (ttrTempFrustum.noNull)
                            {
                                if (ttrTempFrustum.att.frustumSchet > 0)
                                {
                                    ttrTempFrustum.att.frustumSchet--;
                                    continue;
                                }

                                //if (ttrTempFrustum.att.densityObjects <= floatTempCheckerFrustumCulling)
                                {
                                    isOkB = false;
                                    inFrustum = false;
                                    if (!altTreesMain.altTreesManagerData.hideBillboards)
                                    {
                                        if (ttrTempFrustum.att.isBillboard)
                                        {
                                            lock (frustumCamerasLock)
                                            {
                                                for (int k = 0; k < frustumCameras.Length; k++)
                                                {
                                                    if (frustumCameras[k].isActiveAndEnabled)
                                                    {
                                                        inFrustum = AltUtilities.boxInFrustum(frustumCameras[k].myPlanes, ttrTempFrustum.att.bound);
                                                        if (inFrustum)
                                                            break;
                                                    }
                                                }
                                                isOkB = true;
                                            }
                                        }
                                    }


                                    if (!isOkB && !altTreesMain.altTreesManagerData.hideMeshes)
                                    {
                                        if (ttrTempFrustum.att.isMesh)
                                        {
                                            lock (frustumCamerasLock)
                                            {
                                                for (int k = 0; k < frustumCameras.Length; k++)
                                                {
                                                    if (frustumCameras[k].isActiveAndEnabled)
                                                    {
                                                        inFrustum = AltUtilities.boxInFrustum(frustumCameras[k].myPlanes, ttrTempFrustum.att.bound);
                                                        if (inFrustum)
                                                            break;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    ttrTempFrustum.att.inFrustum = inFrustum;
                                    if (inFrustum)
                                        ttrTempFrustum.att.frustumSchet = 3;
                                }
                            }
                        }
                    }


                    if(isGoTo)
                    {
                        isGoTo = false;
                        goto GotoStart;
                    }

                    checkerFrustumCullingIsWorked = false;
                }
            }
            #if UNITY_EDITOR
            catch (System.Exception e)
            {
                Debug.LogError(e);
            }
            #endif
        }


        public void setFloatingOriginJump(Vector3 vector)
        {
            //lock (timerLock)
            {
                int tempInt = Mathf.FloorToInt(Mathf.Abs(vector.x / altTreesMain.altTreesManagerData.sizePatch));
                jump.x += tempInt;
                vector.x -= altTreesMain.altTreesManagerData.sizePatch * tempInt;

                tempInt = Mathf.FloorToInt(Mathf.Abs(vector.y / altTreesMain.altTreesManagerData.sizePatch));
                jump.y += tempInt;
                vector.y -= altTreesMain.altTreesManagerData.sizePatch * tempInt;

                tempInt = Mathf.FloorToInt(Mathf.Abs(vector.z / altTreesMain.altTreesManagerData.sizePatch));
                jump.z += tempInt;
                vector.z -= altTreesMain.altTreesManagerData.sizePatch * tempInt;

                jumpPos -= vector;

                for (int i = 0; i < quads.Length; i++)
                {
                    if (quads[i] != null)
                    {
                        quads[i].setFloatingOriginJump((patches[i].stepX - jump.x) * altTreesMain.altTreesManagerData.sizePatch + jumpPos.x + (float)altTreesMain.altTreesManagerData.sizePatch / 2f, (patches[i].stepY - jump.z) * altTreesMain.altTreesManagerData.sizePatch + jumpPos.z + (float)altTreesMain.altTreesManagerData.sizePatch / 2f);
                    }
                }

                for (int i = 0; i < treesPoolArray.Length; i++)
                {
                    for (int t = 0; t < treesPoolArray[i].treesToRenderCount; t++)
                    {
                        if (treesPoolArray[i].treesToRender[t].noNull)
                        {
                            treesPoolArray[i].treesToRender[t].att.posWorldMesh = treesPoolArray[i].treesToRender[t].att.getPosWorld();
                            treesPoolArray[i].treesToRender[t].att.posWorldBillboard = treesPoolArray[i].treesToRender[t].att.posWorldMesh + new Vector3(0f, treesPoolArray[treesPoolArray[i].treesToRender[t].att.idPrototypeIndex].tree.size * treesPoolArray[i].treesToRender[t].att.heightScale / 2f + treesPoolArray[treesPoolArray[i].treesToRender[t].att.idPrototypeIndex].tree.up * treesPoolArray[i].treesToRender[t].att.heightScale, 0f);
                            treesPoolArray[i].treesToRender[t].att.bound.center = treesPoolArray[i].treesToRender[t].att.posWorldBillboard;

                            if (treesPoolArray[i].treesToRender[t].att.isBillboard)
                            {
                                treesPoolArray[i].treesToRender[t].att.matrixBillboard = Matrix4x4.TRS(treesPoolArray[i].treesToRender[t].att.posWorldBillboard, Quaternion.identity, Vector3.one);
                            }

                            if (treesPoolArray[i].treesToRender[t].att.isMesh)
                            {
                                scaleTemp.x = treesPoolArray[i].treesToRender[t].att.widthScale;
                                scaleTemp.y = treesPoolArray[i].treesToRender[t].att.heightScale;
                                scaleTemp.z = treesPoolArray[i].treesToRender[t].att.widthScale;
                                treesPoolArray[i].treesToRender[t].att.matrixMesh = Matrix4x4.TRS(treesPoolArray[i].treesToRender[t].att.posWorldMesh, Quaternion.AngleAxis(treesPoolArray[i].treesToRender[t].att.rotation, Vector3.up), scaleTemp);
                            }
                        }
                    }
                }

                for (int i = 0; i < collidersUsedList.Count; i++)
                {
                    collidersUsedList[i].collider.go.transform.position = collidersUsedList[i].getPosWorld();
                }

                //checkerTimer(null);
            }
        }



        float _speedWind = 0;
        Vector3 _directionWind = new Vector3();
        Vector3 _directionWind2 = new Vector3();

        float tempFloatNext = 0f;

        void checkCrossFade()
        {
            for (int i = treesCrossFade.Count - 1; i >= 0 ; i--)
            {
                if (altTreesMain.isPlaying)
                    tempFloatNext = treesCrossFade[i].crossFadeTime - Time.time;
                else
                {
                    #if UNITY_EDITOR
                    {
                        tempFloatNext = treesCrossFade[i].crossFadeTime - (float)EditorApplication.timeSinceStartup;
                    }
                    #endif
                }

                if (tempFloatNext <= 0f)
                {
                    if (treesCrossFade[i].currentCrossFadeId == 1)
                    {
                        treesCrossFade[i].isCrossFadeBillboard = false;

                        if (!altTreesMain.isPlaying)//
                        {
                            treesCrossFade[i].crossFadeTreeMeshRenderer.sharedMaterials = treesPoolArray[treesCrossFade[i].idPrototypeIndex].objsArray[treesCrossFade[i].currentLOD].materialsMesh;
                            propBlock.Clear();
                            propBlock.SetColor(HueVariationLeave_PropertyID, treesCrossFade[i].color);
                            propBlock.SetColor(HueVariationBark_PropertyID, treesCrossFade[i].colorBark);
                            propBlock.SetFloat(Alpha_PropertyID, 1.0f);
                            propBlock.SetFloat(Ind_PropertyID, 0.0f);
                            propBlock.SetFloat(smoothValue_PropertyID, 0.0f);
                            treesCrossFade[i].crossFadeTreeMeshRenderer.SetPropertyBlock(propBlock);
                            treesCrossFade[i].alphaPropBlockMesh = 1;
                            treesCrossFade[i].indPropBlockMesh = 0;
                            treesCrossFade[i].smoothPropBlock = 0;

                            treesCrossFade[i].goMesh.GetComponent<AltTreeInstance>().isCrossFade = false;
                        }
                        else
                        {
                            treesCrossFade[i].isCrossFadeMesh = false;
                            if (!gpuInstancingSupport || !treesCrossFade[i].gpuInstancing)
                            {
                                treesCrossFade[i].propBlockMesh.SetFloat(Alpha_PropertyID, 1f);
                                treesCrossFade[i].propBlockMesh.SetFloat(smoothValue_PropertyID, 0f);
                            }
                            treesCrossFade[i].alphaPropBlockMesh = 1;
                            treesCrossFade[i].smoothPropBlock = 0;

                        }
                        delObjBillboard(treesCrossFade[i]);
                    }
                    else if (treesCrossFade[i].currentCrossFadeId == 6)
                    {
                        if (!altTreesMain.isPlaying)//
                        {
                            treesCrossFade[i].crossFadeTreeMeshRenderer.sharedMaterials = treesPoolArray[treesCrossFade[i].idPrototypeIndex].objsArray[treesCrossFade[i].currentLOD].materialsMesh;
                            propBlock.Clear();
                            propBlock.SetColor(HueVariationLeave_PropertyID, treesCrossFade[i].color);
                            propBlock.SetColor(HueVariationBark_PropertyID, treesCrossFade[i].colorBark);
                            propBlock.SetFloat(Alpha_PropertyID, 1.0f);
                            propBlock.SetFloat(Ind_PropertyID, 0.0f);
                            propBlock.SetFloat(smoothValue_PropertyID, 0.0f);
                            treesCrossFade[i].crossFadeTreeMeshRenderer.SetPropertyBlock(propBlock);
                            treesCrossFade[i].alphaPropBlockMesh = 1;
                            treesCrossFade[i].indPropBlockMesh = 0;
                            treesCrossFade[i].smoothPropBlock = 0;

                            treesCrossFade[i].goMesh.GetComponent<AltTreeInstance>().isCrossFade = false;
                        }
                        else
                        {
                            treesCrossFade[i].isCrossFadeMesh = false;
                            if (!gpuInstancingSupport || !treesCrossFade[i].gpuInstancing)
                            {
                                treesCrossFade[i].propBlockMesh.SetFloat(Alpha_PropertyID, 1f);
                                treesCrossFade[i].propBlockMesh.SetFloat(smoothValue_PropertyID, 0f);
                            }
                            treesCrossFade[i].alphaPropBlockMesh = 1;
                            treesCrossFade[i].smoothPropBlock = 0;
                        }
                    }
                    else if (treesCrossFade[i].currentCrossFadeId == 7)
                    {
                        treesCrossFade[i].isCrossFadeBillboard = false;
                        delObjBillboard(treesCrossFade[i]);
                    }
                    else if (treesCrossFade[i].currentCrossFadeId == 2)
                    {
                        if (!altTreesMain.isPlaying)//
                        {
                            delObjMeshEditor(treesCrossFade[i].goMesh);
                        }
                        else
                        {
                            treesCrossFade[i].isCrossFadeMesh = false;
                            if (!gpuInstancingSupport || !treesCrossFade[i].gpuInstancing)
                            {
                                treesCrossFade[i].propBlockMesh.SetFloat(Alpha_PropertyID, 1f);
                                treesCrossFade[i].propBlockMesh.SetFloat(smoothValue_PropertyID, 0f);
                            }
                            treesCrossFade[i].alphaPropBlockMesh = 1;
                            treesCrossFade[i].smoothPropBlock = 0;

                            delObjMesh(treesCrossFade[i]);
                        }
                        treesCrossFade[i].isCrossFadeBillboard = false;
                    }
                    else if (treesCrossFade[i].currentCrossFadeId == 5)
                    {
                        if (!altTreesMain.isPlaying)//
                        {
                            delObjMeshEditor(treesCrossFade[i].goMesh);
                        }
                        else
                        {
                            treesCrossFade[i].isCrossFadeMesh = false;
                            if (!gpuInstancingSupport || !treesCrossFade[i].gpuInstancing)
                            {
                                treesCrossFade[i].propBlockMesh.SetFloat(Alpha_PropertyID, 1f);
                                treesCrossFade[i].propBlockMesh.SetFloat(smoothValue_PropertyID, 0f);
                            }
                            treesCrossFade[i].alphaPropBlockMesh = 1;
                            treesCrossFade[i].smoothPropBlock = 0;

                            delObjMesh(treesCrossFade[i]);
                        }
                    }
                    else if (treesCrossFade[i].currentCrossFadeId == 8)
                    {
                        treesCrossFade[i].isCrossFadeBillboard = false;
                    }
                    else if (treesCrossFade[i].currentCrossFadeId == 3)
                    {
                        if (!altTreesMain.isPlaying)//
                        {
                            delObjMeshEditor(treesCrossFade[i].goMesh);

                            treesCrossFade[i].goMesh = getObjMeshEditor(treesCrossFade[i], treesCrossFade[i].currentLOD);

                            /*delObjPoolEditor(treesCrossFade[i].idPrototype, treesCrossFade[i].currentCrossFadeLOD, treesCrossFade[i].goMesh);

                            treesCrossFade[i].goMesh = getObjMeshEditor(treesCrossFade[i], treesCrossFade[i].currentLOD);
                            treesCrossFade[i].crossFadeTreeMeshRenderer.sharedMaterials = treesPoolArray[treesCrossFade[i].idPrototypeIndex].objsArray[treesCrossFade[i].currentLOD].materialsMesh;
                            propBlock.Clear();
                            propBlock.SetColor(AltTreesManager.HueVariationLeave_PropertyID, treesCrossFade[i].color);
                            propBlock.SetColor(AltTreesManager.HueVariationBark_PropertyID, treesCrossFade[i].colorBark);
                            propBlock.SetFloat(AltTreesManager.Alpha_PropertyID, 1.0f);
                            propBlock.SetFloat(AltTreesManager.Ind_PropertyID, 0.0f);
                            propBlock.SetFloat(AltTreesManager.smoothValue_PropertyID, 1.0f);
                            treesCrossFade[i].crossFadeTreeMeshRenderer.SetPropertyBlock(propBlock);*/
                        }
                        else
                        {
                            treesCrossFade[i].isCrossFadeMesh = false;
                            if (!gpuInstancingSupport || !treesCrossFade[i].gpuInstancing)
                            {
                                treesCrossFade[i].propBlockMesh.SetFloat(Alpha_PropertyID, 1f);
                                treesCrossFade[i].propBlockMesh.SetFloat(smoothValue_PropertyID, 0f);
                            }
                            treesCrossFade[i].alphaPropBlockMesh = 1;
                            treesCrossFade[i].smoothPropBlock = 0;
                        }
                    }
                    else if (treesCrossFade[i].currentCrossFadeId == 4)
                    {
                        if (!altTreesMain.isPlaying)//
                        {
                            treesCrossFade[i].goMesh.GetComponent<AltTreeInstance>().isCrossFade = false;
                        }
                        else
                        {
                            treesCrossFade[i].isCrossFadeMesh = false;
                            if (!gpuInstancingSupport || !treesCrossFade[i].gpuInstancing)
                                treesCrossFade[i].propBlockMesh.SetFloat(smoothValue_PropertyID, 0f);
                            treesCrossFade[i].smoothPropBlock = 0;
                        }
                    }
                    
                    if (!altTreesMain.isPlaying)//
                        treesCrossFade[i].crossFadeTreeMeshRenderer = null;

                    treesCrossFade[i].currentCrossFadeId = -1;
                    if (treesCrossFade[i].isBillboard)
                    {
                        if(!gpuInstancingSupport)
                            treesCrossFade[i].propBlockBillboards.SetFloat(Alpha_PropertyID, 1f);
                        treesCrossFade[i].alphaPropBlockBillboard = 1f;
                    }
                    if (treesCrossFade[i].isMesh)
                    {
                        if (!gpuInstancingSupport || !treesCrossFade[i].gpuInstancing)
                        {
                            treesCrossFade[i].propBlockMesh.SetFloat(Alpha_PropertyID, 1f);
                            treesCrossFade[i].propBlockMesh.SetFloat(smoothValue_PropertyID, 0f);
                        }
                        treesCrossFade[i].alphaPropBlockMesh = 1;
                        treesCrossFade[i].smoothPropBlock = 0;
                    }


                    treesCrossFade.RemoveAt(i);
                    needUpdateScene = true;
                }
                else
                {
                    if (((treesCrossFade[i].currentCrossFadeId == 2 || treesCrossFade[i].currentCrossFadeId == 1) /*&& treesCrossFade[i].crossFadeBillboardMeshRenderer != null*/) || (treesCrossFade[i].currentCrossFadeId == 3 || treesCrossFade[i].currentCrossFadeId == 4) || (treesCrossFade[i].currentCrossFadeId == 5 || treesCrossFade[i].currentCrossFadeId == 7 || treesCrossFade[i].currentCrossFadeId == 6 || treesCrossFade[i].currentCrossFadeId == 8))
                    {
                        propBlock.Clear();
                        if (treesCrossFade[i].currentCrossFadeId == 2)
                        {
                            treesCrossFade[i].alphaPropBlockBillboard = Mathf.Clamp(1f - tempFloatNext / altTreesMain.altTreesManagerData.getCrossFadeTimeBillboard(), 0f, 1.0f);
                            if (!gpuInstancingSupport)
                                treesCrossFade[i].propBlockBillboards.SetFloat(Alpha_PropertyID, treesCrossFade[i].alphaPropBlockBillboard);

                            if (!altTreesMain.isPlaying)//
                            {
                                propBlock.SetColor(HueVariationLeave_PropertyID, treesCrossFade[i].color);
                                propBlock.SetColor(HueVariationBark_PropertyID, treesCrossFade[i].colorBark);
                                treesCrossFade[i].alphaPropBlockMesh = Mathf.Clamp(tempFloatNext / altTreesMain.altTreesManagerData.getCrossFadeTimeBillboard(), 0f, 1.0f);
                                treesCrossFade[i].indPropBlockMesh = 0;
                                treesCrossFade[i].smoothPropBlock = 0;
                                propBlock.SetFloat(Alpha_PropertyID, treesCrossFade[i].alphaPropBlockMesh);
                                propBlock.SetFloat(Ind_PropertyID, 0.0f);
                                propBlock.SetFloat(smoothValue_PropertyID, 0.0f);
                                treesCrossFade[i].crossFadeTreeMeshRenderer.SetPropertyBlock(propBlock);
                            }
                            else
                            {
                                treesCrossFade[i].alphaPropBlockMesh = Mathf.Clamp(tempFloatNext / altTreesMain.altTreesManagerData.getCrossFadeTimeBillboard(), 0f, 1.0f);
                                treesCrossFade[i].smoothPropBlock = 0;
                                if (!gpuInstancingSupport || !treesCrossFade[i].gpuInstancing)
                                {
                                    treesCrossFade[i].propBlockMesh.SetFloat(Alpha_PropertyID, treesCrossFade[i].alphaPropBlockMesh);
                                    treesCrossFade[i].propBlockMesh.SetFloat(smoothValue_PropertyID, 0f);
                                }
                            }
                        }
                        else if (treesCrossFade[i].currentCrossFadeId == 5)
                        {
                            if (!altTreesMain.isPlaying)//
                            {
                                propBlock.SetColor(HueVariationLeave_PropertyID, treesCrossFade[i].color);
                                propBlock.SetColor(HueVariationBark_PropertyID, treesCrossFade[i].colorBark);
                                treesCrossFade[i].alphaPropBlockMesh = Mathf.Clamp(tempFloatNext / altTreesMain.altTreesManagerData.getCrossFadeTimeBillboard(), 0f, 1.0f);
                                treesCrossFade[i].indPropBlockMesh = 0;
                                treesCrossFade[i].smoothPropBlock = 0;
                                propBlock.SetFloat(Alpha_PropertyID, treesCrossFade[i].alphaPropBlockMesh);
                                propBlock.SetFloat(Ind_PropertyID, 0.0f);
                                propBlock.SetFloat(smoothValue_PropertyID, 0.0f);
                                treesCrossFade[i].crossFadeTreeMeshRenderer.SetPropertyBlock(propBlock);
                            }
                            else
                            {
                                treesCrossFade[i].alphaPropBlockMesh = Mathf.Clamp(tempFloatNext / altTreesMain.altTreesManagerData.getCrossFadeTimeBillboard(), 0f, 1.0f);
                                treesCrossFade[i].smoothPropBlock = 0;
                                if (!gpuInstancingSupport || !treesCrossFade[i].gpuInstancing)
                                {
                                    treesCrossFade[i].propBlockMesh.SetFloat(Alpha_PropertyID, treesCrossFade[i].alphaPropBlockMesh);
                                    treesCrossFade[i].propBlockMesh.SetFloat(smoothValue_PropertyID, 0f);
                                }
                            }
                        }
                        else if (treesCrossFade[i].currentCrossFadeId == 8)
                        {
                            treesCrossFade[i].alphaPropBlockBillboard = Mathf.Clamp(1f - tempFloatNext / altTreesMain.altTreesManagerData.getCrossFadeTimeBillboard(), 0f, 1.0f);
                            if (!gpuInstancingSupport)
                                treesCrossFade[i].propBlockBillboards.SetFloat(Alpha_PropertyID, treesCrossFade[i].alphaPropBlockBillboard);
                        }
                        else if (treesCrossFade[i].currentCrossFadeId == 1)
                        {
                            treesCrossFade[i].alphaPropBlockBillboard = Mathf.Clamp(tempFloatNext / altTreesMain.altTreesManagerData.getCrossFadeTimeBillboard(), 0f, 1.0f);
                            if (!gpuInstancingSupport)
                                treesCrossFade[i].propBlockBillboards.SetFloat(Alpha_PropertyID, treesCrossFade[i].alphaPropBlockBillboard);    //?

                            if (!altTreesMain.isPlaying)//
                            {
                                propBlock.SetColor(HueVariationLeave_PropertyID, treesCrossFade[i].color);
                                propBlock.SetColor(HueVariationBark_PropertyID, treesCrossFade[i].colorBark);
                                treesCrossFade[i].alphaPropBlockMesh = Mathf.Clamp(1f - tempFloatNext / altTreesMain.altTreesManagerData.getCrossFadeTimeBillboard(), 0f, 1.0f);
                                treesCrossFade[i].indPropBlockMesh = 0;
                                treesCrossFade[i].smoothPropBlock = 0;
                                propBlock.SetFloat(Alpha_PropertyID, treesCrossFade[i].alphaPropBlockMesh);
                                propBlock.SetFloat(Ind_PropertyID, 0.0f);
                                propBlock.SetFloat(smoothValue_PropertyID, 0.0f);
                                treesCrossFade[i].crossFadeTreeMeshRenderer.SetPropertyBlock(propBlock);        //err
                            }
                            else
                            {
                                treesCrossFade[i].alphaPropBlockMesh = Mathf.Clamp(1f - tempFloatNext / altTreesMain.altTreesManagerData.getCrossFadeTimeBillboard(), 0f, 1.0f);
                                treesCrossFade[i].smoothPropBlock = 0;
                                if (!gpuInstancingSupport || !treesCrossFade[i].gpuInstancing)
                                {
                                    treesCrossFade[i].propBlockMesh.SetFloat(Alpha_PropertyID, treesCrossFade[i].alphaPropBlockMesh);
                                    treesCrossFade[i].propBlockMesh.SetFloat(smoothValue_PropertyID, 0f);
                                }
                            }
                        }
                        else if (treesCrossFade[i].currentCrossFadeId == 6)
                        {
                            if (!altTreesMain.isPlaying)//
                            {
                                propBlock.SetColor(HueVariationLeave_PropertyID, treesCrossFade[i].color);
                                propBlock.SetColor(HueVariationBark_PropertyID, treesCrossFade[i].colorBark);
                                treesCrossFade[i].alphaPropBlockMesh = Mathf.Clamp(1f - tempFloatNext / altTreesMain.altTreesManagerData.getCrossFadeTimeBillboard(), 0f, 1.0f);
                                treesCrossFade[i].indPropBlockMesh = 0;
                                treesCrossFade[i].smoothPropBlock = 0;
                                propBlock.SetFloat(Alpha_PropertyID, treesCrossFade[i].alphaPropBlockMesh);
                                propBlock.SetFloat(Ind_PropertyID, 0.0f);
                                propBlock.SetFloat(smoothValue_PropertyID, 0.0f);
                                treesCrossFade[i].crossFadeTreeMeshRenderer.SetPropertyBlock(propBlock);
                            }
                            else
                            {
                                treesCrossFade[i].alphaPropBlockMesh = Mathf.Clamp(1f - tempFloatNext / altTreesMain.altTreesManagerData.getCrossFadeTimeBillboard(), 0f, 1.0f);
                                treesCrossFade[i].smoothPropBlock = 0;
                                if (!gpuInstancingSupport || !treesCrossFade[i].gpuInstancing)
                                {
                                    treesCrossFade[i].propBlockMesh.SetFloat(Alpha_PropertyID, treesCrossFade[i].alphaPropBlockMesh);
                                    treesCrossFade[i].propBlockMesh.SetFloat(smoothValue_PropertyID, 0f);
                                }
                            }
                        }
                        else if (treesCrossFade[i].currentCrossFadeId == 7)
                        {
                            treesCrossFade[i].alphaPropBlockBillboard = Mathf.Clamp(tempFloatNext / altTreesMain.altTreesManagerData.getCrossFadeTimeBillboard(), 0f, 1.0f);
                            if (!gpuInstancingSupport)
                                treesCrossFade[i].propBlockBillboards.SetFloat(Alpha_PropertyID, treesCrossFade[i].alphaPropBlockBillboard);
                        }
                        else if (treesCrossFade[i].currentCrossFadeId == 3)
                        {
                            if (!altTreesMain.isPlaying)//
                            {
                                propBlock.SetColor(HueVariationLeave_PropertyID, treesCrossFade[i].color);
                                propBlock.SetColor(HueVariationBark_PropertyID, treesCrossFade[i].colorBark);
                                treesCrossFade[i].alphaPropBlockMesh = 1;
                                treesCrossFade[i].indPropBlockMesh = 0;
                                treesCrossFade[i].smoothPropBlock = Mathf.Clamp(1f - tempFloatNext / altTreesMain.altTreesManagerData.getCrossFadeTimeMesh(), 0f, 1.0f);
                                propBlock.SetFloat(Alpha_PropertyID, 1.0f);
                                propBlock.SetFloat(Ind_PropertyID, 0.0f);
                                propBlock.SetFloat(smoothValue_PropertyID, treesCrossFade[i].smoothPropBlock);
                                treesCrossFade[i].crossFadeTreeMeshRenderer.SetPropertyBlock(propBlock);
                            }
                            else
                            {
                                treesCrossFade[i].alphaPropBlockMesh = 1;
                                treesCrossFade[i].smoothPropBlock = Mathf.Clamp(1f - tempFloatNext / altTreesMain.altTreesManagerData.getCrossFadeTimeMesh(), 0f, 1.0f);
                                if (!gpuInstancingSupport || !treesCrossFade[i].gpuInstancing)
                                {
                                    treesCrossFade[i].propBlockMesh.SetFloat(smoothValue_PropertyID, treesCrossFade[i].smoothPropBlock);
                                    treesCrossFade[i].propBlockMesh.SetFloat(Alpha_PropertyID, 1f);
                                }
                            }
                        }
                        else if (treesCrossFade[i].currentCrossFadeId == 4)
                        {
                            if (!altTreesMain.isPlaying)//
                            {
                                propBlock.SetColor(HueVariationLeave_PropertyID, treesCrossFade[i].color);
                                propBlock.SetColor(HueVariationBark_PropertyID, treesCrossFade[i].colorBark);
                                treesCrossFade[i].alphaPropBlockMesh = 1;
                                treesCrossFade[i].indPropBlockMesh = 0;
                                treesCrossFade[i].smoothPropBlock = Mathf.Clamp(tempFloatNext / altTreesMain.altTreesManagerData.getCrossFadeTimeMesh(), 0f, 1.0f);
                                propBlock.SetFloat(Alpha_PropertyID, 1.0f);
                                propBlock.SetFloat(Ind_PropertyID, 0.0f);
                                propBlock.SetFloat(smoothValue_PropertyID, treesCrossFade[i].smoothPropBlock);
                                treesCrossFade[i].crossFadeTreeMeshRenderer.SetPropertyBlock(propBlock);
                            }
                            else
                            {
                                treesCrossFade[i].alphaPropBlockMesh = 1;
                                treesCrossFade[i].smoothPropBlock = Mathf.Clamp(tempFloatNext / altTreesMain.altTreesManagerData.getCrossFadeTimeMesh(), 0f, 1.0f);
                                if (!gpuInstancingSupport || !treesCrossFade[i].gpuInstancing)
                                {
                                    treesCrossFade[i].propBlockMesh.SetFloat(smoothValue_PropertyID, treesCrossFade[i].smoothPropBlock);
                                    treesCrossFade[i].propBlockMesh.SetFloat(Alpha_PropertyID, 1f);
                                }
                            }
                        }
                    }
                }



            }
        }

        public void addPatch(AltTreesPatch _patch, bool isNew = true)
        {
            if (_patch == null)
                return;

            bool isStop = false;
            int idNum = -1;
            if (isNew)
            {
                for (int i = 0; i < patches.Length; i++)
                {
                    if (patches[i] != null && patches[i].Equals(_patch))
                    {
                        isStop = true;
                        break;
                    }
                    else
                    {
                        if (patches[i] == null && idNum == -1)
                        {
                            idNum = i;
                        }
                    }
                }

                if (isStop)
                {
                    altTreesMain.LogError("Already contains AltTrees.");
                    return;
                }

                if (idNum == -1)
                {
                    AltTreesPatch[] altTreesTemp = patches;
                    patches = new AltTreesPatch[patches.Length + 1];

                    for (int i = 0; i < altTreesTemp.Length; i++)
                    {
                        patches[i] = altTreesTemp[i];
                    }
                    patches[patches.Length - 1] = _patch;


                    AltTreesQuad[] quadsTemp = quads;
                    quads = new AltTreesQuad[quads.Length + 1];

                    for (int i = 0; i < quadsTemp.Length; i++)
                    {
                        quads[i] = quadsTemp[i];
                    }

                    idNum = patches.Length - 1;
                }
                else
                {
                    patches[idNum] = _patch;
                }
            }
            else
            {
                for (int i = 0; i < patches.Length; i++)
                {
                    if (patches[i] != null && patches[i].Equals(_patch))
                    {
                        idNum = i;
                        break;
                    }
                }

                if (idNum == -1)
                {
                    altTreesMain.LogError("No contains AltTrees.");
                    return;
                }
            }

            _patch.altTreesId = idNum;
            
            createQuadTree(idNum);
            
            if (isNew)
            {
                if (_patch.prototypes != null)
                {
                    //int isUpdate = 0;
                    for (int i = 0; i < _patch.prototypes.Length; i++)
                    {
                        if (_patch.prototypes[i].tree != null)
                        {
                            _patch.prototypes[i].isEnabled = true;
                            if (_patch.prototypes[i].tree.textureBillboard == null || _patch.prototypes[i].tree.materialBillboard == null || _patch.prototypes[i].tree.materialBillboardGroup == null)
                            {
                                #if UNITY_EDITOR
                                {
                                    if ((Texture2D)AssetDatabase.LoadAssetAtPath("Assets/Plugins/AltSystems/AltTrees/DataBase/Trees/TreeResources/" + _patch.prototypes[i].tree.folderResources + "/Billboard/" + _patch.prototypes[i].tree.id + ".png", typeof(Texture2D)) == null || (Material)AssetDatabase.LoadAssetAtPath("Assets/Plugins/AltSystems/AltTrees/DataBase/Trees/TreeResources/" + _patch.prototypes[i].tree.folderResources + "/Billboard/" + _patch.prototypes[i].tree.id + ".mat", typeof(Material)) == null || (Material)AssetDatabase.LoadAssetAtPath("Assets/Plugins/AltSystems/AltTrees/DataBase/Trees/TreeResources/" + _patch.prototypes[i].tree.folderResources + "/Billboard/" + _patch.prototypes[i].tree.id + "_group.mat", typeof(Material)) == null)
                                    {
                                        _patch.prototypes[i].tree.getTextureBillboard(false);
                                    }
                                    else
                                    {
                                        _patch.prototypes[i].tree.textureBillboard = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/Plugins/AltSystems/AltTrees/DataBase/Trees/TreeResources/" + _patch.prototypes[i].tree.folderResources + "/Billboard/" + _patch.prototypes[i].tree.id + ".png", typeof(Texture2D));
                                        _patch.prototypes[i].tree.materialBillboard = (Material)AssetDatabase.LoadAssetAtPath("Assets/Plugins/AltSystems/AltTrees/DataBase/Trees/TreeResources/" + _patch.prototypes[i].tree.folderResources + "/Billboard/" + _patch.prototypes[i].tree.id + ".mat", typeof(Material));
                                        _patch.prototypes[i].tree.materialBillboardGroup = (Material)AssetDatabase.LoadAssetAtPath("Assets/Plugins/AltSystems/AltTrees/DataBase/Trees/TreeResources/" + _patch.prototypes[i].tree.folderResources + "/Billboard/" + _patch.prototypes[i].tree.id + "_group.mat", typeof(Material));
                                    }

                                    EditorUtility.SetDirty(_patch.prototypes[i].tree);

                                    //if (!_altTrees.prototypes[i].tree.generateTextureBillboardRuntime)
                                    //altTreesMain.LogError("No Billboard for " + _altTrees.prototypes[i].tree.name + ". Press Update Billboard button on tree!");
                                }
                                #else
                                {
									_patch.prototypes[i].tree.getTextureBillboard(true);
                                }
                                #endif
                            }


                            if (_patch.prototypes[i].tree.lods.Length != _patch.prototypes[i].tree.distancesSquares.Length + 1)
                                altTreesMain.LogError("distances.Length+1 != lods.Length. id" + _patch.prototypes[i].tree.id);

                            addInitObjPool(_patch.prototypes[i].tree);
                        }
                        else
                        {
                            //altTreesMain.LogError("_patch.prototypes[i].tree == null. ("+i+")");
                            _patch.prototypes[i].isEnabled = false;
                            //isUpdate++;
                        }
                    }
                    /*if(isUpdate > 0)
                    {
                        AltTreePrototypes[] prototypesTemp = _patch.prototypes;
                        _patch.prototypes = new AltTreePrototypes[prototypesTemp.Length - isUpdate];
                        int indx = 0;
                        for(int i = 0; i < prototypesTemp.Length; i++)
                        {
                            if(prototypesTemp[i].isEnabled)
                            {
                                _patch.prototypes[indx] = prototypesTemp[i];
                                _patch.prototypes[indx].isEnabled = true;
                                indx++;
                            }
                        }
                        #if UNITY_EDITOR
                            EditorUtility.SetDirty(_patch.altTreesManagerData);
                        #endif
                    }*/
                }
            }
            
            //List<int> treesForDelete = new List<int>();
            //List<AltTreesTrees> objectsForDelete = new List<AltTreesTrees>();
            Vector3 vector3Temp;

            List<int> idPrototypes = new List<int>();

            if (_patch.trees != null)
            {
                for (int i = 0; i < _patch.trees.Length; i++)
                {
                    if (_patch.trees[i] != null && _patch.trees[i].noNull)
                    {
                        _patch.trees[i].idPrototypeIndex = getPrototypeIndex(_patch.trees[i].idPrototype);

                        if (_patch.trees[i].idPrototypeIndex != -1)
                        {
                            _patch.trees[i].gpuInstancing = treesPoolArray[_patch.trees[i].idPrototypeIndex].tree.gpuInstancing;
                            _patch.trees[i].currentLOD = -1;
                            _patch.trees[i].currentCrossFadeId = -1;
                            vector3Temp = _patch.trees[i].getPosWorld();
                            if (!altTreesMain.altTreesManagerData.multiThreading)
                                quads[idNum].checkTreesAdd(vector3Temp.x, vector3Temp.z, _patch.trees[i], idNum);

                            _patch.trees[i].altTreesId = idNum;
                            lock (quads[idNum].treePrefabsCountLock)
                            {
                                if (!quads[idNum].treePrefabsCount.ContainsKey(_patch.trees[i].idPrototype))
                                    quads[idNum].treePrefabsCount.Add(_patch.trees[i].idPrototype, 0);
                                quads[idNum].treePrefabsCount[_patch.trees[i].idPrototype]++;
                            }
                            lock (quads[idNum].treesLock)
                            {
                                quads[idNum].treesCount++;
                                quads[idNum].trees.Add(_patch.trees[i]);
                            }

                        }
                        else
                        {
                            if (!idPrototypes.Contains(_patch.trees[i].idPrototype))
                                idPrototypes.Add(_patch.trees[i].idPrototype);
                            _patch.trees[i].noNull = false;
                            //treesForDelete.Add(i);
                        }
                    }
                }
                quads[idNum].isInitBillboards = false;
                quads[idNum].isRender = false;
                /*if (treesForDelete.Count > 0)
                {
                    _patch.EditDataFileTrees(null, 0, treesForDelete);
                }*/

                if (idPrototypes.Count > 0)
                {
                    for (int i = 0; i < idPrototypes.Count; i++)
                    {
                        altTreesMain.LogError("No find tree prototype " + idPrototypes[i]);
                    }
                }

                if (altTreesMain.altTreesManagerData.multiThreading)
                {
                    if (quads[idNum].isChildQuads)
                        ThreadPool.QueueUserWorkItem(quads[idNum].checkTreesAddQuads, 0);
                        //quads[idNum].checkTreesAddQuads(0);
                }
                else
                    quads[idNum].checkTreesInitTrue();
            }
            /*if (_patch.treesNoGroup != null)
            {
                for (int i = 0; i < _patch.treesNoGroup.Length; i++)
                {
                    if (_patch.treesNoGroup[i] != null && _patch.treesNoGroup[i].noNull)
                    {
                        _patch.treesNoGroup[i].idPrototypeIndex = getPrototypeIndex(_patch.treesNoGroup[i].idPrototype);

                        if (_patch.treesNoGroup[i].idPrototypeIndex != -1)
                        {
                            _patch.treesNoGroup[i].currentLOD = -1;
                            _patch.treesNoGroup[i].currentCrossFadeId = -1;
                            vector3Temp = _patch.treesNoGroup[i].getPosWorld();
                            quads[idNum].checkTreesAdd(vector3Temp.x, vector3Temp.z, _patch.treesNoGroup[i], idNum, false);*/

                            /*_patch.treesNoGroup[i].altTreesId = idNum;
                            if (quads[idNum].bound.inBounds(vector3Temp.x, vector3Temp.z, quads[idNum].quadId))
                            {
                                if (quads[idNum].LOD == quads[idNum].maxLOD)
                                {
                                    quads[idNum].treesNoBillbCount++;
                                    quads[idNum].treesNoBillb.Add(_patch.treesNoGroup[i]);
                                }
                            }*/
                        /*}
                        else
                        {
                            _patch.treesNoGroup[i].noNull = false;
                            objectsForDelete.Add(_patch.treesNoGroup[i]);
                        }
                    }
                }
                if (objectsForDelete.Count > 0)
                {
                    _patch.EditDataFileObjects(null, objectsForDelete);
                }
            }*/
        }



        void createQuadTree(int id)
        {
            patches[id].objectsQuadIdTemp = 1;
            quads[id] = new AltTreesQuad((patches[id].stepX - jump.x) * altTreesMain.altTreesManagerData.sizePatch + jumpPos.x + (float)altTreesMain.altTreesManagerData.sizePatch / 2f, (patches[id].stepY - jump.z) * altTreesMain.altTreesManagerData.sizePatch + jumpPos.z + (float)altTreesMain.altTreesManagerData.sizePatch / 2f, altTreesMain.altTreesManagerData.sizePatch, id, 0, altTreesMain.altTreesManagerData.maxLOD, altTreesMain.altTreesManagerData.maxLOD - 1, this, -1);
        }

        AltTreeInstance altTreeInstanceTemp = null;
        Renderer rendererTemp;
        Material[] matsTemp;
        Material matTemp;

        public void addInitObjPool(AltTree go)
        {
            int indexTemp = getPrototypeIndex(go.id);

            if(go.textureBillboard != null)
                go.textureBillboard.mipMapBias = go.billboardsMipMapBias;
            if (go.normalMapBillboard != null)
                go.normalMapBillboard.mipMapBias = go.billboardsNormalMapMipMapBias;

            if (indexTemp == -1)
            {
                int[] treePrototypeIdsTemp = treePrototypeIds;
                treePrototypeIds = new int[treePrototypeIdsTemp.Length + 1];
                for (int i = 0; i < treePrototypeIdsTemp.Length; i++)
                    treePrototypeIds[i] = treePrototypeIdsTemp[i];
                treePrototypeIds[treePrototypeIds.Length - 1] = go.id;
                indexTemp = treePrototypeIds.Length - 1;
                

                AltTreesPool[] treesPoolArrayTemp = treesPoolArray;
                treesPoolArray = new AltTreesPool[treesPoolArray.Length + 1];
                for (int i = 0; i < treesPoolArrayTemp.Length; i++)
                    treesPoolArray[i] = treesPoolArrayTemp[i];
                treesPoolArray[indexTemp] = new AltTreesPool();

                treesPoolArray[indexTemp].tree = go;



                treesPoolArray[indexTemp].treesToRenderLength = 500;
                treesPoolArray[indexTemp].treesToRender = new TreesToRender[treesPoolArray[indexTemp].treesToRenderLength];
                treesPoolArray[indexTemp].treesToRenderCount = 0;
                treesPoolArray[indexTemp].treesToRenderDeletedCount = 0;

                for (int i =0; i < treesPoolArray[indexTemp].treesToRenderLength; i++)
                {
                    treesPoolArray[indexTemp].treesToRender[i] = new TreesToRender();
                }


                treesPoolArray[indexTemp].mesh = getPlaneBillboard();

                boundsTemp = treesPoolArray[indexTemp].mesh.bounds;
                boundsTemp.max += new Vector3(treesPoolArray[indexTemp].tree.size / 2f + treesPoolArray[indexTemp].tree.up, treesPoolArray[indexTemp].tree.size / 2f + treesPoolArray[indexTemp].tree.up, treesPoolArray[indexTemp].tree.size / 2f + treesPoolArray[indexTemp].tree.up);
                boundsTemp.min -= new Vector3(treesPoolArray[indexTemp].tree.size / 2f - treesPoolArray[indexTemp].tree.up, treesPoolArray[indexTemp].tree.size / 2f - treesPoolArray[indexTemp].tree.up, treesPoolArray[indexTemp].tree.size / 2f - treesPoolArray[indexTemp].tree.up);
                boundsTemp.max += new Vector3(treesPoolArray[indexTemp].tree.size * /*widthScale*/2f / 2f, treesPoolArray[indexTemp].tree.size * /*heightScale*/2f / 2f, treesPoolArray[indexTemp].tree.size * /*widthScale*/2f / 2f);
                boundsTemp.min -= new Vector3(treesPoolArray[indexTemp].tree.size * /*widthScale*/2f / 2f, treesPoolArray[indexTemp].tree.size * /*heightScale*/2f / 2f, treesPoolArray[indexTemp].tree.size * /*widthScale*/2f / 2f);
                treesPoolArray[indexTemp].mesh.bounds = boundsTemp;


                /*propBlock.Clear();
                if (!altTreesMain.altTreesManagerData.drawDebugBillboards)
                {
                    propBlock.SetFloat(AltTreesManager.Alpha_PropertyID, 1f);
                    propBlock.SetFloat("_Width", treesPoolArray[indexTemp].tree.size * widthScale / 2f);
                    propBlock.SetFloat("_Height", treesPoolArray[indexTemp].tree.size * heightScale / 2f);
                    propBlock.SetFloat("_Rotation", rotation);
                    propBlock.SetColor("_HueVariation", _color);
                }
                else
                {
                    colorTemp.r = Random.value;
                    colorTemp.g = Random.value;
                    colorTemp.b = Random.value;
                    colorTemp.a = 1f;
                    propBlock.SetFloat(AltTreesManager.Alpha_PropertyID, 1f);
                    propBlock.SetFloat("_Width", treesPoolArray[indexTemp].tree.size * widthScale / 2f);
                    propBlock.SetFloat("_Height", treesPoolArray[indexTemp].tree.size * heightScale / 2f);
                    propBlock.SetFloat("_Rotation", rotation);
                    propBlock.SetColor("_HueVariation", colorTemp);
                }
                objBillboardTemp.mr.SetPropertyBlock(propBlock);*/





                if (altTreesMain.isPlaying && altTreesMain.altTreesManagerData.enableColliders)
                {
                    if (go.colliders != null)
                    {
                        go.isColliders = true;

                        if (go.colliders.Equals(go.billboardColliders))
                            go.isCollidersEqual = true;
                    }

                    if (go.billboardColliders != null)
                        go.isBillboardColliders = true;


                    if (go.isColliders)
                    {
                        needInitColliders = true;
                        treesPoolArray[indexTemp].needInitCollidersCount = (go.isCollidersEqual ? altTreesMain.altTreesManagerData.initCollidersCountPool + altTreesMain.altTreesManagerData.initColliderBillboardsCountPool : altTreesMain.altTreesManagerData.initCollidersCountPool);
                    }
                    if (go.isBillboardColliders && !go.isCollidersEqual)
                    {
                        needInitColliders = true;
                        treesPoolArray[indexTemp].needInitBillboardCollidersCount = altTreesMain.altTreesManagerData.initColliderBillboardsCountPool;
                    }
                }

                treesPoolArray[indexTemp].objsArray = new objsArr[go.lods.Length];

                for (int i = 0; i < go.lods.Length; i++)
                {
                    treesPoolArray[indexTemp].objsArray[i] = new objsArr();
                    treesPoolArray[indexTemp].objsArray[i].attDraw = new AltTreesTrees[gpuInstancingCount];
                    treesPoolArray[indexTemp].objsArray[i].attDrawCrossFade = new AltTreesTrees[gpuInstancingCount];
                    rendererTemp = go.lods[i].GetComponent<MeshRenderer>();
                    treesPoolArray[indexTemp].objsArray[i].mesh = go.lods[i].GetComponent<MeshFilter>().sharedMesh;
                    
                    if (i == go.lods.Length - 1)
                    {
                        treesPoolArray[indexTemp].treeSize = Mathf.Max(treesPoolArray[indexTemp].objsArray[i].mesh.bounds.size.x, Mathf.Max(treesPoolArray[indexTemp].objsArray[i].mesh.bounds.size.y, treesPoolArray[indexTemp].objsArray[i].mesh.bounds.size.z));
                    }


                    if (rendererTemp.sharedMaterials.Length != treesPoolArray[indexTemp].objsArray[i].mesh.subMeshCount)
                        altTreesMain.LogError("Materials Count != subMeshes Count. Materials Count = " + rendererTemp.sharedMaterials.Length + ", subMeshes Count = " + treesPoolArray[indexTemp].objsArray[i].mesh.subMeshCount, go.lods[i].gameObject);

                    //treesPoolArray[indexTemp].objsArray[i].materialsMesh = new Material[rendererTemp.sharedMaterials.Length]
                    treesPoolArray[indexTemp].objsArray[i].materialsMesh = new Material[treesPoolArray[indexTemp].objsArray[i].mesh.subMeshCount];
                    for (int j = 0; j < treesPoolArray[indexTemp].objsArray[i].materialsMesh.Length; j++)
                    {
                        if (j >= rendererTemp.sharedMaterials.Length)
                            treesPoolArray[indexTemp].objsArray[i].materialsMesh[j] = treesPoolArray[indexTemp].objsArray[i].materialsMesh[rendererTemp.sharedMaterials.Length - 1];
                        else
                        {
                            treesPoolArray[indexTemp].objsArray[i].materialsMesh[j] = new Material(rendererTemp.sharedMaterials[j]);
                            
                            #if UNITY_5_6
                                if ((treesPoolArray[indexTemp].tree.gpuInstancing && !treesPoolArray[indexTemp].objsArray[i].materialsMesh[j].enableInstancing) || (!treesPoolArray[indexTemp].tree.gpuInstancing && treesPoolArray[indexTemp].objsArray[i].materialsMesh[j].enableInstancing))
                                {
                                    #if UNITY_EDITOR
                                    {
                                        rendererTemp.sharedMaterials[j].enableInstancing = true;
                                        EditorUtility.SetDirty(rendererTemp.sharedMaterials[j]);
                                    }
                                    #endif

                                    if (treesPoolArray[indexTemp].tree.gpuInstancing)
                                        treesPoolArray[indexTemp].objsArray[i].materialsMesh[j].enableInstancing = true;
                                    else
                                        treesPoolArray[indexTemp].objsArray[i].materialsMesh[j].enableInstancing = false;
                                }
                            #endif

                            treesPoolArray[indexTemp].objsArray[i].materialsMesh[j].shaderKeywords = rendererTemp.sharedMaterials[j].shaderKeywords;
                            treesPoolArray[indexTemp].objsArray[i].materialsMesh[j].hideFlags = HideFlags.HideAndDontSave;
                            if (treesPoolArray[indexTemp].objsArray[i].materialsMesh[j].HasProperty("_Cutoff"))
                                treesPoolArray[indexTemp].objsArray[i].materialsMesh[j].SetFloat("_Cutoff", treesPoolArray[indexTemp].objsArray[i].materialsMesh[j].GetFloat("_Cutoff"));

                            if (starStatWind && treesPoolArray[indexTemp].tree.windMode != 0 && ((treesPoolArray[indexTemp].tree.windMode == 2 && treesPoolArray[indexTemp].tree.windIntensity_TC != 0f) || (treesPoolArray[indexTemp].tree.windMode == 1 && treesPoolArray[indexTemp].tree.loadedConfig && treesPoolArray[indexTemp].tree.windIntensity_ST != 0f)))
                            {
                                treesPoolArray[indexTemp].objsArray[i].materialsMesh[j].EnableKeyword("ENABLE_ALTWIND");
                                treesPoolArray[indexTemp].tree.windStar = true;
                            }
                            else
                                treesPoolArray[indexTemp].objsArray[i].materialsMesh[j].DisableKeyword("ENABLE_ALTWIND");


                            if (treesPoolArray[indexTemp].tree.leavesMaterials != null)
                            {
                                for (int f = 0; f < treesPoolArray[indexTemp].tree.leavesMaterials.Length; f++)
                                {
                                    if (treesPoolArray[indexTemp].tree.leavesMaterials[f].Equals(rendererTemp.sharedMaterials[j]))
                                    {
                                        treesPoolArray[indexTemp].objsArray[i].materialsMesh[j].SetInt("_Type", 0);
                                        f = treesPoolArray[indexTemp].tree.leavesMaterials.Length;
                                    }
                                }
                            }
                            if (treesPoolArray[indexTemp].tree.barkMaterials != null)
                            {
                                for (int f = 0; f < treesPoolArray[indexTemp].tree.barkMaterials.Length; f++)
                                {
                                    if (treesPoolArray[indexTemp].tree.barkMaterials[f].Equals(rendererTemp.sharedMaterials[j]))
                                    {
                                        treesPoolArray[indexTemp].objsArray[i].materialsMesh[j].SetInt("_Type", 1);
                                        f = treesPoolArray[indexTemp].tree.barkMaterials.Length;
                                    }
                                }
                            }
                        }
                    }

                    //treesPoolArray[indexTemp].objsArray[i].materialsMeshCrossFade = new Material[rendererTemp.sharedMaterials.Length];
                    treesPoolArray[indexTemp].objsArray[i].materialsMeshCrossFade = new Material[treesPoolArray[indexTemp].objsArray[i].mesh.subMeshCount];
                    for (int j = 0; j < treesPoolArray[indexTemp].objsArray[i].materialsMeshCrossFade.Length; j++)
                    {
                        if (j >= rendererTemp.sharedMaterials.Length)
                            treesPoolArray[indexTemp].objsArray[i].materialsMeshCrossFade[j] = treesPoolArray[indexTemp].objsArray[i].materialsMesh[rendererTemp.sharedMaterials.Length - 1];
                        else
                        {
                            treesPoolArray[indexTemp].objsArray[i].materialsMeshCrossFade[j] = new Material(treesPoolArray[indexTemp].objsArray[i].materialsMesh[j]);
                            treesPoolArray[indexTemp].objsArray[i].materialsMeshCrossFade[j].shaderKeywords = treesPoolArray[indexTemp].objsArray[i].materialsMesh[j].shaderKeywords;
                            treesPoolArray[indexTemp].objsArray[i].materialsMeshCrossFade[j].hideFlags = HideFlags.HideAndDontSave;
                            treesPoolArray[indexTemp].objsArray[i].materialsMeshCrossFade[j].EnableKeyword("CROSSFADE");
                            if (starStatWind && treesPoolArray[indexTemp].tree.windMode != 0 && ((treesPoolArray[indexTemp].tree.windMode == 2 && treesPoolArray[indexTemp].tree.windIntensity_TC != 0f) || (treesPoolArray[indexTemp].tree.windMode == 1 && treesPoolArray[indexTemp].tree.loadedConfig && treesPoolArray[indexTemp].tree.windIntensity_ST != 0f)))
                            {
                                treesPoolArray[indexTemp].objsArray[i].materialsMeshCrossFade[j].EnableKeyword("ENABLE_ALTWIND");
                                treesPoolArray[indexTemp].tree.windStar = true;
                            }
                            else
                                treesPoolArray[indexTemp].objsArray[i].materialsMeshCrossFade[j].DisableKeyword("ENABLE_ALTWIND");
                        }
                    }
                    rendererTemp = null;
                }

                if (treesPoolArray[indexTemp].tree.materialBillboard != null)
                {
                    treesPoolArray[indexTemp].materialBillboard = new Material(treesPoolArray[indexTemp].tree.materialBillboard);

                    #if UNITY_5_6
                        if (!treesPoolArray[indexTemp].materialBillboard.enableInstancing)
                        {
                            #if UNITY_EDITOR
                            {
                                treesPoolArray[indexTemp].tree.materialBillboard.enableInstancing = true;
                                EditorUtility.SetDirty(treesPoolArray[indexTemp].tree.materialBillboard);
                            }
                            #endif

                            treesPoolArray[indexTemp].materialBillboard.enableInstancing = true;
                        }
                    #endif

                    treesPoolArray[indexTemp].materialBillboard.shaderKeywords = treesPoolArray[indexTemp].tree.materialBillboard.shaderKeywords;
                    treesPoolArray[indexTemp].materialBillboard.hideFlags = HideFlags.DontSave;
                }
                else
                {
                    #if UNITY_EDITOR
                    {
                        treesPoolArray[indexTemp].tree.materialBillboard = (Material)AssetDatabase.LoadAssetAtPath("Assets/Plugins/AltSystems/AltTrees/DataBase/Trees/TreeResources/" + treesPoolArray[indexTemp].tree.folderResources + "/Billboard/" + treesPoolArray[indexTemp].tree.id + ".mat", typeof(Material));
                        treesPoolArray[indexTemp].materialBillboard = new Material(treesPoolArray[indexTemp].tree.materialBillboard);

                        #if UNITY_5_6
                            if (!treesPoolArray[indexTemp].materialBillboard.enableInstancing)
                            {
                                #if UNITY_EDITOR
                                {
                                    treesPoolArray[indexTemp].tree.materialBillboard.enableInstancing = true;
                                    EditorUtility.SetDirty(treesPoolArray[indexTemp].tree.materialBillboard);
                                }
                                #endif

                                treesPoolArray[indexTemp].materialBillboard.enableInstancing = true;
                            }
                        #endif

                        treesPoolArray[indexTemp].materialBillboard.shaderKeywords = ((Material)AssetDatabase.LoadAssetAtPath("Assets/Plugins/AltSystems/AltTrees/DataBase/Trees/TreeResources/" + treesPoolArray[indexTemp].tree.folderResources + "/Billboard/" + treesPoolArray[indexTemp].tree.id + ".mat", typeof(Material))).shaderKeywords;
                        treesPoolArray[indexTemp].materialBillboard.hideFlags = HideFlags.DontSave;
                    }
                    #endif
                }

                if (altTreesMain.altTreesManagerData.drawDebugBillboards)
                    treesPoolArray[indexTemp].materialBillboard.EnableKeyword("DEBUG_ON");

                treesPoolArray[indexTemp].materialBillboardCrossFade = new Material(treesPoolArray[indexTemp].materialBillboard);
                treesPoolArray[indexTemp].materialBillboardCrossFade.shaderKeywords = treesPoolArray[indexTemp].materialBillboard.shaderKeywords;
                treesPoolArray[indexTemp].materialBillboardCrossFade.EnableKeyword("CROSSFADE");
                treesPoolArray[indexTemp].materialBillboardCrossFade.hideFlags = HideFlags.DontSave;



                if (treesPoolArray[indexTemp].tree.materialBillboardGroup != null)
                {
                    treesPoolArray[indexTemp].materialBillboardGroup = new Material(treesPoolArray[indexTemp].tree.materialBillboardGroup);
                    treesPoolArray[indexTemp].materialBillboardGroup.shaderKeywords = treesPoolArray[indexTemp].tree.materialBillboardGroup.shaderKeywords;
                    treesPoolArray[indexTemp].materialBillboardGroup.hideFlags = HideFlags.DontSave;
                }
                else
                {
                    #if UNITY_EDITOR
                    {
                        if ((Material)AssetDatabase.LoadAssetAtPath("Assets/Plugins/AltSystems/AltTrees/DataBase/Trees/TreeResources/" + treesPoolArray[indexTemp].tree.folderResources + "/Billboard/" + treesPoolArray[indexTemp].tree.id + "_group.mat", typeof(Material)) == null)
                            treesPoolArray[indexTemp].tree.getTextureBillboard(false);

                        treesPoolArray[indexTemp].materialBillboardGroup = new Material((Material)AssetDatabase.LoadAssetAtPath("Assets/Plugins/AltSystems/AltTrees/DataBase/Trees/TreeResources/" + treesPoolArray[indexTemp].tree.folderResources + "/Billboard/" + treesPoolArray[indexTemp].tree.id + "_group.mat", typeof(Material)));
                        treesPoolArray[indexTemp].materialBillboardGroup.shaderKeywords = ((Material)AssetDatabase.LoadAssetAtPath("Assets/Plugins/AltSystems/AltTrees/DataBase/Trees/TreeResources/" + treesPoolArray[indexTemp].tree.folderResources + "/Billboard/" + treesPoolArray[indexTemp].tree.id + "_group.mat", typeof(Material))).shaderKeywords;
                        treesPoolArray[indexTemp].materialBillboardGroup.hideFlags = HideFlags.DontSave;
                    }
                    #endif
                }
                if (altTreesMain.altTreesManagerData.drawDebugBillboards)
                    treesPoolArray[indexTemp].materialBillboardGroup.EnableKeyword("DEBUG_ON");

                matTemp = null;
            }
        }
        
        public GameObject getObjMeshEditor(AltTreesTrees att, int lod)
        {
            if (att.idPrototypeIndex >= treesPoolArray.Length || att.idPrototypeIndex < 0 || lod >= treesPoolArray[att.idPrototypeIndex].tree.lods.Length || lod < 0)
            {
                altTreesMain.Log(att.idPrototypeIndex + ", " + treesPoolArray.Length + " = " + lod + ", " + treesPoolArray[att.idPrototypeIndex].tree.lods.Length);
            }
            goTemp = Instantiate(treesPoolArray[att.idPrototypeIndex].tree.lods[lod], vectTemp, Quaternion.identity) as GameObject;    //1
            altTreeInstanceTemp = goTemp.AddComponent<AltTreeInstance>();
            altTreeInstanceTemp.isObject = treesPoolArray[att.idPrototypeIndex].tree.isObject;
            altTreeInstanceTemp.manager = this;
            altTreeInstanceTemp.idTree = att.idTree;
            altTreeInstanceTemp.patchID = att.altTreesId;
            altTreeInstanceTemp.quadID = att.idQuadObject - 1;
            altTreeInstanceTemp.hueLeave = att.color;
            altTreeInstanceTemp.hueLeaveStar = att.color;
            altTreeInstanceTemp.hueBark = att.colorBark;
            altTreeInstanceTemp.hueBarkStar = att.colorBark;

            rendererTemp = goTemp.GetComponent<MeshRenderer>();
            rendererTemp.sharedMaterials = treesPoolArray[att.idPrototypeIndex].objsArray[lod].materialsMesh;

            propBlock.Clear();
            att.alphaPropBlockMesh = 1;
            att.indPropBlockMesh = 0;
            att.smoothPropBlock = 0;
            propBlock.SetFloat(Alpha_PropertyID, 1.0f);
            propBlock.SetFloat(Ind_PropertyID, 0.0f);
            propBlock.SetFloat(smoothValue_PropertyID, 0.0f);
            propBlock.SetColor(HueVariationLeave_PropertyID, att.color);
            propBlock.SetColor(HueVariationBark_PropertyID, att.colorBark);
            rendererTemp.SetPropertyBlock(propBlock);

            goTemp.transform.parent = transform;
            goTemp.hideFlags = HideFlags.DontSave;
            altTreeInstanceTemp = null;
            rendererTemp = null;

            scaleTemp.x = att.widthScale;
            scaleTemp.y = att.heightScale;
            scaleTemp.z = att.widthScale;
            goTemp.transform.localScale = scaleTemp;
            goTemp.transform.position = att.getPosWorld();
            goTemp.transform.rotation = Quaternion.AngleAxis(att.rotation, Vector3.up);
            goTemp.SetActive(true);
            treesList.Add(goTemp, att);

            return goTemp;
        }
        Vector3 scaleTemp = new Vector3();
        public void getObjMesh(AltTreesTrees _tree)
        {
            scaleTemp.x = _tree.widthScale;
            scaleTemp.y = _tree.heightScale;
            scaleTemp.z = _tree.widthScale;
            _tree.posWorldMesh = _tree.getPosWorld();
            _tree.posWorldBillboard = _tree.posWorldMesh + new Vector3(0f, treesPoolArray[_tree.idPrototypeIndex].tree.size * _tree.heightScale / 2f /*- (manager.treesPoolArray[treeTemp.idPrototypeIndex].tree.size * treeTemp.heightScale - manager.treesPoolArray[treeTemp.idPrototypeIndex].tree.size * treeTemp.heightScale / manager.treesPoolArray[treeTemp.idPrototypeIndex].tree.space) / 2f*/ + treesPoolArray[_tree.idPrototypeIndex].tree.up * _tree.heightScale, 0f);
            _tree.matrixMesh = Matrix4x4.TRS(_tree.posWorldMesh, Quaternion.AngleAxis(_tree.rotation, Vector3.up), scaleTemp);
            _tree.bound.center = _tree.posWorldBillboard;
            _tree.bound.size = Vector3.one * 5f * treesPoolArray[_tree.idPrototypeIndex].treeSize;

            if (!gpuInstancingSupport || !_tree.gpuInstancing)
            {
                _tree.propBlockMesh = new MaterialPropertyBlock();
                _tree.propBlockMesh.SetColor(AltTreesManager.HueVariationLeave_PropertyID, _tree.color);
                _tree.propBlockMesh.SetColor(AltTreesManager.HueVariationBark_PropertyID, _tree.colorBark);
            }

            _tree.isMesh = true;

            if (_tree.treesToRenderId == -1)
            {
                if (treesPoolArray[_tree.idPrototypeIndex].treesToRenderDeletedCount == 0)
                {
                    if (treesPoolArray[_tree.idPrototypeIndex].treesToRenderCount == treesPoolArray[_tree.idPrototypeIndex].treesToRenderLength)
                    {
                        int newSize = treesPoolArray[_tree.idPrototypeIndex].treesToRenderLength * 2;
                        TreesToRender[] ttrTemp = treesPoolArray[_tree.idPrototypeIndex].treesToRender;
                        treesPoolArray[_tree.idPrototypeIndex].treesToRender = new TreesToRender[newSize];
                        for (int i = 0; i < treesPoolArray[_tree.idPrototypeIndex].treesToRenderLength; i++)
                            treesPoolArray[_tree.idPrototypeIndex].treesToRender[i] = ttrTemp[i];
                        for (int i = treesPoolArray[_tree.idPrototypeIndex].treesToRenderLength; i < newSize; i++)
                            treesPoolArray[_tree.idPrototypeIndex].treesToRender[i] = new TreesToRender();
                        treesPoolArray[_tree.idPrototypeIndex].treesToRenderLength = newSize;

                        treesPoolArray[_tree.idPrototypeIndex].treesToRender[treesPoolArray[_tree.idPrototypeIndex].treesToRenderCount].att = _tree;
                        treesPoolArray[_tree.idPrototypeIndex].treesToRender[treesPoolArray[_tree.idPrototypeIndex].treesToRenderCount].noNull = true;

                        _tree.treesToRenderId = treesPoolArray[_tree.idPrototypeIndex].treesToRenderCount;

                        treesPoolArray[_tree.idPrototypeIndex].treesToRenderCount++;
                    }
                    else
                    {
                        treesPoolArray[_tree.idPrototypeIndex].treesToRender[treesPoolArray[_tree.idPrototypeIndex].treesToRenderCount].att = _tree;
                        treesPoolArray[_tree.idPrototypeIndex].treesToRender[treesPoolArray[_tree.idPrototypeIndex].treesToRenderCount].noNull = true;

                        _tree.treesToRenderId = treesPoolArray[_tree.idPrototypeIndex].treesToRenderCount;

                        treesPoolArray[_tree.idPrototypeIndex].treesToRenderCount++;
                    }
                }
                else
                {
                    intTemp = treesPoolArray[_tree.idPrototypeIndex].treesToRenderDeleted[0];
                    treesPoolArray[_tree.idPrototypeIndex].treesToRenderDeleted.RemoveAt(0);
                    treesPoolArray[_tree.idPrototypeIndex].treesToRenderDeletedCount--;

                    treesPoolArray[_tree.idPrototypeIndex].treesToRender[intTemp].att = _tree;
                    treesPoolArray[_tree.idPrototypeIndex].treesToRender[intTemp].noNull = true;

                    _tree.treesToRenderId = intTemp;
                }
            }
            else
            {
                treesPoolArray[_tree.idPrototypeIndex].treesToRender[_tree.treesToRenderId].att = _tree;
                treesPoolArray[_tree.idPrototypeIndex].treesToRender[_tree.treesToRenderId].noNull = true;
            }
        }

        //int collidersMaxCount = 0;
        //int colliderBillboardsMaxCount = 0;
        //(go.isCollidersEqual ? altTreesMain.altTreesManagerData.initCollidersCountPool + altTreesMain.altTreesManagerData.initColliderBillboardsCountPool : altTreesMain.altTreesManagerData.initCollidersCountPool)

        ColliderPool colliderPoolTemp = null;

        public ColliderPool getColliderPool(int idPrefab, bool isBillboardCollider, AltTreesTrees att)
        {
            if (!isBillboardCollider || treesPoolArray[att.idPrototypeIndex].tree.isCollidersEqual)
            {

                if (treesPoolArray[att.idPrototypeIndex].collidersArray.Count > 0)
                {
                    colliderPoolTemp = treesPoolArray[att.idPrototypeIndex].collidersArray[0];
                    treesPoolArray[att.idPrototypeIndex].collidersArray.RemoveAt(0);
                    if (colliderPoolTemp.go != null)
                    {
                        colliderPoolTemp.go.SetActive(true);
                        collidersUsedList.Add(att);
                        if (!isBillboardCollider)
                            drawCollidersCount++;
                        else
                            drawColliderBillboardsCount++;

                        treesPoolArray[att.idPrototypeIndex].collidersMaxCount = Mathf.Max(treesPoolArray[att.idPrototypeIndex].collidersMaxCount, drawCollidersCount);
                        treesPoolArray[att.idPrototypeIndex].colliderBillboardsMaxCount = Mathf.Max(treesPoolArray[att.idPrototypeIndex].colliderBillboardsMaxCount, drawColliderBillboardsCount);

                        if (altTreesMain.altTreesManagerData.colliderEvents)
                        {
                            for (int y = 0; y < colliderPoolTemp.colliders.Length; y++)
                            {
                                colliderPoolTemp.colliders[y].idTree = att.idTree;
                                colliderPoolTemp.colliders[y].isObject = att.isObject;
                                colliderPoolTemp.colliders[y].patch = att.altTreesPatch;
                                colliderPoolTemp.colliders[y].idPrototype = att.idPrototype;
                                if (att.isObject)
                                    colliderPoolTemp.colliders[y].quadID = att.idQuadObject;
                                else
                                    colliderPoolTemp.colliders[y].quadID = att.altTreesId;
                            }
                        }

                        return colliderPoolTemp;
                    }
                    else
                    {
                        altTreesMain.LogWarning("colliderPoolTemp.go==null. Instantiate Collider pool. [" + treesPoolArray[att.idPrototypeIndex].tree.name + "]");
                        colliderPoolTemp.go = Instantiate(treesPoolArray[att.idPrototypeIndex].tree.colliders, vectTemp, Quaternion.identity) as GameObject;
                        colliderPoolTemp.go.transform.parent = transform;
                        colliderPoolTemp.go.hideFlags = HideFlags.DontSave;
                        collidersUsedList.Add(att);
                        if (!isBillboardCollider)
                            drawCollidersCount++;
                        else
                            drawColliderBillboardsCount++;

                        if (treesPoolArray[att.idPrototypeIndex].needInitCollidersCount > 0)
                            treesPoolArray[att.idPrototypeIndex].needInitCollidersCount--;

                        treesPoolArray[att.idPrototypeIndex].collidersMaxCount = Mathf.Max(treesPoolArray[att.idPrototypeIndex].collidersMaxCount, drawCollidersCount);
                        treesPoolArray[att.idPrototypeIndex].colliderBillboardsMaxCount = Mathf.Max(treesPoolArray[att.idPrototypeIndex].colliderBillboardsMaxCount, drawColliderBillboardsCount);

                        if (altTreesMain.altTreesManagerData.colliderEvents)
                        {
                            for (int y = 0; y < colliderPoolTemp.colliders.Length; y++)
                            {
                                colliderPoolTemp.colliders[y].idTree = att.idTree;
                                colliderPoolTemp.colliders[y].isObject = att.isObject;
                                colliderPoolTemp.colliders[y].patch = att.altTreesPatch;
                                colliderPoolTemp.colliders[y].idPrototype = att.idPrototype;
                                if (att.isObject)
                                    colliderPoolTemp.colliders[y].quadID = att.idQuadObject;
                                else
                                    colliderPoolTemp.colliders[y].quadID = att.altTreesId;
                            }
                        }

                        return colliderPoolTemp;
                    }
                }
                else
                {
                    //altTreesMain.LogWarning("Instantiate Collider pool. [" + treesPoolArray[indexTemp].tree.name + "]");

                    colliderPoolTemp = new ColliderPool();
                    colliderPoolTemp.go = Instantiate(treesPoolArray[att.idPrototypeIndex].tree.colliders, vectTemp, Quaternion.identity) as GameObject;
                    colliderPoolTemp.go.transform.parent = transform;
                    colliderPoolTemp.go.hideFlags = HideFlags.DontSave;


                    Transform[] trTemp = colliderPoolTemp.go.GetComponentsInChildren<Transform>(true);
                    if (altTreesMain.altTreesManagerData.colliderEvents)
                    {
                        colliderPoolTemp.colliders = new AltCollider[trTemp.Length];
                        for (int y = 0; y < trTemp.Length; y++)
                        {
                            colliderPoolTemp.colliders[y] = trTemp[y].gameObject.AddComponent<AltCollider>();
                        }
                    }

                    collidersUsedList.Add(att);
                    if (!isBillboardCollider)
                        drawCollidersCount++;
                    else
                        drawColliderBillboardsCount++;

                    if (treesPoolArray[att.idPrototypeIndex].needInitCollidersCount > 0)
                        treesPoolArray[att.idPrototypeIndex].needInitCollidersCount--;

                    treesPoolArray[att.idPrototypeIndex].collidersMaxCount = Mathf.Max(treesPoolArray[att.idPrototypeIndex].collidersMaxCount, drawCollidersCount);
                    treesPoolArray[att.idPrototypeIndex].colliderBillboardsMaxCount = Mathf.Max(treesPoolArray[att.idPrototypeIndex].colliderBillboardsMaxCount, drawColliderBillboardsCount);

                    if (altTreesMain.altTreesManagerData.colliderEvents)
                    {
                        for (int y = 0; y < colliderPoolTemp.colliders.Length; y++)
                        {
                            colliderPoolTemp.colliders[y].idTree = att.idTree;
                            colliderPoolTemp.colliders[y].isObject = att.isObject;
                            colliderPoolTemp.colliders[y].patch = att.altTreesPatch;
                            colliderPoolTemp.colliders[y].idPrototype = att.idPrototype;
                            if (att.isObject)
                                colliderPoolTemp.colliders[y].quadID = att.idQuadObject;
                            else
                                colliderPoolTemp.colliders[y].quadID = att.altTreesId;
                        }
                    }

                    return colliderPoolTemp;
                }
            }
            else
            {
                if (treesPoolArray[att.idPrototypeIndex].colliderBillboardsArray.Count > 0)
                {
                    colliderPoolTemp = treesPoolArray[att.idPrototypeIndex].colliderBillboardsArray[0];
                    treesPoolArray[att.idPrototypeIndex].colliderBillboardsArray.RemoveAt(0);
                    if (colliderPoolTemp.go != null)
                    {
                        colliderPoolTemp.go.SetActive(true);
                        collidersUsedList.Add(att);
                        drawColliderBillboardsCount++;

                        treesPoolArray[att.idPrototypeIndex].collidersMaxCount = Mathf.Max(treesPoolArray[att.idPrototypeIndex].collidersMaxCount, drawCollidersCount);
                        treesPoolArray[att.idPrototypeIndex].colliderBillboardsMaxCount = Mathf.Max(treesPoolArray[att.idPrototypeIndex].colliderBillboardsMaxCount, drawColliderBillboardsCount);

                        if (altTreesMain.altTreesManagerData.colliderEvents)
                        {
                            for (int y = 0; y < colliderPoolTemp.colliders.Length; y++)
                            {
                                colliderPoolTemp.colliders[y].idTree = att.idTree;
                                colliderPoolTemp.colliders[y].isObject = att.isObject;
                                colliderPoolTemp.colliders[y].patch = att.altTreesPatch;
                                colliderPoolTemp.colliders[y].idPrototype = att.idPrototype;
                                if (att.isObject)
                                    colliderPoolTemp.colliders[y].quadID = att.idQuadObject;
                                else
                                    colliderPoolTemp.colliders[y].quadID = att.altTreesId;
                            }
                        }

                        return colliderPoolTemp;
                    }
                    else
                    {
                        altTreesMain.LogWarning("colliderPoolTemp.go==null. Instantiate BillboardCollider pool. [" + treesPoolArray[att.idPrototypeIndex].tree.name + "]");
                        colliderPoolTemp.go = Instantiate(treesPoolArray[att.idPrototypeIndex].tree.billboardColliders, vectTemp, Quaternion.identity) as GameObject;
                        colliderPoolTemp.go.transform.parent = transform;
                        colliderPoolTemp.go.hideFlags = HideFlags.DontSave;
                        collidersUsedList.Add(att);
                        drawColliderBillboardsCount++;

                        if (treesPoolArray[att.idPrototypeIndex].needInitBillboardCollidersCount > 0)
                            treesPoolArray[att.idPrototypeIndex].needInitBillboardCollidersCount--;

                        treesPoolArray[att.idPrototypeIndex].collidersMaxCount = Mathf.Max(treesPoolArray[att.idPrototypeIndex].collidersMaxCount, drawCollidersCount);
                        treesPoolArray[att.idPrototypeIndex].colliderBillboardsMaxCount = Mathf.Max(treesPoolArray[att.idPrototypeIndex].colliderBillboardsMaxCount, drawColliderBillboardsCount);

                        if (altTreesMain.altTreesManagerData.colliderEvents)
                        {
                            for (int y = 0; y < colliderPoolTemp.colliders.Length; y++)
                            {
                                colliderPoolTemp.colliders[y].idTree = att.idTree;
                                colliderPoolTemp.colliders[y].isObject = att.isObject;
                                colliderPoolTemp.colliders[y].patch = att.altTreesPatch;
                                colliderPoolTemp.colliders[y].idPrototype = att.idPrototype;
                                if (att.isObject)
                                    colliderPoolTemp.colliders[y].quadID = att.idQuadObject;
                                else
                                    colliderPoolTemp.colliders[y].quadID = att.altTreesId;
                            }
                        }

                        return colliderPoolTemp;
                    }
                }
                else
                {
                    //altTreesMain.LogWarning("Instantiate BillboardCollider pool. [" + treesPoolArray[indexTemp].tree.name + "]");

                    colliderPoolTemp = new ColliderPool();
                    colliderPoolTemp.go = Instantiate(treesPoolArray[att.idPrototypeIndex].tree.billboardColliders, vectTemp, Quaternion.identity) as GameObject;
                    colliderPoolTemp.go.transform.parent = transform;
                    colliderPoolTemp.go.hideFlags = HideFlags.DontSave;


                    Transform[] trTemp = colliderPoolTemp.go.GetComponentsInChildren<Transform>(true);
                    if (altTreesMain.altTreesManagerData.colliderEvents)
                    {
                        colliderPoolTemp.colliders = new AltCollider[trTemp.Length];
                        for (int y = 0; y < trTemp.Length; y++)
                        {
                            colliderPoolTemp.colliders[y] = trTemp[y].gameObject.AddComponent<AltCollider>();
                        }
                    }


                    collidersUsedList.Add(att);
                    drawColliderBillboardsCount++;

                    if (treesPoolArray[att.idPrototypeIndex].needInitBillboardCollidersCount > 0)
                        treesPoolArray[att.idPrototypeIndex].needInitBillboardCollidersCount--;

                    treesPoolArray[att.idPrototypeIndex].collidersMaxCount = Mathf.Max(treesPoolArray[att.idPrototypeIndex].collidersMaxCount, drawCollidersCount);
                    treesPoolArray[att.idPrototypeIndex].colliderBillboardsMaxCount = Mathf.Max(treesPoolArray[att.idPrototypeIndex].colliderBillboardsMaxCount, drawColliderBillboardsCount);

                    if (altTreesMain.altTreesManagerData.colliderEvents)
                    {
                        for (int y = 0; y < colliderPoolTemp.colliders.Length; y++)
                        {
                            colliderPoolTemp.colliders[y].idTree = att.idTree;
                            colliderPoolTemp.colliders[y].isObject = att.isObject;
                            colliderPoolTemp.colliders[y].patch = att.altTreesPatch;
                            colliderPoolTemp.colliders[y].idPrototype = att.idPrototype;
                            if (att.isObject)
                                colliderPoolTemp.colliders[y].quadID = att.idQuadObject;
                            else
                                colliderPoolTemp.colliders[y].quadID = att.altTreesId;
                        }
                    }

                    return colliderPoolTemp;
                }
            }
        }


        public void delColliderPool(int idPrefab, ColliderPool colliderPool, bool isBillboardCollider, AltTreesTrees att)
        {
            if (!isBillboardCollider || treesPoolArray[att.idPrototypeIndex].tree.isCollidersEqual)
            {
                if (treesPoolArray[att.idPrototypeIndex].collidersArray.Count > (treesPoolArray[att.idPrototypeIndex].tree.isCollidersEqual ? altTreesMain.altTreesManagerData.collidersPerOneMaxPool + altTreesMain.altTreesManagerData.colliderBillboardsPerOneMaxPool : altTreesMain.altTreesManagerData.collidersPerOneMaxPool))
                {
                    //altTreesMain.LogWarning("Destroy Collider pool. [" + treesPoolArray[indexTemp].tree.name + "]");

                    colliderPool.go.transform.position = vectTemp;
                    collidersUsedList.Remove(att);
                    if (!isBillboardCollider)
                        drawCollidersCount--;
                    else
                        drawColliderBillboardsCount--;
                    Destroy(colliderPool.go);
                }
                else
                {
                    colliderPool.go.transform.position = vectTemp;   //11
                    colliderPool.go.SetActive(false);
                    collidersUsedList.Remove(att);
                    if (!isBillboardCollider)
                        drawCollidersCount--;
                    else
                        drawColliderBillboardsCount--;
                    treesPoolArray[att.idPrototypeIndex].collidersArray.Add(colliderPool);
                }
            }
            else
            {
                if (treesPoolArray[att.idPrototypeIndex].colliderBillboardsArray.Count > altTreesMain.altTreesManagerData.colliderBillboardsPerOneMaxPool)
                {
                    //altTreesMain.LogWarning("Destroy BillboardCollider pool. [" + treesPoolArray[indexTemp].tree.name + "]");

                    colliderPool.go.transform.position = vectTemp;
                    collidersUsedList.Remove(att);
                    drawColliderBillboardsCount--;
                    Destroy(colliderPool.go);
                }
                else
                {
                    colliderPool.go.transform.position = vectTemp;
                    colliderPool.go.SetActive(false);
                    collidersUsedList.Remove(att);
                    drawColliderBillboardsCount--;
                    treesPoolArray[att.idPrototypeIndex].colliderBillboardsArray.Add(colliderPool);
                }
            }
        }
        

        int intTemp = 0;

        public void getObjBillboard(AltTreesTrees _tree/*, Vector3 pos*//*int idPrefab, float widthScale, float heightScale, float rotation, Color _color*/)
        {
            _tree.colorDebug.r = Random.value;
            _tree.colorDebug.g = Random.value;
            _tree.colorDebug.b = Random.value;
            _tree.colorDebug.a = 1f;

            _tree.posWorldBillboard = _tree.getPosWorld() + new Vector3(0f, treesPoolArray[_tree.idPrototypeIndex].tree.size * _tree.heightScale / 2f /*- (manager.treesPoolArray[treeTemp.idPrototypeIndex].tree.size * treeTemp.heightScale - manager.treesPoolArray[treeTemp.idPrototypeIndex].tree.size * treeTemp.heightScale / manager.treesPoolArray[treeTemp.idPrototypeIndex].tree.space) / 2f*/ + treesPoolArray[_tree.idPrototypeIndex].tree.up * _tree.heightScale, 0f);
            _tree.bound.center = _tree.posWorldBillboard;
            _tree.bound.size = Vector3.one * 5f * treesPoolArray[_tree.idPrototypeIndex].treeSize;

            _tree.matrixBillboard = Matrix4x4.TRS(_tree.posWorldBillboard, Quaternion.identity, Vector3.one);

            _tree.alphaPropBlockBillboard = 1f;
            _tree.widthPropBlock = treesPoolArray[_tree.idPrototypeIndex].tree.size * _tree.widthScale / 2f;
            _tree.heightPropBlock = treesPoolArray[_tree.idPrototypeIndex].tree.size * _tree.heightScale / 2f;
            _tree.huePropBlock = (!altTreesMain.altTreesManagerData.drawDebugBillboards) ? _tree.color : _tree.colorDebug;

            if (!gpuInstancingSupport)
            {
                _tree.propBlockBillboards = new MaterialPropertyBlock();

                _tree.propBlockBillboards.SetFloat(Alpha_PropertyID, 1f);
                _tree.propBlockBillboards.SetFloat(Width_PropertyID, _tree.widthPropBlock);
                _tree.propBlockBillboards.SetFloat(Height_PropertyID, _tree.heightPropBlock);
                _tree.propBlockBillboards.SetFloat(Rotation_PropertyID, _tree.rotation);
                _tree.propBlockBillboards.SetColor(HueVariation_PropertyID, _tree.huePropBlock);
            }


            _tree.isBillboard = true;

            if (_tree.treesToRenderId == -1)
            {
                if (treesPoolArray[_tree.idPrototypeIndex].treesToRenderDeletedCount == 0)
                {
                    if (treesPoolArray[_tree.idPrototypeIndex].treesToRenderCount == treesPoolArray[_tree.idPrototypeIndex].treesToRenderLength)
                    {
                        int newSize = treesPoolArray[_tree.idPrototypeIndex].treesToRenderLength * 2;
                        TreesToRender[] ttrTemp = treesPoolArray[_tree.idPrototypeIndex].treesToRender;
                        treesPoolArray[_tree.idPrototypeIndex].treesToRender = new TreesToRender[newSize];
                        for (int i = 0; i < treesPoolArray[_tree.idPrototypeIndex].treesToRenderLength; i++)
                            treesPoolArray[_tree.idPrototypeIndex].treesToRender[i] = ttrTemp[i];
                        for (int i = treesPoolArray[_tree.idPrototypeIndex].treesToRenderLength; i < newSize; i++)
                            treesPoolArray[_tree.idPrototypeIndex].treesToRender[i] = new TreesToRender();
                        treesPoolArray[_tree.idPrototypeIndex].treesToRenderLength = newSize;

                        treesPoolArray[_tree.idPrototypeIndex].treesToRender[treesPoolArray[_tree.idPrototypeIndex].treesToRenderCount].att = _tree;
                        treesPoolArray[_tree.idPrototypeIndex].treesToRender[treesPoolArray[_tree.idPrototypeIndex].treesToRenderCount].noNull = true;

                        _tree.treesToRenderId = treesPoolArray[_tree.idPrototypeIndex].treesToRenderCount;

                        treesPoolArray[_tree.idPrototypeIndex].treesToRenderCount++;
                    }
                    else
                    {
                        treesPoolArray[_tree.idPrototypeIndex].treesToRender[treesPoolArray[_tree.idPrototypeIndex].treesToRenderCount].att = _tree;
                        treesPoolArray[_tree.idPrototypeIndex].treesToRender[treesPoolArray[_tree.idPrototypeIndex].treesToRenderCount].noNull = true;

                        _tree.treesToRenderId = treesPoolArray[_tree.idPrototypeIndex].treesToRenderCount;

                        treesPoolArray[_tree.idPrototypeIndex].treesToRenderCount++;
                    }
                }
                else
                {
                    intTemp = treesPoolArray[_tree.idPrototypeIndex].treesToRenderDeleted[0];
                    treesPoolArray[_tree.idPrototypeIndex].treesToRenderDeleted.RemoveAt(0);
                    treesPoolArray[_tree.idPrototypeIndex].treesToRenderDeletedCount--;

                    treesPoolArray[_tree.idPrototypeIndex].treesToRender[intTemp].att = _tree;
                    treesPoolArray[_tree.idPrototypeIndex].treesToRender[intTemp].noNull = true;

                    _tree.treesToRenderId = intTemp;
                }
            }
            else
            {
                treesPoolArray[_tree.idPrototypeIndex].treesToRender[_tree.treesToRenderId].att = _tree;
                treesPoolArray[_tree.idPrototypeIndex].treesToRender[_tree.treesToRenderId].noNull = true;
            }
        }


        public void delObjMeshEditor(GameObject go)
        {
            treesList.Remove(go);

            go.transform.position = vectTemp;
            if (altTreesMain.isPlaying)
                Destroy(go);
            else
                DestroyImmediate(go);
        }
        public void delObjMesh(AltTreesTrees _tree)
        {
            if (!_tree.isBillboard)
            {
                treesPoolArray[_tree.idPrototypeIndex].treesToRenderDeletedCount++;
                treesPoolArray[_tree.idPrototypeIndex].treesToRenderDeleted.Add(_tree.treesToRenderId);
                treesPoolArray[_tree.idPrototypeIndex].treesToRender[_tree.treesToRenderId].att = null;
                treesPoolArray[_tree.idPrototypeIndex].treesToRender[_tree.treesToRenderId].noNull = false;
                _tree.treesToRenderId = -1;
                _tree.inFrustum = true;
            }

            if (!gpuInstancingSupport || !_tree.gpuInstancing)
            {
                _tree.propBlockMesh.Clear();
                _tree.propBlockMesh = null;
            }
            _tree.isCrossFadeMesh = false;
            _tree.isMesh = false;
        }



        public void delObjBillboard(AltTreesTrees _tree/*int idPrefab, GameObject go*/)
        {
            if (!_tree.isMesh)
            {
                treesPoolArray[_tree.idPrototypeIndex].treesToRenderDeletedCount++;
                treesPoolArray[_tree.idPrototypeIndex].treesToRenderDeleted.Add(_tree.treesToRenderId);
                treesPoolArray[_tree.idPrototypeIndex].treesToRender[_tree.treesToRenderId].att = null;
                treesPoolArray[_tree.idPrototypeIndex].treesToRender[_tree.treesToRenderId].noNull = false;
                _tree.treesToRenderId = -1;
                _tree.inFrustum = true;
            }

            if (!gpuInstancingSupport)
            {
                _tree.propBlockBillboards.Clear();
                _tree.propBlockBillboards = null;
            }
            _tree.isCrossFadeBillboard = false;
            _tree.isBillboard = false;
        }
        Mesh getPlaneBillboard()
        {
            Mesh ms = new Mesh();

            Vector3[] verts = new Vector3[4];
            Vector2[] uvs = new Vector2[4];
            Vector2[] uvs2 = new Vector2[4];
            Vector2[] uvs3 = new Vector2[4];
            int[] indices = new int[6];

            Vector2 uvs_0 = new Vector2(0, 0);
            Vector2 uvs_1 = new Vector2(1f / 3f, 0);
            Vector2 uvs_2 = new Vector2(1f / 3f, 1f / 3f);
            Vector2 uvs_3 = new Vector2(0, 1f / 3f);

            verts[0] = Vector3.zero;
            verts[1] = Vector3.zero;
            verts[2] = Vector3.zero;
            verts[3] = Vector3.zero;


            uvs[0] = uvs_0;
            uvs[1] = uvs_1;
            uvs[2] = uvs_2;
            uvs[3] = uvs_3;


            uvs2_0.x = -1;
            uvs2_0.y = -1;
            uvs2[0] = uvs2_0;

            uvs2_1.x = 1;
            uvs2_1.y = -1;
            uvs2[1] = uvs2_1;

            uvs2_2.x = 1;
            uvs2_2.y = 1;
            uvs2[2] = uvs2_2;

            uvs2_3.x = -1;
            uvs2_3.y = 1;
            uvs2[3] = uvs2_3;

            uvs2_0.x = -1;
            uvs2_0.y = -1;
            uvs2[0] = uvs2_0;

            uvs2_1.x = 1;
            uvs2_1.y = -1;
            uvs2[1] = uvs2_1;

            uvs2_2.x = 1;
            uvs2_2.y = 1;
            uvs2[2] = uvs2_2;

            uvs2_3.x = -1;
            uvs2_3.y = 1;
            uvs2[3] = uvs2_3;


            indices[0] = 3;
            indices[1] = 2;
            indices[2] = 0;
            indices[3] = 2;
            indices[4] = 1;
            indices[5] = 0;




            ms.vertices = verts;

            ms.uv = uvs;
            ms.uv2 = uvs2;
            ms.uv3 = uvs3;

            ms.SetIndices(indices, MeshTopology.Triangles, 0);
            ms.hideFlags = HideFlags.HideAndDontSave;
            
            return ms;
        }



        void createMeshBillboardsStartThread(object obj)
        {
            #if UNITY_EDITOR
            try
            #endif
            {
                AltTreesQuad atq = obj as AltTreesQuad;

                lock (atq.billboardsGenerationLock)
                {
                    if (atq.isInitBillboards)
                        return;

                    if (atq.isRender)
                    {
                        if (atq.LOD <= atq.startBillboardsLOD)
                            atq.isRender = false;
                    }

                    int countProts = 0;
                    int countTreesTemp = 0;
                    Color colorTemp = new Color();

                    Vector2 uvs_0 = new Vector2(0, 0);
                    Vector2 uvs_1 = new Vector2(1f / 3f, 0);
                    Vector2 uvs_2 = new Vector2(1f / 3f, 1f / 3f);
                    Vector2 uvs_3 = new Vector2(0, 1f / 3f);

                    Vector2 uvs2_0 = new Vector2(-1, -1);
                    Vector2 uvs2_1 = new Vector2(1, -1);
                    Vector2 uvs2_2 = new Vector2(1, 1);
                    Vector2 uvs2_3 = new Vector2(-1, 1);

                    Vector2 uvs3Vect = new Vector2(0, 0);

                    AltTreesTrees att2 = null;
                    AltTree at2 = null;

                    for (int j = 0; j < patches[atq.patchID].prototypes.Length; j++)
                    {
                        lock (atq.treePrefabsCountLock)
                        {
                            if (patches[atq.patchID].prototypes[j].isEnabled && atq.treePrefabsCount.ContainsKey(patches[atq.patchID].prototypes[j].tree.id))
                            {
                                countTreesTemp = atq.treePrefabsCount[patches[atq.patchID].prototypes[j].tree.id];
                                if (countTreesTemp != 0)
                                {
                                    countTreesTemp = (int)System.Math.Ceiling(((double)countTreesTemp) / 16250d);
                                    countProts += countTreesTemp;
                                }
                            }
                        }
                    }

                    int countTemp2 = 0;

                    lock (atq.createMeshBillboardsStructsLock)
                    {
                        atq.createMeshBillboardsStructs = new CreateMeshBillboardsStruct[countProts];
                        atq.countMeshes = countProts;
                        if (countProts > 0)
                            atq.createMeshBillboardsStructsCurrentId = 0;
                        else
                            atq.createMeshBillboardsStructsCurrentId = -1;
                        countProts = 0;
                        for (int j = 0; j < patches[atq.patchID].prototypes.Length; j++)
                        {
                            lock (atq.treePrefabsCountLock)
                            {
                                if (patches[atq.patchID].prototypes[j].isEnabled && atq.treePrefabsCount.ContainsKey(patches[atq.patchID].prototypes[j].tree.id))
                                    countTreesTemp = atq.treePrefabsCount[patches[atq.patchID].prototypes[j].tree.id];
                                else
                                    countTreesTemp = -1;
                            }
                            if (countTreesTemp != -1)
                            {
                                int indexTemp = getPrototypeIndex(patches[atq.patchID].prototypes[j].tree.id);

                                if (countTreesTemp != 0)
                                {
                                    int countTemp = (countTreesTemp < 0) ? 0 : (countTreesTemp > 16250) ? 16250 : countTreesTemp;

                                    atq.createMeshBillboardsStructs[countProts].indexTemp = indexTemp;
                                    atq.createMeshBillboardsStructs[countProts].at = patches[atq.patchID].prototypes[j].tree;
                                    atq.createMeshBillboardsStructs[countProts].widthScaleMax = 1f;
                                    atq.createMeshBillboardsStructs[countProts].heightScaleMax = 1f;
                                    atq.createMeshBillboardsStructs[countProts].verts = new Vector3[countTemp * 4];
                                    atq.createMeshBillboardsStructs[countProts].uvs = new Vector2[countTemp * 4];
                                    atq.createMeshBillboardsStructs[countProts].uvs2 = new Vector2[countTemp * 4];
                                    atq.createMeshBillboardsStructs[countProts].uvs3 = new Vector2[countTemp * 4];
                                    atq.createMeshBillboardsStructs[countProts].cols = new Color[countTemp * 4];
                                    atq.createMeshBillboardsStructs[countProts].indices = new int[countTemp * 6];
                                    atq.createMeshBillboardsStructs[countProts].createMeshIdStep = 0;


                                    int iTemp = 0;
                                    int countT = 0;
                                    int treesCountTemp = -1;

                                    if (altTreesMain.altTreesManagerData.drawDebugBillboards)
                                    {
                                        colorTemp.r = (float)AltUtilities.getRandomDouble();
                                        colorTemp.g = (float)AltUtilities.getRandomDouble();
                                        colorTemp.b = (float)AltUtilities.getRandomDouble();
                                        colorTemp.a = 1f;
                                    }

                                    lock (atq.treesLock)
                                    {
                                        treesCountTemp = atq.treesCount;
                                    }

                                    for (int i2 = 0; i2 < treesCountTemp; i2++)
                                    {
                                        lock (atq.treesLock)
                                        {
                                            if (i2 < atq.trees.Count && atq.trees[i2] != null)
                                                att2 = patches[atq.patchID].trees[atq.trees[i2].idTree];   //! 
                                            else
                                            {
                                                att2 = null;
                                                altTreesMain.LogError("__att==null");
                                            }
                                        }

                                        at2 = patches[atq.patchID].prototypes[j].tree;

                                        if (att2 != null)
                                        {
                                            if (att2.idPrototype == patches[atq.patchID].prototypes[j].tree.id)
                                            {
                                                if (iTemp * 4 >= atq.createMeshBillboardsStructs[countProts].verts.Length)
                                                    altTreesMain.Log(iTemp + ", " + atq.createMeshBillboardsStructs[countProts].verts.Length + ", " + countTreesTemp);

                                                if (att2.widthScale > atq.createMeshBillboardsStructs[countProts].widthScaleMax)
                                                    atq.createMeshBillboardsStructs[countProts].widthScaleMax = att2.widthScale;
                                                if (att2.heightScale > atq.createMeshBillboardsStructs[countProts].heightScaleMax)
                                                    atq.createMeshBillboardsStructs[countProts].heightScaleMax = att2.heightScale;

                                                atq.createMeshBillboardsStructs[countProts].verts[iTemp * 4 + 0] = att2.getPosWorldBillboard();
                                                atq.createMeshBillboardsStructs[countProts].verts[iTemp * 4 + 0].y += at2.size * att2.heightScale / 2f + at2.up * att2.heightScale;
                                                atq.createMeshBillboardsStructs[countProts].verts[iTemp * 4 + 1] = atq.createMeshBillboardsStructs[countProts].verts[iTemp * 4 + 0];
                                                atq.createMeshBillboardsStructs[countProts].verts[iTemp * 4 + 2] = atq.createMeshBillboardsStructs[countProts].verts[iTemp * 4 + 0];
                                                atq.createMeshBillboardsStructs[countProts].verts[iTemp * 4 + 3] = atq.createMeshBillboardsStructs[countProts].verts[iTemp * 4 + 0];


                                                atq.createMeshBillboardsStructs[countProts].uvs[iTemp * 4 + 0] = uvs_0;
                                                atq.createMeshBillboardsStructs[countProts].uvs[iTemp * 4 + 1] = uvs_1;
                                                atq.createMeshBillboardsStructs[countProts].uvs[iTemp * 4 + 2] = uvs_2;
                                                atq.createMeshBillboardsStructs[countProts].uvs[iTemp * 4 + 3] = uvs_3;


                                                if (!altTreesMain.altTreesManagerData.drawDebugBillboards)
                                                {
                                                    atq.createMeshBillboardsStructs[countProts].cols[iTemp * 4 + 0] = att2.color;
                                                    atq.createMeshBillboardsStructs[countProts].cols[iTemp * 4 + 1] = att2.color;
                                                    atq.createMeshBillboardsStructs[countProts].cols[iTemp * 4 + 2] = att2.color;
                                                    atq.createMeshBillboardsStructs[countProts].cols[iTemp * 4 + 3] = att2.color;
                                                }
                                                else
                                                {
                                                    atq.createMeshBillboardsStructs[countProts].cols[iTemp * 4 + 0] = colorTemp;
                                                    atq.createMeshBillboardsStructs[countProts].cols[iTemp * 4 + 1] = colorTemp;
                                                    atq.createMeshBillboardsStructs[countProts].cols[iTemp * 4 + 2] = colorTemp;
                                                    atq.createMeshBillboardsStructs[countProts].cols[iTemp * 4 + 3] = colorTemp;
                                                }


                                                uvs2_0.x = -at2.size * att2.widthScale / 2f;
                                                uvs2_0.y = -at2.size * att2.heightScale / 2f;
                                                atq.createMeshBillboardsStructs[countProts].uvs2[iTemp * 4 + 0] = uvs2_0;

                                                uvs2_1.x = at2.size * att2.widthScale / 2f;
                                                uvs2_1.y = -at2.size * att2.heightScale / 2f;
                                                atq.createMeshBillboardsStructs[countProts].uvs2[iTemp * 4 + 1] = uvs2_1;

                                                uvs2_2.x = at2.size * att2.widthScale / 2f;
                                                uvs2_2.y = at2.size * att2.heightScale / 2f;
                                                atq.createMeshBillboardsStructs[countProts].uvs2[iTemp * 4 + 2] = uvs2_2;

                                                uvs2_3.x = -at2.size * att2.widthScale / 2f;
                                                uvs2_3.y = at2.size * att2.heightScale / 2f;
                                                atq.createMeshBillboardsStructs[countProts].uvs2[iTemp * 4 + 3] = uvs2_3;

                                                uvs3Vect.x = att2.rotation;

                                                atq.createMeshBillboardsStructs[countProts].uvs3[iTemp * 4 + 0] = uvs3Vect;
                                                atq.createMeshBillboardsStructs[countProts].uvs3[iTemp * 4 + 1] = uvs3Vect;
                                                atq.createMeshBillboardsStructs[countProts].uvs3[iTemp * 4 + 2] = uvs3Vect;
                                                atq.createMeshBillboardsStructs[countProts].uvs3[iTemp * 4 + 3] = uvs3Vect;


                                                atq.createMeshBillboardsStructs[countProts].indices[iTemp * 6 + 0] = iTemp * 4 + 3;
                                                atq.createMeshBillboardsStructs[countProts].indices[iTemp * 6 + 1] = iTemp * 4 + 2;
                                                atq.createMeshBillboardsStructs[countProts].indices[iTemp * 6 + 2] = iTemp * 4 + 0;
                                                atq.createMeshBillboardsStructs[countProts].indices[iTemp * 6 + 3] = iTemp * 4 + 2;
                                                atq.createMeshBillboardsStructs[countProts].indices[iTemp * 6 + 4] = iTemp * 4 + 1;
                                                atq.createMeshBillboardsStructs[countProts].indices[iTemp * 6 + 5] = iTemp * 4 + 0;

                                                iTemp++;
                                            }
                                        }
                                        else
                                            altTreesMain.LogError("att==null");


                                        if (iTemp >= countTemp && countTemp != countTreesTemp)
                                        {
                                            countTemp2 = countTreesTemp - countT * countTemp + countTemp;
                                            countTemp2 = (countTemp2 < 0) ? 0 : (countTemp2 > 16250) ? 16250 : countTemp2;

                                            //if (countTemp2 != 0)
                                            {
                                                countProts++;
                                                atq.createMeshBillboardsStructs[countProts].indexTemp = indexTemp;
                                                atq.createMeshBillboardsStructs[countProts].at = patches[atq.patchID].prototypes[j].tree;
                                                atq.createMeshBillboardsStructs[countProts].widthScaleMax = 1f;
                                                atq.createMeshBillboardsStructs[countProts].heightScaleMax = 1f;
                                                atq.createMeshBillboardsStructs[countProts].verts = new Vector3[countTemp2 * 4];
                                                atq.createMeshBillboardsStructs[countProts].uvs = new Vector2[countTemp2 * 4];
                                                atq.createMeshBillboardsStructs[countProts].uvs2 = new Vector2[countTemp2 * 4];
                                                atq.createMeshBillboardsStructs[countProts].uvs3 = new Vector2[countTemp2 * 4];
                                                atq.createMeshBillboardsStructs[countProts].cols = new Color[countTemp2 * 4];
                                                atq.createMeshBillboardsStructs[countProts].indices = new int[countTemp2 * 6];
                                                atq.createMeshBillboardsStructs[countProts].createMeshIdStep = 0;
                                            }

                                            iTemp = 0;

                                            if (altTreesMain.altTreesManagerData.drawDebugBillboards)
                                            {
                                                colorTemp.r = (float)AltUtilities.getRandomDouble();
                                                colorTemp.g = (float)AltUtilities.getRandomDouble();
                                                colorTemp.b = (float)AltUtilities.getRandomDouble();
                                                colorTemp.a = 1f;
                                            }

                                            countT++;
                                        }
                                    }
                                    countProts++;
                                }
                            }
                        }
                    }

                    atq.createMeshBillboardsOk = true;
                }
            }
            #if UNITY_EDITOR
            catch (System.Exception e)
            {
                Debug.LogError(e);
            }
            #endif
        }

        bool createMeshBillboardsThread(AltTreesQuad atq)
        {
            lock (atq.billboardsGenerationLock)
            {
                if (atq.isInitBillboards)
                    return false;
                if (initTimeStarted || atq.treesCount < 1500)
                {
                    atq.meshes.Clear();
                    lock (atq.createMeshBillboardsStructsLock)
                    {
                        for (int j = 0; j < atq.createMeshBillboardsStructs.Length; j++)
                        {
                            Mesh ms = new Mesh();


                            ms.name = "-mesh_" + j + "_" + atq.LOD;
                            ms.vertices = atq.createMeshBillboardsStructs[j].verts;

                            ms.uv = atq.createMeshBillboardsStructs[j].uvs;
                            ms.uv2 = atq.createMeshBillboardsStructs[j].uvs2;
                            ms.uv3 = atq.createMeshBillboardsStructs[j].uvs3;
                            ms.colors = atq.createMeshBillboardsStructs[j].cols;

                            ms.SetIndices(atq.createMeshBillboardsStructs[j].indices, MeshTopology.Triangles, 0);
                            //ms.RecalculateBounds();
                            Bounds bn2 = ms.bounds;
                            bn2.max += new Vector3(atq.createMeshBillboardsStructs[j].at.size * atq.createMeshBillboardsStructs[j].widthScaleMax / 2f, atq.createMeshBillboardsStructs[j].at.size * atq.createMeshBillboardsStructs[j].heightScaleMax / 2f + atq.createMeshBillboardsStructs[j].at.up * atq.createMeshBillboardsStructs[j].heightScaleMax, atq.createMeshBillboardsStructs[j].at.size * atq.createMeshBillboardsStructs[j].widthScaleMax / 2f);
                            bn2.min -= new Vector3(atq.createMeshBillboardsStructs[j].at.size * atq.createMeshBillboardsStructs[j].widthScaleMax / 2f, atq.createMeshBillboardsStructs[j].at.size * atq.createMeshBillboardsStructs[j].heightScaleMax / 2f + atq.createMeshBillboardsStructs[j].at.up * atq.createMeshBillboardsStructs[j].heightScaleMax, atq.createMeshBillboardsStructs[j].at.size * atq.createMeshBillboardsStructs[j].widthScaleMax / 2f);
                            ms.bounds = bn2;
                            ms.hideFlags = HideFlags.HideAndDontSave;

                            atq.meshes.Add(new MeshToRender(ms, atq.createMeshBillboardsStructs[j].indexTemp));

                        }
                        atq.createMeshBillboardsStructs = null;
                    }

                    GCCollect = true;
                    atq.isInitBillboards = true;
                    //atq.isGenerateAllBillboardsOnStart = false;

                    return true;
                }
                else
                {
                    if (atq.createMeshBillboardsStructsCurrentId == 0 && atq.createMeshBillboardsStructs[atq.createMeshBillboardsStructsCurrentId].createMeshIdStep == 0)
                        atq.meshesTemp.Clear();

                    lock (atq.createMeshBillboardsStructsLock)
                    {
                        if (atq.createMeshBillboardsStructsCurrentId != -1)
                        {
                            atq.createMeshBillboardsStructs[atq.createMeshBillboardsStructsCurrentId].createMeshIdStep++;

                            if (atq.createMeshBillboardsStructs[atq.createMeshBillboardsStructsCurrentId].createMeshIdStep == 1)
                            {
                                atq.createMeshBillboardsStructs[atq.createMeshBillboardsStructsCurrentId].ms = new Mesh();


                                atq.createMeshBillboardsStructs[atq.createMeshBillboardsStructsCurrentId].ms.name = "-mesh_" + atq.createMeshBillboardsStructsCurrentId + "_" + atq.LOD;
                                atq.createMeshBillboardsStructs[atq.createMeshBillboardsStructsCurrentId].ms.vertices = atq.createMeshBillboardsStructs[atq.createMeshBillboardsStructsCurrentId].verts;
                                return false;
                            }
                            if (atq.createMeshBillboardsStructs[atq.createMeshBillboardsStructsCurrentId].createMeshIdStep == 2)
                            {
                                atq.createMeshBillboardsStructs[atq.createMeshBillboardsStructsCurrentId].ms.uv = atq.createMeshBillboardsStructs[atq.createMeshBillboardsStructsCurrentId].uvs;
                                return false;
                            }
                            if (atq.createMeshBillboardsStructs[atq.createMeshBillboardsStructsCurrentId].createMeshIdStep == 3)
                            {
                                atq.createMeshBillboardsStructs[atq.createMeshBillboardsStructsCurrentId].ms.uv2 = atq.createMeshBillboardsStructs[atq.createMeshBillboardsStructsCurrentId].uvs2;
                                return false;
                            }
                            if (atq.createMeshBillboardsStructs[atq.createMeshBillboardsStructsCurrentId].createMeshIdStep == 4)
                            {
                                atq.createMeshBillboardsStructs[atq.createMeshBillboardsStructsCurrentId].ms.uv3 = atq.createMeshBillboardsStructs[atq.createMeshBillboardsStructsCurrentId].uvs3;
                                return false;
                            }
                            if (atq.createMeshBillboardsStructs[atq.createMeshBillboardsStructsCurrentId].createMeshIdStep == 5)
                            {
                                atq.createMeshBillboardsStructs[atq.createMeshBillboardsStructsCurrentId].ms.colors = atq.createMeshBillboardsStructs[atq.createMeshBillboardsStructsCurrentId].cols;
                                return false;
                            }

                            atq.createMeshBillboardsStructs[atq.createMeshBillboardsStructsCurrentId].ms.SetIndices(atq.createMeshBillboardsStructs[atq.createMeshBillboardsStructsCurrentId].indices, MeshTopology.Triangles, 0);
                            //ms.RecalculateBounds();
                            Bounds bn2 = atq.createMeshBillboardsStructs[atq.createMeshBillboardsStructsCurrentId].ms.bounds;
                            bn2.max += new Vector3(atq.createMeshBillboardsStructs[atq.createMeshBillboardsStructsCurrentId].at.size * atq.createMeshBillboardsStructs[atq.createMeshBillboardsStructsCurrentId].widthScaleMax / 2f, atq.createMeshBillboardsStructs[atq.createMeshBillboardsStructsCurrentId].at.size * atq.createMeshBillboardsStructs[atq.createMeshBillboardsStructsCurrentId].heightScaleMax / 2f + atq.createMeshBillboardsStructs[atq.createMeshBillboardsStructsCurrentId].at.up * atq.createMeshBillboardsStructs[atq.createMeshBillboardsStructsCurrentId].heightScaleMax, atq.createMeshBillboardsStructs[atq.createMeshBillboardsStructsCurrentId].at.size * atq.createMeshBillboardsStructs[atq.createMeshBillboardsStructsCurrentId].widthScaleMax / 2f);
                            bn2.min -= new Vector3(atq.createMeshBillboardsStructs[atq.createMeshBillboardsStructsCurrentId].at.size * atq.createMeshBillboardsStructs[atq.createMeshBillboardsStructsCurrentId].widthScaleMax / 2f, atq.createMeshBillboardsStructs[atq.createMeshBillboardsStructsCurrentId].at.size * atq.createMeshBillboardsStructs[atq.createMeshBillboardsStructsCurrentId].heightScaleMax / 2f + atq.createMeshBillboardsStructs[atq.createMeshBillboardsStructsCurrentId].at.up * atq.createMeshBillboardsStructs[atq.createMeshBillboardsStructsCurrentId].heightScaleMax, atq.createMeshBillboardsStructs[atq.createMeshBillboardsStructsCurrentId].at.size * atq.createMeshBillboardsStructs[atq.createMeshBillboardsStructsCurrentId].widthScaleMax / 2f);
                            atq.createMeshBillboardsStructs[atq.createMeshBillboardsStructsCurrentId].ms.bounds = bn2;
                            atq.createMeshBillboardsStructs[atq.createMeshBillboardsStructsCurrentId].ms.hideFlags = HideFlags.HideAndDontSave;

                            atq.meshesTemp.Add(new MeshToRender(atq.createMeshBillboardsStructs[atq.createMeshBillboardsStructsCurrentId].ms, atq.createMeshBillboardsStructs[atq.createMeshBillboardsStructsCurrentId].indexTemp));

                            atq.createMeshBillboardsStructsCurrentId++;
                            if (atq.createMeshBillboardsStructsCurrentId == atq.createMeshBillboardsStructs.Length)
                            {
                                atq.createMeshBillboardsStructsCurrentId = -1;

                                atq.createMeshBillboardsStructs = null;

                                GCCollect = true;
                                atq.isInitBillboards = true;
                                //atq.isGenerateAllBillboardsOnStart = false;

                                atq.meshes.Clear();
                                atq.meshes.AddRange(atq.meshesTemp);
                                atq.meshesTemp.Clear();

                                return true;
                            }
                            else
                                return false;
                        }
                        else
                        {
                            atq.meshes.Clear();
                            atq.meshesTemp.Clear();
                            atq.createMeshBillboardsStructsCurrentId = -1;

                            atq.createMeshBillboardsStructs = null;

                            GCCollect = true;
                            atq.isInitBillboards = true;
                            //atq.isGenerateAllBillboardsOnStart = false;

                            return true;
                        }
                    }
                }
            }
        }



        Vector2 uvs_0 = new Vector2(0, 0);
        Vector2 uvs_1 = new Vector2(1f / 3f, 0);
        Vector2 uvs_2 = new Vector2(1f / 3f, 1f / 3f);
        Vector2 uvs_3 = new Vector2(0, 1f / 3f);

        Vector2 uvs2_0 = new Vector2(-1, -1);
        Vector2 uvs2_1 = new Vector2(1, -1);
        Vector2 uvs2_2 = new Vector2(1, 1);
        Vector2 uvs2_3 = new Vector2(-1, 1);

        Vector2 uvs3Vect = new Vector2(0, 0);

        AltTreesTrees att = null;
        AltTree at = null;

        public void createMeshBillboards(AltTreesQuad atq)
        {
            List<MeshToRender> meshesTemp = new List<MeshToRender>();

            /*if (atq.isRender)
            {
                if (atq.LOD <= atq.startBillboardsLOD)
                    atq.isRender = false;
            }*/

            for (int j = 0; j < patches[atq.patchID].prototypes.Length; j++)
            {
                int countTreesTemp = 0;
                lock (atq.treePrefabsCountLock)
                {
                    if (patches[atq.patchID].prototypes[j].isEnabled && atq.treePrefabsCount.ContainsKey(patches[atq.patchID].prototypes[j].tree.id))
                        countTreesTemp = atq.treePrefabsCount[patches[atq.patchID].prototypes[j].tree.id];
                    else
                        countTreesTemp = 0;
                }
                if (countTreesTemp != 0)
                {
                    int indexTemp = getPrototypeIndex(patches[atq.patchID].prototypes[j].tree.id);

                    Mesh ms = new Mesh();

                    int countTemp = Mathf.Clamp(countTreesTemp, 0, 16250);

                    Vector3[] verts = new Vector3[countTemp * 4];
                    Vector2[] uvs = new Vector2[countTemp * 4];
                    Vector2[] uvs2 = new Vector2[countTemp * 4];
                    Vector2[] uvs3 = new Vector2[countTemp * 4];
                    Color[] cols = new Color[countTemp * 4];
                    int[] indices = new int[countTemp * 6];

                    int iTemp = 0;
                    int countT = 0;
                    int treesCountTemp = -1;


                    if (altTreesMain.altTreesManagerData.drawDebugBillboards)
                    {
                        colorTemp.r = Random.value;
                        colorTemp.g = Random.value;
                        colorTemp.b = Random.value;
                        colorTemp.a = 1f;
                    }

                    lock (atq.treesLock)
                    {
                        treesCountTemp = atq.treesCount;
                    }

                    for (int i2 = 0; i2 < treesCountTemp; i2++)
                    {
                        lock (atq.treesLock)
                        {
                            if (i2 < atq.trees.Count && atq.trees[i2] != null)
                                att = patches[atq.patchID].trees[atq.trees[i2].idTree];   //! 
                            else
                            {
                                att = null;
                                altTreesMain.LogError("__att==null");
                            }
                        }
                        
                        at = patches[atq.patchID].prototypes[j].tree;

                        if (att != null)
                        {
                            if (att.idPrototype == patches[atq.patchID].prototypes[j].tree.id)
                            {
                                if (iTemp * 4 >= verts.Length)
                                    altTreesMain.Log(iTemp + ", " + verts.Length + ", " + countTreesTemp);

                                verts[iTemp * 4 + 0] = att.getPosWorldBillboard();
                                verts[iTemp * 4 + 0].y += at.size * att.heightScale / 2f + at.up * att.heightScale;
                                verts[iTemp * 4 + 1] = verts[iTemp * 4 + 0];
                                verts[iTemp * 4 + 2] = verts[iTemp * 4 + 0];
                                verts[iTemp * 4 + 3] = verts[iTemp * 4 + 0];


                                uvs[iTemp * 4 + 0] = uvs_0;
                                uvs[iTemp * 4 + 1] = uvs_1;
                                uvs[iTemp * 4 + 2] = uvs_2;
                                uvs[iTemp * 4 + 3] = uvs_3;


                                if (!altTreesMain.altTreesManagerData.drawDebugBillboards)
                                {
                                    cols[iTemp * 4 + 0] = att.color;
                                    cols[iTemp * 4 + 1] = att.color;
                                    cols[iTemp * 4 + 2] = att.color;
                                    cols[iTemp * 4 + 3] = att.color;
                                }
                                else
                                {
                                    cols[iTemp * 4 + 0] = colorTemp;
                                    cols[iTemp * 4 + 1] = colorTemp;
                                    cols[iTemp * 4 + 2] = colorTemp;
                                    cols[iTemp * 4 + 3] = colorTemp;
                                }


                                uvs2_0.x = -at.size * att.widthScale / 2f;
                                uvs2_0.y = -at.size * att.heightScale / 2f;
                                uvs2[iTemp * 4 + 0] = uvs2_0;

                                uvs2_1.x = at.size * att.widthScale / 2f;
                                uvs2_1.y = -at.size * att.heightScale / 2f;
                                uvs2[iTemp * 4 + 1] = uvs2_1;

                                uvs2_2.x = at.size * att.widthScale / 2f;
                                uvs2_2.y = at.size * att.heightScale / 2f;
                                uvs2[iTemp * 4 + 2] = uvs2_2;

                                uvs2_3.x = -at.size * att.widthScale / 2f;
                                uvs2_3.y = at.size * att.heightScale / 2f;
                                uvs2[iTemp * 4 + 3] = uvs2_3;

                                uvs3Vect.x = att.rotation;

                                uvs3[iTemp * 4 + 0] = uvs3Vect;
                                uvs3[iTemp * 4 + 1] = uvs3Vect;
                                uvs3[iTemp * 4 + 2] = uvs3Vect;
                                uvs3[iTemp * 4 + 3] = uvs3Vect;


                                indices[iTemp * 6 + 0] = iTemp * 4 + 3;
                                indices[iTemp * 6 + 1] = iTemp * 4 + 2;
                                indices[iTemp * 6 + 2] = iTemp * 4 + 0;
                                indices[iTemp * 6 + 3] = iTemp * 4 + 2;
                                indices[iTemp * 6 + 4] = iTemp * 4 + 1;
                                indices[iTemp * 6 + 5] = iTemp * 4 + 0;

                                iTemp++;
                            }
                        }
                        else
                            altTreesMain.LogError("att==null");


                        if (iTemp >= countTemp)
                        {
                            ms.name = "-mesh_" + j + "_" + atq.LOD;
                            ms.vertices = verts;

                            ms.uv = uvs;
                            ms.uv2 = uvs2;
                            ms.uv3 = uvs3;
                            ms.colors = cols;

                            ms.SetIndices(indices, MeshTopology.Triangles, 0);
                            //ms.RecalculateBounds();
                            Bounds bn2 = ms.bounds;
                            bn2.max += new Vector3(at.size * att.widthScale / 2f, at.size * att.heightScale / 2f + at.up * att.heightScale, at.size * att.widthScale / 2f);
                            bn2.min -= new Vector3(at.size * att.widthScale / 2f, at.size * att.heightScale / 2f + at.up * att.heightScale, at.size * att.widthScale / 2f);
                            ms.bounds = bn2;
                            ms.hideFlags = HideFlags.HideAndDontSave;
                            
                            meshesTemp.Add(new MeshToRender(ms, indexTemp));


                            countTemp = Mathf.Clamp(countTreesTemp - countT * countTemp + countTemp, 0, 16250);
                            
                            if (countTemp != 0)
                            {
                                ms = new Mesh();

                                verts = new Vector3[countTemp * 4];
                                uvs = new Vector2[countTemp * 4];
                                uvs2 = new Vector2[countTemp * 4];
                                uvs3 = new Vector2[countTemp * 4];
                                cols = new Color[countTemp * 4];
                                indices = new int[countTemp * 6];


                                if (altTreesMain.altTreesManagerData.drawDebugBillboards)
                                {
                                    colorTemp.r = Random.value;
                                    colorTemp.g = Random.value;
                                    colorTemp.b = Random.value;
                                    colorTemp.a = 1f;
                                }
                            }
                            iTemp = 0;
                            countT++;
                        }
                    }

                    if (iTemp != 0)
                    {
                        ms.name = "+mesh_" + j + "_" + atq.LOD;
                        ms.vertices = verts;

                        ms.uv = uvs;
                        ms.uv2 = uvs2;
                        ms.uv3 = uvs3;
                        ms.colors = cols;

                        ms.SetIndices(indices, MeshTopology.Triangles, 0);
                        //ms.RecalculateBounds();
                        Bounds bn = ms.bounds;
                        bn.max += new Vector3(at.size * att.widthScale / 2f, at.size * att.heightScale / 2f + at.up * att.heightScale, at.size * att.widthScale / 2f);
                        bn.min -= new Vector3(at.size * att.widthScale / 2f, at.size * att.heightScale / 2f + at.up * att.heightScale, at.size * att.widthScale / 2f);
                        ms.bounds = bn;
                        ms.hideFlags = HideFlags.HideAndDontSave;
                        
                        meshesTemp.Add(new MeshToRender(ms, indexTemp));
                    }
                }
            }
            atq.meshes.Clear();

            for (int i = 0; i < meshesTemp.Count; i++)
            {
                atq.meshes.Add(meshesTemp[i]);
            }

            meshesTemp.Clear();

            GCCollect = true;

            atq.isInitBillboards = true;
            //atq.isGenerateAllBillboardsOnStart = false;

        }
    

        public void addDistanceCamera(Transform cam)
        {
            addDistanceCamera(cam, null, null);
        }

        public void addDistanceCamera(Transform cam, AtiTemp att, AltTreeInstance ati)
        {
            if (cam != null)
            {
                lock (distanceCamerasLock)
                {
                    if (distanceCamerasInit)
                    {
                        for (int c = 0; c < distanceCameras.Length; c++)
                        {
                            if (distanceCameras[c].trans.Equals(cam))
                                return;
                        }
                    }
                    
                    camerasTemp = distanceCameras;
                    distanceCameras = new DistanceCamera[(distanceCamerasInit ? distanceCameras.Length + 1 : 1)];
                    if (distanceCamerasInit)
                    {
                        for (int c = 0; c < camerasTemp.Length; c++)
                        {
                            distanceCameras[c] = camerasTemp[c];
                        }
                    }
                    distanceCameras[distanceCameras.Length - 1].trans = cam;

                    if (att != null && ati != null)
                    {
                        distanceCameras[distanceCameras.Length - 1].isSelected = true;
                        distanceCameras[distanceCameras.Length - 1].ati = ati;
                    }

                    treesCamerasTemp = treesCameras;
                    treesCameras = new AtiTemp[treesCameras.Length + 1];
                    for (int c = 0; c < treesCamerasTemp.Length; c++)
                    {
                        treesCameras[c] = treesCamerasTemp[c];
                    }
                    treesCameras[treesCamerasTemp.Length] = att;

                    distanceCamerasInit = true;
                }
            }
        }

        public void removeDistanceCamera(Transform cam)
        {
            lock (distanceCamerasLock)
            {
                if (cam != null && distanceCameras.Length > 0)
                {
                    for (int c = 0; c < distanceCameras.Length; c++)
                    {
                        if (distanceCameras[c].trans.Equals(cam))
                        {
                            int schet = 0;
                            camerasTemp = distanceCameras;
                            distanceCameras = new DistanceCamera[distanceCameras.Length - 1];
                            treesCamerasTemp = treesCameras;
                            treesCameras = new AtiTemp[treesCameras.Length - 1];
                            for (int p = 0; p < camerasTemp.Length; p++)
                            {
                                if (!camerasTemp[p].trans.Equals(cam))
                                {
                                    distanceCameras[schet] = camerasTemp[p];
                                    treesCameras[schet] = treesCamerasTemp[p];
                                    schet++;
                                }
                            }

                            if (distanceCameras.Length == 0)
                                distanceCamerasInit = false;

                            return;
                        }
                    }
                }
            }
        }

        void removeDistanceCamera(int camID)
        {
            lock (distanceCamerasLock)
            {
                if (distanceCameras.Length >= camID)
                {
                    int schet = 0;
                    camerasTemp = distanceCameras;
                    distanceCameras = new DistanceCamera[distanceCameras.Length - 1];
                    treesCamerasTemp = treesCameras;
                    treesCameras = new AtiTemp[treesCameras.Length - 1];
                    for (int p = 0; p < camerasTemp.Length; p++)
                    {
                        if (p != camID)
                        {
                            distanceCameras[schet] = camerasTemp[p];
                            treesCameras[schet] = treesCamerasTemp[p];
                            schet++;
                        }
                    }

                    if (distanceCameras.Length == 0)
                        distanceCamerasInit = false;
                }
            }
        }

        public Transform[] getDistanceCamerasList()
        {
            lock (distanceCamerasLock)
            {
                Transform[] cams = new Transform[distanceCameras.Length];
                for (int c = 0; c < distanceCameras.Length; c++)
                {
                    cams[c] = distanceCameras[c].trans;
                }
                return cams;
            }
        }

        public void addFrustumCullingCamera(Camera camera)
        {
            if (camera != null)
            {
                lock (frustumCamerasLock)
                {
                    if (frustumCamerasInit)
                    {
                        for (int c = 0; c < frustumCameras.Length; c++)
                        {
                            if (frustumCameras[c].cam.Equals(camera))
                                return;
                        }
                    }

                    FrustumCamera[] frustumCamerasTemp = frustumCameras;
                    frustumCameras = new FrustumCamera[(frustumCamerasInit ? frustumCameras.Length + 1 : 1)];
                    if (frustumCamerasInit)
                    {
                        for (int c = 0; c < frustumCamerasTemp.Length; c++)
                        {
                            frustumCameras[c] = frustumCamerasTemp[c];
                        }
                    }
                    frustumCameras[frustumCameras.Length - 1].cam = camera;
                    frustumCameras[frustumCameras.Length - 1].planes = new Plane[6];
                    frustumCameras[frustumCameras.Length - 1].myPlanes = new MyPlane[6];
                    frustumCameras[frustumCameras.Length - 1].myPlanes[0] = new MyPlane();
                    frustumCameras[frustumCameras.Length - 1].myPlanes[1] = new MyPlane();
                    frustumCameras[frustumCameras.Length - 1].myPlanes[2] = new MyPlane();
                    frustumCameras[frustumCameras.Length - 1].myPlanes[3] = new MyPlane();
                    frustumCameras[frustumCameras.Length - 1].myPlanes[4] = new MyPlane();
                    frustumCameras[frustumCameras.Length - 1].myPlanes[5] = new MyPlane();

                    frustumCamerasInit = true;
                }
            }
        }

        public void removeFrustumCullingCamera(Camera camera)
        {
            lock (frustumCamerasLock)
            {
                if (camera != null && frustumCameras.Length > 0)
                {
                    for (int c = 0; c < frustumCameras.Length; c++)
                    {
                        if (frustumCameras[c].cam.Equals(camera))
                        {
                            int schet = 0;
                            FrustumCamera[] frustumCamerasTemp = frustumCameras;
                            frustumCameras = new FrustumCamera[frustumCameras.Length - 1];
                            for (int p = 0; p < frustumCamerasTemp.Length; p++)
                            {
                                if (!frustumCamerasTemp[p].cam.Equals(camera))
                                {
                                    if(schet >= frustumCameras.Length || p >= frustumCamerasTemp.Length)
                                        altTreesMain.Log(frustumCameras.Length + ", " + schet + ", " + frustumCamerasTemp.Length + ", " + p);
                                    frustumCameras[schet] = frustumCamerasTemp[p];
                                    schet++;
                                }
                            }

                            if (frustumCameras.Length == 0)
                                frustumCamerasInit = false;

                            return;
                        }
                    }
                }
            }
        }

        public void removeFrustumCullingCamera(int camID)
        {
            lock (frustumCamerasLock)
            {
                if (frustumCameras.Length >= camID)
                {
                    int schet = 0;
                    FrustumCamera[] frustumCamerasTemp = frustumCameras;
                    frustumCameras = new FrustumCamera[frustumCameras.Length - 1];
                    for (int p = 0; p < frustumCamerasTemp.Length; p++)
                    {
                        if (p != camID)
                        {
                            frustumCameras[schet] = frustumCamerasTemp[p];
                            schet++;
                        }
                    }

                    if (frustumCameras.Length == 0)
                        frustumCamerasInit = false;
                }
            }
        }

        public Camera[] getFrustumCullingCamerasList()
        {
            lock (frustumCamerasLock)
            {
                Camera[] cams = new Camera[frustumCameras.Length];
                for (int c = 0; c < frustumCameras.Length; c++)
                {
                    cams[c] = frustumCameras[c].cam;
                }
                return cams;
            }
        }

        public bool isSelectionTree = false;

        public void setSelectionTrees(Transform[] trs)
        {
            isSelectionTree = true;
            AltTreesTrees attTemp = null;

            for (int i = 0; i < trs.Length; i++)
            {
                altTreeInstanceTemp = trs[i].GetComponent<AltTreeInstance>();
                if (altTreeInstanceTemp != null)
                {
                    if (!altTreeInstanceTemp.isObject)
                    {
                        if (patches[altTreeInstanceTemp.patchID].trees[altTreeInstanceTemp.idTree] != null)
                        {
                            if (!patches[altTreeInstanceTemp.patchID].trees[altTreeInstanceTemp.idTree].goMesh.Equals(trs[i].gameObject))
                            {
                                altTreeInstanceTemp.idTree = copyTree(altTreeInstanceTemp.patchID, altTreeInstanceTemp.idTree, trs[i].gameObject, true, altTreeInstanceTemp.quadID).idTree;
                                quads[altTreeInstanceTemp.patchID].checkTreesAdd(patches[altTreeInstanceTemp.patchID].trees[altTreeInstanceTemp.idTree].getPosWorld().x, patches[altTreeInstanceTemp.patchID].trees[altTreeInstanceTemp.idTree].getPosWorld().z, patches[altTreeInstanceTemp.patchID].trees[altTreeInstanceTemp.idTree], altTreeInstanceTemp.patchID);
                            }


                            addDistanceCamera(trs[i], new AtiTemp(patches[altTreeInstanceTemp.patchID], altTreeInstanceTemp.patchID, altTreeInstanceTemp.idTree, altTreeInstanceTemp.isObject, altTreeInstanceTemp.quadID), altTreeInstanceTemp);

                            quads[altTreeInstanceTemp.patchID].lockQuads(patches[altTreeInstanceTemp.patchID].trees[altTreeInstanceTemp.idTree].getPosWorld());
                        }
                        else
                        {
                            if (altTreesMain.isPlaying)
                                Destroy(trs[i].gameObject);
                            else
                            {

                                #if UNITY_EDITOR
                                    Object[] objs = Selection.objects;
                                    Object[] objsTemp = new Object[objs.Length - 1];
                                    int h = 0;
                                    for (int g = 0; g < objs.Length; g++)
                                    {
                                        if (!((GameObject)objs[g]).Equals(trs[i].gameObject))
                                        {
                                            objsTemp[h] = objs[g];
                                            h++;
                                        }
                                    }

                                    Selection.objects = objsTemp;
                                #endif

                                DestroyImmediate(trs[i].gameObject);
                            }
                        }
                    }
                    else
                    {
                        attTemp = patches[altTreeInstanceTemp.patchID].quadObjects[altTreeInstanceTemp.quadID].findObjectById(altTreeInstanceTemp.idTree);
                        if (attTemp != null)
                        {
                            if (!attTemp.goMesh.Equals(trs[i].gameObject))
                            {
                                altTreeInstanceTemp.idTree = copyTree(altTreeInstanceTemp.patchID, altTreeInstanceTemp.idTree, trs[i].gameObject, false, altTreeInstanceTemp.quadID).idTree;
                            }


                            addDistanceCamera(trs[i], new AtiTemp(patches[altTreeInstanceTemp.patchID], altTreeInstanceTemp.patchID, altTreeInstanceTemp.idTree, altTreeInstanceTemp.isObject, altTreeInstanceTemp.quadID), altTreeInstanceTemp);

                            patches[altTreeInstanceTemp.patchID].quadObjects[altTreeInstanceTemp.quadID].lockQuads(attTemp.getPosWorld());
                        }
                        else
                        {
                            if (altTreesMain.isPlaying)
                                Destroy(trs[i].gameObject);
                            else
                            {
                                #if UNITY_EDITOR
                                    Object[] objs = Selection.objects;
                                    Object[] objsTemp = new Object[objs.Length - 1];
                                    int h = 0;
                                    for (int g = 0; g < objs.Length; g++)
                                    {
                                        if (!((GameObject)objs[g]).Equals(trs[i].gameObject))
                                        {
                                            objsTemp[h] = objs[g];
                                            h++;
                                        }
                                    }

                                    Selection.objects = objsTemp;

                                #endif
                                DestroyImmediate(trs[i].gameObject);
                            }
                        }
                    }
                }
            }
            altTreeInstanceTemp = null;
        }

        public void offSelectionTrees()
        {
            isSelectionTree = false;

            for (int i = 0; i < quads.Length; i++)
            {
                if (quads[i] != null)
                    quads[i].unlockQuads();
            }
        }

        public AltTreesPatch[] saveTrees()
        {
            AltTreeInstance ati = null;
            bool isRecalculateBound = false;
            List<AltTreesPatch> altTreesList = new List<AltTreesPatch>();
            List<AltTreesPatch> altTreesListReturn = new List<AltTreesPatch>();

            lock (distanceCamerasLock)
            {
                for (int i = 0; i < distanceCameras.Length; i++)
                {
                    isRecalculateBound = false;

                    if (distanceCameras[i].trans != null)
                    {
                        if(distanceCameras[i].isSelected)
                        {
                            ati = distanceCameras[i].ati;

                            if (ati != null)
                            {
                                if (!ati.isObject)
                                {
                                    att = ati.manager.patches[ati.patchID].trees[ati.idTree];

                                    if (AltUtilities.fastDistanceSqrt(distanceCameras[i].trans.position, att.getPosWorld()) > 0.0001f || Mathf.Abs(att.rotation - distanceCameras[i].trans.localRotation.eulerAngles.y) > 0.001f || (att.widthScale != distanceCameras[i].trans.localScale.x || att.heightScale != distanceCameras[i].trans.localScale.y || att.color != ati.hueLeave || att.colorBark != ati.hueBark))
                                    {
                                        if (!quads[ati.patchID].bound.inBounds(distanceCameras[i].trans.position.x, distanceCameras[i].trans.position.z))
                                        {
                                            isRecalculateBound = true;
                                            quads[ati.patchID].removeTree(att);

                                            AltTreesPatch patchTemp = getPatch(distanceCameras[i].trans.position + jump * altTreesMain.altTreesManagerData.sizePatch - jumpPos);
                                            AltTreesPatch starPatch;

                                            if (patchTemp == null)
                                            {
                                                patchTemp = addPatch(Mathf.FloorToInt((distanceCameras[i].trans.position.x + jump.x * altTreesMain.altTreesManagerData.sizePatch - jumpPos.x) / ((float)altTreesMain.altTreesManagerData.sizePatch)), Mathf.FloorToInt((distanceCameras[i].trans.position.z + jump.z * altTreesMain.altTreesManagerData.sizePatch + jumpPos.z) / ((float)altTreesMain.altTreesManagerData.sizePatch)));

                                                att.pos = patchTemp.getTreePosLocal(distanceCameras[i].trans.position, jump, jumpPos, altTreesMain.altTreesManagerData.sizePatch);
                                                att.pos2D = new Vector2(att.pos.x, att.pos.z);
                                                att.rotation = distanceCameras[i].trans.localRotation.eulerAngles.y;
                                                att.widthScale = distanceCameras[i].trans.localScale.x;
                                                att.heightScale = distanceCameras[i].trans.localScale.y;
                                                att.maxScaleSquare = Mathf.Max(att.heightScale, att.widthScale);
                                                att.maxScaleSquare *= att.maxScaleSquare;
                                                att.color = ati.hueLeave;
                                                att.colorBark = ati.hueBark;
                                                starPatch = att.altTreesPatch;
                                                att.altTreesPatch = patchTemp;


                                                patchTemp.tempTrees.Add(att);
                                            }
                                            else
                                            {

                                                att.pos = patchTemp.getTreePosLocal(distanceCameras[i].trans.position, jump, jumpPos, altTreesMain.altTreesManagerData.sizePatch);
                                                att.pos2D = new Vector2(att.pos.x, att.pos.z);
                                                att.rotation = distanceCameras[i].trans.localRotation.eulerAngles.y;
                                                att.widthScale = distanceCameras[i].trans.localScale.x;
                                                att.heightScale = distanceCameras[i].trans.localScale.y;
                                                att.maxScaleSquare = Mathf.Max(att.heightScale, att.widthScale);
                                                att.maxScaleSquare *= att.maxScaleSquare;
                                                att.color = ati.hueLeave;
                                                att.colorBark = ati.hueBark;
                                                starPatch = att.altTreesPatch;
                                                att.altTreesPatch = patchTemp;

                                                patchTemp.checkTreePrototype(att.idPrototype, ati.manager.patches[ati.patchID].getAltTreePrototype(att.idPrototype), true, true);
                                                addTrees(new AddTreesPositionsStruct[1] { new AddTreesPositionsStruct(distanceCameras[i].trans.position, ati.manager.patches[ati.patchID].getAltTreePrototype(att.idPrototype)) }, patchTemp.altTreesId, att.rotation, att.heightScale, att.widthScale, ati.hueLeave, ati.hueBark);

                                                if (!altTreesListReturn.Contains(patchTemp))
                                                    altTreesListReturn.Add(patchTemp);
                                            }
                                            List<int> del = new List<int>();
                                            del.Add(ati.idTree);
                                            starPatch.EditDataFileTrees(null, 0, del, -1);

                                            if (starPatch.treesCount == starPatch.treesEmptyCount)
                                            {
                                                if (starPatch.treesData != null)
                                                {
                                                    starPatch.trees = new AltTreesTrees[0];
                                                    starPatch.treesCount = 0;
                                                    starPatch.treesEmptyCount = 0;
                                                    starPatch.treesEmpty = new int[0];

                                                    #if UNITY_EDITOR
                                                        AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(starPatch.treesData));

                                                        EditorUtility.SetDirty(altTreesMain.altTreesManagerData);
                                                        AssetDatabase.SaveAssets();
                                                        AssetDatabase.Refresh();
                                                    #endif
                                                }
                                            }

                                            att = null;
                                        }
                                        else
                                        {
                                            quads[ati.patchID].reInitBillboards(att, distanceCameras[i].trans.position);
                                            att.pos = ati.manager.patches[ati.patchID].getTreePosLocal(distanceCameras[i].trans.position, jump, jumpPos, altTreesMain.altTreesManagerData.sizePatch);
                                            att.pos2D = new Vector2(att.pos.x, att.pos.z);
                                            att.rotation = distanceCameras[i].trans.localRotation.eulerAngles.y;
                                            att.altTreesPatch = ati.manager.patches[ati.patchID];
                                            att.widthScale = distanceCameras[i].trans.localScale.x;
                                            att.heightScale = distanceCameras[i].trans.localScale.y;
                                            att.maxScaleSquare = Mathf.Max(att.heightScale, att.widthScale);
                                            att.maxScaleSquare *= att.maxScaleSquare;
                                            att.color = ati.hueLeave;
                                            att.colorBark = ati.hueBark;

                                            att.altTreesPatch.EditDataFileTrees(null, 0, null, ati.idTree);
                                        }

                                        //ati.manager.patches[ati.patchID].recalculateBound();

                                        if (!altTreesListReturn.Contains(ati.manager.patches[ati.patchID]))
                                            altTreesListReturn.Add(ati.manager.patches[ati.patchID]);

                                        if (isRecalculateBound)
                                            DestroyImmediate(ati.gameObject);
                                    }
                                }
                                else
                                {
                                    att = ati.manager.patches[ati.patchID].quadObjects[ati.quadID].findObjectById(ati.idTree);

                                    if (AltUtilities.fastDistanceSqrt(distanceCameras[i].trans.position, att.getPosWorld()) > 0.0001f || Mathf.Abs(att.rotation - distanceCameras[i].trans.localRotation.eulerAngles.y) > 0.001f || (att.widthScale != distanceCameras[i].trans.localScale.x || att.heightScale != distanceCameras[i].trans.localScale.y || att.color != ati.hueLeave || att.colorBark != ati.hueBark))
                                    {
                                        if (!quads[ati.patchID].bound.inBounds(distanceCameras[i].trans.position.x, distanceCameras[i].trans.position.z))
                                        {
                                            isRecalculateBound = true;
                                            ati.manager.patches[ati.patchID].quadObjects[ati.quadID].removeTree(att, ati.isObject);

                                            AltTreesPatch patchTemp = getPatch(distanceCameras[i].trans.position + jump * altTreesMain.altTreesManagerData.sizePatch - jumpPos);
                                            AltTreesPatch starPatch;

                                            if (patchTemp == null)
                                            {
                                                patchTemp = addPatch(Mathf.FloorToInt((distanceCameras[i].trans.position.x + jump.x * altTreesMain.altTreesManagerData.sizePatch - jumpPos.x) / ((float)altTreesMain.altTreesManagerData.sizePatch)), Mathf.FloorToInt((distanceCameras[i].trans.position.z + jump.z * altTreesMain.altTreesManagerData.sizePatch + jumpPos.z) / ((float)altTreesMain.altTreesManagerData.sizePatch)));

                                                /*patchTemp.checkTreePrototype(att.idPrototype, ati.manager.patches[ati.patchID].getAltTreePrototype(att.idPrototype), false, true);
                                                patchTemp.Init(this, altTreesMain, altTreesMain.altTreesManagerData, true);
                                                addPatch(patchTemp);
                                                #if UNITY_EDITOR
                                                    EditorUtility.SetDirty(altTreesMain.altTreesManagerData);
                                                #endif*/

                                                att.pos = patchTemp.getTreePosLocal(distanceCameras[i].trans.position, jump, jumpPos, altTreesMain.altTreesManagerData.sizePatch);
                                                att.pos2D = new Vector2(att.pos.x, att.pos.z);
                                                att.rotation = distanceCameras[i].trans.localRotation.eulerAngles.y;
                                                att.widthScale = distanceCameras[i].trans.localScale.x;
                                                att.heightScale = distanceCameras[i].trans.localScale.y;
                                                att.maxScaleSquare = Mathf.Max(att.heightScale, att.widthScale);
                                                att.maxScaleSquare *= att.maxScaleSquare;
                                                att.color = ati.hueLeave;
                                                att.colorBark = ati.hueBark;
                                                starPatch = att.altTreesPatch;
                                                att.altTreesPatch = patchTemp;

                                                patchTemp.tempTrees.Add(att);
                                                /*addTrees(new AddTreesPositionsStruct[1] { new AddTreesPositionsStruct(distanceCameras[i].trans.position, ati.manager.patches[ati.patchID].getAltTreePrototype(att.idPrototype)) }, patchTemp.altTreesId, att.rotation, att.heightScale, att.widthScale, ati.hueLeave, ati.hueBark);

                                                if (!altTreesListReturn.Contains(patchTemp))
                                                    altTreesListReturn.Add(patchTemp);*/
                                            }
                                            else
                                            {
                                                att.pos = patchTemp.getTreePosLocal(distanceCameras[i].trans.position, jump, jumpPos, altTreesMain.altTreesManagerData.sizePatch);
                                                att.pos2D = new Vector2(att.pos.x, att.pos.z);
                                                att.rotation = distanceCameras[i].trans.localRotation.eulerAngles.y;
                                                att.widthScale = distanceCameras[i].trans.localScale.x;
                                                att.heightScale = distanceCameras[i].trans.localScale.y;
                                                att.maxScaleSquare = Mathf.Max(att.heightScale, att.widthScale);
                                                att.maxScaleSquare *= att.maxScaleSquare;
                                                att.color = ati.hueLeave;
                                                att.colorBark = ati.hueBark;
                                                starPatch = att.altTreesPatch;
                                                att.altTreesPatch = patchTemp;

                                                patchTemp.checkTreePrototype(att.idPrototype, ati.manager.patches[ati.patchID].getAltTreePrototype(att.idPrototype), true, true);
                                                addTrees(new AddTreesPositionsStruct[1] { new AddTreesPositionsStruct(distanceCameras[i].trans.position, ati.manager.patches[ati.patchID].getAltTreePrototype(att.idPrototype)) }, patchTemp.altTreesId, att.rotation, att.heightScale, att.widthScale, ati.hueLeave, ati.hueBark);

                                                if (!altTreesListReturn.Contains(patchTemp))
                                                    altTreesListReturn.Add(patchTemp);
                                            }
                                            List<AltTreesTrees> del = new List<AltTreesTrees>();
                                            del.Add(att);
                                            starPatch.EditDataFileObjects(-1, del);
                                            att = null;

                                            if (starPatch.treesNoGroupCount == starPatch.treesNoGroupEmptyCount)
                                            {
                                                if (starPatch.treesNoGroupData != null)
                                                {
                                                    starPatch.treesNoGroupCount = 0;
                                                    starPatch.treesNoGroupEmptyCount = 0;

                                                    #if UNITY_EDITOR
                                                        AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(starPatch.treesNoGroupData));

                                                        EditorUtility.SetDirty(altTreesMain.altTreesManagerData);
                                                        AssetDatabase.SaveAssets();
                                                        AssetDatabase.Refresh();
                                                    #endif
                                                }
                                            }
                                        }
                                        else
                                        {
                                            att.idQuadObjectNew = -1;
                                            quads[ati.patchID].reInitBillboards(att, distanceCameras[i].trans.position, false);
                                            att.pos = ati.manager.patches[ati.patchID].getTreePosLocal(distanceCameras[i].trans.position, jump, jumpPos, altTreesMain.altTreesManagerData.sizePatch);
                                            att.pos2D = new Vector2(att.pos.x, att.pos.z);
                                            att.rotation = distanceCameras[i].trans.localRotation.eulerAngles.y;
                                            att.altTreesPatch = ati.manager.patches[ati.patchID];
                                            att.widthScale = distanceCameras[i].trans.localScale.x;
                                            att.heightScale = distanceCameras[i].trans.localScale.y;
                                            att.maxScaleSquare = Mathf.Max(att.heightScale, att.widthScale);
                                            att.maxScaleSquare *= att.maxScaleSquare;
                                            att.color = ati.hueLeave;
                                            att.colorBark = ati.hueBark;

                                            if (att.idQuadObjectNew != -1)
                                            {
                                                List<AltTreesTrees> attl = new List<AltTreesTrees>();
                                                attl.Add(att);
                                                att.altTreesPatch.EditDataFileObjects(-1, attl);
                                                attl.Add(att);
                                                att.idQuadObject = att.idQuadObjectNew;
                                                att.idQuadObjectNew = -1;

                                                Dictionary<int, List<AltTreesTrees>> idQuadsTemp = new Dictionary<int, List<AltTreesTrees>>();
                                                List<AltTreesTrees> attListT = new List<AltTreesTrees>();
                                                attListT.Add(att);
                                                idQuadsTemp.Add(att.idQuadObject, attListT);
                                                //att.altTreesPatch.tempTrees.Add(att);


                                                att.altTreesPatch.EditDataFileObjects(1, null, idQuadsTemp);
                                            }
                                            else
                                                att.altTreesPatch.EditDataFileObjects(-1, null, null, att);
                                        }

                                        //ati.manager.patches[ati.patchID].recalculateBound();

                                        if (!altTreesListReturn.Contains(ati.manager.patches[ati.patchID]))
                                            altTreesListReturn.Add(ati.manager.patches[ati.patchID]);

                                        if (isRecalculateBound)
                                            DestroyImmediate(ati.gameObject);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        if (treesCameras[i] != null)
                        {
                            if (!treesCameras[i].isObject)
                            {
                                att = treesCameras[i].altTrees.trees[treesCameras[i].idTree];
                                quads[treesCameras[i].altTreesId].removeTree(att, false);

                                List<int> del = new List<int>();
                                del.Add(treesCameras[i].idTree);
                                treesCameras[i].altTrees.EditDataFileTrees(null, 0, del, -1);
                                treesCameras[i].altTrees.trees[treesCameras[i].idTree] = null;

                                //treesCameras[i].altTrees.recalculateBound();

                                if (!altTreesListReturn.Contains(treesCameras[i].altTrees))
                                    altTreesListReturn.Add(treesCameras[i].altTrees);
                            }
                            else
                            {
                                att = treesCameras[i].altTrees.quadObjects[treesCameras[i].quadID].findObjectById(treesCameras[i].idTree);
                                quads[treesCameras[i].altTreesId].removeTree(att, true);

                                List<AltTreesTrees> del = new List<AltTreesTrees>();
                                del.Add(att);
                                treesCameras[i].altTrees.EditDataFileObjects(-1, del);
                                treesCameras[i].altTrees.quadObjects[treesCameras[i].quadID].treesNoBillb.Remove(att);

                                //treesCameras[i].altTrees.recalculateBound();

                                if (!altTreesListReturn.Contains(treesCameras[i].altTrees))
                                    altTreesListReturn.Add(treesCameras[i].altTrees);
                            }
                        }
                    }
                }
                isRecalculateBound = false;

                if (!isRecalculateBound)
                {
                    for (int i = 0; i < quads.Length; i++)
                    {
                        if (quads[i] != null)
                            quads[i].goUpdateTrees();
                    }
                }
                else
                {
                    for (int i = 0; i < altTreesList.Count; i++)
                    {
                        removeAltTrees(altTreesList[i], false);
                        addPatch(altTreesList[i], false);
                    }
                }
            }
            clearDistanceCameras();
            return altTreesListReturn.ToArray();
        }

        public bool removeTrees(Vector2 pos, float radius, AltTreesPatch at, List<int> removedTrees, List<AltTreesTrees> removedTreesNoGroup, int idPrototype = -1)
        {
            bool result = false;
            for (int i = 0; i < quads.Length; i++)
            {
                if (patches[i] != null)
                {
                    if (patches[i].Equals(at))
                    {
                        int removedTreesCount = 0;
                        if (quads[i].removeTrees(pos, radius, at, removedTrees, removedTreesNoGroup, true, ref removedTreesCount, idPrototype))
                        {
                            if (removedTrees.Count > 0)
                                quads[i].goUpdateTrees();
                            result = true;
                        }
                    }
                }
            }

            return result;
        }
        public bool removeTrees(Vector2 pos, float radius, AltTreesPatch at, int idPrototype = -1)
        {
            bool result = false;
            for (int i = 0; i < quads.Length; i++)
            {
                if (patches[i] != null)
                {
                    if (patches[i].Equals(at))
                    {
                        int removedTreesCount = 0;
                        if (quads[i].removeTrees(pos, radius, at, null, null, false, ref removedTreesCount, idPrototype))
                        {
                            if (removedTreesCount > 0)
                                quads[i].goUpdateTrees();
                            result = true;
                        }
                    }
                }
            }

            return result;
        }

        /*public bool removeTrees(Vector2 pos, float sizeX, float sizeZ, AltTreesPatch at, List<int> removedTrees, bool udpadeTreesOnScene = true)
        {
            bool result = false;
            for (int i = 0; i < quads.Length; i++)
            {
                if (patches[i] != null)
                {
                    if (patches[i].Equals(at))
                    {
                        if (quads[i].removeTrees(pos, sizeX, sizeZ, at, removedTrees, udpadeTreesOnScene))
                        {

                            if(udpadeTreesOnScene)
                                quads[i].goUpdateTrees();
                            result = true;
                        }
                    }
                }
            }

            return result;
        }*/
    
        public AltTreesTrees[] getTreesForExport(Vector2 pos, float sizeX, float sizeZ, AltTreesPatch at)
        {
            List<AltTreesTrees> attTemp = new List<AltTreesTrees>();

            for (int i = 0; i < quads.Length; i++)
            {
                if (patches[i] != null)
                {
                    if (patches[i].Equals(at))
                    {
                        quads[i].getTreesForExport(pos, sizeX, sizeZ, at, attTemp);
                    }
                }
            }

            return attTemp.ToArray();
        }

        public void getTrees(Vector2 _pos, float radius, bool trees, bool objects, int idPrototype, AltTreesPatch at, List<AltTreesTrees> _attTemp)
        {
            for (int i = 0; i < quads.Length; i++)
            {
                if (patches[i] != null)
                {
                    if (patches[i].Equals(at))
                    {
                        quads[i].getTrees(_pos, radius, trees, objects, idPrototype, at, _attTemp);
                    }
                }
            }
        }

        public void getAllTrees(bool trees, bool objects, int idPrototype, AltTreesPatch at, List<AltTreesTrees> _attTemp)
        {
            for (int i = 0; i < quads.Length; i++)
            {
                if (patches[i] != null)
                {
                    if (patches[i].Equals(at))
                    {
                        quads[i].getAllTrees(trees, objects, idPrototype, at, _attTemp);
                    }
                }
            }
        }


        public void clearDistanceCameras()
        {
            lock (distanceCamerasLock)
            {
                if (altTreesMain.isPlaying)
                {
                    if (altTreesMain.cameraModeDistance == 0 || (altTreesMain.cameraModeDistance == 1 && altTreesMain.activeCameraDistance == null))
                    {
                        if (Camera.main != null && Camera.main.gameObject.activeInHierarchy && Camera.main.enabled)
                        {
                            distanceCameras = new DistanceCamera[1];
                            distanceCameras[0].trans = Camera.main.transform;
                            distanceCameras[0].isSelected = false;
                            treesCameras = new AtiTemp[1];
                        }
                        else
                        {
                            Camera[] cams = Camera.allCameras;
                            if (cams != null && cams.Length > 0)
                            {
                                for (int i = 0; i < cams.Length; i++)
                                {
                                    if (cams[i] != null && cams[i].gameObject.activeInHierarchy && cams[i].enabled)
                                    {
                                        distanceCameras = new DistanceCamera[1];
                                        distanceCameras[0].trans = cams[i].transform;
                                        distanceCameras[0].isSelected = false;
                                        treesCameras = new AtiTemp[1];
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    else if (altTreesMain.cameraModeDistance != 2)
                    {
                        distanceCameras = new DistanceCamera[altTreesMain.activeCameraDistance.Length];
                        treesCameras = new AtiTemp[altTreesMain.activeCameraDistance.Length];
                        for (int i = 0; i < distanceCameras.Length; i++)
                        {
                            distanceCameras[i].trans = altTreesMain.activeCameraDistance[i].transform;
                            distanceCameras[i].isSelected = false;
                        }
                    }
                }
                else
                {
                    if (AltTreesManager.camEditor != null)
                    {
                        distanceCameras = new DistanceCamera[1];
                        distanceCameras[0].trans = camEditor.transform;
                        distanceCameras[0].isSelected = false;
                        treesCameras = new AtiTemp[1];
                    }
                }
            }
        }

        AltTreesPatch getPatch(Vector3 pos)
        {
            return altTreesMain.getPatch(pos, altTreesMain.altTreesManagerData.sizePatch);
        }

        AltTreesPatch addPatch(int _stepX, int _stepY)
        {
            AltTreesPatch atpTemp = new AltTreesPatch(_stepX, _stepY);

            AltTreesPatch[] patchesTemp = altTreesMain.altTreesManagerData.patches;
            altTreesMain.altTreesManagerData.patches = new AltTreesPatch[patchesTemp.Length + 1];
            for (int i = 0; i < patchesTemp.Length; i++)
            {
                altTreesMain.altTreesManagerData.patches[i] = patchesTemp[i];
            }
            altTreesMain.altTreesManagerData.patches[patchesTemp.Length] = atpTemp;
        
            return atpTemp;
        }


        void OnDestroy()
        {
            destroy(false);
        }

        void OnDisable()
        {
            destroy(true);
        }

        void OnApplicationQuit()
        {
            destroy(true);
        }

        /*void OnLevelWasLoaded(int level)
        {
            destroy(true);
        }*/

        public void destroy(bool destroyThis)
        {
            isInit = false;
            distanceCamerasInit = false;
            frustumCamerasInit = false;
            lock (timerLock)
            {
                if (timerCheck != null)
                    timerCheck.Dispose();
                timerLockStop = true;
            }
            
            if (!isDestroyed)
            {
                for (int i = 0; i < patches.Length; i++)
                {
                    if (patches[i] != null)
                    {
                        if (!Application.isPlaying)//
                        {
                            if (patches[i].trees != null)
                            {
                                for (int j = 0; j < patches[i].trees.Length; j++)
                                {
                                    if (patches[i].trees[j] != null && patches[i].trees[j].goMesh != null && patches[i].trees[j].currentLOD != -2)
                                    {
                                        if (Application.isPlaying)
                                            Destroy(patches[i].trees[j].goMesh);
                                        else
                                            DestroyImmediate(patches[i].trees[j].goMesh);
                                    }
                                }
                            }
                        }
                        patches[i].trees = null;
                        if (!Application.isPlaying)//
                        {
                            if (patches[i].quadObjects != null)
                            {
                                for (int t = 0; t < patches[i].quadObjects.Length; t++)
                                {
                                    for (int j = 0; j < patches[i].quadObjects[t].treesNoBillb.Count; j++)
                                    {
                                        if (patches[i].quadObjects[t].treesNoBillb[j] != null && patches[i].quadObjects[t].treesNoBillb[j].goMesh != null && patches[i].quadObjects[t].treesNoBillb[j].currentLOD != -2)
                                        {
                                            if (Application.isPlaying)
                                                Destroy(patches[i].quadObjects[t].treesNoBillb[j].goMesh);
                                            else
                                                DestroyImmediate(patches[i].quadObjects[t].treesNoBillb[j].goMesh);
                                        }
                                    }
                                    patches[i].quadObjects[t].treesNoBillb.Clear();
                                    patches[i].quadObjects[t] = null;
                                }
                                patches[i].quadObjects = null;
                            }
                        }
                        if (patches[i].rendersDebug != null)
                        {
                            for (int j = 0; j < patches[i].rendersDebug.Count; j++)
                            {
                                DestroyImmediate(patches[i].rendersDebug[j]);
                            }
                            patches[i].rendersDebug.Clear();
                        }

                        if (quads != null)
                        {
                            if (quads.Length > i && quads[i] != null)
                            {
                                quads[i].removeMeshes();
                                patches[i] = null;
                                quads[i] = null;
                            }
                        }
                    }
                }

                quadsToRender.Clear();

                int collidersMaxCount = 0;
                int colliderBillboardsMaxCount = 0;

                for (int key = 0; key < treesPoolArray.Length; key++)
                {
                    if (Application.isPlaying)
                    {
                        if (treesPoolArray[key].collidersMaxCount > collidersMaxCount)
                            collidersMaxCount = treesPoolArray[key].collidersMaxCount;
                        if (treesPoolArray[key].colliderBillboardsMaxCount > colliderBillboardsMaxCount)
                            colliderBillboardsMaxCount = treesPoolArray[key].colliderBillboardsMaxCount;

                        Destroy(treesPoolArray[key].materialBillboard);
                        Destroy(treesPoolArray[key].materialBillboardGroup);

                        Destroy(treesPoolArray[key].materialBillboardCrossFade);
                    }
                    else
                    {
                        DestroyImmediate(treesPoolArray[key].materialBillboard);
                        DestroyImmediate(treesPoolArray[key].materialBillboardGroup);

                        DestroyImmediate(treesPoolArray[key].materialBillboardCrossFade);
                    }

                    treesPoolArray[key].treesToRender = null;

                    for (int j = 0; j < treesPoolArray[key].objsArray.Length; j++)
                    {
                        for (int k = 0; k < treesPoolArray[key].objsArray[j].materialsMesh.Length; k++)
                        {
                            if (Application.isPlaying)
                            {
                                Destroy(treesPoolArray[key].objsArray[j].materialsMesh[k]);
                                Destroy(treesPoolArray[key].objsArray[j].materialsMeshCrossFade[k]);
                            }
                            else
                            {
                                DestroyImmediate(treesPoolArray[key].objsArray[j].materialsMesh[k]);
                                DestroyImmediate(treesPoolArray[key].objsArray[j].materialsMeshCrossFade[k]);
                            }
                        }
                    }
                }


                if (Application.isPlaying)
                {
                    if(collidersUsedList != null)
                        collidersUsedList.Clear();
                    collidersUsedList = null;

                    for (int key = 0; key < treesPoolArray.Length; key++)
                    {
                        for (int j = 0; j < treesPoolArray[key].colliderBillboardsArray.Count; j++)
                        {
                            if (treesPoolArray[key].colliderBillboardsArray[j] != null)
                                DestroyImmediate(treesPoolArray[key].colliderBillboardsArray[j].go);
                        }
                        for (int j = 0; j < treesPoolArray[key].collidersArray.Count; j++)
                        {
                            if (treesPoolArray[key].collidersArray[j].go != null)
                                DestroyImmediate(treesPoolArray[key].collidersArray[j].go);
                        }
                    }


                    /*for (int h = 0; h < objBillboardsPool.Count; h++)
                    {
                        objBillboardsPool[h].mr = null;
                        Destroy(objBillboardsPool[h].ms);
                        Destroy(objBillboardsPool[h].go);
                    }*/

                    if (Application.isEditor)
                    {
                        Destroy(goCubeDebug.GetComponent<MeshRenderer>().sharedMaterial);
                        Destroy(goCubeDebug);
                    }

                    collidersMaxCount = Mathf.Clamp(collidersMaxCount, 40, 3000);
                    colliderBillboardsMaxCount = Mathf.Clamp(colliderBillboardsMaxCount, 40, 3000);


                    if (collidersMaxCount > altTreesMain.altTreesManagerData.initCollidersCountPool)
                    {
                        collidersMaxCount = Mathf.Clamp(collidersMaxCount + Mathf.FloorToInt(collidersMaxCount / 10f), 40, 3000);
                        altTreesMain.LogWarning("Recommended change \"Initial Collider Count\" from " + altTreesMain.altTreesManagerData.initCollidersCountPool + " to " + collidersMaxCount + ", and \"Max Colliders Count\" from " + altTreesMain.altTreesManagerData.collidersPerOneMaxPool + " to " + (collidersMaxCount + Mathf.FloorToInt(collidersMaxCount / 2f)) + ".");
                    }
                    else
                    {
                        if (collidersMaxCount > altTreesMain.altTreesManagerData.collidersPerOneMaxPool)
                        {
                            collidersMaxCount = Mathf.Clamp(collidersMaxCount + Mathf.FloorToInt(collidersMaxCount / 10f), 40, 3000);
                            altTreesMain.LogWarning("Recommended change \"Max Colliders Count\" from " + altTreesMain.altTreesManagerData.collidersPerOneMaxPool + " to " + (collidersMaxCount + Mathf.FloorToInt(collidersMaxCount / 2f)) + ".");
                        }
                    }
                    
                    if (colliderBillboardsMaxCount > altTreesMain.altTreesManagerData.initColliderBillboardsCountPool)
                    {
                        colliderBillboardsMaxCount = Mathf.Clamp(colliderBillboardsMaxCount + Mathf.FloorToInt(colliderBillboardsMaxCount / 10f), 40, 3000);
                        altTreesMain.LogWarning("Recommended change \"Initial Billboard Collider Count\" from " + altTreesMain.altTreesManagerData.initColliderBillboardsCountPool + " to " + colliderBillboardsMaxCount + ", and \"Max Billboard Colliders Count\" from " + altTreesMain.altTreesManagerData.colliderBillboardsPerOneMaxPool + " to " + (colliderBillboardsMaxCount + Mathf.FloorToInt(colliderBillboardsMaxCount / 2f)) + ".");
                    }
                    else
                    {
                        if (colliderBillboardsMaxCount > altTreesMain.altTreesManagerData.colliderBillboardsPerOneMaxPool)
                        {
                            colliderBillboardsMaxCount = Mathf.Clamp(colliderBillboardsMaxCount + Mathf.FloorToInt(colliderBillboardsMaxCount / 10f), 40, 3000);
                            altTreesMain.LogWarning("Recommended change \"Max Billboard Colliders Count\" from " + altTreesMain.altTreesManagerData.colliderBillboardsPerOneMaxPool + " to " + (colliderBillboardsMaxCount + Mathf.FloorToInt(colliderBillboardsMaxCount / 2f)) + ".");
                        }
                    }
                }
                else
                {
                    if (Application.isEditor)
                    {
                        DestroyImmediate(goCubeDebug.GetComponent<MeshRenderer>().sharedMaterial);
                        DestroyImmediate(goCubeDebug);
                    }
                }


                AltTreesQuad.objsToInit.Clear();
                isDestroyed = true;

                if (destroyThis)
                    DestroyImmediate(this.gameObject);

            }
        }
        
        public void removeAltTrees(AltTreesPatch altT, bool delPrototypes = true)
        {
            int idAltT = -1;
            for (int i = 0; i < patches.Length; i++)
            {
                if (patches[i] != null)
                {
                    if (patches[i].Equals(altT))
                    {
                        idAltT = i;
                        break;
                    }
                }
            }
            if (idAltT != -1)
            {
                if (!altTreesMain.isPlaying)//
                {
                    if (patches[idAltT].trees != null)
                    {
                        for (int j = 0; j < patches[idAltT].trees.Length; j++)
                        {
                            if (patches[idAltT].trees[j] != null && patches[idAltT].trees[j].goMesh != null)
                                DestroyImmediate(patches[idAltT].trees[j].goMesh);
                        }
                    }
                }

                if (patches[idAltT].rendersDebug != null)
                {
                    for (int j = 0; j < patches[idAltT].rendersDebug.Count; j++)
                    {
                        DestroyImmediate(patches[idAltT].rendersDebug[j]);
                    }
                    patches[idAltT].rendersDebug.Clear();
                }

                if (quads[idAltT] != null)
                {
                    quads[idAltT].removeMeshes();
                    patches[idAltT] = null;
                    quads[idAltT] = null;
                }

            }
        }

        public void addTrees(AddTreesPositionsStruct[] positions, int idAltTree, float rotation, float height, float width, Color hueLeaves, Color hueBark)
        {
            addTrees(positions, idAltTree, false, false, height, rotation, false, false, width, 0f, hueLeaves, hueBark, false, false, true);
        }

        public void addTrees(AddTreesPositionsStruct[] positions, int idAltTree, bool randomRotation, bool isRandomHeight, float height, float heightRandom,
                            bool lockWidthToHeight, bool isRandomWidth, float width, float widthRandom, Color hueLeaves, Color hueBark, bool isRandomHueLeaves, bool isRandomHueBark, bool isTranslate = false, bool isProgressBar = false, string progressBarTitle = "")
        {
            if (patches.Length > idAltTree && patches[idAltTree] != null)
            {
                int positionsLength = positions.Length;
                int countObjs = 0;

                if (positionsLength > 0)
                {
                    int countPlaced = 0;
                    int percentAdded = 0;

                    #if UNITY_EDITOR
                    {
                        if (isProgressBar)
                        {
                            EditorUtility.DisplayProgressBar(progressBarTitle, "Adding... ", 0.0f);
                        }

                    }
                    #endif


                    float rotationTemp = 0f;
                    float heightTemp = 0f;
                    float widthTemp = 0f;

                    float hueLeavesA = hueLeaves.a;
                    float hueBarkA = hueBark.a;

                    int treesBillb = 0;
                    int treesNoBillb = 0;
                    int treesBillbSch = 0;
                    int treesNoBillbSch = 0;

                    for (int i = 0; i < positionsLength; i++)
                    {
                        if (!positions[i].altTree.isObject)
                            treesBillb++;
                        else
                            treesNoBillb++;
                    }

                    AddTreesPositionsStruct[] treesBillbArr = new AddTreesPositionsStruct[treesBillb];
                    AddTreesPositionsStruct[] treesNoBillbArr = new AddTreesPositionsStruct[treesNoBillb];

                    for (int i = 0; i < positionsLength; i++)
                    {
                        if (!positions[i].altTree.isObject)
                        {
                            treesBillbArr[treesBillbSch] = positions[i];
                            treesBillbSch++;
                        }
                        else
                        {
                            treesNoBillbArr[treesNoBillbSch] = positions[i];
                            treesNoBillbSch++;
                        }
                    }
                    treesBillbSch = 0;
                    treesNoBillbSch = 0;

                    if (patches[idAltTree].trees == null)
                        patches[idAltTree].trees = new AltTreesTrees[0];

                    int countEmptyRemove = 0;
                    List<int> changedTrees = new List<int>();
                    int addedTreesCount = 0;

                    if (treesBillb > 0)
                    {
                        countEmptyRemove = 0;

                        for (int j = 0; j < patches[idAltTree].treesEmptyCount; j++)
                        {
                            if (!isTranslate)
                            {
                                rotationTemp = randomRotation ? Random.value * 360f : 0f;
                                heightTemp = isRandomHeight ? height + Random.value * heightRandom : height;

                                if (lockWidthToHeight)
                                    widthTemp = heightTemp;
                                else
                                    widthTemp = isRandomWidth ? width + Random.value * widthRandom : width;
                            }
                            else
                            {
                                rotationTemp = heightRandom;
                                heightTemp = height;
                                widthTemp = width;
                            }

                            if (isRandomHueLeaves)
                                hueLeaves.a = Random.value * hueLeavesA;
                            if (isRandomHueBark)
                                hueBark.a = Random.value * hueBarkA;

                            patches[idAltTree].trees[patches[idAltTree].treesEmpty[j]] = new AltTreesTrees(patches[idAltTree].getTreePosLocal(treesBillbArr[treesBillbSch].pos, jump, jumpPos, altTreesMain.altTreesManagerData.sizePatch), patches[idAltTree].treesEmpty[j], treesBillbArr[treesBillbSch].altTree.id, false, hueLeaves, hueBark, rotationTemp, heightTemp, widthTemp, patches[idAltTree]);
                            patches[idAltTree].trees[patches[idAltTree].treesEmpty[j]].idPrototypeIndex = getPrototypeIndex(patches[idAltTree].trees[patches[idAltTree].treesEmpty[j]].idPrototype);
                            quads[idAltTree].checkTreesAdd(patches[idAltTree].trees[patches[idAltTree].treesEmpty[j]].getPosWorld().x, patches[idAltTree].trees[patches[idAltTree].treesEmpty[j]].getPosWorld().z, patches[idAltTree].trees[patches[idAltTree].treesEmpty[j]], idAltTree);
                            treesBillbSch++;
                            countEmptyRemove++;
                            changedTrees.Add(patches[idAltTree].treesEmpty[j]);


                            #if UNITY_EDITOR
                            {
                                if(isProgressBar)
                                { 
                                    countPlaced++;
                    
                                    if(countPlaced >= (positionsLength / 20f))
                                    {
                                        countPlaced = 0;
                                        percentAdded++;
                                        
                                        EditorUtility.DisplayProgressBar(progressBarTitle, "Adding... ", percentAdded * 0.1f);
                                    }
                                }
                            }
                            #endif


                            if (treesBillbSch >= treesBillb)
                                break;
                        }
                        int[] treesEmptyTemp = patches[idAltTree].treesEmpty;
                        patches[idAltTree].treesEmptyCount -= countEmptyRemove;
                        patches[idAltTree].treesEmpty = new int[patches[idAltTree].treesEmptyCount];
                        for(int i = countEmptyRemove; i < treesEmptyTemp.Length; i++)
                        {
                            patches[idAltTree].treesEmpty[i - countEmptyRemove] = treesEmptyTemp[i];
                        }
                    }

                    AltTreesTrees attTemp = null;
                    bool stopTemp = false;
                    Dictionary<int, List<AltTreesTrees>> idQuadsTemp = new Dictionary<int, List<AltTreesTrees>>();
                    Vector3 vector3Temp = new Vector3();

                    if (treesNoBillb > 0)
                    {
                        for(int j = 0; j < treesNoBillb; j++)
                        {
                            stopTemp = false;
                            if (!isTranslate)
                            {
                                rotationTemp = randomRotation ? Random.value * 360f : 0f;
                                heightTemp = isRandomHeight ? height + Random.value * heightRandom : height;

                                if (lockWidthToHeight)
                                    widthTemp = heightTemp;
                                else
                                    widthTemp = isRandomWidth ? width + Random.value * widthRandom : width;
                            }
                            else
                            {
                                rotationTemp = heightRandom;
                                heightTemp = height;
                                widthTemp = width;
                            }

                            if (isRandomHueLeaves)
                                hueLeaves.a = Random.value * hueLeavesA;
                            if (isRandomHueBark)
                                hueBark.a = Random.value * hueBarkA;

                            
                            attTemp = new AltTreesTrees(patches[idAltTree].getTreePosLocal(treesNoBillbArr[j].pos, jump, jumpPos, altTreesMain.altTreesManagerData.sizePatch), -1, treesNoBillbArr[treesNoBillbSch].altTree.id, true, hueLeaves, hueBark, rotationTemp, heightTemp, widthTemp, patches[idAltTree]);
                            vector3Temp = attTemp.getPosWorld();
                            attTemp.idPrototypeIndex = getPrototypeIndex(attTemp.idPrototype);
                            quads[idAltTree].getObjectQuadId(vector3Temp.x, vector3Temp.z, ref attTemp.idQuadObject, ref stopTemp);

                            if(attTemp.idQuadObject == -1)
                            {
                                vector3Temp.x += 0.01f;
                                vector3Temp.z += 0.01f;
                                stopTemp = false;
                                quads[idAltTree].getObjectQuadId(vector3Temp.x, vector3Temp.z, ref attTemp.idQuadObject, ref stopTemp);
                                if (attTemp.idQuadObject == -1)
                                {
                                    Debug.LogWarning("attTemp.idQuadObject == -1");
                                    continue;
                                }
                            }

                            attTemp.idTree = -1;

                            attTemp.currentLOD = -1;
                            attTemp.currentCrossFadeId = -1;

                            if (!idQuadsTemp.ContainsKey(attTemp.idQuadObject))
                            {
                                List<AltTreesTrees> attListT = new List<AltTreesTrees>();
                                attListT.Add(attTemp);
                                idQuadsTemp.Add(attTemp.idQuadObject, attListT);
                            }
                            else
                            {
                                idQuadsTemp[attTemp.idQuadObject].Add(attTemp);
                            }
                            countObjs++;
                            
                            if (patches[idAltTree].quadObjects[attTemp.idQuadObject - 1].isInitObjects)
                                patches[idAltTree].quadObjects[attTemp.idQuadObject - 1].checkTreesAdd(vector3Temp.x, vector3Temp.z, attTemp, idAltTree, false);
                            


                            #if UNITY_EDITOR
                            {
                                if(isProgressBar)
                                { 
                                    countPlaced++;
                    
                                    if(countPlaced >= (positionsLength / 20f))
                                    {
                                        countPlaced = 0;
                                        percentAdded++;
                                        
                                        EditorUtility.DisplayProgressBar(progressBarTitle, "Adding... ", percentAdded * 0.1f);
                                    }
                                }
                            }
                            #endif
                        }
                    }

                    if (treesBillbSch < treesBillb)
                    {
                        AltTreesTrees[] treesTemp = patches[idAltTree].trees;
                        patches[idAltTree].trees = new AltTreesTrees[treesTemp.Length + (treesBillb - treesBillbSch)];
                        addedTreesCount = treesBillb - treesBillbSch;
                        patches[idAltTree].treesCount = patches[idAltTree].trees.Length;

                        for (int i = 0; i < treesTemp.Length; i++)
                        {
                            patches[idAltTree].trees[i] = treesTemp[i];
                        }

                        int treesBillbSch2 = treesBillbSch;

                        for (int i = 0; i < (treesBillb - treesBillbSch); i++)
                        {
                            if (!isTranslate)
                            {
                                rotationTemp = randomRotation ? Random.value * 360f : 0f;
                                heightTemp = isRandomHeight ? height + Random.value * heightRandom : height;

                                if (lockWidthToHeight)
                                    widthTemp = heightTemp;
                                else
                                    widthTemp = isRandomWidth ? width + Random.value * widthRandom : width;
                            }
                            else
                            {
                                rotationTemp = heightRandom;
                                heightTemp = height;
                                widthTemp = width;
                            }

                            if (isRandomHueLeaves)
                                hueLeaves.a = Random.value * hueLeavesA;
                            if (isRandomHueBark)
                                hueBark.a = Random.value * hueBarkA;

                            patches[idAltTree].trees[treesTemp.Length + i] = new AltTreesTrees(patches[idAltTree].getTreePosLocal(treesBillbArr[treesBillbSch2].pos, jump, jumpPos, altTreesMain.altTreesManagerData.sizePatch), treesTemp.Length + i, treesBillbArr[treesBillbSch2].altTree.id, false, hueLeaves, hueBark, rotationTemp, heightTemp, widthTemp, patches[idAltTree]);
                            patches[idAltTree].trees[treesTemp.Length + i].idPrototypeIndex = getPrototypeIndex(patches[idAltTree].trees[treesTemp.Length + i].idPrototype);
                            quads[idAltTree].checkTreesAdd(patches[idAltTree].trees[treesTemp.Length + i].getPosWorld().x, patches[idAltTree].trees[treesTemp.Length + i].getPosWorld().z, patches[idAltTree].trees[treesTemp.Length + i], idAltTree);

                            treesBillbSch2++;
                            


                            #if UNITY_EDITOR
                            {
                                if(isProgressBar)
                                { 
                                    countPlaced++;
                    
                                    if(countPlaced >= (positionsLength / 20f))
                                    {
                                        countPlaced = 0;
                                        percentAdded++;
                                        
                                        EditorUtility.DisplayProgressBar(progressBarTitle, "Adding... ", percentAdded * 0.1f);
                                    }
                                }
                            }
                            #endif
                        }
                    }


                    #if UNITY_EDITOR
                    {
                        if(isProgressBar)
                        {
                            EditorUtility.ClearProgressBar();
                        }
                    }
                    #endif

                    if(treesBillb > 0)
                        patches[idAltTree].EditDataFileTrees(changedTrees, addedTreesCount, null, -1, null, isProgressBar, progressBarTitle);
                    if (treesNoBillb > 0)
                        patches[idAltTree].EditDataFileObjects(countObjs, null, idQuadsTemp, null, null, isProgressBar, progressBarTitle);

                    //patches[idAltTree].recalculateBound();
                }
                else
                    altTreesMain.LogError("positions.Length == 0");
            }
            else
                altTreesMain.LogError("altTrees.Length<=idAltTree || patches[idAltTree] == null");
        }

        public AltTreesTrees copyTree(int idAltTree, int idTree, GameObject go, bool isTrees, int idQuadObject)
        {
            if (isTrees)
            {
                AltTreesTrees[] treesTemp = patches[idAltTree].trees;
                patches[idAltTree].trees = new AltTreesTrees[treesTemp.Length + 1];
                patches[idAltTree].treesCount = patches[idAltTree].trees.Length;


                for (int i = 0; i < treesTemp.Length; i++)
                {
                    patches[idAltTree].trees[i] = treesTemp[i];
                }

                patches[idAltTree].trees[patches[idAltTree].trees.Length - 1] = new AltTreesTrees(patches[idAltTree].trees[idTree], patches[idAltTree].trees.Length - 1);
                patches[idAltTree].trees[patches[idAltTree].trees.Length - 1].goMesh = go;


                propBlock.Clear();
                att.alphaPropBlockMesh = 1;
                att.indPropBlockMesh = 0;
                att.smoothPropBlock = 0;
                propBlock.SetFloat(Alpha_PropertyID, 1.0f);
                propBlock.SetFloat(Ind_PropertyID, 0.0f);
                propBlock.SetFloat(smoothValue_PropertyID, 0.0f);
                propBlock.SetColor(HueVariationLeave_PropertyID, att.color);
                propBlock.SetColor(HueVariationBark_PropertyID, att.colorBark);
                patches[idAltTree].trees[patches[idAltTree].trees.Length - 1].goMesh.GetComponent<MeshRenderer>().SetPropertyBlock(propBlock);

                patches[idAltTree].trees[patches[idAltTree].trees.Length - 1].idPrototypeIndex = getPrototypeIndex(patches[idAltTree].trees[patches[idAltTree].trees.Length - 1].idPrototype);

                patches[idAltTree].EditDataFileTrees(null, 1, null);

                return patches[idAltTree].trees[patches[idAltTree].trees.Length - 1];
            }
            else
            {
                AltTreesTrees att = new AltTreesTrees(patches[idAltTree].quadObjects[idQuadObject].findObjectById(idTree), -1);
                
                lock (patches[idAltTree].quadObjects[idQuadObject].treesNoBillbLock)
                {
                    patches[idAltTree].quadObjects[idQuadObject].treesNoBillbCount++;
                    patches[idAltTree].quadObjects[idQuadObject].treesNoBillb.Add(att);
                }
                att.goMesh = go;

                propBlock.Clear();
                att.alphaPropBlockMesh = 1;
                att.indPropBlockMesh = 0;
                att.smoothPropBlock = 0;
                propBlock.SetFloat(Alpha_PropertyID, 1.0f);
                propBlock.SetFloat(Ind_PropertyID, 0.0f);
                propBlock.SetFloat(smoothValue_PropertyID, 0.0f);
                propBlock.SetColor(HueVariationLeave_PropertyID, att.color);
                propBlock.SetColor(HueVariationBark_PropertyID, att.colorBark);
                att.goMesh.GetComponent<MeshRenderer>().SetPropertyBlock(propBlock);


                List<AltTreesTrees> attl = new List<AltTreesTrees>();
                attl.Add(att);

                Dictionary<int, List<AltTreesTrees>> idQuadsTemp = new Dictionary<int, List<AltTreesTrees>>();
                List<AltTreesTrees> attListT = new List<AltTreesTrees>();
                attListT.Add(att);
                idQuadsTemp.Add(att.idQuadObject, attListT);

                att.altTreesPatch.EditDataFileObjects(1, null, idQuadsTemp);

                //patches[idAltTree].EditDataFileObjects(true, null, 1, null);

                return att;
            }
        }

        public void addTreesImport(AddTreesStruct[] trees, int idAltTree, bool save)
        {
            if (patches.Length > idAltTree && patches[idAltTree] != null)
            {
                int countObjs = 0;
                if (trees.Length > 0)
                {

                    int treesBillb = 0;
                    int treesNoBillb = 0;
                    int treesBillbSch = 0;
                    int treesNoBillbSch = 0;

                    for (int i = 0; i < trees.Length; i++)
                    {
                        if (!trees[i].isObject)
                            treesBillb++;
                        else
                            treesNoBillb++;
                    }

                    AddTreesStruct[] treesBillbArr = new AddTreesStruct[treesBillb];
                    AddTreesStruct[] treesNoBillbArr = new AddTreesStruct[treesNoBillb];

                    for (int i = 0; i < trees.Length; i++)
                    {
                        if (!trees[i].isObject)
                        {
                            treesBillbArr[treesBillbSch] = trees[i];
                            treesBillbSch++;
                        }
                        else
                        {
                            treesNoBillbArr[treesNoBillbSch] = trees[i];
                            treesNoBillbSch++;
                        }
                    }
                    treesBillbSch = 0;
                    treesNoBillbSch = 0;


                    int countTemp = 0;


                    if (patches[idAltTree].trees == null)
                        patches[idAltTree].trees = new AltTreesTrees[0];


                    int countEmptyRemove = 0;
                    List<int> changedTrees = new List<int>();
                    int addedTreesCount = 0;



                    if (treesBillb > 0)
                    {
                        for (int j = 0; j < patches[idAltTree].treesEmptyCount; j++)
                        {
                            patches[idAltTree].trees[patches[idAltTree].treesEmpty[j]] = new AltTreesTrees(patches[idAltTree].getTreePosLocal(treesBillbArr[treesBillbSch].pos, jump, jumpPos, altTreesMain.altTreesManagerData.sizePatch), patches[idAltTree].treesEmpty[j], treesBillbArr[treesBillbSch].idPrototype, false, treesBillbArr[treesBillbSch].color, treesBillbArr[treesBillbSch].colorBark, treesBillbArr[treesBillbSch].rotation, treesBillbArr[treesBillbSch].heightScale, treesBillbArr[treesBillbSch].widthScale, patches[idAltTree]);
                            patches[idAltTree].trees[patches[idAltTree].treesEmpty[j]].idPrototypeIndex = getPrototypeIndex(patches[idAltTree].trees[patches[idAltTree].treesEmpty[j]].idPrototype);
                            quads[idAltTree].checkTreesAdd(patches[idAltTree].trees[patches[idAltTree].treesEmpty[j]].getPosWorld().x, patches[idAltTree].trees[patches[idAltTree].treesEmpty[j]].getPosWorld().z, patches[idAltTree].trees[patches[idAltTree].treesEmpty[j]], idAltTree);
                            countTemp++;
                            treesBillbSch++;
                            countEmptyRemove++;
                            changedTrees.Add(patches[idAltTree].treesEmpty[j]);

                            if (treesBillbSch >= treesBillb)
                                break;
                        }
                        int[] treesEmptyTemp = patches[idAltTree].treesEmpty;
                        patches[idAltTree].treesEmptyCount -= countEmptyRemove;
                        patches[idAltTree].treesEmpty = new int[patches[idAltTree].treesEmptyCount];
                        for (int i = countEmptyRemove; i < treesEmptyTemp.Length; i++)
                        {
                            patches[idAltTree].treesEmpty[i - countEmptyRemove] = treesEmptyTemp[i];
                        }
                    }

                    AltTreesTrees attTemp = null;
                    bool stopTemp = false;
                    Dictionary<int, List<AltTreesTrees>> idQuadsTemp = new Dictionary<int, List<AltTreesTrees>>();
                    Vector3 vector3Temp = new Vector3();

                    if (treesNoBillb > 0)
                    {
                        for (int j = 0; j < treesNoBillb; j++)
                        {
                            stopTemp = false;

                            attTemp = new AltTreesTrees(patches[idAltTree].getTreePosLocal(treesNoBillbArr[j].pos, jump, jumpPos, altTreesMain.altTreesManagerData.sizePatch), -1, treesNoBillbArr[j].idPrototype, true, treesNoBillbArr[j].color, treesNoBillbArr[j].colorBark, treesNoBillbArr[j].rotation, treesNoBillbArr[j].heightScale, treesNoBillbArr[j].widthScale, patches[idAltTree]);
                            vector3Temp = attTemp.getPosWorld();
                            attTemp.idPrototypeIndex = getPrototypeIndex(attTemp.idPrototype);
                            quads[idAltTree].getObjectQuadId(vector3Temp.x, vector3Temp.z, ref attTemp.idQuadObject, ref stopTemp);
                            attTemp.idTree = -1;

                            attTemp.currentLOD = -1;
                            attTemp.currentCrossFadeId = -1;

                            if (!idQuadsTemp.ContainsKey(attTemp.idQuadObject))
                            {
                                List<AltTreesTrees> attListT = new List<AltTreesTrees>();
                                attListT.Add(attTemp);
                                idQuadsTemp.Add(attTemp.idQuadObject, attListT);
                            }
                            else
                                idQuadsTemp[attTemp.idQuadObject].Add(attTemp);
                            countObjs++;


                            if (patches[idAltTree].quadObjects[attTemp.idQuadObject - 1].isInitObjects)
                                patches[idAltTree].quadObjects[attTemp.idQuadObject - 1].checkTreesAdd(vector3Temp.x, vector3Temp.z, attTemp, idAltTree, false);
                        }
                    }

                    if (treesBillbSch < treesBillb)
                    {
                        AltTreesTrees[] treesTemp = patches[idAltTree].trees;
                        patches[idAltTree].trees = new AltTreesTrees[treesTemp.Length + (treesBillb - treesBillbSch)];
                        addedTreesCount = treesBillb - treesBillbSch;
                        patches[idAltTree].treesCount = patches[idAltTree].trees.Length;

                        for (int i = 0; i < treesTemp.Length; i++)
                        {
                            patches[idAltTree].trees[i] = treesTemp[i];
                        }

                        int treesBillbSch2 = treesBillbSch;

                        for (int i = 0; i < (treesBillb - treesBillbSch); i++)
                        {
                            patches[idAltTree].trees[treesTemp.Length + i] = new AltTreesTrees(patches[idAltTree].getTreePosLocal(treesBillbArr[treesBillbSch2].pos, jump, jumpPos, altTreesMain.altTreesManagerData.sizePatch), treesTemp.Length + i, treesBillbArr[treesBillbSch2].idPrototype, false, treesBillbArr[treesBillbSch2].color, treesBillbArr[treesBillbSch2].colorBark, treesBillbArr[treesBillbSch2].rotation, treesBillbArr[treesBillbSch2].heightScale, treesBillbArr[treesBillbSch2].widthScale, patches[idAltTree]);
                            patches[idAltTree].trees[treesTemp.Length + i].idPrototypeIndex = getPrototypeIndex(patches[idAltTree].trees[treesTemp.Length + i].idPrototype);
                            quads[idAltTree].checkTreesAdd(patches[idAltTree].trees[treesTemp.Length + i].getPosWorld().x, patches[idAltTree].trees[treesTemp.Length + i].getPosWorld().z, patches[idAltTree].trees[treesTemp.Length + i], idAltTree);

                            treesBillbSch2++;
                        }
                    }

                    if (save)
                    {
                        if (treesBillb > 0)
                            patches[idAltTree].EditDataFileTrees(changedTrees, addedTreesCount, null);
                        if (treesNoBillb > 0)
                            patches[idAltTree].EditDataFileObjects(countObjs, null, idQuadsTemp);
                    }


                    //patches[idAltTree].recalculateBound();

                    //patches[idAltTree].altTrees.ReInit();
                }
                else
                    altTreesMain.LogError("positions.Length == 0");
            }
            else
                altTreesMain.LogError("altTrees.Length<=idAltTree || patches[idAltTree] == null");
        }
    }

    public struct CreateMeshBillboardsStruct
    {
        public Vector3[] verts;
        public Vector2[] uvs;
        public Vector2[] uvs2;
        public Vector2[] uvs3;
        public Color[] cols;
        public int[] indices;
        public int indexTemp;
        public float widthScaleMax;
        public float heightScaleMax;
        public AltTree at;
        public int createMeshIdStep;
        public Mesh ms;
    }

    public struct DistanceCamera
    {
        public Transform trans;
        public Vector3 pos;
        public AltTreeInstance ati;
        public bool isSelected;
        public float speed;
    }

    public struct FrustumCamera
    {
        public Camera cam;
        public Plane[] planes;
        public MyPlane[] myPlanes;
        public bool isActiveAndEnabled;
    }

    public class AltTreesQuad
    {
        public AltTreesManager manager;
        public Vector2 pos;
        public int patchID = -1;
        public int LOD = -1;
        public int maxLOD = -1;
        public int startBillboardsLOD = -1;
        public float size = -1;
        public float sizeSQR = -1;
        public bool isActiv = false;
        public bool isRender = false;
        //public List<GameObject> renders = new List<GameObject>();
        public List<MeshToRender> meshes = new List<MeshToRender>();
        public List<MeshToRender> meshesTemp = new List<MeshToRender>();
        public GameObject rendersDebug = null;
        public List<AltTreesQuad> objs = new List<AltTreesQuad>();
        public bool isInitBillboards = false;
        public bool isInitBillboardsInQueue = false;
        public bool isInitTrees = false;
        public bool isInitObjects = false;
        public bool initObjectsStarted = false;
        public int objectsQuadId = 0;
        static public List<AltTreesQuad> objsToInit = new List<AltTreesQuad>();
        public Object treesLock = new Object();
        public Object treesNoBillbLock = new Object();
        public Object treePrefabsCountLock = new Object();
        public Object billboardsGenerationLock = new Object();
        public List<AltTreesTrees> trees = new List<AltTreesTrees>();
        public List<AltTreesTrees> treesNoBillb = new List<AltTreesTrees>();
        public Dictionary<int, int> treePrefabsCount = new Dictionary<int, int>();
        public int treesCount = 0;
        public int treesNoBillbCount = 0;
        public Bounds2D bound;

        public bool isLock = false;
        public bool isUpdate = false;
        //public bool isGenerateAllBillboardsOnStart = false;

        public AltTreesQuad[] quads = new AltTreesQuad[4];
        public bool isChildQuads = false;
        public int quadId = 0;

        public bool createMeshBillboardsStarted = false;
        public bool createMeshBillboardsOk = false;
        public Object createMeshBillboardsStructsLock = new Object();
        public CreateMeshBillboardsStruct[] createMeshBillboardsStructs;
        public int createMeshBillboardsStructsCurrentId = 0;
        public int countMeshes;


        public AltTreesQuad(float x, float z, float _size, int _patchID, int _LOD, int _maxLOD, int _startBillboardsLOD, AltTreesManager _manager, int _quadId = 0)
        {
            manager = _manager;
            pos = new Vector2(x, z);
            patchID = _patchID;
            LOD = _LOD;
            size = _size;
            sizeSQR = size * size;
            maxLOD = _maxLOD;
            startBillboardsLOD = _startBillboardsLOD;

            
            isInitBillboards = false;
            isInitTrees = false;
            isInitObjects = false;
            initObjectsStarted = false;

            bound = new Bounds2D(pos, size);

            if(_quadId == -1)
            {
                manager.patches[patchID].quadObjects = new AltTreesQuad[(int)Mathf.Pow(2, 2 * maxLOD)];
                quadId = 0;
            }
            else
                quadId = _quadId;

            /*if (manager.altTreesMain.altTreesManagerData.generateAllBillboardsOnStart && manager.altTreesMain.isPlaying)
            {
                if (!isInitBillboardsInQueue && !objsToInit.Contains(this))
                {
                    isInitBillboardsInQueue = true;
                    objsToInit.Add(this);
                }
                isGenerateAllBillboardsOnStart = true;
            }*/

            if (LOD < maxLOD)
            {
                isChildQuads = true;
                quads[0] = new AltTreesQuad(x - size / 4f, z + size / 4f, size / 2f, patchID, LOD + 1, maxLOD, startBillboardsLOD, _manager, 1);
                quads[1] = new AltTreesQuad(x + size / 4f, z + size / 4f, size / 2f, patchID, LOD + 1, maxLOD, startBillboardsLOD, _manager, 2);
                quads[2] = new AltTreesQuad(x - size / 4f, z - size / 4f, size / 2f, patchID, LOD + 1, maxLOD, startBillboardsLOD, _manager, 3);
                quads[3] = new AltTreesQuad(x + size / 4f, z - size / 4f, size / 2f, patchID, LOD + 1, maxLOD, startBillboardsLOD, _manager, 4);
            }
            else
            {
                objectsQuadId = manager.patches[patchID].objectsQuadIdTemp;
                manager.patches[patchID].quadObjects[objectsQuadId - 1] = this;
                manager.patches[patchID].objectsQuadIdTemp++;
            }
        }


        bool _isNext_check = false;
        //int idCheck = 0;

        public void check(DistanceCamera[] _cameras, bool _isOk, bool isFirst = false)
        {
            if (_isOk)
            {
                lock (manager.distanceCamerasLock)
                {
                    if (manager.distanceCamerasInit)
                    {
                        for (int c = 0; c < _cameras.Length; c++)
                        {
                            _isNext_check = (AltUtilities.fastDistanceSqrt2D(ref pos, _cameras[c].pos) <= (sizeSQR + sizeSQR) * manager.altTreesMain.altTreesManagerData.distancePatchFactor /* / ((float)maxLOD - (float)LOD))*/);

                            if (_isNext_check)
                                break;
                        }
                    }
                    else
                        _isNext_check = false;
                }

                if (isLock)
                    _isNext_check = true;

                if (_isNext_check && LOD < maxLOD)
                    isActiv = false;
                else
                {
                    _isNext_check = false;
                    if(isInitTrees)
                        isActiv = true;
                }
            }
            else
            {
                _isNext_check = false;
                isActiv = false;
            }

            if (isChildQuads)
            {
                if (isFirst)
                {
                    quads[0].check(_cameras, _isNext_check);
                    quads[1].check(_cameras, _isNext_check);
                    quads[2].check(_cameras, _isNext_check);
                    quads[3].check(_cameras, _isNext_check);

                    /*
                    quads[idCheck].check(_cameras, _isNext_check);

                    idCheck++;
                    if (idCheck == 4)
                        idCheck = 0;*/
                }
                else
                {
                    quads[0].check(_cameras, _isNext_check);
                    quads[1].check(_cameras, _isNext_check);
                    quads[2].check(_cameras, _isNext_check);
                    quads[3].check(_cameras, _isNext_check);
                }
            }
        }

        public void lockQuads(Vector3 vect)
        {
            if (bound.inBounds(vect.x, vect.z, quadId))
            {
                isLock = true;

                if (isChildQuads)
                {
                    quads[0].lockQuads(vect);
                    quads[1].lockQuads(vect);
                    quads[2].lockQuads(vect);
                    quads[3].lockQuads(vect);
                }
            }
        }

        public void reInitBillboards(AltTreesTrees att, Vector3 vect, bool drawGroupBillboards = true)
        {

            if (bound.inBounds(att.getPosWorld().x, att.getPosWorld().z, quadId) && bound.inBounds(vect.x, vect.z, quadId))
            {
                if (drawGroupBillboards)
                    isUpdate = true;

                if (isChildQuads)
                {
                    quads[0].reInitBillboards(att, vect, drawGroupBillboards);
                    quads[1].reInitBillboards(att, vect, drawGroupBillboards);
                    quads[2].reInitBillboards(att, vect, drawGroupBillboards);
                    quads[3].reInitBillboards(att, vect, drawGroupBillboards);
                }
            }
            else if (bound.inBounds(att.getPosWorld().x, att.getPosWorld().z, quadId))
            {
                if (drawGroupBillboards)
                {
                    isUpdate = true;
                    lock (treePrefabsCountLock)
                    {
                        if (!treePrefabsCount.ContainsKey(att.idPrototype))
                            manager.altTreesMain.Log("- " + att.idTree + ". " + att.idPrototype + ", " + trees.Count + ", " + treesCount + " = " + att.pos.ToString() + " = " + pos.ToString());
                        else if (treePrefabsCount[att.idPrototype] <= 0)
                            manager.altTreesMain.Log("+ " + att.idTree + ". " + treePrefabsCount[att.idPrototype] + ", " + trees.Count + ", " + treesCount + " = " + att.pos.ToString() + " = " + pos.ToString());
                        treePrefabsCount[att.idPrototype]--;
                    }
                    lock (treesLock)
                    {
                        trees.Remove(att);
                        treesCount--;
                    }
                }
                else
                {
                    lock (treesNoBillbLock)
                    {
                        treesNoBillb.Remove(att);
                        treesNoBillbCount--;
                    }
                }

                if (isChildQuads)
                {
                    quads[0].reInitBillboards(att, vect, drawGroupBillboards);
                    quads[1].reInitBillboards(att, vect, drawGroupBillboards);
                    quads[2].reInitBillboards(att, vect, drawGroupBillboards);
                    quads[3].reInitBillboards(att, vect, drawGroupBillboards);
                }
            }
            else if (bound.inBounds(vect.x, vect.z, quadId))
            {
                if (drawGroupBillboards)
                {
                    isUpdate = true;
                    lock (treePrefabsCountLock)
                    {
                        if (!treePrefabsCount.ContainsKey(att.idPrototype))
                            treePrefabsCount.Add(att.idPrototype, 0);
                        treePrefabsCount[att.idPrototype]++;
                    }
                    lock (treesLock)
                    {
                        trees.Add(att);
                        treesCount++;
                    }
                }
                else
                {
                    lock (treesNoBillbLock)
                    {
                        if (isInitObjects)
                        {
                            treesNoBillb.Add(att);
                            treesNoBillbCount++;
                        }
                        att.idQuadObjectNew = objectsQuadId;
                    }
                }

                if (isChildQuads)
                {
                    quads[0].reInitBillboards(att, vect, drawGroupBillboards);
                    quads[1].reInitBillboards(att, vect, drawGroupBillboards);
                    quads[2].reInitBillboards(att, vect, drawGroupBillboards);
                    quads[3].reInitBillboards(att, vect, drawGroupBillboards);
                }
            }


        }

        public void removeTree(AltTreesTrees att, bool isObject = true)
        {
            if (bound.inBounds(att.getPosWorld().x, att.getPosWorld().z, quadId))
            {
                if (!isObject)
                {
                    isUpdate = true;
                    lock (treePrefabsCountLock)
                        treePrefabsCount[att.idPrototype]--;
                    lock (treesLock)
                    {
                        trees.Remove(att);
                        treesCount--;
                    }
                }
                else
                {
                    lock (treesNoBillbLock)
                    {
                        treesNoBillb.Remove(att);
                        treesNoBillbCount--;
                    }
                }
            }


            if (isChildQuads)
            {
                quads[0].removeTree(att, isObject);
                quads[1].removeTree(att, isObject);
                quads[2].removeTree(att, isObject);
                quads[3].removeTree(att, isObject);
            }
        }

        public void editTreeSetUpdate(AltTreesTrees att)
        {
            if (bound.inBounds(att.getPosWorld().x, att.getPosWorld().z, quadId))
            {
                isUpdate = true;
            }


            if (isChildQuads)
            {
                quads[0].editTreeSetUpdate(att);
                quads[1].editTreeSetUpdate(att);
                quads[2].editTreeSetUpdate(att);
                quads[3].editTreeSetUpdate(att);
            }
        }

        public bool removeTrees(Vector2 _pos, float _radius, AltTreesPatch _at, List<int> removedTrees, List<AltTreesTrees> removedTreesNoGroup, bool save, ref int removedTreesCount, int idPrototype = -1)
        {
            bool result = false;
            if (AltUtilities.fastDistance2D(ref pos, ref _pos) <= _radius + size * 1.42f)
            {
                AltTree _tree = null;
                if (idPrototype != -1)
                {
                    for (int i = 0; i < manager.treePrototypeIds.Length; i++)
                    {
                        if (manager.treesPoolArray[i].tree.id == idPrototype)
                        {
                            _tree = manager.treesPoolArray[i].tree;
                            break;
                        }
                    }
                }
                if (idPrototype != -1 && _tree == null)
                    return false;

                if (_tree != null)
                {
                    if (!_tree.isObject)
                    {
                        lock (treesLock)
                        {
                            for (int i = treesCount - 1; i >= 0; i--)
                            {
                                if (trees[i].idPrototype == _tree.id && trees[i].noNull)
                                {
                                    if (AltUtilities.fastDistance2D(ref _pos, trees[i].get2DPosWorld()) <= _radius)
                                    {
                                        if (save)
                                        {
                                            if (!removedTrees.Contains(trees[i].idTree))
                                                removedTrees.Add(trees[i].idTree);
                                        }
                                        else
                                            removedTreesCount++;
                                        _at.trees[trees[i].idTree] = null;
                                        isUpdate = true;
                                        lock (treePrefabsCountLock)
                                            treePrefabsCount[trees[i].idPrototype]--;

                                        deleteTreeCheckCrossFade(trees[i]);
                                        trees.Remove(trees[i]);
                                        treesCount--;
                                        result = true;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        lock (treesNoBillbLock)
                        {
                            for (int i = treesNoBillbCount - 1; i >= 0; i--)
                            {
                                if (treesNoBillb[i].idPrototype == _tree.id && treesNoBillb[i].noNull)
                                {
                                    if (AltUtilities.fastDistance2D(ref _pos, treesNoBillb[i].get2DPosWorld()) <= _radius)
                                    {
                                        if (save)
                                        {
                                            if (!removedTreesNoGroup.Contains(treesNoBillb[i]))
                                                removedTreesNoGroup.Add(treesNoBillb[i]);
                                        }
                                        //_at.treesNoGroup[treesNoBillb[i].idTree] = null;

                                        deleteTreeCheckCrossFade(treesNoBillb[i]);
                                        treesNoBillb.Remove(treesNoBillb[i]);
                                        treesNoBillbCount--;
                                        result = true;
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    lock (treesLock)
                    {
                        for (int i = treesCount - 1; i >= 0; i--)
                        {
                            if (trees[i].noNull)
                            {
                                if (AltUtilities.fastDistance2D(ref _pos, trees[i].get2DPosWorld()) <= _radius)
                                {
                                    if (save)
                                    {
                                        if (!removedTrees.Contains(trees[i].idTree))
                                            removedTrees.Add(trees[i].idTree);
                                    }
                                    else
                                        removedTreesCount++;
                                    _at.trees[trees[i].idTree] = null;
                                    isUpdate = true;
                                    lock (treePrefabsCountLock)
                                        treePrefabsCount[trees[i].idPrototype]--;

                                    deleteTreeCheckCrossFade(trees[i]);
                                    trees.Remove(trees[i]);
                                    treesCount--;
                                    result = true;
                                }
                            }
                        }
                    }
                    lock (treesNoBillbLock)
                    {
                        for (int i = treesNoBillbCount - 1; i >= 0; i--)
                        {
                            if (treesNoBillb[i].noNull)
                            {
                                if (AltUtilities.fastDistance2D(ref _pos, treesNoBillb[i].get2DPosWorld()) <= _radius)
                                {
                                    if (save)
                                    {
                                        if (!removedTreesNoGroup.Contains(treesNoBillb[i]))
                                            removedTreesNoGroup.Add(treesNoBillb[i]);
                                    }
                                    //_at.treesNoGroup[treesNoBillb[i].idTree] = null;

                                    deleteTreeCheckCrossFade(treesNoBillb[i]);
                                    treesNoBillb.Remove(treesNoBillb[i]);
                                    treesNoBillbCount--;
                                    result = true;
                                }
                            }
                        }
                    }
                }


                if (isChildQuads)
                {
                    if (quads[0].removeTrees(_pos, _radius, _at, removedTrees, removedTreesNoGroup, save, ref removedTreesCount, idPrototype))
                        result = true;
                    if (quads[1].removeTrees(_pos, _radius, _at, removedTrees, removedTreesNoGroup, save, ref removedTreesCount, idPrototype))
                        result = true;
                    if (quads[2].removeTrees(_pos, _radius, _at, removedTrees, removedTreesNoGroup, save, ref removedTreesCount, idPrototype))
                        result = true;
                    if (quads[3].removeTrees(_pos, _radius, _at, removedTrees, removedTreesNoGroup, save, ref removedTreesCount, idPrototype))
                        result = true;
                }
            }
            return result;
        }

        /*public bool removeTrees(Vector2 _pos, float sizeX, float sizeZ, AltTreesPatch _at, List<int> removedTrees, bool udpadeTreesOnScene = true)
        {
            Bounds2D boundTemp = new Bounds2D(_pos.x, _pos.x + sizeX, _pos.y + sizeZ, _pos.y);
            bool result = false;
            if (bound.isIntersection(boundTemp))
            {
                lock (treesLock)
                {
                    for (int i = treesCount - 1; i >= 0; i--)
                    {
                        EditorUtility.DisplayProgressBar("Export trees to Terrain... ", "Starting...  "+i, 0.0f);

                        removedTrees.Add(trees[i].idTree);
                        _at.trees[trees[i].idTree] = null;
                        isUpdate = true;
                        lock (treePrefabsCountLock)
                            treePrefabsCount[trees[i].idPrototype]--;
                        if (trees[i].goMesh != null)
                            Object.DestroyImmediate(trees[i].goMesh);
                        trees.Remove(trees[i]);
                        treesCount--;
                        result = true;
                    }
                }*/
                /*for (int k = 0; k < att.Length; k++)
                {
                    if (!att[k].isObject)
                    {
                        lock (treesLock)
                        {
                            for (int i = treesCount - 1; i >= 0; i--)
                            {
                                if (att[k].Equals(_at.trees[trees[i].idTree]))
                                {
                                    if (!removedTrees.Contains(trees[i].idTree))
                                        removedTrees.Add(trees[i].idTree);
                                    _at.trees[trees[i].idTree] = null;
                                    isUpdate = true;
                                    lock (treePrefabsCountLock)
                                        treePrefabsCount[trees[i].idPrototype]--;
                                    if (trees[i].goMesh != null)
                                        Object.DestroyImmediate(trees[i].goMesh);
                                    trees.Remove(trees[i]);
                                    treesCount--;
                                    result = true;
                                    i = -1;
                                }
                            }
                        }
                    }
                    else
                    {
                        lock (treesNoBillbLock)
                        {
                            for (int i = treesNoBillbCount - 1; i >= 0; i--)
                            {
                                if (att[k].Equals(treesNoBillb[i]))
                                {
                                    if (!removedTreesNoGroup.Contains(treesNoBillb[i]))
                                        removedTreesNoGroup.Add(treesNoBillb[i]);
                                    isUpdate = true;
                                    if (treesNoBillb[i].goMesh != null)
                                        Object.DestroyImmediate(treesNoBillb[i].goMesh);
                                    treesNoBillb.Remove(treesNoBillb[i]);
                                    treesNoBillbCount--;
                                    result = true;
                                    i = -1;
                                }
                            }
                        }
                    }
                }*/

                /*if (isChildQuads && udpadeTreesOnScene)
                {
                    if (quads[0].removeTrees(_pos, sizeX, sizeZ, _at, removedTrees))
                        result = true;
                    if (quads[1].removeTrees(_pos, sizeX, sizeZ, _at, removedTrees))
                        result = true;
                    if (quads[2].removeTrees(_pos, sizeX, sizeZ, _at, removedTrees))
                        result = true;
                    if (quads[3].removeTrees(_pos, sizeX, sizeZ, _at, removedTrees))
                        result = true;
                }
            }
            return result;
        }
        */
        public void getTreesForExport(Vector2 _pos, float sizeX, float sizeZ, AltTreesPatch _at, List<AltTreesTrees> attTemp2)
        {
            if (LOD == maxLOD)
            {
                Bounds2D boundTemp = new Bounds2D(_pos.x, _pos.x + sizeX, _pos.y + sizeZ, _pos.y);

                if (bound.isIntersection(boundTemp))
                {
                    lock (treesLock)
                    {
                        for (int i = treesCount - 1; i >= 0; i--)
                        {
                            if (boundTemp.inBounds(trees[i].get2DPosWorld()))
                            {
                                attTemp2.Add(trees[i]);
                            }
                        }
                    }
                    /*lock (treesNoBillbLock)
                    {
                        for (int i = treesNoBillbCount - 1; i >= 0; i--)
                        {
                            if (boundTemp.inBounds(treesNoBillb[i].get2DPosWorld()))
                            {
                                attTemp2.Add(treesNoBillb[i]);
                            }
                        }
                    }*/
                }
            }
            else
            {
                if (isChildQuads)
                {
                    quads[0].getTreesForExport(_pos, sizeX, sizeZ, _at, attTemp2);
                    quads[1].getTreesForExport(_pos, sizeX, sizeZ, _at, attTemp2);
                    quads[2].getTreesForExport(_pos, sizeX, sizeZ, _at, attTemp2);
                    quads[3].getTreesForExport(_pos, sizeX, sizeZ, _at, attTemp2);
                }
            }
        }

        public void getTrees(Vector2 _pos, float _radius, bool _trees, bool _objects, int _idPrototype, AltTreesPatch _at, List<AltTreesTrees> _attTemp)
        {
            if (LOD == maxLOD)
            {
                if (AltUtilities.fastDistance2D(ref pos, ref _pos) <= _radius + size * 1.42f)
                {

                    AltTree _tree = null;
                    if (_idPrototype != -1)
                    {
                        for (int i = 0; i < manager.treePrototypeIds.Length; i++)
                        {
                            if (manager.treesPoolArray[i].tree.id == _idPrototype)
                            {
                                _tree = manager.treesPoolArray[i].tree;
                                break;
                            }
                        }
                    }
                    if (_tree != null)
                    {
                        if (!_tree.isObject)
                        {
                            lock (treesLock)
                            {
                                for (int i = treesCount - 1; i >= 0; i--)
                                {
                                    if (trees[i].idPrototype == _tree.id && trees[i].noNull)
                                    {
                                        if (AltUtilities.fastDistance2D(ref _pos, trees[i].get2DPosWorld()) <= _radius)
                                        {
                                            _attTemp.Add(trees[i]);
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            lock (treesNoBillbLock)
                            {
                                for (int i = treesNoBillbCount - 1; i >= 0; i--)
                                {
                                    if (treesNoBillb[i].idPrototype == _tree.id && treesNoBillb[i].noNull)
                                    {
                                        if (AltUtilities.fastDistance2D(ref _pos, treesNoBillb[i].get2DPosWorld()) <= _radius)
                                        {
                                            _attTemp.Add(treesNoBillb[i]);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        if (_trees)
                        {
                            lock (treesLock)
                            {
                                for (int i = treesCount - 1; i >= 0; i--)
                                {
                                    if (trees[i].noNull)
                                    {
                                        if (AltUtilities.fastDistance2D(ref _pos, trees[i].get2DPosWorld()) <= _radius)
                                        {
                                            _attTemp.Add(trees[i]);
                                        }
                                    }
                                }
                            }
                        }
                        if (_objects)
                        {
                            lock (treesNoBillbLock)
                            {
                                for (int i = treesNoBillbCount - 1; i >= 0; i--)
                                {
                                    if (treesNoBillb[i].noNull)
                                    {
                                        if (AltUtilities.fastDistance2D(ref _pos, treesNoBillb[i].get2DPosWorld()) <= _radius)
                                        {
                                            _attTemp.Add(treesNoBillb[i]);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                if (isChildQuads)
                {
                    quads[0].getTrees(_pos, _radius, _trees, _objects, _idPrototype, _at, _attTemp);
                    quads[1].getTrees(_pos, _radius, _trees, _objects, _idPrototype, _at, _attTemp);
                    quads[2].getTrees(_pos, _radius, _trees, _objects, _idPrototype, _at, _attTemp);
                    quads[3].getTrees(_pos, _radius, _trees, _objects, _idPrototype, _at, _attTemp);
                }
            }
        }

        public void getAllTrees(bool _trees, bool _objects, int _idPrototype, AltTreesPatch _at, List<AltTreesTrees> _attTemp)
        {
            if (LOD == maxLOD)
            {
                AltTree _tree = null;
                if (_idPrototype != -1)
                {
                    for (int i = 0; i < manager.treePrototypeIds.Length; i++)
                    {
                        if (manager.treesPoolArray[i].tree.id == _idPrototype)
                        {
                            _tree = manager.treesPoolArray[i].tree;
                            break;
                        }
                    }
                }
                if (_tree != null)
                {
                    if (!_tree.isObject)
                    {
                        lock (treesLock)
                        {
                            for (int i = treesCount - 1; i >= 0; i--)
                            {
                                if (trees[i].idPrototype == _tree.id && trees[i].noNull)
                                {
                                    _attTemp.Add(trees[i]);
                                }
                            }
                        }
                    }
                    else
                    {
                        lock (treesNoBillbLock)
                        {
                            for (int i = treesNoBillbCount - 1; i >= 0; i--)
                            {
                                if (treesNoBillb[i].idPrototype == _tree.id && treesNoBillb[i].noNull)
                                {
                                    _attTemp.Add(treesNoBillb[i]);
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (_trees)
                    {
                        lock (treesLock)
                        {
                            for (int i = treesCount - 1; i >= 0; i--)
                            {
                                if (trees[i].noNull)
                                {
                                    _attTemp.Add(trees[i]);
                                }
                            }
                        }
                    }
                    if (_objects)
                    {
                        lock (treesNoBillbLock)
                        {
                            for (int i = treesNoBillbCount - 1; i >= 0; i--)
                            {
                                if (treesNoBillb[i].noNull)
                                {
                                    _attTemp.Add(treesNoBillb[i]);
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                if (isChildQuads)
                {
                    quads[0].getAllTrees(_trees, _objects, _idPrototype, _at, _attTemp);
                    quads[1].getAllTrees(_trees, _objects, _idPrototype, _at, _attTemp);
                    quads[2].getAllTrees(_trees, _objects, _idPrototype, _at, _attTemp);
                    quads[3].getAllTrees(_trees, _objects, _idPrototype, _at, _attTemp);
                }
            }
        }

        public void unlockQuads()
        {
            isLock = false;

            if (isChildQuads)
            {
                quads[0].unlockQuads();
                quads[1].unlockQuads();
                quads[2].unlockQuads();
                quads[3].unlockQuads();
            }
        }

        public void goUpdateTrees()
        {
            if (isUpdate)
            {
                if (isInitBillboards || isInitBillboardsInQueue)
                {
                    lock (billboardsGenerationLock)
                    {
                        meshes.Clear();
                        meshesTemp.Clear();
                        
                        manager.createMeshBillboards(this);

                        createMeshBillboardsStructsCurrentId = -1;

                        createMeshBillboardsStructs = null;

                        isInitBillboards = true;
                        //isGenerateAllBillboardsOnStart = false;
                        isInitBillboardsInQueue = false;

                        if (objsToInit.Contains(this))
                            objsToInit.Remove(this);
                    }
                }
            }

            isUpdate = false;

            if (isChildQuads)
            {
                quads[0].goUpdateTrees();
                quads[1].goUpdateTrees();
                quads[2].goUpdateTrees();
                quads[3].goUpdateTrees();
            }
        }

        bool _isNext_checkObjs = false;

        public void checkObjs(AltTreesQuad _obj, DistanceCamera[] _cameras, bool enabledThreading, bool isPlaying)
        {
            objs.Clear();
            
            if (_obj == null)
            {
                if (isActiv)
                {
                    if (isRender)
                    {
                        if (LOD > startBillboardsLOD)
                            checkTrees(_cameras, enabledThreading, isPlaying);
                        return;
                    }
                    else
                    {
                        if (isInitBillboards)
                        {
                            if (isChildQuads)
                            {
                                quads[0].checkObjs(this, _cameras, enabledThreading, isPlaying);
                                quads[1].checkObjs(this, _cameras, enabledThreading, isPlaying);
                                quads[2].checkObjs(this, _cameras, enabledThreading, isPlaying);
                                quads[3].checkObjs(this, _cameras, enabledThreading, isPlaying);
                            }
                        }
                        else
                        {
                            if (!isInitBillboardsInQueue && !objsToInit.Contains(this))
                            {
                                isInitBillboardsInQueue = true;
                                objsToInit.Add(this);
                            }
                            return;
                        }
                    }
                }
                else
                {
                    if (isRender)
                    {
                        if (isChildQuads)
                        {
                            quads[0].checkObjs(this, _cameras, enabledThreading, isPlaying);
                            quads[1].checkObjs(this, _cameras, enabledThreading, isPlaying);
                            quads[2].checkObjs(this, _cameras, enabledThreading, isPlaying);
                            quads[3].checkObjs(this, _cameras, enabledThreading, isPlaying);
                        }
                    }
                    else
                    {
                        if (isChildQuads)
                        {
                            quads[0].checkObjs(null, _cameras, enabledThreading, isPlaying);
                            quads[1].checkObjs(null, _cameras, enabledThreading, isPlaying);
                            quads[2].checkObjs(null, _cameras, enabledThreading, isPlaying);
                            quads[3].checkObjs(null, _cameras, enabledThreading, isPlaying);
                        }

                        return;
                    }
                }

                if (isActiv && isInitBillboards && !isRender)
                {
                    if (LOD <= startBillboardsLOD)
                    {
                        manager.quadsToRender.Add(this);
                        manager.needUpdateScene = true;
                    }
                    else
                        checkTrees(_cameras, enabledThreading, isPlaying);

                    if (manager.altTreesMain.altTreesManagerData.drawDebugPatches)
                    {
                        rendersDebug.SetActive(true);
                    }
                    for (int i = 0; i < objs.Count; i++)
                    {
                        if (manager.quadsToRender.Contains(objs[i]))
                            manager.quadsToRender.Remove(objs[i]);

                        /*
                        for (int j = 0; j < objs[i].renders.Count; j++)
                        {
                            objs[i].renders[j].SetActive(false);
                        }
                        */

                        if (LOD <= startBillboardsLOD && objs[i].LOD > startBillboardsLOD)
                            objs[i].deleteTrees();
                        if (manager.altTreesMain.altTreesManagerData.drawDebugPatches)
                        {
                            objs[i].rendersDebug.SetActive(false);
                        }
                        objs[i].isRender = false;
                    }
                    isRender = true;
                }
                else if (!isActiv && isRender)
                {
                    _isNext_checkObjs = true;
                    for (int i = 0; i < objs.Count; i++)
                    {
                        if (!objs[i].isInitBillboards)
                        {
                            _isNext_checkObjs = false;
                            break;
                        }
                    }

                    if (_isNext_checkObjs)
                    {
                        for (int i = 0; i < objs.Count; i++)
                        {
                            if (objs[i].LOD > startBillboardsLOD)
                            {
                                objs[i].checkTrees(_cameras, enabledThreading, isPlaying, true);
                            }
                            else
                            {
                                manager.quadsToRender.Add(objs[i]);
                            }


                            if (manager.altTreesMain.altTreesManagerData.drawDebugPatches)
                            {
                                objs[i].rendersDebug.SetActive(true);
                            }
                            objs[i].isRender = true;
                        }

                        if (manager.quadsToRender.Contains(this))
                            manager.quadsToRender.Remove(this);
                        /*
                        for (int i = 0; i < renders.Count; i++)
                        {
                            renders[i].SetActive(false);
                        }*/
                        if (manager.altTreesMain.altTreesManagerData.drawDebugPatches)
                        {
                            rendersDebug.SetActive(false);
                        }
                        isRender = false;

                        manager.needUpdateScene = true;
                    }
                }
            }
            else
            {
                if (_obj.isActiv)
                {
                    if (isRender)
                    {
                        _obj.objs.Add(this);
                    }
                    else
                    {
                        if (isChildQuads)
                        {
                            quads[0].checkObjs(_obj, _cameras, enabledThreading, isPlaying);
                            quads[1].checkObjs(_obj, _cameras, enabledThreading, isPlaying);
                            quads[2].checkObjs(_obj, _cameras, enabledThreading, isPlaying);
                            quads[3].checkObjs(_obj, _cameras, enabledThreading, isPlaying);
                        }
                    }
                }
                else
                {
                    if (isActiv)
                    {
                        _obj.objs.Add(this);

                        if (!isInitBillboards && !isInitBillboardsInQueue)
                        {
                            if (!objsToInit.Contains(this))
                            {
                                isInitBillboardsInQueue = true;
                                objsToInit.Add(this);
                            }
                        }
                    }
                    else
                    {
                        if (isChildQuads)
                        {
                            quads[0].checkObjs(_obj, _cameras, enabledThreading, isPlaying);
                            quads[1].checkObjs(_obj, _cameras, enabledThreading, isPlaying);
                            quads[2].checkObjs(_obj, _cameras, enabledThreading, isPlaying);
                            quads[3].checkObjs(_obj, _cameras, enabledThreading, isPlaying);
                        }
                    }
                }
            }
        }
        
        public void checkObjsTimer(DistanceCamera[] _cameras, bool isPlaying, bool enableThreading)
        {
            if (isActiv)
            {
                if (LOD > startBillboardsLOD)
                    checkTreesTimer(_cameras, isPlaying, enableThreading);
                return;
            }
            else
            {
                if (isChildQuads)
                {
                    quads[0].checkObjsTimer(_cameras, isPlaying, enableThreading);
                    quads[1].checkObjsTimer(_cameras, isPlaying, enableThreading);
                    quads[2].checkObjsTimer(_cameras, isPlaying, enableThreading);
                    quads[3].checkObjsTimer(_cameras, isPlaying, enableThreading);
                }
            }
        }
        
        public void checkDebugPutches(bool drawDebugPutches)
        {
            if (isActiv && rendersDebug != null)
            {
                rendersDebug.SetActive(drawDebugPutches);
            }

            if (isChildQuads)
            {
                quads[0].checkDebugPutches(drawDebugPutches);
                quads[1].checkDebugPutches(drawDebugPutches);
                quads[2].checkDebugPutches(drawDebugPutches);
                quads[3].checkDebugPutches(drawDebugPutches);
            }
        }

        public void setFloatingOriginJump(float x, float z)
        {
            pos.x = x;
            pos.y = z;

            bound.left = pos.x - size / 2f;
            bound.right = pos.x + size / 2f;

            bound.up = pos.y + size / 2f;
            bound.down = pos.y - size / 2f;

            if (isChildQuads)
            {
                quads[0].setFloatingOriginJump(x - size / 4f, z + size / 4f);
                quads[1].setFloatingOriginJump(x + size / 4f, z + size / 4f);
                quads[2].setFloatingOriginJump(x - size / 4f, z - size / 4f);
                quads[3].setFloatingOriginJump(x + size / 4f, z - size / 4f);
            }
        }

        float distTemp = 0;
        float distTemp2 = 0;
        int newLOD = 0;

        Vector3 scaleTemp = new Vector3();
        Vector3 posWorldTemp;
        float sizePatchSquare = 0f;

        public int countPercentes = 0;
        Vector3 _temp;

        public Vector3 getPosLocal(Vector3 _pos)
        {
            //_temp = (_pos - (manager.jump - manager.altTrees[altTreesID].step) * manager.altTrees[altTreesID].altTrees.altTreesManagerData.sizePatch) / manager.altTrees[altTreesID].altTrees.altTreesManagerData.sizePatch;
            //_temp = manager.jump - manager.altTrees[altTreesID].step + _pos / manager.altTrees[altTreesID].altTrees.altTreesManagerData.sizePatch;
            _temp = manager.jump - manager.patches[patchID].step + (_pos - manager.jumpPos) / manager.patches[patchID].altTrees.altTreesManagerData.sizePatch;

            return _temp;
        }

        int sum_timer = 0;
        float sizePatchSquare_timer = 0;
        float distTemp_timer = 0;
        float distTemp2_timer = 0;
        int newLodTemp_timer = 0;
        bool isStop_timer = false;
        AltTreesTrees treeTemp_timer = null;
        Vector3[] posCamsLocalTemp_timer;
        int i_timer = 0;
        int c_timer = 0;

        int ot_timer = 0;
        int otDo_timer = 0;
        int countPercentes_timer = 0;

        void checkTreesTimer(DistanceCamera[] _cameras, bool isPlaying, bool enabledThreading)
        {
            lock (treesLock)
            {
                lock (treesNoBillbLock)
                {
                    sum_timer = treesCount + (isInitObjects ? treesNoBillbCount : 0);
                }
            }

            if (sum_timer == 0)
                return;

            lock (manager.distanceCamerasLock)
            {
                if(manager.distanceCamerasInit)
                    posCamsLocalTemp_timer = new Vector3[_cameras.Length];
            }

            sizePatchSquare_timer = manager.patches[patchID].altTrees.altTreesManagerData.sizePatch * manager.patches[patchID].altTrees.altTreesManagerData.sizePatch;  //?

            lock (manager.distanceCamerasLock)
            {
                if (manager.distanceCamerasInit)
                {
                    for (i_timer = 0; i_timer < _cameras.Length; i_timer++)
                    {
                        posCamsLocalTemp_timer[i_timer] = getPosLocal(_cameras[i_timer].pos);
                    }
                }
            }

            ot_timer = Mathf.FloorToInt(countPercentes_timer * ((sum_timer / 100f) * manager.altTreesMain.altTreesManagerData.getCheckTreesPerFramePercent() * 4f));
            countPercentes_timer++;
            otDo_timer = Mathf.FloorToInt(countPercentes_timer * ((sum_timer / 100f) * manager.altTreesMain.altTreesManagerData.getCheckTreesPerFramePercent() * 4f));
            if (otDo_timer >= sum_timer)
            {
                otDo_timer = sum_timer;
                countPercentes_timer = 0;
            }


            for (i_timer = ot_timer; i_timer < otDo_timer; i_timer++)
            {
                lock (treesLock)
                {
                    lock (treesNoBillbLock)
                    {
                        if (i_timer < treesCount)
                        {
                            treeTemp_timer = trees[i_timer];
                            isObject_CheckTrees_timer = false;
                        }
                        else
                        {
                            if (i_timer - treesCount < treesNoBillbCount)
                            {
                                treeTemp_timer = treesNoBillb[i_timer - treesCount];
                                isObject_CheckTrees_timer = true;
                            }
                            else
                            {
                                countPercentes_timer = 0;
                                //altTreesMain.LogError("Error");
                                return;
                            }
                        }
                    }
                }

                isStop_timer = false;

                /*#if UNITY_EDITOR
                    if (isPlaying)
                    {
                        lock (manager.camerasLock)
                        {
                            if (manager.cameras.Length > 1 && treeTemp_timer.goMesh != null)
                            {
                                for (c_timer = 0; c_timer < manager.cameras.Length; c_timer++)
                                {
                                    if (manager.cameras[c_timer].Equals(treeTemp_timer.goMesh.transform))
                                    {
                                        isStop_timer = true;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                #endif*/

                if (!isStop_timer)
                {
                    distTemp_timer = 100000000;

                    if (isObject_CheckTrees_timer && manager.treesPoolArray[treeTemp_timer.idPrototypeIndex].tree.densityObjects != 0f)
                        floatTemp_timer = manager.altTreesMain.altTreesManagerData.densityObjects * (3f - manager.treesPoolArray[treeTemp_timer.idPrototypeIndex].tree.densityObjects * 2f);
                    else
                        floatTemp_timer = 101f;

                    if (!isObject_CheckTrees_timer || treeTemp_timer.densityObjects <= floatTemp_timer)
                    {
                        lock (manager.distanceCamerasLock)
                        {
                            if (manager.distanceCamerasInit)
                            {
                                for (c_timer = 0; c_timer < _cameras.Length; c_timer++)
                                {
                                    distTemp2_timer = AltUtilities.fastDistanceSqrt(treeTemp_timer.pos, posCamsLocalTemp_timer[c_timer]);
                                    if (distTemp2_timer < distTemp_timer)
                                        distTemp_timer = distTemp2_timer;
                                }
                            }
                            else
                                distTemp_timer = 100000000;
                        }
                        distTemp_timer *= sizePatchSquare_timer;
                        distTemp_timer /= ((i_timer < treesCount) ? manager.altTreesMain.altTreesManagerData.distanceTreesLODFactor : manager.altTreesMain.altTreesManagerData.distanceObjectsLODFactor);


                        newLodTemp_timer = 0;
                        if (treeTemp_timer.idPrototypeIndex != -1)
                        {
                            for (c_timer = 0; c_timer < manager.treesPoolArray[treeTemp_timer.idPrototypeIndex].tree.distancesSquares.Length; c_timer++)
                            {
                                if (distTemp_timer > manager.treesPoolArray[treeTemp_timer.idPrototypeIndex].tree.distancesSquares[c_timer] * treeTemp_timer.maxScaleSquare)
                                    newLodTemp_timer = c_timer + 1;
                            }
                            if (manager.treesPoolArray[treeTemp_timer.idPrototypeIndex].tree.drawPlaneBillboard && newLodTemp_timer == manager.treesPoolArray[treeTemp_timer.idPrototypeIndex].tree.distancesSquares.Length && distTemp_timer > manager.treesPoolArray[treeTemp_timer.idPrototypeIndex].tree.distancePlaneBillboardSquare * treeTemp_timer.maxScaleSquare)
                                newLodTemp_timer = -2;
                            if (manager.treesPoolArray[treeTemp_timer.idPrototypeIndex].tree.isObject && newLodTemp_timer == -2 && distTemp_timer > manager.treesPoolArray[treeTemp_timer.idPrototypeIndex].tree.distanceCullingSquare * treeTemp_timer.maxScaleSquare)
                                newLodTemp_timer = -3;

                            if(newLodTemp_timer >= 0)
                            {
                                if (newLodTemp_timer < manager.altTreesMain.maxLODTreesAndObjects)
                                {
                                    newLodTemp_timer = manager.altTreesMain.maxLODTreesAndObjects;
                                    if (newLodTemp_timer >= manager.treesPoolArray[treeTemp_timer.idPrototypeIndex].tree.distancesSquares.Length)
                                    {
                                        if (manager.altTreesMain.useBillboardsWhenMaxLODTreesAndObjects)
                                        {
                                            if (manager.treesPoolArray[treeTemp_timer.idPrototypeIndex].tree.drawPlaneBillboard)
                                                newLodTemp_timer = -2;
                                            else if (manager.treesPoolArray[treeTemp_timer.idPrototypeIndex].tree.isObject)
                                                newLodTemp_timer = -3;
                                            else
                                                newLodTemp_timer = manager.treesPoolArray[treeTemp_timer.idPrototypeIndex].tree.distancesSquares.Length - 1;
                                        }
                                        else
                                            newLodTemp_timer = manager.treesPoolArray[treeTemp_timer.idPrototypeIndex].tree.distancesSquares.Length - 1;
                                    }
                                }
                            }
                            

                            treeTemp_timer.newLOD = newLodTemp_timer;
                            treeTemp_timer.distance = distTemp_timer;
                        }
                    }
                    else
                        treeTemp_timer.newLOD = -3;
                }
            }
        }


        MaterialPropertyBlock propBlock = new MaterialPropertyBlock();
        int sum_checkTrees = 0;
        float floatTemp_CheckTrees = 0f;
        bool isObject_CheckTrees = false;
        float floatTemp_timer = 0f;
        bool isObject_CheckTrees_timer = false;

        void checkTrees(DistanceCamera[] _cameras, bool enabledThreading, bool isPlaying, bool forceTrees = false)
        {
            lock (treesLock)
            {
                lock (treesNoBillbLock)
                {
                    sum_checkTrees = treesCount + (isInitObjects ? treesNoBillbCount : 0);
                }
            }
            
            if (!isInitObjects)
                manager.patches[patchID].initObjects(this);

            if (sum_checkTrees == 0)
                return;

            bool isStop = false;
            AltTreesTrees treeTemp = null;

            Vector3[] posCamsLocalTemp = null;
            lock (manager.distanceCamerasLock)
            {
                if (manager.distanceCamerasInit)
                    posCamsLocalTemp = new Vector3[_cameras.Length];
            }

            if (!enabledThreading)
            {
                sizePatchSquare = manager.patches[patchID].altTrees.altTreesManagerData.sizePatch * manager.patches[patchID].altTrees.altTreesManagerData.sizePatch;

                lock (manager.distanceCamerasLock)
                {
                    if (manager.distanceCamerasInit)
                    {
                        for (int c = 0; c < _cameras.Length; c++)
                        {
                            posCamsLocalTemp[c] = getPosLocal(_cameras[c].pos);
                        }
                    }
                }
            }

            int ot = Mathf.FloorToInt(countPercentes * ((sum_checkTrees / 100f) * manager.altTreesMain.altTreesManagerData.getCheckTreesPerFramePercent()));
            countPercentes++;
            int otDo = Mathf.FloorToInt(countPercentes * ((sum_checkTrees / 100f) * manager.altTreesMain.altTreesManagerData.getCheckTreesPerFramePercent()));
            if (otDo >= sum_checkTrees)
            {
                otDo = sum_checkTrees;
                countPercentes = 0;
            }

            if(forceTrees)
            {
                ot = 0;
                lock (treesLock)
                {
                    otDo = treesCount;
                }
            }

            for (int i = ot; i < otDo; i++)
            {
                lock (treesLock)
                {
                    lock (treesNoBillbLock)
                    {
                        if (i < trees.Count)
                        {
                            treeTemp = trees[i];
                            isObject_CheckTrees = false;
                        }
                        else
                        {
                            if (i - treesCount < treesNoBillb.Count)
                            {
                                treeTemp = treesNoBillb[i - treesCount];
                                isObject_CheckTrees = true;
                            }
                            else
                            {
                                countPercentes = 0;
                                manager.altTreesMain.LogError("Error");
                                return;
                            }
                        }
                    }
                }

                if (!treeTemp.noNull)
                    continue;

                isStop = false;

                if (!enabledThreading)
                {
                    #if UNITY_EDITOR
                        if (!isPlaying)
                        {
                            lock (manager.distanceCamerasLock)
                            {
                                if (manager.distanceCamerasInit)
                                {
                                    if (_cameras.Length > 1 && treeTemp.goMesh != null)
                                    {
                                        for (int c = 0; c < _cameras.Length; c++)
                                        {
                                            if (_cameras[c].trans.Equals(treeTemp.goMesh.transform))
                                            {
                                                isStop = true;
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    #endif
                }

                if (!isStop)
                {
                    if (!enabledThreading)
                    {
                        distTemp = 100000000;

                        if (isObject_CheckTrees && manager.treesPoolArray[treeTemp.idPrototypeIndex].tree.densityObjects != 0f)
                            floatTemp_CheckTrees = manager.altTreesMain.altTreesManagerData.densityObjects * (3f - manager.treesPoolArray[treeTemp.idPrototypeIndex].tree.densityObjects * 2f);
                        else
                            floatTemp_CheckTrees = 101f;

                        if (!isObject_CheckTrees || treeTemp.densityObjects <= floatTemp_CheckTrees)
                        {

                            lock (manager.distanceCamerasLock)
                            {
                                if (manager.distanceCamerasInit)
                                {
                                    for (int c = 0; c < _cameras.Length; c++)
                                    {
                                        distTemp2 = AltUtilities.fastDistanceSqrt(treeTemp.pos, posCamsLocalTemp[c]);
                                        if (distTemp2 < distTemp)
                                            distTemp = distTemp2;
                                    }
                                }
                                else
                                {
                                    distTemp = 100000000;
                                }
                            }
                            distTemp *= sizePatchSquare;
                            distTemp /= ((i < treesCount) ? manager.altTreesMain.altTreesManagerData.distanceTreesLODFactor : manager.altTreesMain.altTreesManagerData.distanceObjectsLODFactor);


                            newLOD = 0;
                            for (int j = 0; j < manager.treesPoolArray[treeTemp.idPrototypeIndex].tree.distancesSquares.Length; j++)
                            {
                                if (distTemp > manager.treesPoolArray[treeTemp.idPrototypeIndex].tree.distancesSquares[j] * treeTemp.maxScaleSquare)
                                    newLOD = j + 1;
                            }
                            if (manager.treesPoolArray[treeTemp.idPrototypeIndex].tree.drawPlaneBillboard && newLOD == manager.treesPoolArray[treeTemp.idPrototypeIndex].tree.distancesSquares.Length && distTemp > manager.treesPoolArray[treeTemp.idPrototypeIndex].tree.distancePlaneBillboardSquare * treeTemp.maxScaleSquare)
                                newLOD = -2;
                            if (manager.treesPoolArray[treeTemp.idPrototypeIndex].tree.isObject && newLOD == -2 && distTemp > manager.treesPoolArray[treeTemp.idPrototypeIndex].tree.distanceCullingSquare * treeTemp.maxScaleSquare)
                                newLOD = -3;
                            
                            if (newLOD >= 0)
                            {
                                if (newLOD < manager.altTreesMain.maxLODTreesAndObjects)
                                {
                                    newLOD = manager.altTreesMain.maxLODTreesAndObjects;
                                    if (newLOD >= manager.treesPoolArray[treeTemp.idPrototypeIndex].tree.distancesSquares.Length)
                                    {
                                        if (manager.altTreesMain.useBillboardsWhenMaxLODTreesAndObjects)
                                        {
                                            if (manager.treesPoolArray[treeTemp.idPrototypeIndex].tree.drawPlaneBillboard)
                                                newLOD = -2;
                                            else if (manager.treesPoolArray[treeTemp.idPrototypeIndex].tree.isObject)
                                                newLOD = -3;
                                            else
                                                newLOD = manager.treesPoolArray[treeTemp.idPrototypeIndex].tree.distancesSquares.Length - 1;
                                        }
                                        else
                                            newLOD = manager.treesPoolArray[treeTemp.idPrototypeIndex].tree.distancesSquares.Length - 1;
                                    }
                                }
                            }
                        }
                        else
                            newLOD = -3;
                    }
                    else
                        newLOD = treeTemp.newLOD;

                    if(newLOD != -1)
                    {
                        if (treeTemp.currentLOD == -1)
                        {
                            if (newLOD != -2 && newLOD != -3)
                            {
                                //treeTemp.go = manager.getObjMesh(treeTemp.idPrototype, newLOD, altTreesID, treeTemp.idTree, treeTemp.color, treeTemp.colorBark);
                                if (!isPlaying)//
                                {
                                    treeTemp.goMesh = manager.getObjMeshEditor(treeTemp, newLOD);
                                }
                                else
                                    manager.getObjMesh(treeTemp);
                            
                            }
                            else if (newLOD != -3)
                            {
                                //treeTemp.go = manager.getObjBillboard(treeTemp.idPrototype, treeTemp.widthScale, treeTemp.heightScale, treeTemp.rotation, treeTemp.color);
                                manager.getObjBillboard(treeTemp);

                                manager.needUpdateScene = true;
                                //manager.treesList.Add(treeTemp.go, treeTemp);
                            }



                            if (newLOD != -2 && newLOD != -3)
                            {
                                if (isPlaying && manager.altTreesMain.altTreesManagerData.enableColliders)
                                {
                                    if (manager.treesPoolArray[treeTemp.idPrototypeIndex].tree.isColliders)
                                    {
                                        treeTemp.collider = manager.getColliderPool(treeTemp.idPrototype, false, treeTemp);

                                        scaleTemp.x = treeTemp.widthScale;
                                        scaleTemp.y = treeTemp.heightScale;
                                        scaleTemp.z = treeTemp.widthScale;
                                        treeTemp.collider.go.transform.localScale = scaleTemp;
                                        treeTemp.collider.go.transform.position = treeTemp.getPosWorld();
                                        treeTemp.collider.go.transform.rotation = Quaternion.AngleAxis(treeTemp.rotation, Vector3.up);
                                    }
                                }
                            }
                            else if (newLOD != -3)
                            {
                                /*treeTemp.go.transform.localScale = Vector3.one;
                                treeTemp.go.transform.position = treeTemp.getPosWorld() + new Vector3(0f, manager.treesPoolArray[treeTemp.idPrototypeIndex].tree.size * treeTemp.heightScale / 2f /*- (manager.treesPoolArray[treeTemp.idPrototypeIndex].tree.size * treeTemp.heightScale - manager.treesPoolArray[treeTemp.idPrototypeIndex].tree.size * treeTemp.heightScale / manager.treesPoolArray[treeTemp.idPrototypeIndex].tree.space) / 2f*/ /*+ manager.treesPoolArray[treeTemp.idPrototypeIndex].tree.up * treeTemp.heightScale, 0f);
                                treeTemp.go.transform.rotation = Quaternion.identity;
                                treeTemp.go.SetActive(true);*/

                                if (isPlaying && manager.altTreesMain.altTreesManagerData.enableColliders)
                                {
                                    if (manager.treesPoolArray[treeTemp.idPrototypeIndex].tree.isBillboardColliders)
                                    {
                                        treeTemp.collider = manager.getColliderPool(treeTemp.idPrototype, true, treeTemp);
                                        scaleTemp.x = treeTemp.widthScale;
                                        scaleTemp.y = treeTemp.heightScale;
                                        scaleTemp.z = treeTemp.widthScale;
                                        treeTemp.collider.go.transform.localScale = scaleTemp;
                                        treeTemp.collider.go.transform.position = treeTemp.getPosWorld();
                                        treeTemp.collider.go.transform.rotation = Quaternion.AngleAxis(treeTemp.rotation, Vector3.up);
                                    }
                                }
                            }

                            treeTemp.currentLOD = newLOD;
                        }
                        else
                        {
                            #if UNITY_EDITOR
                                if (!isPlaying)
                                {
                                    lock(manager.distanceCamerasLock)
                                    {
                                        if (_cameras.Length > 1 && manager.isSelectionTree && treeTemp.goMesh != null)
                                        {
                                            for (int j = 0; j < _cameras.Length; j++)
                                            {
                                                if (_cameras[j].trans.Equals(treeTemp.goMesh.transform))
                                                {
                                                    isStop = true;
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }
                            #endif

                            if (!isStop)
                            {
                                if (treeTemp.currentLOD != newLOD)
                                {
                                    bool is_3_OR_4_CrossFade = (treeTemp.currentCrossFadeId == 3 || treeTemp.currentCrossFadeId == 4);
                                    if (treeTemp.currentLOD != -2 && treeTemp.currentLOD != -3)      //  currentLOD is mesh
                                    {
                                        if (newLOD == -2)               //  newLOD is billboard
                                        {
                                            if (isPlaying && manager.altTreesMain.altTreesManagerData.enableColliders)
                                            {
                                                if (!manager.treesPoolArray[treeTemp.idPrototypeIndex].tree.isCollidersEqual)
                                                {
                                                    if (manager.treesPoolArray[treeTemp.idPrototypeIndex].tree.isColliders)
                                                    {
                                                        manager.delColliderPool(treeTemp.idPrototype, treeTemp.collider, false, treeTemp);
                                                    }
                                                    if (manager.treesPoolArray[treeTemp.idPrototypeIndex].tree.isBillboardColliders)
                                                    {
                                                        treeTemp.collider = manager.getColliderPool(treeTemp.idPrototype, true, treeTemp);
                                                        scaleTemp.x = treeTemp.widthScale;
                                                        scaleTemp.y = treeTemp.heightScale;
                                                        scaleTemp.z = treeTemp.widthScale;
                                                        treeTemp.collider.go.transform.localScale = scaleTemp;
                                                        treeTemp.collider.go.transform.position = treeTemp.getPosWorld();
                                                        treeTemp.collider.go.transform.rotation = Quaternion.AngleAxis(treeTemp.rotation, Vector3.up);
                                                    }
                                                }
                                                else
                                                {
                                                    manager.drawCollidersCount--;
                                                    manager.drawColliderBillboardsCount++;

                                                    manager.treesPoolArray[treeTemp.idPrototypeIndex].colliderBillboardsMaxCount = Mathf.Max(manager.treesPoolArray[treeTemp.idPrototypeIndex].colliderBillboardsMaxCount, manager.drawColliderBillboardsCount);
                                                }
                                            }

                                            if (manager.treesPoolArray[treeTemp.idPrototypeIndex].tree.isBillboardCrossFade && !manager.isSelectionTree)
                                            {
                                                if (treeTemp.currentCrossFadeId == -1)
                                                {
                                                    manager.treesCrossFade.Add(treeTemp);
                                                    treeTemp.currentCrossFadeId = 2;
                                                    treeTemp.currentCrossFadeLOD = treeTemp.currentLOD;
                                                    if (!isPlaying)//
                                                    {
                                                        treeTemp.goMesh.GetComponent<AltTreeInstance>().isCrossFade = true;
                                                        treeTemp.crossFadeTreeMeshRenderer = treeTemp.goMesh.GetComponent<MeshRenderer>();
                                                        treeTemp.crossFadeTreeMeshRenderer.sharedMaterials = manager.treesPoolArray[treeTemp.idPrototypeIndex].objsArray[treeTemp.currentLOD].materialsMeshCrossFade;
                                                    }
                                                    else
                                                        treeTemp.isCrossFadeMesh = true;
                                                    manager.getObjBillboard(treeTemp);

                                                    manager.needUpdateScene = true;

                                                    treeTemp.alphaPropBlockBillboard = 0f;
                                                    if (!manager.gpuInstancingSupport)
                                                        treeTemp.propBlockBillboards.SetFloat(AltTreesManager.Alpha_PropertyID, 0f);
                                                    treeTemp.isCrossFadeBillboard = true;

                                                    if (isPlaying)
                                                        treeTemp.crossFadeTime = Time.time + manager.altTreesMain.altTreesManagerData.getCrossFadeTimeBillboard();
                                                    else
                                                    {
                                                        #if UNITY_EDITOR
                                                        {
                                                            treeTemp.crossFadeTime = (float)EditorApplication.timeSinceStartup + manager.altTreesMain.altTreesManagerData.getCrossFadeTimeBillboard();
                                                        }
                                                        #endif
                                                    }
                                                }
                                                else
                                                {
                                                    if (treeTemp.currentCrossFadeId == 1)
                                                    {
                                                        treeTemp.currentCrossFadeId = 2;
                                                        treeTemp.currentCrossFadeLOD = treeTemp.currentLOD;
                                                        /*manager.delObjBillboard(treeTemp);
                                                        treeTemp.isCrossFadeBillboard = false;
                                                        if (!isPlaying)//
                                                        {
                                                            treeTemp.goMesh.SetActive(true);
                                                            treeTemp.goMesh.GetComponent<AltTreeInstance>().isCrossFade = true;
                                                            treeTemp.crossFadeTreeMeshRenderer = treeTemp.goMesh.GetComponent<MeshRenderer>();
                                                            treeTemp.crossFadeTreeMeshRenderer.sharedMaterials = manager.treesPoolArray[treeTemp.idPrototypeIndex].objsArray[treeTemp.currentLOD].materialsMeshCrossFade;
                                                        }
                                                        else
                                                            treeTemp.isCrossFadeMesh = true;*/
                                                        //manager.getObjBillboard(treeTemp, treeTemp.getPosWorld() + new Vector3(0f, manager.treesPoolArray[treeTemp.idPrototypeIndex].tree.size * treeTemp.heightScale / 2f /*- (manager.treesPoolArray[treeTemp.idPrototypeIndex].tree.size * treeTemp.heightScale - manager.treesPoolArray[treeTemp.idPrototypeIndex].tree.size * treeTemp.heightScale / manager.treesPoolArray[treeTemp.idPrototypeIndex].tree.space) / 2f*/ + manager.treesPoolArray[treeTemp.idPrototypeIndex].tree.up * treeTemp.heightScale, 0f));

                                                        treeTemp.alphaPropBlockBillboard = 1f;
                                                        if (!manager.gpuInstancingSupport)
                                                            treeTemp.propBlockBillboards.SetFloat(AltTreesManager.Alpha_PropertyID, 1f);

                                                        if (isPlaying)
                                                            treeTemp.crossFadeTime = Time.time + (manager.altTreesMain.altTreesManagerData.getCrossFadeTimeBillboard() - (treeTemp.crossFadeTime - Time.time));
                                                        else
                                                        {
                                                            #if UNITY_EDITOR
                                                            {
                                                                treeTemp.crossFadeTime = (float)EditorApplication.timeSinceStartup + (manager.altTreesMain.altTreesManagerData.getCrossFadeTimeBillboard() - (treeTemp.crossFadeTime - (float)EditorApplication.timeSinceStartup));
                                                            }
                                                            #endif
                                                        }
                                                    }
                                                    else if (treeTemp.currentCrossFadeId == 2)
                                                    {
                                                        manager.altTreesMain.Log("Error. treeTemp.currentCrossFadeId == 2");
                                                    }
                                                    else if (treeTemp.currentCrossFadeId == 3)
                                                    {
                                                        if (!isPlaying)//
                                                        {
                                                            propBlock.Clear();
                                                            propBlock.SetColor(AltTreesManager.HueVariationLeave_PropertyID, treeTemp.color);
                                                            propBlock.SetColor(AltTreesManager.HueVariationBark_PropertyID, treeTemp.colorBark);
                                                            propBlock.SetFloat(AltTreesManager.Alpha_PropertyID, 1.0f);
                                                            propBlock.SetFloat(AltTreesManager.Ind_PropertyID, 0.0f);
                                                            propBlock.SetFloat(AltTreesManager.smoothValue_PropertyID, 0.0f);
                                                            treeTemp.alphaPropBlockMesh = 1;
                                                            treeTemp.indPropBlockMesh = 0;
                                                            treeTemp.smoothPropBlock = 0;
                                                            treeTemp.crossFadeTreeMeshRenderer.SetPropertyBlock(propBlock);
                                                        }
                                                        else
                                                        {
                                                            if (!manager.gpuInstancingSupport || !treeTemp.gpuInstancing)
                                                            {
                                                                treeTemp.propBlockMesh.SetFloat(AltTreesManager.Alpha_PropertyID, 1f);
                                                                treeTemp.propBlockMesh.SetFloat(AltTreesManager.smoothValue_PropertyID, 0f);
                                                            }
                                                            treeTemp.alphaPropBlockMesh = 1;
                                                            treeTemp.smoothPropBlock = 0;
                                                        }

                                                        treeTemp.currentCrossFadeId = 2;
                                                        treeTemp.currentCrossFadeLOD = treeTemp.currentLOD;
                                                        if (!isPlaying)//
                                                        {
                                                            treeTemp.goMesh.GetComponent<AltTreeInstance>().isCrossFade = true;
                                                            treeTemp.crossFadeTreeMeshRenderer = treeTemp.goMesh.GetComponent<MeshRenderer>();
                                                            treeTemp.crossFadeTreeMeshRenderer.sharedMaterials = manager.treesPoolArray[treeTemp.idPrototypeIndex].objsArray[treeTemp.currentLOD].materialsMeshCrossFade;
                                                        }
                                                        else
                                                            treeTemp.isCrossFadeMesh = true;
                                                        manager.getObjBillboard(treeTemp);

                                                        manager.needUpdateScene = true;

                                                        treeTemp.alphaPropBlockBillboard = 0f;
                                                        if (!manager.gpuInstancingSupport)
                                                            treeTemp.propBlockBillboards.SetFloat(AltTreesManager.Alpha_PropertyID, 0f);
                                                        treeTemp.isCrossFadeBillboard = true;

                                                        if (isPlaying)
                                                            treeTemp.crossFadeTime = Time.time + manager.altTreesMain.altTreesManagerData.getCrossFadeTimeBillboard();
                                                        else
                                                        {
                                                            #if UNITY_EDITOR
                                                            {
                                                                treeTemp.crossFadeTime = (float)EditorApplication.timeSinceStartup + manager.altTreesMain.altTreesManagerData.getCrossFadeTimeBillboard();
                                                            }
                                                            #endif
                                                        }
                                                    }
                                                    else if (treeTemp.currentCrossFadeId == 4)
                                                    {
                                                        if (!isPlaying)//
                                                        {
                                                            propBlock.Clear();
                                                            propBlock.SetColor(AltTreesManager.HueVariationLeave_PropertyID, treeTemp.color);
                                                            propBlock.SetColor(AltTreesManager.HueVariationBark_PropertyID, treeTemp.colorBark);
                                                            propBlock.SetFloat(AltTreesManager.Alpha_PropertyID, 1.0f);
                                                            propBlock.SetFloat(AltTreesManager.Ind_PropertyID, 0.0f);
                                                            propBlock.SetFloat(AltTreesManager.smoothValue_PropertyID, 1.0f);
                                                            treeTemp.alphaPropBlockMesh = 1;
                                                            treeTemp.indPropBlockMesh = 0;
                                                            treeTemp.smoothPropBlock = 1;
                                                            treeTemp.crossFadeTreeMeshRenderer.SetPropertyBlock(propBlock);
                                                        }
                                                        else
                                                        {
                                                            if (!manager.gpuInstancingSupport || !treeTemp.gpuInstancing)
                                                            {
                                                                treeTemp.propBlockMesh.SetFloat(AltTreesManager.Alpha_PropertyID, 1f);
                                                                treeTemp.propBlockMesh.SetFloat(AltTreesManager.smoothValue_PropertyID, 1f);
                                                            }
                                                            treeTemp.alphaPropBlockMesh = 1;
                                                            treeTemp.smoothPropBlock = 1;
                                                        }

                                                        treeTemp.currentCrossFadeId = 2;
                                                        treeTemp.currentCrossFadeLOD = treeTemp.currentLOD;

                                                        if (!isPlaying)//
                                                        {
                                                            treeTemp.goMesh.GetComponent<AltTreeInstance>().isCrossFade = true;
                                                            treeTemp.crossFadeTreeMeshRenderer = treeTemp.goMesh.GetComponent<MeshRenderer>();
                                                            treeTemp.crossFadeTreeMeshRenderer.sharedMaterials = manager.treesPoolArray[treeTemp.idPrototypeIndex].objsArray[treeTemp.currentLOD].materialsMeshCrossFade;
                                                        }
                                                        else
                                                            treeTemp.isCrossFadeMesh = true;
                                                        manager.getObjBillboard(treeTemp);

                                                        manager.needUpdateScene = true;

                                                        treeTemp.alphaPropBlockBillboard = 0f;
                                                        if (!manager.gpuInstancingSupport)
                                                            treeTemp.propBlockBillboards.SetFloat(AltTreesManager.Alpha_PropertyID, 0f);
                                                        treeTemp.isCrossFadeBillboard = true;

                                                        if (isPlaying)
                                                            treeTemp.crossFadeTime = Time.time + manager.altTreesMain.altTreesManagerData.getCrossFadeTimeBillboard();
                                                        else
                                                        {
                                                            #if UNITY_EDITOR
                                                            {
                                                                treeTemp.crossFadeTime = (float)EditorApplication.timeSinceStartup + manager.altTreesMain.altTreesManagerData.getCrossFadeTimeBillboard();
                                                            }
                                                            #endif
                                                        }
                                                    }
                                                    else if (treeTemp.currentCrossFadeId == 6)
                                                    {
                                                        if (!isPlaying)//
                                                        {
                                                            propBlock.Clear();
                                                            propBlock.SetColor(AltTreesManager.HueVariationLeave_PropertyID, treeTemp.color);
                                                            propBlock.SetColor(AltTreesManager.HueVariationBark_PropertyID, treeTemp.colorBark);
                                                            propBlock.SetFloat(AltTreesManager.Alpha_PropertyID, 1.0f);
                                                            propBlock.SetFloat(AltTreesManager.Ind_PropertyID, 0.0f);
                                                            propBlock.SetFloat(AltTreesManager.smoothValue_PropertyID, 0.0f);
                                                            treeTemp.alphaPropBlockMesh = 1;
                                                            treeTemp.indPropBlockMesh = 0;
                                                            treeTemp.smoothPropBlock = 0;
                                                            treeTemp.crossFadeTreeMeshRenderer.SetPropertyBlock(propBlock);
                                                        }
                                                        else
                                                        {
                                                            if (!manager.gpuInstancingSupport || !treeTemp.gpuInstancing)
                                                            {
                                                                treeTemp.propBlockMesh.SetFloat(AltTreesManager.Alpha_PropertyID, 1f);
                                                                treeTemp.propBlockMesh.SetFloat(AltTreesManager.smoothValue_PropertyID, 0f);
                                                            }
                                                            treeTemp.alphaPropBlockMesh = 1;
                                                            treeTemp.smoothPropBlock = 0;
                                                        }

                                                        treeTemp.currentCrossFadeId = 2;
                                                        treeTemp.currentCrossFadeLOD = treeTemp.currentLOD;
                                                        if (!isPlaying)//
                                                        {
                                                            treeTemp.goMesh.GetComponent<AltTreeInstance>().isCrossFade = true;
                                                            treeTemp.crossFadeTreeMeshRenderer = treeTemp.goMesh.GetComponent<MeshRenderer>();
                                                            treeTemp.crossFadeTreeMeshRenderer.sharedMaterials = manager.treesPoolArray[treeTemp.idPrototypeIndex].objsArray[treeTemp.currentLOD].materialsMeshCrossFade;
                                                        }
                                                        else
                                                            treeTemp.isCrossFadeMesh = true;
                                                        manager.getObjBillboard(treeTemp);

                                                        manager.needUpdateScene = true;

                                                        treeTemp.alphaPropBlockBillboard = 0f;
                                                        if (!manager.gpuInstancingSupport)
                                                            treeTemp.propBlockBillboards.SetFloat(AltTreesManager.Alpha_PropertyID, 0f);
                                                        treeTemp.isCrossFadeBillboard = true;

                                                        if (isPlaying)
                                                            treeTemp.crossFadeTime = Time.time + manager.altTreesMain.altTreesManagerData.getCrossFadeTimeBillboard();
                                                        else
                                                        {
                                                            #if UNITY_EDITOR
                                                            {
                                                                treeTemp.crossFadeTime = (float)EditorApplication.timeSinceStartup + manager.altTreesMain.altTreesManagerData.getCrossFadeTimeBillboard();
                                                            }
                                                            #endif
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                if (!isPlaying)//
                                                    manager.delObjMeshEditor(treeTemp.goMesh);
                                                else
                                                    manager.delObjMesh(treeTemp);

                                                manager.getObjBillboard(treeTemp);

                                                manager.needUpdateScene = true;
                                            }
                                        }
                                        else if (newLOD == -3)               //  newLOD is culled
                                        {
                                            if (isPlaying && manager.altTreesMain.altTreesManagerData.enableColliders)
                                            {
                                                if (manager.treesPoolArray[treeTemp.idPrototypeIndex].tree.isColliders)
                                                {
                                                    manager.delColliderPool(treeTemp.idPrototype, treeTemp.collider, false, treeTemp);
                                                }
                                            }

                                            if (manager.treesPoolArray[treeTemp.idPrototypeIndex].tree.isBillboardCrossFade && !manager.isSelectionTree)
                                            {
                                                if (treeTemp.currentCrossFadeId == -1)
                                                {
                                                    manager.treesCrossFade.Add(treeTemp);
                                                    treeTemp.currentCrossFadeId = 5;
                                                    treeTemp.currentCrossFadeLOD = treeTemp.currentLOD;
                                                    if (!isPlaying)//
                                                    {
                                                        treeTemp.goMesh.GetComponent<AltTreeInstance>().isCrossFade = true;
                                                        treeTemp.crossFadeTreeMeshRenderer = treeTemp.goMesh.GetComponent<MeshRenderer>();
                                                        treeTemp.crossFadeTreeMeshRenderer.sharedMaterials = manager.treesPoolArray[treeTemp.idPrototypeIndex].objsArray[treeTemp.currentLOD].materialsMeshCrossFade;
                                                    }
                                                    else
                                                        treeTemp.isCrossFadeMesh = true;

                                                    if (isPlaying)
                                                        treeTemp.crossFadeTime = Time.time + manager.altTreesMain.altTreesManagerData.getCrossFadeTimeBillboard();
                                                    else
                                                    {
                                                        #if UNITY_EDITOR
                                                        {
                                                            treeTemp.crossFadeTime = (float)EditorApplication.timeSinceStartup + manager.altTreesMain.altTreesManagerData.getCrossFadeTimeBillboard();
                                                        }
                                                        #endif
                                                    }
                                                }
                                                else
                                                {
                                                    if (treeTemp.currentCrossFadeId == 6)
                                                    {
                                                        treeTemp.currentCrossFadeId = 5;
                                                        treeTemp.currentCrossFadeLOD = treeTemp.currentLOD;
                                                        if (!isPlaying)//
                                                        {
                                                            treeTemp.crossFadeTreeMeshRenderer = treeTemp.goMesh.GetComponent<MeshRenderer>();
                                                            treeTemp.crossFadeTreeMeshRenderer.sharedMaterials = manager.treesPoolArray[treeTemp.idPrototypeIndex].objsArray[treeTemp.currentLOD].materialsMeshCrossFade;
                                                        }
                                                        else
                                                            treeTemp.isCrossFadeMesh = true;

                                                        if (isPlaying)
                                                            treeTemp.crossFadeTime = Time.time + (manager.altTreesMain.altTreesManagerData.getCrossFadeTimeBillboard() - (treeTemp.crossFadeTime - Time.time));
                                                        else
                                                        {
                                                            #if UNITY_EDITOR
                                                            {
                                                                treeTemp.crossFadeTime = (float)EditorApplication.timeSinceStartup + (manager.altTreesMain.altTreesManagerData.getCrossFadeTimeBillboard() - (treeTemp.crossFadeTime - (float)EditorApplication.timeSinceStartup));
                                                            }
                                                            #endif
                                                        }
                                                    }
                                                    else if (treeTemp.currentCrossFadeId == 1)
                                                    {
                                                        //manager.delObjBillboard(treeTemp.idPrototype, treeTemp.goCrossFade);
                                                        manager.delObjBillboard(treeTemp);
                                                        treeTemp.isCrossFadeBillboard = false;
                                                        treeTemp.currentCrossFadeId = 5;
                                                        treeTemp.currentCrossFadeLOD = treeTemp.currentLOD;
                                                    }
                                                    else if (treeTemp.currentCrossFadeId == 5)
                                                    {
                                                        manager.altTreesMain.Log("Error. treeTemp.currentCrossFadeId == 5");
                                                    }
                                                    else if (treeTemp.currentCrossFadeId == 3)
                                                    {
                                                        if (!isPlaying)//
                                                        {
                                                            manager.delObjMeshEditor(treeTemp.goMesh);
                                                        }
                                                        else
                                                        {
                                                            if (!manager.gpuInstancingSupport || !treeTemp.gpuInstancing)
                                                            {
                                                                treeTemp.propBlockMesh.SetFloat(AltTreesManager.Alpha_PropertyID, 1f);
                                                                treeTemp.propBlockMesh.SetFloat(AltTreesManager.smoothValue_PropertyID, 0f);
                                                            }
                                                            treeTemp.alphaPropBlockMesh = 1;
                                                            treeTemp.smoothPropBlock = 0;
                                                        }

                                                        treeTemp.currentCrossFadeId = 5;
                                                        treeTemp.currentCrossFadeLOD = treeTemp.currentLOD;

                                                        if (!isPlaying)//
                                                        {
                                                            treeTemp.goMesh = manager.getObjMeshEditor(treeTemp, treeTemp.currentLOD);   //1

                                                            treeTemp.goMesh.GetComponent<AltTreeInstance>().isCrossFade = true;
                                                            treeTemp.crossFadeTreeMeshRenderer = treeTemp.goMesh.GetComponent<MeshRenderer>();
                                                            treeTemp.crossFadeTreeMeshRenderer.sharedMaterials = manager.treesPoolArray[treeTemp.idPrototypeIndex].objsArray[treeTemp.currentLOD].materialsMeshCrossFade;
                                                        }
                                                        else
                                                            treeTemp.isCrossFadeMesh = true;

                                                        if (isPlaying)
                                                            treeTemp.crossFadeTime = Time.time + manager.altTreesMain.altTreesManagerData.getCrossFadeTimeBillboard();
                                                        else
                                                        {
                                                            #if UNITY_EDITOR
                                                            {
                                                                treeTemp.crossFadeTime = (float)EditorApplication.timeSinceStartup + manager.altTreesMain.altTreesManagerData.getCrossFadeTimeBillboard();
                                                            }
                                                            #endif
                                                        }
                                                    }
                                                    else if (treeTemp.currentCrossFadeId == 4)
                                                    {
                                                        if (!isPlaying)//
                                                        {
                                                            propBlock.Clear();
                                                            propBlock.SetColor(AltTreesManager.HueVariationLeave_PropertyID, treeTemp.color);
                                                            propBlock.SetColor(AltTreesManager.HueVariationBark_PropertyID, treeTemp.colorBark);
                                                            propBlock.SetFloat(AltTreesManager.Alpha_PropertyID, 1.0f);
                                                            propBlock.SetFloat(AltTreesManager.Ind_PropertyID, 0.0f);
                                                            propBlock.SetFloat(AltTreesManager.smoothValue_PropertyID, 0.0f);
                                                            treeTemp.alphaPropBlockMesh = 1;
                                                            treeTemp.indPropBlockMesh = 0;
                                                            treeTemp.smoothPropBlock = 0;
                                                            treeTemp.crossFadeTreeMeshRenderer.SetPropertyBlock(propBlock);
                                                        }
                                                        else
                                                        {
                                                            if (!manager.gpuInstancingSupport || !treeTemp.gpuInstancing)
                                                            {
                                                                treeTemp.propBlockMesh.SetFloat(AltTreesManager.Alpha_PropertyID, 1f);
                                                                treeTemp.propBlockMesh.SetFloat(AltTreesManager.smoothValue_PropertyID, 0f);
                                                            }
                                                            treeTemp.alphaPropBlockMesh = 1;
                                                            treeTemp.smoothPropBlock = 0;
                                                        }

                                                        treeTemp.currentCrossFadeId = 5;
                                                        treeTemp.currentCrossFadeLOD = treeTemp.currentLOD;

                                                        if (isPlaying)
                                                            treeTemp.crossFadeTime = Time.time + manager.altTreesMain.altTreesManagerData.getCrossFadeTimeBillboard();
                                                        else
                                                        {
                                                            #if UNITY_EDITOR
                                                            {
                                                                treeTemp.crossFadeTime = (float)EditorApplication.timeSinceStartup + manager.altTreesMain.altTreesManagerData.getCrossFadeTimeBillboard();
                                                            }
                                                            #endif
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                if (!isPlaying)//
                                                {
                                                    manager.needUpdateScene = true;
                                                    manager.delObjMeshEditor(treeTemp.goMesh);
                                                }
                                                else
                                                    manager.delObjMesh(treeTemp);
                                            }
                                        }
                                        else                            //  newLOD is mesh
                                        {
                                            if (manager.treesPoolArray[treeTemp.idPrototypeIndex].tree.isMeshCrossFade && !manager.isSelectionTree)
                                            {
                                                if (treeTemp.currentCrossFadeId == -1)
                                                {
                                                    manager.treesCrossFade.Add(treeTemp);

                                                    if (newLOD > treeTemp.currentLOD)
                                                    {
                                                        treeTemp.currentCrossFadeId = 3;
                                                        if (!isPlaying)//
                                                        {
                                                            treeTemp.goMesh.GetComponent<AltTreeInstance>().isCrossFade = true;
                                                            treeTemp.crossFadeTreeMeshRenderer = treeTemp.goMesh.GetComponent<MeshRenderer>();
                                                            propBlock.Clear();
                                                            propBlock.SetColor(AltTreesManager.HueVariationLeave_PropertyID, treeTemp.color);
                                                            propBlock.SetColor(AltTreesManager.HueVariationBark_PropertyID, treeTemp.colorBark);
                                                            propBlock.SetFloat(AltTreesManager.Alpha_PropertyID, 1.0f);
                                                            propBlock.SetFloat(AltTreesManager.Ind_PropertyID, 0.0f);
                                                            propBlock.SetFloat(AltTreesManager.smoothValue_PropertyID, 0.0f);
                                                            treeTemp.alphaPropBlockMesh = 1;
                                                            treeTemp.indPropBlockMesh = 0;
                                                            treeTemp.smoothPropBlock = 0;
                                                            treeTemp.crossFadeTreeMeshRenderer.SetPropertyBlock(propBlock);
                                                        }
                                                        else
                                                            treeTemp.isCrossFadeMesh = true;

                                                        //treeTemp.go = manager.getObjMesh(treeTemp.idPrototype, newLOD, altTreesID, treeTemp.idTree, treeTemp.color, treeTemp.colorBark);
                                                        if (!isPlaying)//
                                                        {
                                                            /*treeTemp.go = manager.getObjMeshEditor(treeTemp, newLOD, altTreesID);

                                                            treeTemp.go.SetActive(false);

                                                            scaleTemp.x = treeTemp.widthScale;
                                                            scaleTemp.y = treeTemp.heightScale;
                                                            scaleTemp.z = treeTemp.widthScale;
                                                            treeTemp.go.transform.localScale = scaleTemp;
                                                            treeTemp.go.transform.position = treeTemp.getPosWorld();
                                                            treeTemp.go.transform.rotation = Quaternion.AngleAxis(treeTemp.rotation, Vector3.up);
                                                            manager.treesList.Add(treeTemp.go, treeTemp);*/
                                                        }
                                                        else
                                                            manager.getObjMesh(treeTemp);
                                                    }
                                                    else
                                                    {
                                                        treeTemp.currentCrossFadeId = 4;
                                                        if (!isPlaying)//
                                                            manager.delObjMeshEditor(treeTemp.goMesh);
                                                        else
                                                            manager.delObjMesh(treeTemp);
                                                    
                                                        if (!isPlaying)//
                                                        {
                                                            treeTemp.goMesh = manager.getObjMeshEditor(treeTemp, newLOD);

                                                            treeTemp.goMesh.GetComponent<AltTreeInstance>().isCrossFade = true;
                                                            treeTemp.crossFadeTreeMeshRenderer = treeTemp.goMesh.GetComponent<MeshRenderer>();
                                                            propBlock.Clear();
                                                            propBlock.SetColor(AltTreesManager.HueVariationLeave_PropertyID, treeTemp.color);
                                                            propBlock.SetColor(AltTreesManager.HueVariationBark_PropertyID, treeTemp.colorBark);
                                                            propBlock.SetFloat(AltTreesManager.Alpha_PropertyID, 1.0f);
                                                            propBlock.SetFloat(AltTreesManager.Ind_PropertyID, 0.0f);
                                                            propBlock.SetFloat(AltTreesManager.smoothValue_PropertyID, 1.0f);
                                                            treeTemp.alphaPropBlockMesh = 1;
                                                            treeTemp.indPropBlockMesh = 0;
                                                            treeTemp.smoothPropBlock = 1;
                                                            treeTemp.crossFadeTreeMeshRenderer.SetPropertyBlock(propBlock);
                                                        }
                                                        else
                                                        {
                                                            treeTemp.isCrossFadeMesh = true;
                                                            manager.getObjMesh(treeTemp);
                                                            if (!manager.gpuInstancingSupport || !treeTemp.gpuInstancing)
                                                            {
                                                                treeTemp.propBlockMesh.SetFloat(AltTreesManager.Alpha_PropertyID, 1f);
                                                                treeTemp.propBlockMesh.SetFloat(AltTreesManager.smoothValue_PropertyID, 1f);
                                                            }
                                                            treeTemp.alphaPropBlockMesh = 1;
                                                            treeTemp.smoothPropBlock = 1;
                                                        }
                                                    }


                                                    treeTemp.currentCrossFadeLOD = treeTemp.currentLOD;
                                                    if (isPlaying)
                                                        treeTemp.crossFadeTime = Time.time + manager.altTreesMain.altTreesManagerData.getCrossFadeTimeMesh();
                                                    else
                                                    {
                                                        #if UNITY_EDITOR
                                                        {
                                                            treeTemp.crossFadeTime = (float)EditorApplication.timeSinceStartup + manager.altTreesMain.altTreesManagerData.getCrossFadeTimeMesh();
                                                        }
                                                        #endif
                                                    }
                                                }
                                                else
                                                {
                                                    if (treeTemp.currentCrossFadeId == 1 || treeTemp.currentCrossFadeId == 6)
                                                    {
                                                        if (!isPlaying)//
                                                        {
                                                            manager.delObjMeshEditor(treeTemp.goMesh);
                                                        }
                                                        else
                                                        {
                                                            manager.delObjMesh(treeTemp);
                                                        }
                                                        //treeTemp.go = manager.getObjMesh(treeTemp.idPrototype, newLOD, altTreesID, treeTemp.idTree, treeTemp.color, treeTemp.colorBark);
                                                        if (!isPlaying)//
                                                        {
                                                            treeTemp.goMesh = manager.getObjMeshEditor(treeTemp, newLOD);

                                                            treeTemp.goMesh.GetComponent<AltTreeInstance>().isCrossFade = true;
                                                            treeTemp.crossFadeTreeMeshRenderer = treeTemp.goMesh.GetComponent<MeshRenderer>();
                                                            treeTemp.crossFadeTreeMeshRenderer.sharedMaterials = manager.treesPoolArray[treeTemp.idPrototypeIndex].objsArray[newLOD].materialsMeshCrossFade;

                                                        }
                                                        else
                                                        {
                                                            treeTemp.isCrossFadeMesh = true;
                                                            manager.getObjMesh(treeTemp);
                                                        }
                                                        float tempFloatNext = 0f;

                                                        if (isPlaying)
                                                            tempFloatNext = treeTemp.crossFadeTime - Time.time;
                                                        else
                                                        {
                                                            #if UNITY_EDITOR
                                                            {
                                                                tempFloatNext = treeTemp.crossFadeTime - (float)EditorApplication.timeSinceStartup;
                                                            }
                                                            #endif
                                                        }
                                                        if (!isPlaying)//
                                                        {
                                                            propBlock.Clear();
                                                            propBlock.SetColor(AltTreesManager.HueVariationLeave_PropertyID, treeTemp.color);
                                                            propBlock.SetColor(AltTreesManager.HueVariationBark_PropertyID, treeTemp.colorBark);
                                                            treeTemp.alphaPropBlockMesh = Mathf.Clamp(1f - tempFloatNext / (float)manager.altTreesMain.altTreesManagerData.getCrossFadeTimeBillboard(), 0f, 1.0f);
                                                            treeTemp.indPropBlockMesh = 0;
                                                            treeTemp.smoothPropBlock = 0;
                                                            propBlock.SetFloat(AltTreesManager.Alpha_PropertyID, treeTemp.alphaPropBlockMesh);
                                                            propBlock.SetFloat(AltTreesManager.Ind_PropertyID, 0.0f);
                                                            propBlock.SetFloat(AltTreesManager.smoothValue_PropertyID, 0.0f);
                                                            treeTemp.crossFadeTreeMeshRenderer.SetPropertyBlock(propBlock);
                                                        }
                                                        else
                                                        {
                                                            treeTemp.alphaPropBlockMesh = Mathf.Clamp(1f - tempFloatNext / (float)manager.altTreesMain.altTreesManagerData.getCrossFadeTimeBillboard(), 0f, 1.0f);
                                                            treeTemp.smoothPropBlock = 0;
                                                            if (!manager.gpuInstancingSupport || !treeTemp.gpuInstancing)
                                                            {
                                                                treeTemp.propBlockMesh.SetFloat(AltTreesManager.Alpha_PropertyID, treeTemp.alphaPropBlockMesh);
                                                                treeTemp.propBlockMesh.SetFloat(AltTreesManager.smoothValue_PropertyID, 0f);
                                                            }
                                                        }
                                                    }
                                                    else if (treeTemp.currentCrossFadeId == 2)
                                                    {
                                                        manager.altTreesMain.Log("Error. treeTemp.currentCrossFadeId == 2");
                                                    }
                                                    else if (is_3_OR_4_CrossFade)
                                                    {
                                                        manager.treesCrossFade.Remove(treeTemp);

                                                        if (treeTemp.currentCrossFadeId == 3)
                                                        {
                                                            if (!isPlaying)//
                                                            {
                                                                manager.delObjMeshEditor(treeTemp.goMesh);

                                                                treeTemp.crossFadeTreeMeshRenderer = null;
                                                            }
                                                            else
                                                            {
                                                                treeTemp.isCrossFadeMesh = false;

                                                                manager.delObjMesh(treeTemp);
                                                            }
                                                        }
                                                        else if (treeTemp.currentCrossFadeId == 4)
                                                        {
                                                            if (!isPlaying)//
                                                            {
                                                                manager.delObjMeshEditor(treeTemp.goMesh);

                                                                treeTemp.crossFadeTreeMeshRenderer = null;
                                                            }
                                                            else
                                                            {
                                                                treeTemp.isCrossFadeMesh = false;

                                                                manager.delObjMesh(treeTemp);
                                                            }
                                                        }

                                                        if (!isPlaying)//
                                                        {
                                                            treeTemp.goMesh = manager.getObjMeshEditor(treeTemp, newLOD);
                                                        }
                                                        else
                                                            manager.getObjMesh(treeTemp);

                                                        treeTemp.currentCrossFadeId = -1;
                                                        treeTemp.currentCrossFadeLOD = -1;
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                if (!isPlaying)//
                                                {
                                                    manager.delObjMeshEditor(treeTemp.goMesh);
                                                }
                                                else
                                                    manager.delObjMesh(treeTemp);

                                                if (!isPlaying)//
                                                {
                                                    manager.needUpdateScene = true;
                                                    treeTemp.goMesh = manager.getObjMeshEditor(treeTemp, newLOD);
                                                }
                                                else
                                                    manager.getObjMesh(treeTemp);
                                            }
                                        }
                                    }
                                    else if (treeTemp.currentLOD == -2)               //  currentLOD is billboard
                                    {
                                        if (newLOD != -3)               //  newLOD is mesh
                                        {
                                            if (isPlaying && manager.altTreesMain.altTreesManagerData.enableColliders)
                                            {
                                                if (!manager.treesPoolArray[treeTemp.idPrototypeIndex].tree.isCollidersEqual)
                                                {
                                                    if (manager.treesPoolArray[treeTemp.idPrototypeIndex].tree.isBillboardColliders)
                                                    {
                                                        manager.delColliderPool(treeTemp.idPrototype, treeTemp.collider, true, treeTemp);
                                                    }
                                                    if (manager.treesPoolArray[treeTemp.idPrototypeIndex].tree.isColliders)
                                                    {
                                                        treeTemp.collider = manager.getColliderPool(treeTemp.idPrototype, false, treeTemp);
                                                        scaleTemp.x = treeTemp.widthScale;
                                                        scaleTemp.y = treeTemp.heightScale;
                                                        scaleTemp.z = treeTemp.widthScale;
                                                        treeTemp.collider.go.transform.localScale = scaleTemp;
                                                        treeTemp.collider.go.transform.position = treeTemp.getPosWorld();
                                                        treeTemp.collider.go.transform.rotation = Quaternion.AngleAxis(treeTemp.rotation, Vector3.up);
                                                    }
                                                }
                                                else
                                                {
                                                    manager.drawCollidersCount++;
                                                    manager.drawColliderBillboardsCount--;

                                                    manager.treesPoolArray[treeTemp.idPrototypeIndex].collidersMaxCount = Mathf.Max(manager.treesPoolArray[treeTemp.idPrototypeIndex].collidersMaxCount, manager.drawCollidersCount);
                                                }
                                            }

                                            if (manager.treesPoolArray[treeTemp.idPrototypeIndex].tree.isBillboardCrossFade && !manager.isSelectionTree)
                                            {
                                                if (treeTemp.currentCrossFadeId == -1)
                                                {
                                                    manager.treesCrossFade.Add(treeTemp);
                                                    treeTemp.currentCrossFadeId = 1;
                                                    treeTemp.currentCrossFadeLOD = -1;

                                                    treeTemp.isCrossFadeBillboard = true;
                                                    treeTemp.alphaPropBlockBillboard = 1f;
                                                    if (!manager.gpuInstancingSupport)
                                                        treeTemp.propBlockBillboards.SetFloat(AltTreesManager.Alpha_PropertyID, 1f);

                                                    if (!isPlaying)//
                                                    {
                                                        treeTemp.goMesh = manager.getObjMeshEditor(treeTemp, newLOD);

                                                        treeTemp.goMesh.GetComponent<AltTreeInstance>().isCrossFade = true;
                                                        treeTemp.crossFadeTreeMeshRenderer = treeTemp.goMesh.GetComponent<MeshRenderer>();
                                                        treeTemp.crossFadeTreeMeshRenderer.sharedMaterials = manager.treesPoolArray[treeTemp.idPrototypeIndex].objsArray[newLOD].materialsMeshCrossFade;

                                                        propBlock.Clear();
                                                        propBlock.SetColor(AltTreesManager.HueVariationLeave_PropertyID, treeTemp.color);
                                                        propBlock.SetColor(AltTreesManager.HueVariationBark_PropertyID, treeTemp.colorBark);
                                                        propBlock.SetFloat(AltTreesManager.Alpha_PropertyID, 0.0f);
                                                        propBlock.SetFloat(AltTreesManager.Ind_PropertyID, 0.0f);
                                                        propBlock.SetFloat(AltTreesManager.smoothValue_PropertyID, 0.0f);
                                                        treeTemp.alphaPropBlockMesh = 0;
                                                        treeTemp.indPropBlockMesh = 0;
                                                        treeTemp.smoothPropBlock = 0;
                                                        treeTemp.crossFadeTreeMeshRenderer.SetPropertyBlock(propBlock);
                                                    }
                                                    else
                                                    {
                                                        treeTemp.isCrossFadeMesh = true;
                                                        manager.getObjMesh(treeTemp);
                                                        if (!manager.gpuInstancingSupport || !treeTemp.gpuInstancing)
                                                        {
                                                            treeTemp.propBlockMesh.SetFloat(AltTreesManager.Alpha_PropertyID, 0f);
                                                            treeTemp.propBlockMesh.SetFloat(AltTreesManager.smoothValue_PropertyID, 0f);
                                                        }
                                                        treeTemp.alphaPropBlockMesh = 0;
                                                        treeTemp.smoothPropBlock = 0;
                                                    }
                                                

                                                    if (isPlaying)
                                                        treeTemp.crossFadeTime = Time.time + manager.altTreesMain.altTreesManagerData.getCrossFadeTimeBillboard();
                                                    else
                                                    {
                                                        #if UNITY_EDITOR
                                                        {
                                                            treeTemp.crossFadeTime = (float)EditorApplication.timeSinceStartup + manager.altTreesMain.altTreesManagerData.getCrossFadeTimeBillboard();
                                                        }
                                                        #endif
                                                    }
                                                }
                                                else
                                                {
                                                    if (treeTemp.currentCrossFadeId == 1)
                                                    {
                                                        manager.altTreesMain.Log("Error. treeTemp.currentCrossFadeId == 1");
                                                    }
                                                    else if (treeTemp.currentCrossFadeId == 2)
                                                    {
                                                        if (!isPlaying)//
                                                        {
                                                            manager.delObjMeshEditor(treeTemp.goMesh);
                                                        }
                                                        else
                                                            manager.delObjMesh(treeTemp);

                                                        if (!isPlaying)//
                                                        {
                                                            treeTemp.goMesh = manager.getObjMeshEditor(treeTemp, newLOD);

                                                            treeTemp.goMesh.GetComponent<AltTreeInstance>().isCrossFade = true;
                                                            treeTemp.crossFadeTreeMeshRenderer = treeTemp.goMesh.GetComponent<MeshRenderer>();
                                                            treeTemp.crossFadeTreeMeshRenderer.sharedMaterials = manager.treesPoolArray[treeTemp.idPrototypeIndex].objsArray[newLOD].materialsMeshCrossFade;
                                                            propBlock.Clear();
                                                            propBlock.SetColor(AltTreesManager.HueVariationLeave_PropertyID, treeTemp.color);
                                                            propBlock.SetColor(AltTreesManager.HueVariationBark_PropertyID, treeTemp.colorBark);
                                                            propBlock.SetFloat(AltTreesManager.Alpha_PropertyID, 0.0f);
                                                            propBlock.SetFloat(AltTreesManager.Ind_PropertyID, 0.0f);
                                                            propBlock.SetFloat(AltTreesManager.smoothValue_PropertyID, 0.0f);
                                                            treeTemp.alphaPropBlockMesh = 0;
                                                            treeTemp.indPropBlockMesh = 0;
                                                            treeTemp.smoothPropBlock = 0;
                                                            treeTemp.crossFadeTreeMeshRenderer.SetPropertyBlock(propBlock);
                                                        }
                                                        else
                                                        {
                                                            treeTemp.isCrossFadeMesh = true;
                                                            manager.getObjMesh(treeTemp);
                                                            if (!manager.gpuInstancingSupport || !treeTemp.gpuInstancing)
                                                            {
                                                                treeTemp.propBlockMesh.SetFloat(AltTreesManager.Alpha_PropertyID, 0f);
                                                                treeTemp.propBlockMesh.SetFloat(AltTreesManager.smoothValue_PropertyID, 0f);
                                                            }
                                                            treeTemp.alphaPropBlockMesh = 0;
                                                            treeTemp.smoothPropBlock = 0;
                                                        }

                                                        if (isPlaying)
                                                            treeTemp.crossFadeTime = Time.time + (manager.altTreesMain.altTreesManagerData.getCrossFadeTimeBillboard() - (treeTemp.crossFadeTime - Time.time));
                                                        else
                                                        {
                                                            #if UNITY_EDITOR
                                                            {
                                                                treeTemp.crossFadeTime = (float)EditorApplication.timeSinceStartup + (manager.altTreesMain.altTreesManagerData.getCrossFadeTimeBillboard() - (treeTemp.crossFadeTime - (float)EditorApplication.timeSinceStartup));
                                                            }
                                                            #endif
                                                        }
                                                        treeTemp.currentCrossFadeId = 1;
                                                        treeTemp.currentCrossFadeLOD = -1;
                                                    }
                                                    else if (treeTemp.currentCrossFadeId == 8)
                                                    {
                                                        if (!isPlaying)//
                                                        {
                                                            treeTemp.goMesh = manager.getObjMeshEditor(treeTemp, newLOD);

                                                            treeTemp.goMesh.GetComponent<AltTreeInstance>().isCrossFade = true;
                                                            treeTemp.crossFadeTreeMeshRenderer = treeTemp.goMesh.GetComponent<MeshRenderer>();
                                                            treeTemp.crossFadeTreeMeshRenderer.sharedMaterials = manager.treesPoolArray[treeTemp.idPrototypeIndex].objsArray[newLOD].materialsMeshCrossFade;
                                                            propBlock.Clear();
                                                            propBlock.SetColor(AltTreesManager.HueVariationLeave_PropertyID, treeTemp.color);
                                                            propBlock.SetColor(AltTreesManager.HueVariationBark_PropertyID, treeTemp.colorBark);
                                                            propBlock.SetFloat(AltTreesManager.Alpha_PropertyID, 0.0f);
                                                            propBlock.SetFloat(AltTreesManager.Ind_PropertyID, 0.0f);
                                                            propBlock.SetFloat(AltTreesManager.smoothValue_PropertyID, 0.0f);
                                                            treeTemp.alphaPropBlockMesh = 0;
                                                            treeTemp.indPropBlockMesh = 0;
                                                            treeTemp.smoothPropBlock = 0;
                                                            treeTemp.crossFadeTreeMeshRenderer.SetPropertyBlock(propBlock);
                                                        }
                                                        else
                                                        {
                                                            treeTemp.isCrossFadeMesh = true;
                                                            manager.getObjMesh(treeTemp);
                                                            if (!manager.gpuInstancingSupport || !treeTemp.gpuInstancing)
                                                            {
                                                                treeTemp.propBlockMesh.SetFloat(AltTreesManager.Alpha_PropertyID, 0f);
                                                                treeTemp.propBlockMesh.SetFloat(AltTreesManager.smoothValue_PropertyID, 0f);
                                                            }
                                                            treeTemp.alphaPropBlockMesh = 0;
                                                            treeTemp.smoothPropBlock = 0;
                                                        }

                                                        if (isPlaying)
                                                            treeTemp.crossFadeTime = Time.time + (manager.altTreesMain.altTreesManagerData.getCrossFadeTimeBillboard() - (treeTemp.crossFadeTime - Time.time));
                                                        else
                                                        {
                                                            #if UNITY_EDITOR
                                                            {
                                                                treeTemp.crossFadeTime = (float)EditorApplication.timeSinceStartup + (manager.altTreesMain.altTreesManagerData.getCrossFadeTimeBillboard() - (treeTemp.crossFadeTime - (float)EditorApplication.timeSinceStartup));
                                                            }
                                                            #endif
                                                        }
                                                        treeTemp.currentCrossFadeId = 1;
                                                        treeTemp.currentCrossFadeLOD = -1;
                                                    }
                                                    else if (is_3_OR_4_CrossFade)
                                                    {
                                                        manager.altTreesMain.Log("Error. treeTemp.currentCrossFadeId == " + treeTemp.currentCrossFadeId);
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                manager.delObjBillboard(treeTemp);

                                                if (!isPlaying)//
                                                {
                                                    manager.needUpdateScene = true;
                                                    treeTemp.goMesh = manager.getObjMeshEditor(treeTemp, newLOD);
                                                }
                                                else
                                                    manager.getObjMesh(treeTemp);
                                            }
                                        }
                                        else if (newLOD == -3)               //  newLOD is culled
                                        {
                                            if (isPlaying && manager.altTreesMain.altTreesManagerData.enableColliders)
                                            {
                                                if (manager.treesPoolArray[treeTemp.idPrototypeIndex].tree.isBillboardColliders)
                                                {
                                                    manager.delColliderPool(treeTemp.idPrototype, treeTemp.collider, true, treeTemp);
                                                }
                                            }

                                            if (manager.treesPoolArray[treeTemp.idPrototypeIndex].tree.isBillboardCrossFade && !manager.isSelectionTree)
                                            {
                                                if (treeTemp.currentCrossFadeId == -1)
                                                {
                                                    manager.treesCrossFade.Add(treeTemp);
                                                    treeTemp.currentCrossFadeId = 7;
                                                    treeTemp.currentCrossFadeLOD = -1;

                                                    treeTemp.isCrossFadeBillboard = true;
                                                    treeTemp.alphaPropBlockBillboard = 1f;
                                                    if (!manager.gpuInstancingSupport)
                                                        treeTemp.propBlockBillboards.SetFloat(AltTreesManager.Alpha_PropertyID, 1f);

                                                    if (isPlaying)
                                                        treeTemp.crossFadeTime = Time.time + manager.altTreesMain.altTreesManagerData.getCrossFadeTimeBillboard();
                                                    else
                                                    {
                                                        #if UNITY_EDITOR
                                                        {
                                                            treeTemp.crossFadeTime = (float)EditorApplication.timeSinceStartup + manager.altTreesMain.altTreesManagerData.getCrossFadeTimeBillboard();
                                                        }
                                                        #endif
                                                    }
                                                }
                                                else
                                                {
                                                    if (treeTemp.currentCrossFadeId == 7)
                                                    {
                                                        manager.altTreesMain.Log("Error. treeTemp.currentCrossFadeId == 7");
                                                    }
                                                    else if (treeTemp.currentCrossFadeId == 2)
                                                    {
                                                        if (!isPlaying)//
                                                        {
                                                            manager.delObjMeshEditor(treeTemp.goMesh);
                                                        }
                                                        else
                                                        {
                                                            treeTemp.isCrossFadeMesh = false;
                                                            manager.delObjMesh(treeTemp);
                                                        }

                                                        if (isPlaying)
                                                            treeTemp.crossFadeTime = Time.time + (manager.altTreesMain.altTreesManagerData.getCrossFadeTimeBillboard() - (treeTemp.crossFadeTime - Time.time));
                                                        else
                                                        {
                                                            #if UNITY_EDITOR
                                                            {
                                                                treeTemp.crossFadeTime = (float)EditorApplication.timeSinceStartup + (manager.altTreesMain.altTreesManagerData.getCrossFadeTimeBillboard() - (treeTemp.crossFadeTime - (float)EditorApplication.timeSinceStartup));
                                                            }
                                                            #endif
                                                        }
                                                        treeTemp.currentCrossFadeId = 7;
                                                        treeTemp.currentCrossFadeLOD = -1;
                                                    }
                                                    else if (treeTemp.currentCrossFadeId == 8)
                                                    {
                                                        if (isPlaying)
                                                            treeTemp.crossFadeTime = Time.time + (manager.altTreesMain.altTreesManagerData.getCrossFadeTimeBillboard() - (treeTemp.crossFadeTime - Time.time));
                                                        else
                                                        {
                                                            #if UNITY_EDITOR
                                                            {
                                                                treeTemp.crossFadeTime = (float)EditorApplication.timeSinceStartup + (manager.altTreesMain.altTreesManagerData.getCrossFadeTimeBillboard() - (treeTemp.crossFadeTime - (float)EditorApplication.timeSinceStartup));
                                                            }
                                                            #endif
                                                        }
                                                        treeTemp.currentCrossFadeId = 7;
                                                        treeTemp.currentCrossFadeLOD = -1;
                                                    }
                                                    else if (is_3_OR_4_CrossFade)
                                                    {
                                                        manager.altTreesMain.Log("Error. treeTemp.currentCrossFadeId == " + treeTemp.currentCrossFadeId);
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                manager.needUpdateScene = true;
                                                manager.delObjBillboard(treeTemp);
                                            }
                                        }
                                    }
                                    else if (treeTemp.currentLOD == -3)               //  currentLOD is culled
                                    {
                                        if (newLOD != -2)               //  newLOD is mesh
                                        {
                                            if (isPlaying && manager.altTreesMain.altTreesManagerData.enableColliders)
                                            {
                                                if (manager.treesPoolArray[treeTemp.idPrototypeIndex].tree.isColliders)
                                                {
                                                    treeTemp.collider = manager.getColliderPool(treeTemp.idPrototype, false, treeTemp);
                                                    scaleTemp.x = treeTemp.widthScale;
                                                    scaleTemp.y = treeTemp.heightScale;
                                                    scaleTemp.z = treeTemp.widthScale;
                                                    treeTemp.collider.go.transform.localScale = scaleTemp;
                                                    treeTemp.collider.go.transform.position = treeTemp.getPosWorld();
                                                    treeTemp.collider.go.transform.rotation = Quaternion.AngleAxis(treeTemp.rotation, Vector3.up);
                                                }
                                            }

                                            if (manager.treesPoolArray[treeTemp.idPrototypeIndex].tree.isBillboardCrossFade && !manager.isSelectionTree)
                                            {
                                                if (treeTemp.currentCrossFadeId == -1)
                                                {
                                                    manager.treesCrossFade.Add(treeTemp);
                                                    treeTemp.currentCrossFadeId = 6;
                                                    treeTemp.currentCrossFadeLOD = -1;

                                                    if (!isPlaying)//
                                                    {
                                                        treeTemp.goMesh = manager.getObjMeshEditor(treeTemp, newLOD);

                                                        treeTemp.goMesh.GetComponent<AltTreeInstance>().isCrossFade = true;
                                                        treeTemp.crossFadeTreeMeshRenderer = treeTemp.goMesh.GetComponent<MeshRenderer>();
                                                        treeTemp.crossFadeTreeMeshRenderer.sharedMaterials = manager.treesPoolArray[treeTemp.idPrototypeIndex].objsArray[newLOD].materialsMeshCrossFade;

                                                        propBlock.Clear();
                                                        propBlock.SetColor(AltTreesManager.HueVariationLeave_PropertyID, treeTemp.color);
                                                        propBlock.SetColor(AltTreesManager.HueVariationBark_PropertyID, treeTemp.colorBark);
                                                        propBlock.SetFloat(AltTreesManager.Alpha_PropertyID, 0.0f);
                                                        propBlock.SetFloat(AltTreesManager.Ind_PropertyID, 0.0f);
                                                        propBlock.SetFloat(AltTreesManager.smoothValue_PropertyID, 0.0f);
                                                        treeTemp.alphaPropBlockMesh = 0;
                                                        treeTemp.indPropBlockMesh = 0;
                                                        treeTemp.smoothPropBlock = 0;
                                                        treeTemp.crossFadeTreeMeshRenderer.SetPropertyBlock(propBlock);
                                                    }
                                                    else
                                                    {
                                                        treeTemp.isCrossFadeMesh = true;
                                                        manager.getObjMesh(treeTemp);
                                                        if (!manager.gpuInstancingSupport || !treeTemp.gpuInstancing)
                                                        {
                                                            treeTemp.propBlockMesh.SetFloat(AltTreesManager.Alpha_PropertyID, 0f);
                                                            treeTemp.propBlockMesh.SetFloat(AltTreesManager.smoothValue_PropertyID, 0f);
                                                        }
                                                        treeTemp.alphaPropBlockMesh = 0;
                                                        treeTemp.smoothPropBlock = 0;
                                                    }


                                                    if (isPlaying)
                                                        treeTemp.crossFadeTime = Time.time + manager.altTreesMain.altTreesManagerData.getCrossFadeTimeBillboard();
                                                    else
                                                    {
                                                        #if UNITY_EDITOR
                                                        {
                                                            treeTemp.crossFadeTime = (float)EditorApplication.timeSinceStartup + manager.altTreesMain.altTreesManagerData.getCrossFadeTimeBillboard();
                                                        }
                                                        #endif
                                                    }
                                                }
                                                else
                                                {
                                                    if (treeTemp.currentCrossFadeId == 6)
                                                    {
                                                        manager.altTreesMain.Log("Error. treeTemp.currentCrossFadeId == 6");
                                                    }
                                                    else if (treeTemp.currentCrossFadeId == 5)
                                                    {
                                                        if (!isPlaying)//
                                                        {
                                                            manager.delObjMeshEditor(treeTemp.goMesh);
                                                        }
                                                        else
                                                        {
                                                            manager.delObjMesh(treeTemp);
                                                        }

                                                        if (!isPlaying)//
                                                        {
                                                            treeTemp.goMesh = manager.getObjMeshEditor(treeTemp, newLOD);

                                                            treeTemp.goMesh.GetComponent<AltTreeInstance>().isCrossFade = true;
                                                            treeTemp.crossFadeTreeMeshRenderer = treeTemp.goMesh.GetComponent<MeshRenderer>();
                                                            treeTemp.crossFadeTreeMeshRenderer.sharedMaterials = manager.treesPoolArray[treeTemp.idPrototypeIndex].objsArray[newLOD].materialsMeshCrossFade;

                                                            propBlock.Clear();
                                                            propBlock.SetColor(AltTreesManager.HueVariationLeave_PropertyID, treeTemp.color);
                                                            propBlock.SetColor(AltTreesManager.HueVariationBark_PropertyID, treeTemp.colorBark);
                                                            propBlock.SetFloat(AltTreesManager.Alpha_PropertyID, 0.0f);
                                                            propBlock.SetFloat(AltTreesManager.Ind_PropertyID, 0.0f);
                                                            propBlock.SetFloat(AltTreesManager.smoothValue_PropertyID, 0.0f);
                                                            treeTemp.alphaPropBlockMesh = 0;
                                                            treeTemp.indPropBlockMesh = 0;
                                                            treeTemp.smoothPropBlock = 0;
                                                            treeTemp.crossFadeTreeMeshRenderer.SetPropertyBlock(propBlock);
                                                        }
                                                        else
                                                        {
                                                            treeTemp.isCrossFadeMesh = true;
                                                            manager.getObjMesh(treeTemp);
                                                            if (!manager.gpuInstancingSupport || !treeTemp.gpuInstancing)
                                                            {
                                                                treeTemp.propBlockMesh.SetFloat(AltTreesManager.Alpha_PropertyID, 0f);
                                                                treeTemp.propBlockMesh.SetFloat(AltTreesManager.smoothValue_PropertyID, 0f);
                                                            }
                                                            treeTemp.alphaPropBlockMesh = 0;
                                                            treeTemp.smoothPropBlock = 0;
                                                        }

                                                        if (isPlaying)
                                                            treeTemp.crossFadeTime = Time.time + (manager.altTreesMain.altTreesManagerData.getCrossFadeTimeBillboard() - (treeTemp.crossFadeTime - Time.time));
                                                        else
                                                        {
                                                            #if UNITY_EDITOR
                                                            {
                                                                treeTemp.crossFadeTime = (float)EditorApplication.timeSinceStartup + (manager.altTreesMain.altTreesManagerData.getCrossFadeTimeBillboard() - (treeTemp.crossFadeTime - (float)EditorApplication.timeSinceStartup));
                                                            }
                                                            #endif
                                                        }
                                                        treeTemp.currentCrossFadeId = 6;
                                                        treeTemp.currentCrossFadeLOD = -1;
                                                    }
                                                    else if (treeTemp.currentCrossFadeId == 7)
                                                    {
                                                        //manager.delObjBillboard(treeTemp);

                                                        if (!isPlaying)//
                                                        {
                                                            treeTemp.goMesh = manager.getObjMeshEditor(treeTemp, newLOD);

                                                            treeTemp.goMesh.GetComponent<AltTreeInstance>().isCrossFade = true;
                                                            treeTemp.crossFadeTreeMeshRenderer = treeTemp.goMesh.GetComponent<MeshRenderer>();
                                                            treeTemp.crossFadeTreeMeshRenderer.sharedMaterials = manager.treesPoolArray[treeTemp.idPrototypeIndex].objsArray[newLOD].materialsMeshCrossFade;

                                                            propBlock.Clear();
                                                            propBlock.SetColor(AltTreesManager.HueVariationLeave_PropertyID, treeTemp.color);
                                                            propBlock.SetColor(AltTreesManager.HueVariationBark_PropertyID, treeTemp.colorBark);
                                                            propBlock.SetFloat(AltTreesManager.Alpha_PropertyID, 0.0f);
                                                            propBlock.SetFloat(AltTreesManager.Ind_PropertyID, 0.0f);
                                                            propBlock.SetFloat(AltTreesManager.smoothValue_PropertyID, 0.0f);
                                                            treeTemp.alphaPropBlockMesh = 0;
                                                            treeTemp.indPropBlockMesh = 0;
                                                            treeTemp.smoothPropBlock = 0;
                                                            treeTemp.crossFadeTreeMeshRenderer.SetPropertyBlock(propBlock);
                                                        }
                                                        else
                                                        {
                                                            treeTemp.isCrossFadeMesh = true;
                                                            manager.getObjMesh(treeTemp);
                                                            if (!manager.gpuInstancingSupport || !treeTemp.gpuInstancing)
                                                            {
                                                                treeTemp.propBlockMesh.SetFloat(AltTreesManager.Alpha_PropertyID, 0f);
                                                                treeTemp.propBlockMesh.SetFloat(AltTreesManager.smoothValue_PropertyID, 0f);
                                                            }
                                                            treeTemp.alphaPropBlockMesh = 0;
                                                            treeTemp.smoothPropBlock = 0;
                                                        }

                                                        treeTemp.currentCrossFadeId = 1;
                                                        treeTemp.currentCrossFadeLOD = -1;
                                                    }
                                                    else if (is_3_OR_4_CrossFade)
                                                    {
                                                        manager.altTreesMain.Log("Error. treeTemp.currentCrossFadeId == " + treeTemp.currentCrossFadeId);
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                if (!isPlaying)//
                                                {
                                                    manager.needUpdateScene = true;
                                                    treeTemp.goMesh = manager.getObjMeshEditor(treeTemp, newLOD);
                                                }
                                                else
                                                    manager.getObjMesh(treeTemp);
                                            }
                                        }
                                        else if (newLOD == -2)               //  newLOD is billboard
                                        {
                                            if (isPlaying && manager.altTreesMain.altTreesManagerData.enableColliders)
                                            {
                                                if (manager.treesPoolArray[treeTemp.idPrototypeIndex].tree.isBillboardColliders)
                                                {
                                                    treeTemp.collider = manager.getColliderPool(treeTemp.idPrototype, true, treeTemp);
                                                    scaleTemp.x = treeTemp.widthScale;
                                                    scaleTemp.y = treeTemp.heightScale;
                                                    scaleTemp.z = treeTemp.widthScale;
                                                    treeTemp.collider.go.transform.localScale = scaleTemp;
                                                    treeTemp.collider.go.transform.position = treeTemp.getPosWorld();
                                                    treeTemp.collider.go.transform.rotation = Quaternion.AngleAxis(treeTemp.rotation, Vector3.up);
                                                }
                                            }

                                            if (manager.treesPoolArray[treeTemp.idPrototypeIndex].tree.isBillboardCrossFade && !manager.isSelectionTree)
                                            {
                                                if (treeTemp.currentCrossFadeId == -1)
                                                {
                                                    manager.treesCrossFade.Add(treeTemp);
                                                    treeTemp.currentCrossFadeId = 8;
                                                    treeTemp.currentCrossFadeLOD = -1;
                                                
                                                    manager.getObjBillboard(treeTemp);

                                                    manager.needUpdateScene = true;

                                                    treeTemp.alphaPropBlockBillboard = 0f;
                                                    if (!manager.gpuInstancingSupport)
                                                        treeTemp.propBlockBillboards.SetFloat(AltTreesManager.Alpha_PropertyID, 0f);
                                                    treeTemp.isCrossFadeBillboard = true;

                                                    if (isPlaying)
                                                        treeTemp.crossFadeTime = Time.time + manager.altTreesMain.altTreesManagerData.getCrossFadeTimeBillboard();
                                                    else
                                                    {
                                                        #if UNITY_EDITOR
                                                        {
                                                            treeTemp.crossFadeTime = (float)EditorApplication.timeSinceStartup + manager.altTreesMain.altTreesManagerData.getCrossFadeTimeBillboard();
                                                        }
                                                        #endif
                                                    }
                                                }
                                                else
                                                {
                                                    if (treeTemp.currentCrossFadeId == 5)
                                                    {
                                                        treeTemp.currentCrossFadeId = 2;

                                                        manager.getObjBillboard(treeTemp);

                                                        manager.needUpdateScene = true;

                                                        treeTemp.alphaPropBlockBillboard = 0f;
                                                        if (!manager.gpuInstancingSupport)
                                                            treeTemp.propBlockBillboards.SetFloat(AltTreesManager.Alpha_PropertyID, 0f);
                                                        treeTemp.isCrossFadeBillboard = true;
                                                    }
                                                    else if (treeTemp.currentCrossFadeId == 7)
                                                    {
                                                        treeTemp.currentCrossFadeId = 8;
                                                        treeTemp.currentCrossFadeLOD = -1;
                                                    }
                                                    else if (treeTemp.currentCrossFadeId == 8)
                                                    {
                                                        manager.altTreesMain.Log("Error. treeTemp.currentCrossFadeId == 8");
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                manager.getObjBillboard(treeTemp);

                                                manager.needUpdateScene = true;
                                            }
                                        }
                                    }

                                    treeTemp.currentLOD = newLOD;
                                }
                            }
                        }
                    }
                }
            }
        }

        void deleteTrees()
        {
            bool isStop = false;

            lock (treesLock)
            {
                for (int i = 0; i < treesCount; i++)
                {
                    isStop = false;

                    if (!manager.altTreesMain.isPlaying && trees[i].currentLOD >= 0)
                    {
                        if (manager.isSelectionTree)
                        {
                            lock (manager.distanceCamerasLock)
                            {
                                for (int j = 0; j < manager.distanceCameras.Length; j++)
                                {
                                    if (manager.distanceCameras[j].trans.Equals(trees[i].goMesh.transform))
                                    {
                                        isStop = true;
                                        break;
                                    }
                                }
                            }
                        }
                    }

                    if (!isStop)
                    {
                        deleteTreeCheckCrossFade(trees[i]);
                    }
                }
            }

            lock (treesNoBillbLock)
            {
                for (int i = 0; i < treesNoBillbCount; i++)
                {
                    isStop = false;

                    if (!manager.altTreesMain.isPlaying && treesNoBillb[i].currentLOD >= 0)
                    {
                        if (manager.isSelectionTree)
                        {
                            lock (manager.distanceCamerasLock)
                            {
                                for (int j = 0; j < manager.distanceCameras.Length; j++)
                                {
                                    if (treesNoBillb[i].goMesh != null && manager.distanceCameras[j].trans.Equals(treesNoBillb[i].goMesh.transform))
                                    {
                                        isStop = true;
                                        break;
                                    }
                                }
                            }
                        }
                    }

                    if (!isStop)
                    {
                        deleteTreeCheckCrossFade(treesNoBillb[i]);
                    }
                }
            }
        }


        public void deleteTreeCheckCrossFade(AltTreesTrees att)
        {
            if (manager.altTreesMain.isPlaying && manager.altTreesMain.altTreesManagerData.enableColliders)
            {
                if (att.currentLOD != -1)
                {
                    if (att.currentLOD == -2)
                    {
                        if (manager.treesPoolArray[att.idPrototypeIndex].tree.isBillboardColliders)
                            manager.delColliderPool(att.idPrototype, att.collider, true, att);
                    }
                    else if (att.currentLOD != -3)
                    {
                        if (manager.treesPoolArray[att.idPrototypeIndex].tree.isColliders)
                            manager.delColliderPool(att.idPrototype, att.collider, false, att);  //11
                    }
                }
            }

            if (att.currentLOD != -1)
            {

                if (att.currentCrossFadeId == -1 && att.currentLOD != -3)
                {
                    if (att.currentLOD != -2)
                    {
                        if (!manager.altTreesMain.isPlaying)//
                        {
                            manager.delObjMeshEditor(att.goMesh);
                        }
                        else
                            manager.delObjMesh(att);
                    }
                    else
                    {
                        manager.delObjBillboard(att);
                    }
                }
                else if (att.currentCrossFadeId == 1)
                {
                    manager.delObjBillboard(att);
                    if (!manager.altTreesMain.isPlaying)//
                    {
                        manager.delObjMeshEditor(att.goMesh);
                        att.crossFadeTreeMeshRenderer = null;
                        att.goMesh = null;
                    }
                    else
                        manager.delObjMesh(att);

                    att.currentCrossFadeId = -1;
                    manager.treesCrossFade.Remove(att);
                }
                else if (att.currentCrossFadeId == 6)
                {
                    if (!manager.altTreesMain.isPlaying)//
                    {
                        manager.delObjMeshEditor(att.goMesh);
                        att.goMesh = null;
                        att.crossFadeTreeMeshRenderer = null;
                    }
                    else
                        manager.delObjMesh(att);
                    
                    att.currentCrossFadeId = -1;
                    manager.treesCrossFade.Remove(att);
                }
                else if (att.currentCrossFadeId == 7)
                {
                    manager.delObjBillboard(att);
                    att.currentCrossFadeId = -1;
                    manager.treesCrossFade.Remove(att);
                }
                else if (att.currentCrossFadeId == 2)
                {
                    if (!manager.altTreesMain.isPlaying)//
                    {
                        manager.delObjMeshEditor(att.goMesh);
                        att.goMesh = null;
                        att.crossFadeTreeMeshRenderer = null;
                    }
                    else
                        manager.delObjMesh(att);
                    manager.delObjBillboard(att);
                    
                    att.currentCrossFadeId = -1;
                    manager.treesCrossFade.Remove(att);
                }
                else if (att.currentCrossFadeId == 5)
                {
                    if (!manager.altTreesMain.isPlaying)//
                    {
                        manager.delObjMeshEditor(att.goMesh);
                        att.goMesh = null;
                        att.crossFadeTreeMeshRenderer = null;
                    }
                    else
                        manager.delObjMesh(att);
                    
                    att.currentCrossFadeId = -1;
                    manager.treesCrossFade.Remove(att);
                }
                else if (att.currentCrossFadeId == 8)
                {
                    manager.delObjBillboard(att);

                    att.currentCrossFadeId = -1;
                    manager.treesCrossFade.Remove(att);
                }
                else if (att.currentCrossFadeId == 3)
                {
                    if (!manager.altTreesMain.isPlaying)//
                    {
                        manager.delObjMeshEditor(att.goMesh);
                        att.goMesh = null;
                        att.crossFadeTreeMeshRenderer = null;
                    }
                    else
                        manager.delObjMesh(att);

                    att.currentCrossFadeId = -1;
                    manager.treesCrossFade.Remove(att);
                }
                else if (att.currentCrossFadeId == 4)
                {
                    if (!manager.altTreesMain.isPlaying)//
                    {
                        manager.delObjMeshEditor(att.goMesh);
                        att.goMesh = null;
                        att.crossFadeTreeMeshRenderer = null;
                    }
                    else
                        manager.delObjMesh(att);

                    att.currentCrossFadeId = -1;
                    manager.treesCrossFade.Remove(att);
                }
            
                att.countCheckLODs = 0;
                att.currentLOD = -1;
            }
        }


        public void checkTreesAdd(float _posX, float _posZ, AltTreesTrees _tree, int _altTreesId, bool groupBillb = true)
	    {
            _tree.altTreesId = _altTreesId;
            if (bound.inBounds(_posX, _posZ, quadId))
		    {
                if (groupBillb)
                {
                    lock (treePrefabsCountLock)
                    {
                        if (!treePrefabsCount.ContainsKey(_tree.idPrototype))
                            treePrefabsCount.Add(_tree.idPrototype, 0);
                        treePrefabsCount[_tree.idPrototype]++;
                    }
                    lock (treesLock)
                    {
                        treesCount++;
                        trees.Add(_tree);
                    }
                    isInitBillboards = false;
                    isRender = false;
                }
                else if(LOD == maxLOD)
                {
                    lock (treesNoBillbLock)
                    {
                        treesNoBillbCount++;
                        treesNoBillb.Add(_tree);
                    }
                }

                if (isChildQuads)
                {
                    quads[0].checkTreesAdd(_posX, _posZ, _tree, _altTreesId, groupBillb);
                    quads[1].checkTreesAdd(_posX, _posZ, _tree, _altTreesId, groupBillb);
                    quads[2].checkTreesAdd(_posX, _posZ, _tree, _altTreesId, groupBillb);
                    quads[3].checkTreesAdd(_posX, _posZ, _tree, _altTreesId, groupBillb);
                }
		    }
        }

        public AltTreesTrees findObjectById(int id)
        {
            for (int i = 0; i < treesNoBillb.Count; i++)
            {
                if (treesNoBillb[i].idTree == id)
                    return treesNoBillb[i];
            }

            return null;
        }

        public void getObjectQuadId(float _posX, float _posZ, ref int idQuad, ref bool stop)
        {
            if (!stop)
            {
                if (bound.inBounds(_posX, _posZ, quadId))
                {
                    if (LOD == maxLOD)
                    {
                        idQuad = objectsQuadId;
                        stop = true;
                        return;
                    }

                    if (isChildQuads)
                    {
                        quads[0].getObjectQuadId(_posX, _posZ, ref idQuad, ref stop);
                        quads[1].getObjectQuadId(_posX, _posZ, ref idQuad, ref stop);
                        quads[2].getObjectQuadId(_posX, _posZ, ref idQuad, ref stop);
                        quads[3].getObjectQuadId(_posX, _posZ, ref idQuad, ref stop);
                    }
                }
            }
        }

        public void checkTreesInitTrue()
        {
            isInitTrees = true;
            if (isChildQuads)
            {
                quads[0].checkTreesInitTrue();
                quads[1].checkTreesInitTrue();
                quads[2].checkTreesInitTrue();
                quads[3].checkTreesInitTrue();
            }
        }

        public void checkTreesAddQuads(object _level)
        {
            checkTreesAddQuads((int)_level);
        }

        public void checkTreesAddQuads(int level)
        {
            #if UNITY_EDITOR
            try
            #endif
            {
                if (isChildQuads)
                {
                    Vector3 vector3Temp;
                    lock (treesLock)
                    {
                        for (int i = 0; i < treesCount; i++)
                        {
                            vector3Temp = trees[i].getPosWorld();
                            for (int h = 0; h < 4; h++)
                            {
                                if (quads[h].bound.inBounds(vector3Temp.x, vector3Temp.z, quads[h].quadId))
                                {
                                    lock (treePrefabsCountLock)
                                    {
                                        if (!quads[h].treePrefabsCount.ContainsKey(trees[i].idPrototype))
                                            quads[h].treePrefabsCount.Add(trees[i].idPrototype, 0);
                                        quads[h].treePrefabsCount[trees[i].idPrototype]++;
                                    }
                                    quads[h].treesCount++;
                                    quads[h].trees.Add(trees[i]);
                                    quads[h].isInitBillboards = false;
                                    quads[h].isRender = false;

                                    h = 4;
                                }
                            }
                        }
                    }

                    quads[0].isInitTrees = true;
                    quads[1].isInitTrees = true;
                    quads[2].isInitTrees = true;
                    quads[3].isInitTrees = true;
                    isInitTrees = true;

                    if (level == 0 || level == 1)
                    {
                        if (quads[0].isChildQuads)
                            ThreadPool.QueueUserWorkItem(quads[0].checkTreesAddQuads, level + 1);
                        if (quads[1].isChildQuads)
                            ThreadPool.QueueUserWorkItem(quads[1].checkTreesAddQuads, level + 1);
                        if (quads[2].isChildQuads)
                            ThreadPool.QueueUserWorkItem(quads[2].checkTreesAddQuads, level + 1);
                        if (quads[3].isChildQuads)
                            ThreadPool.QueueUserWorkItem(quads[3].checkTreesAddQuads, level + 1);
                    }
                    else
                    {
                        if (quads[0].isChildQuads)
                            quads[0].checkTreesAddQuads(level + 1);
                        if (quads[1].isChildQuads)
                            quads[1].checkTreesAddQuads(level + 1);
                        if (quads[2].isChildQuads)
                            quads[2].checkTreesAddQuads(level + 1);
                        if (quads[3].isChildQuads)
                            quads[3].checkTreesAddQuads(level + 1);
                    }
                }
            }
            #if UNITY_EDITOR
            catch (System.Exception e)
            {
                Debug.LogError(e);
            }
            #endif
        }

        public void removeMeshes()
        {
            for (int i = 0; i < meshes.Count; i++)
            {
                if (manager.altTreesMain.isPlaying)
                {
                    Object.Destroy(meshes[i].mesh);
                }
                else
                {
                    Object.DestroyImmediate(meshes[i].mesh);
                }
            }
            meshes.Clear();

            if (isChildQuads)
            {
                quads[0].removeMeshes();
                quads[1].removeMeshes();
                quads[2].removeMeshes();
                quads[3].removeMeshes();
            }
        }
	
	
	
    }


    public class TreesToRender
    {
        public AltTreesTrees att = null;
        public bool noNull = false;
    }

    public class AltTreesPool
    {
	    public AltTree tree;
        public float treeSize = 0;
        public Mesh mesh;
        public TreesToRender[] treesToRender;
        public int treesToRenderCount = 0;
        public int treesToRenderLength = 0;
        
        public List<int> treesToRenderDeleted = new List<int>();
        public int treesToRenderDeletedCount = 0;
        public objsArr[] objsArray = new objsArr[0];
        public List<ColliderPool> collidersArray = new List<ColliderPool>();
        public List<ColliderPool> colliderBillboardsArray = new List<ColliderPool>();
        public int collidersMaxCount = 0;
        public int colliderBillboardsMaxCount = 0;
        public Material materialBillboard;
        public Material materialBillboardCrossFade;
        public Material materialBillboardGroup;
        public int needInitCollidersCount = 0;
        public int needInitBillboardCollidersCount = 0;
    }

    public class ColliderPool
    {
        public GameObject go;
        public AltCollider[] colliders;
    }

    public class objsArr
    {
        public Mesh mesh;
        public Material[] materialsMesh;
        public Material[] materialsMeshCrossFade;

        public AltTreesTrees[] attDraw;
        public AltTreesTrees[] attDrawCrossFade;
        public int attDrawCount = 0;
        public int attDrawCrossFadeCount = 0;
    }

    public class objBillboardPool
    {
        public Mesh ms;
        public GameObject go;
        public MeshRenderer mr;
    }
    
    public class MeshToRender
    {
        public Mesh mesh;
        public int materialId;

        public MeshToRender(Mesh _mesh, int _materialId)
        {
            mesh = _mesh;
            materialId = _materialId;
        }
    }
}




