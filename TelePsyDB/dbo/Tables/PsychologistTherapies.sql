CREATE TABLE [dbo].[PsychologistTherapies] (
    [Id]             INT             IDENTITY (1, 1) NOT NULL,
    [PsychologistId] INT             NOT NULL,
    [TherapyId]      INT             NOT NULL,
    [Rate]           DECIMAL (18, 2) NOT NULL,
    [IsActive]       BIT             NOT NULL,
    CONSTRAINT [PK_PsychologistTherapies] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_PsychologistTherapies_Psychologists_PsychologistId] FOREIGN KEY ([PsychologistId]) REFERENCES [dbo].[Psychologists] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_PsychologistTherapies_Therapies_TherapyId] FOREIGN KEY ([TherapyId]) REFERENCES [dbo].[Therapies] ([Id])
);


GO
CREATE NONCLUSTERED INDEX [IX_PsychologistTherapies_PsychologistId]
    ON [dbo].[PsychologistTherapies]([PsychologistId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_PsychologistTherapies_TherapyId]
    ON [dbo].[PsychologistTherapies]([TherapyId] ASC);

