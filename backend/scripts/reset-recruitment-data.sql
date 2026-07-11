-- ============================================================================
-- RESET RECRUITMENT DATA (all tenants) — CERP
-- Empties every recruitment table for a from-scratch end-to-end test:
--   interviews (feedback/panelists/rounds), offers, applications (+scores/logs),
--   candidates (+documents, background attachments, candidate-only persons and
--   their education/experience rows), criteria (+evaluators), requisitions,
--   hiring requests, recruitment workflow instances, and the number counters
--   (numbering restarts at 0001).
-- PRESERVED: employees hired through recruitment (and their person records,
--   education/experience, and Recruitment-owner documents — employee data now),
--   org structure, workflow definitions, and every other module.
-- Re-runnable; wrap of a single transaction.
-- ============================================================================
SET QUOTED_IDENTIFIER ON; SET ANSI_NULLS ON; SET NOCOUNT ON;
BEGIN TRAN;

-- Candidate-only persons: created for candidates and NOT referenced by any employee.
DECLARE @candidatePersons TABLE (Id uniqueidentifier PRIMARY KEY);
INSERT INTO @candidatePersons (Id)
SELECT DISTINCT c.PersonId
FROM Core.hrms_Candidate c
WHERE c.PersonId IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM Core.hrms_Employee e WHERE e.PersonId = c.PersonId);

-- 1. Interviews
DELETE FROM Core.hrms_InterviewFeedback;
DELETE FROM Core.hrms_InterviewPanelist;
DELETE FROM Core.hrms_Interview;

-- 2. Offers
DELETE FROM Core.hrms_JobOffer;

-- 3. Applications (+ scores + stage logs)
DELETE FROM Core.hrms_ApplicationCriterionScore;
DELETE FROM Core.hrms_JobApplicationStageLog;
DELETE FROM Core.hrms_JobApplication;

-- 4. Candidate files: typed compliance documents + pre-hire background attachments
--    (attachments still anchored to a CANDIDATE id — hired ones were re-anchored to employees).
DELETE FROM Core.hrms_CandidateDocument;
DELETE d FROM Core.hrms_EmployeeDocument d
WHERE d.EmployeeId IN (SELECT Id FROM Core.hrms_Candidate);

-- 5. Candidate-only persons' education/experience rows (+ any attachments on those rows)
DELETE d FROM Core.hrms_EmployeeDocument d
WHERE d.OwnerType IN (N'Education', N'Experience')
  AND (d.OwnerId IN (SELECT Id FROM Core.hrms_EmployeeEducation
                     WHERE PersonId IN (SELECT Id FROM @candidatePersons))
    OR d.OwnerId IN (SELECT Id FROM Core.hrms_EmployeeExperience
                     WHERE PersonId IN (SELECT Id FROM @candidatePersons)));
DELETE FROM Core.hrms_EmployeeEducation  WHERE PersonId IN (SELECT Id FROM @candidatePersons);
DELETE FROM Core.hrms_EmployeeExperience WHERE PersonId IN (SELECT Id FROM @candidatePersons);

-- 6. Candidates, then their now-orphaned person records
DELETE FROM Core.hrms_Candidate;
DELETE FROM Core.CorePerson WHERE Id IN (SELECT Id FROM @candidatePersons);

-- 7. Criteria (+ evaluators), requisitions, hiring requests
DELETE FROM Core.hrms_CriterionEvaluator;
DELETE FROM Core.hrms_RequisitionScreeningCriterion;
DELETE FROM Core.hrms_JobRequisition;
DELETE FROM Core.hrms_HiringRequest;

-- 8. Recruitment workflow runs (definitions stay — they are configuration)
DELETE l FROM Core.hrms_WorkflowActionLog l
WHERE l.InstanceId IN (SELECT Id FROM Core.hrms_WorkflowInstance
                       WHERE EntityType IN (N'HiringRequest', N'JobRequisition', N'JobOffer'));
DELETE FROM Core.hrms_WorkflowInstance
WHERE EntityType IN (N'HiringRequest', N'JobRequisition', N'JobOffer');

-- 9. Number counters — HRQ/REQ/CND/OFR numbering restarts at 0001
DELETE FROM Core.hrms_NumberSequence
WHERE [Key] IN (N'HiringRequest', N'JobRequisition', N'Candidate', N'JobOffer');

COMMIT;

-- Verification
SELECT 'hrms_HiringRequest'    AS TableName, COUNT(*) AS Remaining FROM Core.hrms_HiringRequest
UNION ALL SELECT 'hrms_JobRequisition', COUNT(*) FROM Core.hrms_JobRequisition
UNION ALL SELECT 'hrms_RequisitionScreeningCriterion', COUNT(*) FROM Core.hrms_RequisitionScreeningCriterion
UNION ALL SELECT 'hrms_CriterionEvaluator', COUNT(*) FROM Core.hrms_CriterionEvaluator
UNION ALL SELECT 'hrms_Candidate', COUNT(*) FROM Core.hrms_Candidate
UNION ALL SELECT 'hrms_CandidateDocument', COUNT(*) FROM Core.hrms_CandidateDocument
UNION ALL SELECT 'hrms_JobApplication', COUNT(*) FROM Core.hrms_JobApplication
UNION ALL SELECT 'hrms_JobApplicationStageLog', COUNT(*) FROM Core.hrms_JobApplicationStageLog
UNION ALL SELECT 'hrms_ApplicationCriterionScore', COUNT(*) FROM Core.hrms_ApplicationCriterionScore
UNION ALL SELECT 'hrms_Interview', COUNT(*) FROM Core.hrms_Interview
UNION ALL SELECT 'hrms_InterviewPanelist', COUNT(*) FROM Core.hrms_InterviewPanelist
UNION ALL SELECT 'hrms_InterviewFeedback', COUNT(*) FROM Core.hrms_InterviewFeedback
UNION ALL SELECT 'hrms_JobOffer', COUNT(*) FROM Core.hrms_JobOffer
UNION ALL SELECT 'hrms_NumberSequence (recruitment keys)', COUNT(*) FROM Core.hrms_NumberSequence
    WHERE [Key] IN (N'HiringRequest', N'JobRequisition', N'Candidate', N'JobOffer');
