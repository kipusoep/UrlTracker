CREATE TABLE [icUrlTracker] (
  [Id] int NOT NULL  IDENTITY (1,1)
, [OldUrl] nvarchar(400) NULL
, [OldUrlQueryString] nvarchar(400) NULL
, [OldRegex] nvarchar(400) NULL
, [RedirectRootNodeId] int NULL
, [RedirectNodeId] int NULL
, [RedirectUrl] nvarchar(400) NULL
, [RedirectHttpCode] int NOT NULL DEFAULT ((301))
, [RedirectPassThroughQueryString] bit NOT NULL DEFAULT ((1))
, [ForceRedirect] bit NOT NULL DEFAULT ((0))
, [Notes] nvarchar(400) NULL
, [Is404] bit NOT NULL DEFAULT ((0))
, [Referrer] nvarchar(400) NULL
, [Inserted] datetime NOT NULL DEFAULT (getdate())
, CONSTRAINT [PK_icUrlTracker] PRIMARY KEY ([Id])
)