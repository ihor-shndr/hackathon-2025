using Microsoft.AspNetCore.Mvc;
using MyChat.Models.DTOs.Auth;
using MyChat.Models.DTOs.Common;
using MyChat.Services;

namespace MyChat.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService authService, ILogger<AuthController> logger) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Register(RegisterRequestDto request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                
                return BadRequest(ApiResponse<AuthResponseDto>.ErrorResponse("Validation failed", errors));
            }

            var result = await authService.RegisterAsync(request);
            return Ok(ApiResponse<AuthResponseDto>.SuccessResponse(result, "User registered successfully"));
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "Registration failed for username: {Username}", request.Username);
            return Conflict(ApiResponse<AuthResponseDto>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error during registration for username: {Username}", request.Username);
            return StatusCode(500, ApiResponse<AuthResponseDto>.ErrorResponse("An unexpected error occurred"));
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Login(LoginRequestDto request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                
                return BadRequest(ApiResponse<AuthResponseDto>.ErrorResponse("Validation failed", errors));
            }

            var result = await authService.LoginAsync(request);
            return Ok(ApiResponse<AuthResponseDto>.SuccessResponse(result, "Login successful"));
        }
        catch (UnauthorizedAccessException ex)
        {
            logger.LogWarning(ex, "Login failed for username: {Username}", request.Username);
            return Unauthorized(ApiResponse<AuthResponseDto>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error during login for username: {Username}", request.Username);
            return StatusCode(500, ApiResponse<AuthResponseDto>.ErrorResponse("An unexpected error occurred"));
        }
    }

    [HttpGet("check-username/{username}")]
    public async Task<ActionResult<ApiResponse<bool>>> CheckUsername(string username)
    {
        try
        {
            var exists = await authService.UsernameExistsAsync(username);
            return Ok(ApiResponse<bool>.SuccessResponse(!exists, exists ? "Username is taken" : "Username is available"));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking username availability: {Username}", username);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("An unexpected error occurred"));
        }
    }
}