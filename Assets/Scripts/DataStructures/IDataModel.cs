using Ambition.GameCore;

namespace Ambition.DataStructures
{
    /// <summary>
    /// 全てのCSVベースのデータモデルが実装すべきインターフェースを定義します。
    /// </summary>
    public interface IDataModel
    {
        /// <summary>
        /// データモデルの固有IDを取得します。
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// CSVデータ（CsvData）の一行分を使用して、モデルのインスタンスを初期化します。
        /// </summary>
        /// <param name="data">読み込み元の CsvData オブジェクト。</param>
        /// <param name="rowIndex">CSVデータの対象行のインデックス（ヘッダー行を除く）。</param>
        void Initialize(CsvData data, int rowIndex);
    }
}