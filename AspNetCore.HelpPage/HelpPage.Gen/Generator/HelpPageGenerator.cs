using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;

namespace HelpPage.Gen
{
    public class HelpPageGenerator : IHelpPageProvider
    {
        //
        public static ConcurrentDictionary<object, object> Properties => new ConcurrentDictionary<object, object>();

        private readonly IApiDescriptionGroupCollectionProvider _apiDescriptionsProvider;
        private HelpPageGenOptions _options;

        public HelpPageGenerator(IApiDescriptionGroupCollectionProvider apiDescriptionsProvider,
            IOptions<HelpPageGenOptions> options)
        {
            _options = options.Value;
            _apiDescriptionsProvider = apiDescriptionsProvider;

        }

        /// <summary>
        /// 文档提供者
        /// </summary>
        public XmlDocumentationProvider XmlProvider => _options.XmlProvider;


        /// <summary>
        /// 文档描述字典
        /// </summary>
        public IDictionary<string, OpenApiInfo> GetApiDocs()
        {
            return _options.ApiDocs;

        }

        /// <summary>
        /// 获取文档描述
        /// </summary>
        /// <param name="groupName"></param>
        /// <returns></returns>
        public KeyValuePair<string, OpenApiInfo> GetInfo(string groupName)
        {
            var docs = _options.ApiDocs;
            if (!String.IsNullOrEmpty(groupName))
            {
                if (docs.ContainsKey(groupName))
                {
                    return docs.FirstOrDefault(o => o.Key == groupName);
                }
            }
            return docs.FirstOrDefault();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="groupName"></param>
        /// <param name="info"></param>
        /// <returns></returns>

        public IList<ApiDescription> GetApiDescriptions(string groupName)
        {
            var items = _apiDescriptionsProvider.ApiDescriptionGroups.Items.SelectMany(group => group.Items)
               .Where(apiDesc => !apiDesc.CustomAttributes().OfType<ObsoleteAttribute>().Any())
               .Where(apiDesc => apiDesc.GroupName == null || apiDesc.GroupName == groupName).ToList();






            return items;
        }

        /// <summary>
        /// 获取接口详情的Model
        /// </summary>
        /// <param name="groupName"></param>
        /// <param name="apiDescriptionId"></param>
        /// <returns></returns>
        public HelpPageApiModel GetApiModel(string groupName, string apiDescriptionId)
        {
            HelpPageApiModel model = new HelpPageApiModel();

            var apiDescriptions = GetApiDescriptions(groupName);
            ApiDescription apiDescription = apiDescriptions.FirstOrDefault(api => (api.GroupName == null || api.GroupName == groupName)
                && String.Equals(api.GetFriendlyId(), apiDescriptionId, StringComparison.OrdinalIgnoreCase));

            if (apiDescription != null)
            {
                model = GenerateApiModel(apiDescription);

            }
            return model;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ModelDescriptionGenerator GetModelDescriptionGenerator()
        {
            return (ModelDescriptionGenerator)Properties.GetOrAdd(
                 typeof(ModelDescriptionGenerator),
                 k => InitializeModelDescriptionGenerator());
        }

        #region 私有方法

        private ModelDescriptionGenerator InitializeModelDescriptionGenerator()
        {
            ModelDescriptionGenerator modelGenerator = new ModelDescriptionGenerator(XmlProvider);
            List<ApiDescription> apis = _apiDescriptionsProvider.ApiDescriptionGroups.Items.SelectMany(group => group.Items)
               .Where(apiDesc => !apiDesc.CustomAttributes().OfType<ObsoleteAttribute>().Any()).ToList();
            foreach (ApiDescription api in apis)
            {
                var parameterDescription = api.ParameterDescriptions.FirstOrDefault(p => p.Source == BindingSource.Body);

                if (parameterDescription != null)
                {
                    modelGenerator.GetOrCreateModelDescription(parameterDescription.ParameterDescriptor.ParameterType);
                }
                if(api.SupportedResponseTypes!=null && api.SupportedResponseTypes.Any())
                {
                    ApiResponseType responseType = api.SupportedResponseTypes.FirstOrDefault();
                    modelGenerator.GetOrCreateModelDescription(responseType.Type);
                }
            }
            return modelGenerator;
        }

        /// <summary>
        /// 创建HelpPageApiModel
        /// </summary>
        /// <param name="apiDescription"></param>
        /// <returns></returns>
        private HelpPageApiModel GenerateApiModel(ApiDescription apiDescription)
        {
            HelpPageApiModel apiModel = new HelpPageApiModel()
            {
                ApiDescription = apiDescription,
            };
            ModelDescriptionGenerator modelGenerator = new ModelDescriptionGenerator(XmlProvider);
            HelpPageSampleGenerator sampleGenerator = new HelpPageSampleGenerator();

            GenerateUriParameters(apiModel, modelGenerator);
            GenerateRequestModelDescription(apiModel, modelGenerator, sampleGenerator);
            GenerateResourceDescription(apiModel, modelGenerator);

            return apiModel;
        }

        /// <summary>
        /// url参数 从path或者 query获取
        /// </summary>
        /// <param name="apiModel"></param>
        /// <param name="modelGenerator"></param>
        private void GenerateUriParameters(HelpPageApiModel apiModel, ModelDescriptionGenerator modelGenerator)
        {
            ApiDescription apiDescription = apiModel.ApiDescription;
            foreach (ApiParameterDescription apiParameter in apiDescription.ParameterDescriptions)
            {
                if (apiParameter.Source == BindingSource.Path || apiParameter.Source == BindingSource.Query)
                {
                    ParameterDescriptor parameterDescriptor = apiParameter.ParameterDescriptor;
                    Type parameterType = null;
                    ModelDescription typeDescription = null;
                    ComplexTypeModelDescription complexTypeDescription = null;

                    if (parameterDescriptor != null)
                    {
                        parameterType = parameterDescriptor.ParameterType;
                        typeDescription = modelGenerator.GetOrCreateModelDescription(parameterType);
                        complexTypeDescription = typeDescription as ComplexTypeModelDescription;
                    }
                    if (complexTypeDescription != null
                        && !IsBindableWithTypeConverter(parameterType))
                    {
                        foreach (ParameterDescription uriParameter in complexTypeDescription.Properties)
                        {
                            apiModel.UriParameters.Add(uriParameter);
                        }
                    }
                    else if (parameterDescriptor != null)
                    {
                        ParameterDescription uriParameter =
                            AddParameterDescription(apiModel, apiParameter, typeDescription);

                        if (!apiParameter.IsRequired)
                        {
                            uriParameter.Annotations.Add(new ParameterAnnotation() { Documentation = "必填" });
                        }

                        object defaultValue = apiParameter.DefaultValue;
                        if (defaultValue != null)
                        {
                            uriParameter.Annotations.Add(new ParameterAnnotation() { Documentation = "默认值：" + Convert.ToString(defaultValue, CultureInfo.InvariantCulture) });
                        }
                    }
                    else
                    {
                        ModelDescription modelDescription = modelGenerator.GetOrCreateModelDescription(typeof(string));
                        AddParameterDescription(apiModel, apiParameter, modelDescription);
                    }
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="apiModel"></param>
        /// <param name="apiParameter"></param>
        /// <param name="typeDescription"></param>
        /// <returns></returns>
        private ParameterDescription AddParameterDescription(HelpPageApiModel apiModel,
            ApiParameterDescription apiParameter, ModelDescription typeDescription)
        {
            ParameterDescription parameterDescription = new ParameterDescription
            {
                Name = apiParameter.Name,
                Documentation = XmlProvider.GetDocumentation(apiModel.ApiDescription, apiParameter),
                TypeDescription = typeDescription,
            };

            apiModel.UriParameters.Add(parameterDescription);
            return parameterDescription;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="parameterType"></param>
        /// <returns></returns>
        private static bool IsBindableWithTypeConverter(Type parameterType)
        {
            if (parameterType == null)
            {
                return false;
            }

            return TypeDescriptor.GetConverter(parameterType).CanConvertFrom(typeof(string));
        }


        /// <summary>
        /// body 参数
        /// </summary>
        /// <param name="apiModel"></param>
        /// <param name="modelGenerator"></param>
        /// <param name="sampleGenerator"></param>
        private void GenerateRequestModelDescription(HelpPageApiModel apiModel, ModelDescriptionGenerator modelGenerator, HelpPageSampleGenerator sampleGenerator)
        {
            ApiDescription apiDescription = apiModel.ApiDescription;
            foreach (ApiParameterDescription apiParameter in apiDescription.ParameterDescriptions)
            {
                if (apiParameter.Source == BindingSource.Body)
                {
                    Type parameterType = apiParameter.ParameterDescriptor.ParameterType;
                    apiModel.RequestModelDescription = modelGenerator.GetOrCreateModelDescription(parameterType);
                    apiModel.RequestDocumentation = XmlProvider.GetDocumentation(apiModel.ApiDescription, apiParameter);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="apiModel"></param>
        /// <param name="modelGenerator"></param>
        private static void GenerateResourceDescription(HelpPageApiModel apiModel, ModelDescriptionGenerator modelGenerator)
        {
            IList<ApiResponseType> responseTypes = apiModel.ApiDescription.SupportedResponseTypes;
            if (responseTypes != null && responseTypes.Any())
            {
                ApiResponseType responseType = responseTypes.FirstOrDefault();
                apiModel.ResourceDescription = modelGenerator.GetOrCreateModelDescription(responseType.Type);
            }
            //Type responseType = response.ResponseType ?? response.DeclaredType;
            //if (responseType != null && responseType != typeof(void))
            //{
            //    apiModel.ResourceDescription = modelGenerator.GetOrCreateModelDescription(responseType);
            //}
        }

        #endregion
    }
}
