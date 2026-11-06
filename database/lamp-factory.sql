IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'lampdb')
BEGIN
    CREATE DATABASE lampdb;
END

GO
USE lampdb;
GO

CREATE TABLE [User] (
   UserID INT PRIMARY KEY IDENTITY(1,1),
   Username NVARCHAR(50) NOT NULL UNIQUE,
   Userpass NVARCHAR(50) NOT NULL,
   Fullname NVARCHAR(50),
   Phone VARCHAR(13),
   CreatedAt DATETIME DEFAULT GETDATE(),
   CreatedBy INT,
   ModifiedAt DATETIME,
   ModifiedBy INT,
   FOREIGN KEY (CreatedBy) REFERENCES [User](UserID),
   FOREIGN KEY (ModifiedBy) REFERENCES [User](UserID)
)

CREATE TABLE Role (
    RoleID INT PRIMARY KEY IDENTITY(1,1),
    RoleName NVARCHAR(50) NOT NULL,
    [Description] NVARCHAR(MAX),
    CreatedAt DATETIME DEFAULT GETDATE(),
    CreatedBy INT,
    ModifiedAt DATETIME,
    ModifiedBy INT,
    FOREIGN KEY (CreatedBy) REFERENCES [User](UserID),
    FOREIGN KEY (ModifiedBy) REFERENCES [User](UserID)
)

CREATE TABLE Resource (
    ResourceID INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(50) NOT NULL UNIQUE,
    CreatedAt DATETIME DEFAULT GETDATE(),
    CreatedBy INT,
    ModifiedAt DATETIME,
    ModifiedBy INT,
    FOREIGN KEY (CreatedBy) REFERENCES [User](UserID),
    FOREIGN KEY (ModifiedBy) REFERENCES [User](UserID) 
)

CREATE TABLE [Action] (
    ActionID INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(50) NOT NULL UNIQUE,
    CreatedAt DATETIME DEFAULT GETDATE(),
    CreatedBy INT,
    ModifiedAt DATETIME,
    ModifiedBy INT,
    FOREIGN KEY (CreatedBy) REFERENCES [User](UserID),
    FOREIGN KEY (ModifiedBy) REFERENCES [User](UserID)
)

CREATE TABLE Permission (
    PermissionID INT IDENTITY(1,1) PRIMARY KEY,
    ResourceID INT NOT NULL,
    ActionID INT NOT NULL,
    FOREIGN KEY (ResourceID) REFERENCES Resource(ResourceID),
    FOREIGN KEY (ActionID) REFERENCES [Action](ActionID),
    CreatedAt DATETIME DEFAULT GETDATE(),
    CreatedBy INT,
    ModifiedAt DATETIME,
    ModifiedBy INT,
    FOREIGN KEY (CreatedBy) REFERENCES [User](UserID),
    FOREIGN KEY (ModifiedBy) REFERENCES [User](UserID)
)

CREATE TABLE UserRoles (
    UserID INT NOT NULL,
    RoleID INT NOT NULL,
    PRIMARY KEY (UserID, RoleID),
    FOREIGN KEY (UserID) REFERENCES [User](UserID) ON DELETE CASCADE,
    FOREIGN KEY (RoleID) REFERENCES Role(RoleID) ON DELETE CASCADE
)

CREATE TABLE RolePermissions (
    RoleID INT NOT NULL,
    PermissionID INT NOT NULL,
    PRIMARY KEY (RoleID, PermissionId),
    FOREIGN KEY (RoleID) REFERENCES [Role](RoleID) ON DELETE CASCADE,
    FOREIGN KEY (PermissionID) REFERENCES Permission(PermissionID) ON DELETE CASCADE
)

CREATE TABLE Product (
    ProductID INT PRIMARY KEY IDENTITY(1,1),
    ProductType NVARCHAR(30) NOT NULL,
    ProductColor NVARCHAR(30),
    Material NVARCHAR(200),
    StandardNumber NVARCHAR(100),
    [Weight] FLOAT,
    Voltage FLOAT,
    [Power] FLOAT,
    LampholderType NVARCHAR(30),
    ConsumptionUnit NVARCHAR(20),
    CreatedAt DATETIME DEFAULT GETDATE(),
    CreatedBy INT,
    ModifiedAt DATETIME,
    ModifiedBy INT,
    FOREIGN KEY (CreatedBy) REFERENCES [User](UserID),
    FOREIGN KEY (ModifiedBy) REFERENCES [User](UserID)
)


