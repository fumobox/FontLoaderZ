using System;
using System.Collections;
using System.Collections.Generic;

namespace FontLoaderZ.CompactFontFormat
{
    public sealed class CffCharString
    {
        private ByteSegment[] _gsubrs;

        private int _gsubrsBias;

        private ByteSegment[] _lsubrs;

        private int _lsubrsBias;

        private int _normalWidthX;

        private readonly GlyphPath _path;

        private readonly ValueStack _vs;

        private bool _haveWidth;

        private int _nStems;

        private int _width;

        private CffCharString()
        {
            _path = new GlyphPath();
            _vs = new ValueStack(32);
        }

        public static GlyphPath Load(uint gid, FontModel fontModel, byte[] fontSource)
        {
            var cff = new CffCharString();

            if (fontModel.IsCidFont)
            {
                var fdIndex = fontModel.TopDict.FDSelect[gid];
                var fdDict = fontModel.TopDict.FDArray[fdIndex];
                cff._lsubrs = fdDict.Subrs;
                cff._lsubrsBias = cff._lsubrs == null ? 0 : CompactFontFormatTable.GetSubrBias(cff._lsubrs.Length);
                cff._width = fdDict.DefaultWidthX;
                cff._normalWidthX = fdDict.NormalWidthX;
            }
            else
            {
                cff._lsubrs = fontModel.TopDict.Subrs;
                cff._lsubrsBias = cff._lsubrs == null ? 0 : CompactFontFormatTable.GetSubrBias(cff._lsubrs.Length);
                cff._width = fontModel.TopDict.DefaultWidthX;
                cff._normalWidthX = fontModel.TopDict.NormalWidthX;
            }

            cff._gsubrs = fontModel.GSubrs;

            if (cff._gsubrs != null) cff._gsubrsBias = CompactFontFormatTable.GetSubrBias(cff._gsubrs.Length);

            var seg = fontModel.GetCharString(gid);
            cff.Load(seg, fontSource);
            fontModel._glyphs[gid] = new GlyphInfo((ushort) cff._width, fontModel._glyphs[gid].LeftSideBearing);

            return cff._path;
        }

