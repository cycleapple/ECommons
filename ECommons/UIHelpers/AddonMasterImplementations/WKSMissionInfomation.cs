using Dalamud.Memory;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System.Globalization;

namespace ECommons.UIHelpers.AddonMasterImplementations;
public partial class AddonMaster
{
    /// <summary>
    /// Mission information screen
    /// Can be viewed post you grabbing a moon mission
    /// </summary>
    public unsafe partial class WKSMissionInfomation : AddonMasterBase<AtkUnitBase>
    {
        public WKSMissionInfomation(nint addon) : base(addon) { }
        public WKSMissionInfomation(void* addon) : base(addon) { }

        private string? ReadStringValue(int index)
        {
            if (Addon == null || Addon->AtkValues == null || Addon->AtkValuesCount <= index)
                return null;

            var atkValue = Addon->AtkValues[index];
            if (!atkValue.Type.EqualsAny(ValueType.String, ValueType.String8, ValueType.ManagedString, ValueType.WideString))
                return null;

            var value = atkValue.String.Value;
            if (value == null)
                return null;

            return MemoryHelper.ReadSeStringNullTerminated((nint)value)?.GetText();
        }

        private static uint ParseScore(string? rawValue)
        {
            if (string.IsNullOrWhiteSpace(rawValue))
                return 0;

            if (uint.TryParse(rawValue, NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var result))
                return result;

            var cleanedValue = System.Text.RegularExpressions.Regex.Replace(rawValue, @"[^\d]", "");
            return uint.TryParse(cleanedValue, out result) ? result : 0;
        }

        public string Name
        {
            get
            {
                return ReadStringValue(0) ?? string.Empty;
            }
        }

        public uint CurrentScore
        {
            get
            {
                return ParseScore(ReadStringValue(2));
            }
        }

        public uint SilverScore
        {
            get
            {
                return ParseScore(ReadStringValue(3));
            }
        }

        public uint GoldScore
        {
            get
            {
                return ParseScore(ReadStringValue(4));
            }
        }

        public uint CriticalScore
        {
            get
            {
                var rawValue = ReadStringValue(5);
                if (string.IsNullOrWhiteSpace(rawValue))
                    return 0;

                // Extract the left side of the slash
                var leftSide = rawValue.Split('/')[0].Trim();

                // Number conversion test #1.
                if(uint.TryParse(leftSide, NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var result))
                    return result;

                // Fallback: if the first test fails
                var cleanedValue = System.Text.RegularExpressions.Regex.Replace(leftSide, @"[^\d]", "");
                if(uint.TryParse(cleanedValue, out result))
                    return result;

                return 0; // fallback if parsing fails
            }
        }

        public bool TryGetCriticalScore(out uint score)
        {
            var rawValue = ReadStringValue(5);
            if (string.IsNullOrWhiteSpace(rawValue))
            {
                score = 0;
                return false;
            }

            var leftSide = rawValue.Split('/')[0].Trim();
            score = ParseScore(leftSide);
            return true;
        }

        public AtkComponentButton* CosmoPouchButton => Addon->GetComponentButtonById(26);
        public AtkComponentButton* CosmoCraftingLogButton => Addon->GetComponentButtonById(27);
        public AtkComponentButton* StellerReductionButton => Addon->GetComponentButtonById(28);
        public AtkComponentButton* ReportResultsButton => Addon->GetComponentButtonById(29);
        public AtkComponentButton* AbandonMissionButton => Addon->GetComponentButtonById(30);

        public void CosmoPouch() => ClickButtonIfEnabled(CosmoPouchButton);
        public void CosmoCraftingLog() => ClickButtonIfEnabled(CosmoCraftingLogButton);
        public void StellerReduction() => ClickButtonIfEnabled(StellerReductionButton);
        public void Report() => ClickButtonIfEnabled(ReportResultsButton);
        public void Abandon() => ClickButtonIfEnabled(AbandonMissionButton);

        public override string AddonDescription => "Cosmic Exploration Mission Information";
    }
}
