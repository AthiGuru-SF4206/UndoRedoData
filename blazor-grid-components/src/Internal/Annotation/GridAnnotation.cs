using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Dynamic;

namespace Syncfusion.Blazor.Grids
{
#pragma warning disable BL0005 // Component parameter should not be set outside of its component.
    internal class GridAnnotation
    {
        internal static void MapAnnotation(GridColumn columns, Type type)
        {
            if (type == null || columns?.Field == null)
            {
                return;
            }

            var meta = MetadataExtension.GetMetadataForProperty(type, columns.Field);
            if (meta == null)
            {
                return;
            }

            if (columns.AllowEditing)
            {
                columns.AllowEditing = !meta.ReadOnly;
            }

            if (!columns.IsPrimaryKey)
            {
                columns.IsPrimaryKey = meta.IsPrimaryKey;
            }

            if (!columns.IsIdentity)
            {
                columns.IsIdentity = meta.IsIdentity;
            }

            if (columns.Visible)
            {
                columns.Visible = meta.Visible;
            }

            if (columns.HeaderText == "undefined" || string.IsNullOrEmpty(columns.HeaderText))
            {
                columns.HeaderText = meta.HeaderText!;
            }

            if (string.IsNullOrEmpty(columns.Format))
            {
                columns.Format = meta.FormatString!;
            }

            if (string.IsNullOrEmpty(columns.ForeignKeyField))
            {
                columns.ForeignKeyField = meta.ForeignKey!;
            }

            PropertyChanges(columns, meta);

            ValidationRules? rules = null;
            if (meta.Validations != null && meta.Validations.Count != 0)
            {
                rules = ValidationRules.ToRules(ValidationRules.ToInstance(meta.Validations));
                if (meta.Validations.TryGetValue("messages", out object? messages))
                {
                    rules.Messages = messages as Dictionary<string, object> ?? new Dictionary<string, object>();
                }
            }

            if (columns.ValidationRules == null && rules != null)
            {
                columns.ValidationRules = rules;
            }
        }

        internal static void MapAnnotation(ref List<GridColumn> columns, Type type, bool isDynamic = false)
        {
            if (type == null || (columns != null && columns.Count > 0) || isDynamic)
            {
                return;
            }

            var metadata = MetadataExtension.GetMetadatasForType(type);
            var orderedMetaDataCol = metadata?.OrderBy(x => x.Value?.Order);

            columns ??= new List<GridColumn>();
            
            int index = 0;
            foreach (var item in orderedMetaDataCol!)
            {
                var meta = item.Value;
                
                if (meta?.AutoGenerateField != true)
                {
                    continue;
                }

                var column = new GridColumn();
                column.Field = meta.Property?.Name!;
                column.HeaderText = string.IsNullOrEmpty(meta.HeaderText) ? meta.Property?.Name ?? string.Empty : meta.HeaderText;
                column.AllowEditing = !meta.ReadOnly;

                if (meta.IsPrimaryKey)
                {
                    column.IsPrimaryKey = meta.IsPrimaryKey;
                }

                if (!meta.Visible)
                {
                    column.Visible = meta.Visible;
                }

                if (!string.IsNullOrEmpty(meta.FormatString))
                {
                    column.Format = meta.FormatString;
                }

                if (meta.IsIdentity)
                {
                    column.IsIdentity = meta.IsIdentity;
                }

                PropertyChanges(column, meta);
                
                column.OriginalIndex = column.Index = index++;

                ValidationRules? rules = null;

                if (meta.Validations != null && meta.Validations.Count != 0)
                {
                    rules = ValidationRules.ToRules(ValidationRules.ToInstance(meta.Validations));
                    if (meta.Validations.TryGetValue("messages", out object? messages))
                    {
                        rules.Messages = messages as Dictionary<string, object> ?? new Dictionary<string, object>();
                    }
                }

                if (column.ValidationRules == null && rules != null)
                {
                    column.ValidationRules = rules;
                }

                columns.Add(column);
            }
        }

        internal static void MapDynamicAnnotation<T>(ref List<GridColumn> columns, Type type, IEnumerable<T> data)
        {
            if (data == null || type == null || (columns != null && columns.Count > 0))
            {
                return;
            }

            bool isDynamicObjectType = type.BaseType == typeof(DynamicObject);
            bool isExpandoObjectType = type == typeof(ExpandoObject);

            if (!isExpandoObjectType && !isDynamicObjectType)
            {
                return;
            }

            object? firstObject = data.FirstOrDefault();
            if (firstObject == null)
                return;

            columns ??= new List<GridColumn>();

            int index = 0;
        
            var properties = new List<string>();

            if (isExpandoObjectType)
            {
                properties = ((IDictionary<string, object>)firstObject).Keys.ToList();
            }
            else if (isDynamicObjectType)
            {
                properties = ((DynamicObject)firstObject).GetDynamicMemberNames().ToList();
            }

            foreach (var field in properties)
            {
                var column = new GridColumn();
                column.Field = field;
                column.Index = index++;
                columns.Add(column);
            }
        }

        internal static void PropertyChanges(GridColumn column, Metadata meta)
        {
            if (column.AllowFiltering)
            {
                column.AllowFiltering = meta.AutoGenerateFilter;
            }

            if (column.DisableHtmlEncode)
            {
                column.DisableHtmlEncode = !meta.NeedsHtmlEncode;
            }

            if (column.NullDisplayText == null)
            {
                column.NullDisplayText = meta.NullDisplayText!;
            }

            if (meta.Description != null)
            {
                column.ClipMode = ClipMode.EllipsisWithTooltip;
                column.Description = meta.Description;
            }

            if (!column.ConvertEmptyStringToNull)
            {
                column.ConvertEmptyStringToNull = meta.ConvertEmptyStringToNull;
            }

            if (column.ApplyFormatInEditMode)
            {
                column.ApplyFormatInEditMode = meta.ApplyFormatInEditMode;
            }
        }
    }
#pragma warning restore BL0005 // Component parameter should not be set outside of its component.
}
