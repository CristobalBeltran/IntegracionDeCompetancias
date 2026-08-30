-- ============================================================
-- Script de creación de Base de Datos (versión SQL Server)
-- Sistema de Registro de Asistencia de Empleados
-- Integración de Competencias II - Avance #3
-- ============================================================
-- Diferencias respecto al script de MySQL:
--   * AUTO_INCREMENT           -> IDENTITY(1,1)
--   * ENUM(...)                -> VARCHAR + CHECK
--   * TINYINT(1)                -> BIT
--   * SHA2(texto, 256)          -> HASHBYTES('SHA2_256', texto)
--   * ON UPDATE CURRENT_TIMESTAMP -> trigger (SQL Server no lo trae de fábrica)
--   * ON DUPLICATE KEY UPDATE   -> se maneja en el DAO en C# (IF EXISTS ... UPDATE ELSE INSERT)
-- ============================================================

IF DB_ID('registro_asistencia') IS NOT NULL
BEGIN
    ALTER DATABASE registro_asistencia SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE registro_asistencia;
END
GO

CREATE DATABASE registro_asistencia;
GO

USE registro_asistencia;
GO

-- ------------------------------------------------------------
-- Tabla: roles
-- ------------------------------------------------------------
CREATE TABLE roles (
    id_rol      INT IDENTITY(1,1) PRIMARY KEY,
    nombre_rol  VARCHAR(30) NOT NULL UNIQUE
);
GO

INSERT INTO roles (nombre_rol) VALUES
('ADMINISTRADOR'),
('EMPLEADO');
GO

-- ------------------------------------------------------------
-- Tabla: usuarios
-- ------------------------------------------------------------
CREATE TABLE usuarios (
    id_usuario          INT IDENTITY(1,1) PRIMARY KEY,
    nombre              VARCHAR(50)  NOT NULL,
    apellido            VARCHAR(50)  NOT NULL,
    email               VARCHAR(100) NOT NULL UNIQUE,
    password            VARCHAR(255) NOT NULL, -- se guarda encriptada (hash SHA-256)
    id_rol              INT NOT NULL,
    activo              BIT NOT NULL DEFAULT 1,
    fecha_creacion      DATETIME NOT NULL DEFAULT GETDATE(),
    fecha_actualizacion DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT fk_usuario_rol
        FOREIGN KEY (id_rol) REFERENCES roles(id_rol)
);
GO

-- Simula el "ON UPDATE CURRENT_TIMESTAMP" que trae MySQL de fábrica.
CREATE TRIGGER trg_usuarios_set_fecha_actualizacion
ON usuarios
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE u
    SET fecha_actualizacion = GETDATE()
    FROM usuarios u
    INNER JOIN inserted i ON u.id_usuario = i.id_usuario;
END
GO

-- ------------------------------------------------------------
-- Tabla: asistencia
-- ------------------------------------------------------------
CREATE TABLE asistencia (
    id_asistencia   INT IDENTITY(1,1) PRIMARY KEY,
    id_usuario      INT NOT NULL,
    fecha           DATE NOT NULL,
    hora_entrada    TIME NULL,
    hora_salida     TIME NULL,
    fecha_registro  DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT fk_asistencia_usuario
        FOREIGN KEY (id_usuario) REFERENCES usuarios(id_usuario)
        ON DELETE CASCADE,
    CONSTRAINT uq_usuario_fecha UNIQUE (id_usuario, fecha)
);
GO

-- ------------------------------------------------------------
-- Tabla: reportes
-- Nota: fk_reporte_usuario queda sin CASCADE porque SQL Server no
-- permite dos caminos de cascada hacia la misma tabla (usuarios ->
-- asistencia -> reportes, y usuarios -> reportes). El borrado de
-- reportes ligados a un usuario ocurre igual cuando se borra la
-- asistencia asociada (fk_reporte_asistencia sí tiene CASCADE).
-- ------------------------------------------------------------
CREATE TABLE reportes (
    id_reporte       INT IDENTITY(1,1) PRIMARY KEY,
    id_usuario       INT NOT NULL,
    id_asistencia    INT NULL,
    tipo_reporte     VARCHAR(20) NOT NULL
        CHECK (tipo_reporte IN ('ATRASO', 'SALIDA_ANTICIPADA', 'INASISTENCIA')),
    fecha_incidencia DATE NOT NULL,
    fecha_generacion DATETIME NOT NULL DEFAULT GETDATE(),
    observacion      VARCHAR(255) NULL,
    CONSTRAINT fk_reporte_usuario
        FOREIGN KEY (id_usuario) REFERENCES usuarios(id_usuario),
    CONSTRAINT fk_reporte_asistencia
        FOREIGN KEY (id_asistencia) REFERENCES asistencia(id_asistencia)
        ON DELETE CASCADE
);
GO

-- ------------------------------------------------------------
-- Datos de prueba
-- Las contraseñas se insertan encriptadas con SHA-256, en
-- hexadecimal minúscula, igual que las genera PasswordUtil.cs
-- ------------------------------------------------------------
INSERT INTO usuarios (nombre, apellido, email, password, id_rol) VALUES
('Ana',   'Soto',   'ana.soto@empresa.cl',    LOWER(CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', 'admin1234'), 2)), 1),
('Pedro', 'Rojas',  'pedro.rojas@empresa.cl', LOWER(CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', 'pedro1234'), 2)), 2),
('Maria', 'Diaz',   'maria.diaz@empresa.cl',  LOWER(CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', 'maria1234'), 2)), 2);
GO

INSERT INTO asistencia (id_usuario, fecha, hora_entrada, hora_salida) VALUES
(2, '2026-08-18', '09:15:00', '17:35:00'),
(2, '2026-08-19', '09:45:00', '17:30:00'),
(3, '2026-08-18', '09:10:00', '17:15:00'),
(3, '2026-08-19', NULL,       NULL);
GO

INSERT INTO reportes (id_usuario, id_asistencia, tipo_reporte, fecha_incidencia, observacion) VALUES
(2, 2, 'ATRASO',            '2026-08-19', 'Entrada registrada después de las 09:30'),
(3, 3, 'SALIDA_ANTICIPADA', '2026-08-18', 'Salida registrada antes de las 17:30'),
(3, NULL, 'INASISTENCIA',   '2026-08-19', 'Sin registro de entrada ni salida');
GO

-- ------------------------------------------------------------
-- Consultas de ejemplo para validar que todo quedó bien
-- ------------------------------------------------------------

-- Validar login (comparando el hash, no el texto plano)
SELECT id_usuario, nombre, apellido
FROM usuarios
WHERE email = 'pedro.rojas@empresa.cl'
  AND password = LOWER(CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', 'pedro1234'), 2));

-- Ver todos los reportes generados, con datos del usuario
SELECT r.id_reporte, u.nombre, u.apellido, r.tipo_reporte,
       r.fecha_incidencia, r.observacion
FROM reportes r
JOIN usuarios u ON u.id_usuario = r.id_usuario
ORDER BY r.fecha_incidencia;
