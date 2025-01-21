CREATE EXTENSION IF NOT EXISTS "uuid-ossp"; --it is needed for uuid generator (uuid_generate_v4()) to be available

--Basically, we dynamically create enums if they do not exist
do $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'provider_type') THEN
        CREATE TYPE provider_type AS ENUM ('local', 'google');
    END IF;
	IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'account_type') THEN
        CREATE TYPE account_type AS ENUM ('user', 'gym', 'admin');
    END IF;
	IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'own_decision') THEN
        CREATE TYPE own_decision AS ENUM ('approved', 'rejected');
    END IF;
	IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'rec_type') THEN
        CREATE TYPE rec_type AS ENUM ('main', 'alternative');
    END IF;
	IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'not_type') THEN
        CREATE TYPE not_type AS ENUM ('message', 'alert', 'reminder');
    END IF;
    IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'membership_type') THEN
        CREATE TYPE membership_type AS ENUM ('month', 'halfyear', 'year');
    END IF;
END $$;

--Since we will have a lot of gyms stored (potentially) it does not make sense to store working hours for each of them as a text
--Especially since there are significantly smaller potential patterns [(opening hours, closing hours) pairs] in comparison to the potential number of gyms stored
--We store only ranges without days or any other information
-- public.country definition

-- Drop table

-- DROP TABLE public.country;

CREATE TABLE public.country (
	id uuid DEFAULT uuid_generate_v4() NOT NULL,
	"name" varchar(56) NOT NULL,
	CONSTRAINT country_name_key UNIQUE (name),
	CONSTRAINT country_pkey PRIMARY KEY (id)
);
CREATE INDEX idx_country_name ON public.country USING btree (name);


-- public.currency definition

-- Drop table

-- DROP TABLE public.currency;

CREATE TABLE public.currency (
	id uuid DEFAULT uuid_generate_v4() NOT NULL,
	"name" varchar(10) NOT NULL,
	code bpchar(3) NOT NULL,
	CONSTRAINT currency_code_key UNIQUE (code),
	CONSTRAINT currency_name_key UNIQUE (name),
	CONSTRAINT currency_pkey PRIMARY KEY (id)
);


-- public.working_hours definition

-- Drop table

-- DROP TABLE public.working_hours;

CREATE TABLE public.working_hours (
	id uuid DEFAULT uuid_generate_v4() NOT NULL,
	open_from time NOT NULL,
	open_until time NOT NULL,
	CONSTRAINT working_hours_check CHECK ((open_until > open_from)),
	CONSTRAINT working_hours_open_from_open_until_key UNIQUE (open_from, open_until),
	CONSTRAINT working_hours_pkey PRIMARY KEY (id)
);


-- public.account definition

-- Drop table

-- DROP TABLE public.account;

CREATE TABLE public.account (
	id uuid DEFAULT uuid_generate_v4() NOT NULL,
	username varchar(40) NOT NULL,
	outer_uid varchar(128) NOT NULL,
	email text NOT NULL,
	is_email_verified bool DEFAULT false NOT NULL,
	first_name varchar(60) NOT NULL,
	last_name varchar(60) NOT NULL,
	created_at timestamptz DEFAULT now() NOT NULL,
	last_sign_in timestamptz NULL,
	provider public."provider_type" NOT NULL,
	password_hash bpchar(60) NOT NULL,
	"type" public."account_type" NOT NULL,
	created_by uuid NULL,
	CONSTRAINT account_check CHECK ((last_sign_in > created_at)),
	CONSTRAINT account_created_at_check CHECK ((created_at <= now())),
	CONSTRAINT account_email_key UNIQUE (email),
	CONSTRAINT account_outer_uid_key UNIQUE (outer_uid),
	CONSTRAINT account_pkey PRIMARY KEY (id),
	CONSTRAINT account_username_check CHECK ((length((username)::text) > 5)),
	CONSTRAINT account_username_key UNIQUE (username),
	CONSTRAINT account_created_by_fkey FOREIGN KEY (created_by) REFERENCES public.account(id) ON DELETE SET NULL
);
CREATE INDEX idx_account_email ON public.account USING btree (email);
CREATE INDEX idx_account_outer_uid ON public.account USING btree (outer_uid);
CREATE INDEX idx_account_type ON public.account USING btree (type);
CREATE INDEX idx_account_username ON public.account USING btree (username);
-- public.city definition

