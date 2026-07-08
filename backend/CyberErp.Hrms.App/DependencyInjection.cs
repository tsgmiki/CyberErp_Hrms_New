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
            services.AddScoped<IWorkflowEntityHandler, EmployeeMovementWorkflowHandler>();
            services.AddScoped<IWorkflowEntityHandler, DisciplinaryMeasureWorkflowHandler>();
            services.AddScoped<IWorkflowEntityHandler, EmployeeTerminationWorkflowHandler>();
            services.AddScoped<IGetAllWorkflowInstances, GetAllWorkflowInstances>();
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

            // Document templates & correspondence (HC022)
            services.AddScoped<ICreateDocumentTemplate, CreateDocumentTemplate>();
            services.AddScoped<IUpdateDocumentTemplate, UpdateDocumentTemplate>();
            services.AddScoped<IDeleteDocumentTemplate, DeleteDocumentTemplate>();
            services.AddScoped<IGetDocumentTemplateById, GetDocumentTemplateById>();
            services.AddScoped<IGetAllDocumentTemplates, GetAllDocumentTemplates>();
            services.AddScoped<ISeedDefaultDocumentTemplates, SeedDefaultDocumentTemplates>();
            services.AddScoped<IGetDocumentMergeFields, GetDocumentMergeFields>();
            services.AddScoped<IGenerateEmployeeDocument, GenerateEmployeeDocument>();
            services.AddScoped<IUploadCompanyLogo, UploadCompanyLogo>();
            services.AddScoped<IGetCompanyLogo, GetCompanyLogo>();
            services.AddScoped<IGetCompanyLogoInfo, GetCompanyLogoInfo>();
            services.AddScoped<IDeleteCompanyLogo, DeleteCompanyLogo>();
        }
    }
}
