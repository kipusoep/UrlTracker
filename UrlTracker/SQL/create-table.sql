CREATE TABLE [icUrlTracker](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[OldUrl] [nvarchar](400) NULL,
	[OldUrlQueryString] [nvarchar](400) NULL,
	[OldRegex] [nvarchar](400) NULL,
	[RedirectRootNodeId] [int] NULL,
	[RedirectNodeId] [int] NULL,
	[RedirectUrl] [nvarchar](400) NULL,
	[RedirectHttpCode] [int] NOT NULL,
	[RedirectPassThroughQueryString] [bit] NOT NULL,
	[Notes] [nvarchar](400) NULL,
	[Is404] [bit] NOT NULL,
	[Referrer] [nvarchar](400) NULL,
	[Inserted] [datetime] NOT NULL,
	 CONSTRAINT [PK_icUrlTracker] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)
)
ALTER TABLE [icUrlTracker] ADD  CONSTRAINT [DF_icUrlTracker_RedirectCode]  DEFAULT ((301)) FOR [RedirectHttpCode]
ALTER TABLE [icUrlTracker] ADD  CONSTRAINT [DF_icUrlTracker_RedirectPassThroughQueryString]  DEFAULT ((1)) FOR [RedirectPassThroughQueryString]
ALTER TABLE [icUrlTracker] ADD  CONSTRAINT [DF_icUrlTracker_Is404]  DEFAULT ((0)) FOR [Is404]
ALTER TABLE [icUrlTracker] ADD  CONSTRAINT [DF_icUrlTracker_Inserted]  DEFAULT (getdate()) FOR [Inserted]