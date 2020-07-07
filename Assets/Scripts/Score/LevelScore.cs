using System.Collections.Generic;
using System.Linq;

namespace Phoenix.Score
{
    public class LevelScore
    {
        public string LevelName { get; }
        private readonly List<UserScore> _scores;

        public IEnumerable<UserScore> Scores =>
            (from s in _scores orderby s.Score descending select s).ToList().AsReadOnly();

        public LevelScore(string levelName, List<UserScore> scores)
        {
            LevelName = levelName;
            _scores = scores ?? new List<UserScore>();
        }

        public override string ToString()
        {
            return $"Level: {LevelName}, scores: {{{string.Join(", ", _scores)}}}";
        }

        public void AddScore(UserScore userScore)
        {
            _scores.Add(userScore);
        }
    }
}
