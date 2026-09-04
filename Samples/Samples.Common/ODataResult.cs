namespace Samples.Common;

public record ODataResult<T>(T[] Items, int TotalCount);
