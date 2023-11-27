//----------------------
// Developer Mortal
// Date 2023 - 10 - 11 
// Script Overview 数据本地存读处理器
//----------------------
using System;
using UnityEngine;

namespace GameFrame
{
    public static class LocalDataHandler
    {
        private static string _key = "LocalJson_";

        /// <summary>
        /// 获取数据
        /// </summary>
        /// <param name="key">数据路径</param>
        /// <typeparam name="T">要接受的值</typeparam>
        /// <returns>返回的数据</returns>
        /// <exception cref="Exception"></exception>
        public static T GetObject<T>(string key) where T : class
        {
            string json = PlayerPrefs.GetString(_key + key,"null");
            if (json == null) return null;
            return JsonUtility.FromJson<T>(json);
        }

        /// <summary>
        /// 写入数据到本地
        /// </summary>
        /// <param name="key">写入路径</param>
        /// <param name="saveData">数据</param>
        /// <typeparam name="T">Object</typeparam>
        /// <returns>是否写入成功</returns>
        /// <exception cref="Exception"></exception>
        public static void SaveObject<T>(string key, T saveData) where T : class
        {
            PlayerPrefs.SetString(_key+key,JsonUtility.ToJson(saveData));
            PlayerPrefs.Save();
        }

        /// <summary>
        /// 删除本地数据
        /// </summary>
        /// <param name="key">数据路径</param>
        /// <param name="type">数据存储类型</param>
        /// <returns>是否删除成功</returns>
        public static void DeleteObject(string key)
        {
            PlayerPrefs.DeleteKey(_key + key);
        }

        /// <summary>
        /// 是否存在该数据
        /// </summary>
        /// <param name="key">路径</param>
        /// <returns></returns>
        public static bool Exists(string key)
        {
            string rs = PlayerPrefs.GetString(_key + key, "null");
            return rs != "null";
        }
    }
}