-- Drop table

-- DROP TABLE public.city;

CREATE TABLE public.city (
	id uuid DEFAULT uuid_generate_v4() NOT NULL,
	nelatitude float8 NOT NULL,
	nelongitude float8 NOT NULL,
	swlatitude float8 NOT NULL,
	swlongitude float8 NOT NULL,
	"name" varchar(100) NOT NULL,
	country_id uuid NOT NULL,
	CONSTRAINT city_nelatitude_check CHECK (((nelatitude >= ('-90'::integer)::double precision) AND (nelatitude <= (90)::double precision))),
	CONSTRAINT city_nelongitude_check CHECK (((nelongitude >= ('-180'::integer)::double precision) AND (nelongitude <= (180)::double precision))),
	CONSTRAINT city_pkey PRIMARY KEY (id),
	CONSTRAINT city_swlatitude_check CHECK (((swlatitude >= ('-90'::integer)::double precision) AND (swlatitude <= (90)::double precision))),
	CONSTRAINT city_swlongitude_check CHECK (((swlongitude >= ('-180'::integer)::double precision) AND (swlongitude <= (180)::double precision))),
	CONSTRAINT city_country_id_fkey FOREIGN KEY (country_id) REFERENCES public.country(id) ON DELETE CASCADE
);
CREATE INDEX idx_city_name_country_id ON public.city USING btree (name, country_id);


-- public.gym definition

-- Drop table

-- DROP TABLE public.gym;

CREATE TABLE public.gym (
	id uuid DEFAULT uuid_generate_v4() NOT NULL,
	latitude float8 NOT NULL,
	longitude float8 NOT NULL,
	"name" text NOT NULL,
	external_place_id varchar(50) NOT NULL,
	external_rating numeric(4, 2) NOT NULL,
	external_rating_number int4 NOT NULL,
	phone_number varchar(15) NULL,
	address text NOT NULL,
	website varchar(255) NULL,
	is_wheelchair_accessible bool NOT NULL,
	monthly_mprice numeric(5, 2) NULL,
	yearly_mprice numeric(5, 2) NULL,
	six_months_mprice numeric(5, 2) NULL,
	created_at timestamptz DEFAULT now() NOT NULL,
	price_changed_at timestamptz NULL,
	changed_at timestamptz NULL,
	internal_rating numeric(4, 2) NOT NULL,
	internal_rating_number int4 NOT NULL,
	congestion_rating numeric(4, 2) NOT NULL,
	congestion_rating_number int4 NOT NULL,
	owned_by uuid NULL,
	currency_id uuid NOT NULL,
	city_id uuid NOT NULL,
	CONSTRAINT gym_check CHECK (((price_changed_at >= changed_at) AND (price_changed_at > created_at))),
	CONSTRAINT gym_check1 CHECK (((changed_at > created_at) AND (changed_at <= now()))),
	CONSTRAINT gym_congestion_rating_check CHECK (((congestion_rating >= (0)::numeric) AND (congestion_rating <= (5)::numeric))),
	CONSTRAINT gym_congestion_rating_number_check CHECK ((congestion_rating_number >= 0)),
	CONSTRAINT gym_created_at_check CHECK ((created_at <= now())),
	CONSTRAINT gym_external_place_id_key UNIQUE (external_place_id),
	CONSTRAINT gym_external_rating_check CHECK (((external_rating >= (0)::numeric) AND (external_rating <= (5)::numeric))),
	CONSTRAINT gym_external_rating_number_check CHECK ((external_rating_number >= 0)),
	CONSTRAINT gym_internal_rating_check CHECK (((internal_rating >= (0)::numeric) AND (internal_rating <= (5)::numeric))),
	CONSTRAINT gym_internal_rating_number_check CHECK ((internal_rating_number >= 0)),
	CONSTRAINT gym_latitude_check CHECK (((latitude >= ('-90'::integer)::double precision) AND (latitude <= (90)::double precision))),
	CONSTRAINT gym_longitude_check CHECK (((longitude >= ('-180'::integer)::double precision) AND (longitude <= (180)::double precision))),
	CONSTRAINT gym_monthly_mprice_check CHECK ((monthly_mprice >= (0)::numeric)),
	CONSTRAINT gym_pkey PRIMARY KEY (id),
	CONSTRAINT gym_six_months_mprice_check CHECK ((six_months_mprice >= (0)::numeric)),
	CONSTRAINT gym_yearly_mprice_check CHECK ((yearly_mprice >= (0)::numeric)),
	CONSTRAINT gym_city_id_fkey FOREIGN KEY (city_id) REFERENCES public.city(id) ON DELETE CASCADE,
	CONSTRAINT gym_currency_id_fkey FOREIGN KEY (currency_id) REFERENCES public.currency(id),
	CONSTRAINT gym_owned_by_fkey FOREIGN KEY (owned_by) REFERENCES public.account(id) ON DELETE SET NULL
);
CREATE INDEX idx_gym_city_id ON public.gym USING btree (city_id);
CREATE INDEX idx_gym_external_place_id ON public.gym USING btree (external_place_id);
CREATE INDEX idx_gym_lat_lon ON public.gym USING btree (latitude, longitude);
CREATE INDEX idx_gym_owned_by ON public.gym USING btree (owned_by);
CREATE INDEX idx_gym_price_changed_at ON public.gym USING btree (price_changed_at);


