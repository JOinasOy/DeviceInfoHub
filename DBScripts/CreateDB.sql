-- Create a new database
CREATE DATABASE deviceinfohub;
GO

-- Switch to the new database
USE deviceinfohub;
GO

CREATE TABLE Company (
    Id NVARCHAR(255) PRIMARY KEY,
    Name nvarchar(255),
    ClientId NVARCHAR(255),
    TenantId NVARCHAR(255),
    ClientSecret NVARCHAR(255),
    KandjiApiKey NVARCHAR(255)
);
GO

-- Create the Device table
CREATE TABLE Device (
    Id nvarchar(255) PRIMARY KEY,
    CompanyId nvarchar(255) REFERENCES Company(Id),
    EnrolledDateTime datetime,
    OperatingSystem nvarchar(255),
    DisplayName nvarchar(255),
    Model nvarchar(255),
    Manufacturer nvarchar(255),
    SerialNumber nvarchar(255),
);
GO

-- Create the Application table
CREATE TABLE Application (
    Id nvarchar(255) PRIMARY KEY,
    DisplayName nvarchar(255),
    Publisher nvarchar(255),
    Version nvarchar(50),
    DeviceId nvarchar(255) REFERENCES Device(Id)
);
GO

-- Create the Policy table
CREATE TABLE Policy (
    Id nvarchar(255) PRIMARY KEY,
    Name nvarchar(255),
    Description nvarchar(max),
    DeviceId nvarchar(255) REFERENCES Device(Id)
);
GO