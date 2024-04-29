CREATE SCHEMA `YooAsset` DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

CREATE TABLE `Asset` (
  `Id` int unsigned NOT NULL AUTO_INCREMENT,
  `Symbol` varchar(10) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL COMMENT '代号: BTC', 
  `Name` varchar(10) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL COMMENT '名称: 比特币', 
  `CurrencyCode` VARCHAR(5) NOT NULL COMMENT '货币代码',
  `Type` varchar(10) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL COMMENT '种类： SMI, CRYPTO',
  `CreatedAt` timestamp NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
  `IsActive` INT UNSIGNED NOT NULL DEFAULT 1 COMMENT '0-未开通,1-已开通',
  PRIMARY KEY (`Id`),
  UNIQUE (Symbol),
  KEY `AssetTypeIndex` (`Type`),
  KEY `AssetSymbolIndex` (`Symbol`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Asset.Type = "stock", "cryptocurrency"

CREATE TABLE `AssetPrice` (
  `Id` int unsigned NOT NULL AUTO_INCREMENT,
  `Symbol` varchar(10) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL COMMENT '代号: BTC',
  `Price` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL COMMENT '价钱',
  `CreatedAt` timestamp NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
  PRIMARY KEY (`Id`),
  KEY `AssetPriceSymbolIndex` (`Symbol`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
