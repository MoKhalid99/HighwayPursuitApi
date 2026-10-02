namespace HighwayPursuitApi.DTOs;

public record UploadScoreRequest(string Username, long ScoreValue);

public record LeaderboardEntryDto(int Rank, string Username, long ScoreValue, DateTime AchievedAt);

public record PlayerPersonalBestDto(string Username, long PersonalBestScore, DateTime AchievedAt, int TotalGamesPlayed);