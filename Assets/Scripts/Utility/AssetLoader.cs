using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace Ambition.Utility
{
    /// <summary>
    /// Addressablesの読み込みを簡略化し、存在確認やログ出力を管理するクラス
    /// </summary>
    public static class AssetLoader
    {
        /// <summary>
        /// 指定したアドレス（Key）がAddressablesに存在するかどうかを確認
        /// </summary>
        public static async UniTask<bool> ExistsAsync(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return false;
            }

            AsyncOperationHandle<IList<IResourceLocation>> handle = Addressables.LoadResourceLocationsAsync(key);
            IList<IResourceLocation> locations = await handle.ToUniTask();
            bool isExists = locations != null && locations.Count > 0;

            Addressables.Release(handle);

            return isExists;
        }

        /// <summary>
        /// アセットを読み込み、ハンドルと結果を返す
        /// 読み込み後のハンドル管理（解放）は呼び出し元の責任
        /// </summary>
        /// <typeparam name="T">TextAsset, GameObjectなど</typeparam>
        /// <returns>読み込んだオブジェクト（失敗時はnull）</returns>
        public static async UniTask<(T result, AsyncOperationHandle<T> handle)> LoadAsync<T>(string key) where T : UnityEngine.Object
        {
            Debug.Log($"[AssetLoader] 読み込み開始: {key}");

            bool exists = await ExistsAsync(key);
            if (exists == false)
            {
                Debug.LogWarning($"[AssetLoader] <color=red>指定されたキーが見つかりません</color>: {key}");
                return (null, default(AsyncOperationHandle<T>));
            }

            AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(key);
            T result = await handle.ToUniTask();
            if (handle.Status == AsyncOperationStatus.Succeeded && result != null)
            {
                Debug.Log($"[AssetLoader] <color=green>読み込み完了</color>: {key}");
                return (result, handle);
            }

            Debug.LogError($"[AssetLoader] <color=red>読み込み失敗 (Status: {handle.Status})</color>: {key}");
            Unload(handle, key);
            return (null, default(AsyncOperationHandle<T>));
        }

        /// <summary>
        /// テキストデータ（CSVやJSON）専用の読み込みメソッド
        /// 文字列を取り出した後、内部で即座にハンドルを解放
        /// </summary>
        /// <returns>テキストの中身 (失敗時はnull)</returns>
        public static async UniTask<string> LoadTextDataAndReleaseAsync(string key)
        {
            Debug.Log($"[AssetLoader] テキストデータ読み込み開始: {key}");
            var (asset, handle) = await LoadAsync<TextAsset>(key);
            if (asset != null)
            {
                string textContent = asset.text;
                Unload(handle, key);
                return textContent;
            }

            return null;
        }

        /// <summary>
        /// ハンドルを指定してリソースを解放し、ログを出力
        /// </summary>
        /// <typeparam name="T">アセットの型</typeparam>
        /// <param name="handle">解放するハンドル</param>
        /// <param name="key">ログ出力用のキー名（任意）</param>
        public static void Unload<T>(AsyncOperationHandle<T> handle, string key = "")
        {
            if (handle.IsValid())
            {
                string keyInfo = string.IsNullOrEmpty(key) ? "Unknown Key" : key;
                Debug.Log($"[AssetLoader] リソース解放 (Unload): {keyInfo}");

                Addressables.Release(handle);
            }
            else
            {
                Debug.LogWarning($"[AssetLoader] 無効なハンドルを解放しようとしました: {key}");
            }
        }

        /// <summary>
        /// 指定したラベルが付いた全てのアセットを読み込み
        /// </summary>
        public static async UniTask<(IList<T> results, AsyncOperationHandle<IList<T>> handle)> LoadAssetsByLabelAsync<T>(string label) where T : UnityEngine.Object
        {
            Debug.Log($"[AssetLoader] ラベル読み込み開始: {label}");

            AsyncOperationHandle<IList<T>> handle = Addressables.LoadAssetsAsync<T>(label, null);
            IList<T> results = await handle.ToUniTask();
            if (handle.Status == AsyncOperationStatus.Succeeded && results != null)
            {
                Debug.Log($"[AssetLoader] <color=green>ラベル読み込み完了</color>: {label} ({results.Count}件)");
                return (results, handle);
            }
            else
            {
                Debug.LogError($"[AssetLoader] <color=red>ラベル読み込み失敗</color>: {label}");
                Unload(handle, label);
                return (null, default(AsyncOperationHandle<IList<T>>));
            }
        }

        /// <summary>
        /// 指定したラベルのTextAssetを一括で読み込み、
        /// 「ファイル名」と「テキスト内容」のDictionaryに変換して返します。
        /// 読み込みに使ったメモリは即座に解放されます。
        /// </summary>
        /// <returns>Key: ファイル名, Value: CSVテキスト内容</returns>
        public static async UniTask<Dictionary<string, string>> LoadAllTextDataByLabelAndReleaseAsync(string label)
        {
            var (assets, handle) = await LoadAssetsByLabelAsync<TextAsset>(label);
            if (assets == null)
            {
                return new Dictionary<string, string>();
            }

            Dictionary<string, string> contentMap = new Dictionary<string, string>();
            foreach (var textAsset in assets)
            {
                if (textAsset != null)
                {
                    // textAsset.name はAddressableのアドレス名（ファイル名）
                    contentMap[textAsset.name] = textAsset.text;
                }
            }

            Unload(handle, $"Label: {label}");

            return contentMap;
        }
    }
}
