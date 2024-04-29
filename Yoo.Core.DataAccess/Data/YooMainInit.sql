CREATE TABLE `AgentConfig` (
  `Id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  `AgentCode` VARCHAR(50) NOT NULL COMMENT '代理',
  `ApiKey` VARCHAR(255) NULL COMMENT '代理密钥',
  `BetDbUrl` VARCHAR(255) NULL COMMENT '投注数据库链接',
  `Description` VARCHAR(255) NULL COMMENT '信息',
  `IsActive` INT UNSIGNED NOT NULL DEFAULT 1 COMMENT '0-未开通,1-已开通',
  `CreatedAt` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
  `UpdatedAt` TIMESTAMP NULL COMMENT '更新时间',
  PRIMARY KEY (`Id`),
  KEY `AgentConfigAgentCodeIndex` (`AgentCode`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE utf8mb4_unicode_ci;

CREATE TABLE `Player` (
  `Id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  `AgentCode` VARCHAR(50) NOT NULL COMMENT '代理',
  `LoginName` VARCHAR(100) NOT NULL COMMENT '登录名',
  `NickName` VARCHAR(100) NULL COMMENT '昵称',
  `Balance` DECIMAL(14,4) NOT NULL DEFAULT 0 COMMENT '余额',
  `IpAddress` VARCHAR(50) NULL COMMENT 'Ip地址',
  `LanguageCode` VARCHAR(5) NULL COMMENT '语言代码',
  `CurrencyCode` VARCHAR(5) NOT NULL COMMENT '货币代码',
  `Status` INT UNSIGNED NOT NULL DEFAULT 1 COMMENT '1-已开通,2-未开通,3-已封锁',
  `MaxBetAmount` DECIMAL(14,4) NOT NULL DEFAULT 100000 COMMENT '最大投注金额',
  `LastLoginTime` TIMESTAMP NULL COMMENT '最后上线时间',
  `LastActiveTime` TIMESTAMP NULL COMMENT '最后活跃时间',
  `CreatedAt` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
  `UpdatedAt` TIMESTAMP NULL COMMENT '更新时间',
  PRIMARY KEY (`Id`),
  KEY `PlayerAgentCodeIndex` (`AgentCode`),
  KEY `PlayerLoginNameIndex` (`LoginName`),
  KEY `PlayerStatusIndex` (`Status`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE utf8mb4_unicode_ci;

CREATE TABLE `Transaction` (
  `Id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  `AgentCode` VARCHAR(50) NOT NULL COMMENT '代理',
  `LoginName` VARCHAR(100) NOT NULL COMMENT '登录名',
  `TransferType` INT UNSIGNED NOT NULL COMMENT '1-转进,2-转出',
  `TransactionId` VARCHAR(100) NOT NULL COMMENT '交易单号',
  `Amount` DECIMAL(14,4) NOT NULL COMMENT '交易金额',
  `CreatedBy` VARCHAR(100) NOT NULL COMMENT '创建人',
  `CreatedAt` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
  `UpdatedAt` TIMESTAMP NULL COMMENT '更新时间',
  PRIMARY KEY (`Id`),
  UNIQUE (TransactionId),
  KEY `TransactionAgentCodeIndex` (`AgentCode`),
  KEY `TransactionLoginNameIndex` (`LoginName`),
  KEY `TransactionTransferTypeIndex` (`TransferType`),
  KEY `TransactionTransactionIdIndex` (`TransactionId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE utf8mb4_unicode_ci;

CREATE TABLE `PlayerBalanceLog` (
  `Id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  `AgentCode` VARCHAR(50) NOT NULL COMMENT '代理',
  `LoginName` VARCHAR(100) NOT NULL COMMENT '登录名',
  `BeforeBalance` DECIMAL(14,4) NOT NULL COMMENT '之前余额',
  `AfterBalance` DECIMAL(14,4) NOT NULL COMMENT '之后余额',
  `TransactionAmount` DECIMAL(14,4) NOT NULL COMMENT '交易金额',
  `TransactionType` INT UNSIGNED NOT NULL COMMENT '1-转进,2-转出,3-下注,4-结算,5-退款',
  `TransactionReference` VARCHAR(100) NOT NULL COMMENT '交易单号',
  `CurrencyCode` VARCHAR(5) NOT NULL COMMENT '货币代码',
  `CreatedBy` VARCHAR(100) NOT NULL COMMENT '创建人',
  `CreatedAt` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
  PRIMARY KEY (`Id`),
  KEY `PlayerBalanceLogAgentCodeIndex` (`AgentCode`),
  KEY `PlayerBalanceLogLoginNameIndex` (`LoginName`),
  KEY `PlayerBalanceLogTransactionTypeIndex` (`TransactionType`),
  KEY `PlayerBalanceLogTransactionReferenceIndex` (`TransactionReference`)
)
-- PARTITION BY RANGE COLUMNS (YEAR(CreatedAt), MONTH(CreatedAt)) INTERVAL (1)
ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE utf8mb4_unicode_ci;

CREATE TABLE `PlayerAuthLog` (
  `Id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  `AgentCode` VARCHAR(50) NOT NULL COMMENT '代理',
  `LoginName` VARCHAR(100) NOT NULL COMMENT '登录名',
  `Token` VARCHAR(255) NOT NULL COMMENT 'JWT 验证码',
  `Platform` VARCHAR(50) NOT NULL COMMENT '登录平台',
  `CreatedAt` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
  PRIMARY KEY (`Id`),
  KEY `PlayerAuthLogAgentCodeIndex` (`AgentCode`),
  KEY `PlayerAuthLogLoginNameIndex` (`LoginName`)
)
-- PARTITION BY RANGE COLUMNS (YEAR(CreatedAt), MONTH(CreatedAt)) INTERVAL (1)
ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE utf8mb4_unicode_ci;

CREATE TABLE `BetHistory` (
  `Id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  `AgentCode` VARCHAR(50) NOT NULL COMMENT '代理',
  `LoginName` VARCHAR(100) NOT NULL COMMENT '登录名',
  `GameType` VARCHAR(50) NOT NULL COMMENT '游戏分类',
  `GameCode` VARCHAR(50) NOT NULL COMMENT '游戏代号',
  `TransactionId` VARCHAR(255) NOT NULL COMMENT '订单号',
  `RoundId` VARCHAR(255) NOT NULL COMMENT '期号',
  `BetAmount` DECIMAL(14,4) NOT NULL COMMENT '投注金额',
  `WinAmount` DECIMAL(14,4) DEFAULT 0 COMMENT '结算金额',
  `BetTime` timestamp NOT NULL COMMENT '投注时间',
  `SettleStatus` INT UNSIGNED DEFAULT 1 COMMENT '1-未结算,2-结算中,3-已结算',
  `SettleTime` TIMESTAMP NULL COMMENT '结算时间',
  `CreatedAt` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
  `UpdatedAt` TIMESTAMP NULL COMMENT '更新时间',
  PRIMARY KEY (`Id`),
  KEY `BetHistoryAgentCodeIndex` (`AgentCode`),
  KEY `BetHistoryLoginNameIndex` (`LoginName`),
  KEY `BetHistoryGameTypeIndex` (`GameType`),
  KEY `BetHistoryGameCodeIndex` (`GameCode`),
  KEY `BetHistoryTransactionIdIndex` (`TransactionId`),
  KEY `BetHistoryRoundIdIndex` (`RoundId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE utf8mb4_unicode_ci;


CREATE TABLE `BetDetail` (
  `Id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  `BetHistoryId` BIGINT UNSIGNED NOT NULL COMMENT '投注记录ID',
  `BetType` INT UNSIGNED NOT NULL COMMENT '下注选项类型',
  `BetCode` VARCHAR(50) NOT NULL COMMENT '下注选择',
  `Odds` DECIMAL(14,4) NOT NULL COMMENT '赔率',
  `BetAmount` DECIMAL(14,4) NOT NULL COMMENT '投注金额',
  `WinAmount` DECIMAL(14,4) DEFAULT 0 NULL COMMENT '结算金额',
  `BetResult` VARCHAR(50) DEFAULT NULL COMMENT '投注结果',
  `Status` INT UNSIGNED DEFAULT 1 COMMENT '1-未结算,2-已结算',
  `CreatedAt` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
  `UpdatedAt` TIMESTAMP NULL COMMENT '更新时间',
  PRIMARY KEY (`Id`),
  KEY `BetDetailBetIdIndex` (`BetHistoryId`),
  KEY `BetDetailBetTypeIndex` (`BetType`),
  KEY `BetDetailBetCodeIndex` (`BetCode`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE utf8mb4_unicode_ci;

CREATE TABLE `GameConfig` (
  `Id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  `AgentCode` VARCHAR(50) NOT NULL COMMENT '代理',
  `GameType` VARCHAR(15) NOT NULL COMMENT '游戏分类: STCP,CFFC,BOCP,EVCP',
  `GameName` VARCHAR(50) NOT NULL COMMENT '游戏名称: 股指彩票, BTC分分彩, 电影票房下注, 大事件下注',
  `GameSettings` JSON NULL COMMENT '游戏配置，赔率等等',
  `GameBadge` INT UNSIGNED NOT NULL DEFAULT 1 COMMENT '游戏热度: 1 - 普通 2 - 新 3 - 热门',
  `Rank` INT UNSIGNED DEFAULT 0 COMMENT '排序值 越小越前面',
  `IsActive` INT UNSIGNED NOT NULL DEFAULT 1 COMMENT '0-未开通,1-已开通',
  `CreatedAt` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
  `UpdatedAt` TIMESTAMP NULL COMMENT '更新时间',
  PRIMARY KEY (`Id`),
  KEY `GameConfigAgentCodeIndex` (`AgentCode`),
  KEY `GameConfigGameTypeIndex` (`GameType`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE utf8mb4_unicode_ci;

CREATE TABLE `GameInfo` (
  `Id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  `GameType` VARCHAR(15) NOT NULL COMMENT '游戏分类: STCP,CFFC,BOCP,EVCP',
  `GameCode` VARCHAR(50) NOT NULL COMMENT '游戏代号: BTC',
  `GameIntro` VARCHAR(100) NULL COMMENT '游戏详情',
  `RoundId` VARCHAR(50) NOT NULL COMMENT '期号',
  `IssueStartTime` TIMESTAMP NOT NULL COMMENT '事件开始时间',
  `IssueEndTime` TIMESTAMP NOT NULL COMMENT '事件结束时间',
  `StartBuyTime` TIMESTAMP NOT NULL COMMENT '开始购买时间',
  `EndBuyTime` TIMESTAMP NOT NULL COMMENT '结束购买时间',
  `State` INT UNSIGNED NOT NULL DEFAULT 1 COMMENT '1 - 开售 2 - 关闭 3 - 结束',
  `ResultPrice` DECIMAL(14,4) NULL DEFAULT 0 COMMENT '结果-值(股票/虚拟币/电影票房等用',
  `ResultInfo` JSON NULL COMMENT '结果-选项(非值结果大事件用',
  `ResultTime` TIMESTAMP NULL COMMENT '开奖时间',
  `CreatedBy` VARCHAR(100) NOT NULL COMMENT '创建人',
  `CreatedAt` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
  `UpdatedAt` TIMESTAMP NULL COMMENT '更新时间',
  PRIMARY KEY (`Id`),
  KEY `GameInfoGameTypeIndex` (`GameType`),
  KEY `GameInfoGameCodeIndex` (`GameCode`),
  KEY `GameInfoRoundIdIndex` (`RoundId`),
  KEY `GameInfoStateIndex` (`State`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE utf8mb4_unicode_ci;

CREATE TABLE `GameOption` (
  `Id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  `AgentCode` VARCHAR(50) NOT NULL COMMENT '代理',
  `RoundId` VARCHAR(50) NOT NULL COMMENT '期号',
  `OptionType` INT UNSIGNED NOT NULL COMMENT '下注选项类型：1-股票/虚拟币选大小；2-股票/虚拟币选单双；3-股票/虚拟币选数字；4-电影票房选项；5-大事件选项',
  `OptionCode` VARCHAR(25) NOT NULL COMMENT '对应OptionType类型, big, small etc',
  `OptionName` VARCHAR(50) NULL COMMENT '类型名字',
  `OptionValue` DECIMAL(14, 4) NULL COMMENT '用于储存票房',
  `Odds` DECIMAL(14,4) NOT NULL COMMENT '赔率',
  `Status` INT UNSIGNED NOT NULL DEFAULT 1 COMMENT '1 - 已开通 2 - 未开通',
  `Rank` INT UNSIGNED NOT NULL DEFAULT 0 COMMENT '排序值 越小越前面',
  `CreatedBy` VARCHAR(100) NOT NULL COMMENT '创建人',
  `CreatedAt` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
  PRIMARY KEY (`Id`),
  KEY `GameOptionAgentCodeIndex` (`AgentCode`),
  KEY `GameOptionRoundIdIndex` (`RoundId`),
  KEY `GameOptionOptionTypeIndex` (`OptionType`),
  KEY `GameOptionOptionCodeIndex` (`OptionCode`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE utf8mb4_unicode_ci;

CREATE TABLE `GameManagement` (
  `Id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  `GameType` VARCHAR(15) NOT NULL COMMENT '游戏分类: STCP,CFFC,BOCP,EVCP',
  `GameName` VARCHAR(50) NOT NULL COMMENT '游戏名称: 股指彩票, BTC分分彩, 电影票房下注, 大事件下注',
  `IsActive` INT UNSIGNED NOT NULL DEFAULT 1 COMMENT '0-未开通,1-已开通',
  `MaintenanceStart` TIMESTAMP NULL COMMENT '维修开始时间',
  `MaintenanceEnd` TIMESTAMP NULL COMMENT '维修结束时间: 如果开始不是NULL结束是NULL就代表永久维修',
  PRIMARY KEY (`Id`),
  KEY `GameManagementGameTypeIndex` (`GameType`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE utf8mb4_unicode_ci;