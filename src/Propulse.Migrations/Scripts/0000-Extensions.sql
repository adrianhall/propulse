-- PostgreSQL Extensions Setup
-- This script installs essential PostgreSQL extensions used throughout the application.
-- All extensions are created with IF NOT EXISTS to make this script idempotent.

-- Case-insensitive text type for storing text data that should be compared case-insensitively
-- Used for email addresses, usernames, and other text fields where case doesn't matter
CREATE EXTENSION IF NOT EXISTS "citext";

-- Key-value store data type for storing sets of key/value pairs within a single PostgreSQL value
-- Useful for storing flexible metadata, configuration settings, or sparse data
CREATE EXTENSION IF NOT EXISTS "hstore";

-- Cryptographic functions for password hashing, encryption, and generating secure random values
-- Provides functions like gen_random_uuid(), crypt(), and digest() for security operations
CREATE EXTENSION IF NOT EXISTS "pgcrypto";

-- Trigram matching for efficient text search and similarity scoring
-- Enables fast LIKE/ILIKE queries, fuzzy text matching, and text similarity operations
CREATE EXTENSION IF NOT EXISTS "pg_trgm";

-- UUID generation functions for creating universally unique identifiers
-- Provides uuid_generate_v1(), uuid_generate_v4(), and other UUID generation functions
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";