-- Create databases (quoted to preserve case)
CREATE DATABASE "Comedy";
CREATE DATABASE "Scheduling";

-- Grant database-level privileges
GRANT ALL PRIVILEGES ON DATABASE "Comedy" TO comedypull;
GRANT ALL PRIVILEGES ON DATABASE "Scheduling" TO comedypull;