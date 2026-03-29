CREATE TABLE [dbo].[SessionPackages] (
    [Id]                  INT             IDENTITY (1, 1) NOT NULL,
    [PatientId]           INT             NOT NULL,
    [PsychologistId]      INT             NOT NULL,
    [TherapyId]           INT             NOT NULL,
    [OriginalTotalAmount] DECIMAL (18, 2) NOT NULL,
    [DiscountPercentage]  DECIMAL (18, 2) NOT NULL,
    [FinalAmount]         DECIMAL (18, 2) NOT NULL,
    [TotalSessions]       INT             NOT NULL,
    [UsedSessions]        INT             NOT NULL,
    [IsActive]            BIT             NOT NULL,
    [PaymentId]           INT             NULL,
    [CreatedAt]           DATETIME2 (7)   NOT NULL,
    CONSTRAINT [PK_SessionPackages] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_SessionPackages_Patients_PatientId] FOREIGN KEY ([PatientId]) REFERENCES [dbo].[Patients] ([Id]),
    CONSTRAINT [FK_SessionPackages_Payments_PaymentId] FOREIGN KEY ([PaymentId]) REFERENCES [dbo].[Payments] ([Id]),
    CONSTRAINT [FK_SessionPackages_Psychologists_PsychologistId] FOREIGN KEY ([PsychologistId]) REFERENCES [dbo].[Psychologists] ([Id]),
    CONSTRAINT [FK_SessionPackages_Therapies_TherapyId] FOREIGN KEY ([TherapyId]) REFERENCES [dbo].[Therapies] ([Id])
);


GO
CREATE NONCLUSTERED INDEX [IX_SessionPackages_PatientId]
    ON [dbo].[SessionPackages]([PatientId] ASC);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_SessionPackages_PaymentId]
    ON [dbo].[SessionPackages]([PaymentId] ASC) WHERE ([PaymentId] IS NOT NULL);


GO
CREATE NONCLUSTERED INDEX [IX_SessionPackages_PsychologistId]
    ON [dbo].[SessionPackages]([PsychologistId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_SessionPackages_TherapyId]
    ON [dbo].[SessionPackages]([TherapyId] ASC);

