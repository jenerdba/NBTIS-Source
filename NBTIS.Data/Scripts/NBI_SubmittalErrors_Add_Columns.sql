IF NOT EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'SubmittalErrors' 
    AND COLUMN_NAME IN ('Ignore', 'Reviewed', 'Comments', 'ReviewedBy','IgnoredBy','ReviewedDate','IgnoredDate')
)
BEGIN
    ALTER TABLE SubmittalErrors
    ADD 
        [Ignore] BIT NOT NULL DEFAULT 0,
        [Reviewed] BIT NOT NULL DEFAULT 0,
        [Comments] VARCHAR(1000) NULL,
        [ReviewedBy] VARCHAR(200) NULL,
        [IgnoredBy] VARCHAR(200) NULL,
        [ReviewedDate] DATETIME NULL,
        [IgnoredDate] DATETIME NULL;
END;
