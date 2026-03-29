CREATE TABLE [dbo].[WorkSchedules] (
    [Id]             INT      IDENTITY (1, 1) NOT NULL,
    [PsychologistId] INT      NOT NULL,
    [DayOfWeek]      INT      NOT NULL,
    [StartTime]      TIME (7) NOT NULL,
    [EndTime]        TIME (7) NOT NULL,
    [IsActive]       BIT      NOT NULL,
    CONSTRAINT [PK_WorkSchedules] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_WorkSchedules_Psychologists_PsychologistId] FOREIGN KEY ([PsychologistId]) REFERENCES [dbo].[Psychologists] ([Id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_WorkSchedules_PsychologistId]
    ON [dbo].[WorkSchedules]([PsychologistId] ASC);

