/* ---------------------------------------------------------------------------
   Precondition. This script is NOT idempotent (see build-scripts.ps1 for why),
   so it refuses to run unless the database is exactly at
   20260808112235_SalaryRevisionPerformanceBands and has not already been renamed.
   --------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM [dbo].[__EFMigrationsHistory]
               WHERE [MigrationId] = N'20260808112235_SalaryRevisionPerformanceBands')
BEGIN
    RAISERROR('ABORTED: database is not at SalaryRevisionPerformanceBands. Apply earlier HRMS migrations first.', 16, 1);
    SET NOEXEC ON;
END
GO
IF EXISTS (SELECT 1 FROM [dbo].[__EFMigrationsHistory] WHERE [MigrationId] LIKE N'%_ModuleSchemaRename')
BEGIN
    RAISERROR('ABORTED: ModuleSchemaRename has already been applied to this database.', 16, 1);
    SET NOEXEC ON;
END
GO
BEGIN TRANSACTION;
ALTER TABLE [dbo].[coreModule] DROP CONSTRAINT [FK_coreModule_coreSubsystem_SubsystemId];

ALTER TABLE [dbo].[coreOperation] DROP CONSTRAINT [FK_coreOperation_coreModule_ModuleId];

ALTER TABLE [Core].[coreSalaryScale] DROP CONSTRAINT [FK_coreSalaryScale_hrmsJobGrade_JobGradeId];

ALTER TABLE [Core].[coreSalaryScale] DROP CONSTRAINT [FK_coreSalaryScale_lupStep_StepId];

ALTER TABLE [dbo].[hrmsAchievement] DROP CONSTRAINT [FK_hrmsAchievement_hrmsAppraisal_AppraisalId];

ALTER TABLE [dbo].[hrmsAchievement] DROP CONSTRAINT [FK_hrmsAchievement_hrmsEmployee_EmployeeId];

ALTER TABLE [dbo].[hrmsAnnouncement] DROP CONSTRAINT [FK_hrmsAnnouncement_hrmsBranch_BranchId];

ALTER TABLE [dbo].[hrmsAnnouncement] DROP CONSTRAINT [FK_hrmsAnnouncement_hrmsOrganizationUnit_OrganizationUnitId];

ALTER TABLE [dbo].[hrmsAnnualLeaveDetail] DROP CONSTRAINT [FK_hrmsAnnualLeaveDetail_hrmsAnnualLeaveHeader_AnnualLeaveHeaderId];

ALTER TABLE [dbo].[hrmsAnnualLeaveHeader] DROP CONSTRAINT [FK_hrmsAnnualLeaveHeader_hrmsEmployee_EmployeeId];

ALTER TABLE [dbo].[hrmsAnnualLeaveHeader] DROP CONSTRAINT [FK_hrmsAnnualLeaveHeader_hrmsLeaveBalance_AnnualLeaveLedgerId];

ALTER TABLE [dbo].[hrmsAnnualLeaveSetting] DROP CONSTRAINT [FK_hrmsAnnualLeaveSetting_FiscalYear_FiscalYearId];

ALTER TABLE [dbo].[hrmsApplicationCriterionScore] DROP CONSTRAINT [FK_hrmsApplicationCriterionScore_hrmsJobApplication_ApplicationId];

ALTER TABLE [dbo].[hrmsAppraisal] DROP CONSTRAINT [FK_hrmsAppraisal_hrmsEmployee_EmployeeId];

ALTER TABLE [dbo].[hrmsAppraisal] DROP CONSTRAINT [FK_hrmsAppraisal_hrmsReviewCycle_ReviewCycleId];

ALTER TABLE [dbo].[hrmsAppraisalAppeal] DROP CONSTRAINT [FK_hrmsAppraisalAppeal_hrmsAppraisal_AppraisalId];

ALTER TABLE [dbo].[hrmsAppraisalAppeal] DROP CONSTRAINT [FK_hrmsAppraisalAppeal_hrmsEmployee_EmployeeId];

ALTER TABLE [dbo].[hrmsAppraisalCompetency] DROP CONSTRAINT [FK_hrmsAppraisalCompetency_hrmsAppraisal_AppraisalId];

ALTER TABLE [dbo].[hrmsAppraisalGoal] DROP CONSTRAINT [FK_hrmsAppraisalGoal_hrmsAppraisal_AppraisalId];

ALTER TABLE [dbo].[hrmsAppraisalPeerReview] DROP CONSTRAINT [FK_hrmsAppraisalPeerReview_hrmsAppraisal_AppraisalId];

ALTER TABLE [dbo].[hrmsAppraisalPeerReview] DROP CONSTRAINT [FK_hrmsAppraisalPeerReview_hrmsEmployee_PeerEmployeeId];

ALTER TABLE [dbo].[hrmsBranch] DROP CONSTRAINT [FK_hrmsBranch_hrmsBranch_ParentId];

ALTER TABLE [dbo].[hrmsCalibrationItem] DROP CONSTRAINT [FK_hrmsCalibrationItem_hrmsAppraisal_AppraisalId];

ALTER TABLE [dbo].[hrmsCalibrationItem] DROP CONSTRAINT [FK_hrmsCalibrationItem_hrmsCalibrationSession_CalibrationSessionId];

ALTER TABLE [dbo].[hrmsCalibrationSession] DROP CONSTRAINT [FK_hrmsCalibrationSession_hrmsOrganizationUnit_OrganizationUnitId];

ALTER TABLE [dbo].[hrmsCalibrationSession] DROP CONSTRAINT [FK_hrmsCalibrationSession_hrmsReviewCycle_ReviewCycleId];

ALTER TABLE [dbo].[hrmsCandidate] DROP CONSTRAINT [FK_hrmsCandidate_CorePerson_PersonId];

ALTER TABLE [dbo].[hrmsCandidate] DROP CONSTRAINT [FK_hrmsCandidate_hrmsEmployee_InternalEmployeeId];

ALTER TABLE [dbo].[hrmsCandidateDocument] DROP CONSTRAINT [FK_hrmsCandidateDocument_hrmsCandidate_CandidateId];

ALTER TABLE [dbo].[hrmsCareerPathChangeRequest] DROP CONSTRAINT [FK_hrmsCareerPathChangeRequest_hrmsCareerPath_CurrentCareerPathId];

ALTER TABLE [dbo].[hrmsCareerPathChangeRequest] DROP CONSTRAINT [FK_hrmsCareerPathChangeRequest_hrmsCareerPath_RequestedCareerPathId];

ALTER TABLE [dbo].[hrmsCareerPathChangeRequest] DROP CONSTRAINT [FK_hrmsCareerPathChangeRequest_hrmsEmployee_EmployeeId];

ALTER TABLE [dbo].[hrmsCareerPathStep] DROP CONSTRAINT [FK_hrmsCareerPathStep_hrmsCareerPath_CareerPathId];

ALTER TABLE [dbo].[hrmsCareerPathStep] DROP CONSTRAINT [FK_hrmsCareerPathStep_hrmsJobGrade_JobGradeId];

ALTER TABLE [dbo].[hrmsCareerPathStep] DROP CONSTRAINT [FK_hrmsCareerPathStep_hrmsPositionClass_PositionClassId];

ALTER TABLE [dbo].[hrmsCareerPathStepCompetency] DROP CONSTRAINT [FK_hrmsCareerPathStepCompetency_hrmsCareerPathStep_CareerPathStepId];

ALTER TABLE [dbo].[hrmsCareerPathStepCompetency] DROP CONSTRAINT [FK_hrmsCareerPathStepCompetency_hrmsCompetency_CompetencyId];

ALTER TABLE [dbo].[hrmsClearanceDepartmentApprover] DROP CONSTRAINT [FK_hrmsClearanceDepartmentApprover_hrmsClearanceDepartment_DepartmentId];

ALTER TABLE [dbo].[hrmsCommunityPostReaction] DROP CONSTRAINT [FK_hrmsCommunityPostReaction_hrmsEmployee_EmployeeId];

ALTER TABLE [dbo].[hrmsCommunityPostReaction] DROP CONSTRAINT [FK_hrmsCommunityPostReaction_hrmsLearningCommunityPost_LearningCommunityPostId];

ALTER TABLE [dbo].[hrmsCompanyAsset] DROP CONSTRAINT [FK_hrmsCompanyAsset_hrmsEmployee_AssignedToEmployeeId];

ALTER TABLE [dbo].[hrmsCompensationRequest] DROP CONSTRAINT [FK_hrmsCompensationRequest_hrmsEmployee_EmployeeId];

ALTER TABLE [dbo].[hrmsCompetency] DROP CONSTRAINT [FK_hrmsCompetency_hrmsCompetencyCategory_CompetencyCategoryId];

ALTER TABLE [dbo].[hrmsCriterionEvaluator] DROP CONSTRAINT [FK_hrmsCriterionEvaluator_hrmsEmployee_EmployeeId];

ALTER TABLE [dbo].[hrmsCriterionEvaluator] DROP CONSTRAINT [FK_hrmsCriterionEvaluator_hrmsRequisitionScreeningCriterion_CriterionId];

ALTER TABLE [dbo].[hrmsCriticalPosition] DROP CONSTRAINT [FK_hrmsCriticalPosition_hrmsPosition_PositionId];

ALTER TABLE [dbo].[hrmsDevelopmentAction] DROP CONSTRAINT [FK_hrmsDevelopmentAction_hrmsCompetency_CompetencyId];

ALTER TABLE [dbo].[hrmsDevelopmentAction] DROP CONSTRAINT [FK_hrmsDevelopmentAction_hrmsDevelopmentPlan_DevelopmentPlanId];

ALTER TABLE [dbo].[hrmsDevelopmentPlan] DROP CONSTRAINT [FK_hrmsDevelopmentPlan_hrmsAppraisal_AppraisalId];

ALTER TABLE [dbo].[hrmsDevelopmentPlan] DROP CONSTRAINT [FK_hrmsDevelopmentPlan_hrmsEmployee_EmployeeId];

ALTER TABLE [dbo].[hrmsDisciplinaryMeasure] DROP CONSTRAINT [FK_hrmsDisciplinaryMeasure_hrmsEmployee_EmployeeId];

ALTER TABLE [dbo].[hrmsDynamicFormField] DROP CONSTRAINT [FK_hrmsDynamicFormField_hrmsDynamicForm_DynamicFormId];

ALTER TABLE [dbo].[hrmsDynamicFormRecord] DROP CONSTRAINT [FK_hrmsDynamicFormRecord_hrmsDynamicForm_DynamicFormId];

ALTER TABLE [dbo].[hrmsEmployee] DROP CONSTRAINT [FK_hrmsEmployee_CorePerson_PersonId];

ALTER TABLE [dbo].[hrmsEmployee] DROP CONSTRAINT [FK_hrmsEmployee_coreSalaryScale_SalaryScaleId];

ALTER TABLE [dbo].[hrmsEmployee] DROP CONSTRAINT [FK_hrmsEmployee_hrmsBranch_BranchId];

ALTER TABLE [dbo].[hrmsEmployee] DROP CONSTRAINT [FK_hrmsEmployee_hrmsPosition_PositionId];

ALTER TABLE [dbo].[hrmsEmployeeAllowance] DROP CONSTRAINT [FK_hrmsEmployeeAllowance_hrmsAllowanceType_AllowanceTypeId];

ALTER TABLE [dbo].[hrmsEmployeeAllowance] DROP CONSTRAINT [FK_hrmsEmployeeAllowance_hrmsEmployee_EmployeeId];

ALTER TABLE [dbo].[hrmsEmployeeBenefitEnrollment] DROP CONSTRAINT [FK_hrmsEmployeeBenefitEnrollment_hrmsBenefitPlan_BenefitPlanId];

ALTER TABLE [dbo].[hrmsEmployeeBenefitEnrollment] DROP CONSTRAINT [FK_hrmsEmployeeBenefitEnrollment_hrmsEmployee_EmployeeId];

ALTER TABLE [dbo].[hrmsEmployeeCareerPath] DROP CONSTRAINT [FK_hrmsEmployeeCareerPath_hrmsCareerPath_CareerPathId];

ALTER TABLE [dbo].[hrmsEmployeeCareerPath] DROP CONSTRAINT [FK_hrmsEmployeeCareerPath_hrmsEmployee_EmployeeId];

ALTER TABLE [dbo].[hrmsEmployeeCareerPathStepProgress] DROP CONSTRAINT [FK_hrmsEmployeeCareerPathStepProgress_hrmsEmployeeCareerPath_EmployeeCareerPathId];

ALTER TABLE [dbo].[hrmsEmployeeDependent] DROP CONSTRAINT [FK_hrmsEmployeeDependent_CorePerson_PersonId];

ALTER TABLE [dbo].[hrmsEmployeeDependent] DROP CONSTRAINT [FK_hrmsEmployeeDependent_hrmsEmployee_RelatedEmployeeId];

ALTER TABLE [dbo].[hrmsEmployeeEducation] DROP CONSTRAINT [FK_hrmsEmployeeEducation_CorePerson_PersonId];

ALTER TABLE [dbo].[hrmsEmployeeExperience] DROP CONSTRAINT [FK_hrmsEmployeeExperience_CorePerson_PersonId];

ALTER TABLE [dbo].[hrmsEmployeeFieldValue] DROP CONSTRAINT [FK_hrmsEmployeeFieldValue_hrmsEmployeeFieldDefinition_FieldDefinitionId];

ALTER TABLE [dbo].[hrmsEmployeeGoal] DROP CONSTRAINT [FK_hrmsEmployeeGoal_hrmsEmployee_EmployeeId];

ALTER TABLE [dbo].[hrmsEmployeeGoal] DROP CONSTRAINT [FK_hrmsEmployeeGoal_hrmsOrganizationalObjective_OrganizationalObjectiveId];

ALTER TABLE [dbo].[hrmsEmployeeGoal] DROP CONSTRAINT [FK_hrmsEmployeeGoal_hrmsReviewCycle_ReviewCycleId];

ALTER TABLE [dbo].[hrmsEmployeeGuarantee] DROP CONSTRAINT [FK_hrmsEmployeeGuarantee_hrmsEmployee_EmployeeId];

ALTER TABLE [dbo].[hrmsEmployeeMovement] DROP CONSTRAINT [FK_hrmsEmployeeMovement_coreSalaryScale_ToSalaryScaleId];

ALTER TABLE [dbo].[hrmsEmployeeMovement] DROP CONSTRAINT [FK_hrmsEmployeeMovement_hrmsEmployee_EmployeeId];

ALTER TABLE [dbo].[hrmsEmployeeRecognition] DROP CONSTRAINT [FK_hrmsEmployeeRecognition_hrmsEmployee_EmployeeId];

ALTER TABLE [dbo].[hrmsEmployeeRecognition] DROP CONSTRAINT [FK_hrmsEmployeeRecognition_hrmsRecognitionBadge_RecognitionBadgeId];

ALTER TABLE [dbo].[hrmsEmployeeTermination] DROP CONSTRAINT [FK_hrmsEmployeeTermination_hrmsEmployee_EmployeeId];

ALTER TABLE [dbo].[hrmsEmployeeTrainingCertificate] DROP CONSTRAINT [FK_hrmsEmployeeTrainingCertificate_hrmsEmployee_EmployeeId];

ALTER TABLE [dbo].[hrmsEmployeeTrainingCertificate] DROP CONSTRAINT [FK_hrmsEmployeeTrainingCertificate_hrmsTrainingCourse_TrainingCourseId];

ALTER TABLE [dbo].[hrmsEmployeeTrainingCertificate] DROP CONSTRAINT [FK_hrmsEmployeeTrainingCertificate_hrmsTrainingEnrollment_TrainingEnrollmentId];

ALTER TABLE [dbo].[hrmsExitInterview] DROP CONSTRAINT [FK_hrmsExitInterview_hrmsEmployeeTermination_TerminationId];

ALTER TABLE [dbo].[hrmsGoalActionItem] DROP CONSTRAINT [FK_hrmsGoalActionItem_hrmsEmployeeGoal_EmployeeGoalId];

ALTER TABLE [dbo].[hrmsGrievance] DROP CONSTRAINT [FK_hrmsGrievance_hrmsEmployee_EmployeeId];

ALTER TABLE [dbo].[hrmsGrievanceNote] DROP CONSTRAINT [FK_hrmsGrievanceNote_hrmsGrievance_GrievanceId];

ALTER TABLE [dbo].[hrmsHiringRequest] DROP CONSTRAINT [FK_hrmsHiringRequest_hrmsOrganizationUnit_OrganizationUnitId];

ALTER TABLE [dbo].[hrmsHiringRequest] DROP CONSTRAINT [FK_hrmsHiringRequest_hrmsPositionClass_PositionClassId];

ALTER TABLE [dbo].[hrmsImprovementPlan] DROP CONSTRAINT [FK_hrmsImprovementPlan_hrmsAppraisal_AppraisalId];

ALTER TABLE [dbo].[hrmsImprovementPlan] DROP CONSTRAINT [FK_hrmsImprovementPlan_hrmsEmployee_EmployeeId];

ALTER TABLE [dbo].[hrmsInsuranceClaim] DROP CONSTRAINT [FK_hrmsInsuranceClaim_hrmsEmployee_EmployeeId];

ALTER TABLE [dbo].[hrmsInsuranceClaim] DROP CONSTRAINT [FK_hrmsInsuranceClaim_hrmsInsurancePolicy_InsurancePolicyId];

ALTER TABLE [dbo].[hrmsInsuranceClaimAttachment] DROP CONSTRAINT [FK_hrmsInsuranceClaimAttachment_hrmsInsuranceClaim_InsuranceClaimId];

ALTER TABLE [dbo].[hrmsInsurancePremiumSchedule] DROP CONSTRAINT [FK_hrmsInsurancePremiumSchedule_hrmsInsurancePolicy_InsurancePolicyId];

ALTER TABLE [dbo].[hrmsInterview] DROP CONSTRAINT [FK_hrmsInterview_hrmsJobApplication_ApplicationId];

ALTER TABLE [dbo].[hrmsInterviewFeedback] DROP CONSTRAINT [FK_hrmsInterviewFeedback_hrmsInterviewPanelist_PanelistId];

ALTER TABLE [dbo].[hrmsInterviewPanelist] DROP CONSTRAINT [FK_hrmsInterviewPanelist_hrmsEmployee_EmployeeId];

ALTER TABLE [dbo].[hrmsInterviewPanelist] DROP CONSTRAINT [FK_hrmsInterviewPanelist_hrmsInterview_InterviewId];

ALTER TABLE [dbo].[hrmsJobApplication] DROP CONSTRAINT [FK_hrmsJobApplication_hrmsCandidate_CandidateId];

ALTER TABLE [dbo].[hrmsJobApplication] DROP CONSTRAINT [FK_hrmsJobApplication_hrmsJobRequisition_RequisitionId];

ALTER TABLE [dbo].[hrmsJobApplicationStageLog] DROP CONSTRAINT [FK_hrmsJobApplicationStageLog_hrmsJobApplication_ApplicationId];

ALTER TABLE [dbo].[hrmsJobOffer] DROP CONSTRAINT [FK_hrmsJobOffer_coreSalaryScale_SalaryScaleId];

ALTER TABLE [dbo].[hrmsJobOffer] DROP CONSTRAINT [FK_hrmsJobOffer_hrmsEmployee_HiringManagerEmployeeId];

ALTER TABLE [dbo].[hrmsJobOffer] DROP CONSTRAINT [FK_hrmsJobOffer_hrmsJobApplication_ApplicationId];

ALTER TABLE [dbo].[hrmsJobRequisition] DROP CONSTRAINT [FK_hrmsJobRequisition_coreSalaryScale_SalaryScaleId];

ALTER TABLE [dbo].[hrmsJobRequisition] DROP CONSTRAINT [FK_hrmsJobRequisition_hrmsHiringRequest_HiringRequestId];

ALTER TABLE [dbo].[hrmsJobRequisition] DROP CONSTRAINT [FK_hrmsJobRequisition_hrmsOrganizationUnit_OrganizationUnitId];

ALTER TABLE [dbo].[hrmsJobRequisition] DROP CONSTRAINT [FK_hrmsJobRequisition_hrmsPositionClass_PositionClassId];

ALTER TABLE [dbo].[hrmsJobRequisition] DROP CONSTRAINT [FK_hrmsJobRequisition_hrmsWorkLocation_WorkLocationId];

ALTER TABLE [dbo].[hrmsKnowledgeTransfer] DROP CONSTRAINT [FK_hrmsKnowledgeTransfer_hrmsEmployee_FromEmployeeId];

ALTER TABLE [dbo].[hrmsKnowledgeTransfer] DROP CONSTRAINT [FK_hrmsKnowledgeTransfer_hrmsSuccessionCandidate_SuccessionCandidateId];

ALTER TABLE [dbo].[hrmsLearningCommunity] DROP CONSTRAINT [FK_hrmsLearningCommunity_hrmsTrainingCourse_TrainingCourseId];

ALTER TABLE [dbo].[hrmsLearningCommunityMember] DROP CONSTRAINT [FK_hrmsLearningCommunityMember_hrmsEmployee_EmployeeId];

ALTER TABLE [dbo].[hrmsLearningCommunityMember] DROP CONSTRAINT [FK_hrmsLearningCommunityMember_hrmsLearningCommunity_LearningCommunityId];

ALTER TABLE [dbo].[hrmsLearningCommunityPost] DROP CONSTRAINT [FK_hrmsLearningCommunityPost_hrmsEmployee_EmployeeId];

ALTER TABLE [dbo].[hrmsLearningCommunityPost] DROP CONSTRAINT [FK_hrmsLearningCommunityPost_hrmsLearningCommunity_LearningCommunityId];

ALTER TABLE [dbo].[hrmsLearningPath] DROP CONSTRAINT [FK_hrmsLearningPath_hrmsPosition_TargetPositionId];

ALTER TABLE [dbo].[hrmsLearningPathStep] DROP CONSTRAINT [FK_hrmsLearningPathStep_hrmsLearningPath_LearningPathId];

ALTER TABLE [dbo].[hrmsLearningPathStep] DROP CONSTRAINT [FK_hrmsLearningPathStep_hrmsTrainingCourse_TrainingCourseId];

ALTER TABLE [dbo].[hrmsLeaveBalance] DROP CONSTRAINT [FK_hrmsLeaveBalance_FiscalYear_FiscalYearId];

ALTER TABLE [dbo].[hrmsLeaveBalance] DROP CONSTRAINT [FK_hrmsLeaveBalance_hrmsEmployee_EmployeeId];

ALTER TABLE [dbo].[hrmsLeaveBalance] DROP CONSTRAINT [FK_hrmsLeaveBalance_hrmsLeaveType_LeaveTypeId];

ALTER TABLE [dbo].[hrmsLeaveRequest] DROP CONSTRAINT [FK_hrmsLeaveRequest_FiscalYear_FiscalYearId];

ALTER TABLE [dbo].[hrmsLeaveRequest] DROP CONSTRAINT [FK_hrmsLeaveRequest_hrmsEmployee_EmployeeId];

ALTER TABLE [dbo].[hrmsLeaveRequestLine] DROP CONSTRAINT [FK_hrmsLeaveRequestLine_hrmsLeaveRequest_LeaveRequestId];

ALTER TABLE [dbo].[hrmsLeaveRequestLine] DROP CONSTRAINT [FK_hrmsLeaveRequestLine_hrmsLeaveType_LeaveTypeId];

ALTER TABLE [dbo].[hrmsLoan] DROP CONSTRAINT [FK_hrmsLoan_hrmsEmployee_EmployeeId];

ALTER TABLE [dbo].[hrmsLoan] DROP CONSTRAINT [FK_hrmsLoan_hrmsLoanType_LoanTypeId];

ALTER TABLE [dbo].[hrmsLoanGuarantor] DROP CONSTRAINT [FK_hrmsLoanGuarantor_hrmsLoan_LoanId];

ALTER TABLE [dbo].[hrmsLoanRepaymentSchedule] DROP CONSTRAINT [FK_hrmsLoanRepaymentSchedule_hrmsLoan_LoanId];

ALTER TABLE [dbo].[hrmsMedicalBeneficiary] DROP CONSTRAINT [FK_hrmsMedicalBeneficiary_hrmsMedicalEnrollment_MedicalEnrollmentId];

ALTER TABLE [dbo].[hrmsMedicalClaim] DROP CONSTRAINT [FK_hrmsMedicalClaim_hrmsEmployee_EmployeeId];

ALTER TABLE [dbo].[hrmsMedicalClaim] DROP CONSTRAINT [FK_hrmsMedicalClaim_hrmsMedicalEnrollment_MedicalEnrollmentId];

ALTER TABLE [dbo].[hrmsMedicalClaimAttachment] DROP CONSTRAINT [FK_hrmsMedicalClaimAttachment_hrmsMedicalClaim_MedicalClaimId];

ALTER TABLE [dbo].[hrmsMedicalEnrollment] DROP CONSTRAINT [FK_hrmsMedicalEnrollment_hrmsEmployee_EmployeeId];

ALTER TABLE [dbo].[hrmsMedicalEnrollment] DROP CONSTRAINT [FK_hrmsMedicalEnrollment_hrmsMedicalPlan_MedicalPlanId];

ALTER TABLE [dbo].[hrmsMedicalServiceContract] DROP CONSTRAINT [FK_hrmsMedicalServiceContract_hrmsMedicalProvider_MedicalProviderId];

ALTER TABLE [dbo].[hrmsMentorship] DROP CONSTRAINT [FK_hrmsMentorship_hrmsEmployee_MenteeEmployeeId];

ALTER TABLE [dbo].[hrmsMentorship] DROP CONSTRAINT [FK_hrmsMentorship_hrmsEmployee_MentorEmployeeId];

ALTER TABLE [dbo].[hrmsOrganizationalObjective] DROP CONSTRAINT [FK_hrmsOrganizationalObjective_hrmsOrganizationUnit_OrganizationUnitId];

ALTER TABLE [dbo].[hrmsOrganizationalObjective] DROP CONSTRAINT [FK_hrmsOrganizationalObjective_hrmsOrganizationalObjective_ParentObjectiveId];

ALTER TABLE [dbo].[hrmsOrganizationalObjective] DROP CONSTRAINT [FK_hrmsOrganizationalObjective_hrmsReviewCycle_ReviewCycleId];

ALTER TABLE [dbo].[hrmsOrganizationUnit] DROP CONSTRAINT [FK_hrmsOrganizationUnit_hrmsBranch_BranchId];

ALTER TABLE [dbo].[hrmsOrganizationUnit] DROP CONSTRAINT [FK_hrmsOrganizationUnit_hrmsOrganizationUnit_ParentId];

ALTER TABLE [dbo].[hrmsOrganizationUnit] DROP CONSTRAINT [FK_hrmsOrganizationUnit_hrmsWorkLocation_WorkLocationId];

ALTER TABLE [dbo].[hrmsOtherLeave] DROP CONSTRAINT [FK_hrmsOtherLeave_hrmsEmployee_EmployeeId];

ALTER TABLE [dbo].[hrmsOtherLeave] DROP CONSTRAINT [FK_hrmsOtherLeave_hrmsOtherLeaveSetting_OtherLeaveSettingId];

ALTER TABLE [dbo].[hrmsOtherLeaveDetail] DROP CONSTRAINT [FK_hrmsOtherLeaveDetail_hrmsOtherLeave_OtherLeaveHeaderId];

ALTER TABLE [dbo].[hrmsOtherLeaveSetting] DROP CONSTRAINT [FK_hrmsOtherLeaveSetting_FiscalYear_FiscalYearId];

ALTER TABLE [dbo].[hrmsOtherLeaveSetting] DROP CONSTRAINT [FK_hrmsOtherLeaveSetting_hrmsLeaveType_LeaveTypeId];

ALTER TABLE [dbo].[hrmsPerDiemRate] DROP CONSTRAINT [FK_hrmsPerDiemRate_hrmsJobGrade_JobGradeId];

ALTER TABLE [dbo].[hrmsPipObjective] DROP CONSTRAINT [FK_hrmsPipObjective_hrmsImprovementPlan_PipId];

ALTER TABLE [dbo].[hrmsPosition] DROP CONSTRAINT [FK_hrmsPosition_hrmsBranch_BranchId];

ALTER TABLE [dbo].[hrmsPosition] DROP CONSTRAINT [FK_hrmsPosition_hrmsOrganizationUnit_OrganizationUnitId];

ALTER TABLE [dbo].[hrmsPosition] DROP CONSTRAINT [FK_hrmsPosition_hrmsPositionClass_PositionClassId];

ALTER TABLE [dbo].[hrmsPositionClass] DROP CONSTRAINT [FK_hrmsPositionClass_coreSalaryScale_SalaryScaleId];

ALTER TABLE [dbo].[hrmsPositionClass] DROP CONSTRAINT [FK_hrmsPositionClass_hrmsJobCategory_JobCategoryId];

ALTER TABLE [dbo].[hrmsPositionClass] DROP CONSTRAINT [FK_hrmsPositionClass_hrmsPositionClass_ReportsToPositionClassId];

ALTER TABLE [dbo].[hrmsPositionClass] DROP CONSTRAINT [FK_hrmsPositionClass_hrmsWorkLocation_WorkLocationId];

ALTER TABLE [dbo].[hrmsPositionCompetency] DROP CONSTRAINT [FK_hrmsPositionCompetency_hrmsCompetency_CompetencyId];

ALTER TABLE [dbo].[hrmsPositionCompetency] DROP CONSTRAINT [FK_hrmsPositionCompetency_hrmsPosition_PositionId];

ALTER TABLE [dbo].[hrmsProfileChangeRequest] DROP CONSTRAINT [FK_hrmsProfileChangeRequest_hrmsEmployee_EmployeeId];

ALTER TABLE [dbo].[hrmsRatingScaleLevel] DROP CONSTRAINT [FK_hrmsRatingScaleLevel_hrmsRatingScale_RatingScaleId];

ALTER TABLE [dbo].[hrmsRecognitionBadge] DROP CONSTRAINT [FK_hrmsRecognitionBadge_hrmsAwardCategory_AwardCategoryId];

ALTER TABLE [dbo].[hrmsRecognitionProgram] DROP CONSTRAINT [FK_hrmsRecognitionProgram_hrmsRecognitionBadge_RecognitionBadgeId];

ALTER TABLE [dbo].[hrmsReportField] DROP CONSTRAINT [FK_hrmsReportField_hrmsReport_ReportId];

ALTER TABLE [dbo].[hrmsReportFieldOutput] DROP CONSTRAINT [FK_hrmsReportFieldOutput_hrmsReport_ReportId];

ALTER TABLE [dbo].[hrmsReportRestriction] DROP CONSTRAINT [FK_hrmsReportRestriction_hrmsReport_ReportId];

ALTER TABLE [dbo].[hrmsReportRunRecipient] DROP CONSTRAINT [FK_hrmsReportRunRecipient_hrmsReportRun_ReportRunId];

ALTER TABLE [dbo].[hrmsReportSavedFilter] DROP CONSTRAINT [FK_hrmsReportSavedFilter_hrmsReport_ReportId];

ALTER TABLE [dbo].[hrmsReportSchedule] DROP CONSTRAINT [FK_hrmsReportSchedule_hrmsReport_ReportId];

ALTER TABLE [dbo].[hrmsReportScheduleFieldOutput] DROP CONSTRAINT [FK_hrmsReportScheduleFieldOutput_hrmsReportSchedule_ReportScheduleId];

ALTER TABLE [dbo].[hrmsReportScheduleFieldValue] DROP CONSTRAINT [FK_hrmsReportScheduleFieldValue_hrmsReportSchedule_ReportScheduleId];

ALTER TABLE [dbo].[hrmsReportScheduleRecipient] DROP CONSTRAINT [FK_hrmsReportScheduleRecipient_hrmsReportSchedule_ReportScheduleId];

ALTER TABLE [dbo].[hrmsRequisitionScreeningCriterion] DROP CONSTRAINT [FK_hrmsRequisitionScreeningCriterion_hrmsJobRequisition_RequisitionId];

ALTER TABLE [dbo].[hrmsReviewCycle] DROP CONSTRAINT [FK_hrmsReviewCycle_FiscalYear_FiscalYearId];

ALTER TABLE [dbo].[hrmsReviewCycle] DROP CONSTRAINT [FK_hrmsReviewCycle_hrmsRatingScale_RatingScaleId];

ALTER TABLE [dbo].[hrmsRewardDisbursement] DROP CONSTRAINT [FK_hrmsRewardDisbursement_hrmsEmployeeRecognition_EmployeeRecognitionId];

ALTER TABLE [dbo].[hrmsRewardDisbursement] DROP CONSTRAINT [FK_hrmsRewardDisbursement_hrmsEmployee_EmployeeId];

ALTER TABLE [dbo].[hrmsRewardDisbursement] DROP CONSTRAINT [FK_hrmsRewardDisbursement_hrmsRecognitionBadge_RecognitionBadgeId];

ALTER TABLE [dbo].[hrmsRewardNomination] DROP CONSTRAINT [FK_hrmsRewardNomination_hrmsEmployee_NomineeEmployeeId];

ALTER TABLE [dbo].[hrmsRewardNomination] DROP CONSTRAINT [FK_hrmsRewardNomination_hrmsRecognitionBadge_RecognitionBadgeId];

ALTER TABLE [dbo].[hrmsRewardNomination] DROP CONSTRAINT [FK_hrmsRewardNomination_hrmsRecognitionProgram_RecognitionProgramId];

ALTER TABLE [dbo].[hrmsRewardPointsTransaction] DROP CONSTRAINT [FK_hrmsRewardPointsTransaction_hrmsEmployee_EmployeeId];

ALTER TABLE [dbo].[hrmsSalaryRevisionBand] DROP CONSTRAINT [FK_hrmsSalaryRevisionBand_hrmsSalaryRevision_SalaryRevisionId];

ALTER TABLE [dbo].[hrmsSalaryRevisionLine] DROP CONSTRAINT [FK_hrmsSalaryRevisionLine_hrmsEmployee_EmployeeId];

ALTER TABLE [dbo].[hrmsSalaryRevisionLine] DROP CONSTRAINT [FK_hrmsSalaryRevisionLine_hrmsSalaryRevision_SalaryRevisionId];

ALTER TABLE [dbo].[hrmsSettlementLine] DROP CONSTRAINT [FK_hrmsSettlementLine_hrmsTerminationSettlement_TerminationSettlementId];

ALTER TABLE [dbo].[hrmsSuccessionCandidate] DROP CONSTRAINT [FK_hrmsSuccessionCandidate_hrmsEmployee_EmployeeId];

ALTER TABLE [dbo].[hrmsSuccessionCandidate] DROP CONSTRAINT [FK_hrmsSuccessionCandidate_hrmsSuccessionPlan_SuccessionPlanId];

ALTER TABLE [dbo].[hrmsSuccessionDevelopmentAction] DROP CONSTRAINT [FK_hrmsSuccessionDevelopmentAction_hrmsEmployee_MentorEmployeeId];

ALTER TABLE [dbo].[hrmsSuccessionDevelopmentAction] DROP CONSTRAINT [FK_hrmsSuccessionDevelopmentAction_hrmsSuccessionCandidate_SuccessionCandidateId];

ALTER TABLE [dbo].[hrmsSuccessionPlan] DROP CONSTRAINT [FK_hrmsSuccessionPlan_hrmsCriticalPosition_CriticalPositionId];

ALTER TABLE [dbo].[hrmsSurveyCompletion] DROP CONSTRAINT [FK_hrmsSurveyCompletion_hrmsEmployee_EmployeeId];

ALTER TABLE [dbo].[hrmsSurveyCompletion] DROP CONSTRAINT [FK_hrmsSurveyCompletion_hrmsSurvey_SurveyId];

ALTER TABLE [dbo].[hrmsSurveyResponse] DROP CONSTRAINT [FK_hrmsSurveyResponse_hrmsSurvey_SurveyId];

ALTER TABLE [dbo].[hrmsTalentAssessment] DROP CONSTRAINT [FK_hrmsTalentAssessment_hrmsEmployee_EmployeeId];

ALTER TABLE [dbo].[hrmsTalentAssessment] DROP CONSTRAINT [FK_hrmsTalentAssessment_hrmsTalentReview_TalentReviewId];

ALTER TABLE [dbo].[hrmsTalentRating] DROP CONSTRAINT [FK_hrmsTalentRating_hrmsEmployee_RaterEmployeeId];

ALTER TABLE [dbo].[hrmsTalentRating] DROP CONSTRAINT [FK_hrmsTalentRating_hrmsTalentAssessment_TalentAssessmentId];

ALTER TABLE [dbo].[hrmsTalentReview] DROP CONSTRAINT [FK_hrmsTalentReview_hrmsOrganizationUnit_OrganizationUnitId];

ALTER TABLE [dbo].[hrmsTerminationAssetRecovery] DROP CONSTRAINT [FK_hrmsTerminationAssetRecovery_hrmsCompanyAsset_CompanyAssetId];

ALTER TABLE [dbo].[hrmsTerminationAssetRecovery] DROP CONSTRAINT [FK_hrmsTerminationAssetRecovery_hrmsEmployeeTermination_TerminationId];

ALTER TABLE [dbo].[hrmsTerminationClearance] DROP CONSTRAINT [FK_hrmsTerminationClearance_hrmsClearanceDepartment_DepartmentId];

ALTER TABLE [dbo].[hrmsTerminationClearance] DROP CONSTRAINT [FK_hrmsTerminationClearance_hrmsEmployeeTermination_TerminationId];

ALTER TABLE [dbo].[hrmsTerminationSettlement] DROP CONSTRAINT [FK_hrmsTerminationSettlement_hrmsEmployeeTermination_TerminationId];

ALTER TABLE [dbo].[hrmsTrainingBudget] DROP CONSTRAINT [FK_hrmsTrainingBudget_hrmsOrganizationUnit_OrganizationUnitId];

ALTER TABLE [dbo].[hrmsTrainingCourse] DROP CONSTRAINT [FK_hrmsTrainingCourse_hrmsTrainingCategory_TrainingCategoryId];

ALTER TABLE [dbo].[hrmsTrainingEnrollment] DROP CONSTRAINT [FK_hrmsTrainingEnrollment_hrmsEmployee_EmployeeId];

ALTER TABLE [dbo].[hrmsTrainingEnrollment] DROP CONSTRAINT [FK_hrmsTrainingEnrollment_hrmsTrainingNeed_TrainingNeedId];

ALTER TABLE [dbo].[hrmsTrainingEnrollment] DROP CONSTRAINT [FK_hrmsTrainingEnrollment_hrmsTrainingSession_TrainingSessionId];

ALTER TABLE [dbo].[hrmsTrainingNeed] DROP CONSTRAINT [FK_hrmsTrainingNeed_hrmsCompetency_CompetencyId];

ALTER TABLE [dbo].[hrmsTrainingNeed] DROP CONSTRAINT [FK_hrmsTrainingNeed_hrmsEmployee_EmployeeId];

ALTER TABLE [dbo].[hrmsTrainingNeed] DROP CONSTRAINT [FK_hrmsTrainingNeed_hrmsTrainingCourse_TrainingCourseId];

ALTER TABLE [dbo].[hrmsTrainingProviderPayment] DROP CONSTRAINT [FK_hrmsTrainingProviderPayment_hrmsTrainingSession_TrainingSessionId];

ALTER TABLE [dbo].[hrmsTrainingSession] DROP CONSTRAINT [FK_hrmsTrainingSession_hrmsTrainingCourse_TrainingCourseId];

ALTER TABLE [dbo].[hrmsTripBudget] DROP CONSTRAINT [FK_hrmsTripBudget_hrmsOrganizationUnit_OrganizationUnitId];

ALTER TABLE [dbo].[hrmsTripExpense] DROP CONSTRAINT [FK_hrmsTripExpense_hrmsTripRequest_TripRequestId];

ALTER TABLE [dbo].[hrmsTripRequest] DROP CONSTRAINT [FK_hrmsTripRequest_hrmsEmployee_EmployeeId];

ALTER TABLE [dbo].[hrmsTripRequest] DROP CONSTRAINT [FK_hrmsTripRequest_hrmsTripBudget_TripBudgetId];

ALTER TABLE [dbo].[hrmsWorkflowActionLog] DROP CONSTRAINT [FK_hrmsWorkflowActionLog_hrmsWorkflowInstance_InstanceId];

ALTER TABLE [dbo].[hrmsWorkflowInstance] DROP CONSTRAINT [FK_hrmsWorkflowInstance_hrmsWorkflowDefinition_DefinitionId];

ALTER TABLE [dbo].[hrmsWorkflowStep] DROP CONSTRAINT [FK_hrmsWorkflowStep_hrmsWorkflowDefinition_DefinitionId];

ALTER TABLE [dbo].[hrmsWorkflowStepApprover] DROP CONSTRAINT [FK_hrmsWorkflowStepApprover_hrmsWorkflowStep_StepId];

ALTER TABLE [dbo].[hrmsWorkforcePlan] DROP CONSTRAINT [FK_hrmsWorkforcePlan_FiscalYear_StartFiscalYearId];

ALTER TABLE [dbo].[hrmsWorkforcePlan] DROP CONSTRAINT [FK_hrmsWorkforcePlan_hrmsOrganizationUnit_OrganizationUnitId];

ALTER TABLE [dbo].[hrmsWorkforcePlanLine] DROP CONSTRAINT [FK_hrmsWorkforcePlanLine_hrmsOrganizationUnit_OrganizationUnitId];

ALTER TABLE [dbo].[hrmsWorkforcePlanLine] DROP CONSTRAINT [FK_hrmsWorkforcePlanLine_hrmsPositionClass_PositionClassId];

ALTER TABLE [dbo].[hrmsWorkforcePlanLine] DROP CONSTRAINT [FK_hrmsWorkforcePlanLine_hrmsWorkforcePlan_PlanId];

ALTER TABLE [dbo].[hrmsWorkLocation] DROP CONSTRAINT [FK_hrmsWorkLocation_hrmsWorkLocation_ParentId];

ALTER TABLE [Core].[RolePermission] DROP CONSTRAINT [FK_RolePermission_coreOperation_OperationId];

ALTER TABLE [Core].[User] DROP CONSTRAINT [FK_User_hrmsEmployee_EmployeeId];

ALTER TABLE [Core].[lupStep] DROP CONSTRAINT [PK_lupStep];

ALTER TABLE [dbo].[hrmsWorkWeekConfiguration] DROP CONSTRAINT [PK_hrmsWorkWeekConfiguration];

ALTER TABLE [dbo].[hrmsWorkLocation] DROP CONSTRAINT [PK_hrmsWorkLocation];

ALTER TABLE [dbo].[hrmsWorkforcePlanLine] DROP CONSTRAINT [PK_hrmsWorkforcePlanLine];

ALTER TABLE [dbo].[hrmsWorkforcePlan] DROP CONSTRAINT [PK_hrmsWorkforcePlan];

ALTER TABLE [dbo].[hrmsWorkflowStepApprover] DROP CONSTRAINT [PK_hrmsWorkflowStepApprover];

ALTER TABLE [dbo].[hrmsWorkflowStep] DROP CONSTRAINT [PK_hrmsWorkflowStep];

ALTER TABLE [dbo].[hrmsWorkflowInstance] DROP CONSTRAINT [PK_hrmsWorkflowInstance];

ALTER TABLE [dbo].[hrmsWorkflowDefinition] DROP CONSTRAINT [PK_hrmsWorkflowDefinition];

ALTER TABLE [dbo].[hrmsWorkflowActionLog] DROP CONSTRAINT [PK_hrmsWorkflowActionLog];

ALTER TABLE [dbo].[hrmsTripRequest] DROP CONSTRAINT [PK_hrmsTripRequest];

ALTER TABLE [dbo].[hrmsTripExpense] DROP CONSTRAINT [PK_hrmsTripExpense];

ALTER TABLE [dbo].[hrmsTripBudget] DROP CONSTRAINT [PK_hrmsTripBudget];

ALTER TABLE [dbo].[hrmsTrainingSession] DROP CONSTRAINT [PK_hrmsTrainingSession];

ALTER TABLE [dbo].[hrmsTrainingProviderPayment] DROP CONSTRAINT [PK_hrmsTrainingProviderPayment];

ALTER TABLE [dbo].[hrmsTrainingNeed] DROP CONSTRAINT [PK_hrmsTrainingNeed];

ALTER TABLE [dbo].[hrmsTrainingEnrollment] DROP CONSTRAINT [PK_hrmsTrainingEnrollment];

ALTER TABLE [dbo].[hrmsTrainingCourse] DROP CONSTRAINT [PK_hrmsTrainingCourse];

ALTER TABLE [dbo].[hrmsTrainingCategory] DROP CONSTRAINT [PK_hrmsTrainingCategory];

ALTER TABLE [dbo].[hrmsTrainingBudget] DROP CONSTRAINT [PK_hrmsTrainingBudget];

ALTER TABLE [dbo].[hrmsTerminationSettlement] DROP CONSTRAINT [PK_hrmsTerminationSettlement];

ALTER TABLE [dbo].[hrmsTerminationClearance] DROP CONSTRAINT [PK_hrmsTerminationClearance];

ALTER TABLE [dbo].[hrmsTerminationAssetRecovery] DROP CONSTRAINT [PK_hrmsTerminationAssetRecovery];

ALTER TABLE [dbo].[hrmsTaxBracket] DROP CONSTRAINT [PK_hrmsTaxBracket];

ALTER TABLE [dbo].[hrmsTalentReview] DROP CONSTRAINT [PK_hrmsTalentReview];

ALTER TABLE [dbo].[hrmsTalentRating] DROP CONSTRAINT [PK_hrmsTalentRating];

ALTER TABLE [dbo].[hrmsTalentAssessment] DROP CONSTRAINT [PK_hrmsTalentAssessment];

ALTER TABLE [dbo].[hrmsSurveyResponse] DROP CONSTRAINT [PK_hrmsSurveyResponse];

ALTER TABLE [dbo].[hrmsSurveyCompletion] DROP CONSTRAINT [PK_hrmsSurveyCompletion];

ALTER TABLE [dbo].[hrmsSurvey] DROP CONSTRAINT [PK_hrmsSurvey];

ALTER TABLE [dbo].[hrmsSuggestion] DROP CONSTRAINT [PK_hrmsSuggestion];

ALTER TABLE [dbo].[hrmsSuccessionPlan] DROP CONSTRAINT [PK_hrmsSuccessionPlan];

ALTER TABLE [dbo].[hrmsSuccessionDevelopmentAction] DROP CONSTRAINT [PK_hrmsSuccessionDevelopmentAction];

ALTER TABLE [dbo].[hrmsSuccessionCandidate] DROP CONSTRAINT [PK_hrmsSuccessionCandidate];

ALTER TABLE [dbo].[hrmsSettlementLine] DROP CONSTRAINT [PK_hrmsSettlementLine];

ALTER TABLE [dbo].[hrmsSalaryRevisionLine] DROP CONSTRAINT [PK_hrmsSalaryRevisionLine];

ALTER TABLE [dbo].[hrmsSalaryRevisionBand] DROP CONSTRAINT [PK_hrmsSalaryRevisionBand];

ALTER TABLE [dbo].[hrmsSalaryRevision] DROP CONSTRAINT [PK_hrmsSalaryRevision];

ALTER TABLE [dbo].[hrmsRewardPointsTransaction] DROP CONSTRAINT [PK_hrmsRewardPointsTransaction];

ALTER TABLE [dbo].[hrmsRewardNomination] DROP CONSTRAINT [PK_hrmsRewardNomination];

ALTER TABLE [dbo].[hrmsRewardDisbursement] DROP CONSTRAINT [PK_hrmsRewardDisbursement];

ALTER TABLE [dbo].[hrmsReviewCycle] DROP CONSTRAINT [PK_hrmsReviewCycle];

ALTER TABLE [dbo].[hrmsRequisitionScreeningCriterion] DROP CONSTRAINT [PK_hrmsRequisitionScreeningCriterion];

ALTER TABLE [dbo].[hrmsReportScheduleRecipient] DROP CONSTRAINT [PK_hrmsReportScheduleRecipient];

ALTER TABLE [dbo].[hrmsReportScheduleFieldValue] DROP CONSTRAINT [PK_hrmsReportScheduleFieldValue];

ALTER TABLE [dbo].[hrmsReportScheduleFieldOutput] DROP CONSTRAINT [PK_hrmsReportScheduleFieldOutput];

ALTER TABLE [dbo].[hrmsReportSchedule] DROP CONSTRAINT [PK_hrmsReportSchedule];

ALTER TABLE [dbo].[hrmsReportSavedFilter] DROP CONSTRAINT [PK_hrmsReportSavedFilter];

ALTER TABLE [dbo].[hrmsReportRunRecipient] DROP CONSTRAINT [PK_hrmsReportRunRecipient];

ALTER TABLE [dbo].[hrmsReportRun] DROP CONSTRAINT [PK_hrmsReportRun];

ALTER TABLE [dbo].[hrmsReportRestriction] DROP CONSTRAINT [PK_hrmsReportRestriction];

ALTER TABLE [dbo].[hrmsReportFieldOutput] DROP CONSTRAINT [PK_hrmsReportFieldOutput];

ALTER TABLE [dbo].[hrmsReportField] DROP CONSTRAINT [PK_hrmsReportField];

ALTER TABLE [dbo].[hrmsReport] DROP CONSTRAINT [PK_hrmsReport];

ALTER TABLE [dbo].[hrmsRecognitionProgram] DROP CONSTRAINT [PK_hrmsRecognitionProgram];

ALTER TABLE [dbo].[hrmsRecognitionBadge] DROP CONSTRAINT [PK_hrmsRecognitionBadge];

ALTER TABLE [dbo].[hrmsRatingScaleLevel] DROP CONSTRAINT [PK_hrmsRatingScaleLevel];

ALTER TABLE [dbo].[hrmsRatingScale] DROP CONSTRAINT [PK_hrmsRatingScale];

ALTER TABLE [dbo].[hrmsProfileChangeRequest] DROP CONSTRAINT [PK_hrmsProfileChangeRequest];

ALTER TABLE [dbo].[hrmsPositionCompetency] DROP CONSTRAINT [PK_hrmsPositionCompetency];

ALTER TABLE [dbo].[hrmsPositionClass] DROP CONSTRAINT [PK_hrmsPositionClass];

ALTER TABLE [dbo].[hrmsPosition] DROP CONSTRAINT [PK_hrmsPosition];

ALTER TABLE [dbo].[hrmsPipObjective] DROP CONSTRAINT [PK_hrmsPipObjective];

ALTER TABLE [dbo].[hrmsPerformanceHistory] DROP CONSTRAINT [PK_hrmsPerformanceHistory];

ALTER TABLE [dbo].[hrmsPerDiemRate] DROP CONSTRAINT [PK_hrmsPerDiemRate];

ALTER TABLE [dbo].[hrmsOtherLeaveSetting] DROP CONSTRAINT [PK_hrmsOtherLeaveSetting];

ALTER TABLE [dbo].[hrmsOtherLeaveDetail] DROP CONSTRAINT [PK_hrmsOtherLeaveDetail];

ALTER TABLE [dbo].[hrmsOtherLeave] DROP CONSTRAINT [PK_hrmsOtherLeave];

ALTER TABLE [dbo].[hrmsOrganizationUnit] DROP CONSTRAINT [PK_hrmsOrganizationUnit];

ALTER TABLE [dbo].[hrmsOrganizationalObjective] DROP CONSTRAINT [PK_hrmsOrganizationalObjective];

ALTER TABLE [dbo].[hrmsOfferLetterTemplate] DROP CONSTRAINT [PK_hrmsOfferLetterTemplate];

ALTER TABLE [dbo].[hrmsNumberSequence] DROP CONSTRAINT [PK_hrmsNumberSequence];

ALTER TABLE [dbo].[hrmsMentorship] DROP CONSTRAINT [PK_hrmsMentorship];

ALTER TABLE [dbo].[hrmsMedicalServiceContract] DROP CONSTRAINT [PK_hrmsMedicalServiceContract];

ALTER TABLE [dbo].[hrmsMedicalProvider] DROP CONSTRAINT [PK_hrmsMedicalProvider];

ALTER TABLE [dbo].[hrmsMedicalPlan] DROP CONSTRAINT [PK_hrmsMedicalPlan];

ALTER TABLE [dbo].[hrmsMedicalEnrollment] DROP CONSTRAINT [PK_hrmsMedicalEnrollment];

ALTER TABLE [dbo].[hrmsMedicalClaimAttachment] DROP CONSTRAINT [PK_hrmsMedicalClaimAttachment];

ALTER TABLE [dbo].[hrmsMedicalClaim] DROP CONSTRAINT [PK_hrmsMedicalClaim];

ALTER TABLE [dbo].[hrmsMedicalBeneficiary] DROP CONSTRAINT [PK_hrmsMedicalBeneficiary];

ALTER TABLE [dbo].[hrmsLoanType] DROP CONSTRAINT [PK_hrmsLoanType];

ALTER TABLE [dbo].[hrmsLoanRepaymentSchedule] DROP CONSTRAINT [PK_hrmsLoanRepaymentSchedule];

ALTER TABLE [dbo].[hrmsLoanGuarantor] DROP CONSTRAINT [PK_hrmsLoanGuarantor];

ALTER TABLE [dbo].[hrmsLoan] DROP CONSTRAINT [PK_hrmsLoan];

ALTER TABLE [dbo].[hrmsLeaveType] DROP CONSTRAINT [PK_hrmsLeaveType];

ALTER TABLE [dbo].[hrmsLeaveRequestLine] DROP CONSTRAINT [PK_hrmsLeaveRequestLine];

ALTER TABLE [dbo].[hrmsLeaveRequest] DROP CONSTRAINT [PK_hrmsLeaveRequest];

ALTER TABLE [dbo].[hrmsLeaveBalanceTransaction] DROP CONSTRAINT [PK_hrmsLeaveBalanceTransaction];

ALTER TABLE [dbo].[hrmsLeaveBalance] DROP CONSTRAINT [PK_hrmsLeaveBalance];

ALTER TABLE [dbo].[hrmsLearningPathStep] DROP CONSTRAINT [PK_hrmsLearningPathStep];

ALTER TABLE [dbo].[hrmsLearningPath] DROP CONSTRAINT [PK_hrmsLearningPath];

ALTER TABLE [dbo].[hrmsLearningCommunityPost] DROP CONSTRAINT [PK_hrmsLearningCommunityPost];

ALTER TABLE [dbo].[hrmsLearningCommunityMember] DROP CONSTRAINT [PK_hrmsLearningCommunityMember];

ALTER TABLE [dbo].[hrmsLearningCommunity] DROP CONSTRAINT [PK_hrmsLearningCommunity];

ALTER TABLE [dbo].[hrmsKnowledgeTransfer] DROP CONSTRAINT [PK_hrmsKnowledgeTransfer];

ALTER TABLE [dbo].[hrmsJobRequisition] DROP CONSTRAINT [PK_hrmsJobRequisition];

ALTER TABLE [dbo].[hrmsJobOffer] DROP CONSTRAINT [PK_hrmsJobOffer];

ALTER TABLE [dbo].[hrmsJobGrade] DROP CONSTRAINT [PK_hrmsJobGrade];

ALTER TABLE [dbo].[hrmsJobCategory] DROP CONSTRAINT [PK_hrmsJobCategory];

ALTER TABLE [dbo].[hrmsJobApplicationStageLog] DROP CONSTRAINT [PK_hrmsJobApplicationStageLog];

ALTER TABLE [dbo].[hrmsJobApplication] DROP CONSTRAINT [PK_hrmsJobApplication];

ALTER TABLE [dbo].[hrmsInterviewPanelist] DROP CONSTRAINT [PK_hrmsInterviewPanelist];

ALTER TABLE [dbo].[hrmsInterviewFeedback] DROP CONSTRAINT [PK_hrmsInterviewFeedback];

ALTER TABLE [dbo].[hrmsInterview] DROP CONSTRAINT [PK_hrmsInterview];

ALTER TABLE [dbo].[hrmsInsurancePremiumSchedule] DROP CONSTRAINT [PK_hrmsInsurancePremiumSchedule];

ALTER TABLE [dbo].[hrmsInsurancePolicy] DROP CONSTRAINT [PK_hrmsInsurancePolicy];

ALTER TABLE [dbo].[hrmsInsuranceClaimAttachment] DROP CONSTRAINT [PK_hrmsInsuranceClaimAttachment];

ALTER TABLE [dbo].[hrmsInsuranceClaim] DROP CONSTRAINT [PK_hrmsInsuranceClaim];

ALTER TABLE [dbo].[hrmsImprovementPlan] DROP CONSTRAINT [PK_hrmsImprovementPlan];

ALTER TABLE [dbo].[hrmsHoliday] DROP CONSTRAINT [PK_hrmsHoliday];

ALTER TABLE [dbo].[hrmsHiringRequest] DROP CONSTRAINT [PK_hrmsHiringRequest];

ALTER TABLE [dbo].[hrmsGrievanceNote] DROP CONSTRAINT [PK_hrmsGrievanceNote];

ALTER TABLE [dbo].[hrmsGrievance] DROP CONSTRAINT [PK_hrmsGrievance];

ALTER TABLE [dbo].[hrmsGoalActionItem] DROP CONSTRAINT [PK_hrmsGoalActionItem];

ALTER TABLE [dbo].[hrmsExitQuestionnaire] DROP CONSTRAINT [PK_hrmsExitQuestionnaire];

ALTER TABLE [dbo].[hrmsExitInterview] DROP CONSTRAINT [PK_hrmsExitInterview];

ALTER TABLE [dbo].[hrmsEmployeeTrainingCertificate] DROP CONSTRAINT [PK_hrmsEmployeeTrainingCertificate];

ALTER TABLE [dbo].[hrmsEmployeeTermination] DROP CONSTRAINT [PK_hrmsEmployeeTermination];

ALTER TABLE [dbo].[hrmsEmployeeRecognition] DROP CONSTRAINT [PK_hrmsEmployeeRecognition];

ALTER TABLE [dbo].[hrmsEmployeeMovement] DROP CONSTRAINT [PK_hrmsEmployeeMovement];

ALTER TABLE [dbo].[hrmsEmployeeGuarantee] DROP CONSTRAINT [PK_hrmsEmployeeGuarantee];

ALTER TABLE [dbo].[hrmsEmployeeGoal] DROP CONSTRAINT [PK_hrmsEmployeeGoal];

ALTER TABLE [dbo].[hrmsEmployeeFieldValue] DROP CONSTRAINT [PK_hrmsEmployeeFieldValue];

ALTER TABLE [dbo].[hrmsEmployeeFieldDefinition] DROP CONSTRAINT [PK_hrmsEmployeeFieldDefinition];

ALTER TABLE [dbo].[hrmsEmployeeExperience] DROP CONSTRAINT [PK_hrmsEmployeeExperience];

ALTER TABLE [dbo].[hrmsEmployeeEducation] DROP CONSTRAINT [PK_hrmsEmployeeEducation];

ALTER TABLE [dbo].[hrmsEmployeeDocument] DROP CONSTRAINT [PK_hrmsEmployeeDocument];

ALTER TABLE [dbo].[hrmsEmployeeDependent] DROP CONSTRAINT [PK_hrmsEmployeeDependent];

ALTER TABLE [dbo].[hrmsEmployeeCareerPathStepProgress] DROP CONSTRAINT [PK_hrmsEmployeeCareerPathStepProgress];

ALTER TABLE [dbo].[hrmsEmployeeCareerPath] DROP CONSTRAINT [PK_hrmsEmployeeCareerPath];

ALTER TABLE [dbo].[hrmsEmployeeBenefitEnrollment] DROP CONSTRAINT [PK_hrmsEmployeeBenefitEnrollment];

ALTER TABLE [dbo].[hrmsEmployeeAllowance] DROP CONSTRAINT [PK_hrmsEmployeeAllowance];

ALTER TABLE [dbo].[hrmsEmployee] DROP CONSTRAINT [PK_hrmsEmployee];

ALTER TABLE [dbo].[hrmsDynamicFormRecord] DROP CONSTRAINT [PK_hrmsDynamicFormRecord];

ALTER TABLE [dbo].[hrmsDynamicFormField] DROP CONSTRAINT [PK_hrmsDynamicFormField];

ALTER TABLE [dbo].[hrmsDynamicForm] DROP CONSTRAINT [PK_hrmsDynamicForm];

ALTER TABLE [dbo].[hrmsDocumentTemplate] DROP CONSTRAINT [PK_hrmsDocumentTemplate];

ALTER TABLE [dbo].[hrmsDisciplinaryMeasure] DROP CONSTRAINT [PK_hrmsDisciplinaryMeasure];

ALTER TABLE [dbo].[hrmsDevelopmentPlan] DROP CONSTRAINT [PK_hrmsDevelopmentPlan];

ALTER TABLE [dbo].[hrmsDevelopmentAction] DROP CONSTRAINT [PK_hrmsDevelopmentAction];

ALTER TABLE [dbo].[hrmsCriticalPosition] DROP CONSTRAINT [PK_hrmsCriticalPosition];

ALTER TABLE [dbo].[hrmsCriterionEvaluator] DROP CONSTRAINT [PK_hrmsCriterionEvaluator];

ALTER TABLE [dbo].[hrmsCompetencyCategory] DROP CONSTRAINT [PK_hrmsCompetencyCategory];

ALTER TABLE [dbo].[hrmsCompetency] DROP CONSTRAINT [PK_hrmsCompetency];

ALTER TABLE [dbo].[hrmsCompensationRequest] DROP CONSTRAINT [PK_hrmsCompensationRequest];

ALTER TABLE [dbo].[hrmsCompanyProfile] DROP CONSTRAINT [PK_hrmsCompanyProfile];

ALTER TABLE [dbo].[hrmsCompanyAsset] DROP CONSTRAINT [PK_hrmsCompanyAsset];

ALTER TABLE [dbo].[hrmsCommunityPostReaction] DROP CONSTRAINT [PK_hrmsCommunityPostReaction];

ALTER TABLE [dbo].[hrmsClearanceDepartmentApprover] DROP CONSTRAINT [PK_hrmsClearanceDepartmentApprover];

ALTER TABLE [dbo].[hrmsClearanceDepartment] DROP CONSTRAINT [PK_hrmsClearanceDepartment];

ALTER TABLE [dbo].[hrmsCareerPathStepCompetency] DROP CONSTRAINT [PK_hrmsCareerPathStepCompetency];

ALTER TABLE [dbo].[hrmsCareerPathStep] DROP CONSTRAINT [PK_hrmsCareerPathStep];

ALTER TABLE [dbo].[hrmsCareerPathChangeRequest] DROP CONSTRAINT [PK_hrmsCareerPathChangeRequest];

ALTER TABLE [dbo].[hrmsCareerPath] DROP CONSTRAINT [PK_hrmsCareerPath];

ALTER TABLE [dbo].[hrmsCandidateDocument] DROP CONSTRAINT [PK_hrmsCandidateDocument];

ALTER TABLE [dbo].[hrmsCandidate] DROP CONSTRAINT [PK_hrmsCandidate];

ALTER TABLE [dbo].[hrmsCalibrationSession] DROP CONSTRAINT [PK_hrmsCalibrationSession];

ALTER TABLE [dbo].[hrmsCalibrationItem] DROP CONSTRAINT [PK_hrmsCalibrationItem];

ALTER TABLE [dbo].[hrmsBranch] DROP CONSTRAINT [PK_hrmsBranch];

ALTER TABLE [dbo].[hrmsBenefitPlan] DROP CONSTRAINT [PK_hrmsBenefitPlan];

ALTER TABLE [dbo].[hrmsAwardCategory] DROP CONSTRAINT [PK_hrmsAwardCategory];

ALTER TABLE [dbo].[hrmsAuditLog] DROP CONSTRAINT [PK_hrmsAuditLog];

ALTER TABLE [dbo].[hrmsAppraisalTemplate] DROP CONSTRAINT [PK_hrmsAppraisalTemplate];

ALTER TABLE [dbo].[hrmsAppraisalPeerReview] DROP CONSTRAINT [PK_hrmsAppraisalPeerReview];

ALTER TABLE [dbo].[hrmsAppraisalGoal] DROP CONSTRAINT [PK_hrmsAppraisalGoal];

ALTER TABLE [dbo].[hrmsAppraisalCompetency] DROP CONSTRAINT [PK_hrmsAppraisalCompetency];

ALTER TABLE [dbo].[hrmsAppraisalAppeal] DROP CONSTRAINT [PK_hrmsAppraisalAppeal];

ALTER TABLE [dbo].[hrmsAppraisal] DROP CONSTRAINT [PK_hrmsAppraisal];

ALTER TABLE [dbo].[hrmsApplicationCriterionScore] DROP CONSTRAINT [PK_hrmsApplicationCriterionScore];

ALTER TABLE [dbo].[hrmsAnnualLeaveSetting] DROP CONSTRAINT [PK_hrmsAnnualLeaveSetting];

ALTER TABLE [dbo].[hrmsAnnualLeaveHeader] DROP CONSTRAINT [PK_hrmsAnnualLeaveHeader];

ALTER TABLE [dbo].[hrmsAnnualLeaveDetail] DROP CONSTRAINT [PK_hrmsAnnualLeaveDetail];

ALTER TABLE [dbo].[hrmsAnnouncement] DROP CONSTRAINT [PK_hrmsAnnouncement];

ALTER TABLE [dbo].[hrmsAllowanceType] DROP CONSTRAINT [PK_hrmsAllowanceType];

ALTER TABLE [dbo].[hrmsAchievement] DROP CONSTRAINT [PK_hrmsAchievement];

ALTER TABLE [dbo].[coreSubsystem] DROP CONSTRAINT [PK_coreSubsystem];

ALTER TABLE [Core].[coreSalaryScale] DROP CONSTRAINT [PK_coreSalaryScale];

ALTER TABLE [Core].[CorePerson] DROP CONSTRAINT [PK_CorePerson];

ALTER TABLE [dbo].[coreOperation] DROP CONSTRAINT [PK_coreOperation];

ALTER TABLE [dbo].[coreModule] DROP CONSTRAINT [PK_coreModule];

IF SCHEMA_ID(N'Hrms') IS NULL EXEC(N'CREATE SCHEMA [Hrms];');

EXEC sp_rename N'[Core].[lupStep]', N'Step', 'OBJECT';

EXEC sp_rename N'[dbo].[hrmsWorkWeekConfiguration]', N'WorkWeekConfiguration', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[WorkWeekConfiguration];

EXEC sp_rename N'[dbo].[hrmsWorkLocation]', N'WorkLocation', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[WorkLocation];

EXEC sp_rename N'[dbo].[hrmsWorkforcePlanLine]', N'WorkforcePlanLine', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[WorkforcePlanLine];

EXEC sp_rename N'[dbo].[hrmsWorkforcePlan]', N'WorkforcePlan', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[WorkforcePlan];

EXEC sp_rename N'[dbo].[hrmsWorkflowStepApprover]', N'WorkflowStepApprover', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[WorkflowStepApprover];

EXEC sp_rename N'[dbo].[hrmsWorkflowStep]', N'WorkflowStep', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[WorkflowStep];

EXEC sp_rename N'[dbo].[hrmsWorkflowInstance]', N'WorkflowInstance', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[WorkflowInstance];

EXEC sp_rename N'[dbo].[hrmsWorkflowDefinition]', N'WorkflowDefinition', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[WorkflowDefinition];

EXEC sp_rename N'[dbo].[hrmsWorkflowActionLog]', N'WorkflowActionLog', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[WorkflowActionLog];

EXEC sp_rename N'[dbo].[hrmsTripRequest]', N'TripRequest', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[TripRequest];

EXEC sp_rename N'[dbo].[hrmsTripExpense]', N'TripExpense', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[TripExpense];

EXEC sp_rename N'[dbo].[hrmsTripBudget]', N'TripBudget', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[TripBudget];

EXEC sp_rename N'[dbo].[hrmsTrainingSession]', N'TrainingSession', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[TrainingSession];

EXEC sp_rename N'[dbo].[hrmsTrainingProviderPayment]', N'TrainingProviderPayment', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[TrainingProviderPayment];

EXEC sp_rename N'[dbo].[hrmsTrainingNeed]', N'TrainingNeed', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[TrainingNeed];

EXEC sp_rename N'[dbo].[hrmsTrainingEnrollment]', N'TrainingEnrollment', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[TrainingEnrollment];

EXEC sp_rename N'[dbo].[hrmsTrainingCourse]', N'TrainingCourse', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[TrainingCourse];

EXEC sp_rename N'[dbo].[hrmsTrainingCategory]', N'TrainingCategory', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[TrainingCategory];

EXEC sp_rename N'[dbo].[hrmsTrainingBudget]', N'TrainingBudget', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[TrainingBudget];

EXEC sp_rename N'[dbo].[hrmsTerminationSettlement]', N'TerminationSettlement', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[TerminationSettlement];

EXEC sp_rename N'[dbo].[hrmsTerminationClearance]', N'TerminationClearance', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[TerminationClearance];

EXEC sp_rename N'[dbo].[hrmsTerminationAssetRecovery]', N'TerminationAssetRecovery', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[TerminationAssetRecovery];

EXEC sp_rename N'[dbo].[hrmsTaxBracket]', N'TaxBracket', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[TaxBracket];

EXEC sp_rename N'[dbo].[hrmsTalentReview]', N'TalentReview', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[TalentReview];

EXEC sp_rename N'[dbo].[hrmsTalentRating]', N'TalentRating', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[TalentRating];

EXEC sp_rename N'[dbo].[hrmsTalentAssessment]', N'TalentAssessment', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[TalentAssessment];

EXEC sp_rename N'[dbo].[hrmsSurveyResponse]', N'SurveyResponse', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[SurveyResponse];

EXEC sp_rename N'[dbo].[hrmsSurveyCompletion]', N'SurveyCompletion', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[SurveyCompletion];

EXEC sp_rename N'[dbo].[hrmsSurvey]', N'Survey', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[Survey];

EXEC sp_rename N'[dbo].[hrmsSuggestion]', N'Suggestion', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[Suggestion];

EXEC sp_rename N'[dbo].[hrmsSuccessionPlan]', N'SuccessionPlan', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[SuccessionPlan];

EXEC sp_rename N'[dbo].[hrmsSuccessionDevelopmentAction]', N'SuccessionDevelopmentAction', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[SuccessionDevelopmentAction];

EXEC sp_rename N'[dbo].[hrmsSuccessionCandidate]', N'SuccessionCandidate', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[SuccessionCandidate];

EXEC sp_rename N'[dbo].[hrmsSettlementLine]', N'SettlementLine', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[SettlementLine];

EXEC sp_rename N'[dbo].[hrmsSalaryRevisionLine]', N'SalaryRevisionLine', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[SalaryRevisionLine];

EXEC sp_rename N'[dbo].[hrmsSalaryRevisionBand]', N'SalaryRevisionBand', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[SalaryRevisionBand];

EXEC sp_rename N'[dbo].[hrmsSalaryRevision]', N'SalaryRevision', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[SalaryRevision];

EXEC sp_rename N'[dbo].[hrmsRewardPointsTransaction]', N'RewardPointsTransaction', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[RewardPointsTransaction];

EXEC sp_rename N'[dbo].[hrmsRewardNomination]', N'RewardNomination', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[RewardNomination];

EXEC sp_rename N'[dbo].[hrmsRewardDisbursement]', N'RewardDisbursement', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[RewardDisbursement];

EXEC sp_rename N'[dbo].[hrmsReviewCycle]', N'ReviewCycle', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[ReviewCycle];

EXEC sp_rename N'[dbo].[hrmsRequisitionScreeningCriterion]', N'RequisitionScreeningCriterion', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[RequisitionScreeningCriterion];

EXEC sp_rename N'[dbo].[hrmsReportScheduleRecipient]', N'ReportScheduleRecipient', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[ReportScheduleRecipient];

EXEC sp_rename N'[dbo].[hrmsReportScheduleFieldValue]', N'ReportScheduleFieldValue', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[ReportScheduleFieldValue];

EXEC sp_rename N'[dbo].[hrmsReportScheduleFieldOutput]', N'ReportScheduleFieldOutput', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[ReportScheduleFieldOutput];

EXEC sp_rename N'[dbo].[hrmsReportSchedule]', N'ReportSchedule', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[ReportSchedule];

EXEC sp_rename N'[dbo].[hrmsReportSavedFilter]', N'ReportSavedFilter', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[ReportSavedFilter];

EXEC sp_rename N'[dbo].[hrmsReportRunRecipient]', N'ReportRunRecipient', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[ReportRunRecipient];

EXEC sp_rename N'[dbo].[hrmsReportRun]', N'ReportRun', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[ReportRun];

EXEC sp_rename N'[dbo].[hrmsReportRestriction]', N'ReportRestriction', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[ReportRestriction];

EXEC sp_rename N'[dbo].[hrmsReportFieldOutput]', N'ReportFieldOutput', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[ReportFieldOutput];

EXEC sp_rename N'[dbo].[hrmsReportField]', N'ReportField', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[ReportField];

EXEC sp_rename N'[dbo].[hrmsReport]', N'Report', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[Report];

EXEC sp_rename N'[dbo].[hrmsRecognitionProgram]', N'RecognitionProgram', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[RecognitionProgram];

EXEC sp_rename N'[dbo].[hrmsRecognitionBadge]', N'RecognitionBadge', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[RecognitionBadge];

EXEC sp_rename N'[dbo].[hrmsRatingScaleLevel]', N'RatingScaleLevel', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[RatingScaleLevel];

EXEC sp_rename N'[dbo].[hrmsRatingScale]', N'RatingScale', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[RatingScale];

EXEC sp_rename N'[dbo].[hrmsProfileChangeRequest]', N'ProfileChangeRequest', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[ProfileChangeRequest];

EXEC sp_rename N'[dbo].[hrmsPositionCompetency]', N'PositionCompetency', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[PositionCompetency];

EXEC sp_rename N'[dbo].[hrmsPositionClass]', N'PositionClass', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[PositionClass];

EXEC sp_rename N'[dbo].[hrmsPosition]', N'Position', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[Position];

EXEC sp_rename N'[dbo].[hrmsPipObjective]', N'PipObjective', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[PipObjective];

EXEC sp_rename N'[dbo].[hrmsPerformanceHistory]', N'PerformanceHistory', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[PerformanceHistory];

EXEC sp_rename N'[dbo].[hrmsPerDiemRate]', N'PerDiemRate', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[PerDiemRate];

EXEC sp_rename N'[dbo].[hrmsOtherLeaveSetting]', N'OtherLeaveSetting', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[OtherLeaveSetting];

EXEC sp_rename N'[dbo].[hrmsOtherLeaveDetail]', N'OtherLeaveDetail', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[OtherLeaveDetail];

EXEC sp_rename N'[dbo].[hrmsOtherLeave]', N'OtherLeave', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[OtherLeave];

EXEC sp_rename N'[dbo].[hrmsOrganizationUnit]', N'OrganizationUnit', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[OrganizationUnit];

EXEC sp_rename N'[dbo].[hrmsOrganizationalObjective]', N'OrganizationalObjective', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[OrganizationalObjective];

EXEC sp_rename N'[dbo].[hrmsOfferLetterTemplate]', N'OfferLetterTemplate', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[OfferLetterTemplate];

EXEC sp_rename N'[dbo].[hrmsNumberSequence]', N'NumberSequence', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[NumberSequence];

EXEC sp_rename N'[dbo].[hrmsMentorship]', N'Mentorship', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[Mentorship];

EXEC sp_rename N'[dbo].[hrmsMedicalServiceContract]', N'MedicalServiceContract', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[MedicalServiceContract];

EXEC sp_rename N'[dbo].[hrmsMedicalProvider]', N'MedicalProvider', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[MedicalProvider];

EXEC sp_rename N'[dbo].[hrmsMedicalPlan]', N'MedicalPlan', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[MedicalPlan];

EXEC sp_rename N'[dbo].[hrmsMedicalEnrollment]', N'MedicalEnrollment', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[MedicalEnrollment];

EXEC sp_rename N'[dbo].[hrmsMedicalClaimAttachment]', N'MedicalClaimAttachment', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[MedicalClaimAttachment];

EXEC sp_rename N'[dbo].[hrmsMedicalClaim]', N'MedicalClaim', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[MedicalClaim];

EXEC sp_rename N'[dbo].[hrmsMedicalBeneficiary]', N'MedicalBeneficiary', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[MedicalBeneficiary];

EXEC sp_rename N'[dbo].[hrmsLoanType]', N'LoanType', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[LoanType];

EXEC sp_rename N'[dbo].[hrmsLoanRepaymentSchedule]', N'LoanRepaymentSchedule', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[LoanRepaymentSchedule];

EXEC sp_rename N'[dbo].[hrmsLoanGuarantor]', N'LoanGuarantor', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[LoanGuarantor];

EXEC sp_rename N'[dbo].[hrmsLoan]', N'Loan', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[Loan];

EXEC sp_rename N'[dbo].[hrmsLeaveType]', N'LeaveType', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[LeaveType];

EXEC sp_rename N'[dbo].[hrmsLeaveRequestLine]', N'LeaveRequestLine', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[LeaveRequestLine];

EXEC sp_rename N'[dbo].[hrmsLeaveRequest]', N'LeaveRequest', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[LeaveRequest];

EXEC sp_rename N'[dbo].[hrmsLeaveBalanceTransaction]', N'LeaveBalanceTransaction', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[LeaveBalanceTransaction];

EXEC sp_rename N'[dbo].[hrmsLeaveBalance]', N'LeaveBalance', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[LeaveBalance];

EXEC sp_rename N'[dbo].[hrmsLearningPathStep]', N'LearningPathStep', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[LearningPathStep];

EXEC sp_rename N'[dbo].[hrmsLearningPath]', N'LearningPath', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[LearningPath];

EXEC sp_rename N'[dbo].[hrmsLearningCommunityPost]', N'LearningCommunityPost', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[LearningCommunityPost];

EXEC sp_rename N'[dbo].[hrmsLearningCommunityMember]', N'LearningCommunityMember', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[LearningCommunityMember];

EXEC sp_rename N'[dbo].[hrmsLearningCommunity]', N'LearningCommunity', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[LearningCommunity];

EXEC sp_rename N'[dbo].[hrmsKnowledgeTransfer]', N'KnowledgeTransfer', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[KnowledgeTransfer];

EXEC sp_rename N'[dbo].[hrmsJobRequisition]', N'JobRequisition', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[JobRequisition];

EXEC sp_rename N'[dbo].[hrmsJobOffer]', N'JobOffer', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[JobOffer];

EXEC sp_rename N'[dbo].[hrmsJobGrade]', N'JobGrade', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[JobGrade];

EXEC sp_rename N'[dbo].[hrmsJobCategory]', N'JobCategory', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[JobCategory];

EXEC sp_rename N'[dbo].[hrmsJobApplicationStageLog]', N'JobApplicationStageLog', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[JobApplicationStageLog];

EXEC sp_rename N'[dbo].[hrmsJobApplication]', N'JobApplication', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[JobApplication];

EXEC sp_rename N'[dbo].[hrmsInterviewPanelist]', N'InterviewPanelist', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[InterviewPanelist];

EXEC sp_rename N'[dbo].[hrmsInterviewFeedback]', N'InterviewFeedback', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[InterviewFeedback];

EXEC sp_rename N'[dbo].[hrmsInterview]', N'Interview', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[Interview];

EXEC sp_rename N'[dbo].[hrmsInsurancePremiumSchedule]', N'InsurancePremiumSchedule', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[InsurancePremiumSchedule];

EXEC sp_rename N'[dbo].[hrmsInsurancePolicy]', N'InsurancePolicy', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[InsurancePolicy];

EXEC sp_rename N'[dbo].[hrmsInsuranceClaimAttachment]', N'InsuranceClaimAttachment', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[InsuranceClaimAttachment];

EXEC sp_rename N'[dbo].[hrmsInsuranceClaim]', N'InsuranceClaim', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[InsuranceClaim];

EXEC sp_rename N'[dbo].[hrmsImprovementPlan]', N'ImprovementPlan', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[ImprovementPlan];

EXEC sp_rename N'[dbo].[hrmsHoliday]', N'Holiday', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[Holiday];

EXEC sp_rename N'[dbo].[hrmsHiringRequest]', N'HiringRequest', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[HiringRequest];

EXEC sp_rename N'[dbo].[hrmsGrievanceNote]', N'GrievanceNote', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[GrievanceNote];

EXEC sp_rename N'[dbo].[hrmsGrievance]', N'Grievance', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[Grievance];

EXEC sp_rename N'[dbo].[hrmsGoalActionItem]', N'GoalActionItem', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[GoalActionItem];

EXEC sp_rename N'[dbo].[hrmsExitQuestionnaire]', N'ExitQuestionnaire', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[ExitQuestionnaire];

EXEC sp_rename N'[dbo].[hrmsExitInterview]', N'ExitInterview', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[ExitInterview];

EXEC sp_rename N'[dbo].[hrmsEmployeeTrainingCertificate]', N'EmployeeTrainingCertificate', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[EmployeeTrainingCertificate];

EXEC sp_rename N'[dbo].[hrmsEmployeeTermination]', N'EmployeeTermination', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[EmployeeTermination];

EXEC sp_rename N'[dbo].[hrmsEmployeeRecognition]', N'EmployeeRecognition', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[EmployeeRecognition];

EXEC sp_rename N'[dbo].[hrmsEmployeeMovement]', N'EmployeeMovement', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[EmployeeMovement];

EXEC sp_rename N'[dbo].[hrmsEmployeeGuarantee]', N'EmployeeGuarantee', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[EmployeeGuarantee];

EXEC sp_rename N'[dbo].[hrmsEmployeeGoal]', N'EmployeeGoal', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[EmployeeGoal];

EXEC sp_rename N'[dbo].[hrmsEmployeeFieldValue]', N'EmployeeFieldValue', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[EmployeeFieldValue];

EXEC sp_rename N'[dbo].[hrmsEmployeeFieldDefinition]', N'EmployeeFieldDefinition', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[EmployeeFieldDefinition];

EXEC sp_rename N'[dbo].[hrmsEmployeeExperience]', N'EmployeeExperience', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[EmployeeExperience];

EXEC sp_rename N'[dbo].[hrmsEmployeeEducation]', N'EmployeeEducation', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[EmployeeEducation];

EXEC sp_rename N'[dbo].[hrmsEmployeeDocument]', N'EmployeeDocument', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[EmployeeDocument];

EXEC sp_rename N'[dbo].[hrmsEmployeeDependent]', N'EmployeeDependent', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[EmployeeDependent];

EXEC sp_rename N'[dbo].[hrmsEmployeeCareerPathStepProgress]', N'EmployeeCareerPathStepProgress', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[EmployeeCareerPathStepProgress];

EXEC sp_rename N'[dbo].[hrmsEmployeeCareerPath]', N'EmployeeCareerPath', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[EmployeeCareerPath];

EXEC sp_rename N'[dbo].[hrmsEmployeeBenefitEnrollment]', N'EmployeeBenefitEnrollment', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[EmployeeBenefitEnrollment];

EXEC sp_rename N'[dbo].[hrmsEmployeeAllowance]', N'EmployeeAllowance', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[EmployeeAllowance];

EXEC sp_rename N'[dbo].[hrmsEmployee]', N'Employee', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[Employee];

EXEC sp_rename N'[dbo].[hrmsDynamicFormRecord]', N'DynamicFormRecord', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[DynamicFormRecord];

EXEC sp_rename N'[dbo].[hrmsDynamicFormField]', N'DynamicFormField', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[DynamicFormField];

EXEC sp_rename N'[dbo].[hrmsDynamicForm]', N'DynamicForm', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[DynamicForm];

EXEC sp_rename N'[dbo].[hrmsDocumentTemplate]', N'DocumentTemplate', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[DocumentTemplate];

EXEC sp_rename N'[dbo].[hrmsDisciplinaryMeasure]', N'DisciplinaryMeasure', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[DisciplinaryMeasure];

EXEC sp_rename N'[dbo].[hrmsDevelopmentPlan]', N'DevelopmentPlan', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[DevelopmentPlan];

EXEC sp_rename N'[dbo].[hrmsDevelopmentAction]', N'DevelopmentAction', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[DevelopmentAction];

EXEC sp_rename N'[dbo].[hrmsCriticalPosition]', N'CriticalPosition', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[CriticalPosition];

EXEC sp_rename N'[dbo].[hrmsCriterionEvaluator]', N'CriterionEvaluator', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[CriterionEvaluator];

EXEC sp_rename N'[dbo].[hrmsCompetencyCategory]', N'CompetencyCategory', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[CompetencyCategory];

EXEC sp_rename N'[dbo].[hrmsCompetency]', N'Competency', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[Competency];

EXEC sp_rename N'[dbo].[hrmsCompensationRequest]', N'CompensationRequest', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[CompensationRequest];

EXEC sp_rename N'[dbo].[hrmsCompanyProfile]', N'CompanyProfile', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[CompanyProfile];

EXEC sp_rename N'[dbo].[hrmsCompanyAsset]', N'CompanyAsset', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[CompanyAsset];

EXEC sp_rename N'[dbo].[hrmsCommunityPostReaction]', N'CommunityPostReaction', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[CommunityPostReaction];

EXEC sp_rename N'[dbo].[hrmsClearanceDepartmentApprover]', N'ClearanceDepartmentApprover', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[ClearanceDepartmentApprover];

EXEC sp_rename N'[dbo].[hrmsClearanceDepartment]', N'ClearanceDepartment', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[ClearanceDepartment];

EXEC sp_rename N'[dbo].[hrmsCareerPathStepCompetency]', N'CareerPathStepCompetency', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[CareerPathStepCompetency];

EXEC sp_rename N'[dbo].[hrmsCareerPathStep]', N'CareerPathStep', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[CareerPathStep];

EXEC sp_rename N'[dbo].[hrmsCareerPathChangeRequest]', N'CareerPathChangeRequest', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[CareerPathChangeRequest];

EXEC sp_rename N'[dbo].[hrmsCareerPath]', N'CareerPath', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[CareerPath];

EXEC sp_rename N'[dbo].[hrmsCandidateDocument]', N'CandidateDocument', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[CandidateDocument];

EXEC sp_rename N'[dbo].[hrmsCandidate]', N'Candidate', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[Candidate];

EXEC sp_rename N'[dbo].[hrmsCalibrationSession]', N'CalibrationSession', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[CalibrationSession];

EXEC sp_rename N'[dbo].[hrmsCalibrationItem]', N'CalibrationItem', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[CalibrationItem];

EXEC sp_rename N'[dbo].[hrmsBranch]', N'Branch', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[Branch];

EXEC sp_rename N'[dbo].[hrmsBenefitPlan]', N'BenefitPlan', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[BenefitPlan];

EXEC sp_rename N'[dbo].[hrmsAwardCategory]', N'AwardCategory', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[AwardCategory];

EXEC sp_rename N'[dbo].[hrmsAuditLog]', N'AuditLog', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[AuditLog];

EXEC sp_rename N'[dbo].[hrmsAppraisalTemplate]', N'AppraisalTemplate', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[AppraisalTemplate];

EXEC sp_rename N'[dbo].[hrmsAppraisalPeerReview]', N'AppraisalPeerReview', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[AppraisalPeerReview];

EXEC sp_rename N'[dbo].[hrmsAppraisalGoal]', N'AppraisalGoal', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[AppraisalGoal];

EXEC sp_rename N'[dbo].[hrmsAppraisalCompetency]', N'AppraisalCompetency', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[AppraisalCompetency];

EXEC sp_rename N'[dbo].[hrmsAppraisalAppeal]', N'AppraisalAppeal', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[AppraisalAppeal];

EXEC sp_rename N'[dbo].[hrmsAppraisal]', N'Appraisal', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[Appraisal];

EXEC sp_rename N'[dbo].[hrmsApplicationCriterionScore]', N'ApplicationCriterionScore', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[ApplicationCriterionScore];

EXEC sp_rename N'[dbo].[hrmsAnnualLeaveSetting]', N'AnnualLeaveSetting', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[AnnualLeaveSetting];

EXEC sp_rename N'[dbo].[hrmsAnnualLeaveHeader]', N'AnnualLeaveHeader', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[AnnualLeaveHeader];

EXEC sp_rename N'[dbo].[hrmsAnnualLeaveDetail]', N'AnnualLeaveDetail', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[AnnualLeaveDetail];

EXEC sp_rename N'[dbo].[hrmsAnnouncement]', N'Announcement', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[Announcement];

EXEC sp_rename N'[dbo].[hrmsAllowanceType]', N'AllowanceType', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[AllowanceType];

EXEC sp_rename N'[dbo].[hrmsAchievement]', N'Achievement', 'OBJECT';
ALTER SCHEMA [Hrms] TRANSFER [dbo].[Achievement];

EXEC sp_rename N'[dbo].[coreSubsystem]', N'Subsystem', 'OBJECT';
ALTER SCHEMA [Core] TRANSFER [dbo].[Subsystem];

EXEC sp_rename N'[Core].[coreSalaryScale]', N'SalaryScale', 'OBJECT';

EXEC sp_rename N'[Core].[CorePerson]', N'Person', 'OBJECT';

EXEC sp_rename N'[dbo].[coreOperation]', N'Operation', 'OBJECT';
ALTER SCHEMA [Core] TRANSFER [dbo].[Operation];

EXEC sp_rename N'[dbo].[coreModule]', N'Module', 'OBJECT';
ALTER SCHEMA [Core] TRANSFER [dbo].[Module];

EXEC sp_rename N'[Core].[Step].[IX_lupStep_TenantId_Code]', N'IX_Step_TenantId_Code', 'INDEX';

EXEC sp_rename N'[Hrms].[WorkWeekConfiguration].[IX_hrmsWorkWeekConfiguration_TenantId_IsActive]', N'IX_WorkWeekConfiguration_TenantId_IsActive', 'INDEX';

EXEC sp_rename N'[Hrms].[WorkLocation].[IX_hrmsWorkLocation_TenantId_Code]', N'IX_WorkLocation_TenantId_Code', 'INDEX';

EXEC sp_rename N'[Hrms].[WorkLocation].[IX_hrmsWorkLocation_ParentId]', N'IX_WorkLocation_ParentId', 'INDEX';

EXEC sp_rename N'[Hrms].[WorkforcePlanLine].[IX_hrmsWorkforcePlanLine_PositionClassId]', N'IX_WorkforcePlanLine_PositionClassId', 'INDEX';

EXEC sp_rename N'[Hrms].[WorkforcePlanLine].[IX_hrmsWorkforcePlanLine_PlanId_OrganizationUnitId_PositionClassId_EmploymentType_PeriodIndex]', N'IX_WorkforcePlanLine_PlanId_OrganizationUnitId_PositionClassId_EmploymentType_PeriodIndex', 'INDEX';

EXEC sp_rename N'[Hrms].[WorkforcePlanLine].[IX_hrmsWorkforcePlanLine_PlanId]', N'IX_WorkforcePlanLine_PlanId', 'INDEX';

EXEC sp_rename N'[Hrms].[WorkforcePlanLine].[IX_hrmsWorkforcePlanLine_OrganizationUnitId]', N'IX_WorkforcePlanLine_OrganizationUnitId', 'INDEX';

EXEC sp_rename N'[Hrms].[WorkforcePlan].[IX_hrmsWorkforcePlan_TenantId_Status]', N'IX_WorkforcePlan_TenantId_Status', 'INDEX';

EXEC sp_rename N'[Hrms].[WorkforcePlan].[IX_hrmsWorkforcePlan_StartFiscalYearId]', N'IX_WorkforcePlan_StartFiscalYearId', 'INDEX';

EXEC sp_rename N'[Hrms].[WorkforcePlan].[IX_hrmsWorkforcePlan_RootPlanId]', N'IX_WorkforcePlan_RootPlanId', 'INDEX';

EXEC sp_rename N'[Hrms].[WorkforcePlan].[IX_hrmsWorkforcePlan_OrganizationUnitId]', N'IX_WorkforcePlan_OrganizationUnitId', 'INDEX';

EXEC sp_rename N'[Hrms].[WorkflowStepApprover].[IX_hrmsWorkflowStepApprover_StepId]', N'IX_WorkflowStepApprover_StepId', 'INDEX';

EXEC sp_rename N'[Hrms].[WorkflowStepApprover].[IX_hrmsWorkflowStepApprover_ApproverType_ApproverId]', N'IX_WorkflowStepApprover_ApproverType_ApproverId', 'INDEX';

EXEC sp_rename N'[Hrms].[WorkflowStep].[IX_hrmsWorkflowStep_DefinitionId_StepOrder]', N'IX_WorkflowStep_DefinitionId_StepOrder', 'INDEX';

EXEC sp_rename N'[Hrms].[WorkflowInstance].[IX_hrmsWorkflowInstance_TenantId_Status]', N'IX_WorkflowInstance_TenantId_Status', 'INDEX';

EXEC sp_rename N'[Hrms].[WorkflowInstance].[IX_hrmsWorkflowInstance_Status]', N'IX_WorkflowInstance_Status', 'INDEX';

EXEC sp_rename N'[Hrms].[WorkflowInstance].[IX_hrmsWorkflowInstance_EntityType_EntityId]', N'IX_WorkflowInstance_EntityType_EntityId', 'INDEX';

EXEC sp_rename N'[Hrms].[WorkflowInstance].[IX_hrmsWorkflowInstance_DefinitionId]', N'IX_WorkflowInstance_DefinitionId', 'INDEX';

EXEC sp_rename N'[Hrms].[WorkflowDefinition].[IX_hrmsWorkflowDefinition_TenantId_EntityType]', N'IX_WorkflowDefinition_TenantId_EntityType', 'INDEX';

EXEC sp_rename N'[Hrms].[WorkflowActionLog].[IX_hrmsWorkflowActionLog_InstanceId]', N'IX_WorkflowActionLog_InstanceId', 'INDEX';

EXEC sp_rename N'[Hrms].[TripRequest].[IX_hrmsTripRequest_TripBudgetId]', N'IX_TripRequest_TripBudgetId', 'INDEX';

EXEC sp_rename N'[Hrms].[TripRequest].[IX_hrmsTripRequest_TenantId_TripNumber]', N'IX_TripRequest_TenantId_TripNumber', 'INDEX';

EXEC sp_rename N'[Hrms].[TripRequest].[IX_hrmsTripRequest_Status]', N'IX_TripRequest_Status', 'INDEX';

EXEC sp_rename N'[Hrms].[TripRequest].[IX_hrmsTripRequest_EmployeeId_Status]', N'IX_TripRequest_EmployeeId_Status', 'INDEX';

EXEC sp_rename N'[Hrms].[TripExpense].[IX_hrmsTripExpense_TripRequestId]', N'IX_TripExpense_TripRequestId', 'INDEX';

EXEC sp_rename N'[Hrms].[TripBudget].[IX_hrmsTripBudget_TenantId_FiscalYear_OrganizationUnitId]', N'IX_TripBudget_TenantId_FiscalYear_OrganizationUnitId', 'INDEX';

EXEC sp_rename N'[Hrms].[TripBudget].[IX_hrmsTripBudget_OrganizationUnitId]', N'IX_TripBudget_OrganizationUnitId', 'INDEX';

EXEC sp_rename N'[Hrms].[TrainingSession].[IX_hrmsTrainingSession_TrainingCourseId]', N'IX_TrainingSession_TrainingCourseId', 'INDEX';

EXEC sp_rename N'[Hrms].[TrainingSession].[IX_hrmsTrainingSession_TenantId_TrainingCourseId]', N'IX_TrainingSession_TenantId_TrainingCourseId', 'INDEX';

EXEC sp_rename N'[Hrms].[TrainingSession].[IX_hrmsTrainingSession_TenantId_StartDate]', N'IX_TrainingSession_TenantId_StartDate', 'INDEX';

EXEC sp_rename N'[Hrms].[TrainingProviderPayment].[IX_hrmsTrainingProviderPayment_TrainingSessionId]', N'IX_TrainingProviderPayment_TrainingSessionId', 'INDEX';

EXEC sp_rename N'[Hrms].[TrainingProviderPayment].[IX_hrmsTrainingProviderPayment_TenantId_TrainingSessionId]', N'IX_TrainingProviderPayment_TenantId_TrainingSessionId', 'INDEX';

EXEC sp_rename N'[Hrms].[TrainingProviderPayment].[IX_hrmsTrainingProviderPayment_TenantId_Status]', N'IX_TrainingProviderPayment_TenantId_Status', 'INDEX';

EXEC sp_rename N'[Hrms].[TrainingNeed].[IX_hrmsTrainingNeed_TrainingCourseId]', N'IX_TrainingNeed_TrainingCourseId', 'INDEX';

EXEC sp_rename N'[Hrms].[TrainingNeed].[IX_hrmsTrainingNeed_TenantId_Status]', N'IX_TrainingNeed_TenantId_Status', 'INDEX';

EXEC sp_rename N'[Hrms].[TrainingNeed].[IX_hrmsTrainingNeed_TenantId_EmployeeId]', N'IX_TrainingNeed_TenantId_EmployeeId', 'INDEX';

EXEC sp_rename N'[Hrms].[TrainingNeed].[IX_hrmsTrainingNeed_EmployeeId]', N'IX_TrainingNeed_EmployeeId', 'INDEX';

EXEC sp_rename N'[Hrms].[TrainingNeed].[IX_hrmsTrainingNeed_CompetencyId]', N'IX_TrainingNeed_CompetencyId', 'INDEX';

EXEC sp_rename N'[Hrms].[TrainingEnrollment].[IX_hrmsTrainingEnrollment_TrainingSessionId]', N'IX_TrainingEnrollment_TrainingSessionId', 'INDEX';

EXEC sp_rename N'[Hrms].[TrainingEnrollment].[IX_hrmsTrainingEnrollment_TrainingNeedId]', N'IX_TrainingEnrollment_TrainingNeedId', 'INDEX';

EXEC sp_rename N'[Hrms].[TrainingEnrollment].[IX_hrmsTrainingEnrollment_TenantId_TrainingSessionId_EmployeeId]', N'IX_TrainingEnrollment_TenantId_TrainingSessionId_EmployeeId', 'INDEX';

EXEC sp_rename N'[Hrms].[TrainingEnrollment].[IX_hrmsTrainingEnrollment_TenantId_EmployeeId_Status]', N'IX_TrainingEnrollment_TenantId_EmployeeId_Status', 'INDEX';

EXEC sp_rename N'[Hrms].[TrainingEnrollment].[IX_hrmsTrainingEnrollment_EmployeeId]', N'IX_TrainingEnrollment_EmployeeId', 'INDEX';

EXEC sp_rename N'[Hrms].[TrainingCourse].[IX_hrmsTrainingCourse_TrainingCategoryId]', N'IX_TrainingCourse_TrainingCategoryId', 'INDEX';

EXEC sp_rename N'[Hrms].[TrainingCourse].[IX_hrmsTrainingCourse_TenantId_TrainingCategoryId]', N'IX_TrainingCourse_TenantId_TrainingCategoryId', 'INDEX';

EXEC sp_rename N'[Hrms].[TrainingCourse].[IX_hrmsTrainingCourse_TenantId_Name]', N'IX_TrainingCourse_TenantId_Name', 'INDEX';

EXEC sp_rename N'[Hrms].[TrainingCategory].[IX_hrmsTrainingCategory_TenantId_Name]', N'IX_TrainingCategory_TenantId_Name', 'INDEX';

EXEC sp_rename N'[Hrms].[TrainingBudget].[IX_hrmsTrainingBudget_TenantId_FiscalYear_OrganizationUnitId]', N'IX_TrainingBudget_TenantId_FiscalYear_OrganizationUnitId', 'INDEX';

EXEC sp_rename N'[Hrms].[TrainingBudget].[IX_hrmsTrainingBudget_OrganizationUnitId]', N'IX_TrainingBudget_OrganizationUnitId', 'INDEX';

EXEC sp_rename N'[Hrms].[TerminationSettlement].[IX_hrmsTerminationSettlement_TerminationId]', N'IX_TerminationSettlement_TerminationId', 'INDEX';

EXEC sp_rename N'[Hrms].[TerminationSettlement].[IX_hrmsTerminationSettlement_TenantId_TerminationId]', N'IX_TerminationSettlement_TenantId_TerminationId', 'INDEX';

EXEC sp_rename N'[Hrms].[TerminationClearance].[IX_hrmsTerminationClearance_TerminationId]', N'IX_TerminationClearance_TerminationId', 'INDEX';

EXEC sp_rename N'[Hrms].[TerminationClearance].[IX_hrmsTerminationClearance_DepartmentId]', N'IX_TerminationClearance_DepartmentId', 'INDEX';

EXEC sp_rename N'[Hrms].[TerminationAssetRecovery].[IX_hrmsTerminationAssetRecovery_TerminationId]', N'IX_TerminationAssetRecovery_TerminationId', 'INDEX';

EXEC sp_rename N'[Hrms].[TerminationAssetRecovery].[IX_hrmsTerminationAssetRecovery_TenantId_TerminationId]', N'IX_TerminationAssetRecovery_TenantId_TerminationId', 'INDEX';

EXEC sp_rename N'[Hrms].[TerminationAssetRecovery].[IX_hrmsTerminationAssetRecovery_CompanyAssetId]', N'IX_TerminationAssetRecovery_CompanyAssetId', 'INDEX';

EXEC sp_rename N'[Hrms].[TalentReview].[IX_hrmsTalentReview_TenantId_Status]', N'IX_TalentReview_TenantId_Status', 'INDEX';

EXEC sp_rename N'[Hrms].[TalentReview].[IX_hrmsTalentReview_OrganizationUnitId]', N'IX_TalentReview_OrganizationUnitId', 'INDEX';

EXEC sp_rename N'[Hrms].[TalentRating].[IX_hrmsTalentRating_TalentAssessmentId]', N'IX_TalentRating_TalentAssessmentId', 'INDEX';

EXEC sp_rename N'[Hrms].[TalentRating].[IX_hrmsTalentRating_RaterEmployeeId]', N'IX_TalentRating_RaterEmployeeId', 'INDEX';

EXEC sp_rename N'[Hrms].[TalentAssessment].[IX_hrmsTalentAssessment_TenantId_TalentReviewId_PerformanceBand_PotentialBand]', N'IX_TalentAssessment_TenantId_TalentReviewId_PerformanceBand_PotentialBand', 'INDEX';

EXEC sp_rename N'[Hrms].[TalentAssessment].[IX_hrmsTalentAssessment_TalentReviewId_EmployeeId]', N'IX_TalentAssessment_TalentReviewId_EmployeeId', 'INDEX';

EXEC sp_rename N'[Hrms].[TalentAssessment].[IX_hrmsTalentAssessment_EmployeeId]', N'IX_TalentAssessment_EmployeeId', 'INDEX';

EXEC sp_rename N'[Hrms].[SurveyResponse].[IX_hrmsSurveyResponse_TenantId_SurveyId]', N'IX_SurveyResponse_TenantId_SurveyId', 'INDEX';

EXEC sp_rename N'[Hrms].[SurveyResponse].[IX_hrmsSurveyResponse_SurveyId]', N'IX_SurveyResponse_SurveyId', 'INDEX';

EXEC sp_rename N'[Hrms].[SurveyCompletion].[IX_hrmsSurveyCompletion_TenantId_SurveyId_EmployeeId]', N'IX_SurveyCompletion_TenantId_SurveyId_EmployeeId', 'INDEX';

EXEC sp_rename N'[Hrms].[SurveyCompletion].[IX_hrmsSurveyCompletion_SurveyId]', N'IX_SurveyCompletion_SurveyId', 'INDEX';

EXEC sp_rename N'[Hrms].[SurveyCompletion].[IX_hrmsSurveyCompletion_EmployeeId]', N'IX_SurveyCompletion_EmployeeId', 'INDEX';

EXEC sp_rename N'[Hrms].[Survey].[IX_hrmsSurvey_TenantId_Status]', N'IX_Survey_TenantId_Status', 'INDEX';

EXEC sp_rename N'[Hrms].[Suggestion].[IX_hrmsSuggestion_TenantId_Status]', N'IX_Suggestion_TenantId_Status', 'INDEX';

EXEC sp_rename N'[Hrms].[SuccessionPlan].[IX_hrmsSuccessionPlan_TenantId_Status]', N'IX_SuccessionPlan_TenantId_Status', 'INDEX';

EXEC sp_rename N'[Hrms].[SuccessionPlan].[IX_hrmsSuccessionPlan_TenantId_CriticalPositionId]', N'IX_SuccessionPlan_TenantId_CriticalPositionId', 'INDEX';

EXEC sp_rename N'[Hrms].[SuccessionPlan].[IX_hrmsSuccessionPlan_CriticalPositionId]', N'IX_SuccessionPlan_CriticalPositionId', 'INDEX';

EXEC sp_rename N'[Hrms].[SuccessionDevelopmentAction].[IX_hrmsSuccessionDevelopmentAction_SuccessionCandidateId]', N'IX_SuccessionDevelopmentAction_SuccessionCandidateId', 'INDEX';

EXEC sp_rename N'[Hrms].[SuccessionDevelopmentAction].[IX_hrmsSuccessionDevelopmentAction_MentorEmployeeId]', N'IX_SuccessionDevelopmentAction_MentorEmployeeId', 'INDEX';

EXEC sp_rename N'[Hrms].[SuccessionCandidate].[IX_hrmsSuccessionCandidate_TenantId_EmployeeId]', N'IX_SuccessionCandidate_TenantId_EmployeeId', 'INDEX';

EXEC sp_rename N'[Hrms].[SuccessionCandidate].[IX_hrmsSuccessionCandidate_SuccessionPlanId_Rank]', N'IX_SuccessionCandidate_SuccessionPlanId_Rank', 'INDEX';

EXEC sp_rename N'[Hrms].[SuccessionCandidate].[IX_hrmsSuccessionCandidate_SuccessionPlanId_EmployeeId]', N'IX_SuccessionCandidate_SuccessionPlanId_EmployeeId', 'INDEX';

EXEC sp_rename N'[Hrms].[SuccessionCandidate].[IX_hrmsSuccessionCandidate_EmployeeId]', N'IX_SuccessionCandidate_EmployeeId', 'INDEX';

EXEC sp_rename N'[Hrms].[SettlementLine].[IX_hrmsSettlementLine_TerminationSettlementId]', N'IX_SettlementLine_TerminationSettlementId', 'INDEX';

EXEC sp_rename N'[Hrms].[SettlementLine].[IX_hrmsSettlementLine_TenantId_TerminationSettlementId]', N'IX_SettlementLine_TenantId_TerminationSettlementId', 'INDEX';

EXEC sp_rename N'[Hrms].[SalaryRevisionLine].[IX_hrmsSalaryRevisionLine_SalaryRevisionId]', N'IX_SalaryRevisionLine_SalaryRevisionId', 'INDEX';

EXEC sp_rename N'[Hrms].[SalaryRevisionLine].[IX_hrmsSalaryRevisionLine_EmployeeId]', N'IX_SalaryRevisionLine_EmployeeId', 'INDEX';

EXEC sp_rename N'[Hrms].[SalaryRevisionBand].[IX_hrmsSalaryRevisionBand_SalaryRevisionId_MinScore]', N'IX_SalaryRevisionBand_SalaryRevisionId_MinScore', 'INDEX';

EXEC sp_rename N'[Hrms].[SalaryRevision].[IX_hrmsSalaryRevision_Status]', N'IX_SalaryRevision_Status', 'INDEX';

EXEC sp_rename N'[Hrms].[RewardPointsTransaction].[IX_hrmsRewardPointsTransaction_TenantId_EmployeeId_TransactionDate]', N'IX_RewardPointsTransaction_TenantId_EmployeeId_TransactionDate', 'INDEX';

EXEC sp_rename N'[Hrms].[RewardPointsTransaction].[IX_hrmsRewardPointsTransaction_EmployeeId]', N'IX_RewardPointsTransaction_EmployeeId', 'INDEX';

EXEC sp_rename N'[Hrms].[RewardNomination].[IX_hrmsRewardNomination_TenantId_Status]', N'IX_RewardNomination_TenantId_Status', 'INDEX';

EXEC sp_rename N'[Hrms].[RewardNomination].[IX_hrmsRewardNomination_TenantId_NomineeEmployeeId]', N'IX_RewardNomination_TenantId_NomineeEmployeeId', 'INDEX';

EXEC sp_rename N'[Hrms].[RewardNomination].[IX_hrmsRewardNomination_RecognitionProgramId]', N'IX_RewardNomination_RecognitionProgramId', 'INDEX';

EXEC sp_rename N'[Hrms].[RewardNomination].[IX_hrmsRewardNomination_RecognitionBadgeId]', N'IX_RewardNomination_RecognitionBadgeId', 'INDEX';

EXEC sp_rename N'[Hrms].[RewardNomination].[IX_hrmsRewardNomination_NomineeEmployeeId]', N'IX_RewardNomination_NomineeEmployeeId', 'INDEX';

EXEC sp_rename N'[Hrms].[RewardDisbursement].[IX_hrmsRewardDisbursement_TenantId_Status]', N'IX_RewardDisbursement_TenantId_Status', 'INDEX';

EXEC sp_rename N'[Hrms].[RewardDisbursement].[IX_hrmsRewardDisbursement_TenantId_EmployeeId]', N'IX_RewardDisbursement_TenantId_EmployeeId', 'INDEX';

EXEC sp_rename N'[Hrms].[RewardDisbursement].[IX_hrmsRewardDisbursement_RecognitionBadgeId]', N'IX_RewardDisbursement_RecognitionBadgeId', 'INDEX';

EXEC sp_rename N'[Hrms].[RewardDisbursement].[IX_hrmsRewardDisbursement_EmployeeRecognitionId]', N'IX_RewardDisbursement_EmployeeRecognitionId', 'INDEX';

EXEC sp_rename N'[Hrms].[RewardDisbursement].[IX_hrmsRewardDisbursement_EmployeeId]', N'IX_RewardDisbursement_EmployeeId', 'INDEX';

EXEC sp_rename N'[Hrms].[ReviewCycle].[IX_hrmsReviewCycle_TenantId_Status]', N'IX_ReviewCycle_TenantId_Status', 'INDEX';

EXEC sp_rename N'[Hrms].[ReviewCycle].[IX_hrmsReviewCycle_TenantId_Name]', N'IX_ReviewCycle_TenantId_Name', 'INDEX';

EXEC sp_rename N'[Hrms].[ReviewCycle].[IX_hrmsReviewCycle_RatingScaleId]', N'IX_ReviewCycle_RatingScaleId', 'INDEX';

EXEC sp_rename N'[Hrms].[ReviewCycle].[IX_hrmsReviewCycle_FiscalYearId]', N'IX_ReviewCycle_FiscalYearId', 'INDEX';

EXEC sp_rename N'[Hrms].[RequisitionScreeningCriterion].[IX_hrmsRequisitionScreeningCriterion_RequisitionId]', N'IX_RequisitionScreeningCriterion_RequisitionId', 'INDEX';

EXEC sp_rename N'[Hrms].[ReportScheduleRecipient].[IX_hrmsReportScheduleRecipient_ReportScheduleId]', N'IX_ReportScheduleRecipient_ReportScheduleId', 'INDEX';

EXEC sp_rename N'[Hrms].[ReportScheduleFieldValue].[IX_hrmsReportScheduleFieldValue_ReportScheduleId]', N'IX_ReportScheduleFieldValue_ReportScheduleId', 'INDEX';

EXEC sp_rename N'[Hrms].[ReportScheduleFieldOutput].[IX_hrmsReportScheduleFieldOutput_ReportScheduleId]', N'IX_ReportScheduleFieldOutput_ReportScheduleId', 'INDEX';

EXEC sp_rename N'[Hrms].[ReportSchedule].[IX_hrmsReportSchedule_ReportId]', N'IX_ReportSchedule_ReportId', 'INDEX';

EXEC sp_rename N'[Hrms].[ReportSavedFilter].[IX_hrmsReportSavedFilter_ReportId]', N'IX_ReportSavedFilter_ReportId', 'INDEX';

EXEC sp_rename N'[Hrms].[ReportRunRecipient].[IX_hrmsReportRunRecipient_ReportRunId]', N'IX_ReportRunRecipient_ReportRunId', 'INDEX';

EXEC sp_rename N'[Hrms].[ReportRun].[IX_hrmsReportRun_TenantId_ReportKey]', N'IX_ReportRun_TenantId_ReportKey', 'INDEX';

EXEC sp_rename N'[Hrms].[ReportRestriction].[IX_hrmsReportRestriction_RoleId]', N'IX_ReportRestriction_RoleId', 'INDEX';

EXEC sp_rename N'[Hrms].[ReportRestriction].[IX_hrmsReportRestriction_ReportId]', N'IX_ReportRestriction_ReportId', 'INDEX';

EXEC sp_rename N'[Hrms].[ReportFieldOutput].[IX_hrmsReportFieldOutput_ReportId]', N'IX_ReportFieldOutput_ReportId', 'INDEX';

EXEC sp_rename N'[Hrms].[ReportField].[IX_hrmsReportField_ReportId]', N'IX_ReportField_ReportId', 'INDEX';

EXEC sp_rename N'[Hrms].[Report].[IX_hrmsReport_TenantId_ReportKey]', N'IX_Report_TenantId_ReportKey', 'INDEX';

EXEC sp_rename N'[Hrms].[Report].[IX_hrmsReport_TenantId_IsActive]', N'IX_Report_TenantId_IsActive', 'INDEX';

EXEC sp_rename N'[Hrms].[RecognitionProgram].[IX_hrmsRecognitionProgram_TenantId_Name]', N'IX_RecognitionProgram_TenantId_Name', 'INDEX';

EXEC sp_rename N'[Hrms].[RecognitionProgram].[IX_hrmsRecognitionProgram_RecognitionBadgeId]', N'IX_RecognitionProgram_RecognitionBadgeId', 'INDEX';

EXEC sp_rename N'[Hrms].[RecognitionBadge].[IX_hrmsRecognitionBadge_TenantId_Name]', N'IX_RecognitionBadge_TenantId_Name', 'INDEX';

EXEC sp_rename N'[Hrms].[RecognitionBadge].[IX_hrmsRecognitionBadge_AwardCategoryId]', N'IX_RecognitionBadge_AwardCategoryId', 'INDEX';

EXEC sp_rename N'[Hrms].[RatingScaleLevel].[IX_hrmsRatingScaleLevel_RatingScaleId_Value]', N'IX_RatingScaleLevel_RatingScaleId_Value', 'INDEX';

EXEC sp_rename N'[Hrms].[RatingScale].[IX_hrmsRatingScale_TenantId_Name]', N'IX_RatingScale_TenantId_Name', 'INDEX';

EXEC sp_rename N'[Hrms].[ProfileChangeRequest].[IX_hrmsProfileChangeRequest_Status]', N'IX_ProfileChangeRequest_Status', 'INDEX';

EXEC sp_rename N'[Hrms].[ProfileChangeRequest].[IX_hrmsProfileChangeRequest_EmployeeId]', N'IX_ProfileChangeRequest_EmployeeId', 'INDEX';

EXEC sp_rename N'[Hrms].[PositionCompetency].[IX_hrmsPositionCompetency_PositionId_CompetencyId]', N'IX_PositionCompetency_PositionId_CompetencyId', 'INDEX';

EXEC sp_rename N'[Hrms].[PositionCompetency].[IX_hrmsPositionCompetency_CompetencyId]', N'IX_PositionCompetency_CompetencyId', 'INDEX';

EXEC sp_rename N'[Hrms].[PositionClass].[IX_hrmsPositionClass_WorkLocationId]', N'IX_PositionClass_WorkLocationId', 'INDEX';

EXEC sp_rename N'[Hrms].[PositionClass].[IX_hrmsPositionClass_TenantId_Code]', N'IX_PositionClass_TenantId_Code', 'INDEX';

EXEC sp_rename N'[Hrms].[PositionClass].[IX_hrmsPositionClass_SalaryScaleId]', N'IX_PositionClass_SalaryScaleId', 'INDEX';

EXEC sp_rename N'[Hrms].[PositionClass].[IX_hrmsPositionClass_ReportsToPositionClassId]', N'IX_PositionClass_ReportsToPositionClassId', 'INDEX';

EXEC sp_rename N'[Hrms].[PositionClass].[IX_hrmsPositionClass_JobCategoryId]', N'IX_PositionClass_JobCategoryId', 'INDEX';

EXEC sp_rename N'[Hrms].[Position].[IX_hrmsPosition_TenantId_BranchId_Code]', N'IX_Position_TenantId_BranchId_Code', 'INDEX';

EXEC sp_rename N'[Hrms].[Position].[IX_hrmsPosition_PositionClassId]', N'IX_Position_PositionClassId', 'INDEX';

EXEC sp_rename N'[Hrms].[Position].[IX_hrmsPosition_OrganizationUnitId]', N'IX_Position_OrganizationUnitId', 'INDEX';

EXEC sp_rename N'[Hrms].[Position].[IX_hrmsPosition_BranchId]', N'IX_Position_BranchId', 'INDEX';

EXEC sp_rename N'[Hrms].[PipObjective].[IX_hrmsPipObjective_PipId_SortOrder]', N'IX_PipObjective_PipId_SortOrder', 'INDEX';

EXEC sp_rename N'[Hrms].[PerformanceHistory].[IX_hrmsPerformanceHistory_TenantId_EntityType_EntityId]', N'IX_PerformanceHistory_TenantId_EntityType_EntityId', 'INDEX';

EXEC sp_rename N'[Hrms].[PerDiemRate].[IX_hrmsPerDiemRate_TenantId_JobGradeId_TripType]', N'IX_PerDiemRate_TenantId_JobGradeId_TripType', 'INDEX';

EXEC sp_rename N'[Hrms].[PerDiemRate].[IX_hrmsPerDiemRate_JobGradeId]', N'IX_PerDiemRate_JobGradeId', 'INDEX';

EXEC sp_rename N'[Hrms].[OtherLeaveSetting].[IX_hrmsOtherLeaveSetting_TenantId_IsActive]', N'IX_OtherLeaveSetting_TenantId_IsActive', 'INDEX';

EXEC sp_rename N'[Hrms].[OtherLeaveSetting].[IX_hrmsOtherLeaveSetting_TenantId_FiscalYearId_LeaveTypeId]', N'IX_OtherLeaveSetting_TenantId_FiscalYearId_LeaveTypeId', 'INDEX';

EXEC sp_rename N'[Hrms].[OtherLeaveSetting].[IX_hrmsOtherLeaveSetting_LeaveTypeId]', N'IX_OtherLeaveSetting_LeaveTypeId', 'INDEX';

EXEC sp_rename N'[Hrms].[OtherLeaveSetting].[IX_hrmsOtherLeaveSetting_FiscalYearId]', N'IX_OtherLeaveSetting_FiscalYearId', 'INDEX';

EXEC sp_rename N'[Hrms].[OtherLeaveDetail].[IX_hrmsOtherLeaveDetail_OtherLeaveHeaderId_StartDate_EndDate]', N'IX_OtherLeaveDetail_OtherLeaveHeaderId_StartDate_EndDate', 'INDEX';

EXEC sp_rename N'[Hrms].[OtherLeaveDetail].[IX_hrmsOtherLeaveDetail_OtherLeaveHeaderId]', N'IX_OtherLeaveDetail_OtherLeaveHeaderId', 'INDEX';

EXEC sp_rename N'[Hrms].[OtherLeave].[IX_hrmsOtherLeave_OtherLeaveSettingId]', N'IX_OtherLeave_OtherLeaveSettingId', 'INDEX';

EXEC sp_rename N'[Hrms].[OtherLeave].[IX_hrmsOtherLeave_EmployeeId_Status]', N'IX_OtherLeave_EmployeeId_Status', 'INDEX';

EXEC sp_rename N'[Hrms].[OtherLeave].[IX_hrmsOtherLeave_EmployeeId]', N'IX_OtherLeave_EmployeeId', 'INDEX';

EXEC sp_rename N'[Hrms].[OrganizationUnit].[IX_hrmsOrganizationUnit_WorkLocationId]', N'IX_OrganizationUnit_WorkLocationId', 'INDEX';

EXEC sp_rename N'[Hrms].[OrganizationUnit].[IX_hrmsOrganizationUnit_TenantId_BranchId_Code]', N'IX_OrganizationUnit_TenantId_BranchId_Code', 'INDEX';

EXEC sp_rename N'[Hrms].[OrganizationUnit].[IX_hrmsOrganizationUnit_ParentId]', N'IX_OrganizationUnit_ParentId', 'INDEX';

EXEC sp_rename N'[Hrms].[OrganizationUnit].[IX_hrmsOrganizationUnit_BranchId]', N'IX_OrganizationUnit_BranchId', 'INDEX';

EXEC sp_rename N'[Hrms].[OrganizationalObjective].[IX_hrmsOrganizationalObjective_TenantId_ReviewCycleId_Title]', N'IX_OrganizationalObjective_TenantId_ReviewCycleId_Title', 'INDEX';

EXEC sp_rename N'[Hrms].[OrganizationalObjective].[IX_hrmsOrganizationalObjective_TenantId_ReviewCycleId]', N'IX_OrganizationalObjective_TenantId_ReviewCycleId', 'INDEX';

EXEC sp_rename N'[Hrms].[OrganizationalObjective].[IX_hrmsOrganizationalObjective_ReviewCycleId]', N'IX_OrganizationalObjective_ReviewCycleId', 'INDEX';

EXEC sp_rename N'[Hrms].[OrganizationalObjective].[IX_hrmsOrganizationalObjective_ParentObjectiveId]', N'IX_OrganizationalObjective_ParentObjectiveId', 'INDEX';

EXEC sp_rename N'[Hrms].[OrganizationalObjective].[IX_hrmsOrganizationalObjective_OrganizationUnitId]', N'IX_OrganizationalObjective_OrganizationUnitId', 'INDEX';

EXEC sp_rename N'[Hrms].[OfferLetterTemplate].[IX_hrmsOfferLetterTemplate_TenantId]', N'IX_OfferLetterTemplate_TenantId', 'INDEX';

EXEC sp_rename N'[Hrms].[Mentorship].[IX_hrmsMentorship_TenantId_MenteeEmployeeId]', N'IX_Mentorship_TenantId_MenteeEmployeeId', 'INDEX';

EXEC sp_rename N'[Hrms].[Mentorship].[IX_hrmsMentorship_MentorEmployeeId]', N'IX_Mentorship_MentorEmployeeId', 'INDEX';

EXEC sp_rename N'[Hrms].[Mentorship].[IX_hrmsMentorship_MenteeEmployeeId]', N'IX_Mentorship_MenteeEmployeeId', 'INDEX';

EXEC sp_rename N'[Hrms].[MedicalServiceContract].[IX_hrmsMedicalServiceContract_Status]', N'IX_MedicalServiceContract_Status', 'INDEX';

EXEC sp_rename N'[Hrms].[MedicalServiceContract].[IX_hrmsMedicalServiceContract_MedicalProviderId]', N'IX_MedicalServiceContract_MedicalProviderId', 'INDEX';

EXEC sp_rename N'[Hrms].[MedicalProvider].[IX_hrmsMedicalProvider_TenantId_Name]', N'IX_MedicalProvider_TenantId_Name', 'INDEX';

EXEC sp_rename N'[Hrms].[MedicalPlan].[IX_hrmsMedicalPlan_TenantId_Name]', N'IX_MedicalPlan_TenantId_Name', 'INDEX';

EXEC sp_rename N'[Hrms].[MedicalEnrollment].[IX_hrmsMedicalEnrollment_MedicalPlanId]', N'IX_MedicalEnrollment_MedicalPlanId', 'INDEX';

EXEC sp_rename N'[Hrms].[MedicalEnrollment].[IX_hrmsMedicalEnrollment_EmployeeId]', N'IX_MedicalEnrollment_EmployeeId', 'INDEX';

EXEC sp_rename N'[Hrms].[MedicalClaimAttachment].[IX_hrmsMedicalClaimAttachment_MedicalClaimId]', N'IX_MedicalClaimAttachment_MedicalClaimId', 'INDEX';

EXEC sp_rename N'[Hrms].[MedicalClaim].[IX_hrmsMedicalClaim_TenantId_ClaimNumber]', N'IX_MedicalClaim_TenantId_ClaimNumber', 'INDEX';

EXEC sp_rename N'[Hrms].[MedicalClaim].[IX_hrmsMedicalClaim_Status]', N'IX_MedicalClaim_Status', 'INDEX';

EXEC sp_rename N'[Hrms].[MedicalClaim].[IX_hrmsMedicalClaim_MedicalEnrollmentId]', N'IX_MedicalClaim_MedicalEnrollmentId', 'INDEX';

EXEC sp_rename N'[Hrms].[MedicalClaim].[IX_hrmsMedicalClaim_MedicalBeneficiaryId_Status]', N'IX_MedicalClaim_MedicalBeneficiaryId_Status', 'INDEX';

EXEC sp_rename N'[Hrms].[MedicalClaim].[IX_hrmsMedicalClaim_EmployeeId]', N'IX_MedicalClaim_EmployeeId', 'INDEX';

EXEC sp_rename N'[Hrms].[MedicalBeneficiary].[IX_hrmsMedicalBeneficiary_MedicalEnrollmentId]', N'IX_MedicalBeneficiary_MedicalEnrollmentId', 'INDEX';

EXEC sp_rename N'[Hrms].[LoanType].[IX_hrmsLoanType_TenantId_Name]', N'IX_LoanType_TenantId_Name', 'INDEX';

EXEC sp_rename N'[Hrms].[LoanRepaymentSchedule].[IX_hrmsLoanRepaymentSchedule_Status_DueDate]', N'IX_LoanRepaymentSchedule_Status_DueDate', 'INDEX';

EXEC sp_rename N'[Hrms].[LoanRepaymentSchedule].[IX_hrmsLoanRepaymentSchedule_LoanId_InstallmentNo]', N'IX_LoanRepaymentSchedule_LoanId_InstallmentNo', 'INDEX';

EXEC sp_rename N'[Hrms].[LoanGuarantor].[IX_hrmsLoanGuarantor_LoanId]', N'IX_LoanGuarantor_LoanId', 'INDEX';

EXEC sp_rename N'[Hrms].[Loan].[IX_hrmsLoan_TenantId_LoanNumber]', N'IX_Loan_TenantId_LoanNumber', 'INDEX';

EXEC sp_rename N'[Hrms].[Loan].[IX_hrmsLoan_Status]', N'IX_Loan_Status', 'INDEX';

EXEC sp_rename N'[Hrms].[Loan].[IX_hrmsLoan_LoanTypeId]', N'IX_Loan_LoanTypeId', 'INDEX';

EXEC sp_rename N'[Hrms].[Loan].[IX_hrmsLoan_EmployeeId_Status]', N'IX_Loan_EmployeeId_Status', 'INDEX';

EXEC sp_rename N'[Hrms].[LeaveType].[IX_hrmsLeaveType_TenantId_Code]', N'IX_LeaveType_TenantId_Code', 'INDEX';

EXEC sp_rename N'[Hrms].[LeaveRequestLine].[IX_hrmsLeaveRequestLine_LeaveTypeId]', N'IX_LeaveRequestLine_LeaveTypeId', 'INDEX';

EXEC sp_rename N'[Hrms].[LeaveRequestLine].[IX_hrmsLeaveRequestLine_LeaveRequestId]', N'IX_LeaveRequestLine_LeaveRequestId', 'INDEX';

EXEC sp_rename N'[Hrms].[LeaveRequest].[IX_hrmsLeaveRequest_FiscalYearId]', N'IX_LeaveRequest_FiscalYearId', 'INDEX';

EXEC sp_rename N'[Hrms].[LeaveRequest].[IX_hrmsLeaveRequest_EmployeeId_Status]', N'IX_LeaveRequest_EmployeeId_Status', 'INDEX';

EXEC sp_rename N'[Hrms].[LeaveRequest].[IX_hrmsLeaveRequest_EmployeeId]', N'IX_LeaveRequest_EmployeeId', 'INDEX';

EXEC sp_rename N'[Hrms].[LeaveBalanceTransaction].[IX_hrmsLeaveBalanceTransaction_ReferenceId]', N'IX_LeaveBalanceTransaction_ReferenceId', 'INDEX';

EXEC sp_rename N'[Hrms].[LeaveBalanceTransaction].[IX_hrmsLeaveBalanceTransaction_EmployeeId_LeaveTypeId_FiscalYearId]', N'IX_LeaveBalanceTransaction_EmployeeId_LeaveTypeId_FiscalYearId', 'INDEX';

EXEC sp_rename N'[Hrms].[LeaveBalance].[IX_hrmsLeaveBalance_TenantId_EmployeeId_LeaveTypeId_FiscalYearId]', N'IX_LeaveBalance_TenantId_EmployeeId_LeaveTypeId_FiscalYearId', 'INDEX';

EXEC sp_rename N'[Hrms].[LeaveBalance].[IX_hrmsLeaveBalance_LeaveTypeId]', N'IX_LeaveBalance_LeaveTypeId', 'INDEX';

EXEC sp_rename N'[Hrms].[LeaveBalance].[IX_hrmsLeaveBalance_FiscalYearId]', N'IX_LeaveBalance_FiscalYearId', 'INDEX';

EXEC sp_rename N'[Hrms].[LeaveBalance].[IX_hrmsLeaveBalance_EmployeeId]', N'IX_LeaveBalance_EmployeeId', 'INDEX';

EXEC sp_rename N'[Hrms].[LearningPathStep].[IX_hrmsLearningPathStep_TrainingCourseId]', N'IX_LearningPathStep_TrainingCourseId', 'INDEX';

EXEC sp_rename N'[Hrms].[LearningPathStep].[IX_hrmsLearningPathStep_TenantId_LearningPathId]', N'IX_LearningPathStep_TenantId_LearningPathId', 'INDEX';

EXEC sp_rename N'[Hrms].[LearningPathStep].[IX_hrmsLearningPathStep_LearningPathId]', N'IX_LearningPathStep_LearningPathId', 'INDEX';

EXEC sp_rename N'[Hrms].[LearningPath].[IX_hrmsLearningPath_TenantId_Name]', N'IX_LearningPath_TenantId_Name', 'INDEX';

EXEC sp_rename N'[Hrms].[LearningPath].[IX_hrmsLearningPath_TargetPositionId]', N'IX_LearningPath_TargetPositionId', 'INDEX';

EXEC sp_rename N'[Hrms].[LearningCommunityPost].[IX_hrmsLearningCommunityPost_TenantId_LearningCommunityId_ParentPostId]', N'IX_LearningCommunityPost_TenantId_LearningCommunityId_ParentPostId', 'INDEX';

EXEC sp_rename N'[Hrms].[LearningCommunityPost].[IX_hrmsLearningCommunityPost_LearningCommunityId]', N'IX_LearningCommunityPost_LearningCommunityId', 'INDEX';

EXEC sp_rename N'[Hrms].[LearningCommunityPost].[IX_hrmsLearningCommunityPost_EmployeeId]', N'IX_LearningCommunityPost_EmployeeId', 'INDEX';

EXEC sp_rename N'[Hrms].[LearningCommunityMember].[IX_hrmsLearningCommunityMember_TenantId_LearningCommunityId_EmployeeId]', N'IX_LearningCommunityMember_TenantId_LearningCommunityId_EmployeeId', 'INDEX';

EXEC sp_rename N'[Hrms].[LearningCommunityMember].[IX_hrmsLearningCommunityMember_LearningCommunityId]', N'IX_LearningCommunityMember_LearningCommunityId', 'INDEX';

EXEC sp_rename N'[Hrms].[LearningCommunityMember].[IX_hrmsLearningCommunityMember_EmployeeId]', N'IX_LearningCommunityMember_EmployeeId', 'INDEX';

EXEC sp_rename N'[Hrms].[LearningCommunity].[IX_hrmsLearningCommunity_TrainingCourseId]', N'IX_LearningCommunity_TrainingCourseId', 'INDEX';

EXEC sp_rename N'[Hrms].[LearningCommunity].[IX_hrmsLearningCommunity_TenantId_Name]', N'IX_LearningCommunity_TenantId_Name', 'INDEX';

EXEC sp_rename N'[Hrms].[KnowledgeTransfer].[IX_hrmsKnowledgeTransfer_SuccessionCandidateId]', N'IX_KnowledgeTransfer_SuccessionCandidateId', 'INDEX';

EXEC sp_rename N'[Hrms].[KnowledgeTransfer].[IX_hrmsKnowledgeTransfer_FromEmployeeId]', N'IX_KnowledgeTransfer_FromEmployeeId', 'INDEX';

EXEC sp_rename N'[Hrms].[JobRequisition].[IX_hrmsJobRequisition_WorkLocationId]', N'IX_JobRequisition_WorkLocationId', 'INDEX';

EXEC sp_rename N'[Hrms].[JobRequisition].[IX_hrmsJobRequisition_TenantId_Status]', N'IX_JobRequisition_TenantId_Status', 'INDEX';

EXEC sp_rename N'[Hrms].[JobRequisition].[IX_hrmsJobRequisition_TenantId_RequisitionNumber]', N'IX_JobRequisition_TenantId_RequisitionNumber', 'INDEX';

EXEC sp_rename N'[Hrms].[JobRequisition].[IX_hrmsJobRequisition_SalaryScaleId]', N'IX_JobRequisition_SalaryScaleId', 'INDEX';

EXEC sp_rename N'[Hrms].[JobRequisition].[IX_hrmsJobRequisition_PositionClassId]', N'IX_JobRequisition_PositionClassId', 'INDEX';

EXEC sp_rename N'[Hrms].[JobRequisition].[IX_hrmsJobRequisition_OrganizationUnitId]', N'IX_JobRequisition_OrganizationUnitId', 'INDEX';

EXEC sp_rename N'[Hrms].[JobRequisition].[IX_hrmsJobRequisition_HiringRequestId]', N'IX_JobRequisition_HiringRequestId', 'INDEX';

EXEC sp_rename N'[Hrms].[JobOffer].[IX_hrmsJobOffer_TenantId_Status]', N'IX_JobOffer_TenantId_Status', 'INDEX';

EXEC sp_rename N'[Hrms].[JobOffer].[IX_hrmsJobOffer_TenantId_OfferNumber]', N'IX_JobOffer_TenantId_OfferNumber', 'INDEX';

EXEC sp_rename N'[Hrms].[JobOffer].[IX_hrmsJobOffer_SalaryScaleId]', N'IX_JobOffer_SalaryScaleId', 'INDEX';

EXEC sp_rename N'[Hrms].[JobOffer].[IX_hrmsJobOffer_HiringManagerEmployeeId]', N'IX_JobOffer_HiringManagerEmployeeId', 'INDEX';

EXEC sp_rename N'[Hrms].[JobOffer].[IX_hrmsJobOffer_HiredEmployeeId]', N'IX_JobOffer_HiredEmployeeId', 'INDEX';

EXEC sp_rename N'[Hrms].[JobOffer].[IX_hrmsJobOffer_ApplicationId_CreatedAt]', N'IX_JobOffer_ApplicationId_CreatedAt', 'INDEX';

EXEC sp_rename N'[Hrms].[JobGrade].[IX_hrmsJobGrade_TenantId_Code]', N'IX_JobGrade_TenantId_Code', 'INDEX';

EXEC sp_rename N'[Hrms].[JobCategory].[IX_hrmsJobCategory_TenantId_Code]', N'IX_JobCategory_TenantId_Code', 'INDEX';

EXEC sp_rename N'[Hrms].[JobApplicationStageLog].[IX_hrmsJobApplicationStageLog_ApplicationId]', N'IX_JobApplicationStageLog_ApplicationId', 'INDEX';

EXEC sp_rename N'[Hrms].[JobApplication].[IX_hrmsJobApplication_TenantId_Stage]', N'IX_JobApplication_TenantId_Stage', 'INDEX';

EXEC sp_rename N'[Hrms].[JobApplication].[IX_hrmsJobApplication_TenantId_AppliedAt]', N'IX_JobApplication_TenantId_AppliedAt', 'INDEX';

EXEC sp_rename N'[Hrms].[JobApplication].[IX_hrmsJobApplication_RequisitionId]', N'IX_JobApplication_RequisitionId', 'INDEX';

EXEC sp_rename N'[Hrms].[JobApplication].[IX_hrmsJobApplication_CandidateId_RequisitionId]', N'IX_JobApplication_CandidateId_RequisitionId', 'INDEX';

EXEC sp_rename N'[Hrms].[InterviewPanelist].[IX_hrmsInterviewPanelist_InterviewId_EmployeeId]', N'IX_InterviewPanelist_InterviewId_EmployeeId', 'INDEX';

EXEC sp_rename N'[Hrms].[InterviewPanelist].[IX_hrmsInterviewPanelist_EmployeeId]', N'IX_InterviewPanelist_EmployeeId', 'INDEX';

EXEC sp_rename N'[Hrms].[InterviewFeedback].[IX_hrmsInterviewFeedback_PanelistId_CriterionId]', N'IX_InterviewFeedback_PanelistId_CriterionId', 'INDEX';

EXEC sp_rename N'[Hrms].[InterviewFeedback].[IX_hrmsInterviewFeedback_PanelistId]', N'IX_InterviewFeedback_PanelistId', 'INDEX';

EXEC sp_rename N'[Hrms].[Interview].[IX_hrmsInterview_TenantId_Status]', N'IX_Interview_TenantId_Status', 'INDEX';

EXEC sp_rename N'[Hrms].[Interview].[IX_hrmsInterview_ScheduledStart]', N'IX_Interview_ScheduledStart', 'INDEX';

EXEC sp_rename N'[Hrms].[Interview].[IX_hrmsInterview_ApplicationId]', N'IX_Interview_ApplicationId', 'INDEX';

EXEC sp_rename N'[Hrms].[InsurancePremiumSchedule].[IX_hrmsInsurancePremiumSchedule_Status_DueDate]', N'IX_InsurancePremiumSchedule_Status_DueDate', 'INDEX';

EXEC sp_rename N'[Hrms].[InsurancePremiumSchedule].[IX_hrmsInsurancePremiumSchedule_InsurancePolicyId_Installment]', N'IX_InsurancePremiumSchedule_InsurancePolicyId_Installment', 'INDEX';

EXEC sp_rename N'[Hrms].[InsurancePolicy].[IX_hrmsInsurancePolicy_TenantId_PolicyNumber]', N'IX_InsurancePolicy_TenantId_PolicyNumber', 'INDEX';

EXEC sp_rename N'[Hrms].[InsurancePolicy].[IX_hrmsInsurancePolicy_Status]', N'IX_InsurancePolicy_Status', 'INDEX';

EXEC sp_rename N'[Hrms].[InsuranceClaimAttachment].[IX_hrmsInsuranceClaimAttachment_InsuranceClaimId]', N'IX_InsuranceClaimAttachment_InsuranceClaimId', 'INDEX';

EXEC sp_rename N'[Hrms].[InsuranceClaim].[IX_hrmsInsuranceClaim_TenantId_ClaimNumber]', N'IX_InsuranceClaim_TenantId_ClaimNumber', 'INDEX';

EXEC sp_rename N'[Hrms].[InsuranceClaim].[IX_hrmsInsuranceClaim_Status]', N'IX_InsuranceClaim_Status', 'INDEX';

EXEC sp_rename N'[Hrms].[InsuranceClaim].[IX_hrmsInsuranceClaim_InsurancePolicyId]', N'IX_InsuranceClaim_InsurancePolicyId', 'INDEX';

EXEC sp_rename N'[Hrms].[InsuranceClaim].[IX_hrmsInsuranceClaim_EmployeeId]', N'IX_InsuranceClaim_EmployeeId', 'INDEX';

EXEC sp_rename N'[Hrms].[ImprovementPlan].[IX_hrmsImprovementPlan_TenantId_EmployeeId]', N'IX_ImprovementPlan_TenantId_EmployeeId', 'INDEX';

EXEC sp_rename N'[Hrms].[ImprovementPlan].[IX_hrmsImprovementPlan_EmployeeId]', N'IX_ImprovementPlan_EmployeeId', 'INDEX';

EXEC sp_rename N'[Hrms].[ImprovementPlan].[IX_hrmsImprovementPlan_AppraisalId]', N'IX_ImprovementPlan_AppraisalId', 'INDEX';

EXEC sp_rename N'[Hrms].[Holiday].[IX_hrmsHoliday_TenantId_Date]', N'IX_Holiday_TenantId_Date', 'INDEX';

EXEC sp_rename N'[Hrms].[HiringRequest].[IX_hrmsHiringRequest_TenantId_Status]', N'IX_HiringRequest_TenantId_Status', 'INDEX';

EXEC sp_rename N'[Hrms].[HiringRequest].[IX_hrmsHiringRequest_TenantId_RequestNumber]', N'IX_HiringRequest_TenantId_RequestNumber', 'INDEX';

EXEC sp_rename N'[Hrms].[HiringRequest].[IX_hrmsHiringRequest_PositionClassId]', N'IX_HiringRequest_PositionClassId', 'INDEX';

EXEC sp_rename N'[Hrms].[HiringRequest].[IX_hrmsHiringRequest_OrganizationUnitId]', N'IX_HiringRequest_OrganizationUnitId', 'INDEX';

EXEC sp_rename N'[Hrms].[GrievanceNote].[IX_hrmsGrievanceNote_TenantId_GrievanceId]', N'IX_GrievanceNote_TenantId_GrievanceId', 'INDEX';

EXEC sp_rename N'[Hrms].[GrievanceNote].[IX_hrmsGrievanceNote_GrievanceId]', N'IX_GrievanceNote_GrievanceId', 'INDEX';

EXEC sp_rename N'[Hrms].[Grievance].[IX_hrmsGrievance_TenantId_Status]', N'IX_Grievance_TenantId_Status', 'INDEX';

EXEC sp_rename N'[Hrms].[Grievance].[IX_hrmsGrievance_TenantId_EmployeeId]', N'IX_Grievance_TenantId_EmployeeId', 'INDEX';

EXEC sp_rename N'[Hrms].[Grievance].[IX_hrmsGrievance_TenantId_AssignedToEmployeeId]', N'IX_Grievance_TenantId_AssignedToEmployeeId', 'INDEX';

EXEC sp_rename N'[Hrms].[Grievance].[IX_hrmsGrievance_EmployeeId]', N'IX_Grievance_EmployeeId', 'INDEX';

EXEC sp_rename N'[Hrms].[GoalActionItem].[IX_hrmsGoalActionItem_EmployeeGoalId_SortOrder]', N'IX_GoalActionItem_EmployeeGoalId_SortOrder', 'INDEX';

EXEC sp_rename N'[Hrms].[ExitInterview].[IX_hrmsExitInterview_TerminationId]', N'IX_ExitInterview_TerminationId', 'INDEX';

EXEC sp_rename N'[Hrms].[ExitInterview].[IX_hrmsExitInterview_TenantId_TerminationId]', N'IX_ExitInterview_TenantId_TerminationId', 'INDEX';

EXEC sp_rename N'[Hrms].[EmployeeTrainingCertificate].[IX_hrmsEmployeeTrainingCertificate_TrainingEnrollmentId]', N'IX_EmployeeTrainingCertificate_TrainingEnrollmentId', 'INDEX';

EXEC sp_rename N'[Hrms].[EmployeeTrainingCertificate].[IX_hrmsEmployeeTrainingCertificate_TrainingCourseId]', N'IX_EmployeeTrainingCertificate_TrainingCourseId', 'INDEX';

EXEC sp_rename N'[Hrms].[EmployeeTrainingCertificate].[IX_hrmsEmployeeTrainingCertificate_TenantId_ExpiresOn]', N'IX_EmployeeTrainingCertificate_TenantId_ExpiresOn', 'INDEX';

EXEC sp_rename N'[Hrms].[EmployeeTrainingCertificate].[IX_hrmsEmployeeTrainingCertificate_TenantId_EmployeeId]', N'IX_EmployeeTrainingCertificate_TenantId_EmployeeId', 'INDEX';

EXEC sp_rename N'[Hrms].[EmployeeTrainingCertificate].[IX_hrmsEmployeeTrainingCertificate_TenantId_CertificateNo]', N'IX_EmployeeTrainingCertificate_TenantId_CertificateNo', 'INDEX';

EXEC sp_rename N'[Hrms].[EmployeeTrainingCertificate].[IX_hrmsEmployeeTrainingCertificate_EmployeeId]', N'IX_EmployeeTrainingCertificate_EmployeeId', 'INDEX';

EXEC sp_rename N'[Hrms].[EmployeeTermination].[IX_hrmsEmployeeTermination_Status]', N'IX_EmployeeTermination_Status', 'INDEX';

EXEC sp_rename N'[Hrms].[EmployeeTermination].[IX_hrmsEmployeeTermination_EmployeeId]', N'IX_EmployeeTermination_EmployeeId', 'INDEX';

EXEC sp_rename N'[Hrms].[EmployeeRecognition].[IX_hrmsEmployeeRecognition_TenantId_IsPublic_RecognizedOn]', N'IX_EmployeeRecognition_TenantId_IsPublic_RecognizedOn', 'INDEX';

EXEC sp_rename N'[Hrms].[EmployeeRecognition].[IX_hrmsEmployeeRecognition_TenantId_EmployeeId]', N'IX_EmployeeRecognition_TenantId_EmployeeId', 'INDEX';

EXEC sp_rename N'[Hrms].[EmployeeRecognition].[IX_hrmsEmployeeRecognition_RecognitionBadgeId]', N'IX_EmployeeRecognition_RecognitionBadgeId', 'INDEX';

EXEC sp_rename N'[Hrms].[EmployeeRecognition].[IX_hrmsEmployeeRecognition_EmployeeId]', N'IX_EmployeeRecognition_EmployeeId', 'INDEX';

EXEC sp_rename N'[Hrms].[EmployeeMovement].[IX_hrmsEmployeeMovement_ToSalaryScaleId]', N'IX_EmployeeMovement_ToSalaryScaleId', 'INDEX';

EXEC sp_rename N'[Hrms].[EmployeeMovement].[IX_hrmsEmployeeMovement_Status_EffectiveDate]', N'IX_EmployeeMovement_Status_EffectiveDate', 'INDEX';

EXEC sp_rename N'[Hrms].[EmployeeMovement].[IX_hrmsEmployeeMovement_EmployeeId]', N'IX_EmployeeMovement_EmployeeId', 'INDEX';

EXEC sp_rename N'[Hrms].[EmployeeGuarantee].[IX_hrmsEmployeeGuarantee_TenantId_Status]', N'IX_EmployeeGuarantee_TenantId_Status', 'INDEX';

EXEC sp_rename N'[Hrms].[EmployeeGuarantee].[IX_hrmsEmployeeGuarantee_TenantId_EndDate]', N'IX_EmployeeGuarantee_TenantId_EndDate', 'INDEX';

EXEC sp_rename N'[Hrms].[EmployeeGuarantee].[IX_hrmsEmployeeGuarantee_TenantId_EmployeeId]', N'IX_EmployeeGuarantee_TenantId_EmployeeId', 'INDEX';

EXEC sp_rename N'[Hrms].[EmployeeGuarantee].[IX_hrmsEmployeeGuarantee_EmployeeId]', N'IX_EmployeeGuarantee_EmployeeId', 'INDEX';

EXEC sp_rename N'[Hrms].[EmployeeGoal].[IX_hrmsEmployeeGoal_TenantId_EmployeeId_ReviewCycleId]', N'IX_EmployeeGoal_TenantId_EmployeeId_ReviewCycleId', 'INDEX';

EXEC sp_rename N'[Hrms].[EmployeeGoal].[IX_hrmsEmployeeGoal_ReviewCycleId]', N'IX_EmployeeGoal_ReviewCycleId', 'INDEX';

EXEC sp_rename N'[Hrms].[EmployeeGoal].[IX_hrmsEmployeeGoal_OrganizationalObjectiveId]', N'IX_EmployeeGoal_OrganizationalObjectiveId', 'INDEX';

EXEC sp_rename N'[Hrms].[EmployeeGoal].[IX_hrmsEmployeeGoal_EmployeeId]', N'IX_EmployeeGoal_EmployeeId', 'INDEX';

EXEC sp_rename N'[Hrms].[EmployeeFieldValue].[IX_hrmsEmployeeFieldValue_OwnerType_OwnerId_FieldDefinitionId]', N'IX_EmployeeFieldValue_OwnerType_OwnerId_FieldDefinitionId', 'INDEX';

EXEC sp_rename N'[Hrms].[EmployeeFieldValue].[IX_hrmsEmployeeFieldValue_FieldDefinitionId]', N'IX_EmployeeFieldValue_FieldDefinitionId', 'INDEX';

EXEC sp_rename N'[Hrms].[EmployeeFieldDefinition].[IX_hrmsEmployeeFieldDefinition_TenantId_OwnerType_Name]', N'IX_EmployeeFieldDefinition_TenantId_OwnerType_Name', 'INDEX';

EXEC sp_rename N'[Hrms].[EmployeeExperience].[IX_hrmsEmployeeExperience_PersonId]', N'IX_EmployeeExperience_PersonId', 'INDEX';

EXEC sp_rename N'[Hrms].[EmployeeEducation].[IX_hrmsEmployeeEducation_PersonId]', N'IX_EmployeeEducation_PersonId', 'INDEX';

EXEC sp_rename N'[Hrms].[EmployeeDocument].[IX_hrmsEmployeeDocument_OwnerType_OwnerId]', N'IX_EmployeeDocument_OwnerType_OwnerId', 'INDEX';

EXEC sp_rename N'[Hrms].[EmployeeDocument].[IX_hrmsEmployeeDocument_EmployeeId]', N'IX_EmployeeDocument_EmployeeId', 'INDEX';

EXEC sp_rename N'[Hrms].[EmployeeDependent].[IX_hrmsEmployeeDependent_RelatedEmployeeId]', N'IX_EmployeeDependent_RelatedEmployeeId', 'INDEX';

EXEC sp_rename N'[Hrms].[EmployeeDependent].[IX_hrmsEmployeeDependent_PersonId]', N'IX_EmployeeDependent_PersonId', 'INDEX';

EXEC sp_rename N'[Hrms].[EmployeeCareerPathStepProgress].[IX_hrmsEmployeeCareerPathStepProgress_EmployeeCareerPathId]', N'IX_EmployeeCareerPathStepProgress_EmployeeCareerPathId', 'INDEX';

EXEC sp_rename N'[Hrms].[EmployeeCareerPath].[IX_hrmsEmployeeCareerPath_TenantId_EmployeeId_CareerPathId]', N'IX_EmployeeCareerPath_TenantId_EmployeeId_CareerPathId', 'INDEX';

EXEC sp_rename N'[Hrms].[EmployeeCareerPath].[IX_hrmsEmployeeCareerPath_TenantId_EmployeeId]', N'IX_EmployeeCareerPath_TenantId_EmployeeId', 'INDEX';

EXEC sp_rename N'[Hrms].[EmployeeCareerPath].[IX_hrmsEmployeeCareerPath_TenantId_CareerPathId]', N'IX_EmployeeCareerPath_TenantId_CareerPathId', 'INDEX';

EXEC sp_rename N'[Hrms].[EmployeeCareerPath].[IX_hrmsEmployeeCareerPath_EmployeeId]', N'IX_EmployeeCareerPath_EmployeeId', 'INDEX';

EXEC sp_rename N'[Hrms].[EmployeeCareerPath].[IX_hrmsEmployeeCareerPath_CareerPathId]', N'IX_EmployeeCareerPath_CareerPathId', 'INDEX';

EXEC sp_rename N'[Hrms].[EmployeeBenefitEnrollment].[IX_hrmsEmployeeBenefitEnrollment_EmployeeId]', N'IX_EmployeeBenefitEnrollment_EmployeeId', 'INDEX';

EXEC sp_rename N'[Hrms].[EmployeeBenefitEnrollment].[IX_hrmsEmployeeBenefitEnrollment_BenefitPlanId]', N'IX_EmployeeBenefitEnrollment_BenefitPlanId', 'INDEX';

EXEC sp_rename N'[Hrms].[EmployeeAllowance].[IX_hrmsEmployeeAllowance_EmployeeId]', N'IX_EmployeeAllowance_EmployeeId', 'INDEX';

EXEC sp_rename N'[Hrms].[EmployeeAllowance].[IX_hrmsEmployeeAllowance_AllowanceTypeId]', N'IX_EmployeeAllowance_AllowanceTypeId', 'INDEX';

EXEC sp_rename N'[Hrms].[Employee].[IX_hrmsEmployee_TenantId_PositionId_EmployeeNumber]', N'IX_Employee_TenantId_PositionId_EmployeeNumber', 'INDEX';

EXEC sp_rename N'[Hrms].[Employee].[IX_hrmsEmployee_TenantId_EmployeeNumber]', N'IX_Employee_TenantId_EmployeeNumber', 'INDEX';

EXEC sp_rename N'[Hrms].[Employee].[IX_hrmsEmployee_TenantId_BranchId_EmploymentStatus]', N'IX_Employee_TenantId_BranchId_EmploymentStatus', 'INDEX';

EXEC sp_rename N'[Hrms].[Employee].[IX_hrmsEmployee_SalaryScaleId]', N'IX_Employee_SalaryScaleId', 'INDEX';

EXEC sp_rename N'[Hrms].[Employee].[IX_hrmsEmployee_PositionId]', N'IX_Employee_PositionId', 'INDEX';

EXEC sp_rename N'[Hrms].[Employee].[IX_hrmsEmployee_PersonId]', N'IX_Employee_PersonId', 'INDEX';

EXEC sp_rename N'[Hrms].[Employee].[IX_hrmsEmployee_EmploymentStatus_IsProbation]', N'IX_Employee_EmploymentStatus_IsProbation', 'INDEX';

EXEC sp_rename N'[Hrms].[Employee].[IX_hrmsEmployee_EmploymentStatus]', N'IX_Employee_EmploymentStatus', 'INDEX';

EXEC sp_rename N'[Hrms].[Employee].[IX_hrmsEmployee_DateOfBirth]', N'IX_Employee_DateOfBirth', 'INDEX';

EXEC sp_rename N'[Hrms].[Employee].[IX_hrmsEmployee_BranchId]', N'IX_Employee_BranchId', 'INDEX';

EXEC sp_rename N'[Hrms].[DynamicFormRecord].[IX_hrmsDynamicFormRecord_DynamicFormId_OwnerType_OwnerId_CreatedAt]', N'IX_DynamicFormRecord_DynamicFormId_OwnerType_OwnerId_CreatedAt', 'INDEX';

EXEC sp_rename N'[Hrms].[DynamicFormField].[IX_hrmsDynamicFormField_DynamicFormId_Name]', N'IX_DynamicFormField_DynamicFormId_Name', 'INDEX';

EXEC sp_rename N'[Hrms].[DynamicForm].[IX_hrmsDynamicForm_TenantId_Module_Name]', N'IX_DynamicForm_TenantId_Module_Name', 'INDEX';

EXEC sp_rename N'[Hrms].[DocumentTemplate].[IX_hrmsDocumentTemplate_TenantId_Name]', N'IX_DocumentTemplate_TenantId_Name', 'INDEX';

EXEC sp_rename N'[Hrms].[DisciplinaryMeasure].[IX_hrmsDisciplinaryMeasure_Status]', N'IX_DisciplinaryMeasure_Status', 'INDEX';

EXEC sp_rename N'[Hrms].[DisciplinaryMeasure].[IX_hrmsDisciplinaryMeasure_EmployeeId_Status]', N'IX_DisciplinaryMeasure_EmployeeId_Status', 'INDEX';

EXEC sp_rename N'[Hrms].[DevelopmentPlan].[IX_hrmsDevelopmentPlan_TenantId_EmployeeId]', N'IX_DevelopmentPlan_TenantId_EmployeeId', 'INDEX';

EXEC sp_rename N'[Hrms].[DevelopmentPlan].[IX_hrmsDevelopmentPlan_EmployeeId]', N'IX_DevelopmentPlan_EmployeeId', 'INDEX';

EXEC sp_rename N'[Hrms].[DevelopmentPlan].[IX_hrmsDevelopmentPlan_AppraisalId]', N'IX_DevelopmentPlan_AppraisalId', 'INDEX';

EXEC sp_rename N'[Hrms].[DevelopmentAction].[IX_hrmsDevelopmentAction_DevelopmentPlanId_SortOrder]', N'IX_DevelopmentAction_DevelopmentPlanId_SortOrder', 'INDEX';

EXEC sp_rename N'[Hrms].[DevelopmentAction].[IX_hrmsDevelopmentAction_CompetencyId]', N'IX_DevelopmentAction_CompetencyId', 'INDEX';

EXEC sp_rename N'[Hrms].[CriticalPosition].[IX_hrmsCriticalPosition_TenantId_PositionId]', N'IX_CriticalPosition_TenantId_PositionId', 'INDEX';

EXEC sp_rename N'[Hrms].[CriticalPosition].[IX_hrmsCriticalPosition_TenantId_IsActive]', N'IX_CriticalPosition_TenantId_IsActive', 'INDEX';

EXEC sp_rename N'[Hrms].[CriticalPosition].[IX_hrmsCriticalPosition_PositionId]', N'IX_CriticalPosition_PositionId', 'INDEX';

EXEC sp_rename N'[Hrms].[CriterionEvaluator].[IX_hrmsCriterionEvaluator_EmployeeId]', N'IX_CriterionEvaluator_EmployeeId', 'INDEX';

EXEC sp_rename N'[Hrms].[CriterionEvaluator].[IX_hrmsCriterionEvaluator_CriterionId]', N'IX_CriterionEvaluator_CriterionId', 'INDEX';

EXEC sp_rename N'[Hrms].[CompetencyCategory].[IX_hrmsCompetencyCategory_TenantId_Name]', N'IX_CompetencyCategory_TenantId_Name', 'INDEX';

EXEC sp_rename N'[Hrms].[Competency].[IX_hrmsCompetency_TenantId_Name]', N'IX_Competency_TenantId_Name', 'INDEX';

EXEC sp_rename N'[Hrms].[Competency].[IX_hrmsCompetency_CompetencyCategoryId]', N'IX_Competency_CompetencyCategoryId', 'INDEX';

EXEC sp_rename N'[Hrms].[CompensationRequest].[IX_hrmsCompensationRequest_Status]', N'IX_CompensationRequest_Status', 'INDEX';

EXEC sp_rename N'[Hrms].[CompensationRequest].[IX_hrmsCompensationRequest_EmployeeId]', N'IX_CompensationRequest_EmployeeId', 'INDEX';

EXEC sp_rename N'[Hrms].[CompanyProfile].[IX_hrmsCompanyProfile_TenantId]', N'IX_CompanyProfile_TenantId', 'INDEX';

EXEC sp_rename N'[Hrms].[CompanyAsset].[IX_hrmsCompanyAsset_TenantId_Status]', N'IX_CompanyAsset_TenantId_Status', 'INDEX';

EXEC sp_rename N'[Hrms].[CompanyAsset].[IX_hrmsCompanyAsset_TenantId_AssignedToEmployeeId]', N'IX_CompanyAsset_TenantId_AssignedToEmployeeId', 'INDEX';

EXEC sp_rename N'[Hrms].[CompanyAsset].[IX_hrmsCompanyAsset_AssignedToEmployeeId]', N'IX_CompanyAsset_AssignedToEmployeeId', 'INDEX';

EXEC sp_rename N'[Hrms].[CommunityPostReaction].[IX_hrmsCommunityPostReaction_TenantId_LearningCommunityPostId_EmployeeId]', N'IX_CommunityPostReaction_TenantId_LearningCommunityPostId_EmployeeId', 'INDEX';

EXEC sp_rename N'[Hrms].[CommunityPostReaction].[IX_hrmsCommunityPostReaction_LearningCommunityPostId]', N'IX_CommunityPostReaction_LearningCommunityPostId', 'INDEX';

EXEC sp_rename N'[Hrms].[CommunityPostReaction].[IX_hrmsCommunityPostReaction_EmployeeId]', N'IX_CommunityPostReaction_EmployeeId', 'INDEX';

EXEC sp_rename N'[Hrms].[ClearanceDepartmentApprover].[IX_hrmsClearanceDepartmentApprover_DepartmentId]', N'IX_ClearanceDepartmentApprover_DepartmentId', 'INDEX';

EXEC sp_rename N'[Hrms].[ClearanceDepartmentApprover].[IX_hrmsClearanceDepartmentApprover_ApproverType_ApproverId]', N'IX_ClearanceDepartmentApprover_ApproverType_ApproverId', 'INDEX';

EXEC sp_rename N'[Hrms].[ClearanceDepartment].[IX_hrmsClearanceDepartment_TenantId_Name]', N'IX_ClearanceDepartment_TenantId_Name', 'INDEX';

EXEC sp_rename N'[Hrms].[CareerPathStepCompetency].[IX_hrmsCareerPathStepCompetency_CompetencyId]', N'IX_CareerPathStepCompetency_CompetencyId', 'INDEX';

EXEC sp_rename N'[Hrms].[CareerPathStepCompetency].[IX_hrmsCareerPathStepCompetency_CareerPathStepId_CompetencyId]', N'IX_CareerPathStepCompetency_CareerPathStepId_CompetencyId', 'INDEX';

EXEC sp_rename N'[Hrms].[CareerPathStep].[IX_hrmsCareerPathStep_PositionClassId]', N'IX_CareerPathStep_PositionClassId', 'INDEX';

EXEC sp_rename N'[Hrms].[CareerPathStep].[IX_hrmsCareerPathStep_JobGradeId]', N'IX_CareerPathStep_JobGradeId', 'INDEX';

EXEC sp_rename N'[Hrms].[CareerPathStep].[IX_hrmsCareerPathStep_CareerPathId_StepOrder]', N'IX_CareerPathStep_CareerPathId_StepOrder', 'INDEX';

EXEC sp_rename N'[Hrms].[CareerPathChangeRequest].[IX_hrmsCareerPathChangeRequest_TenantId_EmployeeId_Status]', N'IX_CareerPathChangeRequest_TenantId_EmployeeId_Status', 'INDEX';

EXEC sp_rename N'[Hrms].[CareerPathChangeRequest].[IX_hrmsCareerPathChangeRequest_RequestedCareerPathId]', N'IX_CareerPathChangeRequest_RequestedCareerPathId', 'INDEX';

EXEC sp_rename N'[Hrms].[CareerPathChangeRequest].[IX_hrmsCareerPathChangeRequest_EmployeeId]', N'IX_CareerPathChangeRequest_EmployeeId', 'INDEX';

EXEC sp_rename N'[Hrms].[CareerPathChangeRequest].[IX_hrmsCareerPathChangeRequest_CurrentCareerPathId]', N'IX_CareerPathChangeRequest_CurrentCareerPathId', 'INDEX';

EXEC sp_rename N'[Hrms].[CareerPath].[IX_hrmsCareerPath_TenantId_Code]', N'IX_CareerPath_TenantId_Code', 'INDEX';

EXEC sp_rename N'[Hrms].[CandidateDocument].[IX_hrmsCandidateDocument_CandidateId_DocumentType]', N'IX_CandidateDocument_CandidateId_DocumentType', 'INDEX';

EXEC sp_rename N'[Hrms].[Candidate].[IX_hrmsCandidate_TenantId_IsInTalentPool]', N'IX_Candidate_TenantId_IsInTalentPool', 'INDEX';

EXEC sp_rename N'[Hrms].[Candidate].[IX_hrmsCandidate_TenantId_CandidateNumber]', N'IX_Candidate_TenantId_CandidateNumber', 'INDEX';

EXEC sp_rename N'[Hrms].[Candidate].[IX_hrmsCandidate_PersonId]', N'IX_Candidate_PersonId', 'INDEX';

EXEC sp_rename N'[Hrms].[Candidate].[IX_hrmsCandidate_InternalEmployeeId]', N'IX_Candidate_InternalEmployeeId', 'INDEX';

EXEC sp_rename N'[Hrms].[Candidate].[IX_hrmsCandidate_HiredEmployeeId]', N'IX_Candidate_HiredEmployeeId', 'INDEX';

EXEC sp_rename N'[Hrms].[Candidate].[IX_hrmsCandidate_Email]', N'IX_Candidate_Email', 'INDEX';

EXEC sp_rename N'[Hrms].[CalibrationSession].[IX_hrmsCalibrationSession_TenantId_ReviewCycleId]', N'IX_CalibrationSession_TenantId_ReviewCycleId', 'INDEX';

EXEC sp_rename N'[Hrms].[CalibrationSession].[IX_hrmsCalibrationSession_ReviewCycleId]', N'IX_CalibrationSession_ReviewCycleId', 'INDEX';

EXEC sp_rename N'[Hrms].[CalibrationSession].[IX_hrmsCalibrationSession_OrganizationUnitId]', N'IX_CalibrationSession_OrganizationUnitId', 'INDEX';

EXEC sp_rename N'[Hrms].[CalibrationItem].[IX_hrmsCalibrationItem_CalibrationSessionId]', N'IX_CalibrationItem_CalibrationSessionId', 'INDEX';

EXEC sp_rename N'[Hrms].[CalibrationItem].[IX_hrmsCalibrationItem_AppraisalId]', N'IX_CalibrationItem_AppraisalId', 'INDEX';

EXEC sp_rename N'[Hrms].[Branch].[IX_hrmsBranch_TenantId_Code]', N'IX_Branch_TenantId_Code', 'INDEX';

EXEC sp_rename N'[Hrms].[Branch].[IX_hrmsBranch_ParentId]', N'IX_Branch_ParentId', 'INDEX';

EXEC sp_rename N'[Hrms].[BenefitPlan].[IX_hrmsBenefitPlan_TenantId_Name]', N'IX_BenefitPlan_TenantId_Name', 'INDEX';

EXEC sp_rename N'[Hrms].[AwardCategory].[IX_hrmsAwardCategory_TenantId_Name]', N'IX_AwardCategory_TenantId_Name', 'INDEX';

EXEC sp_rename N'[Hrms].[AuditLog].[IX_hrmsAuditLog_EntityType_EntityId]', N'IX_AuditLog_EntityType_EntityId', 'INDEX';

EXEC sp_rename N'[Hrms].[AuditLog].[IX_hrmsAuditLog_CreatedAt]', N'IX_AuditLog_CreatedAt', 'INDEX';

EXEC sp_rename N'[Hrms].[AuditLog].[IX_hrmsAuditLog_BranchId]', N'IX_AuditLog_BranchId', 'INDEX';

EXEC sp_rename N'[Hrms].[AuditLog].[IX_hrmsAuditLog_Action]', N'IX_AuditLog_Action', 'INDEX';

EXEC sp_rename N'[Hrms].[AppraisalTemplate].[IX_hrmsAppraisalTemplate_TenantId_Name]', N'IX_AppraisalTemplate_TenantId_Name', 'INDEX';

EXEC sp_rename N'[Hrms].[AppraisalPeerReview].[IX_hrmsAppraisalPeerReview_PeerEmployeeId]', N'IX_AppraisalPeerReview_PeerEmployeeId', 'INDEX';

EXEC sp_rename N'[Hrms].[AppraisalPeerReview].[IX_hrmsAppraisalPeerReview_AppraisalId_PeerEmployeeId]', N'IX_AppraisalPeerReview_AppraisalId_PeerEmployeeId', 'INDEX';

EXEC sp_rename N'[Hrms].[AppraisalGoal].[IX_hrmsAppraisalGoal_AppraisalId_SortOrder]', N'IX_AppraisalGoal_AppraisalId_SortOrder', 'INDEX';

EXEC sp_rename N'[Hrms].[AppraisalCompetency].[IX_hrmsAppraisalCompetency_AppraisalId_SortOrder]', N'IX_AppraisalCompetency_AppraisalId_SortOrder', 'INDEX';

EXEC sp_rename N'[Hrms].[AppraisalAppeal].[IX_hrmsAppraisalAppeal_TenantId_Status]', N'IX_AppraisalAppeal_TenantId_Status', 'INDEX';

EXEC sp_rename N'[Hrms].[AppraisalAppeal].[IX_hrmsAppraisalAppeal_EmployeeId]', N'IX_AppraisalAppeal_EmployeeId', 'INDEX';

EXEC sp_rename N'[Hrms].[AppraisalAppeal].[IX_hrmsAppraisalAppeal_AppraisalId]', N'IX_AppraisalAppeal_AppraisalId', 'INDEX';

EXEC sp_rename N'[Hrms].[Appraisal].[IX_hrmsAppraisal_TenantId_ReviewCycleId_Stage]', N'IX_Appraisal_TenantId_ReviewCycleId_Stage', 'INDEX';

EXEC sp_rename N'[Hrms].[Appraisal].[IX_hrmsAppraisal_TenantId_EmployeeId_ReviewCycleId]', N'IX_Appraisal_TenantId_EmployeeId_ReviewCycleId', 'INDEX';

EXEC sp_rename N'[Hrms].[Appraisal].[IX_hrmsAppraisal_ReviewCycleId]', N'IX_Appraisal_ReviewCycleId', 'INDEX';

EXEC sp_rename N'[Hrms].[Appraisal].[IX_hrmsAppraisal_EmployeeId]', N'IX_Appraisal_EmployeeId', 'INDEX';

EXEC sp_rename N'[Hrms].[ApplicationCriterionScore].[IX_hrmsApplicationCriterionScore_ApplicationId_CriterionId]', N'IX_ApplicationCriterionScore_ApplicationId_CriterionId', 'INDEX';

EXEC sp_rename N'[Hrms].[AnnualLeaveSetting].[IX_hrmsAnnualLeaveSetting_TenantId_FiscalYearId]', N'IX_AnnualLeaveSetting_TenantId_FiscalYearId', 'INDEX';

EXEC sp_rename N'[Hrms].[AnnualLeaveSetting].[IX_hrmsAnnualLeaveSetting_FiscalYearId]', N'IX_AnnualLeaveSetting_FiscalYearId', 'INDEX';

EXEC sp_rename N'[Hrms].[AnnualLeaveHeader].[IX_hrmsAnnualLeaveHeader_EmployeeId_Status]', N'IX_AnnualLeaveHeader_EmployeeId_Status', 'INDEX';

EXEC sp_rename N'[Hrms].[AnnualLeaveHeader].[IX_hrmsAnnualLeaveHeader_EmployeeId]', N'IX_AnnualLeaveHeader_EmployeeId', 'INDEX';

EXEC sp_rename N'[Hrms].[AnnualLeaveHeader].[IX_hrmsAnnualLeaveHeader_AnnualLeaveLedgerId]', N'IX_AnnualLeaveHeader_AnnualLeaveLedgerId', 'INDEX';

EXEC sp_rename N'[Hrms].[AnnualLeaveDetail].[IX_hrmsAnnualLeaveDetail_AnnualLeaveHeaderId_StartDate_EndDate]', N'IX_AnnualLeaveDetail_AnnualLeaveHeaderId_StartDate_EndDate', 'INDEX';

EXEC sp_rename N'[Hrms].[AnnualLeaveDetail].[IX_hrmsAnnualLeaveDetail_AnnualLeaveHeaderId]', N'IX_AnnualLeaveDetail_AnnualLeaveHeaderId', 'INDEX';

EXEC sp_rename N'[Hrms].[Announcement].[IX_hrmsAnnouncement_TenantId_IsActive_PublishFrom]', N'IX_Announcement_TenantId_IsActive_PublishFrom', 'INDEX';

EXEC sp_rename N'[Hrms].[Announcement].[IX_hrmsAnnouncement_OrganizationUnitId]', N'IX_Announcement_OrganizationUnitId', 'INDEX';

EXEC sp_rename N'[Hrms].[Announcement].[IX_hrmsAnnouncement_BranchId]', N'IX_Announcement_BranchId', 'INDEX';

EXEC sp_rename N'[Hrms].[AllowanceType].[IX_hrmsAllowanceType_TenantId_Name]', N'IX_AllowanceType_TenantId_Name', 'INDEX';

EXEC sp_rename N'[Hrms].[Achievement].[IX_hrmsAchievement_TenantId_EmployeeId]', N'IX_Achievement_TenantId_EmployeeId', 'INDEX';

EXEC sp_rename N'[Hrms].[Achievement].[IX_hrmsAchievement_EmployeeId]', N'IX_Achievement_EmployeeId', 'INDEX';

EXEC sp_rename N'[Hrms].[Achievement].[IX_hrmsAchievement_AppraisalId]', N'IX_Achievement_AppraisalId', 'INDEX';

EXEC sp_rename N'[Core].[Subsystem].[IX_coreSubsystem_TenantId_Name]', N'IX_Subsystem_TenantId_Name', 'INDEX';

EXEC sp_rename N'[Core].[SalaryScale].[IX_coreSalaryScale_TenantId_JobGradeId_StepId]', N'IX_SalaryScale_TenantId_JobGradeId_StepId', 'INDEX';

EXEC sp_rename N'[Core].[SalaryScale].[IX_coreSalaryScale_StepId]', N'IX_SalaryScale_StepId', 'INDEX';

EXEC sp_rename N'[Core].[SalaryScale].[IX_coreSalaryScale_JobGradeId]', N'IX_SalaryScale_JobGradeId', 'INDEX';

EXEC sp_rename N'[Core].[Person].[IX_CorePerson_FirstName_FatherName_GrandFatherName]', N'IX_Person_FirstName_FatherName_GrandFatherName', 'INDEX';

EXEC sp_rename N'[Core].[Operation].[IX_coreOperation_ModuleId]', N'IX_Operation_ModuleId', 'INDEX';

EXEC sp_rename N'[Core].[Module].[IX_coreModule_SubsystemId]', N'IX_Module_SubsystemId', 'INDEX';

ALTER TABLE [Core].[Step] ADD CONSTRAINT [PK_Step] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[WorkWeekConfiguration] ADD CONSTRAINT [PK_WorkWeekConfiguration] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[WorkLocation] ADD CONSTRAINT [PK_WorkLocation] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[WorkforcePlanLine] ADD CONSTRAINT [PK_WorkforcePlanLine] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[WorkforcePlan] ADD CONSTRAINT [PK_WorkforcePlan] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[WorkflowStepApprover] ADD CONSTRAINT [PK_WorkflowStepApprover] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[WorkflowStep] ADD CONSTRAINT [PK_WorkflowStep] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[WorkflowInstance] ADD CONSTRAINT [PK_WorkflowInstance] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[WorkflowDefinition] ADD CONSTRAINT [PK_WorkflowDefinition] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[WorkflowActionLog] ADD CONSTRAINT [PK_WorkflowActionLog] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[TripRequest] ADD CONSTRAINT [PK_TripRequest] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[TripExpense] ADD CONSTRAINT [PK_TripExpense] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[TripBudget] ADD CONSTRAINT [PK_TripBudget] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[TrainingSession] ADD CONSTRAINT [PK_TrainingSession] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[TrainingProviderPayment] ADD CONSTRAINT [PK_TrainingProviderPayment] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[TrainingNeed] ADD CONSTRAINT [PK_TrainingNeed] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[TrainingEnrollment] ADD CONSTRAINT [PK_TrainingEnrollment] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[TrainingCourse] ADD CONSTRAINT [PK_TrainingCourse] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[TrainingCategory] ADD CONSTRAINT [PK_TrainingCategory] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[TrainingBudget] ADD CONSTRAINT [PK_TrainingBudget] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[TerminationSettlement] ADD CONSTRAINT [PK_TerminationSettlement] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[TerminationClearance] ADD CONSTRAINT [PK_TerminationClearance] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[TerminationAssetRecovery] ADD CONSTRAINT [PK_TerminationAssetRecovery] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[TaxBracket] ADD CONSTRAINT [PK_TaxBracket] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[TalentReview] ADD CONSTRAINT [PK_TalentReview] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[TalentRating] ADD CONSTRAINT [PK_TalentRating] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[TalentAssessment] ADD CONSTRAINT [PK_TalentAssessment] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[SurveyResponse] ADD CONSTRAINT [PK_SurveyResponse] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[SurveyCompletion] ADD CONSTRAINT [PK_SurveyCompletion] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[Survey] ADD CONSTRAINT [PK_Survey] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[Suggestion] ADD CONSTRAINT [PK_Suggestion] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[SuccessionPlan] ADD CONSTRAINT [PK_SuccessionPlan] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[SuccessionDevelopmentAction] ADD CONSTRAINT [PK_SuccessionDevelopmentAction] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[SuccessionCandidate] ADD CONSTRAINT [PK_SuccessionCandidate] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[SettlementLine] ADD CONSTRAINT [PK_SettlementLine] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[SalaryRevisionLine] ADD CONSTRAINT [PK_SalaryRevisionLine] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[SalaryRevisionBand] ADD CONSTRAINT [PK_SalaryRevisionBand] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[SalaryRevision] ADD CONSTRAINT [PK_SalaryRevision] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[RewardPointsTransaction] ADD CONSTRAINT [PK_RewardPointsTransaction] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[RewardNomination] ADD CONSTRAINT [PK_RewardNomination] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[RewardDisbursement] ADD CONSTRAINT [PK_RewardDisbursement] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[ReviewCycle] ADD CONSTRAINT [PK_ReviewCycle] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[RequisitionScreeningCriterion] ADD CONSTRAINT [PK_RequisitionScreeningCriterion] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[ReportScheduleRecipient] ADD CONSTRAINT [PK_ReportScheduleRecipient] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[ReportScheduleFieldValue] ADD CONSTRAINT [PK_ReportScheduleFieldValue] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[ReportScheduleFieldOutput] ADD CONSTRAINT [PK_ReportScheduleFieldOutput] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[ReportSchedule] ADD CONSTRAINT [PK_ReportSchedule] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[ReportSavedFilter] ADD CONSTRAINT [PK_ReportSavedFilter] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[ReportRunRecipient] ADD CONSTRAINT [PK_ReportRunRecipient] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[ReportRun] ADD CONSTRAINT [PK_ReportRun] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[ReportRestriction] ADD CONSTRAINT [PK_ReportRestriction] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[ReportFieldOutput] ADD CONSTRAINT [PK_ReportFieldOutput] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[ReportField] ADD CONSTRAINT [PK_ReportField] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[Report] ADD CONSTRAINT [PK_Report] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[RecognitionProgram] ADD CONSTRAINT [PK_RecognitionProgram] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[RecognitionBadge] ADD CONSTRAINT [PK_RecognitionBadge] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[RatingScaleLevel] ADD CONSTRAINT [PK_RatingScaleLevel] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[RatingScale] ADD CONSTRAINT [PK_RatingScale] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[ProfileChangeRequest] ADD CONSTRAINT [PK_ProfileChangeRequest] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[PositionCompetency] ADD CONSTRAINT [PK_PositionCompetency] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[PositionClass] ADD CONSTRAINT [PK_PositionClass] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[Position] ADD CONSTRAINT [PK_Position] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[PipObjective] ADD CONSTRAINT [PK_PipObjective] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[PerformanceHistory] ADD CONSTRAINT [PK_PerformanceHistory] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[PerDiemRate] ADD CONSTRAINT [PK_PerDiemRate] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[OtherLeaveSetting] ADD CONSTRAINT [PK_OtherLeaveSetting] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[OtherLeaveDetail] ADD CONSTRAINT [PK_OtherLeaveDetail] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[OtherLeave] ADD CONSTRAINT [PK_OtherLeave] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[OrganizationUnit] ADD CONSTRAINT [PK_OrganizationUnit] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[OrganizationalObjective] ADD CONSTRAINT [PK_OrganizationalObjective] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[OfferLetterTemplate] ADD CONSTRAINT [PK_OfferLetterTemplate] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[NumberSequence] ADD CONSTRAINT [PK_NumberSequence] PRIMARY KEY ([TenantId], [Key]);

ALTER TABLE [Hrms].[Mentorship] ADD CONSTRAINT [PK_Mentorship] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[MedicalServiceContract] ADD CONSTRAINT [PK_MedicalServiceContract] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[MedicalProvider] ADD CONSTRAINT [PK_MedicalProvider] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[MedicalPlan] ADD CONSTRAINT [PK_MedicalPlan] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[MedicalEnrollment] ADD CONSTRAINT [PK_MedicalEnrollment] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[MedicalClaimAttachment] ADD CONSTRAINT [PK_MedicalClaimAttachment] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[MedicalClaim] ADD CONSTRAINT [PK_MedicalClaim] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[MedicalBeneficiary] ADD CONSTRAINT [PK_MedicalBeneficiary] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[LoanType] ADD CONSTRAINT [PK_LoanType] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[LoanRepaymentSchedule] ADD CONSTRAINT [PK_LoanRepaymentSchedule] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[LoanGuarantor] ADD CONSTRAINT [PK_LoanGuarantor] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[Loan] ADD CONSTRAINT [PK_Loan] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[LeaveType] ADD CONSTRAINT [PK_LeaveType] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[LeaveRequestLine] ADD CONSTRAINT [PK_LeaveRequestLine] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[LeaveRequest] ADD CONSTRAINT [PK_LeaveRequest] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[LeaveBalanceTransaction] ADD CONSTRAINT [PK_LeaveBalanceTransaction] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[LeaveBalance] ADD CONSTRAINT [PK_LeaveBalance] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[LearningPathStep] ADD CONSTRAINT [PK_LearningPathStep] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[LearningPath] ADD CONSTRAINT [PK_LearningPath] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[LearningCommunityPost] ADD CONSTRAINT [PK_LearningCommunityPost] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[LearningCommunityMember] ADD CONSTRAINT [PK_LearningCommunityMember] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[LearningCommunity] ADD CONSTRAINT [PK_LearningCommunity] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[KnowledgeTransfer] ADD CONSTRAINT [PK_KnowledgeTransfer] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[JobRequisition] ADD CONSTRAINT [PK_JobRequisition] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[JobOffer] ADD CONSTRAINT [PK_JobOffer] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[JobGrade] ADD CONSTRAINT [PK_JobGrade] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[JobCategory] ADD CONSTRAINT [PK_JobCategory] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[JobApplicationStageLog] ADD CONSTRAINT [PK_JobApplicationStageLog] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[JobApplication] ADD CONSTRAINT [PK_JobApplication] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[InterviewPanelist] ADD CONSTRAINT [PK_InterviewPanelist] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[InterviewFeedback] ADD CONSTRAINT [PK_InterviewFeedback] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[Interview] ADD CONSTRAINT [PK_Interview] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[InsurancePremiumSchedule] ADD CONSTRAINT [PK_InsurancePremiumSchedule] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[InsurancePolicy] ADD CONSTRAINT [PK_InsurancePolicy] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[InsuranceClaimAttachment] ADD CONSTRAINT [PK_InsuranceClaimAttachment] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[InsuranceClaim] ADD CONSTRAINT [PK_InsuranceClaim] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[ImprovementPlan] ADD CONSTRAINT [PK_ImprovementPlan] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[Holiday] ADD CONSTRAINT [PK_Holiday] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[HiringRequest] ADD CONSTRAINT [PK_HiringRequest] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[GrievanceNote] ADD CONSTRAINT [PK_GrievanceNote] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[Grievance] ADD CONSTRAINT [PK_Grievance] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[GoalActionItem] ADD CONSTRAINT [PK_GoalActionItem] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[ExitQuestionnaire] ADD CONSTRAINT [PK_ExitQuestionnaire] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[ExitInterview] ADD CONSTRAINT [PK_ExitInterview] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[EmployeeTrainingCertificate] ADD CONSTRAINT [PK_EmployeeTrainingCertificate] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[EmployeeTermination] ADD CONSTRAINT [PK_EmployeeTermination] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[EmployeeRecognition] ADD CONSTRAINT [PK_EmployeeRecognition] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[EmployeeMovement] ADD CONSTRAINT [PK_EmployeeMovement] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[EmployeeGuarantee] ADD CONSTRAINT [PK_EmployeeGuarantee] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[EmployeeGoal] ADD CONSTRAINT [PK_EmployeeGoal] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[EmployeeFieldValue] ADD CONSTRAINT [PK_EmployeeFieldValue] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[EmployeeFieldDefinition] ADD CONSTRAINT [PK_EmployeeFieldDefinition] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[EmployeeExperience] ADD CONSTRAINT [PK_EmployeeExperience] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[EmployeeEducation] ADD CONSTRAINT [PK_EmployeeEducation] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[EmployeeDocument] ADD CONSTRAINT [PK_EmployeeDocument] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[EmployeeDependent] ADD CONSTRAINT [PK_EmployeeDependent] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[EmployeeCareerPathStepProgress] ADD CONSTRAINT [PK_EmployeeCareerPathStepProgress] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[EmployeeCareerPath] ADD CONSTRAINT [PK_EmployeeCareerPath] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[EmployeeBenefitEnrollment] ADD CONSTRAINT [PK_EmployeeBenefitEnrollment] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[EmployeeAllowance] ADD CONSTRAINT [PK_EmployeeAllowance] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[Employee] ADD CONSTRAINT [PK_Employee] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[DynamicFormRecord] ADD CONSTRAINT [PK_DynamicFormRecord] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[DynamicFormField] ADD CONSTRAINT [PK_DynamicFormField] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[DynamicForm] ADD CONSTRAINT [PK_DynamicForm] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[DocumentTemplate] ADD CONSTRAINT [PK_DocumentTemplate] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[DisciplinaryMeasure] ADD CONSTRAINT [PK_DisciplinaryMeasure] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[DevelopmentPlan] ADD CONSTRAINT [PK_DevelopmentPlan] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[DevelopmentAction] ADD CONSTRAINT [PK_DevelopmentAction] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[CriticalPosition] ADD CONSTRAINT [PK_CriticalPosition] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[CriterionEvaluator] ADD CONSTRAINT [PK_CriterionEvaluator] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[CompetencyCategory] ADD CONSTRAINT [PK_CompetencyCategory] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[Competency] ADD CONSTRAINT [PK_Competency] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[CompensationRequest] ADD CONSTRAINT [PK_CompensationRequest] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[CompanyProfile] ADD CONSTRAINT [PK_CompanyProfile] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[CompanyAsset] ADD CONSTRAINT [PK_CompanyAsset] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[CommunityPostReaction] ADD CONSTRAINT [PK_CommunityPostReaction] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[ClearanceDepartmentApprover] ADD CONSTRAINT [PK_ClearanceDepartmentApprover] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[ClearanceDepartment] ADD CONSTRAINT [PK_ClearanceDepartment] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[CareerPathStepCompetency] ADD CONSTRAINT [PK_CareerPathStepCompetency] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[CareerPathStep] ADD CONSTRAINT [PK_CareerPathStep] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[CareerPathChangeRequest] ADD CONSTRAINT [PK_CareerPathChangeRequest] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[CareerPath] ADD CONSTRAINT [PK_CareerPath] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[CandidateDocument] ADD CONSTRAINT [PK_CandidateDocument] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[Candidate] ADD CONSTRAINT [PK_Candidate] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[CalibrationSession] ADD CONSTRAINT [PK_CalibrationSession] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[CalibrationItem] ADD CONSTRAINT [PK_CalibrationItem] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[Branch] ADD CONSTRAINT [PK_Branch] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[BenefitPlan] ADD CONSTRAINT [PK_BenefitPlan] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[AwardCategory] ADD CONSTRAINT [PK_AwardCategory] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[AuditLog] ADD CONSTRAINT [PK_AuditLog] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[AppraisalTemplate] ADD CONSTRAINT [PK_AppraisalTemplate] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[AppraisalPeerReview] ADD CONSTRAINT [PK_AppraisalPeerReview] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[AppraisalGoal] ADD CONSTRAINT [PK_AppraisalGoal] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[AppraisalCompetency] ADD CONSTRAINT [PK_AppraisalCompetency] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[AppraisalAppeal] ADD CONSTRAINT [PK_AppraisalAppeal] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[Appraisal] ADD CONSTRAINT [PK_Appraisal] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[ApplicationCriterionScore] ADD CONSTRAINT [PK_ApplicationCriterionScore] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[AnnualLeaveSetting] ADD CONSTRAINT [PK_AnnualLeaveSetting] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[AnnualLeaveHeader] ADD CONSTRAINT [PK_AnnualLeaveHeader] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[AnnualLeaveDetail] ADD CONSTRAINT [PK_AnnualLeaveDetail] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[Announcement] ADD CONSTRAINT [PK_Announcement] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[AllowanceType] ADD CONSTRAINT [PK_AllowanceType] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[Achievement] ADD CONSTRAINT [PK_Achievement] PRIMARY KEY ([Id]);

ALTER TABLE [Core].[Subsystem] ADD CONSTRAINT [PK_Subsystem] PRIMARY KEY ([Id]);

ALTER TABLE [Core].[SalaryScale] ADD CONSTRAINT [PK_SalaryScale] PRIMARY KEY ([Id]);

ALTER TABLE [Core].[Person] ADD CONSTRAINT [PK_Person] PRIMARY KEY ([Id]);

ALTER TABLE [Core].[Operation] ADD CONSTRAINT [PK_Operation] PRIMARY KEY ([Id]);

ALTER TABLE [Core].[Module] ADD CONSTRAINT [PK_Module] PRIMARY KEY ([Id]);

ALTER TABLE [Hrms].[Achievement] ADD CONSTRAINT [FK_Achievement_Appraisal_AppraisalId] FOREIGN KEY ([AppraisalId]) REFERENCES [Hrms].[Appraisal] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[Achievement] ADD CONSTRAINT [FK_Achievement_Employee_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Hrms].[Employee] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[Announcement] ADD CONSTRAINT [FK_Announcement_Branch_BranchId] FOREIGN KEY ([BranchId]) REFERENCES [Hrms].[Branch] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[Announcement] ADD CONSTRAINT [FK_Announcement_OrganizationUnit_OrganizationUnitId] FOREIGN KEY ([OrganizationUnitId]) REFERENCES [Hrms].[OrganizationUnit] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[AnnualLeaveDetail] ADD CONSTRAINT [FK_AnnualLeaveDetail_AnnualLeaveHeader_AnnualLeaveHeaderId] FOREIGN KEY ([AnnualLeaveHeaderId]) REFERENCES [Hrms].[AnnualLeaveHeader] ([Id]) ON DELETE CASCADE;

ALTER TABLE [Hrms].[AnnualLeaveHeader] ADD CONSTRAINT [FK_AnnualLeaveHeader_Employee_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Hrms].[Employee] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[AnnualLeaveHeader] ADD CONSTRAINT [FK_AnnualLeaveHeader_LeaveBalance_AnnualLeaveLedgerId] FOREIGN KEY ([AnnualLeaveLedgerId]) REFERENCES [Hrms].[LeaveBalance] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[AnnualLeaveSetting] ADD CONSTRAINT [FK_AnnualLeaveSetting_FiscalYear_FiscalYearId] FOREIGN KEY ([FiscalYearId]) REFERENCES [Core].[FiscalYear] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[ApplicationCriterionScore] ADD CONSTRAINT [FK_ApplicationCriterionScore_JobApplication_ApplicationId] FOREIGN KEY ([ApplicationId]) REFERENCES [Hrms].[JobApplication] ([Id]) ON DELETE CASCADE;

ALTER TABLE [Hrms].[Appraisal] ADD CONSTRAINT [FK_Appraisal_Employee_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Hrms].[Employee] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[Appraisal] ADD CONSTRAINT [FK_Appraisal_ReviewCycle_ReviewCycleId] FOREIGN KEY ([ReviewCycleId]) REFERENCES [Hrms].[ReviewCycle] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[AppraisalAppeal] ADD CONSTRAINT [FK_AppraisalAppeal_Appraisal_AppraisalId] FOREIGN KEY ([AppraisalId]) REFERENCES [Hrms].[Appraisal] ([Id]) ON DELETE CASCADE;

ALTER TABLE [Hrms].[AppraisalAppeal] ADD CONSTRAINT [FK_AppraisalAppeal_Employee_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Hrms].[Employee] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[AppraisalCompetency] ADD CONSTRAINT [FK_AppraisalCompetency_Appraisal_AppraisalId] FOREIGN KEY ([AppraisalId]) REFERENCES [Hrms].[Appraisal] ([Id]) ON DELETE CASCADE;

ALTER TABLE [Hrms].[AppraisalGoal] ADD CONSTRAINT [FK_AppraisalGoal_Appraisal_AppraisalId] FOREIGN KEY ([AppraisalId]) REFERENCES [Hrms].[Appraisal] ([Id]) ON DELETE CASCADE;

ALTER TABLE [Hrms].[AppraisalPeerReview] ADD CONSTRAINT [FK_AppraisalPeerReview_Appraisal_AppraisalId] FOREIGN KEY ([AppraisalId]) REFERENCES [Hrms].[Appraisal] ([Id]) ON DELETE CASCADE;

ALTER TABLE [Hrms].[AppraisalPeerReview] ADD CONSTRAINT [FK_AppraisalPeerReview_Employee_PeerEmployeeId] FOREIGN KEY ([PeerEmployeeId]) REFERENCES [Hrms].[Employee] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[Branch] ADD CONSTRAINT [FK_Branch_Branch_ParentId] FOREIGN KEY ([ParentId]) REFERENCES [Hrms].[Branch] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[CalibrationItem] ADD CONSTRAINT [FK_CalibrationItem_Appraisal_AppraisalId] FOREIGN KEY ([AppraisalId]) REFERENCES [Hrms].[Appraisal] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[CalibrationItem] ADD CONSTRAINT [FK_CalibrationItem_CalibrationSession_CalibrationSessionId] FOREIGN KEY ([CalibrationSessionId]) REFERENCES [Hrms].[CalibrationSession] ([Id]) ON DELETE CASCADE;

ALTER TABLE [Hrms].[CalibrationSession] ADD CONSTRAINT [FK_CalibrationSession_OrganizationUnit_OrganizationUnitId] FOREIGN KEY ([OrganizationUnitId]) REFERENCES [Hrms].[OrganizationUnit] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[CalibrationSession] ADD CONSTRAINT [FK_CalibrationSession_ReviewCycle_ReviewCycleId] FOREIGN KEY ([ReviewCycleId]) REFERENCES [Hrms].[ReviewCycle] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[Candidate] ADD CONSTRAINT [FK_Candidate_Employee_InternalEmployeeId] FOREIGN KEY ([InternalEmployeeId]) REFERENCES [Hrms].[Employee] ([Id]) ON DELETE SET NULL;

ALTER TABLE [Hrms].[Candidate] ADD CONSTRAINT [FK_Candidate_Person_PersonId] FOREIGN KEY ([PersonId]) REFERENCES [Core].[Person] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[CandidateDocument] ADD CONSTRAINT [FK_CandidateDocument_Candidate_CandidateId] FOREIGN KEY ([CandidateId]) REFERENCES [Hrms].[Candidate] ([Id]) ON DELETE CASCADE;

ALTER TABLE [Hrms].[CareerPathChangeRequest] ADD CONSTRAINT [FK_CareerPathChangeRequest_CareerPath_CurrentCareerPathId] FOREIGN KEY ([CurrentCareerPathId]) REFERENCES [Hrms].[CareerPath] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[CareerPathChangeRequest] ADD CONSTRAINT [FK_CareerPathChangeRequest_CareerPath_RequestedCareerPathId] FOREIGN KEY ([RequestedCareerPathId]) REFERENCES [Hrms].[CareerPath] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[CareerPathChangeRequest] ADD CONSTRAINT [FK_CareerPathChangeRequest_Employee_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Hrms].[Employee] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[CareerPathStep] ADD CONSTRAINT [FK_CareerPathStep_CareerPath_CareerPathId] FOREIGN KEY ([CareerPathId]) REFERENCES [Hrms].[CareerPath] ([Id]) ON DELETE CASCADE;

ALTER TABLE [Hrms].[CareerPathStep] ADD CONSTRAINT [FK_CareerPathStep_JobGrade_JobGradeId] FOREIGN KEY ([JobGradeId]) REFERENCES [Hrms].[JobGrade] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[CareerPathStep] ADD CONSTRAINT [FK_CareerPathStep_PositionClass_PositionClassId] FOREIGN KEY ([PositionClassId]) REFERENCES [Hrms].[PositionClass] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[CareerPathStepCompetency] ADD CONSTRAINT [FK_CareerPathStepCompetency_CareerPathStep_CareerPathStepId] FOREIGN KEY ([CareerPathStepId]) REFERENCES [Hrms].[CareerPathStep] ([Id]) ON DELETE CASCADE;

ALTER TABLE [Hrms].[CareerPathStepCompetency] ADD CONSTRAINT [FK_CareerPathStepCompetency_Competency_CompetencyId] FOREIGN KEY ([CompetencyId]) REFERENCES [Hrms].[Competency] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[ClearanceDepartmentApprover] ADD CONSTRAINT [FK_ClearanceDepartmentApprover_ClearanceDepartment_DepartmentId] FOREIGN KEY ([DepartmentId]) REFERENCES [Hrms].[ClearanceDepartment] ([Id]) ON DELETE CASCADE;

ALTER TABLE [Hrms].[CommunityPostReaction] ADD CONSTRAINT [FK_CommunityPostReaction_Employee_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Hrms].[Employee] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[CommunityPostReaction] ADD CONSTRAINT [FK_CommunityPostReaction_LearningCommunityPost_LearningCommunityPostId] FOREIGN KEY ([LearningCommunityPostId]) REFERENCES [Hrms].[LearningCommunityPost] ([Id]) ON DELETE CASCADE;

ALTER TABLE [Hrms].[CompanyAsset] ADD CONSTRAINT [FK_CompanyAsset_Employee_AssignedToEmployeeId] FOREIGN KEY ([AssignedToEmployeeId]) REFERENCES [Hrms].[Employee] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[CompensationRequest] ADD CONSTRAINT [FK_CompensationRequest_Employee_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Hrms].[Employee] ([Id]) ON DELETE CASCADE;

ALTER TABLE [Hrms].[Competency] ADD CONSTRAINT [FK_Competency_CompetencyCategory_CompetencyCategoryId] FOREIGN KEY ([CompetencyCategoryId]) REFERENCES [Hrms].[CompetencyCategory] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[CriterionEvaluator] ADD CONSTRAINT [FK_CriterionEvaluator_Employee_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Hrms].[Employee] ([Id]) ON DELETE SET NULL;

ALTER TABLE [Hrms].[CriterionEvaluator] ADD CONSTRAINT [FK_CriterionEvaluator_RequisitionScreeningCriterion_CriterionId] FOREIGN KEY ([CriterionId]) REFERENCES [Hrms].[RequisitionScreeningCriterion] ([Id]) ON DELETE CASCADE;

ALTER TABLE [Hrms].[CriticalPosition] ADD CONSTRAINT [FK_CriticalPosition_Position_PositionId] FOREIGN KEY ([PositionId]) REFERENCES [Hrms].[Position] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[DevelopmentAction] ADD CONSTRAINT [FK_DevelopmentAction_Competency_CompetencyId] FOREIGN KEY ([CompetencyId]) REFERENCES [Hrms].[Competency] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[DevelopmentAction] ADD CONSTRAINT [FK_DevelopmentAction_DevelopmentPlan_DevelopmentPlanId] FOREIGN KEY ([DevelopmentPlanId]) REFERENCES [Hrms].[DevelopmentPlan] ([Id]) ON DELETE CASCADE;

ALTER TABLE [Hrms].[DevelopmentPlan] ADD CONSTRAINT [FK_DevelopmentPlan_Appraisal_AppraisalId] FOREIGN KEY ([AppraisalId]) REFERENCES [Hrms].[Appraisal] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[DevelopmentPlan] ADD CONSTRAINT [FK_DevelopmentPlan_Employee_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Hrms].[Employee] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[DisciplinaryMeasure] ADD CONSTRAINT [FK_DisciplinaryMeasure_Employee_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Hrms].[Employee] ([Id]) ON DELETE CASCADE;

ALTER TABLE [Hrms].[DynamicFormField] ADD CONSTRAINT [FK_DynamicFormField_DynamicForm_DynamicFormId] FOREIGN KEY ([DynamicFormId]) REFERENCES [Hrms].[DynamicForm] ([Id]) ON DELETE CASCADE;

ALTER TABLE [Hrms].[DynamicFormRecord] ADD CONSTRAINT [FK_DynamicFormRecord_DynamicForm_DynamicFormId] FOREIGN KEY ([DynamicFormId]) REFERENCES [Hrms].[DynamicForm] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[Employee] ADD CONSTRAINT [FK_Employee_Branch_BranchId] FOREIGN KEY ([BranchId]) REFERENCES [Hrms].[Branch] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[Employee] ADD CONSTRAINT [FK_Employee_Person_PersonId] FOREIGN KEY ([PersonId]) REFERENCES [Core].[Person] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[Employee] ADD CONSTRAINT [FK_Employee_Position_PositionId] FOREIGN KEY ([PositionId]) REFERENCES [Hrms].[Position] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[Employee] ADD CONSTRAINT [FK_Employee_SalaryScale_SalaryScaleId] FOREIGN KEY ([SalaryScaleId]) REFERENCES [Core].[SalaryScale] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[EmployeeAllowance] ADD CONSTRAINT [FK_EmployeeAllowance_AllowanceType_AllowanceTypeId] FOREIGN KEY ([AllowanceTypeId]) REFERENCES [Hrms].[AllowanceType] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[EmployeeAllowance] ADD CONSTRAINT [FK_EmployeeAllowance_Employee_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Hrms].[Employee] ([Id]) ON DELETE CASCADE;

ALTER TABLE [Hrms].[EmployeeBenefitEnrollment] ADD CONSTRAINT [FK_EmployeeBenefitEnrollment_BenefitPlan_BenefitPlanId] FOREIGN KEY ([BenefitPlanId]) REFERENCES [Hrms].[BenefitPlan] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[EmployeeBenefitEnrollment] ADD CONSTRAINT [FK_EmployeeBenefitEnrollment_Employee_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Hrms].[Employee] ([Id]) ON DELETE CASCADE;

ALTER TABLE [Hrms].[EmployeeCareerPath] ADD CONSTRAINT [FK_EmployeeCareerPath_CareerPath_CareerPathId] FOREIGN KEY ([CareerPathId]) REFERENCES [Hrms].[CareerPath] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[EmployeeCareerPath] ADD CONSTRAINT [FK_EmployeeCareerPath_Employee_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Hrms].[Employee] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[EmployeeCareerPathStepProgress] ADD CONSTRAINT [FK_EmployeeCareerPathStepProgress_EmployeeCareerPath_EmployeeCareerPathId] FOREIGN KEY ([EmployeeCareerPathId]) REFERENCES [Hrms].[EmployeeCareerPath] ([Id]) ON DELETE CASCADE;

ALTER TABLE [Hrms].[EmployeeDependent] ADD CONSTRAINT [FK_EmployeeDependent_Employee_RelatedEmployeeId] FOREIGN KEY ([RelatedEmployeeId]) REFERENCES [Hrms].[Employee] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[EmployeeDependent] ADD CONSTRAINT [FK_EmployeeDependent_Person_PersonId] FOREIGN KEY ([PersonId]) REFERENCES [Core].[Person] ([Id]) ON DELETE CASCADE;

ALTER TABLE [Hrms].[EmployeeEducation] ADD CONSTRAINT [FK_EmployeeEducation_Person_PersonId] FOREIGN KEY ([PersonId]) REFERENCES [Core].[Person] ([Id]) ON DELETE CASCADE;

ALTER TABLE [Hrms].[EmployeeExperience] ADD CONSTRAINT [FK_EmployeeExperience_Person_PersonId] FOREIGN KEY ([PersonId]) REFERENCES [Core].[Person] ([Id]) ON DELETE CASCADE;

ALTER TABLE [Hrms].[EmployeeFieldValue] ADD CONSTRAINT [FK_EmployeeFieldValue_EmployeeFieldDefinition_FieldDefinitionId] FOREIGN KEY ([FieldDefinitionId]) REFERENCES [Hrms].[EmployeeFieldDefinition] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[EmployeeGoal] ADD CONSTRAINT [FK_EmployeeGoal_Employee_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Hrms].[Employee] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[EmployeeGoal] ADD CONSTRAINT [FK_EmployeeGoal_OrganizationalObjective_OrganizationalObjectiveId] FOREIGN KEY ([OrganizationalObjectiveId]) REFERENCES [Hrms].[OrganizationalObjective] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[EmployeeGoal] ADD CONSTRAINT [FK_EmployeeGoal_ReviewCycle_ReviewCycleId] FOREIGN KEY ([ReviewCycleId]) REFERENCES [Hrms].[ReviewCycle] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[EmployeeGuarantee] ADD CONSTRAINT [FK_EmployeeGuarantee_Employee_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Hrms].[Employee] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[EmployeeMovement] ADD CONSTRAINT [FK_EmployeeMovement_Employee_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Hrms].[Employee] ([Id]) ON DELETE CASCADE;

ALTER TABLE [Hrms].[EmployeeMovement] ADD CONSTRAINT [FK_EmployeeMovement_SalaryScale_ToSalaryScaleId] FOREIGN KEY ([ToSalaryScaleId]) REFERENCES [Core].[SalaryScale] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[EmployeeRecognition] ADD CONSTRAINT [FK_EmployeeRecognition_Employee_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Hrms].[Employee] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[EmployeeRecognition] ADD CONSTRAINT [FK_EmployeeRecognition_RecognitionBadge_RecognitionBadgeId] FOREIGN KEY ([RecognitionBadgeId]) REFERENCES [Hrms].[RecognitionBadge] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[EmployeeTermination] ADD CONSTRAINT [FK_EmployeeTermination_Employee_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Hrms].[Employee] ([Id]) ON DELETE CASCADE;

ALTER TABLE [Hrms].[EmployeeTrainingCertificate] ADD CONSTRAINT [FK_EmployeeTrainingCertificate_Employee_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Hrms].[Employee] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[EmployeeTrainingCertificate] ADD CONSTRAINT [FK_EmployeeTrainingCertificate_TrainingCourse_TrainingCourseId] FOREIGN KEY ([TrainingCourseId]) REFERENCES [Hrms].[TrainingCourse] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[EmployeeTrainingCertificate] ADD CONSTRAINT [FK_EmployeeTrainingCertificate_TrainingEnrollment_TrainingEnrollmentId] FOREIGN KEY ([TrainingEnrollmentId]) REFERENCES [Hrms].[TrainingEnrollment] ([Id]) ON DELETE SET NULL;

ALTER TABLE [Hrms].[ExitInterview] ADD CONSTRAINT [FK_ExitInterview_EmployeeTermination_TerminationId] FOREIGN KEY ([TerminationId]) REFERENCES [Hrms].[EmployeeTermination] ([Id]) ON DELETE CASCADE;

ALTER TABLE [Hrms].[GoalActionItem] ADD CONSTRAINT [FK_GoalActionItem_EmployeeGoal_EmployeeGoalId] FOREIGN KEY ([EmployeeGoalId]) REFERENCES [Hrms].[EmployeeGoal] ([Id]) ON DELETE CASCADE;

ALTER TABLE [Hrms].[Grievance] ADD CONSTRAINT [FK_Grievance_Employee_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Hrms].[Employee] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[GrievanceNote] ADD CONSTRAINT [FK_GrievanceNote_Grievance_GrievanceId] FOREIGN KEY ([GrievanceId]) REFERENCES [Hrms].[Grievance] ([Id]) ON DELETE CASCADE;

ALTER TABLE [Hrms].[HiringRequest] ADD CONSTRAINT [FK_HiringRequest_OrganizationUnit_OrganizationUnitId] FOREIGN KEY ([OrganizationUnitId]) REFERENCES [Hrms].[OrganizationUnit] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[HiringRequest] ADD CONSTRAINT [FK_HiringRequest_PositionClass_PositionClassId] FOREIGN KEY ([PositionClassId]) REFERENCES [Hrms].[PositionClass] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[ImprovementPlan] ADD CONSTRAINT [FK_ImprovementPlan_Appraisal_AppraisalId] FOREIGN KEY ([AppraisalId]) REFERENCES [Hrms].[Appraisal] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[ImprovementPlan] ADD CONSTRAINT [FK_ImprovementPlan_Employee_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Hrms].[Employee] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[InsuranceClaim] ADD CONSTRAINT [FK_InsuranceClaim_Employee_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Hrms].[Employee] ([Id]) ON DELETE CASCADE;

ALTER TABLE [Hrms].[InsuranceClaim] ADD CONSTRAINT [FK_InsuranceClaim_InsurancePolicy_InsurancePolicyId] FOREIGN KEY ([InsurancePolicyId]) REFERENCES [Hrms].[InsurancePolicy] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[InsuranceClaimAttachment] ADD CONSTRAINT [FK_InsuranceClaimAttachment_InsuranceClaim_InsuranceClaimId] FOREIGN KEY ([InsuranceClaimId]) REFERENCES [Hrms].[InsuranceClaim] ([Id]) ON DELETE CASCADE;

ALTER TABLE [Hrms].[InsurancePremiumSchedule] ADD CONSTRAINT [FK_InsurancePremiumSchedule_InsurancePolicy_InsurancePolicyId] FOREIGN KEY ([InsurancePolicyId]) REFERENCES [Hrms].[InsurancePolicy] ([Id]) ON DELETE CASCADE;

ALTER TABLE [Hrms].[Interview] ADD CONSTRAINT [FK_Interview_JobApplication_ApplicationId] FOREIGN KEY ([ApplicationId]) REFERENCES [Hrms].[JobApplication] ([Id]) ON DELETE CASCADE;

ALTER TABLE [Hrms].[InterviewFeedback] ADD CONSTRAINT [FK_InterviewFeedback_InterviewPanelist_PanelistId] FOREIGN KEY ([PanelistId]) REFERENCES [Hrms].[InterviewPanelist] ([Id]) ON DELETE CASCADE;

ALTER TABLE [Hrms].[InterviewPanelist] ADD CONSTRAINT [FK_InterviewPanelist_Employee_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Hrms].[Employee] ([Id]) ON DELETE SET NULL;

ALTER TABLE [Hrms].[InterviewPanelist] ADD CONSTRAINT [FK_InterviewPanelist_Interview_InterviewId] FOREIGN KEY ([InterviewId]) REFERENCES [Hrms].[Interview] ([Id]) ON DELETE CASCADE;

ALTER TABLE [Hrms].[JobApplication] ADD CONSTRAINT [FK_JobApplication_Candidate_CandidateId] FOREIGN KEY ([CandidateId]) REFERENCES [Hrms].[Candidate] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[JobApplication] ADD CONSTRAINT [FK_JobApplication_JobRequisition_RequisitionId] FOREIGN KEY ([RequisitionId]) REFERENCES [Hrms].[JobRequisition] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[JobApplicationStageLog] ADD CONSTRAINT [FK_JobApplicationStageLog_JobApplication_ApplicationId] FOREIGN KEY ([ApplicationId]) REFERENCES [Hrms].[JobApplication] ([Id]) ON DELETE CASCADE;

ALTER TABLE [Hrms].[JobOffer] ADD CONSTRAINT [FK_JobOffer_Employee_HiringManagerEmployeeId] FOREIGN KEY ([HiringManagerEmployeeId]) REFERENCES [Hrms].[Employee] ([Id]) ON DELETE SET NULL;

ALTER TABLE [Hrms].[JobOffer] ADD CONSTRAINT [FK_JobOffer_JobApplication_ApplicationId] FOREIGN KEY ([ApplicationId]) REFERENCES [Hrms].[JobApplication] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[JobOffer] ADD CONSTRAINT [FK_JobOffer_SalaryScale_SalaryScaleId] FOREIGN KEY ([SalaryScaleId]) REFERENCES [Core].[SalaryScale] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[JobRequisition] ADD CONSTRAINT [FK_JobRequisition_HiringRequest_HiringRequestId] FOREIGN KEY ([HiringRequestId]) REFERENCES [Hrms].[HiringRequest] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[JobRequisition] ADD CONSTRAINT [FK_JobRequisition_OrganizationUnit_OrganizationUnitId] FOREIGN KEY ([OrganizationUnitId]) REFERENCES [Hrms].[OrganizationUnit] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[JobRequisition] ADD CONSTRAINT [FK_JobRequisition_PositionClass_PositionClassId] FOREIGN KEY ([PositionClassId]) REFERENCES [Hrms].[PositionClass] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[JobRequisition] ADD CONSTRAINT [FK_JobRequisition_SalaryScale_SalaryScaleId] FOREIGN KEY ([SalaryScaleId]) REFERENCES [Core].[SalaryScale] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[JobRequisition] ADD CONSTRAINT [FK_JobRequisition_WorkLocation_WorkLocationId] FOREIGN KEY ([WorkLocationId]) REFERENCES [Hrms].[WorkLocation] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[KnowledgeTransfer] ADD CONSTRAINT [FK_KnowledgeTransfer_Employee_FromEmployeeId] FOREIGN KEY ([FromEmployeeId]) REFERENCES [Hrms].[Employee] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[KnowledgeTransfer] ADD CONSTRAINT [FK_KnowledgeTransfer_SuccessionCandidate_SuccessionCandidateId] FOREIGN KEY ([SuccessionCandidateId]) REFERENCES [Hrms].[SuccessionCandidate] ([Id]) ON DELETE CASCADE;

ALTER TABLE [Hrms].[LearningCommunity] ADD CONSTRAINT [FK_LearningCommunity_TrainingCourse_TrainingCourseId] FOREIGN KEY ([TrainingCourseId]) REFERENCES [Hrms].[TrainingCourse] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[LearningCommunityMember] ADD CONSTRAINT [FK_LearningCommunityMember_Employee_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Hrms].[Employee] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[LearningCommunityMember] ADD CONSTRAINT [FK_LearningCommunityMember_LearningCommunity_LearningCommunityId] FOREIGN KEY ([LearningCommunityId]) REFERENCES [Hrms].[LearningCommunity] ([Id]) ON DELETE CASCADE;

ALTER TABLE [Hrms].[LearningCommunityPost] ADD CONSTRAINT [FK_LearningCommunityPost_Employee_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Hrms].[Employee] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[LearningCommunityPost] ADD CONSTRAINT [FK_LearningCommunityPost_LearningCommunity_LearningCommunityId] FOREIGN KEY ([LearningCommunityId]) REFERENCES [Hrms].[LearningCommunity] ([Id]) ON DELETE CASCADE;

ALTER TABLE [Hrms].[LearningPath] ADD CONSTRAINT [FK_LearningPath_Position_TargetPositionId] FOREIGN KEY ([TargetPositionId]) REFERENCES [Hrms].[Position] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[LearningPathStep] ADD CONSTRAINT [FK_LearningPathStep_LearningPath_LearningPathId] FOREIGN KEY ([LearningPathId]) REFERENCES [Hrms].[LearningPath] ([Id]) ON DELETE CASCADE;

ALTER TABLE [Hrms].[LearningPathStep] ADD CONSTRAINT [FK_LearningPathStep_TrainingCourse_TrainingCourseId] FOREIGN KEY ([TrainingCourseId]) REFERENCES [Hrms].[TrainingCourse] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[LeaveBalance] ADD CONSTRAINT [FK_LeaveBalance_Employee_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Hrms].[Employee] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[LeaveBalance] ADD CONSTRAINT [FK_LeaveBalance_FiscalYear_FiscalYearId] FOREIGN KEY ([FiscalYearId]) REFERENCES [Core].[FiscalYear] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[LeaveBalance] ADD CONSTRAINT [FK_LeaveBalance_LeaveType_LeaveTypeId] FOREIGN KEY ([LeaveTypeId]) REFERENCES [Hrms].[LeaveType] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[LeaveRequest] ADD CONSTRAINT [FK_LeaveRequest_Employee_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Hrms].[Employee] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[LeaveRequest] ADD CONSTRAINT [FK_LeaveRequest_FiscalYear_FiscalYearId] FOREIGN KEY ([FiscalYearId]) REFERENCES [Core].[FiscalYear] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[LeaveRequestLine] ADD CONSTRAINT [FK_LeaveRequestLine_LeaveRequest_LeaveRequestId] FOREIGN KEY ([LeaveRequestId]) REFERENCES [Hrms].[LeaveRequest] ([Id]) ON DELETE CASCADE;

ALTER TABLE [Hrms].[LeaveRequestLine] ADD CONSTRAINT [FK_LeaveRequestLine_LeaveType_LeaveTypeId] FOREIGN KEY ([LeaveTypeId]) REFERENCES [Hrms].[LeaveType] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[Loan] ADD CONSTRAINT [FK_Loan_Employee_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Hrms].[Employee] ([Id]) ON DELETE CASCADE;

ALTER TABLE [Hrms].[Loan] ADD CONSTRAINT [FK_Loan_LoanType_LoanTypeId] FOREIGN KEY ([LoanTypeId]) REFERENCES [Hrms].[LoanType] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[LoanGuarantor] ADD CONSTRAINT [FK_LoanGuarantor_Loan_LoanId] FOREIGN KEY ([LoanId]) REFERENCES [Hrms].[Loan] ([Id]) ON DELETE CASCADE;

ALTER TABLE [Hrms].[LoanRepaymentSchedule] ADD CONSTRAINT [FK_LoanRepaymentSchedule_Loan_LoanId] FOREIGN KEY ([LoanId]) REFERENCES [Hrms].[Loan] ([Id]) ON DELETE CASCADE;

ALTER TABLE [Hrms].[MedicalBeneficiary] ADD CONSTRAINT [FK_MedicalBeneficiary_MedicalEnrollment_MedicalEnrollmentId] FOREIGN KEY ([MedicalEnrollmentId]) REFERENCES [Hrms].[MedicalEnrollment] ([Id]) ON DELETE CASCADE;

ALTER TABLE [Hrms].[MedicalClaim] ADD CONSTRAINT [FK_MedicalClaim_Employee_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Hrms].[Employee] ([Id]) ON DELETE CASCADE;

ALTER TABLE [Hrms].[MedicalClaim] ADD CONSTRAINT [FK_MedicalClaim_MedicalEnrollment_MedicalEnrollmentId] FOREIGN KEY ([MedicalEnrollmentId]) REFERENCES [Hrms].[MedicalEnrollment] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[MedicalClaimAttachment] ADD CONSTRAINT [FK_MedicalClaimAttachment_MedicalClaim_MedicalClaimId] FOREIGN KEY ([MedicalClaimId]) REFERENCES [Hrms].[MedicalClaim] ([Id]) ON DELETE CASCADE;

ALTER TABLE [Hrms].[MedicalEnrollment] ADD CONSTRAINT [FK_MedicalEnrollment_Employee_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Hrms].[Employee] ([Id]) ON DELETE CASCADE;

ALTER TABLE [Hrms].[MedicalEnrollment] ADD CONSTRAINT [FK_MedicalEnrollment_MedicalPlan_MedicalPlanId] FOREIGN KEY ([MedicalPlanId]) REFERENCES [Hrms].[MedicalPlan] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[MedicalServiceContract] ADD CONSTRAINT [FK_MedicalServiceContract_MedicalProvider_MedicalProviderId] FOREIGN KEY ([MedicalProviderId]) REFERENCES [Hrms].[MedicalProvider] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[Mentorship] ADD CONSTRAINT [FK_Mentorship_Employee_MenteeEmployeeId] FOREIGN KEY ([MenteeEmployeeId]) REFERENCES [Hrms].[Employee] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[Mentorship] ADD CONSTRAINT [FK_Mentorship_Employee_MentorEmployeeId] FOREIGN KEY ([MentorEmployeeId]) REFERENCES [Hrms].[Employee] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Core].[Module] ADD CONSTRAINT [FK_Module_Subsystem_SubsystemId] FOREIGN KEY ([SubsystemId]) REFERENCES [Core].[Subsystem] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Core].[Operation] ADD CONSTRAINT [FK_Operation_Module_ModuleId] FOREIGN KEY ([ModuleId]) REFERENCES [Core].[Module] ([Id]) ON DELETE CASCADE;

ALTER TABLE [Hrms].[OrganizationalObjective] ADD CONSTRAINT [FK_OrganizationalObjective_OrganizationUnit_OrganizationUnitId] FOREIGN KEY ([OrganizationUnitId]) REFERENCES [Hrms].[OrganizationUnit] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[OrganizationalObjective] ADD CONSTRAINT [FK_OrganizationalObjective_OrganizationalObjective_ParentObjectiveId] FOREIGN KEY ([ParentObjectiveId]) REFERENCES [Hrms].[OrganizationalObjective] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[OrganizationalObjective] ADD CONSTRAINT [FK_OrganizationalObjective_ReviewCycle_ReviewCycleId] FOREIGN KEY ([ReviewCycleId]) REFERENCES [Hrms].[ReviewCycle] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[OrganizationUnit] ADD CONSTRAINT [FK_OrganizationUnit_Branch_BranchId] FOREIGN KEY ([BranchId]) REFERENCES [Hrms].[Branch] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[OrganizationUnit] ADD CONSTRAINT [FK_OrganizationUnit_OrganizationUnit_ParentId] FOREIGN KEY ([ParentId]) REFERENCES [Hrms].[OrganizationUnit] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[OrganizationUnit] ADD CONSTRAINT [FK_OrganizationUnit_WorkLocation_WorkLocationId] FOREIGN KEY ([WorkLocationId]) REFERENCES [Hrms].[WorkLocation] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[OtherLeave] ADD CONSTRAINT [FK_OtherLeave_Employee_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Hrms].[Employee] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[OtherLeave] ADD CONSTRAINT [FK_OtherLeave_OtherLeaveSetting_OtherLeaveSettingId] FOREIGN KEY ([OtherLeaveSettingId]) REFERENCES [Hrms].[OtherLeaveSetting] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[OtherLeaveDetail] ADD CONSTRAINT [FK_OtherLeaveDetail_OtherLeave_OtherLeaveHeaderId] FOREIGN KEY ([OtherLeaveHeaderId]) REFERENCES [Hrms].[OtherLeave] ([Id]) ON DELETE CASCADE;

ALTER TABLE [Hrms].[OtherLeaveSetting] ADD CONSTRAINT [FK_OtherLeaveSetting_FiscalYear_FiscalYearId] FOREIGN KEY ([FiscalYearId]) REFERENCES [Core].[FiscalYear] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[OtherLeaveSetting] ADD CONSTRAINT [FK_OtherLeaveSetting_LeaveType_LeaveTypeId] FOREIGN KEY ([LeaveTypeId]) REFERENCES [Hrms].[LeaveType] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[PerDiemRate] ADD CONSTRAINT [FK_PerDiemRate_JobGrade_JobGradeId] FOREIGN KEY ([JobGradeId]) REFERENCES [Hrms].[JobGrade] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[PipObjective] ADD CONSTRAINT [FK_PipObjective_ImprovementPlan_PipId] FOREIGN KEY ([PipId]) REFERENCES [Hrms].[ImprovementPlan] ([Id]) ON DELETE CASCADE;

ALTER TABLE [Hrms].[Position] ADD CONSTRAINT [FK_Position_Branch_BranchId] FOREIGN KEY ([BranchId]) REFERENCES [Hrms].[Branch] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[Position] ADD CONSTRAINT [FK_Position_OrganizationUnit_OrganizationUnitId] FOREIGN KEY ([OrganizationUnitId]) REFERENCES [Hrms].[OrganizationUnit] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[Position] ADD CONSTRAINT [FK_Position_PositionClass_PositionClassId] FOREIGN KEY ([PositionClassId]) REFERENCES [Hrms].[PositionClass] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[PositionClass] ADD CONSTRAINT [FK_PositionClass_JobCategory_JobCategoryId] FOREIGN KEY ([JobCategoryId]) REFERENCES [Hrms].[JobCategory] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[PositionClass] ADD CONSTRAINT [FK_PositionClass_PositionClass_ReportsToPositionClassId] FOREIGN KEY ([ReportsToPositionClassId]) REFERENCES [Hrms].[PositionClass] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[PositionClass] ADD CONSTRAINT [FK_PositionClass_SalaryScale_SalaryScaleId] FOREIGN KEY ([SalaryScaleId]) REFERENCES [Core].[SalaryScale] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[PositionClass] ADD CONSTRAINT [FK_PositionClass_WorkLocation_WorkLocationId] FOREIGN KEY ([WorkLocationId]) REFERENCES [Hrms].[WorkLocation] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[PositionCompetency] ADD CONSTRAINT [FK_PositionCompetency_Competency_CompetencyId] FOREIGN KEY ([CompetencyId]) REFERENCES [Hrms].[Competency] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[PositionCompetency] ADD CONSTRAINT [FK_PositionCompetency_Position_PositionId] FOREIGN KEY ([PositionId]) REFERENCES [Hrms].[Position] ([Id]) ON DELETE CASCADE;

ALTER TABLE [Hrms].[ProfileChangeRequest] ADD CONSTRAINT [FK_ProfileChangeRequest_Employee_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Hrms].[Employee] ([Id]) ON DELETE CASCADE;

ALTER TABLE [Hrms].[RatingScaleLevel] ADD CONSTRAINT [FK_RatingScaleLevel_RatingScale_RatingScaleId] FOREIGN KEY ([RatingScaleId]) REFERENCES [Hrms].[RatingScale] ([Id]) ON DELETE CASCADE;

ALTER TABLE [Hrms].[RecognitionBadge] ADD CONSTRAINT [FK_RecognitionBadge_AwardCategory_AwardCategoryId] FOREIGN KEY ([AwardCategoryId]) REFERENCES [Hrms].[AwardCategory] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[RecognitionProgram] ADD CONSTRAINT [FK_RecognitionProgram_RecognitionBadge_RecognitionBadgeId] FOREIGN KEY ([RecognitionBadgeId]) REFERENCES [Hrms].[RecognitionBadge] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[ReportField] ADD CONSTRAINT [FK_ReportField_Report_ReportId] FOREIGN KEY ([ReportId]) REFERENCES [Hrms].[Report] ([Id]) ON DELETE CASCADE;

ALTER TABLE [Hrms].[ReportFieldOutput] ADD CONSTRAINT [FK_ReportFieldOutput_Report_ReportId] FOREIGN KEY ([ReportId]) REFERENCES [Hrms].[Report] ([Id]) ON DELETE CASCADE;

ALTER TABLE [Hrms].[ReportRestriction] ADD CONSTRAINT [FK_ReportRestriction_Report_ReportId] FOREIGN KEY ([ReportId]) REFERENCES [Hrms].[Report] ([Id]) ON DELETE CASCADE;

ALTER TABLE [Hrms].[ReportRunRecipient] ADD CONSTRAINT [FK_ReportRunRecipient_ReportRun_ReportRunId] FOREIGN KEY ([ReportRunId]) REFERENCES [Hrms].[ReportRun] ([Id]) ON DELETE CASCADE;

ALTER TABLE [Hrms].[ReportSavedFilter] ADD CONSTRAINT [FK_ReportSavedFilter_Report_ReportId] FOREIGN KEY ([ReportId]) REFERENCES [Hrms].[Report] ([Id]) ON DELETE CASCADE;

ALTER TABLE [Hrms].[ReportSchedule] ADD CONSTRAINT [FK_ReportSchedule_Report_ReportId] FOREIGN KEY ([ReportId]) REFERENCES [Hrms].[Report] ([Id]) ON DELETE CASCADE;

ALTER TABLE [Hrms].[ReportScheduleFieldOutput] ADD CONSTRAINT [FK_ReportScheduleFieldOutput_ReportSchedule_ReportScheduleId] FOREIGN KEY ([ReportScheduleId]) REFERENCES [Hrms].[ReportSchedule] ([Id]) ON DELETE CASCADE;

ALTER TABLE [Hrms].[ReportScheduleFieldValue] ADD CONSTRAINT [FK_ReportScheduleFieldValue_ReportSchedule_ReportScheduleId] FOREIGN KEY ([ReportScheduleId]) REFERENCES [Hrms].[ReportSchedule] ([Id]) ON DELETE CASCADE;

ALTER TABLE [Hrms].[ReportScheduleRecipient] ADD CONSTRAINT [FK_ReportScheduleRecipient_ReportSchedule_ReportScheduleId] FOREIGN KEY ([ReportScheduleId]) REFERENCES [Hrms].[ReportSchedule] ([Id]) ON DELETE CASCADE;

ALTER TABLE [Hrms].[RequisitionScreeningCriterion] ADD CONSTRAINT [FK_RequisitionScreeningCriterion_JobRequisition_RequisitionId] FOREIGN KEY ([RequisitionId]) REFERENCES [Hrms].[JobRequisition] ([Id]) ON DELETE CASCADE;

ALTER TABLE [Hrms].[ReviewCycle] ADD CONSTRAINT [FK_ReviewCycle_FiscalYear_FiscalYearId] FOREIGN KEY ([FiscalYearId]) REFERENCES [Core].[FiscalYear] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[ReviewCycle] ADD CONSTRAINT [FK_ReviewCycle_RatingScale_RatingScaleId] FOREIGN KEY ([RatingScaleId]) REFERENCES [Hrms].[RatingScale] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[RewardDisbursement] ADD CONSTRAINT [FK_RewardDisbursement_EmployeeRecognition_EmployeeRecognitionId] FOREIGN KEY ([EmployeeRecognitionId]) REFERENCES [Hrms].[EmployeeRecognition] ([Id]) ON DELETE SET NULL;

ALTER TABLE [Hrms].[RewardDisbursement] ADD CONSTRAINT [FK_RewardDisbursement_Employee_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Hrms].[Employee] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[RewardDisbursement] ADD CONSTRAINT [FK_RewardDisbursement_RecognitionBadge_RecognitionBadgeId] FOREIGN KEY ([RecognitionBadgeId]) REFERENCES [Hrms].[RecognitionBadge] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[RewardNomination] ADD CONSTRAINT [FK_RewardNomination_Employee_NomineeEmployeeId] FOREIGN KEY ([NomineeEmployeeId]) REFERENCES [Hrms].[Employee] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[RewardNomination] ADD CONSTRAINT [FK_RewardNomination_RecognitionBadge_RecognitionBadgeId] FOREIGN KEY ([RecognitionBadgeId]) REFERENCES [Hrms].[RecognitionBadge] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[RewardNomination] ADD CONSTRAINT [FK_RewardNomination_RecognitionProgram_RecognitionProgramId] FOREIGN KEY ([RecognitionProgramId]) REFERENCES [Hrms].[RecognitionProgram] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[RewardPointsTransaction] ADD CONSTRAINT [FK_RewardPointsTransaction_Employee_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Hrms].[Employee] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Core].[RolePermission] ADD CONSTRAINT [FK_RolePermission_Operation_OperationId] FOREIGN KEY ([OperationId]) REFERENCES [Core].[Operation] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[SalaryRevisionBand] ADD CONSTRAINT [FK_SalaryRevisionBand_SalaryRevision_SalaryRevisionId] FOREIGN KEY ([SalaryRevisionId]) REFERENCES [Hrms].[SalaryRevision] ([Id]) ON DELETE CASCADE;

ALTER TABLE [Hrms].[SalaryRevisionLine] ADD CONSTRAINT [FK_SalaryRevisionLine_Employee_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Hrms].[Employee] ([Id]) ON DELETE CASCADE;

ALTER TABLE [Hrms].[SalaryRevisionLine] ADD CONSTRAINT [FK_SalaryRevisionLine_SalaryRevision_SalaryRevisionId] FOREIGN KEY ([SalaryRevisionId]) REFERENCES [Hrms].[SalaryRevision] ([Id]) ON DELETE CASCADE;

ALTER TABLE [Core].[SalaryScale] ADD CONSTRAINT [FK_SalaryScale_JobGrade_JobGradeId] FOREIGN KEY ([JobGradeId]) REFERENCES [Hrms].[JobGrade] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Core].[SalaryScale] ADD CONSTRAINT [FK_SalaryScale_Step_StepId] FOREIGN KEY ([StepId]) REFERENCES [Core].[Step] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[SettlementLine] ADD CONSTRAINT [FK_SettlementLine_TerminationSettlement_TerminationSettlementId] FOREIGN KEY ([TerminationSettlementId]) REFERENCES [Hrms].[TerminationSettlement] ([Id]) ON DELETE CASCADE;

ALTER TABLE [Hrms].[SuccessionCandidate] ADD CONSTRAINT [FK_SuccessionCandidate_Employee_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Hrms].[Employee] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[SuccessionCandidate] ADD CONSTRAINT [FK_SuccessionCandidate_SuccessionPlan_SuccessionPlanId] FOREIGN KEY ([SuccessionPlanId]) REFERENCES [Hrms].[SuccessionPlan] ([Id]) ON DELETE CASCADE;

ALTER TABLE [Hrms].[SuccessionDevelopmentAction] ADD CONSTRAINT [FK_SuccessionDevelopmentAction_Employee_MentorEmployeeId] FOREIGN KEY ([MentorEmployeeId]) REFERENCES [Hrms].[Employee] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[SuccessionDevelopmentAction] ADD CONSTRAINT [FK_SuccessionDevelopmentAction_SuccessionCandidate_SuccessionCandidateId] FOREIGN KEY ([SuccessionCandidateId]) REFERENCES [Hrms].[SuccessionCandidate] ([Id]) ON DELETE CASCADE;

ALTER TABLE [Hrms].[SuccessionPlan] ADD CONSTRAINT [FK_SuccessionPlan_CriticalPosition_CriticalPositionId] FOREIGN KEY ([CriticalPositionId]) REFERENCES [Hrms].[CriticalPosition] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[SurveyCompletion] ADD CONSTRAINT [FK_SurveyCompletion_Employee_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Hrms].[Employee] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[SurveyCompletion] ADD CONSTRAINT [FK_SurveyCompletion_Survey_SurveyId] FOREIGN KEY ([SurveyId]) REFERENCES [Hrms].[Survey] ([Id]) ON DELETE CASCADE;

ALTER TABLE [Hrms].[SurveyResponse] ADD CONSTRAINT [FK_SurveyResponse_Survey_SurveyId] FOREIGN KEY ([SurveyId]) REFERENCES [Hrms].[Survey] ([Id]) ON DELETE CASCADE;

ALTER TABLE [Hrms].[TalentAssessment] ADD CONSTRAINT [FK_TalentAssessment_Employee_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Hrms].[Employee] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[TalentAssessment] ADD CONSTRAINT [FK_TalentAssessment_TalentReview_TalentReviewId] FOREIGN KEY ([TalentReviewId]) REFERENCES [Hrms].[TalentReview] ([Id]) ON DELETE CASCADE;

ALTER TABLE [Hrms].[TalentRating] ADD CONSTRAINT [FK_TalentRating_Employee_RaterEmployeeId] FOREIGN KEY ([RaterEmployeeId]) REFERENCES [Hrms].[Employee] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[TalentRating] ADD CONSTRAINT [FK_TalentRating_TalentAssessment_TalentAssessmentId] FOREIGN KEY ([TalentAssessmentId]) REFERENCES [Hrms].[TalentAssessment] ([Id]) ON DELETE CASCADE;

ALTER TABLE [Hrms].[TalentReview] ADD CONSTRAINT [FK_TalentReview_OrganizationUnit_OrganizationUnitId] FOREIGN KEY ([OrganizationUnitId]) REFERENCES [Hrms].[OrganizationUnit] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[TerminationAssetRecovery] ADD CONSTRAINT [FK_TerminationAssetRecovery_CompanyAsset_CompanyAssetId] FOREIGN KEY ([CompanyAssetId]) REFERENCES [Hrms].[CompanyAsset] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[TerminationAssetRecovery] ADD CONSTRAINT [FK_TerminationAssetRecovery_EmployeeTermination_TerminationId] FOREIGN KEY ([TerminationId]) REFERENCES [Hrms].[EmployeeTermination] ([Id]) ON DELETE CASCADE;

ALTER TABLE [Hrms].[TerminationClearance] ADD CONSTRAINT [FK_TerminationClearance_ClearanceDepartment_DepartmentId] FOREIGN KEY ([DepartmentId]) REFERENCES [Hrms].[ClearanceDepartment] ([Id]) ON DELETE SET NULL;

ALTER TABLE [Hrms].[TerminationClearance] ADD CONSTRAINT [FK_TerminationClearance_EmployeeTermination_TerminationId] FOREIGN KEY ([TerminationId]) REFERENCES [Hrms].[EmployeeTermination] ([Id]) ON DELETE CASCADE;

ALTER TABLE [Hrms].[TerminationSettlement] ADD CONSTRAINT [FK_TerminationSettlement_EmployeeTermination_TerminationId] FOREIGN KEY ([TerminationId]) REFERENCES [Hrms].[EmployeeTermination] ([Id]) ON DELETE CASCADE;

ALTER TABLE [Hrms].[TrainingBudget] ADD CONSTRAINT [FK_TrainingBudget_OrganizationUnit_OrganizationUnitId] FOREIGN KEY ([OrganizationUnitId]) REFERENCES [Hrms].[OrganizationUnit] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[TrainingCourse] ADD CONSTRAINT [FK_TrainingCourse_TrainingCategory_TrainingCategoryId] FOREIGN KEY ([TrainingCategoryId]) REFERENCES [Hrms].[TrainingCategory] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[TrainingEnrollment] ADD CONSTRAINT [FK_TrainingEnrollment_Employee_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Hrms].[Employee] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[TrainingEnrollment] ADD CONSTRAINT [FK_TrainingEnrollment_TrainingNeed_TrainingNeedId] FOREIGN KEY ([TrainingNeedId]) REFERENCES [Hrms].[TrainingNeed] ([Id]) ON DELETE SET NULL;

ALTER TABLE [Hrms].[TrainingEnrollment] ADD CONSTRAINT [FK_TrainingEnrollment_TrainingSession_TrainingSessionId] FOREIGN KEY ([TrainingSessionId]) REFERENCES [Hrms].[TrainingSession] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[TrainingNeed] ADD CONSTRAINT [FK_TrainingNeed_Competency_CompetencyId] FOREIGN KEY ([CompetencyId]) REFERENCES [Hrms].[Competency] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[TrainingNeed] ADD CONSTRAINT [FK_TrainingNeed_Employee_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Hrms].[Employee] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[TrainingNeed] ADD CONSTRAINT [FK_TrainingNeed_TrainingCourse_TrainingCourseId] FOREIGN KEY ([TrainingCourseId]) REFERENCES [Hrms].[TrainingCourse] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[TrainingProviderPayment] ADD CONSTRAINT [FK_TrainingProviderPayment_TrainingSession_TrainingSessionId] FOREIGN KEY ([TrainingSessionId]) REFERENCES [Hrms].[TrainingSession] ([Id]) ON DELETE SET NULL;

ALTER TABLE [Hrms].[TrainingSession] ADD CONSTRAINT [FK_TrainingSession_TrainingCourse_TrainingCourseId] FOREIGN KEY ([TrainingCourseId]) REFERENCES [Hrms].[TrainingCourse] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[TripBudget] ADD CONSTRAINT [FK_TripBudget_OrganizationUnit_OrganizationUnitId] FOREIGN KEY ([OrganizationUnitId]) REFERENCES [Hrms].[OrganizationUnit] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[TripExpense] ADD CONSTRAINT [FK_TripExpense_TripRequest_TripRequestId] FOREIGN KEY ([TripRequestId]) REFERENCES [Hrms].[TripRequest] ([Id]) ON DELETE CASCADE;

ALTER TABLE [Hrms].[TripRequest] ADD CONSTRAINT [FK_TripRequest_Employee_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Hrms].[Employee] ([Id]) ON DELETE CASCADE;

ALTER TABLE [Hrms].[TripRequest] ADD CONSTRAINT [FK_TripRequest_TripBudget_TripBudgetId] FOREIGN KEY ([TripBudgetId]) REFERENCES [Hrms].[TripBudget] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Core].[User] ADD CONSTRAINT [FK_User_Employee_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Hrms].[Employee] ([Id]) ON DELETE SET NULL;

ALTER TABLE [Hrms].[WorkflowActionLog] ADD CONSTRAINT [FK_WorkflowActionLog_WorkflowInstance_InstanceId] FOREIGN KEY ([InstanceId]) REFERENCES [Hrms].[WorkflowInstance] ([Id]) ON DELETE CASCADE;

ALTER TABLE [Hrms].[WorkflowInstance] ADD CONSTRAINT [FK_WorkflowInstance_WorkflowDefinition_DefinitionId] FOREIGN KEY ([DefinitionId]) REFERENCES [Hrms].[WorkflowDefinition] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[WorkflowStep] ADD CONSTRAINT [FK_WorkflowStep_WorkflowDefinition_DefinitionId] FOREIGN KEY ([DefinitionId]) REFERENCES [Hrms].[WorkflowDefinition] ([Id]) ON DELETE CASCADE;

ALTER TABLE [Hrms].[WorkflowStepApprover] ADD CONSTRAINT [FK_WorkflowStepApprover_WorkflowStep_StepId] FOREIGN KEY ([StepId]) REFERENCES [Hrms].[WorkflowStep] ([Id]) ON DELETE CASCADE;

ALTER TABLE [Hrms].[WorkforcePlan] ADD CONSTRAINT [FK_WorkforcePlan_FiscalYear_StartFiscalYearId] FOREIGN KEY ([StartFiscalYearId]) REFERENCES [Core].[FiscalYear] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[WorkforcePlan] ADD CONSTRAINT [FK_WorkforcePlan_OrganizationUnit_OrganizationUnitId] FOREIGN KEY ([OrganizationUnitId]) REFERENCES [Hrms].[OrganizationUnit] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[WorkforcePlanLine] ADD CONSTRAINT [FK_WorkforcePlanLine_OrganizationUnit_OrganizationUnitId] FOREIGN KEY ([OrganizationUnitId]) REFERENCES [Hrms].[OrganizationUnit] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[WorkforcePlanLine] ADD CONSTRAINT [FK_WorkforcePlanLine_PositionClass_PositionClassId] FOREIGN KEY ([PositionClassId]) REFERENCES [Hrms].[PositionClass] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Hrms].[WorkforcePlanLine] ADD CONSTRAINT [FK_WorkforcePlanLine_WorkforcePlan_PlanId] FOREIGN KEY ([PlanId]) REFERENCES [Hrms].[WorkforcePlan] ([Id]) ON DELETE CASCADE;

ALTER TABLE [Hrms].[WorkLocation] ADD CONSTRAINT [FK_WorkLocation_WorkLocation_ParentId] FOREIGN KEY ([ParentId]) REFERENCES [Hrms].[WorkLocation] ([Id]) ON DELETE NO ACTION;
GO
IF OBJECT_ID('[Core].[hrms_ReportActivate]', 'P') IS NOT NULL DROP PROCEDURE [Core].[hrms_ReportActivate];
GO
IF OBJECT_ID('[Core].[hrms_ReportClientSchedule]', 'P') IS NOT NULL DROP PROCEDURE [Core].[hrms_ReportClientSchedule];
GO
IF OBJECT_ID('[Core].[hrms_ReportClientScheduleDelete]', 'P') IS NOT NULL DROP PROCEDURE [Core].[hrms_ReportClientScheduleDelete];
GO
IF OBJECT_ID('[Core].[hrms_ReportClientScheduleEnable]', 'P') IS NOT NULL DROP PROCEDURE [Core].[hrms_ReportClientScheduleEnable];
GO
IF OBJECT_ID('[Core].[hrms_ReportClientScheduleFieldOutput]', 'P') IS NOT NULL DROP PROCEDURE [Core].[hrms_ReportClientScheduleFieldOutput];
GO
IF OBJECT_ID('[Core].[hrms_ReportClientScheduleFieldValue]', 'P') IS NOT NULL DROP PROCEDURE [Core].[hrms_ReportClientScheduleFieldValue];
GO
IF OBJECT_ID('[Core].[hrms_ReportClientScheduleRead]', 'P') IS NOT NULL DROP PROCEDURE [Core].[hrms_ReportClientScheduleRead];
GO
IF OBJECT_ID('[Core].[hrms_ReportClientScheduleRecipient]', 'P') IS NOT NULL DROP PROCEDURE [Core].[hrms_ReportClientScheduleRecipient];
GO
IF OBJECT_ID('[Core].[hrms_ReportDelete]', 'P') IS NOT NULL DROP PROCEDURE [Core].[hrms_ReportDelete];
GO
IF OBJECT_ID('[Core].[hrms_ReportFieldOutputRead]', 'P') IS NOT NULL DROP PROCEDURE [Core].[hrms_ReportFieldOutputRead];
GO
IF OBJECT_ID('[Core].[hrms_ReportFieldValues]', 'P') IS NOT NULL DROP PROCEDURE [Core].[hrms_ReportFieldValues];
GO
IF OBJECT_ID('[Core].[hrms_ReportGenerateGetScheduleInfo]', 'P') IS NOT NULL DROP PROCEDURE [Core].[hrms_ReportGenerateGetScheduleInfo];
GO
IF OBJECT_ID('[Core].[hrms_ReportGenerateSendToHistory]', 'P') IS NOT NULL DROP PROCEDURE [Core].[hrms_ReportGenerateSendToHistory];
GO
IF OBJECT_ID('[Core].[hrms_Report_DisciplinaryCases]', 'P') IS NOT NULL DROP PROCEDURE [Core].[hrms_Report_DisciplinaryCases];
GO
IF OBJECT_ID('[Core].[hrms_Report_EmployeeDemographics]', 'P') IS NOT NULL DROP PROCEDURE [Core].[hrms_Report_EmployeeDemographics];
GO
IF OBJECT_ID('[Core].[hrms_Report_EmployeeDirectory]', 'P') IS NOT NULL DROP PROCEDURE [Core].[hrms_Report_EmployeeDirectory];
GO
IF OBJECT_ID('[Core].[hrms_Report_EmployeeDirectoryGrouped]', 'P') IS NOT NULL DROP PROCEDURE [Core].[hrms_Report_EmployeeDirectoryGrouped];
GO
IF OBJECT_ID('[Core].[hrms_Report_EmployeeMovements]', 'P') IS NOT NULL DROP PROCEDURE [Core].[hrms_Report_EmployeeMovements];
GO
IF OBJECT_ID('[Core].[hrms_Report_HeadcountByUnit]', 'P') IS NOT NULL DROP PROCEDURE [Core].[hrms_Report_HeadcountByUnit];
GO
IF OBJECT_ID('[Core].[hrms_Report_LeaveBalances]', 'P') IS NOT NULL DROP PROCEDURE [Core].[hrms_Report_LeaveBalances];
GO
IF OBJECT_ID('[Core].[hrms_Report_LeaveTaken]', 'P') IS NOT NULL DROP PROCEDURE [Core].[hrms_Report_LeaveTaken];
GO
IF OBJECT_ID('[Core].[hrms_Report_NewHires]', 'P') IS NOT NULL DROP PROCEDURE [Core].[hrms_Report_NewHires];
GO
IF OBJECT_ID('[Core].[hrms_Report_ProbationTracking]', 'P') IS NOT NULL DROP PROCEDURE [Core].[hrms_Report_ProbationTracking];
GO
IF OBJECT_ID('[Core].[hrms_Report_RecruitmentPipeline]', 'P') IS NOT NULL DROP PROCEDURE [Core].[hrms_Report_RecruitmentPipeline];
GO
IF OBJECT_ID('[Core].[hrms_Report_SalaryRegister]', 'P') IS NOT NULL DROP PROCEDURE [Core].[hrms_Report_SalaryRegister];
GO
IF OBJECT_ID('[Core].[hrms_Report_Terminations]', 'P') IS NOT NULL DROP PROCEDURE [Core].[hrms_Report_Terminations];
GO
IF OBJECT_ID('[Core].[hrms_Report_TrainingCompletion]', 'P') IS NOT NULL DROP PROCEDURE [Core].[hrms_Report_TrainingCompletion];
GO
IF OBJECT_ID('[Core].[hrms_Report_VacantPositions]', 'P') IS NOT NULL DROP PROCEDURE [Core].[hrms_Report_VacantPositions];
GO
IF OBJECT_ID('[Hrms].[ReportActivate]', 'P') IS NOT NULL DROP PROCEDURE [Hrms].[ReportActivate];
GO
CREATE OR ALTER PROCEDURE [Hrms].[ReportActivate]
    @ReportId UNIQUEIDENTIFIER,
    @IsActive BIT,
    @TenantId NVARCHAR(450) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Hrms.Report
       SET IsActive = @IsActive,
           UpdatedAt = SYSUTCDATETIME(),
           RowVersion = CONVERT(varbinary(8), NEWID())
     WHERE Id = @ReportId
       AND (@TenantId IS NULL OR @TenantId = '' OR TenantId = @TenantId);
END
GO
IF OBJECT_ID('[Hrms].[ReportClientSchedule]', 'P') IS NOT NULL DROP PROCEDURE [Hrms].[ReportClientSchedule];
GO
CREATE OR ALTER PROCEDURE [Hrms].[ReportClientSchedule]
    @ReportScheduleId UNIQUEIDENTIFIER OUTPUT,
    @TenantId NVARCHAR(450),
    @UserId UNIQUEIDENTIFIER = NULL,
    @ReportId UNIQUEIDENTIFIER,
    @Name NVARCHAR(200),
    @IsScheduled BIT,
    @MailSubject NVARCHAR(300) = NULL,
    @MailBody NVARCHAR(MAX) = NULL,
    @IsHideRecipients BIT = 0,
    @Frequency NVARCHAR(20),
    @FrequencyWeekly INT = 0,
    @TimeOfTheDay INT = 0,
    @ScheduleStartDate DATE = NULL,
    @OutputFormat INT = 1,
    @CronExpression NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    IF @ReportScheduleId IS NULL
       OR NOT EXISTS (SELECT 1 FROM Hrms.ReportSchedule WHERE Id = @ReportScheduleId)
    BEGIN
        SET @ReportScheduleId = NEWID();
        INSERT INTO Hrms.ReportSchedule
            (Id, TenantId, ReportId, Name, IsScheduled, IsActive, MailSubject, MailBody, IsHideRecipients,
             Frequency, FrequencyWeekly, TimeOfTheDay, ScheduleStartDate, OutputFormat, CronExpression,
             CreatedAt, RowVersion)
        VALUES
            (@ReportScheduleId, @TenantId, @ReportId, @Name, @IsScheduled, 1, @MailSubject, @MailBody, @IsHideRecipients,
             @Frequency, @FrequencyWeekly, @TimeOfTheDay, @ScheduleStartDate, @OutputFormat, @CronExpression,
             SYSUTCDATETIME(), CONVERT(varbinary(8), NEWID()));
    END
    ELSE
    BEGIN
        UPDATE Hrms.ReportSchedule
           SET Name = @Name, IsScheduled = @IsScheduled, MailSubject = @MailSubject, MailBody = @MailBody,
               IsHideRecipients = @IsHideRecipients, Frequency = @Frequency, FrequencyWeekly = @FrequencyWeekly,
               TimeOfTheDay = @TimeOfTheDay, ScheduleStartDate = @ScheduleStartDate, OutputFormat = @OutputFormat,
               CronExpression = @CronExpression, UpdatedAt = SYSUTCDATETIME(), RowVersion = CONVERT(varbinary(8), NEWID())
         WHERE Id = @ReportScheduleId;
    END
END
GO
IF OBJECT_ID('[Hrms].[ReportClientScheduleDelete]', 'P') IS NOT NULL DROP PROCEDURE [Hrms].[ReportClientScheduleDelete];
GO
CREATE OR ALTER PROCEDURE [Hrms].[ReportClientScheduleDelete]
    @ReportScheduleId UNIQUEIDENTIFIER,
    @IsModifyOnly INT = 0
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM Hrms.ReportScheduleRecipient  WHERE ReportScheduleId = @ReportScheduleId;
    DELETE FROM Hrms.ReportScheduleFieldValue WHERE ReportScheduleId = @ReportScheduleId;
    DELETE FROM Hrms.ReportScheduleFieldOutput WHERE ReportScheduleId = @ReportScheduleId;
    IF @IsModifyOnly = 0
        DELETE FROM Hrms.ReportSchedule WHERE Id = @ReportScheduleId;
END
GO
IF OBJECT_ID('[Hrms].[ReportClientScheduleEnable]', 'P') IS NOT NULL DROP PROCEDURE [Hrms].[ReportClientScheduleEnable];
GO
CREATE OR ALTER PROCEDURE [Hrms].[ReportClientScheduleEnable]
    @ReportScheduleId UNIQUEIDENTIFIER,
    @Enabled INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Hrms.ReportSchedule
       SET IsActive = CASE WHEN @Enabled = 1 THEN 1 ELSE 0 END,
           UpdatedAt = SYSUTCDATETIME(), RowVersion = CONVERT(varbinary(8), NEWID())
     WHERE Id = @ReportScheduleId;
END
GO
IF OBJECT_ID('[Hrms].[ReportClientScheduleFieldOutput]', 'P') IS NOT NULL DROP PROCEDURE [Hrms].[ReportClientScheduleFieldOutput];
GO
CREATE OR ALTER PROCEDURE [Hrms].[ReportClientScheduleFieldOutput]
    @ReportScheduleId UNIQUEIDENTIFIER,
    @ReportKey NVARCHAR(100),
    @Field NVARCHAR(100),
    @Label NVARCHAR(200),
    @FieldOrder INT = 0,
    @SortOrder INT = 0
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Tenant NVARCHAR(450) = (SELECT TOP 1 TenantId FROM Hrms.ReportSchedule WHERE Id = @ReportScheduleId);
    INSERT INTO Hrms.ReportScheduleFieldOutput
        (Id, ReportScheduleId, ReportKey, Field, Label, FieldOrder, SortOrder, TenantId, CreatedAt, RowVersion)
    VALUES
        (NEWID(), @ReportScheduleId, @ReportKey, @Field, @Label, @FieldOrder, @SortOrder, @Tenant, SYSUTCDATETIME(), CONVERT(varbinary(8), NEWID()));
END
GO
IF OBJECT_ID('[Hrms].[ReportClientScheduleFieldValue]', 'P') IS NOT NULL DROP PROCEDURE [Hrms].[ReportClientScheduleFieldValue];
GO
CREATE OR ALTER PROCEDURE [Hrms].[ReportClientScheduleFieldValue]
    @ReportScheduleId UNIQUEIDENTIFIER,
    @ReportKey NVARCHAR(100),
    @Field NVARCHAR(100),
    @Value NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Tenant NVARCHAR(450) = (SELECT TOP 1 TenantId FROM Hrms.ReportSchedule WHERE Id = @ReportScheduleId);
    INSERT INTO Hrms.ReportScheduleFieldValue
        (Id, ReportScheduleId, ReportKey, Field, Value, TenantId, CreatedAt, RowVersion)
    VALUES
        (NEWID(), @ReportScheduleId, @ReportKey, @Field, @Value, @Tenant, SYSUTCDATETIME(), CONVERT(varbinary(8), NEWID()));
END
GO
IF OBJECT_ID('[Hrms].[ReportClientScheduleRead]', 'P') IS NOT NULL DROP PROCEDURE [Hrms].[ReportClientScheduleRead];
GO
CREATE OR ALTER PROCEDURE [Hrms].[ReportClientScheduleRead]
    @Type NVARCHAR(20),
    @Id UNIQUEIDENTIFIER,
    @TenantId NVARCHAR(450) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    IF @Type = 'Read'
        SELECT s.Id AS ReportScheduleId, s.ReportId, r.ReportKey, r.ReportName, s.Name,
               s.IsScheduled, s.IsActive, s.MailSubject, s.MailBody, s.IsHideRecipients,
               s.Frequency, s.FrequencyWeekly, s.TimeOfTheDay, s.ScheduleStartDate,
               s.OutputFormat, s.CronExpression, r.StoredProc
          FROM Hrms.ReportSchedule s
          JOIN Hrms.Report r ON r.Id = s.ReportId
         WHERE s.Id = @Id
           AND (@TenantId IS NULL OR @TenantId = '' OR s.TenantId = @TenantId);
    ELSE
        SELECT s.Id AS ReportScheduleId, s.ReportId, r.ReportKey, r.ReportName, s.Name,
               s.IsScheduled, s.IsActive, s.MailSubject, s.MailBody, s.IsHideRecipients,
               s.Frequency, s.FrequencyWeekly, s.TimeOfTheDay, s.ScheduleStartDate,
               s.OutputFormat, s.CronExpression, r.StoredProc
          FROM Hrms.ReportSchedule s
          JOIN Hrms.Report r ON r.Id = s.ReportId
         WHERE s.ReportId = @Id
           AND (@TenantId IS NULL OR @TenantId = '' OR s.TenantId = @TenantId)
         ORDER BY s.Name;
END
GO
IF OBJECT_ID('[Hrms].[ReportClientScheduleRecipient]', 'P') IS NOT NULL DROP PROCEDURE [Hrms].[ReportClientScheduleRecipient];
GO
CREATE OR ALTER PROCEDURE [Hrms].[ReportClientScheduleRecipient]
    @Type NVARCHAR(20),
    @ReportScheduleId UNIQUEIDENTIFIER,
    @UserId UNIQUEIDENTIFIER = NULL,
    @RoleId UNIQUEIDENTIFIER = NULL,
    @Email NVARCHAR(300) = NULL,
    @TenantId NVARCHAR(450) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    IF @Type = 'Add'
    BEGIN
        DECLARE @Tenant NVARCHAR(450) =
            COALESCE(NULLIF(@TenantId, ''), (SELECT TOP 1 TenantId FROM Hrms.ReportSchedule WHERE Id = @ReportScheduleId));
        DECLARE @ResolvedEmail NVARCHAR(300) = @Email;
        IF @ResolvedEmail IS NULL AND @UserId IS NOT NULL
            SET @ResolvedEmail = (SELECT TOP 1 Email FROM Core.[User] WHERE Id = @UserId);
        INSERT INTO Hrms.ReportScheduleRecipient
            (Id, ReportScheduleId, UserId, RoleId, Email, TenantId, CreatedAt, RowVersion)
        VALUES
            (NEWID(), @ReportScheduleId, @UserId, @RoleId, @ResolvedEmail, @Tenant, SYSUTCDATETIME(), CONVERT(varbinary(8), NEWID()));
    END
    ELSE IF @Type = 'ListUsers'
    BEGIN
        SELECT @ReportScheduleId AS ReportScheduleId, u.Id AS UserId, u.UserName AS UserName,
               CAST(CASE WHEN r.Id IS NOT NULL THEN 1 ELSE 0 END AS BIT) AS IsAssigned, u.Email AS Email
        FROM Core.[User] u
        LEFT JOIN Hrms.ReportScheduleRecipient r
            ON r.ReportScheduleId = @ReportScheduleId AND r.UserId = u.Id
        WHERE (@TenantId IS NULL OR @TenantId = '' OR u.TenantId = @TenantId)
        ORDER BY u.UserName;
    END
    ELSE IF @Type = 'ListRoles'
    BEGIN
        SELECT @ReportScheduleId AS ReportScheduleId, ro.Id AS RoleId, ro.Name AS RoleName,
               CAST(CASE WHEN r.Id IS NOT NULL THEN 1 ELSE 0 END AS BIT) AS IsAssigned
        FROM Core.Role ro
        LEFT JOIN Hrms.ReportScheduleRecipient r
            ON r.ReportScheduleId = @ReportScheduleId AND r.RoleId = ro.Id
        WHERE (@TenantId IS NULL OR @TenantId = '' OR ro.TenantId = @TenantId)
        ORDER BY ro.Name;
    END
END
GO
IF OBJECT_ID('[Hrms].[ReportDelete]', 'P') IS NOT NULL DROP PROCEDURE [Hrms].[ReportDelete];
GO
CREATE OR ALTER PROCEDURE [Hrms].[ReportDelete]
    @ReportId UNIQUEIDENTIFIER,
    @TenantId NVARCHAR(450) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM Hrms.Report
     WHERE Id = @ReportId
       AND (@TenantId IS NULL OR @TenantId = '' OR TenantId = @TenantId);
END
GO
IF OBJECT_ID('[Hrms].[ReportFieldOutputRead]', 'P') IS NOT NULL DROP PROCEDURE [Hrms].[ReportFieldOutputRead];
GO
CREATE OR ALTER PROCEDURE [Hrms].[ReportFieldOutputRead]
    @ReportKey NVARCHAR(100),
    @ReportScheduleId UNIQUEIDENTIFIER = NULL,
    @TenantId NVARCHAR(450) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @ReportId UNIQUEIDENTIFIER =
        (SELECT TOP 1 Id FROM Hrms.Report
          WHERE ReportKey = @ReportKey
            AND (@TenantId IS NULL OR @TenantId = '' OR TenantId = @TenantId));

    SELECT
        CASE WHEN @ReportScheduleId IS NULL THEN 1
             WHEN so.Id IS NOT NULL THEN 1 ELSE 0 END AS IsShow,
        fo.Field AS Field,
        COALESCE(so.Label, fo.Label) AS Label,
        COALESCE(so.SortOrder, 0) AS SortOrder,
        COALESCE(so.FieldOrder, fo.FieldOrder) AS FieldOrder
    FROM Hrms.ReportFieldOutput fo
    LEFT JOIN Hrms.ReportScheduleFieldOutput so
        ON so.ReportScheduleId = @ReportScheduleId AND so.Field = fo.Field
    WHERE fo.ReportId = @ReportId
    ORDER BY COALESCE(so.FieldOrder, fo.FieldOrder), fo.Label;
END
GO
IF OBJECT_ID('[Hrms].[ReportFieldValues]', 'P') IS NOT NULL DROP PROCEDURE [Hrms].[ReportFieldValues];
GO
CREATE OR ALTER PROCEDURE [Hrms].[ReportFieldValues]
    @TenantId  NVARCHAR(64),
    @BranchId  UNIQUEIDENTIFIER = NULL,
    @UserId    UNIQUEIDENTIFIER = NULL,
    @ReportKey NVARCHAR(100),
    @Field     NVARCHAR(100),
    @Dependency NVARCHAR(400) = NULL,
    @Search    NVARCHAR(200) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @Field = '@DynamicDate'
        SELECT v.Value, v.Label
        FROM (VALUES
            (1,  'Today',            'Today'),
            (2,  'Yesterday',        'Yesterday'),
            (3,  'Tomorrow',         'Tomorrow'),
            (4,  'StartOfWeek',      'Start of this week'),
            (5,  'EndOfWeek',        'End of this week'),
            (6,  'StartOfMonth',     'Start of this month'),
            (7,  'EndOfMonth',       'End of this month'),
            (8,  'StartOfLastMonth', 'Start of last month'),
            (9,  'EndOfLastMonth',   'End of last month'),
            (10, 'StartOfQuarter',   'Start of this quarter'),
            (11, 'EndOfQuarter',     'End of this quarter'),
            (12, 'StartOfYear',      'Start of this year'),
            (13, 'EndOfYear',        'End of this year'),
            (14, 'Last7Days',        '7 days ago'),
            (15, 'Last30Days',       '30 days ago'),
            (16, 'Last90Days',       '90 days ago')
        ) v(Seq, Value, Label)
        WHERE @Search IS NULL OR v.Label LIKE '%' + @Search + '%'
        ORDER BY v.Seq;
    ELSE IF @Field = 'OrganizationUnitId'
        SELECT CAST(Id AS NVARCHAR(50)) AS Value, Name AS Label
        FROM Hrms.OrganizationUnit
        WHERE TenantId = @TenantId AND IsActive = 1
          AND (@BranchId IS NULL OR BranchId = @BranchId OR BranchId IS NULL)
          AND (@Search IS NULL OR Name LIKE '%' + @Search + '%')
        ORDER BY Name;
    ELSE IF @Field = 'EmploymentStatus'
        SELECT v.Value, v.Value AS Label
        FROM (VALUES ('Active'),('Probation'),('OnLeave'),('Suspended'),('Terminated'),('Retired')) v(Value);
    ELSE IF @Field = 'LeaveTypeId'
        SELECT CAST(Id AS NVARCHAR(50)) AS Value, Name AS Label
        FROM Hrms.LeaveType
        WHERE TenantId = @TenantId AND IsActive = 1
          AND (@Search IS NULL OR Name LIKE '%' + @Search + '%')
        ORDER BY Name;
    ELSE IF @Field = 'BranchId'
        SELECT CAST(Id AS NVARCHAR(50)) AS Value, Name AS Label
        FROM Hrms.Branch
        WHERE TenantId = @TenantId AND IsActive = 1
          AND (@Search IS NULL OR Name LIKE '%' + @Search + '%')
        ORDER BY Name;
    ELSE IF @Field = 'PositionClassId'
        SELECT CAST(Id AS NVARCHAR(50)) AS Value, Title AS Label
        FROM Hrms.PositionClass
        WHERE TenantId = @TenantId AND IsActive = 1
          AND (@Search IS NULL OR Title LIKE '%' + @Search + '%')
        ORDER BY Title;
    ELSE IF @Field = 'EmploymentNature'
        SELECT v.Value, v.Value AS Label
        FROM (VALUES ('Permanent'),('Contract')) v(Value);
    ELSE IF @Field = 'Gender'
        SELECT v.Value, v.Value AS Label
        FROM (VALUES ('Male'),('Female')) v(Value);
    ELSE IF @Field = 'MovementType'
        SELECT v.Value, v.Value AS Label
        FROM (VALUES ('Transfer'),('Promotion'),('Demotion')) v(Value);
    ELSE IF @Field = 'MovementStatus'
        SELECT v.Value, v.Value AS Label
        FROM (VALUES ('Pending'),('Approved'),('Completed'),('Cancelled')) v(Value);
    ELSE IF @Field = 'TerminationType'
        SELECT v.Value, v.Value AS Label
        FROM (VALUES ('Voluntary'),('Involuntary')) v(Value);
    ELSE IF @Field = 'TerminationStatus'
        SELECT v.Value, v.Value AS Label
        FROM (VALUES ('Initiated'),('ClearanceInProgress'),('Settled'),('Cancelled')) v(Value);
    ELSE IF @Field = 'FiscalYearId'
        SELECT CAST(Id AS NVARCHAR(50)) AS Value, Name AS Label
        FROM Core.FiscalYear
        WHERE TenantId = @TenantId
          AND (@Search IS NULL OR Name LIKE '%' + @Search + '%')
        ORDER BY StartDate DESC;
    ELSE IF @Field = 'JobGradeId'
        SELECT CAST(Id AS NVARCHAR(50)) AS Value, Name AS Label
        FROM Hrms.JobGrade
        WHERE TenantId = @TenantId
          AND (@Search IS NULL OR Name LIKE '%' + @Search + '%')
        ORDER BY Name;
    ELSE IF @Field = 'LeaveStatus'
        SELECT v.Value, v.Value AS Label
        FROM (VALUES ('Draft'),('Pending'),('Approved'),('Rejected'),('Cancelled')) v(Value);
    ELSE IF @Field = 'TrainingCourseId'
        SELECT CAST(Id AS NVARCHAR(50)) AS Value, Name AS Label
        FROM Hrms.TrainingCourse
        WHERE TenantId = @TenantId AND IsActive = 1
          AND (@Search IS NULL OR Name LIKE '%' + @Search + '%')
        ORDER BY Name;
    ELSE IF @Field = 'EnrollmentStatus'
        SELECT v.Value, v.Value AS Label
        FROM (VALUES ('Enrolled'),('Completed'),('NoShow'),('Withdrawn')) v(Value);
    ELSE IF @Field = 'ApplicationStage'
        SELECT v.Value, v.Value AS Label
        FROM (VALUES ('Received'),('Screening'),('Shortlisted'),('Interview'),('Selected'),('OfferPending'),('OfferAccepted'),('Hired'),('Rejected'),('Withdrawn')) v(Value);
    ELSE IF @Field = 'RequisitionStatus'
        SELECT v.Value, v.Value AS Label
        FROM (VALUES ('Draft'),('PendingApproval'),('Approved'),('Posted'),('Closed'),('Cancelled'),('Rejected')) v(Value);
    ELSE IF @Field = 'MeasureType'
        SELECT v.Value, v.Value AS Label
        FROM (VALUES ('VerbalWarning'),('WrittenWarning'),('FinalWarning'),('Suspension'),('SalaryDeduction'),('Demotion'),('Termination')) v(Value);
    ELSE IF @Field = 'DisciplinaryStatus'
        SELECT v.Value, v.Value AS Label
        FROM (VALUES ('Open'),('UnderReview'),('Resolved'),('Cancelled')) v(Value);
    ELSE
        SELECT CAST(NULL AS NVARCHAR(50)) AS Value, CAST(NULL AS NVARCHAR(200)) AS Label
        WHERE 1 = 0;
END
GO
IF OBJECT_ID('[Hrms].[ReportGenerateGetScheduleInfo]', 'P') IS NOT NULL DROP PROCEDURE [Hrms].[ReportGenerateGetScheduleInfo];
GO
CREATE OR ALTER PROCEDURE [Hrms].[ReportGenerateGetScheduleInfo]
    @TenantId NVARCHAR(450) = NULL,
    @ReportScheduleId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT s.Id AS ReportScheduleId, s.ReportId, r.ReportKey, r.ReportName, s.Name,
           s.IsScheduled, s.IsActive, s.MailSubject, s.MailBody, s.IsHideRecipients,
           s.Frequency, s.FrequencyWeekly, s.TimeOfTheDay, s.ScheduleStartDate,
           s.OutputFormat, s.CronExpression, r.StoredProc
      FROM Hrms.ReportSchedule s
      JOIN Hrms.Report r ON r.Id = s.ReportId
     WHERE s.Id = @ReportScheduleId
       AND (@TenantId IS NULL OR @TenantId = '' OR s.TenantId = @TenantId);

    SELECT Field, Value FROM Hrms.ReportScheduleFieldValue WHERE ReportScheduleId = @ReportScheduleId;

    SELECT DISTINCT e.Email FROM (
        SELECT rec.Email AS Email
          FROM Hrms.ReportScheduleRecipient rec
         WHERE rec.ReportScheduleId = @ReportScheduleId AND rec.Email IS NOT NULL AND rec.Email <> ''
        UNION
        SELECT u.Email
          FROM Hrms.ReportScheduleRecipient rec
          JOIN Core.[User] u ON u.Id = rec.UserId
         WHERE rec.ReportScheduleId = @ReportScheduleId
        UNION
        SELECT u.Email
          FROM Hrms.ReportScheduleRecipient rec
          JOIN Core.UserRole ur ON ur.RoleId = rec.RoleId
          JOIN Core.[User] u ON u.Id = ur.UserId
         WHERE rec.ReportScheduleId = @ReportScheduleId
    ) e
    WHERE e.Email IS NOT NULL AND e.Email <> '';

    SELECT 1 AS IsShow, Field, Label, SortOrder, FieldOrder
      FROM Hrms.ReportScheduleFieldOutput
     WHERE ReportScheduleId = @ReportScheduleId
     ORDER BY FieldOrder;
END
GO
IF OBJECT_ID('[Hrms].[ReportGenerateSendToHistory]', 'P') IS NOT NULL DROP PROCEDURE [Hrms].[ReportGenerateSendToHistory];
GO
CREATE OR ALTER PROCEDURE [Hrms].[ReportGenerateSendToHistory]
    @TenantId NVARCHAR(450),
    @ReportKey NVARCHAR(100),
    @IsScheduled BIT = 0,
    @Criteria NVARCHAR(MAX) = NULL,
    @FieldOutput NVARCHAR(MAX) = NULL,
    @TotalRecords INT = 0,
    @RunSeconds INT = 0,
    @RanBy NVARCHAR(200) = NULL,
    @Recipients NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @RunId UNIQUEIDENTIFIER = NEWID();
    INSERT INTO Hrms.ReportRun
        (Id, TenantId, ReportKey, CriteriaJson, [RowCount], DurationMs, RanBy, IsScheduled, FieldOutput, CreatedAt, RowVersion)
    VALUES
        (@RunId, @TenantId, @ReportKey, ISNULL(@Criteria, '{}'), @TotalRecords, @RunSeconds * 1000, @RanBy,
         @IsScheduled, @FieldOutput, SYSUTCDATETIME(), CONVERT(varbinary(8), NEWID()));

    IF @Recipients IS NOT NULL AND @Recipients <> ''
        INSERT INTO Hrms.ReportRunRecipient (Id, ReportRunId, UserId, Email, TenantId, CreatedAt, RowVersion)
        SELECT NEWID(), @RunId, NULL, LTRIM(RTRIM(value)), @TenantId, SYSUTCDATETIME(), CONVERT(varbinary(8), NEWID())
          FROM STRING_SPLIT(@Recipients, ';')
         WHERE LTRIM(RTRIM(value)) <> '';
END
GO
IF OBJECT_ID('[Hrms].[Report_DisciplinaryCases]', 'P') IS NOT NULL DROP PROCEDURE [Hrms].[Report_DisciplinaryCases];
GO
CREATE OR ALTER PROCEDURE [Hrms].[Report_DisciplinaryCases]
    @TenantId     NVARCHAR(64),
    @BranchId     UNIQUEIDENTIFIER = NULL,
    @UserId       UNIQUEIDENTIFIER = NULL,
    @ReportKey    NVARCHAR(100),
    @Criteria     NVARCHAR(MAX) = NULL,
    @OutputFields NVARCHAR(MAX) = NULL,
    @Source       NVARCHAR(20)  = NULL,
    @Roles        NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @viol1   DATE          = TRY_CONVERT(DATE, JSON_VALUE(@Criteria, '$.ViolationDate1'));
    DECLARE @viol2   DATE          = TRY_CONVERT(DATE, JSON_VALUE(@Criteria, '$.ViolationDate2'));
    DECLARE @measure NVARCHAR(30)  = NULLIF(JSON_VALUE(@Criteria, '$.MeasureType'), '');
    DECLARE @dstat   NVARCHAR(30)  = NULLIF(JSON_VALUE(@Criteria, '$.DisciplinaryStatus'), '');
    DECLARE @unitIds NVARCHAR(MAX) = NULLIF(JSON_VALUE(@Criteria, '$.OrganizationUnitId'), '');
    DECLARE @useOutputs BIT        = CASE WHEN ISNULL(@OutputFields, '') IN ('', '[]') THEN 0 ELSE 1 END;

    SELECT c.Field, COALESCE(NULLIF(o.Label, ''), c.Label) AS Label, c.[Type], c.Width, c.LinkPage, c.LinkPageValue
    FROM (VALUES
        (1, 'EmployeeNumber',     'Employee #',        'string',  120, CAST(NULL AS NVARCHAR(100)), CAST(NULL AS NVARCHAR(100))),
        (2, 'FullName',           'Full Name',         'string',  220, NULL, NULL),
        (3, 'UnitName',           'Unit',              'string',  180, NULL, NULL),
        (4, 'ViolationDate',      'Violation Date',    'date',    120, NULL, NULL),
        (5, 'ViolationType',      'Violation',         'string',  160, NULL, NULL),
        (6, 'MeasureType',        'Measure',           'string',  140, NULL, NULL),
        (7, 'DisciplinaryStatus', 'Case Status',       'string',  110, NULL, NULL),
        (8, 'EffectiveDate',      'Effective',         'date',    110, NULL, NULL),
        (9, 'ValidUntil',         'Valid Until',       'date',    110, NULL, NULL),
        (10,'AffectsPromotion',   'Blocks Promotion',  'boolean', 120, NULL, NULL),
        (11,'AffectsReward',      'Blocks Reward',     'boolean', 110, NULL, NULL),
        (12,'Resolution',         'Resolution',        'string',  250, NULL, NULL)
    ) c(Seq, Field, Label, [Type], Width, LinkPage, LinkPageValue)
    LEFT JOIN OPENJSON(@OutputFields)
        WITH (Field NVARCHAR(100) '$.Field', Label NVARCHAR(200) '$.Label', [Order] INT '$.Order') o
        ON o.Field = c.Field
    WHERE @useOutputs = 0 OR o.Field IS NOT NULL
    ORDER BY CASE WHEN @useOutputs = 1 THEN o.[Order] ELSE c.Seq END;

    DECLARE @orderby NVARCHAR(MAX) = STUFF((
        SELECT ',' + QUOTENAME(o.Field)
        FROM OPENJSON(@OutputFields)
            WITH (Field NVARCHAR(100) '$.Field', SortOrder INT '$.SortOrder') o
        WHERE @useOutputs = 1 AND o.SortOrder > 0
          AND o.Field IN ('EmployeeNumber','FullName','UnitName','ViolationDate','ViolationType','MeasureType','DisciplinaryStatus','EffectiveDate','ValidUntil','AffectsPromotion','AffectsReward','Resolution')
        ORDER BY o.SortOrder
        FOR XML PATH('')), 1, 1, '');
    IF @orderby IS NULL OR @orderby = '' SET @orderby = N'[ViolationDate] DESC, [EmployeeNumber]';

    DECLARE @sql NVARCHAR(MAX) = N'
        SELECT e.EmployeeNumber,
               LTRIM(RTRIM(CONCAT(p.FirstName, '' '', p.GrandFatherName))) AS FullName,
               ou.Name AS UnitName,
               d.ViolationDate,
               d.ViolationType,
               d.MeasureType,
               d.Status AS DisciplinaryStatus,
               d.EffectiveDate,
               d.ValidUntil,
               d.AffectsPromotion,
               d.AffectsReward,
               d.Resolution
        FROM Hrms.DisciplinaryMeasure d
        INNER JOIN Hrms.Employee e         ON e.Id   = d.EmployeeId
        LEFT JOIN Core.Person p           ON p.Id   = e.PersonId
        LEFT JOIN Hrms.Position pos        ON pos.Id = e.PositionId
        LEFT JOIN Hrms.OrganizationUnit ou ON ou.Id  = pos.OrganizationUnitId
        WHERE d.TenantId = @TenantId
          AND (@BranchId IS NULL OR e.BranchId = @BranchId)
          AND (@viol1   IS NULL OR d.ViolationDate >= @viol1)
          AND (@viol2   IS NULL OR d.ViolationDate <  DATEADD(DAY, 1, @viol2))
          AND (@measure IS NULL OR d.MeasureType = @measure)
          AND (@dstat   IS NULL OR d.Status = @dstat)
          AND (@unitIds IS NULL OR pos.OrganizationUnitId IN
               (SELECT TRY_CONVERT(UNIQUEIDENTIFIER, LTRIM(RTRIM(value))) FROM STRING_SPLIT(@unitIds, '','')))
        ORDER BY ' + @orderby + N';';

    EXEC sp_executesql @sql,
        N'@TenantId NVARCHAR(64), @BranchId UNIQUEIDENTIFIER, @viol1 DATE, @viol2 DATE, @measure NVARCHAR(30), @dstat NVARCHAR(30), @unitIds NVARCHAR(MAX)',
        @TenantId, @BranchId, @viol1, @viol2, @measure, @dstat, @unitIds;
END
GO
IF OBJECT_ID('[Hrms].[Report_EmployeeDemographics]', 'P') IS NOT NULL DROP PROCEDURE [Hrms].[Report_EmployeeDemographics];
GO
CREATE OR ALTER PROCEDURE [Hrms].[Report_EmployeeDemographics]
    @TenantId     NVARCHAR(64),
    @BranchId     UNIQUEIDENTIFIER = NULL,
    @UserId       UNIQUEIDENTIFIER = NULL,
    @ReportKey    NVARCHAR(100),
    @Criteria     NVARCHAR(MAX) = NULL,
    @OutputFields NVARCHAR(MAX) = NULL,
    @Source       NVARCHAR(20)  = NULL,
    @Roles        NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @unitIds NVARCHAR(MAX) = NULLIF(JSON_VALUE(@Criteria, '$.OrganizationUnitId'), '');
    DECLARE @status  NVARCHAR(30)  = NULLIF(JSON_VALUE(@Criteria, '$.EmploymentStatus'), '');
    DECLARE @useOutputs BIT        = CASE WHEN ISNULL(@OutputFields, '') IN ('', '[]') THEN 0 ELSE 1 END;

    DECLARE @groupBy     NVARCHAR(MAX) = NULLIF(JSON_VALUE(@Criteria, '$.__groupBy'), '');
    DECLARE @dir         NVARCHAR(4)   = CASE WHEN LOWER(ISNULL(JSON_VALUE(@Criteria, '$.__groupOrder'), 'asc')) = 'desc' THEN 'DESC' ELSE 'ASC' END;
    DECLARE @showSummary BIT           = CASE WHEN JSON_VALUE(@Criteria, '$.__showSummary') IN ('true','1') THEN 1 ELSE 0 END;
    IF @groupBy IS NULL SET @groupBy = 'Gender';

    DECLARE @gbJson NVARCHAR(MAX) = '["' + REPLACE(REPLACE(@groupBy, ' ', ''), ',', '","') + '"]';
    DECLARE @groups TABLE (Lvl INT, Field NVARCHAR(100));
    INSERT INTO @groups (Lvl, Field)
    SELECT CAST([key] AS INT), [value]
    FROM OPENJSON(@gbJson)
    WHERE [value] IN ('Gender', 'AgeBand', 'UnitName', 'EmploymentStatus');
    IF NOT EXISTS (SELECT 1 FROM @groups) INSERT INTO @groups (Lvl, Field) VALUES (0, 'Gender');

    DECLARE @groupSel  NVARCHAR(MAX) = STUFF((SELECT ',' + QUOTENAME(Field)             FROM @groups ORDER BY Lvl FOR XML PATH('')), 1, 1, '');
    DECLARE @orderCols NVARCHAR(MAX) = STUFF((SELECT ',' + QUOTENAME(Field) + ' ' + @dir FROM @groups ORDER BY Lvl FOR XML PATH('')), 1, 1, '');

    SELECT c.Field, COALESCE(NULLIF(o.Label, ''), c.Label) AS Label, c.[Type], c.Width, c.LinkPage, c.LinkPageValue
    FROM (VALUES
        (1, 'EmployeeNumber',   'Employee #', 'string', 120, CAST(NULL AS NVARCHAR(100)), CAST(NULL AS NVARCHAR(100))),
        (2, 'FullName',         'Full Name',  'string', 220, NULL, NULL),
        (3, 'Gender',           'Gender',     'string',  90, NULL, NULL),
        (4, 'Age',              'Age',        'number',  70, NULL, NULL),
        (5, 'AgeBand',          'Age Band',   'string', 100, NULL, NULL),
        (6, 'UnitName',         'Unit',       'string', 180, NULL, NULL),
        (7, 'EmploymentStatus', 'Status',     'string', 110, NULL, NULL),
        (8, 'HireDate',         'Hire Date',  'date',   110, NULL, NULL)
    ) c(Seq, Field, Label, [Type], Width, LinkPage, LinkPageValue)
    LEFT JOIN OPENJSON(@OutputFields)
        WITH (Field NVARCHAR(100) '$.Field', Label NVARCHAR(200) '$.Label', [Order] INT '$.Order') o
        ON o.Field = c.Field
    LEFT JOIN @groups g ON g.Field = c.Field
    WHERE g.Field IS NOT NULL OR @useOutputs = 0 OR o.Field IS NOT NULL
    ORDER BY CASE WHEN g.Field IS NOT NULL THEN g.Lvl
                  ELSE 1000 + (CASE WHEN @useOutputs = 1 THEN o.[Order] ELSE c.Seq END) END;

    -- Age computed birthday-accurate; bands follow the common enterprise demographic split.
    DECLARE @selectList NVARCHAR(MAX) = N'
        e.EmployeeNumber,
        LTRIM(RTRIM(CONCAT(p.FirstName, '' '', p.GrandFatherName))) AS FullName,
        p.Gender,
        ca.Age,
        CASE WHEN ca.Age IS NULL THEN ''Unknown''
             WHEN ca.Age < 25 THEN ''Under 25''
             WHEN ca.Age < 35 THEN ''25 - 34''
             WHEN ca.Age < 45 THEN ''35 - 44''
             WHEN ca.Age < 55 THEN ''45 - 54''
             ELSE ''55+'' END AS AgeBand,
        ou.Name AS UnitName,
        e.EmploymentStatus,
        e.HireDate';
    DECLARE @baseFrom NVARCHAR(MAX) = N'
        FROM Hrms.Employee e
        LEFT JOIN Core.Person p           ON p.Id   = e.PersonId
        LEFT JOIN Hrms.Position pos        ON pos.Id = e.PositionId
        LEFT JOIN Hrms.OrganizationUnit ou ON ou.Id  = pos.OrganizationUnitId
        CROSS APPLY (SELECT CASE WHEN e.DateOfBirth IS NULL THEN NULL
            ELSE DATEDIFF(YEAR, e.DateOfBirth, GETDATE())
                 - CASE WHEN DATEADD(YEAR, DATEDIFF(YEAR, e.DateOfBirth, GETDATE()), e.DateOfBirth) > GETDATE() THEN 1 ELSE 0 END
            END AS Age) ca
        WHERE e.TenantId = @TenantId
          AND (@BranchId IS NULL OR e.BranchId = @BranchId)
          AND (@unitIds  IS NULL OR pos.OrganizationUnitId IN
               (SELECT TRY_CONVERT(UNIQUEIDENTIFIER, LTRIM(RTRIM(value))) FROM STRING_SPLIT(@unitIds, '','')))
          AND ((@status IS NULL AND e.IsTerminated = 0) OR e.EmploymentStatus = @status)';
    DECLARE @paramDef NVARCHAR(MAX) =
        N'@TenantId NVARCHAR(64), @BranchId UNIQUEIDENTIFIER, @unitIds NVARCHAR(MAX), @status NVARCHAR(30)';

    DECLARE @sql NVARCHAR(MAX) = N'SELECT ' + @selectList + @baseFrom + N'
        ORDER BY ' + @orderCols + N', [EmployeeNumber];';
    EXEC sp_executesql @sql, @paramDef, @TenantId, @BranchId, @unitIds, @status;

    IF @showSummary = 1
    BEGIN
        DECLARE @sql3 NVARCHAR(MAX) = N'SELECT ' + @groupSel + N', COUNT(*) AS GroupCount
            FROM (SELECT ' + @selectList + @baseFrom + N') d
            GROUP BY ' + @groupSel + N'
            ORDER BY ' + @orderCols + N';';
        EXEC sp_executesql @sql3, @paramDef, @TenantId, @BranchId, @unitIds, @status;
    END
END
GO
IF OBJECT_ID('[Hrms].[Report_EmployeeDirectory]', 'P') IS NOT NULL DROP PROCEDURE [Hrms].[Report_EmployeeDirectory];
GO
CREATE OR ALTER PROCEDURE [Hrms].[Report_EmployeeDirectory]
    @TenantId  NVARCHAR(64),
    @BranchId  UNIQUEIDENTIFIER = NULL,
    @UserId    UNIQUEIDENTIFIER = NULL,
    @ReportKey NVARCHAR(100),
    @Criteria  NVARCHAR(MAX) = NULL,
    @OutputFields NVARCHAR(MAX) = NULL,
    @Source NVARCHAR(20) = NULL,
    @Roles NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @unitId UNIQUEIDENTIFIER = TRY_CONVERT(UNIQUEIDENTIFIER, JSON_VALUE(@Criteria, '$.OrganizationUnitId'));
    DECLARE @status NVARCHAR(30)     = NULLIF(JSON_VALUE(@Criteria, '$.EmploymentStatus'), '');
    DECLARE @hire1  DATE             = TRY_CONVERT(DATE, JSON_VALUE(@Criteria, '$.HireDate1'));
    DECLARE @hire2  DATE             = TRY_CONVERT(DATE, JSON_VALUE(@Criteria, '$.HireDate2'));
    DECLARE @mgrOnly BIT             = CASE WHEN JSON_VALUE(@Criteria, '$.IsManagerial') IN ('true','1') THEN 1 ELSE 0 END;
    DECLARE @useOutputs BIT          = CASE WHEN ISNULL(@OutputFields, '') IN ('', '[]') THEN 0 ELSE 1 END;

    -- Result set 1: the report's columns, filtered + ordered + re-labelled by the user's selection.
    SELECT c.Field, COALESCE(NULLIF(o.Label, ''), c.Label) AS Label, c.[Type], c.Width, c.LinkPage, c.LinkPageValue
    FROM (VALUES
        (1, 'EmployeeNumber',   'Employee #', 'string',   120, CAST(NULL AS NVARCHAR(100)), CAST(NULL AS NVARCHAR(100))),
        (2, 'FullName',         'Full Name',  'string',   220, NULL, NULL),
        (3, 'PositionName',     'Position', 'string', 220, Null, Null),
        (4, 'UnitName',         'Unit',       'string',   180, NULL, NULL),
        (5, 'PositionCode',     'Position',   'string',   120, NULL, NULL),
        (6, 'EmploymentStatus', 'Status',     'string',   110, NULL, NULL),
        (7, 'IsManagerial',     'Managerial', 'boolean',   90, NULL, NULL),
        (8, 'HireDate',         'Hire Date',  'date',     110, NULL, NULL),
        (9, 'Salary',           'Salary',     'currency', 120, NULL, NULL)
    ) c(Seq, Field, Label, [Type], Width, LinkPage, LinkPageValue)
    LEFT JOIN OPENJSON(@OutputFields)
        WITH (Field NVARCHAR(100) '$.Field', Label NVARCHAR(200) '$.Label', [Order] INT '$.Order') o
        ON o.Field = c.Field
    WHERE @useOutputs = 0 OR o.Field IS NOT NULL
    ORDER BY CASE WHEN @useOutputs = 1 THEN o.[Order] ELSE c.Seq END;

    -- Result set 2: the data, ORDER BY'd by the chosen sort fields (SortOrder>0, priority ascending).
    DECLARE @orderby NVARCHAR(MAX) = STUFF((
        SELECT ',' + QUOTENAME(o.Field)
        FROM OPENJSON(@OutputFields)
            WITH (Field NVARCHAR(100) '$.Field', SortOrder INT '$.SortOrder') o
        WHERE @useOutputs = 1 AND o.SortOrder > 0
          AND o.Field IN ('EmployeeNumber','FullName','PositionName','UnitName','PositionCode','EmploymentStatus','IsManagerial','HireDate','Salary')
        ORDER BY o.SortOrder
        FOR XML PATH('')), 1, 1, '');
    IF @orderby IS NULL OR @orderby = '' SET @orderby = N'[EmployeeNumber]';

    DECLARE @sql NVARCHAR(MAX) = N'
        SELECT e.EmployeeNumber,
               LTRIM(RTRIM(CONCAT(p.FirstName, '' '', p.GrandFatherName))) AS FullName,
               ou.Name  AS UnitName,
               poc.Title AS PositionName,
               e.EmploymentStatus,
               e.IsManagerial,
               e.HireDate,
               e.Salary
        FROM Hrms.Employee e
                    left JOIN          Core.Person p             ON p.Id  = e.PersonId
                    left JOIN          Hrms.Position pos        ON pos.Id = e.PositionId
                    left join          Hrms.PositionClass poc   on poc.Id = pos.[PositionClassId]
                    left JOIN Hrms.OrganizationUnit ou ON ou.Id = pos.OrganizationUnitId
        WHERE e.TenantId = @TenantId
          AND (@BranchId IS NULL OR e.BranchId = @BranchId)
          AND (@unitId  IS NULL OR pos.OrganizationUnitId = @unitId)
          AND (@status  IS NULL OR e.EmploymentStatus = @status)
          AND (@hire1   IS NULL OR e.HireDate >= @hire1)
          AND (@hire2   IS NULL OR e.HireDate <  DATEADD(DAY, 1, @hire2))
          AND (@mgrOnly = 0     OR e.IsManagerial = 1)
        ORDER BY ' + @orderby + N';';

    EXEC sp_executesql @sql,
        N'@TenantId NVARCHAR(64), @BranchId UNIQUEIDENTIFIER, @unitId UNIQUEIDENTIFIER, @status NVARCHAR(30), @hire1 DATE, @hire2 DATE, @mgrOnly BIT',
        @TenantId, @BranchId, @unitId, @status, @hire1, @hire2, @mgrOnly;
END
GO
IF OBJECT_ID('[Hrms].[Report_EmployeeDirectoryGrouped]', 'P') IS NOT NULL DROP PROCEDURE [Hrms].[Report_EmployeeDirectoryGrouped];
GO
CREATE OR ALTER PROCEDURE [Hrms].[Report_EmployeeDirectoryGrouped]
    @TenantId     NVARCHAR(64),
    @BranchId     UNIQUEIDENTIFIER = NULL,
    @UserId       UNIQUEIDENTIFIER = NULL,
    @ReportKey    NVARCHAR(100),
    @Criteria     NVARCHAR(MAX) = NULL,
    @OutputFields NVARCHAR(MAX) = NULL,
    @Source       NVARCHAR(20)  = NULL,
    @Roles        NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    -- Standard filters (same as the flat Employee Directory report).
    DECLARE @unitId  UNIQUEIDENTIFIER = TRY_CONVERT(UNIQUEIDENTIFIER, JSON_VALUE(@Criteria, '$.OrganizationUnitId'));
    DECLARE @status  NVARCHAR(30)     = NULLIF(JSON_VALUE(@Criteria, '$.EmploymentStatus'), '');
    DECLARE @hire1   DATE             = TRY_CONVERT(DATE, JSON_VALUE(@Criteria, '$.HireDate1'));
    DECLARE @hire2   DATE             = TRY_CONVERT(DATE, JSON_VALUE(@Criteria, '$.HireDate2'));
    DECLARE @mgrOnly BIT              = CASE WHEN JSON_VALUE(@Criteria, '$.IsManagerial') IN ('true','1') THEN 1 ELSE 0 END;
    DECLARE @useOutputs BIT           = CASE WHEN ISNULL(@OutputFields, '') IN ('', '[]') THEN 0 ELSE 1 END;

    -- PIVOT inputs (reference GridConfig / user grouping payload) travel as reserved criteria values.
    DECLARE @groupBy     NVARCHAR(MAX) = NULLIF(JSON_VALUE(@Criteria, '$.__groupBy'), '');
    DECLARE @dir         NVARCHAR(4)   = CASE WHEN LOWER(ISNULL(JSON_VALUE(@Criteria, '$.__groupOrder'), 'asc')) = 'desc' THEN 'DESC' ELSE 'ASC' END;
    DECLARE @showSummary BIT           = CASE WHEN JSON_VALUE(@Criteria, '$.__showSummary') IN ('true','1') THEN 1 ELSE 0 END;
    IF @groupBy IS NULL SET @groupBy = 'UnitName';   -- default grouping when none chosen

    -- Parse the comma list into an ORDERED, WHITELISTED set of group columns (OPENJSON [key] = level).
    DECLARE @gbJson NVARCHAR(MAX) = '["' + REPLACE(REPLACE(@groupBy, ' ', ''), ',', '","') + '"]';
    DECLARE @groups TABLE (Lvl INT, Field NVARCHAR(100));
    INSERT INTO @groups (Lvl, Field)
    SELECT CAST([key] AS INT), [value]
    FROM OPENJSON(@gbJson)
    WHERE [value] IN ('UnitName', 'EmploymentStatus', 'IsManagerial', 'PositionCode');
    IF NOT EXISTS (SELECT 1 FROM @groups) INSERT INTO @groups (Lvl, Field) VALUES (0, 'UnitName');

    DECLARE @groupSel  NVARCHAR(MAX) = STUFF((SELECT ',' + QUOTENAME(Field)             FROM @groups ORDER BY Lvl FOR XML PATH('')), 1, 1, '');
    DECLARE @orderCols NVARCHAR(MAX) = STUFF((SELECT ',' + QUOTENAME(Field) + ' ' + @dir FROM @groups ORDER BY Lvl FOR XML PATH('')), 1, 1, '');

    ----------------------------------------------------------------------------------------------------
    -- Result set 1: column metadata - the GROUP columns lead (in level order), then the remaining
    -- output columns (filtered + re-labelled by the user's @OutputFields selection).
    ----------------------------------------------------------------------------------------------------
    SELECT c.Field, COALESCE(NULLIF(o.Label, ''), c.Label) AS Label, c.[Type], c.Width, c.LinkPage, c.LinkPageValue
    FROM (VALUES
        (1, 'EmployeeNumber',   'Employee #', 'string',   120, CAST(NULL AS NVARCHAR(100)), CAST(NULL AS NVARCHAR(100))),
        (2, 'FullName',         'Full Name',  'string',   220, NULL, NULL),
        (3, 'UnitName',         'Unit',       'string',   180, NULL, NULL),
        (4, 'PositionCode',     'Position',   'string',   120, NULL, NULL),
        (5, 'EmploymentStatus', 'Status',     'string',   110, NULL, NULL),
        (6, 'IsManagerial',     'Managerial', 'boolean',   90, NULL, NULL),
        (7, 'HireDate',         'Hire Date',  'date',     110, NULL, NULL),
        (8, 'Salary',           'Salary',     'currency', 120, NULL, NULL)
    ) c(Seq, Field, Label, [Type], Width, LinkPage, LinkPageValue)
    LEFT JOIN OPENJSON(@OutputFields)
        WITH (Field NVARCHAR(100) '$.Field', Label NVARCHAR(200) '$.Label', [Order] INT '$.Order') o
        ON o.Field = c.Field
    LEFT JOIN @groups g ON g.Field = c.Field
    WHERE g.Field IS NOT NULL OR @useOutputs = 0 OR o.Field IS NOT NULL
    ORDER BY CASE WHEN g.Field IS NOT NULL THEN g.Lvl
                  ELSE 1000 + (CASE WHEN @useOutputs = 1 THEN o.[Order] ELSE c.Seq END) END;

    -- The shared detail projection + FROM/WHERE, reused by the data and summary sets.
    DECLARE @selectList NVARCHAR(MAX) = N'
        e.EmployeeNumber,
        LTRIM(RTRIM(CONCAT(p.FirstName, '' '', p.GrandFatherName))) AS FullName,
        ou.Name  AS UnitName,
        pos.Code AS PositionCode,
        e.EmploymentStatus,
        e.IsManagerial,
        e.HireDate,
        e.Salary';
    DECLARE @baseFrom NVARCHAR(MAX) = N'
        FROM Hrms.Employee e
        LEFT JOIN Core.Person p             ON p.Id  = e.PersonId
        LEFT JOIN Hrms.Position pos        ON pos.Id = e.PositionId
        LEFT JOIN Hrms.OrganizationUnit ou ON ou.Id = pos.OrganizationUnitId
        WHERE e.TenantId = @TenantId
          AND (@BranchId IS NULL OR e.BranchId = @BranchId)
          AND (@unitId  IS NULL OR pos.OrganizationUnitId = @unitId)
          AND (@status  IS NULL OR e.EmploymentStatus = @status)
          AND (@hire1   IS NULL OR e.HireDate >= @hire1)
          AND (@hire2   IS NULL OR e.HireDate <  DATEADD(DAY, 1, @hire2))
          AND (@mgrOnly = 0     OR e.IsManagerial = 1)';
    DECLARE @paramDef NVARCHAR(MAX) =
        N'@TenantId NVARCHAR(64), @BranchId UNIQUEIDENTIFIER, @unitId UNIQUEIDENTIFIER, @status NVARCHAR(30), @hire1 DATE, @hire2 DATE, @mgrOnly BIT';

    ----------------------------------------------------------------------------------------------------
    -- Result set 2: the detail rows, PRE-GROUPED server-side (ordered by the group columns + level,
    -- then EmployeeNumber). The grid renders these already grouped.
    ----------------------------------------------------------------------------------------------------
    DECLARE @sql NVARCHAR(MAX) = N'SELECT ' + @selectList + @baseFrom + N'
        ORDER BY ' + @orderCols + N', [EmployeeNumber];';
    EXEC sp_executesql @sql, @paramDef, @TenantId, @BranchId, @unitId, @status, @hire1, @hire2, @mgrOnly;

    ----------------------------------------------------------------------------------------------------
    -- Result set 3 (optional): per-group SUBTOTALS - the T-SQL port of ReportGroupedExportBuilder's
    -- group summaries. One row per leaf group: the group column values + GroupCount + SalaryTotal.
    ----------------------------------------------------------------------------------------------------
    IF @showSummary = 1
    BEGIN
        DECLARE @sql3 NVARCHAR(MAX) = N'SELECT ' + @groupSel + N', COUNT(*) AS GroupCount, SUM(d.Salary) AS SalaryTotal
            FROM (SELECT ' + @selectList + @baseFrom + N') d
            GROUP BY ' + @groupSel + N'
            ORDER BY ' + @orderCols + N';';
        EXEC sp_executesql @sql3, @paramDef, @TenantId, @BranchId, @unitId, @status, @hire1, @hire2, @mgrOnly;
    END
END
GO
IF OBJECT_ID('[Hrms].[Report_EmployeeMovements]', 'P') IS NOT NULL DROP PROCEDURE [Hrms].[Report_EmployeeMovements];
GO
CREATE OR ALTER PROCEDURE [Hrms].[Report_EmployeeMovements]
    @TenantId     NVARCHAR(64),
    @BranchId     UNIQUEIDENTIFIER = NULL,
    @UserId       UNIQUEIDENTIFIER = NULL,
    @ReportKey    NVARCHAR(100),
    @Criteria     NVARCHAR(MAX) = NULL,
    @OutputFields NVARCHAR(MAX) = NULL,
    @Source       NVARCHAR(20)  = NULL,
    @Roles        NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @eff1  DATE         = TRY_CONVERT(DATE, JSON_VALUE(@Criteria, '$.EffectiveDate1'));
    DECLARE @eff2  DATE         = TRY_CONVERT(DATE, JSON_VALUE(@Criteria, '$.EffectiveDate2'));
    DECLARE @mtype NVARCHAR(30) = NULLIF(JSON_VALUE(@Criteria, '$.MovementType'), '');
    DECLARE @mstat NVARCHAR(30) = NULLIF(JSON_VALUE(@Criteria, '$.MovementStatus'), '');
    DECLARE @useOutputs BIT     = CASE WHEN ISNULL(@OutputFields, '') IN ('', '[]') THEN 0 ELSE 1 END;

    SELECT c.Field, COALESCE(NULLIF(o.Label, ''), c.Label) AS Label, c.[Type], c.Width, c.LinkPage, c.LinkPageValue
    FROM (VALUES
        (1, 'EmployeeNumber', 'Employee #',     'string',   120, CAST(NULL AS NVARCHAR(100)), CAST(NULL AS NVARCHAR(100))),
        (2, 'FullName',       'Full Name',      'string',   220, NULL, NULL),
        (3, 'MovementType',   'Movement',       'string',   110, NULL, NULL),
        (4, 'TransferKind',   'Transfer Kind',  'string',   110, NULL, NULL),
        (5, 'MovementStatus', 'Status',         'string',   110, NULL, NULL),
        (6, 'EffectiveDate',  'Effective Date', 'date',     120, NULL, NULL),
        (7, 'FromPosition',   'From Position',  'string',   160, NULL, NULL),
        (8, 'ToPosition',     'To Position',    'string',   160, NULL, NULL),
        (9, 'FromSalary',     'From Salary',    'currency', 120, NULL, NULL),
        (10,'ToSalary',       'To Salary',      'currency', 120, NULL, NULL),
        (11,'FromBranchName', 'From Branch',    'string',   150, NULL, NULL),
        (12,'ToBranchName',   'To Branch',      'string',   150, NULL, NULL),
        (13,'ExecutedAt',     'Executed',       'date',     110, NULL, NULL),
        (14,'Reason',         'Reason',         'string',   250, NULL, NULL)
    ) c(Seq, Field, Label, [Type], Width, LinkPage, LinkPageValue)
    LEFT JOIN OPENJSON(@OutputFields)
        WITH (Field NVARCHAR(100) '$.Field', Label NVARCHAR(200) '$.Label', [Order] INT '$.Order') o
        ON o.Field = c.Field
    WHERE @useOutputs = 0 OR o.Field IS NOT NULL
    ORDER BY CASE WHEN @useOutputs = 1 THEN o.[Order] ELSE c.Seq END;

    DECLARE @orderby NVARCHAR(MAX) = STUFF((
        SELECT ',' + QUOTENAME(o.Field)
        FROM OPENJSON(@OutputFields)
            WITH (Field NVARCHAR(100) '$.Field', SortOrder INT '$.SortOrder') o
        WHERE @useOutputs = 1 AND o.SortOrder > 0
          AND o.Field IN ('EmployeeNumber','FullName','MovementType','TransferKind','MovementStatus','EffectiveDate','FromPosition','ToPosition','FromSalary','ToSalary','FromBranchName','ToBranchName','ExecutedAt','Reason')
        ORDER BY o.SortOrder
        FOR XML PATH('')), 1, 1, '');
    IF @orderby IS NULL OR @orderby = '' SET @orderby = N'[EffectiveDate] DESC, [EmployeeNumber]';

    DECLARE @sql NVARCHAR(MAX) = N'
        SELECT e.EmployeeNumber,
               LTRIM(RTRIM(CONCAT(p.FirstName, '' '', p.GrandFatherName))) AS FullName,
               m.MovementType,
               m.TransferKind,
               m.Status AS MovementStatus,
               m.EffectiveDate,
               fp.Code  AS FromPosition,
               tp.Code  AS ToPosition,
               m.FromSalary,
               m.ToSalary,
               fb.Name  AS FromBranchName,
               tb.Name  AS ToBranchName,
               m.ExecutedAt,
               m.Reason
        FROM Hrms.EmployeeMovement m
        INNER JOIN Hrms.Employee e  ON e.Id  = m.EmployeeId
        LEFT JOIN Core.Person p    ON p.Id  = e.PersonId
        LEFT JOIN Hrms.Position fp  ON fp.Id = m.FromPositionId
        LEFT JOIN Hrms.Position tp  ON tp.Id = m.ToPositionId
        LEFT JOIN Hrms.Branch fb    ON fb.Id = m.FromBranchId
        LEFT JOIN Hrms.Branch tb    ON tb.Id = m.ToBranchId
        WHERE m.TenantId = @TenantId
          AND (@BranchId IS NULL OR e.BranchId = @BranchId)
          AND (@eff1  IS NULL OR m.EffectiveDate >= @eff1)
          AND (@eff2  IS NULL OR m.EffectiveDate <  DATEADD(DAY, 1, @eff2))
          AND (@mtype IS NULL OR m.MovementType = @mtype)
          AND (@mstat IS NULL OR m.Status = @mstat)
        ORDER BY ' + @orderby + N';';

    EXEC sp_executesql @sql,
        N'@TenantId NVARCHAR(64), @BranchId UNIQUEIDENTIFIER, @eff1 DATE, @eff2 DATE, @mtype NVARCHAR(30), @mstat NVARCHAR(30)',
        @TenantId, @BranchId, @eff1, @eff2, @mtype, @mstat;
END
GO
IF OBJECT_ID('[Hrms].[Report_HeadcountByUnit]', 'P') IS NOT NULL DROP PROCEDURE [Hrms].[Report_HeadcountByUnit];
GO
CREATE OR ALTER PROCEDURE [Hrms].[Report_HeadcountByUnit]
    @TenantId     NVARCHAR(64),
    @BranchId     UNIQUEIDENTIFIER = NULL,
    @UserId       UNIQUEIDENTIFIER = NULL,
    @ReportKey    NVARCHAR(100),
    @Criteria     NVARCHAR(MAX) = NULL,
    @OutputFields NVARCHAR(MAX) = NULL,
    @Source       NVARCHAR(20)  = NULL,
    @Roles        NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @unitIds NVARCHAR(MAX) = NULLIF(JSON_VALUE(@Criteria, '$.OrganizationUnitId'), '');
    DECLARE @status  NVARCHAR(30)  = NULLIF(JSON_VALUE(@Criteria, '$.EmploymentStatus'), '');
    DECLARE @nature  NVARCHAR(30)  = NULLIF(JSON_VALUE(@Criteria, '$.EmploymentNature'), '');
    DECLARE @useOutputs BIT        = CASE WHEN ISNULL(@OutputFields, '') IN ('', '[]') THEN 0 ELSE 1 END;

    -- Pivot inputs travel as reserved criteria values (same convention as EmployeeDirectoryGrouped).
    DECLARE @groupBy     NVARCHAR(MAX) = NULLIF(JSON_VALUE(@Criteria, '$.__groupBy'), '');
    DECLARE @dir         NVARCHAR(4)   = CASE WHEN LOWER(ISNULL(JSON_VALUE(@Criteria, '$.__groupOrder'), 'asc')) = 'desc' THEN 'DESC' ELSE 'ASC' END;
    DECLARE @showSummary BIT           = CASE WHEN JSON_VALUE(@Criteria, '$.__showSummary') IN ('true','1') THEN 1 ELSE 0 END;
    IF @groupBy IS NULL SET @groupBy = 'UnitName';

    DECLARE @gbJson NVARCHAR(MAX) = '["' + REPLACE(REPLACE(@groupBy, ' ', ''), ',', '","') + '"]';
    DECLARE @groups TABLE (Lvl INT, Field NVARCHAR(100));
    INSERT INTO @groups (Lvl, Field)
    SELECT CAST([key] AS INT), [value]
    FROM OPENJSON(@gbJson)
    WHERE [value] IN ('UnitName', 'BranchName', 'EmploymentStatus', 'EmploymentNature', 'Gender', 'IsManagerial');
    IF NOT EXISTS (SELECT 1 FROM @groups) INSERT INTO @groups (Lvl, Field) VALUES (0, 'UnitName');

    DECLARE @groupSel  NVARCHAR(MAX) = STUFF((SELECT ',' + QUOTENAME(Field)             FROM @groups ORDER BY Lvl FOR XML PATH('')), 1, 1, '');
    DECLARE @orderCols NVARCHAR(MAX) = STUFF((SELECT ',' + QUOTENAME(Field) + ' ' + @dir FROM @groups ORDER BY Lvl FOR XML PATH('')), 1, 1, '');

    -- Result set 1: group columns lead (level order), then the remaining selected output columns.
    SELECT c.Field, COALESCE(NULLIF(o.Label, ''), c.Label) AS Label, c.[Type], c.Width, c.LinkPage, c.LinkPageValue
    FROM (VALUES
        (1, 'EmployeeNumber',   'Employee #', 'string',   120, CAST(NULL AS NVARCHAR(100)), CAST(NULL AS NVARCHAR(100))),
        (2, 'FullName',         'Full Name',  'string',   220, NULL, NULL),
        (3, 'Gender',           'Gender',     'string',    90, NULL, NULL),
        (4, 'UnitName',         'Unit',       'string',   180, NULL, NULL),
        (5, 'BranchName',       'Branch',     'string',   150, NULL, NULL),
        (6, 'PositionTitle',    'Position',   'string',   200, NULL, NULL),
        (7, 'EmploymentStatus', 'Status',     'string',   110, NULL, NULL),
        (8, 'EmploymentNature', 'Nature',     'string',   110, NULL, NULL),
        (9, 'IsManagerial',     'Managerial', 'boolean',   90, NULL, NULL),
        (10,'HireDate',         'Hire Date',  'date',     110, NULL, NULL)
    ) c(Seq, Field, Label, [Type], Width, LinkPage, LinkPageValue)
    LEFT JOIN OPENJSON(@OutputFields)
        WITH (Field NVARCHAR(100) '$.Field', Label NVARCHAR(200) '$.Label', [Order] INT '$.Order') o
        ON o.Field = c.Field
    LEFT JOIN @groups g ON g.Field = c.Field
    WHERE g.Field IS NOT NULL OR @useOutputs = 0 OR o.Field IS NOT NULL
    ORDER BY CASE WHEN g.Field IS NOT NULL THEN g.Lvl
                  ELSE 1000 + (CASE WHEN @useOutputs = 1 THEN o.[Order] ELSE c.Seq END) END;

    DECLARE @selectList NVARCHAR(MAX) = N'
        e.EmployeeNumber,
        LTRIM(RTRIM(CONCAT(p.FirstName, '' '', p.GrandFatherName))) AS FullName,
        p.Gender,
        ou.Name  AS UnitName,
        b.Name   AS BranchName,
        poc.Title AS PositionTitle,
        e.EmploymentStatus,
        e.EmploymentNature,
        e.IsManagerial,
        e.HireDate';
    DECLARE @baseFrom NVARCHAR(MAX) = N'
        FROM Hrms.Employee e
        LEFT JOIN Core.Person p           ON p.Id   = e.PersonId
        LEFT JOIN Hrms.Position pos        ON pos.Id = e.PositionId
        LEFT JOIN Hrms.PositionClass poc   ON poc.Id = pos.PositionClassId
        LEFT JOIN Hrms.OrganizationUnit ou ON ou.Id  = pos.OrganizationUnitId
        LEFT JOIN Hrms.Branch b            ON b.Id   = e.BranchId
        WHERE e.TenantId = @TenantId
          AND (@BranchId IS NULL OR e.BranchId = @BranchId)
          AND (@unitIds  IS NULL OR pos.OrganizationUnitId IN
               (SELECT TRY_CONVERT(UNIQUEIDENTIFIER, LTRIM(RTRIM(value))) FROM STRING_SPLIT(@unitIds, '','')))
          AND ((@status IS NULL AND e.IsTerminated = 0) OR e.EmploymentStatus = @status)
          AND (@nature  IS NULL OR e.EmploymentNature = @nature)';
    DECLARE @paramDef NVARCHAR(MAX) =
        N'@TenantId NVARCHAR(64), @BranchId UNIQUEIDENTIFIER, @unitIds NVARCHAR(MAX), @status NVARCHAR(30), @nature NVARCHAR(30)';

    -- Result set 2: detail rows pre-grouped server-side.
    DECLARE @sql NVARCHAR(MAX) = N'SELECT ' + @selectList + @baseFrom + N'
        ORDER BY ' + @orderCols + N', [EmployeeNumber];';
    EXEC sp_executesql @sql, @paramDef, @TenantId, @BranchId, @unitIds, @status, @nature;

    -- Result set 3 (optional): headcount per leaf group.
    IF @showSummary = 1
    BEGIN
        DECLARE @sql3 NVARCHAR(MAX) = N'SELECT ' + @groupSel + N', COUNT(*) AS GroupCount
            FROM (SELECT ' + @selectList + @baseFrom + N') d
            GROUP BY ' + @groupSel + N'
            ORDER BY ' + @orderCols + N';';
        EXEC sp_executesql @sql3, @paramDef, @TenantId, @BranchId, @unitIds, @status, @nature;
    END
END
GO
IF OBJECT_ID('[Hrms].[Report_LeaveBalances]', 'P') IS NOT NULL DROP PROCEDURE [Hrms].[Report_LeaveBalances];
GO
CREATE OR ALTER PROCEDURE [Hrms].[Report_LeaveBalances]
    @TenantId     NVARCHAR(64),
    @BranchId     UNIQUEIDENTIFIER = NULL,
    @UserId       UNIQUEIDENTIFIER = NULL,
    @ReportKey    NVARCHAR(100),
    @Criteria     NVARCHAR(MAX) = NULL,
    @OutputFields NVARCHAR(MAX) = NULL,
    @Source       NVARCHAR(20)  = NULL,
    @Roles        NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @fiscalYearId UNIQUEIDENTIFIER = TRY_CONVERT(UNIQUEIDENTIFIER, JSON_VALUE(@Criteria, '$.FiscalYearId'));
    DECLARE @leaveTypeId  UNIQUEIDENTIFIER = TRY_CONVERT(UNIQUEIDENTIFIER, JSON_VALUE(@Criteria, '$.LeaveTypeId'));
    DECLARE @unitIds      NVARCHAR(MAX)    = NULLIF(JSON_VALUE(@Criteria, '$.OrganizationUnitId'), '');
    DECLARE @useOutputs   BIT              = CASE WHEN ISNULL(@OutputFields, '') IN ('', '[]') THEN 0 ELSE 1 END;

    SELECT c.Field, COALESCE(NULLIF(o.Label, ''), c.Label) AS Label, c.[Type], c.Width, c.LinkPage, c.LinkPageValue
    FROM (VALUES
        (1, 'EmployeeNumber',  'Employee #',      'string', 120, CAST(NULL AS NVARCHAR(100)), CAST(NULL AS NVARCHAR(100))),
        (2, 'FullName',        'Full Name',       'string', 220, NULL, NULL),
        (3, 'UnitName',        'Unit',            'string', 180, NULL, NULL),
        (4, 'LeaveTypeName',   'Leave Type',      'string', 150, NULL, NULL),
        (5, 'FiscalYearName',  'Fiscal Year',     'string', 110, NULL, NULL),
        (6, 'Entitled',        'Entitled',        'number', 100, NULL, NULL),
        (7, 'CarriedForward',  'Carried Forward', 'number', 120, NULL, NULL),
        (8, 'Adjusted',        'Adjusted',        'number', 100, NULL, NULL),
        (9, 'Taken',           'Taken',           'number', 100, NULL, NULL),
        (10,'Remaining',       'Remaining',       'number', 100, NULL, NULL)
    ) c(Seq, Field, Label, [Type], Width, LinkPage, LinkPageValue)
    LEFT JOIN OPENJSON(@OutputFields)
        WITH (Field NVARCHAR(100) '$.Field', Label NVARCHAR(200) '$.Label', [Order] INT '$.Order') o
        ON o.Field = c.Field
    WHERE @useOutputs = 0 OR o.Field IS NOT NULL
    ORDER BY CASE WHEN @useOutputs = 1 THEN o.[Order] ELSE c.Seq END;

    DECLARE @orderby NVARCHAR(MAX) = STUFF((
        SELECT ',' + QUOTENAME(o.Field)
        FROM OPENJSON(@OutputFields)
            WITH (Field NVARCHAR(100) '$.Field', SortOrder INT '$.SortOrder') o
        WHERE @useOutputs = 1 AND o.SortOrder > 0
          AND o.Field IN ('EmployeeNumber','FullName','UnitName','LeaveTypeName','FiscalYearName','Entitled','CarriedForward','Adjusted','Taken','Remaining')
        ORDER BY o.SortOrder
        FOR XML PATH('')), 1, 1, '');
    IF @orderby IS NULL OR @orderby = '' SET @orderby = N'[EmployeeNumber], [LeaveTypeName]';

    DECLARE @sql NVARCHAR(MAX) = N'
        SELECT e.EmployeeNumber,
               LTRIM(RTRIM(CONCAT(p.FirstName, '' '', p.GrandFatherName))) AS FullName,
               ou.Name AS UnitName,
               lt.Name AS LeaveTypeName,
               fy.Name AS FiscalYearName,
               lb.Entitled,
               lb.CarriedForward,
               lb.Adjusted,
               lb.Taken,
               (lb.Entitled + lb.CarriedForward + lb.Adjusted - lb.Taken) AS Remaining
        FROM Hrms.LeaveBalance lb
        INNER JOIN Hrms.Employee e         ON e.Id   = lb.EmployeeId
        LEFT JOIN Core.Person p           ON p.Id   = e.PersonId
        LEFT JOIN Hrms.Position pos        ON pos.Id = e.PositionId
        LEFT JOIN Hrms.OrganizationUnit ou ON ou.Id  = pos.OrganizationUnitId
        LEFT JOIN Hrms.LeaveType lt        ON lt.Id  = lb.LeaveTypeId
        LEFT JOIN Core.FiscalYear fy          ON fy.Id  = lb.FiscalYearId
        WHERE lb.TenantId = @TenantId
          AND (@BranchId     IS NULL OR e.BranchId = @BranchId)
          AND (@fiscalYearId IS NULL OR lb.FiscalYearId = @fiscalYearId)
          AND (@leaveTypeId  IS NULL OR lb.LeaveTypeId = @leaveTypeId)
          AND (@unitIds      IS NULL OR pos.OrganizationUnitId IN
               (SELECT TRY_CONVERT(UNIQUEIDENTIFIER, LTRIM(RTRIM(value))) FROM STRING_SPLIT(@unitIds, '','')))
        ORDER BY ' + @orderby + N';';

    EXEC sp_executesql @sql,
        N'@TenantId NVARCHAR(64), @BranchId UNIQUEIDENTIFIER, @fiscalYearId UNIQUEIDENTIFIER, @leaveTypeId UNIQUEIDENTIFIER, @unitIds NVARCHAR(MAX)',
        @TenantId, @BranchId, @fiscalYearId, @leaveTypeId, @unitIds;
END
GO
IF OBJECT_ID('[Hrms].[Report_LeaveTaken]', 'P') IS NOT NULL DROP PROCEDURE [Hrms].[Report_LeaveTaken];
GO
CREATE OR ALTER PROCEDURE [Hrms].[Report_LeaveTaken]
    @TenantId     NVARCHAR(64),
    @BranchId     UNIQUEIDENTIFIER = NULL,
    @UserId       UNIQUEIDENTIFIER = NULL,
    @ReportKey    NVARCHAR(100),
    @Criteria     NVARCHAR(MAX) = NULL,
    @OutputFields NVARCHAR(MAX) = NULL,
    @Source       NVARCHAR(20)  = NULL,
    @Roles        NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @start1      DATE             = TRY_CONVERT(DATE, JSON_VALUE(@Criteria, '$.StartDate1'));
    DECLARE @start2      DATE             = TRY_CONVERT(DATE, JSON_VALUE(@Criteria, '$.StartDate2'));
    DECLARE @leaveTypeId UNIQUEIDENTIFIER = TRY_CONVERT(UNIQUEIDENTIFIER, JSON_VALUE(@Criteria, '$.LeaveTypeId'));
    DECLARE @lstat       NVARCHAR(30)     = NULLIF(JSON_VALUE(@Criteria, '$.LeaveStatus'), '');
    DECLARE @unitIds     NVARCHAR(MAX)    = NULLIF(JSON_VALUE(@Criteria, '$.OrganizationUnitId'), '');
    DECLARE @useOutputs  BIT              = CASE WHEN ISNULL(@OutputFields, '') IN ('', '[]') THEN 0 ELSE 1 END;

    SELECT c.Field, COALESCE(NULLIF(o.Label, ''), c.Label) AS Label, c.[Type], c.Width, c.LinkPage, c.LinkPageValue
    FROM (VALUES
        (1, 'EmployeeNumber', 'Employee #',   'string', 120, CAST(NULL AS NVARCHAR(100)), CAST(NULL AS NVARCHAR(100))),
        (2, 'FullName',       'Full Name',    'string', 220, NULL, NULL),
        (3, 'UnitName',       'Unit',         'string', 180, NULL, NULL),
        (4, 'LeaveTypeName',  'Leave Type',   'string', 150, NULL, NULL),
        (5, 'StartDate',      'From',         'date',   110, NULL, NULL),
        (6, 'EndDate',        'To',           'date',   110, NULL, NULL),
        (7, 'DayPart',        'Day Part',     'string',  95, NULL, NULL),
        (8, 'WorkingDays',    'Working Days', 'number', 110, NULL, NULL),
        (9, 'RequestStatus',  'Status',       'string', 100, NULL, NULL),
        (10,'SubmittedDate',  'Submitted',    'date',   110, NULL, NULL)
    ) c(Seq, Field, Label, [Type], Width, LinkPage, LinkPageValue)
    LEFT JOIN OPENJSON(@OutputFields)
        WITH (Field NVARCHAR(100) '$.Field', Label NVARCHAR(200) '$.Label', [Order] INT '$.Order') o
        ON o.Field = c.Field
    WHERE @useOutputs = 0 OR o.Field IS NOT NULL
    ORDER BY CASE WHEN @useOutputs = 1 THEN o.[Order] ELSE c.Seq END;

    DECLARE @orderby NVARCHAR(MAX) = STUFF((
        SELECT ',' + QUOTENAME(o.Field)
        FROM OPENJSON(@OutputFields)
            WITH (Field NVARCHAR(100) '$.Field', SortOrder INT '$.SortOrder') o
        WHERE @useOutputs = 1 AND o.SortOrder > 0
          AND o.Field IN ('EmployeeNumber','FullName','UnitName','LeaveTypeName','StartDate','EndDate','DayPart','WorkingDays','RequestStatus','SubmittedDate')
        ORDER BY o.SortOrder
        FOR XML PATH('')), 1, 1, '');
    IF @orderby IS NULL OR @orderby = '' SET @orderby = N'[StartDate] DESC, [EmployeeNumber]';

    DECLARE @sql NVARCHAR(MAX) = N'
        SELECT e.EmployeeNumber,
               LTRIM(RTRIM(CONCAT(p.FirstName, '' '', p.GrandFatherName))) AS FullName,
               ou.Name AS UnitName,
               lt.Name AS LeaveTypeName,
               ll.StartDate,
               ll.EndDate,
               ll.DayPart,
               ll.WorkingDays,
               lr.Status AS RequestStatus,
               lr.SubmittedDate
        FROM Hrms.LeaveRequestLine ll
        INNER JOIN Hrms.LeaveRequest lr    ON lr.Id  = ll.LeaveRequestId
        INNER JOIN Hrms.Employee e         ON e.Id   = lr.EmployeeId
        LEFT JOIN Core.Person p           ON p.Id   = e.PersonId
        LEFT JOIN Hrms.Position pos        ON pos.Id = e.PositionId
        LEFT JOIN Hrms.OrganizationUnit ou ON ou.Id  = pos.OrganizationUnitId
        LEFT JOIN Hrms.LeaveType lt        ON lt.Id  = ll.LeaveTypeId
        WHERE ll.TenantId = @TenantId
          AND (@BranchId    IS NULL OR e.BranchId = @BranchId)
          AND (@start1      IS NULL OR ll.StartDate >= @start1)
          AND (@start2      IS NULL OR ll.StartDate <  DATEADD(DAY, 1, @start2))
          AND (@leaveTypeId IS NULL OR ll.LeaveTypeId = @leaveTypeId)
          AND (@lstat       IS NULL OR lr.Status = @lstat)
          AND (@unitIds     IS NULL OR pos.OrganizationUnitId IN
               (SELECT TRY_CONVERT(UNIQUEIDENTIFIER, LTRIM(RTRIM(value))) FROM STRING_SPLIT(@unitIds, '','')))
        ORDER BY ' + @orderby + N';';

    EXEC sp_executesql @sql,
        N'@TenantId NVARCHAR(64), @BranchId UNIQUEIDENTIFIER, @start1 DATE, @start2 DATE, @leaveTypeId UNIQUEIDENTIFIER, @lstat NVARCHAR(30), @unitIds NVARCHAR(MAX)',
        @TenantId, @BranchId, @start1, @start2, @leaveTypeId, @lstat, @unitIds;
END
GO
IF OBJECT_ID('[Hrms].[Report_NewHires]', 'P') IS NOT NULL DROP PROCEDURE [Hrms].[Report_NewHires];
GO
CREATE OR ALTER PROCEDURE [Hrms].[Report_NewHires]
    @TenantId     NVARCHAR(64),
    @BranchId     UNIQUEIDENTIFIER = NULL,
    @UserId       UNIQUEIDENTIFIER = NULL,
    @ReportKey    NVARCHAR(100),
    @Criteria     NVARCHAR(MAX) = NULL,
    @OutputFields NVARCHAR(MAX) = NULL,
    @Source       NVARCHAR(20)  = NULL,
    @Roles        NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @hire1   DATE          = TRY_CONVERT(DATE, JSON_VALUE(@Criteria, '$.HireDate1'));
    DECLARE @hire2   DATE          = TRY_CONVERT(DATE, JSON_VALUE(@Criteria, '$.HireDate2'));
    DECLARE @unitIds NVARCHAR(MAX) = NULLIF(JSON_VALUE(@Criteria, '$.OrganizationUnitId'), '');
    DECLARE @nature  NVARCHAR(30)  = NULLIF(JSON_VALUE(@Criteria, '$.EmploymentNature'), '');
    DECLARE @useOutputs BIT        = CASE WHEN ISNULL(@OutputFields, '') IN ('', '[]') THEN 0 ELSE 1 END;

    SELECT c.Field, COALESCE(NULLIF(o.Label, ''), c.Label) AS Label, c.[Type], c.Width, c.LinkPage, c.LinkPageValue
    FROM (VALUES
        (1, 'EmployeeNumber',   'Employee #',  'string',   120, CAST(NULL AS NVARCHAR(100)), CAST(NULL AS NVARCHAR(100))),
        (2, 'FullName',         'Full Name',   'string',   220, NULL, NULL),
        (3, 'Gender',           'Gender',      'string',    90, NULL, NULL),
        (4, 'HireDate',         'Hire Date',   'date',     110, NULL, NULL),
        (5, 'UnitName',         'Unit',        'string',   180, NULL, NULL),
        (6, 'BranchName',       'Branch',      'string',   150, NULL, NULL),
        (7, 'PositionTitle',    'Position',    'string',   200, NULL, NULL),
        (8, 'EmploymentNature', 'Nature',      'string',   110, NULL, NULL),
        (9, 'EmploymentStatus', 'Status',      'string',   110, NULL, NULL),
        (10,'IsProbation',      'On Probation','boolean',   95, NULL, NULL),
        (11,'Salary',           'Salary',      'currency', 120, NULL, NULL)
    ) c(Seq, Field, Label, [Type], Width, LinkPage, LinkPageValue)
    LEFT JOIN OPENJSON(@OutputFields)
        WITH (Field NVARCHAR(100) '$.Field', Label NVARCHAR(200) '$.Label', [Order] INT '$.Order') o
        ON o.Field = c.Field
    WHERE @useOutputs = 0 OR o.Field IS NOT NULL
    ORDER BY CASE WHEN @useOutputs = 1 THEN o.[Order] ELSE c.Seq END;

    DECLARE @orderby NVARCHAR(MAX) = STUFF((
        SELECT ',' + QUOTENAME(o.Field)
        FROM OPENJSON(@OutputFields)
            WITH (Field NVARCHAR(100) '$.Field', SortOrder INT '$.SortOrder') o
        WHERE @useOutputs = 1 AND o.SortOrder > 0
          AND o.Field IN ('EmployeeNumber','FullName','Gender','HireDate','UnitName','BranchName','PositionTitle','EmploymentNature','EmploymentStatus','IsProbation','Salary')
        ORDER BY o.SortOrder
        FOR XML PATH('')), 1, 1, '');
    IF @orderby IS NULL OR @orderby = '' SET @orderby = N'[HireDate] DESC, [EmployeeNumber]';

    DECLARE @sql NVARCHAR(MAX) = N'
        SELECT e.EmployeeNumber,
               LTRIM(RTRIM(CONCAT(p.FirstName, '' '', p.GrandFatherName))) AS FullName,
               p.Gender,
               e.HireDate,
               ou.Name   AS UnitName,
               b.Name    AS BranchName,
               poc.Title AS PositionTitle,
               e.EmploymentNature,
               e.EmploymentStatus,
               e.IsProbation,
               e.Salary
        FROM Hrms.Employee e
        LEFT JOIN Core.Person p           ON p.Id   = e.PersonId
        LEFT JOIN Hrms.Position pos        ON pos.Id = e.PositionId
        LEFT JOIN Hrms.PositionClass poc   ON poc.Id = pos.PositionClassId
        LEFT JOIN Hrms.OrganizationUnit ou ON ou.Id  = pos.OrganizationUnitId
        LEFT JOIN Hrms.Branch b            ON b.Id   = e.BranchId
        WHERE e.TenantId = @TenantId
          AND (@BranchId IS NULL OR e.BranchId = @BranchId)
          AND (@hire1   IS NULL OR e.HireDate >= @hire1)
          AND (@hire2   IS NULL OR e.HireDate <  DATEADD(DAY, 1, @hire2))
          AND (@unitIds IS NULL OR pos.OrganizationUnitId IN
               (SELECT TRY_CONVERT(UNIQUEIDENTIFIER, LTRIM(RTRIM(value))) FROM STRING_SPLIT(@unitIds, '','')))
          AND (@nature  IS NULL OR e.EmploymentNature = @nature)
        ORDER BY ' + @orderby + N';';

    EXEC sp_executesql @sql,
        N'@TenantId NVARCHAR(64), @BranchId UNIQUEIDENTIFIER, @hire1 DATE, @hire2 DATE, @unitIds NVARCHAR(MAX), @nature NVARCHAR(30)',
        @TenantId, @BranchId, @hire1, @hire2, @unitIds, @nature;
END
GO
IF OBJECT_ID('[Hrms].[Report_ProbationTracking]', 'P') IS NOT NULL DROP PROCEDURE [Hrms].[Report_ProbationTracking];
GO
CREATE OR ALTER PROCEDURE [Hrms].[Report_ProbationTracking]
    @TenantId     NVARCHAR(64),
    @BranchId     UNIQUEIDENTIFIER = NULL,
    @UserId       UNIQUEIDENTIFIER = NULL,
    @ReportKey    NVARCHAR(100),
    @Criteria     NVARCHAR(MAX) = NULL,
    @OutputFields NVARCHAR(MAX) = NULL,
    @Source       NVARCHAR(20)  = NULL,
    @Roles        NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @end1    DATE          = TRY_CONVERT(DATE, JSON_VALUE(@Criteria, '$.ProbationEnd1'));
    DECLARE @end2    DATE          = TRY_CONVERT(DATE, JSON_VALUE(@Criteria, '$.ProbationEnd2'));
    DECLARE @unitIds NVARCHAR(MAX) = NULLIF(JSON_VALUE(@Criteria, '$.OrganizationUnitId'), '');
    DECLARE @useOutputs BIT        = CASE WHEN ISNULL(@OutputFields, '') IN ('', '[]') THEN 0 ELSE 1 END;

    SELECT c.Field, COALESCE(NULLIF(o.Label, ''), c.Label) AS Label, c.[Type], c.Width, c.LinkPage, c.LinkPageValue
    FROM (VALUES
        (1, 'EmployeeNumber',   'Employee #',     'string', 120, CAST(NULL AS NVARCHAR(100)), CAST(NULL AS NVARCHAR(100))),
        (2, 'FullName',         'Full Name',      'string', 220, NULL, NULL),
        (3, 'HireDate',         'Hire Date',      'date',   110, NULL, NULL),
        (4, 'ProbationEndDate', 'Probation Ends', 'date',   120, NULL, NULL),
        (5, 'DaysRemaining',    'Days Remaining', 'number', 110, NULL, NULL),
        (6, 'UnitName',         'Unit',           'string', 180, NULL, NULL),
        (7, 'PositionTitle',    'Position',       'string', 200, NULL, NULL),
        (8, 'EmploymentStatus', 'Status',         'string', 110, NULL, NULL)
    ) c(Seq, Field, Label, [Type], Width, LinkPage, LinkPageValue)
    LEFT JOIN OPENJSON(@OutputFields)
        WITH (Field NVARCHAR(100) '$.Field', Label NVARCHAR(200) '$.Label', [Order] INT '$.Order') o
        ON o.Field = c.Field
    WHERE @useOutputs = 0 OR o.Field IS NOT NULL
    ORDER BY CASE WHEN @useOutputs = 1 THEN o.[Order] ELSE c.Seq END;

    DECLARE @orderby NVARCHAR(MAX) = STUFF((
        SELECT ',' + QUOTENAME(o.Field)
        FROM OPENJSON(@OutputFields)
            WITH (Field NVARCHAR(100) '$.Field', SortOrder INT '$.SortOrder') o
        WHERE @useOutputs = 1 AND o.SortOrder > 0
          AND o.Field IN ('EmployeeNumber','FullName','HireDate','ProbationEndDate','DaysRemaining','UnitName','PositionTitle','EmploymentStatus')
        ORDER BY o.SortOrder
        FOR XML PATH('')), 1, 1, '');
    IF @orderby IS NULL OR @orderby = '' SET @orderby = N'[ProbationEndDate], [EmployeeNumber]';

    DECLARE @sql NVARCHAR(MAX) = N'
        SELECT e.EmployeeNumber,
               LTRIM(RTRIM(CONCAT(p.FirstName, '' '', p.GrandFatherName))) AS FullName,
               e.HireDate,
               e.ProbationEndDate,
               DATEDIFF(DAY, CAST(GETDATE() AS DATE), e.ProbationEndDate) AS DaysRemaining,
               ou.Name   AS UnitName,
               poc.Title AS PositionTitle,
               e.EmploymentStatus
        FROM Hrms.Employee e
        LEFT JOIN Core.Person p           ON p.Id   = e.PersonId
        LEFT JOIN Hrms.Position pos        ON pos.Id = e.PositionId
        LEFT JOIN Hrms.PositionClass poc   ON poc.Id = pos.PositionClassId
        LEFT JOIN Hrms.OrganizationUnit ou ON ou.Id  = pos.OrganizationUnitId
        WHERE e.TenantId = @TenantId
          AND (@BranchId IS NULL OR e.BranchId = @BranchId)
          AND e.IsProbation = 1
          AND e.IsTerminated = 0
          AND (@end1    IS NULL OR e.ProbationEndDate >= @end1)
          AND (@end2    IS NULL OR e.ProbationEndDate <  DATEADD(DAY, 1, @end2))
          AND (@unitIds IS NULL OR pos.OrganizationUnitId IN
               (SELECT TRY_CONVERT(UNIQUEIDENTIFIER, LTRIM(RTRIM(value))) FROM STRING_SPLIT(@unitIds, '','')))
        ORDER BY ' + @orderby + N';';

    EXEC sp_executesql @sql,
        N'@TenantId NVARCHAR(64), @BranchId UNIQUEIDENTIFIER, @end1 DATE, @end2 DATE, @unitIds NVARCHAR(MAX)',
        @TenantId, @BranchId, @end1, @end2, @unitIds;
END
GO
IF OBJECT_ID('[Hrms].[Report_RecruitmentPipeline]', 'P') IS NOT NULL DROP PROCEDURE [Hrms].[Report_RecruitmentPipeline];
GO
CREATE OR ALTER PROCEDURE [Hrms].[Report_RecruitmentPipeline]
    @TenantId     NVARCHAR(64),
    @BranchId     UNIQUEIDENTIFIER = NULL,
    @UserId       UNIQUEIDENTIFIER = NULL,
    @ReportKey    NVARCHAR(100),
    @Criteria     NVARCHAR(MAX) = NULL,
    @OutputFields NVARCHAR(MAX) = NULL,
    @Source       NVARCHAR(20)  = NULL,
    @Roles        NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @app1   DATE         = TRY_CONVERT(DATE, JSON_VALUE(@Criteria, '$.AppliedAt1'));
    DECLARE @app2   DATE         = TRY_CONVERT(DATE, JSON_VALUE(@Criteria, '$.AppliedAt2'));
    DECLARE @stage  NVARCHAR(30) = NULLIF(JSON_VALUE(@Criteria, '$.ApplicationStage'), '');
    DECLARE @rstat  NVARCHAR(30) = NULLIF(JSON_VALUE(@Criteria, '$.RequisitionStatus'), '');
    DECLARE @useOutputs BIT      = CASE WHEN ISNULL(@OutputFields, '') IN ('', '[]') THEN 0 ELSE 1 END;

    DECLARE @groupBy     NVARCHAR(MAX) = NULLIF(JSON_VALUE(@Criteria, '$.__groupBy'), '');
    DECLARE @dir         NVARCHAR(4)   = CASE WHEN LOWER(ISNULL(JSON_VALUE(@Criteria, '$.__groupOrder'), 'asc')) = 'desc' THEN 'DESC' ELSE 'ASC' END;
    DECLARE @showSummary BIT           = CASE WHEN JSON_VALUE(@Criteria, '$.__showSummary') IN ('true','1') THEN 1 ELSE 0 END;
    IF @groupBy IS NULL SET @groupBy = 'ApplicationStage';

    DECLARE @gbJson NVARCHAR(MAX) = '["' + REPLACE(REPLACE(@groupBy, ' ', ''), ',', '","') + '"]';
    DECLARE @groups TABLE (Lvl INT, Field NVARCHAR(100));
    INSERT INTO @groups (Lvl, Field)
    SELECT CAST([key] AS INT), [value]
    FROM OPENJSON(@gbJson)
    WHERE [value] IN ('ApplicationStage', 'RequisitionTitle', 'UnitName');
    IF NOT EXISTS (SELECT 1 FROM @groups) INSERT INTO @groups (Lvl, Field) VALUES (0, 'ApplicationStage');

    DECLARE @groupSel  NVARCHAR(MAX) = STUFF((SELECT ',' + QUOTENAME(Field)             FROM @groups ORDER BY Lvl FOR XML PATH('')), 1, 1, '');
    DECLARE @orderCols NVARCHAR(MAX) = STUFF((SELECT ',' + QUOTENAME(Field) + ' ' + @dir FROM @groups ORDER BY Lvl FOR XML PATH('')), 1, 1, '');

    SELECT c.Field, COALESCE(NULLIF(o.Label, ''), c.Label) AS Label, c.[Type], c.Width, c.LinkPage, c.LinkPageValue
    FROM (VALUES
        (1, 'CandidateNumber',   'Candidate #',   'string', 120, CAST(NULL AS NVARCHAR(100)), CAST(NULL AS NVARCHAR(100))),
        (2, 'CandidateName',     'Candidate',     'string', 220, NULL, NULL),
        (3, 'RequisitionNumber', 'Requisition #', 'string', 130, NULL, NULL),
        (4, 'RequisitionTitle',  'Vacancy',       'string', 200, NULL, NULL),
        (5, 'UnitName',          'Unit',          'string', 180, NULL, NULL),
        (6, 'ApplicationStage',  'Stage',         'string', 120, NULL, NULL),
        (7, 'RequisitionStatus', 'Req. Status',   'string', 120, NULL, NULL),
        (8, 'AppliedAt',         'Applied',       'date',   110, NULL, NULL),
        (9, 'ScreeningScore',    'Screening',     'number', 100, NULL, NULL),
        (10,'Source',            'Source',        'string', 110, NULL, NULL)
    ) c(Seq, Field, Label, [Type], Width, LinkPage, LinkPageValue)
    LEFT JOIN OPENJSON(@OutputFields)
        WITH (Field NVARCHAR(100) '$.Field', Label NVARCHAR(200) '$.Label', [Order] INT '$.Order') o
        ON o.Field = c.Field
    LEFT JOIN @groups g ON g.Field = c.Field
    WHERE g.Field IS NOT NULL OR @useOutputs = 0 OR o.Field IS NOT NULL
    ORDER BY CASE WHEN g.Field IS NOT NULL THEN g.Lvl
                  ELSE 1000 + (CASE WHEN @useOutputs = 1 THEN o.[Order] ELSE c.Seq END) END;

    DECLARE @selectList NVARCHAR(MAX) = N'
        ca.CandidateNumber,
        LTRIM(RTRIM(CONCAT(ca.FirstName, '' '', ca.FatherName))) AS CandidateName,
        r.RequisitionNumber,
        r.Title  AS RequisitionTitle,
        ou.Name  AS UnitName,
        a.Stage  AS ApplicationStage,
        r.Status AS RequisitionStatus,
        a.AppliedAt,
        a.ScreeningScore,
        ca.Source';
    DECLARE @baseFrom NVARCHAR(MAX) = N'
        FROM Hrms.JobApplication a
        INNER JOIN Hrms.Candidate ca       ON ca.Id  = a.CandidateId
        INNER JOIN Hrms.JobRequisition r   ON r.Id   = a.RequisitionId
        LEFT JOIN Hrms.OrganizationUnit ou ON ou.Id  = r.OrganizationUnitId
        WHERE a.TenantId = @TenantId
          AND (@BranchId IS NULL OR ou.BranchId = @BranchId OR ou.BranchId IS NULL)
          AND (@app1  IS NULL OR a.AppliedAt >= @app1)
          AND (@app2  IS NULL OR a.AppliedAt <  DATEADD(DAY, 1, @app2))
          AND (@stage IS NULL OR a.Stage = @stage)
          AND (@rstat IS NULL OR r.Status = @rstat)';
    DECLARE @paramDef NVARCHAR(MAX) =
        N'@TenantId NVARCHAR(64), @BranchId UNIQUEIDENTIFIER, @app1 DATE, @app2 DATE, @stage NVARCHAR(30), @rstat NVARCHAR(30)';

    DECLARE @sql NVARCHAR(MAX) = N'SELECT ' + @selectList + @baseFrom + N'
        ORDER BY ' + @orderCols + N', [AppliedAt] DESC, [CandidateNumber];';
    EXEC sp_executesql @sql, @paramDef, @TenantId, @BranchId, @app1, @app2, @stage, @rstat;

    IF @showSummary = 1
    BEGIN
        DECLARE @sql3 NVARCHAR(MAX) = N'SELECT ' + @groupSel + N', COUNT(*) AS GroupCount
            FROM (SELECT ' + @selectList + @baseFrom + N') d
            GROUP BY ' + @groupSel + N'
            ORDER BY ' + @orderCols + N';';
        EXEC sp_executesql @sql3, @paramDef, @TenantId, @BranchId, @app1, @app2, @stage, @rstat;
    END
END
GO
IF OBJECT_ID('[Hrms].[Report_SalaryRegister]', 'P') IS NOT NULL DROP PROCEDURE [Hrms].[Report_SalaryRegister];
GO
CREATE OR ALTER PROCEDURE [Hrms].[Report_SalaryRegister]
    @TenantId     NVARCHAR(64),
    @BranchId     UNIQUEIDENTIFIER = NULL,
    @UserId       UNIQUEIDENTIFIER = NULL,
    @ReportKey    NVARCHAR(100),
    @Criteria     NVARCHAR(MAX) = NULL,
    @OutputFields NVARCHAR(MAX) = NULL,
    @Source       NVARCHAR(20)  = NULL,
    @Roles        NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @unitIds NVARCHAR(MAX)    = NULLIF(JSON_VALUE(@Criteria, '$.OrganizationUnitId'), '');
    DECLARE @gradeId UNIQUEIDENTIFIER = TRY_CONVERT(UNIQUEIDENTIFIER, JSON_VALUE(@Criteria, '$.JobGradeId'));
    DECLARE @status  NVARCHAR(30)     = NULLIF(JSON_VALUE(@Criteria, '$.EmploymentStatus'), '');
    DECLARE @useOutputs BIT           = CASE WHEN ISNULL(@OutputFields, '') IN ('', '[]') THEN 0 ELSE 1 END;

    DECLARE @groupBy     NVARCHAR(MAX) = NULLIF(JSON_VALUE(@Criteria, '$.__groupBy'), '');
    DECLARE @dir         NVARCHAR(4)   = CASE WHEN LOWER(ISNULL(JSON_VALUE(@Criteria, '$.__groupOrder'), 'asc')) = 'desc' THEN 'DESC' ELSE 'ASC' END;
    DECLARE @showSummary BIT           = CASE WHEN JSON_VALUE(@Criteria, '$.__showSummary') IN ('true','1') THEN 1 ELSE 0 END;
    IF @groupBy IS NULL SET @groupBy = 'UnitName';

    DECLARE @gbJson NVARCHAR(MAX) = '["' + REPLACE(REPLACE(@groupBy, ' ', ''), ',', '","') + '"]';
    DECLARE @groups TABLE (Lvl INT, Field NVARCHAR(100));
    INSERT INTO @groups (Lvl, Field)
    SELECT CAST([key] AS INT), [value]
    FROM OPENJSON(@gbJson)
    WHERE [value] IN ('UnitName', 'BranchName', 'JobGradeName', 'EmploymentStatus');
    IF NOT EXISTS (SELECT 1 FROM @groups) INSERT INTO @groups (Lvl, Field) VALUES (0, 'UnitName');

    DECLARE @groupSel  NVARCHAR(MAX) = STUFF((SELECT ',' + QUOTENAME(Field)             FROM @groups ORDER BY Lvl FOR XML PATH('')), 1, 1, '');
    DECLARE @orderCols NVARCHAR(MAX) = STUFF((SELECT ',' + QUOTENAME(Field) + ' ' + @dir FROM @groups ORDER BY Lvl FOR XML PATH('')), 1, 1, '');

    SELECT c.Field, COALESCE(NULLIF(o.Label, ''), c.Label) AS Label, c.[Type], c.Width, c.LinkPage, c.LinkPageValue
    FROM (VALUES
        (1, 'EmployeeNumber',   'Employee #', 'string',   120, CAST(NULL AS NVARCHAR(100)), CAST(NULL AS NVARCHAR(100))),
        (2, 'FullName',         'Full Name',  'string',   220, NULL, NULL),
        (3, 'UnitName',         'Unit',       'string',   180, NULL, NULL),
        (4, 'BranchName',       'Branch',     'string',   150, NULL, NULL),
        (5, 'PositionTitle',    'Position',   'string',   200, NULL, NULL),
        (6, 'JobGradeName',     'Job Grade',  'string',   130, NULL, NULL),
        (7, 'StepName',         'Step',       'string',   100, NULL, NULL),
        (8, 'EmploymentStatus', 'Status',     'string',   110, NULL, NULL),
        (9, 'HireDate',         'Hire Date',  'date',     110, NULL, NULL),
        (10,'Salary',           'Salary',     'currency', 120, NULL, NULL)
    ) c(Seq, Field, Label, [Type], Width, LinkPage, LinkPageValue)
    LEFT JOIN OPENJSON(@OutputFields)
        WITH (Field NVARCHAR(100) '$.Field', Label NVARCHAR(200) '$.Label', [Order] INT '$.Order') o
        ON o.Field = c.Field
    LEFT JOIN @groups g ON g.Field = c.Field
    WHERE g.Field IS NOT NULL OR @useOutputs = 0 OR o.Field IS NOT NULL
    ORDER BY CASE WHEN g.Field IS NOT NULL THEN g.Lvl
                  ELSE 1000 + (CASE WHEN @useOutputs = 1 THEN o.[Order] ELSE c.Seq END) END;

    DECLARE @selectList NVARCHAR(MAX) = N'
        e.EmployeeNumber,
        LTRIM(RTRIM(CONCAT(p.FirstName, '' '', p.GrandFatherName))) AS FullName,
        ou.Name   AS UnitName,
        b.Name    AS BranchName,
        poc.Title AS PositionTitle,
        jg.Name   AS JobGradeName,
        st.Name   AS StepName,
        e.EmploymentStatus,
        e.HireDate,
        e.Salary';
    DECLARE @baseFrom NVARCHAR(MAX) = N'
        FROM Hrms.Employee e
        LEFT JOIN Core.Person p           ON p.Id   = e.PersonId
        LEFT JOIN Hrms.Position pos        ON pos.Id = e.PositionId
        LEFT JOIN Hrms.PositionClass poc   ON poc.Id = pos.PositionClassId
        LEFT JOIN Hrms.OrganizationUnit ou ON ou.Id  = pos.OrganizationUnitId
        LEFT JOIN Hrms.Branch b            ON b.Id   = e.BranchId
        LEFT JOIN Core.SalaryScale ss     ON ss.Id  = e.SalaryScaleId
        LEFT JOIN Hrms.JobGrade jg         ON jg.Id  = ss.JobGradeId
        LEFT JOIN Core.Step st             ON st.Id  = ss.StepId
        WHERE e.TenantId = @TenantId
          AND (@BranchId IS NULL OR e.BranchId = @BranchId)
          AND (@unitIds  IS NULL OR pos.OrganizationUnitId IN
               (SELECT TRY_CONVERT(UNIQUEIDENTIFIER, LTRIM(RTRIM(value))) FROM STRING_SPLIT(@unitIds, '','')))
          AND (@gradeId  IS NULL OR ss.JobGradeId = @gradeId)
          AND ((@status IS NULL AND e.IsTerminated = 0) OR e.EmploymentStatus = @status)';
    DECLARE @paramDef NVARCHAR(MAX) =
        N'@TenantId NVARCHAR(64), @BranchId UNIQUEIDENTIFIER, @unitIds NVARCHAR(MAX), @gradeId UNIQUEIDENTIFIER, @status NVARCHAR(30)';

    DECLARE @sql NVARCHAR(MAX) = N'SELECT ' + @selectList + @baseFrom + N'
        ORDER BY ' + @orderCols + N', [EmployeeNumber];';
    EXEC sp_executesql @sql, @paramDef, @TenantId, @BranchId, @unitIds, @gradeId, @status;

    IF @showSummary = 1
    BEGIN
        DECLARE @sql3 NVARCHAR(MAX) = N'SELECT ' + @groupSel + N', COUNT(*) AS GroupCount, SUM(d.Salary) AS SalaryTotal
            FROM (SELECT ' + @selectList + @baseFrom + N') d
            GROUP BY ' + @groupSel + N'
            ORDER BY ' + @orderCols + N';';
        EXEC sp_executesql @sql3, @paramDef, @TenantId, @BranchId, @unitIds, @gradeId, @status;
    END
END
GO
IF OBJECT_ID('[Hrms].[Report_Terminations]', 'P') IS NOT NULL DROP PROCEDURE [Hrms].[Report_Terminations];
GO
CREATE OR ALTER PROCEDURE [Hrms].[Report_Terminations]
    @TenantId     NVARCHAR(64),
    @BranchId     UNIQUEIDENTIFIER = NULL,
    @UserId       UNIQUEIDENTIFIER = NULL,
    @ReportKey    NVARCHAR(100),
    @Criteria     NVARCHAR(MAX) = NULL,
    @OutputFields NVARCHAR(MAX) = NULL,
    @Source       NVARCHAR(20)  = NULL,
    @Roles        NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @lwd1   DATE         = TRY_CONVERT(DATE, JSON_VALUE(@Criteria, '$.LastWorkingDate1'));
    DECLARE @lwd2   DATE         = TRY_CONVERT(DATE, JSON_VALUE(@Criteria, '$.LastWorkingDate2'));
    DECLARE @type   NVARCHAR(30) = NULLIF(JSON_VALUE(@Criteria, '$.TerminationType'), '');
    DECLARE @tstat  NVARCHAR(40) = NULLIF(JSON_VALUE(@Criteria, '$.TerminationStatus'), '');
    DECLARE @useOutputs BIT      = CASE WHEN ISNULL(@OutputFields, '') IN ('', '[]') THEN 0 ELSE 1 END;

    DECLARE @groupBy     NVARCHAR(MAX) = NULLIF(JSON_VALUE(@Criteria, '$.__groupBy'), '');
    DECLARE @dir         NVARCHAR(4)   = CASE WHEN LOWER(ISNULL(JSON_VALUE(@Criteria, '$.__groupOrder'), 'asc')) = 'desc' THEN 'DESC' ELSE 'ASC' END;
    DECLARE @showSummary BIT           = CASE WHEN JSON_VALUE(@Criteria, '$.__showSummary') IN ('true','1') THEN 1 ELSE 0 END;
    IF @groupBy IS NULL SET @groupBy = 'TerminationType';

    DECLARE @gbJson NVARCHAR(MAX) = '["' + REPLACE(REPLACE(@groupBy, ' ', ''), ',', '","') + '"]';
    DECLARE @groups TABLE (Lvl INT, Field NVARCHAR(100));
    INSERT INTO @groups (Lvl, Field)
    SELECT CAST([key] AS INT), [value]
    FROM OPENJSON(@gbJson)
    WHERE [value] IN ('TerminationType', 'TerminationStatus', 'UnitName', 'BranchName');
    IF NOT EXISTS (SELECT 1 FROM @groups) INSERT INTO @groups (Lvl, Field) VALUES (0, 'TerminationType');

    DECLARE @groupSel  NVARCHAR(MAX) = STUFF((SELECT ',' + QUOTENAME(Field)             FROM @groups ORDER BY Lvl FOR XML PATH('')), 1, 1, '');
    DECLARE @orderCols NVARCHAR(MAX) = STUFF((SELECT ',' + QUOTENAME(Field) + ' ' + @dir FROM @groups ORDER BY Lvl FOR XML PATH('')), 1, 1, '');

    SELECT c.Field, COALESCE(NULLIF(o.Label, ''), c.Label) AS Label, c.[Type], c.Width, c.LinkPage, c.LinkPageValue
    FROM (VALUES
        (1, 'EmployeeNumber',    'Employee #',       'string', 120, CAST(NULL AS NVARCHAR(100)), CAST(NULL AS NVARCHAR(100))),
        (2, 'FullName',          'Full Name',        'string', 220, NULL, NULL),
        (3, 'UnitName',          'Unit',             'string', 180, NULL, NULL),
        (4, 'BranchName',        'Branch',           'string', 150, NULL, NULL),
        (5, 'PositionTitle',     'Position',         'string', 200, NULL, NULL),
        (6, 'TerminationType',   'Type',             'string', 110, NULL, NULL),
        (7, 'TerminationStatus', 'Case Status',      'string', 150, NULL, NULL),
        (8, 'NoticeDate',        'Notice Date',      'date',   110, NULL, NULL),
        (9, 'LastWorkingDate',   'Last Working Day', 'date',   130, NULL, NULL),
        (10,'TenureYears',       'Tenure (Years)',   'number', 110, NULL, NULL),
        (11,'Reason',            'Reason',           'string', 250, NULL, NULL)
    ) c(Seq, Field, Label, [Type], Width, LinkPage, LinkPageValue)
    LEFT JOIN OPENJSON(@OutputFields)
        WITH (Field NVARCHAR(100) '$.Field', Label NVARCHAR(200) '$.Label', [Order] INT '$.Order') o
        ON o.Field = c.Field
    LEFT JOIN @groups g ON g.Field = c.Field
    WHERE g.Field IS NOT NULL OR @useOutputs = 0 OR o.Field IS NOT NULL
    ORDER BY CASE WHEN g.Field IS NOT NULL THEN g.Lvl
                  ELSE 1000 + (CASE WHEN @useOutputs = 1 THEN o.[Order] ELSE c.Seq END) END;

    DECLARE @selectList NVARCHAR(MAX) = N'
        e.EmployeeNumber,
        LTRIM(RTRIM(CONCAT(p.FirstName, '' '', p.GrandFatherName))) AS FullName,
        ou.Name   AS UnitName,
        b.Name    AS BranchName,
        poc.Title AS PositionTitle,
        t.TerminationType,
        t.Status  AS TerminationStatus,
        t.NoticeDate,
        t.LastWorkingDate,
        CAST(ROUND(DATEDIFF(DAY, e.HireDate, t.LastWorkingDate) / 365.25, 1) AS DECIMAL(6,1)) AS TenureYears,
        t.Reason';
    DECLARE @baseFrom NVARCHAR(MAX) = N'
        FROM Hrms.EmployeeTermination t
        INNER JOIN Hrms.Employee e         ON e.Id   = t.EmployeeId
        LEFT JOIN Core.Person p           ON p.Id   = e.PersonId
        LEFT JOIN Hrms.Position pos        ON pos.Id = e.PositionId
        LEFT JOIN Hrms.PositionClass poc   ON poc.Id = pos.PositionClassId
        LEFT JOIN Hrms.OrganizationUnit ou ON ou.Id  = pos.OrganizationUnitId
        LEFT JOIN Hrms.Branch b            ON b.Id   = e.BranchId
        WHERE t.TenantId = @TenantId
          AND (@BranchId IS NULL OR e.BranchId = @BranchId)
          AND (@lwd1  IS NULL OR t.LastWorkingDate >= @lwd1)
          AND (@lwd2  IS NULL OR t.LastWorkingDate <  DATEADD(DAY, 1, @lwd2))
          AND (@type  IS NULL OR t.TerminationType = @type)
          AND (@tstat IS NULL OR t.Status = @tstat)';
    DECLARE @paramDef NVARCHAR(MAX) =
        N'@TenantId NVARCHAR(64), @BranchId UNIQUEIDENTIFIER, @lwd1 DATE, @lwd2 DATE, @type NVARCHAR(30), @tstat NVARCHAR(40)';

    DECLARE @sql NVARCHAR(MAX) = N'SELECT ' + @selectList + @baseFrom + N'
        ORDER BY ' + @orderCols + N', [LastWorkingDate] DESC, [EmployeeNumber];';
    EXEC sp_executesql @sql, @paramDef, @TenantId, @BranchId, @lwd1, @lwd2, @type, @tstat;

    IF @showSummary = 1
    BEGIN
        DECLARE @sql3 NVARCHAR(MAX) = N'SELECT ' + @groupSel + N', COUNT(*) AS GroupCount
            FROM (SELECT ' + @selectList + @baseFrom + N') d
            GROUP BY ' + @groupSel + N'
            ORDER BY ' + @orderCols + N';';
        EXEC sp_executesql @sql3, @paramDef, @TenantId, @BranchId, @lwd1, @lwd2, @type, @tstat;
    END
END
GO
IF OBJECT_ID('[Hrms].[Report_TrainingCompletion]', 'P') IS NOT NULL DROP PROCEDURE [Hrms].[Report_TrainingCompletion];
GO
CREATE OR ALTER PROCEDURE [Hrms].[Report_TrainingCompletion]
    @TenantId     NVARCHAR(64),
    @BranchId     UNIQUEIDENTIFIER = NULL,
    @UserId       UNIQUEIDENTIFIER = NULL,
    @ReportKey    NVARCHAR(100),
    @Criteria     NVARCHAR(MAX) = NULL,
    @OutputFields NVARCHAR(MAX) = NULL,
    @Source       NVARCHAR(20)  = NULL,
    @Roles        NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @sess1    DATE             = TRY_CONVERT(DATE, JSON_VALUE(@Criteria, '$.SessionStart1'));
    DECLARE @sess2    DATE             = TRY_CONVERT(DATE, JSON_VALUE(@Criteria, '$.SessionStart2'));
    DECLARE @courseId UNIQUEIDENTIFIER = TRY_CONVERT(UNIQUEIDENTIFIER, JSON_VALUE(@Criteria, '$.TrainingCourseId'));
    DECLARE @estat    NVARCHAR(30)     = NULLIF(JSON_VALUE(@Criteria, '$.EnrollmentStatus'), '');
    DECLARE @unitIds  NVARCHAR(MAX)    = NULLIF(JSON_VALUE(@Criteria, '$.OrganizationUnitId'), '');
    DECLARE @useOutputs BIT            = CASE WHEN ISNULL(@OutputFields, '') IN ('', '[]') THEN 0 ELSE 1 END;

    SELECT c.Field, COALESCE(NULLIF(o.Label, ''), c.Label) AS Label, c.[Type], c.Width, c.LinkPage, c.LinkPageValue
    FROM (VALUES
        (1, 'EmployeeNumber',   'Employee #',   'string', 120, CAST(NULL AS NVARCHAR(100)), CAST(NULL AS NVARCHAR(100))),
        (2, 'FullName',         'Full Name',    'string', 220, NULL, NULL),
        (3, 'UnitName',         'Unit',         'string', 180, NULL, NULL),
        (4, 'CourseName',       'Course',       'string', 200, NULL, NULL),
        (5, 'SessionStart',     'Session From', 'date',   110, NULL, NULL),
        (6, 'SessionEnd',       'Session To',   'date',   110, NULL, NULL),
        (7, 'DeliveryMode',     'Delivery',     'string', 110, NULL, NULL),
        (8, 'EnrollmentStatus', 'Status',       'string', 110, NULL, NULL),
        (9, 'AttendancePercent','Attendance %', 'number', 110, NULL, NULL),
        (10,'AssessmentScore',  'Score',        'number', 100, NULL, NULL),
        (11,'CompletedOn',      'Completed On', 'date',   110, NULL, NULL),
        (12,'FeedbackRating',   'Feedback',     'number', 100, NULL, NULL)
    ) c(Seq, Field, Label, [Type], Width, LinkPage, LinkPageValue)
    LEFT JOIN OPENJSON(@OutputFields)
        WITH (Field NVARCHAR(100) '$.Field', Label NVARCHAR(200) '$.Label', [Order] INT '$.Order') o
        ON o.Field = c.Field
    WHERE @useOutputs = 0 OR o.Field IS NOT NULL
    ORDER BY CASE WHEN @useOutputs = 1 THEN o.[Order] ELSE c.Seq END;

    DECLARE @orderby NVARCHAR(MAX) = STUFF((
        SELECT ',' + QUOTENAME(o.Field)
        FROM OPENJSON(@OutputFields)
            WITH (Field NVARCHAR(100) '$.Field', SortOrder INT '$.SortOrder') o
        WHERE @useOutputs = 1 AND o.SortOrder > 0
          AND o.Field IN ('EmployeeNumber','FullName','UnitName','CourseName','SessionStart','SessionEnd','DeliveryMode','EnrollmentStatus','AttendancePercent','AssessmentScore','CompletedOn','FeedbackRating')
        ORDER BY o.SortOrder
        FOR XML PATH('')), 1, 1, '');
    IF @orderby IS NULL OR @orderby = '' SET @orderby = N'[SessionStart] DESC, [EmployeeNumber]';

    DECLARE @sql NVARCHAR(MAX) = N'
        SELECT e.EmployeeNumber,
               LTRIM(RTRIM(CONCAT(p.FirstName, '' '', p.GrandFatherName))) AS FullName,
               ou.Name  AS UnitName,
               tc.Name  AS CourseName,
               ts.StartDate AS SessionStart,
               ts.EndDate   AS SessionEnd,
               tc.DeliveryMode,
               te.Status AS EnrollmentStatus,
               te.AttendancePercent,
               te.AssessmentScore,
               te.CompletedOn,
               te.FeedbackRating
        FROM Hrms.TrainingEnrollment te
        INNER JOIN Hrms.TrainingSession ts ON ts.Id  = te.TrainingSessionId
        INNER JOIN Hrms.TrainingCourse tc  ON tc.Id  = ts.TrainingCourseId
        INNER JOIN Hrms.Employee e         ON e.Id   = te.EmployeeId
        LEFT JOIN Core.Person p           ON p.Id   = e.PersonId
        LEFT JOIN Hrms.Position pos        ON pos.Id = e.PositionId
        LEFT JOIN Hrms.OrganizationUnit ou ON ou.Id  = pos.OrganizationUnitId
        WHERE te.TenantId = @TenantId
          AND (@BranchId IS NULL OR e.BranchId = @BranchId)
          AND (@sess1    IS NULL OR ts.StartDate >= @sess1)
          AND (@sess2    IS NULL OR ts.StartDate <  DATEADD(DAY, 1, @sess2))
          AND (@courseId IS NULL OR ts.TrainingCourseId = @courseId)
          AND (@estat    IS NULL OR te.Status = @estat)
          AND (@unitIds  IS NULL OR pos.OrganizationUnitId IN
               (SELECT TRY_CONVERT(UNIQUEIDENTIFIER, LTRIM(RTRIM(value))) FROM STRING_SPLIT(@unitIds, '','')))
        ORDER BY ' + @orderby + N';';

    EXEC sp_executesql @sql,
        N'@TenantId NVARCHAR(64), @BranchId UNIQUEIDENTIFIER, @sess1 DATE, @sess2 DATE, @courseId UNIQUEIDENTIFIER, @estat NVARCHAR(30), @unitIds NVARCHAR(MAX)',
        @TenantId, @BranchId, @sess1, @sess2, @courseId, @estat, @unitIds;
END
GO
IF OBJECT_ID('[Hrms].[Report_VacantPositions]', 'P') IS NOT NULL DROP PROCEDURE [Hrms].[Report_VacantPositions];
GO
CREATE OR ALTER PROCEDURE [Hrms].[Report_VacantPositions]
    @TenantId     NVARCHAR(64),
    @BranchId     UNIQUEIDENTIFIER = NULL,
    @UserId       UNIQUEIDENTIFIER = NULL,
    @ReportKey    NVARCHAR(100),
    @Criteria     NVARCHAR(MAX) = NULL,
    @OutputFields NVARCHAR(MAX) = NULL,
    @Source       NVARCHAR(20)  = NULL,
    @Roles        NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @unitIds NVARCHAR(MAX)    = NULLIF(JSON_VALUE(@Criteria, '$.OrganizationUnitId'), '');
    DECLARE @classId UNIQUEIDENTIFIER = TRY_CONVERT(UNIQUEIDENTIFIER, JSON_VALUE(@Criteria, '$.PositionClassId'));
    DECLARE @useOutputs BIT           = CASE WHEN ISNULL(@OutputFields, '') IN ('', '[]') THEN 0 ELSE 1 END;

    SELECT c.Field, COALESCE(NULLIF(o.Label, ''), c.Label) AS Label, c.[Type], c.Width, c.LinkPage, c.LinkPageValue
    FROM (VALUES
        (1, 'PositionCode',       'Position Code',   'string', 130, CAST(NULL AS NVARCHAR(100)), CAST(NULL AS NVARCHAR(100))),
        (2, 'PositionTitle',      'Position Title',  'string', 220, NULL, NULL),
        (3, 'UnitName',           'Unit',            'string', 180, NULL, NULL),
        (4, 'BranchName',         'Branch',          'string', 150, NULL, NULL),
        (5, 'MinQualifications',  'Qualifications',  'string', 220, NULL, NULL),
        (6, 'MinExperienceYears', 'Min Exp (Years)', 'number', 110, NULL, NULL),
        (7, 'VacantSince',        'Vacant Since',    'date',   110, NULL, NULL)
    ) c(Seq, Field, Label, [Type], Width, LinkPage, LinkPageValue)
    LEFT JOIN OPENJSON(@OutputFields)
        WITH (Field NVARCHAR(100) '$.Field', Label NVARCHAR(200) '$.Label', [Order] INT '$.Order') o
        ON o.Field = c.Field
    WHERE @useOutputs = 0 OR o.Field IS NOT NULL
    ORDER BY CASE WHEN @useOutputs = 1 THEN o.[Order] ELSE c.Seq END;

    DECLARE @orderby NVARCHAR(MAX) = STUFF((
        SELECT ',' + QUOTENAME(o.Field)
        FROM OPENJSON(@OutputFields)
            WITH (Field NVARCHAR(100) '$.Field', SortOrder INT '$.SortOrder') o
        WHERE @useOutputs = 1 AND o.SortOrder > 0
          AND o.Field IN ('PositionCode','PositionTitle','UnitName','BranchName','MinQualifications','MinExperienceYears','VacantSince')
        ORDER BY o.SortOrder
        FOR XML PATH('')), 1, 1, '');
    IF @orderby IS NULL OR @orderby = '' SET @orderby = N'[UnitName], [PositionCode]';

    -- VacantSince approximates from the position row''s last update (vacancy sync touches it).
    DECLARE @sql NVARCHAR(MAX) = N'
        SELECT pos.Code  AS PositionCode,
               poc.Title AS PositionTitle,
               ou.Name   AS UnitName,
               b.Name    AS BranchName,
               poc.MinQualifications,
               poc.MinExperienceYears,
               CAST(COALESCE(pos.UpdatedAt, pos.CreatedAt) AS DATE) AS VacantSince
        FROM Hrms.Position pos
        LEFT JOIN Hrms.PositionClass poc   ON poc.Id = pos.PositionClassId
        LEFT JOIN Hrms.OrganizationUnit ou ON ou.Id  = pos.OrganizationUnitId
        LEFT JOIN Hrms.Branch b            ON b.Id   = pos.BranchId
        WHERE pos.TenantId = @TenantId
          AND (@BranchId IS NULL OR pos.BranchId = @BranchId)
          AND pos.IsVacant = 1
          AND (@unitIds IS NULL OR pos.OrganizationUnitId IN
               (SELECT TRY_CONVERT(UNIQUEIDENTIFIER, LTRIM(RTRIM(value))) FROM STRING_SPLIT(@unitIds, '','')))
          AND (@classId IS NULL OR pos.PositionClassId = @classId)
        ORDER BY ' + @orderby + N';';

    EXEC sp_executesql @sql,
        N'@TenantId NVARCHAR(64), @BranchId UNIQUEIDENTIFIER, @unitIds NVARCHAR(MAX), @classId UNIQUEIDENTIFIER',
        @TenantId, @BranchId, @unitIds, @classId;
END
GO
UPDATE [Hrms].[Report]
SET StoredProc = REPLACE(REPLACE(StoredProc, '[Core].[hrms_', '[Hrms].['), 'Core.hrms_', 'Hrms.')
WHERE StoredProc LIKE '%Core%hrms[_]%';

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260808161031_ModuleSchemaRename', N'10.0.8');

COMMIT;
GO


GO
SET NOEXEC OFF;
GO

