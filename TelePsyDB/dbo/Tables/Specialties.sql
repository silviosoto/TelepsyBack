CREATE TABLE [dbo].[Specialties] (
    [Id]          INT            IDENTITY (1, 1) NOT NULL,
    [Name]        NVARCHAR (150) NOT NULL,
    [Description] NVARCHAR (500) NULL,
    [IsActive]    BIT            NOT NULL,
    CONSTRAINT [PK_Specialties] PRIMARY KEY CLUSTERED ([Id] ASC)
);