CREATE TABLE BOM (
    BOM_ID INT PRIMARY KEY IDENTITY(1,1),
    ProductID INT NOT NULL,
    FOREIGN KEY (ProductID) REFERENCES Product(ProductID),
    CreatedAt DATETIME DEFAULT GETDATE(),
    CreatedBy INT,
    ModifiedAt DATETIME,
    ModifiedBy INT,
    FOREIGN KEY (CreatedBy) REFERENCES [User](UserID),
    FOREIGN KEY (ModifiedBy) REFERENCES [User](UserID)
)

CREATE TABLE BOMComponent (
    ProductID INT NOT NULL,
    BOM_ID INT NOT NULL,
    ConsumptionAmount FLOAT,
    PRIMARY KEY (ProductID, BOM_ID),
    FOREIGN KEY (ProductID) REFERENCES Product(ProductID),
    FOREIGN KEY (BOM_ID) REFERENCES BOM(BOM_ID)
)

CREATE TABLE Warehouse (
    WarehouseID INT PRIMARY KEY IDENTITY(1,1),
    WarehouseType NVARCHAR(30) NOT NULL,
    WarehouseName NVARCHAR(30),
    WarehouseAddress NVARCHAR(MAX),
    CreatedAt DATETIME DEFAULT GETDATE(),
    CreatedBy INT,
    ModifiedAt DATETIME,
    ModifiedBy INT,
    FOREIGN KEY (CreatedBy) REFERENCES [User](UserID),
    FOREIGN KEY (ModifiedBy) REFERENCES [User](UserID)
)

CREATE TABLE WarehouseProduct (
    WarehouseID INT NOT NULL,
    ProductID INT NOT NULL,
    Quantity INT,
    PRIMARY KEY (WarehouseID, ProductID),
    FOREIGN KEY (WarehouseID) REFERENCES Warehouse(WarehouseID),
    FOREIGN KEY (ProductID) REFERENCES Product(ProductID)
)

CREATE TABLE ProductionPlan (
    ProductionPlanID INT PRIMARY KEY IDENTITY(1,1),
    IssueDate Date,
    TargetDate Date,
    [Priority] INT,
    CreatedAt DATETIME DEFAULT GETDATE(),
    CreatedBy INT,
    ModifiedAt DATETIME,
    ModifiedBy INT,
    FOREIGN KEY (CreatedBy) REFERENCES [User](UserID),
    FOREIGN KEY (ModifiedBy) REFERENCES [User](UserID)
)

CREATE TABLE ProductionPlanProduct (
    ProductionPlanID INT NOT NULL,
    ProductID INT NOT NULL,
    MinQuantity INT,
    MaxQuantity INT,
    PRIMARY KEY (ProductionPlanID, ProductID),
    FOREIGN KEY (ProductionPlanID) REFERENCES ProductionPlan(ProductionPlanID),
    FOREIGN KEY (ProductID) REFERENCES Product(ProductID)
)

CREATE TABLE ProductionLine (
    ProductionLineID INT PRIMARY KEY IDENTITY(1,1),
    [Status] BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME DEFAULT GETDATE(),
    CreatedBy INT,
    ModifiedAt DATETIME,
    ModifiedBy INT,
    FOREIGN KEY (CreatedBy) REFERENCES [User](UserID),
    FOREIGN KEY (ModifiedBy) REFERENCES [User](UserID)
)

