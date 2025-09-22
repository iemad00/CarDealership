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
        // Determine the base prefix based on controller namespace
        string basePrefix = _prefix; // Default: "api/v{version:apiVersion}"
        
        if (controller.ControllerType.Namespace?.Contains("Controllers.Admin") == true)
        {
            basePrefix = _prefix + "/dashboard"; // "api/v{version:apiVersion}/dashboard"
        }
        // User controllers just get the default prefix: "api/v{version:apiVersion}"

        foreach (var selector in controller.Selectors)
        {
            if (selector.AttributeRouteModel != null)
            {
                selector.AttributeRouteModel.Template = basePrefix + "/" + selector.AttributeRouteModel.Template?.TrimStart('/');
            }
            else
            {
                selector.AttributeRouteModel = new AttributeRouteModel
                {
                    Template = basePrefix + "/" + controller.ControllerName.ToLower()
                };
            }
        }
    }
}
