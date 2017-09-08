// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

/* LinqExtensions.cs --
 * Ars Magna project, http://arsmagna.ru
 * -------------------------------------------------------
 * Status: poor
 */

#region Using directives

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

#endregion

namespace TinyClient
{
    public static class LinqExtensions
    {
        public static readonly RecordField[] EmptyFieldArray
            = new RecordField[0];

        public static readonly SubField[] EmptySubFieldArray
            = new SubField[0];

        public static readonly string[] EmptyStringArray
            = new string[0];

        // ==========================================================

        public static RecordField AddSubField
            (
                this RecordField field,
                char code,
                string value
            )
        {
            SubField subField = new SubField { Code = code, Value = value };
            field.SubFields.Add(subField);

            return field;
        }

        public static RecordField AddNonEmptySubField
            (
                this RecordField field,
                char code,
                string value
            )
        {
            if (!string.IsNullOrEmpty(value))
            {
                field.AddSubField(code, value);
            }

            return field;
        }

        // ==========================================================

        public static SubField[] AllSubFields
            (
                this IEnumerable fields
            )
        {
            return fields
                .Cast<RecordField>()
                .SelectMany(field => field.SubFields.Cast<SubField>())
                .ToArray();
        }

        // ==========================================================

        public static RecordField[] GetField
            (
                this IEnumerable fields,
                int tag
            )
        {
            return fields
                .Cast<RecordField>()
                .Where(field => field.Tag == tag)
                .ToArray();
        }

        public static RecordField[] GetField
            (
                this FieldCollection fields,
                int tag
            )
        {
            int count = fields.Length;
            List<RecordField> result = null;
            for (int i = 0; i < count; i++)
            {
                if (fields[i].Tag == tag)
                {
                    if (ReferenceEquals(result, null))
                    {
                        result = new List<RecordField>();
                    }
                    result.Add(fields[i]);
                }
            }

            return ReferenceEquals(result, null)
                ? EmptyFieldArray
                : result.ToArray();
        }

        public static RecordField GetField
            (
                this IEnumerable fields,
                int tag,
                int occurrence
            )
        {
            return fields
                .Cast<RecordField>()
                .GetField(tag)
                .GetOccurrence(occurrence);
        }

        public static RecordField GetField
            (
                this FieldCollection fields,
                int tag,
                int occurrence
            )
        {
            int count = fields.Length;
            for (int i = 0; i < count; i++)
            {
                if (fields[i].Tag == tag)
                {
                    if (occurrence == 0)
                    {
                        return fields[i];
                    }
                    occurrence--;
                }
            }

            return null;
        }

        public static RecordField[] GetField
            (
                this IEnumerable fields,
                params int[] tags
            )
        {
            return fields
                .Cast<RecordField>()
                .Where(field => Utility.OneOf(field.Tag, tags))
                .ToArray();
        }

        public static RecordField[] GeField
            (
                this FieldCollection fields,
                params int[] tags
            )
        {
            int count = fields.Length;
            List<RecordField> result = null;
            for (int i = 0; i < count; i++)
            {
                if (Utility.OneOf(fields[i].Tag, tags))
                {
                    if (ReferenceEquals(result, null))
                    {
                        result = new List<RecordField>();
                    }
                    result.Add(fields[i]);
                }
            }

            return ReferenceEquals(result, null)
                ? EmptyFieldArray
                : result.ToArray();
        }

        public static RecordField GetField
            (
                this IEnumerable fields,
                int[] tags,
                int occurrence
            )
        {
            return fields
                .GetField(tags)
                .GetOccurrence(occurrence);
        }

        public static RecordField GetField
            (
                this FieldCollection fields,
                int[] tags,
                int occurrence
            )
        {
            int count = fields.Length;
            for (int i = 0; i < count; i++)
            {
                if (Utility.OneOf(fields[i].Tag, tags))
                {
                    if (occurrence == 0)
                    {
                        return fields[i];
                    }
                    occurrence--;
                }
            }

            return null;
        }

        public static RecordField[] GetField
            (
                this IEnumerable fields,
                Func<RecordField, bool> predicate
            )
        {
            return fields
                .Cast<RecordField>()
                .Where(predicate)
                .ToArray();
        }

        public static RecordField[] GetField
            (
                this IEnumerable fields,
                Action<RecordField> action
            )
        {
            RecordField[] result = fields.Cast<RecordField>().ToArray();
            if (!ReferenceEquals(action, null))
            {
                foreach (RecordField field in result)
                {
                    action(field);
                }
            }

            return result;
        }

