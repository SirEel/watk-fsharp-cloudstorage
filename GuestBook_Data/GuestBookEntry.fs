namespace GuestBook_Data

open System

type GuestBookEntry() = 
    inherit Microsoft.WindowsAzure.StorageClient.TableServiceEntity   
        (
            // partition key
            DateTime.UtcNow.ToString("MMddyyyy")         
            // row key
            , String.Format("{0:10}_{1}"
                , DateTime.MaxValue.Ticks - DateTime.UtcNow.Ticks
                , Guid.NewGuid() )
        )

        member val Message = "" with get, set
        member val GuestName = "" with get, set
        member val PhotoUrl = "" with get, set
        member val ThumbnailUrl = "" with get, set


