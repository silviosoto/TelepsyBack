CREATE TABLE [dbo].[Admins] (
    [Id]         INT            IDENTITY (1, 1) NOT NULL,
    [PersonId]   INT            NOT NULL,
    [IsActive]   BIT            NOT NULL,
    [Department] NVARCHAR (MAX) NOT NULL,
    CONSTRAINT [PK_Admins] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Admins_People_PersonId] FOREIGN KEY ([PersonId]) REFERENCES [dbo].[People] ([Id])
);


GO
CREATE NONCLUSTERED INDEX [IX_Admins_PersonId]
    ON [dbo].[Admins]([PersonId] ASC);

