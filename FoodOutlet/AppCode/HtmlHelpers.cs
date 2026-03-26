using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FoodOutlet.AppCode
{
    /// <summary>
    /// Photo Frame HTML Helper Methods - Universal Image Framing System
    /// Use in Razor views: @Html.PhotoFrame(path, ratio, width)
    /// </summary>
    public static class PhotoFrameHelper
    {
        /// <summary>
        /// Generic PhotoFrame - Universal for all image types
        /// Usage: @Html.PhotoFrame("imagePath", "1:1", 100, "Alt Text", "circle shadow-md")
        /// Ratios: "1:1", "4:3", "16:9"
        /// </summary>
        public static IHtmlContent PhotoFrame(
            this IHtmlHelper helper,
            string imagePath,
            string ratio = "1:1",
            int width = 100,
            string altText = "Image",
            string cssClass = null)
        {
            if (!IsValidRatio(ratio))
                ratio = "1:1";

            if (string.IsNullOrEmpty(imagePath))
                imagePath = "/img/placeholder.png";

            string classes = $"photo-frame ratio-{ratio.Replace(":", "-")}";
            if (!string.IsNullOrEmpty(cssClass))
                classes += " " + cssClass;

            string aspectRatio = ratio.Replace(":", "/");
            string encodedAlt = System.Web.HttpUtility.HtmlAttributeEncode(altText);

            string html = $@"<div class=""{classes}"" style=""width: {width}px; aspect-ratio: {aspectRatio};"" data-image=""{imagePath}"">
                <img src=""{imagePath}"" alt=""{encodedAlt}"" onerror=""this.onerror=null; this.src='/img/placeholder.png'"" />
            </div>";

            return new HtmlString(html);
        }

        /// <summary>
        /// Staff Photo - 100x100, 1:1 ratio, circular
        /// Usage: @Html.StaffPhotoFrame(staff.photo, staff.name)
        /// </summary>
        public static IHtmlContent StaffPhotoFrame(
            this IHtmlHelper helper,
            string imagePath,
            string staffName = "Staff")
        {
            return PhotoFrame(helper, imagePath, "1:1", 100, staffName, "circle shadow-sm border-primary");
        }

        /// <summary>
        /// Recipe Photo - 80x60, 4:3 ratio
        /// Usage: @Html.RecipePhotoFrame(recipe.recipe_img, recipe.recipe_name)
        /// </summary>
        public static IHtmlContent RecipePhotoFrame(
            this IHtmlHelper helper,
            string imagePath,
            string recipeName = "Recipe")
        {
            return PhotoFrame(helper, imagePath, "4:3", 80, recipeName, "rounded shadow-sm");
        }

        /// <summary>
        /// QR Code Photo - 150x150, 1:1 ratio
        /// Usage: @Html.QRCodeFrame(table.qr_code, "Table 1")
        /// </summary>
        public static IHtmlContent QRCodeFrame(
            this IHtmlHelper helper,
            string imagePath,
            string altText = "QR Code")
        {
            return PhotoFrame(helper, imagePath, "1:1", 150, altText, "shadow-lg border-dark");
        }

        /// <summary>
        /// Category Photo - 120x90, 4:3 ratio
        /// Usage: @Html.CategoryPhotoFrame(category.image, category.name)
        /// </summary>
        public static IHtmlContent CategoryPhotoFrame(
            this IHtmlHelper helper,
            string imagePath,
            string categoryName = "Category")
        {
            return PhotoFrame(helper, imagePath, "4:3", 120, categoryName, "rounded");
        }

        /// <summary>
        /// For JavaScript/DataTables - returns HTML string
        /// Usage: PhotoFrameHelper.PhotoFrameHtml(imagePath, "1:1", 100)
        /// </summary>
        public static string PhotoFrameHtml(
            string imagePath,
            string ratio = "1:1",
            int width = 100,
            string altText = "Image",
            string cssClass = null)
        {
            if (!IsValidRatio(ratio))
                ratio = "1:1";

            if (string.IsNullOrEmpty(imagePath))
                imagePath = "/img/placeholder.png";

            string classes = $"photo-frame ratio-{ratio.Replace(":", "-")}";
            if (!string.IsNullOrEmpty(cssClass))
                classes += " " + cssClass;

            string aspectRatio = ratio.Replace(":", "/");
            string encodedAlt = System.Web.HttpUtility.HtmlAttributeEncode(altText);

            return $@"<div class=""{classes}"" style=""width: {width}px; aspect-ratio: {aspectRatio};"" data-image=""{imagePath}""><img src=""{imagePath}"" alt=""{encodedAlt}"" onerror=""this.onerror=null; this.src='/img/placeholder.png'"" /></div>";
        }

        /// <summary>
        /// Validate aspect ratio
        /// </summary>
        private static bool IsValidRatio(string ratio)
        {
            return ratio == "1:1" || ratio == "4:3" || ratio == "16:9";
        }
    }
}
