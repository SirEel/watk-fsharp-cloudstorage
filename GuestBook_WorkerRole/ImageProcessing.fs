namespace GuestBook_WorkerRole

module ImageProcessing =

    open System.Drawing
    open System.Drawing.Drawing2D
    open System.Drawing.Imaging
    open System.IO

    let processImage (input:Stream) (output:Stream) =
        let originalImage = new Bitmap(input)
        let (width, height) =
            if originalImage.Width > originalImage.Height
                then (128, int(float(128 * originalImage.Height) / float(originalImage.Width)) )
                else (int(float(128 *  originalImage.Width) / float(originalImage.Height)), 128 )
                
        let mutable (thumbnailImage:Bitmap) = null
    
        try
            thumbnailImage <- new Bitmap(width, height)
            use graphics = Graphics.FromImage(thumbnailImage)
            graphics.InterpolationMode <- InterpolationMode.HighQualityBicubic
            graphics.SmoothingMode <- SmoothingMode.AntiAlias
            graphics.PixelOffsetMode <- PixelOffsetMode.HighQuality
            graphics.DrawImage(originalImage, 0, 0, width, height)
            thumbnailImage.Save(output, ImageFormat.Jpeg)
        finally
            if thumbnailImage <> null
                then thumbnailImage.Dispose()

