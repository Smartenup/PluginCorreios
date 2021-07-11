CREATE TABLE [dbo].[SU_StateProvinceZipPostalRange]
(
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[StateProvinceId] INT NOT NULL,
	[ZipPostalStart] [nvarchar](max) NOT NULL,
	[ZipPostalEnd] [nvarchar](max) NOT NULL
	CONSTRAINT [PK_SU_StateProvinceZipPostalRange] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO


CREATE TABLE [dbo].[SU_FreeShiping_Range]
(
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Deleted] bit NOT NULL,
	[StateProvinceIds] [nvarchar](max) NULL,
	[ZipPostalStart] [nvarchar](max) NULL,
	[ZipPostalEnd] [nvarchar](max) NULL,
	[UseMinValue] BIT NULL,
	[MinValue] [decimal] (18,4) NULL,
	[ShipingMethod] [nvarchar](max) NOT NULL,
	CONSTRAINT [PK_SU_FreeShiping_Range] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
