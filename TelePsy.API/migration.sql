BEGIN TRANSACTION;
CREATE TABLE [Departments] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_Departments] PRIMARY KEY ([Id])
);

CREATE TABLE [Specialties] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(150) NOT NULL,
    [Description] nvarchar(500) NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_Specialties] PRIMARY KEY ([Id])
);

CREATE TABLE [Cities] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NOT NULL,
    [DepartmentId] int NOT NULL,
    CONSTRAINT [PK_Cities] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Cities_Departments_DepartmentId] FOREIGN KEY ([DepartmentId]) REFERENCES [Departments] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [PsychologistSpecialties] (
    [Id] int NOT NULL IDENTITY,
    [PsychologistId] int NOT NULL,
    [SpecialtyId] int NOT NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_PsychologistSpecialties] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_PsychologistSpecialties_Psychologists_PsychologistId] FOREIGN KEY ([PsychologistId]) REFERENCES [Psychologists] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_PsychologistSpecialties_Specialties_SpecialtyId] FOREIGN KEY ([SpecialtyId]) REFERENCES [Specialties] ([Id]) ON DELETE NO ACTION
);

CREATE INDEX [IX_Cities_DepartmentId] ON [Cities] ([DepartmentId]);

CREATE INDEX [IX_PsychologistSpecialties_PsychologistId] ON [PsychologistSpecialties] ([PsychologistId]);

CREATE INDEX [IX_PsychologistSpecialties_SpecialtyId] ON [PsychologistSpecialties] ([SpecialtyId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260310023140_AddLocations', N'10.0.3');

COMMIT;
GO

