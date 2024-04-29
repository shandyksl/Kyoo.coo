CREATE DEFINER=`yoouser`@`%` PROCEDURE `SpPlaceBet`(
IN inAgentCode VARCHAR(50),
IN inLoginName VARCHAR(100),
IN inCurrencyCode VARCHAR(5),
IN inTransactionId VARCHAR(200),
IN inGameType VARCHAR(50),
IN inGameCode VARCHAR(200),
IN inRoundId VARCHAR(200),
IN inBetAmount DECIMAL(14,4),
IN inBetTime TIMESTAMP,
IN inBetDetailQuery LONGTEXT,
IN inCreatedBy VARCHAR(200)
)
BEGIN
	DECLARE customException CONDITION FOR SQLSTATE '45000';
	DECLARE errMsg TEXT;
	DECLARE shortErrMsg VARCHAR(128);
	DECLARE errorCode INT DEFAULT NULL;
	DECLARE playerBalance DECIMAL(14,4);
	DECLARE playerStatus INT;
	DECLARE EXIT HANDLER FOR SQLEXCEPTION
	BEGIN
		GET DIAGNOSTICS CONDITION 1 errMsg = message_text;
		SET shortErrMsg = REPLACE(errMsg,'You have an error in your SQL syntax; check the manual that corresponds to your MySQL server version for the right syntax to use','Syntax Error');
		ROLLBACK;
		IF errorCode IS NOT NULL THEN
        SIGNAL customException SET MESSAGE_TEXT = shortErrMsg, MYSQL_ERRNO = errorCode;
    ELSE
        SIGNAL customException SET MESSAGE_TEXT = shortErrMsg;
    END IF;
	END;
	
	mainTx: BEGIN
		START TRANSACTION;
	
		SELECT `Balance`,`Status` INTO playerBalance,playerStatus
		FROM Player
		WHERE (LoginName = inLoginName AND AgentCode = inAgentCode AND CurrencyCode = inCurrencyCode)
		FOR UPDATE;

		IF (SELECT COUNT(*) FROM BetHistory WHERE TransactionId = inTransactionId) > 0 THEN SET errMsg = '单号已存在!', errorCode = 400;
		ELSEIF playerBalance IS NULL THEN SET errMsg = '玩家不存在!', errorCode = 106;
		ELSEIF playerStatus != 1 THEN SET errMsg = '玩家未开通!', errorCode = 101;
		ELSEIF (SELECT COUNT(*) FROM GameInfo WHERE GameType = inGameType AND GameCode = inGameCode AND RoundId = inRoundId) = 0 THEN SET errMsg = '游戏不存在', errorCode = 501;
		ELSEIF inBetAmount > (SELECT MaxBetAmount FROM Player WHERE LoginName = inLoginName AND AgentCode = inAgentCode AND CurrencyCode = inCurrencyCode) THEN SET errMsg = '下注金额已超额', errorCode = 502;
		ELSEIF playerBalance < ABS(inBetAmount) THEN SET errMsg = '钱包余额不足!', errorCode = 113;
		END IF;

		IF errMsg IS NOT NULL THEN
			SIGNAL customException SET MESSAGE_TEXT = errMsg;
			LEAVE mainTx;
		END IF;
		
		CALL SpTransfer(inAgentCode, inLoginName, inTransactionId, inBetAmount*-1,3,inCreatedBy);
		
		INSERT INTO BetHistory(AgentCode,LoginName,GameType,GameCode,TransactionId,RoundId,BetAmount,BetTime)
		VALUES(inAgentCode,inLoginName,inGameType,inGameCode,inTransactionId,inRoundId,inBetAmount,inBetTime);
		
		SET @ticketQuery = inBetDetailQuery;
    PREPARE stmt FROM @ticketQuery;
    EXECUTE stmt;
    DEALLOCATE PREPARE stmt;

		SELECT TransactionId FROM `Transaction` WHERE TransactionId = inTransactionId;
        
		COMMIT;
		
	END mainTx;
END