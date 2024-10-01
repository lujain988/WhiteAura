create database WhiteAura
use WhiteAura

-- Users Table
CREATE TABLE Users (
    ID INT PRIMARY KEY IDENTITY,
    FullName NVARCHAR(MAX),
    [Password] NVARCHAR(MAX),
    ConifrmPassword NVARCHAR(MAX),
    Email NVARCHAR(30) UNIQUE,
    PhoneNumber NVARCHAR(20),
    CreatedAt DATETIME DEFAULT GETDATE()
);

-- Services Table
CREATE TABLE Services (
    ID INT PRIMARY KEY IDENTITY,
    ServiceName NVARCHAR(MAX),
    Description TEXT,
    Price DECIMAL(10, 2),
    CreatedAt DATETIME DEFAULT GETDATE()
);

-- Bookings Table
CREATE TABLE Bookings (
    ID INT PRIMARY KEY IDENTITY,
    UserID INT REFERENCES Users(ID),
    ServiceID INT REFERENCES Services(ID),
    BookingDate DATE,
    Status NVARCHAR(MAX) DEFAULT 'pending',
    CreatedAt DATETIME DEFAULT GETDATE()
);

-- Payments Table
CREATE TABLE Payments (
    ID INT PRIMARY KEY IDENTITY,
    BookingID INT REFERENCES Bookings(ID),
    Amount DECIMAL(10, 2),
    PaymentDate DATETIME DEFAULT GETDATE(),
    PaymentMethod NVARCHAR(MAX),
    Status NVARCHAR(MAX) DEFAULT 'pending'
);

-- Plans Table
CREATE TABLE Plans (
    ID INT PRIMARY KEY IDENTITY,
    PlanName NVARCHAR(MAX),
    Description TEXT,
    Price DECIMAL(10, 2),
    CreatedAt DATETIME DEFAULT GETDATE()
);
Alter table Plans
Add UserID int references Users(ID)
-- Blog Table
CREATE TABLE Blog (
    ID INT PRIMARY KEY IDENTITY,
    Title NVARCHAR(200),
    Content TEXT,
    AuthorID INT REFERENCES Users(ID),
    CreatedAt DATETIME DEFAULT GETDATE()
);

-- ContactUs Table
CREATE TABLE ContactUs (
    ID INT PRIMARY KEY IDENTITY,
    UserID INT REFERENCES Users(ID),
    [Message] TEXT,
    CreatedAt DATETIME DEFAULT GETDATE()
);
CREATE TABLE Testimonials (
    ID INT PRIMARY KEY IDENTITY(1,1),
    UserId INT REFERENCES Users(ID) ON DELETE CASCADE,
    Text NVARCHAR(MAX),
    CreatedAt DATETIME DEFAULT GETDATE(),
);

ALTER TABLE Testimonials
ADD statues NVARCHAR(50) DEFAULT 'Pending';


  CREATE TABLE Admin (
    AdminID INT PRIMARY KEY IDENTITY(1,1),
   AdminName NVARCHAR(max)  NULL,
    Role nVARCHAR(255) CHECK (Role IN ('SuperAdmin', 'Admin')) DEFAULT 'Admin',
    Email NVARCHAR(max)  ,
    PasswordHash NVARCHAR(max)  NULL
);

  create table Category (
  ID int primary key identity,
  CategoryName NVARCHAR(MAX);
  [Image] NVARCHAR(MAX);
  [Description] Text;
  [Details] NVARCHAR(MAX);)



-- Insert sample data into Users Table
INSERT INTO Users (FullName, [Password], ConifrmPassword, Email, PhoneNumber)
VALUES 
('Lujain Alazzam', 'password123', 'password123', 'lujain@example.com', '0791234567'),
('John Doe', 'johnpass', 'johnpass', 'john.doe@example.com', '0792345678'),
('Jane Smith', 'janesmith', 'janesmith', 'jane.smith@example.com', '0793456789');

-- Insert sample data into Services Table
INSERT INTO Services (ServiceName, Description, Price)
VALUES 
('Massage Therapy', 'Full body massage therapy for relaxation', 50.00),
('Facial Treatment', 'Complete facial skin care treatment', 70.00),
('Yoga Session', 'One-hour yoga session for beginners', 30.00);

-- Insert sample data into Bookings Table
INSERT INTO Bookings (UserID, ServiceID, BookingDate, Status)
VALUES 
(1, 1, '2024-09-10', 'confirmed'),
(2, 2, '2024-09-11', 'pending'),
(3, 3, '2024-09-12', 'confirmed');

-- Insert sample data into Payments Table
INSERT INTO Payments (BookingID, Amount, PaymentMethod, Status)
VALUES 
(1, 50.00, 'Credit Card', 'completed'),
(2, 70.00, 'PayPal', 'pending'),
(3, 30.00, 'Credit Card', 'completed');

-- Insert sample data into Plans Table
INSERT INTO Plans (PlanName, Description, Price)
VALUES 
('Basic Plan', 'Basic plan with limited services', 100.00),
('Premium Plan', 'Premium plan with all-inclusive services', 200.00),
('VIP Plan', 'VIP plan with exclusive services', 300.00);

-- Insert sample data into Blog Table
INSERT INTO Blog (Title, Content, AuthorID)
VALUES 
('The Benefits of Massage Therapy', 'Massage therapy can help reduce stress...', 1),
('Why You Should Try a Facial Treatment', 'Facial treatments can rejuvenate your skin...', 2),
('Getting Started with Yoga', 'Yoga is a great way to improve your flexibility...', 3);

-- Insert sample data into ContactUs Table
INSERT INTO ContactUs (UserID, [Message])
VALUES 
(1, 'I have a question about your services.'),
(2, 'Can I reschedule my booking?'),
(3, 'Do you offer group yoga sessions?');
