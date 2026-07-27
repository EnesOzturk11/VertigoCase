using System.Globalization;
using VertigoCase.Config;

namespace VertigoCase.UI
{
    /// <summary>
    /// Converts reward amounts into the compact labels used by the wheel artwork.
    /// Keeping this outside SliceView leaves that component responsible only for binding visuals.
    /// </summary>
    public static class WheelRewardLabelFormatter
    {
        public static string Format(int amount)
        {
            if (amount >= GameConstants.Wheel.CompactMillions)
            {
                return BuildCompactLabel(
                    amount,
                    GameConstants.Wheel.CompactMillions,
                    GameConstants.Wheel.MillionsSuffix);
            }

            if (amount >= GameConstants.Wheel.CompactThousands)
            {
                return BuildCompactLabel(
                    amount,
                    GameConstants.Wheel.CompactThousands,
                    GameConstants.Wheel.ThousandsSuffix);
            }

            return GameConstants.Wheel.RewardLabelPrefix +
                   amount.ToString(CultureInfo.InvariantCulture);
        }

        private static string BuildCompactLabel(int amount, int unit, string suffix)
        {
            float scaledAmount = (float)amount / unit;
            return GameConstants.Wheel.RewardLabelPrefix +
                   scaledAmount.ToString("0.#", CultureInfo.InvariantCulture) +
                   suffix;
        }
    }
}
