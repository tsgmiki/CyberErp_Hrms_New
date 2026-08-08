using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <inheritdoc />
    public partial class ModuleSchemaRename : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_coreModule_coreSubsystem_SubsystemId",
                schema: "dbo",
                table: "coreModule");

            migrationBuilder.DropForeignKey(
                name: "FK_coreOperation_coreModule_ModuleId",
                schema: "dbo",
                table: "coreOperation");

            migrationBuilder.DropForeignKey(
                name: "FK_coreSalaryScale_hrmsJobGrade_JobGradeId",
                schema: "Core",
                table: "coreSalaryScale");

            migrationBuilder.DropForeignKey(
                name: "FK_coreSalaryScale_lupStep_StepId",
                schema: "Core",
                table: "coreSalaryScale");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsAchievement_hrmsAppraisal_AppraisalId",
                schema: "dbo",
                table: "hrmsAchievement");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsAchievement_hrmsEmployee_EmployeeId",
                schema: "dbo",
                table: "hrmsAchievement");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsAnnouncement_hrmsBranch_BranchId",
                schema: "dbo",
                table: "hrmsAnnouncement");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsAnnouncement_hrmsOrganizationUnit_OrganizationUnitId",
                schema: "dbo",
                table: "hrmsAnnouncement");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsAnnualLeaveDetail_hrmsAnnualLeaveHeader_AnnualLeaveHeaderId",
                schema: "dbo",
                table: "hrmsAnnualLeaveDetail");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsAnnualLeaveHeader_hrmsEmployee_EmployeeId",
                schema: "dbo",
                table: "hrmsAnnualLeaveHeader");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsAnnualLeaveHeader_hrmsLeaveBalance_AnnualLeaveLedgerId",
                schema: "dbo",
                table: "hrmsAnnualLeaveHeader");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsAnnualLeaveSetting_FiscalYear_FiscalYearId",
                schema: "dbo",
                table: "hrmsAnnualLeaveSetting");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsApplicationCriterionScore_hrmsJobApplication_ApplicationId",
                schema: "dbo",
                table: "hrmsApplicationCriterionScore");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsAppraisal_hrmsEmployee_EmployeeId",
                schema: "dbo",
                table: "hrmsAppraisal");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsAppraisal_hrmsReviewCycle_ReviewCycleId",
                schema: "dbo",
                table: "hrmsAppraisal");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsAppraisalAppeal_hrmsAppraisal_AppraisalId",
                schema: "dbo",
                table: "hrmsAppraisalAppeal");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsAppraisalAppeal_hrmsEmployee_EmployeeId",
                schema: "dbo",
                table: "hrmsAppraisalAppeal");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsAppraisalCompetency_hrmsAppraisal_AppraisalId",
                schema: "dbo",
                table: "hrmsAppraisalCompetency");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsAppraisalGoal_hrmsAppraisal_AppraisalId",
                schema: "dbo",
                table: "hrmsAppraisalGoal");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsAppraisalPeerReview_hrmsAppraisal_AppraisalId",
                schema: "dbo",
                table: "hrmsAppraisalPeerReview");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsAppraisalPeerReview_hrmsEmployee_PeerEmployeeId",
                schema: "dbo",
                table: "hrmsAppraisalPeerReview");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsBranch_hrmsBranch_ParentId",
                schema: "dbo",
                table: "hrmsBranch");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsCalibrationItem_hrmsAppraisal_AppraisalId",
                schema: "dbo",
                table: "hrmsCalibrationItem");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsCalibrationItem_hrmsCalibrationSession_CalibrationSessionId",
                schema: "dbo",
                table: "hrmsCalibrationItem");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsCalibrationSession_hrmsOrganizationUnit_OrganizationUnitId",
                schema: "dbo",
                table: "hrmsCalibrationSession");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsCalibrationSession_hrmsReviewCycle_ReviewCycleId",
                schema: "dbo",
                table: "hrmsCalibrationSession");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsCandidate_CorePerson_PersonId",
                schema: "dbo",
                table: "hrmsCandidate");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsCandidate_hrmsEmployee_InternalEmployeeId",
                schema: "dbo",
                table: "hrmsCandidate");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsCandidateDocument_hrmsCandidate_CandidateId",
                schema: "dbo",
                table: "hrmsCandidateDocument");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsCareerPathChangeRequest_hrmsCareerPath_CurrentCareerPathId",
                schema: "dbo",
                table: "hrmsCareerPathChangeRequest");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsCareerPathChangeRequest_hrmsCareerPath_RequestedCareerPathId",
                schema: "dbo",
                table: "hrmsCareerPathChangeRequest");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsCareerPathChangeRequest_hrmsEmployee_EmployeeId",
                schema: "dbo",
                table: "hrmsCareerPathChangeRequest");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsCareerPathStep_hrmsCareerPath_CareerPathId",
                schema: "dbo",
                table: "hrmsCareerPathStep");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsCareerPathStep_hrmsJobGrade_JobGradeId",
                schema: "dbo",
                table: "hrmsCareerPathStep");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsCareerPathStep_hrmsPositionClass_PositionClassId",
                schema: "dbo",
                table: "hrmsCareerPathStep");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsCareerPathStepCompetency_hrmsCareerPathStep_CareerPathStepId",
                schema: "dbo",
                table: "hrmsCareerPathStepCompetency");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsCareerPathStepCompetency_hrmsCompetency_CompetencyId",
                schema: "dbo",
                table: "hrmsCareerPathStepCompetency");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsClearanceDepartmentApprover_hrmsClearanceDepartment_DepartmentId",
                schema: "dbo",
                table: "hrmsClearanceDepartmentApprover");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsCommunityPostReaction_hrmsEmployee_EmployeeId",
                schema: "dbo",
                table: "hrmsCommunityPostReaction");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsCommunityPostReaction_hrmsLearningCommunityPost_LearningCommunityPostId",
                schema: "dbo",
                table: "hrmsCommunityPostReaction");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsCompanyAsset_hrmsEmployee_AssignedToEmployeeId",
                schema: "dbo",
                table: "hrmsCompanyAsset");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsCompensationRequest_hrmsEmployee_EmployeeId",
                schema: "dbo",
                table: "hrmsCompensationRequest");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsCompetency_hrmsCompetencyCategory_CompetencyCategoryId",
                schema: "dbo",
                table: "hrmsCompetency");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsCriterionEvaluator_hrmsEmployee_EmployeeId",
                schema: "dbo",
                table: "hrmsCriterionEvaluator");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsCriterionEvaluator_hrmsRequisitionScreeningCriterion_CriterionId",
                schema: "dbo",
                table: "hrmsCriterionEvaluator");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsCriticalPosition_hrmsPosition_PositionId",
                schema: "dbo",
                table: "hrmsCriticalPosition");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsDevelopmentAction_hrmsCompetency_CompetencyId",
                schema: "dbo",
                table: "hrmsDevelopmentAction");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsDevelopmentAction_hrmsDevelopmentPlan_DevelopmentPlanId",
                schema: "dbo",
                table: "hrmsDevelopmentAction");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsDevelopmentPlan_hrmsAppraisal_AppraisalId",
                schema: "dbo",
                table: "hrmsDevelopmentPlan");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsDevelopmentPlan_hrmsEmployee_EmployeeId",
                schema: "dbo",
                table: "hrmsDevelopmentPlan");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsDisciplinaryMeasure_hrmsEmployee_EmployeeId",
                schema: "dbo",
                table: "hrmsDisciplinaryMeasure");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsDynamicFormField_hrmsDynamicForm_DynamicFormId",
                schema: "dbo",
                table: "hrmsDynamicFormField");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsDynamicFormRecord_hrmsDynamicForm_DynamicFormId",
                schema: "dbo",
                table: "hrmsDynamicFormRecord");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsEmployee_CorePerson_PersonId",
                schema: "dbo",
                table: "hrmsEmployee");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsEmployee_coreSalaryScale_SalaryScaleId",
                schema: "dbo",
                table: "hrmsEmployee");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsEmployee_hrmsBranch_BranchId",
                schema: "dbo",
                table: "hrmsEmployee");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsEmployee_hrmsPosition_PositionId",
                schema: "dbo",
                table: "hrmsEmployee");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsEmployeeAllowance_hrmsAllowanceType_AllowanceTypeId",
                schema: "dbo",
                table: "hrmsEmployeeAllowance");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsEmployeeAllowance_hrmsEmployee_EmployeeId",
                schema: "dbo",
                table: "hrmsEmployeeAllowance");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsEmployeeBenefitEnrollment_hrmsBenefitPlan_BenefitPlanId",
                schema: "dbo",
                table: "hrmsEmployeeBenefitEnrollment");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsEmployeeBenefitEnrollment_hrmsEmployee_EmployeeId",
                schema: "dbo",
                table: "hrmsEmployeeBenefitEnrollment");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsEmployeeCareerPath_hrmsCareerPath_CareerPathId",
                schema: "dbo",
                table: "hrmsEmployeeCareerPath");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsEmployeeCareerPath_hrmsEmployee_EmployeeId",
                schema: "dbo",
                table: "hrmsEmployeeCareerPath");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsEmployeeCareerPathStepProgress_hrmsEmployeeCareerPath_EmployeeCareerPathId",
                schema: "dbo",
                table: "hrmsEmployeeCareerPathStepProgress");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsEmployeeDependent_CorePerson_PersonId",
                schema: "dbo",
                table: "hrmsEmployeeDependent");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsEmployeeDependent_hrmsEmployee_RelatedEmployeeId",
                schema: "dbo",
                table: "hrmsEmployeeDependent");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsEmployeeEducation_CorePerson_PersonId",
                schema: "dbo",
                table: "hrmsEmployeeEducation");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsEmployeeExperience_CorePerson_PersonId",
                schema: "dbo",
                table: "hrmsEmployeeExperience");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsEmployeeFieldValue_hrmsEmployeeFieldDefinition_FieldDefinitionId",
                schema: "dbo",
                table: "hrmsEmployeeFieldValue");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsEmployeeGoal_hrmsEmployee_EmployeeId",
                schema: "dbo",
                table: "hrmsEmployeeGoal");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsEmployeeGoal_hrmsOrganizationalObjective_OrganizationalObjectiveId",
                schema: "dbo",
                table: "hrmsEmployeeGoal");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsEmployeeGoal_hrmsReviewCycle_ReviewCycleId",
                schema: "dbo",
                table: "hrmsEmployeeGoal");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsEmployeeGuarantee_hrmsEmployee_EmployeeId",
                schema: "dbo",
                table: "hrmsEmployeeGuarantee");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsEmployeeMovement_coreSalaryScale_ToSalaryScaleId",
                schema: "dbo",
                table: "hrmsEmployeeMovement");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsEmployeeMovement_hrmsEmployee_EmployeeId",
                schema: "dbo",
                table: "hrmsEmployeeMovement");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsEmployeeRecognition_hrmsEmployee_EmployeeId",
                schema: "dbo",
                table: "hrmsEmployeeRecognition");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsEmployeeRecognition_hrmsRecognitionBadge_RecognitionBadgeId",
                schema: "dbo",
                table: "hrmsEmployeeRecognition");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsEmployeeTermination_hrmsEmployee_EmployeeId",
                schema: "dbo",
                table: "hrmsEmployeeTermination");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsEmployeeTrainingCertificate_hrmsEmployee_EmployeeId",
                schema: "dbo",
                table: "hrmsEmployeeTrainingCertificate");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsEmployeeTrainingCertificate_hrmsTrainingCourse_TrainingCourseId",
                schema: "dbo",
                table: "hrmsEmployeeTrainingCertificate");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsEmployeeTrainingCertificate_hrmsTrainingEnrollment_TrainingEnrollmentId",
                schema: "dbo",
                table: "hrmsEmployeeTrainingCertificate");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsExitInterview_hrmsEmployeeTermination_TerminationId",
                schema: "dbo",
                table: "hrmsExitInterview");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsGoalActionItem_hrmsEmployeeGoal_EmployeeGoalId",
                schema: "dbo",
                table: "hrmsGoalActionItem");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsGrievance_hrmsEmployee_EmployeeId",
                schema: "dbo",
                table: "hrmsGrievance");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsGrievanceNote_hrmsGrievance_GrievanceId",
                schema: "dbo",
                table: "hrmsGrievanceNote");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsHiringRequest_hrmsOrganizationUnit_OrganizationUnitId",
                schema: "dbo",
                table: "hrmsHiringRequest");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsHiringRequest_hrmsPositionClass_PositionClassId",
                schema: "dbo",
                table: "hrmsHiringRequest");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsImprovementPlan_hrmsAppraisal_AppraisalId",
                schema: "dbo",
                table: "hrmsImprovementPlan");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsImprovementPlan_hrmsEmployee_EmployeeId",
                schema: "dbo",
                table: "hrmsImprovementPlan");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsInsuranceClaim_hrmsEmployee_EmployeeId",
                schema: "dbo",
                table: "hrmsInsuranceClaim");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsInsuranceClaim_hrmsInsurancePolicy_InsurancePolicyId",
                schema: "dbo",
                table: "hrmsInsuranceClaim");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsInsuranceClaimAttachment_hrmsInsuranceClaim_InsuranceClaimId",
                schema: "dbo",
                table: "hrmsInsuranceClaimAttachment");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsInsurancePremiumSchedule_hrmsInsurancePolicy_InsurancePolicyId",
                schema: "dbo",
                table: "hrmsInsurancePremiumSchedule");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsInterview_hrmsJobApplication_ApplicationId",
                schema: "dbo",
                table: "hrmsInterview");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsInterviewFeedback_hrmsInterviewPanelist_PanelistId",
                schema: "dbo",
                table: "hrmsInterviewFeedback");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsInterviewPanelist_hrmsEmployee_EmployeeId",
                schema: "dbo",
                table: "hrmsInterviewPanelist");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsInterviewPanelist_hrmsInterview_InterviewId",
                schema: "dbo",
                table: "hrmsInterviewPanelist");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsJobApplication_hrmsCandidate_CandidateId",
                schema: "dbo",
                table: "hrmsJobApplication");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsJobApplication_hrmsJobRequisition_RequisitionId",
                schema: "dbo",
                table: "hrmsJobApplication");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsJobApplicationStageLog_hrmsJobApplication_ApplicationId",
                schema: "dbo",
                table: "hrmsJobApplicationStageLog");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsJobOffer_coreSalaryScale_SalaryScaleId",
                schema: "dbo",
                table: "hrmsJobOffer");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsJobOffer_hrmsEmployee_HiringManagerEmployeeId",
                schema: "dbo",
                table: "hrmsJobOffer");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsJobOffer_hrmsJobApplication_ApplicationId",
                schema: "dbo",
                table: "hrmsJobOffer");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsJobRequisition_coreSalaryScale_SalaryScaleId",
                schema: "dbo",
                table: "hrmsJobRequisition");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsJobRequisition_hrmsHiringRequest_HiringRequestId",
                schema: "dbo",
                table: "hrmsJobRequisition");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsJobRequisition_hrmsOrganizationUnit_OrganizationUnitId",
                schema: "dbo",
                table: "hrmsJobRequisition");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsJobRequisition_hrmsPositionClass_PositionClassId",
                schema: "dbo",
                table: "hrmsJobRequisition");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsJobRequisition_hrmsWorkLocation_WorkLocationId",
                schema: "dbo",
                table: "hrmsJobRequisition");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsKnowledgeTransfer_hrmsEmployee_FromEmployeeId",
                schema: "dbo",
                table: "hrmsKnowledgeTransfer");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsKnowledgeTransfer_hrmsSuccessionCandidate_SuccessionCandidateId",
                schema: "dbo",
                table: "hrmsKnowledgeTransfer");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsLearningCommunity_hrmsTrainingCourse_TrainingCourseId",
                schema: "dbo",
                table: "hrmsLearningCommunity");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsLearningCommunityMember_hrmsEmployee_EmployeeId",
                schema: "dbo",
                table: "hrmsLearningCommunityMember");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsLearningCommunityMember_hrmsLearningCommunity_LearningCommunityId",
                schema: "dbo",
                table: "hrmsLearningCommunityMember");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsLearningCommunityPost_hrmsEmployee_EmployeeId",
                schema: "dbo",
                table: "hrmsLearningCommunityPost");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsLearningCommunityPost_hrmsLearningCommunity_LearningCommunityId",
                schema: "dbo",
                table: "hrmsLearningCommunityPost");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsLearningPath_hrmsPosition_TargetPositionId",
                schema: "dbo",
                table: "hrmsLearningPath");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsLearningPathStep_hrmsLearningPath_LearningPathId",
                schema: "dbo",
                table: "hrmsLearningPathStep");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsLearningPathStep_hrmsTrainingCourse_TrainingCourseId",
                schema: "dbo",
                table: "hrmsLearningPathStep");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsLeaveBalance_FiscalYear_FiscalYearId",
                schema: "dbo",
                table: "hrmsLeaveBalance");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsLeaveBalance_hrmsEmployee_EmployeeId",
                schema: "dbo",
                table: "hrmsLeaveBalance");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsLeaveBalance_hrmsLeaveType_LeaveTypeId",
                schema: "dbo",
                table: "hrmsLeaveBalance");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsLeaveRequest_FiscalYear_FiscalYearId",
                schema: "dbo",
                table: "hrmsLeaveRequest");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsLeaveRequest_hrmsEmployee_EmployeeId",
                schema: "dbo",
                table: "hrmsLeaveRequest");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsLeaveRequestLine_hrmsLeaveRequest_LeaveRequestId",
                schema: "dbo",
                table: "hrmsLeaveRequestLine");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsLeaveRequestLine_hrmsLeaveType_LeaveTypeId",
                schema: "dbo",
                table: "hrmsLeaveRequestLine");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsLoan_hrmsEmployee_EmployeeId",
                schema: "dbo",
                table: "hrmsLoan");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsLoan_hrmsLoanType_LoanTypeId",
                schema: "dbo",
                table: "hrmsLoan");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsLoanGuarantor_hrmsLoan_LoanId",
                schema: "dbo",
                table: "hrmsLoanGuarantor");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsLoanRepaymentSchedule_hrmsLoan_LoanId",
                schema: "dbo",
                table: "hrmsLoanRepaymentSchedule");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsMedicalBeneficiary_hrmsMedicalEnrollment_MedicalEnrollmentId",
                schema: "dbo",
                table: "hrmsMedicalBeneficiary");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsMedicalClaim_hrmsEmployee_EmployeeId",
                schema: "dbo",
                table: "hrmsMedicalClaim");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsMedicalClaim_hrmsMedicalEnrollment_MedicalEnrollmentId",
                schema: "dbo",
                table: "hrmsMedicalClaim");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsMedicalClaimAttachment_hrmsMedicalClaim_MedicalClaimId",
                schema: "dbo",
                table: "hrmsMedicalClaimAttachment");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsMedicalEnrollment_hrmsEmployee_EmployeeId",
                schema: "dbo",
                table: "hrmsMedicalEnrollment");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsMedicalEnrollment_hrmsMedicalPlan_MedicalPlanId",
                schema: "dbo",
                table: "hrmsMedicalEnrollment");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsMedicalServiceContract_hrmsMedicalProvider_MedicalProviderId",
                schema: "dbo",
                table: "hrmsMedicalServiceContract");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsMentorship_hrmsEmployee_MenteeEmployeeId",
                schema: "dbo",
                table: "hrmsMentorship");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsMentorship_hrmsEmployee_MentorEmployeeId",
                schema: "dbo",
                table: "hrmsMentorship");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsOrganizationalObjective_hrmsOrganizationUnit_OrganizationUnitId",
                schema: "dbo",
                table: "hrmsOrganizationalObjective");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsOrganizationalObjective_hrmsOrganizationalObjective_ParentObjectiveId",
                schema: "dbo",
                table: "hrmsOrganizationalObjective");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsOrganizationalObjective_hrmsReviewCycle_ReviewCycleId",
                schema: "dbo",
                table: "hrmsOrganizationalObjective");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsOrganizationUnit_hrmsBranch_BranchId",
                schema: "dbo",
                table: "hrmsOrganizationUnit");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsOrganizationUnit_hrmsOrganizationUnit_ParentId",
                schema: "dbo",
                table: "hrmsOrganizationUnit");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsOrganizationUnit_hrmsWorkLocation_WorkLocationId",
                schema: "dbo",
                table: "hrmsOrganizationUnit");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsOtherLeave_hrmsEmployee_EmployeeId",
                schema: "dbo",
                table: "hrmsOtherLeave");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsOtherLeave_hrmsOtherLeaveSetting_OtherLeaveSettingId",
                schema: "dbo",
                table: "hrmsOtherLeave");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsOtherLeaveDetail_hrmsOtherLeave_OtherLeaveHeaderId",
                schema: "dbo",
                table: "hrmsOtherLeaveDetail");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsOtherLeaveSetting_FiscalYear_FiscalYearId",
                schema: "dbo",
                table: "hrmsOtherLeaveSetting");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsOtherLeaveSetting_hrmsLeaveType_LeaveTypeId",
                schema: "dbo",
                table: "hrmsOtherLeaveSetting");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsPerDiemRate_hrmsJobGrade_JobGradeId",
                schema: "dbo",
                table: "hrmsPerDiemRate");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsPipObjective_hrmsImprovementPlan_PipId",
                schema: "dbo",
                table: "hrmsPipObjective");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsPosition_hrmsBranch_BranchId",
                schema: "dbo",
                table: "hrmsPosition");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsPosition_hrmsOrganizationUnit_OrganizationUnitId",
                schema: "dbo",
                table: "hrmsPosition");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsPosition_hrmsPositionClass_PositionClassId",
                schema: "dbo",
                table: "hrmsPosition");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsPositionClass_coreSalaryScale_SalaryScaleId",
                schema: "dbo",
                table: "hrmsPositionClass");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsPositionClass_hrmsJobCategory_JobCategoryId",
                schema: "dbo",
                table: "hrmsPositionClass");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsPositionClass_hrmsPositionClass_ReportsToPositionClassId",
                schema: "dbo",
                table: "hrmsPositionClass");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsPositionClass_hrmsWorkLocation_WorkLocationId",
                schema: "dbo",
                table: "hrmsPositionClass");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsPositionCompetency_hrmsCompetency_CompetencyId",
                schema: "dbo",
                table: "hrmsPositionCompetency");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsPositionCompetency_hrmsPosition_PositionId",
                schema: "dbo",
                table: "hrmsPositionCompetency");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsProfileChangeRequest_hrmsEmployee_EmployeeId",
                schema: "dbo",
                table: "hrmsProfileChangeRequest");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsRatingScaleLevel_hrmsRatingScale_RatingScaleId",
                schema: "dbo",
                table: "hrmsRatingScaleLevel");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsRecognitionBadge_hrmsAwardCategory_AwardCategoryId",
                schema: "dbo",
                table: "hrmsRecognitionBadge");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsRecognitionProgram_hrmsRecognitionBadge_RecognitionBadgeId",
                schema: "dbo",
                table: "hrmsRecognitionProgram");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsReportField_hrmsReport_ReportId",
                schema: "dbo",
                table: "hrmsReportField");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsReportFieldOutput_hrmsReport_ReportId",
                schema: "dbo",
                table: "hrmsReportFieldOutput");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsReportRestriction_hrmsReport_ReportId",
                schema: "dbo",
                table: "hrmsReportRestriction");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsReportRunRecipient_hrmsReportRun_ReportRunId",
                schema: "dbo",
                table: "hrmsReportRunRecipient");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsReportSavedFilter_hrmsReport_ReportId",
                schema: "dbo",
                table: "hrmsReportSavedFilter");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsReportSchedule_hrmsReport_ReportId",
                schema: "dbo",
                table: "hrmsReportSchedule");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsReportScheduleFieldOutput_hrmsReportSchedule_ReportScheduleId",
                schema: "dbo",
                table: "hrmsReportScheduleFieldOutput");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsReportScheduleFieldValue_hrmsReportSchedule_ReportScheduleId",
                schema: "dbo",
                table: "hrmsReportScheduleFieldValue");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsReportScheduleRecipient_hrmsReportSchedule_ReportScheduleId",
                schema: "dbo",
                table: "hrmsReportScheduleRecipient");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsRequisitionScreeningCriterion_hrmsJobRequisition_RequisitionId",
                schema: "dbo",
                table: "hrmsRequisitionScreeningCriterion");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsReviewCycle_FiscalYear_FiscalYearId",
                schema: "dbo",
                table: "hrmsReviewCycle");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsReviewCycle_hrmsRatingScale_RatingScaleId",
                schema: "dbo",
                table: "hrmsReviewCycle");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsRewardDisbursement_hrmsEmployeeRecognition_EmployeeRecognitionId",
                schema: "dbo",
                table: "hrmsRewardDisbursement");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsRewardDisbursement_hrmsEmployee_EmployeeId",
                schema: "dbo",
                table: "hrmsRewardDisbursement");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsRewardDisbursement_hrmsRecognitionBadge_RecognitionBadgeId",
                schema: "dbo",
                table: "hrmsRewardDisbursement");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsRewardNomination_hrmsEmployee_NomineeEmployeeId",
                schema: "dbo",
                table: "hrmsRewardNomination");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsRewardNomination_hrmsRecognitionBadge_RecognitionBadgeId",
                schema: "dbo",
                table: "hrmsRewardNomination");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsRewardNomination_hrmsRecognitionProgram_RecognitionProgramId",
                schema: "dbo",
                table: "hrmsRewardNomination");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsRewardPointsTransaction_hrmsEmployee_EmployeeId",
                schema: "dbo",
                table: "hrmsRewardPointsTransaction");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsSalaryRevisionBand_hrmsSalaryRevision_SalaryRevisionId",
                schema: "dbo",
                table: "hrmsSalaryRevisionBand");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsSalaryRevisionLine_hrmsEmployee_EmployeeId",
                schema: "dbo",
                table: "hrmsSalaryRevisionLine");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsSalaryRevisionLine_hrmsSalaryRevision_SalaryRevisionId",
                schema: "dbo",
                table: "hrmsSalaryRevisionLine");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsSettlementLine_hrmsTerminationSettlement_TerminationSettlementId",
                schema: "dbo",
                table: "hrmsSettlementLine");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsSuccessionCandidate_hrmsEmployee_EmployeeId",
                schema: "dbo",
                table: "hrmsSuccessionCandidate");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsSuccessionCandidate_hrmsSuccessionPlan_SuccessionPlanId",
                schema: "dbo",
                table: "hrmsSuccessionCandidate");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsSuccessionDevelopmentAction_hrmsEmployee_MentorEmployeeId",
                schema: "dbo",
                table: "hrmsSuccessionDevelopmentAction");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsSuccessionDevelopmentAction_hrmsSuccessionCandidate_SuccessionCandidateId",
                schema: "dbo",
                table: "hrmsSuccessionDevelopmentAction");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsSuccessionPlan_hrmsCriticalPosition_CriticalPositionId",
                schema: "dbo",
                table: "hrmsSuccessionPlan");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsSurveyCompletion_hrmsEmployee_EmployeeId",
                schema: "dbo",
                table: "hrmsSurveyCompletion");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsSurveyCompletion_hrmsSurvey_SurveyId",
                schema: "dbo",
                table: "hrmsSurveyCompletion");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsSurveyResponse_hrmsSurvey_SurveyId",
                schema: "dbo",
                table: "hrmsSurveyResponse");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsTalentAssessment_hrmsEmployee_EmployeeId",
                schema: "dbo",
                table: "hrmsTalentAssessment");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsTalentAssessment_hrmsTalentReview_TalentReviewId",
                schema: "dbo",
                table: "hrmsTalentAssessment");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsTalentRating_hrmsEmployee_RaterEmployeeId",
                schema: "dbo",
                table: "hrmsTalentRating");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsTalentRating_hrmsTalentAssessment_TalentAssessmentId",
                schema: "dbo",
                table: "hrmsTalentRating");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsTalentReview_hrmsOrganizationUnit_OrganizationUnitId",
                schema: "dbo",
                table: "hrmsTalentReview");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsTerminationAssetRecovery_hrmsCompanyAsset_CompanyAssetId",
                schema: "dbo",
                table: "hrmsTerminationAssetRecovery");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsTerminationAssetRecovery_hrmsEmployeeTermination_TerminationId",
                schema: "dbo",
                table: "hrmsTerminationAssetRecovery");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsTerminationClearance_hrmsClearanceDepartment_DepartmentId",
                schema: "dbo",
                table: "hrmsTerminationClearance");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsTerminationClearance_hrmsEmployeeTermination_TerminationId",
                schema: "dbo",
                table: "hrmsTerminationClearance");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsTerminationSettlement_hrmsEmployeeTermination_TerminationId",
                schema: "dbo",
                table: "hrmsTerminationSettlement");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsTrainingBudget_hrmsOrganizationUnit_OrganizationUnitId",
                schema: "dbo",
                table: "hrmsTrainingBudget");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsTrainingCourse_hrmsTrainingCategory_TrainingCategoryId",
                schema: "dbo",
                table: "hrmsTrainingCourse");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsTrainingEnrollment_hrmsEmployee_EmployeeId",
                schema: "dbo",
                table: "hrmsTrainingEnrollment");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsTrainingEnrollment_hrmsTrainingNeed_TrainingNeedId",
                schema: "dbo",
                table: "hrmsTrainingEnrollment");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsTrainingEnrollment_hrmsTrainingSession_TrainingSessionId",
                schema: "dbo",
                table: "hrmsTrainingEnrollment");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsTrainingNeed_hrmsCompetency_CompetencyId",
                schema: "dbo",
                table: "hrmsTrainingNeed");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsTrainingNeed_hrmsEmployee_EmployeeId",
                schema: "dbo",
                table: "hrmsTrainingNeed");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsTrainingNeed_hrmsTrainingCourse_TrainingCourseId",
                schema: "dbo",
                table: "hrmsTrainingNeed");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsTrainingProviderPayment_hrmsTrainingSession_TrainingSessionId",
                schema: "dbo",
                table: "hrmsTrainingProviderPayment");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsTrainingSession_hrmsTrainingCourse_TrainingCourseId",
                schema: "dbo",
                table: "hrmsTrainingSession");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsTripBudget_hrmsOrganizationUnit_OrganizationUnitId",
                schema: "dbo",
                table: "hrmsTripBudget");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsTripExpense_hrmsTripRequest_TripRequestId",
                schema: "dbo",
                table: "hrmsTripExpense");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsTripRequest_hrmsEmployee_EmployeeId",
                schema: "dbo",
                table: "hrmsTripRequest");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsTripRequest_hrmsTripBudget_TripBudgetId",
                schema: "dbo",
                table: "hrmsTripRequest");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsWorkflowActionLog_hrmsWorkflowInstance_InstanceId",
                schema: "dbo",
                table: "hrmsWorkflowActionLog");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsWorkflowInstance_hrmsWorkflowDefinition_DefinitionId",
                schema: "dbo",
                table: "hrmsWorkflowInstance");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsWorkflowStep_hrmsWorkflowDefinition_DefinitionId",
                schema: "dbo",
                table: "hrmsWorkflowStep");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsWorkflowStepApprover_hrmsWorkflowStep_StepId",
                schema: "dbo",
                table: "hrmsWorkflowStepApprover");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsWorkforcePlan_FiscalYear_StartFiscalYearId",
                schema: "dbo",
                table: "hrmsWorkforcePlan");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsWorkforcePlan_hrmsOrganizationUnit_OrganizationUnitId",
                schema: "dbo",
                table: "hrmsWorkforcePlan");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsWorkforcePlanLine_hrmsOrganizationUnit_OrganizationUnitId",
                schema: "dbo",
                table: "hrmsWorkforcePlanLine");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsWorkforcePlanLine_hrmsPositionClass_PositionClassId",
                schema: "dbo",
                table: "hrmsWorkforcePlanLine");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsWorkforcePlanLine_hrmsWorkforcePlan_PlanId",
                schema: "dbo",
                table: "hrmsWorkforcePlanLine");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsWorkLocation_hrmsWorkLocation_ParentId",
                schema: "dbo",
                table: "hrmsWorkLocation");

            migrationBuilder.DropForeignKey(
                name: "FK_RolePermission_coreOperation_OperationId",
                schema: "Core",
                table: "RolePermission");

            migrationBuilder.DropForeignKey(
                name: "FK_User_hrmsEmployee_EmployeeId",
                schema: "Core",
                table: "User");

            migrationBuilder.DropPrimaryKey(
                name: "PK_lupStep",
                schema: "Core",
                table: "lupStep");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsWorkWeekConfiguration",
                schema: "dbo",
                table: "hrmsWorkWeekConfiguration");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsWorkLocation",
                schema: "dbo",
                table: "hrmsWorkLocation");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsWorkforcePlanLine",
                schema: "dbo",
                table: "hrmsWorkforcePlanLine");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsWorkforcePlan",
                schema: "dbo",
                table: "hrmsWorkforcePlan");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsWorkflowStepApprover",
                schema: "dbo",
                table: "hrmsWorkflowStepApprover");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsWorkflowStep",
                schema: "dbo",
                table: "hrmsWorkflowStep");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsWorkflowInstance",
                schema: "dbo",
                table: "hrmsWorkflowInstance");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsWorkflowDefinition",
                schema: "dbo",
                table: "hrmsWorkflowDefinition");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsWorkflowActionLog",
                schema: "dbo",
                table: "hrmsWorkflowActionLog");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsTripRequest",
                schema: "dbo",
                table: "hrmsTripRequest");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsTripExpense",
                schema: "dbo",
                table: "hrmsTripExpense");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsTripBudget",
                schema: "dbo",
                table: "hrmsTripBudget");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsTrainingSession",
                schema: "dbo",
                table: "hrmsTrainingSession");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsTrainingProviderPayment",
                schema: "dbo",
                table: "hrmsTrainingProviderPayment");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsTrainingNeed",
                schema: "dbo",
                table: "hrmsTrainingNeed");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsTrainingEnrollment",
                schema: "dbo",
                table: "hrmsTrainingEnrollment");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsTrainingCourse",
                schema: "dbo",
                table: "hrmsTrainingCourse");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsTrainingCategory",
                schema: "dbo",
                table: "hrmsTrainingCategory");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsTrainingBudget",
                schema: "dbo",
                table: "hrmsTrainingBudget");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsTerminationSettlement",
                schema: "dbo",
                table: "hrmsTerminationSettlement");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsTerminationClearance",
                schema: "dbo",
                table: "hrmsTerminationClearance");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsTerminationAssetRecovery",
                schema: "dbo",
                table: "hrmsTerminationAssetRecovery");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsTaxBracket",
                schema: "dbo",
                table: "hrmsTaxBracket");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsTalentReview",
                schema: "dbo",
                table: "hrmsTalentReview");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsTalentRating",
                schema: "dbo",
                table: "hrmsTalentRating");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsTalentAssessment",
                schema: "dbo",
                table: "hrmsTalentAssessment");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsSurveyResponse",
                schema: "dbo",
                table: "hrmsSurveyResponse");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsSurveyCompletion",
                schema: "dbo",
                table: "hrmsSurveyCompletion");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsSurvey",
                schema: "dbo",
                table: "hrmsSurvey");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsSuggestion",
                schema: "dbo",
                table: "hrmsSuggestion");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsSuccessionPlan",
                schema: "dbo",
                table: "hrmsSuccessionPlan");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsSuccessionDevelopmentAction",
                schema: "dbo",
                table: "hrmsSuccessionDevelopmentAction");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsSuccessionCandidate",
                schema: "dbo",
                table: "hrmsSuccessionCandidate");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsSettlementLine",
                schema: "dbo",
                table: "hrmsSettlementLine");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsSalaryRevisionLine",
                schema: "dbo",
                table: "hrmsSalaryRevisionLine");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsSalaryRevisionBand",
                schema: "dbo",
                table: "hrmsSalaryRevisionBand");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsSalaryRevision",
                schema: "dbo",
                table: "hrmsSalaryRevision");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsRewardPointsTransaction",
                schema: "dbo",
                table: "hrmsRewardPointsTransaction");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsRewardNomination",
                schema: "dbo",
                table: "hrmsRewardNomination");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsRewardDisbursement",
                schema: "dbo",
                table: "hrmsRewardDisbursement");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsReviewCycle",
                schema: "dbo",
                table: "hrmsReviewCycle");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsRequisitionScreeningCriterion",
                schema: "dbo",
                table: "hrmsRequisitionScreeningCriterion");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsReportScheduleRecipient",
                schema: "dbo",
                table: "hrmsReportScheduleRecipient");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsReportScheduleFieldValue",
                schema: "dbo",
                table: "hrmsReportScheduleFieldValue");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsReportScheduleFieldOutput",
                schema: "dbo",
                table: "hrmsReportScheduleFieldOutput");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsReportSchedule",
                schema: "dbo",
                table: "hrmsReportSchedule");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsReportSavedFilter",
                schema: "dbo",
                table: "hrmsReportSavedFilter");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsReportRunRecipient",
                schema: "dbo",
                table: "hrmsReportRunRecipient");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsReportRun",
                schema: "dbo",
                table: "hrmsReportRun");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsReportRestriction",
                schema: "dbo",
                table: "hrmsReportRestriction");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsReportFieldOutput",
                schema: "dbo",
                table: "hrmsReportFieldOutput");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsReportField",
                schema: "dbo",
                table: "hrmsReportField");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsReport",
                schema: "dbo",
                table: "hrmsReport");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsRecognitionProgram",
                schema: "dbo",
                table: "hrmsRecognitionProgram");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsRecognitionBadge",
                schema: "dbo",
                table: "hrmsRecognitionBadge");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsRatingScaleLevel",
                schema: "dbo",
                table: "hrmsRatingScaleLevel");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsRatingScale",
                schema: "dbo",
                table: "hrmsRatingScale");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsProfileChangeRequest",
                schema: "dbo",
                table: "hrmsProfileChangeRequest");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsPositionCompetency",
                schema: "dbo",
                table: "hrmsPositionCompetency");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsPositionClass",
                schema: "dbo",
                table: "hrmsPositionClass");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsPosition",
                schema: "dbo",
                table: "hrmsPosition");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsPipObjective",
                schema: "dbo",
                table: "hrmsPipObjective");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsPerformanceHistory",
                schema: "dbo",
                table: "hrmsPerformanceHistory");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsPerDiemRate",
                schema: "dbo",
                table: "hrmsPerDiemRate");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsOtherLeaveSetting",
                schema: "dbo",
                table: "hrmsOtherLeaveSetting");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsOtherLeaveDetail",
                schema: "dbo",
                table: "hrmsOtherLeaveDetail");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsOtherLeave",
                schema: "dbo",
                table: "hrmsOtherLeave");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsOrganizationUnit",
                schema: "dbo",
                table: "hrmsOrganizationUnit");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsOrganizationalObjective",
                schema: "dbo",
                table: "hrmsOrganizationalObjective");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsOfferLetterTemplate",
                schema: "dbo",
                table: "hrmsOfferLetterTemplate");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsNumberSequence",
                schema: "dbo",
                table: "hrmsNumberSequence");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsMentorship",
                schema: "dbo",
                table: "hrmsMentorship");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsMedicalServiceContract",
                schema: "dbo",
                table: "hrmsMedicalServiceContract");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsMedicalProvider",
                schema: "dbo",
                table: "hrmsMedicalProvider");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsMedicalPlan",
                schema: "dbo",
                table: "hrmsMedicalPlan");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsMedicalEnrollment",
                schema: "dbo",
                table: "hrmsMedicalEnrollment");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsMedicalClaimAttachment",
                schema: "dbo",
                table: "hrmsMedicalClaimAttachment");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsMedicalClaim",
                schema: "dbo",
                table: "hrmsMedicalClaim");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsMedicalBeneficiary",
                schema: "dbo",
                table: "hrmsMedicalBeneficiary");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsLoanType",
                schema: "dbo",
                table: "hrmsLoanType");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsLoanRepaymentSchedule",
                schema: "dbo",
                table: "hrmsLoanRepaymentSchedule");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsLoanGuarantor",
                schema: "dbo",
                table: "hrmsLoanGuarantor");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsLoan",
                schema: "dbo",
                table: "hrmsLoan");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsLeaveType",
                schema: "dbo",
                table: "hrmsLeaveType");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsLeaveRequestLine",
                schema: "dbo",
                table: "hrmsLeaveRequestLine");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsLeaveRequest",
                schema: "dbo",
                table: "hrmsLeaveRequest");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsLeaveBalanceTransaction",
                schema: "dbo",
                table: "hrmsLeaveBalanceTransaction");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsLeaveBalance",
                schema: "dbo",
                table: "hrmsLeaveBalance");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsLearningPathStep",
                schema: "dbo",
                table: "hrmsLearningPathStep");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsLearningPath",
                schema: "dbo",
                table: "hrmsLearningPath");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsLearningCommunityPost",
                schema: "dbo",
                table: "hrmsLearningCommunityPost");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsLearningCommunityMember",
                schema: "dbo",
                table: "hrmsLearningCommunityMember");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsLearningCommunity",
                schema: "dbo",
                table: "hrmsLearningCommunity");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsKnowledgeTransfer",
                schema: "dbo",
                table: "hrmsKnowledgeTransfer");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsJobRequisition",
                schema: "dbo",
                table: "hrmsJobRequisition");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsJobOffer",
                schema: "dbo",
                table: "hrmsJobOffer");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsJobGrade",
                schema: "dbo",
                table: "hrmsJobGrade");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsJobCategory",
                schema: "dbo",
                table: "hrmsJobCategory");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsJobApplicationStageLog",
                schema: "dbo",
                table: "hrmsJobApplicationStageLog");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsJobApplication",
                schema: "dbo",
                table: "hrmsJobApplication");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsInterviewPanelist",
                schema: "dbo",
                table: "hrmsInterviewPanelist");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsInterviewFeedback",
                schema: "dbo",
                table: "hrmsInterviewFeedback");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsInterview",
                schema: "dbo",
                table: "hrmsInterview");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsInsurancePremiumSchedule",
                schema: "dbo",
                table: "hrmsInsurancePremiumSchedule");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsInsurancePolicy",
                schema: "dbo",
                table: "hrmsInsurancePolicy");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsInsuranceClaimAttachment",
                schema: "dbo",
                table: "hrmsInsuranceClaimAttachment");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsInsuranceClaim",
                schema: "dbo",
                table: "hrmsInsuranceClaim");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsImprovementPlan",
                schema: "dbo",
                table: "hrmsImprovementPlan");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsHoliday",
                schema: "dbo",
                table: "hrmsHoliday");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsHiringRequest",
                schema: "dbo",
                table: "hrmsHiringRequest");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsGrievanceNote",
                schema: "dbo",
                table: "hrmsGrievanceNote");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsGrievance",
                schema: "dbo",
                table: "hrmsGrievance");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsGoalActionItem",
                schema: "dbo",
                table: "hrmsGoalActionItem");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsExitQuestionnaire",
                schema: "dbo",
                table: "hrmsExitQuestionnaire");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsExitInterview",
                schema: "dbo",
                table: "hrmsExitInterview");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsEmployeeTrainingCertificate",
                schema: "dbo",
                table: "hrmsEmployeeTrainingCertificate");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsEmployeeTermination",
                schema: "dbo",
                table: "hrmsEmployeeTermination");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsEmployeeRecognition",
                schema: "dbo",
                table: "hrmsEmployeeRecognition");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsEmployeeMovement",
                schema: "dbo",
                table: "hrmsEmployeeMovement");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsEmployeeGuarantee",
                schema: "dbo",
                table: "hrmsEmployeeGuarantee");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsEmployeeGoal",
                schema: "dbo",
                table: "hrmsEmployeeGoal");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsEmployeeFieldValue",
                schema: "dbo",
                table: "hrmsEmployeeFieldValue");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsEmployeeFieldDefinition",
                schema: "dbo",
                table: "hrmsEmployeeFieldDefinition");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsEmployeeExperience",
                schema: "dbo",
                table: "hrmsEmployeeExperience");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsEmployeeEducation",
                schema: "dbo",
                table: "hrmsEmployeeEducation");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsEmployeeDocument",
                schema: "dbo",
                table: "hrmsEmployeeDocument");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsEmployeeDependent",
                schema: "dbo",
                table: "hrmsEmployeeDependent");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsEmployeeCareerPathStepProgress",
                schema: "dbo",
                table: "hrmsEmployeeCareerPathStepProgress");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsEmployeeCareerPath",
                schema: "dbo",
                table: "hrmsEmployeeCareerPath");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsEmployeeBenefitEnrollment",
                schema: "dbo",
                table: "hrmsEmployeeBenefitEnrollment");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsEmployeeAllowance",
                schema: "dbo",
                table: "hrmsEmployeeAllowance");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsEmployee",
                schema: "dbo",
                table: "hrmsEmployee");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsDynamicFormRecord",
                schema: "dbo",
                table: "hrmsDynamicFormRecord");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsDynamicFormField",
                schema: "dbo",
                table: "hrmsDynamicFormField");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsDynamicForm",
                schema: "dbo",
                table: "hrmsDynamicForm");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsDocumentTemplate",
                schema: "dbo",
                table: "hrmsDocumentTemplate");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsDisciplinaryMeasure",
                schema: "dbo",
                table: "hrmsDisciplinaryMeasure");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsDevelopmentPlan",
                schema: "dbo",
                table: "hrmsDevelopmentPlan");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsDevelopmentAction",
                schema: "dbo",
                table: "hrmsDevelopmentAction");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsCriticalPosition",
                schema: "dbo",
                table: "hrmsCriticalPosition");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsCriterionEvaluator",
                schema: "dbo",
                table: "hrmsCriterionEvaluator");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsCompetencyCategory",
                schema: "dbo",
                table: "hrmsCompetencyCategory");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsCompetency",
                schema: "dbo",
                table: "hrmsCompetency");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsCompensationRequest",
                schema: "dbo",
                table: "hrmsCompensationRequest");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsCompanyProfile",
                schema: "dbo",
                table: "hrmsCompanyProfile");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsCompanyAsset",
                schema: "dbo",
                table: "hrmsCompanyAsset");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsCommunityPostReaction",
                schema: "dbo",
                table: "hrmsCommunityPostReaction");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsClearanceDepartmentApprover",
                schema: "dbo",
                table: "hrmsClearanceDepartmentApprover");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsClearanceDepartment",
                schema: "dbo",
                table: "hrmsClearanceDepartment");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsCareerPathStepCompetency",
                schema: "dbo",
                table: "hrmsCareerPathStepCompetency");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsCareerPathStep",
                schema: "dbo",
                table: "hrmsCareerPathStep");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsCareerPathChangeRequest",
                schema: "dbo",
                table: "hrmsCareerPathChangeRequest");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsCareerPath",
                schema: "dbo",
                table: "hrmsCareerPath");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsCandidateDocument",
                schema: "dbo",
                table: "hrmsCandidateDocument");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsCandidate",
                schema: "dbo",
                table: "hrmsCandidate");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsCalibrationSession",
                schema: "dbo",
                table: "hrmsCalibrationSession");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsCalibrationItem",
                schema: "dbo",
                table: "hrmsCalibrationItem");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsBranch",
                schema: "dbo",
                table: "hrmsBranch");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsBenefitPlan",
                schema: "dbo",
                table: "hrmsBenefitPlan");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsAwardCategory",
                schema: "dbo",
                table: "hrmsAwardCategory");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsAuditLog",
                schema: "dbo",
                table: "hrmsAuditLog");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsAppraisalTemplate",
                schema: "dbo",
                table: "hrmsAppraisalTemplate");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsAppraisalPeerReview",
                schema: "dbo",
                table: "hrmsAppraisalPeerReview");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsAppraisalGoal",
                schema: "dbo",
                table: "hrmsAppraisalGoal");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsAppraisalCompetency",
                schema: "dbo",
                table: "hrmsAppraisalCompetency");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsAppraisalAppeal",
                schema: "dbo",
                table: "hrmsAppraisalAppeal");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsAppraisal",
                schema: "dbo",
                table: "hrmsAppraisal");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsApplicationCriterionScore",
                schema: "dbo",
                table: "hrmsApplicationCriterionScore");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsAnnualLeaveSetting",
                schema: "dbo",
                table: "hrmsAnnualLeaveSetting");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsAnnualLeaveHeader",
                schema: "dbo",
                table: "hrmsAnnualLeaveHeader");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsAnnualLeaveDetail",
                schema: "dbo",
                table: "hrmsAnnualLeaveDetail");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsAnnouncement",
                schema: "dbo",
                table: "hrmsAnnouncement");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsAllowanceType",
                schema: "dbo",
                table: "hrmsAllowanceType");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsAchievement",
                schema: "dbo",
                table: "hrmsAchievement");

            migrationBuilder.DropPrimaryKey(
                name: "PK_coreSubsystem",
                schema: "dbo",
                table: "coreSubsystem");

            migrationBuilder.DropPrimaryKey(
                name: "PK_coreSalaryScale",
                schema: "Core",
                table: "coreSalaryScale");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CorePerson",
                schema: "Core",
                table: "CorePerson");

            migrationBuilder.DropPrimaryKey(
                name: "PK_coreOperation",
                schema: "dbo",
                table: "coreOperation");

            migrationBuilder.DropPrimaryKey(
                name: "PK_coreModule",
                schema: "dbo",
                table: "coreModule");

            migrationBuilder.EnsureSchema(
                name: "Hrms");

            migrationBuilder.RenameTable(
                name: "lupStep",
                schema: "Core",
                newName: "Step",
                newSchema: "Core");

            migrationBuilder.RenameTable(
                name: "hrmsWorkWeekConfiguration",
                schema: "dbo",
                newName: "WorkWeekConfiguration",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsWorkLocation",
                schema: "dbo",
                newName: "WorkLocation",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsWorkforcePlanLine",
                schema: "dbo",
                newName: "WorkforcePlanLine",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsWorkforcePlan",
                schema: "dbo",
                newName: "WorkforcePlan",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsWorkflowStepApprover",
                schema: "dbo",
                newName: "WorkflowStepApprover",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsWorkflowStep",
                schema: "dbo",
                newName: "WorkflowStep",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsWorkflowInstance",
                schema: "dbo",
                newName: "WorkflowInstance",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsWorkflowDefinition",
                schema: "dbo",
                newName: "WorkflowDefinition",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsWorkflowActionLog",
                schema: "dbo",
                newName: "WorkflowActionLog",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsTripRequest",
                schema: "dbo",
                newName: "TripRequest",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsTripExpense",
                schema: "dbo",
                newName: "TripExpense",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsTripBudget",
                schema: "dbo",
                newName: "TripBudget",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsTrainingSession",
                schema: "dbo",
                newName: "TrainingSession",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsTrainingProviderPayment",
                schema: "dbo",
                newName: "TrainingProviderPayment",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsTrainingNeed",
                schema: "dbo",
                newName: "TrainingNeed",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsTrainingEnrollment",
                schema: "dbo",
                newName: "TrainingEnrollment",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsTrainingCourse",
                schema: "dbo",
                newName: "TrainingCourse",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsTrainingCategory",
                schema: "dbo",
                newName: "TrainingCategory",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsTrainingBudget",
                schema: "dbo",
                newName: "TrainingBudget",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsTerminationSettlement",
                schema: "dbo",
                newName: "TerminationSettlement",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsTerminationClearance",
                schema: "dbo",
                newName: "TerminationClearance",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsTerminationAssetRecovery",
                schema: "dbo",
                newName: "TerminationAssetRecovery",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsTaxBracket",
                schema: "dbo",
                newName: "TaxBracket",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsTalentReview",
                schema: "dbo",
                newName: "TalentReview",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsTalentRating",
                schema: "dbo",
                newName: "TalentRating",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsTalentAssessment",
                schema: "dbo",
                newName: "TalentAssessment",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsSurveyResponse",
                schema: "dbo",
                newName: "SurveyResponse",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsSurveyCompletion",
                schema: "dbo",
                newName: "SurveyCompletion",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsSurvey",
                schema: "dbo",
                newName: "Survey",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsSuggestion",
                schema: "dbo",
                newName: "Suggestion",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsSuccessionPlan",
                schema: "dbo",
                newName: "SuccessionPlan",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsSuccessionDevelopmentAction",
                schema: "dbo",
                newName: "SuccessionDevelopmentAction",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsSuccessionCandidate",
                schema: "dbo",
                newName: "SuccessionCandidate",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsSettlementLine",
                schema: "dbo",
                newName: "SettlementLine",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsSalaryRevisionLine",
                schema: "dbo",
                newName: "SalaryRevisionLine",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsSalaryRevisionBand",
                schema: "dbo",
                newName: "SalaryRevisionBand",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsSalaryRevision",
                schema: "dbo",
                newName: "SalaryRevision",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsRewardPointsTransaction",
                schema: "dbo",
                newName: "RewardPointsTransaction",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsRewardNomination",
                schema: "dbo",
                newName: "RewardNomination",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsRewardDisbursement",
                schema: "dbo",
                newName: "RewardDisbursement",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsReviewCycle",
                schema: "dbo",
                newName: "ReviewCycle",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsRequisitionScreeningCriterion",
                schema: "dbo",
                newName: "RequisitionScreeningCriterion",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsReportScheduleRecipient",
                schema: "dbo",
                newName: "ReportScheduleRecipient",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsReportScheduleFieldValue",
                schema: "dbo",
                newName: "ReportScheduleFieldValue",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsReportScheduleFieldOutput",
                schema: "dbo",
                newName: "ReportScheduleFieldOutput",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsReportSchedule",
                schema: "dbo",
                newName: "ReportSchedule",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsReportSavedFilter",
                schema: "dbo",
                newName: "ReportSavedFilter",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsReportRunRecipient",
                schema: "dbo",
                newName: "ReportRunRecipient",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsReportRun",
                schema: "dbo",
                newName: "ReportRun",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsReportRestriction",
                schema: "dbo",
                newName: "ReportRestriction",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsReportFieldOutput",
                schema: "dbo",
                newName: "ReportFieldOutput",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsReportField",
                schema: "dbo",
                newName: "ReportField",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsReport",
                schema: "dbo",
                newName: "Report",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsRecognitionProgram",
                schema: "dbo",
                newName: "RecognitionProgram",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsRecognitionBadge",
                schema: "dbo",
                newName: "RecognitionBadge",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsRatingScaleLevel",
                schema: "dbo",
                newName: "RatingScaleLevel",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsRatingScale",
                schema: "dbo",
                newName: "RatingScale",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsProfileChangeRequest",
                schema: "dbo",
                newName: "ProfileChangeRequest",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsPositionCompetency",
                schema: "dbo",
                newName: "PositionCompetency",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsPositionClass",
                schema: "dbo",
                newName: "PositionClass",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsPosition",
                schema: "dbo",
                newName: "Position",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsPipObjective",
                schema: "dbo",
                newName: "PipObjective",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsPerformanceHistory",
                schema: "dbo",
                newName: "PerformanceHistory",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsPerDiemRate",
                schema: "dbo",
                newName: "PerDiemRate",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsOtherLeaveSetting",
                schema: "dbo",
                newName: "OtherLeaveSetting",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsOtherLeaveDetail",
                schema: "dbo",
                newName: "OtherLeaveDetail",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsOtherLeave",
                schema: "dbo",
                newName: "OtherLeave",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsOrganizationUnit",
                schema: "dbo",
                newName: "OrganizationUnit",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsOrganizationalObjective",
                schema: "dbo",
                newName: "OrganizationalObjective",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsOfferLetterTemplate",
                schema: "dbo",
                newName: "OfferLetterTemplate",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsNumberSequence",
                schema: "dbo",
                newName: "NumberSequence",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsMentorship",
                schema: "dbo",
                newName: "Mentorship",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsMedicalServiceContract",
                schema: "dbo",
                newName: "MedicalServiceContract",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsMedicalProvider",
                schema: "dbo",
                newName: "MedicalProvider",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsMedicalPlan",
                schema: "dbo",
                newName: "MedicalPlan",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsMedicalEnrollment",
                schema: "dbo",
                newName: "MedicalEnrollment",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsMedicalClaimAttachment",
                schema: "dbo",
                newName: "MedicalClaimAttachment",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsMedicalClaim",
                schema: "dbo",
                newName: "MedicalClaim",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsMedicalBeneficiary",
                schema: "dbo",
                newName: "MedicalBeneficiary",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsLoanType",
                schema: "dbo",
                newName: "LoanType",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsLoanRepaymentSchedule",
                schema: "dbo",
                newName: "LoanRepaymentSchedule",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsLoanGuarantor",
                schema: "dbo",
                newName: "LoanGuarantor",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsLoan",
                schema: "dbo",
                newName: "Loan",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsLeaveType",
                schema: "dbo",
                newName: "LeaveType",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsLeaveRequestLine",
                schema: "dbo",
                newName: "LeaveRequestLine",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsLeaveRequest",
                schema: "dbo",
                newName: "LeaveRequest",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsLeaveBalanceTransaction",
                schema: "dbo",
                newName: "LeaveBalanceTransaction",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsLeaveBalance",
                schema: "dbo",
                newName: "LeaveBalance",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsLearningPathStep",
                schema: "dbo",
                newName: "LearningPathStep",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsLearningPath",
                schema: "dbo",
                newName: "LearningPath",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsLearningCommunityPost",
                schema: "dbo",
                newName: "LearningCommunityPost",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsLearningCommunityMember",
                schema: "dbo",
                newName: "LearningCommunityMember",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsLearningCommunity",
                schema: "dbo",
                newName: "LearningCommunity",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsKnowledgeTransfer",
                schema: "dbo",
                newName: "KnowledgeTransfer",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsJobRequisition",
                schema: "dbo",
                newName: "JobRequisition",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsJobOffer",
                schema: "dbo",
                newName: "JobOffer",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsJobGrade",
                schema: "dbo",
                newName: "JobGrade",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsJobCategory",
                schema: "dbo",
                newName: "JobCategory",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsJobApplicationStageLog",
                schema: "dbo",
                newName: "JobApplicationStageLog",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsJobApplication",
                schema: "dbo",
                newName: "JobApplication",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsInterviewPanelist",
                schema: "dbo",
                newName: "InterviewPanelist",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsInterviewFeedback",
                schema: "dbo",
                newName: "InterviewFeedback",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsInterview",
                schema: "dbo",
                newName: "Interview",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsInsurancePremiumSchedule",
                schema: "dbo",
                newName: "InsurancePremiumSchedule",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsInsurancePolicy",
                schema: "dbo",
                newName: "InsurancePolicy",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsInsuranceClaimAttachment",
                schema: "dbo",
                newName: "InsuranceClaimAttachment",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsInsuranceClaim",
                schema: "dbo",
                newName: "InsuranceClaim",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsImprovementPlan",
                schema: "dbo",
                newName: "ImprovementPlan",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsHoliday",
                schema: "dbo",
                newName: "Holiday",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsHiringRequest",
                schema: "dbo",
                newName: "HiringRequest",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsGrievanceNote",
                schema: "dbo",
                newName: "GrievanceNote",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsGrievance",
                schema: "dbo",
                newName: "Grievance",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsGoalActionItem",
                schema: "dbo",
                newName: "GoalActionItem",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsExitQuestionnaire",
                schema: "dbo",
                newName: "ExitQuestionnaire",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsExitInterview",
                schema: "dbo",
                newName: "ExitInterview",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsEmployeeTrainingCertificate",
                schema: "dbo",
                newName: "EmployeeTrainingCertificate",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsEmployeeTermination",
                schema: "dbo",
                newName: "EmployeeTermination",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsEmployeeRecognition",
                schema: "dbo",
                newName: "EmployeeRecognition",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsEmployeeMovement",
                schema: "dbo",
                newName: "EmployeeMovement",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsEmployeeGuarantee",
                schema: "dbo",
                newName: "EmployeeGuarantee",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsEmployeeGoal",
                schema: "dbo",
                newName: "EmployeeGoal",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsEmployeeFieldValue",
                schema: "dbo",
                newName: "EmployeeFieldValue",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsEmployeeFieldDefinition",
                schema: "dbo",
                newName: "EmployeeFieldDefinition",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsEmployeeExperience",
                schema: "dbo",
                newName: "EmployeeExperience",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsEmployeeEducation",
                schema: "dbo",
                newName: "EmployeeEducation",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsEmployeeDocument",
                schema: "dbo",
                newName: "EmployeeDocument",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsEmployeeDependent",
                schema: "dbo",
                newName: "EmployeeDependent",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsEmployeeCareerPathStepProgress",
                schema: "dbo",
                newName: "EmployeeCareerPathStepProgress",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsEmployeeCareerPath",
                schema: "dbo",
                newName: "EmployeeCareerPath",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsEmployeeBenefitEnrollment",
                schema: "dbo",
                newName: "EmployeeBenefitEnrollment",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsEmployeeAllowance",
                schema: "dbo",
                newName: "EmployeeAllowance",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsEmployee",
                schema: "dbo",
                newName: "Employee",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsDynamicFormRecord",
                schema: "dbo",
                newName: "DynamicFormRecord",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsDynamicFormField",
                schema: "dbo",
                newName: "DynamicFormField",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsDynamicForm",
                schema: "dbo",
                newName: "DynamicForm",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsDocumentTemplate",
                schema: "dbo",
                newName: "DocumentTemplate",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsDisciplinaryMeasure",
                schema: "dbo",
                newName: "DisciplinaryMeasure",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsDevelopmentPlan",
                schema: "dbo",
                newName: "DevelopmentPlan",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsDevelopmentAction",
                schema: "dbo",
                newName: "DevelopmentAction",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsCriticalPosition",
                schema: "dbo",
                newName: "CriticalPosition",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsCriterionEvaluator",
                schema: "dbo",
                newName: "CriterionEvaluator",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsCompetencyCategory",
                schema: "dbo",
                newName: "CompetencyCategory",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsCompetency",
                schema: "dbo",
                newName: "Competency",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsCompensationRequest",
                schema: "dbo",
                newName: "CompensationRequest",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsCompanyProfile",
                schema: "dbo",
                newName: "CompanyProfile",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsCompanyAsset",
                schema: "dbo",
                newName: "CompanyAsset",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsCommunityPostReaction",
                schema: "dbo",
                newName: "CommunityPostReaction",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsClearanceDepartmentApprover",
                schema: "dbo",
                newName: "ClearanceDepartmentApprover",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsClearanceDepartment",
                schema: "dbo",
                newName: "ClearanceDepartment",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsCareerPathStepCompetency",
                schema: "dbo",
                newName: "CareerPathStepCompetency",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsCareerPathStep",
                schema: "dbo",
                newName: "CareerPathStep",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsCareerPathChangeRequest",
                schema: "dbo",
                newName: "CareerPathChangeRequest",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsCareerPath",
                schema: "dbo",
                newName: "CareerPath",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsCandidateDocument",
                schema: "dbo",
                newName: "CandidateDocument",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsCandidate",
                schema: "dbo",
                newName: "Candidate",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsCalibrationSession",
                schema: "dbo",
                newName: "CalibrationSession",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsCalibrationItem",
                schema: "dbo",
                newName: "CalibrationItem",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsBranch",
                schema: "dbo",
                newName: "Branch",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsBenefitPlan",
                schema: "dbo",
                newName: "BenefitPlan",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsAwardCategory",
                schema: "dbo",
                newName: "AwardCategory",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsAuditLog",
                schema: "dbo",
                newName: "AuditLog",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsAppraisalTemplate",
                schema: "dbo",
                newName: "AppraisalTemplate",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsAppraisalPeerReview",
                schema: "dbo",
                newName: "AppraisalPeerReview",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsAppraisalGoal",
                schema: "dbo",
                newName: "AppraisalGoal",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsAppraisalCompetency",
                schema: "dbo",
                newName: "AppraisalCompetency",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsAppraisalAppeal",
                schema: "dbo",
                newName: "AppraisalAppeal",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsAppraisal",
                schema: "dbo",
                newName: "Appraisal",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsApplicationCriterionScore",
                schema: "dbo",
                newName: "ApplicationCriterionScore",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsAnnualLeaveSetting",
                schema: "dbo",
                newName: "AnnualLeaveSetting",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsAnnualLeaveHeader",
                schema: "dbo",
                newName: "AnnualLeaveHeader",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsAnnualLeaveDetail",
                schema: "dbo",
                newName: "AnnualLeaveDetail",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsAnnouncement",
                schema: "dbo",
                newName: "Announcement",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsAllowanceType",
                schema: "dbo",
                newName: "AllowanceType",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "hrmsAchievement",
                schema: "dbo",
                newName: "Achievement",
                newSchema: "Hrms");

            migrationBuilder.RenameTable(
                name: "coreSubsystem",
                schema: "dbo",
                newName: "Subsystem",
                newSchema: "Core");

            migrationBuilder.RenameTable(
                name: "coreSalaryScale",
                schema: "Core",
                newName: "SalaryScale",
                newSchema: "Core");

            migrationBuilder.RenameTable(
                name: "CorePerson",
                schema: "Core",
                newName: "Person",
                newSchema: "Core");

            migrationBuilder.RenameTable(
                name: "coreOperation",
                schema: "dbo",
                newName: "Operation",
                newSchema: "Core");

            migrationBuilder.RenameTable(
                name: "coreModule",
                schema: "dbo",
                newName: "Module",
                newSchema: "Core");

            migrationBuilder.RenameIndex(
                name: "IX_lupStep_TenantId_Code",
                schema: "Core",
                table: "Step",
                newName: "IX_Step_TenantId_Code");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsWorkWeekConfiguration_TenantId_IsActive",
                schema: "Hrms",
                table: "WorkWeekConfiguration",
                newName: "IX_WorkWeekConfiguration_TenantId_IsActive");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsWorkLocation_TenantId_Code",
                schema: "Hrms",
                table: "WorkLocation",
                newName: "IX_WorkLocation_TenantId_Code");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsWorkLocation_ParentId",
                schema: "Hrms",
                table: "WorkLocation",
                newName: "IX_WorkLocation_ParentId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsWorkforcePlanLine_PositionClassId",
                schema: "Hrms",
                table: "WorkforcePlanLine",
                newName: "IX_WorkforcePlanLine_PositionClassId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsWorkforcePlanLine_PlanId_OrganizationUnitId_PositionClassId_EmploymentType_PeriodIndex",
                schema: "Hrms",
                table: "WorkforcePlanLine",
                newName: "IX_WorkforcePlanLine_PlanId_OrganizationUnitId_PositionClassId_EmploymentType_PeriodIndex");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsWorkforcePlanLine_PlanId",
                schema: "Hrms",
                table: "WorkforcePlanLine",
                newName: "IX_WorkforcePlanLine_PlanId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsWorkforcePlanLine_OrganizationUnitId",
                schema: "Hrms",
                table: "WorkforcePlanLine",
                newName: "IX_WorkforcePlanLine_OrganizationUnitId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsWorkforcePlan_TenantId_Status",
                schema: "Hrms",
                table: "WorkforcePlan",
                newName: "IX_WorkforcePlan_TenantId_Status");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsWorkforcePlan_StartFiscalYearId",
                schema: "Hrms",
                table: "WorkforcePlan",
                newName: "IX_WorkforcePlan_StartFiscalYearId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsWorkforcePlan_RootPlanId",
                schema: "Hrms",
                table: "WorkforcePlan",
                newName: "IX_WorkforcePlan_RootPlanId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsWorkforcePlan_OrganizationUnitId",
                schema: "Hrms",
                table: "WorkforcePlan",
                newName: "IX_WorkforcePlan_OrganizationUnitId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsWorkflowStepApprover_StepId",
                schema: "Hrms",
                table: "WorkflowStepApprover",
                newName: "IX_WorkflowStepApprover_StepId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsWorkflowStepApprover_ApproverType_ApproverId",
                schema: "Hrms",
                table: "WorkflowStepApprover",
                newName: "IX_WorkflowStepApprover_ApproverType_ApproverId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsWorkflowStep_DefinitionId_StepOrder",
                schema: "Hrms",
                table: "WorkflowStep",
                newName: "IX_WorkflowStep_DefinitionId_StepOrder");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsWorkflowInstance_TenantId_Status",
                schema: "Hrms",
                table: "WorkflowInstance",
                newName: "IX_WorkflowInstance_TenantId_Status");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsWorkflowInstance_Status",
                schema: "Hrms",
                table: "WorkflowInstance",
                newName: "IX_WorkflowInstance_Status");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsWorkflowInstance_EntityType_EntityId",
                schema: "Hrms",
                table: "WorkflowInstance",
                newName: "IX_WorkflowInstance_EntityType_EntityId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsWorkflowInstance_DefinitionId",
                schema: "Hrms",
                table: "WorkflowInstance",
                newName: "IX_WorkflowInstance_DefinitionId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsWorkflowDefinition_TenantId_EntityType",
                schema: "Hrms",
                table: "WorkflowDefinition",
                newName: "IX_WorkflowDefinition_TenantId_EntityType");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsWorkflowActionLog_InstanceId",
                schema: "Hrms",
                table: "WorkflowActionLog",
                newName: "IX_WorkflowActionLog_InstanceId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsTripRequest_TripBudgetId",
                schema: "Hrms",
                table: "TripRequest",
                newName: "IX_TripRequest_TripBudgetId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsTripRequest_TenantId_TripNumber",
                schema: "Hrms",
                table: "TripRequest",
                newName: "IX_TripRequest_TenantId_TripNumber");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsTripRequest_Status",
                schema: "Hrms",
                table: "TripRequest",
                newName: "IX_TripRequest_Status");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsTripRequest_EmployeeId_Status",
                schema: "Hrms",
                table: "TripRequest",
                newName: "IX_TripRequest_EmployeeId_Status");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsTripExpense_TripRequestId",
                schema: "Hrms",
                table: "TripExpense",
                newName: "IX_TripExpense_TripRequestId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsTripBudget_TenantId_FiscalYear_OrganizationUnitId",
                schema: "Hrms",
                table: "TripBudget",
                newName: "IX_TripBudget_TenantId_FiscalYear_OrganizationUnitId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsTripBudget_OrganizationUnitId",
                schema: "Hrms",
                table: "TripBudget",
                newName: "IX_TripBudget_OrganizationUnitId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsTrainingSession_TrainingCourseId",
                schema: "Hrms",
                table: "TrainingSession",
                newName: "IX_TrainingSession_TrainingCourseId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsTrainingSession_TenantId_TrainingCourseId",
                schema: "Hrms",
                table: "TrainingSession",
                newName: "IX_TrainingSession_TenantId_TrainingCourseId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsTrainingSession_TenantId_StartDate",
                schema: "Hrms",
                table: "TrainingSession",
                newName: "IX_TrainingSession_TenantId_StartDate");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsTrainingProviderPayment_TrainingSessionId",
                schema: "Hrms",
                table: "TrainingProviderPayment",
                newName: "IX_TrainingProviderPayment_TrainingSessionId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsTrainingProviderPayment_TenantId_TrainingSessionId",
                schema: "Hrms",
                table: "TrainingProviderPayment",
                newName: "IX_TrainingProviderPayment_TenantId_TrainingSessionId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsTrainingProviderPayment_TenantId_Status",
                schema: "Hrms",
                table: "TrainingProviderPayment",
                newName: "IX_TrainingProviderPayment_TenantId_Status");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsTrainingNeed_TrainingCourseId",
                schema: "Hrms",
                table: "TrainingNeed",
                newName: "IX_TrainingNeed_TrainingCourseId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsTrainingNeed_TenantId_Status",
                schema: "Hrms",
                table: "TrainingNeed",
                newName: "IX_TrainingNeed_TenantId_Status");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsTrainingNeed_TenantId_EmployeeId",
                schema: "Hrms",
                table: "TrainingNeed",
                newName: "IX_TrainingNeed_TenantId_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsTrainingNeed_EmployeeId",
                schema: "Hrms",
                table: "TrainingNeed",
                newName: "IX_TrainingNeed_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsTrainingNeed_CompetencyId",
                schema: "Hrms",
                table: "TrainingNeed",
                newName: "IX_TrainingNeed_CompetencyId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsTrainingEnrollment_TrainingSessionId",
                schema: "Hrms",
                table: "TrainingEnrollment",
                newName: "IX_TrainingEnrollment_TrainingSessionId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsTrainingEnrollment_TrainingNeedId",
                schema: "Hrms",
                table: "TrainingEnrollment",
                newName: "IX_TrainingEnrollment_TrainingNeedId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsTrainingEnrollment_TenantId_TrainingSessionId_EmployeeId",
                schema: "Hrms",
                table: "TrainingEnrollment",
                newName: "IX_TrainingEnrollment_TenantId_TrainingSessionId_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsTrainingEnrollment_TenantId_EmployeeId_Status",
                schema: "Hrms",
                table: "TrainingEnrollment",
                newName: "IX_TrainingEnrollment_TenantId_EmployeeId_Status");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsTrainingEnrollment_EmployeeId",
                schema: "Hrms",
                table: "TrainingEnrollment",
                newName: "IX_TrainingEnrollment_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsTrainingCourse_TrainingCategoryId",
                schema: "Hrms",
                table: "TrainingCourse",
                newName: "IX_TrainingCourse_TrainingCategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsTrainingCourse_TenantId_TrainingCategoryId",
                schema: "Hrms",
                table: "TrainingCourse",
                newName: "IX_TrainingCourse_TenantId_TrainingCategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsTrainingCourse_TenantId_Name",
                schema: "Hrms",
                table: "TrainingCourse",
                newName: "IX_TrainingCourse_TenantId_Name");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsTrainingCategory_TenantId_Name",
                schema: "Hrms",
                table: "TrainingCategory",
                newName: "IX_TrainingCategory_TenantId_Name");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsTrainingBudget_TenantId_FiscalYear_OrganizationUnitId",
                schema: "Hrms",
                table: "TrainingBudget",
                newName: "IX_TrainingBudget_TenantId_FiscalYear_OrganizationUnitId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsTrainingBudget_OrganizationUnitId",
                schema: "Hrms",
                table: "TrainingBudget",
                newName: "IX_TrainingBudget_OrganizationUnitId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsTerminationSettlement_TerminationId",
                schema: "Hrms",
                table: "TerminationSettlement",
                newName: "IX_TerminationSettlement_TerminationId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsTerminationSettlement_TenantId_TerminationId",
                schema: "Hrms",
                table: "TerminationSettlement",
                newName: "IX_TerminationSettlement_TenantId_TerminationId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsTerminationClearance_TerminationId",
                schema: "Hrms",
                table: "TerminationClearance",
                newName: "IX_TerminationClearance_TerminationId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsTerminationClearance_DepartmentId",
                schema: "Hrms",
                table: "TerminationClearance",
                newName: "IX_TerminationClearance_DepartmentId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsTerminationAssetRecovery_TerminationId",
                schema: "Hrms",
                table: "TerminationAssetRecovery",
                newName: "IX_TerminationAssetRecovery_TerminationId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsTerminationAssetRecovery_TenantId_TerminationId",
                schema: "Hrms",
                table: "TerminationAssetRecovery",
                newName: "IX_TerminationAssetRecovery_TenantId_TerminationId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsTerminationAssetRecovery_CompanyAssetId",
                schema: "Hrms",
                table: "TerminationAssetRecovery",
                newName: "IX_TerminationAssetRecovery_CompanyAssetId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsTalentReview_TenantId_Status",
                schema: "Hrms",
                table: "TalentReview",
                newName: "IX_TalentReview_TenantId_Status");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsTalentReview_OrganizationUnitId",
                schema: "Hrms",
                table: "TalentReview",
                newName: "IX_TalentReview_OrganizationUnitId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsTalentRating_TalentAssessmentId",
                schema: "Hrms",
                table: "TalentRating",
                newName: "IX_TalentRating_TalentAssessmentId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsTalentRating_RaterEmployeeId",
                schema: "Hrms",
                table: "TalentRating",
                newName: "IX_TalentRating_RaterEmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsTalentAssessment_TenantId_TalentReviewId_PerformanceBand_PotentialBand",
                schema: "Hrms",
                table: "TalentAssessment",
                newName: "IX_TalentAssessment_TenantId_TalentReviewId_PerformanceBand_PotentialBand");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsTalentAssessment_TalentReviewId_EmployeeId",
                schema: "Hrms",
                table: "TalentAssessment",
                newName: "IX_TalentAssessment_TalentReviewId_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsTalentAssessment_EmployeeId",
                schema: "Hrms",
                table: "TalentAssessment",
                newName: "IX_TalentAssessment_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsSurveyResponse_TenantId_SurveyId",
                schema: "Hrms",
                table: "SurveyResponse",
                newName: "IX_SurveyResponse_TenantId_SurveyId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsSurveyResponse_SurveyId",
                schema: "Hrms",
                table: "SurveyResponse",
                newName: "IX_SurveyResponse_SurveyId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsSurveyCompletion_TenantId_SurveyId_EmployeeId",
                schema: "Hrms",
                table: "SurveyCompletion",
                newName: "IX_SurveyCompletion_TenantId_SurveyId_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsSurveyCompletion_SurveyId",
                schema: "Hrms",
                table: "SurveyCompletion",
                newName: "IX_SurveyCompletion_SurveyId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsSurveyCompletion_EmployeeId",
                schema: "Hrms",
                table: "SurveyCompletion",
                newName: "IX_SurveyCompletion_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsSurvey_TenantId_Status",
                schema: "Hrms",
                table: "Survey",
                newName: "IX_Survey_TenantId_Status");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsSuggestion_TenantId_Status",
                schema: "Hrms",
                table: "Suggestion",
                newName: "IX_Suggestion_TenantId_Status");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsSuccessionPlan_TenantId_Status",
                schema: "Hrms",
                table: "SuccessionPlan",
                newName: "IX_SuccessionPlan_TenantId_Status");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsSuccessionPlan_TenantId_CriticalPositionId",
                schema: "Hrms",
                table: "SuccessionPlan",
                newName: "IX_SuccessionPlan_TenantId_CriticalPositionId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsSuccessionPlan_CriticalPositionId",
                schema: "Hrms",
                table: "SuccessionPlan",
                newName: "IX_SuccessionPlan_CriticalPositionId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsSuccessionDevelopmentAction_SuccessionCandidateId",
                schema: "Hrms",
                table: "SuccessionDevelopmentAction",
                newName: "IX_SuccessionDevelopmentAction_SuccessionCandidateId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsSuccessionDevelopmentAction_MentorEmployeeId",
                schema: "Hrms",
                table: "SuccessionDevelopmentAction",
                newName: "IX_SuccessionDevelopmentAction_MentorEmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsSuccessionCandidate_TenantId_EmployeeId",
                schema: "Hrms",
                table: "SuccessionCandidate",
                newName: "IX_SuccessionCandidate_TenantId_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsSuccessionCandidate_SuccessionPlanId_Rank",
                schema: "Hrms",
                table: "SuccessionCandidate",
                newName: "IX_SuccessionCandidate_SuccessionPlanId_Rank");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsSuccessionCandidate_SuccessionPlanId_EmployeeId",
                schema: "Hrms",
                table: "SuccessionCandidate",
                newName: "IX_SuccessionCandidate_SuccessionPlanId_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsSuccessionCandidate_EmployeeId",
                schema: "Hrms",
                table: "SuccessionCandidate",
                newName: "IX_SuccessionCandidate_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsSettlementLine_TerminationSettlementId",
                schema: "Hrms",
                table: "SettlementLine",
                newName: "IX_SettlementLine_TerminationSettlementId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsSettlementLine_TenantId_TerminationSettlementId",
                schema: "Hrms",
                table: "SettlementLine",
                newName: "IX_SettlementLine_TenantId_TerminationSettlementId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsSalaryRevisionLine_SalaryRevisionId",
                schema: "Hrms",
                table: "SalaryRevisionLine",
                newName: "IX_SalaryRevisionLine_SalaryRevisionId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsSalaryRevisionLine_EmployeeId",
                schema: "Hrms",
                table: "SalaryRevisionLine",
                newName: "IX_SalaryRevisionLine_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsSalaryRevisionBand_SalaryRevisionId_MinScore",
                schema: "Hrms",
                table: "SalaryRevisionBand",
                newName: "IX_SalaryRevisionBand_SalaryRevisionId_MinScore");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsSalaryRevision_Status",
                schema: "Hrms",
                table: "SalaryRevision",
                newName: "IX_SalaryRevision_Status");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsRewardPointsTransaction_TenantId_EmployeeId_TransactionDate",
                schema: "Hrms",
                table: "RewardPointsTransaction",
                newName: "IX_RewardPointsTransaction_TenantId_EmployeeId_TransactionDate");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsRewardPointsTransaction_EmployeeId",
                schema: "Hrms",
                table: "RewardPointsTransaction",
                newName: "IX_RewardPointsTransaction_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsRewardNomination_TenantId_Status",
                schema: "Hrms",
                table: "RewardNomination",
                newName: "IX_RewardNomination_TenantId_Status");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsRewardNomination_TenantId_NomineeEmployeeId",
                schema: "Hrms",
                table: "RewardNomination",
                newName: "IX_RewardNomination_TenantId_NomineeEmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsRewardNomination_RecognitionProgramId",
                schema: "Hrms",
                table: "RewardNomination",
                newName: "IX_RewardNomination_RecognitionProgramId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsRewardNomination_RecognitionBadgeId",
                schema: "Hrms",
                table: "RewardNomination",
                newName: "IX_RewardNomination_RecognitionBadgeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsRewardNomination_NomineeEmployeeId",
                schema: "Hrms",
                table: "RewardNomination",
                newName: "IX_RewardNomination_NomineeEmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsRewardDisbursement_TenantId_Status",
                schema: "Hrms",
                table: "RewardDisbursement",
                newName: "IX_RewardDisbursement_TenantId_Status");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsRewardDisbursement_TenantId_EmployeeId",
                schema: "Hrms",
                table: "RewardDisbursement",
                newName: "IX_RewardDisbursement_TenantId_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsRewardDisbursement_RecognitionBadgeId",
                schema: "Hrms",
                table: "RewardDisbursement",
                newName: "IX_RewardDisbursement_RecognitionBadgeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsRewardDisbursement_EmployeeRecognitionId",
                schema: "Hrms",
                table: "RewardDisbursement",
                newName: "IX_RewardDisbursement_EmployeeRecognitionId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsRewardDisbursement_EmployeeId",
                schema: "Hrms",
                table: "RewardDisbursement",
                newName: "IX_RewardDisbursement_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsReviewCycle_TenantId_Status",
                schema: "Hrms",
                table: "ReviewCycle",
                newName: "IX_ReviewCycle_TenantId_Status");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsReviewCycle_TenantId_Name",
                schema: "Hrms",
                table: "ReviewCycle",
                newName: "IX_ReviewCycle_TenantId_Name");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsReviewCycle_RatingScaleId",
                schema: "Hrms",
                table: "ReviewCycle",
                newName: "IX_ReviewCycle_RatingScaleId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsReviewCycle_FiscalYearId",
                schema: "Hrms",
                table: "ReviewCycle",
                newName: "IX_ReviewCycle_FiscalYearId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsRequisitionScreeningCriterion_RequisitionId",
                schema: "Hrms",
                table: "RequisitionScreeningCriterion",
                newName: "IX_RequisitionScreeningCriterion_RequisitionId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsReportScheduleRecipient_ReportScheduleId",
                schema: "Hrms",
                table: "ReportScheduleRecipient",
                newName: "IX_ReportScheduleRecipient_ReportScheduleId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsReportScheduleFieldValue_ReportScheduleId",
                schema: "Hrms",
                table: "ReportScheduleFieldValue",
                newName: "IX_ReportScheduleFieldValue_ReportScheduleId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsReportScheduleFieldOutput_ReportScheduleId",
                schema: "Hrms",
                table: "ReportScheduleFieldOutput",
                newName: "IX_ReportScheduleFieldOutput_ReportScheduleId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsReportSchedule_ReportId",
                schema: "Hrms",
                table: "ReportSchedule",
                newName: "IX_ReportSchedule_ReportId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsReportSavedFilter_ReportId",
                schema: "Hrms",
                table: "ReportSavedFilter",
                newName: "IX_ReportSavedFilter_ReportId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsReportRunRecipient_ReportRunId",
                schema: "Hrms",
                table: "ReportRunRecipient",
                newName: "IX_ReportRunRecipient_ReportRunId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsReportRun_TenantId_ReportKey",
                schema: "Hrms",
                table: "ReportRun",
                newName: "IX_ReportRun_TenantId_ReportKey");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsReportRestriction_RoleId",
                schema: "Hrms",
                table: "ReportRestriction",
                newName: "IX_ReportRestriction_RoleId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsReportRestriction_ReportId",
                schema: "Hrms",
                table: "ReportRestriction",
                newName: "IX_ReportRestriction_ReportId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsReportFieldOutput_ReportId",
                schema: "Hrms",
                table: "ReportFieldOutput",
                newName: "IX_ReportFieldOutput_ReportId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsReportField_ReportId",
                schema: "Hrms",
                table: "ReportField",
                newName: "IX_ReportField_ReportId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsReport_TenantId_ReportKey",
                schema: "Hrms",
                table: "Report",
                newName: "IX_Report_TenantId_ReportKey");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsReport_TenantId_IsActive",
                schema: "Hrms",
                table: "Report",
                newName: "IX_Report_TenantId_IsActive");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsRecognitionProgram_TenantId_Name",
                schema: "Hrms",
                table: "RecognitionProgram",
                newName: "IX_RecognitionProgram_TenantId_Name");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsRecognitionProgram_RecognitionBadgeId",
                schema: "Hrms",
                table: "RecognitionProgram",
                newName: "IX_RecognitionProgram_RecognitionBadgeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsRecognitionBadge_TenantId_Name",
                schema: "Hrms",
                table: "RecognitionBadge",
                newName: "IX_RecognitionBadge_TenantId_Name");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsRecognitionBadge_AwardCategoryId",
                schema: "Hrms",
                table: "RecognitionBadge",
                newName: "IX_RecognitionBadge_AwardCategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsRatingScaleLevel_RatingScaleId_Value",
                schema: "Hrms",
                table: "RatingScaleLevel",
                newName: "IX_RatingScaleLevel_RatingScaleId_Value");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsRatingScale_TenantId_Name",
                schema: "Hrms",
                table: "RatingScale",
                newName: "IX_RatingScale_TenantId_Name");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsProfileChangeRequest_Status",
                schema: "Hrms",
                table: "ProfileChangeRequest",
                newName: "IX_ProfileChangeRequest_Status");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsProfileChangeRequest_EmployeeId",
                schema: "Hrms",
                table: "ProfileChangeRequest",
                newName: "IX_ProfileChangeRequest_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsPositionCompetency_PositionId_CompetencyId",
                schema: "Hrms",
                table: "PositionCompetency",
                newName: "IX_PositionCompetency_PositionId_CompetencyId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsPositionCompetency_CompetencyId",
                schema: "Hrms",
                table: "PositionCompetency",
                newName: "IX_PositionCompetency_CompetencyId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsPositionClass_WorkLocationId",
                schema: "Hrms",
                table: "PositionClass",
                newName: "IX_PositionClass_WorkLocationId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsPositionClass_TenantId_Code",
                schema: "Hrms",
                table: "PositionClass",
                newName: "IX_PositionClass_TenantId_Code");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsPositionClass_SalaryScaleId",
                schema: "Hrms",
                table: "PositionClass",
                newName: "IX_PositionClass_SalaryScaleId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsPositionClass_ReportsToPositionClassId",
                schema: "Hrms",
                table: "PositionClass",
                newName: "IX_PositionClass_ReportsToPositionClassId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsPositionClass_JobCategoryId",
                schema: "Hrms",
                table: "PositionClass",
                newName: "IX_PositionClass_JobCategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsPosition_TenantId_BranchId_Code",
                schema: "Hrms",
                table: "Position",
                newName: "IX_Position_TenantId_BranchId_Code");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsPosition_PositionClassId",
                schema: "Hrms",
                table: "Position",
                newName: "IX_Position_PositionClassId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsPosition_OrganizationUnitId",
                schema: "Hrms",
                table: "Position",
                newName: "IX_Position_OrganizationUnitId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsPosition_BranchId",
                schema: "Hrms",
                table: "Position",
                newName: "IX_Position_BranchId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsPipObjective_PipId_SortOrder",
                schema: "Hrms",
                table: "PipObjective",
                newName: "IX_PipObjective_PipId_SortOrder");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsPerformanceHistory_TenantId_EntityType_EntityId",
                schema: "Hrms",
                table: "PerformanceHistory",
                newName: "IX_PerformanceHistory_TenantId_EntityType_EntityId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsPerDiemRate_TenantId_JobGradeId_TripType",
                schema: "Hrms",
                table: "PerDiemRate",
                newName: "IX_PerDiemRate_TenantId_JobGradeId_TripType");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsPerDiemRate_JobGradeId",
                schema: "Hrms",
                table: "PerDiemRate",
                newName: "IX_PerDiemRate_JobGradeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsOtherLeaveSetting_TenantId_IsActive",
                schema: "Hrms",
                table: "OtherLeaveSetting",
                newName: "IX_OtherLeaveSetting_TenantId_IsActive");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsOtherLeaveSetting_TenantId_FiscalYearId_LeaveTypeId",
                schema: "Hrms",
                table: "OtherLeaveSetting",
                newName: "IX_OtherLeaveSetting_TenantId_FiscalYearId_LeaveTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsOtherLeaveSetting_LeaveTypeId",
                schema: "Hrms",
                table: "OtherLeaveSetting",
                newName: "IX_OtherLeaveSetting_LeaveTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsOtherLeaveSetting_FiscalYearId",
                schema: "Hrms",
                table: "OtherLeaveSetting",
                newName: "IX_OtherLeaveSetting_FiscalYearId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsOtherLeaveDetail_OtherLeaveHeaderId_StartDate_EndDate",
                schema: "Hrms",
                table: "OtherLeaveDetail",
                newName: "IX_OtherLeaveDetail_OtherLeaveHeaderId_StartDate_EndDate");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsOtherLeaveDetail_OtherLeaveHeaderId",
                schema: "Hrms",
                table: "OtherLeaveDetail",
                newName: "IX_OtherLeaveDetail_OtherLeaveHeaderId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsOtherLeave_OtherLeaveSettingId",
                schema: "Hrms",
                table: "OtherLeave",
                newName: "IX_OtherLeave_OtherLeaveSettingId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsOtherLeave_EmployeeId_Status",
                schema: "Hrms",
                table: "OtherLeave",
                newName: "IX_OtherLeave_EmployeeId_Status");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsOtherLeave_EmployeeId",
                schema: "Hrms",
                table: "OtherLeave",
                newName: "IX_OtherLeave_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsOrganizationUnit_WorkLocationId",
                schema: "Hrms",
                table: "OrganizationUnit",
                newName: "IX_OrganizationUnit_WorkLocationId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsOrganizationUnit_TenantId_BranchId_Code",
                schema: "Hrms",
                table: "OrganizationUnit",
                newName: "IX_OrganizationUnit_TenantId_BranchId_Code");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsOrganizationUnit_ParentId",
                schema: "Hrms",
                table: "OrganizationUnit",
                newName: "IX_OrganizationUnit_ParentId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsOrganizationUnit_BranchId",
                schema: "Hrms",
                table: "OrganizationUnit",
                newName: "IX_OrganizationUnit_BranchId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsOrganizationalObjective_TenantId_ReviewCycleId_Title",
                schema: "Hrms",
                table: "OrganizationalObjective",
                newName: "IX_OrganizationalObjective_TenantId_ReviewCycleId_Title");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsOrganizationalObjective_TenantId_ReviewCycleId",
                schema: "Hrms",
                table: "OrganizationalObjective",
                newName: "IX_OrganizationalObjective_TenantId_ReviewCycleId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsOrganizationalObjective_ReviewCycleId",
                schema: "Hrms",
                table: "OrganizationalObjective",
                newName: "IX_OrganizationalObjective_ReviewCycleId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsOrganizationalObjective_ParentObjectiveId",
                schema: "Hrms",
                table: "OrganizationalObjective",
                newName: "IX_OrganizationalObjective_ParentObjectiveId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsOrganizationalObjective_OrganizationUnitId",
                schema: "Hrms",
                table: "OrganizationalObjective",
                newName: "IX_OrganizationalObjective_OrganizationUnitId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsOfferLetterTemplate_TenantId",
                schema: "Hrms",
                table: "OfferLetterTemplate",
                newName: "IX_OfferLetterTemplate_TenantId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsMentorship_TenantId_MenteeEmployeeId",
                schema: "Hrms",
                table: "Mentorship",
                newName: "IX_Mentorship_TenantId_MenteeEmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsMentorship_MentorEmployeeId",
                schema: "Hrms",
                table: "Mentorship",
                newName: "IX_Mentorship_MentorEmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsMentorship_MenteeEmployeeId",
                schema: "Hrms",
                table: "Mentorship",
                newName: "IX_Mentorship_MenteeEmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsMedicalServiceContract_Status",
                schema: "Hrms",
                table: "MedicalServiceContract",
                newName: "IX_MedicalServiceContract_Status");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsMedicalServiceContract_MedicalProviderId",
                schema: "Hrms",
                table: "MedicalServiceContract",
                newName: "IX_MedicalServiceContract_MedicalProviderId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsMedicalProvider_TenantId_Name",
                schema: "Hrms",
                table: "MedicalProvider",
                newName: "IX_MedicalProvider_TenantId_Name");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsMedicalPlan_TenantId_Name",
                schema: "Hrms",
                table: "MedicalPlan",
                newName: "IX_MedicalPlan_TenantId_Name");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsMedicalEnrollment_MedicalPlanId",
                schema: "Hrms",
                table: "MedicalEnrollment",
                newName: "IX_MedicalEnrollment_MedicalPlanId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsMedicalEnrollment_EmployeeId",
                schema: "Hrms",
                table: "MedicalEnrollment",
                newName: "IX_MedicalEnrollment_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsMedicalClaimAttachment_MedicalClaimId",
                schema: "Hrms",
                table: "MedicalClaimAttachment",
                newName: "IX_MedicalClaimAttachment_MedicalClaimId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsMedicalClaim_TenantId_ClaimNumber",
                schema: "Hrms",
                table: "MedicalClaim",
                newName: "IX_MedicalClaim_TenantId_ClaimNumber");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsMedicalClaim_Status",
                schema: "Hrms",
                table: "MedicalClaim",
                newName: "IX_MedicalClaim_Status");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsMedicalClaim_MedicalEnrollmentId",
                schema: "Hrms",
                table: "MedicalClaim",
                newName: "IX_MedicalClaim_MedicalEnrollmentId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsMedicalClaim_MedicalBeneficiaryId_Status",
                schema: "Hrms",
                table: "MedicalClaim",
                newName: "IX_MedicalClaim_MedicalBeneficiaryId_Status");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsMedicalClaim_EmployeeId",
                schema: "Hrms",
                table: "MedicalClaim",
                newName: "IX_MedicalClaim_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsMedicalBeneficiary_MedicalEnrollmentId",
                schema: "Hrms",
                table: "MedicalBeneficiary",
                newName: "IX_MedicalBeneficiary_MedicalEnrollmentId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsLoanType_TenantId_Name",
                schema: "Hrms",
                table: "LoanType",
                newName: "IX_LoanType_TenantId_Name");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsLoanRepaymentSchedule_Status_DueDate",
                schema: "Hrms",
                table: "LoanRepaymentSchedule",
                newName: "IX_LoanRepaymentSchedule_Status_DueDate");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsLoanRepaymentSchedule_LoanId_InstallmentNo",
                schema: "Hrms",
                table: "LoanRepaymentSchedule",
                newName: "IX_LoanRepaymentSchedule_LoanId_InstallmentNo");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsLoanGuarantor_LoanId",
                schema: "Hrms",
                table: "LoanGuarantor",
                newName: "IX_LoanGuarantor_LoanId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsLoan_TenantId_LoanNumber",
                schema: "Hrms",
                table: "Loan",
                newName: "IX_Loan_TenantId_LoanNumber");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsLoan_Status",
                schema: "Hrms",
                table: "Loan",
                newName: "IX_Loan_Status");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsLoan_LoanTypeId",
                schema: "Hrms",
                table: "Loan",
                newName: "IX_Loan_LoanTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsLoan_EmployeeId_Status",
                schema: "Hrms",
                table: "Loan",
                newName: "IX_Loan_EmployeeId_Status");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsLeaveType_TenantId_Code",
                schema: "Hrms",
                table: "LeaveType",
                newName: "IX_LeaveType_TenantId_Code");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsLeaveRequestLine_LeaveTypeId",
                schema: "Hrms",
                table: "LeaveRequestLine",
                newName: "IX_LeaveRequestLine_LeaveTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsLeaveRequestLine_LeaveRequestId",
                schema: "Hrms",
                table: "LeaveRequestLine",
                newName: "IX_LeaveRequestLine_LeaveRequestId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsLeaveRequest_FiscalYearId",
                schema: "Hrms",
                table: "LeaveRequest",
                newName: "IX_LeaveRequest_FiscalYearId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsLeaveRequest_EmployeeId_Status",
                schema: "Hrms",
                table: "LeaveRequest",
                newName: "IX_LeaveRequest_EmployeeId_Status");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsLeaveRequest_EmployeeId",
                schema: "Hrms",
                table: "LeaveRequest",
                newName: "IX_LeaveRequest_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsLeaveBalanceTransaction_ReferenceId",
                schema: "Hrms",
                table: "LeaveBalanceTransaction",
                newName: "IX_LeaveBalanceTransaction_ReferenceId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsLeaveBalanceTransaction_EmployeeId_LeaveTypeId_FiscalYearId",
                schema: "Hrms",
                table: "LeaveBalanceTransaction",
                newName: "IX_LeaveBalanceTransaction_EmployeeId_LeaveTypeId_FiscalYearId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsLeaveBalance_TenantId_EmployeeId_LeaveTypeId_FiscalYearId",
                schema: "Hrms",
                table: "LeaveBalance",
                newName: "IX_LeaveBalance_TenantId_EmployeeId_LeaveTypeId_FiscalYearId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsLeaveBalance_LeaveTypeId",
                schema: "Hrms",
                table: "LeaveBalance",
                newName: "IX_LeaveBalance_LeaveTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsLeaveBalance_FiscalYearId",
                schema: "Hrms",
                table: "LeaveBalance",
                newName: "IX_LeaveBalance_FiscalYearId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsLeaveBalance_EmployeeId",
                schema: "Hrms",
                table: "LeaveBalance",
                newName: "IX_LeaveBalance_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsLearningPathStep_TrainingCourseId",
                schema: "Hrms",
                table: "LearningPathStep",
                newName: "IX_LearningPathStep_TrainingCourseId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsLearningPathStep_TenantId_LearningPathId",
                schema: "Hrms",
                table: "LearningPathStep",
                newName: "IX_LearningPathStep_TenantId_LearningPathId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsLearningPathStep_LearningPathId",
                schema: "Hrms",
                table: "LearningPathStep",
                newName: "IX_LearningPathStep_LearningPathId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsLearningPath_TenantId_Name",
                schema: "Hrms",
                table: "LearningPath",
                newName: "IX_LearningPath_TenantId_Name");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsLearningPath_TargetPositionId",
                schema: "Hrms",
                table: "LearningPath",
                newName: "IX_LearningPath_TargetPositionId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsLearningCommunityPost_TenantId_LearningCommunityId_ParentPostId",
                schema: "Hrms",
                table: "LearningCommunityPost",
                newName: "IX_LearningCommunityPost_TenantId_LearningCommunityId_ParentPostId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsLearningCommunityPost_LearningCommunityId",
                schema: "Hrms",
                table: "LearningCommunityPost",
                newName: "IX_LearningCommunityPost_LearningCommunityId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsLearningCommunityPost_EmployeeId",
                schema: "Hrms",
                table: "LearningCommunityPost",
                newName: "IX_LearningCommunityPost_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsLearningCommunityMember_TenantId_LearningCommunityId_EmployeeId",
                schema: "Hrms",
                table: "LearningCommunityMember",
                newName: "IX_LearningCommunityMember_TenantId_LearningCommunityId_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsLearningCommunityMember_LearningCommunityId",
                schema: "Hrms",
                table: "LearningCommunityMember",
                newName: "IX_LearningCommunityMember_LearningCommunityId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsLearningCommunityMember_EmployeeId",
                schema: "Hrms",
                table: "LearningCommunityMember",
                newName: "IX_LearningCommunityMember_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsLearningCommunity_TrainingCourseId",
                schema: "Hrms",
                table: "LearningCommunity",
                newName: "IX_LearningCommunity_TrainingCourseId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsLearningCommunity_TenantId_Name",
                schema: "Hrms",
                table: "LearningCommunity",
                newName: "IX_LearningCommunity_TenantId_Name");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsKnowledgeTransfer_SuccessionCandidateId",
                schema: "Hrms",
                table: "KnowledgeTransfer",
                newName: "IX_KnowledgeTransfer_SuccessionCandidateId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsKnowledgeTransfer_FromEmployeeId",
                schema: "Hrms",
                table: "KnowledgeTransfer",
                newName: "IX_KnowledgeTransfer_FromEmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsJobRequisition_WorkLocationId",
                schema: "Hrms",
                table: "JobRequisition",
                newName: "IX_JobRequisition_WorkLocationId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsJobRequisition_TenantId_Status",
                schema: "Hrms",
                table: "JobRequisition",
                newName: "IX_JobRequisition_TenantId_Status");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsJobRequisition_TenantId_RequisitionNumber",
                schema: "Hrms",
                table: "JobRequisition",
                newName: "IX_JobRequisition_TenantId_RequisitionNumber");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsJobRequisition_SalaryScaleId",
                schema: "Hrms",
                table: "JobRequisition",
                newName: "IX_JobRequisition_SalaryScaleId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsJobRequisition_PositionClassId",
                schema: "Hrms",
                table: "JobRequisition",
                newName: "IX_JobRequisition_PositionClassId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsJobRequisition_OrganizationUnitId",
                schema: "Hrms",
                table: "JobRequisition",
                newName: "IX_JobRequisition_OrganizationUnitId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsJobRequisition_HiringRequestId",
                schema: "Hrms",
                table: "JobRequisition",
                newName: "IX_JobRequisition_HiringRequestId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsJobOffer_TenantId_Status",
                schema: "Hrms",
                table: "JobOffer",
                newName: "IX_JobOffer_TenantId_Status");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsJobOffer_TenantId_OfferNumber",
                schema: "Hrms",
                table: "JobOffer",
                newName: "IX_JobOffer_TenantId_OfferNumber");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsJobOffer_SalaryScaleId",
                schema: "Hrms",
                table: "JobOffer",
                newName: "IX_JobOffer_SalaryScaleId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsJobOffer_HiringManagerEmployeeId",
                schema: "Hrms",
                table: "JobOffer",
                newName: "IX_JobOffer_HiringManagerEmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsJobOffer_HiredEmployeeId",
                schema: "Hrms",
                table: "JobOffer",
                newName: "IX_JobOffer_HiredEmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsJobOffer_ApplicationId_CreatedAt",
                schema: "Hrms",
                table: "JobOffer",
                newName: "IX_JobOffer_ApplicationId_CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsJobGrade_TenantId_Code",
                schema: "Hrms",
                table: "JobGrade",
                newName: "IX_JobGrade_TenantId_Code");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsJobCategory_TenantId_Code",
                schema: "Hrms",
                table: "JobCategory",
                newName: "IX_JobCategory_TenantId_Code");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsJobApplicationStageLog_ApplicationId",
                schema: "Hrms",
                table: "JobApplicationStageLog",
                newName: "IX_JobApplicationStageLog_ApplicationId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsJobApplication_TenantId_Stage",
                schema: "Hrms",
                table: "JobApplication",
                newName: "IX_JobApplication_TenantId_Stage");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsJobApplication_TenantId_AppliedAt",
                schema: "Hrms",
                table: "JobApplication",
                newName: "IX_JobApplication_TenantId_AppliedAt");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsJobApplication_RequisitionId",
                schema: "Hrms",
                table: "JobApplication",
                newName: "IX_JobApplication_RequisitionId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsJobApplication_CandidateId_RequisitionId",
                schema: "Hrms",
                table: "JobApplication",
                newName: "IX_JobApplication_CandidateId_RequisitionId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsInterviewPanelist_InterviewId_EmployeeId",
                schema: "Hrms",
                table: "InterviewPanelist",
                newName: "IX_InterviewPanelist_InterviewId_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsInterviewPanelist_EmployeeId",
                schema: "Hrms",
                table: "InterviewPanelist",
                newName: "IX_InterviewPanelist_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsInterviewFeedback_PanelistId_CriterionId",
                schema: "Hrms",
                table: "InterviewFeedback",
                newName: "IX_InterviewFeedback_PanelistId_CriterionId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsInterviewFeedback_PanelistId",
                schema: "Hrms",
                table: "InterviewFeedback",
                newName: "IX_InterviewFeedback_PanelistId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsInterview_TenantId_Status",
                schema: "Hrms",
                table: "Interview",
                newName: "IX_Interview_TenantId_Status");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsInterview_ScheduledStart",
                schema: "Hrms",
                table: "Interview",
                newName: "IX_Interview_ScheduledStart");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsInterview_ApplicationId",
                schema: "Hrms",
                table: "Interview",
                newName: "IX_Interview_ApplicationId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsInsurancePremiumSchedule_Status_DueDate",
                schema: "Hrms",
                table: "InsurancePremiumSchedule",
                newName: "IX_InsurancePremiumSchedule_Status_DueDate");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsInsurancePremiumSchedule_InsurancePolicyId_Installment",
                schema: "Hrms",
                table: "InsurancePremiumSchedule",
                newName: "IX_InsurancePremiumSchedule_InsurancePolicyId_Installment");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsInsurancePolicy_TenantId_PolicyNumber",
                schema: "Hrms",
                table: "InsurancePolicy",
                newName: "IX_InsurancePolicy_TenantId_PolicyNumber");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsInsurancePolicy_Status",
                schema: "Hrms",
                table: "InsurancePolicy",
                newName: "IX_InsurancePolicy_Status");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsInsuranceClaimAttachment_InsuranceClaimId",
                schema: "Hrms",
                table: "InsuranceClaimAttachment",
                newName: "IX_InsuranceClaimAttachment_InsuranceClaimId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsInsuranceClaim_TenantId_ClaimNumber",
                schema: "Hrms",
                table: "InsuranceClaim",
                newName: "IX_InsuranceClaim_TenantId_ClaimNumber");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsInsuranceClaim_Status",
                schema: "Hrms",
                table: "InsuranceClaim",
                newName: "IX_InsuranceClaim_Status");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsInsuranceClaim_InsurancePolicyId",
                schema: "Hrms",
                table: "InsuranceClaim",
                newName: "IX_InsuranceClaim_InsurancePolicyId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsInsuranceClaim_EmployeeId",
                schema: "Hrms",
                table: "InsuranceClaim",
                newName: "IX_InsuranceClaim_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsImprovementPlan_TenantId_EmployeeId",
                schema: "Hrms",
                table: "ImprovementPlan",
                newName: "IX_ImprovementPlan_TenantId_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsImprovementPlan_EmployeeId",
                schema: "Hrms",
                table: "ImprovementPlan",
                newName: "IX_ImprovementPlan_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsImprovementPlan_AppraisalId",
                schema: "Hrms",
                table: "ImprovementPlan",
                newName: "IX_ImprovementPlan_AppraisalId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsHoliday_TenantId_Date",
                schema: "Hrms",
                table: "Holiday",
                newName: "IX_Holiday_TenantId_Date");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsHiringRequest_TenantId_Status",
                schema: "Hrms",
                table: "HiringRequest",
                newName: "IX_HiringRequest_TenantId_Status");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsHiringRequest_TenantId_RequestNumber",
                schema: "Hrms",
                table: "HiringRequest",
                newName: "IX_HiringRequest_TenantId_RequestNumber");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsHiringRequest_PositionClassId",
                schema: "Hrms",
                table: "HiringRequest",
                newName: "IX_HiringRequest_PositionClassId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsHiringRequest_OrganizationUnitId",
                schema: "Hrms",
                table: "HiringRequest",
                newName: "IX_HiringRequest_OrganizationUnitId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsGrievanceNote_TenantId_GrievanceId",
                schema: "Hrms",
                table: "GrievanceNote",
                newName: "IX_GrievanceNote_TenantId_GrievanceId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsGrievanceNote_GrievanceId",
                schema: "Hrms",
                table: "GrievanceNote",
                newName: "IX_GrievanceNote_GrievanceId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsGrievance_TenantId_Status",
                schema: "Hrms",
                table: "Grievance",
                newName: "IX_Grievance_TenantId_Status");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsGrievance_TenantId_EmployeeId",
                schema: "Hrms",
                table: "Grievance",
                newName: "IX_Grievance_TenantId_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsGrievance_TenantId_AssignedToEmployeeId",
                schema: "Hrms",
                table: "Grievance",
                newName: "IX_Grievance_TenantId_AssignedToEmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsGrievance_EmployeeId",
                schema: "Hrms",
                table: "Grievance",
                newName: "IX_Grievance_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsGoalActionItem_EmployeeGoalId_SortOrder",
                schema: "Hrms",
                table: "GoalActionItem",
                newName: "IX_GoalActionItem_EmployeeGoalId_SortOrder");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsExitInterview_TerminationId",
                schema: "Hrms",
                table: "ExitInterview",
                newName: "IX_ExitInterview_TerminationId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsExitInterview_TenantId_TerminationId",
                schema: "Hrms",
                table: "ExitInterview",
                newName: "IX_ExitInterview_TenantId_TerminationId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsEmployeeTrainingCertificate_TrainingEnrollmentId",
                schema: "Hrms",
                table: "EmployeeTrainingCertificate",
                newName: "IX_EmployeeTrainingCertificate_TrainingEnrollmentId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsEmployeeTrainingCertificate_TrainingCourseId",
                schema: "Hrms",
                table: "EmployeeTrainingCertificate",
                newName: "IX_EmployeeTrainingCertificate_TrainingCourseId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsEmployeeTrainingCertificate_TenantId_ExpiresOn",
                schema: "Hrms",
                table: "EmployeeTrainingCertificate",
                newName: "IX_EmployeeTrainingCertificate_TenantId_ExpiresOn");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsEmployeeTrainingCertificate_TenantId_EmployeeId",
                schema: "Hrms",
                table: "EmployeeTrainingCertificate",
                newName: "IX_EmployeeTrainingCertificate_TenantId_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsEmployeeTrainingCertificate_TenantId_CertificateNo",
                schema: "Hrms",
                table: "EmployeeTrainingCertificate",
                newName: "IX_EmployeeTrainingCertificate_TenantId_CertificateNo");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsEmployeeTrainingCertificate_EmployeeId",
                schema: "Hrms",
                table: "EmployeeTrainingCertificate",
                newName: "IX_EmployeeTrainingCertificate_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsEmployeeTermination_Status",
                schema: "Hrms",
                table: "EmployeeTermination",
                newName: "IX_EmployeeTermination_Status");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsEmployeeTermination_EmployeeId",
                schema: "Hrms",
                table: "EmployeeTermination",
                newName: "IX_EmployeeTermination_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsEmployeeRecognition_TenantId_IsPublic_RecognizedOn",
                schema: "Hrms",
                table: "EmployeeRecognition",
                newName: "IX_EmployeeRecognition_TenantId_IsPublic_RecognizedOn");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsEmployeeRecognition_TenantId_EmployeeId",
                schema: "Hrms",
                table: "EmployeeRecognition",
                newName: "IX_EmployeeRecognition_TenantId_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsEmployeeRecognition_RecognitionBadgeId",
                schema: "Hrms",
                table: "EmployeeRecognition",
                newName: "IX_EmployeeRecognition_RecognitionBadgeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsEmployeeRecognition_EmployeeId",
                schema: "Hrms",
                table: "EmployeeRecognition",
                newName: "IX_EmployeeRecognition_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsEmployeeMovement_ToSalaryScaleId",
                schema: "Hrms",
                table: "EmployeeMovement",
                newName: "IX_EmployeeMovement_ToSalaryScaleId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsEmployeeMovement_Status_EffectiveDate",
                schema: "Hrms",
                table: "EmployeeMovement",
                newName: "IX_EmployeeMovement_Status_EffectiveDate");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsEmployeeMovement_EmployeeId",
                schema: "Hrms",
                table: "EmployeeMovement",
                newName: "IX_EmployeeMovement_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsEmployeeGuarantee_TenantId_Status",
                schema: "Hrms",
                table: "EmployeeGuarantee",
                newName: "IX_EmployeeGuarantee_TenantId_Status");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsEmployeeGuarantee_TenantId_EndDate",
                schema: "Hrms",
                table: "EmployeeGuarantee",
                newName: "IX_EmployeeGuarantee_TenantId_EndDate");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsEmployeeGuarantee_TenantId_EmployeeId",
                schema: "Hrms",
                table: "EmployeeGuarantee",
                newName: "IX_EmployeeGuarantee_TenantId_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsEmployeeGuarantee_EmployeeId",
                schema: "Hrms",
                table: "EmployeeGuarantee",
                newName: "IX_EmployeeGuarantee_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsEmployeeGoal_TenantId_EmployeeId_ReviewCycleId",
                schema: "Hrms",
                table: "EmployeeGoal",
                newName: "IX_EmployeeGoal_TenantId_EmployeeId_ReviewCycleId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsEmployeeGoal_ReviewCycleId",
                schema: "Hrms",
                table: "EmployeeGoal",
                newName: "IX_EmployeeGoal_ReviewCycleId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsEmployeeGoal_OrganizationalObjectiveId",
                schema: "Hrms",
                table: "EmployeeGoal",
                newName: "IX_EmployeeGoal_OrganizationalObjectiveId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsEmployeeGoal_EmployeeId",
                schema: "Hrms",
                table: "EmployeeGoal",
                newName: "IX_EmployeeGoal_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsEmployeeFieldValue_OwnerType_OwnerId_FieldDefinitionId",
                schema: "Hrms",
                table: "EmployeeFieldValue",
                newName: "IX_EmployeeFieldValue_OwnerType_OwnerId_FieldDefinitionId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsEmployeeFieldValue_FieldDefinitionId",
                schema: "Hrms",
                table: "EmployeeFieldValue",
                newName: "IX_EmployeeFieldValue_FieldDefinitionId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsEmployeeFieldDefinition_TenantId_OwnerType_Name",
                schema: "Hrms",
                table: "EmployeeFieldDefinition",
                newName: "IX_EmployeeFieldDefinition_TenantId_OwnerType_Name");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsEmployeeExperience_PersonId",
                schema: "Hrms",
                table: "EmployeeExperience",
                newName: "IX_EmployeeExperience_PersonId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsEmployeeEducation_PersonId",
                schema: "Hrms",
                table: "EmployeeEducation",
                newName: "IX_EmployeeEducation_PersonId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsEmployeeDocument_OwnerType_OwnerId",
                schema: "Hrms",
                table: "EmployeeDocument",
                newName: "IX_EmployeeDocument_OwnerType_OwnerId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsEmployeeDocument_EmployeeId",
                schema: "Hrms",
                table: "EmployeeDocument",
                newName: "IX_EmployeeDocument_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsEmployeeDependent_RelatedEmployeeId",
                schema: "Hrms",
                table: "EmployeeDependent",
                newName: "IX_EmployeeDependent_RelatedEmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsEmployeeDependent_PersonId",
                schema: "Hrms",
                table: "EmployeeDependent",
                newName: "IX_EmployeeDependent_PersonId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsEmployeeCareerPathStepProgress_EmployeeCareerPathId",
                schema: "Hrms",
                table: "EmployeeCareerPathStepProgress",
                newName: "IX_EmployeeCareerPathStepProgress_EmployeeCareerPathId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsEmployeeCareerPath_TenantId_EmployeeId_CareerPathId",
                schema: "Hrms",
                table: "EmployeeCareerPath",
                newName: "IX_EmployeeCareerPath_TenantId_EmployeeId_CareerPathId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsEmployeeCareerPath_TenantId_EmployeeId",
                schema: "Hrms",
                table: "EmployeeCareerPath",
                newName: "IX_EmployeeCareerPath_TenantId_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsEmployeeCareerPath_TenantId_CareerPathId",
                schema: "Hrms",
                table: "EmployeeCareerPath",
                newName: "IX_EmployeeCareerPath_TenantId_CareerPathId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsEmployeeCareerPath_EmployeeId",
                schema: "Hrms",
                table: "EmployeeCareerPath",
                newName: "IX_EmployeeCareerPath_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsEmployeeCareerPath_CareerPathId",
                schema: "Hrms",
                table: "EmployeeCareerPath",
                newName: "IX_EmployeeCareerPath_CareerPathId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsEmployeeBenefitEnrollment_EmployeeId",
                schema: "Hrms",
                table: "EmployeeBenefitEnrollment",
                newName: "IX_EmployeeBenefitEnrollment_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsEmployeeBenefitEnrollment_BenefitPlanId",
                schema: "Hrms",
                table: "EmployeeBenefitEnrollment",
                newName: "IX_EmployeeBenefitEnrollment_BenefitPlanId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsEmployeeAllowance_EmployeeId",
                schema: "Hrms",
                table: "EmployeeAllowance",
                newName: "IX_EmployeeAllowance_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsEmployeeAllowance_AllowanceTypeId",
                schema: "Hrms",
                table: "EmployeeAllowance",
                newName: "IX_EmployeeAllowance_AllowanceTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsEmployee_TenantId_PositionId_EmployeeNumber",
                schema: "Hrms",
                table: "Employee",
                newName: "IX_Employee_TenantId_PositionId_EmployeeNumber");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsEmployee_TenantId_EmployeeNumber",
                schema: "Hrms",
                table: "Employee",
                newName: "IX_Employee_TenantId_EmployeeNumber");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsEmployee_TenantId_BranchId_EmploymentStatus",
                schema: "Hrms",
                table: "Employee",
                newName: "IX_Employee_TenantId_BranchId_EmploymentStatus");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsEmployee_SalaryScaleId",
                schema: "Hrms",
                table: "Employee",
                newName: "IX_Employee_SalaryScaleId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsEmployee_PositionId",
                schema: "Hrms",
                table: "Employee",
                newName: "IX_Employee_PositionId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsEmployee_PersonId",
                schema: "Hrms",
                table: "Employee",
                newName: "IX_Employee_PersonId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsEmployee_EmploymentStatus_IsProbation",
                schema: "Hrms",
                table: "Employee",
                newName: "IX_Employee_EmploymentStatus_IsProbation");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsEmployee_EmploymentStatus",
                schema: "Hrms",
                table: "Employee",
                newName: "IX_Employee_EmploymentStatus");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsEmployee_DateOfBirth",
                schema: "Hrms",
                table: "Employee",
                newName: "IX_Employee_DateOfBirth");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsEmployee_BranchId",
                schema: "Hrms",
                table: "Employee",
                newName: "IX_Employee_BranchId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsDynamicFormRecord_DynamicFormId_OwnerType_OwnerId_CreatedAt",
                schema: "Hrms",
                table: "DynamicFormRecord",
                newName: "IX_DynamicFormRecord_DynamicFormId_OwnerType_OwnerId_CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsDynamicFormField_DynamicFormId_Name",
                schema: "Hrms",
                table: "DynamicFormField",
                newName: "IX_DynamicFormField_DynamicFormId_Name");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsDynamicForm_TenantId_Module_Name",
                schema: "Hrms",
                table: "DynamicForm",
                newName: "IX_DynamicForm_TenantId_Module_Name");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsDocumentTemplate_TenantId_Name",
                schema: "Hrms",
                table: "DocumentTemplate",
                newName: "IX_DocumentTemplate_TenantId_Name");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsDisciplinaryMeasure_Status",
                schema: "Hrms",
                table: "DisciplinaryMeasure",
                newName: "IX_DisciplinaryMeasure_Status");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsDisciplinaryMeasure_EmployeeId_Status",
                schema: "Hrms",
                table: "DisciplinaryMeasure",
                newName: "IX_DisciplinaryMeasure_EmployeeId_Status");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsDevelopmentPlan_TenantId_EmployeeId",
                schema: "Hrms",
                table: "DevelopmentPlan",
                newName: "IX_DevelopmentPlan_TenantId_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsDevelopmentPlan_EmployeeId",
                schema: "Hrms",
                table: "DevelopmentPlan",
                newName: "IX_DevelopmentPlan_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsDevelopmentPlan_AppraisalId",
                schema: "Hrms",
                table: "DevelopmentPlan",
                newName: "IX_DevelopmentPlan_AppraisalId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsDevelopmentAction_DevelopmentPlanId_SortOrder",
                schema: "Hrms",
                table: "DevelopmentAction",
                newName: "IX_DevelopmentAction_DevelopmentPlanId_SortOrder");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsDevelopmentAction_CompetencyId",
                schema: "Hrms",
                table: "DevelopmentAction",
                newName: "IX_DevelopmentAction_CompetencyId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsCriticalPosition_TenantId_PositionId",
                schema: "Hrms",
                table: "CriticalPosition",
                newName: "IX_CriticalPosition_TenantId_PositionId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsCriticalPosition_TenantId_IsActive",
                schema: "Hrms",
                table: "CriticalPosition",
                newName: "IX_CriticalPosition_TenantId_IsActive");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsCriticalPosition_PositionId",
                schema: "Hrms",
                table: "CriticalPosition",
                newName: "IX_CriticalPosition_PositionId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsCriterionEvaluator_EmployeeId",
                schema: "Hrms",
                table: "CriterionEvaluator",
                newName: "IX_CriterionEvaluator_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsCriterionEvaluator_CriterionId",
                schema: "Hrms",
                table: "CriterionEvaluator",
                newName: "IX_CriterionEvaluator_CriterionId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsCompetencyCategory_TenantId_Name",
                schema: "Hrms",
                table: "CompetencyCategory",
                newName: "IX_CompetencyCategory_TenantId_Name");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsCompetency_TenantId_Name",
                schema: "Hrms",
                table: "Competency",
                newName: "IX_Competency_TenantId_Name");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsCompetency_CompetencyCategoryId",
                schema: "Hrms",
                table: "Competency",
                newName: "IX_Competency_CompetencyCategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsCompensationRequest_Status",
                schema: "Hrms",
                table: "CompensationRequest",
                newName: "IX_CompensationRequest_Status");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsCompensationRequest_EmployeeId",
                schema: "Hrms",
                table: "CompensationRequest",
                newName: "IX_CompensationRequest_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsCompanyProfile_TenantId",
                schema: "Hrms",
                table: "CompanyProfile",
                newName: "IX_CompanyProfile_TenantId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsCompanyAsset_TenantId_Status",
                schema: "Hrms",
                table: "CompanyAsset",
                newName: "IX_CompanyAsset_TenantId_Status");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsCompanyAsset_TenantId_AssignedToEmployeeId",
                schema: "Hrms",
                table: "CompanyAsset",
                newName: "IX_CompanyAsset_TenantId_AssignedToEmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsCompanyAsset_AssignedToEmployeeId",
                schema: "Hrms",
                table: "CompanyAsset",
                newName: "IX_CompanyAsset_AssignedToEmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsCommunityPostReaction_TenantId_LearningCommunityPostId_EmployeeId",
                schema: "Hrms",
                table: "CommunityPostReaction",
                newName: "IX_CommunityPostReaction_TenantId_LearningCommunityPostId_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsCommunityPostReaction_LearningCommunityPostId",
                schema: "Hrms",
                table: "CommunityPostReaction",
                newName: "IX_CommunityPostReaction_LearningCommunityPostId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsCommunityPostReaction_EmployeeId",
                schema: "Hrms",
                table: "CommunityPostReaction",
                newName: "IX_CommunityPostReaction_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsClearanceDepartmentApprover_DepartmentId",
                schema: "Hrms",
                table: "ClearanceDepartmentApprover",
                newName: "IX_ClearanceDepartmentApprover_DepartmentId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsClearanceDepartmentApprover_ApproverType_ApproverId",
                schema: "Hrms",
                table: "ClearanceDepartmentApprover",
                newName: "IX_ClearanceDepartmentApprover_ApproverType_ApproverId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsClearanceDepartment_TenantId_Name",
                schema: "Hrms",
                table: "ClearanceDepartment",
                newName: "IX_ClearanceDepartment_TenantId_Name");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsCareerPathStepCompetency_CompetencyId",
                schema: "Hrms",
                table: "CareerPathStepCompetency",
                newName: "IX_CareerPathStepCompetency_CompetencyId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsCareerPathStepCompetency_CareerPathStepId_CompetencyId",
                schema: "Hrms",
                table: "CareerPathStepCompetency",
                newName: "IX_CareerPathStepCompetency_CareerPathStepId_CompetencyId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsCareerPathStep_PositionClassId",
                schema: "Hrms",
                table: "CareerPathStep",
                newName: "IX_CareerPathStep_PositionClassId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsCareerPathStep_JobGradeId",
                schema: "Hrms",
                table: "CareerPathStep",
                newName: "IX_CareerPathStep_JobGradeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsCareerPathStep_CareerPathId_StepOrder",
                schema: "Hrms",
                table: "CareerPathStep",
                newName: "IX_CareerPathStep_CareerPathId_StepOrder");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsCareerPathChangeRequest_TenantId_EmployeeId_Status",
                schema: "Hrms",
                table: "CareerPathChangeRequest",
                newName: "IX_CareerPathChangeRequest_TenantId_EmployeeId_Status");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsCareerPathChangeRequest_RequestedCareerPathId",
                schema: "Hrms",
                table: "CareerPathChangeRequest",
                newName: "IX_CareerPathChangeRequest_RequestedCareerPathId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsCareerPathChangeRequest_EmployeeId",
                schema: "Hrms",
                table: "CareerPathChangeRequest",
                newName: "IX_CareerPathChangeRequest_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsCareerPathChangeRequest_CurrentCareerPathId",
                schema: "Hrms",
                table: "CareerPathChangeRequest",
                newName: "IX_CareerPathChangeRequest_CurrentCareerPathId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsCareerPath_TenantId_Code",
                schema: "Hrms",
                table: "CareerPath",
                newName: "IX_CareerPath_TenantId_Code");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsCandidateDocument_CandidateId_DocumentType",
                schema: "Hrms",
                table: "CandidateDocument",
                newName: "IX_CandidateDocument_CandidateId_DocumentType");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsCandidate_TenantId_IsInTalentPool",
                schema: "Hrms",
                table: "Candidate",
                newName: "IX_Candidate_TenantId_IsInTalentPool");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsCandidate_TenantId_CandidateNumber",
                schema: "Hrms",
                table: "Candidate",
                newName: "IX_Candidate_TenantId_CandidateNumber");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsCandidate_PersonId",
                schema: "Hrms",
                table: "Candidate",
                newName: "IX_Candidate_PersonId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsCandidate_InternalEmployeeId",
                schema: "Hrms",
                table: "Candidate",
                newName: "IX_Candidate_InternalEmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsCandidate_HiredEmployeeId",
                schema: "Hrms",
                table: "Candidate",
                newName: "IX_Candidate_HiredEmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsCandidate_Email",
                schema: "Hrms",
                table: "Candidate",
                newName: "IX_Candidate_Email");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsCalibrationSession_TenantId_ReviewCycleId",
                schema: "Hrms",
                table: "CalibrationSession",
                newName: "IX_CalibrationSession_TenantId_ReviewCycleId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsCalibrationSession_ReviewCycleId",
                schema: "Hrms",
                table: "CalibrationSession",
                newName: "IX_CalibrationSession_ReviewCycleId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsCalibrationSession_OrganizationUnitId",
                schema: "Hrms",
                table: "CalibrationSession",
                newName: "IX_CalibrationSession_OrganizationUnitId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsCalibrationItem_CalibrationSessionId",
                schema: "Hrms",
                table: "CalibrationItem",
                newName: "IX_CalibrationItem_CalibrationSessionId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsCalibrationItem_AppraisalId",
                schema: "Hrms",
                table: "CalibrationItem",
                newName: "IX_CalibrationItem_AppraisalId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsBranch_TenantId_Code",
                schema: "Hrms",
                table: "Branch",
                newName: "IX_Branch_TenantId_Code");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsBranch_ParentId",
                schema: "Hrms",
                table: "Branch",
                newName: "IX_Branch_ParentId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsBenefitPlan_TenantId_Name",
                schema: "Hrms",
                table: "BenefitPlan",
                newName: "IX_BenefitPlan_TenantId_Name");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsAwardCategory_TenantId_Name",
                schema: "Hrms",
                table: "AwardCategory",
                newName: "IX_AwardCategory_TenantId_Name");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsAuditLog_EntityType_EntityId",
                schema: "Hrms",
                table: "AuditLog",
                newName: "IX_AuditLog_EntityType_EntityId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsAuditLog_CreatedAt",
                schema: "Hrms",
                table: "AuditLog",
                newName: "IX_AuditLog_CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsAuditLog_BranchId",
                schema: "Hrms",
                table: "AuditLog",
                newName: "IX_AuditLog_BranchId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsAuditLog_Action",
                schema: "Hrms",
                table: "AuditLog",
                newName: "IX_AuditLog_Action");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsAppraisalTemplate_TenantId_Name",
                schema: "Hrms",
                table: "AppraisalTemplate",
                newName: "IX_AppraisalTemplate_TenantId_Name");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsAppraisalPeerReview_PeerEmployeeId",
                schema: "Hrms",
                table: "AppraisalPeerReview",
                newName: "IX_AppraisalPeerReview_PeerEmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsAppraisalPeerReview_AppraisalId_PeerEmployeeId",
                schema: "Hrms",
                table: "AppraisalPeerReview",
                newName: "IX_AppraisalPeerReview_AppraisalId_PeerEmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsAppraisalGoal_AppraisalId_SortOrder",
                schema: "Hrms",
                table: "AppraisalGoal",
                newName: "IX_AppraisalGoal_AppraisalId_SortOrder");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsAppraisalCompetency_AppraisalId_SortOrder",
                schema: "Hrms",
                table: "AppraisalCompetency",
                newName: "IX_AppraisalCompetency_AppraisalId_SortOrder");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsAppraisalAppeal_TenantId_Status",
                schema: "Hrms",
                table: "AppraisalAppeal",
                newName: "IX_AppraisalAppeal_TenantId_Status");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsAppraisalAppeal_EmployeeId",
                schema: "Hrms",
                table: "AppraisalAppeal",
                newName: "IX_AppraisalAppeal_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsAppraisalAppeal_AppraisalId",
                schema: "Hrms",
                table: "AppraisalAppeal",
                newName: "IX_AppraisalAppeal_AppraisalId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsAppraisal_TenantId_ReviewCycleId_Stage",
                schema: "Hrms",
                table: "Appraisal",
                newName: "IX_Appraisal_TenantId_ReviewCycleId_Stage");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsAppraisal_TenantId_EmployeeId_ReviewCycleId",
                schema: "Hrms",
                table: "Appraisal",
                newName: "IX_Appraisal_TenantId_EmployeeId_ReviewCycleId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsAppraisal_ReviewCycleId",
                schema: "Hrms",
                table: "Appraisal",
                newName: "IX_Appraisal_ReviewCycleId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsAppraisal_EmployeeId",
                schema: "Hrms",
                table: "Appraisal",
                newName: "IX_Appraisal_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsApplicationCriterionScore_ApplicationId_CriterionId",
                schema: "Hrms",
                table: "ApplicationCriterionScore",
                newName: "IX_ApplicationCriterionScore_ApplicationId_CriterionId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsAnnualLeaveSetting_TenantId_FiscalYearId",
                schema: "Hrms",
                table: "AnnualLeaveSetting",
                newName: "IX_AnnualLeaveSetting_TenantId_FiscalYearId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsAnnualLeaveSetting_FiscalYearId",
                schema: "Hrms",
                table: "AnnualLeaveSetting",
                newName: "IX_AnnualLeaveSetting_FiscalYearId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsAnnualLeaveHeader_EmployeeId_Status",
                schema: "Hrms",
                table: "AnnualLeaveHeader",
                newName: "IX_AnnualLeaveHeader_EmployeeId_Status");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsAnnualLeaveHeader_EmployeeId",
                schema: "Hrms",
                table: "AnnualLeaveHeader",
                newName: "IX_AnnualLeaveHeader_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsAnnualLeaveHeader_AnnualLeaveLedgerId",
                schema: "Hrms",
                table: "AnnualLeaveHeader",
                newName: "IX_AnnualLeaveHeader_AnnualLeaveLedgerId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsAnnualLeaveDetail_AnnualLeaveHeaderId_StartDate_EndDate",
                schema: "Hrms",
                table: "AnnualLeaveDetail",
                newName: "IX_AnnualLeaveDetail_AnnualLeaveHeaderId_StartDate_EndDate");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsAnnualLeaveDetail_AnnualLeaveHeaderId",
                schema: "Hrms",
                table: "AnnualLeaveDetail",
                newName: "IX_AnnualLeaveDetail_AnnualLeaveHeaderId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsAnnouncement_TenantId_IsActive_PublishFrom",
                schema: "Hrms",
                table: "Announcement",
                newName: "IX_Announcement_TenantId_IsActive_PublishFrom");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsAnnouncement_OrganizationUnitId",
                schema: "Hrms",
                table: "Announcement",
                newName: "IX_Announcement_OrganizationUnitId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsAnnouncement_BranchId",
                schema: "Hrms",
                table: "Announcement",
                newName: "IX_Announcement_BranchId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsAllowanceType_TenantId_Name",
                schema: "Hrms",
                table: "AllowanceType",
                newName: "IX_AllowanceType_TenantId_Name");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsAchievement_TenantId_EmployeeId",
                schema: "Hrms",
                table: "Achievement",
                newName: "IX_Achievement_TenantId_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsAchievement_EmployeeId",
                schema: "Hrms",
                table: "Achievement",
                newName: "IX_Achievement_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsAchievement_AppraisalId",
                schema: "Hrms",
                table: "Achievement",
                newName: "IX_Achievement_AppraisalId");

            migrationBuilder.RenameIndex(
                name: "IX_coreSubsystem_TenantId_Name",
                schema: "Core",
                table: "Subsystem",
                newName: "IX_Subsystem_TenantId_Name");

            migrationBuilder.RenameIndex(
                name: "IX_coreSalaryScale_TenantId_JobGradeId_StepId",
                schema: "Core",
                table: "SalaryScale",
                newName: "IX_SalaryScale_TenantId_JobGradeId_StepId");

            migrationBuilder.RenameIndex(
                name: "IX_coreSalaryScale_StepId",
                schema: "Core",
                table: "SalaryScale",
                newName: "IX_SalaryScale_StepId");

            migrationBuilder.RenameIndex(
                name: "IX_coreSalaryScale_JobGradeId",
                schema: "Core",
                table: "SalaryScale",
                newName: "IX_SalaryScale_JobGradeId");

            migrationBuilder.RenameIndex(
                name: "IX_CorePerson_FirstName_FatherName_GrandFatherName",
                schema: "Core",
                table: "Person",
                newName: "IX_Person_FirstName_FatherName_GrandFatherName");

            migrationBuilder.RenameIndex(
                name: "IX_coreOperation_ModuleId",
                schema: "Core",
                table: "Operation",
                newName: "IX_Operation_ModuleId");

            migrationBuilder.RenameIndex(
                name: "IX_coreModule_SubsystemId",
                schema: "Core",
                table: "Module",
                newName: "IX_Module_SubsystemId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Step",
                schema: "Core",
                table: "Step",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_WorkWeekConfiguration",
                schema: "Hrms",
                table: "WorkWeekConfiguration",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_WorkLocation",
                schema: "Hrms",
                table: "WorkLocation",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_WorkforcePlanLine",
                schema: "Hrms",
                table: "WorkforcePlanLine",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_WorkforcePlan",
                schema: "Hrms",
                table: "WorkforcePlan",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_WorkflowStepApprover",
                schema: "Hrms",
                table: "WorkflowStepApprover",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_WorkflowStep",
                schema: "Hrms",
                table: "WorkflowStep",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_WorkflowInstance",
                schema: "Hrms",
                table: "WorkflowInstance",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_WorkflowDefinition",
                schema: "Hrms",
                table: "WorkflowDefinition",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_WorkflowActionLog",
                schema: "Hrms",
                table: "WorkflowActionLog",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TripRequest",
                schema: "Hrms",
                table: "TripRequest",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TripExpense",
                schema: "Hrms",
                table: "TripExpense",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TripBudget",
                schema: "Hrms",
                table: "TripBudget",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TrainingSession",
                schema: "Hrms",
                table: "TrainingSession",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TrainingProviderPayment",
                schema: "Hrms",
                table: "TrainingProviderPayment",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TrainingNeed",
                schema: "Hrms",
                table: "TrainingNeed",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TrainingEnrollment",
                schema: "Hrms",
                table: "TrainingEnrollment",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TrainingCourse",
                schema: "Hrms",
                table: "TrainingCourse",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TrainingCategory",
                schema: "Hrms",
                table: "TrainingCategory",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TrainingBudget",
                schema: "Hrms",
                table: "TrainingBudget",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TerminationSettlement",
                schema: "Hrms",
                table: "TerminationSettlement",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TerminationClearance",
                schema: "Hrms",
                table: "TerminationClearance",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TerminationAssetRecovery",
                schema: "Hrms",
                table: "TerminationAssetRecovery",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TaxBracket",
                schema: "Hrms",
                table: "TaxBracket",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TalentReview",
                schema: "Hrms",
                table: "TalentReview",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TalentRating",
                schema: "Hrms",
                table: "TalentRating",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TalentAssessment",
                schema: "Hrms",
                table: "TalentAssessment",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SurveyResponse",
                schema: "Hrms",
                table: "SurveyResponse",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SurveyCompletion",
                schema: "Hrms",
                table: "SurveyCompletion",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Survey",
                schema: "Hrms",
                table: "Survey",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Suggestion",
                schema: "Hrms",
                table: "Suggestion",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SuccessionPlan",
                schema: "Hrms",
                table: "SuccessionPlan",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SuccessionDevelopmentAction",
                schema: "Hrms",
                table: "SuccessionDevelopmentAction",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SuccessionCandidate",
                schema: "Hrms",
                table: "SuccessionCandidate",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SettlementLine",
                schema: "Hrms",
                table: "SettlementLine",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SalaryRevisionLine",
                schema: "Hrms",
                table: "SalaryRevisionLine",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SalaryRevisionBand",
                schema: "Hrms",
                table: "SalaryRevisionBand",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SalaryRevision",
                schema: "Hrms",
                table: "SalaryRevision",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RewardPointsTransaction",
                schema: "Hrms",
                table: "RewardPointsTransaction",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RewardNomination",
                schema: "Hrms",
                table: "RewardNomination",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RewardDisbursement",
                schema: "Hrms",
                table: "RewardDisbursement",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ReviewCycle",
                schema: "Hrms",
                table: "ReviewCycle",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RequisitionScreeningCriterion",
                schema: "Hrms",
                table: "RequisitionScreeningCriterion",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ReportScheduleRecipient",
                schema: "Hrms",
                table: "ReportScheduleRecipient",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ReportScheduleFieldValue",
                schema: "Hrms",
                table: "ReportScheduleFieldValue",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ReportScheduleFieldOutput",
                schema: "Hrms",
                table: "ReportScheduleFieldOutput",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ReportSchedule",
                schema: "Hrms",
                table: "ReportSchedule",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ReportSavedFilter",
                schema: "Hrms",
                table: "ReportSavedFilter",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ReportRunRecipient",
                schema: "Hrms",
                table: "ReportRunRecipient",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ReportRun",
                schema: "Hrms",
                table: "ReportRun",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ReportRestriction",
                schema: "Hrms",
                table: "ReportRestriction",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ReportFieldOutput",
                schema: "Hrms",
                table: "ReportFieldOutput",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ReportField",
                schema: "Hrms",
                table: "ReportField",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Report",
                schema: "Hrms",
                table: "Report",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RecognitionProgram",
                schema: "Hrms",
                table: "RecognitionProgram",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RecognitionBadge",
                schema: "Hrms",
                table: "RecognitionBadge",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RatingScaleLevel",
                schema: "Hrms",
                table: "RatingScaleLevel",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RatingScale",
                schema: "Hrms",
                table: "RatingScale",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProfileChangeRequest",
                schema: "Hrms",
                table: "ProfileChangeRequest",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PositionCompetency",
                schema: "Hrms",
                table: "PositionCompetency",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PositionClass",
                schema: "Hrms",
                table: "PositionClass",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Position",
                schema: "Hrms",
                table: "Position",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PipObjective",
                schema: "Hrms",
                table: "PipObjective",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PerformanceHistory",
                schema: "Hrms",
                table: "PerformanceHistory",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PerDiemRate",
                schema: "Hrms",
                table: "PerDiemRate",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OtherLeaveSetting",
                schema: "Hrms",
                table: "OtherLeaveSetting",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OtherLeaveDetail",
                schema: "Hrms",
                table: "OtherLeaveDetail",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OtherLeave",
                schema: "Hrms",
                table: "OtherLeave",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OrganizationUnit",
                schema: "Hrms",
                table: "OrganizationUnit",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OrganizationalObjective",
                schema: "Hrms",
                table: "OrganizationalObjective",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OfferLetterTemplate",
                schema: "Hrms",
                table: "OfferLetterTemplate",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_NumberSequence",
                schema: "Hrms",
                table: "NumberSequence",
                columns: new[] { "TenantId", "Key" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Mentorship",
                schema: "Hrms",
                table: "Mentorship",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MedicalServiceContract",
                schema: "Hrms",
                table: "MedicalServiceContract",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MedicalProvider",
                schema: "Hrms",
                table: "MedicalProvider",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MedicalPlan",
                schema: "Hrms",
                table: "MedicalPlan",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MedicalEnrollment",
                schema: "Hrms",
                table: "MedicalEnrollment",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MedicalClaimAttachment",
                schema: "Hrms",
                table: "MedicalClaimAttachment",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MedicalClaim",
                schema: "Hrms",
                table: "MedicalClaim",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MedicalBeneficiary",
                schema: "Hrms",
                table: "MedicalBeneficiary",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LoanType",
                schema: "Hrms",
                table: "LoanType",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LoanRepaymentSchedule",
                schema: "Hrms",
                table: "LoanRepaymentSchedule",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LoanGuarantor",
                schema: "Hrms",
                table: "LoanGuarantor",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Loan",
                schema: "Hrms",
                table: "Loan",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LeaveType",
                schema: "Hrms",
                table: "LeaveType",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LeaveRequestLine",
                schema: "Hrms",
                table: "LeaveRequestLine",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LeaveRequest",
                schema: "Hrms",
                table: "LeaveRequest",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LeaveBalanceTransaction",
                schema: "Hrms",
                table: "LeaveBalanceTransaction",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LeaveBalance",
                schema: "Hrms",
                table: "LeaveBalance",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LearningPathStep",
                schema: "Hrms",
                table: "LearningPathStep",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LearningPath",
                schema: "Hrms",
                table: "LearningPath",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LearningCommunityPost",
                schema: "Hrms",
                table: "LearningCommunityPost",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LearningCommunityMember",
                schema: "Hrms",
                table: "LearningCommunityMember",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LearningCommunity",
                schema: "Hrms",
                table: "LearningCommunity",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_KnowledgeTransfer",
                schema: "Hrms",
                table: "KnowledgeTransfer",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_JobRequisition",
                schema: "Hrms",
                table: "JobRequisition",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_JobOffer",
                schema: "Hrms",
                table: "JobOffer",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_JobGrade",
                schema: "Hrms",
                table: "JobGrade",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_JobCategory",
                schema: "Hrms",
                table: "JobCategory",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_JobApplicationStageLog",
                schema: "Hrms",
                table: "JobApplicationStageLog",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_JobApplication",
                schema: "Hrms",
                table: "JobApplication",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_InterviewPanelist",
                schema: "Hrms",
                table: "InterviewPanelist",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_InterviewFeedback",
                schema: "Hrms",
                table: "InterviewFeedback",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Interview",
                schema: "Hrms",
                table: "Interview",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_InsurancePremiumSchedule",
                schema: "Hrms",
                table: "InsurancePremiumSchedule",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_InsurancePolicy",
                schema: "Hrms",
                table: "InsurancePolicy",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_InsuranceClaimAttachment",
                schema: "Hrms",
                table: "InsuranceClaimAttachment",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_InsuranceClaim",
                schema: "Hrms",
                table: "InsuranceClaim",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ImprovementPlan",
                schema: "Hrms",
                table: "ImprovementPlan",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Holiday",
                schema: "Hrms",
                table: "Holiday",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_HiringRequest",
                schema: "Hrms",
                table: "HiringRequest",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_GrievanceNote",
                schema: "Hrms",
                table: "GrievanceNote",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Grievance",
                schema: "Hrms",
                table: "Grievance",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_GoalActionItem",
                schema: "Hrms",
                table: "GoalActionItem",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ExitQuestionnaire",
                schema: "Hrms",
                table: "ExitQuestionnaire",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ExitInterview",
                schema: "Hrms",
                table: "ExitInterview",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EmployeeTrainingCertificate",
                schema: "Hrms",
                table: "EmployeeTrainingCertificate",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EmployeeTermination",
                schema: "Hrms",
                table: "EmployeeTermination",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EmployeeRecognition",
                schema: "Hrms",
                table: "EmployeeRecognition",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EmployeeMovement",
                schema: "Hrms",
                table: "EmployeeMovement",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EmployeeGuarantee",
                schema: "Hrms",
                table: "EmployeeGuarantee",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EmployeeGoal",
                schema: "Hrms",
                table: "EmployeeGoal",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EmployeeFieldValue",
                schema: "Hrms",
                table: "EmployeeFieldValue",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EmployeeFieldDefinition",
                schema: "Hrms",
                table: "EmployeeFieldDefinition",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EmployeeExperience",
                schema: "Hrms",
                table: "EmployeeExperience",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EmployeeEducation",
                schema: "Hrms",
                table: "EmployeeEducation",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EmployeeDocument",
                schema: "Hrms",
                table: "EmployeeDocument",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EmployeeDependent",
                schema: "Hrms",
                table: "EmployeeDependent",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EmployeeCareerPathStepProgress",
                schema: "Hrms",
                table: "EmployeeCareerPathStepProgress",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EmployeeCareerPath",
                schema: "Hrms",
                table: "EmployeeCareerPath",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EmployeeBenefitEnrollment",
                schema: "Hrms",
                table: "EmployeeBenefitEnrollment",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EmployeeAllowance",
                schema: "Hrms",
                table: "EmployeeAllowance",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Employee",
                schema: "Hrms",
                table: "Employee",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DynamicFormRecord",
                schema: "Hrms",
                table: "DynamicFormRecord",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DynamicFormField",
                schema: "Hrms",
                table: "DynamicFormField",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DynamicForm",
                schema: "Hrms",
                table: "DynamicForm",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DocumentTemplate",
                schema: "Hrms",
                table: "DocumentTemplate",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DisciplinaryMeasure",
                schema: "Hrms",
                table: "DisciplinaryMeasure",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DevelopmentPlan",
                schema: "Hrms",
                table: "DevelopmentPlan",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DevelopmentAction",
                schema: "Hrms",
                table: "DevelopmentAction",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CriticalPosition",
                schema: "Hrms",
                table: "CriticalPosition",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CriterionEvaluator",
                schema: "Hrms",
                table: "CriterionEvaluator",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CompetencyCategory",
                schema: "Hrms",
                table: "CompetencyCategory",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Competency",
                schema: "Hrms",
                table: "Competency",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CompensationRequest",
                schema: "Hrms",
                table: "CompensationRequest",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CompanyProfile",
                schema: "Hrms",
                table: "CompanyProfile",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CompanyAsset",
                schema: "Hrms",
                table: "CompanyAsset",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CommunityPostReaction",
                schema: "Hrms",
                table: "CommunityPostReaction",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ClearanceDepartmentApprover",
                schema: "Hrms",
                table: "ClearanceDepartmentApprover",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ClearanceDepartment",
                schema: "Hrms",
                table: "ClearanceDepartment",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CareerPathStepCompetency",
                schema: "Hrms",
                table: "CareerPathStepCompetency",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CareerPathStep",
                schema: "Hrms",
                table: "CareerPathStep",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CareerPathChangeRequest",
                schema: "Hrms",
                table: "CareerPathChangeRequest",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CareerPath",
                schema: "Hrms",
                table: "CareerPath",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CandidateDocument",
                schema: "Hrms",
                table: "CandidateDocument",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Candidate",
                schema: "Hrms",
                table: "Candidate",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CalibrationSession",
                schema: "Hrms",
                table: "CalibrationSession",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CalibrationItem",
                schema: "Hrms",
                table: "CalibrationItem",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Branch",
                schema: "Hrms",
                table: "Branch",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BenefitPlan",
                schema: "Hrms",
                table: "BenefitPlan",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AwardCategory",
                schema: "Hrms",
                table: "AwardCategory",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AuditLog",
                schema: "Hrms",
                table: "AuditLog",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AppraisalTemplate",
                schema: "Hrms",
                table: "AppraisalTemplate",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AppraisalPeerReview",
                schema: "Hrms",
                table: "AppraisalPeerReview",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AppraisalGoal",
                schema: "Hrms",
                table: "AppraisalGoal",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AppraisalCompetency",
                schema: "Hrms",
                table: "AppraisalCompetency",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AppraisalAppeal",
                schema: "Hrms",
                table: "AppraisalAppeal",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Appraisal",
                schema: "Hrms",
                table: "Appraisal",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ApplicationCriterionScore",
                schema: "Hrms",
                table: "ApplicationCriterionScore",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AnnualLeaveSetting",
                schema: "Hrms",
                table: "AnnualLeaveSetting",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AnnualLeaveHeader",
                schema: "Hrms",
                table: "AnnualLeaveHeader",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AnnualLeaveDetail",
                schema: "Hrms",
                table: "AnnualLeaveDetail",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Announcement",
                schema: "Hrms",
                table: "Announcement",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AllowanceType",
                schema: "Hrms",
                table: "AllowanceType",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Achievement",
                schema: "Hrms",
                table: "Achievement",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Subsystem",
                schema: "Core",
                table: "Subsystem",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SalaryScale",
                schema: "Core",
                table: "SalaryScale",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Person",
                schema: "Core",
                table: "Person",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Operation",
                schema: "Core",
                table: "Operation",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Module",
                schema: "Core",
                table: "Module",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Achievement_Appraisal_AppraisalId",
                schema: "Hrms",
                table: "Achievement",
                column: "AppraisalId",
                principalSchema: "Hrms",
                principalTable: "Appraisal",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Achievement_Employee_EmployeeId",
                schema: "Hrms",
                table: "Achievement",
                column: "EmployeeId",
                principalSchema: "Hrms",
                principalTable: "Employee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Announcement_Branch_BranchId",
                schema: "Hrms",
                table: "Announcement",
                column: "BranchId",
                principalSchema: "Hrms",
                principalTable: "Branch",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Announcement_OrganizationUnit_OrganizationUnitId",
                schema: "Hrms",
                table: "Announcement",
                column: "OrganizationUnitId",
                principalSchema: "Hrms",
                principalTable: "OrganizationUnit",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AnnualLeaveDetail_AnnualLeaveHeader_AnnualLeaveHeaderId",
                schema: "Hrms",
                table: "AnnualLeaveDetail",
                column: "AnnualLeaveHeaderId",
                principalSchema: "Hrms",
                principalTable: "AnnualLeaveHeader",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AnnualLeaveHeader_Employee_EmployeeId",
                schema: "Hrms",
                table: "AnnualLeaveHeader",
                column: "EmployeeId",
                principalSchema: "Hrms",
                principalTable: "Employee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AnnualLeaveHeader_LeaveBalance_AnnualLeaveLedgerId",
                schema: "Hrms",
                table: "AnnualLeaveHeader",
                column: "AnnualLeaveLedgerId",
                principalSchema: "Hrms",
                principalTable: "LeaveBalance",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AnnualLeaveSetting_FiscalYear_FiscalYearId",
                schema: "Hrms",
                table: "AnnualLeaveSetting",
                column: "FiscalYearId",
                principalSchema: "Core",
                principalTable: "FiscalYear",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ApplicationCriterionScore_JobApplication_ApplicationId",
                schema: "Hrms",
                table: "ApplicationCriterionScore",
                column: "ApplicationId",
                principalSchema: "Hrms",
                principalTable: "JobApplication",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Appraisal_Employee_EmployeeId",
                schema: "Hrms",
                table: "Appraisal",
                column: "EmployeeId",
                principalSchema: "Hrms",
                principalTable: "Employee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Appraisal_ReviewCycle_ReviewCycleId",
                schema: "Hrms",
                table: "Appraisal",
                column: "ReviewCycleId",
                principalSchema: "Hrms",
                principalTable: "ReviewCycle",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AppraisalAppeal_Appraisal_AppraisalId",
                schema: "Hrms",
                table: "AppraisalAppeal",
                column: "AppraisalId",
                principalSchema: "Hrms",
                principalTable: "Appraisal",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AppraisalAppeal_Employee_EmployeeId",
                schema: "Hrms",
                table: "AppraisalAppeal",
                column: "EmployeeId",
                principalSchema: "Hrms",
                principalTable: "Employee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AppraisalCompetency_Appraisal_AppraisalId",
                schema: "Hrms",
                table: "AppraisalCompetency",
                column: "AppraisalId",
                principalSchema: "Hrms",
                principalTable: "Appraisal",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AppraisalGoal_Appraisal_AppraisalId",
                schema: "Hrms",
                table: "AppraisalGoal",
                column: "AppraisalId",
                principalSchema: "Hrms",
                principalTable: "Appraisal",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AppraisalPeerReview_Appraisal_AppraisalId",
                schema: "Hrms",
                table: "AppraisalPeerReview",
                column: "AppraisalId",
                principalSchema: "Hrms",
                principalTable: "Appraisal",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AppraisalPeerReview_Employee_PeerEmployeeId",
                schema: "Hrms",
                table: "AppraisalPeerReview",
                column: "PeerEmployeeId",
                principalSchema: "Hrms",
                principalTable: "Employee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Branch_Branch_ParentId",
                schema: "Hrms",
                table: "Branch",
                column: "ParentId",
                principalSchema: "Hrms",
                principalTable: "Branch",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CalibrationItem_Appraisal_AppraisalId",
                schema: "Hrms",
                table: "CalibrationItem",
                column: "AppraisalId",
                principalSchema: "Hrms",
                principalTable: "Appraisal",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CalibrationItem_CalibrationSession_CalibrationSessionId",
                schema: "Hrms",
                table: "CalibrationItem",
                column: "CalibrationSessionId",
                principalSchema: "Hrms",
                principalTable: "CalibrationSession",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CalibrationSession_OrganizationUnit_OrganizationUnitId",
                schema: "Hrms",
                table: "CalibrationSession",
                column: "OrganizationUnitId",
                principalSchema: "Hrms",
                principalTable: "OrganizationUnit",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CalibrationSession_ReviewCycle_ReviewCycleId",
                schema: "Hrms",
                table: "CalibrationSession",
                column: "ReviewCycleId",
                principalSchema: "Hrms",
                principalTable: "ReviewCycle",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Candidate_Employee_InternalEmployeeId",
                schema: "Hrms",
                table: "Candidate",
                column: "InternalEmployeeId",
                principalSchema: "Hrms",
                principalTable: "Employee",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Candidate_Person_PersonId",
                schema: "Hrms",
                table: "Candidate",
                column: "PersonId",
                principalSchema: "Core",
                principalTable: "Person",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CandidateDocument_Candidate_CandidateId",
                schema: "Hrms",
                table: "CandidateDocument",
                column: "CandidateId",
                principalSchema: "Hrms",
                principalTable: "Candidate",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CareerPathChangeRequest_CareerPath_CurrentCareerPathId",
                schema: "Hrms",
                table: "CareerPathChangeRequest",
                column: "CurrentCareerPathId",
                principalSchema: "Hrms",
                principalTable: "CareerPath",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CareerPathChangeRequest_CareerPath_RequestedCareerPathId",
                schema: "Hrms",
                table: "CareerPathChangeRequest",
                column: "RequestedCareerPathId",
                principalSchema: "Hrms",
                principalTable: "CareerPath",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CareerPathChangeRequest_Employee_EmployeeId",
                schema: "Hrms",
                table: "CareerPathChangeRequest",
                column: "EmployeeId",
                principalSchema: "Hrms",
                principalTable: "Employee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CareerPathStep_CareerPath_CareerPathId",
                schema: "Hrms",
                table: "CareerPathStep",
                column: "CareerPathId",
                principalSchema: "Hrms",
                principalTable: "CareerPath",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CareerPathStep_JobGrade_JobGradeId",
                schema: "Hrms",
                table: "CareerPathStep",
                column: "JobGradeId",
                principalSchema: "Hrms",
                principalTable: "JobGrade",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CareerPathStep_PositionClass_PositionClassId",
                schema: "Hrms",
                table: "CareerPathStep",
                column: "PositionClassId",
                principalSchema: "Hrms",
                principalTable: "PositionClass",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CareerPathStepCompetency_CareerPathStep_CareerPathStepId",
                schema: "Hrms",
                table: "CareerPathStepCompetency",
                column: "CareerPathStepId",
                principalSchema: "Hrms",
                principalTable: "CareerPathStep",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CareerPathStepCompetency_Competency_CompetencyId",
                schema: "Hrms",
                table: "CareerPathStepCompetency",
                column: "CompetencyId",
                principalSchema: "Hrms",
                principalTable: "Competency",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ClearanceDepartmentApprover_ClearanceDepartment_DepartmentId",
                schema: "Hrms",
                table: "ClearanceDepartmentApprover",
                column: "DepartmentId",
                principalSchema: "Hrms",
                principalTable: "ClearanceDepartment",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CommunityPostReaction_Employee_EmployeeId",
                schema: "Hrms",
                table: "CommunityPostReaction",
                column: "EmployeeId",
                principalSchema: "Hrms",
                principalTable: "Employee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CommunityPostReaction_LearningCommunityPost_LearningCommunityPostId",
                schema: "Hrms",
                table: "CommunityPostReaction",
                column: "LearningCommunityPostId",
                principalSchema: "Hrms",
                principalTable: "LearningCommunityPost",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CompanyAsset_Employee_AssignedToEmployeeId",
                schema: "Hrms",
                table: "CompanyAsset",
                column: "AssignedToEmployeeId",
                principalSchema: "Hrms",
                principalTable: "Employee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CompensationRequest_Employee_EmployeeId",
                schema: "Hrms",
                table: "CompensationRequest",
                column: "EmployeeId",
                principalSchema: "Hrms",
                principalTable: "Employee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Competency_CompetencyCategory_CompetencyCategoryId",
                schema: "Hrms",
                table: "Competency",
                column: "CompetencyCategoryId",
                principalSchema: "Hrms",
                principalTable: "CompetencyCategory",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CriterionEvaluator_Employee_EmployeeId",
                schema: "Hrms",
                table: "CriterionEvaluator",
                column: "EmployeeId",
                principalSchema: "Hrms",
                principalTable: "Employee",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_CriterionEvaluator_RequisitionScreeningCriterion_CriterionId",
                schema: "Hrms",
                table: "CriterionEvaluator",
                column: "CriterionId",
                principalSchema: "Hrms",
                principalTable: "RequisitionScreeningCriterion",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CriticalPosition_Position_PositionId",
                schema: "Hrms",
                table: "CriticalPosition",
                column: "PositionId",
                principalSchema: "Hrms",
                principalTable: "Position",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DevelopmentAction_Competency_CompetencyId",
                schema: "Hrms",
                table: "DevelopmentAction",
                column: "CompetencyId",
                principalSchema: "Hrms",
                principalTable: "Competency",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DevelopmentAction_DevelopmentPlan_DevelopmentPlanId",
                schema: "Hrms",
                table: "DevelopmentAction",
                column: "DevelopmentPlanId",
                principalSchema: "Hrms",
                principalTable: "DevelopmentPlan",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DevelopmentPlan_Appraisal_AppraisalId",
                schema: "Hrms",
                table: "DevelopmentPlan",
                column: "AppraisalId",
                principalSchema: "Hrms",
                principalTable: "Appraisal",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DevelopmentPlan_Employee_EmployeeId",
                schema: "Hrms",
                table: "DevelopmentPlan",
                column: "EmployeeId",
                principalSchema: "Hrms",
                principalTable: "Employee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DisciplinaryMeasure_Employee_EmployeeId",
                schema: "Hrms",
                table: "DisciplinaryMeasure",
                column: "EmployeeId",
                principalSchema: "Hrms",
                principalTable: "Employee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DynamicFormField_DynamicForm_DynamicFormId",
                schema: "Hrms",
                table: "DynamicFormField",
                column: "DynamicFormId",
                principalSchema: "Hrms",
                principalTable: "DynamicForm",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DynamicFormRecord_DynamicForm_DynamicFormId",
                schema: "Hrms",
                table: "DynamicFormRecord",
                column: "DynamicFormId",
                principalSchema: "Hrms",
                principalTable: "DynamicForm",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Employee_Branch_BranchId",
                schema: "Hrms",
                table: "Employee",
                column: "BranchId",
                principalSchema: "Hrms",
                principalTable: "Branch",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Employee_Person_PersonId",
                schema: "Hrms",
                table: "Employee",
                column: "PersonId",
                principalSchema: "Core",
                principalTable: "Person",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Employee_Position_PositionId",
                schema: "Hrms",
                table: "Employee",
                column: "PositionId",
                principalSchema: "Hrms",
                principalTable: "Position",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Employee_SalaryScale_SalaryScaleId",
                schema: "Hrms",
                table: "Employee",
                column: "SalaryScaleId",
                principalSchema: "Core",
                principalTable: "SalaryScale",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeAllowance_AllowanceType_AllowanceTypeId",
                schema: "Hrms",
                table: "EmployeeAllowance",
                column: "AllowanceTypeId",
                principalSchema: "Hrms",
                principalTable: "AllowanceType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeAllowance_Employee_EmployeeId",
                schema: "Hrms",
                table: "EmployeeAllowance",
                column: "EmployeeId",
                principalSchema: "Hrms",
                principalTable: "Employee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeBenefitEnrollment_BenefitPlan_BenefitPlanId",
                schema: "Hrms",
                table: "EmployeeBenefitEnrollment",
                column: "BenefitPlanId",
                principalSchema: "Hrms",
                principalTable: "BenefitPlan",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeBenefitEnrollment_Employee_EmployeeId",
                schema: "Hrms",
                table: "EmployeeBenefitEnrollment",
                column: "EmployeeId",
                principalSchema: "Hrms",
                principalTable: "Employee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeCareerPath_CareerPath_CareerPathId",
                schema: "Hrms",
                table: "EmployeeCareerPath",
                column: "CareerPathId",
                principalSchema: "Hrms",
                principalTable: "CareerPath",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeCareerPath_Employee_EmployeeId",
                schema: "Hrms",
                table: "EmployeeCareerPath",
                column: "EmployeeId",
                principalSchema: "Hrms",
                principalTable: "Employee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeCareerPathStepProgress_EmployeeCareerPath_EmployeeCareerPathId",
                schema: "Hrms",
                table: "EmployeeCareerPathStepProgress",
                column: "EmployeeCareerPathId",
                principalSchema: "Hrms",
                principalTable: "EmployeeCareerPath",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeDependent_Employee_RelatedEmployeeId",
                schema: "Hrms",
                table: "EmployeeDependent",
                column: "RelatedEmployeeId",
                principalSchema: "Hrms",
                principalTable: "Employee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeDependent_Person_PersonId",
                schema: "Hrms",
                table: "EmployeeDependent",
                column: "PersonId",
                principalSchema: "Core",
                principalTable: "Person",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeEducation_Person_PersonId",
                schema: "Hrms",
                table: "EmployeeEducation",
                column: "PersonId",
                principalSchema: "Core",
                principalTable: "Person",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeExperience_Person_PersonId",
                schema: "Hrms",
                table: "EmployeeExperience",
                column: "PersonId",
                principalSchema: "Core",
                principalTable: "Person",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeFieldValue_EmployeeFieldDefinition_FieldDefinitionId",
                schema: "Hrms",
                table: "EmployeeFieldValue",
                column: "FieldDefinitionId",
                principalSchema: "Hrms",
                principalTable: "EmployeeFieldDefinition",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeGoal_Employee_EmployeeId",
                schema: "Hrms",
                table: "EmployeeGoal",
                column: "EmployeeId",
                principalSchema: "Hrms",
                principalTable: "Employee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeGoal_OrganizationalObjective_OrganizationalObjectiveId",
                schema: "Hrms",
                table: "EmployeeGoal",
                column: "OrganizationalObjectiveId",
                principalSchema: "Hrms",
                principalTable: "OrganizationalObjective",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeGoal_ReviewCycle_ReviewCycleId",
                schema: "Hrms",
                table: "EmployeeGoal",
                column: "ReviewCycleId",
                principalSchema: "Hrms",
                principalTable: "ReviewCycle",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeGuarantee_Employee_EmployeeId",
                schema: "Hrms",
                table: "EmployeeGuarantee",
                column: "EmployeeId",
                principalSchema: "Hrms",
                principalTable: "Employee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeMovement_Employee_EmployeeId",
                schema: "Hrms",
                table: "EmployeeMovement",
                column: "EmployeeId",
                principalSchema: "Hrms",
                principalTable: "Employee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeMovement_SalaryScale_ToSalaryScaleId",
                schema: "Hrms",
                table: "EmployeeMovement",
                column: "ToSalaryScaleId",
                principalSchema: "Core",
                principalTable: "SalaryScale",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeRecognition_Employee_EmployeeId",
                schema: "Hrms",
                table: "EmployeeRecognition",
                column: "EmployeeId",
                principalSchema: "Hrms",
                principalTable: "Employee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeRecognition_RecognitionBadge_RecognitionBadgeId",
                schema: "Hrms",
                table: "EmployeeRecognition",
                column: "RecognitionBadgeId",
                principalSchema: "Hrms",
                principalTable: "RecognitionBadge",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeTermination_Employee_EmployeeId",
                schema: "Hrms",
                table: "EmployeeTermination",
                column: "EmployeeId",
                principalSchema: "Hrms",
                principalTable: "Employee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeTrainingCertificate_Employee_EmployeeId",
                schema: "Hrms",
                table: "EmployeeTrainingCertificate",
                column: "EmployeeId",
                principalSchema: "Hrms",
                principalTable: "Employee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeTrainingCertificate_TrainingCourse_TrainingCourseId",
                schema: "Hrms",
                table: "EmployeeTrainingCertificate",
                column: "TrainingCourseId",
                principalSchema: "Hrms",
                principalTable: "TrainingCourse",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeTrainingCertificate_TrainingEnrollment_TrainingEnrollmentId",
                schema: "Hrms",
                table: "EmployeeTrainingCertificate",
                column: "TrainingEnrollmentId",
                principalSchema: "Hrms",
                principalTable: "TrainingEnrollment",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ExitInterview_EmployeeTermination_TerminationId",
                schema: "Hrms",
                table: "ExitInterview",
                column: "TerminationId",
                principalSchema: "Hrms",
                principalTable: "EmployeeTermination",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GoalActionItem_EmployeeGoal_EmployeeGoalId",
                schema: "Hrms",
                table: "GoalActionItem",
                column: "EmployeeGoalId",
                principalSchema: "Hrms",
                principalTable: "EmployeeGoal",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Grievance_Employee_EmployeeId",
                schema: "Hrms",
                table: "Grievance",
                column: "EmployeeId",
                principalSchema: "Hrms",
                principalTable: "Employee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GrievanceNote_Grievance_GrievanceId",
                schema: "Hrms",
                table: "GrievanceNote",
                column: "GrievanceId",
                principalSchema: "Hrms",
                principalTable: "Grievance",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_HiringRequest_OrganizationUnit_OrganizationUnitId",
                schema: "Hrms",
                table: "HiringRequest",
                column: "OrganizationUnitId",
                principalSchema: "Hrms",
                principalTable: "OrganizationUnit",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_HiringRequest_PositionClass_PositionClassId",
                schema: "Hrms",
                table: "HiringRequest",
                column: "PositionClassId",
                principalSchema: "Hrms",
                principalTable: "PositionClass",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ImprovementPlan_Appraisal_AppraisalId",
                schema: "Hrms",
                table: "ImprovementPlan",
                column: "AppraisalId",
                principalSchema: "Hrms",
                principalTable: "Appraisal",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ImprovementPlan_Employee_EmployeeId",
                schema: "Hrms",
                table: "ImprovementPlan",
                column: "EmployeeId",
                principalSchema: "Hrms",
                principalTable: "Employee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_InsuranceClaim_Employee_EmployeeId",
                schema: "Hrms",
                table: "InsuranceClaim",
                column: "EmployeeId",
                principalSchema: "Hrms",
                principalTable: "Employee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_InsuranceClaim_InsurancePolicy_InsurancePolicyId",
                schema: "Hrms",
                table: "InsuranceClaim",
                column: "InsurancePolicyId",
                principalSchema: "Hrms",
                principalTable: "InsurancePolicy",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_InsuranceClaimAttachment_InsuranceClaim_InsuranceClaimId",
                schema: "Hrms",
                table: "InsuranceClaimAttachment",
                column: "InsuranceClaimId",
                principalSchema: "Hrms",
                principalTable: "InsuranceClaim",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_InsurancePremiumSchedule_InsurancePolicy_InsurancePolicyId",
                schema: "Hrms",
                table: "InsurancePremiumSchedule",
                column: "InsurancePolicyId",
                principalSchema: "Hrms",
                principalTable: "InsurancePolicy",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Interview_JobApplication_ApplicationId",
                schema: "Hrms",
                table: "Interview",
                column: "ApplicationId",
                principalSchema: "Hrms",
                principalTable: "JobApplication",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_InterviewFeedback_InterviewPanelist_PanelistId",
                schema: "Hrms",
                table: "InterviewFeedback",
                column: "PanelistId",
                principalSchema: "Hrms",
                principalTable: "InterviewPanelist",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_InterviewPanelist_Employee_EmployeeId",
                schema: "Hrms",
                table: "InterviewPanelist",
                column: "EmployeeId",
                principalSchema: "Hrms",
                principalTable: "Employee",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_InterviewPanelist_Interview_InterviewId",
                schema: "Hrms",
                table: "InterviewPanelist",
                column: "InterviewId",
                principalSchema: "Hrms",
                principalTable: "Interview",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_JobApplication_Candidate_CandidateId",
                schema: "Hrms",
                table: "JobApplication",
                column: "CandidateId",
                principalSchema: "Hrms",
                principalTable: "Candidate",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_JobApplication_JobRequisition_RequisitionId",
                schema: "Hrms",
                table: "JobApplication",
                column: "RequisitionId",
                principalSchema: "Hrms",
                principalTable: "JobRequisition",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_JobApplicationStageLog_JobApplication_ApplicationId",
                schema: "Hrms",
                table: "JobApplicationStageLog",
                column: "ApplicationId",
                principalSchema: "Hrms",
                principalTable: "JobApplication",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_JobOffer_Employee_HiringManagerEmployeeId",
                schema: "Hrms",
                table: "JobOffer",
                column: "HiringManagerEmployeeId",
                principalSchema: "Hrms",
                principalTable: "Employee",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_JobOffer_JobApplication_ApplicationId",
                schema: "Hrms",
                table: "JobOffer",
                column: "ApplicationId",
                principalSchema: "Hrms",
                principalTable: "JobApplication",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_JobOffer_SalaryScale_SalaryScaleId",
                schema: "Hrms",
                table: "JobOffer",
                column: "SalaryScaleId",
                principalSchema: "Core",
                principalTable: "SalaryScale",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_JobRequisition_HiringRequest_HiringRequestId",
                schema: "Hrms",
                table: "JobRequisition",
                column: "HiringRequestId",
                principalSchema: "Hrms",
                principalTable: "HiringRequest",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_JobRequisition_OrganizationUnit_OrganizationUnitId",
                schema: "Hrms",
                table: "JobRequisition",
                column: "OrganizationUnitId",
                principalSchema: "Hrms",
                principalTable: "OrganizationUnit",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_JobRequisition_PositionClass_PositionClassId",
                schema: "Hrms",
                table: "JobRequisition",
                column: "PositionClassId",
                principalSchema: "Hrms",
                principalTable: "PositionClass",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_JobRequisition_SalaryScale_SalaryScaleId",
                schema: "Hrms",
                table: "JobRequisition",
                column: "SalaryScaleId",
                principalSchema: "Core",
                principalTable: "SalaryScale",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_JobRequisition_WorkLocation_WorkLocationId",
                schema: "Hrms",
                table: "JobRequisition",
                column: "WorkLocationId",
                principalSchema: "Hrms",
                principalTable: "WorkLocation",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_KnowledgeTransfer_Employee_FromEmployeeId",
                schema: "Hrms",
                table: "KnowledgeTransfer",
                column: "FromEmployeeId",
                principalSchema: "Hrms",
                principalTable: "Employee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_KnowledgeTransfer_SuccessionCandidate_SuccessionCandidateId",
                schema: "Hrms",
                table: "KnowledgeTransfer",
                column: "SuccessionCandidateId",
                principalSchema: "Hrms",
                principalTable: "SuccessionCandidate",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LearningCommunity_TrainingCourse_TrainingCourseId",
                schema: "Hrms",
                table: "LearningCommunity",
                column: "TrainingCourseId",
                principalSchema: "Hrms",
                principalTable: "TrainingCourse",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LearningCommunityMember_Employee_EmployeeId",
                schema: "Hrms",
                table: "LearningCommunityMember",
                column: "EmployeeId",
                principalSchema: "Hrms",
                principalTable: "Employee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LearningCommunityMember_LearningCommunity_LearningCommunityId",
                schema: "Hrms",
                table: "LearningCommunityMember",
                column: "LearningCommunityId",
                principalSchema: "Hrms",
                principalTable: "LearningCommunity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LearningCommunityPost_Employee_EmployeeId",
                schema: "Hrms",
                table: "LearningCommunityPost",
                column: "EmployeeId",
                principalSchema: "Hrms",
                principalTable: "Employee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LearningCommunityPost_LearningCommunity_LearningCommunityId",
                schema: "Hrms",
                table: "LearningCommunityPost",
                column: "LearningCommunityId",
                principalSchema: "Hrms",
                principalTable: "LearningCommunity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LearningPath_Position_TargetPositionId",
                schema: "Hrms",
                table: "LearningPath",
                column: "TargetPositionId",
                principalSchema: "Hrms",
                principalTable: "Position",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LearningPathStep_LearningPath_LearningPathId",
                schema: "Hrms",
                table: "LearningPathStep",
                column: "LearningPathId",
                principalSchema: "Hrms",
                principalTable: "LearningPath",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LearningPathStep_TrainingCourse_TrainingCourseId",
                schema: "Hrms",
                table: "LearningPathStep",
                column: "TrainingCourseId",
                principalSchema: "Hrms",
                principalTable: "TrainingCourse",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LeaveBalance_Employee_EmployeeId",
                schema: "Hrms",
                table: "LeaveBalance",
                column: "EmployeeId",
                principalSchema: "Hrms",
                principalTable: "Employee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LeaveBalance_FiscalYear_FiscalYearId",
                schema: "Hrms",
                table: "LeaveBalance",
                column: "FiscalYearId",
                principalSchema: "Core",
                principalTable: "FiscalYear",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LeaveBalance_LeaveType_LeaveTypeId",
                schema: "Hrms",
                table: "LeaveBalance",
                column: "LeaveTypeId",
                principalSchema: "Hrms",
                principalTable: "LeaveType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LeaveRequest_Employee_EmployeeId",
                schema: "Hrms",
                table: "LeaveRequest",
                column: "EmployeeId",
                principalSchema: "Hrms",
                principalTable: "Employee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LeaveRequest_FiscalYear_FiscalYearId",
                schema: "Hrms",
                table: "LeaveRequest",
                column: "FiscalYearId",
                principalSchema: "Core",
                principalTable: "FiscalYear",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LeaveRequestLine_LeaveRequest_LeaveRequestId",
                schema: "Hrms",
                table: "LeaveRequestLine",
                column: "LeaveRequestId",
                principalSchema: "Hrms",
                principalTable: "LeaveRequest",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LeaveRequestLine_LeaveType_LeaveTypeId",
                schema: "Hrms",
                table: "LeaveRequestLine",
                column: "LeaveTypeId",
                principalSchema: "Hrms",
                principalTable: "LeaveType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Loan_Employee_EmployeeId",
                schema: "Hrms",
                table: "Loan",
                column: "EmployeeId",
                principalSchema: "Hrms",
                principalTable: "Employee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Loan_LoanType_LoanTypeId",
                schema: "Hrms",
                table: "Loan",
                column: "LoanTypeId",
                principalSchema: "Hrms",
                principalTable: "LoanType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LoanGuarantor_Loan_LoanId",
                schema: "Hrms",
                table: "LoanGuarantor",
                column: "LoanId",
                principalSchema: "Hrms",
                principalTable: "Loan",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LoanRepaymentSchedule_Loan_LoanId",
                schema: "Hrms",
                table: "LoanRepaymentSchedule",
                column: "LoanId",
                principalSchema: "Hrms",
                principalTable: "Loan",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MedicalBeneficiary_MedicalEnrollment_MedicalEnrollmentId",
                schema: "Hrms",
                table: "MedicalBeneficiary",
                column: "MedicalEnrollmentId",
                principalSchema: "Hrms",
                principalTable: "MedicalEnrollment",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MedicalClaim_Employee_EmployeeId",
                schema: "Hrms",
                table: "MedicalClaim",
                column: "EmployeeId",
                principalSchema: "Hrms",
                principalTable: "Employee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MedicalClaim_MedicalEnrollment_MedicalEnrollmentId",
                schema: "Hrms",
                table: "MedicalClaim",
                column: "MedicalEnrollmentId",
                principalSchema: "Hrms",
                principalTable: "MedicalEnrollment",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MedicalClaimAttachment_MedicalClaim_MedicalClaimId",
                schema: "Hrms",
                table: "MedicalClaimAttachment",
                column: "MedicalClaimId",
                principalSchema: "Hrms",
                principalTable: "MedicalClaim",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MedicalEnrollment_Employee_EmployeeId",
                schema: "Hrms",
                table: "MedicalEnrollment",
                column: "EmployeeId",
                principalSchema: "Hrms",
                principalTable: "Employee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MedicalEnrollment_MedicalPlan_MedicalPlanId",
                schema: "Hrms",
                table: "MedicalEnrollment",
                column: "MedicalPlanId",
                principalSchema: "Hrms",
                principalTable: "MedicalPlan",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MedicalServiceContract_MedicalProvider_MedicalProviderId",
                schema: "Hrms",
                table: "MedicalServiceContract",
                column: "MedicalProviderId",
                principalSchema: "Hrms",
                principalTable: "MedicalProvider",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Mentorship_Employee_MenteeEmployeeId",
                schema: "Hrms",
                table: "Mentorship",
                column: "MenteeEmployeeId",
                principalSchema: "Hrms",
                principalTable: "Employee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Mentorship_Employee_MentorEmployeeId",
                schema: "Hrms",
                table: "Mentorship",
                column: "MentorEmployeeId",
                principalSchema: "Hrms",
                principalTable: "Employee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Module_Subsystem_SubsystemId",
                schema: "Core",
                table: "Module",
                column: "SubsystemId",
                principalSchema: "Core",
                principalTable: "Subsystem",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Operation_Module_ModuleId",
                schema: "Core",
                table: "Operation",
                column: "ModuleId",
                principalSchema: "Core",
                principalTable: "Module",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrganizationalObjective_OrganizationUnit_OrganizationUnitId",
                schema: "Hrms",
                table: "OrganizationalObjective",
                column: "OrganizationUnitId",
                principalSchema: "Hrms",
                principalTable: "OrganizationUnit",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrganizationalObjective_OrganizationalObjective_ParentObjectiveId",
                schema: "Hrms",
                table: "OrganizationalObjective",
                column: "ParentObjectiveId",
                principalSchema: "Hrms",
                principalTable: "OrganizationalObjective",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrganizationalObjective_ReviewCycle_ReviewCycleId",
                schema: "Hrms",
                table: "OrganizationalObjective",
                column: "ReviewCycleId",
                principalSchema: "Hrms",
                principalTable: "ReviewCycle",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrganizationUnit_Branch_BranchId",
                schema: "Hrms",
                table: "OrganizationUnit",
                column: "BranchId",
                principalSchema: "Hrms",
                principalTable: "Branch",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrganizationUnit_OrganizationUnit_ParentId",
                schema: "Hrms",
                table: "OrganizationUnit",
                column: "ParentId",
                principalSchema: "Hrms",
                principalTable: "OrganizationUnit",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrganizationUnit_WorkLocation_WorkLocationId",
                schema: "Hrms",
                table: "OrganizationUnit",
                column: "WorkLocationId",
                principalSchema: "Hrms",
                principalTable: "WorkLocation",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OtherLeave_Employee_EmployeeId",
                schema: "Hrms",
                table: "OtherLeave",
                column: "EmployeeId",
                principalSchema: "Hrms",
                principalTable: "Employee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OtherLeave_OtherLeaveSetting_OtherLeaveSettingId",
                schema: "Hrms",
                table: "OtherLeave",
                column: "OtherLeaveSettingId",
                principalSchema: "Hrms",
                principalTable: "OtherLeaveSetting",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OtherLeaveDetail_OtherLeave_OtherLeaveHeaderId",
                schema: "Hrms",
                table: "OtherLeaveDetail",
                column: "OtherLeaveHeaderId",
                principalSchema: "Hrms",
                principalTable: "OtherLeave",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OtherLeaveSetting_FiscalYear_FiscalYearId",
                schema: "Hrms",
                table: "OtherLeaveSetting",
                column: "FiscalYearId",
                principalSchema: "Core",
                principalTable: "FiscalYear",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OtherLeaveSetting_LeaveType_LeaveTypeId",
                schema: "Hrms",
                table: "OtherLeaveSetting",
                column: "LeaveTypeId",
                principalSchema: "Hrms",
                principalTable: "LeaveType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PerDiemRate_JobGrade_JobGradeId",
                schema: "Hrms",
                table: "PerDiemRate",
                column: "JobGradeId",
                principalSchema: "Hrms",
                principalTable: "JobGrade",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PipObjective_ImprovementPlan_PipId",
                schema: "Hrms",
                table: "PipObjective",
                column: "PipId",
                principalSchema: "Hrms",
                principalTable: "ImprovementPlan",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Position_Branch_BranchId",
                schema: "Hrms",
                table: "Position",
                column: "BranchId",
                principalSchema: "Hrms",
                principalTable: "Branch",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Position_OrganizationUnit_OrganizationUnitId",
                schema: "Hrms",
                table: "Position",
                column: "OrganizationUnitId",
                principalSchema: "Hrms",
                principalTable: "OrganizationUnit",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Position_PositionClass_PositionClassId",
                schema: "Hrms",
                table: "Position",
                column: "PositionClassId",
                principalSchema: "Hrms",
                principalTable: "PositionClass",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PositionClass_JobCategory_JobCategoryId",
                schema: "Hrms",
                table: "PositionClass",
                column: "JobCategoryId",
                principalSchema: "Hrms",
                principalTable: "JobCategory",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PositionClass_PositionClass_ReportsToPositionClassId",
                schema: "Hrms",
                table: "PositionClass",
                column: "ReportsToPositionClassId",
                principalSchema: "Hrms",
                principalTable: "PositionClass",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PositionClass_SalaryScale_SalaryScaleId",
                schema: "Hrms",
                table: "PositionClass",
                column: "SalaryScaleId",
                principalSchema: "Core",
                principalTable: "SalaryScale",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PositionClass_WorkLocation_WorkLocationId",
                schema: "Hrms",
                table: "PositionClass",
                column: "WorkLocationId",
                principalSchema: "Hrms",
                principalTable: "WorkLocation",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PositionCompetency_Competency_CompetencyId",
                schema: "Hrms",
                table: "PositionCompetency",
                column: "CompetencyId",
                principalSchema: "Hrms",
                principalTable: "Competency",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PositionCompetency_Position_PositionId",
                schema: "Hrms",
                table: "PositionCompetency",
                column: "PositionId",
                principalSchema: "Hrms",
                principalTable: "Position",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProfileChangeRequest_Employee_EmployeeId",
                schema: "Hrms",
                table: "ProfileChangeRequest",
                column: "EmployeeId",
                principalSchema: "Hrms",
                principalTable: "Employee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RatingScaleLevel_RatingScale_RatingScaleId",
                schema: "Hrms",
                table: "RatingScaleLevel",
                column: "RatingScaleId",
                principalSchema: "Hrms",
                principalTable: "RatingScale",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RecognitionBadge_AwardCategory_AwardCategoryId",
                schema: "Hrms",
                table: "RecognitionBadge",
                column: "AwardCategoryId",
                principalSchema: "Hrms",
                principalTable: "AwardCategory",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RecognitionProgram_RecognitionBadge_RecognitionBadgeId",
                schema: "Hrms",
                table: "RecognitionProgram",
                column: "RecognitionBadgeId",
                principalSchema: "Hrms",
                principalTable: "RecognitionBadge",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ReportField_Report_ReportId",
                schema: "Hrms",
                table: "ReportField",
                column: "ReportId",
                principalSchema: "Hrms",
                principalTable: "Report",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ReportFieldOutput_Report_ReportId",
                schema: "Hrms",
                table: "ReportFieldOutput",
                column: "ReportId",
                principalSchema: "Hrms",
                principalTable: "Report",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ReportRestriction_Report_ReportId",
                schema: "Hrms",
                table: "ReportRestriction",
                column: "ReportId",
                principalSchema: "Hrms",
                principalTable: "Report",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ReportRunRecipient_ReportRun_ReportRunId",
                schema: "Hrms",
                table: "ReportRunRecipient",
                column: "ReportRunId",
                principalSchema: "Hrms",
                principalTable: "ReportRun",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ReportSavedFilter_Report_ReportId",
                schema: "Hrms",
                table: "ReportSavedFilter",
                column: "ReportId",
                principalSchema: "Hrms",
                principalTable: "Report",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ReportSchedule_Report_ReportId",
                schema: "Hrms",
                table: "ReportSchedule",
                column: "ReportId",
                principalSchema: "Hrms",
                principalTable: "Report",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ReportScheduleFieldOutput_ReportSchedule_ReportScheduleId",
                schema: "Hrms",
                table: "ReportScheduleFieldOutput",
                column: "ReportScheduleId",
                principalSchema: "Hrms",
                principalTable: "ReportSchedule",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ReportScheduleFieldValue_ReportSchedule_ReportScheduleId",
                schema: "Hrms",
                table: "ReportScheduleFieldValue",
                column: "ReportScheduleId",
                principalSchema: "Hrms",
                principalTable: "ReportSchedule",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ReportScheduleRecipient_ReportSchedule_ReportScheduleId",
                schema: "Hrms",
                table: "ReportScheduleRecipient",
                column: "ReportScheduleId",
                principalSchema: "Hrms",
                principalTable: "ReportSchedule",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RequisitionScreeningCriterion_JobRequisition_RequisitionId",
                schema: "Hrms",
                table: "RequisitionScreeningCriterion",
                column: "RequisitionId",
                principalSchema: "Hrms",
                principalTable: "JobRequisition",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ReviewCycle_FiscalYear_FiscalYearId",
                schema: "Hrms",
                table: "ReviewCycle",
                column: "FiscalYearId",
                principalSchema: "Core",
                principalTable: "FiscalYear",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ReviewCycle_RatingScale_RatingScaleId",
                schema: "Hrms",
                table: "ReviewCycle",
                column: "RatingScaleId",
                principalSchema: "Hrms",
                principalTable: "RatingScale",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RewardDisbursement_EmployeeRecognition_EmployeeRecognitionId",
                schema: "Hrms",
                table: "RewardDisbursement",
                column: "EmployeeRecognitionId",
                principalSchema: "Hrms",
                principalTable: "EmployeeRecognition",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_RewardDisbursement_Employee_EmployeeId",
                schema: "Hrms",
                table: "RewardDisbursement",
                column: "EmployeeId",
                principalSchema: "Hrms",
                principalTable: "Employee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RewardDisbursement_RecognitionBadge_RecognitionBadgeId",
                schema: "Hrms",
                table: "RewardDisbursement",
                column: "RecognitionBadgeId",
                principalSchema: "Hrms",
                principalTable: "RecognitionBadge",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RewardNomination_Employee_NomineeEmployeeId",
                schema: "Hrms",
                table: "RewardNomination",
                column: "NomineeEmployeeId",
                principalSchema: "Hrms",
                principalTable: "Employee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RewardNomination_RecognitionBadge_RecognitionBadgeId",
                schema: "Hrms",
                table: "RewardNomination",
                column: "RecognitionBadgeId",
                principalSchema: "Hrms",
                principalTable: "RecognitionBadge",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RewardNomination_RecognitionProgram_RecognitionProgramId",
                schema: "Hrms",
                table: "RewardNomination",
                column: "RecognitionProgramId",
                principalSchema: "Hrms",
                principalTable: "RecognitionProgram",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RewardPointsTransaction_Employee_EmployeeId",
                schema: "Hrms",
                table: "RewardPointsTransaction",
                column: "EmployeeId",
                principalSchema: "Hrms",
                principalTable: "Employee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RolePermission_Operation_OperationId",
                schema: "Core",
                table: "RolePermission",
                column: "OperationId",
                principalSchema: "Core",
                principalTable: "Operation",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SalaryRevisionBand_SalaryRevision_SalaryRevisionId",
                schema: "Hrms",
                table: "SalaryRevisionBand",
                column: "SalaryRevisionId",
                principalSchema: "Hrms",
                principalTable: "SalaryRevision",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SalaryRevisionLine_Employee_EmployeeId",
                schema: "Hrms",
                table: "SalaryRevisionLine",
                column: "EmployeeId",
                principalSchema: "Hrms",
                principalTable: "Employee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SalaryRevisionLine_SalaryRevision_SalaryRevisionId",
                schema: "Hrms",
                table: "SalaryRevisionLine",
                column: "SalaryRevisionId",
                principalSchema: "Hrms",
                principalTable: "SalaryRevision",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SalaryScale_JobGrade_JobGradeId",
                schema: "Core",
                table: "SalaryScale",
                column: "JobGradeId",
                principalSchema: "Hrms",
                principalTable: "JobGrade",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SalaryScale_Step_StepId",
                schema: "Core",
                table: "SalaryScale",
                column: "StepId",
                principalSchema: "Core",
                principalTable: "Step",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SettlementLine_TerminationSettlement_TerminationSettlementId",
                schema: "Hrms",
                table: "SettlementLine",
                column: "TerminationSettlementId",
                principalSchema: "Hrms",
                principalTable: "TerminationSettlement",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SuccessionCandidate_Employee_EmployeeId",
                schema: "Hrms",
                table: "SuccessionCandidate",
                column: "EmployeeId",
                principalSchema: "Hrms",
                principalTable: "Employee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SuccessionCandidate_SuccessionPlan_SuccessionPlanId",
                schema: "Hrms",
                table: "SuccessionCandidate",
                column: "SuccessionPlanId",
                principalSchema: "Hrms",
                principalTable: "SuccessionPlan",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SuccessionDevelopmentAction_Employee_MentorEmployeeId",
                schema: "Hrms",
                table: "SuccessionDevelopmentAction",
                column: "MentorEmployeeId",
                principalSchema: "Hrms",
                principalTable: "Employee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SuccessionDevelopmentAction_SuccessionCandidate_SuccessionCandidateId",
                schema: "Hrms",
                table: "SuccessionDevelopmentAction",
                column: "SuccessionCandidateId",
                principalSchema: "Hrms",
                principalTable: "SuccessionCandidate",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SuccessionPlan_CriticalPosition_CriticalPositionId",
                schema: "Hrms",
                table: "SuccessionPlan",
                column: "CriticalPositionId",
                principalSchema: "Hrms",
                principalTable: "CriticalPosition",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SurveyCompletion_Employee_EmployeeId",
                schema: "Hrms",
                table: "SurveyCompletion",
                column: "EmployeeId",
                principalSchema: "Hrms",
                principalTable: "Employee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SurveyCompletion_Survey_SurveyId",
                schema: "Hrms",
                table: "SurveyCompletion",
                column: "SurveyId",
                principalSchema: "Hrms",
                principalTable: "Survey",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SurveyResponse_Survey_SurveyId",
                schema: "Hrms",
                table: "SurveyResponse",
                column: "SurveyId",
                principalSchema: "Hrms",
                principalTable: "Survey",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TalentAssessment_Employee_EmployeeId",
                schema: "Hrms",
                table: "TalentAssessment",
                column: "EmployeeId",
                principalSchema: "Hrms",
                principalTable: "Employee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TalentAssessment_TalentReview_TalentReviewId",
                schema: "Hrms",
                table: "TalentAssessment",
                column: "TalentReviewId",
                principalSchema: "Hrms",
                principalTable: "TalentReview",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TalentRating_Employee_RaterEmployeeId",
                schema: "Hrms",
                table: "TalentRating",
                column: "RaterEmployeeId",
                principalSchema: "Hrms",
                principalTable: "Employee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TalentRating_TalentAssessment_TalentAssessmentId",
                schema: "Hrms",
                table: "TalentRating",
                column: "TalentAssessmentId",
                principalSchema: "Hrms",
                principalTable: "TalentAssessment",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TalentReview_OrganizationUnit_OrganizationUnitId",
                schema: "Hrms",
                table: "TalentReview",
                column: "OrganizationUnitId",
                principalSchema: "Hrms",
                principalTable: "OrganizationUnit",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TerminationAssetRecovery_CompanyAsset_CompanyAssetId",
                schema: "Hrms",
                table: "TerminationAssetRecovery",
                column: "CompanyAssetId",
                principalSchema: "Hrms",
                principalTable: "CompanyAsset",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TerminationAssetRecovery_EmployeeTermination_TerminationId",
                schema: "Hrms",
                table: "TerminationAssetRecovery",
                column: "TerminationId",
                principalSchema: "Hrms",
                principalTable: "EmployeeTermination",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TerminationClearance_ClearanceDepartment_DepartmentId",
                schema: "Hrms",
                table: "TerminationClearance",
                column: "DepartmentId",
                principalSchema: "Hrms",
                principalTable: "ClearanceDepartment",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_TerminationClearance_EmployeeTermination_TerminationId",
                schema: "Hrms",
                table: "TerminationClearance",
                column: "TerminationId",
                principalSchema: "Hrms",
                principalTable: "EmployeeTermination",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TerminationSettlement_EmployeeTermination_TerminationId",
                schema: "Hrms",
                table: "TerminationSettlement",
                column: "TerminationId",
                principalSchema: "Hrms",
                principalTable: "EmployeeTermination",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TrainingBudget_OrganizationUnit_OrganizationUnitId",
                schema: "Hrms",
                table: "TrainingBudget",
                column: "OrganizationUnitId",
                principalSchema: "Hrms",
                principalTable: "OrganizationUnit",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TrainingCourse_TrainingCategory_TrainingCategoryId",
                schema: "Hrms",
                table: "TrainingCourse",
                column: "TrainingCategoryId",
                principalSchema: "Hrms",
                principalTable: "TrainingCategory",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TrainingEnrollment_Employee_EmployeeId",
                schema: "Hrms",
                table: "TrainingEnrollment",
                column: "EmployeeId",
                principalSchema: "Hrms",
                principalTable: "Employee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TrainingEnrollment_TrainingNeed_TrainingNeedId",
                schema: "Hrms",
                table: "TrainingEnrollment",
                column: "TrainingNeedId",
                principalSchema: "Hrms",
                principalTable: "TrainingNeed",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_TrainingEnrollment_TrainingSession_TrainingSessionId",
                schema: "Hrms",
                table: "TrainingEnrollment",
                column: "TrainingSessionId",
                principalSchema: "Hrms",
                principalTable: "TrainingSession",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TrainingNeed_Competency_CompetencyId",
                schema: "Hrms",
                table: "TrainingNeed",
                column: "CompetencyId",
                principalSchema: "Hrms",
                principalTable: "Competency",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TrainingNeed_Employee_EmployeeId",
                schema: "Hrms",
                table: "TrainingNeed",
                column: "EmployeeId",
                principalSchema: "Hrms",
                principalTable: "Employee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TrainingNeed_TrainingCourse_TrainingCourseId",
                schema: "Hrms",
                table: "TrainingNeed",
                column: "TrainingCourseId",
                principalSchema: "Hrms",
                principalTable: "TrainingCourse",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TrainingProviderPayment_TrainingSession_TrainingSessionId",
                schema: "Hrms",
                table: "TrainingProviderPayment",
                column: "TrainingSessionId",
                principalSchema: "Hrms",
                principalTable: "TrainingSession",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_TrainingSession_TrainingCourse_TrainingCourseId",
                schema: "Hrms",
                table: "TrainingSession",
                column: "TrainingCourseId",
                principalSchema: "Hrms",
                principalTable: "TrainingCourse",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TripBudget_OrganizationUnit_OrganizationUnitId",
                schema: "Hrms",
                table: "TripBudget",
                column: "OrganizationUnitId",
                principalSchema: "Hrms",
                principalTable: "OrganizationUnit",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TripExpense_TripRequest_TripRequestId",
                schema: "Hrms",
                table: "TripExpense",
                column: "TripRequestId",
                principalSchema: "Hrms",
                principalTable: "TripRequest",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TripRequest_Employee_EmployeeId",
                schema: "Hrms",
                table: "TripRequest",
                column: "EmployeeId",
                principalSchema: "Hrms",
                principalTable: "Employee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TripRequest_TripBudget_TripBudgetId",
                schema: "Hrms",
                table: "TripRequest",
                column: "TripBudgetId",
                principalSchema: "Hrms",
                principalTable: "TripBudget",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_User_Employee_EmployeeId",
                schema: "Core",
                table: "User",
                column: "EmployeeId",
                principalSchema: "Hrms",
                principalTable: "Employee",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkflowActionLog_WorkflowInstance_InstanceId",
                schema: "Hrms",
                table: "WorkflowActionLog",
                column: "InstanceId",
                principalSchema: "Hrms",
                principalTable: "WorkflowInstance",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkflowInstance_WorkflowDefinition_DefinitionId",
                schema: "Hrms",
                table: "WorkflowInstance",
                column: "DefinitionId",
                principalSchema: "Hrms",
                principalTable: "WorkflowDefinition",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkflowStep_WorkflowDefinition_DefinitionId",
                schema: "Hrms",
                table: "WorkflowStep",
                column: "DefinitionId",
                principalSchema: "Hrms",
                principalTable: "WorkflowDefinition",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkflowStepApprover_WorkflowStep_StepId",
                schema: "Hrms",
                table: "WorkflowStepApprover",
                column: "StepId",
                principalSchema: "Hrms",
                principalTable: "WorkflowStep",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkforcePlan_FiscalYear_StartFiscalYearId",
                schema: "Hrms",
                table: "WorkforcePlan",
                column: "StartFiscalYearId",
                principalSchema: "Core",
                principalTable: "FiscalYear",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkforcePlan_OrganizationUnit_OrganizationUnitId",
                schema: "Hrms",
                table: "WorkforcePlan",
                column: "OrganizationUnitId",
                principalSchema: "Hrms",
                principalTable: "OrganizationUnit",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkforcePlanLine_OrganizationUnit_OrganizationUnitId",
                schema: "Hrms",
                table: "WorkforcePlanLine",
                column: "OrganizationUnitId",
                principalSchema: "Hrms",
                principalTable: "OrganizationUnit",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkforcePlanLine_PositionClass_PositionClassId",
                schema: "Hrms",
                table: "WorkforcePlanLine",
                column: "PositionClassId",
                principalSchema: "Hrms",
                principalTable: "PositionClass",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkforcePlanLine_WorkforcePlan_PlanId",
                schema: "Hrms",
                table: "WorkforcePlanLine",
                column: "PlanId",
                principalSchema: "Hrms",
                principalTable: "WorkforcePlan",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkLocation_WorkLocation_ParentId",
                schema: "Hrms",
                table: "WorkLocation",
                column: "ParentId",
                principalSchema: "Hrms",
                principalTable: "WorkLocation",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        
            UpdateProcedures(migrationBuilder);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Achievement_Appraisal_AppraisalId",
                schema: "Hrms",
                table: "Achievement");

            migrationBuilder.DropForeignKey(
                name: "FK_Achievement_Employee_EmployeeId",
                schema: "Hrms",
                table: "Achievement");

            migrationBuilder.DropForeignKey(
                name: "FK_Announcement_Branch_BranchId",
                schema: "Hrms",
                table: "Announcement");

            migrationBuilder.DropForeignKey(
                name: "FK_Announcement_OrganizationUnit_OrganizationUnitId",
                schema: "Hrms",
                table: "Announcement");

            migrationBuilder.DropForeignKey(
                name: "FK_AnnualLeaveDetail_AnnualLeaveHeader_AnnualLeaveHeaderId",
                schema: "Hrms",
                table: "AnnualLeaveDetail");

            migrationBuilder.DropForeignKey(
                name: "FK_AnnualLeaveHeader_Employee_EmployeeId",
                schema: "Hrms",
                table: "AnnualLeaveHeader");

            migrationBuilder.DropForeignKey(
                name: "FK_AnnualLeaveHeader_LeaveBalance_AnnualLeaveLedgerId",
                schema: "Hrms",
                table: "AnnualLeaveHeader");

            migrationBuilder.DropForeignKey(
                name: "FK_AnnualLeaveSetting_FiscalYear_FiscalYearId",
                schema: "Hrms",
                table: "AnnualLeaveSetting");

            migrationBuilder.DropForeignKey(
                name: "FK_ApplicationCriterionScore_JobApplication_ApplicationId",
                schema: "Hrms",
                table: "ApplicationCriterionScore");

            migrationBuilder.DropForeignKey(
                name: "FK_Appraisal_Employee_EmployeeId",
                schema: "Hrms",
                table: "Appraisal");

            migrationBuilder.DropForeignKey(
                name: "FK_Appraisal_ReviewCycle_ReviewCycleId",
                schema: "Hrms",
                table: "Appraisal");

            migrationBuilder.DropForeignKey(
                name: "FK_AppraisalAppeal_Appraisal_AppraisalId",
                schema: "Hrms",
                table: "AppraisalAppeal");

            migrationBuilder.DropForeignKey(
                name: "FK_AppraisalAppeal_Employee_EmployeeId",
                schema: "Hrms",
                table: "AppraisalAppeal");

            migrationBuilder.DropForeignKey(
                name: "FK_AppraisalCompetency_Appraisal_AppraisalId",
                schema: "Hrms",
                table: "AppraisalCompetency");

            migrationBuilder.DropForeignKey(
                name: "FK_AppraisalGoal_Appraisal_AppraisalId",
                schema: "Hrms",
                table: "AppraisalGoal");

            migrationBuilder.DropForeignKey(
                name: "FK_AppraisalPeerReview_Appraisal_AppraisalId",
                schema: "Hrms",
                table: "AppraisalPeerReview");

            migrationBuilder.DropForeignKey(
                name: "FK_AppraisalPeerReview_Employee_PeerEmployeeId",
                schema: "Hrms",
                table: "AppraisalPeerReview");

            migrationBuilder.DropForeignKey(
                name: "FK_Branch_Branch_ParentId",
                schema: "Hrms",
                table: "Branch");

            migrationBuilder.DropForeignKey(
                name: "FK_CalibrationItem_Appraisal_AppraisalId",
                schema: "Hrms",
                table: "CalibrationItem");

            migrationBuilder.DropForeignKey(
                name: "FK_CalibrationItem_CalibrationSession_CalibrationSessionId",
                schema: "Hrms",
                table: "CalibrationItem");

            migrationBuilder.DropForeignKey(
                name: "FK_CalibrationSession_OrganizationUnit_OrganizationUnitId",
                schema: "Hrms",
                table: "CalibrationSession");

            migrationBuilder.DropForeignKey(
                name: "FK_CalibrationSession_ReviewCycle_ReviewCycleId",
                schema: "Hrms",
                table: "CalibrationSession");

            migrationBuilder.DropForeignKey(
                name: "FK_Candidate_Employee_InternalEmployeeId",
                schema: "Hrms",
                table: "Candidate");

            migrationBuilder.DropForeignKey(
                name: "FK_Candidate_Person_PersonId",
                schema: "Hrms",
                table: "Candidate");

            migrationBuilder.DropForeignKey(
                name: "FK_CandidateDocument_Candidate_CandidateId",
                schema: "Hrms",
                table: "CandidateDocument");

            migrationBuilder.DropForeignKey(
                name: "FK_CareerPathChangeRequest_CareerPath_CurrentCareerPathId",
                schema: "Hrms",
                table: "CareerPathChangeRequest");

            migrationBuilder.DropForeignKey(
                name: "FK_CareerPathChangeRequest_CareerPath_RequestedCareerPathId",
                schema: "Hrms",
                table: "CareerPathChangeRequest");

            migrationBuilder.DropForeignKey(
                name: "FK_CareerPathChangeRequest_Employee_EmployeeId",
                schema: "Hrms",
                table: "CareerPathChangeRequest");

            migrationBuilder.DropForeignKey(
                name: "FK_CareerPathStep_CareerPath_CareerPathId",
                schema: "Hrms",
                table: "CareerPathStep");

            migrationBuilder.DropForeignKey(
                name: "FK_CareerPathStep_JobGrade_JobGradeId",
                schema: "Hrms",
                table: "CareerPathStep");

            migrationBuilder.DropForeignKey(
                name: "FK_CareerPathStep_PositionClass_PositionClassId",
                schema: "Hrms",
                table: "CareerPathStep");

            migrationBuilder.DropForeignKey(
                name: "FK_CareerPathStepCompetency_CareerPathStep_CareerPathStepId",
                schema: "Hrms",
                table: "CareerPathStepCompetency");

            migrationBuilder.DropForeignKey(
                name: "FK_CareerPathStepCompetency_Competency_CompetencyId",
                schema: "Hrms",
                table: "CareerPathStepCompetency");

            migrationBuilder.DropForeignKey(
                name: "FK_ClearanceDepartmentApprover_ClearanceDepartment_DepartmentId",
                schema: "Hrms",
                table: "ClearanceDepartmentApprover");

            migrationBuilder.DropForeignKey(
                name: "FK_CommunityPostReaction_Employee_EmployeeId",
                schema: "Hrms",
                table: "CommunityPostReaction");

            migrationBuilder.DropForeignKey(
                name: "FK_CommunityPostReaction_LearningCommunityPost_LearningCommunityPostId",
                schema: "Hrms",
                table: "CommunityPostReaction");

            migrationBuilder.DropForeignKey(
                name: "FK_CompanyAsset_Employee_AssignedToEmployeeId",
                schema: "Hrms",
                table: "CompanyAsset");

            migrationBuilder.DropForeignKey(
                name: "FK_CompensationRequest_Employee_EmployeeId",
                schema: "Hrms",
                table: "CompensationRequest");

            migrationBuilder.DropForeignKey(
                name: "FK_Competency_CompetencyCategory_CompetencyCategoryId",
                schema: "Hrms",
                table: "Competency");

            migrationBuilder.DropForeignKey(
                name: "FK_CriterionEvaluator_Employee_EmployeeId",
                schema: "Hrms",
                table: "CriterionEvaluator");

            migrationBuilder.DropForeignKey(
                name: "FK_CriterionEvaluator_RequisitionScreeningCriterion_CriterionId",
                schema: "Hrms",
                table: "CriterionEvaluator");

            migrationBuilder.DropForeignKey(
                name: "FK_CriticalPosition_Position_PositionId",
                schema: "Hrms",
                table: "CriticalPosition");

            migrationBuilder.DropForeignKey(
                name: "FK_DevelopmentAction_Competency_CompetencyId",
                schema: "Hrms",
                table: "DevelopmentAction");

            migrationBuilder.DropForeignKey(
                name: "FK_DevelopmentAction_DevelopmentPlan_DevelopmentPlanId",
                schema: "Hrms",
                table: "DevelopmentAction");

            migrationBuilder.DropForeignKey(
                name: "FK_DevelopmentPlan_Appraisal_AppraisalId",
                schema: "Hrms",
                table: "DevelopmentPlan");

            migrationBuilder.DropForeignKey(
                name: "FK_DevelopmentPlan_Employee_EmployeeId",
                schema: "Hrms",
                table: "DevelopmentPlan");

            migrationBuilder.DropForeignKey(
                name: "FK_DisciplinaryMeasure_Employee_EmployeeId",
                schema: "Hrms",
                table: "DisciplinaryMeasure");

            migrationBuilder.DropForeignKey(
                name: "FK_DynamicFormField_DynamicForm_DynamicFormId",
                schema: "Hrms",
                table: "DynamicFormField");

            migrationBuilder.DropForeignKey(
                name: "FK_DynamicFormRecord_DynamicForm_DynamicFormId",
                schema: "Hrms",
                table: "DynamicFormRecord");

            migrationBuilder.DropForeignKey(
                name: "FK_Employee_Branch_BranchId",
                schema: "Hrms",
                table: "Employee");

            migrationBuilder.DropForeignKey(
                name: "FK_Employee_Person_PersonId",
                schema: "Hrms",
                table: "Employee");

            migrationBuilder.DropForeignKey(
                name: "FK_Employee_Position_PositionId",
                schema: "Hrms",
                table: "Employee");

            migrationBuilder.DropForeignKey(
                name: "FK_Employee_SalaryScale_SalaryScaleId",
                schema: "Hrms",
                table: "Employee");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeAllowance_AllowanceType_AllowanceTypeId",
                schema: "Hrms",
                table: "EmployeeAllowance");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeAllowance_Employee_EmployeeId",
                schema: "Hrms",
                table: "EmployeeAllowance");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeBenefitEnrollment_BenefitPlan_BenefitPlanId",
                schema: "Hrms",
                table: "EmployeeBenefitEnrollment");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeBenefitEnrollment_Employee_EmployeeId",
                schema: "Hrms",
                table: "EmployeeBenefitEnrollment");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeCareerPath_CareerPath_CareerPathId",
                schema: "Hrms",
                table: "EmployeeCareerPath");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeCareerPath_Employee_EmployeeId",
                schema: "Hrms",
                table: "EmployeeCareerPath");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeCareerPathStepProgress_EmployeeCareerPath_EmployeeCareerPathId",
                schema: "Hrms",
                table: "EmployeeCareerPathStepProgress");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeDependent_Employee_RelatedEmployeeId",
                schema: "Hrms",
                table: "EmployeeDependent");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeDependent_Person_PersonId",
                schema: "Hrms",
                table: "EmployeeDependent");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeEducation_Person_PersonId",
                schema: "Hrms",
                table: "EmployeeEducation");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeExperience_Person_PersonId",
                schema: "Hrms",
                table: "EmployeeExperience");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeFieldValue_EmployeeFieldDefinition_FieldDefinitionId",
                schema: "Hrms",
                table: "EmployeeFieldValue");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeGoal_Employee_EmployeeId",
                schema: "Hrms",
                table: "EmployeeGoal");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeGoal_OrganizationalObjective_OrganizationalObjectiveId",
                schema: "Hrms",
                table: "EmployeeGoal");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeGoal_ReviewCycle_ReviewCycleId",
                schema: "Hrms",
                table: "EmployeeGoal");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeGuarantee_Employee_EmployeeId",
                schema: "Hrms",
                table: "EmployeeGuarantee");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeMovement_Employee_EmployeeId",
                schema: "Hrms",
                table: "EmployeeMovement");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeMovement_SalaryScale_ToSalaryScaleId",
                schema: "Hrms",
                table: "EmployeeMovement");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeRecognition_Employee_EmployeeId",
                schema: "Hrms",
                table: "EmployeeRecognition");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeRecognition_RecognitionBadge_RecognitionBadgeId",
                schema: "Hrms",
                table: "EmployeeRecognition");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeTermination_Employee_EmployeeId",
                schema: "Hrms",
                table: "EmployeeTermination");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeTrainingCertificate_Employee_EmployeeId",
                schema: "Hrms",
                table: "EmployeeTrainingCertificate");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeTrainingCertificate_TrainingCourse_TrainingCourseId",
                schema: "Hrms",
                table: "EmployeeTrainingCertificate");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeTrainingCertificate_TrainingEnrollment_TrainingEnrollmentId",
                schema: "Hrms",
                table: "EmployeeTrainingCertificate");

            migrationBuilder.DropForeignKey(
                name: "FK_ExitInterview_EmployeeTermination_TerminationId",
                schema: "Hrms",
                table: "ExitInterview");

            migrationBuilder.DropForeignKey(
                name: "FK_GoalActionItem_EmployeeGoal_EmployeeGoalId",
                schema: "Hrms",
                table: "GoalActionItem");

            migrationBuilder.DropForeignKey(
                name: "FK_Grievance_Employee_EmployeeId",
                schema: "Hrms",
                table: "Grievance");

            migrationBuilder.DropForeignKey(
                name: "FK_GrievanceNote_Grievance_GrievanceId",
                schema: "Hrms",
                table: "GrievanceNote");

            migrationBuilder.DropForeignKey(
                name: "FK_HiringRequest_OrganizationUnit_OrganizationUnitId",
                schema: "Hrms",
                table: "HiringRequest");

            migrationBuilder.DropForeignKey(
                name: "FK_HiringRequest_PositionClass_PositionClassId",
                schema: "Hrms",
                table: "HiringRequest");

            migrationBuilder.DropForeignKey(
                name: "FK_ImprovementPlan_Appraisal_AppraisalId",
                schema: "Hrms",
                table: "ImprovementPlan");

            migrationBuilder.DropForeignKey(
                name: "FK_ImprovementPlan_Employee_EmployeeId",
                schema: "Hrms",
                table: "ImprovementPlan");

            migrationBuilder.DropForeignKey(
                name: "FK_InsuranceClaim_Employee_EmployeeId",
                schema: "Hrms",
                table: "InsuranceClaim");

            migrationBuilder.DropForeignKey(
                name: "FK_InsuranceClaim_InsurancePolicy_InsurancePolicyId",
                schema: "Hrms",
                table: "InsuranceClaim");

            migrationBuilder.DropForeignKey(
                name: "FK_InsuranceClaimAttachment_InsuranceClaim_InsuranceClaimId",
                schema: "Hrms",
                table: "InsuranceClaimAttachment");

            migrationBuilder.DropForeignKey(
                name: "FK_InsurancePremiumSchedule_InsurancePolicy_InsurancePolicyId",
                schema: "Hrms",
                table: "InsurancePremiumSchedule");

            migrationBuilder.DropForeignKey(
                name: "FK_Interview_JobApplication_ApplicationId",
                schema: "Hrms",
                table: "Interview");

            migrationBuilder.DropForeignKey(
                name: "FK_InterviewFeedback_InterviewPanelist_PanelistId",
                schema: "Hrms",
                table: "InterviewFeedback");

            migrationBuilder.DropForeignKey(
                name: "FK_InterviewPanelist_Employee_EmployeeId",
                schema: "Hrms",
                table: "InterviewPanelist");

            migrationBuilder.DropForeignKey(
                name: "FK_InterviewPanelist_Interview_InterviewId",
                schema: "Hrms",
                table: "InterviewPanelist");

            migrationBuilder.DropForeignKey(
                name: "FK_JobApplication_Candidate_CandidateId",
                schema: "Hrms",
                table: "JobApplication");

            migrationBuilder.DropForeignKey(
                name: "FK_JobApplication_JobRequisition_RequisitionId",
                schema: "Hrms",
                table: "JobApplication");

            migrationBuilder.DropForeignKey(
                name: "FK_JobApplicationStageLog_JobApplication_ApplicationId",
                schema: "Hrms",
                table: "JobApplicationStageLog");

            migrationBuilder.DropForeignKey(
                name: "FK_JobOffer_Employee_HiringManagerEmployeeId",
                schema: "Hrms",
                table: "JobOffer");

            migrationBuilder.DropForeignKey(
                name: "FK_JobOffer_JobApplication_ApplicationId",
                schema: "Hrms",
                table: "JobOffer");

            migrationBuilder.DropForeignKey(
                name: "FK_JobOffer_SalaryScale_SalaryScaleId",
                schema: "Hrms",
                table: "JobOffer");

            migrationBuilder.DropForeignKey(
                name: "FK_JobRequisition_HiringRequest_HiringRequestId",
                schema: "Hrms",
                table: "JobRequisition");

            migrationBuilder.DropForeignKey(
                name: "FK_JobRequisition_OrganizationUnit_OrganizationUnitId",
                schema: "Hrms",
                table: "JobRequisition");

            migrationBuilder.DropForeignKey(
                name: "FK_JobRequisition_PositionClass_PositionClassId",
                schema: "Hrms",
                table: "JobRequisition");

            migrationBuilder.DropForeignKey(
                name: "FK_JobRequisition_SalaryScale_SalaryScaleId",
                schema: "Hrms",
                table: "JobRequisition");

            migrationBuilder.DropForeignKey(
                name: "FK_JobRequisition_WorkLocation_WorkLocationId",
                schema: "Hrms",
                table: "JobRequisition");

            migrationBuilder.DropForeignKey(
                name: "FK_KnowledgeTransfer_Employee_FromEmployeeId",
                schema: "Hrms",
                table: "KnowledgeTransfer");

            migrationBuilder.DropForeignKey(
                name: "FK_KnowledgeTransfer_SuccessionCandidate_SuccessionCandidateId",
                schema: "Hrms",
                table: "KnowledgeTransfer");

            migrationBuilder.DropForeignKey(
                name: "FK_LearningCommunity_TrainingCourse_TrainingCourseId",
                schema: "Hrms",
                table: "LearningCommunity");

            migrationBuilder.DropForeignKey(
                name: "FK_LearningCommunityMember_Employee_EmployeeId",
                schema: "Hrms",
                table: "LearningCommunityMember");

            migrationBuilder.DropForeignKey(
                name: "FK_LearningCommunityMember_LearningCommunity_LearningCommunityId",
                schema: "Hrms",
                table: "LearningCommunityMember");

            migrationBuilder.DropForeignKey(
                name: "FK_LearningCommunityPost_Employee_EmployeeId",
                schema: "Hrms",
                table: "LearningCommunityPost");

            migrationBuilder.DropForeignKey(
                name: "FK_LearningCommunityPost_LearningCommunity_LearningCommunityId",
                schema: "Hrms",
                table: "LearningCommunityPost");

            migrationBuilder.DropForeignKey(
                name: "FK_LearningPath_Position_TargetPositionId",
                schema: "Hrms",
                table: "LearningPath");

            migrationBuilder.DropForeignKey(
                name: "FK_LearningPathStep_LearningPath_LearningPathId",
                schema: "Hrms",
                table: "LearningPathStep");

            migrationBuilder.DropForeignKey(
                name: "FK_LearningPathStep_TrainingCourse_TrainingCourseId",
                schema: "Hrms",
                table: "LearningPathStep");

            migrationBuilder.DropForeignKey(
                name: "FK_LeaveBalance_Employee_EmployeeId",
                schema: "Hrms",
                table: "LeaveBalance");

            migrationBuilder.DropForeignKey(
                name: "FK_LeaveBalance_FiscalYear_FiscalYearId",
                schema: "Hrms",
                table: "LeaveBalance");

            migrationBuilder.DropForeignKey(
                name: "FK_LeaveBalance_LeaveType_LeaveTypeId",
                schema: "Hrms",
                table: "LeaveBalance");

            migrationBuilder.DropForeignKey(
                name: "FK_LeaveRequest_Employee_EmployeeId",
                schema: "Hrms",
                table: "LeaveRequest");

            migrationBuilder.DropForeignKey(
                name: "FK_LeaveRequest_FiscalYear_FiscalYearId",
                schema: "Hrms",
                table: "LeaveRequest");

            migrationBuilder.DropForeignKey(
                name: "FK_LeaveRequestLine_LeaveRequest_LeaveRequestId",
                schema: "Hrms",
                table: "LeaveRequestLine");

            migrationBuilder.DropForeignKey(
                name: "FK_LeaveRequestLine_LeaveType_LeaveTypeId",
                schema: "Hrms",
                table: "LeaveRequestLine");

            migrationBuilder.DropForeignKey(
                name: "FK_Loan_Employee_EmployeeId",
                schema: "Hrms",
                table: "Loan");

            migrationBuilder.DropForeignKey(
                name: "FK_Loan_LoanType_LoanTypeId",
                schema: "Hrms",
                table: "Loan");

            migrationBuilder.DropForeignKey(
                name: "FK_LoanGuarantor_Loan_LoanId",
                schema: "Hrms",
                table: "LoanGuarantor");

            migrationBuilder.DropForeignKey(
                name: "FK_LoanRepaymentSchedule_Loan_LoanId",
                schema: "Hrms",
                table: "LoanRepaymentSchedule");

            migrationBuilder.DropForeignKey(
                name: "FK_MedicalBeneficiary_MedicalEnrollment_MedicalEnrollmentId",
                schema: "Hrms",
                table: "MedicalBeneficiary");

            migrationBuilder.DropForeignKey(
                name: "FK_MedicalClaim_Employee_EmployeeId",
                schema: "Hrms",
                table: "MedicalClaim");

            migrationBuilder.DropForeignKey(
                name: "FK_MedicalClaim_MedicalEnrollment_MedicalEnrollmentId",
                schema: "Hrms",
                table: "MedicalClaim");

            migrationBuilder.DropForeignKey(
                name: "FK_MedicalClaimAttachment_MedicalClaim_MedicalClaimId",
                schema: "Hrms",
                table: "MedicalClaimAttachment");

            migrationBuilder.DropForeignKey(
                name: "FK_MedicalEnrollment_Employee_EmployeeId",
                schema: "Hrms",
                table: "MedicalEnrollment");

            migrationBuilder.DropForeignKey(
                name: "FK_MedicalEnrollment_MedicalPlan_MedicalPlanId",
                schema: "Hrms",
                table: "MedicalEnrollment");

            migrationBuilder.DropForeignKey(
                name: "FK_MedicalServiceContract_MedicalProvider_MedicalProviderId",
                schema: "Hrms",
                table: "MedicalServiceContract");

            migrationBuilder.DropForeignKey(
                name: "FK_Mentorship_Employee_MenteeEmployeeId",
                schema: "Hrms",
                table: "Mentorship");

            migrationBuilder.DropForeignKey(
                name: "FK_Mentorship_Employee_MentorEmployeeId",
                schema: "Hrms",
                table: "Mentorship");

            migrationBuilder.DropForeignKey(
                name: "FK_Module_Subsystem_SubsystemId",
                schema: "Core",
                table: "Module");

            migrationBuilder.DropForeignKey(
                name: "FK_Operation_Module_ModuleId",
                schema: "Core",
                table: "Operation");

            migrationBuilder.DropForeignKey(
                name: "FK_OrganizationalObjective_OrganizationUnit_OrganizationUnitId",
                schema: "Hrms",
                table: "OrganizationalObjective");

            migrationBuilder.DropForeignKey(
                name: "FK_OrganizationalObjective_OrganizationalObjective_ParentObjectiveId",
                schema: "Hrms",
                table: "OrganizationalObjective");

            migrationBuilder.DropForeignKey(
                name: "FK_OrganizationalObjective_ReviewCycle_ReviewCycleId",
                schema: "Hrms",
                table: "OrganizationalObjective");

            migrationBuilder.DropForeignKey(
                name: "FK_OrganizationUnit_Branch_BranchId",
                schema: "Hrms",
                table: "OrganizationUnit");

            migrationBuilder.DropForeignKey(
                name: "FK_OrganizationUnit_OrganizationUnit_ParentId",
                schema: "Hrms",
                table: "OrganizationUnit");

            migrationBuilder.DropForeignKey(
                name: "FK_OrganizationUnit_WorkLocation_WorkLocationId",
                schema: "Hrms",
                table: "OrganizationUnit");

            migrationBuilder.DropForeignKey(
                name: "FK_OtherLeave_Employee_EmployeeId",
                schema: "Hrms",
                table: "OtherLeave");

            migrationBuilder.DropForeignKey(
                name: "FK_OtherLeave_OtherLeaveSetting_OtherLeaveSettingId",
                schema: "Hrms",
                table: "OtherLeave");

            migrationBuilder.DropForeignKey(
                name: "FK_OtherLeaveDetail_OtherLeave_OtherLeaveHeaderId",
                schema: "Hrms",
                table: "OtherLeaveDetail");

            migrationBuilder.DropForeignKey(
                name: "FK_OtherLeaveSetting_FiscalYear_FiscalYearId",
                schema: "Hrms",
                table: "OtherLeaveSetting");

            migrationBuilder.DropForeignKey(
                name: "FK_OtherLeaveSetting_LeaveType_LeaveTypeId",
                schema: "Hrms",
                table: "OtherLeaveSetting");

            migrationBuilder.DropForeignKey(
                name: "FK_PerDiemRate_JobGrade_JobGradeId",
                schema: "Hrms",
                table: "PerDiemRate");

            migrationBuilder.DropForeignKey(
                name: "FK_PipObjective_ImprovementPlan_PipId",
                schema: "Hrms",
                table: "PipObjective");

            migrationBuilder.DropForeignKey(
                name: "FK_Position_Branch_BranchId",
                schema: "Hrms",
                table: "Position");

            migrationBuilder.DropForeignKey(
                name: "FK_Position_OrganizationUnit_OrganizationUnitId",
                schema: "Hrms",
                table: "Position");

            migrationBuilder.DropForeignKey(
                name: "FK_Position_PositionClass_PositionClassId",
                schema: "Hrms",
                table: "Position");

            migrationBuilder.DropForeignKey(
                name: "FK_PositionClass_JobCategory_JobCategoryId",
                schema: "Hrms",
                table: "PositionClass");

            migrationBuilder.DropForeignKey(
                name: "FK_PositionClass_PositionClass_ReportsToPositionClassId",
                schema: "Hrms",
                table: "PositionClass");

            migrationBuilder.DropForeignKey(
                name: "FK_PositionClass_SalaryScale_SalaryScaleId",
                schema: "Hrms",
                table: "PositionClass");

            migrationBuilder.DropForeignKey(
                name: "FK_PositionClass_WorkLocation_WorkLocationId",
                schema: "Hrms",
                table: "PositionClass");

            migrationBuilder.DropForeignKey(
                name: "FK_PositionCompetency_Competency_CompetencyId",
                schema: "Hrms",
                table: "PositionCompetency");

            migrationBuilder.DropForeignKey(
                name: "FK_PositionCompetency_Position_PositionId",
                schema: "Hrms",
                table: "PositionCompetency");

            migrationBuilder.DropForeignKey(
                name: "FK_ProfileChangeRequest_Employee_EmployeeId",
                schema: "Hrms",
                table: "ProfileChangeRequest");

            migrationBuilder.DropForeignKey(
                name: "FK_RatingScaleLevel_RatingScale_RatingScaleId",
                schema: "Hrms",
                table: "RatingScaleLevel");

            migrationBuilder.DropForeignKey(
                name: "FK_RecognitionBadge_AwardCategory_AwardCategoryId",
                schema: "Hrms",
                table: "RecognitionBadge");

            migrationBuilder.DropForeignKey(
                name: "FK_RecognitionProgram_RecognitionBadge_RecognitionBadgeId",
                schema: "Hrms",
                table: "RecognitionProgram");

            migrationBuilder.DropForeignKey(
                name: "FK_ReportField_Report_ReportId",
                schema: "Hrms",
                table: "ReportField");

            migrationBuilder.DropForeignKey(
                name: "FK_ReportFieldOutput_Report_ReportId",
                schema: "Hrms",
                table: "ReportFieldOutput");

            migrationBuilder.DropForeignKey(
                name: "FK_ReportRestriction_Report_ReportId",
                schema: "Hrms",
                table: "ReportRestriction");

            migrationBuilder.DropForeignKey(
                name: "FK_ReportRunRecipient_ReportRun_ReportRunId",
                schema: "Hrms",
                table: "ReportRunRecipient");

            migrationBuilder.DropForeignKey(
                name: "FK_ReportSavedFilter_Report_ReportId",
                schema: "Hrms",
                table: "ReportSavedFilter");

            migrationBuilder.DropForeignKey(
                name: "FK_ReportSchedule_Report_ReportId",
                schema: "Hrms",
                table: "ReportSchedule");

            migrationBuilder.DropForeignKey(
                name: "FK_ReportScheduleFieldOutput_ReportSchedule_ReportScheduleId",
                schema: "Hrms",
                table: "ReportScheduleFieldOutput");

            migrationBuilder.DropForeignKey(
                name: "FK_ReportScheduleFieldValue_ReportSchedule_ReportScheduleId",
                schema: "Hrms",
                table: "ReportScheduleFieldValue");

            migrationBuilder.DropForeignKey(
                name: "FK_ReportScheduleRecipient_ReportSchedule_ReportScheduleId",
                schema: "Hrms",
                table: "ReportScheduleRecipient");

            migrationBuilder.DropForeignKey(
                name: "FK_RequisitionScreeningCriterion_JobRequisition_RequisitionId",
                schema: "Hrms",
                table: "RequisitionScreeningCriterion");

            migrationBuilder.DropForeignKey(
                name: "FK_ReviewCycle_FiscalYear_FiscalYearId",
                schema: "Hrms",
                table: "ReviewCycle");

            migrationBuilder.DropForeignKey(
                name: "FK_ReviewCycle_RatingScale_RatingScaleId",
                schema: "Hrms",
                table: "ReviewCycle");

            migrationBuilder.DropForeignKey(
                name: "FK_RewardDisbursement_EmployeeRecognition_EmployeeRecognitionId",
                schema: "Hrms",
                table: "RewardDisbursement");

            migrationBuilder.DropForeignKey(
                name: "FK_RewardDisbursement_Employee_EmployeeId",
                schema: "Hrms",
                table: "RewardDisbursement");

            migrationBuilder.DropForeignKey(
                name: "FK_RewardDisbursement_RecognitionBadge_RecognitionBadgeId",
                schema: "Hrms",
                table: "RewardDisbursement");

            migrationBuilder.DropForeignKey(
                name: "FK_RewardNomination_Employee_NomineeEmployeeId",
                schema: "Hrms",
                table: "RewardNomination");

            migrationBuilder.DropForeignKey(
                name: "FK_RewardNomination_RecognitionBadge_RecognitionBadgeId",
                schema: "Hrms",
                table: "RewardNomination");

            migrationBuilder.DropForeignKey(
                name: "FK_RewardNomination_RecognitionProgram_RecognitionProgramId",
                schema: "Hrms",
                table: "RewardNomination");

            migrationBuilder.DropForeignKey(
                name: "FK_RewardPointsTransaction_Employee_EmployeeId",
                schema: "Hrms",
                table: "RewardPointsTransaction");

            migrationBuilder.DropForeignKey(
                name: "FK_RolePermission_Operation_OperationId",
                schema: "Core",
                table: "RolePermission");

            migrationBuilder.DropForeignKey(
                name: "FK_SalaryRevisionBand_SalaryRevision_SalaryRevisionId",
                schema: "Hrms",
                table: "SalaryRevisionBand");

            migrationBuilder.DropForeignKey(
                name: "FK_SalaryRevisionLine_Employee_EmployeeId",
                schema: "Hrms",
                table: "SalaryRevisionLine");

            migrationBuilder.DropForeignKey(
                name: "FK_SalaryRevisionLine_SalaryRevision_SalaryRevisionId",
                schema: "Hrms",
                table: "SalaryRevisionLine");

            migrationBuilder.DropForeignKey(
                name: "FK_SalaryScale_JobGrade_JobGradeId",
                schema: "Core",
                table: "SalaryScale");

            migrationBuilder.DropForeignKey(
                name: "FK_SalaryScale_Step_StepId",
                schema: "Core",
                table: "SalaryScale");

            migrationBuilder.DropForeignKey(
                name: "FK_SettlementLine_TerminationSettlement_TerminationSettlementId",
                schema: "Hrms",
                table: "SettlementLine");

            migrationBuilder.DropForeignKey(
                name: "FK_SuccessionCandidate_Employee_EmployeeId",
                schema: "Hrms",
                table: "SuccessionCandidate");

            migrationBuilder.DropForeignKey(
                name: "FK_SuccessionCandidate_SuccessionPlan_SuccessionPlanId",
                schema: "Hrms",
                table: "SuccessionCandidate");

            migrationBuilder.DropForeignKey(
                name: "FK_SuccessionDevelopmentAction_Employee_MentorEmployeeId",
                schema: "Hrms",
                table: "SuccessionDevelopmentAction");

            migrationBuilder.DropForeignKey(
                name: "FK_SuccessionDevelopmentAction_SuccessionCandidate_SuccessionCandidateId",
                schema: "Hrms",
                table: "SuccessionDevelopmentAction");

            migrationBuilder.DropForeignKey(
                name: "FK_SuccessionPlan_CriticalPosition_CriticalPositionId",
                schema: "Hrms",
                table: "SuccessionPlan");

            migrationBuilder.DropForeignKey(
                name: "FK_SurveyCompletion_Employee_EmployeeId",
                schema: "Hrms",
                table: "SurveyCompletion");

            migrationBuilder.DropForeignKey(
                name: "FK_SurveyCompletion_Survey_SurveyId",
                schema: "Hrms",
                table: "SurveyCompletion");

            migrationBuilder.DropForeignKey(
                name: "FK_SurveyResponse_Survey_SurveyId",
                schema: "Hrms",
                table: "SurveyResponse");

            migrationBuilder.DropForeignKey(
                name: "FK_TalentAssessment_Employee_EmployeeId",
                schema: "Hrms",
                table: "TalentAssessment");

            migrationBuilder.DropForeignKey(
                name: "FK_TalentAssessment_TalentReview_TalentReviewId",
                schema: "Hrms",
                table: "TalentAssessment");

            migrationBuilder.DropForeignKey(
                name: "FK_TalentRating_Employee_RaterEmployeeId",
                schema: "Hrms",
                table: "TalentRating");

            migrationBuilder.DropForeignKey(
                name: "FK_TalentRating_TalentAssessment_TalentAssessmentId",
                schema: "Hrms",
                table: "TalentRating");

            migrationBuilder.DropForeignKey(
                name: "FK_TalentReview_OrganizationUnit_OrganizationUnitId",
                schema: "Hrms",
                table: "TalentReview");

            migrationBuilder.DropForeignKey(
                name: "FK_TerminationAssetRecovery_CompanyAsset_CompanyAssetId",
                schema: "Hrms",
                table: "TerminationAssetRecovery");

            migrationBuilder.DropForeignKey(
                name: "FK_TerminationAssetRecovery_EmployeeTermination_TerminationId",
                schema: "Hrms",
                table: "TerminationAssetRecovery");

            migrationBuilder.DropForeignKey(
                name: "FK_TerminationClearance_ClearanceDepartment_DepartmentId",
                schema: "Hrms",
                table: "TerminationClearance");

            migrationBuilder.DropForeignKey(
                name: "FK_TerminationClearance_EmployeeTermination_TerminationId",
                schema: "Hrms",
                table: "TerminationClearance");

            migrationBuilder.DropForeignKey(
                name: "FK_TerminationSettlement_EmployeeTermination_TerminationId",
                schema: "Hrms",
                table: "TerminationSettlement");

            migrationBuilder.DropForeignKey(
                name: "FK_TrainingBudget_OrganizationUnit_OrganizationUnitId",
                schema: "Hrms",
                table: "TrainingBudget");

            migrationBuilder.DropForeignKey(
                name: "FK_TrainingCourse_TrainingCategory_TrainingCategoryId",
                schema: "Hrms",
                table: "TrainingCourse");

            migrationBuilder.DropForeignKey(
                name: "FK_TrainingEnrollment_Employee_EmployeeId",
                schema: "Hrms",
                table: "TrainingEnrollment");

            migrationBuilder.DropForeignKey(
                name: "FK_TrainingEnrollment_TrainingNeed_TrainingNeedId",
                schema: "Hrms",
                table: "TrainingEnrollment");

            migrationBuilder.DropForeignKey(
                name: "FK_TrainingEnrollment_TrainingSession_TrainingSessionId",
                schema: "Hrms",
                table: "TrainingEnrollment");

            migrationBuilder.DropForeignKey(
                name: "FK_TrainingNeed_Competency_CompetencyId",
                schema: "Hrms",
                table: "TrainingNeed");

            migrationBuilder.DropForeignKey(
                name: "FK_TrainingNeed_Employee_EmployeeId",
                schema: "Hrms",
                table: "TrainingNeed");

            migrationBuilder.DropForeignKey(
                name: "FK_TrainingNeed_TrainingCourse_TrainingCourseId",
                schema: "Hrms",
                table: "TrainingNeed");

            migrationBuilder.DropForeignKey(
                name: "FK_TrainingProviderPayment_TrainingSession_TrainingSessionId",
                schema: "Hrms",
                table: "TrainingProviderPayment");

            migrationBuilder.DropForeignKey(
                name: "FK_TrainingSession_TrainingCourse_TrainingCourseId",
                schema: "Hrms",
                table: "TrainingSession");

            migrationBuilder.DropForeignKey(
                name: "FK_TripBudget_OrganizationUnit_OrganizationUnitId",
                schema: "Hrms",
                table: "TripBudget");

            migrationBuilder.DropForeignKey(
                name: "FK_TripExpense_TripRequest_TripRequestId",
                schema: "Hrms",
                table: "TripExpense");

            migrationBuilder.DropForeignKey(
                name: "FK_TripRequest_Employee_EmployeeId",
                schema: "Hrms",
                table: "TripRequest");

            migrationBuilder.DropForeignKey(
                name: "FK_TripRequest_TripBudget_TripBudgetId",
                schema: "Hrms",
                table: "TripRequest");

            migrationBuilder.DropForeignKey(
                name: "FK_User_Employee_EmployeeId",
                schema: "Core",
                table: "User");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkflowActionLog_WorkflowInstance_InstanceId",
                schema: "Hrms",
                table: "WorkflowActionLog");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkflowInstance_WorkflowDefinition_DefinitionId",
                schema: "Hrms",
                table: "WorkflowInstance");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkflowStep_WorkflowDefinition_DefinitionId",
                schema: "Hrms",
                table: "WorkflowStep");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkflowStepApprover_WorkflowStep_StepId",
                schema: "Hrms",
                table: "WorkflowStepApprover");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkforcePlan_FiscalYear_StartFiscalYearId",
                schema: "Hrms",
                table: "WorkforcePlan");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkforcePlan_OrganizationUnit_OrganizationUnitId",
                schema: "Hrms",
                table: "WorkforcePlan");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkforcePlanLine_OrganizationUnit_OrganizationUnitId",
                schema: "Hrms",
                table: "WorkforcePlanLine");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkforcePlanLine_PositionClass_PositionClassId",
                schema: "Hrms",
                table: "WorkforcePlanLine");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkforcePlanLine_WorkforcePlan_PlanId",
                schema: "Hrms",
                table: "WorkforcePlanLine");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkLocation_WorkLocation_ParentId",
                schema: "Hrms",
                table: "WorkLocation");

            migrationBuilder.DropPrimaryKey(
                name: "PK_WorkWeekConfiguration",
                schema: "Hrms",
                table: "WorkWeekConfiguration");

            migrationBuilder.DropPrimaryKey(
                name: "PK_WorkLocation",
                schema: "Hrms",
                table: "WorkLocation");

            migrationBuilder.DropPrimaryKey(
                name: "PK_WorkforcePlanLine",
                schema: "Hrms",
                table: "WorkforcePlanLine");

            migrationBuilder.DropPrimaryKey(
                name: "PK_WorkforcePlan",
                schema: "Hrms",
                table: "WorkforcePlan");

            migrationBuilder.DropPrimaryKey(
                name: "PK_WorkflowStepApprover",
                schema: "Hrms",
                table: "WorkflowStepApprover");

            migrationBuilder.DropPrimaryKey(
                name: "PK_WorkflowStep",
                schema: "Hrms",
                table: "WorkflowStep");

            migrationBuilder.DropPrimaryKey(
                name: "PK_WorkflowInstance",
                schema: "Hrms",
                table: "WorkflowInstance");

            migrationBuilder.DropPrimaryKey(
                name: "PK_WorkflowDefinition",
                schema: "Hrms",
                table: "WorkflowDefinition");

            migrationBuilder.DropPrimaryKey(
                name: "PK_WorkflowActionLog",
                schema: "Hrms",
                table: "WorkflowActionLog");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TripRequest",
                schema: "Hrms",
                table: "TripRequest");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TripExpense",
                schema: "Hrms",
                table: "TripExpense");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TripBudget",
                schema: "Hrms",
                table: "TripBudget");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TrainingSession",
                schema: "Hrms",
                table: "TrainingSession");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TrainingProviderPayment",
                schema: "Hrms",
                table: "TrainingProviderPayment");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TrainingNeed",
                schema: "Hrms",
                table: "TrainingNeed");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TrainingEnrollment",
                schema: "Hrms",
                table: "TrainingEnrollment");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TrainingCourse",
                schema: "Hrms",
                table: "TrainingCourse");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TrainingCategory",
                schema: "Hrms",
                table: "TrainingCategory");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TrainingBudget",
                schema: "Hrms",
                table: "TrainingBudget");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TerminationSettlement",
                schema: "Hrms",
                table: "TerminationSettlement");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TerminationClearance",
                schema: "Hrms",
                table: "TerminationClearance");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TerminationAssetRecovery",
                schema: "Hrms",
                table: "TerminationAssetRecovery");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TaxBracket",
                schema: "Hrms",
                table: "TaxBracket");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TalentReview",
                schema: "Hrms",
                table: "TalentReview");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TalentRating",
                schema: "Hrms",
                table: "TalentRating");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TalentAssessment",
                schema: "Hrms",
                table: "TalentAssessment");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SurveyResponse",
                schema: "Hrms",
                table: "SurveyResponse");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SurveyCompletion",
                schema: "Hrms",
                table: "SurveyCompletion");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Survey",
                schema: "Hrms",
                table: "Survey");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Suggestion",
                schema: "Hrms",
                table: "Suggestion");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SuccessionPlan",
                schema: "Hrms",
                table: "SuccessionPlan");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SuccessionDevelopmentAction",
                schema: "Hrms",
                table: "SuccessionDevelopmentAction");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SuccessionCandidate",
                schema: "Hrms",
                table: "SuccessionCandidate");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Subsystem",
                schema: "Core",
                table: "Subsystem");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Step",
                schema: "Core",
                table: "Step");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SettlementLine",
                schema: "Hrms",
                table: "SettlementLine");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SalaryScale",
                schema: "Core",
                table: "SalaryScale");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SalaryRevisionLine",
                schema: "Hrms",
                table: "SalaryRevisionLine");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SalaryRevisionBand",
                schema: "Hrms",
                table: "SalaryRevisionBand");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SalaryRevision",
                schema: "Hrms",
                table: "SalaryRevision");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RewardPointsTransaction",
                schema: "Hrms",
                table: "RewardPointsTransaction");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RewardNomination",
                schema: "Hrms",
                table: "RewardNomination");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RewardDisbursement",
                schema: "Hrms",
                table: "RewardDisbursement");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ReviewCycle",
                schema: "Hrms",
                table: "ReviewCycle");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RequisitionScreeningCriterion",
                schema: "Hrms",
                table: "RequisitionScreeningCriterion");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ReportScheduleRecipient",
                schema: "Hrms",
                table: "ReportScheduleRecipient");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ReportScheduleFieldValue",
                schema: "Hrms",
                table: "ReportScheduleFieldValue");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ReportScheduleFieldOutput",
                schema: "Hrms",
                table: "ReportScheduleFieldOutput");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ReportSchedule",
                schema: "Hrms",
                table: "ReportSchedule");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ReportSavedFilter",
                schema: "Hrms",
                table: "ReportSavedFilter");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ReportRunRecipient",
                schema: "Hrms",
                table: "ReportRunRecipient");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ReportRun",
                schema: "Hrms",
                table: "ReportRun");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ReportRestriction",
                schema: "Hrms",
                table: "ReportRestriction");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ReportFieldOutput",
                schema: "Hrms",
                table: "ReportFieldOutput");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ReportField",
                schema: "Hrms",
                table: "ReportField");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Report",
                schema: "Hrms",
                table: "Report");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RecognitionProgram",
                schema: "Hrms",
                table: "RecognitionProgram");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RecognitionBadge",
                schema: "Hrms",
                table: "RecognitionBadge");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RatingScaleLevel",
                schema: "Hrms",
                table: "RatingScaleLevel");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RatingScale",
                schema: "Hrms",
                table: "RatingScale");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProfileChangeRequest",
                schema: "Hrms",
                table: "ProfileChangeRequest");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PositionCompetency",
                schema: "Hrms",
                table: "PositionCompetency");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PositionClass",
                schema: "Hrms",
                table: "PositionClass");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Position",
                schema: "Hrms",
                table: "Position");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PipObjective",
                schema: "Hrms",
                table: "PipObjective");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Person",
                schema: "Core",
                table: "Person");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PerformanceHistory",
                schema: "Hrms",
                table: "PerformanceHistory");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PerDiemRate",
                schema: "Hrms",
                table: "PerDiemRate");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OtherLeaveSetting",
                schema: "Hrms",
                table: "OtherLeaveSetting");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OtherLeaveDetail",
                schema: "Hrms",
                table: "OtherLeaveDetail");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OtherLeave",
                schema: "Hrms",
                table: "OtherLeave");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OrganizationUnit",
                schema: "Hrms",
                table: "OrganizationUnit");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OrganizationalObjective",
                schema: "Hrms",
                table: "OrganizationalObjective");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Operation",
                schema: "Core",
                table: "Operation");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OfferLetterTemplate",
                schema: "Hrms",
                table: "OfferLetterTemplate");

            migrationBuilder.DropPrimaryKey(
                name: "PK_NumberSequence",
                schema: "Hrms",
                table: "NumberSequence");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Module",
                schema: "Core",
                table: "Module");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Mentorship",
                schema: "Hrms",
                table: "Mentorship");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MedicalServiceContract",
                schema: "Hrms",
                table: "MedicalServiceContract");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MedicalProvider",
                schema: "Hrms",
                table: "MedicalProvider");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MedicalPlan",
                schema: "Hrms",
                table: "MedicalPlan");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MedicalEnrollment",
                schema: "Hrms",
                table: "MedicalEnrollment");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MedicalClaimAttachment",
                schema: "Hrms",
                table: "MedicalClaimAttachment");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MedicalClaim",
                schema: "Hrms",
                table: "MedicalClaim");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MedicalBeneficiary",
                schema: "Hrms",
                table: "MedicalBeneficiary");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LoanType",
                schema: "Hrms",
                table: "LoanType");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LoanRepaymentSchedule",
                schema: "Hrms",
                table: "LoanRepaymentSchedule");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LoanGuarantor",
                schema: "Hrms",
                table: "LoanGuarantor");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Loan",
                schema: "Hrms",
                table: "Loan");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LeaveType",
                schema: "Hrms",
                table: "LeaveType");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LeaveRequestLine",
                schema: "Hrms",
                table: "LeaveRequestLine");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LeaveRequest",
                schema: "Hrms",
                table: "LeaveRequest");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LeaveBalanceTransaction",
                schema: "Hrms",
                table: "LeaveBalanceTransaction");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LeaveBalance",
                schema: "Hrms",
                table: "LeaveBalance");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LearningPathStep",
                schema: "Hrms",
                table: "LearningPathStep");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LearningPath",
                schema: "Hrms",
                table: "LearningPath");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LearningCommunityPost",
                schema: "Hrms",
                table: "LearningCommunityPost");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LearningCommunityMember",
                schema: "Hrms",
                table: "LearningCommunityMember");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LearningCommunity",
                schema: "Hrms",
                table: "LearningCommunity");

            migrationBuilder.DropPrimaryKey(
                name: "PK_KnowledgeTransfer",
                schema: "Hrms",
                table: "KnowledgeTransfer");

            migrationBuilder.DropPrimaryKey(
                name: "PK_JobRequisition",
                schema: "Hrms",
                table: "JobRequisition");

            migrationBuilder.DropPrimaryKey(
                name: "PK_JobOffer",
                schema: "Hrms",
                table: "JobOffer");

            migrationBuilder.DropPrimaryKey(
                name: "PK_JobGrade",
                schema: "Hrms",
                table: "JobGrade");

            migrationBuilder.DropPrimaryKey(
                name: "PK_JobCategory",
                schema: "Hrms",
                table: "JobCategory");

            migrationBuilder.DropPrimaryKey(
                name: "PK_JobApplicationStageLog",
                schema: "Hrms",
                table: "JobApplicationStageLog");

            migrationBuilder.DropPrimaryKey(
                name: "PK_JobApplication",
                schema: "Hrms",
                table: "JobApplication");

            migrationBuilder.DropPrimaryKey(
                name: "PK_InterviewPanelist",
                schema: "Hrms",
                table: "InterviewPanelist");

            migrationBuilder.DropPrimaryKey(
                name: "PK_InterviewFeedback",
                schema: "Hrms",
                table: "InterviewFeedback");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Interview",
                schema: "Hrms",
                table: "Interview");

            migrationBuilder.DropPrimaryKey(
                name: "PK_InsurancePremiumSchedule",
                schema: "Hrms",
                table: "InsurancePremiumSchedule");

            migrationBuilder.DropPrimaryKey(
                name: "PK_InsurancePolicy",
                schema: "Hrms",
                table: "InsurancePolicy");

            migrationBuilder.DropPrimaryKey(
                name: "PK_InsuranceClaimAttachment",
                schema: "Hrms",
                table: "InsuranceClaimAttachment");

            migrationBuilder.DropPrimaryKey(
                name: "PK_InsuranceClaim",
                schema: "Hrms",
                table: "InsuranceClaim");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ImprovementPlan",
                schema: "Hrms",
                table: "ImprovementPlan");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Holiday",
                schema: "Hrms",
                table: "Holiday");

            migrationBuilder.DropPrimaryKey(
                name: "PK_HiringRequest",
                schema: "Hrms",
                table: "HiringRequest");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GrievanceNote",
                schema: "Hrms",
                table: "GrievanceNote");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Grievance",
                schema: "Hrms",
                table: "Grievance");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GoalActionItem",
                schema: "Hrms",
                table: "GoalActionItem");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ExitQuestionnaire",
                schema: "Hrms",
                table: "ExitQuestionnaire");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ExitInterview",
                schema: "Hrms",
                table: "ExitInterview");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EmployeeTrainingCertificate",
                schema: "Hrms",
                table: "EmployeeTrainingCertificate");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EmployeeTermination",
                schema: "Hrms",
                table: "EmployeeTermination");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EmployeeRecognition",
                schema: "Hrms",
                table: "EmployeeRecognition");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EmployeeMovement",
                schema: "Hrms",
                table: "EmployeeMovement");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EmployeeGuarantee",
                schema: "Hrms",
                table: "EmployeeGuarantee");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EmployeeGoal",
                schema: "Hrms",
                table: "EmployeeGoal");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EmployeeFieldValue",
                schema: "Hrms",
                table: "EmployeeFieldValue");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EmployeeFieldDefinition",
                schema: "Hrms",
                table: "EmployeeFieldDefinition");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EmployeeExperience",
                schema: "Hrms",
                table: "EmployeeExperience");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EmployeeEducation",
                schema: "Hrms",
                table: "EmployeeEducation");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EmployeeDocument",
                schema: "Hrms",
                table: "EmployeeDocument");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EmployeeDependent",
                schema: "Hrms",
                table: "EmployeeDependent");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EmployeeCareerPathStepProgress",
                schema: "Hrms",
                table: "EmployeeCareerPathStepProgress");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EmployeeCareerPath",
                schema: "Hrms",
                table: "EmployeeCareerPath");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EmployeeBenefitEnrollment",
                schema: "Hrms",
                table: "EmployeeBenefitEnrollment");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EmployeeAllowance",
                schema: "Hrms",
                table: "EmployeeAllowance");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Employee",
                schema: "Hrms",
                table: "Employee");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DynamicFormRecord",
                schema: "Hrms",
                table: "DynamicFormRecord");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DynamicFormField",
                schema: "Hrms",
                table: "DynamicFormField");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DynamicForm",
                schema: "Hrms",
                table: "DynamicForm");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DocumentTemplate",
                schema: "Hrms",
                table: "DocumentTemplate");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DisciplinaryMeasure",
                schema: "Hrms",
                table: "DisciplinaryMeasure");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DevelopmentPlan",
                schema: "Hrms",
                table: "DevelopmentPlan");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DevelopmentAction",
                schema: "Hrms",
                table: "DevelopmentAction");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CriticalPosition",
                schema: "Hrms",
                table: "CriticalPosition");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CriterionEvaluator",
                schema: "Hrms",
                table: "CriterionEvaluator");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CompetencyCategory",
                schema: "Hrms",
                table: "CompetencyCategory");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Competency",
                schema: "Hrms",
                table: "Competency");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CompensationRequest",
                schema: "Hrms",
                table: "CompensationRequest");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CompanyProfile",
                schema: "Hrms",
                table: "CompanyProfile");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CompanyAsset",
                schema: "Hrms",
                table: "CompanyAsset");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CommunityPostReaction",
                schema: "Hrms",
                table: "CommunityPostReaction");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ClearanceDepartmentApprover",
                schema: "Hrms",
                table: "ClearanceDepartmentApprover");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ClearanceDepartment",
                schema: "Hrms",
                table: "ClearanceDepartment");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CareerPathStepCompetency",
                schema: "Hrms",
                table: "CareerPathStepCompetency");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CareerPathStep",
                schema: "Hrms",
                table: "CareerPathStep");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CareerPathChangeRequest",
                schema: "Hrms",
                table: "CareerPathChangeRequest");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CareerPath",
                schema: "Hrms",
                table: "CareerPath");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CandidateDocument",
                schema: "Hrms",
                table: "CandidateDocument");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Candidate",
                schema: "Hrms",
                table: "Candidate");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CalibrationSession",
                schema: "Hrms",
                table: "CalibrationSession");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CalibrationItem",
                schema: "Hrms",
                table: "CalibrationItem");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Branch",
                schema: "Hrms",
                table: "Branch");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BenefitPlan",
                schema: "Hrms",
                table: "BenefitPlan");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AwardCategory",
                schema: "Hrms",
                table: "AwardCategory");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AuditLog",
                schema: "Hrms",
                table: "AuditLog");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AppraisalTemplate",
                schema: "Hrms",
                table: "AppraisalTemplate");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AppraisalPeerReview",
                schema: "Hrms",
                table: "AppraisalPeerReview");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AppraisalGoal",
                schema: "Hrms",
                table: "AppraisalGoal");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AppraisalCompetency",
                schema: "Hrms",
                table: "AppraisalCompetency");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AppraisalAppeal",
                schema: "Hrms",
                table: "AppraisalAppeal");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Appraisal",
                schema: "Hrms",
                table: "Appraisal");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ApplicationCriterionScore",
                schema: "Hrms",
                table: "ApplicationCriterionScore");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AnnualLeaveSetting",
                schema: "Hrms",
                table: "AnnualLeaveSetting");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AnnualLeaveHeader",
                schema: "Hrms",
                table: "AnnualLeaveHeader");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AnnualLeaveDetail",
                schema: "Hrms",
                table: "AnnualLeaveDetail");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Announcement",
                schema: "Hrms",
                table: "Announcement");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AllowanceType",
                schema: "Hrms",
                table: "AllowanceType");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Achievement",
                schema: "Hrms",
                table: "Achievement");

            migrationBuilder.EnsureSchema(
                name: "dbo");

            migrationBuilder.RenameTable(
                name: "WorkWeekConfiguration",
                schema: "Hrms",
                newName: "hrmsWorkWeekConfiguration",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "WorkLocation",
                schema: "Hrms",
                newName: "hrmsWorkLocation",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "WorkforcePlanLine",
                schema: "Hrms",
                newName: "hrmsWorkforcePlanLine",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "WorkforcePlan",
                schema: "Hrms",
                newName: "hrmsWorkforcePlan",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "WorkflowStepApprover",
                schema: "Hrms",
                newName: "hrmsWorkflowStepApprover",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "WorkflowStep",
                schema: "Hrms",
                newName: "hrmsWorkflowStep",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "WorkflowInstance",
                schema: "Hrms",
                newName: "hrmsWorkflowInstance",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "WorkflowDefinition",
                schema: "Hrms",
                newName: "hrmsWorkflowDefinition",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "WorkflowActionLog",
                schema: "Hrms",
                newName: "hrmsWorkflowActionLog",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "TripRequest",
                schema: "Hrms",
                newName: "hrmsTripRequest",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "TripExpense",
                schema: "Hrms",
                newName: "hrmsTripExpense",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "TripBudget",
                schema: "Hrms",
                newName: "hrmsTripBudget",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "TrainingSession",
                schema: "Hrms",
                newName: "hrmsTrainingSession",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "TrainingProviderPayment",
                schema: "Hrms",
                newName: "hrmsTrainingProviderPayment",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "TrainingNeed",
                schema: "Hrms",
                newName: "hrmsTrainingNeed",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "TrainingEnrollment",
                schema: "Hrms",
                newName: "hrmsTrainingEnrollment",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "TrainingCourse",
                schema: "Hrms",
                newName: "hrmsTrainingCourse",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "TrainingCategory",
                schema: "Hrms",
                newName: "hrmsTrainingCategory",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "TrainingBudget",
                schema: "Hrms",
                newName: "hrmsTrainingBudget",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "TerminationSettlement",
                schema: "Hrms",
                newName: "hrmsTerminationSettlement",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "TerminationClearance",
                schema: "Hrms",
                newName: "hrmsTerminationClearance",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "TerminationAssetRecovery",
                schema: "Hrms",
                newName: "hrmsTerminationAssetRecovery",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "TaxBracket",
                schema: "Hrms",
                newName: "hrmsTaxBracket",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "TalentReview",
                schema: "Hrms",
                newName: "hrmsTalentReview",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "TalentRating",
                schema: "Hrms",
                newName: "hrmsTalentRating",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "TalentAssessment",
                schema: "Hrms",
                newName: "hrmsTalentAssessment",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "SurveyResponse",
                schema: "Hrms",
                newName: "hrmsSurveyResponse",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "SurveyCompletion",
                schema: "Hrms",
                newName: "hrmsSurveyCompletion",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "Survey",
                schema: "Hrms",
                newName: "hrmsSurvey",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "Suggestion",
                schema: "Hrms",
                newName: "hrmsSuggestion",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "SuccessionPlan",
                schema: "Hrms",
                newName: "hrmsSuccessionPlan",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "SuccessionDevelopmentAction",
                schema: "Hrms",
                newName: "hrmsSuccessionDevelopmentAction",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "SuccessionCandidate",
                schema: "Hrms",
                newName: "hrmsSuccessionCandidate",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "Subsystem",
                schema: "Core",
                newName: "coreSubsystem",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "Step",
                schema: "Core",
                newName: "lupStep",
                newSchema: "Core");

            migrationBuilder.RenameTable(
                name: "SettlementLine",
                schema: "Hrms",
                newName: "hrmsSettlementLine",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "SalaryScale",
                schema: "Core",
                newName: "coreSalaryScale",
                newSchema: "Core");

            migrationBuilder.RenameTable(
                name: "SalaryRevisionLine",
                schema: "Hrms",
                newName: "hrmsSalaryRevisionLine",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "SalaryRevisionBand",
                schema: "Hrms",
                newName: "hrmsSalaryRevisionBand",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "SalaryRevision",
                schema: "Hrms",
                newName: "hrmsSalaryRevision",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "RewardPointsTransaction",
                schema: "Hrms",
                newName: "hrmsRewardPointsTransaction",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "RewardNomination",
                schema: "Hrms",
                newName: "hrmsRewardNomination",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "RewardDisbursement",
                schema: "Hrms",
                newName: "hrmsRewardDisbursement",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "ReviewCycle",
                schema: "Hrms",
                newName: "hrmsReviewCycle",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "RequisitionScreeningCriterion",
                schema: "Hrms",
                newName: "hrmsRequisitionScreeningCriterion",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "ReportScheduleRecipient",
                schema: "Hrms",
                newName: "hrmsReportScheduleRecipient",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "ReportScheduleFieldValue",
                schema: "Hrms",
                newName: "hrmsReportScheduleFieldValue",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "ReportScheduleFieldOutput",
                schema: "Hrms",
                newName: "hrmsReportScheduleFieldOutput",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "ReportSchedule",
                schema: "Hrms",
                newName: "hrmsReportSchedule",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "ReportSavedFilter",
                schema: "Hrms",
                newName: "hrmsReportSavedFilter",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "ReportRunRecipient",
                schema: "Hrms",
                newName: "hrmsReportRunRecipient",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "ReportRun",
                schema: "Hrms",
                newName: "hrmsReportRun",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "ReportRestriction",
                schema: "Hrms",
                newName: "hrmsReportRestriction",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "ReportFieldOutput",
                schema: "Hrms",
                newName: "hrmsReportFieldOutput",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "ReportField",
                schema: "Hrms",
                newName: "hrmsReportField",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "Report",
                schema: "Hrms",
                newName: "hrmsReport",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "RecognitionProgram",
                schema: "Hrms",
                newName: "hrmsRecognitionProgram",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "RecognitionBadge",
                schema: "Hrms",
                newName: "hrmsRecognitionBadge",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "RatingScaleLevel",
                schema: "Hrms",
                newName: "hrmsRatingScaleLevel",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "RatingScale",
                schema: "Hrms",
                newName: "hrmsRatingScale",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "ProfileChangeRequest",
                schema: "Hrms",
                newName: "hrmsProfileChangeRequest",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "PositionCompetency",
                schema: "Hrms",
                newName: "hrmsPositionCompetency",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "PositionClass",
                schema: "Hrms",
                newName: "hrmsPositionClass",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "Position",
                schema: "Hrms",
                newName: "hrmsPosition",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "PipObjective",
                schema: "Hrms",
                newName: "hrmsPipObjective",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "Person",
                schema: "Core",
                newName: "CorePerson",
                newSchema: "Core");

            migrationBuilder.RenameTable(
                name: "PerformanceHistory",
                schema: "Hrms",
                newName: "hrmsPerformanceHistory",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "PerDiemRate",
                schema: "Hrms",
                newName: "hrmsPerDiemRate",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "OtherLeaveSetting",
                schema: "Hrms",
                newName: "hrmsOtherLeaveSetting",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "OtherLeaveDetail",
                schema: "Hrms",
                newName: "hrmsOtherLeaveDetail",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "OtherLeave",
                schema: "Hrms",
                newName: "hrmsOtherLeave",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "OrganizationUnit",
                schema: "Hrms",
                newName: "hrmsOrganizationUnit",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "OrganizationalObjective",
                schema: "Hrms",
                newName: "hrmsOrganizationalObjective",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "Operation",
                schema: "Core",
                newName: "coreOperation",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "OfferLetterTemplate",
                schema: "Hrms",
                newName: "hrmsOfferLetterTemplate",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "NumberSequence",
                schema: "Hrms",
                newName: "hrmsNumberSequence",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "Module",
                schema: "Core",
                newName: "coreModule",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "Mentorship",
                schema: "Hrms",
                newName: "hrmsMentorship",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "MedicalServiceContract",
                schema: "Hrms",
                newName: "hrmsMedicalServiceContract",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "MedicalProvider",
                schema: "Hrms",
                newName: "hrmsMedicalProvider",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "MedicalPlan",
                schema: "Hrms",
                newName: "hrmsMedicalPlan",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "MedicalEnrollment",
                schema: "Hrms",
                newName: "hrmsMedicalEnrollment",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "MedicalClaimAttachment",
                schema: "Hrms",
                newName: "hrmsMedicalClaimAttachment",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "MedicalClaim",
                schema: "Hrms",
                newName: "hrmsMedicalClaim",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "MedicalBeneficiary",
                schema: "Hrms",
                newName: "hrmsMedicalBeneficiary",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "LoanType",
                schema: "Hrms",
                newName: "hrmsLoanType",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "LoanRepaymentSchedule",
                schema: "Hrms",
                newName: "hrmsLoanRepaymentSchedule",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "LoanGuarantor",
                schema: "Hrms",
                newName: "hrmsLoanGuarantor",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "Loan",
                schema: "Hrms",
                newName: "hrmsLoan",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "LeaveType",
                schema: "Hrms",
                newName: "hrmsLeaveType",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "LeaveRequestLine",
                schema: "Hrms",
                newName: "hrmsLeaveRequestLine",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "LeaveRequest",
                schema: "Hrms",
                newName: "hrmsLeaveRequest",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "LeaveBalanceTransaction",
                schema: "Hrms",
                newName: "hrmsLeaveBalanceTransaction",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "LeaveBalance",
                schema: "Hrms",
                newName: "hrmsLeaveBalance",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "LearningPathStep",
                schema: "Hrms",
                newName: "hrmsLearningPathStep",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "LearningPath",
                schema: "Hrms",
                newName: "hrmsLearningPath",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "LearningCommunityPost",
                schema: "Hrms",
                newName: "hrmsLearningCommunityPost",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "LearningCommunityMember",
                schema: "Hrms",
                newName: "hrmsLearningCommunityMember",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "LearningCommunity",
                schema: "Hrms",
                newName: "hrmsLearningCommunity",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "KnowledgeTransfer",
                schema: "Hrms",
                newName: "hrmsKnowledgeTransfer",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "JobRequisition",
                schema: "Hrms",
                newName: "hrmsJobRequisition",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "JobOffer",
                schema: "Hrms",
                newName: "hrmsJobOffer",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "JobGrade",
                schema: "Hrms",
                newName: "hrmsJobGrade",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "JobCategory",
                schema: "Hrms",
                newName: "hrmsJobCategory",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "JobApplicationStageLog",
                schema: "Hrms",
                newName: "hrmsJobApplicationStageLog",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "JobApplication",
                schema: "Hrms",
                newName: "hrmsJobApplication",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "InterviewPanelist",
                schema: "Hrms",
                newName: "hrmsInterviewPanelist",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "InterviewFeedback",
                schema: "Hrms",
                newName: "hrmsInterviewFeedback",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "Interview",
                schema: "Hrms",
                newName: "hrmsInterview",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "InsurancePremiumSchedule",
                schema: "Hrms",
                newName: "hrmsInsurancePremiumSchedule",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "InsurancePolicy",
                schema: "Hrms",
                newName: "hrmsInsurancePolicy",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "InsuranceClaimAttachment",
                schema: "Hrms",
                newName: "hrmsInsuranceClaimAttachment",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "InsuranceClaim",
                schema: "Hrms",
                newName: "hrmsInsuranceClaim",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "ImprovementPlan",
                schema: "Hrms",
                newName: "hrmsImprovementPlan",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "Holiday",
                schema: "Hrms",
                newName: "hrmsHoliday",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "HiringRequest",
                schema: "Hrms",
                newName: "hrmsHiringRequest",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "GrievanceNote",
                schema: "Hrms",
                newName: "hrmsGrievanceNote",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "Grievance",
                schema: "Hrms",
                newName: "hrmsGrievance",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "GoalActionItem",
                schema: "Hrms",
                newName: "hrmsGoalActionItem",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "ExitQuestionnaire",
                schema: "Hrms",
                newName: "hrmsExitQuestionnaire",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "ExitInterview",
                schema: "Hrms",
                newName: "hrmsExitInterview",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "EmployeeTrainingCertificate",
                schema: "Hrms",
                newName: "hrmsEmployeeTrainingCertificate",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "EmployeeTermination",
                schema: "Hrms",
                newName: "hrmsEmployeeTermination",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "EmployeeRecognition",
                schema: "Hrms",
                newName: "hrmsEmployeeRecognition",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "EmployeeMovement",
                schema: "Hrms",
                newName: "hrmsEmployeeMovement",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "EmployeeGuarantee",
                schema: "Hrms",
                newName: "hrmsEmployeeGuarantee",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "EmployeeGoal",
                schema: "Hrms",
                newName: "hrmsEmployeeGoal",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "EmployeeFieldValue",
                schema: "Hrms",
                newName: "hrmsEmployeeFieldValue",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "EmployeeFieldDefinition",
                schema: "Hrms",
                newName: "hrmsEmployeeFieldDefinition",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "EmployeeExperience",
                schema: "Hrms",
                newName: "hrmsEmployeeExperience",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "EmployeeEducation",
                schema: "Hrms",
                newName: "hrmsEmployeeEducation",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "EmployeeDocument",
                schema: "Hrms",
                newName: "hrmsEmployeeDocument",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "EmployeeDependent",
                schema: "Hrms",
                newName: "hrmsEmployeeDependent",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "EmployeeCareerPathStepProgress",
                schema: "Hrms",
                newName: "hrmsEmployeeCareerPathStepProgress",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "EmployeeCareerPath",
                schema: "Hrms",
                newName: "hrmsEmployeeCareerPath",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "EmployeeBenefitEnrollment",
                schema: "Hrms",
                newName: "hrmsEmployeeBenefitEnrollment",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "EmployeeAllowance",
                schema: "Hrms",
                newName: "hrmsEmployeeAllowance",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "Employee",
                schema: "Hrms",
                newName: "hrmsEmployee",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "DynamicFormRecord",
                schema: "Hrms",
                newName: "hrmsDynamicFormRecord",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "DynamicFormField",
                schema: "Hrms",
                newName: "hrmsDynamicFormField",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "DynamicForm",
                schema: "Hrms",
                newName: "hrmsDynamicForm",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "DocumentTemplate",
                schema: "Hrms",
                newName: "hrmsDocumentTemplate",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "DisciplinaryMeasure",
                schema: "Hrms",
                newName: "hrmsDisciplinaryMeasure",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "DevelopmentPlan",
                schema: "Hrms",
                newName: "hrmsDevelopmentPlan",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "DevelopmentAction",
                schema: "Hrms",
                newName: "hrmsDevelopmentAction",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "CriticalPosition",
                schema: "Hrms",
                newName: "hrmsCriticalPosition",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "CriterionEvaluator",
                schema: "Hrms",
                newName: "hrmsCriterionEvaluator",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "CompetencyCategory",
                schema: "Hrms",
                newName: "hrmsCompetencyCategory",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "Competency",
                schema: "Hrms",
                newName: "hrmsCompetency",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "CompensationRequest",
                schema: "Hrms",
                newName: "hrmsCompensationRequest",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "CompanyProfile",
                schema: "Hrms",
                newName: "hrmsCompanyProfile",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "CompanyAsset",
                schema: "Hrms",
                newName: "hrmsCompanyAsset",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "CommunityPostReaction",
                schema: "Hrms",
                newName: "hrmsCommunityPostReaction",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "ClearanceDepartmentApprover",
                schema: "Hrms",
                newName: "hrmsClearanceDepartmentApprover",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "ClearanceDepartment",
                schema: "Hrms",
                newName: "hrmsClearanceDepartment",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "CareerPathStepCompetency",
                schema: "Hrms",
                newName: "hrmsCareerPathStepCompetency",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "CareerPathStep",
                schema: "Hrms",
                newName: "hrmsCareerPathStep",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "CareerPathChangeRequest",
                schema: "Hrms",
                newName: "hrmsCareerPathChangeRequest",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "CareerPath",
                schema: "Hrms",
                newName: "hrmsCareerPath",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "CandidateDocument",
                schema: "Hrms",
                newName: "hrmsCandidateDocument",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "Candidate",
                schema: "Hrms",
                newName: "hrmsCandidate",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "CalibrationSession",
                schema: "Hrms",
                newName: "hrmsCalibrationSession",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "CalibrationItem",
                schema: "Hrms",
                newName: "hrmsCalibrationItem",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "Branch",
                schema: "Hrms",
                newName: "hrmsBranch",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "BenefitPlan",
                schema: "Hrms",
                newName: "hrmsBenefitPlan",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "AwardCategory",
                schema: "Hrms",
                newName: "hrmsAwardCategory",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "AuditLog",
                schema: "Hrms",
                newName: "hrmsAuditLog",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "AppraisalTemplate",
                schema: "Hrms",
                newName: "hrmsAppraisalTemplate",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "AppraisalPeerReview",
                schema: "Hrms",
                newName: "hrmsAppraisalPeerReview",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "AppraisalGoal",
                schema: "Hrms",
                newName: "hrmsAppraisalGoal",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "AppraisalCompetency",
                schema: "Hrms",
                newName: "hrmsAppraisalCompetency",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "AppraisalAppeal",
                schema: "Hrms",
                newName: "hrmsAppraisalAppeal",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "Appraisal",
                schema: "Hrms",
                newName: "hrmsAppraisal",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "ApplicationCriterionScore",
                schema: "Hrms",
                newName: "hrmsApplicationCriterionScore",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "AnnualLeaveSetting",
                schema: "Hrms",
                newName: "hrmsAnnualLeaveSetting",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "AnnualLeaveHeader",
                schema: "Hrms",
                newName: "hrmsAnnualLeaveHeader",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "AnnualLeaveDetail",
                schema: "Hrms",
                newName: "hrmsAnnualLeaveDetail",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "Announcement",
                schema: "Hrms",
                newName: "hrmsAnnouncement",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "AllowanceType",
                schema: "Hrms",
                newName: "hrmsAllowanceType",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "Achievement",
                schema: "Hrms",
                newName: "hrmsAchievement",
                newSchema: "dbo");

            migrationBuilder.RenameIndex(
                name: "IX_WorkWeekConfiguration_TenantId_IsActive",
                schema: "dbo",
                table: "hrmsWorkWeekConfiguration",
                newName: "IX_hrmsWorkWeekConfiguration_TenantId_IsActive");

            migrationBuilder.RenameIndex(
                name: "IX_WorkLocation_TenantId_Code",
                schema: "dbo",
                table: "hrmsWorkLocation",
                newName: "IX_hrmsWorkLocation_TenantId_Code");

            migrationBuilder.RenameIndex(
                name: "IX_WorkLocation_ParentId",
                schema: "dbo",
                table: "hrmsWorkLocation",
                newName: "IX_hrmsWorkLocation_ParentId");

            migrationBuilder.RenameIndex(
                name: "IX_WorkforcePlanLine_PositionClassId",
                schema: "dbo",
                table: "hrmsWorkforcePlanLine",
                newName: "IX_hrmsWorkforcePlanLine_PositionClassId");

            migrationBuilder.RenameIndex(
                name: "IX_WorkforcePlanLine_PlanId_OrganizationUnitId_PositionClassId_EmploymentType_PeriodIndex",
                schema: "dbo",
                table: "hrmsWorkforcePlanLine",
                newName: "IX_hrmsWorkforcePlanLine_PlanId_OrganizationUnitId_PositionClassId_EmploymentType_PeriodIndex");

            migrationBuilder.RenameIndex(
                name: "IX_WorkforcePlanLine_PlanId",
                schema: "dbo",
                table: "hrmsWorkforcePlanLine",
                newName: "IX_hrmsWorkforcePlanLine_PlanId");

            migrationBuilder.RenameIndex(
                name: "IX_WorkforcePlanLine_OrganizationUnitId",
                schema: "dbo",
                table: "hrmsWorkforcePlanLine",
                newName: "IX_hrmsWorkforcePlanLine_OrganizationUnitId");

            migrationBuilder.RenameIndex(
                name: "IX_WorkforcePlan_TenantId_Status",
                schema: "dbo",
                table: "hrmsWorkforcePlan",
                newName: "IX_hrmsWorkforcePlan_TenantId_Status");

            migrationBuilder.RenameIndex(
                name: "IX_WorkforcePlan_StartFiscalYearId",
                schema: "dbo",
                table: "hrmsWorkforcePlan",
                newName: "IX_hrmsWorkforcePlan_StartFiscalYearId");

            migrationBuilder.RenameIndex(
                name: "IX_WorkforcePlan_RootPlanId",
                schema: "dbo",
                table: "hrmsWorkforcePlan",
                newName: "IX_hrmsWorkforcePlan_RootPlanId");

            migrationBuilder.RenameIndex(
                name: "IX_WorkforcePlan_OrganizationUnitId",
                schema: "dbo",
                table: "hrmsWorkforcePlan",
                newName: "IX_hrmsWorkforcePlan_OrganizationUnitId");

            migrationBuilder.RenameIndex(
                name: "IX_WorkflowStepApprover_StepId",
                schema: "dbo",
                table: "hrmsWorkflowStepApprover",
                newName: "IX_hrmsWorkflowStepApprover_StepId");

            migrationBuilder.RenameIndex(
                name: "IX_WorkflowStepApprover_ApproverType_ApproverId",
                schema: "dbo",
                table: "hrmsWorkflowStepApprover",
                newName: "IX_hrmsWorkflowStepApprover_ApproverType_ApproverId");

            migrationBuilder.RenameIndex(
                name: "IX_WorkflowStep_DefinitionId_StepOrder",
                schema: "dbo",
                table: "hrmsWorkflowStep",
                newName: "IX_hrmsWorkflowStep_DefinitionId_StepOrder");

            migrationBuilder.RenameIndex(
                name: "IX_WorkflowInstance_TenantId_Status",
                schema: "dbo",
                table: "hrmsWorkflowInstance",
                newName: "IX_hrmsWorkflowInstance_TenantId_Status");

            migrationBuilder.RenameIndex(
                name: "IX_WorkflowInstance_Status",
                schema: "dbo",
                table: "hrmsWorkflowInstance",
                newName: "IX_hrmsWorkflowInstance_Status");

            migrationBuilder.RenameIndex(
                name: "IX_WorkflowInstance_EntityType_EntityId",
                schema: "dbo",
                table: "hrmsWorkflowInstance",
                newName: "IX_hrmsWorkflowInstance_EntityType_EntityId");

            migrationBuilder.RenameIndex(
                name: "IX_WorkflowInstance_DefinitionId",
                schema: "dbo",
                table: "hrmsWorkflowInstance",
                newName: "IX_hrmsWorkflowInstance_DefinitionId");

            migrationBuilder.RenameIndex(
                name: "IX_WorkflowDefinition_TenantId_EntityType",
                schema: "dbo",
                table: "hrmsWorkflowDefinition",
                newName: "IX_hrmsWorkflowDefinition_TenantId_EntityType");

            migrationBuilder.RenameIndex(
                name: "IX_WorkflowActionLog_InstanceId",
                schema: "dbo",
                table: "hrmsWorkflowActionLog",
                newName: "IX_hrmsWorkflowActionLog_InstanceId");

            migrationBuilder.RenameIndex(
                name: "IX_TripRequest_TripBudgetId",
                schema: "dbo",
                table: "hrmsTripRequest",
                newName: "IX_hrmsTripRequest_TripBudgetId");

            migrationBuilder.RenameIndex(
                name: "IX_TripRequest_TenantId_TripNumber",
                schema: "dbo",
                table: "hrmsTripRequest",
                newName: "IX_hrmsTripRequest_TenantId_TripNumber");

            migrationBuilder.RenameIndex(
                name: "IX_TripRequest_Status",
                schema: "dbo",
                table: "hrmsTripRequest",
                newName: "IX_hrmsTripRequest_Status");

            migrationBuilder.RenameIndex(
                name: "IX_TripRequest_EmployeeId_Status",
                schema: "dbo",
                table: "hrmsTripRequest",
                newName: "IX_hrmsTripRequest_EmployeeId_Status");

            migrationBuilder.RenameIndex(
                name: "IX_TripExpense_TripRequestId",
                schema: "dbo",
                table: "hrmsTripExpense",
                newName: "IX_hrmsTripExpense_TripRequestId");

            migrationBuilder.RenameIndex(
                name: "IX_TripBudget_TenantId_FiscalYear_OrganizationUnitId",
                schema: "dbo",
                table: "hrmsTripBudget",
                newName: "IX_hrmsTripBudget_TenantId_FiscalYear_OrganizationUnitId");

            migrationBuilder.RenameIndex(
                name: "IX_TripBudget_OrganizationUnitId",
                schema: "dbo",
                table: "hrmsTripBudget",
                newName: "IX_hrmsTripBudget_OrganizationUnitId");

            migrationBuilder.RenameIndex(
                name: "IX_TrainingSession_TrainingCourseId",
                schema: "dbo",
                table: "hrmsTrainingSession",
                newName: "IX_hrmsTrainingSession_TrainingCourseId");

            migrationBuilder.RenameIndex(
                name: "IX_TrainingSession_TenantId_TrainingCourseId",
                schema: "dbo",
                table: "hrmsTrainingSession",
                newName: "IX_hrmsTrainingSession_TenantId_TrainingCourseId");

            migrationBuilder.RenameIndex(
                name: "IX_TrainingSession_TenantId_StartDate",
                schema: "dbo",
                table: "hrmsTrainingSession",
                newName: "IX_hrmsTrainingSession_TenantId_StartDate");

            migrationBuilder.RenameIndex(
                name: "IX_TrainingProviderPayment_TrainingSessionId",
                schema: "dbo",
                table: "hrmsTrainingProviderPayment",
                newName: "IX_hrmsTrainingProviderPayment_TrainingSessionId");

            migrationBuilder.RenameIndex(
                name: "IX_TrainingProviderPayment_TenantId_TrainingSessionId",
                schema: "dbo",
                table: "hrmsTrainingProviderPayment",
                newName: "IX_hrmsTrainingProviderPayment_TenantId_TrainingSessionId");

            migrationBuilder.RenameIndex(
                name: "IX_TrainingProviderPayment_TenantId_Status",
                schema: "dbo",
                table: "hrmsTrainingProviderPayment",
                newName: "IX_hrmsTrainingProviderPayment_TenantId_Status");

            migrationBuilder.RenameIndex(
                name: "IX_TrainingNeed_TrainingCourseId",
                schema: "dbo",
                table: "hrmsTrainingNeed",
                newName: "IX_hrmsTrainingNeed_TrainingCourseId");

            migrationBuilder.RenameIndex(
                name: "IX_TrainingNeed_TenantId_Status",
                schema: "dbo",
                table: "hrmsTrainingNeed",
                newName: "IX_hrmsTrainingNeed_TenantId_Status");

            migrationBuilder.RenameIndex(
                name: "IX_TrainingNeed_TenantId_EmployeeId",
                schema: "dbo",
                table: "hrmsTrainingNeed",
                newName: "IX_hrmsTrainingNeed_TenantId_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_TrainingNeed_EmployeeId",
                schema: "dbo",
                table: "hrmsTrainingNeed",
                newName: "IX_hrmsTrainingNeed_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_TrainingNeed_CompetencyId",
                schema: "dbo",
                table: "hrmsTrainingNeed",
                newName: "IX_hrmsTrainingNeed_CompetencyId");

            migrationBuilder.RenameIndex(
                name: "IX_TrainingEnrollment_TrainingSessionId",
                schema: "dbo",
                table: "hrmsTrainingEnrollment",
                newName: "IX_hrmsTrainingEnrollment_TrainingSessionId");

            migrationBuilder.RenameIndex(
                name: "IX_TrainingEnrollment_TrainingNeedId",
                schema: "dbo",
                table: "hrmsTrainingEnrollment",
                newName: "IX_hrmsTrainingEnrollment_TrainingNeedId");

            migrationBuilder.RenameIndex(
                name: "IX_TrainingEnrollment_TenantId_TrainingSessionId_EmployeeId",
                schema: "dbo",
                table: "hrmsTrainingEnrollment",
                newName: "IX_hrmsTrainingEnrollment_TenantId_TrainingSessionId_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_TrainingEnrollment_TenantId_EmployeeId_Status",
                schema: "dbo",
                table: "hrmsTrainingEnrollment",
                newName: "IX_hrmsTrainingEnrollment_TenantId_EmployeeId_Status");

            migrationBuilder.RenameIndex(
                name: "IX_TrainingEnrollment_EmployeeId",
                schema: "dbo",
                table: "hrmsTrainingEnrollment",
                newName: "IX_hrmsTrainingEnrollment_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_TrainingCourse_TrainingCategoryId",
                schema: "dbo",
                table: "hrmsTrainingCourse",
                newName: "IX_hrmsTrainingCourse_TrainingCategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_TrainingCourse_TenantId_TrainingCategoryId",
                schema: "dbo",
                table: "hrmsTrainingCourse",
                newName: "IX_hrmsTrainingCourse_TenantId_TrainingCategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_TrainingCourse_TenantId_Name",
                schema: "dbo",
                table: "hrmsTrainingCourse",
                newName: "IX_hrmsTrainingCourse_TenantId_Name");

            migrationBuilder.RenameIndex(
                name: "IX_TrainingCategory_TenantId_Name",
                schema: "dbo",
                table: "hrmsTrainingCategory",
                newName: "IX_hrmsTrainingCategory_TenantId_Name");

            migrationBuilder.RenameIndex(
                name: "IX_TrainingBudget_TenantId_FiscalYear_OrganizationUnitId",
                schema: "dbo",
                table: "hrmsTrainingBudget",
                newName: "IX_hrmsTrainingBudget_TenantId_FiscalYear_OrganizationUnitId");

            migrationBuilder.RenameIndex(
                name: "IX_TrainingBudget_OrganizationUnitId",
                schema: "dbo",
                table: "hrmsTrainingBudget",
                newName: "IX_hrmsTrainingBudget_OrganizationUnitId");

            migrationBuilder.RenameIndex(
                name: "IX_TerminationSettlement_TerminationId",
                schema: "dbo",
                table: "hrmsTerminationSettlement",
                newName: "IX_hrmsTerminationSettlement_TerminationId");

            migrationBuilder.RenameIndex(
                name: "IX_TerminationSettlement_TenantId_TerminationId",
                schema: "dbo",
                table: "hrmsTerminationSettlement",
                newName: "IX_hrmsTerminationSettlement_TenantId_TerminationId");

            migrationBuilder.RenameIndex(
                name: "IX_TerminationClearance_TerminationId",
                schema: "dbo",
                table: "hrmsTerminationClearance",
                newName: "IX_hrmsTerminationClearance_TerminationId");

            migrationBuilder.RenameIndex(
                name: "IX_TerminationClearance_DepartmentId",
                schema: "dbo",
                table: "hrmsTerminationClearance",
                newName: "IX_hrmsTerminationClearance_DepartmentId");

            migrationBuilder.RenameIndex(
                name: "IX_TerminationAssetRecovery_TerminationId",
                schema: "dbo",
                table: "hrmsTerminationAssetRecovery",
                newName: "IX_hrmsTerminationAssetRecovery_TerminationId");

            migrationBuilder.RenameIndex(
                name: "IX_TerminationAssetRecovery_TenantId_TerminationId",
                schema: "dbo",
                table: "hrmsTerminationAssetRecovery",
                newName: "IX_hrmsTerminationAssetRecovery_TenantId_TerminationId");

            migrationBuilder.RenameIndex(
                name: "IX_TerminationAssetRecovery_CompanyAssetId",
                schema: "dbo",
                table: "hrmsTerminationAssetRecovery",
                newName: "IX_hrmsTerminationAssetRecovery_CompanyAssetId");

            migrationBuilder.RenameIndex(
                name: "IX_TalentReview_TenantId_Status",
                schema: "dbo",
                table: "hrmsTalentReview",
                newName: "IX_hrmsTalentReview_TenantId_Status");

            migrationBuilder.RenameIndex(
                name: "IX_TalentReview_OrganizationUnitId",
                schema: "dbo",
                table: "hrmsTalentReview",
                newName: "IX_hrmsTalentReview_OrganizationUnitId");

            migrationBuilder.RenameIndex(
                name: "IX_TalentRating_TalentAssessmentId",
                schema: "dbo",
                table: "hrmsTalentRating",
                newName: "IX_hrmsTalentRating_TalentAssessmentId");

            migrationBuilder.RenameIndex(
                name: "IX_TalentRating_RaterEmployeeId",
                schema: "dbo",
                table: "hrmsTalentRating",
                newName: "IX_hrmsTalentRating_RaterEmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_TalentAssessment_TenantId_TalentReviewId_PerformanceBand_PotentialBand",
                schema: "dbo",
                table: "hrmsTalentAssessment",
                newName: "IX_hrmsTalentAssessment_TenantId_TalentReviewId_PerformanceBand_PotentialBand");

            migrationBuilder.RenameIndex(
                name: "IX_TalentAssessment_TalentReviewId_EmployeeId",
                schema: "dbo",
                table: "hrmsTalentAssessment",
                newName: "IX_hrmsTalentAssessment_TalentReviewId_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_TalentAssessment_EmployeeId",
                schema: "dbo",
                table: "hrmsTalentAssessment",
                newName: "IX_hrmsTalentAssessment_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_SurveyResponse_TenantId_SurveyId",
                schema: "dbo",
                table: "hrmsSurveyResponse",
                newName: "IX_hrmsSurveyResponse_TenantId_SurveyId");

            migrationBuilder.RenameIndex(
                name: "IX_SurveyResponse_SurveyId",
                schema: "dbo",
                table: "hrmsSurveyResponse",
                newName: "IX_hrmsSurveyResponse_SurveyId");

            migrationBuilder.RenameIndex(
                name: "IX_SurveyCompletion_TenantId_SurveyId_EmployeeId",
                schema: "dbo",
                table: "hrmsSurveyCompletion",
                newName: "IX_hrmsSurveyCompletion_TenantId_SurveyId_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_SurveyCompletion_SurveyId",
                schema: "dbo",
                table: "hrmsSurveyCompletion",
                newName: "IX_hrmsSurveyCompletion_SurveyId");

            migrationBuilder.RenameIndex(
                name: "IX_SurveyCompletion_EmployeeId",
                schema: "dbo",
                table: "hrmsSurveyCompletion",
                newName: "IX_hrmsSurveyCompletion_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_Survey_TenantId_Status",
                schema: "dbo",
                table: "hrmsSurvey",
                newName: "IX_hrmsSurvey_TenantId_Status");

            migrationBuilder.RenameIndex(
                name: "IX_Suggestion_TenantId_Status",
                schema: "dbo",
                table: "hrmsSuggestion",
                newName: "IX_hrmsSuggestion_TenantId_Status");

            migrationBuilder.RenameIndex(
                name: "IX_SuccessionPlan_TenantId_Status",
                schema: "dbo",
                table: "hrmsSuccessionPlan",
                newName: "IX_hrmsSuccessionPlan_TenantId_Status");

            migrationBuilder.RenameIndex(
                name: "IX_SuccessionPlan_TenantId_CriticalPositionId",
                schema: "dbo",
                table: "hrmsSuccessionPlan",
                newName: "IX_hrmsSuccessionPlan_TenantId_CriticalPositionId");

            migrationBuilder.RenameIndex(
                name: "IX_SuccessionPlan_CriticalPositionId",
                schema: "dbo",
                table: "hrmsSuccessionPlan",
                newName: "IX_hrmsSuccessionPlan_CriticalPositionId");

            migrationBuilder.RenameIndex(
                name: "IX_SuccessionDevelopmentAction_SuccessionCandidateId",
                schema: "dbo",
                table: "hrmsSuccessionDevelopmentAction",
                newName: "IX_hrmsSuccessionDevelopmentAction_SuccessionCandidateId");

            migrationBuilder.RenameIndex(
                name: "IX_SuccessionDevelopmentAction_MentorEmployeeId",
                schema: "dbo",
                table: "hrmsSuccessionDevelopmentAction",
                newName: "IX_hrmsSuccessionDevelopmentAction_MentorEmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_SuccessionCandidate_TenantId_EmployeeId",
                schema: "dbo",
                table: "hrmsSuccessionCandidate",
                newName: "IX_hrmsSuccessionCandidate_TenantId_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_SuccessionCandidate_SuccessionPlanId_Rank",
                schema: "dbo",
                table: "hrmsSuccessionCandidate",
                newName: "IX_hrmsSuccessionCandidate_SuccessionPlanId_Rank");

            migrationBuilder.RenameIndex(
                name: "IX_SuccessionCandidate_SuccessionPlanId_EmployeeId",
                schema: "dbo",
                table: "hrmsSuccessionCandidate",
                newName: "IX_hrmsSuccessionCandidate_SuccessionPlanId_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_SuccessionCandidate_EmployeeId",
                schema: "dbo",
                table: "hrmsSuccessionCandidate",
                newName: "IX_hrmsSuccessionCandidate_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_Subsystem_TenantId_Name",
                schema: "dbo",
                table: "coreSubsystem",
                newName: "IX_coreSubsystem_TenantId_Name");

            migrationBuilder.RenameIndex(
                name: "IX_Step_TenantId_Code",
                schema: "Core",
                table: "lupStep",
                newName: "IX_lupStep_TenantId_Code");

            migrationBuilder.RenameIndex(
                name: "IX_SettlementLine_TerminationSettlementId",
                schema: "dbo",
                table: "hrmsSettlementLine",
                newName: "IX_hrmsSettlementLine_TerminationSettlementId");

            migrationBuilder.RenameIndex(
                name: "IX_SettlementLine_TenantId_TerminationSettlementId",
                schema: "dbo",
                table: "hrmsSettlementLine",
                newName: "IX_hrmsSettlementLine_TenantId_TerminationSettlementId");

            migrationBuilder.RenameIndex(
                name: "IX_SalaryScale_TenantId_JobGradeId_StepId",
                schema: "Core",
                table: "coreSalaryScale",
                newName: "IX_coreSalaryScale_TenantId_JobGradeId_StepId");

            migrationBuilder.RenameIndex(
                name: "IX_SalaryScale_StepId",
                schema: "Core",
                table: "coreSalaryScale",
                newName: "IX_coreSalaryScale_StepId");

            migrationBuilder.RenameIndex(
                name: "IX_SalaryScale_JobGradeId",
                schema: "Core",
                table: "coreSalaryScale",
                newName: "IX_coreSalaryScale_JobGradeId");

            migrationBuilder.RenameIndex(
                name: "IX_SalaryRevisionLine_SalaryRevisionId",
                schema: "dbo",
                table: "hrmsSalaryRevisionLine",
                newName: "IX_hrmsSalaryRevisionLine_SalaryRevisionId");

            migrationBuilder.RenameIndex(
                name: "IX_SalaryRevisionLine_EmployeeId",
                schema: "dbo",
                table: "hrmsSalaryRevisionLine",
                newName: "IX_hrmsSalaryRevisionLine_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_SalaryRevisionBand_SalaryRevisionId_MinScore",
                schema: "dbo",
                table: "hrmsSalaryRevisionBand",
                newName: "IX_hrmsSalaryRevisionBand_SalaryRevisionId_MinScore");

            migrationBuilder.RenameIndex(
                name: "IX_SalaryRevision_Status",
                schema: "dbo",
                table: "hrmsSalaryRevision",
                newName: "IX_hrmsSalaryRevision_Status");

            migrationBuilder.RenameIndex(
                name: "IX_RewardPointsTransaction_TenantId_EmployeeId_TransactionDate",
                schema: "dbo",
                table: "hrmsRewardPointsTransaction",
                newName: "IX_hrmsRewardPointsTransaction_TenantId_EmployeeId_TransactionDate");

            migrationBuilder.RenameIndex(
                name: "IX_RewardPointsTransaction_EmployeeId",
                schema: "dbo",
                table: "hrmsRewardPointsTransaction",
                newName: "IX_hrmsRewardPointsTransaction_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_RewardNomination_TenantId_Status",
                schema: "dbo",
                table: "hrmsRewardNomination",
                newName: "IX_hrmsRewardNomination_TenantId_Status");

            migrationBuilder.RenameIndex(
                name: "IX_RewardNomination_TenantId_NomineeEmployeeId",
                schema: "dbo",
                table: "hrmsRewardNomination",
                newName: "IX_hrmsRewardNomination_TenantId_NomineeEmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_RewardNomination_RecognitionProgramId",
                schema: "dbo",
                table: "hrmsRewardNomination",
                newName: "IX_hrmsRewardNomination_RecognitionProgramId");

            migrationBuilder.RenameIndex(
                name: "IX_RewardNomination_RecognitionBadgeId",
                schema: "dbo",
                table: "hrmsRewardNomination",
                newName: "IX_hrmsRewardNomination_RecognitionBadgeId");

            migrationBuilder.RenameIndex(
                name: "IX_RewardNomination_NomineeEmployeeId",
                schema: "dbo",
                table: "hrmsRewardNomination",
                newName: "IX_hrmsRewardNomination_NomineeEmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_RewardDisbursement_TenantId_Status",
                schema: "dbo",
                table: "hrmsRewardDisbursement",
                newName: "IX_hrmsRewardDisbursement_TenantId_Status");

            migrationBuilder.RenameIndex(
                name: "IX_RewardDisbursement_TenantId_EmployeeId",
                schema: "dbo",
                table: "hrmsRewardDisbursement",
                newName: "IX_hrmsRewardDisbursement_TenantId_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_RewardDisbursement_RecognitionBadgeId",
                schema: "dbo",
                table: "hrmsRewardDisbursement",
                newName: "IX_hrmsRewardDisbursement_RecognitionBadgeId");

            migrationBuilder.RenameIndex(
                name: "IX_RewardDisbursement_EmployeeRecognitionId",
                schema: "dbo",
                table: "hrmsRewardDisbursement",
                newName: "IX_hrmsRewardDisbursement_EmployeeRecognitionId");

            migrationBuilder.RenameIndex(
                name: "IX_RewardDisbursement_EmployeeId",
                schema: "dbo",
                table: "hrmsRewardDisbursement",
                newName: "IX_hrmsRewardDisbursement_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_ReviewCycle_TenantId_Status",
                schema: "dbo",
                table: "hrmsReviewCycle",
                newName: "IX_hrmsReviewCycle_TenantId_Status");

            migrationBuilder.RenameIndex(
                name: "IX_ReviewCycle_TenantId_Name",
                schema: "dbo",
                table: "hrmsReviewCycle",
                newName: "IX_hrmsReviewCycle_TenantId_Name");

            migrationBuilder.RenameIndex(
                name: "IX_ReviewCycle_RatingScaleId",
                schema: "dbo",
                table: "hrmsReviewCycle",
                newName: "IX_hrmsReviewCycle_RatingScaleId");

            migrationBuilder.RenameIndex(
                name: "IX_ReviewCycle_FiscalYearId",
                schema: "dbo",
                table: "hrmsReviewCycle",
                newName: "IX_hrmsReviewCycle_FiscalYearId");

            migrationBuilder.RenameIndex(
                name: "IX_RequisitionScreeningCriterion_RequisitionId",
                schema: "dbo",
                table: "hrmsRequisitionScreeningCriterion",
                newName: "IX_hrmsRequisitionScreeningCriterion_RequisitionId");

            migrationBuilder.RenameIndex(
                name: "IX_ReportScheduleRecipient_ReportScheduleId",
                schema: "dbo",
                table: "hrmsReportScheduleRecipient",
                newName: "IX_hrmsReportScheduleRecipient_ReportScheduleId");

            migrationBuilder.RenameIndex(
                name: "IX_ReportScheduleFieldValue_ReportScheduleId",
                schema: "dbo",
                table: "hrmsReportScheduleFieldValue",
                newName: "IX_hrmsReportScheduleFieldValue_ReportScheduleId");

            migrationBuilder.RenameIndex(
                name: "IX_ReportScheduleFieldOutput_ReportScheduleId",
                schema: "dbo",
                table: "hrmsReportScheduleFieldOutput",
                newName: "IX_hrmsReportScheduleFieldOutput_ReportScheduleId");

            migrationBuilder.RenameIndex(
                name: "IX_ReportSchedule_ReportId",
                schema: "dbo",
                table: "hrmsReportSchedule",
                newName: "IX_hrmsReportSchedule_ReportId");

            migrationBuilder.RenameIndex(
                name: "IX_ReportSavedFilter_ReportId",
                schema: "dbo",
                table: "hrmsReportSavedFilter",
                newName: "IX_hrmsReportSavedFilter_ReportId");

            migrationBuilder.RenameIndex(
                name: "IX_ReportRunRecipient_ReportRunId",
                schema: "dbo",
                table: "hrmsReportRunRecipient",
                newName: "IX_hrmsReportRunRecipient_ReportRunId");

            migrationBuilder.RenameIndex(
                name: "IX_ReportRun_TenantId_ReportKey",
                schema: "dbo",
                table: "hrmsReportRun",
                newName: "IX_hrmsReportRun_TenantId_ReportKey");

            migrationBuilder.RenameIndex(
                name: "IX_ReportRestriction_RoleId",
                schema: "dbo",
                table: "hrmsReportRestriction",
                newName: "IX_hrmsReportRestriction_RoleId");

            migrationBuilder.RenameIndex(
                name: "IX_ReportRestriction_ReportId",
                schema: "dbo",
                table: "hrmsReportRestriction",
                newName: "IX_hrmsReportRestriction_ReportId");

            migrationBuilder.RenameIndex(
                name: "IX_ReportFieldOutput_ReportId",
                schema: "dbo",
                table: "hrmsReportFieldOutput",
                newName: "IX_hrmsReportFieldOutput_ReportId");

            migrationBuilder.RenameIndex(
                name: "IX_ReportField_ReportId",
                schema: "dbo",
                table: "hrmsReportField",
                newName: "IX_hrmsReportField_ReportId");

            migrationBuilder.RenameIndex(
                name: "IX_Report_TenantId_ReportKey",
                schema: "dbo",
                table: "hrmsReport",
                newName: "IX_hrmsReport_TenantId_ReportKey");

            migrationBuilder.RenameIndex(
                name: "IX_Report_TenantId_IsActive",
                schema: "dbo",
                table: "hrmsReport",
                newName: "IX_hrmsReport_TenantId_IsActive");

            migrationBuilder.RenameIndex(
                name: "IX_RecognitionProgram_TenantId_Name",
                schema: "dbo",
                table: "hrmsRecognitionProgram",
                newName: "IX_hrmsRecognitionProgram_TenantId_Name");

            migrationBuilder.RenameIndex(
                name: "IX_RecognitionProgram_RecognitionBadgeId",
                schema: "dbo",
                table: "hrmsRecognitionProgram",
                newName: "IX_hrmsRecognitionProgram_RecognitionBadgeId");

            migrationBuilder.RenameIndex(
                name: "IX_RecognitionBadge_TenantId_Name",
                schema: "dbo",
                table: "hrmsRecognitionBadge",
                newName: "IX_hrmsRecognitionBadge_TenantId_Name");

            migrationBuilder.RenameIndex(
                name: "IX_RecognitionBadge_AwardCategoryId",
                schema: "dbo",
                table: "hrmsRecognitionBadge",
                newName: "IX_hrmsRecognitionBadge_AwardCategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_RatingScaleLevel_RatingScaleId_Value",
                schema: "dbo",
                table: "hrmsRatingScaleLevel",
                newName: "IX_hrmsRatingScaleLevel_RatingScaleId_Value");

            migrationBuilder.RenameIndex(
                name: "IX_RatingScale_TenantId_Name",
                schema: "dbo",
                table: "hrmsRatingScale",
                newName: "IX_hrmsRatingScale_TenantId_Name");

            migrationBuilder.RenameIndex(
                name: "IX_ProfileChangeRequest_Status",
                schema: "dbo",
                table: "hrmsProfileChangeRequest",
                newName: "IX_hrmsProfileChangeRequest_Status");

            migrationBuilder.RenameIndex(
                name: "IX_ProfileChangeRequest_EmployeeId",
                schema: "dbo",
                table: "hrmsProfileChangeRequest",
                newName: "IX_hrmsProfileChangeRequest_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_PositionCompetency_PositionId_CompetencyId",
                schema: "dbo",
                table: "hrmsPositionCompetency",
                newName: "IX_hrmsPositionCompetency_PositionId_CompetencyId");

            migrationBuilder.RenameIndex(
                name: "IX_PositionCompetency_CompetencyId",
                schema: "dbo",
                table: "hrmsPositionCompetency",
                newName: "IX_hrmsPositionCompetency_CompetencyId");

            migrationBuilder.RenameIndex(
                name: "IX_PositionClass_WorkLocationId",
                schema: "dbo",
                table: "hrmsPositionClass",
                newName: "IX_hrmsPositionClass_WorkLocationId");

            migrationBuilder.RenameIndex(
                name: "IX_PositionClass_TenantId_Code",
                schema: "dbo",
                table: "hrmsPositionClass",
                newName: "IX_hrmsPositionClass_TenantId_Code");

            migrationBuilder.RenameIndex(
                name: "IX_PositionClass_SalaryScaleId",
                schema: "dbo",
                table: "hrmsPositionClass",
                newName: "IX_hrmsPositionClass_SalaryScaleId");

            migrationBuilder.RenameIndex(
                name: "IX_PositionClass_ReportsToPositionClassId",
                schema: "dbo",
                table: "hrmsPositionClass",
                newName: "IX_hrmsPositionClass_ReportsToPositionClassId");

            migrationBuilder.RenameIndex(
                name: "IX_PositionClass_JobCategoryId",
                schema: "dbo",
                table: "hrmsPositionClass",
                newName: "IX_hrmsPositionClass_JobCategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_Position_TenantId_BranchId_Code",
                schema: "dbo",
                table: "hrmsPosition",
                newName: "IX_hrmsPosition_TenantId_BranchId_Code");

            migrationBuilder.RenameIndex(
                name: "IX_Position_PositionClassId",
                schema: "dbo",
                table: "hrmsPosition",
                newName: "IX_hrmsPosition_PositionClassId");

            migrationBuilder.RenameIndex(
                name: "IX_Position_OrganizationUnitId",
                schema: "dbo",
                table: "hrmsPosition",
                newName: "IX_hrmsPosition_OrganizationUnitId");

            migrationBuilder.RenameIndex(
                name: "IX_Position_BranchId",
                schema: "dbo",
                table: "hrmsPosition",
                newName: "IX_hrmsPosition_BranchId");

            migrationBuilder.RenameIndex(
                name: "IX_PipObjective_PipId_SortOrder",
                schema: "dbo",
                table: "hrmsPipObjective",
                newName: "IX_hrmsPipObjective_PipId_SortOrder");

            migrationBuilder.RenameIndex(
                name: "IX_Person_FirstName_FatherName_GrandFatherName",
                schema: "Core",
                table: "CorePerson",
                newName: "IX_CorePerson_FirstName_FatherName_GrandFatherName");

            migrationBuilder.RenameIndex(
                name: "IX_PerformanceHistory_TenantId_EntityType_EntityId",
                schema: "dbo",
                table: "hrmsPerformanceHistory",
                newName: "IX_hrmsPerformanceHistory_TenantId_EntityType_EntityId");

            migrationBuilder.RenameIndex(
                name: "IX_PerDiemRate_TenantId_JobGradeId_TripType",
                schema: "dbo",
                table: "hrmsPerDiemRate",
                newName: "IX_hrmsPerDiemRate_TenantId_JobGradeId_TripType");

            migrationBuilder.RenameIndex(
                name: "IX_PerDiemRate_JobGradeId",
                schema: "dbo",
                table: "hrmsPerDiemRate",
                newName: "IX_hrmsPerDiemRate_JobGradeId");

            migrationBuilder.RenameIndex(
                name: "IX_OtherLeaveSetting_TenantId_IsActive",
                schema: "dbo",
                table: "hrmsOtherLeaveSetting",
                newName: "IX_hrmsOtherLeaveSetting_TenantId_IsActive");

            migrationBuilder.RenameIndex(
                name: "IX_OtherLeaveSetting_TenantId_FiscalYearId_LeaveTypeId",
                schema: "dbo",
                table: "hrmsOtherLeaveSetting",
                newName: "IX_hrmsOtherLeaveSetting_TenantId_FiscalYearId_LeaveTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_OtherLeaveSetting_LeaveTypeId",
                schema: "dbo",
                table: "hrmsOtherLeaveSetting",
                newName: "IX_hrmsOtherLeaveSetting_LeaveTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_OtherLeaveSetting_FiscalYearId",
                schema: "dbo",
                table: "hrmsOtherLeaveSetting",
                newName: "IX_hrmsOtherLeaveSetting_FiscalYearId");

            migrationBuilder.RenameIndex(
                name: "IX_OtherLeaveDetail_OtherLeaveHeaderId_StartDate_EndDate",
                schema: "dbo",
                table: "hrmsOtherLeaveDetail",
                newName: "IX_hrmsOtherLeaveDetail_OtherLeaveHeaderId_StartDate_EndDate");

            migrationBuilder.RenameIndex(
                name: "IX_OtherLeaveDetail_OtherLeaveHeaderId",
                schema: "dbo",
                table: "hrmsOtherLeaveDetail",
                newName: "IX_hrmsOtherLeaveDetail_OtherLeaveHeaderId");

            migrationBuilder.RenameIndex(
                name: "IX_OtherLeave_OtherLeaveSettingId",
                schema: "dbo",
                table: "hrmsOtherLeave",
                newName: "IX_hrmsOtherLeave_OtherLeaveSettingId");

            migrationBuilder.RenameIndex(
                name: "IX_OtherLeave_EmployeeId_Status",
                schema: "dbo",
                table: "hrmsOtherLeave",
                newName: "IX_hrmsOtherLeave_EmployeeId_Status");

            migrationBuilder.RenameIndex(
                name: "IX_OtherLeave_EmployeeId",
                schema: "dbo",
                table: "hrmsOtherLeave",
                newName: "IX_hrmsOtherLeave_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_OrganizationUnit_WorkLocationId",
                schema: "dbo",
                table: "hrmsOrganizationUnit",
                newName: "IX_hrmsOrganizationUnit_WorkLocationId");

            migrationBuilder.RenameIndex(
                name: "IX_OrganizationUnit_TenantId_BranchId_Code",
                schema: "dbo",
                table: "hrmsOrganizationUnit",
                newName: "IX_hrmsOrganizationUnit_TenantId_BranchId_Code");

            migrationBuilder.RenameIndex(
                name: "IX_OrganizationUnit_ParentId",
                schema: "dbo",
                table: "hrmsOrganizationUnit",
                newName: "IX_hrmsOrganizationUnit_ParentId");

            migrationBuilder.RenameIndex(
                name: "IX_OrganizationUnit_BranchId",
                schema: "dbo",
                table: "hrmsOrganizationUnit",
                newName: "IX_hrmsOrganizationUnit_BranchId");

            migrationBuilder.RenameIndex(
                name: "IX_OrganizationalObjective_TenantId_ReviewCycleId_Title",
                schema: "dbo",
                table: "hrmsOrganizationalObjective",
                newName: "IX_hrmsOrganizationalObjective_TenantId_ReviewCycleId_Title");

            migrationBuilder.RenameIndex(
                name: "IX_OrganizationalObjective_TenantId_ReviewCycleId",
                schema: "dbo",
                table: "hrmsOrganizationalObjective",
                newName: "IX_hrmsOrganizationalObjective_TenantId_ReviewCycleId");

            migrationBuilder.RenameIndex(
                name: "IX_OrganizationalObjective_ReviewCycleId",
                schema: "dbo",
                table: "hrmsOrganizationalObjective",
                newName: "IX_hrmsOrganizationalObjective_ReviewCycleId");

            migrationBuilder.RenameIndex(
                name: "IX_OrganizationalObjective_ParentObjectiveId",
                schema: "dbo",
                table: "hrmsOrganizationalObjective",
                newName: "IX_hrmsOrganizationalObjective_ParentObjectiveId");

            migrationBuilder.RenameIndex(
                name: "IX_OrganizationalObjective_OrganizationUnitId",
                schema: "dbo",
                table: "hrmsOrganizationalObjective",
                newName: "IX_hrmsOrganizationalObjective_OrganizationUnitId");

            migrationBuilder.RenameIndex(
                name: "IX_Operation_ModuleId",
                schema: "dbo",
                table: "coreOperation",
                newName: "IX_coreOperation_ModuleId");

            migrationBuilder.RenameIndex(
                name: "IX_OfferLetterTemplate_TenantId",
                schema: "dbo",
                table: "hrmsOfferLetterTemplate",
                newName: "IX_hrmsOfferLetterTemplate_TenantId");

            migrationBuilder.RenameIndex(
                name: "IX_Module_SubsystemId",
                schema: "dbo",
                table: "coreModule",
                newName: "IX_coreModule_SubsystemId");

            migrationBuilder.RenameIndex(
                name: "IX_Mentorship_TenantId_MenteeEmployeeId",
                schema: "dbo",
                table: "hrmsMentorship",
                newName: "IX_hrmsMentorship_TenantId_MenteeEmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_Mentorship_MentorEmployeeId",
                schema: "dbo",
                table: "hrmsMentorship",
                newName: "IX_hrmsMentorship_MentorEmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_Mentorship_MenteeEmployeeId",
                schema: "dbo",
                table: "hrmsMentorship",
                newName: "IX_hrmsMentorship_MenteeEmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_MedicalServiceContract_Status",
                schema: "dbo",
                table: "hrmsMedicalServiceContract",
                newName: "IX_hrmsMedicalServiceContract_Status");

            migrationBuilder.RenameIndex(
                name: "IX_MedicalServiceContract_MedicalProviderId",
                schema: "dbo",
                table: "hrmsMedicalServiceContract",
                newName: "IX_hrmsMedicalServiceContract_MedicalProviderId");

            migrationBuilder.RenameIndex(
                name: "IX_MedicalProvider_TenantId_Name",
                schema: "dbo",
                table: "hrmsMedicalProvider",
                newName: "IX_hrmsMedicalProvider_TenantId_Name");

            migrationBuilder.RenameIndex(
                name: "IX_MedicalPlan_TenantId_Name",
                schema: "dbo",
                table: "hrmsMedicalPlan",
                newName: "IX_hrmsMedicalPlan_TenantId_Name");

            migrationBuilder.RenameIndex(
                name: "IX_MedicalEnrollment_MedicalPlanId",
                schema: "dbo",
                table: "hrmsMedicalEnrollment",
                newName: "IX_hrmsMedicalEnrollment_MedicalPlanId");

            migrationBuilder.RenameIndex(
                name: "IX_MedicalEnrollment_EmployeeId",
                schema: "dbo",
                table: "hrmsMedicalEnrollment",
                newName: "IX_hrmsMedicalEnrollment_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_MedicalClaimAttachment_MedicalClaimId",
                schema: "dbo",
                table: "hrmsMedicalClaimAttachment",
                newName: "IX_hrmsMedicalClaimAttachment_MedicalClaimId");

            migrationBuilder.RenameIndex(
                name: "IX_MedicalClaim_TenantId_ClaimNumber",
                schema: "dbo",
                table: "hrmsMedicalClaim",
                newName: "IX_hrmsMedicalClaim_TenantId_ClaimNumber");

            migrationBuilder.RenameIndex(
                name: "IX_MedicalClaim_Status",
                schema: "dbo",
                table: "hrmsMedicalClaim",
                newName: "IX_hrmsMedicalClaim_Status");

            migrationBuilder.RenameIndex(
                name: "IX_MedicalClaim_MedicalEnrollmentId",
                schema: "dbo",
                table: "hrmsMedicalClaim",
                newName: "IX_hrmsMedicalClaim_MedicalEnrollmentId");

            migrationBuilder.RenameIndex(
                name: "IX_MedicalClaim_MedicalBeneficiaryId_Status",
                schema: "dbo",
                table: "hrmsMedicalClaim",
                newName: "IX_hrmsMedicalClaim_MedicalBeneficiaryId_Status");

            migrationBuilder.RenameIndex(
                name: "IX_MedicalClaim_EmployeeId",
                schema: "dbo",
                table: "hrmsMedicalClaim",
                newName: "IX_hrmsMedicalClaim_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_MedicalBeneficiary_MedicalEnrollmentId",
                schema: "dbo",
                table: "hrmsMedicalBeneficiary",
                newName: "IX_hrmsMedicalBeneficiary_MedicalEnrollmentId");

            migrationBuilder.RenameIndex(
                name: "IX_LoanType_TenantId_Name",
                schema: "dbo",
                table: "hrmsLoanType",
                newName: "IX_hrmsLoanType_TenantId_Name");

            migrationBuilder.RenameIndex(
                name: "IX_LoanRepaymentSchedule_Status_DueDate",
                schema: "dbo",
                table: "hrmsLoanRepaymentSchedule",
                newName: "IX_hrmsLoanRepaymentSchedule_Status_DueDate");

            migrationBuilder.RenameIndex(
                name: "IX_LoanRepaymentSchedule_LoanId_InstallmentNo",
                schema: "dbo",
                table: "hrmsLoanRepaymentSchedule",
                newName: "IX_hrmsLoanRepaymentSchedule_LoanId_InstallmentNo");

            migrationBuilder.RenameIndex(
                name: "IX_LoanGuarantor_LoanId",
                schema: "dbo",
                table: "hrmsLoanGuarantor",
                newName: "IX_hrmsLoanGuarantor_LoanId");

            migrationBuilder.RenameIndex(
                name: "IX_Loan_TenantId_LoanNumber",
                schema: "dbo",
                table: "hrmsLoan",
                newName: "IX_hrmsLoan_TenantId_LoanNumber");

            migrationBuilder.RenameIndex(
                name: "IX_Loan_Status",
                schema: "dbo",
                table: "hrmsLoan",
                newName: "IX_hrmsLoan_Status");

            migrationBuilder.RenameIndex(
                name: "IX_Loan_LoanTypeId",
                schema: "dbo",
                table: "hrmsLoan",
                newName: "IX_hrmsLoan_LoanTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_Loan_EmployeeId_Status",
                schema: "dbo",
                table: "hrmsLoan",
                newName: "IX_hrmsLoan_EmployeeId_Status");

            migrationBuilder.RenameIndex(
                name: "IX_LeaveType_TenantId_Code",
                schema: "dbo",
                table: "hrmsLeaveType",
                newName: "IX_hrmsLeaveType_TenantId_Code");

            migrationBuilder.RenameIndex(
                name: "IX_LeaveRequestLine_LeaveTypeId",
                schema: "dbo",
                table: "hrmsLeaveRequestLine",
                newName: "IX_hrmsLeaveRequestLine_LeaveTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_LeaveRequestLine_LeaveRequestId",
                schema: "dbo",
                table: "hrmsLeaveRequestLine",
                newName: "IX_hrmsLeaveRequestLine_LeaveRequestId");

            migrationBuilder.RenameIndex(
                name: "IX_LeaveRequest_FiscalYearId",
                schema: "dbo",
                table: "hrmsLeaveRequest",
                newName: "IX_hrmsLeaveRequest_FiscalYearId");

            migrationBuilder.RenameIndex(
                name: "IX_LeaveRequest_EmployeeId_Status",
                schema: "dbo",
                table: "hrmsLeaveRequest",
                newName: "IX_hrmsLeaveRequest_EmployeeId_Status");

            migrationBuilder.RenameIndex(
                name: "IX_LeaveRequest_EmployeeId",
                schema: "dbo",
                table: "hrmsLeaveRequest",
                newName: "IX_hrmsLeaveRequest_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_LeaveBalanceTransaction_ReferenceId",
                schema: "dbo",
                table: "hrmsLeaveBalanceTransaction",
                newName: "IX_hrmsLeaveBalanceTransaction_ReferenceId");

            migrationBuilder.RenameIndex(
                name: "IX_LeaveBalanceTransaction_EmployeeId_LeaveTypeId_FiscalYearId",
                schema: "dbo",
                table: "hrmsLeaveBalanceTransaction",
                newName: "IX_hrmsLeaveBalanceTransaction_EmployeeId_LeaveTypeId_FiscalYearId");

            migrationBuilder.RenameIndex(
                name: "IX_LeaveBalance_TenantId_EmployeeId_LeaveTypeId_FiscalYearId",
                schema: "dbo",
                table: "hrmsLeaveBalance",
                newName: "IX_hrmsLeaveBalance_TenantId_EmployeeId_LeaveTypeId_FiscalYearId");

            migrationBuilder.RenameIndex(
                name: "IX_LeaveBalance_LeaveTypeId",
                schema: "dbo",
                table: "hrmsLeaveBalance",
                newName: "IX_hrmsLeaveBalance_LeaveTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_LeaveBalance_FiscalYearId",
                schema: "dbo",
                table: "hrmsLeaveBalance",
                newName: "IX_hrmsLeaveBalance_FiscalYearId");

            migrationBuilder.RenameIndex(
                name: "IX_LeaveBalance_EmployeeId",
                schema: "dbo",
                table: "hrmsLeaveBalance",
                newName: "IX_hrmsLeaveBalance_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_LearningPathStep_TrainingCourseId",
                schema: "dbo",
                table: "hrmsLearningPathStep",
                newName: "IX_hrmsLearningPathStep_TrainingCourseId");

            migrationBuilder.RenameIndex(
                name: "IX_LearningPathStep_TenantId_LearningPathId",
                schema: "dbo",
                table: "hrmsLearningPathStep",
                newName: "IX_hrmsLearningPathStep_TenantId_LearningPathId");

            migrationBuilder.RenameIndex(
                name: "IX_LearningPathStep_LearningPathId",
                schema: "dbo",
                table: "hrmsLearningPathStep",
                newName: "IX_hrmsLearningPathStep_LearningPathId");

            migrationBuilder.RenameIndex(
                name: "IX_LearningPath_TenantId_Name",
                schema: "dbo",
                table: "hrmsLearningPath",
                newName: "IX_hrmsLearningPath_TenantId_Name");

            migrationBuilder.RenameIndex(
                name: "IX_LearningPath_TargetPositionId",
                schema: "dbo",
                table: "hrmsLearningPath",
                newName: "IX_hrmsLearningPath_TargetPositionId");

            migrationBuilder.RenameIndex(
                name: "IX_LearningCommunityPost_TenantId_LearningCommunityId_ParentPostId",
                schema: "dbo",
                table: "hrmsLearningCommunityPost",
                newName: "IX_hrmsLearningCommunityPost_TenantId_LearningCommunityId_ParentPostId");

            migrationBuilder.RenameIndex(
                name: "IX_LearningCommunityPost_LearningCommunityId",
                schema: "dbo",
                table: "hrmsLearningCommunityPost",
                newName: "IX_hrmsLearningCommunityPost_LearningCommunityId");

            migrationBuilder.RenameIndex(
                name: "IX_LearningCommunityPost_EmployeeId",
                schema: "dbo",
                table: "hrmsLearningCommunityPost",
                newName: "IX_hrmsLearningCommunityPost_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_LearningCommunityMember_TenantId_LearningCommunityId_EmployeeId",
                schema: "dbo",
                table: "hrmsLearningCommunityMember",
                newName: "IX_hrmsLearningCommunityMember_TenantId_LearningCommunityId_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_LearningCommunityMember_LearningCommunityId",
                schema: "dbo",
                table: "hrmsLearningCommunityMember",
                newName: "IX_hrmsLearningCommunityMember_LearningCommunityId");

            migrationBuilder.RenameIndex(
                name: "IX_LearningCommunityMember_EmployeeId",
                schema: "dbo",
                table: "hrmsLearningCommunityMember",
                newName: "IX_hrmsLearningCommunityMember_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_LearningCommunity_TrainingCourseId",
                schema: "dbo",
                table: "hrmsLearningCommunity",
                newName: "IX_hrmsLearningCommunity_TrainingCourseId");

            migrationBuilder.RenameIndex(
                name: "IX_LearningCommunity_TenantId_Name",
                schema: "dbo",
                table: "hrmsLearningCommunity",
                newName: "IX_hrmsLearningCommunity_TenantId_Name");

            migrationBuilder.RenameIndex(
                name: "IX_KnowledgeTransfer_SuccessionCandidateId",
                schema: "dbo",
                table: "hrmsKnowledgeTransfer",
                newName: "IX_hrmsKnowledgeTransfer_SuccessionCandidateId");

            migrationBuilder.RenameIndex(
                name: "IX_KnowledgeTransfer_FromEmployeeId",
                schema: "dbo",
                table: "hrmsKnowledgeTransfer",
                newName: "IX_hrmsKnowledgeTransfer_FromEmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_JobRequisition_WorkLocationId",
                schema: "dbo",
                table: "hrmsJobRequisition",
                newName: "IX_hrmsJobRequisition_WorkLocationId");

            migrationBuilder.RenameIndex(
                name: "IX_JobRequisition_TenantId_Status",
                schema: "dbo",
                table: "hrmsJobRequisition",
                newName: "IX_hrmsJobRequisition_TenantId_Status");

            migrationBuilder.RenameIndex(
                name: "IX_JobRequisition_TenantId_RequisitionNumber",
                schema: "dbo",
                table: "hrmsJobRequisition",
                newName: "IX_hrmsJobRequisition_TenantId_RequisitionNumber");

            migrationBuilder.RenameIndex(
                name: "IX_JobRequisition_SalaryScaleId",
                schema: "dbo",
                table: "hrmsJobRequisition",
                newName: "IX_hrmsJobRequisition_SalaryScaleId");

            migrationBuilder.RenameIndex(
                name: "IX_JobRequisition_PositionClassId",
                schema: "dbo",
                table: "hrmsJobRequisition",
                newName: "IX_hrmsJobRequisition_PositionClassId");

            migrationBuilder.RenameIndex(
                name: "IX_JobRequisition_OrganizationUnitId",
                schema: "dbo",
                table: "hrmsJobRequisition",
                newName: "IX_hrmsJobRequisition_OrganizationUnitId");

            migrationBuilder.RenameIndex(
                name: "IX_JobRequisition_HiringRequestId",
                schema: "dbo",
                table: "hrmsJobRequisition",
                newName: "IX_hrmsJobRequisition_HiringRequestId");

            migrationBuilder.RenameIndex(
                name: "IX_JobOffer_TenantId_Status",
                schema: "dbo",
                table: "hrmsJobOffer",
                newName: "IX_hrmsJobOffer_TenantId_Status");

            migrationBuilder.RenameIndex(
                name: "IX_JobOffer_TenantId_OfferNumber",
                schema: "dbo",
                table: "hrmsJobOffer",
                newName: "IX_hrmsJobOffer_TenantId_OfferNumber");

            migrationBuilder.RenameIndex(
                name: "IX_JobOffer_SalaryScaleId",
                schema: "dbo",
                table: "hrmsJobOffer",
                newName: "IX_hrmsJobOffer_SalaryScaleId");

            migrationBuilder.RenameIndex(
                name: "IX_JobOffer_HiringManagerEmployeeId",
                schema: "dbo",
                table: "hrmsJobOffer",
                newName: "IX_hrmsJobOffer_HiringManagerEmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_JobOffer_HiredEmployeeId",
                schema: "dbo",
                table: "hrmsJobOffer",
                newName: "IX_hrmsJobOffer_HiredEmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_JobOffer_ApplicationId_CreatedAt",
                schema: "dbo",
                table: "hrmsJobOffer",
                newName: "IX_hrmsJobOffer_ApplicationId_CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_JobGrade_TenantId_Code",
                schema: "dbo",
                table: "hrmsJobGrade",
                newName: "IX_hrmsJobGrade_TenantId_Code");

            migrationBuilder.RenameIndex(
                name: "IX_JobCategory_TenantId_Code",
                schema: "dbo",
                table: "hrmsJobCategory",
                newName: "IX_hrmsJobCategory_TenantId_Code");

            migrationBuilder.RenameIndex(
                name: "IX_JobApplicationStageLog_ApplicationId",
                schema: "dbo",
                table: "hrmsJobApplicationStageLog",
                newName: "IX_hrmsJobApplicationStageLog_ApplicationId");

            migrationBuilder.RenameIndex(
                name: "IX_JobApplication_TenantId_Stage",
                schema: "dbo",
                table: "hrmsJobApplication",
                newName: "IX_hrmsJobApplication_TenantId_Stage");

            migrationBuilder.RenameIndex(
                name: "IX_JobApplication_TenantId_AppliedAt",
                schema: "dbo",
                table: "hrmsJobApplication",
                newName: "IX_hrmsJobApplication_TenantId_AppliedAt");

            migrationBuilder.RenameIndex(
                name: "IX_JobApplication_RequisitionId",
                schema: "dbo",
                table: "hrmsJobApplication",
                newName: "IX_hrmsJobApplication_RequisitionId");

            migrationBuilder.RenameIndex(
                name: "IX_JobApplication_CandidateId_RequisitionId",
                schema: "dbo",
                table: "hrmsJobApplication",
                newName: "IX_hrmsJobApplication_CandidateId_RequisitionId");

            migrationBuilder.RenameIndex(
                name: "IX_InterviewPanelist_InterviewId_EmployeeId",
                schema: "dbo",
                table: "hrmsInterviewPanelist",
                newName: "IX_hrmsInterviewPanelist_InterviewId_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_InterviewPanelist_EmployeeId",
                schema: "dbo",
                table: "hrmsInterviewPanelist",
                newName: "IX_hrmsInterviewPanelist_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_InterviewFeedback_PanelistId_CriterionId",
                schema: "dbo",
                table: "hrmsInterviewFeedback",
                newName: "IX_hrmsInterviewFeedback_PanelistId_CriterionId");

            migrationBuilder.RenameIndex(
                name: "IX_InterviewFeedback_PanelistId",
                schema: "dbo",
                table: "hrmsInterviewFeedback",
                newName: "IX_hrmsInterviewFeedback_PanelistId");

            migrationBuilder.RenameIndex(
                name: "IX_Interview_TenantId_Status",
                schema: "dbo",
                table: "hrmsInterview",
                newName: "IX_hrmsInterview_TenantId_Status");

            migrationBuilder.RenameIndex(
                name: "IX_Interview_ScheduledStart",
                schema: "dbo",
                table: "hrmsInterview",
                newName: "IX_hrmsInterview_ScheduledStart");

            migrationBuilder.RenameIndex(
                name: "IX_Interview_ApplicationId",
                schema: "dbo",
                table: "hrmsInterview",
                newName: "IX_hrmsInterview_ApplicationId");

            migrationBuilder.RenameIndex(
                name: "IX_InsurancePremiumSchedule_Status_DueDate",
                schema: "dbo",
                table: "hrmsInsurancePremiumSchedule",
                newName: "IX_hrmsInsurancePremiumSchedule_Status_DueDate");

            migrationBuilder.RenameIndex(
                name: "IX_InsurancePremiumSchedule_InsurancePolicyId_Installment",
                schema: "dbo",
                table: "hrmsInsurancePremiumSchedule",
                newName: "IX_hrmsInsurancePremiumSchedule_InsurancePolicyId_Installment");

            migrationBuilder.RenameIndex(
                name: "IX_InsurancePolicy_TenantId_PolicyNumber",
                schema: "dbo",
                table: "hrmsInsurancePolicy",
                newName: "IX_hrmsInsurancePolicy_TenantId_PolicyNumber");

            migrationBuilder.RenameIndex(
                name: "IX_InsurancePolicy_Status",
                schema: "dbo",
                table: "hrmsInsurancePolicy",
                newName: "IX_hrmsInsurancePolicy_Status");

            migrationBuilder.RenameIndex(
                name: "IX_InsuranceClaimAttachment_InsuranceClaimId",
                schema: "dbo",
                table: "hrmsInsuranceClaimAttachment",
                newName: "IX_hrmsInsuranceClaimAttachment_InsuranceClaimId");

            migrationBuilder.RenameIndex(
                name: "IX_InsuranceClaim_TenantId_ClaimNumber",
                schema: "dbo",
                table: "hrmsInsuranceClaim",
                newName: "IX_hrmsInsuranceClaim_TenantId_ClaimNumber");

            migrationBuilder.RenameIndex(
                name: "IX_InsuranceClaim_Status",
                schema: "dbo",
                table: "hrmsInsuranceClaim",
                newName: "IX_hrmsInsuranceClaim_Status");

            migrationBuilder.RenameIndex(
                name: "IX_InsuranceClaim_InsurancePolicyId",
                schema: "dbo",
                table: "hrmsInsuranceClaim",
                newName: "IX_hrmsInsuranceClaim_InsurancePolicyId");

            migrationBuilder.RenameIndex(
                name: "IX_InsuranceClaim_EmployeeId",
                schema: "dbo",
                table: "hrmsInsuranceClaim",
                newName: "IX_hrmsInsuranceClaim_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_ImprovementPlan_TenantId_EmployeeId",
                schema: "dbo",
                table: "hrmsImprovementPlan",
                newName: "IX_hrmsImprovementPlan_TenantId_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_ImprovementPlan_EmployeeId",
                schema: "dbo",
                table: "hrmsImprovementPlan",
                newName: "IX_hrmsImprovementPlan_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_ImprovementPlan_AppraisalId",
                schema: "dbo",
                table: "hrmsImprovementPlan",
                newName: "IX_hrmsImprovementPlan_AppraisalId");

            migrationBuilder.RenameIndex(
                name: "IX_Holiday_TenantId_Date",
                schema: "dbo",
                table: "hrmsHoliday",
                newName: "IX_hrmsHoliday_TenantId_Date");

            migrationBuilder.RenameIndex(
                name: "IX_HiringRequest_TenantId_Status",
                schema: "dbo",
                table: "hrmsHiringRequest",
                newName: "IX_hrmsHiringRequest_TenantId_Status");

            migrationBuilder.RenameIndex(
                name: "IX_HiringRequest_TenantId_RequestNumber",
                schema: "dbo",
                table: "hrmsHiringRequest",
                newName: "IX_hrmsHiringRequest_TenantId_RequestNumber");

            migrationBuilder.RenameIndex(
                name: "IX_HiringRequest_PositionClassId",
                schema: "dbo",
                table: "hrmsHiringRequest",
                newName: "IX_hrmsHiringRequest_PositionClassId");

            migrationBuilder.RenameIndex(
                name: "IX_HiringRequest_OrganizationUnitId",
                schema: "dbo",
                table: "hrmsHiringRequest",
                newName: "IX_hrmsHiringRequest_OrganizationUnitId");

            migrationBuilder.RenameIndex(
                name: "IX_GrievanceNote_TenantId_GrievanceId",
                schema: "dbo",
                table: "hrmsGrievanceNote",
                newName: "IX_hrmsGrievanceNote_TenantId_GrievanceId");

            migrationBuilder.RenameIndex(
                name: "IX_GrievanceNote_GrievanceId",
                schema: "dbo",
                table: "hrmsGrievanceNote",
                newName: "IX_hrmsGrievanceNote_GrievanceId");

            migrationBuilder.RenameIndex(
                name: "IX_Grievance_TenantId_Status",
                schema: "dbo",
                table: "hrmsGrievance",
                newName: "IX_hrmsGrievance_TenantId_Status");

            migrationBuilder.RenameIndex(
                name: "IX_Grievance_TenantId_EmployeeId",
                schema: "dbo",
                table: "hrmsGrievance",
                newName: "IX_hrmsGrievance_TenantId_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_Grievance_TenantId_AssignedToEmployeeId",
                schema: "dbo",
                table: "hrmsGrievance",
                newName: "IX_hrmsGrievance_TenantId_AssignedToEmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_Grievance_EmployeeId",
                schema: "dbo",
                table: "hrmsGrievance",
                newName: "IX_hrmsGrievance_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_GoalActionItem_EmployeeGoalId_SortOrder",
                schema: "dbo",
                table: "hrmsGoalActionItem",
                newName: "IX_hrmsGoalActionItem_EmployeeGoalId_SortOrder");

            migrationBuilder.RenameIndex(
                name: "IX_ExitInterview_TerminationId",
                schema: "dbo",
                table: "hrmsExitInterview",
                newName: "IX_hrmsExitInterview_TerminationId");

            migrationBuilder.RenameIndex(
                name: "IX_ExitInterview_TenantId_TerminationId",
                schema: "dbo",
                table: "hrmsExitInterview",
                newName: "IX_hrmsExitInterview_TenantId_TerminationId");

            migrationBuilder.RenameIndex(
                name: "IX_EmployeeTrainingCertificate_TrainingEnrollmentId",
                schema: "dbo",
                table: "hrmsEmployeeTrainingCertificate",
                newName: "IX_hrmsEmployeeTrainingCertificate_TrainingEnrollmentId");

            migrationBuilder.RenameIndex(
                name: "IX_EmployeeTrainingCertificate_TrainingCourseId",
                schema: "dbo",
                table: "hrmsEmployeeTrainingCertificate",
                newName: "IX_hrmsEmployeeTrainingCertificate_TrainingCourseId");

            migrationBuilder.RenameIndex(
                name: "IX_EmployeeTrainingCertificate_TenantId_ExpiresOn",
                schema: "dbo",
                table: "hrmsEmployeeTrainingCertificate",
                newName: "IX_hrmsEmployeeTrainingCertificate_TenantId_ExpiresOn");

            migrationBuilder.RenameIndex(
                name: "IX_EmployeeTrainingCertificate_TenantId_EmployeeId",
                schema: "dbo",
                table: "hrmsEmployeeTrainingCertificate",
                newName: "IX_hrmsEmployeeTrainingCertificate_TenantId_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_EmployeeTrainingCertificate_TenantId_CertificateNo",
                schema: "dbo",
                table: "hrmsEmployeeTrainingCertificate",
                newName: "IX_hrmsEmployeeTrainingCertificate_TenantId_CertificateNo");

            migrationBuilder.RenameIndex(
                name: "IX_EmployeeTrainingCertificate_EmployeeId",
                schema: "dbo",
                table: "hrmsEmployeeTrainingCertificate",
                newName: "IX_hrmsEmployeeTrainingCertificate_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_EmployeeTermination_Status",
                schema: "dbo",
                table: "hrmsEmployeeTermination",
                newName: "IX_hrmsEmployeeTermination_Status");

            migrationBuilder.RenameIndex(
                name: "IX_EmployeeTermination_EmployeeId",
                schema: "dbo",
                table: "hrmsEmployeeTermination",
                newName: "IX_hrmsEmployeeTermination_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_EmployeeRecognition_TenantId_IsPublic_RecognizedOn",
                schema: "dbo",
                table: "hrmsEmployeeRecognition",
                newName: "IX_hrmsEmployeeRecognition_TenantId_IsPublic_RecognizedOn");

            migrationBuilder.RenameIndex(
                name: "IX_EmployeeRecognition_TenantId_EmployeeId",
                schema: "dbo",
                table: "hrmsEmployeeRecognition",
                newName: "IX_hrmsEmployeeRecognition_TenantId_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_EmployeeRecognition_RecognitionBadgeId",
                schema: "dbo",
                table: "hrmsEmployeeRecognition",
                newName: "IX_hrmsEmployeeRecognition_RecognitionBadgeId");

            migrationBuilder.RenameIndex(
                name: "IX_EmployeeRecognition_EmployeeId",
                schema: "dbo",
                table: "hrmsEmployeeRecognition",
                newName: "IX_hrmsEmployeeRecognition_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_EmployeeMovement_ToSalaryScaleId",
                schema: "dbo",
                table: "hrmsEmployeeMovement",
                newName: "IX_hrmsEmployeeMovement_ToSalaryScaleId");

            migrationBuilder.RenameIndex(
                name: "IX_EmployeeMovement_Status_EffectiveDate",
                schema: "dbo",
                table: "hrmsEmployeeMovement",
                newName: "IX_hrmsEmployeeMovement_Status_EffectiveDate");

            migrationBuilder.RenameIndex(
                name: "IX_EmployeeMovement_EmployeeId",
                schema: "dbo",
                table: "hrmsEmployeeMovement",
                newName: "IX_hrmsEmployeeMovement_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_EmployeeGuarantee_TenantId_Status",
                schema: "dbo",
                table: "hrmsEmployeeGuarantee",
                newName: "IX_hrmsEmployeeGuarantee_TenantId_Status");

            migrationBuilder.RenameIndex(
                name: "IX_EmployeeGuarantee_TenantId_EndDate",
                schema: "dbo",
                table: "hrmsEmployeeGuarantee",
                newName: "IX_hrmsEmployeeGuarantee_TenantId_EndDate");

            migrationBuilder.RenameIndex(
                name: "IX_EmployeeGuarantee_TenantId_EmployeeId",
                schema: "dbo",
                table: "hrmsEmployeeGuarantee",
                newName: "IX_hrmsEmployeeGuarantee_TenantId_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_EmployeeGuarantee_EmployeeId",
                schema: "dbo",
                table: "hrmsEmployeeGuarantee",
                newName: "IX_hrmsEmployeeGuarantee_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_EmployeeGoal_TenantId_EmployeeId_ReviewCycleId",
                schema: "dbo",
                table: "hrmsEmployeeGoal",
                newName: "IX_hrmsEmployeeGoal_TenantId_EmployeeId_ReviewCycleId");

            migrationBuilder.RenameIndex(
                name: "IX_EmployeeGoal_ReviewCycleId",
                schema: "dbo",
                table: "hrmsEmployeeGoal",
                newName: "IX_hrmsEmployeeGoal_ReviewCycleId");

            migrationBuilder.RenameIndex(
                name: "IX_EmployeeGoal_OrganizationalObjectiveId",
                schema: "dbo",
                table: "hrmsEmployeeGoal",
                newName: "IX_hrmsEmployeeGoal_OrganizationalObjectiveId");

            migrationBuilder.RenameIndex(
                name: "IX_EmployeeGoal_EmployeeId",
                schema: "dbo",
                table: "hrmsEmployeeGoal",
                newName: "IX_hrmsEmployeeGoal_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_EmployeeFieldValue_OwnerType_OwnerId_FieldDefinitionId",
                schema: "dbo",
                table: "hrmsEmployeeFieldValue",
                newName: "IX_hrmsEmployeeFieldValue_OwnerType_OwnerId_FieldDefinitionId");

            migrationBuilder.RenameIndex(
                name: "IX_EmployeeFieldValue_FieldDefinitionId",
                schema: "dbo",
                table: "hrmsEmployeeFieldValue",
                newName: "IX_hrmsEmployeeFieldValue_FieldDefinitionId");

            migrationBuilder.RenameIndex(
                name: "IX_EmployeeFieldDefinition_TenantId_OwnerType_Name",
                schema: "dbo",
                table: "hrmsEmployeeFieldDefinition",
                newName: "IX_hrmsEmployeeFieldDefinition_TenantId_OwnerType_Name");

            migrationBuilder.RenameIndex(
                name: "IX_EmployeeExperience_PersonId",
                schema: "dbo",
                table: "hrmsEmployeeExperience",
                newName: "IX_hrmsEmployeeExperience_PersonId");

            migrationBuilder.RenameIndex(
                name: "IX_EmployeeEducation_PersonId",
                schema: "dbo",
                table: "hrmsEmployeeEducation",
                newName: "IX_hrmsEmployeeEducation_PersonId");

            migrationBuilder.RenameIndex(
                name: "IX_EmployeeDocument_OwnerType_OwnerId",
                schema: "dbo",
                table: "hrmsEmployeeDocument",
                newName: "IX_hrmsEmployeeDocument_OwnerType_OwnerId");

            migrationBuilder.RenameIndex(
                name: "IX_EmployeeDocument_EmployeeId",
                schema: "dbo",
                table: "hrmsEmployeeDocument",
                newName: "IX_hrmsEmployeeDocument_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_EmployeeDependent_RelatedEmployeeId",
                schema: "dbo",
                table: "hrmsEmployeeDependent",
                newName: "IX_hrmsEmployeeDependent_RelatedEmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_EmployeeDependent_PersonId",
                schema: "dbo",
                table: "hrmsEmployeeDependent",
                newName: "IX_hrmsEmployeeDependent_PersonId");

            migrationBuilder.RenameIndex(
                name: "IX_EmployeeCareerPathStepProgress_EmployeeCareerPathId",
                schema: "dbo",
                table: "hrmsEmployeeCareerPathStepProgress",
                newName: "IX_hrmsEmployeeCareerPathStepProgress_EmployeeCareerPathId");

            migrationBuilder.RenameIndex(
                name: "IX_EmployeeCareerPath_TenantId_EmployeeId_CareerPathId",
                schema: "dbo",
                table: "hrmsEmployeeCareerPath",
                newName: "IX_hrmsEmployeeCareerPath_TenantId_EmployeeId_CareerPathId");

            migrationBuilder.RenameIndex(
                name: "IX_EmployeeCareerPath_TenantId_EmployeeId",
                schema: "dbo",
                table: "hrmsEmployeeCareerPath",
                newName: "IX_hrmsEmployeeCareerPath_TenantId_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_EmployeeCareerPath_TenantId_CareerPathId",
                schema: "dbo",
                table: "hrmsEmployeeCareerPath",
                newName: "IX_hrmsEmployeeCareerPath_TenantId_CareerPathId");

            migrationBuilder.RenameIndex(
                name: "IX_EmployeeCareerPath_EmployeeId",
                schema: "dbo",
                table: "hrmsEmployeeCareerPath",
                newName: "IX_hrmsEmployeeCareerPath_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_EmployeeCareerPath_CareerPathId",
                schema: "dbo",
                table: "hrmsEmployeeCareerPath",
                newName: "IX_hrmsEmployeeCareerPath_CareerPathId");

            migrationBuilder.RenameIndex(
                name: "IX_EmployeeBenefitEnrollment_EmployeeId",
                schema: "dbo",
                table: "hrmsEmployeeBenefitEnrollment",
                newName: "IX_hrmsEmployeeBenefitEnrollment_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_EmployeeBenefitEnrollment_BenefitPlanId",
                schema: "dbo",
                table: "hrmsEmployeeBenefitEnrollment",
                newName: "IX_hrmsEmployeeBenefitEnrollment_BenefitPlanId");

            migrationBuilder.RenameIndex(
                name: "IX_EmployeeAllowance_EmployeeId",
                schema: "dbo",
                table: "hrmsEmployeeAllowance",
                newName: "IX_hrmsEmployeeAllowance_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_EmployeeAllowance_AllowanceTypeId",
                schema: "dbo",
                table: "hrmsEmployeeAllowance",
                newName: "IX_hrmsEmployeeAllowance_AllowanceTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_Employee_TenantId_PositionId_EmployeeNumber",
                schema: "dbo",
                table: "hrmsEmployee",
                newName: "IX_hrmsEmployee_TenantId_PositionId_EmployeeNumber");

            migrationBuilder.RenameIndex(
                name: "IX_Employee_TenantId_EmployeeNumber",
                schema: "dbo",
                table: "hrmsEmployee",
                newName: "IX_hrmsEmployee_TenantId_EmployeeNumber");

            migrationBuilder.RenameIndex(
                name: "IX_Employee_TenantId_BranchId_EmploymentStatus",
                schema: "dbo",
                table: "hrmsEmployee",
                newName: "IX_hrmsEmployee_TenantId_BranchId_EmploymentStatus");

            migrationBuilder.RenameIndex(
                name: "IX_Employee_SalaryScaleId",
                schema: "dbo",
                table: "hrmsEmployee",
                newName: "IX_hrmsEmployee_SalaryScaleId");

            migrationBuilder.RenameIndex(
                name: "IX_Employee_PositionId",
                schema: "dbo",
                table: "hrmsEmployee",
                newName: "IX_hrmsEmployee_PositionId");

            migrationBuilder.RenameIndex(
                name: "IX_Employee_PersonId",
                schema: "dbo",
                table: "hrmsEmployee",
                newName: "IX_hrmsEmployee_PersonId");

            migrationBuilder.RenameIndex(
                name: "IX_Employee_EmploymentStatus_IsProbation",
                schema: "dbo",
                table: "hrmsEmployee",
                newName: "IX_hrmsEmployee_EmploymentStatus_IsProbation");

            migrationBuilder.RenameIndex(
                name: "IX_Employee_EmploymentStatus",
                schema: "dbo",
                table: "hrmsEmployee",
                newName: "IX_hrmsEmployee_EmploymentStatus");

            migrationBuilder.RenameIndex(
                name: "IX_Employee_DateOfBirth",
                schema: "dbo",
                table: "hrmsEmployee",
                newName: "IX_hrmsEmployee_DateOfBirth");

            migrationBuilder.RenameIndex(
                name: "IX_Employee_BranchId",
                schema: "dbo",
                table: "hrmsEmployee",
                newName: "IX_hrmsEmployee_BranchId");

            migrationBuilder.RenameIndex(
                name: "IX_DynamicFormRecord_DynamicFormId_OwnerType_OwnerId_CreatedAt",
                schema: "dbo",
                table: "hrmsDynamicFormRecord",
                newName: "IX_hrmsDynamicFormRecord_DynamicFormId_OwnerType_OwnerId_CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_DynamicFormField_DynamicFormId_Name",
                schema: "dbo",
                table: "hrmsDynamicFormField",
                newName: "IX_hrmsDynamicFormField_DynamicFormId_Name");

            migrationBuilder.RenameIndex(
                name: "IX_DynamicForm_TenantId_Module_Name",
                schema: "dbo",
                table: "hrmsDynamicForm",
                newName: "IX_hrmsDynamicForm_TenantId_Module_Name");

            migrationBuilder.RenameIndex(
                name: "IX_DocumentTemplate_TenantId_Name",
                schema: "dbo",
                table: "hrmsDocumentTemplate",
                newName: "IX_hrmsDocumentTemplate_TenantId_Name");

            migrationBuilder.RenameIndex(
                name: "IX_DisciplinaryMeasure_Status",
                schema: "dbo",
                table: "hrmsDisciplinaryMeasure",
                newName: "IX_hrmsDisciplinaryMeasure_Status");

            migrationBuilder.RenameIndex(
                name: "IX_DisciplinaryMeasure_EmployeeId_Status",
                schema: "dbo",
                table: "hrmsDisciplinaryMeasure",
                newName: "IX_hrmsDisciplinaryMeasure_EmployeeId_Status");

            migrationBuilder.RenameIndex(
                name: "IX_DevelopmentPlan_TenantId_EmployeeId",
                schema: "dbo",
                table: "hrmsDevelopmentPlan",
                newName: "IX_hrmsDevelopmentPlan_TenantId_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_DevelopmentPlan_EmployeeId",
                schema: "dbo",
                table: "hrmsDevelopmentPlan",
                newName: "IX_hrmsDevelopmentPlan_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_DevelopmentPlan_AppraisalId",
                schema: "dbo",
                table: "hrmsDevelopmentPlan",
                newName: "IX_hrmsDevelopmentPlan_AppraisalId");

            migrationBuilder.RenameIndex(
                name: "IX_DevelopmentAction_DevelopmentPlanId_SortOrder",
                schema: "dbo",
                table: "hrmsDevelopmentAction",
                newName: "IX_hrmsDevelopmentAction_DevelopmentPlanId_SortOrder");

            migrationBuilder.RenameIndex(
                name: "IX_DevelopmentAction_CompetencyId",
                schema: "dbo",
                table: "hrmsDevelopmentAction",
                newName: "IX_hrmsDevelopmentAction_CompetencyId");

            migrationBuilder.RenameIndex(
                name: "IX_CriticalPosition_TenantId_PositionId",
                schema: "dbo",
                table: "hrmsCriticalPosition",
                newName: "IX_hrmsCriticalPosition_TenantId_PositionId");

            migrationBuilder.RenameIndex(
                name: "IX_CriticalPosition_TenantId_IsActive",
                schema: "dbo",
                table: "hrmsCriticalPosition",
                newName: "IX_hrmsCriticalPosition_TenantId_IsActive");

            migrationBuilder.RenameIndex(
                name: "IX_CriticalPosition_PositionId",
                schema: "dbo",
                table: "hrmsCriticalPosition",
                newName: "IX_hrmsCriticalPosition_PositionId");

            migrationBuilder.RenameIndex(
                name: "IX_CriterionEvaluator_EmployeeId",
                schema: "dbo",
                table: "hrmsCriterionEvaluator",
                newName: "IX_hrmsCriterionEvaluator_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_CriterionEvaluator_CriterionId",
                schema: "dbo",
                table: "hrmsCriterionEvaluator",
                newName: "IX_hrmsCriterionEvaluator_CriterionId");

            migrationBuilder.RenameIndex(
                name: "IX_CompetencyCategory_TenantId_Name",
                schema: "dbo",
                table: "hrmsCompetencyCategory",
                newName: "IX_hrmsCompetencyCategory_TenantId_Name");

            migrationBuilder.RenameIndex(
                name: "IX_Competency_TenantId_Name",
                schema: "dbo",
                table: "hrmsCompetency",
                newName: "IX_hrmsCompetency_TenantId_Name");

            migrationBuilder.RenameIndex(
                name: "IX_Competency_CompetencyCategoryId",
                schema: "dbo",
                table: "hrmsCompetency",
                newName: "IX_hrmsCompetency_CompetencyCategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_CompensationRequest_Status",
                schema: "dbo",
                table: "hrmsCompensationRequest",
                newName: "IX_hrmsCompensationRequest_Status");

            migrationBuilder.RenameIndex(
                name: "IX_CompensationRequest_EmployeeId",
                schema: "dbo",
                table: "hrmsCompensationRequest",
                newName: "IX_hrmsCompensationRequest_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_CompanyProfile_TenantId",
                schema: "dbo",
                table: "hrmsCompanyProfile",
                newName: "IX_hrmsCompanyProfile_TenantId");

            migrationBuilder.RenameIndex(
                name: "IX_CompanyAsset_TenantId_Status",
                schema: "dbo",
                table: "hrmsCompanyAsset",
                newName: "IX_hrmsCompanyAsset_TenantId_Status");

            migrationBuilder.RenameIndex(
                name: "IX_CompanyAsset_TenantId_AssignedToEmployeeId",
                schema: "dbo",
                table: "hrmsCompanyAsset",
                newName: "IX_hrmsCompanyAsset_TenantId_AssignedToEmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_CompanyAsset_AssignedToEmployeeId",
                schema: "dbo",
                table: "hrmsCompanyAsset",
                newName: "IX_hrmsCompanyAsset_AssignedToEmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_CommunityPostReaction_TenantId_LearningCommunityPostId_EmployeeId",
                schema: "dbo",
                table: "hrmsCommunityPostReaction",
                newName: "IX_hrmsCommunityPostReaction_TenantId_LearningCommunityPostId_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_CommunityPostReaction_LearningCommunityPostId",
                schema: "dbo",
                table: "hrmsCommunityPostReaction",
                newName: "IX_hrmsCommunityPostReaction_LearningCommunityPostId");

            migrationBuilder.RenameIndex(
                name: "IX_CommunityPostReaction_EmployeeId",
                schema: "dbo",
                table: "hrmsCommunityPostReaction",
                newName: "IX_hrmsCommunityPostReaction_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_ClearanceDepartmentApprover_DepartmentId",
                schema: "dbo",
                table: "hrmsClearanceDepartmentApprover",
                newName: "IX_hrmsClearanceDepartmentApprover_DepartmentId");

            migrationBuilder.RenameIndex(
                name: "IX_ClearanceDepartmentApprover_ApproverType_ApproverId",
                schema: "dbo",
                table: "hrmsClearanceDepartmentApprover",
                newName: "IX_hrmsClearanceDepartmentApprover_ApproverType_ApproverId");

            migrationBuilder.RenameIndex(
                name: "IX_ClearanceDepartment_TenantId_Name",
                schema: "dbo",
                table: "hrmsClearanceDepartment",
                newName: "IX_hrmsClearanceDepartment_TenantId_Name");

            migrationBuilder.RenameIndex(
                name: "IX_CareerPathStepCompetency_CompetencyId",
                schema: "dbo",
                table: "hrmsCareerPathStepCompetency",
                newName: "IX_hrmsCareerPathStepCompetency_CompetencyId");

            migrationBuilder.RenameIndex(
                name: "IX_CareerPathStepCompetency_CareerPathStepId_CompetencyId",
                schema: "dbo",
                table: "hrmsCareerPathStepCompetency",
                newName: "IX_hrmsCareerPathStepCompetency_CareerPathStepId_CompetencyId");

            migrationBuilder.RenameIndex(
                name: "IX_CareerPathStep_PositionClassId",
                schema: "dbo",
                table: "hrmsCareerPathStep",
                newName: "IX_hrmsCareerPathStep_PositionClassId");

            migrationBuilder.RenameIndex(
                name: "IX_CareerPathStep_JobGradeId",
                schema: "dbo",
                table: "hrmsCareerPathStep",
                newName: "IX_hrmsCareerPathStep_JobGradeId");

            migrationBuilder.RenameIndex(
                name: "IX_CareerPathStep_CareerPathId_StepOrder",
                schema: "dbo",
                table: "hrmsCareerPathStep",
                newName: "IX_hrmsCareerPathStep_CareerPathId_StepOrder");

            migrationBuilder.RenameIndex(
                name: "IX_CareerPathChangeRequest_TenantId_EmployeeId_Status",
                schema: "dbo",
                table: "hrmsCareerPathChangeRequest",
                newName: "IX_hrmsCareerPathChangeRequest_TenantId_EmployeeId_Status");

            migrationBuilder.RenameIndex(
                name: "IX_CareerPathChangeRequest_RequestedCareerPathId",
                schema: "dbo",
                table: "hrmsCareerPathChangeRequest",
                newName: "IX_hrmsCareerPathChangeRequest_RequestedCareerPathId");

            migrationBuilder.RenameIndex(
                name: "IX_CareerPathChangeRequest_EmployeeId",
                schema: "dbo",
                table: "hrmsCareerPathChangeRequest",
                newName: "IX_hrmsCareerPathChangeRequest_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_CareerPathChangeRequest_CurrentCareerPathId",
                schema: "dbo",
                table: "hrmsCareerPathChangeRequest",
                newName: "IX_hrmsCareerPathChangeRequest_CurrentCareerPathId");

            migrationBuilder.RenameIndex(
                name: "IX_CareerPath_TenantId_Code",
                schema: "dbo",
                table: "hrmsCareerPath",
                newName: "IX_hrmsCareerPath_TenantId_Code");

            migrationBuilder.RenameIndex(
                name: "IX_CandidateDocument_CandidateId_DocumentType",
                schema: "dbo",
                table: "hrmsCandidateDocument",
                newName: "IX_hrmsCandidateDocument_CandidateId_DocumentType");

            migrationBuilder.RenameIndex(
                name: "IX_Candidate_TenantId_IsInTalentPool",
                schema: "dbo",
                table: "hrmsCandidate",
                newName: "IX_hrmsCandidate_TenantId_IsInTalentPool");

            migrationBuilder.RenameIndex(
                name: "IX_Candidate_TenantId_CandidateNumber",
                schema: "dbo",
                table: "hrmsCandidate",
                newName: "IX_hrmsCandidate_TenantId_CandidateNumber");

            migrationBuilder.RenameIndex(
                name: "IX_Candidate_PersonId",
                schema: "dbo",
                table: "hrmsCandidate",
                newName: "IX_hrmsCandidate_PersonId");

            migrationBuilder.RenameIndex(
                name: "IX_Candidate_InternalEmployeeId",
                schema: "dbo",
                table: "hrmsCandidate",
                newName: "IX_hrmsCandidate_InternalEmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_Candidate_HiredEmployeeId",
                schema: "dbo",
                table: "hrmsCandidate",
                newName: "IX_hrmsCandidate_HiredEmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_Candidate_Email",
                schema: "dbo",
                table: "hrmsCandidate",
                newName: "IX_hrmsCandidate_Email");

            migrationBuilder.RenameIndex(
                name: "IX_CalibrationSession_TenantId_ReviewCycleId",
                schema: "dbo",
                table: "hrmsCalibrationSession",
                newName: "IX_hrmsCalibrationSession_TenantId_ReviewCycleId");

            migrationBuilder.RenameIndex(
                name: "IX_CalibrationSession_ReviewCycleId",
                schema: "dbo",
                table: "hrmsCalibrationSession",
                newName: "IX_hrmsCalibrationSession_ReviewCycleId");

            migrationBuilder.RenameIndex(
                name: "IX_CalibrationSession_OrganizationUnitId",
                schema: "dbo",
                table: "hrmsCalibrationSession",
                newName: "IX_hrmsCalibrationSession_OrganizationUnitId");

            migrationBuilder.RenameIndex(
                name: "IX_CalibrationItem_CalibrationSessionId",
                schema: "dbo",
                table: "hrmsCalibrationItem",
                newName: "IX_hrmsCalibrationItem_CalibrationSessionId");

            migrationBuilder.RenameIndex(
                name: "IX_CalibrationItem_AppraisalId",
                schema: "dbo",
                table: "hrmsCalibrationItem",
                newName: "IX_hrmsCalibrationItem_AppraisalId");

            migrationBuilder.RenameIndex(
                name: "IX_Branch_TenantId_Code",
                schema: "dbo",
                table: "hrmsBranch",
                newName: "IX_hrmsBranch_TenantId_Code");

            migrationBuilder.RenameIndex(
                name: "IX_Branch_ParentId",
                schema: "dbo",
                table: "hrmsBranch",
                newName: "IX_hrmsBranch_ParentId");

            migrationBuilder.RenameIndex(
                name: "IX_BenefitPlan_TenantId_Name",
                schema: "dbo",
                table: "hrmsBenefitPlan",
                newName: "IX_hrmsBenefitPlan_TenantId_Name");

            migrationBuilder.RenameIndex(
                name: "IX_AwardCategory_TenantId_Name",
                schema: "dbo",
                table: "hrmsAwardCategory",
                newName: "IX_hrmsAwardCategory_TenantId_Name");

            migrationBuilder.RenameIndex(
                name: "IX_AuditLog_EntityType_EntityId",
                schema: "dbo",
                table: "hrmsAuditLog",
                newName: "IX_hrmsAuditLog_EntityType_EntityId");

            migrationBuilder.RenameIndex(
                name: "IX_AuditLog_CreatedAt",
                schema: "dbo",
                table: "hrmsAuditLog",
                newName: "IX_hrmsAuditLog_CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_AuditLog_BranchId",
                schema: "dbo",
                table: "hrmsAuditLog",
                newName: "IX_hrmsAuditLog_BranchId");

            migrationBuilder.RenameIndex(
                name: "IX_AuditLog_Action",
                schema: "dbo",
                table: "hrmsAuditLog",
                newName: "IX_hrmsAuditLog_Action");

            migrationBuilder.RenameIndex(
                name: "IX_AppraisalTemplate_TenantId_Name",
                schema: "dbo",
                table: "hrmsAppraisalTemplate",
                newName: "IX_hrmsAppraisalTemplate_TenantId_Name");

            migrationBuilder.RenameIndex(
                name: "IX_AppraisalPeerReview_PeerEmployeeId",
                schema: "dbo",
                table: "hrmsAppraisalPeerReview",
                newName: "IX_hrmsAppraisalPeerReview_PeerEmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_AppraisalPeerReview_AppraisalId_PeerEmployeeId",
                schema: "dbo",
                table: "hrmsAppraisalPeerReview",
                newName: "IX_hrmsAppraisalPeerReview_AppraisalId_PeerEmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_AppraisalGoal_AppraisalId_SortOrder",
                schema: "dbo",
                table: "hrmsAppraisalGoal",
                newName: "IX_hrmsAppraisalGoal_AppraisalId_SortOrder");

            migrationBuilder.RenameIndex(
                name: "IX_AppraisalCompetency_AppraisalId_SortOrder",
                schema: "dbo",
                table: "hrmsAppraisalCompetency",
                newName: "IX_hrmsAppraisalCompetency_AppraisalId_SortOrder");

            migrationBuilder.RenameIndex(
                name: "IX_AppraisalAppeal_TenantId_Status",
                schema: "dbo",
                table: "hrmsAppraisalAppeal",
                newName: "IX_hrmsAppraisalAppeal_TenantId_Status");

            migrationBuilder.RenameIndex(
                name: "IX_AppraisalAppeal_EmployeeId",
                schema: "dbo",
                table: "hrmsAppraisalAppeal",
                newName: "IX_hrmsAppraisalAppeal_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_AppraisalAppeal_AppraisalId",
                schema: "dbo",
                table: "hrmsAppraisalAppeal",
                newName: "IX_hrmsAppraisalAppeal_AppraisalId");

            migrationBuilder.RenameIndex(
                name: "IX_Appraisal_TenantId_ReviewCycleId_Stage",
                schema: "dbo",
                table: "hrmsAppraisal",
                newName: "IX_hrmsAppraisal_TenantId_ReviewCycleId_Stage");

            migrationBuilder.RenameIndex(
                name: "IX_Appraisal_TenantId_EmployeeId_ReviewCycleId",
                schema: "dbo",
                table: "hrmsAppraisal",
                newName: "IX_hrmsAppraisal_TenantId_EmployeeId_ReviewCycleId");

            migrationBuilder.RenameIndex(
                name: "IX_Appraisal_ReviewCycleId",
                schema: "dbo",
                table: "hrmsAppraisal",
                newName: "IX_hrmsAppraisal_ReviewCycleId");

            migrationBuilder.RenameIndex(
                name: "IX_Appraisal_EmployeeId",
                schema: "dbo",
                table: "hrmsAppraisal",
                newName: "IX_hrmsAppraisal_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_ApplicationCriterionScore_ApplicationId_CriterionId",
                schema: "dbo",
                table: "hrmsApplicationCriterionScore",
                newName: "IX_hrmsApplicationCriterionScore_ApplicationId_CriterionId");

            migrationBuilder.RenameIndex(
                name: "IX_AnnualLeaveSetting_TenantId_FiscalYearId",
                schema: "dbo",
                table: "hrmsAnnualLeaveSetting",
                newName: "IX_hrmsAnnualLeaveSetting_TenantId_FiscalYearId");

            migrationBuilder.RenameIndex(
                name: "IX_AnnualLeaveSetting_FiscalYearId",
                schema: "dbo",
                table: "hrmsAnnualLeaveSetting",
                newName: "IX_hrmsAnnualLeaveSetting_FiscalYearId");

            migrationBuilder.RenameIndex(
                name: "IX_AnnualLeaveHeader_EmployeeId_Status",
                schema: "dbo",
                table: "hrmsAnnualLeaveHeader",
                newName: "IX_hrmsAnnualLeaveHeader_EmployeeId_Status");

            migrationBuilder.RenameIndex(
                name: "IX_AnnualLeaveHeader_EmployeeId",
                schema: "dbo",
                table: "hrmsAnnualLeaveHeader",
                newName: "IX_hrmsAnnualLeaveHeader_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_AnnualLeaveHeader_AnnualLeaveLedgerId",
                schema: "dbo",
                table: "hrmsAnnualLeaveHeader",
                newName: "IX_hrmsAnnualLeaveHeader_AnnualLeaveLedgerId");

            migrationBuilder.RenameIndex(
                name: "IX_AnnualLeaveDetail_AnnualLeaveHeaderId_StartDate_EndDate",
                schema: "dbo",
                table: "hrmsAnnualLeaveDetail",
                newName: "IX_hrmsAnnualLeaveDetail_AnnualLeaveHeaderId_StartDate_EndDate");

            migrationBuilder.RenameIndex(
                name: "IX_AnnualLeaveDetail_AnnualLeaveHeaderId",
                schema: "dbo",
                table: "hrmsAnnualLeaveDetail",
                newName: "IX_hrmsAnnualLeaveDetail_AnnualLeaveHeaderId");

            migrationBuilder.RenameIndex(
                name: "IX_Announcement_TenantId_IsActive_PublishFrom",
                schema: "dbo",
                table: "hrmsAnnouncement",
                newName: "IX_hrmsAnnouncement_TenantId_IsActive_PublishFrom");

            migrationBuilder.RenameIndex(
                name: "IX_Announcement_OrganizationUnitId",
                schema: "dbo",
                table: "hrmsAnnouncement",
                newName: "IX_hrmsAnnouncement_OrganizationUnitId");

            migrationBuilder.RenameIndex(
                name: "IX_Announcement_BranchId",
                schema: "dbo",
                table: "hrmsAnnouncement",
                newName: "IX_hrmsAnnouncement_BranchId");

            migrationBuilder.RenameIndex(
                name: "IX_AllowanceType_TenantId_Name",
                schema: "dbo",
                table: "hrmsAllowanceType",
                newName: "IX_hrmsAllowanceType_TenantId_Name");

            migrationBuilder.RenameIndex(
                name: "IX_Achievement_TenantId_EmployeeId",
                schema: "dbo",
                table: "hrmsAchievement",
                newName: "IX_hrmsAchievement_TenantId_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_Achievement_EmployeeId",
                schema: "dbo",
                table: "hrmsAchievement",
                newName: "IX_hrmsAchievement_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_Achievement_AppraisalId",
                schema: "dbo",
                table: "hrmsAchievement",
                newName: "IX_hrmsAchievement_AppraisalId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsWorkWeekConfiguration",
                schema: "dbo",
                table: "hrmsWorkWeekConfiguration",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsWorkLocation",
                schema: "dbo",
                table: "hrmsWorkLocation",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsWorkforcePlanLine",
                schema: "dbo",
                table: "hrmsWorkforcePlanLine",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsWorkforcePlan",
                schema: "dbo",
                table: "hrmsWorkforcePlan",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsWorkflowStepApprover",
                schema: "dbo",
                table: "hrmsWorkflowStepApprover",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsWorkflowStep",
                schema: "dbo",
                table: "hrmsWorkflowStep",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsWorkflowInstance",
                schema: "dbo",
                table: "hrmsWorkflowInstance",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsWorkflowDefinition",
                schema: "dbo",
                table: "hrmsWorkflowDefinition",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsWorkflowActionLog",
                schema: "dbo",
                table: "hrmsWorkflowActionLog",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsTripRequest",
                schema: "dbo",
                table: "hrmsTripRequest",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsTripExpense",
                schema: "dbo",
                table: "hrmsTripExpense",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsTripBudget",
                schema: "dbo",
                table: "hrmsTripBudget",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsTrainingSession",
                schema: "dbo",
                table: "hrmsTrainingSession",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsTrainingProviderPayment",
                schema: "dbo",
                table: "hrmsTrainingProviderPayment",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsTrainingNeed",
                schema: "dbo",
                table: "hrmsTrainingNeed",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsTrainingEnrollment",
                schema: "dbo",
                table: "hrmsTrainingEnrollment",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsTrainingCourse",
                schema: "dbo",
                table: "hrmsTrainingCourse",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsTrainingCategory",
                schema: "dbo",
                table: "hrmsTrainingCategory",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsTrainingBudget",
                schema: "dbo",
                table: "hrmsTrainingBudget",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsTerminationSettlement",
                schema: "dbo",
                table: "hrmsTerminationSettlement",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsTerminationClearance",
                schema: "dbo",
                table: "hrmsTerminationClearance",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsTerminationAssetRecovery",
                schema: "dbo",
                table: "hrmsTerminationAssetRecovery",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsTaxBracket",
                schema: "dbo",
                table: "hrmsTaxBracket",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsTalentReview",
                schema: "dbo",
                table: "hrmsTalentReview",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsTalentRating",
                schema: "dbo",
                table: "hrmsTalentRating",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsTalentAssessment",
                schema: "dbo",
                table: "hrmsTalentAssessment",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsSurveyResponse",
                schema: "dbo",
                table: "hrmsSurveyResponse",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsSurveyCompletion",
                schema: "dbo",
                table: "hrmsSurveyCompletion",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsSurvey",
                schema: "dbo",
                table: "hrmsSurvey",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsSuggestion",
                schema: "dbo",
                table: "hrmsSuggestion",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsSuccessionPlan",
                schema: "dbo",
                table: "hrmsSuccessionPlan",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsSuccessionDevelopmentAction",
                schema: "dbo",
                table: "hrmsSuccessionDevelopmentAction",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsSuccessionCandidate",
                schema: "dbo",
                table: "hrmsSuccessionCandidate",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_coreSubsystem",
                schema: "dbo",
                table: "coreSubsystem",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_lupStep",
                schema: "Core",
                table: "lupStep",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsSettlementLine",
                schema: "dbo",
                table: "hrmsSettlementLine",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_coreSalaryScale",
                schema: "Core",
                table: "coreSalaryScale",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsSalaryRevisionLine",
                schema: "dbo",
                table: "hrmsSalaryRevisionLine",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsSalaryRevisionBand",
                schema: "dbo",
                table: "hrmsSalaryRevisionBand",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsSalaryRevision",
                schema: "dbo",
                table: "hrmsSalaryRevision",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsRewardPointsTransaction",
                schema: "dbo",
                table: "hrmsRewardPointsTransaction",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsRewardNomination",
                schema: "dbo",
                table: "hrmsRewardNomination",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsRewardDisbursement",
                schema: "dbo",
                table: "hrmsRewardDisbursement",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsReviewCycle",
                schema: "dbo",
                table: "hrmsReviewCycle",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsRequisitionScreeningCriterion",
                schema: "dbo",
                table: "hrmsRequisitionScreeningCriterion",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsReportScheduleRecipient",
                schema: "dbo",
                table: "hrmsReportScheduleRecipient",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsReportScheduleFieldValue",
                schema: "dbo",
                table: "hrmsReportScheduleFieldValue",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsReportScheduleFieldOutput",
                schema: "dbo",
                table: "hrmsReportScheduleFieldOutput",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsReportSchedule",
                schema: "dbo",
                table: "hrmsReportSchedule",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsReportSavedFilter",
                schema: "dbo",
                table: "hrmsReportSavedFilter",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsReportRunRecipient",
                schema: "dbo",
                table: "hrmsReportRunRecipient",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsReportRun",
                schema: "dbo",
                table: "hrmsReportRun",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsReportRestriction",
                schema: "dbo",
                table: "hrmsReportRestriction",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsReportFieldOutput",
                schema: "dbo",
                table: "hrmsReportFieldOutput",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsReportField",
                schema: "dbo",
                table: "hrmsReportField",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsReport",
                schema: "dbo",
                table: "hrmsReport",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsRecognitionProgram",
                schema: "dbo",
                table: "hrmsRecognitionProgram",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsRecognitionBadge",
                schema: "dbo",
                table: "hrmsRecognitionBadge",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsRatingScaleLevel",
                schema: "dbo",
                table: "hrmsRatingScaleLevel",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsRatingScale",
                schema: "dbo",
                table: "hrmsRatingScale",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsProfileChangeRequest",
                schema: "dbo",
                table: "hrmsProfileChangeRequest",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsPositionCompetency",
                schema: "dbo",
                table: "hrmsPositionCompetency",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsPositionClass",
                schema: "dbo",
                table: "hrmsPositionClass",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsPosition",
                schema: "dbo",
                table: "hrmsPosition",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsPipObjective",
                schema: "dbo",
                table: "hrmsPipObjective",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CorePerson",
                schema: "Core",
                table: "CorePerson",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsPerformanceHistory",
                schema: "dbo",
                table: "hrmsPerformanceHistory",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsPerDiemRate",
                schema: "dbo",
                table: "hrmsPerDiemRate",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsOtherLeaveSetting",
                schema: "dbo",
                table: "hrmsOtherLeaveSetting",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsOtherLeaveDetail",
                schema: "dbo",
                table: "hrmsOtherLeaveDetail",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsOtherLeave",
                schema: "dbo",
                table: "hrmsOtherLeave",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsOrganizationUnit",
                schema: "dbo",
                table: "hrmsOrganizationUnit",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsOrganizationalObjective",
                schema: "dbo",
                table: "hrmsOrganizationalObjective",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_coreOperation",
                schema: "dbo",
                table: "coreOperation",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsOfferLetterTemplate",
                schema: "dbo",
                table: "hrmsOfferLetterTemplate",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsNumberSequence",
                schema: "dbo",
                table: "hrmsNumberSequence",
                columns: new[] { "TenantId", "Key" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_coreModule",
                schema: "dbo",
                table: "coreModule",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsMentorship",
                schema: "dbo",
                table: "hrmsMentorship",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsMedicalServiceContract",
                schema: "dbo",
                table: "hrmsMedicalServiceContract",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsMedicalProvider",
                schema: "dbo",
                table: "hrmsMedicalProvider",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsMedicalPlan",
                schema: "dbo",
                table: "hrmsMedicalPlan",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsMedicalEnrollment",
                schema: "dbo",
                table: "hrmsMedicalEnrollment",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsMedicalClaimAttachment",
                schema: "dbo",
                table: "hrmsMedicalClaimAttachment",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsMedicalClaim",
                schema: "dbo",
                table: "hrmsMedicalClaim",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsMedicalBeneficiary",
                schema: "dbo",
                table: "hrmsMedicalBeneficiary",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsLoanType",
                schema: "dbo",
                table: "hrmsLoanType",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsLoanRepaymentSchedule",
                schema: "dbo",
                table: "hrmsLoanRepaymentSchedule",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsLoanGuarantor",
                schema: "dbo",
                table: "hrmsLoanGuarantor",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsLoan",
                schema: "dbo",
                table: "hrmsLoan",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsLeaveType",
                schema: "dbo",
                table: "hrmsLeaveType",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsLeaveRequestLine",
                schema: "dbo",
                table: "hrmsLeaveRequestLine",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsLeaveRequest",
                schema: "dbo",
                table: "hrmsLeaveRequest",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsLeaveBalanceTransaction",
                schema: "dbo",
                table: "hrmsLeaveBalanceTransaction",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsLeaveBalance",
                schema: "dbo",
                table: "hrmsLeaveBalance",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsLearningPathStep",
                schema: "dbo",
                table: "hrmsLearningPathStep",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsLearningPath",
                schema: "dbo",
                table: "hrmsLearningPath",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsLearningCommunityPost",
                schema: "dbo",
                table: "hrmsLearningCommunityPost",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsLearningCommunityMember",
                schema: "dbo",
                table: "hrmsLearningCommunityMember",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsLearningCommunity",
                schema: "dbo",
                table: "hrmsLearningCommunity",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsKnowledgeTransfer",
                schema: "dbo",
                table: "hrmsKnowledgeTransfer",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsJobRequisition",
                schema: "dbo",
                table: "hrmsJobRequisition",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsJobOffer",
                schema: "dbo",
                table: "hrmsJobOffer",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsJobGrade",
                schema: "dbo",
                table: "hrmsJobGrade",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsJobCategory",
                schema: "dbo",
                table: "hrmsJobCategory",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsJobApplicationStageLog",
                schema: "dbo",
                table: "hrmsJobApplicationStageLog",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsJobApplication",
                schema: "dbo",
                table: "hrmsJobApplication",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsInterviewPanelist",
                schema: "dbo",
                table: "hrmsInterviewPanelist",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsInterviewFeedback",
                schema: "dbo",
                table: "hrmsInterviewFeedback",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsInterview",
                schema: "dbo",
                table: "hrmsInterview",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsInsurancePremiumSchedule",
                schema: "dbo",
                table: "hrmsInsurancePremiumSchedule",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsInsurancePolicy",
                schema: "dbo",
                table: "hrmsInsurancePolicy",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsInsuranceClaimAttachment",
                schema: "dbo",
                table: "hrmsInsuranceClaimAttachment",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsInsuranceClaim",
                schema: "dbo",
                table: "hrmsInsuranceClaim",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsImprovementPlan",
                schema: "dbo",
                table: "hrmsImprovementPlan",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsHoliday",
                schema: "dbo",
                table: "hrmsHoliday",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsHiringRequest",
                schema: "dbo",
                table: "hrmsHiringRequest",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsGrievanceNote",
                schema: "dbo",
                table: "hrmsGrievanceNote",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsGrievance",
                schema: "dbo",
                table: "hrmsGrievance",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsGoalActionItem",
                schema: "dbo",
                table: "hrmsGoalActionItem",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsExitQuestionnaire",
                schema: "dbo",
                table: "hrmsExitQuestionnaire",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsExitInterview",
                schema: "dbo",
                table: "hrmsExitInterview",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsEmployeeTrainingCertificate",
                schema: "dbo",
                table: "hrmsEmployeeTrainingCertificate",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsEmployeeTermination",
                schema: "dbo",
                table: "hrmsEmployeeTermination",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsEmployeeRecognition",
                schema: "dbo",
                table: "hrmsEmployeeRecognition",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsEmployeeMovement",
                schema: "dbo",
                table: "hrmsEmployeeMovement",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsEmployeeGuarantee",
                schema: "dbo",
                table: "hrmsEmployeeGuarantee",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsEmployeeGoal",
                schema: "dbo",
                table: "hrmsEmployeeGoal",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsEmployeeFieldValue",
                schema: "dbo",
                table: "hrmsEmployeeFieldValue",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsEmployeeFieldDefinition",
                schema: "dbo",
                table: "hrmsEmployeeFieldDefinition",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsEmployeeExperience",
                schema: "dbo",
                table: "hrmsEmployeeExperience",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsEmployeeEducation",
                schema: "dbo",
                table: "hrmsEmployeeEducation",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsEmployeeDocument",
                schema: "dbo",
                table: "hrmsEmployeeDocument",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsEmployeeDependent",
                schema: "dbo",
                table: "hrmsEmployeeDependent",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsEmployeeCareerPathStepProgress",
                schema: "dbo",
                table: "hrmsEmployeeCareerPathStepProgress",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsEmployeeCareerPath",
                schema: "dbo",
                table: "hrmsEmployeeCareerPath",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsEmployeeBenefitEnrollment",
                schema: "dbo",
                table: "hrmsEmployeeBenefitEnrollment",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsEmployeeAllowance",
                schema: "dbo",
                table: "hrmsEmployeeAllowance",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsEmployee",
                schema: "dbo",
                table: "hrmsEmployee",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsDynamicFormRecord",
                schema: "dbo",
                table: "hrmsDynamicFormRecord",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsDynamicFormField",
                schema: "dbo",
                table: "hrmsDynamicFormField",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsDynamicForm",
                schema: "dbo",
                table: "hrmsDynamicForm",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsDocumentTemplate",
                schema: "dbo",
                table: "hrmsDocumentTemplate",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsDisciplinaryMeasure",
                schema: "dbo",
                table: "hrmsDisciplinaryMeasure",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsDevelopmentPlan",
                schema: "dbo",
                table: "hrmsDevelopmentPlan",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsDevelopmentAction",
                schema: "dbo",
                table: "hrmsDevelopmentAction",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsCriticalPosition",
                schema: "dbo",
                table: "hrmsCriticalPosition",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsCriterionEvaluator",
                schema: "dbo",
                table: "hrmsCriterionEvaluator",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsCompetencyCategory",
                schema: "dbo",
                table: "hrmsCompetencyCategory",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsCompetency",
                schema: "dbo",
                table: "hrmsCompetency",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsCompensationRequest",
                schema: "dbo",
                table: "hrmsCompensationRequest",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsCompanyProfile",
                schema: "dbo",
                table: "hrmsCompanyProfile",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsCompanyAsset",
                schema: "dbo",
                table: "hrmsCompanyAsset",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsCommunityPostReaction",
                schema: "dbo",
                table: "hrmsCommunityPostReaction",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsClearanceDepartmentApprover",
                schema: "dbo",
                table: "hrmsClearanceDepartmentApprover",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsClearanceDepartment",
                schema: "dbo",
                table: "hrmsClearanceDepartment",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsCareerPathStepCompetency",
                schema: "dbo",
                table: "hrmsCareerPathStepCompetency",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsCareerPathStep",
                schema: "dbo",
                table: "hrmsCareerPathStep",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsCareerPathChangeRequest",
                schema: "dbo",
                table: "hrmsCareerPathChangeRequest",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsCareerPath",
                schema: "dbo",
                table: "hrmsCareerPath",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsCandidateDocument",
                schema: "dbo",
                table: "hrmsCandidateDocument",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsCandidate",
                schema: "dbo",
                table: "hrmsCandidate",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsCalibrationSession",
                schema: "dbo",
                table: "hrmsCalibrationSession",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsCalibrationItem",
                schema: "dbo",
                table: "hrmsCalibrationItem",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsBranch",
                schema: "dbo",
                table: "hrmsBranch",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsBenefitPlan",
                schema: "dbo",
                table: "hrmsBenefitPlan",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsAwardCategory",
                schema: "dbo",
                table: "hrmsAwardCategory",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsAuditLog",
                schema: "dbo",
                table: "hrmsAuditLog",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsAppraisalTemplate",
                schema: "dbo",
                table: "hrmsAppraisalTemplate",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsAppraisalPeerReview",
                schema: "dbo",
                table: "hrmsAppraisalPeerReview",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsAppraisalGoal",
                schema: "dbo",
                table: "hrmsAppraisalGoal",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsAppraisalCompetency",
                schema: "dbo",
                table: "hrmsAppraisalCompetency",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsAppraisalAppeal",
                schema: "dbo",
                table: "hrmsAppraisalAppeal",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsAppraisal",
                schema: "dbo",
                table: "hrmsAppraisal",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsApplicationCriterionScore",
                schema: "dbo",
                table: "hrmsApplicationCriterionScore",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsAnnualLeaveSetting",
                schema: "dbo",
                table: "hrmsAnnualLeaveSetting",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsAnnualLeaveHeader",
                schema: "dbo",
                table: "hrmsAnnualLeaveHeader",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsAnnualLeaveDetail",
                schema: "dbo",
                table: "hrmsAnnualLeaveDetail",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsAnnouncement",
                schema: "dbo",
                table: "hrmsAnnouncement",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsAllowanceType",
                schema: "dbo",
                table: "hrmsAllowanceType",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrmsAchievement",
                schema: "dbo",
                table: "hrmsAchievement",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_coreModule_coreSubsystem_SubsystemId",
                schema: "dbo",
                table: "coreModule",
                column: "SubsystemId",
                principalSchema: "dbo",
                principalTable: "coreSubsystem",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_coreOperation_coreModule_ModuleId",
                schema: "dbo",
                table: "coreOperation",
                column: "ModuleId",
                principalSchema: "dbo",
                principalTable: "coreModule",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_coreSalaryScale_hrmsJobGrade_JobGradeId",
                schema: "Core",
                table: "coreSalaryScale",
                column: "JobGradeId",
                principalSchema: "dbo",
                principalTable: "hrmsJobGrade",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_coreSalaryScale_lupStep_StepId",
                schema: "Core",
                table: "coreSalaryScale",
                column: "StepId",
                principalSchema: "Core",
                principalTable: "lupStep",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsAchievement_hrmsAppraisal_AppraisalId",
                schema: "dbo",
                table: "hrmsAchievement",
                column: "AppraisalId",
                principalSchema: "dbo",
                principalTable: "hrmsAppraisal",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsAchievement_hrmsEmployee_EmployeeId",
                schema: "dbo",
                table: "hrmsAchievement",
                column: "EmployeeId",
                principalSchema: "dbo",
                principalTable: "hrmsEmployee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsAnnouncement_hrmsBranch_BranchId",
                schema: "dbo",
                table: "hrmsAnnouncement",
                column: "BranchId",
                principalSchema: "dbo",
                principalTable: "hrmsBranch",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsAnnouncement_hrmsOrganizationUnit_OrganizationUnitId",
                schema: "dbo",
                table: "hrmsAnnouncement",
                column: "OrganizationUnitId",
                principalSchema: "dbo",
                principalTable: "hrmsOrganizationUnit",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsAnnualLeaveDetail_hrmsAnnualLeaveHeader_AnnualLeaveHeaderId",
                schema: "dbo",
                table: "hrmsAnnualLeaveDetail",
                column: "AnnualLeaveHeaderId",
                principalSchema: "dbo",
                principalTable: "hrmsAnnualLeaveHeader",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsAnnualLeaveHeader_hrmsEmployee_EmployeeId",
                schema: "dbo",
                table: "hrmsAnnualLeaveHeader",
                column: "EmployeeId",
                principalSchema: "dbo",
                principalTable: "hrmsEmployee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsAnnualLeaveHeader_hrmsLeaveBalance_AnnualLeaveLedgerId",
                schema: "dbo",
                table: "hrmsAnnualLeaveHeader",
                column: "AnnualLeaveLedgerId",
                principalSchema: "dbo",
                principalTable: "hrmsLeaveBalance",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsAnnualLeaveSetting_FiscalYear_FiscalYearId",
                schema: "dbo",
                table: "hrmsAnnualLeaveSetting",
                column: "FiscalYearId",
                principalSchema: "Core",
                principalTable: "FiscalYear",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsApplicationCriterionScore_hrmsJobApplication_ApplicationId",
                schema: "dbo",
                table: "hrmsApplicationCriterionScore",
                column: "ApplicationId",
                principalSchema: "dbo",
                principalTable: "hrmsJobApplication",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsAppraisal_hrmsEmployee_EmployeeId",
                schema: "dbo",
                table: "hrmsAppraisal",
                column: "EmployeeId",
                principalSchema: "dbo",
                principalTable: "hrmsEmployee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsAppraisal_hrmsReviewCycle_ReviewCycleId",
                schema: "dbo",
                table: "hrmsAppraisal",
                column: "ReviewCycleId",
                principalSchema: "dbo",
                principalTable: "hrmsReviewCycle",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsAppraisalAppeal_hrmsAppraisal_AppraisalId",
                schema: "dbo",
                table: "hrmsAppraisalAppeal",
                column: "AppraisalId",
                principalSchema: "dbo",
                principalTable: "hrmsAppraisal",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsAppraisalAppeal_hrmsEmployee_EmployeeId",
                schema: "dbo",
                table: "hrmsAppraisalAppeal",
                column: "EmployeeId",
                principalSchema: "dbo",
                principalTable: "hrmsEmployee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsAppraisalCompetency_hrmsAppraisal_AppraisalId",
                schema: "dbo",
                table: "hrmsAppraisalCompetency",
                column: "AppraisalId",
                principalSchema: "dbo",
                principalTable: "hrmsAppraisal",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsAppraisalGoal_hrmsAppraisal_AppraisalId",
                schema: "dbo",
                table: "hrmsAppraisalGoal",
                column: "AppraisalId",
                principalSchema: "dbo",
                principalTable: "hrmsAppraisal",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsAppraisalPeerReview_hrmsAppraisal_AppraisalId",
                schema: "dbo",
                table: "hrmsAppraisalPeerReview",
                column: "AppraisalId",
                principalSchema: "dbo",
                principalTable: "hrmsAppraisal",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsAppraisalPeerReview_hrmsEmployee_PeerEmployeeId",
                schema: "dbo",
                table: "hrmsAppraisalPeerReview",
                column: "PeerEmployeeId",
                principalSchema: "dbo",
                principalTable: "hrmsEmployee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsBranch_hrmsBranch_ParentId",
                schema: "dbo",
                table: "hrmsBranch",
                column: "ParentId",
                principalSchema: "dbo",
                principalTable: "hrmsBranch",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsCalibrationItem_hrmsAppraisal_AppraisalId",
                schema: "dbo",
                table: "hrmsCalibrationItem",
                column: "AppraisalId",
                principalSchema: "dbo",
                principalTable: "hrmsAppraisal",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsCalibrationItem_hrmsCalibrationSession_CalibrationSessionId",
                schema: "dbo",
                table: "hrmsCalibrationItem",
                column: "CalibrationSessionId",
                principalSchema: "dbo",
                principalTable: "hrmsCalibrationSession",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsCalibrationSession_hrmsOrganizationUnit_OrganizationUnitId",
                schema: "dbo",
                table: "hrmsCalibrationSession",
                column: "OrganizationUnitId",
                principalSchema: "dbo",
                principalTable: "hrmsOrganizationUnit",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsCalibrationSession_hrmsReviewCycle_ReviewCycleId",
                schema: "dbo",
                table: "hrmsCalibrationSession",
                column: "ReviewCycleId",
                principalSchema: "dbo",
                principalTable: "hrmsReviewCycle",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsCandidate_CorePerson_PersonId",
                schema: "dbo",
                table: "hrmsCandidate",
                column: "PersonId",
                principalSchema: "Core",
                principalTable: "CorePerson",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsCandidate_hrmsEmployee_InternalEmployeeId",
                schema: "dbo",
                table: "hrmsCandidate",
                column: "InternalEmployeeId",
                principalSchema: "dbo",
                principalTable: "hrmsEmployee",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsCandidateDocument_hrmsCandidate_CandidateId",
                schema: "dbo",
                table: "hrmsCandidateDocument",
                column: "CandidateId",
                principalSchema: "dbo",
                principalTable: "hrmsCandidate",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsCareerPathChangeRequest_hrmsCareerPath_CurrentCareerPathId",
                schema: "dbo",
                table: "hrmsCareerPathChangeRequest",
                column: "CurrentCareerPathId",
                principalSchema: "dbo",
                principalTable: "hrmsCareerPath",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsCareerPathChangeRequest_hrmsCareerPath_RequestedCareerPathId",
                schema: "dbo",
                table: "hrmsCareerPathChangeRequest",
                column: "RequestedCareerPathId",
                principalSchema: "dbo",
                principalTable: "hrmsCareerPath",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsCareerPathChangeRequest_hrmsEmployee_EmployeeId",
                schema: "dbo",
                table: "hrmsCareerPathChangeRequest",
                column: "EmployeeId",
                principalSchema: "dbo",
                principalTable: "hrmsEmployee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsCareerPathStep_hrmsCareerPath_CareerPathId",
                schema: "dbo",
                table: "hrmsCareerPathStep",
                column: "CareerPathId",
                principalSchema: "dbo",
                principalTable: "hrmsCareerPath",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsCareerPathStep_hrmsJobGrade_JobGradeId",
                schema: "dbo",
                table: "hrmsCareerPathStep",
                column: "JobGradeId",
                principalSchema: "dbo",
                principalTable: "hrmsJobGrade",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsCareerPathStep_hrmsPositionClass_PositionClassId",
                schema: "dbo",
                table: "hrmsCareerPathStep",
                column: "PositionClassId",
                principalSchema: "dbo",
                principalTable: "hrmsPositionClass",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsCareerPathStepCompetency_hrmsCareerPathStep_CareerPathStepId",
                schema: "dbo",
                table: "hrmsCareerPathStepCompetency",
                column: "CareerPathStepId",
                principalSchema: "dbo",
                principalTable: "hrmsCareerPathStep",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsCareerPathStepCompetency_hrmsCompetency_CompetencyId",
                schema: "dbo",
                table: "hrmsCareerPathStepCompetency",
                column: "CompetencyId",
                principalSchema: "dbo",
                principalTable: "hrmsCompetency",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsClearanceDepartmentApprover_hrmsClearanceDepartment_DepartmentId",
                schema: "dbo",
                table: "hrmsClearanceDepartmentApprover",
                column: "DepartmentId",
                principalSchema: "dbo",
                principalTable: "hrmsClearanceDepartment",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsCommunityPostReaction_hrmsEmployee_EmployeeId",
                schema: "dbo",
                table: "hrmsCommunityPostReaction",
                column: "EmployeeId",
                principalSchema: "dbo",
                principalTable: "hrmsEmployee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsCommunityPostReaction_hrmsLearningCommunityPost_LearningCommunityPostId",
                schema: "dbo",
                table: "hrmsCommunityPostReaction",
                column: "LearningCommunityPostId",
                principalSchema: "dbo",
                principalTable: "hrmsLearningCommunityPost",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsCompanyAsset_hrmsEmployee_AssignedToEmployeeId",
                schema: "dbo",
                table: "hrmsCompanyAsset",
                column: "AssignedToEmployeeId",
                principalSchema: "dbo",
                principalTable: "hrmsEmployee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsCompensationRequest_hrmsEmployee_EmployeeId",
                schema: "dbo",
                table: "hrmsCompensationRequest",
                column: "EmployeeId",
                principalSchema: "dbo",
                principalTable: "hrmsEmployee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsCompetency_hrmsCompetencyCategory_CompetencyCategoryId",
                schema: "dbo",
                table: "hrmsCompetency",
                column: "CompetencyCategoryId",
                principalSchema: "dbo",
                principalTable: "hrmsCompetencyCategory",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsCriterionEvaluator_hrmsEmployee_EmployeeId",
                schema: "dbo",
                table: "hrmsCriterionEvaluator",
                column: "EmployeeId",
                principalSchema: "dbo",
                principalTable: "hrmsEmployee",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsCriterionEvaluator_hrmsRequisitionScreeningCriterion_CriterionId",
                schema: "dbo",
                table: "hrmsCriterionEvaluator",
                column: "CriterionId",
                principalSchema: "dbo",
                principalTable: "hrmsRequisitionScreeningCriterion",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsCriticalPosition_hrmsPosition_PositionId",
                schema: "dbo",
                table: "hrmsCriticalPosition",
                column: "PositionId",
                principalSchema: "dbo",
                principalTable: "hrmsPosition",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsDevelopmentAction_hrmsCompetency_CompetencyId",
                schema: "dbo",
                table: "hrmsDevelopmentAction",
                column: "CompetencyId",
                principalSchema: "dbo",
                principalTable: "hrmsCompetency",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsDevelopmentAction_hrmsDevelopmentPlan_DevelopmentPlanId",
                schema: "dbo",
                table: "hrmsDevelopmentAction",
                column: "DevelopmentPlanId",
                principalSchema: "dbo",
                principalTable: "hrmsDevelopmentPlan",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsDevelopmentPlan_hrmsAppraisal_AppraisalId",
                schema: "dbo",
                table: "hrmsDevelopmentPlan",
                column: "AppraisalId",
                principalSchema: "dbo",
                principalTable: "hrmsAppraisal",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsDevelopmentPlan_hrmsEmployee_EmployeeId",
                schema: "dbo",
                table: "hrmsDevelopmentPlan",
                column: "EmployeeId",
                principalSchema: "dbo",
                principalTable: "hrmsEmployee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsDisciplinaryMeasure_hrmsEmployee_EmployeeId",
                schema: "dbo",
                table: "hrmsDisciplinaryMeasure",
                column: "EmployeeId",
                principalSchema: "dbo",
                principalTable: "hrmsEmployee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsDynamicFormField_hrmsDynamicForm_DynamicFormId",
                schema: "dbo",
                table: "hrmsDynamicFormField",
                column: "DynamicFormId",
                principalSchema: "dbo",
                principalTable: "hrmsDynamicForm",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsDynamicFormRecord_hrmsDynamicForm_DynamicFormId",
                schema: "dbo",
                table: "hrmsDynamicFormRecord",
                column: "DynamicFormId",
                principalSchema: "dbo",
                principalTable: "hrmsDynamicForm",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsEmployee_CorePerson_PersonId",
                schema: "dbo",
                table: "hrmsEmployee",
                column: "PersonId",
                principalSchema: "Core",
                principalTable: "CorePerson",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsEmployee_coreSalaryScale_SalaryScaleId",
                schema: "dbo",
                table: "hrmsEmployee",
                column: "SalaryScaleId",
                principalSchema: "Core",
                principalTable: "coreSalaryScale",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsEmployee_hrmsBranch_BranchId",
                schema: "dbo",
                table: "hrmsEmployee",
                column: "BranchId",
                principalSchema: "dbo",
                principalTable: "hrmsBranch",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsEmployee_hrmsPosition_PositionId",
                schema: "dbo",
                table: "hrmsEmployee",
                column: "PositionId",
                principalSchema: "dbo",
                principalTable: "hrmsPosition",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsEmployeeAllowance_hrmsAllowanceType_AllowanceTypeId",
                schema: "dbo",
                table: "hrmsEmployeeAllowance",
                column: "AllowanceTypeId",
                principalSchema: "dbo",
                principalTable: "hrmsAllowanceType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsEmployeeAllowance_hrmsEmployee_EmployeeId",
                schema: "dbo",
                table: "hrmsEmployeeAllowance",
                column: "EmployeeId",
                principalSchema: "dbo",
                principalTable: "hrmsEmployee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsEmployeeBenefitEnrollment_hrmsBenefitPlan_BenefitPlanId",
                schema: "dbo",
                table: "hrmsEmployeeBenefitEnrollment",
                column: "BenefitPlanId",
                principalSchema: "dbo",
                principalTable: "hrmsBenefitPlan",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsEmployeeBenefitEnrollment_hrmsEmployee_EmployeeId",
                schema: "dbo",
                table: "hrmsEmployeeBenefitEnrollment",
                column: "EmployeeId",
                principalSchema: "dbo",
                principalTable: "hrmsEmployee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsEmployeeCareerPath_hrmsCareerPath_CareerPathId",
                schema: "dbo",
                table: "hrmsEmployeeCareerPath",
                column: "CareerPathId",
                principalSchema: "dbo",
                principalTable: "hrmsCareerPath",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsEmployeeCareerPath_hrmsEmployee_EmployeeId",
                schema: "dbo",
                table: "hrmsEmployeeCareerPath",
                column: "EmployeeId",
                principalSchema: "dbo",
                principalTable: "hrmsEmployee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsEmployeeCareerPathStepProgress_hrmsEmployeeCareerPath_EmployeeCareerPathId",
                schema: "dbo",
                table: "hrmsEmployeeCareerPathStepProgress",
                column: "EmployeeCareerPathId",
                principalSchema: "dbo",
                principalTable: "hrmsEmployeeCareerPath",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsEmployeeDependent_CorePerson_PersonId",
                schema: "dbo",
                table: "hrmsEmployeeDependent",
                column: "PersonId",
                principalSchema: "Core",
                principalTable: "CorePerson",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsEmployeeDependent_hrmsEmployee_RelatedEmployeeId",
                schema: "dbo",
                table: "hrmsEmployeeDependent",
                column: "RelatedEmployeeId",
                principalSchema: "dbo",
                principalTable: "hrmsEmployee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsEmployeeEducation_CorePerson_PersonId",
                schema: "dbo",
                table: "hrmsEmployeeEducation",
                column: "PersonId",
                principalSchema: "Core",
                principalTable: "CorePerson",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsEmployeeExperience_CorePerson_PersonId",
                schema: "dbo",
                table: "hrmsEmployeeExperience",
                column: "PersonId",
                principalSchema: "Core",
                principalTable: "CorePerson",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsEmployeeFieldValue_hrmsEmployeeFieldDefinition_FieldDefinitionId",
                schema: "dbo",
                table: "hrmsEmployeeFieldValue",
                column: "FieldDefinitionId",
                principalSchema: "dbo",
                principalTable: "hrmsEmployeeFieldDefinition",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsEmployeeGoal_hrmsEmployee_EmployeeId",
                schema: "dbo",
                table: "hrmsEmployeeGoal",
                column: "EmployeeId",
                principalSchema: "dbo",
                principalTable: "hrmsEmployee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsEmployeeGoal_hrmsOrganizationalObjective_OrganizationalObjectiveId",
                schema: "dbo",
                table: "hrmsEmployeeGoal",
                column: "OrganizationalObjectiveId",
                principalSchema: "dbo",
                principalTable: "hrmsOrganizationalObjective",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsEmployeeGoal_hrmsReviewCycle_ReviewCycleId",
                schema: "dbo",
                table: "hrmsEmployeeGoal",
                column: "ReviewCycleId",
                principalSchema: "dbo",
                principalTable: "hrmsReviewCycle",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsEmployeeGuarantee_hrmsEmployee_EmployeeId",
                schema: "dbo",
                table: "hrmsEmployeeGuarantee",
                column: "EmployeeId",
                principalSchema: "dbo",
                principalTable: "hrmsEmployee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsEmployeeMovement_coreSalaryScale_ToSalaryScaleId",
                schema: "dbo",
                table: "hrmsEmployeeMovement",
                column: "ToSalaryScaleId",
                principalSchema: "Core",
                principalTable: "coreSalaryScale",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsEmployeeMovement_hrmsEmployee_EmployeeId",
                schema: "dbo",
                table: "hrmsEmployeeMovement",
                column: "EmployeeId",
                principalSchema: "dbo",
                principalTable: "hrmsEmployee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsEmployeeRecognition_hrmsEmployee_EmployeeId",
                schema: "dbo",
                table: "hrmsEmployeeRecognition",
                column: "EmployeeId",
                principalSchema: "dbo",
                principalTable: "hrmsEmployee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsEmployeeRecognition_hrmsRecognitionBadge_RecognitionBadgeId",
                schema: "dbo",
                table: "hrmsEmployeeRecognition",
                column: "RecognitionBadgeId",
                principalSchema: "dbo",
                principalTable: "hrmsRecognitionBadge",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsEmployeeTermination_hrmsEmployee_EmployeeId",
                schema: "dbo",
                table: "hrmsEmployeeTermination",
                column: "EmployeeId",
                principalSchema: "dbo",
                principalTable: "hrmsEmployee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsEmployeeTrainingCertificate_hrmsEmployee_EmployeeId",
                schema: "dbo",
                table: "hrmsEmployeeTrainingCertificate",
                column: "EmployeeId",
                principalSchema: "dbo",
                principalTable: "hrmsEmployee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsEmployeeTrainingCertificate_hrmsTrainingCourse_TrainingCourseId",
                schema: "dbo",
                table: "hrmsEmployeeTrainingCertificate",
                column: "TrainingCourseId",
                principalSchema: "dbo",
                principalTable: "hrmsTrainingCourse",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsEmployeeTrainingCertificate_hrmsTrainingEnrollment_TrainingEnrollmentId",
                schema: "dbo",
                table: "hrmsEmployeeTrainingCertificate",
                column: "TrainingEnrollmentId",
                principalSchema: "dbo",
                principalTable: "hrmsTrainingEnrollment",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsExitInterview_hrmsEmployeeTermination_TerminationId",
                schema: "dbo",
                table: "hrmsExitInterview",
                column: "TerminationId",
                principalSchema: "dbo",
                principalTable: "hrmsEmployeeTermination",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsGoalActionItem_hrmsEmployeeGoal_EmployeeGoalId",
                schema: "dbo",
                table: "hrmsGoalActionItem",
                column: "EmployeeGoalId",
                principalSchema: "dbo",
                principalTable: "hrmsEmployeeGoal",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsGrievance_hrmsEmployee_EmployeeId",
                schema: "dbo",
                table: "hrmsGrievance",
                column: "EmployeeId",
                principalSchema: "dbo",
                principalTable: "hrmsEmployee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsGrievanceNote_hrmsGrievance_GrievanceId",
                schema: "dbo",
                table: "hrmsGrievanceNote",
                column: "GrievanceId",
                principalSchema: "dbo",
                principalTable: "hrmsGrievance",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsHiringRequest_hrmsOrganizationUnit_OrganizationUnitId",
                schema: "dbo",
                table: "hrmsHiringRequest",
                column: "OrganizationUnitId",
                principalSchema: "dbo",
                principalTable: "hrmsOrganizationUnit",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsHiringRequest_hrmsPositionClass_PositionClassId",
                schema: "dbo",
                table: "hrmsHiringRequest",
                column: "PositionClassId",
                principalSchema: "dbo",
                principalTable: "hrmsPositionClass",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsImprovementPlan_hrmsAppraisal_AppraisalId",
                schema: "dbo",
                table: "hrmsImprovementPlan",
                column: "AppraisalId",
                principalSchema: "dbo",
                principalTable: "hrmsAppraisal",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsImprovementPlan_hrmsEmployee_EmployeeId",
                schema: "dbo",
                table: "hrmsImprovementPlan",
                column: "EmployeeId",
                principalSchema: "dbo",
                principalTable: "hrmsEmployee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsInsuranceClaim_hrmsEmployee_EmployeeId",
                schema: "dbo",
                table: "hrmsInsuranceClaim",
                column: "EmployeeId",
                principalSchema: "dbo",
                principalTable: "hrmsEmployee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsInsuranceClaim_hrmsInsurancePolicy_InsurancePolicyId",
                schema: "dbo",
                table: "hrmsInsuranceClaim",
                column: "InsurancePolicyId",
                principalSchema: "dbo",
                principalTable: "hrmsInsurancePolicy",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsInsuranceClaimAttachment_hrmsInsuranceClaim_InsuranceClaimId",
                schema: "dbo",
                table: "hrmsInsuranceClaimAttachment",
                column: "InsuranceClaimId",
                principalSchema: "dbo",
                principalTable: "hrmsInsuranceClaim",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsInsurancePremiumSchedule_hrmsInsurancePolicy_InsurancePolicyId",
                schema: "dbo",
                table: "hrmsInsurancePremiumSchedule",
                column: "InsurancePolicyId",
                principalSchema: "dbo",
                principalTable: "hrmsInsurancePolicy",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsInterview_hrmsJobApplication_ApplicationId",
                schema: "dbo",
                table: "hrmsInterview",
                column: "ApplicationId",
                principalSchema: "dbo",
                principalTable: "hrmsJobApplication",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsInterviewFeedback_hrmsInterviewPanelist_PanelistId",
                schema: "dbo",
                table: "hrmsInterviewFeedback",
                column: "PanelistId",
                principalSchema: "dbo",
                principalTable: "hrmsInterviewPanelist",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsInterviewPanelist_hrmsEmployee_EmployeeId",
                schema: "dbo",
                table: "hrmsInterviewPanelist",
                column: "EmployeeId",
                principalSchema: "dbo",
                principalTable: "hrmsEmployee",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsInterviewPanelist_hrmsInterview_InterviewId",
                schema: "dbo",
                table: "hrmsInterviewPanelist",
                column: "InterviewId",
                principalSchema: "dbo",
                principalTable: "hrmsInterview",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsJobApplication_hrmsCandidate_CandidateId",
                schema: "dbo",
                table: "hrmsJobApplication",
                column: "CandidateId",
                principalSchema: "dbo",
                principalTable: "hrmsCandidate",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsJobApplication_hrmsJobRequisition_RequisitionId",
                schema: "dbo",
                table: "hrmsJobApplication",
                column: "RequisitionId",
                principalSchema: "dbo",
                principalTable: "hrmsJobRequisition",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsJobApplicationStageLog_hrmsJobApplication_ApplicationId",
                schema: "dbo",
                table: "hrmsJobApplicationStageLog",
                column: "ApplicationId",
                principalSchema: "dbo",
                principalTable: "hrmsJobApplication",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsJobOffer_coreSalaryScale_SalaryScaleId",
                schema: "dbo",
                table: "hrmsJobOffer",
                column: "SalaryScaleId",
                principalSchema: "Core",
                principalTable: "coreSalaryScale",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsJobOffer_hrmsEmployee_HiringManagerEmployeeId",
                schema: "dbo",
                table: "hrmsJobOffer",
                column: "HiringManagerEmployeeId",
                principalSchema: "dbo",
                principalTable: "hrmsEmployee",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsJobOffer_hrmsJobApplication_ApplicationId",
                schema: "dbo",
                table: "hrmsJobOffer",
                column: "ApplicationId",
                principalSchema: "dbo",
                principalTable: "hrmsJobApplication",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsJobRequisition_coreSalaryScale_SalaryScaleId",
                schema: "dbo",
                table: "hrmsJobRequisition",
                column: "SalaryScaleId",
                principalSchema: "Core",
                principalTable: "coreSalaryScale",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsJobRequisition_hrmsHiringRequest_HiringRequestId",
                schema: "dbo",
                table: "hrmsJobRequisition",
                column: "HiringRequestId",
                principalSchema: "dbo",
                principalTable: "hrmsHiringRequest",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsJobRequisition_hrmsOrganizationUnit_OrganizationUnitId",
                schema: "dbo",
                table: "hrmsJobRequisition",
                column: "OrganizationUnitId",
                principalSchema: "dbo",
                principalTable: "hrmsOrganizationUnit",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsJobRequisition_hrmsPositionClass_PositionClassId",
                schema: "dbo",
                table: "hrmsJobRequisition",
                column: "PositionClassId",
                principalSchema: "dbo",
                principalTable: "hrmsPositionClass",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsJobRequisition_hrmsWorkLocation_WorkLocationId",
                schema: "dbo",
                table: "hrmsJobRequisition",
                column: "WorkLocationId",
                principalSchema: "dbo",
                principalTable: "hrmsWorkLocation",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsKnowledgeTransfer_hrmsEmployee_FromEmployeeId",
                schema: "dbo",
                table: "hrmsKnowledgeTransfer",
                column: "FromEmployeeId",
                principalSchema: "dbo",
                principalTable: "hrmsEmployee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsKnowledgeTransfer_hrmsSuccessionCandidate_SuccessionCandidateId",
                schema: "dbo",
                table: "hrmsKnowledgeTransfer",
                column: "SuccessionCandidateId",
                principalSchema: "dbo",
                principalTable: "hrmsSuccessionCandidate",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsLearningCommunity_hrmsTrainingCourse_TrainingCourseId",
                schema: "dbo",
                table: "hrmsLearningCommunity",
                column: "TrainingCourseId",
                principalSchema: "dbo",
                principalTable: "hrmsTrainingCourse",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsLearningCommunityMember_hrmsEmployee_EmployeeId",
                schema: "dbo",
                table: "hrmsLearningCommunityMember",
                column: "EmployeeId",
                principalSchema: "dbo",
                principalTable: "hrmsEmployee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsLearningCommunityMember_hrmsLearningCommunity_LearningCommunityId",
                schema: "dbo",
                table: "hrmsLearningCommunityMember",
                column: "LearningCommunityId",
                principalSchema: "dbo",
                principalTable: "hrmsLearningCommunity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsLearningCommunityPost_hrmsEmployee_EmployeeId",
                schema: "dbo",
                table: "hrmsLearningCommunityPost",
                column: "EmployeeId",
                principalSchema: "dbo",
                principalTable: "hrmsEmployee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsLearningCommunityPost_hrmsLearningCommunity_LearningCommunityId",
                schema: "dbo",
                table: "hrmsLearningCommunityPost",
                column: "LearningCommunityId",
                principalSchema: "dbo",
                principalTable: "hrmsLearningCommunity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsLearningPath_hrmsPosition_TargetPositionId",
                schema: "dbo",
                table: "hrmsLearningPath",
                column: "TargetPositionId",
                principalSchema: "dbo",
                principalTable: "hrmsPosition",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsLearningPathStep_hrmsLearningPath_LearningPathId",
                schema: "dbo",
                table: "hrmsLearningPathStep",
                column: "LearningPathId",
                principalSchema: "dbo",
                principalTable: "hrmsLearningPath",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsLearningPathStep_hrmsTrainingCourse_TrainingCourseId",
                schema: "dbo",
                table: "hrmsLearningPathStep",
                column: "TrainingCourseId",
                principalSchema: "dbo",
                principalTable: "hrmsTrainingCourse",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsLeaveBalance_FiscalYear_FiscalYearId",
                schema: "dbo",
                table: "hrmsLeaveBalance",
                column: "FiscalYearId",
                principalSchema: "Core",
                principalTable: "FiscalYear",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsLeaveBalance_hrmsEmployee_EmployeeId",
                schema: "dbo",
                table: "hrmsLeaveBalance",
                column: "EmployeeId",
                principalSchema: "dbo",
                principalTable: "hrmsEmployee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsLeaveBalance_hrmsLeaveType_LeaveTypeId",
                schema: "dbo",
                table: "hrmsLeaveBalance",
                column: "LeaveTypeId",
                principalSchema: "dbo",
                principalTable: "hrmsLeaveType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsLeaveRequest_FiscalYear_FiscalYearId",
                schema: "dbo",
                table: "hrmsLeaveRequest",
                column: "FiscalYearId",
                principalSchema: "Core",
                principalTable: "FiscalYear",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsLeaveRequest_hrmsEmployee_EmployeeId",
                schema: "dbo",
                table: "hrmsLeaveRequest",
                column: "EmployeeId",
                principalSchema: "dbo",
                principalTable: "hrmsEmployee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsLeaveRequestLine_hrmsLeaveRequest_LeaveRequestId",
                schema: "dbo",
                table: "hrmsLeaveRequestLine",
                column: "LeaveRequestId",
                principalSchema: "dbo",
                principalTable: "hrmsLeaveRequest",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsLeaveRequestLine_hrmsLeaveType_LeaveTypeId",
                schema: "dbo",
                table: "hrmsLeaveRequestLine",
                column: "LeaveTypeId",
                principalSchema: "dbo",
                principalTable: "hrmsLeaveType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsLoan_hrmsEmployee_EmployeeId",
                schema: "dbo",
                table: "hrmsLoan",
                column: "EmployeeId",
                principalSchema: "dbo",
                principalTable: "hrmsEmployee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsLoan_hrmsLoanType_LoanTypeId",
                schema: "dbo",
                table: "hrmsLoan",
                column: "LoanTypeId",
                principalSchema: "dbo",
                principalTable: "hrmsLoanType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsLoanGuarantor_hrmsLoan_LoanId",
                schema: "dbo",
                table: "hrmsLoanGuarantor",
                column: "LoanId",
                principalSchema: "dbo",
                principalTable: "hrmsLoan",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsLoanRepaymentSchedule_hrmsLoan_LoanId",
                schema: "dbo",
                table: "hrmsLoanRepaymentSchedule",
                column: "LoanId",
                principalSchema: "dbo",
                principalTable: "hrmsLoan",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsMedicalBeneficiary_hrmsMedicalEnrollment_MedicalEnrollmentId",
                schema: "dbo",
                table: "hrmsMedicalBeneficiary",
                column: "MedicalEnrollmentId",
                principalSchema: "dbo",
                principalTable: "hrmsMedicalEnrollment",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsMedicalClaim_hrmsEmployee_EmployeeId",
                schema: "dbo",
                table: "hrmsMedicalClaim",
                column: "EmployeeId",
                principalSchema: "dbo",
                principalTable: "hrmsEmployee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsMedicalClaim_hrmsMedicalEnrollment_MedicalEnrollmentId",
                schema: "dbo",
                table: "hrmsMedicalClaim",
                column: "MedicalEnrollmentId",
                principalSchema: "dbo",
                principalTable: "hrmsMedicalEnrollment",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsMedicalClaimAttachment_hrmsMedicalClaim_MedicalClaimId",
                schema: "dbo",
                table: "hrmsMedicalClaimAttachment",
                column: "MedicalClaimId",
                principalSchema: "dbo",
                principalTable: "hrmsMedicalClaim",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsMedicalEnrollment_hrmsEmployee_EmployeeId",
                schema: "dbo",
                table: "hrmsMedicalEnrollment",
                column: "EmployeeId",
                principalSchema: "dbo",
                principalTable: "hrmsEmployee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsMedicalEnrollment_hrmsMedicalPlan_MedicalPlanId",
                schema: "dbo",
                table: "hrmsMedicalEnrollment",
                column: "MedicalPlanId",
                principalSchema: "dbo",
                principalTable: "hrmsMedicalPlan",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsMedicalServiceContract_hrmsMedicalProvider_MedicalProviderId",
                schema: "dbo",
                table: "hrmsMedicalServiceContract",
                column: "MedicalProviderId",
                principalSchema: "dbo",
                principalTable: "hrmsMedicalProvider",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsMentorship_hrmsEmployee_MenteeEmployeeId",
                schema: "dbo",
                table: "hrmsMentorship",
                column: "MenteeEmployeeId",
                principalSchema: "dbo",
                principalTable: "hrmsEmployee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsMentorship_hrmsEmployee_MentorEmployeeId",
                schema: "dbo",
                table: "hrmsMentorship",
                column: "MentorEmployeeId",
                principalSchema: "dbo",
                principalTable: "hrmsEmployee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsOrganizationalObjective_hrmsOrganizationUnit_OrganizationUnitId",
                schema: "dbo",
                table: "hrmsOrganizationalObjective",
                column: "OrganizationUnitId",
                principalSchema: "dbo",
                principalTable: "hrmsOrganizationUnit",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsOrganizationalObjective_hrmsOrganizationalObjective_ParentObjectiveId",
                schema: "dbo",
                table: "hrmsOrganizationalObjective",
                column: "ParentObjectiveId",
                principalSchema: "dbo",
                principalTable: "hrmsOrganizationalObjective",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsOrganizationalObjective_hrmsReviewCycle_ReviewCycleId",
                schema: "dbo",
                table: "hrmsOrganizationalObjective",
                column: "ReviewCycleId",
                principalSchema: "dbo",
                principalTable: "hrmsReviewCycle",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsOrganizationUnit_hrmsBranch_BranchId",
                schema: "dbo",
                table: "hrmsOrganizationUnit",
                column: "BranchId",
                principalSchema: "dbo",
                principalTable: "hrmsBranch",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsOrganizationUnit_hrmsOrganizationUnit_ParentId",
                schema: "dbo",
                table: "hrmsOrganizationUnit",
                column: "ParentId",
                principalSchema: "dbo",
                principalTable: "hrmsOrganizationUnit",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsOrganizationUnit_hrmsWorkLocation_WorkLocationId",
                schema: "dbo",
                table: "hrmsOrganizationUnit",
                column: "WorkLocationId",
                principalSchema: "dbo",
                principalTable: "hrmsWorkLocation",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsOtherLeave_hrmsEmployee_EmployeeId",
                schema: "dbo",
                table: "hrmsOtherLeave",
                column: "EmployeeId",
                principalSchema: "dbo",
                principalTable: "hrmsEmployee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsOtherLeave_hrmsOtherLeaveSetting_OtherLeaveSettingId",
                schema: "dbo",
                table: "hrmsOtherLeave",
                column: "OtherLeaveSettingId",
                principalSchema: "dbo",
                principalTable: "hrmsOtherLeaveSetting",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsOtherLeaveDetail_hrmsOtherLeave_OtherLeaveHeaderId",
                schema: "dbo",
                table: "hrmsOtherLeaveDetail",
                column: "OtherLeaveHeaderId",
                principalSchema: "dbo",
                principalTable: "hrmsOtherLeave",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsOtherLeaveSetting_FiscalYear_FiscalYearId",
                schema: "dbo",
                table: "hrmsOtherLeaveSetting",
                column: "FiscalYearId",
                principalSchema: "Core",
                principalTable: "FiscalYear",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsOtherLeaveSetting_hrmsLeaveType_LeaveTypeId",
                schema: "dbo",
                table: "hrmsOtherLeaveSetting",
                column: "LeaveTypeId",
                principalSchema: "dbo",
                principalTable: "hrmsLeaveType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsPerDiemRate_hrmsJobGrade_JobGradeId",
                schema: "dbo",
                table: "hrmsPerDiemRate",
                column: "JobGradeId",
                principalSchema: "dbo",
                principalTable: "hrmsJobGrade",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsPipObjective_hrmsImprovementPlan_PipId",
                schema: "dbo",
                table: "hrmsPipObjective",
                column: "PipId",
                principalSchema: "dbo",
                principalTable: "hrmsImprovementPlan",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsPosition_hrmsBranch_BranchId",
                schema: "dbo",
                table: "hrmsPosition",
                column: "BranchId",
                principalSchema: "dbo",
                principalTable: "hrmsBranch",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsPosition_hrmsOrganizationUnit_OrganizationUnitId",
                schema: "dbo",
                table: "hrmsPosition",
                column: "OrganizationUnitId",
                principalSchema: "dbo",
                principalTable: "hrmsOrganizationUnit",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsPosition_hrmsPositionClass_PositionClassId",
                schema: "dbo",
                table: "hrmsPosition",
                column: "PositionClassId",
                principalSchema: "dbo",
                principalTable: "hrmsPositionClass",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsPositionClass_coreSalaryScale_SalaryScaleId",
                schema: "dbo",
                table: "hrmsPositionClass",
                column: "SalaryScaleId",
                principalSchema: "Core",
                principalTable: "coreSalaryScale",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsPositionClass_hrmsJobCategory_JobCategoryId",
                schema: "dbo",
                table: "hrmsPositionClass",
                column: "JobCategoryId",
                principalSchema: "dbo",
                principalTable: "hrmsJobCategory",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsPositionClass_hrmsPositionClass_ReportsToPositionClassId",
                schema: "dbo",
                table: "hrmsPositionClass",
                column: "ReportsToPositionClassId",
                principalSchema: "dbo",
                principalTable: "hrmsPositionClass",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsPositionClass_hrmsWorkLocation_WorkLocationId",
                schema: "dbo",
                table: "hrmsPositionClass",
                column: "WorkLocationId",
                principalSchema: "dbo",
                principalTable: "hrmsWorkLocation",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsPositionCompetency_hrmsCompetency_CompetencyId",
                schema: "dbo",
                table: "hrmsPositionCompetency",
                column: "CompetencyId",
                principalSchema: "dbo",
                principalTable: "hrmsCompetency",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsPositionCompetency_hrmsPosition_PositionId",
                schema: "dbo",
                table: "hrmsPositionCompetency",
                column: "PositionId",
                principalSchema: "dbo",
                principalTable: "hrmsPosition",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsProfileChangeRequest_hrmsEmployee_EmployeeId",
                schema: "dbo",
                table: "hrmsProfileChangeRequest",
                column: "EmployeeId",
                principalSchema: "dbo",
                principalTable: "hrmsEmployee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsRatingScaleLevel_hrmsRatingScale_RatingScaleId",
                schema: "dbo",
                table: "hrmsRatingScaleLevel",
                column: "RatingScaleId",
                principalSchema: "dbo",
                principalTable: "hrmsRatingScale",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsRecognitionBadge_hrmsAwardCategory_AwardCategoryId",
                schema: "dbo",
                table: "hrmsRecognitionBadge",
                column: "AwardCategoryId",
                principalSchema: "dbo",
                principalTable: "hrmsAwardCategory",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsRecognitionProgram_hrmsRecognitionBadge_RecognitionBadgeId",
                schema: "dbo",
                table: "hrmsRecognitionProgram",
                column: "RecognitionBadgeId",
                principalSchema: "dbo",
                principalTable: "hrmsRecognitionBadge",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsReportField_hrmsReport_ReportId",
                schema: "dbo",
                table: "hrmsReportField",
                column: "ReportId",
                principalSchema: "dbo",
                principalTable: "hrmsReport",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsReportFieldOutput_hrmsReport_ReportId",
                schema: "dbo",
                table: "hrmsReportFieldOutput",
                column: "ReportId",
                principalSchema: "dbo",
                principalTable: "hrmsReport",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsReportRestriction_hrmsReport_ReportId",
                schema: "dbo",
                table: "hrmsReportRestriction",
                column: "ReportId",
                principalSchema: "dbo",
                principalTable: "hrmsReport",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsReportRunRecipient_hrmsReportRun_ReportRunId",
                schema: "dbo",
                table: "hrmsReportRunRecipient",
                column: "ReportRunId",
                principalSchema: "dbo",
                principalTable: "hrmsReportRun",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsReportSavedFilter_hrmsReport_ReportId",
                schema: "dbo",
                table: "hrmsReportSavedFilter",
                column: "ReportId",
                principalSchema: "dbo",
                principalTable: "hrmsReport",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsReportSchedule_hrmsReport_ReportId",
                schema: "dbo",
                table: "hrmsReportSchedule",
                column: "ReportId",
                principalSchema: "dbo",
                principalTable: "hrmsReport",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsReportScheduleFieldOutput_hrmsReportSchedule_ReportScheduleId",
                schema: "dbo",
                table: "hrmsReportScheduleFieldOutput",
                column: "ReportScheduleId",
                principalSchema: "dbo",
                principalTable: "hrmsReportSchedule",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsReportScheduleFieldValue_hrmsReportSchedule_ReportScheduleId",
                schema: "dbo",
                table: "hrmsReportScheduleFieldValue",
                column: "ReportScheduleId",
                principalSchema: "dbo",
                principalTable: "hrmsReportSchedule",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsReportScheduleRecipient_hrmsReportSchedule_ReportScheduleId",
                schema: "dbo",
                table: "hrmsReportScheduleRecipient",
                column: "ReportScheduleId",
                principalSchema: "dbo",
                principalTable: "hrmsReportSchedule",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsRequisitionScreeningCriterion_hrmsJobRequisition_RequisitionId",
                schema: "dbo",
                table: "hrmsRequisitionScreeningCriterion",
                column: "RequisitionId",
                principalSchema: "dbo",
                principalTable: "hrmsJobRequisition",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsReviewCycle_FiscalYear_FiscalYearId",
                schema: "dbo",
                table: "hrmsReviewCycle",
                column: "FiscalYearId",
                principalSchema: "Core",
                principalTable: "FiscalYear",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsReviewCycle_hrmsRatingScale_RatingScaleId",
                schema: "dbo",
                table: "hrmsReviewCycle",
                column: "RatingScaleId",
                principalSchema: "dbo",
                principalTable: "hrmsRatingScale",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsRewardDisbursement_hrmsEmployeeRecognition_EmployeeRecognitionId",
                schema: "dbo",
                table: "hrmsRewardDisbursement",
                column: "EmployeeRecognitionId",
                principalSchema: "dbo",
                principalTable: "hrmsEmployeeRecognition",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsRewardDisbursement_hrmsEmployee_EmployeeId",
                schema: "dbo",
                table: "hrmsRewardDisbursement",
                column: "EmployeeId",
                principalSchema: "dbo",
                principalTable: "hrmsEmployee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsRewardDisbursement_hrmsRecognitionBadge_RecognitionBadgeId",
                schema: "dbo",
                table: "hrmsRewardDisbursement",
                column: "RecognitionBadgeId",
                principalSchema: "dbo",
                principalTable: "hrmsRecognitionBadge",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsRewardNomination_hrmsEmployee_NomineeEmployeeId",
                schema: "dbo",
                table: "hrmsRewardNomination",
                column: "NomineeEmployeeId",
                principalSchema: "dbo",
                principalTable: "hrmsEmployee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsRewardNomination_hrmsRecognitionBadge_RecognitionBadgeId",
                schema: "dbo",
                table: "hrmsRewardNomination",
                column: "RecognitionBadgeId",
                principalSchema: "dbo",
                principalTable: "hrmsRecognitionBadge",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsRewardNomination_hrmsRecognitionProgram_RecognitionProgramId",
                schema: "dbo",
                table: "hrmsRewardNomination",
                column: "RecognitionProgramId",
                principalSchema: "dbo",
                principalTable: "hrmsRecognitionProgram",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsRewardPointsTransaction_hrmsEmployee_EmployeeId",
                schema: "dbo",
                table: "hrmsRewardPointsTransaction",
                column: "EmployeeId",
                principalSchema: "dbo",
                principalTable: "hrmsEmployee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsSalaryRevisionBand_hrmsSalaryRevision_SalaryRevisionId",
                schema: "dbo",
                table: "hrmsSalaryRevisionBand",
                column: "SalaryRevisionId",
                principalSchema: "dbo",
                principalTable: "hrmsSalaryRevision",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsSalaryRevisionLine_hrmsEmployee_EmployeeId",
                schema: "dbo",
                table: "hrmsSalaryRevisionLine",
                column: "EmployeeId",
                principalSchema: "dbo",
                principalTable: "hrmsEmployee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsSalaryRevisionLine_hrmsSalaryRevision_SalaryRevisionId",
                schema: "dbo",
                table: "hrmsSalaryRevisionLine",
                column: "SalaryRevisionId",
                principalSchema: "dbo",
                principalTable: "hrmsSalaryRevision",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsSettlementLine_hrmsTerminationSettlement_TerminationSettlementId",
                schema: "dbo",
                table: "hrmsSettlementLine",
                column: "TerminationSettlementId",
                principalSchema: "dbo",
                principalTable: "hrmsTerminationSettlement",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsSuccessionCandidate_hrmsEmployee_EmployeeId",
                schema: "dbo",
                table: "hrmsSuccessionCandidate",
                column: "EmployeeId",
                principalSchema: "dbo",
                principalTable: "hrmsEmployee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsSuccessionCandidate_hrmsSuccessionPlan_SuccessionPlanId",
                schema: "dbo",
                table: "hrmsSuccessionCandidate",
                column: "SuccessionPlanId",
                principalSchema: "dbo",
                principalTable: "hrmsSuccessionPlan",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsSuccessionDevelopmentAction_hrmsEmployee_MentorEmployeeId",
                schema: "dbo",
                table: "hrmsSuccessionDevelopmentAction",
                column: "MentorEmployeeId",
                principalSchema: "dbo",
                principalTable: "hrmsEmployee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsSuccessionDevelopmentAction_hrmsSuccessionCandidate_SuccessionCandidateId",
                schema: "dbo",
                table: "hrmsSuccessionDevelopmentAction",
                column: "SuccessionCandidateId",
                principalSchema: "dbo",
                principalTable: "hrmsSuccessionCandidate",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsSuccessionPlan_hrmsCriticalPosition_CriticalPositionId",
                schema: "dbo",
                table: "hrmsSuccessionPlan",
                column: "CriticalPositionId",
                principalSchema: "dbo",
                principalTable: "hrmsCriticalPosition",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsSurveyCompletion_hrmsEmployee_EmployeeId",
                schema: "dbo",
                table: "hrmsSurveyCompletion",
                column: "EmployeeId",
                principalSchema: "dbo",
                principalTable: "hrmsEmployee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsSurveyCompletion_hrmsSurvey_SurveyId",
                schema: "dbo",
                table: "hrmsSurveyCompletion",
                column: "SurveyId",
                principalSchema: "dbo",
                principalTable: "hrmsSurvey",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsSurveyResponse_hrmsSurvey_SurveyId",
                schema: "dbo",
                table: "hrmsSurveyResponse",
                column: "SurveyId",
                principalSchema: "dbo",
                principalTable: "hrmsSurvey",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsTalentAssessment_hrmsEmployee_EmployeeId",
                schema: "dbo",
                table: "hrmsTalentAssessment",
                column: "EmployeeId",
                principalSchema: "dbo",
                principalTable: "hrmsEmployee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsTalentAssessment_hrmsTalentReview_TalentReviewId",
                schema: "dbo",
                table: "hrmsTalentAssessment",
                column: "TalentReviewId",
                principalSchema: "dbo",
                principalTable: "hrmsTalentReview",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsTalentRating_hrmsEmployee_RaterEmployeeId",
                schema: "dbo",
                table: "hrmsTalentRating",
                column: "RaterEmployeeId",
                principalSchema: "dbo",
                principalTable: "hrmsEmployee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsTalentRating_hrmsTalentAssessment_TalentAssessmentId",
                schema: "dbo",
                table: "hrmsTalentRating",
                column: "TalentAssessmentId",
                principalSchema: "dbo",
                principalTable: "hrmsTalentAssessment",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsTalentReview_hrmsOrganizationUnit_OrganizationUnitId",
                schema: "dbo",
                table: "hrmsTalentReview",
                column: "OrganizationUnitId",
                principalSchema: "dbo",
                principalTable: "hrmsOrganizationUnit",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsTerminationAssetRecovery_hrmsCompanyAsset_CompanyAssetId",
                schema: "dbo",
                table: "hrmsTerminationAssetRecovery",
                column: "CompanyAssetId",
                principalSchema: "dbo",
                principalTable: "hrmsCompanyAsset",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsTerminationAssetRecovery_hrmsEmployeeTermination_TerminationId",
                schema: "dbo",
                table: "hrmsTerminationAssetRecovery",
                column: "TerminationId",
                principalSchema: "dbo",
                principalTable: "hrmsEmployeeTermination",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsTerminationClearance_hrmsClearanceDepartment_DepartmentId",
                schema: "dbo",
                table: "hrmsTerminationClearance",
                column: "DepartmentId",
                principalSchema: "dbo",
                principalTable: "hrmsClearanceDepartment",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsTerminationClearance_hrmsEmployeeTermination_TerminationId",
                schema: "dbo",
                table: "hrmsTerminationClearance",
                column: "TerminationId",
                principalSchema: "dbo",
                principalTable: "hrmsEmployeeTermination",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsTerminationSettlement_hrmsEmployeeTermination_TerminationId",
                schema: "dbo",
                table: "hrmsTerminationSettlement",
                column: "TerminationId",
                principalSchema: "dbo",
                principalTable: "hrmsEmployeeTermination",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsTrainingBudget_hrmsOrganizationUnit_OrganizationUnitId",
                schema: "dbo",
                table: "hrmsTrainingBudget",
                column: "OrganizationUnitId",
                principalSchema: "dbo",
                principalTable: "hrmsOrganizationUnit",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsTrainingCourse_hrmsTrainingCategory_TrainingCategoryId",
                schema: "dbo",
                table: "hrmsTrainingCourse",
                column: "TrainingCategoryId",
                principalSchema: "dbo",
                principalTable: "hrmsTrainingCategory",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsTrainingEnrollment_hrmsEmployee_EmployeeId",
                schema: "dbo",
                table: "hrmsTrainingEnrollment",
                column: "EmployeeId",
                principalSchema: "dbo",
                principalTable: "hrmsEmployee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsTrainingEnrollment_hrmsTrainingNeed_TrainingNeedId",
                schema: "dbo",
                table: "hrmsTrainingEnrollment",
                column: "TrainingNeedId",
                principalSchema: "dbo",
                principalTable: "hrmsTrainingNeed",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsTrainingEnrollment_hrmsTrainingSession_TrainingSessionId",
                schema: "dbo",
                table: "hrmsTrainingEnrollment",
                column: "TrainingSessionId",
                principalSchema: "dbo",
                principalTable: "hrmsTrainingSession",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsTrainingNeed_hrmsCompetency_CompetencyId",
                schema: "dbo",
                table: "hrmsTrainingNeed",
                column: "CompetencyId",
                principalSchema: "dbo",
                principalTable: "hrmsCompetency",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsTrainingNeed_hrmsEmployee_EmployeeId",
                schema: "dbo",
                table: "hrmsTrainingNeed",
                column: "EmployeeId",
                principalSchema: "dbo",
                principalTable: "hrmsEmployee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsTrainingNeed_hrmsTrainingCourse_TrainingCourseId",
                schema: "dbo",
                table: "hrmsTrainingNeed",
                column: "TrainingCourseId",
                principalSchema: "dbo",
                principalTable: "hrmsTrainingCourse",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsTrainingProviderPayment_hrmsTrainingSession_TrainingSessionId",
                schema: "dbo",
                table: "hrmsTrainingProviderPayment",
                column: "TrainingSessionId",
                principalSchema: "dbo",
                principalTable: "hrmsTrainingSession",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsTrainingSession_hrmsTrainingCourse_TrainingCourseId",
                schema: "dbo",
                table: "hrmsTrainingSession",
                column: "TrainingCourseId",
                principalSchema: "dbo",
                principalTable: "hrmsTrainingCourse",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsTripBudget_hrmsOrganizationUnit_OrganizationUnitId",
                schema: "dbo",
                table: "hrmsTripBudget",
                column: "OrganizationUnitId",
                principalSchema: "dbo",
                principalTable: "hrmsOrganizationUnit",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsTripExpense_hrmsTripRequest_TripRequestId",
                schema: "dbo",
                table: "hrmsTripExpense",
                column: "TripRequestId",
                principalSchema: "dbo",
                principalTable: "hrmsTripRequest",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsTripRequest_hrmsEmployee_EmployeeId",
                schema: "dbo",
                table: "hrmsTripRequest",
                column: "EmployeeId",
                principalSchema: "dbo",
                principalTable: "hrmsEmployee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsTripRequest_hrmsTripBudget_TripBudgetId",
                schema: "dbo",
                table: "hrmsTripRequest",
                column: "TripBudgetId",
                principalSchema: "dbo",
                principalTable: "hrmsTripBudget",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsWorkflowActionLog_hrmsWorkflowInstance_InstanceId",
                schema: "dbo",
                table: "hrmsWorkflowActionLog",
                column: "InstanceId",
                principalSchema: "dbo",
                principalTable: "hrmsWorkflowInstance",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsWorkflowInstance_hrmsWorkflowDefinition_DefinitionId",
                schema: "dbo",
                table: "hrmsWorkflowInstance",
                column: "DefinitionId",
                principalSchema: "dbo",
                principalTable: "hrmsWorkflowDefinition",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsWorkflowStep_hrmsWorkflowDefinition_DefinitionId",
                schema: "dbo",
                table: "hrmsWorkflowStep",
                column: "DefinitionId",
                principalSchema: "dbo",
                principalTable: "hrmsWorkflowDefinition",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsWorkflowStepApprover_hrmsWorkflowStep_StepId",
                schema: "dbo",
                table: "hrmsWorkflowStepApprover",
                column: "StepId",
                principalSchema: "dbo",
                principalTable: "hrmsWorkflowStep",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsWorkforcePlan_FiscalYear_StartFiscalYearId",
                schema: "dbo",
                table: "hrmsWorkforcePlan",
                column: "StartFiscalYearId",
                principalSchema: "Core",
                principalTable: "FiscalYear",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsWorkforcePlan_hrmsOrganizationUnit_OrganizationUnitId",
                schema: "dbo",
                table: "hrmsWorkforcePlan",
                column: "OrganizationUnitId",
                principalSchema: "dbo",
                principalTable: "hrmsOrganizationUnit",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsWorkforcePlanLine_hrmsOrganizationUnit_OrganizationUnitId",
                schema: "dbo",
                table: "hrmsWorkforcePlanLine",
                column: "OrganizationUnitId",
                principalSchema: "dbo",
                principalTable: "hrmsOrganizationUnit",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsWorkforcePlanLine_hrmsPositionClass_PositionClassId",
                schema: "dbo",
                table: "hrmsWorkforcePlanLine",
                column: "PositionClassId",
                principalSchema: "dbo",
                principalTable: "hrmsPositionClass",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsWorkforcePlanLine_hrmsWorkforcePlan_PlanId",
                schema: "dbo",
                table: "hrmsWorkforcePlanLine",
                column: "PlanId",
                principalSchema: "dbo",
                principalTable: "hrmsWorkforcePlan",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsWorkLocation_hrmsWorkLocation_ParentId",
                schema: "dbo",
                table: "hrmsWorkLocation",
                column: "ParentId",
                principalSchema: "dbo",
                principalTable: "hrmsWorkLocation",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RolePermission_coreOperation_OperationId",
                schema: "Core",
                table: "RolePermission",
                column: "OperationId",
                principalSchema: "dbo",
                principalTable: "coreOperation",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_User_hrmsEmployee_EmployeeId",
                schema: "Core",
                table: "User",
                column: "EmployeeId",
                principalSchema: "dbo",
                principalTable: "hrmsEmployee",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        
            RevertProcedures(migrationBuilder);
        }
    
        /// <summary>
        /// Recreates the 28 report procedures under the Hrms schema with their bodies rewritten to the
        /// new table names. EF does not track procedures, so this rides in the SAME migration as the
        /// table renames — split across two migrations there would be a window where the procedures
        /// still point at tables that no longer exist.
        /// </summary>
        private static void UpdateProcedures(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"-- ===BATCH===
IF OBJECT_ID('[Core].[hrms_ReportActivate]', 'P') IS NOT NULL DROP PROCEDURE [Core].[hrms_ReportActivate];");
            migrationBuilder.Sql(@"-- ===BATCH===
IF OBJECT_ID('[Core].[hrms_ReportClientSchedule]', 'P') IS NOT NULL DROP PROCEDURE [Core].[hrms_ReportClientSchedule];");
            migrationBuilder.Sql(@"-- ===BATCH===
IF OBJECT_ID('[Core].[hrms_ReportClientScheduleDelete]', 'P') IS NOT NULL DROP PROCEDURE [Core].[hrms_ReportClientScheduleDelete];");
            migrationBuilder.Sql(@"-- ===BATCH===
IF OBJECT_ID('[Core].[hrms_ReportClientScheduleEnable]', 'P') IS NOT NULL DROP PROCEDURE [Core].[hrms_ReportClientScheduleEnable];");
            migrationBuilder.Sql(@"-- ===BATCH===
IF OBJECT_ID('[Core].[hrms_ReportClientScheduleFieldOutput]', 'P') IS NOT NULL DROP PROCEDURE [Core].[hrms_ReportClientScheduleFieldOutput];");
            migrationBuilder.Sql(@"-- ===BATCH===
IF OBJECT_ID('[Core].[hrms_ReportClientScheduleFieldValue]', 'P') IS NOT NULL DROP PROCEDURE [Core].[hrms_ReportClientScheduleFieldValue];");
            migrationBuilder.Sql(@"-- ===BATCH===
IF OBJECT_ID('[Core].[hrms_ReportClientScheduleRead]', 'P') IS NOT NULL DROP PROCEDURE [Core].[hrms_ReportClientScheduleRead];");
            migrationBuilder.Sql(@"-- ===BATCH===
IF OBJECT_ID('[Core].[hrms_ReportClientScheduleRecipient]', 'P') IS NOT NULL DROP PROCEDURE [Core].[hrms_ReportClientScheduleRecipient];");
            migrationBuilder.Sql(@"-- ===BATCH===
IF OBJECT_ID('[Core].[hrms_ReportDelete]', 'P') IS NOT NULL DROP PROCEDURE [Core].[hrms_ReportDelete];");
            migrationBuilder.Sql(@"-- ===BATCH===
IF OBJECT_ID('[Core].[hrms_ReportFieldOutputRead]', 'P') IS NOT NULL DROP PROCEDURE [Core].[hrms_ReportFieldOutputRead];");
            migrationBuilder.Sql(@"-- ===BATCH===
IF OBJECT_ID('[Core].[hrms_ReportFieldValues]', 'P') IS NOT NULL DROP PROCEDURE [Core].[hrms_ReportFieldValues];");
            migrationBuilder.Sql(@"-- ===BATCH===
IF OBJECT_ID('[Core].[hrms_ReportGenerateGetScheduleInfo]', 'P') IS NOT NULL DROP PROCEDURE [Core].[hrms_ReportGenerateGetScheduleInfo];");
            migrationBuilder.Sql(@"-- ===BATCH===
IF OBJECT_ID('[Core].[hrms_ReportGenerateSendToHistory]', 'P') IS NOT NULL DROP PROCEDURE [Core].[hrms_ReportGenerateSendToHistory];");
            migrationBuilder.Sql(@"-- ===BATCH===
IF OBJECT_ID('[Core].[hrms_Report_DisciplinaryCases]', 'P') IS NOT NULL DROP PROCEDURE [Core].[hrms_Report_DisciplinaryCases];");
            migrationBuilder.Sql(@"-- ===BATCH===
IF OBJECT_ID('[Core].[hrms_Report_EmployeeDemographics]', 'P') IS NOT NULL DROP PROCEDURE [Core].[hrms_Report_EmployeeDemographics];");
            migrationBuilder.Sql(@"-- ===BATCH===
IF OBJECT_ID('[Core].[hrms_Report_EmployeeDirectory]', 'P') IS NOT NULL DROP PROCEDURE [Core].[hrms_Report_EmployeeDirectory];");
            migrationBuilder.Sql(@"-- ===BATCH===
IF OBJECT_ID('[Core].[hrms_Report_EmployeeDirectoryGrouped]', 'P') IS NOT NULL DROP PROCEDURE [Core].[hrms_Report_EmployeeDirectoryGrouped];");
            migrationBuilder.Sql(@"-- ===BATCH===
IF OBJECT_ID('[Core].[hrms_Report_EmployeeMovements]', 'P') IS NOT NULL DROP PROCEDURE [Core].[hrms_Report_EmployeeMovements];");
            migrationBuilder.Sql(@"-- ===BATCH===
IF OBJECT_ID('[Core].[hrms_Report_HeadcountByUnit]', 'P') IS NOT NULL DROP PROCEDURE [Core].[hrms_Report_HeadcountByUnit];");
            migrationBuilder.Sql(@"-- ===BATCH===
IF OBJECT_ID('[Core].[hrms_Report_LeaveBalances]', 'P') IS NOT NULL DROP PROCEDURE [Core].[hrms_Report_LeaveBalances];");
            migrationBuilder.Sql(@"-- ===BATCH===
IF OBJECT_ID('[Core].[hrms_Report_LeaveTaken]', 'P') IS NOT NULL DROP PROCEDURE [Core].[hrms_Report_LeaveTaken];");
            migrationBuilder.Sql(@"-- ===BATCH===
IF OBJECT_ID('[Core].[hrms_Report_NewHires]', 'P') IS NOT NULL DROP PROCEDURE [Core].[hrms_Report_NewHires];");
            migrationBuilder.Sql(@"-- ===BATCH===
IF OBJECT_ID('[Core].[hrms_Report_ProbationTracking]', 'P') IS NOT NULL DROP PROCEDURE [Core].[hrms_Report_ProbationTracking];");
            migrationBuilder.Sql(@"-- ===BATCH===
IF OBJECT_ID('[Core].[hrms_Report_RecruitmentPipeline]', 'P') IS NOT NULL DROP PROCEDURE [Core].[hrms_Report_RecruitmentPipeline];");
            migrationBuilder.Sql(@"-- ===BATCH===
IF OBJECT_ID('[Core].[hrms_Report_SalaryRegister]', 'P') IS NOT NULL DROP PROCEDURE [Core].[hrms_Report_SalaryRegister];");
            migrationBuilder.Sql(@"-- ===BATCH===
IF OBJECT_ID('[Core].[hrms_Report_Terminations]', 'P') IS NOT NULL DROP PROCEDURE [Core].[hrms_Report_Terminations];");
            migrationBuilder.Sql(@"-- ===BATCH===
IF OBJECT_ID('[Core].[hrms_Report_TrainingCompletion]', 'P') IS NOT NULL DROP PROCEDURE [Core].[hrms_Report_TrainingCompletion];");
            migrationBuilder.Sql(@"-- ===BATCH===
IF OBJECT_ID('[Core].[hrms_Report_VacantPositions]', 'P') IS NOT NULL DROP PROCEDURE [Core].[hrms_Report_VacantPositions];");

            migrationBuilder.Sql(@"-- ===BATCH===
IF OBJECT_ID('[Hrms].[ReportActivate]', 'P') IS NOT NULL DROP PROCEDURE [Hrms].[ReportActivate];");
            migrationBuilder.Sql(@"-- ===BATCH===
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
END");

            migrationBuilder.Sql(@"-- ===BATCH===
IF OBJECT_ID('[Hrms].[ReportClientSchedule]', 'P') IS NOT NULL DROP PROCEDURE [Hrms].[ReportClientSchedule];");
            migrationBuilder.Sql(@"-- ===BATCH===
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
END");

            migrationBuilder.Sql(@"-- ===BATCH===
IF OBJECT_ID('[Hrms].[ReportClientScheduleDelete]', 'P') IS NOT NULL DROP PROCEDURE [Hrms].[ReportClientScheduleDelete];");
            migrationBuilder.Sql(@"-- ===BATCH===
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
END");

            migrationBuilder.Sql(@"-- ===BATCH===
IF OBJECT_ID('[Hrms].[ReportClientScheduleEnable]', 'P') IS NOT NULL DROP PROCEDURE [Hrms].[ReportClientScheduleEnable];");
            migrationBuilder.Sql(@"-- ===BATCH===
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
END");

            migrationBuilder.Sql(@"-- ===BATCH===
IF OBJECT_ID('[Hrms].[ReportClientScheduleFieldOutput]', 'P') IS NOT NULL DROP PROCEDURE [Hrms].[ReportClientScheduleFieldOutput];");
            migrationBuilder.Sql(@"-- ===BATCH===
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
END");

            migrationBuilder.Sql(@"-- ===BATCH===
IF OBJECT_ID('[Hrms].[ReportClientScheduleFieldValue]', 'P') IS NOT NULL DROP PROCEDURE [Hrms].[ReportClientScheduleFieldValue];");
            migrationBuilder.Sql(@"-- ===BATCH===
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
END");

            migrationBuilder.Sql(@"-- ===BATCH===
IF OBJECT_ID('[Hrms].[ReportClientScheduleRead]', 'P') IS NOT NULL DROP PROCEDURE [Hrms].[ReportClientScheduleRead];");
            migrationBuilder.Sql(@"-- ===BATCH===
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
END");

            migrationBuilder.Sql(@"-- ===BATCH===
IF OBJECT_ID('[Hrms].[ReportClientScheduleRecipient]', 'P') IS NOT NULL DROP PROCEDURE [Hrms].[ReportClientScheduleRecipient];");
            migrationBuilder.Sql(@"-- ===BATCH===
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
END");

            migrationBuilder.Sql(@"-- ===BATCH===
IF OBJECT_ID('[Hrms].[ReportDelete]', 'P') IS NOT NULL DROP PROCEDURE [Hrms].[ReportDelete];");
            migrationBuilder.Sql(@"-- ===BATCH===
CREATE OR ALTER PROCEDURE [Hrms].[ReportDelete]
    @ReportId UNIQUEIDENTIFIER,
    @TenantId NVARCHAR(450) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM Hrms.Report
     WHERE Id = @ReportId
       AND (@TenantId IS NULL OR @TenantId = '' OR TenantId = @TenantId);
END");

            migrationBuilder.Sql(@"-- ===BATCH===
IF OBJECT_ID('[Hrms].[ReportFieldOutputRead]', 'P') IS NOT NULL DROP PROCEDURE [Hrms].[ReportFieldOutputRead];");
            migrationBuilder.Sql(@"-- ===BATCH===
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
END");

            migrationBuilder.Sql(@"-- ===BATCH===
IF OBJECT_ID('[Hrms].[ReportFieldValues]', 'P') IS NOT NULL DROP PROCEDURE [Hrms].[ReportFieldValues];");
            migrationBuilder.Sql(@"-- ===BATCH===
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
END");

            migrationBuilder.Sql(@"-- ===BATCH===
IF OBJECT_ID('[Hrms].[ReportGenerateGetScheduleInfo]', 'P') IS NOT NULL DROP PROCEDURE [Hrms].[ReportGenerateGetScheduleInfo];");
            migrationBuilder.Sql(@"-- ===BATCH===
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
END");

            migrationBuilder.Sql(@"-- ===BATCH===
IF OBJECT_ID('[Hrms].[ReportGenerateSendToHistory]', 'P') IS NOT NULL DROP PROCEDURE [Hrms].[ReportGenerateSendToHistory];");
            migrationBuilder.Sql(@"-- ===BATCH===
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
END");

            migrationBuilder.Sql(@"-- ===BATCH===
IF OBJECT_ID('[Hrms].[Report_DisciplinaryCases]', 'P') IS NOT NULL DROP PROCEDURE [Hrms].[Report_DisciplinaryCases];");
            migrationBuilder.Sql(@"-- ===BATCH===
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
END");

            migrationBuilder.Sql(@"-- ===BATCH===
IF OBJECT_ID('[Hrms].[Report_EmployeeDemographics]', 'P') IS NOT NULL DROP PROCEDURE [Hrms].[Report_EmployeeDemographics];");
            migrationBuilder.Sql(@"-- ===BATCH===
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

    DECLARE @gbJson NVARCHAR(MAX) = '[""' + REPLACE(REPLACE(@groupBy, ' ', ''), ',', '"",""') + '""]';
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
END");

            migrationBuilder.Sql(@"-- ===BATCH===
IF OBJECT_ID('[Hrms].[Report_EmployeeDirectory]', 'P') IS NOT NULL DROP PROCEDURE [Hrms].[Report_EmployeeDirectory];");
            migrationBuilder.Sql(@"-- ===BATCH===
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
END");

            migrationBuilder.Sql(@"-- ===BATCH===
IF OBJECT_ID('[Hrms].[Report_EmployeeDirectoryGrouped]', 'P') IS NOT NULL DROP PROCEDURE [Hrms].[Report_EmployeeDirectoryGrouped];");
            migrationBuilder.Sql(@"-- ===BATCH===
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
    DECLARE @gbJson NVARCHAR(MAX) = '[""' + REPLACE(REPLACE(@groupBy, ' ', ''), ',', '"",""') + '""]';
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
END");

            migrationBuilder.Sql(@"-- ===BATCH===
IF OBJECT_ID('[Hrms].[Report_EmployeeMovements]', 'P') IS NOT NULL DROP PROCEDURE [Hrms].[Report_EmployeeMovements];");
            migrationBuilder.Sql(@"-- ===BATCH===
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
END");

            migrationBuilder.Sql(@"-- ===BATCH===
IF OBJECT_ID('[Hrms].[Report_HeadcountByUnit]', 'P') IS NOT NULL DROP PROCEDURE [Hrms].[Report_HeadcountByUnit];");
            migrationBuilder.Sql(@"-- ===BATCH===
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

    DECLARE @gbJson NVARCHAR(MAX) = '[""' + REPLACE(REPLACE(@groupBy, ' ', ''), ',', '"",""') + '""]';
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
END");

            migrationBuilder.Sql(@"-- ===BATCH===
IF OBJECT_ID('[Hrms].[Report_LeaveBalances]', 'P') IS NOT NULL DROP PROCEDURE [Hrms].[Report_LeaveBalances];");
            migrationBuilder.Sql(@"-- ===BATCH===
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
END");

            migrationBuilder.Sql(@"-- ===BATCH===
IF OBJECT_ID('[Hrms].[Report_LeaveTaken]', 'P') IS NOT NULL DROP PROCEDURE [Hrms].[Report_LeaveTaken];");
            migrationBuilder.Sql(@"-- ===BATCH===
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
END");

            migrationBuilder.Sql(@"-- ===BATCH===
IF OBJECT_ID('[Hrms].[Report_NewHires]', 'P') IS NOT NULL DROP PROCEDURE [Hrms].[Report_NewHires];");
            migrationBuilder.Sql(@"-- ===BATCH===
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
END");

            migrationBuilder.Sql(@"-- ===BATCH===
IF OBJECT_ID('[Hrms].[Report_ProbationTracking]', 'P') IS NOT NULL DROP PROCEDURE [Hrms].[Report_ProbationTracking];");
            migrationBuilder.Sql(@"-- ===BATCH===
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
END");

            migrationBuilder.Sql(@"-- ===BATCH===
IF OBJECT_ID('[Hrms].[Report_RecruitmentPipeline]', 'P') IS NOT NULL DROP PROCEDURE [Hrms].[Report_RecruitmentPipeline];");
            migrationBuilder.Sql(@"-- ===BATCH===
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

    DECLARE @gbJson NVARCHAR(MAX) = '[""' + REPLACE(REPLACE(@groupBy, ' ', ''), ',', '"",""') + '""]';
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
END");

            migrationBuilder.Sql(@"-- ===BATCH===
IF OBJECT_ID('[Hrms].[Report_SalaryRegister]', 'P') IS NOT NULL DROP PROCEDURE [Hrms].[Report_SalaryRegister];");
            migrationBuilder.Sql(@"-- ===BATCH===
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

    DECLARE @gbJson NVARCHAR(MAX) = '[""' + REPLACE(REPLACE(@groupBy, ' ', ''), ',', '"",""') + '""]';
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
END");

            migrationBuilder.Sql(@"-- ===BATCH===
IF OBJECT_ID('[Hrms].[Report_Terminations]', 'P') IS NOT NULL DROP PROCEDURE [Hrms].[Report_Terminations];");
            migrationBuilder.Sql(@"-- ===BATCH===
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

    DECLARE @gbJson NVARCHAR(MAX) = '[""' + REPLACE(REPLACE(@groupBy, ' ', ''), ',', '"",""') + '""]';
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
END");

            migrationBuilder.Sql(@"-- ===BATCH===
IF OBJECT_ID('[Hrms].[Report_TrainingCompletion]', 'P') IS NOT NULL DROP PROCEDURE [Hrms].[Report_TrainingCompletion];");
            migrationBuilder.Sql(@"-- ===BATCH===
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
END");

            migrationBuilder.Sql(@"-- ===BATCH===
IF OBJECT_ID('[Hrms].[Report_VacantPositions]', 'P') IS NOT NULL DROP PROCEDURE [Hrms].[Report_VacantPositions];");
            migrationBuilder.Sql(@"-- ===BATCH===
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
END");

            // The report registry stores the procedure name it must execute.
            migrationBuilder.Sql(@"-- ===BATCH===
UPDATE [Hrms].[Report]
SET StoredProc = REPLACE(REPLACE(StoredProc, '[Core].[hrms_', '[Hrms].['), 'Core.hrms_', 'Hrms.')
WHERE StoredProc LIKE '%Core%hrms[_]%';");
        }

        private static void RevertProcedures(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"-- ===BATCH===
IF OBJECT_ID('[Hrms].[ReportActivate]', 'P') IS NOT NULL DROP PROCEDURE [Hrms].[ReportActivate];");
            migrationBuilder.Sql(@"-- ===BATCH===
IF OBJECT_ID('[Hrms].[ReportClientSchedule]', 'P') IS NOT NULL DROP PROCEDURE [Hrms].[ReportClientSchedule];");
            migrationBuilder.Sql(@"-- ===BATCH===
IF OBJECT_ID('[Hrms].[ReportClientScheduleDelete]', 'P') IS NOT NULL DROP PROCEDURE [Hrms].[ReportClientScheduleDelete];");
            migrationBuilder.Sql(@"-- ===BATCH===
IF OBJECT_ID('[Hrms].[ReportClientScheduleEnable]', 'P') IS NOT NULL DROP PROCEDURE [Hrms].[ReportClientScheduleEnable];");
            migrationBuilder.Sql(@"-- ===BATCH===
IF OBJECT_ID('[Hrms].[ReportClientScheduleFieldOutput]', 'P') IS NOT NULL DROP PROCEDURE [Hrms].[ReportClientScheduleFieldOutput];");
            migrationBuilder.Sql(@"-- ===BATCH===
IF OBJECT_ID('[Hrms].[ReportClientScheduleFieldValue]', 'P') IS NOT NULL DROP PROCEDURE [Hrms].[ReportClientScheduleFieldValue];");
            migrationBuilder.Sql(@"-- ===BATCH===
IF OBJECT_ID('[Hrms].[ReportClientScheduleRead]', 'P') IS NOT NULL DROP PROCEDURE [Hrms].[ReportClientScheduleRead];");
            migrationBuilder.Sql(@"-- ===BATCH===
IF OBJECT_ID('[Hrms].[ReportClientScheduleRecipient]', 'P') IS NOT NULL DROP PROCEDURE [Hrms].[ReportClientScheduleRecipient];");
            migrationBuilder.Sql(@"-- ===BATCH===
IF OBJECT_ID('[Hrms].[ReportDelete]', 'P') IS NOT NULL DROP PROCEDURE [Hrms].[ReportDelete];");
            migrationBuilder.Sql(@"-- ===BATCH===
IF OBJECT_ID('[Hrms].[ReportFieldOutputRead]', 'P') IS NOT NULL DROP PROCEDURE [Hrms].[ReportFieldOutputRead];");
            migrationBuilder.Sql(@"-- ===BATCH===
IF OBJECT_ID('[Hrms].[ReportFieldValues]', 'P') IS NOT NULL DROP PROCEDURE [Hrms].[ReportFieldValues];");
            migrationBuilder.Sql(@"-- ===BATCH===
IF OBJECT_ID('[Hrms].[ReportGenerateGetScheduleInfo]', 'P') IS NOT NULL DROP PROCEDURE [Hrms].[ReportGenerateGetScheduleInfo];");
            migrationBuilder.Sql(@"-- ===BATCH===
IF OBJECT_ID('[Hrms].[ReportGenerateSendToHistory]', 'P') IS NOT NULL DROP PROCEDURE [Hrms].[ReportGenerateSendToHistory];");
            migrationBuilder.Sql(@"-- ===BATCH===
IF OBJECT_ID('[Hrms].[Report_DisciplinaryCases]', 'P') IS NOT NULL DROP PROCEDURE [Hrms].[Report_DisciplinaryCases];");
            migrationBuilder.Sql(@"-- ===BATCH===
IF OBJECT_ID('[Hrms].[Report_EmployeeDemographics]', 'P') IS NOT NULL DROP PROCEDURE [Hrms].[Report_EmployeeDemographics];");
            migrationBuilder.Sql(@"-- ===BATCH===
IF OBJECT_ID('[Hrms].[Report_EmployeeDirectory]', 'P') IS NOT NULL DROP PROCEDURE [Hrms].[Report_EmployeeDirectory];");
            migrationBuilder.Sql(@"-- ===BATCH===
IF OBJECT_ID('[Hrms].[Report_EmployeeDirectoryGrouped]', 'P') IS NOT NULL DROP PROCEDURE [Hrms].[Report_EmployeeDirectoryGrouped];");
            migrationBuilder.Sql(@"-- ===BATCH===
IF OBJECT_ID('[Hrms].[Report_EmployeeMovements]', 'P') IS NOT NULL DROP PROCEDURE [Hrms].[Report_EmployeeMovements];");
            migrationBuilder.Sql(@"-- ===BATCH===
IF OBJECT_ID('[Hrms].[Report_HeadcountByUnit]', 'P') IS NOT NULL DROP PROCEDURE [Hrms].[Report_HeadcountByUnit];");
            migrationBuilder.Sql(@"-- ===BATCH===
IF OBJECT_ID('[Hrms].[Report_LeaveBalances]', 'P') IS NOT NULL DROP PROCEDURE [Hrms].[Report_LeaveBalances];");
            migrationBuilder.Sql(@"-- ===BATCH===
IF OBJECT_ID('[Hrms].[Report_LeaveTaken]', 'P') IS NOT NULL DROP PROCEDURE [Hrms].[Report_LeaveTaken];");
            migrationBuilder.Sql(@"-- ===BATCH===
IF OBJECT_ID('[Hrms].[Report_NewHires]', 'P') IS NOT NULL DROP PROCEDURE [Hrms].[Report_NewHires];");
            migrationBuilder.Sql(@"-- ===BATCH===
IF OBJECT_ID('[Hrms].[Report_ProbationTracking]', 'P') IS NOT NULL DROP PROCEDURE [Hrms].[Report_ProbationTracking];");
            migrationBuilder.Sql(@"-- ===BATCH===
IF OBJECT_ID('[Hrms].[Report_RecruitmentPipeline]', 'P') IS NOT NULL DROP PROCEDURE [Hrms].[Report_RecruitmentPipeline];");
            migrationBuilder.Sql(@"-- ===BATCH===
IF OBJECT_ID('[Hrms].[Report_SalaryRegister]', 'P') IS NOT NULL DROP PROCEDURE [Hrms].[Report_SalaryRegister];");
            migrationBuilder.Sql(@"-- ===BATCH===
IF OBJECT_ID('[Hrms].[Report_Terminations]', 'P') IS NOT NULL DROP PROCEDURE [Hrms].[Report_Terminations];");
            migrationBuilder.Sql(@"-- ===BATCH===
IF OBJECT_ID('[Hrms].[Report_TrainingCompletion]', 'P') IS NOT NULL DROP PROCEDURE [Hrms].[Report_TrainingCompletion];");
            migrationBuilder.Sql(@"-- ===BATCH===
IF OBJECT_ID('[Hrms].[Report_VacantPositions]', 'P') IS NOT NULL DROP PROCEDURE [Hrms].[Report_VacantPositions];");

            migrationBuilder.Sql(@"-- ===BATCH===
IF OBJECT_ID('[Core].[hrms_ReportActivate]', 'P') IS NOT NULL DROP PROCEDURE [Core].[hrms_ReportActivate];");
            migrationBuilder.Sql(@"-- ===BATCH===
CREATE   PROCEDURE Core.hrms_ReportActivate
    @ReportId UNIQUEIDENTIFIER,
    @IsActive BIT,
    @TenantId NVARCHAR(450) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.hrmsReport
       SET IsActive = @IsActive,
           UpdatedAt = SYSUTCDATETIME(),
           RowVersion = CONVERT(varbinary(8), NEWID())
     WHERE Id = @ReportId
       AND (@TenantId IS NULL OR @TenantId = '' OR TenantId = @TenantId);
END");

            migrationBuilder.Sql(@"-- ===BATCH===
IF OBJECT_ID('[Core].[hrms_ReportClientSchedule]', 'P') IS NOT NULL DROP PROCEDURE [Core].[hrms_ReportClientSchedule];");
            migrationBuilder.Sql(@"-- ===BATCH===
CREATE   PROCEDURE Core.hrms_ReportClientSchedule
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
       OR NOT EXISTS (SELECT 1 FROM dbo.hrmsReportSchedule WHERE Id = @ReportScheduleId)
    BEGIN
        SET @ReportScheduleId = NEWID();
        INSERT INTO dbo.hrmsReportSchedule
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
        UPDATE dbo.hrmsReportSchedule
           SET Name = @Name, IsScheduled = @IsScheduled, MailSubject = @MailSubject, MailBody = @MailBody,
               IsHideRecipients = @IsHideRecipients, Frequency = @Frequency, FrequencyWeekly = @FrequencyWeekly,
               TimeOfTheDay = @TimeOfTheDay, ScheduleStartDate = @ScheduleStartDate, OutputFormat = @OutputFormat,
               CronExpression = @CronExpression, UpdatedAt = SYSUTCDATETIME(), RowVersion = CONVERT(varbinary(8), NEWID())
         WHERE Id = @ReportScheduleId;
    END
END");

            migrationBuilder.Sql(@"-- ===BATCH===
IF OBJECT_ID('[Core].[hrms_ReportClientScheduleDelete]', 'P') IS NOT NULL DROP PROCEDURE [Core].[hrms_ReportClientScheduleDelete];");
            migrationBuilder.Sql(@"-- ===BATCH===
CREATE   PROCEDURE Core.hrms_ReportClientScheduleDelete
    @ReportScheduleId UNIQUEIDENTIFIER,
    @IsModifyOnly INT = 0
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM dbo.hrmsReportScheduleRecipient  WHERE ReportScheduleId = @ReportScheduleId;
    DELETE FROM dbo.hrmsReportScheduleFieldValue WHERE ReportScheduleId = @ReportScheduleId;
    DELETE FROM dbo.hrmsReportScheduleFieldOutput WHERE ReportScheduleId = @ReportScheduleId;
    IF @IsModifyOnly = 0
        DELETE FROM dbo.hrmsReportSchedule WHERE Id = @ReportScheduleId;
END");

            migrationBuilder.Sql(@"-- ===BATCH===
IF OBJECT_ID('[Core].[hrms_ReportClientScheduleEnable]', 'P') IS NOT NULL DROP PROCEDURE [Core].[hrms_ReportClientScheduleEnable];");
            migrationBuilder.Sql(@"-- ===BATCH===
CREATE   PROCEDURE Core.hrms_ReportClientScheduleEnable
    @ReportScheduleId UNIQUEIDENTIFIER,
    @Enabled INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.hrmsReportSchedule
       SET IsActive = CASE WHEN @Enabled = 1 THEN 1 ELSE 0 END,
           UpdatedAt = SYSUTCDATETIME(), RowVersion = CONVERT(varbinary(8), NEWID())
     WHERE Id = @ReportScheduleId;
END");

            migrationBuilder.Sql(@"-- ===BATCH===
IF OBJECT_ID('[Core].[hrms_ReportClientScheduleFieldOutput]', 'P') IS NOT NULL DROP PROCEDURE [Core].[hrms_ReportClientScheduleFieldOutput];");
            migrationBuilder.Sql(@"-- ===BATCH===
CREATE   PROCEDURE Core.hrms_ReportClientScheduleFieldOutput
    @ReportScheduleId UNIQUEIDENTIFIER,
    @ReportKey NVARCHAR(100),
    @Field NVARCHAR(100),
    @Label NVARCHAR(200),
    @FieldOrder INT = 0,
    @SortOrder INT = 0
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Tenant NVARCHAR(450) = (SELECT TOP 1 TenantId FROM dbo.hrmsReportSchedule WHERE Id = @ReportScheduleId);
    INSERT INTO dbo.hrmsReportScheduleFieldOutput
        (Id, ReportScheduleId, ReportKey, Field, Label, FieldOrder, SortOrder, TenantId, CreatedAt, RowVersion)
    VALUES
        (NEWID(), @ReportScheduleId, @ReportKey, @Field, @Label, @FieldOrder, @SortOrder, @Tenant, SYSUTCDATETIME(), CONVERT(varbinary(8), NEWID()));
END");

            migrationBuilder.Sql(@"-- ===BATCH===
IF OBJECT_ID('[Core].[hrms_ReportClientScheduleFieldValue]', 'P') IS NOT NULL DROP PROCEDURE [Core].[hrms_ReportClientScheduleFieldValue];");
            migrationBuilder.Sql(@"-- ===BATCH===
CREATE   PROCEDURE Core.hrms_ReportClientScheduleFieldValue
    @ReportScheduleId UNIQUEIDENTIFIER,
    @ReportKey NVARCHAR(100),
    @Field NVARCHAR(100),
    @Value NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Tenant NVARCHAR(450) = (SELECT TOP 1 TenantId FROM dbo.hrmsReportSchedule WHERE Id = @ReportScheduleId);
    INSERT INTO dbo.hrmsReportScheduleFieldValue
        (Id, ReportScheduleId, ReportKey, Field, Value, TenantId, CreatedAt, RowVersion)
    VALUES
        (NEWID(), @ReportScheduleId, @ReportKey, @Field, @Value, @Tenant, SYSUTCDATETIME(), CONVERT(varbinary(8), NEWID()));
END");

            migrationBuilder.Sql(@"-- ===BATCH===
IF OBJECT_ID('[Core].[hrms_ReportClientScheduleRead]', 'P') IS NOT NULL DROP PROCEDURE [Core].[hrms_ReportClientScheduleRead];");
            migrationBuilder.Sql(@"-- ===BATCH===
CREATE   PROCEDURE Core.hrms_ReportClientScheduleRead
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
          FROM dbo.hrmsReportSchedule s
          JOIN dbo.hrmsReport r ON r.Id = s.ReportId
         WHERE s.Id = @Id
           AND (@TenantId IS NULL OR @TenantId = '' OR s.TenantId = @TenantId);
    ELSE
        SELECT s.Id AS ReportScheduleId, s.ReportId, r.ReportKey, r.ReportName, s.Name,
               s.IsScheduled, s.IsActive, s.MailSubject, s.MailBody, s.IsHideRecipients,
               s.Frequency, s.FrequencyWeekly, s.TimeOfTheDay, s.ScheduleStartDate,
               s.OutputFormat, s.CronExpression, r.StoredProc
          FROM dbo.hrmsReportSchedule s
          JOIN dbo.hrmsReport r ON r.Id = s.ReportId
         WHERE s.ReportId = @Id
           AND (@TenantId IS NULL OR @TenantId = '' OR s.TenantId = @TenantId)
         ORDER BY s.Name;
END");

            migrationBuilder.Sql(@"-- ===BATCH===
IF OBJECT_ID('[Core].[hrms_ReportClientScheduleRecipient]', 'P') IS NOT NULL DROP PROCEDURE [Core].[hrms_ReportClientScheduleRecipient];");
            migrationBuilder.Sql(@"-- ===BATCH===
CREATE   PROCEDURE Core.hrms_ReportClientScheduleRecipient
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
            COALESCE(NULLIF(@TenantId, ''), (SELECT TOP 1 TenantId FROM dbo.hrmsReportSchedule WHERE Id = @ReportScheduleId));
        DECLARE @ResolvedEmail NVARCHAR(300) = @Email;
        IF @ResolvedEmail IS NULL AND @UserId IS NOT NULL
            SET @ResolvedEmail = (SELECT TOP 1 Email FROM Core.[User] WHERE Id = @UserId);
        INSERT INTO dbo.hrmsReportScheduleRecipient
            (Id, ReportScheduleId, UserId, RoleId, Email, TenantId, CreatedAt, RowVersion)
        VALUES
            (NEWID(), @ReportScheduleId, @UserId, @RoleId, @ResolvedEmail, @Tenant, SYSUTCDATETIME(), CONVERT(varbinary(8), NEWID()));
    END
    ELSE IF @Type = 'ListUsers'
    BEGIN
        SELECT @ReportScheduleId AS ReportScheduleId, u.Id AS UserId, u.UserName AS UserName,
               CAST(CASE WHEN r.Id IS NOT NULL THEN 1 ELSE 0 END AS BIT) AS IsAssigned, u.Email AS Email
        FROM Core.[User] u
        LEFT JOIN dbo.hrmsReportScheduleRecipient r
            ON r.ReportScheduleId = @ReportScheduleId AND r.UserId = u.Id
        WHERE (@TenantId IS NULL OR @TenantId = '' OR u.TenantId = @TenantId)
        ORDER BY u.UserName;
    END
    ELSE IF @Type = 'ListRoles'
    BEGIN
        SELECT @ReportScheduleId AS ReportScheduleId, ro.Id AS RoleId, ro.Name AS RoleName,
               CAST(CASE WHEN r.Id IS NOT NULL THEN 1 ELSE 0 END AS BIT) AS IsAssigned
        FROM Core.Role ro
        LEFT JOIN dbo.hrmsReportScheduleRecipient r
            ON r.ReportScheduleId = @ReportScheduleId AND r.RoleId = ro.Id
        WHERE (@TenantId IS NULL OR @TenantId = '' OR ro.TenantId = @TenantId)
        ORDER BY ro.Name;
    END
END");

            migrationBuilder.Sql(@"-- ===BATCH===
IF OBJECT_ID('[Core].[hrms_ReportDelete]', 'P') IS NOT NULL DROP PROCEDURE [Core].[hrms_ReportDelete];");
            migrationBuilder.Sql(@"-- ===BATCH===
CREATE   PROCEDURE Core.hrms_ReportDelete
    @ReportId UNIQUEIDENTIFIER,
    @TenantId NVARCHAR(450) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM dbo.hrmsReport
     WHERE Id = @ReportId
       AND (@TenantId IS NULL OR @TenantId = '' OR TenantId = @TenantId);
END");

            migrationBuilder.Sql(@"-- ===BATCH===
IF OBJECT_ID('[Core].[hrms_ReportFieldOutputRead]', 'P') IS NOT NULL DROP PROCEDURE [Core].[hrms_ReportFieldOutputRead];");
            migrationBuilder.Sql(@"-- ===BATCH===
CREATE   PROCEDURE Core.hrms_ReportFieldOutputRead
    @ReportKey NVARCHAR(100),
    @ReportScheduleId UNIQUEIDENTIFIER = NULL,
    @TenantId NVARCHAR(450) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @ReportId UNIQUEIDENTIFIER =
        (SELECT TOP 1 Id FROM dbo.hrmsReport
          WHERE ReportKey = @ReportKey
            AND (@TenantId IS NULL OR @TenantId = '' OR TenantId = @TenantId));

    SELECT
        CASE WHEN @ReportScheduleId IS NULL THEN 1
             WHEN so.Id IS NOT NULL THEN 1 ELSE 0 END AS IsShow,
        fo.Field AS Field,
        COALESCE(so.Label, fo.Label) AS Label,
        COALESCE(so.SortOrder, 0) AS SortOrder,
        COALESCE(so.FieldOrder, fo.FieldOrder) AS FieldOrder
    FROM dbo.hrmsReportFieldOutput fo
    LEFT JOIN dbo.hrmsReportScheduleFieldOutput so
        ON so.ReportScheduleId = @ReportScheduleId AND so.Field = fo.Field
    WHERE fo.ReportId = @ReportId
    ORDER BY COALESCE(so.FieldOrder, fo.FieldOrder), fo.Label;
END");

            migrationBuilder.Sql(@"-- ===BATCH===
IF OBJECT_ID('[Core].[hrms_ReportFieldValues]', 'P') IS NOT NULL DROP PROCEDURE [Core].[hrms_ReportFieldValues];");
            migrationBuilder.Sql(@"-- ===BATCH===
CREATE   PROCEDURE Core.hrms_ReportFieldValues
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
        FROM dbo.hrmsOrganizationUnit
        WHERE TenantId = @TenantId AND IsActive = 1
          AND (@BranchId IS NULL OR BranchId = @BranchId OR BranchId IS NULL)
          AND (@Search IS NULL OR Name LIKE '%' + @Search + '%')
        ORDER BY Name;
    ELSE IF @Field = 'EmploymentStatus'
        SELECT v.Value, v.Value AS Label
        FROM (VALUES ('Active'),('Probation'),('OnLeave'),('Suspended'),('Terminated'),('Retired')) v(Value);
    ELSE IF @Field = 'LeaveTypeId'
        SELECT CAST(Id AS NVARCHAR(50)) AS Value, Name AS Label
        FROM dbo.hrmsLeaveType
        WHERE TenantId = @TenantId AND IsActive = 1
          AND (@Search IS NULL OR Name LIKE '%' + @Search + '%')
        ORDER BY Name;
    ELSE IF @Field = 'BranchId'
        SELECT CAST(Id AS NVARCHAR(50)) AS Value, Name AS Label
        FROM dbo.hrmsBranch
        WHERE TenantId = @TenantId AND IsActive = 1
          AND (@Search IS NULL OR Name LIKE '%' + @Search + '%')
        ORDER BY Name;
    ELSE IF @Field = 'PositionClassId'
        SELECT CAST(Id AS NVARCHAR(50)) AS Value, Title AS Label
        FROM dbo.hrmsPositionClass
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
        FROM dbo.hrmsJobGrade
        WHERE TenantId = @TenantId
          AND (@Search IS NULL OR Name LIKE '%' + @Search + '%')
        ORDER BY Name;
    ELSE IF @Field = 'LeaveStatus'
        SELECT v.Value, v.Value AS Label
        FROM (VALUES ('Draft'),('Pending'),('Approved'),('Rejected'),('Cancelled')) v(Value);
    ELSE IF @Field = 'TrainingCourseId'
        SELECT CAST(Id AS NVARCHAR(50)) AS Value, Name AS Label
        FROM dbo.hrmsTrainingCourse
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
END");

            migrationBuilder.Sql(@"-- ===BATCH===
IF OBJECT_ID('[Core].[hrms_ReportGenerateGetScheduleInfo]', 'P') IS NOT NULL DROP PROCEDURE [Core].[hrms_ReportGenerateGetScheduleInfo];");
            migrationBuilder.Sql(@"-- ===BATCH===
CREATE   PROCEDURE Core.hrms_ReportGenerateGetScheduleInfo
    @TenantId NVARCHAR(450) = NULL,
    @ReportScheduleId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT s.Id AS ReportScheduleId, s.ReportId, r.ReportKey, r.ReportName, s.Name,
           s.IsScheduled, s.IsActive, s.MailSubject, s.MailBody, s.IsHideRecipients,
           s.Frequency, s.FrequencyWeekly, s.TimeOfTheDay, s.ScheduleStartDate,
           s.OutputFormat, s.CronExpression, r.StoredProc
      FROM dbo.hrmsReportSchedule s
      JOIN dbo.hrmsReport r ON r.Id = s.ReportId
     WHERE s.Id = @ReportScheduleId
       AND (@TenantId IS NULL OR @TenantId = '' OR s.TenantId = @TenantId);

    SELECT Field, Value FROM dbo.hrmsReportScheduleFieldValue WHERE ReportScheduleId = @ReportScheduleId;

    SELECT DISTINCT e.Email FROM (
        SELECT rec.Email AS Email
          FROM dbo.hrmsReportScheduleRecipient rec
         WHERE rec.ReportScheduleId = @ReportScheduleId AND rec.Email IS NOT NULL AND rec.Email <> ''
        UNION
        SELECT u.Email
          FROM dbo.hrmsReportScheduleRecipient rec
          JOIN Core.[User] u ON u.Id = rec.UserId
         WHERE rec.ReportScheduleId = @ReportScheduleId
        UNION
        SELECT u.Email
          FROM dbo.hrmsReportScheduleRecipient rec
          JOIN Core.UserRole ur ON ur.RoleId = rec.RoleId
          JOIN Core.[User] u ON u.Id = ur.UserId
         WHERE rec.ReportScheduleId = @ReportScheduleId
    ) e
    WHERE e.Email IS NOT NULL AND e.Email <> '';

    SELECT 1 AS IsShow, Field, Label, SortOrder, FieldOrder
      FROM dbo.hrmsReportScheduleFieldOutput
     WHERE ReportScheduleId = @ReportScheduleId
     ORDER BY FieldOrder;
END");

            migrationBuilder.Sql(@"-- ===BATCH===
IF OBJECT_ID('[Core].[hrms_ReportGenerateSendToHistory]', 'P') IS NOT NULL DROP PROCEDURE [Core].[hrms_ReportGenerateSendToHistory];");
            migrationBuilder.Sql(@"-- ===BATCH===
CREATE   PROCEDURE Core.hrms_ReportGenerateSendToHistory
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
    INSERT INTO dbo.hrmsReportRun
        (Id, TenantId, ReportKey, CriteriaJson, [RowCount], DurationMs, RanBy, IsScheduled, FieldOutput, CreatedAt, RowVersion)
    VALUES
        (@RunId, @TenantId, @ReportKey, ISNULL(@Criteria, '{}'), @TotalRecords, @RunSeconds * 1000, @RanBy,
         @IsScheduled, @FieldOutput, SYSUTCDATETIME(), CONVERT(varbinary(8), NEWID()));

    IF @Recipients IS NOT NULL AND @Recipients <> ''
        INSERT INTO dbo.hrmsReportRunRecipient (Id, ReportRunId, UserId, Email, TenantId, CreatedAt, RowVersion)
        SELECT NEWID(), @RunId, NULL, LTRIM(RTRIM(value)), @TenantId, SYSUTCDATETIME(), CONVERT(varbinary(8), NEWID())
          FROM STRING_SPLIT(@Recipients, ';')
         WHERE LTRIM(RTRIM(value)) <> '';
END");

            migrationBuilder.Sql(@"-- ===BATCH===
IF OBJECT_ID('[Core].[hrms_Report_DisciplinaryCases]', 'P') IS NOT NULL DROP PROCEDURE [Core].[hrms_Report_DisciplinaryCases];");
            migrationBuilder.Sql(@"-- ===BATCH===
CREATE   PROCEDURE Core.hrms_Report_DisciplinaryCases
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
        FROM dbo.hrmsDisciplinaryMeasure d
        INNER JOIN dbo.hrmsEmployee e         ON e.Id   = d.EmployeeId
        LEFT JOIN Core.CorePerson p           ON p.Id   = e.PersonId
        LEFT JOIN dbo.hrmsPosition pos        ON pos.Id = e.PositionId
        LEFT JOIN dbo.hrmsOrganizationUnit ou ON ou.Id  = pos.OrganizationUnitId
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
END");

            migrationBuilder.Sql(@"-- ===BATCH===
IF OBJECT_ID('[Core].[hrms_Report_EmployeeDemographics]', 'P') IS NOT NULL DROP PROCEDURE [Core].[hrms_Report_EmployeeDemographics];");
            migrationBuilder.Sql(@"-- ===BATCH===
CREATE   PROCEDURE Core.hrms_Report_EmployeeDemographics
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

    DECLARE @gbJson NVARCHAR(MAX) = '[""' + REPLACE(REPLACE(@groupBy, ' ', ''), ',', '"",""') + '""]';
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
        FROM dbo.hrmsEmployee e
        LEFT JOIN Core.CorePerson p           ON p.Id   = e.PersonId
        LEFT JOIN dbo.hrmsPosition pos        ON pos.Id = e.PositionId
        LEFT JOIN dbo.hrmsOrganizationUnit ou ON ou.Id  = pos.OrganizationUnitId
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
END");

            migrationBuilder.Sql(@"-- ===BATCH===
IF OBJECT_ID('[Core].[hrms_Report_EmployeeDirectory]', 'P') IS NOT NULL DROP PROCEDURE [Core].[hrms_Report_EmployeeDirectory];");
            migrationBuilder.Sql(@"-- ===BATCH===
CREATE   PROCEDURE [Core].[hrms_Report_EmployeeDirectory]
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
        FROM dbo.hrmsEmployee e
                    left JOIN          Core.CorePerson p             ON p.Id  = e.PersonId
                    left JOIN          dbo.hrmsPosition pos        ON pos.Id = e.PositionId
                    left join          dbo.hrmsPositionClass poc   on poc.Id = pos.[PositionClassId]
                    left JOIN dbo.hrmsOrganizationUnit ou ON ou.Id = pos.OrganizationUnitId
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
END");

            migrationBuilder.Sql(@"-- ===BATCH===
IF OBJECT_ID('[Core].[hrms_Report_EmployeeDirectoryGrouped]', 'P') IS NOT NULL DROP PROCEDURE [Core].[hrms_Report_EmployeeDirectoryGrouped];");
            migrationBuilder.Sql(@"-- ===BATCH===
CREATE   PROCEDURE Core.hrms_Report_EmployeeDirectoryGrouped
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
    DECLARE @gbJson NVARCHAR(MAX) = '[""' + REPLACE(REPLACE(@groupBy, ' ', ''), ',', '"",""') + '""]';
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
        FROM dbo.hrmsEmployee e
        LEFT JOIN Core.CorePerson p             ON p.Id  = e.PersonId
        LEFT JOIN dbo.hrmsPosition pos        ON pos.Id = e.PositionId
        LEFT JOIN dbo.hrmsOrganizationUnit ou ON ou.Id = pos.OrganizationUnitId
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
END");

            migrationBuilder.Sql(@"-- ===BATCH===
IF OBJECT_ID('[Core].[hrms_Report_EmployeeMovements]', 'P') IS NOT NULL DROP PROCEDURE [Core].[hrms_Report_EmployeeMovements];");
            migrationBuilder.Sql(@"-- ===BATCH===
CREATE   PROCEDURE Core.hrms_Report_EmployeeMovements
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
        FROM dbo.hrmsEmployeeMovement m
        INNER JOIN dbo.hrmsEmployee e  ON e.Id  = m.EmployeeId
        LEFT JOIN Core.CorePerson p    ON p.Id  = e.PersonId
        LEFT JOIN dbo.hrmsPosition fp  ON fp.Id = m.FromPositionId
        LEFT JOIN dbo.hrmsPosition tp  ON tp.Id = m.ToPositionId
        LEFT JOIN dbo.hrmsBranch fb    ON fb.Id = m.FromBranchId
        LEFT JOIN dbo.hrmsBranch tb    ON tb.Id = m.ToBranchId
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
END");

            migrationBuilder.Sql(@"-- ===BATCH===
IF OBJECT_ID('[Core].[hrms_Report_HeadcountByUnit]', 'P') IS NOT NULL DROP PROCEDURE [Core].[hrms_Report_HeadcountByUnit];");
            migrationBuilder.Sql(@"-- ===BATCH===
CREATE   PROCEDURE Core.hrms_Report_HeadcountByUnit
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

    DECLARE @gbJson NVARCHAR(MAX) = '[""' + REPLACE(REPLACE(@groupBy, ' ', ''), ',', '"",""') + '""]';
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
        FROM dbo.hrmsEmployee e
        LEFT JOIN Core.CorePerson p           ON p.Id   = e.PersonId
        LEFT JOIN dbo.hrmsPosition pos        ON pos.Id = e.PositionId
        LEFT JOIN dbo.hrmsPositionClass poc   ON poc.Id = pos.PositionClassId
        LEFT JOIN dbo.hrmsOrganizationUnit ou ON ou.Id  = pos.OrganizationUnitId
        LEFT JOIN dbo.hrmsBranch b            ON b.Id   = e.BranchId
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
END");

            migrationBuilder.Sql(@"-- ===BATCH===
IF OBJECT_ID('[Core].[hrms_Report_LeaveBalances]', 'P') IS NOT NULL DROP PROCEDURE [Core].[hrms_Report_LeaveBalances];");
            migrationBuilder.Sql(@"-- ===BATCH===
CREATE   PROCEDURE Core.hrms_Report_LeaveBalances
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
        FROM dbo.hrmsLeaveBalance lb
        INNER JOIN dbo.hrmsEmployee e         ON e.Id   = lb.EmployeeId
        LEFT JOIN Core.CorePerson p           ON p.Id   = e.PersonId
        LEFT JOIN dbo.hrmsPosition pos        ON pos.Id = e.PositionId
        LEFT JOIN dbo.hrmsOrganizationUnit ou ON ou.Id  = pos.OrganizationUnitId
        LEFT JOIN dbo.hrmsLeaveType lt        ON lt.Id  = lb.LeaveTypeId
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
END");

            migrationBuilder.Sql(@"-- ===BATCH===
IF OBJECT_ID('[Core].[hrms_Report_LeaveTaken]', 'P') IS NOT NULL DROP PROCEDURE [Core].[hrms_Report_LeaveTaken];");
            migrationBuilder.Sql(@"-- ===BATCH===
CREATE   PROCEDURE Core.hrms_Report_LeaveTaken
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
        FROM dbo.hrmsLeaveRequestLine ll
        INNER JOIN dbo.hrmsLeaveRequest lr    ON lr.Id  = ll.LeaveRequestId
        INNER JOIN dbo.hrmsEmployee e         ON e.Id   = lr.EmployeeId
        LEFT JOIN Core.CorePerson p           ON p.Id   = e.PersonId
        LEFT JOIN dbo.hrmsPosition pos        ON pos.Id = e.PositionId
        LEFT JOIN dbo.hrmsOrganizationUnit ou ON ou.Id  = pos.OrganizationUnitId
        LEFT JOIN dbo.hrmsLeaveType lt        ON lt.Id  = ll.LeaveTypeId
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
END");

            migrationBuilder.Sql(@"-- ===BATCH===
IF OBJECT_ID('[Core].[hrms_Report_NewHires]', 'P') IS NOT NULL DROP PROCEDURE [Core].[hrms_Report_NewHires];");
            migrationBuilder.Sql(@"-- ===BATCH===
CREATE   PROCEDURE Core.hrms_Report_NewHires
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
        FROM dbo.hrmsEmployee e
        LEFT JOIN Core.CorePerson p           ON p.Id   = e.PersonId
        LEFT JOIN dbo.hrmsPosition pos        ON pos.Id = e.PositionId
        LEFT JOIN dbo.hrmsPositionClass poc   ON poc.Id = pos.PositionClassId
        LEFT JOIN dbo.hrmsOrganizationUnit ou ON ou.Id  = pos.OrganizationUnitId
        LEFT JOIN dbo.hrmsBranch b            ON b.Id   = e.BranchId
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
END");

            migrationBuilder.Sql(@"-- ===BATCH===
IF OBJECT_ID('[Core].[hrms_Report_ProbationTracking]', 'P') IS NOT NULL DROP PROCEDURE [Core].[hrms_Report_ProbationTracking];");
            migrationBuilder.Sql(@"-- ===BATCH===
CREATE   PROCEDURE Core.hrms_Report_ProbationTracking
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
        FROM dbo.hrmsEmployee e
        LEFT JOIN Core.CorePerson p           ON p.Id   = e.PersonId
        LEFT JOIN dbo.hrmsPosition pos        ON pos.Id = e.PositionId
        LEFT JOIN dbo.hrmsPositionClass poc   ON poc.Id = pos.PositionClassId
        LEFT JOIN dbo.hrmsOrganizationUnit ou ON ou.Id  = pos.OrganizationUnitId
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
END");

            migrationBuilder.Sql(@"-- ===BATCH===
IF OBJECT_ID('[Core].[hrms_Report_RecruitmentPipeline]', 'P') IS NOT NULL DROP PROCEDURE [Core].[hrms_Report_RecruitmentPipeline];");
            migrationBuilder.Sql(@"-- ===BATCH===
CREATE   PROCEDURE Core.hrms_Report_RecruitmentPipeline
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

    DECLARE @gbJson NVARCHAR(MAX) = '[""' + REPLACE(REPLACE(@groupBy, ' ', ''), ',', '"",""') + '""]';
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
        FROM dbo.hrmsJobApplication a
        INNER JOIN dbo.hrmsCandidate ca       ON ca.Id  = a.CandidateId
        INNER JOIN dbo.hrmsJobRequisition r   ON r.Id   = a.RequisitionId
        LEFT JOIN dbo.hrmsOrganizationUnit ou ON ou.Id  = r.OrganizationUnitId
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
END");

            migrationBuilder.Sql(@"-- ===BATCH===
IF OBJECT_ID('[Core].[hrms_Report_SalaryRegister]', 'P') IS NOT NULL DROP PROCEDURE [Core].[hrms_Report_SalaryRegister];");
            migrationBuilder.Sql(@"-- ===BATCH===
CREATE   PROCEDURE Core.hrms_Report_SalaryRegister
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

    DECLARE @gbJson NVARCHAR(MAX) = '[""' + REPLACE(REPLACE(@groupBy, ' ', ''), ',', '"",""') + '""]';
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
        FROM dbo.hrmsEmployee e
        LEFT JOIN Core.CorePerson p           ON p.Id   = e.PersonId
        LEFT JOIN dbo.hrmsPosition pos        ON pos.Id = e.PositionId
        LEFT JOIN dbo.hrmsPositionClass poc   ON poc.Id = pos.PositionClassId
        LEFT JOIN dbo.hrmsOrganizationUnit ou ON ou.Id  = pos.OrganizationUnitId
        LEFT JOIN dbo.hrmsBranch b            ON b.Id   = e.BranchId
        LEFT JOIN Core.coreSalaryScale ss     ON ss.Id  = e.SalaryScaleId
        LEFT JOIN dbo.hrmsJobGrade jg         ON jg.Id  = ss.JobGradeId
        LEFT JOIN Core.lupStep st             ON st.Id  = ss.StepId
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
END");

            migrationBuilder.Sql(@"-- ===BATCH===
IF OBJECT_ID('[Core].[hrms_Report_Terminations]', 'P') IS NOT NULL DROP PROCEDURE [Core].[hrms_Report_Terminations];");
            migrationBuilder.Sql(@"-- ===BATCH===
CREATE   PROCEDURE Core.hrms_Report_Terminations
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

    DECLARE @gbJson NVARCHAR(MAX) = '[""' + REPLACE(REPLACE(@groupBy, ' ', ''), ',', '"",""') + '""]';
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
        FROM dbo.hrmsEmployeeTermination t
        INNER JOIN dbo.hrmsEmployee e         ON e.Id   = t.EmployeeId
        LEFT JOIN Core.CorePerson p           ON p.Id   = e.PersonId
        LEFT JOIN dbo.hrmsPosition pos        ON pos.Id = e.PositionId
        LEFT JOIN dbo.hrmsPositionClass poc   ON poc.Id = pos.PositionClassId
        LEFT JOIN dbo.hrmsOrganizationUnit ou ON ou.Id  = pos.OrganizationUnitId
        LEFT JOIN dbo.hrmsBranch b            ON b.Id   = e.BranchId
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
END");

            migrationBuilder.Sql(@"-- ===BATCH===
IF OBJECT_ID('[Core].[hrms_Report_TrainingCompletion]', 'P') IS NOT NULL DROP PROCEDURE [Core].[hrms_Report_TrainingCompletion];");
            migrationBuilder.Sql(@"-- ===BATCH===
CREATE   PROCEDURE Core.hrms_Report_TrainingCompletion
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
        FROM dbo.hrmsTrainingEnrollment te
        INNER JOIN dbo.hrmsTrainingSession ts ON ts.Id  = te.TrainingSessionId
        INNER JOIN dbo.hrmsTrainingCourse tc  ON tc.Id  = ts.TrainingCourseId
        INNER JOIN dbo.hrmsEmployee e         ON e.Id   = te.EmployeeId
        LEFT JOIN Core.CorePerson p           ON p.Id   = e.PersonId
        LEFT JOIN dbo.hrmsPosition pos        ON pos.Id = e.PositionId
        LEFT JOIN dbo.hrmsOrganizationUnit ou ON ou.Id  = pos.OrganizationUnitId
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
END");

            migrationBuilder.Sql(@"-- ===BATCH===
IF OBJECT_ID('[Core].[hrms_Report_VacantPositions]', 'P') IS NOT NULL DROP PROCEDURE [Core].[hrms_Report_VacantPositions];");
            migrationBuilder.Sql(@"-- ===BATCH===
CREATE   PROCEDURE Core.hrms_Report_VacantPositions
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
        FROM dbo.hrmsPosition pos
        LEFT JOIN dbo.hrmsPositionClass poc   ON poc.Id = pos.PositionClassId
        LEFT JOIN dbo.hrmsOrganizationUnit ou ON ou.Id  = pos.OrganizationUnitId
        LEFT JOIN dbo.hrmsBranch b            ON b.Id   = pos.BranchId
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
END");

            migrationBuilder.Sql(@"-- ===BATCH===
UPDATE [dbo].[hrmsReport]
SET StoredProc = REPLACE(REPLACE(StoredProc, '[Hrms].[', '[Core].[hrms_'), 'Hrms.', 'Core.hrms_')
WHERE StoredProc LIKE '%Hrms%';");
        }
}
}
