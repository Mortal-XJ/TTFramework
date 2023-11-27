using System;
using System.IO;
using System.Timers;
using UnityEngine;

namespace GameFrame
{
    public static class Log
    {
        static readonly string _savePath = Path.Combine(Path.GetDirectoryName(Application.dataPath), "Logs");
        private static string _saveTime;
        private static Timer _timerUpdateTime;

        static Log()
        {
            _saveTime = DateTime.Now.ToString("yyyy-MM-dd-HH");
            _timerUpdateTime = new Timer(3600000);
            _timerUpdateTime.Elapsed += OnUpdateTime;
            _timerUpdateTime.Start();
            AppDomain.CurrentDomain.ProcessExit += OnExit;
        }

        private static void OnExit(object sender, EventArgs e)
        {
            _timerUpdateTime.Dispose();
        }

        private static void OnUpdateTime(object sender, ElapsedEventArgs e)
        {
            MainThreadContext.Instance.Push((() => _saveTime = DateTime.Now.ToString("yyyy-MM-dd-HH")));
        }

        public static void Debug(System.Object mes, Color textColor)
        {
#if UNITY_EDITOR
            UnityEngine.Debug.Log($"[<color=#{ColorUtility.ToHtmlStringRGB(textColor)}>{LogType.DEBUG}</color>] {mes}");
            SaveLog(LogType.DEBUG, mes.ToString());
#endif
        }

        public static void Debug(System.Object mes)
        {
            Log.Debug(mes, Color.green);
        }

        public static void Error(System.Object mes, Color textColor)
        {
#if UNITY_EDITOR
            UnityEngine.Debug.LogError(
                $"[<color=#{ColorUtility.ToHtmlStringRGB(textColor)}>{LogType.ERROR}</color>] {mes}");      
#endif
            SaveLog(LogType.ERROR, mes.ToString());
        }

        public static void Error(System.Object mes)
        {
            Log.Error(mes, Color.red);
        }

        public static void Warning(System.Object mes, Color textColor)
        {
#if UNITY_EDITOR
            UnityEngine.Debug.LogWarning(
                $"[<color=#{ColorUtility.ToHtmlStringRGB(textColor)}>{LogType.WARNING}</color>] {mes}");      
#endif
            SaveLog(LogType.ERROR, mes.ToString());
        }

        public static void Warning(System.Object mes)
        {
            Log.Warning(mes, Color.yellow);
        }

        static void SaveLog(LogType logType, string mes)
        {
            string path = Path.Combine(_savePath, $"{logType}{_saveTime}.txt");
            if (!Directory.Exists(_savePath))
                Directory.CreateDirectory(_savePath);
            string logmes = $"[{logType}] [{System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}] {mes}\n";
            File.AppendAllText(path, logmes);
        }

        enum LogType
        {
            DEBUG,
            WARNING,
            ERROR,
        }
    }
}