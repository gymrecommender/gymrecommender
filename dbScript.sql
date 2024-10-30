CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'provider_type') THEN
        CREATE TYPE provider_type AS ENUM ('local', 'google');
    END IF;
	IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'account_type') THEN
        CREATE TYPE account_type AS ENUM ('user', 'gym', 'admin');
    END IF;
	IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'own_status') THEN
        CREATE TYPE own_status AS ENUM ('pending', 'approved', 'rejected');
    END IF;
	IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'rec_type') THEN
        CREATE TYPE rec_type AS ENUM ('main', 'alternative');
    END IF;
	IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'not_type') THEN
        CREATE TYPE not_type AS ENUM ('message', 'alert', 'reminder');
    END IF;
END $$;

CREATE TABLE if not exists Working_hours
(
  id uuid NOT null DEFAULT uuid_generate_v4(),
  open_from TIME NOT NULL,
  open_to TIME NOT NULL,
  PRIMARY KEY (id),
  UNIQUE (open_from),
  UNIQUE (open_to),
  check (open_to > open_from)
);

CREATE TABLE if not exists Currency
(
  id uuid NOT null DEFAULT uuid_generate_v4(),
  name VARCHAR(10) NOT NULL,
  code CHAR(3) NOT NULL,
  PRIMARY KEY (id),
  UNIQUE (name),
  UNIQUE (code)
);

CREATE TABLE if not exists Account
(
  id uuid NOT null DEFAULT uuid_generate_v4(),
  username VARCHAR(40) NOT null check (length(username) > 5),
  email VARCHAR(60) not NULL,
  first_name VARCHAR(60) NOT NULL,
  last_name VARCHAR(60) NOT NULL,
  created_at TIMESTAMP NOT null check (created_at <= NOW()),
  provider provider_type NOT NULL,
  password_hash CHAR(60) NOT NULL,
  type account_type NOT NULL,
  created_by uuid,
  PRIMARY KEY (id),
  FOREIGN KEY (created_by) REFERENCES Account(id),
  UNIQUE (username),
  UNIQUE (email)
);
create index if not exists idx_account_username on Account(username);
create index if not exists idx_account_email on Account(email);
create index if not exists idx_account_type on Account("type");


CREATE TABLE if not exists Gym
(
  id uuid NOT null DEFAULT uuid_generate_v4(),
  latitude FLOAT NOT null check (latitude between -90 and 90),
  longitude FLOAT NOT null check (longitude between -180 and 180),
  name VARCHAR(80) NOT NULL,
  external_place_id VARCHAR(50) NOT NULL,
  external_rating NUMERIC(4,2) NOT null check (external_rating between 0 and 5),
  external_rating_number INT NOT null check (external_rating_number >= 0),
  phone_number VARCHAR(15),
  address VARCHAR(80) NOT NULL,
  website VARCHAR(255),
  is_wheelchair_accessible BOOL NOT NULL,
  membership_price NUMERIC(5,2) check (membership_price >= 0),
  created_at TIMESTAMP NOT null check (created_at <= NOW()),
  price_changed_at TIMESTAMP check (price_changed_at >= changed_at and price_changed_at > created_at),
  changed_at TIMESTAMP check (changed_at > created_at and changed_at <= NOW()),
  internal_rating NUMERIC(4,2) NOT null check (internal_rating between 0 and 5),
  internal_rating_number INT NOT null check (internal_rating_number >= 0),
  congestion_rating NUMERIC(4,2) NOT null check (congestion_rating between 0 and 5),
  congestion_rating_number INT NOT null check (congestion_rating_number >= 0),
  owned_by uuid,
  currency_id uuid NOT NULL,
  PRIMARY KEY (id),
  FOREIGN KEY (owned_by) REFERENCES Account(id),
  FOREIGN KEY (currency_id) REFERENCES Currency(id),
  UNIQUE (external_place_id)
);
create index if not exists idx_gym_external_place_id on Gym(external_place_id);
create index if not exists idx_gym_lat_lon on Gym(latitude, longitude);
create index if not exists idx_gym_price_changed_at on Gym(price_changed_at);
create index if not exists idx_gym_owned_by on Gym(owned_by);


CREATE TABLE if not exists Ownership
(
  id uuid NOT null DEFAULT uuid_generate_v4(),
  requested_at TIMESTAMP NOT null check (requested_at <= NOW()),
  responded_at TIMESTAMP check (responded_at > requested_at and responded_at <= NOW()),
  status own_status NOT NULL,
  responded_by uuid,
  requested_by uuid NOT NULL,
  gym_id uuid  NOT NULL,
  PRIMARY KEY (id),
  FOREIGN KEY (responded_by) REFERENCES Account(id),
  FOREIGN KEY (requested_by) REFERENCES Account(id),
  FOREIGN KEY (gym_id) REFERENCES Gym(id),
  UNIQUE (gym_id, requested_by, status)
);

