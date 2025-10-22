using System;
using System.Collections.Generic;

public static class Heap
{
    public static Heap<I, P> MinHeap<I, P>() where P : IComparable<P>
    {
        int _compare(P first, P second)
        {
            return first.CompareTo(second);
        }
        return new Heap<I, P>(_compare);
    }

    public static Heap<I, P> MinHeap<I, P>(Func<P, P, int> compare)
    {
        return new Heap<I, P>(compare);
    }

    public static Heap<I, P> MaxHeap<I, P>() where P : IComparable<P>
    {
        int _compare(P first, P second)
        {
            return -first.CompareTo(second);
        }
        return new Heap<I, P>(_compare);
    }

    public static Heap<I, P> MaxHeap<I, P>(Func<P, P, int> compare)
    {
        int _compare(P first, P second)
        {
            return -compare(first, second);
        }
        return new Heap<I, P>(_compare);
    }
}

public class Heap<I, P>
{
    List<(I Item, P Priority)> _nodes = new List<(I Item, P Priority)>();
    Func<P, P, int> _compare;

    public Heap(Func<P, P, int> compare)
    {
        _compare = compare;
    }

    public void Push(I item, P priority)
    {
        _nodes.Add((item, priority));
        var idx = _nodes.Count - 1;
        while (true)
        {
            if (Parent(idx) is int parentIdx)
            {
                var parent = _nodes[parentIdx].Priority;
                if (_compare(parent, priority) <= 0)
                    break;
                (_nodes[parentIdx], _nodes[idx]) = (_nodes[idx], _nodes[parentIdx]);
                idx = parentIdx;
            }
            else break;
        }
    }

    public (I Item, P Priority)? Pop()
    {
        // Return early if the heap is empty.
        if (_nodes.Count == 0)
            return null;
        // The heap isn't empty. Find the tail, swap it with the head, remove tail, and re-heapify.
        var tailIdx = _nodes.Count - 1;
        (_nodes[0], _nodes[tailIdx]) = (_nodes[tailIdx], _nodes[0]);

        var popped = _nodes[tailIdx];
        _nodes.RemoveAt(tailIdx);

        if (1 <= _nodes.Count)
            Heapify(0);
        return popped;
    }

    public (I Item, P Priority)? Peek()
    {
        if (_nodes.Count > 0)
            return _nodes[0];
        return null;
    }

    public bool Find(I item)
    {
        var idx = _nodes.FindIndex((node) => node.Item.Equals(item));
        if (idx < 0)
            return false;
        return true;
    }

    public (I Item, P Priority)? Remove(I item)
    {
        var idx = _nodes.FindIndex((node) => node.Item.Equals(item));
        if (idx < 0)
            return null;

        // At least the heap isn't empty. Fin the tail, swap it with our target, remove the tail, and re-heapify.
        var tailIdx = _nodes.Count - 1;
        (_nodes[idx], _nodes[tailIdx]) = (_nodes[tailIdx], _nodes[idx]);
        var removed = _nodes[tailIdx];
        _nodes.RemoveAt(tailIdx);

        if (idx <= _nodes.Count - 1)
            Heapify(idx);
        return removed;
    }

    int? Parent(int idx)
    {
        if (idx == 0)
            return null;
        return (idx - 1) / 2;
    }

    int? Left(int idx)
    {
        var left = 2 * idx + 1;
        if (left > _nodes.Count - 1)
            return null;
        return left;
    }

    int? Right(int idx)
    {
        var right = 2 * idx + 2;
        if (right > _nodes.Count - 1)
            return null;
        return right;
    }

    void Heapify(int idx)
    {
        var minPriority = _nodes[idx].Priority;
        int minIdx = idx;
        if (Left(idx) is int leftIdx)
        {
            var left = _nodes[leftIdx].Priority;
            if (_compare(left, minPriority) < 0)
            {
                minPriority = left;
                minIdx = leftIdx;
            }
        }
        if (Right(idx) is int rightIdx)
        {
            var right = _nodes[rightIdx].Priority;
            if (_compare(right, minPriority) < 0)
            {
                minPriority = right;
                minIdx = rightIdx;
            }
        }

        if (minIdx == idx)
            return;

        (_nodes[idx], _nodes[minIdx]) = (_nodes[minIdx], _nodes[idx]);
        Heapify(minIdx);
    }
}