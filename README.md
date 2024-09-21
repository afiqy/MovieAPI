# MovieAPI

This is a .NET 8.0 Web API project that integrates with AWS Cognito for authentication, uses MySQL as the database, and Redis (via Redis Labs free version) for caching. The project demonstrates how to validate AWS Cognito JWT tokens and securely handle authentication with dynamic JWKS signing key resolution.

## Table of Contents

- [Features](#features)
- [Prerequisites](#prerequisites)
- [Installation](#installation)
- [Configuration](#configuration)
- [Using Redis Cloud (Redis Labs Free Version)](#using-redis-cloud-redis-labs-free-version)
- [Running the Application](#running-the-application)
- [Testing with Swagger](#testing-with-swagger)
- [Troubleshooting](#troubleshooting)

## Features

- JWT authentication via AWS Cognito
- MySQL integration for storing user data and favorites
- Redis (from Redis Labs) for caching frequently requested data
- Dynamic JWKS key resolution for JWT validation
- API documentation with Swagger

## Prerequisites

Before running the application, ensure you have the following installed:

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [MySQL Server](https://dev.mysql.com/downloads/mysql/)
- [Redis](https://redis.io/download) or Redis Cloud from Redis Labs (covered below)
- AWS Cognito User Pool and App Client set up (for authentication)

## Installation

1. **Clone the repository**:

    ```bash
    git clone https://github.com/afiqy/MovieAPI.git
    cd MovieAPI
    ```

2. **Install the required NuGet packages**:

    ```bash
    dotnet restore
    ```

3. **Set up your MySQL database**:

    Create a MySQL database and update the connection string in `appsettings.json`:

    ```json
    "ConnectionStrings": {
      "DefaultConnection": "Server=localhost;Database=MovieDB;User=root;Password=yourpassword;"
    }
    ```

4. **Set up Redis (using Redis Labs free version)**:

    If you're not using a local Redis instance, you can create a free Redis instance using [Redis Cloud (Redis Labs)](https://redis.com/try-free/).

    - **Sign up** for a free Redis Cloud account.
    - **Create a free database instance** in Redis Cloud.
    - **Get the Redis connection string** from the Redis Labs dashboard (it should look like `redis-12345.c9.us-east-1-2.ec2.cloud.redislabs.com:16379`).
    - Update the `Redis:ConnectionString` in `appsettings.json`:

    ```json
    "Redis": {
      "ConnectionString": "redis-12345.c9.us-east-1-2.ec2.cloud.redislabs.com:16379,password=your_password_here"
    }
    ```

    This will allow you to use Redis Cloud for caching in your project.

5. **Configure AWS Cognito**:

    You need to set up AWS Cognito for authentication. Update the `AWS` section in `appsettings.json` with your **User Pool ID**, **Region**, **App Client ID**, and (optionally) **Access Key ID** and **Secret Access Key** if required.

    ```json
    "AWS": {
      "UserPoolId": "your_user_pool_id",
      "ClientId": "your_app_client_id",
      "Region": "your_region",
      "AccessKeyId": "your_access_key",
      "SecretAccessKey": "your_secret_access_key"
    }
    ```

6. **Run the database migrations**:

    After configuring the connection string, run the Entity Framework Core migrations to create the database tables:

    ```bash
    dotnet ef database update
    ```

## Configuration

- **MySQL Database**: The connection string can be configured in `appsettings.json` under `ConnectionStrings:DefaultConnection`.
- **Redis**: You can either use a local Redis instance or configure Redis Cloud. Update the Redis connection string in `appsettings.json` under `Redis:ConnectionString` with the Redis Cloud connection details from Redis Labs.
- **AWS Cognito**: Set your AWS Cognito credentials and region in the `AWS` section of `appsettings.json`.

## Using Redis Cloud (Redis Labs Free Version)

You can use Redis for caching by using a **free Redis Cloud instance** from [Redis Labs](https://redis.com/try-free/). The steps to use it are as follows:

1. **Sign up for Redis Cloud** at [https://redis.com/try-free/](https://redis.com/try-free/).
2. **Create a free Redis database**. You'll get a Redis connection string like:
    ```bash
    redis-12345.c9.us-east-1-2.ec2.cloud.redislabs.com:16379
    ```
3. **Add the connection string** to your `appsettings.json` under the `Redis` section:
```json
"Redis": {
  "ConnectionString": "redis-12345.c9.us-east-1-2.ec2.cloud.redislabs.com:16379,password=your_password_here"
}


##Running the Application

1. **Run the API:**
    To run the API locally:
    ```bash
        dotnet run
    ```
2. **Access the API:**
    By default, the API will be available at https://localhost:7055 or http://localhost:5048.
3. **Access Swagger UI:**
    You can access the Swagger API documentation at:
    ```bash
        https://localhost:7055/swagger
    ```

##Testing with Swagger
1. **Login via AWS Cognito:**
    Use the /api/auth/login endpoint to authenticate and get an Access Token.
2. **Authorize in Swagger:**
    Use the Authorize button in Swagger and paste the Bearer token you received after logging in.
3. **Test other endpoints:**
    Now you can test other API endpoints like /api/movies, /api/favourites, etc., using the Access Token for authentication.

##### Design Decisions

The **MovieAPI** project is built with clear separation of responsibilities to make the code easy to manage and extend:

1. **Organized Structure**: The code is divided into different layers:
   - **Controllers** handle incoming requests and pass them to services.
   - **Services** handle the main business logic, like calling external APIs or managing data.
   - **Data Layer** (via `ApplicationDbContext`) handles all interactions with the database.
   - **DTOs** ensure only needed data is passed between different parts of the system.

2. **AWS Cognito Integration**: We use AWS Cognito to handle user authentication and registration, which provides secure and scalable user management without the need to store sensitive password data ourselves.

3. **Redis Caching**: Using Redis to cache frequent requests (like popular movies) improves speed and reduces unnecessary calls to external APIs, making the app faster and more efficient.

4. **Pagination for Large Data**: To ensure the system can handle large datasets, movie lists and favorite movies are displayed in pages (10 at a time), preventing performance issues.

---

### Caching Strategy

We use **Redis** to cache data, which speeds up the app and reduces load:

1. **Movie Data Caching**: When a user requests movie lists or details, we save (cache) the data in Redis for an hour. This way, if someone asks for the same information again, we can quickly return the cached data instead of hitting the external API repeatedly.

2. **Cache Keys**: Each cached item has a unique key (e.g., `MovieList_Page_1`), ensuring that different pages or searches are stored separately, making sure the data is correct for every request.

3. **Expiration Time**: Cached data expires after a certain time (like 1 hour), ensuring that outdated information is not served to users.

**Why this is better**: Caching reduces the load on external services (TMDB) and speeds up responses for users, leading to better performance and reduced costs for API usage.

---

### Security Measures

We’ve taken strong steps to ensure the system is secure:

1. **JWT Authentication**: Users must log in to get a **JWT token**, which they use to access protected areas of the app (e.g., adding favorite movies). This ensures only authorized users can access certain features.

2. **AWS Cognito for User Management**: AWS Cognito handles user sign-up, login, and password management securely, meaning we don't store passwords ourselves. It also confirms email addresses for added security.

3. **SQL Injection Protection**: Using **Entity Framework** prevents SQL injection attacks by using safe queries to interact with the database.

4. **Environment Configuration**: Sensitive information (like AWS keys) is stored in environment files (`appsettings.json`), making the application more secure by not hard-coding these values.

**Why this is better**: By using AWS Cognito and secure coding practices, we ensure user data is safe and we don’t have to manage complex security details like password encryption ourselves. This makes the app more secure and easier to maintain.