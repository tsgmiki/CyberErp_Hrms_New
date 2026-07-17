using Microsoft.Extensions.DependencyInjection;
using CyberErp.Hrms.App.Features.Core.Users.Login;
using CyberErp.Hrms.App.Features.Core.Users.Logout;
using CyberErp.Hrms.App.Features.Core.Users.GetCurrentUser;
using CyberErp.Hrms.App.Features.Core.Users.Register;
using CyberErp.Hrms.App.Features.Core.OrganizationUnits;
using CyberErp.Hrms.App.Features.Core.Positions;
using CyberErp.Hrms.App.Features.Core.PositionClasses;
using CyberErp.Hrms.App.Features.Core.JobGrades;
using CyberErp.Hrms.App.Features.Core.JobCategories;
using CyberErp.Hrms.App.Features.Core.WorkLocations;
using CyberErp.Hrms.App.Features.Core.Branches;
using CyberErp.Hrms.App.Features.Core.AuditLogs;
using CyberErp.Hrms.App.Features.Core.Employees;
using CyberErp.Hrms.App.Features.Core.EmployeeFields;
using CyberErp.Hrms.App.Features.Core.DocumentTemplates;
using CyberErp.Hrms.App.Features.Core.Workflows;
using CyberErp.Hrms.App.Features.Core.ClearanceDepartments;
using CyberErp.Hrms.App.Features.Core.Roles;

