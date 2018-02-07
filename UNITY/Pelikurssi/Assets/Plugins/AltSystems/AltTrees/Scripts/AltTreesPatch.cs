using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Threading;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AltSystems.AltTrees
{
    [System.Serializable]
    public class AltTreesPatch
    {
        public AltTreePrototypes[] prototypes = new AltTreePrototypes[0];
        [HideInInspector]
        [System.NonSerialized]
        public AltTreesTrees[] trees = new AltTreesTrees[0];
        [HideInInspector]
        //[System.NonSerialized]
        //public AltTreesTrees[] treesNoGroup = new AltTreesTrees[0];

        [System.NonSerialized]
        public int treesCount = 0;
        [System.NonSerialized]
        public int treesEmptyCount = 0;
        [System.NonSerialized]
        public int[] treesEmpty;
        [System.NonSerialized]
        public int treesNoGroupCount = 0;
        [System.NonSerialized]
        public int treesNoGroupEmptyCount = 0;
        //[System.NonSerialized]
        //public int[] treesNoGroupEmpty;

        #if UNITY_EDITOR
            AltTreesDataLinks dataLinks = null;
        #endif
        
        [System.NonSerialized]
        public int objectsQuadIdTemp = 1;
        public int updateDataQuadsTempId = 0;
        public List<UpdateDataQuadsTemp> updateDataQuadsTempList;
        public bool checkTreesAddStop = false;
        
        /*public float sizeQuad = 0f;
        public Vector2 minPosXZ = new Vector2(2000, 2000);
        public Vector2 maxPosXZ = new Vector2(0, 0);*/

        public TextAsset treesData;
        public TextAsset treesNoGroupData;
    
        public int maxLOD = 5;
	    public int startBillboardsLOD = 3;
        public bool draw = true;
        public bool drawDebugPutches = false;
	    public bool drawDebugPutchesStar = false;
	    public GameObject cube;
        public AltTreesManager altTreesManager = null;
        public AltTreesManagerData altTreesManagerData;
        public AltTrees altTrees = null;

        public Vector3 step = new Vector3(0,0,0);
        public int stepX = 0;
        public int stepY = 0;

        [System.NonSerialized]
        public int brushSize = 5;
        [System.NonSerialized]
        public int treeCount = 5;
        [System.NonSerialized]
        public int idPlacingPrototype = 0;
        [System.NonSerialized]
        public int speedPlace = 2;
        [System.NonSerialized]
        public bool randomRotation = true;

        
        public int altTreesId = -1;
        [System.NonSerialized]
        public List<GameObject> rendersDebug = new List<GameObject>();

        [System.NonSerialized]
        public List<AltTreesTrees> tempTrees;
        
        #if UNITY_EDITOR
            string pathStr1 = "";
            string pathStr2 = "";
        #endif

        
        [System.NonSerialized]
        public List<AltTreesTrees> editTreesTempListTrees = null;
        [System.NonSerialized]
        public List<AltTreesTrees> editTreesTempListObjects = null;

        public AltTreesPatch(int _stepX, int _stepY)
        {
            if(tempTrees == null)
                tempTrees = new List<AltTreesTrees>();
            step.x = _stepX;
            step.z = _stepY;
            stepX = _stepX;
            stepY = _stepY;
        }

        public AltTreesPatch()
        {
            if (tempTrees == null)
                tempTrees = new List<AltTreesTrees>();
        }

        [System.NonSerialized]
        public bool isInitStarted = false;
        [System.NonSerialized]
        public bool isLoadingData = false;
        [System.NonSerialized]
        public bool isLoadingDataOk = false;
        [System.NonSerialized]
        public bool isInit = false;
        [System.NonSerialized]
        public int threadsCount = 0;
        [System.NonSerialized]
        public int threadsCountNeed = 0;

        [System.NonSerialized]
        byte[] bytesTemp;
        [System.NonSerialized]
        public byte[] bytesTemp3;


        [System.NonSerialized]
        public AltTreesQuad[] quadObjects = null;

        void loadDataThread(object obj)
        {
            try
            {
                #if UNITY_EDITOR
                    if(treesData != null)
                    {
                        if(pathStr1 != "")
                            bytesTemp = File.ReadAllBytes(pathStr1);
                        else
                            bytesTemp = treesData.bytes;
                    }
                    if(treesNoGroupData != null)
                    {
                        if(pathStr2 != "")
                            bytesTemp3 = File.ReadAllBytes(pathStr2);
                        else
                            bytesTemp3 = treesNoGroupData.bytes;
                    }
                #endif
                isLoadingDataOk = true;
                isLoadingData = false;
            }
            catch (System.Exception e)
            {
                Debug.LogError(e);
            }
        }


        public void disable()
        {
            isInit = false;
            isInitStarted = false;
            isLoadingData = false;
            isLoadingDataOk = false;
            threadsCount = 0;
            threadsCountNeed = 0;
            bytesTemp = null;
            bytesTemp3 = null;
        }
        

        public void initObjects(AltTreesQuad atq)
        {
            if (!atq.isInitObjects)
            {
                if (!atq.initObjectsStarted)
                {
                    atq.initObjectsStarted = true;
                    if (altTrees.altTreesManagerData.multiThreading && altTrees.isPlaying)
                        ThreadPool.QueueUserWorkItem(initObjectsLoadDataThread, atq);
                    else
                        initObjectsLoadDataThread(atq);
                }
            }
        }

        void initObjectsLoadDataThread(object obj)
        {
            #if UNITY_EDITOR
            try
            #endif
            {
                AltTreesQuad objectQuadId = obj as AltTreesQuad;

                if(bytesTemp3 != null && bytesTemp3.Length > 0)
                {
                    int version = AltUtilities.ReadBytesInt(bytesTemp3, 0);

                    Vector3 _pos = new Vector3();
                    int _idPrototype;
                    Color _color = new Color();
                    Color _colorBark = new Color();
                    float _rotation;
                    float _heightScale;
                    float _widthScale;
                    Vector3 vector3Temp = new Vector3();

                    List<int> idPrototypes = new List<int>();

                    if (version == 2)
                    {
                        if (AltUtilities.ReadBytesInt(bytesTemp3, 8) == objectsQuadIdTemp - 1)
                        {
                            int addrObjs = AltUtilities.ReadBytesInt(bytesTemp3, 20 + (objectQuadId.objectsQuadId - 1) * 4);
                            int countObjs = 0;
                            int countAll = 0;

                            treesNoGroupCount = AltUtilities.ReadBytesInt(bytesTemp3, 12);
                            treesNoGroupEmptyCount = AltUtilities.ReadBytesInt(bytesTemp3, 16);

                            while (addrObjs != -1)
                            {
                                countObjs = AltUtilities.ReadBytesInt(bytesTemp3, addrObjs + 4);
                                
                                if (countObjs > 0)
                                {
                                    for (int i = 0; i < countObjs; i++)
                                    {
                                        if (AltUtilities.ReadBytesBool(bytesTemp3, addrObjs + 8 + i * 61 + 0))
                                        {
                                            _pos = AltUtilities.ReadBytesVector3(bytesTemp3, addrObjs + 8 + i * 61 + 1);
                                            _idPrototype = AltUtilities.ReadBytesInt(bytesTemp3, addrObjs + 8 + i * 61 + 13);
                                            _color = AltUtilities.ReadBytesColor(bytesTemp3, addrObjs + 8 + i * 61 + 17);
                                            _colorBark = AltUtilities.ReadBytesColor(bytesTemp3, addrObjs + 8 + i * 61 + 33);
                                            _rotation = AltUtilities.ReadBytesFloat(bytesTemp3, addrObjs + 8 + i * 61 + 49);
                                            _heightScale = AltUtilities.ReadBytesFloat(bytesTemp3, addrObjs + 8 + i * 61 + 53);
                                            _widthScale = AltUtilities.ReadBytesFloat(bytesTemp3, addrObjs + 8 + i * 61 + 57);
                                            
                                            AltTreesTrees att = new AltTreesTrees(_pos, countAll + i, _idPrototype, true, _color, _colorBark, _rotation, _heightScale, _widthScale, this);
                                            
                                            
                                            att.idPrototypeIndex = altTreesManager.getPrototypeIndex(_idPrototype);
                                            att.idQuadObject = objectQuadId.objectsQuadId;
                                            att.idQuadObjectNew = -1;

                                            if (att.idPrototypeIndex != -1)
                                            {
                                                att.gpuInstancing = altTreesManager.treesPoolArray[att.idPrototypeIndex].tree.gpuInstancing;
                                                att.currentLOD = -1;
                                                att.currentCrossFadeId = -1;
                                                vector3Temp = att.getPosWorld();
                                                objectQuadId.checkTreesAdd(vector3Temp.x, vector3Temp.z, att, altTreesId, false);
                                            }
                                            else
                                            {
                                                Debug.Log("i = " + i.ToString());
                                                Debug.Log("countObjs = " + countObjs.ToString());
                                                /*Debug.Log("_pos = " + _pos.ToString());
                                                Debug.Log("_idPrototype = " + _idPrototype.ToString());
                                                Debug.Log("_color = " + _color.ToString());
                                                Debug.Log("_colorBark = " + _colorBark.ToString());
                                                Debug.Log("_heightScale = " + _heightScale.ToString());
                                                Debug.Log("_widthScale = " + _widthScale.ToString());*/

                                                if (!idPrototypes.Contains(_idPrototype))
                                                    idPrototypes.Add(_idPrototype);
                                            }
                                        }
                                    }
                                    countAll += countObjs;
                                }
                                addrObjs = AltUtilities.ReadBytesInt(bytesTemp3, addrObjs);
                            }
                        }
                        else
                            altTrees.LogError("Count Quads in file(" + AltUtilities.ReadBytesInt(bytesTemp3, 8) + ") != Count Quads in Manager(" + (objectsQuadIdTemp - 1) + ")");
                    }

                    if(idPrototypes.Count > 0)
                    {
                        for (int i = 0; i < idPrototypes.Count; i++)
                        {
                            altTrees.LogError("No find prototype " + idPrototypes[i]);
                        }
                    }
                }

                objectQuadId.isInitObjects = true;
            }
            #if UNITY_EDITOR
            catch (System.Exception e)
            {
                Debug.LogError(e);
            }
            #endif
        }

        public bool Init(AltTreesManager _altTreesManager, AltTrees _altTrees, AltTreesManagerData _altTreesManagerData, bool save)
        {
            if(!save)
            {
                if (tempTrees == null)
                    tempTrees = new List<AltTreesTrees>();

                altTreesManager = _altTreesManager;
                altTrees = _altTrees;
                altTreesManagerData = _altTreesManagerData;
                isInitStarted = false;
                threadsCountNeed = 0;
                threadsCount = 0;
                isInit = true;
                return true;
            }


            if (tempTrees == null)
                tempTrees = new List<AltTreesTrees>();

            if (!isInit)
            {
                if (isInitStarted)
                {
                    if (threadsCount != threadsCountNeed)
                    {
                        return false;
                    }
                }
                if(isLoadingData && !isLoadingDataOk)
                    return false;
            }
            else
                return true;

            if(!isInitStarted)
            {
                altTreesManager = _altTreesManager;
                altTrees = _altTrees;
                altTreesManagerData = _altTreesManagerData;

                if (treesData == null)
                {
                    #if UNITY_EDITOR
                        treesData = (TextAsset)AssetDatabase.LoadAssetAtPath("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + altTrees.getIdManager() + "/treesData_" + stepX + "_" + stepY + ".bytes", typeof(TextAsset));
                        EditorUtility.SetDirty(altTrees.altTreesManagerData);
                    #endif
                    if (treesData == null)
                    {
                        treesCount = 0;
                        treesEmptyCount = 0;
                
                        trees = null;
                        trees = new AltTreesTrees[0];
                        treesEmpty = new int[0];
                    }
                }
                if (treesNoGroupData == null)
                {
                    #if UNITY_EDITOR
                        treesNoGroupData = (TextAsset)AssetDatabase.LoadAssetAtPath("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + altTrees.getIdManager() + "/treesData_" + stepX + "_" + stepY + "_objs.bytes", typeof(TextAsset));
                        EditorUtility.SetDirty(altTrees.altTreesManagerData);
                    #endif
                    if (treesNoGroupData == null)
                    {
                        treesNoGroupCount = 0;
                        treesNoGroupEmptyCount = 0;

                        //treesNoGroup = null;
                        //treesNoGroup = new AltTreesTrees[0];
                        //treesNoGroupEmpty = new int[0];
                    }
                }
            }

            
            if(!isInitStarted && !isLoadingDataOk)
            {
                if (altTrees.altTreesManagerData.multiThreading && Application.isPlaying)
                {
                    isLoadingData = true;
                    isLoadingDataOk = false;

                    #if UNITY_EDITOR
                        if(treesData != null)
                            pathStr1 = AssetDatabase.GetAssetPath(treesData);
                        if (treesNoGroupData != null)
                            pathStr2 = AssetDatabase.GetAssetPath(treesNoGroupData);
                    #endif

                    #if !UNITY_EDITOR
                        if(treesData != null)
                            bytesTemp = treesData.bytes;
                        if (treesNoGroupData != null)
                            bytesTemp3 = treesNoGroupData.bytes;
                    #endif

                    ThreadPool.QueueUserWorkItem(loadDataThread);

                    return false;
                }
                else
                {
                    #if UNITY_EDITOR
                        if (treesData != null)
                        { 
                            pathStr1 = AssetDatabase.GetAssetPath(treesData);
                            if(pathStr1 != "")
                                bytesTemp = File.ReadAllBytes(pathStr1);
                            else
                                bytesTemp = treesData.bytes;
                        }
                        if (treesNoGroupData != null)
                        {
                            pathStr2 = AssetDatabase.GetAssetPath(treesNoGroupData);
                            if(pathStr2 != "")
                                bytesTemp3 = File.ReadAllBytes(pathStr2);
                            else
                                bytesTemp3 = treesNoGroupData.bytes;
                        }
                    #else
                        if(treesData != null)
                            bytesTemp = treesData.bytes;
                        if (treesNoGroupData != null)
                            bytesTemp3 = treesNoGroupData.bytes;
                    #endif
                    isLoadingDataOk = true;
                }
            }

            if (treesData != null)
            {
                int version = AltUtilities.ReadBytesInt(bytesTemp, 0);

                if (version == 1)
                {
                    if (!isInitStarted)
                    {
                        treesCount = AltUtilities.ReadBytesInt(bytesTemp, 4);
                        treesEmptyCount = AltUtilities.ReadBytesInt(bytesTemp, 8);

                        trees = new AltTreesTrees[treesCount];
                        treesEmpty = new int[treesEmptyCount];
                    }

                    if (!isInitStarted)
                    {
                        if (altTrees.altTreesManagerData.multiThreading && treesCount > 500 && Application.isPlaying)
                        {
                            isInitStarted = true;
                            threadsCountNeed = 10;

                            for (int i = 0; i < 10; i++)
                                ThreadPool.QueueUserWorkItem(readTreesFromFileThread, i);
                            
                            return false;
                        }
                        else
                        {
                            Vector3 _pos = new Vector3();
                            int _idPrototype;
                            Color _color = new Color();
                            Color _colorBark = new Color();
                            float _rotation;
                            float _heightScale;
                            float _widthScale;

                            for (int i = 0; i < treesCount; i++)
                            {
                                _pos = AltUtilities.ReadBytesVector3(bytesTemp, 12 + i * 60 + 0);
                                _idPrototype = AltUtilities.ReadBytesInt(bytesTemp, 12 + i * 60 + 12);
                                _color = AltUtilities.ReadBytesColor(bytesTemp, 12 + i * 60 + 16);
                                _colorBark = AltUtilities.ReadBytesColor(bytesTemp, 12 + i * 60 + 32);
                                _rotation = AltUtilities.ReadBytesFloat(bytesTemp, 12 + i * 60 + 48);
                                _heightScale = AltUtilities.ReadBytesFloat(bytesTemp, 12 + i * 60 + 52);
                                _widthScale = AltUtilities.ReadBytesFloat(bytesTemp, 12 + i * 60 + 56);

                                AltTreesTrees att = new AltTreesTrees(_pos, i, _idPrototype, false, _color, _colorBark, _rotation, _heightScale, _widthScale, this);

                                trees[i] = att;
                            }

                            for (int i = 0; i < treesEmptyCount; i++)
                            {
                                treesEmpty[i] = AltUtilities.ReadBytesInt(bytesTemp, 12 + treesCount * 60 + i * 4);
                                if (treesEmpty[i] < trees.Length)
                                    trees[treesEmpty[i]].noNull = false;
                                else
                                    altTrees.LogError("treesEmpty[i] = " + treesEmpty[i] + ", trees.Length = " + trees.Length);
                            }
                        }
                    }
                    else
                    {
                        for (int i = 0; i < treesEmptyCount; i++)
                        {
                            treesEmpty[i] = AltUtilities.ReadBytesInt(bytesTemp, 12 + treesCount * 60 + i * 4);
                            if (treesEmpty[i] < trees.Length)
                            {
                                trees[treesEmpty[i]].noNull = false;
                            }
                            else
                                altTrees.LogError("treesEmpty[i] = " + treesEmpty[i] + ", trees.Length = " + trees.Length);
                        }
                    }
                }

                bytesTemp = null;
            }
            if (treesNoGroupData != null)
            {
                int version = AltUtilities.ReadBytesInt(bytesTemp3, 0);

                if (version == 1)
                {
                    #if UNITY_EDITOR
                    {
                        EditorUtility.DisplayProgressBar("Converting Data file to new format... ", "Converting Data file to new format... ", 0.0f);

                        if (!System.IO.Directory.Exists("AltSystems/AltTrees/AltTreesDataBackups/" + altTrees.getIdManager()))
                        {
                            System.IO.Directory.CreateDirectory("AltSystems/AltTrees/AltTreesDataBackups/" + altTrees.getIdManager());
                            System.IO.File.Copy("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + altTrees.getIdManager() + "/treesData_" + stepX + "_" + stepY + "_objs.bytes", "AltSystems/AltTrees/AltTreesDataBackups/" + altTrees.getIdManager() + "/treesData_" + stepX + "_" + stepY + "_objs.bytes");
                        }
                        else
                        {
                            if (!System.IO.File.Exists("AltSystems/AltTrees/AltTreesDataBackups/" + altTrees.getIdManager() + "/treesData_" + stepX + "_" + stepY + "_objs.bytes"))
                            {
                                System.IO.File.Copy("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + altTrees.getIdManager() + "/treesData_" + stepX + "_" + stepY + "_objs.bytes", "AltSystems/AltTrees/AltTreesDataBackups/" + altTrees.getIdManager() + "/treesData_" + stepX + "_" + stepY + "_objs.bytes");
                            }
                            else
                            {
                                bool stop = false;
                                int t = 2;
                                while(!stop)
                                {
                                    if (!System.IO.Directory.Exists("AltSystems/AltTrees/AltTreesDataBackups/" + altTrees.getIdManager() + "_" + t))
                                    {
                                        System.IO.Directory.CreateDirectory("AltSystems/AltTrees/AltTreesDataBackups/" + altTrees.getIdManager() + "_" + t);
                                        System.IO.File.Copy("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + altTrees.getIdManager() + "/treesData_" + stepX + "_" + stepY + "_objs.bytes", "AltSystems/AltTrees/AltTreesDataBackups/" + altTrees.getIdManager() + "_" + t + "/treesData_" + stepX + "_" + stepY + "_objs.bytes");
                                        stop = true;
                                    }
                                    else
                                        t++;
                                }
                            }
                        }
                        updateDataQuadsTempId = 1;

                        updateDataQuadsTempList = new List<UpdateDataQuadsTemp>();

                        UpdateDataQuadsTemp qt = new UpdateDataQuadsTemp((float)altTreesManagerData.sizePatch / 2f, (float)altTreesManagerData.sizePatch / 2f, altTreesManagerData.sizePatch, 0, altTreesManagerData.maxLOD, updateDataQuadsTempList, ref updateDataQuadsTempId, 0);
                        UpdateDataQuadsTemp[] qtArray = updateDataQuadsTempList.ToArray();
                        
                        treesNoGroupCount = AltUtilities.ReadBytesInt(bytesTemp3, 4);
                        treesNoGroupEmptyCount = AltUtilities.ReadBytesInt(bytesTemp3, 8);

                        AltTreesTrees[] treesNoGroup = new AltTreesTrees[treesNoGroupCount];
                        int[] treesNoGroupEmpty = new int[treesNoGroupEmptyCount];

                        Vector3 _pos = new Vector3();
                        int _idPrototype;
                        Color _color = new Color();
                        Color _colorBark = new Color();
                        float _rotation;
                        float _heightScale;
                        float _widthScale;

                        Vector2 pos2D;
                        int sch = 0;

                        EditorUtility.DisplayProgressBar("Converting Data file to new format... ", "Converting Data file to new format... ", 0.01f);

                        for (int i = 0; i < treesNoGroupCount; i++)
                        {
                            _pos = AltUtilities.ReadBytesVector3(bytesTemp3, 12 + i * 60 + 0);
                            _idPrototype = AltUtilities.ReadBytesInt(bytesTemp3, 12 + i * 60 + 12);
                            _color = AltUtilities.ReadBytesColor(bytesTemp3, 12 + i * 60 + 16);
                            _colorBark = AltUtilities.ReadBytesColor(bytesTemp3, 12 + i * 60 + 32);
                            _rotation = AltUtilities.ReadBytesFloat(bytesTemp3, 12 + i * 60 + 48);
                            _heightScale = AltUtilities.ReadBytesFloat(bytesTemp3, 12 + i * 60 + 52);
                            _widthScale = AltUtilities.ReadBytesFloat(bytesTemp3, 12 + i * 60 + 56);

                            AltTreesTrees att = new AltTreesTrees(_pos, i, _idPrototype, true, _color, _colorBark, _rotation, _heightScale, _widthScale, this);
                            
                            treesNoGroup[i] = att;


                            if (sch > treesNoGroupCount / 10f)
                            {
                                EditorUtility.DisplayProgressBar("Converting Data file to new format... ", "Converting Data file to new format... ", 0.01f + 0.98f * (float)((float)i / (float)treesNoGroupCount));

                                sch = 0;
                            }
                            sch++;
                        }

                        for (int i = 0; i < treesNoGroupEmptyCount; i++)
                        {
                            treesNoGroupEmpty[i] = AltUtilities.ReadBytesInt(bytesTemp3, 12 + treesNoGroupCount * 60 + i * 4);
                            if (treesNoGroupEmpty[i] < treesNoGroup.Length)
                                treesNoGroup[treesNoGroupEmpty[i]].noNull = false;
                            else
                                altTrees.LogError("treesNoGroupEmpty[i] = " + treesNoGroupEmpty[i] + ", treesNoGroup.Length = " + treesNoGroup.Length);
                        }

                        sch = 0;

                        for (int i = 0; i < treesNoGroupCount; i++)
                        {
                            if (treesNoGroup[i].noNull)
                            {
                                pos2D = treesNoGroup[i].getPosWorld2DWithoutJump();
                                checkTreesAddStop = false;
                                qt.checkTreesAdd(pos2D.x, pos2D.y, treesNoGroup[i], ref checkTreesAddStop);
                                sch++;
                            }
                        }
                        
                        int summBytes = 20 + qtArray.Length * 4;
                        
                        
                        using (BinaryWriter writer = new BinaryWriter(File.Open("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + altTrees.getIdManager() + "/treesData_" + stepX + "_" + stepY + "_objs.bytes", FileMode.Create)))
                        {
                            byte[] bytesTemp2 = new byte[summBytes];
                            AltUtilities.WriteBytes(2, bytesTemp2, 0);
                            AltUtilities.WriteBytes(summBytes, bytesTemp2, 4);
                            AltUtilities.WriteBytes(qtArray.Length, bytesTemp2, 8);
                            AltUtilities.WriteBytes(sch, bytesTemp2, 12);
                            AltUtilities.WriteBytes(0, bytesTemp2, 16);

                            for (int i = 0; i < qtArray.Length; i++)
                            {
                                AltUtilities.WriteBytes(-1, bytesTemp2, 20 + i*4);
                            }
                            
                            writer.Write(bytesTemp2);
                        }
                        
                        #if UNITY_EDITOR
                            AssetDatabase.SaveAssets();
                            AssetDatabase.Refresh();
                            treesNoGroupData = (TextAsset)AssetDatabase.LoadAssetAtPath("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + altTrees.getIdManager() + "/treesData_" + stepX + "_" + stepY + "_objs.bytes", typeof(TextAsset));
                            EditorUtility.SetDirty(altTrees.altTreesManagerData);
                        #endif

                        
                        
                        int startBytes = 0;
                        byte[] bytes4Temp = new byte[4];
                        byte[] bytes61Temp = new byte[61];
                        AltTreesTrees attTemp;


                        if (File.Exists("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + altTrees.getIdManager() + "/treesData_" + stepX + "_" + stepY + "_objs.bytesTemp"))
                            File.Delete("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + altTrees.getIdManager() + "/treesData_" + stepX + "_" + stepY + "_objs.bytesTemp");

                        if (File.Exists("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + altTrees.getIdManager() + "/treesData_" + stepX + "_" + stepY + "_objs.bytesTemp.meta"))
                            File.Delete("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + altTrees.getIdManager() + "/treesData_" + stepX + "_" + stepY + "_objs.bytesTemp.meta");

                        File.Copy("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + altTrees.getIdManager() + "/treesData_" + stepX + "_" + stepY + "_objs.bytes", "Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + altTrees.getIdManager() + "/treesData_" + stepX + "_" + stepY + "_objs.bytesTemp");

                        using (BinaryWriter writer = new BinaryWriter(File.Open("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + altTrees.getIdManager() + "/treesData_" + stepX + "_" + stepY + "_objs.bytesTemp", FileMode.Open)))
                        {
                            for (int i = 0; i < qtArray.Length; i++)
                            {
                                for (int h = 0; h < qtArray[i].objs.Count; h++)
                                {
                                    attTemp = qtArray[i].objs[h];

                                    if (h == 0)
                                    {
                                        AltUtilities.WriteBytes(summBytes, bytes4Temp, 0);
                                        writer.Seek(20 + i * 4, SeekOrigin.Begin);
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
                                }
                                if (qtArray[i].objs.Count != 0)
                                {
                                    writer.Seek(startBytes, SeekOrigin.Begin);
                                    AltUtilities.WriteBytes(qtArray[i].objs.Count, bytes4Temp, 0);
                                    writer.Write(bytes4Temp);
                                }
                            }

                            writer.Seek(4, SeekOrigin.Begin);
                            AltUtilities.WriteBytes(summBytes, bytes4Temp, 0);
                            writer.Write(bytes4Temp);
                        }

                        if (File.Exists("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + altTrees.getIdManager() + "/treesData_" + stepX + "_" + stepY + "_objs.bytes"))
                            File.Replace("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + altTrees.getIdManager() + "/treesData_" + stepX + "_" + stepY + "_objs.bytesTemp", "Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + altTrees.getIdManager() + "/treesData_" + stepX + "_" + stepY + "_objs.bytes", null);
                        else
                            File.Move("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + altTrees.getIdManager() + "/treesData_" + stepX + "_" + stepY + "_objs.bytesTemp", "Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + altTrees.getIdManager() + "/treesData_" + stepX + "_" + stepY + "_objs.bytes");

                        treesNoGroupCount = 0;
                        treesNoGroupEmptyCount = 0;

                        treesNoGroup = null;
                        treesNoGroup = new AltTreesTrees[0];
                        treesNoGroupEmpty = new int[0];

                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();


                        pathStr2 = AssetDatabase.GetAssetPath(treesNoGroupData);
                        if (pathStr2 != "")
                            bytesTemp3 = File.ReadAllBytes(pathStr2);
                        else
                            bytesTemp3 = treesNoGroupData.bytes;

                        EditorUtility.ClearProgressBar();
                    }
                    #endif
                }
                else if(version == 2)
                {
                    treesNoGroupCount = AltUtilities.ReadBytesInt(bytesTemp3, 12);
                    treesNoGroupEmptyCount = AltUtilities.ReadBytesInt(bytesTemp3, 16);
                }
            }

            bytesTemp = null;

            //System.GC.Collect();

            bool isStop = false;
            for (int i = 0; i < prototypes.Length; i++)
            {
                if (prototypes[i].tree != null && prototypes[i].tree.isObject != prototypes[i].isObject)
                {
                    checkTreeOrObject(i);
                    isStop = true;
                }

                #if UNITY_EDITOR
                {
                    if (prototypes[i].tree != null)
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
                        }

                        if (dataLinks.getId(prototypes[i].tree) == -1)
                        {
                            altTrees.LogError("dataLinks.getId(prototypes[i].tree) == -1. (" + prototypes[i].tree.id + ")");

                            string[] filesTemp = Directory.GetFiles("Assets", prototypes[i].tree.name + ".spm", SearchOption.AllDirectories);
                            GameObject goT = null;
                            if (filesTemp.Length > 0 && filesTemp[0].Length > 0)
                                goT = (GameObject)AssetDatabase.LoadAssetAtPath(filesTemp[0], typeof(GameObject));

                            dataLinks.addTree(goT, prototypes[i].tree, prototypes[i].tree.id);



                            AssetDatabase.SaveAssets();
                            AssetDatabase.Refresh();
                            EditorUtility.SetDirty(dataLinks);
                        }
                    }
                }
                #endif
            }

            isInitStarted = false;
            threadsCountNeed = 0;
            threadsCount = 0;

            if (isStop)
            {
                isInit = false;
                disable();

                return Init(_altTreesManager, _altTrees, _altTreesManagerData, save);
            }
            else
            {
                isInit = true;
                return true;
            }
        }

        public void readTreesFromFileThread(object state)
        {
            #if UNITY_EDITOR
            try
            #endif
            {
                int id = (int)state;
                int c = (int)System.Math.Floor((double)treesCount / 10d);
                int t = c * (id + 1);
                if (id == 9)
                    t = treesCount;

                Vector3 _pos = new Vector3();
                int _idPrototype;
                Color _color = new Color();
                Color _colorBark = new Color();
                float _rotation;
                float _heightScale;
                float _widthScale;

                for (int i = c * id; i < t; i++)
                {
                    _pos = AltUtilities.ReadBytesVector3(bytesTemp, 12 + i * 60 + 0);
                    _idPrototype = AltUtilities.ReadBytesInt(bytesTemp, 12 + i * 60 + 12);
                    _color = AltUtilities.ReadBytesColor(bytesTemp, 12 + i * 60 + 16);
                    _colorBark = AltUtilities.ReadBytesColor(bytesTemp, 12 + i * 60 + 32);
                    _rotation = AltUtilities.ReadBytesFloat(bytesTemp, 12 + i * 60 + 48);
                    _heightScale = AltUtilities.ReadBytesFloat(bytesTemp, 12 + i * 60 + 52);
                    _widthScale = AltUtilities.ReadBytesFloat(bytesTemp, 12 + i * 60 + 56);

                    AltTreesTrees att = new AltTreesTrees(_pos, i, _idPrototype, false, _color, _colorBark, _rotation, _heightScale, _widthScale, this);

                    trees[i] = att;
                }
                Interlocked.Increment(ref threadsCount);
            }
            #if UNITY_EDITOR
            catch (System.Exception e)
            {
                Debug.LogError(e);
            }
            #endif
        }

        public void EditDataFileTrees(List<int> changedTrees, int addedTreesCount, List<int> removedTrees, int editingTree = -1, List<AltTreesTrees> editingTrees = null, bool isProgressBar = false, string progressBarTitle = "")
        {
            #if UNITY_EDITOR
            {
                if ((changedTrees != null && changedTrees.Count > 0) || addedTreesCount > 0 || (removedTrees != null && removedTrees.Count > 0) || editingTree!=-1 || (editingTrees != null && editingTrees.Count > 0))
                {
                    byte[] bytes4Temp = new byte[4];
                    byte[] bytes60Temp = new byte[60];
                    if (!File.Exists("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + altTrees.getIdManager() + "/treesData_" + stepX + "_" + stepY + ".bytes"))
                    {
                        using (BinaryWriter writer = new BinaryWriter(File.Open("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + altTrees.getIdManager() + "/treesData_" + stepX + "_" + stepY + ".bytes", FileMode.Create)))
                        {
                            byte[] bytes12Temp = new byte[12];
                            AltUtilities.WriteBytes(1, bytes12Temp, 0);
                            AltUtilities.WriteBytes(0, bytes12Temp, 4);
                            AltUtilities.WriteBytes(0, bytes12Temp, 8);
                            writer.Write(bytes12Temp);
                        }
                        #if UNITY_EDITOR
                            AssetDatabase.SaveAssets();
                            AssetDatabase.Refresh();
                            treesData = (TextAsset)AssetDatabase.LoadAssetAtPath("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + altTrees.getIdManager() + "/treesData_" + stepX + "_" + stepY + ".bytes", typeof(TextAsset));
                            EditorUtility.SetDirty(altTrees.altTreesManagerData);
                        #endif
                    }

                    if (File.Exists("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + altTrees.getIdManager() + "/treesData_" + stepX + "_" + stepY + ".bytesTemp"))
                        File.Delete("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + altTrees.getIdManager() + "/treesData_" + stepX + "_" + stepY + ".bytesTemp");

                    if (File.Exists("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + altTrees.getIdManager() + "/treesData_" + stepX + "_" + stepY + ".bytesTemp.meta"))
                        File.Delete("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + altTrees.getIdManager() + "/treesData_" + stepX + "_" + stepY + ".bytesTemp.meta");

                    File.Copy("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + altTrees.getIdManager() + "/treesData_" + stepX + "_" + stepY + ".bytes", "Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + altTrees.getIdManager() + "/treesData_" + stepX + "_" + stepY + ".bytesTemp");

                    using (BinaryWriter writer = new BinaryWriter(File.Open("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + altTrees.getIdManager() + "/treesData_" + stepX + "_" + stepY + ".bytesTemp", FileMode.Open)))
                    {
                        if(addedTreesCount > 0)
                        {
                            AltUtilities.WriteBytes(treesCount, bytes4Temp, 0);
                            writer.Seek(4, SeekOrigin.Begin);
                            writer.Write(bytes4Temp);
                            writer.Seek(12 + 60 * (treesCount - addedTreesCount), SeekOrigin.Begin);
                            for (int i = treesCount - addedTreesCount; i < treesCount; i++)
                            {
                                AltUtilities.WriteBytes(trees[i].pos, bytes60Temp, 0);
                                AltUtilities.WriteBytes(trees[i].idPrototype, bytes60Temp, 12);
                                AltUtilities.WriteBytes(trees[i].color, bytes60Temp, 16);
                                AltUtilities.WriteBytes(trees[i].colorBark, bytes60Temp, 32);
                                AltUtilities.WriteBytes(trees[i].rotation, bytes60Temp, 48);
                                AltUtilities.WriteBytes(trees[i].heightScale, bytes60Temp, 52);
                                AltUtilities.WriteBytes(trees[i].widthScale, bytes60Temp, 56);
                                writer.Write(bytes60Temp);
                            }

                            writer.Seek(12 + 60 * treesCount, SeekOrigin.Begin);
                            for (int i = 0; i < treesEmptyCount; i++)
                            {
                                AltUtilities.WriteBytes(treesEmpty[i], bytes4Temp, 0);
                                writer.Write(bytes4Temp);
                            }
                        }
                        if(changedTrees != null && changedTrees.Count > 0)
                        {
                            int countPlaced = 0;
                            int percentAdded = 0;

                            #if UNITY_EDITOR
                            {
                                if (isProgressBar)
                                {
                                    EditorUtility.DisplayProgressBar(progressBarTitle, "Saving trees... ", 0.0f);
                                }

                            }
                            #endif

                            AltUtilities.WriteBytes(treesEmptyCount, bytes4Temp, 0);
                            writer.Seek(8, SeekOrigin.Begin);
                            writer.Write(bytes4Temp);
                            writer.Seek(12 + 60 * treesCount, SeekOrigin.Begin);

                            for (int i = 0; i < treesEmptyCount; i++)
                            {
                                AltUtilities.WriteBytes(treesEmpty[i], bytes4Temp, 0);
                                writer.Write(bytes4Temp);
                            }

                            for (int i = 0; i < changedTrees.Count; i++)
                            {
                                writer.Seek(12 + changedTrees[i] * 60, SeekOrigin.Begin);

                                AltUtilities.WriteBytes(trees[changedTrees[i]].pos, bytes60Temp, 0);
                                AltUtilities.WriteBytes(trees[changedTrees[i]].idPrototype, bytes60Temp, 12);
                                AltUtilities.WriteBytes(trees[changedTrees[i]].color, bytes60Temp, 16);
                                AltUtilities.WriteBytes(trees[changedTrees[i]].colorBark, bytes60Temp, 32);
                                AltUtilities.WriteBytes(trees[changedTrees[i]].rotation, bytes60Temp, 48);
                                AltUtilities.WriteBytes(trees[changedTrees[i]].heightScale, bytes60Temp, 52);
                                AltUtilities.WriteBytes(trees[changedTrees[i]].widthScale, bytes60Temp, 56);
                                writer.Write(bytes60Temp);

                                #if UNITY_EDITOR
                                {
                                    if(isProgressBar)
                                    { 
                                        countPlaced++;
                    
                                        if(countPlaced >= (changedTrees.Count / 20f))
                                        {
                                            countPlaced = 0;
                                            percentAdded++;
                                            
                                            EditorUtility.DisplayProgressBar(progressBarTitle, "Saving trees... ", percentAdded * 0.1f);
                                        }
                                    }
                                }
                                #endif
                            }
                            changedTrees.Clear();

                            #if UNITY_EDITOR
                            {
                                if(isProgressBar)
                                {
                                    EditorUtility.ClearProgressBar();
                                }
                            }
                            #endif
                        }
                        if(removedTrees != null && removedTrees.Count > 0)
                        {
                            treesEmptyCount += removedTrees.Count;
                            for(int i = 0; i < treesEmpty.Length; i++)
                            {
                                removedTrees.Add(treesEmpty[i]);
                            }
                            treesEmpty = removedTrees.ToArray();

                            AltUtilities.WriteBytes(treesEmptyCount, bytes4Temp, 0);
                            writer.Seek(8, SeekOrigin.Begin);
                            writer.Write(bytes4Temp);
                            writer.Seek(12 + 60 * treesCount, SeekOrigin.Begin);
                            for (int i = 0; i < treesEmptyCount; i++)
                            {
                                AltUtilities.WriteBytes(treesEmpty[i], bytes4Temp, 0);
                                writer.Write(bytes4Temp);
                            }
                        }
                        if(editingTree != -1)
                        {
                            writer.Seek(12 + editingTree * 60, SeekOrigin.Begin);

                            AltUtilities.WriteBytes(trees[editingTree].pos, bytes60Temp, 0);
                            AltUtilities.WriteBytes(trees[editingTree].idPrototype, bytes60Temp, 12);
                            AltUtilities.WriteBytes(trees[editingTree].color, bytes60Temp, 16);
                            AltUtilities.WriteBytes(trees[editingTree].colorBark, bytes60Temp, 32);
                            AltUtilities.WriteBytes(trees[editingTree].rotation, bytes60Temp, 48);
                            AltUtilities.WriteBytes(trees[editingTree].heightScale, bytes60Temp, 52);
                            AltUtilities.WriteBytes(trees[editingTree].widthScale, bytes60Temp, 56);
                            writer.Write(bytes60Temp);
                        }
                        if(editingTrees != null)
                        {
                            for (int i = 0; i < editingTrees.Count; i++)
                            {
                                writer.Seek(12 + editingTrees[i].idTree * 60, SeekOrigin.Begin);

                                AltUtilities.WriteBytes(trees[editingTrees[i].idTree].pos, bytes60Temp, 0);
                                AltUtilities.WriteBytes(trees[editingTrees[i].idTree].idPrototype, bytes60Temp, 12);
                                AltUtilities.WriteBytes(trees[editingTrees[i].idTree].color, bytes60Temp, 16);
                                AltUtilities.WriteBytes(trees[editingTrees[i].idTree].colorBark, bytes60Temp, 32);
                                AltUtilities.WriteBytes(trees[editingTrees[i].idTree].rotation, bytes60Temp, 48);
                                AltUtilities.WriteBytes(trees[editingTrees[i].idTree].heightScale, bytes60Temp, 52);
                                AltUtilities.WriteBytes(trees[editingTrees[i].idTree].widthScale, bytes60Temp, 56);
                                writer.Write(bytes60Temp);
                            }
                        }
                    }

                    if (File.Exists("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + altTrees.getIdManager() + "/treesData_" + stepX + "_" + stepY + ".bytes"))
                        File.Replace("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + altTrees.getIdManager() + "/treesData_" + stepX + "_" + stepY + ".bytesTemp", "Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + altTrees.getIdManager() + "/treesData_" + stepX + "_" + stepY + ".bytes", null);
                    else
                        File.Move("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + altTrees.getIdManager() + "/treesData_" + stepX + "_" + stepY + ".bytesTemp", "Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + altTrees.getIdManager() + "/treesData_" + stepX + "_" + stepY + ".bytes");

                }
            }
            #endif
        }
        
        public void EditDataFileObjects(int changedAndAddTrees, List<AltTreesTrees> removedTrees = null, Dictionary<int, List<AltTreesTrees>> quadsAddObjects = null, AltTreesTrees editingTree = null, List<AltTreesTrees> editingTrees = null, bool isProgressBar = false, string progressBarTitle = "")
        {
            #if UNITY_EDITOR
            {
                if (changedAndAddTrees > 0 || (removedTrees != null && removedTrees.Count > 0) || editingTree != null || (editingTrees != null && editingTrees.Count > 0))
                {
                    byte[] bytes4Temp = new byte[4];
                    byte[] bytes61Temp = new byte[61];
                    byte[] bytes1Temp = new byte[1];


                    if (!File.Exists("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + altTrees.getIdManager() + "/treesData_" + stepX + "_" + stepY + "_objs.bytes"))
                    {
                        updateDataQuadsTempId = 1;

                        updateDataQuadsTempList = new List<UpdateDataQuadsTemp>();

                        new UpdateDataQuadsTemp((float)altTreesManagerData.sizePatch / 2f, (float)altTreesManagerData.sizePatch / 2f, altTreesManagerData.sizePatch, 0, altTreesManagerData.maxLOD, updateDataQuadsTempList, ref updateDataQuadsTempId, 0);
                        UpdateDataQuadsTemp[] qtArray = updateDataQuadsTempList.ToArray();

                        int summBytes = 20 + qtArray.Length * 4;
                        
                        using (BinaryWriter writer = new BinaryWriter(File.Open("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + altTrees.getIdManager() + "/treesData_" + stepX + "_" + stepY + "_objs.bytes", FileMode.Create)))
                        {
                            byte[] bytesTemp2 = new byte[summBytes];
                            AltUtilities.WriteBytes(2, bytesTemp2, 0);
                            AltUtilities.WriteBytes(summBytes, bytesTemp2, 4);
                            AltUtilities.WriteBytes(qtArray.Length, bytesTemp2, 8);
                            AltUtilities.WriteBytes(0, bytesTemp2, 12);
                            AltUtilities.WriteBytes(0, bytesTemp2, 16);

                            for (int i = 0; i < qtArray.Length; i++)
                            {
                                AltUtilities.WriteBytes(-1, bytesTemp2, 20 + i * 4);
                            }

                            writer.Write(bytesTemp2);
                        }
                        
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                        treesNoGroupData = (TextAsset)AssetDatabase.LoadAssetAtPath("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + altTrees.getIdManager() + "/treesData_" + stepX + "_" + stepY + "_objs.bytes", typeof(TextAsset));
                        EditorUtility.SetDirty(altTrees.altTreesManagerData);

                        if (treesNoGroupData != null)
                        {
                            pathStr2 = AssetDatabase.GetAssetPath(treesNoGroupData);
                            if (pathStr2 != "")
                                bytesTemp3 = File.ReadAllBytes(pathStr2);
                            else
                                bytesTemp3 = treesNoGroupData.bytes;
                        }
                    }
                    if (bytesTemp3 == null)
                    {
                        if (treesNoGroupData != null)
                        {
                            pathStr2 = AssetDatabase.GetAssetPath(treesNoGroupData);
                            if (pathStr2 != "")
                                bytesTemp3 = File.ReadAllBytes(pathStr2);
                            else
                                bytesTemp3 = treesNoGroupData.bytes;
                        }
                    }

                    if (treesNoGroupCount != AltUtilities.ReadBytesInt(bytesTemp3, 12) || treesNoGroupEmptyCount != AltUtilities.ReadBytesInt(bytesTemp3, 16))
                    {
                        pathStr2 = AssetDatabase.GetAssetPath(treesNoGroupData);
                        if (pathStr2 != "")
                        {
                            bytesTemp3 = File.ReadAllBytes(pathStr2);
                        }
                        else
                        {
                            AssetDatabase.SaveAssets();
                            AssetDatabase.Refresh();
                            bytesTemp3 = treesNoGroupData.bytes;
                        }
                    }
                    
                    if (bytesTemp3 != null && bytesTemp3.Length > 0)
                    {
                        int version = AltUtilities.ReadBytesInt(bytesTemp3, 0);

                        if (version == 2)
                        {
                            if (AltUtilities.ReadBytesInt(bytesTemp3, 8) == objectsQuadIdTemp - 1)
                            {
                                if (changedAndAddTrees > 0)
                                {
                                    int changed = 0;
                                    int added = 0;

                                    #if UNITY_EDITOR
                                    {
                                        if (isProgressBar)
                                        {
                                            EditorUtility.DisplayProgressBar(progressBarTitle, "Saving objects... ", 0.0f);
                                        }

                                    }
                                    #endif

                                    int countPlaced = 0;
                                    float countPlacedAll = 0;


                                    if (File.Exists("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + altTrees.getIdManager() + "/treesData_" + stepX + "_" + stepY + "_objs.bytesTemp"))
                                        File.Delete("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + altTrees.getIdManager() + "/treesData_" + stepX + "_" + stepY + "_objs.bytesTemp");

                                    if (File.Exists("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + altTrees.getIdManager() + "/treesData_" + stepX + "_" + stepY + "_objs.bytesTemp.meta"))
                                        File.Delete("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + altTrees.getIdManager() + "/treesData_" + stepX + "_" + stepY + "_objs.bytesTemp.meta");

                                    File.Copy("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + altTrees.getIdManager() + "/treesData_" + stepX + "_" + stepY + "_objs.bytes", "Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + altTrees.getIdManager() + "/treesData_" + stepX + "_" + stepY + "_objs.bytesTemp");

                                    using (BinaryWriter writer = new BinaryWriter(File.Open("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + altTrees.getIdManager() + "/treesData_" + stepX + "_" + stepY + "_objs.bytesTemp", FileMode.Open)))
                                    {
                                        int summBytes = AltUtilities.ReadBytesInt(bytesTemp3, 4);

                                        foreach (int key in quadsAddObjects.Keys)
                                        {
                                            List<AltTreesTrees> attArr = quadsAddObjects[key];
                                            int count = attArr.Count;
                                            
                                            int addrObjs = AltUtilities.ReadBytesInt(bytesTemp3, 20 + (key - 1) * 4);
                                            int countObjs = 0;
                                            int countAll = 0;
                                            int countTemp = 0;
                                            int last = 0;
                                            bool isOk = true;
                                            changed = 0;
                                            added = 0;

                                            //System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
                                            //timer.Start();

                                            if(treesNoGroupEmptyCount > 0)
                                            {
                                                while (addrObjs != -1 && countTemp != count && isOk)
                                                {
                                                    countObjs = AltUtilities.ReadBytesInt(bytesTemp3, addrObjs + 4);
                                                    isOk = false;

                                                    if (countObjs > 0)
                                                    {
                                                        for (int i = 0; i < countObjs; i++)
                                                        {
                                                            if (!AltUtilities.ReadBytesBool(bytesTemp3, addrObjs + 8 + i * 61 + 0))
                                                            {
                                                                attArr[last].idTree = countAll + i;
                                                                countTemp++;
                                                                changed++;
                                                            
                                                                writer.Seek(addrObjs + 8 + i * 61, SeekOrigin.Begin);
                                                                AltUtilities.WriteBytes(true, bytes61Temp, 0);
                                                                AltUtilities.WriteBytes(attArr[last].pos, bytes61Temp, 1);
                                                                AltUtilities.WriteBytes(attArr[last].idPrototype, bytes61Temp, 13);
                                                                AltUtilities.WriteBytes(attArr[last].color, bytes61Temp, 17);
                                                                AltUtilities.WriteBytes(attArr[last].colorBark, bytes61Temp, 33);
                                                                AltUtilities.WriteBytes(attArr[last].rotation, bytes61Temp, 49);
                                                                AltUtilities.WriteBytes(attArr[last].heightScale, bytes61Temp, 53);
                                                                AltUtilities.WriteBytes(attArr[last].widthScale, bytes61Temp, 57);
                                                                writer.Write(bytes61Temp);


                                                                #if UNITY_EDITOR
                                                                {
                                                                    if(isProgressBar)
                                                                    { 
                                                                        countPlaced++;
                                                                        countPlacedAll++;


                                                                        if (countPlaced >= (changedAndAddTrees / 50f))
                                                                        {
                                                                            countPlaced = 0;
                                        
                                                                            EditorUtility.DisplayProgressBar(progressBarTitle, "Saving objects... ", countPlacedAll / changedAndAddTrees);
                                                                        }
                                                                    }
                                                                }
                                                                #endif


                                                                isOk = true;
                                                                last++;

                                                                if (countTemp == count || !isOk)
                                                                    break;
                                                            }
                                                        }
                                                        countAll += countObjs;
                                                    }
                                                    addrObjs = AltUtilities.ReadBytesInt(bytesTemp3, addrObjs);
                                                }
                                            }

                                            if (countTemp != count)
                                            {
                                                int starAddr = -1;
                                                addrObjs = AltUtilities.ReadBytesInt(bytesTemp3, 20 + (key - 1) * 4);
                                                starAddr = 20 + (key - 1) * 4;

                                                countAll = 0;

                                                while (addrObjs != -1)
                                                {
                                                    countAll += AltUtilities.ReadBytesInt(bytesTemp3, addrObjs + 4);
                                                    starAddr = addrObjs;
                                                    addrObjs = AltUtilities.ReadBytesInt(bytesTemp3, addrObjs);
                                                }
                                                writer.Seek(starAddr, SeekOrigin.Begin);
                                                starAddr = summBytes;
                                                summBytes = summBytes + 8 + (count - countTemp) * 61;
                                                AltUtilities.WriteBytes(starAddr, bytes4Temp, 0);
                                                writer.Write(bytes4Temp);
                                                writer.Seek(4, SeekOrigin.Begin);
                                                AltUtilities.WriteBytes(summBytes, bytes4Temp, 0);
                                                writer.Write(bytes4Temp);

                                                writer.Seek(starAddr, SeekOrigin.Begin);

                                                AltUtilities.WriteBytes(-1, bytes4Temp, 0);
                                                writer.Write(bytes4Temp);
                                                AltUtilities.WriteBytes((count - countTemp), bytes4Temp, 0);
                                                writer.Write(bytes4Temp);
                                                
                                                for (int i = 0; i < count - countTemp; i++)
                                                {
                                                    attArr[last].idTree = countAll + i;
                                                    added++;
                                                    
                                                    AltUtilities.WriteBytes(true, bytes61Temp, 0);
                                                    AltUtilities.WriteBytes(attArr[last].pos, bytes61Temp, 1);
                                                    AltUtilities.WriteBytes(attArr[last].idPrototype, bytes61Temp, 13);
                                                    AltUtilities.WriteBytes(attArr[last].color, bytes61Temp, 17);
                                                    AltUtilities.WriteBytes(attArr[last].colorBark, bytes61Temp, 33);
                                                    AltUtilities.WriteBytes(attArr[last].rotation, bytes61Temp, 49);
                                                    AltUtilities.WriteBytes(attArr[last].heightScale, bytes61Temp, 53);
                                                    AltUtilities.WriteBytes(attArr[last].widthScale, bytes61Temp, 57);
                                                    writer.Write(bytes61Temp);


                                                    #if UNITY_EDITOR
                                                    {
                                                        if(isProgressBar)
                                                        { 
                                                            countPlaced++;
                                                            countPlacedAll++;

                                                            if (countPlaced >= (changedAndAddTrees / 50f))
                                                            {
                                                                countPlaced = 0;
                                        
                                                                EditorUtility.DisplayProgressBar(progressBarTitle, "Saving objects... ", countPlacedAll / changedAndAddTrees);
                                                            }
                                                        }
                                                    }
                                                    #endif

                                                    last++;
                                                }
                                            }
                                            

                                            treesNoGroupCount += added;
                                            treesNoGroupEmptyCount -= changed;
                                            
                                            writer.Seek(12, SeekOrigin.Begin);
                                            //AltUtilities.WriteBytes(treesNoGroupCount, bytes4Temp, 0);
                                            AltUtilities.WriteBytes(treesNoGroupCount, bytes4Temp, 0);
                                            writer.Write(bytes4Temp);
                                            //AltUtilities.WriteBytes(treesNoGroupEmptyCount, bytes4Temp, 0)
                                            AltUtilities.WriteBytes(treesNoGroupEmptyCount, bytes4Temp, 0);
                                            writer.Write(bytes4Temp);
                                            
                                            attArr.Clear();
                                            attArr = null;
                                        }
                                    }

                                    if (File.Exists("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + altTrees.getIdManager() + "/treesData_" + stepX + "_" + stepY + "_objs.bytes"))
                                        File.Replace("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + altTrees.getIdManager() + "/treesData_" + stepX + "_" + stepY + "_objs.bytesTemp", "Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + altTrees.getIdManager() + "/treesData_" + stepX + "_" + stepY + "_objs.bytes", null);
                                    else
                                        File.Move("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + altTrees.getIdManager() + "/treesData_" + stepX + "_" + stepY + "_objs.bytesTemp", "Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + altTrees.getIdManager() + "/treesData_" + stepX + "_" + stepY + "_objs.bytes");

                                    /*AssetDatabase.SaveAssets();
                                    AssetDatabase.Refresh();

                                    pathStr2 = AssetDatabase.GetAssetPath(treesNoGroupData);
                                    if (pathStr2 != "")
                                        bytesTemp3 = File.ReadAllBytes(pathStr2);
                                    else
                                        bytesTemp3 = treesNoGroupData.bytes;*/


                                    #if UNITY_EDITOR
                                    {
                                        if(isProgressBar)
                                        {
                                            EditorUtility.ClearProgressBar();
                                        }
                                    }
                                    #endif
                                }
                                if (removedTrees != null && removedTrees.Count > 0)
                                {
                                    if (File.Exists("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + altTrees.getIdManager() + "/treesData_" + stepX + "_" + stepY + "_objs.bytesTemp"))
                                        File.Delete("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + altTrees.getIdManager() + "/treesData_" + stepX + "_" + stepY + "_objs.bytesTemp");

                                    if (File.Exists("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + altTrees.getIdManager() + "/treesData_" + stepX + "_" + stepY + "_objs.bytesTemp.meta"))
                                        File.Delete("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + altTrees.getIdManager() + "/treesData_" + stepX + "_" + stepY + "_objs.bytesTemp.meta");

                                    File.Copy("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + altTrees.getIdManager() + "/treesData_" + stepX + "_" + stepY + "_objs.bytes", "Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + altTrees.getIdManager() + "/treesData_" + stepX + "_" + stepY + "_objs.bytesTemp");

                                    using (BinaryWriter writer = new BinaryWriter(File.Open("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + altTrees.getIdManager() + "/treesData_" + stepX + "_" + stepY + "_objs.bytesTemp", FileMode.Open)))
                                    {
                                        int addrObjs = 0;
                                        int countObjs = 0;
                                        int countAll = 0;
                                        bool isOk = false;
                                        int deleted = 0;

                                        for (int g = 0; g < removedTrees.Count; g++)
                                        {
                                            if (removedTrees[g].idQuadObject != -1)
                                            {
                                                addrObjs = AltUtilities.ReadBytesInt(bytesTemp3, 20 + (removedTrees[g].idQuadObject - 1) * 4);
                                                countObjs = 0;
                                                countAll = 0;
                                                isOk = false;

                                                while (addrObjs != -1 && !isOk)
                                                {
                                                    countObjs = AltUtilities.ReadBytesInt(bytesTemp3, addrObjs + 4);

                                                    if (countObjs > 0)
                                                    {
                                                        if (removedTrees[g].idTree < countAll + countObjs)
                                                        {
                                                            isOk = true;

                                                            writer.Seek(addrObjs + 8 + (removedTrees[g].idTree - countAll) * 61, SeekOrigin.Begin);
                                                            AltUtilities.WriteBytes(false, bytes1Temp, 0);
                                                            writer.Write(bytes1Temp);

                                                            treesNoGroupEmptyCount++;
                                                            deleted++;
                                                        }
                                                    }
                                                    if (!isOk)
                                                    {
                                                        countAll += countObjs;
                                                        addrObjs = AltUtilities.ReadBytesInt(bytesTemp3, addrObjs);
                                                    }
                                                }

                                                if (!isOk)
                                                    altTrees.LogError("Object not deleted(no finded).");
                                            }
                                            else
                                                altTrees.LogError("Object not deleted(idQuadObject == -1).");
                                        }

                                        /*if (treesNoGroupEmptyCount != AltUtilities.ReadBytesInt(bytesTemp3, 16) + deleted)
                                            altTrees.LogWarning("treesNoGroupEmptyCount = " + treesNoGroupEmptyCount + ", " + (AltUtilities.ReadBytesInt(bytesTemp3, 16) + deleted));*/

                                        writer.Seek(16, SeekOrigin.Begin);
                                        //AltUtilities.WriteBytes(treesNoGroupEmptyCount, bytes4Temp, 0);
                                        AltUtilities.WriteBytes(AltUtilities.ReadBytesInt(bytesTemp3, 16) + deleted, bytes4Temp, 0);
                                        writer.Write(bytes4Temp);

                                    }

                                    if (File.Exists("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + altTrees.getIdManager() + "/treesData_" + stepX + "_" + stepY + "_objs.bytes"))
                                        File.Replace("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + altTrees.getIdManager() + "/treesData_" + stepX + "_" + stepY + "_objs.bytesTemp", "Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + altTrees.getIdManager() + "/treesData_" + stepX + "_" + stepY + "_objs.bytes", null);
                                    else
                                        File.Move("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + altTrees.getIdManager() + "/treesData_" + stepX + "_" + stepY + "_objs.bytesTemp", "Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + altTrees.getIdManager() + "/treesData_" + stepX + "_" + stepY + "_objs.bytes");

                                    removedTrees.Clear();
                                }
                                if (editingTree != null || (editingTrees != null && editingTrees.Count > 0))
                                {
                                    if (File.Exists("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + altTrees.getIdManager() + "/treesData_" + stepX + "_" + stepY + "_objs.bytesTemp"))
                                        File.Delete("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + altTrees.getIdManager() + "/treesData_" + stepX + "_" + stepY + "_objs.bytesTemp");

                                    if (File.Exists("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + altTrees.getIdManager() + "/treesData_" + stepX + "_" + stepY + "_objs.bytesTemp.meta"))
                                        File.Delete("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + altTrees.getIdManager() + "/treesData_" + stepX + "_" + stepY + "_objs.bytesTemp.meta");

                                    File.Copy("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + altTrees.getIdManager() + "/treesData_" + stepX + "_" + stepY + "_objs.bytes", "Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + altTrees.getIdManager() + "/treesData_" + stepX + "_" + stepY + "_objs.bytesTemp");

                                    using (BinaryWriter writer = new BinaryWriter(File.Open("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + altTrees.getIdManager() + "/treesData_" + stepX + "_" + stepY + "_objs.bytesTemp", FileMode.Open)))
                                    {
                                        int addrObjs = 0;
                                        int countObjs = 0;
                                        int countAll = 0;
                                        bool isOk = false;

                                        if (editingTree != null)
                                        {
                                            addrObjs = AltUtilities.ReadBytesInt(bytesTemp3, 20 + (editingTree.idQuadObject - 1) * 4);

                                            while (addrObjs != -1 && !isOk)
                                            {
                                                countObjs = AltUtilities.ReadBytesInt(bytesTemp3, addrObjs + 4);

                                                if (countObjs > 0)
                                                {
                                                    if (editingTree.idTree < countAll + countObjs)
                                                    {
                                                        isOk = true;

                                                        writer.Seek(addrObjs + 8 + (editingTree.idTree - countAll) * 61, SeekOrigin.Begin);

                                                        AltUtilities.WriteBytes(true, bytes61Temp, 0);
                                                        AltUtilities.WriteBytes(editingTree.pos, bytes61Temp, 1);
                                                        AltUtilities.WriteBytes(editingTree.idPrototype, bytes61Temp, 13);
                                                        AltUtilities.WriteBytes(editingTree.color, bytes61Temp, 17);
                                                        AltUtilities.WriteBytes(editingTree.colorBark, bytes61Temp, 33);
                                                        AltUtilities.WriteBytes(editingTree.rotation, bytes61Temp, 49);
                                                        AltUtilities.WriteBytes(editingTree.heightScale, bytes61Temp, 53);
                                                        AltUtilities.WriteBytes(editingTree.widthScale, bytes61Temp, 57);
                                                        writer.Write(bytes61Temp);
                                                    }
                                                }
                                                if (!isOk)
                                                {
                                                    countAll += countObjs;
                                                    addrObjs = AltUtilities.ReadBytesInt(bytesTemp3, addrObjs);
                                                }
                                            }

                                            if (!isOk)
                                                altTrees.LogError("Object not editing(no finded).");
                                        }
                                        else
                                        {
                                            for (int g = 0; g < editingTrees.Count; g++)
                                            {
                                                addrObjs = 0;
                                                countObjs = 0;
                                                countAll = 0;
                                                isOk = false;

                                                addrObjs = AltUtilities.ReadBytesInt(bytesTemp3, 20 + (editingTrees[g].idQuadObject - 1) * 4);

                                                while (addrObjs != -1 && !isOk)
                                                {
                                                    countObjs = AltUtilities.ReadBytesInt(bytesTemp3, addrObjs + 4);

                                                    if (countObjs > 0)
                                                    {
                                                        if (editingTrees[g].idTree < countAll + countObjs)
                                                        {
                                                            isOk = true;

                                                            writer.Seek(addrObjs + 8 + (editingTrees[g].idTree - countAll) * 61, SeekOrigin.Begin);

                                                            AltUtilities.WriteBytes(true, bytes61Temp, 0);
                                                            AltUtilities.WriteBytes(editingTrees[g].pos, bytes61Temp, 1);
                                                            AltUtilities.WriteBytes(editingTrees[g].idPrototype, bytes61Temp, 13);
                                                            AltUtilities.WriteBytes(editingTrees[g].color, bytes61Temp, 17);
                                                            AltUtilities.WriteBytes(editingTrees[g].colorBark, bytes61Temp, 33);
                                                            AltUtilities.WriteBytes(editingTrees[g].rotation, bytes61Temp, 49);
                                                            AltUtilities.WriteBytes(editingTrees[g].heightScale, bytes61Temp, 53);
                                                            AltUtilities.WriteBytes(editingTrees[g].widthScale, bytes61Temp, 57);
                                                            writer.Write(bytes61Temp);
                                                        }
                                                    }
                                                    if (!isOk)
                                                    {
                                                        countAll += countObjs;
                                                        addrObjs = AltUtilities.ReadBytesInt(bytesTemp3, addrObjs);
                                                    }
                                                }

                                                if (!isOk)
                                                    altTrees.LogError("Object not editing(no finded).");
                                            }
                                        }
                                    }
                                    if (File.Exists("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + altTrees.getIdManager() + "/treesData_" + stepX + "_" + stepY + "_objs.bytes"))
                                        File.Replace("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + altTrees.getIdManager() + "/treesData_" + stepX + "_" + stepY + "_objs.bytesTemp", "Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + altTrees.getIdManager() + "/treesData_" + stepX + "_" + stepY + "_objs.bytes", null);
                                    else
                                        File.Move("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + altTrees.getIdManager() + "/treesData_" + stepX + "_" + stepY + "_objs.bytesTemp", "Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + altTrees.getIdManager() + "/treesData_" + stepX + "_" + stepY + "_objs.bytes");

                                }
                            }
                            else
                                altTrees.LogError("Count Quads in file(" + AltUtilities.ReadBytesInt(bytesTemp3, 8) + ") != Count Quads in Manager(" + (objectsQuadIdTemp - 1) + ")");
                        }
                        else
                            altTrees.LogError("Old version of the objects file.");
                    }
                    else
                        altTrees.LogError("bytesTemp3 == null || bytesTemp3.Length == 0");


                    pathStr2 = AssetDatabase.GetAssetPath(treesNoGroupData);
                    if (pathStr2 != "")
                        bytesTemp3 = File.ReadAllBytes(pathStr2);
                    else
                    {
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                        bytesTemp3 = treesNoGroupData.bytes;
                    }
                }
                else
                    altTrees.LogError("false");
            }
            #endif
        }


        public void addTrees(AddTreesPositionsStruct[] positions, bool randomRotation, bool isRandomHeight, float height, float heightRandom,
                            bool lockWidthToHeight, bool isRandomWidth, float width, float widthRandom, Color32 hueLeaves, Color32 hueBark, bool isRandomHueLeaves, bool isRandomHueBark, bool isProgressBar = false, string progressBarTitle = "")
        {
            altTreesManager.addTrees(positions, altTreesId, randomRotation, isRandomHeight, height, heightRandom, lockWidthToHeight, isRandomWidth, width, widthRandom, hueLeaves, hueBark, isRandomHueLeaves, isRandomHueBark, false, isProgressBar, progressBarTitle);
        }

        public void addTreesImport(AddTreesStruct[] trees, bool save)
        {
            altTreesManager.addTreesImport(trees, altTreesId, save);
        }

        public bool removeTrees(Vector2 pos, float radius, List<int> removedTrees, List<AltTreesTrees> removedTreesNoGroup, int idPrototype = -1)
        {
            if (altTreesManager.removeTrees(pos, radius, this, removedTrees, removedTreesNoGroup, idPrototype))
            {
                //recalculateBound();
                return true;
            }
            else
                return false;
        }
        public bool removeTrees(Vector2 pos, float radius, int idPrototype = -1)
        {
            if (altTreesManager.removeTrees(pos, radius, this, idPrototype))
            {
                //recalculateBound();
                return true;
            }
            else
                return false;
        }

        /*public bool removeTrees(Vector2 pos, float sizeX, float sizeZ, List<int> removedTrees, bool udpadeTreesOnScene = true)
        {
            if (altTreesManager.removeTrees(pos, sizeX, sizeZ, this, removedTrees, udpadeTreesOnScene))
            {
                //recalculateBound();
                return true;
            }
            else
                return false;
        }*/

        public AltTreesTrees[] getTreesForExport(Vector2 pos, float sizeX, float sizeZ)
        {
            return altTreesManager.getTreesForExport(pos, sizeX, sizeZ, this);
        }

        public void getTrees(Vector2 _pos, float radius, bool trees, bool objects, int idPrototype, List<AltTreesTrees> _attTemp)
        {
            altTreesManager.getTrees(_pos, radius, trees, objects, idPrototype, this, _attTemp);
        }

        public void getAllTrees(bool trees, bool objects, int idPrototype, List<AltTreesTrees> _attTemp)
        {
            altTreesManager.getAllTrees(trees, objects, idPrototype, this, _attTemp);
        }

        public void checkTreePrototype(int id, AltTree tree, bool addInitObjPool, bool save)
        {
            bool isOk = false;
            for (int i = 0; i < prototypes.Length; i++)
            {
                if (prototypes[i].isEnabled && prototypes[i].tree.id == id)
                {
                    isOk = true;
                    break;
                }
            }
            if (!isOk)
            {
                AltTreePrototypes[] protTemp = prototypes;
                prototypes = new AltTreePrototypes[protTemp.Length + 1];
                for (int i = 0; i < protTemp.Length; i++)
                {
                    prototypes[i] = protTemp[i];
                }
                prototypes[prototypes.Length - 1] = new AltTreePrototypes();
                prototypes[prototypes.Length - 1].tree = tree;
                prototypes[prototypes.Length - 1].isObject = tree.isObject;
                if(addInitObjPool)
                {
                    altTreesManager.addInitObjPool(tree);
                }
                if(save)
                { 
                    #if UNITY_EDITOR
                        EditorUtility.SetDirty(altTrees.altTreesManagerData);
                    #endif
                }
            }
        }

        public void checkTreeOrObject(int id)
        {
            if (prototypes[id].isObject != prototypes[id].tree.isObject)
            {
                #if UNITY_EDITOR
                    EditorUtility.DisplayProgressBar("Working ... Please wait ... ", "Working ... Please wait ... ", 0.1f);
                #endif
                altTreesManager.addPatch(this);

                AltTreesTrees[] treesTemp = trees;
                //AltTreesTrees[] objectsTemp = new AltTreesTrees[treesNoGroupCount - treesNoGroupEmptyCount];
                AltTreesTrees[] objectsTemp = new AltTreesTrees[treesNoGroupCount];

                int objectsSch = 0;
                int countQuads = 0;

                if (bytesTemp3 != null && bytesTemp3.Length > 0)
                {
                    int version = AltUtilities.ReadBytesInt(bytesTemp3, 0);

                    Vector3 _pos = new Vector3();
                    int _idPrototype;
                    Color _color = new Color();
                    Color _colorBark = new Color();
                    float _rotation;
                    float _heightScale;
                    float _widthScale;
                    int objectsSch2 = 0;

                    countQuads = AltUtilities.ReadBytesInt(bytesTemp3, 8);

                    if (version == 2)
                    {
                        if (countQuads == objectsQuadIdTemp - 1)
                        {
                            for (int t = 0; t < countQuads; t++)
                            {
                                int addrObjs = AltUtilities.ReadBytesInt(bytesTemp3, 20 + t * 4);
                                int countObjs = 0;
                                objectsSch2 = 0;

                                while (addrObjs != -1)
                                {
                                    countObjs = AltUtilities.ReadBytesInt(bytesTemp3, addrObjs + 4);

                                    if (countObjs > 0)
                                    {
                                        for (int i = 0; i < countObjs; i++)
                                        {
                                            if (AltUtilities.ReadBytesBool(bytesTemp3, addrObjs + 8 + i * 61 + 0))
                                            {
                                                _pos = AltUtilities.ReadBytesVector3(bytesTemp3, addrObjs + 8 + i * 61 + 1);
                                                _idPrototype = AltUtilities.ReadBytesInt(bytesTemp3, addrObjs + 8 + i * 61 + 13);
                                                _color = AltUtilities.ReadBytesColor(bytesTemp3, addrObjs + 8 + i * 61 + 17);
                                                _colorBark = AltUtilities.ReadBytesColor(bytesTemp3, addrObjs + 8 + i * 61 + 33);
                                                _rotation = AltUtilities.ReadBytesFloat(bytesTemp3, addrObjs + 8 + i * 61 + 49);
                                                _heightScale = AltUtilities.ReadBytesFloat(bytesTemp3, addrObjs + 8 + i * 61 + 53);
                                                _widthScale = AltUtilities.ReadBytesFloat(bytesTemp3, addrObjs + 8 + i * 61 + 57);

                                                AltTreesTrees att = new AltTreesTrees(_pos, objectsSch2, _idPrototype, true, _color, _colorBark, _rotation, _heightScale, _widthScale, this);

                                                att.idQuadObject = t;
                                                objectsTemp[objectsSch] = att;
                                                objectsSch++;
                                                objectsSch2++;
                                            }
                                        }
                                    }
                                    addrObjs = AltUtilities.ReadBytesInt(bytesTemp3, addrObjs);
                                }
                            }
                        }
                        else
                            altTrees.LogError("Count Quads in file(" + AltUtilities.ReadBytesInt(bytesTemp3, 8) + ") != Count Quads in Manager(" + (objectsQuadIdTemp - 1) + ")");
                    }
                    else
                        altTrees.LogError("Version == "+version);
                }
                //altTrees.LogError("treesNoGroupCount(" + treesNoGroupCount + ") != treesNoGroupEmptyCount(" + treesNoGroupEmptyCount + ")");

                treesNoGroupCount = objectsSch;
                treesNoGroupEmptyCount = 0;

                //if (objectsTemp.Length != treesNoGroupCount)
                //    altTrees.LogError("objectsTemp.Length("+ objectsTemp.Length + ") != treesNoGroupCount("+ treesNoGroupCount + ")");

                AltTreesTrees[] treesNoGroup = null;

                #if UNITY_EDITOR
                    EditorUtility.DisplayProgressBar("Working ... Please wait ... ", "Working ... Please wait ... ", 0.2f);
                #endif

                int count = 0;
                bool boolTemp = false;

                if (prototypes[id].isObject)
                {
                    for (int i = 0; i < treesNoGroupCount; i++)
                    {
                        if (objectsTemp[i].idPrototype == prototypes[id].tree.id)
                        {
                            count++;
                        }
                    }
                    treesNoGroup = new AltTreesTrees[treesNoGroupCount - count];
                    trees = new AltTreesTrees[treesCount - treesEmptyCount + count];

                    int treesIndx = 0;
                    int treesNoGroupIndx = 0;

                    for (int i = 0; i < treesNoGroupCount; i++)
                    {
                        if (objectsTemp[i].idPrototype == prototypes[id].tree.id)
                        {
                            trees[treesIndx] = objectsTemp[i];
                            treesIndx++;
                        }
                        else
                        {
                            treesNoGroup[treesNoGroupIndx] = objectsTemp[i];
                            treesNoGroupIndx++;
                        }
                    }
                    for (int i = 0; i < treesTemp.Length; i++)
                    {
                        boolTemp = false;
                        for (int j = 0; j < treesEmptyCount; j++)
                        {
                            if (treesEmpty[j] == i)
                            {
                                boolTemp = true;
                                break;
                            }
                        }
                        if (!boolTemp)
                        {
                            trees[treesIndx] = treesTemp[i];
                            treesIndx++;
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < treesCount; i++)
                    {
                        if (treesTemp[i].idPrototype == prototypes[id].tree.id)
                        {
                            boolTemp = false;
                            for (int j = 0; j < treesEmptyCount; j++)
                            {
                                if (treesEmpty[j] == i)
                                {
                                    boolTemp = true;
                                    break;
                                }
                            }
                            if (!boolTemp)
                                count++;
                        }
                    }
                    trees = new AltTreesTrees[treesCount - treesEmptyCount - count];
                    treesNoGroup = new AltTreesTrees[treesNoGroupCount + count];

                    int treesIndx = 0;
                    int treesNoGroupIndx = 0;

                    for (int i = 0; i < treesTemp.Length; i++)
                    {
                        boolTemp = false;
                        for (int j = 0; j < treesEmptyCount; j++)
                        {
                            if (treesEmpty[j] == i)
                            {
                                boolTemp = true;
                                break;
                            }
                        }
                        if (!boolTemp)
                        {
                            if (treesTemp[i].idPrototype == prototypes[id].tree.id)
                            {
                                treesNoGroup[treesNoGroupIndx] = treesTemp[i];
                                treesNoGroupIndx++;
                            }
                            else
                            {
                                trees[treesIndx] = treesTemp[i];
                                treesIndx++;
                            }
                        }
                    }
                    for (int i = 0; i < treesNoGroupCount; i++)
                    {
                        treesNoGroup[treesNoGroupIndx] = objectsTemp[i];
                        treesNoGroupIndx++;
                    }
                }
                prototypes[id].isObject = prototypes[id].tree.isObject;

                treesCount = trees.Length;
                treesEmptyCount = 0;
                treesEmpty = new int[0];

                treesNoGroupCount = treesNoGroup.Length;
                treesNoGroupEmptyCount = 0;

                byte[] bytes4Temp = new byte[4];
                byte[] bytes60Temp = new byte[60];
                byte[] bytes61Temp = new byte[61];

                #if UNITY_EDITOR
                    EditorUtility.DisplayProgressBar("Working ... Please wait ... ", "Working ... Please wait ... ", 0.3f);
                #endif

                if (File.Exists("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + altTrees.getIdManager() + "/treesData_" + stepX + "_" + stepY + ".bytesTemp"))
                    File.Delete("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + altTrees.getIdManager() + "/treesData_" + stepX + "_" + stepY + ".bytesTemp");

                if (File.Exists("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + altTrees.getIdManager() + "/treesData_" + stepX + "_" + stepY + ".bytesTemp.meta"))
                    File.Delete("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + altTrees.getIdManager() + "/treesData_" + stepX + "_" + stepY + ".bytesTemp.meta");
                
                using (BinaryWriter writer = new BinaryWriter(File.Open("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + altTrees.getIdManager() + "/treesData_" + stepX + "_" + stepY + ".bytesTemp", FileMode.Create)))
                {
                    AltUtilities.WriteBytes(1, bytes4Temp, 0);
                    writer.Write(bytes4Temp);
                    AltUtilities.WriteBytes(treesCount, bytes4Temp, 0);
                    writer.Write(bytes4Temp);
                    AltUtilities.WriteBytes(0, bytes4Temp, 0);
                    writer.Write(bytes4Temp);
                    
                    for (int i = 0; i < treesCount; i++)
                    {
                        AltUtilities.WriteBytes(trees[i].pos, bytes60Temp, 0);
                        AltUtilities.WriteBytes(trees[i].idPrototype, bytes60Temp, 12);
                        AltUtilities.WriteBytes(trees[i].color, bytes60Temp, 16);
                        AltUtilities.WriteBytes(trees[i].colorBark, bytes60Temp, 32);
                        AltUtilities.WriteBytes(trees[i].rotation, bytes60Temp, 48);
                        AltUtilities.WriteBytes(trees[i].heightScale, bytes60Temp, 52);
                        AltUtilities.WriteBytes(trees[i].widthScale, bytes60Temp, 56);
                        writer.Write(bytes60Temp);
                    }
                }
                if (File.Exists("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + altTrees.getIdManager() + "/treesData_" + stepX + "_" + stepY + ".bytes"))
                    File.Replace("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + altTrees.getIdManager() + "/treesData_" + stepX + "_" + stepY + ".bytesTemp", "Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + altTrees.getIdManager() + "/treesData_" + stepX + "_" + stepY + ".bytes", null);
                else
                    File.Move("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + altTrees.getIdManager() + "/treesData_" + stepX + "_" + stepY + ".bytesTemp", "Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + altTrees.getIdManager() + "/treesData_" + stepX + "_" + stepY + ".bytes");

                #if UNITY_EDITOR
                    EditorUtility.DisplayProgressBar("Working ... Please wait ... ", "Working ... Please wait ... ", 0.4f);
                #endif

                if (File.Exists("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + altTrees.getIdManager() + "/treesData_" + stepX + "_" + stepY + "_objs.bytesTemp"))
                    File.Delete("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + altTrees.getIdManager() + "/treesData_" + stepX + "_" + stepY + "_objs.bytesTemp");

                if (File.Exists("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + altTrees.getIdManager() + "/treesData_" + stepX + "_" + stepY + "_objs.bytesTemp.meta"))
                    File.Delete("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + altTrees.getIdManager() + "/treesData_" + stepX + "_" + stepY + "_objs.bytesTemp.meta");
                
                using (BinaryWriter writer = new BinaryWriter(File.Open("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + altTrees.getIdManager() + "/treesData_" + stepX + "_" + stepY + "_objs.bytesTemp", FileMode.Create)))
                {
                    int summBytes = 20 + (objectsQuadIdTemp - 1) * 4;
                    byte[] bytesTemp2 = new byte[summBytes];
                    AltUtilities.WriteBytes(2, bytesTemp2, 0);
                    AltUtilities.WriteBytes(summBytes, bytesTemp2, 4);
                    AltUtilities.WriteBytes((objectsQuadIdTemp - 1), bytesTemp2, 8);
                    AltUtilities.WriteBytes(treesNoGroupCount, bytesTemp2, 12);
                    AltUtilities.WriteBytes(0, bytesTemp2, 16);

                    for (int i = 0; i < (objectsQuadIdTemp - 1); i++)
                    {
                        AltUtilities.WriteBytes(-1, bytesTemp2, 20 + i * 4);
                    }
                    writer.Write(bytesTemp2);
                    
                    AltTreesTrees attTemp = null;
                    int startBytes = 0;

                    Vector3 vector3Temp;
                    bool stopTemp = false;
                    for (int h = 0; h < treesNoGroup.Length; h++)
                    {
                        stopTemp = false;
                        vector3Temp = treesNoGroup[h].getPosWorld();
                        altTreesManager.quads[altTreesId].getObjectQuadId(vector3Temp.x, vector3Temp.z, ref treesNoGroup[h].idQuadObject, ref stopTemp);
                    }

                    for (int i = 0; i < (objectsQuadIdTemp - 1); i++)
                    {
                        int countObjs = 0;
                        for (int h = 0; h < treesNoGroup.Length; h++)
                        {
                            attTemp = treesNoGroup[h];

                            if (attTemp.idQuadObject - 1 == i)
                            {
                                if (countObjs == 0)
                                {
                                    AltUtilities.WriteBytes(summBytes, bytes4Temp, 0);
                                    writer.Seek(20 + i * 4, SeekOrigin.Begin);
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
                }
                
                if (File.Exists("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + altTrees.getIdManager() + "/treesData_" + stepX + "_" + stepY + "_objs.bytes"))
                    File.Replace("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + altTrees.getIdManager() + "/treesData_" + stepX + "_" + stepY + "_objs.bytesTemp", "Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + altTrees.getIdManager() + "/treesData_" + stepX + "_" + stepY + "_objs.bytes", null);
                else
                    File.Move("Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + altTrees.getIdManager() + "/treesData_" + stepX + "_" + stepY + "_objs.bytesTemp", "Assets/Plugins/AltSystems/AltTrees/DataBase/AltTreesData/" + altTrees.getIdManager() + "/treesData_" + stepX + "_" + stepY + "_objs.bytes");

                #if UNITY_EDITOR
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    EditorUtility.SetDirty(altTrees.altTreesManagerData);
                    
                    EditorUtility.ClearProgressBar();
                #endif
                

                if (treesNoGroupData != null)
                {
                    pathStr2 = AssetDatabase.GetAssetPath(treesNoGroupData);
                    if (pathStr2 != "")
                        bytesTemp3 = File.ReadAllBytes(pathStr2);
                    else
                    {
                        bytesTemp3 = treesNoGroupData.bytes;
                    }
                }
                else
                    bytesTemp3 = null;

                altTrees.reInitTimer = 1;
            }
        }

        public Vector3 getTreePosLocal(Vector3 pos, Vector3 jump, Vector3 jumpPos, float sizePatch)
        {
            return (pos - (step - jump) * sizePatch - jumpPos) / sizePatch;
        }

        /*public void recalculateBound()
        {
            minPosXZ = new Vector2(1000000, 1000000);
            maxPosXZ = new Vector2(-1000000, -1000000);

            if (trees != null)
            {
                for (int j = 0; j < trees.Length; j++)
                {
                    if (trees[j] != null)
                    {
                        if (trees[j].pos.x < minPosXZ.x)
                            minPosXZ.x = trees[j].pos.x;
                        if (trees[j].pos.x > maxPosXZ.x)
                            maxPosXZ.x = trees[j].pos.x;
                        if (trees[j].pos.z < minPosXZ.y)
                            minPosXZ.y = trees[j].pos.z;
                        if (trees[j].pos.z > maxPosXZ.y)
                            maxPosXZ.y = trees[j].pos.z;
                    }
                }
            }
            if (treesNoGroup != null)
            {
                for (int j = 0; j < treesNoGroup.Length; j++)
                {
                    if (treesNoGroup[j] != null)
                    {
                        if (treesNoGroup[j].pos.x < minPosXZ.x)
                            minPosXZ.x = treesNoGroup[j].pos.x;
                        if (treesNoGroup[j].pos.x > maxPosXZ.x)
                            maxPosXZ.x = treesNoGroup[j].pos.x;
                        if (treesNoGroup[j].pos.z < minPosXZ.y)
                            minPosXZ.y = treesNoGroup[j].pos.z;
                        if (treesNoGroup[j].pos.z > maxPosXZ.y)
                            maxPosXZ.y = treesNoGroup[j].pos.z;
                    }
                }
            }

            sizeQuad = Mathf.Max(maxPosXZ.x - minPosXZ.x, maxPosXZ.y - minPosXZ.y);
        }*/

        public AltTree getAltTreePrototype(int idPrototype)
        {
            for (int j = 0; j < prototypes.Length; j++)
            {
                if (prototypes[j].tree.id == idPrototype)
                    return prototypes[j].tree;
            }
            return null;
        }
    }

    public class AddTreesPositionsStruct
    {
        public Vector3 pos;
        public AltTree altTree;

        public AddTreesPositionsStruct(Vector3 _pos, AltTree _altTree)
        {
            pos = _pos;
            altTree = _altTree;
        }
    }

    public class AddTreesStruct
    {
        public Vector3 pos;
        public int idPrototype = -1;
        public Color color;
        public Color colorBark;
        public float rotation;
        public float heightScale;
        public float widthScale;
        public bool isObject = false;

        public AddTreesStruct(Vector3 _pos, int _idPrototype, bool _isObject, float _rotation = 0f, float _heightScale = 1f, float _widthScale = 1f, Color? _colorLeaves = null, Color? _colorBark = null)
        {
            pos = _pos;
            idPrototype = _idPrototype;
            color = _colorLeaves ?? new Color32(255, 0, 0, 80);
            colorBark = _colorBark ?? new Color32(0, 0, 0, 100);
            rotation = _rotation;
            heightScale = _heightScale;
            widthScale = _widthScale;
            isObject = _isObject;
        }
    }

    [System.Serializable]
    public class AltTreePrototypes
    {
        [System.NonSerialized]
        public bool isEnabled = true;
        public bool isObject = false;
        public AltTree tree = null;
        public int idBundle = -1;
    }

    public class AltTreesTrees
    {
        public Vector3 posWorldBillboard;
        public Vector3 posWorldMesh;
        public Vector3 pos;
        public Vector2 pos2D;
        public Bounds3D bound = new Bounds3D();
        public bool inFrustum = true;
        public int frustumSchet = 0;
        public int idTree;
        public int idPrototype;
        public int idPrototypeIndex;
        public bool gpuInstancing = false;
        public int treesToRenderId = -1;
        [System.NonSerialized]
        public int altTreesId;
        public MaterialPropertyBlock propBlockBillboards = null;
        public MaterialPropertyBlock propBlockMesh = null;
        public Matrix4x4 matrixMesh;
        public Matrix4x4 matrixBillboard;

        public float densityObjects = 0f;

        static float densityObjectsSchet = 0f;

        public Color color = new Color(1, 1, 1, 0);
        public Color colorBark = new Color(1, 1, 1, 0);
        public Color colorDebug = new Color();
        public float rotation;
        public float heightScale;
        public float widthScale;
        public float maxScaleSquare;

        public float widthPropBlock;
        public float heightPropBlock;
        public float alphaPropBlockBillboard;
        public float alphaPropBlockMesh;
        public Color huePropBlock;
        public float indPropBlockMesh;
        public float smoothPropBlock;

        public bool isObject = false;

        [System.NonSerialized]
        public GameObject goMesh;
        [System.NonSerialized]
        public ColliderPool collider;
        public int currentLOD = -1;

        public int newLOD = -1;
        public float distance = 0;

        public int currentCrossFadeId = -1;
        public int currentCrossFadeLOD = -1;
        public float crossFadeTime = 0f;


        public bool isBillboard = false;
        public bool isCrossFadeBillboard = false;

        public bool isMesh = false;
        public bool isCrossFadeMesh = false;

        //[System.NonSerialized]
        //public GameObject goCrossFade;
        //[System.NonSerialized]
        //public MeshRenderer crossFadeBillboardMeshRenderer;
        [System.NonSerialized]
        public MeshRenderer crossFadeTreeMeshRenderer;
        [System.NonSerialized]
        public int countCheckLODs = 0;

        public bool noNull = false;

        [System.NonSerialized]
        public int idQuadObject = -1;
        [System.NonSerialized]
        public int idQuadObjectNew = -1;

        [System.NonSerialized]
        public AltTreesPatch altTreesPatch;

        public AltTreesTrees(Vector3 _pos, int _idTree, int _idPrototype, bool _isObject, Color _colorLeaves, Color _colorBark, float _rotation, float _heightScale, float _widthScale, AltTreesPatch _altTreesPatch)
        {
            pos = _pos;
            pos2D = new Vector2(pos.x, pos.z);
            idTree = _idTree;
            idPrototype = _idPrototype;
            idPrototypeIndex = -1;
            gpuInstancing = false;

            densityObjects = densityObjectsSchet;
            densityObjectsSchet++;
            if (densityObjectsSchet >= 100f)
                densityObjectsSchet = 0f;

            color = _colorLeaves;
            colorBark = _colorBark;
            rotation = _rotation;
            heightScale = _heightScale;
            widthScale = _widthScale;
            maxScaleSquare = Mathf.Max(heightScale, widthScale);
            maxScaleSquare *= maxScaleSquare;

            altTreesPatch = _altTreesPatch;
            
            isObject = _isObject;

            noNull = true;
        }

        public AltTreesTrees(AltTreesTrees att, int _idTree)
        {
            pos = att.pos;
            pos2D = att.pos2D;
            idTree = _idTree;
            idPrototype = att.idPrototype;
            idPrototypeIndex = att.idPrototypeIndex;
            gpuInstancing = att.gpuInstancing;
            idQuadObject = att.idQuadObject;

            densityObjects = densityObjectsSchet;
            densityObjectsSchet++;
            if (densityObjectsSchet >= 100f)
                densityObjectsSchet = 0f;

            color = att.color;
            rotation = att.rotation;
            heightScale = att.heightScale;
            widthScale = att.widthScale;
            maxScaleSquare = Mathf.Max(heightScale, widthScale);
            maxScaleSquare *= maxScaleSquare;

            altTreesPatch = att.altTreesPatch;

            isObject = att.isObject;

            noNull = true;

            currentLOD = att.currentLOD;
        }

        Vector3 temp;
        public Vector3 getPosWorld()
        {
            temp.x = (pos.x + altTreesPatch.step.x - altTreesPatch.altTreesManager.jump.x) * altTreesPatch.altTrees.altTreesManagerData.sizePatch + altTreesPatch.altTreesManager.jumpPos.x;
            temp.y = (pos.y + altTreesPatch.step.y - altTreesPatch.altTreesManager.jump.y) * altTreesPatch.altTrees.altTreesManagerData.sizePatch + altTreesPatch.altTreesManager.jumpPos.y;
            temp.z = (pos.z + altTreesPatch.step.z - altTreesPatch.altTreesManager.jump.z) * altTreesPatch.altTrees.altTreesManagerData.sizePatch + altTreesPatch.altTreesManager.jumpPos.z;

            return temp;
        }

        Vector2 temp2D;
        public Vector2 getPosWorld2DWithoutJump()
        {
            temp2D.x = pos.x * altTreesPatch.altTrees.altTreesManagerData.sizePatch;
            temp2D.y = pos.z * altTreesPatch.altTrees.altTreesManagerData.sizePatch;

            return temp2D;
        }

        public Vector2 getPosWorld2DWithJump()
        {
            temp2D.x = (pos.x + altTreesPatch.step.x - altTreesPatch.altTreesManager.jump.x) * altTreesPatch.altTrees.altTreesManagerData.sizePatch + altTreesPatch.altTreesManager.jumpPos.x;
            temp2D.y = (pos.z + altTreesPatch.step.z - altTreesPatch.altTreesManager.jump.z) * altTreesPatch.altTrees.altTreesManagerData.sizePatch + altTreesPatch.altTreesManager.jumpPos.z;
            
            return temp2D;
        }

        public Vector2 get2DPosWorld()
        {
            temp.x = (pos.x + altTreesPatch.step.x - altTreesPatch.altTreesManager.jump.x) * altTreesPatch.altTrees.altTreesManagerData.sizePatch + altTreesPatch.altTreesManager.jumpPos.x;
            temp.z = (pos.z + altTreesPatch.step.z - altTreesPatch.altTreesManager.jump.z) * altTreesPatch.altTrees.altTreesManagerData.sizePatch + altTreesPatch.altTreesManager.jumpPos.z;

            return new Vector2(temp.x, temp.z);
        }

        public Vector3 getPosWorldBillboard()
        {
            temp.x = pos.x * altTreesPatch.altTrees.altTreesManagerData.sizePatch;
            temp.y = pos.y * altTreesPatch.altTrees.altTreesManagerData.sizePatch;
            temp.z = pos.z * altTreesPatch.altTrees.altTreesManagerData.sizePatch;

            return temp;
        }
    }

    public class UpdateDataQuadsTemp
    {
        public Vector2 pos;
        public int LOD = -1;
        public int maxLOD = -1;
        public float size = -1;
        public float sizeSQR = -1;
        public Bounds2D bound;
        public bool isChildQuads = false;
        public UpdateDataQuadsTemp[] quads = new UpdateDataQuadsTemp[4];
        public int id = 0;
        public int quadId = 0;

        public List<AltTreesTrees> objs = new List<AltTreesTrees>();

        public UpdateDataQuadsTemp(float x, float z, float _size, int _LOD, int _maxLOD, List<UpdateDataQuadsTemp> _updateDataQuadsTempList, ref int _updateDataQuadsTempId, int _quadId)
        {
            pos = new Vector2(x, z);
            LOD = _LOD;
            size = _size;
            sizeSQR = size * size;
            maxLOD = _maxLOD;
            quadId = _quadId;

            bound = new Bounds2D(pos, size);

            if (LOD < maxLOD)
            {
                isChildQuads = true;
                quads[0] = new UpdateDataQuadsTemp(x - size / 4f, z + size / 4f, size / 2f, LOD + 1, maxLOD, _updateDataQuadsTempList, ref _updateDataQuadsTempId, 1);
                quads[1] = new UpdateDataQuadsTemp(x + size / 4f, z + size / 4f, size / 2f, LOD + 1, maxLOD, _updateDataQuadsTempList, ref _updateDataQuadsTempId, 2);
                quads[2] = new UpdateDataQuadsTemp(x - size / 4f, z - size / 4f, size / 2f, LOD + 1, maxLOD, _updateDataQuadsTempList, ref _updateDataQuadsTempId, 3);
                quads[3] = new UpdateDataQuadsTemp(x + size / 4f, z - size / 4f, size / 2f, LOD + 1, maxLOD, _updateDataQuadsTempList, ref _updateDataQuadsTempId, 4);
            }
            else
            {
                _updateDataQuadsTempList.Add(this);
                id = _updateDataQuadsTempId;
                _updateDataQuadsTempId++;
            }
        }

        public void checkTreesAdd(float _posX, float _posZ, AltTreesTrees _tree, ref bool _checkTreesAddStop)
        {
            if (bound.inBounds(_posX, _posZ, quadId))
            {
                if (LOD == maxLOD)
                {
                    objs.Add(_tree);
                    _checkTreesAddStop = true;
                }
                else
                {
                    if(!_checkTreesAddStop)
                        quads[0].checkTreesAdd(_posX, _posZ, _tree, ref _checkTreesAddStop);
                    if (!_checkTreesAddStop)
                        quads[1].checkTreesAdd(_posX, _posZ, _tree, ref _checkTreesAddStop);
                    if (!_checkTreesAddStop)
                        quads[2].checkTreesAdd(_posX, _posZ, _tree, ref _checkTreesAddStop);
                    if (!_checkTreesAddStop)
                        quads[3].checkTreesAdd(_posX, _posZ, _tree, ref _checkTreesAddStop);
                }
            }
        }
    }
}


