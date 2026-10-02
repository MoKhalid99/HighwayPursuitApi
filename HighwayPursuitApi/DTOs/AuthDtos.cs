namespace HighwayPursuitApi.DTOs;

public record RegisterRequestDto(string Username, string Password);

public record LoginRequestDto(string Username, string Password);

public record AuthResponseDto(string Token, string Username, DateTime ExpiresAt);