-- public.gym_working_hours definition

-- Drop table

-- DROP TABLE public.gym_working_hours;

CREATE TABLE public.gym_working_hours (
	id uuid DEFAULT uuid_generate_v4() NOT NULL,
	weekday int4 NOT NULL,
	changed_at timestamptz NULL,
	gym_id uuid NOT NULL,
	working_hours_id uuid NOT NULL,
	CONSTRAINT gym_working_hours_changed_at_check CHECK ((changed_at <= now())),
	CONSTRAINT gym_working_hours_pkey PRIMARY KEY (id),
	CONSTRAINT gym_working_hours_weekday_check CHECK (((weekday >= 0) AND (weekday <= 6))),
	CONSTRAINT gym_working_hours_weekday_gym_id_working_hours_id_key UNIQUE (weekday, gym_id, working_hours_id),
	CONSTRAINT gym_working_hours_gym_id_fkey FOREIGN KEY (gym_id) REFERENCES public.gym(id) ON DELETE CASCADE,
	CONSTRAINT gym_working_hours_working_hours_id_fkey FOREIGN KEY (working_hours_id) REFERENCES public.working_hours(id)
);
CREATE INDEX idx_gym_working_hours_gym_id ON public.gym_working_hours USING btree (gym_id);


-- public.notification definition

-- Drop table

-- DROP TABLE public.notification;

CREATE TABLE public.notification (
	id uuid DEFAULT uuid_generate_v4() NOT NULL,
	"type" public."not_type" NOT NULL,
	message text NOT NULL,
	created_at timestamptz DEFAULT now() NOT NULL,
	read_at timestamptz NULL,
	user_id uuid NOT NULL,
	CONSTRAINT notification_check CHECK (((read_at > created_at) AND (read_at <= now()))),
	CONSTRAINT notification_created_at_check CHECK ((created_at <= now())),
	CONSTRAINT notification_pkey PRIMARY KEY (id),
	CONSTRAINT notification_user_id_fkey FOREIGN KEY (user_id) REFERENCES public.account(id) ON DELETE CASCADE
);
CREATE INDEX idx_notification_user_id ON public.notification USING btree (user_id);


-- public.ownership definition

-- Drop table

-- DROP TABLE public.ownership;

CREATE TABLE public.ownership (
	id uuid DEFAULT uuid_generate_v4() NOT NULL,
	requested_at timestamptz DEFAULT now() NOT NULL,
	responded_at timestamptz NULL,
	decision public."own_decision" NULL,
	message text NULL,
	responded_by uuid NULL,
	requested_by uuid NOT NULL,
	gym_id uuid NOT NULL,
	CONSTRAINT ownership_check CHECK (((responded_at > requested_at) AND (responded_at <= now()))),
	CONSTRAINT ownership_gym_id_requested_by_decision_key UNIQUE (gym_id, requested_by, decision),
	CONSTRAINT ownership_pkey PRIMARY KEY (id),
	CONSTRAINT ownership_requested_at_check CHECK ((requested_at <= now())),
	CONSTRAINT ownership_gym_id_fkey FOREIGN KEY (gym_id) REFERENCES public.gym(id),
	CONSTRAINT ownership_requested_by_fkey FOREIGN KEY (requested_by) REFERENCES public.account(id) ON DELETE CASCADE,
	CONSTRAINT ownership_responded_by_fkey FOREIGN KEY (responded_by) REFERENCES public.account(id) ON DELETE SET NULL
);


