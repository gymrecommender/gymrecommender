using System;
using System.Collections.Generic;
using backend.Enums;
using Microsoft.EntityFrameworkCore;

namespace backend.Models;

public partial class GymrecommenderContext : DbContext
{
    public GymrecommenderContext()
    {
    }

    public GymrecommenderContext(DbContextOptions<GymrecommenderContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Account> Accounts { get; set; }

    public virtual DbSet<Availability> Availabilities { get; set; }

    public virtual DbSet<Bookmark> Bookmarks { get; set; }

    public virtual DbSet<City> Cities { get; set; }

    public virtual DbSet<CongestionRating> CongestionRatings { get; set; }

    public virtual DbSet<Country> Countries { get; set; }

    public virtual DbSet<Currency> Currencies { get; set; }

    public virtual DbSet<Gym> Gyms { get; set; }

    public virtual DbSet<GymWorkingHour> GymWorkingHours { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Ownership> Ownerships { get; set; }

    public virtual DbSet<Rating> Ratings { get; set; }

    public virtual DbSet<Recommendation> Recommendations { get; set; }

    public virtual DbSet<Request> Requests { get; set; }

    public virtual DbSet<RequestPause> RequestPauses { get; set; }

    public virtual DbSet<RequestPeriod> RequestPeriods { get; set; }

    public virtual DbSet<UserToken> UserTokens { get; set; }

    public virtual DbSet<WorkingHour> WorkingHours { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasPostgresEnum<AccountType>("account_type")
            .HasPostgresEnum<NotificationType>("not_type")
            .HasPostgresEnum<OwnershipDecision>("own_decision")
            .HasPostgresEnum<ProviderType>("provider_type")
            .HasPostgresEnum<RecommendationType>("rec_type")
            .HasPostgresExtension("uuid-ossp");

        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("account_pkey");

            entity.ToTable("account");

            entity.HasIndex(e => e.Email, "account_email_key").IsUnique();

            entity.HasIndex(e => e.OuterUid, "account_outer_uid_key").IsUnique();

            entity.HasIndex(e => e.Username, "account_username_key").IsUnique();

            entity.HasIndex(e => e.Email, "idx_account_email");

            entity.HasIndex(e => e.OuterUid, "idx_account_outer_uid");

            entity.HasIndex(e => e.Username, "idx_account_username");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.Email).HasColumnName("email");
            entity.Property(e => e.FirstName)
                .HasMaxLength(60)
                .HasColumnName("first_name");
            entity.Property(e => e.IsEmailVerified)
                .HasDefaultValue(false)
                .HasColumnName("is_email_verified");
            entity.Property(e => e.LastName)
                .HasMaxLength(60)
                .HasColumnName("last_name");
            entity.Property(e => e.LastSignIn).HasColumnName("last_sign_in");
            entity.Property(e => e.OuterUid)
                .HasMaxLength(128)
                .HasColumnName("outer_uid");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(60)
                .IsFixedLength()
                .HasColumnName("password_hash");
            entity.Property(e => e.Username)
                .HasMaxLength(40)
                .HasColumnName("username");
            entity.Property(e => e.Type)
                .HasColumnName("type");
            entity.Property(e => e.Provider)
                .HasColumnName("provider");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.InverseCreatedByNavigation)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("account_created_by_fkey");
        });

        modelBuilder.Entity<Availability>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("availability_pkey");

            entity.ToTable("availability");

            entity.HasIndex(e => new { e.GymId, e.MarkedBy }, "availability_gym_id_marked_by_key").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.ChangedAt).HasColumnName("changed_at");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.EndTime).HasColumnName("end_time");
            entity.Property(e => e.GymId).HasColumnName("gym_id");
            entity.Property(e => e.MarkedBy).HasColumnName("marked_by");
            entity.Property(e => e.StartTime).HasColumnName("start_time");
        });

        modelBuilder.Entity<Bookmark>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("bookmark_pkey");

            entity.ToTable("bookmark");

            entity.HasIndex(e => new { e.UserId, e.GymId }, "bookmark_user_id_gym_id_key").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.GymId).HasColumnName("gym_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Gym).WithMany(p => p.Bookmarks)
                .HasForeignKey(d => d.GymId)
                .HasConstraintName("bookmark_gym_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.Bookmarks)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("bookmark_user_id_fkey");
        });

        modelBuilder.Entity<City>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("city_pkey");

            entity.ToTable("city");

            entity.HasIndex(e => new { e.Name, e.CountryId }, "idx_city_name_country_id");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.CountryId).HasColumnName("country_id");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.Nelatitude).HasColumnName("nelatitude");
            entity.Property(e => e.Nelongitude).HasColumnName("nelongitude");
            entity.Property(e => e.Swlatitude).HasColumnName("swlatitude");
            entity.Property(e => e.Swlongitude).HasColumnName("swlongitude");

            entity.HasOne(d => d.Country).WithMany(p => p.Cities)
                .HasForeignKey(d => d.CountryId)
                .HasConstraintName("city_country_id_fkey");
        });

        modelBuilder.Entity<CongestionRating>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("congestion_rating_pkey");

            entity.ToTable("congestion_rating");

            entity.HasIndex(e => new { e.GymId, e.UserId }, "congestion_rating_gym_id_user_id_key").IsUnique();

            entity.HasIndex(e => e.GymId, "idx_congestion_rating_gym_id");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.AvgWaitingTime).HasColumnName("avg_waiting_time");
            entity.Property(e => e.ChangedAt).HasColumnName("changed_at");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.Crowdedness).HasColumnName("crowdedness");
            entity.Property(e => e.GymId).HasColumnName("gym_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.VisitTime).HasColumnName("visit_time");
            entity.Property(e => e.Weekday).HasColumnName("weekday");

            entity.HasOne(d => d.Gym).WithMany(p => p.CongestionRatings)
                .HasForeignKey(d => d.GymId)
                .HasConstraintName("congestion_rating_gym_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.CongestionRatings)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("congestion_rating_user_id_fkey");
        });

        modelBuilder.Entity<Country>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("country_pkey");

            entity.ToTable("country");

            entity.HasIndex(e => e.Name, "country_name_key").IsUnique();

            entity.HasIndex(e => e.Name, "idx_country_name");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(56)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Currency>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("currency_pkey");

            entity.ToTable("currency");

            entity.HasIndex(e => e.Code, "currency_code_key").IsUnique();

            entity.HasIndex(e => e.Name, "currency_name_key").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(3)
                .IsFixedLength()
                .HasColumnName("code");
            entity.Property(e => e.Name)
                .HasMaxLength(10)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Gym>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("gym_pkey");

            entity.ToTable("gym");

            entity.HasIndex(e => e.ExternalPlaceId, "gym_external_place_id_key").IsUnique();

            entity.HasIndex(e => e.CityId, "idx_gym_city_id");

            entity.HasIndex(e => e.ExternalPlaceId, "idx_gym_external_place_id");

            entity.HasIndex(e => new { e.Latitude, e.Longitude }, "idx_gym_lat_lon");

            entity.HasIndex(e => e.OwnedBy, "idx_gym_owned_by");

            entity.HasIndex(e => e.PriceChangedAt, "idx_gym_price_changed_at");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.Address).HasColumnName("address");
            entity.Property(e => e.ChangedAt).HasColumnName("changed_at");
            entity.Property(e => e.CityId).HasColumnName("city_id");
            entity.Property(e => e.CongestionRating)
                .HasPrecision(4, 2)
                .HasColumnName("congestion_rating");
            entity.Property(e => e.CongestionRatingNumber).HasColumnName("congestion_rating_number");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.CurrencyId).HasColumnName("currency_id");
            entity.Property(e => e.ExternalPlaceId)
                .HasMaxLength(50)
                .HasColumnName("external_place_id");
            entity.Property(e => e.ExternalRating)
                .HasPrecision(4, 2)
                .HasColumnName("external_rating");
            entity.Property(e => e.ExternalRatingNumber).HasColumnName("external_rating_number");
            entity.Property(e => e.InternalRating)
                .HasPrecision(4, 2)
                .HasColumnName("internal_rating");
            entity.Property(e => e.InternalRatingNumber).HasColumnName("internal_rating_number");
            entity.Property(e => e.IsWheelchairAccessible).HasColumnName("is_wheelchair_accessible");
            entity.Property(e => e.Latitude).HasColumnName("latitude");
            entity.Property(e => e.Longitude).HasColumnName("longitude");
            entity.Property(e => e.MonthlyMprice)
                .HasPrecision(5, 2)
                .HasColumnName("monthly_mprice");
            entity.Property(e => e.Name)
                .HasMaxLength(80)
                .HasColumnName("name");
            entity.Property(e => e.OwnedBy).HasColumnName("owned_by");
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(15)
                .HasColumnName("phone_number");
            entity.Property(e => e.PriceChangedAt).HasColumnName("price_changed_at");
            entity.Property(e => e.SixMonthsMprice)
                .HasPrecision(5, 2)
                .HasColumnName("six_months_mprice");
            entity.Property(e => e.Website)
                .HasMaxLength(255)
                .HasColumnName("website");
            entity.Property(e => e.YearlyMprice)
                .HasPrecision(5, 2)
                .HasColumnName("yearly_mprice");

            entity.HasOne(d => d.City).WithMany(p => p.Gyms)
                .HasForeignKey(d => d.CityId)
                .HasConstraintName("gym_city_id_fkey");

            entity.HasOne(d => d.Currency).WithMany(p => p.Gyms)
                .HasForeignKey(d => d.CurrencyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("gym_currency_id_fkey");

            entity.HasOne(d => d.OwnedByNavigation).WithMany(p => p.Gyms)
                .HasForeignKey(d => d.OwnedBy)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("gym_owned_by_fkey");
        });

        modelBuilder.Entity<GymWorkingHour>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("gym_working_hours_pkey");

            entity.ToTable("gym_working_hours");

            entity.HasIndex(e => new { e.Weekday, e.GymId, e.WorkingHoursId }, "gym_working_hours_weekday_gym_id_working_hours_id_key").IsUnique();

            entity.HasIndex(e => e.GymId, "idx_gym_working_hours_gym_id");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.ChangedAt).HasColumnName("changed_at");
            entity.Property(e => e.GymId).HasColumnName("gym_id");
            entity.Property(e => e.Weekday).HasColumnName("weekday");
            entity.Property(e => e.WorkingHoursId).HasColumnName("working_hours_id");

            entity.HasOne(d => d.Gym).WithMany(p => p.GymWorkingHours)
                .HasForeignKey(d => d.GymId)
                .HasConstraintName("gym_working_hours_gym_id_fkey");

            entity.HasOne(d => d.WorkingHours).WithMany(p => p.GymWorkingHours)
                .HasForeignKey(d => d.WorkingHoursId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("gym_working_hours_working_hours_id_fkey");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("notification_pkey");

            entity.ToTable("notification");

            entity.HasIndex(e => e.UserId, "idx_notification_user_id");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.Message).HasColumnName("message");
            entity.Property(e => e.ReadAt).HasColumnName("read_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Type).HasColumnName("type");

            entity.HasOne(d => d.User).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("notification_user_id_fkey");
        });

        modelBuilder.Entity<Ownership>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("ownership_pkey");

            entity.ToTable("ownership");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.GymId).HasColumnName("gym_id");
            entity.Property(e => e.Message).HasColumnName("message");
            entity.Property(e => e.RequestedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("requested_at");
            entity.Property(e => e.RequestedBy).HasColumnName("requested_by");
            entity.Property(e => e.RespondedAt).HasColumnName("responded_at");
            entity.Property(e => e.RespondedBy).HasColumnName("responded_by");
            entity.Property(e => e.Decision).HasColumnName("decision");

            entity.HasOne(d => d.Gym).WithMany(p => p.Ownerships)
                .HasForeignKey(d => d.GymId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("ownership_gym_id_fkey");

            entity.HasOne(d => d.RequestedByNavigation).WithMany(p => p.OwnershipRequestedByNavigations)
                .HasForeignKey(d => d.RequestedBy)
                .HasConstraintName("ownership_requested_by_fkey");

            entity.HasOne(d => d.RespondedByNavigation).WithMany(p => p.OwnershipRespondedByNavigations)
                .HasForeignKey(d => d.RespondedBy)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("ownership_responded_by_fkey");
        });

        modelBuilder.Entity<Rating>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("rating_pkey");

            entity.ToTable("rating");

            entity.HasIndex(e => e.GymId, "idx_rating_gym_id");

            entity.HasIndex(e => new { e.UserId, e.GymId }, "rating_user_id_gym_id_key").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.ChangedAt).HasColumnName("changed_at");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.GymId).HasColumnName("gym_id");
            entity.Property(e => e.Rating1).HasColumnName("rating");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Gym).WithMany(p => p.Ratings)
                .HasForeignKey(d => d.GymId)
                .HasConstraintName("rating_gym_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.Ratings)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("rating_user_id_fkey");
        });

        modelBuilder.Entity<Recommendation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("recommendation_pkey");

            entity.ToTable("recommendation");

            entity.HasIndex(e => e.RequestId, "idx_recommendation_request_id");

            entity.HasIndex(e => new { e.GymId, e.RequestId }, "recommendation_gym_id_request_id_key").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.CongestionScore)
                .HasPrecision(4, 2)
                .HasColumnName("congestion_score");
            entity.Property(e => e.CurrencyId).HasColumnName("currency_id");
            entity.Property(e => e.GymId).HasColumnName("gym_id");
            entity.Property(e => e.RatingScore)
                .HasPrecision(4, 2)
                .HasColumnName("rating_score");
            entity.Property(e => e.RequestId).HasColumnName("request_id");
            entity.Property(e => e.Tcost)
                .HasPrecision(4, 2)
                .HasColumnName("tcost");
            entity.Property(e => e.TcostScore)
                .HasPrecision(4, 2)
                .HasColumnName("tcost_score");
            entity.Property(e => e.Time).HasColumnName("time");
            entity.Property(e => e.TimeScore)
                .HasPrecision(4, 2)
                .HasColumnName("time_score");
            entity.Property(e => e.TotalScore)
                .HasPrecision(4, 2)
                .HasColumnName("total_score");
            entity.Property(e => e.Type).HasColumnName("type");

            entity.HasOne(d => d.Currency).WithMany(p => p.Recommendations)
                .HasForeignKey(d => d.CurrencyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("recommendation_currency_id_fkey");

            entity.HasOne(d => d.Gym).WithMany(p => p.Recommendations)
                .HasForeignKey(d => d.GymId)
                .HasConstraintName("recommendation_gym_id_fkey");

            entity.HasOne(d => d.Request).WithMany(p => p.Recommendations)
                .HasForeignKey(d => d.RequestId)
                .HasConstraintName("recommendation_request_id_fkey");
        });

        modelBuilder.Entity<Request>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("request_pkey");

            entity.ToTable("request");

            entity.HasIndex(e => e.Name, "idx_request_name");

            entity.HasIndex(e => e.UserId, "idx_request_user_id");

            entity.HasIndex(e => new { e.UserId, e.Name }, "request_user_id_name_key").IsUnique();

            entity.HasIndex(e => new { e.UserId, e.RequestedAt }, "request_user_id_requested_at_key").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.MinCongestionRating)
                .HasPrecision(4, 2)
                .HasColumnName("min_congestion_rating");
            entity.Property(e => e.MinMembershipPrice).HasColumnName("min_membership_price");
            entity.Property(e => e.MinRating)
                .HasPrecision(4, 2)
                .HasColumnName("min_rating");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
            entity.Property(e => e.OriginLatitude).HasColumnName("origin_latitude");
            entity.Property(e => e.OriginLongitude).HasColumnName("origin_longitude");
            entity.Property(e => e.RequestedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("requested_at");
            entity.Property(e => e.TimePriority).HasColumnName("time_priority");
            entity.Property(e => e.TotalCostPriority).HasColumnName("total_cost_priority");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.Requests)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("request_user_id_fkey");
        });

        modelBuilder.Entity<RequestPause>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("request_pause_pkey");

            entity.ToTable("request_pause");

            entity.HasIndex(e => e.Ip, "idx_request_pause_ip");

            entity.HasIndex(e => e.UserId, "idx_request_pause_user_id");

            entity.HasIndex(e => e.Ip, "request_pause_ip_key").IsUnique();

            entity.HasIndex(e => e.UserId, "request_pause_user_id_key").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.Ip).HasColumnName("ip");
            entity.Property(e => e.StartedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("started_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithOne(p => p.RequestPause)
                .HasForeignKey<RequestPause>(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("request_pause_user_id_fkey");
        });

        modelBuilder.Entity<RequestPeriod>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("request_period_pkey");

            entity.ToTable("request_period");

            entity.HasIndex(e => e.RequestId, "idx_request_period_request_id");

            entity.HasIndex(e => new { e.RequestId, e.Weekday }, "request_period_request_id_weekday_key").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.ArrivalTime).HasColumnName("arrival_time");
            entity.Property(e => e.DepartureTime).HasColumnName("departure_time");
            entity.Property(e => e.RequestId).HasColumnName("request_id");
            entity.Property(e => e.Weekday).HasColumnName("weekday");

            entity.HasOne(d => d.Request).WithMany(p => p.RequestPeriods)
                .HasForeignKey(d => d.RequestId)
                .HasConstraintName("request_period_request_id_fkey");
        });

        modelBuilder.Entity<UserToken>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("user_token_pkey");

            entity.ToTable("user_token");

            entity.HasIndex(e => e.UserId, "idx_user_token_user_id");

            entity.HasIndex(e => e.OuterToken, "user_token_outer_token_key").IsUnique();

            entity.HasIndex(e => e.UserId, "user_token_user_id_key").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.ExpiresAt).HasColumnName("expires_at");
            entity.Property(e => e.OuterToken).HasColumnName("outer_token");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithOne(p => p.UserToken)
                .HasForeignKey<UserToken>(d => d.UserId)
                .HasConstraintName("user_token_user_id_fkey");
        });

        modelBuilder.Entity<WorkingHour>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("working_hours_pkey");

            entity.ToTable("working_hours");

            entity.HasIndex(e => new { e.OpenFrom, e.OpenUntil }, "working_hours_open_from_open_until_key").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.OpenFrom).HasColumnName("open_from");
            entity.Property(e => e.OpenUntil).HasColumnName("open_until");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
