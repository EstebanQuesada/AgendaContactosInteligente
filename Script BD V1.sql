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



--V2---------------------------------------------------------------------------------------------------------------------------------------------------------------
USE AgendaContactosDB;
GO

-- Quitar restricción única global del correo si existe
IF EXISTS (
    SELECT 1
    FROM sys.objects
    WHERE type = 'UQ'
      AND name = 'UQ_Correo_Email'
)
BEGIN
    ALTER TABLE Correo DROP CONSTRAINT UQ_Correo_Email;
END
GO

-- Crear índice único filtrado para correos activos
IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'UX_Correo_Email_Activo'
      AND object_id = OBJECT_ID('dbo.Correo')
)
BEGIN
    CREATE UNIQUE INDEX UX_Correo_Email_Activo
    ON dbo.Correo (Email)
    WHERE IsActive = 1;
END
GO


--usp_Contacto_Create
CREATE OR ALTER PROCEDURE dbo.usp_Contacto_Create
    @Nombre NVARCHAR(100),
    @Apellido NVARCHAR(100) = NULL,
    @EsFavorito BIT = 0
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        SET @Nombre = LTRIM(RTRIM(@Nombre));
        SET @Apellido = NULLIF(LTRIM(RTRIM(@Apellido)), '');

        IF @Nombre IS NULL OR @Nombre = ''
            THROW 50001, 'El nombre del contacto es obligatorio.', 1;

        IF LEN(@Nombre) > 100
            THROW 50002, 'El nombre excede el máximo permitido de 100 caracteres.', 1;

        IF @Apellido IS NOT NULL AND LEN(@Apellido) > 100
            THROW 50003, 'El apellido excede el máximo permitido de 100 caracteres.', 1;

        INSERT INTO dbo.Contacto
        (
            Nombre,
            Apellido,
            EsFavorito,
            FechaCreacion,
            IsActive,
            CreatedAt
        )
        VALUES
        (
            @Nombre,
            @Apellido,
            @EsFavorito,
            SYSDATETIME(),
            1,
            SYSDATETIME()
        );

        DECLARE @NuevoID INT = SCOPE_IDENTITY();

        SELECT
            ContactoID,
            Nombre,
            Apellido,
            EsFavorito,
            FechaCreacion,
            IsActive,
            CreatedAt,
            UpdatedAt
        FROM dbo.Contacto
        WHERE ContactoID = @NuevoID;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END;
GO

--usp_Contacto_Update
CREATE OR ALTER PROCEDURE dbo.usp_Contacto_Update
    @ContactoID INT,
    @Nombre NVARCHAR(100),
    @Apellido NVARCHAR(100) = NULL,
    @EsFavorito BIT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        SET @Nombre = LTRIM(RTRIM(@Nombre));
        SET @Apellido = NULLIF(LTRIM(RTRIM(@Apellido)), '');

        IF @ContactoID IS NULL OR @ContactoID <= 0
            THROW 50010, 'El ContactoID es inválido.', 1;

        IF NOT EXISTS (
            SELECT 1
            FROM dbo.Contacto
            WHERE ContactoID = @ContactoID
              AND IsActive = 1
        )
            THROW 50011, 'El contacto no existe o está inactivo.', 1;

        IF @Nombre IS NULL OR @Nombre = ''
            THROW 50012, 'El nombre del contacto es obligatorio.', 1;

        IF LEN(@Nombre) > 100
            THROW 50013, 'El nombre excede el máximo permitido de 100 caracteres.', 1;

        IF @Apellido IS NOT NULL AND LEN(@Apellido) > 100
            THROW 50014, 'El apellido excede el máximo permitido de 100 caracteres.', 1;

        UPDATE dbo.Contacto
        SET
            Nombre = @Nombre,
            Apellido = @Apellido,
            EsFavorito = @EsFavorito,
            UpdatedAt = SYSDATETIME()
        WHERE ContactoID = @ContactoID;

        SELECT
            ContactoID,
            Nombre,
            Apellido,
            EsFavorito,
            FechaCreacion,
            IsActive,
            CreatedAt,
            UpdatedAt
        FROM dbo.Contacto
        WHERE ContactoID = @ContactoID;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END;
