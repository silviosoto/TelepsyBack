CREATE TABLE [dbo].[Payments] (
    [Id]               INT             IDENTITY (1, 1) NOT NULL,
    [Amount]           DECIMAL (18, 2) NOT NULL,
    [Date]             DATETIME2 (7)   NOT NULL,
    [Status]           NVARCHAR (MAX)  NOT NULL,
    [TransactionId]    NVARCHAR (MAX)  NOT NULL,
    [AppointmentId]    INT             NOT NULL,
    [PatientInvoiceId] INT             NULL,
    CONSTRAINT [PK_Payments] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Payments_Appointments_AppointmentId] FOREIGN KEY ([AppointmentId]) REFERENCES [dbo].[Appointments] ([Id]),
    CONSTRAINT [FK_Payments_Invoices_PatientInvoiceId] FOREIGN KEY ([PatientInvoiceId]) REFERENCES [dbo].[Invoices] ([Id])
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Payments_AppointmentId]
    ON [dbo].[Payments]([AppointmentId] ASC);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Payments_PatientInvoiceId]
    ON [dbo].[Payments]([PatientInvoiceId] ASC) WHERE ([PatientInvoiceId] IS NOT NULL);

