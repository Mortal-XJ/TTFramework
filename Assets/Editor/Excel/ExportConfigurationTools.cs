using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class ExportConfigurationTools 
{
    [MenuItem("GameFrame/ExportExcel")]
    static void ExportExcel()
    {
        Debug.Log("开始导出Excel");
        Debug.Log(Path.Combine(Directory.GetParent(Application.dataPath).ToString(),"gen.bat"));
        var process = new Process();
        process.StartInfo.FileName =Path.Combine(Directory.GetParent(Application.dataPath).ToString(),"gen.bat");
        process.StartInfo.CreateNoWindow = true;        // 不显示命令行窗口
        process.StartInfo.RedirectStandardOutput = true; // 重定向输出，这样你可以从输出中读取
        process.StartInfo.UseShellExecute = false;       // 必须为false，这样我们可以重定向输入/输
        process.Start();

        process.WaitForExit(); // 等待.bat文件执行完毕
        Debug.Log($"Excel导出完成");

    }
}
