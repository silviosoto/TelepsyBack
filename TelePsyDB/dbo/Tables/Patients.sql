CREATE TABLE [dbo].[Patients] (
    [Id]                 INT            IDENTITY (1, 1) NOT NULL,
    [PersonId]           INT            NOT NULL,
    [IsActive]           BIT            NOT NULL,
    [Occupation]         NVARCHAR (100) NOT NULL,
    [EmergencyContact]   NVARCHAR (MAX) NOT NULL,
    [PreferredGender]    NVARCHAR (MAX) NOT NULL,
    [Interests]          NVARCHAR (MAX) NOT NULL,
    [ProfilePicturePath] NVARCHAR (MAX) DEFAULT (N'') NOT NULL,
    CONSTRAINT [PK_Patients] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Patients_People_PersonId] FOREIGN KEY ([PersonId]) REFERENCES [dbo].[People] ([Id])
);


GO
CREATE NONCLUSTERED INDEX [IX_Patients_PersonId]
    ON [dbo].[Patients]([PersonId] ASC);

