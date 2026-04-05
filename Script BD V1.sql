CREATE DATABASE AgendaContactosDB;
GO

USE AgendaContactosDB;
GO

-- =========================
-- TABLA: Contacto
-- =========================
CREATE TABLE Contacto (
    ContactoID INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(100) NOT NULL,
    Apellido NVARCHAR(100) NULL,
    EsFavorito BIT NOT NULL DEFAULT 0,
    FechaCreacion DATETIME2 NOT NULL DEFAULT GETDATE(),
    IsActive BIT NOT NULL DEFAULT 1,

    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2 NULL
);
GO

-- =========================
-- TABLA: Telefono
-- =========================
CREATE TABLE Telefono (
    TelefonoID INT IDENTITY(1,1) PRIMARY KEY,
    ContactoID INT NOT NULL,
    Numero NVARCHAR(20) NOT NULL,
    Tipo NVARCHAR(50) NULL,

    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),

    CONSTRAINT CK_Telefono_Numero CHECK (LEN(Numero) >= 8)
);
GO

-- =========================
-- TABLA: Correo
-- =========================
CREATE TABLE Correo (
    CorreoID INT IDENTITY(1,1) PRIMARY KEY,
    ContactoID INT NOT NULL,
    Email NVARCHAR(150) NOT NULL,

    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),

    CONSTRAINT UQ_Correo_Email UNIQUE (Email),
    CONSTRAINT CK_Correo_Email CHECK (Email LIKE '%_@_%._%')
);
GO

-- =========================
-- TABLA: Direccion
-- =========================
CREATE TABLE Direccion (
    DireccionID INT IDENTITY(1,1) PRIMARY KEY,
    ContactoID INT NOT NULL,
    Provincia NVARCHAR(100) NULL,
    Canton NVARCHAR(100) NULL,
    Distrito NVARCHAR(100) NULL,
    DireccionExacta NVARCHAR(300) NULL,

    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE()
);
GO

-- =========================
-- TABLA: Nota
-- =========================
CREATE TABLE Nota (
    NotaID INT IDENTITY(1,1) PRIMARY KEY,
    ContactoID INT NOT NULL,
    Contenido NVARCHAR(MAX) NOT NULL,

    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE()
);
GO

-- =========================
-- TABLA: Etiqueta
-- =========================
CREATE TABLE Etiqueta (
    EtiquetaID INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(100) NOT NULL,

    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),

    CONSTRAINT UQ_Etiqueta_Nombre UNIQUE (Nombre)
);
GO

-- =========================
-- TABLA: ContactoEtiqueta (Relación N:M)
-- =========================
CREATE TABLE ContactoEtiqueta (
    ContactoID INT NOT NULL,
    EtiquetaID INT NOT NULL,

    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),

    PRIMARY KEY (ContactoID, EtiquetaID)
);
GO

--RELACIONES (FOREIGN KEYS)

-- Telefono → Contacto
ALTER TABLE Telefono
ADD CONSTRAINT FK_Telefono_Contacto
FOREIGN KEY (ContactoID) REFERENCES Contacto(ContactoID);

-- Correo → Contacto
ALTER TABLE Correo
ADD CONSTRAINT FK_Correo_Contacto
FOREIGN KEY (ContactoID) REFERENCES Contacto(ContactoID);

-- Direccion → Contacto
ALTER TABLE Direccion
ADD CONSTRAINT FK_Direccion_Contacto
FOREIGN KEY (ContactoID) REFERENCES Contacto(ContactoID);

-- Nota → Contacto
ALTER TABLE Nota
ADD CONSTRAINT FK_Nota_Contacto
FOREIGN KEY (ContactoID) REFERENCES Contacto(ContactoID);

-- ContactoEtiqueta → Contacto
ALTER TABLE ContactoEtiqueta
ADD CONSTRAINT FK_ContactoEtiqueta_Contacto
FOREIGN KEY (ContactoID) REFERENCES Contacto(ContactoID);

-- ContactoEtiqueta → Etiqueta
ALTER TABLE ContactoEtiqueta
ADD CONSTRAINT FK_ContactoEtiqueta_Etiqueta
FOREIGN KEY (EtiquetaID) REFERENCES Etiqueta(EtiquetaID);
GO

--ÍNDICES (RENDIMIENTO)
-- Búsqueda por nombre (importante para tu buscador)
CREATE INDEX IX_Contacto_Nombre
ON Contacto (Nombre, Apellido);

-- Teléfonos por contacto
CREATE INDEX IX_Telefono_ContactoID
ON Telefono (ContactoID);

-- Correos por contacto
CREATE INDEX IX_Correo_ContactoID
ON Correo (ContactoID);

-- Direcciones por provincia (filtro futuro)
CREATE INDEX IX_Direccion_Provincia
ON Direccion (Provincia);

-- Etiquetas
CREATE INDEX IX_ContactoEtiqueta_Contacto
ON ContactoEtiqueta (ContactoID);

CREATE INDEX IX_ContactoEtiqueta_Etiqueta
ON ContactoEtiqueta (EtiquetaID);
GO