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
                Debug.Log($"[ID検索成功] 名前: {yamadaById.Name}, 年齢: {yamadaById.Age}, 守備: {yamadaById.Position}, " +
                    $"体力: {yamadaById.Health}, メンタル: {yamadaById.Mental}, 疲労蓄積: {yamadaById.Fatigue}" +
                    $"能力値[筋力]: {yamadaById.Muscle}, 能力値[技術]: {yamadaById.Technique}, 能力値[集中]: {yamadaById.Concentration}," +
                    $"評価: {yamadaById.Evaluation}");
            }
            else
            {
                Debug.LogError($"ID: {targetId} の選手が見つかりません。");
            }
        }
    }
}
