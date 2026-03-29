CREATE TABLE [dbo].[PsychologyNotes] (
    [Id]                    INT            IDENTITY (1, 1) NOT NULL,
    [PatientId]             INT            NOT NULL,
    [PsychologistId]        INT            NOT NULL,
    [AppointmentId]         INT            NULL,
    [Date]                  DATETIME2 (7)  NOT NULL,
    [SessionNumber]         INT            NOT NULL,
    [ReasonForSession]      NVARCHAR (MAX) NOT NULL,
    [Evolution]             NVARCHAR (MAX) NOT NULL,
    [Interventions]         NVARCHAR (MAX) NOT NULL,
    [TherapeuticPlan]       NVARCHAR (MAX) NOT NULL,
    [NextAppointmentDate]   DATETIME2 (7)  NULL,
    [ProfessionalSignature] NVARCHAR (MAX) NOT NULL,
    CONSTRAINT [PK_PsychologyNotes] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_PsychologyNotes_Appointments_AppointmentId] FOREIGN KEY ([AppointmentId]) REFERENCES [dbo].[Appointments] ([Id]),
    CONSTRAINT [FK_PsychologyNotes_Patients_PatientId] FOREIGN KEY ([PatientId]) REFERENCES [dbo].[Patients] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_PsychologyNotes_Psychologists_PsychologistId] FOREIGN KEY ([PsychologistId]) REFERENCES [dbo].[Psychologists] ([Id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_PsychologyNotes_AppointmentId]
    ON [dbo].[PsychologyNotes]([AppointmentId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_PsychologyNotes_PatientId]
    ON [dbo].[PsychologyNotes]([PatientId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_PsychologyNotes_PsychologistId]
    ON [dbo].[PsychologyNotes]([PsychologistId] ASC);