        public static RecordField[] GetField
            (
                this IEnumerable fields,
                Action<RecordField> fieldAction,
                Action<SubField> subFieldAction
            )
        {
            RecordField[] result = fields.Cast<RecordField>().ToArray();
            if (!ReferenceEquals(fieldAction, null)
                || !ReferenceEquals(subFieldAction, null))
            {
                foreach (RecordField field in result)
                {
                    if (!ReferenceEquals(fieldAction, null))
                    {
                        fieldAction(field);
                    }
                    if (!ReferenceEquals(subFieldAction, null))
                    {
                        foreach (SubField subField in field.SubFields)
                        {
                            subFieldAction(subField);
                        }
                    }
                }
            }
            return result;
        }

        public static RecordField[] GetField
            (
                this IEnumerable fields,
                Action<SubField> action
            )
        {
            RecordField[] result = fields.Cast<RecordField>().ToArray();
            if (!ReferenceEquals(action, null))
            {
                foreach (RecordField field in result)
                {
                    foreach (SubField subField in field.SubFields)
                    {
                        action(subField);
                    }
                }
            }
            return result;
        }

        public static RecordField[] GetField
            (
                this IEnumerable fields,
                Func<SubField, bool> predicate
            )
        {
            return fields
                .Cast<RecordField>()
                .Where(field => field.SubFields.Cast<SubField>().Any(predicate))
                .ToArray();
        }

        public static RecordField[] GetField
            (
                this IEnumerable fields,
                char[] codes,
                Func<SubField, bool> predicate
            )
        {
            return fields
                .Cast<RecordField>()
                .Where(field => field.SubFields
                    .Cast<SubField>()
                    .Any(sub => Utility.OneOf(sub.Code, codes)
                                && predicate(sub)))
                .ToArray();
        }

        public static RecordField[] GetField
            (
                this IEnumerable fields,
                char[] codes,
                params string[] values
            )
        {
            return fields
                .Cast<RecordField>()
                .Where(field => field.SubFields
                    .Cast<SubField>()
                    .Any(sub => Utility.OneOf(sub.Code, codes)
                                && Utility.OneOf(sub.Value, values)))
                .ToArray();
        }

        public static RecordField[] GetField
            (
                this IEnumerable fields,
                char code,
                string value
            )
        {
            return fields
                .Cast<RecordField>()
                .Where(field => field.SubFields
                    .Cast<SubField>()
                    .Any(sub => Utility.SameChar(code, sub.Code)
                                && Utility.SameString(value, sub.Value)))
                .ToArray();
        }

        public static RecordField[] GetField
            (
                this IEnumerable fields,
                Func<RecordField, bool> fieldPredicate,
                Func<SubField, bool> subPredicate
            )
        {
            return fields
                .Cast<RecordField>()
                .Where(fieldPredicate)
                .Where(field => field.SubFields
                .Cast<SubField>()
                .Any(subPredicate))
                .ToArray();
        }

        // ==========================================================

        public static SubField GetFirstSubField
            (
                this IEnumerable subFields,
                char code
            )
        {
            return subFields
                .Cast<SubField>()
                .FirstOrDefault(sub => Utility.SameChar(sub.Code, code));
        }

        public static SubField GetFirstSubField
            (
                this IEnumerable subFields,
                params char[] codes
            )
        {
            return subFields
                .Cast<SubField>()
                .FirstOrDefault(sub => Utility.OneOf(sub.Code, codes));
        }

        public static SubField GetFirstSubField
            (
                this IEnumerable subFields,
                char code,
                string value
            )
        {
            return subFields
                .Cast<SubField>()
                .FirstOrDefault
                (
                    sub => Utility.SameChar(sub.Code, code)
                        && Utility.SameString(sub.Value, value)
                );
        }

        // ==========================================================

        public static T GetOccurrence<T>
            (
                this T[] array,
                int occurrence
            )
        {
            occurrence = occurrence >= 0
                ? occurrence
                : array.Length + occurrence;
            T result = default(T);
            if (occurrence >= 0 && occurrence < array.Length)
            {
                result = array[occurrence];
            }
            return result;
        }

        public static T GetOccurrence<T>
            (
                this IList list,
                int occurrence
            )
        {
            occurrence = occurrence >= 0
                ? occurrence
                : list.Count + occurrence;
            T result = default(T);
            if (occurrence >= 0 && occurrence < list.Count)
            {
                result = (T)list[occurrence];
            }
            return result;
        }

