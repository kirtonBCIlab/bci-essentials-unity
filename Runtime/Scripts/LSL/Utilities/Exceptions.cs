using System;

namespace BCIEssentials.LSLFramework
{
    public class DuplicateOutletException : Exception
    {
        public DuplicateOutletException()
        : base(
            "Another Stream Writer with the same name and type is "
            + "already live, opening this one would duplicate streams"
        )
        { }
    }

    public class EpochLengthException : Exception
    {
        public EpochLengthException(float previousEpochLength)
        : base(
            "Epoch length doesn't match the previous value: "
            + $"{previousEpochLength}, this will prevent "
            + "the back end from making proper predictions"
        )
        { }
    }
}