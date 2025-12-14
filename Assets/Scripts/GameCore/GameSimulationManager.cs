using Ambition.DataStructures;
using Ambition.RuntimeData;
using Cysharp.Threading.Tasks;
using System.IO;
using UnityEngine;

namespace Ambition.GameCore
{
    /// <summary>
    /// ゲームの動的な状態（夫、妻、環境、家計、ターン）を一元管理し、
    /// 行動の実行やセーブ・ロードを行う中核クラス
    /// </summary>
    public class GameSimulationManager : MonoBehaviour
    {
        /// <summary>
        /// インスタンス
        /// </summary>
        public static GameSimulationManager Instance { get; private set; }

        // --- 定数 ---

        /// <summary>
        /// セーブファイル名
        /// </summary>
        private const string SAVE_FILE_NAME = "save_data.json";

        // --- メンバ変数 ---

        /// <summary>
        /// 夫
        /// </summary>
        private RuntimePlayerStatus husband;

        /// <summary>
        /// 妻
        /// </summary>
        private RuntimeWifeStatus wife;

        /// <summary>
        /// 環境
        /// </summary>
        private RuntimeEnvironmentStatus environment;

        /// <summary>
        /// 家計
        /// </summary>
        private RuntimeHouseholdBudget budget;

        /// <summary>
        /// ゲーム進行度
        /// </summary>
        private int currentTurn;

        // --- プロパティ ---

        public RuntimePlayerStatus Husband => husband;
        public RuntimeWifeStatus Wife => wife;
        public RuntimeEnvironmentStatus Environment => environment;
        public RuntimeHouseholdBudget Budget => budget;

        /// <summary>
        /// 夫の名前を取得するヘルパープロパティ
        /// </summary>
        public string HusbandName
        {
            get
            {
                if (husband == null)
                {
                    return "";
                }

                var master = DataManager.Instance.GetDataById<PlayerStatsModel>(husband.PlayerId);
                return master != null ? master.Name : "Unknown";
            }
        }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        // --- ゲーム開始 / 初期化 ---

        /// <summary>
        /// ゲーム開始時の初期化処理。
        /// </summary>
        /// <param name="playerId">育成対象の選手ID</param>
        public void StartNewGame(int playerId, int wifeTypeId = 1, int initialHouseId = 101)
        {
            PlayerStatsModel playerMaster = DataManager.Instance.GetDataById<PlayerStatsModel>(playerId);
            if (playerMaster == null)
            {
                Debug.LogError($"ID: {playerId} の選手マスタデータが見つかりません。");
                return;
            }

            WifeStatsModel wifeMaster = DataManager.Instance.GetDataById<WifeStatsModel>(wifeTypeId);
            if (wifeMaster == null)
            {
                Debug.LogError($"ID: {wifeTypeId} の妻のマスタデータが見つかりません。");
                return;
            }

            HousingModel houseMaster = DataManager.Instance.GetDataById<HousingModel>(initialHouseId);
            if (houseMaster == null)
            {
                Debug.LogError($"ID: {initialHouseId} の家のマスタデータが見つかりません。");
                return;
            }

            // 各データの初期化
            this.husband = new RuntimePlayerStatus(playerMaster);
            this.wife = new RuntimeWifeStatus(wifeMaster);
            this.environment = new RuntimeEnvironmentStatus(initialHouseId);
            int initialMoney = GameSettings.GetInt("Initial_Money", 1000000);
            this.budget = new RuntimeHouseholdBudget(initialMoney);

            // --- 固定費の初期計算 ---
            this.budget.FixedCost.UpdateRent(houseMaster.MonthlyRent);

            // MasterDataから年棒が見つからない場合は仮で1500万を設定
            int initialSalary = playerMaster.Salary > 0 ? playerMaster.Salary : 15000000;
            this.budget.FixedCost.UpdateTax(playerMaster.Salary);
            this.budget.FixedCost.RecalculateMaintenance(this.environment);
            this.budget.FixedCost.RecalculateFoodCost(this.environment.MealRank);

            this.currentTurn = 1;

            Debug.Log($"新規ゲーム開始: 選手 {playerMaster.Name} (年俸:{initialSalary:N0}円)");
        }

        // --- 行動ロジック ---

