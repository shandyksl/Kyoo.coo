CREATE DEFINER=`yoouser`@`%` PROCEDURE `SpTransfer`(
IN inAgentCode VARCHAR(30),
IN inLoginName VARCHAR(255),
IN inTransactionRef VARCHAR(255),
IN inAmount DECIMAL(18,2),
IN inTransactionType INT,
IN inCreatedBy VARCHAR(100)
)
BEGIN
	DECLARE customException CONDITION FOR SQLSTATE '45000';
	DECLARE playerId INT;
	DECLARE playerStatus INT;
	DECLARE errMsg TEXT;
	DECLARE shortErrMsg VARCHAR(128);
	DECLARE playerCurrency VARCHAR(10);
	DECLARE playerBalance DECIMAL(18,2);
	DECLARE playerBalanceAfter DECIMAL(18,2);
	DECLARE transferType INT;
	DECLARE errorCode INT DEFAULT NULL;
	
	DECLARE EXIT HANDLER FOR SQLEXCEPTION
	BEGIN
		GET DIAGNOSTICS CONDITION 1 errMsg = message_text;
		SET shortErrMsg = LEFT(errMsg,128);
		ROLLBACK;
		IF errorCode IS NOT NULL THEN
        SIGNAL customException SET MESSAGE_TEXT = shortErrMsg, MYSQL_ERRNO = errorCode;
    ELSE
        SIGNAL customException SET MESSAGE_TEXT = shortErrMsg;
    END IF;
	END;
	
	mainTx: BEGIN
		START TRANSACTION;

		SELECT `Id`,`Balance`,`Status`,`CurrencyCode` INTO playerId, playerBalance, playerStatus,playerCurrency
		FROM Player
		WHERE (LoginName = inLoginName AND AgentCode = inAgentCode)
		FOR UPDATE;
		SET playerBalanceAfter = playerBalance + inAmount;

		IF (SELECT COUNT(*) FROM `Transaction` WHERE TransactionId = inTransactionRef) > 0 THEN SET errMsg = '交易编号已存在!', errorCode = 204;
		ELSEIF playerId IS NULL THEN SET errMsg = '玩家不存在!', errorCode = 106;
		ELSEIF playerStatus != 1 THEN SET errMsg = '玩家并未开通!', errorCode = 101;
		ELSEIF playerBalance IS NULL THEN SET errMsg = '查找不了该玩家的钱包!', errorCode = 112;
		ELSEIF inAmount < 0 AND playerBalance < ABS(inAmount) THEN SET errMsg = '钱包余额不足!', errorCode = 113;
		END IF;

		IF inAmount > 0 THEN SET transferType = 1;
		ELSE SET transferType = 2;
		END IF;

		IF errMsg IS NOT NULL THEN
			SIGNAL customException SET MESSAGE_TEXT = errMsg;
			LEAVE mainTx;
		END IF;
		
		UPDATE Player SET Balance = playerBalanceAfter, UpdatedAt = CURRENT_TIMESTAMP() WHERE Id = playerId;
		
		INSERT INTO `Transaction` (AgentCode,LoginName,TransferType,TransactionId,Amount,CreatedBy,CreatedAt)
		VALUES(inAgentCode,inLoginName,transferType,inTransactionRef,inAmount,inCreatedBy,CURRENT_TIMESTAMP());
		
		INSERT INTO PlayerBalanceLog (AgentCode,LoginName,AfterBalance,BeforeBalance,TransactionAmount,TransactionType,TransactionReference,CurrencyCode,CreatedBy)
		VALUES(inAgentCode,inLoginName,playerBalanceAfter,playerBalance,inAmount,inTransactionType,inTransactionRef,playerCurrency,inCreatedBy);
    SELECT TransactionId FROM `Transaction` WHERE TransactionId = inTransactionRef;
        
		COMMIT;
		
	END mainTx;
END