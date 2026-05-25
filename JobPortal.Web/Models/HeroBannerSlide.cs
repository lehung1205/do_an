namespace JobPortal.Web.Models;



/// <summary>

/// Ảnh banner carousel trang chủ — đặt file vào wwwroot/images/hero/

/// </summary>

public sealed class HeroBannerSlide

{

    /// <summary>Đường dẫn từ wwwroot, ví dụ /images/hero/banner.png</summary>

    public string ImagePath { get; init; } = null!;



    public static IReadOnlyList<HeroBannerSlide> DefaultSlides { get; } = new[]

    {

        new HeroBannerSlide { ImagePath = "/images/hero/banner.png" },

        new HeroBannerSlide { ImagePath = "/images/hero/banner2.png" },

        new HeroBannerSlide { ImagePath = "/images/hero/banner3.png" },

        new HeroBannerSlide { ImagePath = "/images/hero/banner4.png" },

    };

}


