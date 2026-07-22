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
            services.AddScoped<IGetMyEmployee, GetMyEmployee>();
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
            services.AddScoped<IGetAllEmployeeMovements, GetAllEmployeeMovements>();
            services.AddScoped<IGetEmployeeMovementById, GetEmployeeMovementById>();
            services.AddScoped<IAssessEmployeeTransfer, AssessEmployeeTransfer>();
            services.AddScoped<IMovementNotifier, MovementNotifier>();
            services.AddScoped<IExecuteEmployeeMovement, ExecuteEmployeeMovement>();
            services.AddScoped<IApproveEmployeeMovement, ApproveEmployeeMovement>();
            services.AddScoped<IExecuteDueMovements, ExecuteDueMovements>();
            services.AddScoped<ICancelEmployeeMovement, CancelEmployeeMovement>();
            services.AddScoped<IDeleteEmployeeMovement, DeleteEmployeeMovement>();
            services.AddScoped<ISaveDisciplinaryMeasure, SaveDisciplinaryMeasure>();
            services.AddScoped<IGetDisciplinaryMeasures, GetDisciplinaryMeasures>();
            services.AddScoped<IGetDisciplinaryMeasureById, GetDisciplinaryMeasureById>();
            services.AddScoped<IGetDisciplinaryCases, GetDisciplinaryCases>();
            services.AddScoped<IGetDisciplinaryEligibility, GetDisciplinaryEligibility>();
            services.AddScoped<IDeleteDisciplinaryMeasure, DeleteDisciplinaryMeasure>();
            services.AddScoped<IDisciplinaryNotifier, DisciplinaryNotifier>();
            services.AddScoped<IDisciplinaryEligibilityService, DisciplinaryEligibilityService>();

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
            services.AddScoped<IWorkflowEntityHandler, SalaryRevisionWorkflowHandler>();
            services.AddScoped<IWorkflowEntityHandler, MedicalClaimWorkflowHandler>();
            services.AddScoped<IWorkflowEntityHandler, InsuranceClaimWorkflowHandler>();
            services.AddScoped<IWorkflowEntityHandler, LoanWorkflowHandler>();
            services.AddScoped<IWorkflowEntityHandler, TripRequestWorkflowHandler>();
            services.AddScoped<IWorkflowEntityHandler, EmployeeTerminationWorkflowHandler>();
            services.AddScoped<IWorkflowEntityHandler, WorkforcePlanWorkflowHandler>();
            services.AddScoped<IWorkflowEntityHandler, HiringRequestWorkflowHandler>();
            services.AddScoped<IWorkflowEntityHandler, JobRequisitionWorkflowHandler>();
            services.AddScoped<IWorkflowEntityHandler, JobOfferWorkflowHandler>();
            services.AddScoped<IWorkflowEntityHandler, Features.Core.Performance.AppraisalWorkflowHandler>();

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
            services.AddScoped<IGenerateMovementDocument, GenerateMovementDocument>();

            // Generic report engine (SP-driven; ported from the reference APSmart module).
            services.AddScoped<Features.Core.Reports.IGetReportCatalog, Features.Core.Reports.GetReportCatalog>();
            services.AddScoped<Features.Core.Reports.IGetReportSchema, Features.Core.Reports.GetReportSchema>();
            services.AddScoped<Features.Core.Reports.IGetReportFieldValues, Features.Core.Reports.GetReportFieldValues>();
            services.AddScoped<Features.Core.Reports.IGenerateReport, Features.Core.Reports.GenerateReport>();
            services.AddScoped<Features.Core.Reports.ISaveReport, Features.Core.Reports.SaveReport>();
            services.AddScoped<Features.Core.Reports.ISeedDefaultReports, Features.Core.Reports.SeedDefaultReports>();
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
            services.AddScoped<Features.Core.Performance.IPerformanceVisibilityService, Features.Core.Performance.PerformanceVisibilityService>();
            services.AddScoped<Features.Core.Performance.IGetEmployeeOptions, Features.Core.Performance.GetEmployeeOptions>();
            services.AddScoped<Features.Core.Performance.IAppraisalWorkflowService, Features.Core.Performance.AppraisalWorkflowService>();
            services.AddScoped<Features.Core.Performance.ICompleteAppraisal, Features.Core.Performance.CompleteAppraisal>();
            services.AddScoped<Features.Core.Performance.IReviewerSignOffAppraisal, Features.Core.Performance.ReviewerSignOffAppraisal>();
            services.AddScoped<Features.Core.Performance.IHrCloseAppraisal, Features.Core.Performance.HrCloseAppraisal>();
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
            services.AddScoped<Features.Core.Performance.IGetMyPeerReviews, Features.Core.Performance.GetMyPeerReviews>();
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
            services.AddScoped<Features.Core.CareerDevelopment.ISuggestSuccessionCandidates, Features.Core.CareerDevelopment.SuggestSuccessionCandidates>();

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
            // HC160 — succession plans route through the generic approval engine when a definition is active.
            services.AddScoped<IWorkflowEntityHandler, Features.Core.CareerDevelopment.SuccessionPlanWorkflowHandler>();
            // HC149 — talent-review sessions likewise (approve → calibration begins, reject → resubmittable).
            services.AddScoped<IWorkflowEntityHandler, Features.Core.CareerDevelopment.TalentReviewWorkflowHandler>();

            // Employee 360 development profile (Performance ↔ Career Development bridge, HC158).
            services.AddScoped<Features.Core.CareerDevelopment.IGetEmployeeDevelopmentProfile, Features.Core.CareerDevelopment.GetEmployeeDevelopmentProfile>();

            // Gap → Individual Development Plan (HC130/HC155) + auto-refresh readiness on appraisal completion (HC153).
            services.AddScoped<Features.Core.CareerDevelopment.ICreateDevelopmentPlanFromGap, Features.Core.CareerDevelopment.CreateDevelopmentPlanFromGap>();
            services.AddScoped<Features.Core.Performance.IAppraisalCompletedHandler, Features.Core.CareerDevelopment.SuccessionReadinessRefreshHandler>();

            // Reward & Recognition (§3.7.4, HC177–HC186).
            // §3.10.1 Compensation & Benefit — CB1 (allowance catalogue + per-employee allowances)
            services.AddScoped<Features.Core.Compensation.ISaveAllowanceType, Features.Core.Compensation.SaveAllowanceType>();
            services.AddScoped<Features.Core.Compensation.IDeleteAllowanceType, Features.Core.Compensation.DeleteAllowanceType>();
            services.AddScoped<Features.Core.Compensation.IGetAllowanceTypeById, Features.Core.Compensation.GetAllowanceTypeById>();
            services.AddScoped<Features.Core.Compensation.IGetAllAllowanceTypes, Features.Core.Compensation.GetAllAllowanceTypes>();
            services.AddScoped<Features.Core.Compensation.ISaveEmployeeAllowance, Features.Core.Compensation.SaveEmployeeAllowance>();
            services.AddScoped<Features.Core.Compensation.IGetEmployeeAllowances, Features.Core.Compensation.GetEmployeeAllowances>();
            services.AddScoped<Features.Core.Compensation.IDeleteEmployeeAllowance, Features.Core.Compensation.DeleteEmployeeAllowance>();
            services.AddScoped<Features.Core.Compensation.IGetCompensationSummary, Features.Core.Compensation.GetCompensationSummary>();
            // CB2 — salary revision planning + simulation (HC228)
            services.AddScoped<Features.Core.Compensation.ISimulateSalaryRevision, Features.Core.Compensation.SimulateSalaryRevision>();
            services.AddScoped<Features.Core.Compensation.ISaveSalaryRevision, Features.Core.Compensation.SaveSalaryRevision>();
            services.AddScoped<Features.Core.Compensation.IGetSalaryRevisionById, Features.Core.Compensation.GetSalaryRevisionById>();
            services.AddScoped<Features.Core.Compensation.IGetAllSalaryRevisions, Features.Core.Compensation.GetAllSalaryRevisions>();
            services.AddScoped<Features.Core.Compensation.ISetSalaryRevisionLine, Features.Core.Compensation.SetSalaryRevisionLine>();
            services.AddScoped<Features.Core.Compensation.ISubmitSalaryRevision, Features.Core.Compensation.SubmitSalaryRevision>();
            services.AddScoped<Features.Core.Compensation.IApproveSalaryRevision, Features.Core.Compensation.ApproveSalaryRevision>();
            services.AddScoped<Features.Core.Compensation.IApplySalaryRevision, Features.Core.Compensation.ApplySalaryRevision>();
            services.AddScoped<Features.Core.Compensation.IDeleteSalaryRevision, Features.Core.Compensation.DeleteSalaryRevision>();
            // CB3 — benefit plans, enrollment, tax config, deductions (HC230–232)
            services.AddScoped<Features.Core.Compensation.ISaveBenefitPlan, Features.Core.Compensation.SaveBenefitPlan>();
            services.AddScoped<Features.Core.Compensation.IDeleteBenefitPlan, Features.Core.Compensation.DeleteBenefitPlan>();
            services.AddScoped<Features.Core.Compensation.IGetBenefitPlanById, Features.Core.Compensation.GetBenefitPlanById>();
            services.AddScoped<Features.Core.Compensation.IGetAllBenefitPlans, Features.Core.Compensation.GetAllBenefitPlans>();
            services.AddScoped<Features.Core.Compensation.IEnrollBenefit, Features.Core.Compensation.EnrollBenefit>();
            services.AddScoped<Features.Core.Compensation.IWaiveBenefit, Features.Core.Compensation.WaiveBenefit>();
            services.AddScoped<Features.Core.Compensation.ITerminateBenefit, Features.Core.Compensation.TerminateBenefit>();
            services.AddScoped<Features.Core.Compensation.IGetEmployeeBenefits, Features.Core.Compensation.GetEmployeeBenefits>();
            services.AddScoped<Features.Core.Compensation.ISaveTaxBracket, Features.Core.Compensation.SaveTaxBracket>();
            services.AddScoped<Features.Core.Compensation.IDeleteTaxBracket, Features.Core.Compensation.DeleteTaxBracket>();
            services.AddScoped<Features.Core.Compensation.IGetAllTaxBrackets, Features.Core.Compensation.GetAllTaxBrackets>();
            services.AddScoped<Features.Core.Compensation.IGetTaxBracketById, Features.Core.Compensation.GetTaxBracketById>();

            // Per-operation endpoint authorization (enforces RolePermission.CanView on [RequirePermission] actions)
            services.AddScoped<Common.Authorization.IEndpointPermissionService, Common.Authorization.EndpointPermissionService>();
            services.AddScoped<Features.Core.Compensation.IGetPayrollDeductions, Features.Core.Compensation.GetPayrollDeductions>();
            // CB4 — employee self-service (HC233/234)
            services.AddScoped<Features.Core.Compensation.IGetMyCompensation, Features.Core.Compensation.GetMyCompensation>();
            services.AddScoped<Features.Core.Compensation.ISubmitCompensationRequest, Features.Core.Compensation.SubmitCompensationRequest>();
            services.AddScoped<Features.Core.Compensation.IGetCompensationRequests, Features.Core.Compensation.GetCompensationRequests>();
            services.AddScoped<Features.Core.Compensation.IResolveCompensationRequest, Features.Core.Compensation.ResolveCompensationRequest>();
            // §3.10.2 Medical Benefit — MB1 (providers, plans, contracts)
            services.AddScoped<Features.Core.Medical.ISaveMedicalProvider, Features.Core.Medical.SaveMedicalProvider>();
            services.AddScoped<Features.Core.Medical.IDeleteMedicalProvider, Features.Core.Medical.DeleteMedicalProvider>();
            services.AddScoped<Features.Core.Medical.IGetMedicalProviderById, Features.Core.Medical.GetMedicalProviderById>();
            services.AddScoped<Features.Core.Medical.IGetAllMedicalProviders, Features.Core.Medical.GetAllMedicalProviders>();
            services.AddScoped<Features.Core.Medical.ISaveMedicalPlan, Features.Core.Medical.SaveMedicalPlan>();
            services.AddScoped<Features.Core.Medical.IDeleteMedicalPlan, Features.Core.Medical.DeleteMedicalPlan>();
            services.AddScoped<Features.Core.Medical.IGetMedicalPlanById, Features.Core.Medical.GetMedicalPlanById>();
            services.AddScoped<Features.Core.Medical.IGetAllMedicalPlans, Features.Core.Medical.GetAllMedicalPlans>();
            services.AddScoped<Features.Core.Medical.ISaveMedicalContract, Features.Core.Medical.SaveMedicalContract>();
            services.AddScoped<Features.Core.Medical.IDeleteMedicalContract, Features.Core.Medical.DeleteMedicalContract>();
            services.AddScoped<Features.Core.Medical.IGetAllMedicalContracts, Features.Core.Medical.GetAllMedicalContracts>();
            services.AddScoped<Features.Core.Medical.IGetMedicalContractById, Features.Core.Medical.GetMedicalContractById>();
            // MB2 — enrollment + beneficiaries
            services.AddScoped<Features.Core.Medical.ISaveMedicalEnrollment, Features.Core.Medical.SaveMedicalEnrollment>();
            services.AddScoped<Features.Core.Medical.IGetEmployeeMedicalEnrollments, Features.Core.Medical.GetEmployeeMedicalEnrollments>();
            services.AddScoped<Features.Core.Medical.IGetMyMedicalEnrollments, Features.Core.Medical.GetMyMedicalEnrollments>();
            services.AddScoped<Features.Core.Medical.ISetMedicalEnrollmentStatus, Features.Core.Medical.SetMedicalEnrollmentStatus>();
            services.AddScoped<Features.Core.Medical.IAddMedicalBeneficiary, Features.Core.Medical.AddMedicalBeneficiary>();
            services.AddScoped<Features.Core.Medical.IRemoveMedicalBeneficiary, Features.Core.Medical.RemoveMedicalBeneficiary>();
            services.AddScoped<Features.Core.Medical.IDeleteMedicalEnrollment, Features.Core.Medical.DeleteMedicalEnrollment>();
            // MB3 — claims lifecycle + expense reports
            services.AddScoped<Features.Core.Medical.ISubmitMedicalClaim, Features.Core.Medical.SubmitMedicalClaim>();
            services.AddScoped<Features.Core.Medical.IGetMedicalClaims, Features.Core.Medical.GetMedicalClaims>();
            services.AddScoped<Features.Core.Medical.IGetMedicalClaimById, Features.Core.Medical.GetMedicalClaimById>();
            services.AddScoped<Features.Core.Medical.IDownloadMedicalClaimAttachment, Features.Core.Medical.DownloadMedicalClaimAttachment>();
            services.AddScoped<Features.Core.Medical.IApproveMedicalClaim, Features.Core.Medical.ApproveMedicalClaim>();
            services.AddScoped<Features.Core.Medical.IRejectMedicalClaim, Features.Core.Medical.RejectMedicalClaim>();
            services.AddScoped<Features.Core.Medical.IMarkMedicalClaimPaid, Features.Core.Medical.MarkMedicalClaimPaid>();
            services.AddScoped<Features.Core.Medical.IGetMedicalExpenseReport, Features.Core.Medical.GetMedicalExpenseReport>();

            // §3.10.3 Insurance Management — I1 (policies + premium schedule)
            services.AddScoped<Features.Core.Insurance.ISaveInsurancePolicy, Features.Core.Insurance.SaveInsurancePolicy>();
            services.AddScoped<Features.Core.Insurance.IDeleteInsurancePolicy, Features.Core.Insurance.DeleteInsurancePolicy>();
            services.AddScoped<Features.Core.Insurance.IGetInsurancePolicyById, Features.Core.Insurance.GetInsurancePolicyById>();
            services.AddScoped<Features.Core.Insurance.IGetAllInsurancePolicies, Features.Core.Insurance.GetAllInsurancePolicies>();
            services.AddScoped<Features.Core.Insurance.IGeneratePremiumSchedule, Features.Core.Insurance.GeneratePremiumSchedule>();
            services.AddScoped<Features.Core.Insurance.IAddPremiumSchedule, Features.Core.Insurance.AddPremiumSchedule>();
            services.AddScoped<Features.Core.Insurance.IRemovePremiumSchedule, Features.Core.Insurance.RemovePremiumSchedule>();
            services.AddScoped<Features.Core.Insurance.IMarkInsurancePremiumPaid, Features.Core.Insurance.MarkInsurancePremiumPaid>();
            services.AddScoped<Features.Core.Insurance.ISubmitInsuranceClaim, Features.Core.Insurance.SubmitInsuranceClaim>();
            services.AddScoped<Features.Core.Insurance.IGetInsuranceClaims, Features.Core.Insurance.GetInsuranceClaims>();
            services.AddScoped<Features.Core.Insurance.IGetInsuranceClaimById, Features.Core.Insurance.GetInsuranceClaimById>();
            services.AddScoped<Features.Core.Insurance.IDownloadInsuranceClaimAttachment, Features.Core.Insurance.DownloadInsuranceClaimAttachment>();
            services.AddScoped<Features.Core.Insurance.IApproveInsuranceClaim, Features.Core.Insurance.ApproveInsuranceClaim>();
            services.AddScoped<Features.Core.Insurance.IRejectInsuranceClaim, Features.Core.Insurance.RejectInsuranceClaim>();
            services.AddScoped<Features.Core.Insurance.IMarkInsuranceClaimPaid, Features.Core.Insurance.MarkInsuranceClaimPaid>();

            // §3.10.4 Employee Loan — L1 (loan types + requests + guarantors + schedule + workflow)
            services.AddScoped<Features.Core.Loans.ISaveLoanType, Features.Core.Loans.SaveLoanType>();
            services.AddScoped<Features.Core.Loans.IDeleteLoanType, Features.Core.Loans.DeleteLoanType>();
            services.AddScoped<Features.Core.Loans.IGetLoanTypeById, Features.Core.Loans.GetLoanTypeById>();
            services.AddScoped<Features.Core.Loans.IGetAllLoanTypes, Features.Core.Loans.GetAllLoanTypes>();
            services.AddScoped<Features.Core.Loans.IRequestLoan, Features.Core.Loans.RequestLoan>();
            services.AddScoped<Features.Core.Loans.IGetLoans, Features.Core.Loans.GetLoans>();
            services.AddScoped<Features.Core.Loans.IGetLoanById, Features.Core.Loans.GetLoanById>();
            services.AddScoped<Features.Core.Loans.IApproveLoan, Features.Core.Loans.ApproveLoan>();
            services.AddScoped<Features.Core.Loans.IRejectLoan, Features.Core.Loans.RejectLoan>();
            services.AddScoped<Features.Core.Loans.ICancelLoan, Features.Core.Loans.CancelLoan>();
            services.AddScoped<Features.Core.Loans.IDisburseLoan, Features.Core.Loans.DisburseLoan>();
            services.AddScoped<Features.Core.Loans.IRecordLoanRepayment, Features.Core.Loans.RecordLoanRepayment>();
            services.AddScoped<Features.Core.Loans.IIncrementLoanInstallment, Features.Core.Loans.IncrementLoanInstallment>();
            services.AddScoped<Features.Core.Loans.IGiveLoanConsent, Features.Core.Loans.GiveLoanConsent>();

            // §3.10.5 Trip Management — T1 (per-diem rates + travel budgets)
            services.AddScoped<Features.Core.Trips.ISavePerDiemRate, Features.Core.Trips.SavePerDiemRate>();
            services.AddScoped<Features.Core.Trips.IDeletePerDiemRate, Features.Core.Trips.DeletePerDiemRate>();
            services.AddScoped<Features.Core.Trips.IGetAllPerDiemRates, Features.Core.Trips.GetAllPerDiemRates>();
            services.AddScoped<Features.Core.Trips.IGetPerDiemRateById, Features.Core.Trips.GetPerDiemRateById>();
            services.AddScoped<Features.Core.Trips.ISaveTripBudget, Features.Core.Trips.SaveTripBudget>();
            services.AddScoped<Features.Core.Trips.IDeleteTripBudget, Features.Core.Trips.DeleteTripBudget>();
            services.AddScoped<Features.Core.Trips.IGetAllTripBudgets, Features.Core.Trips.GetAllTripBudgets>();
            services.AddScoped<Features.Core.Trips.IGetTripBudgetById, Features.Core.Trips.GetTripBudgetById>();
            services.AddScoped<Features.Core.Trips.IRequestTrip, Features.Core.Trips.RequestTrip>();
            services.AddScoped<Features.Core.Trips.IGetTrips, Features.Core.Trips.GetTrips>();
            services.AddScoped<Features.Core.Trips.IGetTripById, Features.Core.Trips.GetTripById>();
            services.AddScoped<Features.Core.Trips.IApproveTrip, Features.Core.Trips.ApproveTrip>();
            services.AddScoped<Features.Core.Trips.IRejectTrip, Features.Core.Trips.RejectTrip>();
            services.AddScoped<Features.Core.Trips.ICancelTrip, Features.Core.Trips.CancelTrip>();
            services.AddScoped<Features.Core.Trips.ITransitionTrip, Features.Core.Trips.TransitionTrip>();
            services.AddScoped<Features.Core.Trips.IAddTripExpense, Features.Core.Trips.AddTripExpense>();
            services.AddScoped<Features.Core.Trips.IRemoveTripExpense, Features.Core.Trips.RemoveTripExpense>();
            services.AddScoped<Features.Core.Trips.IGetTripBudgetUtilization, Features.Core.Trips.GetTripBudgetUtilization>();
            services.AddScoped<Features.Core.Trips.IDisburseTripAdvance, Features.Core.Trips.DisburseTripAdvance>();
            services.AddScoped<Features.Core.Trips.ISettleTrip, Features.Core.Trips.SettleTrip>();
            services.AddScoped<Features.Core.Trips.IGetTripAgingReport, Features.Core.Trips.GetTripAgingReport>();
            services.AddScoped<Features.Core.Trips.ITripSettlementReminder, Features.Core.Trips.TripSettlementReminder>();

            services.AddScoped<Features.Core.Rewards.ISaveAwardCategory, Features.Core.Rewards.SaveAwardCategory>();
            services.AddScoped<Features.Core.Rewards.IDeleteAwardCategory, Features.Core.Rewards.DeleteAwardCategory>();
            services.AddScoped<Features.Core.Rewards.IGetAwardCategoryById, Features.Core.Rewards.GetAwardCategoryById>();
            services.AddScoped<Features.Core.Rewards.IGetAllAwardCategories, Features.Core.Rewards.GetAllAwardCategories>();
            services.AddScoped<Features.Core.Rewards.ISaveRecognitionProgram, Features.Core.Rewards.SaveRecognitionProgram>();
            services.AddScoped<Features.Core.Rewards.IDeleteRecognitionProgram, Features.Core.Rewards.DeleteRecognitionProgram>();
            services.AddScoped<Features.Core.Rewards.IGetRecognitionProgramById, Features.Core.Rewards.GetRecognitionProgramById>();
            services.AddScoped<Features.Core.Rewards.IGetAllRecognitionPrograms, Features.Core.Rewards.GetAllRecognitionPrograms>();
            services.AddScoped<Features.Core.Rewards.ISaveRewardNomination, Features.Core.Rewards.SaveRewardNomination>();
            services.AddScoped<Features.Core.Rewards.IDeleteRewardNomination, Features.Core.Rewards.DeleteRewardNomination>();
            services.AddScoped<Features.Core.Rewards.IGetRewardNominationById, Features.Core.Rewards.GetRewardNominationById>();
            services.AddScoped<Features.Core.Rewards.IGetAllRewardNominations, Features.Core.Rewards.GetAllRewardNominations>();
            services.AddScoped<Features.Core.Rewards.IApproveRewardNomination, Features.Core.Rewards.ApproveRewardNomination>();
            services.AddScoped<IWorkflowEntityHandler, Features.Core.Workflows.RewardNominationWorkflowHandler>();
            services.AddScoped<Features.Core.Rewards.IGetRewardPoints, Features.Core.Rewards.GetRewardPoints>();
            services.AddScoped<Features.Core.Rewards.IRedeemRewardPoints, Features.Core.Rewards.RedeemRewardPoints>();
            services.AddScoped<Features.Core.Rewards.IGetAllRewardDisbursements, Features.Core.Rewards.GetAllRewardDisbursements>();
            services.AddScoped<Features.Core.Rewards.IMarkRewardDisbursementPaid, Features.Core.Rewards.MarkRewardDisbursementPaid>();
            services.AddScoped<Features.Core.Rewards.IExportRewardDisbursements, Features.Core.Rewards.ExportRewardDisbursements>();
            services.AddScoped<Features.Core.Rewards.IGetRecognitionWall, Features.Core.Rewards.GetRecognitionWall>();
            // HC181 — auto-grant badges from completed appraisals.
            services.AddScoped<Features.Core.Performance.IAppraisalCompletedHandler, Features.Core.Rewards.AppraisalAutoGrantHandler>();

            // Training & Development (§3.8, HC187–HC202) — Phase TD1: catalog + needs.
            services.AddScoped<Features.Core.Training.ISaveTrainingCategory, Features.Core.Training.SaveTrainingCategory>();
            services.AddScoped<Features.Core.Training.IDeleteTrainingCategory, Features.Core.Training.DeleteTrainingCategory>();
            services.AddScoped<Features.Core.Training.IGetTrainingCategoryById, Features.Core.Training.GetTrainingCategoryById>();
            services.AddScoped<Features.Core.Training.IGetAllTrainingCategories, Features.Core.Training.GetAllTrainingCategories>();
            services.AddScoped<Features.Core.Training.ISaveTrainingCourse, Features.Core.Training.SaveTrainingCourse>();
            services.AddScoped<Features.Core.Training.IDeleteTrainingCourse, Features.Core.Training.DeleteTrainingCourse>();
            services.AddScoped<Features.Core.Training.IGetTrainingCourseById, Features.Core.Training.GetTrainingCourseById>();
            services.AddScoped<Features.Core.Training.IGetAllTrainingCourses, Features.Core.Training.GetAllTrainingCourses>();
            services.AddScoped<Features.Core.Training.ISaveTrainingNeed, Features.Core.Training.SaveTrainingNeed>();
            services.AddScoped<Features.Core.Training.IDeleteTrainingNeed, Features.Core.Training.DeleteTrainingNeed>();
            services.AddScoped<Features.Core.Training.ICancelTrainingNeed, Features.Core.Training.CancelTrainingNeed>();
            services.AddScoped<Features.Core.Training.IGetTrainingNeedById, Features.Core.Training.GetTrainingNeedById>();
            services.AddScoped<Features.Core.Training.IGetAllTrainingNeeds, Features.Core.Training.GetAllTrainingNeeds>();
            services.AddScoped<Features.Core.Training.ITrainingNeedDecision, Features.Core.Training.TrainingNeedDecision>();
            services.AddScoped<Features.Core.Training.IGetTrainingNeedSuggestions, Features.Core.Training.GetTrainingNeedSuggestions>();
            services.AddScoped<IWorkflowEntityHandler, Features.Core.Workflows.TrainingNeedWorkflowHandler>();

            // Phase TD2 — delivery: sessions, enrollments, budgets (HC190/HC197/HC198/HC199).
            services.AddScoped<Features.Core.Training.ISaveTrainingSession, Features.Core.Training.SaveTrainingSession>();
            services.AddScoped<Features.Core.Training.ICreateTrainingSessionSeries, Features.Core.Training.CreateTrainingSessionSeries>();
            services.AddScoped<Features.Core.Training.IRescheduleTrainingSession, Features.Core.Training.RescheduleTrainingSession>();
            services.AddScoped<Features.Core.Training.ICompleteTrainingSession, Features.Core.Training.CompleteTrainingSession>();
            services.AddScoped<Features.Core.Training.ICancelTrainingSession, Features.Core.Training.CancelTrainingSession>();
            services.AddScoped<Features.Core.Training.IDeleteTrainingSession, Features.Core.Training.DeleteTrainingSession>();
            services.AddScoped<Features.Core.Training.IGetTrainingSessionById, Features.Core.Training.GetTrainingSessionById>();
            services.AddScoped<Features.Core.Training.IGetAllTrainingSessions, Features.Core.Training.GetAllTrainingSessions>();
            services.AddScoped<Features.Core.Training.IEnrollTraining, Features.Core.Training.EnrollTraining>();
            services.AddScoped<Features.Core.Training.IRecordTrainingParticipation, Features.Core.Training.RecordTrainingParticipation>();
            services.AddScoped<Features.Core.Training.ISubmitTrainingFeedback, Features.Core.Training.SubmitTrainingFeedback>();
            services.AddScoped<Features.Core.Training.IWithdrawTrainingEnrollment, Features.Core.Training.WithdrawTrainingEnrollment>();
            services.AddScoped<Features.Core.Training.IDeleteTrainingEnrollment, Features.Core.Training.DeleteTrainingEnrollment>();
            services.AddScoped<Features.Core.Training.IGetAllTrainingEnrollments, Features.Core.Training.GetAllTrainingEnrollments>();
            services.AddScoped<Features.Core.Training.ISaveTrainingBudget, Features.Core.Training.SaveTrainingBudget>();
            services.AddScoped<Features.Core.Training.IDeleteTrainingBudget, Features.Core.Training.DeleteTrainingBudget>();
            services.AddScoped<Features.Core.Training.IGetAllTrainingBudgets, Features.Core.Training.GetAllTrainingBudgets>();
            services.AddScoped<Features.Core.Training.IGetTrainingBudgetUtilization, Features.Core.Training.GetTrainingBudgetUtilization>();

            // Phase TD3 — learning paths, certification + CPD, provider payments (HC193/HC200/HC202).
            services.AddScoped<Features.Core.Training.ISaveLearningPath, Features.Core.Training.SaveLearningPath>();
            services.AddScoped<Features.Core.Training.IDeleteLearningPath, Features.Core.Training.DeleteLearningPath>();
            services.AddScoped<Features.Core.Training.IGetLearningPathById, Features.Core.Training.GetLearningPathById>();
            services.AddScoped<Features.Core.Training.IGetAllLearningPaths, Features.Core.Training.GetAllLearningPaths>();
            services.AddScoped<Features.Core.Training.IGetLearningPathProgress, Features.Core.Training.GetLearningPathProgress>();
            services.AddScoped<Features.Core.Training.IIssueTrainingCertificate, Features.Core.Training.IssueTrainingCertificate>();
            services.AddScoped<Features.Core.Training.ISaveTrainingCertificate, Features.Core.Training.SaveTrainingCertificate>();
            services.AddScoped<Features.Core.Training.IRenewTrainingCertificate, Features.Core.Training.RenewTrainingCertificate>();
            services.AddScoped<Features.Core.Training.IDeleteTrainingCertificate, Features.Core.Training.DeleteTrainingCertificate>();
            services.AddScoped<Features.Core.Training.IGetAllTrainingCertificates, Features.Core.Training.GetAllTrainingCertificates>();
            services.AddScoped<Features.Core.Training.IGetExpiringTrainingCertificates, Features.Core.Training.GetExpiringTrainingCertificates>();
            services.AddScoped<Features.Core.Training.IGetCpdSummary, Features.Core.Training.GetCpdSummary>();
            services.AddScoped<Features.Core.Training.IGetAllProviderPayments, Features.Core.Training.GetAllProviderPayments>();
            services.AddScoped<Features.Core.Training.IMarkProviderPaymentPaid, Features.Core.Training.MarkProviderPaymentPaid>();
            services.AddScoped<Features.Core.Training.IExportProviderPayments, Features.Core.Training.ExportProviderPayments>();
            services.AddScoped<Features.Core.DocumentTemplates.IGenerateTrainingCertificate, Features.Core.DocumentTemplates.GenerateTrainingCertificate>();

            // Phase TD6 — social learning: communities + discussion threads (HC198/HC199).
            services.AddScoped<Features.Core.Training.ISaveLearningCommunity, Features.Core.Training.SaveLearningCommunity>();
            services.AddScoped<Features.Core.Training.IDeleteLearningCommunity, Features.Core.Training.DeleteLearningCommunity>();
            services.AddScoped<Features.Core.Training.IJoinLearningCommunity, Features.Core.Training.JoinLearningCommunity>();
            services.AddScoped<Features.Core.Training.ILeaveLearningCommunity, Features.Core.Training.LeaveLearningCommunity>();
            services.AddScoped<Features.Core.Training.IGetAllLearningCommunities, Features.Core.Training.GetAllLearningCommunities>();
            services.AddScoped<Features.Core.Training.IGetCommunityPosts, Features.Core.Training.GetCommunityPosts>();
            services.AddScoped<Features.Core.Training.ICreateCommunityPost, Features.Core.Training.CreateCommunityPost>();
            services.AddScoped<Features.Core.Training.IDeleteCommunityPost, Features.Core.Training.DeleteCommunityPost>();
            services.AddScoped<Features.Core.Training.IReactToCommunityPost, Features.Core.Training.ReactToCommunityPost>();
            services.AddScoped<Features.Core.Training.IGetCommunityAnalytics, Features.Core.Training.GetCommunityAnalytics>();
            services.AddScoped<Features.Core.Rewards.IGetPointsLeaderboard, Features.Core.Rewards.GetPointsLeaderboard>();

            // Employee Engagement (§3.9.1) — Phase E1: suggestions, grievances, announcements.
            services.AddScoped<Features.Core.Engagement.ISubmitSuggestion, Features.Core.Engagement.SubmitSuggestion>();
            services.AddScoped<Features.Core.Engagement.IRespondSuggestion, Features.Core.Engagement.RespondSuggestion>();
            services.AddScoped<Features.Core.Engagement.IDeleteSuggestion, Features.Core.Engagement.DeleteSuggestion>();
            services.AddScoped<Features.Core.Engagement.IGetAllSuggestions, Features.Core.Engagement.GetAllSuggestions>();
            services.AddScoped<Features.Core.Engagement.ISubmitGrievance, Features.Core.Engagement.SubmitGrievance>();
            services.AddScoped<Features.Core.Engagement.IAssignGrievance, Features.Core.Engagement.AssignGrievance>();
            services.AddScoped<Features.Core.Engagement.IResolveGrievance, Features.Core.Engagement.ResolveGrievance>();
            services.AddScoped<Features.Core.Engagement.ICloseGrievance, Features.Core.Engagement.CloseGrievance>();
            services.AddScoped<Features.Core.Engagement.IAddGrievanceNote, Features.Core.Engagement.AddGrievanceNote>();
            services.AddScoped<Features.Core.Engagement.IGetGrievanceById, Features.Core.Engagement.GetGrievanceById>();
            services.AddScoped<Features.Core.Engagement.IGetAllGrievances, Features.Core.Engagement.GetAllGrievances>();
            services.AddScoped<Features.Core.Engagement.ISaveAnnouncement, Features.Core.Engagement.SaveAnnouncement>();
            services.AddScoped<Features.Core.Engagement.IDeleteAnnouncement, Features.Core.Engagement.DeleteAnnouncement>();
            services.AddScoped<Features.Core.Engagement.IGetAllAnnouncements, Features.Core.Engagement.GetAllAnnouncements>();
            services.AddScoped<Features.Core.Engagement.IGetAnnouncementFeed, Features.Core.Engagement.GetAnnouncementFeed>();

            // Exit / Separation (§3.9.2) — Phase X1: initiation gates, notifications, exit letters.
            services.AddScoped<Features.Core.Employees.ITerminationNotifier, Features.Core.Employees.TerminationNotifier>();
            services.AddScoped<Features.Core.DocumentTemplates.IGenerateTerminationDocument, Features.Core.DocumentTemplates.GenerateTerminationDocument>();

            // Phase X2 — asset recovery (HC214/HC215).
            services.AddScoped<Features.Core.Employees.ISaveCompanyAsset, Features.Core.Employees.SaveCompanyAsset>();
            services.AddScoped<Features.Core.Employees.IDeleteCompanyAsset, Features.Core.Employees.DeleteCompanyAsset>();
            services.AddScoped<Features.Core.Employees.IAssignCompanyAsset, Features.Core.Employees.AssignCompanyAsset>();
            services.AddScoped<Features.Core.Employees.IReturnCompanyAsset, Features.Core.Employees.ReturnCompanyAsset>();
            services.AddScoped<Features.Core.Employees.IGetAllCompanyAssets, Features.Core.Employees.GetAllCompanyAssets>();
            services.AddScoped<Features.Core.Employees.IGetAssetRecoveries, Features.Core.Employees.GetAssetRecoveries>();
            services.AddScoped<Features.Core.Employees.IResolveAssetRecovery, Features.Core.Employees.ResolveAssetRecovery>();

            // Phase X3 — exit interviews + final settlements (HC216–HC219).
            services.AddScoped<Features.Core.Employees.IGetExitQuestionnaire, Features.Core.Employees.GetExitQuestionnaire>();
            services.AddScoped<Features.Core.Employees.ISaveExitQuestionnaire, Features.Core.Employees.SaveExitQuestionnaire>();
            services.AddScoped<Features.Core.Employees.ILaunchExitInterview, Features.Core.Employees.LaunchExitInterview>();
            services.AddScoped<Features.Core.Employees.ISubmitExitInterview, Features.Core.Employees.SubmitExitInterview>();
            services.AddScoped<Features.Core.Employees.IGetExitInterview, Features.Core.Employees.GetExitInterview>();
            services.AddScoped<Features.Core.Employees.IBuildTerminationSettlement, Features.Core.Employees.BuildTerminationSettlement>();
            services.AddScoped<Features.Core.Employees.IUpdateSettlementLines, Features.Core.Employees.UpdateSettlementLines>();
            services.AddScoped<Features.Core.Employees.IApproveTerminationSettlement, Features.Core.Employees.ApproveTerminationSettlement>();
            services.AddScoped<Features.Core.Employees.IMarkTerminationSettlementPaid, Features.Core.Employees.MarkTerminationSettlementPaid>();
            services.AddScoped<Features.Core.Employees.IGetTerminationSettlement, Features.Core.Employees.GetTerminationSettlement>();
            services.AddScoped<Features.Core.DocumentTemplates.IGenerateSettlementDocument, Features.Core.DocumentTemplates.GenerateSettlementDocument>();

            // Phase E2 — surveys & polls (HC204).
            services.AddScoped<Features.Core.Engagement.ISaveSurvey, Features.Core.Engagement.SaveSurvey>();
            services.AddScoped<Features.Core.Engagement.IOpenSurvey, Features.Core.Engagement.OpenSurvey>();
            services.AddScoped<Features.Core.Engagement.ICloseSurvey, Features.Core.Engagement.CloseSurvey>();
            services.AddScoped<Features.Core.Engagement.IDeleteSurvey, Features.Core.Engagement.DeleteSurvey>();
            services.AddScoped<Features.Core.Engagement.IGetAllSurveys, Features.Core.Engagement.GetAllSurveys>();
            services.AddScoped<Features.Core.Engagement.IGetSurveyFeed, Features.Core.Engagement.GetSurveyFeed>();
            services.AddScoped<Features.Core.Engagement.IGetSurveyById, Features.Core.Engagement.GetSurveyById>();
            services.AddScoped<Features.Core.Engagement.ISubmitSurveyResponse, Features.Core.Engagement.SubmitSurveyResponse>();
            services.AddScoped<Features.Core.Engagement.IGetSurveyResults, Features.Core.Engagement.GetSurveyResults>();

            // Dynamic navigation — subsystem master list + menu seeding, plus the template-era
            // Module/Operation feature handlers (IFeatureHandler style) now exposed via controllers.
            services.AddScoped<Features.Core.Roles.ISaveRolePermissions, Features.Core.Roles.SaveRolePermissions>();
            services.AddScoped<Features.Core.Roles.IGetAllRolePermissions, Features.Core.Roles.GetAllRolePermissions>();
            services.AddScoped<Features.Core.Roles.IDeleteRolePermission, Features.Core.Roles.DeleteRolePermission>();
            services.AddScoped<Features.Core.Subsystems.ISaveSubsystem, Features.Core.Subsystems.SaveSubsystem>();
            services.AddScoped<Features.Core.Subsystems.IGetAllSubsystems, Features.Core.Subsystems.GetAllSubsystems>();
            services.AddScoped<Features.Core.Subsystems.IDeleteSubsystem, Features.Core.Subsystems.DeleteSubsystem>();
            services.AddScoped<Features.Core.Modules.ISeedDefaultMenu, Features.Core.Modules.SeedDefaultMenu>();
            services.AddScoped<
                Common.Handlers.IFeatureHandler<Features.Core.Modules.Create.CreateModuleRequest, Features.Core.Modules.DTOs.ModuleResult>,
                Features.Core.Modules.Create.CreateModuleHandler>();
            services.AddScoped<
                Common.Handlers.IFeatureHandler<Features.Core.Modules.Update.UpdateModuleRequest, Features.Core.Modules.DTOs.ModuleResult>,
                Features.Core.Modules.Update.UpdateModuleHandler>();
            services.AddScoped<
                Common.Handlers.IFeatureHandler<Features.Core.Modules.Delete.DeleteModuleRequest, Features.Core.Modules.DTOs.ModuleResult?>,
                Features.Core.Modules.Delete.DeleteModuleHandler>();
            services.AddScoped<
                Common.Handlers.IFeatureHandler<Features.Core.Modules.GetAll.GetAllModulesRequest, Common.DTOs.PaginatedResponse<Features.Core.Modules.DTOs.GetModuleDto>>,
                Features.Core.Modules.GetAll.GetAllModulesHandler>();
            services.AddScoped<
                Common.Handlers.IFeatureHandler<Features.Core.Modules.GetById.GetModuleByIdRequest, Features.Core.Modules.DTOs.GetModuleDto?>,
                Features.Core.Modules.GetById.GetModuleByIdHandler>();
            services.AddScoped<
                Common.Handlers.IFeatureHandler<Features.Core.Modules.GetOperations.GetModuleWithOperationsRequest, IEnumerable<Features.Core.Modules.DTOs.GetModuleWithOperationResult>>,
                Features.Core.Modules.GetOperations.GetModuleWithOperationsHandler>();
            services.AddScoped<
                Common.Handlers.IFeatureHandler<Features.Core.Operations.Create.CreateOperationRequest, Features.Core.Operations.DTOs.OperationResult>,
                Features.Core.Operations.Create.CreateOperationHandler>();
            services.AddScoped<
                Common.Handlers.IFeatureHandler<Features.Core.Operations.Update.UpdateOperationRequest, Features.Core.Operations.DTOs.OperationResult>,
                Features.Core.Operations.Update.UpdateOperationHandler>();
            services.AddScoped<
                Common.Handlers.IFeatureHandler<Features.Core.Operations.Delete.DeleteOperationRequest, Features.Core.Operations.DTOs.OperationResult?>,
                Features.Core.Operations.Delete.DeleteOperationHandler>();
            services.AddScoped<
                Common.Handlers.IFeatureHandler<Features.Core.Operations.GetAll.GetAllOperationsRequest, Common.DTOs.PaginatedResponse<Features.Core.Operations.DTOs.OperationDto>>,
                Features.Core.Operations.GetAll.GetAllOperationsHandler>();
            services.AddScoped<
                Common.Handlers.IFeatureHandler<Features.Core.Operations.GetById.GetOperationByIdRequest, Features.Core.Operations.DTOs.OperationDto?>,
                Features.Core.Operations.GetById.GetOperationByIdHandler>();
        }
    }
}