GO

--usp_Contacto_GetById
CREATE OR ALTER PROCEDURE dbo.usp_Contacto_GetById
    @ContactoID INT
AS
BEGIN
    SET NOCOUNT ON;

    IF @ContactoID IS NULL OR @ContactoID <= 0
        THROW 50020, 'El ContactoID es inválido.', 1;

    SELECT
        ContactoID,
        Nombre,
        Apellido,
        EsFavorito,
        FechaCreacion,
        IsActive,
        CreatedAt,
        UpdatedAt
    FROM dbo.Contacto
    WHERE ContactoID = @ContactoID
      AND IsActive = 1;
END;
GO

--usp_Contacto_Delete

CREATE OR ALTER PROCEDURE dbo.usp_Contacto_Delete
    @ContactoID INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        IF @ContactoID IS NULL OR @ContactoID <= 0
            THROW 50030, 'El ContactoID es inválido.', 1;

        IF NOT EXISTS (
            SELECT 1 FROM dbo.Contacto
            WHERE ContactoID = @ContactoID
              AND IsActive = 1
        )
            THROW 50031, 'El contacto no existe o ya está inactivo.', 1;

        UPDATE dbo.Contacto
        SET
            IsActive = 0,
            UpdatedAt = SYSDATETIME()
        WHERE ContactoID = @ContactoID;

        UPDATE dbo.Telefono
        SET IsActive = 0
        WHERE ContactoID = @ContactoID
          AND IsActive = 1;

        UPDATE dbo.Correo
        SET IsActive = 0
        WHERE ContactoID = @ContactoID
          AND IsActive = 1;

        UPDATE dbo.Direccion
        SET IsActive = 0
        WHERE ContactoID = @ContactoID
          AND IsActive = 1;

        SELECT CAST(1 AS BIT) AS Ok, 'Contacto eliminado correctamente.' AS Mensaje;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END;
GO

--usp_Telefono_Create
CREATE OR ALTER PROCEDURE dbo.usp_Telefono_Create
    @ContactoID INT,
    @Numero NVARCHAR(20),
    @Tipo NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        SET @Numero = LTRIM(RTRIM(@Numero));
        SET @Tipo = NULLIF(LTRIM(RTRIM(@Tipo)), '');

        IF @ContactoID IS NULL OR @ContactoID <= 0
            THROW 50100, 'El ContactoID es inválido.', 1;

        IF NOT EXISTS (
            SELECT 1 FROM dbo.Contacto
            WHERE ContactoID = @ContactoID
              AND IsActive = 1
        )
            THROW 50101, 'El contacto no existe o está inactivo.', 1;

        IF @Numero IS NULL OR @Numero = ''
            THROW 50102, 'El número de teléfono es obligatorio.', 1;

        IF LEN(@Numero) < 8 OR LEN(@Numero) > 20
            THROW 50103, 'El número de teléfono debe tener entre 8 y 20 caracteres.', 1;

        IF EXISTS (
            SELECT 1
            FROM dbo.Telefono
            WHERE ContactoID = @ContactoID
              AND Numero = @Numero
              AND IsActive = 1
        )
            THROW 50104, 'El número ya existe para este contacto.', 1;

        INSERT INTO dbo.Telefono
        (
            ContactoID,
            Numero,
            Tipo,
            IsActive,
            CreatedAt
        )
        VALUES
        (
            @ContactoID,
            @Numero,
            @Tipo,
            1,
            SYSDATETIME()
        );

        DECLARE @TelefonoID INT = SCOPE_IDENTITY();

        SELECT
            TelefonoID,
            ContactoID,
            Numero,
            Tipo,
            IsActive,
            CreatedAt
        FROM dbo.Telefono
        WHERE TelefonoID = @TelefonoID;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END;
GO

--usp_Telefono_Update

