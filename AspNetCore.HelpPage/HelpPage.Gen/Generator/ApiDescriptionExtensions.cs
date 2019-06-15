using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace HelpPage.Gen
{
    public static class ApiDescriptionExtensions
    {
        /// <summary>
        /// 获取方法描述
        /// </summary>
        /// <param name="apiDescription"></param>
        /// <returns></returns>
        public static MethodInfo MethodInfo(this ApiDescription apiDescription)
        {
            var controllerActionDescriptor = apiDescription.ActionDescriptor as ControllerActionDescriptor;
            return controllerActionDescriptor?.MethodInfo;
        }
        /// <summary>
        /// 获取方法自定义特性
        /// </summary>
        /// <param name="apiDescription"></param>
        /// <returns></returns>
        public static IEnumerable<object> CustomAttributes(this ApiDescription apiDescription)
        {
            var methodInfo = apiDescription.MethodInfo();

            if (methodInfo == null) return Enumerable.Empty<object>();

            return methodInfo.GetCustomAttributes(true)
                .Union(methodInfo.DeclaringType.GetCustomAttributes(true));
        }
    }
}
