using System.ComponentModel;
using MapleStory_HealthKeeper.Converter;

namespace MapleStory_HealthKeeper.Helper
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum HealMode
    {
        /// <summary>
        /// 百分比
        /// </summary>
        [Description("%")]
        Percentage,

        /// <summary>
        /// 精確數值
        /// </summary>
        [Description("滴")]
        ExactValue
    }
}