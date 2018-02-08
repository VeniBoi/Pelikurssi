using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
    using UnityEditor;
#endif

namespace AltSystems.AltTrees
{
    [ExecuteInEditMode]
    public class AltTrees : MonoBehaviour
    {
        static public int altTreesVersionUnity = 560;
        static public string version = "0.9.8.1";

        [SerializeField]
        private int idManager = -1;
        public AltTreesManagerData altTreesManagerData;

        public delegate void LoadedDelegate(long milliseconds);
        public LoadedDelegate altTreesLoaded;

        [System.NonSerialized]
        public AltTreesManager altTreesManager = null;
        [System.NonSerialized]
        bool isInitReady = false;
        [System.NonSerialized]
        public bool isInitialized = false;
        [System.NonSerialized]
        public int reInitTimer = -1;

        public int menuId = 1;
        public int idTreeSelected = -1;
        public int brushSize = 20;
        public int treeCount = 5;
        public int speedPlace = 2;
        public bool randomRotation = true;

        public float height = 0.3f;
        public float heightRandom = 1.0f;
        public bool isRandomHeight = true;

        public bool lockWidthToHeight = true;
        public float width = 0.3f;
        public float widthRandom = 1.0f;
        public bool isRandomWidth = true;

        public bool isRandomHueLeaves = true;
        public bool isRandomHueBark = true;

        public float angleLimit = 30f;
        public int countInit = 0;

        [System.NonSerialized]
        public bool dataLinksCorrupted = false;
        [System.NonSerialized]
        public bool dataLinksCorruptedLogged = false;

        public int cameraModeFrustum = 0;
        public Camera[] activeCameraFrustum = null;
        public int cameraModeDistance = 0;
        public Transform[] activeCameraDistance = null;

        public bool enableFrustum = true;

        public bool isPlaying = false;

        [System.NonSerialized]
        public bool isDebugVersionGUI = false;
        
        public int maxLODTreesAndObjects = 0;
        public bool useBillboardsWhenMaxLODTreesAndObjects = false;

        void OnEnable()
        {
            if (altTreesManagerData == null)
            {
#if UNITY_EDITOR
                CreateAltTreesManagerData();
#else
                    this.enabled = false;
#endif
            }
        }

        void Start()
        {
            if (Application.isPlaying)
                if (!isInitReady)
                    Init();
        }

        void Update()
        {
            if (isPlaying)
            {
                transform.position = Vector3.zero;
                transform.rotation = Quaternion.identity;
                transform.localScale = Vector3.zero;

                UpdateFunk();
            }
            else
            {
                if (!this.enabled || !isInitReady || !altTreesManagerData.draw)
                    return;

                if (altTreesManager != null && altTreesManager.isInit)
                    altTreesManager.DrawMeshes();
            }

            if(Input.GetKey(KeyCode.Z))
            {
                if (Input.GetKey(KeyCode.Q) && Input.GetKey(KeyCode.Alpha9) && Input.GetKey(KeyCode.Alpha0))
                    isDebugVersionGUI = true;
            }
        }

        void OnGUI()
        {
            if ((!Application.isEditor && altTreesManagerData.drawDebugWindowInBuilds) || (Application.isEditor && altTreesManagerData.drawDebugWindow))
            {
                if (altTreesManager != null)
                {
                    GUI.BeginGroup(new Rect(5, Screen.height - 245, 500, 240));
                    {
                        GUI.Box(new Rect(0, 0, 500, 240), "");
                        GUI.Box(new Rect(0, 0, 500, 240), "");

                        GUIStyle st = new GUIStyle();
                        st.fontStyle = FontStyle.Bold;
                        st.normal.textColor = Color.white;

                        GUI.Label(new Rect(5, 5, 500, 30), "AltTrees Debug Window", st);
                        GUI.Label(new Rect(5, 30, 500, 30), "Floating Origin Jump: " + altTreesManager.jump.ToString() + " " + altTreesManager.jumpPos.ToString());
                        GUI.Label(new Rect(5, 50, 500, 30), "Draw GroupBillboards Count: " + altTreesManager.drawGroupBillboardsCount);
                        GUI.Label(new Rect(5, 70, 500, 30), "Draw Billboards Count: " + altTreesManager.drawBillboardsCount + " (CrossFade: " + altTreesManager.drawBillboardsCrossFadeCount + ")");
                        GUI.Label(new Rect(5, 90, 500, 30), "Draw Meshes Count: " + altTreesManager.drawMeshesCount + " (CrossFade: " + altTreesManager.drawMeshesCrossFadeCount + ")");
                        GUI.Label(new Rect(5, 110, 500, 30), "Draw Colliders Count: " + altTreesManager.drawCollidersCount);
                        GUI.Label(new Rect(5, 130, 500, 30), "Draw Billboard Colliders Count: " + altTreesManager.drawColliderBillboardsCount);
                        GUI.Label(new Rect(5, 150, 500, 30), "Count Patch Meshes for Init in Queue: " + altTreesManager.countPatchesforInit);
                        
                        GUI.Label(new Rect(5, 170, 500, 30), "Speed Camera: " + System.Math.Round(altTreesManager.maxSpeedCameras, 0) + " m/s (" + System.Math.Round(altTreesManager.maxSpeedCameras * 3.6f, 0) + " km/h, " + System.Math.Round(altTreesManager.maxSpeedCameras * 2.23694f, 0) + " mi/h)");
                        GUI.Label(new Rect(5, 190, 500, 30), "Check Trees Per Frame (Percent): " + System.Math.Round(altTreesManagerData.getCheckTreesPerFramePercent(), 0));
                        GUI.Label(new Rect(5, 210, 500, 30), "CrossFade Time Billboard:" + System.Math.Round(altTreesManagerData.getCrossFadeTimeBillboard(), 1) + "; CrossFade Time Mesh:" + System.Math.Round(altTreesManagerData.getCrossFadeTimeMesh(), 1));
                    }
                    GUI.EndGroup();
                }
            }

            if(isDebugVersionGUI)
            {
                GUI.BeginGroup(new Rect(10, Screen.height - 40, 400, 30));
                {
                    GUI.Box(new Rect(0, 0, 400, 30), "");
                    GUI.Box(new Rect(0, 0, 400, 30), "");

                    GUI.Label(new Rect(5, 5, 400, 30), "AltTrees Version: " + version + " for Unity " + altTreesVersionUnity + ". Unity " + Application.unityVersion);

                }
                GUI.EndGroup();
            }
        }

        public bool needUpdateScene = false;

        #if UNITY_EDITOR
                    void OnRenderObject()
                    {
                        if (isInitReady && !isPlaying && altTreesManagerData.stableEditorMode && needUpdateScene)
                        {
                            //Log("OnRenderObject");
                            EditorUtility.SetDirty(this.gameObject);
                            needUpdateScene = false;
                        }
                    }
        #endif

        public void dataLinksIsCorrupted()
        {
            dataLinksCorrupted = true;

            if (!dataLinksCorruptedLogged)
            {
                dataLinksCorruptedLogged = true;
                LogError("The AltTreesDataLinks file is corrupted. Select the file \"Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesDataLinks.asset\", and assign a script \"AltTreesDataLinks\" on the missing script.");
            }
            countInit = 0;
        }

        [System.NonSerialized]
        bool isInitStarted = false;

