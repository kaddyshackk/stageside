-- Create databases (quoted to preserve case)
CREATE DATABASE "Comedy";
CREATE DATABASE "Pipeline";

-- Grant database-level privileges
GRANT ALL PRIVILEGES ON DATABASE "Comedy" TO comedypull;
GRANT ALL PRIVILEGES ON DATABASE "Pipeline" TO comedypull;