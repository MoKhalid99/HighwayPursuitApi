namespace HighwayPursuitApi.Models
{
	public class Score
	{
		public int Id { get; set; }
		public int PlayerId { get; set; }
		public long ScoreValue { get; set; }
		public DateTime AchievedAt { get; set; } = DateTime.UtcNow;

		// Navigation Property
		public Player? Player { get; set; }
	}
}