CREATE OR ALTER PROCEDURE dbo.usp_Telefono_Update
    @TelefonoID INT,
    @Numero NVARCHAR(20),
    @Tipo NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        SET @Numero = LTRIM(RTRIM(@Numero));
        SET @Tipo = NULLIF(LTRIM(RTRIM(@Tipo)), '');

        IF @TelefonoID IS NULL OR @TelefonoID <= 0
            THROW 50110, 'El TelefonoID es inválido.', 1;

        IF NOT EXISTS (
            SELECT 1
            FROM dbo.Telefono
            WHERE TelefonoID = @TelefonoID
              AND IsActive = 1
        )
            THROW 50111, 'El teléfono no existe o está inactivo.', 1;

        IF @Numero IS NULL OR @Numero = ''
            THROW 50112, 'El número de teléfono es obligatorio.', 1;

        IF LEN(@Numero) < 8 OR LEN(@Numero) > 20
            THROW 50113, 'El número de teléfono debe tener entre 8 y 20 caracteres.', 1;

        IF EXISTS (
            SELECT 1
            FROM dbo.Telefono
            WHERE Numero = @Numero
              AND IsActive = 1
              AND TelefonoID <> @TelefonoID
              AND ContactoID = (SELECT ContactoID FROM dbo.Telefono WHERE TelefonoID = @TelefonoID)
        )
            THROW 50114, 'Ya existe otro teléfono activo con ese número para el mismo contacto.', 1;

        UPDATE dbo.Telefono
        SET
            Numero = @Numero,
            Tipo = @Tipo
        WHERE TelefonoID = @TelefonoID;

        SELECT
            TelefonoID,
            ContactoID,
            Numero,
            Tipo,
            IsActive,
            CreatedAt
        FROM dbo.Telefono
        WHERE TelefonoID = @TelefonoID;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END;
GO

--usp_Telefono_ListByContactoId
CREATE OR ALTER PROCEDURE dbo.usp_Telefono_ListByContactoId
    @ContactoID INT
AS
BEGIN
    SET NOCOUNT ON;

    IF @ContactoID IS NULL OR @ContactoID <= 0
        THROW 50120, 'El ContactoID es inválido.', 1;

    SELECT
        TelefonoID,
        ContactoID,
        Numero,
        Tipo,
        IsActive,
        CreatedAt
    FROM dbo.Telefono
    WHERE ContactoID = @ContactoID
      AND IsActive = 1
    ORDER BY TelefonoID ASC;
END;
GO

--usp_Telefono_Delete
CREATE OR ALTER PROCEDURE dbo.usp_Telefono_Delete
    @TelefonoID INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        IF @TelefonoID IS NULL OR @TelefonoID <= 0
            THROW 50130, 'El TelefonoID es inválido.', 1;

        IF NOT EXISTS (
            SELECT 1 FROM dbo.Telefono
            WHERE TelefonoID = @TelefonoID
              AND IsActive = 1
        )
            THROW 50131, 'El teléfono no existe o ya está inactivo.', 1;

        UPDATE dbo.Telefono
        SET IsActive = 0
        WHERE TelefonoID = @TelefonoID;

        SELECT CAST(1 AS BIT) AS Ok, 'Teléfono eliminado correctamente.' AS Mensaje;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END;
GO