        // ==========================================================

        public static SubField[] GetSubField
            (
                this IEnumerable subFields,
                char code
            )
        {
            return subFields
                .Cast<SubField>()
                .Where(sub => Utility.SameChar(sub.Code, code))
                .ToArray();
        }

        public static SubField[] GetSubField
            (
                this SubFieldCollection subFields,
                char code
            )
        {
            int count = subFields.Length;
            List<SubField> result = null;
            for (int i = 0; i < count; i++)
            {
                if (Utility.SameChar(subFields[i].Code, code))
                {
                    if (ReferenceEquals(result, null))
                    {
                        result = new List<SubField>();
                    }
                    result.Add(subFields[i]);
                }
            }

            return ReferenceEquals(result, null)
                ? EmptySubFieldArray
                : result.ToArray();
        }

        public static SubField[] GetSubField
            (
                this IEnumerable subFields,
                params char[] codes
            )
        {
            return subFields
                .Cast<SubField>()
                .Where(sub => Utility.OneOf(sub.Code, codes))
                .ToArray();
        }

        public static SubField[] GetSubField
            (
                this SubFieldCollection subFields,
                params char[] codes
            )
        {
            int count = subFields.Length;
            List<SubField> result = null;
            for (int i = 0; i < count; i++)
            {
                if (Utility.OneOf(subFields[i].Code, codes))
                {
                    if (ReferenceEquals(result, null))
                    {
                        result = new List<SubField>();
                    }
                    result.Add(subFields[i]);
                }
            }

            return ReferenceEquals(result, null)
                ? EmptySubFieldArray
                : result.ToArray();
        }

        public static SubField[] GetSubField
            (
                this IEnumerable subFields,
                Action<SubField> action
            )
        {
            SubField[] result = subFields
                .Cast<SubField>()
                .ToArray();

            if (!ReferenceEquals(action, null))
            {
                foreach (SubField subField in result)
                {
                    action(subField);
                }
            }

            return result;
        }

        public static SubField[] GetSubField
            (
                this SubFieldCollection subFields,
                Action<SubField> action
            )
        {
            int count = subFields.Length;
            SubField[] result = new SubField[count];
            for (int i = 0; i < count; i++)
            {
                SubField item = subFields[i];
                result[i] = item;
                action(item);
            }

            return result;
        }

        public static SubField[] GetSubField
            (
                this IEnumerable fields,
                Func<RecordField, bool> fieldPredicate,
                Func<SubField, bool> subPredicate
            )
        {
            return fields
                .Cast<RecordField>()
                .Where(fieldPredicate)
                .GetSubField()
                .Where(subPredicate)
                .ToArray();
        }

        public static SubField[] GetSubField
            (
                this IEnumerable fields,
                int[] tags,
                char[] codes
            )
        {
            return fields
                .Cast<RecordField>()
                .GetField(tags)
                .GetSubField(codes)
                .ToArray();
        }

        public static SubField[] GetSubField
            (
                this RecordField field,
                char code
            )
        {
            SubFieldCollection subFields = field.SubFields;
            int count = subFields.Length;
            List<SubField> result = null;
            for (int i = 0; i < count; i++)
            {
                if (Utility.SameChar(subFields[i].Code, code))
                {
                    if (ReferenceEquals(result, null))
                    {
                        result = new List<SubField>();
                    }
                    result.Add(subFields[i]);
                }
            }

            return ReferenceEquals(result, null)
                ? EmptySubFieldArray
                : result.ToArray();
        }

        public static SubField GetSubField
            (
                this RecordField field,
                char code,
                int occurrence
            )
        {
            SubFieldCollection subFields = field.SubFields;
            int count = subFields.Length;
            for (int i = 0; i < count; i++)
            {
                if (Utility.SameChar(subFields[i].Code, code))
                {
                    if (occurrence == 0)
                    {
                        return subFields[i];
                    }
                    occurrence--;
                }
            }

            return null;
        }

        // ==========================================================

        public static string GetSubFieldValue
            (
                this SubField subField
            )
        {
            return subField == null
                       ? null
                       : subField.Value;
        }

        public static string[] GetSubFieldValue
            (
                this IEnumerable subFields
            )
        {
            return subFields
                .Cast<SubField>()
                .Select(subField => subField.Value)
                .ToArray();
        }

        // ==========================================================

