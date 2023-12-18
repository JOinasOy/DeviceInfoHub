-- Create a new database
CREATE DATABASE deviceinfohub;
GO

-- Switch to the new database
USE deviceinfohub;
GO

CREATE TABLE Company (
    Id INT IDENTITY(1,1) NOT NULL,
    Name nvarchar(50),
    ClientId NVARCHAR(70),
    TenantId NVARCHAR(70),
    ClientSecret NVARCHAR(70),
    KandjiApiKey NVARCHAR(70),
    LastUpdated datetime,
    Archived BIT,
    PRIMARY KEY (Id)
);
GO

-- Create the Device table
CREATE TABLE Device (
    Id INT IDENTITY(1,1) NOT NULL,
    DeviceId nvarchar(50),
    CompanyId int REFERENCES Company(Id),
    EnrolledDateTime datetime,
    OperatingSystem nvarchar(50),
    DisplayName nvarchar(50),
    Model nvarchar(50),
    Manufacturer nvarchar(50),
    SerialNumber nvarchar(30),
    LastUpdated datetime,
    LastUpdatedDesc nvarchar(255),
    Archived BIT,
    PRIMARY KEY (Id)
);
GO

-- Create the Application table
CREATE TABLE Application (
    Id INT IDENTITY(1,1) NOT NULL,
    DisplayName nvarchar(50),
    Publisher nvarchar(50),
    Version nvarchar(50),
    DeviceId int REFERENCES Device(Id),
    LastUpdated datetime,
    Archived BIT,
    PRIMARY KEY (Id)
);
GO

-- Create the Policy table
CREATE TABLE Policy (
    Id INT IDENTITY(1,1) NOT NULL,
    Name nvarchar(50),
    Description nvarchar(max),
    DeviceId int REFERENCES Device(Id),
    LastUpdated datetime,
    Archived BIT,
    PRIMARY KEY (Id)
);
GO