-- public.rating definition

-- Drop table

-- DROP TABLE public.rating;

CREATE TABLE public.rating (
	id uuid DEFAULT uuid_generate_v4() NOT NULL,
	created_at timestamptz DEFAULT now() NOT NULL,
	changed_at timestamptz NULL,
	rating int4 NOT NULL,
	user_id uuid NOT NULL,
	gym_id uuid NOT NULL,
	CONSTRAINT rating_check CHECK (((changed_at > created_at) AND (changed_at <= now()))),
	CONSTRAINT rating_created_at_check CHECK ((created_at <= now())),
	CONSTRAINT rating_pkey PRIMARY KEY (id),
	CONSTRAINT rating_rating_check CHECK (((rating >= 0) AND (rating <= 5))),
	CONSTRAINT rating_user_id_gym_id_key UNIQUE (user_id, gym_id),
	CONSTRAINT rating_gym_id_fkey FOREIGN KEY (gym_id) REFERENCES public.gym(id) ON DELETE CASCADE,
	CONSTRAINT rating_user_id_fkey FOREIGN KEY (user_id) REFERENCES public.account(id) ON DELETE CASCADE
);
CREATE INDEX idx_rating_gym_id ON public.rating USING btree (gym_id);


-- public.request definition

-- Drop table

-- DROP TABLE public.request;

CREATE TABLE public.request (
	id uuid DEFAULT uuid_generate_v4() NOT NULL,
	requested_at timestamptz DEFAULT now() NOT NULL,
	origin_latitude float8 NOT NULL,
	origin_longitude float8 NOT NULL,
	time_priority int4 NOT NULL,
	total_cost_priority int4 NOT NULL,
	min_congestion_rating numeric(4, 2) NOT NULL,
	min_rating numeric(4, 2) NOT NULL,
	min_membership_price int4 NOT NULL,
	"name" varchar(50) NULL,
	user_id uuid NOT NULL,
	memb_type public."membership_type" NOT NULL,
	departure_time time NULL,
	arrival_time time NULL,
	CONSTRAINT request_check CHECK ((((total_cost_priority >= 0) AND (total_cost_priority <= 100)) AND ((total_cost_priority + time_priority) = 100))),
	CONSTRAINT request_min_congestion_rating_check CHECK (((min_congestion_rating >= (1)::numeric) AND (min_congestion_rating <= (5)::numeric))),
	CONSTRAINT request_min_membership_price_check CHECK ((min_membership_price >= 0)),
	CONSTRAINT request_min_rating_check CHECK (((min_rating >= (1)::numeric) AND (min_rating <= (5)::numeric))),
	CONSTRAINT request_origin_latitude_check CHECK (((origin_latitude >= ('-90'::integer)::double precision) AND (origin_latitude <= (90)::double precision))),
	CONSTRAINT request_origin_longitude_check CHECK (((origin_longitude >= ('-180'::integer)::double precision) AND (origin_longitude <= (180)::double precision))),
	CONSTRAINT request_pkey PRIMARY KEY (id),
	CONSTRAINT request_requested_at_check CHECK ((requested_at <= now())),
	CONSTRAINT request_time_priority_check CHECK (((time_priority >= 0) AND (time_priority <= 100))),
	CONSTRAINT request_user_id_name_key UNIQUE (user_id, name),
	CONSTRAINT request_user_id_requested_at_key UNIQUE (user_id, requested_at),
	CONSTRAINT request_user_id_fkey FOREIGN KEY (user_id) REFERENCES public.account(id) ON DELETE CASCADE
);
CREATE INDEX idx_request_name ON public.request USING btree (name);
CREATE INDEX idx_request_user_id ON public.request USING btree (user_id);


-- public.request_pause definition

-- Drop table

-- DROP TABLE public.request_pause;

