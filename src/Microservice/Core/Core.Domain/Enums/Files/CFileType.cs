namespace Core.Domain
{
    public enum CFileType
    {

    }


    public enum CImageType
    {
        None = 0,
        Avatar = 1,      // Ảnh đại diện
        CoverPhoto = 2,  // Ảnh bìa
        Gallery = 3,     // Ảnh trong thư viện
        Thumbnail = 4,   // Ảnh thu nhỏ
        Banner = 5,       // Ảnh banner
    }

    public enum CFileExtensionType
    {
        None = 0,
        #region image extension
        JPEG = 1,  // Định dạng ảnh phổ biến nhất, thường dùng cho ảnh chụp và ảnh trên web.
        PNG = 2,   // Định dạng ảnh hỗ trợ nền trong suốt, thường dùng cho đồ họa web.
        GIF = 3,   // Định dạng ảnh động, thường dùng cho ảnh động ngắn.
        BMP = 4,   // Định dạng ảnh bitmap, ít được sử dụng do kích thước tệp lớn.
        TIFF = 5,  // Định dạng ảnh chất lượng cao, thường dùng trong in ấn.
        SVG = 6,   // Định dạng ảnh vector phổ biến trên web, có thể phóng to mà không mất chất lượng.
        EPS = 7,   // Định dạng ảnh vector thường dùng trong in ấn và thiết kế đồ họa.
        AI = 8,    // Định dạng ảnh vector của Adobe Illustrator.
        WebP = 9,  // Định dạng ảnh mới của Google, hỗ trợ nén tốt hơn JPEG và PNG.
        HEIF = 10  // Định dạng ảnh mới, thường dùng trên các thiết bị của Apple.
        #endregion image extension
    }


    public enum CBlobType
    {
        None = 0,
        AzureBlob = 1,
        ImageKit = 2,
        Local = 3
    }
}