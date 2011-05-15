﻿using System;
using NUnit.Framework;

namespace Rook.Core.Collections
{
    [TestFixture]
    public class SliceVectorSpec : VectorSpec
    {
        [Test]
        public void ShouldProvideItemCount()
        {
            SliceDigits(0, 0).Count.ShouldEqual(0);
            SliceDigits(0, 5).Count.ShouldEqual(5);
            SliceDigits(7, 10).Count.ShouldEqual(3);
        }

        [Test]
        public void ShouldBeEnumerable()
        {
            Vector<int> empty = SliceDigits(0, 0);
            Vector<int> nonempty = SliceDigits(0, 10);

            empty.ShouldBeEmpty();
            nonempty.ShouldList(0, 1, 2, 3, 4, 5, 6, 7, 8, 9);
        }

        [Test]
        public void ShouldSliceFromStartIndexInclusiveToEndIndexExclusive()
        {
            SliceDigits(0, 0).ShouldBeEmpty();
            SliceDigits(5, 5).ShouldBeEmpty();
            SliceDigits(9, 9).ShouldBeEmpty();
            
            SliceDigits(0, 1).ShouldList(0);
            SliceDigits(0, 2).ShouldList(0, 1);
            SliceDigits(0, 9).ShouldList(0, 1, 2, 3, 4, 5, 6, 7, 8);
            SliceDigits(0, 10).ShouldList(0, 1, 2, 3, 4, 5, 6, 7, 8, 9);

            SliceDigits(5, 6).ShouldList(5);
            SliceDigits(5, 7).ShouldList(5, 6);
            SliceDigits(5, 9).ShouldList(5, 6, 7, 8);
            SliceDigits(5, 10).ShouldList(5, 6, 7, 8, 9);
        }

        [Test]
        public void ShouldGetItemsByIndex()
        {
            Vector<int> start = SliceDigits(0, 2);
            start[0].ShouldEqual(0);
            start[1].ShouldEqual(1);

            Vector<int> end = SliceDigits(5, 10);
            end[0].ShouldEqual(5);
            end[1].ShouldEqual(6);
            end[2].ShouldEqual(7);
            end[3].ShouldEqual(8);
            end[4].ShouldEqual(9);
        }

        [Test]
        public void ShouldCreateNewVectorWithNewValueAppended()
        {
            Vector<int> interiorSlice = SliceDigits(1, 4);
            Vector<int> tailSlice = SliceDigits(7, 10);

            interiorSlice.ShouldList(1, 2, 3);
            tailSlice.ShouldList(7, 8, 9);

            interiorSlice.Append(-1).ShouldList(1, 2, 3, -1);
            tailSlice.Append(-1).ShouldList(7, 8, 9, -1);

            interiorSlice.ShouldList(1, 2, 3);
            tailSlice.ShouldList(7, 8, 9);
        }

        [Test]
        public void ShouldCreateNewVectorWithAlteredCell()
        {
            Vector<int> interiorSlice = SliceDigits(1, 4);
            Vector<int> tailSlice = SliceDigits(7, 10);

            interiorSlice.ShouldList(1, 2, 3);
            tailSlice.ShouldList(7, 8, 9);

            interiorSlice.With(0, 0).ShouldList(0, 2, 3);
            interiorSlice.With(1, 0).ShouldList(1, 0, 3);
            interiorSlice.With(2, 0).ShouldList(1, 2, 0);

            tailSlice.With(0, 0).ShouldList(0, 8, 9);
            tailSlice.With(1, 0).ShouldList(7, 0, 9);
            tailSlice.With(2, 0).ShouldList(7, 8, 0);

            interiorSlice.ShouldList(1, 2, 3);
            tailSlice.ShouldList(7, 8, 9);
        }

        [Test]
        public void ShouldCreateSlices()
        {
            Vector<int> sliceOfInteriorSlice = SliceDigits(2, 9).Slice(1, 4);
            Vector<int> sliceOfTailSlice = SliceDigits(3, 10).Slice(1, 4);

            sliceOfInteriorSlice.ShouldList(3, 4, 5);
            sliceOfTailSlice.ShouldList(4, 5, 6);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException), ExpectedMessage = "endIndexExclusive must be greater than or equal to startIndexInclusive.")]
        public void ShouldDemandUpperLimitCannotBeSmallerThanLowerLimit()
        {
            SliceDigits(1, 0);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException), ExpectedMessage = "endIndexExclusive must be less than or equal to the source vector's Count.")]
        public void ShouldDemandUpperLimitCannotExceedSourceVectorCount()
        {
            SliceDigits(0, 11);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException), ExpectedMessage = "startIndexInclusive cannot be negative.")]
        public void ShouldDemandLowerLimitCannotBeNegative()
        {
            SliceDigits(-1, 0);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException), ExpectedMessage = "startIndexInclusive must be a valid index for the source vector.")]
        public void ShouldDemandLowerLimitMustBeValidForSoureVector()
        {
            SliceDigits(10, 10);
        }

        [Test]
        public void ShouldAllowTakingAnEmptySliceOfAnEmptySourceVector()
        {
            //This is a test of the internal SliceVector constructor,
            //indirectly since we cannot construct it directly.

            Vector<int> empty = new ArrayVector<int>();

            empty.Slice(0, 0).ShouldBeEmpty();
        }

        [Test]
        public void ShouldThrowExceptionWhenGivenIndexIsOutOfRange()
        {
            Vector<int> fiveToNine = SliceDigits(5, 10);

            AssertIndexOutOfRange(() => fiveToNine.With(5, 5));
            AssertIndexOutOfRange(() => fiveToNine.With(-1, -1));

            AssertIndexOutOfRange(() => { int value = fiveToNine[5]; });
            AssertIndexOutOfRange(() => { int value = fiveToNine[-1]; });
        }

        private static Vector<int> SliceDigits(int startIndexInclusive, int endIndexExclusive)
        {
            var digits = new ArrayVector<int>(0, 1, 2, 3, 4, 5, 6, 7, 8, 9);
            return digits.Slice(startIndexInclusive, endIndexExclusive);
        }
    }
}