using Microsoft.AspNetCore.Mvc;
using PhotoBoardApi.Data;
using PhotoBoardApi.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;      // JwtSecurityToken, JwtSecurityTokenHandler
using Microsoft.IdentityModel.Tokens;       // SymmetricSecurityKey, SigningCredentials, SecurityAlgorithms

namespace PhotoBoardApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public UserController(ApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterUserDto dto)
    {
        if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
        {
            return BadRequest("이미 사용 중인 이메일입니다.");
        }

        var hashedPassword = HashPassword(dto.Password);

        var user = new User
        {
            Email = dto.Email,
            PasswordHash = hashedPassword,
            DisplayName = dto.DisplayName
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return Ok("회원가입 성공");
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginUserDto dto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);

        if (user == null || user.PasswordHash != HashPassword(dto.Password))
        {
            return Unauthorized("이메일 또는 비밀번호가 잘못되었습니다.");
        }

        var jwtSettings = _configuration.GetSection("Jwt");

        var claims = new[]
        {
        new Claim(JwtRegisteredClaimNames.Sub, user.Email),
        new Claim("id", user.Id.ToString()),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expires = DateTime.UtcNow.AddMinutes(Convert.ToDouble(jwtSettings["ExpiresInMinutes"]));

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: expires,
            signingCredentials: creds
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        return Ok(new { token = tokenString });
    }


    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
}