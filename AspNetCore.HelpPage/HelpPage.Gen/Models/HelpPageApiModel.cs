using Microsoft.AspNetCore.Mvc.ApiExplorer;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace HelpPage.Gen
{
    public class HelpPageApiModel
    {
        public HelpPageApiModel()
        {
            UriParameters = new Collection<ParameterDescription>();
            //SampleRequests = new Dictionary<MediaTypeHeaderValue, object>();
            //SampleResponses = new Dictionary<MediaTypeHeaderValue, object>();
            //ErrorMessages = new Collection<string>();
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
