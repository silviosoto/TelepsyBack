CREATE TABLE [dbo].[BlockedSlots] (
    [Id]             INT            IDENTITY (1, 1) NOT NULL,
    [PsychologistId] INT            NOT NULL,
    [StartDateTime]  DATETIME2 (7)  NOT NULL,
    [EndDateTime]    DATETIME2 (7)  NOT NULL,
    [Reason]         NVARCHAR (MAX) NOT NULL,
    CONSTRAINT [PK_BlockedSlots] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_BlockedSlots_Psychologists_PsychologistId] FOREIGN KEY ([PsychologistId]) REFERENCES [dbo].[Psychologists] ([Id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_BlockedSlots_PsychologistId]
    ON [dbo].[BlockedSlots]([PsychologistId] ASC);

