using UnityEngine;
using System.Threading.Tasks;

public class RewardManager : MonoBehaviour
{
    public static RewardManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// BattleManager等から呼ばれ、敵撃破時やイベントクリア時の報酬付与を処理します。
    /// 将来的にサーバーサイド（Firebase Cloud Functions等）への報酬リクエストへ切り替えるためのカプセル化層です。
    /// </summary>
    public async Task ProcessBattleReward(EnemyData enemyData)
    {
        if (enemyData == null) return;

        int goldReward = enemyData.rewardGold;

        // 1. ローカル／サーバーのデータストア（Firebase）にゴールドを加算
        if (FirebaseManager.Instance != null)
        {
            await FirebaseManager.Instance.AddGold(goldReward);
        }
        else
        {
            Debug.LogWarning("FirebaseManager.Instance is null. Local fallback reward applied.");
        }

        // 2. 将来用のフック：ドロップアイテムや宝箱出現の抽選処理
        // TriggerItemDropLottery(enemyData);

        // 3. 将来用のフック：報酬獲得のUIアニメーションやパネル表示演出のトリガー
        // TriggerRewardUIPanel(goldReward);

        Debug.Log($"[RewardManager] Processed reward for defeating {enemyData.enemyName}: +{goldReward} Gold.");
    }
}