namespace CyberErp.Hrms.App
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Users - Auth
            services.AddScoped<ILoginUser, LoginUser>();
            services.AddScoped<ILogoutUser, LogoutUser>();
            services.AddScoped<ILogoutCookieUser, LogoutCookieHandler>();
            services.AddScoped<IRegisterUser, RegisterUser>();
            services.AddScoped<IRegisterWithGoogle, RegisterWithGoogle>();
            services.AddScoped<IGetCurrentUser, GetCurrentUser>();

            // Organizational Structure (HRMS §3.1)
            AddOrganizationalStructure(services);

            // Generic centralized lookup system (2-table reference data, global across tenants)
            services.AddScoped<Features.Core.Lookups.IGetLookupItems, Features.Core.Lookups.GetLookupItems>();
            services.AddScoped<Features.Core.Lookups.IGetAllLookupCategories, Features.Core.Lookups.GetAllLookupCategories>();
            services.AddScoped<Features.Core.Lookups.ISaveLookupCategory, Features.Core.Lookups.SaveLookupCategory>();
            services.AddScoped<Features.Core.Lookups.IDeleteLookupCategory, Features.Core.Lookups.DeleteLookupCategory>();

            return services;
        }

        private static void AddOrganizationalStructure(IServiceCollection services)
        {
            // Organization Units
            services.AddScoped<ICreateOrganizationUnit, CreateOrganizationUnit>();
            services.AddScoped<IUpdateOrganizationUnit, UpdateOrganizationUnit>();
            services.AddScoped<IDeleteOrganizationUnit, DeleteOrganizationUnit>();
            services.AddScoped<IGetOrganizationUnitById, GetOrganizationUnitById>();
            services.AddScoped<IGetAllOrganizationUnits, GetAllOrganizationUnits>();
            services.AddScoped<IGetOrganizationTree, GetOrganizationTree>();

            // Positions
            services.AddScoped<ICreatePosition, CreatePosition>();
            services.AddScoped<IUpdatePosition, UpdatePosition>();
            services.AddScoped<IDeletePosition, DeletePosition>();
            services.AddScoped<IGetPositionById, GetPositionById>();
            services.AddScoped<IGetAllPositions, GetAllPositions>();

            // Position Classes (job definitions)
            services.AddScoped<ICreatePositionClass, CreatePositionClass>();
            services.AddScoped<IUpdatePositionClass, UpdatePositionClass>();
            services.AddScoped<IDeletePositionClass, DeletePositionClass>();
            services.AddScoped<IGetPositionClassById, GetPositionClassById>();
            services.AddScoped<IGetAllPositionClasses, GetAllPositionClasses>();

            // Job Grades
            services.AddScoped<ICreateJobGrade, CreateJobGrade>();
            services.AddScoped<IUpdateJobGrade, UpdateJobGrade>();
            services.AddScoped<IDeleteJobGrade, DeleteJobGrade>();
            services.AddScoped<IGetJobGradeById, GetJobGradeById>();
            services.AddScoped<IGetAllJobGrades, GetAllJobGrades>();

            // Salary steps (lupStep) + salary scale (coreSalaryScale)
            services.AddScoped<Features.Core.Steps.ISaveStep, Features.Core.Steps.SaveStep>();
            services.AddScoped<Features.Core.Steps.IGetStepById, Features.Core.Steps.GetStepById>();
            services.AddScoped<Features.Core.Steps.IGetAllSteps, Features.Core.Steps.GetAllSteps>();
            services.AddScoped<Features.Core.Steps.IDeleteStep, Features.Core.Steps.DeleteStep>();
            services.AddScoped<Features.Core.SalaryScales.ISaveSalaryScale, Features.Core.SalaryScales.SaveSalaryScale>();
            services.AddScoped<Features.Core.SalaryScales.IGetSalaryScaleById, Features.Core.SalaryScales.GetSalaryScaleById>();
            services.AddScoped<Features.Core.SalaryScales.IGetAllSalaryScales, Features.Core.SalaryScales.GetAllSalaryScales>();
            services.AddScoped<Features.Core.SalaryScales.IDeleteSalaryScale, Features.Core.SalaryScales.DeleteSalaryScale>();

            // Attendance & Leave (HC030–HC052) — Phase 1: leave setup + working calendar
            services.AddScoped<Features.Core.Leaves.IWorkingCalendar, Features.Core.Leaves.WorkingCalendar>();
            services.AddScoped<Features.Core.Leaves.ISaveWorkWeekConfiguration, Features.Core.Leaves.SaveWorkWeekConfiguration>();
            services.AddScoped<Features.Core.Leaves.IDeleteWorkWeekConfiguration, Features.Core.Leaves.DeleteWorkWeekConfiguration>();
            services.AddScoped<Features.Core.Leaves.IGetWorkWeekConfigurationById, Features.Core.Leaves.GetWorkWeekConfigurationById>();
            services.AddScoped<Features.Core.Leaves.IGetAllWorkWeekConfigurations, Features.Core.Leaves.GetAllWorkWeekConfigurations>();
            services.AddScoped<Features.Core.Leaves.ISaveLeaveType, Features.Core.Leaves.SaveLeaveType>();
            services.AddScoped<Features.Core.Leaves.IGetLeaveTypeById, Features.Core.Leaves.GetLeaveTypeById>();
            services.AddScoped<Features.Core.Leaves.IGetAllLeaveTypes, Features.Core.Leaves.GetAllLeaveTypes>();
            services.AddScoped<Features.Core.Leaves.IDeleteLeaveType, Features.Core.Leaves.DeleteLeaveType>();
            services.AddScoped<Features.Core.Leaves.ISaveHoliday, Features.Core.Leaves.SaveHoliday>();
            services.AddScoped<Features.Core.Leaves.IGetHolidayById, Features.Core.Leaves.GetHolidayById>();
            services.AddScoped<Features.Core.Leaves.IGetAllHolidays, Features.Core.Leaves.GetAllHolidays>();
            services.AddScoped<Features.Core.Leaves.IDeleteHoliday, Features.Core.Leaves.DeleteHoliday>();
            services.AddScoped<Features.Core.Leaves.IGetWorkingDays, Features.Core.Leaves.GetWorkingDays>();
            // Phase 2: leave balances (ledger) + leave requests (workflow-backed)
            services.AddScoped<Features.Core.Leaves.ILeaveBalanceService, Features.Core.Leaves.LeaveBalanceService>();
            services.AddScoped<Features.Core.Leaves.IGetLeaveBalances, Features.Core.Leaves.GetLeaveBalances>();
            services.AddScoped<Features.Core.Leaves.ISetLeaveBalance, Features.Core.Leaves.SetLeaveBalance>();
            services.AddScoped<Features.Core.Leaves.ISubmitLeaveRequest, Features.Core.Leaves.SubmitLeaveRequest>();
            services.AddScoped<Features.Core.Leaves.ICancelLeaveRequest, Features.Core.Leaves.CancelLeaveRequest>();
            services.AddScoped<Features.Core.Leaves.IGetLeaveRequestById, Features.Core.Leaves.GetLeaveRequestById>();
            services.AddScoped<Features.Core.Leaves.IGetAllLeaveRequests, Features.Core.Leaves.GetAllLeaveRequests>();
            services.AddScoped<IWorkflowEntityHandler, Features.Core.Leaves.LeaveRequestWorkflowHandler>();
            services.AddScoped<Features.Core.Leaves.ISubmitAnnualLeave, Features.Core.Leaves.SubmitAnnualLeave>();
            services.AddScoped<Features.Core.Leaves.ICancelAnnualLeave, Features.Core.Leaves.CancelAnnualLeave>();
            services.AddScoped<Features.Core.Leaves.IGetAnnualLeaveById, Features.Core.Leaves.GetAnnualLeaveById>();
            services.AddScoped<Features.Core.Leaves.IGetAllAnnualLeaves, Features.Core.Leaves.GetAllAnnualLeaves>();
            services.AddScoped<IWorkflowEntityHandler, Features.Core.Leaves.AnnualLeaveWorkflowHandler>();
            // Fiscal-year integration: FY CRUD/resolver, accrual policy, entitlement generation + rollover
            services.AddScoped<Features.Core.Leaves.ISaveFiscalYear, Features.Core.Leaves.SaveFiscalYear>();
            services.AddScoped<Features.Core.Leaves.IGetFiscalYearById, Features.Core.Leaves.GetFiscalYearById>();
            services.AddScoped<Features.Core.Leaves.IGetAllFiscalYears, Features.Core.Leaves.GetAllFiscalYears>();
            services.AddScoped<Features.Core.Leaves.IDeleteFiscalYear, Features.Core.Leaves.DeleteFiscalYear>();
            services.AddScoped<Features.Core.Leaves.IFiscalYearResolver, Features.Core.Leaves.FiscalYearResolver>();
            services.AddScoped<Features.Core.Leaves.ISaveAnnualLeaveSetting, Features.Core.Leaves.SaveAnnualLeaveSetting>();
            services.AddScoped<Features.Core.Leaves.IGetAnnualLeaveSettingById, Features.Core.Leaves.GetAnnualLeaveSettingById>();
            services.AddScoped<Features.Core.Leaves.IGetAllAnnualLeaveSettings, Features.Core.Leaves.GetAllAnnualLeaveSettings>();
            services.AddScoped<Features.Core.Leaves.IDeleteAnnualLeaveSetting, Features.Core.Leaves.DeleteAnnualLeaveSetting>();
            services.AddScoped<Features.Core.Leaves.ILeaveAccrualService, Features.Core.Leaves.LeaveAccrualService>();
            services.AddScoped<Features.Core.Leaves.IGetAnnualLeaveLedger, Features.Core.Leaves.GetAnnualLeaveLedger>();

            // Job Categories
            services.AddScoped<ICreateJobCategory, CreateJobCategory>();
            services.AddScoped<IUpdateJobCategory, UpdateJobCategory>();
            services.AddScoped<IDeleteJobCategory, DeleteJobCategory>();
            services.AddScoped<IGetJobCategoryById, GetJobCategoryById>();
            services.AddScoped<IGetAllJobCategories, GetAllJobCategories>();

            // Work Locations
            services.AddScoped<ICreateWorkLocation, CreateWorkLocation>();
            services.AddScoped<IUpdateWorkLocation, UpdateWorkLocation>();
            services.AddScoped<IDeleteWorkLocation, DeleteWorkLocation>();
            services.AddScoped<IGetWorkLocationById, GetWorkLocationById>();
            services.AddScoped<IGetAllWorkLocations, GetAllWorkLocations>();

            // Branches (multi-branch structure)
            services.AddScoped<ICreateBranch, CreateBranch>();
            services.AddScoped<IUpdateBranch, UpdateBranch>();
            services.AddScoped<IDeleteBranch, DeleteBranch>();
            services.AddScoped<IGetBranchById, GetBranchById>();
            services.AddScoped<IGetAllBranches, GetAllBranches>();

            // Audit trail (read-only)
            services.AddScoped<IGetAllAuditLogs, GetAllAuditLogs>();

            // Employee Data Management (HRMS §3.2)
            services.AddScoped<ICreateEmployee, CreateEmployee>();
            services.AddScoped<IUpdateEmployee, UpdateEmployee>();
            services.AddScoped<IDeleteEmployee, DeleteEmployee>();
            services.AddScoped<IGetEmployeeById, GetEmployeeById>();
            services.AddScoped<IGetAllEmployees, GetAllEmployees>();
            services.AddScoped<IUploadEmployeePhoto, UploadEmployeePhoto>();
            services.AddScoped<IGetEmployeePhoto, GetEmployeePhoto>();
            services.AddScoped<IGetEmployeesOnProbation, GetEmployeesOnProbation>();
            services.AddScoped<IGetUpcomingRetirements, GetUpcomingRetirements>();
            services.AddScoped<ISaveEmployeeEducation, SaveEmployeeEducation>();
            services.AddScoped<IDeleteEmployeeEducation, DeleteEmployeeEducation>();
            services.AddScoped<IGetEmployeeEducations, GetEmployeeEducations>();
            services.AddScoped<ISaveEmployeeExperience, SaveEmployeeExperience>();
            services.AddScoped<IDeleteEmployeeExperience, DeleteEmployeeExperience>();
            services.AddScoped<IGetEmployeeExperiences, GetEmployeeExperiences>();
            services.AddScoped<ISaveEmployeeDependent, SaveEmployeeDependent>();
            services.AddScoped<IDeleteEmployeeDependent, DeleteEmployeeDependent>();
            services.AddScoped<IGetEmployeeDependents, GetEmployeeDependents>();
            services.AddScoped<IUploadEmployeeDocument, UploadEmployeeDocument>();
            services.AddScoped<IGetEmployeeDocuments, GetEmployeeDocuments>();
            services.AddScoped<IDownloadEmployeeDocument, DownloadEmployeeDocument>();
            services.AddScoped<IDeleteEmployeeDocument, DeleteEmployeeDocument>();

            // Personnel actions: transfers / promotions / demotions + discipline (HC015-HC029)
            services.AddScoped<ISaveEmployeeMovement, SaveEmployeeMovement>();
            services.AddScoped<IGetEmployeeMovements, GetEmployeeMovements>();
            services.AddScoped<IExecuteEmployeeMovement, ExecuteEmployeeMovement>();
            services.AddScoped<ICancelEmployeeMovement, CancelEmployeeMovement>();
            services.AddScoped<IDeleteEmployeeMovement, DeleteEmployeeMovement>();
            services.AddScoped<ISaveDisciplinaryMeasure, SaveDisciplinaryMeasure>();
            services.AddScoped<IGetDisciplinaryMeasures, GetDisciplinaryMeasures>();
            services.AddScoped<IDeleteDisciplinaryMeasure, DeleteDisciplinaryMeasure>();

            // Termination & clearance (offboarding)
            services.AddScoped<ISaveEmployeeTermination, SaveEmployeeTermination>();
            services.AddScoped<IGetEmployeeTerminations, GetEmployeeTerminations>();
            services.AddScoped<IGetTerminatedEmployees, GetTerminatedEmployees>();
            services.AddScoped<IGetMyClearances, GetMyClearances>();
            services.AddScoped<IUpdateTerminationClearance, UpdateTerminationClearance>();
            services.AddScoped<IFinalizeEmployeeTermination, FinalizeEmployeeTermination>();
            services.AddScoped<ICancelEmployeeTermination, CancelEmployeeTermination>();
            services.AddScoped<IDeleteEmployeeTermination, DeleteEmployeeTermination>();

            // Reinstatement (reverse a settled termination, restoring placement)
            services.AddScoped<IGetReinstatementInfo, GetReinstatementInfo>();
            services.AddScoped<IReinstateEmployee, ReinstateEmployee>();

            // Dynamic clearance configuration (departments + approvers)
            services.AddScoped<ISaveClearanceDepartment, SaveClearanceDepartment>();
            services.AddScoped<IGetAllClearanceDepartments, GetAllClearanceDepartments>();
            services.AddScoped<IGetClearanceDepartmentById, GetClearanceDepartmentById>();
            services.AddScoped<IDeleteClearanceDepartment, DeleteClearanceDepartment>();

            // Roles & user-role assignment (workflow approver authorization, HC025 groundwork)
            services.AddScoped<ISaveRole, SaveRole>();
            services.AddScoped<IGetAllRoles, GetAllRoles>();
            services.AddScoped<IDeleteRole, DeleteRole>();
            services.AddScoped<ISaveUserRole, SaveUserRole>();
            services.AddScoped<IGetAllUserRoles, GetAllUserRoles>();
            services.AddScoped<IDeleteUserRole, DeleteUserRole>();
            services.AddScoped<IGetAllUsers, GetAllUsers>();

            // User administration (System > Users): create / edit / delete
            services.AddScoped<Features.Core.Users.ISaveUser, Features.Core.Users.SaveUser>();
            services.AddScoped<Features.Core.Users.IGetUserById, Features.Core.Users.GetUserById>();
            services.AddScoped<Features.Core.Users.IDeleteUser, Features.Core.Users.DeleteUser>();

            // Generic workflow engine + per-module outcome handlers
            services.AddScoped<IWorkflowService, WorkflowService>();
            services.AddScoped<IWorkflowGate, WorkflowGate>();
            services.AddScoped<IWorkflowApproverAuth, WorkflowApproverAuth>();
            services.AddScoped<IOrgManagerResolver, OrgManagerResolver>();
            services.AddScoped<IWorkflowEntityHandler, EmployeeMovementWorkflowHandler>();
            services.AddScoped<IWorkflowEntityHandler, DisciplinaryMeasureWorkflowHandler>();
            services.AddScoped<IWorkflowEntityHandler, EmployeeTerminationWorkflowHandler>();
            services.AddScoped<IWorkflowEntityHandler, WorkforcePlanWorkflowHandler>();
            services.AddScoped<IWorkflowEntityHandler, HiringRequestWorkflowHandler>();
            services.AddScoped<IWorkflowEntityHandler, JobRequisitionWorkflowHandler>();
            services.AddScoped<IWorkflowEntityHandler, JobOfferWorkflowHandler>();

            // Recruitment & Talent Acquisition — Phase 1 (HC077–HC100 core)
            services.AddScoped<Features.Core.Recruitment.ISaveHiringRequest, Features.Core.Recruitment.SaveHiringRequest>();
            services.AddScoped<Features.Core.Recruitment.IGetHiringRequestById, Features.Core.Recruitment.GetHiringRequestById>();
            services.AddScoped<Features.Core.Recruitment.IGetAllHiringRequests, Features.Core.Recruitment.GetAllHiringRequests>();
            services.AddScoped<Features.Core.Recruitment.IDeleteHiringRequest, Features.Core.Recruitment.DeleteHiringRequest>();
            services.AddScoped<Features.Core.Recruitment.ISubmitHiringRequest, Features.Core.Recruitment.SubmitHiringRequest>();
            services.AddScoped<Features.Core.Recruitment.ICloseHiringRequest, Features.Core.Recruitment.CloseHiringRequest>();
            services.AddScoped<Features.Core.Recruitment.IGetRecruitmentBudgetMonitor, Features.Core.Recruitment.GetRecruitmentBudgetMonitor>();
            services.AddScoped<Features.Core.Recruitment.ISaveJobRequisition, Features.Core.Recruitment.SaveJobRequisition>();
            services.AddScoped<Features.Core.Recruitment.IGetJobRequisitionById, Features.Core.Recruitment.GetJobRequisitionById>();
            services.AddScoped<Features.Core.Recruitment.IGetAllJobRequisitions, Features.Core.Recruitment.GetAllJobRequisitions>();
            services.AddScoped<Features.Core.Recruitment.IDeleteJobRequisition, Features.Core.Recruitment.DeleteJobRequisition>();
            services.AddScoped<Features.Core.Recruitment.ISubmitJobRequisition, Features.Core.Recruitment.SubmitJobRequisition>();
            services.AddScoped<Features.Core.Recruitment.ISetRequisitionPosting, Features.Core.Recruitment.SetRequisitionPosting>();
            services.AddScoped<Features.Core.Recruitment.IGenerateRequisitionPosting, Features.Core.Recruitment.GenerateRequisitionPosting>();
            services.AddScoped<Features.Core.Recruitment.IPostJobRequisition, Features.Core.Recruitment.PostJobRequisition>();
            services.AddScoped<Features.Core.Recruitment.ICloseJobRequisition, Features.Core.Recruitment.CloseJobRequisition>();
            services.AddScoped<Features.Core.Recruitment.ICancelJobRequisition, Features.Core.Recruitment.CancelJobRequisition>();
            services.AddScoped<Features.Core.Recruitment.ISaveCandidate, Features.Core.Recruitment.SaveCandidate>();
            services.AddScoped<Features.Core.Recruitment.IGetCandidateById, Features.Core.Recruitment.GetCandidateById>();
            services.AddScoped<Features.Core.Recruitment.IGetAllCandidates, Features.Core.Recruitment.GetAllCandidates>();
            services.AddScoped<Features.Core.Recruitment.IDeleteCandidate, Features.Core.Recruitment.DeleteCandidate>();
            services.AddScoped<Features.Core.Recruitment.ISetCandidateTalentPool, Features.Core.Recruitment.SetCandidateTalentPool>();
            services.AddScoped<Features.Core.Recruitment.IAnonymizeCandidate, Features.Core.Recruitment.AnonymizeCandidate>();
            services.AddScoped<Features.Core.Recruitment.IUploadCandidateResume, Features.Core.Recruitment.UploadCandidateResume>();
            services.AddScoped<Features.Core.Recruitment.IGetCandidateResume, Features.Core.Recruitment.GetCandidateResume>();
            services.AddScoped<Features.Core.Recruitment.IMatchCandidates, Features.Core.Recruitment.MatchCandidates>();
            services.AddScoped<Features.Core.Recruitment.ICreateJobApplication, Features.Core.Recruitment.CreateJobApplication>();
            services.AddScoped<Features.Core.Recruitment.IGetJobApplicationById, Features.Core.Recruitment.GetJobApplicationById>();
            services.AddScoped<Features.Core.Recruitment.IGetAllJobApplications, Features.Core.Recruitment.GetAllJobApplications>();
            services.AddScoped<Features.Core.Recruitment.IMoveJobApplicationStage, Features.Core.Recruitment.MoveJobApplicationStage>();
            services.AddScoped<Features.Core.Recruitment.IScoreJobApplication, Features.Core.Recruitment.ScoreJobApplication>();
            services.AddScoped<Features.Core.Recruitment.IGetApplicationRanking, Features.Core.Recruitment.GetApplicationRanking>();
            services.AddScoped<Features.Core.Recruitment.IUploadCandidateDocument, Features.Core.Recruitment.UploadCandidateDocument>();
            services.AddScoped<Features.Core.Recruitment.IGetCandidateDocuments, Features.Core.Recruitment.GetCandidateDocuments>();
            services.AddScoped<Features.Core.Recruitment.IDownloadCandidateDocument, Features.Core.Recruitment.DownloadCandidateDocument>();
            services.AddScoped<Features.Core.Recruitment.IDeleteCandidateDocument, Features.Core.Recruitment.DeleteCandidateDocument>();
            services.AddScoped<Features.Core.Recruitment.IHireCandidate, Features.Core.Recruitment.HireCandidate>();
            // Candidate structured background (writes the same person-owned education/experience rows the employee uses)
            services.AddScoped<Features.Core.Recruitment.IGetCandidateEducations, Features.Core.Recruitment.GetCandidateEducations>();
            services.AddScoped<Features.Core.Recruitment.ISaveCandidateEducation, Features.Core.Recruitment.SaveCandidateEducation>();
            services.AddScoped<Features.Core.Recruitment.IDeleteCandidateEducation, Features.Core.Recruitment.DeleteCandidateEducation>();
            services.AddScoped<Features.Core.Recruitment.IGetCandidateExperiences, Features.Core.Recruitment.GetCandidateExperiences>();
            services.AddScoped<Features.Core.Recruitment.ISaveCandidateExperience, Features.Core.Recruitment.SaveCandidateExperience>();
            services.AddScoped<Features.Core.Recruitment.IDeleteCandidateExperience, Features.Core.Recruitment.DeleteCandidateExperience>();
            services.AddScoped<Features.Core.Recruitment.IUploadCandidateBackgroundDocument, Features.Core.Recruitment.UploadCandidateBackgroundDocument>();
            services.AddScoped<Features.Core.Recruitment.IGetCandidateBackgroundDocuments, Features.Core.Recruitment.GetCandidateBackgroundDocuments>();
            services.AddScoped<Features.Core.Recruitment.IDownloadCandidateBackgroundDocument, Features.Core.Recruitment.DownloadCandidateBackgroundDocument>();
            services.AddScoped<Features.Core.Recruitment.IDeleteCandidateBackgroundDocument, Features.Core.Recruitment.DeleteCandidateBackgroundDocument>();
            // Recruitment Phase 2 — interviews & panels (HC101–HC109)
            services.AddScoped<Features.Core.Recruitment.ISaveInterview, Features.Core.Recruitment.SaveInterview>();
            services.AddScoped<Features.Core.Recruitment.IGetInterviews, Features.Core.Recruitment.GetInterviews>();
            services.AddScoped<Features.Core.Recruitment.ISetInterviewStatus, Features.Core.Recruitment.SetInterviewStatus>();
            services.AddScoped<Features.Core.Recruitment.ISubmitInterviewFeedback, Features.Core.Recruitment.SubmitInterviewFeedback>();
            services.AddScoped<Features.Core.Recruitment.IGetInterviewConsolidated, Features.Core.Recruitment.GetInterviewConsolidated>();
            services.AddScoped<Features.Core.Recruitment.IDeleteInterview, Features.Core.Recruitment.DeleteInterview>();
            // Recruitment Phase 2 — offers (HC111–HC114)
            services.AddScoped<Features.Core.Recruitment.ISaveJobOffer, Features.Core.Recruitment.SaveJobOffer>();
            services.AddScoped<Features.Core.Recruitment.IGetJobOffers, Features.Core.Recruitment.GetJobOffers>();
            services.AddScoped<Features.Core.Recruitment.IGetOfferDefaults, Features.Core.Recruitment.GetOfferDefaults>();
            services.AddScoped<Features.Core.Recruitment.IOfferDelivery, Features.Core.Recruitment.OfferDelivery>();
            services.AddScoped<Features.Core.Recruitment.IOfferLetterComposer, Features.Core.Recruitment.OfferLetterComposer>();
            services.AddScoped<Features.Core.Recruitment.IGetOfferLetterTemplate, Features.Core.Recruitment.GetOfferLetterTemplate>();
            services.AddScoped<Features.Core.Recruitment.ISaveOfferLetterTemplate, Features.Core.Recruitment.SaveOfferLetterTemplate>();
            services.AddScoped<Features.Core.Recruitment.IGetCompanyProfile, Features.Core.Recruitment.GetCompanyProfile>();
            services.AddScoped<Features.Core.Recruitment.ISaveCompanyProfile, Features.Core.Recruitment.SaveCompanyProfile>();
            services.AddScoped<Features.Core.Recruitment.IGetOfferMergeFields, Features.Core.Recruitment.GetOfferMergeFields>();
            services.AddScoped<Features.Core.Recruitment.IPreviewOfferLetter, Features.Core.Recruitment.PreviewOfferLetter>();
            services.AddScoped<Features.Core.Recruitment.ISubmitJobOffer, Features.Core.Recruitment.SubmitJobOffer>();
            services.AddScoped<Features.Core.Recruitment.ISendJobOffer, Features.Core.Recruitment.SendJobOffer>();
            services.AddScoped<Features.Core.Recruitment.IRespondJobOffer, Features.Core.Recruitment.RespondJobOffer>();
            services.AddScoped<Features.Core.Recruitment.IWithdrawJobOffer, Features.Core.Recruitment.WithdrawJobOffer>();
            services.AddScoped<Features.Core.Recruitment.IGenerateOfferLetter, Features.Core.Recruitment.GenerateOfferLetter>();
            services.AddScoped<Features.Core.Recruitment.IDeleteJobOffer, Features.Core.Recruitment.DeleteJobOffer>();
            services.AddScoped<Features.Core.Recruitment.IGetHireQueue, Features.Core.Recruitment.GetHireQueue>();
            services.AddScoped<Features.Core.Recruitment.IAdoptInterviewScores, Features.Core.Recruitment.AdoptInterviewScores>();
            services.AddScoped<Features.Core.Recruitment.IGetEvaluatorContext, Features.Core.Recruitment.GetEvaluatorContext>();
            services.AddScoped<Features.Core.Recruitment.IBulkMoveApplicationStage, Features.Core.Recruitment.BulkMoveApplicationStage>();
            services.AddScoped<Features.Core.Recruitment.IInterviewNotifier, Features.Core.Recruitment.InterviewNotifier>();

            // Workforce Planning (HC053–HC076)
            services.AddScoped<Features.Core.WorkforcePlans.ISaveWorkforcePlan, Features.Core.WorkforcePlans.SaveWorkforcePlan>();
            services.AddScoped<Features.Core.WorkforcePlans.IGetWorkforcePlanById, Features.Core.WorkforcePlans.GetWorkforcePlanById>();
            services.AddScoped<Features.Core.WorkforcePlans.IGetAllWorkforcePlans, Features.Core.WorkforcePlans.GetAllWorkforcePlans>();
            services.AddScoped<Features.Core.WorkforcePlans.IDeleteWorkforcePlan, Features.Core.WorkforcePlans.DeleteWorkforcePlan>();
            services.AddScoped<Features.Core.WorkforcePlans.ISubmitWorkforcePlan, Features.Core.WorkforcePlans.SubmitWorkforcePlan>();
            services.AddScoped<Features.Core.WorkforcePlans.ICreateWorkforcePlanVersion, Features.Core.WorkforcePlans.CreateWorkforcePlanVersion>();
            services.AddScoped<Features.Core.WorkforcePlans.IGetEstablishmentOverview, Features.Core.WorkforcePlans.GetEstablishmentOverview>();
            services.AddScoped<Features.Core.WorkforcePlans.IPopulateWorkforcePlan, Features.Core.WorkforcePlans.PopulateWorkforcePlan>();
            services.AddScoped<Features.Core.WorkforcePlans.ISuggestPlanSeparations, Features.Core.WorkforcePlans.SuggestPlanSeparations>();
            services.AddScoped<Features.Core.WorkforcePlans.IGetWorkforcePlanSummary, Features.Core.WorkforcePlans.GetWorkforcePlanSummary>();
            services.AddScoped<Features.Core.WorkforcePlans.ICompareWorkforcePlans, Features.Core.WorkforcePlans.CompareWorkforcePlans>();
            services.AddScoped<Features.Core.WorkforcePlans.IGetApprovedDemand, Features.Core.WorkforcePlans.GetApprovedDemand>();
            services.AddScoped<IGetAllWorkflowInstances, GetAllWorkflowInstances>();
            services.AddScoped<IGetMyApprovals, GetMyApprovals>();
            services.AddScoped<IGetWorkflowStats, GetWorkflowStats>();
            services.AddScoped<IGetWorkflowActions, GetWorkflowActions>();
            services.AddScoped<ISaveWorkflowDefinition, SaveWorkflowDefinition>();
            services.AddScoped<IGetAllWorkflowDefinitions, GetAllWorkflowDefinitions>();
            services.AddScoped<IGetWorkflowDefinitionById, GetWorkflowDefinitionById>();
            services.AddScoped<IDeleteWorkflowDefinition, DeleteWorkflowDefinition>();
            services.AddScoped<ISeedDefaultWorkflows, SeedDefaultWorkflows>();
            services.AddScoped<ICreateEmployeeField, CreateEmployeeField>();
            services.AddScoped<IUpdateEmployeeField, UpdateEmployeeField>();
            services.AddScoped<IDeleteEmployeeField, DeleteEmployeeField>();
            services.AddScoped<IGetEmployeeFieldById, GetEmployeeFieldById>();
            services.AddScoped<IGetAllEmployeeFields, GetAllEmployeeFields>();
            // Shared custom-field engine (HC021) — used by the Employee form and all child forms.
            services.AddScoped<ICustomFieldService, CustomFieldService>();
            // Dynamic form/tab builder (user-defined custom tabs) — reusable across modules.
            services.AddScoped<Features.Core.DynamicForms.IDynamicFormService, Features.Core.DynamicForms.DynamicFormService>();

            // Performance Management (HC118–HC147) — Phase A: configuration foundation.
            AddPerformanceManagement(services);

            // Career Development (HC148–HC169) — Phase 3.7.A: Succession Planning (HC148–HC160).
            AddCareerDevelopment(services);

            // Document templates & correspondence (HC022)
            services.AddScoped<ICreateDocumentTemplate, CreateDocumentTemplate>();
            services.AddScoped<IUpdateDocumentTemplate, UpdateDocumentTemplate>();
            services.AddScoped<IDeleteDocumentTemplate, DeleteDocumentTemplate>();
            services.AddScoped<IGetDocumentTemplateById, GetDocumentTemplateById>();
            services.AddScoped<IGetAllDocumentTemplates, GetAllDocumentTemplates>();
            services.AddScoped<ISeedDefaultDocumentTemplates, SeedDefaultDocumentTemplates>();
            services.AddScoped<IGetDocumentMergeFields, GetDocumentMergeFields>();
            services.AddScoped<IGenerateEmployeeDocument, GenerateEmployeeDocument>();
            services.AddScoped<IGenerateAnnualLeaveDocument, GenerateAnnualLeaveDocument>();

            // Generic report engine (SP-driven; ported from the reference APSmart module).
            services.AddScoped<Features.Core.Reports.IGetReportCatalog, Features.Core.Reports.GetReportCatalog>();
            services.AddScoped<Features.Core.Reports.IGetReportSchema, Features.Core.Reports.GetReportSchema>();
            services.AddScoped<Features.Core.Reports.IGetReportFieldValues, Features.Core.Reports.GetReportFieldValues>();
            services.AddScoped<Features.Core.Reports.IGenerateReport, Features.Core.Reports.GenerateReport>();
            services.AddScoped<Features.Core.Reports.ISaveReport, Features.Core.Reports.SaveReport>();
            services.AddScoped<Features.Core.Reports.IDeleteReport, Features.Core.Reports.DeleteReport>();
            services.AddScoped<Features.Core.Reports.IGetReportById, Features.Core.Reports.GetReportById>();
            services.AddScoped<Features.Core.Reports.IGetAllReports, Features.Core.Reports.GetAllReports>();
            services.AddScoped<Features.Core.Reports.ISaveReportFilter, Features.Core.Reports.SaveReportFilter>();
            services.AddScoped<Features.Core.Reports.IGetReportFilter, Features.Core.Reports.GetReportFilter>();
            services.AddScoped<Features.Core.Reports.IDeleteReportFilter, Features.Core.Reports.DeleteReportFilter>();
            services.AddScoped<Features.Core.Reports.IGetReportHistory, Features.Core.Reports.GetReportHistory>();
            services.AddScoped<Features.Core.Reports.ISaveReportSchedule, Features.Core.Reports.SaveReportSchedule>();
            services.AddScoped<Features.Core.Reports.IGetReportSchedules, Features.Core.Reports.GetReportSchedules>();
            services.AddScoped<Features.Core.Reports.IDeleteReportSchedule, Features.Core.Reports.DeleteReportSchedule>();
            services.AddScoped<Features.Core.Reports.IRunReportSchedule, Features.Core.Reports.RunReportSchedule>();
            services.AddScoped<Features.Core.Reports.IGetReportScheduleDetail, Features.Core.Reports.GetReportScheduleDetail>();
            services.AddScoped<Features.Core.Reports.ISetReportScheduleEnabled, Features.Core.Reports.SetReportScheduleEnabled>();
            services.AddScoped<Features.Core.Reports.ISetReportActive, Features.Core.Reports.SetReportActive>();
            services.AddScoped<Features.Core.Reports.IReportAccessGuard, Features.Core.Reports.ReportAccessGuard>();
            services.AddScoped<Features.Core.Reports.IEmailGeneratedReport, Features.Core.Reports.EmailGeneratedReport>();
            services.AddScoped<Features.Core.Reports.ISetReportRestrictions, Features.Core.Reports.SetReportRestrictions>();
            services.AddScoped<IUploadCompanyLogo, UploadCompanyLogo>();
            services.AddScoped<IGetCompanyLogo, GetCompanyLogo>();
            services.AddScoped<IGetCompanyLogoInfo, GetCompanyLogoInfo>();
            services.AddScoped<IDeleteCompanyLogo, DeleteCompanyLogo>();
        }

        // Performance Management (HC118–HC147) — Phase A: rating scales, competency library, position
        // competencies, review cycles, appraisal templates.
        private static void AddPerformanceManagement(IServiceCollection services)
        {
            services.AddScoped<Features.Core.Performance.ICreateCompetencyCategory, Features.Core.Performance.CreateCompetencyCategory>();
            services.AddScoped<Features.Core.Performance.IUpdateCompetencyCategory, Features.Core.Performance.UpdateCompetencyCategory>();
            services.AddScoped<Features.Core.Performance.IDeleteCompetencyCategory, Features.Core.Performance.DeleteCompetencyCategory>();
            services.AddScoped<Features.Core.Performance.IGetCompetencyCategoryById, Features.Core.Performance.GetCompetencyCategoryById>();
            services.AddScoped<Features.Core.Performance.IGetAllCompetencyCategories, Features.Core.Performance.GetAllCompetencyCategories>();

            services.AddScoped<Features.Core.Performance.ICreateCompetency, Features.Core.Performance.CreateCompetency>();
            services.AddScoped<Features.Core.Performance.IUpdateCompetency, Features.Core.Performance.UpdateCompetency>();
            services.AddScoped<Features.Core.Performance.IDeleteCompetency, Features.Core.Performance.DeleteCompetency>();
            services.AddScoped<Features.Core.Performance.IGetCompetencyById, Features.Core.Performance.GetCompetencyById>();
            services.AddScoped<Features.Core.Performance.IGetAllCompetencies, Features.Core.Performance.GetAllCompetencies>();

            services.AddScoped<Features.Core.Performance.IGetPositionCompetencies, Features.Core.Performance.GetPositionCompetencies>();
            services.AddScoped<Features.Core.Performance.ISavePositionCompetencies, Features.Core.Performance.SavePositionCompetencies>();

            services.AddScoped<Features.Core.Performance.ISaveRatingScale, Features.Core.Performance.SaveRatingScale>();
            services.AddScoped<Features.Core.Performance.IDeleteRatingScale, Features.Core.Performance.DeleteRatingScale>();
            services.AddScoped<Features.Core.Performance.IGetRatingScaleById, Features.Core.Performance.GetRatingScaleById>();
            services.AddScoped<Features.Core.Performance.IGetAllRatingScales, Features.Core.Performance.GetAllRatingScales>();

            services.AddScoped<Features.Core.Performance.ISaveReviewCycle, Features.Core.Performance.SaveReviewCycle>();
            services.AddScoped<Features.Core.Performance.IDeleteReviewCycle, Features.Core.Performance.DeleteReviewCycle>();
            services.AddScoped<Features.Core.Performance.IGetReviewCycleById, Features.Core.Performance.GetReviewCycleById>();
            services.AddScoped<Features.Core.Performance.IGetAllReviewCycles, Features.Core.Performance.GetAllReviewCycles>();

            services.AddScoped<Features.Core.Performance.ICreateAppraisalTemplate, Features.Core.Performance.CreateAppraisalTemplate>();
            services.AddScoped<Features.Core.Performance.IUpdateAppraisalTemplate, Features.Core.Performance.UpdateAppraisalTemplate>();
            services.AddScoped<Features.Core.Performance.IDeleteAppraisalTemplate, Features.Core.Performance.DeleteAppraisalTemplate>();
            services.AddScoped<Features.Core.Performance.IGetAppraisalTemplateById, Features.Core.Performance.GetAppraisalTemplateById>();
            services.AddScoped<Features.Core.Performance.IGetAllAppraisalTemplates, Features.Core.Performance.GetAllAppraisalTemplates>();

            // Phase B — objectives & goals (HC118–HC122)
            services.AddScoped<Features.Core.Performance.ISaveOrganizationalObjective, Features.Core.Performance.SaveOrganizationalObjective>();
            services.AddScoped<Features.Core.Performance.IDeleteOrganizationalObjective, Features.Core.Performance.DeleteOrganizationalObjective>();
            services.AddScoped<Features.Core.Performance.IGetOrganizationalObjectiveById, Features.Core.Performance.GetOrganizationalObjectiveById>();
            services.AddScoped<Features.Core.Performance.IGetAllOrganizationalObjectives, Features.Core.Performance.GetAllOrganizationalObjectives>();
            services.AddScoped<Features.Core.Performance.ISaveEmployeeGoal, Features.Core.Performance.SaveEmployeeGoal>();
            services.AddScoped<Features.Core.Performance.IDeleteEmployeeGoal, Features.Core.Performance.DeleteEmployeeGoal>();
            services.AddScoped<Features.Core.Performance.IGetEmployeeGoalById, Features.Core.Performance.GetEmployeeGoalById>();
            services.AddScoped<Features.Core.Performance.IGetAllEmployeeGoals, Features.Core.Performance.GetAllEmployeeGoals>();

            // Phase C1 — scored appraisals (HC127/HC138)
            services.AddScoped<Features.Core.Performance.IGenerateAppraisal, Features.Core.Performance.GenerateAppraisal>();
            services.AddScoped<Features.Core.Performance.ISaveAppraisalScores, Features.Core.Performance.SaveAppraisalScores>();
            services.AddScoped<Features.Core.Performance.ISubmitAppraisalSelfAssessment, Features.Core.Performance.SubmitAppraisalSelfAssessment>();
            services.AddScoped<Features.Core.Performance.ICompleteAppraisal, Features.Core.Performance.CompleteAppraisal>();
            services.AddScoped<Features.Core.Performance.IDeleteAppraisal, Features.Core.Performance.DeleteAppraisal>();
            services.AddScoped<Features.Core.Performance.IGetAppraisalById, Features.Core.Performance.GetAppraisalById>();
            services.AddScoped<Features.Core.Performance.IGetAllAppraisals, Features.Core.Performance.GetAllAppraisals>();

            // Phase D3 — acknowledgment / signing + appeals (HC142–144, HC146)
            services.AddScoped<Features.Core.Performance.IAcknowledgeAppraisal, Features.Core.Performance.AcknowledgeAppraisal>();
            services.AddScoped<Features.Core.Performance.IManagerSignAppraisal, Features.Core.Performance.ManagerSignAppraisal>();
            services.AddScoped<Features.Core.Performance.ISubmitAppraisalAppeal, Features.Core.Performance.SubmitAppraisalAppeal>();
            services.AddScoped<Features.Core.Performance.IStartAppraisalAppealReview, Features.Core.Performance.StartAppraisalAppealReview>();
            services.AddScoped<Features.Core.Performance.IResolveAppraisalAppeal, Features.Core.Performance.ResolveAppraisalAppeal>();
            services.AddScoped<Features.Core.Performance.IGetAppraisalAppealById, Features.Core.Performance.GetAppraisalAppealById>();
            services.AddScoped<Features.Core.Performance.IGetAllAppraisalAppeals, Features.Core.Performance.GetAllAppraisalAppeals>();

            // Phase C2 — peer review, calibration, version history (HC127/128/129/132)
            services.AddScoped<Features.Core.Performance.IPerformanceHistoryWriter, Features.Core.Performance.PerformanceHistoryWriter>();
            services.AddScoped<Features.Core.Performance.IGetPerformanceHistory, Features.Core.Performance.GetPerformanceHistory>();
            services.AddScoped<Features.Core.Performance.IInviteAppraisalPeers, Features.Core.Performance.InviteAppraisalPeers>();
            services.AddScoped<Features.Core.Performance.ISubmitAppraisalPeerReview, Features.Core.Performance.SubmitAppraisalPeerReview>();
            services.AddScoped<Features.Core.Performance.IRemoveAppraisalPeerReview, Features.Core.Performance.RemoveAppraisalPeerReview>();
            services.AddScoped<Features.Core.Performance.IGetAppraisalPeerReviews, Features.Core.Performance.GetAppraisalPeerReviews>();
            services.AddScoped<Features.Core.Performance.ICreateCalibrationSession, Features.Core.Performance.CreateCalibrationSession>();
            services.AddScoped<Features.Core.Performance.ISaveCalibrationItem, Features.Core.Performance.SaveCalibrationItem>();
            services.AddScoped<Features.Core.Performance.IFinalizeCalibrationSession, Features.Core.Performance.FinalizeCalibrationSession>();
            services.AddScoped<Features.Core.Performance.IDeleteCalibrationSession, Features.Core.Performance.DeleteCalibrationSession>();
            services.AddScoped<Features.Core.Performance.IGetCalibrationSessionById, Features.Core.Performance.GetCalibrationSessionById>();
            services.AddScoped<Features.Core.Performance.IGetAllCalibrationSessions, Features.Core.Performance.GetAllCalibrationSessions>();

            // Phase D1 — development plans (IDP) & improvement plans (PIP) (HC130/131/135)
            services.AddScoped<Features.Core.Performance.ISaveDevelopmentPlan, Features.Core.Performance.SaveDevelopmentPlan>();
            services.AddScoped<Features.Core.Performance.IDeleteDevelopmentPlan, Features.Core.Performance.DeleteDevelopmentPlan>();
            services.AddScoped<Features.Core.Performance.IGetDevelopmentPlanById, Features.Core.Performance.GetDevelopmentPlanById>();
            services.AddScoped<Features.Core.Performance.IGetAllDevelopmentPlans, Features.Core.Performance.GetAllDevelopmentPlans>();
            services.AddScoped<Features.Core.Performance.ISaveImprovementPlan, Features.Core.Performance.SaveImprovementPlan>();
            services.AddScoped<Features.Core.Performance.IRecordImprovementPlanOutcome, Features.Core.Performance.RecordImprovementPlanOutcome>();
            services.AddScoped<Features.Core.Performance.IDeleteImprovementPlan, Features.Core.Performance.DeleteImprovementPlan>();
            services.AddScoped<Features.Core.Performance.IGetImprovementPlanById, Features.Core.Performance.GetImprovementPlanById>();
            services.AddScoped<Features.Core.Performance.IGetAllImprovementPlans, Features.Core.Performance.GetAllImprovementPlans>();

            // Phase D2 — achievements & recognition (HC139–141)
            services.AddScoped<Features.Core.Performance.ISaveAchievement, Features.Core.Performance.SaveAchievement>();
            services.AddScoped<Features.Core.Performance.IDeleteAchievement, Features.Core.Performance.DeleteAchievement>();
            services.AddScoped<Features.Core.Performance.IGetAchievementById, Features.Core.Performance.GetAchievementById>();
            services.AddScoped<Features.Core.Performance.IGetAllAchievements, Features.Core.Performance.GetAllAchievements>();
            services.AddScoped<Features.Core.Performance.ICreateRecognitionBadge, Features.Core.Performance.CreateRecognitionBadge>();
            services.AddScoped<Features.Core.Performance.IUpdateRecognitionBadge, Features.Core.Performance.UpdateRecognitionBadge>();
            services.AddScoped<Features.Core.Performance.IDeleteRecognitionBadge, Features.Core.Performance.DeleteRecognitionBadge>();
            services.AddScoped<Features.Core.Performance.IGetRecognitionBadgeById, Features.Core.Performance.GetRecognitionBadgeById>();
            services.AddScoped<Features.Core.Performance.IGetAllRecognitionBadges, Features.Core.Performance.GetAllRecognitionBadges>();
            services.AddScoped<Features.Core.Performance.ISaveEmployeeRecognition, Features.Core.Performance.SaveEmployeeRecognition>();
            services.AddScoped<Features.Core.Performance.IDeleteEmployeeRecognition, Features.Core.Performance.DeleteEmployeeRecognition>();
            services.AddScoped<Features.Core.Performance.IGetEmployeeRecognitionById, Features.Core.Performance.GetEmployeeRecognitionById>();
            services.AddScoped<Features.Core.Performance.IGetAllEmployeeRecognitions, Features.Core.Performance.GetAllEmployeeRecognitions>();

            // Phase D4 — dashboard + unified summary (HC134, HC147)
            services.AddScoped<Features.Core.Performance.IGetPerformanceDashboard, Features.Core.Performance.GetPerformanceDashboard>();
            services.AddScoped<Features.Core.Performance.IGetEmployeePerformanceSummary, Features.Core.Performance.GetEmployeePerformanceSummary>();
        }

        // Career Development §3.7.A — Succession Planning (HC148–HC160).
        private static void AddCareerDevelopment(IServiceCollection services)
        {
            // Critical positions (HC151)
            services.AddScoped<Features.Core.CareerDevelopment.ISaveCriticalPosition, Features.Core.CareerDevelopment.SaveCriticalPosition>();
            services.AddScoped<Features.Core.CareerDevelopment.IDeleteCriticalPosition, Features.Core.CareerDevelopment.DeleteCriticalPosition>();
            services.AddScoped<Features.Core.CareerDevelopment.IGetCriticalPositionById, Features.Core.CareerDevelopment.GetCriticalPositionById>();
            services.AddScoped<Features.Core.CareerDevelopment.IGetAllCriticalPositions, Features.Core.CareerDevelopment.GetAllCriticalPositions>();

            // Talent review + 9-box (HC148–HC150)
            services.AddScoped<Features.Core.CareerDevelopment.ISaveTalentReview, Features.Core.CareerDevelopment.SaveTalentReview>();
            services.AddScoped<Features.Core.CareerDevelopment.IDeleteTalentReview, Features.Core.CareerDevelopment.DeleteTalentReview>();
            services.AddScoped<Features.Core.CareerDevelopment.IGetTalentReviewById, Features.Core.CareerDevelopment.GetTalentReviewById>();
            services.AddScoped<Features.Core.CareerDevelopment.IGetAllTalentReviews, Features.Core.CareerDevelopment.GetAllTalentReviews>();
            services.AddScoped<Features.Core.CareerDevelopment.IGetTalentReviewNineBox, Features.Core.CareerDevelopment.GetTalentReviewNineBox>();

            // Talent assessments (multi-rater 9-box placement, HC148/HC149)
            services.AddScoped<Features.Core.CareerDevelopment.ISaveTalentAssessment, Features.Core.CareerDevelopment.SaveTalentAssessment>();
            services.AddScoped<Features.Core.CareerDevelopment.IDeleteTalentAssessment, Features.Core.CareerDevelopment.DeleteTalentAssessment>();
            services.AddScoped<Features.Core.CareerDevelopment.IGetTalentAssessmentById, Features.Core.CareerDevelopment.GetTalentAssessmentById>();
            services.AddScoped<Features.Core.CareerDevelopment.IGetAllTalentAssessments, Features.Core.CareerDevelopment.GetAllTalentAssessments>();

            // Succession plans + chart (HC152, HC157, HC159)
            services.AddScoped<Features.Core.CareerDevelopment.ISaveSuccessionPlan, Features.Core.CareerDevelopment.SaveSuccessionPlan>();
            services.AddScoped<Features.Core.CareerDevelopment.IDeleteSuccessionPlan, Features.Core.CareerDevelopment.DeleteSuccessionPlan>();
            services.AddScoped<Features.Core.CareerDevelopment.IGetSuccessionPlanById, Features.Core.CareerDevelopment.GetSuccessionPlanById>();
            services.AddScoped<Features.Core.CareerDevelopment.IGetAllSuccessionPlans, Features.Core.CareerDevelopment.GetAllSuccessionPlans>();
            services.AddScoped<Features.Core.CareerDevelopment.IGetSuccessionChart, Features.Core.CareerDevelopment.GetSuccessionChart>();

            // Succession candidates + development/knowledge-transfer + competency gap (HC153–HC156, HC160)
            services.AddScoped<Features.Core.CareerDevelopment.ISaveSuccessionCandidate, Features.Core.CareerDevelopment.SaveSuccessionCandidate>();
            services.AddScoped<Features.Core.CareerDevelopment.IDeleteSuccessionCandidate, Features.Core.CareerDevelopment.DeleteSuccessionCandidate>();
            services.AddScoped<Features.Core.CareerDevelopment.IGetSuccessionCandidateById, Features.Core.CareerDevelopment.GetSuccessionCandidateById>();
            services.AddScoped<Features.Core.CareerDevelopment.IGetAllSuccessionCandidates, Features.Core.CareerDevelopment.GetAllSuccessionCandidates>();
            services.AddScoped<Features.Core.CareerDevelopment.IGetSuccessionCandidateGap, Features.Core.CareerDevelopment.GetSuccessionCandidateGap>();

            // Integration & business logic (HC148, HC153, HC158)
            services.AddScoped<Features.Core.CareerDevelopment.IComputeSuccessionCandidateReadiness, Features.Core.CareerDevelopment.ComputeSuccessionCandidateReadiness>();
            services.AddScoped<Features.Core.CareerDevelopment.IGetSuccessionCandidateProfile, Features.Core.CareerDevelopment.GetSuccessionCandidateProfile>();
            services.AddScoped<Features.Core.CareerDevelopment.IIdentifyHiPos, Features.Core.CareerDevelopment.IdentifyHiPos>();

            AddCareerPath(services);
        }

        // ===== Career Development §3.7.B — Career Path (HC161–HC169) =====
        private static void AddCareerPath(IServiceCollection services)
        {
            // Career path (definition).
            services.AddScoped<Features.Core.CareerDevelopment.ISaveCareerPath, Features.Core.CareerDevelopment.SaveCareerPath>();
            services.AddScoped<Features.Core.CareerDevelopment.IDeleteCareerPath, Features.Core.CareerDevelopment.DeleteCareerPath>();
            services.AddScoped<Features.Core.CareerDevelopment.IGetCareerPathById, Features.Core.CareerDevelopment.GetCareerPathById>();
            services.AddScoped<Features.Core.CareerDevelopment.IGetAllCareerPaths, Features.Core.CareerDevelopment.GetAllCareerPaths>();

            // Career path step (+ required competencies).
            services.AddScoped<Features.Core.CareerDevelopment.ISaveCareerPathStep, Features.Core.CareerDevelopment.SaveCareerPathStep>();
            services.AddScoped<Features.Core.CareerDevelopment.IDeleteCareerPathStep, Features.Core.CareerDevelopment.DeleteCareerPathStep>();
            services.AddScoped<Features.Core.CareerDevelopment.IGetCareerPathStepById, Features.Core.CareerDevelopment.GetCareerPathStepById>();
            services.AddScoped<Features.Core.CareerDevelopment.IGetAllCareerPathSteps, Features.Core.CareerDevelopment.GetAllCareerPathSteps>();

            // Employee career path assignment (+ step progress).
            services.AddScoped<Features.Core.CareerDevelopment.ISaveEmployeeCareerPath, Features.Core.CareerDevelopment.SaveEmployeeCareerPath>();
            services.AddScoped<Features.Core.CareerDevelopment.IDeleteEmployeeCareerPath, Features.Core.CareerDevelopment.DeleteEmployeeCareerPath>();
            services.AddScoped<Features.Core.CareerDevelopment.IGetEmployeeCareerPathById, Features.Core.CareerDevelopment.GetEmployeeCareerPathById>();
            services.AddScoped<Features.Core.CareerDevelopment.IGetAllEmployeeCareerPaths, Features.Core.CareerDevelopment.GetAllEmployeeCareerPaths>();

            // Mentorship.
            services.AddScoped<Features.Core.CareerDevelopment.ISaveMentorship, Features.Core.CareerDevelopment.SaveMentorship>();
            services.AddScoped<Features.Core.CareerDevelopment.IDeleteMentorship, Features.Core.CareerDevelopment.DeleteMentorship>();
            services.AddScoped<Features.Core.CareerDevelopment.IGetMentorshipById, Features.Core.CareerDevelopment.GetMentorshipById>();
            services.AddScoped<Features.Core.CareerDevelopment.IGetAllMentorships, Features.Core.CareerDevelopment.GetAllMentorships>();

            // Career path change request (+ submit/approve/reject).
            services.AddScoped<Features.Core.CareerDevelopment.ISaveCareerPathChangeRequest, Features.Core.CareerDevelopment.SaveCareerPathChangeRequest>();
            services.AddScoped<Features.Core.CareerDevelopment.IDeleteCareerPathChangeRequest, Features.Core.CareerDevelopment.DeleteCareerPathChangeRequest>();
            services.AddScoped<Features.Core.CareerDevelopment.IGetCareerPathChangeRequestById, Features.Core.CareerDevelopment.GetCareerPathChangeRequestById>();
            services.AddScoped<Features.Core.CareerDevelopment.IGetAllCareerPathChangeRequests, Features.Core.CareerDevelopment.GetAllCareerPathChangeRequests>();
            services.AddScoped<Features.Core.CareerDevelopment.ISubmitCareerPathChangeRequest, Features.Core.CareerDevelopment.SubmitCareerPathChangeRequest>();
            services.AddScoped<Features.Core.CareerDevelopment.IApproveCareerPathChangeRequest, Features.Core.CareerDevelopment.ApproveCareerPathChangeRequest>();
            services.AddScoped<Features.Core.CareerDevelopment.IRejectCareerPathChangeRequest, Features.Core.CareerDevelopment.RejectCareerPathChangeRequest>();

            // Analytics.
            services.AddScoped<Features.Core.CareerDevelopment.IVisualizeCareerPath, Features.Core.CareerDevelopment.VisualizeCareerPath>();
            services.AddScoped<Features.Core.CareerDevelopment.IGetCareerPathUtilization, Features.Core.CareerDevelopment.GetCareerPathUtilization>();

            // Step 4 — integration & business logic (HC163/HC164/HC167/HC169).
            services.AddScoped<Features.Core.CareerDevelopment.ISuggestCareerPaths, Features.Core.CareerDevelopment.SuggestCareerPaths>();
            services.AddScoped<Features.Core.CareerDevelopment.IGetCareerPathRecommendations, Features.Core.CareerDevelopment.GetCareerPathRecommendations>();
            services.AddScoped<Features.Core.CareerDevelopment.ICreateDevelopmentGoals, Features.Core.CareerDevelopment.CreateDevelopmentGoals>();
            services.AddScoped<IWorkflowEntityHandler, Features.Core.CareerDevelopment.CareerPathChangeRequestWorkflowHandler>();

            // Employee 360 development profile (Performance ↔ Career Development bridge, HC158).
            services.AddScoped<Features.Core.CareerDevelopment.IGetEmployeeDevelopmentProfile, Features.Core.CareerDevelopment.GetEmployeeDevelopmentProfile>();

            // Gap → Individual Development Plan (HC130/HC155) + auto-refresh readiness on appraisal completion (HC153).
            services.AddScoped<Features.Core.CareerDevelopment.ICreateDevelopmentPlanFromGap, Features.Core.CareerDevelopment.CreateDevelopmentPlanFromGap>();
            services.AddScoped<Features.Core.Performance.IAppraisalCompletedHandler, Features.Core.CareerDevelopment.SuccessionReadinessRefreshHandler>();
        }
    }
}
