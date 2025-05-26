using Microsoft.AspNetCore.Mvc;
using PhotoBoardApi.Data;
using PhotoBoardApi.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace PhotoBoardApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public UserController(ApplicationDbContext context)
    {
        _context = context;
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

    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
}