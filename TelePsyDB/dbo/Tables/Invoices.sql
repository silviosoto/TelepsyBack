CREATE TABLE [dbo].[Invoices] (
    [Id]             INT             IDENTITY (1, 1) NOT NULL,
    [InvoiceNumber]  NVARCHAR (MAX)  NOT NULL,
    [IssueDate]      DATETIME2 (7)   NOT NULL,
    [TotalAmount]    DECIMAL (18, 2) NOT NULL,
    [Type]           INT             NOT NULL,
    [Status]         INT             NOT NULL,
    [PatientId]      INT             NULL,
    [PaymentId]      INT             NULL,
    [PsychologistId] INT             NULL,
    CONSTRAINT [PK_Invoices] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Invoices_Patients_PatientId] FOREIGN KEY ([PatientId]) REFERENCES [dbo].[Patients] ([Id]),
    CONSTRAINT [FK_Invoices_Psychologists_PsychologistId] FOREIGN KEY ([PsychologistId]) REFERENCES [dbo].[Psychologists] ([Id])
);


GO
CREATE NONCLUSTERED INDEX [IX_Invoices_PatientId]
    ON [dbo].[Invoices]([PatientId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Invoices_PsychologistId]
    ON [dbo].[Invoices]([PsychologistId] ASC);

