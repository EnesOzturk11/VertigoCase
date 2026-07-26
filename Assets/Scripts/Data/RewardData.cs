using UnityEngine;
using VertigoCase.Config;

namespace VertigoCase.Data
{
    /// <summary>
    /// Definition (data) of a single reward. It lives as a ".asset" file in the Project,
    /// not in a scene, so it can be created and edited freely from the editor — rewards can be
    /// added or changed without touching code. Even the bomb is not special: it is a
    /// RewardData whose type is Bomb.
    /// </summary>
    [CreateAssetMenu(fileName = "Reward_", menuName = "Vertigo/Reward Data")]
    public class RewardData : ScriptableObject
    {
        [Tooltip("Display name shown in the UI, e.g. 'Cash'")]
        public string displayName;

        [Tooltip("Reward type (Cash/Gold/Chest/Weapon/Bomb)")]
        public RewardType type;

        [Tooltip("Icon shown on the wheel slice")]
        public Sprite icon;

        [Tooltip("Base amount. Will be scaled by zone later.")]
        public int baseAmount = GameConstants.Rewards.DefaultBaseAmount;
    }
}