        /// <summary>
        /// 妻の行動を実行
        /// </summary>
        public bool ExecuteWifeAction(WifeActionModel action)
        {
            if (action == null)
            {
                return false;
            }

            bool isRestAction = action.SubCategory == WifeActionModel.ActionSubCategory.WIFE_REST;
            if (CheckActionRequirements(action, isRestAction) == false)
            {
                return false;
            }

            ConsumeResources(action, isRestAction);
            ApplyEffectsToHusband(action);
            ApplyEffectsToWife(action);
            ApplySpecialLogic(action);

            Debug.Log($"行動実行完了: {action.Name} (Main:{action.MainCategory}, Sub:{action.SubCategory})");

            ProceedTurn();

            return true;
        }

        /// <summary>
        /// 金銭や体力が足りているかチェック
        /// </summary>
        private bool CheckActionRequirements(WifeActionModel action, bool isRestAction)
        {
            if (budget.CurrentSavings < action.CostMoney)
            {
                Debug.LogWarning("資金不足です。");
                return false;
            }

            if (isRestAction == false && wife.CurrentHealth < action.CostWifeHealth)
            {
                Debug.LogWarning("妻の体力が不足しています。");
                return false;
            }

            return true;
        }

        /// <summary>
        /// リソース（お金・体力）を消費
        /// </summary>
        private void ConsumeResources(WifeActionModel action, bool isRestAction)
        {
            if (action.CostMoney > 0)
            {
                budget.TrySpend(action.CostMoney);
            }
            else if (action.CostMoney < 0)
            {
                budget.AddIncome(Mathf.Abs(action.CostMoney));
            }

            if (isRestAction == false && action.CostWifeHealth > 0)
            {
                wife.ConsumeHealth(action.CostWifeHealth);
            }
        }

        /// <summary>
        /// 夫への効果を適用
        /// </summary>
        /// <param name="action"></param>
        private void ApplyEffectsToHusband(WifeActionModel action)
        {
            if (husband == null)
            {
                return;
            }

            if (action.HealthChange != 0)
            {
                husband.ChangeHealth(action.HealthChange);
            }

            if (action.MentalChange != 0)
            {
                husband.ChangeMental(action.MentalChange);
            }

            if (action.FatigueChange != 0)
            {
                husband.ChangeFatigue(action.FatigueChange);
            }

            if (action.LoveChange != 0)
            {
                husband.ChangeLove(action.LoveChange);
            }

            if (action.MuscleChange != 0 || action.TechniqueChange != 0 || action.ConcentrationChange != 0)
            {
                husband.GrowAbility(action.MuscleChange, action.TechniqueChange, action.ConcentrationChange);
            }
        }

        /// <summary>
        /// 妻への効果を適用
        /// </summary>
        /// <param name="action"></param>
        private void ApplyEffectsToWife(WifeActionModel action)
        {
            if (wife == null)
            {
                return;
            }

            if (action.StressChange != 0)
            {
                Debug.Log($"妻ストレス変動: {action.StressChange}");
            }
        }

        /// <summary>
        /// サブカテゴリに応じた特殊なロジックを実行
        /// </summary>
        private void ApplySpecialLogic(WifeActionModel action)
        {
            switch (action.SubCategory)
            {
                // --- 妻の休養 ---
                case WifeActionModel.ActionSubCategory.WIFE_REST:
                    int recoverAmount = wife.MaxHealth / 2;
                    wife.RecoverHealth(recoverAmount);
                    Debug.Log($"完全休養: 体力{recoverAmount}回復");
                    break;

                // --- 叱る ---
                case WifeActionModel.ActionSubCategory.SCOLD:

                    break;

                // --- 設備投資 (家具購入) ---
                case WifeActionModel.ActionSubCategory.INVESTMENT:
                    // 何を強化するかは、ActionIDや別パラメータで判断する必要がある
                    // 仮実装: SkillExpの値を「強化対象ID」として扱うなどの運用ルールを決める
                    UpgradeEnvironment(action.SkillExp);
                    break;

                // --- 拠点変更 (引っ越し) ---
                case WifeActionModel.ActionSubCategory.MOVE_BASE:
                    // 引っ越し処理
                    // 例: ActionID 3031 = マンションへ引っ越し
                    // 新しい家のIDを特定して移動させる
                    int newHouseId = action.SkillExp; // 仮: SkillExp列を家IDとして利用
                    ChangeHouse(newHouseId);
                    break;

                // --- スポンサー要請 ---
                case WifeActionModel.ActionSubCategory.REQUEST_SPONSOR:
                    // 妻の「社交スキル」や「営業スキル」に応じて成功判定
                    /*
                    if (wife.SocialLevel >= 3) {
                         budget.AddIncome(1000000); // 成功報酬
                         Debug.Log("スポンサー獲得成功！");
                    } else {
                         Debug.Log("スポンサー獲得失敗...");
                    }
                    */
                    break;

                default:
                    break;
            }
        }