        private void Load(ByteSegment segment, byte[] fontSource)
        {
            var i = 0;
            while (i < segment.Count)
            {
                var b0 = fontSource[segment.Offset + i];
                i++;

                if (b0 == 12)
                {
                    var b1 = fontSource[segment.Offset + i++];

                    switch (b1)
                    {
                        case 34:
                        {
                            // |- dx1 dx2 dy2 dx3 dx4 dx5 dx6 hflex (12 34) |-
                            _path.Add(_vs.Shift(), 0);
                            _path.Add(_vs.Shift(), _vs.Shift());
                            _path.Add(_vs.Shift(), 0);
                            _path.Add(_vs.Shift(), 0);
                            _path.Add(_vs.Shift(), 0);
                            _path.Add(_vs.Shift(), 0);
                            break;
                        }
                        case 35:
                        {
                            // |- dx1 dy1 dx2 dy2 dx3 dy3 dx4 dy4 dx5 dy5 dx6 dy6 fd flex (12 35) |-
                            _path.Add(_vs.Shift(), _vs.Shift());
                            _path.Add(_vs.Shift(), _vs.Shift());
                            _path.Add(_vs.Shift(), _vs.Shift());
                            _path.Add(_vs.Shift(), _vs.Shift());
                            _path.Add(_vs.Shift(), _vs.Shift());
                            _path.Add(_vs.Shift(), _vs.Shift());
                            break;
                        }
                        case 36:
                        {
                            // |- dx1 dy1 dx2 dy2 dx3 dx4 dx5 dy5 dx6 hflex1 (12 36) |-
                            _path.Add(_vs.Shift(), _vs.Shift());
                            _path.Add(_vs.Shift(), _vs.Shift());
                            _path.Add(_vs.Shift(), 0);
                            _path.Add(_vs.Shift(), 0);
                            _path.Add(_vs.Shift(), _vs.Shift());
                            _path.Add(_vs.Shift(), 0);
                            break;
                        }
                        case 37:
                        {
                            // |- dx1 dy1 dx2 dy2 dx3 dy3 dx4 dy4 dx5 dy5 dx6 flex1 (12 37) |-
                            _path.Add(_vs.Shift(), _vs.Shift());
                            _path.Add(_vs.Shift(), _vs.Shift());
                            _path.Add(_vs.Shift(), _vs.Shift());
                            _path.Add(_vs.Shift(), _vs.Shift());
                            _path.Add(_vs.Shift(), _vs.Shift());
                            _path.Add(_vs.Shift(), 0);
                            break;
                        }
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                else if (b0 <= 27 || 29 <= b0 && b0 <= 31)
                {
                    switch (b0)
                    {
                        case 1:
                            // |- y dy {dya dyb}* hstem (1) |-
                            LoadStems();
                            break;
                        case 3:
                            // |- x dx {dxa dxb}* vstem (3) |-
                            LoadStems();
                            break;
                        case 4:
                            // |- dy1 vmoveto (4) |-
                            if (_vs.Count > 1 && !_haveWidth)
                            {
                                _width = (int)_vs.Shift() + _normalWidthX;
                                _haveWidth = true;
                            }

                            _path.NewContour(0, _vs.Shift());
                            //_lastPoint.y += _vs.Pop();
                            //NewContour(_lastPoint);
                            break;
                        case 5:
                        {
                            // |- {dxa dya}+ rlineto (5) |-
                            while (_vs.Count > 0) _path.Add(_vs.Shift(), _vs.Shift());

                            break;
                        }
                        case 6:
                        {
                            // |- dx1 {dya dxb}* hlineto (6) |-
                            // |- {dxa dyb}+ hlineto (6) |-
                            while (_vs.Count > 0)
                            {
                                _path.Add(_vs.Shift(), 0);
                                if (_vs.Count == 0) break;

                                _path.Add(0, _vs.Shift());
                            }

                            break;
                        }
                        case 7:
                        {
                            // |- dy1 {dxa dyb}* vlineto (7) |-
                            // |- {dya dxb}+ vlineto (7) |-
                            while (_vs.Count > 0)
                            {
                                _path.Add(0, _vs.Shift());
                                if (_vs.Count == 0) break;

                                _path.Add(_vs.Shift(), 0);
                            }

                            break;
                        }
                        case 8:
                        {
                            // |- {dxa dya dxb dyb dxc dyc}+ rrcurveto (8) |-
                            while (_vs.Count > 0)
                            {
                                _path.Add(_vs.Shift(), _vs.Shift(), true);
                                _path.Add(_vs.Shift(), _vs.Shift(), true);
                                _path.Add(_vs.Shift(), _vs.Shift());
                            }

                            break;
                        }
                        case 10:
                            // callsubr
                            var lIndex = (int) _vs.Pop() + _lsubrsBias;
                            Load(_lsubrs[lIndex], fontSource);
                            break;
                        case 11:
                            // return
                            break;
                        case 12:
                            // escape
                            break;
                        case 14:
                            // End of charstring
                            if (_vs.Count > 0 && !_haveWidth)
                            {
                                _width = (int)_vs.Shift() + _normalWidthX;
                                _haveWidth = true;
                            }

                            break;
                        case 18:
                            LoadStems();
                            break;
                        case 19:
                        case 20:
                            // hintmask, cntrmask
                            LoadStems();
                            i += (_nStems + 7) >> 3;
                            break;
                        case 21:
                            // |- dx1 dy1 rmoveto (21) |-
                            if (_vs.Count > 2 && !_haveWidth)
                            {
                                _width = (int)_vs.Shift() + _normalWidthX;
                                _haveWidth = true;
                            }

                            _path.NewContour(_vs.Shift(), _vs.Shift());
                            break;
                        case 22:
                            // |- dx1 hmoveto (22) |-
                            if (_vs.Count > 1 && !_haveWidth)
                            {
                                _width = (int)_vs.Shift() + _normalWidthX;
                                _haveWidth = true;
                            }

                            _path.NewContour(_vs.Shift(), 0);
                            break;
                        case 23:
                            LoadStems();
                            break;
                        case 24:
                        {
                            // |- {dxa dya dxb dyb dxc dyc}+ dxd dyd rcurveline (24) |-

                            // Curve
                            while (_vs.Count > 4)
                            {
                                _path.Add(_vs.Shift(), _vs.Shift(), true);
                                _path.Add(_vs.Shift(), _vs.Shift(), true);
                                _path.Add(_vs.Shift(), _vs.Shift());
                            }

                            // Line
                            _path.Add(_vs.Shift(), _vs.Shift());
                            break;
                        }
                        case 25:
                        {
                            // |- {dxa dya}+ dxb dyb dxc dyc dxd dyd rlinecurve (25) |-

                            // Lines
                            while (_vs.Count > 6) _path.Add(_vs.Shift(), _vs.Shift());

                            // Curves
                            _path.Add(_vs.Shift(), _vs.Shift(), true);
                            _path.Add(_vs.Shift(), _vs.Shift(), true);
                            _path.Add(_vs.Shift(), _vs.Shift());
                            break;
                        }
                        case 26:
                        {
                            // |- dx1? {dya dxb dyb dyc}+ vvcurveto (26) |-
                            var dx = 0f;
                            if (_vs.Count % 2 == 1) dx = _vs.Shift();
                            while (_vs.Count > 0)
                            {
                                _path.Add(dx, _vs.Shift(), true);
                                _path.Add(_vs.Shift(), _vs.Shift(), true);
                                _path.Add(0, _vs.Shift());
                                dx = 0;
                            }

                            break;
                        }
                        case 27:
                        {
                            // |- dy1? {dxa dxb dyb dxc}+ hhcurveto (27) |-
                            var dy1 = 0f;
                            if (_vs.Count % 2 == 1) dy1 = _vs.Shift();
                            while (_vs.Count != 0)
                            {
                                _path.Add(_vs.Shift(), dy1, true);
                                _path.Add(_vs.Shift(), _vs.Shift(), true);
                                _path.Add(_vs.Shift(), 0);
                                dy1 = 0;
                            }

                            break;
                        }
                        case 28:
                            break;
                        case 29:
                            // callgsubr
                            var gIndex = (int) _vs.Pop() + _gsubrsBias;
                            Load(_gsubrs[gIndex], fontSource);
                            break;
                        case 30:
                        {
                            // |- dy1 dx2 dy2 dx3 {dxa dxb dyb dyc dyd dxe dye dxf}* dyf? vhcurveto (30) |-
                            // |- {dya dxb dyb dxc dxd dxe dye dyf}+ dxf? vhcurveto (30) |-
                            var even = true;
                            while (_vs.Count >= 4)
                            {
                                if (even)
                                {
                                    _path.Add(0, _vs.Shift(), true);
                                    _path.Add(_vs.Shift(), _vs.Shift(), true);
                                    _path.Add(_vs.Shift(), 0);
                                }
                                else
                                {
                                    _path.Add(_vs.Shift(), 0, true);
                                    _path.Add(_vs.Shift(), _vs.Shift(), true);
                                    _path.Add(0, _vs.Shift());
                                }

                                even = !even;
                            }

                            if (_vs.Count == 1)
                                _path.ReplaceLastPoint(
                                    _path.LastPoint.x + (even ? _vs.Shift() : 0),
                                    _path.LastPoint.y + (even ? 0 : _vs.Shift())
                                );

                            break;
                        }
                        case 31:
                        {
                            // |- dx1 dx2 dy2 dy3 {dya dxb dyb dxc dxd dxe dye dyf}* dxf? hvcurveto (31) |-
                            // |- {dxa dxb dyb dyc dyd dxe dye dxf}+ dyf? hvcurveto (31) |-
                            var even = true;
                            while (_vs.Count >= 4)
                            {
                                if (even)
                                {
                                    _path.Add(_vs.Shift(), 0, true);
                                    _path.Add(_vs.Shift(), _vs.Shift(), true);
                                    _path.Add(0, _vs.Shift());
                                }
                                else
                                {
                                    _path.Add(0, _vs.Shift(), true);
                                    _path.Add(_vs.Shift(), _vs.Shift(), true);
                                    _path.Add(_vs.Shift(), 0);
                                }

                                even = !even;
                            }

                            if (_vs.Count == 1)
                                _path.ReplaceLastPoint(
                                    _path.LastPoint.x + (even ? 0 : _vs.Shift()),
                                    _path.LastPoint.y + (even ? _vs.Shift() : 0)
                                );

                            break;
                        }
                        default:
                            throw new ArgumentOutOfRangeException($"Invalid operator: {b0}");
                    }
                }
                // Operands
                else if (32 <= b0 && b0 <= 246)
                {
                    var val = b0 - 139;
                    _vs.Push(val);
                }
                else if (247 <= b0 && b0 <= 250)
                {
                    var b1 = fontSource[segment.Offset + i++];
                    var val = (b0 - 247) * 256 + b1 + 108;
                    _vs.Push(val);
                }
                else if (251 <= b0 && b0 <= 254)
                {
                    var b1 = fontSource[segment.Offset + i++];
                    var val = -(b0 - 251) * 256 - b1 - 108;
                    _vs.Push(val);
                }
                else if (b0 == 28)
                {
                    var b1 = fontSource[segment.Offset + i++];
                    var b2 = fontSource[segment.Offset + i++];
                    var val = CffDictParser.TwosComp((b1 << 8) | b2, 16);
                    _vs.Push(val);
                }
                else if (b0 == 255)
                {
                    var b1 = fontSource[segment.Offset + i++];
                    var b2 = fontSource[segment.Offset + i++];
                    var b3 = fontSource[segment.Offset + i++];
                    var b4 = fontSource[segment.Offset + i++];
                    var val = CffDictParser.TwosComp((b1 << 24) | (b2 << 16) | (b3 << 8) | b4, 32) /
                              (float) Math.Pow(2, 16);
                    _vs.Push(val);
                }
                else
                {
                    throw new FontException(string.Format("Invalid data: {0}(0x{0:X2})", b0));
                }
            }
        }

        private void LoadStems()
        {
            var hasWidthArg = _vs.Count % 2 != 0;
            if (hasWidthArg && !_haveWidth) _width = (int)_vs.Shift() + _normalWidthX;

            _nStems += _vs.Count >> 1;
            _vs.Clear();
            _haveWidth = true;
        }

        private sealed class ValueStack : IEnumerable<float>
        {
            private readonly int _baseSize;
            private float[] _values;

            public ValueStack(int size)
            {
                _baseSize = size;
                _values = new float[size];
            }

            public int Count { get; private set; }

            public IEnumerator<float> GetEnumerator()
            {
                for (var i = 0; i < Count; i++) yield return _values[i];
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            private void ExpandArray()
            {
                var arr = new float[_values.Length + _baseSize];
                for (var i = 0; i < _values.Length; i++) arr[i] = _values[i];

                _values = arr;
            }

            public void Clear()
            {
                Count = 0;
            }

            public void Push(float value)
            {
                _values[Count++] = value;
                if (Count == _values.Length) ExpandArray();
            }

            public float Shift()
            {
                if (Count == 0) throw new IndexOutOfRangeException();
                var v = _values[0];
                for (var i = 0; i < Count - 1; i++) _values[i] = _values[i + 1];

                Count--;
                return v;
            }

            public float Pop()
            {
                if (Count == 0) throw new IndexOutOfRangeException();
                return _values[--Count];
            }
        }
    }
}