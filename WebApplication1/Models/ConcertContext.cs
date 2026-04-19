using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Concert.Models;

public partial class ConcertContext : DbContext
{
    public ConcertContext()
    {
    }

    public ConcertContext(DbContextOptions<ConcertContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Concert> Concerts { get; set; }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<Group> Groups { get; set; }

    public virtual DbSet<Member> Members { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Ticket> Tickets { get; set; }

    public virtual DbSet<Venue> Venues { get; set; }
    public virtual DbSet<Comment> Comments { get; set; }


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=localhost;Database=Concert;Username=postgres;Password=Danchenko2007");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresEnum("member_role", new[] { "Вокаліст", "Гітарист", "Басист", "Барабанщик", "Клавішник" });

        modelBuilder.Entity<Concert>(entity =>
        {
            entity.HasKey(e => e.ConcertId).HasName("concerts_pkey");

            entity.ToTable("concerts");

            entity.Property(e => e.ConcertId).HasColumnName("concert_id");
            entity.Property(e => e.DateTime).HasColumnName("date_time");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");
            entity.Property(e => e.VenueId).HasColumnName("venue_id");

            entity.HasOne(d => d.Venue).WithMany(p => p.Concerts)
                .HasForeignKey(d => d.VenueId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("concerts_venue_id_fkey");

            entity.HasMany(d => d.Groups).WithMany(p => p.Concerts)
                .UsingEntity<Dictionary<string, object>>(
                    "ConcertGroup",
                    r => r.HasOne<Group>().WithMany()
                        .HasForeignKey("GroupId")
                        .HasConstraintName("concert_groups_group_id_fkey"),
                    l => l.HasOne<Concert>().WithMany()
                        .HasForeignKey("ConcertId")
                        .HasConstraintName("concert_groups_concert_id_fkey"),
                    j =>
                    {
                        j.HasKey("ConcertId", "GroupId").HasName("concert_groups_pkey");
                        j.ToTable("concert_groups");
                        j.IndexerProperty<int>("ConcertId").HasColumnName("concert_id");
                        j.IndexerProperty<int>("GroupId").HasColumnName("group_id");
                    });
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.CustomerId).HasName("customers_pkey");

            entity.ToTable("customers");

            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.BirthDate).HasColumnName("birth_date");
            entity.Property(e => e.Email).HasMaxLength(255).HasColumnName("email");
            entity.Property(e => e.FullName)
                .HasMaxLength(255)
                .HasColumnName("full_name");
            entity.Property(e => e.LoyaltyDiscount)
                .HasPrecision(5, 2)
                .HasDefaultValue(0m)
                .HasColumnName("loyalty_discount");
        });
        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasKey(e => e.CommentId);
            entity.ToTable("comments");

            entity.HasOne(d => d.Customer)
                .WithMany() // Це залишається порожнім, бо у Customer поки немає списку коментарів
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.Concert)
                .WithMany(p => p.Comments) // <--- ОСЬ ЦЕ ГОЛОВНЕ! (Додай p => p.Comments)
                .HasForeignKey(d => d.ConcertId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Group>(entity =>
        {
            entity.HasKey(e => e.GroupId).HasName("groups_pkey");

            entity.ToTable("groups");
            entity.Property(e => e.VideoUrl).HasMaxLength(255).HasColumnName("video_url");
            entity.Property(e => e.GroupId).HasColumnName("group_id");
            entity.Property(e => e.BgVideoUrl)
                .HasMaxLength(255)
                .HasColumnName("bg_video_url");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.FanClubUrl)
                .HasMaxLength(255)
                .HasColumnName("fan_club_url");
            entity.Property(e => e.LogoUrl)
                .HasMaxLength(255)
                .HasColumnName("logo_url");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");

            entity.HasMany(d => d.Members).WithMany(p => p.Groups)
                .UsingEntity<Dictionary<string, object>>(
                    "GroupParticipant",
                    r => r.HasOne<Member>().WithMany()
                        .HasForeignKey("MemberId")
                        .HasConstraintName("group_participant_member_id_fkey"),
                    l => l.HasOne<Group>().WithMany()
                        .HasForeignKey("GroupId")
                        .HasConstraintName("group_participant_group_id_fkey"),
                    j =>
                    {
                        j.HasKey("GroupId", "MemberId").HasName("group_participant_pkey");
                        j.ToTable("group_participant");
                        j.IndexerProperty<int>("GroupId").HasColumnName("group_id");
                        j.IndexerProperty<int>("MemberId").HasColumnName("member_id");
                    });
        });

        modelBuilder.Entity<Member>(entity =>
        {
            entity.HasKey(e => e.MemberId).HasName("members_pkey");
            entity.Property(e => e.ImageUrl).HasMaxLength(255).HasColumnName("image_url");
            entity.Property(e => e.InstagramUrl).HasMaxLength(255).HasColumnName("instagram_url");
            entity.ToTable("members");

            entity.Property(e => e.MemberId).HasColumnName("member_id");
            entity.Property(e => e.FullName)
                .HasMaxLength(255)
                .HasColumnName("full_name");
            entity.Property(e => e.Role).HasColumnName("role");

            entity.HasOne(d => d.RoleNavigation).WithMany(p => p.Members)
                .HasForeignKey(d => d.Role)
                .HasConstraintName("fk_member_role");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("roles_pkey");

            entity.ToTable("roles");

            entity.HasIndex(e => e.RoleName, "roles_role_name_key").IsUnique();

            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.RoleName)
                .HasMaxLength(50)
                .HasColumnName("role_name");
        });

        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.HasKey(e => e.TicketId).HasName("tickets_pkey");

            entity.ToTable("tickets");

            entity.Property(e => e.TicketId).HasColumnName("ticket_id");
            entity.Property(e => e.ConcertId).HasColumnName("concert_id");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.Price)
                .HasPrecision(10, 2)
                .HasColumnName("price");
            entity.Property(e => e.RNumber).HasColumnName("r_number");
            entity.Property(e => e.SeatNumber).HasColumnName("seat_number");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasColumnName("status")
                .HasConversion<string>();

            entity.HasOne(d => d.Concert).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.ConcertId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("tickets_concert_id_fkey");

            entity.HasOne(d => d.Customer).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("tickets_customer_id_fkey");
        });

        modelBuilder.Entity<Venue>(entity =>
        {
            entity.HasKey(e => e.VenueId).HasName("venues_pkey");

            entity.ToTable("venues");
            entity.Property(e => e.ImageUrl).HasMaxLength(255).HasColumnName("image_url");
            entity.Property(e => e.VenueId).HasColumnName("venue_id");
            entity.Property(e => e.Address).HasColumnName("address");
            entity.Property(e => e.Capacity).HasColumnName("capacity");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.TotalRows).HasColumnName("total_rows");
            entity.Property(e => e.TotalSeats).HasColumnName("total_seats");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
