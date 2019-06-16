using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;

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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="description"></param>
        /// <returns></returns>
        public static string GetFriendlyId(this ApiDescription description)
        {
            string path = description.RelativePath;
            string[] urlParts = path.Split('?');
            string localPath = urlParts[0];
            string queryKeyString = null;
            if (urlParts.Length > 1)
            {
                string query = urlParts[1];
                string[] queryKeys = HttpUtility.ParseQueryString(query).AllKeys;
                queryKeyString = String.Join("_", queryKeys);
            }

            StringBuilder friendlyPath = new StringBuilder();
            friendlyPath.AppendFormat("{0}-{1}",
                description.HttpMethod,
                localPath.Replace("/", "-").Replace("{", String.Empty).Replace("}", String.Empty));
            if (queryKeyString != null)
            {
                friendlyPath.AppendFormat("_{0}", queryKeyString.Replace('.', '-'));
            }
            return friendlyPath.ToString();
        }
    }
}