CREATE TABLE ProductionDocument (
    ProductionDocumentID INT PRIMARY KEY IDENTITY(1,1),
    ProductionDate DATE,
    ProducedQuantity INT,
    RejectedQuantity INT,
    ProductionShift INT,
    ProductionPlanID INT NOT NULL,
    FOREIGN KEY (ProductionPlanID) REFERENCES ProductionPlan(ProductionPlanID),
    ProductID INT NOT NULL,
    FOREIGN KEY (ProductID) REFERENCES Product(ProductID),
    ProductionLineID INT NOT NULL,
    FOREIGN KEY (ProductionLineID) REFERENCES ProductionLine(ProductionLineID),
    CreatedAt DATETIME DEFAULT GETDATE(),
    CreatedBy INT,
    ModifiedAt DATETIME,
    ModifiedBy INT,
    FOREIGN KEY (CreatedBy) REFERENCES [User](UserID),
    FOREIGN KEY (ModifiedBy) REFERENCES [User](UserID)
)

CREATE TABLE Operator (
    OperatorID INT PRIMARY KEY IDENTITY(1,1),
    NationalID NVARCHAR(30) UNIQUE NOT NULL,
    FirstName NVARCHAR(100),
    LastName NVARCHAR(100),
    HiringDate DATE,
    CreatedAt DATETIME DEFAULT GETDATE(),
    CreatedBy INT,
    ModifiedAt DATETIME,
    ModifiedBy INT,
    FOREIGN KEY (CreatedBy) REFERENCES [User](UserID),
    FOREIGN KEY (ModifiedBy) REFERENCES [User](UserID)
)

CREATE TABLE ProductionDocumentOperator (
    ProductionDocumentID INT NOT NULL,
    OperatorID INT NOT NULL,
    OperatorRole NVARCHAR(50),
    AssignedDateFrom Date,
    AssignedDateUntil Date,
    PRIMARY KEY (ProductionDocumentID, OperatorID),
    FOREIGN KEY (ProductionDocumentID) REFERENCES ProductionDocument(ProductionDocumentID),
    FOREIGN KEY (OperatorID) REFERENCES Operator(OperatorID)
)

CREATE TABLE LineStop (
    LineStopID INT PRIMARY KEY IDENTITY(1,1),
    LineStopDate DATE,
    Reason NVARCHAR(MAX),
    ProductionDocumentID INT NOT NULL,
    FOREIGN KEY (ProductionDocumentID) REFERENCES ProductionDocument(ProductionDocumentID),
    CreatedAt DATETIME DEFAULT GETDATE(),
    CreatedBy INT,
    ModifiedAt DATETIME,
    ModifiedBy INT,
    FOREIGN KEY (CreatedBy) REFERENCES [User](UserID),
    FOREIGN KEY (ModifiedBy) REFERENCES [User](UserID)
)

CREATE TABLE QualityControl (
    QualityControlID INT PRIMARY KEY IDENTITY(1,1),
    QualityControlType NVARCHAR(30) NOT NULL,
    InspectionType NVARCHAR(100),
    SampleRatio FLOAT,
    BatchNumber NVARCHAR(100) UNIQUE,
    QualityControlDate DATE,
    QualityControlAcceptance BIT,
    ProductionDocumentID INT NOT NULL,
    FOREIGN KEY (ProductionDocumentID) REFERENCES ProductionDocument(ProductionDocumentID),
    CreatedAt DATETIME DEFAULT GETDATE(),
    CreatedBy INT,
    ModifiedAt DATETIME,
    ModifiedBy INT,
    FOREIGN KEY (CreatedBy) REFERENCES [User](UserID),
    FOREIGN KEY (ModifiedBy) REFERENCES [User](UserID)
)

CREATE TABLE SalesVoucher (
    SalesVoucherID INT PRIMARY KEY IDENTITY(1,1),
    WarehouseID INT NOT NULL,
    FOREIGN KEY (WarehouseID) REFERENCES Warehouse(WarehouseID),
    CreatedAt DATETIME DEFAULT GETDATE(),
    CreatedBy INT,
    ModifiedAt DATETIME,
    ModifiedBy INT,
    FOREIGN KEY (CreatedBy) REFERENCES [User](UserID),
    FOREIGN KEY (ModifiedBy) REFERENCES [User](UserID)
)

CREATE TABLE SalesVoucherProduct (
    SalesVoucherID INT NOT NULL,
    ProductID INT NOT NULL,
    SalesQuantity INT,
    PRIMARY KEY (SalesVoucherID, ProductID),
    FOREIGN KEY (SalesVoucherID) REFERENCES SalesVoucher(SalesVoucherID),
    FOREIGN KEY (ProductID) REFERENCES Product(ProductID)
)

