-- Create databases
CREATE DATABASE "Processing";
CREATE DATABASE "Comedy";

-- Grant database-level privileges
GRANT ALL PRIVILEGES ON DATABASE "Processing" TO comedypull;
GRANT ALL PRIVILEGES ON DATABASE "Comedy" TO comedypull;
