CREATE TABLE [dbo].[AuditLogs] (
    [Id]         INT            IDENTITY (1, 1) NOT NULL,
    [UserId]     NVARCHAR (MAX) NOT NULL,
    [Action]     NVARCHAR (MAX) NOT NULL,
    [EntityName] NVARCHAR (MAX) NOT NULL,
    [EntityId]   NVARCHAR (MAX) NOT NULL,
    [Timestamp]  DATETIME2 (7)  NOT NULL,
    [Details]    NVARCHAR (MAX) NOT NULL,
    [IPAddress]  NVARCHAR (MAX) NOT NULL,
    CONSTRAINT [PK_AuditLogs] PRIMARY KEY CLUSTERED ([Id] ASC)
);

