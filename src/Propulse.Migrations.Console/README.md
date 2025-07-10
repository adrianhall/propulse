# Propulse Migrations Console

A command-line tool for applying database migrations using the Propulse migration service.

## Usage

### Basic Usage

```bash
# Using command line argument
migrate -c "Server=localhost;Database=propulse;User Id=admin;Password=password;"

# Using environment variable
export ConnectionStrings__DefaultConnection="Server=localhost;Database=propulse;User Id=admin;Password=password;"
migrate
```

### Command Line Options

- `-c, --connection-string <connection-string>`: The database connection string to use for migrations
- `--no-logging`: Disable logging output (useful for testing purposes)
- `--help`: Show help information

### Environment Variables

- `ConnectionStrings__DefaultConnection`: The default database connection string (compatible with .NET Aspire)

## Exit Codes

The application returns different exit codes to indicate the result:

- `0`: Success - migrations completed successfully
- `1`: Invalid arguments or configuration errors
- `2`: Migration execution failed
- `3`: Unexpected error occurred

## Examples

### Run migrations with connection string argument
```bash
migrate --connection-string "Server=localhost;Database=propulse;User Id=admin;Password=password;"
```

### Run migrations with environment variable
```bash
export ConnectionStrings__DefaultConnection="Server=localhost;Database=propulse;User Id=admin;Password=password;"
migrate
```

### Run migrations without logging (for scripts/automation)
```bash
migrate --connection-string "Server=localhost;Database=propulse;User Id=admin;Password=password;" --no-logging
```

### Check if migrations succeeded in a script
```bash
migrate --connection-string "Server=localhost;Database=propulse;User Id=admin;Password=password;"
if [ $? -eq 0 ]; then
    echo "Migrations completed successfully"
else
    echo "Migrations failed with exit code $?"
    exit 1
fi
```

## Building

```bash
dotnet build
```

## Publishing

```bash
# Publish as self-contained executable
dotnet publish -c Release -r win-x64 --self-contained

# Publish as framework-dependent
dotnet publish -c Release
```

## Integration with .NET Aspire

This tool is designed to work seamlessly with .NET Aspire applications. The `ConnectionStrings__DefaultConnection` environment variable follows the standard .NET configuration pattern and can be automatically populated by Aspire's configuration system.
