namespace GerberParser.Abstracts.EARCUT;

public class ObjectPool<T> where T : new()
{
    private T[] _currentBlock;
    private int _currentIndex;
    private int _blockSize;
    private readonly List<T[]> _allocations = new List<T[]>();

    public ObjectPool()
    {
    }

    public ObjectPool(int blockSize)
    {
        Reset(blockSize);
    }

    public T Construct()
    {
        if (_currentBlock == null || _currentIndex >= _blockSize)
        {
            _currentBlock = new T[_blockSize];
            _allocations.Add(_currentBlock);
            _currentIndex = 0;
        }

        T obj = _currentBlock[_currentIndex++];
        return obj;
    }

    public void Reset(int newBlockSize)
    {
        _allocations.Clear();
        _blockSize = Math.Max(1, newBlockSize);
        _currentBlock = null;
        _currentIndex = _blockSize;
    }

    public void Clear()
    {
        Reset(_blockSize);
    }
}
