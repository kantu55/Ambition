using UnityEngine;
using System.Collections.Generic;
using Ambition.DataStructures;

namespace Ambition.GameCore
{
    /// <summary>
    /// GameSettingsデータを辞書化し、コード内のどこからでも
    /// GameSettings.Get("Key") で値を取得できるようにするヘルパークラス
    /// </summary>
	public static class GameSettings
	{
        /// <summary>
        /// ゲーム設定のキャッシュ
        /// </summary>
		private static Dictionary<string, float> settingsCache;

        /// <summary>
        /// データの初期化・再ロード
        /// </summary>
        public static void Initialize()
        {
            var list = DataManager.Instance.GetDatas<GameSettingModel>();
            settingsCache = new Dictionary<string, float>();

            foreach (var item in list)
            {
                if (settingsCache.ContainsKey(item.Key))
                {
                    continue;
                }

                settingsCache.Add(item.Key, item.Value);
            }
        }

        /// <summary>
        /// float値を取得
        /// </summary>
        public static float GetFloat(string key, float defaultValue = 0f)
        {
            if (settingsCache == null)
            {
                Initialize();
            }

            if (settingsCache.ContainsKey(key))
            {
                return settingsCache[key];
            }

            Debug.LogWarning($"GameSetting Keyが見つかりません: {key}");
            return defaultValue;
        }
        /// <summary>
        /// int値を取得（小数点切り捨て）
        /// </summary>
        public static int GetInt(string key, int defaultValue = 0)
        {
            return (int)GetFloat(key, (float)defaultValue);
        }
    }
}