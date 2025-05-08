

IF COL_LENGTH('SubmittalLogs', 'ApproveRejectComments') IS NULL
BEGIN
    ALTER TABLE SubmittalLogs
    ADD ApproveRejectComments NVARCHAR(1000)
END

go
CREATE OR ALTER VIEW [NBI].[VSubmittalLogs]    
AS    
SELECT 
    ROW_NUMBER() OVER (ORDER BY SubmitId) AS SubmittalLogsViewID,
    *
FROM
(
    -- First part: States
    SELECT
        LS.Description AS Lookup_States_Description,
        LS.Abbreviation AS Lookup_States_Abbreviation,
        UPPER(COALESCE(LStatus.StatusDescription, 'Not Started')) AS Lookup_Statuses_StatusDescription,
        1 AS StateOrder,
        SL.SubmitId,
        CAST(SL.SubmittedBy AS VARCHAR(10)) AS SubmittedBy,
        SL.IsPartial,
        SL.StatusCode,
        CASE 
            WHEN SL.ReportContent IS NOT NULL THEN 'Yes'
            ELSE NULL
        END AS ReportContent,
        SL.UploadDate,
        SL.UploadedBy,
        SL.SubmitDate,
        SL.Submitter,
        SL.ReviewDate,
        SL.Reviewer,
        SL.ApproveRejectDate,		
        SL.Approver
		,SL.ApproveRejectComments
    FROM [NBI].[Lookup_States] LS
    LEFT JOIN [NBI].SubmittalLogs SL ON SL.SubmittedBy = CAST(LS.Code AS VARCHAR(10))
    LEFT JOIN [NBI].[Lookup_Statuses] LStatus ON SL.StatusCode = LStatus.StatusCode
    WHERE 
        (
            LStatus.StatusDescription IN (
                'Division Review', 'HQ Review', 'Rejected', 'Accepted', 'Returned By Division'
            )
        )
        OR LStatus.StatusDescription IS NULL

    UNION ALL

    -- Second part: Agencies
    SELECT
        UPPER(LV.Description) AS Lookup_States_Description,
        LV.Code AS Lookup_States_Abbreviation,
        UPPER(COALESCE(LS.StatusDescription, 'Not Started')) AS Lookup_Statuses_StatusDescription,
        2 AS StateOrder,
        SL.SubmitId,
        CAST(SL.SubmittedBy AS VARCHAR(10)) AS SubmittedBy,
        SL.IsPartial,
        SL.StatusCode,
        CASE 
            WHEN SL.ReportContent IS NOT NULL THEN 'Yes'
            ELSE NULL
        END AS ReportContent,
        SL.UploadDate,
        SL.UploadedBy,
        SL.SubmitDate,
        SL.Submitter,
        SL.ReviewDate,
        SL.Reviewer,
        SL.ApproveRejectDate,
        SL.Approver
		,SL.ApproveRejectComments
    FROM [NBI].[LookupValues] LV
    LEFT JOIN [NBI].SubmittalLogs SL ON CAST(SL.SubmittedBy AS VARCHAR(10)) = LV.Code
    LEFT JOIN [NBI].[Lookup_Statuses] LS ON SL.StatusCode = LS.StatusCode
    WHERE 
        LV.FieldName = 'BCL01'
        AND LV.IsActive = 1
        AND LV.Code NOT LIKE 'S%'
        AND LV.Code NOT LIKE 'L%'
        AND LV.Code NOT LIKE 'P%'
        AND (
            (LS.StatusDescription IN (
                'Division Review', 'HQ Review', 'Rejected', 'Accepted', 'Returned By Division'
            ))
            OR LS.StatusDescription IS NULL
        )
) AS Combined;

GO


