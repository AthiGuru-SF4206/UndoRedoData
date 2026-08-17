using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;

namespace Syncfusion.Blazor
{
    internal static class MetadataExtension
    {
        private static Metadata CreateMetadata(PropertyInfo property)
        {
            if (property == null)
                return null!;

            var meta = new Metadata();
            meta.Property = property;
            object[] attributes = property.GetCustomAttributes(true).Cast<object>().ToArray();

            EnsureDisplay(attributes, meta);
            EnsureDisplayFormat(attributes, meta);
            EnsureEdit(attributes, meta);
            EnsureVisibility(attributes, meta);
            EnsureValidations(attributes, meta);
            
            var validationList = meta.Validations?.Where(e => e.Key.Equals("MaxLength", StringComparison.Ordinal) && e.Value?.Equals(-1) == true)?.ToList();
            if (validationList != null)
            {
                foreach (var validation in validationList)
                {
                    meta.Validations?.Remove(validation.Key);
                }
            }
            return meta;
        }

        public static Dictionary<string, Metadata> GetMetadatasForType(Type modelType)
        {
            var properties = modelType?.GetProperties();
            var res = new Dictionary<string, Metadata>();

            if (properties != null)
            {
                foreach (var prop in properties)
                {
                    res.TryAdd(prop.Name, CreateMetadata(prop));
                }
            }

            return res;
        }

        public static Metadata GetMetadataForProperty(Type modelType, string propertyName)
        {
            var complexfield = propertyName.Split('.');
            var modelAttribute = modelType?.CustomAttributes?.FirstOrDefault();
            var isComplexClass = modelAttribute != null && modelAttribute.AttributeType?.Name?.Contains("ValidateComplexType", StringComparison.CurrentCulture) == true;
            if (complexfield.Length > 1)
            {
                PropertyInfo? info = modelType?.GetProperty(complexfield[0]);
                var infoAttribute = info?.CustomAttributes?.FirstOrDefault();

                // In .NET 10, complex object validation is typically enabled with [ValidateComplexType].
                // The following check detects whether the model type is marked with ValidatableTypeAttribute,
                // which indicates that the type participates in custom complex validation.
                var hasValidatableTypeAttribute = modelType?.CustomAttributes.Any(ca => ca.AttributeType?.Name == "ValidatableTypeAttribute");
                if (isComplexClass || (infoAttribute != null && infoAttribute.AttributeType?.Name?.Contains("ValidateComplexType", StringComparison.CurrentCulture) == true)
                    || hasValidatableTypeAttribute == true)
                {
                    for (var i = 1; i < complexfield.Length; i++)
                    {
                        info = info?.PropertyType?.GetProperty(complexfield[i]);
                    }

                    return CreateMetadata(info!);
                }
                else
                {
                    return null!;
                }
            }
            else
            {
                var prop = modelType?.GetProperty(propertyName);
                return CreateMetadata(prop!);
            }
        }

        private static void EnsureValidations(object[] attributes, Metadata meta)
        {
            var valid = new Dictionary<string, object>();
            var messages = new Dictionary<string, string>();

            GetCustomAttribute<RequiredAttribute>(attributes, e =>
            {
                if (e?.ErrorMessage != null)
                {
                    messages.Add("required", e.ErrorMessage);
                }

                valid.Add("required", true);
            });

            GetCustomAttribute<StringLengthAttribute>(attributes, str =>
            {
                if (str?.ErrorMessage != null)
                {
                    messages.Add("rangeLength", str.ErrorMessage);
                }

                valid["rangeLength"] = new[] { str?.MinimumLength, str?.MaximumLength };
            });

            GetCustomAttribute<RangeAttribute>(attributes, range =>
            {
                if (range?.ErrorMessage != null)
                {
                    messages.Add("range", range.ErrorMessage);
                }

                valid["range"] = new object[] { range?.Minimum!, range?.Maximum! };
            });

            GetCustomAttribute<RegularExpressionAttribute>(attributes, regex =>
            {
                if (regex?.ErrorMessage != null)
                {
                    messages.Add("regex", regex.ErrorMessage);
                }

                valid["regex"] = regex?.Pattern!;
            });

            GetCustomAttribute<MinLengthAttribute>(attributes, minLength =>
            {
                if (minLength?.ErrorMessage != null)
                {
                    messages.Add("minLength", minLength.ErrorMessage);
                }

                valid["minLength"] = minLength?.Length!;
            });

            GetCustomAttribute<MaxLengthAttribute>(attributes, maxLength =>
            {
                if (maxLength?.ErrorMessage != null)
                {
                    messages.Add("maxLength", maxLength.ErrorMessage);
                }

                valid["maxLength"] = maxLength?.Length!;
            });

            GetCustomAttribute<EmailAddressAttribute>(attributes, mail =>
            {
                if (mail?.ErrorMessage != null)
                {
                    messages.Add("email", mail.ErrorMessage);
                }

                valid.Add("email", true);
            });

            GetCustomAttribute<CompareAttribute>(attributes, compare =>
            {
                if (compare?.ErrorMessage != null)
                {
                    messages.Add("equalTo", compare.ErrorMessage);
                }

                valid["equalTo"] = compare?.OtherProperty!;
            });

            GetCustomAttribute<DataTypeAttribute>(attributes, dt =>
            {
                string type;
                switch (dt.DataType)
                {
                    case DataType.Custom:
                        type = dt?.CustomDataType!;
                        break;
                    case DataType.Date:
                    case DataType.DateTime:
                        type = "date";
                        break;
                    case DataType.EmailAddress:
                        type = "email";
                        break;
                    case DataType.ImageUrl:
                        type = "url";
                        meta.Validations!["accept"] = "image/*";
                        break;
                    case DataType.Url:
                        type = "url";
                        break;
                    default:
                        meta.CustomDataType = dt.DataType.ToString();
                        return;
                }

                meta.CustomDataType = type;
                valid[type] = true;
            });

            if (messages.Count != 0)
            {
                valid.Add("messages", messages);
            }

            meta.Validations = valid;
        }

