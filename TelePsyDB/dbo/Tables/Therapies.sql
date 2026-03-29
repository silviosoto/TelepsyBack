CREATE TABLE [dbo].[Therapies] (
    [Id]          INT            IDENTITY (1, 1) NOT NULL,
    [Name]        NVARCHAR (100) NOT NULL,
    [Description] NVARCHAR (500) NOT NULL,
    [IsActive]    BIT            NOT NULL,
    CONSTRAINT [PK_Therapies] PRIMARY KEY CLUSTERED ([Id] ASC)
);

