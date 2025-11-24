-- Create databases (quoted to preserve case)
CREATE DATABASE "Scheduling";
CREATE DATABASE "SpaCollecting";
CREATE DATABASE "Processing";
CREATE DATABASE "Comedy";

-- Grant database-level privileges
GRANT ALL PRIVILEGES ON DATABASE "Scheduling" TO comedypull;
GRANT ALL PRIVILEGES ON DATABASE "SpaCollecting" TO comedypull;
GRANT ALL PRIVILEGES ON DATABASE "Processing" TO comedypull;
GRANT ALL PRIVILEGES ON DATABASE "Comedy" TO comedypull;