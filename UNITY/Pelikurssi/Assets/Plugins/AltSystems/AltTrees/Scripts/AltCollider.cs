using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AltSystems.AltTrees
{
    public class AltCollider : MonoBehaviour
    {
        [HideInInspector]
        public AltTreesPatch patch;
        [HideInInspector]
        public int idTree;
        [HideInInspector]
        public bool isObject = true;
        [HideInInspector]
        public int quadID;
        [HideInInspector]
        public int idPrototype;
        

        void OnCollisionEnter(Collision collisionInfo)
        {
            AltTreesTrees attTemp = null;

            if (!isObject)
            {
                if (patch.trees.Length > idTree && idTree >= 0)
                {
                    attTemp = patch.trees[idTree];
                }
                else
                    patch.altTrees.LogError("patch.trees.Length <= idTree || idTree < 0");
            }
            else
            {
                if (patch.quadObjects.Length > quadID - 1 && idTree >= 0)
                {
                    attTemp = patch.quadObjects[quadID - 1].findObjectById(idTree);
                }
                else
                    patch.altTrees.LogError("patch.quadObjects.Length <= quadID - 1 || idTree < 0");
            }

            if (attTemp != null && attTemp.noNull)
            {
                AltCollisionInfo collision = new AltCollisionInfo(new AltTreesInfo(attTemp.altTreesPatch, attTemp.idTree, attTemp.isObject, (attTemp.isObject ? attTemp.idQuadObject : attTemp.altTreesId), attTemp.idPrototype, attTemp.getPosWorld(), attTemp.color, attTemp.colorBark, attTemp.rotation, attTemp.heightScale, attTemp.widthScale), collisionInfo);

                patch.altTrees.SendMessage("AltCollisionEnter", collision, SendMessageOptions.DontRequireReceiver);
            }
            else
                patch.altTrees.LogError("attTemp == null || !attTemp.noNull");
        }

        void OnCollisionExit(Collision collisionInfo)
        {
            AltTreesTrees attTemp = null;

            if (!isObject)
            {
                if (patch.trees.Length > idTree && idTree >= 0)
                {
                    attTemp = patch.trees[idTree];
                }
                else
                    patch.altTrees.LogError("patch.trees.Length <= idTree || idTree < 0");
            }
            else
            {
                if (patch.quadObjects.Length > quadID - 1 && idTree >= 0)
                {
                    attTemp = patch.quadObjects[quadID - 1].findObjectById(idTree);
                }
                else
                    patch.altTrees.LogError("patch.quadObjects.Length <= quadID - 1 || idTree < 0");
            }

            if (attTemp != null && attTemp.noNull)
            {
                AltCollisionInfo collision = new AltCollisionInfo(new AltTreesInfo(attTemp.altTreesPatch, attTemp.idTree, attTemp.isObject, (attTemp.isObject ? attTemp.idQuadObject : attTemp.altTreesId), attTemp.idPrototype, attTemp.getPosWorld(), attTemp.color, attTemp.colorBark, attTemp.rotation, attTemp.heightScale, attTemp.widthScale), collisionInfo);

                patch.altTrees.SendMessage("AltCollisionExit", collision, SendMessageOptions.DontRequireReceiver);
            }
            else
                patch.altTrees.LogError("attTemp == null || !attTemp.noNull");
        }

        void OnCollisionStay(Collision collisionInfo)
        {
            AltTreesTrees attTemp = null;

            if (!isObject)
            {
                if (patch.trees.Length > idTree && idTree >= 0)
                {
                    attTemp = patch.trees[idTree];
                }
                else
                    patch.altTrees.LogError("patch.trees.Length <= idTree || idTree < 0");
            }
            else
            {
                if (patch.quadObjects.Length > quadID - 1 && idTree >= 0)
                {
                    attTemp = patch.quadObjects[quadID - 1].findObjectById(idTree);
                }
                else
                    patch.altTrees.LogError("patch.quadObjects.Length <= quadID - 1 || idTree < 0");
            }

            if (attTemp != null && attTemp.noNull)
            {
                AltCollisionInfo collision = new AltCollisionInfo(new AltTreesInfo(attTemp.altTreesPatch, attTemp.idTree, attTemp.isObject, (attTemp.isObject ? attTemp.idQuadObject : attTemp.altTreesId), attTemp.idPrototype, attTemp.getPosWorld(), attTemp.color, attTemp.colorBark, attTemp.rotation, attTemp.heightScale, attTemp.widthScale), collisionInfo);

                patch.altTrees.SendMessage("AltCollisionStay", collision, SendMessageOptions.DontRequireReceiver);
            }
            else
                patch.altTrees.LogError("attTemp == null || !attTemp.noNull");
        }

        public AltCollisionInfo getCollisionInfo()
        {
            AltTreesTrees attTemp = null;

            if (!isObject)
            {
                if (patch.trees.Length > idTree && idTree >= 0)
                {
                    attTemp = patch.trees[idTree];
                }
                else
                {
                    patch.altTrees.LogError("patch.trees.Length <= idTree || idTree < 0");
                    return null;
                }
            }
            else
            {
                if (patch.quadObjects.Length > quadID - 1 && idTree >= 0)
                {
                    attTemp = patch.quadObjects[quadID - 1].findObjectById(idTree);
                }
                else
                {
                    patch.altTrees.LogError("patch.quadObjects.Length <= quadID - 1 || idTree < 0");
                    return null;
                }
            }

            if (attTemp != null && attTemp.noNull)
            {
                return new AltCollisionInfo(new AltTreesInfo(attTemp.altTreesPatch, attTemp.idTree, attTemp.isObject, (attTemp.isObject ? attTemp.idQuadObject : attTemp.altTreesId), attTemp.idPrototype, attTemp.getPosWorld(), attTemp.color, attTemp.colorBark, attTemp.rotation, attTemp.heightScale, attTemp.widthScale), null);
            }
            else
            {
                patch.altTrees.LogError("attTemp == null || !attTemp.noNull");
                return null;
            }
        }
    }

    public class AltCollisionInfo
    {
        public AltTreesInfo treeInfo;
        public Collision collisionInfo;

        public AltCollisionInfo(AltTreesInfo _treeInfo, Collision _collisionInfo)
        {
            treeInfo = _treeInfo;
            collisionInfo = _collisionInfo;
        }
    }
}