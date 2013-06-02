namespace GuestBook_WebApp

module GuestBook_Controller =

    open System
    open System.Diagnostics
    open System.IO

    open GuestBook_Data
    open GuestBook_Storage

    let initializeStorage() =
        StorageSource.initializeStorage()

    let createGuestBookEntry(guestName, message, fileName, fileContentType, fileStream) =
    
        let  uniqueBlobName =
            String.Format
                (
                      "guestbookpics/image{0}{1}" 
                    , Guid.NewGuid().ToString()
                    , Path.GetExtension(fileName)
                )
        let blob = StorageSource.getBlockBlobReference(uniqueBlobName)
        blob.Properties.ContentType <- fileContentType
        blob.UploadFromStream(fileStream)

        let entry = 
            new GuestBookEntry
                (
                      GuestName = guestName
                    , Message = message
                    , PhotoUrl = blob.Uri.ToString()
                    , ThumbnailUrl = blob.Uri.ToString()
                )

        let ds = new GuestBookDataSource()
        ds.AddGuestBookEntry(entry)
            |> ignore

        Trace.TraceInformation
            ("Added entry {0}-{1} in table storage for guest {2}"
            , entry.PartitionKey, entry.RowKey, entry.GuestName)

        let message = 
            String.Format
                (
                      "{0},{1},{2}"
                    , blob.Uri
                    , entry.PartitionKey
                    , entry.RowKey)

        StorageSource.enqueueMessage(message)

        Trace.TraceInformation
            ("Queued message to process blob '{0}'"
            , uniqueBlobName)