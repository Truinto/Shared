using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;

namespace Shared.CollectionNS
{
    /// <summary>
    /// Extension class for collection operations.
    /// </summary>
    public static class CollectionTool
    {
        #region Search

        /// <summary>
        /// Returns collection count or iterates and counts through all elements.
        /// </summary>
        public static int Count(this IEnumerable enumerable)
        {
            if (enumerable is ICollection coll)
                return coll.Count;

            int count = 0;
            foreach (var item in enumerable)
                count++;
            return count;
        }

        public static bool TryGet<T>(this IEnumerable enumerable, Func<T, bool> pred, [NotNullWhen(true)] out T? result)
        {
            if (enumerable is IList list)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    var item = (T)list[i];
                    if (pred(item))
                    {
                        result = item;
                        return true;
                    }
                }
            }
            foreach (T item in enumerable)
            {
                if (pred(item))
                {
                    result = item;
                    return true;
                }
            }
            result = default;
            return false;
        }

        /// <summary>
        /// Gets the first element that is of type <typeparamref name="T"/>.
        /// </summary>
        public static T? Get<T>(this IEnumerable enumerable)
        {
            if (enumerable == null)
                return default;
            foreach (var item in enumerable)
                if (item is T t)
                    return t;
            return default;
        }

        /// <summary>
        /// Gets all elements that are of type <typeparamref name="T"/>.
        /// </summary>
        public static IEnumerable<T> GetAll<T>(this IEnumerable enumerable)
        {
            if (enumerable == null)
                yield break;
            foreach (var item in enumerable)
                if (item is T t)
                    yield return t;
        }

        public static T? GetFirst<T>(this IList<T> list)
        {
            if (list.Count <= 0)
                return default;
            return list[0];
        }

        public static T? GetLast<T>(this IList<T> list)
        {
            if (list.Count <= 0)
                return default;
            return list[list.Count - 1];
        }

        /// <summary>
        /// Checks if right type can be assigned to left type. <br/>
        /// Works similiar to IsAssignableFrom, but will returns false for non-identical ValueTypes (which need boxing) and void (which overflows the stack).     
        /// </summary>
        public static bool IsTypeCompatible(this Type? left, Type? right)
        {
            if (left == null || right == null)
                return false;
            if (left == right)
                return true;
            if (left.IsValueType || right.IsValueType) // value types must match exactly
                return false;
            return left.IsAssignableFrom(right);
        }

        /// <summary>
        /// Find index in a collection by a predicate.
        /// </summary>
        public static int GetIndex<T>(this IEnumerable enumerable, Func<T, bool> pred)
        {
            int num = 0;
            foreach (T item in enumerable)
            {
                if (pred(item))
                    return num;
                num++;
            }
            return -1;
        }

        /// <summary>
        /// Find index in a collection by a predicate.
        /// </summary>
        public static int GetIndex<T>(this IEnumerable<T> enumerable, Func<T, bool> pred)
        {
            int num = 0;
            foreach (T item in enumerable)
            {
                if (pred(item))
                    return num;
                num++;
            }
            return -1;
        }

        /// <summary>
        /// Find index in a collection by object.Equals.
        /// </summary>
        public static int GetIndex<T>(this IEnumerable<T> enumerable, T element)
        {
            int i = 0;
            foreach (T item in enumerable)
            {
                if (Equals(item, element))
                    return i;
                i++;
            }
            return -1;
        }

        /// <summary>
        /// Find index in a collection by object.Equals. Returns notFound, if no element matches.
        /// </summary>
        public static int GetIndex<T>(this IEnumerable<T> enumerable, T element, int notFound)
        {
            int i = 0;
            foreach (T item in enumerable)
            {
                if (Equals(item, element))
                    return i;
                i++;
            }
            return notFound;
        }

        /// <summary>
        /// Get element at index or default.
        /// </summary>
        public static T? AtIndex<T>(this IEnumerable enumerable, int index)
        {
            int i = 0;
            foreach (var obj in enumerable)
            {
                if (i == index)
                    return (T)obj;
            }
            return default;
        }

        /// <summary>
        /// Get element at index or default.
        /// </summary>
        public static T? AtIndex<T>(this IEnumerable<T> enumerable, int index)
        {
            return enumerable.ElementAtOrDefault(index);
        }

        /// <summary>
        /// Check every element with <see cref="object.ReferenceEquals(object?, object?)"/>. Returns false if <paramref name="element"/> is null.
        /// </summary>
        public static bool ContainsReference(this IEnumerable enumerable, object? element)
        {
            if (element == null)
                return false;
            foreach (var item in enumerable)
                if (ReferenceEquals(item, element))
                    return true;
            return false;
        }

        /// <summary>
        /// Check every element with <see cref="object.Equals(object?)"/>. Returns false if <paramref name="element"/> is null.
        /// </summary>
        public static bool Contains(this IEnumerable enumerable, object? element)
        {
            if (element == null)
                return false;
            foreach (var item in enumerable)
                if (Equals(item, element))
                    return true;
            return false;
        }

        /// <summary>
        /// Check every element with <see cref="object.Equals(object?)"/>. Recursively up to <paramref name="depth"/>. Returns false if <paramref name="element"/> is null.
        /// </summary>
        public static bool ContainsMany(this IEnumerable enumerable, object? element, int depth = 50)
        {
            if (element == null || depth < 0)
                return false;
            foreach (var item in enumerable)
            {
                if (Equals(item, element))
                    return true;
                if (item is IEnumerable enumerable2 && ContainsMany(enumerable2, element, depth - 1))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// <inheritdoc cref="System.Linq.Enumerable.Contains{TSource}(IEnumerable{TSource}, TSource)"/>
        /// </summary>
        public static bool Contains<T>(this IEnumerable<IEnumerable<T>> enumerable, T element)
        {
            foreach (var item in enumerable)
                if (item.Contains(element))
                    return true;
            return false;
        }

        /// <summary>
        /// Search a sequence of elements in <paramref name="array"/>.
        /// </summary>
        public static bool ContainsSequence(this IList array, IList search, int offsetArray = 0, int offsetSearch = 0, int lengthArray = -1, int lengthSearch = -1)
        {
            if (lengthArray < 0)
                lengthArray = array.Count;
            if (lengthSearch < 0)
                lengthSearch = search.Count;
            else if (lengthSearch == 0)
                return true;

            int i = offsetArray;
            int j = offsetSearch;
            while (true)
            {
                if (array.Count >= i || search.Count >= j) // failed if either array out of entities
                    return false;

                if (i - offsetArray >= lengthArray) // failed if array length is shortened
                    return false;

                if (!Equals(array[i++], search[j++])) // restart search if mismatch                
                    j = offsetSearch;

                if (j - offsetSearch >= lengthSearch) // success if enough entities match
                    return true;
            }
        }

        /// <inheritdoc cref="ContainsSequence(IList, IList, int, int, int, int)"/>
        public static bool ContainsSequence(this byte[] array, byte[] search, int offsetArray = 0, int offsetSearch = 0, int lengthArray = -1, int lengthSearch = -1)
        {
            if (lengthArray < 0)
                lengthArray = array.Length;
            if (lengthSearch < 0)
                lengthSearch = search.Length;
            else if (lengthSearch == 0)
                return true;

            int i = offsetArray;
            int j = offsetSearch;
            while (true)
            {
                if (array.Length <= i || search.Length <= j) // failed if either array out of entities
                    return false;

                if (i - offsetArray >= lengthArray) // failed if array length is shortened
                    return false;

                if (array[i++] != search[j++]) // restart search if mismatch                
                    j = offsetSearch;

                if (j - offsetSearch >= lengthSearch) // success if enough entities match
                    return true;
            }
        }

        /// <summary>
        /// Check a text file starts with a specific ASCII string. File can have ASCII or Unicode (UTF-16) encoding.
        /// </summary>
        public static bool StartsWith(this FileInfo filePath, string text, bool throwOnError = false)
        {
            try
            {
                using var stream = new FileStream(filePath.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                stream.Position = 0;
                int bLength = stream.Read(Buffer, 0, Math.Min(Buffer.Length, text.Length * 2 + 2));

                bool unicode = false;
                int ib = 0;
                if (bLength >= 2)
                {
                    if (Buffer[0] == 0xFF && Buffer[1] == 0xFE)
                    {
                        unicode = true; //little-endian
                        ib = 2;
                    }
                    else if (Buffer[1] == 0xFF && Buffer[0] == 0xFE)
                    {
                        unicode = true; //big-endian
                        ib = 3;
                    }
                }

                for (int i = 0; i < text.Length;)
                {
                    if (ib >= bLength)
                        return false;
                    if (text[i++] != Buffer[ib++])
                        return false;
                    if (unicode)
                        ib++;
                }
                return true;
            } catch (Exception)
            {
                if (!throwOnError)
                    return false;
                throw;
            }
        }

        /// <summary>
        /// Filters a sequence and projects each element into a new form.
        /// </summary>
        public static IEnumerable<TResult> SelectWhere<T, TResult>(this IEnumerable<T> enumerable, Func<T, bool> predicate, Func<T, TResult> selector)
        {
            return enumerable.Where(predicate).Select(selector);
        }

        /// <summary>
        /// Projects each element into a new form, but filters null elements and null results.
        /// </summary>
        public static IEnumerable<TResult> SelectNotNull<T, TResult>(this IEnumerable<T> enumerable, Func<T, TResult?> selector) where TResult : class
        {
            foreach (T item in enumerable)
            {
                if (item == null)
                    continue;
                var item2 = selector(item);
                if (item2 != null)
                    yield return item2;
            }
        }

        #endregion

        #region AddUnique

        /// <summary>
        /// Adds items to a list, which are not already in the list. Compares with Equal().
        /// </summary>
        public static void AddUnique<T>(this IList<T> list, IEnumerable<T> values)
        {
            foreach (var value in values)
            {
                if (value is null)
                    continue;
                foreach (var item in list)
                {
                    if (value.Equals(item))
                        goto next;
                }
                list.Add(value);
            next:;
            }
        }

        /// <summary>
        /// Adds items to a list, which are not already in the list. Types can be different. Compares with Equal() of <typeparamref name="T1"/>.
        /// </summary>
        public static void AddUnique<T1, T2>(this IList<T1> list, IEnumerable<T2> values, Func<T2, T1> converter)
        {
            foreach (var value in values)
            {
                if (value is null)
                    continue;
                foreach (var item in list)
                {
                    if (item is not null && item.Equals(value))
                        goto next;
                }
                list.Add(converter(value));
            next:;
            }
        }

        /// <summary>
        /// Adds items to a list, which are not already in the list. Types can be different. Compares with Equal() of <typeparamref name="T2"/>.
        /// </summary>
        public static void AddUnique2<T1, T2>(this IList<T1> list, IEnumerable<T2> values, Func<T2, T1> converter)
        {
            foreach (var value in values)
            {
                if (value is null)
                    continue;
                foreach (var item in list)
                {
                    if (value.Equals(item))
                        goto next;
                }
                list.Add(converter(value));
            next:;
            }
        }

        public static List<T> AddUnique<T>(this List<T> list, T item)
        {
            if (!list.Contains(item))
                list.Add(item);
            return list;
        }

        public static List<T> AddUnique<T>(this List<T> list, Func<T, bool> pred, Func<T> getter)
        {
            if (!list.Any(pred))
                list.Add(getter());
            return list;
        }

        #endregion

        #region As X

        /// <summary>
        /// Returns itself, if value is an array. Otherwise creates a new array.
        /// </summary>
        public static T[] AsArray<T>(this IEnumerable<T> values)
        {
            return values as T[] ?? values.ToArray();
        }

        /// <summary>
        /// Returns itself, if value is a list. Otherwise creates a new list.
        /// </summary>
        public static List<T> AsList<T>(this IEnumerable<T> values)
        {
            return values as List<T> ?? values.ToList();
        }

        #endregion

        #region Insert

        /// <summary>
        /// Adds an item to a enumerable.
        /// </summary>
        public static IEnumerable<T> Concat<T>(this IEnumerable<T> enumerable, T value)
        {
            foreach (var item in enumerable)
                yield return item;
            yield return value;
        }

        /// <summary>
        /// Adds an item to a enumerable.
        /// </summary>
        public static IEnumerable<T> Concat<T>(this T value, IEnumerable<T> enumerable)
        {
            yield return value;
            foreach (var item in enumerable)
                yield return item;
        }

        /// <summary>Injects a new element into an enumerable, if a condition is met.</summary>
        public static IEnumerable<T> InjectBefore<T>(this IEnumerable<T> enumerable, Func<T, bool> pred, Func<T, T> value)
        {
            foreach (var item in enumerable)
            {
                if (pred(item))
                    yield return value(item);
                yield return item;
            }
        }

        /// <summary>Injects a new element into an enumerable, if a condition is met.</summary>
        public static IEnumerable<T> InjectAfter<T>(this IEnumerable<T> enumerable, Func<T, bool> pred, Func<T, T> value)
        {
            foreach (var item in enumerable)
            {
                yield return item;
                if (pred(item))
                    yield return value(item);
            }
        }

        #endregion

        #region Append

        /// <summary>
        /// Adds elements to a collection. Mutates and returns <paramref name="destination"/>.
        /// </summary>
        public static TList Add<T, TList>(this TList destination, IEnumerable<T> source) where TList : ICollection<T>
        {
            if (destination == null)
                throw new ArgumentNullException(nameof(destination));
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (destination is List<T> list)
            {
                list.AddRange(source);
                return destination;
            }

            foreach (var item in source)
                destination.Add(item);

            return destination;
        }

        /// <summary>
        /// Returns numerable with element at the first position.
        /// </summary>
        public static IEnumerable<T> AddBefore<T>(this IEnumerable<T> collection, T element)
        {
            yield return element;
            foreach (var item in collection)
                yield return item;
        }

        /// <summary>
        /// Returns numerable with element at the last position.
        /// </summary>
        public static IEnumerable<T> AddAfter<T>(this IEnumerable<T> collection, T element)
        {
            foreach (var item in collection)
                yield return item;
            yield return element;
        }

        /// <summary>Appends objects on array, returning a new array.</summary>
        public static T[] Append<T>(this T[] orig, params T[] objs)
        {
            orig ??= [];
            objs ??= [];

            int i, j;
            T[] result = new T[orig.Length + objs.Length];
            for (i = 0; i < orig.Length; i++)
                result[i] = orig[i];
            for (j = 0; i < result.Length; i++)
                result[i] = objs[j++];
            return result;
        }

        /// <summary>Appends objects on list, returning a new list.</summary>
        public static List<T> Append<T>(this IList<T> orig, IList<T> objs)
        {
            var result = new List<T>(orig);
            result.AddRange(objs);
            return result;
        }

        /// <summary>Appends objects on array and overwrites the original.</summary>
        public static void AppendAndReplace<T>(ref T[] orig, params T[] objs)
        {
            AppendAndReplace(ref orig, (IEnumerable<T>)objs);
        }

        /// <summary>Appends objects on array and overwrites the original.</summary>
        public static void AppendAndReplace<T>(ref T[] orig, IEnumerable<T> objs)
        {
            orig ??= [];

            T[] result = new T[orig.Length + objs.Count()];
            int i;
            for (i = 0; i < orig.Length; i++)
                result[i] = orig[i];
            foreach (var obj in objs)
                result[i++] = obj;
            orig = result;
        }

        #endregion

        #region Remove

        public static void RemoveAll<T>(ref T[] orig, Func<T, bool> predicate)
        {
            orig = orig.Where(w => !predicate(w)).ToArray();
        }

        public static void InsertAt<T>(ref T[] orig, T obj, int index = -1)
        {
            orig ??= [];
            if (index < 0 || index > orig.Length) index = orig.Length;

            T[] result = new T[orig.Length + 1];
            for (int i = 0, j = 0; i < result.Length; i++)
            {
                if (i == index)
                    result[i] = obj;
                else
                    result[i] = orig[j++];
            }
            orig = result;
        }

        public static void RemoveAt<T>(ref T[] orig, int index)
        {
            orig ??= [];
            if (index < 0 || index >= orig.Length) return;

            T[] result = new T[orig.Length - 1];
            for (int i = 0, j = 0; i < result.Length; i++)
            {
                if (i != index)
                    result[i] = orig[j++];
            }
            orig = result;
        }

        public static void RemoveGet<T>(this List<T> list, List<T> result, Func<T, bool> predicate)
        {
            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (predicate(list[i]))
                {
                    result.Add(list[i]);
                    list.RemoveAt(i);
                    return;
                }
            }
        }

        public static void RemoveGet<T1, T2>(this List<T1> list, List<T2> result, Func<T1, bool> predicate, Func<T1, T2> select)
        {
            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (predicate(list[i]))
                {
                    result.Add(select(list[i]));
                    list.RemoveAt(i);
                    return;
                }
            }
        }

        #endregion Remove

        #region Ensure

        /// <summary>
        /// Get dictionary by key and create new value with standard constructor, if it did not exist.
        /// </summary>
        /// <returns>true if new value was created</returns>
        public static bool Ensure<TKey, TValue>(this Dictionary<TKey, TValue> dic, TKey key, out TValue value) where TValue : new() where TKey : notnull
        {
            if (dic.TryGetValue(key, out value!))
                return false;
            dic[key] = value = new();
            return true;
        }

        public static bool Ensure<TKey, TValue>(this Dictionary<TKey, TValue> dic, TKey key, out TValue value, Func<TValue> getter) where TKey : notnull
        {
            if (dic.TryGetValue(key, out value!))
                return false;
            dic[key] = value = getter();
            return true;
        }

        #endregion

        #region Sort

        /// <summary>
        /// Sorts the elements in a list using the IComparable&lt;<typeparamref name="T"/>&gt; implementation.
        /// </summary>
        public static void QuickSort<T>(IList<T> list, int leftBound, int rightBound) where T : IComparable<T>
        {
            if (rightBound <= leftBound)
                return;

            int i = leftBound;
            int j = rightBound;
            T pivot = list[leftBound];
            while (i <= j)
            {
                while (list[i].CompareTo(pivot) < 0)
                    i++;
                while (list[j].CompareTo(pivot) > 0)
                    j--;
                if (i <= j)
                {
                    T value = list[i];
                    list[i++] = list[j];
                    list[j--] = value;
                }
            }

            if (leftBound < j)
                QuickSort(list, leftBound, j);
            if (i < rightBound)
                QuickSort(list, i, rightBound);
        }

        /// <summary>
        /// Sorts the elements in a list using the IComparable&lt;<typeparamref name="T2"/>&gt; implementation of a specified member.
        /// </summary>
        public static void QuickSort<T1, T2>(IList<T1> list, Func<T1, T2> keySelector, int leftBound, int rightBound) where T2 : IComparable<T2>
        {
            int i = leftBound;
            int j = rightBound;
            T2 pivot = keySelector(list[leftBound]);
            while (i <= j)
            {
                while (keySelector(list[i]).CompareTo(pivot) < 0)
                    i++;
                while (keySelector(list[j]).CompareTo(pivot) > 0)
                    j--;
                if (i <= j)
                {
                    T1 value = list[i];
                    list[i++] = list[j];
                    list[j--] = value;
                }
            }

            if (leftBound < j)
                QuickSort(list, keySelector, leftBound, j);
            if (i < rightBound)
                QuickSort(list, keySelector, i, rightBound);
        }

        /// <summary>
        /// Sorts the elements in a list using a delegate.
        /// </summary>
        public static void QuickSort<T>(IList<T> list, Func<T, T, int> comparer, int leftBound, int rightBound)
        {
            int i = leftBound;
            int j = rightBound;
            T pivot = list[leftBound];
            while (i <= j)
            {
                while (comparer(list[i], pivot) < 0)
                    i++;
                while (comparer(list[j], pivot) > 0)
                    j--;
                if (i <= j)
                {
                    T value = list[i];
                    list[i++] = list[j];
                    list[j--] = value;
                }
            }

            if (leftBound < j)
                QuickSort(list, comparer, leftBound, j);
            if (i < rightBound)
                QuickSort(list, comparer, i, rightBound);
        }

        /// <summary>
        /// Sorts the elements in a list using the IComparable&lt;<typeparamref name="T"/>&gt; implementation.
        /// </summary>
        public static void Sort<T>(this IList<T> collection) where T : IComparable<T>
        {
            if (collection is Array array)
                Array.Sort(array);
            else
                QuickSort(collection, 0, collection.Count - 1);
        }

        /// <summary>
        /// Sorts the elements in a list using the IComparable&lt;<typeparamref name="T2"/>&gt; implementation of a specified member.
        /// </summary>
        public static void Sort<T1, T2>(this IList<T1> collection, Func<T1, T2> keySelector) where T2 : IComparable<T2>
        {
            QuickSort(collection, keySelector, 0, collection.Count - 1);
        }

        /// <summary>
        /// Sorts the elements in a list using a delegate.
        /// </summary>
        public static void Sort<T>(this IList<T> collection, Func<T, T, int> comparer)
        {
            QuickSort(collection, comparer, 0, collection.Count - 1);
        }

        /// <summary>
        /// Sorts the elements in an <see cref="Array"/> using the IComparable&lt;<typeparamref name="T2"/>&gt; implementation of a specified member.
        /// </summary>
        public static void Sort<T1, T2>(this T1[] array, Func<T1, T2> keySelector) where T2 : IComparable<T2>
        {
            Array.Sort(array, (T1 t1, T1 t2) => keySelector(t1).CompareTo(keySelector(t2)));
        }

        /// <summary>
        /// Move item to index, shifting all elements inbetween by one.
        /// </summary>
        public static void MoveTo<T>(this IList<T> array, int index1, int index2, bool throwOnOutOfBounds = false)
        {
            if (array == null) throw new ArgumentNullException(nameof(array));

            if (!throwOnOutOfBounds && (index1 >= array.Count || index2 >= array.Count))
                return;

            bool forward = index1 < index2;
            int index = index1;

            T buff = array[index1];
            while (index != index2)
            {
                if (forward)
                    array[index] = array[++index];
                else
                    array[index] = array[--index];
            }
            array[index] = buff;
        }

        /// <summary>
        /// Swap position of two elements by index.
        /// </summary>
        public static void Swap<T>(this IList<T> array, int index1, int index2, bool throwOnOutOfBounds = false)
        {
            if (array == null) throw new ArgumentNullException(nameof(array));

            if (!throwOnOutOfBounds && (index1 >= array.Count || index2 >= array.Count))
                return;

            (array[index1], array[index2]) = (array[index2], array[index1]);
        }
#if NET5_0_OR_GREATER
        /// <summary>
        /// Swap position of an element with an element at an index.
        /// </summary>
        public static void Swap<T>(this IList<T> array, T obj, Index index2, bool throwOnOutOfBounds = false)
        {
            ArgumentNullException.ThrowIfNull(array);

            int index1 = array.GetIndex(obj);
            if (!throwOnOutOfBounds && index1 < 0)
                return;

            Swap(array, index1, index2.GetOffset(array.Count), throwOnOutOfBounds);
        }

        /// <summary>
        /// Swap position of an element with an element at an index.
        /// </summary>
        public static void Swap<T>(this IList<T> array, Func<T, bool> pred, Index index2, bool throwOnOutOfBounds = false)
        {
            ArgumentNullException.ThrowIfNull(array);
            ArgumentNullException.ThrowIfNull(pred);

            int index1 = array.GetIndex(pred);
            if (!throwOnOutOfBounds && index1 < 0)
                return;

            Swap(array, index1, index2.GetOffset(array.Count), throwOnOutOfBounds);
        }
#endif
        #endregion

        #region Create

        /// <summary>
        /// Fills collection with a value.
        /// </summary>
        public static T[] Fill<T>(this T[] array, T value)
        {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_0_OR_GREATER
            Array.Fill(array, value);
#else
            for (int i = 0; i < array.Length; i++)
                array[i] = value;
#endif
            return array;
        }

        #endregion

        public static readonly byte[] Buffer = new byte[2048];

        private static readonly List<object> _list = [];

        /// <summary>
        /// Gets a static list object. Do not save reference.
        /// Call <b>Flush&lt;T&gt;()</b> to receive output.
        /// </summary>
        public static List<object> GetList()
        {
            System.Threading.Monitor.Enter(_list);
            _list.Clear();
            return _list;
        }

        /// <summary>
        /// Use when finished with <b>GetList()</b>
        /// </summary>
        public static T[] Flush<T>() where T : class
        {
            var result = new T[_list.Count];
            _list.CopyTo(result);
            _list.Clear();
            System.Threading.Monitor.Exit(_list);
            return result;
        }
    }
}
