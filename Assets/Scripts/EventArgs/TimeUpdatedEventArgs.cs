namespace EventArgs
{
    public class TimeUpdatedEventArgs
    {
        public float Time { get; }
        public float TotalTime { get; }

        public TimeUpdatedEventArgs(float time, float totalTime)
        {
            Time = time;
            TotalTime = totalTime;
        }
    }
}
