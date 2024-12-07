using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace Quizanchos.WebApi.Util;

public class SkipControllerConvention : IApplicationModelConvention
{
    private readonly Type _controllerTypeToSkip;

    public SkipControllerConvention(Type controllerTypeToSkip)
    {
        _controllerTypeToSkip = controllerTypeToSkip;
    }

    public void Apply(ApplicationModel application)
    {
        ControllerModel? controller = application.Controllers
            .FirstOrDefault(c => c.ControllerType == _controllerTypeToSkip);

        if (controller is not null)
        {
            application.Controllers.Remove(controller);
        }
    }
}
