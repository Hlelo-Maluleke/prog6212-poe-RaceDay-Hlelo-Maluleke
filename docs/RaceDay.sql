--CREATE DATABASE RACEDAYDB;

--CREATE TABLE USERS (
--    userID INT IDENTITY(1,1) PRIMARY KEY,
--    email VARCHAR(255) UNIQUE NOT NULL,
--    passwordHash VARCHAR(255) NOT NULL,
--    fullName VARCHAR(255) NOT NULL,
--    phone VARCHAR(50),
--    created_at DATETIME DEFAULT GETDATE(),
--    updated_at DATETIME DEFAULT GETDATE()
--);

--CREATE TABLE ORGANISERS (
--    organiserID INT IDENTITY(1,1) PRIMARY KEY,
--    userID INT NOT NULL UNIQUE REFERENCES USERS(userID) ON DELETE CASCADE,
--    companyName VARCHAR(255),
--    created_at DATETIME DEFAULT GETDATE(),
--    updated_at DATETIME DEFAULT GETDATE()
--);

--CREATE TABLE PARTICIPANTS (
--    participantID INT IDENTITY(1,1) PRIMARY KEY,
--    userID INT NOT NULL UNIQUE REFERENCES USERS(userID) ON DELETE CASCADE,
--    dateOfBirth DATE,
--    idNumber VARCHAR(20),
--    emergencyContact VARCHAR(100),
--    created_at DATETIME DEFAULT GETDATE(),
--    updated_at DATETIME DEFAULT GETDATE()
--);

--CREATE TABLE events (
--    eventID INT IDENTITY(1,1) PRIMARY KEY,
--    organiserID INT NOT NULL REFERENCES ORGANISERS(organiserID) ON DELETE CASCADE,
--    name VARCHAR(255) NOT NULL,
--    description TEXT,
--    eventDate DATE NOT NULL,
--    startTime TIME,
--    location VARCHAR(255) NOT NULL,
--    eventType VARCHAR(20) NOT NULL CHECK (eventType IN ('run', 'walk', 'cycle')),
--    status VARCHAR(20) DEFAULT 'draft' CHECK (status IN ('draft', 'published', 'cancelled', 'completed')),
--    created_at DATETIME DEFAULT GETDATE(),
--    updated_at DATETIME DEFAULT GETDATE()
--);

CREATE TABLE CATEGORIES (
    categoryID INT IDENTITY(1,1) PRIMARY KEY,
    eventID INT NOT NULL REFERENCES events(eventID) ON DELETE CASCADE,
    name VARCHAR(100) NOT NULL,
    description TEXT,
    distanceKm DECIMAL(5,2) NOT NULL,
    entryFee DECIMAL(10,2) DEFAULT 0.00,
    maxParticipants INT,
    startTime TIME,
    created_at DATETIME DEFAULT GETDATE(),
    updated_at DATETIME DEFAULT GETDATE(),
    CONSTRAINT unique_event_category_name UNIQUE (eventID, name)
);