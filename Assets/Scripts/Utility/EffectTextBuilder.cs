using System.Text;

namespace Ambition.Utility
{
    public static class EffectTextBuilder
    {
        private static StringBuilder stringBuilder = new StringBuilder(256);
        public static string BuildEffectText(int hp, int mp, int cond, int ability, int love = 0, int publicEye = 0, int teamEval = 0)
        {
            stringBuilder.Clear();
            stringBuilder.Append("【効果】\n");

            bool hasEffect = false;
            if (hp != 0)
            {
                stringBuilder.Append($"体力: {hp}\n");
                hasEffect = true;
            }

            if (mp != 0)
            {
                stringBuilder.Append($"精神 {mp}\n");
                hasEffect = true;
            }

            if (cond != 0)
            {
                stringBuilder.Append($"コンディション {cond}\n");
                hasEffect = true;
            }

            if (ability != 0)
            {
                stringBuilder.Append($"能力 {ability}\n");
                hasEffect = true;
            }

            if (love != 0)
            {
                stringBuilder.Append($"夫婦仲 {love}\n");
                hasEffect = true;
            }

            if (publicEye != 0)
            {
                stringBuilder.Append($"世間の目 {publicEye}\n");
                hasEffect = true;
            }

            if (teamEval != 0)
            {
                stringBuilder.Append($"チーム評価 {teamEval}\n");
                hasEffect = true;
            }

            if (!hasEffect)
            {
                stringBuilder.Append("なし\n");
            }

            return stringBuilder.ToString();
        }

        private static string FormatChangeValue(int value)
        {
            return value > 0 ? $"+{value}" : value.ToString();
        }
    }
}