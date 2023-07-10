using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace RazorHtmlEmails.GComFuelManagerMigracion.Services
{
    public class RazorViewToStringRender : IRazorViewToStringRenderer
    {
        private readonly IRazorViewEngine razorEngine;
        private readonly ITempDataProvider dataProvider;
        private readonly IServiceProvider serviceProvider;

        public RazorViewToStringRender(IRazorViewEngine razorEngine, ITempDataProvider dataProvider, IServiceProvider serviceProvider)
        {
            this.razorEngine = razorEngine;
            this.dataProvider = dataProvider;
            this.serviceProvider = serviceProvider;
        }

        public async Task<string> RenderViewToStringAsync<TModel>(string viewModel, TModel model)
        {
            var actionContext = GetActionContext();
            var view = FindView(actionContext, viewModel);

            await using var output = new StringWriter();
            var viewContext = new ViewContext(actionContext, view, new ViewDataDictionary<TModel>(
                metadataProvider: new EmptyModelMetadataProvider(),
                modelState: new ModelStateDictionary())
            {
                Model = model
            },
            new TempDataDictionary(actionContext.HttpContext, dataProvider),
            output,
            new HtmlHelperOptions());

            await view.RenderAsync(viewContext);

            return output.ToString();
        }

        private IView FindView(ActionContext actionContext, string viewModel)
        {
            var getViewResult = razorEngine.GetView(executingFilePath: null, viewPath: viewModel, isMainPage: true);

            if (getViewResult.Success)
            {
                return getViewResult.View;
            }

            var findViewResult = razorEngine.FindView(actionContext, viewModel, isMainPage: true);

            if (findViewResult.Success)
            {
                return findViewResult.View;
            }

            var searchedLocations = getViewResult.SearchedLocations.Concat(findViewResult.SearchedLocations);
            var errorMessage = string.Join(Environment.NewLine, new[] { $"Unable to find view '{viewModel}'. The following locations were searched" }.Concat(searchedLocations));

            throw new InvalidOperationException(errorMessage);
        }

        private ActionContext GetActionContext()
        {
            var httpContext = new DefaultHttpContext
            {
                RequestServices = serviceProvider
            };
            return new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        }
    }

    public interface IRazorViewToStringRenderer
    {
        Task<string> RenderViewToStringAsync<TModel>(string viewModel, TModel model);
    }
}
