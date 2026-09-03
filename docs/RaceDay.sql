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

-- SAMPLE DATA
-- 1. USERS (Base accounts for all users)
--INSERT INTO USERS (email, passwordHash, fullName, phone) VALUES
--    ('thabo@runningclub.co.za', '$2b$12$KIXxWq9XZ5wY2vZ3c4dE5fG6h7i8j9k0l1m2n3o4p5q6r7s8t9u0v1w2x3', 'Thabo Mokoena', '082 123 4567'),
--    ('susan@capetowncycles.co.za', '$2b$12$KIXxWq9XZ5wY2vZ3c4dE5fG6h7i8j9k0l1m2n3o4p5q6r7s8t9u0v1w2x3', 'Susan Klein', '083 987 6543'),
--    ('lindiwe@runsa.co.za', '$2b$12$KIXxWq9XZ5wY2vZ3c4dE5fG6h7i8j9k0l1m2n3o4p5q6r7s8t9u0v1w2x3', 'Lindiwe Nkosi', '076 555 1234'),
--    ('michael@cyclezone.co.za', '$2b$12$KIXxWq9XZ5wY2vZ3c4dE5fG6h7i8j9k0l1m2n3o4p5q6r7s8t9u0v1w2x3', 'Michael Jacobs', '072 444 5678'),
--    ('sipho@fitness.co.za', '$2b$12$KIXxWq9XZ5wY2vZ3c4dE5fG6h7i8j9k0l1m2n3o4p5q6r7s8t9u0v1w2x3', 'Sipho Dlamini', '073 333 9012');

-- 2. ORGANISERS (Pre-created - only these can create events)
--INSERT INTO ORGANISERS (userID, companyName) VALUES
--    (1, 'Soweto Running Club'),    -- Thabo
--    (2, 'Cape Town Cycle Tours');  -- Susan

-- 3. PARTICIPANTS (Self-registered users)
--INSERT INTO participants (userID, dateOfBirth, idNumber, emergencyContact) VALUES
--    (3, '1985-06-15', '8506151234089', 'Sipho Mokoena - 082 111 2222'),  -- Lindiwe
--    (4, '1992-11-22', '9211225678012', 'Sarah Jacobs - 083 444 5555'),   -- Michael
--    (5, '1998-03-01', '9803019012345', 'Nomsa Dlamini - 076 666 7777');  -- Sipho

-- 4. EVENTS (3 Events managed by the 2 Organisers)
--INSERT INTO EVENTS (organiserID, name, description, eventDate, startTime, location, eventType, status) VALUES
--    (1, 'Soweto Marathon', 'One of South Africa''s most iconic road races through the heart of Soweto. Includes 10km, 21km, and 42km categories.', '2026-11-15', '06:00:00', 'Soweto, Johannesburg', 'run', 'published'),
--    (2, 'Cape Town Cycle Tour', 'The world''s largest timed cycle race. Stunning route along Chapman''s Peak and Cape Peninsula.', '2026-03-08', '07:30:00', 'Cape Town CBD', 'cycle', 'published'),
--    (1, 'Durban Community Parkrun', 'Weekly free community walking/running event at Durban Botanic Gardens. Open to all ages and fitness levels.', '2026-10-25', '08:00:00', 'Durban Botanic Gardens', 'walk', 'published');

-- 5. CATEGORIES (Categories for each event, with distances)
-- Soweto Marathon Categories (Event 1)
--INSERT INTO CATEGORIES (eventID, name, description, distanceKm, entryFee, maxParticipants, startTime) VALUES
--    (1, '10km Road Run', 'Short distance category for beginners and fun-runners', 10.00, 150.00, 500, '06:15:00'),
--    (1, '21km Half Marathon', 'Intermediate distance for competitive runners', 21.10, 250.00, 300, '06:00:00'),
--    (1, '42km Full Marathon', 'The ultimate challenge for endurance athletes', 42.20, 350.00, 200, '06:00:00');

-- Cape Town Cycle Tour Categories (Event 2)
--INSERT INTO CATEGORIES (eventID, name, description, distanceKm, entryFee, maxParticipants, startTime) VALUES
--    (2, '40km Fun Ride', 'Casual cycle for families and beginners', 40.00, 100.00, 1000, '08:00:00'),
--    (2, '109km Classic', 'Full Cape Town Cycle Tour experience', 109.00, 300.00, 1500, '07:30:00');


-- Durban Community Parkrun Categories (Event 3 - FREE event!)
--INSERT INTO CATEGORIES (eventID, name, description, distanceKm, entryFee, maxParticipants, startTime) VALUES
--    (3, '5km Community Walk', 'Free family-friendly 5km walk', 5.00, 0.00, 200, '08:00:00'),
--    (3, '5km Community Run', 'Free 5km run for fitness enthusiasts', 5.00, 0.00, 150, '08:00:00');

-- 6. ROUTES (Route information for specific categories)
--INSERT INTO ROUTES (categoryID, mapUrl, elevationGain, description) VALUES
--    (2, 'https://maps.example.com/soweto-21km-route', 180, 'Scenic route through Soweto landmarks including Mandela House and Orlando Stadium'),
--    (5, 'https://maps.example.com/ctct-109km-route', 850, 'Challenging route with Chapman''s Peak Drive and Constantia climbs'),
--    (6, 'https://maps.example.com/durban-5km-route', 25, 'Flat, family-friendly route through Durban Botanic Gardens');

-- 7. ENROLMENTS (Sample participant entries)
-- Lindiwe (Participant ID 1) enrolments
--INSERT INTO ENROLMENTS (participantID, categoryID, status) VALUES
--    (1, 2, 'confirmed'),  -- Soweto Half Marathon
--    (1, 5, 'confirmed'),  -- CTCT 109km Classic
--    (1, 6, 'confirmed');  -- Durban 5km Walk (free)

-- Michael (Participant ID 2) enrolments
--INSERT INTO ENROLMENTS (participantID, categoryID, status) VALUES
--    (2, 1, 'confirmed'),  -- Soweto 10km
--    (2, 5, 'confirmed'),  -- CTCT 109km Classic
--    (2, 7, 'pending');    -- Durban 5km Run (pending approval)

-- Sipho (Participant ID 3) enrolments
INSERT INTO ENROLMENTS (participantID, categoryID, status) VALUES
    (3, 3, 'confirmed'),  -- Soweto Full Marathon
    (3, 4, 'confirmed');  -- CTCT 40km Fun Ride