CREATE TABLE public.request_pause (
	id uuid DEFAULT uuid_generate_v4() NOT NULL,
	user_id uuid NULL,
	ip bytea NULL,
	started_at timestamptz DEFAULT now() NOT NULL,
	CONSTRAINT request_pause_check CHECK (((user_id IS NOT NULL) OR (ip IS NOT NULL))),
	CONSTRAINT request_pause_ip_check CHECK (((octet_length(ip) = 4) OR (octet_length(ip) = 16))),
	CONSTRAINT request_pause_ip_key UNIQUE (ip),
	CONSTRAINT request_pause_pkey PRIMARY KEY (id),
	CONSTRAINT request_pause_started_at_check CHECK ((started_at <= now())),
	CONSTRAINT request_pause_user_id_key UNIQUE (user_id),
	CONSTRAINT request_pause_user_id_fkey FOREIGN KEY (user_id) REFERENCES public.account(id) ON DELETE CASCADE
);
CREATE INDEX idx_request_pause_ip ON public.request_pause USING btree (ip);
CREATE INDEX idx_request_pause_user_id ON public.request_pause USING btree (user_id);


-- public.availability definition

-- Drop table

-- DROP TABLE public.availability;

CREATE TABLE public.availability (
	id uuid DEFAULT uuid_generate_v4() NOT NULL,
	created_at timestamptz DEFAULT now() NOT NULL,
	start_time timestamptz NOT NULL,
	end_time timestamptz NULL,
	changed_at timestamptz NULL,
	gym_id uuid NOT NULL,
	marked_by uuid NOT NULL,
	CONSTRAINT availability_check CHECK (((end_time > start_time) AND (end_time > now()))),
	CONSTRAINT availability_created_at_check CHECK ((created_at <= now())),
	CONSTRAINT availability_gym_id_marked_by_key UNIQUE (gym_id, marked_by),
	CONSTRAINT availability_pkey PRIMARY KEY (id),
	CONSTRAINT availability_gym_id_fkey FOREIGN KEY (gym_id) REFERENCES public.gym(id) ON DELETE CASCADE,
	CONSTRAINT availability_marked_by_fkey FOREIGN KEY (marked_by) REFERENCES public.account(id)
);


-- public.bookmark definition

-- Drop table

-- DROP TABLE public.bookmark;

CREATE TABLE public.bookmark (
	id uuid DEFAULT uuid_generate_v4() NOT NULL,
	created_at timestamptz DEFAULT now() NOT NULL,
	user_id uuid NOT NULL,
	gym_id uuid NOT NULL,
	CONSTRAINT bookmark_created_at_check CHECK ((created_at <= now())),
	CONSTRAINT bookmark_pkey PRIMARY KEY (id),
	CONSTRAINT bookmark_user_id_gym_id_key UNIQUE (user_id, gym_id),
	CONSTRAINT bookmark_gym_id_fkey FOREIGN KEY (gym_id) REFERENCES public.gym(id) ON DELETE CASCADE,
	CONSTRAINT bookmark_user_id_fkey FOREIGN KEY (user_id) REFERENCES public.account(id) ON DELETE CASCADE
);

-- public.congestion_rating definition

-- Drop table

-- DROP TABLE public.congestion_rating;

CREATE TABLE public.congestion_rating (
	id uuid DEFAULT uuid_generate_v4() NOT NULL,
	created_at timestamptz DEFAULT now() NOT NULL,
	changed_at timestamptz NULL,
	visit_time time NOT NULL,
	weekday int4 NOT NULL,
	avg_waiting_time int4 NOT NULL,
	crowdedness int4 NOT NULL,
	gym_id uuid NOT NULL,
	user_id uuid NOT NULL,
	CONSTRAINT congestion_rating_avg_waiting_time_check CHECK (((avg_waiting_time >= 1) AND (avg_waiting_time <= 5))),
	CONSTRAINT congestion_rating_check CHECK (((changed_at > created_at) AND (changed_at <= now()))),
	CONSTRAINT congestion_rating_created_at_check CHECK ((created_at <= now())),
	CONSTRAINT congestion_rating_crowdedness_check CHECK (((crowdedness >= 1) AND (crowdedness <= 5))),
	CONSTRAINT congestion_rating_gym_id_user_id_key UNIQUE (gym_id, user_id),
	CONSTRAINT congestion_rating_pkey PRIMARY KEY (id),
	CONSTRAINT congestion_rating_weekday_check CHECK (((weekday >= 0) AND (weekday <= 6))),
	CONSTRAINT congestion_rating_gym_id_fkey FOREIGN KEY (gym_id) REFERENCES public.gym(id) ON DELETE CASCADE,
	CONSTRAINT congestion_rating_user_id_fkey FOREIGN KEY (user_id) REFERENCES public.account(id) ON DELETE CASCADE
);
CREATE INDEX idx_congestion_rating_gym_id ON public.congestion_rating USING btree (gym_id);


