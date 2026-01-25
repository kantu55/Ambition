using Ambition.Core.Config;
using Ambition.Core.Constants;
using Ambition.Core.Managers;
using Ambition.Data.Master;
using Ambition.Data.Runtime;
using Cysharp.Threading.Tasks;
using System.IO;
using UnityEngine;
using UnityEngine.Accessibility;
using UnityEngine.SceneManagement;

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
        /// 年月（進行度）
        /// </summary>
        private RuntimeDate date;

        private RuntimeReputation reputation;

        /// <summary>
        /// ゲーム進行度
        /// </summary>
        private int currentTurn;

        /// <summary>
        /// 発生したイベントデータ
        /// </summary>
        public EventModel currentEventData = null;


        // --- プロパティ ---

        public RuntimePlayerStatus Husband => husband;
        public RuntimeWifeStatus Wife => wife;
        public RuntimeEnvironmentStatus Environment => environment;
        public RuntimeHouseholdBudget Budget => budget;
        public RuntimeDate Date => date;
        public RuntimeReputation Reputation => reputation;
        public EventModel CurrentEventData => currentEventData;

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
            int initialMoney = GameSettings.GetInt(SettingKeys.InitialMoney, 1000000);
            this.budget = new RuntimeHouseholdBudget(initialMoney);
            this.date = new RuntimeDate(1, 3);
            this.reputation = new RuntimeReputation();

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

            if (CheckActionRequirements(action, false) == false)
            {
                return false;
            }

            // --- 行動実行フェーズ ---

            // イベント/試合処理
            ProcessMonthlyEvents(action);

            // 当月結果確定
            ConfirmMonthlyResults();

            // パラメータ変動適用
            ApplyParam(action);

            // 状態異常更新
            UpdateStatusAilments();

            // 怪我判定と更新
            UpdateInjuryStatus();

            // 年次処理（必要月のみ）
            ProcessAnnualEvents();

            // 収入確定
            ProcessIncome();

            // 支出処理
            ProcessExpenses();

            // ゲームオーバー判定
            if (CheckGameOver() == false)
            {
                Debug.LogError("ゲームオーバー判定: ゲームを終了します。");
                return false;
            }

            // 妻スキルexp/Lv更新
            UpdateWifeSkill();

            // ターン進行
            ProceedTurn();

            Debug.Log($"行動実行完了: {action.Name} (Main:{action.GetMainCategory()}, Sub:{action.Name})");

            return true;
        }

        /// <summary>
        /// 金銭や体力が足りているかチェック
        /// </summary>
        private bool CheckActionRequirements(WifeActionModel action, bool isRestAction)
        {
            if (budget.CurrentSavings < action.CashCost)
            {
                Debug.LogWarning("資金不足です。");
                return false;
            }

            return true;
        }

        /// <summary>
        /// リソース（お金・体力）を消費
        /// </summary>
        private void ConsumeResources(WifeActionModel action, bool isRestAction)
        {
            if (action.CashCost > 0)
            {
                budget.TrySpend(action.CashCost);
            }
            else if (action.CashCost < 0)
            {
                budget.AddIncome(Mathf.Abs(action.CashCost));
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

            if (action.DeltaHP != 0)
            {
                husband.ChangeHealth(action.DeltaHP);
            }

            if (action.DeltaMP != 0)
            {
                husband.ChangeMental(action.DeltaMP);
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
        }

        /// <summary>
        /// サブカテゴリに応じた特殊なロジックを実行
        /// </summary>
        private void ApplySpecialLogic(WifeActionModel action)
        {
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

            this.date.AdvanceMonth();

            Debug.Log($"ターンが進みました: {this.currentTurn}ターン目");
        }

        private void ProcessMonthlyEvents(WifeActionModel action)
        {
            Debug.Log("--- (3)イベント/試合 ---");

            bool hasScheduledEvent = ProcessMonthlyScheduledEvents();
            if (hasScheduledEvent)
            {
                return;
            }

            EventModel randomEvent = PickRandomEvent();
            if (randomEvent != null)
            {
                this.currentEventData = randomEvent;
                SceneManager.LoadScene("EventScene");
            }
        }

        /// <summary>
        /// ランダムイベントを抽出する
        /// </summary>
        private EventModel PickRandomEvent()
        {
            // todo: DataManagerなどからイベントリストを取得してランダムに抽選するロジックを実装
            return null;
        }

        private bool ProcessMonthlyScheduledEvents()
        {
            int currentMonth = this.date.Month;
            bool isEventOccurred = false;

            switch (currentMonth)
            {
                case 1: // 1月：年棒入金
                    Debug.Log($"<color=yellow>【1月】年俸が入金されました: ¥{husband.Salary:N0}</color>");
                    budget.AddIncome(husband.Salary);
                    isEventOccurred = true;
                    break;

                case 3: // 3月：所得税支払い
                    int incomeTax = budget.FixedCost.CalculateIncomeTax(husband.Salary);
                    Debug.Log($"<color=red>【3月】所得税の支払い: ¥{incomeTax:N0}</color>");
                    if (!budget.TrySpend(incomeTax))
                    {
                        Debug.LogWarning("資金不足で所得税が払えません！借金イベントなどを検討してください。");
                    }

                    isEventOccurred = true;
                    break;

                case 5: // 5月：固定資産税支払い
                    int assetTax = budget.FixedCost.CalculateFixedAssetTax();
                    Debug.Log($"<color=red>【5月】固定資産税の支払い: ¥{assetTax:N0}</color>");
                    if (!budget.TrySpend(assetTax))
                    {
                        Debug.LogWarning("資金不足で固定資産税が払えません！");
                    }

                    isEventOccurred = true;
                    break;

                case 12: // 12月：契約更改
                    Debug.Log("<color=cyan>【12月】契約更改</color>");
                    int newSalary = CalculateNewSalary(husband.TeamEvaluation, husband.Salary);
                    husband.UpdateSalary(newSalary);
                    Debug.Log($"来年の年俸が ¥{newSalary:N0} に決定しました。");
                    isEventOccurred = true;
                    break;

            }

            return isEventOccurred;
        }

        /// <summary>
        /// 契約更改時の新年俸計算ロジック
        /// </summary>
        /// <param name="evaluation"></param>
        /// <param name="currentSalary"></param>
        /// <returns></returns>
        private int CalculateNewSalary(int evaluation, int currentSalary)
        {
            // 評価1につき 1% アップなどのロジック
            float increaseRate = 1.0f + (evaluation * 0.01f);
            return (int)(currentSalary * increaseRate);
        }

        /// <summary>
        /// 当月結果確定
        /// </summary>
        private void ConfirmMonthlyResults()
        {
            Debug.Log("--- (4-A)当月結果確定 ---");
        }

        /// <summary>
        /// パラメータ変動適用
        /// </summary>
        private void ApplyParam(WifeActionModel action)
        {
            Debug.Log("--- (4-B)パラメータ変動適用 ---");
            ConsumeResources(action, false);
            ApplyEffectsToHusband(action);
            ApplyEffectsToWife(action);
            ApplySpecialLogic(action);

        }

        /// <summary>
        /// 状態異常更新
        /// </summary>
        private void UpdateStatusAilments()
        {
            Debug.Log("--- (4-C)状態異常更新 ---");
        }

        /// <summary>
        /// 怪我判定と更新
        /// </summary>
        private void UpdateInjuryStatus()
        {
            Debug.Log("--- (4-D)怪我判定/更新 ---");
        }

        /// <summary>
        /// 年次処理（必要月のみ）
        /// </summary>
        private void ProcessAnnualEvents()
        {
            Debug.Log("--- (4-E)年次処理 ---");
        }

        /// <summary>
        /// 収入確定
        /// </summary>
        private void ProcessIncome()
        {
            Debug.Log("--- (4-F)収入確定 ---");
        }

        /// <summary>
        /// 支出処理
        /// </summary>
        private void ProcessExpenses()
        {
            Debug.Log("--- (4-G)支出確定 ---");
            this.budget.PayMonthlyFixedCosts();
        }

        /// <summary>
        /// ゲームオーバー判定
        /// </summary>
        /// <returns>ゲーム継続可能ならtrue、ゲームオーバーならfalse</returns>
        private bool CheckGameOver()
        {
            Debug.Log("--- (4-H)エンド判定 ---");
            return true;
        }

        /// <summary>
        /// 妻スキルexp/Lv更新
        /// </summary>
        private void UpdateWifeSkill()
        {
            Debug.Log("--- (5)妻スキルexp/Lv更新 ---");
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
                DateData = this.date.ToSaveData(),
                ReputationData = this.reputation.ToSaveData()
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
            this.date = new RuntimeDate(saveData.DateData);
            this.reputation = new RuntimeReputation(saveData.ReputationData);

            Debug.Log("ロード完了。ゲームを再開します。");
            return true;
        }
    }
}
