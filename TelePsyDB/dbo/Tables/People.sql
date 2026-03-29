CREATE TABLE [dbo].[People] (
    [Id]          INT            IDENTITY (1, 1) NOT NULL,
    [FirstName]   NVARCHAR (100) NOT NULL,
    [LastName]    NVARCHAR (100) NOT NULL,
    [DateOfBirth] DATETIME2 (7)  NOT NULL,
    [Gender]      NVARCHAR (20)  NOT NULL,
    [PhoneNumber] NVARCHAR (MAX) NOT NULL,
    [Address]     NVARCHAR (MAX) NOT NULL,
    [City]        NVARCHAR (100) NOT NULL,
    [State]       NVARCHAR (MAX) NOT NULL,
    [Country]     NVARCHAR (MAX) NOT NULL,
    [IsActive]    BIT            NOT NULL,
    [UserId]      NVARCHAR (450) NOT NULL,
    CONSTRAINT [PK_People] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_People_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_People_UserId]
    ON [dbo].[People]([UserId] ASC);

