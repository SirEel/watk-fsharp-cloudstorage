namespace GuestBook_Storage

module StorageSource =

    open System
    open System.Net
    open Microsoft.WindowsAzure
    open Microsoft.WindowsAzure.StorageClient

    let mutable private blobStorage : CloudBlobClient = null
    let mutable private blobContainer : CloudBlobContainer = null
    let mutable private queue : CloudQueue = null

    let initializeStorage() =

        let storageAccount = 
            CloudStorageAccount.FromConfigurationSetting("DataConnectionString")

        blobStorage <- storageAccount.CreateCloudBlobClient()
        blobContainer <- blobStorage.GetContainerReference("guestbookpics")
        blobContainer.CreateIfNotExist() 
            |> ignore
        let permissions = blobContainer.GetPermissions()
        permissions.PublicAccess <- BlobContainerPublicAccessType.Container
        blobContainer.SetPermissions(permissions)

        queue <-
            storageAccount
                .CreateCloudQueueClient()
                .GetQueueReference("guestthumbs")
        queue.CreateIfNotExist() 
            |> ignore

    let getBlockBlobReference( uniqueBlobName ) =
        blobStorage.GetBlockBlobReference(uniqueBlobName)

    let enqueueMessage( message:string ) =
        let queueMessage = new CloudQueueMessage(message)
        queue.AddMessage(queueMessage)

    let getBlobReference( blobUri ) =
        blobContainer.GetBlobReference(blobUri)

    let getQueueMessage() =
        queue.GetMessage()

    let deleteQueueMessage(msg) =
        queue.DeleteMessage(msg)