CREATE TABLE if not exists Request
(
  id uuid NOT null DEFAULT uuid_generate_v4(),
  requested_at TIMESTAMP NOT null check (requested_at <= NOW()),
  origin_latitude FLOAT NOT null check (origin_latitude between -90 and 90),
  origin_longitude FLOAT NOT null check (origin_longitude between -180 and 180),
  departure_time TIME,
  arrival_time TIME check (arrival_time > departure_time),
  time_priority INT NOT null check (time_priority between 0 and 100),
  tcost_priority INT NOT null check (tcost_priority between 0 and 100),
  price_priority INT NOT null check (price_priority between 0 and 100),
  rating_priority INT NOT null check (rating_priority between 0 and 100),
  congestion_rating_priority INT NOT null check (congestion_rating_priority between 0 and 100),
  name VARCHAR(50) NOT NULL,
  user_id uuid NOT NULL,
  PRIMARY KEY (id),
  FOREIGN KEY (user_id) REFERENCES Account(id),
  UNIQUE (user_id, name),
  UNIQUE (user_id, requested_at)
);
create index if not exists idx_request_user_id on Request(user_id);
create index if not exists idx_request_name on Request("name");

CREATE TABLE if not exists Recommendation
(
  tcost NUMERIC(4,2) NOT null check (tcost >= 0),
  time TIME NOT NULL,
  time_score NUMERIC(4,2) NOT null check (time_score between 0 and 10),
  tcost_score NUMERIC(4,2) NOT null check (tcost_score between 0 and 10),
  price_score NUMERIC(4,2) NOT null check (price_score between 0 and 10),
  congestion_score NUMERIC(4,2) NOT null check (congestion_score between 0 and 10),
  rating_score NUMERIC(4,2) NOT null check (rating_score between 0 and 10),
  total_score NUMERIC(4,2) NOT null check (total_score between 0 and 10),
  type rec_type NOT NULL,
  gym_id uuid NOT NULL,
  request_id uuid NOT NULL,
  currency_id uuid NOT NULL,
  PRIMARY KEY (gym_id, request_id),
  FOREIGN KEY (gym_id) REFERENCES Gym(id),
  FOREIGN KEY (request_id) REFERENCES Request(id),
  FOREIGN KEY (currency_id) REFERENCES Currency(id)
);
create index if not exists idx_recommendation_request_id on Recommendation(request_id);

CREATE TABLE if not exists Rating
(
  created_at TIMESTAMP NOT null check (created_at <= NOW()),
  rating INT NOT null check (rating between 0 and 5),
  user_id uuid NOT NULL,
  gym_id uuid NOT NULL,
  PRIMARY KEY (user_id, gym_id),
  FOREIGN KEY (user_id) REFERENCES Account(id),
  FOREIGN KEY (gym_id) REFERENCES Gym(id)
);
create index if not exists idx_rating_gym_id on Rating(gym_id);

CREATE TABLE if not exists Congestion_rating
(
  created_at TIMESTAMP NOT null check (created_at <= NOW()),
  changed_at TIMESTAMP NOT null check (changed_at > created_at and changed_at <= NOW()),
  visit_time TIME NOT NULL,
  weekday INT NOT null check (weekday between 0 and 6),
  avg_waiting_time INT NOT null check (avg_waiting_time between 1 and 5),
  crowdedness INT NOT null check (crowdedness between 1 and 5),
  gym_id uuid NOT NULL,
  user_id uuid NOT NULL,
  PRIMARY KEY (gym_id, user_id),
  FOREIGN KEY (gym_id) REFERENCES Gym(id),
  FOREIGN KEY (user_id) REFERENCES Account(id)
);
create index if not exists idx_congestion_rating_gym_id on Congestion_rating(gym_id);

CREATE TABLE if not exists Gym_working_hours
(
  weekday INT NOT null check (weekday between 0 and 6),
  changed_at TIMESTAMP check (changed_at <= NOW()),
  gym_id uuid NOT NULL,
  working_hours_id uuid NOT NULL,
  PRIMARY KEY (weekday, gym_id, working_hours_id),
  FOREIGN KEY (gym_id) REFERENCES Gym(id),
  FOREIGN KEY (working_hours_id) REFERENCES Working_hours(id)
);
create index if not exists idx_gym_working_hours_gym_id on Gym_working_hours(gym_id);

CREATE TABLE if not exists Bookmark
(
  created_at TIMESTAMP NOT null check(created_at <= NOW()),
  user_id uuid NOT NULL,
  gym_id uuid NOT NULL,
  PRIMARY KEY (user_id, gym_id),
  FOREIGN KEY (user_id) REFERENCES Gym(id),
  FOREIGN KEY (gym_id) REFERENCES Account(id)
);

