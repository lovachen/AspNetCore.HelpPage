using Microsoft.AspNetCore.Mvc.ApplicationModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace HelpPage.Gen
{
    /// <summary>
    /// 
    /// </summary>
    public class HelpPageApplicationConvention : IApplicationModelConvention
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="application"></param>
        public void Apply(ApplicationModel application)
        {
            application.ApiExplorer.IsVisible = true;
        }
    }
}
