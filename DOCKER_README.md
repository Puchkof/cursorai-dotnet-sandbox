# HeroBoxAI Docker Setup

This guide explains how to run the HeroBoxAI application locally using Docker Compose.

## Prerequisites

- [Docker](https://docs.docker.com/get-docker/) (version 20.10 or later)
- [Docker Compose](https://docs.docker.com/compose/install/) (version 2.0 or later)

## Services

The docker-compose setup includes the following services:

### 1. PostgreSQL Database (`postgres`)
- **Image**: `postgres:16-alpine`
- **Port**: `5432`
- **Database**: `heroboxai`
- **Username**: `postgres`
- **Password**: `postgres`

### 2. HeroBoxAI Web API (`heroboxai-api`)
- **Port**: `8080`
- **Environment**: Development
- **Swagger UI**: Available at `http://localhost:8080/swagger`

### 3. pgAdmin (Optional - `pgadmin`)
- **Port**: `5050`
- **Email**: `admin@heroboxai.com`
- **Password**: `admin`
- **URL**: `http://localhost:5050`

## Quick Start

### 1. Clone and Navigate
```bash
git clone <repository-url>
cd <repository-name>
```

### 2. Start All Services
```bash
docker-compose up -d
```

This command will:
- Pull the required Docker images
- Build the HeroBoxAI API image
- Start PostgreSQL database
- Start the HeroBoxAI Web API
- Start pgAdmin (optional)

### 3. Verify Services
```bash
# Check if all services are running
docker-compose ps

# View logs
docker-compose logs -f heroboxai-api
```

### 4. Access the Application
- **API Swagger UI**: http://localhost:8080/swagger
- **API Base URL**: http://localhost:8080
- **pgAdmin**: http://localhost:5050 (optional)

## Available Commands

### Start Services
```bash
# Start all services in detached mode
docker-compose up -d

# Start specific service
docker-compose up -d postgres

# Start with logs visible
docker-compose up
```

### Stop Services
```bash
# Stop all services
docker-compose down

# Stop and remove volumes (WARNING: This will delete database data)
docker-compose down -v
```

### View Logs
```bash
# View logs for all services
docker-compose logs

# View logs for specific service
docker-compose logs heroboxai-api
docker-compose logs postgres

# Follow logs in real-time
docker-compose logs -f heroboxai-api
```

### Rebuild Application
```bash
# Rebuild the API image and restart
docker-compose build heroboxai-api
docker-compose up -d heroboxai-api
```

## Database Management

### Using pgAdmin
1. Access pgAdmin at http://localhost:5050
2. Login with:
   - Email: `admin@heroboxai.com`
   - Password: `admin`
3. Add a new server connection:
   - Host: `postgres`
   - Port: `5432`
   - Database: `heroboxai`
   - Username: `postgres`
   - Password: `postgres`

### Using Command Line
```bash
# Connect to PostgreSQL container
docker-compose exec postgres psql -U postgres -d heroboxai

# Run SQL commands
\dt  # List tables
\q   # Quit
```

## Environment Variables

The following environment variables are configured in docker-compose.yml:

### API Configuration
- `ASPNETCORE_ENVIRONMENT=Development`
- `ASPNETCORE_URLS=http://+:8080`
- `ConnectionStrings__DefaultConnection=Host=postgres;Database=heroboxai;Username=postgres;Password=postgres`

### JWT Configuration
- `JwtSettings__SecretKey=YourSuperSecretKeyThatIsAtLeast32CharactersLongForSecurity!`
- `JwtSettings__Issuer=HeroBoxAI`
- `JwtSettings__Audience=HeroBoxAI-Users`
- `JwtSettings__ExpiryInMinutes=60`

## Data Persistence

- **Database Data**: Stored in Docker volume `postgres_data`
- **pgAdmin Data**: Stored in Docker volume `pgadmin_data`
- **Application Logs**: Mapped to `./HeroBoxAI/logs` directory

## Troubleshooting

### Common Issues

#### 1. Port Already in Use
If you get port conflicts, you can change the ports in docker-compose.yml:
```yaml
ports:
  - "8081:8080"  # Change 8080 to 8081
```

#### 2. Database Connection Issues
- Ensure PostgreSQL container is healthy: `docker-compose ps`
- Check database logs: `docker-compose logs postgres`
- Verify connection string in environment variables

#### 3. API Not Starting
- Check API logs: `docker-compose logs heroboxai-api`
- Ensure database is ready before API starts (health check configured)
- Verify all environment variables are set correctly

#### 4. Build Issues
```bash
# Clean rebuild
docker-compose down
docker-compose build --no-cache heroboxai-api
docker-compose up -d
```

### Reset Everything
```bash
# Stop all services and remove volumes
docker-compose down -v

# Remove all images
docker-compose down --rmi all

# Start fresh
docker-compose up -d
```

## Development Workflow

### Making Code Changes
1. Make your code changes in the HeroBoxAI project
2. Rebuild and restart the API:
   ```bash
   docker-compose build heroboxai-api
   docker-compose up -d heroboxai-api
   ```

### Database Migrations
The application automatically applies database migrations on startup. If you add new migrations:
1. Rebuild the API image
2. Restart the API service
3. Migrations will be applied automatically

## API Endpoints

Once the application is running, you can access the following endpoints:

### Authentication
- `POST /api/users/register` - Register a new user
- `POST /api/users/login` - Login user

### Users
- `GET /api/users` - Get all users
- `GET /api/users/{id}` - Get user by ID

### Clans
- `GET /api/clans` - Get all clans
- `POST /api/clans` - Create a new clan (requires authentication)
- `PUT /api/clans/{id}` - Update clan (requires authentication)
- `DELETE /api/clans/{id}` - Delete clan (requires authentication)

### Items
- `GET /api/items` - Get all items
- `POST /api/items` - Create a new item (requires authentication)

For detailed API documentation, visit the Swagger UI at http://localhost:8080/swagger

## Security Notes

- The JWT secret key in this setup is for development only
- Database credentials are default values for local development
- In production, use proper secrets management
- Consider using HTTPS in production environments

## Support

If you encounter any issues:
1. Check the troubleshooting section above
2. Review the logs using `docker-compose logs`
3. Ensure all prerequisites are installed and up to date 