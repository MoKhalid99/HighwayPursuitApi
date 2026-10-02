namespace HighwayPursuitApi.Models
{
	public class Player
	{
		public int Id { get; set; }
		public string Username { get; set; } = string.Empty;
		public string PasswordHash { get; set; } = string.Empty;
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

		// Navigation Property
		public ICollection<Score> Scores { get; set; } = new List<Score>();
	}

}