        private void UpgradeEnvironment(int targetType)
        {
            budget.FixedCost.RecalculateMaintenance(environment);
            Debug.Log("設備をアップグレードしました。維持費が再計算されます。");
        }

        private void ChangeHouse(int newHouseId)
        {
            environment.MoveHouse(newHouseId);
            HousingModel houseData = DataManager.Instance.GetDataById<HousingModel>(newHouseId);
            if (houseData != null)
            {
                budget.FixedCost.UpdateRent(houseData.MonthlyRent);
                Debug.Log($"引っ越し完了: {houseData.HouseName} (家賃: {houseData.MonthlyRent:N0}円)");
            }
        }

        /// <summary>
        /// ターンを進める
        /// </summary>
        private void ProceedTurn()
        {
            this.currentTurn++;

            int recoverAmount = GameSettings.GetInt("Turn_Health_Recover", 30);
            this.wife.RecoverHealth(recoverAmount);

            Debug.Log("--- 月末処理: 固定費支払い ---");
            this.budget.PayMonthlyFixedCosts();

            Debug.Log($"ターンが進みました: {this.currentTurn}ターン目");
        }

        /// <summary>
        /// 食事ランクを変更し、固定費を再計算
        /// </summary>
        public void ChangeMealRank(int newRank)
        {
            this.environment.ChangeMealRank(newRank);
            this.budget.FixedCost.RecalculateFoodCost(newRank);
            Debug.Log($"食事ランクを Lv.{newRank} に変更しました。食費: {this.budget.FixedCost.FoodCost:N0}円");
        }

        /// <summary>
        /// セーブデータが存在するか
        /// </summary>
        public bool HasSaveData()
        {
            string path = Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);
            return File.Exists(path);
        }

        /// <summary>
        /// 現在のゲーム状態を非同期でファイルに保存
        /// </summary>
        public async UniTask<bool> SaveGameAsync()
        {
            if (this.husband == null && this.wife == null)
            {
                Debug.LogError("セーブ失敗: データが初期化されていません。");
                return false;
            }

            // セーブデータオブジェクトの作成
            GameSaveData saveData = new GameSaveData
            {
                // ここに作成したデータを保存していく
                CurrentTurn = this.currentTurn,
                PlayerData = this.husband.ToSaveData(),
                WifeData = this.wife.ToSaveData(),
                EnvironmentData = this.environment.ToSaveData(),
                BudgetData = this.budget.ToSaveData(),
            };

            string json = JsonUtility.ToJson(saveData, prettyPrint: true);
            string path = Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);
            await UniTask.RunOnThreadPool(() => File.WriteAllText(path, json));

            Debug.Log($"セーブ完了: {path}");
            return true;
        }

        /// <summary>
        /// セーブデータを非同期でロードし、ゲーム状態を復元します。
        /// </summary>
        public async UniTask<bool> LoadGameAsync()
        {
            string path = Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);
            if (!File.Exists(path))
            {
                Debug.LogWarning("セーブデータが見つかりません。");
                return false;
            }

            string json = await UniTask.RunOnThreadPool(() => File.ReadAllText(path));
            GameSaveData saveData = JsonUtility.FromJson<GameSaveData>(json);
            if (saveData == null)
            {
                Debug.LogError("データの読み込みに失敗しました（データ破損の可能性）。");
                return false;
            }

            // 各モデルの再構築・復元
            this.currentTurn = saveData.CurrentTurn;
            this.husband = new RuntimePlayerStatus(saveData.PlayerData);
            this.wife = new RuntimeWifeStatus(saveData.WifeData);
            this.environment = new RuntimeEnvironmentStatus(saveData.EnvironmentData);
            this.budget = new RuntimeHouseholdBudget(saveData.BudgetData);

            Debug.Log("ロード完了。ゲームを再開します。");
            return true;
        }
    }
}