--usp_Correo_Create
CREATE OR ALTER PROCEDURE dbo.usp_Correo_Create
    @ContactoID INT,
    @Email NVARCHAR(150)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        SET @Email = LOWER(LTRIM(RTRIM(@Email)));

        IF @ContactoID IS NULL OR @ContactoID <= 0
            THROW 50200, 'El ContactoID es inválido.', 1;

        IF NOT EXISTS (
            SELECT 1 FROM dbo.Contacto
            WHERE ContactoID = @ContactoID
              AND IsActive = 1
        )
            THROW 50201, 'El contacto no existe o está inactivo.', 1;

        IF @Email IS NULL OR @Email = ''
            THROW 50202, 'El correo es obligatorio.', 1;

        IF LEN(@Email) > 150
            THROW 50203, 'El correo excede el máximo permitido de 150 caracteres.', 1;

        IF @Email NOT LIKE '%_@_%._%'
            THROW 50204, 'El formato del correo no es válido.', 1;

        IF EXISTS (
            SELECT 1
            FROM dbo.Correo
            WHERE Email = @Email
              AND IsActive = 1
        )
            THROW 50205, 'El correo ya está registrado en otro contacto activo.', 1;

        INSERT INTO dbo.Correo
        (
            ContactoID,
            Email,
            IsActive,
            CreatedAt
        )
        VALUES
        (
            @ContactoID,
            @Email,
            1,
            SYSDATETIME()
        );

        DECLARE @CorreoID INT = SCOPE_IDENTITY();

        SELECT
            CorreoID,
            ContactoID,
            Email,
            IsActive,
            CreatedAt
        FROM dbo.Correo
        WHERE CorreoID = @CorreoID;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END;
GO

--usp_Correo_Update
CREATE OR ALTER PROCEDURE dbo.usp_Correo_Update
    @CorreoID INT,
    @Email NVARCHAR(150)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        SET @Email = LOWER(LTRIM(RTRIM(@Email)));

        IF @CorreoID IS NULL OR @CorreoID <= 0
            THROW 50210, 'El CorreoID es inválido.', 1;

        IF NOT EXISTS (
            SELECT 1 FROM dbo.Correo
            WHERE CorreoID = @CorreoID
              AND IsActive = 1
        )
            THROW 50211, 'El correo no existe o está inactivo.', 1;

        IF @Email IS NULL OR @Email = ''
            THROW 50212, 'El correo es obligatorio.', 1;

        IF LEN(@Email) > 150
            THROW 50213, 'El correo excede el máximo permitido de 150 caracteres.', 1;

        IF @Email NOT LIKE '%_@_%._%'
            THROW 50214, 'El formato del correo no es válido.', 1;

        IF EXISTS (
            SELECT 1
            FROM dbo.Correo
            WHERE Email = @Email
              AND IsActive = 1
              AND CorreoID <> @CorreoID
        )
            THROW 50215, 'El correo ya está registrado en otro contacto activo.', 1;

        UPDATE dbo.Correo
        SET Email = @Email
        WHERE CorreoID = @CorreoID;

        SELECT
            CorreoID,
            ContactoID,
            Email,
            IsActive,
            CreatedAt
        FROM dbo.Correo
        WHERE CorreoID = @CorreoID;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END;
GO

--usp_Correo_ListByContactoId

CREATE OR ALTER PROCEDURE dbo.usp_Correo_ListByContactoId
    @ContactoID INT
AS
BEGIN
    SET NOCOUNT ON;

    IF @ContactoID IS NULL OR @ContactoID <= 0
        THROW 50220, 'El ContactoID es inválido.', 1;

    SELECT
        CorreoID,
        ContactoID,
        Email,
        IsActive,
        CreatedAt
    FROM dbo.Correo
    WHERE ContactoID = @ContactoID
      AND IsActive = 1
    ORDER BY CorreoID ASC;
END;
GO

--usp_Correo_Delete

CREATE OR ALTER PROCEDURE dbo.usp_Correo_Delete
    @CorreoID INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        IF @CorreoID IS NULL OR @CorreoID <= 0
            THROW 50230, 'El CorreoID es inválido.', 1;

        IF NOT EXISTS (
            SELECT 1 FROM dbo.Correo
            WHERE CorreoID = @CorreoID
              AND IsActive = 1
        )
            THROW 50231, 'El correo no existe o ya está inactivo.', 1;

        UPDATE dbo.Correo
        SET IsActive = 0
        WHERE CorreoID = @CorreoID;

        SELECT CAST(1 AS BIT) AS Ok, 'Correo eliminado correctamente.' AS Mensaje;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END;
GO

--usp_Direccion_UpsertByContactoId

