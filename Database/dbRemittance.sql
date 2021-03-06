USE [dbRemittance]
GO
/****** Object:  Table [dbo].[ActionLog]    Script Date: 13/01/2019 8:09:44 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ActionLog](
	[Id] [uniqueidentifier] NOT NULL,
	[Who] [int] NULL,
	[When] [datetime] NULL,
	[AffectedRecordId] [nvarchar](50) NULL,
	[What] [xml] NULL,
	[ActionCRUD] [nvarchar](50) NULL,
	[Entity] [nvarchar](50) NULL,
	[IPAddress] [nvarchar](50) NULL,
 CONSTRAINT [PK_ActionLog] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Api]    Script Date: 13/01/2019 8:09:45 PM ******/
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
/****** Object:  Table [dbo].[Branch]    Script Date: 13/01/2019 8:09:45 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Branch](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](50) NULL,
	[Address] [nvarchar](150) NULL,
	[Phone] [nvarchar](50) NULL,
	[Email] [nvarchar](100) NULL,
	[BankId] [int] NULL,
	[CountryId] [nvarchar](50) NULL,
 CONSTRAINT [PK_Branch] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[BusinessUser]    Script Date: 13/01/2019 8:09:45 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[BusinessUser](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[LoginName] [nvarchar](50) NULL,
	[Password] [nvarchar](50) NULL,
	[RoleId] [int] NULL,
	[IsActive] [bit] NULL,
	[PwdTimeStamp] [datetime] NULL,
	[EmployeeId] [int] NULL,
	[ExpiryDate] [datetime] NULL,
	[BranchCode] [nvarchar](20) NULL,
	[AgentCode] [nvarchar](20) NULL,
 CONSTRAINT [PK_User] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[City]    Script Date: 13/01/2019 8:09:45 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[City](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CountryId] [nvarchar](50) NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
 CONSTRAINT [PK_City] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Company]    Script Date: 13/01/2019 8:09:45 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Company](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](50) NULL,
	[Phone] [nvarchar](50) NULL,
	[Fax] [nvarchar](50) NULL,
	[Email] [nvarchar](50) NULL,
	[ContactPerson] [nvarchar](50) NULL,
	[LogoName] [nvarchar](100) NULL,
	[CompanyUrl] [nvarchar](max) NULL,
	[BaseCurrency] [int] NULL,
	[LocalCurrency] [int] NULL,
	[CompanyAddress] [nvarchar](1000) NULL,
	[CityId] [int] NOT NULL,
 CONSTRAINT [PK_Company] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Country]    Script Date: 13/01/2019 8:09:45 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Country](
	[Id] [nvarchar](50) NOT NULL,
	[Name] [nvarchar](50) NULL,
	[Code] [nvarchar](50) NULL,
 CONSTRAINT [PK_CountryControl] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Currency]    Script Date: 13/01/2019 8:09:45 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Currency](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](50) NULL,
	[Symbol] [nvarchar](50) NULL,
 CONSTRAINT [PK_Currency] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Department]    Script Date: 13/01/2019 8:09:45 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Department](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Code] [nvarchar](50) NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[Information] [nvarchar](100) NULL,
 CONSTRAINT [PK_Department] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Designation]    Script Date: 13/01/2019 8:09:45 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Designation](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[DepartmentId] [int] NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_Designation] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Employee]    Script Date: 13/01/2019 8:09:45 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Employee](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Code] [nvarchar](50) NOT NULL,
	[FullName] [nvarchar](100) NULL,
	[PresentAddress] [nvarchar](200) NULL,
	[PermanentAddress] [nvarchar](200) NULL,
	[FatherName] [nvarchar](100) NULL,
	[MotherName] [nvarchar](100) NULL,
	[NationalId] [nvarchar](50) NULL,
	[PassportNo] [nvarchar](50) NULL,
	[Email] [nvarchar](100) NULL,
	[OfficePhone] [nvarchar](50) NULL,
	[OfficeMobile] [nvarchar](50) NULL,
	[ResidentPhone] [nvarchar](50) NULL,
	[ResidentMobile] [nvarchar](50) NULL,
	[BloodGroup] [nvarchar](10) NULL,
	[PhotoPath] [nvarchar](250) NULL,
	[AuthenticationCode] [nvarchar](50) NULL,
 CONSTRAINT [PK_Employee] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[ExchangeHouse]    Script Date: 13/01/2019 8:09:45 PM ******/
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
/****** Object:  Table [dbo].[ExHApi]    Script Date: 13/01/2019 8:09:45 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ExHApi](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ExchangeHouseId] [int] NULL,
	[APIId] [int] NULL,
	[ActivationDate] [datetime] NULL,
	[CreatedBy] [int] NULL,
	[CreationDate] [datetime] NULL,
 CONSTRAINT [PK_ExHApi] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[ExHIPAddress]    Script Date: 13/01/2019 8:09:45 PM ******/
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
	[DiscontinuationDate] [datetime] NULL,
 CONSTRAINT [PK_ExHIPAddress] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[ExHRemitData]    Script Date: 13/01/2019 8:09:45 PM ******/
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
/****** Object:  Table [dbo].[ExHRemitDataDetails]    Script Date: 13/01/2019 8:09:45 PM ******/
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
/****** Object:  Table [dbo].[ExHUser]    Script Date: 13/01/2019 8:09:45 PM ******/
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
/****** Object:  Table [dbo].[Module]    Script Date: 13/01/2019 8:09:45 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Module](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[ImageName] [nvarchar](50) NULL,
	[Ordering] [tinyint] NULL,
	[IsActive] [bit] NULL,
 CONSTRAINT [PK_ModuleName] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Role]    Script Date: 13/01/2019 8:09:45 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Role](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](50) NULL,
 CONSTRAINT [PK_Role] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[RoleSubModuleItem]    Script Date: 13/01/2019 8:09:45 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RoleSubModuleItem](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RoleId] [int] NULL,
	[SubModuleItemId] [int] NULL,
	[CreateOperation] [bit] NULL,
	[ReadOperation] [bit] NULL,
	[UpdateOperation] [bit] NULL,
	[DeleteOperation] [bit] NULL,
 CONSTRAINT [PK_RoleSubModuleItem] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[SubModule]    Script Date: 13/01/2019 8:09:45 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SubModule](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ModuleId] [int] NULL,
	[Name] [nvarchar](100) NULL,
	[Ordering] [tinyint] NULL,
	[IsActive] [bit] NULL,
 CONSTRAINT [PK_SubModule] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[SubModuleItem]    Script Date: 13/01/2019 8:09:45 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SubModuleItem](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[SubModuleId] [int] NULL,
	[Name] [nvarchar](200) NULL,
	[UrlPath] [nvarchar](200) NULL,
	[Ordering] [tinyint] NULL,
	[IsBaseItem] [bit] NULL,
	[IsActive] [bit] NULL,
	[BaseItemId] [int] NULL,
 CONSTRAINT [PK_SubModuleItem] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Workflowaction]    Script Date: 13/01/2019 8:09:45 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Workflowaction](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](50) NULL,
 CONSTRAINT [PK_Workflowactions] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[WorkflowactionSetting]    Script Date: 13/01/2019 8:09:45 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[WorkflowactionSetting](
	[Id] [uniqueidentifier] NOT NULL,
	[SubMouduleItemId] [int] NULL,
	[EmployeeId] [int] NULL,
	[WorkflowactionId] [int] NULL,
 CONSTRAINT [PK_WorkflowactionSetting] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
ALTER TABLE [dbo].[ActionLog] ADD  CONSTRAINT [DF_ActionLog_Id]  DEFAULT (newid()) FOR [Id]
GO
ALTER TABLE [dbo].[Module] ADD  CONSTRAINT [DF_Module_IsActive]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[SubModule] ADD  CONSTRAINT [DF_SubModule_IsActive]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[SubModuleItem] ADD  CONSTRAINT [DF_SubModuleItem_IsBaseItem]  DEFAULT ((0)) FOR [IsBaseItem]
GO
ALTER TABLE [dbo].[SubModuleItem] ADD  CONSTRAINT [DF_SubModuleItem_IsActive]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[ActionLog]  WITH NOCHECK ADD  CONSTRAINT [FK_ActionLog_User] FOREIGN KEY([Who])
REFERENCES [dbo].[BusinessUser] ([Id])
GO
ALTER TABLE [dbo].[ActionLog] CHECK CONSTRAINT [FK_ActionLog_User]
GO
ALTER TABLE [dbo].[Branch]  WITH CHECK ADD  CONSTRAINT [FK_Branch_Country] FOREIGN KEY([CountryId])
REFERENCES [dbo].[Country] ([Id])
GO
ALTER TABLE [dbo].[Branch] CHECK CONSTRAINT [FK_Branch_Country]
GO
ALTER TABLE [dbo].[BusinessUser]  WITH NOCHECK ADD  CONSTRAINT [FK_User_Employee] FOREIGN KEY([EmployeeId])
REFERENCES [dbo].[Employee] ([Id])
GO
ALTER TABLE [dbo].[BusinessUser] CHECK CONSTRAINT [FK_User_Employee]
GO
ALTER TABLE [dbo].[BusinessUser]  WITH NOCHECK ADD  CONSTRAINT [FK_User_Role] FOREIGN KEY([RoleId])
REFERENCES [dbo].[Role] ([Id])
GO
ALTER TABLE [dbo].[BusinessUser] CHECK CONSTRAINT [FK_User_Role]
GO
ALTER TABLE [dbo].[City]  WITH NOCHECK ADD  CONSTRAINT [FK_City_Country] FOREIGN KEY([CountryId])
REFERENCES [dbo].[Country] ([Id])
GO
ALTER TABLE [dbo].[City] CHECK CONSTRAINT [FK_City_Country]
GO
ALTER TABLE [dbo].[Company]  WITH NOCHECK ADD  CONSTRAINT [FK_Company_BaseCurrency] FOREIGN KEY([BaseCurrency])
REFERENCES [dbo].[Currency] ([Id])
GO
ALTER TABLE [dbo].[Company] CHECK CONSTRAINT [FK_Company_BaseCurrency]
GO
ALTER TABLE [dbo].[Company]  WITH NOCHECK ADD  CONSTRAINT [FK_Company_LocalCurrency] FOREIGN KEY([LocalCurrency])
REFERENCES [dbo].[Currency] ([Id])
GO
ALTER TABLE [dbo].[Company] CHECK CONSTRAINT [FK_Company_LocalCurrency]
GO
ALTER TABLE [dbo].[Designation]  WITH NOCHECK ADD  CONSTRAINT [FK_Designation_Department] FOREIGN KEY([DepartmentId])
REFERENCES [dbo].[Department] ([Id])
GO
ALTER TABLE [dbo].[Designation] CHECK CONSTRAINT [FK_Designation_Department]
GO
ALTER TABLE [dbo].[ExHIPAddress]  WITH CHECK ADD  CONSTRAINT [FK_ExHIPAddress_ExchangeHouse] FOREIGN KEY([ExchangeHouseID])
REFERENCES [dbo].[ExchangeHouse] ([Id])
GO
ALTER TABLE [dbo].[ExHIPAddress] CHECK CONSTRAINT [FK_ExHIPAddress_ExchangeHouse]
GO
ALTER TABLE [dbo].[ExHUser]  WITH CHECK ADD  CONSTRAINT [FK_ExHUser_ExchangeHouse] FOREIGN KEY([ExchangeHouseId])
REFERENCES [dbo].[ExchangeHouse] ([Id])
GO
ALTER TABLE [dbo].[ExHUser] CHECK CONSTRAINT [FK_ExHUser_ExchangeHouse]
GO
ALTER TABLE [dbo].[RoleSubModuleItem]  WITH NOCHECK ADD  CONSTRAINT [FK_RoleSubModuleItem_Role] FOREIGN KEY([RoleId])
REFERENCES [dbo].[Role] ([Id])
GO
ALTER TABLE [dbo].[RoleSubModuleItem] CHECK CONSTRAINT [FK_RoleSubModuleItem_Role]
GO
ALTER TABLE [dbo].[RoleSubModuleItem]  WITH NOCHECK ADD  CONSTRAINT [FK_RoleSubModuleItem_SubModuleItem] FOREIGN KEY([SubModuleItemId])
REFERENCES [dbo].[SubModuleItem] ([Id])
GO
ALTER TABLE [dbo].[RoleSubModuleItem] CHECK CONSTRAINT [FK_RoleSubModuleItem_SubModuleItem]
GO
ALTER TABLE [dbo].[SubModule]  WITH CHECK ADD  CONSTRAINT [FK_SubModule_Module] FOREIGN KEY([ModuleId])
REFERENCES [dbo].[Module] ([Id])
GO
ALTER TABLE [dbo].[SubModule] CHECK CONSTRAINT [FK_SubModule_Module]
GO
ALTER TABLE [dbo].[SubModuleItem]  WITH NOCHECK ADD  CONSTRAINT [FK_SubModuleItem_SubModule] FOREIGN KEY([SubModuleId])
REFERENCES [dbo].[SubModule] ([Id])
GO
ALTER TABLE [dbo].[SubModuleItem] CHECK CONSTRAINT [FK_SubModuleItem_SubModule]
GO
ALTER TABLE [dbo].[SubModuleItem]  WITH NOCHECK ADD  CONSTRAINT [FK_SubModuleItem_SubModuleItem] FOREIGN KEY([BaseItemId])
REFERENCES [dbo].[SubModuleItem] ([Id])
GO
ALTER TABLE [dbo].[SubModuleItem] CHECK CONSTRAINT [FK_SubModuleItem_SubModuleItem]
GO
ALTER TABLE [dbo].[WorkflowactionSetting]  WITH NOCHECK ADD  CONSTRAINT [FK_WorkflowactionSetting_Employee] FOREIGN KEY([EmployeeId])
REFERENCES [dbo].[Employee] ([Id])
GO
ALTER TABLE [dbo].[WorkflowactionSetting] CHECK CONSTRAINT [FK_WorkflowactionSetting_Employee]
GO
ALTER TABLE [dbo].[WorkflowactionSetting]  WITH NOCHECK ADD  CONSTRAINT [FK_WorkflowactionSetting_SubModuleItem] FOREIGN KEY([SubMouduleItemId])
REFERENCES [dbo].[SubModuleItem] ([Id])
GO
ALTER TABLE [dbo].[WorkflowactionSetting] CHECK CONSTRAINT [FK_WorkflowactionSetting_SubModuleItem]
GO
ALTER TABLE [dbo].[WorkflowactionSetting]  WITH NOCHECK ADD  CONSTRAINT [FK_WorkflowactionSetting_Workflowaction] FOREIGN KEY([WorkflowactionId])
REFERENCES [dbo].[Workflowaction] ([Id])
GO
ALTER TABLE [dbo].[WorkflowactionSetting] CHECK CONSTRAINT [FK_WorkflowactionSetting_Workflowaction]
GO