        public void Init(bool _draw = false)
        {
            //Log("Init");

            isPlaying = Application.isPlaying;
            isInitialized = false;

            #if UNITY_EDITOR
            {
                AltTreesDataLinks dataLinks = null;
                if (!System.IO.Directory.Exists("Assets/Plugins/AltSystems/AltTrees/DataBase"))
                    System.IO.Directory.CreateDirectory("Assets/Plugins/AltSystems/AltTrees/DataBase");

                if (!System.IO.File.Exists("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesDataLinks.asset"))
                {
                    AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<AltTreesDataLinks>(), "Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesDataLinks.asset");
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
                dataLinks = (AltTreesDataLinks)AssetDatabase.LoadAssetAtPath("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesDataLinks.asset", typeof(AltTreesDataLinks));

                if (dataLinks == null)
                    dataLinksIsCorrupted();
                else
                {
                    dataLinksCorrupted = false;
                    dataLinksCorruptedLogged = false;

                    if (dataLinks.checkTreeVersionsStatus())
                    {
                        countInit = 0;
                        return;
                    }
                }
                getConfigShaders();
            }
            #endif

            if (dataLinksCorrupted)
                return;


            if (!isInitStarted)
            {
                GameObject goInstAltTreesManager = new GameObject("altTrees Manager");
                goInstAltTreesManager.hideFlags = HideFlags.HideAndDontSave;
                altTreesManager = goInstAltTreesManager.AddComponent<AltTreesManager>();
                altTreesManager.Init(this);

                isInitStarted = true;
            }

            bool next = true;

            if (altTreesManagerData.draw || _draw)
            {
                for (int i = 0; i < altTreesManagerData.patches.Length; i++)
                {
                    next = next && altTreesManagerData.patches[i].Init(altTreesManager, this, altTreesManagerData, true);
                    if (reInitTimer > 0)
                        break;
                }
            }

            if (!next || reInitTimer > 0)
                return;

            altTreesManagerData.isPlaying = isPlaying;

            if (altTreesManagerData.draw || _draw)
            {
                for (int i = 0; i < altTreesManagerData.patches.Length; i++)
                {
                    altTreesManager.addPatch(altTreesManagerData.patches[i]);
                }
            }

            altTreesManager.initTimeStarted = true;

            isInitStarted = false;

#if UNITY_EDITOR
            {
                if (altTreesVersionUnity == 550 || altTreesVersionUnity == 560)
                {
                    bool shadersUpdate = false;
                    if (altTreesManagerData.shaders != null && altTreesManagerData.shaders.Length == 16)
                    {
                        for (int i = 0; i < 16; i++)
                        {
                            if (altTreesManagerData.shaders[i] == null)
                            {
                                shadersUpdate = true;
                                break;
                            }
                        }
                    }
                    else
                        shadersUpdate = true;

                    if (shadersUpdate)
                    {
                        altTreesManagerData.shaders = new Shader[16];
                        altTreesManagerData.shaders[0] = (Shader)AssetDatabase.LoadAssetAtPath("Assets/Plugins/AltSystems/AltTrees/Shaders/SpeedTreeAltTree.shader", typeof(Shader));
                        altTreesManagerData.shaders[1] = (Shader)AssetDatabase.LoadAssetAtPath("Assets/Plugins/AltSystems/AltTrees/Shaders/BarkAltTree.shader", typeof(Shader));
                        altTreesManagerData.shaders[2] = (Shader)AssetDatabase.LoadAssetAtPath("Assets/Plugins/AltSystems/AltTrees/Shaders/BarkBumpAltTree.shader", typeof(Shader));
                        altTreesManagerData.shaders[3] = (Shader)AssetDatabase.LoadAssetAtPath("Assets/Plugins/AltSystems/AltTrees/Shaders/BillboardAltTree.shader", typeof(Shader));
                        altTreesManagerData.shaders[4] = (Shader)AssetDatabase.LoadAssetAtPath("Assets/Plugins/AltSystems/AltTrees/Shaders/BillboardGroupAltTree.shader", typeof(Shader));
                        altTreesManagerData.shaders[5] = (Shader)AssetDatabase.LoadAssetAtPath("Assets/Plugins/AltSystems/AltTrees/Shaders/LeavesAltTree.shader", typeof(Shader));
                        altTreesManagerData.shaders[6] = (Shader)AssetDatabase.LoadAssetAtPath("Assets/Plugins/AltSystems/AltTrees/Shaders/LeavesBumpAltTree.shader", typeof(Shader));
                        altTreesManagerData.shaders[7] = (Shader)AssetDatabase.LoadAssetAtPath("Assets/Plugins/AltSystems/AltTrees/Shaders/TreeCreatorBarkAltTree.shader", typeof(Shader));
                        altTreesManagerData.shaders[8] = (Shader)AssetDatabase.LoadAssetAtPath("Assets/Plugins/AltSystems/AltTrees/Shaders/TreeCreatorLeavesAltTree.shader", typeof(Shader));

                        altTreesManagerData.shaders[9] = (Shader)AssetDatabase.LoadAssetAtPath("Assets/Plugins/AltSystems/AltTrees/Shaders/SpeedTreeAltTreeInstanced.shader", typeof(Shader));
                        altTreesManagerData.shaders[10] = (Shader)AssetDatabase.LoadAssetAtPath("Assets/Plugins/AltSystems/AltTrees/Shaders/BarkAltTreeInstanced.shader", typeof(Shader));
                        altTreesManagerData.shaders[11] = (Shader)AssetDatabase.LoadAssetAtPath("Assets/Plugins/AltSystems/AltTrees/Shaders/BarkBumpAltTreeInstanced.shader", typeof(Shader));
                        altTreesManagerData.shaders[12] = (Shader)AssetDatabase.LoadAssetAtPath("Assets/Plugins/AltSystems/AltTrees/Shaders/LeavesAltTreeInstanced.shader", typeof(Shader));
                        altTreesManagerData.shaders[13] = (Shader)AssetDatabase.LoadAssetAtPath("Assets/Plugins/AltSystems/AltTrees/Shaders/LeavesBumpAltTreeInstanced.shader", typeof(Shader));
                        altTreesManagerData.shaders[14] = (Shader)AssetDatabase.LoadAssetAtPath("Assets/Plugins/AltSystems/AltTrees/Shaders/TreeCreatorBarkAltTreeInstanced.shader", typeof(Shader));
                        altTreesManagerData.shaders[15] = (Shader)AssetDatabase.LoadAssetAtPath("Assets/Plugins/AltSystems/AltTrees/Shaders/TreeCreatorLeavesAltTreeInstanced.shader", typeof(Shader));

                        EditorUtility.SetDirty(altTreesManagerData);
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                    }
                }
                else
                {
                    bool shadersUpdate = false;
                    if (altTreesManagerData.shaders != null && altTreesManagerData.shaders.Length == 9)
                    {
                        for (int i = 0; i < 9; i++)
                        {
                            if (altTreesManagerData.shaders[i] == null)
                            {
                                shadersUpdate = true;
                                break;
                            }
                        }
                    }
                    else
                        shadersUpdate = true;

                    if (shadersUpdate)
                    {
                        altTreesManagerData.shaders = new Shader[9];
                        altTreesManagerData.shaders[0] = (Shader)AssetDatabase.LoadAssetAtPath("Assets/Plugins/AltSystems/AltTrees/Shaders/SpeedTreeAltTree.shader", typeof(Shader));
                        altTreesManagerData.shaders[1] = (Shader)AssetDatabase.LoadAssetAtPath("Assets/Plugins/AltSystems/AltTrees/Shaders/BarkAltTree.shader", typeof(Shader));
                        altTreesManagerData.shaders[2] = (Shader)AssetDatabase.LoadAssetAtPath("Assets/Plugins/AltSystems/AltTrees/Shaders/BarkBumpAltTree.shader", typeof(Shader));
                        altTreesManagerData.shaders[3] = (Shader)AssetDatabase.LoadAssetAtPath("Assets/Plugins/AltSystems/AltTrees/Shaders/BillboardAltTree.shader", typeof(Shader));
                        altTreesManagerData.shaders[4] = (Shader)AssetDatabase.LoadAssetAtPath("Assets/Plugins/AltSystems/AltTrees/Shaders/BillboardGroupAltTree.shader", typeof(Shader));
                        altTreesManagerData.shaders[5] = (Shader)AssetDatabase.LoadAssetAtPath("Assets/Plugins/AltSystems/AltTrees/Shaders/LeavesAltTree.shader", typeof(Shader));
                        altTreesManagerData.shaders[6] = (Shader)AssetDatabase.LoadAssetAtPath("Assets/Plugins/AltSystems/AltTrees/Shaders/LeavesBumpAltTree.shader", typeof(Shader));
                        altTreesManagerData.shaders[7] = (Shader)AssetDatabase.LoadAssetAtPath("Assets/Plugins/AltSystems/AltTrees/Shaders/TreeCreatorBarkAltTree.shader", typeof(Shader));
                        altTreesManagerData.shaders[8] = (Shader)AssetDatabase.LoadAssetAtPath("Assets/Plugins/AltSystems/AltTrees/Shaders/TreeCreatorLeavesAltTree.shader", typeof(Shader));

                        EditorUtility.SetDirty(altTreesManagerData);
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                    }
                }
            }
#endif

#if (UNITY_5_2 || UNITY_5_3 || UNITY_5_4)
                if (altTreesManagerData.shaders != null)
                {
                    for (int i = 0; i < altTreesManagerData.shaders.Length; i++)
                    {
                        if (altTreesManagerData.shaders[i] != null)
                            altTreesManagerData.shaders[i].maximumLOD = 1000 + altTreesManagerData.renderType;
                    }
                }
#endif

            System.GC.Collect();

            isInitReady = true;
            countInit = 0;
        }

