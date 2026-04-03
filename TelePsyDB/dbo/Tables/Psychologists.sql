CREATE TABLE [dbo].[Psychologists] (
    [Id]                 INT             IDENTITY (1, 1) NOT NULL,
    [PersonId]           INT             NOT NULL,
    [LicenseNumber]      NVARCHAR (50)   NOT NULL,
    [Specialization]     NVARCHAR (MAX)  NOT NULL,
    [University]         NVARCHAR (MAX)  NOT NULL,
    [ExperienceYears]    INT             NOT NULL,
    [SessionRate]        DECIMAL (18, 2) NOT NULL,
    [Bio]                NVARCHAR (MAX)  NOT NULL,
    [IsActive]           BIT             NOT NULL,
    [Hobbies]            NVARCHAR (MAX)  NOT NULL,
    [IsVerified]         BIT             NOT NULL,
    [CvPath]             NVARCHAR (MAX)  NULL,
    [ProfilePicturePath] NVARCHAR (MAX)  NULL,
    [BankAccountType]   NVARCHAR (20)   NULL,
    [BankAccountNumber] NVARCHAR (50)   NULL,
    CONSTRAINT [PK_Psychologists] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Psychologists_People_PersonId] FOREIGN KEY ([PersonId]) REFERENCES [dbo].[People] ([Id])
);


GO
CREATE NONCLUSTERED INDEX [IX_Psychologists_PersonId]
    ON [dbo].[Psychologists]([PersonId] ASC);

