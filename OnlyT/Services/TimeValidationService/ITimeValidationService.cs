using System;

namespace OnlyT.Services.TimeValidationService;

public interface ITimeValidationService
{
    DateTime? GetValidatedTime();
}