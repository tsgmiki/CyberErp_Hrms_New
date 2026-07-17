using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <inheritdoc />
    public partial class RenameHrmsTablesToDbo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_coreSalaryScale_hrms_JobGrade_JobGradeId",
                schema: "Core",
                table: "coreSalaryScale");

            migrationBuilder.DropForeignKey(
                name: "FK_hrms_Achievement_hrms_Appraisal_AppraisalId",
                schema: "Core",
                table: "hrms_Achievement");

            migrationBuilder.DropForeignKey(
                name: "FK_hrms_Achievement_hrms_Employee_EmployeeId",
                schema: "Core",
                table: "hrms_Achievement");

            migrationBuilder.DropForeignKey(
                name: "FK_hrms_AnnualLeaveDetail_hrms_AnnualLeaveHeader_AnnualLeaveHeaderId",
                schema: "Core",
                table: "hrms_AnnualLeaveDetail");

            migrationBuilder.DropForeignKey(
                name: "FK_hrms_AnnualLeaveHeader_hrms_Employee_EmployeeId",
                schema: "Core",
                table: "hrms_AnnualLeaveHeader");

            migrationBuilder.DropForeignKey(
                name: "FK_hrms_AnnualLeaveHeader_hrms_LeaveBalance_AnnualLeaveLedgerId",
                schema: "Core",
                table: "hrms_AnnualLeaveHeader");

            migrationBuilder.DropForeignKey(
                name: "FK_hrms_AnnualLeaveSetting_FiscalYear_FiscalYearId",
                schema: "Core",
                table: "hrms_AnnualLeaveSetting");

            migrationBuilder.DropForeignKey(
                name: "FK_hrms_AnnualLeaveSetting_hrms_LeaveType_LeaveTypeId",
                schema: "Core",
                table: "hrms_AnnualLeaveSetting");

            migrationBuilder.DropForeignKey(
                name: "FK_hrms_ApplicationCriterionScore_hrms_JobApplication_ApplicationId",
                schema: "Core",
                table: "hrms_ApplicationCriterionScore");

            migrationBuilder.DropForeignKey(
                name: "FK_hrms_Appraisal_hrms_Employee_EmployeeId",
                schema: "Core",
                table: "hrms_Appraisal");

            migrationBuilder.DropForeignKey(
                name: "FK_hrms_Appraisal_hrms_ReviewCycle_ReviewCycleId",
                schema: "Core",
                table: "hrms_Appraisal");

            migrationBuilder.DropForeignKey(
                name: "FK_hrms_AppraisalAppeal_hrms_Appraisal_AppraisalId",
                schema: "Core",
                table: "hrms_AppraisalAppeal");

            migrationBuilder.DropForeignKey(
                name: "FK_hrms_AppraisalAppeal_hrms_Employee_EmployeeId",
                schema: "Core",
                table: "hrms_AppraisalAppeal");

            migrationBuilder.DropForeignKey(
                name: "FK_hrms_AppraisalCompetency_hrms_Appraisal_AppraisalId",
                schema: "Core",
                table: "hrms_AppraisalCompetency");

            migrationBuilder.DropForeignKey(
                name: "FK_hrms_AppraisalGoal_hrms_Appraisal_AppraisalId",
                schema: "Core",
                table: "hrms_AppraisalGoal");

            migrationBuilder.DropForeignKey(
                name: "FK_hrms_AppraisalPeerReview_hrms_Appraisal_AppraisalId",
                schema: "Core",
                table: "hrms_AppraisalPeerReview");

            migrationBuilder.DropForeignKey(
                name: "FK_hrms_AppraisalPeerReview_hrms_Employee_PeerEmployeeId",
                schema: "Core",
                table: "hrms_AppraisalPeerReview");

            migrationBuilder.DropForeignKey(
                name: "FK_hrms_Branch_hrms_Branch_ParentId",
                schema: "Core",
                table: "hrms_Branch");

            migrationBuilder.DropForeignKey(
                name: "FK_hrms_CalibrationItem_hrms_Appraisal_AppraisalId",
                schema: "Core",
                table: "hrms_CalibrationItem");

            migrationBuilder.DropForeignKey(
                name: "FK_hrms_CalibrationItem_hrms_CalibrationSession_CalibrationSessionId",
                schema: "Core",
                table: "hrms_CalibrationItem");

            migrationBuilder.DropForeignKey(
                name: "FK_hrms_CalibrationSession_hrms_OrganizationUnit_OrganizationUnitId",
                schema: "Core",
                table: "hrms_CalibrationSession");

            migrationBuilder.DropForeignKey(
                name: "FK_hrms_CalibrationSession_hrms_ReviewCycle_ReviewCycleId",
                schema: "Core",
                table: "hrms_CalibrationSession");

            migrationBuilder.DropForeignKey(
                name: "FK_hrms_Candidate_CorePerson_PersonId",
                schema: "Core",
                table: "hrms_Candidate");

            migrationBuilder.DropForeignKey(
                name: "FK_hrms_Candidate_hrms_Employee_InternalEmployeeId",
                schema: "Core",
                table: "hrms_Candidate");

            migrationBuilder.DropForeignKey(
                name: "FK_hrms_CandidateDocument_hrms_Candidate_CandidateId",
                schema: "Core",
                table: "hrms_CandidateDocument");

            migrationBuilder.DropForeignKey(
                name: "FK_hrms_ClearanceDepartmentApprover_hrms_ClearanceDepartment_DepartmentId",
                schema: "Core",
                table: "hrms_ClearanceDepartmentApprover");

            migrationBuilder.DropForeignKey(
                name: "FK_hrms_Competency_hrms_CompetencyCategory_CompetencyCategoryId",
                schema: "Core",
                table: "hrms_Competency");

            migrationBuilder.DropForeignKey(
                name: "FK_hrms_CriterionEvaluator_hrms_Employee_EmployeeId",
                schema: "Core",
                table: "hrms_CriterionEvaluator");

            migrationBuilder.DropForeignKey(
                name: "FK_hrms_CriterionEvaluator_hrms_RequisitionScreeningCriterion_CriterionId",
                schema: "Core",
                table: "hrms_CriterionEvaluator");

            migrationBuilder.DropForeignKey(
                name: "FK_hrms_DevelopmentAction_hrms_Competency_CompetencyId",
                schema: "Core",
                table: "hrms_DevelopmentAction");

            migrationBuilder.DropForeignKey(
                name: "FK_hrms_DevelopmentAction_hrms_DevelopmentPlan_DevelopmentPlanId",
                schema: "Core",
                table: "hrms_DevelopmentAction");

            migrationBuilder.DropForeignKey(
                name: "FK_hrms_DevelopmentPlan_hrms_Appraisal_AppraisalId",
                schema: "Core",
                table: "hrms_DevelopmentPlan");

            migrationBuilder.DropForeignKey(
                name: "FK_hrms_DevelopmentPlan_hrms_Employee_EmployeeId",
                schema: "Core",
                table: "hrms_DevelopmentPlan");

            migrationBuilder.DropForeignKey(
                name: "FK_hrms_DisciplinaryMeasure_hrms_Employee_EmployeeId",
                schema: "Core",
                table: "hrms_DisciplinaryMeasure");

            migrationBuilder.DropForeignKey(
                name: "FK_hrms_DynamicFormField_hrms_DynamicForm_DynamicFormId",
                schema: "Core",
                table: "hrms_DynamicFormField");

            migrationBuilder.DropForeignKey(
                name: "FK_hrms_DynamicFormRecord_hrms_DynamicForm_DynamicFormId",
                schema: "Core",
                table: "hrms_DynamicFormRecord");

            migrationBuilder.DropForeignKey(
                name: "FK_hrms_Employee_CorePerson_PersonId",
                schema: "Core",
                table: "hrms_Employee");

            migrationBuilder.DropForeignKey(
                name: "FK_hrms_Employee_coreSalaryScale_SalaryScaleId",
                schema: "Core",
                table: "hrms_Employee");

            migrationBuilder.DropForeignKey(
                name: "FK_hrms_Employee_hrms_Branch_BranchId",
                schema: "Core",
                table: "hrms_Employee");

            migrationBuilder.DropForeignKey(
                name: "FK_hrms_Employee_hrms_Position_PositionId",
                schema: "Core",
                table: "hrms_Employee");

            migrationBuilder.DropForeignKey(
                name: "FK_hrms_EmployeeDependent_CorePerson_PersonId",
                schema: "Core",
                table: "hrms_EmployeeDependent");

            migrationBuilder.DropForeignKey(
                name: "FK_hrms_EmployeeDependent_hrms_Employee_RelatedEmployeeId",
                schema: "Core",
                table: "hrms_EmployeeDependent");

            migrationBuilder.DropForeignKey(
                name: "FK_hrms_EmployeeEducation_CorePerson_PersonId",
                schema: "Core",
                table: "hrms_EmployeeEducation");

            migrationBuilder.DropForeignKey(
                name: "FK_hrms_EmployeeExperience_CorePerson_PersonId",
                schema: "Core",
                table: "hrms_EmployeeExperience");

            migrationBuilder.DropForeignKey(
                name: "FK_hrms_EmployeeFieldValue_hrms_EmployeeFieldDefinition_FieldDefinitionId",
                schema: "Core",
                table: "hrms_EmployeeFieldValue");

            migrationBuilder.DropForeignKey(
                name: "FK_hrms_EmployeeGoal_hrms_Employee_EmployeeId",
                schema: "Core",
                table: "hrms_EmployeeGoal");

            migrationBuilder.DropForeignKey(
                name: "FK_hrms_EmployeeGoal_hrms_OrganizationalObjective_OrganizationalObjectiveId",
                schema: "Core",
                table: "hrms_EmployeeGoal");

            migrationBuilder.DropForeignKey(
                name: "FK_hrms_EmployeeGoal_hrms_ReviewCycle_ReviewCycleId",
                schema: "Core",
                table: "hrms_EmployeeGoal");

            migrationBuilder.DropForeignKey(
                name: "FK_hrms_EmployeeMovement_coreSalaryScale_ToSalaryScaleId",
                schema: "Core",
                table: "hrms_EmployeeMovement");

            migrationBuilder.DropForeignKey(
                name: "FK_hrms_EmployeeMovement_hrms_Employee_EmployeeId",
                schema: "Core",
                table: "hrms_EmployeeMovement");

            migrationBuilder.DropForeignKey(
                name: "FK_hrms_EmployeeRecognition_hrms_Employee_EmployeeId",
                schema: "Core",
                table: "hrms_EmployeeRecognition");

            migrationBuilder.DropForeignKey(
                name: "FK_hrms_EmployeeRecognition_hrms_RecognitionBadge_RecognitionBadgeId",
                schema: "Core",
                table: "hrms_EmployeeRecognition");

            migrationBuilder.DropForeignKey(
                name: "FK_hrms_EmployeeTermination_hrms_Employee_EmployeeId",
                schema: "Core",
                table: "hrms_EmployeeTermination");

            migrationBuilder.DropForeignKey(
                name: "FK_hrms_GoalActionItem_hrms_EmployeeGoal_EmployeeGoalId",
                schema: "Core",
                table: "hrms_GoalActionItem");

            migrationBuilder.DropForeignKey(
                name: "FK_hrms_HiringRequest_hrms_OrganizationUnit_OrganizationUnitId",
                schema: "Core",
                table: "hrms_HiringRequest");

            migrationBuilder.DropForeignKey(
                name: "FK_hrms_HiringRequest_hrms_PositionClass_PositionClassId",
                schema: "Core",
                table: "hrms_HiringRequest");

            migrationBuilder.DropForeignKey(
                name: "FK_hrms_ImprovementPlan_hrms_Appraisal_AppraisalId",
                schema: "Core",
                table: "hrms_ImprovementPlan");

            migrationBuilder.DropForeignKey(
                name: "FK_hrms_ImprovementPlan_hrms_Employee_EmployeeId",
                schema: "Core",
                table: "hrms_ImprovementPlan");

            migrationBuilder.DropForeignKey(
                name: "FK_hrms_Interview_hrms_JobApplication_ApplicationId",
                schema: "Core",
                table: "hrms_Interview");

            migrationBuilder.DropForeignKey(
                name: "FK_hrms_InterviewFeedback_hrms_InterviewPanelist_PanelistId",
                schema: "Core",
                table: "hrms_InterviewFeedback");

            migrationBuilder.DropForeignKey(
                name: "FK_hrms_InterviewPanelist_hrms_Employee_EmployeeId",
                schema: "Core",
                table: "hrms_InterviewPanelist");

            migrationBuilder.DropForeignKey(
                name: "FK_hrms_InterviewPanelist_hrms_Interview_InterviewId",
                schema: "Core",
                table: "hrms_InterviewPanelist");

            migrationBuilder.DropForeignKey(
                name: "FK_hrms_JobApplication_hrms_Candidate_CandidateId",
                schema: "Core",
                table: "hrms_JobApplication");

            migrationBuilder.DropForeignKey(
                name: "FK_hrms_JobApplication_hrms_JobRequisition_RequisitionId",
                schema: "Core",
                table: "hrms_JobApplication");

            migrationBuilder.DropForeignKey(
                name: "FK_hrms_JobApplicationStageLog_hrms_JobApplication_ApplicationId",
                schema: "Core",
                table: "hrms_JobApplicationStageLog");

            migrationBuilder.DropForeignKey(
                name: "FK_hrms_JobOffer_coreSalaryScale_SalaryScaleId",
                schema: "Core",
                table: "hrms_JobOffer");

            migrationBuilder.DropForeignKey(
                name: "FK_hrms_JobOffer_hrms_Employee_HiringManagerEmployeeId",
                schema: "Core",
                table: "hrms_JobOffer");

            migrationBuilder.DropForeignKey(
                name: "FK_hrms_JobOffer_hrms_JobApplication_ApplicationId",
                schema: "Core",
                table: "hrms_JobOffer");

            migrationBuilder.DropForeignKey(
                name: "FK_hrms_JobRequisition_coreSalaryScale_SalaryScaleId",
                schema: "Core",
                table: "hrms_JobRequisition");

            migrationBuilder.DropForeignKey(
                name: "FK_hrms_JobRequisition_hrms_HiringRequest_HiringRequestId",
                schema: "Core",
                table: "hrms_JobRequisition");

            migrationBuilder.DropForeignKey(
                name: "FK_hrms_JobRequisition_hrms_OrganizationUnit_OrganizationUnitId",
                schema: "Core",
                table: "hrms_JobRequisition");

            migrationBuilder.DropForeignKey(
                name: "FK_hrms_JobRequisition_hrms_PositionClass_PositionClassId",
                schema: "Core",
                table: "hrms_JobRequisition");

            migrationBuilder.DropForeignKey(
                name: "FK_hrms_JobRequisition_hrms_WorkLocation_WorkLocationId",
                schema: "Core",
                table: "hrms_JobRequisition");

            migrationBuilder.DropForeignKey(
                name: "FK_hrms_LeaveBalance_FiscalYear_FiscalYearId",
                schema: "Core",
                table: "hrms_LeaveBalance");

            migrationBuilder.DropForeignKey(
                name: "FK_hrms_LeaveBalance_hrms_Employee_EmployeeId",
                schema: "Core",
                table: "hrms_LeaveBalance");

            migrationBuilder.DropForeignKey(
                name: "FK_hrms_LeaveBalance_hrms_LeaveType_LeaveTypeId",
                schema: "Core",
                table: "hrms_LeaveBalance");

            migrationBuilder.DropForeignKey(
                name: "FK_hrms_LeaveRequest_FiscalYear_FiscalYearId",
                schema: "Core",
                table: "hrms_LeaveRequest");

            migrationBuilder.DropForeignKey(
                name: "FK_hrms_LeaveRequest_hrms_Employee_EmployeeId",
                schema: "Core",
                table: "hrms_LeaveRequest");

            migrationBuilder.DropForeignKey(
                name: "FK_hrms_LeaveRequestLine_hrms_LeaveRequest_LeaveRequestId",
                schema: "Core",
                table: "hrms_LeaveRequestLine");

            migrationBuilder.DropForeignKey(
                name: "FK_hrms_LeaveRequestLine_hrms_LeaveType_LeaveTypeId",
                schema: "Core",
                table: "hrms_LeaveRequestLine");

            migrationBuilder.DropForeignKey(
                name: "FK_hrms_OrganizationalObjective_hrms_OrganizationUnit_OrganizationUnitId",
                schema: "Core",
                table: "hrms_OrganizationalObjective");

            migrationBuilder.DropForeignKey(
                name: "FK_hrms_OrganizationalObjective_hrms_OrganizationalObjective_ParentObjectiveId",
                schema: "Core",
                table: "hrms_OrganizationalObjective");

            migrationBuilder.DropForeignKey(
                name: "FK_hrms_OrganizationalObjective_hrms_ReviewCycle_ReviewCycleId",
                schema: "Core",
                table: "hrms_OrganizationalObjective");

            migrationBuilder.DropForeignKey(
                name: "FK_hrms_OrganizationUnit_hrms_Branch_BranchId",
                schema: "Core",
                table: "hrms_OrganizationUnit");

            migrationBuilder.DropForeignKey(
                name: "FK_hrms_OrganizationUnit_hrms_OrganizationUnit_ParentId",
                schema: "Core",
                table: "hrms_OrganizationUnit");

            migrationBuilder.DropForeignKey(
                name: "FK_hrms_OrganizationUnit_hrms_WorkLocation_WorkLocationId",
                schema: "Core",
                table: "hrms_OrganizationUnit");

            migrationBuilder.DropForeignKey(
                name: "FK_hrms_PipObjective_hrms_ImprovementPlan_PipId",
                schema: "Core",
                table: "hrms_PipObjective");

            migrationBuilder.DropForeignKey(
                name: "FK_hrms_Position_hrms_Branch_BranchId",
                schema: "Core",
                table: "hrms_Position");

            migrationBuilder.DropForeignKey(
                name: "FK_hrms_Position_hrms_OrganizationUnit_OrganizationUnitId",
                schema: "Core",
                table: "hrms_Position");

            migrationBuilder.DropForeignKey(
                name: "FK_hrms_Position_hrms_PositionClass_PositionClassId",
                schema: "Core",
                table: "hrms_Position");

            migrationBuilder.DropForeignKey(
                name: "FK_hrms_PositionClass_coreSalaryScale_SalaryScaleId",
                schema: "Core",
                table: "hrms_PositionClass");

            migrationBuilder.DropForeignKey(
                name: "FK_hrms_PositionClass_hrms_JobCategory_JobCategoryId",
                schema: "Core",
                table: "hrms_PositionClass");

            migrationBuilder.DropForeignKey(
                name: "FK_hrms_PositionClass_hrms_PositionClass_ReportsToPositionClassId",
                schema: "Core",
                table: "hrms_PositionClass");

            migrationBuilder.DropForeignKey(
                name: "FK_hrms_PositionClass_hrms_WorkLocation_WorkLocationId",
                schema: "Core",
                table: "hrms_PositionClass");

            migrationBuilder.DropForeignKey(
                name: "FK_hrms_PositionCompetency_hrms_Competency_CompetencyId",
                schema: "Core",
                table: "hrms_PositionCompetency");

            migrationBuilder.DropForeignKey(
                name: "FK_hrms_PositionCompetency_hrms_Position_PositionId",
                schema: "Core",
                table: "hrms_PositionCompetency");

            migrationBuilder.DropForeignKey(
                name: "FK_hrms_RatingScaleLevel_hrms_RatingScale_RatingScaleId",
                schema: "Core",
                table: "hrms_RatingScaleLevel");

            migrationBuilder.DropForeignKey(
                name: "FK_hrms_ReportField_hrms_Report_ReportId",
                schema: "Core",
                table: "hrms_ReportField");

            migrationBuilder.DropForeignKey(
                name: "FK_hrms_ReportFieldOutput_hrms_Report_ReportId",
                schema: "Core",
                table: "hrms_ReportFieldOutput");

            migrationBuilder.DropForeignKey(
                name: "FK_hrms_ReportRestriction_hrms_Report_ReportId",
                schema: "Core",
                table: "hrms_ReportRestriction");

            migrationBuilder.DropForeignKey(
                name: "FK_hrms_ReportRunRecipient_hrms_ReportRun_ReportRunId",
                schema: "Core",
                table: "hrms_ReportRunRecipient");

            migrationBuilder.DropForeignKey(
                name: "FK_hrms_ReportSavedFilter_hrms_Report_ReportId",
                schema: "Core",
                table: "hrms_ReportSavedFilter");

            migrationBuilder.DropForeignKey(
                name: "FK_hrms_ReportSchedule_hrms_Report_ReportId",
                schema: "Core",
                table: "hrms_ReportSchedule");

            migrationBuilder.DropForeignKey(
                name: "FK_hrms_ReportScheduleFieldOutput_hrms_ReportSchedule_ReportScheduleId",
                schema: "Core",
                table: "hrms_ReportScheduleFieldOutput");

            migrationBuilder.DropForeignKey(
                name: "FK_hrms_ReportScheduleFieldValue_hrms_ReportSchedule_ReportScheduleId",
                schema: "Core",
                table: "hrms_ReportScheduleFieldValue");

            migrationBuilder.DropForeignKey(
                name: "FK_hrms_ReportScheduleRecipient_hrms_ReportSchedule_ReportScheduleId",
                schema: "Core",
                table: "hrms_ReportScheduleRecipient");

            migrationBuilder.DropForeignKey(
                name: "FK_hrms_RequisitionScreeningCriterion_hrms_JobRequisition_RequisitionId",
                schema: "Core",
                table: "hrms_RequisitionScreeningCriterion");

            migrationBuilder.DropForeignKey(
                name: "FK_hrms_ReviewCycle_FiscalYear_FiscalYearId",
                schema: "Core",
                table: "hrms_ReviewCycle");

            migrationBuilder.DropForeignKey(
                name: "FK_hrms_ReviewCycle_hrms_RatingScale_RatingScaleId",
                schema: "Core",
                table: "hrms_ReviewCycle");

            migrationBuilder.DropForeignKey(
                name: "FK_hrms_TerminationClearance_hrms_ClearanceDepartment_DepartmentId",
                schema: "Core",
                table: "hrms_TerminationClearance");

            migrationBuilder.DropForeignKey(
                name: "FK_hrms_TerminationClearance_hrms_EmployeeTermination_TerminationId",
                schema: "Core",
                table: "hrms_TerminationClearance");

            migrationBuilder.DropForeignKey(
                name: "FK_hrms_WorkflowActionLog_hrms_WorkflowInstance_InstanceId",
                schema: "Core",
                table: "hrms_WorkflowActionLog");

            migrationBuilder.DropForeignKey(
                name: "FK_hrms_WorkflowInstance_hrms_WorkflowDefinition_DefinitionId",
                schema: "Core",
                table: "hrms_WorkflowInstance");

            migrationBuilder.DropForeignKey(
                name: "FK_hrms_WorkflowStep_hrms_WorkflowDefinition_DefinitionId",
                schema: "Core",
                table: "hrms_WorkflowStep");

            migrationBuilder.DropForeignKey(
                name: "FK_hrms_WorkflowStepApprover_hrms_WorkflowStep_StepId",
                schema: "Core",
                table: "hrms_WorkflowStepApprover");

            migrationBuilder.DropForeignKey(
                name: "FK_hrms_WorkforcePlan_FiscalYear_StartFiscalYearId",
                schema: "Core",
                table: "hrms_WorkforcePlan");

            migrationBuilder.DropForeignKey(
                name: "FK_hrms_WorkforcePlan_hrms_OrganizationUnit_OrganizationUnitId",
                schema: "Core",
                table: "hrms_WorkforcePlan");

            migrationBuilder.DropForeignKey(
                name: "FK_hrms_WorkforcePlanLine_hrms_OrganizationUnit_OrganizationUnitId",
                schema: "Core",
                table: "hrms_WorkforcePlanLine");

            migrationBuilder.DropForeignKey(
                name: "FK_hrms_WorkforcePlanLine_hrms_PositionClass_PositionClassId",
                schema: "Core",
                table: "hrms_WorkforcePlanLine");

            migrationBuilder.DropForeignKey(
                name: "FK_hrms_WorkforcePlanLine_hrms_WorkforcePlan_PlanId",
                schema: "Core",
                table: "hrms_WorkforcePlanLine");

            migrationBuilder.DropForeignKey(
                name: "FK_hrms_WorkLocation_hrms_WorkLocation_ParentId",
                schema: "Core",
                table: "hrms_WorkLocation");

            migrationBuilder.DropForeignKey(
                name: "FK_User_hrms_Employee_EmployeeId",
                schema: "Core",
                table: "User");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrms_WorkWeekConfiguration",
                schema: "Core",
                table: "hrms_WorkWeekConfiguration");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrms_WorkLocation",
                schema: "Core",
                table: "hrms_WorkLocation");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrms_WorkforcePlanLine",
                schema: "Core",
                table: "hrms_WorkforcePlanLine");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrms_WorkforcePlan",
                schema: "Core",
                table: "hrms_WorkforcePlan");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrms_WorkflowStepApprover",
                schema: "Core",
                table: "hrms_WorkflowStepApprover");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrms_WorkflowStep",
                schema: "Core",
                table: "hrms_WorkflowStep");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrms_WorkflowInstance",
                schema: "Core",
                table: "hrms_WorkflowInstance");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrms_WorkflowDefinition",
                schema: "Core",
                table: "hrms_WorkflowDefinition");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrms_WorkflowActionLog",
                schema: "Core",
                table: "hrms_WorkflowActionLog");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrms_TerminationClearance",
                schema: "Core",
                table: "hrms_TerminationClearance");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrms_ReviewCycle",
                schema: "Core",
                table: "hrms_ReviewCycle");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrms_RequisitionScreeningCriterion",
                schema: "Core",
                table: "hrms_RequisitionScreeningCriterion");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrms_ReportScheduleRecipient",
                schema: "Core",
                table: "hrms_ReportScheduleRecipient");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrms_ReportScheduleFieldValue",
                schema: "Core",
                table: "hrms_ReportScheduleFieldValue");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrms_ReportScheduleFieldOutput",
                schema: "Core",
                table: "hrms_ReportScheduleFieldOutput");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrms_ReportSchedule",
                schema: "Core",
                table: "hrms_ReportSchedule");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrms_ReportSavedFilter",
                schema: "Core",
                table: "hrms_ReportSavedFilter");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrms_ReportRunRecipient",
                schema: "Core",
                table: "hrms_ReportRunRecipient");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrms_ReportRun",
                schema: "Core",
                table: "hrms_ReportRun");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrms_ReportRestriction",
                schema: "Core",
                table: "hrms_ReportRestriction");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrms_ReportFieldOutput",
                schema: "Core",
                table: "hrms_ReportFieldOutput");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrms_ReportField",
                schema: "Core",
                table: "hrms_ReportField");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrms_Report",
                schema: "Core",
                table: "hrms_Report");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrms_RecognitionBadge",
                schema: "Core",
                table: "hrms_RecognitionBadge");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrms_RatingScaleLevel",
                schema: "Core",
                table: "hrms_RatingScaleLevel");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrms_RatingScale",
                schema: "Core",
                table: "hrms_RatingScale");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrms_PositionCompetency",
                schema: "Core",
                table: "hrms_PositionCompetency");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrms_PositionClass",
                schema: "Core",
                table: "hrms_PositionClass");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrms_Position",
                schema: "Core",
                table: "hrms_Position");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrms_PipObjective",
                schema: "Core",
                table: "hrms_PipObjective");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrms_PerformanceHistory",
                schema: "Core",
                table: "hrms_PerformanceHistory");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrms_OrganizationUnit",
                schema: "Core",
                table: "hrms_OrganizationUnit");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrms_OrganizationalObjective",
                schema: "Core",
                table: "hrms_OrganizationalObjective");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrms_OfferLetterTemplate",
                schema: "Core",
                table: "hrms_OfferLetterTemplate");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrms_NumberSequence",
                schema: "Core",
                table: "hrms_NumberSequence");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrms_LeaveType",
                schema: "Core",
                table: "hrms_LeaveType");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrms_LeaveRequestLine",
                schema: "Core",
                table: "hrms_LeaveRequestLine");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrms_LeaveRequest",
                schema: "Core",
                table: "hrms_LeaveRequest");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrms_LeaveBalanceTransaction",
                schema: "Core",
                table: "hrms_LeaveBalanceTransaction");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrms_LeaveBalance",
                schema: "Core",
                table: "hrms_LeaveBalance");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrms_JobRequisition",
                schema: "Core",
                table: "hrms_JobRequisition");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrms_JobOffer",
                schema: "Core",
                table: "hrms_JobOffer");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrms_JobGrade",
                schema: "Core",
                table: "hrms_JobGrade");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrms_JobCategory",
                schema: "Core",
                table: "hrms_JobCategory");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrms_JobApplicationStageLog",
                schema: "Core",
                table: "hrms_JobApplicationStageLog");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrms_JobApplication",
                schema: "Core",
                table: "hrms_JobApplication");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrms_InterviewPanelist",
                schema: "Core",
                table: "hrms_InterviewPanelist");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrms_InterviewFeedback",
                schema: "Core",
                table: "hrms_InterviewFeedback");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrms_Interview",
                schema: "Core",
                table: "hrms_Interview");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrms_ImprovementPlan",
                schema: "Core",
                table: "hrms_ImprovementPlan");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrms_Holiday",
                schema: "Core",
                table: "hrms_Holiday");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrms_HiringRequest",
                schema: "Core",
                table: "hrms_HiringRequest");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrms_GoalActionItem",
                schema: "Core",
                table: "hrms_GoalActionItem");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrms_EmployeeTermination",
                schema: "Core",
                table: "hrms_EmployeeTermination");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrms_EmployeeRecognition",
                schema: "Core",
                table: "hrms_EmployeeRecognition");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrms_EmployeeMovement",
                schema: "Core",
                table: "hrms_EmployeeMovement");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrms_EmployeeGoal",
                schema: "Core",
                table: "hrms_EmployeeGoal");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrms_EmployeeFieldValue",
                schema: "Core",
                table: "hrms_EmployeeFieldValue");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrms_EmployeeFieldDefinition",
                schema: "Core",
                table: "hrms_EmployeeFieldDefinition");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrms_EmployeeExperience",
                schema: "Core",
                table: "hrms_EmployeeExperience");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrms_EmployeeEducation",
                schema: "Core",
                table: "hrms_EmployeeEducation");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrms_EmployeeDocument",
                schema: "Core",
                table: "hrms_EmployeeDocument");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrms_EmployeeDependent",
                schema: "Core",
                table: "hrms_EmployeeDependent");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrms_Employee",
                schema: "Core",
                table: "hrms_Employee");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrms_DynamicFormRecord",
                schema: "Core",
                table: "hrms_DynamicFormRecord");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrms_DynamicFormField",
                schema: "Core",
                table: "hrms_DynamicFormField");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrms_DynamicForm",
                schema: "Core",
                table: "hrms_DynamicForm");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrms_DocumentTemplate",
                schema: "Core",
                table: "hrms_DocumentTemplate");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrms_DisciplinaryMeasure",
                schema: "Core",
                table: "hrms_DisciplinaryMeasure");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrms_DevelopmentPlan",
                schema: "Core",
                table: "hrms_DevelopmentPlan");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrms_DevelopmentAction",
                schema: "Core",
                table: "hrms_DevelopmentAction");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrms_CriterionEvaluator",
                schema: "Core",
                table: "hrms_CriterionEvaluator");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrms_CompetencyCategory",
                schema: "Core",
                table: "hrms_CompetencyCategory");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrms_Competency",
                schema: "Core",
                table: "hrms_Competency");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrms_CompanyProfile",
                schema: "Core",
                table: "hrms_CompanyProfile");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrms_ClearanceDepartmentApprover",
                schema: "Core",
                table: "hrms_ClearanceDepartmentApprover");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrms_ClearanceDepartment",
                schema: "Core",
                table: "hrms_ClearanceDepartment");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrms_CandidateDocument",
                schema: "Core",
                table: "hrms_CandidateDocument");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrms_Candidate",
                schema: "Core",
                table: "hrms_Candidate");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrms_CalibrationSession",
                schema: "Core",
                table: "hrms_CalibrationSession");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrms_CalibrationItem",
                schema: "Core",
                table: "hrms_CalibrationItem");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrms_Branch",
                schema: "Core",
                table: "hrms_Branch");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrms_AuditLog",
                schema: "Core",
                table: "hrms_AuditLog");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrms_AppraisalTemplate",
                schema: "Core",
                table: "hrms_AppraisalTemplate");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrms_AppraisalPeerReview",
                schema: "Core",
                table: "hrms_AppraisalPeerReview");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrms_AppraisalGoal",
                schema: "Core",
                table: "hrms_AppraisalGoal");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrms_AppraisalCompetency",
                schema: "Core",
                table: "hrms_AppraisalCompetency");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrms_AppraisalAppeal",
                schema: "Core",
                table: "hrms_AppraisalAppeal");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrms_Appraisal",
                schema: "Core",
                table: "hrms_Appraisal");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrms_ApplicationCriterionScore",
                schema: "Core",
                table: "hrms_ApplicationCriterionScore");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrms_AnnualLeaveSetting",
                schema: "Core",
                table: "hrms_AnnualLeaveSetting");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrms_AnnualLeaveHeader",
                schema: "Core",
                table: "hrms_AnnualLeaveHeader");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrms_AnnualLeaveDetail",
                schema: "Core",
                table: "hrms_AnnualLeaveDetail");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrms_Achievement",
                schema: "Core",
                table: "hrms_Achievement");

            migrationBuilder.EnsureSchema(
                name: "dbo");

            migrationBuilder.RenameTable(
                name: "hrms_WorkWeekConfiguration",
                schema: "Core",
                newName: "hrmsWorkWeekConfiguration",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "hrms_WorkLocation",
                schema: "Core",
                newName: "hrmsWorkLocation",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "hrms_WorkforcePlanLine",
                schema: "Core",
                newName: "hrmsWorkforcePlanLine",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "hrms_WorkforcePlan",
                schema: "Core",
                newName: "hrmsWorkforcePlan",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "hrms_WorkflowStepApprover",
                schema: "Core",
                newName: "hrmsWorkflowStepApprover",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "hrms_WorkflowStep",
                schema: "Core",
                newName: "hrmsWorkflowStep",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "hrms_WorkflowInstance",
                schema: "Core",
                newName: "hrmsWorkflowInstance",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "hrms_WorkflowDefinition",
                schema: "Core",
                newName: "hrmsWorkflowDefinition",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "hrms_WorkflowActionLog",
                schema: "Core",
                newName: "hrmsWorkflowActionLog",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "hrms_TerminationClearance",
                schema: "Core",
                newName: "hrmsTerminationClearance",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "hrms_ReviewCycle",
                schema: "Core",
                newName: "hrmsReviewCycle",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "hrms_RequisitionScreeningCriterion",
                schema: "Core",
                newName: "hrmsRequisitionScreeningCriterion",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "hrms_ReportScheduleRecipient",
                schema: "Core",
                newName: "hrmsReportScheduleRecipient",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "hrms_ReportScheduleFieldValue",
                schema: "Core",
                newName: "hrmsReportScheduleFieldValue",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "hrms_ReportScheduleFieldOutput",
                schema: "Core",
                newName: "hrmsReportScheduleFieldOutput",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "hrms_ReportSchedule",
                schema: "Core",
                newName: "hrmsReportSchedule",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "hrms_ReportSavedFilter",
                schema: "Core",
                newName: "hrmsReportSavedFilter",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "hrms_ReportRunRecipient",
                schema: "Core",
                newName: "hrmsReportRunRecipient",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "hrms_ReportRun",
                schema: "Core",
                newName: "hrmsReportRun",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "hrms_ReportRestriction",
                schema: "Core",
                newName: "hrmsReportRestriction",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "hrms_ReportFieldOutput",
                schema: "Core",
                newName: "hrmsReportFieldOutput",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "hrms_ReportField",
                schema: "Core",
                newName: "hrmsReportField",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "hrms_Report",
                schema: "Core",
                newName: "hrmsReport",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "hrms_RecognitionBadge",
                schema: "Core",
                newName: "hrmsRecognitionBadge",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "hrms_RatingScaleLevel",
                schema: "Core",
                newName: "hrmsRatingScaleLevel",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "hrms_RatingScale",
                schema: "Core",
                newName: "hrmsRatingScale",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "hrms_PositionCompetency",
                schema: "Core",
                newName: "hrmsPositionCompetency",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "hrms_PositionClass",
                schema: "Core",
                newName: "hrmsPositionClass",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "hrms_Position",
                schema: "Core",
                newName: "hrmsPosition",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "hrms_PipObjective",
                schema: "Core",
                newName: "hrmsPipObjective",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "hrms_PerformanceHistory",
                schema: "Core",
                newName: "hrmsPerformanceHistory",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "hrms_OrganizationUnit",
                schema: "Core",
                newName: "hrmsOrganizationUnit",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "hrms_OrganizationalObjective",
                schema: "Core",
                newName: "hrmsOrganizationalObjective",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "hrms_OfferLetterTemplate",
                schema: "Core",
                newName: "hrmsOfferLetterTemplate",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "hrms_NumberSequence",
                schema: "Core",
                newName: "hrmsNumberSequence",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "hrms_LeaveType",
                schema: "Core",
                newName: "hrmsLeaveType",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "hrms_LeaveRequestLine",
                schema: "Core",
                newName: "hrmsLeaveRequestLine",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "hrms_LeaveRequest",
                schema: "Core",
                newName: "hrmsLeaveRequest",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "hrms_LeaveBalanceTransaction",
                schema: "Core",
                newName: "hrmsLeaveBalanceTransaction",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "hrms_LeaveBalance",
                schema: "Core",
                newName: "hrmsLeaveBalance",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "hrms_JobRequisition",
                schema: "Core",
                newName: "hrmsJobRequisition",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "hrms_JobOffer",
                schema: "Core",
                newName: "hrmsJobOffer",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "hrms_JobGrade",
                schema: "Core",
                newName: "hrmsJobGrade",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "hrms_JobCategory",
                schema: "Core",
                newName: "hrmsJobCategory",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "hrms_JobApplicationStageLog",
                schema: "Core",
                newName: "hrmsJobApplicationStageLog",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "hrms_JobApplication",
                schema: "Core",
                newName: "hrmsJobApplication",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "hrms_InterviewPanelist",
                schema: "Core",
                newName: "hrmsInterviewPanelist",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "hrms_InterviewFeedback",
                schema: "Core",
                newName: "hrmsInterviewFeedback",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "hrms_Interview",
                schema: "Core",
                newName: "hrmsInterview",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "hrms_ImprovementPlan",
                schema: "Core",
                newName: "hrmsImprovementPlan",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "hrms_Holiday",
                schema: "Core",
                newName: "hrmsHoliday",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "hrms_HiringRequest",
                schema: "Core",
                newName: "hrmsHiringRequest",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "hrms_GoalActionItem",
                schema: "Core",
                newName: "hrmsGoalActionItem",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "hrms_EmployeeTermination",
                schema: "Core",
                newName: "hrmsEmployeeTermination",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "hrms_EmployeeRecognition",
                schema: "Core",
                newName: "hrmsEmployeeRecognition",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "hrms_EmployeeMovement",
                schema: "Core",
                newName: "hrmsEmployeeMovement",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "hrms_EmployeeGoal",
                schema: "Core",
                newName: "hrmsEmployeeGoal",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "hrms_EmployeeFieldValue",
                schema: "Core",
                newName: "hrmsEmployeeFieldValue",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "hrms_EmployeeFieldDefinition",
                schema: "Core",
                newName: "hrmsEmployeeFieldDefinition",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "hrms_EmployeeExperience",
                schema: "Core",
                newName: "hrmsEmployeeExperience",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "hrms_EmployeeEducation",
                schema: "Core",
                newName: "hrmsEmployeeEducation",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "hrms_EmployeeDocument",
                schema: "Core",
                newName: "hrmsEmployeeDocument",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "hrms_EmployeeDependent",
                schema: "Core",
                newName: "hrmsEmployeeDependent",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "hrms_Employee",
                schema: "Core",
                newName: "hrmsEmployee",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "hrms_DynamicFormRecord",
                schema: "Core",
                newName: "hrmsDynamicFormRecord",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "hrms_DynamicFormField",
                schema: "Core",
                newName: "hrmsDynamicFormField",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "hrms_DynamicForm",
                schema: "Core",
                newName: "hrmsDynamicForm",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "hrms_DocumentTemplate",
                schema: "Core",
                newName: "hrmsDocumentTemplate",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "hrms_DisciplinaryMeasure",
                schema: "Core",
                newName: "hrmsDisciplinaryMeasure",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "hrms_DevelopmentPlan",
                schema: "Core",
                newName: "hrmsDevelopmentPlan",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "hrms_DevelopmentAction",
                schema: "Core",
                newName: "hrmsDevelopmentAction",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "hrms_CriterionEvaluator",
                schema: "Core",
                newName: "hrmsCriterionEvaluator",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "hrms_CompetencyCategory",
                schema: "Core",
                newName: "hrmsCompetencyCategory",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "hrms_Competency",
                schema: "Core",
                newName: "hrmsCompetency",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "hrms_CompanyProfile",
                schema: "Core",
                newName: "hrmsCompanyProfile",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "hrms_ClearanceDepartmentApprover",
                schema: "Core",
                newName: "hrmsClearanceDepartmentApprover",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "hrms_ClearanceDepartment",
                schema: "Core",
                newName: "hrmsClearanceDepartment",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "hrms_CandidateDocument",
                schema: "Core",
                newName: "hrmsCandidateDocument",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "hrms_Candidate",
                schema: "Core",
                newName: "hrmsCandidate",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "hrms_CalibrationSession",
                schema: "Core",
                newName: "hrmsCalibrationSession",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "hrms_CalibrationItem",
                schema: "Core",
                newName: "hrmsCalibrationItem",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "hrms_Branch",
                schema: "Core",
                newName: "hrmsBranch",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "hrms_AuditLog",
                schema: "Core",
                newName: "hrmsAuditLog",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "hrms_AppraisalTemplate",
                schema: "Core",
                newName: "hrmsAppraisalTemplate",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "hrms_AppraisalPeerReview",
                schema: "Core",
                newName: "hrmsAppraisalPeerReview",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "hrms_AppraisalGoal",
                schema: "Core",
                newName: "hrmsAppraisalGoal",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "hrms_AppraisalCompetency",
                schema: "Core",
                newName: "hrmsAppraisalCompetency",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "hrms_AppraisalAppeal",
                schema: "Core",
                newName: "hrmsAppraisalAppeal",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "hrms_Appraisal",
                schema: "Core",
                newName: "hrmsAppraisal",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "hrms_ApplicationCriterionScore",
                schema: "Core",
                newName: "hrmsApplicationCriterionScore",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "hrms_AnnualLeaveSetting",
                schema: "Core",
                newName: "hrmsAnnualLeaveSetting",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "hrms_AnnualLeaveHeader",
                schema: "Core",
                newName: "hrmsAnnualLeaveHeader",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "hrms_AnnualLeaveDetail",
                schema: "Core",
                newName: "hrmsAnnualLeaveDetail",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "hrms_Achievement",
                schema: "Core",
                newName: "hrmsAchievement",
                newSchema: "dbo");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_WorkWeekConfiguration_TenantId_IsActive",
                schema: "dbo",
                table: "hrmsWorkWeekConfiguration",
                newName: "IX_hrmsWorkWeekConfiguration_TenantId_IsActive");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_WorkLocation_TenantId_Code",
                schema: "dbo",
                table: "hrmsWorkLocation",
                newName: "IX_hrmsWorkLocation_TenantId_Code");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_WorkLocation_ParentId",
                schema: "dbo",
                table: "hrmsWorkLocation",
                newName: "IX_hrmsWorkLocation_ParentId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_WorkforcePlanLine_PositionClassId",
                schema: "dbo",
                table: "hrmsWorkforcePlanLine",
                newName: "IX_hrmsWorkforcePlanLine_PositionClassId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_WorkforcePlanLine_PlanId_OrganizationUnitId_PositionClassId_EmploymentType_PeriodIndex",
                schema: "dbo",
                table: "hrmsWorkforcePlanLine",
                newName: "IX_hrmsWorkforcePlanLine_PlanId_OrganizationUnitId_PositionClassId_EmploymentType_PeriodIndex");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_WorkforcePlanLine_PlanId",
                schema: "dbo",
                table: "hrmsWorkforcePlanLine",
                newName: "IX_hrmsWorkforcePlanLine_PlanId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_WorkforcePlanLine_OrganizationUnitId",
                schema: "dbo",
                table: "hrmsWorkforcePlanLine",
                newName: "IX_hrmsWorkforcePlanLine_OrganizationUnitId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_WorkforcePlan_TenantId_Status",
                schema: "dbo",
                table: "hrmsWorkforcePlan",
                newName: "IX_hrmsWorkforcePlan_TenantId_Status");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_WorkforcePlan_StartFiscalYearId",
                schema: "dbo",
                table: "hrmsWorkforcePlan",
                newName: "IX_hrmsWorkforcePlan_StartFiscalYearId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_WorkforcePlan_RootPlanId",
                schema: "dbo",
                table: "hrmsWorkforcePlan",
                newName: "IX_hrmsWorkforcePlan_RootPlanId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_WorkforcePlan_OrganizationUnitId",
                schema: "dbo",
                table: "hrmsWorkforcePlan",
                newName: "IX_hrmsWorkforcePlan_OrganizationUnitId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_WorkflowStepApprover_StepId",
                schema: "dbo",
                table: "hrmsWorkflowStepApprover",
                newName: "IX_hrmsWorkflowStepApprover_StepId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_WorkflowStepApprover_ApproverType_ApproverId",
                schema: "dbo",
                table: "hrmsWorkflowStepApprover",
                newName: "IX_hrmsWorkflowStepApprover_ApproverType_ApproverId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_WorkflowStep_DefinitionId_StepOrder",
                schema: "dbo",
                table: "hrmsWorkflowStep",
                newName: "IX_hrmsWorkflowStep_DefinitionId_StepOrder");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_WorkflowInstance_Status",
                schema: "dbo",
                table: "hrmsWorkflowInstance",
                newName: "IX_hrmsWorkflowInstance_Status");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_WorkflowInstance_EntityType_EntityId",
                schema: "dbo",
                table: "hrmsWorkflowInstance",
                newName: "IX_hrmsWorkflowInstance_EntityType_EntityId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_WorkflowInstance_DefinitionId",
                schema: "dbo",
                table: "hrmsWorkflowInstance",
                newName: "IX_hrmsWorkflowInstance_DefinitionId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_WorkflowDefinition_TenantId_EntityType",
                schema: "dbo",
                table: "hrmsWorkflowDefinition",
                newName: "IX_hrmsWorkflowDefinition_TenantId_EntityType");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_WorkflowActionLog_InstanceId",
                schema: "dbo",
                table: "hrmsWorkflowActionLog",
                newName: "IX_hrmsWorkflowActionLog_InstanceId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_TerminationClearance_TerminationId",
                schema: "dbo",
                table: "hrmsTerminationClearance",
                newName: "IX_hrmsTerminationClearance_TerminationId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_TerminationClearance_DepartmentId",
                schema: "dbo",
                table: "hrmsTerminationClearance",
                newName: "IX_hrmsTerminationClearance_DepartmentId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_ReviewCycle_TenantId_Status",
                schema: "dbo",
                table: "hrmsReviewCycle",
                newName: "IX_hrmsReviewCycle_TenantId_Status");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_ReviewCycle_TenantId_Name",
                schema: "dbo",
                table: "hrmsReviewCycle",
                newName: "IX_hrmsReviewCycle_TenantId_Name");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_ReviewCycle_RatingScaleId",
                schema: "dbo",
                table: "hrmsReviewCycle",
                newName: "IX_hrmsReviewCycle_RatingScaleId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_ReviewCycle_FiscalYearId",
                schema: "dbo",
                table: "hrmsReviewCycle",
                newName: "IX_hrmsReviewCycle_FiscalYearId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_RequisitionScreeningCriterion_RequisitionId",
                schema: "dbo",
                table: "hrmsRequisitionScreeningCriterion",
                newName: "IX_hrmsRequisitionScreeningCriterion_RequisitionId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_ReportScheduleRecipient_ReportScheduleId",
                schema: "dbo",
                table: "hrmsReportScheduleRecipient",
                newName: "IX_hrmsReportScheduleRecipient_ReportScheduleId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_ReportScheduleFieldValue_ReportScheduleId",
                schema: "dbo",
                table: "hrmsReportScheduleFieldValue",
                newName: "IX_hrmsReportScheduleFieldValue_ReportScheduleId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_ReportScheduleFieldOutput_ReportScheduleId",
                schema: "dbo",
                table: "hrmsReportScheduleFieldOutput",
                newName: "IX_hrmsReportScheduleFieldOutput_ReportScheduleId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_ReportSchedule_ReportId",
                schema: "dbo",
                table: "hrmsReportSchedule",
                newName: "IX_hrmsReportSchedule_ReportId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_ReportSavedFilter_ReportId",
                schema: "dbo",
                table: "hrmsReportSavedFilter",
                newName: "IX_hrmsReportSavedFilter_ReportId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_ReportRunRecipient_ReportRunId",
                schema: "dbo",
                table: "hrmsReportRunRecipient",
                newName: "IX_hrmsReportRunRecipient_ReportRunId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_ReportRun_TenantId_ReportKey",
                schema: "dbo",
                table: "hrmsReportRun",
                newName: "IX_hrmsReportRun_TenantId_ReportKey");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_ReportRestriction_RoleId",
                schema: "dbo",
                table: "hrmsReportRestriction",
                newName: "IX_hrmsReportRestriction_RoleId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_ReportRestriction_ReportId",
                schema: "dbo",
                table: "hrmsReportRestriction",
                newName: "IX_hrmsReportRestriction_ReportId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_ReportFieldOutput_ReportId",
                schema: "dbo",
                table: "hrmsReportFieldOutput",
                newName: "IX_hrmsReportFieldOutput_ReportId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_ReportField_ReportId",
                schema: "dbo",
                table: "hrmsReportField",
                newName: "IX_hrmsReportField_ReportId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_Report_TenantId_ReportKey",
                schema: "dbo",
                table: "hrmsReport",
                newName: "IX_hrmsReport_TenantId_ReportKey");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_Report_TenantId_IsActive",
                schema: "dbo",
                table: "hrmsReport",
                newName: "IX_hrmsReport_TenantId_IsActive");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_RecognitionBadge_TenantId_Name",
                schema: "dbo",
                table: "hrmsRecognitionBadge",
                newName: "IX_hrmsRecognitionBadge_TenantId_Name");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_RatingScaleLevel_RatingScaleId_Value",
                schema: "dbo",
                table: "hrmsRatingScaleLevel",
                newName: "IX_hrmsRatingScaleLevel_RatingScaleId_Value");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_RatingScale_TenantId_Name",
                schema: "dbo",
                table: "hrmsRatingScale",
                newName: "IX_hrmsRatingScale_TenantId_Name");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_PositionCompetency_PositionId_CompetencyId",
                schema: "dbo",
                table: "hrmsPositionCompetency",
                newName: "IX_hrmsPositionCompetency_PositionId_CompetencyId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_PositionCompetency_CompetencyId",
                schema: "dbo",
                table: "hrmsPositionCompetency",
                newName: "IX_hrmsPositionCompetency_CompetencyId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_PositionClass_WorkLocationId",
                schema: "dbo",
                table: "hrmsPositionClass",
                newName: "IX_hrmsPositionClass_WorkLocationId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_PositionClass_TenantId_Code",
                schema: "dbo",
                table: "hrmsPositionClass",
                newName: "IX_hrmsPositionClass_TenantId_Code");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_PositionClass_SalaryScaleId",
                schema: "dbo",
                table: "hrmsPositionClass",
                newName: "IX_hrmsPositionClass_SalaryScaleId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_PositionClass_ReportsToPositionClassId",
                schema: "dbo",
                table: "hrmsPositionClass",
                newName: "IX_hrmsPositionClass_ReportsToPositionClassId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_PositionClass_JobCategoryId",
                schema: "dbo",
                table: "hrmsPositionClass",
                newName: "IX_hrmsPositionClass_JobCategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_Position_TenantId_BranchId_Code",
                schema: "dbo",
                table: "hrmsPosition",
                newName: "IX_hrmsPosition_TenantId_BranchId_Code");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_Position_PositionClassId",
                schema: "dbo",
                table: "hrmsPosition",
                newName: "IX_hrmsPosition_PositionClassId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_Position_OrganizationUnitId",
                schema: "dbo",
                table: "hrmsPosition",
                newName: "IX_hrmsPosition_OrganizationUnitId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_Position_BranchId",
                schema: "dbo",
                table: "hrmsPosition",
                newName: "IX_hrmsPosition_BranchId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_PipObjective_PipId_SortOrder",
                schema: "dbo",
                table: "hrmsPipObjective",
                newName: "IX_hrmsPipObjective_PipId_SortOrder");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_PerformanceHistory_TenantId_EntityType_EntityId",
                schema: "dbo",
                table: "hrmsPerformanceHistory",
                newName: "IX_hrmsPerformanceHistory_TenantId_EntityType_EntityId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_OrganizationUnit_WorkLocationId",
                schema: "dbo",
                table: "hrmsOrganizationUnit",
                newName: "IX_hrmsOrganizationUnit_WorkLocationId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_OrganizationUnit_TenantId_BranchId_Code",
                schema: "dbo",
                table: "hrmsOrganizationUnit",
                newName: "IX_hrmsOrganizationUnit_TenantId_BranchId_Code");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_OrganizationUnit_ParentId",
                schema: "dbo",
                table: "hrmsOrganizationUnit",
                newName: "IX_hrmsOrganizationUnit_ParentId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_OrganizationUnit_BranchId",
                schema: "dbo",
                table: "hrmsOrganizationUnit",
                newName: "IX_hrmsOrganizationUnit_BranchId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_OrganizationalObjective_TenantId_ReviewCycleId_Title",
                schema: "dbo",
                table: "hrmsOrganizationalObjective",
                newName: "IX_hrmsOrganizationalObjective_TenantId_ReviewCycleId_Title");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_OrganizationalObjective_TenantId_ReviewCycleId",
                schema: "dbo",
                table: "hrmsOrganizationalObjective",
                newName: "IX_hrmsOrganizationalObjective_TenantId_ReviewCycleId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_OrganizationalObjective_ReviewCycleId",
                schema: "dbo",
                table: "hrmsOrganizationalObjective",
                newName: "IX_hrmsOrganizationalObjective_ReviewCycleId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_OrganizationalObjective_ParentObjectiveId",
                schema: "dbo",
                table: "hrmsOrganizationalObjective",
                newName: "IX_hrmsOrganizationalObjective_ParentObjectiveId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_OrganizationalObjective_OrganizationUnitId",
                schema: "dbo",
                table: "hrmsOrganizationalObjective",
                newName: "IX_hrmsOrganizationalObjective_OrganizationUnitId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_OfferLetterTemplate_TenantId",
                schema: "dbo",
                table: "hrmsOfferLetterTemplate",
                newName: "IX_hrmsOfferLetterTemplate_TenantId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_LeaveType_TenantId_Code",
                schema: "dbo",
                table: "hrmsLeaveType",
                newName: "IX_hrmsLeaveType_TenantId_Code");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_LeaveRequestLine_LeaveTypeId",
                schema: "dbo",
                table: "hrmsLeaveRequestLine",
                newName: "IX_hrmsLeaveRequestLine_LeaveTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_LeaveRequestLine_LeaveRequestId",
                schema: "dbo",
                table: "hrmsLeaveRequestLine",
                newName: "IX_hrmsLeaveRequestLine_LeaveRequestId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_LeaveRequest_FiscalYearId",
                schema: "dbo",
                table: "hrmsLeaveRequest",
                newName: "IX_hrmsLeaveRequest_FiscalYearId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_LeaveRequest_EmployeeId_Status",
                schema: "dbo",
                table: "hrmsLeaveRequest",
                newName: "IX_hrmsLeaveRequest_EmployeeId_Status");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_LeaveRequest_EmployeeId",
                schema: "dbo",
                table: "hrmsLeaveRequest",
                newName: "IX_hrmsLeaveRequest_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_LeaveBalanceTransaction_ReferenceId",
                schema: "dbo",
                table: "hrmsLeaveBalanceTransaction",
                newName: "IX_hrmsLeaveBalanceTransaction_ReferenceId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_LeaveBalanceTransaction_EmployeeId_LeaveTypeId_FiscalYearId",
                schema: "dbo",
                table: "hrmsLeaveBalanceTransaction",
                newName: "IX_hrmsLeaveBalanceTransaction_EmployeeId_LeaveTypeId_FiscalYearId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_LeaveBalance_TenantId_EmployeeId_LeaveTypeId_FiscalYearId",
                schema: "dbo",
                table: "hrmsLeaveBalance",
                newName: "IX_hrmsLeaveBalance_TenantId_EmployeeId_LeaveTypeId_FiscalYearId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_LeaveBalance_LeaveTypeId",
                schema: "dbo",
                table: "hrmsLeaveBalance",
                newName: "IX_hrmsLeaveBalance_LeaveTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_LeaveBalance_FiscalYearId",
                schema: "dbo",
                table: "hrmsLeaveBalance",
                newName: "IX_hrmsLeaveBalance_FiscalYearId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_LeaveBalance_EmployeeId",
                schema: "dbo",
                table: "hrmsLeaveBalance",
                newName: "IX_hrmsLeaveBalance_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_JobRequisition_WorkLocationId",
                schema: "dbo",
                table: "hrmsJobRequisition",
                newName: "IX_hrmsJobRequisition_WorkLocationId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_JobRequisition_TenantId_Status",
                schema: "dbo",
                table: "hrmsJobRequisition",
                newName: "IX_hrmsJobRequisition_TenantId_Status");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_JobRequisition_TenantId_RequisitionNumber",
                schema: "dbo",
                table: "hrmsJobRequisition",
                newName: "IX_hrmsJobRequisition_TenantId_RequisitionNumber");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_JobRequisition_SalaryScaleId",
                schema: "dbo",
                table: "hrmsJobRequisition",
                newName: "IX_hrmsJobRequisition_SalaryScaleId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_JobRequisition_PositionClassId",
                schema: "dbo",
                table: "hrmsJobRequisition",
                newName: "IX_hrmsJobRequisition_PositionClassId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_JobRequisition_OrganizationUnitId",
                schema: "dbo",
                table: "hrmsJobRequisition",
                newName: "IX_hrmsJobRequisition_OrganizationUnitId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_JobRequisition_HiringRequestId",
                schema: "dbo",
                table: "hrmsJobRequisition",
                newName: "IX_hrmsJobRequisition_HiringRequestId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_JobOffer_TenantId_Status",
                schema: "dbo",
                table: "hrmsJobOffer",
                newName: "IX_hrmsJobOffer_TenantId_Status");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_JobOffer_TenantId_OfferNumber",
                schema: "dbo",
                table: "hrmsJobOffer",
                newName: "IX_hrmsJobOffer_TenantId_OfferNumber");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_JobOffer_SalaryScaleId",
                schema: "dbo",
                table: "hrmsJobOffer",
                newName: "IX_hrmsJobOffer_SalaryScaleId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_JobOffer_HiringManagerEmployeeId",
                schema: "dbo",
                table: "hrmsJobOffer",
                newName: "IX_hrmsJobOffer_HiringManagerEmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_JobOffer_HiredEmployeeId",
                schema: "dbo",
                table: "hrmsJobOffer",
                newName: "IX_hrmsJobOffer_HiredEmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_JobOffer_ApplicationId_CreatedAt",
                schema: "dbo",
                table: "hrmsJobOffer",
                newName: "IX_hrmsJobOffer_ApplicationId_CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_JobGrade_TenantId_Code",
                schema: "dbo",
                table: "hrmsJobGrade",
                newName: "IX_hrmsJobGrade_TenantId_Code");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_JobCategory_TenantId_Code",
                schema: "dbo",
                table: "hrmsJobCategory",
                newName: "IX_hrmsJobCategory_TenantId_Code");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_JobApplicationStageLog_ApplicationId",
                schema: "dbo",
                table: "hrmsJobApplicationStageLog",
                newName: "IX_hrmsJobApplicationStageLog_ApplicationId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_JobApplication_TenantId_Stage",
                schema: "dbo",
                table: "hrmsJobApplication",
                newName: "IX_hrmsJobApplication_TenantId_Stage");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_JobApplication_TenantId_AppliedAt",
                schema: "dbo",
                table: "hrmsJobApplication",
                newName: "IX_hrmsJobApplication_TenantId_AppliedAt");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_JobApplication_RequisitionId",
                schema: "dbo",
                table: "hrmsJobApplication",
                newName: "IX_hrmsJobApplication_RequisitionId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_JobApplication_CandidateId_RequisitionId",
                schema: "dbo",
                table: "hrmsJobApplication",
                newName: "IX_hrmsJobApplication_CandidateId_RequisitionId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_InterviewPanelist_InterviewId_EmployeeId",
                schema: "dbo",
                table: "hrmsInterviewPanelist",
                newName: "IX_hrmsInterviewPanelist_InterviewId_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_InterviewPanelist_EmployeeId",
                schema: "dbo",
                table: "hrmsInterviewPanelist",
                newName: "IX_hrmsInterviewPanelist_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_InterviewFeedback_PanelistId_CriterionId",
                schema: "dbo",
                table: "hrmsInterviewFeedback",
                newName: "IX_hrmsInterviewFeedback_PanelistId_CriterionId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_InterviewFeedback_PanelistId",
                schema: "dbo",
                table: "hrmsInterviewFeedback",
                newName: "IX_hrmsInterviewFeedback_PanelistId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_Interview_TenantId_Status",
                schema: "dbo",
                table: "hrmsInterview",
                newName: "IX_hrmsInterview_TenantId_Status");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_Interview_ScheduledStart",
                schema: "dbo",
                table: "hrmsInterview",
                newName: "IX_hrmsInterview_ScheduledStart");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_Interview_ApplicationId",
                schema: "dbo",
                table: "hrmsInterview",
                newName: "IX_hrmsInterview_ApplicationId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_ImprovementPlan_TenantId_EmployeeId",
                schema: "dbo",
                table: "hrmsImprovementPlan",
                newName: "IX_hrmsImprovementPlan_TenantId_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_ImprovementPlan_EmployeeId",
                schema: "dbo",
                table: "hrmsImprovementPlan",
                newName: "IX_hrmsImprovementPlan_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_ImprovementPlan_AppraisalId",
                schema: "dbo",
                table: "hrmsImprovementPlan",
                newName: "IX_hrmsImprovementPlan_AppraisalId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_Holiday_TenantId_Date",
                schema: "dbo",
                table: "hrmsHoliday",
                newName: "IX_hrmsHoliday_TenantId_Date");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_HiringRequest_TenantId_Status",
                schema: "dbo",
                table: "hrmsHiringRequest",
                newName: "IX_hrmsHiringRequest_TenantId_Status");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_HiringRequest_TenantId_RequestNumber",
                schema: "dbo",
                table: "hrmsHiringRequest",
                newName: "IX_hrmsHiringRequest_TenantId_RequestNumber");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_HiringRequest_PositionClassId",
                schema: "dbo",
                table: "hrmsHiringRequest",
                newName: "IX_hrmsHiringRequest_PositionClassId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_HiringRequest_OrganizationUnitId",
                schema: "dbo",
                table: "hrmsHiringRequest",
                newName: "IX_hrmsHiringRequest_OrganizationUnitId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_GoalActionItem_EmployeeGoalId_SortOrder",
                schema: "dbo",
                table: "hrmsGoalActionItem",
                newName: "IX_hrmsGoalActionItem_EmployeeGoalId_SortOrder");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_EmployeeTermination_Status",
                schema: "dbo",
                table: "hrmsEmployeeTermination",
                newName: "IX_hrmsEmployeeTermination_Status");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_EmployeeTermination_EmployeeId",
                schema: "dbo",
                table: "hrmsEmployeeTermination",
                newName: "IX_hrmsEmployeeTermination_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_EmployeeRecognition_TenantId_IsPublic",
                schema: "dbo",
                table: "hrmsEmployeeRecognition",
                newName: "IX_hrmsEmployeeRecognition_TenantId_IsPublic");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_EmployeeRecognition_TenantId_EmployeeId",
                schema: "dbo",
                table: "hrmsEmployeeRecognition",
                newName: "IX_hrmsEmployeeRecognition_TenantId_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_EmployeeRecognition_RecognitionBadgeId",
                schema: "dbo",
                table: "hrmsEmployeeRecognition",
                newName: "IX_hrmsEmployeeRecognition_RecognitionBadgeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_EmployeeRecognition_EmployeeId",
                schema: "dbo",
                table: "hrmsEmployeeRecognition",
                newName: "IX_hrmsEmployeeRecognition_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_EmployeeMovement_ToSalaryScaleId",
                schema: "dbo",
                table: "hrmsEmployeeMovement",
                newName: "IX_hrmsEmployeeMovement_ToSalaryScaleId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_EmployeeMovement_Status_EffectiveDate",
                schema: "dbo",
                table: "hrmsEmployeeMovement",
                newName: "IX_hrmsEmployeeMovement_Status_EffectiveDate");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_EmployeeMovement_EmployeeId",
                schema: "dbo",
                table: "hrmsEmployeeMovement",
                newName: "IX_hrmsEmployeeMovement_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_EmployeeGoal_TenantId_EmployeeId_ReviewCycleId",
                schema: "dbo",
                table: "hrmsEmployeeGoal",
                newName: "IX_hrmsEmployeeGoal_TenantId_EmployeeId_ReviewCycleId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_EmployeeGoal_ReviewCycleId",
                schema: "dbo",
                table: "hrmsEmployeeGoal",
                newName: "IX_hrmsEmployeeGoal_ReviewCycleId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_EmployeeGoal_OrganizationalObjectiveId",
                schema: "dbo",
                table: "hrmsEmployeeGoal",
                newName: "IX_hrmsEmployeeGoal_OrganizationalObjectiveId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_EmployeeGoal_EmployeeId",
                schema: "dbo",
                table: "hrmsEmployeeGoal",
                newName: "IX_hrmsEmployeeGoal_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_EmployeeFieldValue_OwnerType_OwnerId_FieldDefinitionId",
                schema: "dbo",
                table: "hrmsEmployeeFieldValue",
                newName: "IX_hrmsEmployeeFieldValue_OwnerType_OwnerId_FieldDefinitionId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_EmployeeFieldValue_FieldDefinitionId",
                schema: "dbo",
                table: "hrmsEmployeeFieldValue",
                newName: "IX_hrmsEmployeeFieldValue_FieldDefinitionId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_EmployeeFieldDefinition_TenantId_OwnerType_Name",
                schema: "dbo",
                table: "hrmsEmployeeFieldDefinition",
                newName: "IX_hrmsEmployeeFieldDefinition_TenantId_OwnerType_Name");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_EmployeeExperience_PersonId",
                schema: "dbo",
                table: "hrmsEmployeeExperience",
                newName: "IX_hrmsEmployeeExperience_PersonId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_EmployeeEducation_PersonId",
                schema: "dbo",
                table: "hrmsEmployeeEducation",
                newName: "IX_hrmsEmployeeEducation_PersonId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_EmployeeDocument_OwnerType_OwnerId",
                schema: "dbo",
                table: "hrmsEmployeeDocument",
                newName: "IX_hrmsEmployeeDocument_OwnerType_OwnerId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_EmployeeDocument_EmployeeId",
                schema: "dbo",
                table: "hrmsEmployeeDocument",
                newName: "IX_hrmsEmployeeDocument_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_EmployeeDependent_RelatedEmployeeId",
                schema: "dbo",
                table: "hrmsEmployeeDependent",
                newName: "IX_hrmsEmployeeDependent_RelatedEmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_EmployeeDependent_PersonId",
                schema: "dbo",
                table: "hrmsEmployeeDependent",
                newName: "IX_hrmsEmployeeDependent_PersonId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_Employee_TenantId_PositionId_EmployeeNumber",
                schema: "dbo",
                table: "hrmsEmployee",
                newName: "IX_hrmsEmployee_TenantId_PositionId_EmployeeNumber");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_Employee_TenantId_EmployeeNumber",
                schema: "dbo",
                table: "hrmsEmployee",
                newName: "IX_hrmsEmployee_TenantId_EmployeeNumber");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_Employee_SalaryScaleId",
                schema: "dbo",
                table: "hrmsEmployee",
                newName: "IX_hrmsEmployee_SalaryScaleId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_Employee_PositionId",
                schema: "dbo",
                table: "hrmsEmployee",
                newName: "IX_hrmsEmployee_PositionId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_Employee_PersonId",
                schema: "dbo",
                table: "hrmsEmployee",
                newName: "IX_hrmsEmployee_PersonId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_Employee_EmploymentStatus_IsProbation",
                schema: "dbo",
                table: "hrmsEmployee",
                newName: "IX_hrmsEmployee_EmploymentStatus_IsProbation");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_Employee_EmploymentStatus",
                schema: "dbo",
                table: "hrmsEmployee",
                newName: "IX_hrmsEmployee_EmploymentStatus");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_Employee_DateOfBirth",
                schema: "dbo",
                table: "hrmsEmployee",
                newName: "IX_hrmsEmployee_DateOfBirth");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_Employee_BranchId",
                schema: "dbo",
                table: "hrmsEmployee",
                newName: "IX_hrmsEmployee_BranchId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_DynamicFormRecord_DynamicFormId_OwnerType_OwnerId_CreatedAt",
                schema: "dbo",
                table: "hrmsDynamicFormRecord",
                newName: "IX_hrmsDynamicFormRecord_DynamicFormId_OwnerType_OwnerId_CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_DynamicFormField_DynamicFormId_Name",
                schema: "dbo",
                table: "hrmsDynamicFormField",
                newName: "IX_hrmsDynamicFormField_DynamicFormId_Name");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_DynamicForm_TenantId_Module_Name",
                schema: "dbo",
                table: "hrmsDynamicForm",
                newName: "IX_hrmsDynamicForm_TenantId_Module_Name");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_DocumentTemplate_TenantId_Name",
                schema: "dbo",
                table: "hrmsDocumentTemplate",
                newName: "IX_hrmsDocumentTemplate_TenantId_Name");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_DisciplinaryMeasure_Status",
                schema: "dbo",
                table: "hrmsDisciplinaryMeasure",
                newName: "IX_hrmsDisciplinaryMeasure_Status");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_DisciplinaryMeasure_EmployeeId",
                schema: "dbo",
                table: "hrmsDisciplinaryMeasure",
                newName: "IX_hrmsDisciplinaryMeasure_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_DevelopmentPlan_TenantId_EmployeeId",
                schema: "dbo",
                table: "hrmsDevelopmentPlan",
                newName: "IX_hrmsDevelopmentPlan_TenantId_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_DevelopmentPlan_EmployeeId",
                schema: "dbo",
                table: "hrmsDevelopmentPlan",
                newName: "IX_hrmsDevelopmentPlan_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_DevelopmentPlan_AppraisalId",
                schema: "dbo",
                table: "hrmsDevelopmentPlan",
                newName: "IX_hrmsDevelopmentPlan_AppraisalId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_DevelopmentAction_DevelopmentPlanId_SortOrder",
                schema: "dbo",
                table: "hrmsDevelopmentAction",
                newName: "IX_hrmsDevelopmentAction_DevelopmentPlanId_SortOrder");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_DevelopmentAction_CompetencyId",
                schema: "dbo",
                table: "hrmsDevelopmentAction",
                newName: "IX_hrmsDevelopmentAction_CompetencyId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_CriterionEvaluator_EmployeeId",
                schema: "dbo",
                table: "hrmsCriterionEvaluator",
                newName: "IX_hrmsCriterionEvaluator_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_CriterionEvaluator_CriterionId",
                schema: "dbo",
                table: "hrmsCriterionEvaluator",
                newName: "IX_hrmsCriterionEvaluator_CriterionId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_CompetencyCategory_TenantId_Name",
                schema: "dbo",
                table: "hrmsCompetencyCategory",
                newName: "IX_hrmsCompetencyCategory_TenantId_Name");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_Competency_TenantId_Name",
                schema: "dbo",
                table: "hrmsCompetency",
                newName: "IX_hrmsCompetency_TenantId_Name");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_Competency_CompetencyCategoryId",
                schema: "dbo",
                table: "hrmsCompetency",
                newName: "IX_hrmsCompetency_CompetencyCategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_CompanyProfile_TenantId",
                schema: "dbo",
                table: "hrmsCompanyProfile",
                newName: "IX_hrmsCompanyProfile_TenantId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_ClearanceDepartmentApprover_DepartmentId",
                schema: "dbo",
                table: "hrmsClearanceDepartmentApprover",
                newName: "IX_hrmsClearanceDepartmentApprover_DepartmentId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_ClearanceDepartmentApprover_ApproverType_ApproverId",
                schema: "dbo",
                table: "hrmsClearanceDepartmentApprover",
                newName: "IX_hrmsClearanceDepartmentApprover_ApproverType_ApproverId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_ClearanceDepartment_TenantId_Name",
                schema: "dbo",
                table: "hrmsClearanceDepartment",
                newName: "IX_hrmsClearanceDepartment_TenantId_Name");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_CandidateDocument_CandidateId_DocumentType",
                schema: "dbo",
                table: "hrmsCandidateDocument",
                newName: "IX_hrmsCandidateDocument_CandidateId_DocumentType");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_Candidate_TenantId_IsInTalentPool",
                schema: "dbo",
                table: "hrmsCandidate",
                newName: "IX_hrmsCandidate_TenantId_IsInTalentPool");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_Candidate_TenantId_CandidateNumber",
                schema: "dbo",
                table: "hrmsCandidate",
                newName: "IX_hrmsCandidate_TenantId_CandidateNumber");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_Candidate_PersonId",
                schema: "dbo",
                table: "hrmsCandidate",
                newName: "IX_hrmsCandidate_PersonId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_Candidate_InternalEmployeeId",
                schema: "dbo",
                table: "hrmsCandidate",
                newName: "IX_hrmsCandidate_InternalEmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_Candidate_HiredEmployeeId",
                schema: "dbo",
                table: "hrmsCandidate",
                newName: "IX_hrmsCandidate_HiredEmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_Candidate_Email",
                schema: "dbo",
                table: "hrmsCandidate",
                newName: "IX_hrmsCandidate_Email");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_CalibrationSession_TenantId_ReviewCycleId",
                schema: "dbo",
                table: "hrmsCalibrationSession",
                newName: "IX_hrmsCalibrationSession_TenantId_ReviewCycleId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_CalibrationSession_ReviewCycleId",
                schema: "dbo",
                table: "hrmsCalibrationSession",
                newName: "IX_hrmsCalibrationSession_ReviewCycleId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_CalibrationSession_OrganizationUnitId",
                schema: "dbo",
                table: "hrmsCalibrationSession",
                newName: "IX_hrmsCalibrationSession_OrganizationUnitId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_CalibrationItem_CalibrationSessionId",
                schema: "dbo",
                table: "hrmsCalibrationItem",
                newName: "IX_hrmsCalibrationItem_CalibrationSessionId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_CalibrationItem_AppraisalId",
                schema: "dbo",
                table: "hrmsCalibrationItem",
                newName: "IX_hrmsCalibrationItem_AppraisalId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_Branch_TenantId_Code",
                schema: "dbo",
                table: "hrmsBranch",
                newName: "IX_hrmsBranch_TenantId_Code");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_Branch_ParentId",
                schema: "dbo",
                table: "hrmsBranch",
                newName: "IX_hrmsBranch_ParentId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_AuditLog_EntityType_EntityId",
                schema: "dbo",
                table: "hrmsAuditLog",
                newName: "IX_hrmsAuditLog_EntityType_EntityId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_AuditLog_CreatedAt",
                schema: "dbo",
                table: "hrmsAuditLog",
                newName: "IX_hrmsAuditLog_CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_AuditLog_BranchId",
                schema: "dbo",
                table: "hrmsAuditLog",
                newName: "IX_hrmsAuditLog_BranchId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_AuditLog_Action",
                schema: "dbo",
                table: "hrmsAuditLog",
                newName: "IX_hrmsAuditLog_Action");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_AppraisalTemplate_TenantId_Name",
                schema: "dbo",
                table: "hrmsAppraisalTemplate",
                newName: "IX_hrmsAppraisalTemplate_TenantId_Name");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_AppraisalPeerReview_PeerEmployeeId",
                schema: "dbo",
                table: "hrmsAppraisalPeerReview",
                newName: "IX_hrmsAppraisalPeerReview_PeerEmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_AppraisalPeerReview_AppraisalId_PeerEmployeeId",
                schema: "dbo",
                table: "hrmsAppraisalPeerReview",
                newName: "IX_hrmsAppraisalPeerReview_AppraisalId_PeerEmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_AppraisalGoal_AppraisalId_SortOrder",
                schema: "dbo",
                table: "hrmsAppraisalGoal",
                newName: "IX_hrmsAppraisalGoal_AppraisalId_SortOrder");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_AppraisalCompetency_AppraisalId_SortOrder",
                schema: "dbo",
                table: "hrmsAppraisalCompetency",
                newName: "IX_hrmsAppraisalCompetency_AppraisalId_SortOrder");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_AppraisalAppeal_TenantId_Status",
                schema: "dbo",
                table: "hrmsAppraisalAppeal",
                newName: "IX_hrmsAppraisalAppeal_TenantId_Status");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_AppraisalAppeal_EmployeeId",
                schema: "dbo",
                table: "hrmsAppraisalAppeal",
                newName: "IX_hrmsAppraisalAppeal_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_AppraisalAppeal_AppraisalId",
                schema: "dbo",
                table: "hrmsAppraisalAppeal",
                newName: "IX_hrmsAppraisalAppeal_AppraisalId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_Appraisal_TenantId_ReviewCycleId_Stage",
                schema: "dbo",
                table: "hrmsAppraisal",
                newName: "IX_hrmsAppraisal_TenantId_ReviewCycleId_Stage");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_Appraisal_TenantId_EmployeeId_ReviewCycleId",
                schema: "dbo",
                table: "hrmsAppraisal",
                newName: "IX_hrmsAppraisal_TenantId_EmployeeId_ReviewCycleId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_Appraisal_ReviewCycleId",
                schema: "dbo",
                table: "hrmsAppraisal",
                newName: "IX_hrmsAppraisal_ReviewCycleId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_Appraisal_EmployeeId",
                schema: "dbo",
                table: "hrmsAppraisal",
                newName: "IX_hrmsAppraisal_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_ApplicationCriterionScore_ApplicationId_CriterionId",
                schema: "dbo",
                table: "hrmsApplicationCriterionScore",
                newName: "IX_hrmsApplicationCriterionScore_ApplicationId_CriterionId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_AnnualLeaveSetting_TenantId_FiscalYearId_LeaveTypeId",
                schema: "dbo",
                table: "hrmsAnnualLeaveSetting",
                newName: "IX_hrmsAnnualLeaveSetting_TenantId_FiscalYearId_LeaveTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_AnnualLeaveSetting_LeaveTypeId",
                schema: "dbo",
                table: "hrmsAnnualLeaveSetting",
                newName: "IX_hrmsAnnualLeaveSetting_LeaveTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_AnnualLeaveSetting_FiscalYearId",
                schema: "dbo",
                table: "hrmsAnnualLeaveSetting",
                newName: "IX_hrmsAnnualLeaveSetting_FiscalYearId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_AnnualLeaveHeader_EmployeeId_Status",
                schema: "dbo",
                table: "hrmsAnnualLeaveHeader",
                newName: "IX_hrmsAnnualLeaveHeader_EmployeeId_Status");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_AnnualLeaveHeader_EmployeeId",
                schema: "dbo",
                table: "hrmsAnnualLeaveHeader",
                newName: "IX_hrmsAnnualLeaveHeader_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_AnnualLeaveHeader_AnnualLeaveLedgerId",
                schema: "dbo",
                table: "hrmsAnnualLeaveHeader",
                newName: "IX_hrmsAnnualLeaveHeader_AnnualLeaveLedgerId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_AnnualLeaveDetail_AnnualLeaveHeaderId_StartDate_EndDate",
                schema: "dbo",
                table: "hrmsAnnualLeaveDetail",
                newName: "IX_hrmsAnnualLeaveDetail_AnnualLeaveHeaderId_StartDate_EndDate");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_AnnualLeaveDetail_AnnualLeaveHeaderId",
                schema: "dbo",
                table: "hrmsAnnualLeaveDetail",
                newName: "IX_hrmsAnnualLeaveDetail_AnnualLeaveHeaderId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_Achievement_TenantId_EmployeeId",
                schema: "dbo",
                table: "hrmsAchievement",
                newName: "IX_hrmsAchievement_TenantId_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_Achievement_EmployeeId",
                schema: "dbo",
                table: "hrmsAchievement",
                newName: "IX_hrmsAchievement_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrms_Achievement_AppraisalId",
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
                name: "PK_hrmsTerminationClearance",
                schema: "dbo",
                table: "hrmsTerminationClearance",
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
                name: "PK_hrmsPerformanceHistory",
                schema: "dbo",
                table: "hrmsPerformanceHistory",
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
                name: "PK_hrmsGoalActionItem",
                schema: "dbo",
                table: "hrmsGoalActionItem",
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
                name: "PK_hrmsCompanyProfile",
                schema: "dbo",
                table: "hrmsCompanyProfile",
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
                name: "PK_hrmsAchievement",
                schema: "dbo",
                table: "hrmsAchievement",
                column: "Id");

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
                name: "FK_hrmsAnnualLeaveSetting_hrmsLeaveType_LeaveTypeId",
                schema: "dbo",
                table: "hrmsAnnualLeaveSetting",
                column: "LeaveTypeId",
                principalSchema: "dbo",
                principalTable: "hrmsLeaveType",
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
                name: "FK_hrmsClearanceDepartmentApprover_hrmsClearanceDepartment_DepartmentId",
                schema: "dbo",
                table: "hrmsClearanceDepartmentApprover",
                column: "DepartmentId",
                principalSchema: "dbo",
                principalTable: "hrmsClearanceDepartment",
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
                name: "FK_hrmsGoalActionItem_hrmsEmployeeGoal_EmployeeGoalId",
                schema: "dbo",
                table: "hrmsGoalActionItem",
                column: "EmployeeGoalId",
                principalSchema: "dbo",
                principalTable: "hrmsEmployeeGoal",
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
                name: "FK_hrmsRatingScaleLevel_hrmsRatingScale_RatingScaleId",
                schema: "dbo",
                table: "hrmsRatingScaleLevel",
                column: "RatingScaleId",
                principalSchema: "dbo",
                principalTable: "hrmsRatingScale",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

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
                name: "FK_User_hrmsEmployee_EmployeeId",
                schema: "Core",
                table: "User",
                column: "EmployeeId",
                principalSchema: "dbo",
                principalTable: "hrmsEmployee",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            // ---- Recreate report/lookup stored procedures to reference the renamed tables ----
            // hrms_Report_EmployeeDirectory
            migrationBuilder.Sql(@"
CREATE OR ALTER PROCEDURE [Core].[hrms_Report_EmployeeDirectory]
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
            // hrms_Report_EmployeeDirectoryGrouped
            migrationBuilder.Sql(@"
CREATE OR ALTER PROCEDURE Core.hrms_Report_EmployeeDirectoryGrouped
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
            // hrms_ReportActivate
            migrationBuilder.Sql(@"
CREATE OR ALTER PROCEDURE Core.hrms_ReportActivate
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
            // hrms_ReportClientSchedule
            migrationBuilder.Sql(@"
CREATE OR ALTER PROCEDURE Core.hrms_ReportClientSchedule
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
            // hrms_ReportClientScheduleDelete
            migrationBuilder.Sql(@"
CREATE OR ALTER PROCEDURE Core.hrms_ReportClientScheduleDelete
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
            // hrms_ReportClientScheduleEnable
            migrationBuilder.Sql(@"
CREATE OR ALTER PROCEDURE Core.hrms_ReportClientScheduleEnable
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
            // hrms_ReportClientScheduleFieldOutput
            migrationBuilder.Sql(@"
CREATE OR ALTER PROCEDURE Core.hrms_ReportClientScheduleFieldOutput
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
            // hrms_ReportClientScheduleFieldValue
            migrationBuilder.Sql(@"
CREATE OR ALTER PROCEDURE Core.hrms_ReportClientScheduleFieldValue
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
            // hrms_ReportClientScheduleRead
            migrationBuilder.Sql(@"
CREATE OR ALTER PROCEDURE Core.hrms_ReportClientScheduleRead
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
            // hrms_ReportClientScheduleRecipient
            migrationBuilder.Sql(@"
CREATE OR ALTER PROCEDURE Core.hrms_ReportClientScheduleRecipient
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
            // hrms_ReportDelete
            migrationBuilder.Sql(@"
CREATE OR ALTER PROCEDURE Core.hrms_ReportDelete
    @ReportId UNIQUEIDENTIFIER,
    @TenantId NVARCHAR(450) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM dbo.hrmsReport
     WHERE Id = @ReportId
       AND (@TenantId IS NULL OR @TenantId = '' OR TenantId = @TenantId);
END");
            // hrms_ReportFieldOutputRead
            migrationBuilder.Sql(@"
CREATE OR ALTER PROCEDURE Core.hrms_ReportFieldOutputRead
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
            // hrms_ReportFieldValues
            migrationBuilder.Sql(@"
CREATE OR ALTER PROCEDURE Core.hrms_ReportFieldValues
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
    ELSE
        SELECT CAST(NULL AS NVARCHAR(50)) AS Value, CAST(NULL AS NVARCHAR(200)) AS Label
        WHERE 1 = 0;
END");
            // hrms_ReportGenerateGetScheduleInfo
            migrationBuilder.Sql(@"
CREATE OR ALTER PROCEDURE Core.hrms_ReportGenerateGetScheduleInfo
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
            // hrms_ReportGenerateSendToHistory
            migrationBuilder.Sql(@"
CREATE OR ALTER PROCEDURE Core.hrms_ReportGenerateSendToHistory
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

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_coreSalaryScale_hrmsJobGrade_JobGradeId",
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
                name: "FK_hrmsAnnualLeaveSetting_hrmsLeaveType_LeaveTypeId",
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
                name: "FK_hrmsClearanceDepartmentApprover_hrmsClearanceDepartment_DepartmentId",
                schema: "dbo",
                table: "hrmsClearanceDepartmentApprover");

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
                name: "FK_hrmsGoalActionItem_hrmsEmployeeGoal_EmployeeGoalId",
                schema: "dbo",
                table: "hrmsGoalActionItem");

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
                name: "FK_hrmsRatingScaleLevel_hrmsRatingScale_RatingScaleId",
                schema: "dbo",
                table: "hrmsRatingScaleLevel");

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
                name: "FK_hrmsTerminationClearance_hrmsClearanceDepartment_DepartmentId",
                schema: "dbo",
                table: "hrmsTerminationClearance");

            migrationBuilder.DropForeignKey(
                name: "FK_hrmsTerminationClearance_hrmsEmployeeTermination_TerminationId",
                schema: "dbo",
                table: "hrmsTerminationClearance");

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
                name: "FK_User_hrmsEmployee_EmployeeId",
                schema: "Core",
                table: "User");

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
                name: "PK_hrmsTerminationClearance",
                schema: "dbo",
                table: "hrmsTerminationClearance");

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
                name: "PK_hrmsGoalActionItem",
                schema: "dbo",
                table: "hrmsGoalActionItem");

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
                name: "PK_hrmsCompanyProfile",
                schema: "dbo",
                table: "hrmsCompanyProfile");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsClearanceDepartmentApprover",
                schema: "dbo",
                table: "hrmsClearanceDepartmentApprover");

            migrationBuilder.DropPrimaryKey(
                name: "PK_hrmsClearanceDepartment",
                schema: "dbo",
                table: "hrmsClearanceDepartment");

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
                name: "PK_hrmsAchievement",
                schema: "dbo",
                table: "hrmsAchievement");

            migrationBuilder.RenameTable(
                name: "hrmsWorkWeekConfiguration",
                schema: "dbo",
                newName: "hrms_WorkWeekConfiguration",
                newSchema: "Core");

            migrationBuilder.RenameTable(
                name: "hrmsWorkLocation",
                schema: "dbo",
                newName: "hrms_WorkLocation",
                newSchema: "Core");

            migrationBuilder.RenameTable(
                name: "hrmsWorkforcePlanLine",
                schema: "dbo",
                newName: "hrms_WorkforcePlanLine",
                newSchema: "Core");

            migrationBuilder.RenameTable(
                name: "hrmsWorkforcePlan",
                schema: "dbo",
                newName: "hrms_WorkforcePlan",
                newSchema: "Core");

            migrationBuilder.RenameTable(
                name: "hrmsWorkflowStepApprover",
                schema: "dbo",
                newName: "hrms_WorkflowStepApprover",
                newSchema: "Core");

            migrationBuilder.RenameTable(
                name: "hrmsWorkflowStep",
                schema: "dbo",
                newName: "hrms_WorkflowStep",
                newSchema: "Core");

            migrationBuilder.RenameTable(
                name: "hrmsWorkflowInstance",
                schema: "dbo",
                newName: "hrms_WorkflowInstance",
                newSchema: "Core");

            migrationBuilder.RenameTable(
                name: "hrmsWorkflowDefinition",
                schema: "dbo",
                newName: "hrms_WorkflowDefinition",
                newSchema: "Core");

            migrationBuilder.RenameTable(
                name: "hrmsWorkflowActionLog",
                schema: "dbo",
                newName: "hrms_WorkflowActionLog",
                newSchema: "Core");

            migrationBuilder.RenameTable(
                name: "hrmsTerminationClearance",
                schema: "dbo",
                newName: "hrms_TerminationClearance",
                newSchema: "Core");

            migrationBuilder.RenameTable(
                name: "hrmsReviewCycle",
                schema: "dbo",
                newName: "hrms_ReviewCycle",
                newSchema: "Core");

            migrationBuilder.RenameTable(
                name: "hrmsRequisitionScreeningCriterion",
                schema: "dbo",
                newName: "hrms_RequisitionScreeningCriterion",
                newSchema: "Core");

            migrationBuilder.RenameTable(
                name: "hrmsReportScheduleRecipient",
                schema: "dbo",
                newName: "hrms_ReportScheduleRecipient",
                newSchema: "Core");

            migrationBuilder.RenameTable(
                name: "hrmsReportScheduleFieldValue",
                schema: "dbo",
                newName: "hrms_ReportScheduleFieldValue",
                newSchema: "Core");

            migrationBuilder.RenameTable(
                name: "hrmsReportScheduleFieldOutput",
                schema: "dbo",
                newName: "hrms_ReportScheduleFieldOutput",
                newSchema: "Core");

            migrationBuilder.RenameTable(
                name: "hrmsReportSchedule",
                schema: "dbo",
                newName: "hrms_ReportSchedule",
                newSchema: "Core");

            migrationBuilder.RenameTable(
                name: "hrmsReportSavedFilter",
                schema: "dbo",
                newName: "hrms_ReportSavedFilter",
                newSchema: "Core");

            migrationBuilder.RenameTable(
                name: "hrmsReportRunRecipient",
                schema: "dbo",
                newName: "hrms_ReportRunRecipient",
                newSchema: "Core");

            migrationBuilder.RenameTable(
                name: "hrmsReportRun",
                schema: "dbo",
                newName: "hrms_ReportRun",
                newSchema: "Core");

            migrationBuilder.RenameTable(
                name: "hrmsReportRestriction",
                schema: "dbo",
                newName: "hrms_ReportRestriction",
                newSchema: "Core");

            migrationBuilder.RenameTable(
                name: "hrmsReportFieldOutput",
                schema: "dbo",
                newName: "hrms_ReportFieldOutput",
                newSchema: "Core");

            migrationBuilder.RenameTable(
                name: "hrmsReportField",
                schema: "dbo",
                newName: "hrms_ReportField",
                newSchema: "Core");

            migrationBuilder.RenameTable(
                name: "hrmsReport",
                schema: "dbo",
                newName: "hrms_Report",
                newSchema: "Core");

            migrationBuilder.RenameTable(
                name: "hrmsRecognitionBadge",
                schema: "dbo",
                newName: "hrms_RecognitionBadge",
                newSchema: "Core");

            migrationBuilder.RenameTable(
                name: "hrmsRatingScaleLevel",
                schema: "dbo",
                newName: "hrms_RatingScaleLevel",
                newSchema: "Core");

            migrationBuilder.RenameTable(
                name: "hrmsRatingScale",
                schema: "dbo",
                newName: "hrms_RatingScale",
                newSchema: "Core");

            migrationBuilder.RenameTable(
                name: "hrmsPositionCompetency",
                schema: "dbo",
                newName: "hrms_PositionCompetency",
                newSchema: "Core");

            migrationBuilder.RenameTable(
                name: "hrmsPositionClass",
                schema: "dbo",
                newName: "hrms_PositionClass",
                newSchema: "Core");

            migrationBuilder.RenameTable(
                name: "hrmsPosition",
                schema: "dbo",
                newName: "hrms_Position",
                newSchema: "Core");

            migrationBuilder.RenameTable(
                name: "hrmsPipObjective",
                schema: "dbo",
                newName: "hrms_PipObjective",
                newSchema: "Core");

            migrationBuilder.RenameTable(
                name: "hrmsPerformanceHistory",
                schema: "dbo",
                newName: "hrms_PerformanceHistory",
                newSchema: "Core");

            migrationBuilder.RenameTable(
                name: "hrmsOrganizationUnit",
                schema: "dbo",
                newName: "hrms_OrganizationUnit",
                newSchema: "Core");

            migrationBuilder.RenameTable(
                name: "hrmsOrganizationalObjective",
                schema: "dbo",
                newName: "hrms_OrganizationalObjective",
                newSchema: "Core");

            migrationBuilder.RenameTable(
                name: "hrmsOfferLetterTemplate",
                schema: "dbo",
                newName: "hrms_OfferLetterTemplate",
                newSchema: "Core");

            migrationBuilder.RenameTable(
                name: "hrmsNumberSequence",
                schema: "dbo",
                newName: "hrms_NumberSequence",
                newSchema: "Core");

            migrationBuilder.RenameTable(
                name: "hrmsLeaveType",
                schema: "dbo",
                newName: "hrms_LeaveType",
                newSchema: "Core");

            migrationBuilder.RenameTable(
                name: "hrmsLeaveRequestLine",
                schema: "dbo",
                newName: "hrms_LeaveRequestLine",
                newSchema: "Core");

            migrationBuilder.RenameTable(
                name: "hrmsLeaveRequest",
                schema: "dbo",
                newName: "hrms_LeaveRequest",
                newSchema: "Core");

            migrationBuilder.RenameTable(
                name: "hrmsLeaveBalanceTransaction",
                schema: "dbo",
                newName: "hrms_LeaveBalanceTransaction",
                newSchema: "Core");

            migrationBuilder.RenameTable(
                name: "hrmsLeaveBalance",
                schema: "dbo",
                newName: "hrms_LeaveBalance",
                newSchema: "Core");

            migrationBuilder.RenameTable(
                name: "hrmsJobRequisition",
                schema: "dbo",
                newName: "hrms_JobRequisition",
                newSchema: "Core");

            migrationBuilder.RenameTable(
                name: "hrmsJobOffer",
                schema: "dbo",
                newName: "hrms_JobOffer",
                newSchema: "Core");

            migrationBuilder.RenameTable(
                name: "hrmsJobGrade",
                schema: "dbo",
                newName: "hrms_JobGrade",
                newSchema: "Core");

            migrationBuilder.RenameTable(
                name: "hrmsJobCategory",
                schema: "dbo",
                newName: "hrms_JobCategory",
                newSchema: "Core");

            migrationBuilder.RenameTable(
                name: "hrmsJobApplicationStageLog",
                schema: "dbo",
                newName: "hrms_JobApplicationStageLog",
                newSchema: "Core");

            migrationBuilder.RenameTable(
                name: "hrmsJobApplication",
                schema: "dbo",
                newName: "hrms_JobApplication",
                newSchema: "Core");

            migrationBuilder.RenameTable(
                name: "hrmsInterviewPanelist",
                schema: "dbo",
                newName: "hrms_InterviewPanelist",
                newSchema: "Core");

            migrationBuilder.RenameTable(
                name: "hrmsInterviewFeedback",
                schema: "dbo",
                newName: "hrms_InterviewFeedback",
                newSchema: "Core");

            migrationBuilder.RenameTable(
                name: "hrmsInterview",
                schema: "dbo",
                newName: "hrms_Interview",
                newSchema: "Core");

            migrationBuilder.RenameTable(
                name: "hrmsImprovementPlan",
                schema: "dbo",
                newName: "hrms_ImprovementPlan",
                newSchema: "Core");

            migrationBuilder.RenameTable(
                name: "hrmsHoliday",
                schema: "dbo",
                newName: "hrms_Holiday",
                newSchema: "Core");

            migrationBuilder.RenameTable(
                name: "hrmsHiringRequest",
                schema: "dbo",
                newName: "hrms_HiringRequest",
                newSchema: "Core");

            migrationBuilder.RenameTable(
                name: "hrmsGoalActionItem",
                schema: "dbo",
                newName: "hrms_GoalActionItem",
                newSchema: "Core");

            migrationBuilder.RenameTable(
                name: "hrmsEmployeeTermination",
                schema: "dbo",
                newName: "hrms_EmployeeTermination",
                newSchema: "Core");

            migrationBuilder.RenameTable(
                name: "hrmsEmployeeRecognition",
                schema: "dbo",
                newName: "hrms_EmployeeRecognition",
                newSchema: "Core");

            migrationBuilder.RenameTable(
                name: "hrmsEmployeeMovement",
                schema: "dbo",
                newName: "hrms_EmployeeMovement",
                newSchema: "Core");

            migrationBuilder.RenameTable(
                name: "hrmsEmployeeGoal",
                schema: "dbo",
                newName: "hrms_EmployeeGoal",
                newSchema: "Core");

            migrationBuilder.RenameTable(
                name: "hrmsEmployeeFieldValue",
                schema: "dbo",
                newName: "hrms_EmployeeFieldValue",
                newSchema: "Core");

            migrationBuilder.RenameTable(
                name: "hrmsEmployeeFieldDefinition",
                schema: "dbo",
                newName: "hrms_EmployeeFieldDefinition",
                newSchema: "Core");

            migrationBuilder.RenameTable(
                name: "hrmsEmployeeExperience",
                schema: "dbo",
                newName: "hrms_EmployeeExperience",
                newSchema: "Core");

            migrationBuilder.RenameTable(
                name: "hrmsEmployeeEducation",
                schema: "dbo",
                newName: "hrms_EmployeeEducation",
                newSchema: "Core");

            migrationBuilder.RenameTable(
                name: "hrmsEmployeeDocument",
                schema: "dbo",
                newName: "hrms_EmployeeDocument",
                newSchema: "Core");

            migrationBuilder.RenameTable(
                name: "hrmsEmployeeDependent",
                schema: "dbo",
                newName: "hrms_EmployeeDependent",
                newSchema: "Core");

            migrationBuilder.RenameTable(
                name: "hrmsEmployee",
                schema: "dbo",
                newName: "hrms_Employee",
                newSchema: "Core");

            migrationBuilder.RenameTable(
                name: "hrmsDynamicFormRecord",
                schema: "dbo",
                newName: "hrms_DynamicFormRecord",
                newSchema: "Core");

            migrationBuilder.RenameTable(
                name: "hrmsDynamicFormField",
                schema: "dbo",
                newName: "hrms_DynamicFormField",
                newSchema: "Core");

            migrationBuilder.RenameTable(
                name: "hrmsDynamicForm",
                schema: "dbo",
                newName: "hrms_DynamicForm",
                newSchema: "Core");

            migrationBuilder.RenameTable(
                name: "hrmsDocumentTemplate",
                schema: "dbo",
                newName: "hrms_DocumentTemplate",
                newSchema: "Core");

            migrationBuilder.RenameTable(
                name: "hrmsDisciplinaryMeasure",
                schema: "dbo",
                newName: "hrms_DisciplinaryMeasure",
                newSchema: "Core");

            migrationBuilder.RenameTable(
                name: "hrmsDevelopmentPlan",
                schema: "dbo",
                newName: "hrms_DevelopmentPlan",
                newSchema: "Core");

            migrationBuilder.RenameTable(
                name: "hrmsDevelopmentAction",
                schema: "dbo",
                newName: "hrms_DevelopmentAction",
                newSchema: "Core");

            migrationBuilder.RenameTable(
                name: "hrmsCriterionEvaluator",
                schema: "dbo",
                newName: "hrms_CriterionEvaluator",
                newSchema: "Core");

            migrationBuilder.RenameTable(
                name: "hrmsCompetencyCategory",
                schema: "dbo",
                newName: "hrms_CompetencyCategory",
                newSchema: "Core");

            migrationBuilder.RenameTable(
                name: "hrmsCompetency",
                schema: "dbo",
                newName: "hrms_Competency",
                newSchema: "Core");

            migrationBuilder.RenameTable(
                name: "hrmsCompanyProfile",
                schema: "dbo",
                newName: "hrms_CompanyProfile",
                newSchema: "Core");

            migrationBuilder.RenameTable(
                name: "hrmsClearanceDepartmentApprover",
                schema: "dbo",
                newName: "hrms_ClearanceDepartmentApprover",
                newSchema: "Core");

            migrationBuilder.RenameTable(
                name: "hrmsClearanceDepartment",
                schema: "dbo",
                newName: "hrms_ClearanceDepartment",
                newSchema: "Core");

            migrationBuilder.RenameTable(
                name: "hrmsCandidateDocument",
                schema: "dbo",
                newName: "hrms_CandidateDocument",
                newSchema: "Core");

            migrationBuilder.RenameTable(
                name: "hrmsCandidate",
                schema: "dbo",
                newName: "hrms_Candidate",
                newSchema: "Core");

            migrationBuilder.RenameTable(
                name: "hrmsCalibrationSession",
                schema: "dbo",
                newName: "hrms_CalibrationSession",
                newSchema: "Core");

            migrationBuilder.RenameTable(
                name: "hrmsCalibrationItem",
                schema: "dbo",
                newName: "hrms_CalibrationItem",
                newSchema: "Core");

            migrationBuilder.RenameTable(
                name: "hrmsBranch",
                schema: "dbo",
                newName: "hrms_Branch",
                newSchema: "Core");

            migrationBuilder.RenameTable(
                name: "hrmsAuditLog",
                schema: "dbo",
                newName: "hrms_AuditLog",
                newSchema: "Core");

            migrationBuilder.RenameTable(
                name: "hrmsAppraisalTemplate",
                schema: "dbo",
                newName: "hrms_AppraisalTemplate",
                newSchema: "Core");

            migrationBuilder.RenameTable(
                name: "hrmsAppraisalPeerReview",
                schema: "dbo",
                newName: "hrms_AppraisalPeerReview",
                newSchema: "Core");

            migrationBuilder.RenameTable(
                name: "hrmsAppraisalGoal",
                schema: "dbo",
                newName: "hrms_AppraisalGoal",
                newSchema: "Core");

            migrationBuilder.RenameTable(
                name: "hrmsAppraisalCompetency",
                schema: "dbo",
                newName: "hrms_AppraisalCompetency",
                newSchema: "Core");

            migrationBuilder.RenameTable(
                name: "hrmsAppraisalAppeal",
                schema: "dbo",
                newName: "hrms_AppraisalAppeal",
                newSchema: "Core");

            migrationBuilder.RenameTable(
                name: "hrmsAppraisal",
                schema: "dbo",
                newName: "hrms_Appraisal",
                newSchema: "Core");

            migrationBuilder.RenameTable(
                name: "hrmsApplicationCriterionScore",
                schema: "dbo",
                newName: "hrms_ApplicationCriterionScore",
                newSchema: "Core");

            migrationBuilder.RenameTable(
                name: "hrmsAnnualLeaveSetting",
                schema: "dbo",
                newName: "hrms_AnnualLeaveSetting",
                newSchema: "Core");

            migrationBuilder.RenameTable(
                name: "hrmsAnnualLeaveHeader",
                schema: "dbo",
                newName: "hrms_AnnualLeaveHeader",
                newSchema: "Core");

            migrationBuilder.RenameTable(
                name: "hrmsAnnualLeaveDetail",
                schema: "dbo",
                newName: "hrms_AnnualLeaveDetail",
                newSchema: "Core");

            migrationBuilder.RenameTable(
                name: "hrmsAchievement",
                schema: "dbo",
                newName: "hrms_Achievement",
                newSchema: "Core");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsWorkWeekConfiguration_TenantId_IsActive",
                schema: "Core",
                table: "hrms_WorkWeekConfiguration",
                newName: "IX_hrms_WorkWeekConfiguration_TenantId_IsActive");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsWorkLocation_TenantId_Code",
                schema: "Core",
                table: "hrms_WorkLocation",
                newName: "IX_hrms_WorkLocation_TenantId_Code");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsWorkLocation_ParentId",
                schema: "Core",
                table: "hrms_WorkLocation",
                newName: "IX_hrms_WorkLocation_ParentId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsWorkforcePlanLine_PositionClassId",
                schema: "Core",
                table: "hrms_WorkforcePlanLine",
                newName: "IX_hrms_WorkforcePlanLine_PositionClassId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsWorkforcePlanLine_PlanId_OrganizationUnitId_PositionClassId_EmploymentType_PeriodIndex",
                schema: "Core",
                table: "hrms_WorkforcePlanLine",
                newName: "IX_hrms_WorkforcePlanLine_PlanId_OrganizationUnitId_PositionClassId_EmploymentType_PeriodIndex");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsWorkforcePlanLine_PlanId",
                schema: "Core",
                table: "hrms_WorkforcePlanLine",
                newName: "IX_hrms_WorkforcePlanLine_PlanId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsWorkforcePlanLine_OrganizationUnitId",
                schema: "Core",
                table: "hrms_WorkforcePlanLine",
                newName: "IX_hrms_WorkforcePlanLine_OrganizationUnitId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsWorkforcePlan_TenantId_Status",
                schema: "Core",
                table: "hrms_WorkforcePlan",
                newName: "IX_hrms_WorkforcePlan_TenantId_Status");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsWorkforcePlan_StartFiscalYearId",
                schema: "Core",
                table: "hrms_WorkforcePlan",
                newName: "IX_hrms_WorkforcePlan_StartFiscalYearId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsWorkforcePlan_RootPlanId",
                schema: "Core",
                table: "hrms_WorkforcePlan",
                newName: "IX_hrms_WorkforcePlan_RootPlanId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsWorkforcePlan_OrganizationUnitId",
                schema: "Core",
                table: "hrms_WorkforcePlan",
                newName: "IX_hrms_WorkforcePlan_OrganizationUnitId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsWorkflowStepApprover_StepId",
                schema: "Core",
                table: "hrms_WorkflowStepApprover",
                newName: "IX_hrms_WorkflowStepApprover_StepId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsWorkflowStepApprover_ApproverType_ApproverId",
                schema: "Core",
                table: "hrms_WorkflowStepApprover",
                newName: "IX_hrms_WorkflowStepApprover_ApproverType_ApproverId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsWorkflowStep_DefinitionId_StepOrder",
                schema: "Core",
                table: "hrms_WorkflowStep",
                newName: "IX_hrms_WorkflowStep_DefinitionId_StepOrder");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsWorkflowInstance_Status",
                schema: "Core",
                table: "hrms_WorkflowInstance",
                newName: "IX_hrms_WorkflowInstance_Status");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsWorkflowInstance_EntityType_EntityId",
                schema: "Core",
                table: "hrms_WorkflowInstance",
                newName: "IX_hrms_WorkflowInstance_EntityType_EntityId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsWorkflowInstance_DefinitionId",
                schema: "Core",
                table: "hrms_WorkflowInstance",
                newName: "IX_hrms_WorkflowInstance_DefinitionId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsWorkflowDefinition_TenantId_EntityType",
                schema: "Core",
                table: "hrms_WorkflowDefinition",
                newName: "IX_hrms_WorkflowDefinition_TenantId_EntityType");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsWorkflowActionLog_InstanceId",
                schema: "Core",
                table: "hrms_WorkflowActionLog",
                newName: "IX_hrms_WorkflowActionLog_InstanceId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsTerminationClearance_TerminationId",
                schema: "Core",
                table: "hrms_TerminationClearance",
                newName: "IX_hrms_TerminationClearance_TerminationId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsTerminationClearance_DepartmentId",
                schema: "Core",
                table: "hrms_TerminationClearance",
                newName: "IX_hrms_TerminationClearance_DepartmentId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsReviewCycle_TenantId_Status",
                schema: "Core",
                table: "hrms_ReviewCycle",
                newName: "IX_hrms_ReviewCycle_TenantId_Status");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsReviewCycle_TenantId_Name",
                schema: "Core",
                table: "hrms_ReviewCycle",
                newName: "IX_hrms_ReviewCycle_TenantId_Name");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsReviewCycle_RatingScaleId",
                schema: "Core",
                table: "hrms_ReviewCycle",
                newName: "IX_hrms_ReviewCycle_RatingScaleId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsReviewCycle_FiscalYearId",
                schema: "Core",
                table: "hrms_ReviewCycle",
                newName: "IX_hrms_ReviewCycle_FiscalYearId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsRequisitionScreeningCriterion_RequisitionId",
                schema: "Core",
                table: "hrms_RequisitionScreeningCriterion",
                newName: "IX_hrms_RequisitionScreeningCriterion_RequisitionId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsReportScheduleRecipient_ReportScheduleId",
                schema: "Core",
                table: "hrms_ReportScheduleRecipient",
                newName: "IX_hrms_ReportScheduleRecipient_ReportScheduleId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsReportScheduleFieldValue_ReportScheduleId",
                schema: "Core",
                table: "hrms_ReportScheduleFieldValue",
                newName: "IX_hrms_ReportScheduleFieldValue_ReportScheduleId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsReportScheduleFieldOutput_ReportScheduleId",
                schema: "Core",
                table: "hrms_ReportScheduleFieldOutput",
                newName: "IX_hrms_ReportScheduleFieldOutput_ReportScheduleId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsReportSchedule_ReportId",
                schema: "Core",
                table: "hrms_ReportSchedule",
                newName: "IX_hrms_ReportSchedule_ReportId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsReportSavedFilter_ReportId",
                schema: "Core",
                table: "hrms_ReportSavedFilter",
                newName: "IX_hrms_ReportSavedFilter_ReportId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsReportRunRecipient_ReportRunId",
                schema: "Core",
                table: "hrms_ReportRunRecipient",
                newName: "IX_hrms_ReportRunRecipient_ReportRunId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsReportRun_TenantId_ReportKey",
                schema: "Core",
                table: "hrms_ReportRun",
                newName: "IX_hrms_ReportRun_TenantId_ReportKey");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsReportRestriction_RoleId",
                schema: "Core",
                table: "hrms_ReportRestriction",
                newName: "IX_hrms_ReportRestriction_RoleId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsReportRestriction_ReportId",
                schema: "Core",
                table: "hrms_ReportRestriction",
                newName: "IX_hrms_ReportRestriction_ReportId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsReportFieldOutput_ReportId",
                schema: "Core",
                table: "hrms_ReportFieldOutput",
                newName: "IX_hrms_ReportFieldOutput_ReportId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsReportField_ReportId",
                schema: "Core",
                table: "hrms_ReportField",
                newName: "IX_hrms_ReportField_ReportId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsReport_TenantId_ReportKey",
                schema: "Core",
                table: "hrms_Report",
                newName: "IX_hrms_Report_TenantId_ReportKey");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsReport_TenantId_IsActive",
                schema: "Core",
                table: "hrms_Report",
                newName: "IX_hrms_Report_TenantId_IsActive");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsRecognitionBadge_TenantId_Name",
                schema: "Core",
                table: "hrms_RecognitionBadge",
                newName: "IX_hrms_RecognitionBadge_TenantId_Name");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsRatingScaleLevel_RatingScaleId_Value",
                schema: "Core",
                table: "hrms_RatingScaleLevel",
                newName: "IX_hrms_RatingScaleLevel_RatingScaleId_Value");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsRatingScale_TenantId_Name",
                schema: "Core",
                table: "hrms_RatingScale",
                newName: "IX_hrms_RatingScale_TenantId_Name");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsPositionCompetency_PositionId_CompetencyId",
                schema: "Core",
                table: "hrms_PositionCompetency",
                newName: "IX_hrms_PositionCompetency_PositionId_CompetencyId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsPositionCompetency_CompetencyId",
                schema: "Core",
                table: "hrms_PositionCompetency",
                newName: "IX_hrms_PositionCompetency_CompetencyId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsPositionClass_WorkLocationId",
                schema: "Core",
                table: "hrms_PositionClass",
                newName: "IX_hrms_PositionClass_WorkLocationId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsPositionClass_TenantId_Code",
                schema: "Core",
                table: "hrms_PositionClass",
                newName: "IX_hrms_PositionClass_TenantId_Code");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsPositionClass_SalaryScaleId",
                schema: "Core",
                table: "hrms_PositionClass",
                newName: "IX_hrms_PositionClass_SalaryScaleId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsPositionClass_ReportsToPositionClassId",
                schema: "Core",
                table: "hrms_PositionClass",
                newName: "IX_hrms_PositionClass_ReportsToPositionClassId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsPositionClass_JobCategoryId",
                schema: "Core",
                table: "hrms_PositionClass",
                newName: "IX_hrms_PositionClass_JobCategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsPosition_TenantId_BranchId_Code",
                schema: "Core",
                table: "hrms_Position",
                newName: "IX_hrms_Position_TenantId_BranchId_Code");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsPosition_PositionClassId",
                schema: "Core",
                table: "hrms_Position",
                newName: "IX_hrms_Position_PositionClassId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsPosition_OrganizationUnitId",
                schema: "Core",
                table: "hrms_Position",
                newName: "IX_hrms_Position_OrganizationUnitId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsPosition_BranchId",
                schema: "Core",
                table: "hrms_Position",
                newName: "IX_hrms_Position_BranchId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsPipObjective_PipId_SortOrder",
                schema: "Core",
                table: "hrms_PipObjective",
                newName: "IX_hrms_PipObjective_PipId_SortOrder");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsPerformanceHistory_TenantId_EntityType_EntityId",
                schema: "Core",
                table: "hrms_PerformanceHistory",
                newName: "IX_hrms_PerformanceHistory_TenantId_EntityType_EntityId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsOrganizationUnit_WorkLocationId",
                schema: "Core",
                table: "hrms_OrganizationUnit",
                newName: "IX_hrms_OrganizationUnit_WorkLocationId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsOrganizationUnit_TenantId_BranchId_Code",
                schema: "Core",
                table: "hrms_OrganizationUnit",
                newName: "IX_hrms_OrganizationUnit_TenantId_BranchId_Code");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsOrganizationUnit_ParentId",
                schema: "Core",
                table: "hrms_OrganizationUnit",
                newName: "IX_hrms_OrganizationUnit_ParentId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsOrganizationUnit_BranchId",
                schema: "Core",
                table: "hrms_OrganizationUnit",
                newName: "IX_hrms_OrganizationUnit_BranchId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsOrganizationalObjective_TenantId_ReviewCycleId_Title",
                schema: "Core",
                table: "hrms_OrganizationalObjective",
                newName: "IX_hrms_OrganizationalObjective_TenantId_ReviewCycleId_Title");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsOrganizationalObjective_TenantId_ReviewCycleId",
                schema: "Core",
                table: "hrms_OrganizationalObjective",
                newName: "IX_hrms_OrganizationalObjective_TenantId_ReviewCycleId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsOrganizationalObjective_ReviewCycleId",
                schema: "Core",
                table: "hrms_OrganizationalObjective",
                newName: "IX_hrms_OrganizationalObjective_ReviewCycleId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsOrganizationalObjective_ParentObjectiveId",
                schema: "Core",
                table: "hrms_OrganizationalObjective",
                newName: "IX_hrms_OrganizationalObjective_ParentObjectiveId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsOrganizationalObjective_OrganizationUnitId",
                schema: "Core",
                table: "hrms_OrganizationalObjective",
                newName: "IX_hrms_OrganizationalObjective_OrganizationUnitId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsOfferLetterTemplate_TenantId",
                schema: "Core",
                table: "hrms_OfferLetterTemplate",
                newName: "IX_hrms_OfferLetterTemplate_TenantId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsLeaveType_TenantId_Code",
                schema: "Core",
                table: "hrms_LeaveType",
                newName: "IX_hrms_LeaveType_TenantId_Code");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsLeaveRequestLine_LeaveTypeId",
                schema: "Core",
                table: "hrms_LeaveRequestLine",
                newName: "IX_hrms_LeaveRequestLine_LeaveTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsLeaveRequestLine_LeaveRequestId",
                schema: "Core",
                table: "hrms_LeaveRequestLine",
                newName: "IX_hrms_LeaveRequestLine_LeaveRequestId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsLeaveRequest_FiscalYearId",
                schema: "Core",
                table: "hrms_LeaveRequest",
                newName: "IX_hrms_LeaveRequest_FiscalYearId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsLeaveRequest_EmployeeId_Status",
                schema: "Core",
                table: "hrms_LeaveRequest",
                newName: "IX_hrms_LeaveRequest_EmployeeId_Status");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsLeaveRequest_EmployeeId",
                schema: "Core",
                table: "hrms_LeaveRequest",
                newName: "IX_hrms_LeaveRequest_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsLeaveBalanceTransaction_ReferenceId",
                schema: "Core",
                table: "hrms_LeaveBalanceTransaction",
                newName: "IX_hrms_LeaveBalanceTransaction_ReferenceId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsLeaveBalanceTransaction_EmployeeId_LeaveTypeId_FiscalYearId",
                schema: "Core",
                table: "hrms_LeaveBalanceTransaction",
                newName: "IX_hrms_LeaveBalanceTransaction_EmployeeId_LeaveTypeId_FiscalYearId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsLeaveBalance_TenantId_EmployeeId_LeaveTypeId_FiscalYearId",
                schema: "Core",
                table: "hrms_LeaveBalance",
                newName: "IX_hrms_LeaveBalance_TenantId_EmployeeId_LeaveTypeId_FiscalYearId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsLeaveBalance_LeaveTypeId",
                schema: "Core",
                table: "hrms_LeaveBalance",
                newName: "IX_hrms_LeaveBalance_LeaveTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsLeaveBalance_FiscalYearId",
                schema: "Core",
                table: "hrms_LeaveBalance",
                newName: "IX_hrms_LeaveBalance_FiscalYearId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsLeaveBalance_EmployeeId",
                schema: "Core",
                table: "hrms_LeaveBalance",
                newName: "IX_hrms_LeaveBalance_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsJobRequisition_WorkLocationId",
                schema: "Core",
                table: "hrms_JobRequisition",
                newName: "IX_hrms_JobRequisition_WorkLocationId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsJobRequisition_TenantId_Status",
                schema: "Core",
                table: "hrms_JobRequisition",
                newName: "IX_hrms_JobRequisition_TenantId_Status");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsJobRequisition_TenantId_RequisitionNumber",
                schema: "Core",
                table: "hrms_JobRequisition",
                newName: "IX_hrms_JobRequisition_TenantId_RequisitionNumber");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsJobRequisition_SalaryScaleId",
                schema: "Core",
                table: "hrms_JobRequisition",
                newName: "IX_hrms_JobRequisition_SalaryScaleId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsJobRequisition_PositionClassId",
                schema: "Core",
                table: "hrms_JobRequisition",
                newName: "IX_hrms_JobRequisition_PositionClassId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsJobRequisition_OrganizationUnitId",
                schema: "Core",
                table: "hrms_JobRequisition",
                newName: "IX_hrms_JobRequisition_OrganizationUnitId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsJobRequisition_HiringRequestId",
                schema: "Core",
                table: "hrms_JobRequisition",
                newName: "IX_hrms_JobRequisition_HiringRequestId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsJobOffer_TenantId_Status",
                schema: "Core",
                table: "hrms_JobOffer",
                newName: "IX_hrms_JobOffer_TenantId_Status");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsJobOffer_TenantId_OfferNumber",
                schema: "Core",
                table: "hrms_JobOffer",
                newName: "IX_hrms_JobOffer_TenantId_OfferNumber");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsJobOffer_SalaryScaleId",
                schema: "Core",
                table: "hrms_JobOffer",
                newName: "IX_hrms_JobOffer_SalaryScaleId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsJobOffer_HiringManagerEmployeeId",
                schema: "Core",
                table: "hrms_JobOffer",
                newName: "IX_hrms_JobOffer_HiringManagerEmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsJobOffer_HiredEmployeeId",
                schema: "Core",
                table: "hrms_JobOffer",
                newName: "IX_hrms_JobOffer_HiredEmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsJobOffer_ApplicationId_CreatedAt",
                schema: "Core",
                table: "hrms_JobOffer",
                newName: "IX_hrms_JobOffer_ApplicationId_CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsJobGrade_TenantId_Code",
                schema: "Core",
                table: "hrms_JobGrade",
                newName: "IX_hrms_JobGrade_TenantId_Code");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsJobCategory_TenantId_Code",
                schema: "Core",
                table: "hrms_JobCategory",
                newName: "IX_hrms_JobCategory_TenantId_Code");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsJobApplicationStageLog_ApplicationId",
                schema: "Core",
                table: "hrms_JobApplicationStageLog",
                newName: "IX_hrms_JobApplicationStageLog_ApplicationId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsJobApplication_TenantId_Stage",
                schema: "Core",
                table: "hrms_JobApplication",
                newName: "IX_hrms_JobApplication_TenantId_Stage");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsJobApplication_TenantId_AppliedAt",
                schema: "Core",
                table: "hrms_JobApplication",
                newName: "IX_hrms_JobApplication_TenantId_AppliedAt");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsJobApplication_RequisitionId",
                schema: "Core",
                table: "hrms_JobApplication",
                newName: "IX_hrms_JobApplication_RequisitionId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsJobApplication_CandidateId_RequisitionId",
                schema: "Core",
                table: "hrms_JobApplication",
                newName: "IX_hrms_JobApplication_CandidateId_RequisitionId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsInterviewPanelist_InterviewId_EmployeeId",
                schema: "Core",
                table: "hrms_InterviewPanelist",
                newName: "IX_hrms_InterviewPanelist_InterviewId_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsInterviewPanelist_EmployeeId",
                schema: "Core",
                table: "hrms_InterviewPanelist",
                newName: "IX_hrms_InterviewPanelist_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsInterviewFeedback_PanelistId_CriterionId",
                schema: "Core",
                table: "hrms_InterviewFeedback",
                newName: "IX_hrms_InterviewFeedback_PanelistId_CriterionId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsInterviewFeedback_PanelistId",
                schema: "Core",
                table: "hrms_InterviewFeedback",
                newName: "IX_hrms_InterviewFeedback_PanelistId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsInterview_TenantId_Status",
                schema: "Core",
                table: "hrms_Interview",
                newName: "IX_hrms_Interview_TenantId_Status");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsInterview_ScheduledStart",
                schema: "Core",
                table: "hrms_Interview",
                newName: "IX_hrms_Interview_ScheduledStart");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsInterview_ApplicationId",
                schema: "Core",
                table: "hrms_Interview",
                newName: "IX_hrms_Interview_ApplicationId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsImprovementPlan_TenantId_EmployeeId",
                schema: "Core",
                table: "hrms_ImprovementPlan",
                newName: "IX_hrms_ImprovementPlan_TenantId_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsImprovementPlan_EmployeeId",
                schema: "Core",
                table: "hrms_ImprovementPlan",
                newName: "IX_hrms_ImprovementPlan_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsImprovementPlan_AppraisalId",
                schema: "Core",
                table: "hrms_ImprovementPlan",
                newName: "IX_hrms_ImprovementPlan_AppraisalId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsHoliday_TenantId_Date",
                schema: "Core",
                table: "hrms_Holiday",
                newName: "IX_hrms_Holiday_TenantId_Date");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsHiringRequest_TenantId_Status",
                schema: "Core",
                table: "hrms_HiringRequest",
                newName: "IX_hrms_HiringRequest_TenantId_Status");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsHiringRequest_TenantId_RequestNumber",
                schema: "Core",
                table: "hrms_HiringRequest",
                newName: "IX_hrms_HiringRequest_TenantId_RequestNumber");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsHiringRequest_PositionClassId",
                schema: "Core",
                table: "hrms_HiringRequest",
                newName: "IX_hrms_HiringRequest_PositionClassId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsHiringRequest_OrganizationUnitId",
                schema: "Core",
                table: "hrms_HiringRequest",
                newName: "IX_hrms_HiringRequest_OrganizationUnitId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsGoalActionItem_EmployeeGoalId_SortOrder",
                schema: "Core",
                table: "hrms_GoalActionItem",
                newName: "IX_hrms_GoalActionItem_EmployeeGoalId_SortOrder");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsEmployeeTermination_Status",
                schema: "Core",
                table: "hrms_EmployeeTermination",
                newName: "IX_hrms_EmployeeTermination_Status");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsEmployeeTermination_EmployeeId",
                schema: "Core",
                table: "hrms_EmployeeTermination",
                newName: "IX_hrms_EmployeeTermination_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsEmployeeRecognition_TenantId_IsPublic",
                schema: "Core",
                table: "hrms_EmployeeRecognition",
                newName: "IX_hrms_EmployeeRecognition_TenantId_IsPublic");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsEmployeeRecognition_TenantId_EmployeeId",
                schema: "Core",
                table: "hrms_EmployeeRecognition",
                newName: "IX_hrms_EmployeeRecognition_TenantId_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsEmployeeRecognition_RecognitionBadgeId",
                schema: "Core",
                table: "hrms_EmployeeRecognition",
                newName: "IX_hrms_EmployeeRecognition_RecognitionBadgeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsEmployeeRecognition_EmployeeId",
                schema: "Core",
                table: "hrms_EmployeeRecognition",
                newName: "IX_hrms_EmployeeRecognition_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsEmployeeMovement_ToSalaryScaleId",
                schema: "Core",
                table: "hrms_EmployeeMovement",
                newName: "IX_hrms_EmployeeMovement_ToSalaryScaleId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsEmployeeMovement_Status_EffectiveDate",
                schema: "Core",
                table: "hrms_EmployeeMovement",
                newName: "IX_hrms_EmployeeMovement_Status_EffectiveDate");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsEmployeeMovement_EmployeeId",
                schema: "Core",
                table: "hrms_EmployeeMovement",
                newName: "IX_hrms_EmployeeMovement_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsEmployeeGoal_TenantId_EmployeeId_ReviewCycleId",
                schema: "Core",
                table: "hrms_EmployeeGoal",
                newName: "IX_hrms_EmployeeGoal_TenantId_EmployeeId_ReviewCycleId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsEmployeeGoal_ReviewCycleId",
                schema: "Core",
                table: "hrms_EmployeeGoal",
                newName: "IX_hrms_EmployeeGoal_ReviewCycleId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsEmployeeGoal_OrganizationalObjectiveId",
                schema: "Core",
                table: "hrms_EmployeeGoal",
                newName: "IX_hrms_EmployeeGoal_OrganizationalObjectiveId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsEmployeeGoal_EmployeeId",
                schema: "Core",
                table: "hrms_EmployeeGoal",
                newName: "IX_hrms_EmployeeGoal_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsEmployeeFieldValue_OwnerType_OwnerId_FieldDefinitionId",
                schema: "Core",
                table: "hrms_EmployeeFieldValue",
                newName: "IX_hrms_EmployeeFieldValue_OwnerType_OwnerId_FieldDefinitionId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsEmployeeFieldValue_FieldDefinitionId",
                schema: "Core",
                table: "hrms_EmployeeFieldValue",
                newName: "IX_hrms_EmployeeFieldValue_FieldDefinitionId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsEmployeeFieldDefinition_TenantId_OwnerType_Name",
                schema: "Core",
                table: "hrms_EmployeeFieldDefinition",
                newName: "IX_hrms_EmployeeFieldDefinition_TenantId_OwnerType_Name");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsEmployeeExperience_PersonId",
                schema: "Core",
                table: "hrms_EmployeeExperience",
                newName: "IX_hrms_EmployeeExperience_PersonId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsEmployeeEducation_PersonId",
                schema: "Core",
                table: "hrms_EmployeeEducation",
                newName: "IX_hrms_EmployeeEducation_PersonId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsEmployeeDocument_OwnerType_OwnerId",
                schema: "Core",
                table: "hrms_EmployeeDocument",
                newName: "IX_hrms_EmployeeDocument_OwnerType_OwnerId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsEmployeeDocument_EmployeeId",
                schema: "Core",
                table: "hrms_EmployeeDocument",
                newName: "IX_hrms_EmployeeDocument_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsEmployeeDependent_RelatedEmployeeId",
                schema: "Core",
                table: "hrms_EmployeeDependent",
                newName: "IX_hrms_EmployeeDependent_RelatedEmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsEmployeeDependent_PersonId",
                schema: "Core",
                table: "hrms_EmployeeDependent",
                newName: "IX_hrms_EmployeeDependent_PersonId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsEmployee_TenantId_PositionId_EmployeeNumber",
                schema: "Core",
                table: "hrms_Employee",
                newName: "IX_hrms_Employee_TenantId_PositionId_EmployeeNumber");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsEmployee_TenantId_EmployeeNumber",
                schema: "Core",
                table: "hrms_Employee",
                newName: "IX_hrms_Employee_TenantId_EmployeeNumber");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsEmployee_SalaryScaleId",
                schema: "Core",
                table: "hrms_Employee",
                newName: "IX_hrms_Employee_SalaryScaleId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsEmployee_PositionId",
                schema: "Core",
                table: "hrms_Employee",
                newName: "IX_hrms_Employee_PositionId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsEmployee_PersonId",
                schema: "Core",
                table: "hrms_Employee",
                newName: "IX_hrms_Employee_PersonId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsEmployee_EmploymentStatus_IsProbation",
                schema: "Core",
                table: "hrms_Employee",
                newName: "IX_hrms_Employee_EmploymentStatus_IsProbation");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsEmployee_EmploymentStatus",
                schema: "Core",
                table: "hrms_Employee",
                newName: "IX_hrms_Employee_EmploymentStatus");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsEmployee_DateOfBirth",
                schema: "Core",
                table: "hrms_Employee",
                newName: "IX_hrms_Employee_DateOfBirth");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsEmployee_BranchId",
                schema: "Core",
                table: "hrms_Employee",
                newName: "IX_hrms_Employee_BranchId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsDynamicFormRecord_DynamicFormId_OwnerType_OwnerId_CreatedAt",
                schema: "Core",
                table: "hrms_DynamicFormRecord",
                newName: "IX_hrms_DynamicFormRecord_DynamicFormId_OwnerType_OwnerId_CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsDynamicFormField_DynamicFormId_Name",
                schema: "Core",
                table: "hrms_DynamicFormField",
                newName: "IX_hrms_DynamicFormField_DynamicFormId_Name");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsDynamicForm_TenantId_Module_Name",
                schema: "Core",
                table: "hrms_DynamicForm",
                newName: "IX_hrms_DynamicForm_TenantId_Module_Name");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsDocumentTemplate_TenantId_Name",
                schema: "Core",
                table: "hrms_DocumentTemplate",
                newName: "IX_hrms_DocumentTemplate_TenantId_Name");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsDisciplinaryMeasure_Status",
                schema: "Core",
                table: "hrms_DisciplinaryMeasure",
                newName: "IX_hrms_DisciplinaryMeasure_Status");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsDisciplinaryMeasure_EmployeeId",
                schema: "Core",
                table: "hrms_DisciplinaryMeasure",
                newName: "IX_hrms_DisciplinaryMeasure_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsDevelopmentPlan_TenantId_EmployeeId",
                schema: "Core",
                table: "hrms_DevelopmentPlan",
                newName: "IX_hrms_DevelopmentPlan_TenantId_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsDevelopmentPlan_EmployeeId",
                schema: "Core",
                table: "hrms_DevelopmentPlan",
                newName: "IX_hrms_DevelopmentPlan_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsDevelopmentPlan_AppraisalId",
                schema: "Core",
                table: "hrms_DevelopmentPlan",
                newName: "IX_hrms_DevelopmentPlan_AppraisalId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsDevelopmentAction_DevelopmentPlanId_SortOrder",
                schema: "Core",
                table: "hrms_DevelopmentAction",
                newName: "IX_hrms_DevelopmentAction_DevelopmentPlanId_SortOrder");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsDevelopmentAction_CompetencyId",
                schema: "Core",
                table: "hrms_DevelopmentAction",
                newName: "IX_hrms_DevelopmentAction_CompetencyId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsCriterionEvaluator_EmployeeId",
                schema: "Core",
                table: "hrms_CriterionEvaluator",
                newName: "IX_hrms_CriterionEvaluator_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsCriterionEvaluator_CriterionId",
                schema: "Core",
                table: "hrms_CriterionEvaluator",
                newName: "IX_hrms_CriterionEvaluator_CriterionId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsCompetencyCategory_TenantId_Name",
                schema: "Core",
                table: "hrms_CompetencyCategory",
                newName: "IX_hrms_CompetencyCategory_TenantId_Name");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsCompetency_TenantId_Name",
                schema: "Core",
                table: "hrms_Competency",
                newName: "IX_hrms_Competency_TenantId_Name");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsCompetency_CompetencyCategoryId",
                schema: "Core",
                table: "hrms_Competency",
                newName: "IX_hrms_Competency_CompetencyCategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsCompanyProfile_TenantId",
                schema: "Core",
                table: "hrms_CompanyProfile",
                newName: "IX_hrms_CompanyProfile_TenantId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsClearanceDepartmentApprover_DepartmentId",
                schema: "Core",
                table: "hrms_ClearanceDepartmentApprover",
                newName: "IX_hrms_ClearanceDepartmentApprover_DepartmentId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsClearanceDepartmentApprover_ApproverType_ApproverId",
                schema: "Core",
                table: "hrms_ClearanceDepartmentApprover",
                newName: "IX_hrms_ClearanceDepartmentApprover_ApproverType_ApproverId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsClearanceDepartment_TenantId_Name",
                schema: "Core",
                table: "hrms_ClearanceDepartment",
                newName: "IX_hrms_ClearanceDepartment_TenantId_Name");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsCandidateDocument_CandidateId_DocumentType",
                schema: "Core",
                table: "hrms_CandidateDocument",
                newName: "IX_hrms_CandidateDocument_CandidateId_DocumentType");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsCandidate_TenantId_IsInTalentPool",
                schema: "Core",
                table: "hrms_Candidate",
                newName: "IX_hrms_Candidate_TenantId_IsInTalentPool");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsCandidate_TenantId_CandidateNumber",
                schema: "Core",
                table: "hrms_Candidate",
                newName: "IX_hrms_Candidate_TenantId_CandidateNumber");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsCandidate_PersonId",
                schema: "Core",
                table: "hrms_Candidate",
                newName: "IX_hrms_Candidate_PersonId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsCandidate_InternalEmployeeId",
                schema: "Core",
                table: "hrms_Candidate",
                newName: "IX_hrms_Candidate_InternalEmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsCandidate_HiredEmployeeId",
                schema: "Core",
                table: "hrms_Candidate",
                newName: "IX_hrms_Candidate_HiredEmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsCandidate_Email",
                schema: "Core",
                table: "hrms_Candidate",
                newName: "IX_hrms_Candidate_Email");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsCalibrationSession_TenantId_ReviewCycleId",
                schema: "Core",
                table: "hrms_CalibrationSession",
                newName: "IX_hrms_CalibrationSession_TenantId_ReviewCycleId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsCalibrationSession_ReviewCycleId",
                schema: "Core",
                table: "hrms_CalibrationSession",
                newName: "IX_hrms_CalibrationSession_ReviewCycleId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsCalibrationSession_OrganizationUnitId",
                schema: "Core",
                table: "hrms_CalibrationSession",
                newName: "IX_hrms_CalibrationSession_OrganizationUnitId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsCalibrationItem_CalibrationSessionId",
                schema: "Core",
                table: "hrms_CalibrationItem",
                newName: "IX_hrms_CalibrationItem_CalibrationSessionId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsCalibrationItem_AppraisalId",
                schema: "Core",
                table: "hrms_CalibrationItem",
                newName: "IX_hrms_CalibrationItem_AppraisalId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsBranch_TenantId_Code",
                schema: "Core",
                table: "hrms_Branch",
                newName: "IX_hrms_Branch_TenantId_Code");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsBranch_ParentId",
                schema: "Core",
                table: "hrms_Branch",
                newName: "IX_hrms_Branch_ParentId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsAuditLog_EntityType_EntityId",
                schema: "Core",
                table: "hrms_AuditLog",
                newName: "IX_hrms_AuditLog_EntityType_EntityId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsAuditLog_CreatedAt",
                schema: "Core",
                table: "hrms_AuditLog",
                newName: "IX_hrms_AuditLog_CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsAuditLog_BranchId",
                schema: "Core",
                table: "hrms_AuditLog",
                newName: "IX_hrms_AuditLog_BranchId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsAuditLog_Action",
                schema: "Core",
                table: "hrms_AuditLog",
                newName: "IX_hrms_AuditLog_Action");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsAppraisalTemplate_TenantId_Name",
                schema: "Core",
                table: "hrms_AppraisalTemplate",
                newName: "IX_hrms_AppraisalTemplate_TenantId_Name");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsAppraisalPeerReview_PeerEmployeeId",
                schema: "Core",
                table: "hrms_AppraisalPeerReview",
                newName: "IX_hrms_AppraisalPeerReview_PeerEmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsAppraisalPeerReview_AppraisalId_PeerEmployeeId",
                schema: "Core",
                table: "hrms_AppraisalPeerReview",
                newName: "IX_hrms_AppraisalPeerReview_AppraisalId_PeerEmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsAppraisalGoal_AppraisalId_SortOrder",
                schema: "Core",
                table: "hrms_AppraisalGoal",
                newName: "IX_hrms_AppraisalGoal_AppraisalId_SortOrder");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsAppraisalCompetency_AppraisalId_SortOrder",
                schema: "Core",
                table: "hrms_AppraisalCompetency",
                newName: "IX_hrms_AppraisalCompetency_AppraisalId_SortOrder");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsAppraisalAppeal_TenantId_Status",
                schema: "Core",
                table: "hrms_AppraisalAppeal",
                newName: "IX_hrms_AppraisalAppeal_TenantId_Status");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsAppraisalAppeal_EmployeeId",
                schema: "Core",
                table: "hrms_AppraisalAppeal",
                newName: "IX_hrms_AppraisalAppeal_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsAppraisalAppeal_AppraisalId",
                schema: "Core",
                table: "hrms_AppraisalAppeal",
                newName: "IX_hrms_AppraisalAppeal_AppraisalId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsAppraisal_TenantId_ReviewCycleId_Stage",
                schema: "Core",
                table: "hrms_Appraisal",
                newName: "IX_hrms_Appraisal_TenantId_ReviewCycleId_Stage");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsAppraisal_TenantId_EmployeeId_ReviewCycleId",
                schema: "Core",
                table: "hrms_Appraisal",
                newName: "IX_hrms_Appraisal_TenantId_EmployeeId_ReviewCycleId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsAppraisal_ReviewCycleId",
                schema: "Core",
                table: "hrms_Appraisal",
                newName: "IX_hrms_Appraisal_ReviewCycleId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsAppraisal_EmployeeId",
                schema: "Core",
                table: "hrms_Appraisal",
                newName: "IX_hrms_Appraisal_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsApplicationCriterionScore_ApplicationId_CriterionId",
                schema: "Core",
                table: "hrms_ApplicationCriterionScore",
                newName: "IX_hrms_ApplicationCriterionScore_ApplicationId_CriterionId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsAnnualLeaveSetting_TenantId_FiscalYearId_LeaveTypeId",
                schema: "Core",
                table: "hrms_AnnualLeaveSetting",
                newName: "IX_hrms_AnnualLeaveSetting_TenantId_FiscalYearId_LeaveTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsAnnualLeaveSetting_LeaveTypeId",
                schema: "Core",
                table: "hrms_AnnualLeaveSetting",
                newName: "IX_hrms_AnnualLeaveSetting_LeaveTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsAnnualLeaveSetting_FiscalYearId",
                schema: "Core",
                table: "hrms_AnnualLeaveSetting",
                newName: "IX_hrms_AnnualLeaveSetting_FiscalYearId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsAnnualLeaveHeader_EmployeeId_Status",
                schema: "Core",
                table: "hrms_AnnualLeaveHeader",
                newName: "IX_hrms_AnnualLeaveHeader_EmployeeId_Status");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsAnnualLeaveHeader_EmployeeId",
                schema: "Core",
                table: "hrms_AnnualLeaveHeader",
                newName: "IX_hrms_AnnualLeaveHeader_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsAnnualLeaveHeader_AnnualLeaveLedgerId",
                schema: "Core",
                table: "hrms_AnnualLeaveHeader",
                newName: "IX_hrms_AnnualLeaveHeader_AnnualLeaveLedgerId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsAnnualLeaveDetail_AnnualLeaveHeaderId_StartDate_EndDate",
                schema: "Core",
                table: "hrms_AnnualLeaveDetail",
                newName: "IX_hrms_AnnualLeaveDetail_AnnualLeaveHeaderId_StartDate_EndDate");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsAnnualLeaveDetail_AnnualLeaveHeaderId",
                schema: "Core",
                table: "hrms_AnnualLeaveDetail",
                newName: "IX_hrms_AnnualLeaveDetail_AnnualLeaveHeaderId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsAchievement_TenantId_EmployeeId",
                schema: "Core",
                table: "hrms_Achievement",
                newName: "IX_hrms_Achievement_TenantId_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsAchievement_EmployeeId",
                schema: "Core",
                table: "hrms_Achievement",
                newName: "IX_hrms_Achievement_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_hrmsAchievement_AppraisalId",
                schema: "Core",
                table: "hrms_Achievement",
                newName: "IX_hrms_Achievement_AppraisalId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrms_WorkWeekConfiguration",
                schema: "Core",
                table: "hrms_WorkWeekConfiguration",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrms_WorkLocation",
                schema: "Core",
                table: "hrms_WorkLocation",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrms_WorkforcePlanLine",
                schema: "Core",
                table: "hrms_WorkforcePlanLine",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrms_WorkforcePlan",
                schema: "Core",
                table: "hrms_WorkforcePlan",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrms_WorkflowStepApprover",
                schema: "Core",
                table: "hrms_WorkflowStepApprover",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrms_WorkflowStep",
                schema: "Core",
                table: "hrms_WorkflowStep",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrms_WorkflowInstance",
                schema: "Core",
                table: "hrms_WorkflowInstance",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrms_WorkflowDefinition",
                schema: "Core",
                table: "hrms_WorkflowDefinition",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrms_WorkflowActionLog",
                schema: "Core",
                table: "hrms_WorkflowActionLog",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrms_TerminationClearance",
                schema: "Core",
                table: "hrms_TerminationClearance",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrms_ReviewCycle",
                schema: "Core",
                table: "hrms_ReviewCycle",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrms_RequisitionScreeningCriterion",
                schema: "Core",
                table: "hrms_RequisitionScreeningCriterion",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrms_ReportScheduleRecipient",
                schema: "Core",
                table: "hrms_ReportScheduleRecipient",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrms_ReportScheduleFieldValue",
                schema: "Core",
                table: "hrms_ReportScheduleFieldValue",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrms_ReportScheduleFieldOutput",
                schema: "Core",
                table: "hrms_ReportScheduleFieldOutput",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrms_ReportSchedule",
                schema: "Core",
                table: "hrms_ReportSchedule",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrms_ReportSavedFilter",
                schema: "Core",
                table: "hrms_ReportSavedFilter",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrms_ReportRunRecipient",
                schema: "Core",
                table: "hrms_ReportRunRecipient",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrms_ReportRun",
                schema: "Core",
                table: "hrms_ReportRun",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrms_ReportRestriction",
                schema: "Core",
                table: "hrms_ReportRestriction",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrms_ReportFieldOutput",
                schema: "Core",
                table: "hrms_ReportFieldOutput",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrms_ReportField",
                schema: "Core",
                table: "hrms_ReportField",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrms_Report",
                schema: "Core",
                table: "hrms_Report",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrms_RecognitionBadge",
                schema: "Core",
                table: "hrms_RecognitionBadge",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrms_RatingScaleLevel",
                schema: "Core",
                table: "hrms_RatingScaleLevel",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrms_RatingScale",
                schema: "Core",
                table: "hrms_RatingScale",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrms_PositionCompetency",
                schema: "Core",
                table: "hrms_PositionCompetency",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrms_PositionClass",
                schema: "Core",
                table: "hrms_PositionClass",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrms_Position",
                schema: "Core",
                table: "hrms_Position",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrms_PipObjective",
                schema: "Core",
                table: "hrms_PipObjective",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrms_PerformanceHistory",
                schema: "Core",
                table: "hrms_PerformanceHistory",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrms_OrganizationUnit",
                schema: "Core",
                table: "hrms_OrganizationUnit",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrms_OrganizationalObjective",
                schema: "Core",
                table: "hrms_OrganizationalObjective",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrms_OfferLetterTemplate",
                schema: "Core",
                table: "hrms_OfferLetterTemplate",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrms_NumberSequence",
                schema: "Core",
                table: "hrms_NumberSequence",
                columns: new[] { "TenantId", "Key" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrms_LeaveType",
                schema: "Core",
                table: "hrms_LeaveType",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrms_LeaveRequestLine",
                schema: "Core",
                table: "hrms_LeaveRequestLine",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrms_LeaveRequest",
                schema: "Core",
                table: "hrms_LeaveRequest",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrms_LeaveBalanceTransaction",
                schema: "Core",
                table: "hrms_LeaveBalanceTransaction",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrms_LeaveBalance",
                schema: "Core",
                table: "hrms_LeaveBalance",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrms_JobRequisition",
                schema: "Core",
                table: "hrms_JobRequisition",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrms_JobOffer",
                schema: "Core",
                table: "hrms_JobOffer",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrms_JobGrade",
                schema: "Core",
                table: "hrms_JobGrade",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrms_JobCategory",
                schema: "Core",
                table: "hrms_JobCategory",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrms_JobApplicationStageLog",
                schema: "Core",
                table: "hrms_JobApplicationStageLog",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrms_JobApplication",
                schema: "Core",
                table: "hrms_JobApplication",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrms_InterviewPanelist",
                schema: "Core",
                table: "hrms_InterviewPanelist",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrms_InterviewFeedback",
                schema: "Core",
                table: "hrms_InterviewFeedback",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrms_Interview",
                schema: "Core",
                table: "hrms_Interview",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrms_ImprovementPlan",
                schema: "Core",
                table: "hrms_ImprovementPlan",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrms_Holiday",
                schema: "Core",
                table: "hrms_Holiday",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrms_HiringRequest",
                schema: "Core",
                table: "hrms_HiringRequest",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrms_GoalActionItem",
                schema: "Core",
                table: "hrms_GoalActionItem",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrms_EmployeeTermination",
                schema: "Core",
                table: "hrms_EmployeeTermination",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrms_EmployeeRecognition",
                schema: "Core",
                table: "hrms_EmployeeRecognition",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrms_EmployeeMovement",
                schema: "Core",
                table: "hrms_EmployeeMovement",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrms_EmployeeGoal",
                schema: "Core",
                table: "hrms_EmployeeGoal",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrms_EmployeeFieldValue",
                schema: "Core",
                table: "hrms_EmployeeFieldValue",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrms_EmployeeFieldDefinition",
                schema: "Core",
                table: "hrms_EmployeeFieldDefinition",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrms_EmployeeExperience",
                schema: "Core",
                table: "hrms_EmployeeExperience",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrms_EmployeeEducation",
                schema: "Core",
                table: "hrms_EmployeeEducation",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrms_EmployeeDocument",
                schema: "Core",
                table: "hrms_EmployeeDocument",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrms_EmployeeDependent",
                schema: "Core",
                table: "hrms_EmployeeDependent",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrms_Employee",
                schema: "Core",
                table: "hrms_Employee",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrms_DynamicFormRecord",
                schema: "Core",
                table: "hrms_DynamicFormRecord",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrms_DynamicFormField",
                schema: "Core",
                table: "hrms_DynamicFormField",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrms_DynamicForm",
                schema: "Core",
                table: "hrms_DynamicForm",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrms_DocumentTemplate",
                schema: "Core",
                table: "hrms_DocumentTemplate",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrms_DisciplinaryMeasure",
                schema: "Core",
                table: "hrms_DisciplinaryMeasure",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrms_DevelopmentPlan",
                schema: "Core",
                table: "hrms_DevelopmentPlan",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrms_DevelopmentAction",
                schema: "Core",
                table: "hrms_DevelopmentAction",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrms_CriterionEvaluator",
                schema: "Core",
                table: "hrms_CriterionEvaluator",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrms_CompetencyCategory",
                schema: "Core",
                table: "hrms_CompetencyCategory",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrms_Competency",
                schema: "Core",
                table: "hrms_Competency",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrms_CompanyProfile",
                schema: "Core",
                table: "hrms_CompanyProfile",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrms_ClearanceDepartmentApprover",
                schema: "Core",
                table: "hrms_ClearanceDepartmentApprover",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrms_ClearanceDepartment",
                schema: "Core",
                table: "hrms_ClearanceDepartment",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrms_CandidateDocument",
                schema: "Core",
                table: "hrms_CandidateDocument",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrms_Candidate",
                schema: "Core",
                table: "hrms_Candidate",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrms_CalibrationSession",
                schema: "Core",
                table: "hrms_CalibrationSession",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrms_CalibrationItem",
                schema: "Core",
                table: "hrms_CalibrationItem",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrms_Branch",
                schema: "Core",
                table: "hrms_Branch",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrms_AuditLog",
                schema: "Core",
                table: "hrms_AuditLog",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrms_AppraisalTemplate",
                schema: "Core",
                table: "hrms_AppraisalTemplate",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrms_AppraisalPeerReview",
                schema: "Core",
                table: "hrms_AppraisalPeerReview",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrms_AppraisalGoal",
                schema: "Core",
                table: "hrms_AppraisalGoal",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrms_AppraisalCompetency",
                schema: "Core",
                table: "hrms_AppraisalCompetency",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrms_AppraisalAppeal",
                schema: "Core",
                table: "hrms_AppraisalAppeal",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrms_Appraisal",
                schema: "Core",
                table: "hrms_Appraisal",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrms_ApplicationCriterionScore",
                schema: "Core",
                table: "hrms_ApplicationCriterionScore",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrms_AnnualLeaveSetting",
                schema: "Core",
                table: "hrms_AnnualLeaveSetting",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrms_AnnualLeaveHeader",
                schema: "Core",
                table: "hrms_AnnualLeaveHeader",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrms_AnnualLeaveDetail",
                schema: "Core",
                table: "hrms_AnnualLeaveDetail",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_hrms_Achievement",
                schema: "Core",
                table: "hrms_Achievement",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_coreSalaryScale_hrms_JobGrade_JobGradeId",
                schema: "Core",
                table: "coreSalaryScale",
                column: "JobGradeId",
                principalSchema: "Core",
                principalTable: "hrms_JobGrade",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_Achievement_hrms_Appraisal_AppraisalId",
                schema: "Core",
                table: "hrms_Achievement",
                column: "AppraisalId",
                principalSchema: "Core",
                principalTable: "hrms_Appraisal",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_Achievement_hrms_Employee_EmployeeId",
                schema: "Core",
                table: "hrms_Achievement",
                column: "EmployeeId",
                principalSchema: "Core",
                principalTable: "hrms_Employee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_AnnualLeaveDetail_hrms_AnnualLeaveHeader_AnnualLeaveHeaderId",
                schema: "Core",
                table: "hrms_AnnualLeaveDetail",
                column: "AnnualLeaveHeaderId",
                principalSchema: "Core",
                principalTable: "hrms_AnnualLeaveHeader",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_AnnualLeaveHeader_hrms_Employee_EmployeeId",
                schema: "Core",
                table: "hrms_AnnualLeaveHeader",
                column: "EmployeeId",
                principalSchema: "Core",
                principalTable: "hrms_Employee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_AnnualLeaveHeader_hrms_LeaveBalance_AnnualLeaveLedgerId",
                schema: "Core",
                table: "hrms_AnnualLeaveHeader",
                column: "AnnualLeaveLedgerId",
                principalSchema: "Core",
                principalTable: "hrms_LeaveBalance",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_AnnualLeaveSetting_FiscalYear_FiscalYearId",
                schema: "Core",
                table: "hrms_AnnualLeaveSetting",
                column: "FiscalYearId",
                principalSchema: "Core",
                principalTable: "FiscalYear",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_AnnualLeaveSetting_hrms_LeaveType_LeaveTypeId",
                schema: "Core",
                table: "hrms_AnnualLeaveSetting",
                column: "LeaveTypeId",
                principalSchema: "Core",
                principalTable: "hrms_LeaveType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_ApplicationCriterionScore_hrms_JobApplication_ApplicationId",
                schema: "Core",
                table: "hrms_ApplicationCriterionScore",
                column: "ApplicationId",
                principalSchema: "Core",
                principalTable: "hrms_JobApplication",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_Appraisal_hrms_Employee_EmployeeId",
                schema: "Core",
                table: "hrms_Appraisal",
                column: "EmployeeId",
                principalSchema: "Core",
                principalTable: "hrms_Employee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_Appraisal_hrms_ReviewCycle_ReviewCycleId",
                schema: "Core",
                table: "hrms_Appraisal",
                column: "ReviewCycleId",
                principalSchema: "Core",
                principalTable: "hrms_ReviewCycle",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_AppraisalAppeal_hrms_Appraisal_AppraisalId",
                schema: "Core",
                table: "hrms_AppraisalAppeal",
                column: "AppraisalId",
                principalSchema: "Core",
                principalTable: "hrms_Appraisal",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_AppraisalAppeal_hrms_Employee_EmployeeId",
                schema: "Core",
                table: "hrms_AppraisalAppeal",
                column: "EmployeeId",
                principalSchema: "Core",
                principalTable: "hrms_Employee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_AppraisalCompetency_hrms_Appraisal_AppraisalId",
                schema: "Core",
                table: "hrms_AppraisalCompetency",
                column: "AppraisalId",
                principalSchema: "Core",
                principalTable: "hrms_Appraisal",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_AppraisalGoal_hrms_Appraisal_AppraisalId",
                schema: "Core",
                table: "hrms_AppraisalGoal",
                column: "AppraisalId",
                principalSchema: "Core",
                principalTable: "hrms_Appraisal",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_AppraisalPeerReview_hrms_Appraisal_AppraisalId",
                schema: "Core",
                table: "hrms_AppraisalPeerReview",
                column: "AppraisalId",
                principalSchema: "Core",
                principalTable: "hrms_Appraisal",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_AppraisalPeerReview_hrms_Employee_PeerEmployeeId",
                schema: "Core",
                table: "hrms_AppraisalPeerReview",
                column: "PeerEmployeeId",
                principalSchema: "Core",
                principalTable: "hrms_Employee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_Branch_hrms_Branch_ParentId",
                schema: "Core",
                table: "hrms_Branch",
                column: "ParentId",
                principalSchema: "Core",
                principalTable: "hrms_Branch",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_CalibrationItem_hrms_Appraisal_AppraisalId",
                schema: "Core",
                table: "hrms_CalibrationItem",
                column: "AppraisalId",
                principalSchema: "Core",
                principalTable: "hrms_Appraisal",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_CalibrationItem_hrms_CalibrationSession_CalibrationSessionId",
                schema: "Core",
                table: "hrms_CalibrationItem",
                column: "CalibrationSessionId",
                principalSchema: "Core",
                principalTable: "hrms_CalibrationSession",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_CalibrationSession_hrms_OrganizationUnit_OrganizationUnitId",
                schema: "Core",
                table: "hrms_CalibrationSession",
                column: "OrganizationUnitId",
                principalSchema: "Core",
                principalTable: "hrms_OrganizationUnit",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_CalibrationSession_hrms_ReviewCycle_ReviewCycleId",
                schema: "Core",
                table: "hrms_CalibrationSession",
                column: "ReviewCycleId",
                principalSchema: "Core",
                principalTable: "hrms_ReviewCycle",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_Candidate_CorePerson_PersonId",
                schema: "Core",
                table: "hrms_Candidate",
                column: "PersonId",
                principalSchema: "Core",
                principalTable: "CorePerson",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_Candidate_hrms_Employee_InternalEmployeeId",
                schema: "Core",
                table: "hrms_Candidate",
                column: "InternalEmployeeId",
                principalSchema: "Core",
                principalTable: "hrms_Employee",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_CandidateDocument_hrms_Candidate_CandidateId",
                schema: "Core",
                table: "hrms_CandidateDocument",
                column: "CandidateId",
                principalSchema: "Core",
                principalTable: "hrms_Candidate",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_ClearanceDepartmentApprover_hrms_ClearanceDepartment_DepartmentId",
                schema: "Core",
                table: "hrms_ClearanceDepartmentApprover",
                column: "DepartmentId",
                principalSchema: "Core",
                principalTable: "hrms_ClearanceDepartment",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_Competency_hrms_CompetencyCategory_CompetencyCategoryId",
                schema: "Core",
                table: "hrms_Competency",
                column: "CompetencyCategoryId",
                principalSchema: "Core",
                principalTable: "hrms_CompetencyCategory",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_CriterionEvaluator_hrms_Employee_EmployeeId",
                schema: "Core",
                table: "hrms_CriterionEvaluator",
                column: "EmployeeId",
                principalSchema: "Core",
                principalTable: "hrms_Employee",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_CriterionEvaluator_hrms_RequisitionScreeningCriterion_CriterionId",
                schema: "Core",
                table: "hrms_CriterionEvaluator",
                column: "CriterionId",
                principalSchema: "Core",
                principalTable: "hrms_RequisitionScreeningCriterion",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_DevelopmentAction_hrms_Competency_CompetencyId",
                schema: "Core",
                table: "hrms_DevelopmentAction",
                column: "CompetencyId",
                principalSchema: "Core",
                principalTable: "hrms_Competency",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_DevelopmentAction_hrms_DevelopmentPlan_DevelopmentPlanId",
                schema: "Core",
                table: "hrms_DevelopmentAction",
                column: "DevelopmentPlanId",
                principalSchema: "Core",
                principalTable: "hrms_DevelopmentPlan",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_DevelopmentPlan_hrms_Appraisal_AppraisalId",
                schema: "Core",
                table: "hrms_DevelopmentPlan",
                column: "AppraisalId",
                principalSchema: "Core",
                principalTable: "hrms_Appraisal",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_DevelopmentPlan_hrms_Employee_EmployeeId",
                schema: "Core",
                table: "hrms_DevelopmentPlan",
                column: "EmployeeId",
                principalSchema: "Core",
                principalTable: "hrms_Employee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_DisciplinaryMeasure_hrms_Employee_EmployeeId",
                schema: "Core",
                table: "hrms_DisciplinaryMeasure",
                column: "EmployeeId",
                principalSchema: "Core",
                principalTable: "hrms_Employee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_DynamicFormField_hrms_DynamicForm_DynamicFormId",
                schema: "Core",
                table: "hrms_DynamicFormField",
                column: "DynamicFormId",
                principalSchema: "Core",
                principalTable: "hrms_DynamicForm",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_DynamicFormRecord_hrms_DynamicForm_DynamicFormId",
                schema: "Core",
                table: "hrms_DynamicFormRecord",
                column: "DynamicFormId",
                principalSchema: "Core",
                principalTable: "hrms_DynamicForm",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_Employee_CorePerson_PersonId",
                schema: "Core",
                table: "hrms_Employee",
                column: "PersonId",
                principalSchema: "Core",
                principalTable: "CorePerson",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_Employee_coreSalaryScale_SalaryScaleId",
                schema: "Core",
                table: "hrms_Employee",
                column: "SalaryScaleId",
                principalSchema: "Core",
                principalTable: "coreSalaryScale",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_Employee_hrms_Branch_BranchId",
                schema: "Core",
                table: "hrms_Employee",
                column: "BranchId",
                principalSchema: "Core",
                principalTable: "hrms_Branch",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_Employee_hrms_Position_PositionId",
                schema: "Core",
                table: "hrms_Employee",
                column: "PositionId",
                principalSchema: "Core",
                principalTable: "hrms_Position",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_EmployeeDependent_CorePerson_PersonId",
                schema: "Core",
                table: "hrms_EmployeeDependent",
                column: "PersonId",
                principalSchema: "Core",
                principalTable: "CorePerson",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_EmployeeDependent_hrms_Employee_RelatedEmployeeId",
                schema: "Core",
                table: "hrms_EmployeeDependent",
                column: "RelatedEmployeeId",
                principalSchema: "Core",
                principalTable: "hrms_Employee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_EmployeeEducation_CorePerson_PersonId",
                schema: "Core",
                table: "hrms_EmployeeEducation",
                column: "PersonId",
                principalSchema: "Core",
                principalTable: "CorePerson",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_EmployeeExperience_CorePerson_PersonId",
                schema: "Core",
                table: "hrms_EmployeeExperience",
                column: "PersonId",
                principalSchema: "Core",
                principalTable: "CorePerson",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_EmployeeFieldValue_hrms_EmployeeFieldDefinition_FieldDefinitionId",
                schema: "Core",
                table: "hrms_EmployeeFieldValue",
                column: "FieldDefinitionId",
                principalSchema: "Core",
                principalTable: "hrms_EmployeeFieldDefinition",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_EmployeeGoal_hrms_Employee_EmployeeId",
                schema: "Core",
                table: "hrms_EmployeeGoal",
                column: "EmployeeId",
                principalSchema: "Core",
                principalTable: "hrms_Employee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_EmployeeGoal_hrms_OrganizationalObjective_OrganizationalObjectiveId",
                schema: "Core",
                table: "hrms_EmployeeGoal",
                column: "OrganizationalObjectiveId",
                principalSchema: "Core",
                principalTable: "hrms_OrganizationalObjective",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_EmployeeGoal_hrms_ReviewCycle_ReviewCycleId",
                schema: "Core",
                table: "hrms_EmployeeGoal",
                column: "ReviewCycleId",
                principalSchema: "Core",
                principalTable: "hrms_ReviewCycle",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_EmployeeMovement_coreSalaryScale_ToSalaryScaleId",
                schema: "Core",
                table: "hrms_EmployeeMovement",
                column: "ToSalaryScaleId",
                principalSchema: "Core",
                principalTable: "coreSalaryScale",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_EmployeeMovement_hrms_Employee_EmployeeId",
                schema: "Core",
                table: "hrms_EmployeeMovement",
                column: "EmployeeId",
                principalSchema: "Core",
                principalTable: "hrms_Employee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_EmployeeRecognition_hrms_Employee_EmployeeId",
                schema: "Core",
                table: "hrms_EmployeeRecognition",
                column: "EmployeeId",
                principalSchema: "Core",
                principalTable: "hrms_Employee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_EmployeeRecognition_hrms_RecognitionBadge_RecognitionBadgeId",
                schema: "Core",
                table: "hrms_EmployeeRecognition",
                column: "RecognitionBadgeId",
                principalSchema: "Core",
                principalTable: "hrms_RecognitionBadge",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_EmployeeTermination_hrms_Employee_EmployeeId",
                schema: "Core",
                table: "hrms_EmployeeTermination",
                column: "EmployeeId",
                principalSchema: "Core",
                principalTable: "hrms_Employee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_GoalActionItem_hrms_EmployeeGoal_EmployeeGoalId",
                schema: "Core",
                table: "hrms_GoalActionItem",
                column: "EmployeeGoalId",
                principalSchema: "Core",
                principalTable: "hrms_EmployeeGoal",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_HiringRequest_hrms_OrganizationUnit_OrganizationUnitId",
                schema: "Core",
                table: "hrms_HiringRequest",
                column: "OrganizationUnitId",
                principalSchema: "Core",
                principalTable: "hrms_OrganizationUnit",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_HiringRequest_hrms_PositionClass_PositionClassId",
                schema: "Core",
                table: "hrms_HiringRequest",
                column: "PositionClassId",
                principalSchema: "Core",
                principalTable: "hrms_PositionClass",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_ImprovementPlan_hrms_Appraisal_AppraisalId",
                schema: "Core",
                table: "hrms_ImprovementPlan",
                column: "AppraisalId",
                principalSchema: "Core",
                principalTable: "hrms_Appraisal",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_ImprovementPlan_hrms_Employee_EmployeeId",
                schema: "Core",
                table: "hrms_ImprovementPlan",
                column: "EmployeeId",
                principalSchema: "Core",
                principalTable: "hrms_Employee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_Interview_hrms_JobApplication_ApplicationId",
                schema: "Core",
                table: "hrms_Interview",
                column: "ApplicationId",
                principalSchema: "Core",
                principalTable: "hrms_JobApplication",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_InterviewFeedback_hrms_InterviewPanelist_PanelistId",
                schema: "Core",
                table: "hrms_InterviewFeedback",
                column: "PanelistId",
                principalSchema: "Core",
                principalTable: "hrms_InterviewPanelist",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_InterviewPanelist_hrms_Employee_EmployeeId",
                schema: "Core",
                table: "hrms_InterviewPanelist",
                column: "EmployeeId",
                principalSchema: "Core",
                principalTable: "hrms_Employee",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_InterviewPanelist_hrms_Interview_InterviewId",
                schema: "Core",
                table: "hrms_InterviewPanelist",
                column: "InterviewId",
                principalSchema: "Core",
                principalTable: "hrms_Interview",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_JobApplication_hrms_Candidate_CandidateId",
                schema: "Core",
                table: "hrms_JobApplication",
                column: "CandidateId",
                principalSchema: "Core",
                principalTable: "hrms_Candidate",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_JobApplication_hrms_JobRequisition_RequisitionId",
                schema: "Core",
                table: "hrms_JobApplication",
                column: "RequisitionId",
                principalSchema: "Core",
                principalTable: "hrms_JobRequisition",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_JobApplicationStageLog_hrms_JobApplication_ApplicationId",
                schema: "Core",
                table: "hrms_JobApplicationStageLog",
                column: "ApplicationId",
                principalSchema: "Core",
                principalTable: "hrms_JobApplication",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_JobOffer_coreSalaryScale_SalaryScaleId",
                schema: "Core",
                table: "hrms_JobOffer",
                column: "SalaryScaleId",
                principalSchema: "Core",
                principalTable: "coreSalaryScale",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_JobOffer_hrms_Employee_HiringManagerEmployeeId",
                schema: "Core",
                table: "hrms_JobOffer",
                column: "HiringManagerEmployeeId",
                principalSchema: "Core",
                principalTable: "hrms_Employee",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_JobOffer_hrms_JobApplication_ApplicationId",
                schema: "Core",
                table: "hrms_JobOffer",
                column: "ApplicationId",
                principalSchema: "Core",
                principalTable: "hrms_JobApplication",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_JobRequisition_coreSalaryScale_SalaryScaleId",
                schema: "Core",
                table: "hrms_JobRequisition",
                column: "SalaryScaleId",
                principalSchema: "Core",
                principalTable: "coreSalaryScale",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_JobRequisition_hrms_HiringRequest_HiringRequestId",
                schema: "Core",
                table: "hrms_JobRequisition",
                column: "HiringRequestId",
                principalSchema: "Core",
                principalTable: "hrms_HiringRequest",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_JobRequisition_hrms_OrganizationUnit_OrganizationUnitId",
                schema: "Core",
                table: "hrms_JobRequisition",
                column: "OrganizationUnitId",
                principalSchema: "Core",
                principalTable: "hrms_OrganizationUnit",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_JobRequisition_hrms_PositionClass_PositionClassId",
                schema: "Core",
                table: "hrms_JobRequisition",
                column: "PositionClassId",
                principalSchema: "Core",
                principalTable: "hrms_PositionClass",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_JobRequisition_hrms_WorkLocation_WorkLocationId",
                schema: "Core",
                table: "hrms_JobRequisition",
                column: "WorkLocationId",
                principalSchema: "Core",
                principalTable: "hrms_WorkLocation",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_LeaveBalance_FiscalYear_FiscalYearId",
                schema: "Core",
                table: "hrms_LeaveBalance",
                column: "FiscalYearId",
                principalSchema: "Core",
                principalTable: "FiscalYear",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_LeaveBalance_hrms_Employee_EmployeeId",
                schema: "Core",
                table: "hrms_LeaveBalance",
                column: "EmployeeId",
                principalSchema: "Core",
                principalTable: "hrms_Employee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_LeaveBalance_hrms_LeaveType_LeaveTypeId",
                schema: "Core",
                table: "hrms_LeaveBalance",
                column: "LeaveTypeId",
                principalSchema: "Core",
                principalTable: "hrms_LeaveType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_LeaveRequest_FiscalYear_FiscalYearId",
                schema: "Core",
                table: "hrms_LeaveRequest",
                column: "FiscalYearId",
                principalSchema: "Core",
                principalTable: "FiscalYear",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_LeaveRequest_hrms_Employee_EmployeeId",
                schema: "Core",
                table: "hrms_LeaveRequest",
                column: "EmployeeId",
                principalSchema: "Core",
                principalTable: "hrms_Employee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_LeaveRequestLine_hrms_LeaveRequest_LeaveRequestId",
                schema: "Core",
                table: "hrms_LeaveRequestLine",
                column: "LeaveRequestId",
                principalSchema: "Core",
                principalTable: "hrms_LeaveRequest",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_LeaveRequestLine_hrms_LeaveType_LeaveTypeId",
                schema: "Core",
                table: "hrms_LeaveRequestLine",
                column: "LeaveTypeId",
                principalSchema: "Core",
                principalTable: "hrms_LeaveType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_OrganizationalObjective_hrms_OrganizationUnit_OrganizationUnitId",
                schema: "Core",
                table: "hrms_OrganizationalObjective",
                column: "OrganizationUnitId",
                principalSchema: "Core",
                principalTable: "hrms_OrganizationUnit",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_OrganizationalObjective_hrms_OrganizationalObjective_ParentObjectiveId",
                schema: "Core",
                table: "hrms_OrganizationalObjective",
                column: "ParentObjectiveId",
                principalSchema: "Core",
                principalTable: "hrms_OrganizationalObjective",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_OrganizationalObjective_hrms_ReviewCycle_ReviewCycleId",
                schema: "Core",
                table: "hrms_OrganizationalObjective",
                column: "ReviewCycleId",
                principalSchema: "Core",
                principalTable: "hrms_ReviewCycle",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_OrganizationUnit_hrms_Branch_BranchId",
                schema: "Core",
                table: "hrms_OrganizationUnit",
                column: "BranchId",
                principalSchema: "Core",
                principalTable: "hrms_Branch",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_OrganizationUnit_hrms_OrganizationUnit_ParentId",
                schema: "Core",
                table: "hrms_OrganizationUnit",
                column: "ParentId",
                principalSchema: "Core",
                principalTable: "hrms_OrganizationUnit",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_OrganizationUnit_hrms_WorkLocation_WorkLocationId",
                schema: "Core",
                table: "hrms_OrganizationUnit",
                column: "WorkLocationId",
                principalSchema: "Core",
                principalTable: "hrms_WorkLocation",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_PipObjective_hrms_ImprovementPlan_PipId",
                schema: "Core",
                table: "hrms_PipObjective",
                column: "PipId",
                principalSchema: "Core",
                principalTable: "hrms_ImprovementPlan",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_Position_hrms_Branch_BranchId",
                schema: "Core",
                table: "hrms_Position",
                column: "BranchId",
                principalSchema: "Core",
                principalTable: "hrms_Branch",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_Position_hrms_OrganizationUnit_OrganizationUnitId",
                schema: "Core",
                table: "hrms_Position",
                column: "OrganizationUnitId",
                principalSchema: "Core",
                principalTable: "hrms_OrganizationUnit",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_Position_hrms_PositionClass_PositionClassId",
                schema: "Core",
                table: "hrms_Position",
                column: "PositionClassId",
                principalSchema: "Core",
                principalTable: "hrms_PositionClass",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_PositionClass_coreSalaryScale_SalaryScaleId",
                schema: "Core",
                table: "hrms_PositionClass",
                column: "SalaryScaleId",
                principalSchema: "Core",
                principalTable: "coreSalaryScale",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_PositionClass_hrms_JobCategory_JobCategoryId",
                schema: "Core",
                table: "hrms_PositionClass",
                column: "JobCategoryId",
                principalSchema: "Core",
                principalTable: "hrms_JobCategory",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_PositionClass_hrms_PositionClass_ReportsToPositionClassId",
                schema: "Core",
                table: "hrms_PositionClass",
                column: "ReportsToPositionClassId",
                principalSchema: "Core",
                principalTable: "hrms_PositionClass",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_PositionClass_hrms_WorkLocation_WorkLocationId",
                schema: "Core",
                table: "hrms_PositionClass",
                column: "WorkLocationId",
                principalSchema: "Core",
                principalTable: "hrms_WorkLocation",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_PositionCompetency_hrms_Competency_CompetencyId",
                schema: "Core",
                table: "hrms_PositionCompetency",
                column: "CompetencyId",
                principalSchema: "Core",
                principalTable: "hrms_Competency",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_PositionCompetency_hrms_Position_PositionId",
                schema: "Core",
                table: "hrms_PositionCompetency",
                column: "PositionId",
                principalSchema: "Core",
                principalTable: "hrms_Position",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_RatingScaleLevel_hrms_RatingScale_RatingScaleId",
                schema: "Core",
                table: "hrms_RatingScaleLevel",
                column: "RatingScaleId",
                principalSchema: "Core",
                principalTable: "hrms_RatingScale",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_ReportField_hrms_Report_ReportId",
                schema: "Core",
                table: "hrms_ReportField",
                column: "ReportId",
                principalSchema: "Core",
                principalTable: "hrms_Report",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_ReportFieldOutput_hrms_Report_ReportId",
                schema: "Core",
                table: "hrms_ReportFieldOutput",
                column: "ReportId",
                principalSchema: "Core",
                principalTable: "hrms_Report",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_ReportRestriction_hrms_Report_ReportId",
                schema: "Core",
                table: "hrms_ReportRestriction",
                column: "ReportId",
                principalSchema: "Core",
                principalTable: "hrms_Report",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_ReportRunRecipient_hrms_ReportRun_ReportRunId",
                schema: "Core",
                table: "hrms_ReportRunRecipient",
                column: "ReportRunId",
                principalSchema: "Core",
                principalTable: "hrms_ReportRun",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_ReportSavedFilter_hrms_Report_ReportId",
                schema: "Core",
                table: "hrms_ReportSavedFilter",
                column: "ReportId",
                principalSchema: "Core",
                principalTable: "hrms_Report",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_ReportSchedule_hrms_Report_ReportId",
                schema: "Core",
                table: "hrms_ReportSchedule",
                column: "ReportId",
                principalSchema: "Core",
                principalTable: "hrms_Report",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_ReportScheduleFieldOutput_hrms_ReportSchedule_ReportScheduleId",
                schema: "Core",
                table: "hrms_ReportScheduleFieldOutput",
                column: "ReportScheduleId",
                principalSchema: "Core",
                principalTable: "hrms_ReportSchedule",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_ReportScheduleFieldValue_hrms_ReportSchedule_ReportScheduleId",
                schema: "Core",
                table: "hrms_ReportScheduleFieldValue",
                column: "ReportScheduleId",
                principalSchema: "Core",
                principalTable: "hrms_ReportSchedule",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_ReportScheduleRecipient_hrms_ReportSchedule_ReportScheduleId",
                schema: "Core",
                table: "hrms_ReportScheduleRecipient",
                column: "ReportScheduleId",
                principalSchema: "Core",
                principalTable: "hrms_ReportSchedule",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_RequisitionScreeningCriterion_hrms_JobRequisition_RequisitionId",
                schema: "Core",
                table: "hrms_RequisitionScreeningCriterion",
                column: "RequisitionId",
                principalSchema: "Core",
                principalTable: "hrms_JobRequisition",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_ReviewCycle_FiscalYear_FiscalYearId",
                schema: "Core",
                table: "hrms_ReviewCycle",
                column: "FiscalYearId",
                principalSchema: "Core",
                principalTable: "FiscalYear",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_ReviewCycle_hrms_RatingScale_RatingScaleId",
                schema: "Core",
                table: "hrms_ReviewCycle",
                column: "RatingScaleId",
                principalSchema: "Core",
                principalTable: "hrms_RatingScale",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_TerminationClearance_hrms_ClearanceDepartment_DepartmentId",
                schema: "Core",
                table: "hrms_TerminationClearance",
                column: "DepartmentId",
                principalSchema: "Core",
                principalTable: "hrms_ClearanceDepartment",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_TerminationClearance_hrms_EmployeeTermination_TerminationId",
                schema: "Core",
                table: "hrms_TerminationClearance",
                column: "TerminationId",
                principalSchema: "Core",
                principalTable: "hrms_EmployeeTermination",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_WorkflowActionLog_hrms_WorkflowInstance_InstanceId",
                schema: "Core",
                table: "hrms_WorkflowActionLog",
                column: "InstanceId",
                principalSchema: "Core",
                principalTable: "hrms_WorkflowInstance",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_WorkflowInstance_hrms_WorkflowDefinition_DefinitionId",
                schema: "Core",
                table: "hrms_WorkflowInstance",
                column: "DefinitionId",
                principalSchema: "Core",
                principalTable: "hrms_WorkflowDefinition",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_WorkflowStep_hrms_WorkflowDefinition_DefinitionId",
                schema: "Core",
                table: "hrms_WorkflowStep",
                column: "DefinitionId",
                principalSchema: "Core",
                principalTable: "hrms_WorkflowDefinition",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_WorkflowStepApprover_hrms_WorkflowStep_StepId",
                schema: "Core",
                table: "hrms_WorkflowStepApprover",
                column: "StepId",
                principalSchema: "Core",
                principalTable: "hrms_WorkflowStep",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_WorkforcePlan_FiscalYear_StartFiscalYearId",
                schema: "Core",
                table: "hrms_WorkforcePlan",
                column: "StartFiscalYearId",
                principalSchema: "Core",
                principalTable: "FiscalYear",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_WorkforcePlan_hrms_OrganizationUnit_OrganizationUnitId",
                schema: "Core",
                table: "hrms_WorkforcePlan",
                column: "OrganizationUnitId",
                principalSchema: "Core",
                principalTable: "hrms_OrganizationUnit",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_WorkforcePlanLine_hrms_OrganizationUnit_OrganizationUnitId",
                schema: "Core",
                table: "hrms_WorkforcePlanLine",
                column: "OrganizationUnitId",
                principalSchema: "Core",
                principalTable: "hrms_OrganizationUnit",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_WorkforcePlanLine_hrms_PositionClass_PositionClassId",
                schema: "Core",
                table: "hrms_WorkforcePlanLine",
                column: "PositionClassId",
                principalSchema: "Core",
                principalTable: "hrms_PositionClass",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_WorkforcePlanLine_hrms_WorkforcePlan_PlanId",
                schema: "Core",
                table: "hrms_WorkforcePlanLine",
                column: "PlanId",
                principalSchema: "Core",
                principalTable: "hrms_WorkforcePlan",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_hrms_WorkLocation_hrms_WorkLocation_ParentId",
                schema: "Core",
                table: "hrms_WorkLocation",
                column: "ParentId",
                principalSchema: "Core",
                principalTable: "hrms_WorkLocation",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_User_hrms_Employee_EmployeeId",
                schema: "Core",
                table: "User",
                column: "EmployeeId",
                principalSchema: "Core",
                principalTable: "hrms_Employee",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            // ---- Restore stored procedures to the original Core.hrms_* table names ----
            // hrms_Report_EmployeeDirectory
            migrationBuilder.Sql(@"
CREATE OR ALTER PROCEDURE [Core].[hrms_Report_EmployeeDirectory]
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
        FROM Core.hrms_Employee e
                    left JOIN          Core.CorePerson p             ON p.Id  = e.PersonId
                    left JOIN          Core.hrms_Position pos        ON pos.Id = e.PositionId
                    left join          Core.hrms_PositionClass poc   on poc.Id = pos.[PositionClassId]
                    left JOIN Core.hrms_OrganizationUnit ou ON ou.Id = pos.OrganizationUnitId
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
            // hrms_Report_EmployeeDirectoryGrouped
            migrationBuilder.Sql(@"
CREATE OR ALTER PROCEDURE Core.hrms_Report_EmployeeDirectoryGrouped
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
        FROM Core.hrms_Employee e
        LEFT JOIN Core.CorePerson p             ON p.Id  = e.PersonId
        LEFT JOIN Core.hrms_Position pos        ON pos.Id = e.PositionId
        LEFT JOIN Core.hrms_OrganizationUnit ou ON ou.Id = pos.OrganizationUnitId
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
            // hrms_ReportActivate
            migrationBuilder.Sql(@"
CREATE OR ALTER PROCEDURE Core.hrms_ReportActivate
    @ReportId UNIQUEIDENTIFIER,
    @IsActive BIT,
    @TenantId NVARCHAR(450) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Core.hrms_Report
       SET IsActive = @IsActive,
           UpdatedAt = SYSUTCDATETIME(),
           RowVersion = CONVERT(varbinary(8), NEWID())
     WHERE Id = @ReportId
       AND (@TenantId IS NULL OR @TenantId = '' OR TenantId = @TenantId);
END");
            // hrms_ReportClientSchedule
            migrationBuilder.Sql(@"
CREATE OR ALTER PROCEDURE Core.hrms_ReportClientSchedule
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
       OR NOT EXISTS (SELECT 1 FROM Core.hrms_ReportSchedule WHERE Id = @ReportScheduleId)
    BEGIN
        SET @ReportScheduleId = NEWID();
        INSERT INTO Core.hrms_ReportSchedule
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
        UPDATE Core.hrms_ReportSchedule
           SET Name = @Name, IsScheduled = @IsScheduled, MailSubject = @MailSubject, MailBody = @MailBody,
               IsHideRecipients = @IsHideRecipients, Frequency = @Frequency, FrequencyWeekly = @FrequencyWeekly,
               TimeOfTheDay = @TimeOfTheDay, ScheduleStartDate = @ScheduleStartDate, OutputFormat = @OutputFormat,
               CronExpression = @CronExpression, UpdatedAt = SYSUTCDATETIME(), RowVersion = CONVERT(varbinary(8), NEWID())
         WHERE Id = @ReportScheduleId;
    END
END");
            // hrms_ReportClientScheduleDelete
            migrationBuilder.Sql(@"
CREATE OR ALTER PROCEDURE Core.hrms_ReportClientScheduleDelete
    @ReportScheduleId UNIQUEIDENTIFIER,
    @IsModifyOnly INT = 0
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM Core.hrms_ReportScheduleRecipient  WHERE ReportScheduleId = @ReportScheduleId;
    DELETE FROM Core.hrms_ReportScheduleFieldValue WHERE ReportScheduleId = @ReportScheduleId;
    DELETE FROM Core.hrms_ReportScheduleFieldOutput WHERE ReportScheduleId = @ReportScheduleId;
    IF @IsModifyOnly = 0
        DELETE FROM Core.hrms_ReportSchedule WHERE Id = @ReportScheduleId;
END");
            // hrms_ReportClientScheduleEnable
            migrationBuilder.Sql(@"
CREATE OR ALTER PROCEDURE Core.hrms_ReportClientScheduleEnable
    @ReportScheduleId UNIQUEIDENTIFIER,
    @Enabled INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Core.hrms_ReportSchedule
       SET IsActive = CASE WHEN @Enabled = 1 THEN 1 ELSE 0 END,
           UpdatedAt = SYSUTCDATETIME(), RowVersion = CONVERT(varbinary(8), NEWID())
     WHERE Id = @ReportScheduleId;
END");
            // hrms_ReportClientScheduleFieldOutput
            migrationBuilder.Sql(@"
CREATE OR ALTER PROCEDURE Core.hrms_ReportClientScheduleFieldOutput
    @ReportScheduleId UNIQUEIDENTIFIER,
    @ReportKey NVARCHAR(100),
    @Field NVARCHAR(100),
    @Label NVARCHAR(200),
    @FieldOrder INT = 0,
    @SortOrder INT = 0
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Tenant NVARCHAR(450) = (SELECT TOP 1 TenantId FROM Core.hrms_ReportSchedule WHERE Id = @ReportScheduleId);
    INSERT INTO Core.hrms_ReportScheduleFieldOutput
        (Id, ReportScheduleId, ReportKey, Field, Label, FieldOrder, SortOrder, TenantId, CreatedAt, RowVersion)
    VALUES
        (NEWID(), @ReportScheduleId, @ReportKey, @Field, @Label, @FieldOrder, @SortOrder, @Tenant, SYSUTCDATETIME(), CONVERT(varbinary(8), NEWID()));
END");
            // hrms_ReportClientScheduleFieldValue
            migrationBuilder.Sql(@"
CREATE OR ALTER PROCEDURE Core.hrms_ReportClientScheduleFieldValue
    @ReportScheduleId UNIQUEIDENTIFIER,
    @ReportKey NVARCHAR(100),
    @Field NVARCHAR(100),
    @Value NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Tenant NVARCHAR(450) = (SELECT TOP 1 TenantId FROM Core.hrms_ReportSchedule WHERE Id = @ReportScheduleId);
    INSERT INTO Core.hrms_ReportScheduleFieldValue
        (Id, ReportScheduleId, ReportKey, Field, Value, TenantId, CreatedAt, RowVersion)
    VALUES
        (NEWID(), @ReportScheduleId, @ReportKey, @Field, @Value, @Tenant, SYSUTCDATETIME(), CONVERT(varbinary(8), NEWID()));
END");
            // hrms_ReportClientScheduleRead
            migrationBuilder.Sql(@"
CREATE OR ALTER PROCEDURE Core.hrms_ReportClientScheduleRead
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
          FROM Core.hrms_ReportSchedule s
          JOIN Core.hrms_Report r ON r.Id = s.ReportId
         WHERE s.Id = @Id
           AND (@TenantId IS NULL OR @TenantId = '' OR s.TenantId = @TenantId);
    ELSE
        SELECT s.Id AS ReportScheduleId, s.ReportId, r.ReportKey, r.ReportName, s.Name,
               s.IsScheduled, s.IsActive, s.MailSubject, s.MailBody, s.IsHideRecipients,
               s.Frequency, s.FrequencyWeekly, s.TimeOfTheDay, s.ScheduleStartDate,
               s.OutputFormat, s.CronExpression, r.StoredProc
          FROM Core.hrms_ReportSchedule s
          JOIN Core.hrms_Report r ON r.Id = s.ReportId
         WHERE s.ReportId = @Id
           AND (@TenantId IS NULL OR @TenantId = '' OR s.TenantId = @TenantId)
         ORDER BY s.Name;
END");
            // hrms_ReportClientScheduleRecipient
            migrationBuilder.Sql(@"
CREATE OR ALTER PROCEDURE Core.hrms_ReportClientScheduleRecipient
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
            COALESCE(NULLIF(@TenantId, ''), (SELECT TOP 1 TenantId FROM Core.hrms_ReportSchedule WHERE Id = @ReportScheduleId));
        DECLARE @ResolvedEmail NVARCHAR(300) = @Email;
        IF @ResolvedEmail IS NULL AND @UserId IS NOT NULL
            SET @ResolvedEmail = (SELECT TOP 1 Email FROM Core.[User] WHERE Id = @UserId);
        INSERT INTO Core.hrms_ReportScheduleRecipient
            (Id, ReportScheduleId, UserId, RoleId, Email, TenantId, CreatedAt, RowVersion)
        VALUES
            (NEWID(), @ReportScheduleId, @UserId, @RoleId, @ResolvedEmail, @Tenant, SYSUTCDATETIME(), CONVERT(varbinary(8), NEWID()));
    END
    ELSE IF @Type = 'ListUsers'
    BEGIN
        SELECT @ReportScheduleId AS ReportScheduleId, u.Id AS UserId, u.UserName AS UserName,
               CAST(CASE WHEN r.Id IS NOT NULL THEN 1 ELSE 0 END AS BIT) AS IsAssigned, u.Email AS Email
        FROM Core.[User] u
        LEFT JOIN Core.hrms_ReportScheduleRecipient r
            ON r.ReportScheduleId = @ReportScheduleId AND r.UserId = u.Id
        WHERE (@TenantId IS NULL OR @TenantId = '' OR u.TenantId = @TenantId)
        ORDER BY u.UserName;
    END
    ELSE IF @Type = 'ListRoles'
    BEGIN
        SELECT @ReportScheduleId AS ReportScheduleId, ro.Id AS RoleId, ro.Name AS RoleName,
               CAST(CASE WHEN r.Id IS NOT NULL THEN 1 ELSE 0 END AS BIT) AS IsAssigned
        FROM Core.Role ro
        LEFT JOIN Core.hrms_ReportScheduleRecipient r
            ON r.ReportScheduleId = @ReportScheduleId AND r.RoleId = ro.Id
        WHERE (@TenantId IS NULL OR @TenantId = '' OR ro.TenantId = @TenantId)
        ORDER BY ro.Name;
    END
END");
            // hrms_ReportDelete
            migrationBuilder.Sql(@"
CREATE OR ALTER PROCEDURE Core.hrms_ReportDelete
    @ReportId UNIQUEIDENTIFIER,
    @TenantId NVARCHAR(450) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM Core.hrms_Report
     WHERE Id = @ReportId
       AND (@TenantId IS NULL OR @TenantId = '' OR TenantId = @TenantId);
END");
            // hrms_ReportFieldOutputRead
            migrationBuilder.Sql(@"
CREATE OR ALTER PROCEDURE Core.hrms_ReportFieldOutputRead
    @ReportKey NVARCHAR(100),
    @ReportScheduleId UNIQUEIDENTIFIER = NULL,
    @TenantId NVARCHAR(450) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @ReportId UNIQUEIDENTIFIER =
        (SELECT TOP 1 Id FROM Core.hrms_Report
          WHERE ReportKey = @ReportKey
            AND (@TenantId IS NULL OR @TenantId = '' OR TenantId = @TenantId));

    SELECT
        CASE WHEN @ReportScheduleId IS NULL THEN 1
             WHEN so.Id IS NOT NULL THEN 1 ELSE 0 END AS IsShow,
        fo.Field AS Field,
        COALESCE(so.Label, fo.Label) AS Label,
        COALESCE(so.SortOrder, 0) AS SortOrder,
        COALESCE(so.FieldOrder, fo.FieldOrder) AS FieldOrder
    FROM Core.hrms_ReportFieldOutput fo
    LEFT JOIN Core.hrms_ReportScheduleFieldOutput so
        ON so.ReportScheduleId = @ReportScheduleId AND so.Field = fo.Field
    WHERE fo.ReportId = @ReportId
    ORDER BY COALESCE(so.FieldOrder, fo.FieldOrder), fo.Label;
END");
            // hrms_ReportFieldValues
            migrationBuilder.Sql(@"
CREATE OR ALTER PROCEDURE Core.hrms_ReportFieldValues
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
        FROM Core.hrms_OrganizationUnit
        WHERE TenantId = @TenantId AND IsActive = 1
          AND (@BranchId IS NULL OR BranchId = @BranchId OR BranchId IS NULL)
          AND (@Search IS NULL OR Name LIKE '%' + @Search + '%')
        ORDER BY Name;
    ELSE IF @Field = 'EmploymentStatus'
        SELECT v.Value, v.Value AS Label
        FROM (VALUES ('Active'),('Probation'),('OnLeave'),('Suspended'),('Terminated'),('Retired')) v(Value);
    ELSE IF @Field = 'LeaveTypeId'
        SELECT CAST(Id AS NVARCHAR(50)) AS Value, Name AS Label
        FROM Core.hrms_LeaveType
        WHERE TenantId = @TenantId AND IsActive = 1
          AND (@Search IS NULL OR Name LIKE '%' + @Search + '%')
        ORDER BY Name;
    ELSE
        SELECT CAST(NULL AS NVARCHAR(50)) AS Value, CAST(NULL AS NVARCHAR(200)) AS Label
        WHERE 1 = 0;
END");
            // hrms_ReportGenerateGetScheduleInfo
            migrationBuilder.Sql(@"
CREATE OR ALTER PROCEDURE Core.hrms_ReportGenerateGetScheduleInfo
    @TenantId NVARCHAR(450) = NULL,
    @ReportScheduleId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT s.Id AS ReportScheduleId, s.ReportId, r.ReportKey, r.ReportName, s.Name,
           s.IsScheduled, s.IsActive, s.MailSubject, s.MailBody, s.IsHideRecipients,
           s.Frequency, s.FrequencyWeekly, s.TimeOfTheDay, s.ScheduleStartDate,
           s.OutputFormat, s.CronExpression, r.StoredProc
      FROM Core.hrms_ReportSchedule s
      JOIN Core.hrms_Report r ON r.Id = s.ReportId
     WHERE s.Id = @ReportScheduleId
       AND (@TenantId IS NULL OR @TenantId = '' OR s.TenantId = @TenantId);

    SELECT Field, Value FROM Core.hrms_ReportScheduleFieldValue WHERE ReportScheduleId = @ReportScheduleId;

    SELECT DISTINCT e.Email FROM (
        SELECT rec.Email AS Email
          FROM Core.hrms_ReportScheduleRecipient rec
         WHERE rec.ReportScheduleId = @ReportScheduleId AND rec.Email IS NOT NULL AND rec.Email <> ''
        UNION
        SELECT u.Email
          FROM Core.hrms_ReportScheduleRecipient rec
          JOIN Core.[User] u ON u.Id = rec.UserId
         WHERE rec.ReportScheduleId = @ReportScheduleId
        UNION
        SELECT u.Email
          FROM Core.hrms_ReportScheduleRecipient rec
          JOIN Core.UserRole ur ON ur.RoleId = rec.RoleId
          JOIN Core.[User] u ON u.Id = ur.UserId
         WHERE rec.ReportScheduleId = @ReportScheduleId
    ) e
    WHERE e.Email IS NOT NULL AND e.Email <> '';

    SELECT 1 AS IsShow, Field, Label, SortOrder, FieldOrder
      FROM Core.hrms_ReportScheduleFieldOutput
     WHERE ReportScheduleId = @ReportScheduleId
     ORDER BY FieldOrder;
END");
            // hrms_ReportGenerateSendToHistory
            migrationBuilder.Sql(@"
CREATE OR ALTER PROCEDURE Core.hrms_ReportGenerateSendToHistory
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
    INSERT INTO Core.hrms_ReportRun
        (Id, TenantId, ReportKey, CriteriaJson, [RowCount], DurationMs, RanBy, IsScheduled, FieldOutput, CreatedAt, RowVersion)
    VALUES
        (@RunId, @TenantId, @ReportKey, ISNULL(@Criteria, '{}'), @TotalRecords, @RunSeconds * 1000, @RanBy,
         @IsScheduled, @FieldOutput, SYSUTCDATETIME(), CONVERT(varbinary(8), NEWID()));

    IF @Recipients IS NOT NULL AND @Recipients <> ''
        INSERT INTO Core.hrms_ReportRunRecipient (Id, ReportRunId, UserId, Email, TenantId, CreatedAt, RowVersion)
        SELECT NEWID(), @RunId, NULL, LTRIM(RTRIM(value)), @TenantId, SYSUTCDATETIME(), CONVERT(varbinary(8), NEWID())
          FROM STRING_SPLIT(@Recipients, ';')
         WHERE LTRIM(RTRIM(value)) <> '';
END");

        }
    }
}
