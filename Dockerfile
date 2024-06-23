# Use the official .NET 8 SDK image as a build environment
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /src

# Step 1: Copy the solution file and project files
COPY Crossword.sln ./
COPY CrossWeb/CrossWeb.csproj CrossWeb/
COPY Crossword/Crossword.csproj Crossword/

# Step 2: Restore dependencies for the entire solution
RUN dotnet restore CrossWeb/CrossWeb.csproj

# Step 3: Copy the entire source code (this will invalidate the cache if any source file changes)
COPY . .

# Step 4: Build the project
WORKDIR /src/CrossWeb
RUN dotnet publish -c Release -o out

# Step 5: Use the official .NET 8 ASP.NET Core runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build-env /src/CrossWeb/out .

# Step 6: Expose port 80
EXPOSE 80

# Step 7: Set the entry point for the application
ENTRYPOINT ["dotnet", "CrossWeb.dll"]