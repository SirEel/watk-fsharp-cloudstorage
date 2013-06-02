namespace GuestBook_WorkerRole

open System
open System.Diagnostics
open System.Net
open System.Threading
open Microsoft.WindowsAzure
open Microsoft.WindowsAzure.ServiceRuntime
open Microsoft.WindowsAzure.StorageClient

open System.Text.RegularExpressions

open GuestBook_Data
open GuestBook_Storage

type WorkerRole() =
    inherit RoleEntryPoint() 

    override wr.OnStart() = 

        ServicePointManager.DefaultConnectionLimit <- 12

        CloudStorageAccount.SetConfigurationSettingPublisher
            (
                Action<_,_>(fun (configName:string) (configSetter:Func<_,_>) ->
                    configName
                    |> RoleEnvironment.GetConfigurationSettingValue
                    |> configSetter.Invoke 
                    |> ignore
                )
            )

        let rec initializeStorage() =
            try
                StorageSource.initializeStorage()
            with 
            | :? StorageClientException as e 
                    when e.ErrorCode = StorageErrorCode.TransportError
                ->  Trace.TraceError(
                        "Storage services initialization failure. "
                        + "Check your storage account configuration settings. If running locally, "
                        + "ensure that the Development Storage service is running. Message: '{0}'",
                        e.Message)
                    Thread.Sleep 5000
                    initializeStorage()

        initializeStorage()

        base.OnStart()

    override wr.Run() =
        Trace.TraceInformation("Listening for queue messages...")

        // pull messages endlessly
        while true do
            try
                let msg = StorageSource.getQueueMessage()
                if msg = null 
                    then Thread.Sleep 5000
                else
                    let messageParts = msg.AsString.Split(',')                  
                    let imageBlobUri = messageParts.[0]
                    let partitionKey = messageParts.[1]
                    let rowKey = messageParts.[2]

                    Trace.TraceInformation("Processing thumbnail image of blob '{0}'.", imageBlobUri)

                    let thumbnailBlobUri = 
                        Regex.Replace(imageBlobUri, "([^\\.]+)(\\.[^\\.]+)?$", "$1-thumb$2")
                    let inputBlob = StorageSource.getBlobReference(imageBlobUri)
                    let outputBlob = StorageSource.getBlobReference(thumbnailBlobUri)
                    
                    use input = inputBlob.OpenRead()
                    use output = outputBlob.OpenWrite()         
                    ImageProcessing.processImage input output
                    output.Commit()

                    outputBlob.Properties.ContentType <- "image/jpeg"
                    outputBlob.SetProperties()

                    Trace.TraceInformation("Updating thumbnailBlobUri for table entry '{0}-{1}'."
                        , partitionKey, rowKey)

                    let ds = new GuestBookDataSource()
                    ds.UpdateImageThumbnail(partitionKey, rowKey, thumbnailBlobUri)
                    |> ignore
         
                    StorageSource.deleteQueueMessage(msg)

                    Trace.TraceInformation("Generated thumbnail in blob '{0}'.", thumbnailBlobUri)
            with 
            | :? StorageClientException as e
                ->  Trace.TraceError(
                        "Exception when processing queue item. Message: '{0}'",
                        e.Message)
                    Thread.Sleep 5000
    
