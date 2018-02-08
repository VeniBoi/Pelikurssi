using UnityEngine;

namespace AltSystems.AltTrees
{
    public class AltTreesManagerData : ScriptableObject
    {
        //[HideInInspector]
        public AltTreesPatch[] patches = new AltTreesPatch[0];

        [HideInInspector]
        public bool draw = true;
        //[HideInInspector]
        //public bool generateAllBillboardsOnStart = false;
        [HideInInspector]
        public bool enableColliders = true;

        #if (UNITY_5_2 || UNITY_5_3 || UNITY_5_4)
            [HideInInspector]
            public int renderType = 1;
        #endif

        [HideInInspector]
        public float densityObjects = 100;
        


        [HideInInspector]
        public bool shadowsMeshes = true;
        [HideInInspector]
        public bool shadowsBillboards = true;
        [HideInInspector]
        public bool shadowsGroupBillboards = true;

        [HideInInspector]
        public bool autoConfig = true;
        [HideInInspector]
        public int sizePatch = 1000;
        [HideInInspector]
        public int maxLOD = 4;
        [HideInInspector]
        public float distancePatchFactor = 1.5f;
        [HideInInspector]
        public float distanceTreesLODFactor = 1f;
        [HideInInspector]
        public float distanceObjectsLODFactor = 1f;
        [HideInInspector]
        public float checkTreesPerFramePercent = 20f;
        [HideInInspector]
        public float crossFadeTimeBillboard = 0.4f;
        [HideInInspector]
        public float crossFadeTimeMesh = 0.4f;
        
        [HideInInspector]
        public bool crossFadeDependenceOnSpeed = false;
        [HideInInspector]
        public float crossFadeDependenceOnSpeedMaxSpeed = 50f;
        [HideInInspector]
        public float crossFadeDependenceOnSpeedMaxCoefficient = 1f;
        [HideInInspector]
        public bool checkTreesDependenceOnSpeed = false;
        [HideInInspector]
        public float checkTreesDependenceOnSpeedMaxSpeed = 50f;
        [HideInInspector]
        public float checkTreesDependenceOnSpeedMaxCoefficient = 25f;

        [HideInInspector]
        public bool isPlaying = false;

        [HideInInspector]
        public float maxSpeedCameras = 0f;
        

        public float getCrossFadeTimeBillboard()
        {
            if (crossFadeDependenceOnSpeed && isPlaying)
            {
                return crossFadeTimeBillboard + (1f - Mathf.Clamp01(maxSpeedCameras / crossFadeDependenceOnSpeedMaxSpeed)) * crossFadeDependenceOnSpeedMaxCoefficient;
            }
            else
                return crossFadeTimeBillboard;
        }

        public float getCrossFadeTimeMesh()
        {
            if (crossFadeDependenceOnSpeed && isPlaying)
            {
                return crossFadeTimeMesh + (1f - Mathf.Clamp01(maxSpeedCameras / crossFadeDependenceOnSpeedMaxSpeed)) * crossFadeDependenceOnSpeedMaxCoefficient;
            }
            else
                return crossFadeTimeMesh;
        }

        public float getCheckTreesPerFramePercent()
        {
            if (checkTreesDependenceOnSpeed && isPlaying)
            {
                float getCheckTreesPerFramePercentTemp = (maxSpeedCameras / checkTreesDependenceOnSpeedMaxSpeed) * checkTreesDependenceOnSpeedMaxCoefficient;
                if (getCheckTreesPerFramePercentTemp < 1f)
                    getCheckTreesPerFramePercentTemp = 1f;
                else if (getCheckTreesPerFramePercentTemp > checkTreesDependenceOnSpeedMaxCoefficient)
                    getCheckTreesPerFramePercentTemp = checkTreesDependenceOnSpeedMaxCoefficient;

                getCheckTreesPerFramePercentTemp = checkTreesPerFramePercent + getCheckTreesPerFramePercentTemp;
                if (getCheckTreesPerFramePercentTemp < 0f)
                    getCheckTreesPerFramePercentTemp = 0f;
                else if (getCheckTreesPerFramePercentTemp > 100f)
                    getCheckTreesPerFramePercentTemp = 100f;

                return getCheckTreesPerFramePercentTemp;
            }
            else
            {
                float getCheckTreesPerFramePercentTemp = checkTreesPerFramePercent;
                if (getCheckTreesPerFramePercentTemp < 0f)
                    getCheckTreesPerFramePercentTemp = 0f;
                else if (getCheckTreesPerFramePercentTemp > 100f)
                    getCheckTreesPerFramePercentTemp = 100f;

                return getCheckTreesPerFramePercentTemp;
            }
        }

        [HideInInspector]
        public int initCollidersCountPool = 50;
        [HideInInspector]
        public int collidersPerOneMaxPool = 70;
        [HideInInspector]
        public int initColliderBillboardsCountPool = 40;
        [HideInInspector]
        public int colliderBillboardsPerOneMaxPool = 60;


        [HideInInspector]
        public bool drawDebugPatches = false;
        [HideInInspector]
        public bool drawDebugWindow = false;
        [HideInInspector]
        public bool drawDebugWindowInBuilds = false;
        [HideInInspector]
        public bool drawDebugPatchesStar = false;
        [HideInInspector]
        public bool drawDebugBillboards = false;
        [HideInInspector]
        public bool drawDebugBillboardsStar = false;
        [HideInInspector]
        public bool debugLog = true;
        [HideInInspector]
        public bool debugLogInBilds = false;
        [HideInInspector]
        public bool hideMeshes = false;
        [HideInInspector]
        public bool hideBillboards = false;
        [HideInInspector]
        public bool hideGroupBillboards = false;

        [HideInInspector]
        public bool multiThreading = true;

        [HideInInspector]
        public bool stableEditorMode = true;


        [HideInInspector]
        public bool colliderEvents = false;


        [HideInInspector]
        public Shader[] shaders;
    }
}