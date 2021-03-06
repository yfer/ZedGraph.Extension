﻿using System;
using ZedGraph;

namespace Yfer.ZedGraph.Extension
{
    [Serializable]
    public class FilteredPointPairList<T> : IPointList
        where T : struct, IComparable, IComparable<T>, IConvertible, IEquatable<T>, IFormattable //Make sure that T is only numeric Types
    {

        #region Fields

        /// <summary>
        /// Instance of an array of x values
        /// </summary>
        private readonly T[] _y;

        private readonly double _xfreq;
        private readonly double _yfreq;

        /// <summary>
        /// This is the maximum number of points that you want to see in the filtered dataset
        /// </summary>
        private int _maxPts = 2000;

        /// <summary>
        /// The index of the xMinBound above
        /// </summary>
        private int _minxBoundIndex = -1;

        /// <summary>
        /// The index of the xMaxBound above
        /// </summary>
        private int _maxxBoundIndex = -1;

        #endregion

        #region Properties

        /// <summary>
        /// Indexer to access the specified <see cref="PointPair"/> object by
        /// its ordinal position in the list.
        /// </summary>
        /// <remarks>
        /// Returns <see cref="PointPairBase.Missing" /> for any value of <see paramref="index" />
        /// that is outside of its corresponding array bounds.
        /// </remarks>
        /// <param name="index">The ordinal position (zero-based) of the
        /// <see cref="PointPair"/> object to be accessed.</param>
        /// <value>A <see cref="PointPair"/> object reference.</value>
        public PointPair this[int index]
        {
            get { return _points[index]; }          
        }

        private PointPair[] _points;

        private void FilterPoints()
        {
            _points = new PointPair[Count];
            
            //points in interval
            var coef = ((double) _maxxBoundIndex - _minxBoundIndex)/Count;
            var start = 0;
            for (var i = 0; i < Count; i++)
            {   
                var length = (int)Math.Round(coef*(i+1)) - start;

                var segment = new ArraySegment<T>(_y, _minxBoundIndex + start, length);
                start += length;

                var min = _y[segment.Offset];
                var max = _y[segment.Offset];
                for (var j = segment.Offset; j < segment.Count; j++)
                {
                    if (min.CompareTo(_y[j]) > 0)
                        min = _y[j];
                    if (max.CompareTo(_y[j]) < 0)
                        max = _y[j];
                }
                _points[i] = new PointPair(
                    (_minxBoundIndex + i * coef) / _xfreq,
                    Convert.ToDouble(max) / _yfreq,
                    Convert.ToDouble(min) / _yfreq);
            }
        }

        
        /// <summary>
        /// Returns the number of points according to the current state of the filter.
        /// </summary>
        public int Count { get; private set; }

        /// <summary>
        /// Gets the desired number of filtered points to output.  You can set this value by
        /// calling <see cref="SetBounds" />.
        /// </summary>
        public int MaxPts
        {
            get { return _maxPts; }
        }

        #endregion

        #region Constructors
        
        /// <summary>
        /// Constructor to initialize the PointPairList from two arrays of
        /// type double.
        /// </summary>
        public FilteredPointPairList(T[] y, double xfreq, double yfreq)
        {
            _y = y;
            _xfreq = xfreq;
            _yfreq = yfreq;
        }

        /// <summary>
        /// The Copy Constructor
        /// </summary>
        /// <param name="rhs">The FilteredPointList from which to copy</param>
        public FilteredPointPairList(FilteredPointPairList<T> rhs)
        {
            _y = (T[]) rhs._y.Clone();
            _minxBoundIndex = rhs._minxBoundIndex;
            _maxxBoundIndex = rhs._maxxBoundIndex;
            _maxPts = rhs._maxPts;

        }

        /// <summary>
        /// Deep-copy clone routine
        /// </summary>
        /// <returns>A new, independent copy of the FilteredPointList</returns>
        public virtual object Clone()
        {
            return new FilteredPointPairList<T>(this);
        }


        #endregion

        #region Methods

        /// <summary>
        /// Set the data bounds to the specified minimum, maximum, and point count.  Use values of
        /// min=double.MinValue and max=double.MaxValue to get the full range of data. Call this method anytime the zoom range is changed.
        /// </summary>
        /// <param name="minx">The lower bound for the X data of interest</param>
        /// <param name="maxx">The upper bound for the X data of interest</param>
        /// <param name="maxPts">The maximum number of points allowed to be output by the filter</param>
        public void SetBounds(double minx, double maxx, int maxPts)
        {
            _maxPts = maxPts;

            // find the index of the start and end of the bounded range
            var first = (int) Math.Floor(_xfreq*minx);
            var last = (int) Math.Ceiling(_xfreq*maxx);
            if (first < 0) first = 0;
            if (first > _y.Length) first = -1;
            if (last > _y.Length) last = _y.Length;
            if (last < 0) last = -1;

            _minxBoundIndex = first;
            _maxxBoundIndex = last;

            //compute point count to display
            Count = 0;
            if (_minxBoundIndex >= 0 && _maxxBoundIndex >= 0)
            {
                Count = _maxxBoundIndex - _minxBoundIndex;
                if (Count > _maxPts)
                    Count = MaxPts;
            }

            FilterPoints();
        }

        #endregion

    }
}