using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Business.DTO.AuthDto;
using Database.Context;
using Database.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Business.Services
{
    public class UserService(LMSContext context, IConfiguration config, ILogger<UserService> logger)
    {
        private readonly LMSContext _context = context;
        private readonly IConfiguration _config = config;
        private readonly ILogger<UserService> _logger = logger;

        public async Task<Result> RegisterAsync(RegistrationDto user)
        {
            if (user == null) return new Result(false, "User is null");
            if (string.IsNullOrWhiteSpace(user.Email)) return new Result(false, "Email is required");
            if (string.IsNullOrWhiteSpace(user.Password)) return new Result(false, "Password is required");

            user.Email = user.Email.Trim();

            bool exists = await _context.User.AnyAsync(u => u.Email == user.Email);
            if (exists)
                return new Result(false, "An email already exists");

            // Default role
            if (user.RoleId == 0) user.RoleId = 3;

            var entity = new User
            {
                UserName = user.UserName,
                Email = user.Email,
                Password = new PasswordHasher<RegistrationDto>().HashPassword(user, user.Password),
                RoleId = user.RoleId == 0 ? 3 : user.RoleId
            };

            _context.User.Add(entity);
            var result = await Result.DBCommitAsync(_context, "Registration Successful", _logger, "Failed to save", entity);
            if (!result.Success)
            {
                return result;
            }

            entity = await _context.User
                   .Include(u => u.Role)  // join Role table
                   .FirstOrDefaultAsync(u => u.RoleId == user.RoleId);

            if (entity == null)
                return new Result(false, "Could not retrieve user after registration");

            string token = GenerateJwt(entity);

            return new Result(true, "Registration successful", new LoginResponseDto
            {
                UserId = entity.UserId,
                Token = token,
                UserName = entity.UserName,
                Email = entity.Email,
                Role = entity.Role?.RoleName ?? "Guest"
            });
        }

        public async Task<Result> Login(LoginRequestDto user)
        {
            if (user == null) return new Result(false, "User is null");
            if (string.IsNullOrWhiteSpace(user.Email)) return new Result(false, "Email is required");
            if (string.IsNullOrWhiteSpace(user.Password)) return new Result(false, "Password is required");

            user.Email = user.Email.Trim();

            var userInfo = await _context.User.FirstOrDefaultAsync(u => u.Email == user.Email && !u.IsDeleted);
            if (userInfo == null)
                return new Result(false, "No such email exists");

            var verify = new PasswordHasher<User>()
                .VerifyHashedPassword(userInfo, userInfo.Password, user.Password);

            if (verify == PasswordVerificationResult.Failed)
                return new Result(false, "Incorrect password");

            User? entity = await _context.User
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == user.Email && !u.IsDeleted);
            if (entity == null) return new Result(false, "User not found after verification");

            return new Result(true, "Logged in successfully", new LoginResponseDto
            {
                UserId = entity.UserId,
                Token = GenerateJwt(entity),
                UserName = entity.UserName,
                Email = entity.Email,
                Role = entity.Role?.RoleName ?? "User"
            });
        }

        private string GenerateJwt(User user)
        {
            var claims = new[]
            {
        new Claim(ClaimTypes.NameIdentifier, user.UserId),
        new Claim(ClaimTypes.Name, user.UserName),
        new Claim(ClaimTypes.Email, user.Email),
        new Claim(ClaimTypes.Role, user?.Role?.RoleName ?? "Guest")
    };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<Result> Update(UpdateDto user, string updatedBy)
        {
            if (user == null) return new Result(false, "User is null");
            if (string.IsNullOrWhiteSpace(user.Email)) return new Result(false, "Email is required");

            user.Email = user.Email.Trim();

            var user1 = await _context.User
                .Include(x => x.Role)
                .FirstOrDefaultAsync(u => u.Email == user.Email && !u.IsDeleted);

            if (user1 == null)
                return new Result(false, "No user found");

            // ✅ Update fields safely (don’t do: user1 = user)
            user1.UserName = user.UserName;
            user1.RoleId = user.RoleId == 0 ? user1.RoleId : user.RoleId;

            // Only hash if a new password is provided
            if (!string.IsNullOrWhiteSpace(user.Password))
            {
                user1.Password = new PasswordHasher<User>().HashPassword(user1, user.Password);
            }
            user1.UpdatedDate = DateTime.UtcNow;
            user1.UpdatedBy = updatedBy;
            _context.User.Update(user1);
            var result = await Result.DBCommitAsync(_context, "User info updated successfully", null, "Failed to update user", user1);
            if (!result.Success)
            {
                return new Result(false, "Update failed");
            }
            return new Result(true, "update successful", new LoginResponseDto
            {
                UserId = user1.UserId,
                UserName = user1.UserName,
                Email = user1.Email,
                Role = user1?.Role?.RoleName ?? "User",
            });
        }

        public async Task<Result> Delete(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return new Result(false, "UserId is required");

            var u = _context.User.Find(userId);
            if (u == null)
                return new Result(false, "User not found");

            u.IsDeleted = true;
            _context.User.Update(u);

            return await Result.DBCommitAsync(_context, "User is deleted", null, "Failed to delete user", null);
        }

        public async Task<Result> UserById(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return new Result(false, "UserId is required");

            var user = await _context.User.Where(u => u.UserId == userId && !u.IsDeleted)
                .AsNoTracking()
                .Include(u => u.Role)
                .FirstOrDefaultAsync();

            if (user?.IsDeleted != false)
                return new Result(false, "This user is not found");

            return new Result(true, "The user is found", new LoginResponseDto
            {
                UserId = user.UserId,
                UserName = user.UserName,
                Email = user.Email,
                Role = user?.Role?.RoleName ?? "User"
            });
        }

        public async Task<Result> AllUsers()
        {
            var list = await _context.User
    .Where(u => !u.IsDeleted)
    .Select(u => new LoginResponseDto
    {
        UserId = u.UserId,
        UserName = u.UserName,
        Email = u.Email,
        Role = u.Role!.RoleName
    })
    .AsNoTracking()
    .ToListAsync();

            return list.Count > 0
                ? new Result(true, "All users are fetched", list)
                : new Result(false, "No user found", list);
        }

        public async Task<Result> GetAllLawyers()
        {
            var list = await _context.User.Where(u => u.RoleId == 2)
                .Select(u => new User
                {
                    UserId = u.UserId,
                    UserName = u.UserName,
                    Email = u.Email,
                    RoleId = u.RoleId
                })
                .AsNoTracking()
                .Include(u => u.Role)
                .ToListAsync();
            if (list.Count > 0)
            {
                return new Result(true, "All lawyers fetched", list);
            }
            return new Result(false, "No Lawyers found");
        }
    }
}