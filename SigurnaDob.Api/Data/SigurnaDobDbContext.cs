using Microsoft.EntityFrameworkCore;
using SigurnaDob.Shared.Models;

namespace SigurnaDob.Api.Data;

public class SigurnaDobDbContext : DbContext
{
    public SigurnaDobDbContext(DbContextOptions<SigurnaDobDbContext> options) : base(options)
    {
    }

    public DbSet<ResidentStatus> ResidentStatuses => Set<ResidentStatus>();
    public DbSet<RoomStatus> RoomStatuses => Set<RoomStatus>();
    public DbSet<CareTaskType> CareTaskTypes => Set<CareTaskType>();
    public DbSet<CareTaskStatus> CareTaskStatuses => Set<CareTaskStatus>();
    public DbSet<VisitRequestStatus> VisitRequestStatuses => Set<VisitRequestStatus>();
    public DbSet<ActivityType> ActivityTypes => Set<ActivityType>();

    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<Resident> Residents => Set<Resident>();
    public DbSet<Staff> Staff => Set<Staff>();
    public DbSet<FamilyContact> FamilyContacts => Set<FamilyContact>();

    public DbSet<AppUser> AppUsers => Set<AppUser>();
    public DbSet<AppRole> AppRoles => Set<AppRole>();
    public DbSet<AppUserRole> AppUserRoles => Set<AppUserRole>();

    public DbSet<CareTask> CareTasks => Set<CareTask>();
    public DbSet<VisitRequest> VisitRequests => Set<VisitRequest>();

    public DbSet<Activity> Activities => Set<Activity>();
    public DbSet<ActivityParticipant> ActivityParticipants => Set<ActivityParticipant>();

    public DbSet<ResidentDocument> ResidentDocuments => Set<ResidentDocument>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Lookup tabele - unique nazivi
        modelBuilder.Entity<ResidentStatus>().HasIndex(x => x.Name).IsUnique();
        modelBuilder.Entity<RoomStatus>().HasIndex(x => x.Name).IsUnique();
        modelBuilder.Entity<CareTaskType>().HasIndex(x => x.Name).IsUnique();
        modelBuilder.Entity<CareTaskStatus>().HasIndex(x => x.Name).IsUnique();
        modelBuilder.Entity<VisitRequestStatus>().HasIndex(x => x.Name).IsUnique();
        modelBuilder.Entity<ActivityType>().HasIndex(x => x.Name).IsUnique();
        modelBuilder.Entity<AppRole>().HasIndex(x => x.Name).IsUnique();

        // Rooms
        modelBuilder.Entity<Room>().HasIndex(x => x.RoomNumber).IsUnique();
        modelBuilder.Entity<Room>()
            .HasOne(r => r.RoomStatus)
            .WithMany(rs => rs.Rooms)
            .HasForeignKey(r => r.RoomStatusId);

        // Residents
        modelBuilder.Entity<Resident>()
            .HasOne(r => r.ResidentStatus)
            .WithMany(rs => rs.Residents)
            .HasForeignKey(r => r.ResidentStatusId);

        modelBuilder.Entity<Resident>()
            .HasOne(r => r.Room)
            .WithMany(room => room.Residents)
            .HasForeignKey(r => r.RoomId);

        // FamilyContacts
        modelBuilder.Entity<FamilyContact>()
            .HasOne(fc => fc.Resident)
            .WithMany(r => r.FamilyContacts)
            .HasForeignKey(fc => fc.ResidentId);

        // AppUsers - nullable + unique FK prema Staff/FamilyContact (1:0..1)
        modelBuilder.Entity<AppUser>().HasIndex(x => x.Username).IsUnique();
        modelBuilder.Entity<AppUser>().HasIndex(x => x.StaffId).IsUnique();
        modelBuilder.Entity<AppUser>().HasIndex(x => x.FamilyContactId).IsUnique();

        modelBuilder.Entity<AppUser>()
            .HasOne(u => u.Staff)
            .WithOne(s => s.AppUser)
            .HasForeignKey<AppUser>(u => u.StaffId);

        modelBuilder.Entity<AppUser>()
            .HasOne(u => u.FamilyContact)
            .WithOne(fc => fc.AppUser)
            .HasForeignKey<AppUser>(u => u.FamilyContactId);

        // AppUserRoles - spojna tablica, composite PK
        modelBuilder.Entity<AppUserRole>()
            .HasKey(ur => new { ur.AppUserId, ur.AppRoleId });

        modelBuilder.Entity<AppUserRole>()
            .HasOne(ur => ur.AppUser)
            .WithMany(u => u.AppUserRoles)
            .HasForeignKey(ur => ur.AppUserId);

        modelBuilder.Entity<AppUserRole>()
            .HasOne(ur => ur.AppRole)
            .WithMany(r => r.AppUserRoles)
            .HasForeignKey(ur => ur.AppRoleId);

