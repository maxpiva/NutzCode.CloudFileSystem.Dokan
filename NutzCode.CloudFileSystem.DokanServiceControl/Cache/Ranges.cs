using System.Collections.Generic;
using System.Linq;

namespace NutzCode.CloudFileSystem.DokanServiceControl.Cache
{
    public class Ranges : List<Range>
    {
        private Range FindFirstRange(long start, long end)
        {
            for (int x = 0; x < Count; x++)
            {
                if ((this[x].StartPosition >= start) && (this[x].StartPosition < end))
                    return this[x];
            }
            return null;
        }

        public bool PositionBelongsToCache(long position)
        {
            return FindFirstRange(position, position) != null;
        }

        public Range FirstBlock(long startPos, long size)
        {
            long endPos = startPos + size;
            Range r = FindFirstRange(startPos, endPos);
            if (r == null)
            {
                Range tr = new Range();
                tr.StartPosition = startPos;
                tr.EndPosition = endPos;
                tr.Found = false;
                return tr;
            }
            if (r.StartPosition > startPos)
            {
                Range tr = new Range();
                tr.StartPosition = startPos;
                tr.EndPosition = r.StartPosition;
                tr.Found = false;
                return tr;
            }
            if (r.EndPosition >= endPos)
            {
                Range tr = new Range();
                tr.StartPosition = r.StartPosition;
                tr.EndPosition = endPos;
                tr.Found = true;
                return tr;
            }
            Range tr2 = new Range();
            tr2.StartPosition = r.StartPosition;
            tr2.EndPosition = r.EndPosition;
            tr2.Found = true;
            return tr2;
        }

        public List<Range> InCache(long startPos, long size)
        {
            List<Range> trs = new List<Range>();
            long endPos = startPos + size;
            Range r = FindFirstRange(startPos, endPos);
            do
            {
                if (r == null)
                {
                    Range tr = new Range();
                    tr.StartPosition = startPos;
                    tr.EndPosition = endPos;
                    tr.Found = false;
                    trs.Add(tr);
                    return trs;
                }
                if (r.StartPosition > startPos)
                {
                    Range tr = new Range();
                    tr.StartPosition = startPos;
                    tr.EndPosition = r.StartPosition;
                    tr.Found = false;
                    trs.Add(tr);
                }
                if (r.EndPosition >= endPos)
                {
                    Range tr = new Range();
                    tr.StartPosition = r.StartPosition;
                    tr.EndPosition = endPos;
                    tr.Found = true;
                    trs.Add(tr);
                    return trs;
                }
                Range tr2 = new Range();
                tr2.StartPosition = r.StartPosition;
                tr2.EndPosition = r.EndPosition;
                tr2.Found = true;
                trs.Add(tr2);
                startPos = r.EndPosition;
                r = FindFirstRange(startPos, endPos);
            } while (true);

        }
        public void RemoveSize(long start, long size)
        {
            long EndPos = start + size;
            int x = 0;
            while (x < Count)
            {
                if ((EndPos >= this[x].StartPosition) && (EndPos <= this[x].EndPosition))
                {
                    this[x].StartPosition = EndPos;
                    return;
                }
                if ((start >= this[x].StartPosition) && (start <= this[x].EndPosition))
                {
                    if (EndPos < this[x].EndPosition)
                    {
                        Range r = new Range();
                        r.StartPosition = EndPos;
                        r.EndPosition = this[x].EndPosition;
                        this[x].EndPosition = start;
                        if (x + 1 < Count)
                            Insert(x + 1, r);
                        else
                            Add(r);
                        return;
                    }
                    int y = x + 1;
                    while (y < Count)
                    {
                        if (this[y].StartPosition <= EndPos)
                        {
                            if (this[y].EndPosition <= EndPos)
                                Remove(this[y]);
                            else
                            {
                                this[x].StartPosition = EndPos;
                                break;
                            }
                        }
                        break;
                    }
                    this[x].EndPosition = start;
                    return;
                }
                x++;
            }
        }
        public void AddSize(long start, long size)
        {
            long EndPos = start + size;
            int x = 0;
            while (x < Count)
            {
                if ((EndPos >= this[x].StartPosition) && (EndPos <= this[x].EndPosition))
                {
                    if (start < this[x].StartPosition)
                    {
                        this[x].StartPosition = start;

                    }
                    return;
                }
                if ((start >= this[x].StartPosition) && (start <= this[x].EndPosition))
                {
                    int y = x + 1;
                    while (y < Count)
                    {
                        if (this[y].StartPosition == EndPos)
                        {
                            EndPos = this[y].EndPosition;
                            Remove(this[y]);
                        }
                        else if (this[y].StartPosition < EndPos)
                        {
                            if (EndPos < this[y].EndPosition)
                                EndPos = this[y].EndPosition;
                            Remove(this[y]);
                        }
                        else
                            break;
                    }
                    this[x].EndPosition = EndPos;
                    return;
                }
                x++;
            }
            Range r = new Range();
            r.StartPosition = start;
            r.EndPosition = EndPos;
            bool found = false;
            foreach (Range r2 in this)
            {
                if (EndPos < r.StartPosition)
                {
                    Insert(IndexOf(r2), r);
                    found = true;
                    break;
                }
            }
            if (!found)
                Add(r);
        }
    }
    public class Range
    {
        public long StartPosition { get; set; }
        public long EndPosition { get; set; }
        public bool Found { get; set; }
    }
}
