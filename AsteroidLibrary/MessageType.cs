using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AsteroidLibrary
{
    public enum MessageType
    {
        EndOfMessage = 0,
        Movement = 1,
        AddedClient = 2,
        PlayerReadyStatus = 3
    }
}
