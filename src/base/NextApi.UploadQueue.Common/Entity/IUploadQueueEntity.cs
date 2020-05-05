using System;
using NextApi.Common.Entity;

namespace NextApi.UploadQueue.Common.Entity
{
    /// <summary>
    /// Base interface for Entity that supported in the "UploadQueue" mechanism
    /// </summary>
    public interface IUploadQueueEntity : IEntity<Guid>
    {
    }
}
