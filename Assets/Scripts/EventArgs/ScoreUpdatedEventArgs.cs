namespace EventArgs
{
    public class ScoreUpdatedEventArgs
    {
        public int NewScore { get; }
        public int OldScore { get; }

        public ScoreUpdatedEventArgs(int newScore, int oldScore)
        {
            NewScore = newScore;
            OldScore = oldScore;
        }
    }
}
