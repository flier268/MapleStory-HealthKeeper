namespace MapleStory_HealthKeeper.Model
{
    internal struct HealthPointer
    {
        internal IntPtr Hook { get; set; }
        internal IntPtr Hp { get; set; }
        internal IntPtr MaxHp { get; set; }
        internal IntPtr Mp { get; set; }
        internal IntPtr MaxMp { get; set; }
        internal IntPtr Exp { get; set; }
        internal IntPtr MaxExp { get; set; }
    }
}