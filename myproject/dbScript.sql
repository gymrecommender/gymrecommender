-- Enable the uuid extension if not already enabled
CREATE EXTENSION IF NOT EXISTS "uuid-ossp"; -- it is needed for uuid generator (uuid_generate_v4()) to be available

-- Dynamically create enums if they do not exist
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
END $$ LANGUAGE plpgsql;

-- Table definitions
CREATE TABLE IF NOT EXISTS Working_hours (
    id uuid NOT NULL DEFAULT uuid_generate_v4(),
    open_from TIME NOT NULL,
    open_until TIME NOT NULL,
    PRIMARY KEY (id),
    UNIQUE (open_from),
    UNIQUE (open_until),
    CHECK (open_until > open_from)
);

CREATE TABLE IF NOT EXISTS Currency (
    id uuid NOT NULL DEFAULT uuid_generate_v4(),
    name VARCHAR(10) NOT NULL,
    code CHAR(3) NOT NULL,
    PRIMARY KEY (id),
    UNIQUE (name),
    UNIQUE (code)
);

CREATE TABLE IF NOT EXISTS Account (
    id uuid NOT NULL DEFAULT uuid_generate_v4(),
    username VARCHAR(40) NOT NULL CHECK (length(username) > 5),
    email VARCHAR(60) NOT NULL,
    first_name VARCHAR(60) NOT NULL,
    last_name VARCHAR(60) NOT NULL,
    created_at TIMESTAMP NOT NULL CHECK (created_at <= NOW()),
    provider provider_type NOT NULL,
    password_hash CHAR(60) NOT NULL,
    type account_type NOT NULL,
    created_by uuid,
    PRIMARY KEY (id),
    FOREIGN KEY (created_by) REFERENCES Account(id),
    UNIQUE (username),
    UNIQUE (email)
);
CREATE INDEX IF NOT EXISTS idx_account_username ON Account(username);
CREATE INDEX IF NOT EXISTS idx_account_email ON Account(email);
CREATE INDEX IF NOT EXISTS idx_account_type ON Account("type");

CREATE TABLE IF NOT EXISTS Gym (
    id uuid NOT NULL DEFAULT uuid_generate_v4(),
    latitude FLOAT NOT NULL CHECK (latitude BETWEEN -90 AND 90),
    longitude FLOAT NOT NULL CHECK (longitude BETWEEN -180 AND 180),
    name VARCHAR(80) NOT NULL,
    external_place_id VARCHAR(50) NOT NULL,
    external_rating NUMERIC(4,2) NOT NULL CHECK (external_rating BETWEEN 0 AND 5),
    external_rating_number INT NOT NULL CHECK (external_rating_number >= 0),
    phone_number VARCHAR(15),
    address VARCHAR(80) NOT NULL,
    website VARCHAR(255),
    is_wheelchair_accessible BOOL NOT NULL,
    membership_price NUMERIC(5,2) CHECK (membership_price >= 0),
    created_at TIMESTAMP NOT NULL CHECK (created_at <= NOW()),
    price_changed_at TIMESTAMP CHECK (price_changed_at >= created_at),
    changed_at TIMESTAMP CHECK (changed_at > created_at AND changed_at <= NOW()),
    internal_rating NUMERIC(4,2) NOT NULL CHECK (internal_rating BETWEEN 0 AND 5),
    internal_rating_number INT NOT NULL CHECK (internal_rating_number >= 0),
    congestion_rating NUMERIC(4,2) NOT NULL CHECK (congestion_rating BETWEEN 0 AND 5),
    congestion_rating_number INT NOT NULL CHECK (congestion_rating_number >= 0),
    owned_by uuid,
    currency_id uuid NOT NULL,
    PRIMARY KEY (id),
    FOREIGN KEY (owned_by) REFERENCES Account(id),
    FOREIGN KEY (currency_id) REFERENCES Currency(id),
    UNIQUE (external_place_id)
);
CREATE INDEX IF NOT EXISTS idx_gym_external_place_id ON Gym(external_place_id);
CREATE INDEX IF NOT EXISTS idx_gym_lat_lon ON Gym(latitude, longitude);
CREATE INDEX IF NOT EXISTS idx_gym_price_changed_at ON Gym(price_changed_at);
CREATE INDEX IF NOT EXISTS idx_gym_owned_by ON Gym(owned_by);

