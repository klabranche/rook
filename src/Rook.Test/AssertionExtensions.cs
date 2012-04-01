﻿using System;
using System.Collections.Generic;
using System.Linq;
using Should;

namespace Rook
{
    public static class AssertionExtensions
    {
        public static void ShouldList<T>(this IEnumerable<T> actual, params T[] expected)
        {
            actual.ShouldEqual(expected);
        }

        public static void ShouldThrow<TException>(this Action shouldThrow, string expectedMessage) where TException : Exception
        {
            bool threw = false;

            try
            {
                shouldThrow();
            }
            catch (TException ex)
            {
                ex.Message.ShouldEqual(expectedMessage);
                threw = true;
            }

            threw.ShouldBeTrue(String.Format("Expected {0}.", typeof(TException).Name));
        }

        public static void ShouldThrow<TException>(this Func<object> shouldThrow, string expectedMessage) where TException : Exception
        {
            Action action = () => shouldThrow();
            action.ShouldThrow<TException>(expectedMessage);
        }

        public static void ShouldList<T>(this IEnumerable<T> actual, params Action<T>[] itemExpectations)
        {
            var array = actual.ToArray();

            array.Length.ShouldEqual(itemExpectations.Length);

            for (int i = 0; i < array.Length; i++)
                itemExpectations[i](array[i]);
        }
    }
}