USE [dbRemittance]
GO
/****** Object:  Table [dbo].[Api]    Script Date: 10/01/2019 7:54:50 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Api](
	[Id] [nvarchar](50) NULL,
	[APIName] [nvarchar](50) NULL,
	[EndPoint] [nvarchar](50) NULL
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[ExchangeHouse]    Script Date: 10/01/2019 7:54:50 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ExchangeHouse](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](50) NULL,
	[ContactAddress] [nvarchar](200) NULL,
	[ContavtPhone] [nvarchar](50) NULL,
	[Fax] [nvarchar](50) NULL,
	[CountryOfOrigin] [nchar](10) NULL,
	[DateOfBusinessWithBank] [nvarchar](50) NULL,
	[OpenHours] [int] NULL,
	[CloseHours] [int] NULL,
	[BankGuranteeAmount] [float] NULL,
	[BankGuranteeExpiryDate] [datetime] NULL,
	[BankGuranteeDescription] [nvarchar](50) NULL,
	[MinimumBalance] [float] NULL,
	[ExchangeHouseLicenseExpiryDate] [datetime] NULL,
	[BangladeshBankApprovalDate] [datetime] NULL,
	[AMLQuestionaireReceiveDate] [datetime] NULL,
	[CurrentStatus] [nvarchar](50) NULL,
	[RemittanceTransactionMechanism] [nvarchar](50) NULL,
	[ExHCode] [nvarchar](50) NULL,
 CONSTRAINT [PK_ExchangeHouse] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[ExHIPAddress]    Script Date: 10/01/2019 7:54:50 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ExHIPAddress](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[IPAddress] [nvarchar](50) NULL,
	[ActivtionDate] [datetime] NULL,
	[ReferenceDocForIPRequest] [nvarchar](50) NULL,
	[CreatedBy] [nvarchar](50) NULL,
	[CreationDate] [datetime] NULL,
	[IsActive] [bit] NULL,
	[UpdatedBy] [nvarchar](50) NULL,
	[UpdateTime] [datetime] NULL,
	[ExchangeHouseID] [int] NULL,
 CONSTRAINT [PK_ExHIPAddress] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[ExHRemitData]    Script Date: 10/01/2019 7:54:50 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ExHRemitData](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ExchangeHouseId] [int] NULL,
	[DataCreationMechanism] [nvarchar](50) NULL,
	[CreatedBy] [int] NULL,
	[CreationDateTime] [datetime] NULL,
	[DataParsingDate] [datetime] NULL,
	[DataParsingStatus] [nvarchar](10) NULL,
	[DataParsedBy] [int] NULL,
	[CommentIfFailOrPartialParsed] [nchar](10) NULL,
 CONSTRAINT [PK_ExHRemitData] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[ExHRemitDataDetails]    Script Date: 10/01/2019 7:54:50 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ExHRemitDataDetails](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ExHRemitDataId] [int] NULL,
	[RawRemitData] [ntext] NULL,
	[ParsedStatus] [nvarchar](10) NULL,
 CONSTRAINT [PK_ExHRemitDataDetails] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[ExHUser]    Script Date: 10/01/2019 7:54:50 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ExHUser](
	[Id] [int] NOT NULL,
	[ExchangeHouseId] [int] NULL,
	[ExUserName] [nvarchar](50) NULL,
	[ExUserPassword] [nvarchar](50) NULL,
	[ReferenceForUser] [nvarchar](50) NULL,
 CONSTRAINT [PK_ExHUser] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
