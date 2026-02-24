-- VibeLink Database Initialization Script
-- This runs automatically when the PostgreSQL container starts for the first time

-- Create extensions
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pg_trgm";

-- Indexes for better performance (EF Core creates tables, we add extra indexes)
-- These will be created after EF Core migrations run

-- Note: The actual tables are created by Entity Framework Core migrations
-- This script is for additional setup like extensions and custom indexes

-- Create a function to update updated_at timestamps (for future use)
CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ language 'plpgsql';

-- Grant permissions (if needed for read replicas in the future)
-- GRANT SELECT ON ALL TABLES IN SCHEMA public TO readonly_user;