        public static bool HaveNotSubField
            (
                this RecordField field,
                char code
            )
        {
            SubFieldCollection subFields = field.SubFields;
            int count = subFields.Length;
            for (int i = 0; i < count; i++)
            {
                if (Utility.SameChar(subFields[i].Code, code))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool HaveNotSubField
            (
                this RecordField field,
                params char[] codes
            )
        {
            SubFieldCollection subFields = field.SubFields;
            int count = subFields.Length;
            for (int i = 0; i < count; i++)
            {
                if (Utility.OneOf(subFields[i].Code, codes))
                {
                    return false;
                }
            }

            return true;
        }

        // ==========================================================

        public static bool HaveSubField
            (
                this RecordField field,
                char code
            )
        {
            SubFieldCollection subFields = field.SubFields;
            int count = subFields.Length;
            for (int i = 0; i < count; i++)
            {
                if (Utility.SameChar(subFields[i].Code, code))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool HaveSubField
            (
                this RecordField field,
                params char[] codes
            )
        {
            SubFieldCollection subFields = field.SubFields;
            int count = subFields.Length;
            for (int i = 0; i < count; i++)
            {
                if (Utility.OneOf(subFields[i].Code, codes))
                {
                    return true;
                }
            }

            return false;
        }


        // ==========================================================

        public static RecordField[] NotNullValue
            (
                this IEnumerable fields
            )
        {
            return fields
                .Cast<RecordField>()
                .Where(field => !string.IsNullOrEmpty(field.Value))
                .ToArray();
        }

        public static SubField GetFirstSubField
            (
                this RecordField field,
                char code
            )
        {
            return field.SubFields
                .Cast<SubField>()
                .FirstOrDefault(subField => Utility.SameChar(subField.Code, code));
        }

        public static string GetSubFieldValue
            (
                this RecordField field,
                char code,
                int occurrence
            )
        {
            SubField result = field.GetSubField
                (
                    code,
                    occurrence
                );
            return ReferenceEquals(result, null)
                ? null
                : result.Value;
        }

        public static string GetFirstSubFieldValue
            (
                this RecordField field,
                char code
            )
        {
            SubField result = field.GetFirstSubField(code);
            return ReferenceEquals(result, null)
                ? null
                : result.Value;
        }

        public static SubField[] FilterSubFields
            (
                this IEnumerable subFields,
                params char[] codes
            )
        {
            return subFields
                .Cast<SubField>()
                .Where(subField => Utility.OneOf(subField.Code, codes))
                .ToArray();
        }

        public static SubField[] FilterSubFields
            (
                this RecordField field,
                params char[] codes
            )
        {
            return field.SubFields.FilterSubFields(codes);
        }

        public static RecordField SetSubField
            (
                this RecordField field,
                char code,
                string value
            )
        {
            SubField subField = field.SubFields
                .Cast<SubField>()
                .FirstOrDefault(sub => Utility.SameChar(code, sub.Code));
            if (ReferenceEquals(subField, null))
            {
                subField = new SubField { Code = code };
                field.SubFields.Add(subField);
            }
            subField.Value = value;
            return field;
        }

        public static RecordField ReplaceSubField
            (
                this RecordField field,
                char code,
                string oldValue,
                string newValue
            )
        {
            var found = field.SubFields
                .Cast<SubField>()
                .Where
                    (
                        subField => Utility.SameChar(code, subField.Code)
                        && Utility.SameString(oldValue, subField.Value)
                    );
            foreach (SubField subField in found)
            {
                subField.Value = newValue;
            }
            return field;
        }

        public static RecordField RemoveSubField
            (
                this RecordField field,
                char code
            )
        {
            SubField[] found = field.SubFields
                .Cast<SubField>()
                .Where(sub => Utility.SameChar(sub.Code, code))
                .ToArray();
            foreach (SubField subField in found)
            {
                field.SubFields.Remove(subField);
            }
            return field;
        }

        public static RecordField ReplaceSubField
            (
                this RecordField field,
                char code,
                string newValue
            )
        {
            string oldValue = field.GetSubFieldValue(code, 0);
            bool changed = !Utility.SameString(oldValue, newValue);
            if (changed)
            {
                field.SetSubField(code, newValue);
            }
            return field;
        }

        public static string GetFieldValue
            (
                this RecordField field
            )
        {
            return ReferenceEquals(field, null)
                       ? null
                       : field.Value;
        }

        public static string[] GetFieldValue
            (
                this IEnumerable fields
            )
        {
            return fields
                .Cast<RecordField>()
                .Select(field => field.Value)
                .ToArray();
        }

        public static RecordField GetFirstField
            (
                this IEnumerable fields,
                int tag
            )
        {
            return fields
                .Cast<RecordField>()
                .FirstOrDefault(field => field.Tag == tag);
        }

        public static RecordField GetFirstField
            (
                this IEnumerable fields,
                params int[] tags
            )
        {
            return fields
                .Cast<RecordField>()
                .FirstOrDefault(field => Utility.OneOf(field.Tag, tags));
        }

        public static SubField GetFirstSubField
            (
                this IEnumerable fields,
                int tag,
                char code
            )
        {
            foreach (RecordField field in fields)
            {
                if (field.Tag == tag)
                {
                    foreach (SubField subField in field.SubFields)
                    {
                        if (Utility.SameChar(subField.Code, code))
                        {
                            return subField;
                        }
                    }
                }
            }
            return null;
        }

        public static string GetFirstFieldValue
            (
                this IEnumerable fields,
                int tag
            )
        {
            foreach (RecordField field in fields)
            {
                if (field.Tag == tag)
                {
                    return field.Value;
                }
            }
            return null;
        }

        public static string[] GetFieldValue
            (
                this IEnumerable fields,
                int tag
            )
        {
            ArrayList list = new ArrayList();
            foreach (RecordField field in fields)
            {
                if (field.Tag == tag
                    && !string.IsNullOrEmpty(field.Value))
                {
                    list.Add(field.Value);
                }
            }
            string[] result = new string[list.Count];
            list.CopyTo(result);
            return result;
        }

        public static string GetFirstSubFieldValue
            (
                this IEnumerable fields,
                int tag,
                char code
            )
        {
            foreach (RecordField field in fields)
            {
                if (field.Tag == tag)
                {
                    foreach (SubField subField in field.SubFields)
                    {
                        if (Utility.SameChar(subField.Code, code))
                        {
                            return subField.Value;
                        }
                    }
                }
            }

            return null;
        }

        // ==========================================================

        public static RecordField[] WithNullValue
            (
                this IEnumerable fields
            )
        {
            return fields
                .Cast<RecordField>()
                .Where(field => string.IsNullOrEmpty(field.Value))
                .ToArray();
        }

        public static RecordField[] WithNullValue
            (
                this MarcRecord record
            )
        {
            FieldCollection fields = record.Fields;
            int count = fields.Length;
            List<RecordField> result = null;
            for (int i = 0; i < count; i++)
            {
                if (string.IsNullOrEmpty(fields[i].Value))
                {
                    if (ReferenceEquals(result, null))
                    {
                        result = new List<RecordField>();
                    }
                    result.Add(fields[i]);
                }
            }

            return ReferenceEquals(result, null)
                ? EmptyFieldArray
                : result.ToArray();
        }

        // ==========================================================

        public static RecordField[] WithoutSubFields
            (
                this IEnumerable fields
            )
        {
            return fields
                .Cast<RecordField>()
                .Where(field => field.SubFields.Length == 0)
                .ToArray();
        }

        public static RecordField[] WithoutSubFields
            (
                this MarcRecord record
            )
        {
            FieldCollection fields = record.Fields;
            int count = fields.Length;
            List<RecordField> result = null;
            for (int i = 0; i < count; i++)
            {
                if (fields[i].SubFields.Length == 0)
                {
                    if (ReferenceEquals(result, null))
                    {
                        result = new List<RecordField>();
                    }
                    result.Add(fields[i]);
                }
            }

            return ReferenceEquals(result, null)
                ? EmptyFieldArray
                : result.ToArray();
        }

        // ==========================================================

        public static RecordField[] WithSubFields
            (
                this IEnumerable fields
            )
        {
            return fields
                .Cast<RecordField>()
                .Where(field => field.SubFields.Length != 0)
                .ToArray();
        }

        public static RecordField[] WithSubFields
            (
                this MarcRecord record
            )
        {
            FieldCollection fields = record.Fields;
            int count = fields.Length;
            List<RecordField> result = null;
            for (int i = 0; i < count; i++)
            {
                if (fields[i].SubFields.Length != 0)
                {
                    if (ReferenceEquals(result, null))
                    {
                        result = new List<RecordField>();
                    }
                    result.Add(fields[i]);
                }
            }

            return ReferenceEquals(result, null)
                ? EmptyFieldArray
                : result.ToArray();
        }
    }
}
