namespace ScriptMaster.Core
{
    public interface IFullKeyInfo
    {
        public Keys Key { get; set; }

        public int ScanCode { get; set; }

        public bool IsExtented { get; set; }
    }
}