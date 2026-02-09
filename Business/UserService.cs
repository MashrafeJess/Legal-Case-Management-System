using Database.Context;
using Database.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Business
{
    public class UserService
    {
        private readonly LMSContext context = new();

        public Result Registration(User user)
        {
            if (user == null) return new Result(false, "User is null");
            if (string.IsNullOrWhiteSpace(user.Email)) return new Result(false, "Email is required");
            if (string.IsNullOrWhiteSpace(user.Password)) return new Result(false, "Password is required");

            user.Email = user.Email.Trim();

            bool exists = context.User.Any(u => u.Email == user.Email);
            if (exists)
                return new Result(false, "An email already exists");

            // Default role
            if (user.RoleId == 0) user.RoleId = 3;

            // ✅ Correct hasher type
            user.Password = new PasswordHasher<User>().HashPassword(user, user.Password);

            // Optional: default flags
            user.IsDeleted = false;

            context.User.Add(user);
            return Result.DBcommit(context, "Registration Successful", "Failed to register user", user);
        }

        public Result Login(User user)
        {
            if (user == null) return new Result(false, "User is null");
            if (string.IsNullOrWhiteSpace(user.Email)) return new Result(false, "Email is required");
            if (string.IsNullOrWhiteSpace(user.Password)) return new Result(false, "Password is required");

            user.Email = user.Email.Trim();

            var userInfo = context.User.FirstOrDefault(u => u.Email == user.Email && !u.IsDeleted);
            if (userInfo == null)
                return new Result(false, "No such email exists");

            // ✅ Verify against stored hash
            var verify = new PasswordHasher<User>()
                .VerifyHashedPassword(userInfo, userInfo.Password, user.Password);

            // ✅ Correct condition: failed means incorrect
            if (verify == PasswordVerificationResult.Failed)
                return new Result(false, "Incorrect password");

            // ✅ Return the DB userInfo (with UserId, RoleId, etc.)
            return new Result(true, "Logged in successfully", userInfo);
        }

        public Result Update(User user)
        {
            if (user == null) return new Result(false, "User is null");
            if (string.IsNullOrWhiteSpace(user.Email)) return new Result(false, "Email is required");

            user.Email = user.Email.Trim();

            var user1 = context.User.FirstOrDefault(u => u.Email == user.Email && !u.IsDeleted);
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

            context.User.Update(user1);
            return Result.DBcommit(context, "User info updated successfully", "Failed to update user", user1);
        }

        public Result Delete(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return new Result(false, "UserId is required");

            var u = context.User.Find(userId);
            if (u == null)
                return new Result(false, "User not found");

            u.IsDeleted = true;
            context.User.Update(u);

            return Result.DBcommit(context, "User is deleted", "Failed to delete user", null);
        }

        public Result UserById(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return new Result(false, "UserId is required");

            var user = context.User.Find(userId);
            if (user == null || user.IsDeleted)
                return new Result(false, "This user is not found");

            return new Result(true, "The user is found", user);
        }

        public Result AllUsers()
        {
            var list = context.User.Where(u => !u.IsDeleted).ToList();

            return list.Count > 0
                ? new Result(true, "All users are fetched", list)
                : new Result(false, "No user found", list);
        }
        public Result GetAllLawyers()
        {
            var list = context.User.Where(u => u.RoleId == 2).ToList();
            return new Result(true, "All lawyers fetched");
        }

    }
}