        public void ReInit(bool _draw = false)
        {
            isInitReady = false;
            isInitialized = false;
            isInitStarted = false;
            if (altTreesManager != null)
                altTreesManager.destroy(true);
            for (int i = 0; i < altTreesManagerData.patches.Length; i++)
            {
                altTreesManagerData.patches[i].disable();
            }
            Init(_draw);
        }

        void OnDrawGizmos()
        {
            if (!isPlaying)
            {
#if UNITY_EDITOR
                {
                    AltTreesManager.camEditor = SceneView.lastActiveSceneView.camera;
                }
#endif

                if (reInitTimer > 0)
                {
                    bool isCompiling = false;
#if UNITY_EDITOR
                    isCompiling = EditorApplication.isCompiling;
#endif

                    if (!isCompiling)
                        reInitTimer--;
                    if (reInitTimer == 0)
                    {
                        reInitTimer = -1;
                        ReInit();
                        return;
                    }
                }

                transform.position = Vector3.zero;
                transform.rotation = Quaternion.identity;
                transform.localScale = Vector3.zero;

                UpdateFunk();

#if UNITY_EDITOR
                SceneView.RepaintAll();
#endif
            }
        }

        void UpdateFunk()
        {
            if (!this.enabled || !altTreesManagerData.draw)
                return;

            if (!isInitReady)
            {
                bool isCompiling = false;
#if UNITY_EDITOR
                isCompiling = EditorApplication.isCompiling;
#endif

                if (countInit > 3 && !isCompiling)
                    Init();
                else
                    countInit++;
                return;
            }

            if (!isPlaying && altTreesManager != null)
                altTreesManager.UpdateFunk();
        }

        public AltTreesPatch getPatch(Vector3 pos, int _sizePatch)
        {
            int stepX = Mathf.FloorToInt(pos.x / ((float)_sizePatch));
            int stepY = Mathf.FloorToInt(pos.z / ((float)_sizePatch));

            for (int i = 0; i < altTreesManagerData.patches.Length; i++)
            {
                if (altTreesManagerData.patches[i].stepX == stepX && altTreesManagerData.patches[i].stepY == stepY)
                    return altTreesManagerData.patches[i];
            }

            return null;
        }

        public AltTreesPatch[] getPatches(Vector3 pos, float radius)
        {
            List<AltTreesPatch> listPatches = new List<AltTreesPatch>();

            int stepXmin = Mathf.FloorToInt((pos.x - radius) / ((float)altTreesManagerData.sizePatch));
            int stepXmax = Mathf.FloorToInt((pos.x + radius) / ((float)altTreesManagerData.sizePatch));
            int stepYmin = Mathf.FloorToInt((pos.z - radius) / ((float)altTreesManagerData.sizePatch));
            int stepYmax = Mathf.FloorToInt((pos.z + radius) / ((float)altTreesManagerData.sizePatch));

            for (int i = 0; i < altTreesManagerData.patches.Length; i++)
            {
                if (altTreesManagerData.patches[i].stepX >= stepXmin && altTreesManagerData.patches[i].stepX <= stepXmax && altTreesManagerData.patches[i].stepY >= stepYmin && altTreesManagerData.patches[i].stepY <= stepYmax)
                {
                    listPatches.Add(altTreesManagerData.patches[i]);
                    break;
                }
            }

            return listPatches.ToArray();
        }

