CREATE TABLE [dbo].[GlobalConfigurations] (
    [Id]          INT            IDENTITY (1, 1) NOT NULL,
    [Key]         NVARCHAR (MAX) NOT NULL,
    [Value]       NVARCHAR (MAX) NOT NULL,
    [LastUpdated] DATETIME2 (7)  NOT NULL,
    CONSTRAINT [PK_GlobalConfigurations] PRIMARY KEY CLUSTERED ([Id] ASC)
);