CREATE TABLE PurchaseVoucher (
    PurchaseVoucherID INT PRIMARY KEY IDENTITY(1,1),
    PurchaseDate DATE,
    WarehouseID INT NOT NULL,
    FOREIGN KEY (WarehouseID) REFERENCES Warehouse(WarehouseID),
    CreatedAt DATETIME DEFAULT GETDATE(),
    CreatedBy INT,
    ModifiedAt DATETIME,
    ModifiedBy INT,
    FOREIGN KEY (CreatedBy) REFERENCES [User](UserID),
    FOREIGN KEY (ModifiedBy) REFERENCES [User](UserID)
)

CREATE TABLE PurchaseVoucherProduct (
    PurchaseVoucherID INT NOT NULL,
    ProductID INT NOT NULL,
    PurchasedQuantity INT,
    PRIMARY KEY (PurchaseVoucherID, ProductID),
    FOREIGN KEY (PurchaseVoucherID) REFERENCES PurchaseVoucher(PurchaseVoucherID),
    FOREIGN KEY (ProductID) REFERENCES Product(ProductID)
)

CREATE TABLE ChargeVoucher (
    ChargeVoucherID INT PRIMARY KEY IDENTITY(1,1),
    ChargeDate DATE,
    WarehouseID INT NOT NULL,
    ProductionDocumentID INT NOT NULL,
    FOREIGN KEY (WarehouseID) REFERENCES Warehouse(WarehouseID),
    FOREIGN KEY (ProductionDocumentID) REFERENCES ProductionDocument(ProductionDocumentID),
    CreatedAt DATETIME DEFAULT GETDATE(),
    CreatedBy INT,
    ModifiedAt DATETIME,
    ModifiedBy INT,
    FOREIGN KEY (CreatedBy) REFERENCES [User](UserID),
    FOREIGN KEY (ModifiedBy) REFERENCES [User](UserID)
)

CREATE TABLE ChargeVoucherProduct (
    ChargeVoucherID INT NOT NULL,
    ProductID INT NOT NULL,
    ChargeQuantity INT,
    PRIMARY KEY (ChargeVoucherID, ProductID),
    FOREIGN KEY (ChargeVoucherID) REFERENCES ChargeVoucher(ChargeVoucherID),
    FOREIGN KEY (ProductID) REFERENCES Product(ProductID)
)

CREATE TABLE RequestVoucher (
    RequestVoucherID INT PRIMARY KEY IDENTITY(1,1),
    RequestDate DATE,
    WarehouseID INT NOT NULL,
    ProductionDocumentID INT NOT NULL,
    FOREIGN KEY (WarehouseID) REFERENCES Warehouse(WarehouseID),
    FOREIGN KEY (ProductionDocumentID) REFERENCES ProductionDocument(ProductionDocumentID),
    CreatedAt DATETIME DEFAULT GETDATE(),
    CreatedBy INT,
    ModifiedAt DATETIME,
    ModifiedBy INT,
    FOREIGN KEY (CreatedBy) REFERENCES [User](UserID),
    FOREIGN KEY (ModifiedBy) REFERENCES [User](UserID)
)

CREATE TABLE RequestVoucherProduct (
    RequestVoucherID INT NOT NULL,
    ProductID INT NOT NULL,
    RequestedQuantity INT,
    PRIMARY KEY (RequestVoucherID, ProductID),
    FOREIGN KEY (RequestVoucherID) REFERENCES RequestVoucher(RequestVoucherID),
    FOREIGN KEY (ProductID) REFERENCES Product(ProductID)
)

CREATE TABLE RefreshToken (
    TokenID INT IDENTITY(1,1) PRIMARY KEY,
    Token VARCHAR(200) NOT NULL UNIQUE,
    CreatedAt DATETIME DEFAULT GETDATE(),
    ExpiryDate DATETIME NOT NULL,
    IsRevoked BIT DEFAULT 0,
    UserID INT NOT NULL,
    CONSTRAINT FK_RefreshToken_User FOREIGN KEY (UserID) REFERENCES [User](UserID) ON DELETE CASCADE
)