CREATE TABLE if not exists Notification
(
  id uuid NOT null DEFAULT uuid_generate_v4(),
  type not_type NOT NULL,
  message TEXT NOT NULL,
  created_at TIMESTAMP NOT null check (created_at <= NOW()),
  read_at DATE check (read_at > created_at and read_at <= NOW()),
  user_id uuid NOT NULL,
  PRIMARY KEY (id),
  FOREIGN KEY (user_id) REFERENCES Account(id)
);
create index if not exists idx_notification_user_id on Notification(user_id);


CREATE table if not exists Availability
(
  created_at TIMESTAMP NOT null check (created_at <= NOW()),
  start_time TIMESTAMP NOT NULL,
  end_time timestamp check (end_time > start_time and end_time > NOW()),
  changed_at TIMESTAMP check (changed_at > created_at and changed_at <= NOW()),
  gym_id uuid NOT NULL,
  marked_by uuid NOT NULL,
  PRIMARY KEY (gym_id, marked_by),
  FOREIGN KEY (gym_id) REFERENCES Gym(id),
  FOREIGN KEY (marked_by) REFERENCES Account(id)
);

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
        FROM Account
        WHERE id = account_id
        AND type = expected_type
    ) THEN
        RAISE EXCEPTION 'Account referenced in % must have type = %', TG_TABLE_NAME, expected_type;
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
    BEFORE INSERT OR UPDATE ON Notification
    FOR EACH ROW
    EXECUTE FUNCTION enforce_account_type('user', 'user_id');
  END IF;

  IF NOT EXISTS (
    SELECT 1 FROM pg_trigger WHERE tgname = 'enforce_availability_gym_type'
  ) THEN
    CREATE TRIGGER enforce_availability_gym_type
    BEFORE INSERT OR UPDATE ON Availability
    FOR EACH ROW
    EXECUTE FUNCTION enforce_account_type('gym', 'marked_by');
  END IF;

  IF NOT EXISTS (
    SELECT 1 FROM pg_trigger WHERE tgname = 'enforce_bookmark_user_type'
  ) THEN
    CREATE TRIGGER enforce_bookmark_user_type
    BEFORE INSERT OR UPDATE ON Bookmark
    FOR EACH ROW
    EXECUTE FUNCTION enforce_account_type('user', 'user_id');
  END IF;

  IF NOT EXISTS (
    SELECT 1 FROM pg_trigger WHERE tgname = 'enforce_congestion_rating_user_type'
  ) THEN
    CREATE TRIGGER enforce_congestion_rating_user_type
    BEFORE INSERT OR UPDATE ON Congestion_rating
    FOR EACH ROW
    EXECUTE FUNCTION enforce_account_type('user', 'user_id');
  END IF;

  IF NOT EXISTS (
    SELECT 1 FROM pg_trigger WHERE tgname = 'enforce_rating_user_type'
  ) THEN
    CREATE TRIGGER enforce_rating_user_type
    BEFORE INSERT OR UPDATE ON Rating
    FOR EACH ROW
    EXECUTE FUNCTION enforce_account_type('user', 'user_id');
  END IF;

  IF NOT EXISTS (
    SELECT 1 FROM pg_trigger WHERE tgname = 'enforce_request_user_type'
  ) THEN
    CREATE TRIGGER enforce_request_user_type
    BEFORE INSERT OR UPDATE ON Request
    FOR EACH ROW
    EXECUTE FUNCTION enforce_account_type('user', 'user_id');
  END IF;

  IF NOT EXISTS (
    SELECT 1 FROM pg_trigger WHERE tgname = 'enforce_ownership_responded_admin_type'
  ) THEN
    CREATE TRIGGER enforce_ownership_responded_admin_type
    BEFORE INSERT OR UPDATE ON Ownership
    FOR EACH ROW
    EXECUTE FUNCTION enforce_account_type('admin', 'responded_by');
  END IF;

  IF NOT EXISTS (
    SELECT 1 FROM pg_trigger WHERE tgname = 'enforce_ownership_requested_gym_type'
  ) THEN
    CREATE TRIGGER enforce_ownership_requested_gym_type
    BEFORE INSERT OR UPDATE ON Ownership
    FOR EACH ROW
    EXECUTE FUNCTION enforce_account_type('gym', 'requested_by');
  END IF;

  IF NOT EXISTS (
    SELECT 1 FROM pg_trigger WHERE tgname = 'enforce_gym_gym_type'
  ) THEN
    CREATE TRIGGER enforce_gym_gym_type
    BEFORE INSERT OR UPDATE ON Gym
    FOR EACH ROW
    EXECUTE FUNCTION enforce_account_type('gym', 'owned_by');
  END IF;

  IF NOT EXISTS (
    SELECT 1 FROM pg_trigger WHERE tgname = 'enforce_account_admin_type'
  ) THEN
    CREATE TRIGGER enforce_account_admin_type
    BEFORE INSERT OR UPDATE ON Account
    FOR EACH ROW
    EXECUTE FUNCTION enforce_account_type('admin', 'created_by');
  END IF;

END $$;