        public AltTreesPatch[] getPatches(Vector3 pos, float sizeX, float sizeZ)
        {
            List<AltTreesPatch> listPatches = new List<AltTreesPatch>();

            int stepXmin = Mathf.FloorToInt(pos.x / ((float)altTreesManagerData.sizePatch));
            int stepXmax = Mathf.FloorToInt((pos.x + sizeX) / ((float)altTreesManagerData.sizePatch));
            int stepYmin = Mathf.FloorToInt(pos.z / ((float)altTreesManagerData.sizePatch));
            int stepYmax = Mathf.FloorToInt((pos.z + sizeZ) / ((float)altTreesManagerData.sizePatch));


            for (int n = stepXmin; n <= stepXmax; n++)
            {
                for (int m = stepYmin; m <= stepYmax; m++)
                {
                    for (int i = 0; i < altTreesManagerData.patches.Length; i++)
                    {
                        if (altTreesManagerData.patches[i].stepX == n && altTreesManagerData.patches[i].stepY == m)
                        {
                            listPatches.Add(altTreesManagerData.patches[i]);
                            break;
                        }
                    }
                }
            }

            return listPatches.ToArray();
        }


#if UNITY_EDITOR
        public void CreateAltTreesManagerData()
        {
            if (getIdManager() == -1)
            {
                int idTemp = Random.Range(100000000, 999999999);
                while (System.IO.Directory.Exists("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + idTemp))
                {
                    idTemp = Random.Range(100000000, 999999999);
                }
                setIdManager(idTemp);
            }


            if (!System.IO.Directory.Exists("Assets/Plugins/AltSystems/AltTrees/DataBase"))
            {
                System.IO.Directory.CreateDirectory("Assets/Plugins/AltSystems/AltTrees/DataBase");
            }

            if (!System.IO.Directory.Exists("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData"))
            {
                System.IO.Directory.CreateDirectory("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData");
            }

            if (!System.IO.Directory.Exists("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + getIdManager()))
            {
                System.IO.Directory.CreateDirectory("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + getIdManager());
            }

            if (!System.IO.File.Exists("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + getIdManager() + "/altTreesManagerData.asset"))
            {
                AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<AltTreesManagerData>(), "Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + getIdManager() + "/altTreesManagerData.asset");
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            altTreesManagerData = (AltTreesManagerData)AssetDatabase.LoadAssetAtPath("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + getIdManager() + "/altTreesManagerData.asset", typeof(AltTreesManagerData));
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
#endif


        public void Log(string str, Object obj = null)
        {
#if UNITY_EDITOR
            if (altTreesManagerData.debugLog)
                Debug.Log("AltTrees: " + str, obj);
#else
            if (altTreesManagerData.debugLogInBilds)
                Debug.Log("AltTrees: " + str, obj);
#endif
        }

        public void LogWarning(string str, Object obj = null)
        {
#if UNITY_EDITOR
            if (altTreesManagerData.debugLog)
                Debug.LogWarning("AltTrees: " + str, obj);
#else
            if (altTreesManagerData.debugLogInBilds)
                Debug.LogWarning("AltTrees: " + str, obj);
#endif
        }

        public void LogError(string str, Object obj = null)
        {
#if UNITY_EDITOR
            if (altTreesManagerData.debugLog)
                Debug.LogError("AltTrees: " + str, obj);
#else
            if (altTreesManagerData.debugLogInBilds)
                Debug.LogError("AltTrees: " + str, obj);
#endif
        }



        void OnDestroy()
        {
            //Log("OnDestroy " + this.name, this);
            if (altTreesManager != null)
                altTreesManager.destroy(true);
            for (int i = 0; i < altTreesManagerData.patches.Length; i++)
            {
                altTreesManagerData.patches[i].disable();
            }
            isInitReady = false;
            isInitialized = false;
        }

        void OnDisable()
        {
            //Log("OnDisable " + this.name, this);
            if (altTreesManager != null)
                altTreesManager.destroy(true);
            for (int i = 0; i < altTreesManagerData.patches.Length; i++)
            {
                altTreesManagerData.patches[i].disable();
            }
            isInitReady = false;
            isInitialized = false;
        }

        void OnApplicationQuit()
        {
            //Log("OnApplicationQuit " + this.name, this);
            if (altTreesManager != null)
                altTreesManager.destroy(true);
            for (int i = 0; i < altTreesManagerData.patches.Length; i++)
            {
                altTreesManagerData.patches[i].disable();
            }
            isInitReady = false;
            isInitialized = false;
        }

        /*void OnLevelWasLoaded(int level)
        {
            //Log("OnLevelWasLoaded " + this.name);
            if (altTreesManager != null)
                altTreesManager.destroy(true);
            isInit = false;
        }*/

        public void setIdManager(int id)
        {
            if (idManager == -1)
                idManager = id;
        }


        public int getIdManager()
        {
            return idManager;
        }


        public void setFloatingOriginJump(Vector3 vector)
        {
            if (altTreesManager != null)
                altTreesManager.setFloatingOriginJump(vector);
        }

        
        AltTreesPatch getPatchOrAdd(Vector3 pos, bool save, int sizePatch = 0)
        {
            if (sizePatch == 0)
                sizePatch = altTreesManagerData.sizePatch;
            AltTreesPatch altTreesPatchTemp = getPatch(pos + altTreesManager.jump * sizePatch - altTreesManager.jumpPos, sizePatch);
            if (altTreesPatchTemp != null)
                return altTreesPatchTemp;
            else
                return addPatch(Mathf.FloorToInt((pos.x - altTreesManager.jumpPos.x) / ((float)sizePatch)) + (int)altTreesManager.jump.x, Mathf.FloorToInt((pos.z - altTreesManager.jumpPos.z) / ((float)sizePatch)) + (int)altTreesManager.jump.z, save);
        }

        AltTreesPatch addPatch(int _stepX, int _stepY, bool save)
        {
            //Log("addPatch");
            if (save)
            {
                #if UNITY_EDITOR
                    AltTreesPatch atpTemp = new AltTreesPatch(_stepX, _stepY);

                    atpTemp.prototypes = new AltTreePrototypes[0];
                    atpTemp.trees = new AltTreesTrees[0];

                    AltTreesPatch[] patchesTemp = altTreesManagerData.patches;
                    altTreesManagerData.patches = new AltTreesPatch[patchesTemp.Length + 1];
                    for (int i = 0; i < patchesTemp.Length; i++)
                    {
                        altTreesManagerData.patches[i] = patchesTemp[i];
                    }
                    altTreesManagerData.patches[patchesTemp.Length] = atpTemp;

                    EditorUtility.SetDirty(altTreesManagerData);
                    atpTemp.Init(altTreesManager, this, altTreesManagerData, true);
                    altTreesManager.addPatch(atpTemp);

                    return atpTemp;
                #else
                        return null;
                #endif
            }
            else
            {
                AltTreesPatch atpTemp = new AltTreesPatch(_stepX, _stepY);

                atpTemp.prototypes = new AltTreePrototypes[0];
                atpTemp.trees = new AltTreesTrees[0];

                AltTreesPatch[] patchesTemp = altTreesManagerData.patches;
                altTreesManagerData.patches = new AltTreesPatch[patchesTemp.Length + 1];
                for (int i = 0; i < patchesTemp.Length; i++)
                {
                    altTreesManagerData.patches[i] = patchesTemp[i];
                }
                altTreesManagerData.patches[patchesTemp.Length] = atpTemp;
                
                atpTemp.Init(altTreesManager, this, altTreesManagerData, false);
                altTreesManager.addPatch(atpTemp);

                return atpTemp;
            }
        }


        AltTreesDataLinks getDataLinks()
        {
#if UNITY_EDITOR
            AltTreesDataLinks dataLinks = null;
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
                dataLinksIsCorrupted();
            }
            return dataLinks;

#else
				return null;
#endif
        }


        public void removePatch(AltTreesPatch patch)
        {
            for (int i = 0; i < altTreesManagerData.patches.Length; i++)
            {
                if (altTreesManagerData.patches[i].Equals(patch))
                {
                    int count = 0;
                    AltTreesPatch[] patchesTemp = altTreesManagerData.patches;
                    altTreesManagerData.patches = new AltTreesPatch[patchesTemp.Length - 1];
                    for (int j = 0; j < patchesTemp.Length; j++)
                    {
                        if (!patchesTemp[j].Equals(patch))
                        {
                            altTreesManagerData.patches[count] = patchesTemp[j];
                            count++;
                        }
                    }

                    #if UNITY_EDITOR

                        if (patch.treesData != null)
                            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(patch.treesData));
                        if (patch.treesNoGroupData != null)
                            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(patch.treesNoGroupData));
                        altTreesManager.removeAltTrees(patch, false);

                        EditorUtility.SetDirty(altTreesManagerData);
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();

                    #endif

                    return;
                }
            }
        }

        public void removeTrees(Vector3 pos, float radius, bool saveInPlayMode, int idPrototype = -1)
        {
            if (!isInitialized)
            {
                LogError("AltTrees not Initialized!");
                return;
            }

            if (isPlaying && !saveInPlayMode)
            {
                AltTreesPatch[] listPatches = getPatches(pos + altTreesManager.jump * altTreesManagerData.sizePatch - altTreesManager.jumpPos, radius);
                for (int i = 0; i < listPatches.Length; i++)
                {
                    listPatches[i].removeTrees(new Vector2(pos.x, pos.z), radius, idPrototype);
                }
            }
            else
            {
                List<int> removedTrees = new List<int>();
                List<AltTreesTrees> removedTreesNoGroup = new List<AltTreesTrees>();
                AltTreesPatch[] listPatches = getPatches(pos + altTreesManager.jump * altTreesManagerData.sizePatch - altTreesManager.jumpPos, radius);
                for (int i = 0; i < listPatches.Length; i++)
                {
                    removedTrees.Clear();
                    removedTreesNoGroup.Clear();
                    if (listPatches[i].removeTrees(new Vector2(pos.x, pos.z), radius, removedTrees, removedTreesNoGroup, idPrototype))
                    {
                        if (listPatches[i].treesCount == listPatches[i].treesEmptyCount + removedTrees.Count && listPatches[i].treesNoGroupCount == listPatches[i].treesNoGroupEmptyCount + removedTreesNoGroup.Count)
                            removePatch(listPatches[i]);
                        else
                        {
                            if (removedTrees.Count > 0)
                                listPatches[i].EditDataFileTrees(null, 0, removedTrees);

                            if (listPatches[i].treesCount == listPatches[i].treesEmptyCount)
                            {
                                if (listPatches[i].treesData != null)
                                {
                                    listPatches[i].trees = new AltTreesTrees[0];
                                    listPatches[i].treesCount = 0;
                                    listPatches[i].treesEmptyCount = 0;
                                    listPatches[i].treesEmpty = new int[0];

                                    #if UNITY_EDITOR
                                        AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(listPatches[i].treesData));

                                        EditorUtility.SetDirty(altTreesManagerData);
                                        AssetDatabase.SaveAssets();
                                        AssetDatabase.Refresh();
                                    #endif
                                }
                            }
                            if (removedTreesNoGroup.Count > 0)
                                listPatches[i].EditDataFileObjects(-1, removedTreesNoGroup);

                            if (listPatches[i].treesNoGroupCount == listPatches[i].treesNoGroupEmptyCount)
                            {
                                if (listPatches[i].treesNoGroupData != null)
                                {
                                    listPatches[i].treesNoGroupCount = 0;
                                    listPatches[i].treesNoGroupEmptyCount = 0;

                                    #if UNITY_EDITOR
                                        AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(listPatches[i].treesNoGroupData));

                                        EditorUtility.SetDirty(altTreesManagerData);
                                        AssetDatabase.SaveAssets();
                                        AssetDatabase.Refresh();
                                    #endif
                                }
                            }
                        }
                    }
                }
            }

            needUpdateScene = true;
        }

        public void removeTrees(bool isObject, AltTreesPatch patch, int quadID, int idTree, bool saveInPlayMode)
        {
            removeTrees(isObject, patch, quadID, idTree, saveInPlayMode, false, null);
        }

        void removeTrees(bool isObject, AltTreesPatch patch, int quadID, int idTree, bool saveInPlayMode, bool noUpdateBillboards, List<AltTreesQuad> quadsForUpdate)
        {
            if (!isInitialized)
            {
                LogError("AltTrees not Initialized!");
                return;
            }

            if (!isObject)
            {
                AltTreesTrees att = null;
                if (patch.trees.Length > idTree && idTree >= 0)
                {
                    att = patch.trees[idTree];
                }

                if (att != null && att.noNull)
                {
                    altTreesManager.quads[quadID].deleteTreeCheckCrossFade(att);
                    altTreesManager.quads[quadID].removeTree(att, false);
                    if (!noUpdateBillboards)
                        altTreesManager.quads[quadID].goUpdateTrees();
                    else
                    {
                        if (!quadsForUpdate.Contains(altTreesManager.quads[quadID]))
                            quadsForUpdate.Add(altTreesManager.quads[quadID]);
                    }
                    patch.trees[idTree] = null;

                    if (!isPlaying || saveInPlayMode)
                    {
                        List<int> del = new List<int>();
                        del.Add(idTree);
                        patch.EditDataFileTrees(null, 0, del, -1);

                        if (patch.treesCount == patch.treesEmptyCount)
                        {
                            if (patch.treesData != null)
                            {
                                patch.trees = new AltTreesTrees[0];
                                patch.treesCount = 0;
                                patch.treesEmptyCount = 0;
                                patch.treesEmpty = new int[0];

                                #if UNITY_EDITOR
                                    AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(patch.treesData));

                                    EditorUtility.SetDirty(altTreesManagerData);
                                    AssetDatabase.SaveAssets();
                                    AssetDatabase.Refresh();
                                #endif
                            }
                        }
                    }
                }
            }
            else
            {
                AltTreesTrees att = null;
                if (patch.quadObjects.Length > quadID - 1 && idTree >= 0)
                {
                    att = patch.quadObjects[quadID - 1].findObjectById(idTree);
                }

                if (att != null && att.noNull)
                {
                    patch.quadObjects[quadID - 1].deleteTreeCheckCrossFade(att);
                    patch.quadObjects[quadID - 1].removeTree(att, true);

                    if (!isPlaying || saveInPlayMode)
                    {
                        List<AltTreesTrees> del = new List<AltTreesTrees>();
                        del.Add(att);
                        patch.EditDataFileObjects(-1, del);

                        if (patch.treesNoGroupCount == patch.treesNoGroupEmptyCount)
                        {
                            if (patch.treesNoGroupData != null)
                            {
                                patch.treesNoGroupCount = 0;
                                patch.treesNoGroupEmptyCount = 0;

                                #if UNITY_EDITOR
                                    AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(patch.treesNoGroupData));

                                    EditorUtility.SetDirty(altTreesManagerData);
                                    AssetDatabase.SaveAssets();
                                    AssetDatabase.Refresh();
                                #endif
                            }
                        }
                    }
                }
            }

            needUpdateScene = true;
        }

        public void removeTrees(AltCollisionInfo aci, bool saveInPlayMode)
        {
            removeTrees(aci.treeInfo.isObject, aci.treeInfo.patch, aci.treeInfo.quadID, aci.treeInfo.idTree, saveInPlayMode);
        }

        public void removeTrees(AltTreesInfo[] atiList, bool saveInPlayMode)
        {
            if (!isInitialized)
            {
                LogError("AltTrees not Initialized!");
                return;
            }
            if (atiList == null)
            {
                LogError("atiList == null");
                return;
            }

            List<AltTreesQuad> quadsForUpdate = new List<AltTreesQuad>();

            for (int i = 0; i < atiList.Length; i++)
                removeTrees(atiList[i].isObject, atiList[i].patch, atiList[i].quadID, atiList[i].idTree, saveInPlayMode, true, quadsForUpdate);
            for (int i = 0; i < quadsForUpdate.Count; i++)
                quadsForUpdate[i].goUpdateTrees();

            needUpdateScene = true;
        }

        public void removeTrees(List<AltTreesInfo> atiList, bool saveInPlayMode)
        {
            if (!isInitialized)
            {
                LogError("AltTrees not Initialized!");
                return;
            }
            if (atiList == null)
            {
                LogError("atiList == null");
                return;
            }

            List<AltTreesQuad> quadsForUpdate = new List<AltTreesQuad>();

            for (int i = 0; i < atiList.Count; i++)
                removeTrees(atiList[i].isObject, atiList[i].patch, atiList[i].quadID, atiList[i].idTree, saveInPlayMode, true, quadsForUpdate);
            for (int i = 0; i < quadsForUpdate.Count; i++)
                quadsForUpdate[i].goUpdateTrees();

            needUpdateScene = true;
        }

        public void getTrees(Vector3 pos, float radius, out List<AltTreesInfo> _atiList, bool trees = true, bool objects = true, int idPrototype = -1)
        {
            _atiList = new List<AltTreesInfo>();
            if (!isInitialized)
            {
                LogError("AltTrees not Initialized!");
                return;
            }

            AltTreesPatch[] listPatches = getPatches(pos + altTreesManager.jump * altTreesManagerData.sizePatch - altTreesManager.jumpPos, radius);
            List<AltTreesTrees> attTemp = new List<AltTreesTrees>();
            for (int i = 0; i < listPatches.Length; i++)
            {
                listPatches[i].getTrees(new Vector2(pos.x, pos.z), radius, trees, objects, idPrototype, attTemp);
            }

            AltTreesInfo atiT = null;

            for (int i = 0; i < attTemp.Count; i++)
            {
                atiT = new AltTreesInfo(attTemp[i].altTreesPatch, attTemp[i].idTree, attTemp[i].isObject, (attTemp[i].isObject ? attTemp[i].idQuadObject : attTemp[i].altTreesId), attTemp[i].idPrototype, attTemp[i].getPosWorld(), attTemp[i].color, attTemp[i].colorBark, attTemp[i].rotation, attTemp[i].heightScale, attTemp[i].widthScale);
                _atiList.Add(atiT);
            }
        }

        public void getAllTrees(out List<AltTreesInfo> _atiList, bool trees = true, bool objects = true, int idPrototype = -1)
        {
            _atiList = new List<AltTreesInfo>();
            if (!isInitialized)
            {
                LogError("AltTrees not Initialized!");
                return;
            }
            
            List<AltTreesTrees> attTemp = new List<AltTreesTrees>();
            for (int i = 0; i < altTreesManagerData.patches.Length; i++)
            {
                altTreesManagerData.patches[i].getAllTrees(trees, objects, idPrototype, attTemp);
            }

            AltTreesInfo atiT = null;

            for (int i = 0; i < attTemp.Count; i++)
            {
                atiT = new AltTreesInfo(attTemp[i].altTreesPatch, attTemp[i].idTree, attTemp[i].isObject, (attTemp[i].isObject ? attTemp[i].idQuadObject : attTemp[i].altTreesId), attTemp[i].idPrototype, attTemp[i].getPosWorld(), attTemp[i].color, attTemp[i].colorBark, attTemp[i].rotation, attTemp[i].heightScale, attTemp[i].widthScale);
                _atiList.Add(atiT);
            }
        }


        public void addTrees(AddTreesStruct[] trees, bool saveInPlayMode)
        {
            if (!isInitialized)
            {
                LogError("AltTrees not Initialized!");
                return;
            }

            if (!isPlaying || saveInPlayMode)
            {
                AltTreesDataLinks dataLinks = getDataLinks();

                if (dataLinksCorrupted)
                {
                    LogError("dataLinks Corrupted!");
                    return;
                }


                Dictionary<AltTreesPatch, List<AddTreesStruct>> tempListPatches = new Dictionary<AltTreesPatch, List<AddTreesStruct>>();
                AltTree _at = null;
                AltTreesPatch tempPatch = null;

                for (int i = 0; i < trees.Length; i++)
                {
                    _at = dataLinks.getAltTree(trees[i].idPrototype);

                    if (_at != null)
                    {
                        tempPatch = getPatchOrAdd(trees[i].pos, true);

                        if (tempListPatches.ContainsKey(tempPatch))
                        {
                            tempListPatches[tempPatch].Add(trees[i]);
                        }
                        else
                        {
                            tempPatch.checkTreePrototype(_at.id, _at, true, false);
                            tempListPatches.Add(tempPatch, new List<AddTreesStruct>());
                            tempListPatches[tempPatch].Add(trees[i]);
                        }
                    }
                }

                foreach (AltTreesPatch key in tempListPatches.Keys)
                {
                    key.addTreesImport(tempListPatches[key].ToArray(), true);
                }
            }
            else
            {
                Dictionary<AltTreesPatch, List<AddTreesStruct>> tempListPatches = new Dictionary<AltTreesPatch, List<AddTreesStruct>>();
                AltTree _at = null;
                AltTreesPatch tempPatch = null;

                for (int i = 0; i < trees.Length; i++)
                {
                    tempPatch = getPatchOrAdd(trees[i].pos, false);

                    _at = null;
                    
                    for (int p = 0; p < altTreesManagerData.patches.Length; p++)
                    {
                        for (int h = 0; h < altTreesManagerData.patches[p].prototypes.Length; h++)
                        {
                            if (altTreesManagerData.patches[p].prototypes[h].tree.id == trees[i].idPrototype)
                            {
                                _at = altTreesManagerData.patches[p].prototypes[h].tree;
                                p = altTreesManagerData.patches.Length;
                                break;
                            }
                        }
                    }

                    //Log(tempPatch.prototypes.Length + ", " + tempPatch.stepX + " " + tempPatch.stepY);


                    if (_at != null)
                    {

                        if (tempListPatches.ContainsKey(tempPatch))
                        {
                            tempListPatches[tempPatch].Add(trees[i]);
                        }
                        else
                        {
                            tempPatch.checkTreePrototype(_at.id, _at, true, false);
                            tempListPatches.Add(tempPatch, new List<AddTreesStruct>());
                            tempListPatches[tempPatch].Add(trees[i]);
                        }
                    }
                    else
                        LogError("Prototype " + trees[i].idPrototype + " not finded!");
                }

                foreach (AltTreesPatch key in tempListPatches.Keys)
                {
                    key.addTreesImport(tempListPatches[key].ToArray(), false);
                }
            }
            needUpdateScene = true;
        }

        Vector3 scaleTemp = new Vector3();

        void editTree(AltTreesInfo treeInfo, bool saveInPlayMode, List<AltTreesPatch> patchesList = null, bool saveData = true, bool udpadeTreesOnScene = true)
        {
            if (!isInitialized)
            {
                LogError("AltTrees not Initialized!");
                return;
            }

            AltTreesTrees attTemp = null;
            if (!treeInfo.isObject)
            {
                if (treeInfo.patch.trees.Length > treeInfo.idTree && treeInfo.idTree >= 0)
                {
                    attTemp = treeInfo.patch.trees[treeInfo.idTree];
                }
                else
                {
                    LogError("patch.trees.Length <= idTree || idTree < 0");
                    return;
                }
            }
            else
            {
                if (treeInfo.patch.quadObjects.Length > treeInfo.quadID - 1 && treeInfo.idTree >= 0)
                {
                    attTemp = treeInfo.patch.quadObjects[treeInfo.quadID - 1].findObjectById(treeInfo.idTree);
                }
                else
                {
                    LogError("patch.quadObjects.Length <= quadID - 1 || idTree < 0");
                    return;
                }
            }

            if (attTemp != null && attTemp.noNull)
            {
                attTemp.pos = treeInfo.patch.getTreePosLocal(treeInfo.pos, altTreesManager.jump, altTreesManager.jumpPos, altTreesManagerData.sizePatch);
                attTemp.color = treeInfo.color;
                attTemp.colorBark = treeInfo.colorBark;
                attTemp.rotation = treeInfo.rotation;
                attTemp.heightScale = treeInfo.heightScale;
                attTemp.widthScale = treeInfo.widthScale;

                if (attTemp.isBillboard)
                {
                    attTemp.posWorldBillboard = attTemp.getPosWorld() + new Vector3(0f, altTreesManager.treesPoolArray[attTemp.idPrototypeIndex].tree.size * attTemp.heightScale / 2f /*- (manager.treesPoolArray[treeTemp.idPrototypeIndex].tree.size * treeTemp.heightScale - manager.treesPoolArray[treeTemp.idPrototypeIndex].tree.size * treeTemp.heightScale / manager.treesPoolArray[treeTemp.idPrototypeIndex].tree.space) / 2f*/ + altTreesManager.treesPoolArray[attTemp.idPrototypeIndex].tree.up * attTemp.heightScale, 0f);
                    attTemp.bound.center = attTemp.posWorldBillboard;
                    attTemp.bound.size = Vector3.one * 5f * altTreesManager.treesPoolArray[attTemp.idPrototypeIndex].treeSize;

                    attTemp.matrixBillboard = Matrix4x4.TRS(attTemp.posWorldBillboard, Quaternion.identity, Vector3.one);


                    attTemp.widthPropBlock = altTreesManager.treesPoolArray[attTemp.idPrototypeIndex].tree.size * attTemp.widthScale / 2f;
                    attTemp.heightPropBlock = altTreesManager.treesPoolArray[attTemp.idPrototypeIndex].tree.size * attTemp.heightScale / 2f;
                    attTemp.huePropBlock = (!altTreesManagerData.drawDebugBillboards) ? attTemp.color : attTemp.colorDebug;

                    if (!altTreesManager.gpuInstancingSupport)
                    {
                        attTemp.propBlockBillboards.Clear();
                        attTemp.propBlockBillboards.SetFloat(AltTreesManager.Alpha_PropertyID, attTemp.alphaPropBlockBillboard);
                        attTemp.propBlockBillboards.SetFloat(AltTreesManager.Width_PropertyID, attTemp.widthPropBlock);
                        attTemp.propBlockBillboards.SetFloat(AltTreesManager.Height_PropertyID, attTemp.heightPropBlock);
                        attTemp.propBlockBillboards.SetFloat(AltTreesManager.Rotation_PropertyID, attTemp.rotation);
                        attTemp.propBlockBillboards.SetColor(AltTreesManager.HueVariation_PropertyID, attTemp.huePropBlock);
                    }
                }
                if (attTemp.isMesh)
                {

                    scaleTemp.x = attTemp.widthScale;
                    scaleTemp.y = attTemp.heightScale;
                    scaleTemp.z = attTemp.widthScale;
                    attTemp.posWorldMesh = attTemp.getPosWorld();
                    attTemp.posWorldBillboard = attTemp.posWorldMesh + new Vector3(0f, altTreesManager.treesPoolArray[attTemp.idPrototypeIndex].tree.size * attTemp.heightScale / 2f /*- (manager.treesPoolArray[treeTemp.idPrototypeIndex].tree.size * treeTemp.heightScale - manager.treesPoolArray[treeTemp.idPrototypeIndex].tree.size * treeTemp.heightScale / manager.treesPoolArray[treeTemp.idPrototypeIndex].tree.space) / 2f*/ + altTreesManager.treesPoolArray[attTemp.idPrototypeIndex].tree.up * attTemp.heightScale, 0f);
                    attTemp.matrixMesh = Matrix4x4.TRS(attTemp.posWorldMesh, Quaternion.AngleAxis(attTemp.rotation, Vector3.up), scaleTemp);
                    attTemp.bound.center = attTemp.posWorldBillboard;
                    attTemp.bound.size = Vector3.one * 5f * altTreesManager.treesPoolArray[attTemp.idPrototypeIndex].treeSize;

                    if (!altTreesManager.gpuInstancingSupport || !attTemp.gpuInstancing)
                    {
                        attTemp.propBlockMesh.Clear();
                        attTemp.propBlockMesh.SetColor(AltTreesManager.HueVariationLeave_PropertyID, attTemp.color);
                        attTemp.propBlockMesh.SetColor(AltTreesManager.HueVariationBark_PropertyID, attTemp.colorBark);
                    }
                }

                if (!treeInfo.isObject && udpadeTreesOnScene)
                    altTreesManager.quads[treeInfo.quadID].editTreeSetUpdate(attTemp);

                if (saveData)
                {
                    if (!treeInfo.isObject && udpadeTreesOnScene)
                        altTreesManager.quads[treeInfo.quadID].goUpdateTrees();

                    if (!isPlaying || saveInPlayMode)
                    {
                        if (!treeInfo.isObject)
                            treeInfo.patch.EditDataFileTrees(null, 0, null, attTemp.idTree);
                        else
                            treeInfo.patch.EditDataFileObjects(-1, null, null, attTemp);
                    }
                }
                else
                {
                    if(!patchesList.Contains(treeInfo.patch))
                    {
                        patchesList.Add(treeInfo.patch);
                        treeInfo.patch.editTreesTempListTrees = new List<AltTreesTrees>();
                        treeInfo.patch.editTreesTempListObjects = new List<AltTreesTrees>();
                    }


                    if (treeInfo.isObject)
                        treeInfo.patch.editTreesTempListObjects.Add(attTemp);
                    else
                        treeInfo.patch.editTreesTempListTrees.Add(attTemp);
                }
            }
            else
            {
                LogError("attTemp == null || !attTemp.noNull");
                return;
            }

            needUpdateScene = true;
        }

        public void editTrees(AltTreesInfo treeInfo, bool saveInPlayMode, bool udpadeTreesOnScene = true)
        {
            editTree(treeInfo, saveInPlayMode);
        }

        public void editTrees(AltTreesInfo[] treesInfo, bool saveInPlayMode, bool displayProgressBar = false, bool udpadeTreesOnScene = true)
        {
            List<int> quadIds = new List<int>();
            
            List<AltTreesPatch> patchesList = new List<AltTreesPatch>();

            #if UNITY_EDITOR
                float schProgress = 0;
            #endif
            float treesCount = treesInfo.Length;
            for (int i = 0; i < treesCount; i++)
            {
                #if UNITY_EDITOR
                {
                    if (displayProgressBar)
                    {
                        schProgress++;
                        if (schProgress > treesCount / 20f)
                        {
                            EditorUtility.DisplayProgressBar("Wait please...  " + Mathf.FloorToInt((i / treesCount) * 100f) + "%", "Wait please... ", i / treesCount);

                            schProgress = 0;
                        }
                    }
                }
                #endif
                
                editTree(treesInfo[i], saveInPlayMode, patchesList, false, udpadeTreesOnScene);
                if (udpadeTreesOnScene)
                {
                    if (!treesInfo[i].isObject && !quadIds.Contains(treesInfo[i].quadID))
                        quadIds.Add(treesInfo[i].quadID);
                }
            }

            if (udpadeTreesOnScene)
            {
                for (int i = 0; i < quadIds.Count; i++)
                {
                    altTreesManager.quads[quadIds[i]].goUpdateTrees();
                }
            }

            for (int i = 0; i < patchesList.Count; i++)
            {
                if (!isPlaying || saveInPlayMode)
                {
                    if (patchesList[i].editTreesTempListTrees.Count > 0)
                        patchesList[i].EditDataFileTrees(null, 0, null, -1, patchesList[i].editTreesTempListTrees);
                    if (patchesList[i].editTreesTempListObjects.Count > 0)
                        patchesList[i].EditDataFileObjects(-1, null, null, null, patchesList[i].editTreesTempListObjects);
                }
                patchesList[i].editTreesTempListTrees.Clear();
                patchesList[i].editTreesTempListObjects.Clear();
                patchesList[i].editTreesTempListTrees = null;
                patchesList[i].editTreesTempListObjects = null;
            }
        }

        public void editTrees(List<AltTreesInfo> treesInfo, bool saveInPlayMode, bool displayProgressBar = false, bool udpadeTreesOnScene = true)
        {
            List<int> quadIds = new List<int>();
            
            List<AltTreesPatch> patchesList = new List<AltTreesPatch>();

            #if UNITY_EDITOR
                float schProgress = 0;
            #endif
            float treesCount = treesInfo.Count;
            for (int i = 0; i < treesCount; i++)
            {
                #if UNITY_EDITOR
                {
                    if (displayProgressBar)
                    {
                        schProgress++;
                        if (schProgress > treesCount / 20f)
                        {
                            EditorUtility.DisplayProgressBar("Wait please...  " + Mathf.FloorToInt((i / treesCount) * 100f) + "%", "Wait please... ", i / treesCount);

                            schProgress = 0;
                        }
                    }
                }
                #endif
                
                editTree(treesInfo[i], saveInPlayMode, patchesList, false, udpadeTreesOnScene);
                if (udpadeTreesOnScene)
                {
                    if (!treesInfo[i].isObject && !quadIds.Contains(treesInfo[i].quadID))
                        quadIds.Add(treesInfo[i].quadID);
                }
            }

            if (udpadeTreesOnScene)
            {
                for (int i = 0; i < quadIds.Count; i++)
                {
                    altTreesManager.quads[quadIds[i]].goUpdateTrees();
                }
            }

            for (int i = 0; i < patchesList.Count; i++)
            {
                if (!isPlaying || saveInPlayMode)
                {
                    if (patchesList[i].editTreesTempListTrees.Count > 0)
                        patchesList[i].EditDataFileTrees(null, 0, null, -1, patchesList[i].editTreesTempListTrees);
                    if (patchesList[i].editTreesTempListObjects.Count > 0)
                        patchesList[i].EditDataFileObjects(-1, null, null, null, patchesList[i].editTreesTempListObjects);
                }
                patchesList[i].editTreesTempListTrees.Clear();
                patchesList[i].editTreesTempListObjects.Clear();
                patchesList[i].editTreesTempListTrees = null;
                patchesList[i].editTreesTempListObjects = null;
            }
        }

        public AltTree[] getPrototypesList()
        {
            if (!isInitialized)
            {
                LogError("AltTrees not Initialized!");
                return null;
            }

            if(altTreesManager.treesPoolArray.Length == 0)
                return null;

            AltTree[] list = new AltTree[altTreesManager.treesPoolArray.Length];
            for(int i = 0; i < altTreesManager.treesPoolArray.Length; i++)
            {
                list[i] = altTreesManager.treesPoolArray[i].tree;
            }

            return list;
        }

        public static AltTree[] getPrototypesListInProject()
        {
            #if UNITY_EDITOR
            {
                if (!Application.isPlaying)
                {
                    AltTreesDataLinks dataLinks = null;

                    if (System.IO.File.Exists("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesDataLinks.asset"))
                        dataLinks = (AltTreesDataLinks)AssetDatabase.LoadAssetAtPath("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesDataLinks.asset", typeof(AltTreesDataLinks));

                    if (dataLinks == null)
                        return null;
                
                    int countTrees = 0;
                    int countTrees2 = 0;

                    if (dataLinks.altTrees != null)
                    {
                        for (int i = 0; i < dataLinks.altTrees.Length; i++)
                        {
                            if (dataLinks.altTrees[i] != null)
                                countTrees++;
                        }
                    }
                    else
                        return null;

                    AltTree[] list = new AltTree[countTrees];
                    for (int i = 0; i < countTrees; i++)
                    {
                        if (dataLinks.altTrees[i] != null)
                        {
                            list[countTrees2] = dataLinks.altTrees[i];
                            countTrees2++;
                        }
                    }

                    return list;
                }
                else
                    return null;
            }
            #else
                return null;
            #endif
        }

        public AltTree getPrototype(int id)
        {
            if (!isInitialized)
            {
                LogError("AltTrees not Initialized!");
                return null;
            }

            if (altTreesManager.treesPoolArray.Length == 0)
                return null;
            
            for (int i = 0; i < altTreesManager.treesPoolArray.Length; i++)
            {
                if (altTreesManager.treesPoolArray[i].tree.id == id)
                    return altTreesManager.treesPoolArray[i].tree;
            }

            return null;
        }

        public static AltTree getPrototypeInProject(int id)
        {
            #if UNITY_EDITOR
            {
                if (!Application.isPlaying)
                {
                    AltTreesDataLinks dataLinks = null;

                    if (System.IO.File.Exists("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesDataLinks.asset"))
                        dataLinks = (AltTreesDataLinks)AssetDatabase.LoadAssetAtPath("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesDataLinks.asset", typeof(AltTreesDataLinks));

                    if (dataLinks == null)
                        return null;
                
                    if (dataLinks.altTrees != null)
                    {
                        for (int i = 0; i < dataLinks.altTrees.Length; i++)
                        {
                            if (dataLinks.altTrees[i] != null && dataLinks.altTrees[i].id == id)
                                return dataLinks.altTrees[i];
                        }
                    }
                    
                    return null;
                }
                else
                    return null;
            }
            #else
                return null;
            #endif
        }

        public void addFrustumCullingCamera(Camera camera)
        {
            /*if (!isInitialized)
            {
                LogError("AltTrees not Initialized!");
                return;
            }*/

            if(cameraModeFrustum != 2)
            {
                LogError("Set \"Frustum Culling Camera\" to \"Via Script\"");
                return;
            }

            altTreesManager.addFrustumCullingCamera(camera);
        }

        public void removeFrustumCullingCamera(Camera camera)
        {
            /*if (!isInitialized)
            {
                LogError("AltTrees not Initialized!");
                return;
            }*/

            if (cameraModeFrustum != 2)
            {
                LogError("Set \"Frustum Culling Camera\" to \"Via Script\"");
                return;
            }

            altTreesManager.removeFrustumCullingCamera(camera);
        }

        public Camera[] getFrustumCullingCamerasList()
        {
            /*if (!isInitialized)
            {
                LogError("AltTrees not Initialized!");
                return null;
            }*/

            return altTreesManager.getFrustumCullingCamerasList();
        }

        public void addDistanceCamera(Transform camera)
        {
            /*if (!isInitialized)
            {
                LogError("AltTrees not Initialized!");
                return;
            }*/

            if (cameraModeDistance != 2)
            {
                LogError("Set \"Distance Camera\" to \"Via Script\"");
                return;
            }

            altTreesManager.addDistanceCamera(camera);
        }

        public void removeDistanceCamera(Transform camera)
        {
            /*if (!isInitialized)
            {
                LogError("AltTrees not Initialized!");
                return;
            }*/

            if (cameraModeDistance != 2)
            {
                LogError("Set \"Distance Camera\" to \"Via Script\"");
                return;
            }

            altTreesManager.removeDistanceCamera(camera);
        }

        public Transform[] getDistanceCamerasList()
        {
            /*if (!isInitialized)
            {
                LogError("AltTrees not Initialized!");
                return null;
            }*/

            return altTreesManager.getDistanceCamerasList();
        }



        public static Dictionary<string, string> getConfigShaders()
        {
            #if UNITY_EDITOR
            {
                Dictionary<string, string> dict = new Dictionary<string, string>();
                try
                {
                    if (System.IO.File.Exists("Assets/Plugins/AltSystems/AltTrees/Shaders/Library/Configurator/config.AltConf"))
                    {
                        using (System.IO.StreamReader sr = new System.IO.StreamReader("Assets/Plugins/AltSystems/AltTrees/Shaders/Library/Configurator/config.AltConf"))
                        {
                            string[] strs = sr.ReadToEnd().Split(';');
                            for(int i = 0; i < strs.Length; i++)
                            {
                                if(strs[i].Length > 0)
                                {
                                    string[] str = strs[i].Split(':');
                                    if (!dict.ContainsKey(str[0]))
                                        dict.Add(str[0], str[1]);
                                }
                            }
                        }
                    }
                    else
                    {
                        using (System.IO.StreamWriter sw = new System.IO.StreamWriter("Assets/Plugins/AltSystems/AltTrees/Shaders/Library/Configurator/config.AltConf", false, System.Text.Encoding.UTF8))
                        {
                            sw.Write("hue:high");
                        }
                        dict.Add("hue", "high");


                        string str = "";
                        using (System.IO.StreamReader sr = new System.IO.StreamReader("Assets/Plugins/AltSystems/AltTrees/Shaders/Library/Configurator/HueHigh.cgincTemp"))
                        {
                            str = sr.ReadToEnd();
                        }
                        using (System.IO.StreamWriter sw = new System.IO.StreamWriter("Assets/Plugins/AltSystems/AltTrees/Shaders/Library/Configurator/Hue.cginc", false, System.Text.Encoding.UTF8))
                        {
                            sw.Write(str);
                        }

                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                    }

                    return dict;
                }
                catch (System.Exception e)
                {
                    Debug.LogError(e.Message);
                    return null;
                }
            }
            #else
                return null;
            #endif

        }
    }



    public class AltTreesInfo
    {
        public AltTreesPatch patch;
        public int idTree;
        public bool isObject = true;
        public int quadID;
        public int idPrototype;
        
        public Vector3 pos = new Vector3();
        public Color color = new Color();
        public Color colorBark = new Color();
        public float rotation;
        public float heightScale;
        public float widthScale;

        public AltTreesInfo(AltTreesPatch _patch, int _idTree, bool _isObject, int _quadID, int _idPrototype, Vector3 _pos, Color _color, Color _colorBark, float _rotation, float _heightScale, float _widthScale)
        {
            patch = _patch;
            idTree = _idTree;
            isObject = _isObject;
            quadID = _quadID;
            idPrototype = _idPrototype;

            pos = _pos;
            color = _color;
            colorBark = _colorBark;
            rotation = _rotation;
            heightScale = _heightScale;
            widthScale = _widthScale;
        }
    }
}