CREATE TABLE [dbo].[InvoiceDetails] (
    [Id]               INT             IDENTITY (1, 1) NOT NULL,
    [InvoiceId]        INT             NOT NULL,
    [AppointmentId]    INT             NULL,
    [Description]      NVARCHAR (MAX)  NOT NULL,
    [UnitPrice]        DECIMAL (18, 2) NOT NULL,
    [CommissionAmount] DECIMAL (18, 2) NOT NULL,
    [Total]            DECIMAL (18, 2) NOT NULL,
    CONSTRAINT [PK_InvoiceDetails] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_InvoiceDetails_Appointments_AppointmentId] FOREIGN KEY ([AppointmentId]) REFERENCES [dbo].[Appointments] ([Id]),
    CONSTRAINT [FK_InvoiceDetails_Invoices_InvoiceId] FOREIGN KEY ([InvoiceId]) REFERENCES [dbo].[Invoices] ([Id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_InvoiceDetails_AppointmentId]
    ON [dbo].[InvoiceDetails]([AppointmentId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_InvoiceDetails_InvoiceId]
    ON [dbo].[InvoiceDetails]([InvoiceId] ASC);

