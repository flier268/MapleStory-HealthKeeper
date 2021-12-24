namespace MapleStory_HealthKeeper.Plugin
{
    public class PreciseDelay
    {
        private readonly AutoResetEvent AutoResetEvent = new(false);

        public PreciseDelay()
        {
            _ = WinMM.timeBeginPeriod(1);
        }

        public void Delay(int delay_ms)
        {
            AutoResetEvent.WaitOne(delay_ms);
        }
    }
}