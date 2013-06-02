namespace GuestBook_Data

open System
open System.Linq
open Microsoft.WindowsAzure
open Microsoft.WindowsAzure.StorageClient

type GuestBookDataSource() =

    // static constructor code
    static let storageAccount = CloudStorageAccount.FromConfigurationSetting("DataConnectionString")
    
    static do 
        CloudTableClient.CreateTablesFromModel
            (
                typeof<GuestBookDataContext>
                ,storageAccount.TableEndpoint.AbsoluteUri
                ,storageAccount.Credentials
            )

    // non-static constructor code
    let context = 
        new GuestBookDataContext
            (
                storageAccount.TableEndpoint.AbsoluteUri
                ,storageAccount.Credentials
            )
    do context.RetryPolicy <- RetryPolicies.Retry(3, TimeSpan.FromSeconds(1.0))

    // public methods
    member public this.GetGuestBookEntries() =
        // Table LINQ doesn't support DateTime.UtcNow.ToString(), so we must call it here and use
        // the resulting string in .Where lambda
        let now = DateTime.UtcNow.ToString("MMddyyyy")
        context.GuestBookEntry
            .Where(fun (g:GuestBookEntry) -> 
                g.PartitionKey = now)

    member public this.AddGuestBookEntry(newEntry:GuestBookEntry) =
        context.AddObject("GuestBookEntry", newEntry)
        context.SaveChanges()

    member public this.UpdateImageThumbnail(partitionKey, rowKey, thumbUrl) =
        let entry = 
            context.GuestBookEntry
                .Where(fun (g:GuestBookEntry) -> g.PartitionKey = partitionKey && g.RowKey = rowKey)
                .FirstOrDefault()
        entry.ThumbnailUrl <- thumbUrl
        context.UpdateObject(entry)
        context.SaveChanges()
    


