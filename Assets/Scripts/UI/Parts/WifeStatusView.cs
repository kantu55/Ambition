using Ambition.Data.Runtime;
using System.Text;
using TMPro;
using UnityEngine;

namespace Ambition.UI.MainGame.Parts
{
    /// <summary>
    /// 妻のステータス表示を管理するビューコンポーネント
    /// </summary>
    public class WifeStatusView : MonoBehaviour
    {
        [Header("Wife UI")]
        [SerializeField] private TextMeshProUGUI wifeCookingLevelText;
        [SerializeField] private TextMeshProUGUI wifeCareLevelText;
        [SerializeField] private TextMeshProUGUI wifePRLevelText;
        [SerializeField] private TextMeshProUGUI wifeCaochLevelText;

        // 文字列生成時のGC Allocを避けるためのStringBuilder
        private StringBuilder stringBuilder = new StringBuilder(512);

        /// <summary>
        /// 妻のステータス情報を更新
        /// </summary>
        /// <param name="wife">妻のステータス</param>
        public void UpdateWifeInfo(RuntimeWifeStatus wife)
        {
            if (wife == null)
            {
                return;
            }

            // スキル一覧
            wifeCookingLevelText.SetText(GetWifeSkillText("料理: Lv", wife.CookingLevel));
            wifeCareLevelText.SetText(GetWifeSkillText("ケア: Lv", wife.CareLevel));
            wifePRLevelText.SetText(GetWifeSkillText("PR: Lv", wife.PRLevel));
            wifeCaochLevelText.SetText(GetWifeSkillText("コーチ: Lv", wife.CoachLevel));
        }

        /// <summary>
        /// スキルテキストを生成
        /// </summary>
        /// <param name="titleName">スキル名のプレフィックス</param>
        /// <param name="level">レベル</param>
        /// <returns>整形されたスキルテキスト</returns>
        private string GetWifeSkillText(string titleName, int level)
        {
            stringBuilder.Clear();
            stringBuilder.Append(titleName).Append(level);
            return stringBuilder.ToString();
        }
    }
}