using CoreApp.Application.DTOs.Auth;
using CoreApp.Application.Interfaces.Auth;
using CoreApp.Domain.Entities;
using CoreApp.Infrastructure.Auth;
using CoreApp.Infrastructure.Data;
using CoreApp.Infrastructure.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CoreApp.Infrastructure.Services;

public class AuthService(CoreAppDbContext context, IOptions<JwtSettings> jwtOptions) : IAuthService
{
    private readonly CoreAppDbContext _context = context;
    private readonly JwtSettings _jwtSettings = jwtOptions.Value;

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            throw new Exception("User already exists");

        var user = new User
        {
            Email = request.Email,
            Username = request.Username,
            PasswordHash = PasswordHasher.Hash(request.Password),
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return await GenerateAuthResponseAsync(user);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user == null || !PasswordHasher.Verify(request.Password, user.PasswordHash))
            throw new Exception("Invalid credentials");

        return await GenerateAuthResponseAsync(user);
    }

    public async Task<AuthResponse> RefreshTokenAsync(string refreshToken)
    {
        var token = await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken && !rt.IsRevoked);

        if (token == null || token.Expires < DateTime.UtcNow)
            throw new Exception("Invalid or expired refresh token");

        token.IsRevoked = true;
        await _context.SaveChangesAsync();

        return await GenerateAuthResponseAsync(token.User!);
    }

    private async Task<AuthResponse> GenerateAuthResponseAsync(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_jwtSettings.SecretKey);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Email, user.Email)
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);

        var refresh = new RefreshToken
        {
            Token = Guid.NewGuid().ToString(),
            Expires = DateTime.UtcNow.AddDays(7),
            UserId = user.Id
        };

        _context.RefreshTokens.Add(refresh);
        await _context.SaveChangesAsync();

        return new AuthResponse
        {
            Token = tokenHandler.WriteToken(token),
            RefreshToken = refresh.Token,
            ExpiresAt = tokenDescriptor.Expires!.Value
        };
    }
}