        // CareTasks
        modelBuilder.Entity<CareTask>()
            .HasOne(ct => ct.Resident)
            .WithMany(r => r.CareTasks)
            .HasForeignKey(ct => ct.ResidentId);

        modelBuilder.Entity<CareTask>()
            .HasOne(ct => ct.CareTaskType)
            .WithMany(t => t.CareTasks)
            .HasForeignKey(ct => ct.CareTaskTypeId);

        modelBuilder.Entity<CareTask>()
            .HasOne(ct => ct.CareTaskStatus)
            .WithMany(s => s.CareTasks)
            .HasForeignKey(ct => ct.CareTaskStatusId);

        modelBuilder.Entity<CareTask>()
            .HasOne(ct => ct.AssignedStaff)
            .WithMany(s => s.AssignedCareTasks)
            .HasForeignKey(ct => ct.AssignedStaffId);

        // VisitRequests
        modelBuilder.Entity<VisitRequest>()
            .HasOne(vr => vr.Resident)
            .WithMany(r => r.VisitRequests)
            .HasForeignKey(vr => vr.ResidentId);

        modelBuilder.Entity<VisitRequest>()
            .HasOne(vr => vr.FamilyContact)
            .WithMany(fc => fc.VisitRequests)
            .HasForeignKey(vr => vr.FamilyContactId);

        modelBuilder.Entity<VisitRequest>()
            .HasOne(vr => vr.VisitRequestStatus)
            .WithMany(s => s.VisitRequests)
            .HasForeignKey(vr => vr.VisitRequestStatusId);

        modelBuilder.Entity<VisitRequest>()
            .HasOne(vr => vr.DecidedByStaff)
            .WithMany(s => s.DecidedVisitRequests)
            .HasForeignKey(vr => vr.DecidedByStaffId);

        // Activities
        modelBuilder.Entity<Activity>()
            .HasOne(a => a.ActivityType)
            .WithMany(t => t.Activities)
            .HasForeignKey(a => a.ActivityTypeId);

        // ActivityParticipants - spojna tablica, composite PK
        modelBuilder.Entity<ActivityParticipant>()
            .HasKey(ap => new { ap.ActivityId, ap.ResidentId });

        modelBuilder.Entity<ActivityParticipant>()
            .HasOne(ap => ap.Activity)
            .WithMany(a => a.ActivityParticipants)
            .HasForeignKey(ap => ap.ActivityId);

        modelBuilder.Entity<ActivityParticipant>()
            .HasOne(ap => ap.Resident)
            .WithMany(r => r.ActivityParticipants)
            .HasForeignKey(ap => ap.ResidentId);

        // ResidentDocuments
        modelBuilder.Entity<ResidentDocument>()
            .HasOne(rd => rd.Resident)
            .WithMany(r => r.ResidentDocuments)
            .HasForeignKey(rd => rd.ResidentId);

