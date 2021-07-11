
drop table [RegionZipPostal]
CREATE TABLE [dbo].[RegionZipPostal]
(
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RegionName] [varchar](max) NOT NULL
	CONSTRAINT [PK_RegionZipPostal] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

drop table [RegionZipPostalState]
CREATE TABLE [dbo].[RegionZipPostalState]
(
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RegionZipPostalId] INT NOT NULL,
	[StateProvinceÎd] INT NOT NULL,
	CONSTRAINT [PK_RegionZipPostalState] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO


ALTER TABLE [dbo].[RegionZipPostalState]  WITH CHECK ADD  CONSTRAINT [RegionZipPostalState_RegionZipPostal] FOREIGN KEY([RegionZipPostalId])
REFERENCES [dbo].[RegionZipPostal] ([Id])
GO

drop table [RegionZipPostalRange]
CREATE TABLE [dbo].[RegionZipPostalRange]
(
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RegionZipPostalId] INT NOT NULL,
	[ZipPostalStart] [nvarchar](max) NOT NULL,
	[ZipPostalEnd] [nvarchar](max) NOT NULL
	CONSTRAINT [PK_RegionZipPostalRange] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO


ALTER TABLE [dbo].[RegionZipPostalRange]  WITH CHECK ADD  CONSTRAINT [RegionZipPostalRange_RegionZipPostal] FOREIGN KEY([RegionZipPostalId])
REFERENCES [dbo].[RegionZipPostal] ([Id])
GO