-- public.recommendation definition

-- Drop table

-- DROP TABLE public.recommendation;

CREATE TABLE public.recommendation (
	id uuid DEFAULT uuid_generate_v4() NOT NULL,
	tcost numeric(4, 2) NOT NULL,
	"time" time NOT NULL,
	time_score numeric(4, 2) NOT NULL,
	tcost_score numeric(4, 2) NOT NULL,
	congestion_score numeric(4, 2) NULL,
	rating_score numeric(4, 2) NULL,
	total_score numeric(4, 2) NOT NULL,
	"type" public."rec_type" NOT NULL,
	gym_id uuid NOT NULL,
	request_id uuid NOT NULL,
	currency_id uuid NOT NULL,
	CONSTRAINT recommendation_congestion_score_check CHECK ((((congestion_score >= (0)::numeric) AND (congestion_score <= (10)::numeric)) OR (congestion_score = (-1)::numeric))),
	CONSTRAINT recommendation_gym_id_request_id_key UNIQUE (gym_id, request_id),
	CONSTRAINT recommendation_pkey PRIMARY KEY (id),
	CONSTRAINT recommendation_rating_score_check CHECK ((((rating_score >= (0)::numeric) AND (rating_score <= (10)::numeric)) OR (rating_score = (-1)::numeric))),
	CONSTRAINT recommendation_tcost_check CHECK ((tcost >= (0)::numeric) OR (tcost = (1)::numeric)),
	CONSTRAINT recommendation_tcost_score_check CHECK (((tcost_score >= (0)::numeric) AND (tcost_score <= (10)::numeric))),
	CONSTRAINT recommendation_time_score_check CHECK (((time_score >= (0)::numeric) AND (time_score <= (10)::numeric))),
	CONSTRAINT recommendation_total_score_check CHECK (((total_score >= (0)::numeric) AND (total_score <= (10)::numeric))),
	CONSTRAINT recommendation_currency_id_fkey FOREIGN KEY (currency_id) REFERENCES public.currency(id),
	CONSTRAINT recommendation_gym_id_fkey FOREIGN KEY (gym_id) REFERENCES public.gym(id) ON DELETE CASCADE,
	CONSTRAINT recommendation_request_id_fkey FOREIGN KEY (request_id) REFERENCES public.request(id) ON DELETE CASCADE
);
CREATE INDEX idx_recommendation_request_id ON public.recommendation USING btree (request_id);

--long story short, since all types of accounts are stored in the same table, we want to ensure that for some foreign keys we have accounts of certain type
--for example, in the account table created_by field must reference only admin accounts, because only admin accounts have rights to create other admin accounts
--analogously, ratings can be left only by user accounts, and we must ensure that it is so in the db as well
CREATE OR REPLACE FUNCTION enforce_account_type()
RETURNS TRIGGER AS $$
DECLARE
    expected_type account_type;
    field_name VARCHAR;
    account_id UUID;
    check_query TEXT;
BEGIN
    expected_type := TG_ARGV[0]::account_type;
    field_name := TG_ARGV[1];

    check_query := format('SELECT ($1).%I FROM (SELECT $1 AS rec) AS x', field_name);

    EXECUTE check_query INTO account_id USING NEW;
    IF account_id IS NULL THEN
        RETURN NEW;
    END IF;
	

    IF NOT EXISTS (
        SELECT 1
        FROM account
        WHERE id = account_id
        AND type = expected_type
    ) THEN
        RAISE EXCEPTION 'account referenced in % must have type = %', TG_TABLE_NAME, expected_type;
    END IF;

    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

