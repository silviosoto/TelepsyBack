CREATE TABLE [dbo].[Appointments] (
    [Id]                    INT             IDENTITY (1, 1) NOT NULL,
    [PatientId]             INT             NOT NULL,
    [PsychologistId]        INT             NOT NULL,
    [ScheduledTime]         DATETIME2 (7)   NOT NULL,
    [DurationMinutes]       INT             NOT NULL,
    [Status]                INT             NOT NULL,
    [VideoLink]             NVARCHAR (MAX)  NOT NULL,
    [PaymentId]             INT             NULL,
    [PsychologistInvoiceId] INT             NULL,
    [Rate]                  DECIMAL (18, 2) DEFAULT ((0.0)) NOT NULL,
    [TherapyId]             INT             DEFAULT ((0)) NOT NULL,
    [PatientJoinedAt]       DATETIME2 (7)   NULL,
    [PsychologistJoinedAt]  DATETIME2 (7)   NULL,
    [SessionPackageId]      INT             NULL,
    [CreatedAt]             DATETIME2 (7)   DEFAULT (GETUTCDATE()) NOT NULL,
    CONSTRAINT [PK_Appointments] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Appointments_Invoices_PsychologistInvoiceId] FOREIGN KEY ([PsychologistInvoiceId]) REFERENCES [dbo].[Invoices] ([Id]),
    CONSTRAINT [FK_Appointments_Patients_PatientId] FOREIGN KEY ([PatientId]) REFERENCES [dbo].[Patients] ([Id]),
    CONSTRAINT [FK_Appointments_Psychologists_PsychologistId] FOREIGN KEY ([PsychologistId]) REFERENCES [dbo].[Psychologists] ([Id]),
    CONSTRAINT [FK_Appointments_SessionPackages_SessionPackageId] FOREIGN KEY ([SessionPackageId]) REFERENCES [dbo].[SessionPackages] ([Id]),
    CONSTRAINT [FK_Appointments_Therapies_TherapyId] FOREIGN KEY ([TherapyId]) REFERENCES [dbo].[Therapies] ([Id])
);


GO
CREATE NONCLUSTERED INDEX [IX_Appointments_PatientId]
    ON [dbo].[Appointments]([PatientId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Appointments_PsychologistId]
    ON [dbo].[Appointments]([PsychologistId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Appointments_PsychologistInvoiceId]
    ON [dbo].[Appointments]([PsychologistInvoiceId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Appointments_TherapyId]
    ON [dbo].[Appointments]([TherapyId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Appointments_SessionPackageId]
    ON [dbo].[Appointments]([SessionPackageId] ASC);

