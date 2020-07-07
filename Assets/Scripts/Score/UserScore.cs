namespace Phoenix.Score
{
    public struct UserScore
    {
        public string UserName { get; set; }
        public int Score { get; set; }

        public UserScore(string userName, int score)
        {
            UserName = userName;
            Score = score;
        }

        public override string ToString()
        {
            return $"user: {UserName}, score: {Score}";
        }
    }
}
