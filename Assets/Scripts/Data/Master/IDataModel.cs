using Ambition.Utility;

namespace Ambition.Data.Master
{
    /// <summary>
    /// 全てのCSVベースのデータモデルが実装すべきインターフェース
    /// </summary>
    public interface IDataModel
    {
        /// <summary>
        /// データモデルの固有ID
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// CSVデータ（CsvData）の一行分を使用して、モデルのインスタンスを初期化
        /// </summary>
        /// <param name="data">読み込み元の CsvData オブジェクト。</param>
        /// <param name="rowIndex">CSVデータの対象行のインデックス（ヘッダー行を除く）。</param>
        void Initialize(CsvData data, int rowIndex);
    }
}