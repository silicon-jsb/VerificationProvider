
using Microsoft.EntityFrameworkCore;
using VerificationProvider.Data.Entities;

namespace VerificationProvider.Data.Contexts;

public class DataContext(DbContextOptions options) : DbContext(options)
{
	public DbSet<VerificationRequestEntity> VerificationRequests { get; set; }
}
