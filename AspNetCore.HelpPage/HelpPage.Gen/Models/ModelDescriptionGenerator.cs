using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Serialization;

namespace HelpPage.Gen
{
    public class ModelDescriptionGenerator
    {
        private readonly IDictionary<Type, Func<object, string>> AnnotationTextGenerator = new Dictionary<Type, Func<object, string>>
        {
            { typeof(RequiredAttribute), a => "必填" },
            { typeof(RangeAttribute), a =>
                {
                    RangeAttribute range = (RangeAttribute)a;
                    return String.Format(CultureInfo.CurrentCulture, "数值 {0} 和 {1} 之间", range.Minimum, range.Maximum);
                }
            },
            { typeof(MaxLengthAttribute), a =>
                {
                    MaxLengthAttribute maxLength = (MaxLengthAttribute)a;
                    return String.Format(CultureInfo.CurrentCulture, "最大长度: {0}", maxLength.Length);
                }
            },
            { typeof(MinLengthAttribute), a =>
                {
                    MinLengthAttribute minLength = (MinLengthAttribute)a;
                    return String.Format(CultureInfo.CurrentCulture, "最小长度: {0}", minLength.Length);
                }
            },
            { typeof(StringLengthAttribute), a =>
                {
                    StringLengthAttribute strLength = (StringLengthAttribute)a;
                    return String.Format(CultureInfo.CurrentCulture, "长度在 {0} and {1} 和之间", strLength.MinimumLength, strLength.MaximumLength);
                }
            },
            { typeof(DataTypeAttribute), a =>
                {
                    DataTypeAttribute dataType = (DataTypeAttribute)a;
                    return String.Format(CultureInfo.CurrentCulture, "值类型 {0}", dataType.CustomDataType ?? dataType.DataType.ToString());
                }
            },
            { typeof(RegularExpressionAttribute), a =>
                {
                    RegularExpressionAttribute regularExpression = (RegularExpressionAttribute)a;
                    return String.Format(CultureInfo.CurrentCulture, "正则 pattern: {0}", regularExpression.Pattern);
                }
            },
        };

        /// <summary>
        /// 基础数据类型
        /// </summary>
        private readonly IDictionary<Type, string> DefaultTypeDocumentation = new Dictionary<Type, string>
        {
            { typeof(Int16), "integer" },
            { typeof(Int32), "integer" },
            { typeof(Int64), "integer" },
            { typeof(UInt16), "unsigned integer" },
            { typeof(UInt32), "unsigned integer" },
            { typeof(UInt64), "unsigned integer" },
            { typeof(Byte), "byte" },
            { typeof(Char), "character" },
            { typeof(SByte), "signed byte" },
            { typeof(Uri), "URI" },
            { typeof(Single), "decimal number" },
            { typeof(Double), "decimal number" },
            { typeof(Decimal), "decimal number" },
            { typeof(String), "string" },
            { typeof(Guid), "globally unique identifier" },
            { typeof(TimeSpan), "time interval" },
            { typeof(DateTime), "date" },
            { typeof(DateTimeOffset), "date" },
            { typeof(Boolean), "boolean" },
        };

        /// <summary>
        /// 
        /// </summary>
        public Dictionary<string, ModelDescription> GeneratedModels { get; private set; }

        public XmlDocumentationProvider XmlProvider { get; private set; }

