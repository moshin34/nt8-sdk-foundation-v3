using System;
using System.Collections.Generic;

namespace NT8.SDK.Common
{
    /// <summary>Minimal SMA crossover signal (portable, no external deps).</summary>
    public sealed class SmaCrossSignal
    {
        private readonly int _fast, _slow;
        private readonly Queue<double> _fq = new Queue<double>(), _sq = new Queue<double>();
        private double _fs, _ss;
        private bool _has, _long, _short;

        public SmaCrossSignal(int fast, int slow)
        {
            if (fast <= 0) throw new ArgumentOutOfRangeException("fast");
            if (slow <= 0) throw new ArgumentOutOfRangeException("slow");
            if (fast >= slow) throw new ArgumentException("fast must be < slow");
            _fast = fast; _slow = slow;
        }

        public void OnPriceTick(DateTime time, double price)
        {
            _fq.Enqueue(price); _fs += price; if (_fq.Count > _fast) _fs -= _fq.Dequeue();
            _sq.Enqueue(price); _ss += price; if (_sq.Count > _slow) _ss -= _sq.Dequeue();
            _has = _sq.Count == _slow;
            if (_has)
            {
                double f = _fs / _fq.Count, s = _ss / _sq.Count;
                _long = f > s; _short = f < s;
            }
        }

        public bool IsLong { get { return _has && _long; } }
        public bool IsShort { get { return _has && _short; } }
        public bool HasValue { get { return _has; } }
    }
}