using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.UIElements;

public class CTestAccountEditorUI : MonoBehaviour
{
    [InitializeOnLoadMethod]
    private static void InitializeOnLoad()
    {
        EditorApplication.delayCall += WaitForUnityEditorToolbar;
    }

    private static Type Toolbar =
        typeof(EditorGUI)
            .Assembly
            .GetType("UnityEditor.Toolbar");

    private static FieldInfo Toolbar_get =
        Toolbar
            .GetField("get");

    private static Type GUIView =
        typeof(EditorGUI)
            .Assembly
            .GetType("UnityEditor.GUIView");

    private static PropertyInfo GUIView_imguiContainer =
        GUIView
            .GetProperty(
                "imguiContainer",
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic);

    private static FieldInfo IMGUIContainer_m_OnGUIHandler =
        typeof(IMGUIContainer)
            .GetField(
                "m_OnGUIHandler",
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic);

    private static void WaitForUnityEditorToolbar()
    {
        var toolbar = Toolbar_get.GetValue(null);
        if (toolbar != null)
        {
            AttachToUnityEditorToolbar(toolbar);
            return;
        }

        EditorApplication.delayCall += WaitForUnityEditorToolbar;
    }

    private static void AttachToUnityEditorToolbar(object toolbar)
    {
        var toolbarGUIContainer = (IMGUIContainer) GUIView_imguiContainer.GetValue(toolbar, null);

        var toolbarGUIHandler =
            (Action)
            IMGUIContainer_m_OnGUIHandler
                .GetValue(toolbarGUIContainer);

        toolbarGUIHandler += OnGUI;

        IMGUIContainer_m_OnGUIHandler
            .SetValue(toolbarGUIContainer, toolbarGUIHandler);
    }

    private static void OnGUI()
    {
        var testRect = new Rect(490, 5, 120, 22);
        var testContent = "RegistAccount";
        if (GUI.Button(testRect, testContent, new GUIStyle("button")))
        {
            CTestAccountManager.RegistNewAccount();
        }
        
        testRect.x += testRect.width + 1;
        testContent = "LoginAccount";
        if (GUI.Button(testRect, testContent, new GUIStyle("button")))
        {
            CTestAccountManager.CopyLastLoginAccount();
        }

        testRect.x += testRect.width + 1;
        testContent = "CAccountManager";
        if (GUI.Button(testRect, testContent, new GUIStyle("button")))
        {
            CTestAccountEditorWindow.OpenWindow();
        }
        
        // testRect.x += testRect.width + 1;
        // testRect.width = testRect.width / 2;
        // testContent = "Clear";
        // if (GUI.Button(testRect, testContent, new GUIStyle("button")))
        // {
        //     PlayerPrefs.SetString("CTestAccount", "");
        // }
    }
}
