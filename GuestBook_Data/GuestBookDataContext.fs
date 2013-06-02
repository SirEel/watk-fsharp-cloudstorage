namespace GuestBook_Data

type GuestBookDataContext
    (
         baseAddress:string
        ,credentials:Microsoft.WindowsAzure.StorageCredentials 
    ) =
    inherit Microsoft.WindowsAzure.StorageClient.TableServiceContext(baseAddress, credentials)
    
    member this.GuestBookEntry =
        this.CreateQuery<GuestBookEntry>("GuestBookEntry")