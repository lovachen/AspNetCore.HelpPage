using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.XPath;

namespace HelpPage.Gen
{
    public class XmlDocumentationProvider
    {
        private XPathNavigator _documentNavigator;
        private const string TypeExpression = "/doc/members/member[@name='T:{0}']";
        private const string MethodExpression = "/doc/members/member[@name='M:{0}']";
        private const string PropertyExpression = "/doc/members/member[@name='P:{0}']";
        private const string FieldExpression = "/doc/members/member[@name='F:{0}']";
        private const string ParameterExpression = "param[@name='{0}']";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="documentPath"></param>
        public XmlDocumentationProvider(string documentPath)
        {
            if (documentPath == null)
            {
                throw new ArgumentNullException("documentPath");
            }
            XPathDocument xpath = new XPathDocument(documentPath);
            _documentNavigator = xpath.CreateNavigator();
        }

        /// <summary>
        /// 获取控制器的文档说明内容
        /// </summary>
        /// <param name="ctrlName"></param>
        /// <returns></returns>
        public string GetDocumentation(TypeInfo controller)
        {
            XPathNavigator typeNode = GetTypeNode(controller);
            return GetTagValue(typeNode, "summary");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="description"></param>
        /// <returns></returns>
        public string GetDocumentation(ApiDescription description)
        {
            XPathNavigator methodNode = GetMethodNode(description);
            return GetTagValue(methodNode, "summary");
        }

        /// <summary>
        /// 获取类型的说明
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public string GetDocumentation(Type type)
        {
            XPathNavigator typeNode = GetTypeNode(type);
            return GetTagValue(typeNode, "summary");
        }

        /// <summary>
        /// 属性类型文档说明
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        public string GetDocumentation(MemberInfo member)
        {
            string memberName = String.Format(CultureInfo.InvariantCulture, "{0}.{1}", GetTypeName(member.DeclaringType), member.Name);
            string expression = member.MemberType == MemberTypes.Field ? FieldExpression : PropertyExpression;
            string selectExpression = String.Format(CultureInfo.InvariantCulture, expression, memberName);
            XPathNavigator propertyNode = _documentNavigator.SelectSingleNode(selectExpression);
            return GetTagValue(propertyNode, "summary");
        }

        public virtual string GetDocumentation(ApiDescription apiDescription, ApiParameterDescription parameterDescriptor)
        {
            XPathNavigator methodNode = GetMethodNode(apiDescription);
            if (methodNode != null)
            {
                string parameterName = parameterDescriptor.Name;
                XPathNavigator parameterNode = methodNode.SelectSingleNode(String.Format(CultureInfo.InvariantCulture, ParameterExpression, parameterName));
                if (parameterNode != null)
                {
                    return parameterNode.Value.Trim();
                }
            }
            return null;
        }

        #region 私有方法

        /// <summary>
        /// 获取标签的内容
        /// </summary>
        /// <param name="parentNode"></param>
        /// <param name="tagName"></param>
        /// <returns></returns>
        private static string GetTagValue(XPathNavigator parentNode, string tagName)
        {
            if (parentNode != null)
            {
                XPathNavigator node = parentNode.SelectSingleNode(tagName);
                if (node != null)
                {
                    return node.Value.Trim();
                }
            }

            return null;
        }

        /// <summary>
        /// 获取文档中的类标签
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private XPathNavigator GetTypeNode(Type type)
        {
            string controllerTypeName = GetTypeName(type);
            string selectExpression = String.Format(CultureInfo.InvariantCulture, TypeExpression, controllerTypeName);
            return _documentNavigator.SelectSingleNode(selectExpression);
        }

        /// <summary>
        /// 获取类名称
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static string GetTypeName(Type type)
        {
            string name = type.FullName;
            if (type.IsGenericType)
            {
                Type genericType = type.GetGenericTypeDefinition();
                Type[] genericArguments = type.GetGenericArguments();
                string genericTypeName = genericType.FullName;

                genericTypeName = genericTypeName.Substring(0, genericTypeName.IndexOf('`'));
                string[] argumentTypeNames = genericArguments.Select(t => GetTypeName(t)).ToArray();
                name = String.Format(CultureInfo.InvariantCulture, "{0}{{{1}}}", genericTypeName, String.Join(",", argumentTypeNames));
            }
            if (type.IsNested)
            {
                name = name.Replace("+", ".");
            }
            return name;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="description"></param>
        /// <returns></returns>
        private XPathNavigator GetMethodNode(ApiDescription description)
        {
            var actionDescriptor = description.ActionDescriptor as ControllerActionDescriptor;
            if (actionDescriptor != null)
            {
                string selectExpression = String.Format(CultureInfo.InvariantCulture, MethodExpression, GetMemberName(actionDescriptor.MethodInfo));
                return _documentNavigator.SelectSingleNode(selectExpression);
            }
            return null;
        }

        /// <summary>
        /// 方法名称
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        private static string GetMemberName(MethodInfo method)
        {
            string name = String.Format(CultureInfo.InvariantCulture, "{0}.{1}", GetTypeName(method.DeclaringType), method.Name);
            ParameterInfo[] parameters = method.GetParameters();
            if (parameters.Length != 0)
            {
                string[] parameterTypeNames = parameters.Select(param => GetTypeName(param.ParameterType)).ToArray();
                name += String.Format(CultureInfo.InvariantCulture, "({0})", String.Join(",", parameterTypeNames));
            }

            return name;
        }

        #endregion
    }
}
