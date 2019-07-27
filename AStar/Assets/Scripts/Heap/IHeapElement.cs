using System;

public interface IHeapElement<T> : IComparable<T>
{
    int HeapIndex{get; set;}
}