        // Nema fizičkog brisanja nigdje u modelu -> nijedan FK ne smije cascade-delete roditelja.
        foreach (var foreignKey in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
        {
            foreignKey.DeleteBehavior = DeleteBehavior.Restrict;
        }

        // Seed lookup tablica (fiksni ID-jevi i nazivi iz SPECIFIKACIJA.md)
        modelBuilder.Entity<ResidentStatus>().HasData(
            new ResidentStatus { Id = 1, Name = "U pripremi za prijem" },
            new ResidentStatus { Id = 2, Name = "Aktivan" },
            new ResidentStatus { Id = 3, Name = "Privremeno odsutan" },
            new ResidentStatus { Id = 4, Name = "Premješten iz doma" },
            new ResidentStatus { Id = 5, Name = "Arhiviran" }
        );

        modelBuilder.Entity<RoomStatus>().HasData(
            new RoomStatus { Id = 1, Name = "U uporabi" },
            new RoomStatus { Id = 2, Name = "Održavanje" },
            new RoomStatus { Id = 3, Name = "Izvan uporabe" }
        );

        modelBuilder.Entity<CareTaskType>().HasData(
            new CareTaskType { Id = 1, Name = "Terapija" },
            new CareTaskType { Id = 2, Name = "Prehrana" },
            new CareTaskType { Id = 3, Name = "Higijena" },
            new CareTaskType { Id = 4, Name = "Pratnja" },
            new CareTaskType { Id = 5, Name = "Administrativno" }
        );

        modelBuilder.Entity<CareTaskStatus>().HasData(
            new CareTaskStatus { Id = 1, Name = "Novo" },
            new CareTaskStatus { Id = 2, Name = "Dodijeljeno" },
            new CareTaskStatus { Id = 3, Name = "U tijeku" },
            new CareTaskStatus { Id = 4, Name = "Izvršeno" },
            new CareTaskStatus { Id = 5, Name = "Otkazano" }
        );

        modelBuilder.Entity<VisitRequestStatus>().HasData(
            new VisitRequestStatus { Id = 1, Name = "Zaprimljeno" },
            new VisitRequestStatus { Id = 2, Name = "Odobreno" },
            new VisitRequestStatus { Id = 3, Name = "Odbijeno" },
            new VisitRequestStatus { Id = 4, Name = "Održano" },
            new VisitRequestStatus { Id = 5, Name = "Otkazano" }
        );

        modelBuilder.Entity<ActivityType>().HasData(
            new ActivityType { Id = 1, Name = "Društvena" },
            new ActivityType { Id = 2, Name = "Kreativna" },
            new ActivityType { Id = 3, Name = "Tjelovježba" },
            new ActivityType { Id = 4, Name = "Edukativna" },
            new ActivityType { Id = 5, Name = "Izlet" }
        );

        // Seed demo soba (za ručno testiranje kapaciteta i pravila dodjele prema statusu sobe)
        modelBuilder.Entity<Room>().HasData(
            new Room { Id = 1, RoomNumber = "101", Capacity = 1, RoomStatusId = 1 },
            new Room { Id = 2, RoomNumber = "102", Capacity = 2, RoomStatusId = 1 },
            new Room { Id = 3, RoomNumber = "103", Capacity = 4, RoomStatusId = 1 },
            new Room { Id = 4, RoomNumber = "104", Capacity = 2, RoomStatusId = 2 }
        );

        // Seed uloga
        modelBuilder.Entity<AppRole>().HasData(
            new AppRole { Id = 1, Name = "Admin" },
            new AppRole { Id = 2, Name = "Coordinator" },
            new AppRole { Id = 3, Name = "Caregiver" },
            new AppRole { Id = 4, Name = "FamilyMember" }
        );

        // Seed demo djelatnika (za demo Caregiver račun)
        modelBuilder.Entity<Staff>().HasData(
            new Staff { Id = 1, FullName = "Ivana Njegovateljica", Position = "Njegovateljica", IsActive = true }
        );

        // Seed demo korisničkih računa - lozinke NISU tajna, namjerno su poznate demo vrijednosti:
        // admin/Admin123!, coordinator/Coord123!, caregiver/Caregiver123!, familymember/Family123!
        //
        // VAŽNO: hash je unaprijed izračunat (PasswordHasher<AppUser>.HashPassword) i zalijepljen kao
        // literal, NE poziva se HashPassword ovdje. HashPassword svaki put generira nasumičnu sol pa bi
        // poziv unutar OnModelCreating pri svakom pokretanju aplikacije proizveo drugačiji hash nego onaj
        // zabetoniran u migraciji, što EF Core prijavljuje kao "model se mijenja pri svakom buildu".
        var seedCreatedAt = new DateTime(2026, 9, 11, 0, 0, 0, DateTimeKind.Utc);

        modelBuilder.Entity<AppUser>().HasData(
            new AppUser
            {
                Id = 1,
                Username = "admin",
                PasswordHash = "AQAAAAIAAYagAAAAEFLutaNEinL3QdP2YNWoaL0Qr/tPTAdVIGGkQWYoYdtWxHmHiebIUxFM3ohoxjos1Q==",
                IsActive = true,
                CreatedAt = seedCreatedAt
            },
            new AppUser
            {
                Id = 2,
                Username = "coordinator",
                PasswordHash = "AQAAAAIAAYagAAAAEBJLKQlZj3dSLD7NuQ99SlhqMhe0HpLJdNuq/Hs8gY92z7XYk8OA1irGdOWBAkaUzA==",
                IsActive = true,
                CreatedAt = seedCreatedAt
            },
            new AppUser
            {
                Id = 3,
                Username = "caregiver",
                PasswordHash = "AQAAAAIAAYagAAAAEOwSz/Mzqr6ENjI97rKG0mwoE4sk8cTvIxWnKGBdhQ3Gil76gFMpwule2AkdoIRFPA==",
                IsActive = true,
                StaffId = 1,
                CreatedAt = seedCreatedAt
            },
            new AppUser
            {
                Id = 4,
                Username = "familymember",
                PasswordHash = "AQAAAAIAAYagAAAAELVhZzQ0bK3F1Lz15ptwP/gNojjzJzaj4tTXcuEKlAVQwhHq3Crgsn/2yTToOi4zmQ==",
                IsActive = true,
                // FamilyContactId ostaje null - FamilyContact zahtijeva postojećeg Resident-a (Dan 2)
                CreatedAt = seedCreatedAt
            }
        );

        // Poveži svaki demo račun s njegovom ulogom (1:1 za demo svrhe)
        modelBuilder.Entity<AppUserRole>().HasData(
            new AppUserRole { AppUserId = 1, AppRoleId = 1 },
            new AppUserRole { AppUserId = 2, AppRoleId = 2 },
            new AppUserRole { AppUserId = 3, AppRoleId = 3 },
            new AppUserRole { AppUserId = 4, AppRoleId = 4 }
        );
    }
}