CREATE OR ALTER PROCEDURE dbo.usp_Direccion_UpsertByContactoId
    @ContactoID INT,
    @Provincia NVARCHAR(100) = NULL,
    @Canton NVARCHAR(100) = NULL,
    @Distrito NVARCHAR(100) = NULL,
    @DireccionExacta NVARCHAR(300) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        SET @Provincia = NULLIF(LTRIM(RTRIM(@Provincia)), '');
        SET @Canton = NULLIF(LTRIM(RTRIM(@Canton)), '');
        SET @Distrito = NULLIF(LTRIM(RTRIM(@Distrito)), '');
        SET @DireccionExacta = NULLIF(LTRIM(RTRIM(@DireccionExacta)), '');

        IF @ContactoID IS NULL OR @ContactoID <= 0
            THROW 50300, 'El ContactoID es inválido.', 1;

        IF NOT EXISTS (
            SELECT 1 FROM dbo.Contacto
            WHERE ContactoID = @ContactoID
              AND IsActive = 1
        )
            THROW 50301, 'El contacto no existe o está inactivo.', 1;

        IF @Provincia IS NOT NULL AND LEN(@Provincia) > 100
            THROW 50302, 'Provincia excede el máximo permitido.', 1;

        IF @Canton IS NOT NULL AND LEN(@Canton) > 100
            THROW 50303, 'Cantón excede el máximo permitido.', 1;

        IF @Distrito IS NOT NULL AND LEN(@Distrito) > 100
            THROW 50304, 'Distrito excede el máximo permitido.', 1;

        IF @DireccionExacta IS NOT NULL AND LEN(@DireccionExacta) > 300
            THROW 50305, 'La dirección exacta excede el máximo permitido.', 1;

        IF EXISTS (
            SELECT 1
            FROM dbo.Direccion
            WHERE ContactoID = @ContactoID
              AND IsActive = 1
        )
        BEGIN
            UPDATE dbo.Direccion
            SET
                Provincia = @Provincia,
                Canton = @Canton,
                Distrito = @Distrito,
                DireccionExacta = @DireccionExacta
            WHERE ContactoID = @ContactoID
              AND IsActive = 1;
        END
        ELSE
        BEGIN
            INSERT INTO dbo.Direccion
            (
                ContactoID,
                Provincia,
                Canton,
                Distrito,
                DireccionExacta,
                IsActive,
                CreatedAt
            )
            VALUES
            (
                @ContactoID,
                @Provincia,
                @Canton,
                @Distrito,
                @DireccionExacta,
                1,
                SYSDATETIME()
            );
        END

        SELECT TOP 1
            DireccionID,
            ContactoID,
            Provincia,
            Canton,
            Distrito,
            DireccionExacta,
            IsActive,
            CreatedAt
        FROM dbo.Direccion
        WHERE ContactoID = @ContactoID
          AND IsActive = 1
        ORDER BY DireccionID DESC;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END;
GO

--usp_Direccion_GetByContactoId
CREATE OR ALTER PROCEDURE dbo.usp_Direccion_GetByContactoId
    @ContactoID INT
AS
BEGIN
    SET NOCOUNT ON;

    IF @ContactoID IS NULL OR @ContactoID <= 0
        THROW 50310, 'El ContactoID es inválido.', 1;

    SELECT TOP 1
        DireccionID,
        ContactoID,
        Provincia,
        Canton,
        Distrito,
        DireccionExacta,
        IsActive,
        CreatedAt
    FROM dbo.Direccion
    WHERE ContactoID = @ContactoID
      AND IsActive = 1
    ORDER BY DireccionID DESC;
END;
GO

--usp_Direccion_DeleteByContactoId

CREATE OR ALTER PROCEDURE dbo.usp_Direccion_DeleteByContactoId
    @ContactoID INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        IF @ContactoID IS NULL OR @ContactoID <= 0
            THROW 50320, 'El ContactoID es inválido.', 1;

        UPDATE dbo.Direccion
        SET IsActive = 0
        WHERE ContactoID = @ContactoID
          AND IsActive = 1;

        SELECT CAST(1 AS BIT) AS Ok, 'Dirección eliminada correctamente.' AS Mensaje;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END;
