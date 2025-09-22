using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace CarDealership.Extensions;

public static class RoutePrefixExtensions
{
    public static void UseGeneralRoutePrefix(this MvcOptions opts, string prefix)
    {
        opts.Conventions.Add(new RoutePrefixConvention(prefix));
    }
}

public class RoutePrefixConvention : IControllerModelConvention
{
    private readonly string _prefix;

    public RoutePrefixConvention(string prefix)
    {
        _prefix = prefix;
    }

    public void Apply(ControllerModel controller)
    {
        foreach (var selector in controller.Selectors)
        {
            if (selector.AttributeRouteModel != null)
            {
                selector.AttributeRouteModel.Template = _prefix + "/" + selector.AttributeRouteModel.Template?.TrimStart('/');
            }
            else
            {
                selector.AttributeRouteModel = new AttributeRouteModel
                {
                    Template = _prefix + "/" + controller.ControllerName.ToLower()
                };
            }
        }
    }
}
