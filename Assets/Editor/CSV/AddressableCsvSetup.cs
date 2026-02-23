using System.IO;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

public static class AddressableCsvSetup
{
    private const string MENU_PATH = "Addressables/Setup CSVs in Target Folder";
    private const string TARGET_FOLDER_PATH = "Assets/GameAssets/MasterData/Csv";
    private const string TARGET_EXTENSION = ".csv";
    private const string TARGET_LABEL = "MasterData";
    private const string ERROR_SETTINGS_NOT_FOUND = "AddressableAssetSettingsが見つかりません。Addressablesの初期化が完了しているか確認してください。";
    private const string ERROR_FOLDER_NOT_FOUND = "対象のフォルダが見つかりません: ";

    [MenuItem(MENU_PATH)]
    public static void SetupCsvsInFolder()
    {
        AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;

        if (settings == null)
        {
            Debug.LogError(ERROR_SETTINGS_NOT_FOUND);
            return;
        }

        if (!AssetDatabase.IsValidFolder(TARGET_FOLDER_PATH))
        {
            Debug.LogError(ERROR_FOLDER_NOT_FOUND + TARGET_FOLDER_PATH);
            return;
        }

        // 指定フォルダ内のTextAssetを検索
        string[] guids = AssetDatabase.FindAssets("t:TextAsset", new[] { TARGET_FOLDER_PATH });
        bool isModified = false;

        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            string extension = Path.GetExtension(assetPath);

            if (extension.ToLower() != TARGET_EXTENSION)
            {
                continue;
            }

            AddressableAssetEntry entry = settings.FindAssetEntry(guid);

            // Addressablesに登録されていない場合は、デフォルトグループに追加
            if (entry == null)
            {
                AddressableAssetGroup defaultGroup = settings.DefaultGroup;
                entry = settings.CreateOrMoveEntry(guid, defaultGroup, false, false);
            }

            if (entry != null)
            {
                // アドレスを拡張子なしのファイル名に変更
                string fileName = Path.GetFileNameWithoutExtension(assetPath);
                entry.address = fileName;

                // ラベルを追加（第2引数: 有効化、第3引数: 設定に存在しない場合は追加）
                entry.SetLabel(TARGET_LABEL, true, true);

                isModified = true;
            }
        }

        // 変更があった場合のみ保存処理を行う
        if (isModified)
        {
            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();
            Debug.Log($"{TARGET_FOLDER_PATH} 内のCSVファイルのAddressable設定を更新しました。");
        }
    }
}
