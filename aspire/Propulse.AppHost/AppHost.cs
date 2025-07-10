#pragma warning disable IDE0211 // Convert to Main program method

var builder = DistributedApplication.CreateBuilder(args);

// =========================================================================
// Azure Replacement Services
// =========================================================================
var postgres = builder.AddPostgres("postgres").WithDbGate();
var database = postgres.AddDatabase("Propulse");

// =========================================================================
// Install the Database Schema
// =========================================================================
var migrations = builder.AddProject<Projects.Propulse_Migrations_Console>("migrations")
    .WithReference(database, "DefaultConnection").WaitFor(database);

// =========================================================================
// Propulse Services
// =========================================================================

builder.Build().Run();
