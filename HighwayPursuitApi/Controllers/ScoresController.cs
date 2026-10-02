using System.Security.Claims;
using HighwayPursuitApi.Data;
using HighwayPursuitApi.DTOs;
using HighwayPursuitApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HighwayPursuitApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ScoresController : ControllerBase
{
	private readonly GameDbContext _context;

	public ScoresController(GameDbContext context)
	{
		_context = context;
	}

	/// <summary>
	/// login to upload a score for the authenticated player. The player's ID is extracted from the JWT token.
	/// </summary>
	[HttpPost("upload")]
	[Authorize]
	public async Task<IActionResult> UploadScore([FromBody] UploadScoreRequest request)
	{
		if (request.ScoreValue < 0)
		{
			return BadRequest("The score value must be a non-negative number.");
		}

		//For authenticated users, we can get the player ID from the JWT token claims
		var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
		if (!int.TryParse(userIdClaim, out int playerId))
		{
			return Unauthorized("this user is not authorized to upload scores.");
		}

		var newScore = new Score
		{
			PlayerId = playerId,
			ScoreValue = request.ScoreValue,
			AchievedAt = DateTime.UtcNow
		};

		_context.Scores.Add(newScore);
		await _context.SaveChangesAsync();

		return Ok(new
		{
			Message = "Done",	
			Score = newScore.ScoreValue,
			AchievedAt = newScore.AchievedAt
		});
	}

	/// <summary>
	/// Top 8 leaderboard (highest scores across all players). This endpoint is public and does not require authentication.
	/// </summary>
	[HttpGet("top8")]
	public async Task<ActionResult<IEnumerable<LeaderboardEntryDto>>> GetTop8Leaderboard()
	{
		var top8Scores = await _context.Scores
			.AsNoTracking()
			.Include(s => s.Player)
			.GroupBy(s => s.PlayerId)
			.Select(g => g.OrderByDescending(s => s.ScoreValue).First())
			.OrderByDescending(s => s.ScoreValue)
			.Take(8)
			.ToListAsync();

		var leaderboard = top8Scores.Select((s, index) => new LeaderboardEntryDto(
			Rank: index + 1,
			Username: s.Player?.Username ?? "Unknown",
			ScoreValue: s.ScoreValue,
			AchievedAt: s.AchievedAt
		));

		return Ok(leaderboard);
	}

	/// <summary>
	/// (your Best)
	/// </summary>
	[HttpGet("my-best")]
	[Authorize]
	public async Task<ActionResult<PlayerPersonalBestDto>> GetMyBestScore()
	{
		var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
		if (!int.TryParse(userIdClaim, out int playerId))
		{
			return Unauthorized();
		}

		var playerData = await _context.Players
			.AsNoTracking()
			.Where(p => p.Id == playerId)
			.Select(p => new
			{
				p.Username,
				TotalGames = p.Scores.Count,
				BestScore = p.Scores.OrderByDescending(s => s.ScoreValue).FirstOrDefault()
			})
			.FirstOrDefaultAsync();

		if (playerData == null || playerData.BestScore == null)
		{
			return NotFound("Can't find your personal best score or you haven't played any games yet.");
		}

		var result = new PlayerPersonalBestDto(
			Username: playerData.Username,
			PersonalBestScore: playerData.BestScore.ScoreValue,
			AchievedAt: playerData.BestScore.AchievedAt,
			TotalGamesPlayed: playerData.TotalGames
		);

		return Ok(result);
	}

	/// <summary>
	/// Get a player's personal best score by username. This endpoint is public and does not require authentication.
	/// </summary>
	[HttpGet("player/{username}/best")]
	public async Task<ActionResult<PlayerPersonalBestDto>> GetPlayerBest(string username)
	{
		var playerData = await _context.Players
			.AsNoTracking()
			.Where(p => p.Username == username)
			.Select(p => new
			{
				p.Username,
				TotalGames = p.Scores.Count,
				BestScore = p.Scores.OrderByDescending(s => s.ScoreValue).FirstOrDefault()
			})
			.FirstOrDefaultAsync();

		if (playerData == null || playerData.BestScore == null)
		{
			return NotFound($"can't find personal best score for player '{username}' or they haven't played any games yet.");
		}

		var result = new PlayerPersonalBestDto(
			Username: playerData.Username,
			PersonalBestScore: playerData.BestScore.ScoreValue,
			AchievedAt: playerData.BestScore.AchievedAt,
			TotalGamesPlayed: playerData.TotalGames
		);

		return Ok(result);
	}
}