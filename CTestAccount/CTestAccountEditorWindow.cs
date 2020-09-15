using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CTestAccountEditorWindow : EditorWindow
{
    public static void OpenWindow()
    {
        Rect rect = new Rect(0,0,800,900);
        
        GetWindowWithRect(typeof(CTestAccountEditorWindow), rect, true, "CTestAccount");
    }

    private Vector2 scrollVec2 = Vector2.zero;
    private void OnGUI()
    {
        CTestAccountMainData mainData = CTestAccountManager.LoadAccountMainData();

        bool hasLoad = false;
        string oldName = mainData.accountName;
        
        GUILayout.BeginHorizontal();
        GUILayout.TextArea("用户名");
        mainData.accountName = GUILayout.TextField(mainData.accountName);
        GUILayout.EndHorizontal();
        
        GUILayout.Space(2);
        
        GUILayout.BeginHorizontal();
        GUILayout.TextArea("账号");
        GUILayout.TextArea("注册时间");
        GUILayout.TextArea("最后使用时间");
        GUILayout.TextArea("");
        GUILayout.EndHorizontal();
        
        scrollVec2 = GUILayout.BeginScrollView(scrollVec2);
        GUILayout.BeginVertical(GUILayout.Width(position.width - 20));

        if (mainData.accountDataList != null)
        {
            mainData.accountDataList.Sort((a, b) => -a.lastLoginTime.CompareTo(b.lastLoginTime));
            
            for (int i = 0; i < mainData.accountDataList.Count; i++)
            {
                GUILayout.BeginHorizontal();
                GUILayout.TextArea(CTestAccountManager.GetAccountStrByData(mainData.accountName, mainData.accountDataList[i]));
                GUILayout.TextArea(CTestAccountManager.GetSystemTimeFromStamp(mainData.accountDataList[i].registTime).ToString());
                GUILayout.TextArea(CTestAccountManager.GetSystemTimeFromStamp(mainData.accountDataList[i].lastLoginTime).ToString());
                if (GUILayout.Button("使用账号"))
                {
                    mainData.accountDataList[i].lastLoginTime = CTestAccountManager.GetCurrentTimeStamp();
                    hasLoad = true;
                    CTestAccountManager.CopyAccount(mainData.accountName, mainData.accountDataList[i]);
                }
                
                GUILayout.EndHorizontal();
                GUILayout.Space(1);
            }            
        }

        GUILayout.EndVertical();
        GUILayout.EndScrollView();

        if (hasLoad || oldName != mainData.accountName)
        {
            CTestAccountManager.SaveAccountMainData(mainData);
            if (hasLoad)
            {
                Close();
            }
        }
    }
}