        public ModelDescriptionGenerator(XmlDocumentationProvider xmlProvider)
        {
            this.XmlProvider = xmlProvider;
            GeneratedModels = new Dictionary<string, ModelDescription>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="modelType"></param>
        /// <returns></returns>
        public ModelDescription GetOrCreateModelDescription(Type modelType)
        {
            if (modelType == null)
            {
                throw new ArgumentNullException("modelType");
            }
            Type underlyingType = Nullable.GetUnderlyingType(modelType);
            if (underlyingType != null)
            {
                modelType = underlyingType;
            }
            ModelDescription modelDescription;
            string modelName = ModelNameHelper.GetModelName(modelType);
            if (GeneratedModels.TryGetValue(modelName, out modelDescription))
            {
                if (modelType != modelDescription.ModelType)
                {
                    throw new InvalidOperationException(
                        String.Format(
                            CultureInfo.CurrentCulture,
                            "无法创建参数模型描述.  '{0}' 未知的数据类型 '{1}' and '{2}'. ",
                            modelName,
                            modelDescription.ModelType.FullName,
                            modelType.FullName));
                }

                return modelDescription;
            }
            //如果是基础类型
            if (DefaultTypeDocumentation.ContainsKey(modelType))
            {
                return GenerateSimpleTypeModelDescription(modelType);
            }
            //枚举类型
            if (modelType.IsEnum)
            {
                return GenerateEnumTypeModelDescription(modelType);
            }
            //泛型
            if (modelType.IsGenericType)
            {
                Type[] genericArguments = modelType.GetGenericArguments();

                if (genericArguments.Length == 1)
                {
                    Type enumerableType = typeof(IEnumerable<>).MakeGenericType(genericArguments);
                    if (enumerableType.IsAssignableFrom(modelType))
                    {
                        return GenerateCollectionModelDescription(modelType, genericArguments[0]);
                    }
                }
                if (genericArguments.Length == 2)
                {
                    Type dictionaryType = typeof(IDictionary<,>).MakeGenericType(genericArguments);
                    if (dictionaryType.IsAssignableFrom(modelType))
                    {
                        return GenerateDictionaryModelDescription(modelType, genericArguments[0], genericArguments[1]);
                    }

                    Type keyValuePairType = typeof(KeyValuePair<,>).MakeGenericType(genericArguments);
                    if (keyValuePairType.IsAssignableFrom(modelType))
                    {
                        return GenerateKeyValuePairModelDescription(modelType, genericArguments[0], genericArguments[1]);
                    }
                }
            }
            //数组
            if (modelType.IsArray)
            {
                Type elementType = modelType.GetElementType();
                return GenerateCollectionModelDescription(modelType, elementType);
            }

            if (modelType == typeof(NameValueCollection))
            {
                return GenerateDictionaryModelDescription(modelType, typeof(string), typeof(string));
            }

            if (typeof(IDictionary).IsAssignableFrom(modelType))
            {
                return GenerateDictionaryModelDescription(modelType, typeof(object), typeof(object));
            }

            if (typeof(IEnumerable).IsAssignableFrom(modelType))
            {
                return GenerateCollectionModelDescription(modelType, typeof(object));
            }
            return GenerateComplexTypeModelDescription(modelType);
        }

        private ModelDescription GenerateSimpleTypeModelDescription(Type modelType)
        {
            SimpleTypeModelDescription simpleModelDescription = new SimpleTypeModelDescription
            {
                Name = ModelNameHelper.GetModelName(modelType),
                ModelType = modelType,
                Documentation = CreateDefaultDocumentation(modelType)
            }; 
            GeneratedModels.Add(simpleModelDescription.Name, simpleModelDescription);
            return simpleModelDescription;
        }
        /// <summary>
        /// 获取基础数据类型的文档参数说明
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private string CreateDefaultDocumentation(Type type)
        {
            string documentation;
            if (DefaultTypeDocumentation.TryGetValue(type, out documentation))
            {
                return documentation;
            }
            if (XmlProvider != null)
            {
                documentation = XmlProvider.GetDocumentation(type);
            }

            return documentation;
        }

        private EnumTypeModelDescription GenerateEnumTypeModelDescription(Type modelType)
        {
            EnumTypeModelDescription enumDescription = new EnumTypeModelDescription
            {
                Name = ModelNameHelper.GetModelName(modelType),
                ModelType = modelType,
                Documentation = CreateDefaultDocumentation(modelType)
            };
            bool hasDataContractAttribute = modelType.GetCustomAttribute<DataContractAttribute>() != null;
            foreach (FieldInfo field in modelType.GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                if (ShouldDisplayMember(field, hasDataContractAttribute))
                {
                    EnumValueDescription enumValue = new EnumValueDescription
                    {
                        Name = field.Name,
                        Value = field.GetRawConstantValue().ToString()
                    };
                    if (XmlProvider != null)
                    {
                        enumValue.Documentation = XmlProvider.GetDocumentation(field);
                    }
                    enumDescription.Values.Add(enumValue);
                }
            }
            GeneratedModels.Add(enumDescription.Name, enumDescription);

            return enumDescription;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="member"></param>
        /// <param name="hasDataContractAttribute"></param>
        /// <returns></returns>
        private static bool ShouldDisplayMember(MemberInfo member, bool hasDataContractAttribute)
        {
            JsonIgnoreAttribute jsonIgnore = member.GetCustomAttribute<JsonIgnoreAttribute>();
            XmlIgnoreAttribute xmlIgnore = member.GetCustomAttribute<XmlIgnoreAttribute>();
            IgnoreDataMemberAttribute ignoreDataMember = member.GetCustomAttribute<IgnoreDataMemberAttribute>();
            NonSerializedAttribute nonSerialized = member.GetCustomAttribute<NonSerializedAttribute>();
            ApiExplorerSettingsAttribute apiExplorerSetting = member.GetCustomAttribute<ApiExplorerSettingsAttribute>();

            bool hasMemberAttribute = member.DeclaringType.IsEnum ?
                member.GetCustomAttribute<EnumMemberAttribute>() != null :
                member.GetCustomAttribute<DataMemberAttribute>() != null;
            return jsonIgnore == null &&
                xmlIgnore == null &&
                ignoreDataMember == null &&
                nonSerialized == null &&
                (apiExplorerSetting == null || !apiExplorerSetting.IgnoreApi) &&
                (!hasDataContractAttribute || hasMemberAttribute);
        }

        /// <summary>
        /// 泛型对象
        /// </summary>
        /// <param name="modelType"></param>
        /// <param name="elementType"></param>
        /// <returns></returns>
        private CollectionModelDescription GenerateCollectionModelDescription(Type modelType, Type elementType)
        {
            ModelDescription collectionModelDescription = GetOrCreateModelDescription(elementType);
            if (collectionModelDescription != null)
            {
                return new CollectionModelDescription
                {
                    Name = ModelNameHelper.GetModelName(modelType),
                    ModelType = modelType,
                    ElementDescription = collectionModelDescription
                };
            }

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="modelType"></param>
        /// <param name="keyType"></param>
        /// <param name="valueType"></param>
        /// <returns></returns>
        private DictionaryModelDescription GenerateDictionaryModelDescription(Type modelType, Type keyType, Type valueType)
        {
            ModelDescription keyModelDescription = GetOrCreateModelDescription(keyType);
            ModelDescription valueModelDescription = GetOrCreateModelDescription(valueType);

            return new DictionaryModelDescription
            {
                Name = ModelNameHelper.GetModelName(modelType),
                ModelType = modelType,
                KeyModelDescription = keyModelDescription,
                ValueModelDescription = valueModelDescription
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="modelType"></param>
        /// <param name="keyType"></param>
        /// <param name="valueType"></param>
        /// <returns></returns>
        private KeyValuePairModelDescription GenerateKeyValuePairModelDescription(Type modelType, Type keyType, Type valueType)
        {
            ModelDescription keyModelDescription = GetOrCreateModelDescription(keyType);
            ModelDescription valueModelDescription = GetOrCreateModelDescription(valueType);

            return new KeyValuePairModelDescription
            {
                Name = ModelNameHelper.GetModelName(modelType),
                ModelType = modelType,
                KeyModelDescription = keyModelDescription,
                ValueModelDescription = valueModelDescription
            };
        }

        private ModelDescription GenerateComplexTypeModelDescription(Type modelType)
        {
            ComplexTypeModelDescription complexModelDescription = new ComplexTypeModelDescription
            {
                Name = ModelNameHelper.GetModelName(modelType),
                ModelType = modelType,
                Documentation = CreateDefaultDocumentation(modelType)
            };

            GeneratedModels.Add(complexModelDescription.Name, complexModelDescription);
            bool hasDataContractAttribute = modelType.GetCustomAttribute<DataContractAttribute>() != null;
            PropertyInfo[] properties = modelType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo property in properties)
            {
                if (ShouldDisplayMember(property, hasDataContractAttribute))
                {
                    ParameterDescription propertyModel = new ParameterDescription
                    {
                        Name = GetMemberName(property, hasDataContractAttribute)
                    };

                    if (XmlProvider != null)
                    {
                        propertyModel.Documentation = XmlProvider.GetDocumentation(property);
                    }

                    GenerateAnnotations(property, propertyModel);
                    complexModelDescription.Properties.Add(propertyModel);
                    propertyModel.TypeDescription = GetOrCreateModelDescription(property.PropertyType);
                }
            }

            FieldInfo[] fields = modelType.GetFields(BindingFlags.Public | BindingFlags.Instance);
            foreach (FieldInfo field in fields)
            {
                if (ShouldDisplayMember(field, hasDataContractAttribute))
                {
                    ParameterDescription propertyModel = new ParameterDescription
                    {
                        Name = GetMemberName(field, hasDataContractAttribute)
                    };

                    if (XmlProvider != null)
                    {
                        propertyModel.Documentation = XmlProvider.GetDocumentation(field);
                    }

                    complexModelDescription.Properties.Add(propertyModel);
                    propertyModel.TypeDescription = GetOrCreateModelDescription(field.FieldType);
                }
            }

            return complexModelDescription;
        }

        private static string GetMemberName(MemberInfo member, bool hasDataContractAttribute)
        {
            JsonPropertyAttribute jsonProperty = member.GetCustomAttribute<JsonPropertyAttribute>();
            if (jsonProperty != null && !String.IsNullOrEmpty(jsonProperty.PropertyName))
            {
                return jsonProperty.PropertyName;
            }

            if (hasDataContractAttribute)
            {
                DataMemberAttribute dataMember = member.GetCustomAttribute<DataMemberAttribute>();
                if (dataMember != null && !String.IsNullOrEmpty(dataMember.Name))
                {
                    return dataMember.Name;
                }
            }

            return member.Name;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="property"></param>
        /// <param name="propertyModel"></param>
        private void GenerateAnnotations(MemberInfo property, ParameterDescription propertyModel)
        {
            List<ParameterAnnotation> annotations = new List<ParameterAnnotation>();

            IEnumerable<Attribute> attributes = property.GetCustomAttributes();
            foreach (Attribute attribute in attributes)
            {
                Func<object, string> textGenerator;
                if (AnnotationTextGenerator.TryGetValue(attribute.GetType(), out textGenerator))
                {
                    annotations.Add(
                        new ParameterAnnotation
                        {
                            AnnotationAttribute = attribute,
                            Documentation = textGenerator(attribute)
                        });
                }
            }

            // Rearrange the annotations
            annotations.Sort((x, y) =>
            {
                // Special-case RequiredAttribute so that it shows up on top
                if (x.AnnotationAttribute is RequiredAttribute)
                {
                    return -1;
                }
                if (y.AnnotationAttribute is RequiredAttribute)
                {
                    return 1;
                }

                // Sort the rest based on alphabetic order of the documentation
                return String.Compare(x.Documentation, y.Documentation, StringComparison.OrdinalIgnoreCase);
            });

            foreach (ParameterAnnotation annotation in annotations)
            {
                propertyModel.Annotations.Add(annotation);
            }
        }
    }
}
