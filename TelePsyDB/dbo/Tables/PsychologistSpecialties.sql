CREATE TABLE [dbo].[PsychologistSpecialties] (
    [Id]             INT IDENTITY (1, 1) NOT NULL,
    [PsychologistId] INT NOT NULL,
    [SpecialtyId]    INT NOT NULL,
    [IsActive]       BIT NULL,
    CONSTRAINT [PK_PsychologistSpecialties] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_PsychologistSpecialties_Psychologists_PsychologistId] FOREIGN KEY ([PsychologistId]) REFERENCES [dbo].[Psychologists] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_PsychologistSpecialties_Specialties_SpecialtyId] FOREIGN KEY ([SpecialtyId]) REFERENCES [dbo].[Specialties] ([Id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_PsychologistSpecialties_PsychologistId]
    ON [dbo].[PsychologistSpecialties]([PsychologistId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_PsychologistSpecialties_SpecialtyId]
    ON [dbo].[PsychologistSpecialties]([SpecialtyId] ASC);

