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

--CREATE TABLE CATEGORIES (
--    categoryID INT IDENTITY(1,1) PRIMARY KEY,
--    eventID INT NOT NULL REFERENCES events(eventID) ON DELETE CASCADE,
--    name VARCHAR(100) NOT NULL,
--    description TEXT,
--    distanceKm DECIMAL(5,2) NOT NULL,
--    entryFee DECIMAL(10,2) DEFAULT 0.00,
--    maxParticipants INT,
--    startTime TIME,
--    created_at DATETIME DEFAULT GETDATE(),
--    updated_at DATETIME DEFAULT GETDATE(),
--    CONSTRAINT unique_event_category_name UNIQUE (eventID, name)
--);

--CREATE TABLE ROUTES (
--    routeID INT IDENTITY(1,1) PRIMARY KEY,
--    categoryID INT NOT NULL UNIQUE REFERENCES CATEGORIES(categoryID) ON DELETE CASCADE,
--    mapUrl VARCHAR(255),
--    elevationGain INT,  -- in meters
--    description TEXT,
--    created_at DATETIME DEFAULT GETDATE(),
--    updated_at DATETIME DEFAULT GETDATE()
--);

--CREATE TABLE ENROLMENTS (
--    enrolmentID INT IDENTITY(1,1) PRIMARY KEY,
--    participantID INT NOT NULL REFERENCES PARTICIPANTS(participantID),
--    categoryID INT NOT NULL REFERENCES CATEGORIES(categoryID),
--    enrolmentDate DATETIME DEFAULT GETDATE(),
--    status VARCHAR(20) DEFAULT 'pending' CHECK (status IN ('pending', 'confirmed', 'withdrawn')),
--    created_at DATETIME DEFAULT GETDATE(),
--    updated_at DATETIME DEFAULT GETDATE(),
--    CONSTRAINT unique_participant_category UNIQUE (participantID, categoryID)
--);

--CREATE TABLE results (
--    resultID INT IDENTITY(1,1) PRIMARY KEY,
--    enrolmentID INT NOT NULL UNIQUE REFERENCES enrolments(enrolmentID) ON DELETE CASCADE,
--    finishTimeSeconds INT,
--    position INT,
--    created_at DATETIME DEFAULT GETDATE(),
--    updated_at DATETIME DEFAULT GETDATE()
--);

-- SAMPLE DATA SEEDING
-- 1. USERS (Base accounts for all users)
INSERT INTO USERS (email, passwordHash, fullName, phone) VALUES
    ('thabo@runningclub.co.za', '$2b$12$KIXxWq9XZ5wY2vZ3c4dE5fG6h7i8j9k0l1m2n3o4p5q6r7s8t9u0v1w2x3', 'Thabo Mokoena', '082 123 4567'),
    ('susan@capetowncycles.co.za', '$2b$12$KIXxWq9XZ5wY2vZ3c4dE5fG6h7i8j9k0l1m2n3o4p5q6r7s8t9u0v1w2x3', 'Susan Klein', '083 987 6543'),
    ('lindiwe@runsa.co.za', '$2b$12$KIXxWq9XZ5wY2vZ3c4dE5fG6h7i8j9k0l1m2n3o4p5q6r7s8t9u0v1w2x3', 'Lindiwe Nkosi', '076 555 1234'),
    ('michael@cyclezone.co.za', '$2b$12$KIXxWq9XZ5wY2vZ3c4dE5fG6h7i8j9k0l1m2n3o4p5q6r7s8t9u0v1w2x3', 'Michael Jacobs', '072 444 5678'),
    ('sipho@fitness.co.za', '$2b$12$KIXxWq9XZ5wY2vZ3c4dE5fG6h7i8j9k0l1m2n3o4p5q6r7s8t9u0v1w2x3', 'Sipho Dlamini', '073 333 9012');