        private static void EnsureDisplayFormat(object[] attributes, Metadata meta)
        {
            GetCustomAttribute<DisplayFormatAttribute>(attributes, format =>
            {
                meta.FormatString = format.DataFormatString!;
                meta.ApplyFormatInEditMode = format.ApplyFormatInEditMode;
                meta.NeedsHtmlEncode = format.HtmlEncode;
                meta.NullDisplayText = format.NullDisplayText!;
                meta.ConvertEmptyStringToNull = format.ConvertEmptyStringToNull;
            });
        }

        private static void EnsureDisplay(object[] attributes, Metadata meta)
        {
            var display = GetCustomAttribute<DisplayAttribute>(attributes);
            if (display != null)
            {
                meta.AutoGenerateField = display.GetAutoGenerateField() ?? true;
                meta.HeaderText = string.IsNullOrEmpty(display.GetName()) ? (display.GetShortName() ?? string.Empty) : display.GetName();
                meta.Order = display.GetOrder() ?? 0;
                meta.AutoGenerateFilter = display.GetAutoGenerateFilter() ?? true;
                meta.Description = display.GetDescription();
            }
        }

        internal static string? GetDisplayName(this Enum enumValue)
        {
            string displayName;
            displayName = enumValue?.GetType()
                ?.GetMember(enumValue.ToString() )
                ?.FirstOrDefault()
                ?.GetCustomAttribute<DisplayAttribute>()
                ?.GetName()!;
            if (String.IsNullOrEmpty(displayName))
            {
                displayName = enumValue?.ToString()!; 
            }
            return displayName;
        }

        private static void EnsureVisibility(object[] attributes, Metadata meta)
        {
            GetCustomAttribute<ScaffoldColumnAttribute>(attributes, s => meta.Visible = s.Scaffold);
        }

        private static void EnsureEdit(object[] attributes, Metadata meta)
        {
            GetCustomAttribute<KeyAttribute>(attributes, key => meta.IsPrimaryKey = true);
            GetCustomAttribute<EditableAttribute>(attributes, edit => meta.ReadOnly = !edit.AllowEdit);
            if (!meta.ReadOnly)
                GetCustomAttribute<System.ComponentModel.ReadOnlyAttribute>(attributes, edit => meta.ReadOnly = edit.IsReadOnly);
            GetCustomAttribute<DatabaseGeneratedAttribute>(attributes, edit => meta.IsIdentity = edit.DatabaseGeneratedOption.Equals(DatabaseGeneratedOption.Identity));
        }

        private static T? GetCustomAttribute<T>(object[] attributes)
            where T : class
        {
            return attributes?.FirstOrDefault(a => a?.GetType() == typeof(T)) as T;
        }

        private static void GetCustomAttribute<T>(object[] attributes, Action<T> onComplete)
            where T : class
        {
            var att = GetCustomAttribute<T>(attributes);
            if (att != null && onComplete != null)
            {
                onComplete.Invoke(att);
            }
        }
    }
}
