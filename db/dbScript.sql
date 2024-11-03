CREATE EXTENSION IF NOT EXISTS "uuid-ossp"; --it is needed for uuid generator (uuid_generate_v4()) to be available

--Basically, we dynamically create enums if they do not exist
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

--Since we will have a lot of gyms stored (potentially) it does not make sense to store working hours for each of them as a text
--Especially since there are significantly smaller potential patterns [(opening hours, closing hours) pairs] in comparison to the potential number of gyms stored
CREATE TABLE if not exists Working_hours
(
  id uuid NOT null DEFAULT uuid_generate_v4(),--uuid is analogous to id, but uuid is pseudo-random and the length and format of uuids are the same
  open_from TIME NOT NULL,
  open_until TIME NOT NULL,
  PRIMARY KEY (id),
  UNIQUE (open_from),
  UNIQUE (open_until),
  check (open_until > open_from)
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
  provider provider_type NOT NULL, --we want to differentiate between google accounts and local accounts
  password_hash CHAR(60) NOT NULL, --we are storing hash of the password that we got from bycrypt
  type account_type NOT NULL, --we want to differentate between user, admin and gym accounts
  created_by uuid, --admin accounts can be created only by another admin accounts. in this case we want to register who created the admin account
  PRIMARY KEY (id),
  FOREIGN KEY (created_by) REFERENCES Account(id),
  UNIQUE (username),
  UNIQUE (email)
);
create index if not exists idx_account_username on Account(username);
create index if not exists idx_account_email on Account(email);
create index if not exists idx_account_type on Account("type");

create table if not exists Country
(
	id uuid not null default uuid_generate_v4(),
	name varchar(56) not null,
	primary key(id),
	UNIQUE (name)
);
create index if not exists idx_country_name on Country("name");

create table if not exists City
(
	id uuid not null default uuid_generate_v4(),
	nelatitude FLOAT NOT null check (nelatitude between -90 and 90), --rectangular approximation of the area of the city
    nelongitude FLOAT NOT null check (nelongitude between -180 and 180),
    swlatitude FLOAT NOT null check (swlatitude between -90 and 90),
    swlongitude FLOAT NOT null check (swlongitude between -180 and 180),
	name varchar(100) not null,
	country_id uuid not null,
	primary key(id),
	foreign key (country_id) references Country(id)
);
create index if not exists idx_city_name_country_id on City("name", country_id);

CREATE TABLE if not exists Gym
(
  id uuid NOT null DEFAULT uuid_generate_v4(),
  latitude FLOAT NOT null check (latitude between -90 and 90),
  longitude FLOAT NOT null check (longitude between -180 and 180),
  name VARCHAR(80) NOT NULL,
  external_place_id VARCHAR(50) NOT NULL, --id of the place in Google Places API
  external_rating NUMERIC(4,2) NOT null check (external_rating between 0 and 5), --rating retrieved from Google
  external_rating_number INT NOT null check (external_rating_number >= 0),--number of accounts that contributed to the google rating
  phone_number VARCHAR(15),
  address VARCHAR(80) NOT NULL,
  website VARCHAR(255),
  is_wheelchair_accessible BOOL NOT NULL,
  membership_price NUMERIC(5,2) check (membership_price >= 0),
  created_at TIMESTAMP NOT null check (created_at <= NOW()),
  price_changed_at TIMESTAMP check (price_changed_at >= changed_at and price_changed_at > created_at),
  changed_at TIMESTAMP check (changed_at > created_at and changed_at <= NOW()),
  internal_rating NUMERIC(4,2) NOT null check (internal_rating between 0 and 5), --local rating by local users
  internal_rating_number INT NOT null check (internal_rating_number >= 0), --number of accounts that contributed to the local rating
  congestion_rating NUMERIC(4,2) NOT null check (congestion_rating between 0 and 5),
  congestion_rating_number INT NOT null check (congestion_rating_number >= 0),
  owned_by uuid, --a gym account that manages information about this gym
  currency_id uuid NOT NULL,
  city_id uuid not null,
  PRIMARY KEY (id),
  FOREIGN KEY (owned_by) REFERENCES Account(id),
  FOREIGN KEY (currency_id) REFERENCES Currency(id),
  foreign key (city_id) references City(id),
  UNIQUE (external_place_id)
);
create index if not exists idx_gym_external_place_id on Gym(external_place_id);
create index if not exists idx_gym_lat_lon on Gym(latitude, longitude);
create index if not exists idx_gym_price_changed_at on Gym(price_changed_at);
create index if not exists idx_gym_owned_by on Gym(owned_by);
create index if not exists idx_gym_city_id on Gym(city_id);

