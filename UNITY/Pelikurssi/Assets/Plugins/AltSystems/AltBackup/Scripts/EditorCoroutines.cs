﻿#if UNITY_EDITOR
using System.Collections;
using UnityEditor;
using UnityEngine;

namespace AltSystems.AltBackup
{
    public class EditorCoroutines
    {
        IEnumerator coroutine;

        EditorCoroutines(IEnumerator _coroutine)
        {
            coroutine = _coroutine;
        }

        public static void Start(IEnumerator _coroutine)
        {
            EditorCoroutines coroutine = new EditorCoroutines(_coroutine);
            coroutine._start();
        }

        void _start()
        {
            EditorApplication.update += _update;
        }

        void _update()
        {
            if (!coroutine.MoveNext())
            {
                EditorApplication.update -= _update;
            }
        }
    }
}
#endif