using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HighwayPursuitApi.Data;
using HighwayPursuitApi.DTOs;
using HighwayPursuitApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace HighwayPursuitApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
	private readonly GameDbContext _context;
	private readonly IConfiguration _config;
	private readonly PasswordHasher<Player> _passwordHasher = new();

	public AuthController(GameDbContext context, IConfiguration config)
	{
		_context = context;
		_config = config;
	}

	[HttpPost("register")]
	public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
	{
		if (await _context.Players.AnyAsync(p => p.Username == request.Username))
		{
			return BadRequest("The username is already taken. Please choose a different one.");
		}

		var player = new Player
		{
			Username = request.Username
		};

		// Hash the password before storing it in the database
		player.PasswordHash = _passwordHasher.HashPassword(player, request.Password);

		_context.Players.Add(player);
		await _context.SaveChangesAsync();

		var tokenResponse = GenerateJwtToken(player);
		return Ok(tokenResponse);
	}

	[HttpPost("login")]
	public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
	{
		var player = await _context.Players
			.FirstOrDefaultAsync(p => p.Username == request.Username);

		if (player == null)
		{
			return Unauthorized("the username or password is incorrect.");
		}

		// is the password correct?
		var result = _passwordHasher.VerifyHashedPassword(player, player.PasswordHash, request.Password);
		if (result == PasswordVerificationResult.Failed)
		{
			return Unauthorized("the password is incorrect.");
		}

		var tokenResponse = GenerateJwtToken(player);
		return Ok(tokenResponse);
	}

	private AuthResponseDto GenerateJwtToken(Player player)
	{
		var jwtSettings = _config.GetSection("JwtSettings");
		var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Secret"]!));
		var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

		var claims = new[]
		{
			new Claim(ClaimTypes.NameIdentifier, player.Id.ToString()),
			new Claim(ClaimTypes.Name, player.Username)
		};

		var expiresAt = DateTime.UtcNow.AddDays(double.Parse(jwtSettings["ExpiryDays"]!));

		var token = new JwtSecurityToken(
			issuer: jwtSettings["Issuer"],
			audience: jwtSettings["Audience"],
			claims: claims,
			expires: expiresAt,
			signingCredentials: creds
		);

		var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

		return new AuthResponseDto(tokenString, player.Username, expiresAt);
	}
}