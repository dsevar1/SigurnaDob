using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SigurnaDob.Api.Data;
using SigurnaDob.Shared.DTOs.Auth;
using SigurnaDob.Shared.Models;

namespace SigurnaDob.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly SigurnaDobDbContext _db;
    private readonly IPasswordHasher<AppUser> _passwordHasher;
    private readonly IConfiguration _configuration;

    public AuthController(SigurnaDobDbContext db, IPasswordHasher<AppUser> passwordHasher, IConfiguration configuration)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _configuration = configuration;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDto>> Login(LoginRequestDto request)
    {
        var user = await _db.AppUsers
            .Include(u => u.AppUserRoles)
                .ThenInclude(ur => ur.AppRole)
            .SingleOrDefaultAsync(u => u.Username == request.Username);

        if (user is null || !user.IsActive)
        {
            return Unauthorized();
        }

        var verificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (verificationResult == PasswordVerificationResult.Failed)
        {
            return Unauthorized();
        }

        var roles = user.AppUserRoles.Select(ur => ur.AppRole!.Name).ToList();
        var (token, expiresAt) = BuildToken(user, roles);

        return Ok(new LoginResponseDto
        {
            Token = token,
            ExpiresAt = expiresAt,
            Username = user.Username,
            Roles = roles,
            StaffId = user.StaffId,
            FamilyContactId = user.FamilyContactId
        });
    }

    private (string Token, DateTime ExpiresAt) BuildToken(AppUser user, List<string> roles)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username)
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        if (user.StaffId is not null)
        {
            claims.Add(new Claim("staff_id", user.StaffId.Value.ToString()));
        }

        if (user.FamilyContactId is not null)
        {
            claims.Add(new Claim("family_contact_id", user.FamilyContactId.Value.ToString()));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiryMinutes = _configuration.GetValue<int>("Jwt:ExpiryMinutes");
        var expiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }
}
