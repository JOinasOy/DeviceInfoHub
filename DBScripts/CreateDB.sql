/*
    SQL Script for Creating Database and Tables for DeviceInfoHub

    This script is designed to set up the database for the DeviceInfoHub application. 
    It includes the creation of the 'deviceinfohub' database and the necessary tables to store company, user, and device information.

    The script performs the following operations:
    1. Creates a new database named 'deviceinfohub'.
    2. Defines and creates tables for storing data related to companies, users, devices, applications, and policies.
    3. Establishes primary key constraints for each table and foreign key relationships between them to maintain data integrity.

    Tables created:
    - Company: Stores information about companies.
    - Users: Contains user details and links to their respective companies.
    - Device: Holds information about devices, linked to users and companies.
    - Application: Stores details about applications installed on devices.
    - Policy: Contains policy information applied to devices.
*/

-- Create a new database
CREATE DATABASE deviceinfohub;
GO

-- Switch to the new database
USE deviceinfohub;
GO

-- Create the Company table
CREATE TABLE Company (
    Id INT IDENTITY(1,1) NOT NULL,
    Name nvarchar(50),
    ClientId NVARCHAR(70),
    TenantId NVARCHAR(70),
    ClientSecret NVARCHAR(70),
    KandjiURL NVARCHAR(100),
    KandjiApiKey NVARCHAR(70),
    LastUpdated datetime,
    Archived BIT,
    PRIMARY KEY (Id)
);
GO

-- Create the Users table
CREATE TABLE Users (
    Id INT IDENTITY(1,1) NOT NULL,
    UserId nvarchar(50),
    CompanyId int REFERENCES Company(Id) NOT NULL,
    DisplayName nvarchar(100),
    UserPrincipalName nvarchar(100),
    GivenName NVARCHAR(100),
    Email NVARCHAR(150),
    Department NVARCHAR(100),
    LastUpdated datetime,
    Archived BIT,
    PRIMARY KEY (Id)
);
GO

-- Create the Device table
CREATE TABLE Device (
    Id INT IDENTITY(1,1) NOT NULL,
    UserId int REFERENCES Users(Id) NOT NULL,
    DeviceId nvarchar(50),
    CompanyId int REFERENCES Company(Id) NOT NULL,
    DeviceName nvarchar(50),
    FirstEnrollment datetime,
    LastEnrollment datetime,
    Platform nvarchar(50),
    OsVersion nvarchar(50),
    LastSyncDateTime datetime,
    Model nvarchar(255),
    Manufacturer nvarchar(50),
    SerialNumber nvarchar(30),
    TotalStorageSpaceInBytes BIGINT,
    FreeStorageSpaceInBytes BIGINT,
    PhysicalMemoryInBytes BIGINT,
    DBLastUpdated datetime,
    DBLastUpdatedDesc nvarchar(255),
    Source nvarchar(50),
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