GO

--usp_Nota_Create
CREATE OR ALTER PROCEDURE dbo.usp_Nota_Create
    @ContactoID INT,
    @Contenido NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        SET @Contenido = LTRIM(RTRIM(@Contenido));

        IF @ContactoID IS NULL OR @ContactoID <= 0
            THROW 50400, 'El ContactoID es inválido.', 1;

        IF NOT EXISTS (
            SELECT 1 FROM dbo.Contacto
            WHERE ContactoID = @ContactoID
              AND IsActive = 1
        )
            THROW 50401, 'El contacto no existe o está inactivo.', 1;

        IF @Contenido IS NULL OR @Contenido = ''
            THROW 50402, 'El contenido de la nota es obligatorio.', 1;

        INSERT INTO dbo.Nota
        (
            ContactoID,
            Contenido,
            CreatedAt
        )
        VALUES
        (
            @ContactoID,
            @Contenido,
            SYSDATETIME()
        );

        DECLARE @NotaID INT = SCOPE_IDENTITY();

        SELECT
            NotaID,
            ContactoID,
            Contenido,
            CreatedAt
        FROM dbo.Nota
        WHERE NotaID = @NotaID;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END;
GO

--usp_Nota_Update
CREATE OR ALTER PROCEDURE dbo.usp_Nota_Update
    @NotaID INT,
    @Contenido NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        SET @Contenido = LTRIM(RTRIM(@Contenido));

        IF @NotaID IS NULL OR @NotaID <= 0
            THROW 50410, 'El NotaID es inválido.', 1;

        IF NOT EXISTS (
            SELECT 1 FROM dbo.Nota WHERE NotaID = @NotaID
        )
            THROW 50411, 'La nota no existe.', 1;

        IF @Contenido IS NULL OR @Contenido = ''
            THROW 50412, 'El contenido de la nota es obligatorio.', 1;

        UPDATE dbo.Nota
        SET Contenido = @Contenido
        WHERE NotaID = @NotaID;

        SELECT
            NotaID,
            ContactoID,
            Contenido,
            CreatedAt
        FROM dbo.Nota
        WHERE NotaID = @NotaID;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END;
GO

--usp_Nota_ListByContactoId
CREATE OR ALTER PROCEDURE dbo.usp_Nota_ListByContactoId
    @ContactoID INT
AS
BEGIN
    SET NOCOUNT ON;

    IF @ContactoID IS NULL OR @ContactoID <= 0
        THROW 50420, 'El ContactoID es inválido.', 1;

    SELECT
        NotaID,
        ContactoID,
        Contenido,
        CreatedAt
    FROM dbo.Nota
    WHERE ContactoID = @ContactoID
    ORDER BY NotaID ASC;
END;
GO

--usp_Nota_Delete
CREATE OR ALTER PROCEDURE dbo.usp_Nota_Delete
    @NotaID INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        IF @NotaID IS NULL OR @NotaID <= 0
            THROW 50430, 'El NotaID es inválido.', 1;

        IF NOT EXISTS (
            SELECT 1 FROM dbo.Nota WHERE NotaID = @NotaID
        )
            THROW 50431, 'La nota no existe.', 1;

        DELETE FROM dbo.Nota
        WHERE NotaID = @NotaID;

        SELECT CAST(1 AS BIT) AS Ok, 'Nota eliminada correctamente.' AS Mensaje;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END;
GO

--V3--------------------------------------------------------------------------------------------------------------------

--dbo.usp_Contacto_List
CREATE OR ALTER PROCEDURE dbo.usp_Contacto_List
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        ContactoID,
        Nombre,
        Apellido,
        EsFavorito,
        FechaCreacion,
        IsActive
    FROM Contacto
    WHERE IsActive = 1
    ORDER BY EsFavorito DESC, Nombre ASC
END
GO