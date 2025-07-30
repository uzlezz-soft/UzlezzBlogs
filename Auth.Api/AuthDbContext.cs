using Auth.Api.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Auth.Api;

public class AuthDbContext : IdentityDbContext<User>
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options)
        : base(options)
    {
        Database.EnsureCreated();
    }
}
