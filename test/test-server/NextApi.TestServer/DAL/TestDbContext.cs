using System;
using Microsoft.EntityFrameworkCore;
using NextApi.Common.Abstractions.Security;
using NextApi.Server.UploadQueue.DAL;
using NextApi.TestServer.Model;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

namespace NextApi.TestServer.DAL
{
    public interface ITestDbContext : IUploadQueueDbContext
    {
        DbSet<TestCity> Cities { get; set; }
        DbSet<TestRole> Roles { get; set; }
        DbSet<TestUser> Users { get; set; }
        DbSet<TestTreeItem> TestTreeItems { get; set; }
    }

    public class TestDbContext : UploadQueueDbContext, ITestDbContext
    {
        public DbSet<TestCity> Cities { get; set; }
        public DbSet<TestRole> Roles { get; set; }
        public DbSet<TestUser> Users { get; set; }
        public DbSet<TestTreeItem> TestTreeItems { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.HasCharSet(CharSet.Utf8Mb4);
            builder.Entity<TestUser>(e =>
            {
                e.HasOne(u => u.Role)
                    .WithMany()
                    .HasForeignKey(u => u.RoleId);
                e.HasOne(u => u.City)
                    .WithMany()
                    .HasForeignKey(u => u.CityId);
            });
            builder.Entity<TestTreeItem>(e =>
            {
                e.HasOne(t => t.Parent)
                    .WithMany(tp => tp.Children)
                    .HasForeignKey(t => t.ParentId)
                    .IsRequired(false);
            });
            builder.Entity<TestCity>().Property(t => t.Id).HasColumnType("binary(16)");
            builder.Entity<TestUser>().HasData(new TestUser
            {
                Id = 1,
                Name = "TestUserName",
                Surname = "TestUserSurname",
                Email = "TestUserEmail",
                Enabled = true,
                DecimalProperty = decimal.One,
                Birthday =  DateTime.Parse("2000-10-30T19:30:00"),
                BirthdayAsOffset = DateTimeOffset.Parse("2000-10-30T19:30:00+00:00")
            });
        }

        public TestDbContext(DbContextOptions options, INextApiUserAccessor nextApiUserAccessor) : base(options,
            nextApiUserAccessor)
        {
        }
    }
}
