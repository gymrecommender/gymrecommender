using System;
using Microsoft.EntityFrameworkCore.Migrations;
using backend.Enums;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:account_type.account_type", "user,gym,admin")
                .Annotation("Npgsql:Enum:not_type.notification_type", "message,alert,reminder")
                .Annotation("Npgsql:Enum:own_decision.ownership_decision", "approved,rejected")
                .Annotation("Npgsql:Enum:provider_type.provider_type", "local,google")
                .Annotation("Npgsql:Enum:rec_type.recommendation_type", "main,alternative")
                .Annotation("Npgsql:PostgresExtension:uuid-ossp", ",,");

            migrationBuilder.CreateTable(
                name: "account",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    username = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    outer_uid = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    is_email_verified = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    first_name = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    last_name = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    last_sign_in = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    password_hash = table.Column<string>(type: "character(60)", fixedLength: true, maxLength: 60, nullable: false),
                    type = table.Column<AccountType>(type: "account_type", nullable: false),
                    provider = table.Column<ProviderType>(type: "provider_type", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("account_pkey", x => x.id);
                    table.ForeignKey(
                        name: "account_created_by_fkey",
                        column: x => x.created_by,
                        principalTable: "account",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "availability",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    start_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    end_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    changed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    gym_id = table.Column<Guid>(type: "uuid", nullable: false),
                    marked_by = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("availability_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "country",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    name = table.Column<string>(type: "character varying(56)", maxLength: 56, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("country_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "currency",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    name = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    code = table.Column<string>(type: "character(3)", fixedLength: true, maxLength: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("currency_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "working_hours",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    open_from = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    open_until = table.Column<TimeOnly>(type: "time without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("working_hours_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "notification",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    message = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    read_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    type = table.Column<NotificationType>(type: "notification_type", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("notification_pkey", x => x.id);
                    table.ForeignKey(
                        name: "notification_user_id_fkey",
                        column: x => x.user_id,
                        principalTable: "account",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "request",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    requested_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    origin_latitude = table.Column<double>(type: "double precision", nullable: false),
                    origin_longitude = table.Column<double>(type: "double precision", nullable: false),
                    time_priority = table.Column<int>(type: "integer", nullable: false),
                    total_cost_priority = table.Column<int>(type: "integer", nullable: false),
                    min_congestion_rating = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: false),
                    min_rating = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: false),
                    min_membership_price = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("request_pkey", x => x.id);
                    table.ForeignKey(
                        name: "request_user_id_fkey",
                        column: x => x.user_id,
                        principalTable: "account",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "request_pause",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    ip = table.Column<byte[]>(type: "bytea", nullable: true),
                    started_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("request_pause_pkey", x => x.id);
                    table.ForeignKey(
                        name: "request_pause_user_id_fkey",
                        column: x => x.user_id,
                        principalTable: "account",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_token",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    outer_token = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "now()"),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("user_token_pkey", x => x.id);
                    table.ForeignKey(
                        name: "user_token_user_id_fkey",
                        column: x => x.user_id,
                        principalTable: "account",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "city",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    nelatitude = table.Column<double>(type: "double precision", nullable: false),
                    nelongitude = table.Column<double>(type: "double precision", nullable: false),
                    swlatitude = table.Column<double>(type: "double precision", nullable: false),
                    swlongitude = table.Column<double>(type: "double precision", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    country_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("city_pkey", x => x.id);
                    table.ForeignKey(
                        name: "city_country_id_fkey",
                        column: x => x.country_id,
                        principalTable: "country",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "request_period",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    request_id = table.Column<Guid>(type: "uuid", nullable: false),
                    weekday = table.Column<int>(type: "integer", nullable: false),
                    arrival_time = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    departure_time = table.Column<TimeOnly>(type: "time without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("request_period_pkey", x => x.id);
                    table.ForeignKey(
                        name: "request_period_request_id_fkey",
                        column: x => x.request_id,
                        principalTable: "request",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "gym",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    latitude = table.Column<double>(type: "double precision", nullable: false),
                    longitude = table.Column<double>(type: "double precision", nullable: false),
                    name = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    external_place_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    external_rating = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: false),
                    external_rating_number = table.Column<int>(type: "integer", nullable: false),
                    phone_number = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true),
                    address = table.Column<string>(type: "text", nullable: false),
                    website = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    is_wheelchair_accessible = table.Column<bool>(type: "boolean", nullable: false),
                    monthly_mprice = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    yearly_mprice = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    six_months_mprice = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    price_changed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    changed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    internal_rating = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: false),
                    internal_rating_number = table.Column<int>(type: "integer", nullable: false),
                    congestion_rating = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: false),
                    congestion_rating_number = table.Column<int>(type: "integer", nullable: false),
                    owned_by = table.Column<Guid>(type: "uuid", nullable: true),
                    currency_id = table.Column<Guid>(type: "uuid", nullable: false),
                    city_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("gym_pkey", x => x.id);
                    table.ForeignKey(
                        name: "gym_city_id_fkey",
                        column: x => x.city_id,
                        principalTable: "city",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "gym_currency_id_fkey",
                        column: x => x.currency_id,
                        principalTable: "currency",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "gym_owned_by_fkey",
                        column: x => x.owned_by,
                        principalTable: "account",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "bookmark",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    gym_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("bookmark_pkey", x => x.id);
                    table.ForeignKey(
                        name: "bookmark_gym_id_fkey",
                        column: x => x.gym_id,
                        principalTable: "gym",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "bookmark_user_id_fkey",
                        column: x => x.user_id,
                        principalTable: "account",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "congestion_rating",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    changed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    visit_time = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    weekday = table.Column<int>(type: "integer", nullable: false),
                    avg_waiting_time = table.Column<int>(type: "integer", nullable: false),
                    crowdedness = table.Column<int>(type: "integer", nullable: false),
                    gym_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("congestion_rating_pkey", x => x.id);
                    table.ForeignKey(
                        name: "congestion_rating_gym_id_fkey",
                        column: x => x.gym_id,
                        principalTable: "gym",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "congestion_rating_user_id_fkey",
                        column: x => x.user_id,
                        principalTable: "account",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "gym_working_hours",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    weekday = table.Column<int>(type: "integer", nullable: false),
                    changed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    gym_id = table.Column<Guid>(type: "uuid", nullable: false),
                    working_hours_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("gym_working_hours_pkey", x => x.id);
                    table.ForeignKey(
                        name: "gym_working_hours_gym_id_fkey",
                        column: x => x.gym_id,
                        principalTable: "gym",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "gym_working_hours_working_hours_id_fkey",
                        column: x => x.working_hours_id,
                        principalTable: "working_hours",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "ownership",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    requested_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    responded_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    message = table.Column<string>(type: "text", nullable: true),
                    decision = table.Column<OwnershipDecision>(type: "ownership_decision", nullable: false),
                    responded_by = table.Column<Guid>(type: "uuid", nullable: true),
                    requested_by = table.Column<Guid>(type: "uuid", nullable: false),
                    gym_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("ownership_pkey", x => x.id);
                    table.ForeignKey(
                        name: "ownership_gym_id_fkey",
                        column: x => x.gym_id,
                        principalTable: "gym",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "ownership_requested_by_fkey",
                        column: x => x.requested_by,
                        principalTable: "account",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "ownership_responded_by_fkey",
                        column: x => x.responded_by,
                        principalTable: "account",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "rating",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    changed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    rating = table.Column<int>(type: "integer", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    gym_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("rating_pkey", x => x.id);
                    table.ForeignKey(
                        name: "rating_gym_id_fkey",
                        column: x => x.gym_id,
                        principalTable: "gym",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "rating_user_id_fkey",
                        column: x => x.user_id,
                        principalTable: "account",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "recommendation",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    tcost = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: false),
                    time = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    time_score = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: false),
                    tcost_score = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: false),
                    congestion_score = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: true),
                    rating_score = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: true),
                    total_score = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: false),
                    type = table.Column<RecommendationType>(type: "recommendation_type", nullable: false),
                    gym_id = table.Column<Guid>(type: "uuid", nullable: false),
                    request_id = table.Column<Guid>(type: "uuid", nullable: false),
                    currency_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("recommendation_pkey", x => x.id);
                    table.ForeignKey(
                        name: "recommendation_currency_id_fkey",
                        column: x => x.currency_id,
                        principalTable: "currency",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "recommendation_gym_id_fkey",
                        column: x => x.gym_id,
                        principalTable: "gym",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "recommendation_request_id_fkey",
                        column: x => x.request_id,
                        principalTable: "request",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "account_email_key",
                table: "account",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "account_outer_uid_key",
                table: "account",
                column: "outer_uid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "account_username_key",
                table: "account",
                column: "username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_account_email",
                table: "account",
                column: "email");

            migrationBuilder.CreateIndex(
                name: "idx_account_outer_uid",
                table: "account",
                column: "outer_uid");

            migrationBuilder.CreateIndex(
                name: "idx_account_username",
                table: "account",
                column: "username");

            migrationBuilder.CreateIndex(
                name: "IX_account_created_by",
                table: "account",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "availability_gym_id_marked_by_key",
                table: "availability",
                columns: new[] { "gym_id", "marked_by" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "bookmark_user_id_gym_id_key",
                table: "bookmark",
                columns: new[] { "user_id", "gym_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_bookmark_gym_id",
                table: "bookmark",
                column: "gym_id");

            migrationBuilder.CreateIndex(
                name: "idx_city_name_country_id",
                table: "city",
                columns: new[] { "name", "country_id" });

            migrationBuilder.CreateIndex(
                name: "IX_city_country_id",
                table: "city",
                column: "country_id");

            migrationBuilder.CreateIndex(
                name: "congestion_rating_gym_id_user_id_key",
                table: "congestion_rating",
                columns: new[] { "gym_id", "user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_congestion_rating_gym_id",
                table: "congestion_rating",
                column: "gym_id");

            migrationBuilder.CreateIndex(
                name: "IX_congestion_rating_user_id",
                table: "congestion_rating",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "country_name_key",
                table: "country",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_country_name",
                table: "country",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "currency_code_key",
                table: "currency",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "currency_name_key",
                table: "currency",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "gym_external_place_id_key",
                table: "gym",
                column: "external_place_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_gym_city_id",
                table: "gym",
                column: "city_id");

            migrationBuilder.CreateIndex(
                name: "idx_gym_external_place_id",
                table: "gym",
                column: "external_place_id");

            migrationBuilder.CreateIndex(
                name: "idx_gym_lat_lon",
                table: "gym",
                columns: new[] { "latitude", "longitude" });

            migrationBuilder.CreateIndex(
                name: "idx_gym_owned_by",
                table: "gym",
                column: "owned_by");

            migrationBuilder.CreateIndex(
                name: "idx_gym_price_changed_at",
                table: "gym",
                column: "price_changed_at");

            migrationBuilder.CreateIndex(
                name: "IX_gym_currency_id",
                table: "gym",
                column: "currency_id");

            migrationBuilder.CreateIndex(
                name: "gym_working_hours_weekday_gym_id_working_hours_id_key",
                table: "gym_working_hours",
                columns: new[] { "weekday", "gym_id", "working_hours_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_gym_working_hours_gym_id",
                table: "gym_working_hours",
                column: "gym_id");

            migrationBuilder.CreateIndex(
                name: "IX_gym_working_hours_working_hours_id",
                table: "gym_working_hours",
                column: "working_hours_id");

            migrationBuilder.CreateIndex(
                name: "idx_notification_user_id",
                table: "notification",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_ownership_gym_id",
                table: "ownership",
                column: "gym_id");

            migrationBuilder.CreateIndex(
                name: "IX_ownership_requested_by",
                table: "ownership",
                column: "requested_by");

            migrationBuilder.CreateIndex(
                name: "IX_ownership_responded_by",
                table: "ownership",
                column: "responded_by");

            migrationBuilder.CreateIndex(
                name: "idx_rating_gym_id",
                table: "rating",
                column: "gym_id");

            migrationBuilder.CreateIndex(
                name: "rating_user_id_gym_id_key",
                table: "rating",
                columns: new[] { "user_id", "gym_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_recommendation_request_id",
                table: "recommendation",
                column: "request_id");

            migrationBuilder.CreateIndex(
                name: "IX_recommendation_currency_id",
                table: "recommendation",
                column: "currency_id");

            migrationBuilder.CreateIndex(
                name: "recommendation_gym_id_request_id_key",
                table: "recommendation",
                columns: new[] { "gym_id", "request_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_request_name",
                table: "request",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "idx_request_user_id",
                table: "request",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "request_user_id_name_key",
                table: "request",
                columns: new[] { "user_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "request_user_id_requested_at_key",
                table: "request",
                columns: new[] { "user_id", "requested_at" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_request_pause_ip",
                table: "request_pause",
                column: "ip");

            migrationBuilder.CreateIndex(
                name: "idx_request_pause_user_id",
                table: "request_pause",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "request_pause_ip_key",
                table: "request_pause",
                column: "ip",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "request_pause_user_id_key",
                table: "request_pause",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_request_period_request_id",
                table: "request_period",
                column: "request_id");

            migrationBuilder.CreateIndex(
                name: "request_period_request_id_weekday_key",
                table: "request_period",
                columns: new[] { "request_id", "weekday" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_user_token_user_id",
                table: "user_token",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "user_token_outer_token_key",
                table: "user_token",
                column: "outer_token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "user_token_user_id_key",
                table: "user_token",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "working_hours_open_from_open_until_key",
                table: "working_hours",
                columns: new[] { "open_from", "open_until" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "availability");

            migrationBuilder.DropTable(
                name: "bookmark");

            migrationBuilder.DropTable(
                name: "congestion_rating");

            migrationBuilder.DropTable(
                name: "gym_working_hours");

            migrationBuilder.DropTable(
                name: "notification");

            migrationBuilder.DropTable(
                name: "ownership");

            migrationBuilder.DropTable(
                name: "rating");

            migrationBuilder.DropTable(
                name: "recommendation");

            migrationBuilder.DropTable(
                name: "request_pause");

            migrationBuilder.DropTable(
                name: "request_period");

            migrationBuilder.DropTable(
                name: "user_token");

            migrationBuilder.DropTable(
                name: "working_hours");

            migrationBuilder.DropTable(
                name: "gym");

            migrationBuilder.DropTable(
                name: "request");

            migrationBuilder.DropTable(
                name: "city");

            migrationBuilder.DropTable(
                name: "currency");

            migrationBuilder.DropTable(
                name: "account");

            migrationBuilder.DropTable(
                name: "country");
        }
    }
}