DO $$
BEGIN
  IF NOT EXISTS (
    SELECT 1 FROM pg_trigger WHERE tgname = 'enforce_notification_user_type'
  ) THEN
    CREATE TRIGGER enforce_notification_user_type
    BEFORE INSERT OR UPDATE ON notification
    FOR EACH ROW
    EXECUTE FUNCTION enforce_account_type('user', 'user_id');
  END IF;

  IF NOT EXISTS (
    SELECT 1 FROM pg_trigger WHERE tgname = 'enforce_availability_gym_type'
  ) THEN
    CREATE TRIGGER enforce_availability_gym_type
    BEFORE INSERT OR UPDATE ON availability
    FOR EACH ROW
    EXECUTE FUNCTION enforce_account_type('gym', 'marked_by');
  END IF;

  IF NOT EXISTS (
    SELECT 1 FROM pg_trigger WHERE tgname = 'enforce_bookmark_user_type'
  ) THEN
    CREATE TRIGGER enforce_bookmark_user_type
    BEFORE INSERT OR UPDATE ON bookmark
    FOR EACH ROW
    EXECUTE FUNCTION enforce_account_type('user', 'user_id');
  END IF;

  IF NOT EXISTS (
    SELECT 1 FROM pg_trigger WHERE tgname = 'enforce_congestion_rating_user_type'
  ) THEN
    CREATE TRIGGER enforce_congestion_rating_user_type
    BEFORE INSERT OR UPDATE ON congestion_rating
    FOR EACH ROW
    EXECUTE FUNCTION enforce_account_type('user', 'user_id');
  END IF;

  IF NOT EXISTS (
    SELECT 1 FROM pg_trigger WHERE tgname = 'enforce_rating_user_type'
  ) THEN
    CREATE TRIGGER enforce_rating_user_type
    BEFORE INSERT OR UPDATE ON rating
    FOR EACH ROW
    EXECUTE FUNCTION enforce_account_type('user', 'user_id');
  END IF;

  IF NOT EXISTS (
    SELECT 1 FROM pg_trigger WHERE tgname = 'enforce_request_user_type'
  ) THEN
    CREATE TRIGGER enforce_request_user_type
    BEFORE INSERT OR UPDATE ON request
    FOR EACH ROW
    EXECUTE FUNCTION enforce_account_type('user', 'user_id');
  END IF;

  IF NOT EXISTS (
    SELECT 1 FROM pg_trigger WHERE tgname = 'enforce_ownership_responded_admin_type'
  ) THEN
    CREATE TRIGGER enforce_ownership_responded_admin_type
    BEFORE INSERT OR UPDATE ON ownership
    FOR EACH ROW
    EXECUTE FUNCTION enforce_account_type('admin', 'responded_by');
  END IF;

  IF NOT EXISTS (
    SELECT 1 FROM pg_trigger WHERE tgname = 'enforce_ownership_requested_gym_type'
  ) THEN
    CREATE TRIGGER enforce_ownership_requested_gym_type
    BEFORE INSERT OR UPDATE ON ownership
    FOR EACH ROW
    EXECUTE FUNCTION enforce_account_type('gym', 'requested_by');
  END IF;

  IF NOT EXISTS (
    SELECT 1 FROM pg_trigger WHERE tgname = 'enforce_gym_gym_type'
  ) THEN
    CREATE TRIGGER enforce_gym_gym_type
    BEFORE INSERT OR UPDATE ON gym
    FOR EACH ROW
    EXECUTE FUNCTION enforce_account_type('gym', 'owned_by');
  END IF;

IF NOT EXISTS (
    SELECT 1 FROM pg_trigger WHERE tgname = 'enforce_request_pause_user_type'
  ) THEN
    CREATE TRIGGER enforce_request_pause_user_type
    BEFORE INSERT OR UPDATE ON request_pause
    FOR EACH ROW
    EXECUTE FUNCTION enforce_account_type('user', 'user_id');
  END IF;

  IF NOT EXISTS (
    SELECT 1 FROM pg_trigger WHERE tgname = 'enforce_account_admin_type'
  ) THEN
    CREATE TRIGGER enforce_account_admin_type
    BEFORE INSERT OR UPDATE ON account
    FOR EACH ROW
    EXECUTE FUNCTION enforce_account_type('admin', 'created_by');
  END IF;

END $$;