CREATE TABLE IF NOT EXISTS Notification (
    id uuid NOT NULL DEFAULT uuid_generate_v4(),
    type not_type NOT NULL,
    message TEXT NOT NULL,
    created_at TIMESTAMP NOT NULL CHECK (created_at <= NOW()),
    read_at DATE CHECK (read_at > created_at AND read_at <= NOW()),
    user_id uuid NOT NULL,
    PRIMARY KEY (id),
    FOREIGN KEY (user_id) REFERENCES Account(id)
);
CREATE INDEX IF NOT EXISTS idx_notification_user_id ON Notification(user_id);

CREATE TABLE IF NOT EXISTS Ownership (
    id uuid NOT NULL DEFAULT uuid_generate_v4(),
    requested_at TIMESTAMP NOT NULL CHECK (requested_at <= NOW()),
    responded_at TIMESTAMP CHECK (responded_at > requested_at AND responded_at <= NOW()),
    status own_status NOT NULL,
    message TEXT,
    responded_by uuid,
    requested_by uuid NOT NULL,
    gym_id uuid NOT NULL,
    PRIMARY KEY (id),
    FOREIGN KEY (responded_by) REFERENCES Account(id),
    FOREIGN KEY (requested_by) REFERENCES Account(id),
    FOREIGN KEY (gym_id) REFERENCES Gym(id),
    UNIQUE (gym_id, requested_by, status)
);

CREATE TABLE IF NOT EXISTS Request (
    id uuid NOT NULL DEFAULT uuid_generate_v4(),
    requested_at TIMESTAMP NOT NULL CHECK (requested_at <= NOW()),
    origin_latitude FLOAT NOT NULL CHECK (origin_latitude BETWEEN -90 AND 90),
    origin_longitude FLOAT NOT NULL CHECK (origin_longitude BETWEEN -180 AND 180),
    departure_time TIME,
    weekday INT CHECK (weekday BETWEEN 0 AND 6),
    arrival_time TIME CHECK (arrival_time > departure_time),
    time_priority INT NOT NULL CHECK (time_priority BETWEEN 0 AND 100),
    tcost_priority INT NOT NULL CHECK (tcost_priority BETWEEN 0 AND 100),
    price_priority INT NOT NULL CHECK (price_priority BETWEEN 0 AND 100),
    rating_priority INT NOT NULL CHECK (rating_priority BETWEEN 0 AND 100),
    congestion_rating_priority INT NOT NULL CHECK (congestion_rating_priority BETWEEN 0 AND 100),
    name VARCHAR(50),
    user_id uuid NOT NULL,
    PRIMARY KEY (id),
    FOREIGN KEY (user_id) REFERENCES Account(id),
    UNIQUE (user_id, name),
    UNIQUE (user_id, requested_at)
);
CREATE INDEX IF NOT EXISTS idx_request_user_id ON Request(user_id);
CREATE INDEX IF NOT EXISTS idx_request_name ON Request("name");

-- Trigger function to enforce account type constraints
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

-- Triggers
CREATE TRIGGER enforce_notification_user_type
    BEFORE INSERT OR UPDATE ON Notification
    FOR EACH ROW
    EXECUTE FUNCTION enforce_account_type('user', 'user_id');

CREATE TRIGGER enforce_availability_gym_type
    BEFORE INSERT OR UPDATE ON Ownership
    FOR EACH ROW
    EXECUTE FUNCTION enforce_account_type('gym', 'requested_by');

-- Additional trigger statements should be added below each respective table definition.
