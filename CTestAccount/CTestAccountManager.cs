using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CTestAccountManager : MonoBehaviour
{
    private const string CTestAccountMainKey = "CTestAccount"; 

    public static CTestAccountMainData LoadAccountMainData()
    {
        string str = PlayerPrefs.GetString(CTestAccountMainKey);

        if (string.IsNullOrEmpty(str))
        {
            return new CTestAccountMainData("CTest");
        }

        CTestAccountSaveData saveData = JsonUtility.FromJson<CTestAccountSaveData>(str);
        
        CTestAccountMainData mainData = new CTestAccountMainData(saveData.accountName);

        string[] dataStrList = saveData.accountDataStr.Split(new char[]{'|'});

        for (int i = 0; i < dataStrList.Length; i++)
        {
            CTestAccountData data = JsonUtility.FromJson<CTestAccountData>(dataStrList[i]);
            if (data != null)
            {
                mainData.accountDataList.Add(data);
            }
        }
        
        return mainData;
    }

    public static void SaveAccountMainData(CTestAccountMainData mainData)
    {
        if (mainData == null)
        {
            return;
        }
        
        string dataStr = "";

        if (mainData.accountDataList != null)
        {
            for (int i = 0; i < mainData.accountDataList.Count; i++)
            {
                dataStr += JsonUtility.ToJson(mainData.accountDataList[i]);
                if (i != mainData.accountDataList.Count - 1)
                {
                    dataStr += "|";
                }
            }
        }
        
        CTestAccountSaveData saveData = new CTestAccountSaveData(mainData.accountName, dataStr);
        
        PlayerPrefs.SetString(CTestAccountMainKey, JsonUtility.ToJson(saveData));
    }
    
    /// <summary>
    /// 注册新账号
    /// </summary>
    public static void RegistNewAccount()
    {
        CTestAccountMainData accountmainData = LoadAccountMainData();

        ClearOldAccount(accountmainData);

        int currentStampTime = GetCurrentTimeStamp();
        
        CTestAccountData accountData = new CTestAccountData(GetNewAccountIndex(accountmainData), currentStampTime,currentStampTime);
        
        accountmainData.accountDataList.Add(accountData);

        SaveAccountMainData(accountmainData);

        CopyAccount(accountmainData.accountName, accountData);
    }

    /// <summary>
    /// 获取并复制最后使用账号
    /// </summary>
    public static void CopyLastLoginAccount()
    {
        CTestAccountMainData accountMainData = LoadAccountMainData();
        
        CTestAccountData accountData = GetLastLoginAccount(accountMainData);
        
        if (accountData != null)
        {
            accountData.lastLoginTime = GetCurrentTimeStamp();
            SaveAccountMainData(accountMainData);
        }
        
        CopyAccount(accountMainData.accountName, accountData);
    }

    /// <summary>
    /// 复制账号
    /// </summary>
    /// <param name="accountName"></param>
    /// <param name="accountData"></param>
    public static void CopyAccount(string accountName, CTestAccountData accountData)
    {
        if (accountData == null)
        {
            return;
        }
        string accountStr = GetAccountStrByData(accountName, accountData);
        TextEditor t = new TextEditor();
        t.text = accountStr;
        t.OnFocus();
        t.Copy();
        Debug.Log("已复制账号：" + accountStr);
    }

    /// <summary>
    /// 获取账号
    /// </summary>
    /// <param name="accountName"></param>
    /// <param name="accountData"></param>
    /// <returns></returns>
    public static string GetAccountStrByData(string accountName, CTestAccountData accountData)
    {
        if (accountData == null)
        {
            return "";
        }
        DateTime dateTime = GetSystemTimeFromStamp(accountData.registTime);
        string accountStr = accountName + "_" + dateTime.Month + "_" + dateTime.Day + "_" + accountData.index;;
        return accountStr;
    }

    /// <summary>
    /// 清理废弃账号
    /// </summary>
    /// <param name="mainData"></param>
    public static void ClearOldAccount(CTestAccountMainData mainData)
    {
        if (mainData != null && mainData.accountDataList != null && mainData.accountDataList.Count != 0)
        {
            int currentStamp = GetCurrentTimeStamp();
            int checkTime = 86400 * 14;

            mainData.accountDataList = mainData.accountDataList.FindAll(x => currentStamp - x.lastLoginTime < checkTime);
        }
    }

    /// <summary>
    /// 获取最后使用账号
    /// </summary>
    /// <returns></returns>
    private static CTestAccountData GetLastLoginAccount(CTestAccountMainData accountMainData)
    {
        if (accountMainData == null)
        {
            return null;
        }

        if (accountMainData.accountDataList == null || accountMainData.accountDataList.Count == 0)
        {
            return null;
        }

        CTestAccountData accountData = accountMainData.accountDataList[0];
        
        accountMainData.accountDataList.ForEach(x =>
        {
            //不能改成大于 防止同一秒点击多次
            if (x.lastLoginTime >= accountData.lastLoginTime)
            {
                accountData = x;
            }
        });
        return accountData;
    }

    /// <summary>
    /// 获取新账号index
    /// </summary>
    /// <param name="mainData"></param>
    /// <returns></returns>
    private static int GetNewAccountIndex(CTestAccountMainData mainData)
    {
        CTestAccountData lastAccount = GetLastLoginAccount(mainData);

        if (lastAccount == null)
        {
            return 1;
        }

        DateTime lastDateTime = GetSystemTimeFromStamp(lastAccount.registTime);

        if (lastDateTime.Year == DateTime.UtcNow.Year && lastDateTime.Month == DateTime.UtcNow.Month && lastDateTime.Day == DateTime.UtcNow.Day)
        {
            return lastAccount.index + 1;
        }
        else
        {
            return 1;
        }
    }

    #region 时间戳相关
    
    /// <summary>
    /// 获取当前时间戳
    /// </summary>
    /// <returns></returns>
    public static int GetCurrentTimeStamp()
    {
        TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
        return (int)ts.TotalSeconds;
    }

    /// <summary>
    /// 系统时间转时间戳
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    private int GetStampTimeFromSystemTime(DateTime time)
    {
        DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
        return (int)(time - startTime).TotalSeconds;
    }

    /// <summary>
    /// 时间戳转系统时间
    /// </summary>
    /// <param name="timeStamp"></param>
    /// <returns></returns>
    public static DateTime GetSystemTimeFromStamp(int timeStamp)
    {
        DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));

        long lTime = long.Parse(timeStamp + "0000000");
        TimeSpan toNow = new TimeSpan(lTime);
        return dtStart.Add(toNow);
    }
    #endregion
}

public class CTestAccountSaveData
{
    public string accountName;
    public string accountDataStr;

    public CTestAccountSaveData(string accountName, string accountDataStr)
    {
        this.accountName = accountName;
        this.accountDataStr = accountDataStr;
    }
}

public class CTestAccountMainData
{
    /// <summary>
    /// 账号前缀
    /// </summary>
    public string accountName;
    /// <summary>
    /// 账号列表
    /// </summary>
    public List<CTestAccountData> accountDataList;

    public CTestAccountMainData(string accountName)
    {
        this.accountName = accountName;
        accountDataList = new List<CTestAccountData>();
    }
}

public class CTestAccountData
{
    /// <summary>
    /// 当日注册序号
    /// </summary>
    public int index;
    /// <summary>
    /// 注册时间
    /// </summary>
    public int registTime;
    /// <summary>
    /// 最后使用时间
    /// </summary>
    public int lastLoginTime;

    public CTestAccountData(int index, int registTime, int lastLoginTime)
    {
        this.index = index;
        this.registTime = registTime;
        this.lastLoginTime = lastLoginTime;
    }
}
