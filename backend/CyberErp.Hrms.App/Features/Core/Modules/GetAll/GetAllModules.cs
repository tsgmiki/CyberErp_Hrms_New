using FluentValidation;
using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Features.Core.Modules.DTOs;
using Microsoft.Extensions.Logging;

namespace CyberErp.Hrms.App.Features.Core.Modules.GetAll
{
    public class GetAllModules(
        IGetAllModuleRepository repository,
        IValidator<GetAllRequest> validator,
        ILogger<GetAllModules> logger
        ) : IGetAllModules
    {
        private readonly IGetAllModuleRepository _repository = repository;
        private readonly IValidator<GetAllRequest> _validator = validator;
        private readonly ILogger<GetAllModules> _logger = logger;

        public async Task<PaginatedResponse<GetModuleDto>> GetAllAsync(GetAllRequest request)
        {
            var validationResult = await _validator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Get all Modules failed validation for request parameters");
                throw new ValidationException(validationResult.Errors);
            }
            var result = await _repository.GetAllAsync(request);
            return result;
        }
    }
}