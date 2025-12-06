using Ambition.DataStructures;
using Ambition.GameCore;
using UnityEngine;

namespace Ambition.DebugTools
{
    /// <summary>
    /// データ読み込みのテストを実行するクラス。
    /// </summary>
    public class DataTestRunner : MonoBehaviour
    {
        private void Start()
        {
            int targetId = 1001;
            PlayerStatsModel yamadaById = DataManager.Instance.GetDataById<PlayerStatsModel>(targetId);

            if (yamadaById != null)
            {
                Debug.Log($"[ID検索成功] 名前: {yamadaById.Name}, 守備: {yamadaById.Position}, パワー: {yamadaById.Power}");
            }
            else
            {
                Debug.LogError($"ID: {targetId} の選手が見つかりません。");
            }
        }
    }
}
