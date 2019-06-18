using Microsoft.AspNetCore.Mvc.ApiExplorer;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http.Headers;

namespace HelpPage.Gen
{
    public class HelpPageApiModel
    {
        public HelpPageApiModel()
        {
            UriParameters = new Collection<ParameterDescription>();
            SampleRequests = new Dictionary<MediaTypeHeaderValue, object>();
            SampleResponses = new Dictionary<MediaTypeHeaderValue, object>();
            ErrorMessages = new Collection<string>();
        }

        public ApiDescription ApiDescription { get; set; }

        /// <summary>
        /// 接口描述说明
        /// </summary>
        public string Documentation { get; set; }

        /// <summary>
        /// Body 的参数模型
        /// </summary>
        public ModelDescription RequestModelDescription { get; set; }

        /// <summary>
        /// 请求说明
        /// </summary>
        public string RequestDocumentation { get; set; }

        /// <summary>
        /// Url参数
        /// </summary>
        public Collection<ParameterDescription> UriParameters { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public IList<ParameterDescription> RequestBodyParameters
        {
            get
            {
                return GetParameterDescriptions(RequestModelDescription);
            }
        }

        /// <summary>
        /// 响应描述
        /// </summary>
        public ModelDescription ResponseDescription { get; set; }

        /// <summary>
        /// 响应说明
        /// </summary>
        public string ResponseDocumentation { get; set; }


        /// <summary>
        /// 结果对象
        /// </summary>
        public ModelDescription ResourceDescription { get; set; }


        /// <summary>
        /// 结果对象参数集合
        /// </summary>
        public IList<ParameterDescription> ResourceProperties
        {
            get
            {
                return GetParameterDescriptions(ResourceDescription);
            }
        }


        /// <summary>
        /// 请求媒体格式
        /// </summary>
        public IDictionary<MediaTypeHeaderValue, object> SampleRequests { get; private set; }

        /// <summary>
        /// 响应媒体格式
        /// </summary>
        public IDictionary<MediaTypeHeaderValue, object> SampleResponses { get; private set; }


        /// <summary>
        /// 
        /// </summary>
        public Collection<string> ErrorMessages { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="modelDescription"></param>
        /// <returns></returns>
        private static IList<ParameterDescription> GetParameterDescriptions(ModelDescription modelDescription)
        {
            ComplexTypeModelDescription complexTypeModelDescription = modelDescription as ComplexTypeModelDescription;
            if (complexTypeModelDescription != null)
            {
                return complexTypeModelDescription.Properties;
            }

            CollectionModelDescription collectionModelDescription = modelDescription as CollectionModelDescription;
            if (collectionModelDescription != null)
            {
                complexTypeModelDescription = collectionModelDescription.ElementDescription as ComplexTypeModelDescription;
                if (complexTypeModelDescription != null)
                {
                    return complexTypeModelDescription.Properties;
                }
            }

            return null;
        }
    }
}