CREATE TABLE if not exists Ownership
(
  id uuid NOT null DEFAULT uuid_generate_v4(),
  requested_at TIMESTAMP NOT null check (requested_at <= NOW()), --time at which a gym account requested the ownership
  responded_at TIMESTAMP check (responded_at > requested_at and responded_at <= NOW()), --time at which an admin made a decision regarding the ownership
  status own_status NOT NULL, --status of the requested ownership: pending, approved, rejected
  message text, --message the admin might write in order to explain his/her decision
  responded_by uuid, --the admin account that made the decision
  requested_by uuid NOT NULL, --the gym account that made the ownership request
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
  requested_at TIMESTAMP NOT null check (requested_at <= NOW()), --time of the request
  origin_latitude FLOAT NOT null check (origin_latitude between -90 and 90), --latitude of the starting location
  origin_longitude FLOAT NOT null check (origin_longitude between -180 and 180),
  departure_time TIME, --preferred departure_time specified by user
  weekday INT check (weekday between 0 and 6), --day of the week for which the user wants to receive the recommendations 
  arrival_time TIME check (arrival_time > departure_time), --preferred arrival time specified by user
  time_priority INT NOT null check (time_priority between 0 and 100), --the value of the travelling time slider on the frontend
  tcost_priority INT NOT null check (tcost_priority between 0 and 100) CHECK (tcost_priority + time_priority = 100),
  min_congestion_rating NUMERIC(4,2) NOT null check (min_congestion_rating between 1 and 5),
  min_rating NUMERIC(4,2) NOT null check (min_rating between 1 and 5),
  min_membership_price INT NOT null check (min_membership_price >= 0),
  name VARCHAR(50),--each user can specify the name of the request 
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
  tcost NUMERIC(4,2) NOT null check (tcost >= 0), --total travelling cost to get to the gym
  time TIME NOT NULL, --total travelling time to the gym
  time_score NUMERIC(4,2) NOT null check (time_score between 0 and 10), --calculated score for the travelling time
  tcost_score NUMERIC(4,2) NOT null check (tcost_score between 0 and 10), --calculated score for the total price
  congestion_score NUMERIC(4,2) check (congestion_score between 0 and 10),
  rating_score NUMERIC(4,2) check (rating_score between 0 and 10),
  total_score NUMERIC(4,2) NOT null check (total_score between 0 and 10),
  type rec_type NOT NULL, --main or alternative rating
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
  visit_time TIME NOT NULL, --time of the account's owner visit
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
  type not_type NOT NULL,--more of an expansion for the potential future elaboration. for now we need just the 'message' type
  message TEXT NOT NULL,
  created_at TIMESTAMP NOT null check (created_at <= NOW()),
  read_at DATE check (read_at > created_at and read_at <= NOW()), --time at which the user read/opened the notification
  user_id uuid NOT NULL,
  PRIMARY KEY (id),
  FOREIGN KEY (user_id) REFERENCES Account(id)
);
create index if not exists idx_notification_user_id on Notification(user_id);

--Gyms can mark unavailability of the gyms for a certain time period
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

--long story short, since all types of accounts are stored in the same table, we want to ensure that for some foreign keys we have accounts of certain type
--for example, in the Account table created_by field must reference only admin accounts, because only admin accounts have rights to create other admin accounts
--analogously, Ratings can be left only by user accounts, and we must ensure that it is so in the db as well
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



