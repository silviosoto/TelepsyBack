CREATE TABLE [dbo].[ClinicalRecords] (
    [Id]             INT            IDENTITY (1, 1) NOT NULL,
    [PatientId]      INT            NOT NULL,
    [PsychologistId] INT            NOT NULL,
    [Date]           DATETIME2 (7)  NOT NULL,
    [Notes]          NVARCHAR (MAX) NOT NULL,
    [Diagnosis]      NVARCHAR (MAX) NOT NULL,
    [TreatmentPlan]  NVARCHAR (MAX) NOT NULL,
    CONSTRAINT [PK_ClinicalRecords] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_ClinicalRecords_Patients_PatientId] FOREIGN KEY ([PatientId]) REFERENCES [dbo].[Patients] ([Id]),
    CONSTRAINT [FK_ClinicalRecords_Psychologists_PsychologistId] FOREIGN KEY ([PsychologistId]) REFERENCES [dbo].[Psychologists] ([Id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_ClinicalRecords_PatientId]
    ON [dbo].[ClinicalRecords]([PatientId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_ClinicalRecords_PsychologistId]
    ON [dbo].[ClinicalRecords]([PsychologistId] ASC);

